// using Microsoft.Office.Interop.Word;
using Microsoft.VisualBasic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Web;

namespace SqlCollegeTranscripts
{
    //internal partial class DataGridViewForm : Form
    internal class SqlFactory
    {

        #region Variables
        //The table and job for this sql - myJob will by "" or "combo"
        internal string errorMsg = String.Empty;
        internal string myTable = "";
        internal int myPage = 0;  // Asks for all records, 1 is first page
        internal int myPageSize;  // Set by constructor
        internal bool includeAllColumnsInAllTables { get; set; }
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
        // PKs_OstensiveDictionary used to determine which rows to show user for any key.  (Keys is the PKs of all tables in this sql factory)  
        internal Dictionary<Tuple<string,string, string>, List<field>> PKs_OstensiveDictionary = new Dictionary<Tuple<string, string, string>, List<field>>();
        // Pks_InnerjoinMap - map pkRefTable of all FK's in myTable to all ijs below it (include grandkids).  Used to get table string. (Program never requires Table string for lower tables) 
        internal Dictionary<Tuple<string, string, string>, List<innerJoin>> PKs_InnerjoinMap = new Dictionary<Tuple<string, string, string>, List<innerJoin>>();

        #endregion

        //Constructors
        internal SqlFactory(string table, int page, int pageSize, bool includeInnerJoin)
        {
                includeAllColumnsInAllTables = false;   // Very slow in datagridview, if we make this true for transcripts - 89 columns; but database call fast
                myTable = table;
                myPage = page;
                myPageSize = pageSize;

            // This sets currentSql table and field strings - and these remain the same for this table.
            // This also sets DisplayFieldDicitionary each foreign table key in main table
            if (includeInnerJoin)
            {
                SqlFactoryFinishConstructor();
            }
        }
        internal SqlFactory(string table, int page, int pageSize):this(table,page,pageSize,true){  }

        internal void SqlFactoryFinishConstructor()
        {
            errorMsg = addInnerJoins();

            // If there is no ostensive definition for PK or FK, add the primary key of myTable or refTable
            foreach (Tuple<string, string, string> key in PKs_OstensiveDictionary.Keys)
            {
                if (PKs_OstensiveDictionary[key].Count == 0)
                {
                    field check = new field(key.Item2, key.Item3, DbType.Int32, 4);
                    field PK_myTable = dataHelper.getTablePrimaryKeyField(myTable);
                    if (check.isSameFieldAs(PK_myTable))
                    {
                        PKs_OstensiveDictionary[key].Add(PK_myTable);
                    }
                    else  // primary key of ref table - only PK's added to PK_Ostensive - either of myTable or RefTable of FKs
                    {
                        field PK_RefTable = dataHelper.getFieldFromFieldsDT(key.Item2, key.Item3);
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
                sqlString = "SELECT COUNT(1) FROM " + sqlTableString() + " " + SqlStatic.sqlWhereString(myWheres, strict);  // + " " + sqlOrderByStr();
            }
            else if (cmd == command.selectAll || (recordCount <= offset + myPageSize))
            {
                sqlString = "SELECT " + SqlStatic.sqlFieldString(myFields) + " FROM " + sqlTableString() + " " + SqlStatic.sqlWhereString(myWheres, strict) + " " + SqlStatic.sqlOrderByStr(myOrderBys) + " ";
            }
            else if (cmd == command.select)
            { 
                // Sql 2012 required for this "Fetch" clause paging
                sqlString = "SELECT " + SqlStatic.sqlFieldString(myFields) + " FROM " + sqlTableString() + SqlStatic.sqlWhereString(myWheres, strict) + SqlStatic.sqlOrderByStr(myOrderBys) + " OFFSET " + offset.ToString() + " ROWS FETCH NEXT " + myPageSize.ToString() + " ROWS ONLY";
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
                    sqlFieldStringSB.Append(SqlStatic.sqlFieldString(fls));  // function converts fls to list of fields seperated by comma
                    sqlFieldStringSB.Append(")");
                }
                sqlFieldStringSB.Append(" as DisplayMember");
                sqlFieldStringSB.Append(", ");
                // Add primary key of table as ValueField (May not need to add this twice but O.K. with Alias) 
                sqlFieldStringSB.Append(dataHelper.QualifiedAliasFieldName(cmbField));
                sqlFieldStringSB.Append(" as ValueMember");
                sqlString = "SELECT DISTINCT " + sqlFieldStringSB.ToString() + " FROM " + sqlTableString(cmbField, key) + " " + SqlStatic.sqlWhereString(myComboWheres, false) + " Order by DisplayMember";
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
                sqlString = "SELECT DISTINCT " + sqlFieldStringSB.ToString() + " FROM " + tableString + " " + SqlStatic.sqlWhereString(myComboWheres, false) + " Order by DisplayMember";
            }
            return sqlString;
        }

        internal string returnOneFieldSql(field fld)
        {
            string sqlString = "SELECT " + dataHelper.QualifiedAliasFieldName(fld) + " FROM " + sqlTableString() + " " + SqlStatic.sqlWhereString(myWheres, true) + " " + SqlStatic.sqlOrderByStr(myOrderBys) + " ";
            return sqlString;
        }


        internal string returnFixDatabaseSql(List<where> whereList) // Where string passed to this function
        {
            string strWhere = SqlStatic.sqlWhereString(whereList, true);
            return returnFixDatabaseSql(strWhere);
        }

        internal string returnFixDatabaseSql(string strWhere) // Where string passed to this function
        {
            return "SELECT " + SqlStatic.sqlFieldString(myFields) + " FROM " + sqlTableString() + " " + strWhere + " " + SqlStatic.sqlOrderByStr(myOrderBys) + " ";
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
            string ts = PkField.table + " AS " + PkField.tableAlias;
            if (PKs_InnerjoinMap.Keys.Contains(key))
            {
                List<innerJoin> ijList = PKs_InnerjoinMap[key];
                return sqlExtendTableStringByInnerJoins(ijList, ts);
            }
            else
            {
                return ts;
            }
        }

        private string sqlExtendTableStringByInnerJoins(List<innerJoin> ijList, string ts)
        {
            foreach (innerJoin ij in ijList)
            {
                string condition = string.Format(" {0} = {1} ", dataHelper.QualifiedAliasFieldName(ij.fkFld), dataHelper.QualifiedAliasFieldName(ij.pkRefFld));
                ts = "([" + ij.pkRefFld.table + "] AS " + ij.pkRefFld.tableAlias + " INNER JOIN " + ts + " ON " + condition + ")";
                // Recursive step
                Tuple<string, string, string> RefTableKey 
                    = Tuple.Create(ij.pkRefFld.tableAlias, ij.pkRefFld.table, ij.pkRefFld.fieldName);
                // This will only go 1 deep - will handle all FK's of myTable, but these may have many inner joins below them
                if (PKs_InnerjoinMap.Keys.Contains(RefTableKey))
                { 
                    List<innerJoin> RefTable_ijs = PKs_InnerjoinMap[RefTableKey];
                    ts = sqlExtendTableStringByInnerJoins(RefTable_ijs, ts);
                }
            }
            return ts;
        }

        // addInnerJoin is very complex - it must store somewhere everything about the table that the program needs to know 
        // Go through table and adds to myInnerJoins and myFields lists
        // Also, for every table in the sql, add map from its primary key to all its ForeignKeys.  (PKs_InnerjoinMap[PK] --> list of FK inner joins)
        // Also, for every table in the sql, add map from its primary key to all its displayFields (PKs_OstensiveDictionary[PK] --> list of display fields)
        // addInnerJoin() loops through the main table.  The second version of the function loops through subtables that are display keys for this table
        private string addInnerJoins()
        {
            List<string> msgStrings = new List<string>();  // List of error messages.
            List<string> allTables = new List<string>();   // List of tables
            allTables.Add(myTable);  // So next use of myTable will myTable01

            // Primary key of myTable
            // 1. Get pkMyTable and key version of pkMyTable
            field pkMyTable = dataHelper.getTablePrimaryKeyField(myTable);
            Tuple<string, string, string> pkMyTableKey = Tuple.Create(pkMyTable.tableAlias, pkMyTable.table, pkMyTable.fieldName);
            // 2, Set keyStack
            List<Tuple<string, string, string>> keyStack = new List<Tuple<string, string, string>>();
            keyStack.Add(pkMyTableKey);  // pkMyTableKey in stack - Used for non-FK fields 
            // 3, Put Primary key of main table in the first field of myFields, with keystack including itself
            pkMyTable.keyStack = keyStack; 
            myFields.Add(pkMyTable);
            // 4. Create pk keys for 2 dictionaries - pointing to empty lists
            List<field> myTableDisplayFields = new List<field>();
            PKs_OstensiveDictionary.Add(pkMyTableKey, myTableDisplayFields); 
            List<innerJoin> myTableInnerJoins = new List<innerJoin>();
            PKs_InnerjoinMap.Add(pkMyTableKey, myTableInnerJoins);  

            // Loop through myTable
            DataRow[] drs = dataHelper.fieldsDT.Select("TableName = '" + myTable + "'", "ColNum ASC");
            foreach (DataRow dr in drs)
            {
                // Get the field for this row - the same table and tableAlias is used for all columns/fields in this table
                field drField = dataHelper.getFieldFromFieldsDT(dr);
                drField.keyStack = keyStack;  // 1 element added above - modified for FK below
                if (dataHelper.isForeignKeyField(drField))
                {
                    //0. Get keyStack for Foreign key - drField.tableAlias will be 00, and so no need to change  
                    List<Tuple<string, string, string>> newKeyStack = new List<Tuple<string, string, string>>();
                    if (dataHelper.isDisplayKey(drField) || includeAllColumnsInAllTables)  // if includeAll... is true, all rows treated as DKs
                    {
                        // For display keys, start with pkMyTableKey
                        newKeyStack.Add(pkMyTableKey);
                    }
                    drField.keyStack = newKeyStack;

                    // 1. Add to myFields
                    myFields.Add(drField);

                    // 2. Create Inner Join and add to PKs_InnerJoinMap for pkMyTableKey.
                    // Primary key of ref table - already 2 steps down from myTable
                    field pkLowerTable = dataHelper.getForeignKeyRefField(drField);  
                    // 2a. This pkLowerTable may need an alias (as in _map tables) - keyStack handled in recursive step
                    int i = allTables.Count(e => e.Equals(pkLowerTable.table));
                    if (i > 0)
                    {
                        pkLowerTable.tableAlias = pkLowerTable.table + dataHelper.twoDigitNumber(i);
                    }
                    innerJoin new_ij = new innerJoin(drField, pkLowerTable);
                    myInnerJoins.Add(new_ij);
                    //// Add all innerjoins in table to pkMyTableKey
                    PKs_InnerjoinMap[pkMyTableKey].Add(new_ij);

                    //3a. Create dictionary key for inner join map - PKs_OstensiveDefinitions key added in recursive step
                    Tuple<string, string, string> pkLowerTableKey
                        = Tuple.Create(pkLowerTable.tableAlias, pkLowerTable.table, pkLowerTable.fieldName);
                    List<innerJoin> pkLowerTableInnerJoins = new List<innerJoin>();
                    PKs_InnerjoinMap.Add(pkLowerTableKey, pkLowerTableInnerJoins);

                    addInnerJoins(pkLowerTableKey, pkLowerTable, newKeyStack, ref allTables, ref msgStrings);
                }
                else if (dataHelper.isTablePrimaryKeyField(drField))
                {
                    // Primary key added above as first field
                }
                else  // Neither PK or FK
                {
                    // Add all myTable fields - keystack is myTable pk added above
                    myFields.Add(drField);
                    if (dataHelper.isDisplayKey(drField))
                    {
                            PKs_OstensiveDictionary[pkMyTableKey].Add(drField);
                    }
                }
            }

            // Return error messages if any
            if (msgStrings.Count > 0) { return String.Join(", ", msgStrings); } else { return string.Empty; } 
        }

        private void addInnerJoins(Tuple<string, string, string> pkLowerTableofMyTableFk, field pkCurrentTable, List<Tuple<string, string, string>> keyStack, ref List<string> allTables, ref List<string> msgStrings)
        {
            // Loop through fields in adding to keystack, myInnerJoins, myFieldList, PK_OstensiveDefinitions, PK_InnerJoinMap plus allTables and msgStrings
            // pkLowerTableofMyTableFk is the PK of an FK in my table (i.e. the PK of the Reference table for some FK in myTable" - used for key in PK_InnerJoinMap 
            // pkCurrentTable is the PK of the table we are looping through - may be 1 or more tables down from myTable
            // keyStack is the stack of keys - one for each recursive call -  It traces a path back to myTable
            // All tables in the keyStack will get a lower text DisplayKey in their ostensive definition.
            // keyStack is also used to check for circular display keys - i.e. a vicious circle
            // keyStack also added to the field to help us know where the field come from (used in transcripts.cs)
            // allTables is all tables that have been added - used to do an allias when table added 2 or more times
            // msgStrings returns error messages

            // New Allias for current table
            int i = allTables.Count(e => e.Equals(pkCurrentTable.table));
            if (i > 0)
            {
                pkCurrentTable.tableAlias = pkCurrentTable.table + dataHelper.twoDigitNumber(i);
            }
            // Get the key version of pkCurrentTable
            Tuple<string, string, string> pkCurrentTableKey = Tuple.Create(pkCurrentTable.tableAlias, pkCurrentTable.table, pkCurrentTable.fieldName);

            // Circular check
            if (!keyStack.Contains(pkCurrentTableKey))  // Eror Check : if table already in myTableStack, skip this foreign key
            {
                allTables.Add(pkCurrentTable.table);  // Prepare for tableAlias of next use of refTable if any
                // Add key to PKs_OstensiveDictionary for pkCurrentTableKey and map to an empty list
                List<field> odList = new List<field>();
                PKs_OstensiveDictionary.Add(pkCurrentTableKey, odList);

                // Loop through rows
                keyStack.Add(pkCurrentTableKey);  // Same key stack used for all rows including FKs - to trace the keystack
                DataRow[] drs = dataHelper.fieldsDT.Select("TableName = '" + pkCurrentTable.table + "'", "ColNum ASC");
                foreach (DataRow dr in drs)
                {
                    // Get the field for this row - the same tableAlias and key stack used for all fields in current Table
                    field drField = dataHelper.getFieldFromFieldsDT(dr); // This field may or may not be added to myFields
                    drField.tableAlias = pkCurrentTable.tableAlias;
                    drField.keyStack = keyStack;

                    if (dataHelper.isForeignKeyField(drField))
                    {
                        if (dataHelper.isDisplayKey(drField) || includeAllColumnsInAllTables)
                        {
                            // 0. Only add highest level fk fields.
                            if (keyStack.Count == 1 || includeAllColumnsInAllTables)  //Highest level fk that is not a dk
                            {
                                myFields.Add(drField);
                            }
                            else if (keyStack.Count == 2 && keyStack[0].Item2 == myTable)   //Highest level fk that is a dk
                            {
                                myFields.Add(drField);
                            }
                    
                            // 1. Create Inner Join and add to myInnerjoins and PKs_InnerJoinMap. 
                            field pkLowerField = dataHelper.getForeignKeyRefField(drField);  // Primary key of ref table
                            innerJoin new_ij = new innerJoin(drField, pkLowerField);
                            myInnerJoins.Add(new_ij);
                            PKs_InnerjoinMap[pkLowerTableofMyTableFk].Add(new_ij);  // Only used to get tables 'under' myTableKey

                            // 2. Recursive step - Lists are by Reference, but we want this list by value - so make a copy and use the copy
                            List<Tuple<string, string, string>> keyStackByValue = new List<Tuple<string, string, string>>(keyStack);
                            addInnerJoins(pkLowerTableofMyTableFk, pkLowerField, keyStackByValue, ref allTables, ref msgStrings);
                        }
                    }
                    else if (dataHelper.isTablePrimaryKeyField(drField))
                    {
                        // Foreign keys are added as field in myFields - so no need to add primary key of ref tables
                    }
                    else  // Neither PK or FK
                    {
                        // Add if myTable and all lower table displaykeys
                        if (dataHelper.isDisplayKey(drField) || includeAllColumnsInAllTables)
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
            else
            {
                msgStrings.Append("Skipping circular foreignkey: (" + dataHelper.QualifiedAliasFieldName(pkCurrentTable) + ")");
            }
        }

        internal bool TableIsInMyInnerJoins(field PkField, string tableAliasName)
        {
            if (tableAliasName == PkField.tableAlias)  // Table can be filtered on itself 
            { 
                return true; 
            } 
            Tuple<string, string, string> key = Tuple.Create(PkField.tableAlias, PkField.table, PkField.fieldName);
            if (PKs_InnerjoinMap.ContainsKey(key))
            { 
                foreach (innerJoin ij in PKs_InnerjoinMap[key])
                {
                    // Recursive call
                    if (TableIsInMyInnerJoins(ij.pkRefFld, tableAliasName))
                    {
                        return true;
                    }
                }
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
            if (PKs_InnerjoinMap.Keys.Contains(key))
            { 
                foreach (innerJoin ij in PKs_InnerjoinMap[key])
                {
                    if(MainFilterTableIsInSql(mainFilter, ij.pkRefFld,out tableAlias))
                    {
                        return true;
                    }
                }
            }
            tableAlias = string.Empty;
            return false;
        }

        // Find value with unknown Alias - use this form when there is only one such basefield in dr.
        internal string getStringValueFromDataRowBasefield(DataRow dr, field fieldToFind)
        {
            return getStringValueFromDataRowBasefield(dr, fieldToFind, defaultAncesterField(fieldToFind));
        }

        // Find value with unknown Alias - use this form when there are more than one such baseFields.
        internal string getStringValueFromDataRowBasefield(DataRow dr, field fieldToFind, string ancesterTable)
        {
            // Get col and value
            int transFieldToFindColumnID = colIndexOfBaseField(fieldToFind, ancesterTable);
            string transFieldToFindValue = dr[transFieldToFindColumnID].ToString();
            return transFieldToFindValue;
        }

        internal int getIntValueFromDataRowBasefield(DataRow dr, field fieldToFind)
        {
            return getIntValueFromDataRowBasefield(dr, fieldToFind, defaultAncesterField(fieldToFind));
        }

        internal int getIntValueFromDataRowBasefield(DataRow dr, field fieldToFind, string ancesterTable)
        {
            return Int32.Parse(getStringValueFromDataRowBasefield(dr, fieldToFind, ancesterTable));
        }
        internal bool getBoolValueFromDataRowBasefield(DataRow dr, field fieldToFind, string ancesterTable)
        {
            return Boolean.Parse(getStringValueFromDataRowBasefield(dr, fieldToFind, ancesterTable));
        }


        internal Single getSingleValueFromDataRowBasefield(DataRow dr, field fieldToFind)
        {
            return getSingleValueFromDataRowBasefield(dr,fieldToFind,defaultAncesterField(fieldToFind));
        }

        internal Single getSingleValueFromDataRowBasefield(DataRow dr, field fieldToFind, string ancesterTable)
        {
            return Single.Parse(getStringValueFromDataRowBasefield(dr, fieldToFind, ancesterTable));
        }

        // Most flds have their own table PK in the keyStack
        // But the keyStack for a FK that is not a display key starts with the pk of the Reference table.
        private string defaultAncesterField(field fld)
        {
            if (dataHelper.isForeignKeyField(fld))
            {
                field pkRefField = dataHelper.getForeignKeyRefField(fld);
                return pkRefField.table;
            }
            else
            {
                return fld.table;
            }
        }

        // Find index with unknown Alias - Use this method if you are certain there is only one
        internal int colIndexOfBaseField(field fld)
        {
                return colIndexOfBaseField(fld, defaultAncesterField(fld));    
        }

        // Find index with unknown Alias - Use this method if thee may be more than one
        internal int colIndexOfBaseField(field fld, string ancesterTable)
        {
            // Get ancesterKey from this ancesterTable - this must be in the keyStack of the field.
            // Ex: transcript Grade column (perhaps A-) keystack will be the pk tuples of transcript and grade tables
            // Ex: transcript ReqName column keyStack will be pk tuples of transcript, courseterm, course, requirement, requirement names tables
            // Ex: 
            field pkAncesterTable = dataHelper.getTablePrimaryKeyField(ancesterTable);
            Tuple<string, string, string> ancestorKey = Tuple.Create(pkAncesterTable.tableAlias, pkAncesterTable.table, pkAncesterTable.fieldName);

            int intToReturn = -1;
            // Use stepsFromFld when one keyStack is a subStack of another - in this case return the shortest keyStack
            int stepsFromFld = 2222;  
            for (int i = 0; i < myFields.Count; i++)
            {
                if (myFields[i].isSameBaseFieldAs(fld))  // No AncestorKey given - use this if certain there is only one matching field
                {
                    if (myFields[i].keyStack != null)  // pkMyTable.keyStack == null - no longer true 
                    {
                        int count = 0;
                        foreach (Tuple<string, string, string> key in myFields[i].keyStack)
                        {
                            if (key.Item2 == ancestorKey.Item2 && key.Item3 == ancestorKey.Item3)
                            {
                                // If we use myTable as the ancester field, count will be 0 and the stepsFromFld = keyStack.count
                                if (myFields[i].keyStack.Count - count < stepsFromFld)
                                { 
                                    stepsFromFld = myFields[i].keyStack.Count - count; 
                                    intToReturn = i;
                                }
                            }
                            count++;
                        }
                    }
                }
            }
            return intToReturn;
        }

        // Class in class to note the static methods - but not consistently applied to all static methods
        internal static class SqlStatic
        {
            internal static string sqlWhereString(List<where> whereList, bool strict)
            {
                // Make a list of the conditions
                List<string> WhereStrList = new List<string>();
                foreach (where ws in whereList)
                {
                    string condition = "";
                    if (dataHelper.TryParseToDbType(ws.whereValue, ws.fl.dbType))
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
                                    // Strict use of "Like"
                                    condition = dataHelper.QualifiedAliasFieldName(ws.fl) + " = '" + ws.whereValue + "'";  //Exact string
                                }
                                else
                                {
                                    // Non-strict uses of "Like"
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

            internal static string sqlFieldString(List<field> fieldList)
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

            internal static string sqlOrderByStr(List<orderBy> orderByList)
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




