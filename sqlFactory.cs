// using Microsoft.Office.Interop.Word;
using Microsoft.VisualBasic;
using System.Data;
using System.Text;

namespace SqlCollegeTranscripts
{
    //internal partial class DataGridViewForm : Form
    internal class sqlFactory
    {
        //Constructor
        internal sqlFactory(string table, int page, int pageSize)
        {
            myTable = table;
            myPage = page;
            myPageSize = pageSize;
            
            // Put Primary key of main table in the first field of myFields
            string pk = dataHelper.getTablePrimaryKeyField(myTable).fieldName;
            field fi = new field(myTable, pk, DbType.Int32, 4);
            myFields.Add(fi);

            // This sets currentSql table and field strings - and these remain the same for this table.
            // This also sets DisplayFieldDicitionary each foreign table key in main table
            errorMsg = callInnerJoins();
        }

        #region Variables
        //The table and job for this sql - myJob will by "" or "combo"
        internal string errorMsg = String.Empty;
        internal string myTable = "";
        internal int myPage = 0;  // Asks for all records, 1 is first page
        internal int myPageSize;  // Set by constructor
        internal int TotalPages { get; set; } // Set when you set record count
        private int recordCount = 0;
        internal int RecordCount
        {
            get { return recordCount; }
            set
            {
                recordCount = value;
                TotalPages = (int)Math.Ceiling((decimal)recordCount / myPageSize);
            }
        }

        // Four lists to build, and SQL will be built from these
        // myInnerJoins and myFields remain the same for a given table & job		
        internal List<innerJoin> myInnerJoins = new List<innerJoin>();
        internal List<orderBy> myOrderBys = new List<orderBy>();
        internal List<field> myFields = new List<field>();  // Table and field
        internal List<where> myWheres = new List<where>();
        // Fields for combos - main form constructs these from myRepresentativeColumns list for each combo
        // internal List<field>[] myFieldsCombo = new List<field>[8];  // Table and field

        // Dictionary(field --> Field List) - foreign keys of this table mapped to fields to show in combo.  Set by innerjoin call.	
        internal Dictionary<string, List<field>> DisplayFieldsDictionary = new Dictionary<string, List<field>>();
        #endregion

        // The primary function of this class

        internal string returnSql(command cmd)
        {
            // The main function of this class - used for tables and combos.
            // Logic: Set 
            int offset = (myPage - 1) * myPageSize;
            string sqlString = "";
            //This exception only for combo's, and class must be immediately destroyed afterwards
            if (cmd == command.count)
            {
                sqlString = "SELECT COUNT(1) FROM " + sqlTableString() + " " + sqlWhereString(false);  // + " " + sqlOrderByStr();
            }
            else if (cmd == command.select)
            {
                if (myPage == -1 || (recordCount <= offset + myPageSize))
                {
                    sqlString = "SELECT " + sqlFieldString(myFields) + " FROM " + sqlTableString() + " " + sqlWhereString(false) + " " + sqlOrderByStr(myOrderBys) + " ";
                }
                else
                { // Sql 2012 required for this "Fetch" clause paging
                    sqlString = "SELECT " + sqlFieldString(myFields) + " FROM " + sqlTableString() + sqlWhereString(false) + sqlOrderByStr(myOrderBys) + " OFFSET " + offset.ToString() + " ROWS FETCH NEXT " + myPageSize.ToString() + " ROWS ONLY";
                }
            }
            return sqlString;
        }

        internal string returnFixDatabaseSql(string strWhere) // Where string passed to this function
        {
            return "SELECT " + sqlFieldString(myFields) + " FROM " + sqlTableString() + " WHERE " + strWhere + " " + sqlOrderByStr(myOrderBys) + " ";
        }

        internal string returnComboSql(field fkColumn)
        {
            string sqlString = "";
            StringBuilder sqlFieldStringSB = new StringBuilder();
            // Get primary key of the combo table - required since this is the value feild
            field FkTablePKField = dataHelper.getForeignKeyRefField(fkColumn);
            // Create display field from fields  Concat_WS(x,y,z) as DisplayField  - should be moved to msSql
            sqlFieldStringSB.Append(dataHelper.QualifiedFieldName(FkTablePKField));
            sqlFieldStringSB.Append(", ");

            List<field> fls = DisplayFieldsDictionary[fkColumn.fieldName];
            string strFields = String.Empty;
            if (fls.Count == 1)
            {
                sqlFieldStringSB.Append(sqlFieldString(fls));
            }
            else
            {
                sqlFieldStringSB.Append("Concat_WS(',',");
                sqlFieldStringSB.Append(sqlFieldString(fls));
                sqlFieldStringSB.Append(")");
            }
            sqlFieldStringSB.Append(" as DisplayField");
            sqlFieldStringSB.Append(", ");
            // Add primary key of table as ValueField (May not need to add this twice but O.K. with Alias 
            sqlFieldStringSB.Append(dataHelper.QualifiedFieldName(FkTablePKField));
            sqlFieldStringSB.Append(" as ValueField");
            sqlString = "SELECT DISTINCT " + sqlFieldStringSB.ToString() + " FROM " + sqlTableString() + " " + sqlWhereString(false) + " Order by DisplayField";
            return sqlString;
        }

        private string sqlTableString()
        {
            string ts = myTable;
            foreach (innerJoin ij in myInnerJoins)
            {
                // Get the 'on' condition
                StringBuilder sb = new StringBuilder();
                sb.Append(dataHelper.QualifiedFieldName(ij.fld));
                sb.Append("=");
                field fld2 = new field(ij.table2, ij.table2PrimaryKey, DbType.Int32, 4);
                sb.Append(dataHelper.QualifiedFieldName(fld2));
                string condition = sb.ToString();

                // construct inner join
                ts = "([" + ij.table2 + "] INNER JOIN " + ts + " ON " + condition + ")";
            }
            return ts;
        }

        private string sqlFieldString(List<field> fieldList)
        {
            // Make a list of the qualified fields, i.e. [table].[field]
            List<string> fieldStrList = new List<string>();
            foreach (field fs in fieldList)
            {
                fieldStrList.Add(dataHelper.QualifiedFieldName(fs));
            }
            // Join with commas - .Join knows to skip a closing comma.
            return String.Join(",", fieldStrList);
        }

        private string sqlWhereString(bool strict)   // Not strict uses "Like" to get strings that start with a string
        {
            // Make a list of the conditions
            List<string> WhereStrList = new List<string>();
            foreach (where ws in myWheres)
            {
                // Get where condition
                string condition = "";
                switch (ws.fl.dbType)
                {
                    case DbType.Int64:
                    case DbType.Int32:
                    case DbType.Int16:
                    case DbType.Decimal:
                    case DbType.Byte:
                    case DbType.Double:
                    case DbType.Single:
                    case DbType.Binary:
                        condition = dataHelper.QualifiedFieldName(ws.fl) + " = " + ws.whereValue;
                        break;
                    case DbType.Date:
                    case DbType.DateTimeOffset:
                    case DbType.DateTime:
                    case DbType.DateTime2:
                    case DbType.Time:
                        condition = dataHelper.QualifiedFieldName(ws.fl) + "= #" + ws.whereValue + "#";
                        break;
                    case DbType.String:
                    case DbType.AnsiString:
                    case DbType.StringFixedLength:
                    case DbType.AnsiStringFixedLength:
                        if(strict)
                        {
                            condition = dataHelper.QualifiedFieldName(ws.fl) + " '" + ws.whereValue + "'";  //Exact string
                        }
                        else
                        { 
                            condition = dataHelper.QualifiedFieldName(ws.fl) + " Like '" + ws.whereValue + "%'";  //Same starting string
                        }
                        break;
                    case DbType.Boolean:
                        if (ws.whereValue.ToLower() == "true")
                        {
                            condition = dataHelper.QualifiedFieldName(ws.fl) + " = '" + MsSql.trueString + "'";
                        }
                        else
                        {
                            condition = dataHelper.QualifiedFieldName(ws.fl) + " = '" + MsSql.falseString + "'";
                        }
                        break;
                }
                if (condition != "")
                {
                    WhereStrList.Add(condition);
                }
            }
            // Use list of conditions to get sql where clause.
            if (WhereStrList.Count > 0)
            {
                // Return string constructed from list of conditions
                return " WHERE " + String.Join(" AND ", WhereStrList);
            }
            else
            {
                return string.Empty;   // No where conditions
            }
        }

        private string sqlOrderByStr(List<orderBy> orderByList)
        {
            if (orderByList.Count == 0) { return ""; }
            //Make a list of order by clauses
            List<string> orderByStrList = new List<string>();
            foreach (orderBy ob in orderByList)
            {
                string qualFieldName = dataHelper.QualifiedFieldName(ob.fld);
                if (ob.sortOrder == System.Windows.Forms.SortOrder.Descending)
                {
                    orderByStrList.Add(qualFieldName + " DESC");
                }
                else
                {
                    orderByStrList.Add(qualFieldName + " ASC");
                }
            }
            // Return string constructed by list of order by clauses
            return " ORDER BY " + String.Join(",", orderByStrList);
        }

        // Go through table and adds to myInnerJoins and myFields lists
        // Note - 0 inner joins will add fields from myTable (1st Call)
        // Two ways to call
        private string callInnerJoins()
        {
            field empty = new field("None", "Note", DbType.String, 0);
            string msg = callInnerJoins(myTable, empty);
            return msg;
        }
        private string callInnerJoins(string currentTable, field myTableInnerJoinField)
        {
            // (myTableInnerJoinField used in constructing the DisplayFieldsDictionary
            // - i.e. a map from top table FK's to all displaykeys that come from it including grandson tables)
      
            StringBuilder MsgStr = new StringBuilder();
            DataRow[] drs = dataHelper.fieldsDT.Select("TableName = '" + currentTable + "'");
            // Loop through fields in table - adding to innerjoins and fields lists
            foreach (DataRow dr in drs)
            {
                // Get the field for this row - containing table, fieldName, DataType, Size
                field drField = dataHelper.getFieldFromFieldsDT(dr);

                //if Primary Key - program assumes this will be the first field
                if (dataHelper.isTablePrimaryKeyField(drField))
                {
                    // Don't add primary key to fields - but primary key of myTable added in DataGridViewForm.cs call
                    // Also foreign keys are added - so no need to add primary key of son tables
                }

                // if Foreign Key
                else if (dataHelper.fieldIsForeignKey(drField))  // Inner join
                {
                    field RefTableField = dataHelper.getForeignKeyRefField(drField);
                    string table2 = RefTableField.table;
                    // Handle circles (don't allow) and repeats (needs an alias)
                    int alliasCount = 0;
                    bool everythingOK = true;
                    foreach (innerJoin ij in myInnerJoins)
                    {
                        // Don't allow circular  Table1 --> Table2 --> Table1
                        if (ij.fld.table == table2)
                        {
                            // msgStr.Append(translation.tr("WarningTwoInnerJoinsOnTable", table2, "", ""));
                            MsgStr.Append("Circular innerjoin: " + currentTable);
                        }
                        // Requires allias - i tells you how many times this table has been an alliaas
                        // If Table1 -->Table2, Table1 --> Table2, Table1 -->Table3, then i=2
                        if (ij.table2 == table2)
                        {
                            alliasCount++;
                            everythingOK = false;
                            MsgStr.Append("Alias: " + table2 + alliasCount.ToString());
                        }
                    }
                    innerJoin new_ij = new innerJoin(drField, table2);
                    if (everythingOK)
                    {
                        // Following not yet implemented - Allias name ignored as of now
                        if (alliasCount > 0)
                        {
                            new_ij.table2Allias = table2 + alliasCount.ToString();
                        }
                        // Add to inner joins
                        myInnerJoins.Add(new_ij);

                        //Recursive step - table 2 becomes table 1
                        if (currentTable == myTable)
                        {
                            // Add this FK field of the main table - needed when editing table
                            myFields.Add(drField);
                            // Add the display keys of the lower tables for all FK in myTable
                            callInnerJoins(table2, drField);  // field1 will remain even for grandson tables
                        }
                        else if (dataHelper.isDisplayKey(drField.table, drField.fieldName))
                        {
                            // For currentTable other than myTable, only add the lower display keys
                            // if this FK is a display key (i.e. such as in "marraige table with only person 1 and person2 FK's
                            callInnerJoins(table2, myTableInnerJoinField);   //Maintains higher level "field1" to any grandsons.
                        }
                    }
                }

                // A non-Key (Only these are added to myFields)
                else  // A none key field
                {
                    if (currentTable == myTable)  // In My Table
                    {
                        myFields.Add(drField);
                    }
                    else if (dataHelper.isDisplayKey(currentTable, drField.fieldName))  // looping through a son of myTable
                    {
                        myFields.Add(drField);
                        //Add fi to the DisplayFieldsDictionary
                        List<field> fieldList = new List<field>();
                        if (DisplayFieldsDictionary.ContainsKey(myTableInnerJoinField.fieldName))
                        {
                            fieldList = DisplayFieldsDictionary[myTableInnerJoinField.fieldName];
                        }
                        fieldList.Add(drField);
                        DisplayFieldsDictionary[myTableInnerJoinField.fieldName] = fieldList;
                    }
                }

            }
            return MsgStr.ToString();
        }

        internal bool fieldIsInMyFields(string tableName, string fieldName)
        {
            foreach (field fl in myFields)
            {
                if (fl.table == tableName && fl.fieldName == fieldName)
                { return true; }

            }
            return false;
        }

        internal bool TableIsInMyTables(string tableName)
        {
            if (tableName == myTable) { return true; }
            foreach (innerJoin ij in myInnerJoins)
            {
                if (tableName == ij.table2) { return true; }
            }
            return false;
        }

    }
}



//---------------------------------------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------------------------------
//Some notes on sql-------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------------------------------------
//OpenSchema can have 2 variable: adSchemaTables and array(TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME,TABLE_TYPE)
//                                adSchemaColumns and array(TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME,COLUMN_NAME)
//                                adSchemaIndexes and array(TABLE_CATALOG,TABLE_SCHEMA,INDEX_NAME,TYPE,TABLE_NAME)
//In access table_catalog and table_schema are Empty.  An empty table_name returns all tables.




