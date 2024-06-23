Imports System.Data.SqlClient
Imports DevExpress.XtraEditors
Imports System.IO
Imports System.Text
Imports System.Drawing.Printing
Imports System.Drawing.Imaging
Imports Microsoft.Reporting.WinForms
Imports System.Net.Mail
Imports System.Net
Public Class UCMemberCollections
    Private Shared _instance As UCMemberCollections
    Public Shared ReadOnly Property Instance As UCMemberCollections
        Get
            If _instance Is Nothing Then _instance = New UCMemberCollections()
            Return _instance
        End Get
    End Property
    Private IsEdit As Boolean
    Private ID As Integer
    Private MemberNo As Integer
    Private InitialAmountPaid
    Private ReprintBalance As Decimal
    Private IsReprint As Boolean
    Private Shared M_streams As List(Of Stream)
    Private Shared M_currentPageIndex As Integer = 0
#Region "SUBS"
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Reset()
    End Sub
    Public Sub Reset()
        IsEdit = False
        ID = MemberNo = Nothing
        DeleteSimpleButton.Enabled = False
        ReprintSimpleButton.Enabled = False
        ProjectLayoutControlItem.Enabled = False
        UnitLayoutControlItem.Enabled = False
        BankLayoutControlItem.Enabled = False
        LayoutControlItem5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        AppClass.LoadToLookUpEdit("SELECT ID,methodName FROM tblPaymentMethod", PayMethodLookUpEdit, "methodName", "ID")
        AppClass.LoadToLookUpEdit("SELECT ID,purposeName FROM tblPaymentPurpose", PurposeLookUpEdit, "purposeName", "ID")
        AddParams("@true", True)
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(bankName) AS bankName FROM tblBanks WHERE active=@true", BankLookUpEdit, "bankName", "ID")
        IsReprint = False
        AppClass.ClearItems(LayoutControl1)
        'PayMethodLookUpEdit.EditValue = 5
        PostingDateTextEdit.EditValue = Date.Now.ToString("dd-MMM-yy")
        AppClass.GenerateID("SELECT MAX(RIGHT(receiptNo,5)) As receiptNo FROM tblIncomeHeader", "receiptNo", ReceiptNoTextEdit, "00001", "00000")
        TransactionDateEdit.EditValue = Date.Now
        sql = "SELECT M.memberNo,M.memberID AS [MemberID],UPPER(M.memberName) AS [MemberName],M.idNo AS [IDNo],"
        sql &= "A.contact AS [Contact] FROM tblMember M LEFT OUTER JOIN tblMemberAddress A ON M.memberNo=A.memberNo ORDER BY M.memberID"
        AppClass.LoadToSearchLookUpEdit(sql, MemberSearchLookUpEdit, "MemberName", "memberNo")
        TransactionDateEdit.Properties.ReadOnly = False
        PurposeLookUpEdit.Properties.ReadOnly = False
        ReasonTextEdit.Properties.ReadOnly = True
    End Sub
    Function NumberToText(ByVal n As Integer) As String

        Select Case n
            Case 0
                Return ""

            Case 1 To 19
                Dim arr() As String = {"One", "Two", "Three", "Four", "Five", "Six", "Seven",
                  "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen",
                    "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"}
                Return arr(n - 1) & " "

            Case 20 To 99
                Dim arr() As String = {"Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"}
                Return arr(n \ 10 - 2) & " " & NumberToText(n Mod 10)

            Case 100 To 199
                Return "One Hundred " & NumberToText(n Mod 100)

            Case 200 To 999
                Return NumberToText(n \ 100) & "Hundreds " & NumberToText(n Mod 100)

            Case 1000 To 1999
                Return "One Thousand " & NumberToText(n Mod 1000)

            Case 2000 To 999999
                Return NumberToText(n \ 1000) & "Thousands " & NumberToText(n Mod 1000)

            Case 1000000 To 1999999
                Return "One Million " & NumberToText(n Mod 1000000)

            Case 1000000 To 999999999
                Return NumberToText(n \ 1000000) & "Millions " & NumberToText(n Mod 1000000)
            Case 1000000000 To 1999999999
                Return "One Billion " & NumberToText(n Mod 1000000000)

            Case Else
                Return NumberToText(n \ 1000000000) & "Billion " _
                  & NumberToText(n Mod 1000000000)
        End Select
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
    Function Datavalidation() As Boolean
        errmsg = ""
        If String.IsNullOrEmpty(AmountTextEdit.EditValue) Then
            errmsg = "Enter Amount"
            AmountTextEdit.Focus()
        ElseIf String.IsNullOrEmpty(ReferenceTextEdit.EditValue) Then
            errmsg = "Enter Reference!"
            ReferenceTextEdit.Focus()
        ElseIf MemberNo = Nothing Then
            errmsg = "Select Member"
            'cbxMember.Focus()
        ElseIf PurposeLookUpEdit.EditValue = Nothing Then
            errmsg = "Select purpose"
            PurposeLookUpEdit.Focus()
        ElseIf PurposeLookUpEdit.EditValue = 4 AndAlso ProjectLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select Project"
            PurposeLookUpEdit.Focus()
        ElseIf PurposeLookUpEdit.EditValue = 4 AndAlso UnitLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select Unit"
            UnitLookUpEdit.Focus()
        ElseIf PayMethodLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select Payment Method"
            PayMethodLookUpEdit.Focus()
        ElseIf PurposeLookUpEdit.EditValue > 1 AndAlso CDec(AppClass.GetRegistrationFee(MemberNo)) = 0 Then
            errmsg = "Member Hasn't Paid For Their Registration Fee"
        ElseIf PurposeLookUpEdit.EditValue = 1 AndAlso CDec(AppClass.GetRegistrationFee(MemberNo)) = 1000 Then
            errmsg = "Registration Already Paid For This Member"
        ElseIf PurposeLookUpEdit.EditValue > 2 AndAlso CDec(AppClass.GetShareAmount(MemberNo)) < 20000 Then
            errmsg = "Member Hasn't Reached Minimum Share Threshold Required To Make This Posting"
        ElseIf PurposeLookUpEdit.EditValue = 1 AndAlso CDec(AmountTextEdit.EditValue) <> 1000 Then
            errmsg = "Member Registration Fee Is KES 1,000"
            With AmountTextEdit
                .Focus()
                .SelectAll()
            End With
        ElseIf PurposeLookUpEdit.EditValue = 4 AndAlso CDec(AmountTextEdit.Text) > CDec(BalanceTextEdit.Text) Then
            errmsg = "Amount Being Paid Is More Than The Balance"
            With AmountTextEdit
                .Focus()
                .SelectAll()
            End With
        ElseIf Not AppClass.CheckDate(CDate(TransactionDateEdit.EditValue).Date) Then
            errmsg = "Date Cannot Be Greater Than Today"
            TransactionDateEdit.Focus()
        End If
        If errmsg <> "" Then
            AppClass.ShowNotification(errmsg)
            Return False
        Else
            Return True
        End If
    End Function
    Function ValidateEdit() As Boolean
        errmsg = ""
        If String.IsNullOrEmpty(AmountTextEdit.EditValue) Then
            errmsg = "Enter Amount"
            AmountTextEdit.Focus()
        ElseIf String.IsNullOrEmpty(ReferenceTextEdit.EditValue) Then
            errmsg = "Enter Reference!"
            ReferenceTextEdit.Focus()
        ElseIf MemberNo = Nothing Then
            errmsg = "Select Member"
            'cbxMember.Focus()
        ElseIf PurposeLookUpEdit.EditValue = 4 AndAlso ProjectLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select Project"
            ProjectLookUpEdit.Focus()
        ElseIf PurposeLookUpEdit.EditValue = 4 AndAlso UnitLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select Unit"
            UnitLookUpEdit.Focus()
        ElseIf PurposeLookUpEdit.EditValue <> 1 AndAlso CDec(AppClass.GetRegistrationFee(MemberNo)) = 0 Then
            errmsg = "Member Hasn't Paid For Their Registration Fee"
        ElseIf PurposeLookUpEdit.EditValue > 2 AndAlso CDec(AppClass.GetShareAmount(MemberNo)) < 20000 Then
            errmsg = "Member Hasn't Reached Minimum Share Threshold Required To Make This Posting"
        ElseIf PurposeLookUpEdit.EditValue = 4 AndAlso CDec(AmountTextEdit.EditValue) > CDec(BalanceTextEdit.EditValue) Then
            errmsg = "Amount Being Paid Is More Than The Balance"
            With AmountTextEdit
                .Focus()
                .SelectAll()
            End With
        End If

        If errmsg <> "" Then
            AppClass.ShowNotification(errmsg)
            Return False
        Else
            Return True
        End If
    End Function
    Function HasUnitBalance(member As Integer) As Boolean
        If PurposeLookUpEdit.EditValue = 2 AndAlso Not MemberNo = Nothing Then
            AddParams("@memberno", member)
            Dim AmountOwed As Decimal = CDec(AppClass.FetchDBValue("SELECT ISNULL(SUM(dbo.fnGetUnitPaymentBalance(u.ID)),0) AS SumOfUnit from tblUnits u where u.memberNo=@memberno"))
            If AmountOwed > 0 Then
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If
    End Function
    Sub SendMessage()
        Dim Message As String = Nothing
        If CInt(PurposeLookUpEdit.EditValue) < 4 Then
            Message = "Dear " & AppClass.GetFirstName(MemberSearchLookUpEdit.Text.ToString) & ", we have received Ksh." & CDec(AmountTextEdit.EditValue).ToString("N") _
                & " as " & PurposeLookUpEdit.Text.ToString.ToLower & " for your account." & Environment.NewLine
        ElseIf CInt(PurposeLookUpEdit.EditValue) = 4 Then
            Message = "Dear " & AppClass.GetFirstName(MemberSearchLookUpEdit.Text.ToString) & ", we have receieved Ksh. " & CDec(AmountTextEdit.EditValue).ToString("N") _
                    & " as project payment for " & PurposeLookUpEdit.Text & Environment.NewLine & "Closing Balance is Ksh. " & CDec(BalanceTextEdit.EditValue).ToString("N") & Environment.NewLine
        End If
        Dim Receipient As String = AppClass.FormatContactNo(MemberNo)
        AppClass.SendMessage(Receipient, Message)
    End Sub
    Sub Save()
        AddParams("@no", MemberNo)
        Dim hasId = CStr(AppClass.FetchDBValue("SELECT ISNULL(memberID,'NONE') AS memberid FROM tblMember WHERE memberNo=@no"))
        AppClass.GenerateID("SELECT MAX(RIGHT(receiptNo,5)) As receiptNo FROM tblIncomeHeader", "receiptNo", ReceiptNoTextEdit, "00001", "00000")
        AddParams("@ref", Trim(ReferenceTextEdit.EditValue))
        If CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblIncomeDetails WHERE reference=@ref")) > 0 Then
            If AppClass.AlertQuestion("Reference No Already Exists! Do You Wish To Proceed Using It?") = System.Windows.Forms.DialogResult.No Then
                With AmountTextEdit
                    .Focus()
                    .SelectAll()
                End With
                Exit Sub
            End If
        End If

        Using connection As New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Using MyTransaction As SqlTransaction = connection.BeginTransaction
                Try
                    Dim paidAmount, totalPaidAmount As Decimal
                    If PurposeLookUpEdit.EditValue = 4 Then
                        AddParams("@id", UnitLookUpEdit.EditValue)
                        paidAmount = CDec(AppClass.FetchDBValue("SELECT ISNULL(unitPaid,0) AS paidAmount FROM tblUnits WHERE ID=@id"))
                        totalPaidAmount = CDec(AmountTextEdit.EditValue) + paidAmount
                    End If

                    ID = AppClass.GenerateDBID("tblIncomeHeader")

                    sql = "INSERT INTO tblIncomeHeader (ID,receiptNo,postDate,postBy) VALUES(@id,@receipt,@date,@by)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = ID
                            .Parameters.Add(New SqlParameter("@receipt", SqlDbType.VarChar)).Value = ReceiptNoTextEdit.EditValue
                            .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = Date.Now.Date
                            .Parameters.Add(New SqlParameter("@by", SqlDbType.Int)).Value = LogedUserID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "INSERT INTO tblIncomeDetails (ID,receiptDate,memberNo,purposeId,projectId,unitId,amount,paymentMethodId,bankId,reference) "
                    sql &= "VALUES(@id,@date,@no,@pid,@project,@unit,@amount,@pay,@bid,@ref)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = ID
                            .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = CDate(TransactionDateEdit.EditValue).Date
                            .Parameters.Add(New SqlParameter("@no", SqlDbType.Int)).Value = MemberNo
                            .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = PurposeLookUpEdit.EditValue
                            If PurposeLookUpEdit.EditValue = 4 Then
                                .Parameters.Add(New SqlParameter("@project", SqlDbType.Int)).Value = ProjectLookUpEdit.EditValue
                                .Parameters.Add(New SqlParameter("@unit", SqlDbType.Int)).Value = UnitLookUpEdit.EditValue
                            Else
                                .Parameters.Add(New SqlParameter("@project", SqlDbType.Int)).Value = DBNull.Value
                                .Parameters.Add(New SqlParameter("@unit", SqlDbType.Int)).Value = DBNull.Value
                            End If
                            .Parameters.Add(New SqlParameter("@amount", SqlDbType.Decimal)).Value = CDec(AmountTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@pay", SqlDbType.Int)).Value = PayMethodLookUpEdit.EditValue
                            .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = IIf(BankLookUpEdit.EditValue Is Nothing, DBNull.Value, BankLookUpEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = ReferenceTextEdit.EditValue
                            .ExecuteNonQuery()
                        End With
                    End Using

                    If PurposeLookUpEdit.EditValue = 4 Then
                        sql = "UPDATE tblUnits SET unitPaid=@newamount WHERE ID=@id"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@newamount", SqlDbType.Decimal)).Value = totalPaidAmount
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = UnitLookUpEdit.EditValue
                                .ExecuteNonQuery()
                            End With
                        End Using

                        sql = "INSERT INTO tblUnitPayment (unitId,openingBal,paymentDate,amountPaid,closingBal,transactionType,transactionID) VALUES(@uid,@obal,@date,@paid,@cbal,@type,@tid)"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Clear()
                                .Parameters.Add("@uid", SqlDbType.Int).Value = UnitLookUpEdit.EditValue
                                .Parameters.Add("@obal", SqlDbType.Decimal).Value = CDec(BalanceTextEdit.EditValue)
                                .Parameters.Add("@date", SqlDbType.Date).Value = CDate(TransactionDateEdit.EditValue).Date
                                .Parameters.Add("@paid", SqlDbType.Decimal).Value = CDec(AmountTextEdit.EditValue)
                                Dim bal As Decimal = CDec(BalanceTextEdit.EditValue) - CDec(AmountTextEdit.EditValue)
                                .Parameters.Add("@cbal", SqlDbType.Decimal).Value = bal
                                .Parameters.Add("@type", SqlDbType.Int).Value = 2
                                .Parameters.Add("@tid", SqlDbType.Int).Value = ID
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If

                    If PurposeLookUpEdit.EditValue = 1 AndAlso (CDec(AmountTextEdit.EditValue) = 1000) Then
                        If hasId <> "NONE" Then
                            If AppClass.AlertQuestion("Member Already Assigned Member No. Do You Wish To Reassign?") = System.Windows.Forms.DialogResult.Yes Then
                                AppClass.GenerateID("SELECT MAX(RIGHT(memberID,4)) As memberID FROM tblMember", "memberID", MemberIdTextEdit, "0001", "0000", "PHCSL/")
                                sql = "UPDATE tblMember SET memberID=@mid WHERE memberNo=@no"
                                Using cmd = New SqlCommand(sql, connection, MyTransaction)
                                    With cmd
                                        .Parameters.Clear()
                                        .Parameters.Add("@mid", SqlDbType.VarChar).Value = MemberIdTextEdit.EditValue
                                        .Parameters.Add("@no", SqlDbType.Int).Value = MemberNo
                                        .ExecuteNonQuery()
                                    End With
                                End Using
                            End If
                        Else
                            AppClass.GenerateID("SELECT MAX(RIGHT(memberID,4)) As memberID FROM tblMember", "memberID", MemberIdTextEdit, "0001", "0000", "PHCSL/")
                            sql = "UPDATE tblMember SET memberID=@mid WHERE memberNo=@no"
                            Using cmd = New SqlCommand(sql, connection, MyTransaction)
                                With cmd
                                    .Parameters.Clear()
                                    .Parameters.Add("@mid", SqlDbType.VarChar).Value = MemberIdTextEdit.Text
                                    .Parameters.Add("@no", SqlDbType.Int).Value = MemberNo
                                    .ExecuteNonQuery()
                                End With
                            End Using
                        End If
                    End If

                    sql = "INSERT INTO tblLogs VALUES(@uid,@act,@actdate)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@uid", SqlDbType.Int).Value = LogedUserID
                            .Parameters.Add("@act", SqlDbType.NVarChar).Value = "Posted Collections For " & PurposeLookUpEdit.Text & " For Member " & MemberSearchLookUpEdit.Text
                            .Parameters.Add("@actdate", SqlDbType.Date).Value = Date.Now.Date
                            .ExecuteNonQuery()
                        End With
                    End Using

                    MyTransaction.Commit()

                    If PayMethodLookUpEdit.EditValue = 1 AndAlso PurposeLookUpEdit.EditValue = 1 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "cash ccount", "Registration Fee", CDec(AmountTextEdit.EditValue), 0, Nothing, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "registration fee", "Registration Fee", 0, CDec(AmountTextEdit.EditValue), Nothing, 5, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue = 1 AndAlso PurposeLookUpEdit.EditValue = 2 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "cash account", "Shares Payment", CDec(AmountTextEdit.EditValue), 0, Nothing, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "shares", "Shares Payment", 0, CDec(AmountTextEdit.EditValue), Nothing, 4, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue = 1 AndAlso PurposeLookUpEdit.EditValue = 3 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "cash account", "Member Deposits", CDec(AmountTextEdit.EditValue), 0, Nothing, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "deposits", "Member Deposits", 0, CDec(AmountTextEdit.EditValue), Nothing, 3, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue = 1 AndAlso PurposeLookUpEdit.EditValue = 4 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "cash account", "Project Payment", CDec(AmountTextEdit.EditValue), 0, ProjectLookUpEdit.EditValue, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "unit payment", "Project Payment", 0, CDec(AmountTextEdit.EditValue), ProjectLookUpEdit.EditValue, 5, 2, ID)

                    ElseIf PayMethodLookUpEdit.EditValue <> 1 AndAlso PurposeLookUpEdit.EditValue = 1 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "bank account", "Registration Fee", CDec(AmountTextEdit.EditValue), 0, Nothing, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "registration fee", "Registration Fee", 0, CDec(AmountTextEdit.EditValue), Nothing, 5, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue <> 1 AndAlso PurposeLookUpEdit.EditValue = 2 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "bank account", "Shares Payment", CDec(AmountTextEdit.EditValue), 0, Nothing, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "shares", "Shares Payment", 0, CDec(AmountTextEdit.EditValue), Nothing, 4, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue <> 1 AndAlso PurposeLookUpEdit.EditValue = 3 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "bank account", "Member Deposits", CDec(AmountTextEdit.EditValue), 0, Nothing, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "deposits", "Member Deposits", 0, CDec(AmountTextEdit.EditValue), Nothing, 3, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue <> 1 AndAlso PurposeLookUpEdit.EditValue = 4 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "bank account", "Project Payment", CDec(AmountTextEdit.EditValue), 0, ProjectLookUpEdit.EditValue, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "unit payment", "Project Payment", 0, CDec(AmountTextEdit.EditValue), ProjectLookUpEdit.EditValue, 5, 2, ID)
                    End If

                    If PayMethodLookUpEdit.EditValue = 3 OrElse PayMethodLookUpEdit.EditValue = 4 AndAlso PurposeLookUpEdit.EditValue = 0 Then
                        AppClass.BankPosting(CDate(TransactionDateEdit.EditValue).Date, BankLookUpEdit.EditValue, CDec(AmountTextEdit.EditValue), 0, "Registration Fees-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                             , PayMethodLookUpEdit.EditValue, False, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue = 3 OrElse PayMethodLookUpEdit.EditValue = 4 AndAlso PurposeLookUpEdit.EditValue = 1 Then
                        AppClass.BankPosting(CDate(TransactionDateEdit.EditValue).Date, BankLookUpEdit.EditValue, CDec(AmountTextEdit.EditValue), 0, "Shares Payment-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                            , PayMethodLookUpEdit.EditValue, False, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue = 3 OrElse PayMethodLookUpEdit.EditValue = 4 AndAlso PurposeLookUpEdit.EditValue = 2 Then
                        AppClass.BankPosting(CDate(TransactionDateEdit.EditValue).Date, BankLookUpEdit.EditValue, CDec(AmountTextEdit.EditValue), 0, "Member Deposits-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                            , PayMethodLookUpEdit.EditValue, False, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue = 3 OrElse PayMethodLookUpEdit.EditValue = 4 AndAlso PurposeLookUpEdit.EditValue = 3 Then
                        AppClass.BankPosting(CDate(TransactionDateEdit.EditValue).Date, BankLookUpEdit.EditValue, CDec(AmountTextEdit.EditValue), 0, "Project Payment-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                            , PayMethodLookUpEdit.EditValue, False, 2, ID)
                    End If

                    If MemberIdTextEdit.EditValue IsNot Nothing AndAlso PurposeLookUpEdit.EditValue = 0 Then
                        AppClass.ShowNotification("Member Assigned Member No: " & MemberIdTextEdit.EditValue.ToString)
                    End If

                    AppClass.ShowNotification("Saved Successfully!")
                    If AppClass.AlertQuestion("Do You Wish To Print A Receipt For This Transaction?") = System.Windows.Forms.DialogResult.Yes Then
                        If AppClass.AlertQuestion("Do You Wish To Download The Receipt To Your PC?") = System.Windows.Forms.DialogResult.Yes Then
                            reportStuff(True)
                        Else
                            reportStuff()
                        End If
                    Else
                        If AppClass.AlertQuestion("Do You Wish To Download The Receipt To Your PC?") = System.Windows.Forms.DialogResult.Yes Then
                            reportStuff(True, False)
                        End If
                    End If

                    If AppClass.AlertQuestion("Do You Wish notify client on this payment by SMS?") = System.Windows.Forms.DialogResult.Yes Then
                        SendMessage()
                    End If

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
        ID = Nothing
        MemberNo = Nothing
        IsEdit = True
        AppClass.ClearItems(LayoutControl1)
        'Dim unused As New DataTable
        AddParams("@id", fid)
        Dim Searchdt As DataTable = AppClass.LoadToDatatable("spFindCollection", True)

        ID = CInt(Searchdt.Rows(0)(0))
        IDTextEdit.EditValue = CInt(Searchdt.Rows(0)(0))
        ReceiptNoTextEdit.EditValue = Searchdt.Rows(0)(1)
        PostingDateTextEdit.EditValue = CDate(Searchdt.Rows(0)(2)).ToString("dd-MMM-yy")
        TransactionDateEdit.EditValue = CDate(Searchdt.Rows(0)(3))
        MemberNo = CInt(Searchdt.Rows(0)(4))
        MemberSearchLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(4))
        PurposeLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(5))
        If PurposeLookUpEdit.EditValue = 4 Then
            AddParams("@no", MemberNo)
            AppClass.LoadToLookUpEdit("SELECT DISTINCT P.ID AS ID,UPPER(P.projectName) AS projectName " &
                                            "FROM   tblUnits AS U INNER JOIN tblProjects AS P ON U.projectId = P.ID INNER JOIN tblUnitSale S ON U.ID=S.unitId " &
                                            "WHERE  (U.memberNo = @no)", ProjectLookUpEdit, "projectName", "ID")
        End If
        If Not IsDBNull(Searchdt.Rows(0)(6)) Then
            ProjectLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(6))
        End If
        If Not IsDBNull(Searchdt.Rows(0)(7)) Then
            UnitLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(7))
        End If
        InitialAmountPaid = CDec(Searchdt.Rows(0)(8))
        AmountTextEdit.EditValue = CDec(Searchdt.Rows(0)(8)).ToString("N")
        PayMethodLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(9))
        If Not IsDBNull(Searchdt.Rows(0)(10)) Then
            BankLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(10))
        End If
        If Not IsDBNull(Searchdt.Rows(0)(11)) Then
            ReferenceTextEdit.EditValue = CStr(Searchdt.Rows(0)(11)).ToUpper.ToString
        End If
        'txtMemberName.Text = CStr(dt.Rows(0)(12))
        ReferenceTextEdit.Properties.ReadOnly = False
        ProjectLayoutControlItem.Enabled = False
        TransactionDateEdit.Properties.ReadOnly = True
        UnitLayoutControlItem.Enabled = False
        PurposeLookUpEdit.Properties.ReadOnly = True
        DeleteSimpleButton.Enabled = True
        AddParams("@id", ID)
        ReasonTextEdit.EditValue = CStr(AppClass.FetchDBValue("SELECT COALESCE(UPPER(editReason),'[None]') FROM tblIncomeHeader WHERE (ID=@id)"))
        If ReasonTextEdit.EditValue = "[None]" Then
            ReasonTextEdit.EditValue = Nothing
        End If
        ReasonTextEdit.Properties.ReadOnly = False
        ReprintSimpleButton.Enabled = True
        If PurposeLookUpEdit.EditValue = 4 Then
            AddParams("@type", 2)
            AddParams("@tid", ID)
            ReprintBalance = CDec(AppClass.FetchDBValue("SELECT ISNULL(closingBal,0) AS Bal FROM tblUnitPayment WHERE (transactionType=@type) AND (transactionID=@tid)"))
        End If
    End Sub
    Sub Edit()
        Dim paidAmount, lessThis, totalPaidAmount As Decimal
        If PurposeLookUpEdit.EditValue = 4 Then
            AddParams("@id", UnitLookUpEdit.EditValue)
            paidAmount = CDec(AppClass.FetchDBValue("SELECT ISNULL(SUM(amountPaid),0) AS paidAmount FROM tblUnitPayment WHERE unitId=@id"))
            lessThis = paidAmount - InitialAmountPaid
            totalPaidAmount = CDec(AmountTextEdit.EditValue) + lessThis
        End If

        Using connection = New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If  '0759253481
            End With
            Using MyTransaction As SqlTransaction = connection.BeginTransaction
                Try
                    sql = "UPDATE tblIncomeDetails SET receiptDate=@date,amount=@amount,paymentMethodId=@pay,bankId=@bid "
                    sql &= "WHERE (ID=@id)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = CDate(TransactionDateEdit.EditValue).Date
                            .Parameters.Add(New SqlParameter("@amount", SqlDbType.Decimal)).Value = CDec(AmountTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@pay", SqlDbType.Int)).Value = PayMethodLookUpEdit.EditValue
                            .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = IIf(BankLookUpEdit.EditValue Is Nothing, DBNull.Value, BankLookUpEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = CInt(IDTextEdit.EditValue)
                            .ExecuteNonQuery()
                        End With
                    End Using

                    If PurposeLookUpEdit.EditValue = 4 Then
                        sql = "UPDATE tblUnits SET unitPaid=@newamount WHERE ID=@id"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@newamount", SqlDbType.Decimal)).Value = totalPaidAmount
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = UnitLookUpEdit.EditValue
                                .ExecuteNonQuery()
                            End With
                        End Using
                        Dim openingb As Decimal
                        AddParams("@type", 2)
                        AddParams("@tid", ID)
                        openingb = CDec(AppClass.FetchDBValue("SELECT openingBal FROM tblUnitPayment WHERE (transactionType=@type) AND (transactionID=@tid)"))
                        Dim bal As Decimal = openingb - CDec(AmountTextEdit.EditValue)
                        sql = "UPDATE tblUnitPayment SET amountPaid=@paid,closingBal=@bal WHERE (transactionType=@type) AND (transactionID=@tid)"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add("@paid", SqlDbType.Decimal).Value = CDec(AmountTextEdit.EditValue)
                                .Parameters.Add("@bal", SqlDbType.Decimal).Value = bal
                                .Parameters.Add("@type", SqlDbType.Int).Value = 2
                                .Parameters.Add("@tid", SqlDbType.Int).Value = CInt(IDTextEdit.EditValue)
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If

                    sql = "DELETE FROM tblLedger WHERE (transactionType=@type) AND (transactionID=@tid)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        cmd.Parameters.Add("@type", SqlDbType.Int).Value = 2
                        cmd.Parameters.Add("@tid", SqlDbType.Int).Value = CInt(IDTextEdit.EditValue)
                        cmd.ExecuteNonQuery()
                    End Using
                    sql = "DELETE FROM tblBankPostings WHERE (transactionType=@type) AND (transactionID=@tid)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        cmd.Parameters.Add("@type", SqlDbType.Int).Value = 2
                        cmd.Parameters.Add("@tid", SqlDbType.Int).Value = CInt(IDTextEdit.EditValue)
                        cmd.ExecuteNonQuery()
                    End Using
                    sql = "INSERT INTO tblLogs VALUES(@uid,@act,@actdate)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@uid", SqlDbType.Int).Value = LogedUserID
                            .Parameters.Add("@act", SqlDbType.NVarChar).Value = "Edited Collections For " & PurposeLookUpEdit.Text & " For Member " & MemberSearchLookUpEdit.Text
                            .Parameters.Add("@actdate", SqlDbType.Date).Value = Date.Now.Date
                            .ExecuteNonQuery()
                        End With
                    End Using

                    MyTransaction.Commit()

                    If PayMethodLookUpEdit.EditValue = 1 AndAlso PurposeLookUpEdit.EditValue = 1 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "cash ccount", "Registration Fee", CDec(AmountTextEdit.EditValue), 0, Nothing, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "registration fee", "Registration Fee", 0, CDec(AmountTextEdit.EditValue), Nothing, 5, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue = 1 AndAlso PurposeLookUpEdit.EditValue = 2 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "cash account", "Shares Payment", CDec(AmountTextEdit.EditValue), 0, Nothing, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "shares", "Shares Payment", 0, CDec(AmountTextEdit.EditValue), Nothing, 4, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue = 1 AndAlso PurposeLookUpEdit.EditValue = 3 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "cash account", "Member Deposits", CDec(AmountTextEdit.EditValue), 0, Nothing, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "deposits", "Member Deposits", 0, CDec(AmountTextEdit.EditValue), Nothing, 3, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue = 1 AndAlso PurposeLookUpEdit.EditValue = 4 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "cash account", "Project Payment", CDec(AmountTextEdit.EditValue), 0, ProjectLookUpEdit.EditValue, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "unit payment", "Project Payment", 0, CDec(AmountTextEdit.EditValue), ProjectLookUpEdit.EditValue, 5, 2, ID)

                    ElseIf PayMethodLookUpEdit.EditValue <> 1 AndAlso PurposeLookUpEdit.EditValue = 1 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "bank account", "Registration Fee", CDec(AmountTextEdit.EditValue), 0, Nothing, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "registration fee", "Registration Fee", 0, CDec(AmountTextEdit.EditValue), Nothing, 5, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue <> 1 AndAlso PurposeLookUpEdit.EditValue = 2 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "bank account", "Shares Payment", CDec(AmountTextEdit.EditValue), 0, Nothing, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "shares", "Shares Payment", 0, CDec(AmountTextEdit.EditValue), Nothing, 4, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue <> 1 AndAlso PurposeLookUpEdit.EditValue = 3 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "bank account", "Member Deposits", CDec(AmountTextEdit.EditValue), 0, Nothing, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "deposits", "Member Deposits", 0, CDec(AmountTextEdit.EditValue), Nothing, 3, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue <> 1 AndAlso PurposeLookUpEdit.EditValue = 4 Then
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "bank account", "Project Payment", CDec(AmountTextEdit.EditValue), 0, ProjectLookUpEdit.EditValue, 2, 2, ID)
                        AppClass.AddToLedger(CDate(TransactionDateEdit.EditValue).Date, "unit payment", "Project Payment", 0, CDec(AmountTextEdit.EditValue), ProjectLookUpEdit.EditValue, 5, 2, ID)
                    End If

                    If PayMethodLookUpEdit.EditValue = 3 OrElse PayMethodLookUpEdit.EditValue = 4 AndAlso PurposeLookUpEdit.EditValue = 0 Then
                        AppClass.BankPosting(CDate(TransactionDateEdit.EditValue).Date, BankLookUpEdit.EditValue, CDec(AmountTextEdit.EditValue), 0, "Registration Fees-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                             , PayMethodLookUpEdit.EditValue, False, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue = 3 OrElse PayMethodLookUpEdit.EditValue = 4 AndAlso PurposeLookUpEdit.EditValue = 1 Then
                        AppClass.BankPosting(CDate(TransactionDateEdit.EditValue).Date, BankLookUpEdit.EditValue, CDec(AmountTextEdit.EditValue), 0, "Shares Payment-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                            , PayMethodLookUpEdit.EditValue, False, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue = 3 OrElse PayMethodLookUpEdit.EditValue = 4 AndAlso PurposeLookUpEdit.EditValue = 2 Then
                        AppClass.BankPosting(CDate(TransactionDateEdit.EditValue).Date, BankLookUpEdit.EditValue, CDec(AmountTextEdit.EditValue), 0, "Member Deposits-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                            , PayMethodLookUpEdit.EditValue, False, 2, ID)
                    ElseIf PayMethodLookUpEdit.EditValue = 3 OrElse PayMethodLookUpEdit.EditValue = 4 AndAlso PurposeLookUpEdit.EditValue = 3 Then
                        AppClass.BankPosting(CDate(TransactionDateEdit.EditValue).Date, BankLookUpEdit.EditValue, CDec(AmountTextEdit.EditValue), 0, "Project Payment-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                            , PayMethodLookUpEdit.EditValue, False, 2, ID)
                    End If

                    AppClass.ShowNotification("Edited Successfully!")
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

        Using connection = New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With
            Using MyTransaction As SqlTransaction = connection.BeginTransaction
                Try

                    sql = "DELETE FROM tblIncomeDetails WHERE (ID=@id)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        cmd.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = CInt(IDTextEdit.EditValue)
                        cmd.ExecuteNonQuery()
                    End Using

                    sql = "DELETE FROM tblIncomeHeader WHERE (ID=@id)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        cmd.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = CInt(IDTextEdit.EditValue)
                        cmd.ExecuteNonQuery()
                    End Using
                    sql = "DELETE FROM tblUnitPayment WHERE (transactionType=@type) AND (transactionID=@id)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 2
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = CInt(IDTextEdit.EditValue)
                            .ExecuteNonQuery()
                        End With
                    End Using

                    If PurposeLookUpEdit.EditValue = 4 Then
                        Dim paidAmount, lessThis As Decimal
                        AddParams("@id", UnitLookUpEdit.EditValue)
                        paidAmount = CDec(AppClass.FetchDBValue("SELECT ISNULL(unitPaid,0) AS paidAmount FROM tblUnits WHERE ID=@id"))
                        lessThis = paidAmount - InitialAmountPaid
                        '  totalPaidAmount = CDec(txtAmount.Text) + lessThis

                        sql = "UPDATE tblUnits SET unitPaid=@newamount WHERE ID=@id"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@newamount", SqlDbType.Decimal)).Value = lessThis
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = UnitLookUpEdit.EditValue
                                .ExecuteNonQuery()
                            End With
                        End Using
                        sql = "DELETE FROM tblUnitPayment WHERE (transactionType=@type) AND (transactionID=@id)"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 2
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = CInt(IDTextEdit.EditValue)
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If

                    sql = "DELETE FROM tblLedger WHERE (transactionType=@type) AND (transactionID=@id)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 2
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = CInt(IDTextEdit.EditValue)
                            .ExecuteNonQuery()
                        End With
                    End Using
                    sql = "DELETE FROM tblBankPostings WHERE (transactionType=@type) AND (transactionID=@id)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 2
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = CInt(IDTextEdit.EditValue)
                            .ExecuteNonQuery()
                        End With
                    End Using
                    MyTransaction.Commit()

                    AppClass.ShowNotification("Deleted Successfully!")
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
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        Select Case XtraTabControl1.SelectedTabPageIndex
            Case 0
                If Not IsEdit Then
                    If Not Datavalidation() Then
                        Exit Sub
                    End If

                    If HasUnitBalance(MemberNo) Then
                        AppClass.ShowNotification("Member has balance from units sold to them")
                        Exit Sub
                    End If

                    If PurposeLookUpEdit.EditValue = 1 Then
                        AddParams("@id", 1)
                        AddParams("@no", MemberNo)
                        If CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblIncomeDetails WHERE (purposeId=@id) AND (memberNo=@no)")) > 0 Then
                            AppClass.ShowNotification("Member Has Already Paid For Their Registration Fee!")
                            Exit Sub
                        End If
                    End If
                    Save()
                Else
                    If Not ValidateEdit() Then
                        Exit Sub
                    End If
                    If String.IsNullOrEmpty(ReasonTextEdit.EditValue) Then
                        AppClass.ShowNotification("Enter Reason For Editing This Transaction")
                        ReasonTextEdit.Focus()
                        Exit Sub
                    End If
                    If PurposeLookUpEdit.EditValue = 4 Then
                        AddParams("@date", CDate(TransactionDateEdit.EditValue).Date)
                        AddParams("@uid", UnitLookUpEdit.EditValue)
                        If CInt(AppClass.FetchDBValue("SELECT COUNT(unitId) FROM tblUnitPayment WHERE (paymentDate > @date) AND (unitId = @uid)")) > 0 Then
                            AppClass.ShowNotification("You Cannot Edit This Transaction As Other Payments Have Been Made After This Date")
                            Exit Sub
                        End If
                    End If
                    Edit()
                End If
        End Select
    End Sub
    Private Sub PurposeLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles PurposeLookUpEdit.EditValueChanged
        If PurposeLookUpEdit.EditValue <> 1 Then
            Dim regPaid As Decimal
            AddParams("@id", 1)
            AddParams("@no", MemberNo)
            regPaid = AppClass.FetchDBValue("SELECT ISNULL(SUM(amount),0) AS amount FROM tblIncomeDetails WHERE (purposeId=@id) AND (memberNo=@no)")
            If regPaid < 1000 Then
                AppClass.ShowNotification("Member Has Not Yet Completed Registration Fee Payment!")
                PurposeLookUpEdit.EditValue = 1
                ' cbxMember.Focus()
                Exit Sub
            End If
        End If
        If PurposeLookUpEdit.EditValue > 2 Then
            If CDec(AppClass.GetShareAmount(MemberNo)) < 20000 Then
                AppClass.ShowNotification("Member Share Threshold Not Reached")
                PurposeLookUpEdit.EditValue = 1
                Exit Sub
            End If
        End If
        'If PurposeLookUpEdit.EditValue = 2 Then
        '    If MemberNo <> Nothing Then
        '        If HasUnitBalance(MemberNo) Then
        '            AppClass.ShowNotification("Member has balance from units sold to them")
        '        End If
        '    End If
        'End If
        If PurposeLookUpEdit.EditValue = 4 Then
            ProjectLayoutControlItem.Enabled = True
            UnitLayoutControlItem.Enabled = True
            ' txtBalance.Visible = True
            'lblBalances.Visible = True
            LayoutControlItem5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always
            AddParams("@no", MemberNo)
            AppClass.LoadToLookUpEdit("SELECT DISTINCT P.ID AS ID,UPPER(P.projectName) AS projectName " &
                                      "FROM   tblUnits AS U INNER JOIN tblProjects AS P ON U.projectId = P.ID INNER JOIN tblUnitSale S ON U.ID=S.unitId " &
                                      "WHERE  (U.memberNo = @no) AND (dbo.fnGetPaid(U.ID) < S.netAmount)", ProjectLookUpEdit, "projectName", "ID")
        Else
            ProjectLayoutControlItem.Enabled = False
            UnitLayoutControlItem.Enabled = False
            LayoutControlItem5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        End If
    End Sub
    Private Sub ProjectLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles ProjectLookUpEdit.EditValueChanged
        If ProjectLookUpEdit.EditValue IsNot Nothing Then
            AddParams("@id", ProjectLookUpEdit.EditValue)
            AddParams("@no", MemberNo)
            AppClass.LoadToLookUpEdit("SELECT ID,UPPER(unitName) AS unitName FROM tblUnits WHERE (projectId=@id) AND (memberNo=@no)", UnitLookUpEdit, "unitName", "ID")
        End If
    End Sub
    Private Sub PayMethodLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles PayMethodLookUpEdit.EditValueChanged
        If PayMethodLookUpEdit.EditValue IsNot Nothing Then
            AddParams("@id", PayMethodLookUpEdit.EditValue)
            If CBool(AppClass.FetchDBValue("SELECT bankRequired FROM tblPaymentMethod WHERE ID=@id")) = True Then
                BankLayoutControlItem.Enabled = True
            Else
                BankLayoutControlItem.Enabled = False
            End If
            BankLookUpEdit.EditValue = Nothing
        End If
    End Sub
    Private Sub UnitLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles UnitLookUpEdit.EditValueChanged
        If UnitLookUpEdit.EditValue IsNot Nothing Then
            BalanceTextEdit.EditValue = GetBalance(MemberNo, UnitLookUpEdit.EditValue)
        End If
    End Sub
    Private Sub MemberSearchLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles MemberSearchLookUpEdit.EditValueChanged
        If MemberSearchLookUpEdit.EditValue IsNot Nothing AndAlso MemberSearchLookUpEdit.EditValue IsNot "" Then
            MemberNo = MemberSearchLookUpEdit.EditValue
            AddParams("@no", MemberNo)
            AddParams("@false", False)
            If CInt(AppClass.FetchDBValue("SELECT COUNT(ID) FROM tblDefaultPayments WHERE (memberNo=@no) AND (deactivated=@false)")) > 0 Then
                AddParams("@no", MemberNo)
                PurposeLookUpEdit.EditValue = CInt(AppClass.FetchDBValue("SELECT defaultPayment FROM tblDefaultPayments WHERE (memberNo=@no)"))
            End If
            If PurposeLookUpEdit.EditValue = 2 AndAlso Not IsEdit Then
                If HasUnitBalance(MemberNo) Then
                    AppClass.ShowNotification("Member has balance from units sold to them")
                End If
            End If
        End If
    End Sub
    Private Sub ReprintSimpleButton_Click(sender As Object, e As EventArgs) Handles ReprintSimpleButton.Click
        If XtraTabControl1.SelectedTabPageIndex = 0 Then
            IsReprint = True
            If AppClass.AlertQuestion("Do You Wish To Download The Receipt To Your PC?") = System.Windows.Forms.DialogResult.Yes Then
                ReportStuff(True)
            Else
                ReportStuff()
            End If
            Reset()
        Else
            ' reportStuffBulk()
            ' bulkReset()
        End If
    End Sub
    Private Sub FindSimpleButton_Click(sender As Object, e As EventArgs) Handles FindSimpleButton.Click
        Dim SearchBy As String = "Search by member or purpose..."
        Using frm As New SearchForm(SearchBy, "spSearchCollection")
            If frm.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                Find(frm.DataGridView.CurrentRow.Cells(0).Value)
            End If
        End Using
    End Sub
    Private Sub DeleteSimpleButton_Click(sender As Object, e As EventArgs) Handles DeleteSimpleButton.Click
        If AppClass.AlertQuestion("Are you sure you want to delete this transaction?") = DialogResult.Yes Then
            Delete()
        End If
    End Sub
#End Region
#Region "PRINT"
    Public Shared Sub PrintToPrinter(ByVal report As LocalReport)
        Export(report)
    End Sub
    Public Shared Function CreateStream(ByVal name As String, ByVal fileNameExtension As String, ByVal encoding As Encoding, ByVal mimeType As String, ByVal willSeek As Boolean) As Stream
        Dim stream As Stream = New MemoryStream()
        M_streams.Add(stream)
        Return stream
    End Function
    Public Shared Sub Export(ByVal report As LocalReport)
        Dim deviceInfo As String = "<DeviceInfo>" &
               "<OutputFormat>EMF</OutputFormat>" &
               "<PageWidth>8.27in</PageWidth> " &
               "<PageHeight>11.69in</PageHeight>" &
               "<MarginTop>0in</MarginTop>" &
                "<MarginLeft>0in</MarginLeft>" &
                "<MarginRight>0in</MarginRight>" &
                "<MarginBottom>0in</MarginBottom>" &
            "</DeviceInfo>"
        Dim warnings As Warning() = Nothing
        M_streams = New List(Of Stream)()
        report.Render("Image", deviceInfo, AddressOf CreateStream, warnings)

        For Each stream As Stream In M_streams
            stream.Position = 0
        Next

        'If print Then
        '    print()
        'End If
    End Sub
    Private Sub Print()
        If M_streams Is Nothing OrElse M_streams.Count = 0 Then
            Throw New Exception("Error: no stream to print.")
        End If
        Dim printDoc As New PrintDocument()
        If Not printDoc.PrinterSettings.IsValid Then
            Throw New Exception("Error: cannot find the default printer.")
        Else
            AddHandler printDoc.PrintPage, AddressOf PrintPage
            M_currentPageIndex = 0
            'printDoc.PrinterSettings.PrinterName = "POS-80C"
            printDoc.Print()
        End If
    End Sub
    ' Handler for PrintPageEvents
    Private Sub PrintPage(ByVal sender As Object, ByVal ev As PrintPageEventArgs)
        Dim pageImage As New Metafile(M_streams(M_currentPageIndex))
        ' Adjust rectangular area with printer margins.
        'Dim adjustedRect As New Rectangle(ev.PageBounds.Left - CInt(ev.PageSettings.HardMarginX), _
        '                                  ev.PageBounds.Top - CInt(ev.PageSettings.HardMarginY), _
        '                                  ev.PageBounds.Width, _
        '                                  ev.PageBounds.Height)
        'Dim adjustedrect As New Rectangle(100, 100, 300, 600)
        Dim adjustedrect As New Rectangle(0, 0, 827, 1169)
        ' Draw a white background for the report
        ev.Graphics.FillRectangle(Brushes.White, adjustedrect)

        ' Draw the report content
        ev.Graphics.DrawImage(pageImage, adjustedrect)

        ' Prepare for the next page. Make sure we haven't hit the end.
        'm_currentPageIndex += 1
        'ev.HasMorePages = (m_currentPageIndex < m_streams.Count)
    End Sub
    Public Shared Sub DisposePrint()
        If M_streams IsNot Nothing Then

            For Each stream As Stream In M_streams
                stream.Close()
            Next

            M_streams = Nothing
        End If
    End Sub
    Sub ReportStuff(Optional ToDownLoad As Boolean = False, Optional ToPrint As Boolean = True)
        AddParams("@id", LogedUserID)
        Dim postby As String = CStr(AppClass.FetchDBValue("SELECT UPPER(userName) AS userName FROM tblUsers WHERE (ID=@id)")).ToUpper.ToString
        Dim receivedFrom As String = Nothing
        Dim idNo As String = Nothing
        Dim phoneNo As String = Nothing
        Dim memberId As String = Nothing
        Dim showBalance As Boolean = False

        receivedFrom = CStr(MemberSearchLookUpEdit.EditValue.ToString)
        AddParams("@no", MemberNo)
        idNo = CStr(AppClass.FetchDBValue("SELECT ISNULL(idNo,'N/A') AS isNo FROM tblMember WHERE (memberNo=@no)"))
        AddParams("@no", MemberNo)
        phoneNo = CStr(AppClass.FetchDBValue("SELECT ISNULL(contact,'N/A') AS contact FROM tblMemberAddress WHERE (memberNo=@no)"))
        AddParams("@no", MemberNo)
        memberId = CStr(AppClass.FetchDBValue("SELECT ISNULL(memberID,'N/A') AS memberID FROM tblMember WHERE (memberNo=@no)"))

        Dim balance As Decimal = 0
        If PurposeLookUpEdit.EditValue = 4 Then
            balance = CDec(BalanceTextEdit.EditValue) - CDec(AmountTextEdit.EditValue)
            showBalance = True
        End If

        Dim Report As New LocalReport
        Dim myparam As ReportParameter
        Dim myParams As New List(Of ReportParameter)
        myparam = New ReportParameter("prmReceivedFrom", CStr(receivedFrom))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmDate", CDate(TransactionDateEdit.EditValue).ToString("dd-MM-yy"))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmNo", CInt(ReceiptNoTextEdit.Text))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmIdNo", CStr(idNo.ToUpper.ToString))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmPhoneNo", CStr(phoneNo).ToUpper.ToString)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmPaymentMethod", CStr(PayMethodLookUpEdit.Text).ToUpper.ToString)
        myParams.Add(myparam)
        If IsReprint = False Then
            myparam = New ReportParameter("prmBalance", CDec(balance))
            myParams.Add(myparam)
        Else
            myparam = New ReportParameter("prmBalance", CDec(ReprintBalance))
            myParams.Add(myparam)
        End If
        myparam = New ReportParameter("prmPrintedBy", CStr(postby).ToUpper.ToString)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmAmountInWords", CStr(NumberToText(CInt(AmountTextEdit.EditValue)).ToUpper.ToString & " ONLY"))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmReference", CStr(ReferenceTextEdit.EditValue).ToUpper.ToString)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmMemberNo", CStr(memberId).ToUpper.ToString)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmShowBalance", showBalance)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmShowStamp", ToDownLoad)
        myParams.Add(myparam)

        Report.ReportEmbeddedResource = "phcsl.Receipt.rdlc"
        Report.SetParameters(myParams)

        ds = New DSApp
        dt = New DSApp.unitsaleDataTable
        If PurposeLookUpEdit.EditValue = 1 Then
            dt.Rows.Add("REGISTRATION FEES", CDec(AmountTextEdit.EditValue))
        ElseIf PurposeLookUpEdit.EditValue = 2 Then
            dt.Rows.Add("SHARES", CDec(AmountTextEdit.EditValue))
        ElseIf PurposeLookUpEdit.EditValue = 3 Then
            dt.Rows.Add("DEPOSITS", CDec(AmountTextEdit.EditValue))
        ElseIf PurposeLookUpEdit.EditValue = 4 Then
            dt.Rows.Add("Payment For Project " & ProjectLookUpEdit.Text & " Plot " & UnitLookUpEdit.Text, CDec(AmountTextEdit.EditValue))
        End If

        ds.Tables.Add(dt)
        dt.Dispose()
        ds.Dispose()
        Dim reportsource As New ReportDataSource("DataSet1", CType(dt, DataTable))

        Report.DataSources.Clear()
        Report.DataSources.Add(reportsource)
        Export(Report)
        If ToPrint Then
            Print()
        End If

        Dim FileName As String = Nothing
        ' If AppClass.AlertQuestion("Download This Receipt To Your PC?") = System.Windows.Forms.DialogResult.Yes Then
        If ToDownLoad Then
            Dim SFD As New XtraFolderBrowserDialog
            With SFD
                If .ShowDialog = DialogResult.OK Then
                    FileName = .SelectedPath + "\" + ReceiptNoTextEdit.EditValue & ".pdf"
                End If
            End With
            SaveToPdf(Report, FileName)
        End If
    End Sub
    Function CheckEmail() As Boolean
        Try
            Using connection = New SqlConnection(connstr)
                With connection
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With
                sql = "select email FROM tblMemberAddress WHERE memberNo=@no"
                Using cmd = New SqlCommand(sql, connection)
                    cmd.Parameters.Add(New SqlParameter("@no", SqlDbType.Int)).Value = MemberNo
                    Using rd = cmd.ExecuteReader
                        rd.Read()
                        If Not IsDBNull(rd.Item(0)) Then
                            Return True
                        Else
                            Return False
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            AppClass.ShowError(ex.Message)
            Return False
        End Try
    End Function
    Public Sub SaveToPdf(ByVal viewer As LocalReport, ByVal savePath As String)
        Dim Bytes() As Byte = viewer.Render("PDF", "", Nothing, Nothing, Nothing, Nothing, Nothing)

        Using Stream As New FileStream(savePath, FileMode.Create)
            Stream.Write(Bytes, 0, Bytes.Length)
        End Using
    End Sub
    Sub SendMail()
        If Not CheckForInternetConnection() Then
            XtraMessageBox.Show("Internet Is Required To Send The Email", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If
        Dim email, passwd As String
        Dim decryptedPwd As String
        If CInt(AppClass.FetchDBValue("SELECT COUNT(ID) FROM tblEmailSetting")) > 0 Then
            email = CStr(AppClass.FetchDBValue("SELECT email FROM tblEmailSetting")).Trim
            passwd = CStr(AppClass.FetchDBValue("SELECT emailPwd FROM tblEmailSetting"))
            decryptedPwd = CStr(AppClass.Decrypt(passwd)).Trim
        Else
            AppClass.ShowNotification("Email Credentials Not Set")
            Exit Sub
        End If
        AddParams("@no", MemberNo)
        Dim emailTo As String = CStr(AppClass.FetchDBValue("SELECT email FROM tblMemberAddress WHERE memberNo=@no")).Trim
        Try
            Dim spath As String = Application.StartupPath & "\Receipts\" & ReceiptNoTextEdit.EditValue.ToString & ".pdf"
            Dim Smtp_Server As New SmtpClient
            Dim e_mail As New MailMessage()
            Smtp_Server.UseDefaultCredentials = False
            Smtp_Server.Credentials = New Net.NetworkCredential(email, decryptedPwd)
            Smtp_Server.Port = 587
            Smtp_Server.EnableSsl = True
            Smtp_Server.Host = "smtp.gmail.com"

            Dim messageBody As String = "Attached Find Receipt from P.C.E.A Housing Cooperative"

            e_mail = New MailMessage With {
                .From = New MailAddress(email)
            }
            e_mail.To.Add(emailTo)
            e_mail.Subject = "Receipt From PCEA Housing - " & ReceiptNoTextEdit.EditValue.ToString
            e_mail.IsBodyHtml = False
            e_mail.Body = messageBody
            If FileExists(spath) Then
                Dim attach As Attachment
                attach = New Attachment(spath)
                e_mail.Attachments.Add(attach)
            End If

            Smtp_Server.Send(e_mail)

        Catch ex As Exception
            AppClass.ShowError(ex.Message)
        End Try
    End Sub
    Public Shared Function CheckForInternetConnection() As Boolean
        Try
            Using client = New WebClient()
                Using stream = client.OpenRead("http://www.google.com")
                    Return True
                End Using
            End Using
        Catch
            Return False
        End Try
    End Function
    Private Function FileExists(ByVal FileFullPath As String) _
     As Boolean
        If Trim(FileFullPath) = "" Then Return False

        Dim f As New IO.FileInfo(FileFullPath)
        Return f.Exists

    End Function
#End Region
End Class
