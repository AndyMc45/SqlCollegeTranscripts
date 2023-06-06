namespace SqlCollegeTranscripts
{
    internal partial class frmCaptions
        : System.Windows.Forms.Form
    {
        internal frmCaptions(string caption, string job)
        {
            this.Text = caption;
            this.job = job;
            labels = new List<Label>();
            tableCollection = new List<string>();
            //This call is required by the Windows Form Designer.
            InitializeComponent();
        }

        internal string selectedCaption = "";
        internal string job = "";
        internal int totalPages = 0;
        internal List<string> tableCollection;
        internal List<Label> labels;
        internal int gridFormWidth = 0;
        internal int gridFormLeftLocation = 0;
        private bool loading = false;

        private void frmCaptions_Load(object sender, EventArgs e)
        {
            //ASM int j = 0 
            //ASM int captionCount = 0;
            this.Height = 105;
            int left = 35;
            int top = 35;
            loading = true;
            if (job == "pages")
            {
                //Create new labels - format and add to controls
                for (int i = 0; i <= totalPages - 1; i++)
                {
                    Label lbl = new Label();
                    lbl.Click += label_Click;
                    //increase width if need be
                    if (left + 600 + 35 > this.Width * 15)
                    {
                        if (left + 600 + 70 < gridFormWidth * 7)
                        {
                            this.Width = (left + 600 + 70) / 15;
                            this.Location = new Point(gridFormWidth / 2 - this.Width / 2 + gridFormLeftLocation, this.Location.Y);
                            // this.Left = this.Left - (left + 600 + 70) / 30;
                        }
                        else  // Begin new row
                        {
                            this.Height += 30;
                            top += 300;
                            left = 35;
                        }
                    }
                    lbl.Top = top / 15;
                    lbl.Left = left / 15;
                    lbl.Text = (i + 1).ToString();
                    lbl.Visible = true;
                    // lbl.Anchor = AnchorStyles.D;
                    lbl.Width = 30;
                    left += 600;
                    this.Controls.Add(lbl);
                }
            }
            else if (job == "tableCollection")
            {
                this.Width = 200;
                for (int i = 0; i < tableCollection.Count; i++)
                {
                    //Set width
                    if (this.Width * 15 < tableCollection[i].Length * 300)
                    {
                        if (i * 300 < gridFormWidth * 15)
                        {
                            this.Width = i * 300 / 15;
                        }
                        else
                        {
                            //In case the datagrid is very small
                            this.Width = gridFormWidth;
                        }
                    }
                    //if (i > 0)
                    //{
                    Label lbl = new Label();
                    lbl.Click += label_Click;
                    labels.Add(lbl);

                    //}
                    labels[i].TextAlign = ContentAlignment.TopLeft;
                    labels[i].Visible = true;
                    labels[i].Width = this.Width - left / 15;
                    labels[i].Left = left / 15; //35
                    if (i > 0)
                    {
                        this.Height += 20;
                        top += 300;
                    }
                    labels[i].Top = top / 15;
                    labels[i].Text = i.ToString();
                }
            }
            else if (job == "cmdAdd")
            {
                this.Width = 667;
                this.Height = 79;
                Label lbl = new Label();
                lbl.Click += label_Click;
                labels.Add(lbl);  // label[[0]
                label[0].Left = left / 15;
                label[0].Top = top / 15;
                label[0].Width = (10000 - (2 * left)) / 15;
                label[0].Text = (string)tableCollection[0];
                label[1].Left = left / 15;
                label[1].Top = (top + 300) / 15;
                label[1].Width = (10000 - (2 * left)) / 15;
                label[1].Text = (string)tableCollection[1];
                label[1].Visible = true;
            }
            loading = false;

        }
        private void label_Click(Object eventSender, EventArgs eventArgs)
        {
            Label lbl = (Label)eventSender;
            selectedCaption = lbl.Text;
            this.Close();
        }
        private void Form_Closed(Object eventSender, EventArgs eventArgs)
        {
        }


    }
}