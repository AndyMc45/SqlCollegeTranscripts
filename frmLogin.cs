
namespace SqlCollegeTranscripts
{
    internal partial class frmLogin : Form
    {
        internal string? password;
        internal frmLogin()
        {
            InitializeComponent();
        }
        private void frmLogin_Load(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = '#';
        }
        
        private void cmdCancel_Click(Object eventSender, EventArgs eventArgs)
        {
            password = "";
            this.Close();
        }
        private void cmdOK_Click(Object eventSender, EventArgs eventArgs)
        {
            password = txtPassword.Text;
            this.Close();

        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { SendKeys.Send("{TAB}"); }

        }
    }
}