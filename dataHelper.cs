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
            this.table2PrimaryKey = dataHelper.getTablePrimaryKeyField(table2);
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

        internal static void updateFieldsTable()  // Called once after all system fields loaded
        {
            Dictionary<string, List<field>> DisplayFieldDict = new Dictionary<string, List<field>>();
            string lastTableName = string.Empty;
            foreach (DataRow dr in fieldsDT.Rows)
            {
                //Set is_PK 
                string columnName = getStringValueFieldsDT(dr, "ColumnName");
                string tableName = getStringValueFieldsDT(dr, "TableName");
                string tablePkCol = getTablePrimaryKeyField(tableName);
                if (columnName == tablePkCol)
                {
                    int isPkColumn = dr.Table.Columns.IndexOf("is_PK");
                    dr[isPkColumn] = true;
                }

                //Set DisplayFields + Set is_FK + FkRefTable + FkRefCol + is_DK
                if (tableName != lastTableName)  // To save time, if equal, use the last DisplayFieldDict
                {
                    sqlFactory tempSql = new sqlFactory(tableName, 1, 200);
                    DisplayFieldDict = tempSql.DisplayFieldsDictionary;
                }
                // Foreign key
                if (DisplayFieldDict.ContainsKey(columnName))
                {
                    // Set is_FK
                    int isFkColumn = dr.Table.Columns.IndexOf("is_FK");
                    dr[isFkColumn] = true;
                    // Set FkRefTable + FkRefCol
                    field fi = dataHelper.getForeignTableAndKey(tableName, columnName);
                    int fkRefTableCol = dr.Table.Columns.IndexOf("FkRefTable");
                    dr[fkRefTableCol] = fi.table;
                    int fkRefColCol = dr.Table.Columns.IndexOf("FkRefCol");
                    dr[fkRefColCol] = fi.fieldName;
                    // DisplayFields
                    List<field> fieldList = DisplayFieldDict[columnName];
                    List<string> fieldNameList = new List<string>();
                    foreach (field f in fieldList)   // Could be done with linq
                    {
                        fieldNameList.Add(f.fieldName);
                    }
                    string displayFields = String.Join(",", fieldNameList);
                    int displayFieldsCol = dr.Table.Columns.IndexOf("DisplayFields");
                    dr[displayFieldsCol] = displayFields;
                }

                // Set is_DK - used in program, and so you can update live in fieldsDT table
                if (formLoad_isDisplayKey(tableName, columnName)) // Only use of "isDisplayField
                {
                    int isDisplayKey = dr.Table.Columns.IndexOf("is_DK");
                    dr[isDisplayKey] = true;  // Only displays itself
                }

                lastTableName = tableName;  // For next time around
            }
        }

        #region  Get information from system tables

        internal static bool isDisplayKey(string tableName, string columnName)
        {
            bool result = dataHelper.getBoolValueFieldsDT(tableName, columnName, "is_DK");
            return result;
        }
        internal static bool formLoad_isDisplayKey(string tableName, string fieldName)
        {
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

        internal static field getField(string tableName, string fieldName)
        {
            string dbType = getStringValueFieldsDT(tableName, fieldName, "DataType");
            int size = getIntValueFieldsDT(tableName, fieldName, "MaxLength");

            field fi = new field(tableName, fieldName, dbType, size);
            return fi;
        }

        internal static string getTablePrimaryKeyField(string table)
        {
            // This should return String.Empty if there is none (Don't assume there is)
            string strSelect = "TableName = '" + table + "' AND is_PK ='True'";
            DataRow dr = dataHelper.indexColumnsDT.Select(strSelect).FirstOrDefault();
            if (dr == null) { return string.Empty; }  // Required
            int returnField = dr.Table.Columns.IndexOf("ColumnName");
            return Convert.ToString(dr[returnField]);
        }
        internal static bool isTablePrimaryKeyField(string table, string fld)
        {
            string pk = getTablePrimaryKeyField(table);
            if (pk != String.Empty && pk == fld) { return true; }
            return false;
        }
        internal static bool fieldIsForeignKey(string table, string fld)
        {
            DataRow[] dr = dataHelper.foreignKeysDT.Select(string.Format("FkTable = '{0}' AND FkColumn = '{1}'", table, fld));
            if (dr.Count() > 0)  // Should be 0 or 1
            {
                return true;
            }
            return false;
        }
        internal static field getForeignTableAndKey(string table1, string fld1)
        {
            DataRow dr = dataHelper.foreignKeysDT.Select(string.Format("FkTable = '{0}' AND FkColumn = '{1}'", table1, fld1)).FirstOrDefault();
            if (dr == null)
            {
                return new field("MissingRefTable", "MissingRefColumn", "int", 4);
            }
            int RefTableCol = dr.Table.Columns.IndexOf("RefTable");
            string RefTable = Convert.ToString(dr[RefTableCol]);
            int RefPkColumnCol = dr.Table.Columns.IndexOf("RefPkColumn");
            string RefPkColumn = Convert.ToString(dr[RefPkColumnCol]);
            return new field(RefTable, RefPkColumn, "int", 4);
        }
        internal static string getSqlTrue()
        {
            string result = "";
            //    if (msSql)
            //    {
            result = "'True'";
            //    }
            //    else if (msAccess)
            //    {
            //        result = "True";
            //    }
            return result;
        }
        internal static string getSqlFalse()
        {
            string result = "";
            //    if (msSql)
            //    {
            result = "'False'";
            //    }
            //    else if (msAccess)
            //    {
            //        result = "False";
            //    }
            return result;
            //}

        }
        internal static string getStringValueFieldsDT(string table, string field, string fieldToReturn)
        {
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("TableName = '{0}' AND ColumnName = '{1}'", table, field)).FirstOrDefault();
            int returnField = dr.Table.Columns.IndexOf(fieldToReturn);
            return Convert.ToString(dr[returnField]);
        }
        internal static string getStringValueFieldsDT(DataRow dr, string fieldToReturn)
        {
            int returnField = dr.Table.Columns.IndexOf(fieldToReturn);
            return Convert.ToString(dr[returnField]);
        }

        internal static int getIntValueFieldsDT(string table, string field, string fieldToReturn)
        {
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("TableName = '{0}' AND ColumnName = '{1}'", table, field)).FirstOrDefault();
            int returnField = dr.Table.Columns.IndexOf(fieldToReturn);
            return Convert.ToInt32(dr[fieldToReturn]);
        }
        internal static bool getBoolValueFieldsDT(string table, string field, string fieldToReturn)
        {
            bool result = false;
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("tableName = '{0}' AND ColumnName = '{1}'", table, field)).FirstOrDefault();
            int returnField = dr.Table.Columns.IndexOf(fieldToReturn);
            return Convert.ToBoolean(dr[fieldToReturn]);
        }

        #endregion
    }




}
