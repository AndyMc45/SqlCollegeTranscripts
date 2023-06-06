using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Data.SqlClient;

namespace SqlCollegeTranscripts
{
    internal static class TaColNames
    {
        // tables
        internal static string coursesTable = "Courses";
        internal static string courseTermsTable = "CourseTerms";
        internal static string degreeLevelTable = "DegreeLevel";
        internal static string degreesTable = "Degrees";
        internal static string deliveryMethodTable = "DeliveryMethod";
        internal static string departmentsTable = "Departments";
        internal static string facultyTable = "Faculty";
        internal static string gradesTable = "Grades";
        internal static string gradRequirementsTable = "GradRequirements";
        internal static string handbooksTable = "Handbooks";
        internal static string requirementsTable = "Requirements";
        internal static string requirmentNameTable = "RequirementName";
        internal static string requirments_MapTable = "Requirements_Map";
        internal static string studentDegreesTable = "StudentDegrees";
        internal static string studentGradReqTable = "StudentGradReq";   // 
        internal static string studentsTable = "Students";
        internal static string studentDegreeTable = "StudentDegrees";   // 
        internal static string termsTable = "Terms";
        internal static string transcriptTable = "Transcript";
        internal static string transferCreditsTable = "TransferCredits";


        // Non-FK Columns in tables tat are used - 
        internal static field Student_StudentName = dataHelper.getFieldFromFieldsDT(studentsTable, "studentName");
        internal static field Degrees_DegreeName = dataHelper.getFieldFromFieldsDT(degreesTable, "degreeName" );

        // Needed in filling studentGradReqTable
        internal static field CourseTerms_Credits = dataHelper.getFieldFromFieldsDT(courseTermsTable, "credits" );
        internal static field DegreeLevel_DegreeLevel = dataHelper.getFieldFromFieldsDT(degreeLevelTable, "degreeLevel" );
        internal static field DeliveryMethod_level = dataHelper.getFieldFromFieldsDT(deliveryMethodTable, "deliveryLevel" );
        internal static field ReqName_xTimes = dataHelper.getFieldFromFieldsDT(requirmentNameTable, "xTimes");

        internal static field Grades_QP = dataHelper.getFieldFromFieldsDT(gradesTable, "QP" );
        internal static field Grades_earnedCredits = dataHelper.getFieldFromFieldsDT(gradesTable, "earnedCredits" );
        internal static field Grades_creditsInQPA = dataHelper.getFieldFromFieldsDT(gradesTable, "creditsInQPA" );
        internal static field GradRequirements_reqCreditsOrTimes = dataHelper.getFieldFromFieldsDT(gradRequirementsTable, "reqCreditsOrTimes" );
        internal static field GradRequirements_reqEqDmCredits = dataHelper.getFieldFromFieldsDT(gradRequirementsTable, "reqEqDmCredits" );
        internal static field GradRequirements_creditLimit = dataHelper.getFieldFromFieldsDT(gradRequirementsTable, "creditLimit" );
        internal static field RequirementsMap_req_AncestorID = dataHelper.getFieldFromFieldsDT(requirments_MapTable, "req_AncestorID" );
        internal static field RequirementsMap_req_DescendantID = dataHelper.getFieldFromFieldsDT(requirments_MapTable, "req_DescendantID" );

        // Columns that are Updated
        internal static field SGRT_crReq = dataHelper.getFieldFromFieldsDT(studentGradReqTable, "crReq" );
        internal static field SGRT_crEarned = dataHelper.getFieldFromFieldsDT(studentGradReqTable, "crEarned" );
        internal static field SGRT_crInProgress = dataHelper.getFieldFromFieldsDT(studentGradReqTable, "crInProgress" );
        internal static field SGRT_crLimit = dataHelper.getFieldFromFieldsDT(studentGradReqTable, "crLimit" );
        internal static field SGRT_crUnused = dataHelper.getFieldFromFieldsDT(studentGradReqTable, "crUnused" );
        internal static field SGRT_crEqDmReq = dataHelper.getFieldFromFieldsDT(studentGradReqTable, "crEqDmReq" );
        internal static field SGRT_crEqDmErnd = dataHelper.getFieldFromFieldsDT(studentGradReqTable, "crEqDmErnd" );
        internal static field SGRT_crEqDmInPro = dataHelper.getFieldFromFieldsDT(studentGradReqTable, "crEqDmInPro" );
        internal static field SGRT_QP_total = dataHelper.getFieldFromFieldsDT(studentGradReqTable, "QP_total" );
        internal static field SGRT_QP_credits = dataHelper.getFieldFromFieldsDT(studentGradReqTable, "QP_credits" );
        internal static field SGRT_QP_average = dataHelper.getFieldFromFieldsDT(studentGradReqTable, "QP_average" );
        internal static field SGRT_Fulfilled = dataHelper.getFieldFromFieldsDT(studentGradReqTable, "Fulfilled" );
    }
    internal class Transcript
    {
        public Transcript(int studentDegreeID)
        {
            this.studentDegreeID = studentDegreeID;
            this.studentName = string.Empty;
            this.degreeName = string.Empty;
            this.errorMsgs = new List<string>();
            studentDegreesDataRow = getStudentDegreeDataRow(); // May change errorMsg
        }
        public Transcript(string studentName, string degreeName)
        {
            this.studentName = studentName; 
            this.degreeName = degreeName;
            this.studentDegreeID = 0;
            this.errorMsgs = new List<string>();
            studentDegreesDataRow = getStudentDegreeDataRow(); // May change errorMsg
        }

        public DataTable transcriptDT { get; set; }  // No editing
        public DataRow studentDegreesDataRow { get; set; } // No editing 
        public DataTable gradRequirementsDT { get; set; } // No editing

        public string studentName { get; set; }
        public string degreeName { get; set; }
        public int studentDegreeID { get; set; }
        public List<string> errorMsgs { get; set; }

        SqlFactory transcriptSql = new SqlFactory(TaColNames.transcriptTable, 0, 0, true);
        SqlFactory studentDegreesSql = new SqlFactory(TaColNames.studentDegreesTable, 0, 0, true);

        private DataRow getStudentDegreeDataRow() // Also sets studentDegreeID if not set
        {
            // Add wheres
            if (studentDegreeID != 0)
            {
                field fld = dataHelper.getTablePrimaryKeyField(TaColNames.studentDegreesTable);
                where wh = new where(fld, studentDegreeID.ToString());
                studentDegreesSql.myWheres.Add(wh);
            }
            else if( ! String.IsNullOrEmpty(degreeName) && ! String.IsNullOrEmpty(studentName)) 
            {
                where wh1 = new where(TaColNames.Student_StudentName, studentName);
                studentDegreesSql.myWheres.Add(wh1);
                where wh2 = new where(TaColNames.Degrees_DegreeName, degreeName);
                studentDegreesSql.myWheres.Add(wh2);
            }
            // Get data row
            string sqlString = studentDegreesSql.returnSql(command.selectAll);
            using (DataTable tempDT = new DataTable())
            {
                string strError = MsSql.FillDataTable(tempDT, sqlString);
                if (strError != string.Empty) { errorMsgs.Add(strError); }
                if (tempDT.Rows.Count > 0)
                {
                    if (studentDegreeID == 0)   // "StudentName, degree" Constructor sets to 0.
                    { 
                        field pkFld = dataHelper.getTablePrimaryKeyField(TaColNames.studentDegreeTable);
                        int pkIndex = tempDT.Rows[0].Table.Columns[pkFld.fieldName].Ordinal;  // Probably always 0
                        studentDegreeID = (int)tempDT.Rows[0].ItemArray[pkIndex];
                    } 
                    return tempDT.Rows[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public void fillTranscriptTable()
        {
            if (studentDegreesDataRow != null)  // Trust all is O.K. if studentDegreesDataRow is set
            {
                field fkStudentDegreeID = 
                    dataHelper.getForeignKeyFromRefTableName(TaColNames.transcriptTable, TaColNames.studentDegreeTable);
                where wh = new where(fkStudentDegreeID, studentDegreeID.ToString());
                transcriptSql.myWheres.Add(wh);
                string sqlString = transcriptSql.returnSql(command.selectAll);
                transcriptDT = new DataTable();
                string strError = MsSql.FillDataTable(transcriptDT, sqlString);
                if (strError != string.Empty) { errorMsgs.Add("ERROR in fillTranscriptTable (Transcript.cs): " + strError); }
            }
        }

        public void fillGradRequirementsDT()
        {
            if (studentDegreesDataRow != null)
            { 
                Dictionary<int,List<int>> requirement_fullmap = new Dictionary<int,List<int>>();

                //1. Get old records for this student (put in sgrDaDt) -- added these last time we printed his/her transcript
                SqlFactory studentGradReqSql = new SqlFactory(TaColNames.studentGradReqTable, 0, 0);
                field fkSGR_StudentDegreeID = dataHelper.getForeignKeyFromRefTableName(TaColNames.studentGradReqTable, TaColNames.studentDegreeTable);
                where wh_Sgr_SdID = new where(fkSGR_StudentDegreeID, studentDegreeID.ToString());
                studentGradReqSql.myWheres.Add(wh_Sgr_SdID);
                string sqlString = studentGradReqSql.returnSql(command.selectAll);
                MsSqlWithDaDt sgrDaDt = new MsSqlWithDaDt(sqlString);
                string strError = sgrDaDt.errorMsg;
                if (strError != string.Empty) { errorMsgs.Add("ERROR in fillGradRequirementsDT (Transcript.cs): " + strError); }

                //2. Delete these old records from sgrDaDt / and push deletes down to studentGradReqTable
                MsSql.SetDeleteCommand(studentGradReqSql.myTable, sgrDaDt.da); // delete rows based on primary key
                MsSql.DeleteRowsFromDT(sgrDaDt.dt, wh_Sgr_SdID); //wh used to select rows to delete, and then deletes (based on pk of selected rows)

                //3. Get GradRequirements records for this student / degree / handbook (Place in gradRequirementDT - not edited)
                SqlFactory GradRequirementsSql = new SqlFactory(TaColNames.gradRequirementsTable, 0, 0);
                // Get DegreeID where value (from studentDegreesDataRow)
                field StuDegree_DegreeID = dataHelper.getForeignKeyFromRefTableName(TaColNames.studentDegreeTable, TaColNames.degreesTable);
                string myDegreeID = studentDegreesSql.getStringValueFromDataRowBasefield(studentDegreesDataRow, StuDegree_DegreeID);
                // Get DegreeID Where field in GradReq table
                field GradReq_DegreeID = dataHelper.getForeignKeyFromRefTableName(TaColNames.gradRequirementsTable, TaColNames.degreesTable);
                // Create and add the where (i.e. where GradReq.DegreeID = this student degreeID)
                where whDegreeID = new where(GradReq_DegreeID, myDegreeID);
                GradRequirementsSql.myWheres.Add(whDegreeID);  

                // Get HandbookID where and add - same as above
                field fk_SD_HandbookID = dataHelper.getForeignKeyFromRefTableName(TaColNames.studentDegreeTable, TaColNames.handbooksTable);
                string myHandbookID = studentDegreesSql.getStringValueFromDataRowBasefield(studentDegreesDataRow, fk_SD_HandbookID);
                field fkHandbookID = dataHelper.getForeignKeyFromRefTableName(TaColNames.gradRequirementsTable, TaColNames.handbooksTable);
                where whHandbookID = new where(fkHandbookID, myHandbookID);
                GradRequirementsSql.myWheres.Add(whHandbookID);

                // Get rows in GradRequirement table filtered by above two wheres - and fill gradRequiremensDT with these rows
                string sqlString2 = GradRequirementsSql.returnSql(command.selectAll);
                gradRequirementsDT = new DataTable();
                string errorMsg2 = MsSql.FillDataTable(gradRequirementsDT, sqlString2);
                if (errorMsg2 != string.Empty) { errorMsgs.Add("ERROR in fillGradRequirements 2 (Transcript.cs): " + errorMsg2); }

                //4a. Insert a corresponding row in studentGradReq - NOTE: studentGradReq was emptied above, now replacing these rows 
                field fk_studgradReq_gradReqID = dataHelper.getForeignKeyFromRefTableName(TaColNames.studentGradReqTable, TaColNames.gradRequirementsTable);
                field pkGradReqTable = dataHelper.getTablePrimaryKeyField(TaColNames.gradRequirementsTable); 

                foreach (DataRow dr in gradRequirementsDT.Rows)  // Rows in GradRequirement Table
                {
                    List<where> whList = new List<where>();
                    whList.Add(wh_Sgr_SdID); // Defined above
                    string pkGradReqTable_value = GradRequirementsSql.getStringValueFromDataRowBasefield(dr, pkGradReqTable); // Get GradRequirement ID
                    where wh_GradReqID = new where(fk_studgradReq_gradReqID, pkGradReqTable_value);  // Used to insert into studGradReq table
                    whList.Add(wh_GradReqID);
                    // Insert row into studentGradReqTable with only StudentDegreeID and gradReqID foreign key filled
                    MsSql.SetInsertCommand(TaColNames.studentGradReqTable, whList, sgrDaDt.da);
                    sgrDaDt.da.InsertCommand.ExecuteNonQuery();
                }
                // 4b. Reload sgrDaDt into memory with new values from studentGradReq table - i.e. the rows inserted in 4a
                studentGradReqSql.myWheres.Clear();
                studentGradReqSql.myWheres.Add(wh_Sgr_SdID);
                sqlString = studentGradReqSql.returnSql(command.selectAll);
                sgrDaDt.dt = new DataTable();
                strError = MsSql.FillDataTable(sgrDaDt.dt, sqlString);
                if (strError != string.Empty) { errorMsgs.Add("ERROR in fillGradRequirementsDT (Transcript.cs): " + strError); }

                //5a.  Set update command on extraDA - (Note: still using sgrDaDt.dt and da - now with rows that have only stuDegreeID and gradReqID  .)
                List<field> updateFields = new List<field>();
                updateFields.Add(TaColNames.SGRT_crReq);
                updateFields.Add(TaColNames.SGRT_crEarned);
                updateFields.Add(TaColNames.SGRT_crInProgress);
                updateFields.Add(TaColNames.SGRT_crLimit);
                updateFields.Add(TaColNames.SGRT_crUnused);
                updateFields.Add(TaColNames.SGRT_crEqDmReq);
                updateFields.Add(TaColNames.SGRT_crEqDmErnd);
                updateFields.Add(TaColNames.SGRT_crEqDmInPro);
                updateFields.Add(TaColNames.SGRT_QP_total);
                updateFields.Add(TaColNames.SGRT_QP_credits);
                updateFields.Add(TaColNames.SGRT_QP_average);
                MsSql.SetUpdateCommand(updateFields, sgrDaDt.dt);

                //5c. Fill rows of sgrDaDt.dt (studentGradReqTable for this student) from the rows in transcriptDT
                field pkTranscripts = dataHelper.getTablePrimaryKeyField(TaColNames.transcriptTable);
                foreach (DataRow transDR in transcriptDT.Rows)
                {
                    // 0. Primary key - for use in noting errors
                    int pkThisTransDR = Int32.Parse(transcriptSql.getStringValueFromDataRowBasefield(transDR, pkTranscripts, TaColNames.transcriptTable));
                    // 1. Get all the information we need from transDR
                    // Get foreign key we are looking for
                    field fkFieldToFind = dataHelper.getForeignKeyFromRefTableName(TaColNames.coursesTable, TaColNames.requirementsTable);
                    int courseReqIdValue = transcriptSql.getIntValueFromDataRowBasefield(transDR, fkFieldToFind, TaColNames.coursesTable);
                    bool xTimes = transcriptSql.getBoolValueFromDataRowBasefield(transDR, TaColNames.ReqName_xTimes, TaColNames.requirementsTable);
                    // Get two degreeLevels to make sure course is as high as this degree degreeLevel - otherwise credits wasted
                    int sdDegreeLevel = transcriptSql.getIntValueFromDataRowBasefield(transDR, TaColNames.DegreeLevel_DegreeLevel, TaColNames.studentDegreesTable);
                    int courseTermDegreeLevel = transcriptSql.getIntValueFromDataRowBasefield(transDR, TaColNames.DegreeLevel_DegreeLevel, TaColNames.courseTermsTable);
                    // Get two DeliveryMethod_levels to check totals on delivery-methods
                    // Following uses distance from transcript to deliveryMethod in stack to pick the short keyStack
                    int courseDeliveryLevel = transcriptSql.getIntValueFromDataRowBasefield(transDR, TaColNames.DeliveryMethod_level, TaColNames.transcriptTable);
                    int stuDegDeliveryLevel = transcriptSql.getIntValueFromDataRowBasefield(transDR, TaColNames.DeliveryMethod_level, TaColNames.studentDegreesTable);
                    // Get grade details  -  Grades table columns: int: QP, bit: earnedCredits, bit: creditsInQPA
                    Single gradeQP = transcriptSql.getSingleValueFromDataRowBasefield(transDR, TaColNames.Grades_QP, TaColNames.transcriptTable);
                    bool grade_earnedCredits = transcriptSql.getBoolValueFromDataRowBasefield(transDR, TaColNames.Grades_earnedCredits, TaColNames.transcriptTable);
                    bool grade_creditsInQPA = transcriptSql.getBoolValueFromDataRowBasefield(transDR, TaColNames.Grades_creditsInQPA, TaColNames.transcriptTable);
                    // Get number of credits this course is worth
                    Single courseCredits = transcriptSql.getSingleValueFromDataRowBasefield(transDR, TaColNames.CourseTerms_Credits, TaColNames.courseTermsTable);

                    // 2. Add the requirement to requirements map, if this reqID is not already in the map
                    if (!requirement_fullmap.Keys.Contains(courseReqIdValue))
                    {
                        List<int> fullMap = new List<int>();
                        getlistOfRequirementsFulfilledBy(courseReqIdValue, ref fullMap); 
                        requirement_fullmap.Add(courseReqIdValue, fullMap);
                    }

                    // 3. Insert this information in dataHelper.extraDT, and then push down to studentGradRequirments (the table in extraDT)
                    //    Loop through stuGradReq table and add this transcript row if it meets this requirement 
                    foreach (DataRow stuGradReqDR in sgrDaDt.dt.Rows)
                    {
                        // fk_studgradReq_gradReqID defined above when inserting values into sgrt (Just getting FK field name in sgrt - probabaly 'requirementID')
                        int sgrt_reqID = studentGradReqSql.getIntValueFromDataRowBasefield(stuGradReqDR, fk_studgradReq_gradReqID);
                        // Check if this transcript fulfills the requirement of this row of the student's stuGradReq table
                        if (requirement_fullmap[courseReqIdValue].Contains(sgrt_reqID))
                        {
                            UpdateStuGradReqDR(stuGradReqDR, sdDegreeLevel, courseTermDegreeLevel, stuDegDeliveryLevel, courseDeliveryLevel,
                                gradeQP, grade_earnedCredits,grade_creditsInQPA , courseCredits, xTimes);
                        }
                    }
                }
            }
        }

        private void UpdateStuGradReqDR(DataRow sgrDR, int sdDegreeLevel, int tDegreeLevel, int sdDeliveryLevel, int tDeliveryLevel,
                    Single grade_QP, bool grade_EarnedCredits, bool grade_creditsInQPA, Single credits, bool xTimes)
        { 
            if(sdDegreeLevel ! == tDeliveryLevel) 
            {
                errorMsgs.Add("Error");
            }
        }

        // Get list of requirements fulfilled by this requirement
        private void getlistOfRequirementsFulfilledBy(int reqID, ref List<int> returnList)
        { 
            if(!returnList.Contains(reqID)) { 
                returnList.Add(reqID); // Fulfills itself
            }
            SqlFactory requirementMapSql = new SqlFactory(TaColNames.requirments_MapTable, 0, 0);
            field ancestorFld = TaColNames.RequirementsMap_req_AncestorID;
            where wh = new where(ancestorFld, reqID.ToString());
            requirementMapSql.myWheres.Add(wh);
            string sqlString = requirementMapSql.returnSql(command.selectAll);
            DataTable dt = new DataTable();
            MsSql.FillDataTable(dt, sqlString);
            foreach(DataRow dr in dt.Rows)
            {
                int descendantReqCol = dr.Table.Columns.IndexOf(TaColNames.RequirementsMap_req_DescendantID.fieldName);
                int descendantReq = Int32.Parse(dr[descendantReqCol].ToString());
                if (!returnList.Contains(descendantReq))
                {
                    getlistOfRequirementsFulfilledBy(descendantReq, ref returnList);
                }
            }
        }

    }

}
