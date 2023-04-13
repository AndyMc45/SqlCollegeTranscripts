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
            runTimer = true;
            loadingMainFilter = false;
            pageSize = 0;
            logFileName = string.Empty;
        }
        internal bool debugging { get; set; }
        internal bool runTimer { get; set; }
        internal bool loadingMainFilter { get; set; } //use updating to stop events when making programatic changes 
        internal int pageSize { get; set; }
        internal string logFileName { get; set; }

        internal FileStream? ts;
        internal Color[] nonDkColorArray = new Color[] { Color.LightCyan, Color.LavenderBlush, Color.SeaShell, Color.AliceBlue, Color.LightGray, Color.LightSalmon, Color.Azure, Color.LightCyan };
        internal Color[] DkColorArray = new Color[] { Color.MediumSpringGreen, Color.PaleGreen, Color.LightGreen, Color.GreenYellow, Color.MediumSpringGreen, Color.PaleGreen, Color.LightGreen, Color.GreenYellow};
        internal Color DefaultColumnColor = Color.Yellow;
        internal Color PrimaryKeyColor = Color.Pink;

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
            writingTable = false;
            doNotRebindGridFV = false;
            doNotWriteGrid = false;
            fixingDatabase = false;   // Manually set this - 
            strFixingDatabaseSql = string.Empty;
            FkFieldInEditingControl = null;
            tableHasForeignKeys = false;
            currentComboFilterValue_isDirty = false;
        }
        internal bool fixingDatabase { get; set; }
        internal string strFixingDatabaseSql { get; set; }
        internal bool writingTable { get; set; }
        internal field? FkFieldInEditingControl { get; set; }
        internal bool tableHasForeignKeys { get; set; }
        internal bool currentComboFilterValue_isDirty { get; set; }
        internal bool doNotRebindGridFV { get; set; }
        internal bool doNotWriteGrid { get; set; }

    }

}
