using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlCollegeTranscripts
{
    internal class FormOptions
    {
        internal FormOptions()
        {
            debugging = false;
            runTimer = false;
            updating = false;
            pageSize = 0;
            logFileName = string.Empty;
        }
        internal bool debugging { get; set; }
        internal bool runTimer { get; set; }
        internal bool updating { get; set; } //use updating to stop events when making programatic changes 
        internal int pageSize { get; set; }
        internal string logFileName { get; set; }

        internal FileStream? ts;
        internal Color[] ColorArray = new Color[] { Color.LightCyan, Color.LavenderBlush, Color.SeaShell, Color.LightGreen, Color.AliceBlue, Color.LightGray, Color.LightSalmon, Color.Azure };
        internal Color[] DKColorArray = new Color[] { Color.GreenYellow, Color.LightGreen, Color.PaleGreen, Color.Chartreuse, Color.GreenYellow, Color.LightGreen, Color.PaleGreen, Color.Chartreuse };

        internal Color DisplayKeyColor = Color.MediumSpringGreen;
    }

    internal class ConnectionOptions
    {
        public ConnectionOptions() 
        {
            readOnly = false;
            mySql = false;
            msSql= false;
        }
        internal bool readOnly { get; set; }
        internal bool mySql { get; set; }
        internal bool msSql { get; set; }
    }

    internal class TableOptions
    {
        internal TableOptions() 
        {
            updating = false;
            fixingDatabase = false;
            strFixDatabaseWhereCondition = string.Empty;
            FkFieldInEditingControl = null;
        }
        internal bool fixingDatabase { get; set; }
        internal string strFixDatabaseWhereCondition { get; set; }
        internal bool updating { get; set; }
        internal field? FkFieldInEditingControl { get; set; }
    }

}
