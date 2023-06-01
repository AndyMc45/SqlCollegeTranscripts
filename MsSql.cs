using Microsoft.VisualBasic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SqlCollegeTranscripts

{
    internal static class MsSql
    {
        // Properties
        internal static string databaseType = "MsSql";
        internal static string trueString = "True";
        internal static string falseString = "False";
        // Two connections
        internal static SqlConnection cn { get; set; }
        internal static SqlConnection noDatabaseConnection { get; set; }
        // Three sql dataAdapters
        internal static SqlDataAdapter currentDA { get; set; }
        // When using extraDT, be careful not to change it before executing any DA command on it.
        internal static SqlDataAdapter extraDA { get; set; } 
        // Used for all datatables that don't have own DataAdaptor - see "GetDataAdaptor" below
        internal static SqlDataAdapter readOnlyDA { get; set; }  // No update of table and so no need to keep adaptar, etc.

        // Methods
        private static SqlDataAdapter GetDataAdaptor(DataTable dataTable)
        {
            if(dataTable == dataHelper.currentDT)
            {
                if(currentDA == null) { currentDA = new SqlDataAdapter(); }
                return currentDA;
            }
            else if(dataTable == dataHelper.extraDT)
            {
                if (extraDA == null) { extraDA = new SqlDataAdapter(); }
                return extraDA;
            }
            else
            {
                if (readOnlyDA == null) { readOnlyDA = new SqlDataAdapter(); }
                return readOnlyDA;   // Returns null
            }
        }

        // Set update command - only one set field and the where is for PK=@PK - i.e. only one row
        internal static void SetUpdateCommand(List<field> fieldsToSet, DataTable dataTable)
        {
            if (fieldsToSet.Count > 0)  // Should always be true
            { 
                // Get data adapter
                SqlDataAdapter da = GetDataAdaptor(dataTable);
                SqlCommand sqlCmd = new SqlCommand();
                // Get primary key field and add it as parameter
                field pkFld = dataHelper.getTablePrimaryKeyField(fieldsToSet[0].table);
                string PK = pkFld.fieldName;
                sqlCmd.Parameters.Add("@" + PK, SqlDbType.Int, 4, PK);
                // Get update query String
                List<string> setList = new List<string>();
                foreach (field fieldToSet in fieldsToSet ) 
                { 
                    SqlDbType sqlDbType = GetSqlDbType(fieldToSet.dbType);
                    int size = fieldToSet.size;
                    setList.Add(String.Format("{0} = @{1}", fieldToSet.fieldName, fieldToSet.fieldName));
                    sqlCmd.Parameters.Add("@" + fieldToSet.fieldName, sqlDbType, size, fieldToSet.fieldName);
                }
                string sqlUpdate = String.Format("UPDATE {0} SET {1} WHERE {2} = {3}", fieldsToSet[0].table, String.Join(",", setList), PK, "@" + PK);
                sqlCmd.CommandText = sqlUpdate;
                sqlCmd.Connection = MsSql.cn;
                da.UpdateCommand = sqlCmd;
            }
        }

        internal static void SetDeleteCommand(string tableName, DataTable dataTable)
        {
            // Do this once in the program
            string msg = string.Empty;
            // Get data adapter
            SqlDataAdapter da = GetDataAdaptor(dataTable);
            string PK = dataHelper.getTablePrimaryKeyField(tableName).fieldName;
            string sqlUpdate = String.Format("DELETE FROM {0} WHERE {1} = {2}", tableName, PK, "@" + PK);
            SqlCommand sqlCmd = new SqlCommand(sqlUpdate, MsSql.cn);
            sqlCmd.Parameters.Add("@" + PK, SqlDbType.Int, 4, PK);
            da.DeleteCommand = sqlCmd;
        }

        // Set insert command - one parameter for each enabled combo field
        internal static void SetInsertCommand(string tableName, List<where> whereList, DataTable dataTable)
        {
            if(whereList.Count == 0)
            {
                SqlDataAdapter da = GetDataAdaptor(dataTable);
                string sqlString = String.Format("INSERT INTO {0} DEFAULT VALUES", tableName);
                SqlCommand sqlCmd = new SqlCommand(sqlString, MsSql.cn);
                da.InsertCommand = sqlCmd;
            }
            else
            { 
                // Get data adapter
                SqlDataAdapter da = GetDataAdaptor(dataTable);

                // Get the insert command and attach to adapter
                StringBuilder sb = new StringBuilder();
                sb.Append("INSERT INTO ");
                sb.Append(tableName + " (");
                List<string> strFieldList = new List<string>(); 
                foreach(where wh in whereList) 
                {
                    strFieldList.Add(wh.fl.fieldName);
                }
                string strFieldNames = String.Join(", ",strFieldList);
                sb.Append(strFieldNames);
                sb.Append(") VALUES (");
                string strParamFieldNames = "@" + String.Join(", @", strFieldList);
                sb.Append(strParamFieldNames + ")");
                SqlCommand sqlCmd = new SqlCommand(sb.ToString(), MsSql.cn);

                // Add parameters
                foreach (where wh in whereList)
                {
                    SqlDbType sqlDbType = GetSqlDbType(wh.fl.dbType);
                    sqlCmd.Parameters.Add("@" + wh.fl.fieldName, sqlDbType, wh.fl.size, wh.fl.fieldName).Value = wh.whereValue;
                }
                da.InsertCommand = sqlCmd;
            }
        }

        internal static SqlDbType GetSqlDbType(DbType dbType)
        {
            string strDbType = dbType.ToString();
            string strSqlDbType = string.Empty;
            switch (strDbType.ToLower())
            {
                case "int64" : 
                    strSqlDbType = "BigInt";
                    break;
                case "boolean" :
                    strSqlDbType = "Bit";
                    break;
                case "ansistringfixedlength" :
                    strSqlDbType = "Char";
                    break;
                case "double" :
                    strSqlDbType = "Float";
                    break;
                case "int32" :
                    strSqlDbType = "Int";
                    break;
                case "stringfixedlength" :
                    strSqlDbType = "NChar";
                    break;
                case "string" :
                    strSqlDbType = "NVarChar";
                    break;
                case "single" :
                    strSqlDbType = "Real";
                    break;
                case "int16" :
                    strSqlDbType = "SmallInt";
                    break;
                case "object" :
                    strSqlDbType = "Variant";
                    break;
                case "byte" :
                    strSqlDbType = "TinyInt";
                    break;
                case "guid" :
                    strSqlDbType = "UniqueIdentifier";
                    break;
                case "binary" :
                    strSqlDbType = "VarBinary";
                    break;
                case "ansistring" :
                    strSqlDbType = "VarChar";
                    break;
                default:
                    strSqlDbType = strDbType;
                    break;
            }
            SqlDbType sqlDbType = (SqlDbType)Enum.Parse(typeof(SqlDbType), strSqlDbType, true);
            return sqlDbType;
        }

        internal static List<string> defaultConnectionString()
        {
            List<string> defaultStrings = new List<string>();
            defaultStrings.Add("Data Source={0}; initial catalog = {1}; user id = {2}; password = {3}; MultipleActiveResultSets=true");
            defaultStrings.Add("server={0};database={1};Trusted_Connection=True; MultipleActiveResultSets=true");
            return defaultStrings;
            // DESKTOP - 120JH08\SQLEXPRESS
        }

        internal static void openConnection(string connectionString)
        {
            if (cn == null)   // may be false if using frmConnection
            {
                cn = new SqlConnection(connectionString);
            }
            if (cn.State != ConnectionState.Open)
            {
                cn.Open();
            }
        }
        
        internal static void openNoDatabaseConnection(string connectionString)
        {
            if (noDatabaseConnection == null)   // may be false if using frmConnection
            {
                noDatabaseConnection = new SqlConnection(connectionString);
            }
            if (noDatabaseConnection.State != ConnectionState.Open)
            {
                noDatabaseConnection.Open();
            }
        }

        internal static string FillDataTable(DataTable dt, string sqlString)
        {
            SqlDataAdapter da = GetDataAdaptor(dt);
            da = new SqlDataAdapter();  //I guess
            SqlCommand sqlCmd = new SqlCommand(sqlString, cn);
            da.SelectCommand = sqlCmd;
            try
            {
                da.Fill(dt);
                return string.Empty;
            }
            catch(Exception ex)
            {
                return ex.Message + "  SqlString: " + sqlString; 

            }
        }

        internal static void CloseConnectionAndDataAdapters()
        {
            currentDA = null;
            extraDA = null;
            readOnlyDA = null;
            if (cn != null)
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
            }
            cn = null;
        }

        internal static void CloseNoDatabaseConnection()
        {
            if (noDatabaseConnection != null)
            {
                if (noDatabaseConnection.State == ConnectionState.Open)
                {
                    noDatabaseConnection.Close();
                }
            }
            noDatabaseConnection = null;
        }

        internal static int GetRecordCount(string strSql)
        {
            int result = 0; 
            using (SqlCommand cmd = new SqlCommand(strSql, cn))
            {
                result = (int)cmd.ExecuteScalar();
            }
            return result;
        }

        internal static string initializeDatabaseInformationTables()
        {
            // foreignKeysDT
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT sfk.name, OBJECT_NAME(sfk.parent_object_id) FkTable, ");
            sb.Append("COL_NAME(sfkc.parent_object_id, sfkc.parent_column_id) FkColumn, ");
            sb.Append("OBJECT_NAME(sfk.referenced_object_id) RefTable, ");
            sb.Append("COL_NAME(sfkc.referenced_object_id, sfkc.referenced_column_id) RefPkColumn ");
            sb.Append("FROM  sys.foreign_keys sfk INNER JOIN sys.foreign_key_columns sfkc ");
            sb.Append("ON sfk.OBJECT_ID = sfkc.constraint_object_id ");
            sb.Append("INNER JOIN sys.tables t ON t.OBJECT_ID = sfkc.referenced_object_id");
            string sqlForeignKeys = sb.ToString();
            readOnlyDA = new SqlDataAdapter(sqlForeignKeys, cn);
            readOnlyDA.Fill(dataHelper.foreignKeysDT);

            // Indexes - note: si.index_id = 0 if index is a heap (no columns)
            sb.Clear();
            sb.Append("select OBJECT_NAME(so.object_id) as TableName, si.[name] as IndexName, ");
            sb.Append("si.is_primary_key as is_PK, si.is_unique as _unique, ");
            sb.Append("(SELECT COUNT(*) FROM sys.index_columns sic ");
            sb.Append("WHERE si.object_id = sic.object_id AND si.index_id = sic.index_id ) as ColCount ");
            sb.Append("from sys.objects so  ");
            sb.Append("inner join sys.indexes si on so.object_id = si.object_id ");
            sb.Append("where so.is_ms_shipped <> 1 AND so.type = 'U' AND si.index_id > 0 ");
            sb.Append("order by TableName ");
            string sqlIndexes = sb.ToString();
            readOnlyDA = new SqlDataAdapter(sqlIndexes, cn);
            readOnlyDA.Fill(dataHelper.indexesDT);

            // IndexColumns
            sb.Clear();
            sb.Append("SELECT OBJECT_NAME(so.object_id) as TableName, si.name as IndexName, COL_NAME(si.object_id, sic.column_id) as ColumnName, ");
            sb.Append("si.is_primary_key as is_PK, si.is_unique as _unique ");
            sb.Append("FROM sys.objects so ");
            sb.Append("inner join sys.indexes si on so.object_id = si.object_id ");
            sb.Append("inner join sys.index_columns sic on si.object_id = sic.object_id AND si.index_id = sic.index_id ");
            sb.Append("WHERE so.is_ms_shipped <> 1 and so.type = 'U' AND si.index_id > 0 ");
            sb.Append("ORDER BY TableName, is_PK desc, IndexName ");
            string sqlIndexColumns = sb.ToString();
            readOnlyDA = new SqlDataAdapter(sqlIndexColumns, cn);
            readOnlyDA.Fill(dataHelper.indexColumnsDT);

            // Tables
            sb.Clear();
            sb.Append("SELECT so.name as TableName , ");
            sb.Append("so.name as TableDisplayName , ");
            sb.Append("st.max_column_id_used as ColNum, ");
            sb.Append("st.create_date as Created, st.modify_date as Modified, '' as DK_Index, 0 as Hidden ");
            sb.Append("FROM sys.objects so inner join sys.tables st on so.object_id = st.object_id ");
            sb.Append("WHERE so.is_ms_shipped <> 1 AND so.type = 'U' and st.lob_data_space_id = 0 ");
            sb.Append("ORDER BY TableName ");
            string sqlTables = sb.ToString();
            readOnlyDA = new SqlDataAdapter(sqlTables, cn);
            readOnlyDA.Fill(dataHelper.tablesDT);

            // Fields - do this last
            sb.Clear();
            sb.Append("SELECT so.name as TableName, ");
            sb.Append("sc.column_id as ColNum,  ");
            sb.Append("Col_Name(so.object_id, sc.column_id) as ColumnName, ");
            sb.Append("Col_Name(so.object_id, sc.column_id) as ColumnDisplayName, ");
            sb.Append("TYPE_NAME(sc.system_type_id) as DataType, ");
            sb.Append("sc.is_nullable as Nullable, ");
            sb.Append("sc.is_identity as _identity, ");
            sb.Append("CAST('0' as bit) as is_PK, CAST('0' as bit) as is_FK, CAST('0' as bit) as is_DK, ");
            sb.Append("sc.max_length as MaxLength, ");
            sb.Append("'' as RefTable, '' as RefPkColumn, '' as DisplayFields, 0 as Width ");
            sb.Append("FROM sys.objects so inner join sys.columns sc on so.object_id = sc.object_id ");
            sb.Append("inner join sys.tables st on so.object_id = st.object_id ");
            sb.Append("WHERE so.is_ms_shipped <> 1 AND so.type = 'U' and st.lob_data_space_id = 0 ");
            sb.Append("ORDER BY ColNum ");
            string sqlFields = sb.ToString();
            readOnlyDA = new SqlDataAdapter(sqlFields, cn);
            readOnlyDA.Fill(dataHelper.fieldsDT);
            string errorNotice = String.Empty;
            errorNotice = dataHelper.updateTablesDTtableOnProgramLoad(); // fills in "DK_Index"
            dataHelper.updateFieldsDTtableOnProgramLoad(); // fills in many columns that are used in the rest of the program
            return errorNotice;
        }

        // Copied from web.  Point: SqlParameter implements this conversion
        internal static DbType GetDbType(Type runtimeType)
        {
            var nonNullableType = Nullable.GetUnderlyingType(runtimeType);
            if (nonNullableType != null)
            {
                runtimeType = nonNullableType;
            }

            var templateValue = (Object)null;
            if (runtimeType.IsClass == false)
            {
                templateValue = Activator.CreateInstance(runtimeType);
            }

            var sqlParamter = new SqlParameter(parameterName: String.Empty, value: templateValue);

            return sqlParamter.DbType;
        }

        internal static string DeleteRowsFromDT(DataTable dt, where wh )   // Doing 1 where only, usually the PK & value
        {
            try
            {
                DataRow[] drs = dt.Select(string.Format("{0} = {1}", wh.fl.fieldName, wh.whereValue));
                foreach (DataRow dr in drs) 
                { 
                    // Delete datarow from dataTable
                    dr.Delete();
                }
                DataRow[] drArray = new DataRow[drs.Count()];
                for (int i = 0; i < drArray.Length; i++)
                { 
                    drArray[i] = drs[i];
                }
                // Delete Command (set above) uses PK field of each dr to delete - should be first column of dt
                // Again, the dt might have inner joins but these are ignored.  Delete acts on the PK of main table.
                // A pledge - the dataTable must have an adaptor and its deleteCommand must be set
                MsSql.GetDataAdaptor(dt).Update(drArray);  
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
                Console.Beep();
            }
        }

        internal static int ExecuteIntScalar(string sqlString)
        {
            SqlCommand cmd = new SqlCommand(sqlString,cn);
            int value = (int) cmd.ExecuteScalar();
            return value;
        }

    }
}
