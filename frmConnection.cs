using Microsoft.VisualBasic;
// using System.Data.SqlClient;
using System.Text;
using System.Data;
using InfoBox;

namespace SqlCollegeTranscripts
{
    internal partial class frmConnection
        : System.Windows.Forms.Form
    {

        public frmConnection() : base()
        {
            InitializeComponent();
            csList = new List<connectionString>();
        }

        public bool success = false;
        List<connectionString> csList { get; set; }
        private string password = string.Empty;

        private void frmConnection_Load(object sender, EventArgs e)
        {
            string msg = "";
            lblConnection.Text = "Enter Connection String. Numbers in brackets {} represent values given above.  Use {3} for password.";

            //Load txtHelp
            msg = "For information on connection strings, see www.connectionstrings.com  ";
            msg = msg + Environment.NewLine + "You must pass test before O.K. butten is enabled";
            txtHelp.Text = msg;

            //Disable OK
            cmdOK.Enabled = false;

            // Get past successful connections and put in combobox
            csList = AppData.GetConnectionStringList();
            foreach (connectionString conString in csList)
            {
                cmbStrings.Items.Add(conString.comboString);
            }
            // Add defaults
            List<string> defaultComboItems = MsSql.defaultConnectionString();
            foreach (string str in defaultComboItems)
            {
                cmbStrings.Items.Add(str);
            }
            // Select the first - this will set the 3 txtboxes
            cmbStrings.SelectedIndex = 0;
        }

        private void cmdDatabaseList_Click(Object eventSender, EventArgs eventArgs)
        {
            StringBuilder sb = new StringBuilder();
            if (this.txtServer.Text == "")
            {
                sb.AppendLine("You must enter a Server name");
            }
            if (cmbStrings.Text.IndexOf("{2}") > -1 && this.txtUserId.Text == string.Empty)
            {
                sb.AppendLine("Enter a user name or choose a connection string without {2}.");
            }
            if (sb.Length > 0)
            {
                InformationBox.Show(sb.ToString());
            }
            else
            {
                string conStr = cmbStrings.Text;

                // Remove {1} from conStr
                int one = conStr.IndexOf("{1}");
                if (one > -1)
                {
                    int nextSemi = conStr.IndexOf(";", one);
                    int lastSemi = conStr.Substring(0, one).LastIndexOf(";");
                    conStr = conStr.Substring(0,lastSemi + 1) + conStr.Substring(nextSemi+1);
                }

                conStr = String.Format(conStr, txtServer.Text, "none", txtUserId.Text);
                if (conStr.IndexOf("{3}") > -1)
                {
                    frmLogin passwordForm = new frmLogin();
                    passwordForm.ShowDialog();
                    string password = passwordForm.password;
                    passwordForm = null;
                    conStr.Replace("{3}", password);
                }
                List<string> databaseList = getSqlDatabaseList(conStr);
                if (databaseList.Count > 0)
                { 
                    //frmDeleteDatabase used to show databases
                    frmListItems databaseListForm = new frmListItems();
                    databaseListForm.myList = databaseList;
                    databaseListForm.formCaption = "Hello";
                    databaseListForm.myJob = frmListItems.job.SelectString;
                    databaseListForm.ShowDialog();
                    if (databaseListForm.returnString != string.Empty)
                    {
                        this.txtDatabase.Text = databaseListForm.returnString;
                    }
                    databaseListForm = null;
                }
            }
        }

        private List<string> getSqlDatabaseList(string cs)
        {
            List<string> strList = new List<string>();
            try
            {
                MsSql.openNoDatabaseConnection(cs);
                DataTable databases = MsSql.noDatabaseConnection.GetSchema("Databases");
                foreach (DataRow database in databases.Rows)
                {
                    strList.Add(database.Field<String>("database_name"));
                }
                MsSql.CloseNoDatabaseConnection();
            }
            catch (Exception exc)
            {
                InformationBox.Show("Error opening connection: " + exc.Message, "Error opening Connection", InformationBoxIcon.Error);
            }
            return strList;
        }



        private void cmdCancel_Click(Object eventSender, EventArgs eventArgs)
        {
            this.Close();
        }

        private void cmdTest_Click_1(object sender, EventArgs e)
        {
            string cs = cmbStrings.Text;
            //Password
            string showUser = cs.Replace("{3}", "*********");  // Used in error message
            if (cs.IndexOf("{3}") >= 0)
            {
                frmLogin passwordForm = new frmLogin();
                passwordForm.ShowDialog();
                password = passwordForm.password;
                passwordForm = null;
                cs = cs.Replace("{3}", password);
            }
            cs = String.Format(cs, this.txtServer.Text, this.txtDatabase.Text, this.txtUserId.Text, password);
            //Try to open connection
            try
            {
                MsSql.openConnection(cs);

                if (Information.Err().Description != "")
                {
                    InformationBox.Show("The connection failed. " + Environment.NewLine
                                    + "Your connection string : " + Environment.NewLine + showUser
                                    + Environment.NewLine + "VB error message:"
                                    + Environment.NewLine + Information.Err().Description, "Error",InformationBoxIcon.Error);
                    cmdOK.Enabled = false;
                    success = false;
                }
                else
                {
                    InformationBox.Show("Test passed.", "Success", InformationBoxIcon.Exclamation);
                    cmdOK.Enabled = true;
                    success = true;
                }
            }
            catch
            {
                if (Information.Err().Description != "")
                {
                    InformationBox.Show("We are sorry to report that the connection failed. " + Environment.NewLine
                                    + "Your connection string : " + Environment.NewLine + showUser
                                    + Environment.NewLine + "Error message:"
                                    + Environment.NewLine + Information.Err().Description, "Error", InformationBoxIcon.Error);
                    cmdOK.Enabled = false;
                    success = false;
                }
            }
        }

        private void cmdOK_Click(Object eventSender, EventArgs eventArgs)
        {
            string strCS = cmbStrings.Text;
            strCS = String.Format(strCS, this.txtServer.Text, this.txtDatabase.Text, this.txtUserId.Text, password);
            bool readOnly = this.chkReadOnly.Checked;
            connectionString conString = new connectionString(cmbStrings.Text, this.txtServer.Text, this.txtUserId.Text,
                        this.txtDatabase.Text, MsSql.databaseType, readOnly);
            foreach (connectionString cs in csList)
            {
                if (AppData.areEqual(cs, conString))
                {
                    csList.Remove(cs);
                    break;  // only remove one
                }
            }
            csList.Insert(0,conString);
            AppData.storeConnectionStringList(csList);
            this.Close();
        }

        private void cmbStrings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStrings.SelectedIndex > -1)
            {
                // Condition required because a default may have been selected
                if (csList.Count > cmbStrings.SelectedIndex)
                {
                    txtServer.Text = csList[cmbStrings.SelectedIndex].server;
                    txtUserId.Text = csList[cmbStrings.SelectedIndex].user;
                    txtDatabase.Text = csList[cmbStrings.SelectedIndex].databaseName;
                }
            }
        }

    }
}