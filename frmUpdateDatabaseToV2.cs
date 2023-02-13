namespace SqlCollegeTranscripts
{
    public partial class frmUpdateDatabaseToV2 : Form
    {
        public frmUpdateDatabaseToV2()
        {
            InitializeComponent();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void cmdAddSectionsToCT_Click(object sender, EventArgs e)
        {
            using (frmDatabaseInfo formDI = new frmDatabaseInfo())
            {
                formDI.job = "CourseTerms";
                formDI.ShowDialog();
            }
        }
        private void btnMergeCourses_Click(object sender, EventArgs e)
        {
            using (frmDatabaseInfo formDI = new frmDatabaseInfo())
            {
                formDI.job = "Courses";
                formDI.ShowDialog();
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
}
