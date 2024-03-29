﻿namespace SqlCollegeTranscripts
{
    partial class DataGridViewForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataGridViewForm));
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            lblMainFilter = new Label();
            MainMenu1 = new MenuStrip();
            mnuFile = new ToolStripMenuItem();
            mnuConnections = new ToolStripMenuItem();
            mnuConnectionList = new ToolStripMenuItem();
            mnuBlankLine3 = new ToolStripMenuItem();
            mnuAddConnection = new ToolStripMenuItem();
            mnuDeleteConnection = new ToolStripMenuItem();
            mnuClose = new ToolStripMenuItem();
            mnuOpenTables = new ToolStripMenuItem();
            mnuTranscript = new ToolStripMenuItem();
            mnuTranscriptCheckGradRequirements = new ToolStripMenuItem();
            mnuTranscriptPrint = new ToolStripMenuItem();
            mnuPrintTranscript = new ToolStripMenuItem();
            mnuPrintTranscriptEnglish = new ToolStripMenuItem();
            mnuPrintRole = new ToolStripMenuItem();
            mnuPrintGrade = new ToolStripMenuItem();
            mnuLocations = new ToolStripMenuItem();
            mnuTranscriptFolder = new ToolStripMenuItem();
            mnuTranscriptTemplate = new ToolStripMenuItem();
            mnuTranscriptTemplateEnglish = new ToolStripMenuItem();
            mnuRoleTemplate = new ToolStripMenuItem();
            mnuGradeTemplate = new ToolStripMenuItem();
            mnuLine = new ToolStripMenuItem();
            mnuPrintTermSummary = new ToolStripMenuItem();
            mnuAddressBook = new ToolStripMenuItem();
            mnuAddressBookLabels = new ToolStripMenuItem();
            mnuAddressBookPhoneNumbers = new ToolStripMenuItem();
            mnuAddressBookAddresses = new ToolStripMenuItem();
            mnuAddressBookEmails = new ToolStripMenuItem();
            mnuAddressBookGetEmails = new ToolStripMenuItem();
            mnuTools = new ToolStripMenuItem();
            mnuDatabaseInfo = new ToolStripMenuItem();
            mnuDuplicateDisplayKeys = new ToolStripMenuItem();
            mnuForeignKeyMissing = new ToolStripMenuItem();
            mnuPrintCurrentTable = new ToolStripMenuItem();
            mnuViewLog = new ToolStripMenuItem();
            mnuHelp = new ToolStripMenuItem();
            mnuHelpFile = new ToolStripMenuItem();
            toolStripBottom = new ToolStrip();
            toolStripMsg = new ToolStripLabel();
            txtRecordsPerPage = new ToolStripTextBox();
            toolStripButton5 = new ToolStripButton();
            toolStripButton4 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton1 = new ToolStripButton();
            toolStripButtonColumnWidth = new ToolStripButton();
            splitContainer1 = new SplitContainer();
            tableLayoutPanel = new TableLayoutPanel();
            cmbMainFilter = new ComboBox();
            lblGridFilter = new Label();
            cmbGridFilterFields_0 = new ComboBox();
            cmbGridFilterFields_1 = new ComboBox();
            cmbGridFilterValue_3 = new ComboBox();
            cmbGridFilterFields_4 = new ComboBox();
            cmbGridFilterValue_4 = new ComboBox();
            cmbGridFilterFields_5 = new ComboBox();
            cmbGridFilterValue_1 = new ComboBox();
            cmbGridFilterValue_5 = new ComboBox();
            cmbGridFilterValue_2 = new ComboBox();
            cmbGridFilterFields_2 = new ComboBox();
            cmbGridFilterValue_0 = new ComboBox();
            cmbGridFilterFields_3 = new ComboBox();
            rbMerge = new RadioButton();
            btnDeleteAddMerge = new Button();
            GridContextMenu = new ContextMenuStrip(components);
            GridContextMenu_FindInDescendent = new ToolStripMenuItem();
            GridContextMenu_FindInAncestor = new ToolStripMenuItem();
            GridContextMenu_TimesUsedAsFK = new ToolStripMenuItem();
            deleteUnusedForeignKeysToolStripMenuItem = new ToolStripMenuItem();
            correctDuplicateDKsToolStripMenuItem = new ToolStripMenuItem();
            rbAdd = new RadioButton();
            rbDelete = new RadioButton();
            rbEdit = new RadioButton();
            rbView = new RadioButton();
            lblComboFilter = new Label();
            cmbGridFilterFields_7 = new ComboBox();
            cmbGridFilterValue_6 = new ComboBox();
            cmbGridFilterFields_6 = new ComboBox();
            cmbGridFilterValue_7 = new ComboBox();
            cmbGridFilterFields_8 = new ComboBox();
            cmbGridFilterValue_8 = new ComboBox();
            lblCmbFilterField_3 = new Label();
            cmbComboFilterValue_3 = new ComboBox();
            lblCmbFilterField_4 = new Label();
            cmbComboFilterValue_4 = new ComboBox();
            lblCmbFilterField_5 = new Label();
            cmbComboFilterValue_5 = new ComboBox();
            lblCmbFilterField_0 = new Label();
            cmbComboFilterValue_0 = new ComboBox();
            lblCmbFilterField_1 = new Label();
            cmbComboFilterValue_1 = new ComboBox();
            lblCmbFilterField_2 = new Label();
            cmbComboFilterValue_2 = new ComboBox();
            cmbComboTableList = new ComboBox();
            button1 = new Button();
            btnReload = new Button();
            txtMessages = new TextBox();
            dataGridView1 = new DataGridView();
            MainMenu1.SuspendLayout();
            toolStripBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            GridContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // lblMainFilter
            // 
            lblMainFilter.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(lblMainFilter, 3);
            lblMainFilter.Location = new Point(9, 4);
            lblMainFilter.Margin = new Padding(4);
            lblMainFilter.Name = "lblMainFilter";
            lblMainFilter.Size = new Size(178, 44);
            lblMainFilter.TabIndex = 107;
            lblMainFilter.Text = "lblMainFilter";
            lblMainFilter.TextAlign = ContentAlignment.TopRight;
            // 
            // MainMenu1
            // 
            MainMenu1.BackColor = SystemColors.ControlLight;
            MainMenu1.ImageScalingSize = new Size(20, 20);
            MainMenu1.Items.AddRange(new ToolStripItem[] { mnuFile, mnuOpenTables, mnuTranscript, mnuAddressBook, mnuTools, mnuHelp });
            MainMenu1.Location = new Point(0, 0);
            MainMenu1.Name = "MainMenu1";
            MainMenu1.Padding = new Padding(8, 4, 0, 4);
            MainMenu1.Size = new Size(1922, 37);
            MainMenu1.TabIndex = 38;
            // 
            // mnuFile
            // 
            mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuConnections, mnuClose });
            mnuFile.Name = "mnuFile";
            mnuFile.Size = new Size(62, 29);
            mnuFile.Text = "Files";
            // 
            // mnuConnections
            // 
            mnuConnections.DropDownItems.AddRange(new ToolStripItem[] { mnuConnectionList, mnuBlankLine3, mnuAddConnection, mnuDeleteConnection });
            mnuConnections.Name = "mnuConnections";
            mnuConnections.Size = new Size(212, 34);
            mnuConnections.Text = "Connections";
            // 
            // mnuConnectionList
            // 
            mnuConnectionList.Name = "mnuConnectionList";
            mnuConnectionList.Size = new Size(303, 34);
            mnuConnectionList.Text = "Recent Connections";
            mnuConnectionList.DropDownItemClicked += mnuDatabaseList_DropDownItemClicked;
            // 
            // mnuBlankLine3
            // 
            mnuBlankLine3.Name = "mnuBlankLine3";
            mnuBlankLine3.Size = new Size(303, 34);
            mnuBlankLine3.Text = "---------------------------";
            // 
            // mnuAddConnection
            // 
            mnuAddConnection.Name = "mnuAddConnection";
            mnuAddConnection.Size = new Size(303, 34);
            mnuAddConnection.Text = "Add Connection";
            mnuAddConnection.Click += mnuAddDatabase_Click;
            // 
            // mnuDeleteConnection
            // 
            mnuDeleteConnection.Name = "mnuDeleteConnection";
            mnuDeleteConnection.Size = new Size(303, 34);
            mnuDeleteConnection.Text = "Delete Connection";
            mnuDeleteConnection.Click += mnuDeleteDatabase_Click;
            // 
            // mnuClose
            // 
            mnuClose.Name = "mnuClose";
            mnuClose.Size = new Size(212, 34);
            mnuClose.Text = "&Close";
            mnuClose.Click += mnuClose_Click;
            // 
            // mnuOpenTables
            // 
            mnuOpenTables.Name = "mnuOpenTables";
            mnuOpenTables.Size = new Size(76, 29);
            mnuOpenTables.Text = "Tables";
            mnuOpenTables.DropDownItemClicked += mnuOpenTables_DropDownItemClicked;
            mnuOpenTables.Click += mnuOpenTables_Click;
            // 
            // mnuTranscript
            // 
            mnuTranscript.DropDownItems.AddRange(new ToolStripItem[] { mnuTranscriptCheckGradRequirements, mnuTranscriptPrint, mnuLocations, mnuLine, mnuPrintTermSummary });
            mnuTranscript.Name = "mnuTranscript";
            mnuTranscript.Size = new Size(103, 29);
            mnuTranscript.Text = "Transcript";
            // 
            // mnuTranscriptCheckGradRequirements
            // 
            mnuTranscriptCheckGradRequirements.Name = "mnuTranscriptCheckGradRequirements";
            mnuTranscriptCheckGradRequirements.Size = new Size(359, 34);
            mnuTranscriptCheckGradRequirements.Text = "Check Grad Requirements";
            mnuTranscriptCheckGradRequirements.Click += mnuTranscriptCheckGradRequirements_Click;
            // 
            // mnuTranscriptPrint
            // 
            mnuTranscriptPrint.DropDownItems.AddRange(new ToolStripItem[] { mnuPrintTranscript, mnuPrintTranscriptEnglish, mnuPrintRole, mnuPrintGrade });
            mnuTranscriptPrint.Name = "mnuTranscriptPrint";
            mnuTranscriptPrint.Size = new Size(359, 34);
            mnuTranscriptPrint.Text = "Print transcript files";
            mnuTranscriptPrint.Click += mnuTranscriptPrint_Click;
            // 
            // mnuPrintTranscript
            // 
            mnuPrintTranscript.Name = "mnuPrintTranscript";
            mnuPrintTranscript.Size = new Size(368, 34);
            mnuPrintTranscript.Text = "Print Student transcript";
            // 
            // mnuPrintTranscriptEnglish
            // 
            mnuPrintTranscriptEnglish.Name = "mnuPrintTranscriptEnglish";
            mnuPrintTranscriptEnglish.Size = new Size(368, 34);
            mnuPrintTranscriptEnglish.Text = "Print Student transcript - English";
            // 
            // mnuPrintRole
            // 
            mnuPrintRole.Name = "mnuPrintRole";
            mnuPrintRole.Size = new Size(368, 34);
            mnuPrintRole.Text = "Print Course role sheet";
            // 
            // mnuPrintGrade
            // 
            mnuPrintGrade.Name = "mnuPrintGrade";
            mnuPrintGrade.Size = new Size(368, 34);
            mnuPrintGrade.Text = "Print Course grade sheet";
            // 
            // mnuLocations
            // 
            mnuLocations.DropDownItems.AddRange(new ToolStripItem[] { mnuTranscriptFolder, mnuTranscriptTemplate, mnuTranscriptTemplateEnglish, mnuRoleTemplate, mnuGradeTemplate });
            mnuLocations.Name = "mnuLocations";
            mnuLocations.Size = new Size(359, 34);
            mnuLocations.Text = "Locations";
            // 
            // mnuTranscriptFolder
            // 
            mnuTranscriptFolder.Name = "mnuTranscriptFolder";
            mnuTranscriptFolder.Size = new Size(402, 34);
            mnuTranscriptFolder.Text = "Transcript folder";
            // 
            // mnuTranscriptTemplate
            // 
            mnuTranscriptTemplate.Name = "mnuTranscriptTemplate";
            mnuTranscriptTemplate.Size = new Size(402, 34);
            mnuTranscriptTemplate.Text = "Student transcript template";
            // 
            // mnuTranscriptTemplateEnglish
            // 
            mnuTranscriptTemplateEnglish.Name = "mnuTranscriptTemplateEnglish";
            mnuTranscriptTemplateEnglish.Size = new Size(402, 34);
            mnuTranscriptTemplateEnglish.Text = "Student transcript template - English";
            // 
            // mnuRoleTemplate
            // 
            mnuRoleTemplate.Name = "mnuRoleTemplate";
            mnuRoleTemplate.Size = new Size(402, 34);
            mnuRoleTemplate.Text = "Course role template";
            // 
            // mnuGradeTemplate
            // 
            mnuGradeTemplate.Name = "mnuGradeTemplate";
            mnuGradeTemplate.Size = new Size(402, 34);
            mnuGradeTemplate.Text = "Course Grade template";
            // 
            // mnuLine
            // 
            mnuLine.Name = "mnuLine";
            mnuLine.Size = new Size(359, 34);
            mnuLine.Text = "-----------------------------------";
            // 
            // mnuPrintTermSummary
            // 
            mnuPrintTermSummary.Name = "mnuPrintTermSummary";
            mnuPrintTermSummary.Size = new Size(359, 34);
            mnuPrintTermSummary.Text = "Print term summary";
            // 
            // mnuAddressBook
            // 
            mnuAddressBook.DropDownItems.AddRange(new ToolStripItem[] { mnuAddressBookLabels, mnuAddressBookPhoneNumbers, mnuAddressBookAddresses, mnuAddressBookEmails, mnuAddressBookGetEmails });
            mnuAddressBook.Name = "mnuAddressBook";
            mnuAddressBook.Size = new Size(139, 29);
            mnuAddressBook.Text = "Address Book";
            // 
            // mnuAddressBookLabels
            // 
            mnuAddressBookLabels.Name = "mnuAddressBookLabels";
            mnuAddressBookLabels.Size = new Size(264, 34);
            mnuAddressBookLabels.Text = "Print mailing labels";
            // 
            // mnuAddressBookPhoneNumbers
            // 
            mnuAddressBookPhoneNumbers.Name = "mnuAddressBookPhoneNumbers";
            mnuAddressBookPhoneNumbers.Size = new Size(264, 34);
            mnuAddressBookPhoneNumbers.Text = "Print phone book";
            // 
            // mnuAddressBookAddresses
            // 
            mnuAddressBookAddresses.Name = "mnuAddressBookAddresses";
            mnuAddressBookAddresses.Size = new Size(264, 34);
            mnuAddressBookAddresses.Text = "Print address book";
            // 
            // mnuAddressBookEmails
            // 
            mnuAddressBookEmails.Name = "mnuAddressBookEmails";
            mnuAddressBookEmails.Size = new Size(264, 34);
            mnuAddressBookEmails.Text = "Print email book";
            // 
            // mnuAddressBookGetEmails
            // 
            mnuAddressBookGetEmails.Name = "mnuAddressBookGetEmails";
            mnuAddressBookGetEmails.Size = new Size(264, 34);
            mnuAddressBookGetEmails.Text = "Get email list";
            // 
            // mnuTools
            // 
            mnuTools.DropDownItems.AddRange(new ToolStripItem[] { mnuDatabaseInfo, mnuDuplicateDisplayKeys, mnuForeignKeyMissing, mnuPrintCurrentTable, mnuViewLog });
            mnuTools.Name = "mnuTools";
            mnuTools.Size = new Size(69, 29);
            mnuTools.Text = "Tools";
            // 
            // mnuDatabaseInfo
            // 
            mnuDatabaseInfo.Name = "mnuDatabaseInfo";
            mnuDatabaseInfo.Size = new Size(364, 34);
            mnuDatabaseInfo.Text = "Database Information";
            mnuDatabaseInfo.Click += mnuToolsDatabaseInformation_Click;
            // 
            // mnuDuplicateDisplayKeys
            // 
            mnuDuplicateDisplayKeys.Name = "mnuDuplicateDisplayKeys";
            mnuDuplicateDisplayKeys.Size = new Size(364, 34);
            mnuDuplicateDisplayKeys.Text = "Update : Duplicate Display Keys";
            mnuDuplicateDisplayKeys.Click += mnuToolDuplicateDisplayKeys_Click;
            // 
            // mnuForeignKeyMissing
            // 
            mnuForeignKeyMissing.Name = "mnuForeignKeyMissing";
            mnuForeignKeyMissing.Size = new Size(364, 34);
            mnuForeignKeyMissing.Text = "Update : Check for Foreign Key";
            mnuForeignKeyMissing.Click += mnuForeignKeyMissing_Click;
            // 
            // mnuPrintCurrentTable
            // 
            mnuPrintCurrentTable.Name = "mnuPrintCurrentTable";
            mnuPrintCurrentTable.Size = new Size(364, 34);
            mnuPrintCurrentTable.Text = "Print Selection";
            // 
            // mnuViewLog
            // 
            mnuViewLog.Name = "mnuViewLog";
            mnuViewLog.Size = new Size(364, 34);
            mnuViewLog.Text = "View Log";
            // 
            // mnuHelp
            // 
            mnuHelp.DropDownItems.AddRange(new ToolStripItem[] { mnuHelpFile });
            mnuHelp.Name = "mnuHelp";
            mnuHelp.Size = new Size(65, 29);
            mnuHelp.Text = "Help";
            // 
            // mnuHelpFile
            // 
            mnuHelpFile.Name = "mnuHelpFile";
            mnuHelpFile.Size = new Size(179, 34);
            mnuHelpFile.Text = "Help file";
            // 
            // toolStripBottom
            // 
            toolStripBottom.AutoSize = false;
            toolStripBottom.Dock = DockStyle.Bottom;
            toolStripBottom.ImageScalingSize = new Size(20, 20);
            toolStripBottom.Items.AddRange(new ToolStripItem[] { toolStripMsg, txtRecordsPerPage, toolStripButton5, toolStripButton4, toolStripButton3, toolStripButton2, toolStripButton1, toolStripButtonColumnWidth });
            toolStripBottom.Location = new Point(0, 960);
            toolStripBottom.Name = "toolStripBottom";
            toolStripBottom.Size = new Size(1922, 34);
            toolStripBottom.TabIndex = 75;
            toolStripBottom.Text = "toolStrip1";
            // 
            // toolStripMsg
            // 
            toolStripMsg.ForeColor = Color.Red;
            toolStripMsg.Name = "toolStripMsg";
            toolStripMsg.Size = new Size(90, 29);
            toolStripMsg.Text = "Messages";
            // 
            // txtRecordsPerPage
            // 
            txtRecordsPerPage.Alignment = ToolStripItemAlignment.Right;
            txtRecordsPerPage.Name = "txtRecordsPerPage";
            txtRecordsPerPage.Size = new Size(62, 34);
            txtRecordsPerPage.Text = "200";
            txtRecordsPerPage.TextBoxTextAlign = HorizontalAlignment.Center;
            txtRecordsPerPage.ToolTipText = "Records Per Page";
            txtRecordsPerPage.Leave += txtRecordsPerPage_Leave;
            // 
            // toolStripButton5
            // 
            toolStripButton5.Alignment = ToolStripItemAlignment.Right;
            toolStripButton5.AutoToolTip = false;
            toolStripButton5.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton5.Image = Properties.Resources.end_arrow_32;
            toolStripButton5.ImageTransparentColor = Color.Magenta;
            toolStripButton5.Name = "toolStripButton5";
            toolStripButton5.Size = new Size(34, 29);
            toolStripButton5.Text = "toolStripButton5";
            toolStripButton5.ToolTipText = "+3 pages";
            toolStripButton5.Click += toolStripButton5_Click;
            // 
            // toolStripButton4
            // 
            toolStripButton4.Alignment = ToolStripItemAlignment.Right;
            toolStripButton4.AutoToolTip = false;
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton4.Image = Properties.Resources.iconmonstr_arrow_32_OneRight;
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(34, 29);
            toolStripButton4.Text = "toolStripButton4";
            toolStripButton4.ToolTipText = "Next";
            toolStripButton4.Click += toolStripButton4_Click;
            // 
            // toolStripButton3
            // 
            toolStripButton3.Alignment = ToolStripItemAlignment.Right;
            toolStripButton3.AutoToolTip = false;
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(70, 29);
            toolStripButton3.Text = "Page #";
            toolStripButton3.TextImageRelation = TextImageRelation.Overlay;
            toolStripButton3.ToolTipText = "Select Page";
            toolStripButton3.Click += toolStripButton3_Click;
            // 
            // toolStripButton2
            // 
            toolStripButton2.Alignment = ToolStripItemAlignment.Right;
            toolStripButton2.AutoToolTip = false;
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton2.Image = Properties.Resources.iconmonstr_arrow_32_OneLEFT;
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(34, 29);
            toolStripButton2.Text = "toolStripButton2";
            toolStripButton2.ToolTipText = "Previous";
            toolStripButton2.Click += toolStripButton2_Click;
            // 
            // toolStripButton1
            // 
            toolStripButton1.Alignment = ToolStripItemAlignment.Right;
            toolStripButton1.AutoToolTip = false;
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton1.Image = Properties.Resources.iconmonstr_arrow_32_LEFT;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(34, 29);
            toolStripButton1.Text = "toolStripButton1";
            toolStripButton1.ToolTipText = "-3 pages";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // toolStripButtonColumnWidth
            // 
            toolStripButtonColumnWidth.Alignment = ToolStripItemAlignment.Right;
            toolStripButtonColumnWidth.AutoSize = false;
            toolStripButtonColumnWidth.AutoToolTip = false;
            toolStripButtonColumnWidth.BackColor = Color.Silver;
            toolStripButtonColumnWidth.CheckOnClick = true;
            toolStripButtonColumnWidth.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonColumnWidth.DoubleClickEnabled = true;
            toolStripButtonColumnWidth.Image = (Image)resources.GetObject("toolStripButtonColumnWidth.Image");
            toolStripButtonColumnWidth.ImageTransparentColor = Color.Magenta;
            toolStripButtonColumnWidth.Margin = new Padding(10, 0, 10, 0);
            toolStripButtonColumnWidth.Name = "toolStripButtonColumnWidth";
            toolStripButtonColumnWidth.Size = new Size(162, 24);
            toolStripButtonColumnWidth.Text = "Narrow";
            toolStripButtonColumnWidth.Click += toolStripColumnWidth_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(0, 38);
            splitContainer1.Margin = new Padding(4);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel);
            splitContainer1.Panel1.Controls.Add(txtMessages);
            splitContainer1.Panel1.Margin = new Padding(0, 100, 0, 100);
            splitContainer1.Panel1MinSize = 3;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(dataGridView1);
            splitContainer1.Panel2MinSize = 3;
            splitContainer1.Size = new Size(2220, 922);
            splitContainer1.SplitterDistance = 512;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 77;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 31;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 61F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel.Controls.Add(lblMainFilter, 1, 0);
            tableLayoutPanel.Controls.Add(cmbMainFilter, 4, 0);
            tableLayoutPanel.Controls.Add(lblGridFilter, 1, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_0, 4, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_1, 11, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_3, 8, 2);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_4, 11, 2);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_4, 15, 2);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_5, 18, 2);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_1, 15, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_5, 22, 2);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_2, 22, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_2, 18, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_0, 8, 1);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_3, 4, 2);
            tableLayoutPanel.Controls.Add(rbMerge, 21, 0);
            tableLayoutPanel.Controls.Add(btnDeleteAddMerge, 19, 0);
            tableLayoutPanel.Controls.Add(rbAdd, 17, 0);
            tableLayoutPanel.Controls.Add(rbDelete, 15, 0);
            tableLayoutPanel.Controls.Add(rbEdit, 13, 0);
            tableLayoutPanel.Controls.Add(rbView, 11, 0);
            tableLayoutPanel.Controls.Add(lblComboFilter, 1, 6);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_7, 11, 3);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_6, 8, 3);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_6, 4, 3);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_7, 15, 3);
            tableLayoutPanel.Controls.Add(cmbGridFilterFields_8, 18, 3);
            tableLayoutPanel.Controls.Add(cmbGridFilterValue_8, 22, 3);
            tableLayoutPanel.Controls.Add(lblCmbFilterField_3, 8, 7);
            tableLayoutPanel.Controls.Add(cmbComboFilterValue_3, 11, 7);
            tableLayoutPanel.Controls.Add(lblCmbFilterField_4, 14, 7);
            tableLayoutPanel.Controls.Add(cmbComboFilterValue_4, 17, 7);
            tableLayoutPanel.Controls.Add(lblCmbFilterField_5, 20, 7);
            tableLayoutPanel.Controls.Add(cmbComboFilterValue_5, 23, 7);
            tableLayoutPanel.Controls.Add(lblCmbFilterField_0, 8, 6);
            tableLayoutPanel.Controls.Add(cmbComboFilterValue_0, 11, 6);
            tableLayoutPanel.Controls.Add(lblCmbFilterField_1, 14, 6);
            tableLayoutPanel.Controls.Add(cmbComboFilterValue_1, 17, 6);
            tableLayoutPanel.Controls.Add(lblCmbFilterField_2, 20, 6);
            tableLayoutPanel.Controls.Add(cmbComboFilterValue_2, 23, 6);
            tableLayoutPanel.Controls.Add(cmbComboTableList, 4, 6);
            tableLayoutPanel.Controls.Add(button1, 4, 5);
            tableLayoutPanel.Controls.Add(btnReload, 24, 0);
            tableLayoutPanel.Location = new Point(0, 34);
            tableLayoutPanel.Margin = new Padding(4);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 8;
            tableLayoutPanel.RowStyles.Add(new RowStyle());
            tableLayoutPanel.RowStyles.Add(new RowStyle());
            tableLayoutPanel.RowStyles.Add(new RowStyle());
            tableLayoutPanel.RowStyles.Add(new RowStyle());
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));
            tableLayoutPanel.RowStyles.Add(new RowStyle());
            tableLayoutPanel.RowStyles.Add(new RowStyle());
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel.Size = new Size(1911, 356);
            tableLayoutPanel.TabIndex = 108;
            // 
            // cmbMainFilter
            // 
            cmbMainFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbMainFilter.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbMainFilter.AutoCompleteSource = AutoCompleteSource.ListItems;
            tableLayoutPanel.SetColumnSpan(cmbMainFilter, 7);
            cmbMainFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMainFilter.ForeColor = SystemColors.WindowText;
            cmbMainFilter.Location = new Point(195, 4);
            cmbMainFilter.Margin = new Padding(4);
            cmbMainFilter.Name = "cmbMainFilter";
            cmbMainFilter.Size = new Size(426, 33);
            cmbMainFilter.TabIndex = 110;
            cmbMainFilter.DropDown += AdjustWidthComboBox_DropDown;
            cmbMainFilter.SelectedIndexChanged += cmbMainFilter_SelectedIndexChanged;
            // 
            // lblGridFilter
            // 
            lblGridFilter.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(lblGridFilter, 3);
            lblGridFilter.Location = new Point(9, 56);
            lblGridFilter.Margin = new Padding(4);
            lblGridFilter.Name = "lblGridFilter";
            lblGridFilter.Size = new Size(178, 41);
            lblGridFilter.TabIndex = 135;
            lblGridFilter.Text = "lblGridFilter";
            lblGridFilter.TextAlign = ContentAlignment.TopRight;
            // 
            // cmbGridFilterFields_0
            // 
            cmbGridFilterFields_0.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbGridFilterFields_0.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_0.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_0, 4);
            cmbGridFilterFields_0.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_0.ForeColor = Color.Black;
            cmbGridFilterFields_0.FormattingEnabled = true;
            cmbGridFilterFields_0.Location = new Point(195, 56);
            cmbGridFilterFields_0.Margin = new Padding(4);
            cmbGridFilterFields_0.Name = "cmbGridFilterFields_0";
            cmbGridFilterFields_0.Size = new Size(240, 33);
            cmbGridFilterFields_0.TabIndex = 129;
            cmbGridFilterFields_0.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterFields_0.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterFields_1
            // 
            cmbGridFilterFields_1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbGridFilterFields_1.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_1.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_1, 4);
            cmbGridFilterFields_1.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_1.ForeColor = Color.Black;
            cmbGridFilterFields_1.FormattingEnabled = true;
            cmbGridFilterFields_1.Location = new Point(629, 56);
            cmbGridFilterFields_1.Margin = new Padding(4);
            cmbGridFilterFields_1.Name = "cmbGridFilterFields_1";
            cmbGridFilterFields_1.Size = new Size(240, 33);
            cmbGridFilterFields_1.TabIndex = 131;
            cmbGridFilterFields_1.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterFields_1.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_3
            // 
            cmbGridFilterValue_3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_3, 3);
            cmbGridFilterValue_3.FormattingEnabled = true;
            cmbGridFilterValue_3.Location = new Point(443, 105);
            cmbGridFilterValue_3.Margin = new Padding(4);
            cmbGridFilterValue_3.Name = "cmbGridFilterValue_3";
            cmbGridFilterValue_3.Size = new Size(178, 33);
            cmbGridFilterValue_3.TabIndex = 156;
            cmbGridFilterValue_3.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_3.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_3.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_3.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterFields_4
            // 
            cmbGridFilterFields_4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbGridFilterFields_4.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_4.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_4, 4);
            cmbGridFilterFields_4.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_4.ForeColor = Color.Black;
            cmbGridFilterFields_4.FormattingEnabled = true;
            cmbGridFilterFields_4.Location = new Point(629, 105);
            cmbGridFilterFields_4.Margin = new Padding(4);
            cmbGridFilterFields_4.Name = "cmbGridFilterFields_4";
            cmbGridFilterFields_4.Size = new Size(240, 33);
            cmbGridFilterFields_4.TabIndex = 158;
            cmbGridFilterFields_4.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterFields_4.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_4
            // 
            cmbGridFilterValue_4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_4, 3);
            cmbGridFilterValue_4.FormattingEnabled = true;
            cmbGridFilterValue_4.Location = new Point(877, 105);
            cmbGridFilterValue_4.Margin = new Padding(4);
            cmbGridFilterValue_4.Name = "cmbGridFilterValue_4";
            cmbGridFilterValue_4.Size = new Size(178, 33);
            cmbGridFilterValue_4.TabIndex = 157;
            cmbGridFilterValue_4.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_4.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_4.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_4.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterFields_5
            // 
            cmbGridFilterFields_5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbGridFilterFields_5.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_5.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_5, 4);
            cmbGridFilterFields_5.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_5.ForeColor = Color.Black;
            cmbGridFilterFields_5.FormattingEnabled = true;
            cmbGridFilterFields_5.Location = new Point(1063, 105);
            cmbGridFilterFields_5.Margin = new Padding(4);
            cmbGridFilterFields_5.Name = "cmbGridFilterFields_5";
            cmbGridFilterFields_5.Size = new Size(240, 33);
            cmbGridFilterFields_5.TabIndex = 159;
            cmbGridFilterFields_5.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterFields_5.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_1
            // 
            cmbGridFilterValue_1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_1, 3);
            cmbGridFilterValue_1.FormattingEnabled = true;
            cmbGridFilterValue_1.Location = new Point(877, 56);
            cmbGridFilterValue_1.Margin = new Padding(4);
            cmbGridFilterValue_1.Name = "cmbGridFilterValue_1";
            cmbGridFilterValue_1.Size = new Size(178, 33);
            cmbGridFilterValue_1.TabIndex = 155;
            cmbGridFilterValue_1.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_1.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_1.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_1.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterValue_5
            // 
            cmbGridFilterValue_5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_5, 3);
            cmbGridFilterValue_5.FormattingEnabled = true;
            cmbGridFilterValue_5.Location = new Point(1311, 105);
            cmbGridFilterValue_5.Margin = new Padding(4);
            cmbGridFilterValue_5.Name = "cmbGridFilterValue_5";
            cmbGridFilterValue_5.Size = new Size(179, 33);
            cmbGridFilterValue_5.TabIndex = 162;
            cmbGridFilterValue_5.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_5.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_5.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_5.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterValue_2
            // 
            cmbGridFilterValue_2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_2, 3);
            cmbGridFilterValue_2.FormattingEnabled = true;
            cmbGridFilterValue_2.Location = new Point(1311, 56);
            cmbGridFilterValue_2.Margin = new Padding(4);
            cmbGridFilterValue_2.Name = "cmbGridFilterValue_2";
            cmbGridFilterValue_2.Size = new Size(179, 33);
            cmbGridFilterValue_2.TabIndex = 163;
            cmbGridFilterValue_2.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_2.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_2.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_2.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterFields_2
            // 
            cmbGridFilterFields_2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbGridFilterFields_2.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_2.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_2, 4);
            cmbGridFilterFields_2.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_2.ForeColor = Color.Black;
            cmbGridFilterFields_2.FormattingEnabled = true;
            cmbGridFilterFields_2.Location = new Point(1063, 56);
            cmbGridFilterFields_2.Margin = new Padding(4);
            cmbGridFilterFields_2.Name = "cmbGridFilterFields_2";
            cmbGridFilterFields_2.Size = new Size(240, 33);
            cmbGridFilterFields_2.TabIndex = 133;
            cmbGridFilterFields_2.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterFields_2.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_0
            // 
            cmbGridFilterValue_0.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_0, 3);
            cmbGridFilterValue_0.FormattingEnabled = true;
            cmbGridFilterValue_0.Location = new Point(443, 56);
            cmbGridFilterValue_0.Margin = new Padding(4);
            cmbGridFilterValue_0.Name = "cmbGridFilterValue_0";
            cmbGridFilterValue_0.Size = new Size(178, 33);
            cmbGridFilterValue_0.TabIndex = 164;
            cmbGridFilterValue_0.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_0.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_0.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_0.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterFields_3
            // 
            cmbGridFilterFields_3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbGridFilterFields_3.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_3.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_3, 4);
            cmbGridFilterFields_3.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_3.ForeColor = Color.Black;
            cmbGridFilterFields_3.FormattingEnabled = true;
            cmbGridFilterFields_3.Location = new Point(195, 105);
            cmbGridFilterFields_3.Margin = new Padding(4);
            cmbGridFilterFields_3.Name = "cmbGridFilterFields_3";
            cmbGridFilterFields_3.Size = new Size(240, 33);
            cmbGridFilterFields_3.TabIndex = 165;
            cmbGridFilterFields_3.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterFields_3.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // rbMerge
            // 
            rbMerge.Anchor = AnchorStyles.Top;
            rbMerge.AutoSize = true;
            tableLayoutPanel.SetColumnSpan(rbMerge, 2);
            rbMerge.Location = new Point(1263, 2);
            rbMerge.Margin = new Padding(2);
            rbMerge.Name = "rbMerge";
            rbMerge.Size = new Size(88, 29);
            rbMerge.TabIndex = 121;
            rbMerge.Text = "Merge";
            rbMerge.UseVisualStyleBackColor = true;
            rbMerge.CheckedChanged += rbMerge_CheckedChanged;
            // 
            // btnDeleteAddMerge
            // 
            btnDeleteAddMerge.Anchor = AnchorStyles.Top;
            btnDeleteAddMerge.AutoSize = true;
            tableLayoutPanel.SetColumnSpan(btnDeleteAddMerge, 2);
            btnDeleteAddMerge.ContextMenuStrip = GridContextMenu;
            btnDeleteAddMerge.Enabled = false;
            btnDeleteAddMerge.ForeColor = Color.Black;
            btnDeleteAddMerge.Location = new Point(1125, 4);
            btnDeleteAddMerge.Margin = new Padding(4);
            btnDeleteAddMerge.Name = "btnDeleteAddMerge";
            btnDeleteAddMerge.Size = new Size(116, 44);
            btnDeleteAddMerge.TabIndex = 127;
            btnDeleteAddMerge.Text = "Merge rows";
            btnDeleteAddMerge.UseVisualStyleBackColor = true;
            btnDeleteAddMerge.Click += btnDeleteAddMerge_Click;
            // 
            // GridContextMenu
            // 
            GridContextMenu.ImageScalingSize = new Size(20, 20);
            GridContextMenu.Items.AddRange(new ToolStripItem[] { GridContextMenu_FindInDescendent, GridContextMenu_FindInAncestor, GridContextMenu_TimesUsedAsFK, deleteUnusedForeignKeysToolStripMenuItem, correctDuplicateDKsToolStripMenuItem });
            GridContextMenu.Name = "contextMenuStrip1";
            GridContextMenu.Size = new Size(290, 164);
            // 
            // GridContextMenu_FindInDescendent
            // 
            GridContextMenu_FindInDescendent.Name = "GridContextMenu_FindInDescendent";
            GridContextMenu_FindInDescendent.Size = new Size(289, 32);
            GridContextMenu_FindInDescendent.Text = "Find in Descendent table";
            GridContextMenu_FindInDescendent.Click += GridContextMenu_FindInDescendent_Click;
            // 
            // GridContextMenu_FindInAncestor
            // 
            GridContextMenu_FindInAncestor.DoubleClickEnabled = true;
            GridContextMenu_FindInAncestor.Name = "GridContextMenu_FindInAncestor";
            GridContextMenu_FindInAncestor.Size = new Size(289, 32);
            GridContextMenu_FindInAncestor.Text = "Find in Ancestor table";
            GridContextMenu_FindInAncestor.Click += GridContextMenu_FindInAncestor_Click;
            // 
            // GridContextMenu_TimesUsedAsFK
            // 
            GridContextMenu_TimesUsedAsFK.Name = "GridContextMenu_TimesUsedAsFK";
            GridContextMenu_TimesUsedAsFK.Size = new Size(289, 32);
            GridContextMenu_TimesUsedAsFK.Text = "Count FK uses";
            GridContextMenu_TimesUsedAsFK.Click += GridContextMenu_TimesUsedAsFK_Click;
            // 
            // deleteUnusedForeignKeysToolStripMenuItem
            // 
            deleteUnusedForeignKeysToolStripMenuItem.Name = "deleteUnusedForeignKeysToolStripMenuItem";
            deleteUnusedForeignKeysToolStripMenuItem.Size = new Size(289, 32);
            deleteUnusedForeignKeysToolStripMenuItem.Text = "Find &Unused Foreign Keys";
            deleteUnusedForeignKeysToolStripMenuItem.Click += GridContextMenu_FindUnusedFK_Click;
            // 
            // correctDuplicateDKsToolStripMenuItem
            // 
            correctDuplicateDKsToolStripMenuItem.Name = "correctDuplicateDKsToolStripMenuItem";
            correctDuplicateDKsToolStripMenuItem.Size = new Size(289, 32);
            correctDuplicateDKsToolStripMenuItem.Text = "Correct Duplicate DK's";
            correctDuplicateDKsToolStripMenuItem.Click += mnuToolDuplicateDisplayKeys_Click;
            // 
            // rbAdd
            // 
            rbAdd.Anchor = AnchorStyles.Top;
            rbAdd.AutoSize = true;
            tableLayoutPanel.SetColumnSpan(rbAdd, 2);
            rbAdd.Location = new Point(1023, 2);
            rbAdd.Margin = new Padding(2);
            rbAdd.Name = "rbAdd";
            rbAdd.Size = new Size(71, 29);
            rbAdd.TabIndex = 119;
            rbAdd.Text = "Add";
            rbAdd.UseVisualStyleBackColor = true;
            rbAdd.CheckedChanged += rbAdd_CheckedChanged;
            // 
            // rbDelete
            // 
            rbDelete.Anchor = AnchorStyles.Top;
            rbDelete.AutoSize = true;
            tableLayoutPanel.SetColumnSpan(rbDelete, 2);
            rbDelete.Location = new Point(891, 2);
            rbDelete.Margin = new Padding(2);
            rbDelete.Name = "rbDelete";
            rbDelete.Size = new Size(87, 29);
            rbDelete.TabIndex = 120;
            rbDelete.Text = "Delete";
            rbDelete.UseVisualStyleBackColor = true;
            rbDelete.CheckedChanged += rbDelete_CheckedChanged;
            // 
            // rbEdit
            // 
            rbEdit.Anchor = AnchorStyles.Top;
            rbEdit.AutoSize = true;
            tableLayoutPanel.SetColumnSpan(rbEdit, 2);
            rbEdit.Location = new Point(777, 2);
            rbEdit.Margin = new Padding(2);
            rbEdit.Name = "rbEdit";
            rbEdit.Size = new Size(67, 29);
            rbEdit.TabIndex = 118;
            rbEdit.Text = "Edit";
            rbEdit.UseVisualStyleBackColor = true;
            rbEdit.CheckedChanged += rbEdit_CheckedChanged;
            // 
            // rbView
            // 
            rbView.Anchor = AnchorStyles.Top;
            rbView.AutoSize = true;
            tableLayoutPanel.SetColumnSpan(rbView, 2);
            rbView.Location = new Point(650, 2);
            rbView.Margin = new Padding(2);
            rbView.Name = "rbView";
            rbView.Size = new Size(74, 29);
            rbView.TabIndex = 117;
            rbView.Text = "View";
            rbView.UseVisualStyleBackColor = true;
            rbView.CheckedChanged += rbView_CheckedChanged;
            // 
            // lblComboFilter
            // 
            lblComboFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(lblComboFilter, 3);
            lblComboFilter.Location = new Point(9, 219);
            lblComboFilter.Margin = new Padding(4);
            lblComboFilter.Name = "lblComboFilter";
            lblComboFilter.Size = new Size(178, 35);
            lblComboFilter.TabIndex = 153;
            lblComboFilter.Text = "lblComboFilter";
            lblComboFilter.TextAlign = ContentAlignment.TopRight;
            // 
            // cmbGridFilterFields_7
            // 
            cmbGridFilterFields_7.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbGridFilterFields_7.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_7.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_7, 4);
            cmbGridFilterFields_7.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_7.ForeColor = Color.Black;
            cmbGridFilterFields_7.FormattingEnabled = true;
            cmbGridFilterFields_7.Location = new Point(629, 146);
            cmbGridFilterFields_7.Margin = new Padding(4);
            cmbGridFilterFields_7.Name = "cmbGridFilterFields_7";
            cmbGridFilterFields_7.Size = new Size(240, 33);
            cmbGridFilterFields_7.TabIndex = 184;
            cmbGridFilterFields_7.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_6
            // 
            cmbGridFilterValue_6.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_6, 3);
            cmbGridFilterValue_6.FormattingEnabled = true;
            cmbGridFilterValue_6.Location = new Point(443, 146);
            cmbGridFilterValue_6.Margin = new Padding(4);
            cmbGridFilterValue_6.Name = "cmbGridFilterValue_6";
            cmbGridFilterValue_6.Size = new Size(178, 33);
            cmbGridFilterValue_6.TabIndex = 186;
            cmbGridFilterValue_6.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_6.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_6.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_6.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterFields_6
            // 
            cmbGridFilterFields_6.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbGridFilterFields_6.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_6.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_6, 4);
            cmbGridFilterFields_6.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_6.ForeColor = Color.Black;
            cmbGridFilterFields_6.FormattingEnabled = true;
            cmbGridFilterFields_6.Location = new Point(195, 146);
            cmbGridFilterFields_6.Margin = new Padding(4);
            cmbGridFilterFields_6.Name = "cmbGridFilterFields_6";
            cmbGridFilterFields_6.Size = new Size(240, 33);
            cmbGridFilterFields_6.TabIndex = 183;
            cmbGridFilterFields_6.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_7
            // 
            cmbGridFilterValue_7.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_7, 3);
            cmbGridFilterValue_7.FormattingEnabled = true;
            cmbGridFilterValue_7.Location = new Point(877, 146);
            cmbGridFilterValue_7.Margin = new Padding(4);
            cmbGridFilterValue_7.Name = "cmbGridFilterValue_7";
            cmbGridFilterValue_7.Size = new Size(178, 33);
            cmbGridFilterValue_7.TabIndex = 187;
            cmbGridFilterValue_7.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_7.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_7.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_7.Enter += cmbGridFilterValue_Enter;
            // 
            // cmbGridFilterFields_8
            // 
            cmbGridFilterFields_8.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbGridFilterFields_8.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbGridFilterFields_8.BackColor = Color.White;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterFields_8, 4);
            cmbGridFilterFields_8.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGridFilterFields_8.ForeColor = Color.Black;
            cmbGridFilterFields_8.FormattingEnabled = true;
            cmbGridFilterFields_8.Location = new Point(1063, 146);
            cmbGridFilterFields_8.Margin = new Padding(4);
            cmbGridFilterFields_8.Name = "cmbGridFilterFields_8";
            cmbGridFilterFields_8.Size = new Size(240, 33);
            cmbGridFilterFields_8.TabIndex = 185;
            cmbGridFilterFields_8.SelectedIndexChanged += cmbGridFilterFields_SelectedIndexChanged;
            // 
            // cmbGridFilterValue_8
            // 
            cmbGridFilterValue_8.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbGridFilterValue_8, 3);
            cmbGridFilterValue_8.FormattingEnabled = true;
            cmbGridFilterValue_8.Location = new Point(1311, 146);
            cmbGridFilterValue_8.Margin = new Padding(4);
            cmbGridFilterValue_8.Name = "cmbGridFilterValue_8";
            cmbGridFilterValue_8.Size = new Size(179, 33);
            cmbGridFilterValue_8.TabIndex = 188;
            cmbGridFilterValue_8.DropDown += AdjustWidthComboBox_DropDown;
            cmbGridFilterValue_8.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbGridFilterValue_8.TextChanged += cmbGridFilterValue_TextChanged;
            cmbGridFilterValue_8.Enter += cmbGridFilterValue_Enter;
            // 
            // lblCmbFilterField_3
            // 
            lblCmbFilterField_3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(lblCmbFilterField_3, 3);
            lblCmbFilterField_3.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblCmbFilterField_3.Location = new Point(441, 258);
            lblCmbFilterField_3.Margin = new Padding(2, 0, 2, 0);
            lblCmbFilterField_3.Name = "lblCmbFilterField_3";
            lblCmbFilterField_3.Size = new Size(182, 30);
            lblCmbFilterField_3.TabIndex = 174;
            lblCmbFilterField_3.Text = "lblCmbFilterField_3";
            lblCmbFilterField_3.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbComboFilterValue_3
            // 
            cmbComboFilterValue_3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbComboFilterValue_3, 3);
            cmbComboFilterValue_3.FormattingEnabled = true;
            cmbComboFilterValue_3.Location = new Point(627, 260);
            cmbComboFilterValue_3.Margin = new Padding(2);
            cmbComboFilterValue_3.Name = "cmbComboFilterValue_3";
            cmbComboFilterValue_3.Size = new Size(182, 33);
            cmbComboFilterValue_3.TabIndex = 180;
            cmbComboFilterValue_3.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboFilterValue_3.TextChanged += cmbComboFilterValue_TextChanged;
            cmbComboFilterValue_3.Enter += cmbComboFilterValue_Enter;
            cmbComboFilterValue_3.Leave += cmbComboFilterValue_Leave;
            cmbComboFilterValue_3.MouseLeave += cmbComboFilterValue_Leave;
            // 
            // lblCmbFilterField_4
            // 
            lblCmbFilterField_4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(lblCmbFilterField_4, 3);
            lblCmbFilterField_4.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblCmbFilterField_4.Location = new Point(813, 258);
            lblCmbFilterField_4.Margin = new Padding(2, 0, 2, 0);
            lblCmbFilterField_4.Name = "lblCmbFilterField_4";
            lblCmbFilterField_4.Size = new Size(182, 30);
            lblCmbFilterField_4.TabIndex = 175;
            lblCmbFilterField_4.Text = "lblCmbFilterField_4";
            lblCmbFilterField_4.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbComboFilterValue_4
            // 
            cmbComboFilterValue_4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbComboFilterValue_4, 3);
            cmbComboFilterValue_4.FormattingEnabled = true;
            cmbComboFilterValue_4.Location = new Point(999, 260);
            cmbComboFilterValue_4.Margin = new Padding(2);
            cmbComboFilterValue_4.Name = "cmbComboFilterValue_4";
            cmbComboFilterValue_4.Size = new Size(182, 33);
            cmbComboFilterValue_4.TabIndex = 181;
            cmbComboFilterValue_4.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboFilterValue_4.TextChanged += cmbComboFilterValue_TextChanged;
            cmbComboFilterValue_4.Enter += cmbComboFilterValue_Enter;
            cmbComboFilterValue_4.Leave += cmbComboFilterValue_Leave;
            cmbComboFilterValue_4.MouseLeave += cmbComboFilterValue_Leave;
            // 
            // lblCmbFilterField_5
            // 
            lblCmbFilterField_5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(lblCmbFilterField_5, 3);
            lblCmbFilterField_5.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblCmbFilterField_5.Location = new Point(1185, 258);
            lblCmbFilterField_5.Margin = new Padding(2, 0, 2, 0);
            lblCmbFilterField_5.Name = "lblCmbFilterField_5";
            lblCmbFilterField_5.Size = new Size(182, 30);
            lblCmbFilterField_5.TabIndex = 176;
            lblCmbFilterField_5.Text = "lblCmbFilterField_5";
            lblCmbFilterField_5.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbComboFilterValue_5
            // 
            cmbComboFilterValue_5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbComboFilterValue_5, 3);
            cmbComboFilterValue_5.FormattingEnabled = true;
            cmbComboFilterValue_5.Location = new Point(1371, 260);
            cmbComboFilterValue_5.Margin = new Padding(2);
            cmbComboFilterValue_5.Name = "cmbComboFilterValue_5";
            cmbComboFilterValue_5.Size = new Size(183, 33);
            cmbComboFilterValue_5.TabIndex = 182;
            cmbComboFilterValue_5.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboFilterValue_5.SelectedIndexChanged += cmbGridFilterValue_SelectedIndexChanged;
            cmbComboFilterValue_5.TextChanged += cmbComboFilterValue_TextChanged;
            cmbComboFilterValue_5.Enter += cmbComboFilterValue_Enter;
            cmbComboFilterValue_5.Leave += cmbComboFilterValue_Leave;
            cmbComboFilterValue_5.MouseLeave += cmbComboFilterValue_Leave;
            // 
            // lblCmbFilterField_0
            // 
            lblCmbFilterField_0.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(lblCmbFilterField_0, 3);
            lblCmbFilterField_0.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblCmbFilterField_0.Location = new Point(441, 215);
            lblCmbFilterField_0.Margin = new Padding(2, 0, 2, 0);
            lblCmbFilterField_0.Name = "lblCmbFilterField_0";
            lblCmbFilterField_0.Size = new Size(182, 30);
            lblCmbFilterField_0.TabIndex = 168;
            lblCmbFilterField_0.Text = "lblCmbFilterField_0";
            lblCmbFilterField_0.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbComboFilterValue_0
            // 
            cmbComboFilterValue_0.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbComboFilterValue_0, 3);
            cmbComboFilterValue_0.FormattingEnabled = true;
            cmbComboFilterValue_0.Location = new Point(627, 217);
            cmbComboFilterValue_0.Margin = new Padding(2);
            cmbComboFilterValue_0.Name = "cmbComboFilterValue_0";
            cmbComboFilterValue_0.Size = new Size(182, 33);
            cmbComboFilterValue_0.TabIndex = 177;
            cmbComboFilterValue_0.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboFilterValue_0.TextChanged += cmbComboFilterValue_TextChanged;
            cmbComboFilterValue_0.Enter += cmbComboFilterValue_Enter;
            cmbComboFilterValue_0.Leave += cmbComboFilterValue_Leave;
            cmbComboFilterValue_0.MouseLeave += cmbComboFilterValue_Leave;
            // 
            // lblCmbFilterField_1
            // 
            lblCmbFilterField_1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(lblCmbFilterField_1, 3);
            lblCmbFilterField_1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblCmbFilterField_1.Location = new Point(813, 215);
            lblCmbFilterField_1.Margin = new Padding(2, 0, 2, 0);
            lblCmbFilterField_1.Name = "lblCmbFilterField_1";
            lblCmbFilterField_1.Size = new Size(182, 30);
            lblCmbFilterField_1.TabIndex = 170;
            lblCmbFilterField_1.Text = "lblCmbFilterField_1";
            lblCmbFilterField_1.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbComboFilterValue_1
            // 
            cmbComboFilterValue_1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbComboFilterValue_1, 3);
            cmbComboFilterValue_1.FormattingEnabled = true;
            cmbComboFilterValue_1.Location = new Point(999, 217);
            cmbComboFilterValue_1.Margin = new Padding(2);
            cmbComboFilterValue_1.Name = "cmbComboFilterValue_1";
            cmbComboFilterValue_1.Size = new Size(182, 33);
            cmbComboFilterValue_1.TabIndex = 178;
            cmbComboFilterValue_1.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboFilterValue_1.TextChanged += cmbComboFilterValue_TextChanged;
            cmbComboFilterValue_1.Enter += cmbComboFilterValue_Enter;
            cmbComboFilterValue_1.Leave += cmbComboFilterValue_Leave;
            cmbComboFilterValue_1.MouseLeave += cmbComboFilterValue_Leave;
            // 
            // lblCmbFilterField_2
            // 
            lblCmbFilterField_2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(lblCmbFilterField_2, 3);
            lblCmbFilterField_2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblCmbFilterField_2.Location = new Point(1185, 215);
            lblCmbFilterField_2.Margin = new Padding(2, 0, 2, 0);
            lblCmbFilterField_2.Name = "lblCmbFilterField_2";
            lblCmbFilterField_2.Size = new Size(182, 30);
            lblCmbFilterField_2.TabIndex = 172;
            lblCmbFilterField_2.Text = "lblCmbFilterField_1";
            lblCmbFilterField_2.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbComboFilterValue_2
            // 
            cmbComboFilterValue_2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.SetColumnSpan(cmbComboFilterValue_2, 3);
            cmbComboFilterValue_2.FormattingEnabled = true;
            cmbComboFilterValue_2.Location = new Point(1371, 217);
            cmbComboFilterValue_2.Margin = new Padding(2);
            cmbComboFilterValue_2.Name = "cmbComboFilterValue_2";
            cmbComboFilterValue_2.Size = new Size(183, 33);
            cmbComboFilterValue_2.TabIndex = 179;
            cmbComboFilterValue_2.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboFilterValue_2.TextChanged += cmbComboFilterValue_TextChanged;
            cmbComboFilterValue_2.Enter += cmbComboFilterValue_Enter;
            cmbComboFilterValue_2.Leave += cmbComboFilterValue_Leave;
            cmbComboFilterValue_2.MouseLeave += cmbComboFilterValue_Leave;
            // 
            // cmbComboTableList
            // 
            cmbComboTableList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cmbComboTableList.AutoCompleteMode = AutoCompleteMode.Suggest;
            cmbComboTableList.AutoCompleteSource = AutoCompleteSource.ListItems;
            tableLayoutPanel.SetColumnSpan(cmbComboTableList, 4);
            cmbComboTableList.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbComboTableList.FormattingEnabled = true;
            cmbComboTableList.Location = new Point(193, 217);
            cmbComboTableList.Margin = new Padding(2);
            cmbComboTableList.Name = "cmbComboTableList";
            cmbComboTableList.Size = new Size(244, 33);
            cmbComboTableList.TabIndex = 167;
            cmbComboTableList.DropDown += AdjustWidthComboBox_DropDown;
            cmbComboTableList.SelectedIndexChanged += cmbComboTableList_SelectedIndexChanged;
            // 
            // button1
            // 
            button1.BackColor = Color.Black;
            button1.CausesValidation = false;
            tableLayoutPanel.SetColumnSpan(button1, 22);
            button1.ForeColor = Color.Black;
            button1.Location = new Point(195, 199);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(1357, 12);
            button1.TabIndex = 189;
            button1.Text = "b";
            button1.UseVisualStyleBackColor = false;
            // 
            // btnReload
            // 
            btnReload.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnReload.BackgroundImage = Properties.Resources.reload;
            btnReload.BackgroundImageLayout = ImageLayout.Zoom;
            btnReload.Location = new Point(1434, 4);
            btnReload.Margin = new Padding(4);
            btnReload.Name = "btnReload";
            btnReload.Size = new Size(56, 38);
            btnReload.TabIndex = 190;
            btnReload.UseVisualStyleBackColor = true;
            btnReload.Click += btnReload_Click;
            // 
            // txtMessages
            // 
            txtMessages.BackColor = SystemColors.ControlLight;
            txtMessages.Dock = DockStyle.Top;
            txtMessages.ForeColor = Color.Red;
            txtMessages.Location = new Point(0, 0);
            txtMessages.Margin = new Padding(2);
            txtMessages.Name = "txtMessages";
            txtMessages.ReadOnly = true;
            txtMessages.Size = new Size(2220, 31);
            txtMessages.TabIndex = 109;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.WhiteSmoke;
            dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.ColumnHeadersHeight = 29;
            dataGridView1.ContextMenuStrip = GridContextMenu;
            dataGridView1.Dock = DockStyle.Left;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.WhiteSmoke;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Margin = new Padding(4, 5, 4, 5);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.RowTemplate.Height = 27;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(2220, 405);
            dataGridView1.TabIndex = 1;
            dataGridView1.CellBeginEdit += dataGridView1_CellBeginEdit;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            dataGridView1.CellEnter += dataGridView1_CellEnter;
            dataGridView1.CellLeave += dataGridView1_CellLeave;
            dataGridView1.CellParsing += dataGridView1_CellParsing;
            dataGridView1.CellValidated += dataGridView1_CellValidated;
            dataGridView1.CellValidating += dataGridView1_CellValidating;
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
            dataGridView1.ColumnHeaderMouseClick += dataGridView1_ColumnHeaderMouseClick;
            dataGridView1.ColumnWidthChanged += dataGridView1_ColumnWidthChanged;
            dataGridView1.CurrentCellDirtyStateChanged += dataGridView1_CurrentCellDirtyStateChanged;
            dataGridView1.DataError += dataGridView1_DataError;
            dataGridView1.EditingControlShowing += dataGridView1_EditingControlShowing;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            dataGridView1.Enter += dataGridView1_Enter;
            dataGridView1.MouseLeave += dataGridView1_MouseLeave;
            dataGridView1.Validating += dataGridView1_Validating;
            dataGridView1.Validated += dataGridView1_Validated;
            // 
            // DataGridViewForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1922, 994);
            Controls.Add(splitContainer1);
            Controls.Add(toolStripBottom);
            Controls.Add(MainMenu1);
            KeyPreview = true;
            Margin = new Padding(4, 5, 4, 5);
            Name = "DataGridViewForm";
            Text = "DataGridViewForm";
            FormClosed += DataGridViewForm_FormClosed;
            Load += DataGridViewForm_Load;
            Resize += DataGridViewForm_Resize;
            MainMenu1.ResumeLayout(false);
            MainMenu1.PerformLayout();
            toolStripBottom.ResumeLayout(false);
            toolStripBottom.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            GridContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        public System.Windows.Forms.MenuStrip MainMenu1;
        public System.Windows.Forms.ToolStripMenuItem mnuFile;
        public System.Windows.Forms.ToolStripMenuItem mnuConnections;
        private System.Windows.Forms.ToolStripMenuItem mnuConnectionList;
        public System.Windows.Forms.ToolStripMenuItem mnuBlankLine3;
        public System.Windows.Forms.ToolStripMenuItem mnuAddConnection;
        public System.Windows.Forms.ToolStripMenuItem mnuDeleteConnection;
        public System.Windows.Forms.ToolStripMenuItem mnuClose;
        public System.Windows.Forms.ToolStripMenuItem mnuOpenTables;
        public System.Windows.Forms.ToolStripMenuItem mnuTranscript;
        public System.Windows.Forms.ToolStripMenuItem mnuTranscriptPrint;
        public System.Windows.Forms.ToolStripMenuItem mnuPrintTranscript;
        public System.Windows.Forms.ToolStripMenuItem mnuPrintTranscriptEnglish;
        public System.Windows.Forms.ToolStripMenuItem mnuPrintRole;
        public System.Windows.Forms.ToolStripMenuItem mnuPrintGrade;
        public System.Windows.Forms.ToolStripMenuItem mnuLocations;
        public System.Windows.Forms.ToolStripMenuItem mnuTranscriptFolder;
        public System.Windows.Forms.ToolStripMenuItem mnuTranscriptTemplate;
        public System.Windows.Forms.ToolStripMenuItem mnuTranscriptTemplateEnglish;
        public System.Windows.Forms.ToolStripMenuItem mnuRoleTemplate;
        public System.Windows.Forms.ToolStripMenuItem mnuGradeTemplate;
        public System.Windows.Forms.ToolStripMenuItem mnuLine;
        public System.Windows.Forms.ToolStripMenuItem mnuPrintTermSummary;
        public System.Windows.Forms.ToolStripMenuItem mnuAddressBook;
        public System.Windows.Forms.ToolStripMenuItem mnuAddressBookLabels;
        public System.Windows.Forms.ToolStripMenuItem mnuAddressBookPhoneNumbers;
        public System.Windows.Forms.ToolStripMenuItem mnuAddressBookAddresses;
        public System.Windows.Forms.ToolStripMenuItem mnuAddressBookEmails;
        public System.Windows.Forms.ToolStripMenuItem mnuAddressBookGetEmails;
        public System.Windows.Forms.ToolStripMenuItem mnuTools;
        public System.Windows.Forms.ToolStripMenuItem mnuPrintCurrentTable;
        public System.Windows.Forms.ToolStripMenuItem mnuHelp;
        public System.Windows.Forms.ToolStripMenuItem mnuHelpFile;
        private ToolStrip toolStripBottom;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private ToolStripButton toolStripButton4;
        private ToolStripButton toolStripButton5;
        private SplitContainer splitContainer1;
        private TableLayoutPanel tableLayoutPanel;
        private Label lblMainFilter;
        private DataGridView dataGridView1;
        private ToolStripLabel toolStripMsg;
        private ContextMenuStrip GridContextMenu;
        private ToolStripMenuItem GridContextMenu_FindInDescendent;
        private ComboBox cmbMainFilter;
        private RadioButton rbAdd;
        private RadioButton rbEdit;
        private RadioButton rbView;
        private RadioButton rbDelete;
        private RadioButton rbMerge;
        private TextBox txtMessages;
        private ToolStripTextBox txtRecordsPerPage;
        private Button btnDeleteAddMerge;
        private ToolStripMenuItem mnuDatabaseInfo;
        private ComboBox cmbGridFilterFields_0;
        private ToolStripMenuItem contextMenu_ShowAllFilters;
        private ComboBox cmbGridFilterFields_1;
        private ComboBox cmbGridFilterFields_2;
        private Label lblGridFilter;
        private Label lblComboFilter;
        private ComboBox cmbGridFilterValue_3;
        private ComboBox cmbGridFilterFields_4;
        private ComboBox cmbGridFilterValue_4;
        private ComboBox cmbGridFilterFields_5;
        private ComboBox cmbGridFilterValue_1;
        private ComboBox cmbGridFilterValue_5;
        private ComboBox cmbGridFilterValue_2;
        private ComboBox cmbGridFilterValue_0;
        private ComboBox cmbGridFilterFields_3;
        private ComboBox cmbComboTableList;
        private Label lblCmbFilterField_0;
        private Label lblCmbFilterField_1;
        private Label lblCmbFilterField_2;
        private Label lblCmbFilterField_3;
        private Label lblCmbFilterField_4;
        private Label lblCmbFilterField_5;
        private ComboBox cmbComboFilterValue_0;
        private ComboBox cmbComboFilterValue_1;
        private ComboBox cmbComboFilterValue_2;
        private ComboBox cmbComboFilterValue_3;
        private ComboBox cmbComboFilterValue_4;
        private ComboBox cmbComboFilterValue_5;
        private ToolStripMenuItem GridContextMenu_FindInAncestor;
        private ToolStripMenuItem mnuDuplicateDisplayKeys;
        private ToolStripMenuItem mnuForeignKeyMissing;
        private ToolStripMenuItem mnuViewLog;
        private ComboBox cmbGridFilterFields_7;
        private ComboBox cmbGridFilterValue_6;
        private ComboBox cmbGridFilterFields_6;
        private ComboBox cmbGridFilterValue_7;
        private ComboBox cmbGridFilterFields_8;
        private ComboBox cmbGridFilterValue_8;
        private Button button1;
        private Button btnReload;
        private ToolStripMenuItem deleteUnusedForeignKeysToolStripMenuItem;
        private ToolStripMenuItem correctDuplicateDKsToolStripMenuItem;
        private ToolStripMenuItem GridContextMenu_TimesUsedAsFK;
        private ToolStripMenuItem mnuTranscriptCheckGradRequirements;
        private ToolStripButton toolStripButtonColumnWidth;
        //private Button button2;
    }
}