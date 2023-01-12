using AccessFreeData2;
using Microsoft.Office.Interop.Word;
using Microsoft.VisualBasic;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace AccessFreeData
{
    //internal partial class DataGridViewForm : Form
	internal class SqlBuilder
	{
        //Constructor sets myTable, myJob, strSqlTable, and strSqlFields.
        internal SqlBuilder(string table, string job, int page, int pageSize)
        {
            myTable = table;
            myJob = job;
			myPage = page;
			myPageSize = pageSize;
        }

        //The table and job for this sql - myJob will by "" or "combo"
        internal string myTable = "";
        internal string myJob = "";
		internal int myPage = 0;  // Asks for all records, 1 is first page
		internal int myPageSize = 1;
		internal int TotalPages { get; set; }
        private int recordCount = 0;
		internal int RecordCount 
		{ 
			get { return recordCount; }
			set { 
					recordCount = value;
					TotalPages = (int)Math.Ceiling((decimal)recordCount/ myPageSize);
				} 
		}		

        // Four lists to build, and SQL will be built from these
        // myInnerJoins and myFields remain the same for a given table & job		
        internal List<where> myWheres = new List<where>();
        internal List<orderBy> myOrderBys = new List<orderBy>();
        internal List<innerJoin> myInnerJoins = new List<innerJoin>();
        internal List<field>myFields = new List<field>();  // Table and field

		// This used only once for a sqlTemp
		internal bool ignoreTopBoxes = false;

		// The primary function of this class
		//internal string returnSql(string command)
		//{
		//	if (command == "count")
		//	{
		//		return returnSql(command, 0);
		//	}
		//	return "Error: See returnSql definition";
  //      }
        internal string returnSql(string command)
        {
            // The main function of this class - used for tables and combos.
			// Logic: Set 
            int offset = (myPage - 1) * myPageSize;
            string sqlString = "";
			//This exception only for combo's, and class must be immediately destroyed afterwards
			if (command == "count") 
			{
				sqlString = "SELECT COUNT(1) FROM " + sqlTableString() + " " + sqlWhereString();  // + " " + sqlOrderByStr();
            }
            else if(myPage == -1 || (recordCount <= offset + myPageSize)) 
			{  
				sqlString = "SELECT " + sqlFieldString() + " FROM " + sqlTableString() + " " + sqlWhereString() + " " + sqlOrderByStr() + " ";
			} 
			else 
			{ // Sql 2012 required for this "Fetch" clause paging
				sqlString =  "SELECT " + sqlFieldString() + " FROM " + sqlTableString() + sqlWhereString() + sqlOrderByStr() + " OFFSET " + offset.ToString() + " ROWS FETCH NEXT " + myPageSize.ToString() + " ROWS ONLY";
            }
			return sqlString;
        }
        
		private string sqlTableString()
        {
            string ts = myTable;
            foreach (innerJoin ij in myInnerJoins)
            {
                // Get the 'on' condition
                StringBuilder sb = new StringBuilder();
                sb.Append(SqlHelper.QualifiedFieldName(ij.table1, ij.field1));
                sb.Append("=");
                sb.Append(SqlHelper.QualifiedFieldName(ij.table2, ij.table2PrimaryKey));
                string condition = sb.ToString();

                // construct inner join
                ts = "([" + ij.table2 + "] INNER JOIN " + ts + " ON " + condition + ")";
            }
            return ts;
        }

		private string sqlFieldString()
		{
			// Make a list of the qualified fields, i.e. [table].[field]
            List<string> fieldStrList = new List<string>();
            foreach (field fs in myFields)
            {
				fieldStrList.Add(SqlHelper.QualifiedFieldName(fs.table, fs.fieldName));
            }
			// Join with commas - .Join knows to skip a closing comma.
            return String.Join(",", fieldStrList);
        }

        private string sqlWhereString()
        {
			if(myWheres.Count == 0) { return ""; }
			// Make a list of the conditions
            List<string> WhereStrList = new List<string>();
            foreach (where ws in myWheres)
            {
                // Get where condition
                string condition = "";
                switch (ws.dbType)
				{
					case "bigint":
					case "numeric":
                    case "smallint":
                    case "decimal":
                    case "smallmoney":
                    case "int":
                    case "tinyint":
                    case "money":
                    case "float":
                    case "real":
					case "binary":
						condition = SqlHelper.QualifiedFieldName(ws.table, ws.field) + " = " + ws.value;
						break;
					case "date":
                    case "datetimeoffset":
                    case "datetime2":
                    case "smalldatetime":
                    case "datetime":
					case "time":
                        condition = SqlHelper.QualifiedFieldName(ws.table,ws.field) + "= #" + ws.value + "#";
                        break;
					case "char":
					case "varchar":
                    case "nchar":
					case "nvarchar":
                        condition = SqlHelper.QualifiedFieldName(ws.table, ws.field) + " Like '" + ws.value + "%'";  //Same starting string
						break;
					case "bit":
                        if (ws.value.ToLower() == "true"){
                            condition = SqlHelper.QualifiedFieldName(ws.table, ws.field) + " = " + SqlHelper.getSqlTrue();
                        }else{
                            condition = SqlHelper.QualifiedFieldName(ws.table, ws.field) + " = " + SqlHelper.getSqlFalse();
                        }
					break;
                }
				if(condition != "")
				{ 
					WhereStrList.Add(condition);
				}
            }
			// Return string constructed from list of conditions
            return " WHERE " + String.Join(" AND ", WhereStrList);
        }

		private string sqlOrderByStr()
		{
			if(myOrderBys.Count == 0) { return ""; }
			//Make a list of order by clauses
            List<string> orderByStrList = new List<string>();
            foreach (orderBy ob in myOrderBys)
            {
				string qualFieldName = SqlHelper.QualifiedFieldName(ob.table, ob.field);
				if(ob.sortOrder == System.Windows.Forms.SortOrder.Descending) { 
					orderByStrList.Add(qualFieldName + " DESC");
				}
				else{
                    orderByStrList.Add(qualFieldName + " ASC");
                }
            }
			// Return string constructed by list of order by clauses
            return " ORDER BY " + String.Join(",", orderByStrList);
        }

        // Go through table and adds to myInnerJoins and myFields lists
		// Note - 0 inner joins will add fields from myTable (1st Call)
        internal string callInnerJoins(string tableName, bool showWhiteColumn)
		{
			StringBuilder MsgStr = new StringBuilder();
             string field1 = "", table2 = "";
			string strSql = "Select * from afdFieldData where tableName = '" + tableName + "' ORDER BY zOrder DESC";
			SqlCommand cmd = new SqlCommand(strSql, SqlHelper.cn);
			using (SqlDataReader rs = cmd.ExecuteReader())
			{
				// Loop through fields - adding to innerjoins and fields lists
				while (rs.Read())
				{
					field1 = rs.GetString("fieldName");
					field fi = new field(tableName, field1);
                    //Primary Key field - program assumes this will be the first field
                    if (SqlHelper.isTablePrimaryKeyField(tableName, field1))
					{
                        // Add the primary key if option select - but don't add for combo
						// Don't add for myTable because this is added in main call
                        if (showWhiteColumn && myJob != "combo"  && tableName != myTable)
                        {
                            myFields.Add(fi);
                        }
                    }
                    else if (SqlHelper.fieldIsForeignKey(tableName, field1))  // Inner join
					{
						table2 = SqlHelper.getForeignTable(tableName, field1);
						// Handle circles (don't allow) and repeats (needs an alias)
						int alliasCount = 0;
						foreach (innerJoin ij in myInnerJoins)
						{
							// Don't allow circular  Table1 --> Table2 --> Table1
							if (ij.table1 == table2)
							{
								// msgStr.Append(translation.tr("WarningTwoInnerJoinsOnTable", table2, "", ""));
								MsgStr.Append("Circular innerjoin: " + tableName);
								return MsgStr.ToString();
							}
							// Requires allias - i tells you how many times this table has been an alliaas
							// If Table1 (Table2ID)-->Table2, Table1 (Table2ID2) --> Table2, Table3 (Table2ID)-->Table2, then i=2
							if (ij.table2 == table2)
							{
								alliasCount++;
								MsgStr.Append("Alias: " + table2 + alliasCount.ToString());
							}
						}
                        // myFields.Add(fi); // Delete later
                        innerJoin new_ij = new innerJoin(tableName, field1, table2);
						if (alliasCount > 0) { new_ij.table2Allias = table2 + alliasCount.ToString(); }
						myInnerJoins.Add(new_ij);
						callInnerJoins(table2, showWhiteColumn); //Recursive step - table 2 becomes table 1
					}
					else
					{
						field fl = new field(tableName, field1);
						if (myJob == "combo")
						{
							if (rs.GetBoolean("showInYellow"))
							{
								myFields.Add(fl);
							}
						}
						else if (tableName == myTable)
						{
							myFields.Add(fl);  // Could add "hiddenColumn" field to asfFieldData

						}
						else   // looping through a son of myTable
						{
							if (rs.GetBoolean("showInAll") || rs.GetBoolean("showInYellow"))
							{
								myFields.Add(fl);
							}
						}
					}
				}
			}
			return MsgStr.ToString();
        }

		internal bool fieldIsInMyFields(string tableName, string fieldName)
		{ 
			foreach(field fl in myFields)
			{
				if(fl.table == tableName && fl.fieldName == fieldName)
				{ return true; }

			}
			return false;
		}

		internal bool TableIsInMyTables(string tableName)
		{
			if(tableName == myTable) { return true; }
			foreach (innerJoin ij in myInnerJoins)
			{
				if (tableName == ij.table2) { return true; }
			}
			return false;	
		}

  //      private string shortCondition(string vnt)
		//{
		//	bool deleting = false, finishedDeleting = false;
		//	StringBuilder newVnt = new StringBuilder();
		//	//Delete from first [ to the next .
		//	int ln = Strings.Len(vnt);
		//	int tempForEndVar = ln;
		//	for (int i = 1; i <= tempForEndVar; i++)
		//	{
		//		if (!finishedDeleting && vnt.Substring(i - 1, Math.Min(1, vnt.Length - (i - 1))) == "[")
		//		{
		//			deleting = true;
		//		}
		//		if (deleting)
		//		{
		//			//nothing
		//		}
		//		else
		//		{
		//			newVnt.Append(vnt.Substring(i - 1, Math.Min(1, vnt.Length - (i - 1))));
		//		}
		//		if (deleting && vnt.Substring(i - 1, Math.Min(1, vnt.Length - (i - 1))) == ".")
		//		{
		//			deleting = false;
		//			finishedDeleting = true;
		//		}
		//	}
		//	return newVnt.ToString();
		//	//    shortCondition = Replace(CStr(vnt), "[", "")
		//	//    shortCondition = Replace(shortCondition, "]", "")
		//}

		//private void applyAllCellFilters(string tableName)
		//{
		//	string val = "", myField = "", dbType = "";
		//	//This is called if the user has set cell filter in afdFieldData for some field
		//	string strSql = "SELECT * FROM afdFieldData WHERE tableName = '" + tableName + "'";
		//	using (SqlCommand cmd = new(strSql, SqlHelper.cn))
		//	{ 
		//		using (SqlDataReader rs = cmd.ExecuteReader())
		//		{
		//			while (rs.Read())
		//			{
		//				try
		//				{
		//					if (!rs.IsDBNull(rs.GetOrdinal("fieldName")) && !rs.IsDBNull(rs.GetOrdinal("filterValue")))
		//					{
		//						myField = rs.GetString(rs.GetOrdinal("fieldName"));
		//						val = rs.GetString(rs.GetOrdinal("filterValue"));
		//						if (val != "" && myField != "")
		//						{
		//							dbType = rs.GetString(rs.GetOrdinal("dbType")); // must update adfColumnsData to include this field
		//							where wh = new where(myField, val, dbType);
		//							myWheres.Add(wh);
		//						}
		//					}
		//				}
		//				catch
		//				{

		//				}
		//			}
		//			return;
		//		}
  //          }
  //      }
		//internal void higherInnerJoin(string ct, string ct_RestrictedTable)
		//{
		//	//Only called for a table in a combo box.
		//	//ct is the tableCurrent. It is filtered by ct_restrictedTable (which is in another combobox)
		//	//Example myTa ct=course-term, ct_restrictedTable = term with value 49, this combo = course
		//	//So I want to restrict courses to those which were offered in term 49, i.e. to x such that x-49 is in courses-term.
		//	//Get the primary key of myTable (i.e. course_id)
		//	string myTableID = SqlHelper.getTablePrimaryKeyFieldSQL(myTable);
		//	//Get its foreign key in current table (i.e. course_id).
		//	string myTableKeyFieldInCT = SqlHelper.getTable2ForeignKeyField(ct, myTable);
		//	//Get the foreign key of myTable (term_id)
		//	string restrictedTableKeyFieldInCT = SqlHelper.getTable2ForeignKeyField(ct, ct_RestrictedTable);
		//	//Get the last value of the restricted table. (i.e 49)
		//	int val = Convert.ToInt32(Double.Parse(SqlHelper.getSystemTableValue("afdTableData", ct_RestrictedTable, "", "filterID", DbType.Int32)));
		//	//Make the strSqlFields distinct -- these are all in the combo table
		//	//I don't add any fields from the higher table (i.e. from course_term table
		//	strSqlFields = "Distinct " + strSqlFields;
		//	//Inner Join Condition: ct.myTableID = myTable.myTableID, course_term.courseID = courses.CourseID
		//	string condition = "[" + ct + "].[" + myTableKeyFieldInCT + "] = " + myTableID;
		//	//Inner Join [ct] INNER JOIN (strSqlTable) ON condition
		//	if ((strSqlTable.IndexOf("Inner") + 1) == 0)
		//	{
		//		strSqlTable = "[" + ct + "] INNER JOIN " + strSqlTable + " ON " + condition;
		//	}
		//	else
		//	{
		//		strSqlTable = "[" + ct + "] INNER JOIN (" + strSqlTable + ") ON " + condition;
		//	}
		//	//Finally we restrict the id of the restricted table in the higher table
		//	//This higher table was not originally in strSqlTable, but the above put it in.
		//	sqlWhere(colSqlPermanentWhere, "[" + ct + "].[" + restrictedTableKeyFieldInCT + "]", val.ToString(), DbType.Int32);
		//}

		//----------------------------------------------------------------------
		//--------------------------sql notes-----------------------------------

		//SELECT [tablename.]fieldname [,[tablename.]fieldname ...]
		//        FROM tablename INNER JOIN tablename2
		//        ON tablename.fieldname = tablename2.fieldname2
		//SELECT col1 FROM myTable WHERE col1 LIKE 'Ken%'"
		//SELECT * FROM books ORDER BY author, price DESC
		//SELECT DISTINCTROW Sum(UnitPrice * Quantity) AS Sales, (FirstName & Chr(32) & LastName) AS Name
		//FROM Employees INNER JOIN (Orders INNER JOIN [Order Details]
		//ON [Order Details].OrderID = Orders.OrderID )
		//ON Orders.EmployeeID = Employees.EmployeeID
		//GROUP BY (FirstName & Chr(32) & LastName);
		//Open record
		//sql = "SELECT * FROM ??"
		//rs.Open sql, cn, adOpenKeyset, adLockOptimistic
		//Delete records
		//While not rs.EOF
		//   rs.delete adAffectCurrent
		//   modifiedRecords = modifiedRecords + 1
		//   rs.moveNext
		//Wend
		//ModifyRecords
		//While not rs.EOF
		//   temp = rs("?1")
		//   temp2 = rs("?2")
		//   rs("?1") = temp & temp2
		//   rs.update
		//   rs.movenext
		//Wend
		//Add records
		//rs.addNew
		//rs.update (?)

		//---------------------------------------------------------------------------------------------------------------------------------------------
		//---------------------------------------------------------------------------------------------------------------------------------------------
		//Some notes on sql-------------------------------------------------------------------------
		//---------------------------------------------------------------------------------------------------------------------------------------------
		//OpenSchema can have 2 variable: adSchemaTables and array(TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME,TABLE_TYPE)
		//                                adSchemaColumns and array(TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME,COLUMN_NAME)
		//                                adSchemaIndexes and array(TABLE_CATALOG,TABLE_SCHEMA,INDEX_NAME,TYPE,TABLE_NAME)
		//In access table_catalog and table_schema are Empty.  An empty table_name returns all tables.
	}
}







//OLD - TO BE DELETED
//These two strings determine the sql string
//private string strSqlTable = "", strSqlFields = ""; //never change

//These colections store the current where and orderby info;
//The colSqlPermanentWhere are things that don't change
//private OrderedDictionary _colSqlOrderBy = null;
//private OrderedDictionary colSqlOrderBy
//{
//	get
//	{
//		if (_colSqlOrderBy is null)
//		{
//			_colSqlOrderBy = new OrderedDictionary(System.StringComparer.OrdinalIgnoreCase);
//		}
//		return _colSqlOrderBy;
//	}
//	set
//	{
//		_colSqlOrderBy = value;
//	}
//}

//private OrderedDictionary _colSqlPermanentWhere = null;
//private OrderedDictionary colSqlPermanentWhere
//{
//	get
//	{
//		if (_colSqlPermanentWhere is null)
//		{
//			_colSqlPermanentWhere = new OrderedDictionary(System.StringComparer.OrdinalIgnoreCase);
//		}
//		return _colSqlPermanentWhere;
//	}
//	set
//	{
//		_colSqlPermanentWhere = value;
//	}
//}

//private OrderedDictionary _colSqlWhere = null;
//private OrderedDictionary colSqlWhere
//{
//	get
//	{
//		if (_colSqlWhere is null)
//		{
//			_colSqlWhere = new OrderedDictionary(System.StringComparer.OrdinalIgnoreCase);
//		}
//		return _colSqlWhere;
//	}
//	set
//	{
//		_colSqlWhere = value;
//	}
//}

//internal Sub setTableString(table As String, job As String)
//Dim strSql As String, rs As ADODB.Recordset, fld As String, joinTable As String
//'This is called immediately after initialization.
//'It sets myTable, myJob, strSqlTable, and strSqlFields.
//    strSqlTable = ""
//    strSqlFields = ""
//    myTable = table
//    myJob = job
//    If myTable <> "" Then
//        'Initialize strSqlFields, strSqlTable
//        Call callInnerJoins(myTable) 'this will set sort fields
//        'If there were no inner joins
//        If strSqlTable = "" Then
//            strSqlTable = "[" & myTable & "]"
//            strSqlFields = strSqlFields & frmDataGrid.getTablePrimaryKeyFieldSQL(myTable)
//        Else
//            'Add the myTable ID field to strSqlFields -- this already ends in a comma (or in a strange case may be empty)
//            strSqlFields = strSqlFields & frmDataGrid.getTablePrimaryKeyFieldSQL(myTable)
//        End If
//    End If
//End Sub

//internal void GetOrderBy(string tableName, string field)
//{
//    int vnt = 0;
//    int i = 0;
//    string descAsc = "", strSql = "";
//    string orderClause = "";

//    string fld = "[" + tableName + "].[" + shortFld + "]";

//    //Adds fld to colSqlOrderBy
//    if (fldType == DbType.Boolean || fldType == DbType.String || fldType == DbType.String || fldType == DbType.String || fldType == DbType.String || fldType == DbType.String || fldType == DbType.String || fldType == DbType.DateTime || fldType == DbType.Date || fldType == DbType.Time || fldType == DbType.DateTime || fldType == DbType.DateTime || fldType == DbType.Currency || fldType == DbType.Single || fldType == DbType.Double || fldType == DbType.Int32 || fldType == DbType.Decimal || fldType == DbType.Int16 || fldType == DbType.SByte || fldType == DbType.Decimal)
//    {
//        //Deletes old sort on this field value and flips ASC and DESC
//        foreach (int vntIterator in colSqlOrderBy.Values)
//        {
//            vnt = vntIterator;
//            i++;
//            if ((vnt.ToString().IndexOf(fld) + 1) == 1)
//            {
//                if (vnt.ToString().IndexOf("ASC") >= 0)
//                {
//                    descAsc = "DESC";
//                }
//                else
//                {
//                    descAsc = "ASC";
//                }
//                colSqlOrderBy.RemoveAt(i - 1);
//                goto label10;
//            }
//            vnt = default(int);
//        }
//        //If we get here, we have not jumped to 10 - descAsc not yet set, so set from systemTables
//        descAsc = "ASC"; //default
//        strSql = "SELECT zTOa FROM afdFieldData WHERE tableName = '" + tableName + "' AND fieldName = '" + shortFld + "'";
//        SqlCommand cmd = new SqlCommand(strSql, SqlHelper.cn);
//        using (SqlDataReader rs = cmd.ExecuteReader())
//        {
//            if (rs.Read())
//            {
//                if (Convert.ToBoolean(rs["zTOa"]))
//                {
//                    descAsc = "DESC";
//                }
//            }
//        }

//    label10: orderClause = fld + " " + descAsc;
//        //Add new
//        if (colSqlOrderBy.Count == 0)
//        {
//            colSqlOrderBy.Add(Guid.NewGuid().ToString(), orderClause);
//        }
//        else
//        {
//            colSqlOrderBy.Insert(0, Guid.NewGuid().ToString(), orderClause);
//        }
//    }
//    else
//    {
//        // msgSB(translation.tr("FieldNotOrderable", orderClause, "", ""));
//    }
//}


//		private string getSqlWhere(ComboBox[] cmbWhereBackup, ComboBox[] cmbWhere, TextBox txtWhere)
//		{
//            //UPGRADE_WARNING: (2065) ADODB.DataTypeEnum property DataTypeEnum.adEmpty has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2065
//            string result = "";
//			string whereFld = "", whereCond = "";
//			DbType whereType = DbType.Object;
//			string vnt = "";
//			//Clear colSqlWhere
//			int i = 1;
//			foreach (string vntIterator in colSqlWhere.Values)
//			{
//				vnt = vntIterator;
//				colSqlWhere.RemoveAt(i - 1);
//				vnt = default(string);
//			}
//			whereTableCaption = "(";

//			//Load new where clauses in colSqlWhere from the topBoxes that are visible and the extraWhere of this class
//			if (!ignoreTopBoxes && myJob != "combo")
//			{
//				//Update sqlwhere by cmbWhere and txtWhere boxes
//				if (txtWhere.Visible)
//				{
//					if (Strings.Len(txtWhere.Text.Trim()) > 0)
//					{
//                        // whereFld = SqlHelper.getCorrectFieldName(rsCurrent, Convert.ToString(txtWhere.Tag));
//                        whereFld = SqlHelper.getCorrectFieldName(Convert.ToString(txtWhere.Tag));
//						whereCond = txtWhere.Text;
////ASM					whereType = ADORecordSetHelper.GetDBType(rsCurrent.GetField(whereFld).FieldMetadata.DataType);
//						sqlWhere(colSqlWhere, whereFld, whereCond, whereType);
//					}
//				}
//                int tempForEndVar = cmbWhere.Count() - 1;
//				for (i = 0; i <= tempForEndVar; i++)
//				{
//					if (cmbWhere[i].Visible)
//					{
//						if (cmbWhere[i].SelectedIndex > 0)
//						{
//							//whereFld = DataGridViewForm.getTablePrimaryKeyFieldSQL(DataGridViewForm.getForeignTable(myTable, DataGridViewForm.cmbWhere(i).Tag))
//							whereFld = "[" + myTable + "].[" + SqlHelper.short_Renamed(Convert.ToString(cmbWhere[i].Tag)) + "]"; //This should be in SQL
//							whereCond = (string)cmbWhereBackup[i].Items[cmbWhere[i].SelectedIndex];
//							whereType = DbType.Int32;
//							sqlWhere(colSqlWhere, whereFld, whereCond, whereType);
//						}
//					}
//				}
//			}
//			//Update sqlWhere by extra
//			if (myExtraField != "")
//			{
//				whereFld = myExtraField;
//				whereCond = myExtraValue;
//				whereType = myExtraType;
//				sqlWhere(colSqlWhere, whereFld, whereCond, whereType);
//			}

//			//Begin with the things that don't change
//			foreach (string vntIterator2 in colSqlPermanentWhere.Values)
//			{
//				vnt = vntIterator2;
//				if (result == "")
//				{
//					result = "WHERE " + vnt;
//				}
//				else
//				{
//					result = result + " AND " + vnt;
//				}
//				whereTableCaption = whereTableCaption + shortCondition(vnt) + ",";
//				vnt = default(string);
//			}
//			//Add the changing things (added in this sub-routine)
//			foreach (string vntIterator3 in colSqlWhere.Values)
//			{
//				vnt = vntIterator3;
//				if (result == "")
//				{
//					result = "WHERE " + vnt;
//				}
//				else
//				{
//					result = result + " AND " + vnt;
//				}
//				whereTableCaption = whereTableCaption + shortCondition(vnt) + ",";
//				vnt = default(string);
//			}
//			//Note: whereTableCaption continued in getSqlOrderBy

//			return result;
//		}
//private void sqlWhere(OrderedDictionary whereCollection, string fld, string val, DbType fldType)
//{
//	int vnt = 0;
//	int i = 0;
//	string condition = "";
//	//Adds this where clause to whereCollection
//	//Delete old where for this field if any
//	foreach (int vntIterator in whereCollection.Values)
//	{
//		vnt = vntIterator;
//		i++;
//		if ((vnt.ToString().IndexOf(fld) + 1) == 1)
//		{
//			whereCollection.RemoveAt(i - 1);
//		}
//		vnt = default(int);
//	}
//	foreach (int vntIterator2 in whereCollection.Values)
//	{
//		vnt = vntIterator2;
//		i++;
//		if ((vnt.ToString().IndexOf(fld) + 1) == 1)
//		{
//			whereCollection.RemoveAt(i - 1);
//		}
//		vnt = default(int);
//	}

//	//Get condition - only allow certain types of fields
//	if (fldType == DbType.String || fldType == DbType.String || fldType == DbType.String || fldType == DbType.String || fldType == DbType.String || fldType == DbType.String)
//	{
//		condition = fld + " Like '" + val + "%'";
//	}
//	else if (fldType == DbType.DateTime || fldType == DbType.Date || fldType == DbType.Time || fldType == DbType.DateTime || fldType == DbType.DateTime)
//	{ 
//		condition = fld + "= #" + val + "#";
//	}
//	else if (fldType == DbType.Single || fldType == DbType.Double || fldType == DbType.Int32 || fldType == DbType.Decimal || fldType == DbType.Int16 || fldType == DbType.SByte || fldType == DbType.Decimal)
//	{ 
//		condition = fld + " = " + val;
//	}
//	else if (fldType == DbType.Boolean)
//	{ 
//		if (val == "True")
//		{
//			condition = fld + " = " + SqlHelper.getSqlTrue();
//		}
//		else
//		{
//			condition = fld + " = " + SqlHelper.getSqlFalse();
//		}
//	}

//	//Add new where for this field
//	if (condition != "")
//	{
//		if (whereCollection.Count == 0)
//		{
//			whereCollection.Add(Guid.NewGuid().ToString(), condition);
//		}
//		else
//		{
//			whereCollection.Insert(0, Guid.NewGuid().ToString(), condition);
//		}
//	}
//	else
//	{
//		// msgSB(translation.tr("FieldHasAnUnrecognizedType", fld, ((int) fldType).ToString(), ""));
//		SystemSounds.Beep.Play();
//	}
//}

//private string getSqlOrderBy()
//{
//	string result = "";
//	string vnt = "";
//	//get orderby clause
//	foreach (string vntIterator in colSqlOrderBy.Values)
//	{
//		vnt = vntIterator;
//		if (result == "")
//		{
//			result = "ORDER BY " + vnt;
//		}
//		else
//		{
//			result = result + ", " + vnt;
//		}
//		whereTableCaption = whereTableCaption + shortCondition(vnt) + ",";
//		vnt = default(string);
//	}

//	//Finish whereTableCaption (begun in getSqlWhere
//	if (whereTableCaption == "(")
//	{
//		whereTableCaption = "";
//	}
//	else
//	{
//		whereTableCaption = whereTableCaption.Substring(0, Math.Min(Strings.Len(whereTableCaption) - 1, whereTableCaption.Length)) + ")";
//	}
//	return result;
//}



