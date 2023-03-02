using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SqlCollegeTranscripts
{
    public class FkComboColumn : DataGridViewColumn
    {
        public FkComboColumn() : base(new FkComboCell())  {   }
       
        public override DataGridViewCell CellTemplate
        {  
            get
            {
                return base.CellTemplate;
            }
            set
            {
                // Ensure that the cell used for the template is a CalendarCell.
                if (value != null &&
                    !value.GetType().IsAssignableFrom(typeof(FkComboCell)))
                {
                    throw new InvalidCastException("Must be a FkComboCell");
                }
                base.CellTemplate = value;
            }
        }

    }

    public class FkComboCell : DataGridViewTextBoxCell
    {

        public FkComboCell() : base()
        {
            // Use the short date format.
            // this.Style.Format = "d";
        }

        public DataTable dataTable { get; set; }
 
        public override object Clone()
        {
            var clone = (FkComboCell)base.Clone();
            // clone.dataTable = dt;
            return clone;
        }

        public override void InitializeEditingControl(int rowIndex, object
            initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            // Set the value of the editing control to the current cell value.
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
            FkComboBoxEditingControl ctl = DataGridView.EditingControl as FkComboBoxEditingControl;
            if (ctl != null) { 
                // Fill the combo.
                ctl.DataSource = null;   
                ctl.Items.Clear(); // ? if needed
                ctl.ValueMember = "ValueField";
                ctl.DisplayMember = "DisplayField";
                ctl.DataSource = dataTable;
                // Other combo settings
                ctl.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                ctl.AutoCompleteSource = AutoCompleteSource.ListItems;
            }
        }

        public override Type EditType
        {
            get
            {
                // Return the type of the editing control that CalendarCell uses.
                return typeof(FkComboBoxEditingControl);
            }
        }

        public override Type ValueType
        {
            get
            {
                // Return the type of the value that CalendarCell contains.

                return typeof(Int32);
            }
        }

        public override object DefaultNewRowValue
        {
            get
            {
                // Use the current date and time as the default value.
                return 0;
            }
        }
    }

    public class FkComboBoxEditingControl : DataGridViewComboBoxEditingControl
    {

        public override object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return this.SelectedValue;
        }

        public DataTable dataTable { 
            set 
            {
                this.ValueMember = "ValueField";
                this.DisplayMember = "DisplayField";
                this.DataSource = value;
            }
        }
    }
}