// using Microsoft.Office.Interop.Word;
using Microsoft.VisualBasic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Web;

namespace SqlCollegeTranscripts
{
    //internal partial class DataGridViewForm : Form
    internal class sqlFactory
    {

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
                if (myPageSize > 0)
                {
                    TotalPages = (int)Math.Ceiling((decimal)recordCount / myPageSize);
                }
            }
        }

        // Four lists to build, and SQL will be built from these
        // myInnerJoins and myFields remain the same for a given table & job		
        internal List<innerJoin> myInnerJoins = new List<innerJoin>();
        internal List<orderBy> myOrderBys = new List<orderBy>();
        internal List<field> myFields = new List<field>();  // Table and field
        internal List<where> myWheres = new List<where>();
        internal List<where> myComboWheres = new List<where>();  // Must be redone for every combo call
        // PKs_OstensiveDictionary used to determine which rows to show user for an key.  (Keys is the PKs of all tables in this sql factory)  
        internal Dictionary<Tuple<string,string, string>, List<field>> PKs_OstensiveDictionary = new Dictionary<Tuple<string, string, string>, List<field>>();
        // Pks_InnerjoinMap used to find sql table string for any key
        internal Dictionary<Tuple<string, string, string>, List<innerJoin>> PKs_InnerjoinMap = new Dictionary<Tuple<string, string, string>, List<innerJoin>>();

        #endregion

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
            errorMsg = addInnerJoins();

            // If there is no ostensive definition for PK or FK, add the primary key of myTable or refTable
            foreach (Tuple<string, string, string> key in PKs_OstensiveDictionary.Keys)
            {
                if (PKs_OstensiveDictionary[key].Count == 0)
                {
                    field check = new field(key.Item2,key.Item3, DbType.Int32, 4);
                    field PK_myTable = dataHelper.getTablePrimaryKeyField(myTable);
                    if (check.isSameFieldAs(PK_myTable))
                    {
                        PKs_OstensiveDictionary[key].Add(PK_myTable);
                    }
                    else  // primary key of ref table - only PK's added to PK_Ostensive - either of myTable or RefTable of FKs
                    {
                        field PK_RefTable = dataHelper.getFieldFromFieldsDT(key.Item2,key.Item3);
                        PK_RefTable.tableAlias = key.Item1;
                        PKs_OstensiveDictionary[key].Add(PK_RefTable);
                    }
                }
            }
        }


        // The primary function of this class - 1 overload
        internal string returnSql(command cmd)
        {
            return returnSql(cmd, false);
        }
        
        internal string returnSql(command cmd, bool strict)
        {
            // The main function of this class - used for tables and combos.
            // Logic: Set 
            int offset = (myPage - 1) * myPageSize;
            string sqlString = "";
            //This exception only for combo's, and class must be immediately destroyed afterwards
            if (cmd == command.count)
            {
                sqlString = "SELECT COUNT(1) FROM " + sqlTableString() + " " + sqlWhereString(myWheres, strict);  // + " " + sqlOrderByStr();
            }
            else if (cmd == command.selectAll || (recordCount <= offset + myPageSize))
            {
                sqlString = "SELECT " + sqlFieldString(myFields) + " FROM " + sqlTableString() + " " + sqlWhereString(myWheres, strict) + " " + sqlOrderByStr(myOrderBys) + " ";
            }
            else if (cmd == command.select)
            { 
                // Sql 2012 required for this "Fetch" clause paging
                sqlString = "SELECT " + sqlFieldString(myFields) + " FROM " + sqlTableString() + sqlWhereString(myWheres, strict) + sqlOrderByStr(myOrderBys) + " OFFSET " + offset.ToString() + " ROWS FETCH NEXT " + myPageSize.ToString() + " ROWS ONLY";
            }

            return sqlString;
        }

        // This factory is still the currentSql factory
        internal string returnComboSql(field cmbField, comboValueType cmbValueType)  // Return all Display keys
        {
            string sqlString = string.Empty;

            // For primary keys, displayMember is the ostensive display keys and value member is the Pk value.
            if (cmbValueType == comboValueType.PK_myTable || cmbValueType == comboValueType.PK_refTable)  // no distinction between these two
            { 
                StringBuilder sqlFieldStringSB = new StringBuilder();
                // Put the cmbField as first field
                Tuple<string, string, string> key = Tuple.Create(cmbField.tableAlias, cmbField.table, cmbField.fieldName);
                List<field> fls = PKs_OstensiveDictionary[key];  // Error if not present
                // If myTable has no display keys, make the primary field the display key
                if (fls.Count == 0)
                {
                    sqlFieldStringSB.Append(dataHelper.QualifiedAliasFieldName(cmbField));
                }
                else if (fls.Count == 1)
                {
                    sqlFieldStringSB.Append(dataHelper.QualifiedAliasFieldName(fls[0]));
                }
                else
                {
                    sqlFieldStringSB.Append("Concat_WS(',',");
                    sqlFieldStringSB.Append(sqlFieldString(fls));  // function converts fls to list of fields seperated by comma
                    sqlFieldStringSB.Append(")");
                }
                sqlFieldStringSB.Append(" as DisplayMember");
                sqlFieldStringSB.Append(", ");
                // Add primary key of table as ValueField (May not need to add this twice but O.K. with Alias) 
                sqlFieldStringSB.Append(dataHelper.QualifiedAliasFieldName(cmbField));
                sqlFieldStringSB.Append(" as ValueMember");
                sqlString = "SELECT DISTINCT " + sqlFieldStringSB.ToString() + " FROM " + sqlTableString(cmbField, key) + " " + sqlWhereString(myComboWheres, false) + " Order by DisplayMember";
            }
            // For text fields return distinct values - used in combo boxes 
            else if (cmbValueType == comboValueType.textField_myTable || cmbValueType == comboValueType.textField_refTable)  // no distinction between these two
            {
                // For non-Keys return distinct values - used in combo boxes 
                StringBuilder sqlFieldStringSB = new StringBuilder();
                sqlFieldStringSB.Append(dataHelper.QualifiedAliasFieldName(cmbField));
                sqlFieldStringSB.Append(" as DisplayMember");
                sqlFieldStringSB.Append(", ");
                // Add primary key of table as ValueField (May not need to add this twice but O.K. with Alias 
                sqlFieldStringSB.Append(dataHelper.QualifiedAliasFieldName(cmbField));
                sqlFieldStringSB.Append(" as ValueMember");
                field PkField = dataHelper.getTablePrimaryKeyField(cmbField.table);
                PkField.tableAlias = cmbField.tableAlias;
                Tuple<string, string, string> key = Tuple.Create(PkField.tableAlias, PkField.table, PkField.fieldName);
                string tableString = sqlTableString(PkField, key);
                //if (dataHelper.isForeignKeyField(cmbField))
                //{
                //    ijList = PKs_InnerjoinMap[Keyfield.fieldName];
                //    field PkField = dataHelper.getForeignKeyRefField(Keyfield);
                //    tableString = sqlTableString(PkField, ijList);
                //}
                sqlString = "SELECT DISTINCT " + sqlFieldStringSB.ToString() + " FROM " + tableString + " " + sqlWhereString(myComboWheres, false) + " Order by DisplayMember";
            }
            return sqlString;
        }

        internal string returnFixDatabaseSql(List<where> whereList) // Where string passed to this function
        {
            string strWhere = sqlWhereString(whereList, true);
            return returnFixDatabaseSql(strWhere);
        }

        internal string returnFixDatabaseSql(string strWhere) // Where string passed to this function
        {
            return "SELECT " + sqlFieldString(myFields) + " FROM " + sqlTableString() + " " + strWhere + " " + sqlOrderByStr(myOrderBys) + " ";
        }

        private string sqlTableString()
        {
            field myPK = dataHelper.getTablePrimaryKeyField(myTable);
            Tuple<string,string,string> key = Tuple.Create(myPK.tableAlias, myPK.table, myPK.fieldName); // Should find all innerjoins
            string ts = sqlTableString(myPK, key);
            return ts;
        }

        private string sqlTableString(field PkField, Tuple<string,string,string> key)
        {
            List<innerJoin> ijList = PKs_InnerjoinMap[key];
            string ts = PkField.table + " AS " + PkField.tableAlias;
            return sqlExtendTableStringByInnerJoins(ijList, ts);
        }

        private string sqlExtendTableStringByInnerJoins(List<innerJoin> ijList, string ts)
        {
            foreach (innerJoin ij in ijList)
            {
                string condition = string.Format(" {0} = {1} ", dataHelper.QualifiedAliasFieldName(ij.fkFld), dataHelper.QualifiedAliasFieldName(ij.pkRefFld));
                ts = "([" + ij.pkRefFld.table + "] AS " + ij.pkRefFld.tableAlias + " INNER JOIN " + ts + " ON " + condition + ")";
                // Recursive step
                Tuple<string, string, string> RefTableKey = Tuple.Create(ij.pkRefFld.tableAlias, ij.pkRefFld.table, ij.pkRefFld.fieldName);
                List<innerJoin> RefTable_ijs = PKs_InnerjoinMap[RefTableKey];
                ts = sqlExtendTableStringByInnerJoins(RefTable_ijs, ts);
            }
            return ts;
        }

        private string sqlFieldString(List<field> fieldList)
        {
            // Make a list of the qualified fields, i.e. [table].[field]
            List<string> fieldStrList = new List<string>();
            foreach (field fs in fieldList)
            {
                fieldStrList.Add(dataHelper.QualifiedAliasFieldName(fs));
            }
            // Join with commas - .Join knows to skip a closing comma.
            return String.Join(",", fieldStrList);
        }

        private string sqlWhereString(List<where> myOrComboWheres, bool strict)   // Not strict uses "Like" to get strings that start with a string
        {
            // Make a list of the conditions
            List<string> WhereStrList = new List<string>();
            foreach (where ws in myOrComboWheres)
            {
                string condition = "";
                if (dataHelper.TryParseToDbType(ws.whereValue,ws.fl.dbType))
                { 
                    DbTypeType dbTypeType = dataHelper.GetDbTypeType(ws.fl.dbType);
                    // Get where condition
                    switch (dbTypeType)
                    {
                        case DbTypeType.isInteger:
                        case DbTypeType.isDecimal:
                            condition = dataHelper.QualifiedAliasFieldName(ws.fl) + " = " + ws.whereValue;
                            break;
                        case DbTypeType.isDateTime:
                            condition = dataHelper.QualifiedAliasFieldName(ws.fl) + "= #" + ws.whereValue + "#";
                            break;
                        case DbTypeType.isString:
                            if (strict)
                            {
                                condition = dataHelper.QualifiedAliasFieldName(ws.fl) + " = '" + ws.whereValue + "'";  //Exact string
                            }
                            else
                            { 
                                condition = dataHelper.QualifiedAliasFieldName(ws.fl) + " Like '" + ws.whereValue + "%'";  //Same starting string
                            }
                            break;
                        case DbTypeType.isBoolean:
                            if (ws.whereValue.ToLower() == "true")
                            {
                                condition = dataHelper.QualifiedAliasFieldName(ws.fl) + " = '" + MsSql.trueString + "'";
                            }
                            else
                            {
                                condition = dataHelper.QualifiedAliasFieldName(ws.fl) + " = '" + MsSql.falseString + "'";
                            }
                            break;
                    }
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
                string qualFieldName = dataHelper.QualifiedAliasFieldName(ob.fld);
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
        // Also, for every table in the sql, add map from its primary key to all its ForeignKeys.  (PKs_InnerjoinMap[PK] --> list of FK inner joins)
        // Also, for every table in the sql, add map from its primary key to all its displayFields (PKs_OstensiveDictionary[PK] --> list of display fields)
        private string addInnerJoins()
        {
            List<string> msgStrings = new List<string>();  // List of error messages.
            List<string> allTables = new List<string>();
            allTables.Add(myTable);  // So next use of myTable will myTable01
            field myTableKeyField = dataHelper.getTablePrimaryKeyField(myTable);
            Tuple<string, string, string> key = Tuple.Create(myTableKeyField.tableAlias, myTableKeyField.table, myTableKeyField.fieldName);
            List<field> myTableDisplayFields = new List<field>();
            PKs_OstensiveDictionary.Add(key, myTableDisplayFields);  // Add myTable key and map to EMPTY list (filled in below)
            List<innerJoin> myTableInnerJoins = new List<innerJoin>();
            PKs_InnerjoinMap.Add(key, myTableInnerJoins);  // add myTable key and map to EMPTY list (filled in below)
            List<string> myTableStack = new List<string>();  // Start empty so that myTable can be added once.
            List<Tuple<string, string, string>> keyStack = new List<Tuple<string, string, string>>();
            keyStack.Add(key);   // keyStack begins with PK of my table
            addInnerJoins(key, myTableKeyField, keyStack, ref allTables, ref msgStrings );
            
            if (msgStrings.Count > 0) 
            {return String.Join(", ", msgStrings); } 
            else { return string.Empty; }
        }

        private void addInnerJoins(Tuple<string, string, string> myTableKey, field currentTablePK, List<Tuple<string, string, string>> keyStack, ref List<string> allTables, ref List<string> msgStrings)
        {
            // CurrentKey is the PK of my table or "RefPK of FK in myTable"
            // currentTablePK is the PK of the table we are looping through - may be 2 or more tables down
            // keyStack is the list of keys all of which will get a lower text DisplayKey in their ostensive definition.
            // keyStack is also used to check for circular display keys - i.e. a vicious circle
            // allTables is all tables that have been added - used to do an allias when table added 2 or more times
            // msgStrings returns error messages

            // Loop through fields in currentTable - adding to myInnerJoins, myFieldList, PK_OstensiveDefinitions, PK_InnerJoinMap plus allTables and msgStrings
            DataRow[] drs = dataHelper.fieldsDT.Select("TableName = '" + currentTablePK.table + "'","ColNum ASC");
            foreach (DataRow dr in drs)
            {
                // Get the field for this row - the same table and tableAlias is used for all columns/fields in this table
                field drField = dataHelper.getFieldFromFieldsDT(dr);
                drField.tableAlias = currentTablePK.tableAlias;

                if (dataHelper.isForeignKeyField(drField))  // Must add Inner join
                {
                    // Get PK of the table - may need to use "01" or greater as table alias   
                    field RefTableField = dataHelper.getForeignKeyRefField(drField);  // Primary key of ref table
                    int i = allTables.Count(e=>e.Equals(RefTableField.table));
                    if (i > 0) 
                    {
                        RefTableField.tableAlias = RefTableField.table + dataHelper.twoDigitNumber(i); 
                    }
                    allTables.Add(RefTableField.table);  // Prepare for next use of refTable if any
                    // Get the key version of this PK
                    Tuple<string, string, string> newKey = Tuple.Create(RefTableField.tableAlias, RefTableField.table, RefTableField.fieldName);
                    // Circular check: if table already in myTableStack, skip this foreign key
                    if (keyStack.Contains(newKey))  // Check that this works for tuples
                    {
                        msgStrings.Append("Skipping circular foreignkey: (" + dataHelper.QualifiedAliasFieldName(drField) + ")");
                    }
                    else // 99.9% of time - 2 cases
                    {
                        // 1. Add to myFields and innerjoins 
                        if (currentTablePK.table == myTable)
                        { 
                            myFields.Add(drField);
                        }
                        innerJoin new_ij = new innerJoin(drField, RefTableField);
                        myInnerJoins.Add(new_ij);

                        // 2. Recursive step (2 Cases): 1. currentTable is myTable and 2. currentTable is a DK.
                        // (Do nothing for non-DK FK in lower tables - these FKs are not shown in grid for myTable)
                        if (currentTablePK.table == myTable)
                        {
                            // 1. Add all myTable FK to PKs_InnerjoinMap[myTableKey]
                            PKs_InnerjoinMap[myTableKey].Add(new_ij);
                            // 2. Recursive step    
                            // 2a. Create empty dictionary items for PKs_Ostensive dictionary and PKs_InnerjoinMap
                            List<field> fiList = new List<field>();
                            PKs_OstensiveDictionary.Add(newKey, fiList);
                            List<innerJoin> ijList = new List<innerJoin>();
                            PKs_InnerjoinMap.Add(newKey, ijList);
                            // 2b. Create new key stack for myTable FKs
                            List<Tuple<string, string, string>> newKeyStack = new List<Tuple<string, string, string>>();
                            if (dataHelper.isDisplayKey(drField))
                            {
                                newKeyStack.Add(myTableKey);
                            }
                            newKeyStack.Add(newKey);   // keyStack begins with RefPK of this FK
                            // 2b2. Recursive call with these three Empty "Lists"
                            addInnerJoins(newKey, RefTableField, newKeyStack, ref allTables, ref msgStrings);
                        }
                        else if ( dataHelper.isDisplayKey(drField) )  // Not my table
                        {
                            // 1. Add to PK_InnerjoinMap
                                PKs_InnerjoinMap[myTableKey].Add(new_ij);  // Previously only myTableKey
                            // 2. Recursive step (2nd Case): a foreign key not in myTable that isDispayKey 
                            // 2a. Create empty dictionary items for PKs_Ostensive dictionary and PKs_InnerjoinMap
                            List<field> fiList = new List<field>();
                            PKs_OstensiveDictionary.Add(newKey, fiList);
                            List<innerJoin> ijList = new List<innerJoin>();
                            PKs_InnerjoinMap.Add(newKey, ijList);
                            // 2b. Add to myTableStack
                            keyStack.Add(newKey);
                            // 2d. Recursive call with old key and tablestack
                            addInnerJoins(myTableKey, RefTableField, keyStack, ref allTables, ref msgStrings);
                        }
                    }
                }
                else if (dataHelper.isTablePrimaryKeyField(drField))
                {
                    // Primary key added in class constructor as first field
                    // Also foreign keys are added as field in myFields - so no need to add primary key of ref tables
                }
                else  // Neither PK or FK
                {
                    if ( currentTablePK.table == myTable || dataHelper.isDisplayKey(drField) )  // Add if myTable and all lower table displaykeys
                    {
                        myFields.Add(drField);
                        if (dataHelper.isDisplayKey(drField))
                        {
                            // Add to PKs_OstensiveDictionary to each key in keyStack
                            foreach (Tuple<string, string, string> key in keyStack)
                            { 
                                PKs_OstensiveDictionary[key].Add(drField);
                            }
                        }
                    }
                }
            }
        }

        internal bool TableIsInMyInnerJoins(field PkField, string tableAliasName)
        {
            if (tableAliasName == PkField.tableAlias)  // Table can be filtered on itself 
            { 
                return true; 
            } 
            Tuple<string, string, string> key = Tuple.Create(PkField.tableAlias, PkField.table, PkField.fieldName);
            foreach (innerJoin ij in PKs_InnerjoinMap[key])
            {
                // Recursive call
                if (TableIsInMyInnerJoins(ij.pkRefFld, tableAliasName))
                {
                    return true;
                }
//                if (tableAliasName == ij.pkRefFld.tableAlias) { return true; }
            }
            return false;
        }

        internal bool MainFilterTableIsInSql(where mainFilter, field PkField, out string tableAlias)
        {
            // Same logic as "TableIsInMyInnerJoin" but will return a tableAlias that matches the mainFilter field
            if (mainFilter.fl.isSameBaseFieldAs(PkField))  // Table itself is the first table in the Sql table string
            { 
                tableAlias = PkField.tableAlias;  
                return true; 
            }
            // Recursive search
            Tuple<string, string, string> key = Tuple.Create(PkField.tableAlias, PkField.table, PkField.fieldName);
            foreach (innerJoin ij in PKs_InnerjoinMap[key])
            {
                if(MainFilterTableIsInSql(mainFilter, ij.pkRefFld,out tableAlias))
                {
                    return true;
                }
            }
            tableAlias = string.Empty;
            return false;
        }
        // PKs_InnerjoinMap[PKs_InnerjoinMap.Keys.ToList()[1]]


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




