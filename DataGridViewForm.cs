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
        
        internal bool debugging = false;
        internal bool runTimer = false;

        internal bool updating = false; //updating means changes is cmbFkFilter or cmbText are by me not the user

        internal ProgramMode programMode = ProgramMode.none;
        internal bool FkColsDataDirty = false;
        internal bool mySql = false, msSql = false;  // currently only using msSql
        internal sqlFactory? currentSql = null;  //builds the sql string via .myInnerJoins, .myFields, .myWheres, my.OrderBys
        internal sqlFactory? comboSql = null;  //Always used "just in time" to fill combo, then destroyed
        private int pageSize = 0;
        private string logFileName = "";
        internal bool fixingDatabase = false;   // Used when fixing keys on database
        internal string strFixDatabaseWhereCondition = String.Empty; 
        private bool readOnly = false;
        private where? mainFilter;   //Stores main filter
        internal BindingList<where>? pastFilters;  // Use this to fill in past main filters in combo box
        private FileStream? ts;
        private Color[] ColorArray = new Color[] {Color.LightCyan, Color.LightGreen,Color.LavenderBlush,Color.SeaShell, Color.AliceBlue, Color.Azure,Color.LightGray,Color.LightSalmon };
        private Color DisplayKeyColor= Color.MidnightBlue;

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        #region Entire Form constructor, events, and methods only used in these

        internal DataGridViewForm()
        {
            //Required by windows forms
            InitializeComponent();
        }

        private void DataGridViewForm_Load(object sender, EventArgs e)
        {
            updating = true; // following calls cmbMainFilter changeindex event. "Updating" cancels that event.

            // Set things that don't change even if connection changes
            // 0.  Main filter datasource and 'last' element never changes
            where wh = new where("none", "none", "0", DbType.Int32, 4);
            wh.displayValue = "Select Research object";
            pastFilters = new BindingList<where>();
            pastFilters.Add(wh);
            cmbMainFilter.DisplayMember = "displayValue";
            cmbMainFilter.ValueMember = "whereValue";
            cmbMainFilter.DataSource = pastFilters;
            lblMainFilter.Text = "Research:";
            cmbMainFilter.Enabled = false;  // Enable when anything added and leave enabled

            // 1. Set language
            // 2. Set pageSize
            pageSize = 200;
            string strPageSize = AppData.GetKeyValue("RPP");  // If not set, this will return string.Empty
            int newPageSize = 0;
            if (int.TryParse(strPageSize, out newPageSize))
            {
                if (newPageSize > 9)  // Don't allow less than 10
                {
                    pageSize = newPageSize;
                }
            }
            txtRecordsPerPage.Text = pageSize.ToString();

            // 3. Menu options from last save
            // 4. Set font size
            SetControlsOnFormLoad();

            // 5. Load Database List (in files menu)
            load_mnuDatabaseList();

            // 6. Open Log file
            // openLogFile(); //errors ignored

            // 7. Open last connection 
            string msg = openConnection();
            if (msg != string.Empty) { msgText(msg); txtMessages.ForeColor = Color.Red; }

            // 8. Build English database - will do nothing if Boolean BuildingUpEnglishDatabase in MultiLingual.cs set to false
            MultiLingual.InsertEnglishIntoDatabase(this);

            updating = false;
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

        private void SetControlsOnFormLoad()
        {
            // Translations
            lblGridFilter.Text = MultiLingual.tr("Grid Filters:", this);
            lblComboFilter.Text = MultiLingual.tr("Key Combo Filters:", this);


            // Control arrays - can't make array in design mode in .net

            ComboBox[] cmbColumnFilterFields = { cmbColumnFilterFields_0, cmbColumnFilterFields_1, cmbColumnFilterFields_2 };
            TextBox[] txtColumnFilter = { txtColumnFilter_0, txtColumnFilter_1, txtColumnFilter_2 };

            ComboBox[] cmbComboFilterFields = { cmbComboFilterFields_0, cmbComboFilterFields_1, cmbComboFilterFields_2, cmbComboFilterFields_3, cmbComboFilterFields_4, cmbComboFilterFields_5 };
            ComboBox[]  cmbComboFilter = { cmbComboFilter_0, cmbComboFilter_1, cmbComboFilter_2, cmbComboFilter_3, cmbComboFilter_4, cmbComboFilter_5 };
            RadioButton[] radioButtons = { rbView, rbEdit, rbDelete, rbAdd, rbMerge };

            //Get size from registry and define "font"
            int size = 8;  // default
            try { size = Convert.ToInt32(Interaction.GetSetting("AccessFreeData", "Options", "FontSize", "9")); }catch { }
            System.Drawing.Font font = new System.Drawing.Font("Arial", size, FontStyle.Regular);

            // Set font of all single controls - 
            dataGridView1.Font = font;
            lblMainFilter.Font = font;
            cmbMainFilter.Font = font; 
            // Set font of filters - 
            for (int i = 0; i <= cmbColumnFilterFields.Count() - 1; i++) {
                cmbComboFilter[i].Font = font;
                cmbColumnFilterFields[i].Font = font;
                cmbColumnFilterFields[i].AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbColumnFilterFields[i].AutoCompleteMode = AutoCompleteMode.None;
                txtColumnFilter[i].Font = font;
            }
            for (int i = 0; i <= cmbComboFilterFields.Count() - 1; i++)
            {
                cmbComboFilter[i].Font = font;
                cmbComboFilter[i].Font = font;
                cmbComboFilterFields[i].Font = font;
                cmbComboFilterFields[i].AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbComboFilterFields[i].AutoCompleteMode = AutoCompleteMode.None;
            }

            for (int i = 0; i <= radioButtons.Count() - 1; i++) { radioButtons[i].Font = font; }
            cmbEditColumn.Font = font;
            cmbEditColumn.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbEditColumn.AutoCompleteMode = AutoCompleteMode.None;
            txtRecordsPerPage.Font = font;
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
                logFileName = fileCurrentName;
                FileStream tempAuxVar2 = new FileStream(logFileName, FileMode.Create);
                ts = new FileStream(logFileName, FileMode.OpenOrCreate, FileAccess.Write);
                tsWriter = new StreamWriter(ts);
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
                tsWriter = new StreamWriter(ts);
                tsWriter.WriteLine("</d" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateAndTime.Day(DateTime.Now).ToString() + ">");
                tsWriter.WriteLine("</sessions>");
                tsWriter.Close();
                ts = null;
            }
            catch (Exception exc)
            {
                // NotUpgradedHelper.NotifyNotUpgradedElement("Resume in On-Error-Resume-Next Block");
            }
        }

        #endregion
        //----------------------------------------------------------------------------------------------------------------------
    
        //----------------------------------------------------------------------------------------------------------------------
        #region Opening and closing Connection methods - open and close and related functions
        private string openConnection()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                // 1. Close old connection if any
                closeConnectionAndIntializeForm();

                // 2. Get connection string
                msSql = true;

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
                    readOnly = csObject.readOnly;

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
                    programMode = ProgramMode.none;  // Sets panel height to 2.
                    msgText("Recommend: First select object to research.  Example: choose student from student table (or whatever), right click, select 'Find / research object in higher tables'.");
                    msgColor(Color.Navy);
                }
            }
            catch (System.Exception excep)
            {
                sb.AppendLine("Error opening connection.");
                MessageBox.Show("Error opening the connection. " + Environment.NewLine + excep.Message + Environment.NewLine + Information.Err().Number.ToString());
                closeConnectionAndIntializeForm();
            }
            return sb.ToString();
        }

        private void disableAllFilters(bool includeMain)
        {
            // Reset main filter - first element added on form load and never removed
            if (includeMain) 
            { 
                while (pastFilters.Count > 1)
                {
                    pastFilters.RemoveAt(pastFilters.Count - 2);
                }
            }
            // Disable edit column combo
            cmbEditColumn.Enabled = false;
            // Disable Grid filters
            ComboBox[] cmbColumnFilterFields = { cmbColumnFilterFields_0, cmbColumnFilterFields_1, cmbColumnFilterFields_2 };
            TextBox[] txtColumnFilter = { txtColumnFilter_0, txtColumnFilter_1, txtColumnFilter_2 };
            for (int i = 0; i <= cmbColumnFilterFields.Count() - 1; i++)
            {
                cmbColumnFilterFields[i].Visible = true;
                cmbColumnFilterFields[i].Enabled = false;
                cmbColumnFilterFields[i].DataSource = null;  // Clear datasource
                txtColumnFilter[i].Visible = true;
                txtColumnFilter[i].Text = string.Empty;
                txtColumnFilter[i].Enabled = false;
                txtColumnFilter[i].PlaceholderText = "Filter Value";
            }
            // Disable the combo filters
            ComboBox[] cmbComboFilterFields = { cmbComboFilterFields_0, cmbComboFilterFields_1, cmbComboFilterFields_2, cmbComboFilterFields_3, cmbComboFilterFields_4, cmbComboFilterFields_5 };
            ComboBox[] cmbComboFilter = { cmbComboFilter_0, cmbComboFilter_1, cmbComboFilter_2, cmbComboFilter_3, cmbComboFilter_4, cmbComboFilter_5 };
            for (int i = 0; i <= cmbComboFilterFields.Count() - 1; i++)
            {
                cmbComboFilterFields[i].Visible = true;
                cmbComboFilterFields[i].Enabled = false;
                cmbComboFilterFields[i].DataSource = null;  // Clear datasource
                cmbComboFilter[i].Visible = true;
                cmbComboFilter[i].Enabled = false; 
                cmbComboFilter[i].DataSource = null;
            }

            SetTableLayoutPanelHeight();
        }

         private void closeConnectionAndIntializeForm()
        {
            MsSql.CloseConnectionAndDataAdapters();
            dataHelper.clearDataTables();
            disableAllFilters(true);

            // Clear other old values
            currentSql = null;
            comboSql = null;
            readOnly = false;
            msSql = false;
            FkColsDataDirty = false;
            fixingDatabase = false;

            // Delete the old mnuOpentable members
            if (mnuOpenTables.DropDownItems != null)
            {
                mnuOpenTables.DropDownItems.Clear();
            }
            // Hide special menus
            mnuTranscript.Available = false;
            mnuAddressBook.Available = false;

            programMode = ProgramMode.none;
        }

        #endregion
        //----------------------------------------------------------------------------------------------------------------------

        //----------------------------------------------------------------------------------------------------------------------
        #region Writing the Grid
        //----------------------------------------------------------------------------------------------------------------------
        // Write_Grid_NewTable called by mnuOpenTable_Click, mnuFather_Click, mnuSon_Click
        // Set initial state and things that don't change for this table here 
        private void writeGrid_NewTable(string table)
        {
            ComboBox[] cmbColumnFilterFields = { cmbColumnFilterFields_0, cmbColumnFilterFields_1, cmbColumnFilterFields_2 };
            ComboBox[] cmbComboFilterFields = { cmbComboFilterFields_0, cmbComboFilterFields_1, cmbComboFilterFields_2, cmbComboFilterFields_3, cmbComboFilterFields_4, cmbComboFilterFields_5 };
            ComboBox[] cmbComboFilter = { cmbComboFilter_0, cmbComboFilter_1, cmbComboFilter_2, cmbComboFilter_3, cmbComboFilter_4, cmbComboFilter_5 };
            var watch = new Stopwatch();

            updating = true;

            if(runTimer) Stopwatch.StartNew();

            //1. Reset variables and control properties;
            fixingDatabase = false;
            strFixDatabaseWhereCondition = String.Empty;
            programMode = ProgramMode.view;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Yellow;
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.Yellow;

            disableAllFilters(false);

            rbView.Checked = true; // Will show the first two rows

            //2. Create currentSql - same currentSql used until new table is loaded
            currentSql = new sqlFactory(table, 1, pageSize);
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

            //3. Fill edit columns combo (only 1)
            BindingList<field> editFields = new BindingList<field>();
            field editField1 = new field("_none", "_none", DbType.Int32, 4); // "_none", "_none" used in code to spot pseudo field
            editField1.DisplayMember = MultiLingual.tr("Select Column", this);
            editFields.Add(editField1);
            DataRow[] fieldsInTable = dataHelper.fieldsDT.Select(String.Format("TableName = '{0}' AND is_PK = 'false' AND is_DK = 'false'", currentSql.myTable));
            foreach (DataRow dr in fieldsInTable)
            {
                field drField = dataHelper.getFieldFromFieldsDT(dr);
                drField.DisplayMember = MultiLingual.tr(drField.DisplayMember, this);
                editFields.Add(drField);
            }
            //bind to drop down
            cmbEditColumn.DisplayMember = "DisplayMember";
            cmbEditColumn.ValueMember = "ValueMember";
            cmbEditColumn.DataSource = editFields;
            cmbEditColumn.Enabled = false;  // Make true when choosing "edit" mode

            //4. Bind the 3 cmbColumnFilterFields with fields for user to select to filter grid
            BindingList<field> filterFields = new BindingList<field>();
            field noFilter = new field("_none", "_none", DbType.Int32, 4); // "_none", "_none" used in code to spot pseudo field
            noFilter.DisplayMember = MultiLingual.tr("Filter Grid",this);
            filterFields.Add(noFilter);
            foreach (field fl in currentSql.myFields)
            {
                filterFields.Add(fl);
                fl.DisplayMember = MultiLingual.tr(fl.DisplayMember, this);
            }
            for (int i = 0; i < cmbColumnFilterFields.Length; i++)
            {
                cmbColumnFilterFields[i].BindingContext = new BindingContext();  // Required to change 1 by 1.
                cmbColumnFilterFields[i].DisplayMember = "DisplayMember";
                cmbColumnFilterFields[i].ValueMember = "ValueMember";  //Entire field
                cmbColumnFilterFields[i].DataSource = filterFields;
                cmbColumnFilterFields[i].Enabled = true;  //Always visible so no need to make visible
            }

            //5. Bind the cmbComboFilterFields with fields for user to select to filter grid
            BindingList<field> comboFilterFieldsAll = new BindingList<field>();
            field cNoFilter2 = new field("_none", "_none", DbType.Int32, 4); // "_none", "_none" used in code to spot pseudo field
            cNoFilter2.DisplayMember = MultiLingual.tr("Filter Any Key Combo", this);
            comboFilterFieldsAll.Add(cNoFilter2);
            int intFilter = 0;
            // One filter for each foreign key
            foreach (string key in currentSql.DisplayFieldsDictionary.Keys)
            { 
                if(intFilter < cmbColumnFilterFields.Length)
                {
                    BindingList<field> comboFilterFields = new BindingList<field>();
                    field cNoFilter = new field("_none", "_none", DbType.Int32, 4); // "_none", "_none" used in code to spot pseudo field
                    cNoFilter.DisplayMember = String.Format(MultiLingual.tr("Filter {0} Combo",this),key);
                    comboFilterFields.Add(cNoFilter);
                    // Get sqlFactory for this Ref table
                    field fkField = dataHelper.getFieldFromFieldsDT(currentSql.myTable, key);
                    field refTablePK = dataHelper.getForeignKeyRefField(fkField);
                    comboSql = new sqlFactory(refTablePK.table, 0, 0);
                    // Add the fields for comboSql into comboFilterFields bindinglist
                    foreach(field fl in comboSql.myFields)
                    {
                        if (!dataHelper.fieldIsForeignKey(fl))
                        {
                            fl.DisplayMember = String.Format("{0}.{1}", fl.table, fl.fieldName);
                            comboFilterFieldsAll.Add(fl);
                            comboFilterFields.Add(fl);
                        }
                    }
                    cmbComboFilterFields[intFilter].BindingContext = new BindingContext();  // Required to change 1 by 1.
                    cmbComboFilterFields[intFilter].DisplayMember = "DisplayMember";
                    cmbComboFilterFields[intFilter].ValueMember = "ValueMember";  //Entire field
                    cmbComboFilterFields[intFilter].DataSource = comboFilterFields;
                    cmbComboFilterFields[intFilter].Enabled = false;  //Make visible in edit or add mode.
                    cmbComboFilter[intFilter].Enabled = false;  //Make visible in edit or add mode.
                    intFilter++; 
                }
            }
            // Fill left over filters with "all".
            while (intFilter < cmbComboFilterFields.Count())
            {
                cmbComboFilterFields[intFilter].BindingContext = new BindingContext();  // Required to change 1 by 1.
                cmbComboFilterFields[intFilter].DisplayMember = "DisplayMember";
                cmbComboFilterFields[intFilter].ValueMember = "ValueMember";  //Entire field
                cmbComboFilterFields[intFilter].DataSource = comboFilterFieldsAll;
                cmbComboFilterFields[intFilter].Enabled = false;  //Make visible in edit or add mode.
                cmbComboFilter[intFilter].Enabled = false;  //Make visible in edit or add mode.
                intFilter++;
            }

            updating = false;

            //7. WriteGrid - next step
            writeGrid_NewFilter();
            if (runTimer) { watch.Stop(); msgTextAdd(" " + watch.ElapsedMilliseconds.ToString() + " "); }
        }

        internal void writeGrid_NewFilter()
        {
            // Create the where clauses in sqlBuilder currentSql
            callSqlWheres();
            // Get record Count
            string strSql = currentSql.returnSql(command.count);
            currentSql.RecordCount = MsSql.GetRecordCount(strSql);

            // Fetch must have an order by clause - so I will add one on first column
            if (currentSql.myOrderBys.Count == 0)  //Should always be true at this point
            {
                orderBy ob = new orderBy(currentSql.myFields[0], System.Windows.Forms.SortOrder.Ascending);
                currentSql.myOrderBys.Add(ob);
            }
            writeGrid_NewPage();
        }

        internal void writeGrid_NewPage()
        {
            updating = true;
            // CENTRAL USE OF sqlCurrent IN PROGRAM
            string strSql = currentSql.returnSql(command.select);
            if (fixingDatabase)
            {
                strSql = currentSql.returnFixDatabaseSql(strFixDatabaseWhereCondition);
            }

            //Clear the grid 
            if (dataGridView1.DataSource != null)
            {
                dataGridView1.DataSource = null;  // Deleting this results in columns not always in right order
            }
            dataGridView1.Columns.Clear();  // Deleting this results in FK fields not being colored
            dataGridView1.AutoGenerateColumns = true;

            // Bind database
            dataHelper.currentDT = new DataTable();
            MsSql.FillDataTable(dataHelper.currentDT, strSql);
            dataGridView1.DataSource = dataHelper.currentDT;

            dataGridView1.AutoGenerateColumns= false;

            // Replace foreign key colums with FkComboColumn
            if (currentSql != null)
            {
                int intCols = dataGridView1.ColumnCount;
                for (int i = 0; i < intCols; i++)
                {
                    DataGridViewColumn dgvCol = dataGridView1.Columns[i];
                    if (currentSql.DisplayFieldsDictionary.Keys.Contains(dgvCol.Name))
                    {
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
            }

            //Format controls
            SetTableLayoutPanelHeight();

            // Set toolStripButton3 caption
            toolStripButton3.Text = currentSql.myPage.ToString() + "/" + currentSql.TotalPages.ToString();

            //Set form caption
            string dbPath = Interaction.GetSetting("AccessFreeData", "DatabaseList", "path0", "");
            StringBuilder sb = new StringBuilder(dbPath.Substring(dbPath.LastIndexOf("\\") + 1));
            sb.Append(" - " + currentSql.myTable);
            sb.Append(" -  " + currentSql.RecordCount.ToString() + " rows");
            sb.Append(", Page: " + currentSql.myPage.ToString());
            this.Text = sb.ToString();

            //Set column widths
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

                if (currentSql.myOrderBys.Count > 0)
                {
                    field fldob = currentSql.myOrderBys[0].fld;
                    int gridColumn = currentSql.myFields.FindIndex(x => x == fldob);  // gridColumn index = myFields index
                    System.Windows.Forms.SortOrder sortOrder = currentSql.myOrderBys[0].sortOrder;
                    dataGridView1.Columns[gridColumn].SortMode = DataGridViewColumnSortMode.Programmatic;
                    dataGridView1.Columns[gridColumn].HeaderCell.SortGlyphDirection = sortOrder;
                }
            }

            // Color headers
            int keyIndex = 0;
            foreach (string key in currentSql.DisplayFieldsDictionary.Keys)
            {
                int colIndex = currentSql.myFields.FindIndex(x => x.fieldName == key);
                dataGridView1.Columns[colIndex].HeaderCell.Style.BackColor = ColorArray[keyIndex];
                dataGridView1.Columns[colIndex].HeaderCell.Style.SelectionBackColor = ColorArray[keyIndex];
                foreach (field fl in currentSql.DisplayFieldsDictionary[key])
                {
                    colIndex = currentSql.myFields.FindIndex(x => x == fl);
                    dataGridView1.Columns[colIndex].HeaderCell.Style.BackColor = ColorArray[keyIndex];
                    dataGridView1.Columns[colIndex].HeaderCell.Style.SelectionBackColor = ColorArray[keyIndex];
                }
                keyIndex = keyIndex + 1;
            }

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.ReadOnly = true;
            }
            if (programMode == ProgramMode.edit)
            {
                cmbEditColumn_SelectedIndexChangeContent();
            }

            updating = false;
        }

        private void callSqlWheres()
        {   // Adds all the filters - FK, DK, non-Key and MainFilter
            TextBox[] txtColumnFilter = { txtColumnFilter_0, txtColumnFilter_1, txtColumnFilter_2 };
            ComboBox[] cmbColumnFilterFields = {cmbColumnFilterFields_0, cmbColumnFilterFields_1, cmbColumnFilterFields_2 };

            // 1. Clear any old filters from currentSql
            currentSql.myWheres.Clear();

            // 2. Main filter - add this where to the currentSql)
            if (mainFilter != null)
            {
                if (Convert.ToInt32(mainFilter.whereValue) > 0)
                {
                    // Check that the table is in the myFields
                    if (currentSql.TableIsInMyTables(mainFilter.fl.table))
                    {
                        currentSql.myWheres.Add(mainFilter);
                    }
                }
            }
            // 3. cmbColumnFilterFields - Currently 3. ? to add these when program mode is add; easy to add this here.
            for (int i = 0; i < txtColumnFilter.Length; i++)
            {
                if (txtColumnFilter[i].Enabled)
                {
                    if (txtColumnFilter[i].Text != string.Empty) // Will be empty if pseudo item choosen in dropdown
                    {
                        field selectedField = (field)cmbColumnFilterFields[i].SelectedItem;
                        if(selectedField.fieldName != "none")  //Just to be sure
                        { 
                            where wh = new where(selectedField, txtColumnFilter[i].Text);
                            currentSql.myWheres.Add(wh);
                        }
                    }
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
            fixingDatabase = true;
            strFixDatabaseWhereCondition = whereCondition;
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
                    if(cs.comboString == returnString)
                    {
                        csList.Remove(cs);
                        break;   // Only remove the first one - should never be more than 1
                    }
                }
                AppData.storeConnectionStringList(csList);
            }
        }

        private void GridContextMenu_FindInHigherTable_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // 0. Get the table they want to go to.

                // 1. Define mainFilter (Type: where).
                // 1a. This assumes the first column in row is the Primary Key and it is an integer
                string value = dataGridView1.SelectedRows[0].Cells[0].Value.ToString(); //Integer
                // 2b. Set DisplayValue to string with all non-empty cells from selected row (may shorten later)                
                List<String> displayValueList = new List<String>();
                foreach (DataGridViewCell cell in dataGridView1.SelectedRows[0].Cells)
                {
                    if (cell.Value != null)
                    {
                        if (!String.IsNullOrEmpty(cell.Value.ToString()))
                        {
                            field fi = currentSql.myFields[cell.ColumnIndex];
                            if (dataHelper.isDisplayKey(fi.table, fi.fieldName))
                            {
                                displayValueList.Add(Convert.ToString(cell.Value));
                            }
                        }
                    }
                }
                //1c. Get mainfilter
                string displayValue = String.Join(", ", displayValueList);
                mainFilter = new where(currentSql.myFields[0].table, currentSql.myFields[0].fieldName, value, DbType.Int32, 4);
                mainFilter.displayValue = displayValue; // WhereValue set in constructor
                //1d. Update pastFilters list
                pastFilters.Insert(0, mainFilter);
                cmbMainFilter.SelectedIndex = 0;
            }
        }

        private void cmbMainFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!updating)
            {
                int i = cmbMainFilter.SelectedIndex;
                if (i != -1)  // Always true (I suspect)
                {
                    //Last element means "no filter".
                    if (cmbMainFilter.SelectedIndex == cmbMainFilter.Items.Count - 1)
                    {
                        mainFilter = null;
                        lblMainFilter.Text = "Research: ";
                    }
                    else
                    {
                        mainFilter = pastFilters[i];   // pastFilters is a "where" list that is used to bind cmbMainFilter
                        lblMainFilter.Text = "Object: " + mainFilter.fl.table;
                    }
                }
                cmbMainFilter.Enabled = true;
                writeGrid_NewFilter();
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
            string msg = openConnection();
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
                string msg = openConnection();
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

        // Update datatable in FkComboEditingControl
        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
    {
        if (FkColsDataDirty)
        {
            DataGridViewColumn col = dataGridView1.Columns[e.ColumnIndex];
            FkComboColumn Fkcol = col as FkComboColumn;
            if (Fkcol != null)  // Will be null for non-foreign key row
            {
                field fkField = new field(currentSql.myTable, col.Name, DbType.Int32, 4);
                field FkTablePKField = dataHelper.getForeignKeyRefField(fkField);
                comboSql = new sqlFactory(FkTablePKField.table, 0, 0);
                // Assign datatable to each cell in FK column
                // Filter combo dataTable here
                callSqlWheresForCombo(0);
                string strSql = comboSql.returnComboSql(FkTablePKField);

                dataHelper.extraDT = new DataTable();
                MsSql.FillDataTable(dataHelper.extraDT, strSql);
                int index = dataHelper.currentDT.Columns.IndexOf(col.Name);
                for (int j = 0; j < dataHelper.currentDT.Rows.Count; j++)
                {
                    FkComboCell fkCell = (FkComboCell)dataGridView1.Rows[j].Cells[index];
                    fkCell.dataTable = dataHelper.extraDT;
                }
                FkColsDataDirty = false;
            }
        }
    }


    // When foreign key selected in dropdown, this puts the selectedvalue into the cell.
    private void dataGridView1_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
    {
        // Note: CellParsing accepts user input and map it to a different cell value / format.
        if (programMode == ProgramMode.edit) msgDebug(", CPa");
        if(dataGridView1.EditingControl != null)  // Always true
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
        if(programMode == ProgramMode.edit) msgDebug(", GrVed");
    }
        
    // Not in use
    private void dataGridView1_Validating(object sender, CancelEventArgs e)
    {
        if (programMode == ProgramMode.edit) msgDebug(", GrVing");
    }
        
    // Not in use
    private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
    {
        if (programMode == ProgramMode.edit) msgDebug(",CeDirty:" + dataGridView1.IsCurrentCellDirty.ToString());
    }
        
    // Push the change in the cell value down to the database
    private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {   
        if (programMode == ProgramMode.edit) msgDebug(", CVC");
        if (!updating)
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
                    if(fkCol != null) 
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

    // Not in use
    private void dataGridView1_CellLeave(object sender, DataGridViewCellEventArgs e)
    {   
        if (programMode == ProgramMode.edit) msgDebug(", CeLeave");
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

            // Not in use
            ctl.SelectedIndexChanged -= new EventHandler(cmbFkCombo_SelectedIndexChanged);
            ctl.SelectedIndexChanged += new EventHandler(cmbFkCombo_SelectedIndexChanged);
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
                foreach(DataGridViewCell cell in dataGridView1.SelectedCells)
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
        //----------------------------------------------------------------------------------------------------------------------
 
        //----------------------------------------------------------------------------------------------------------------------
        #region Events - Non-Datagrid Controls
        // Not in use
        private void cmbFkCombo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void cmbEditColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!updating)
            { 
                cmbEditColumn_SelectedIndexChangeContent();
            }
        }
        private void cmbEditColumn_SelectedIndexChangeContent()
        {
         
            if (cmbEditColumn.SelectedIndex > 0)  // 0 is the "Select column" choice
            {
                string strSelectedColumn = ((field)cmbEditColumn.SelectedValue).fieldName.ToString();
                DataGridViewColumn selectedColumn = dataGridView1.Columns[strSelectedColumn];
                if (selectedColumn != null)
                {
                    foreach (DataGridViewColumn col in dataGridView1.Columns)
                    {
                        // Fill drop down combo for the selected foreign key row
                        if (col == selectedColumn) 
                        { 
                            col.ReadOnly = false;
                            FkColsDataDirty = true;

                        }
                        else { col.ReadOnly = true; }
                    }
                }
                int colIndex = dataGridView1.Columns[strSelectedColumn].Index;
                field fld = currentSql.myFields[colIndex];  // Trusting this is the same index as above
                MsSql.SetUpdateCommand(fld, dataHelper.currentDT);
            }
            else
            {
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    col.ReadOnly = true;
                }
                cmbComboFilterFields_0.Enabled = false;
            }
            SetTableLayoutPanelHeight();
        }

        private DataTable GetDataTableForDropDownEditingControl(field fkField)
        { 
            DataTable dataTable = new DataTable();


            return dataTable;
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
                    pageSize = rpp;
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
                cmbEditColumn.Enabled = false;
                btnDeleteAddMerge.Enabled = false;
                EnableComboFilters(false);
            }
        }
        private void rbDelete_CheckedChanged(object sender, EventArgs e)
        {
            if (rbDelete.Checked)
            {
                programMode = ProgramMode.delete;
                dataGridView1.MultiSelect = false;
                dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
                cmbEditColumn.Enabled = false;
                btnDeleteAddMerge.Enabled = true;
                btnDeleteAddMerge.Text = MultiLingual.tr("Delete row", this);
                EnableComboFilters(false);
                // Add deleteCommand
                MsSql.SetDeleteCommand(currentSql.myTable, dataHelper.currentDT);
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.ReadOnly = true;
                }
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
                EnableComboFilters(true);
                cmbEditColumn.Enabled = true;
                btnDeleteAddMerge.Enabled = false;
            }
        }

        private void rbAdd_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAdd.Checked)
            {
                programMode = ProgramMode.add;
                dataGridView1.MultiSelect = false;
                dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
                cmbEditColumn.Enabled = false;
                btnDeleteAddMerge.Enabled = true;
                btnDeleteAddMerge.Text = MultiLingual.tr("Add row", this);
                EnableComboFilters(true);
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.ReadOnly = true;
                }

                //// Create the InsertCommand.
                //command = new SqlCommand(
                //    "INSERT INTO Customers (CustomerID, CompanyName) " +
                //    "VALUES (@CustomerID, @CompanyName)", connection);

                //// Add the parameters for the InsertCommand.
                //command.Parameters.Add("@CustomerID", SqlDbType.NChar, 5, "CustomerID");
                //command.Parameters.Add("@CompanyName", SqlDbType.NVarChar, 40, "CompanyName");

                //adapter.InsertCommand = command;



                //// Get display rows and put in cellFilters
                //TextBox[] txtDkFilter = { txtDkFilter_0, txtDkFilter_1, txtDkFilter_2, txtDkFilter_3, txtDkFilter_4, txtDkFilter_5, txtDkFilter_6 };
                //TextBox[] txtCmbColumnFilter = { txtCmbColumnFilter_0 };
                //ComboBox[] cmbColumnFilterFields = { cmbColumnFilterFields_0};
                //int cellFilter = 0;
                //foreach (DataGridViewColumn col in dataGridView1.Columns)
                //{
                //    field fi = currentSql.myFields[col.Index];
                //    if (fi.table == currentSql.myTable && dataHelper.isDisplayKey(fi.table, fi.fieldName))
                //    {
                //        if (!dataHelper.fieldIsForeignKey(fi) && !dataHelper.isTablePrimaryKeyField(fi))
                //        {
                //            updating = true;
                //            cmbColumnFilterFields[cellFilter].Enabled = true;
                //            string strValueMember = cmbColumnFilterFields[cellFilter].ValueMember;
                //            string filterExpression = String.Format("{0} = '{1}'",strValueMember,fi.fieldName);
                //            DataTable dt = (DataTable)cmbColumnFilterFields[cellFilter].DataSource;
                //            DataRow dr = dt.Select(filterExpression).First(); 
                //            cmbColumnFilterFields[cellFilter].SelectedValue = dr;
                //            // cmbColumnFilterFields[cellFilter].Text = fi.fieldName;  // Set value ??? or this is enough?
                //            txtDkFilters[cellFilter].Enabled = true;
                //            txtDkFilters[cellFilter].Text = String.Empty;  // Or set to current row?

                //            cellFilter = cellFilter + 1;
                //        }
                //    }
                //}
                //// Disable the rest
                //while (cellFilter < cmbColumnFilterFields.Length)
                //{
                //    cmbColumnFilterFields[cellFilter].Enabled = false;
                //    txtDkFilters[cellFilter].Enabled = false;
                //    txtDkFilters[cellFilter].Text = String.Empty;
                //    cellFilter= cellFilter + 1; 
                //}
                //SetTableLayoutPanelHeight();
            }
        }

        private void rbMerge_CheckedChanged(object sender, EventArgs e)
        {
            if (rbMerge.Checked) 
            { 
                programMode = ProgramMode.merge;
                cmbEditColumn.Enabled = false;
                btnDeleteAddMerge.Enabled = true;
                btnDeleteAddMerge.Text = MultiLingual.tr("Merge 2 rows", this);
                dataGridView1.MultiSelect = true;
                EnableComboFilters(false);
                MsSql.SetDeleteCommand(currentSql.myTable, dataHelper.currentDT);
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.ReadOnly = true;
                }
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
                        "Then we will delete the row {1} from this table.  Do you want to continue?",firstPKCount, firstPK, secondPK);
                    DialogResult reply = MessageBox.Show(msg, "Merge two rows?", MessageBoxButtons.YesNo);
                    if (reply == DialogResult.Yes)
                    {
                        foreach (DataRow dr in drs)
                        {
                            string FKColumnName = dr.ItemArray[dr.Table.Columns.IndexOf("ColumnName")].ToString();
                            string TableWithFK = dr.ItemArray[dr.Table.Columns.IndexOf("TableName")].ToString();
                            field fld = new field(TableWithFK,FKColumnName, DbType.Int32,4);
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
                DataRow dr = dataHelper.currentDT.Select(string.Format("{0} = {1}", PKfield.fieldName,PKvalue)).FirstOrDefault();
                DeleteOneDataRowFromCurrentDT(dr);
            }
        }
         private void txtColumnFilter_TextChanged(object sender, EventArgs e)
        {
            if (updating == false)
            {
                writeGrid_NewFilter();
            }
        }

        public void cmbColumnFilterFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextBox[] txtColumnFilter = { txtColumnFilter_0, txtColumnFilter_1, txtColumnFilter_2 };
            ComboBox[] cmbColumnFilterFields = {cmbColumnFilterFields_0, cmbColumnFilterFields_1, cmbColumnFilterFields_2 };
            if (!updating)
            { // Clear txtColumnFilter which will call "WriteGrid_NewFilter"
                ComboBox cmb = (ComboBox)sender;
                for (int i = 0; i < cmbColumnFilterFields.Count(); i++)
                {
                    if (cmb == cmbColumnFilterFields[i])
                    {
                        txtColumnFilter[i].Text = string.Empty;
                        if (cmb.SelectedIndex < 1)
                        {
                            txtColumnFilter[i].Enabled = false;
                        }
                        else
                        {
                            txtColumnFilter[i].Enabled = true;
                        }
                    }
                }
            }
        }

        public void cmbComboFilterFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox[] cmbComboFilterFields = { cmbComboFilterFields_0, cmbComboFilterFields_1, cmbComboFilterFields_2, cmbComboFilterFields_3, cmbComboFilterFields_4, cmbComboFilterFields_5 };
            ComboBox[] cmbComboFilter = { cmbComboFilter_0, cmbComboFilter_1, cmbComboFilter_2, cmbComboFilter_3, cmbComboFilter_4, cmbComboFilter_5 };

            if (!updating)
            {
                FkColsDataDirty = true;
                ComboBox cmb = (ComboBox)sender;
                for (int i = 0; i < cmbComboFilterFields.Count(); i++)
                {
                    // Get combo box
                    if (cmb == cmbComboFilterFields[i])
                    {
                        updating = true;
                        cmbComboFilter[i].DataSource = null;
                        cmbComboFilter[i].Items.Clear();
                        if (cmb.SelectedIndex < 1)
                        {
                            cmbComboFilter[i].Enabled = false;
                        }
                        else
                        {
                            cmbComboFilter[i].Enabled = true;
                            // Fill cmbComboFilter[i]
                            field selectedField = (field)cmbComboFilterFields[i].SelectedValue;
                            if (dataHelper.isTablePrimaryKeyField(selectedField))
                            {
                                cmbComboFilter[i].DropDownStyle = ComboBoxStyle.DropDownList;
                            }
                            else
                            {
                                cmbComboFilter[i].DropDownStyle = ComboBoxStyle.DropDown;
                            }
                            comboSql = new sqlFactory(selectedField.table, 0, 0);
                            callSqlWheresForCombo(i+1);
                            string strSql = comboSql.returnComboSql(selectedField);
                            dataHelper.extraDT = new DataTable();
                            MsSql.FillDataTable(dataHelper.extraDT, strSql);
                            cmbComboFilter[i].DisplayMember = "DisplayMember";
                            cmbComboFilter[i].ValueMember = "ValueMember";
                            cmbComboFilter[i].DataSource = dataHelper.extraDT;
                        }
                        updating = false;
                    }
                }
            }
        }

        // Adjusts the width of all the items shown when combobox dropped down to longest item
        private void AdjustWidthComboBox_DropDown(object sender, EventArgs e)
        {
            // All elements must have a "DisplayMember" field or property.

            // A. Get list of display strings in ComboBox
            var senderComboBox = (System.Windows.Forms.ComboBox)sender;
            if(senderComboBox.Items.Count > 0) { 
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
                    foreach (where wh in itemsList) { displayValueList.Add(wh.displayValue); }
                }

                // B. Get and set width
                int width = senderComboBox.DropDownWidth;
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
        private void SetTableLayoutPanelHeight()
        {
            int height = 2;
            if (programMode != ProgramMode.none)  // 3rd row unless programMode is none.
            {
                height = cmbColumnFilterFields_0.Top + cmbColumnFilterFields_0.Height + 2;
            }
            if (cmbColumnFilterFields_0.Enabled)  // 3rd row
            {
                height = cmbColumnFilterFields_0.Top + cmbColumnFilterFields_0.Height + 2;
            }
            if (cmbComboFilterFields_0.Enabled)  // 4th row
            {
                height = cmbComboFilterFields_0.Top + cmbComboFilterFields_0.Height + 2;
            }
            if (cmbComboFilterFields_3.Enabled)  // 5th row
            {
                height = cmbComboFilterFields_3.Top + cmbComboFilterFields_3.Height + 2;
            }

            tableLayoutPanel.Height = height;
            // Reposition the splitter
            if (splitContainer1.Width > 0)  // trying to catch error: "Splitterdistance must be between panel1minsize and width and pan2minsize.
            {
                this.splitContainer1.SplitterDistance = txtMessages.Height + tableLayoutPanel.Height;
            }
        }

        private void callSqlWheresForCombo(int beginIndex)
        {   // Adds all the filters - FK, DK, non-Key and MainFilter
            ComboBox[] cmbComboFilterFields = { cmbComboFilterFields_0, cmbComboFilterFields_1, cmbComboFilterFields_2, cmbComboFilterFields_3, cmbComboFilterFields_4, cmbComboFilterFields_5 };
            ComboBox[] cmbComboFilter = { cmbComboFilter_0, cmbComboFilter_1, cmbComboFilter_2, cmbComboFilter_3, cmbComboFilter_4, cmbComboFilter_5 };

            // Clear any old filters from currentSql
            comboSql.myWheres.Clear();

            //Main filter - add this where to the currentSql)
            if (mainFilter != null)
            {
                if (Convert.ToInt32(mainFilter.whereValue) > 0)
                {
                    // Check that the table and field is in the myFields
                    if (comboSql.TableIsInMyTables(mainFilter.fl.table))
                    {
                        comboSql.myWheres.Add(mainFilter);
                    }
                }
            }
            // cmbComboFilterFields  (6 fields)
            for (int i = beginIndex; i < cmbComboFilter.Length; i++)
            {
                if (cmbComboFilter[i].Enabled)  // False if first row of cmbComboFilterFields[i] is choosen
                {
                    field selectedField = (field)cmbComboFilterFields[i].SelectedValue;  // Get field from 1st combo of 2
                    if (comboSql.TableIsInMyTables(selectedField.table))
                    {
                        String filterText = String.Empty;
                        if (cmbComboFilter[i].SelectedValue == null)   // User entered value; not allowed in Primary Key field
                        {
                            filterText = cmbComboFilter[i].Text;
                        }
                        else
                        {
                            filterText = cmbComboFilter[i].SelectedValue.ToString();
                        }
                        where wh = new where(selectedField, filterText);
                        comboSql.myWheres.Add(wh);
                    }
                }
            }
        }

        private void EnableComboFilters(bool enable)
        {
            ComboBox[] cmbComboFilterFields = { cmbComboFilterFields_0, cmbComboFilterFields_1, cmbComboFilterFields_2, cmbComboFilterFields_3, cmbComboFilterFields_4, cmbComboFilterFields_5 };
            ComboBox[] cmbComboFilter = { cmbComboFilter_0, cmbComboFilter_1, cmbComboFilter_2, cmbComboFilter_3, cmbComboFilter_4, cmbComboFilter_5 };
            for (int i = 0; i <= cmbComboFilterFields.Count() - 1; i++)
            {
                cmbComboFilterFields[i].Enabled = enable;
                // cmbComboFilter[i].Enabled = enable;  // let user handle this
            }
            SetTableLayoutPanelHeight();
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
            catch(Exception ex)
            {
                msgColor(Color.Red);
                msgText(ex.Message);
                Console.Beep();

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

        private void msgDebug(string text)
        {
            if (debugging)
            { 
                string msg = MultiLingual.tr(text, this);
                txtMessages.Text += msg;
                toolStripMsg.Text += msg;
            }
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

        #endregion
        //----------------------------------------------------------------------------------------------------------------------
    }
}


