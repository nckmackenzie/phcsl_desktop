Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports DevExpress.XtraEditors
Imports System.IO
Imports ExcelDataReader
Public Class UCImportExpenses
    Private Shared _instance As UCImportExpenses
    Private FilePath As String
    Dim tables As DataTableCollection
    Public Shared ReadOnly Property Instance As UCImportExpenses
        Get
            If _instance Is Nothing Then _instance = New UCImportExpenses()
            Return _instance
        End Get
    End Property
#Region "SUBS"
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Reset()
    End Sub

    Sub Reset()
        SaveSimpleButton.Enabled = False
        ValidateSimpleButton.Enabled = False
        AppClass.FormatDatagridView(DataGridView)
        DataGridView.Rows.Clear()
        FilePath = Nothing
    End Sub
    Sub ImportData(datatable As DataTable)
        DataGridView.Rows.Clear()
        For Each row As DataRow In datatable.Rows
            DataGridView.Rows.Add(row.ItemArray)
        Next
    End Sub
    Private Function ValidateAndSetCell(row As DataGridViewRow, connection As SqlConnection, transaction As SqlTransaction, query As String, paramName As String, cellIndex As Integer, resultCellIndex As Integer) As Boolean
        Using cmd As New SqlCommand(query, connection, transaction)
            cmd.Parameters.Add(New SqlParameter(paramName, SqlDbType.VarChar)).Value = row.Cells(cellIndex).Value.ToString().ToLower()
            Using rd = cmd.ExecuteReader
                If rd.Read() AndAlso rd.HasRows AndAlso Not IsDBNull(rd.Item(0)) Then
                    row.Cells(resultCellIndex).Value = CInt(rd.Item(0))
                    Return True
                Else
                    row.Cells(cellIndex).Style.ForeColor = Color.Red
                    Return False
                End If
            End Using
        End Using
    End Function

    Private Sub ImportSimpleButton_Click(sender As Object, e As EventArgs) Handles ImportSimpleButton.Click
        Try
            Dim OFD As New XtraOpenFileDialog
            With OFD
                .Filter = "Excel Workbook|*.xlsx" '//set exetensions to filter
                .Title = "Import Expense"
                .ValidateNames = True
                .Multiselect = False
                If .ShowDialog = System.Windows.Forms.DialogResult.OK Then
                    FilePath = .FileName
                    ValidateSimpleButton.Enabled = True
                    'ImportExcelToDataGridView(FilePath)
                    Using stream = File.Open(.FileName, FileMode.Open, FileAccess.Read)
                        Using reader As IExcelDataReader = ExcelReaderFactory.CreateReader(stream)
                            Dim result As DataSet = reader.AsDataSet(New ExcelDataSetConfiguration() With {
                                                                 .ConfigureDataTable = Function(__) New ExcelDataTableConfiguration() With {
                                                                 .UseHeaderRow = True}})
                            tables = result.Tables
                            ImportData(tables("Sheet1"))
                        End Using
                    End Using
                End If
            End With

        Catch ex As Exception
            AppClass.ShowError($"An error occurred: {ex.Message}")
        End Try
    End Sub

    Private Sub ValidateSimpleButton_Click(sender As Object, e As EventArgs) Handles ValidateSimpleButton.Click
        For Each row As DataGridViewRow In DataGridView.Rows
            For Each cell As DataGridViewCell In row.Cells
                cell.Style.ForeColor = Color.Black
            Next
        Next

        Using connection As New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Dim ErrorCount As Int16 = 0

            Using MyTransaction As SqlTransaction = connection.BeginTransaction
                Try

                    For Each row As DataGridViewRow In DataGridView.Rows
                        Dim hasError As Boolean = False
                        ' Validate date
                        'If Not AppClass.IsValidDate(row.Cells(0).Value.ToString()) Then
                        '    row.Cells(0).Style.ForeColor = Color.Red
                        '    hasError = True
                        'End If

                        ' Validate expense account
                        If Not ValidateAndSetCell(row, connection, MyTransaction, "SELECT ID FROM tblGlAccounts WHERE LOWER(accountName) = @value", "@value", 1, 7) Then
                            hasError = True
                        End If

                        ' Validate pay method
                        If Not ValidateAndSetCell(row, connection, MyTransaction, "SELECT ID FROM tblPaymentMethod WHERE LOWER(methodName) = @value", "@value", 3, 8) Then
                            hasError = True
                        End If

                        ' Validate expensed to
                        If Not ValidateAndSetCell(row, connection, MyTransaction, "SELECT ID FROM tblStaffs WHERE LOWER(staffName) = @value", "@value", 5, 9) Then
                            hasError = True
                        End If

                        If hasError Then
                            ErrorCount += 1
                        End If

                    Next

                    If ErrorCount > 0 Then
                        AppClass.ShowNotification("You have invalid records imported.")
                    Else
                        AppClass.ShowNotification("All records validated successfully.")
                        SaveSimpleButton.Enabled = True
                    End If

                Catch ex As Exception
                    AppClass.ShowError($"An error occurred: {ex.Message}")
                    MyTransaction.Rollback()
                End Try
            End Using
        End Using
    End Sub
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        AddParams("@is", True)
        AppClass.GenerateID("SELECT MAX(RIGHT(voucherNo,5)) As ref FROM tblExpenses WHERE (isBulk=@is)", "ref", TextEdit1, "00001", "00000", "EXP/")
        Using connection As New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Dim ErrorCount As Int16 = 0
            Dim id As Integer = AppClass.GenerateDBID("tblexpenses")
            Dim sid As Integer = AppClass.GenerateDBID("tblStaffIndebtness")

            Using MyTransaction As SqlTransaction = connection.BeginTransaction
                Try

                    For i = 0 To DataGridView.Rows.Count - 1
                        sql = "INSERT INTO tblExpenses (ID,expenseDate,voucherNo,amount,glCodeId,expenseType,projectId,[description],"
                        sql &= "completed,paymentMethodId,bankId,paymentReference,discountPercent,discountAmount,totalDiscount,vatType,"
                        sql &= "exclusiveAmount,vat,inclusiveAmount,transactionType,staffId) VALUES(@id,@date,@voucher,@amount,@gl,@type,"
                        sql &= "@pid,@desc,@comp,@paymethod,@bankid,@payref,@per,@damnt,@discount,@vtype,@exc,@vat,@inc,@ttype,@staff)"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Clear()
                                .Parameters.Add("@id", SqlDbType.Int).Value = id + i
                                .Parameters.Add("@date", SqlDbType.Date).Value = CDate(DataGridView.Rows(i).Cells(0).Value).Date
                                .Parameters.Add("@voucher", SqlDbType.VarChar).Value = TextEdit1.EditValue
                                .Parameters.Add("@amount", SqlDbType.Decimal).Value = CDec(DataGridView.Rows(i).Cells(2).Value)
                                .Parameters.Add("@gl", SqlDbType.Int).Value = CInt(DataGridView.Rows(i).Cells(7).Value)
                                .Parameters.Add("@type", SqlDbType.Int).Value = 2
                                .Parameters.Add("@pid", SqlDbType.Int).Value = DBNull.Value
                                If DataGridView.Rows(i).Cells(6).Value Is Nothing OrElse Not String.IsNullOrEmpty(DataGridView.Rows(i).Cells(6).Value) Then
                                    .Parameters.Add("@desc", SqlDbType.VarChar).Value = DataGridView.Rows(i).Cells(6).Value.ToString().ToLower
                                Else
                                    .Parameters.Add("@desc", SqlDbType.VarChar).Value = DBNull.Value
                                End If
                                .Parameters.Add("@comp", SqlDbType.Bit).Value = True
                                .Parameters.Add("@paymethod", SqlDbType.Int).Value = DataGridView.Rows(i).Cells(8).Value
                                .Parameters.Add("@bankid", SqlDbType.Int).Value = DBNull.Value
                                If DataGridView.Rows(i).Cells(4).Value Is Nothing OrElse Not String.IsNullOrEmpty(DataGridView.Rows(i).Cells(4).Value) Then
                                    .Parameters.Add("@payref", SqlDbType.VarChar).Value = DataGridView.Rows(i).Cells(4).Value.ToString().ToLower
                                Else
                                    .Parameters.Add("@payref", SqlDbType.VarChar).Value = DBNull.Value
                                End If
                                .Parameters.Add(New SqlParameter("@per", SqlDbType.Decimal)).Value = DBNull.Value
                                .Parameters.Add(New SqlParameter("@damnt", SqlDbType.Decimal)).Value = DBNull.Value
                                .Parameters.Add(New SqlParameter("@discount", SqlDbType.Decimal)).Value = DBNull.Value

                                .Parameters.Add(New SqlParameter("@vtype", SqlDbType.Int)).Value = 1
                                .Parameters.Add(New SqlParameter("@exc", SqlDbType.Decimal)).Value = CDec(DataGridView.Rows(i).Cells(2).Value)
                                .Parameters.Add(New SqlParameter("@vat", SqlDbType.Decimal)).Value = 0
                                .Parameters.Add(New SqlParameter("@inc", SqlDbType.Decimal)).Value = CDec(DataGridView.Rows(i).Cells(2).Value)
                                .Parameters.Add(New SqlParameter("@ttype", SqlDbType.Int)).Value = 5
                                .Parameters.Add(New SqlParameter("@staff", SqlDbType.Int)).Value = CInt(DataGridView.Rows(i).Cells(9).Value)
                                .ExecuteNonQuery()
                            End With
                        End Using

                        If CStr(DataGridView.Rows(i).Cells(5).Value).ToLower <> "[none]" Then
                            sql = "INSERT INTO tblStaffIndebtness (ID,transactionDate,staffId,debit,credit,reference,narration,transactionType,transactionId) "
                            sql &= "VALUES(@id,@tdate,@sid,@debit,@credit,@reference,@narr,@type,@tid)"
                            Using cmd = New SqlCommand(sql, connection, MyTransaction)
                                With cmd
                                    .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = sid + i
                                    .Parameters.Add(New SqlParameter("@tdate", SqlDbType.Date)).Value = CDate(DataGridView.Rows(i).Cells(0).Value).Date
                                    .Parameters.Add(New SqlParameter("@sid", SqlDbType.Int)).Value = CInt(DataGridView.Rows(i).Cells(9).Value)
                                    .Parameters.Add(New SqlParameter("@debit", SqlDbType.Decimal)).Value = 0
                                    .Parameters.Add(New SqlParameter("@credit", SqlDbType.Decimal)).Value = CDec(DataGridView.Rows(i).Cells(2).Value)
                                    If DataGridView.Rows(i).Cells(4).Value Is Nothing OrElse Not String.IsNullOrEmpty(DataGridView.Rows(i).Cells(4).Value) Then
                                        .Parameters.Add("@reference", SqlDbType.VarChar).Value = DataGridView.Rows(i).Cells(4).Value.ToString().ToLower
                                    Else
                                        .Parameters.Add("@reference", SqlDbType.VarChar).Value = DBNull.Value
                                    End If
                                    If DataGridView.Rows(i).Cells(6).Value Is Nothing OrElse Not String.IsNullOrEmpty(DataGridView.Rows(i).Cells(6).Value) Then
                                        .Parameters.Add("@narr", SqlDbType.VarChar).Value = DataGridView.Rows(i).Cells(6).Value.ToString().ToLower
                                    Else
                                        .Parameters.Add("@narr", SqlDbType.VarChar).Value = DBNull.Value
                                    End If
                                    .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 5
                                    .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = id
                                    .ExecuteNonQuery()
                                End With
                            End Using
                        End If

                        sql = "INSERT INTO tblLedger (transactionDate,account,[description],debit,credit,projectId,accountId,transactionType,transactionID) "
                        sql &= "VALUES(@date,@account,@desc,@debit,@credit,@pid,@aid,@type,@tid)"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Clear()
                                .Parameters.Add("@date", SqlDbType.Date).Value = CDate(DataGridView.Rows(i).Cells(0).Value).Date
                                .Parameters.Add("@account", SqlDbType.VarChar).Value = CStr(DataGridView.Rows(i).Cells(1).Value).ToLower
                                .Parameters.Add("@desc", SqlDbType.VarChar).Value = "Expense Payments"
                                .Parameters.Add("@debit", SqlDbType.Decimal).Value = CDec(DataGridView.Rows(i).Cells(2).Value)
                                .Parameters.Add("@credit", SqlDbType.Decimal).Value = 0
                                .Parameters.Add("@pid", SqlDbType.Int).Value = DBNull.Value
                                .Parameters.Add("@aid", SqlDbType.Int).Value = 6
                                .Parameters.Add("@type", SqlDbType.Int).Value = 5
                                .Parameters.Add("@tid", SqlDbType.Int).Value = id
                                .ExecuteNonQuery()
                            End With
                        End Using
                        sql = "INSERT INTO tblLedger (transactionDate,account,[description],debit,credit,projectId,accountId,transactionType,transactionID) "
                        sql &= "VALUES(@date,@account,@desc,@debit,@credit,@pid,@aid,@type,@tid)"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Clear()
                                .Parameters.Add("@date", SqlDbType.Date).Value = CDate(DataGridView.Rows(i).Cells(0).Value).Date
                                If CInt(DataGridView.Rows(i).Cells(9).Value) = 2 Then
                                    If CInt(DataGridView.Rows(i).Cells(8).Value) = 1 Then
                                        .Parameters.Add("@account", SqlDbType.VarChar).Value = "Cash Account"
                                    Else
                                        .Parameters.Add("@account", SqlDbType.VarChar).Value = "Bank Account"
                                    End If
                                Else
                                    .Parameters.Add("@account", SqlDbType.VarChar).Value = "petty cash"
                                End If
                                .Parameters.Add("@desc", SqlDbType.VarChar).Value = "Expense Payments"
                                .Parameters.Add("@debit", SqlDbType.Decimal).Value = 0
                                .Parameters.Add("@credit", SqlDbType.Decimal).Value = CDec(DataGridView.Rows(i).Cells(2).Value)
                                .Parameters.Add("@pid", SqlDbType.Int).Value = DBNull.Value
                                .Parameters.Add("@aid", SqlDbType.Int).Value = 2
                                .Parameters.Add("@type", SqlDbType.Int).Value = 5
                                .Parameters.Add("@tid", SqlDbType.Int).Value = id
                                .ExecuteNonQuery()
                            End With
                        End Using
                        If CInt(DataGridView.Rows(i).Cells(8).Value) > 1 Then
                            'sql = "INSERT INTO tblBankPostings (transactionDate,bankId,debit,credit,[description],reference,payMethodId,cleared,transactionType,transactionID) "
                            'sql &= "VALUES(@date,@bank,@debit,@credit,@desc,@reference,@paymethod,@cleared,@type,@tid)"
                            'Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            '    With cmd
                            '        .Parameters.Clear()
                            '        .Parameters.Add("@date", SqlDbType.Date).Value = CDate(DataGridView.Rows(i).Cells(0).Value).Date
                            '        .Parameters.Add("@bank", SqlDbType.Int).Value = cbxBank.SelectedValue
                            '        .Parameters.Add("@debit", SqlDbType.Decimal).Value = 0
                            '        .Parameters.Add("@credit", SqlDbType.Decimal).Value = CDec(txtInclusive.Text)
                            '        .Parameters.Add("@desc", SqlDbType.VarChar).Value = "Expense Payments"
                            '        .Parameters.Add("@reference", SqlDbType.VarChar).Value = txtPaymentReference.Text.ToLower.ToString
                            '        .Parameters.Add("@paymethod", SqlDbType.Int).Value = cbxPaymentMethod.SelectedValue
                            '        .Parameters.Add("@cleared", SqlDbType.Bit).Value = False
                            '        .Parameters.Add("@type", SqlDbType.Int).Value = 5
                            '        .Parameters.Add("@tid", SqlDbType.Int).Value = id
                            '        .ExecuteNonQuery()
                            '    End With
                            'End Using
                        End If

                        sql = "INSERT INTO tblLogs VALUES(@uid,@act,@actdate)"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Clear()
                                .Parameters.Add("@uid", SqlDbType.Int).Value = LogedUserID
                                .Parameters.Add("@act", SqlDbType.NVarChar).Value = "Expense Posting "
                                .Parameters.Add("@actdate", SqlDbType.Date).Value = Date.Now.Date
                                .ExecuteNonQuery()
                            End With
                        End Using
                    Next

                    sql = "INSERT INTO tblLogs VALUES(@uid,@act,@actdate)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@uid", SqlDbType.Int).Value = LogedUserID
                            .Parameters.Add("@act", SqlDbType.NVarChar).Value = "Expense Posting "
                            .Parameters.Add("@actdate", SqlDbType.Date).Value = Date.Now.Date
                            .ExecuteNonQuery()
                        End With
                    End Using

                    With MyTransaction
                        .Commit()
                        .Dispose()
                    End With

                    AppClass.ShowNotification("Saved Successfully!")
                    Reset()

                Catch ex As Exception
                    AppClass.ShowError($"An error occurred: {ex.Message}")
                    MyTransaction.Rollback()
                End Try
            End Using
        End Using
    End Sub
#End Region
End Class
