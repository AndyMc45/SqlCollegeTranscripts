using System.Data;
// using static System.Windows.Forms.VisualStyles.VisualStyleElement;
// using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SqlCollegeTranscripts
{
    internal static class MultiLingual
    {
        private static bool BuildingUpEnglishDatabase = false;  // Change this to true to collect English values

        public static string tr(string strEnglish, Form form)
        {
            if (BuildingUpEnglishDatabase)
            {
                UpdateOrInsertOneRow(form.Name, "Translate", "tr", strEnglish);
            }
            return strEnglish;  // To change later
        }

        private static List<Control> GetChildList(Control cnt)
        {
            List<Control> listA = new List<Control>();
            listA.Add(cnt);
            foreach (Control c in cnt.Controls)
            {
                List<Control> listB = GetChildList(c);
                listA.AddRange(listB);
            }
            return listA;
        }

        internal static void InsertEnglishIntoDatabase(Form form)
        {
            if (!BuildingUpEnglishDatabase) { return; }

            //Load zMultilingual into dtExtra
            string strSql = "Select zMultilingualID, Form, Control, Property, en from zMultilingual";
            MsSql.FillDataTable(dataHelper.extraDT, strSql);
            List<Control> listA = GetChildList(form);
            foreach (Control cnt in listA)
            {
                if (cnt is Label)
                {
                    Label lbl = (Label)cnt;
                    UpdateOrInsertOneRow(form.Name, cnt.Name, "Text", lbl.Text);
                }
                else if (cnt is RadioButton)
                {
                    RadioButton radioButton = (RadioButton)cnt;
                    UpdateOrInsertOneRow(form.Name, cnt.Name, "Text", radioButton.Text);
                }
                else if (cnt is TextBox)
                {
                    TextBox textBox = (TextBox)cnt;
                    UpdateOrInsertOneRow(form.Name, cnt.Name, "PlaceholderText", textBox.PlaceholderText);
                }
                else if (cnt is MenuStrip)
                {
                    MenuStrip ms = (MenuStrip)cnt;
                    foreach (ToolStripMenuItem tsmi in ms.Items)
                    {
                        ProcessToolStripMenuItem(tsmi, form);
                    }
                }
                else if (cnt is ToolStrip)
                {
                    ToolStrip ts = (ToolStrip)cnt;
                    foreach (ToolStripItem tsmi in ts.Items)
                    {
                        if (tsmi is ToolStripLabel)
                        {
                            ToolStripLabel lbl = (ToolStripLabel)tsmi;
                            UpdateOrInsertOneRow(form.Name, cnt.Name, "PlaceholderText", lbl.Text);
                        }
                    }
                }
            }
        }

        private static void ProcessToolStripMenuItem(ToolStripMenuItem tsmi, Form form)
        {
            UpdateOrInsertOneRow(form.Name, tsmi.Name, "Text", tsmi.Text);
            foreach (ToolStripMenuItem tsmi2 in tsmi.DropDown.Items)
            {
                ProcessToolStripMenuItem(tsmi2, form);
            }

        }

        static void UpdateOrInsertOneRow(string formName, string controlName, string property, string en)
        {
            if (!String.IsNullOrEmpty(en))
            {
                string condition = string.Format("Form = '{0}' AND Control = '{1}' AND Property = '{2}'", formName, controlName, property);
                DataRow dr = dataHelper.extraDT.Select(condition).FirstOrDefault();
                if (dr != null)
                {
                    dr["en"] = en;
                }
                else
                {
                    dr = dataHelper.extraDT.NewRow();
                    dr["Form"] = formName;
                    dr["Control"] = controlName;
                    dr["Property"] = property;
                    dr["en"] = en;
                    dataHelper.extraDT.Rows.Add(dr);
                }
            }
        }
    }
}