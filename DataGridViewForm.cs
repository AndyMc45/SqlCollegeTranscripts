using Microsoft.VisualBasic;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics.Metrics;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.ApplicationServices;
using SqlCollegeTranscripts;
using System.Windows.Forms.VisualStyles;

namespace SqlCollegeTranscripts
{
    // The basic design of the program:
    // sqlCurrent stores the table and fields, the Where clauses, and one OrderBy clause.
    // sqlCurrent.returnSql returns the sql string.  This is then bound to the Grid (via an sqlDataAdaptor)

    //User actions, and how the program reacts to each as follows
    //1.  Open New connection -- calls closeConnection (reinitiae everything to empty state; list tables in menu), 
    //    and then open the new connection
    //2.  Open New Table -- sets new sqlCurrent, calls Write New Filters, Calls write New Orderby, Calls write Grid
    //    Write New Table sets table strings (including inner joins) and field strings.
    //    Write the New Filters sets the where clauses - as well as setting up the filters at the top of the screen
    //    Write New OrderBy simply adds an order by clause -- this can be changed via click header of grid event.
    //    Write the Grid - binds dataViewGrid1 and then sets up the rest of the screen.

    internal partial class DataGridViewForm : Form
    {
        #region Variables
        //Store one extra fld that is not on the form -- for Father, Son, etc. recordsets
        internal bool mySql = false, msSql = false;  // currently only using msSql
        internal float changeColwidth = 0;
        internal int changeCol = 0, changeRow = 0;
        internal sqlFactory? currentSql = null;  //builds the sql string via .myInnerJoins, .myFields, .myWheres, my.OrderBys
        private int pageSize = 0;
        private string logFileName = "";
        internal bool updating = false; //updating means changes is cmbFilter or cmbText are by me not the user
        private bool readOnly = false;
        private where? mainFilter;   //Stores main filter
        internal BindingList<where>? pastFilters;  // Use this to fill in past main filters in combo box
        private FileStream? ts;
        // DisplayFieldMap maps foreign keys to Display list fields 
        // private List<Tuple<string, List<field>>> DisplayFieldMap = new List<Tuple<string, List<field>>>();
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
            where wh = new where("none","none", "0", "int");
            wh.displayValue= "Select Row";
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
            setFontSizeForAllControls();

            // 5. Load Database List (in files menu)
            load_mnuDatabaseList();

            // 6. Open Log file
            // openLogFile(); //errors ignored

            // 7. Open last connection 
                string msg = openConnection();            
                if(msg != string.Empty) { txtMessages.Text = msg; } 
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

        private void setFontSizeForAllControls()
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
            for (int i = 0; i <= cmbFilter.Count() - 1; i++) { cmbFilter[i].Font = font; }
            for (int i = 0; i <= txtCellFilter.Count() - 1; i++) { txtCellFilter[i].Font = font; }
            for (int i = 0; i <= cmbCellFields.Count() - 1; i++) { cmbCellFields[i].Font = font; }
            for (int i = 0; i <= radioButtons.Count() - 1; i++) { radioButtons[i].Font = font; }
            cmbEditColumn.Font = font;
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
                    sb.AppendLine("No previous connection string.");
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

                    foreach (DataRow row in dataHelper.tablesDT.Rows)
                    {
                        string? tn = row["TableName"].ToString();
                        if(tn != null) { 
                            mnuOpenTables.DropDownItems.Add(tn);
                        }
                    }
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
            while(pastFilters.Count > 1) 
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
            tableLayoutPanel_Filters.Height = 2;
            if (cmbCellFields_1.Enabled)
            {
                tableLayoutPanel_Filters.Height =
                    txtMessages.Height + cmbCellFields_1.Top + cmbCellFields_1.Height + 5;
            }
            if (_cmbFilter_0.Enabled || rbEdit.Checked)
            {
                tableLayoutPanel_Filters.Height =
                    txtMessages.Height + _cmbFilter_0.Top + _cmbFilter_1.Height + 5;
            }
            if (_cmbFilter_3.Enabled)
            {
                tableLayoutPanel_Filters.Height =
                    txtMessages.Height + _cmbFilter_3.Top + _cmbFilter_3.Height + 5;
            }
            if (_cmbFilter_5.Enabled)
            {
                tableLayoutPanel_Filters.Height =
                    txtMessages.Height + _cmbFilter_5.Top + _cmbFilter_5.Height + 5;
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
        #region Writing the Grid
        //----------------------------------------------------------------------------------------------------------------------
        // Write_Grid_NewTable called by mnuOpenTable_Click, mnuFather_Click, mnuSon_Click
        // Set initial state and things that don't change for this table here 
        private void writeGrid_NewTable(string table)
        {
  //          var watch = Stopwatch.StartNew();

            toolStripMsg.Text = table;
            rbView.Checked = true;
            updating = true; 
            resetComboAndCellFilters();
            updating = false;

            // Create currentSql - same currentSql used until new table is loaded
            currentSql = new sqlFactory(table, 1, pageSize);

            // Put Primary key of main table in the first field of myFields
            string pk = dataHelper.getTablePrimaryKeyField(currentSql.myTable);
            field fi = new field(currentSql.myTable, pk, "int");
            currentSql.myFields.Add(fi);

            // This sets currentSql table and field strings - and these remain the same for this table.
            // This also sets DisplayFieldDicitionary each foreign table key in main table
            string msg = currentSql.callInnerJoins(currentSql.myTable, "");
            if (!String.IsNullOrEmpty(msg)) { toolStripBottom.Text = msg; }

            // Set up 8 cmbFilter labels and boxes - BUT don't bind them yet - 9 milliseconds
            ComboBox[] cmbFilter = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7 };
            Label[] lblcmbFilter = { _lblCmbFilter_0, _lblCmbFilter_1, _lblCmbFilter_2, _lblCmbFilter_3, _lblCmbFilter_4, _lblCmbFilter_5, _lblCmbFilter_6, _lblCmbFilter_7 };
            int i = 0;
            // one combo box for each foreignKey
            foreach (String key in currentSql.DisplayFieldsDictionary.Keys)
            {
                field fi2 = dataHelper.getForeignTableAndKey(currentSql.myTable, key);
                lblcmbFilter[i].Text = fi2.table; // Can be translated or changed
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
            DataTable dt = new DataTable();
            dt.Columns.Add("DisplayName", Type.GetType("System.String"));
            dt.Columns.Add("ColumnName", Type.GetType("System.String"));
            DataRow dataRow = dt.NewRow();
            dataRow[0] = "Column to Edit";
            dataRow[1] = "0";
            dt.Rows.Add(dataRow);
            foreach (DataRowView drv in viewFieldsDT2)
            {
                int colNameCol = drv.Row.Table.Columns.IndexOf("ColumnName");
                DataRow dr = dt.NewRow();
                dr[0] = drv[colNameCol].ToString();
                dr[1] = drv[colNameCol].ToString();
                dt.Rows.Add(dr);
            }
            cmbEditColumn.DisplayMember = "DisplayName";
            cmbEditColumn.ValueMember = "ColumnName";
            cmbEditColumn.DataSource = dt;
            cmbEditColumn.Enabled = false;  // Make true if "edit" mode

            writeGrid_NewMainFilter();
//           watch.Stop();
//           txtMessages.Text = watch.ElapsedMilliseconds.ToString() + " " + txtMessages.Text;

        }

        // Write_Grid_NewTable called by WriteGrid_NewTable, Main_Filter Changed
        internal async Task writeGrid_NewMainFilter()
        {
            ComboBox[] cmbFilters = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7 };
            List<string> keys = currentSql.DisplayFieldsDictionary.Keys.ToList();
            int i = 0;
            var getTasks = new List<Task>();
            foreach (string key in keys)
            {
                currentSql.myFieldsCombo[i] = currentSql.DisplayFieldsDictionary[key];
                string strSql = currentSql.returnSql(command.fkfilter, key, i);
                getTasks.Add(CmbFkFilter_FillOneFromDatabaseAsync(cmbFilters[i], strSql));
                i++;
            }
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
            string strSql = currentSql.returnSql(command.count,"",0);
            currentSql.RecordCount = MsSql.GetRecordCount(strSql);
            await writeGrid_NewOrderBy();
        }

        internal async Task writeGrid_NewOrderBy()
        {
            // Fetch must have an order by clause - so I will add one on first column
            if (currentSql.myOrderBys.Count == 0)  //Should always be true at this point
            {
                orderBy ob = new orderBy(currentSql.myFields[0], System.Windows.Forms.SortOrder.Ascending);
                currentSql.myOrderBys.Add(ob);
            }
            await writeGrid_NewPage();
        }

        internal async Task writeGrid_NewPage()
        {

            ComboBox[] cmbFilter = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7 };
            Label[] lblcmbFilter = { _lblCmbFilter_0, _lblCmbFilter_1, _lblCmbFilter_2, _lblCmbFilter_3, _lblCmbFilter_4, _lblCmbFilter_5, _lblCmbFilter_6, _lblCmbFilter_7 };

            // CENTRAL USE OF sqlCurrent IN PROGRAM
            string strSql = currentSql.returnSql(command.select,"", 0);

            //Clear the grid
            if (dataGridView1.DataSource != null)
            {
                dataGridView1.DataSource = null;
            }


            // Bind database
            dataHelper.currentDT = new System.Data.DataTable("currentDT");
            MsSql.FillDataTable(dataHelper.currentDT, strSql);
            dataGridView1.DataSource = dataHelper.currentDT;

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
                //Set width of column - default
                string dbType = dataHelper.getStringValueFieldsDT(baseTable, fld, "DataType");
                switch (dbType)
                {
                    case "bigint":
                    case "numeric":
                    case "smallint":
                    case "decimal":
                    case "smallmoney":
                    case "int":
                    case "tinyint":
                    case "money":
                    case "float":
                    case "real":
                    case "binary":
                        dataGridView1.Columns[i].Width = 47; //one character = about 200 points
                        break;
                    case "date":
                    case "datetimeoffset":
                    case "datetime2":
                    case "smalldatetime":
                    case "datetime":
                    case "time":
                        dataGridView1.Columns[i].Width = 67;
                        break;
                    case "char":
                    case "varchar":
                    case "nchar":
                    case "nvarchar":
                        dataGridView1.Columns[i].Width = 107;  //Same starting string
                        break;
                    case "bit":
                        dataGridView1.Columns[i].Width = 67;
                        break;
                    default:
                        dataGridView1.Columns[i].Width = 107;
                        break;
                }
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
            return;

            // MessageBox.Show(Information.Err().Description + Environment.NewLine + translation.tr("ErrorWritingToGrid", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
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
                            where wh = new where(currentSql.myTable, cmbFilters[i].Tag.ToString(), selectedValue, "int");
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
                        string dataType = dataHelper.getStringValueFieldsDT(currentSql.myTable, fieldName, "DataType");
                        field fi = new field(currentSql.myTable, fieldName, dataType);
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
                foreach( DataGridViewCell cell in dataGridView1.SelectedRows[0].Cells ) 
                {
                    if (cell.Value != null)
                    {  
                        if(!String.IsNullOrEmpty(cell.Value.ToString()))
                        {
                            field fi = currentSql.myFields[cell.ColumnIndex];
                            if(dataHelper.isDisplayKey(fi.table,fi.fieldName))
                            { 
                                displayValueList.Add(Convert.ToString(cell.Value));
                            }
                        }
                    }
                }
                string displayValue = String.Join(", ", displayValueList);
                mainFilter = new where(currentSql.myFields[0].table, currentSql.myFields[0].fieldName, value, "int");
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
                    if(_cmbMainFilter.SelectedIndex == _cmbMainFilter.Items.Count - 1)
                    {
                        mainFilter = null;
                        lblMainFilter.Text = "No Filter";
                    }
                    else { 
                        mainFilter= pastFilters[i];   // pastFilters is a "where" list that is used to bind _cmbMainFilter
                        lblMainFilter.Text = mainFilter.fl.table;
                    }
                }
                writeGrid_NewMainFilter();
            }
        }

        private void mnuOpenTables_Click(object sender, EventArgs e) { }

        private void mnuOpenTables_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string tableName = e.ClickedItem.Text;
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
            foreach (connectionString cs in csList) {
                // {0} for server, {1} for Database, {2} for user, {3} for password (unknown)
                string csString = String.Format(cs.comboString, cs.server, cs.databaseName, cs.user, "******");
                mnuConnectionList.DropDownItems.Add(csString);
            }
        }

        #endregion

        //----------------------------------------------------------------------------------------------------------------------
        #region Events - Control Events
        //----------------------------------------------------------------------------------------------------------------------
        #region 5 paging buttons & RecordsPerPage (RPP)
        // Paging - <<
        private void txtRecordsPerPage_Leave(object sender, EventArgs e)
        {
            int rpp = 0;
            if(int.TryParse(txtRecordsPerPage.Text,out rpp))
            {
                if (rpp > 9)
                {
                    pageSize = rpp;
                    AppData.SaveKeyValue("RPP",rpp.ToString());
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
                    if(!captionIsInt)
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
                    if(currentSql.myPage < currentSql.TotalPages)
                    { 
                        currentSql.myPage++;
                        writeGrid_NewPage();
                    }
                }
            }
            // Paging - >>
            private void toolStripButton5_Click(object sender, EventArgs e)
            {
                if(currentSql != null)
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
            // Clear txtCellFilter which will call "WriteGrid_NewFilter"
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

          private void cmbCellFilterSelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            if (cmb.SelectedIndex > -1) 
            {
                writeGrid_NewFilter();
            }

        }

        // Adjusts the width of all the items shown when combobox dropped down to longest item
        private void AdjustWidthComboBox_DropDown(object sender, EventArgs e)
        {
            // Bound with a DataView and DisplayMember column is "DisplayFields"
            ComboBox[] cmbCellFields = { cmbCellFields_1, cmbCellFields_2 };
            // Bound with a DataTable and DisplayMember column is "DisplayFields"
            ComboBox[] cmbFilters = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7 };
            TextBox[] cmbCellFilters = { txtCellFilter_1, txtCellFilter_2 };
            // Bound with BindingList<where>.  DisplayMember is "where" class property "DisplayValue"
            ComboBox[] cmbMainFilters = { _cmbMainFilter };

            // A. Get list of display strings in ComboBox
            var senderComboBox = (System.Windows.Forms.ComboBox)sender;
            List<string> displayValueList = new List<string>();
            if (cmbCellFields.Contains(senderComboBox))
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

        private void tableLayoutPanel_Filters_SizeChanged(object sender, EventArgs e)
        {
            TableLayoutPanel senderPanel = (TableLayoutPanel)sender;
            this.splitContainer1.SplitterDistance = senderPanel.Height + 5; 
        }

        private void rbView_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void mnuToolsDatabaseInformation_Click(object sender, EventArgs e)
        {
            frmDatabaseInfo formDBI = new frmDatabaseInfo();
            formDBI.ShowDialog();
        }

        private void _cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            int index = cb.SelectedIndex;
            if (index != -1)
            {
                writeGrid_NewFilter();  // Doesn't change items in cmb_Filters
            }
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

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void rbEdit_CheckedChanged(object sender, EventArgs e)
        {
            if (rbEdit.Checked) 
            {
                cmbEditColumn.Enabled = true; }
            else 
            {
                cmbEditColumn.Enabled = false;    
            }
            SetTableLayoutPanelHeight(); 
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            txtMessages.Text = "Cell double clicked";
        }


        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            txtMessages.Text = string.Empty;
            txtMessages.Text = string.Empty;
            if (dataGridView1.SelectedRows.Count > 0)
            {
                if (rbEdit.Checked)
                {
                    if (cmbEditColumn.SelectedIndex > 0)
                    {
                        txtMessages.Text = "Edit column " + cmbEditColumn.SelectedValue;
                    }

                }
            }
        }

        private void rbDelete_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void _lblCmbFilter_7_Click(object sender, EventArgs e)
        {

        }

        private void CommonDialogOpen_FileOk(object sender, CancelEventArgs e)
        {

        }

        internal object convertValue(string str, DbType fieldType)
        {
            object result = null;
            string errorMsg = "";
            try
            {
                if (fieldType == DbType.Int16)
                {
                    errorMsg = "integer";
                    return Convert.ToInt32(Double.Parse(str));
                }
                else if (fieldType == DbType.Boolean)
                {
                    errorMsg = "boolean";
                    bool tempBool = false;
                    return (Boolean.TryParse(str, out tempBool)) ? tempBool : Convert.ToBoolean(Double.Parse(str));
                }
                else if (fieldType == DbType.Int32)
                {
                    errorMsg = "long integer";
                    return Convert.ToInt32(Double.Parse(str));
                }
                else if (fieldType == DbType.Single)
                {
                    errorMsg = "small real number";
                    return Single.Parse(str);
                }
                else if (fieldType == DbType.Double)
                {
                    errorMsg = "real number";
                    return Double.Parse(str);
                }
                else if (fieldType == DbType.DateTime)
                {
                    errorMsg = "legal date";
                    return DateTime.Parse(str);
                }
                else if (fieldType == DbType.Currency)
                {
                    errorMsg = "currency";
                    return Decimal.Parse(str, NumberStyles.Currency | NumberStyles.AllowExponent);
                }
                else if (fieldType == DbType.Decimal)
                {
                    errorMsg = "decimal";
                    return Decimal.Parse(str, NumberStyles.Currency | NumberStyles.AllowExponent);
                }
                else
                { //strings adVarChar, adVarWChar, adWChar, adChar, adLongVarChar, adLongVarWChar
                    return str;
                }
            }
            catch
            {
                // result = "ERROR! " + translation.tr("TheValueIsNotA", str, errorMsg, "");
            }
            return result;
        }


        internal bool findInCombos(string comboTable, string strID)
        {
            ComboBox[] cmbFilter = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7 };
            //Puts strID into the comboTable box, returns true if succesful.
            //CurrentTable is already set and Combos already loaded -- but the grid may not be written yet.
            //int tempForEndVar = cmbFilter.Count() - 1;
            //for (int i = 0; i <= tempForEndVar; i++)
            //{
            //    if (cmbFilter[i].Visible)
            //    {
            //        //Combo table is in cmbFilter(i) (cmbFilter(i).tag is the id of the table - must.
            //        if (comboTable == tableInCombo(i))
            //        {
            //            int tempForEndVar2 = cmbFilterBackup[i].Items.Count - 1;
            //            for (int j = 0; j <= tempForEndVar2; j++)
            //            {
            //                if ((string)cmbFilterBackup[i].Items[j] == strID)
            //                { //Find strID in cmbFilterBackup
            //                    updating = true;
            //                    cmbFilter[i].SelectedIndex = j; //Select strID in cmbFilter
            //                    return true;
            //                }
            //            }
            //        }
            //    }
            //}
            return false;
        }

        #endregion

    }
}


// OLD PROGRAM

#region OLD Connection - open and close and related functions
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////OPEN CONNECTION TO WRITE GRID
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////}
//private void setupTopBoxes()
//{
//    //Called when you switch a table.  Sets up all the top boxes for that table
//    //This is only called by newSqlCurrent.
//    string gridFld = "", fld = "";
//    string son = "", strWidth = "";
//    int width = 0;

//    //On Error GoTo errHandler
//    SqlDataReader rs = new SqlDataReader("");
//    SqlDataReader rs2 = new SqlDataReader("");
//    updating = true; //Prevents topBox events from firing when topBox is changed

//    //Always show cmbAdd and cmbShowAll
//    cmdAdd.Visible = true;
//    cmdShowAll.Visible = true;

//    //Set the top and height of the flexgrid
//    dataGridView1.Top = (40 + 280 + 355 + 40) / 15; //a gap, labels, cmbFilter box, a gap
//    dataGridView1.Height = this.ClientSize.Height - (dataGridView1.Top + 70);

//    //Hide cmbFilter controls - all at top
//    int tempForEndVar = lblcmbFilter.ControlCount() - 1;
//    for (int i = 0; i <= tempForEndVar; i++)
//    {
//        lblcmbFilter[i].Top = 3;
//        lblcmbFilter[i].Visible = false;
//        lblcmbFilter[i].Height = 17;
//        cmbFilter[i].Top = 19;
//        cmbFilter[i].Visible = false;
//        cmbFilterBackup[i].Visible = false;
//    }

//    //Hide txtWhere
//    lblTxtWhere.Visible = false;
//    txtWhere.Visible = false;
//    txtWhere.Text = "";

//    //Clear out old box info
//    clearTopBoxes();

//    //Begin to the left of the show all
//    int nextleft = 1340;

//    //Show the text box field
//    string strSql = "Select * from afdTableData where tableName = '" + currentSql.myTable + "'";
//    rs.Open(strSql, this.ClientSize.cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//    if (!rs.EOF)
//    {
//        gridFld = strValue(rs, "textBox"); //gridFld must be visible in the current table
//                                           //User has selected a textBox
//        if (gridFld != "")
//        {
//            //See if there is such a fld in this table
//            rs.Close();
//            strSql = "Select * from afdFieldData where tableName = '" + currentSql.myTable + "' AND fieldName = '" + short_Renamed(gridFld) + "'";
//            rs.Open(strSql, this.ClientSize.cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//            //Set up txt boox
//            if (!rs.EOF)
//            {
//                width = lngValue(rs, "width");
//                if (width == 0)
//                {
//                    width = 2000;
//                }
//                //Set lbl
//                lblTxtWhere.Left = (nextleft + 50) / 15;
//                lblTxtWhere.Width = width / 15;
//                lblTxtWhere.Text = short_Renamed(gridFld) + ":";
//                lblTxtWhere.Visible = true;
//                lblTxtWhere.Font = lblTxtWhere.Font.Change(bold: true);
//                //Set text
//                txtWhere.Left = nextleft / 15;
//                txtWhere.Width = width / 15;
//                txtWhere.Text = "";
//                txtWhere.Tag = gridFld;
//                txtWhere.Visible = true;
//                nextleft = nextleft + width + 200;
//            }
//            else
//            {
//                MessageBox.Show(translation.tr("YourTextBoxFieldEntryInTheAfdTableDataTableIsNotAFieldInThisTableIgnoringEntry", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//            }
//        }
//    }
//    rs.Close();

//    //Go through the table, and find all ID fields -- list each in a combo where box.
//    strSql = "Select * from afdFieldData where tableName = '" + currentSql.myTable + "'";
//    rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);

//    //Show the combo boxes
//    int Index = 0;
//    while (!rs.EOF)
//    {
//        fld = Convert.ToString(rs["fieldName"]);
//        //fld is an innerjoin field
//        if (this.ClientSize.strValue(currentSql.myTable, fld))
//        {
//            //Show the cmbFilter box
//            son = getForeignTable(currentSql.myTable, fld);
//            strWidth = getSystemTableValue("afdTableData", son, "", "comboWidth", DbType.Int32);
//            if (Information.IsNumeric(strWidth))
//            {
//                width = Convert.ToInt32(Double.Parse(strWidth)); //May be 0
//            }
//            else
//            {
//                width = 0;
//            }
//            if (width == 0)
//            {
//                width = 2000; //default
//            }
//            //Start a second row
//            if (nextleft + width + cmdAdd.Width * 15 + 200 > dataGridView1.Width * 15)
//            {
//                nextleft = 50;
//                int tempForEndVar2 = cmbFilter.ControlCount() - 1;
//                for (int i = Index; i <= tempForEndVar2; i++)
//                {
//                    lblcmbFilter[i].Top = lblcmbFilter[i].Top + 19 + 22; //move over label and cmbFilter
//                    cmbFilter[i].Top = cmbFilter[i].Top + 19 + 22;
//                }
//                dataGridView1.Top = dataGridView1.Top + 19 + 24;
//                dataGridView1.Height = this.ClientSize.Height - (dataGridView1.Top + 70);
//            }
//            //Position and set label and box
//            lblcmbFilter[Index].Left = (nextleft + 50) / 15;
//            lblcmbFilter[Index].Width = width / 15;
//            lblcmbFilter[Index].Text = son + ":";
//            lblcmbFilter[Index].Visible = true;
//            lblcmbFilter[Index].Font = lblcmbFilter[Index].Font.Change(bold: true);
//            cmbFilter[Index].Left = nextleft / 15;
//            cmbFilter[Index].Width = width / 15;
//            cmbFilter[Index].Tag = fld; //tag is the foreign key of the son (the table in combo)
//            cmbFilter[Index].Visible = true;
//            //Fill the combo box
//            fillComboWhere(Index);
//            //Prepare for next cmbFilter
//            nextleft = nextleft + width + 200;
//            Index++;
//        }
//        rs.MoveNext();
//    }
//    rs.Close();
//    return;

//    MessageBox.Show(Information.Err().Description + Environment.NewLine + translation.tr("ErrorWritingComboBoxes", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//}
//private void fillComboWhere(int Index)
//{
//    int i = 0;
//    string strFieldList = "";
//    string[] fldArray = null;
//    string fieldFather = "";
//    frmCaptions captionsForm = frmCaptions.CreateInstance();
//    string sqlCurrentTableFilterTable = "";

//    string comboTable = getForeignTable(currentSql.myTable, Convert.ToString(cmbFilter[Index].Tag));

//    //Initialize
//    SqlCommand rs = new SqlCommand();
//    SqlCommand rsFieldData = new SqlCommand();
//    //SqlDataReader rs = new SqlDataReader("");
//    //SqlDataReader rsFieldData = new SqlDataReader("");

//    //Clear combo
//    cmbFilter[Index].Items.Clear();
//    cmbFilterBackup[Index].Items.Clear();

//    //Get sqlClass strSql for this cmbFilter table
//    sql sqlClass = new sql();
//    sqlClass.setTableString(comboTable, "combo");


//    if (mnuOptionsUseTableFilters.Checked)
//    {
//        //Don't restrict this combo if this combo is the restricted table.
//        //Suppose this table is "Course-Term" and it is restricted by "Term", perhaps term 42. We still want to include all terms in the table
//        sqlCurrentTableFilterTable = sqlCurrent.mytableFilterTable();
//        if (sqlCurrentTableFilterTable == comboTable)
//        {
//            //Note this in label
//            if ((lblcmbFilter[Index].Text.IndexOf(" (F)") + 1) == 0)
//            {
//                lblcmbFilter[Index].Text = lblcmbFilter[Index].Text + " (F)";
//            }
//        }
//        else
//        {
//            //If there is a restricted table, then restrict this combo by that table
//            //Suppose this is "Course".  higherInnderJoin restricts this to courses offered in term 42.
//            //Higher inner join changes various things in sqlClass and should only be called once.
//            if (sqlCurrentTableFilterTable != "")
//            {
//                //Only use of higherInderJoin
//                sqlClass.higherInnerJoin(currentSql.myTable, sqlCurrentTableFilterTable);
//            }
//            //Update the label
//            lblcmbFilter[Index].Text = StringsHelper.Replace(lblcmbFilter[Index].Text, " (F)", "", 1, -1, CompareMethod.Binary);
//        }
//    }
//    ComboBox[] cmbFilterBackup = { _cmbFilterBackup_0, _cmbFilterBackup_1, _cmbFilterBackup_2, _cmbFilterBackup_3, _cmbFilterBackup_4, _cmbFilterBackup_5, _cmbFilterBackup_6, _cmbFilterBackup_7, _cmbFilterBackup_8 };
//    ComboBox[] cmbFilter = { _cmbFilter_0, _cmbFilter_1, _cmbFilter_2, _cmbFilter_3, _cmbFilter_4, _cmbFilter_5, _cmbFilter_6, _cmbFilter_7, _cmbFilter_8 };
//    string strSql = sqlClass.returnSql();

//    //Open recordset -- the restricted class can cause some unknown error if we have an object in the table
//    //In this case, don't restrict the recordset
//    //UPGRADE_TODO: (1069) Error handling statement (On Error Resume Next) was converted to a pattern that might have a different behavior. More Information: https://docs.mobilize.net/vbuc/ewis#1069
//    bool resume = true;
//    try
//    {
//        rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);

//        if (mnuOptionsUseTableFilters.Checked && Information.Err().Description != "")
//        {
//            sqlClass = new sql();
//            sqlClass.setTableString(comboTable, "combo");
//            strSql = sqlClass.returnSql();
//            //UPGRADE_TODO: (1069) Error handling statement (On Error Goto 0) was converted to a pattern that might have a different behavior. More Information: https://docs.mobilize.net/vbuc/ewis#1069
//            resume = false;
//            Debug.WriteLine(strSql);
//            rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//        }
//    }
//    catch (Exception exc)
//    {
//        NotUpgradedHelper.NotifyNotUpgradedElement("Resume in On-Error-Resume-Next Block");
//        if (!resume)
//        {
//            throw exc;
//        }
//    }

//    //Set page
//    int comboPage = 1;
//    int comboPages = Convert.ToInt32(Math.Floor(rs.RecordCount / ((double)recordsPerPage)) + 1);
//    if (comboPages > 1)
//    {
//        captionsForm = frmCaptions.CreateInstance();
//        captionsForm.myPages = comboPages;
//        captionsForm.job = "pages";
//        //UPGRADE_ISSUE: (2064) Void method Global.Load was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //VB.Global.Load(captionsForm);
//        captionsForm.Text = "combo page: " + comboTable;
//        captionsForm.ShowDialog();
//        if (captionsForm.selectedCaption == "")
//        {
//            comboPage = 0;
//        }
//        else
//        {
//            comboPage = Convert.ToInt32(Double.Parse(captionsForm.selectedCaption));
//        }
//        //Note page in lblcmbFilter
//        lblcmbFilter[Index].Text = comboTable + ":" + "(" + comboPage.ToString() + ")";
//        captionsForm = null;
//    }

//    //Put first element in cmbFilter and cmbFilterBackup lists
//    cmbFilter[Index].AddItem("  ");
//    cmbFilterBackup[Index].AddItem("0");

//    //Paging
//    if (comboPages > 1)
//    {
//        rs.Move((comboPage - 1) * recordsPerPage);
//    }

//    //Get the fldArray -- fields to be included in combo box strings
//    fldArray = new string[] { "" };
//    int tempForEndVar = rs.FieldsMetadata.Count - 1;
//    for (i = 0; i <= tempForEndVar; i++)
//    {
//        if (!isIdField(rs.GetField(i).FieldMetadata.ColumnName))
//        {
//            fieldFather = pathDownTo(comboTable, rs.GetField(i).FieldMetadata.ColumnName);
//            if (fieldFather != "")
//            {
//                //Only include defining fields -- elimiating this gives all data from bottom table
//                strSql = "Select * from afdFieldData where tableName = '" + fieldFather + "' AND fieldName = '" + rs.GetField(i).FieldMetadata.ColumnName + "' AND showInYellow = " + getSqlTrue();
//                rsFieldData.Open(strSql, this.ClientSize.cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//                if (!rsFieldData.EOF)
//                {
//                    if (fldArray[0] != "")
//                    {
//                        fldArray = ArraysHelper.RedimPreserve(fldArray, new int[] { fldArray.GetUpperBound(0) + 2 });
//                    }
//                    fldArray[fldArray.GetUpperBound(0)] = rs.GetField(i).FieldMetadata.ColumnName;
//                }
//                rsFieldData.Close();
//            }
//        }
//    }
//    //Put in an element for each row in table
//    string tablePrimaryKey = getTablePrimaryKeyField(comboTable);
//    i = 1;
//    int recordLimit = recordsPerPage + 1;
//    if (comboPage == 0)
//    {
//        recordLimit = 0;
//    }
//    while (!rs.EOF && i < recordLimit)
//    {
//        i++;
//        strFieldList = getComboString(rs, fldArray);
//        if (strFieldList == "")
//        {
//            strFieldList = "ID " + lngValue(rs, tablePrimaryKey).ToString();
//        }
//        cmbFilter[Index].AddItem(strFieldList);
//        //cmbFilterBackup contains the ID of this record -- this is what we use to get the record
//        cmbFilterBackup[Index].AddItem(strValue(rs, tablePrimaryKey));
//        rs.MoveNext();
//    }
//    //cleanup
//    rs.Close();
//}
//private string getComboString(SqlDataReader rs, string[] fldArray)
//{
//    string str = "", temp = "";
//    StringBuilder strFieldList = new StringBuilder();
//    //No fields -- this handled above
//    if (fldArray[0] == "")
//    {
//        return "";
//    }
//    //Regular case
//    foreach (string fldArray_item in fldArray)
//    {
//        //        If Not isIdField(rs.Fields(c).Name) Then
//        //            str = strValue(rs, rs.Fields(c).Name)
//        str = strValue(rs, fldArray_item);
//        temp = fldArray_item;
//        if (Strings.Len(temp) > 5)
//        {
//            temp = temp.Substring(0, Math.Min(4, temp.Length)) + "..";
//        }
//        if (str.ToLower() == "true")
//        {
//            str = temp;
//        }
//        else if (str.ToLower() == "false")
//        {
//            str = "~" + temp;
//        }
//        //Add comma if needed
//        if (str == "")
//        {
//            str = "(" + fldArray_item + ")";
//        }
//        if (strFieldList.ToString() == "")
//        {
//            strFieldList = new StringBuilder(str);
//        }
//        else
//        {
//            strFieldList.Append(", " + str);
//        }
//        //        End If
//    }
//    return strFieldList.ToString();
//}
#endregion

#region OLD Control Events
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////VARIOUS CONTROL EVENTS
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
//private void flexGrid_MouseDown(Object eventSender, MouseEventArgs eventArgs)
//{
//    int Button = (eventArgs.Button == MouseButtons.Left) ? 1 : ((eventArgs.Button == MouseButtons.Right) ? 2 : 4);
//    int Shift = ((int)Control.ModifierKeys) / 65536;
//    float x = eventArgs.X * 15;
//    float y = eventArgs.Y * 15;
//    changeCol = dataGridView1.CurrentCell.ColumnIndex;
//    changeRow = dataGridView1.CurrentCell.RowIndex;
//    changeColwidth = x;
//}
//private void flexGrid_MouseUp(Object eventSender, MouseEventArgs eventArgs)
//{
//    int Button = (eventArgs.Button == MouseButtons.Left) ? 1 : ((eventArgs.Button == MouseButtons.Right) ? 2 : 4);
//    int Shift = ((int)Control.ModifierKeys) / 65536;
//    float x = eventArgs.X * 15;
//    float y = eventArgs.Y * 15;
//    int newWidth = 0;
//    string baseTable = "";
//    string fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//    if (fld != "")
//    {
//        if (dataGridView1.CurrentCell.ColumnIndex == changeCol && dataGridView1.CurrentCell.RowIndex == changeRow)
//        {
//            if ((changeColwidth - x) > 50 || (changeColwidth - x) < -50)
//            {
//                newWidth = Convert.ToInt32(dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].Width - (changeColwidth - x));
//                if (newWidth < 100)
//                {
//                    newWidth = 100;
//                }
//                dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].Width = newWidth / 15;
//                //SetColumnWidth(dataGridView1.CurrentCell.ColumnIndex, newWidth / 15);
//                //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//                //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//                //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//                baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//                updateFieldSystemTable(baseTable, short_Renamed(colCaption(dataGridView1.CurrentCell.ColumnIndex)), "width", newWidth.ToString(), DbType.Int32);
//            }
//        }
//    }
//}
//private void cmdShowAll_Click(Object eventSender, EventArgs eventArgs)
//{
//    bool tempRefParam = true;
//    writeGrid(ref tempRefParam);
//}
//private void lblcmbFilter_DoubleClick(Object eventSender, EventArgs eventArgs)
//{
//    int Index = Array.IndexOf(this.lblcmbFilter, eventSender);
//    //Sets content of cmbFilter(Index) to ""
//    if (cmbFilter[Index].SelectedIndex > 0)
//    {
//        cmbFilter[Index].SelectedIndex = 0;
//    }
//}
//private void lblcmbFilter_Click(Object eventSender, EventArgs eventArgs)
//{
//    int Index = Array.IndexOf(this.lblcmbFilter, eventSender);
//    msgSB(cmbFilter[Index].Text);
//}
//private void lblcmbFilter_MouseDown(Object eventSender, MouseEventArgs eventArgs)
//{
//    int Index = Array.IndexOf(this.lblcmbFilter, eventSender);
//    int Button = (eventArgs.Button == MouseButtons.Left) ? 1 : ((eventArgs.Button == MouseButtons.Right) ? 2 : 4);
//    int Shift = ((int)Control.ModifierKeys) / 65536;
//    float x = eventArgs.X * 15;
//    float y = eventArgs.Y * 15;
//    changeCol = Index;
//    changeColwidth = x;
//}
//private void lblcmbFilter_MouseUp(Object eventSender, MouseEventArgs eventArgs)
//{
//    int Index = Array.IndexOf(this.lblcmbFilter, eventSender);
//    int Button = (eventArgs.Button == MouseButtons.Left) ? 1 : ((eventArgs.Button == MouseButtons.Right) ? 2 : 4);
//    int Shift = ((int)Control.ModifierKeys) / 65536;
//    float x = eventArgs.X * 15;
//    float y = eventArgs.Y * 15;
//    int newWidth = 0;
//    string baseTable = "", myTable = "";
//    if ((changeColwidth - x) > 50 || (changeColwidth - x) < -50)
//    {
//        newWidth = Convert.ToInt32(cmbFilter[Index].Width * 15 - (changeColwidth - x));
//        if (newWidth < 300)
//        {
//            newWidth = 300;
//        }
//        cmbFilter[Index].Width = newWidth / 15;
//        myTable = getForeignTable(currentSql.myTable, Convert.ToString(cmbFilter[Index].Tag));
//        updateTableSystemTable(myTable, "comboWidth", newWidth.ToString(), DbType.Int32);
//    }
//}

//private void lblTxtWhere_DoubleClick(Object eventSender, EventArgs eventArgs)
//{
//    if (txtWhere.Text != "")
//    {
//        updating = true;
//        txtWhere.Text = "";
//        bool tempRefParam = false;
//        writeGrid(ref tempRefParam);
//    }
//}

//private void lblTxtWhere_MouseDown(Object eventSender, MouseEventArgs eventArgs)
//{
//    int Button = (eventArgs.Button == MouseButtons.Left) ? 1 : ((eventArgs.Button == MouseButtons.Right) ? 2 : 4);
//    int Shift = ((int)Control.ModifierKeys) / 65536;
//    float x = eventArgs.X * 15;
//    float y = eventArgs.Y * 15;
//    changeColwidth = x;
//}
//private void lblTxtWhere_MouseUp(Object eventSender, MouseEventArgs eventArgs)
//{
//    int Button = (eventArgs.Button == MouseButtons.Left) ? 1 : ((eventArgs.Button == MouseButtons.Right) ? 2 : 4);
//    int Shift = ((int)Control.ModifierKeys) / 65536;
//    float x = eventArgs.X * 15;
//    float y = eventArgs.Y * 15;
//    int newWidth = 0;
//    if ((changeColwidth - x) > 50 || (changeColwidth - x) < -50)
//    {
//        newWidth = Convert.ToInt32(txtWhere.Width * 15 - (changeColwidth - x));
//        if (newWidth < 300)
//        {
//            newWidth = 300;
//        }
//        txtWhere.Width = newWidth / 15;
//        updateFieldSystemTable(currentSql.myTable, Convert.ToString(txtWhere.Tag), "width", newWidth.ToString(), DbType.Int32);
//    }
//}

//private void txtWhere_KeyPress(Object eventSender, KeyPressEventArgs eventArgs)
//{
//    int KeyAscii = Convert.ToInt32(eventArgs.KeyChar);
//    try
//    {
//        if (KeyAscii == 13)
//        {
//            bool tempRefParam = false;
//            writeGrid(ref tempRefParam);
//        }
//    }
//    finally
//    {
//        if (KeyAscii == 0)
//        {
//            eventArgs.Handled = true;
//        }
//        eventArgs.KeyChar = Convert.ToChar(KeyAscii);
//    }
//}
//private void cmdAdd_Click(Object eventSender, EventArgs eventArgs)
//{
//    DialogResult reply = (DialogResult)0;
//    string msg = "";
//    bool found = false;
//    string fld = "", errMsg = "", str = "";
//    string[] fieldNames = null;
//    object[] fieldValues = null;
//    int newID = 0;
//    string whereClause = "", emptyComboField = "", emptyComboTable = "", fullWhereClause = "";
//    int emptyComboIndex = 0;
//    int rowsToAdd = 0;
//    UpgradeHelpers.DB.FieldHelper adoField = null;
//    OrderedDictionary tableCollection = new OrderedDictionary(System.StringComparer.OrdinalIgnoreCase);
//    frmCaptions captionsForm = null;
//    string lessRecords = "", moreRecords = "";
//    //To add, something must be visible -- because I can't add an empty record in Access?
//    if (!txtWhere.Visible && !cmbFilter[0].Visible)
//    {
//        MessageBox.Show(translation.tr("YouMustFirstOpenAfdTableDataTableAndEnterOneOfTheFieldsInInTextBoxFieldInRow", currentSql.myTable, currentSql.myTable, ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//        return;
//    }

//    //Check for empty txtWhere
//    if (txtWhere.Visible)
//    {
//        if (txtWhere.Text.Trim() == "")
//        {
//            errMsg = translation.tr("PleaseEnterAInTheTextBox", Convert.ToString(txtWhere.Tag), "", "");
//            if (txtWhere.Enabled)
//            {
//                txtWhere.Focus();
//            }
//            MessageBox.Show(errMsg, AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//            return;
//        }
//    }
//    //Check for empty cmbFilter
//    int tempForEndVar = cmbFilter.ControlCount() - 1;
//    for (int i = 0; i <= tempForEndVar; i++)
//    {
//        if (cmbFilter[i].Visible)
//        {
//            //Error if empty (unless it is an emptyComboField)
//            if (cmbFilter[i].SelectedIndex < 1)
//            {
//                //Make sure this is an emptyComboField
//                emptyComboTable = tableInCombo(i);
//                //Ensure that this is the only emptyComboField missing a value
//                //Don't allow an addAll if the txtWhere is visible - partly to simplify, partly to prevent too many records
//                //                If Not txtWhere.Visible Then
//                if (isAddAllField(currentSql.myTable, Convert.ToString(cmbFilter[i].Tag)))
//                {
//                    reply = MessageBox.Show(translation.tr("WarningThisWillAddMultipleRecordsContinue", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()), MessageBoxButtons.YesNo);
//                    if (reply == System.Windows.Forms.DialogResult.Yes)
//                    {
//                        emptyComboField = Convert.ToString(cmbFilter[i].Tag);
//                    }
//                    else
//                    {
//                        errMsg = translation.tr("PleaseSelectA", emptyComboTable, "", "");
//                    }
//                }
//                else
//                {
//                    errMsg = translation.tr("PleaseSelectA", emptyComboTable, "", "");
//                }
//                //                Else
//                //                    errMsg = tr("PleaseSelectA", emptyComboTable, "", "")
//                //                End If
//            }
//        }
//        if (errMsg != "")
//        {
//            if (cmbFilter[i].Enabled)
//            {
//                cmbFilter[i].Focus();
//            }
//            MessageBox.Show(errMsg, AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//            return;
//        }
//    }

//    //Warn if there is already an record with these id's (non-add all).
//    if (currentDR.RecordCount > 0 && cmbFilter[0].Visible && emptyComboField == "")
//    {
//        //Exit if noDoubles are allowed
//        if (noDoublesAllowed(currentSql.myTable))
//        {
//            MessageBox.Show(translation.tr("ThereIsAlreadyOneMatchingRecordAndThisTableIsMarkedNoDoubles", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//            return;
//        }
//        else
//        {
//            reply = MessageBox.Show(translation.tr("ThereIsAlreadyOneMatchingRecordDoYouWantToContinue", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()), MessageBoxButtons.YesNo);
//            if (reply == System.Windows.Forms.DialogResult.No)
//            {
//                return;
//            }
//        }
//    }

//    //ADD RECORDS

//    SqlDataReader rs = new SqlDataReader("");
//    SqlDataReader rsTemp = new SqlDataReader("");
//    SqlDataReader rsCheck = new SqlDataReader("");

//    //Open empty recordset in current table
//    string strSql = "SELECT * FROM [" + currentSql.myTable + "] WHERE " + getTablePrimaryKeyFieldSQL(currentSql.myTable) + " = 0 ";
//    rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);

//    //Set the fields that are listed in the cmbFilter and txtWhere boxes
//    //Each cmbFilter sets an ID value,
//    //This includes all the ID values in the table (except currentSql.myTable + "ID", and perhaps an empty "add multiple records" cmbFilter ID field).
//    //txtWhere sets a text field in the main table.
//    int j = -1; //The number of fields
//    int txtWhereIndex = -1; //must not be 0
//    if (txtWhere.Visible)
//    {
//        j++;
//        fieldNames = ArraysHelper.RedimPreserve(fieldNames, new int[] { 1 });
//        fieldValues = ArraysHelper.RedimPreserve(fieldValues, new int[] { 1 });
//        fieldNames[0] = Convert.ToString(txtWhere.Tag);
//        //UPGRADE_WARNING: (1068) convertValue() of type Variant is being forced to Scalar. More Information: https://docs.mobilize.net/vbuc/ewis#1068
//        fieldValues[0] = ReflectionHelper.GetPrimitiveValue(convertValue(txtWhere.Text, SqlDataReader.GetDBType(rs.GetField(Convert.ToString(txtWhere.Tag)).FieldMetadata.DataType)));
//        txtWhereIndex = 0;
//    }
//    int tempForEndVar2 = cmbFilter.ControlCount() - 1;
//    for (int i = 0; i <= tempForEndVar2; i++)
//    {
//        if (cmbFilter[i].Visible)
//        {
//            //Add field name
//            j++;
//            fieldNames = ArraysHelper.RedimPreserve(fieldNames, new int[] { j + 1 });
//            fieldValues = ArraysHelper.RedimPreserve(fieldValues, new int[] { j + 1 });
//            fieldNames[j] = Convert.ToString(cmbFilter[i].Tag);
//            //Mark the emptyComboIndex, else set fieldvalue
//            if (fieldNames[j] == emptyComboField)
//            {
//                emptyComboIndex = j;
//            }
//            else
//            {
//                str = cmbFilterBackup[i].GetListItem(cmbFilter[i].SelectedIndex);
//                //UPGRADE_WARNING: (1068) convertValue() of type Variant is being forced to Scalar. More Information: https://docs.mobilize.net/vbuc/ewis#1068
//                fieldValues[j] = ReflectionHelper.GetPrimitiveValue(convertValue(str, DbType.Int32));
//            }
//        }
//    }

//    //Get number of rows to be added
//    if (emptyComboField != "")
//    {
//        //Ask if they want to add one record for each element of the empty combo, or
//        //if they want to have add only missing values (i.e. values that are missing for the current cmbFilter values).
//        lessRecords = "For each " + fieldNames[emptyComboIndex] + " only allow one record in entire table.";
//        moreRecords = "For each " + fieldNames[emptyComboIndex] + " create a record for current cmbFilter values.";
//        tableCollection.Add(Guid.NewGuid().ToString(), lessRecords);
//        tableCollection.Add(Guid.NewGuid().ToString(), moreRecords);
//        captionsForm = frmCaptions.CreateInstance();
//        captionsForm.tableCollection = tableCollection;
//        captionsForm.job = "cmdAdd";
//        captionsForm.Text = "Add multiple records";
//        //UPGRADE_ISSUE: (2064) Void method Global.Load was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //VB.Global.Load(captionsForm);
//        captionsForm.ShowDialog();
//        if (captionsForm.selectedCaption == "")
//        {
//            captionsForm = null;
//            return;
//        }
//        else
//        {
//            if (captionsForm.selectedCaption == lessRecords)
//            {
//                lessRecords = "True";
//            }
//            else if (captionsForm.selectedCaption == moreRecords)
//            {
//                moreRecords = "True";
//            }
//        }
//        captionsForm.tableCollection = null;
//        captionsForm = null;

//        //Get number of rows to create
//        strSql = "SELECT * FROM [" + emptyComboTable + "]";
//        rsTemp.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//        if (moreRecords == "True")
//        {
//            rowsToAdd = rsTemp.RecordCount;
//        }
//        else
//        {
//            //Count records to add - same routine as used below
//            rowsToAdd = 0;
//            while (!rsTemp.EOF)
//            {
//                found = false; //Default.  May be changed to true below
//                               //Look for this value in the current record set
//                if (currentDR.RecordCount > 0)
//                {
//                    currentDR.MoveFirst();
//                }
//                while (!currentDR.EOF)
//                {
//                    if (lngValue(currentDR, emptyComboField) == lngValue(rsTemp, getTablePrimaryKeyField(emptyComboTable)))
//                    {
//                        found = true;
//                    }
//                    currentDR.MoveNext();
//                }
//                if (currentDR.RecordCount > 0)
//                {
//                    currentDR.MoveFirst(); //Return to start in case used elsewhere
//                }
//                if (!found)
//                {
//                    rowsToAdd++;
//                }
//                rsTemp.MoveNext();
//            }
//            //Gets the distinct elements of emptyComboTable found in currentSql.myTable
//            //            strSql = "SELECT COUNT (Distinct [" & fieldNames(emptyComboIndex) & "]) AS MyCount FROM [" & currentSql.myTable & "]"
//            //            rsTemp.Open strSql, cn, adOpenKeyset, adLockOptimistic
//            //            rowsToAdd = addAllTableRows - rsTemp.Fields.Item("MyCount")
//            //            rsTemp.Close
//        }
//        rsTemp.Close();

//    }
//    else
//    {
//        rowsToAdd = 1;
//    }

//    //Ask for a final confirmation
//    reply = MessageBox.Show(translation.tr("AddATotalOfRecords", rowsToAdd.ToString(), "", "") + Environment.NewLine + msg, AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()), MessageBoxButtons.OKCancel);

//    if (reply == System.Windows.Forms.DialogResult.OK)
//    {
//        //Increase the list of fieldnames to include all fields that don't allow null elements
//        foreach (UpgradeHelpers.DB.FieldHelper adoFieldIterator in rs.Fields)
//        {
//            adoField = adoFieldIterator;
//            //UPGRADE_ISSUE: (2064) ADODB.field property adoField.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//            //UPGRADE_ISSUE: (2064) ADODB.Properties property Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//            //UPGRADE_ISSUE: (2064) ADODB.Property property Properties.Value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//            if (isNullable(adoField))
//            {
//                //Field can be null
//            }
//            else if (Convert.ToBoolean(adoField.getProperties().Item("ISAUTOINCREMENT").getValue()))
//            {
//                //Auto increment
//            }
//            else
//            {
//                //Check if field is already in the array
//                found = false;
//                int tempForEndVar3 = fieldNames.GetUpperBound(0);
//                for (j = 0; j <= tempForEndVar3; j++)
//                {
//                    if (fieldNames[j] == adoField.FieldMetadata.ColumnName)
//                    {
//                        found = true;
//                    }
//                }
//                //Must add field the field list
//                if (!found)
//                {
//                    //Check to see it is not an auto field
//                    j = fieldNames.GetUpperBound(0);
//                    fieldNames = ArraysHelper.RedimPreserve(fieldNames, new int[] { j + 2 });
//                    fieldValues = ArraysHelper.RedimPreserve(fieldValues, new int[] { j + 2 });
//                    fieldNames[j + 1] = adoField.FieldMetadata.ColumnName;
//                    //UPGRADE_WARNING: (1068) defaultValue() of type Variant is being forced to Scalar. More Information: https://docs.mobilize.net/vbuc/ewis#1068
//                    fieldValues[j + 1] = ReflectionHelper.GetPrimitiveValue(defaultValue(SqlDataReader.GetDBType(adoField.FieldMetadata.DataType)));
//                }
//            }
//            adoField = default(UpgradeHelpers.DB.FieldHelper);
//        }

//        //Regular add
//        if (emptyComboField == "")
//        {
//            //UPGRADE_WARNING: (2065) ADODB.Recordset method rs.AddNew has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2065
//            rs.AddNew(ArraysHelper.CastArray<object[]>(fieldNames), fieldValues);
//            rs.Update();
//            writeLog("Add", rs);
//            //Add all - uses
//        }
//        else
//        {
//            //Select all elements from emptyComboTable
//            strSql = "SELECT * FROM [" + emptyComboTable + "]";
//            rsTemp.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);

//            while (!rsTemp.EOF)
//            {
//                //Set emptyComboIndex value
//                newID = lngValue(rsTemp, getTablePrimaryKeyField(emptyComboTable));
//                fieldValues[emptyComboIndex] = lngValue(rsTemp, getTablePrimaryKeyField(emptyComboTable));
//                //Decide whether to add
//                if (moreRecords == "True")
//                {
//                    found = false; //This will add the record
//                }
//                else
//                {
//                    found = false; //Default.  May be changed to true below
//                                   //Look for this value in the current record set
//                    if (currentDR.RecordCount > 0)
//                    {
//                        currentDR.MoveFirst();
//                    }
//                    while (!currentDR.EOF)
//                    {
//                        if (lngValue(currentDR, emptyComboField) == lngValue(rsTemp, getTablePrimaryKeyField(emptyComboTable)))
//                        {
//                            found = true;
//                        }
//                        currentDR.MoveNext();
//                    }
//                    if (currentDR.RecordCount > 0)
//                    {
//                        currentDR.MoveFirst();
//                    }
//                }
//                //Add the record if newID
//                if (!found)
//                {
//                    //UPGRADE_WARNING: (2065) ADODB.Recordset method rs.AddNew has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2065
//                    rs.AddNew(ArraysHelper.CastArray<object[]>(fieldNames), fieldValues);
//                    rs.Update();
//                    writeLog("Add", rs);
//                }
//                rsTemp.MoveNext();
//            }
//        }
//    }
//    rs.Close();
//    rs = null;
//    rsTemp = null;
//    rsCheck = null;

//    bool tempRefParam = false;
//    writeGrid(ref tempRefParam);



//}
//private void cmbFilter_DropDown(Object eventSender, EventArgs eventArgs)
//{
//    int Index = Array.IndexOf(this.cmbFilter, eventSender);
//    tools.setComboHeight(Index); //This will call cmbFilter_DropDown -- but with updating = true

//}
//private void cmbFilter_SelectedIndexChanged(Object eventSender, EventArgs eventArgs)
//{
//    int Index = Array.IndexOf(this.cmbFilter, eventSender);
//    string currentFilterTable = "", originalFilterTable = "";
//    int myIndex = 0;
//    string myTable = "";
//    int myValue = 0;
//    int iIndex = 0;
//    string iTable = "";
//    int iValue = 0;
//    int newFilterIndex = 0;

//    if (!updating)
//    {
//        if (mnuOptionsUseTableFilters.Checked)
//        {
//            //Set variables
//            myTable = getForeignTable(currentSql.myTable, Convert.ToString(cmbFilter[Index].Tag));
//            myIndex = cmbFilter[Index].SelectedIndex;
//            myValue = Convert.ToInt32(Double.Parse(cmbFilterBackup[Index].GetListItem(myIndex)));
//            originalFilterTable = sqlCurrent.mytableFilterTable();
//            currentFilterTable = originalFilterTable;
//            if (myTable == currentFilterTable)
//            {
//                //Set filter value if this is the filter table
//                updateTableSystemTable(myTable, "filterID", myValue.ToString(), DbType.Int32);
//                if (myIndex == 0)
//                {
//                    //Set all cmbFilter filterID's -- later we will pick one as the new index
//                    int tempForEndVar = cmbFilter.ControlCount() - 1;
//                    for (int i = 0; i <= tempForEndVar; i++)
//                    {
//                        if (cmbFilter[i].Visible)
//                        {
//                            iTable = getForeignTable(currentSql.myTable, Convert.ToString(cmbFilter[i].Tag));
//                            iIndex = cmbFilter[i].SelectedIndex;
//                            if (iIndex > -1)
//                            {
//                                iValue = Convert.ToInt32(Double.Parse(cmbFilterBackup[i].GetListItem(iIndex)));
//                            }
//                            else
//                            {
//                                iValue = 0;
//                            }
//                            updateTableSystemTable(iTable, "filterID", iValue.ToString(), DbType.Int32);
//                        }
//                    }
//                }
//            }
//            else if (currentFilterTable == "" && myIndex > 0)
//            {
//                //Set this value
//                updateTableSystemTable(myTable, "filterID", myValue.ToString(), DbType.Int32);
//            }

//            //Redo combos
//            currentFilterTable = sqlCurrent.mytableFilterTable(); //May have changed
//                                                                  //Find it and set value
//            newFilterIndex = -1;
//            if (currentFilterTable != "")
//            {
//                int tempForEndVar2 = cmbFilter.ControlCount() - 1;
//                for (int i = 0; i <= tempForEndVar2; i++)
//                {
//                    if (cmbFilter[i].Visible)
//                    {
//                        if (currentFilterTable == getForeignTable(currentSql.myTable, Convert.ToString(cmbFilter[i].Tag)))
//                        {
//                            newFilterIndex = i;
//                        }
//                    }
//                }
//            }
//            //Three possibilites
//            if (newFilterIndex > -1)
//            {
//                if (currentFilterTable == originalFilterTable)
//                {
//                    //A new index for the filter table
//                    int tempForEndVar3 = cmbFilter.ControlCount() - 1;
//                    for (int i = 0; i <= tempForEndVar3; i++)
//                    {
//                        if (cmbFilter[i].Visible)
//                        {
//                            if (i != myIndex)
//                            {
//                                myTable = getForeignTable(currentSql.myTable, Convert.ToString(cmbFilter[i].Tag));
//                                myIndex = cmbFilter[i].SelectedIndex;
//                                if (myIndex > -1)
//                                {
//                                    myValue = Convert.ToInt32(Double.Parse(cmbFilterBackup[i].GetListItem(myIndex)));
//                                }
//                                else
//                                {
//                                    myValue = 0;
//                                }
//                                fillComboWhere(i);
//                                if (myValue > 0)
//                                {
//                                    findInCombos(myTable, myValue.ToString());
//                                }
//                            }
//                        }
//                    }
//                }
//                else
//                {
//                    //New filter table and index
//                    myIndex = cmbFilter[newFilterIndex].SelectedIndex;
//                    if (myIndex > -1)
//                    {
//                        myValue = Convert.ToInt32(Double.Parse(cmbFilterBackup[newFilterIndex].GetListItem(myIndex)));
//                    }
//                    else
//                    {
//                        myValue = 0;
//                    }
//                    int tempForEndVar4 = cmbFilter.ControlCount() - 1;
//                    for (int i = 0; i <= tempForEndVar4; i++)
//                    {
//                        if (cmbFilter[i].Visible)
//                        {
//                            fillComboWhere(i); //New index will change content
//                        }
//                    }
//                    if (myValue > 0)
//                    {
//                        findInCombos(currentFilterTable, myValue.ToString());
//                    }
//                }
//            }
//            else
//            {
//                //No filter table - redo everything
//                int tempForEndVar5 = cmbFilter.ControlCount() - 1;
//                for (int i = 0; i <= tempForEndVar5; i++)
//                {
//                    if (cmbFilter[i].Visible)
//                    {
//                        myTable = getForeignTable(currentSql.myTable, Convert.ToString(cmbFilter[i].Tag));
//                        myIndex = cmbFilter[i].SelectedIndex;
//                        if (myIndex > -1)
//                        {
//                            myValue = Convert.ToInt32(Double.Parse(cmbFilterBackup[i].GetListItem(myIndex)));
//                        }
//                        else
//                        {
//                            myValue = 0;
//                        }
//                        fillComboWhere(i);
//                        if (myValue > 0)
//                        {
//                            findInCombos(myTable, myValue.ToString());
//                        }
//                    }
//                }
//            }
//        }
//        bool tempRefParam = false;
//        writeGrid(ref tempRefParam);
//    }
//}
////UPGRADE_WARNING: (2050) MSHierarchicalFlexGridLib.MSHFlexGrid Event dataGridView1.EnterCell was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2050
//private void flexGrid_EnterCell()
//{
//    int i = 0;
//    string msg = "", baseTable = "", father = "";
//    string fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//    //Change color to indicate that this is selected
//    if (!blueColumn() && !header())
//    {
//        msgSB(fld + " - " + Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].Value));
//        dataGridView1.CurrentCell.Style.ForeColor = Color.White;
//        dataGridView1.CurrentCell.Style.BackColor = Color.Blue;
//        //Get basetable of the fld
//        //fld = colCaption(dataGridView1.col)
//        //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//    }
//    else
//    {
//        msgSB(fld);
//    }
//}
//private void flexGrid_KeyPress(Object eventSender, KeyPressEventArgs eventArgs)
//{
//    int KeyAscii = Convert.ToInt32(eventArgs.KeyChar);
//    try
//    {
//        //Special keys
//        switch (KeyAscii)
//        {
//            case 13:  //Return 
//                flexGrid_DoubleClick(flexGrid, new EventArgs());
//                break;
//            case 2:  //cntl B 
//                mnuFindInCombos_Click(mnuFindInCombos, new EventArgs());
//                break;
//            case 3:  //cnt C 
//                     //UPGRADE_WARNING: (2081) Clipboard.SetText has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2081 
//                Clipboard.SetText(Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].Value));
//                break;
//            case 6:  //cntl F 
//                mnuFather_Click(mnuFather, new EventArgs());
//                break;
//            case 19:  //cntl S 
//                mnuSon_Click(mnuSon, new EventArgs());
//                break;
//            case 26:  //cntl z 
//                      //Call temp - used for testing 
//                break;
//        }
//    }
//    finally
//    {
//        if (KeyAscii == 0)
//        {
//            eventArgs.Handled = true;
//        }
//        eventArgs.KeyChar = Convert.ToChar(KeyAscii);
//    }
//}
//private void flexGrid_CellLeave(Object eventSender, EventArgs eventArgs)
//{
//    //Redo the color of the cell
//    msgSB("");
//    if (!blueColumn() && !header())
//    {
//        dataGridView1.CurrentCell.Style.ForeColor = dataGridView1.ForeColor;
//        dataGridView1.CurrentCell.Style.BackColor = dataGridView1.DefaultCellStyle.BackColor;
//    }
//}
//private void flexGrid_CellEnter(Object eventSender, EventArgs eventArgs)
//{
//    //Get rid of these on a wide variety of circumstances
//    listNewData.Visible = false;
//    txtNewData.Visible = false;
//}
//private void flexgrid_Scroll(Object eventSender, EventArgs eventArgs)
//{
//    //Get rid of these on a wide variety of circumstances
//    listNewData.Visible = false;
//    txtNewData.Visible = false;
//    txtNewData.Tag = "";
//}
//private void flexGrid_DoubleClick(Object eventSender, EventArgs eventArgs)
//{
//    string msg = "";
//    string reply = "", fld = "";
//    int Index = 0;
//    SqlDataReader rs = null;
//    string strSql = "", baseTable = "";
//    //Set up the listNewData box or a reply box
//    msgSB("");
//    //Make sure you are not in the 1st (blue) column, and that the chart is not empty (empty chart if only 1 row -- the header)
//    if (!blueColumn() && dataGridView1.RowCount > 1)
//    {
//        //Get baseTable of the element
//        fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//        //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//        //sort if you are in the header row
//        if (header())
//        {
//            //Add this because in odd cases dataGridView1.row = 0 but dataGridView1.rowsel = row doubled-clicked ?
//            if (dataGridView1.CurrentCell.RowIndex == 0)
//            {
//                sqlCurrent.sqlOrderBy(baseTable, short_Renamed(fld), SqlDataReader.GetDBType(currentDR.GetField(fld).FieldMetadata.DataType));
//                bool tempRefParam = true;
//                writeGrid(ref tempRefParam);
//            }
//            //Updatable fld in the currentSql.myTable
//        }
//        else if (fieldUpdatable(colCaption(dataGridView1.CurrentCell.ColumnIndex)))
//        {
//            DbType switchVar = SqlDataReader.GetDBType(currentDR.GetField(fld).FieldMetadata.DataType);
//            if (switchVar == DbType.Int32 || switchVar == DbType.Int16 || switchVar == DbType.Single || switchVar == DbType.Double || switchVar == DbType.Currency || switchVar == DbType.DateTime || switchVar == DbType.Time || switchVar == DbType.DateTime || switchVar == DbType.DateTime)
//            {
//                txtNewData.Text = Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].Value);
//                formatTxtBox(dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex, SqlDataReader.GetDBType(currentDR.GetField(fld).FieldMetadata.DataType));
//            }
//            else if (switchVar == DbType.Boolean)
//            {  //Boolean -- field is in currentSql.myTable
//                listNewData.Items.Clear();
//                listNewData.AddItem("True");
//                listNewData.AddItem("False");
//                listNewDataBackup.Items.Clear();
//                listNewDataBackup.AddItem("True");
//                listNewDataBackup.AddItem("False");
//                listNewData.Text = flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].FormattedValue.ToString(); //select the text if possible
//                listNewData.Tag = "updateFld";
//                formatList(dataGridView1.CurrentCell.ColumnIndex, dataGridView1.CurrentCell.RowIndex, "");
//            }
//            else if (switchVar == DbType.String || switchVar == DbType.String || switchVar == DbType.String || switchVar == DbType.String || switchVar == DbType.String || switchVar == DbType.String)
//            {  //text
//                txtNewData.Text = Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].Value);
//                txtNewData.Tag = dataGridView1.CurrentCell.RowIndex.ToString() + "." + dataGridView1.CurrentCell.ColumnIndex.ToString();
//                //Change for an empty
//                if (isTranscript)
//                {
//                    if (currentSql.myTable == "????")
//                    {
//                        if (fld == "????")
//                        {
//                            if (txtNewData.Text == "")
//                            {
//                                txtNewData.Text = transcript.getSchoolNumber("student");
//                            }
//                        }
//                    }
//                    else if (currentSql.myTable == "??")
//                    {
//                        if (fld == "????")
//                        {
//                            if (txtNewData.Text == "")
//                            {
//                                txtNewData.Text = transcript.getSchoolNumber("teacher");
//                            }
//                        }
//                    }
//                    else if (currentSql.myTable == "???")
//                    {
//                        if (fld == "?????")
//                        {
//                            if (txtNewData.Text == "")
//                            {
//                                txtNewData.Text = transcript.getSchoolNumber("nonStudent");
//                            }
//                        }
//                    }
//                }
//                formatTxtBox(dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex, SqlDataReader.GetDBType(currentDR.GetField(fld).FieldMetadata.DataType));
//            }
//            else
//            {
//                this.ClientSize.msgSB(translation.tr("FieldHasAnUnrecognizedType", fld, ((int)SqlDataReader.GetDBType(currentDR.GetField(fld).FieldMetadata.DataType)).ToString(), ""));
//            }
//        }
//        else if (idFieldUpdatable(colCaption(dataGridView1.CurrentCell.ColumnIndex)))
//        {
//            //Basetable will be a son of the currentSql.myTable
//            rs = new SqlDataReader("");
//            listNewData.Items.Clear();
//            listNewDataBackup.Items.Clear();
//            //Fill if with the combo table
//            //Get the index of the combo
//            int tempForEndVar = cmbFilter.ControlCount() - 1;
//            for (int i = 0; i <= tempForEndVar; i++)
//            {
//                if (cmbFilter[i].Visible && tableInCombo(i) == baseTable)
//                {
//                    Index = i;
//                }
//            }
//            int tempForEndVar2 = cmbFilter[Index].Items.Count - 1;
//            for (int i = 1; i <= tempForEndVar2; i++)
//            {
//                listNewData.AddItem(cmbFilter[Index].GetListItem(i));
//                listNewDataBackup.AddItem(cmbFilterBackup[Index].GetListItem(i));
//            }
//            rs = null;
//            listNewData.Text = flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].FormattedValue.ToString(); //select the text if possible
//            listNewData.Tag = "updateID";
//            formatList(dataGridView1.CurrentCell.ColumnIndex, dataGridView1.CurrentCell.RowIndex, baseTable); //shows the listNewData
//        }
//        else
//        {
//            mnuFindInCombos_Click(mnuFindInCombos, new EventArgs());
//        }
//    }
//}
//private void listNewData_KeyPress(Object eventSender, KeyPressEventArgs eventArgs)
//{
//    int KeyAscii = Convert.ToInt32(eventArgs.KeyChar);
//    try
//    {
//        if (KeyAscii == 13)
//        {
//            listNewData_DoubleClick(listNewData, new EventArgs());
//        }
//    }
//    finally
//    {
//        if (KeyAscii == 0)
//        {
//            eventArgs.Handled = true;
//        }
//        eventArgs.KeyChar = Convert.ToChar(KeyAscii);
//    }
//}
//private void listNewData_Leave(Object eventSender, EventArgs eventArgs)
//{
//    listNewData.Visible = false;
//    txtNewData.Visible = false;
//    txtNewData.Tag = "";
//}
//private void listNewData_DoubleClick(Object eventSender, EventArgs eventArgs)
//{
//    string fld = "", backupListValue = "", oldValue = "";
//    int c = 0;
//    string listValue = "", strSql = "";
//    int Index = 0;
//    object vnt = null;
//    string baseTable = "";
//    int selectedRecordID = 0, currentTableRecordID = 0;
//    string strNewRecord = "", strOrgRecord = "", msg = "";
//    int flexGridRow = 0;
//    SqlDataReader rs = new SqlDataReader("");
//    if (ListBoxHelper.GetSelectedIndex(listNewData) > -1)
//    {
//        flexGridRow = dataGridView1.RowCount;
//        if (dataGridView1.Enabled)
//        {
//            dataGridView1.Focus();
//        }
//        listNewData.Visible = false;
//        fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//        currentTableRecordID = Convert.ToInt32(Double.Parse(Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, colField(getTablePrimaryKeyField(currentSql.myTable))].Value)));
//        //List value is the string of the chosen item in the list
//        listValue = listNewData.GetListItem(ListBoxHelper.GetSelectedIndex(listNewData));
//        //Backup list value is the index of the chosen item in its basetable
//        backupListValue = listNewDataBackup.GetListItem(ListBoxHelper.GetSelectedIndex(listNewData));
//        //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//        //Update the given fld -- a boolean
//        if (Convert.ToString(listNewData.Tag) == "updateFld")
//        {
//            if (listValue != flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].FormattedValue.ToString())
//            {
//                //Get ID of the selected record -- this should always exist
//                strSql = "SELECT * FROM [" + currentSql.myTable + "] WHERE " + getTablePrimaryKeyFieldSQL(currentSql.myTable) + " = " + currentTableRecordID.ToString();
//                rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//                //UPGRADE_WARNING: (1068) convertValue() of type Variant is being forced to Scalar. More Information: https://docs.mobilize.net/vbuc/ewis#1068
//                vnt = ReflectionHelper.GetPrimitiveValue(convertValue(backupListValue, SqlDataReader.GetDBType(rs.GetField(fld).FieldMetadata.DataType)));
//                //ConvertValue may return a message beginning with "ERROR!"
//                if (ReflectionHelper.GetPrimitiveValue<string>(vnt).StartsWith("ERROR! "))
//                {
//                    MessageBox.Show(ReflectionHelper.GetPrimitiveValue<string>(vnt), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//                }
//                else
//                {
//                    //Ask with a warning before you update
//                    if (userApprovedUpdate(currentSql.myTable, fld, ""))
//                    {
//                        if (ReflectionHelper.GetPrimitiveValue<string>(vnt) == "")
//                        {
//                            //UPGRADE_WARNING: (1049) Use of Null/IsNull() detected. More Information: https://docs.mobilize.net/vbuc/ewis#1049
//                            vnt = DBNull.Value;
//                        }
//                        writeLog("Before", rs);
//                        rs[fld] = vnt;
//                        rs.Update();
//                        writeLog("After", rs);
//                        rs.Close();
//                        bool tempRefParam = false;
//                        writeGrid(ref tempRefParam);
//                        if (dataGridView1.RowCount > flexGridRow && flexGridRow > 0)
//                        {
//                            dataGridView1.FirstDisplayedScrollingRowIndex = flexGridRow;
//                        }
//                    }
//                }
//            }
//        }
//        else
//        {
//            //Get ID field to update
//            fld = getForeignKeyFieldName(currentSql.myTable, baseTable);
//            selectedRecordID = Convert.ToInt32(Double.Parse(Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, colField(fld)].Value)));
//            //update the ID of the field (if different values)
//            if (Convert.ToInt32(Double.Parse(backupListValue)) != selectedRecordID && Convert.ToInt32(Double.Parse(backupListValue)) != 0)
//            {
//                //Get the two record strings - loop through the combo looking for it
//                //Get the index of the combo
//                int tempForEndVar = cmbFilter.ControlCount() - 1;
//                for (int i = 0; i <= tempForEndVar; i++)
//                {
//                    if (cmbFilter[i].Visible && tableInCombo(i) == baseTable)
//                    {
//                        Index = i;
//                    }
//                }
//                //Look for org value in the combos - Used for original string only --
//                //May not be present because combo may be limited
//                int tempForEndVar2 = cmbFilter[Index].Items.Count - 1;
//                for (int i = 0; i <= tempForEndVar2; i++)
//                {
//                    if (StringsHelper.ToDoubleSafe(cmbFilterBackup[Index].GetListItem(i)) == selectedRecordID)
//                    {
//                        strOrgRecord = cmbFilter[Index].GetListItem(i);
//                    }
//                    if (cmbFilterBackup[Index].GetListItem(i) == backupListValue)
//                    {
//                        strNewRecord = cmbFilter[Index].GetListItem(i);
//                    }
//                }
//                //Prepare to substitute in currentSql.myTable
//                strSql = "SELECT * FROM [" + currentSql.myTable + "] WHERE " + getTablePrimaryKeyFieldSQL(currentSql.myTable) + " = " + currentTableRecordID.ToString();
//                rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//                //UPGRADE_WARNING: (1068) convertValue() of type Variant is being forced to Scalar. More Information: https://docs.mobilize.net/vbuc/ewis#1068
//                vnt = ReflectionHelper.GetPrimitiveValue(convertValue(backupListValue, SqlDataReader.GetDBType(rs.GetField(fld).FieldMetadata.DataType)));
//                if (ReflectionHelper.GetPrimitiveValue<string>(vnt).StartsWith("ERROR! "))
//                {
//                    MessageBox.Show(ReflectionHelper.GetPrimitiveValue<string>(vnt), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//                }
//                else
//                {
//                    //Make substitution
//                    if (strNewRecord == "")
//                    { //New record not in cmbFilter
//                        msg = "Replace: " + strOrgRecord + Environment.NewLine + Environment.NewLine + "New: " + listValue; //Give value in list box only
//                    }
//                    else
//                    {
//                        msg = "Org: " + strOrgRecord + Environment.NewLine + Environment.NewLine + "New: " + strNewRecord; //New is value in cmbFilter box
//                    }
//                    if (userApprovedUpdate(baseTable, "", msg))
//                    {
//                        if (ReflectionHelper.GetPrimitiveValue<string>(vnt) == "")
//                        {
//                            //UPGRADE_WARNING: (1049) Use of Null/IsNull() detected. More Information: https://docs.mobilize.net/vbuc/ewis#1049
//                            vnt = DBNull.Value;
//                        }
//                        writeLog("Before", rs);
//                        rs[fld] = vnt;
//                        rs.Update();
//                        writeLog("After", rs);
//                        rs.Close();
//                        bool tempRefParam2 = false;
//                        writeGrid(ref tempRefParam2);
//                        if (dataGridView1.RowCount > flexGridRow)
//                        {
//                            dataGridView1.FirstDisplayedScrollingRowIndex = flexGridRow;
//                        }
//                    }
//                }
//            }
//        }
//    }
//    rs = null;
//}
//private void txtNewData_DoubleClick(Object eventSender, EventArgs eventArgs)
//{
//    txtNewData_KeyPress(txtNewData, new KeyPressEventArgs(Convert.ToChar(13)));
//}
//private void txtNewData_Leave(Object eventSender, EventArgs eventArgs)
//{
//    if (Convert.ToString(txtNewData.Tag) == dataGridView1.CurrentCell.RowIndex.ToString() + "." + dataGridView1.CurrentCell.ColumnIndex.ToString())
//    {
//        txtNewData_DoubleClick(txtNewData, new EventArgs());
//    }
//    txtNewData.Visible = false;
//}
//private void txtNewData_KeyPress(Object eventSender, KeyPressEventArgs eventArgs)
//{
//    int KeyAscii = Convert.ToInt32(eventArgs.KeyChar);
//    try
//    {
//        string fld = "", errorMsg = "";
//        object vnt = null;
//        int[] idArray = new int[1];
//        string baseTable = "";
//        int reply = 0, flexGridRow = 0;
//        int p = 0;
//        SqlDataReader rs = null;
//        string strSql = "";
//        int selectedRecordID = 0;
//        //On Error GoTo errHandler:
//        //Update data on return
//        if (KeyAscii == 13)
//        {
//            if (txtNewData.Text != flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].FormattedValue.ToString())
//            {
//                flexGridRow = dataGridView1.CurrentCell.RowIndex;
//                fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//                //Get ID of selected item -- this should always exist
//                selectedRecordID = Convert.ToInt32(Double.Parse(Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, colField(getTablePrimaryKeyField(currentSql.myTable))].Value)));
//                rs = new SqlDataReader("");
//                strSql = "SELECT * FROM [" + currentSql.myTable + "] WHERE " + getTablePrimaryKeyFieldSQL(currentSql.myTable) + " = " + selectedRecordID.ToString();
//                //rs should always have one item
//                rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//                //UPGRADE_WARNING: (1068) convertValue() of type Variant is being forced to Scalar. More Information: https://docs.mobilize.net/vbuc/ewis#1068
//                vnt = ReflectionHelper.GetPrimitiveValue(convertValue(txtNewData.Text, SqlDataReader.GetDBType(rs.GetField(fld).FieldMetadata.DataType)));
//                if (ReflectionHelper.GetPrimitiveValue<string>(vnt).StartsWith("ERROR! "))
//                {
//                    MessageBox.Show(ReflectionHelper.GetPrimitiveValue<string>(vnt), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//                }
//                else
//                {
//                    //UPGRADE_ISSUE: (2064) ADODB.Field property rs.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//                    //UPGRADE_ISSUE: (2064) ADODB.Properties property Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//                    //UPGRADE_ISSUE: (2064) ADODB.Property property Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//                    baseTable = Convert.ToString(rs.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//                    if (userApprovedUpdate(baseTable, fld, ""))
//                    {
//                        //Can cause an error for msgbox and modal forms in exe files only
//                        //So do this after the msgbox is called.
//                        dataGridView1.Focus();
//                        txtNewData.Visible = false;
//                        if (ReflectionHelper.GetPrimitiveValue<string>(vnt) == "")
//                        {
//                            //UPGRADE_WARNING: (1049) Use of Null/IsNull() detected. More Information: https://docs.mobilize.net/vbuc/ewis#1049
//                            vnt = DBNull.Value;
//                        }
//                        writeLog("Before", rs);
//                        rs[fld] = vnt;
//                        rs.Update();
//                        writeLog("After", rs);
//                        bool tempRefParam = false;
//                        writeGrid(ref tempRefParam);
//                        //UPGRADE_TODO: (1069) Error handling statement (On Error Resume Next) was converted to a pattern that might have a different behavior. More Information: https://docs.mobilize.net/vbuc/ewis#1069
//                        try
//                        { //Row may be deleted (as by answersToSummaryTable)
//                            if (dataGridView1.RowCount > flexGridRow && flexGridRow > 0)
//                            {
//                                dataGridView1.FirstDisplayedScrollingRowIndex = flexGridRow;
//                            }
//                        }
//                        catch (Exception exc)
//                        {
//                            NotUpgradedHelper.NotifyNotUpgradedElement("Resume in On-Error-Resume-Next Block");
//                        }
//                    }
//                }
//                rs.Close();
//                rs = null;
//            }
//        }
//        if (KeyAscii == 0)
//        {
//            eventArgs.Handled = true;
//        }
//        return;

//        MessageBox.Show(translation.tr("ErrorPerhapsTheInformationYouEnterIsOfTheWrongTypeOrIsTooLongOrBig", "", "", "") + Environment.NewLine + translation.tr("Details", "", "", "") + Information.Err().Description + " See txtNewData-KeyPress", AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//        rs = null;
//    }
//    finally
//    {
//        if (KeyAscii == 0)
//        {
//            eventArgs.Handled = true;
//        }
//        eventArgs.KeyChar = Convert.ToChar(KeyAscii);
//    }
//}
#endregion

#region OLD Various Properties
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////VARIOUS PROPERTIES
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
//private bool entireRowsSelected()
//{
//    int j = 0;
//    //Get the number of visible columns.  Will be less than number of fields in table because ID columns are not visible.
//    int tempForEndVar = dataGridView1.ColumnCount - 1;
//    for (int i = 0; i <= tempForEndVar; i++)
//    {
//        if (dataGridView1.Columns[i].Width > 0)
//        {
//            j = i;
//        }
//    }
//    //One Row selected -- the header (header) is ignored because can't select complete width and header
//    //return dataGridView1.CurrentCell.ColumnIndex == 1 && dataGridView1.ColSel >= j;
//    return dataGridView1.CurrentCell.ColumnIndex == 1 && dataGridView1.SelectedRows.Count > 0;
//}
//private bool oneCellInTableChoosen()
//{
//    bool result = false;
//    if (!blueColumn() && !header())
//    {
//        if (dataGridView1.CurrentCell.RowIndex == dataGridView1.SelectedCells[1].RowIndex && dataGridView1.CurrentCell.ColumnIndex == dataGridView1.SelectedCells[1].ColumnIndex)
//        {
//            result = true;
//        }
//    }
//    return result;
//}

//private string pathDownTo(string tableName, string fld)
//{
//    //Return the immediate father of fld if there is a path from the tableName down to the fld, else return "".
//    //The fld may or may not begin with a tableName and dot
//    string result = "";
//    string son = "";
//    //Select all fields that are in the table
//    SqlDataReader rs = new SqlDataReader("");
//    string strSql = "SELECT * FROM afdFieldData WHERE tableName = '" + tableName + "'";
//    rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//    //If this field if fld, we are finished -- return tableName
//    while (!rs.EOF)
//    {
//        //If the field in this table return tableName
//        if (short_Renamed(fld) == Convert.ToString(rs["fieldName"]))
//        {
//            result = tableName;
//            goto cleanup;
//        }
//        rs.MoveNext();
//    }

//    //Recursive step
//    if (rs.RecordCount != 0)
//    {
//        rs.MoveFirst();
//    }
//    while (!rs.EOF)
//    {
//        //If is an ID field, see if there is a path from this son table to fld
//        if (fieldIsForeignKey(tableName, Convert.ToString(rs["fieldName"])))
//        {
//            son = getForeignTable(tableName, Convert.ToString(rs["fieldName"]));
//            //Will return empty if there is no path, or the step above fld if there is
//            result = pathDownTo(son, fld);
//            if (result != "")
//            {
//                goto cleanup;
//            }
//        }
//        rs.MoveNext();
//    }
//cleanup:
//    rs.Close();
//    return result;
//}
//private void clearTopBoxes()
//{
//    //Sets contents of topBoxes to "".
//    updating = true;
//    txtWhere.Text = "";
//    int tempForEndVar = cmbFilter.ControlCount() - 1;
//    for (int i = 0; i <= tempForEndVar; i++)
//    {
//        if (cmbFilter[i].Visible)
//        {
//            if (cmbFilter[i].Items.Count > 0)
//            {
//                cmbFilter[i].SelectedIndex = 0;
//            }
//        }
//    }
//    updating = false;
//}
//private bool isAddAllField(string table, string tableField)
//{
//    bool result = false;
//    //On Error GoTo Errhandler
//    SqlDataReader rs = new SqlDataReader("");
//    string strSql = "SELECT * FROM afdFieldData WHERE tableName = '" + table + "' AND fieldName = '" + tableField + "'";
//    rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//    result = Convert.ToBoolean(rs["addAll"]);
//    rs.Close();
//    return result;

//    MessageBox.Show(translation.tr("ErrorItSeemsYourSystemFilesAreMissingARowTryRewritingThem", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    return result;
//}
//private bool noDoublesAllowed(string table)
//{
//    bool result = false;
//    //On Error GoTo Errhandler
//    SqlDataReader rs = new SqlDataReader("");
//    string strSql = "SELECT * FROM afdTableData WHERE tableName = '" + table + "' AND noDouble = " + getSqlTrue();
//    rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//    result = !rs.EOF;
//    rs.Close();
//    return result;

//    MessageBox.Show(translation.tr("ErrorItSeemsYourSystemFilesAreMissingARowTryRewritingThem", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    return result;
//}
//private bool insertInTextBox(string fld, string value)
//{
//    bool result = false;
//    if (txtWhere.Visible)
//    {
//        if (Convert.ToString(txtWhere.Tag) == fld)
//        {
//            txtWhere.Text = value;
//            result = true;
//        }
//    }
//    return result;
//}
//private void addOpenTableMenus()
//{
//    //Read tables from the database and load into menu items
//    //On Error Resume Next
//    //Add mnuOpenTable members
//    SqlDataReader rs = new SqlDataReader("");
//    string strSql = "SELECT * FROM afdTableData Where hidden = " + getSqlFalse() + " ORDER BY zOrder";
//    rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//    int i = 0;
//    while (!rs.EOF)
//    {
//        if (i > 0)
//        {
//            ControlArrayHelper.LoadControl(this, "mnuOpenTable", i);
//        }
//        else
//        {
//            mnuOpenTable[0].Enabled = true;
//        }
//        mnuOpenTable[i].Text = Convert.ToString(rs["tableName"]);
//        i++;
//        rs.MoveNext();
//    }
//    //Clean up
//    rs.Close();
//}
//internal string colCaption(int c)
//{
//    return Convert.ToString(flexGrid[0, c].Value);
//}
//internal int colField(string fld)
//{
//    //Here I want to worry about the table name in the string
//    string colCaptionShort = "";
//    string fldShort = short_Renamed(fld);
//    int tempForEndVar = dataGridView1.ColumnCount - 1;
//    for (int i = 1; i <= tempForEndVar; i++)
//    {
//        colCaptionShort = colCaption(i).Substring(colCaption(i).IndexOf('.') + 1);
//        if (colCaptionShort == fldShort)
//        {
//            return i;
//        }
//    }
//    MessageBox.Show("No column with the caption: " + fld, AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    return 0; //Not found
//}
//internal object convertValue(string str, DbType fieldType)
//{
//    object result = null;
//    string errorMsg = "";
//    try
//    {
//        if (fieldType == DbType.Int16)
//        {
//            errorMsg = "integer";
//            return Convert.ToInt32(Double.Parse(str));
//        }
//        else if (fieldType == DbType.Boolean)
//        {
//            errorMsg = "boolean";
//            bool tempBool = false;
//            return (Boolean.TryParse(str, out tempBool)) ? tempBool : Convert.ToBoolean(Double.Parse(str));
//        }
//        else if (fieldType == DbType.Int32)
//        {
//            errorMsg = "long integer";
//            return Convert.ToInt32(Double.Parse(str));
//        }
//        else if (fieldType == DbType.Single)
//        {
//            errorMsg = "small real number";
//            return Single.Parse(str);
//        }
//        else if (fieldType == DbType.Double)
//        {
//            errorMsg = "real number";
//            return Double.Parse(str);
//        }
//        else if (fieldType == DbType.DateTime)
//        {
//            errorMsg = "legal date";
//            return DateTime.Parse(str);
//        }
//        else if (fieldType == DbType.Currency)
//        {
//            errorMsg = "currency";
//            return Decimal.Parse(str, NumberStyles.Currency | NumberStyles.AllowExponent);
//        }
//        else if (fieldType == DbType.Decimal)
//        {
//            errorMsg = "decimal";
//            return Decimal.Parse(str, NumberStyles.Currency | NumberStyles.AllowExponent);
//        }
//        else
//        { //strings adVarChar, adVarWChar, adWChar, adChar, adLongVarChar, adLongVarWChar
//            return str;
//        }
//    }
//    catch
//    {
//        result = "ERROR! " + translation.tr("TheValueIsNotA", str, errorMsg, "");
//    }
//    return result;
//}
//private object defaultValue(DbType fieldType)
//{
//    //UPGRADE_WARNING: (2065) ADODB.DataTypeEnum property DataTypeEnum.adEmpty has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2065
//    object result = null;
//    DbType adDBFileTime = DbType.Object;
//    if (fieldType == DbType.Int16 || fieldType == DbType.Int32 || fieldType == DbType.Single || fieldType == DbType.Double || fieldType == DbType.Decimal || fieldType == DbType.Decimal)
//    {
//        result = 0;
//    }
//    else if (fieldType == DbType.Boolean)
//    {
//        result = false;
//    }
//    else if (fieldType == DbType.DateTime || fieldType == DbType.Time || fieldType == DbType.DateTime || fieldType == adDBFileTime)
//    {
//        result = DateTime.Now;
//    }
//    else if (fieldType == DbType.Currency)
//    {
//        result = Convert.ToDecimal(0);
//    }
//    else if (fieldType == DbType.String || fieldType == DbType.String || fieldType == DbType.String || fieldType == DbType.String || fieldType == DbType.String || fieldType == DbType.String)
//    {
//        result = "";
//    }
//    else
//    { //ignore binary - type 128 because i get errors anyway
//        MessageBox.Show("Unknown data type " + ((int)fieldType).ToString(), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    }
//    return result;
//}
//private bool isNullable(UpgradeHelpers.DB.FieldHelper fldName)
//{
//    return (fldName.FieldMetadata.AllowDBNull) == (((int)UpgradeHelpers.DB.FieldAttributeEnum.adFldMayBeNull) != 0);

//}

//private bool fieldIsInTable(object tableName, string fld)
//{
//    bool result = false;
//    UpgradeHelpers.DB.FieldHelper f = null;
//    //Check to see if fld is in the table
//    SqlDataReader rs = new SqlDataReader("");
//    string strSql = "Select * FROM [" + ReflectionHelper.GetPrimitiveValue<string>(tableName) + "]";
//    rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//    foreach (UpgradeHelpers.DB.FieldHelper fIterator in rs.Fields)
//    {
//        f = fIterator;
//        if (short_Renamed(f.FieldMetadata.ColumnName) == short_Renamed(fld))
//        {
//            result = true;
//            goto label20;
//        }
//        f = default(UpgradeHelpers.DB.FieldHelper);
//    }
//label20: rs.Close();
//    return result;
//}
//private bool idFieldUpdatable(string fld)
//{

//    if (readOnly)
//    {
//        return false;
//    }

//    //Book center rules
//    if (bookCenter)
//    {
//        if (fld == "lang_name")
//        {
//            return false;
//        }
//    }

//    //Only allow fields of certain types to be updatable -- because you can't tell the real id field from a number
//    DbType switchVar = SqlDataReader.GetDBType(currentDR.GetField(fld).FieldMetadata.DataType);
//    if (switchVar == DbType.String || switchVar == DbType.String || switchVar == DbType.String || switchVar == DbType.String || switchVar == DbType.String || switchVar == DbType.String)
//    { //text
//      //ok
//    }
//    else
//    {
//        return false;
//    }

//    return idFieldUpdatableInPrinciple(fld);

//}
//private bool idFieldUpdatableInPrinciple(string fld)
//{
//    // Basetable of Fld + ID must be in currentSql.myTable (that is in the database table, not table in grid).
//    //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    bool result = false;
//    string baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//    //There must be a field in the current table that inner joins with the base table
//    SqlDataReader rs = new SqlDataReader("");
//    string strSql = "SELECT * FROM afdFieldData WHERE tableName = '" + currentSql.myTable + "' AND innerJoin = '" + baseTable + "'";
//    rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//    result = !rs.EOF;
//    rs.Close();

//    return result;
//}

//private bool fieldUpdatable(string fld)
//{
//    //Fld updatable if it is in the currentSql.myTable (transcript or whatever)
//    //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    if (readOnly)
//    {
//        return false;
//    }
//    else if (isIdField(fld))
//    {
//        return false;
//    }
//    else if (Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue()) != currentSql.myTable)
//    {
//        return false;
//    }
//    else
//    {
//        return true;
//    }
//}
//private void formatList(int colIndex, int rowIndex, string son)
//{
//    int width = 0;

//    //Set top and left
//    //listNewData.Top = dataGridView1.Top + dataGridView1.RowPos[rowIndex] / 15 + dataGridView1.Rows[rowIndex].Height;
//    //listNewData.Left = dataGridView1.Left + dataGridView1.ColPos[colIndex] / 15 + 3;
//    listNewData.Top = dataGridView1.Top + dataGridView1.Rows[rowIndex].Height;
//    listNewData.Left = dataGridView1.Left + 3;
//    //Set height
//    if (listNewData.Items.Count > 10)
//    {
//        listNewData.Height = ((10 * 250) + 50) / 15;
//    }
//    else
//    {
//        listNewData.Height = ((listNewData.Items.Count * 250) + 50) / 15;
//    }
//    listNewData.Visible = !listNewData.Visible;
//    //Set visibility and order
//    if (listNewData.Visible)
//    {
//        listNewData.BringToFront(); // make sure the listNewData is on top of the grid - gets focus
//        listNewData.Focus();
//    }
//    //Set width
//    listNewData.Width = (dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].Width + 50) / 15;
//}
//private void formatTxtBox(int rowIndex, int colIndex, DbType fldType)
//{
//    //UPGRADE_WARNING: (2065) ADODB.DataTypeEnum property DataTypeEnum.adEmpty has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2065
//    DbType myFldType = DbType.Object;
//    //txtNewData.Top = dataGridView1.Top + dataGridView1.RowPos[rowIndex] / 15 + dataGridView1.Rows[rowIndex].Height;
//    //txtNewData.Left = dataGridView1.Left + dataGridView1.ColPos[colIndex] / 15 + 3;
//    txtNewData.Top = dataGridView1.Top + dataGridView1.Rows[rowIndex].Height;
//    txtNewData.Left = dataGridView1.Left + 3;
//    txtNewData.Visible = !txtNewData.Visible;

//    //Make a long and high box for anything with a return
//    if (txtNewData.Text.IndexOf(Environment.NewLine) >= 0)
//    {
//        myFldType = DbType.String;
//    }
//    else
//    {
//        myFldType = fldType;
//    }
//    if (dataGridView1.Columns[colIndex].Width > 0)
//    {
//        //ColWidth may be -1 for currency and dates for unknown reason
//        //very long text -- make a long and high txtBox
//        if (myFldType == DbType.String || myFldType == DbType.String)
//        {
//            txtNewData.Width = 700;
//            txtNewData.Height = 201;
//            if (txtNewData.Left * 15 + 10500 > dataGridView1.Left * 15 + dataGridView1.Width * 15)
//            {
//                txtNewData.Left = (dataGridView1.Left + dataGridView1.Width) - 707;
//            }
//            if (txtNewData.Top * 15 + 3010 > dataGridView1.Top * 15 + dataGridView1.Height * 15)
//            {
//                txtNewData.Top = (dataGridView1.Top + dataGridView1.Height) - 201;
//            }
//            //long text -- make a long and narrow
//        }
//        else if (myFldType == DbType.String || myFldType == DbType.String || myFldType == DbType.String || myFldType == DbType.String || myFldType == DbType.Double)
//        {  //integer or single or date-- field is in the currentSql.myTable
//            txtNewData.Width = 533;
//            txtNewData.Height = 20;
//            if (txtNewData.Left * 15 + 8000 > dataGridView1.Left * 15 + dataGridView1.Width * 15)
//            {
//                txtNewData.Left = (dataGridView1.Left + dataGridView1.Width) - 540;
//            }
//            //Short text
//        }
//        else
//        {
//            txtNewData.Width = (dataGridView1.Columns[colIndex].Width + 850) / 15;
//            txtNewData.Height = 20;
//        }
//    }
//    else
//    {
//        txtNewData.Width = 167;
//        if (txtNewData.Left * 15 + 2500 > dataGridView1.Left * 15 + dataGridView1.Width * 15)
//        {
//            txtNewData.Left = (dataGridView1.Left + dataGridView1.Width) - 173;
//        }
//        txtNewData.Height = 20;
//    }

//    //Make sure on top
//    if (txtNewData.Visible)
//    {
//        txtNewData.BringToFront(); // make sure the txtNewData is on top of the grid
//        txtNewData.Focus();
//    }



//internal bool header()
//{
//    //The header is row 0.
//    //If dataGridView1.rows > 2 And dataGridView1.row = 1 And dataGridView1.RowSel + 1 = dataGridView1.rows Then
//    return dataGridView1.CurrentCell.RowIndex == 0;
//}
//internal bool blueColumn()
//{
//    //The blue column is the left most column (We might say, "the column header").
//    //But the numbering of it is unreliable -- perhaps because I have hidden columns
//    //Usually dataGridView1.col returns 1, but occassionally 0.
//    //In effect clicking in column yields dataGridView1.col = 1, i.e. the first column (which may be hidden ?)
//    if (dataGridView1.CurrentCell.ColumnIndex == 0)
//    {
//        return true; //Only occurs in strange cases
//    }
//    else
//    {
//        return false;
//    }
//}

//internal float sngValue(SqlDataReader rs, string field)
//{
//    string newName = getCorrectFieldName(rs, field);
//    if (newName == "")
//    {
//        MessageBox.Show(translation.tr("ErrorFieldNotShowingInTableSeeAsdFieldData", field, "", "") + Environment.NewLine + translation.tr("AfterThisProgramCrashesReopenItAndFixError", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//        throw new System.Exception("7, , " + "I did not find the field " + field + " in the flexgird." + Environment.NewLine + "Use \"showInAll\" field or \"showInYellow\" field in afdFieldData to show " + field + " in this talbe.");
//    }
//    //Get the result
//    if (Convert.IsDBNull(rs[newName]))
//    {
//        return 0;
//    }
//    else
//    {
//        return Convert.ToSingle(rs[newName]);
//    }
//}
//internal int lngValue(SqlDataReader rs, string field)
//{
//    string newName = getCorrectFieldName(rs, field);
//    if (newName == "")
//    {
//        MessageBox.Show(translation.tr("ErrorFieldNotShowingInTableSeeAsdFieldData", field, "", "") + Environment.NewLine + translation.tr("AfterThisProgramCrashesReopenItAndFixError", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//        throw new System.Exception("7, , " + "I did not find the field " + field + " in the flexgird." + Environment.NewLine + "Use \"showInAll\" field or \"showInYellow\" field in afdFieldData to show " + field + " in this talbe.");
//    }
//    //Get the result
//    if (Convert.IsDBNull(rs[newName]))
//    {
//        return 0;
//    }
//    else
//    {
//        return Convert.ToInt32(rs[newName]);
//    }

//}

//private bool userApprovedUpdate(string table, string title, string msg)
//{
//    DialogResult reply = (DialogResult)0;
//    //Ignore for answers

//    if (isTranscript && title == "Answer")
//    {
//        return true;
//    }
//    //Ask about update

//    if (msg == "")
//    {
//        reply = MessageBox.Show(translation.tr("UpdateTheTableRecord", table, "", ""), title, MessageBoxButtons.OKCancel);
//    }
//    else
//    {
//        reply = MessageBox.Show(translation.tr("UpdateTheTableRecord", table, "", "") + Environment.NewLine + Environment.NewLine + msg, title, MessageBoxButtons.OKCancel);
//    }
//    //Return true or false
//    return reply == System.Windows.Forms.DialogResult.OK;
//}
//internal void writeLog(string job, SqlDataReader rs)
//{
//    StreamWriter tsWriter = null;
//    string table = "";
//    UpgradeHelpers.DB.FieldHelper f = null;
//    int rowID = 0;
//    try
//    {
//        //UPGRADE_ISSUE: (2064) ADODB.Field property rs.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Properties property Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Property property Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        table = Convert.ToString(rs.GetField(0).getProperties().Item("BASETABLENAME").getValue());
//        rowID = Convert.ToInt32(rs[short_Renamed(getTablePrimaryKeyField(table))]);
//        table = StringsHelper.Replace(table, " ", "_", 1, -1, CompareMethod.Binary);
//        tsWriter = new StreamWriter(ts);
//        tsWriter.WriteLine("<table-" + table + "-" + rowID.ToString() + "-" + job + ">");
//        foreach (UpgradeHelpers.DB.FieldHelper fIterator in rs.Fields)
//        {
//            f = fIterator;
//            tsWriter.WriteLine("<field-" + StringsHelper.Replace(f.FieldMetadata.ColumnName, " ", "_", 1, -1, CompareMethod.Binary) + ">" + xmlTransform(strValue(rs, f.FieldMetadata.ColumnName)) + "</field-" + f.FieldMetadata.ColumnName + ">");
//            f = default(UpgradeHelpers.DB.FieldHelper);
//        }
//        tsWriter.WriteLine("</table-" + table + "-" + rowID.ToString() + "-" + job + ">");
//    }
//    catch (System.Exception excep)
//    {
//        msgSB(translation.tr("ErrorWritingLog", "", "", "") + excep.Message);
//    }
//}
//private string xmlTransform(string str)
//{
//    string strTemp = StringsHelper.Replace(str, "<", "(", 1, -1, CompareMethod.Binary);
//    strTemp = StringsHelper.Replace(strTemp, ">", ")", 1, -1, CompareMethod.Binary);
//    strTemp = StringsHelper.Replace(strTemp, "&", "?", 1, -1, CompareMethod.Binary);
//    return strTemp;
//}
//private string sonTable(string cTable, string son)
//{
//    //Find the next step down the path towards the Son table
//    //Example sonTable(transcript,department) = course-term
//    string result = "";
//    UpgradeHelpers.DB.FieldHelper f = null;
//    SqlDataReader rs = null;
//    string strSql = "";
//    //CTable is the son --> no son
//    if (cTable == son)
//    {
//        result = "";
//        //CTable is the son
//    }
//    else if (fieldIsInTable(cTable, getTablePrimaryKeyField(son)))
//    {
//        result = son;
//        //Recursive step -- look if the son is somewhere down the line from a cTable direct son (an enumerator field in cTable)
//    }
//    else
//    {
//        rs = new SqlDataReader("");
//        strSql = "Select * FROM [" + cTable + "]";
//        rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//        foreach (UpgradeHelpers.DB.FieldHelper fIterator in rs.Fields)
//        {
//            f = fIterator;
//            //Find any "ID" field
//            if (fieldIsForeignKey(cTable, f.FieldMetadata.ColumnName))
//            {
//                //This is the son of cTable if son table is found anywhere down the line.
//                if (pathDownTo(getForeignTable(cTable, f.FieldMetadata.ColumnName), getTablePrimaryKeyField(son)) != "")
//                {
//                    result = getForeignTable(cTable, f.FieldMetadata.ColumnName);
//                }
//                rs.Close();
//                rs = null;
//                return result;
//            }
//            f = default(UpgradeHelpers.DB.FieldHelper);
//        }
//        rs.Close();
//        //Son not found
//        result = "";
//        rs = null;
//    }
//    return result;
//}
#endregion

#region Old Menus and commands		

////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////FILE MENU COMMANDS
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
//internal void mnuFile_Click(Object eventSender, EventArgs eventArgs)
//{
//    mnuCreateDataTables.Enabled = cn.State != ConnectionState.Closed;
//    listNewData.Visible = false;
//    txtNewData.Visible = false;
//    txtNewData.Tag = "";
//}



//internal void mnuDeleteDatabase_Click(Object eventSender, EventArgs eventArgs)
//{
//    frmDeleteDatabase fDeleteDatabase = frmDeleteDatabase.CreateInstance();
//    //    fDeleteDatabase.caption = "Delete Database from list"
//    fDeleteDatabase.ShowDialog();
//    fDeleteDatabase = null;
//    load_mnuDatabaseList();
//}
//internal void mnuClose_Click(Object eventSender, EventArgs eventArgs)
//{
//    this.Close();
//}
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////TABLES MENU COMMANDS
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------

////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////TABLE MENU COMMANDS
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
//internal void mnuTable_Click(Object eventSender, EventArgs eventArgs)
//{
//    string strFilterID = "";
//    listNewData.Visible = false;
//    txtNewData.Visible = false;
//    txtNewData.Tag = "";
//    //Enable and check the items in the table menu
//    if (currentSql.myTable == "")
//    {
//        mnuTableHide.Enabled = false;
//        mnuTableNoDouble.Enabled = false;
//        mnuTableRefresh.Enabled = false;
//        mnuTableHide.Checked = false;
//        mnuTableNoDouble.Checked = false;
//        mnuTableComboWidth.Enabled = false;
//    }
//    else
//    {
//        mnuTableHide.Enabled = true;
//        mnuTableNoDouble.Enabled = true;
//        mnuTableRefresh.Enabled = true;
//        mnuTableComboWidth.Enabled = true;
//        if (getSystemTableValue("afdTableData", currentSql.myTable, "", "noDouble", DbType.Boolean) == "True")
//        {
//            mnuTableNoDouble.Checked = true;
//        }
//    }
//}
//internal void mnuTableComboWidth_Click(Object eventSender, EventArgs eventArgs)
//{
//    string baseTable = "", fld = "", colwidth = "";
//    int junk = 0;
//    try
//    {
//        baseTable = currentSql.myTable;
//        fld = getTablePrimaryKeyField(currentSql.myTable);
//        colwidth = getSystemTableValue("afdTableData", currentSql.myTable, "", "comboWidth", DbType.Int32);
//        if (colwidth == "0")
//        {
//            colwidth = "";
//        }
//        colwidth = InputBoxHelper.InputBox(translation.tr("ColumnComboWidthForThisTable", "", "", ""), "", colwidth);
//        if (colwidth == "")
//        {
//            colwidth = "0";
//        }
//        junk = Convert.ToInt32(Double.Parse(colwidth)); //to cause an error if not an integer
//        updateTableSystemTable(currentSql.myTable, "comboWidth", colwidth, DbType.Int32);
//    }
//    catch
//    {
//        MessageBox.Show(translation.tr("YouDidNotEnterAnInteger", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    }

//}
//internal void mnuTableRefresh_Click(Object eventSender, EventArgs eventArgs)
//{
//    newSqlCurrent(currentSql.myTable);
//    bool tempRefParam = false;
//    writeGrid(ref tempRefParam);
//}
//internal void mnuTableHide_Click(Object eventSender, EventArgs eventArgs)
//{
//    DialogResult reply = MessageBox.Show(translation.tr("HideTableInTablesMenu", currentSql.myTable, "", "") + Environment.NewLine + "Warning: To undo this you must use the system table afdTableData", AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()), MessageBoxButtons.YesNo);
//    if (reply == System.Windows.Forms.DialogResult.Yes)
//    {
//        updateTableSystemTable(currentSql.myTable, "hidden", "True", DbType.Boolean);
//    }
//}
//internal void mnuTableNoDouble_Click(Object eventSender, EventArgs eventArgs)
//{
//    mnuTableNoDouble.Checked = !mnuTableNoDouble.Checked;
//    if (mnuTableNoDouble.Checked)
//    {
//        updateTableSystemTable(currentSql.myTable, "noDouble", "True", DbType.Boolean);
//    }
//    else
//    {
//        updateTableSystemTable(currentSql.myTable, "noDouble", "False", DbType.Boolean);
//    }
//}
//internal void mnuTableTextBox_Click(Object eventSender, EventArgs eventArgs)
//{
//    string fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//    //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    string baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//    //Allow all types of fields
//    updateTableSystemTable(baseTable, "textBox", short_Renamed(fld), DbType.String);
//    newSqlCurrent(currentSql.myTable);
//    bool tempRefParam = false;
//    writeGrid(ref tempRefParam);
//}
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////COLUMN MENU COMMANDS
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
//internal void mnuColumn_Click(Object eventSender, EventArgs eventArgs)
//{
//    listNewData.Visible = false;
//    txtNewData.Visible = false;
//    txtNewData.Tag = "";
//    //Default values - non-enabled
//    mnuSortColumnAZ.Enabled = false;
//    mnuSortColumnZA.Enabled = false;
//    mnuSortOnce.Enabled = false;
//    mnuColumnWidth.Enabled = false;
//    mnuTableTextBox.Enabled = false;
//    mnuColumnShowInAll.Enabled = false;
//    mnuColumnShowInYellow.Enabled = false;
//    //Default values - non-checked
//    mnuSortColumnAZ.Checked = false;
//    mnuSortColumnZA.Checked = false;
//    mnuSortOnce.Checked = false;
//    mnuColumnWidth.Checked = false;
//    mnuColumnShowInAll.Checked = false;
//    mnuColumnShowInYellow.Checked = false;
//    mnuTableTextBox.Checked = false;
//    //Exit - third clause is situation where nothing is choosen (first loading table)
//    //if (currentSql.myTable == "" || dataGridView1.CurrentCell.ColumnIndex != dataGridView1.SelectedColumns.ColSel || (dataGridView1.CurrentCell.ColumnIndex == dataGridView1.ColumnCount - 1 && dataGridView1.RowSel == 0 && dataGridView1.CurrentCell.RowIndex == 0))
//    if (currentSql.myTable == "" || dataGridView1.CurrentCell == null)
//    {
//        return;
//    }
//    //I am in some colum
//    string fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//    //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    string baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//    //Enable sort, width
//    mnuSortOnce.Enabled = true;
//    mnuSortColumnAZ.Enabled = true;
//    mnuSortColumnZA.Enabled = true;
//    mnuColumnWidth.Enabled = true;
//    mnuColumnShowInAll.Enabled = true;
//    mnuColumnShowInYellow.Enabled = true;
//    if (getSystemTableValue("afdFieldData", baseTable, fld, "showInAll", DbType.Boolean) == "True")
//    {
//        mnuColumnShowInAll.Checked = true;
//    }
//    if (getSystemTableValue("afdFieldData", baseTable, fld, "showInYellow", DbType.Boolean) == "True")
//    {
//        mnuColumnShowInYellow.Checked = true;
//    }
//    if (getSystemTableValue("afdFieldData", baseTable, fld, "sort", DbType.Boolean) == "True")
//    {
//        if (getSystemTableValue("afdFieldData", baseTable, fld, "zTOa", DbType.Boolean) == "True")
//        {
//            this.ClientSize.mnuSortColumnZA.Checked = true;
//        }
//        else
//        {
//            this.ClientSize.mnuSortColumnAZ.Checked = true;
//        }
//    }
//    //Enable the column width, textbox, resticted
//    if (fieldIsInTable(currentSql.myTable, short_Renamed(fld)))
//    {
//        mnuTableTextBox.Enabled = true;
//    }
//}
//internal void mnuSortOnce_Click(Object eventSender, EventArgs eventArgs)
//{
//    string fld = "", baseTable = "";
//    if (!blueColumn() && dataGridView1.RowCount > 1)
//    {
//        //Get baseTable of the element
//        fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//        //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//        sqlCurrent.sqlOrderBy(baseTable, short_Renamed(fld), SqlDataReader.GetDBType(currentDR.GetField(fld).FieldMetadata.DataType));
//        bool tempRefParam = false;
//        writeGrid(ref tempRefParam);
//    }
//}
//internal void mnuColumnShowInAll_Click(Object eventSender, EventArgs eventArgs)
//{
//    string fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//    //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    string baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//    mnuColumnShowInAll.Checked = !mnuColumnShowInAll.Checked;
//    if (mnuColumnShowInAll.Checked)
//    {
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "showInAll", "True", DbType.Boolean);
//    }
//    else
//    {
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "showInAll", "False", DbType.Boolean);
//    }
//}
//internal void mnuColumnShowInYellow_Click(Object eventSender, EventArgs eventArgs)
//{
//    string fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//    //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    string baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//    mnuColumnShowInYellow.Checked = !mnuColumnShowInYellow.Checked;
//    if (mnuColumnShowInYellow.Checked)
//    {
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "showInYellow", "True", DbType.Boolean);
//    }
//    else
//    {
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "showInYellow", "False", DbType.Boolean);
//    }
//}
//internal void mnuColumnWidth_Click(Object eventSender, EventArgs eventArgs)
//{
//    string baseTable = "", fld = "", colwidth = "";
//    int junk = 0;
//    try
//    {
//        fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//        //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//        colwidth = dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].Width.ToString();
//        if (colwidth == "0")
//        {
//            colwidth = "";
//        }
//        colwidth = InputBoxHelper.InputBox(translation.tr("ColumnWidthOfColumn", short_Renamed(fld), "", ""), "", colwidth);
//        if (colwidth == "")
//        {
//            colwidth = "0";
//        }
//        junk = Convert.ToInt32(Double.Parse(colwidth)); //to cause an error if not an integer
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "width", colwidth, DbType.Int32);
//        bool tempRefParam = false;
//        writeGrid(ref tempRefParam);
//    }
//    catch
//    {
//        MessageBox.Show(translation.tr("YouDidNotEnterAnInteger", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    }
//}
//internal void mnuSortColumnAZ_Click(Object eventSender, EventArgs eventArgs)
//{
//    string fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//    //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    string baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//    mnuSortColumnAZ.Checked = !mnuSortColumnAZ.Checked;
//    if (mnuSortColumnAZ.Checked)
//    {
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "sort", "True", DbType.Boolean);
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "zTOa", "False", DbType.Boolean);
//    }
//    else
//    {
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "sort", "False", DbType.Boolean);
//    }
//    bool tempRefParam = false;
//    writeGrid(ref tempRefParam);
//}
//internal void mnuSortColumnZA_Click(Object eventSender, EventArgs eventArgs)
//{
//    string fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//    //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    string baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//    mnuSortColumnZA.Checked = !mnuSortColumnZA.Checked;
//    if (mnuSortColumnZA.Checked)
//    {
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "sort", "True", DbType.Boolean);
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "zTOa", "True", DbType.Boolean);
//    }
//    else
//    {
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "sort", "False", DbType.Boolean);
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "zTOa", "False", DbType.Boolean);
//    }
//    bool tempRefParam = false;
//    writeGrid(ref tempRefParam);
//}
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////RECORD MENU COMMANDS
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
//internal void mnuRecord_Click(Object eventSender, EventArgs eventArgs)
//{
//    string strFilterID = "";
//    listNewData.Visible = false;
//    txtNewData.Visible = false;
//    txtNewData.Tag = "";
//    mnuFather.Enabled = false;
//    mnuMergeRecords.Enabled = false;
//    mnuCopyRows.Enabled = false;
//    mnuChangeRows.Enabled = false;
//    mnuDelete.Enabled = false;
//    if (currentSql.myTable != "")
//    {
//        if (dataGridView1.CurrentCell.RowIndex > 0 && entireRowsSelected())
//        {
//            mnuCopyRows.Enabled = true;
//            mnuChangeRows.Enabled = true;
//            strFilterID = getSystemTableValue("afdTableData", currentSql.myTable, "", "filterID", DbType.Int32);
//            mnuDelete.Enabled = true;
//            //if (dataGridView1.CurrentCell.RowIndex == dataGridView1.R)
//            //{
//            //mnuFather.Enabled = true;
//            //}
//            //else
//            //{
//            //mnuMergeRecords.Enabled = true;
//            //}
//        }
//    }
//}
//internal void mnuFather_Click(Object eventSender, EventArgs eventArgs)
//{
//    string strID = "", oldTable = "";
//    int tables = 0;
//    string father = "", strSql = "";
//    SqlDataReader rs = new SqlDataReader("");
//    OrderedDictionary tableCollection = new OrderedDictionary(System.StringComparer.OrdinalIgnoreCase);
//    frmCaptions captionsForm = null;
//    //Show the father table of the current selected cell -- where value is this row / this cell ID.
//    if (!blueColumn() && !header())
//    {
//        //Get tables collection
//        int tempForEndVar = mnuOpenTable.ControlCount() - 1;
//        for (int c = 0; c <= tempForEndVar; c++)
//        {
//            father = mnuOpenTable[c].Text;
//            if (father != currentSql.myTable)
//            {
//                strSql = "SELECT * FROM [afdFieldData] where innerJoin = '" + currentSql.myTable + "' AND tableName = '" + father + "'";
//                rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//                if (!rs.EOF)
//                {
//                    tableCollection.Add(Guid.NewGuid().ToString(), father);
//                }
//                rs.Close();
//            }
//        }
//        //Get father table
//        if (tableCollection.Count == 0)
//        {
//            msgSB(translation.tr("TheTableDoesNotHaveAFather", currentSql.myTable, "", ""));
//            return;
//        }
//        else if (tableCollection.Count == 1)
//        {
//            father = (string)tableCollection[0];
//        }
//        else
//        {
//            //multiple tables
//            captionsForm = frmCaptions.CreateInstance();
//            captionsForm.tableCollection = tableCollection;
//            captionsForm.job = "tableCollection";
//            captionsForm.Text = translation.tr("SelectFatherTable", "", "", "");
//            //UPGRADE_ISSUE: (2064) Void method Global.Load was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//            //VB.Global.Load(captionsForm);
//            captionsForm.ShowDialog();
//            if (captionsForm.selectedCaption == "")
//            {
//                captionsForm = null;
//                return;
//            }
//            else
//            {
//                father = captionsForm.selectedCaption;
//            }
//            captionsForm.tableCollection = null;
//            captionsForm = null;
//        }
//        //Open father table
//        strID = Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, colField(getTablePrimaryKeyField(currentSql.myTable))].Value);
//        oldTable = currentSql.myTable;
//        newSqlCurrent(father);
//        //Select oldTable ID into combos if possible
//        findInCombos(oldTable, strID);
//        bool tempRefParam = true;
//        writeGrid(ref tempRefParam);
//    }
//}
//internal void mnuMergeRecords_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Merges two rows in the database -- all visible elements must be the same.
//    string fTable = "";
//    string strSql = "";
//    SqlDataReader rs = null, rs2 = null;
//    UpgradeHelpers.DB.FieldHelper f = null;
//    int r = dataGridView1.CurrentCell.RowIndex;
//    int r2 = dataGridView1.CurrentCell.RowIndex;  // ASM - void for now

//    //Make sure two rows are choosen
//    if (r == r2)
//    {
//        MessageBox.Show(translation.tr("PleaseChooseTwoRecordsTheFirstAndLastRecordsChosenWillBeMerged", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//        return;
//    }

//    //All rows must be the same
//    int tempForEndVar = dataGridView1.ColumnCount - 1;
//    for (int c = 1; c <= tempForEndVar; c++)
//    {
//        //Use short because the primary key for currentSql.myTable is only found once in dataGrid, and hence will be short in colCaption
//        if (colCaption(c) != short_Renamed(getTablePrimaryKeyField(currentSql.myTable)))
//        { //all except the ID
//            if (Convert.ToString(flexGrid[r, c].Value) != Convert.ToString(flexGrid[r2, c].Value))
//            {
//                MessageBox.Show(translation.tr("TheTwoRecordsMustBeExactlyAlikePleaseChangeTheContentOfColumn", colCaption(c), "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//                return;
//            }
//        }
//    }

//    //Ask to merge and then merge
//    DialogResult reply = MessageBox.Show(translation.tr("MergeTheFirstTwoRows", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()), MessageBoxButtons.YesNo);
//    if (reply == System.Windows.Forms.DialogResult.Yes)
//    {
//        rs = new SqlDataReader("");
//        rs2 = new SqlDataReader("");
//        //Get ID's
//        r = Convert.ToInt32(Double.Parse(Convert.ToString(flexGrid[r, colField(getTablePrimaryKeyField(currentSql.myTable))].Value))); //ID to be retained
//        r2 = Convert.ToInt32(Double.Parse(Convert.ToString(flexGrid[r2, colField(getTablePrimaryKeyField(currentSql.myTable))].Value))); //ID to be deleted

//        //Delete the record with ID r2
//        strSql = "SELECT * FROM [" + currentSql.myTable + "] WHERE " + getTablePrimaryKeyFieldSQL(currentSql.myTable) + " = " + r2.ToString();
//        rs2.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//        //Delete rs2
//        writeLog("Merge-delete", rs2);
//        rs2.Delete();
//        rs2.Update();
//        rs2.Close();

//        //Replace r2 with r in all father tables of currentSql.myTable
//        int tempForEndVar2 = mnuOpenTable.ControlCount() - 1;
//        for (int c = 0; c <= tempForEndVar2; c++)
//        {
//            fTable = mnuOpenTable[c].Text;
//            if (fTable != currentSql.myTable)
//            {
//                strSql = "SELECT * FROM afdFieldData where innerJoin = '" + currentSql.myTable + "' AND tableName = '" + fTable + "'";
//                rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//                if (!rs.EOF)
//                {
//                    //Find all occurances of r2 in the table, and change to r
//                    strSql = "SELECT * FROM [" + fTable + "] WHERE " + getForeignKeyFieldName(fTable, currentSql.myTable) + " = " + r2.ToString();
//                    rs2.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//                    while (!rs2.EOF)
//                    {
//                        rs2[getForeignKeyFieldName(fTable, currentSql.myTable)] = r;
//                        writeLog("Merge-update", rs2);
//                        rs2.Update();
//                        rs2.MoveNext();
//                    }
//                    rs2.Close();
//                }
//                rs.Close();
//            }
//        }
//        rs = null;
//        rs2 = null;
//        bool tempRefParam = false;
//        writeGrid(ref tempRefParam);
//    }
//}
//internal void mnuCopyRows_Click(Object eventSender, EventArgs eventArgs)
//{
//    frmCopy cy = null;
//    //Make sure one or more rows are selected and not the header
//    if (entireRowsSelected() && String.CompareOrdinal(currentSql.myTable, "") > 0)
//    {
//        //Get the fld to change and the new value
//        cy = frmCopy.CreateInstance();
//        cy.table = currentSql.myTable;
//        cy.Text = translation.tr("CopyRecords", "", "", "");
//        //UPGRADE_ISSUE: (2064) Void method Global.Load was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //VB.Global.Load(cy);
//        cy.ShowDialog();
//        cy = null;
//    }
//    else
//    {
//        MessageBox.Show(translation.tr("PleaseChooseTheRecordYouWouldLikeToCopy", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    }
//}
//internal void mnuChangeRows_Click(Object eventSender, EventArgs eventArgs)
//{
//    frmCopy cy = null;
//    //Make sure one or more rows are selected and not the header
//    if (entireRowsSelected() && String.CompareOrdinal(currentSql.myTable, "") > 0)
//    {
//        //Get the fld to change and the new value
//        cy = frmCopy.CreateInstance();
//        cy.job = "change";
//        cy.table = currentSql.myTable;
//        cy.Text = translation.tr("ChangeRecords", "", "", "");
//        //UPGRADE_ISSUE: (2064) Void method Global.Load was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //VB.Global.Load(cy);
//        cy.ShowDialog();
//        cy = null;
//    }
//    else
//    {
//        MessageBox.Show(translation.tr("PleaseChooseTheRecordYouWouldLikeToChange", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    }
//}
//internal void mnuDelete_Click(Object eventSender, EventArgs eventArgs)
//{
//    DialogResult reply = (DialogResult)0;
//    int firstRow = 0, lastRow = 0;
//    int[] idArray = null;
//    string idField = "";
//    int arrIndex = 0;
//    SqlDataReader rs = null;
//    string strSql = "";
//    //Don't allow if no table is yet loaded
//    if (currentSql.myTable == "")
//    {
//        MessageBox.Show(translation.tr("FirstSelectATable", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//        return;
//    }
//    if (header())
//    {
//        MessageBox.Show(translation.tr("CantDeleteTheHeader", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//        return;
//    }
//    //Make sure row slected
//    if (entireRowsSelected())
//    {
//        reply = MessageBox.Show(translation.tr("DoYouWantToDeleteTheRecord", (dataGridView1.CurrentCell.RowIndex + 1 - dataGridView1.CurrentCell.RowIndex).ToString(), "", "") + Environment.NewLine + Environment.NewLine + translation.tr("WarningThisWillBeFinal", "", "", ""), "", MessageBoxButtons.YesNo);
//        if (reply == System.Windows.Forms.DialogResult.Yes)
//        {
//            firstRow = dataGridView1.CurrentCell.RowIndex;
//            lastRow = dataGridView1.CurrentCell.RowIndex;  // ASM not correct 
//            idField = getTablePrimaryKeyField(currentSql.myTable);
//            idArray = new int[lastRow - firstRow + 1];
//            int tempForEndVar = lastRow;
//            for (int i = firstRow; i <= tempForEndVar; i++)
//            {
//                //Fill idArray with rows to delete
//                idArray[arrIndex] = Convert.ToInt32(Double.Parse(Convert.ToString(flexGrid[i, colField(idField)].Value)));
//                arrIndex++;
//            }
//            deleteRows(currentSql.myTable, idArray);
//        }
//    }
//    else
//    {
//        MessageBox.Show(translation.tr("FirstChooseARecordToDelete", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    }
//}
//private void deleteRows(string table, int[] idArray)
//{
//    string strSql = "";
//    int rowsDeleted = 0;
//    int idSingle = 0;
//    string higherTable = "";
//    SqlDataReader rsFieldData = new SqlDataReader("");
//    SqlDataReader rsHigherTable = new SqlDataReader("");
//    SqlDataReader rs = new SqlDataReader("");
//    foreach (int idArray_item in idArray)
//    {
//        idSingle = idArray_item;
//        strSql = "SELECT * FROM [" + table + "] WHERE " + getTablePrimaryKeyFieldSQL(table) + " = " + idSingle.ToString();
//        //There should always be exactly one row -- but if not, I'll still continue
//        rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//        if (rs.RecordCount != 1)
//        {
//            MessageBox.Show("ASM Debug error: something is wrong -- wrong number of rows.  No row in " + table + " with id = " + idSingle.ToString(), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//            goto label60;
//        }
//        //Check if the database will be corrupted -- deleting an ID in a higher table
//        strSql = "Select * FROM afdFieldData WHERE fieldName = '" + short_Renamed(getTablePrimaryKeyField(table)) + "'";
//        rsFieldData.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//        while (!rsFieldData.EOF)
//        {
//            higherTable = strValue(rsFieldData, "tableName");
//            //Make sure this is not the same table  (<table, tableID> is in afdFieldData)
//            if (higherTable.ToLower() != table.ToLower())
//            {
//                //If this ID is in the higher table, the row can't be deleted
//                strSql = "Select * from [" + higherTable + "] WHERE " + short_Renamed(getTablePrimaryKeyField(table)) + " = " + idSingle.ToString();
//                rsHigherTable.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//                //Found this ID in a higher table
//                if (!rsHigherTable.EOF)
//                {
//                    MessageBox.Show(translation.tr("YouCantDeleteTheRowWithThisIDIsUsedIn", getTablePrimaryKeyField(table), idSingle.ToString(), higherTable), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//                    rsHigherTable.Close();
//                    rsFieldData.Close();
//                    goto label60;
//                }
//                rsHigherTable.Close();
//            }
//            rsFieldData.MoveNext();
//        }
//        rsFieldData.Close();
//        //Delete row
//        writeLog("Delete", rs);
//        rs.Delete();
//        rs.Update();
//        rowsDeleted++;
//    label60: rs.Close();
//    }
//    //Send msg
//    if (rowsDeleted > 0)
//    {
//        MessageBox.Show(translation.tr("NumberOfDeletedRecords", rowsDeleted.ToString(), "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//        bool tempRefParam = false;
//        writeGrid(ref tempRefParam);
//    }
//}
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////CELL MENU COMMANDS
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
//internal void mnuCell_Click(Object eventSender, EventArgs eventArgs)
//{
//    string fld = "", baseTable = "";
//    listNewData.Visible = false;
//    txtNewData.Visible = false;
//    txtNewData.Tag = "";
//    //Default
//    mnuCellChange.Enabled = false;
//    mnuSon.Enabled = false;
//    mnuCellRestrict.Enabled = false;
//    mnuCellRestrict.Checked = false;
//    mnuFindInCombos.Enabled = false;
//    mnuFindPresent.Enabled = false;
//    mnuFindDatabase.Enabled = false;
//    if (currentSql.myTable != "")
//    {
//        if (oneCellInTableChoosen())
//        {
//            fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//            //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//            //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//            //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//            baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//            mnuCellRestrict.Enabled = true;
//            this.ClientSize.mnuCellRestrict.Checked = getSystemTableValue("afdFieldData", baseTable, short_Renamed(fld), "filterValue", DbType.String) != "";
//            if (fieldUpdatable(fld))
//            {
//                mnuCellChange.Enabled = true;
//                mnuFindPresent.Enabled = true;
//                mnuFindDatabase.Enabled = true;
//            }
//            else if (idFieldUpdatable(fld))
//            {
//                mnuFindInCombos.Enabled = true;
//                mnuSon.Enabled = true;
//            }
//        }
//    }
//}
//internal void mnuCellChange_Click(Object eventSender, EventArgs eventArgs)
//{
//    flexGrid_DoubleClick(flexGrid, new EventArgs());
//}

//internal void mnuCellRestrict_Click(Object eventSender, EventArgs eventArgs)
//{
//    SqlDataReader rs = null;
//    string strSql = "";
//    string fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//    string filterValue = Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].Value);
//    //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    string baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//    if (mnuCellRestrict.Checked)
//    {
//        mnuCellRestrict.Checked = false;
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "filterValue", "", DbType.String);
//    }
//    else
//    {
//        mnuCellRestrict.Checked = true;
//        //Turn off all old filters on this table
//        rs = new SqlDataReader("");
//        strSql = "SELECT * FROM afdFieldData WHERE tableName = '" + baseTable + "'";
//        rs.Open(strSql, this.ClientSize.cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//        while (!rs.EOF)
//        {
//            rs["filterValue"] = "";
//            rs.Update();
//            rs.MoveNext();
//        }
//        rs = null;
//        //Add new filter
//        //Change false from "  x  " to "False"
//        if (SqlDataReader.GetDBType(currentDR.GetField(fld).FieldMetadata.DataType) == DbType.Boolean)
//        {
//            if (filterValue != "True")
//            {
//                filterValue = "False";
//            }
//        }
//        updateFieldSystemTable(baseTable, short_Renamed(fld), "filterValue", filterValue, DbType.String);
//    }
//    newSqlCurrent(currentSql.myTable);
//    bool tempRefParam = false;
//    writeGrid(ref tempRefParam);
//}

//internal void mnuSon_Click(Object eventSender, EventArgs eventArgs)
//{
//    string son = "", strID = "", fld = "", comboTable = "", baseTable = "", val = "";
//    int i = 0;
//    int c = 0;
//    if (oneCellInTableChoosen())
//    {
//        //Get base table of selected column
//        fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//        val = Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].Value);
//        //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//        //Get sonTable
//        son = sonTable(currentSql.myTable, baseTable);
//        //Send msg if no son to show
//        if (son == "")
//        {
//            msgSB(translation.tr("ThisColumnIsNotFromASonTable", "", "", ""));
//        }
//        else
//        {
//            //Get the ID current row in the son table
//            strID = Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, colField(getTablePrimaryKeyField(baseTable))].Value);
//            //Prepare grid
//            newSqlCurrent(son);
//            //Limit to fld (baseTable) ID number
//            sqlCurrent.sqlExtraWhere(getTablePrimaryKeyFieldSQL(baseTable), strID, DbType.Int32);
//            findInCombos(baseTable, strID); //Above can be eliminated if this were always successful.
//            insertInTextBox(fld, val);
//            //write grid
//            bool tempRefParam = true;
//            writeGrid(ref tempRefParam);
//        }
//    }
//    else
//    {
//        //Call MsgBox(tr("PleaseChooseACellInTheTable", "", "", ""))
//    }
//}
//internal void mnuFindInCombos_Click(Object eventSender, EventArgs eventArgs)
//{
//    string baseTable = "", tableOfFld = "", fld = "", strID = "", temp = "";
//    bool found = false;
//    //One cell selected
//    if (oneCellInTableChoosen())
//    {
//        //If this is in the currentSql.myTable, try to insert it in the text box
//        fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//        //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//        baseTable = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//        if (baseTable == currentSql.myTable)
//        {
//            if (insertInTextBox(fld, Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].Value)))
//            {
//                bool tempRefParam = false;
//                writeGrid(ref tempRefParam);
//            }
//            else
//            {
//                msgSB(translation.tr("ThisFieldIsIn", baseTable, "", ""));
//            }
//            return;
//            //If not in the baseTable, look in combos
//        }
//        else
//        {
//            found = false; //default
//            strID = Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, colField(getTablePrimaryKeyField(baseTable))].Value); //Should always exist
//            found = findInCombos(baseTable, strID);
//            //If not found, move up to fld's father table
//            while (!found)
//            {
//                fld = getCorrectFieldName(currentDR, getTablePrimaryKeyField(baseTable));
//                //UPGRADE_ISSUE: (2064) ADODB.Field property currentDR.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//                //UPGRADE_ISSUE: (2064) ADODB.Properties property currentDR.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//                //UPGRADE_ISSUE: (2064) ADODB.Property property currentDR.Properties.value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//                temp = Convert.ToString(currentDR.GetField(fld).getProperties().Item("BASETABLENAME").getValue());
//                if (temp != baseTable && temp != currentSql.myTable)
//                { //should always be true by my sql
//                    baseTable = temp;
//                    strID = Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, colField(getTablePrimaryKeyField(baseTable))].Value); //Should always exist
//                    found = findInCombos(baseTable, strID);
//                }
//                else
//                {
//                    return;
//                }
//            }
//            if (found)
//            {
//                bool tempRefParam2 = true;
//                writeGrid(ref tempRefParam2);
//            }
//        }
//    }
//}
//internal void mnuFindDatabase_Click(Object eventSender, EventArgs eventArgs)
//{
//    string fld = "", value = "";
//    if (dataGridView1.RowCount > 1 || dataGridView1.ColumnCount > 2)
//    { //Flexgrid
//      //One cell selected
//        if (oneCellInTableChoosen())
//        {
//            fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//            value = Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].Value);
//            clearTopBoxes();
//            sqlCurrent.sqlExtraWhere(fld, value, SqlDataReader.GetDBType(currentDR.GetField(fld).FieldMetadata.DataType));
//            bool tempRefParam = true;
//            writeGrid(ref tempRefParam);
//        }
//        else if (header())
//        {
//            fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//            value = InputBoxHelper.InputBox(translation.tr("PleaseEnterTheValueToFindIn", fld, "", ""));
//            if (value != "")
//            {
//                sqlCurrent.sqlExtraWhere(fld, value, SqlDataReader.GetDBType(currentDR.GetField(fld).FieldMetadata.DataType));
//            }
//            bool tempRefParam2 = true;
//            writeGrid(ref tempRefParam2);
//        }
//    }
//}
//internal void mnuFindPresent_Click(Object eventSender, EventArgs eventArgs)
//{
//    string fld = "", value = "";
//    if (dataGridView1.RowCount > 1 || dataGridView1.ColumnCount > 2)
//    {
//        //One cell selected
//        if (oneCellInTableChoosen())
//        {
//            fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//            value = Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, dataGridView1.CurrentCell.ColumnIndex].Value);
//            sqlCurrent.sqlExtraWhere(fld, value, SqlDataReader.GetDBType(currentDR.GetField(fld).FieldMetadata.DataType));
//            bool tempRefParam = true;
//            writeGrid(ref tempRefParam);
//        }
//        else if (header())
//        {
//            fld = colCaption(dataGridView1.CurrentCell.ColumnIndex);
//            value = InputBoxHelper.InputBox(translation.tr("PleaseEnterPhraseToFindInField", fld, "", ""));
//            if (value != "")
//            {
//                sqlCurrent.sqlExtraWhere(fld, value, SqlDataReader.GetDBType(currentDR.GetField(fld).FieldMetadata.DataType));
//            }
//            bool tempRefParam2 = true;
//            writeGrid(ref tempRefParam2);
//        }
//    }
//}

////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////Options MENU COMMANDS
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
//internal void mnuOptionsUseTableFilters_Click(Object eventSender, EventArgs eventArgs)
//{
//    mnuOptionsUseTableFilters.Checked = !mnuOptionsUseTableFilters.Checked;
//    if (mnuOptionsUseTableFilters.Checked)
//    {
//        InteractionHelper.SaveSettingRegistryKey("AccessFreeData", "Options", "UseTableFilters", "True");
//    }
//    else
//    {
//        InteractionHelper.SaveSettingRegistryKey("AccessFreeData", "Options", "UseTableFilters", "False");
//    }
//    bool tempRefParam = false;
//    writeGrid(ref tempRefParam);
//}
//internal void mnuOptionShowID_Click(Object eventSender, EventArgs eventArgs)
//{
//    mnuOptionShowID.Checked = !mnuOptionShowID.Checked;
//    if (mnuOptionShowID.Checked)
//    {
//        InteractionHelper.SaveSettingRegistryKey("AccessFreeData", "Options", "mnuOptionShowID", "True");
//    }
//    else
//    {
//        InteractionHelper.SaveSettingRegistryKey("AccessFreeData", "Options", "mnuOptionShowID", "False");
//    }

//}
//internal void mnuToolHideWhiteColumns_Click(Object eventSender, EventArgs eventArgs)
//{
//    mnuToolHideWhiteColumns.Checked = !mnuToolHideWhiteColumns.Checked;
//    if (mnuToolHideWhiteColumns.Checked)
//    {
//        InteractionHelper.SaveSettingRegistryKey("AccessFreeData", "Options", "HideWhiteColumns", "True");
//    }
//    else
//    {
//        InteractionHelper.SaveSettingRegistryKey("AccessFreeData", "Options", "HideWhiteColumns", "False");
//    }
//    bool tempRefParam = false;
//    writeGrid(ref tempRefParam);
//}
//internal void mnuToolHideRedColumns_Click(Object eventSender, EventArgs eventArgs)
//{
//    mnuToolHideRedColumns.Checked = !mnuToolHideRedColumns.Checked;
//    if (mnuToolHideRedColumns.Checked)
//    {
//        InteractionHelper.SaveSettingRegistryKey("AccessFreeData", "Options", "HideRedColumns", "True");
//    }
//    else
//    {
//        InteractionHelper.SaveSettingRegistryKey("AccessFreeData", "Options", "HideRedColumns", "False");
//    }
//    bool tempRefParam = false;
//    writeGrid(ref tempRefParam);
//}
//internal void mnuChinese_Click(Object eventSender, EventArgs eventArgs)
//{
//    translation.load_chinese_messages();
//    translation.load_chinese_captions();
//    translation.load_captions(DataGridViewForm.DefInstance);
//    InteractionHelper.SaveSettingRegistryKey("AccessFreeData", "Options", "Language", "chinese");
//}
//internal void mnuEnglish_Click(Object eventSender, EventArgs eventArgs)
//{
//    translation.load_english_messages();
//    translation.load_english_captions();
//    translation.load_captions(DataGridViewForm.DefInstance);
//    InteractionHelper.SaveSettingRegistryKey("AccessFreeData", "Options", "Language", "english");
//}
//internal void mnuRecordsPerPage_Click(Object eventSender, EventArgs eventArgs)
//{
//    string reply = "";
//    try
//    {
//        reply = InputBoxHelper.InputBox(translation.tr("RecordsPerPage", "", "", ""), "Records per page", recordsPerPage.ToString());
//        if (reply != "")
//        {
//            recordsPerPage = Convert.ToInt32(Double.Parse(reply));
//            InteractionHelper.SaveSettingRegistryKey("AccessFreeData", "Options", "RecordsPerPage", reply);
//        }
//    }
//    catch
//    {
//        MessageBox.Show(translation.tr("YouDidNotEnterAnInteger", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    }
//}
//internal void mnuToolsFontSize_Click(Object eventSender, EventArgs eventArgs)
//{
//    string reply = "";
//    try
//    {
//        reply = InputBoxHelper.InputBox(translation.tr("FlexGridFontSize", "", "", ""), "Font", flexGridfontSize.ToString());
//        if (reply != "")
//        {
//            flexGridfontSize = Convert.ToInt32(Double.Parse(reply));
//            InteractionHelper.SaveSettingRegistryKey("AccessFreeData", "Options", "FontSize", reply);
//            dataGridView1.Font = dataGridView1.Font.Change(size: flexGridfontSize);
//            int tempForEndVar = cmbFilter.ControlCount() - 1;
//            for (int i = 0; i <= tempForEndVar; i++)
//            {
//                cmbFilter[i].Font = cmbFilter[i].Font.Change(size: flexGridfontSize);
//            }
//            txtWhere.Font = txtWhere.Font.Change(size: flexGridfontSize);
//        }
//    }
//    catch
//    {
//        MessageBox.Show(translation.tr("YouDidNotEnterAnInteger", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    }
//}

////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////TOOLS MENU COMMANDS
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
//internal void mnuTools_Click(Object eventSender, EventArgs eventArgs)
//{
//    listNewData.Visible = false;
//    txtNewData.Visible = false;
//    txtNewData.Tag = "";
//    //Default
//    mnuViewLog.Enabled = false;
//    mnuRepairDatabase.Enabled = false;
//    mnuShowID.Enabled = false;
//    mnuFindID.Enabled = false;
//    mnuPrintCurrentTable.Enabled = false;
//    mnuToolsClearSorting.Enabled = false;
//    if (currentSql.myTable != "")
//    {
//        mnuViewLog.Enabled = true;
//        mnuRepairDatabase.Enabled = true;
//        mnuFindID.Enabled = true;
//        mnuToolsClearSorting.Enabled = true;
//        if (dataGridView1.CurrentCell.RowIndex > 0)
//        {
//            mnuShowID.Enabled = true;
//        }
//        if (dataGridView1.SelectedRows.Count > 0)
//        {
//            mnuPrintCurrentTable.Enabled = true;
//        }
//    }

//    mnuToolShowSystemTables.Enabled = false;
//    mnuOptionsUseTableFilters.Enabled = false;
//    mnuToolsClearCellFilters.Enabled = false;
//    if (mnuOpenTable[0].Enabled)
//    { //These is a connection and system tables
//        mnuToolShowSystemTables.Enabled = true;
//        mnuOptionsUseTableFilters.Enabled = true;
//        mnuToolsClearCellFilters.Enabled = true;
//    }


//}
//internal void mnuToolsClearSorting_Click(Object eventSender, EventArgs eventArgs)
//{
//    SqlDataReader rs = null;
//    string strSql = "";
//    if (tableExists("afdFieldData"))
//    {
//        rs = new SqlDataReader("");
//        strSql = "SELECT * FROM afdFieldData";
//        rs.Open(strSql, this.ClientSize.cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//        while (!rs.EOF)
//        {
//            writeLog("BeforeChange", rs);
//            rs["sort"] = false;
//            rs["zTOa"] = false;
//            rs.Update();
//            writeLog("AfterChange", rs);
//            rs.MoveNext();
//        }
//        rs = null;
//    }
//}
//internal void mnuToolsClearCellFilters_Click(Object eventSender, EventArgs eventArgs)
//{
//    SqlDataReader rs = null;
//    string strSql = "";
//    if (tableExists("afdFieldData"))
//    {
//        rs = new SqlDataReader("");
//        strSql = "SELECT * FROM afdFieldData";
//        rs.Open(strSql, this.ClientSize.cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//        while (!rs.EOF)
//        {
//            writeLog("BeforeChange", rs);
//            rs["filterValue"] = "";
//            rs.Update();
//            writeLog("AfterChange", rs);
//            rs.MoveNext();
//        }
//        rs = null;
//    }
//}
//internal void mnuToolShowSystemTables_Click(Object eventSender, EventArgs eventArgs)
//{
//    updateTableSystemTable("afdTableData", "hidden", "False", DbType.Boolean);
//    updateTableSystemTable("afdFieldData", "hidden", "False", DbType.Boolean);
//}
//internal void mnuViewLog_Click(Object eventSender, EventArgs eventArgs)
//{
//    int lngR = 0;
//    string tempFileName = "";
//    int junk = 0;
//    try
//    {
//        tempFileName = logFileName;
//        closeLogFile();
//        openLogFile();
//        //UPGRADE_WARNING: (2081) ShellExecuteA has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2081
//        ProcessStartInfo startInfo = new ProcessStartInfo();
//        startInfo.UseShellExecute = true;
//        startInfo.CreateNoWindow = true;
//        startInfo.Verb = "open";
//        startInfo.FileName = tempFileName;
//        startInfo.Arguments = "";
//        startInfo.WorkingDirectory = "0";
//        startInfo.WindowStyle = ProcessWindowStyle.Normal;
//        Process shellExecuteProcess = Process.Start(startInfo);
//        lngR = shellExecuteProcess.ExitCode;
//    }
//    catch (System.Exception excep)
//    {
//        MessageBox.Show(excep.Message, AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    }
//}
//internal void mnuFindID_Click(Object eventSender, EventArgs eventArgs)
//{
//    int lng = 0;
//    //UPGRADE_TODO: (1069) Error handling statement (On Error Resume Next) was converted to a pattern that might have a different behavior. More Information: https://docs.mobilize.net/vbuc/ewis#1069
//    try
//    {
//        string reply = InputBoxHelper.InputBox(translation.tr("SearchForTheFollowing", getTablePrimaryKeyField(currentSql.myTable), "", ""), "");
//        int c = colField(getTablePrimaryKeyField(currentSql.myTable));
//        if (reply != "")
//        {
//            lng = Convert.ToInt32(Double.Parse(reply));
//            int tempForEndVar = dataGridView1.RowCount;
//            for (int i = 1; i <= tempForEndVar; i++)
//            {
//                if (Convert.ToString(flexGrid[i, c].Value) == lng.ToString())
//                {
//                    //UPGRADE_ISSUE: (2064) MSHierarchicalFlexGridLib.MSHFlexGrid property dataGridView1.RowPosition was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//                    //dataGridView1.setRowPosition(1, i);
//                    //dataGridView1.CurrentCell.RowIndex = 1;
//                    //dataGridView1.CurrentCell.ColumnIndex = 1;
//                    //dataGridView1.ColSel = dataGridView1.ColumnCount - 1;
//                    return;
//                }
//            }
//            MessageBox.Show(translation.tr("NotFoundInTable", getTablePrimaryKeyField(currentSql.myTable), "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//        }
//    }
//    catch (Exception exc)
//    {
//        NotUpgradedHelper.NotifyNotUpgradedElement("Resume in On-Error-Resume-Next Block");
//    }
//    return;

//    MessageBox.Show(translation.tr("YouDidNotEnterAnInteger", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//}
//internal void mnuShowID_Click(Object eventSender, EventArgs eventArgs)
//{
//    //UPGRADE_TODO: (1069) Error handling statement (On Error Resume Next) was converted to a pattern that might have a different behavior. More Information: https://docs.mobilize.net/vbuc/ewis#1069
//    try
//    {
//        string strID = Convert.ToString(flexGrid[dataGridView1.CurrentCell.RowIndex, colField(getTablePrimaryKeyField(currentSql.myTable))].Value);
//        //dataGridView1.CurrentCell.ColumnIndex = 1;
//        //dataGridView1.ColSel = dataGridView1.ColumnCount - 1;
//        MessageBox.Show(getTablePrimaryKeyField(currentSql.myTable) + ": " + strID, AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    }
//    catch (Exception exc)
//    {
//        NotUpgradedHelper.NotifyNotUpgradedElement("Resume in On-Error-Resume-Next Block");
//    }
//}
//internal void mnuRepairDatabase_Click(Object eventSender, EventArgs eventArgs)
//{
//    string strSql = "";
//    int id = 0;
//    UpgradeHelpers.DB.FieldHelper f = null;
//    string table = "", msg = "";
//    string son = "";
//    int recordsChanged = 0, oldChanges = 0;
//    DialogResult reply = MessageBox.Show("Warning: this could take a long time." + Environment.NewLine + "Warning: backup first" + Environment.NewLine + "This replaces every foreign key that has no match in primary table with \"-1\".", AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()), MessageBoxButtons.OKCancel);
//    if (reply == System.Windows.Forms.DialogResult.Cancel)
//    {
//        return;
//    }
//    SqlDataReader rs = new SqlDataReader("");
//    SqlDataReader rs2 = new SqlDataReader("");
//    //loop through tables in database
//    int tempForEndVar = mnuOpenTable.ControlCount() - 1;
//    for (int c = 0; c <= tempForEndVar; c++)
//    {
//        table = mnuOpenTable[c].Text;
//        msgSB("Checking " + table);
//        strSql = "SELECT * FROM  [" + table + "]";
//        rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//        //Loop through fields looking for an ID
//        while (!rs.EOF)
//        {
//            foreach (UpgradeHelpers.DB.FieldHelper fIterator in rs.Fields)
//            {
//                f = fIterator;
//                if (fieldIsForeignKey(table, f.FieldMetadata.ColumnName))
//                {
//                    son = getForeignTable(table, f.FieldMetadata.ColumnName);
//                    //Get id -- id = -1 means that there is no match in son table
//                    id = lngValue(rs, f.FieldMetadata.ColumnName);
//                    if (id != -1)
//                    {
//                        //Get name of son table
//                        //Check if the son has this id
//                        strSql = "SELECT * FROM [" + son + "] WHERE " + getTablePrimaryKeyField(son) + " = " + lngValue(rs, f.FieldMetadata.ColumnName).ToString();
//                        rs2.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
//                        //Error if rs2 is empty
//                        if (rs2.EOF)
//                        {
//                            writeLog("Before", rs);
//                            //   Change value in father to -1
//                            rs[f.FieldMetadata.ColumnName] = -1;
//                            rs.Update();
//                            writeLog("After", rs);
//                            recordsChanged++;
//                        }
//                        rs2.Close();
//                    }
//                    else
//                    {
//                        oldChanges++;
//                        writeLog("Bad-ID", rs);
//                    }
//                }
//                f = default(UpgradeHelpers.DB.FieldHelper);
//            }
//            rs.MoveNext();
//        }
//        rs.Close();
//    }
//    if (recordsChanged + oldChanges == 0)
//    {
//        msg = translation.tr("FinishedRepairingTablesNoChangesOrBadIdNumbers", "", "", "");
//    }
//    else
//    {
//        msg = translation.tr("FoundBadIdNumbersAllChangedTo", recordsChanged.ToString(), "", "") + Environment.NewLine +
//              translation.tr("FoundOtherIdNumbersWhichWereAlready", oldChanges.ToString(), "", "") + Environment.NewLine +
//              translation.tr("SeeLogForDetails", "", "", "");
//    }
//    MessageBox.Show(msg, AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//    rs = null;
//    rs2 = null;
//}
//internal void mnuPrintCurrentTable_Click(Object eventSender, EventArgs eventArgs)
//{
//    int ubFlds = 0, t = 0, ubNum = 0;
//    string str = "";
//    string[] colFlds = null;
//    int[] colNumbers = null;
//    string[] mat = null;
//    SqlDataReader rs = null;
//    string fld = "";
//    if (dataGridView1.RowCount > 1 || dataGridView1.ColumnCount > 2)
//    {
//        if (currentDR.State == ConnectionState.Open)
//        {
//            rs = new SqlDataReader("");
//            rs.CursorLocation = CursorLocationEnum.adUseClient;
//            rs.CursorType = CursorTypeEnum.adOpenKeyset;
//            int tempForEndVar = dataGridView1.SelectedRows.Count;
//            for (int i = dataGridView1.CurrentCell.ColumnIndex; i <= tempForEndVar; i++)
//            {
//                if (dataGridView1.Columns[i].Width > 0 && i > 0)
//                {
//                    str = colCaption(i);
//                    colFlds = ArraysHelper.RedimPreserve(colFlds, new int[] { t + 1 });
//                    colFlds[t] = str;
//                    colNumbers = ArraysHelper.RedimPreserve(colNumbers, new int[] { t + 1 });
//                    colNumbers[t] = i;
//                    rs.FieldsMetadata.Add(str, UpgradeHelpers.DB.DBUtils.GetType(DbType.String), 255);
//                    t++;
//                }
//            }
//            ubFlds = colFlds.GetUpperBound(0);
//            ubNum = colNumbers.GetUpperBound(0);
//            rs.Open();
//            int tempForEndVar2 = dataGridView1.SelectedRows.Count;
//            for (int r = dataGridView1.CurrentCell.RowIndex; r <= tempForEndVar2; r++)
//            {
//                rs.AddNew();
//                int tempForEndVar3 = ubNum;
//                for (int i = 0; i <= tempForEndVar3; i++)
//                {
//                    str = Convert.ToString(flexGrid[r, colNumbers[i]].Value);
//                    fld = colFlds[i];
//                    rs[fld] = str;
//                }
//                rs.Update();
//            }
//            if (rs.RecordCount > 0)
//            {
//                msWord.printSelection(rs);
//            }
//            else
//            {
//                MessageBox.Show(translation.tr("PleaseChooseAnAreaOfTheGrid", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
//            }
//            rs.Close();
//            rs = null;
//        }
//    }
//}

////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////ADDRESS BOOK MENU COMMANDS
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
//internal void mnuAddressBook_Click(Object eventSender, EventArgs eventArgs)
//{
//    mnuAddressBookLabels.Enabled = false;
//    mnuAddressBookPhoneNumbers.Enabled = false;
//    mnuAddressBookAddresses.Enabled = false;
//    mnuAddressBookEmails.Enabled = false;
//    mnuAddressBookGetEmails.Enabled = false;
//    if (currentSql.myTable != "")
//    {
//        if (fieldIsInTable(currentSql.myTable, "??ID") || fieldIsInTable(currentSql.myTable, "??ID"))
//        {
//            mnuAddressBookLabels.Enabled = true;
//            mnuAddressBookPhoneNumbers.Enabled = true;
//            mnuAddressBookAddresses.Enabled = true;
//            mnuAddressBookEmails.Enabled = true;
//            mnuAddressBookGetEmails.Enabled = true;
//        }
//        //Taiwan churches
//        if (fieldIsInTable(currentSql.myTable, "????_ID"))
//        {
//            mnuAddressBookLabels.Enabled = true;
//        }
//    }
//}
//internal void mnuAddressBookGetEmails_Click(Object eventSender, EventArgs eventArgs)
//{
//    if (fieldIsInTable(currentSql.myTable, "??ID"))
//    {
//        addressBook.getEmailList(currentDR, false);
//    }
//    else if (fieldIsInTable(currentSql.myTable, "??ID"))
//    {
//        addressBook.getEmailList(currentDR, true);
//    }
//}
//internal void mnuAddressBookEmails_Click(Object eventSender, EventArgs eventArgs)
//{
//    if (fieldIsInTable(currentSql.myTable, "??ID"))
//    {
//        addressBook.makeEmailbook(currentDR, false);
//    }
//    else if (fieldIsInTable(currentSql.myTable, "??ID"))
//    {
//        addressBook.makeEmailbook(currentDR, true);
//    }
//}
//internal void mnuAddressBookLabels_Click(Object eventSender, EventArgs eventArgs)
//{
//    if (fieldIsInTable(currentSql.myTable, "????_ID"))
//    {
//        addressBook.MakeMailingLabels(currentDR, false, true);
//    }
//    else if (fieldIsInTable(currentSql.myTable, "??ID"))
//    {
//        addressBook.MakeMailingLabels(currentDR, false, false);
//    }
//    else if (fieldIsInTable(currentSql.myTable, "??ID"))
//    {
//        addressBook.MakeMailingLabels(currentDR, true, false);
//    }
//}
//internal void mnuAddressBookPhoneNumbers_Click(Object eventSender, EventArgs eventArgs)
//{
//    if (fieldIsInTable(currentSql.myTable, "??ID"))
//    {
//        addressBook.makePhonebook(currentDR, false);
//    }
//    else if (fieldIsInTable(currentSql.myTable, "??ID"))
//    {
//        addressBook.makePhonebook(currentDR, true);
//    }
//}
//internal void mnuAddressBookaddresses_Click(Object eventSender, EventArgs eventArgs)
//{
//    if (fieldIsInTable(currentSql.myTable, "??ID"))
//    {
//        addressBook.makeAddressBook(currentDR, false);
//    }
//    else if (fieldIsInTable(currentSql.myTable, "??ID"))
//    {
//        addressBook.makeAddressBook(currentDR, true);
//    }
//}

////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
////TRANSCRIPT MENU COMMANDS
////---------------------------------------------------------------------------------------------------------------------------------------------
////---------------------------------------------------------------------------------------------------------------------------------------------
//internal void mnuTranscript_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Transcript menu -- menu opened
//    this.ClientSize.mnuAnswersToAnswerSummaryTable.Enabled = currentSql.myTable == "Answers";
//    listNewData.Visible = false;
//    txtNewData.Visible = false;
//    txtNewData.Tag = "";
//}
//internal void mnuTranscriptFolder_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Transcript menu -- select the CRTS folder to save transcripts, etc. to.
//    string newpath = transcript.OpenDirectoryTV("CRTS transcript folder");
//    if (newpath != "")
//    {
//        InteractionHelper.SaveSettingRegistryKey("transcript", "files", "documents", newpath);
//    }
//}
//internal void mnuTranscriptTemplate_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Transcript menu - select template file
//    string filePath = Interaction.GetSetting("transcript", "files", "transcriptTemplate", "");
//    string newpath = transcript.getTemplatePath(ref filePath, translation.tr("StudentTranscriptTemplate", "", "", ""));
//    if (newpath != "")
//    {
//        InteractionHelper.SaveSettingRegistryKey("transcript", "files", "transcriptTemplate", newpath);
//    }
//}
//internal void mnuTranscriptTemplateEnglish_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Transcript menu - select template file
//    string filePath = Interaction.GetSetting("transcript", "files", "transcriptTemplateEnglish", "");
//    string newpath = transcript.getTemplatePath(ref filePath, translation.tr("StudentTranscriptTemplateEnglish", "", "", ""));
//    if (newpath != "")
//    {
//        InteractionHelper.SaveSettingRegistryKey("transcript", "files", "transcriptTemplateEnglish", newpath);
//    }

//}
//internal void mnuGradeTemplate_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Transcript menu - select template file
//    string filePath = Interaction.GetSetting("transcript", "files", "courseGradeTemplate", "");
//    string newpath = transcript.getTemplatePath(ref filePath, translation.tr("CourseGradeTemplate", "", "", ""));
//    if (newpath != "")
//    {
//        InteractionHelper.SaveSettingRegistryKey("transcript", "files", "courseGradeTemplate", newpath);
//    }

//}
//internal void mnuRoleTemplate_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Transcript menu - select template file
//    string filePath = Interaction.GetSetting("transcript", "files", "courseRoleTemplate", "");
//    string newpath = transcript.getTemplatePath(ref filePath, translation.tr("CourseRoleTemplate", "", "", ""));
//    if (newpath != "")
//    {
//        InteractionHelper.SaveSettingRegistryKey("transcript", "files", "courseRoleTemplate", newpath);
//    }
//}
//internal void mnuPrintTranscript_Click(Object eventSender, EventArgs eventArgs)
//{
//    if (currentDR.State == ConnectionState.Open && currentSql.myTable == "transcript")
//    {
//        //Order the records
//        bool tempRefParam = false;
//        writeGrid(ref tempRefParam);
//        //Set document path -- ok if fails
//        if (Interaction.GetSetting("transcript", "files", "transcriptTemplate", "") == "")
//        {
//            mnuTranscriptTemplate_Click(mnuTranscriptTemplate, new EventArgs());
//        }
//        else if (!File.Exists(Interaction.GetSetting("transcript", "files", "transcriptTemplate", "")))
//        {
//            mnuTranscriptTemplate_Click(mnuTranscriptTemplate, new EventArgs());
//        }
//        //print transcript
//        if (File.Exists(Interaction.GetSetting("transcript", "files", "transcriptTemplate", "")))
//        {
//            this.ClientSize.Cursor = Cursors.WaitCursor;
//            transcript.printTranscript(currentDR, "chinese");
//            this.ClientSize.Cursor = CursorHelper.CursorDefault;
//        }
//    }
//}
//internal void mnuPrintTranscriptEnglish_Click(Object eventSender, EventArgs eventArgs)
//{
//    if (currentDR.State == ConnectionState.Open && currentSql.myTable == "transcript")
//    {
//        //Order the records
//        bool tempRefParam = false;
//        writeGrid(ref tempRefParam);
//        //Set document path -- ok if fails
//        if (Interaction.GetSetting("transcript", "files", "transcriptTemplate", "") == "")
//        {
//            mnuTranscriptTemplateEnglish_Click(mnuTranscriptTemplateEnglish, new EventArgs());
//        }
//        else if (!File.Exists(Interaction.GetSetting("transcript", "files", "transcriptTemplateEnglish", "")))
//        {
//            mnuTranscriptTemplateEnglish_Click(mnuTranscriptTemplateEnglish, new EventArgs());
//        }
//        //print transcript
//        if (File.Exists(Interaction.GetSetting("transcript", "files", "transcriptTemplateEnglish", "")))
//        {
//            this.ClientSize.Cursor = Cursors.WaitCursor;
//            transcript.printTranscript(currentDR, "english");
//            this.ClientSize.Cursor = CursorHelper.CursorDefault;
//        }
//    }

//}
//internal void mnuPrintGrade_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Transcript menu - print grade sheet for teacher
//    if (currentDR.State == ConnectionState.Open && currentSql.myTable == "transcript")
//    {
//        //Order the records
//        bool tempRefParam = false;
//        writeGrid(ref tempRefParam);
//        //Set document path -- ok if fails
//        if (Interaction.GetSetting("transcript", "files", "courseGradeTemplate", "") == "")
//        {
//            mnuGradeTemplate_Click(mnuGradeTemplate, new EventArgs());
//        }
//        else if (!File.Exists(Interaction.GetSetting("transcript", "files", "courseGradeTemplate", "")))
//        {
//            mnuGradeTemplate_Click(mnuGradeTemplate, new EventArgs());
//        }
//        //Print course grade sheet
//        if (File.Exists(Interaction.GetSetting("transcript", "files", "courseGradeTemplate", "")))
//        {
//            transcript.printCourseGradeSheet(currentDR);
//        }
//    }
//}
//internal void mnuPrintRole_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Transcript menu - print role sheet for teacher and role book
//    if (currentDR.State == ConnectionState.Open)
//    {
//        //Order the records
//        bool tempRefParam = false;
//        writeGrid(ref tempRefParam);
//        //Set document path -- ok if fails
//        if (Interaction.GetSetting("transcript", "files", "courseRoleTemplate", "") == "")
//        {
//            mnuRoleTemplate_Click(mnuRoleTemplate, new EventArgs());
//        }
//        else if (!File.Exists(Interaction.GetSetting("transcript", "files", "courseRoleTemplate", "")))
//        {
//            mnuRoleTemplate_Click(mnuRoleTemplate, new EventArgs());
//        }
//        if (File.Exists(Interaction.GetSetting("transcript", "files", "courseRoleTemplate", "")))
//        {
//            transcript.printCourseRole(currentDR);
//        }
//    }
//}
//internal void mnuPrintTermSummary_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Transcript menu - print term summary
//    frmPrintTermSummary pt = frmPrintTermSummary.CreateInstance();
//    //UPGRADE_ISSUE: (2064) Void method Global.Load was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    //VB.Global.Load(pt);
//    pt.ShowDialog();
//}
//internal void mnuAnswersToAnswerSummaryTable_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Transcript menu -- copy answers to answer summary table
//    if (currentSql.myTable == "Answers")
//    {
//        transcript.answersToAnswerSummaryTable();
//    }
//}
//internal void mnuShowQPA_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Transcript menu -- show qpa in status bar
//    mnuShowQPA.Checked = !mnuShowQPA.Checked;
//    transcript.showQPA = mnuShowQPA.Checked;
//    InteractionHelper.SaveSettingRegistryKey("transcript", "options", "showQPA", mnuShowQPA.Checked.ToString());
//}



//internal void mnuCreatedataTables_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Try to create the table.  If you succeed, then close and open the connection - to get new list.
//    if (createSystemDataTables(false))
//    {
//        openConnection();
//    }
//}
//internal void mnuUpdateDataTables_Click(Object eventSender, EventArgs eventArgs)
//{
//    //Try to create the table.  If you succeed, then close and open the connection - to get new list.
//    if (createSystemDataTables(true))
//    {
//        openConnection();
//    }
//}

//The returnSql of combo's is restricted by higherInnerJoin.
// if (currentSql.myJob != "combo")
//{
//    string mtft = mytableFilterTable();
//    //Restrict the sql of the datagrid to the value of myTableFilterTable And insert this value into the combo
//    if (mtft != "")
//    {
//        string val = sysTables.getSystemTableValue("afdTableData", mtft, "", "filterID", DbType.Int32);
//        string myField = SqlHelper.getTable2ForeignKeyField(currentSql.myTable, mtft);
//        //This will cause the getSqlWhere to add a where clause for this table and value
//        findInCombos(mtft, val);
//    }
//}

// Create orderBy clauses in sqlBuilder from last page (if any)
// Start with ID order then keep track of last user orderby 

//  Original code to fill combo with filtered
//            if (cmb.SelectedIndex > -1)
//            {
//                if (cmb.SelectedValue != "0")   // Chose "0" as the "Cell Filter" value
//                { 
//                    // Set myFieldsCombo
//                    currentSql.myFieldsCombo.Clear();
//                    List<field> fields = new List<field>();
//                    // The selected text is the ColumnName and the SelectedValue is the dbType
//                    field fi = dataHelper.getField(currentSql.myTable, cmb.SelectedValue.ToString());
//                    fields.Add(fi);  // Only one field in the combo
//                    currentSql.myFieldsCombo = fields;

//                    // Set myOrderBysCombo to the first field in representative fields.
//                    currentSql.myOrderBysCombo.Clear();
//                    field sortField = currentSql.myFieldsCombo[0];  // Only field, added above
//                    orderBy ob = new orderBy(sortField, System.Windows.Forms.SortOrder.Ascending);
//                    currentSql.myOrderBysCombo.Add(ob);

//                    // Get Sql string to use to bind combo
////                    string strSql = currentSql.returnSql(command.cellfilter, cmb.SelectedValue.ToString());
//                    TextBox txtCellFilter = new TextBox();  
//                    if(cmb = )
//                    txtCellFilter_1.Enabled = true;
//              }

//string vnt = "";

//// whereTableCaption = "(";

////Load new where clauses in colSqlWhere from the topBoxes that are visible and the extraWhere of this class
//if (!currentSql.ignoreTopBoxes && currentSql.myJob != "combo")
//{
//    //Begin with txtWhere Box
//    if (txtCellFilter_0.Visible)
//    {
//        if (Strings.Len(txtCellFilter_0.Text.Trim()) > 0)
//        {
//            // whereFld = SqlHelper.getCorrectFieldName(rsCurrent, Convert.ToString(txtWhere.Tag));
//            string left = txtCellFilter_0.Tag.ToString();
//            string right = txtCellFilter_0.Text.Trim();
//            string dbType = "varchar";  // ASM - change to dbType in asfFields table
//            where wh = new where(left,right, dbType);
//            currentSql.myWheres.Add(wh);
//        }
//    }
//    for (int i = 0; i <= cmbFilter.Count() - 1; i++)
//    {
//        if (cmbFilter[i].Visible)
//        {
//            if (cmbFilter[i].SelectedIndex > 0)
//            {
//                string left = cmbFilter[i].Tag.ToString();
//                string right = cmbFilter[i].Text.Trim();
//                string dbType = "varchar";  // ASM - change to dbType in asfFields table
//                where wh = new where(left, right, dbType);
//                currentSql.myWheres.Add(wh);
//            }
//        }
//    }
//}
////Update sqlWhere by extra
//if (myExtraField != "")
//{
//    string left = myExtraField;
//    string right = myExtraValue;
//    string dbType = "varchar";  // ASM - change to dbType in asfFields table
//    where wh = new where(left, right, dbType);
//    currentSql.myWheres.Add(wh);
//}

#endregion

// Old DataGrid Setup columns
////Special widths and colors
////Bold if column is filtered
//if (getSystemTableValue("afdFieldData", baseTable, short_Renamed(fld), "filterValue", DbType.String) != "")
//{
//    dataGridView1.CurrentCell = flexGrid2[i, 0];
//    dataGridView1.Columns[i].DefaultCellStyle.Font = new Font("verdana", 10, FontStyle.Bold);
//}

////Special widths and colors
////Bold if column is filtered
//if (getSystemTableValue("afdFieldData", baseTable, short_Renamed(fld), "sort", DbType.Boolean) == "True")
//{
//    dataGridView1.CurrentCell = flexGrid2[i, 0];
//    dataGridView1.Columns[i].DefaultCellStyle.Font = new Font("verdana", 10, FontStyle.Bold);
//}

//    //Hide all ID columns - except the Primary key if option checked
//    if (mnuOptionShowID.Checked && this.ClientSize.isTablePrimaryKeyField(currentSql.myTable, fld))
//    {
//        dataGridView1.CurrentCell = flexGrid2[i, 0];
//        //UPGRADE_WARNING: (2080) QBCOlor(15) was upgraded to System.Drawing.Color.White and has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2080
//        dataGridView1.Columns[i].DefaultCellStyle.Font = new Font("verdana", 10, FontStyle.Regular);
//    }
//    else if (isIdField(fld))
//    {
//        dataGridView1.Columns[i].Width = lngValue(rsTemp, "width") / 15;
//    }
//    else
//    {
//        //Set color of header row of column - blue or yellow or orange
//        dataGridView1.CurrentCell = flexGrid2[i, 0];
//        if (fieldUpdatable(colCaption(i)))
//        {
//            dataGridView1.CurrentCell.Style.BackColor = Color.LightGreen; //Light Green -- field in this table, can update
//                                                                          //Book center: Change light green to light blue if the field has a multilingual translation
//            if (bookCenter)
//            {
//                temp = colCaption(i);
//                temp = StringsHelper.Replace(temp.ToLower(), "_name", "", 1, -1, CompareMethod.Binary);
//                if (tableExists(temp + "_ml"))
//                {
//                    dataGridView1.CurrentCell.Style.BackColor = Color.LightCyan; //Light cyan (blue) -- multilingual field (needs translated)
//                }
//            }
//        }
//        else if (idFieldUpdatable(colCaption(i)))
//        {
//            //UPGRADE_WARNING: (2080) QBCOlor(14) was upgraded to System.Drawing.Color.LightYellow and has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2080
//            dataGridView1.CurrentCell.Style.BackColor = Color.LightYellow; //Light yellow -- id-field in this table, can update this id.
//        }
//        else if (idFieldUpdatableInPrinciple(colCaption(i)))
//        {
//            //UPGRADE_WARNING: (2080) QBCOlor(15) was upgraded to System.Drawing.Color.White and has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2080
//            dataGridView1.CurrentCell.Style.BackColor = Color.White; //Bright white -- id-field in this table, but not of a type that represents the table (for example, a number).
//                                                                     //Hide these white columns if the option is set
//            if (mnuToolHideWhiteColumns.Checked)
//            {
//                dataGridView1.Columns[i].Width = 0;
//            }
//        }
//        else
//        {
//            //UPGRADE_WARNING: (2080) QBCOlor(13) was upgraded to System.Drawing.Color.Magenta and has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2080
//            dataGridView1.CurrentCell.Style.BackColor = Color.Magenta; //magenta (red)
//                                                                       //Hide these red columns if the option is set
//            if (mnuToolHideRedColumns.Checked)
//            {
//                dataGridView1.Columns[i].Width = 0;
//            }
//        }
//    }
//    //Center material in column
//    dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
//    //UPGRADE_ISSUE: (2064) MSHierarchicalFlexGridLib.MSHFlexGrid property dataGridView1.ColAlignmentHeader was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
//    // dataGridView1.setColAlignmentHeader((int)DataGridViewContentAlignment.MiddleLeft, i, 0);
//}
//rsTemp = null;
////Force repaint -- this is what may take time
//this.ClientSize.Refresh();
////Reset variables
//this.ClientSize.Cursor = CursorHelper.CursorDefault;
//progressBar1.Visible = false;
//updating = false;
//Clear out any extra where clause from sqlCurrent -- these only used once.
//sqlCurrent.sqlExtraWhere("", "", DbType.Object);
//sqlCurrent.ignoreTopBoxes = false;
//if (isTranscript)
//{
//    if (currentSql.myTable == "transcript" && tablePages == 1)
//    {
//        transcript.QpaToSB(currentDR);
//    }


