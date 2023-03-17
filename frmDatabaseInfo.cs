using System.Data;
using System.Data.SqlClient;
using System.Text;
//using static System.ComponentModel.Design.ObjectSelectorEditor;


namespace SqlCollegeTranscripts
{

    public partial class frmDatabaseInfo : Form
    {
        public frmDatabaseInfo()
        {
            InitializeComponent();
        }

        public string job { get; set; }

        private void frmDatabaseInfo_Load(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(job))
            {
                if (dataHelper.fieldsDT != null) { dgvMain.DataSource = dataHelper.fieldsDT; }
            }
            // 8. Build English database - will do nothing if Boolean BuildingUpEnglishDatabase in MultiLingual.cs set to false
            MultiLingual.InsertEnglishIntoDatabase(this);
        }

        private void cmdTables_Click(object sender, EventArgs e)
        {
            if (dataHelper.tablesDT != null) { dgvMain.DataSource = dataHelper.tablesDT; }
        }

        private void cmdForeignKeys_Click(object sender, EventArgs e)
        {
            if (dataHelper.foreignKeysDT != null) { dgvMain.DataSource = dataHelper.foreignKeysDT; }
        }

        private void cmdIndexes_Click(object sender, EventArgs e)
        {
            if (dataHelper.indexesDT != null) { dgvMain.DataSource = dataHelper.indexesDT; }
        }

        private void cmdFields_Click(object sender, EventArgs e)
        {
            if (dataHelper.fieldsDT != null) { dgvMain.DataSource = dataHelper.fieldsDT; }
        }

        private void cmdIndexColumns_Click(object sender, EventArgs e)
        {
            if (dataHelper.indexColumnsDT != null) { dgvMain.DataSource = dataHelper.indexColumnsDT; }
        }

        private void cmdExtraDT_Click(object sender, EventArgs e)
        {
            if (dataHelper.extraDT != null) { dgvMain.DataSource = dataHelper.extraDT; }
        }

        private void frmDatabaseInfo_FormClosed(object sender, FormClosedEventArgs e)
        {
            dgvMain.DataSource = null;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
