using Microsoft.VisualBasic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Windows.Media.AppBroadcasting;

namespace SqlCollegeTranscripts

{
    internal static class MsSql
    {
        internal static string databaseType = "MsSql";
        internal static SqlConnection cn { get; set; }
        internal static SqlDataAdapter currentDA { get; set; }
        internal static SqlDataAdapter extraDA { get; set; }  // Might update, delete so can not reuse until grid closed
        internal static SqlDataAdapter readOnlyDA { get; set; }  // No update of table and so no need to keep adaptar, etc.
       
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
        internal static void SetUpdateCommand(field fieldToSet, DataTable dataTable)
        {
            // Do this once in the program
            string msg = string.Empty;
            // Get data adapter
            SqlDataAdapter da = GetDataAdaptor(dataTable);
            string tableName = fieldToSet.table;
            string fieldName = fieldToSet.fieldName;
            string dbType = fieldToSet.dbType;
            SqlDbType sqlDBType = (SqlDbType)Enum.Parse(typeof(SqlDbType), dbType, true);
            int size = fieldToSet.size;
            string PK = dataHelper.getTablePrimaryKeyField(tableName);
            string sqlUpdate =  String.Format("UPDATE {0} SET {1} = {2} WHERE {3} = {4}", 
                    tableName, fieldName, "@" + fieldName, PK, "@" + PK);
            SqlCommand sqlCmd = new SqlCommand(sqlUpdate, MsSql.cn);
            sqlCmd.Parameters.Add("@" + fieldName, sqlDBType , size, fieldName);
            sqlCmd.Parameters.Add("@" + PK, SqlDbType.Int, size, PK);
            da.UpdateCommand = sqlCmd;
        }

        internal static List<string> defaultConnectionString()
        {
            List<string> defaultStrings = new List<string>();
            defaultStrings.Add("Data Source={0}; initial catalog = {1}; user id = {2}; password = {3}; MultipleActiveResultSets=true");
            defaultStrings.Add("server={0};database={1};Trusted_Connection=True; MultipleActiveResultSets=true");
            return defaultStrings;
            // DESKTOP - 120JH08\SQLEXPRESS
        }

        internal static void initializeDatabaseInformationTables()
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
            dataHelper.foreignKeysDT = new DataTable("foreighKeysDT");
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
            dataHelper.indexesDT = new DataTable("indexesDT");
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
            dataHelper.indexColumnsDT = new DataTable("indexColumnsDT");
            readOnlyDA = new SqlDataAdapter(sqlIndexColumns, cn);
            readOnlyDA.Fill(dataHelper.indexColumnsDT);
            // Tables
            sb.Clear();
            sb.Append("SELECT so.name as TableName , ");
            sb.Append("so.name as TableDisplayName , ");
            sb.Append("st.max_column_id_used as ColNum, ");
            sb.Append("st.create_date as Created, st.modify_date as Modified, 0 as Hidden ");
            sb.Append("FROM sys.objects so inner join sys.tables st on so.object_id = st.object_id ");
            sb.Append("WHERE so.is_ms_shipped <> 1 AND so.type = 'U' and st.lob_data_space_id = 0 ");
            sb.Append("ORDER BY TableName ");
            string sqlTables = sb.ToString();
            dataHelper.tablesDT = new DataTable("tablesDT");
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
            sb.Append("'' as FkRefTable, '' as FkRefCol, '' as DisplayFields, 0 as Width ");
            sb.Append("FROM sys.objects so inner join sys.columns sc on so.object_id = sc.object_id ");
            sb.Append("inner join sys.tables st on so.object_id = st.object_id ");
            sb.Append("WHERE so.is_ms_shipped <> 1 AND so.type = 'U' and st.lob_data_space_id = 0 ");
            string sqlFields = sb.ToString();
            dataHelper.fieldsDT = new DataTable("fieldsDT");
            readOnlyDA = new SqlDataAdapter(sqlFields, cn);
            readOnlyDA.Fill(dataHelper.fieldsDT);
            dataHelper.updateFieldsTable();
        }

        internal static void openConnection(string connectionString)
        {
            if (cn == null)   // ? always true since cn has been closed ?
            {
                cn = new SqlConnection(connectionString);
            }
            if (cn.State != ConnectionState.Open)
            {
                cn.Open();
            }
        }

        internal static void FillDataTable(DataTable dt, string sqlString)
        {
            SqlDataAdapter da = GetDataAdaptor(dt);
            SqlCommand sqlCmd = new SqlCommand(sqlString, cn);
            da.SelectCommand = sqlCmd;
            da.Fill(dt);
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

        internal static int GetRecordCount(string strSql)
        {
            int result = 20;  // Testing
            using (SqlCommand cmd = new SqlCommand(strSql, cn))
            {
                result = (int)cmd.ExecuteScalar();
            }
            return result;
        }
    }
}
