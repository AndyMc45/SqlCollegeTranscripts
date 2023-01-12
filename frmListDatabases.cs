using Microsoft.VisualBasic;
using System;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using AccessFreeData2;

namespace AccessFreeData
{
	internal partial class frmListDatabases
		: System.Windows.Forms.Form
	{
		public frmListDatabases()
			: base()
		{
			//This call is required by the Windows Form Designer.
			InitializeComponent();
		}

		public string serverOnlyConnectionString;
        public string databaseName = "";

        private void frmDeleteDatabase_Load_1(object sender, EventArgs e)
        {
            //This form used both for deleting connections AND listing databases on Server
            translation.load_captions(this);
			if (serverOnlyConnectionString == "")
			{
				write_list();
				this.Text = translation.tr("DeleteDatabase", "", "", "");
			}
			else
			{
				this.cmdDelete.Visible = false;
				this.cmdExit.Text = "OK";
				this.Text = "List of Databases on the Server";
				this.listSqlDatabases(serverOnlyConnectionString);
			}
		}
		private void cmdDelete_Click(Object eventSender, EventArgs eventArgs)
		{
			int i = 0;
			string dbCS = "", path = "", dbType = "", readOnly = "";
			//Update settings - move all elements in the list after the deleted item forward one spot
			//We use settings and not the caption, because captions may contain " (Read only)"
			if (lstDatabaseList.Items.Count > 0)
			{
				i = lstDatabaseList.SelectedIndex; //Index of setting is also i.
				Helper.regitDelete("DatabaseList", "path", i);
                Helper.regitDelete("DatabaseList", "type", i);
                Helper.regitDelete("DatabaseList", "ro", i);
                Helper.regitDelete("DatabaseList", "cs", i);

				//Clear the list and rewrite it
				lstDatabaseList.Items.Clear();
				write_list();
			}
		}
		private void write_list()
		{
			int i = 0;
			string caption = Interaction.GetSetting("AccessFreeData", "DatabaseList", "path" + i.ToString().Trim(), "end"); //Default is "end" in case it doesn't exist
			while (caption != "end")
			{
				lstDatabaseList.Items.Add(caption);
				i++;
				caption = Interaction.GetSetting("AccessFreeData", "DatabaseList", "path" + i.ToString().Trim(), "end"); //Default is "end" in case it doesn't exist
			}
			//If there are no elements in the path, delete the msSql and msAccess default strings
			//This allows user to see my default strings if need be.
			if (i == 0)
			{
				Interaction.DeleteSetting("AccessFreeData", "DatabaseList", "");
			}
		}
		private void listSqlDatabases(string cs)
		{
			using (var con = new SqlConnection(cs))
            {
				try { 
					con.Open();
					DataTable databases = con.GetSchema("Databases");
					foreach (DataRow database in databases.Rows)
					{
						String databaseName = database.Field<String>("database_name");
						lstDatabaseList.Items.Add(databaseName);
	                }
				} catch(Exception exc) {
                    MessageBox.Show("Error opening connection: " + exc.Message);
                }
			}
        }
		private void cmdExit_Click(Object eventSender, EventArgs eventArgs)
		{	// Retrun selected database name 
			if (serverOnlyConnectionString != "")
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