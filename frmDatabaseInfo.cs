using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SqlCollegeTranscripts;
using static System.ComponentModel.Design.ObjectSelectorEditor;
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
        public string table { get; set; }
        public List<string> indexColumns { get; set; }

        private DataTable? localDT;


        private void frmDatabaseInfo_Load(object sender, EventArgs e)
        {
            if (job == "CreateGroups")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT courseID, termID, graduateCourse, count(*) FROM CourseTerms ");
                sb.Append("GROUP BY courseID, termID, graduateCourse ");
                sb.Append("HAVING count(*) > 1");
                string strSql = sb.ToString();
                localDT= new DataTable();
                MsSql.FillDataTable(localDT, strSql);
                dgvMain.DataSource = localDT;
            }
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
            if(dataHelper.indexesDT != null) { dgvMain.DataSource = dataHelper.indexesDT; }
        }

        private void cmdFields_Click(object sender, EventArgs e)
        {
            if (dataHelper.fieldsDT != null) { dgvMain.DataSource = dataHelper.fieldsDT; }
        }

        private void cmdIndexColumns_Click(object sender, EventArgs e)
        {
            if (dataHelper.indexColumnsDT != null) { dgvMain.DataSource = dataHelper.indexColumnsDT; }
        }
        private void frmDatabaseInfo_FormClosed(object sender, FormClosedEventArgs e)
        {
            dgvMain.DataSource = null;
        }
    }
}
