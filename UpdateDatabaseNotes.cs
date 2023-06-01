using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlCollegeTranscripts
{
    internal class UpdateDatabaseNotes
    {
        /*
First, make the following changes using SSMS  (sql files found in Documents > SQL Server management Studio)
1.  Drop constraints - this drops all the "non-zero" constraints

2.  Change Column names - changes column names to English - we have sql file for this

3.  Change Table names - changes tables name to English - we have sql file for this

4.  Drop 3 "answer - question" tables  (? if sql file given)

5.  Do the following (no sql file given)
    a.  Drop "non-student" table
    b.  Add All Requirement tables - You can generate this code in SQL Server Management Studio
    c.  Add new Departments table - Add values for short names:  NT, OT, PT, TH, CH, BC, --
    d.  Drop Primary keys that have chinese names and then re-add (or just change the name)
   'Backup database at every drop or change content step'

Next Create all ForeignKeys and  Display Keys - important to do to prevent deleting things you should not delete
Next Create all ForeignKeys and  Display Keys.  Use tools

1.  Under tools, you can check if a proposed non-Foreignkey can become a foreign key.

2.  Under tools & context menu, you can check if proposed "Display key" list is unique - (manually check is_DK for proposed DK list)

3.  For duplicates in CourseTerms, add a column called "section".
    THEN, CHECK 'CREDITS' and "FACULTY" (as well as course, term, section) as DK and merge these; 
    FINALLY, divide things with different faculty or credits into sections using the following SQL

    USE [CrtsTranscript_2007_Test]
    GO
    WITH CTE AS
    (Select CourseTerms.courseID, CourseTerms.section as sec, ROW_NUMBER() 
    OVER  (PARTITION BY CourseTerms.courseID, CourseTerms.termID Order by CourseTerms.CourseTermID) RN  From CourseTerms)

    UPDATE CTE
    SET CTE.sec = RN

    GO

4.  There is also a table option that allows you to edit display keys - which can eliminate a duplicate


Other things to do
5.  Replace the Departs_Old table with Requirements table  (and at the very end delete Departs_Old Table 
    a. Add new DepartmentID column to Courses and give short name of deparment for each course.
       To do this run following script in Sql Sever Managment studio, and then set this as FK and DK.
       (Assumes we have created new Departments Table - simple table with 7 rows as described above) 
        USE [CrtsTranscript_2007_Test]
        GO
        UPDATE [dbo].[Courses]
            SET [deparmentID] = [Departments].DepartmentID
            FROM COURSES Inner Join Departs_Old on Courses.depart_oldID = Departs_Old.depart_oldID
				          Inner Join Departments on Departs_Old.departmentName = Departments.Dep_ShortName
            WHERE Departments.Dep_ShortName = Departs_Old.departmentName
        GO

    b. Transfer old requirements from Departs_Old to new requirement table - (eventually will delete old_ReqID column)
        (The Departs_Old is really a requirements table.)
        INSERT INTO Requirements
			([req_name]
           ,[e_req_name]
           ,[degreeLevelID]
		   ,[old_ReqID] )
        SELECT 
			Concat_WS('-',departmentName,requiredCourse),
			Concat_WS('-',departmentName,eRequiredCourse),
			'2',
			depart_oldID
        FROM [dbo].[Departs_Old]

    c. Copy these rows into Requirements again, but with 3 as degreeLevelID - i.e. graduate
        INSERT INTO [dbo].[Requirements]
           ([req_name]
           ,[e_req_name]
           ,[degreeLevelID]
           ,[old_ReqID])
        Select 
           [Requirements].[req_name]
           ,[Requirements].[e_req_name]
           ,'3'
           ,[old_ReqID]
        FROM
	        Requirements

    d. Add New RequirementID column to Grad_Requirement, and make it "match" the depart_OldID requirement

        UPDATE [dbo].[GradRequirements]  
        SET GradRequirements.requirementID = 
        (
            Select Requirements.requirementID From Requirements
	        inner join Departs_Old on Requirements.old_ReqID = Departs_Old.depart_oldID
            Where GradRequirements.departmentID = Departs_Old.depart_oldID
	    ) 
    
        (Transfer credit table should be done here with similar logic for departments - see below step p.)

    e.  Add degreeLevelID as FK to COURSETERMS, DEGREES)  (Set these = old 'graduateCourse' 'graduateDegree'.) 

        UPDATE [dbo].[CourseTerms]
        SET CourseTerms.DegreeLevelID =
        CASE WHEN  CourseTerms.graduateCourse = 'True' THEN 2   -- CHANGE TO MASTER LEVEL
        Else 3 END   -- CHANGE TO UNDERGRAD LEVEL

        UPDATE [dbo].[Degrees]
        SET Degrees.degreeLevelID =
        CASE WHEN  Degrees.graduateDegree = 'True' THEN 2   -- CHANGE TO MASTER LEVEL ID
        Else 3 END   -- CHANGE TO UNDERGRAD LEVEL

    f. Update the GradRequirement requirementID based on the degree (2 or 3)

        I have two sets of requirments, the first set for Masters, the second set for undergrad.
        Set these Gradrequirements all to the first set, i.e. masters
        Now change the GradRequirements for a non-Masters degree to the second set with the following:.

        UPDATE [dbo].[GradRequirements] 
        SET GradRequirements.requirementID =  R3.requirementID
        FROM GradRequirements INNER JOIN Requirements as R2 On GradRequirements.requirementID = R2.requirementID
		    Inner Join Requirements r3 On R3.req_name = R2.req_name
		    Inner Join Degrees on GradRequirements.degreeID = Degrees.degreeID
	        Where Degrees.degreeLevelID = 3  AND NOT R3.requirementID = GradRequirements.requirementID -- R3 is the Bachelor

   'Backup database after any permanent change or a few non-permanent changes'

		
    g. Push the DegreeLevelID down from CourseTerms to Courses.
       (Use same stratagy as above.)
        g1.  Add DegreeLevelID - set default as 3
                Update to 2 if "gradudateCourse" = 'true'
            
        g2. Add degreeLevelID to courses - set default as 3

        g3. Double the number of courses with the second set seting degreeLevelID to 2

        USE [CrtsTranscript_2007_Test]
        GO
        INSERT INTO [dbo].[Courses]
           ([courseName]
           ,[degreeLevelID]
           ,[eCourseName]
           ,[deparmentID]
           ,[depart_oldID]
           ,[note]
           ,[section])
        Select
           [courseName]
           , '3'
           ,[eCourseName]
           ,[deparmentID]
           ,[depart_oldID]
           ,[note]
           ,[section]
	    FROM Courses
        GO
               
        
        g4.  Map CourseTerms.CourseID to Course with correct requirement, i.e. requirement with degreeLevel 2 for grads and requirement with 3 for BA
        USE [CrtsTranscript_2007_Test]
        GO
        -- Assume original courses are all labeled 2, i.e. grad courses, and select a course with same name and department but on level 3, i.e. BA 
        UPDATE [dbo].[CourseTerms]  
        SET CourseTerms.courseID = 
        c3.courseID
		FROM CourseTerms INNER JOIN Courses AS c2  on CourseTerms.CourseID = c2.courseID
			Inner Join Requirements  on c2.depart_oldID = Requirements.old_ReqID
			Inner Join Courses c3 on c2.courseName = c3.CourseName	    
			WHERE c3.depart_oldID = c2.depart_oldID AND CourseTerms.degreeLevelID = '3' and c3.degreeLevelID = '3'

        GO
         
    h.  Set Courses.RequirmentID from OLD ID and course Level
        (Missing Sql - but not hard)    

    i.  Delete unused FK in Requirements and in Courses  (Use ^U and Delete)

   'Backup database after any permanent change or a few non-permanent changes'


    j.  Transfer listingCredit to note and delete column
        USE [CrtsTranscript_2007_Test]
        GO
        UPDATE [dbo].[transcript]
           SET [note] = 
           CASE WHEN   transcript.listeningCredit = 'True' THEN Concat('Listen Credit ', note)
           Else transcript.note END
        GO

    k.  Added "GradeStatus"
        k1.  This is FK in transcript table - options (in statusKey column): "forCredit, auditting, dropped, withdrawn, etc.
        k2.  Use following to set the status based on the audit column of transcripts - then delete audit column.
            'Backup database at every step'

        UPDATE [dbo].[transcript]
        SET [gradeStatusID] = 2   -- Must pick "audit" gradeStatusID
        FROM transcript
        Where transcript.auditing = 'True'

    l.  Added "RequirementStatus
        l1.  This is a FK of the requirement table - the name of the requirement should reflect this status
        12.  Examples (in statusKey column):  online, onlineSimultanius, inPerson, DVD, directedStudy
            (GradRequirments could be "60 online credits", "60 onlineSimulanius or inPerson credits")
        13.  Use following to update and then delete 'self-study' column in transcripts
            'Backup database at every step'

        USE [CrtsTranscript_2007_Test]
        GO
        UPDATE [dbo].[transcript]
        SET [requirementStatusID] = 2
        FROM transcript
        Where transcript.selfStudyCourse = 'True'
        GO

        USE [CrtsTranscript_2007_Test]
        GO
        UPDATE [dbo].[transcript]
        SET [requirementStatusID] = 4
        FROM transcript
        where note Like '%DVD%'
        GO

    M. Create table "RequirementNames" and fill it with the following - 34 rows
        (Could do this earlier.)
        USE [CrtsTranscript_2007_Test]
        GO
        INSERT INTO [dbo].[RequirementName]
                   ([name]
                   ,[e_name])
        Select  DISTINCT
		        Requirements.req_name,
		        Requirements.e_req_name
        FROM	Requirements
        GO

        USE [CrtsTranscript_2007_Test]
        GO
        UPDATE [dbo].[Requirements]
           SET [requirementNameID] = RequirementName.requirementNameID
           From RequirementName
         WHERE Requirements.req_name = RequirementName.name AND Requirements.e_req_name = RequirementName.e_name
        GO



    M2.  And then add RequirementNames as FK to Requirments, and then delete two columns from Requrirements



    N.  Add Handbooks table.  Insert rows with
        INSERT INTO [dbo].[Handbooks]
           ([handbook])
        Select  DISTINCT
		        GradRequirements.yearbook
        FROM	GradRequirements

        Update GradRequirements and studentDegrees with handbookID column using

        UPDATE [dbo].[GradRequirements]
        SET  [handbookID] = 
        (Select Handbooks.handbookID From Handbooks Where HandBooks.handbook = GradRequirements.yearbook)
   
        StudentDegrees yearbook has various errors, so before updating we must
        1.  Add "Unknown" as a value in new Handbooks table (probably ID 8)
            Add "1997" as a value in new Handbooks table

        2.  Use sqlEditor to change all values with leading spaces in "yearbook" - first change to 1000 then back to original value

        To find other problem rows use
        select yearbook from studentDegrees WHERE NOT EXISTS ( Select handbook FROM Handbooks Where handbook = yearbook)

        UPDATE StudentDegrees
        SET yearBook = 'Unknown' Where yearbook is null

        UPDATE StudentDegrees
        SET yearBook = 'Unknown' Where yearbook = '80'

        UPDATE StudentDegrees
        SET yearBook = '2008' Where yearbook = '2009'

        UPDATE StudentDegrees
        SET yearBook = '2008' Where yearbook = '2009'

        UPDATE StudentDegrees
        SET yearbook = '2012' Where yearbook = '2011' OR yearbook = '2013'

        Once there are no problem rows, run
        UPDATE [dbo].[StudentDegrees]
        SET  [handbookID] = 
        (Select Handbooks.handbookID From Handbooks Where HandBooks.handbook = StudentDegrees.yearbook)

    o.  Add "deliveryMethodID" to degrees - for each degree, update by hand in SqlEditor
        Add online degrees to degree table - Ask school to switch studentdegree to these for all online students 

    p.  Transfer credits
        1.  Delete "FulfillsGradRequirements from Grades table
        2.  Transfer table: 
            a. Add requirementID.  Run (this grabs the first because we have already added 2 - 1 for Master and 1 for BA

                UPDATE [dbo].[TransferCredits]
                SET 
                  [requirementID] = 
		            (
			            Select Top 1 Requirements.requirementID From Requirements
			            inner join Departs_Old on Requirements.depart_OldID = Departs_Old.depart_oldID
			            Where TransferCredits.requirementID = Departs_Old.depart_oldID
		            ) 

                GO


               Correct FK errors in SqlEditor - and make it a foreign key
            b. Add deliveryMethod - set default to "1", and make a foreign key
            c. FullfillsRequirmentNoCredits column - bit (obvious meaning) 
            d. Delete the S grade in grades file after chaning to P it in all transcripts 




    z.  Finally, delete all old, unused columns.       DO THIS AFTER WEEKS OF TESTING
         
         
         
         
         
         
         
         
         
         
         
         */


    }
}
