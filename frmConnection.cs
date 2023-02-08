using Microsoft.VisualBasic;
// using System.Data.SqlClient;
using System.Text;

namespace SqlCollegeTranscripts
{
    internal partial class frmConnection
        : System.Windows.Forms.Form
    {

        public frmConnection()
            : base()
        {
            InitializeComponent();
        }

        public bool success = false;
        List<connectionString> pastConnectionStrings;
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
            pastConnectionStrings = AppData.GetConnectionStringList();
            foreach (connectionString conString in pastConnectionStrings)
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

            // 8. Build English database - will do nothing if Boolean BuildingUpEnglishDatabase in MultiLingual.cs set to false
            MultiLingual.InsertEnglishIntoDatabase(this);

        }

        private void cmdDatabaseList_Click(Object eventSender, EventArgs eventArgs)
        {
            StringBuilder sb = new StringBuilder();
            if (this.txtServer.Text == "")
            {
                sb.AppendLine("You must enter a Server name");
            }
            if (cmbStrings.Text.IndexOf("{1}") > -1)
            {
                sb.AppendLine("Delete the database and {1} from connection string");
            }
            if (cmbStrings.Text.IndexOf("{2}") > -1 && this.txtUserId.Text == string.Empty)
            {
                sb.AppendLine("Enter a user name or choose a connection string without {2}.");
            }
            if (sb.Length > 0)
            {
                MessageBox.Show(sb.ToString());
            }
            else
            {
                string conStr = cmbStrings.Text;
                conStr = String.Format(conStr, txtServer.Text, txtDatabase.Text, txtUserId.Text);
                if (conStr.IndexOf("{3}") > -1)
                {
                    frmLogin passwordForm = new frmLogin();
                    passwordForm.ShowDialog();
                    string password = passwordForm.password;
                    passwordForm = null;
                    conStr.Replace("{3}", password);
                }
                //frmDeleteDatabase used to show databases
                frmListDatabases databaseListForm = new frmListDatabases();
                databaseListForm.serverOnlyConnectionString = conStr;
                databaseListForm.ShowDialog();
                if (databaseListForm.databaseName != string.Empty)
                {
                    this.txtDatabase.Text = databaseListForm.databaseName;
                }
                databaseListForm = null;
            }
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
                    MessageBox.Show("The connection failed. " + Environment.NewLine
                                    + "Your connection string : " + Environment.NewLine + showUser
                                    + Environment.NewLine + "VB error message:"
                                    + Environment.NewLine + Information.Err().Description);
                    cmdOK.Enabled = false;
                    success = false;
                }
                else
                {
                    MessageBox.Show("Test passed.");
                    cmdOK.Enabled = true;
                    success = true;
                }
            }
            catch
            {
                if (Information.Err().Description != "")
                {
                    MessageBox.Show("We are sorry to report that the connection failed. " + Environment.NewLine
                                    + "Your connection string : " + Environment.NewLine + showUser
                                    + Environment.NewLine + "Error message:"
                                    + Environment.NewLine + Information.Err().Description);
                    cmdOK.Enabled = false;
                    success = false;
                }
            }
        }

        private void cmdOK_Click(Object eventSender, EventArgs eventArgs)
        {
            // 1.  Save Application Data.
            string cs = cmbStrings.Text;
            cs = String.Format(cs, this.txtServer.Text, this.txtDatabase.Text, this.txtUserId.Text, password);
            bool readOnly = this.chkReadOnly.Checked;
            connectionString conString = new connectionString(cmbStrings.Text, this.txtServer.Text, this.txtUserId.Text,
                        this.txtDatabase.Text, MsSql.databaseType, readOnly);
            AppData.storeConnectionString(conString);

            // 2.
            this.Close();
        }

        private void cmbStrings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStrings.SelectedIndex > -1)
            {
                // Condition required because a default may have been selected
                if (pastConnectionStrings.Count > cmbStrings.SelectedIndex)
                {
                    txtServer.Text = pastConnectionStrings[cmbStrings.SelectedIndex].server;
                    txtUserId.Text = pastConnectionStrings[cmbStrings.SelectedIndex].user;
                    txtDatabase.Text = pastConnectionStrings[cmbStrings.SelectedIndex].databaseName;
                }
            }
        }

    }
}