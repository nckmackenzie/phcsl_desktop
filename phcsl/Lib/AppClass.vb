Imports AfricasTalkingCS
Imports DevExpress.XtraEditors
Imports DevExpress.XtraLayout
Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text
Imports System.Globalization
Public Class AppClass
    Public Shared Sub UserLogin(userid As String, password As String, txtbx As DevExpress.XtraEditors.TextEdit)
        LogedUserID = Nothing
        connstr = Nothing
        If LoginForm.DatabaseComboBoxEdit.SelectedIndex = 0 Then
            connstr = "Data Source=localhost\SQLEXPRESS;Initial Catalog=housingDB;User ID=sa;Password=NA-b$H12;"
        ElseIf LoginForm.DatabaseComboBoxEdit.SelectedIndex = 1 Then
            connstr = "Data Source=localhost\SQLEXPRESS;Initial Catalog=housingTest;User ID=sa;Password=NA-b$H12;"
        End If

        Dim salt As String = ""
        Dim Active As Boolean
        Dim promptpwd As Boolean
        Dim pwddatechng As Date
        Using connection = New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Dim readSaltQuery As String = "SELECT * FROM tblUsers WHERE UserID=@id"
            Dim sqlcmd As New SqlCommand(readSaltQuery, connection)
            sqlcmd.Parameters.AddWithValue("@id", userid)

            Dim reader As SqlDataReader = sqlcmd.ExecuteReader
            While reader.Read
                LogedUserID = reader("ID")
                salt = reader("salt").ToString()
                promptpwd = CBool(reader("promptPasswordChange"))
                pwddatechng = CDate(reader("passwordChangeDate"))
                Active = reader("active")
            End While
            reader.Close()

            If Active = False Then
                MessageBox.Show("This User Has Been Disabled.Contact Admin", "Alert", MessageBoxButtons.OK _
                                 , MessageBoxIcon.Information)
                Exit Sub
            End If

            Dim pass = Encryption.HashString(password)
            Dim HashedAndSalted = Encryption.HashString(String.Format("{0}{1}", pass, salt))
            Dim checkQuery As String = "SELECT COUNT(*) FROM tblUsers WHERE UserID=@user AND userPwd=@pass"
            Dim sqlcmd0 As New SqlCommand(checkQuery, connection)
            sqlcmd0.Parameters.AddWithValue("@user", userid)
            sqlcmd0.Parameters.AddWithValue("@pass", HashedAndSalted)
            Dim result As Integer = Convert.ToInt32(sqlcmd0.ExecuteScalar())
            If result = 1 Then
                MainForm.Show()
                LoginForm.Hide()
            Else
                MessageBox.Show("Password Entered Is Incorrect", "Alert", MessageBoxButtons.OK _
                                  , MessageBoxIcon.Information)
                With txtbx
                    .Focus()
                    .SelectAll()
                End With
            End If
        End Using
    End Sub
    Public Shared Function Encrypt(textToEncrypt As String)
        Dim hashedString As String
        Dim data As Byte() = UTF8Encoding.UTF8.GetBytes(textToEncrypt)
        Using md5s As New MD5CryptoServiceProvider()
            Dim keys As Byte() = md5s.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash))
            Using tripDes As New TripleDESCryptoServiceProvider() With {
                .Key = keys,
                .Mode = CipherMode.ECB,
                .Padding = Security.Cryptography.PaddingMode.PKCS7}
                Dim transform As ICryptoTransform = tripDes.CreateEncryptor()
                Dim results As Byte() = transform.TransformFinalBlock(data, 0, data.Length)
                hashedString = Convert.ToBase64String(results, 0, results.Length)
            End Using
        End Using
        Return hashedString
    End Function
    Public Shared Function Decrypt(textToDecrypt As String)
        Dim decryptedString As String
        Dim data As Byte() = Convert.FromBase64String(textToDecrypt)
        Using md5s As New MD5CryptoServiceProvider()
            Dim keys As Byte() = md5s.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash))
            Using tripDes As New TripleDESCryptoServiceProvider() With {
                .Key = keys,
                .Mode = CipherMode.ECB,
                .Padding = Security.Cryptography.PaddingMode.PKCS7}
                Dim transform As ICryptoTransform = tripDes.CreateDecryptor()
                Dim results As Byte() = transform.TransformFinalBlock(data, 0, data.Length)
                decryptedString = UTF8Encoding.UTF8.GetString(results)
            End Using
        End Using
        Return decryptedString
    End Function
    Public Shared Sub SaveUser(userid As String, username As String, type As Int16, pass As String,
                               hash As String, contact As String, active As Boolean, pwddate As Date, promt As Boolean)
        Using connection = New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            RecordCount = 0

            Dim sqlcmd As New SqlCommand("spCrudUsers", connection) With {
                .CommandType = CommandType.StoredProcedure
            }
            sqlcmd.Parameters.AddWithValue("@action", "insert")
            sqlcmd.Parameters.AddWithValue("@userid", userid)
            sqlcmd.Parameters.AddWithValue("@username", username)
            sqlcmd.Parameters.AddWithValue("@userTypeId", type)
            sqlcmd.Parameters.AddWithValue("@pwd", pass)
            sqlcmd.Parameters.AddWithValue("@hash", hash)
            sqlcmd.Parameters.AddWithValue("@contact", contact)
            sqlcmd.Parameters.AddWithValue("@active", active)
            sqlcmd.Parameters.AddWithValue("@pwdchangedate", pwddate)
            sqlcmd.Parameters.AddWithValue("@promptpwdchange", promt)
            RecordCount = sqlcmd.ExecuteNonQuery()
        End Using
    End Sub
    Public Shared Sub ExecQuery(Query As String)
        '//SUB TO SAVE,UPDATE AND DELETE RECORDS TO THE DATAS SOURCE
        Try
            RecordCount = 0
            Using connection = New SqlConnection(connstr)
                connection.Open()
                Using cmd = New SqlCommand(Query, connection)
                    params.ForEach(Sub(p) cmd.Parameters.Add(p))
                    params.Clear()
                    RecordCount = cmd.ExecuteNonQuery()
                End Using
            End Using


        Catch ex As Exception
            MessageBox.Show(ex.Message, "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try
    End Sub
    Public Shared Sub ExecSP(sp As String)
        '//SUB TO SAVE,UPDATE AND DELETE RECORDS TO THE DATAS SOURCE
        Try
            RecordCount = 0
            Using connection = New SqlConnection(connstr)
                connection.Open()
                Using cmd = New SqlCommand(sp, connection)
                    cmd.CommandType = CommandType.StoredProcedure
                    params.ForEach(Sub(p) cmd.Parameters.Add(p))
                    params.Clear()
                    RecordCount = cmd.ExecuteNonQuery()
                End Using
            End Using


        Catch ex As Exception
            MessageBox.Show(ex.Message, "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try
    End Sub
    Public Shared Sub ShowNotification(msg As String)
        XtraMessageBox.Show(msg, "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
    Public Shared Sub ShowError(msg As String)
        XtraMessageBox.Show(msg, "Alert", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub
    Public Shared Sub GenerateID(Query As String, Field As String, TxtBox As TextEdit, Code As String, format As String, Optional Prexfix As String = Nothing)
        '//sub to generate the last id from the database
        Try
            Using connection = New SqlConnection(connstr)
                With connection
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With
                Using cmd = New SqlCommand(Query, connection)
                    params.ForEach(Sub(p) cmd.Parameters.Add(p))
                    params.Clear()

                    Using rd = cmd.ExecuteReader
                        rd.Read()
                        If Not rd.HasRows Or IsDBNull(rd.Item(Field)) Then
                            TxtBox.Text = Prexfix & Code
                        Else
                            Dim newid As Integer = CInt(rd.Item(Field))
                            newid += 1
                            TxtBox.Text = Prexfix & newid.ToString(format)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ShowError(ex.Message)
        End Try
    End Sub
    Public Shared Function FnGenerateID(Query As String, Field As String, Code As String, format As String, Optional Prexfix As String = Nothing) As String
        Dim GeneratedId As String
        Try
            Using connection = New SqlConnection(connstr)
                With connection
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With
                Using cmd = New SqlCommand(Query, connection)
                    params.ForEach(Sub(p) cmd.Parameters.Add(p))
                    params.Clear()

                    Using rd = cmd.ExecuteReader
                        rd.Read()
                        If Not rd.HasRows Or IsDBNull(rd.Item(Field)) Then
                            GeneratedId = Prexfix & Code
                        Else
                            Dim newid As Integer = CInt(rd.Item(Field))
                            newid += 1
                            GeneratedId = Prexfix & newid.ToString(format)
                        End If
                    End Using
                End Using
            End Using

            Return GeneratedId
        Catch ex As Exception
            ShowError(ex.Message)
            Return Nothing
        End Try
    End Function
    Public Shared Function AlertQuestion(Prompt As String) As DialogResult
        Dim selectedAnswer As DialogResult
        selectedAnswer = XtraMessageBox.Show(Prompt, "Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        Return selectedAnswer
    End Function
    Public Shared Sub ClearItems(lyControl As LayoutControl)
        For Each cntr In lyControl.Controls.OfType(Of TextEdit)()
            cntr.EditValue = Nothing
        Next
    End Sub
    Public Shared Function LoadToDatatable(Query As String, Optional IsSP As Boolean = False) As DataTable
        '//sub to fetch data from the data source and dump results in a datatable
        Dim ResultsFromDB As New DataTable
        Try
            Using connection = New SqlConnection(connstr)
                With connection
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With

                Using da = New SqlDataAdapter(Query, connection)
                    params.ForEach(Sub(p) da.SelectCommand.Parameters.Add(p))
                    params.Clear()
                    da.SelectCommand.CommandType = If(IsSP, CommandType.StoredProcedure, CommandType.Text)
                    Using dt = New DataTable
                        da.Fill(ResultsFromDB)
                    End Using
                End Using
            End Using

            Return ResultsFromDB
        Catch ex As Exception
            ShowError(ex.Message)
            Return Nothing
        End Try
    End Function
    Public Shared Function Find(fid As Integer, qry As String) As DataTable
        Dim DBValues As New DataTable
        Try
            Using con As New SqlConnection(connstr)
                With con
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With
                Using cmd As New SqlCommand(qry, con)
                    cmd.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = fid
                    Using da As New SqlDataAdapter(cmd)
                        da.Fill(DBValues)
                    End Using
                End Using
            End Using
            Return DBValues
        Catch ex As Exception
            ShowError(ex.Message)
            Return Nothing
        End Try
    End Function
    Public Shared Sub SendMessage(PhoneNo As String, Message As String)
        Try
            Dim gateway As New AfricasTalkingGateway(SMS_USER_NAME.ToString, SMS_API_KEY.ToString)
            Dim SMS = gateway.SendMessage(PhoneNo, Message, SMS_SENDER.ToString)
        Catch ex As AfricasTalkingGatewayException
            MessageBox.Show(ex.Message)
        End Try
    End Sub
    Public Shared Function SendBulkSMS(Receipients As String, Message As String) As String
        Dim _gateway As New AfricasTalkingGateway(SMS_USER_NAME, SMS_API_KEY)
        Try
            Dim Results As String = _gateway.SendMessage(Receipients, Message, SMS_SENDER)
            Dim JsonObject = Newtonsoft.Json.Linq.JObject.Parse(Results)
            Dim JsonMessage As String = JsonObject("SMSMessageData")("Message").ToString()
            Return JsonMessage
        Catch ex As Exception
            Return ex.Message.ToString()
            ShowError(ex.Message)
        End Try
    End Function
    Public Shared Sub FormatDatagridView(dgw As DataGridView)
        With dgw
            .BackgroundColor = Color.WhiteSmoke
            .EnableHeadersVisualStyles = False
            .ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .ColumnHeadersHeight = 24
            .ColumnHeadersDefaultCellStyle.Font = New Font("Trebuchet MS", 8.25, FontStyle.Bold)
            .ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.SystemColors.ActiveCaption
            .AlternatingRowsDefaultCellStyle.Font = New Font("Trebuchet MS", 8.25, FontStyle.Regular)
            .AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 244, 244)
            .DefaultCellStyle.Font = New Font("Trebuchet MS", 8.25, FontStyle.Regular)
        End With
    End Sub
    Public Shared Function GetFirstName(fullName As String)
        Dim FirstName As String
        Dim SplitedNames() As String = fullName.Split(" ")
        FirstName = SplitedNames(0)
        Return FirstName
    End Function
    Public Shared Sub LoadToLookUpEdit(Query As String, Control As DevExpress.XtraEditors.LookUpEdit, DisplayMember As String, ValueMember As String)
        Try
            dt = New DataTable
            dt = LoadToDatatable(Query)
            With Control.Properties
                .DisplayMember = DisplayMember
                .ValueMember = ValueMember
                .DataSource = dt
                .ShowHeader = False
                .ShowFooter = False
                '.Columns.Add()
            End With
        Catch ex As Exception
            ShowError(ex.Message)
        End Try
    End Sub
    Public Shared Sub LoadToSearchLookUpEdit(Query As String, Control As DevExpress.XtraEditors.SearchLookUpEdit, DisplayMember As String, ValueMember As String)
        Try
            dt = New DataTable
            dt = LoadToDatatable(Query)
            With Control.Properties
                .DisplayMember = DisplayMember
                .ValueMember = ValueMember
                .DataSource = dt
                .ShowFooter = False
                '.Columns.Add()
            End With
        Catch ex As Exception
            ShowError(ex.Message)
        End Try
    End Sub
    Public Shared Function FetchDBValue(query As String) As Object
        Dim value As Object
        Try
            dt = New DataTable
            dt = LoadToDatatable(query)
            If dt.Rows.Count > 0 Then
                value = dt.Rows(0)(0)
            Else
                value = Nothing
            End If
            Return value
        Catch ex As Exception
            ShowError(ex.Message)
            Return Nothing
        End Try
    End Function
    Public Shared Function TitleCase(Word As String) As String
        Dim cultureInfo As New System.Globalization.CultureInfo("en-US")
        Return cultureInfo.TextInfo.ToTitleCase(Word)
    End Function

    Public Shared Function GetRegistrationFee(memberNo As Integer)
        Dim regFee As Decimal
        AddParams("@no", memberNo)
        regFee = FetchDBValue("SELECT ISNULL(SUM(amount),0) FROM tblIncomeDetails WHERE (purposeId=1) AND (memberNo=@no)")
        Return regFee
    End Function
    Public Shared Function GetShareAmount(memberNo As Short) As Decimal
        Dim shareTotal As Decimal
        Dim transferedOut, transferedIn, paidShares, fromDeposits, toDeposits, fromBonus, receivedFromUnit, withdrawn As Decimal
        'AddParams("@no", memberNo)
        'AddParams("@status", 1)
        'bonus = CDec(fetchDBValue("SELECT ISNULL(SUM(amount),0) AS amount FROM tblBonus WHERE (memberNo=@no) AND (staus=@status)"))
        AddParams("@pid", 2)
        AddParams("@no", memberNo)
        paidShares = CDec(FetchDBValue("SELECT ISNULL(SUM(amount),0) AS shareTotal FROM tblIncomeDetails WHERE (purposeId=@pid) AND (memberNo=@no)"))
        AddParams("@type", 1)
        AddParams("@no", memberNo)
        transferedOut = CDec(FetchDBValue("SELECT ISNULL(SUM(amount),0) AS transferedOut FROM tblTransfers WHERE (transferType=@type) AND (transferFrom=@no)"))
        AddParams("@type", 1)
        AddParams("@no", memberNo)
        transferedIn = CDec(FetchDBValue("SELECT ISNULL(SUM(amount),0) AS transferedIn FROM tblTransfers WHERE (transferType=@type) AND (transferTo=@no)"))
        AddParams("@type", 4)
        AddParams("@no", memberNo)
        fromDeposits = CDec(FetchDBValue("SELECT ISNULL(SUM(amount),0) AS transferedIn FROM tblTransfers WHERE (transferType=@type) AND (transferTo=@no)"))
        AddParams("@type", 5)
        AddParams("@no", memberNo)
        toDeposits = CDec(FetchDBValue("SELECT ISNULL(SUM(amount),0) AS transferedOut FROM tblTransfers WHERE (transferType=@type) AND (transferTo=@no)"))
        AddParams("@type", 6)
        AddParams("@no", memberNo)
        fromBonus = CDec(FetchDBValue("SELECT ISNULL(SUM(amount),0) AS transferedIn FROM tblTransfers WHERE (transferType=@type) AND (transferTo=@no)"))
        AddParams("@type", 10)
        AddParams("@no", memberNo)
        receivedFromUnit = CDec(FetchDBValue("SELECT ISNULL(SUM(amount),0) AS transferedIn FROM tblTransfers WHERE (transferType=@type) AND (transferTo=@no)"))
        AddParams("@no", memberNo)
        withdrawn = CDec(FetchDBValue("SELECT ISNULL(SUM(amount),0) as withdrawal FROM tblShareWithdrawal WHERE (memberNo=@no)"))
        shareTotal = (paidShares + transferedIn + fromDeposits + fromBonus + receivedFromUnit) - (transferedOut + toDeposits + withdrawn)
        Return shareTotal
    End Function
    Public Shared Function CheckDate(txnDate As Date) As Boolean
        If txnDate.Date > Date.Now.Date Then
            Return False
        Else
            Return True
        End If
    End Function
    Public Shared Function FormatContactNo(MemberNo As Integer)
        AddParams("@no", MemberNo)
        Dim Contact As String = FetchDBValue("SELECT contact FROM tblMemberAddress WHERE memberNo = @no")
        Return "+254" & Contact.Substring(1)
    End Function
    Public Shared Function GenerateDBID(Table As String, Optional Column As String = "ID") As Integer
        Dim ID As Integer
        If CInt(FetchDBValue("SELECT COUNT(*) FROM " + Table)) = 0 Then
            ID = 1
        Else
            ID = (CInt(FetchDBValue("SELECT TOP 1 " + Column + " FROM " + Table + " ORDER BY " + Column + " DESC"))) + 1
        End If
        Return ID
    End Function
    Public Shared Sub AddToLedger(txnDate As Date, account As String, desc As String, debit As Decimal, credit As Decimal _
                                 , projectId As String, accountid As Short, transactiontype As Integer, transactionId As Integer)
        Try
            Using connection = New SqlConnection(connstr)
                With connection
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With
                sql = "INSERT INTO tblLedger (transactionDate,account,[description],debit,credit,projectId,accountId,transactionType,transactionID) "
                sql &= "VALUES(@date,@account,@desc,@debit,@credit,@pid,@aid,@type,@tid)"
                Using cmd = New SqlCommand(sql, connection)
                    With cmd
                        .Parameters.Clear()
                        .Parameters.Add("@date", SqlDbType.Date).Value = txnDate.Date
                        .Parameters.Add("@account", SqlDbType.VarChar).Value = account
                        .Parameters.Add("@desc", SqlDbType.VarChar).Value = desc
                        .Parameters.Add("@debit", SqlDbType.Decimal).Value = debit
                        .Parameters.Add("@credit", SqlDbType.Decimal).Value = credit
                        If Not String.IsNullOrEmpty(projectId) Then
                            .Parameters.Add("@pid", SqlDbType.Int).Value = CInt(projectId)
                        Else
                            .Parameters.Add("@pid", SqlDbType.Int).Value = DBNull.Value
                        End If
                        .Parameters.Add("@aid", SqlDbType.Int).Value = accountid
                        .Parameters.Add("@type", SqlDbType.Int).Value = transactiontype
                        .Parameters.Add("@tid", SqlDbType.Int).Value = transactionId
                        .ExecuteNonQuery()
                    End With
                End Using
            End Using
        Catch ex As Exception
            ShowError(ex.Message)
        End Try
    End Sub
    Public Shared Sub BankPosting(txnDate As Date, bankid As Short, debit As Decimal, credit As Decimal, desc As String, reference As String,
                                  paymethod As Short, cleared As Boolean, transactionType As Integer, transactionID As Integer)
        Try
            Using connection = New SqlConnection(connstr)
                With connection
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With
                sql = "INSERT INTO tblBankPostings (transactionDate,bankId,debit,credit,[description],reference,payMethodId,cleared,transactionType,transactionID) "
                sql &= "VALUES(@date,@bank,@debit,@credit,@desc,@reference,@paymethod,@cleared,@type,@tid)"
                Using cmd = New SqlCommand(sql, connection)
                    With cmd
                        .Parameters.Clear()
                        .Parameters.Add("@date", SqlDbType.Date).Value = txnDate
                        .Parameters.Add("@bank", SqlDbType.Int).Value = bankid
                        .Parameters.Add("@debit", SqlDbType.Decimal).Value = debit
                        .Parameters.Add("@credit", SqlDbType.Decimal).Value = credit
                        .Parameters.Add("@desc", SqlDbType.VarChar).Value = desc
                        .Parameters.Add("@reference", SqlDbType.VarChar).Value = reference
                        .Parameters.Add("@paymethod", SqlDbType.Int).Value = paymethod
                        .Parameters.Add("@cleared", SqlDbType.Bit).Value = cleared
                        .Parameters.Add("@type", SqlDbType.Int).Value = transactionType
                        .Parameters.Add("@tid", SqlDbType.Int).Value = transactionID
                        .ExecuteNonQuery()
                    End With
                End Using
            End Using
        Catch ex As Exception
            ShowError(ex.Message)
        End Try
    End Sub
    Public Shared Sub LoadToGrid(Query As String, Grid As DataGridView, Optional IsSP As Boolean = False)
        '//sub to fetch data from the data source and dump results in a datagridview
        Try
            dt = New DataTable
            dt = LoadToDatatable(Query, IsSP)
            Grid.DataSource = dt
        Catch ex As Exception
            ShowError(ex.Message)
        End Try
    End Sub
    Public Shared Function GetGridTotal(DatagridView As DataGridView, CellNo As Integer) As Decimal
        Dim Total As Decimal
        For i = 0 To DatagridView.Rows.Count - 1 '//loop
            Total += CInt(DatagridView.Rows(i).Cells(CellNo).Value)
        Next
        Return Total
    End Function
    Public Shared Sub LoadToControl(Query As String, Control As CheckedComboBoxEdit, Table As String, DisplayMember As String, ValueMember As String)
        '//sub to fetch data from the data source and dump results in a datagridview
        Try
            Using connection = New SqlConnection(connstr)
                With connection
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With
                Using da = New SqlDataAdapter(Query, connection)
                    params.ForEach(Sub(p) da.SelectCommand.Parameters.Add(p))
                    params.Clear()
                    Using ds = New DataSet
                        da.Fill(ds, Table)
                        With Control.Properties
                            .DisplayMember = DisplayMember
                            .ValueMember = ValueMember
                            .DataSource = ds.Tables(Table)
                        End With
                    End Using
                End Using
            End Using
        Catch ex As Exception
            ShowError(ex.Message)
        End Try
    End Sub
    Public Shared Function IsValidDate(dateString As String) As Boolean
        Dim formats As String() = {"dd/MM/yyyy", "dd-MM-yyyy"}
        Dim dateValue As DateTime

        If DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, dateValue) Then
            Return True
        Else
            Return False
        End If
    End Function

End Class
