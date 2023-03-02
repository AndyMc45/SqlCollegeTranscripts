using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Windows.ApplicationModel.Email;
using Windows.Media.Playback;
using Windows.UI.Core.AnimationMetrics;
using Windows.UI.Popups;

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
        internal ProgramMode programMode = ProgramMode.view;
        internal bool mySql = false, msSql = false;  // currently only using msSql
        internal sqlFactory? currentSql = null;  //builds the sql string via .myInnerJoins, .myFields, .myWheres, my.OrderBys
        private int pageSize = 0;
        private string logFileName = "";
        internal bool updating = false; //updating means changes is cmbFilter or cmbText are by me not the user
        private bool readOnly = false;
        private where? mainFilter;   //Stores main filter
        internal BindingList<where>? pastFilters;  // Use this to fill in past main filters in combo box
        private FileStream? ts;
        private Color[] ColorArray = new Color[] {Color.LightCyan, Color.LightGreen,Color.LavenderBlush,Color.SeaShell, Color.AliceBlue, Color.Azure,Color.LightGray,Color.LightSalmon };
        DataGridViewCell oldCurrentCell;

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        #region Entire Form constructor, events, and methods only used in these
        //----------------------------------------------------------------------------------------------------------------------

        internal DataGridViewForm()
        {
            //Required by windows forms
            InitializeComponent();
        }

        private void DataGridViewForm_Load(object sender, EventArgs e)
        {
            // Set things that don't change even if connection changes

            // 0.  Main filter datasource and 'last' element never changes
            where wh = new where("none", "none", "0", DbType.Int32, 4);
            wh.displayValue = "Select Row";
            updating = true; // following calls _cmbMainFilter changeindex event. "Updating" cancels that event.
            pastFilters = new BindingList<where>();
            pastFilters.Add(wh);
            _cmbMainFilter.DisplayMember = "displayValue";
            _cmbMainFilter.ValueMember = "whereValue";
            _cmbMainFilter.DataSource = pastFilters;
            lblMainFilter.Text = "Row Filter:";
            updating = false;

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
            SetFontSizeEtcForAllControls();

            // 5. Load Database List (in files menu)
            load_mnuDatabaseList();

            // 6. Open Log file
            // openLogFile(); //errors ignored

            // 7. Open last connection 
            string msg = openConnection();
            if (msg != string.Empty) { txtMessages.Text = msg; }

            // 8. Build English database - will do nothing if Boolean BuildingUpEnglishDatabase in MultiLingual.cs set to false
            dataHelper.extraDT = new DataTable();  // extraDT used to insert English Into Database, and for other non-permenant things
            MultiLingual.InsertEnglishIntoDatabase(this);
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

        private void SetFontSizeEtcForAllControls()
        {
            // Control arrays - can't make array in design mode in .net
            Label[] lblcmbFilter = { _lblCmbFilter_0, _lblCmbFilter_1, _lblCmbFilter_2, _lblCmbFilter_3, _lblCmbFilter_4, _lblCmbFilter_5, _lblCmbFilter_6, _lblCmbFilter_7 };
            ComboBox[] cmbFilter = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7 };
            TextBox[] txtCellFilter = { txtCellFilter_1, txtCellFilter_2 };
            ComboBox[] cmbCellFields = { cmbCellFields_1, cmbCellFields_2 };
            RadioButton[] radioButtons = { rbView, rbEdit, rbDelete, rbAdd, rbMerge };

            //Get size from registry
            int size = 8;  // default
            try { size = Convert.ToInt32(Interaction.GetSetting("AccessFreeData", "Options", "FontSize", "9")); }
            catch { }
            // Set font
            System.Drawing.Font font = new System.Drawing.Font("Arial", size, FontStyle.Regular);
            // Set all controls - (count simply iterate over all controls!)
            dataGridView1.Font = font;
            lblMainFilter.Font = font;
            for (int i = 0; i <= lblcmbFilter.Count() - 1; i++) { lblcmbFilter[i].Font = font; }
            for (int i = 0; i <= cmbFilter.Count() - 1; i++) { 
                cmbFilter[i].Font = font; 
                cmbFilter[i].AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbFilter[i].AutoCompleteMode = AutoCompleteMode.Suggest;
            }
            for (int i = 0; i <= txtCellFilter.Count() - 1; i++) { txtCellFilter[i].Font = font; }
            for (int i = 0; i <= cmbCellFields.Count() - 1; i++) { 
                cmbCellFields[i].Font = font;
                cmbCellFields[i].Font = font;
                cmbCellFields[i].AutoCompleteSource = AutoCompleteSource.ListItems;
                cmbCellFields[i].AutoCompleteMode = AutoCompleteMode.Suggest;

            }
            for (int i = 0; i <= radioButtons.Count() - 1; i++) { radioButtons[i].Font = font; }
            cmbEditColumn.Font = font;
            cmbEditColumn.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbEditColumn.AutoCompleteMode = AutoCompleteMode.Suggest;
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
        #region Opening and closing Connection methods - open and close and related functions
        //----------------------------------------------------------------------------------------------------------------------
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

                    //6. Fill Information Datatables
                    MsSql.initializeDatabaseInformationTables();
                    
                    // 7.  Load Table mainmenu
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
                    programMode = ProgramMode.view;
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

        private void resetMainFilter()
        {
            // Remove all but the last element in pastFilters list (i.e. "Row Filter")
            while (pastFilters.Count > 1)
            {
                pastFilters.RemoveAt(pastFilters.Count - 2);
            }
        }

        private void resetComboAndCellFilters()
        {
            Label[] lblcmbFilter = { _lblCmbFilter_0, _lblCmbFilter_1, _lblCmbFilter_2, _lblCmbFilter_3, _lblCmbFilter_4, _lblCmbFilter_5, _lblCmbFilter_6, _lblCmbFilter_7 };
            ComboBox[] cmbFilter = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7 };
            ComboBox[] cmbCellFields = { cmbCellFields_1, cmbCellFields_2 };
            TextBox[] txtCellFilter = { txtCellFilter_1, txtCellFilter_2 };

            rbView.Checked = true;
            cmbEditColumn.Enabled = false;
            _cmbMainFilter.Enabled = false;  // Sets panel height to 2

            // Hide, disable and clear all the cmbCellFilters and cmbCellFields 
            for (int i = 0; i <= cmbCellFields.Count() - 1; i++)
            {
                txtCellFilter[i].Visible = true;
                txtCellFilter[i].Text = string.Empty;
                txtCellFilter[i].Enabled = false;
                txtCellFilter[i].PlaceholderText = "Column Filter";
                cmbCellFields[i].Visible = true;
                cmbCellFields[i].Enabled = false;
                cmbCellFields[i].DataSource = null;
            }
            // Disable all the cmbFilters and lblCmbFilters
            for (int i = 0; i <= lblcmbFilter.Count() - 1; i++)
            {
                lblcmbFilter[i].Text = "FK filter:";
                lblcmbFilter[i].Visible = true;
                cmbFilter[i].Visible = true;
                cmbFilter[i].Enabled = false;
                cmbFilter[i].DataSource = null;
                cmbFilter[i].Items.Clear();
            }
            // Set height of TableLayoutPanel - which will also move the splitContainer splitter.
            SetTableLayoutPanelHeight();
        }

        private void SetTableLayoutPanelHeight()
        {
            int height = 2;
            if (_cmbMainFilter.Enabled)  // 1st row
            {
                height = _cmbMainFilter.Top + _cmbMainFilter.Height + 2;
            }
            if (_cmbFilter_0.Enabled || rbEdit.Checked)  // 2nd row
            {
                height = _cmbFilter_0.Top + _cmbFilter_0.Height + 2;
            }
            if (_cmbFilter_3.Enabled)  // 3rd row
            {
                height = _cmbFilter_3.Top + _cmbFilter_3.Height + 2;
            }
            if (_cmbFilter_5.Enabled)  // 4th row
            {
                height = _cmbFilter_5.Top + _cmbFilter_5.Height + 2;
            }
            tableLayoutPanel.Height = height;
            // Reposition the splitter
            if (splitContainer1.Width > 0)  // trying to catch error: "Splitterdistance must be between panel1minsize and width and pan2minsize.
            { 
                this.splitContainer1.SplitterDistance = txtMessages.Height + tableLayoutPanel.Height;
            }
        }

        private void closeConnectionAndIntializeForm()
        {
            MsSql.CloseConnectionAndDataAdapters();
            // Set all dataHelper datatables to null
            dataHelper.clearDataTables();

            // Clear other old values
            currentSql = null;
            readOnly = false;
            msSql = false;

            // Hide and Delete the old mnuOpentable members
            if (mnuOpenTables.DropDownItems != null)
            {
                mnuOpenTables.DropDownItems.Clear();
            }
            // Hide special menus
            mnuTranscript.Available = false;
            mnuAddressBook.Available = false;

            // Format and hide various controls
            resetMainFilter();
            updating = true;
            resetComboAndCellFilters();
            updating = false;
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        #region Writing the Grid - upto New Page
        //----------------------------------------------------------------------------------------------------------------------
        // Write_Grid_NewTable called by mnuOpenTable_Click, mnuFather_Click, mnuSon_Click
        // Set initial state and things that don't change for this table here 
        private void writeGrid_NewTable(string table)
        {
            //          var watch = Stopwatch.StartNew();
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Yellow;
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.Yellow;
            toolStripMsg.Text = table;
            rbView.Checked = true;
            updating = true;
            resetComboAndCellFilters();

            // Create currentSql - same currentSql used until new table is loaded
            currentSql = new sqlFactory(table, 1, pageSize);

            //// Put Primary key of main table in the first field of myFields
            //string pk = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
            //field fi = new field(currentSql.myTable, pk, "int", 4);
            //currentSql.myFields.Add(fi);

            //// This sets currentSql table and field strings - and these remain the same for this table.
            //// This also sets DisplayFieldDicitionary each foreign table key in main table
            //string msg = currentSql.callInnerJoins();
            if (!String.IsNullOrEmpty(currentSql.errorMsg)) { toolStripBottom.Text = currentSql.errorMsg; }

            // Set up 8 cmbFilter labels and boxes - BUT don't bind them yet - 9 milliseconds
            _cmbMainFilter.Enabled = true;
            ComboBox[] cmbFilter = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7 };
            Label[] lblcmbFilter = { _lblCmbFilter_0, _lblCmbFilter_1, _lblCmbFilter_2, _lblCmbFilter_3, _lblCmbFilter_4, _lblCmbFilter_5, _lblCmbFilter_6, _lblCmbFilter_7 };
            int i = 0;
            // one combo box for each foreignKey
            foreach (string key in currentSql.DisplayFieldsDictionary.Keys)
            {
                field FK = dataHelper.getFieldValueFieldsDT(currentSql.myTable, key);
                field RefPK = dataHelper.getForeignKeyRefField(FK);
                lblcmbFilter[i].Text = RefPK.table; // Can be translated or changed
                cmbFilter[i].Enabled = true;
                cmbFilter[i].Tag = key;  // Used in program and so don't translate or change
                i++;
            }

            // Set up the 2 cmbCellFieldFilter "labels" - 8 milliseconds
            ComboBox[] cmbCellFields = { cmbCellFields_1, cmbCellFields_2 };
            for (int j = 0; j < cmbCellFields.Length; j++)
            {
                DataView viewFieldsDT = new DataView(dataHelper.fieldsDT);
                viewFieldsDT.RowFilter = String.Format("TableName = '{0}' AND is_PK = 'false' AND is_FK = 'false'", currentSql.myTable);
                DataTable dt2 = new DataTable();
                dt2.Columns.Add("DisplayName", Type.GetType("System.String"));
                dt2.Columns.Add("ColumnName", Type.GetType("System.String"));
                DataRow dataRow2 = dt2.NewRow();
                dataRow2[0] = "Select Column";
                dataRow2[1] = "0";
                dt2.Rows.Add(dataRow2);
                foreach (DataRowView drv in viewFieldsDT)
                {
                    int colNameCol = drv.Row.Table.Columns.IndexOf("ColumnName");
                    DataRow dr = dt2.NewRow();
                    dr[0] = drv[colNameCol].ToString();
                    dr[1] = drv[colNameCol].ToString();
                    dt2.Rows.Add(dr);
                }
                cmbCellFields[j].DisplayMember = "DisplayName";
                cmbCellFields[j].ValueMember = "ColumnName";
                cmbCellFields[j].DataSource = dt2;
                cmbCellFields[j].Enabled = true;
            }

            // Set up the edit columns
            DataView viewFieldsDT2 = new DataView(dataHelper.fieldsDT);
            viewFieldsDT2.RowFilter = String.Format("TableName = '{0}' AND is_PK = 'false' AND is_DK = 'false'", currentSql.myTable);
            //Create 2 column datatable to bind to the drop down
            DataTable dt = new DataTable();
            dt.Columns.Add("DisplayName", Type.GetType("System.String"));
            dt.Columns.Add("ColumnName", Type.GetType("System.String"));
            DataRow dataRow = dt.NewRow();
            dataRow[0] = "Column to Edit";
            dataRow[1] = "0";  // Meaningless name - check for this below to indentify this row 0
            dt.Rows.Add(dataRow);
            foreach (DataRowView drv in viewFieldsDT2)
            {
                int colNameCol = drv.Row.Table.Columns.IndexOf("ColumnName");
                DataRow dr = dt.NewRow();
                dr[0] = drv[colNameCol].ToString();
                dr[1] = drv[colNameCol].ToString();
                dt.Rows.Add(dr);
            }
            //bind do drop down
            cmbEditColumn.DisplayMember = "DisplayName";
            cmbEditColumn.ValueMember = "ColumnName";
            cmbEditColumn.DataSource = dt;
            cmbEditColumn.Enabled = false;  // Make true if "edit" mode

            updating = false;

            writeGrid_NewMainFilter();
            //           watch.Stop();
            //           txtMessages.Text = watch.ElapsedMilliseconds.ToString() + " " + txtMessages.Text;

        }

        internal async Task writeGrid_NewMainFilter()
        {
            updating = true;
            ComboBox[] cmbFilters = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7 };
            List<string> keys = currentSql.DisplayFieldsDictionary.Keys.ToList();
            int i = 0;
            var getTasks = new List<Task>();
            //Fill Combos
            foreach (string key in keys)
            {
                field FK = dataHelper.getFieldValueFieldsDT(currentSql.myTable, key);
                string strSql = currentSql.returnComboSql(command.fkfilter, FK);
                getTasks.Add(CmbFkFilter_FillOneFromDatabaseAsync(cmbFilters[i], strSql));
                i++;
            }
            updating = false;
            getTasks.Add(writeGrid_NewFilter());
            await Task.WhenAll(getTasks);
        }

        internal async Task CmbFkFilter_FillOneFromDatabaseAsync(ComboBox cmb, string strSql)
        {
            DataTable dataTable = new System.Data.DataTable();
            MsSql.FillDataTable(dataTable, strSql);
            // Add "No filter" as first row
            DataRow dr = dataTable.NewRow();
            dr["DisplayField"] = "No Filter";
            dr["ValueField"] = 0;// Some ID
            dataTable.Rows.InsertAt(dr, 0);
            cmb.DisplayMember = "DisplayField";
            cmb.ValueMember = "ValueField";
            cmb.DataSource = dataTable;
            cmb.Enabled = true;
        }

        internal async Task writeGrid_NewFilter()
        {
            // Create the where clauses in sqlBuilder currentSql
            callSqlWheres();
            // Get record Count
            string strSql = currentSql.returnSql(command.count);
            currentSql.RecordCount = MsSql.GetRecordCount(strSql);
            // await 
                writeGrid_NewOrderBy();
        }

        internal void writeGrid_NewOrderBy()
        {
            // Fetch must have an order by clause - so I will add one on first column
            if (currentSql.myOrderBys.Count == 0)  //Should always be true at this point
            {
                orderBy ob = new orderBy(currentSql.myFields[0], System.Windows.Forms.SortOrder.Ascending);
                currentSql.myOrderBys.Add(ob);
            }
            writeGrid_NewPage();
        }

        #endregion 

        #region writeGrid_NewPage

        internal void writeGrid_NewPage()
        {
            ComboBox[] cmbFilter = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7 };
            Label[] lblcmbFilter = { _lblCmbFilter_0, _lblCmbFilter_1, _lblCmbFilter_2, _lblCmbFilter_3, _lblCmbFilter_4, _lblCmbFilter_5, _lblCmbFilter_6, _lblCmbFilter_7 };

            // CENTRAL USE OF sqlCurrent IN PROGRAM
            string strSql = currentSql.returnSql(command.select);

            //Clear the grid - try deleting this because may not be necessary
            if (dataGridView1.DataSource != null)
            {
                dataGridView1.DataSource = null;
            }

            dataGridView1.Columns.Clear();
            dataGridView1.AutoGenerateColumns = true;

            // Bind database
            dataHelper.currentDT = new System.Data.DataTable("currentDT");
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

            dataGridView1.RowHeadersWidth = 27;
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
        }

        private void callSqlWheres()
        {   // Adds all the filters 
            ComboBox[] cmbFilters = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7 };
            TextBox[] txtCellFilters = { txtCellFilter_1, txtCellFilter_2 };
            ComboBox[] cmbCellFields = { cmbCellFields_1, cmbCellFields_2 };

            // Clear any old filters from currentSql
            currentSql.myWheres.Clear();

            //Main filter - add this where to the currentSql)
            if (mainFilter != null)
            {
                if (Convert.ToInt32(mainFilter.whereValue) > 0)
                {
                    // Check that the table and field is in the myFields
                    if (currentSql.TableIsInMyTables(mainFilter.fl.table))
                    {
                        currentSql.myWheres.Add(mainFilter);
                    }
                }
            }
            // Foreign key filters
            for (int i = 0; i < cmbFilters.Length; i++)
            {
                if (cmbFilters[i].Enabled)
                {
                    if (cmbFilters[i].SelectedIndex != -1)
                    {
                        string selectedValue = cmbFilters[i].GetItemText(cmbFilters[i].SelectedValue);
                        if (Convert.ToInt32(selectedValue) > 0)
                        {
                            where wh = new where(currentSql.myTable, cmbFilters[i].Tag.ToString(), selectedValue, DbType.Int32, 4);
                            currentSql.myWheres.Add(wh);
                        }
                    }
                }
            }
            // Cell filters
            for (int i = 0; i < txtCellFilters.Length; i++)
            {
                if (txtCellFilters[i].Enabled)
                {
                    if (txtCellFilters[i].Text != string.Empty)
                    {
                        string selectedValue = txtCellFilters[i].Text;
                        string fieldName = cmbCellFields[i].GetItemText(cmbCellFields[i].SelectedValue);
                        string strDataType = dataHelper.getStringValueFieldsDT(currentSql.myTable, fieldName, "DataType");
                        DbType dbType = dataHelper.ConvertStringToDbType(strDataType);
                        int size = dataHelper.getIntValueFieldsDT(currentSql.myTable, fieldName, "MaxLength");
                        field fi = new field(currentSql.myTable, fieldName, dbType, size);
                        where wh = new where(fi, selectedValue);
                        currentSql.myWheres.Add(wh);
                    }

                }
            }
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        #region EVENTS - Menu events
        //----------------------------------------------------------------------------------------------------------------------

        private void mnuToolsDatabaseInformation_Click(object sender, EventArgs e)
        {
            frmDatabaseInfo formDBI = new frmDatabaseInfo();
            formDBI.ShowDialog();
        }

        private void mnuDeleteDatabase_Click(object sender, EventArgs e)
        {
            //frmDeleteDatabase used to show databases
            frmListDatabases databaseListForm = new frmListDatabases();
            databaseListForm.ShowDialog();
            databaseListForm = null;
        }

        private void contextMenu_MainFilter_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Define mainFilter (Type: where).
                // This assumes the first column in row is the index and it is an integer
                string value = dataGridView1.SelectedRows[0].Cells[0].Value.ToString(); //Integer
                // Set DisplayValue to string with all non-empty cells from selected row (may shorten later)                
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
                string displayValue = String.Join(", ", displayValueList);
                mainFilter = new where(currentSql.myFields[0].table, currentSql.myFields[0].fieldName, value, DbType.Int32, 4);
                mainFilter.displayValue = displayValue;
                //Update pastFilters list
                pastFilters.Insert(0, mainFilter);
                _cmbMainFilter.SelectedIndex = 0;
            }
        }

        private void _cmbMainFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!updating)
            {
                int i = _cmbMainFilter.SelectedIndex;
                if (i != -1)  // Always true (I suspect)
                {
                    //Last element means "no filter".
                    if (_cmbMainFilter.SelectedIndex == _cmbMainFilter.Items.Count - 1)
                    {
                        mainFilter = null;
                        lblMainFilter.Text = "No Filter";
                    }
                    else
                    {
                        mainFilter = pastFilters[i];   // pastFilters is a "where" list that is used to bind _cmbMainFilter
                        lblMainFilter.Text = mainFilter.fl.table;
                    }
                }
                writeGrid_NewMainFilter();
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
            //No need to change if already 0
            if (index > 0)
            {
                List<connectionString> csList = AppData.GetConnectionStringList();
                connectionString cs = csList[index];   // Assumes list in dropdown matches csList
                AppData.storeConnectionString(cs);

                //2. Open connection - this reads the index 0 settings.  Main use of openConnection
                string msg = openConnection();
                if (msg != string.Empty) { txtMessages.Text = msg; }
            }
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
                if (msg != string.Empty) { txtMessages.Text = msg; }
            }
        }

        private void MainMenu1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void mnuDatabase_Click(object sender, EventArgs e)
        {

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
        #region Events - Datagrid Events
        //----------------------------------------------------------------------------------------------------------------------
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            FkComboBoxEditingControl ctl = e.Control as FkComboBoxEditingControl;
            if (ctl != null)
            {
                ctl.DropDown -= new EventHandler(AdjustWidthComboBox_DropDown);
                ctl.DropDown += new EventHandler(AdjustWidthComboBox_DropDown);
            }
        }


        private void dataGridView1_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }
        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("OOPS! Database error.");
            sb.AppendLine(e.Exception.Message);
            MessageBox.Show(sb.ToString());
        }

        private void dataGridView1_CurrentCellChanged(object sender, EventArgs e)
        {
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            // Change colors
            DataGridViewCellStyle cs = new DataGridViewCellStyle();
            cs.BackColor = Color.Aqua;
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style = cs;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Change colors
            DataGridViewCellStyle cs = new DataGridViewCellStyle();
            cs.BackColor = DefaultBackColor;
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style = cs;
        }

        private void dataGridView1_CellLeave(object sender, DataGridViewCellEventArgs e)
        {   // Push the change down to the database.
            if (dataGridView1.IsCurrentCellDirty)
            {
                field PKfield = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                int pkIndex = dataGridView1.Columns[PKfield.fieldName].Index;
                int PKvalue = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[pkIndex].Value);
                DataRow dr = dataHelper.currentDT.Select(string.Format("{0} = {1}", PKfield.fieldName, PKvalue)).FirstOrDefault();
                string strCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].EditedFormattedValue.ToString();
                string strOrg = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                string colName = dataGridView1.Columns[e.ColumnIndex].Name;
                int drColIndex = dataHelper.currentDT.Columns[colName].Ordinal;
                field drColField = currentSql.myFields[drColIndex];
                //string strCellValidated = dataHelper
                string message;
                bool boolTryParse = dataHelper.TryParseToDbType(strCell, drColField.dbType, this, out message);
                if (boolTryParse)
                {
                    try { dr[drColIndex] = strCell; }
                    catch (Exception ex) { 
                        txtMessages.Text = ex.Message; 
                        return; 
                    }
                    // Only update this one row
                    if (dr.RowState == DataRowState.Modified) // always true because I just modified it
                    {
                        DataRow[] drArray = new DataRow[1];
                        drArray[0] = dr;
                        int i = MsSql.currentDA.Update(drArray);
                        txtMessages.Text = MultiLingual.tr("Rows Modified: " + i.ToString(), this);
                    }
                }
                else
                {
                    dataGridView1.EditingControl.Text = strOrg;
                    txtMessages.Text = message;

                }
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // Only allow 2 rows to be selected - used when merging
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


        #endregion

        #region Events - Non-Datagrid

        private void cmbEditColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!updating)
            { 
                cmbEditColumn_SelectedIndexChangeContent();
            }
        }
        private void cmbEditColumn_SelectedIndexChangeContent()
        {
            if (cmbEditColumn.SelectedIndex > 0)
            {
                string strSelectedColumn = cmbEditColumn.SelectedValue.ToString();
                DataGridViewColumn selectedColumn = dataGridView1.Columns[strSelectedColumn];
                if (selectedColumn != null)
                {
                    foreach (DataGridViewColumn col in dataGridView1.Columns)
                    {
                        if (col == selectedColumn) 
                        { 
                            col.ReadOnly = false;
                            FkComboColumn Fkcol = col as FkComboColumn;
                            if (Fkcol != null)
                            {  // Assign Datatable to each cell 
                                field fk = new field(currentSql.myTable, col.Name, DbType.Int32, 4);
                                string strSql = currentSql.returnComboSql(command.fkfilter, fk);
                                MsSql.FillDataTable(dataHelper.extraDT, strSql);
                                int index = dataHelper.currentDT.Columns.IndexOf(col.Name);
                                for (int j = 0; j < dataHelper.currentDT.Rows.Count; j++)
                                {
                                    FkComboCell fkCell = (FkComboCell)dataGridView1.Rows[j].Cells[index];
                                    fkCell.dataTable = dataHelper.extraDT;
                                }
                            }
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
                // Add deleteCommand
                MsSql.SetDeleteCommand(currentSql.myTable, dataHelper.currentDT);
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.ReadOnly = true;
                }
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
                cmbEditColumn.Enabled = true;
                btnDeleteAddMerge.Enabled = false;
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
                    txtMessages.Text = MultiLingual.tr("Please select exactly two rows", this);
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
                        deleteOneDataRow(dr);
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
                        string PKfield = currentSql.myFields[0].fieldName;
                        string PKfield2 = dataHelper.getTablePrimaryKeyField(currentSql.myTable).fieldName;

                        dataHelper.currentDT.Select()




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
                    txtMessages.Text = MultiLingual.tr("Please select row to delete", this);
                    return;
                }
                // int index = dataGridView1.Rows.IndexOf(dataGridView1.SelectedRows[0]);
                field PKfield = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
                int colIndex = dataGridView1.Columns[PKfield.fieldName].Index;
                if (colIndex != 0)
                {
                    txtMessages.Text = MultiLingual.tr("The first column must be the primary key", this);
                    return;
                }
                int PKvalue = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[colIndex].Value);
                DataRow dr = dataHelper.currentDT.Select(string.Format("{0} = {1}", PKfield.fieldName,PKvalue)).FirstOrDefault();
                deleteOneDataRow(dr);
            }
        }

        private void deleteOneDataRow(DataRow dr)
        {
            if (dr == null)
            {
                txtMessages.Text = MultiLingual.tr("Can't find underlying data row in data table", this);
                return;
            }
            dr.Delete();
            // Only update this one row
            DataRow[] drArray = new DataRow[1];
            drArray[0] = dr;
            MsSql.currentDA.Update(drArray);
        }

        private void lblMainFilter_Click(object sender, EventArgs e)
        {
        }

        private void txtCellFilterTextChanged(object sender, EventArgs e)
        {
            if (updating == false)
            {
                writeGrid_NewFilter();
            }
        }

        public void cmbCellFieldsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!updating)
            { // Clear txtCellFilter which will call "WriteGrid_NewFilter"
                TextBox[] txtCellFilters = { txtCellFilter_1, txtCellFilter_2 };
                ComboBox[] cmbCellFields = { cmbCellFields_1, cmbCellFields_2 };
                ComboBox cmb = (ComboBox)sender;
                for (int i = 0; i < cmbCellFields.Count(); i++)
                {
                    if (cmb == cmbCellFields[i])
                    {
                        txtCellFilters[i].Text = string.Empty;
                        if (cmb.SelectedIndex < 1)
                        {
                            txtCellFilters[i].Enabled = false;
                        }
                        else
                        {
                            txtCellFilters[i].Enabled = true;
                        }
                    }
                }
            }
        }

        private void _cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!updating)
            {
                ComboBox cb = (ComboBox)sender;
                int index = cb.SelectedIndex;
                if (index != -1)
                {
                    writeGrid_NewFilter();  // Doesn't change items in cmb_Filters
                }
            }
        }

        // Adjusts the width of all the items shown when combobox dropped down to longest item
        private void AdjustWidthComboBox_DropDown(object sender, EventArgs e)
        {
            // Bound with a "DataView" and DisplayMember column is "DisplayFields"
            ComboBox[] cmbCellFields = { cmbCellFields_1, cmbCellFields_2 };
            // Bound with a "DataTable" and DisplayMember column is "DisplayFields"
            ComboBox[] cmbFilters = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7 };
            TextBox[] cmbCellFilters = { txtCellFilter_1, txtCellFilter_2 };
            // Bound with "BindingList<where>" and DisplayMember is "where" class property "DisplayValue"
            ComboBox[] cmbMainFilters = { _cmbMainFilter };

            // A. Get list of display strings in ComboBox
            var senderComboBox = (System.Windows.Forms.ComboBox)sender;
            List<string> displayValueList = new List<string>();
            if (senderComboBox is FkComboBoxEditingControl)
            {
                var itemsList = senderComboBox.Items.Cast<DataRowView>();
                foreach (DataRowView drv in itemsList)
                {
                    int index = drv.Row.Table.Columns.IndexOf("DisplayField");
                    displayValueList.Add(drv.Row.ItemArray[index].ToString());
                }
            }
            else if (cmbCellFields.Contains(senderComboBox))
            {
                var itemsList = senderComboBox.Items.Cast<DataRowView>();
                foreach (DataRowView drv in itemsList)
                {
                    int index = drv.Row.Table.Columns.IndexOf("DisplayName");
                    displayValueList.Add(drv.Row.ItemArray[index].ToString());
                }
            }
            else if (cmbFilters.Contains(senderComboBox))
            {
                var itemsList = senderComboBox.Items.Cast<DataRowView>();
                foreach (DataRowView drv in itemsList)
                {
                    int index = drv.Row.Table.Columns.IndexOf("DisplayField");
                    displayValueList.Add(drv.Row.ItemArray[index].ToString());
                }
            }
            else if (cmbMainFilters.Contains(senderComboBox))
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

        private void mnuUpdateDatabase_Click(object sender, EventArgs e)
        {
            frmUpdateDatabaseToV2 updateForm = new frmUpdateDatabaseToV2();
            updateForm.ShowDialog();
            updateForm = null;
        }


        #endregion

        //----------------------------------------------------------------------------------------------------------------------

        #region New Various functions and methods
        //----------------------------------------------------------------------------------------------------------------------

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

        private void _lblCmbFilter_7_Click(object sender, EventArgs e)
        {

        }


         #endregion

    }
}


