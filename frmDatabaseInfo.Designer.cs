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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.cmdTables = new System.Windows.Forms.ToolStripButton();
            this.cmdFields = new System.Windows.Forms.ToolStripButton();
            this.cmdForeignKeys = new System.Windows.Forms.ToolStripButton();
            this.cmdIndexes = new System.Windows.Forms.ToolStripButton();
            this.cmdIndexColumns = new System.Windows.Forms.ToolStripButton();
            this.cmdExtraDT = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.btnExit = new System.Windows.Forms.ToolStripButton();
            this.dgvMain = new System.Windows.Forms.DataGridView();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmdTables,
            this.cmdFields,
            this.cmdForeignKeys,
            this.cmdIndexes,
            this.cmdIndexColumns,
            this.cmdExtraDT,
            this.toolStripLabel1,
            this.btnExit});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1182, 27);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // cmdTables
            // 
            this.cmdTables.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cmdTables.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdTables.Name = "cmdTables";
            this.cmdTables.Size = new System.Drawing.Size(54, 24);
            this.cmdTables.Text = "Tables";
            this.cmdTables.Click += new System.EventHandler(this.cmdTables_Click);
            // 
            // cmdFields
            // 
            this.cmdFields.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cmdFields.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdFields.Name = "cmdFields";
            this.cmdFields.Size = new System.Drawing.Size(51, 24);
            this.cmdFields.Text = "Fields";
            this.cmdFields.Click += new System.EventHandler(this.cmdFields_Click);
            // 
            // cmdForeignKeys
            // 
            this.cmdForeignKeys.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cmdForeignKeys.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdForeignKeys.Name = "cmdForeignKeys";
            this.cmdForeignKeys.Size = new System.Drawing.Size(97, 24);
            this.cmdForeignKeys.Text = "Foreign Keys";
            this.cmdForeignKeys.Click += new System.EventHandler(this.cmdForeignKeys_Click);
            // 
            // cmdIndexes
            // 
            this.cmdIndexes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cmdIndexes.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdIndexes.Name = "cmdIndexes";
            this.cmdIndexes.Size = new System.Drawing.Size(63, 24);
            this.cmdIndexes.Text = "Indexes";
            this.cmdIndexes.Click += new System.EventHandler(this.cmdIndexes_Click);
            // 
            // cmdIndexColumns
            // 
            this.cmdIndexColumns.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cmdIndexColumns.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdIndexColumns.Name = "cmdIndexColumns";
            this.cmdIndexColumns.RightToLeftAutoMirrorImage = true;
            this.cmdIndexColumns.Size = new System.Drawing.Size(110, 24);
            this.cmdIndexColumns.Text = "Index-columns";
            this.cmdIndexColumns.Click += new System.EventHandler(this.cmdIndexColumns_Click);
            // 
            // cmdExtraDT
            // 
            this.cmdExtraDT.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cmdExtraDT.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdExtraDT.Name = "cmdExtraDT";
            this.cmdExtraDT.Size = new System.Drawing.Size(64, 24);
            this.cmdExtraDT.Text = "ExtraDT";
            this.cmdExtraDT.Click += new System.EventHandler(this.cmdExtraDT_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(84, 24);
            this.toolStripLabel1.Text = "lblMessage";
            // 
            // btnExit
            // 
            this.btnExit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnExit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(37, 24);
            this.btnExit.Text = "Exit";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // dgvMain
            // 
            this.dgvMain.AllowUserToAddRows = false;
            this.dgvMain.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvMain.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMain.Location = new System.Drawing.Point(0, 27);
            this.dgvMain.Margin = new System.Windows.Forms.Padding(2);
            this.dgvMain.Name = "dgvMain";
            this.dgvMain.RowHeadersWidth = 62;
            this.dgvMain.RowTemplate.Height = 33;
            this.dgvMain.Size = new System.Drawing.Size(1182, 333);
            this.dgvMain.TabIndex = 1;
            // 
            // frmDatabaseInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1182, 360);
            this.Controls.Add(this.dgvMain);
            this.Controls.Add(this.toolStrip1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "frmDatabaseInfo";
            this.Text = "dgvMaintenance";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmDatabaseInfo_FormClosed);
            this.Load += new System.EventHandler(this.frmDatabaseInfo_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private ToolStripButton cmdExtraDT;
        private ToolStripButton btnExit;
    }
}