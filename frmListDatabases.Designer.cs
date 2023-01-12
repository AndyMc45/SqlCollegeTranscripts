
namespace AccessFreeData
{
	partial class frmListDatabases
	{

		#region "Windows Form Designer generated code "
		//public static frmDeleteDatabase CreateInstance()
		//{
		//	frmDeleteDatabase theInstance = new frmDeleteDatabase();
		//	theInstance.Form_Load();
		//	return theInstance;
		//}
		private string[] visualControls = new string[]{"components", "ToolTipMain", "cmdDelete", "cmdExit", "lstDatabaseList", "listBoxHelper1", "commandButtonHelper1"};
		//Required by the Windows Form Designer
		private System.ComponentModel.IContainer components;
		public System.Windows.Forms.ToolTip ToolTipMain;
		public System.Windows.Forms.Button cmdDelete;
		public System.Windows.Forms.Button cmdExit;
		public System.Windows.Forms.ListBox lstDatabaseList;
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.ToolTipMain = new System.Windows.Forms.ToolTip(this.components);
            this.cmdDelete = new System.Windows.Forms.Button();
            this.cmdExit = new System.Windows.Forms.Button();
            this.lstDatabaseList = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // cmdDelete
            // 
            this.cmdDelete.AllowDrop = true;
            this.cmdDelete.BackColor = System.Drawing.SystemColors.Control;
            this.cmdDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmdDelete.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdDelete.Location = new System.Drawing.Point(88, 232);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdDelete.Size = new System.Drawing.Size(89, 25);
            this.cmdDelete.TabIndex = 2;
            this.cmdDelete.Text = "resDelete";
            this.cmdDelete.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.cmdDelete.UseVisualStyleBackColor = false;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // cmdExit
            // 
            this.cmdExit.AllowDrop = true;
            this.cmdExit.BackColor = System.Drawing.SystemColors.Control;
            this.cmdExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmdExit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdExit.Location = new System.Drawing.Point(224, 232);
            this.cmdExit.Name = "cmdExit";
            this.cmdExit.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdExit.Size = new System.Drawing.Size(89, 25);
            this.cmdExit.TabIndex = 1;
            this.cmdExit.Text = "resExit";
            this.cmdExit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.cmdExit.UseVisualStyleBackColor = false;
            this.cmdExit.Click += new System.EventHandler(this.cmdExit_Click);
            // 
            // lstDatabaseList
            // 
            this.lstDatabaseList.AllowDrop = true;
            this.lstDatabaseList.BackColor = System.Drawing.SystemColors.Window;
            this.lstDatabaseList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lstDatabaseList.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lstDatabaseList.ItemHeight = 17;
            this.lstDatabaseList.Location = new System.Drawing.Point(24, 24);
            this.lstDatabaseList.Name = "lstDatabaseList";
            this.lstDatabaseList.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lstDatabaseList.Size = new System.Drawing.Size(345, 174);
            this.lstDatabaseList.TabIndex = 0;
            this.lstDatabaseList.SelectedIndexChanged += new System.EventHandler(this.lstDatabaseList_SelectedIndexChanged);
            // 
            // frmDeleteDatabase
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(397, 268);
            this.Controls.Add(this.cmdDelete);
            this.Controls.Add(this.cmdExit);
            this.Controls.Add(this.lstDatabaseList);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Location = new System.Drawing.Point(3, 22);
            this.Name = "frmDeleteDatabase";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "resDeleteDatabase";
            this.Closed += new System.EventHandler(this.Form_Closed);
            this.Load += new System.EventHandler(this.frmDeleteDatabase_Load_1);
            this.ResumeLayout(false);

		}
		#endregion
	}
}