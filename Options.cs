using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlCollegeTranscripts
{
    public class FormOptions
    {
        public FormOptions()   // After the program runs once, these options are stored in registry - no use changing here
        {
            debugging = false;
            runTimer = false;
            loadingMainFilter = false;
            narrowColumns = false;
            pageSize = 0;
            logFileName = string.Empty;
        }
        public bool debugging { get; set; }
        public bool runTimer { get; set; }
        public bool loadingMainFilter { get; set; } //use updating to stop events when making programatic changes 
        public int pageSize { get; set; }
        public string logFileName { get; set; }
        public bool narrowColumns { get; set; }

        public FileStream? ts;
        public Color[] nonDkColorArray = new Color[] { Color.LightCyan, Color.LavenderBlush, Color.Plum, Color.Pink, Color.LightGray, Color.LightSalmon, Color.Azure, Color.OrangeRed };
        public Color[] DkColorArray = new Color[] { Color.MediumSpringGreen, Color.PaleGreen, Color.LightGreen, Color.GreenYellow, Color.MediumSpringGreen, Color.PaleGreen, Color.LightGreen, Color.GreenYellow};
        public Color DefaultColumnColor = Color.Yellow;
        public Color PrimaryKeyColor = Color.Pink;

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
            firstTimeWritingTable = true;
            currentComboFilterValue_isDirty = false;
            allowDisplayKeyEdit = false;

        }
        internal bool fixingDatabase { get; set; }
        internal string strFixingDatabaseSql { get; set; }
        internal bool writingTable { get; set; }
        internal field? FkFieldInEditingControl { get; set; }
        internal bool tableHasForeignKeys { get; set; }
        internal bool currentComboFilterValue_isDirty { get; set; }
        internal bool doNotRebindGridFV { get; set; }
        internal bool doNotWriteGrid { get; set; }
        internal bool firstTimeWritingTable { get; set; }
        internal bool allowDisplayKeyEdit { get; set; }

    }

}
