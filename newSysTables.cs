using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessFreeData2
{
    internal static class newSysTables
    {

        #region New System Tables
        //----------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------
        internal static bool tableExists(string tableName)
        {
            // bool isTableExist = cn.GetSchema("Tables").;
            //                       .AsEnumerable()
            //                    .Any(row => row[2] == tableName);
            return true;
        }

        #endregion


        #region Old System Tables
        ////---------------------------------------------------------------------------------------------------------------------------------------------
        ////---------------------------------------------------------------------------------------------------------------------------------------------
        ////SYSTEM TABLES FUNCTIONS
        ////---------------------------------------------------------------------------------------------------------------------------------------------
        ////---------------------------------------------------------------------------------------------------------------------------------------------

        //private bool createSystemDataTables(bool updating)
        //{
        //    bool result = false;
        //    int c = 0;
        //    bool oneFieldShown = false;
        //    string tableField = "";
        //    //Return true if successful, false if an error occurs
        //    //On Error GoTo errHandler  'Use this to prevent opening a connection with no system tables
        //    //Ask to exit
        //    DialogResult reply = MessageBox.Show(translation.tr("WriteAfdTableDataAndAfdFieldDataTables", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()), MessageBoxButtons.YesNo);
        //    if (reply == System.Windows.Forms.DialogResult.No)
        //    {
        //        return result;
        //    }
        //    if (tableExists("afdTableData") && !updating)
        //    {
        //        if (reply == System.Windows.Forms.DialogResult.Yes)
        //        {
        //            reply = MessageBox.Show(translation.tr("ThisWillDeleteTheOldTablesThisIsYourLastWarning", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()), MessageBoxButtons.OKCancel);
        //        }
        //        if (reply == System.Windows.Forms.DialogResult.Cancel)
        //        {
        //            return result;
        //        }
        //        DbCommand TempCommand = null;
        //        TempCommand = cn.CreateCommand();
        //        UpgradeHelpers.DB.DbConnectionHelper.ResetCommandTimeOut(TempCommand);
        //        TempCommand.CommandText = "DROP TABLE afdTableData";
        //        UpgradeHelpers.DB.TransactionManager.SetCommandTransaction(TempCommand);
        //        TempCommand.ExecuteNonQuery();
        //    }
        //    if (tableExists("afdFieldData") && !updating)
        //    {
        //        DbCommand TempCommand_2 = null;
        //        TempCommand_2 = cn.CreateCommand();
        //        UpgradeHelpers.DB.DbConnectionHelper.ResetCommandTimeOut(TempCommand_2);
        //        TempCommand_2.CommandText = "DROP TABLE afdFieldData";
        //        UpgradeHelpers.DB.TransactionManager.SetCommandTransaction(TempCommand_2);
        //        TempCommand_2.ExecuteNonQuery();
        //    }

        //    //Begin routine
        //    SqlDataReader rs = new SqlDataReader("");
        //    SqlDataReader rs2 = new SqlDataReader("");
        //    SqlDataReader rsCol = new SqlDataReader("");
        //    SqlDataReader adoRsCol = new SqlDataReader("");

        //    //Create table - may cause an error
        //    if (!updating)
        //    {
        //        if (msSql)
        //        {
        //            DbCommand TempCommand_3 = null;
        //            TempCommand_3 = cn.CreateCommand();
        //            UpgradeHelpers.DB.DbConnectionHelper.ResetCommandTimeOut(TempCommand_3);
        //            TempCommand_3.CommandText = "CREATE TABLE afdTableData(tableDataID INTEGER IDENTITY PRIMARY KEY, tableName VARCHAR(70),zOrder SMALLINT, textBox VARCHAR(70), filterID INTEGER,  noDouble BIT NOT NULL, hidden BIT NOT NULL, comboWidth SMALLINT)";
        //            UpgradeHelpers.DB.TransactionManager.SetCommandTransaction(TempCommand_3);
        //            TempCommand_3.ExecuteNonQuery();
        //            DbCommand TempCommand_4 = null;
        //            TempCommand_4 = cn.CreateCommand();
        //            UpgradeHelpers.DB.DbConnectionHelper.ResetCommandTimeOut(TempCommand_4);
        //            TempCommand_4.CommandText = "CREATE TABLE afdFieldData(autoKey BIT NOT NULL, fieldDataID INTEGER IDENTITY PRIMARY KEY, tableName VARCHAR(70), fieldName VARCHAR(70), innerJoin VARCHAR(70), showInYellow BIT NOT NULL, showInAll BIT NOT NULL, width SMALLINT , sort BIT NOT NULL, zTOa BIT NOT NULL, filterValue VARCHAR(70), addAll BIT NOT NULL,zOrder SMALLINT)";
        //            UpgradeHelpers.DB.TransactionManager.SetCommandTransaction(TempCommand_4);
        //            TempCommand_4.ExecuteNonQuery();
        //        }
        //        else if (msAccess)
        //        {
        //            DbCommand TempCommand_5 = null;
        //            TempCommand_5 = cn.CreateCommand();
        //            UpgradeHelpers.DB.DbConnectionHelper.ResetCommandTimeOut(TempCommand_5);
        //            TempCommand_5.CommandText = "CREATE TABLE afdTableData(tableDataID COUNTER NOT NULL, tableName VARCHAR,zOrder SMALLINT, textBox VARCHAR, filterID INTEGER,  noDouble LOGICAL, hidden LOGICAL, comboWidth SMALLINT)";
        //            UpgradeHelpers.DB.TransactionManager.SetCommandTransaction(TempCommand_5);
        //            TempCommand_5.ExecuteNonQuery();
        //            DbCommand TempCommand_6 = null;
        //            TempCommand_6 = cn.CreateCommand();
        //            UpgradeHelpers.DB.DbConnectionHelper.ResetCommandTimeOut(TempCommand_6);
        //            TempCommand_6.CommandText = "CREATE TABLE afdFieldData(autoKey LOGICAL, fieldDataID COUNTER  NOT NULL, tableName VARCHAR, fieldName VARCHAR, innerJoin VARCHAR, showInYellow LOGICAL, showInAll LOGICAL, width SMALLINT , sort LOGICAL, zTOa LOGICAL, filterValue VARCHAR, addAll LOGICAL,zOrder SMALLINT)";
        //            UpgradeHelpers.DB.TransactionManager.SetCommandTransaction(TempCommand_6);
        //            TempCommand_6.ExecuteNonQuery();
        //            DbCommand TempCommand_7 = null;
        //            TempCommand_7 = cn.CreateCommand();
        //            UpgradeHelpers.DB.DbConnectionHelper.ResetCommandTimeOut(TempCommand_7);
        //            TempCommand_7.CommandText = "CREATE UNIQUE INDEX pk ON afdTableData (tableDataID) With Primary";
        //            UpgradeHelpers.DB.TransactionManager.SetCommandTransaction(TempCommand_7);
        //            TempCommand_7.ExecuteNonQuery();
        //            DbCommand TempCommand_8 = null;
        //            TempCommand_8 = cn.CreateCommand();
        //            UpgradeHelpers.DB.DbConnectionHelper.ResetCommandTimeOut(TempCommand_8);
        //            TempCommand_8.CommandText = "CREATE UNIQUE INDEX pk ON afdFieldData (fieldDataID) With Primary";
        //            UpgradeHelpers.DB.TransactionManager.SetCommandTransaction(TempCommand_8);
        //            TempCommand_8.ExecuteNonQuery();
        //        }
        //        MessageBox.Show(translation.tr("TwoTablesAfdFieldDataAndAfdTableDataHaveBeenAddedToYourDatabase", "", "", "") + Environment.NewLine + translation.tr("UseTheseToFormatTheProgram", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
        //    }

        //    UpgradeHelpers.DB.TransactionManager.DeEnlist(cn);
        //    cn.Close();
        //    //UPGRADE_TODO: (7010) The connection string must be verified to fullfill the .NET data provider connection string requirements. More Information: https://docs.mobilize.net/vbuc/ewis#7010
        //    cn.Open();

        //    //Following section the same for creating and updating -- because created table has no rows and so all will be added.

        //    //Open afdFieldData table
        //    string strSql = "SELECT * FROM afdFieldData";
        //    rsCol.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //    //Open afdTableData table
        //    strSql = "SELECT * FROM afdTableData";
        //    rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);

        //    //Open the table schema
        //    //UPGRADE_WARNING: (2081) Array has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2081
        //    //UPGRADE_ISSUE: (2064) ADODB.SchemaEnum property SchemaEnum.adSchemaTables was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
        //    //UPGRADE_ISSUE: (2064) ADODB.Connection method schemaDT.OpenSchema was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
        //    SqlDataReader adoRs = schemaDT.OpenSchema(UpgradeStubs.ADODB_SchemaEnum.getadSchemaTables(), new object[] { null, null, null, "Table" }, null); //adoRs is table schema
        //                                                                                                                                                    //Add a rows to the afdTableData and afdFieldData IF IT IS NOT ALREADY PRESENT
        //    while (!adoRs.EOF)
        //    {
        //        if (Convert.ToString(adoRs["Table_Name"]) != "afdFieldData" && Convert.ToString(adoRs["Table_Name"]) != "afdTableData" && !Convert.ToString(adoRs["Table_Name"]).StartsWith("sys"))
        //        {
        //            strSql = "SELECT * FROM afdTableData WHERE TableName = '" + Convert.ToString(adoRs["Table_Name"]) + "'";
        //            rs2.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //            if (rs2.EOF)
        //            {
        //                rs.AddNew();
        //                rs["TableName"] = adoRs["Table_Name"];
        //                rs["noDouble"] = true;
        //                rs["hidden"] = false;
        //                rs["comboWidth"] = 0;
        //                rs.Update();
        //            }
        //            rs2.Close();
        //            //Open the column schema for this table
        //            strSql = "Select * FROM [" + Convert.ToString(adoRs["Table_Name"]) + "]";
        //            adoRsCol.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //            //For each table, add a row to the afdFieldData for each element of field
        //            int tempForEndVar = adoRsCol.FieldsMetadata.Count - 1;
        //            for (c = 0; c <= tempForEndVar; c++)
        //            {
        //                strSql = "SELECT * FROM afdFieldData WHERE TableName = '" + Convert.ToString(adoRs["Table_Name"]) + "' AND fieldName = '" + adoRsCol.GetField(c).FieldMetadata.ColumnName + "'";
        //                rs2.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //                if (rs2.EOF)
        //                {
        //                    rsCol.AddNew();
        //                    rsCol["tableName"] = adoRs["Table_Name"];
        //                    rsCol["fieldName"] = adoRsCol.GetField(c).FieldMetadata.ColumnName;
        //                    rsCol["width"] = 0;
        //                    rsCol["showInAll"] = false;
        //                    rsCol["showInYellow"] = false;
        //                    rsCol["addAll"] = false;
        //                    rsCol["sort"] = false;
        //                    rsCol["zTOa"] = false;
        //                    rsCol["autoKey"] = false;
        //                    rsCol["zOrder"] = 0;
        //                    //Set the autoKey field of afdFieldData
        //                    //UPGRADE_ISSUE: (2064) ADODB.Field property Fields.Properties was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
        //                    //UPGRADE_ISSUE: (2064) ADODB.Properties property Fields.Properties.Item was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
        //                    //UPGRADE_ISSUE: (2064) ADODB.Property property Fields.Properties.Value was not upgraded. More Information: https://docs.mobilize.net/vbuc/ewis#2064
        //                    if (Convert.ToBoolean(adoRsCol.GetField(c).getProperties().Item("ISAUTOINCREMENT").getValue()))
        //                    {
        //                        rsCol["autoKey"] = true;
        //                    }
        //                    rsCol.Update();
        //                }
        //                rs2.Close();
        //            }
        //            adoRsCol.Close();
        //        }
        //        adoRs.MoveNext();
        //    }
        //    adoRs.Close();

        //    //NOTE: rs and rsCol are still open

        //    //Add the two system tables to afdTableData table
        //    if (!updating)
        //    {
        //        rs.AddNew();
        //        rs["tableName"] = "afdFieldData";
        //        rs["noDouble"] = "True";
        //        rs["hidden"] = false;
        //        rs["comboWidth"] = 0;
        //        rs.Update();
        //        rs.AddNew();
        //        rs["tableName"] = "afdTableData";
        //        rs["noDouble"] = "True";
        //        rs["hidden"] = false;
        //        rs["comboWidth"] = 0;
        //        rs.Update();

        //        //Add afdTableData fields to afdFieldData
        //        int tempForEndVar2 = rs.FieldsMetadata.Count - 1;
        //        for (c = 0; c <= tempForEndVar2; c++)
        //        {
        //            rsCol.AddNew();
        //            rsCol["tableName"] = "afdTableData";
        //            rsCol["fieldName"] = rs.GetField(c).FieldMetadata.ColumnName;
        //            rsCol["width"] = 0;
        //            rsCol["showInAll"] = false;
        //            rsCol["showInYellow"] = false;
        //            rsCol["addAll"] = false;
        //            rsCol["sort"] = false;
        //            rsCol["zTOa"] = false;
        //            rsCol["autoKey"] = false;
        //            rsCol["zOrder"] = 0;
        //            if (rs.GetField(c).FieldMetadata.ColumnName == "tableDataID")
        //            {
        //                rsCol["autokey"] = true;
        //            }
        //            rsCol.Update();
        //        }

        //        //Add afdFieldData fields to afdFieldData
        //        int tempForEndVar3 = rsCol.FieldsMetadata.Count - 1;
        //        for (c = 0; c <= tempForEndVar3; c++)
        //        {
        //            rsCol.AddNew();
        //            rsCol["tableName"] = "afdFieldData";
        //            rsCol["fieldName"] = rsCol.GetField(c).FieldMetadata.ColumnName;
        //            rsCol["width"] = 0;
        //            rsCol["showInAll"] = false;
        //            rsCol["showInYellow"] = false;
        //            rsCol["addAll"] = false;
        //            rsCol["sort"] = false;
        //            rsCol["zTOa"] = false;
        //            rsCol["autoKey"] = false;
        //            rsCol["innerJoin"] = ""; //This is used below - we don't want a null
        //            if (rsCol.GetField(c).FieldMetadata.ColumnName == "fieldDataID")
        //            {
        //                rsCol["autokey"] = true;
        //            }
        //            rsCol.Update();
        //        }
        //    }

        //    //Close recordsets
        //    rs.Close();
        //    rsCol.Close();


        //    //Check that all tables have primary keys -- add a new one if need be
        //    MessageBox.Show(translation.tr("CheckingThatAllTablesHavePrimaryKeys", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
        //    bool addKeyColumnSuccess = true;
        //    strSql = "SELECT * FROM afdTableData";
        //    rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //    while (!rs.EOF)
        //    {
        //        strSql = "SELECT * FROM afdFieldData WHERE tableName = '" + strValue(rs, "tableName") + "' AND autoKey = " + getSqlTrue();
        //        rsCol.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //        if (rsCol.EOF)
        //        {
        //            reply = MessageBox.Show(translation.tr("TheTableDoesntHaveAPrimaryKey", strValue(rs, "tableName"), "", "") + Environment.NewLine + translation.tr("DoYouWantMeToAddOne", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()), MessageBoxButtons.OKCancel);
        //            if (reply == System.Windows.Forms.DialogResult.OK)
        //            {
        //                if (addKeyColumn(strValue(rs, "tableName")))
        //                {
        //                    //all is well
        //                }
        //                else
        //                {
        //                    addKeyColumnSuccess = false;

        //                    MessageBox.Show(translation.tr("ICouldNotAddTheKeyCommandFailure", "", "", ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
        //                }
        //            }
        //            if (reply != System.Windows.Forms.DialogResult.OK || addKeyColumnSuccess == false)
        //            {
        //                rs.Close();
        //                rsCol.Close();
        //                result = false;
        //                DbCommand TempCommand_9 = null;
        //                TempCommand_9 = cn.CreateCommand();
        //                UpgradeHelpers.DB.DbConnectionHelper.ResetCommandTimeOut(TempCommand_9);
        //                TempCommand_9.CommandText = "DROP TABLE afdTableData";
        //                UpgradeHelpers.DB.TransactionManager.SetCommandTransaction(TempCommand_9);
        //                TempCommand_9.ExecuteNonQuery();
        //                DbCommand TempCommand_10 = null;
        //                TempCommand_10 = cn.CreateCommand();
        //                UpgradeHelpers.DB.DbConnectionHelper.ResetCommandTimeOut(TempCommand_10);
        //                TempCommand_10.CommandText = "DROP TABLE afdFieldData";
        //                UpgradeHelpers.DB.TransactionManager.SetCommandTransaction(TempCommand_10);
        //                TempCommand_10.ExecuteNonQuery();
        //                goto cleanup;
        //            }
        //        }
        //        rsCol.Close();
        //        rs.MoveNext();
        //    }
        //    rs.Close();

        //    //Mark all obvious inner joins
        //    //Select all non-auto rows
        //    strSql = "Select * from afdFieldData where autoKey = " + getSqlFalse();
        //    rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //    while (!rs.EOF)
        //    {
        //        //See if this field matches any auto row
        //        strSql = "Select * from afdFieldData WHERE fieldName = '" + short_Renamed(strValue(rs, "fieldName")) + "'" + " AND autoKey = " + getSqlTrue();
        //        rsCol.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //        //If so, record the TABLE in the innerJoin column
        //        if (!rsCol.EOF)
        //        {
        //            rs["innerJoin"] = strValue(rsCol, "tableName");
        //            rs.Update();
        //        }
        //        rsCol.Close();
        //        rs.MoveNext();
        //    }
        //    rs.Close();


        //    //For each table, mark one showInAll and showInYellow field and sort on these columns
        //    if (!updating)
        //    {
        //        strSql = "SELECT * FROM afdFieldData ORDER BY tableName";
        //        rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //        tableField = "";
        //        while (!rs.EOF)
        //        {
        //            //Set tableField to new table
        //            if (tableField != Convert.ToString(rs["tableName"]))
        //            {
        //                oneFieldShown = false;
        //                tableField = Convert.ToString(rs["tableName"]);
        //            }
        //            //Show one field in this table
        //            if (!oneFieldShown)
        //            {
        //                if (!Convert.ToBoolean(rs["autoKey"]))
        //                {
        //                    oneFieldShown = true; //I count this field as shown even if it is an inner join -- because it probably shows something
        //                    if (strValue(rs, "innerJoin") == "")
        //                    {
        //                        rs["showInAll"] = true;
        //                        rs["showInYellow"] = true;
        //                        rs["sort"] = true;
        //                        rs.Update();
        //                    }
        //                }
        //            }
        //            else if (showField(short_Renamed(this.ClientSize.strValue(rs, "fieldName"))))
        //            {
        //                rs["showInAll"] = true;
        //                rs["showInYellow"] = true;
        //                rs.Update();
        //            }
        //            rs.MoveNext();
        //        }
        //        rs.Close();

        //        //afdTableData: Order the tables alphabetically and insert showInAll field as (default) textbox
        //        c = 1;
        //        strSql = "SELECT * FROM afdTableData ORDER BY tableName";
        //        rs.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //        while (!rs.EOF)
        //        {
        //            //Skip definition tables
        //            if (Convert.ToString(rs["tableName"]) != "afdFieldData" && Convert.ToString(rs["tableName"]) != "afdTableData")
        //            {
        //                //Order field
        //                rs["zOrder"] = c;
        //                c++;
        //                //Insert default textbox if any - non-innerJoin that is showInAll
        //                strSql = "Select * from afdFieldData WHERE tableName = '" + Convert.ToString(rs["tableName"]) + "'" + " AND showInAll = " + getSqlTrue();
        //                rsCol.Open(strSql, cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //                while (!rsCol.EOF)
        //                {
        //                    if (strValue(rsCol, "innerJoin") == "")
        //                    { //There will only be one
        //                        rs["textBox"] = rsCol["fieldName"];
        //                    }
        //                    rsCol.MoveNext();
        //                }
        //                rsCol.Close();
        //            }
        //            else
        //            {
        //                //Order the system tables - same number for both
        //                rs["zOrder"] = rs.RecordCount + 1;
        //            }
        //            rs.Update();
        //            rs.MoveNext();
        //        }
        //        rs.Close();
        //    }

        //    //Table updated successfully
        //    result = true;

        //cleanup:
        //    //UPGRADE_TODO: (1069) Error handling statement (On Error Resume Next) was converted to a pattern that might have a different behavior. More Information: https://docs.mobilize.net/vbuc/ewis#1069
        //    try
        //    {
        //        rs = null;
        //        rs2 = null;
        //        adoRs = null;
        //        rsCol = null;
        //        adoRsCol = null;
        //    }
        //    catch (Exception exc)
        //    {
        //        NotUpgradedHelper.NotifyNotUpgradedElement("Resume in On-Error-Resume-Next Block");
        //    }

        //    return result;

        //    result = false;
        //    goto cleanup;
        //    return result;
        //}
        //private bool showField(string fld)
        //{
        //    bool result = false;
        //    switch (fld)
        //    {
        //        case "????":
        //        case "??":
        //            result = true;
        //            break;
        //        //UPGRADE_NOTE: (7001) The following case (switch) seems to be dead code More Information: https://docs.mobilize.net/vbuc/ewis#7001
        //        //case "????" : 
        //        //result = true; 
        //        //break;
        //        //UPGRADE_NOTE: (7001) The following case (switch) seems to be dead code More Information: https://docs.mobilize.net/vbuc/ewis#7001
        //        //case "??" : 
        //        //result = true; 
        //        //break;
        //        //UPGRADE_NOTE: (7001) The following case (switch) seems to be dead code More Information: https://docs.mobilize.net/vbuc/ewis#7001
        //        //case "????" : 
        //        //result = true; 
        //        //break;
        //        //UPGRADE_NOTE: (7001) The following case (switch) seems to be dead code More Information: https://docs.mobilize.net/vbuc/ewis#7001
        //        //case "??" : 
        //        //result = true; 
        //        //break;
        //        //UPGRADE_NOTE: (7001) The following case (switch) seems to be dead code More Information: https://docs.mobilize.net/vbuc/ewis#7001
        //        //case "????" : 
        //        //result = true; 
        //        //break;
        //        //UPGRADE_NOTE: (7001) The following case (switch) seems to be dead code More Information: https://docs.mobilize.net/vbuc/ewis#7001
        //        //case "??" : 
        //        //result = true; 
        //        //break;
        //        //UPGRADE_NOTE: (7001) The following case (switch) seems to be dead code More Information: https://docs.mobilize.net/vbuc/ewis#7001
        //        //case "??" : 
        //        //result = true; 
        //        //break;
        //        //UPGRADE_NOTE: (7001) The following case (switch) seems to be dead code More Information: https://docs.mobilize.net/vbuc/ewis#7001
        //        //case "????" : 
        //        //result = true; 
        //        //break;
        //        //UPGRADE_NOTE: (7001) The following case (switch) seems to be dead code More Information: https://docs.mobilize.net/vbuc/ewis#7001
        //        //case "????" : 
        //        //result = true; 
        //        //break;
        //        default:
        //            result = false;
        //            break;
        //    }
        //    return result;
        //}
        //private bool addKeyColumn(string table)
        //{
        //    bool result = false;
        //    //On Error GoTo errhandler
        //    SqlDataReader rs = new SqlDataReader("");
        //    string fieldName = table + "_afdID";
        //    if (msAccess)
        //    {
        //        DbCommand TempCommand = null;
        //        TempCommand = cn.CreateCommand();
        //        UpgradeHelpers.DB.DbConnectionHelper.ResetCommandTimeOut(TempCommand);
        //        TempCommand.CommandText = "ALTER TABLE [" + table + "] ADD [" + fieldName + "] INTEGER IDENTITY ";
        //        UpgradeHelpers.DB.TransactionManager.SetCommandTransaction(TempCommand);
        //        TempCommand.ExecuteNonQuery();
        //    }
        //    else if (msSql)
        //    {
        //        DbCommand TempCommand_2 = null;
        //        TempCommand_2 = cn.CreateCommand();
        //        UpgradeHelpers.DB.DbConnectionHelper.ResetCommandTimeOut(TempCommand_2);
        //        TempCommand_2.CommandText = "ALTER TABLE [" + table + "] ADD [" + fieldName + "] INTEGER IDENTITY ";
        //        UpgradeHelpers.DB.TransactionManager.SetCommandTransaction(TempCommand_2);
        //        TempCommand_2.ExecuteNonQuery();
        //    }

        //    //Add the field to the afdFieldData table
        //    string strSql = "SELECT * FROM afdFieldData";
        //    rs.Open(strSql, this.ClientSize.cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //    rs.AddNew();
        //    rs["autoKey"] = true;
        //    rs["tableName"] = table;
        //    rs["fieldName"] = fieldName;
        //    rs["width"] = 0;
        //    rs["showInAll"] = false;
        //    rs["showInYellow"] = false;
        //    rs["addAll"] = false;
        //    rs["sort"] = false;
        //    rs["zTOa"] = false;
        //    rs["zOrder"] = 0;
        //    rs.Update();
        //    rs.Close();
        //    //Delay for a second to let this get in register (?)
        //    MessageBox.Show(translation.tr("PrimaryKeyOfIsColumn", table, fieldName, ""), AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
        //    rs = null;
        //    return true;

        //    rs = null;
        //    MessageBox.Show(translation.tr("SorryUnableToAddPrimaryKeyAtThisTime", "", "", "") + Environment.NewLine + Information.Err().Description, AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
        //    return result;
        //}
        //internal void updateFieldDataTableUnknown(string field, string systemTableField, string NewValue, DbType fldType)
        //{
        //    SqlDataReader rs = null;
        //    string strSql = "";
        //    //Update afdFieldData for all tables with this field
        //    if (tableExists("afdFieldData"))
        //    {
        //        rs = new SqlDataReader("");
        //        strSql = "SELECT * FROM afdFieldData where fieldName = '" + field + "'";
        //        rs.Open(strSql, this.ClientSize.cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //        while (!rs.EOF)
        //        {
        //            updateFieldSystemTable(strValue(rs, "tableName"), field, systemTableField, NewValue, fldType);
        //            rs.MoveNext();
        //        }
        //    }
        //}
        //private void updateTableSystemTable(string table, string tableProperty, string NewValue, DbType fldType)
        //{
        //    updateSystemTable("afdTableData", table, "", tableProperty, NewValue, fldType);
        //}
        //private void updateFieldSystemTable(string table, string field, string property, string NewValue, DbType fldType)
        //{
        //    updateSystemTable("afdFieldData", table, field, property, NewValue, fldType);
        //}

        //private void updateSystemTable(string systemTableName, string table, string field, string property, string NewValue, DbType fldType)
        //{
        //    SqlDataReader rs = null;
        //    string strSql = "", errMsg = "";
        //    //Find the table-field column in the afdFieldData and update the systemTableField of that row to New Value (perhaps filterValue to xxx)
        //    //or Find the table column i the afdTableData and update the ...
        //    //Get the correct record of the correct table
        //    if (tableExists(systemTableName))
        //    {
        //        rs = new SqlDataReader("");
        //        if (systemTableName == "afdFieldData")
        //        {
        //            strSql = "SELECT * FROM afdFieldData where tableName = '" + table + "' AND fieldName = '" + field + "'";
        //            errMsg = "Error in afdFieldData table. No row with tableName = '" + table + "' and fieldName = '" + field + "'. Nothing updated";
        //        }
        //        else
        //        {
        //            strSql = "SELECT * FROM afdTableData where tableName = '" + table + "'";
        //            errMsg = "Error in afdTableData table. No row with tableName = '" + table + "'. Nothing updated";
        //        }
        //        rs.Open(strSql, this.ClientSize.cn, UpgradeHelpers.DB.LockTypeEnum.LockOptimistic);
        //        //Update the record
        //        if (!rs.EOF)
        //        {
        //            writeLog("ChangeBefore", rs);
        //            rs[property] = NewValue;
        //            rs.Update();
        //            writeLog("ChangeBefore", rs);
        //        }
        //        else
        //        {
        //            MessageBox.Show(errMsg, AssemblyHelper.GetTitle(System.Reflection.Assembly.GetExecutingAssembly()));
        //        }
        //        rs.Close();
        //        rs = null;
        //    }
        //}

        #endregion





    }
}
