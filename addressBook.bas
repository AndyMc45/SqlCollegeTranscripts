Attribute VB_Name = "addressBook"
Option Explicit
Private doc As Word.Document
Public Sub getEmailList(rs As ADODB.Recordset, geRen As Boolean)
Dim myRange As range, wrdApp As Word.Application, reply As Integer, i As Integer, emailList As String, noEmailList As String, email As String
    reply = MsgBox("Do you want to make an email list with " & CStr(rs.RecordCount) & " emails?", vbYesNo)
    If reply = vbYes Then
        Set wrdApp = New Word.Application
        If openFile(wrdApp) Then
            'wrdApp.Visible = False
            If Not rs.RecordCount = 0 Then
                rs.MoveFirst
            End If
            While Not rs.EOF
                i = i + 1
                Call frmDataGrid.msgSB("Record " & CStr(i))
                email = getEmail(rs, geRen)
                'Check for a bad email -- often intentional because no email is found.
                If InStr(email, "@") = 0 Or InStr(email, ";") = 0 Then
                    noEmailList = noEmailList & vbNewLine & vbTab & email
                Else
                    emailList = emailList & email & " "
                End If
                rs.MoveNext
            Wend
            wrdApp.Visible = True
            doc.Paragraphs.Add
            doc.Paragraphs(doc.Paragraphs.count).range.Text = "No emails:" & noEmailList
            doc.Paragraphs.Add
            doc.Paragraphs(doc.Paragraphs.count).range.Text = emailList
            Set doc = Nothing
        End If
        Set wrdApp = Nothing
    End If
End Sub
Private Function getEmail(rs As ADODB.Recordset, geRen As Boolean)
Dim strName As String, foreignName As String, msg As String
Dim tuanTi As Boolean, strEmail As String, strNote As String, phoneType As String
Dim rs1 As ADODB.Recordset, rs2 As ADODB.Recordset, rs3 As ADODB.Recordset, rs4 As ADODB.Recordset, strSql As String
Dim personID As Long, groupID As Long
    
    tuanTi = Not geRen
    Set rs1 = New ADODB.Recordset
    Set rs2 = New ADODB.Recordset
    'Column 1
    'Get either geRen or tuanTi name
    If geRen Then
        personID = frmDataGrid.lngValue(rs, "個人ID")
        strSql = "SELECT * FROM 個人 WHERE 個人ID = " & personID
        rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one record
        If Not rs2.EOF Then
            strName = frmDataGrid.strValue(rs2, "貴姓") & frmDataGrid.strValue(rs2, "名字") & frmDataGrid.strValue(rs2, "職稱")
            foreignName = frmDataGrid.strValue(rs2, "FirstName") & " " & frmDataGrid.strValue(rs2, "LastName")
            If strName = "" Then
                strName = foreignName
            ElseIf Asc(strName) > 67 And Asc(strName) < 128 And foreignName <> "" Then
                strName = foreignName
            End If
        End If
        rs2.Close
    ElseIf tuanTi Then
        groupID = frmDataGrid.lngValue(rs, "團體ID")
        strSql = "SELECT * FROM 團體 WHERE 團體ID = " & groupID
        rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one record
        If Not rs2.EOF Then
            strName = frmDataGrid.strValue(rs2, "機構名稱")
        End If
        rs2.Close
    End If

    If geRen Then
        strSql = "SELECT * FROM 個人email WHERE 個人ID = " & personID
    ElseIf tuanTi Then
        strSql = "SELECT * FROM 團體email WHERE 團體ID = " & groupID
    End If
    rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'This may have any number of elements
    
    While Not rs2.EOF
        If Not frmDataGrid.strValue(rs2, "inactive") = "False" Then
            If frmDataGrid.strValue(rs2, "email") <> "" Then
                strEmail = strEmail & frmDataGrid.strValue(rs2, "email") & ";"
            End If
        End If
        rs2.MoveNext
    Wend
    rs2.Close
    
    'A bad idea - pass the name up if there is no email.  The upper routine will check for "@" and ";" and warn.
    If strEmail = "" Then
        getEmail = strName
    Else
        getEmail = strEmail
    End If
    
    Set rs1 = Nothing
    Set rs2 = Nothing
    Set rs3 = Nothing

End Function
Public Sub makeAddressBook(rs As ADODB.Recordset, geRen As Boolean)
Dim myRange As range, wrdApp As Word.Application, reply As Integer, i As Integer
    reply = MsgBox("Do you want to make an address book with " & CStr(rs.RecordCount) & " addresses?", vbYesNo)
    If reply = vbYes Then
        Set wrdApp = New Word.Application
        If openFile(wrdApp) Then
            'wrdApp.Visible = False
            Call setupPage("addressBook")
            Call setupTable("addressBook", rs.RecordCount)
            If Not rs.RecordCount = 0 Then
                rs.MoveFirst
            End If
            While Not rs.EOF
                i = i + 1
                Call frmDataGrid.msgSB("Record " & CStr(i))
                Call insertAddressbookEntry(rs, i, geRen)
                rs.MoveNext
            Wend
            Call formatFont(doc.tables(1).range, "addressBook")
            Call formatParagraphs(doc.tables(1).range, "addressBook")
            wrdApp.Visible = True
            Set doc = Nothing
        End If
        Set wrdApp = Nothing
    End If
End Sub
Private Sub insertAddressbookEntry(rs As ADODB.Recordset, tableRow As Integer, geRen As Boolean)
Dim strName As String, foreignName As String, groupName As String, msg As String
Dim tuanTi As Boolean, strAddress As String, strNote As String, country As String
Dim rs1 As ADODB.Recordset, rs2 As ADODB.Recordset, rs3 As ADODB.Recordset, rs4 As ADODB.Recordset, strSql As String
Dim personID As Long, groupID As Long
    
    tuanTi = Not geRen
    Set rs1 = New ADODB.Recordset
    Set rs2 = New ADODB.Recordset
    Set rs3 = New ADODB.Recordset
    
    'Column 1
    'Person or group name - not both
    If geRen Then
        personID = frmDataGrid.lngValue(rs, "個人ID")
        strSql = "SELECT * FROM 個人 WHERE 個人ID = " & personID
        rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one record
        If Not rs2.EOF Then
            strName = frmDataGrid.strValue(rs2, "貴姓") & frmDataGrid.strValue(rs2, "名字") & frmDataGrid.strValue(rs2, "職稱")
            foreignName = frmDataGrid.strValue(rs2, "FirstName") & " " & frmDataGrid.strValue(rs2, "LastName")
            If strName = "" Then
                strName = foreignName
            ElseIf Asc(strName) > 67 And Asc(strName) < 128 And foreignName <> "" Then
                strName = foreignName
            End If
        End If
        rs2.Close
    ElseIf tuanTi Then
        groupID = frmDataGrid.lngValue(rs, "團體ID")
        strSql = "SELECT * FROM 團體 WHERE 團體ID = " & groupID
        rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one record
        If Not rs2.EOF Then
            strName = frmDataGrid.strValue(rs2, "機構名稱")
        End If
        rs2.Close
    End If
    Call cellInsert(doc, 1, tableRow, 1, strName)

    'Column 2 - addresses
    If geRen Then
        strSql = "SELECT * FROM 個人地址 WHERE 個人ID = " & personID
    ElseIf tuanTi Then
        strSql = "SELECT * FROM 團體地址 WHERE 團體ID = " & groupID
    End If
    
    rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'This may have any number of elements
    
    'Get main address
    While Not rs2.EOF
        'Add new line for second address
        If strAddress <> "" Then
            strAddress = strAddress & vbNewLine
        End If
        'Get country from the country Code
        strSql = "SELECT * FROM 國家 WHERE 國家ID = " & frmDataGrid.lngValue(rs2, "國家ID")
        rs3.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one
        If Not rs3.EOF Then
            country = frmDataGrid.strValue(rs3, "國家名稱")
        End If
        rs3.Close
        'ROC address
        If UCase(country) = "ROC" Then
            'Zip
            If frmDataGrid.strValue(rs2, "區域") <> "" Then
                strAddress = strAddress & frmDataGrid.strValue(rs2, "區域") & ", "
            End If
            'Address
            strAddress = strAddress & frmDataGrid.strValue(rs2, "地址一行")   'this must be present
            'Second line
            If frmDataGrid.strValue(rs2, "地址二行") <> "" Then
                strAddress = strAddress & ", " & frmDataGrid.strValue(rs2, "地址二行")
            End If
            'The organization if any for individual
            If geRen Then
                If frmDataGrid.strValue(rs2, "機構名稱") <> "" Then
                    strAddress = strAddress & ", " & frmDataGrid.strValue(rs2, "機構名稱")
                End If
            End If
        'Non-ROC
        Else
            If geRen Then
                If frmDataGrid.strValue(rs2, "機構名稱") <> "" Then
                    strAddress = strAddress & frmDataGrid.strValue(rs2, "機構名稱") & " , "
                End If
            End If
            'Address
            strAddress = strAddress & frmDataGrid.strValue(rs2, "地址一行") & ", "
            'Second line and zip
            strAddress = strAddress & frmDataGrid.strValue(rs2, "地址二行") & ", "
            'zip and Country
                strAddress = strAddress & frmDataGrid.strValue(rs2, "區域") & ", " & country
        End If
        rs2.MoveNext
    Wend
    rs2.Close
    Call cellInsert(doc, 1, tableRow, 2, strAddress)
    
    Set rs1 = Nothing
    Set rs2 = Nothing
    Set rs3 = Nothing
End Sub
Public Sub makeEmailbook(rs As ADODB.Recordset, geRen As Boolean)
Dim myRange As range, wrdApp As Word.Application, reply As Integer, i As Integer
    'Insert cells
    reply = MsgBox("Do you want to make " & CStr(rs.RecordCount) & " records into an email list?", vbYesNo)
    If reply = vbYes Then
        Set wrdApp = New Word.Application
        If openFile(wrdApp) Then
            wrdApp.Visible = False
            Call setupPage("phoneBook")
            Call setupTable("phoneBook", rs.RecordCount)
            If Not rs.RecordCount = 0 Then
                rs.MoveFirst
            End If
            While Not rs.EOF
                i = i + 1
                Call frmDataGrid.msgSB("Record " & CStr(i))
                Call insertEmailEntry(rs, i, geRen)
                rs.MoveNext
            Wend
            Call formatFont(doc.tables(1).range, "phoneBook")
            Call formatParagraphs(doc.tables(1).range, "phoneBook")
            wrdApp.Visible = True
            Set doc = Nothing
        End If
        Set wrdApp = Nothing
    End If
End Sub
Private Sub insertEmailEntry(rs As ADODB.Recordset, tableRow As Integer, geRen As Boolean)
Dim strName As String, foreignName As String, msg As String
Dim tuanTi As Boolean, strEmail As String, strNote As String, phoneType As String
Dim rs1 As ADODB.Recordset, rs2 As ADODB.Recordset, rs3 As ADODB.Recordset, rs4 As ADODB.Recordset, strSql As String
Dim personID As Long, groupID As Long
    
    tuanTi = Not geRen
    Set rs1 = New ADODB.Recordset
    Set rs2 = New ADODB.Recordset
    'Column 1
    'Get either geRen or tuanTi name
    If geRen Then
        personID = frmDataGrid.lngValue(rs, "個人ID")
        strSql = "SELECT * FROM 個人 WHERE 個人ID = " & personID
        rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one record
        If Not rs2.EOF Then
            strName = frmDataGrid.strValue(rs2, "貴姓") & frmDataGrid.strValue(rs2, "名字") & frmDataGrid.strValue(rs2, "職稱")
            foreignName = frmDataGrid.strValue(rs2, "FirstName") & " " & frmDataGrid.strValue(rs2, "LastName")
            If strName = "" Then
                strName = foreignName
            ElseIf Asc(strName) > 67 And Asc(strName) < 128 And foreignName <> "" Then
                strName = foreignName
            End If
        End If
        rs2.Close
    ElseIf tuanTi Then
        groupID = frmDataGrid.lngValue(rs, "團體ID")
        strSql = "SELECT * FROM 團體 WHERE 團體ID = " & groupID
        rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one record
        If Not rs2.EOF Then
            strName = frmDataGrid.strValue(rs2, "機構名稱")
        End If
        rs2.Close
    End If
    Call cellInsert(doc, 1, tableRow, 1, strName)

    'Column 2
    If geRen Then
        strSql = "SELECT * FROM 個人email WHERE 個人ID = " & personID
    ElseIf tuanTi Then
        strSql = "SELECT * FROM 團體email WHERE 團體ID = " & groupID
    End If
    rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'This may have any number of elements
    If rs2.EOF Then 'Missing address
        strEmail = ""
    Else
        While Not rs2.EOF
            If Not frmDataGrid.strValue(rs2, "inactive") = "False" Then
                If strEmail <> "" Then
                    strEmail = strEmail & vbNewLine
                End If
                strEmail = strEmail & frmDataGrid.strValue(rs2, "email")
            End If
            rs2.MoveNext
        Wend
    End If
    rs2.Close
    Call cellInsert(doc, 1, tableRow, 2, strEmail)
    
    Set rs1 = Nothing
    Set rs2 = Nothing
    Set rs3 = Nothing

End Sub
Public Sub MakeMailingLabels(rs As ADODB.Recordset, geRen As Boolean, taiwanChurch As Boolean)
Dim myRange As range, wrdApp As Word.Application, reply As Integer, i As Integer
    reply = MsgBox("Do you want to make " & CStr(rs.RecordCount) & " labels?", vbYesNo)
    If reply = vbYes Then
        Set wrdApp = GetWord
        If openFile(wrdApp) Then
            wrdApp.Visible = False
            Call setupPage("labels")
            If rs.RecordCount > 0 Then
                rs.MoveFirst
            End If
            While Not rs.EOF
                i = i + 1
                Call frmDataGrid.msgSB("Record " & CStr(i))
                Call setupTable("labels", 1)
                Call insertLabelEntry(rs, i, geRen, taiwanChurch)
                Call formatFont(doc.tables(doc.tables.count).range, "labels")
                Call formatParagraphs(doc.tables(doc.tables.count).range, "labels")
                Call addOnePointParagraph(doc)
                rs.MoveNext
            Wend
            wrdApp.Visible = True
            Set doc = Nothing
        End If
        Set wrdApp = Nothing
    End If
End Sub
Sub insertLabelEntry(rs As ADODB.Recordset, tableNum As Integer, geRen As Boolean, taiwanChurch As Boolean)
Dim addressString As String, strName As String, foreignName As String, groupName As String, missingAddresses As Integer, msg As String
Dim country As String, tuanTi As Boolean
Dim rs1 As ADODB.Recordset, rs2 As ADODB.Recordset, rs3 As ADODB.Recordset, rs4 As ADODB.Recordset, strSql As String
Dim personID As Long, groupID As Long, found As Boolean, p As Integer, y As Integer
    
    tuanTi = Not geRen
    Set rs1 = New ADODB.Recordset
    Set rs2 = New ADODB.Recordset
    Set rs3 = New ADODB.Recordset
    
   
    'Get person name for either tuanTi or geRen (assumes 個人ID in every tuanti and geren table)
    If taiwanChurch Then
        'strName = frmDataGrid.strValue(rs, "貴姓") & frmDataGrid.strValue(rs, "名字") & frmDataGrid.strValue(rs, "職稱")
        groupName = frmDataGrid.strValue(rs, "機構名稱")
        p = InStr(groupName, "(")
        If p > 0 Then
            y = InStr(Mid(groupName, p - 1), "郵撥")
            If y > 0 Then
                groupName = Mid(groupName, 1, p - 1)
            End If
        End If
    Else
        personID = frmDataGrid.lngValue(rs, "個人ID")
        strSql = "SELECT * FROM 個人 WHERE 個人ID = " & personID
        rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one record
        If Not rs2.EOF Then
            strName = frmDataGrid.strValue(rs2, "貴姓") & frmDataGrid.strValue(rs2, "名字") & frmDataGrid.strValue(rs2, "職稱")
            foreignName = frmDataGrid.strValue(rs2, "FirstName") & " " & frmDataGrid.strValue(rs2, "LastName")
            If strName = "" Then
                strName = foreignName
            ElseIf foreignName = "" Then
                foreignName = strName
            End If
        End If
        rs2.Close
    
        'Get the group name
        If tuanTi Then
            groupID = frmDataGrid.lngValue(rs, "團體ID")
            strSql = "SELECT * FROM 團體 WHERE 團體ID = " & groupID
            rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one record
            If Not rs2.EOF Then
                groupName = frmDataGrid.strValue(rs2, "機構名稱")
            End If
            rs2.Close
        End If
    End If
    
    'Get the address
    If taiwanChurch Then
        If frmDataGrid.strValue(rs, "區域") <> "" Then
            addressString = frmDataGrid.strValue(rs, "區域") & vbNewLine
        End If
        'Address
        addressString = addressString & frmDataGrid.strValue(rs, "地址一行") & vbNewLine  'this must be present
       'The organization and name
        addressString = addressString
        If groupName <> "" Then
            addressString = addressString & groupName
        End If
        'The name
        If Trim(strName) <> "" Then
            If groupName <> "" Then
                addressString = addressString & " - " & strName
            Else
                addressString = addressString & strName
            End If
        End If
    Else
        If geRen Then
            strSql = "SELECT * FROM 個人地址 WHERE 個人ID = " & personID
        ElseIf tuanTi Then
            strSql = "SELECT * FROM 團體地址 WHERE 團體ID = " & groupID
        End If
        
        rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'This may have any number of elements
        
        If rs2.EOF Then 'Missing address
            If groupName <> "" Then
                addressString = groupName & " - "
            End If
            addressString = addressString & strName
            addressString = addressString & vbNewLine & "(No address)"
        Else
            'Get main address
            While Not rs2.EOF And Not found
                If rs2.Fields.Item("主要地址") = True Then
                    found = True
                Else
                    rs2.MoveNext
                End If
            Wend
            'If none was found, then move to the first
            If rs2.EOF Then
                rs2.MoveFirst
            End If
            
            'Get country from the country Code
            strSql = "SELECT * FROM 國家 WHERE 國家ID = " & frmDataGrid.lngValue(rs2, "國家ID")
            rs3.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one
            If Not rs3.EOF Then
                country = frmDataGrid.strValue(rs3, "國家名稱")
            End If
            If UCase(country) = "ROC" Then
                'Zip
                If frmDataGrid.strValue(rs2, "區域") <> "" Then
                    addressString = frmDataGrid.strValue(rs2, "區域") & vbNewLine
                End If
                'Address
                addressString = addressString & frmDataGrid.strValue(rs2, "地址一行")  'this must be present
                'Second line
                If frmDataGrid.strValue(rs2, "地址二行") <> "" Then
                    addressString = addressString & vbNewLine & frmDataGrid.strValue(rs2, "地址二行")
                End If
                'The organization and name
                addressString = addressString & vbNewLine
                If geRen Then
                    If frmDataGrid.strValue(rs2, "機構名稱") <> "" Then
                        addressString = addressString & frmDataGrid.strValue(rs2, "機構名稱") & " - "
                    End If
                ElseIf tuanTi Then
                    addressString = addressString & groupName & " - "
                End If
                'The name
                If Trim(strName) <> "" Then
                    addressString = addressString & strName
                End If
            'Non-ROC
            Else
                If geRen Then
                    If frmDataGrid.strValue(rs2, "機構名稱") <> "" Then
                        addressString = addressString & frmDataGrid.strValue(rs2, "機構名稱") & " - "
                    End If
                ElseIf tuanTi Then
                    addressString = addressString & groupName & " - "
                End If
                'The name
                If Trim(foreignName) <> "" Then
                    addressString = addressString & foreignName
                End If
                'Address
                addressString = addressString & vbNewLine & frmDataGrid.strValue(rs2, "地址一行") & vbNewLine
                'Second line and zip
                If frmDataGrid.strValue(rs2, "地址二行") <> "" Then
                    addressString = addressString & frmDataGrid.strValue(rs2, "地址二行") & " " & frmDataGrid.strValue(rs2, "區域") & vbNewLine
                End If
                'Country
                addressString = addressString & country
            End If
        End If
        rs2.Close
    End If
    'Insert address
    Call cellInsert(doc, tableNum, 1, 1, addressString)
    
    Set rs1 = Nothing
    Set rs2 = Nothing
    Set rs3 = Nothing
End Sub
Public Sub makePhonebook(rs As ADODB.Recordset, geRen As Boolean)
Dim myRange As range, wrdApp As Word.Application, reply As Integer, i As Integer
    'Insert cells
    reply = MsgBox("Do you want to make " & CStr(rs.RecordCount) & " records into a phone book?", vbYesNo)
    If reply = vbYes Then
        Set wrdApp = New Word.Application
        If openFile(wrdApp) Then
            wrdApp.Visible = False
            Call setupPage("phoneBook")
            Call setupTable("phoneBook", rs.RecordCount)
            If Not rs.RecordCount = 0 Then
                rs.MoveFirst
            End If
            While Not rs.EOF
                i = i + 1
                Call frmDataGrid.msgSB("Record " & CStr(i))
                Call insertPhonebookEntry(rs, i, geRen)
                rs.MoveNext
            Wend
            Call formatFont(doc.tables(1).range, "phoneBook")
            Call formatParagraphs(doc.tables(1).range, "phoneBook")
            wrdApp.Visible = True
            Set doc = Nothing
        End If
        Set wrdApp = Nothing
    End If
End Sub
Private Sub insertPhonebookEntry(rs As ADODB.Recordset, tableRow As Integer, geRen As Boolean)
Dim strName As String, foreignName As String, msg As String
Dim tuanTi As Boolean, strPhone As String, strNote As String, phoneType As String
Dim rs1 As ADODB.Recordset, rs2 As ADODB.Recordset, rs3 As ADODB.Recordset, rs4 As ADODB.Recordset, strSql As String
Dim personID As Long, groupID As Long
    
    tuanTi = Not geRen
    Set rs1 = New ADODB.Recordset
    Set rs2 = New ADODB.Recordset
    Set rs3 = New ADODB.Recordset
    
'    'column 1
'    Call cellInsert(doc, 1, tableRow, 1, CStr(tableRow))
   
    'Column 1
    'Get either geRen or tuanTi name
    If geRen Then
        personID = frmDataGrid.lngValue(rs, "個人ID")
        strSql = "SELECT * FROM 個人 WHERE 個人ID = " & personID
        rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one record
        If Not rs2.EOF Then
            strName = frmDataGrid.strValue(rs2, "貴姓") & frmDataGrid.strValue(rs2, "名字") & frmDataGrid.strValue(rs2, "職稱")
            foreignName = frmDataGrid.strValue(rs2, "FirstName") & " " & frmDataGrid.strValue(rs2, "LastName")
            If strName = "" Then
                strName = foreignName
            ElseIf Asc(strName) > 67 And Asc(strName) < 128 And foreignName <> "" Then
                strName = foreignName
            End If
        End If
        rs2.Close
    ElseIf tuanTi Then
        groupID = frmDataGrid.lngValue(rs, "團體ID")
        strSql = "SELECT * FROM 團體 WHERE 團體ID = " & groupID
        rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one record
        If Not rs2.EOF Then
            strName = frmDataGrid.strValue(rs2, "機構名稱")
        End If
        rs2.Close
    End If
    Call cellInsert(doc, 1, tableRow, 1, strName)

    'Column 2
    If geRen Then
        strSql = "SELECT * FROM 個人電話 WHERE 個人ID = " & personID
    ElseIf tuanTi Then
        strSql = "SELECT * FROM 團體電話 WHERE 團體ID = " & groupID
    End If
    rs2.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'This may have any number of elements
    
    If rs2.EOF Then 'Missing address
        strPhone = ""
    Else
        While Not rs2.EOF
           'Get phone type from phone type table
            strSql = "SELECT * FROM 電話類別 WHERE 電話類別ID = " & frmDataGrid.lngValue(rs2, "電話類別ID")
            rs3.Open strSql, frmDataGrid.cn, adOpenKeyset, adLockOptimistic  'Exactly one
            If Not rs3.EOF Then  'Always true
                phoneType = frmDataGrid.strValue(rs3, "類別")
            End If
            rs3.Close
            If frmDataGrid.strValue(rs2, "備註") <> "" Then
                strNote = " (" & frmDataGrid.strValue(rs2, "備註") & ")"
            Else
                strNote = ""
            End If
            If strPhone <> "" Then
                strPhone = strPhone & vbNewLine
            End If
            strPhone = strPhone & phoneType & ": " & frmDataGrid.strValue(rs2, "電話號碼") & strNote
            rs2.MoveNext
        Wend
        rs2.Close
    End If
    Call cellInsert(doc, 1, tableRow, 2, strPhone)
    
    Set rs1 = Nothing
    Set rs2 = Nothing
    Set rs3 = Nothing
End Sub
Private Sub setupPage(job As String)
    With doc.PageSetup
        'From Variables
        .LeftMargin = CentimetersToPoints(0.9)
        .TopMargin = CentimetersToPoints(1.67)
        .RightMargin = CentimetersToPoints(0.9)
        .BottomMargin = CentimetersToPoints(1.01)
        .Gutter = CentimetersToPoints(0)
        .HeaderDistance = CentimetersToPoints(1.5)
        .FooterDistance = CentimetersToPoints(1.5)
        .PageWidth = CentimetersToPoints(21)
        .PageHeight = CentimetersToPoints(29.7)
        .TextColumns.SetCount NumColumns:=2
        .TextColumns.width = CentimetersToPoints(9.2)
        .TextColumns.Spacing = CentimetersToPoints(0.8)
        If job = "labels" Then
            .TextColumns.SetCount NumColumns:=2
            .TextColumns.width = CentimetersToPoints(9.2)
            .TextColumns.Spacing = CentimetersToPoints(0.8)
            .TextColumns.LineBetween = False
        ElseIf job = "phoneBook" Then
            .TextColumns.SetCount NumColumns:=3
            .TextColumns.Spacing = CentimetersToPoints(0.35)
            .TextColumns.width = CentimetersToPoints(6.03)
            .TextColumns.LineBetween = True
        ElseIf job = "phoneBook" Then
            .TextColumns.SetCount NumColumns:=2
            .TextColumns.Spacing = CentimetersToPoints(0.35)
            .TextColumns.width = CentimetersToPoints(9.45)
            .TextColumns.LineBetween = True
        End If
    End With

End Sub
Private Sub setupTable(job As String, numberOfRows As Integer)
Dim numberOfColumns As Integer, i As Integer
Dim myRange As range
    'Add paragraph at end and insert table in it.
    doc.Paragraphs.Add
    Set myRange = doc.Paragraphs(doc.Paragraphs.count - 1).range
    If job = "labels" Then
        numberOfColumns = 1 'Variable
    ElseIf job = "phoneBook" Then
        numberOfColumns = 2
    ElseIf job = "addressBook" Then
        numberOfColumns = 2
   End If
    
    doc.tables.Add range:=myRange, NumRows:=numberOfRows, NumColumns:=numberOfColumns
    
    'Set up table
    With doc.tables(doc.tables.count)
        If job = "labels" Then
            .Columns(1).PreferredWidthType = wdPreferredWidthPoints
            .Columns(1).PreferredWidth = CentimetersToPoints(9.2)
            .rows.HeightRule = wdRowHeightExactly
            .rows.Height = CentimetersToPoints(2.17)
            .range.Cells.VerticalAlignment = wdCellAlignVerticalCenter
        ElseIf job = "phoneBook" Then
            .Columns(1).PreferredWidthType = wdPreferredWidthPoints
            .Columns(1).PreferredWidth = CentimetersToPoints(2.01)
            .Columns(2).PreferredWidthType = wdPreferredWidthPoints
            .Columns(2).PreferredWidth = CentimetersToPoints(4.02)
            .rows.HeightRule = wdRowHeightAuto
            .Borders(wdBorderHorizontal).LineStyle = wdLineStyleSingle
            .range.Cells.VerticalAlignment = wdCellAlignVerticalTop
        ElseIf job = "addressBook" Then
            .Columns(1).PreferredWidthType = wdPreferredWidthPoints
            .Columns(1).PreferredWidth = CentimetersToPoints(2.01)
            .Columns(2).PreferredWidthType = wdPreferredWidthPoints
            .Columns(2).PreferredWidth = CentimetersToPoints(7.44)
            .rows.HeightRule = wdRowHeightAuto
            .Borders(wdBorderHorizontal).LineStyle = wdLineStyleSingle
            .range.Cells.VerticalAlignment = wdCellAlignVerticalTop
        End If
        .Borders(wdBorderVertical).LineStyle = wdLineStyleNone
        .Borders(wdBorderDiagonalDown).LineStyle = wdLineStyleNone
        .Borders(wdBorderDiagonalUp).LineStyle = wdLineStyleNone
        .Borders(wdBorderLeft).LineStyle = wdLineStyleNone
        .Borders(wdBorderRight).LineStyle = wdLineStyleNone
        .Borders(wdBorderTop).LineStyle = wdLineStyleNone
        .Borders(wdBorderBottom).LineStyle = wdLineStyleNone
        'Variables
        .TopPadding = CentimetersToPoints(0)
        .BottomPadding = CentimetersToPoints(0)
        .LeftPadding = CentimetersToPoints(0.05)
        .RightPadding = CentimetersToPoints(0.05)
        .Spacing = 0
        'Non variables
        .rows.AllowBreakAcrossPages = False
        .Borders.Shadow = False
        .AllowAutoFit = False
    End With
End Sub
Sub formatParagraphs(range As Word.range, job As String)
    With range.ParagraphFormat
        'Variables
        .LineSpacingRule = wdLineSpaceExactly
        If job = "labels" Then
            .LineSpacing = 13
        Else
            .LineSpacing = 11
        End If
        'No variables
        .LeftIndent = CentimetersToPoints(0)
        .RightIndent = CentimetersToPoints(0)
        .SpaceBefore = 0
        .SpaceBeforeAuto = False
        .SpaceAfter = 0
        .SpaceAfterAuto = False
        .Alignment = wdAlignParagraphLeft
        .WidowControl = False
        .KeepWithNext = False
        .KeepTogether = True
        .PageBreakBefore = False
        .NoLineNumber = True 'changed
        .Hyphenation = True
        .FirstLineIndent = CentimetersToPoints(0)
        .OutlineLevel = wdOutlineLevelBodyText
        .CharacterUnitLeftIndent = 0
        .CharacterUnitRightIndent = 0
        .CharacterUnitFirstLineIndent = 0
        .LineUnitBefore = 0
        .LineUnitAfter = 0
        .AutoAdjustRightIndent = False
        .DisableLineHeightGrid = False
        .FarEastLineBreakControl = True
        .WordWrap = True
        .HangingPunctuation = True
        .HalfWidthPunctuationOnTopOfLine = False
        .AddSpaceBetweenFarEastAndAlpha = False
        .AddSpaceBetweenFarEastAndDigit = False
        .BaseLineAlignment = wdBaselineAlignBaseline
    End With

End Sub
Sub formatFont(range As Word.range, job As String)
    'Format the font
    With range.Font
        'Variables
        .NameFarEast = "細明體"
        .NameAscii = "Arial"
        .NameOther = "Arial"
        .Name = "Arial"
        If job = "labels" Then
            .Size = 12
        Else
            .Size = 9
        End If
        'Non-variables
        .Bold = False
        .Italic = False
        .Underline = wdUnderlineNone
        .UnderlineColor = wdColorAutomatic
        .StrikeThrough = False
        .DoubleStrikeThrough = False
        .Outline = False
        .Emboss = False
        .Shadow = False
        .Hidden = False
        .SmallCaps = False
        .AllCaps = False
        .Color = wdColorAutomatic
        .Engrave = False
        .Superscript = False
        .Subscript = False
        .Spacing = 0
        .Scaling = 100
        .Position = 0
        .Kerning = 0
        .Animation = wdAnimationNone
        .DisableCharacterSpaceGrid = False
        .EmphasisMark = wdEmphasisMarkNone
    End With
End Sub
Sub addOnePointParagraph(doc As Document)
Dim i As Integer, myRange As range
    'No variables
    doc.Paragraphs.Add
    i = doc.Paragraphs.count - 1
    With doc.Paragraphs(i)
        .LeftIndent = CentimetersToPoints(0)
        .RightIndent = CentimetersToPoints(0)
        .SpaceBefore = 0
        .SpaceBeforeAuto = False
        .SpaceAfter = 0
        .SpaceAfterAuto = False
        .LineSpacingRule = wdLineSpaceExactly
        .LineSpacing = 1
        .Alignment = wdAlignParagraphLeft
        .WidowControl = False
        .KeepWithNext = False
        .KeepTogether = False
        .PageBreakBefore = False
        .NoLineNumber = False
        .Hyphenation = True
        .FirstLineIndent = CentimetersToPoints(0)
        .OutlineLevel = wdOutlineLevelBodyText
        .CharacterUnitLeftIndent = 0
        .CharacterUnitRightIndent = 0
        .CharacterUnitFirstLineIndent = 0
        .LineUnitBefore = 0
        .LineUnitAfter = 0
        .AutoAdjustRightIndent = False
        .DisableLineHeightGrid = False
        .FarEastLineBreakControl = True
        .WordWrap = True
        .HangingPunctuation = True
        .HalfWidthPunctuationOnTopOfLine = False
        .AddSpaceBetweenFarEastAndAlpha = False
        .AddSpaceBetweenFarEastAndDigit = False
        .BaseLineAlignment = wdBaselineAlignBaseline
    End With
'    If chkOneLabelPerPage Then
'            doc.Paragraphs.Add
'            i = doc.Paragraphs.count - 2
'            Set myRange = doc.Paragraphs(i).Range
'            myRange.InsertBreak Type:=wdPageBreak
'    End If
End Sub
Private Sub cellInsert(doc As Word.Document, t As Integer, r, c, str)
    doc.tables(t).Cell(r, c).range = str
End Sub
Function addToString(temp1 As String, temp2 As String) As String
    If temp2 <> "" Then
        addToString = temp1 & temp2 & " "
    Else
        addToString = ""
    End If
End Function
Private Function openFile(wrdApp As Word.Application) As Boolean
    On Error GoTo errHandler
    Set doc = wrdApp.Documents.Add(newTemplate:=False, DocumentType:=wdNewBlankDocument, Visible:=True)
    openFile = True
    Exit Function
errHandler:
    openFile = False
    MsgBox Err.Description
End Function

