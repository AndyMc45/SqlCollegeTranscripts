
namespace AccessFreeData
{
	partial class frmConnection
	{
		#region "Windows Form Designer generated code "

		private string[] visualControls = new string[]{"components", "ToolTipMain", "cmdDatabaseList", "optAccess", "optMsSql", "Frame1", "_optSpecialDatabase_2", "_optSpecialDatabase_1", "_optSpecialDatabase_0", "txtUserId", "CommonDialog", "chkReadOnly", "txtServer", "txtShortName", "txtPath", "cmdFile", "txtHelp", "cmbStrings", "cmdTest", "cmdCancel", "cmdOK", "Label4", "Label7", "Label6", "Label5", "Label3", "Label2", "Label1", "lblConnection", "optSpecialDatabase", "commandButtonHelper1"};

		//Required by the Windows Form Designer
		private System.ComponentModel.IContainer components;
		public System.Windows.Forms.ToolTip ToolTipMain;
		public System.Windows.Forms.Button cmdDatabaseList;
		private System.Windows.Forms.RadioButton _optSpecialDatabase_2;
		private System.Windows.Forms.RadioButton _optSpecialDatabase_1;
		private System.Windows.Forms.RadioButton _optSpecialDatabase_0;
		public System.Windows.Forms.TextBox txtUserId;
		public System.Windows.Forms.OpenFileDialog CommonDialogOpen;
		public System.Windows.Forms.SaveFileDialog CommonDialogSave;
		public System.Windows.Forms.FontDialog CommonDialogFont;
		public System.Windows.Forms.ColorDialog CommonDialogColor;
		public System.Windows.Forms.PrintDialog CommonDialogPrint;
		public System.Windows.Forms.CheckBox chkReadOnly;
		public System.Windows.Forms.TextBox txtServer;
		public System.Windows.Forms.TextBox txtDatabase;
		public System.Windows.Forms.TextBox txtHelp;
		public System.Windows.Forms.ComboBox cmbStrings;
		public System.Windows.Forms.Button cmdTest;
		public System.Windows.Forms.Button cmdCancel;
		public System.Windows.Forms.Button cmdOK;
		public System.Windows.Forms.Label Label7;
		public System.Windows.Forms.Label Label6;
		public System.Windows.Forms.Label lblUserID;
		public System.Windows.Forms.Label lblServer;
		public System.Windows.Forms.Label lblDatabase;
		public System.Windows.Forms.Label lblConnection;
		public System.Windows.Forms.RadioButton[] optSpecialDatabase = new System.Windows.Forms.RadioButton[3];
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.ToolTipMain = new System.Windows.Forms.ToolTip(this.components);
            this.cmdDatabaseList = new System.Windows.Forms.Button();
            this._optSpecialDatabase_2 = new System.Windows.Forms.RadioButton();
            this._optSpecialDatabase_1 = new System.Windows.Forms.RadioButton();
            this._optSpecialDatabase_0 = new System.Windows.Forms.RadioButton();
            this.txtUserId = new System.Windows.Forms.TextBox();
            this.CommonDialogOpen = new System.Windows.Forms.OpenFileDialog();
            this.CommonDialogSave = new System.Windows.Forms.SaveFileDialog();
            this.CommonDialogFont = new System.Windows.Forms.FontDialog();
            this.CommonDialogColor = new System.Windows.Forms.ColorDialog();
            this.CommonDialogPrint = new System.Windows.Forms.PrintDialog();
            this.chkReadOnly = new System.Windows.Forms.CheckBox();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.txtHelp = new System.Windows.Forms.TextBox();
            this.cmbStrings = new System.Windows.Forms.ComboBox();
            this.cmdTest = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.Label7 = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.lblUserID = new System.Windows.Forms.Label();
            this.lblServer = new System.Windows.Forms.Label();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.lblConnection = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdDatabaseList
            // 
            this.cmdDatabaseList.AllowDrop = true;
            this.cmdDatabaseList.BackColor = System.Drawing.SystemColors.Control;
            this.cmdDatabaseList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmdDatabaseList.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdDatabaseList.Location = new System.Drawing.Point(664, 35);
            this.cmdDatabaseList.Name = "cmdDatabaseList";
            this.cmdDatabaseList.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdDatabaseList.Size = new System.Drawing.Size(161, 26);
            this.cmdDatabaseList.TabIndex = 25;
            this.cmdDatabaseList.Text = "Sql database list";
            this.cmdDatabaseList.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.cmdDatabaseList.UseVisualStyleBackColor = false;
            this.cmdDatabaseList.Click += new System.EventHandler(this.cmdDatabaseList_Click);
            // 
            // _optSpecialDatabase_2
            // 
            this._optSpecialDatabase_2.AllowDrop = true;
            this._optSpecialDatabase_2.BackColor = System.Drawing.SystemColors.Control;
            this._optSpecialDatabase_2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this._optSpecialDatabase_2.ForeColor = System.Drawing.SystemColors.ControlText;
            this._optSpecialDatabase_2.Location = new System.Drawing.Point(55, 219);
            this._optSpecialDatabase_2.Name = "_optSpecialDatabase_2";
            this._optSpecialDatabase_2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._optSpecialDatabase_2.Size = new System.Drawing.Size(137, 22);
            this._optSpecialDatabase_2.TabIndex = 20;
            this._optSpecialDatabase_2.TabStop = true;
            this._optSpecialDatabase_2.Tag = "noneOfAbove";
            this._optSpecialDatabase_2.Text = "Neither";
            this._optSpecialDatabase_2.UseVisualStyleBackColor = false;
            this._optSpecialDatabase_2.CheckedChanged += new System.EventHandler(this._optSpecialDatabase_2_CheckedChanged);
            // 
            // _optSpecialDatabase_1
            // 
            this._optSpecialDatabase_1.AllowDrop = true;
            this._optSpecialDatabase_1.BackColor = System.Drawing.SystemColors.Control;
            this._optSpecialDatabase_1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this._optSpecialDatabase_1.ForeColor = System.Drawing.SystemColors.ControlText;
            this._optSpecialDatabase_1.Location = new System.Drawing.Point(55, 192);
            this._optSpecialDatabase_1.Name = "_optSpecialDatabase_1";
            this._optSpecialDatabase_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._optSpecialDatabase_1.Size = new System.Drawing.Size(113, 21);
            this._optSpecialDatabase_1.TabIndex = 19;
            this._optSpecialDatabase_1.TabStop = true;
            this._optSpecialDatabase_1.Tag = "addressBook";
            this._optSpecialDatabase_1.Text = "Address book";
            this._optSpecialDatabase_1.UseVisualStyleBackColor = false;
            this._optSpecialDatabase_1.CheckedChanged += new System.EventHandler(this._optSpecialDatabase_1_CheckedChanged);
            // 
            // _optSpecialDatabase_0
            // 
            this._optSpecialDatabase_0.AllowDrop = true;
            this._optSpecialDatabase_0.BackColor = System.Drawing.SystemColors.Control;
            this._optSpecialDatabase_0.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this._optSpecialDatabase_0.ForeColor = System.Drawing.SystemColors.ControlText;
            this._optSpecialDatabase_0.Location = new System.Drawing.Point(55, 164);
            this._optSpecialDatabase_0.Name = "_optSpecialDatabase_0";
            this._optSpecialDatabase_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._optSpecialDatabase_0.Size = new System.Drawing.Size(113, 21);
            this._optSpecialDatabase_0.TabIndex = 18;
            this._optSpecialDatabase_0.TabStop = true;
            this._optSpecialDatabase_0.Tag = "transcript";
            this._optSpecialDatabase_0.Text = "Transcript";
            this._optSpecialDatabase_0.UseVisualStyleBackColor = false;
            this._optSpecialDatabase_0.CheckedChanged += new System.EventHandler(this._optSpecialDatabase_0_CheckedChanged);
            // 
            // txtUserId
            // 
            this.txtUserId.AcceptsReturn = true;
            this.txtUserId.AllowDrop = true;
            this.txtUserId.BackColor = System.Drawing.SystemColors.Window;
            this.txtUserId.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtUserId.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtUserId.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtUserId.Location = new System.Drawing.Point(488, 35);
            this.txtUserId.MaxLength = 0;
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtUserId.Size = new System.Drawing.Size(161, 23);
            this.txtUserId.TabIndex = 15;
            // 
            // chkReadOnly
            // 
            this.chkReadOnly.AllowDrop = true;
            this.chkReadOnly.BackColor = System.Drawing.SystemColors.Control;
            this.chkReadOnly.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkReadOnly.ForeColor = System.Drawing.SystemColors.ControlText;
            this.chkReadOnly.Location = new System.Drawing.Point(207, 164);
            this.chkReadOnly.Name = "chkReadOnly";
            this.chkReadOnly.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.chkReadOnly.Size = new System.Drawing.Size(137, 33);
            this.chkReadOnly.TabIndex = 13;
            this.chkReadOnly.Text = "Read only";
            this.chkReadOnly.UseVisualStyleBackColor = false;
            // 
            // txtServer
            // 
            this.txtServer.AcceptsReturn = true;
            this.txtServer.AllowDrop = true;
            this.txtServer.BackColor = System.Drawing.SystemColors.Window;
            this.txtServer.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtServer.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtServer.Location = new System.Drawing.Point(8, 35);
            this.txtServer.MaxLength = 0;
            this.txtServer.Name = "txtServer";
            this.txtServer.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtServer.Size = new System.Drawing.Size(209, 23);
            this.txtServer.TabIndex = 12;
            // 
            // txtDatabase
            // 
            this.txtDatabase.AcceptsReturn = true;
            this.txtDatabase.AllowDrop = true;
            this.txtDatabase.BackColor = System.Drawing.SystemColors.Window;
            this.txtDatabase.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtDatabase.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtDatabase.Location = new System.Drawing.Point(264, 35);
            this.txtDatabase.MaxLength = 0;
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtDatabase.Size = new System.Drawing.Size(169, 23);
            this.txtDatabase.TabIndex = 8;
            // 
            // txtHelp
            // 
            this.txtHelp.AcceptsReturn = true;
            this.txtHelp.AllowDrop = true;
            this.txtHelp.BackColor = System.Drawing.SystemColors.Control;
            this.txtHelp.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtHelp.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtHelp.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtHelp.Location = new System.Drawing.Point(8, 247);
            this.txtHelp.MaxLength = 0;
            this.txtHelp.Multiline = true;
            this.txtHelp.Name = "txtHelp";
            this.txtHelp.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtHelp.Size = new System.Drawing.Size(809, 42);
            this.txtHelp.TabIndex = 5;
            this.txtHelp.Text = "Text1";
            // 
            // cmbStrings
            // 
            this.cmbStrings.AllowDrop = true;
            this.cmbStrings.BackColor = System.Drawing.SystemColors.Window;
            this.cmbStrings.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmbStrings.ForeColor = System.Drawing.SystemColors.WindowText;
            this.cmbStrings.Location = new System.Drawing.Point(8, 96);
            this.cmbStrings.Name = "cmbStrings";
            this.cmbStrings.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmbStrings.Size = new System.Drawing.Size(809, 25);
            this.cmbStrings.TabIndex = 3;
            // 
            // cmdTest
            // 
            this.cmdTest.AllowDrop = true;
            this.cmdTest.BackColor = System.Drawing.SystemColors.Control;
            this.cmdTest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmdTest.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdTest.Location = new System.Drawing.Point(552, 184);
            this.cmdTest.Name = "cmdTest";
            this.cmdTest.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdTest.Size = new System.Drawing.Size(81, 31);
            this.cmdTest.TabIndex = 2;
            this.cmdTest.Text = "Test";
            this.cmdTest.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.cmdTest.UseVisualStyleBackColor = false;
            this.cmdTest.Click += new System.EventHandler(this.cmdTest_Click_1);
            // 
            // cmdCancel
            // 
            this.cmdCancel.AllowDrop = true;
            this.cmdCancel.BackColor = System.Drawing.SystemColors.Control;
            this.cmdCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmdCancel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdCancel.Location = new System.Drawing.Point(752, 184);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdCancel.Size = new System.Drawing.Size(73, 32);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.cmdCancel.UseVisualStyleBackColor = false;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.AllowDrop = true;
            this.cmdOK.BackColor = System.Drawing.SystemColors.Control;
            this.cmdOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmdOK.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdOK.Location = new System.Drawing.Point(652, 184);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdOK.Size = new System.Drawing.Size(81, 31);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "O.K.";
            this.cmdOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.cmdOK.UseVisualStyleBackColor = false;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // Label7
            // 
            this.Label7.AllowDrop = true;
            this.Label7.BackColor = System.Drawing.SystemColors.Control;
            this.Label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label7.Location = new System.Drawing.Point(207, 136);
            this.Label7.Name = "Label7";
            this.Label7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label7.Size = new System.Drawing.Size(129, 17);
            this.Label7.TabIndex = 17;
            this.Label7.Text = "Read only database:";
            // 
            // Label6
            // 
            this.Label6.AllowDrop = true;
            this.Label6.BackColor = System.Drawing.SystemColors.Control;
            this.Label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Label6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label6.Location = new System.Drawing.Point(21, 135);
            this.Label6.Name = "Label6";
            this.Label6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label6.Size = new System.Drawing.Size(121, 18);
            this.Label6.TabIndex = 16;
            this.Label6.Text = "School Databases:";
            // 
            // lblUserID
            // 
            this.lblUserID.AllowDrop = true;
            this.lblUserID.BackColor = System.Drawing.SystemColors.Control;
            this.lblUserID.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblUserID.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblUserID.Location = new System.Drawing.Point(488, 15);
            this.lblUserID.Name = "lblUserID";
            this.lblUserID.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblUserID.Size = new System.Drawing.Size(153, 17);
            this.lblUserID.TabIndex = 14;
            this.lblUserID.Text = "[2] User ID";
            // 
            // lblServer
            // 
            this.lblServer.AllowDrop = true;
            this.lblServer.BackColor = System.Drawing.SystemColors.Control;
            this.lblServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblServer.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblServer.Location = new System.Drawing.Point(8, 15);
            this.lblServer.Name = "lblServer";
            this.lblServer.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblServer.Size = new System.Drawing.Size(209, 17);
            this.lblServer.TabIndex = 11;
            this.lblServer.Text = "[0] Server: ";
            // 
            // lblDatabase
            // 
            this.lblDatabase.AllowDrop = true;
            this.lblDatabase.BackColor = System.Drawing.SystemColors.Control;
            this.lblDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblDatabase.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblDatabase.Location = new System.Drawing.Point(264, 15);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblDatabase.Size = new System.Drawing.Size(169, 17);
            this.lblDatabase.TabIndex = 9;
            this.lblDatabase.Text = "[1] Database:";
            // 
            // lblConnection
            // 
            this.lblConnection.AllowDrop = true;
            this.lblConnection.BackColor = System.Drawing.SystemColors.Control;
            this.lblConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblConnection.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblConnection.Location = new System.Drawing.Point(8, 67);
            this.lblConnection.Name = "lblConnection";
            this.lblConnection.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblConnection.Size = new System.Drawing.Size(809, 17);
            this.lblConnection.TabIndex = 4;
            // 
            // frmConnection
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(829, 297);
            this.Controls.Add(this.cmdDatabaseList);
            this.Controls.Add(this._optSpecialDatabase_2);
            this.Controls.Add(this._optSpecialDatabase_1);
            this.Controls.Add(this._optSpecialDatabase_0);
            this.Controls.Add(this.txtUserId);
            this.Controls.Add(this.chkReadOnly);
            this.Controls.Add(this.txtServer);
            this.Controls.Add(this.txtDatabase);
            this.Controls.Add(this.txtHelp);
            this.Controls.Add(this.cmbStrings);
            this.Controls.Add(this.cmdTest);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.Label7);
            this.Controls.Add(this.Label6);
            this.Controls.Add(this.lblUserID);
            this.Controls.Add(this.lblServer);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.lblConnection);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Location = new System.Drawing.Point(4, 30);
            this.Name = "frmConnection";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Connection String";
            this.Load += new System.EventHandler(this.frmConnection_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		void ReLoadForm(bool addEvents)
		{
			InitializeoptSpecialDatabase();
		}
		void InitializeoptSpecialDatabase()
		{
			this.optSpecialDatabase = new System.Windows.Forms.RadioButton[3];
			this.optSpecialDatabase[2] = _optSpecialDatabase_2;
			this.optSpecialDatabase[1] = _optSpecialDatabase_1;
			this.optSpecialDatabase[0] = _optSpecialDatabase_0;
		}
		#endregion
	}
}