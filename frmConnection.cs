using AccessFreeData2;
using Microsoft.VisualBasic;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Windows.Forms;

namespace AccessFreeData
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
        // The main value - cs without password, stored in registry
        private string cs = string.Empty;   
		//Other values stored in the registry on cmdOK. 
		private string databaseType = "", specialDatabase = "", shortPath = "",  readOnly = "";
 		private string appName = "AccessFreeData";
		
        private void frmConnection_Load(object sender, EventArgs e)
        {
            string msg = "";
			//Set up captions
			translation.load_captions(this);
			this.Text = translation.tr("SetTheConnectionString", "", "", "");
			lblConnection.Text = "Enter Connection String. Numbers in brackets represent values given above.  Use [3] for password.";

			//Load txtHelp
			msg = "For information on connection strings, I use www.connectionstrings.com  ";
			msg = msg + Environment.NewLine +  "You must pass test before O.K. butten is enabled";
			txtHelp.Text = msg;

			//Disable OK
			cmdOK.Enabled = false;

			//Load the last successful server and login (i.e. userid); Only keeping one.
			this.txtServer.Text = Interaction.GetSetting(appName, "Default", "server", ".\\SQLExpress");
			this.txtUserId.Text = Interaction.GetSetting(appName, "Default", "userid", "");

			//Set the default to not a special database (i.e. Neither)
			_optSpecialDatabase_2.Checked = true;

			// Load successful cmbStrings
			cmbStrings.Items.Clear();
			string pastSuccess = Interaction.GetSetting(appName, "DatabaseList", "ComboString0", "end");
			if (pastSuccess != null) { cmbStrings.Text = pastSuccess; }
			int i = 0;
			while (pastSuccess != "end" && i < 8)
			{ 
				cmbStrings.Items.Add(pastSuccess);
				i++;
				pastSuccess = Interaction.GetSetting(appName, "DatabaseList", "ComboString" + i.ToString().Trim(), "end");
            }

        }
		
		private void cmdDatabaseList_Click(Object eventSender, EventArgs eventArgs)
		{
			if (this.txtServer.Text == "" || this.txtUserId.Text == "")
			{
				MessageBox.Show("You must first enter (1) Server name and (3) User id");
			}
			else
			{
				frmLogin passwordForm = new frmLogin();
				passwordForm.ShowDialog();
				string password = passwordForm.password;
				passwordForm = null;

				//This assumes MsSql - frmDeleteDatabase used to show databases
				frmListDatabases databaseListForm = new frmListDatabases();
                string conStr = $"data source = {this.txtServer.Text}; user id = {this.txtUserId.Text}; password = {password}";
				databaseListForm.serverOnlyConnectionString = conStr;
                databaseListForm.ShowDialog();
				if (databaseListForm.databaseName != "")
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

		private void cmdOK_Click(Object eventSender, EventArgs eventArgs)
		{
			success = true;
            // 1.  Set all the variables for registry.
			cs = cmbStrings.Text;
            cs = cs.Replace("[0]", this.txtServer.Text);
            cs = cs.Replace("[1]", this.txtDatabase.Text);
            cs = cs.Replace("[2]", this.txtUserId.Text);
            databaseType = "MsSql";
			shortPath = this.txtServer.Text + " \\ " + this.txtDatabase.Text;
            if (this.chkReadOnly.Checked)
            {
				readOnly = "true";
				this.shortPath += "(Read Only)";
            }
            else
            {
                readOnly = "false";
            }
			if (specialDatabase != "") {
				this.shortPath += " (" + specialDatabase + ")";
			}

			// 2. Save everything to registry
			Helper.regitPush("DatabaseList", "cs", cs, 8);
            Helper.regitPush("DatabaseList", "type", databaseType, 8);
            Helper.regitPush("DatabaseList", "path", shortPath, 8);
            Helper.regitPush("DatabaseList", "sd", specialDatabase, 8);
            Helper.regitPush("DatabaseList", "ro", readOnly, 8);
            Helper.regitPush("DatabaseList", "cs", cs, 8);
            Helper.regitPush("DatabaseList", "comboString", cmbStrings.Text, 8);

            // 3. For next load - could add and then read from cs0 - but will not change with cmbString change so no need
            if (this.databaseType == "MsSql")   // always true
			{
				Interaction.SaveSetting("AccessFreeData", "Default", "server", this.txtServer.Text);
				Interaction.SaveSetting("AccessFreeData", "Default", "userid", this.txtUserId.Text);
                Interaction.SaveSetting("AccessFreeData", "Default", "path", this.shortPath);
            }
			// 4.
			this.Close();
		}

        private void cmdTest_Click_1(object sender, EventArgs e)
        {
			//ASM string shortFile = "";
			//ASM int p = 0;
			//Get connection string
			string cs = cmbStrings.Text;
			cs = cs.Replace("[0]", this.txtServer.Text);
			cs = cs.Replace("[1]", this.txtDatabase.Text);
			cs = cs.Replace("[2]", this.txtUserId.Text);
			string noPasswordCs = cs;
			noPasswordCs = noPasswordCs.Replace("[4]", "*******");
			//Password
			if (cs.IndexOf("[3]") >= 0)
			{
                frmLogin passwordForm = new frmLogin();
                passwordForm.ShowDialog();
                string? password = passwordForm.password;
                passwordForm = null;
                cs = cs.Replace("[3]",password);
			}
			//Set up cn
			SqlConnection cn = new SqlConnection();
			if (databaseType == "msSql")
			{
				cn.ConnectionString = cs;
			}
			else
			{
				cn.ConnectionString = cs;
			}

			//Try to open connection
			try
			{
				cn.Open();

				if (Information.Err().Description != "")
				{
					MessageBox.Show("We are sorry to report that the connection failed. " + Environment.NewLine + "Your connection string : " + Environment.NewLine + noPasswordCs + Environment.NewLine + "VB error message:" + Environment.NewLine + Information.Err().Description);
				}
				else
				{
					MessageBox.Show("Test passed.");
					cn.Close();
					cmdOK.Enabled = true;
					// Helper.regitPush("DatabaseList", "ComboString", cmbStrings.Text, 8);				
				}
			}
            //ASM catch (Exception exc)
			catch
            {
                //ASM     NotUpgradedHelper.NotifyNotUpgradedElement("Resume in On-Error-Resume-Next Block");
                MessageBox.Show("We are sorry to report that the connection failed. " + Environment.NewLine + "Your connection string : " + Environment.NewLine + noPasswordCs + Environment.NewLine + "VB error message:" + Environment.NewLine + Information.Err().Description);
            }

    }

        private bool isInitializingComponent;

        private void _optSpecialDatabase_0_CheckedChanged(object sender, EventArgs e)
        {
			specialDatabase = "transcript";
        }

        private void _optSpecialDatabase_1_CheckedChanged(object sender, EventArgs e)
        {
			specialDatabase = "addressBook";
        }

        private void _optSpecialDatabase_2_CheckedChanged(object sender, EventArgs e)
        {
			specialDatabase = "";   //Neither
        }

 		private void Form_Closed(Object eventSender, EventArgs eventArgs)
		{
		}
	}
}