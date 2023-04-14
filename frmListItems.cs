//using System.Data;
// using System.Data.SqlClient;

namespace SqlCollegeTranscripts
{
    internal partial class frmListItems
        : System.Windows.Forms.Form
    {
        public frmListItems() : base()
        {
            //This call is required by the Windows Form Designer.
            InitializeComponent();
        }

        public enum job { 
            DeleteConnections,
            SelectString
        }

        //This form used for (1) deleting connections AND listing / selecting a from a list
        public job myJob;

        // For deleting connection, we will build a list of connection strings - see below
        List<connectionString> csList = new List<connectionString>();

        // For a list, we will feed in the list into myList
        public List<string> myList = new List<string>();

        public string formCaption;
        public string returnString = string.Empty;
        public int returnIndex = -1;

        private void frmListItems_Load(object sender, EventArgs e)
        {
            // listing strings to select - maybe databases or tables or anything
            if (myJob == job.SelectString)  
            {
                this.cmdExit.Text = "OK";
            }
            else if(myJob== job.DeleteConnections)   // deleting databases fromlist
            {
                this.cmdExit.Text = "Delete";
            }
            this.Text = formCaption; //"List of Databases on the Server";
            listBox1.Items.AddRange(myList.ToArray());

            // 8. Build English database - will do nothing if Boolean BuildingUpEnglishDatabase in MultiLingual.cs set to false
            MultiLingual.InsertEnglishIntoDatabase(this);
        }

        // Set width of form
        // B. Get and set width
        //int width = senderComboBox.DropDownWidth;
        //using (Graphics g = senderComboBox.CreateGraphics())
        //{
        //    System.Drawing.Font font = senderComboBox.Font;
        //    int vertScrollBarWidth = (senderComboBox.Items.Count > senderComboBox.MaxDropDownItems)
        //        ? SystemInformation.VerticalScrollBarWidth : 0;
        //    var itemsList = senderComboBox.Items.Cast<object>().Select(item => item.ToString());
        //    foreach (string s in displayValueList)
        //    {
        //        int newWidth = (int)g.MeasureString(s, font).Width + vertScrollBarWidth;
        //        if (width < newWidth)
        //        {
        //            width = newWidth;
        //        }
        //    }
        //}
        //senderComboBox.DropDownWidth = width;
        
 
        private void cmdExit_Click(Object eventSender, EventArgs eventArgs)
        {   // Return selected item 
            returnString = listBox1.GetItemText(listBox1.SelectedItem);
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            { 
                returnIndex = listBox1.SelectedIndex;
            }
        }

    }
}