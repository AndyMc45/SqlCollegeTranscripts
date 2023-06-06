namespace SqlCollegeTranscripts
{
    partial class frmDatabaseInfo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            toolStrip1 = new ToolStrip();
            cmdTables = new ToolStripButton();
            cmdFields = new ToolStripButton();
            cmdForeignKeys = new ToolStripButton();
            cmdIndexes = new ToolStripButton();
            cmdIndexColumns = new ToolStripButton();
            cmdComboDT = new ToolStripButton();
            toolStripLabel1 = new ToolStripLabel();
            btnExit = new ToolStripButton();
            dgvMain = new DataGridView();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMain).BeginInit();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(24, 24);
            toolStrip1.Items.AddRange(new ToolStripItem[] { cmdTables, cmdFields, cmdForeignKeys, cmdIndexes, cmdIndexColumns, cmdComboDT, toolStripLabel1, btnExit });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1182, 27);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // cmdTables
            // 
            cmdTables.DisplayStyle = ToolStripItemDisplayStyle.Text;
            cmdTables.ImageTransparentColor = Color.Magenta;
            cmdTables.Name = "cmdTables";
            cmdTables.Size = new Size(54, 24);
            cmdTables.Text = "Tables";
            cmdTables.Click += cmdTables_Click;
            // 
            // cmdFields
            // 
            cmdFields.DisplayStyle = ToolStripItemDisplayStyle.Text;
            cmdFields.ImageTransparentColor = Color.Magenta;
            cmdFields.Name = "cmdFields";
            cmdFields.Size = new Size(51, 24);
            cmdFields.Text = "Fields";
            cmdFields.Click += cmdFields_Click;
            // 
            // cmdForeignKeys
            // 
            cmdForeignKeys.DisplayStyle = ToolStripItemDisplayStyle.Text;
            cmdForeignKeys.ImageTransparentColor = Color.Magenta;
            cmdForeignKeys.Name = "cmdForeignKeys";
            cmdForeignKeys.Size = new Size(97, 24);
            cmdForeignKeys.Text = "Foreign Keys";
            cmdForeignKeys.Click += cmdForeignKeys_Click;
            // 
            // cmdIndexes
            // 
            cmdIndexes.DisplayStyle = ToolStripItemDisplayStyle.Text;
            cmdIndexes.ImageTransparentColor = Color.Magenta;
            cmdIndexes.Name = "cmdIndexes";
            cmdIndexes.Size = new Size(63, 24);
            cmdIndexes.Text = "Indexes";
            cmdIndexes.Click += cmdIndexes_Click;
            // 
            // cmdIndexColumns
            // 
            cmdIndexColumns.DisplayStyle = ToolStripItemDisplayStyle.Text;
            cmdIndexColumns.ImageTransparentColor = Color.Magenta;
            cmdIndexColumns.Name = "cmdIndexColumns";
            cmdIndexColumns.RightToLeftAutoMirrorImage = true;
            cmdIndexColumns.Size = new Size(110, 24);
            cmdIndexColumns.Text = "Index-columns";
            cmdIndexColumns.Click += cmdIndexColumns_Click;
            // 
            // cmdComboDT
            // 
            cmdComboDT.DisplayStyle = ToolStripItemDisplayStyle.Text;
            cmdComboDT.ImageTransparentColor = Color.Magenta;
            cmdComboDT.Name = "cmdComboDT";
            cmdComboDT.Size = new Size(80, 24);
            cmdComboDT.Text = "ComboDT";
            cmdComboDT.Click += cmdComboDT_Click;
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(84, 24);
            toolStripLabel1.Text = "lblMessage";
            // 
            // btnExit
            // 
            btnExit.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnExit.ImageTransparentColor = Color.Magenta;
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(37, 24);
            btnExit.Text = "Exit";
            btnExit.Click += btnExit_Click;
            // 
            // dgvMain
            // 
            dgvMain.AllowUserToAddRows = false;
            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvMain.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMain.Dock = DockStyle.Fill;
            dgvMain.Location = new Point(0, 27);
            dgvMain.Margin = new Padding(2);
            dgvMain.Name = "dgvMain";
            dgvMain.RowHeadersWidth = 62;
            dgvMain.RowTemplate.Height = 33;
            dgvMain.Size = new Size(1182, 333);
            dgvMain.TabIndex = 1;
            // 
            // frmDatabaseInfo
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1182, 360);
            Controls.Add(dgvMain);
            Controls.Add(toolStrip1);
            Margin = new Padding(2);
            Name = "frmDatabaseInfo";
            Text = "dgvMaintenance";
            FormClosed += frmDatabaseInfo_FormClosed;
            Load += frmDatabaseInfo_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMain).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private ToolStripButton cmdTables;
        private DataGridView dgvMain;
        private ToolStripButton cmdFields;
        private ToolStripButton cmdForeignKeys;
        private ToolStripButton cmdIndexes;
        private ToolStripButton cmdIndexColumns;
        private ToolStripLabel toolStripLabel1;
        private ToolStripButton cmdComboDT;
        private ToolStripButton btnExit;
    }
}