﻿using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace SqlCollegeTranscripts
{
    #region Helper classes - field, where, innerJoin, orderBy, and enum command

    public class field
    {
        public string fieldName { get; set; }
        public string table { get; set; }
        public DbType dbType { get; set; }
        public int size { get; set; }   

        public field(string table, string fieldName, DbType dbType, int size)
        {
            this.table = table;
            this.fieldName = fieldName;
            this.dbType = dbType;
            this.size = size;
            this.displayMember = fieldName;
        }
        public field valueMember { get { return this; }}
        public string displayMember { get; set; }  // Used to display where in combo


    }

    public class where
    {
        public where(field fl, string whereValue)
        {
            this.fl = fl;
            this.whereValue = whereValue;
        }
        public where(string table, string field, string whereValue, DbType dbType, int size)
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
        count
    }

    internal enum ProgramMode
    {
        none,
        view,
        edit,
        add,
        delete,
        merge
    }
    internal enum DbTypeType
    {
        isString,
        isDecimal,
        isInteger,
        isDateTime
    }



    #endregion

    public static class dataHelper
    {
        // Variables
        public static DataTable currentDT { get; set; }   //Data Table that is bound to the datagrid
        public static DataTable tablesDT { get; set; }
        public static DataTable fieldsDT { get; set; }
        public static DataTable foreignKeysDT { get; set; }
        public static DataTable indexesDT { get; set; }
        public static DataTable indexColumnsDT { get; set; }
        public static DataTable extraDT { get; set; }

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
        internal static void initializeDataTables()
        {
            currentDT = new DataTable("currentDT");  // Never use the names, but helpful in debugging
            tablesDT = new DataTable("tablesDT");
            fieldsDT = new DataTable("fieldsDT");
            foreignKeysDT = new DataTable("foreighKeysDT");
            indexesDT = new DataTable("indexesDT");
            indexColumnsDT = new DataTable("indexColumnsDT");
            extraDT = new DataTable("extraDT");
        }

        internal static string QualifiedFieldName(field fld)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[" + fld.table + "]");
            sb.Append(".");
            sb.Append("[" + fld.fieldName + "]");
            return sb.ToString();
        }

        internal static DbType ConvertStringToDbType(string strVarType)
        {
            string strDbType = string.Empty;
            //  strVarType is sqlDbType but could also be mySql - just need to expand cases
            switch (strVarType.ToLower())
            {
                case "bigint":
                    strDbType = "Int64";
                    break;
                case "bit":
                    strDbType = "Boolean";
                    break;
                case "char":
                    strDbType = "AnsiStringFixedLength";
                    break;
                case "float":
                    strDbType = "Double";
                    break;
                case "int":
                    strDbType = "Int32";
                    break;
                case "nchar":
                    strDbType = "StringFixedLength";
                    break;
                case "nvarchar":
                    strDbType = "String";
                    break;
                case "real":
                    strDbType = "Single";
                    break;
                case "smallint":
                    strDbType = "Int16";
                    break;
                case "variant":
                    strDbType = "Object";
                    break;
                case "tinyint":
                    strDbType = "Byte";
                    break;
                case "uniqueidentifier":
                    strDbType = "Guid";
                    break;
                case "varbinary":
                    strDbType = "Binary";
                    break;
                case "varchar":
                    strDbType = "AnsiString";
                    break;
                default:
                    strDbType = strVarType;   // SqlDbType and DbType have same name
                    break;
            }
            DbType dbType = (DbType)Enum.Parse(typeof(DbType), strDbType, true);
            return dbType;
        }

        internal static DbTypeType GetDbTypeType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Byte:
                    return DbTypeType.isInteger;
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Single:
                    return DbTypeType.isDecimal;
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.Date:
                case DbType.Time:
                    return DbTypeType.isDateTime;
            }
            return DbTypeType.isString;  // Not checking anything else
        }

        internal static bool TryParseToDbType(string str, DbType dbType, Form form, out string message)
        {
            message = String.Empty;
            switch(dbType)
            {
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Byte:
                    int i;
                    if (int.TryParse(str, out i))
                    {
                        return true;
                    }
                    else
                    {
                        message = String.Format(MultiLingual.tr("Unable to parse '{0}' as {1}.", form),str,"integer");
                        return false;
                    }
                case DbType.Decimal: 
                case DbType.Double:
                case DbType.Single:
                    Decimal dec;
                    if(Decimal.TryParse(str, out dec))
                    {
                        return true;
                    }
                    else
                    {
                        message = String.Format(MultiLingual.tr("Unable to parse {0} as {1}.", form), str, "decimal");
                        return false;
                    }
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.Date:
                case DbType.Time:
                    DateTime dt;
                    if (DateTime.TryParse(str, out dt))
                    {
                        return true;
                    }
                    else
                    {
                        message = String.Format(MultiLingual.tr("Unable to parse {0} as {1}.", form), str, "Date / Time");
                        return false;
                    }
            }
            return true;  // Not checking anything else
        }



        #region  Get values from FieldsDT DataRow (&& overloads)

        internal static string getStringValueFromFieldsDT(DataRow dr, string fieldToReturn)
        {
            return Convert.ToString(dr[dr.Table.Columns.IndexOf(fieldToReturn)]);
        }

        internal static int getIntValueFromFieldsDT(DataRow dr, string fieldToReturn)
        {
            return Convert.ToInt32(dr[dr.Table.Columns.IndexOf(fieldToReturn)]);
        }

        internal static bool getBoolValueFromFieldsDT(DataRow dr, string fieldToReturn)
        {
            int returnField = dr.Table.Columns.IndexOf(fieldToReturn);
            return Convert.ToBoolean(dr[returnField]);
        }

        internal static bool isDisplayKey(string tableName, string columnName)
        {
            return dataHelper.getBoolValueFieldsDT(tableName, columnName, "is_DK");
        }


        // Begin overloads  -- Gets DataRow and then use above methods

        internal static DataRow getDataRowFromFieldsDT(string tableName, string columnName)
        {
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("TableName = '{0}' AND ColumnName = '{1}'", tableName, columnName)).FirstOrDefault();
            return dr;
        }

        internal static field getFieldFromFieldsDT(string tableName, string columnName)
        {
            DataRow dr = getDataRowFromFieldsDT(tableName, columnName);
            return getFieldFromFieldsDT(dr);
        }
        internal static field getFieldFromFieldsDT(DataRow dr)
        {
            string tableName = getStringValueFromFieldsDT(dr, "TableName");
            string columnName = getStringValueFromFieldsDT(dr, "ColumnName");
            int size = getIntValueFromFieldsDT(dr, "MaxLength"); ;
            string strDbType = getStringValueFromFieldsDT(dr, "DataType");
            DbType dbType = dataHelper.ConvertStringToDbType(strDbType);
            return new field(tableName, columnName, dbType, size);
        }


        internal static string getStringValueFieldsDT(string tableName, string columnName, string columnToReturn)
        {
            DataRow dr = getDataRowFromFieldsDT(tableName, columnName);
            return getStringValueFromFieldsDT(dr,columnToReturn);
        }

        internal static int getIntValueFieldsDT(string tableName, string columnName, string columnToReturn)
        {
            DataRow dr = getDataRowFromFieldsDT(tableName,columnName);
            return getIntValueFromFieldsDT(dr,columnToReturn);
        }
 
        internal static bool getBoolValueFieldsDT(string tableName, string columnName, string columnToReturn)
        {
            DataRow dr = getDataRowFromFieldsDT(tableName, columnName);
            return getBoolValueFromFieldsDT(dr,columnToReturn);
        }

        internal static field getFieldFromTableAndColumnName(string tableName, string columnName)
        {
            string strDbType = getStringValueFieldsDT(tableName, columnName, "DataType");
            DbType dbType = dataHelper.ConvertStringToDbType(strDbType);
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
            string FkRefTable = getStringValueFromFieldsDT(dr, "FkRefTable");
            string FkRefCol = getStringValueFromFieldsDT(dr, "FkRefCol");
            return getFieldFromTableAndColumnName(FkRefTable,FkRefCol);
        }
  
        internal static field getForeignKeyRefField(field foreignKey)
        {   // Assumes we have checked that this row in the FieldsDT is a foreignkey
            DataRow dr = getDataRowFromFieldsDT(foreignKey.table,foreignKey.fieldName);
            string FkRefTable = getStringValueFromFieldsDT(dr, "RefTable");
            string FkRefCol = getStringValueFromFieldsDT(dr, "RefPkColumn");
            return getFieldFromTableAndColumnName(FkRefTable, FkRefCol);
        }

        internal static field getTablePrimaryKeyField(string table)
        {
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("TableName = '{0}' AND is_PK", table)).FirstOrDefault();
            return getFieldFromFieldsDT(dr);
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
                field rowField = getFieldFromFieldsDT(dr);  // Each row in fieldsDT is a field

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
                field rowField = getFieldFromFieldsDT(dr);
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
                return new field(table, "MissingPrimaryKey", DbType.Int32, 4);
            }
            string columnName = dr[dr.Table.Columns.IndexOf("ColumnName")].ToString();
            return getFieldFromFieldsDT(table, columnName);
        }

        private static field getForeignTablePrimaryKeyFoundational(field rowfield)
        {
            DataRow dr = dataHelper.foreignKeysDT.Select(string.Format("FkTable = '{0}' AND FkColumn = '{1}'", rowfield.table, rowfield.fieldName)).FirstOrDefault();
            if (dr == null)
            {
                return new field("MissingRefTable", "MissingRefColumn", DbType.Int32, 4);
            }
            string RefTable = Convert.ToString(dr[dr.Table.Columns.IndexOf("RefTable")]);
            string RefPkColumn = Convert.ToString(dr[dr.Table.Columns.IndexOf("RefPkColumn")]);
            return new field(RefTable, RefPkColumn, DbType.Int32, 4);
        }

        private static bool isDisplayKeyFoundational(string tableName, string fieldName)  // Note 'private
        {
            switch (tableName.ToLower())
            {
                case "courses":
                    switch(fieldName.ToLower()) 
                    {
                        case "departmentid":
                        case "coursename":
                            return true;
                        default: 
                            return false;
                    }
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
                case "faculty":
                    switch (fieldName.ToLower())
                    {
                        case "facultyname":
                        case "efacultyname":
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
