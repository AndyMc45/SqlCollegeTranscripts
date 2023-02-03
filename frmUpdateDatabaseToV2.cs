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
                formDI.job = "CreateGroups";
                List<string> ic = new List<string>();
                ic.Add("courseID");
                ic.Add("termID");
                formDI.indexColumns = ic;
                formDI.ShowDialog();
            }


        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
