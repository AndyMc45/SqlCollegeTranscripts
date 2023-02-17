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
            btnLoadCommand.Enabled = true;
            btnExecuteCommand.Enabled = false;
            PrepareForLoadCommand();
            // 8. Build English database - will do nothing if Boolean BuildingUpEnglishDatabase in MultiLingual.cs set to false
            // MultiLingual.InsertEnglishIntoDatabase(this);
        }

        private void PrepareForLoadCommand()
        {
            if (job == "CourseTerms")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT courseID, termID, graduateCourse, section, count(*) FROM CourseTerms ");
                sb.Append("GROUP BY courseID, termID, graduateCourse, section ");
                sb.Append("HAVING count(*) > 1");
                string strSql = sb.ToString();
                using (DataTable readOnlyDT = new DataTable("TemporaryAdaptor"))
                {
                    MsSql.FillDataTable(readOnlyDT, strSql);
                    dgvMain.DataSource = readOnlyDT;
                }
            }
            else if (job == "Courses")
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT courseName, departmentID, count(*) FROM Courses ");
                sb.Append("GROUP BY courseName, departmentID ");
                sb.Append("HAVING count(*) > 1");
                string strSql = sb.ToString();
                using (DataTable readOnlyDT = new DataTable("readOnly"))
                {
                    MsSql.FillDataTable(readOnlyDT, strSql);
                    dgvMain.DataSource = readOnlyDT;
                }
            }
        }

        private void btnLoadCommand_Click(object sender, EventArgs e)
        {
            if (job == "CourseTerms")
            {
                if(dgvMain.Rows.Count > 0)
                {
                    sqlFactory sqlFact = new sqlFactory("CourseTerms", 1, 200);
//                    sqlFact.callInnerJoins();
                    int gridColumn = dgvMain.Columns["courseID"].Index;  //Original dgvMain (groups)
                    string courseID = dgvMain.Rows[0].Cells[gridColumn].Value.ToString();
                    field fl = new field("CourseTerms", "courseID", DbType.Int32, 4);
                    where wh1 = new where(fl, courseID);
                    gridColumn = dgvMain.Columns["termID"].Index;
                    string termID = dgvMain.Rows[0].Cells[gridColumn].Value.ToString();
                    fl = new field("CourseTerms", "termID", DbType.Int32, 4);
                    where wh2 = new where(fl, termID);
                    gridColumn = dgvMain.Columns["graduateCourse"].Index;
                    string graduateCourse = dgvMain.Rows[0].Cells[gridColumn].Value.ToString();
                    fl = new field("CourseTerms", "graduateCourse", DbType.Boolean, 4);
                    where wh3 = new where(fl, graduateCourse);
                    sqlFact.myWheres.Add(wh1);
                    sqlFact.myWheres.Add(wh2);
                    sqlFact.myWheres.Add(wh3);
                    string sqlString = sqlFact.returnSql(command.select);

                    // Fill dgv with extraDT
                    dataHelper.extraDT = new DataTable("extraDT");
                    MsSql.FillDataTable(dataHelper.extraDT, sqlString);
                    dgvMain.DataSource = dataHelper.extraDT;    

                    // Set update command for extraDT
                    string strUpdate = "UPDATE CourseTerms SET section = @section WHERE courseTermID = @courseTermID";
                    SqlCommand cmdUpdate = new SqlCommand(strUpdate, MsSql.cn);
                    cmdUpdate.Parameters.Clear();
                    cmdUpdate.Parameters.Add("@courseTermID", SqlDbType.Int, 4, "courseTermID");
                    cmdUpdate.Parameters.Add("@section", SqlDbType.Int, 4, "section");
                    MsSql.extraDA.UpdateCommand = cmdUpdate;
                    btnLoadCommand.Enabled = false;
                    btnExecuteCommand.Enabled = true;
                    int i = 1;
                    int j = dgvMain.Columns.IndexOf(dgvMain.Columns["section"]);
                    foreach (DataGridViewRow dr in dgvMain.Rows)
                    {
                        dgvMain[j,i-1].Value = i.ToString();
                        i = i + 1;
                    }
                }

            }
        }

        private void btnExecuteCommand_Click(object sender, EventArgs e)
        {
            MsSql.extraDA.Update(dataHelper.extraDT);
            PrepareForLoadCommand();
            btnExecuteCommand.Enabled = false;  
            btnLoadCommand.Enabled = true;
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
        
    }
}
