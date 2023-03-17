
namespace SqlCollegeTranscripts
{
	partial class frmListItems
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
		public System.Windows.Forms.Button cmdExit;
		public System.Windows.Forms.ListBox listBox1;
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.ToolTipMain = new System.Windows.Forms.ToolTip(this.components);
            this.cmdExit = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
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
            // listBox1
            // 
            this.listBox1.AllowDrop = true;
            this.listBox1.BackColor = System.Drawing.SystemColors.Window;
            this.listBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.listBox1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.listBox1.ItemHeight = 17;
            this.listBox1.Location = new System.Drawing.Point(24, 24);
            this.listBox1.Name = "listBox1";
            this.listBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.listBox1.Size = new System.Drawing.Size(345, 174);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.lstDatabaseList_SelectedIndexChanged);
            // 
            // frmListItems
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(397, 268);
            this.Controls.Add(this.cmdExit);
            this.Controls.Add(this.listBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Location = new System.Drawing.Point(3, 22);
            this.Name = "frmListItems";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Caption";
            this.Load += new System.EventHandler(this.frmListItems_Load);
            this.ResumeLayout(false);

		}
		#endregion
	}
}