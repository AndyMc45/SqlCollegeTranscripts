using System.Data;
using System.Data.SqlClient;

namespace SqlCollegeTranscripts
{
    internal partial class frmListDatabases
        : System.Windows.Forms.Form
    {
        //This form used both for deleting connections AND listing / selecting a database from Server list


        public frmListDatabases()
            : base()
        {
            //This call is required by the Windows Form Designer.
            InitializeComponent();
        }

        List<connectionString> csList = new List<connectionString>();
        public string serverOnlyConnectionString = string.Empty;
        public string databaseName = string.Empty;

        private void frmDeleteDatabase_Load_1(object sender, EventArgs e)
        {
            if (serverOnlyConnectionString != string.Empty)  // listing databases to select
            {
                this.cmdDelete.Visible = false;  // Delete invisible
                this.cmdExit.Text = "OK";
                this.Text = "List of Databases on the Server";
                this.listSqlDatabases(serverOnlyConnectionString);
            }
            else   // deleting databases fromlist
            {
                this.cmdExit.Text = "Exit";
                this.cmdDelete.Text = "Delete";
                write_csList();
            }
            // 8. Build English database - will do nothing if Boolean BuildingUpEnglishDatabase in MultiLingual.cs set to false
            MultiLingual.InsertEnglishIntoDatabase(this);
        }
        private void cmdDelete_Click(Object eventSender, EventArgs eventArgs)
        {
            // Delete item from Registry
            int i = lstDatabaseList.SelectedIndex;
            if (i > -1)
            {
                AppData.DeleteConnectionStringFromList(i);  // Assumes lblDataList and csList match for every i
                                                            //Clear the connection string list and rewrite it
                lstDatabaseList.Items.Clear();
                write_csList();
            }
        }
        private void write_csList()
        {
            csList = AppData.GetConnectionStringList();
            foreach (connectionString cs in csList)
            {
                lstDatabaseList.Items.Add(cs.comboString);
            }
        }
        private void listSqlDatabases(string cs)
        {
            using (var con = new SqlConnection(cs))
            {
                try
                {
                    con.Open();
                    DataTable databases = con.GetSchema("Databases");
                    foreach (DataRow database in databases.Rows)
                    {
                        String databaseName = database.Field<String>("database_name");
                        lstDatabaseList.Items.Add(databaseName);
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Error opening connection: " + exc.Message);
                }
            }
        }
        private void cmdExit_Click(Object eventSender, EventArgs eventArgs)
        {   // Retrun selected database name 
            if (serverOnlyConnectionString != string.Empty)
            {
                databaseName = lstDatabaseList.GetItemText(lstDatabaseList.SelectedItem);
            }
            this.Close();
        }
        private void Form_Closed(Object eventSender, EventArgs eventArgs)
        {
        }
        private void lstDatabaseList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}