
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
        private string[] visualControls = new string[] { "components", "ToolTipMain", "cmdDelete", "cmdExit", "lstDatabaseList", "listBoxHelper1", "commandButtonHelper1" };
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
            components = new System.ComponentModel.Container();
            ToolTipMain = new ToolTip(components);
            cmdExit = new Button();
            listBox1 = new ListBox();
            SuspendLayout();
            // 
            // cmdExit
            // 
            cmdExit.AllowDrop = true;
            cmdExit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cmdExit.BackColor = SystemColors.Control;
            cmdExit.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            cmdExit.ForeColor = SystemColors.ControlText;
            cmdExit.Location = new Point(224, 232);
            cmdExit.Name = "cmdExit";
            cmdExit.RightToLeft = RightToLeft.No;
            cmdExit.Size = new Size(89, 25);
            cmdExit.TabIndex = 1;
            cmdExit.Text = "resExit";
            cmdExit.TextImageRelation = TextImageRelation.ImageAboveText;
            cmdExit.UseVisualStyleBackColor = false;
            cmdExit.Click += cmdExit_Click;
            // 
            // listBox1
            // 
            listBox1.AllowDrop = true;
            listBox1.BackColor = SystemColors.Window;
            listBox1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            listBox1.ForeColor = SystemColors.WindowText;
            listBox1.ItemHeight = 20;
            listBox1.Location = new Point(24, 24);
            listBox1.Name = "listBox1";
            listBox1.RightToLeft = RightToLeft.No;
            listBox1.Size = new Size(345, 164);
            listBox1.TabIndex = 0;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            // 
            // frmListItems
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(10F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            ClientSize = new Size(397, 268);
            Controls.Add(cmdExit);
            Controls.Add(listBox1);
            Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            Location = new Point(3, 22);
            Name = "frmListItems";
            RightToLeft = RightToLeft.No;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Caption";
            Load += frmListItems_Load;
            Resize += frmListItems_Resize;
            ResumeLayout(false);
        }
        #endregion
    }
}