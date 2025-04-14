Imports System.Data.SqlClient
Public Class UCInterestOnDeposit
    Private Shared _instance As UCInterestOnDeposit
    Private IsEdit As Boolean
    Private ID As Integer
    Public Shared ReadOnly Property Instance As UCInterestOnDeposit
        Get
            If _instance Is Nothing Then _instance = New UCInterestOnDeposit()
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
        sql = "SELECT M.memberNo,M.memberID AS [MemberID],UPPER(M.memberName) AS [MemberName],M.idNo AS [IDNo],"
        sql &= "A.contact AS [Contact] FROM tblMember M LEFT OUTER JOIN tblMemberAddress A ON M.memberNo=A.memberNo ORDER BY M.memberID"
        AppClass.LoadToSearchLookUpEdit(sql, MemberSearchLookUpEdit, "MemberName", "memberNo")
        DeleteSimpleButton.Enabled = False
        PaymentDateEdit.EditValue = Date.Now
        PaymentMethodLookUpEdit.Properties.ReadOnly = True
        BankLookUpEdit.Properties.ReadOnly = True
        AccountMpesaTextEdit.Properties.ReadOnly = True
        PloughOptionsComboBoxEdit.Properties.ReadOnly = True
        ReferenceTextEdit.Properties.ReadOnly = True
        AppClass.LoadToLookUpEdit("SELECT ID,methodName FROM tblPaymentMethod", PaymentMethodLookUpEdit, "methodName", "ID")
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(bankName) AS bankName FROM tblBanks", BankLookUpEdit, "bankName", "ID")
        AppClass.LoadToLookUpEdit("SELECT ID,purposeName FROM tblPaymentPurpose WHERE ID > 1", PloughOptionsComboBoxEdit, "purposeName", "ID")
        ProjectLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        UnitLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        AppClass.ClearItems(LayoutControl1)
    End Sub
    Function Datavalidation() As Boolean
        errmsg = ""
        If String.IsNullOrEmpty(AmountTextEdit.Text) Then
            errmsg = "Enter Amount"
            AmountTextEdit.Focus()
        ElseIf MemberSearchLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select Member"
            MemberSearchLookUpEdit.Focus()
        ElseIf DividendYearTextEdit.EditValue Is Nothing Then
            errmsg = "Enter Year"
            DividendYearTextEdit.Focus()
        ElseIf PaymentDateEdit.EditValue Is Nothing Then
            errmsg = "Select Date"
            PaymentDateEdit.Focus()
        ElseIf PaymentTypeComboBoxEdit.SelectedIndex = 1 AndAlso PaymentMethodLookUpEdit.EditValue = Nothing Then
            errmsg = "Select Payment Method"
            PaymentMethodLookUpEdit.Focus()
        ElseIf PaymentTypeComboBoxEdit.SelectedIndex = 1 AndAlso PaymentMethodLookUpEdit.EditValue > 1 AndAlso String.IsNullOrEmpty(AccountMpesaTextEdit.Text) Then
            errmsg = "Enter Account Paid"
            AccountMpesaTextEdit.Focus()
        ElseIf PaymentTypeComboBoxEdit.SelectedIndex = 1 AndAlso String.IsNullOrEmpty(ReferenceTextEdit.Text) Then
            errmsg = "Enter Reference"
            ReferenceTextEdit.Focus()
        End If
        If errmsg <> "" Then
            AppClass.ShowNotification(errmsg)
            Return False
        Else
            Return True
        End If
    End Function
    Function GetBalance(memno As Integer, unit As Integer) As Decimal
        Dim balance As Decimal
        Dim sellingPrice As Decimal
        Dim paidAmount As Decimal
        AddParams("@no", memno)
        AddParams("@id", unit)
        sellingPrice = CDec(AppClass.FetchDBValue("SELECT ISNULL(S.netAmount,0) AS netAmount FROM   tblUnits U INNER JOIN tblUnitSale S ON U.ID=S.unitId WHERE  U.memberNo=@no AND S.unitId=@id"))
        AddParams("@id", unit)
        paidAmount = CDec(AppClass.FetchDBValue("SELECT ISNULL(SUM(amountPaid),0) AS paidAmount FROM tblUnitPayment WHERE unitId=@id"))
        balance = sellingPrice - paidAmount
        Return balance
    End Function
    Sub SavePlough()
        Dim myTransaction As SqlTransaction = Nothing
        Dim PloughType As Integer

        Try
            Dim tid As Integer = CInt(AppClass.GenerateDBID("tblTransfers"))
            Dim TransferRef = AppClass.FnGenerateID("SELECT MAX(RIGHT(reference,5)) AS ref FROM tblTransfers", "ref", "00001", "00000")

            If PloughOptionsComboBoxEdit.EditValue = 2 Then
                PloughType = 6
            ElseIf PloughOptionsComboBoxEdit.EditValue = 3 Then
                PloughType = 7
            ElseIf PloughOptionsComboBoxEdit.EditValue = 4 Then
                PloughType = 8
            Else
                PloughType = 6
            End If

            Using connection = New SqlConnection(connstr)
                With connection
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With
                myTransaction = connection.BeginTransaction

                sql = "INSERT INTO tblBonus (ID,memberNo,amount,yearName,staus,transferRef,PloughedAs,TransactionType) "
                sql &= "VALUES(@id,@member,@amount,@year,@status,@tref,@plough,@type)"
                Using cmd = New SqlCommand(sql, connection, myTransaction)
                    With cmd
                        .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = ID
                        .Parameters.Add(New SqlParameter("@member", SqlDbType.Int)).Value = MemberSearchLookUpEdit.EditValue
                        .Parameters.Add(New SqlParameter("@amount", SqlDbType.Decimal)).Value = CDec(AmountTextEdit.Text)
                        .Parameters.Add(New SqlParameter("@year", SqlDbType.VarChar)).Value = DividendYearTextEdit.Text
                        .Parameters.Add(New SqlParameter("@status", SqlDbType.Int)).Value = 1
                        .Parameters.Add(New SqlParameter("@tref", SqlDbType.Int)).Value = tid
                        .Parameters.Add(New SqlParameter("@plough", SqlDbType.Int)).Value = PloughOptionsComboBoxEdit.EditValue
                        .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = "interest on deposit"
                        .ExecuteNonQuery()
                    End With
                End Using

                sql = "INSERT INTO tblTransfers (ID,transferType,transferDate,transferFrom,transferTo,amount,projectId,unitId,reference) "
                sql &= "VALUES(@id,@type,@date,@from,@to,@amount,@pid,@uid,@ref)"
                Using cmd = New SqlCommand(sql, connection, myTransaction)
                    With cmd
                        .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = tid
                        .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = PloughType
                        .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = CDate(PaymentDateEdit.EditValue).Date
                        .Parameters.Add(New SqlParameter("@from", SqlDbType.Int)).Value = CInt(MemberSearchLookUpEdit.EditValue)
                        .Parameters.Add(New SqlParameter("@to", SqlDbType.Int)).Value = CInt(MemberSearchLookUpEdit.EditValue)
                        .Parameters.Add(New SqlParameter("@amount", SqlDbType.Decimal)).Value = CDec(AmountTextEdit.Text)
                        .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = DBNull.Value
                        .Parameters.Add(New SqlParameter("@uid", SqlDbType.Int)).Value = DBNull.Value
                        .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = TransferRef
                        .ExecuteNonQuery()
                    End With
                End Using

                If PloughOptionsComboBoxEdit.EditValue = 4 Then
                    AddParams("@id", UnitLookUpEdit.EditValue)
                    Dim paidAmount As Decimal = CDec(AppClass.FetchDBValue("SELECT ISNULL(unitPaid,0) AS paidAmount FROM tblUnits WHERE ID=@id"))
                    Dim totalPaidAmount As Decimal = CDec(AmountTextEdit.EditValue) + paidAmount

                    sql = "UPDATE tblUnits SET unitPaid=@newamount WHERE ID=@id"
                    Using cmd = New SqlCommand(sql, connection, myTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@newamount", SqlDbType.Decimal)).Value = totalPaidAmount
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = UnitLookUpEdit.EditValue
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "INSERT INTO tblUnitPayment (unitId,openingBal,paymentDate,amountPaid,closingBal,transactionType,transactionID) VALUES(@uid,@obal,@date,@paid,@cbal,@type,@tid)"
                    Using cmd = New SqlCommand(sql, connection, myTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@uid", SqlDbType.Int).Value = UnitLookUpEdit.EditValue
                            .Parameters.Add("@obal", SqlDbType.Decimal).Value = CDec(BalanceTextEdit.EditValue)
                            .Parameters.Add("@date", SqlDbType.Date).Value = CDate(PaymentDateEdit.EditValue).Date
                            .Parameters.Add("@paid", SqlDbType.Decimal).Value = CDec(AmountTextEdit.EditValue)
                            Dim bal As Decimal = CDec(BalanceTextEdit.EditValue) - CDec(AmountTextEdit.EditValue)
                            .Parameters.Add("@cbal", SqlDbType.Decimal).Value = bal
                            .Parameters.Add("@type", SqlDbType.Int).Value = 13
                            .Parameters.Add("@tid", SqlDbType.Int).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using
                End If

                sql = "INSERT INTO tblLogs VALUES(@uid,@act,@actdate)"
                Using cmd = New SqlCommand(sql, connection, myTransaction)
                    With cmd
                        .Parameters.Clear()
                        .Parameters.Add("@uid", SqlDbType.Int).Value = LogedUserID
                        .Parameters.Add("@act", SqlDbType.NVarChar).Value = "Dividend Payment For " & MemberSearchLookUpEdit.Text & " For " & DividendYearTextEdit.Text
                        .Parameters.Add("@actdate", SqlDbType.Date).Value = Date.Now.Date
                        .ExecuteNonQuery()
                    End With
                End Using
                myTransaction.Commit()
                myTransaction.Dispose()

                If PloughOptionsComboBoxEdit.EditValue = 2 Then
                    AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "member rebates", "Bonus Transfer", 0, CDec(AmountTextEdit.Text), Nothing, 4, 11, ID)
                    AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "shares", "Bonus Transfer", CDec(AmountTextEdit.Text), 0, Nothing, 4, 11, ID)
                ElseIf PloughOptionsComboBoxEdit.EditValue = 3 Then
                    AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "member rebates", "Bonus Transfer", 0, CDec(AmountTextEdit.Text), Nothing, 4, 11, ID)
                    AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "deposits", "Bonus Transfer", CDec(AmountTextEdit.Text), 0, Nothing, 3, 11, ID)
                ElseIf PloughOptionsComboBoxEdit.EditValue = 4 Then
                    AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "member rebates", "Bonus Transfer", 0, CDec(AmountTextEdit.Text), Nothing, 4, 11, ID)
                    AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "unit payment", "Bonus Transfer", CDec(AmountTextEdit.Text), 0, Nothing, 5, 11, ID)
                End If


            End Using
        Catch ex As Exception
            If myTransaction IsNot Nothing Then
                myTransaction.Rollback()
            End If
            AppClass.ShowError(ex.Message)
        End Try
    End Sub
    Sub Save()
        If Not String.IsNullOrEmpty(ReferenceTextEdit.Text) Then
            AddParams("@ref", ReferenceTextEdit.Text.ToLower.ToString)
            If CInt(AppClass.FetchDBValue("SELECT COUNT(ID) FROM tblBonus WHERE reference=@ref")) > 0 Then
                AppClass.ShowNotification("Reference Already Exists")
                With ReferenceTextEdit
                    .Focus()
                    .SelectAll()
                End With
                Exit Sub
            End If
        End If

        ID = CInt(AppClass.GenerateDBID("tblBonus"))
        If PaymentTypeComboBoxEdit.SelectedIndex = 0 Then
            SavePlough()
            'AppClass.AddToLedger(CDate(Date.Now).Date, "Share Capital", "Dividend Payment", CDec(AmountTextEdit.EditValue), 0, Nothing, 4, 13, ID)
            'AppClass.AddToLedger(CDate(Date.Now.Date), "Bank Account", "Dividend Payment", 0, CDec(AmountTextEdit.EditValue), Nothing, 1, 13, ID)
            AppClass.ShowNotification("Saved Successfully!")
            Reset()
        ElseIf PaymentTypeComboBoxEdit.SelectedIndex = 1 Then
            AddParams("@id", ID)
            AddParams("@member", MemberSearchLookUpEdit.EditValue)
            AddParams("@amount", AmountTextEdit.EditValue)
            AddParams("@year", DividendYearTextEdit.EditValue)
            AddParams("@status", 2)
            AddParams("@date", CDate(PaymentDateEdit.EditValue).Date)
            AddParams("@pay", PaymentMethodLookUpEdit.EditValue)
            AddParams("@acc", AccountMpesaTextEdit.EditValue.ToLower.ToLower)
            AddParams("@ref", ReferenceTextEdit.EditValue.ToLower.ToString)
            If PaymentMethodLookUpEdit.EditValue > 2 Then
                AddParams("@bid", BankLookUpEdit.EditValue)
            Else
                AddParams("@bid", DBNull.Value)
            End If
            AppClass.ExecQuery("INSERT INTO tblBonus (ID,memberNo,amount,yearName,staus,payDate,paymentMethod,paidAccount,reference,bankId) " &
                      "VALUES(@id,@member,@amount,@year,@status,@date,@pay,@acc,@ref,@bid)")
            If RecordCount > 0 Then
                AppClass.AddToLedger(CDate(Date.Now).Date, "member rebates", "Dividend Payment", CDec(AmountTextEdit.EditValue), 0, Nothing, 4, 13, ID)
                If PaymentMethodLookUpEdit.EditValue = 1 Then
                    AppClass.AddToLedger(CDate(Date.Now.Date), "Cash Account", "Dividend Payment", 0, CDec(AmountTextEdit.EditValue), Nothing, 2, 13, ID)
                Else
                    AppClass.AddToLedger(CDate(Date.Now.Date), "Bank Account", "Dividend Payment", 0, CDec(AmountTextEdit.EditValue), Nothing, 2, 13, ID)

                End If
                If PaymentMethodLookUpEdit.EditValue > 2 Then
                    AppClass.BankPosting(CDate(Date.Now.Date), BankLookUpEdit.EditValue, 0, CDec(AmountTextEdit.EditValue), "Dividends Payment To " & MemberSearchLookUpEdit.Text _
                                             , ReferenceTextEdit.EditValue.ToLower.ToString, PaymentMethodLookUpEdit.EditValue, False, 13, ID)
                End If

                AddParams("@action", "insert")
                AddParams("@userid", LogedUserID)
                AddParams("@activity", "Dividend Payment For " & MemberSearchLookUpEdit.Text & " For " & DividendYearTextEdit.Text)
                AddParams("@actDate", Date.Now.Date)
                AppClass.ExecSP("spLogs")
                AppClass.ShowNotification("Saved Successfully!")
                Reset()
            End If
        End If
    End Sub
#End Region
#Region "EVENTS"
    Private Sub PaymentTypeComboBoxEdit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles PaymentTypeComboBoxEdit.SelectedIndexChanged
        If PaymentTypeComboBoxEdit.SelectedIndex = 0 Then
            PaymentMethodLookUpEdit.Properties.ReadOnly = True
            BankLookUpEdit.Properties.ReadOnly = True
            AccountMpesaTextEdit.Properties.ReadOnly = True
            PloughOptionsComboBoxEdit.Properties.ReadOnly = False
            ReferenceTextEdit.Properties.ReadOnly = True
        Else
            PaymentMethodLookUpEdit.Properties.ReadOnly = False
            BankLookUpEdit.Properties.ReadOnly = False
            AccountMpesaTextEdit.Properties.ReadOnly = False
            PloughOptionsComboBoxEdit.Properties.ReadOnly = True
            ReferenceTextEdit.Properties.ReadOnly = False
        End If
    End Sub
    Private Sub PaymentMethodLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles PaymentMethodLookUpEdit.EditValueChanged
        If Not PaymentMethodLookUpEdit.EditValue = Nothing Then
            AddParams("@id", PaymentMethodLookUpEdit.EditValue)
            If CBool(AppClass.FetchDBValue("SELECT bankRequired FROM tblPaymentMethod WHERE ID=@id")) = True Then
                BankLookUpEdit.Enabled = True
                BankLookUpEdit.EditValue = 1
            Else
                BankLookUpEdit.Enabled = False
                BankLookUpEdit.EditValue = Nothing
            End If
        End If
    End Sub
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If Not Datavalidation() Then
            Exit Sub
        End If
        Save()
    End Sub
    Private Sub DeleteSimpleButton_Click(sender As Object, e As EventArgs) Handles DeleteSimpleButton.Click

    End Sub
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
    Private Sub PloughOptionsComboBoxEdit_EditValueChanged(sender As Object, e As EventArgs) Handles PloughOptionsComboBoxEdit.EditValueChanged
        If PloughOptionsComboBoxEdit.EditValue > 2 Then
            If CDec(AppClass.GetShareAmount(MemberSearchLookUpEdit.EditValue)) < 20000 Then
                AppClass.ShowNotification("Member Share Threshold Not Reached")

                Exit Sub
            End If
        End If
        If PloughOptionsComboBoxEdit.EditValue = 4 Then
            ProjectLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always
            UnitLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always
            BalanceLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always
            AddParams("@no", MemberSearchLookUpEdit.EditValue)
            AppClass.LoadToLookUpEdit("SELECT DISTINCT P.ID AS ID,UPPER(P.projectName) AS projectName " &
                                      "FROM   tblUnits AS U INNER JOIN tblProjects AS P ON U.projectId = P.ID INNER JOIN tblUnitSale S ON U.ID=S.unitId " &
                                      "WHERE  (U.memberNo = @no) AND (dbo.fnGetPaid(U.ID) < S.netAmount)", ProjectLookUpEdit, "projectName", "ID")
        Else
            ProjectLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
            UnitLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
            BalanceLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
            ProjectLookUpEdit.EditValue = Nothing
            UnitLookUpEdit.EditValue = Nothing
        End If
    End Sub
    Private Sub ProjectLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles ProjectLookUpEdit.EditValueChanged
        If ProjectLookUpEdit.EditValue IsNot Nothing Then
            AddParams("@id", ProjectLookUpEdit.EditValue)
            AddParams("@no", MemberSearchLookUpEdit.EditValue)
            AppClass.LoadToLookUpEdit("SELECT ID,UPPER(unitName) AS unitName FROM tblUnits WHERE (projectId=@id) AND (memberNo=@no)", UnitLookUpEdit, "unitName", "ID")
        End If
    End Sub
    Private Sub UnitLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles UnitLookUpEdit.EditValueChanged
        If UnitLookUpEdit.EditValue IsNot Nothing Then
            BalanceTextEdit.EditValue = GetBalance(MemberSearchLookUpEdit.EditValue, UnitLookUpEdit.EditValue)
        End If
    End Sub
    Private Sub FindSimpleButton_Click(sender As Object, e As EventArgs) Handles FindSimpleButton.Click
        'Dim SearchBy As String = "Search Dividends..."
        'Using frm As New SearchForm(SearchBy, "spBonusSearch")
        '    If frm.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
        '        Find(frm.DataGridView.CurrentRow.Cells(0).Value)
        '    End If
        'End Using
    End Sub
#End Region
End Class
