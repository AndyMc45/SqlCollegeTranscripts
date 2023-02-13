using System.Data;
using System.Text;

namespace SqlCollegeTranscripts
{
    #region Helper classes - field, where, innerJoin, orderBy, and enum command

    public class field
    {
        public string fieldName { get; set; }
        public string table { get; set; }
        public string dbType { get; set; }
        public int size { get; set; }   

        public field(string table, string fieldName, string dbType, int size)
        {
            this.table = table;
            this.fieldName = fieldName;
            this.dbType = dbType;
            this.size = size;
        }
    }

    public class where
    {
        public where(field fl, string whereValue)
        {
            this.fl = fl;
            this.whereValue = whereValue;
        }
        public where(string table, string field, string whereValue, string dbType, int size)
        {
            this.fl = new field(table, field, dbType, size);
            this.whereValue = whereValue;
        }
        public field fl { get; set; }
        public string whereValue { get; set; }
        public string displayValue { get; set; }  // Used to display where in combo
    }

    internal class innerJoin
    {
        internal field fld { get; set; }
        internal string table2 { get; set; }
        internal string table2PrimaryKey { get; set; }
        internal string table2Allias { get; set; }
        internal innerJoin(field fld, string table2)
        {
            this.fld = fld;
            this.table2 = table2;
            // All inner joins must be to the primary field of table2
            this.table2PrimaryKey = dataHelper.getTablePrimaryKeyField(table2).fieldName;
            this.table2Allias = string.Empty;
        }
    }

    internal class orderBy
    {
        internal field fld;
        internal System.Windows.Forms.SortOrder sortOrder;

        internal orderBy(field fld, System.Windows.Forms.SortOrder sortOrder)
        {
            this.fld = fld;
            this.sortOrder = sortOrder;
        }
    }

    internal enum command
    {
        select,
        count,
        fkfilter,
        cellfilter
    }

    internal enum ProgramMode
    {
        view,
        edit,
        add,
        delete,
        merge
    }



    #endregion

    public static class dataHelper
    {
        // Variables
        public static DataTable? currentDT = null;   //Data Table that is bound to the datagrid
        public static DataTable? tablesDT = null;
        public static DataTable? fieldsDT = null;
        public static DataTable? foreignKeysDT = null;
        public static DataTable? indexesDT = null;
        public static DataTable? indexColumnsDT = null;
        public static DataTable? extraDT = null;

        internal static void clearDataTables()
        {
            currentDT = null;
            tablesDT = null;
            fieldsDT = null;
            foreignKeysDT = null;
            indexesDT = null;
            indexColumnsDT = null;
            extraDT = null;
    }

        internal static string QualifiedFieldName(field fld)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[" + fld.table + "]");
            sb.Append(".");
            sb.Append("[" + fld.fieldName + "]");
            return sb.ToString();
        }

        #region  Get values from FieldsDT DataRow (&& overloads)

        internal static string getStringValueFieldsDT(DataRow dr, string fieldToReturn)
        {
            return Convert.ToString(dr[dr.Table.Columns.IndexOf(fieldToReturn)]);
        }

        internal static int getIntValueFieldsDT(DataRow dr, string fieldToReturn)
        {
            return Convert.ToInt32(dr[dr.Table.Columns.IndexOf(fieldToReturn)]);
        }

        internal static bool getBoolValueFieldsDT(DataRow dr, string fieldToReturn)
        {
            int returnField = dr.Table.Columns.IndexOf(fieldToReturn);
            return Convert.ToBoolean(dr[returnField]);
        }

        internal static field getFieldValueFieldsDT(DataRow dr)
        {
            string tableName = getStringValueFieldsDT(dr, "TableName");
            string columnName = getStringValueFieldsDT(dr, "ColumnName");
            int size = getIntValueFieldsDT(dr, "MaxLength"); ;
            string dbType = getStringValueFieldsDT(dr, "DataType");
            return new field(tableName, columnName, dbType, size);
        }

        internal static bool isDisplayKey(string tableName, string columnName)
        {
            return dataHelper.getBoolValueFieldsDT(tableName, columnName, "is_DK");
        }


        // Begin overloads  -- Gets DataRow and then use above methods

        internal static DataRow getDataRow(string tableName, string columnName)
        {
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("TableName = '{0}' AND ColumnName = '{1}'", tableName, columnName)).FirstOrDefault();
            return dr;
        }

        internal static field getFieldValueFieldsDT(string tableName, string columnName)
        {
            DataRow dr = getDataRow(tableName, columnName);
            return getFieldValueFieldsDT(dr);
        }

        internal static string getStringValueFieldsDT(string tableName, string columnName, string columnToReturn)
        {
            DataRow dr = getDataRow(tableName, columnName);
            return getStringValueFieldsDT(dr,columnToReturn);
        }

        internal static int getIntValueFieldsDT(string tableName, string columnName, string columnToReturn)
        {
            DataRow dr = getDataRow(tableName,columnName);
            return getIntValueFieldsDT(dr,columnToReturn);
        }

        internal static bool getBoolValueFieldsDT(string tableName, string columnName, string columnToReturn)
        {
            DataRow dr = getDataRow(tableName, columnName);
            return getBoolValueFieldsDT(dr,columnToReturn);
        }

        internal static field getFieldFromTableAndColumnName(string tableName, string columnName)
        {
            string dbType = getStringValueFieldsDT(tableName, columnName, "DataType");
            int size = getIntValueFieldsDT(tableName, columnName, "MaxLength");
            field fi = new field(tableName, columnName, dbType, size);
            return fi;
        }

        internal static bool isTablePrimaryKeyField(field columnField)
        {
            return getBoolValueFieldsDT(columnField.table, columnField.fieldName, "is_PK");
        }
        internal static bool fieldIsForeignKey(field columnField)
        {
            return getBoolValueFieldsDT(columnField.table, columnField.fieldName, "is_FK");
        }

        internal static field getForeignKeyRefField(DataRow dr)
        {   // Assumes we have checked that this row in the FieldsDT is a foreignkey
            string FkRefTable = getStringValueFieldsDT(dr, "FkRefTable");
            string FkRefCol = getStringValueFieldsDT(dr, "FkRefCol");
            return getFieldFromTableAndColumnName(FkRefTable,FkRefCol);
        }
  
        internal static field getForeignKeyRefField(field foreignKey)
        {   // Assumes we have checked that this row in the FieldsDT is a foreignkey
            DataRow dr = getDataRow(foreignKey.table,foreignKey.fieldName);
            string FkRefTable = getStringValueFieldsDT(dr, "RefTable");
            string FkRefCol = getStringValueFieldsDT(dr, "RefPkColumn");
            return getFieldFromTableAndColumnName(FkRefTable, FkRefCol);
        }

        internal static field getTablePrimaryKeyField(string table)
        {
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("TableName = '{0}' AND is_PK", table)).FirstOrDefault();
            return getFieldValueFieldsDT(dr);
        }

        #endregion

        #region  Getting information from non - fieldDT (used to move it into fieldDT)

        internal static void updateFieldsTable()
        {
            // Updates the FieldsDT from the other table.
            // Uses "Foundational" methods to put info into FieldsDT.
            // After calling this, we can get all information from FieldsDT

            // First update is_PK and is_DK and is_FK- because these used in innerjoin call below
            foreach (DataRow dr in fieldsDT.Rows)
            {
                field rowField = getFieldValueFieldsDT(dr);  // Each row in fieldsDT is a field

                //Set is_PK 
                field tablePkfield = getTablePrimaryKeyFoundational(rowField.table);
                if (rowField.fieldName == tablePkfield.fieldName )      
                {
                    dr[dr.Table.Columns.IndexOf("is_PK")] = true;
                }

                // Set is_DK 
                if (isDisplayKeyFoundational(rowField.table, rowField.fieldName))
                {
                    dr[dr.Table.Columns.IndexOf("is_DK")] = true;  // Only displays itself
                }

                // Set is_FK + FkRefTable + FkRefCol + 
                if (fieldIsForeignKeyFoundational(rowField))
                {
                    dr[dr.Table.Columns.IndexOf("is_FK")] = true;  // Only displays itself
                    field refField = getForeignTablePrimaryKeyFoundational(rowField);
                    dr[dr.Table.Columns.IndexOf("RefTable")] = refField.table;  // Only displays itself
                    dr[dr.Table.Columns.IndexOf("RefPkColumn")] = refField.fieldName;  // Only displays itself
                }
            }

            //Do DisplayKeyDictionary
            Dictionary<string, List<field>> DisplayFieldDict = new Dictionary<string, List<field>>();
            string lastTableName = string.Empty;
            foreach (DataRow dr in fieldsDT.Rows)
            {
                field rowField = getFieldValueFieldsDT(dr);
                if (rowField.table != lastTableName)  // To save time, if equal, use the last DisplayFieldDict
                {
                    sqlFactory tempSql = new sqlFactory(rowField.table, 1, 200);
                    DisplayFieldDict = tempSql.DisplayFieldsDictionary;
                }
                // Foreign key
                if (DisplayFieldDict.ContainsKey(rowField.fieldName))
                {
                    // DisplayFields
                    List<field> fieldList = DisplayFieldDict[rowField.fieldName];
                    List<string> fieldNameList = new List<string>();
                    foreach (field f in fieldList)   // Could be done with linq
                    {
                        fieldNameList.Add(f.fieldName);
                    }
                    string displayFields = String.Join(",", fieldNameList);
                    dr[dr.Table.Columns.IndexOf("DisplayFields")] = displayFields;
                }
                lastTableName = rowField.table;  // For next time around
            }
        }

        private static bool fieldIsForeignKeyFoundational(field fld)
        {
            DataRow[] dr = dataHelper.foreignKeysDT.Select(string.Format("FkTable = '{0}' AND FkColumn = '{1}'", fld.table, fld.fieldName));
            if (dr.Count() > 0)  // Should be 0 or 1
            {
                return true;
            }
            return false;
        }

        private static field getTablePrimaryKeyFoundational(string table)
        {
            string strSelect = "TableName = '" + table + "' AND is_PK ='True'";
            DataRow dr = dataHelper.indexColumnsDT.Select(strSelect).FirstOrDefault();
            if (dr == null)
            {
                return new field(table, "MissingPrimaryKey", "int", 4);
            }
            string columnName = dr[dr.Table.Columns.IndexOf("ColumnName")].ToString();
            return getFieldValueFieldsDT(table, columnName);
        }

        private static field getForeignTablePrimaryKeyFoundational(field rowfield)
        {
            DataRow dr = dataHelper.foreignKeysDT.Select(string.Format("FkTable = '{0}' AND FkColumn = '{1}'", rowfield.table, rowfield.fieldName)).FirstOrDefault();
            if (dr == null)
            {
                return new field("MissingRefTable", "MissingRefColumn", "int", 4);
            }
            string RefTable = Convert.ToString(dr[dr.Table.Columns.IndexOf("RefTable")]);
            string RefPkColumn = Convert.ToString(dr[dr.Table.Columns.IndexOf("RefPkColumn")]);
            return new field(RefTable, RefPkColumn, "int", 4);
        }

        private static bool isDisplayKeyFoundational(string tableName, string fieldName)  // Note 'private
        {
            // Using this for now - will later change to column in unique index that begins with DK
            if (fieldName.Contains("Name")) { return true; }

            switch (tableName.ToLower())
            {
                case "transcript":
                    switch (fieldName.ToLower())
                    {
                        case "studentid":
                        case "coursetermid":
                            return true;
                        default:
                            return false;
                    }
                case "courseterms":
                    switch (fieldName.ToLower())
                    {
                        case "courseid":
                        case "termid":
                        case "term":
                        case "_group":
                            return true;
                        default:
                            return false;
                    }
                case "students":
                    switch (fieldName.ToLower())
                    {
                        case "studentname":
                        case "estudentname":
                            return true;
                        default:
                            return false;
                    }
                case "terms":
                    switch (fieldName.ToLower())
                    {
                        case "term":
                            return true;
                        default:
                            return false;
                    }
                case "studentdegrees":
                    switch (fieldName.ToLower())
                    {
                        case "studentid":
                        case "degreeid":
                            return true;
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }

        #endregion
    }




}
