using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Globalization;

namespace SqlCollegeTranscripts
{
    #region Helper classes - field, where, innerJoin, orderBy, and enum command

    public class field
    {
        public field(string table, string fieldName, DbType dbType, int size, fieldType fType)
        {
            // Only call this constructor with pseudoFields = true;
            this.table = table;
            this.tableAlias = table + "00";  //Default
            this.fieldName = fieldName;
            this.dbType = dbType;
            this.size = size;
            this.fType = fType;
            this.displayMember = fieldName;  // Default
        }

        public field(string table, string fieldName, DbType dbType, int size):
            this(table, fieldName, dbType, size, fieldType.regular){  }

        public string fieldName { get; set; }
        public string table { get; set; }
        public string tableAlias { get; set; }
        public DbType dbType { get; set; }
        public int size { get; set; }
        public fieldType fType { get; set; }

        public field ValueMember { get { return this; }}  //Field itself - ValueMember used when binding Combos to fields

        private string displayMember;
        public string DisplayMember { 
            set { displayMember = value; } 
            get {
                string lastTwoDigetOfTableAlias = tableAlias.Substring(tableAlias.Length - 2);
                if (fType == fieldType.pseudo)
                {
                    return "PsuedoField";  // Should never see this
                }
                else if (fType == fieldType.longName)
                {
                    return tableAlias + ":" + fieldName;
                }
                else
                {
                    if (dataHelper.isTablePrimaryKeyField(this))
                    {
                        return "PK: " + fieldName;
                    }
                    else if (dataHelper.isForeignKeyField(this))
                    {
                        return "FK: " + fieldName;
                    }
                    else
                    {
                        if (lastTwoDigetOfTableAlias! == "00")
                        {
                            return fieldName;
                        }
                        else
                        {
                            return lastTwoDigetOfTableAlias + ": " + fieldName;
                        }
                    }

                }
            }
        }  // Used to display where in combo

        public bool isSameFieldAs(field fl)
        {   
            if(fl == null) { return false; }
            if (this.fieldName == fl.fieldName && this.table == fl.table && this.tableAlias == fl.tableAlias) { return true; } else { return false; }  
        }

        public bool isSameBaseFieldAs(field fl)
        {
            if (fl == null) { return false; }
            if (this.fieldName == fl.fieldName && this.table == fl.table) { return true; } else { return false; }

        }
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
            this.DisplayMember= whereValue;
        }
  
        public field fl { get; set; }
        public string whereValue { get; set; }

        public string DisplayMember { get; set; }  // Used to display where in combo

        public where ValueMember { get { return this; } }

        public bool isSameWhereAs(where wh)
        {
            if (wh == null) { return false; }
            if (this.fl.isSameFieldAs(wh.fl) && this.whereValue == wh.whereValue) 
            { return true; } else { return false; }
        }
    }

    internal class innerJoin
    {
        internal field fkFld { get; set; }
        internal field pkRefFld { get; set; }
        internal innerJoin(field fkFld, field pkRefField)
        {
            this.fkFld = fkFld;
            this.pkRefFld = pkRefField;
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
        selectAll,
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
        isDateTime,
        isBoolean,
        isBinary
    }

    public enum fieldType
    {
        regular,
        longName,
        pseudo
    }

    public enum comboValueType
    { 
        // This enum is not actually needed, but using it to remember there are four returnComboSql cases
        PK_myTable,
        PK_refTable,
        textField_myTable,
        textField_refTable
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
        public static DataTable editingControlDT { get; set; }

        internal static void initializeDataTables()
        {
            currentDT = new DataTable("currentDT");  // Never use the names, but helpful in debugging
            tablesDT = new DataTable("tablesDT");
            fieldsDT = new DataTable("fieldsDT");
            foreignKeysDT = new DataTable("foreighKeysDT");
            indexesDT = new DataTable("indexesDT");
            indexColumnsDT = new DataTable("indexColumnsDT");
            extraDT = new DataTable("extraDT");
            editingControlDT = new DataTable("editingControlDT");
        }

        internal static string QualifiedFieldName(field fld)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[" + fld.table + "]");
            sb.Append(".");
            sb.Append("[" + fld.fieldName + "]");
            return sb.ToString();
        }

        internal static string QualifiedAliasFieldName(field fld)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[" + fld.tableAlias + "]");
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
                case DbType.Boolean:
                    return DbTypeType.isBoolean;
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Byte:
                case DbType.SByte:   // -127 to 127 - signed byte
                    return DbTypeType.isInteger;
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Single:
                    return DbTypeType.isDecimal;
                case DbType.Date:
                case DbType.DateTimeOffset:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.Time:
                    return DbTypeType.isDateTime;
                case DbType.Binary:    // Not handled in program
                    return DbTypeType.isBinary;
                case DbType.String:
                case DbType.AnsiString:
                case DbType.StringFixedLength:
                case DbType.AnsiStringFixedLength:
                    return DbTypeType.isString;
            }
            return DbTypeType.isString;  // Not checking anything else; just a guess
        }

        internal static string errMsg = String.Empty;
        internal static string errMsgParameter1 = String.Empty;
        internal static string errMsgParameter2 = String.Empty;

        internal static bool TryParseToDbType(string str, DbType dbType)
        {
            DbTypeType dbTypeType = GetDbTypeType(dbType);
            switch(dbTypeType)
            {
                case DbTypeType.isInteger:
                    int i;
                    if (int.TryParse(str, out i))
                    {
                        return true;
                    }
                    else
                    {
                        errMsg = "Unable to parse '{0}' as {1}.";
                        errMsgParameter1 = str;
                        errMsgParameter2 = "integer";
                        return false;
                    }
                case DbTypeType.isDecimal: 
                    Decimal dec;
                    if(Decimal.TryParse(str, NumberStyles.Number ^ NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out dec))
                    {
                        return true;
                    }
                    else
                    {
                        errMsg = "Unable to parse '{0}' as {1}.";
                        errMsgParameter1 = str;
                        errMsgParameter2 = "decimal";
                        return false;
                    }
                case DbTypeType.isDateTime:
                    DateTime dt;
                    if (DateTime.TryParse(str, out dt))
                    {
                        return true;
                    }
                    else
                    {
                        errMsg = "Unable to parse '{0}' as {1}.";
                        errMsgParameter1 = str;
                        errMsgParameter2 = "Date / Time";
                        return false;
                    }
                case DbTypeType.isBoolean:
                    Boolean tf;
                    if (Boolean.TryParse(str, out tf))
                    {
                        return true;
                    }
                    else
                    {
                        errMsg = "Unable to parse '{0}' as {1}.";
                        errMsgParameter1 = str;
                        errMsgParameter2 = "Boolean";
                        return false;
                    }

            }
            return true;  // Not checking anything else - assuming it will be a string
        }

        internal static where GetWhereFromPrimaryKey(string table, string PKvalue)
        {
            field pkField = getTablePrimaryKeyField(table);
            where newWhere = new where(pkField, PKvalue);
            // Get display name 
            sqlFactory sf = new sqlFactory(table, 0, 0);
            sf.myComboWheres.Add(newWhere);
            string strSql = sf.returnComboSql(pkField, comboValueType.PK_myTable);
            dataHelper.extraDT = new DataTable();
            MsSql.FillDataTable(dataHelper.extraDT, strSql);
            string displayMember = "Missing DK";
            if (extraDT.Rows.Count > 0)
            {
                int colIndex = extraDT.Columns["DisplayMember"].Ordinal;
                displayMember = extraDT.Rows[0][colIndex].ToString();
            }
            displayMember = table + ": " + displayMember;
            newWhere.DisplayMember = displayMember;
            return newWhere;
        }

        // Add a "0" to i if it is less than 10.
        internal static string twoDigitNumber(int i)
        {
            if (i < 10)
            {
                return "0" + i.ToString();
            }
            else
            {
                return i.ToString();
            }
        }


        #region  Get values from FieldsDT DataRow (&& overloads)

        private static string getStringValueFromFieldsDT(DataRow dr, string fieldToReturn)
        {
            return Convert.ToString(dr[dr.Table.Columns.IndexOf(fieldToReturn)]);
        }

        private static int getIntValueFromFieldsDT(DataRow dr, string fieldToReturn)
        {
            return Convert.ToInt32(dr[dr.Table.Columns.IndexOf(fieldToReturn)]);
        }

        private static bool getBoolValueFromFieldsDT(DataRow dr, string fieldToReturn)
        {
            int returnField = dr.Table.Columns.IndexOf(fieldToReturn);
            return Convert.ToBoolean(dr[returnField]);
        }

        // Use above private methods to get specific data from a DataRow

        internal static bool isDisplayKey(field fi)
        {
            return dataHelper.getBoolValueFieldsDT(fi.table, fi.fieldName, "is_DK");
        }

        internal static field getFieldFromFieldsDT(string tableName, string columnName)
        {
            DataRow dr = getDataRowFromFieldsDTFoundational(tableName, columnName);
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
            DataRow dr = getDataRowFromFieldsDTFoundational(tableName, columnName);
            return getStringValueFromFieldsDT(dr,columnToReturn);
        }

        internal static int getIntValueFieldsDT(string tableName, string columnName, string columnToReturn)
        {
            DataRow dr = getDataRowFromFieldsDTFoundational(tableName,columnName);
            return getIntValueFromFieldsDT(dr,columnToReturn);
        }
 
        internal static bool getBoolValueFieldsDT(string tableName, string columnName, string columnToReturn)
        {
            DataRow dr = getDataRowFromFieldsDTFoundational(tableName, columnName);
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
 
        internal static bool isForeignKeyField(field columnField)
        {
            return getBoolValueFieldsDT(columnField.table, columnField.fieldName, "is_FK");
        }

        internal static field getForeignKeyRefField(DataRow dr)
        {   // Assumes we have checked that this row in the FieldsDT is a foreignkey
            string FkRefTable = getStringValueFromFieldsDT(dr, "RefTable");
            string FkRefCol = getStringValueFromFieldsDT(dr, "RefPkColumn");
            return getFieldFromTableAndColumnName(FkRefTable,FkRefCol);
        }

        internal static field getForeignKeyRefField(field foreignKey)
        {   // Assumes we have checked that this row in the FieldsDT is a foreignkey
            DataRow dr = getDataRowFromFieldsDTFoundational(foreignKey.table,foreignKey.fieldName);
            string FkRefTable = getStringValueFromFieldsDT(dr, "RefTable");
            string FkRefCol = getStringValueFromFieldsDT(dr, "RefPkColumn");
            return getFieldFromTableAndColumnName(FkRefTable, FkRefCol);
        }

        internal static field getForeignKeyFromRefField(field RefField, string FkTable)
        {   // Assumes we have checked that there is this foreignkey in FkTable
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("TableName = '{0}' AND RefTable = '{1}'", FkTable, RefField.table)).FirstOrDefault();
            return getFieldFromFieldsDT(dr);
        }

        internal static field getTablePrimaryKeyField(string table)
        {
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("TableName = '{0}' AND is_PK", table)).FirstOrDefault();
            return getFieldFromFieldsDT(dr);
        }

        #endregion

        #region  Loading tableDT (DK_Index) and fieldsDT (many columns) on connnection.open from other tables

        internal static string updateTablesDTtableOnProgramLoad()
        {
            List<string> NoDKList = new List<string>();
            List<string> NoPKList = new List<string>();

            // Updates "DK_Index" in tablesDT
            foreach (DataRow dr in tablesDT.Rows)
            {
                // Get DK_Index for this table
                string tableName = dr["TableName"].ToString();

                // PK - used to check errors
                DataRow[] drs2 = dataHelper.indexesDT.Select(string.Format("TableName = '{0}' AND is_PK = 'True'", tableName));
                if (drs2.Count() == 0) { NoPKList.Add(tableName); }


                // DK - used in program as well as to check for errors
                DataRow[] drs = dataHelper.indexesDT.Select(string.Format("TableName = '{0}' AND _unique = 'True' AND is_PK = 'False'", tableName));
                if(drs.Count() == 0) { NoDKList.Add(tableName); }
                string indexName = string.Empty;
                int dkRow = -1;
                foreach (DataRow dr2 in drs)
                {
                    indexName = dr2["IndexName"].ToString();
                    if (indexName.Length > 1)
                    {
                        if (indexName.Substring(0, 2).ToLower() == "dk")
                        {
                            dr["DK_Index"] = indexName;
                            break; // break out of inner for loop
                        }
                    }
                }
                // Found non-primary unique index not starting with "dk"
                if (indexName != string.Empty)
                {
                    dr["DK_Index"] = indexName;
                }
            }
            if(NoDKList.Count > 0 || NoPKList.Count>0)
            { 
            return "No DK: " + String.Join(", ", NoDKList) + " No PK: " + String.Join(", ", NoPKList);
            }
            else { return string.Empty; }
        }

        internal static void updateFieldsDTtableOnProgramLoad()
        {
            // Updates FieldsDT from the other table.
            // Uses "Foundational" methods to put info into FieldsDT.
            // After calling this, we can get all information from FieldsDT
            // First update is_PK, is_DK, is_FK - because these used in innerjoin call below
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
                return new field(table, "MissingPrimaryKey", DbType.Int32, 4, fieldType.pseudo);
            }
            string columnName = dr[dr.Table.Columns.IndexOf("ColumnName")].ToString();
            return getFieldFromFieldsDT(table, columnName);
        }

        private static field getForeignTablePrimaryKeyFoundational(field rowfield)
        {
            DataRow dr = dataHelper.foreignKeysDT.Select(string.Format("FkTable = '{0}' AND FkColumn = '{1}'", rowfield.table, rowfield.fieldName)).FirstOrDefault();
            if (dr == null)
            {
                return new field("MissingRefTable", "MissingRefColumn", DbType.Int32, 4, fieldType.pseudo);
            }
            string RefTable = Convert.ToString(dr[dr.Table.Columns.IndexOf("RefTable")]);
            string RefPkColumn = Convert.ToString(dr[dr.Table.Columns.IndexOf("RefPkColumn")]);
            return new field(RefTable, RefPkColumn, DbType.Int32, 4);
        }

        private static bool isDisplayKeyFoundational(string tableName, string fieldName)  // Note 'private
        {
            DataRow dr = dataHelper.tablesDT.Select(string.Format("TableName = '{0}'",tableName)).FirstOrDefault();
            if (dr != null) // always true 
            {
                string DK_Index = dr["DK_Index"].ToString();
                if (!String.IsNullOrEmpty(DK_Index))
                {
                    // May be two indexes on same column
                    DataRow[] drs = dataHelper.indexColumnsDT.Select(string.Format("TableName = '{0}' AND ColumnName = '{1}'", tableName,fieldName));
                    foreach(DataRow dr2 in drs) 
                    {
                        if (dr2["IndexName"].ToString() == DK_Index)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static DataRow getDataRowFromFieldsDTFoundational(string tableName, string columnName)
        {
            DataRow dr = dataHelper.fieldsDT.Select(string.Format("TableName = '{0}' AND ColumnName = '{1}'", tableName, columnName)).FirstOrDefault();
            return dr;
        }


        #endregion

    }




}
