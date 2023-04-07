using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Media.Playback;
using Windows.UI.Core.AnimationMetrics;
using Windows.UI.Popups;
using Windows.ApplicationModel.UserDataTasks;
using System.ComponentModel.DataAnnotations;
using Windows.ApplicationModel.AppExtensions;

namespace SqlCollegeTranscripts
{
    // The basic design of the program:
    // sqlCurrent stores the table and fields, the Where clauses, and one OrderBy clause.
    // sqlCurrent.returnSql returns the sql string.  This is then bound to the Grid (via an sqlDataAdaptor)

    //User actions, and how the program reacts to each as follows
    //0.  Form Load - Loads user settings and other things which don't depend on the table
    //1.  Open New connection -- called near end of Form Load and by File-->connection menu
    //    First calls closeConnection (reinitiae everything to empty state; list tables in menu), 
    //    and then open the new connection
    //2.  Open New Table -- calls writeGrid_NewTable
    //    WriteGrid_NewTable - Sets new sqlCurrent - Calls sqlCurrent.SetInnerJoins which sets sql table strings and field strings.
    //    WriteGrid_NewTable also resets the top filters and then sets them up for the table (with no actual filtering)
    //    WriteGrid_NewTable calls Write New Filters calls write New Orderby calls write Grid
    //    Write the New Filters sets the where clauses in sqlCurrent
    //    Write New OrderBy simply adds an order by clause -- this can be changed via click header of grid event.
    //    Write the Grid - binds dataViewGrid1 and then does some formatting on datagridview.
    //3.  Five modes
    //    View -- Base
    //    Edit  -- User selects 1 column in table to edit - not table PK or DK but may be FK - and FK may have several DK columns. 
    //          -- Selecting a column also sets currentDA.UpdateCommand (i.e. currentSql's dataadapter's UpdateCommand)
    //          -- User clicks on an edit column - textbox appears for non-keys, drop-down appears for FK.
    //          -- When user exits the cell, call currentDA.Update()

    internal partial class DataGridViewForm : Form
    {
        #region Variables
        private FormOptions? formOptions;
        private ConnectionOptions? connectionOptions;
        private TableOptions? tableOptions;
        internal ProgramMode programMode = ProgramMode.none;
        internal sqlFactory? currentSql = null;  //builds the sql string via .myInnerJoins, .myFields, .myWheres, my.OrderBys
        internal sqlFactory? comboSql = null;  //Always used "just in time" to fill combo, then destroyed
        internal BindingList<where>? MainFilterList;  // Use this to fill in past main filters in combo box

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Entire Form constructor, events, and Log file

        internal DataGridViewForm()
        {
            //Required by windows forms
            InitializeComponent();
        }

        private void DataGridViewForm_Load(object sender, EventArgs e)
        {
            formOptions = new FormOptions();  // Sets initial option values
            // Set things that don't change even if connection changes
            // 0.  Main filter datasource and 'last' element never changes
            field fi = new field("none", "none", DbType.Int32, 4, true);
            where wh = new where(fi, "0");
            wh.DisplayMember = "No Research object";
            MainFilterList = new BindingList<where>();
            MainFilterList.Add(wh);
            cmbMainFilter.DisplayMember = "DisplayMember";
            cmbMainFilter.ValueMember = "ValueMemeber";
            cmbMainFilter.DataSource = MainFilterList;
            lblMainFilter.Text = "Research:";
            cmbMainFilter.Enabled = false;  // Enabled by EnableMainFilter() when more than one element

            // 1. Set language
            // 2. Set pageSize
            formOptions.pageSize = 200;
            string strPageSize = AppData.GetKeyValue("RPP");  // If not set, this will return string.Empty
            int newPageSize = 0;
            if (int.TryParse(strPageSize, out newPageSize))
            {
                if (newPageSize > 9)  // Don't allow less than 10
                {
                    formOptions.pageSize = newPageSize;
                }
            }
            txtRecordsPerPage.Text = formOptions.pageSize.ToString();

            // 3. Menu options from last save
            // 4. Set font size
            DesignControlsOnFormLoad();

            // 5. Load Database List (in files menu)
            load_mnuDatabaseList();

            // 6. Open Log file
            // openLogFile(); //errors ignored

            // 7. Open last connection 
            string msg = OpenConnection();
            if (msg != string.Empty) { msgText(msg); txtMessages.ForeColor = Color.Red; }

            // 8. Build English database - will do nothing if Boolean BuildingUpEnglishDatabase in MultiLingual.cs set to false
            MultiLingual.InsertEnglishIntoDatabase(this);
            programMode = ProgramMode.none;
            SetAllFiltersOnModeChange(); // Will disable filters and call SetTableLayoutPanelHeight();
        }

        private void DataGridViewForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                //Cleanup
                MsSql.CloseConnectionAndDataAdapters();
                closeLogFile();
                // ts = null;
                // fso = null;
            }
            catch { }
        }

        private void DesignControlsOnFormLoad()
        {
            // Note: these things that never change           
            // Translations
            lblGridFilter.Text = MultiLingual.tr("Filter Grid:", this);
            lblComboFilter.Text = MultiLingual.tr("Filter Dropdowns:", this);

            // Control arrays - can't make array in design mode in .net
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5 };
            Label[] lblCmbFilterFields = { lblCmbFilterField_0, lblCmbFilterField_1, lblCmbFilterField_2, lblCmbFilterField_3, lblCmbFilterField_4, lblCmbFilterField_5 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };
            RadioButton[] radioButtons = { rbView, rbEdit, rbDelete, rbAdd, rbMerge };

            //Get size from registry and define "font"
            int size = 8;  // default
            try { size = Convert.ToInt32(Interaction.GetSetting("AccessFreeData", "Options", "FontSize", "9")); } catch { }
            System.Drawing.Font font = new System.Drawing.Font("Arial", size, FontStyle.Regular);

            // Set font of all single controls - 
            dataGridView1.Font = font;
            lblMainFilter.Font = font;
            lblGridFilter.Font = font;
            lblComboFilter.Font = font;
            cmbMainFilter.Font = font;
            cmbComboTableList.Font = font;
            txtRecordsPerPage.Font = font;

            // Set font of arrays 
            for (int i = 0; i <= cmbGridFilterFields.Count() - 1; i++)
            {
                cmbGridFilterFields[i].Font = font;
                cmbGridFilterFields[i].DropDownStyle = ComboBoxStyle.DropDownList;
                cmbGridFilterFields[i].FlatStyle = FlatStyle.Flat;
                cmbGridFilterFields[i].AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbGridFilterFields[i].AutoCompleteMode = AutoCompleteMode.None;
                cmbGridFilterValue[i].Font = font;
                cmbGridFilterValue[i].DropDownStyle = ComboBoxStyle.DropDownList;
                cmbGridFilterValue[i].FlatStyle = FlatStyle.Flat;
                cmbGridFilterValue[i].AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbGridFilterValue[i].AutoCompleteMode = AutoCompleteMode.Suggest;

            }
            for (int i = 0; i <= cmbComboFilterValue.Count() - 1; i++)
            {
                lblCmbFilterFields[i].Font = font;
                cmbComboFilterValue[i].Font = font;
                cmbGridFilterFields[i].DropDownStyle = ComboBoxStyle.DropDown;
                cmbGridFilterFields[i].FlatStyle = FlatStyle.Flat;
                cmbComboFilterValue[i].AutoCompleteSource = AutoCompleteSource.CustomSource;
                cmbComboFilterValue[i].AutoCompleteMode = AutoCompleteMode.Suggest;
            }
            for (int i = 0; i <= radioButtons.Count() - 1; i++)
            {
                radioButtons[i].Font = font;
            }
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = formOptions.DefaultColumnColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = formOptions.DefaultColumnColor;
        }

        private void openLogFile()
        {
            StreamWriter tsWriter = null;
            string fileCurrentName = "";
            System.DateTime fileCurrentDate = DateTime.FromOADate(0), fileDate = DateTime.FromOADate(0);
            FileInfo fil = null;
            try
            {
                string dbPath = Interaction.GetSetting("AccessFreeData", "DatabaseList", "path0", "");
                string logDir = dbPath.Substring(0, Math.Min(dbPath.LastIndexOf("\\") + 1, dbPath.Length)) + "log";
                //Create folder
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                for (int i = 1; i <= 50; i++)
                {
                    //if file i does not exist, create and use it
                    fileCurrentName = logDir + "\\transcriptLog" + fileNumber(i) + ".xml";
                    if (!File.Exists(fileCurrentName))
                    {
                        goto label20;
                    }
                    //Compare date of fileCurrentName to previous, delete and use this name if less than previous
                    fil = new FileInfo(fileCurrentName);
                    if (fil.LastWriteTime < fileCurrentDate)
                    {
                        File.Delete(fileCurrentName);
                        fil = null;
                        goto label20;
                    }
                    //Update fileCurrentDate
                    fileCurrentDate = fil.LastWriteTime;
                    fil = null;
                }
                //No files found -- use file 1, 2nd loop, delete & recreate to change create date
                fileCurrentName = logDir + "\\transcriptLog" + fileNumber(1) + ".xml";
                File.Delete(fileCurrentName);

            label20://UPGRADE_WARNING: (2081) CreateTextFile has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2081
                FileStream tempAuxVar = new FileStream(fileCurrentName, FileMode.Create);
                formOptions.logFileName = fileCurrentName;
                FileStream tempAuxVar2 = new FileStream(formOptions.logFileName, FileMode.Create);
                formOptions.ts = new FileStream(formOptions.logFileName, FileMode.OpenOrCreate, FileAccess.Write);
                tsWriter = new StreamWriter(formOptions.ts);
                tsWriter.WriteLine("<?xml version = \"1.0\" encoding=\"Big5\"?>");
                tsWriter.WriteLine("<sessions>"); //manually add </sessions> if veiwing in xml
                tsWriter.WriteLine("<d" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateAndTime.Day(DateTime.Now).ToString() + ">");
                tsWriter.WriteLine("<dataBase>" + "\"" + dbPath + "\"" + "</dataBase>");
            }
            catch (Exception exc)
            {
                // NotUpgradedHelper.NotifyNotUpgradedElement("Resume in On-Error-Resume-Next Block");
            }

        }

        private void closeLogFile()
        {
            StreamWriter tsWriter = null;
            //UPGRADE_TODO: (1069) Error handling statement (On Error Resume Next) was converted to a pattern that might have a different behavior. More Information: https://docs.mobilize.net/vbuc/ewis#1069
            try
            {
                tsWriter = new StreamWriter(formOptions.ts);
                tsWriter.WriteLine("</d" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateAndTime.Day(DateTime.Now).ToString() + ">");
                tsWriter.WriteLine("</sessions>");
                tsWriter.Close();
                formOptions.ts = null;
            }
            catch (Exception exc)
            {
                // NotUpgradedHelper.NotifyNotUpgradedElement("Resume in On-Error-Resume-Next Block");
            }
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Opening and closing Connection 
        private string OpenConnection()
        {
            programMode = ProgramMode.none;  // Affects SetAllFilters and sets PanelHeight
            SetAllFiltersOnModeChange();
            connectionOptions = new ConnectionOptions();  // resets options
            StringBuilder sb = new StringBuilder();
            try
            {
                // 1. Close old connection if any
                CloseConnection();

                // 2. Get connection string
                connectionOptions.msSql = true;

                connectionString csObject = AppData.GetFirstConnectionStringOrNull();
                if (csObject == null)
                {
                    sb.AppendLine(MultiLingual.tr("No previous connection string.", this));
                }
                else
                {
                    // {0} is server, {1} is Database, {2} is user, {3} is password (unknown)
                    string cs = String.Format(csObject.comboString, csObject.server, csObject.databaseName, csObject.user);

                    // Get password from user
                    if (cs.IndexOf("{3}") >= 0)
                    {
                        frmLogin passwordForm = new frmLogin();
                        passwordForm.ShowDialog();
                        string password = passwordForm.password;
                        passwordForm = null;
                        cs = cs.Replace("{3}", password);
                    }

                    //Read only database variable
                    connectionOptions.readOnly = csObject.readOnly;

                    // 5. Open connection
                    MsSql.openConnection(cs);

                    // 6. Initialize datatables
                    dataHelper.initializeDataTables();

                    // 7. Fill Information Datatables
                    MsSql.initializeDatabaseInformationTables();

                    // 8.  Load Table mainmenu
                    foreach (DataRow row in dataHelper.tablesDT.Rows)
                    {
                        ToolStripItem tsi = new ToolStripMenuItem();
                        string tn = row["TableName"].ToString();
                        tsi.Name = tn;
                        tsi.Text = tn;
                        if (tn != null)
                        {
                            mnuOpenTables.DropDownItems.Add(tsi);
                        }
                    }

                    msgText("Recommend: First select object to research.  Example: choose student from student table (or whatever), right click, select 'Find / research object in higher tables'.");
                    msgColor(Color.Navy);
                }
            }
            catch (System.Exception excep)
            {
                sb.AppendLine("Error opening connection.");
                MessageBox.Show("Error opening the connection. " + Environment.NewLine + excep.Message + Environment.NewLine + Information.Err().Number.ToString());
                CloseConnection();
            }
            return sb.ToString();
        }

        private void CloseConnection()
        {
            MsSql.CloseConnectionAndDataAdapters();
            dataHelper.clearDataTables();

            // Clear other old values
            currentSql = null;
            comboSql = null;

            // Delete the old mnuOpentable members
            if (mnuOpenTables.DropDownItems != null)
            {
                mnuOpenTables.DropDownItems.Clear();
            }
            // Hide special menus
            mnuTranscript.Available = false;
            mnuAddressBook.Available = false;
        }


        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Write to Gird

        private void writeGrid_NewTable(string table)
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5 };

            Stopwatch watch = new Stopwatch();
            if (formOptions.runTimer) Stopwatch.StartNew();

            // 0. New tableOptions and Clear Grid-Combo filters
            tableOptions = new TableOptions(); // Resets options

            // This does not fire any events because index will -1 (and text-change tag will be null)
            ClearFiltersOnNewTable();

            //1. Create currentSql - same currentSql used until new table is loaded
            currentSql = new sqlFactory(table, 1, formOptions.pageSize);
            // Above may produce error message
            if (!String.IsNullOrEmpty(currentSql.errorMsg))
            {
                msgColor(Color.Red);
                msgText(currentSql.errorMsg);
            }
            else
            {
                msgColor(Color.Navy);
                msgText(currentSql.myTable);
            }

            // 2. Bind some of 6 cmbGridFilterFields with fields for user to select
            //   Only setting up cmbGridFilterFields - Values set on cmbGridFilterField SelectionChanged event
            int comboNumber = 0;
            //2a.  Bind first grid filter to all non DK, FK fields  (Skip when adding a row)
            BindingList<field> filterFields = new BindingList<field>();
            //First Fill filterFields
            foreach (field fl in currentSql.myFields)
            {
                if (fl.table == currentSql.myTable && (dataHelper.isForeignKeyField(fl) || (dataHelper.isDisplayKey(fl))))
                {
                    // Handled below as single choices
                }
                else
                {
                    // Change foreign keys to primary key of ref table
                    if (dataHelper.isForeignKeyField(fl))
                    {
                        field fl2 = dataHelper.getForeignKeyRefField(fl);
                        fl2.DisplayMember = MultiLingual.tr(fl2.DisplayMember, this);
                        filterFields.Add(fl2);
                    }
                    else
                    {
                        fl.DisplayMember = MultiLingual.tr(fl.DisplayMember, this);
                        filterFields.Add(fl);
                    }
                }
            }
            // Then Bind combo to filterField bindingList
            cmbGridFilterFields[comboNumber].BindingContext = new BindingContext();  // Required to change 1 by 1.
            cmbGridFilterFields[comboNumber].DisplayMember = "DisplayMember";
            cmbGridFilterFields[comboNumber].ValueMember = "ValueMember";  //Entire field
            // Fires change_selectedindex which binds cmbGridFilterValues 
            // which calls cmbGridFilterValue_textchanged. Which writes grid with new value
            // "Updating" cancels this write grid
            formOptions.updating = true;
            cmbGridFilterFields[comboNumber].DataSource = filterFields;
            formOptions.updating = false;
            cmbGridFilterFields[comboNumber].BackColor = ComboBox.DefaultBackColor;
            cmbGridFilterValue[comboNumber].BackColor = ComboBox.DefaultBackColor;
            cmbGridFilterValue[comboNumber].FlatStyle = FlatStyle.Flat;
            comboNumber++;

            // 2b.   Bind one cmbGridFilterFields for each DK and FK
            foreach (field fl in currentSql.myFields)
            {
                if (fl.table == currentSql.myTable && (dataHelper.isForeignKeyField(fl) || (dataHelper.isDisplayKey(fl))))
                {
                    if (comboNumber < cmbGridFilterFields.Count())
                    {
                        BindingList<field> dkField = new BindingList<field>();
                        // Change foreign keys to primary key of ref table
                        if (dataHelper.isForeignKeyField(fl))
                        {
                            field fl2 = dataHelper.getForeignKeyRefField(fl);
                            dkField.Add(fl2);
                        }
                        else
                        {
                            dkField.Add(fl);
                        }
                        cmbGridFilterFields[comboNumber].DisplayMember = "DisplayMember";
                        cmbGridFilterFields[comboNumber].ValueMember = "ValueMember";  //Entire field
                        formOptions.updating = true; // following calls cmbGridFilterValue_textchanged. "Updating" cancels that event.
                        cmbGridFilterFields[comboNumber].DataSource = dkField;
                        formOptions.updating = false;
                        comboNumber++;
                    }
                }
            }

            //4. Bind the cmbComboTableList with Primary keys of Reference Tables
            BindingList<field> comboTableList = new BindingList<field>();
            string filter = String.Format("TableName = '{0}' and (is_FK = 'true' or is_PK = 'true')", currentSql.myTable);
            DataRow[] drs = dataHelper.fieldsDT.Select(filter);
            foreach (DataRow dr in drs)
            {
                field fi = dataHelper.getFieldFromFieldsDT(dr);
                if (dataHelper.isForeignKeyField(fi))
                {
                    fi = dataHelper.getForeignKeyRefField(dr);  // Primary key of ref table
                    tableOptions.tableHasForeignKeys = true;
                }
                comboTableList.Add(fi);  // Only add primary keys
            }
            if (comboTableList.Count > 0)
            {
                cmbComboTableList.BindingContext = new BindingContext();  // Required to change 1 by 1.
                cmbComboTableList.DisplayMember = "DisplayMember";
                cmbComboTableList.ValueMember = "ValueMember";  //Entire field
                formOptions.updating = true;   // Following can't yet change colors because uses grid colors and grid not loaded
                cmbComboTableList.DataSource = comboTableList;  // Will clear combofilters
                formOptions.updating = false;
            }

            //5. Set programMode to ProgramMode.view
            rbView.Checked = true;


            //6.  Enable or disable menu items
            GridContextMenu_FindInAncestor.Enabled = tableOptions.tableHasForeignKeys;
            DataRow[] drs2 = dataHelper.fieldsDT.Select(String.Format("RefTable = '{0}'", currentSql.myTable));
            GridContextMenu_FindInDescendent.Enabled = (drs2.Count() > 0);

            //7. WriteGrid - next step
            writeGrid_NewFilter();

            if (formOptions.runTimer) { watch.Stop(); msgTextAdd(" " + watch.ElapsedMilliseconds.ToString() + " "); }
        }

        internal void writeGrid_NewFilter()
        {
            // 1. Create the where clauses in sqlBuilder currentSql
            callSqlWheres();
            // 2. Get record Count
            string strSql = currentSql.returnSql(command.count);
            currentSql.RecordCount = MsSql.GetRecordCount(strSql);
            // 3. Fetch must have an order by clause - so I may add one on first column
            if (currentSql.myOrderBys.Count == 0)  //Should always be true at this point
            {
                orderBy ob = new orderBy(currentSql.myFields[0], System.Windows.Forms.SortOrder.Ascending);
                currentSql.myOrderBys.Add(ob);
            }

            writeGrid_NewPage();
        }

        internal void writeGrid_NewPage()
        {

            // 1. Get the Sql command for grid
            // CENTRAL and Only USE OF sqlCurrent.returnSql IN PROGRAM
            string strSql = currentSql.returnSql(command.select);
            // Can be deleted later - used to fix non-unique values I want to use as display values 
            if (tableOptions.fixingDatabase)
            {
                strSql = currentSql.returnFixDatabaseSql(tableOptions.strFixDatabaseWhereCondition);
            }

            //2. Clear the grid 
            if (dataGridView1.DataSource != null)
            {
                dataGridView1.DataSource = null;  // Deleting this results in columns not always in right order
            }
            dataGridView1.Columns.Clear();  // Deleting this results in FK fields not being colored
            dataGridView1.AutoGenerateColumns = true;

            // 3. Fill currentDT and Bind the Grid to it.
            dataHelper.currentDT = new DataTable();
            MsSql.FillDataTable(dataHelper.currentDT, strSql);
            dataGridView1.DataSource = dataHelper.currentDT;

            // 4. Replace foreign key colums with FkComboColumn
            dataGridView1.AutoGenerateColumns = false;
            int intCols = dataGridView1.ColumnCount;
            for (int i = 0; i < intCols; i++)
            {
                if (dataHelper.isForeignKeyField(currentSql.myFields[i]))
                {
                    DataGridViewColumn dgvCol = dataGridView1.Columns[i];
                    string dpn = dgvCol.DataPropertyName;
                    int index = dgvCol.Index;
                    dataGridView1.Columns.Remove(dgvCol);
                    FkComboColumn col = new FkComboColumn();
                    col.ValueType = typeof(Int32);
                    col.DataPropertyName = dpn;
                    col.Name = dpn;
                    col.HeaderText = dpn;
                    dataGridView1.Columns.Insert(index, col);
                }
            }

            //5. Format controls
            // a. Set toolStripButton3 caption
            toolStripButton3.Text = currentSql.myPage.ToString() + "/" + currentSql.TotalPages.ToString();
            // b. Set form caption
            string dbPath = Interaction.GetSetting("AccessFreeData", "DatabaseList", "path0", "");
            StringBuilder sb = new StringBuilder(dbPath.Substring(dbPath.LastIndexOf("\\") + 1));
            sb.Append(" - " + currentSql.myTable);
            sb.Append(" -  " + currentSql.RecordCount.ToString() + " rows");
            sb.Append(", Page: " + currentSql.myPage.ToString());
            this.Text = sb.ToString();
            // c. Set column widths
            dataGridView1.RowHeadersWidth = 27; //default
            for (int i = 0; i <= dataGridView1.ColumnCount - 1; i++)
            {
                string fldAlias = dataGridView1.Columns[i].Name;
                string fld = currentSql.myFields[i].fieldName;
                string baseTable = currentSql.myFields[i].table; // Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());

                //Change to default width if set in afdFieldData
                int savedWidth = dataHelper.getIntValueFieldsDT(baseTable, fld, "width");
                if (savedWidth > (47 * 15))
                {
                    dataGridView1.Columns[i].Width = savedWidth / 15;
                }
            }
            // d. Show correct orderby glyph
            if (currentSql.myOrderBys.Count > 0)
            {
                field fldob = currentSql.myOrderBys[0].fld;
                int gridColumn = currentSql.myFields.FindIndex(x => x == fldob);  // gridColumn index = myFields index
                System.Windows.Forms.SortOrder sortOrder = currentSql.myOrderBys[0].sortOrder;
                dataGridView1.Columns[gridColumn].SortMode = DataGridViewColumnSortMode.Programmatic;
                dataGridView1.Columns[gridColumn].HeaderCell.SortGlyphDirection = sortOrder;
            }

            // e. Format the rest of the grid and form
            SetHeaderColorsOnWritePage();
            SetColumnsReadOnlyProperty(); // Might change editable column
            SetTableLayoutPanelHeight();
            SetAllFiltersOnModeChange();   // Because Write_NewPage may require changes
            ColorComboBoxes();   // Must be after the above for some reason
        }


        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Setting up Filters, Colors, TablePanel

        private void SetTableLayoutPanelHeight()
        {
            int height = 2;
            if (programMode != ProgramMode.none) //2nd Row
            {
                height = cmbGridFilterFields_0.Top + cmbGridFilterFields_0.Height + 10;
            }
            if (cmbGridFilterFields_0.Enabled)  // 3rd row
            {
                height = cmbGridFilterFields_0.Top + cmbGridFilterFields_0.Height + 10;
            }
            if (cmbGridFilterFields_3.Enabled)  // 4th row
            {
                height = cmbGridFilterFields_3.Top + cmbGridFilterFields_3.Height + 10;
            }
            if (cmbComboTableList.Enabled)  // 5th row
            {
                height = cmbComboTableList.Top + cmbComboTableList.Height + 10;
            }
            if (cmbComboFilterValue_3.Enabled)  // 6th row
            {
                height = cmbComboFilterValue_3.Top + cmbComboFilterValue_3.Height + 10;
            }

            tableLayoutPanel.Height = height;
            // Reposition the splitter
            if (splitContainer1.Width > 0)  // trying to catch error: "Splitterdistance must be between panel1minsize and width and pan2minsize.
            {
                this.splitContainer1.SplitterDistance = txtMessages.Height + tableLayoutPanel.Height;
            }
        }

        private void SetColumnsReadOnlyProperty()
        {
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.ReadOnly = true;  // default    
                if (programMode == ProgramMode.edit)  // 0 is the "Select column" choice
                {
                    int columnIndex = dataGridView1.Columns.IndexOf(col);
                    field colField = currentSql.myFields[columnIndex];
                    if (colField.table == currentSql.myTable &&
                        !dataHelper.isTablePrimaryKeyField(colField) &&
                        !dataHelper.isDisplayKey(colField))
                    {
                        col.ReadOnly = false;
                    }
                }
            }
        }

        private void ColorComboBoxes()
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5 };
            Label[] lblCmbFilterFields = { lblCmbFilterField_0, lblCmbFilterField_1, lblCmbFilterField_2, lblCmbFilterField_3, lblCmbFilterField_4, lblCmbFilterField_5 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };

            for (int i = 0; i < cmbGridFilterFields.Length; i++)
            {
                // Color all 12 "Filter Grid" combos
                if (cmbGridFilterFields[i].Enabled == true)
                {
                    field selectedField = (field)cmbGridFilterFields[i].SelectedValue;
                    for (int j = 0; j < currentSql.myFields.Count; j++)
                    {
                        field colField = currentSql.myFields[j];
                        // Switch FK to Primary KEy of Ref table - this is what I added to the  Filter fields
                        if (dataHelper.isForeignKeyField(colField))
                        {
                            colField = dataHelper.getForeignKeyRefField(colField);
                        }
                        if (selectedField.isSameFieldAs(colField))
                        {
                            if (dataGridView1.Columns.Count > 0)
                            {
                                cmbGridFilterValue[i].BackColor = dataGridView1.Columns[j].HeaderCell.Style.BackColor;
                                cmbGridFilterFields[i].BackColor = dataGridView1.Columns[j].HeaderCell.Style.BackColor;
                            }
                        }
                    }
                }
            }
            // Color the combo filter labels and combos
            for (int i = 0; i < lblCmbFilterFields.Length; i++)
            {
                // Color combobox the same as corresponding header
                if (lblCmbFilterFields[i].Enabled == true)
                {
                    field selectedField = (field)cmbComboFilterValue[i].Tag; // Set in indexChange event   
                    for (int j = 0; j < currentSql.myFields.Count; j++)
                    {
                        field colField = currentSql.myFields[j];
                        if (selectedField.isSameFieldAs(colField))
                        {
                            if (dataGridView1.Columns.Count > 0)
                            {
                                lblCmbFilterFields[i].BackColor = dataGridView1.Columns[j].HeaderCell.Style.BackColor;
                                cmbComboFilterValue[i].BackColor = dataGridView1.Columns[j].HeaderCell.Style.BackColor;
                            }
                        }
                    }
                }
            }

        }

        private void SetHeaderColorsOnWritePage()
        {
            int nonDkNumber = -1;
            int dkNumber = -1;
            bool currentArrayIsDkColors = false;
            string lastTable = currentSql.myTable;
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                // Display keys and foreignkeys
                field fieldi = currentSql.myFields[i];
                bool myDisplayKey = dataHelper.isDisplayKey(fieldi) && fieldi.table == currentSql.myTable;
                bool myForeignKey = dataHelper.isForeignKeyField(fieldi) && fieldi.table == currentSql.myTable;
                // 2nd default
                if (myDisplayKey)
                {
                    dkNumber++;
                    dataGridView1.Columns[i].HeaderCell.Style.BackColor = formOptions.DkColorArray[dkNumber];
                    dataGridView1.Columns[i].HeaderCell.Style.SelectionBackColor = formOptions.DkColorArray[dkNumber];
                    currentArrayIsDkColors = true;
                    if (myForeignKey)
                    {
                        lastTable = dataHelper.getForeignKeyRefField(fieldi).table;
                    }
                    else
                    {
                        lastTable = currentSql.myTable;
                    }
                }
                else if (myForeignKey)
                {
                    nonDkNumber++;
                    dataGridView1.Columns[i].HeaderCell.Style.BackColor = formOptions.nonDkColorArray[nonDkNumber];
                    dataGridView1.Columns[i].HeaderCell.Style.SelectionBackColor = formOptions.nonDkColorArray[nonDkNumber];
                    currentArrayIsDkColors = false;
                    lastTable = dataHelper.getForeignKeyRefField(fieldi).table;
                }
                else if (lastTable != currentSql.myTable & fieldi.table != currentSql.myTable)
                {
                    if (currentArrayIsDkColors)
                    {
                        dataGridView1.Columns[i].HeaderCell.Style.BackColor = formOptions.DkColorArray[dkNumber];
                        dataGridView1.Columns[i].HeaderCell.Style.SelectionBackColor = formOptions.DkColorArray[dkNumber];
                    }
                    else
                    {
                        dataGridView1.Columns[i].HeaderCell.Style.BackColor = formOptions.nonDkColorArray[nonDkNumber];
                        dataGridView1.Columns[i].HeaderCell.Style.SelectionBackColor = formOptions.nonDkColorArray[nonDkNumber];
                    }
                }
                else
                {
                    dataGridView1.Columns[i].HeaderCell.Style.BackColor = formOptions.DefaultColumnColor;
                    dataGridView1.Columns[i].HeaderCell.Style.SelectionBackColor = formOptions.DefaultColumnColor;
                    lastTable = currentSql.myTable;
                }
            }
        }

        private void SetAllFiltersOnModeChange()
        {
            switch (programMode)
            {
                case ProgramMode.none:
                    EnableMainFilter(false);
                    EnableGridFilters(false);
                    EnableComboFilters(false);
                    break;
                case ProgramMode.view:
                    EnableMainFilter(true);
                    EnableGridFilters(true);
                    EnableComboFilters(true);
                    break;
                case ProgramMode.edit:
                    EnableGridFilters(true);
                    EnableComboFilters(true);
                    break;
                case ProgramMode.delete:
                    EnableGridFilters(true);
                    EnableComboFilters(false);
                    break;
                case ProgramMode.add:
                    EnableGridFilters(true);
                    EnableComboFilters(true);
                    break;
                case ProgramMode.merge:
                    EnableGridFilters(true);
                    EnableComboFilters(false);
                    break;
            }
            // All modes
            SetColumnsReadOnlyProperty();
            SetSelectedCellsColor();
            SetTableLayoutPanelHeight();
        }

        private void ClearFiltersOnNewTable()
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5 };
            for (int i = 0; i < cmbGridFilterFields.Length; i++)
            {
                cmbGridFilterFields[i].DataSource = null;
                cmbGridFilterFields[i].Items.Clear();
                cmbGridFilterFields[i].Visible = false;
                cmbGridFilterFields[i].Enabled = false;

                cmbGridFilterValue[i].DataSource = null;
                cmbGridFilterValue[i].Items.Clear();
                cmbGridFilterValue[i].Visible = false;
                cmbGridFilterValue[i].Enabled = false;

            }
            cmbComboTableList.DataSource = null;
            cmbComboTableList.Visible = false;
            cmbComboTableList.Enabled = false;

            ClearAllComboFilters();
        }

        private void ClearAllComboFilters()
        {

            Label[] lblCmbFilterFields = { lblCmbFilterField_0, lblCmbFilterField_1, lblCmbFilterField_2, lblCmbFilterField_3, lblCmbFilterField_4, lblCmbFilterField_5 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };
            for (int i = 0; i < cmbComboFilterValue.Length; i++)
            {
                cmbComboFilterValue[i].DataSource = null;
                cmbComboFilterValue[i].Text = String.Empty;   // No major event when this changed
                lblCmbFilterFields[i].Text = String.Empty;
                // Selection_change event of cmbComboTableList will make some visible
                // Visible labels will continue to visible for the table, but txtBoxes may be disabled
                cmbComboFilterValue[i].Visible = false;
                cmbComboFilterValue[i].Enabled = false;
                lblCmbFilterFields[i].Visible = false;
                lblCmbFilterFields[i].Enabled = false;
            }
        }

        private void EnableComboFilters(bool enable)
        {
            // Combo filter selection combo
            if (cmbComboTableList.Items.Count == 0 || enable == false)
            {
                cmbComboTableList.Enabled = false;
                cmbComboTableList.Visible = false;
            }
            else
            {
                cmbComboTableList.Enabled = true;
                cmbComboTableList.Visible = true;
            }
            // Six Combo filters
            Label[] lblCmbFilterFields = { lblCmbFilterField_0, lblCmbFilterField_1, lblCmbFilterField_2, lblCmbFilterField_3, lblCmbFilterField_4, lblCmbFilterField_5 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };
            // Keep in use labels visible, but disable to txtBox - and don't filter on them when disabled
            for (int i = 0; i < cmbComboFilterValue.Length; i++)
            {
                if (lblCmbFilterFields[i].Visible && enable == true)
                {
                    cmbComboFilterValue[i].Enabled = true;
                }
                else
                {
                    cmbComboFilterValue[i].Enabled = false;
                }
            }

        }

        private void EnableMainFilter(bool enable)
        {
            // Remove Past filters when disabling it - only done for "programMode.none"
            if (!enable)
            {
                while (MainFilterList.Count > 1) { MainFilterList.RemoveAt(MainFilterList.Count - 2); }
                cmbMainFilter.Enabled = false;
            }
            else
            {
                if (MainFilterList.Count > 10) { MainFilterList.RemoveAt(9); }  // #10 is the dummy
                if (MainFilterList.Count > 1)
                {
                    cmbMainFilter.Enabled = true;  // The first item is the dummy field
                }
            }


        }

        // The content of the cmbGridFilterFields is set in Write_NewTable and never changed for the given table.
        // The content of cmbGridFilterValue will be filtered by Combo Filters 
        private void EnableGridFilters(bool enable)
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5 };
            for (int i = 0; i < cmbGridFilterFields.Count(); i++)
            {
                if (cmbGridFilterFields[i].Items.Count == 0 || enable == false)
                {
                    cmbGridFilterFields[i].Visible = false;
                    cmbGridFilterFields[i].Enabled = false;
                    cmbGridFilterValue[i].Visible = false;
                    cmbGridFilterValue[i].Enabled = false;
                }
                else
                {
                    cmbGridFilterFields[i].Visible = true;
                    cmbGridFilterFields[i].Enabled = true;
                    cmbGridFilterValue[i].Visible = true;
                    cmbGridFilterValue[i].Enabled = true;
                }
            }
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region EVENTS - Menu events
        //----------------------------------------------------------------------------------------------------------------------
        private void mnuToolDuplicateDisplayKeys_Click(object sender, EventArgs e)
        {
            // Get display key list of strings from fields table
            if (currentSql == null) { return; }
            String filter = String.Format("TableName = '{0}' and is_DK = 'true'", currentSql.myTable);
            DataRow[] drs = dataHelper.fieldsDT.Select(filter);

            if (drs.Count() == 0) { msgText("No display keys!"); return; }

            List<String> dkFields = new List<String>();
            foreach (DataRow row in drs)
            {
                dkFields.Add(row["ColumnName"].ToString());
            }
            string fields = String.Join(",", dkFields);
            string fields2 = fields + ", Count(*)";
            String sql1 = String.Format("Select {0} From {1} Group By {2} Having Count(*) > 1", fields2, currentSql.myTable, fields);
            dataHelper.extraDT = new DataTable();
            MsSql.FillDataTable(dataHelper.extraDT, sql1);
            if (dataHelper.extraDT.Rows.Count == 0) { msgText("Everything O.K. No duplicates!"); return; }
            msgColor(Color.Navy);
            List<String> andConditions = new List<String>();
            foreach (DataRow row in dataHelper.extraDT.Rows)
            {
                List<String> atomicStatements = new List<String>();
                foreach (string dkField in dkFields)
                {
                    field fl = new field(currentSql.myTable, dkField, DbType.String, 0);
                    String atomicStatement = String.Format("{0} = '{1}'", dataHelper.QualifiedFieldName(fl), row[dkField].ToString());
                    atomicStatements.Add(atomicStatement);
                }
                andConditions.Add("(" + String.Join(" AND ", atomicStatements) + ")");
            }
            string whereCondition = String.Join(" OR ", andConditions);
            tableOptions.fixingDatabase = true;
            tableOptions.strFixDatabaseWhereCondition = whereCondition;
            writeGrid_NewPage();
        }

        private void mnuToolsDatabaseInformation_Click(object sender, EventArgs e)
        {
            frmDatabaseInfo formDBI = new frmDatabaseInfo();
            formDBI.ShowDialog();
        }

        private void mnuDeleteDatabase_Click(object sender, EventArgs e)
        {
            List<connectionString> csList = AppData.GetConnectionStringList();
            List<string> strCsList = new List<string>();
            foreach (connectionString cs in csList)
            {
                strCsList.Add(cs.comboString);
            }
            frmListItems databaseListForm = new frmListItems();
            databaseListForm.myList = strCsList;
            databaseListForm.myJob = frmListItems.job.DeleteConnections;
            databaseListForm.ShowDialog();
            string returnString = databaseListForm.returnString;
            databaseListForm = null;
            if (returnString != null)
            {
                foreach (connectionString cs in csList)
                {
                    if (cs.comboString == returnString)
                    {
                        csList.Remove(cs);
                        break;   // Only remove the first one - should never be more than 1
                    }
                }
                AppData.storeConnectionStringList(csList);
            }
        }

        private void GridContextMenu_FindInDescendent_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                // Get list of tables that have this table as foreign key
                DataRow[] drs = dataHelper.fieldsDT.Select(String.Format("RefTable = '{0}'", currentSql.myTable));
                if (drs.Count() > 0)
                {
                    List<string> tableList = new List<string>();
                    foreach (DataRow dr in drs)
                    {
                        tableList.Add(dr["TableName"].ToString());
                    }

                    // Get user choice if more than one
                    frmListItems DescendentTablesForm = new frmListItems();
                    DescendentTablesForm.myList = tableList;
                    DescendentTablesForm.myJob = frmListItems.job.SelectString;
                    DescendentTablesForm.Text = "Select Table";
                    DescendentTablesForm.ShowDialog();
                    string selectedTable = DescendentTablesForm.returnString;
                    DescendentTablesForm = null;
                    if (selectedTable != null)
                    {
                        // 1. Define mainFilter (Type: where).
                        // 1a. This assumes the first column in row is the Primary Key and it is an integer
                        string value = dataGridView1.SelectedRows[0].Cells[0].Value.ToString(); //Integer



                        //// 2. Get DisplayValue of selected row                
                        //List<String> displayValueList = new List<String>();
                        //foreach (field dkField in currentSql.DisplayFields_Ostensive)
                        //{
                        //    int fieldIndex = dataHelper.currentDT.Columns.IndexOf(dkField.fieldName);
                        //    string str = dataGridView1.SelectedRows[0].Cells[fieldIndex].Value.ToString();
                        //    if (!String.IsNullOrEmpty(str))
                        //    {
                        //        displayValueList.Add(str);
                        //    }
                        //}
                        //string displayValue = string.Empty;
                        //if (displayValueList.Count == 0)
                        //{ displayValue = "No Display columns!"; }
                        //else
                        //{ displayValue = String.Join(", ", displayValueList); }

                        ////3. Get filter "where" for selected row
                        //field fi2 = new field(currentSql.myFields[0].table, currentSql.myFields[0].fieldName, DbType.Int32, 4);
                        where newMainFilter = dataHelper.GetWhereFromPrimaryKey(currentSql.myTable, value);
                        //where newMainFilter = new where(fi2, value);
                        // newMainFilter.DisplayMember = displayValue; // WhereValue set in constructor
                        //1d. Update pastFilters list

                        foreach (where wh in MainFilterList)
                        {
                            if (wh.isSameWhereAs(newMainFilter)) { MainFilterList.Remove(wh); break; }
                        }

                        MainFilterList.Insert(0, newMainFilter);
                        cmbMainFilter.Refresh();
                        formOptions.updating = true;  // following will not write grid
                        cmbMainFilter.SelectedIndex = 0;
                        formOptions.updating = false;
                        writeGrid_NewTable(selectedTable);
                    }
                }
            }
        }

        private void GridContextMenu_FindInAncestor_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                // Get list of Foriegn keys
                DataRow[] drs = dataHelper.fieldsDT.Select(String.Format("TableName = '{0}' AND is_FK = 'True'", currentSql.myTable));
                if (drs.Count() > 0)
                {
                    List<string> tableList = new List<string>();
                    foreach (DataRow dr in drs)
                    {
                        tableList.Add(dr["RefTable"].ToString());
                    }

                    // Get user choice if more than one
                    frmListItems AncestorTablesForm = new frmListItems();
                    AncestorTablesForm.myList = tableList;
                    AncestorTablesForm.myJob = frmListItems.job.SelectString;
                    AncestorTablesForm.Text = "Select Table";
                    AncestorTablesForm.ShowDialog();
                    string selectedTable = AncestorTablesForm.returnString;
                    int selectedTableIndex = AncestorTablesForm.returnIndex;
                    AncestorTablesForm = null;
                    if (selectedTableIndex > -1)
                    {
                        // 1.  Get Where for ancestor table 
                        DataRow dr = drs[selectedTableIndex];
                        field fieldInCurrentTable = dataHelper.getFieldFromFieldsDT(dr);
                        int dgColumnIndex = -1;
                        for (int i = 0; i < currentSql.myFields.Count; i++)
                        {
                            if (currentSql.myFields[i].isSameFieldAs(fieldInCurrentTable))
                            {
                                dgColumnIndex = i;
                                break;
                            }
                        }
                        if (dgColumnIndex > -1)  // A needless check
                        {
                            string whereValue = dataGridView1.SelectedRows[0].Cells[dgColumnIndex].Value.ToString();

                            where newMainFilter = dataHelper.GetWhereFromPrimaryKey(selectedTable, whereValue);
                            foreach (where wh in MainFilterList)
                            {
                                if (wh.isSameWhereAs(newMainFilter)) { MainFilterList.Remove(wh); break; }
                            }
                            MainFilterList.Insert(0, newMainFilter);
                            cmbMainFilter.Refresh();
                            formOptions.updating = true;  // following will not write grid
                            cmbMainFilter.SelectedIndex = 0;
                            formOptions.updating = false;
                            writeGrid_NewTable(selectedTable);
                        }
                    }
                }
            }

        }


        private void mnuOpenTables_Click(object sender, EventArgs e) { }

        private void mnuOpenTables_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string tableName = e.ClickedItem.Name;  // the text can be translated.  Set Name = Text when adding ToolStripMenuItem
            //Open a new table
            writeGrid_NewTable(tableName);
        }

        private void mnuDatabaseList_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // Get index clicked
            ToolStripItem clickedItem = e.ClickedItem;
            int index = 0;
            ToolStripMenuItem? fatherItem = sender as ToolStripMenuItem;
            if (fatherItem != null)
            {
                for (int i = 0; i < fatherItem.DropDownItems.Count; i++)
                {
                    if (fatherItem.DropDownItems[i] == clickedItem)
                    {
                        index = i; break;
                    }
                }
            }

            string readOnly = "", connectionString = "", str = "", tye = "", sd = "";
            //1. Change clicked link to index 0 - index 0 used to open connection
            if (index > 0)  //No need to change if already 0
            {
                List<connectionString> csList = AppData.GetConnectionStringList();
                connectionString cs = csList[index];   // Assumes list in dropdown matches csList
                csList.Remove(cs);
                csList.Insert(0, cs);
                AppData.storeConnectionStringList(csList);
            }
            //2. Open connection - this reads the index 0 settings.  Main use of openConnection
            string msg = OpenConnection();
            msgColor(Color.Red);
            if (msg != string.Empty) { msgText(msg); }

        }

        // Add Database - frmConnection
        internal void mnuAddDatabase_Click(Object eventSender, EventArgs eventArgs)
        {
            frmConnection connectionForm = new frmConnection();
            //Get connection string
            connectionForm.ShowDialog();
            bool connectionAdded = connectionForm.success;
            connectionForm.Close();
            //Store values, reload menu, open connection
            if (connectionAdded)
            {
                load_mnuDatabaseList();
                string msg = OpenConnection();
                msgColor(Color.Red);
                if (msg != string.Empty) { msgText(msg); }
            }
        }

        internal void load_mnuDatabaseList()
        {
            //Get list from App.Data
            List<connectionString> csList = AppData.GetConnectionStringList();
            mnuConnectionList.DropDownItems.Clear();
            foreach (connectionString cs in csList)
            {
                // {0} for server, {1} for Database, {2} for user, {3} for password (unknown)
                string csString = String.Format(cs.comboString, cs.server, cs.databaseName, cs.user, "******");
                mnuConnectionList.DropDownItems.Add(csString);
            }
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Events - Datagrid Events

        private void dataGridView1_MouseLeave(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                MessageBox.Show("Current cell not yet saved. Click any cell to save.", "Change not saved");
            }
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(",CeDirty:" + dataGridView1.IsCurrentCellDirty.ToString());
            if (dataGridView1.IsCurrentCellDirty)
            {
                DataGridViewColumn col = dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex];
                FkComboColumn fkCol = col as FkComboColumn;
                if (fkCol != null)
                {
                    SendKeys.Send("{ENTER}");
                }

            }
        }

        // Update datatable in FkComboEditingControl on entering cell
        // Even if I leave, I will re-enter
        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (programMode == ProgramMode.edit)
            {
                field colField = currentSql.myFields[e.ColumnIndex];
                // Set update command
                MsSql.SetUpdateCommand(colField, dataHelper.currentDT);
                // Handle foreign keys
                DataGridViewColumn col = dataGridView1.Columns[e.ColumnIndex];
                FkComboColumn Fkcol = col as FkComboColumn;
                if (Fkcol != null)  // Will be null for non-foreign key row
                {
                    // Update the Editing control
                    // if (!colField.SameFieldAs(tableOptions.FkFieldInEditingControl))
                    // {
                    // Fill Data table (datahelper.extraDT)
                    field FkTablePKField = dataHelper.getForeignKeyRefField(colField);
                    FillExtraDTForUseInCombo(FkTablePKField);   //Filter by all comboFilters
                                                                // Assign datatable to each cell in FK column
                    int index = dataHelper.currentDT.Columns.IndexOf(col.Name);
                    for (int j = 0; j < dataHelper.currentDT.Rows.Count; j++)
                    {
                        FkComboCell fkCell = (FkComboCell)dataGridView1.Rows[j].Cells[index];
                        fkCell.dataTable = dataHelper.extraDT;
                    }
                    tableOptions.FkFieldInEditingControl = colField;
                    // }
                }
            }
        }

        // When foreign key selected in dropdown, this puts the selectedvalue into the cell.
        private void dataGridView1_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            // Note: CellParsing accepts user input and map it to a different cell value / format.
            if (programMode == ProgramMode.edit) msgDebug(", Cell_Parsing");
            // Using editing control - true for both foreign keys and strings, but false for checkbox changes
            if (dataGridView1.EditingControl != null)
            {
                DataGridViewColumn selectedColumn = dataGridView1.Columns[e.ColumnIndex];
                FkComboColumn Fkcol = selectedColumn as FkComboColumn;
                if (Fkcol != null)
                {
                    FkComboBoxEditingControl editingControl = dataGridView1.EditingControl as FkComboBoxEditingControl;
                    if (editingControl != null)  // Always true ?
                    {
                        int foreignKey = Convert.ToInt32(editingControl.SelectedValue);
                        e.Value = foreignKey;
                        e.ParsingApplied = true;
                        return;
                    }
                }
            }
            e.ParsingApplied = false;
        }

        // Push the change in the cell value down to the database
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", CVC");
            if (!formOptions.updating)
            {
                DataGridViewCell currentCell = dataGridView1.CurrentCell;
                field drColField = currentSql.myFields[currentCell.ColumnIndex];
                field PKfield = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                int pkIndex = dataGridView1.Columns[PKfield.fieldName].Index;
                int PKvalue = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[pkIndex].Value);
                DataRow dr = dataHelper.currentDT.Select(string.Format("{0} = {1}", PKfield.fieldName, PKvalue)).FirstOrDefault();
                if (dataGridView1.IsCurrentCellDirty) // always true
                {
                    dr.EndEdit();  // Changes rowstate to modified
                    DataRow[] drArray = new DataRow[1];
                    drArray[0] = dr;
                    int i = MsSql.currentDA.Update(drArray);
                    msgColor(Color.Navy);
                    msgText("Rows Modified: ");
                    msgTextAdd(i.ToString());
                    // Write the grid if this is a foreign key
                    if (i > 0)
                    {
                        DataGridViewColumn col = dataGridView1.Columns[currentCell.ColumnIndex];
                        FkComboColumn fkCol = col as FkComboColumn;
                        if (fkCol != null)
                        {
                            writeGrid_NewPage();
                        }
                    }

                }
            }
        }

        // Message to user about data error - cancel error
        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("OOPS! Database error.");
            sb.AppendLine(e.Exception.Message);
            sb.AppendLine("Error happened " + e.Context.ToString());

            if (e.Context == DataGridViewDataErrorContexts.Commit)
            {
                sb.AppendLine("Commit error");
            }
            if (e.Context == DataGridViewDataErrorContexts.CurrentCellChange)
            {
                sb.AppendLine("Cell change");
            }
            if (e.Context == DataGridViewDataErrorContexts.Parsing)
            {
                sb.AppendLine("Parsing error");
            }
            if (e.Context == DataGridViewDataErrorContexts.LeaveControl)
            {
                sb.AppendLine("Leave control error");
            }

            if ((e.Exception) is ConstraintException)
            {
                DataGridView view = (DataGridView)sender;
                view.Rows[e.RowIndex].ErrorText = "an error";
                view.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "an error";
                e.ThrowException = false;
            }

            MessageBox.Show(sb.ToString());

        }

        // Change background color of cell
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", BE");
            // Change colors
            DataGridViewCellStyle cs = new DataGridViewCellStyle();
            cs.BackColor = Color.Aqua;
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style = cs;
        }

        // Restore default background color of cell
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", EE");
            // Change color back to default
            DataGridViewCellStyle cs = new DataGridViewCellStyle();
            cs.BackColor = DefaultBackColor;
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style = cs;
        }

        // Add new events to FkComboBoxEditingControl editing control
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            FkComboBoxEditingControl ctl = e.Control as FkComboBoxEditingControl;
            if (ctl != null)
            {
                if (programMode == ProgramMode.edit) msgDebug(", EditControlShowing");
                ctl.DropDown -= new EventHandler(AdjustWidthComboBox_DropDown);
                ctl.DropDown += new EventHandler(AdjustWidthComboBox_DropDown);
            }
        }

        // Only allow 2 rows to be selected - used when merging
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count != 0)
            {
                if (programMode == ProgramMode.merge)
                {
                    if (dataGridView1.SelectedRows.Count > 2)
                    {
                        for (int i = 2; i < dataGridView1.SelectedRows.Count; i++)
                        {
                            dataGridView1.SelectedRows[i].Selected = false;
                        }
                    }
                }
                else if (programMode == ProgramMode.edit)
                {

                    {
                        SetSelectedCellsColor();
                    }
                }
            }
        }

        // Sort grid on column
        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            System.Windows.Forms.SortOrder newSortOrder = System.Windows.Forms.SortOrder.Ascending;   // default
                                                                                                      // Check for same column ascending and change to descending  
            if (currentSql.myOrderBys.Count > 0)
            {
                if (currentSql.myOrderBys[0].fld == currentSql.myFields[e.ColumnIndex])  // New column is the same as the old.
                {
                    if (currentSql.myOrderBys[0].sortOrder == System.Windows.Forms.SortOrder.Ascending)
                    {
                        newSortOrder = System.Windows.Forms.SortOrder.Descending;
                    }
                }
            }
            // Update myOrdersBy to sort by newColumn and dir - may be same column
            currentSql.myOrderBys.Clear();
            orderBy ob = new orderBy(currentSql.myFields[e.ColumnIndex], newSortOrder);
            currentSql.myOrderBys.Add(ob);
            currentSql.myPage = 1;
            // Write the grid with the new order - write Grid will format the header cell
            writeGrid_NewPage();
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Events - Non-Datagrid Controls

        // Calls Write_NewFilter
        private void cmbMainFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = cmbMainFilter.SelectedIndex;
            //Last element means "no filter".
            if (cmbMainFilter.SelectedIndex == cmbMainFilter.Items.Count - 1)
            {
                lblMainFilter.Text = "Research: ";
            }
            else
            {
                lblMainFilter.Text = "Research: ";
            }
            // Load_Form adds dummy item and binds (so Count = 1)
            // Find in Descendent / Ancester add filter, sets mainfilter to 0 and then calls Write_NewTable
            // So in either case, don't call WriteGrid here.
            if (cmbMainFilter.Items.Count > 1 && !formOptions.updating)
            {
                writeGrid_NewFilter();
            }
        }

        // Update the GridFilterValues to the possible values for new filter field.
        // Will rewrite grid with Filter Value set to selected item 0 (= string.empty)
        public void cmbGridFilterFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5 };
            ComboBox cmb = (ComboBox)sender;
            if (cmb.SelectedIndex > -1)
            {
                for (int i = 0; i < cmbGridFilterFields.Count(); i++)
                {
                    // Get combo box
                    if (cmbGridFilterFields[i] == cmb)
                    {
                        RebindGridFilterValues(i);
                    }
                }
            }
        }

        // Loads new values into GridFilterValue with index i, and selects the first value (dummy value = no filter)
        private void RebindGridFilterValues(int i)
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5 };

            // This sets GridFilterValue text to item 0 (String.Empty), and calls Write_newFilter
            // Do this even when updating, because values should show with "Select Value" as first element.
            // But GridFilterValue text change will not write Grid when updating.
            cmbGridFilterValue[i].DataSource = null;
            cmbGridFilterValue[i].Items.Clear();
            cmbGridFilterValue[i].Enabled = true;

            // Fill cmbComboFilterValue[i]
            field selectedField = (field)cmbGridFilterFields[i].SelectedValue;
            FillExtraDTForUseInCombo(selectedField);
            DataRow firstRow = dataHelper.extraDT.NewRow();
            // The Datasource in GridFieldsCombos must have "DisplayMember" and "ValueMember" columns
            // Leave these values null, because they might need correct type
            int displayColIndex = dataHelper.extraDT.Columns.IndexOf("DisplayMember");
            int valueColIndex = dataHelper.extraDT.Columns.IndexOf("ValueMember");
            dataHelper.extraDT.Rows.InsertAt(firstRow, 0); // Even if no rows
            cmbGridFilterValue[i].DisplayMember = "DisplayMember";
            cmbGridFilterValue[i].ValueMember = "ValueMember";
            cmbGridFilterValue[i].DataSource = dataHelper.extraDT;  // Calls WriteGrid_NewFilters
            this.ActiveControl = null;  // To unselect the text (and change color from blue to background color).
        }

        // Calls Write_NewFilter
        private void cmbGridFilterValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            if (cmb.SelectedIndex > -1)  // If data source null
            {
                if (!formOptions.updating)  // On Write_NewTable
                {
                    writeGrid_NewFilter();
                }
            }
        }

        private void cmbComboTableList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Label[] lblCmbFilterFields = { lblCmbFilterField_0, lblCmbFilterField_1, lblCmbFilterField_2, lblCmbFilterField_3, lblCmbFilterField_4, lblCmbFilterField_5 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };

            ClearAllComboFilters();
            ComboBox cmb = (ComboBox)sender;
            // Fill combo filters with each value in ostensive definition
            if (cmb.SelectedIndex > -1)
            {
                field selectedValue = (field)cmb.SelectedValue;
                comboSql = new sqlFactory(selectedValue.table, 0, 0);
                int comboNumber = 0;
                // I added primary key if there is no display-key -  O.K. if this is the PK of this table.  But . . .
                // If this is the primary key of a FK of this table, I must change this to FK of this table.
                // Example: the degree key of Student-Degree table if Degree table has no DK.
                // Only used to get the right color of the header below
                if (selectedValue.table != currentSql.myTable)
                {
                    if (comboSql.DisplayFields_Ostensive.Count == 1)
                    {
                        field odField = comboSql.DisplayFields_Ostensive[0];
                        if (dataHelper.isTablePrimaryKeyField(odField))
                        {
                            comboSql.DisplayFields_Ostensive.RemoveAt(0);
                            DataRow[] drs = dataHelper.fieldsDT.Select(String.Format("TableName = '{0}' AND RefTable = '{1}'", currentSql.myTable, odField.table));
                            field fkField = dataHelper.getFieldFromFieldsDT(drs[0]);
                            comboSql.DisplayFields_Ostensive.Add(fkField);
                        }
                    }
                }
                foreach (field fi in comboSql.DisplayFields_Ostensive)
                {
                    // Get correct header color - on updating in Write_NewTable currentSql is not yet loaded and so myFields is empty
                    if (!formOptions.updating)  // Only used to stop coloring on writing_page.  This is done at end of writing_page
                    {
                        int colIndex = -1;
                        for (int i = 0; i < currentSql.myFields.Count; i++)
                        {
                            // Normal case
                            if (currentSql.myFields[i].isSameFieldAs(fi)) { colIndex = i; break; }
                        }
                        lblCmbFilterFields[comboNumber].BackColor = dataGridView1.Columns[colIndex].HeaderCell.Style.BackColor;
                        cmbComboFilterValue[comboNumber].BackColor = dataGridView1.Columns[colIndex].HeaderCell.Style.BackColor;
                    }
                    lblCmbFilterFields[comboNumber].Text = fi.fieldName + ":"; // Shorter than DisplayName
                    lblCmbFilterFields[comboNumber].Visible = true;
                    lblCmbFilterFields[comboNumber].Enabled = true;
                    cmbComboFilterValue[comboNumber].Visible = true;
                    cmbComboFilterValue[comboNumber].Enabled = true;
                    cmbComboFilterValue[comboNumber].Tag = fi;  // Used in program
                    comboNumber++;
                }
            }
        }

        private void cmbComboFilterValue_TextChanged(object sender, EventArgs e)
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5 };
            // If a grid filter field is string.empty (i.e. no filter), and this combo change changes 
            // its possible values, then call its index change event (although it is not changed).
            // This will redo the possible values in grid filter value combo 
            ComboBox cmb = (ComboBox)sender;
            if(cmb.Tag != null)   // I set to null when clearing all filters
            { 
                for (int i = 0; i < cmbGridFilterFields.Count(); i++)
                {
                    if (cmbGridFilterFields[i].Enabled)
                    {
                        if (cmbGridFilterValue[i].SelectedIndex < 1)   // EmptyString or -1
                        {
                            //No datasource
                            if (cmbGridFilterValue[i].SelectedIndex == -1)
                            {
                                RebindGridFilterValues(i);
                                return;
                            }
                            field gridFilterField = (field)cmbGridFilterFields[i].SelectedValue;
                            field comboFilterField = (field)cmb.Tag;
                            // Same field - will rebind to this element only
                            if (gridFilterField.isSameFieldAs(comboFilterField))
                            {
                                RebindGridFilterValues(i);
                                return;
                            }
                            if (dataHelper.isTablePrimaryKeyField(gridFilterField))
                            {
                                if (gridFilterField.table == currentSql.myTable)
                                {
                                    //Fire event
                                    RebindGridFilterValues(i);
                                }
                                else
                                {
                                    sqlFactory sqlFact = new sqlFactory(gridFilterField.table, 0, 0);
                                    if (sqlFact.TableIsInMyTables(comboFilterField.table))
                                    {
                                        //Fire event
                                        RebindGridFilterValues(i);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
 
        // On entering cell, load the combo with all useful distinct values
        // When viewing, this will be the distinct values in this column in grid
        // When editing, it will be all distinct values consistent with main filter and other combo filters 
        private void cmbComboFilterValue_Enter(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            // Set source to null when it needs changed
            if (cmb.DataSource == null)
            {
                field fi = (field)cmb.Tag;
                //Fill with distinct strings from either currentDT or the entire Table
                List<string> strList = new List<string>();
                if (InAnEditingMode())
                {   // Will select distinct elements, and filter by other visible comboFilter values.
                    // Since this filter is null, it will not filter by this filter
                    FillExtraDTForUseInCombo(fi);
                    strList = dataHelper.extraDT.AsEnumerable().Select(x => x["DisplayMember"].ToString()).ToList();
                }
                else
                {
                    // Select distinct elements in currentDT
                    strList = (dataHelper.currentDT.AsEnumerable().Select(x => x[fi.fieldName].ToString()).Distinct()).ToList();
                }
                // Insert Dummy element
                strList.Insert(0, String.Empty);
                BindingList<string> strBindingList = new BindingList<string>(strList);
                // Text will change to string.empty, but don't want to fire text change event because text originally was string.empty
                formOptions.updating = true;
                cmb.DataSource = strBindingList;
                formOptions.updating = false;
            }
        }


        #region 5 paging buttons & RecordsPerPage (RPP)
        // Paging - <<
        private void txtRecordsPerPage_Leave(object sender, EventArgs e)
        {
            int rpp = 0;
            if (int.TryParse(txtRecordsPerPage.Text, out rpp))
            {
                if (rpp > 9)
                {
                    formOptions.pageSize = rpp;
                    AppData.SaveKeyValue("RPP", rpp.ToString());
                }

            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                int oneFifth = (int)Math.Ceiling((decimal)currentSql.TotalPages / 5);
                int pageLeap = Math.Max(currentSql.myPage - oneFifth, 1);
                if (currentSql.myPage != pageLeap)
                {
                    currentSql.myPage = pageLeap;
                    writeGrid_NewPage();
                }
            }

        }
        // Paging - <
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                if (currentSql.myPage > 1)
                {
                    currentSql.myPage--;
                    writeGrid_NewPage();
                }
            }
        }
        // Paging - pages / totalPages
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                frmCaptions captionsForm = new frmCaptions("Pages", "pages");
                //Get connection string
                captionsForm.totalPages = currentSql.TotalPages;
                captionsForm.gridFormWidth = this.Width;
                captionsForm.gridFormLeftLocation = this.Location.X;
                captionsForm.ShowDialog();
                int pageSelected;
                bool captionIsInt = Int32.TryParse(captionsForm.selectedCaption, out pageSelected);
                if (!captionIsInt)
                {
                    pageSelected = currentSql.myPage;
                }
                captionsForm.Close();
                if (pageSelected != currentSql.myPage)
                {
                    currentSql.myPage = pageSelected;
                    writeGrid_NewPage();
                }
            }
        }
        // Paging - >
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                if (currentSql.myPage < currentSql.TotalPages)
                {
                    currentSql.myPage++;
                    writeGrid_NewPage();
                }
            }
        }
        // Paging - >>
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (currentSql != null)
            {
                int oneFifth = (int)Math.Ceiling((decimal)currentSql.TotalPages / 5);
                int pageLeap = Math.Min(currentSql.myPage + oneFifth, currentSql.TotalPages);
                if (pageLeap != currentSql.TotalPages)
                {
                    currentSql.myPage = pageLeap;
                    writeGrid_NewPage();
                }
            }
        }
        #endregion

        #region 5 mode radio buttons changed - view, edit, add, delete merge

        private void rbView_CheckedChanged(object sender, EventArgs e)
        {
            if (rbView.Checked)
            {
                programMode = ProgramMode.view;
                dataGridView1.MultiSelect = false;
                dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
                btnDeleteAddMerge.Enabled = false;
                SetAllFiltersOnModeChange();
            }
        }

        private void rbDelete_CheckedChanged(object sender, EventArgs e)
        {
            if (rbDelete.Checked)
            {
                programMode = ProgramMode.delete;
                dataGridView1.MultiSelect = false;
                dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
                btnDeleteAddMerge.Enabled = true;
                btnDeleteAddMerge.Text = MultiLingual.tr("Delete row", this);
                // Add deleteCommand
                MsSql.SetDeleteCommand(currentSql.myTable, dataHelper.currentDT);
                SetAllFiltersOnModeChange();
            }
        }

        private void rbEdit_CheckedChanged(object sender, EventArgs e)
        {
            if (rbEdit.Checked)
            {
                programMode = ProgramMode.edit;
                dataGridView1.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
                dataGridView1.MultiSelect = false;
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.ReadOnly = true;
                }
                btnDeleteAddMerge.Enabled = false;
                SetAllFiltersOnModeChange();
            }
        }

        private void rbAdd_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAdd.Checked)
            {
                programMode = ProgramMode.add;
                dataGridView1.MultiSelect = false;
                dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
                btnDeleteAddMerge.Enabled = true;
                btnDeleteAddMerge.Text = MultiLingual.tr("Add row", this);
                SetAllFiltersOnModeChange();

                //// Create the InsertCommand.
                //command = new SqlCommand(
                //    "INSERT INTO Customers (CustomerID, CompanyName) " +
                //    "VALUES (@CustomerID, @CompanyName)", connection);

                //// Add the parameters for the InsertCommand.
                //command.Parameters.Add("@CustomerID", SqlDbType.NChar, 5, "CustomerID");
                //command.Parameters.Add("@CompanyName", SqlDbType.NVarChar, 40, "CompanyName");

                //adapter.InsertCommand = command;
            }
        }


        private void rbMerge_CheckedChanged(object sender, EventArgs e)
        {
            if (rbMerge.Checked)
            {
                programMode = ProgramMode.merge;
                btnDeleteAddMerge.Enabled = true;
                btnDeleteAddMerge.Text = MultiLingual.tr("Merge 2 rows", this);
                dataGridView1.MultiSelect = true;
                MsSql.SetDeleteCommand(currentSql.myTable, dataHelper.currentDT);
                SetAllFiltersOnModeChange();
            }
        }

        #endregion

        private void DataGridViewForm_Resize(object sender, EventArgs e)
        {
            SetTableLayoutPanelHeight();
        }

        private void btnDeleteAddMerge_Click(object sender, EventArgs e)
        {
            txtMessages.Text = string.Empty;
            if (programMode == ProgramMode.merge)
            {
                if (dataGridView1.SelectedRows.Count != 2)
                {
                    msgColor(Color.Red);
                    msgText("Please select exactly two rows");
                    return;
                }
                // Get two PK values
                int firstPK = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);
                int secondPK = Convert.ToInt32(dataGridView1.SelectedRows[1].Cells[0].Value);
                txtMessages.Text = String.Format(" {0} is the first and {1} is the second", firstPK, secondPK);
                //  Get rows in the fieldsDT that have myTable as RefTable 
                DataRow[] drs = dataHelper.fieldsDT.Select(String.Format("RefTable = '{0}'", currentSql.myTable));
                // Count how many rows the firstPK and secondPK as FK's in other tables
                int firstPKCount = 0;
                int secondPKCount = 0;
                foreach (DataRow dr in drs)    // Only 1 dr with "Courses" main table, and that is the CourseTerm table.
                {
                    string FKColumnName = dr.ItemArray[dr.Table.Columns.IndexOf("ColumnName")].ToString();
                    string TableWithFK = dr.ItemArray[dr.Table.Columns.IndexOf("TableName")].ToString();

                    string strSql = String.Format("SELECT COUNT(1) FROM {0} where {1} = '{2}'", TableWithFK, FKColumnName, firstPK);
                    firstPKCount = firstPKCount + MsSql.GetRecordCount(strSql);
                    strSql = String.Format("SELECT COUNT(1) FROM {0} where {1} = '{2}'", TableWithFK, FKColumnName, secondPK);
                    secondPKCount = secondPKCount + MsSql.GetRecordCount(strSql);
                    txtMessages.Text = String.Format(" Counts: {0} and {1}", firstPKCount, secondPKCount);
                }
                string msg = String.Empty;
                if (firstPKCount == 0 || secondPKCount == 0)
                {
                    int PkToDelete = 0;
                    if (firstPKCount == 0) { PkToDelete = firstPK; }
                    else { PkToDelete = secondPK; }
                    msg = String.Format(
                        "Deleting row with ID {0} will have no other effect on the Database.  " +
                        "Do you want us to delete the row with ID {0}?", PkToDelete);

                    DialogResult reply = MessageBox.Show(msg, "Delete one row", MessageBoxButtons.YesNo);
                    if (reply == DialogResult.Yes)
                    {
                        field PKField = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                        DataRow dr = dataHelper.currentDT.Select(string.Format("{0} = {1}", PKField.fieldName, PkToDelete)).FirstOrDefault();
                        DeleteOneDataRowFromCurrentDT(dr);
                    }
                }
                else
                {
                    msg = String.Format("Other tables use this table as a foreign key.  To merge these two rows, we will first " +
                        "replace {0} occurances of {1} with {2} in these tables. " +
                        "Then we will delete the row {1} from this table.  Do you want to continue?", firstPKCount, firstPK, secondPK);
                    DialogResult reply = MessageBox.Show(msg, "Merge two rows?", MessageBoxButtons.YesNo);
                    if (reply == DialogResult.Yes)
                    {
                        foreach (DataRow dr in drs)
                        {
                            string FKColumnName = dr.ItemArray[dr.Table.Columns.IndexOf("ColumnName")].ToString();
                            string TableWithFK = dr.ItemArray[dr.Table.Columns.IndexOf("TableName")].ToString();
                            field fld = new field(TableWithFK, FKColumnName, DbType.Int32, 4);
                            // 1. Put the rows to be updated into extraDT  (i.e. select rows where FK value in firstPK)
                            string sqlString = String.Format("Select * from {0} where {1} = '{2}'", TableWithFK, FKColumnName, firstPK);
                            dataHelper.extraDT = new DataTable();
                            MsSql.FillDataTable(dataHelper.extraDT, sqlString);
                            txtMessages.Text = txtMessages.Text + "; " + dataHelper.extraDT.Rows.Count;
                            // 2. Update these rows in extraDT - loop through these rows and change the FK column)
                            foreach (DataRow dr2 in dataHelper.extraDT.Rows)
                            {
                                dr2[FKColumnName] = secondPK;
                                // dr2.AcceptChanges();
                            }
                            // 3. Push these changes to the Database.
                            MsSql.SetUpdateCommand(fld, dataHelper.extraDT);
                            MsSql.extraDA.Update(dataHelper.extraDT);
                        }
                        // 4.  Delete merged row from currentDT
                        field PKField = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                        DataRow dr3 = dataHelper.currentDT.Select(String.Format("{0} = {1}", PKField.fieldName, firstPK)).FirstOrDefault();
                        DeleteOneDataRowFromCurrentDT(dr3);
                    }

                }
                // field pkField = dataHelper.getTablePrimaryKeyField(currentSql.myTable);

                /// Change the FK value in all Higher tables to the one with more such values.
                /// 
            }
            else if (programMode == ProgramMode.delete)
            {
                if (dataGridView1.SelectedRows.Count != 1)
                {
                    msgColor(Color.Red);
                    msgText("Please select row to delete");
                    return;
                }
                // int index = dataGridView1.Rows.IndexOf(dataGridView1.SelectedRows[0]);
                field PKfield = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                int colIndex = dataGridView1.Columns[PKfield.fieldName].Index;
                if (colIndex != 0)
                {
                    msgColor(Color.Red);
                    msgText("The first column must be the primary key.");
                    return;
                }
                int PKvalue = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[colIndex].Value);
                DataRow dr = dataHelper.currentDT.Select(string.Format("{0} = {1}", PKfield.fieldName, PKvalue)).FirstOrDefault();
                DeleteOneDataRowFromCurrentDT(dr);
            }
        }


        // Adjusts the width of all the items shown when combobox dropped down to longest item
        private void AdjustWidthComboBox_DropDown(object sender, EventArgs e)
        {
            // All elements must have a "DisplayMember" field or property.

            // A. Get list of display strings in ComboBox
            var senderComboBox = (System.Windows.Forms.ComboBox)sender;
            if (senderComboBox.Items.Count > 0)
            {
                List<string> displayValueList = new List<string>();
                // 1. FkComboBoxEditingControl
                if (senderComboBox is FkComboBoxEditingControl)
                {
                    var itemsList = senderComboBox.Items.Cast<DataRowView>();
                    foreach (DataRowView drv in itemsList)
                    {
                        int index = drv.Row.Table.Columns.IndexOf("DisplayMember");
                        displayValueList.Add(drv.Row.ItemArray[index].ToString());
                    }
                }
                // 2. Combo bound to fields[]
                else if (senderComboBox.Items[0] is field)
                {
                    var itemsList = senderComboBox.Items.Cast<field>();
                    foreach (field fl in itemsList) { displayValueList.Add(fl.DisplayMember); }
                }
                // 3. Combo bound to DataRowView[]
                else if (senderComboBox.Items[0] is DataRowView)
                {
                    var itemsList = senderComboBox.Items.Cast<DataRowView>();
                    foreach (DataRowView drv in itemsList)
                    {
                        int index = drv.Row.Table.Columns.IndexOf("DisplayMember");
                        displayValueList.Add(drv.Row.ItemArray[index].ToString());
                    }
                }
                // 4. Combo bound to where[]
                else if (senderComboBox.Items[0] is where)
                {
                    var itemsList = senderComboBox.Items.Cast<where>();
                    foreach (where wh in itemsList) { displayValueList.Add(wh.DisplayMember); }
                }
                else if (senderComboBox.Items[0] is string)
                {
                    var itemsList = senderComboBox.Items.Cast<string>();
                    foreach (string str in itemsList) { displayValueList.Add(str); }
                }

                // B. Get and set width
                int width = senderComboBox.Width;
                using (Graphics g = senderComboBox.CreateGraphics())
                {
                    System.Drawing.Font font = senderComboBox.Font;
                    int vertScrollBarWidth = (senderComboBox.Items.Count > senderComboBox.MaxDropDownItems)
                        ? SystemInformation.VerticalScrollBarWidth : 0;
                    // var itemsList = senderComboBox.Items.Cast<object>().Select(item => item.ToString());
                    foreach (string s in displayValueList)
                    {
                        int newWidth = (int)g.MeasureString(s, font).Width + vertScrollBarWidth;
                        if (width < newWidth)
                        {
                            width = newWidth;
                        }
                    }
                }
                senderComboBox.DropDownWidth = width;
            }

        }

        private void mnuUpdateDatabase_Click(object sender, EventArgs e)
        {
            frmUpdateDatabaseToV2 updateForm = new frmUpdateDatabaseToV2();
            updateForm.ShowDialog();
            updateForm = null;
        }


        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        #region Other functions and methods

        private void callSqlWheres()
        {
            ComboBox[] cmbGridFilterFields = { cmbGridFilterFields_0, cmbGridFilterFields_1, cmbGridFilterFields_2, cmbGridFilterFields_3, cmbGridFilterFields_4, cmbGridFilterFields_5 };
            ComboBox[] cmbGridFilterValue = { cmbGridFilterValue_0, cmbGridFilterValue_1, cmbGridFilterValue_2, cmbGridFilterValue_3, cmbGridFilterValue_4, cmbGridFilterValue_5 };

            // 1. Clear any old filters from currentSql
            currentSql.myWheres.Clear();

            // 2. Main filter - add this where to the currentSql)
            if (cmbMainFilter.SelectedIndex != cmbMainFilter.Items.Count - 1)
            {
                where mfWhere = (where)cmbMainFilter.SelectedValue;
                if (Convert.ToInt32(mfWhere.whereValue) > 0)
                {
                    // Check that the table is in the myFields
                    if (currentSql.TableIsInMyTables(mfWhere.fl.table))
                    {
                        currentSql.myWheres.Add(mfWhere);
                    }
                }
            }
            // 3. cmbGridFilterFields - Currently 6.
            for (int i = 0; i < cmbGridFilterFields.Length; i++)
            {
                if (cmbGridFilterFields[i].Enabled)  // Usually true, but enable only 3 when only 1 or 2 display keys
                {
                    if (cmbGridFilterValue[i].SelectedIndex > 0) // 0 is the pseudo item "-1" or "Filter Value"
                    {
                        field selectedField = (field)cmbGridFilterFields[i].SelectedItem;
                        where wh = new where(selectedField, cmbGridFilterValue[i].SelectedValue.ToString());
                        if (dataHelper.TryParseToDbType(wh.whereValue, selectedField.dbType))
                        {
                            currentSql.myWheres.Add(wh);
                        }
                        else
                        {
                            string erroMsg = String.Format(dataHelper.errMsg, dataHelper.errMsgParameter1, dataHelper.errMsgParameter2);
                            msgColor(Color.Red);
                            msgText(erroMsg);
                        }
                    }
                }
            }
        }

        private void callSqlWheresForCombo()
        {   // Adds all the filters - FK, DK, non-Key and
            Label[] lblCmbFilterFields = { lblCmbFilterField_0, lblCmbFilterField_1, lblCmbFilterField_2, lblCmbFilterField_3, lblCmbFilterField_4, lblCmbFilterField_5 };
            ComboBox[] cmbComboFilterValue = { cmbComboFilterValue_0, cmbComboFilterValue_1, cmbComboFilterValue_2, cmbComboFilterValue_3, cmbComboFilterValue_4, cmbComboFilterValue_5 };

            // Clear any old filters from currentSql
            comboSql.myWheres.Clear();

            //Main filter - add this where to the currentSql)
            if (cmbMainFilter.SelectedIndex != cmbMainFilter.Items.Count - 1)
            {
                where mfWhere = (where)cmbMainFilter.SelectedValue;
                if (Convert.ToInt32(mfWhere.whereValue) > 0)
                {
                    // Check that the table and field is in the myFields
                    if (comboSql.TableIsInMyTables(mfWhere.fl.table))
                    {
                        comboSql.myWheres.Add(mfWhere);
                    }
                }
            }
            // cmbComboFilterFields  (6 fields)
            for (int i = 0; i < cmbComboFilterValue.Length; i++)
            {
                if (cmbComboFilterValue[i].Enabled)  // True iff visible
                {
                    if (cmbComboFilterValue[i].DataSource != null)  // Probably not needed but just in case 
                    {
                        if (cmbComboFilterValue[i].Text != String.Empty)
                        {
                            field selectedField = (field)cmbComboFilterValue[i].Tag;
                            if (comboSql.TableIsInMyTables(selectedField.table))  // Should always be true
                            {
                                where wh = new where(selectedField, cmbComboFilterValue[i].Text); // May be type error
                                comboSql.myWheres.Add(wh);
                            }
                        }
                    }
                }
            }
        }

        private void FillExtraDTForUseInCombo(field PKField)
        {
            comboSql = new sqlFactory(PKField.table, 0, 0);
            callSqlWheresForCombo();  // Filter by main filter and visible combo filters
            // combo.returnComboSql works very differently for Primary keys and non-Primary keys
            string strSql = comboSql.returnComboSql(PKField);
            dataHelper.extraDT = new DataTable();
            MsSql.FillDataTable(dataHelper.extraDT, strSql);
        }

        private void DeleteOneDataRowFromCurrentDT(DataRow dr)
        {
            if (dr == null)
            {
                msgColor(Color.Red);
                msgText("Can't find underlying data row in data table");
                return;
            }
            try
            {
                dr.Delete();
                // Only update this one row
                DataRow[] drArray = new DataRow[1];
                drArray[0] = dr;
                MsSql.currentDA.Update(drArray);
            }
            catch (Exception ex)
            {
                msgColor(Color.Red);
                msgText(ex.Message);
                Console.Beep();

            }
        }

        private void SetSelectedCellsColor()
        {
            foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
            {
                DataGridViewCellStyle csReverse = new DataGridViewCellStyle();
                csReverse.SelectionBackColor = dataGridView1.DefaultCellStyle.SelectionForeColor;
                csReverse.SelectionForeColor = dataGridView1.DefaultCellStyle.SelectionBackColor;
                DataGridViewCellStyle csDefault = new DataGridViewCellStyle();
                csDefault.SelectionBackColor = dataGridView1.DefaultCellStyle.SelectionBackColor;
                csDefault.SelectionForeColor = dataGridView1.DefaultCellStyle.SelectionForeColor;
                if (cell.ReadOnly == false)
                {
                    cell.Style = csReverse;
                }
                else
                {
                    cell.Style = csDefault;
                }
            }
        }

        private void msgColor(Color color)
        {
            txtMessages.ForeColor = color;
            toolStripMsg.ForeColor = color;
        }

        private void msgText(string text)
        {
            string msg = MultiLingual.tr(text, this);
            txtMessages.Text = msg;
            toolStripMsg.Text = msg;
        }

        private void msgTextAdd(string text)
        {
            string msg = MultiLingual.tr(text, this);
            txtMessages.Text += msg;
            toolStripMsg.Text += msg;
        }

        // Add a "0" to i if it is less than 10.
        private string fileNumber(int i)
        {
            if (i < 10)
            {
                return "0" + i.ToString();
            }
            else
            {
                return i.ToString();
            }
        }

        private bool InAnEditingMode()
        {
            if (programMode == ProgramMode.edit || programMode == ProgramMode.add)
            { return true; }
            return false;
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------

        #region Debugging functions

        private void msgDebug(string text)
        {
            if (formOptions.debugging)
            {
                string msg = MultiLingual.tr(text, this);
                txtMessages.Text += msg;
                toolStripMsg.Text += msg;
            }
        }

        // Not in use
        private void dataGridView1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", CeVed");
        }

        // Not in use
        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", CeVing[" + e.ColumnIndex.ToString() + "," + e.RowIndex.ToString() + "]");
        }

        // Not in use
        private void dataGridView1_Validated(object sender, EventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", GrVed");
        }

        // Not in use
        private void dataGridView1_Validating(object sender, CancelEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", GrVing");
        }

        // Not in use
        private void dataGridView1_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", CeLeave");
        }
        // Not in use    
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", ClickCell");
        }

        // Not in Use
        private void dataGridView1_Enter(object sender, EventArgs e)
        {
            if (programMode == ProgramMode.edit) msgDebug(", EnterGrid");
        }





        #endregion

    }
}


