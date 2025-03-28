Imports System.Data.SqlClient

Public Class UCExpenses
    Private Shared _instance As UCExpenses
    Private IsEdit As Boolean
    Private BankRequired As Boolean
    Private ID As Integer
    Public Shared ReadOnly Property Instance As UCExpenses
        Get
            If _instance Is Nothing Then _instance = New UCExpenses()
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
        AppClass.LoadToLookUpEdit("SELECT ID,methodName FROM tblPaymentMethod", PaymentMethodLookUpEdit, "methodName", "ID")
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(bankName) as BankName FROM tblBanks", BankLookUpEdit, "BankName", "ID")
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(accountName) as AccountName FROM tblGlAccounts WHERE active=1 AND accountTypeId=6 ORDER BY AccountName", ExpenseAccountLookUpEdit, "AccountName", "ID")
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(staffName) as StaffName FROM tblStaffs WHERE active=1 ORDER BY StaffName", ExpensedToLookUpEdit, "StaffName", "ID")
        sql = "SELECT p.ID,UPPER(p.projectName) as ProjectName "
        sql &= "FROM tblProjects p"
        AppClass.LoadToLookUpEdit(sql, ProjectLookUpBoxEdit, "ProjectName", "ID")
        DataGridView.Rows.Clear()
        AppClass.FormatDatagridView(DataGridView)
        DeleteSimpleButton.Enabled = False
        IsEdit = False
        ID = Nothing
        InpusReset()
        AppClass.GenerateID("SELECT MAX(RIGHT(voucherNo,5)) as Field FROM tblExpenses where isBulk =1", "Field", ExpenseRefTextEdit, "00001", "00000", "EXP/")
    End Sub
    Sub InpusReset()
        AppClass.ClearItems(LayoutControl1)
        AddSimpleButton.Text = "Add"
        PettyCashCheckEdit.Checked = True
        ProjectLookUpBoxEdit.Properties.ReadOnly = True
        DiscountValueTextEdit.Properties.ReadOnly = True
        BankLookUpEdit.Properties.ReadOnly = True
        VatTypeLookUpEdit.SelectedIndex = 0
        BankRequired = False
    End Sub
    Function GetDiscountedAmount(UnitValue As Decimal) As Decimal
        If DiscountTypeComboBoxEdit.EditValue Is Nothing Then
            Return 0
        End If
        Dim DiscountValue As Decimal = If(DiscountValueTextEdit.EditValue IsNot Nothing, Convert.ToDecimal(DiscountValueTextEdit.EditValue), 0)
        If DiscountTypeComboBoxEdit.SelectedIndex = 0 Then
            Return UnitValue * (DiscountValue / 100)
        Else
            Return DiscountValue
        End If
    End Function
    Function CalculateNetAmount() As Decimal
        Dim ExpenseAmount As Decimal = If(ExpenseAmountTextEdit.EditValue IsNot Nothing, CDec(ExpenseAmountTextEdit.EditValue), 0)
        Dim DiscountedAmount = GetDiscountedAmount(ExpenseAmount)
        DiscountedAmountTextEdit.EditValue = DiscountedAmount.ToString("N")
        Return ExpenseAmount - DiscountedAmount
    End Function
    Function CellIsEmpty(dgv As DataGridView, rowIndex As Integer, colIndex As Integer) As Boolean
        Dim cellValue As Object = dgv.Rows(rowIndex).Cells(colIndex).Value
        Return cellValue Is Nothing
        'Try
        '    If rowIndex < 0 Or rowIndex >= dgv.Rows.Count Or colIndex < 0 Or colIndex >= dgv.Columns.Count Then
        '        Return True
        '    End If

        '    Dim cellValue As Object = dgv.Rows(rowIndex).Cells(colIndex).Value

        '    Return cellValue Is Nothing OrElse IsDBNull(cellValue) OrElse String.IsNullOrWhiteSpace(cellValue.ToString())
        'Catch ex As Exception
        '    Return True
        'End Try
    End Function
    Function Validation() As Boolean
        errmsg = Nothing
        If ExpenseTypeComboBoxEdit.EditValue Is Nothing Then
            errmsg = "Select expense type"
            ExpenseTypeComboBoxEdit.Focus()
        ElseIf ExpenseTypeComboBoxEdit.SelectedIndex = 1 AndAlso ProjectLookUpBoxEdit.EditValue Is Nothing Then
            errmsg = "Select project"
            ProjectLookUpBoxEdit.Focus()
        ElseIf ExpenseDateEdit.EditValue Is Nothing Then
            errmsg = "Select expense date"
            ExpenseDateEdit.Focus()
        ElseIf Not AppClass.CheckDate(CDate(ExpenseDateEdit.EditValue)) Then
            errmsg = "Expense date cannot be in the future"
            ExpenseDateEdit.Focus()
        ElseIf ExpenseAccountLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select expense account"
            ExpenseAccountLookUpEdit.Focus()
        ElseIf ExpenseAmountTextEdit.EditValue Is Nothing Then
            errmsg = "Enter expense amount"
            ExpenseAmountTextEdit.Focus()
        ElseIf DiscountTypeComboBoxEdit.SelectedIndex > -1 AndAlso DiscountValueTextEdit.EditValue Is Nothing Then
            errmsg = "Enter discount"
            DiscountValueTextEdit.Focus()
        ElseIf PaymentMethodLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select payment method"
            PaymentMethodLookUpEdit.Focus()
        ElseIf BankRequired AndAlso BankLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select bank"
            BankLookUpEdit.Focus()
        End If

        If errmsg IsNot Nothing Then
            AppClass.ShowError(errmsg, True)
            Return False
        Else
            Return True
        End If
    End Function
    Sub CalculateVAT()
        Dim result = AppClass.CalculateVAT(NetAmountTextEdit.EditValue, VatTypeLookUpEdit.SelectedIndex + 1, 16)
        ExclusiveTextEdit.EditValue = result.AmountExcl.ToString("N")
        VatAmountTextEdit.EditValue = result.VatAmount.ToString("N")
        InclusiveTextEdit.EditValue = result.AmountIncl.ToString("N")
    End Sub
    Sub Save()
        ID = AppClass.GenerateDBID("tblExpenses")
        Dim IndebtnessId As Integer = AppClass.GenerateDBID("tblStaffIndebtness")
        AppClass.GenerateID("SELECT MAX(RIGHT(voucherNo,5)) as Field FROM tblExpenses where isBulk =1", "Field", ExpenseRefTextEdit, "00001", "00000", "EXP/")
        Using Connection As New SqlConnection(connstr)
            With Connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With
            Using MyTransaction As SqlTransaction = Connection.BeginTransaction
                Try

                    For i = 0 To DataGridView.Rows.Count - 1
                        sql = "INSERT INTO tblExpenses (ID,expenseDate,voucherNo,amount,glCodeId,projectId,description,expenseType,paymentMethodId,"
                        sql &= "bankId,paymentReference,discountPercent,discountAmount,totalDiscount,vatType,exclusiveAmount,vat,inclusiveAmount,transactionType,staffId,isBulk,isPettyCash) "
                        sql &= "VALUES(@id,@date,@voucher,@amount,@gl,@pid,@desc,@type,@pay,@bid,@ref,@discper,@discamount,@disc,@vtype,@exc,@vat,@inc,@ttype,@sid,@is,@petty)"

                        Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = ID + i
                                .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = DataGridView.Rows(i).Cells(4).Value
                                .Parameters.Add(New SqlParameter("@voucher", SqlDbType.VarChar)).Value = ExpenseRefTextEdit.EditValue
                                .Parameters.Add(New SqlParameter("@amount", SqlDbType.Decimal)).Value = CDec(DataGridView.Rows(i).Cells(7).Value)
                                .Parameters.Add(New SqlParameter("@gl", SqlDbType.Int)).Value = CInt(DataGridView.Rows(i).Cells(5).Value)
                                If CellIsEmpty(DataGridView, i, 2) Then
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = CInt(DataGridView.Rows(i).Cells(2).Value)
                                End If
                                If CellIsEmpty(DataGridView, i, 20) Then
                                    .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = CStr(DataGridView.Rows(i).Cells(20).Value).ToLower
                                End If
                                .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = If(CInt(DataGridView.Rows(i).Cells(0).Value) = 1, 2, 1)
                                .Parameters.Add(New SqlParameter("@pay", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(16).Value
                                If CellIsEmpty(DataGridView, i, 18) Then
                                    .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(18).Value
                                End If
                                If CellIsEmpty(DataGridView, i, 19) Then
                                    .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = DataGridView.Rows(i).Cells(19).Value
                                End If
                                If CellIsEmpty(DataGridView, i, 8) Then
                                    .Parameters.Add(New SqlParameter("@discper", SqlDbType.Decimal)).Value = DBNull.Value
                                    .Parameters.Add(New SqlParameter("@discamount", SqlDbType.Decimal)).Value = DBNull.Value
                                    .Parameters.Add(New SqlParameter("@disc", SqlDbType.Decimal)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@discper", SqlDbType.Decimal)).Value = If(DataGridView.Rows(i).Cells(8).Value = 1, DataGridView.Rows(i).Cells(9).Value, DBNull.Value)
                                    .Parameters.Add(New SqlParameter("@discamount", SqlDbType.Decimal)).Value = If(DataGridView.Rows(i).Cells(8).Value = 2, DataGridView.Rows(i).Cells(9).Value, DBNull.Value)
                                    .Parameters.Add(New SqlParameter("@disc", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(10).Value
                                End If
                                .Parameters.Add(New SqlParameter("@vtype", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(12).Value
                                .Parameters.Add(New SqlParameter("@exc", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(13).Value
                                .Parameters.Add(New SqlParameter("@vat", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(14).Value
                                .Parameters.Add(New SqlParameter("@inc", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(15).Value
                                .Parameters.Add(New SqlParameter("@ttype", SqlDbType.Int)).Value = 5
                                If CellIsEmpty(DataGridView, i, 21) Then
                                    .Parameters.Add(New SqlParameter("@sid", SqlDbType.VarChar)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@sid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(21).Value
                                End If
                                .Parameters.Add(New SqlParameter("@is", SqlDbType.Bit)).Value = 1
                                .Parameters.Add(New SqlParameter("@petty", SqlDbType.Bit)).Value = DataGridView.Rows(i).Cells(23).Value
                                .ExecuteNonQuery()
                            End With
                        End Using

                        If Not CellIsEmpty(DataGridView, i, 21) Then
                            sql = "INSERT INTO tblStaffIndebtness (ID,transactionDate,staffId,credit,reference,narration,transactionType,transactionId) "
                            sql &= "VALUES(@id,@date,@sid,@credit,@ref,@nar,@type,@tid)"
                            Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                                With cmd
                                    .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = IndebtnessId + i
                                    .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = DataGridView.Rows(i).Cells(4).Value
                                    .Parameters.Add(New SqlParameter("@sid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(21).Value
                                    .Parameters.Add(New SqlParameter("@credit", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(15).Value
                                    If CellIsEmpty(DataGridView, i, 19) Then
                                        .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = DBNull.Value
                                    Else
                                        .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = DataGridView.Rows(i).Cells(19).Value
                                    End If
                                    If CellIsEmpty(DataGridView, i, 20) Then
                                        .Parameters.Add(New SqlParameter("@nar", SqlDbType.VarChar)).Value = DBNull.Value
                                    Else
                                        .Parameters.Add(New SqlParameter("@nar", SqlDbType.VarChar)).Value = DataGridView.Rows(i).Cells(20).Value
                                    End If
                                    .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 5
                                    .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID + i
                                    .ExecuteNonQuery()
                                End With
                            End Using
                        End If

                        sql = "INSERT INTO tblLedger (transactionDate,account,description,debit,credit,projectId,accountId,transactionType,transactionID) "
                        sql &= "VALUES(@date,@account,@desc,@debit,@credit,@pid,@aid,@type,@tid)"
                        Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = DataGridView.Rows(i).Cells(4).Value
                                .Parameters.Add(New SqlParameter("@account", SqlDbType.VarChar)).Value = CStr(DataGridView.Rows(i).Cells(6).Value).ToLower
                                If CellIsEmpty(DataGridView, i, 20) Then
                                    .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = DataGridView.Rows(i).Cells(20).Value
                                End If
                                .Parameters.Add(New SqlParameter("@debit", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(15).Value
                                .Parameters.Add(New SqlParameter("@credit", SqlDbType.Decimal)).Value = 0
                                If CellIsEmpty(DataGridView, i, 2) Then
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(2).Value
                                End If
                                .Parameters.Add(New SqlParameter("@aid", SqlDbType.Int)).Value = 6
                                .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 5
                                .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID + i
                                .ExecuteNonQuery()
                            End With
                        End Using

                        sql = "INSERT INTO tblLedger (transactionDate,account,description,debit,credit,projectId,accountId,transactionType,transactionID) "
                        sql &= "VALUES(@date,@account,@desc,@debit,@credit,@pid,@aid,@type,@tid)"
                        Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = DataGridView.Rows(i).Cells(4).Value
                                .Parameters.Add(New SqlParameter("@account", SqlDbType.VarChar)).Value = If(CInt(DataGridView.Rows(i).Cells(16).Value) = 1, "cash account", "bank account")
                                If CellIsEmpty(DataGridView, i, 20) Then
                                    .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = CStr(DataGridView.Rows(i).Cells(20).Value).ToLower
                                End If
                                .Parameters.Add(New SqlParameter("@debit", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(15).Value
                                .Parameters.Add(New SqlParameter("@credit", SqlDbType.Decimal)).Value = 0
                                If CellIsEmpty(DataGridView, i, 2) Then
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = CInt(DataGridView.Rows(i).Cells(2).Value)
                                End If
                                .Parameters.Add(New SqlParameter("@aid", SqlDbType.Int)).Value = 6
                                .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 5
                                .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID + i
                                .ExecuteNonQuery()
                            End With
                        End Using

                        If DataGridView.Rows(i).Cells(16).Value > 2 Then
                            sql = "INSERT INTO tblBankPostings (transactionDate,bankId,credit,description,reference,paymethodId,transactionType,transactionId) "
                            sql &= "VALUES(@date,@bid,@credit,@desc,@ref,@pid,@type,@tid)"
                            Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                                With cmd
                                    .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = DataGridView.Rows(i).Cells(4).Value
                                    .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(18).Value
                                    .Parameters.Add(New SqlParameter("@credit", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(15).Value
                                    If CellIsEmpty(DataGridView, i, 20) Then
                                        .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = DBNull.Value
                                    Else
                                        .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = CStr(DataGridView.Rows(i).Cells(20).Value).ToLower
                                    End If
                                    If CellIsEmpty(DataGridView, i, 19) Then
                                        .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = DBNull.Value
                                    Else
                                        .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = CStr(DataGridView.Rows(i).Cells(19).Value).ToLower
                                    End If
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(16).Value
                                    .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 5
                                    .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID + i
                                    .ExecuteNonQuery()
                                End With
                            End Using
                        End If
                    Next

                    MyTransaction.Commit()
                    AppClass.ShowNotification("Saved Successfully")
                    Reset()

                Catch ex As InvalidOperationException
                    AppClass.ShowError(ex.Message)
                    MyTransaction.Rollback()
                Catch ex As SqlException
                    AppClass.ShowError(ex.Message)
                    MyTransaction.Rollback()
                Catch ex As Exception
                    AppClass.ShowError(ex.Message)
                    MyTransaction.Rollback()
                End Try
            End Using
        End Using
    End Sub
    Sub Find(fid As Integer)
        IsEdit = True

        AddParams("@id", fid)
        Dim ExpenseRef As String = CStr(AppClass.FetchDBValue("SELECT voucherNo FROM tblExpenses WHERE ID=@id"))

        ExpenseRefTextEdit.EditValue = ExpenseRef
        AddParams("@voucherno", ExpenseRef)
        Dim Searchdt As DataTable = AppClass.LoadToDatatable("spExpensesByRef", True)

        For Each row In Searchdt.Rows
            DataGridView.Rows.Add(row(0), row(1),
                                      If(row(2) = 0, Nothing, row(2)),
                                      If(IsDBNull(row(3)), Nothing, row(3)),
                                      row(4), row(5),
                                      row(6), CDec(row(7)).ToString("N"),
                                      If(row(8) = 0, Nothing, row(8)),
                                      row(9),
                                      row(10),
                                      CDec(row(11)).ToString("N"), row(12), CDec(row(13)).ToString("N"),
                                      CDec(row(14)).ToString("N"), CDec(row(15)).ToString("N"), row(16),
                                      row(17), If(row(18) = 0, Nothing, row(18)),
                                      row(19),
                                      row(20),
                                      If(IsDBNull(row(21)), Nothing, row(21)), If(IsDBNull(row(22)), Nothing, row(22)), row(23), row(24))
        Next
        DeleteSimpleButton.Enabled = True
        ExpenseTotalTextEdit.EditValue = CDec(AppClass.GetGridTotal(DataGridView, 13)).ToString("N")
    End Sub
    Sub Edit()
        ID = AppClass.GenerateDBID("tblExpenses")
        Dim IndebtnessId As Integer = AppClass.GenerateDBID("tblStaffIndebtness")

        Using Connection As New SqlConnection(connstr)
            With Connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With
            Using MyTransaction As SqlTransaction = Connection.BeginTransaction
                Try

                    sql = "DELETE FROM tblExpenses WHERE voucherNo=@voucher"
                    Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                        cmd.Parameters.Add(New SqlParameter("@voucher", SqlDbType.VarChar)).Value = ExpenseRefTextEdit.EditValue
                        cmd.ExecuteNonQuery()
                    End Using

                    For i = 0 To DataGridView.Rows.Count - 1
                        sql = "INSERT INTO tblExpenses (ID,expenseDate,voucherNo,amount,glCodeId,projectId,description,expenseType,paymentMethodId,"
                        sql &= "bankId,paymentReference,discountPercent,discountAmount,totalDiscount,vatType,exclusiveAmount,vat,inclusiveAmount,transactionType,staffId,isBulk,isPettyCash) "
                        sql &= "VALUES(@id,@date,@voucher,@amount,@gl,@pid,@desc,@type,@pay,@bid,@ref,@discper,@discamount,@disc,@vtype,@exc,@vat,@inc,@ttype,@sid,@is,@petty)"

                        Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = ID + i
                                .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = DataGridView.Rows(i).Cells(4).Value
                                .Parameters.Add(New SqlParameter("@voucher", SqlDbType.VarChar)).Value = ExpenseRefTextEdit.EditValue
                                .Parameters.Add(New SqlParameter("@amount", SqlDbType.Decimal)).Value = CDec(DataGridView.Rows(i).Cells(7).Value)
                                .Parameters.Add(New SqlParameter("@gl", SqlDbType.Int)).Value = CInt(DataGridView.Rows(i).Cells(5).Value)
                                If CellIsEmpty(DataGridView, i, 2) Then
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = CInt(DataGridView.Rows(i).Cells(2).Value)
                                End If
                                If CellIsEmpty(DataGridView, i, 20) Then
                                    .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = CStr(DataGridView.Rows(i).Cells(20).Value).ToLower
                                End If
                                .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = If(CInt(DataGridView.Rows(i).Cells(0).Value) = 1, 2, 1)
                                .Parameters.Add(New SqlParameter("@pay", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(16).Value
                                If CellIsEmpty(DataGridView, i, 18) Then
                                    .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(18).Value
                                End If
                                If CellIsEmpty(DataGridView, i, 19) Then
                                    .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = DataGridView.Rows(i).Cells(19).Value
                                End If
                                If CellIsEmpty(DataGridView, i, 8) Then
                                    .Parameters.Add(New SqlParameter("@discper", SqlDbType.Decimal)).Value = DBNull.Value
                                    .Parameters.Add(New SqlParameter("@discamount", SqlDbType.Decimal)).Value = DBNull.Value
                                    .Parameters.Add(New SqlParameter("@disc", SqlDbType.Decimal)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@discper", SqlDbType.Decimal)).Value = If(DataGridView.Rows(i).Cells(8).Value = 1, DataGridView.Rows(i).Cells(9).Value, DBNull.Value)
                                    .Parameters.Add(New SqlParameter("@discamount", SqlDbType.Decimal)).Value = If(DataGridView.Rows(i).Cells(8).Value = 2, DataGridView.Rows(i).Cells(9).Value, DBNull.Value)
                                    .Parameters.Add(New SqlParameter("@disc", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(10).Value
                                End If
                                .Parameters.Add(New SqlParameter("@vtype", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(12).Value
                                .Parameters.Add(New SqlParameter("@exc", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(13).Value
                                .Parameters.Add(New SqlParameter("@vat", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(14).Value
                                .Parameters.Add(New SqlParameter("@inc", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(15).Value
                                .Parameters.Add(New SqlParameter("@ttype", SqlDbType.Int)).Value = 5
                                If CellIsEmpty(DataGridView, i, 21) Then
                                    .Parameters.Add(New SqlParameter("@sid", SqlDbType.VarChar)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@sid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(21).Value
                                End If
                                .Parameters.Add(New SqlParameter("@is", SqlDbType.Bit)).Value = 1
                                .Parameters.Add(New SqlParameter("@petty", SqlDbType.Bit)).Value = DataGridView.Rows(i).Cells(23).Value
                                .ExecuteNonQuery()
                            End With
                        End Using

                        If DataGridView.Rows(i).Cells(24).Value IsNot Nothing Then
                            sql = "DELETE FROM tblStaffIndebtness WHERE transactionType=5 AND transactionId=@id"
                            Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                                cmd.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(24).Value
                                cmd.ExecuteNonQuery()
                            End Using

                            sql = "DELETE FROM tblLedger WHERE transactionType=5 AND transactionId=@id"
                            Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                                cmd.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(24).Value
                                cmd.ExecuteNonQuery()
                            End Using

                            sql = "DELETE FROM tblBankPostings WHERE transactionType=5 AND transactionId=@id"
                            Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                                cmd.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(24).Value
                                cmd.ExecuteNonQuery()
                            End Using
                        End If

                        If Not CellIsEmpty(DataGridView, i, 21) Then
                            sql = "INSERT INTO tblStaffIndebtness (ID,transactionDate,staffId,credit,reference,narration,transactionType,transactionId) "
                            sql &= "VALUES(@id,@date,@sid,@credit,@ref,@nar,@type,@tid)"
                            Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                                With cmd
                                    .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = IndebtnessId + i
                                    .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = DataGridView.Rows(i).Cells(4).Value
                                    .Parameters.Add(New SqlParameter("@sid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(21).Value
                                    .Parameters.Add(New SqlParameter("@credit", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(15).Value
                                    If CellIsEmpty(DataGridView, i, 19) Then
                                        .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = DBNull.Value
                                    Else
                                        .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = DataGridView.Rows(i).Cells(19).Value
                                    End If
                                    If CellIsEmpty(DataGridView, i, 20) Then
                                        .Parameters.Add(New SqlParameter("@nar", SqlDbType.Int)).Value = DBNull.Value
                                    Else
                                        .Parameters.Add(New SqlParameter("@nar", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(20).Value
                                    End If
                                    .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 5
                                    .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID + i
                                    .ExecuteNonQuery()
                                End With
                            End Using
                        End If

                        sql = "INSERT INTO tblLedger (transactionDate,account,description,debit,credit,projectId,accountId,transactionType,transactionID) "
                        sql &= "VALUES(@date,@account,@desc,@debit,@credit,@pid,@aid,@type,@tid)"
                        Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = DataGridView.Rows(i).Cells(4).Value
                                .Parameters.Add(New SqlParameter("@account", SqlDbType.VarChar)).Value = CStr(DataGridView.Rows(i).Cells(6).Value).ToLower
                                If CellIsEmpty(DataGridView, i, 20) Then
                                    .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = DataGridView.Rows(i).Cells(20).Value
                                End If
                                .Parameters.Add(New SqlParameter("@debit", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(15).Value
                                .Parameters.Add(New SqlParameter("@credit", SqlDbType.Decimal)).Value = 0
                                If CellIsEmpty(DataGridView, i, 2) Then
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(2).Value
                                End If
                                .Parameters.Add(New SqlParameter("@aid", SqlDbType.Int)).Value = 6
                                .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 5
                                .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID + i
                                .ExecuteNonQuery()
                            End With
                        End Using

                        sql = "INSERT INTO tblLedger (transactionDate,account,description,debit,credit,projectId,accountId,transactionType,transactionID) "
                        sql &= "VALUES(@date,@account,@desc,@debit,@credit,@pid,@aid,@type,@tid)"
                        Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = DataGridView.Rows(i).Cells(4).Value
                                .Parameters.Add(New SqlParameter("@account", SqlDbType.VarChar)).Value = If(CInt(DataGridView.Rows(i).Cells(16).Value) = 1, "cash account", "bank account")
                                If CellIsEmpty(DataGridView, i, 20) Then
                                    .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = CStr(DataGridView.Rows(i).Cells(20).Value).ToLower
                                End If
                                .Parameters.Add(New SqlParameter("@debit", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(15).Value
                                .Parameters.Add(New SqlParameter("@credit", SqlDbType.Decimal)).Value = 0
                                If CellIsEmpty(DataGridView, i, 2) Then
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = DBNull.Value
                                Else
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = CInt(DataGridView.Rows(i).Cells(2).Value)
                                End If
                                .Parameters.Add(New SqlParameter("@aid", SqlDbType.Int)).Value = 6
                                .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 5
                                .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID + i
                                .ExecuteNonQuery()
                            End With
                        End Using

                        If DataGridView.Rows(i).Cells(16).Value > 2 Then
                            sql = "INSERT INTO tblBankPostings (transactionDate,bankId,credit,description,reference,paymethodId,transactionType,transactionId) "
                            sql &= "VALUES(@date,@bid,@credit,@desc,@ref,@pid,@type,@tid)"
                            Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                                With cmd
                                    .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = DataGridView.Rows(i).Cells(4).Value
                                    .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(18).Value
                                    .Parameters.Add(New SqlParameter("@credit", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(15).Value
                                    If CellIsEmpty(DataGridView, i, 20) Then
                                        .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = DBNull.Value
                                    Else
                                        .Parameters.Add(New SqlParameter("@desc", SqlDbType.VarChar)).Value = CStr(DataGridView.Rows(i).Cells(20).Value).ToLower
                                    End If
                                    If CellIsEmpty(DataGridView, i, 19) Then
                                        .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = DBNull.Value
                                    Else
                                        .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = CStr(DataGridView.Rows(i).Cells(19).Value).ToLower
                                    End If
                                    .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(16).Value
                                    .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 5
                                    .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID + i
                                    .ExecuteNonQuery()
                                End With
                            End Using
                        End If
                    Next

                    MyTransaction.Commit()
                    AppClass.ShowNotification("Edited Successfully")
                    Reset()

                Catch ex As InvalidOperationException
                    AppClass.ShowError(ex.Message)
                    MyTransaction.Rollback()
                Catch ex As SqlException
                    AppClass.ShowError(ex.Message)
                    MyTransaction.Rollback()
                Catch ex As Exception
                    AppClass.ShowError(ex.Message)
                    MyTransaction.Rollback()
                End Try
            End Using
        End Using
    End Sub
    Sub Delete()
        Using Connection As New SqlConnection(connstr)
            With Connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With
            Using MyTransaction As SqlTransaction = Connection.BeginTransaction
                Try

                    sql = "DELETE FROM tblExpenses WHERE voucherNo=@voucher"
                    Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                        cmd.Parameters.Add(New SqlParameter("@voucher", SqlDbType.VarChar)).Value = ExpenseRefTextEdit.EditValue
                        cmd.ExecuteNonQuery()
                    End Using

                    For i = 0 To DataGridView.Rows.Count - 1
                        If DataGridView.Rows(i).Cells(24).Value IsNot Nothing Then
                            sql = "DELETE FROM tblStaffIndebtness WHERE transactionType=5 AND transactionId=@id"
                            Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                                cmd.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(24).Value
                                cmd.ExecuteNonQuery()
                            End Using

                            sql = "DELETE FROM tblLedger WHERE transactionType=5 AND transactionId=@id"
                            Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                                cmd.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(24).Value
                                cmd.ExecuteNonQuery()
                            End Using

                            sql = "DELETE FROM tblBankPostings WHERE transactionType=5 AND transactionId=@id"
                            Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                                cmd.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(24).Value
                                cmd.ExecuteNonQuery()
                            End Using
                        End If
                    Next

                    MyTransaction.Commit()
                    AppClass.ShowNotification("Deleted Successfully")
                    Reset()

                Catch ex As InvalidOperationException
                    AppClass.ShowError(ex.Message)
                    MyTransaction.Rollback()
                Catch ex As SqlException
                    AppClass.ShowError(ex.Message)
                    MyTransaction.Rollback()
                Catch ex As Exception
                    AppClass.ShowError(ex.Message)
                    MyTransaction.Rollback()
                End Try
            End Using
        End Using
    End Sub
#End Region
#Region "EVENTS"
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
    Private Sub ExpenseTypeComboBoxEdit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ExpenseTypeComboBoxEdit.SelectedIndexChanged
        With ProjectLookUpBoxEdit
            .Properties.ReadOnly = ExpenseTypeComboBoxEdit.SelectedIndex = 0
            .EditValue = Nothing
        End With
    End Sub
    Private Sub PaymentMethodLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles PaymentMethodLookUpEdit.EditValueChanged
        If PaymentMethodLookUpEdit.EditValue IsNot Nothing Then
            AddParams("@id", CInt(PaymentMethodLookUpEdit.EditValue))
            BankRequired = CBool(AppClass.FetchDBValue("SELECT bankRequired FROM tblPaymentMethod WHERE ID=@id"))
            BankLookUpEdit.Properties.ReadOnly = Not BankRequired
        Else
            BankLookUpEdit.Properties.ReadOnly = False
        End If
        BankLookUpEdit.EditValue = Nothing
    End Sub
    Private Sub DiscountTypeComboBoxEdit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles DiscountTypeComboBoxEdit.SelectedIndexChanged
        DiscountValueTextEdit.Properties.ReadOnly = Not DiscountTypeComboBoxEdit.SelectedIndex > -1
        DiscountValueTextEdit.EditValue = Nothing
        NetAmountTextEdit.EditValue = CalculateNetAmount().ToString("N")
    End Sub
    Private Sub ExpenseAmountTextEdit_Leave(sender As Object, e As EventArgs) Handles ExpenseAmountTextEdit.Leave
        If ExpenseAmountTextEdit.EditValue IsNot Nothing Then
            NetAmountTextEdit.EditValue = CalculateNetAmount().ToString("N")
            CalculateVAT()
        End If
    End Sub
    Private Sub AddSimpleButton_Click(sender As Object, e As EventArgs) Handles AddSimpleButton.Click
        If Not Validation() Then
            Return
        End If

        Select Case AddSimpleButton.Text
            Case "Edit"
                DataGridView.CurrentRow.Cells(0).Value = ExpenseTypeComboBoxEdit.SelectedIndex + 1
                DataGridView.CurrentRow.Cells(1).Value = ExpenseTypeComboBoxEdit.Text
                DataGridView.CurrentRow.Cells(2).Value = If(ExpenseTypeComboBoxEdit.SelectedIndex = 0, Nothing, ProjectLookUpBoxEdit.EditValue)
                DataGridView.CurrentRow.Cells(3).Value = If(ExpenseTypeComboBoxEdit.SelectedIndex = 0, Nothing, ProjectLookUpBoxEdit.Text)
                DataGridView.CurrentRow.Cells(4).Value = CDate(ExpenseDateEdit.EditValue).ToString("dd-MM-yyyy")
                DataGridView.CurrentRow.Cells(5).Value = ExpenseAccountLookUpEdit.EditValue
                DataGridView.CurrentRow.Cells(6).Value = ExpenseAccountLookUpEdit.Text
                DataGridView.CurrentRow.Cells(7).Value = CDec(ExpenseAmountTextEdit.EditValue).ToString("N")
                DataGridView.CurrentRow.Cells(8).Value = If(DiscountTypeComboBoxEdit.EditValue IsNot Nothing, DiscountTypeComboBoxEdit.SelectedIndex + 1, 0)
                DataGridView.CurrentRow.Cells(9).Value = If(DiscountTypeComboBoxEdit.EditValue IsNot Nothing, DiscountValueTextEdit.EditValue, 0)
                DataGridView.CurrentRow.Cells(10).Value = If(DiscountTypeComboBoxEdit.EditValue IsNot Nothing, CDec(DiscountedAmountTextEdit.EditValue).ToString("N"), 0)
                DataGridView.CurrentRow.Cells(11).Value = NetAmountTextEdit.EditValue
                DataGridView.CurrentRow.Cells(12).Value = VatTypeLookUpEdit.SelectedIndex + 1
                DataGridView.CurrentRow.Cells(13).Value = ExclusiveTextEdit.EditValue
                DataGridView.CurrentRow.Cells(14).Value = CDec(VatAmountTextEdit.EditValue)
                DataGridView.CurrentRow.Cells(15).Value = CDec(InclusiveTextEdit.EditValue)
                DataGridView.CurrentRow.Cells(16).Value = PaymentMethodLookUpEdit.EditValue
                DataGridView.CurrentRow.Cells(17).Value = PaymentMethodLookUpEdit.Text
                DataGridView.CurrentRow.Cells(18).Value = BankLookUpEdit.EditValue
                DataGridView.CurrentRow.Cells(19).Value = PaymentReferenceTextEdit.EditValue?.ToString.ToUpper
                DataGridView.CurrentRow.Cells(20).Value = DescriptionTextEdit.EditValue?.ToString.ToUpper
                DataGridView.CurrentRow.Cells(21).Value = ExpensedToLookUpEdit.EditValue
                DataGridView.CurrentRow.Cells(22).Value = ExpensedToLookUpEdit.Text
                DataGridView.CurrentRow.Cells(23).Value = PettyCashCheckEdit.Checked
            Case "Add"
                DataGridView.Rows.Add(ExpenseTypeComboBoxEdit.SelectedIndex + 1, ExpenseTypeComboBoxEdit.Text,
                                      If(ExpenseTypeComboBoxEdit.SelectedIndex = 0, Nothing, ProjectLookUpBoxEdit.EditValue),
                                      If(ExpenseTypeComboBoxEdit.SelectedIndex = 0, Nothing, ProjectLookUpBoxEdit.Text),
                                      CDate(ExpenseDateEdit.EditValue).ToString("dd-MM-yyyy"), ExpenseAccountLookUpEdit.EditValue,
                                      ExpenseAccountLookUpEdit.Text, CDec(ExpenseAmountTextEdit.EditValue).ToString("N"),
                                      If(DiscountTypeComboBoxEdit.EditValue IsNot Nothing, DiscountTypeComboBoxEdit.SelectedIndex + 1, 0),
                                      If(DiscountTypeComboBoxEdit.EditValue IsNot Nothing, DiscountValueTextEdit.EditValue, 0),
                                      If(DiscountTypeComboBoxEdit.EditValue IsNot Nothing, CDec(DiscountedAmountTextEdit.EditValue).ToString("N"), 0),
                                      NetAmountTextEdit.EditValue, VatTypeLookUpEdit.SelectedIndex + 1, ExclusiveTextEdit.EditValue,
                                      CDec(VatAmountTextEdit.EditValue), CDec(InclusiveTextEdit.EditValue), PaymentMethodLookUpEdit.EditValue,
                                      PaymentMethodLookUpEdit.Text, BankLookUpEdit.EditValue,
                                      PaymentReferenceTextEdit.EditValue?.ToString.ToUpper,
                                      DescriptionTextEdit.EditValue?.ToString.ToUpper,
                                      ExpensedToLookUpEdit.EditValue, ExpensedToLookUpEdit.Text, PettyCashCheckEdit.Checked)
        End Select
        InpusReset()
        ExpenseTotalTextEdit.EditValue = CDec(AppClass.GetGridTotal(DataGridView, 13)).ToString("N")
    End Sub
    Private Sub DiscountValueTextEdit_Leave(sender As Object, e As EventArgs) Handles DiscountValueTextEdit.Leave
        If DiscountValueTextEdit.EditValue IsNot Nothing Then
            NetAmountTextEdit.EditValue = CalculateNetAmount().ToString("N")
            CalculateVAT()
        End If
    End Sub
    Private Sub VatTypeLookUpEdit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles VatTypeLookUpEdit.SelectedIndexChanged
        If VatTypeLookUpEdit.EditValue IsNot Nothing AndAlso NetAmountTextEdit.EditValue IsNot Nothing Then
            CalculateVAT()
        End If
    End Sub
    Private Sub DataGridView_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView.CellDoubleClick
        ExpenseTypeComboBoxEdit.SelectedIndex = CInt(DataGridView.CurrentRow.Cells(0).Value) - 1
        ProjectLookUpBoxEdit.EditValue = DataGridView.CurrentRow.Cells(2).Value
        ExpenseDateEdit.EditValue = CDate(DataGridView.CurrentRow.Cells(4).Value)
        ExpenseAccountLookUpEdit.EditValue = DataGridView.CurrentRow.Cells(5).Value
        ExpenseAmountTextEdit.EditValue = CDec(DataGridView.CurrentRow.Cells(7).Value)
        If DataGridView.CurrentRow.Cells(8).Value IsNot Nothing Then
            DiscountTypeComboBoxEdit.SelectedIndex = CInt(DataGridView.CurrentRow.Cells(8).Value) - 1
        Else
            DiscountTypeComboBoxEdit.SelectedIndex = -1
        End If
        DiscountValueTextEdit.EditValue = DataGridView.CurrentRow.Cells(9).Value
        DiscountedAmountTextEdit.EditValue = DataGridView.CurrentRow.Cells(10).Value
        NetAmountTextEdit.EditValue = CDec(DataGridView.CurrentRow.Cells(11).Value).ToString("N")
        VatTypeLookUpEdit.SelectedIndex = CInt(DataGridView.CurrentRow.Cells(12).Value) - 1
        ExclusiveTextEdit.EditValue = CDec(DataGridView.CurrentRow.Cells(13).Value).ToString("N")
        VatAmountTextEdit.EditValue = CDec(DataGridView.CurrentRow.Cells(14).Value).ToString("N")
        InclusiveTextEdit.EditValue = CDec(DataGridView.CurrentRow.Cells(15).Value).ToString("N")
        PaymentMethodLookUpEdit.EditValue = DataGridView.CurrentRow.Cells(16).Value
        BankLookUpEdit.EditValue = DataGridView.CurrentRow.Cells(18).Value
        PaymentReferenceTextEdit.EditValue = DataGridView.CurrentRow.Cells(19).Value?.ToString.ToUpper
        DescriptionTextEdit.EditValue = DataGridView.CurrentRow.Cells(20).Value?.ToString.ToUpper
        ExpensedToLookUpEdit.EditValue = DataGridView.CurrentRow.Cells(21).Value
        PettyCashCheckEdit.Checked = DataGridView.CurrentRow.Cells(23).Value
        AddSimpleButton.Text = "Edit"
    End Sub
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If DataGridView.Rows.Count = 0 Then
            AppClass.ShowError("No expenses added", True)
            Return
        End If

        If Not IsEdit Then
            Save()
        Else
            Edit()
        End If
    End Sub
    Private Sub FindSimpleButton_Click(sender As Object, e As EventArgs) Handles FindSimpleButton.Click
        Dim SearchBy As String = "Search by expense account,amount or description"
        Using frm As New SearchForm(SearchBy, "spSearchExpenses")
            If frm.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                Find(frm.DataGridView.CurrentRow.Cells(0).Value)
            End If
        End Using
    End Sub
    Private Sub DeleteSimpleButton_Click(sender As Object, e As EventArgs) Handles DeleteSimpleButton.Click
        If AppClass.AlertQuestion("Are you sure you want to delete this expense(s)?") = DialogResult.Yes Then
            Delete()
        End If
    End Sub
#End Region
End Class
