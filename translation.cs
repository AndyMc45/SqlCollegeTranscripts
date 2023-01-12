using Microsoft.VisualBasic;
using System;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace AccessFreeData
{
	internal static class translation
	{

		private static OrderedDictionary _cntKeys = null;
		private static OrderedDictionary cntKeys
		{
			get
			{
				if (_cntKeys is null)
				{
					_cntKeys = new OrderedDictionary(System.StringComparer.OrdinalIgnoreCase);
				}
				return _cntKeys;
			}
			set
			{
				_cntKeys = value;
			}
		}

		private static OrderedDictionary _cntTranslations = null;
		private static OrderedDictionary cntTranslations
		{
			get
			{
				if (_cntTranslations is null)
				{
					_cntTranslations = new OrderedDictionary(System.StringComparer.OrdinalIgnoreCase);
				}
				return _cntTranslations;
			}
			set
			{
				_cntTranslations = value;
			}
		}

		private static OrderedDictionary _msgKeys = null;
		internal static OrderedDictionary msgKeys
		{
			get
			{
				if (_msgKeys is null)
				{
					_msgKeys = new OrderedDictionary(System.StringComparer.OrdinalIgnoreCase);
				}
				return _msgKeys;
			}
			set
			{
				_msgKeys = value;
			}
		}

		private static OrderedDictionary _msgTranslations = null;
		internal static OrderedDictionary msgTranslations
		{
			get
			{
				if (_msgTranslations is null)
				{
					_msgTranslations = new OrderedDictionary(System.StringComparer.OrdinalIgnoreCase);
				}
				return _msgTranslations;
			}
			set
			{
				_msgTranslations = value;
			}
		}

		//There are two types of keys: control keys and message keys.
		//Keys must be added in three places - English list, Chinese list, and key list
		//Key lists are only for making a translation table

		internal static string tr(string keyWord, string var1, string var2, string var3)
		{
			string result = "";
			string storedString = "";
			try
			{
				storedString = (string) msgTranslations[keyWord];
				if (storedString == null) { storedString = "See line 90."; }
				storedString = storedString.Replace("[1]", var1);
				storedString = storedString.Replace("[2]", var2);
				storedString = storedString.Replace("[3]", var3);
				return storedString;
			}
			catch
			{
				MessageBox.Show("Missing the msgKey: " + keyWord);
				result = keyWord + " " + var1 + " " + var2 + " " + var3;
			}
			return result;
		}
		internal static void load_chinese_messages()
		{
			int count = 0;
			//Remove old
			count = msgTranslations.Count;
			int tempForEndVar = count;
			for (int i = 1; i <= tempForEndVar; i++)
			{
				msgTranslations.RemoveAt(0);
			}
			//Add new
			msgTranslations.Add("ReadOnlyDatabase", "Read only database?");
			msgTranslations.Add("ERRORTheTableDoesntHaveAPrimaryKey", "ERROR: The table [1] doesn't have a primary key.");
			msgTranslations.Add("TheTableDoesntHaveAPrimaryKey", "The table [1] doesn't have a primary key.");
			msgTranslations.Add("AfterThisProgramCrashesReopenItAndRunCreateSystemFiles", "After this program crashes, reopen it and run \"Create System Files\"");
			msgTranslations.Add("AfterThisProgramCrashesReopenItAndFixError", "After this program crashes, reopen it and fix this error");
			msgTranslations.Add("ErrorIsNotThePrimaryKeyOfAnyTable", "Error: [1] is not the primary key of any table.");
			msgTranslations.Add("WriteAfdTableDataAndAfdFieldDataTables", "Write afdTableData and afdFieldData tables?");
			msgTranslations.Add("ThisWillDeleteTheOldTablesThisIsYourLastWarning", "This will delete the old tables.  This is your last warning");
			msgTranslations.Add("TwoTablesAfdFieldDataAndAfdTableDataHaveBeenAddedToYourDatabase", "Two tables, \"afdFieldData\" and \"afdTableData\" have been added to your database");
			msgTranslations.Add("UseTheseToFormatTheProgram", "Use these to format the program.");
			msgTranslations.Add("CheckingThatAllTablesHavePrimaryKeys", "Checking that all tables have primary keys");
			msgTranslations.Add("TheTableTableNameDoesntHaveAPrimaryKey", "The table [1] doesn't have a primary key.");
			msgTranslations.Add("DoYouWantMeToAddOne", "Do you want me to add one?");
			msgTranslations.Add("ICouldNotAddTheKeyCommandFailure", "I could not add the key.  Command failure.");
			msgTranslations.Add("PrimaryKeyOfIsColumn", "Primary key of [1] is column [2]");
			msgTranslations.Add("SorryUnableToAddPrimaryKeyAtThisTime", "Sorry.  Unable to add primary key at this time.");
			msgTranslations.Add("YouMustCreateSystemDataFilesToUseTheProgram", "You must create systemDataFiles to use the program.");
			msgTranslations.Add("UseTheFileMenuCommandToDoThis", "Use the file menu command to do this.");
			msgTranslations.Add("ErrorOpeningTheConnection", "Error opening the connection.");
			msgTranslations.Add("YourTextBoxFieldEntryInTheAfdTableDataTableIsNotAFieldInThisTableIgnoringEntry", "Your textBox field entry in the afdTableData table is not a field in this table.  Ignoring entry");
			msgTranslations.Add("ErrorWritingComboBoxes", "Error writing combo boxes.");
			msgTranslations.Add("ErrorWritingInShowCmbWhere", "Error writing [1] in showCmbWhere");
			msgTranslations.Add("ErrorWritingToGrid", "Error writing to grid.");
			msgTranslations.Add("PleaseChooseTheRecordYouWouldLikeToCopy", "請選則您要複製的記錄.");
			msgTranslations.Add("PleaseChooseTheRecordYouWouldLikeToChange", "請選則您要更改的記錄.");
			msgTranslations.Add("PleaseChooseAnAreaOfTheGrid", "請選擇範圍");
			msgTranslations.Add("YouMustFirstOpenAfdTableDataTableAndEnterOneOfTheFieldsInInTextBoxFieldInRow", "You must first open \"afdTableData\" table and enter one of the fields in \"[1]\" in textBox field in \"[2]\" row.");
			msgTranslations.Add("PleaseEnterAInTheTextBox", "Please enter a [1] in the text box.");
			msgTranslations.Add("PleaseSelectA", "Please select a [1]");
			msgTranslations.Add("ThereIsAlreadyOneMatchingRecordDoYouWantToContinue", "記錄已經在. 要繼續嗎？");
			msgTranslations.Add("AddATotalOfRecords", "加 [1] 記錄嗎？");
			msgTranslations.Add("UnrecognizedType", "Unrecognized type: ");
			msgTranslations.Add("FirstSelectATable", "First select a table!");
			msgTranslations.Add("CantDeleteTheHeader", "Can't delete the header");
			msgTranslations.Add("DoYouWantToDeleteTheRecord", "要刪除 [1] 記錄嗎？");
			msgTranslations.Add("WarningThisWillBeFinal", "敬告: 不可復原");
			msgTranslations.Add("FirstChooseARecordToDelete", "先選要刪除的記錄.");
			msgTranslations.Add("PleaseChooseACellInTheTable", "情您選表內的儲存格");
			msgTranslations.Add("NotFoundInTable", "[1] not found in table.");
			msgTranslations.Add("YouDidNotEnterAnInteger", "You did not enter an integer.");
			msgTranslations.Add("PleaseEnterTheValueToFindIn", "請輸入您要尋找在 [1] 的詞：");
			msgTranslations.Add("PleaseChooseTwoRecordsTheFirstAndLastRecordsChosenWillBeMerged", "請選兩個記錄。（我會合併第一和第最後被選的記錄）");
			msgTranslations.Add("TheTwoRecordsMustBeExactlyAlikePleaseChangeTheContentOfColumn", "所有的內容必須依莫以樣. 請您先蓋了 [1] 內容。");
			msgTranslations.Add("MergeTheFirstTwoRows", "合併頭兩行嗎？");
			msgTranslations.Add("FinishedRepairingTablesNoChangesOrBadIdNumbers", "Finished repairing tables. No changes or bad id numbers.");
			msgTranslations.Add("FoundBadIdNumbersAllChangedTo", "Found [1] bad id numbers.  All changed to -1");
			msgTranslations.Add("FoundOtherIdNumbersWhichWereAlready", "Found [1] other id numbers which were already -1");
			msgTranslations.Add("SeeLogForDetails", "See Log for details");
			msgTranslations.Add("ErrorPerhapsTheInformationYouEnterIsOfTheWrongTypeOrIsTooLongOrBig", "Error. Perhaps the information you enter is of the wrong type or is too long or big?");
			msgTranslations.Add("Details", "Details: ");
			msgTranslations.Add("YouCantDeleteTheRowWithThisIDIsUsedIn", "You can't delete the row with [1] = [2]. This ID is used in [3].");
			msgTranslations.Add("NumberOfDeletedRecords", "已刪除了 [1] 記錄.");
			msgTranslations.Add("FieldErrorInLngValue", "Field error in lngValue: ");
			msgTranslations.Add("UpdateTheTableRecord", "更新 [1] 記錄?");
			msgTranslations.Add("InTheAfdFieldDataTableTheFieldOfTheTableIsRestrictedToTheValue", "In the afdFieldData table, the field \"[1]\" of the table \"[2]\" is restricted to the value \"[3]\".");
			msgTranslations.Add("ThisValueIsTheWrongTypeForThisField", "This value is the wrong type for this field");
			msgTranslations.Add("NoFieldsWithTheSameValueInAllSelectedRows", "No fields with the same value in all selected rows!");
			msgTranslations.Add("TableMustHaveAPrimaryKey", "Table must have a [1] primary key");
			msgTranslations.Add("DoYouWantToRecord", "要 [1] [2]記錄嗎？");
			msgTranslations.Add("EnterPassword", "Enter password:");
			msgTranslations.Add("YouShouldNeedToAddRowsToAnMlTablePleaseEnterPassword", "You should need to add rows to an \"_ml\" table.  Please enter password");
			msgTranslations.Add("SearchForTheFollowing", "Search for the following [1]:");
			msgTranslations.Add("PleaseEnterPhraseToFindInField", "請輸入您要尋找在 [1] 的詞：");
			msgTranslations.Add("RecordsPerPage", "Records per page: ");
			msgTranslations.Add("ThisColumnIsNotFromASonTable", "本欄不是本表的子");
			msgTranslations.Add("ErrorWritingLog", "Error writing log. ");
			msgTranslations.Add("WarningTwoInnerJoinsOnTable", "Warning: Two inner joins on \"[1]\" table. ");
			msgTranslations.Add("PrintingRow", "Printing row: ");
			msgTranslations.Add("TheTableDoesNotHaveAFather", "[1]表格沒有父親表格");
			msgTranslations.Add("ThisFieldIsIn", "This field is in [1].");
			msgTranslations.Add("TheValueIsNotA", "The value [1] is not a [2].");
			msgTranslations.Add("WarningThisWillAddMultipleRecordsContinue", "Warning: This will add multiple records. Do you want to continue?");
			msgTranslations.Add("ThereIsAlreadyOneMatchingRecordAndThisTableIsMarkedNoDoubles", "There is already one matching record, and this table is marked \"No Doubles\".");
			msgTranslations.Add("ErrorItSeemsYourSystemFilesAreMissingARowTryRewritingThem", "Error: Your system files are missing a row? Try re-writing them.");
			msgTranslations.Add("StudentTranscriptTemplate", "Student Transcript Template");
			msgTranslations.Add("StudentTranscriptTemplateEnglish", "Student Transcript Template - English");
			msgTranslations.Add("CourseGradeTemplate", "Course Grade Template");
			msgTranslations.Add("CourseRoleTemplate", "Course Role Template");
			msgTranslations.Add("IncludingTransferCredits", "包含轉學分");
			msgTranslations.Add("Credits", "Credits");
			msgTranslations.Add("DeleteDatabase", "Delete Database");
			msgTranslations.Add("ErrorFieldNotShowingInTableSeeAsdFieldData", "Error: Field [1] not showing in table.  Correct this in asdFieldData.");
			msgTranslations.Add("SelectFatherTable", "Select Father Table");
			msgTranslations.Add("DatabaseNotFound", "Database not found");
			msgTranslations.Add("FieldNotOrderable", "Field [1] is not orderable.");
			msgTranslations.Add("FieldHasAnUnrecognizedType", "Field [1] has an unrecognized type: [2]");
			msgTranslations.Add("CopyRecords", "Copy Records");
			msgTranslations.Add("ChangeRecords", "Change Records");
			msgTranslations.Add("SelectComboThatMustHaveAnItem", "Combo to use as filter");
			msgTranslations.Add("ColumnWidthOfColumn", "Select column width of column [1].");
			msgTranslations.Add("HideTableInTablesMenu", "Hide [1] table in the tables menu?");
			msgTranslations.Add("FlexGridFontSize", "Flex grid font size:");
			msgTranslations.Add("SetTheConnectionString", "Set the Connection String");
			msgTranslations.Add("ColumnComboWidthForThisTable", "Width of table in top boxes");
		}
		internal static void load_english_messages()
		{
			int count = 0;
			//Remove old
			count = msgTranslations.Count;
			int tempForEndVar = count;
			for (int i = 1; i <= tempForEndVar; i++)
			{
				msgTranslations.RemoveAt(0);
			}
			//Add new
			msgTranslations.Add("ReadOnlyDatabase", "Read only database?");
			msgTranslations.Add("ERRORTheTableDoesntHaveAPrimaryKey", "ERROR: The table [1] doesn't have a primary key.");
			msgTranslations.Add("TheTableDoesntHaveAPrimaryKey", "The table [1] doesn't have a primary key.");
			msgTranslations.Add("AfterThisProgramCrashesReopenItAndRunCreateSystemFiles", "After this program crashes, reopen it and run \"Create System Files\"");
			msgTranslations.Add("AfterThisProgramCrashesReopenItAndFixError", "After this program crashes, reopen it and fix this error");
			msgTranslations.Add("ErrorIsNotThePrimaryKeyOfAnyTable", "Error: [1] is not the primary key of any table.");
			msgTranslations.Add("WriteAfdTableDataAndAfdFieldDataTables", "Write afdTableData and afdFieldData tables?");
			msgTranslations.Add("ThisWillDeleteTheOldTablesThisIsYourLastWarning", "This will delete the old tables.  This is your last warning");
			msgTranslations.Add("TwoTablesAfdFieldDataAndAfdTableDataHaveBeenAddedToYourDatabase", "Two tables, \"afdFieldData\" and \"afdTableData\" have been added to your database");
			msgTranslations.Add("UseTheseToFormatTheProgram", "Use these to format the program.");
			msgTranslations.Add("CheckingThatAllTablesHavePrimaryKeys", "Checking that all tables have primary keys");
			msgTranslations.Add("TheTableTableNameDoesntHaveAPrimaryKey", "The table [1] doesn't have a primary key.");
			msgTranslations.Add("DoYouWantMeToAddOne", "Do you want me to add one?");
			msgTranslations.Add("ICouldNotAddTheKeyCommandFailure", "I could not add the key.  Command failure.");
			msgTranslations.Add("PrimaryKeyOfIsColumn", "Primary key of [1] is column [2]");
			msgTranslations.Add("SorryUnableToAddPrimaryKeyAtThisTime", "Sorry.  Unable to add primary key at this time.");
			msgTranslations.Add("YouMustCreateSystemDataFilesToUseTheProgram", "You must create systemDataFiles to use the program.");
			msgTranslations.Add("UseTheFileMenuCommandToDoThis", "Use the file menu command to do this.");
			msgTranslations.Add("ErrorOpeningTheConnection", "Error opening the connection.");
			msgTranslations.Add("YourTextBoxFieldEntryInTheAfdTableDataTableIsNotAFieldInThisTableIgnoringEntry", "Your textBox field entry in the afdTableData table is not a field in this table.  Ignoring entry");
			msgTranslations.Add("ErrorWritingComboBoxes", "Error writing combo boxes.");
			msgTranslations.Add("ErrorWritingInShowCmbWhere", "Error writing [1] in showCmbWhere");
			msgTranslations.Add("ErrorWritingToGrid", "Error writing to grid.");
			msgTranslations.Add("PleaseChooseTheRecordYouWouldLikeToCopy", "Please choose the Record you would like to copy");
			msgTranslations.Add("PleaseChooseTheRecordYouWouldLikeToChange", "Please choose the Record you would like to change.");
			msgTranslations.Add("PleaseChooseAnAreaOfTheGrid", "Please choose an area of the grid.");
			msgTranslations.Add("YouMustFirstOpenAfdTableDataTableAndEnterOneOfTheFieldsInInTextBoxFieldInRow", "You must first open \"afdTableData\" table and enter one of the fields in \"[1]\" in textBox field in \"[2]\" row.");
			msgTranslations.Add("PleaseEnterAInTheTextBox", "Please enter a [1] in the text box.");
			msgTranslations.Add("PleaseSelectA", "Please select a [1]");
			msgTranslations.Add("ThereIsAlreadyOneMatchingRecordDoYouWantToContinue", "There is already one matching record.  Do you want to continue?");
			msgTranslations.Add("AddATotalOfRecords", "Add [1] record(s)?");
			msgTranslations.Add("UnrecognizedType", "Unrecognized type: ");
			msgTranslations.Add("FirstSelectATable", "First select a table!");
			msgTranslations.Add("CantDeleteTheHeader", "Can't delete the header");
			msgTranslations.Add("DoYouWantToDeleteTheRecord", "Do you want to delete the [1] record?");
			msgTranslations.Add("WarningThisWillBeFinal", "Warning: This will be final");
			msgTranslations.Add("FirstChooseARecordToDelete", "First choose a record to delete");
			msgTranslations.Add("PleaseChooseACellInTheTable", "Please choose a cell in the table");
			msgTranslations.Add("NotFoundInTable", "[1] not found in table.");
			msgTranslations.Add("YouDidNotEnterAnInteger", "You did not enter an integer.");
			msgTranslations.Add("PleaseEnterTheValueToFindIn", "Please enter the value to find in [1]:");
			msgTranslations.Add("PleaseChooseTwoRecordsTheFirstAndLastRecordsChosenWillBeMerged", "Please choose two records. (The first and last records chosen will be merged.).");
			msgTranslations.Add("TheTwoRecordsMustBeExactlyAlikePleaseChangeTheContentOfColumn", "The two records must be exactly alike.  Please change the content of column [1].");
			msgTranslations.Add("MergeTheFirstTwoRows", "Merge the first two rows?");
			msgTranslations.Add("FinishedRepairingTablesNoChangesOrBadIdNumbers", "Finished repairing tables. No changes or bad id numbers.");
			msgTranslations.Add("FoundBadIdNumbersAllChangedTo", "Found [1] bad id numbers.  All changed to -1");
			msgTranslations.Add("FoundOtherIdNumbersWhichWereAlready", "Found [1] other id numbers which were already -1");
			msgTranslations.Add("SeeLogForDetails", "See Log for details");
			msgTranslations.Add("ErrorPerhapsTheInformationYouEnterIsOfTheWrongTypeOrIsTooLongOrBig", "Error. Perhaps the information you enter is of the wrong type or is too long or big?");
			msgTranslations.Add("Details", "Details: ");
			msgTranslations.Add("YouCantDeleteTheRowWithThisIDIsUsedIn", "You can't delete the row with [1] = [2]. This ID is used in [3].");
			msgTranslations.Add("NumberOfDeletedRecords", "Number of deleted records [1]");
			msgTranslations.Add("FieldErrorInLngValue", "Field error in lngValue: ");
			msgTranslations.Add("UpdateTheTableRecord", "Update the table [1] record?");
			msgTranslations.Add("InTheAfdFieldDataTableTheFieldOfTheTableIsRestrictedToTheValue", "In the afdFieldData table, the field \"[1]\" of the table \"[2]\" is restricted to the value \"[3]\".");
			msgTranslations.Add("ThisValueIsTheWrongTypeForThisField", "This value is the wrong type for this field");
			msgTranslations.Add("NoFieldsWithTheSameValueInAllSelectedRows", "No fields with the same value in all selected rows!");
			msgTranslations.Add("TableMustHaveAPrimaryKey", "Table must have a [1] primary key");
			msgTranslations.Add("DoYouWantToRecord", " Do you want to [1] record [2]?");
			msgTranslations.Add("EnterPassword", "Enter password:");
			msgTranslations.Add("YouShouldNeedToAddRowsToAnMlTablePleaseEnterPassword", "You should need to add rows to an \"_ml\" table.  Please enter password");
			msgTranslations.Add("SearchForTheFollowing", "Search for the following [1]:");
			msgTranslations.Add("PleaseEnterPhraseToFindInField", "Please enter phrase to find in field [1]:");
			msgTranslations.Add("RecordsPerPage", "Records per page: ");
			msgTranslations.Add("ThisColumnIsNotFromASonTable", "This column is not from a son table");
			msgTranslations.Add("ErrorWritingLog", "Error writing log. ");
			msgTranslations.Add("WarningTwoInnerJoinsOnTable", "Warning: Two inner joins on \"[1]\" table. ");
			msgTranslations.Add("PrintingRow", "Printing row: ");
			msgTranslations.Add("TheTableDoesNotHaveAFather", "The table [1] does not have a father");
			msgTranslations.Add("ThisFieldIsIn", "This field is in [1].");
			msgTranslations.Add("TheValueIsNotA", "The value [1] is not a [2].");
			msgTranslations.Add("WarningThisWillAddMultipleRecordsContinue", "Warning: This will add multiple records. Do you want to continue?");
			msgTranslations.Add("ThereIsAlreadyOneMatchingRecordAndThisTableIsMarkedNoDoubles", "There is already one matching record, and this table is marked \"No Doubles\".");
			msgTranslations.Add("ErrorItSeemsYourSystemFilesAreMissingARowTryRewritingThem", "Error: Your system files are missing a row? Try re-writing them.");
			msgTranslations.Add("StudentTranscriptTemplate", "Student Transcript Template");
			msgTranslations.Add("StudentTranscriptTemplateEnglish", "Student Transcript Template - English");
			msgTranslations.Add("CourseGradeTemplate", "Course Grade Template");
			msgTranslations.Add("CourseRoleTemplate", "Course Role Template");
			msgTranslations.Add("IncludingTransferCredits", "Including transfer credits");
			msgTranslations.Add("Credits", "Credits");
			msgTranslations.Add("DeleteDatabase", "Delete Database");
			msgTranslations.Add("ErrorFieldNotShowingInTableSeeAsdFieldData", "Error: Field [1] not showing in table.  Correct this in asdFieldData.");
			msgTranslations.Add("SelectFatherTable", "Select Father Table");
			msgTranslations.Add("DatabaseNotFound", "Database not found");
			msgTranslations.Add("FieldNotOrderable", "Field [1] is not orderable.");
			msgTranslations.Add("FieldHasAnUnrecognizedType", "Field [1] has an unrecognized type: [2]");
			msgTranslations.Add("CopyRecords", "Copy Records");
			msgTranslations.Add("ChangeRecords", "Change Records");
			msgTranslations.Add("SelectComboThatMustHaveAnItem", "Combo to use as filter");
			msgTranslations.Add("ColumnWidthOfColumn", "Select column width of column [1].");
			msgTranslations.Add("HideTableInTablesMenu", "Hide [1] table in the tables menu?");
			msgTranslations.Add("FlexGridFontSize", "Flex grid font size:");
			msgTranslations.Add("SetTheConnectionString", "Set the Connection String");
			msgTranslations.Add("ColumnComboWidthForThisTable", "Width of table in top boxes");

		}
		internal static void load_msg_keys()
		{
			//Only called once in program - not used in program yet
			msgKeys.Add(Guid.NewGuid().ToString(), "ReadOnlyDatabase");
			msgKeys.Add(Guid.NewGuid().ToString(), "ERRORTheTableDoesntHaveAPrimaryKey");
			msgKeys.Add(Guid.NewGuid().ToString(), "TheTableDoesntHaveAPrimaryKey");
			msgKeys.Add(Guid.NewGuid().ToString(), "AfterThisProgramCrashesReopenItAndRunCreateSystemFiles");
			msgKeys.Add(Guid.NewGuid().ToString(), "AfterThisProgramCrashesReopenItAndFixError");
			msgKeys.Add(Guid.NewGuid().ToString(), "ErrorIsNotThePrimaryKeyOfAnyTable");
			msgKeys.Add(Guid.NewGuid().ToString(), "WriteAfdTableDataAndAfdFieldDataTables");
			msgKeys.Add(Guid.NewGuid().ToString(), "ThisWillDeleteTheOldTablesThisIsYourLastWarning");
			msgKeys.Add(Guid.NewGuid().ToString(), "TwoTablesAfdFieldDataAndAfdTableDataHaveBeenAddedToYourDatabase");
			msgKeys.Add(Guid.NewGuid().ToString(), "UseTheseToFormatTheProgram");
			msgKeys.Add(Guid.NewGuid().ToString(), "CheckingThatAllTablesHavePrimaryKeys");
			msgKeys.Add(Guid.NewGuid().ToString(), "TheTableTableNameDoesntHaveAPrimaryKey");
			msgKeys.Add(Guid.NewGuid().ToString(), "DoYouWantMeToAddOne");
			msgKeys.Add(Guid.NewGuid().ToString(), "ICouldNotAddTheKeyCommandFailure");
			msgKeys.Add(Guid.NewGuid().ToString(), "PrimaryKeyOfIsColumn");
			msgKeys.Add(Guid.NewGuid().ToString(), "SorryUnableToAddPrimaryKeyAtThisTime");
			msgKeys.Add(Guid.NewGuid().ToString(), "YouMustCreateSystemDataFilesToUseTheProgram");
			msgKeys.Add(Guid.NewGuid().ToString(), "UseTheFileMenuCommandToDoThis");
			msgKeys.Add(Guid.NewGuid().ToString(), "ErrorOpeningTheConnection");
			msgKeys.Add(Guid.NewGuid().ToString(), "YourTextBoxFieldEntryInTheAfdTableDataTableIsNotAFieldInThisTableIgnoringEntry");
			msgKeys.Add(Guid.NewGuid().ToString(), "ErrorWritingComboBoxes");
			msgKeys.Add(Guid.NewGuid().ToString(), "ErrorWritingInShowCmbWhere");
			msgKeys.Add(Guid.NewGuid().ToString(), "ErrorWritingToGrid");
			msgKeys.Add(Guid.NewGuid().ToString(), "PleaseChooseTheRecordYouWouldLikeToCopy");
			msgKeys.Add(Guid.NewGuid().ToString(), "PleaseChooseTheRecordYouWouldLikeToChange");
			msgKeys.Add(Guid.NewGuid().ToString(), "PleaseChooseAnAreaOfTheGrid");
			msgKeys.Add(Guid.NewGuid().ToString(), "YouMustFirstOpenAfdTableDataTableAndEnterOneOfTheFieldsInInTextBoxFieldInRow");
			msgKeys.Add(Guid.NewGuid().ToString(), "PleaseEnterAInTheTextBox");
			msgKeys.Add(Guid.NewGuid().ToString(), "PleaseSelectA");
			msgKeys.Add(Guid.NewGuid().ToString(), "ThereIsAlreadyOneMatchingRecordDoYouWantToContinue");
			msgKeys.Add(Guid.NewGuid().ToString(), "AddATotalOfRecords");
			msgKeys.Add(Guid.NewGuid().ToString(), "UnrecognizedType");
			msgKeys.Add(Guid.NewGuid().ToString(), "FirstSelectATable");
			msgKeys.Add(Guid.NewGuid().ToString(), "CantDeleteTheHeader");
			msgKeys.Add(Guid.NewGuid().ToString(), "DoYouWantToDeleteTheRecord");
			msgKeys.Add(Guid.NewGuid().ToString(), "WarningThisWillBeFinal");
			msgKeys.Add(Guid.NewGuid().ToString(), "FirstChooseARecordToDelete");
			msgKeys.Add(Guid.NewGuid().ToString(), "PleaseChooseACellInTheTable");
			msgKeys.Add(Guid.NewGuid().ToString(), "NotFoundInTable");
			msgKeys.Add(Guid.NewGuid().ToString(), "YouDidNotEnterAnInteger");
			msgKeys.Add(Guid.NewGuid().ToString(), "PleaseEnterTheValueToFindIn");
			msgKeys.Add(Guid.NewGuid().ToString(), "PleaseChooseTwoRecordsTheFirstAndLastRecordsChosenWillBeMerged");
			msgKeys.Add(Guid.NewGuid().ToString(), "TheTwoRecordsMustBeExactlyAlikePleaseChangeTheContentOfColumn");
			msgKeys.Add(Guid.NewGuid().ToString(), "MergeTheFirstTwoRows");
			msgKeys.Add(Guid.NewGuid().ToString(), "FinishedRepairingTablesNoChangesOrBadIdNumbers");
			msgKeys.Add(Guid.NewGuid().ToString(), "FoundBadIdNumbersAllChangedTo");
			msgKeys.Add(Guid.NewGuid().ToString(), "FoundOtherIdNumbersWhichWereAlready");
			msgKeys.Add(Guid.NewGuid().ToString(), "SeeLogForDetails");
			msgKeys.Add(Guid.NewGuid().ToString(), "ErrorPerhapsTheInformationYouEnterIsOfTheWrongTypeOrIsTooLongOrBig");
			msgKeys.Add(Guid.NewGuid().ToString(), "Details");
			msgKeys.Add(Guid.NewGuid().ToString(), "YouCantDeleteTheRowWithThisIDIsUsedIn");
			msgKeys.Add(Guid.NewGuid().ToString(), "NumberOfDeletedRecords");
			msgKeys.Add(Guid.NewGuid().ToString(), "FieldErrorInLngValue");
			msgKeys.Add(Guid.NewGuid().ToString(), "UpdateTheTableRecord");
			msgKeys.Add(Guid.NewGuid().ToString(), "InTheAfdFieldDataTableTheFieldOfTheTableIsRestrictedToTheValue");
			msgKeys.Add(Guid.NewGuid().ToString(), "ThisValueIsTheWrongTypeForThisField");
			msgKeys.Add(Guid.NewGuid().ToString(), "NoFieldsWithTheSameValueInAllSelectedRows");
			msgKeys.Add(Guid.NewGuid().ToString(), "TableMustHaveAPrimaryKey");
			msgKeys.Add(Guid.NewGuid().ToString(), "DoYouWantToRecord");
			msgKeys.Add(Guid.NewGuid().ToString(), "EnterPassword");
			msgKeys.Add(Guid.NewGuid().ToString(), "YouShouldNeedToAddRowsToAnMlTablePleaseEnterPassword");
			msgKeys.Add(Guid.NewGuid().ToString(), "SearchForTheFollowing");
			msgKeys.Add(Guid.NewGuid().ToString(), "PleaseEnterPhraseToFindInField");
			msgKeys.Add(Guid.NewGuid().ToString(), "RecordsPerPage");
			msgKeys.Add(Guid.NewGuid().ToString(), "ThisColumnIsNotFromASonTable");
			msgKeys.Add(Guid.NewGuid().ToString(), "ErrorWritingLog");
			msgKeys.Add(Guid.NewGuid().ToString(), "WarningTwoInnerJoinsOnTable");
			msgKeys.Add(Guid.NewGuid().ToString(), "PrintingRow");
			msgKeys.Add(Guid.NewGuid().ToString(), "TheTableDoesNotHaveAFather");
			msgKeys.Add(Guid.NewGuid().ToString(), "ThisFieldIsIn");
			msgKeys.Add(Guid.NewGuid().ToString(), "TheValueIsNotA");
			msgKeys.Add(Guid.NewGuid().ToString(), "WarningThisWillAddMultipleRecordsContinue");
			msgKeys.Add(Guid.NewGuid().ToString(), "ThereIsAlreadyOneMatchingRecordAndThisTableIsMarkedNoDoubles");
			msgKeys.Add(Guid.NewGuid().ToString(), "ErrorItSeemsYourSystemFilesAreMissingARowTryRewritingThem");
			msgKeys.Add(Guid.NewGuid().ToString(), "StudentTranscriptTemplate");
			msgKeys.Add(Guid.NewGuid().ToString(), "StudentTranscriptTemplateEnglish");
			msgKeys.Add(Guid.NewGuid().ToString(), "CourseGradeTemplate");
			msgKeys.Add(Guid.NewGuid().ToString(), "CourseRoleTemplate");
			msgKeys.Add(Guid.NewGuid().ToString(), "IncludingTransferCredits");
			msgKeys.Add(Guid.NewGuid().ToString(), "Credits");
			msgKeys.Add(Guid.NewGuid().ToString(), "DeleteDatabase");
			msgKeys.Add(Guid.NewGuid().ToString(), "ErrorFieldNotShowingInTableSeeAsdFieldData");
			msgKeys.Add(Guid.NewGuid().ToString(), "SelectFatherTable");
			msgKeys.Add(Guid.NewGuid().ToString(), "DatabaseNotFound");
			msgKeys.Add(Guid.NewGuid().ToString(), "FieldNotOrderable");
			msgKeys.Add(Guid.NewGuid().ToString(), "FieldHasAnUnrecognizedType");
			msgKeys.Add(Guid.NewGuid().ToString(), "CopyRecords");
			msgKeys.Add(Guid.NewGuid().ToString(), "ChangeRecords");
			msgKeys.Add(Guid.NewGuid().ToString(), "SelectComboThatMustHaveAnItem");
			msgKeys.Add(Guid.NewGuid().ToString(), "ColumnWidthOfColumn");
			msgKeys.Add(Guid.NewGuid().ToString(), "ColumnComboWidthForThisTable");
			msgKeys.Add(Guid.NewGuid().ToString(), "HideTableInTablesMenu");
			msgKeys.Add(Guid.NewGuid().ToString(), "FlexGridFontSize");
			msgKeys.Add(Guid.NewGuid().ToString(), "SetTheConnectionString");

		}
		internal static void load_captions(Form frm)
		{
			//Control cnt = null;
			//string str = "";
			////UPGRADE_WARNING: (2065) Form property frm.Controls has a new behavior. More Information: https://docs.mobilize.net/vbuc/ewis#2065
			//foreach (Control cntIterator in ContainerHelper.Controls(frm))
			//{
			//	cnt = cntIterator;
			//	//UPGRADE_TODO: (1069) Error handling statement (On Error Resume Next) was converted to a pattern that might have a different behavior. More Information: https://docs.mobilize.net/vbuc/ewis#1069
			//	try
			//	{ //No error possible
			//		str = "";
			//		str = (string) cntTranslations[cnt.Name];
			//		//Print translations of controls to debug window
			//		//If Err.Description <> "" Then
			//		//If Mid(cnt.Name, 1, 3) = "mnu" Then
			//		//Debug.Print vbTab & "cntTranslations.Add Item:=""" & cnt.caption & """, Key:=""" & cnt.Name & """"
			//		//End If
			//		//Err.Clear
			//		//End If
			//		if (str != "")
			//		{
			//			cnt.Text = (string) cntTranslations[cnt.Name];
			//		}
			//		cnt = default(Control);
			//	}
			//	catch (Exception exc)
			//	{
			//		NotUpgradedHelper.NotifyNotUpgradedElement("Resume in On-Error-Resume-Next Block");
			//	}
			//}
		}
		internal static void load_english_captions()
		{
			int count = 0;
			//Remove old
			count = cntTranslations.Count;
			int tempForEndVar = count;
			for (int i = 1; i <= tempForEndVar; i++)
			{
				cntTranslations.RemoveAt(0);
			}
			//Add new
			load_english_messages();
			cntTranslations.Add("cmdShowAll", "All");
			cntTranslations.Add("cmdAdd", "Add");
			cntTranslations.Add("mnuFile", "File");
			cntTranslations.Add("mnuDatabase", "Database");
			cntTranslations.Add("mnuAddDatabase", "Add database");
			cntTranslations.Add("mnuDeleteDatabase", "Delete database");
			cntTranslations.Add("mnuBlankLine", "-----------------------------");
			cntTranslations.Add("mnuCrreateDataTables", "Create system data tables");
			cntTranslations.Add("mnuBlankLine2", "-----------------------------");
			cntTranslations.Add("mnuCloseDatabaseConnection", "&Close connection");
			cntTranslations.Add("mnuClose", "&Exit");
			cntTranslations.Add("mnuOpenTables", "Tables");
			cntTranslations.Add("mnuDeleteMenu", "Delete");
			cntTranslations.Add("mnuDelete", "Delete Record");
			cntTranslations.Add("mnuFindMenu", "Find");
			cntTranslations.Add("mnuFather", "Find in father table(&F)");
			cntTranslations.Add("mnuSon", "Find in son table(&S)");
			cntTranslations.Add("mnuFindInCombos", "Find in combo boxes(&B)");
			cntTranslations.Add("mnuFindPresent", "Find in current data grid");
			cntTranslations.Add("mnuFindDatabase", "Find in the database");
			cntTranslations.Add("mnuTools", "Tools");
			cntTranslations.Add("mnuMergeRecords", "Merge two records");
			cntTranslations.Add("mnuCopyRows", "Copy multiple records");
			cntTranslations.Add("mnuChangeRows", "Change multiple records");
			cntTranslations.Add("mnuRecordsPerPage", "Records per page");
			cntTranslations.Add("mnuSelection", "Selection");
			cntTranslations.Add("mnuPrintCurrentTable", "Print selection");
			cntTranslations.Add("mnuRepair", "Repair");
			cntTranslations.Add("mnuRepairDatabase", "Repair Database");
			cntTranslations.Add("mnuShowID", "Show ID of current record");
			cntTranslations.Add("mnuFindID", "Find record by ID");
			cntTranslations.Add("mnuViewLog", "Show Log file");
			cntTranslations.Add("mnuHelp", "Help");
			cntTranslations.Add("mnuHelpFile", "Help file");
			cntTranslations.Add("cmdDelete", "Delete");
			cntTranslations.Add("cmdExit", "Exit");
			cntTranslations.Add("label", "88");
			cntTranslations.Add("mnuTranscript", "Transcript");
			cntTranslations.Add("mnuLocations", "File Locations");
			cntTranslations.Add("mnuTranscriptFolder", "Transcripts folder");
			cntTranslations.Add("mnuTranscriptTemplate", "Transcript template");
			cntTranslations.Add("mnuRoleTemplate", "Class role template");
			cntTranslations.Add("mnuGradeTemplate", "Class grade sheet template");
			cntTranslations.Add("mnuTranscriptPrint", "Print");
			cntTranslations.Add("mnuPrintTranscript", "Print transcript");
			cntTranslations.Add("mnuPrintTranscriptEnglish", "Print transcript - English");
			cntTranslations.Add("mnuPrintRole", "Print class role");
			cntTranslations.Add("mnuPrintGrade", "Print class grade sheet");
			cntTranslations.Add("mnuAnswersToAnswerSummaryTable", "Answers to Answer summary table");
			cntTranslations.Add("mnuPrintTermSummary", "Print term summary");
			cntTranslations.Add("mnuShowQPA", "Show QPA");
			cntTranslations.Add("mnuDatabaseList", "mnuDatabaseList(0)");
			cntTranslations.Add("mnuCreateDataTables", "Create system data tables");
			cntTranslations.Add("mnuOpenTable", "openTable");
			cntTranslations.Add("mnuTable", "Table");
			cntTranslations.Add("mnuTableNoDouble", "No \"Doubles\" in this table");
			cntTranslations.Add("mnuTableHide", "Hide this table");
			cntTranslations.Add("mnuRecordFilter", "Filter other tables on this Record");
			cntTranslations.Add("mnuColumn", "Column");
			cntTranslations.Add("mnuSortOnce", "Sort column now");
			cntTranslations.Add("mnuSortColumnAZ", "Sort Column on load A-Z ");
			cntTranslations.Add("mnuSortColumnZA", "Sort Column on load Z-A");
			cntTranslations.Add("mnuColumnWidth", "Set Column Width");
			cntTranslations.Add("mnuTableTextBox", "Set as textbox field");
			cntTranslations.Add("mnuColumnShowInAll", "Show this column in all tables");
			cntTranslations.Add("mnuColumnShowInYellow", "Show this column in father tables");
			cntTranslations.Add("mnuRecord", "Record");
			cntTranslations.Add("mnuCell", "Cell");
			cntTranslations.Add("mnuCellChange", "Edit cell");
			cntTranslations.Add("mnuCellRestrict", "Filter all tables on this cell");
			cntTranslations.Add("mnuTranscriptTemplateEnglish", "Student transcript template - English");
			cntTranslations.Add("mnuLine", "-----------------------------------");
			cntTranslations.Add("mnuToolsClearCellFilters", "Clear all cell filters");
			cntTranslations.Add("mnuOptionsUseTableFilters", "Use table filters");
			cntTranslations.Add("mnuToolsClearSorting", "Clear all sorting");
			cntTranslations.Add("mnuToolHideRedColumns", "Hide Red Columns");
			cntTranslations.Add("mnuOptions", "Options");
			cntTranslations.Add("mnuLanguage", "Language");
			cntTranslations.Add("mnuEnglish", "English");
			cntTranslations.Add("mnuChinese", "Chinese");
			cntTranslations.Add("mnuToolsFontSize", "Font size");
			cntTranslations.Add("mnuAdministration", "Administration");
			cntTranslations.Add("mnuToolShowSystemTables", "Show system tables");
			cntTranslations.Add("cmdCancel", "Cancel");
			cntTranslations.Add("cmdOK", "O.K.");
			cntTranslations.Add("lblChangeHelp", "You can only change fields who values are the same in every record ");
			cntTranslations.Add("lblValue", "New Value:");
			cntTranslations.Add("lblChangeField", "Field to change:");
			cntTranslations.Add("lblConnection", "Enter Connection String. Use [1] for path, [2] for password.");
			cntTranslations.Add("mnuBlankLine3", "---------------------------");
			cntTranslations.Add("mnuUpdateDataTables", "Update system data tables");
			cntTranslations.Add("mnuTableComboWidth", "Table width (in combo)");
			cntTranslations.Add("mnuTableRefresh", "Refresh table");
			cntTranslations.Add("mnuAddressBook", "Address Book");
			cntTranslations.Add("mnuAddressBookLabels", "Print mailing labels");
			cntTranslations.Add("mnuAddressBookPhoneNumbers", "Print phone book");
			cntTranslations.Add("mnuAddressBookAddresses", "Print address book");
			cntTranslations.Add("mnuAddressBookEmails", "Print email book");
			cntTranslations.Add("mnuAddressBookGetEmails", "Get email list");
			cntTranslations.Add("mnuToolHideWhiteColumns", "Hide white columns");
			cntTranslations.Add("mnuOptionShowID", "Show row ID number");

		}
		internal static void load_chinese_captions()
		{
			int count = 0;
			//Remove old
			count = cntTranslations.Count;
			int tempForEndVar = count;
			for (int i = 1; i <= tempForEndVar; i++)
			{
				cntTranslations.RemoveAt(0);
			}
			//Add new
			load_chinese_messages();
			cntTranslations.Add("cmdShowAll", " 顯示全部");
			cntTranslations.Add("cmdAdd", "新增記錄");
			cntTranslations.Add("mnuFile", "檔案");
			cntTranslations.Add("mnuDatabase", "資料庫");
			cntTranslations.Add("mnuAddDatabase", "Add 資料庫");
			cntTranslations.Add("mnuDeleteDatabase", "Delete 庫位置");
			cntTranslations.Add("mnuBlankLine", "-----------------------------");
			cntTranslations.Add("mnuCrreateDataTables", "Create system data tables");
			cntTranslations.Add("mnuBlankLine2", "-----------------------------");
			cntTranslations.Add("mnuCloseDatabaseConnection", "&Close connection");
			cntTranslations.Add("mnuClose", "&Exit");
			cntTranslations.Add("mnuOpenTables", "開新表格");
			cntTranslations.Add("mnuDeleteMenu", "刪除記錄");
			cntTranslations.Add("mnuDelete", "刪除記錄");
			cntTranslations.Add("mnuFindMenu", "尋找");
			cntTranslations.Add("mnuFather", "顯露父表格(&F)");
			cntTranslations.Add("mnuSon", "回到子表格(&S)");
			cntTranslations.Add("mnuFindInCombos", "在上面Boxes尋找(&B)");
			cntTranslations.Add("mnuFindPresent", "在顯露記錄中尋找");
			cntTranslations.Add("mnuFindDatabase", "在資料庫中尋找");
			cntTranslations.Add("mnuTools", "工具");
			cntTranslations.Add("mnuMergeRecords", "合併兩個記錄");
			cntTranslations.Add("mnuCopyRows", "複製記錄");
			cntTranslations.Add("mnuChangeRows", "更改記錄");
			cntTranslations.Add("mnuRecordsPerPage", "Records per page");
			cntTranslations.Add("mnuSelection", "選則範圍");
			cntTranslations.Add("mnuPrintCurrentTable", "列印選則範圍");
			cntTranslations.Add("mnuRepair", "Repair");
			cntTranslations.Add("mnuRepairDatabase", "Repair Database");
			cntTranslations.Add("mnuShowID", "顯示本記錄ID");
			cntTranslations.Add("mnuFindID", "找記錄ID");
			cntTranslations.Add("mnuViewLog", "顯示LOG檔案");
			cntTranslations.Add("mnuHelp", "Help");
			cntTranslations.Add("mnuHelpFile", "Help file");
			cntTranslations.Add("cmdDelete", "Delete");
			cntTranslations.Add("cmdExit", "Exit");
			cntTranslations.Add("label", "88");
			cntTranslations.Add("mnuTranscript", "Transcript");
			cntTranslations.Add("mnuLocations", "File Locations");
			cntTranslations.Add("mnuTranscriptFolder", "Transcripts folder");
			cntTranslations.Add("mnuTranscriptTemplate", "Transcript template");
			cntTranslations.Add("mnuRoleTemplate", "Class role template");
			cntTranslations.Add("mnuGradeTemplate", "Class grade sheet template");
			cntTranslations.Add("mnuTranscriptPrint", "Print");
			cntTranslations.Add("mnuPrintTranscript", "Print transcript");
			cntTranslations.Add("mnuPrintTranscriptEnglish", "Print transcript - English");
			cntTranslations.Add("mnuPrintRole", "Print class role");
			cntTranslations.Add("mnuPrintGrade", "Print class grade sheet");
			cntTranslations.Add("mnuAnswersToAnswerSummaryTable", "Answers to Answer summary table");
			cntTranslations.Add("mnuPrintTermSummary", "Print term summary");
			cntTranslations.Add("mnuShowQPA", "Show QPA");
			cntTranslations.Add("mnuDatabaseList", "mnuDatabaseList(0)");
			cntTranslations.Add("mnuCreateDataTables", "Create system data tables");
			cntTranslations.Add("mnuOpenTable", "openTable");
			cntTranslations.Add("mnuTable", "Table");
			cntTranslations.Add("mnuTableNoDouble", "No \"Doubles\" in this table");
			cntTranslations.Add("mnuTableHide", "Hide this table");
			cntTranslations.Add("mnuRecordFilter", "Filter other tables on this record");
			cntTranslations.Add("mnuColumn", "Column");
			cntTranslations.Add("mnuSortOnce", "Sort column now");
			cntTranslations.Add("mnuSortColumnAZ", "Sort Column on load A-Z ");
			cntTranslations.Add("mnuSortColumnZA", "Sort Column on load Z-A");
			cntTranslations.Add("mnuColumnWidth", "Set Column Width");
			cntTranslations.Add("mnuTableTextBox", "Set as textbox field");
			cntTranslations.Add("mnuColumnShowInAll", "Show this column in all tables");
			cntTranslations.Add("mnuColumnShowInYellow", "Show this column in father tables");
			cntTranslations.Add("mnuRecord", "Record");
			cntTranslations.Add("mnuCell", "Cell");
			cntTranslations.Add("mnuCellChange", "Edit cell");
			cntTranslations.Add("mnuCellRestrict", "Filter all tables on this cell");
			cntTranslations.Add("mnuTranscriptTemplateEnglish", "Student transcript template - English");
			cntTranslations.Add("mnuLine", "-----------------------------------");
			cntTranslations.Add("mnuToolsClearCellFilters", "Clear all cell filters");
			cntTranslations.Add("mnuOptionsUseTableFilters", "Use table filters");
			cntTranslations.Add("mnuToolsClearSorting", "Clear all sorting");
			cntTranslations.Add("mnuToolHideRedColumns", "Hide Red Columns");
			cntTranslations.Add("mnuOptions", "Options");
			cntTranslations.Add("mnuLanguage", "Language");
			cntTranslations.Add("mnuEnglish", "English");
			cntTranslations.Add("mnuChinese", "Chinese");
			cntTranslations.Add("mnuToolsFontSize", "Font size");
			cntTranslations.Add("mnuAdministration", "Administration");
			cntTranslations.Add("mnuToolShowSystemTables", "Show system tables");
			cntTranslations.Add("cmdCancel", "Cancel");
			cntTranslations.Add("cmdOK", "O.K.");
			cntTranslations.Add("lblChangeHelp", "在每一個選取範圍的行、欄的內容必需一樣才能改 ");
			cntTranslations.Add("lblValue", "新 的 內 容：");
			cntTranslations.Add("lblChangeField", "要 改 變 的 蘭：");
			cntTranslations.Add("lblConnection", "Enter Connection String. Use [1] for path, [2] for password.");
			cntTranslations.Add("mnuBlankLine3", "---------------------------");
			cntTranslations.Add("mnuUpdateDataTables", "Update system data tables");
			cntTranslations.Add("mnuTableComboWidth", "Table width (in combo)");
			cntTranslations.Add("mnuTableRefresh", "Refresh table");
			cntTranslations.Add("mnuAddressBook", "Address Book");
			cntTranslations.Add("mnuAddressBookLabels", "Print mailing labels");
			cntTranslations.Add("mnuAddressBookPhoneNumbers", "Print phone book");
			cntTranslations.Add("mnuAddressBookAddresses", "Print address book");
			cntTranslations.Add("mnuAddressBookEmails", "Print email book");
			cntTranslations.Add("mnuAddressBookGetEmails", "Get email list");
			cntTranslations.Add("mnuToolHideWhiteColumns", "Hide white columns");
			cntTranslations.Add("mnuOptionShowID", "Show row ID number");
		}
		internal static void load_control_keys()
		{
			//Only called once in program - not used in program yet
			cntKeys.Add(Guid.NewGuid().ToString(), "cmdShowAll");
			cntKeys.Add(Guid.NewGuid().ToString(), "cmdAdd");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuFile");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuDatabase");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuAddDatabase");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuDeleteDatabase");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuBlankLine");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuCrreateDataTables");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuCloseDatabaseConnection");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuBlankLine2");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuClose");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuOpenTables");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuDeleteMenu");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuDelete");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuFindMenu");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuFather");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuSon");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuFindInCombos");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuFindPresent");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuFindDatabase");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuTools");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuMergeRecords");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuCopyRows");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuChangeRows");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuRecordsPerPage");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuSelection");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuPrintCurrentTable");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuRepair");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuRepairDatabase");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuShowID");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuFindID");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuViewLog");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuHelp");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuHelpFile");
			cntKeys.Add(Guid.NewGuid().ToString(), "cmdDelete");
			cntKeys.Add(Guid.NewGuid().ToString(), "cmdExit");
			cntKeys.Add(Guid.NewGuid().ToString(), "label");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuTranscript");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuLocations");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuTranscriptFolder");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuTranscriptTemplate");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuRoleTemplate");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuGradeTemplate");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuTranscriptPrint");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuPrintTranscript");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuPrintTranscriptEnglish");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuPrintRole");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuPrintGrade");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuAnswersToAnswerSummaryTable");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuPrintTermSummary");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuShowQPA");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuDatabaseList");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuCreateDataTables");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuOpenTable");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuTable");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuTableNoDouble");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuTableHide");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuRecordFilter");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuColumn");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuSortOnce");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuSortColumnAZ");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuSortColumnZA");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuColumnWidth");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuTableTextBox");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuColumnShowInAll");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuColumnShowInYellow");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuRecord");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuCell");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuCellChange");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuCellRestrict");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuTranscriptTemplateEnglish");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuLine");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuToolsClearCellFilters");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuOptionsUseTableFilters");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuToolsClearSorting");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuToolHideRedColumns");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuOptions");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuLanguage");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuEnglish");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuChinese");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuToolsFontSize");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuAdministration");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuToolShowSystemTables");
			cntKeys.Add(Guid.NewGuid().ToString(), "cmdCancel");
			cntKeys.Add(Guid.NewGuid().ToString(), "cmdOK");
			cntKeys.Add(Guid.NewGuid().ToString(), "lblChangeHelp");
			cntKeys.Add(Guid.NewGuid().ToString(), "lblValue");
			cntKeys.Add(Guid.NewGuid().ToString(), "lblChangeField");
			cntKeys.Add(Guid.NewGuid().ToString(), "lblConnection");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuBlankLine3");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuUpdateDataTables");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuTableComboWidth");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuTableRefresh");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuAddressBook");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuAddressBookLabels");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuAddressBookPhoneNumbers");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuAddressBookAddresses");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuAddressBookEmails");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuAddressBookGetEmails");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuToolHideWhiteColumns");
			cntKeys.Add(Guid.NewGuid().ToString(), "mnuOptionShowID");
		}
	}
}