using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AccessFreeData2
{
    #region Helper classes - field, where, innerJoin, orderBy, and enum direction
    internal class field
    {
        internal string fieldName;
        internal string table;
        internal field(string table, string fieldName)
        {
            this.table = table;
            this.fieldName = fieldName;
        }
    }
    internal class where 
    {  
        internal where(string table, string field, string value, string dbType)
        {
            this.table = table;
            this.field = field;
            this.value = value;
            this.dbType = dbType;
        }
        internal string table { get; set; }
        internal string field { get; set; }
        internal string value { get; set; } 
        internal string dbType { get; set; }  
    }
    internal class innerJoin 
    { 
        internal string table1 { get; set; } 
        internal string field1 { get; set; } 
        internal string table2 { get; set; }
        internal string table2PrimaryKey { get; set; }
        internal string table2Allias { get; set; }
        internal innerJoin(string table1, string field1, string table2) { 
            this.table1 = table1;
            this.table2 = table2;   
            this.field1 = field1;
            this.table2PrimaryKey = SqlHelper.getTablePrimaryKeyField(table2,false);
            this.table2Allias = string.Empty;
        }
    }
    internal class orderBy {
        internal string field;
        internal string table;
        internal System.Windows.Forms.SortOrder sortOrder;
        internal int gridColumn = 0;

        internal orderBy(string table, string field, System.Windows.Forms.SortOrder sortOrder, int gridColumn)
        {
            this.table = table;
            this.field = field;
            this.sortOrder = sortOrder;
            this.gridColumn = gridColumn;
        }
    }
    #endregion

    internal static class SqlHelper
    {
        // Variables
        internal static SqlConnection? cn = null;
        internal static System.Data.DataTable? tablesDT = null;
        internal static System.Data.DataTable? fieldsDT = null;
        internal static System.Data.SqlClient.SqlDataAdapter? tablesDA = null;
        internal static System.Data.SqlClient.SqlDataAdapter? fieldsDA = null;
        internal static System.Data.DataSet? sysDataSet = null;

        internal static string QualifiedFieldName(string table, string field)
        { 
            StringBuilder sb = new StringBuilder(); 
            sb.Append("[" + table + "]");
            sb.Append(".");
            sb.Append("[" + field + "]");
            return sb.ToString();
        }

        internal static string getTablePrimaryKeyField(string table, bool useQualifiedName)
        {
            string result = "";
            result = getStringValueSystemTableField(table, "primaryKey");
            if (useQualifiedName && result != string.Empty)
            {
                result = QualifiedFieldName(table, result);
            }
            return result;
        }

        internal static bool isTablePrimaryKeyField(string table, string fld)
        {
            DataRow[] dr = SqlHelper.tablesDT.Select(string.Format("tableName = '{0}'", table));
            bool isTablePrimaryKeyField = false; 
            if (dr.Count() > 0)  // Should always be 1
            {
                if (!Convert.IsDBNull(dr[0]["primaryKey"]))  // Should always be true
                {
                    if (fld == Convert.ToString(dr[0]["primaryKey"]))
                    {
                        isTablePrimaryKeyField = true;
                    }
                }
            }
            return isTablePrimaryKeyField;
        }

        internal static string getTableNameFromPrimaryKey(string fld)
        {
            DataRow[] dr = SqlHelper.tablesDT.Select(string.Format("primaryKey = '{0}'", fld));
            string tableName = "";
            if (dr.Count() > 0)  // Should always be 1
            {
                if (!Convert.IsDBNull(dr[0]["primaryKey"]))  // should always be true
                {
                    tableName = Convert.ToString(dr[0]["primaryKey"]);
                }
            }
            return tableName;
        }

        internal static bool fieldIsForeignKey(string table, string fld)
        {
            string innerJoin = getForeignTable(table, fld);
            return innerJoin != string.Empty;
        }

        internal static string getForeignTable(string table, string fld)
        {
            return getStringValueSystemFieldsField(table, fld, "innerJoin");
        }

        // Should this be Table1?
        internal static string getTable2ForeignKeyField(string table, string innerJoinTable)
        {
            //    //This gets the field in the table that is innerjoined to the innerJoinTable
            //    //Example: Table = course-term, field = course_id, innerJoin = course.  This will return course_id.
            string innerJoinField = string.Empty;
            DataRow[] dr = SqlHelper.fieldsDT.Select(string.Format("tableName = '{0}' AND innerJoin = '{1}'", table, innerJoinTable));
            if (dr.Count() > 0)  // Should always be 1 because only call if true
            {
                if (!Convert.IsDBNull(dr[0]["fieldName"]))  // should always be true
                {
                    innerJoinTable = Convert.ToString(dr[0]["fieldName"]);
                }
            }
            return innerJoinField;
        }

        internal static bool isIdField(string fld)
        {
            bool result = false;
            //    string strSql = "SELECT * FROM afdFieldData WHERE fieldName = '" + short_Renamed(fld) + "'";
            //    SqlDataReader rs = new SqlDataReader("");
            //    rs.Open(strSql, this.ClientSize.cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
            //    while (!rs.EOF)
            //    {
            //        if (Convert.ToBoolean(rs["autoKey"]))
            //        {
            //            result = true;
            //        }
            //        else if (strValue(rs, "innerJoin") != "")
            //        {
            result = true;
            //        }
            //        rs.MoveNext();
            //    }
            return result;
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

        internal static string strValue(SqlDataReader rs, int intField)
        {
            if (Convert.IsDBNull(rs.GetValue(intField)))
            {
                return "";
            }
            else
            {
                return Convert.ToString(rs.GetValue(intField)).Trim();
            }
        }
        internal static string strValue(SqlDataReader rs, string field)
        {
            int fieldInt = rs.GetOrdinal(field);
            return strValue(rs,fieldInt);
        }

        //internal static string getCorrectFieldName(SqlDataReader rs, string field)
        //ASM - Skip unless trouble arises
        internal static string getCorrectFieldName(SqlDataReader rs, string field)
        {
            return field;  // ASM
            //string newName = "";
            ////Try the regular field
            //int tempForEndVar = rs.FieldsMetadata.Count - 1;
            //for (int c = 0; c <= tempForEndVar; c++)
            //{
            //    if (rs.GetField(c).FieldMetadata.ColumnName == field)
            //    {
            //        return field;
            //    }
            //}
            ////Make field short or long and try again
            //if ((field.IndexOf('.') + 1) == 0)
            //{
            //    newName = longField(rs, field);
            //}
            //else
            //{
            //    newName = short_Renamed(field);
            //}
            ////Try again
            //int tempForEndVar2 = rs.FieldsMetadata.Count - 1;
            //for (int c = 0; c <= tempForEndVar2; c++)
            //{
            //    if (rs.GetField(c).FieldMetadata.ColumnName == newName)
            //    {
            //        return newName;
            //    }
            //}
            ////Failure
            //return "";
        }

        internal static string longField(SqlDataReader rs, string field)
        {
            //int tempForEndVar = rs.FieldsMetadata.Count - 1;
            //for (int i = 0; i <= tempForEndVar; i++)
            //{
            //    if (Strings.Len(rs.GetField(i).FieldMetadata.ColumnName) > Strings.Len(field) + 2)
            //    {
            //        if (rs.GetField(i).FieldMetadata.ColumnName.Substring(Strings.Len(rs.GetField(i).FieldMetadata.ColumnName) - Strings.Len(field)) == field)
            //        {
            //            return rs.GetField(i).FieldMetadata.ColumnName;
            //        }
            //    }
            //}
            return field; //give up
        }

        internal static string short_Renamed(string field)
        {
            return field.Substring(field.IndexOf('.') + 1);
        }


        internal static string getStringValueSystemTableField(string table, string fieldToReturn)
        {
            string result = string.Empty;
            DataRow[] dr = SqlHelper.tablesDT.Select(string.Format("tableName = '{0}'", table));
            if (dr.Count() > 0)  // Should always be 1
            {
                if (!Convert.IsDBNull(dr[0][fieldToReturn]))
                {
                    result = Convert.ToString(dr[0][fieldToReturn]);
                }
            }
            return result;
        }
        internal static bool getBoolValueSystemTableField(string table, string fieldToReturn)
        {
            bool result = false;  //Default
            DataRow[] dr = SqlHelper.tablesDT.Select(string.Format("tableName = '{0}'", table));
            if (dr.Count() > 0)  // Should always be 1
            {
                if (!Convert.IsDBNull(dr[0][fieldToReturn]))
                {
                    result = Convert.ToBoolean(dr[0][fieldToReturn]);
                }
            }
            return result;
        }
        internal static int getIntValueSystemTableField(string table, string fieldToReturn)
        {
            int result = -1;  //Default
            DataRow[] dr = SqlHelper.tablesDT.Select(string.Format("tableName = '{0}'", table));
            if (dr.Count() > 0)  // Should always be 1
            {
                if (!Convert.IsDBNull(dr[0][fieldToReturn]))
                {
                    result = Convert.ToInt32(dr[0][fieldToReturn]);
                }
            }
            return result;
        }

        internal static string getStringValueSystemFieldsField(string table, string field, string fieldToReturn) 
        { 
            string result = string.Empty;
            DataRow[] dr = SqlHelper.fieldsDT.Select(string.Format("tableName = '{0}' AND fieldName = '{1}'", table, field));
            if (dr.Count() > 0)  // Should always be 1
            {
                if (!Convert.IsDBNull(dr[0][fieldToReturn]))
                {
                    result = Convert.ToString(dr[0][fieldToReturn]);
                }
            }
            return result;
        }
        internal static int getIntValueSystemFieldsField(string table, string field, string fieldToReturn)
        {
            int result = 0;
            DataRow[] dr = SqlHelper.fieldsDT.Select(string.Format("tableName = '{0}' AND fieldName = '{1}'", table, field));
            if (dr.Count() > 0)  // Should always be 1
            {
                if (!Convert.IsDBNull(dr[0][fieldToReturn]))
                {
                    result = Convert.ToInt32(dr[0][fieldToReturn]);
                }
            }
            return result;
        }
        internal static bool getBoolValueSystemFieldsField(string table, string field, string fieldToReturn)
        {
            bool result = false;
            DataRow[] dr = SqlHelper.fieldsDT.Select(string.Format("tableName = '{0}' AND fieldName = '{1}'", table, field));
            if (dr.Count() > 0)  // Should always be 1
            {
                if (!Convert.IsDBNull(dr[0][fieldToReturn]))
                {
                    result = Convert.ToBoolean(dr[0][fieldToReturn]);
                }
            }
            return result;
        }

    }




}
