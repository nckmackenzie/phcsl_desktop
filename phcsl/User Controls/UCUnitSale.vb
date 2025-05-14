Imports System.Data.SqlClient
Imports DevExpress.XtraEditors
Imports System.IO
Imports System.Text
Imports System.Drawing.Printing
Imports System.Drawing.Imaging
Imports Microsoft.Reporting.WinForms
Imports System.Net.Mail
Imports System.Net
Public Class UCUnitSale
    Private Shared _instance As UCUnitSale
    Private IsEdit As Boolean
    Private ID As Integer
    Private BankRequired As Boolean
    Private IsReprint As Boolean
    Private Shared M_streams As List(Of Stream)
    Private Shared M_currentPageIndex As Integer = 0
    Private InitialAmountDeductedFromDeposits As Decimal
    Private InitialTitleID As Integer
    Public Shared ReadOnly Property Instance As UCUnitSale
        Get
            If _instance Is Nothing Then _instance = New UCUnitSale()
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
        AppClass.LoadToLookUpEdit("SELECT ID,methodName FROM tblPaymentMethod", PaymethodLookUpEdit, "methodName", "ID")
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(bankName) as BankName FROM tblBanks", BankLookUpEdit, "BankName", "ID")
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(agentName) as AgentName FROM tblAgents WHERE active=1", SaleAgentLookUpEdit, "AgentName", "ID")
        sql = "SELECT p.ID,UPPER(p.projectName) as ProjectName "
        sql &= "FROM tblProjects p WHERE (SELECT COUNT(*) FROM tblUnits WHERE sold=1 and projectId = p.ID) < noOfUnits"
        AppClass.LoadToLookUpEdit(sql, ProjectLookUpEdit, "ProjectName", "ID")
        sql = "SELECT M.memberNo,M.memberID AS [MemberID],UPPER(M.memberName) AS [MemberName],M.idNo AS [IDNo],"
        sql &= "A.contact AS [Contact] FROM tblMember M LEFT OUTER JOIN tblMemberAddress A ON M.memberNo=A.memberNo ORDER BY M.memberID"
        AppClass.LoadToSearchLookUpEdit(sql, MemberSearchLookUpEdit, "MemberName", "memberNo")
        AppClass.LoadToSearchLookUpEdit(sql, RefeeringMemberSearchLookUpEdit, "MemberName", "memberNo")
        AppClass.ClearItems(LayoutControl1)
        MemberNonMemberComboBoxEdit.SelectedIndex = 0
        MemberSearchLookUpEdit.Properties.ReadOnly = False
        NoneMemberNameTextEdit.Properties.ReadOnly = True
        ContactTextEdit.Properties.ReadOnly = True
        IDNoTextEdit.Properties.ReadOnly = True
        DiscountValueTextEdit.Properties.ReadOnly = True
        RefeeringMemberSearchLookUpEdit.Properties.ReadOnly = True
        PaymethodLookUpEdit.Properties.ReadOnly = False
        ProjectLookUpEdit.Properties.ReadOnly = False
        UnitLookUpEdit.Properties.ReadOnly = False
        TitleTextEdit.Properties.ReadOnly = False
        DeductionTypeComboBoxEdit.Properties.ReadOnly = False
        PlanComboBoxEdit.SelectedIndex = 0
        DeductionTypeComboBoxEdit.SelectedIndex = 3
        IsEdit = False
        ID = Nothing
        DeleteSimpleButton.Enabled = False
        ReprintSimpleButton.Enabled = False
        DiscountTypeComboBoxEdit.Properties.ReadOnly = True
        BankLookUpEdit.Properties.ReadOnly = True
        BankRequired = False
        InitialAmountDeductedFromDeposits = 0
        InitialTitleID = 0
    End Sub
    Function GetDiscountedAmount(UnitValue As Decimal) As Decimal
        If Not DiscountCheckEdit.Checked Then
            Return 0
        End If
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
        Dim UnitPrice As Decimal = If(UnitPriceTextEdit.EditValue IsNot Nothing, CDec(UnitPriceTextEdit.EditValue), 0)
        Dim DiscountedAmount = GetDiscountedAmount(UnitPrice)
        DiscountedAmountTextEdit.EditValue = DiscountedAmount.ToString("N")
        Return UnitPrice - DiscountedAmount
    End Function
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
    Sub GetUnitPrice()
        If PlanComboBoxEdit.SelectedIndex = 1 Then
            If MemberNonMemberComboBoxEdit.SelectedIndex = 1 Then
                AddParams("@id", CInt(UnitLookUpEdit.EditValue))
                UnitPriceTextEdit.EditValue = CDec(AppClass.FetchDBValue("SELECT COALESCE(sellingPriceNonMemberTwelvePlan,0) AS UnitPrice FROM tblUnits WHERE ID=@id"))
            Else
                AddParams("@id", CInt(UnitLookUpEdit.EditValue))
                UnitPriceTextEdit.EditValue = CDec(AppClass.FetchDBValue("SELECT COALESCE(sellingPriceMemberTwelvePlan,0) AS UnitPrice FROM tblUnits WHERE ID=@id"))
            End If
        Else
            If MemberNonMemberComboBoxEdit.SelectedIndex = 1 Then
                AddParams("@id", CInt(UnitLookUpEdit.EditValue))
                UnitPriceTextEdit.EditValue = CDec(AppClass.FetchDBValue("SELECT COALESCE(unitSellingPriceNonMember,0) AS UnitPrice FROM tblUnits WHERE ID=@id"))
            Else
                AddParams("@id", CInt(UnitLookUpEdit.EditValue))
                UnitPriceTextEdit.EditValue = CDec(AppClass.FetchDBValue("SELECT COALESCE(unitSellingPriceMember,0) AS UnitPrice FROM tblUnits WHERE ID=@id"))
            End If
        End If
        Dim PlansSelected As Boolean = PlanComboBoxEdit.SelectedIndex > -1
        'If Not PlansSelected Then
        '    If MemberNonMemberComboBoxEdit.SelectedIndex = 1 Then
        '        AddParams("@id", CInt(UnitLookUpEdit.EditValue))
        '        UnitPriceTextEdit.EditValue = CDec(AppClass.FetchDBValue("SELECT COALESCE(unitSellingPriceNonMember,0) AS UnitPrice FROM tblUnits WHERE ID=@id"))
        '    Else
        '        AddParams("@id", CInt(UnitLookUpEdit.EditValue))
        '        UnitPriceTextEdit.EditValue = CDec(AppClass.FetchDBValue("SELECT COALESCE(unitSellingPriceMember,0) AS UnitPrice FROM tblUnits WHERE ID=@id"))
        '    End If
        'Else
        '    If PlanComboBoxEdit.SelectedIndex = 0 Then
        '        AddParams("@id", CInt(UnitLookUpEdit.EditValue))
        '        UnitPriceTextEdit.EditValue = CDec(AppClass.FetchDBValue("SELECT COALESCE(sellingPriceSixPlan,0) AS UnitPrice FROM tblUnits WHERE ID=@id"))
        '    Else
        '        AddParams("@id", CInt(UnitLookUpEdit.EditValue))
        '        UnitPriceTextEdit.EditValue = CDec(AppClass.FetchDBValue("SELECT COALESCE(sellingPriceTwelvePlan,0) AS UnitPrice FROM tblUnits WHERE ID=@id"))
        '    End If
        'End If
        NetTextEdit.EditValue = CalculateNetAmount().ToString("N")
    End Sub
    Function Datavalidation() As Boolean
        errmsg = Nothing

        If ProjectLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select project"
            ProjectLookUpEdit.Focus()
        ElseIf UnitLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select unit"
            UnitLookUpEdit.Focus()
        ElseIf MemberNonMemberComboBoxEdit.SelectedIndex = -1 Then
            errmsg = "Select whether selling to member or non member"
            MemberNonMemberComboBoxEdit.Focus()
        ElseIf MemberNonMemberComboBoxEdit.SelectedIndex = 0 AndAlso MemberSearchLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select buying member"
            MemberSearchLookUpEdit.Focus()
        ElseIf MemberNonMemberComboBoxEdit.SelectedIndex = 1 AndAlso NoneMemberNameTextEdit.EditValue Is Nothing Then
            errmsg = "Enter non member name"
            NoneMemberNameTextEdit.Focus()
        ElseIf MemberNonMemberComboBoxEdit.SelectedIndex = 1 AndAlso IDNoTextEdit.EditValue Is Nothing Then
            errmsg = "Enter non member ID Number"
            IDNoTextEdit.Focus()
        ElseIf MemberNonMemberComboBoxEdit.SelectedIndex = 1 AndAlso ContactTextEdit.EditValue Is Nothing Then
            errmsg = "Enter non member contact"
            ContactTextEdit.Focus()
        ElseIf DiscountCheckEdit.Checked AndAlso DiscountTypeComboBoxEdit.SelectedIndex = -1 Then
            errmsg = "Select discount type"
            DiscountTypeComboBoxEdit.Focus()
        ElseIf DiscountCheckEdit.Checked AndAlso DiscountValueTextEdit.EditValue Is Nothing Then
            errmsg = "Enter discount amount/percent"
            DiscountValueTextEdit.Focus()
        ElseIf AmountPaidUnitTextEdit.EditValue Is Nothing Then
            errmsg = "Enter amount paid for unit"
            AmountPaidUnitTextEdit.Focus()
        ElseIf DeductionTypeComboBoxEdit.SelectedIndex > 0 AndAlso PaymethodLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select payment method"
            PaymethodLookUpEdit.Focus()
        ElseIf BankRequired AndAlso BankLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select bank"
            BankLookUpEdit.Focus()
        ElseIf ReferenceTextEdit.EditValue Is Nothing Then
            errmsg = "Enter reference"
            ReferenceTextEdit.Focus()
        ElseIf ReferalCheckEdit.Checked AndAlso RefeeringMemberSearchLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select refering member"
            RefeeringMemberSearchLookUpEdit.Focus()
        ElseIf SaleDateEdit.EditValue Is Nothing Then
            errmsg = "Enter sale date"
            SaleDateEdit.Focus()
        ElseIf CDec(AmountPaidUnitTextEdit.EditValue) > CDec(NetTextEdit.EditValue) Then
            errmsg = "Cannot pay more than total net value"
            AmountPaidUnitTextEdit.Focus()
        End If

        If errmsg IsNot Nothing Then
            AppClass.ShowError(errmsg, True)
            Return False
        Else
            Return True
        End If
    End Function
    Sub Save()
        Dim DeductionsID As Integer = AppClass.GenerateDBID("tblDepositDeduction")
        Dim TitlePayID As Integer = AppClass.GenerateDBID("tblTitlePay")
        Dim TitleFee As Decimal = If(TitleTextEdit.EditValue IsNot Nothing, CDec(TitleTextEdit.EditValue), 0)
        Dim UnitFee As Decimal = If(AmountPaidUnitTextEdit.EditValue IsNot Nothing, CDec(AmountPaidUnitTextEdit.EditValue), 0)
        Dim TotalDeductions As Decimal = TitleFee + UnitFee
        Dim DeductionType As Integer = 1
        Select Case DeductionTypeComboBoxEdit.SelectedIndex
            Case 0
                DeductionType = 2
            Case 1
                DeductionType = 3
            Case 2
                DeductionType = 4
            Case 3
                DeductionType = 1
        End Select
        ID = AppClass.GenerateDBID("tblUnitSale")
        Using connection As New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Using MyTransaction As SqlTransaction = connection.BeginTransaction
                Try

                    sql = "INSERT INTO tblUnitSale (ID,projectId,unitId,saleType,discounted,discountPercent,discount,netAmount,titleAmount,unitAmount,deductionType,initialUnitFee,paymentId,bankId,discountType,paymentReference,referal) "
                    sql &= "VALUES(@id,@pid,@uid,@stype,@disc,@discval,@discamount,@net,@title,@unit,@dtype,@initial,@pay,@bid,@disctype,@ref,@refer)"

                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = ID
                            .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = CInt(ProjectLookUpEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@uid", SqlDbType.Int)).Value = CInt(UnitLookUpEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@stype", SqlDbType.Int)).Value = MemberNonMemberComboBoxEdit.SelectedIndex + 1
                            .Parameters.Add(New SqlParameter("@disc", SqlDbType.Bit)).Value = DiscountCheckEdit.Checked
                            .Parameters.Add(New SqlParameter("@discval", SqlDbType.Decimal)).Value = If(DiscountCheckEdit.Checked, CDec(DiscountValueTextEdit.EditValue), 0)
                            .Parameters.Add(New SqlParameter("@discamount", SqlDbType.Decimal)).Value = If(DiscountCheckEdit.Checked, CDec(DiscountedAmountTextEdit.EditValue), 0)
                            .Parameters.Add(New SqlParameter("@net", SqlDbType.Decimal)).Value = CDec(NetTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@title", SqlDbType.Decimal)).Value = If(TitleTextEdit.EditValue IsNot Nothing, CDec(TitleTextEdit.EditValue), 0)
                            .Parameters.Add(New SqlParameter("@unit", SqlDbType.Decimal)).Value = CDec(AmountPaidUnitTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@dtype", SqlDbType.Int)).Value = DeductionType
                            .Parameters.Add(New SqlParameter("@initial", SqlDbType.Decimal)).Value = CDec(UnitPriceTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@pay", SqlDbType.Int)).Value = PaymethodLookUpEdit.EditValue
                            If BankLookUpEdit.EditValue IsNot Nothing Then
                                .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = BankLookUpEdit.EditValue
                            Else
                                .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = DBNull.Value
                            End If
                            .Parameters.Add(New SqlParameter("@disctype", SqlDbType.Int)).Value = If(DiscountCheckEdit.Checked, DiscountTypeComboBoxEdit.SelectedIndex + 1, DBNull.Value)
                            .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = ReferenceTextEdit.EditValue.ToString.ToLower
                            .Parameters.Add(New SqlParameter("@refer", SqlDbType.Bit)).Value = ReferalCheckEdit.Checked
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "UPDATE tblUnits SET sold=@sold,soldBy=@by,soldDate=@sdate,unitPaid=@paid,memberNo=@member,nonMemberName=@name,nonMemberID=@idno,nonMemberContact=@contact "
                    sql &= "WHERE ID=@id"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@sold", SqlDbType.Bit)).Value = True
                            .Parameters.Add(New SqlParameter("@by", SqlDbType.Int)).Value = If(SaleAgentLookUpEdit.EditValue, DBNull.Value)
                            .Parameters.Add(New SqlParameter("@sdate", SqlDbType.Date)).Value = CDate(SaleDateEdit.EditValue).Date
                            .Parameters.Add(New SqlParameter("@paid", SqlDbType.Int)).Value = CDec(AmountPaidUnitTextEdit.EditValue)
                            If MemberNonMemberComboBoxEdit.SelectedIndex = 0 Then
                                .Parameters.Add(New SqlParameter("@member", SqlDbType.Int)).Value = MemberSearchLookUpEdit.EditValue
                                .Parameters.Add(New SqlParameter("@name", SqlDbType.VarChar)).Value = DBNull.Value
                                .Parameters.Add(New SqlParameter("@idno", SqlDbType.VarChar)).Value = DBNull.Value
                                .Parameters.Add(New SqlParameter("@contact", SqlDbType.VarChar)).Value = DBNull.Value
                            Else
                                .Parameters.Add(New SqlParameter("@member", SqlDbType.Int)).Value = DBNull.Value
                                .Parameters.Add(New SqlParameter("@name", SqlDbType.VarChar)).Value = CStr(NoneMemberNameTextEdit.EditValue).ToLower
                                .Parameters.Add(New SqlParameter("@idno", SqlDbType.VarChar)).Value = IDNoTextEdit.EditValue
                                .Parameters.Add(New SqlParameter("@contact", SqlDbType.VarChar)).Value = ContactTextEdit.EditValue
                            End If
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = UnitLookUpEdit.EditValue
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "INSERT INTO tblUnitPayment (unitId,openingBal,paymentDate,amountPaid,closingBal,transactionType,transactionId) "
                    sql &= "VALUES (@uid,@obal,@date,@paid,@cbal,@type,@tid)"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@uid", SqlDbType.Int)).Value = CInt(UnitLookUpEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@obal", SqlDbType.Decimal)).Value = CDec(NetTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = CDate(SaleDateEdit.EditValue).Date
                            .Parameters.Add(New SqlParameter("@paid", SqlDbType.Decimal)).Value = CDec(AmountPaidUnitTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@cbal", SqlDbType.Decimal)).Value = CDec(NetTextEdit.EditValue) - CDec(AmountPaidUnitTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 1
                            .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    If DeductionTypeComboBoxEdit.SelectedIndex < 3 Then
                        sql = "INSERT INTO tblDepositDeduction (ID,deductionDate,deductionType,memberNo,amount,projectId,unitId,transactionType,transactionId) "
                        sql &= "VALUES (@id,@date,@type,@member,@amount,@pid,@uid,@ttype,@tid)"
                        Using cmd As New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = DeductionsID
                                .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = CDate(SaleDateEdit.EditValue).Date
                                .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 1
                                .Parameters.Add(New SqlParameter("@member", SqlDbType.Int)).Value = MemberSearchLookUpEdit.EditValue
                                .Parameters.Add(New SqlParameter("@amount", SqlDbType.Decimal)).Value = TotalDeductions
                                .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = CInt(ProjectLookUpEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@uid", SqlDbType.Int)).Value = CInt(UnitLookUpEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@ttype", SqlDbType.Int)).Value = 1
                                .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If

                    If TitleTextEdit.EditValue IsNot Nothing AndAlso CDec(TitleTextEdit.EditValue) > 0 Then
                        sql = "INSERT INTO tblTitlePay (ID,unitId,amountPaid,payDate,memberType,memberId,nonMember,paymethodId,bankId,payReference,projectId,deducted)"
                        sql &= " VALUES(@id,@uid,@amount,@date,@type,@member,@nonm,@payid,@bid,@ref,@pid,@deducted)"
                        Using cmd As New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = TitlePayID
                                .Parameters.Add(New SqlParameter("@uid", SqlDbType.Int)).Value = CInt(UnitLookUpEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@amount", SqlDbType.Decimal)).Value = CDec(TitleTextEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = CDate(SaleDateEdit.EditValue).Date
                                .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = MemberNonMemberComboBoxEdit.SelectedIndex + 1
                                If MemberNonMemberComboBoxEdit.SelectedIndex = 0 Then
                                    .Parameters.Add(New SqlParameter("@member", SqlDbType.Int)).Value = MemberSearchLookUpEdit.EditValue
                                Else
                                    .Parameters.Add(New SqlParameter("@nonm", SqlDbType.VarChar)).Value = CStr(NoneMemberNameTextEdit.EditValue).ToLower
                                End If
                                .Parameters.Add(New SqlParameter("@payid", SqlDbType.Int)).Value = PaymethodLookUpEdit.EditValue
                                If BankLookUpEdit.EditValue IsNot Nothing Then
                                    .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = BankLookUpEdit.EditValue
                                Else
                                    .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = DBNull.Value
                                End If
                                .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = ReferenceTextEdit.EditValue.ToString.ToLower
                                .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = CInt(ProjectLookUpEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@deducted", SqlDbType.Bit)).Value = DeductionTypeComboBoxEdit.SelectedIndex < 3
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If

                    If PaymethodLookUpEdit.EditValue = 1 AndAlso DeductionTypeComboBoxEdit.SelectedIndex = 3 Then
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "cash ccount", "Project Payment", TotalDeductions, 0, Nothing, 2, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "unit payment", "Unit Payment", 0, TotalDeductions, Nothing, 5, 1, ID)
                    ElseIf PaymethodLookUpEdit.EditValue > 1 AndAlso PaymethodLookUpEdit.EditValue < 5 AndAlso DeductionTypeComboBoxEdit.SelectedIndex = 3 Then
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "bank ccount", "Project Payment", TotalDeductions, 0, Nothing, 2, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "unit payment", "Unit Payment", 0, TotalDeductions, Nothing, 5, 1, ID)
                    ElseIf DeductionTypeComboBoxEdit.SelectedIndex = 2 Then
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "deposits", "Title Payment", CDec(TitleTextEdit.EditValue), 0, Nothing, 3, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "title fees", "Unit Payment", 0, CDec(TitleTextEdit.EditValue), Nothing, 5, 1, ID)
                        If PaymethodLookUpEdit.EditValue = 1 Then
                            AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "cash ccount", "Project Payment", CDec(AmountPaidUnitTextEdit.EditValue), 0, Nothing, 2, 1, ID)
                            AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "unit payment", "Unit Payment", 0, CDec(AmountPaidUnitTextEdit.EditValue), Nothing, 5, 1, ID)
                        ElseIf PaymethodLookUpEdit.EditValue > 1 AndAlso PaymethodLookUpEdit.EditValue < 5 Then
                            AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "bank ccount", "Project Payment", CDec(AmountPaidUnitTextEdit.EditValue), 0, Nothing, 2, 1, ID)
                            AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "unit payment", "Unit Payment", 0, CDec(AmountPaidUnitTextEdit.EditValue), Nothing, 5, 1, ID)
                        End If
                    ElseIf DeductionTypeComboBoxEdit.SelectedIndex = 1 Then
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "deposits", "Unit Payment", CDec(AmountPaidUnitTextEdit.EditValue), 0, Nothing, 3, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "unit payment", "Unit Payment", 0, CDec(AmountPaidUnitTextEdit.EditValue), Nothing, 5, 1, ID)
                        If TitleTextEdit.EditValue IsNot Nothing AndAlso CDec(TitleTextEdit.EditValue) > 0 Then
                            If PaymethodLookUpEdit.EditValue = 1 Then
                                AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "cash ccount", "Title Payment", CDec(TitleTextEdit.EditValue), 0, Nothing, 2, 1, ID)
                                AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "title fees", "Title Payment", 0, CDec(TitleTextEdit.EditValue), Nothing, 5, 1, ID)
                            ElseIf PaymethodLookUpEdit.EditValue > 1 AndAlso PaymethodLookUpEdit.EditValue < 5 Then
                                AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "bank ccount", "Project Payment", CDec(TitleTextEdit.EditValue), 0, Nothing, 2, 1, ID)
                                AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "title fees", "Title Payment", 0, CDec(TitleTextEdit.EditValue), Nothing, 5, 1, ID)
                            End If
                        End If
                    ElseIf DeductionTypeComboBoxEdit.SelectedIndex = 0 Then
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "deposits", "Unit Payment", CDec(AmountPaidUnitTextEdit.EditValue), 0, Nothing, 3, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "unit payment", "Unit Payment", 0, CDec(AmountPaidUnitTextEdit.EditValue), Nothing, 5, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "deposits", "Title Payment", CDec(TitleTextEdit.EditValue), 0, Nothing, 3, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "title fees", "Unit Payment", 0, CDec(TitleTextEdit.EditValue), Nothing, 5, 1, ID)
                    End If

                    If BankRequired AndAlso DeductionTypeComboBoxEdit.SelectedIndex = 3 Then
                        AppClass.BankPosting(CDate(SaleDateEdit.EditValue).Date, BankLookUpEdit.EditValue, TotalDeductions, 0, "Units Payment-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                            , PaymethodLookUpEdit.EditValue, False, 1, ID)
                    ElseIf BankRequired AndAlso DeductionTypeComboBoxEdit.SelectedIndex = 2 Then
                        AppClass.BankPosting(CDate(SaleDateEdit.EditValue).Date, BankLookUpEdit.EditValue, AmountPaidUnitTextEdit.EditValue, 0, "Units Payment-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                           , PaymethodLookUpEdit.EditValue, False, 1, ID)
                    ElseIf BankRequired AndAlso DeductionTypeComboBoxEdit.SelectedIndex = 1 AndAlso CDec(TitleTextEdit.EditValue) > 0 Then
                        AppClass.BankPosting(CDate(SaleDateEdit.EditValue).Date, BankLookUpEdit.EditValue, TitleTextEdit.EditValue, 0, "Title Payment-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                           , PaymethodLookUpEdit.EditValue, False, 1, ID)
                    End If


                    MyTransaction.Commit()
                    AppClass.ShowNotification("Saved Successfully")
                    If AppClass.AlertQuestion("Do You Wish To Print A Receipt For This Transaction?") = System.Windows.Forms.DialogResult.Yes Then
                        ReportStuff(False)
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
    Sub Find(fid As Int16)
        IsEdit = True
        sql = "SELECT ID,UPPER(projectName) as ProjectName "
        sql &= "FROM tblProjects"
        AppClass.LoadToLookUpEdit(sql, ProjectLookUpEdit, "ProjectName", "ID")

        AddParams("@id", fid)
        Dim Searchdt As DataTable = AppClass.LoadToDatatable("SELECT * FROM tblUnitSale WHERE ID=@id", False)
        ID = Searchdt.Rows(0)(0)
        ProjectLookUpEdit.EditValue = Searchdt.Rows(0)(1)
        UnitLookUpEdit.EditValue = Searchdt.Rows(0)(2)
        MemberNonMemberComboBoxEdit.SelectedIndex = Searchdt.Rows(0)(3) - 1
        DiscountCheckEdit.Checked = CBool(Searchdt.Rows(0)(4))
        If IsDBNull(Searchdt.Rows(0)(14)) Then
            DiscountTypeComboBoxEdit.EditValue = Nothing
        Else
            DiscountTypeComboBoxEdit.SelectedIndex = CInt(Searchdt.Rows(0)(14)) - 1
        End If
        DiscountValueTextEdit.EditValue = If(Searchdt.Rows(0)(5) > 0, Searchdt.Rows(0)(5), Nothing)
        DiscountedAmountTextEdit.EditValue = If(Searchdt.Rows(0)(6) > 0, CDec(Searchdt.Rows(0)(6)), Nothing)
        NetTextEdit.EditValue = CDec(Searchdt.Rows(0)(7))
        TitleTextEdit.EditValue = If(Searchdt.Rows(0)(8) > 0, CDec(Searchdt.Rows(0)(8)), Nothing)
        AmountPaidUnitTextEdit.EditValue = CDec(Searchdt.Rows(0)(9))
        If Searchdt.Rows(0)(10) = 1 Then
            DeductionTypeComboBoxEdit.SelectedIndex = 3
        ElseIf Searchdt.Rows(0)(10) = 2 Then
            DeductionTypeComboBoxEdit.SelectedIndex = 0
        ElseIf Searchdt.Rows(0)(10) = 3 Then
            DeductionTypeComboBoxEdit.SelectedIndex = 1
        End If
        UnitPriceTextEdit.EditValue = CDec(Searchdt.Rows(0)(11))
        PaymethodLookUpEdit.EditValue = Searchdt.Rows(0)(12)
        If IsDBNull(Searchdt.Rows(0)(13)) Then
            BankLookUpEdit.EditValue = Nothing
        Else
            BankLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(13))
        End If
        If IsDBNull(Searchdt.Rows(0)(15)) Then
            ReferenceTextEdit.EditValue = Nothing
        Else
            ReferenceTextEdit.EditValue = Searchdt.Rows(0)(15).ToString.ToUpper
        End If
        ReferalCheckEdit.Checked = CBool(Searchdt.Rows(0)(16))

        sql = "SELECT soldDate,memberNo,nonMemberName,soldBy,coalesce(sellingPriceSixPlan,0),coalesce(sellingPriceTwelvePlan,0),nonMemberID,nonMemberContact FROM tblUnits WHERE ID=@id"
        AddParams("@id", UnitLookUpEdit.EditValue)
        Dim Unitdt As DataTable = AppClass.LoadToDatatable(sql, False)
        SaleDateEdit.EditValue = CDate(Unitdt.Rows(0)(0))
        If MemberNonMemberComboBoxEdit.SelectedIndex = 0 Then
            MemberSearchLookUpEdit.EditValue = Unitdt.Rows(0)(1)
        Else
            MemberSearchLookUpEdit.EditValue = Nothing
            NoneMemberNameTextEdit.EditValue = Unitdt.Rows(0)(2).ToString.ToUpper
            If IsDBNull(Unitdt.Rows(0)(6)) Then
                IDNoTextEdit.EditValue = Nothing
            Else
                IDNoTextEdit.EditValue = Unitdt.Rows(0)(6).ToString.ToUpper
            End If
            If IsDBNull(Unitdt.Rows(0)(7)) Then
                ContactTextEdit.EditValue = Nothing
            Else
                ContactTextEdit.EditValue = Unitdt.Rows(0)(7).ToString.ToUpper
            End If
        End If
        If IsDBNull(Unitdt.Rows(0)(3)) Then
            SaleAgentLookUpEdit.EditValue = Nothing
        Else
            SaleAgentLookUpEdit.EditValue = Unitdt.Rows(0)(3)
        End If
        If CDec(Unitdt.Rows(0)(4)) = CDec(Searchdt.Rows(0)(11)) Then
            PlanComboBoxEdit.SelectedIndex = 0
        ElseIf CDec(Unitdt.Rows(0)(5)) = CDec(Searchdt.Rows(0)(11)) Then
            PlanComboBoxEdit.SelectedIndex = 1
        End If

        AddParams("@uid", CInt(UnitLookUpEdit.EditValue))
        Dim HasOtherPayment As Boolean = CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblUnitPayment WHERE (unitId=@uid)")) > 1

        AddParams("@id", ID)
        InitialAmountDeductedFromDeposits = CDec(AppClass.FetchDBValue("SELECT COALESCE(amount,0) FROM tblDepositDeduction WHERE transactionType=1 AND transactionId=@id"))

        AddParams("@uid", CInt(UnitLookUpEdit.EditValue))
        AddParams("@amount", CDec(TitleTextEdit.EditValue))
        AddParams("@date", CDate(SaleDateEdit.EditValue))
        If CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblTitlePay WHERE unitId=@uid AND amountPaid=@amount AND payDate=@date")) > 0 Then
            AddParams("@uid", CInt(UnitLookUpEdit.EditValue))
            AddParams("@amount", CDec(TitleTextEdit.EditValue))
            AddParams("@date", CDate(SaleDateEdit.EditValue))
            InitialTitleID = CInt(AppClass.FetchDBValue("SELECT ID FROM tblTitlePay WHERE unitId=@uid AND amountPaid=@amount AND payDate=@date"))
        End If
        DeleteSimpleButton.Enabled = Not HasOtherPayment
        ReprintSimpleButton.Enabled = True
        ProjectLookUpEdit.Properties.ReadOnly = True
        UnitLookUpEdit.Properties.ReadOnly = True
    End Sub
    Sub Edit()
        Dim DeductionsID As Integer = AppClass.GenerateDBID("tblDepositDeduction")
        Dim TitlePayID As Integer = AppClass.GenerateDBID("tblTitlePay")
        Dim TitleFee As Decimal = If(TitleTextEdit.EditValue IsNot Nothing, CDec(TitleTextEdit.EditValue), 0)
        Dim UnitFee As Decimal = If(AmountPaidUnitTextEdit.EditValue IsNot Nothing, CDec(AmountPaidUnitTextEdit.EditValue), 0)
        Dim TotalDeductions As Decimal = TitleFee + UnitFee
        Dim DeductionType As Integer = 1
        Select Case DeductionTypeComboBoxEdit.SelectedIndex
            Case 0
                DeductionType = 2
            Case 1
                DeductionType = 3
            Case 2
                DeductionType = 4
            Case 3
                DeductionType = 1
        End Select

        Using connection As New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Using MyTransaction As SqlTransaction = connection.BeginTransaction
                Try

                    sql = "UPDATE tblUnitSale SET saleType=@stype,discounted=@disc,discountPercent=@discval,discount=@discamount,netAmount=@net,titleAmount=@title, "
                    sql &= "unitAmount=@unit,deductionType=@dtype,initialUnitFee=@initial,paymentId=@pay,bankId=@bid,discountType=@disctype,paymentReference=@ref,referal=@refer "
                    sql &= "WHERE ID=@id"

                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@stype", SqlDbType.Int)).Value = MemberNonMemberComboBoxEdit.SelectedIndex + 1
                            .Parameters.Add(New SqlParameter("@disc", SqlDbType.Bit)).Value = DiscountCheckEdit.Checked
                            .Parameters.Add(New SqlParameter("@discval", SqlDbType.Decimal)).Value = If(DiscountCheckEdit.Checked, CDec(DiscountValueTextEdit.EditValue), 0)
                            .Parameters.Add(New SqlParameter("@discamount", SqlDbType.Decimal)).Value = If(DiscountCheckEdit.Checked, CDec(DiscountedAmountTextEdit.EditValue), 0)
                            .Parameters.Add(New SqlParameter("@net", SqlDbType.Decimal)).Value = CDec(NetTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@title", SqlDbType.Decimal)).Value = If(TitleTextEdit.EditValue IsNot Nothing, CDec(TitleTextEdit.EditValue), 0)
                            .Parameters.Add(New SqlParameter("@unit", SqlDbType.Decimal)).Value = CDec(AmountPaidUnitTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@dtype", SqlDbType.Int)).Value = DeductionType
                            .Parameters.Add(New SqlParameter("@initial", SqlDbType.Decimal)).Value = CDec(UnitPriceTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@pay", SqlDbType.Int)).Value = PaymethodLookUpEdit.EditValue
                            If BankLookUpEdit.EditValue IsNot Nothing Then
                                .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = BankLookUpEdit.EditValue
                            Else
                                .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = DBNull.Value
                            End If
                            .Parameters.Add(New SqlParameter("@disctype", SqlDbType.Int)).Value = If(DiscountCheckEdit.Checked, DiscountTypeComboBoxEdit.SelectedIndex + 1, DBNull.Value)
                            .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = ReferenceTextEdit.EditValue.ToString.ToLower
                            .Parameters.Add(New SqlParameter("@refer", SqlDbType.Bit)).Value = ReferalCheckEdit.Checked
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "UPDATE tblUnits SET sold=@sold,soldBy=@by,soldDate=@sdate,unitPaid=@paid,memberNo=@member,nonMemberName=@name,nonMemberID=@idno,nonMemberContact=@contact "
                    sql &= "WHERE ID=@id"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@sold", SqlDbType.Bit)).Value = True
                            .Parameters.Add(New SqlParameter("@by", SqlDbType.Int)).Value = If(SaleAgentLookUpEdit.EditValue, DBNull.Value)
                            .Parameters.Add(New SqlParameter("@sdate", SqlDbType.Date)).Value = CDate(SaleDateEdit.EditValue).Date
                            .Parameters.Add(New SqlParameter("@paid", SqlDbType.Int)).Value = CDec(AmountPaidUnitTextEdit.EditValue)
                            If MemberNonMemberComboBoxEdit.SelectedIndex = 0 Then
                                .Parameters.Add(New SqlParameter("@member", SqlDbType.Int)).Value = MemberSearchLookUpEdit.EditValue
                                .Parameters.Add(New SqlParameter("@name", SqlDbType.VarChar)).Value = DBNull.Value
                                .Parameters.Add(New SqlParameter("@idno", SqlDbType.VarChar)).Value = DBNull.Value
                                .Parameters.Add(New SqlParameter("@contact", SqlDbType.VarChar)).Value = DBNull.Value
                            Else
                                .Parameters.Add(New SqlParameter("@member", SqlDbType.Int)).Value = DBNull.Value
                                .Parameters.Add(New SqlParameter("@name", SqlDbType.VarChar)).Value = CStr(NoneMemberNameTextEdit.EditValue).ToLower
                                .Parameters.Add(New SqlParameter("@idno", SqlDbType.VarChar)).Value = IDNoTextEdit.EditValue
                                .Parameters.Add(New SqlParameter("@contact", SqlDbType.VarChar)).Value = ContactTextEdit.EditValue
                            End If
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = UnitLookUpEdit.EditValue
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "DELETE FROM tblUnitPayment WHERE transactionType=1 AND transactionId=@tid"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "DELETE FROM tblDepositDeduction WHERE transactionType=1 AND transactionId=@tid"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    If InitialTitleID > 0 Then
                        sql = "DELETE FROM tblTitlePay WHERE ID=@id"
                        Using cmd As New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = InitialTitleID
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If

                    sql = "DELETE FROM tblLedger WHERE transactionType=1 AND transactionID=@tid"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "DELETE FROM tblBankPostings WHERE transactionType=1 AND transactionID=@tid"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "INSERT INTO tblUnitPayment (unitId,openingBal,paymentDate,amountPaid,closingBal,transactionType,transactionId) "
                    sql &= "VALUES (@uid,@obal,@date,@paid,@cbal,@type,@tid)"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@uid", SqlDbType.Int)).Value = CInt(UnitLookUpEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@obal", SqlDbType.Decimal)).Value = CDec(NetTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = CDate(SaleDateEdit.EditValue).Date
                            .Parameters.Add(New SqlParameter("@paid", SqlDbType.Decimal)).Value = CDec(AmountPaidUnitTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@cbal", SqlDbType.Decimal)).Value = CDec(NetTextEdit.EditValue) - CDec(AmountPaidUnitTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 1
                            .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    If DeductionTypeComboBoxEdit.SelectedIndex < 3 Then
                        sql = "INSERT INTO tblDepositDeduction (ID,deductionDate,deductionType,memberNo,amount,projectId,unitId,transactionType,transactionId) "
                        sql &= "VALUES (@id,@date,@type,@member,@amount,@pid,@uid,@ttype,@tid)"
                        Using cmd As New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = DeductionsID
                                .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = CDate(SaleDateEdit.EditValue).Date
                                .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = 1
                                .Parameters.Add(New SqlParameter("@member", SqlDbType.Int)).Value = MemberSearchLookUpEdit.EditValue
                                .Parameters.Add(New SqlParameter("@amount", SqlDbType.Decimal)).Value = TotalDeductions
                                .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = CInt(ProjectLookUpEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@uid", SqlDbType.Int)).Value = CInt(UnitLookUpEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@ttype", SqlDbType.Int)).Value = 1
                                .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If

                    If TitleTextEdit.EditValue IsNot Nothing AndAlso CDec(TitleTextEdit.EditValue) > 0 Then
                        sql = "INSERT INTO tblTitlePay (ID,unitId,amountPaid,payDate,memberType,memberId,nonMember,paymethodId,bankId,payReference,projectId,deducted)"
                        sql &= " VALUES(@id,@uid,@amount,@date,@type,@member,@nonm,@payid,@bid,@ref,@pid,@deducted)"
                        Using cmd As New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = TitlePayID
                                .Parameters.Add(New SqlParameter("@uid", SqlDbType.Int)).Value = CInt(UnitLookUpEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@amount", SqlDbType.Decimal)).Value = CDec(TitleTextEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = CDate(SaleDateEdit.EditValue).Date
                                .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = MemberNonMemberComboBoxEdit.SelectedIndex + 1
                                If MemberNonMemberComboBoxEdit.SelectedIndex = 0 Then
                                    .Parameters.Add(New SqlParameter("@member", SqlDbType.Int)).Value = MemberSearchLookUpEdit.EditValue
                                Else
                                    .Parameters.Add(New SqlParameter("@nonm", SqlDbType.VarChar)).Value = CStr(NoneMemberNameTextEdit.EditValue).ToLower
                                End If
                                .Parameters.Add(New SqlParameter("@payid", SqlDbType.Int)).Value = PaymethodLookUpEdit.EditValue
                                If BankLookUpEdit.EditValue IsNot Nothing Then
                                    .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = BankLookUpEdit.EditValue
                                Else
                                    .Parameters.Add(New SqlParameter("@bid", SqlDbType.Int)).Value = DBNull.Value
                                End If
                                .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = ReferenceTextEdit.EditValue.ToString.ToLower
                                .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = CInt(ProjectLookUpEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@deducted", SqlDbType.Bit)).Value = DeductionTypeComboBoxEdit.SelectedIndex < 3
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If


                    If PaymethodLookUpEdit.EditValue = 1 AndAlso DeductionTypeComboBoxEdit.SelectedIndex = 3 Then
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "cash ccount", "Project Payment", TotalDeductions, 0, Nothing, 2, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "unit payment", "Unit Payment", 0, TotalDeductions, Nothing, 5, 1, ID)
                    ElseIf PaymethodLookUpEdit.EditValue > 1 AndAlso PaymethodLookUpEdit.EditValue < 5 AndAlso DeductionTypeComboBoxEdit.SelectedIndex = 3 Then
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "bank ccount", "Project Payment", TotalDeductions, 0, Nothing, 2, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "unit payment", "Unit Payment", 0, TotalDeductions, Nothing, 5, 1, ID)
                    ElseIf DeductionTypeComboBoxEdit.SelectedIndex = 2 Then
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "deposits", "Title Payment", CDec(TitleTextEdit.EditValue), 0, Nothing, 3, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "title fees", "Unit Payment", 0, CDec(TitleTextEdit.EditValue), Nothing, 5, 1, ID)
                        If PaymethodLookUpEdit.EditValue = 1 Then
                            AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "cash ccount", "Project Payment", CDec(AmountPaidUnitTextEdit.EditValue), 0, Nothing, 2, 1, ID)
                            AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "unit payment", "Unit Payment", 0, CDec(AmountPaidUnitTextEdit.EditValue), Nothing, 5, 1, ID)
                        ElseIf PaymethodLookUpEdit.EditValue > 1 AndAlso PaymethodLookUpEdit.EditValue < 5 Then
                            AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "bank ccount", "Project Payment", CDec(AmountPaidUnitTextEdit.EditValue), 0, Nothing, 2, 1, ID)
                            AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "unit payment", "Unit Payment", 0, CDec(AmountPaidUnitTextEdit.EditValue), Nothing, 5, 1, ID)
                        End If
                    ElseIf DeductionTypeComboBoxEdit.SelectedIndex = 1 Then
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "deposits", "Unit Payment", CDec(AmountPaidUnitTextEdit.EditValue), 0, Nothing, 3, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "unit payment", "Unit Payment", 0, CDec(AmountPaidUnitTextEdit.EditValue), Nothing, 5, 1, ID)
                        If TitleTextEdit.EditValue IsNot Nothing AndAlso CDec(TitleTextEdit.EditValue) > 0 Then
                            If PaymethodLookUpEdit.EditValue = 1 Then
                                AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "cash ccount", "Title Payment", CDec(TitleTextEdit.EditValue), 0, Nothing, 2, 1, ID)
                                AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "title fees", "Title Payment", 0, CDec(TitleTextEdit.EditValue), Nothing, 5, 1, ID)
                            ElseIf PaymethodLookUpEdit.EditValue > 1 AndAlso PaymethodLookUpEdit.EditValue < 5 Then
                                AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "bank ccount", "Project Payment", CDec(TitleTextEdit.EditValue), 0, Nothing, 2, 1, ID)
                                AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "title fees", "Title Payment", 0, CDec(TitleTextEdit.EditValue), Nothing, 5, 1, ID)
                            End If
                        End If
                    ElseIf DeductionTypeComboBoxEdit.SelectedIndex = 0 Then
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "deposits", "Unit Payment", CDec(AmountPaidUnitTextEdit.EditValue), 0, Nothing, 3, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "unit payment", "Unit Payment", 0, CDec(AmountPaidUnitTextEdit.EditValue), Nothing, 5, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "deposits", "Title Payment", CDec(TitleTextEdit.EditValue), 0, Nothing, 3, 1, ID)
                        AppClass.AddToLedger(CDate(SaleDateEdit.EditValue).Date, "title fees", "Unit Payment", 0, CDec(TitleTextEdit.EditValue), Nothing, 5, 1, ID)
                    End If

                    If BankRequired AndAlso DeductionTypeComboBoxEdit.SelectedIndex = 3 Then
                        AppClass.BankPosting(CDate(SaleDateEdit.EditValue).Date, BankLookUpEdit.EditValue, TotalDeductions, 0, "Units Payment-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                            , PaymethodLookUpEdit.EditValue, False, 1, ID)
                    ElseIf BankRequired AndAlso DeductionTypeComboBoxEdit.SelectedIndex = 2 Then
                        AppClass.BankPosting(CDate(SaleDateEdit.EditValue).Date, BankLookUpEdit.EditValue, AmountPaidUnitTextEdit.EditValue, 0, "Units Payment-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                           , PaymethodLookUpEdit.EditValue, False, 1, ID)
                    ElseIf BankRequired AndAlso DeductionTypeComboBoxEdit.SelectedIndex = 1 AndAlso CDec(TitleTextEdit.EditValue) > 0 Then
                        AppClass.BankPosting(CDate(SaleDateEdit.EditValue).Date, BankLookUpEdit.EditValue, TitleTextEdit.EditValue, 0, "Title Payment-" & MemberSearchLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                           , PaymethodLookUpEdit.EditValue, False, 1, ID)
                    End If


                    MyTransaction.Commit()
                    AppClass.ShowNotification("Edited Successfully")
                    If AppClass.AlertQuestion("Do You Wish To Print A Receipt For This Transaction?") = System.Windows.Forms.DialogResult.Yes Then
                        ReportStuff(False)
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
    Sub Delete()
        Using connection As New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Using MyTransaction As SqlTransaction = connection.BeginTransaction
                Try

                    sql = "DELETE FROM tblUnitSale WHERE ID=@id"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "UPDATE tblUnits SET sold=@sold,soldBy=@by,soldDate=@sdate,unitPaid=@paid,memberNo=@member,nonMemberName=@name,nonMemberID=@idno,nonMemberContact=@contact "
                    sql &= "WHERE ID=@id"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@sold", SqlDbType.Bit)).Value = 0
                            .Parameters.Add(New SqlParameter("@by", SqlDbType.Int)).Value = DBNull.Value
                            .Parameters.Add(New SqlParameter("@sdate", SqlDbType.Date)).Value = DBNull.Value
                            .Parameters.Add(New SqlParameter("@paid", SqlDbType.Int)).Value = DBNull.Value
                            .Parameters.Add(New SqlParameter("@member", SqlDbType.Int)).Value = DBNull.Value
                            .Parameters.Add(New SqlParameter("@name", SqlDbType.VarChar)).Value = DBNull.Value
                            .Parameters.Add(New SqlParameter("@idno", SqlDbType.VarChar)).Value = DBNull.Value
                            .Parameters.Add(New SqlParameter("@contact", SqlDbType.VarChar)).Value = DBNull.Value
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = UnitLookUpEdit.EditValue
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "DELETE FROM tblUnitPayment WHERE transactionType=1 AND transactionId=@tid"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "DELETE FROM tblDepositDeduction WHERE transactionType=1 AND transactionId=@tid"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    If InitialTitleID > 0 Then
                        sql = "DELETE FROM tblTitlePay WHERE ID=@id"
                        Using cmd As New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = InitialTitleID
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If

                    sql = "DELETE FROM tblLedger WHERE transactionType=1 AND transactionID=@tid"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "DELETE FROM tblBankPostings WHERE transactionType=1 AND transactionID=@tid"
                    Using cmd As New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

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
    Private Sub DeductionTypeComboBoxEdit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles DeductionTypeComboBoxEdit.SelectedIndexChanged
        If DeductionTypeComboBoxEdit.SelectedIndex = 0 Then
            With PaymethodLookUpEdit
                .Properties.ReadOnly = True
                .EditValue = 5
            End With
        Else
            With PaymethodLookUpEdit
                .Properties.ReadOnly = False
                .EditValue = Nothing
            End With
        End If
    End Sub
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
    Private Sub ReferalCheckEdit_CheckedChanged(sender As Object, e As EventArgs) Handles ReferalCheckEdit.CheckedChanged
        RefeeringMemberSearchLookUpEdit.Properties.ReadOnly = Not ReferalCheckEdit.Checked
        RefeeringMemberSearchLookUpEdit.EditValue = Nothing
    End Sub
    Private Sub MemberNonMemberComboBoxEdit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles MemberNonMemberComboBoxEdit.SelectedIndexChanged
        NoneMemberNameTextEdit.Properties.ReadOnly = MemberNonMemberComboBoxEdit.SelectedIndex < 1
        ContactTextEdit.Properties.ReadOnly = MemberNonMemberComboBoxEdit.SelectedIndex < 1
        IDNoTextEdit.Properties.ReadOnly = MemberNonMemberComboBoxEdit.SelectedIndex < 1
        NoneMemberNameTextEdit.EditValue = Nothing
        ContactTextEdit.EditValue = Nothing
        IDNoTextEdit.EditValue = Nothing
        MemberSearchLookUpEdit.Properties.ReadOnly = MemberNonMemberComboBoxEdit.SelectedIndex > 0
        MemberSearchLookUpEdit.EditValue = Nothing
        'With PlanComboBoxEdit
        '    .EditValue = Nothing
        '    .Properties.ReadOnly = MemberNonMemberComboBoxEdit.SelectedIndex = 1
        'End With
        With DeductionTypeComboBoxEdit
            .SelectedIndex = 3
            .Properties.ReadOnly = MemberNonMemberComboBoxEdit.SelectedIndex = 1
        End With
        GetUnitPrice()
    End Sub
    Private Sub ProjectLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles ProjectLookUpEdit.EditValueChanged
        If ProjectLookUpEdit.EditValue IsNot Nothing Then
            sql = "SELECT ID,UPPER(unitName) AS UnitName FROM tblUnits "
            sql &= "WHERE projectId=@pid "
            If Not IsEdit Then
                sql &= "AND sold=0 AND archived=0 AND ID NOT IN (SELECT unitId FROM tblReservations WHERE projectId=@pid AND (released=0 OR reserveTill >= @today))"
            End If
            AddParams("@pid", CInt(ProjectLookUpEdit.EditValue))
            If Not IsEdit Then
                AddParams("@today", Date.Now.Date)
            End If
            AppClass.LoadToLookUpEdit(sql, UnitLookUpEdit, "UnitName", "ID")
            AddParams("@id", CInt(ProjectLookUpEdit.EditValue))
            sql = "SELECT titleFeeInclusive FROM tblProjects WHERE ID=@id"
                Dim TitleFeeInclusive As Boolean = CBool(AppClass.FetchDBValue(sql))
                With TitleTextEdit
                    .EditValue = Nothing
                    .Properties.ReadOnly = TitleFeeInclusive
                End With
            End If
    End Sub
    Private Sub DiscountCheckEdit_CheckedChanged(sender As Object, e As EventArgs) Handles DiscountCheckEdit.CheckedChanged
        DiscountTypeComboBoxEdit.Properties.ReadOnly = Not DiscountCheckEdit.Checked
        DiscountValueTextEdit.Properties.ReadOnly = Not DiscountCheckEdit.Checked
        DiscountValueTextEdit.EditValue = Nothing
        DiscountTypeComboBoxEdit.EditValue = Nothing
        NetTextEdit.EditValue = CalculateNetAmount().ToString("N")
    End Sub
    Private Sub UnitLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles UnitLookUpEdit.EditValueChanged, PlanComboBoxEdit.SelectedIndexChanged
        If UnitLookUpEdit.EditValue IsNot Nothing Then
            If MemberNonMemberComboBoxEdit.SelectedIndex = 0 Then
                GetUnitPrice()
            End If
        End If
    End Sub
    Private Sub DiscountValueTextEdit_Leave(sender As Object, e As EventArgs) Handles DiscountValueTextEdit.Leave
        NetTextEdit.EditValue = CalculateNetAmount().ToString("N")
    End Sub
    Private Sub DiscountTypeComboBoxEdit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles DiscountTypeComboBoxEdit.SelectedIndexChanged
        If DiscountTypeComboBoxEdit.EditValue IsNot Nothing AndAlso DiscountValueTextEdit.EditValue IsNot Nothing Then
            NetTextEdit.EditValue = CalculateNetAmount().ToString("N")
        End If
    End Sub
    Private Sub PaymethodLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles PaymethodLookUpEdit.EditValueChanged
        If PaymethodLookUpEdit.EditValue IsNot Nothing Then
            AddParams("@id", CInt(PaymethodLookUpEdit.EditValue))
            BankRequired = CBool(AppClass.FetchDBValue("SELECT bankRequired FROM tblPaymentMethod WHERE ID=@id"))
            BankLookUpEdit.Properties.ReadOnly = Not BankRequired
        Else
            BankLookUpEdit.Properties.ReadOnly = False
        End If
        BankLookUpEdit.EditValue = Nothing
    End Sub
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If Not Datavalidation() Then
            Return
        End If

        Dim TotalAmount As Decimal = CDec(TitleTextEdit.EditValue) + CDec(AmountPaidUnitTextEdit.EditValue)

        If MemberNonMemberComboBoxEdit.SelectedIndex = 0 Then
            AddParams("@memberno", MemberSearchLookUpEdit.EditValue)
            AddParams("@today", CDate(SaleDateEdit.EditValue).Date)
            Dim MemberDeposits As Decimal = CDec(AppClass.FetchDBValue("SELECT dbo.fnGetMemberDeposits(@memberno,@today)"))
            If DeductionTypeComboBoxEdit.SelectedIndex = 0 AndAlso (MemberDeposits + InitialAmountDeductedFromDeposits) < TotalAmount Then
                AppClass.ShowError("Member deposit is insufficient for the selected deductions")
                Return
            End If
            If DeductionTypeComboBoxEdit.SelectedIndex = 1 AndAlso (MemberDeposits + InitialAmountDeductedFromDeposits) < CDec(AmountPaidUnitTextEdit.EditValue) Then
                AppClass.ShowError("Member deposit is insufficient for the selected deductions")
                Return
            End If
            If DeductionTypeComboBoxEdit.SelectedIndex = 2 AndAlso (MemberDeposits + InitialAmountDeductedFromDeposits) < CDec(TitleTextEdit.EditValue) Then
                AppClass.ShowError("Member deposit is insufficient for the selected deductions")
                Return
            End If
        End If
        If Not IsEdit Then
            Save()
        Else
            Edit()
        End If
    End Sub
    Private Sub FindSimpleButton_Click(sender As Object, e As EventArgs) Handles FindSimpleButton.Click
        Dim SearchBy As String = "Search by project name,unit no,buyer name, payment reference"
        Using frm As New SearchForm(SearchBy, "spSearchUnitSales")
            If frm.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                Find(frm.DataGridView.CurrentRow.Cells(0).Value)
            End If
        End Using
    End Sub
    Private Sub ReprintSimpleButton_Click(sender As Object, e As EventArgs) Handles ReprintSimpleButton.Click
        ReportStuff()
    End Sub
    Private Sub DeleteSimpleButton_Click(sender As Object, e As EventArgs) Handles DeleteSimpleButton.Click
        If AppClass.AlertQuestion("Are you sure you want to delete this sale?") = DialogResult.Yes Then
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
        AddParams("@no", MemberSearchLookUpEdit.EditValue)
        Dim idNo As String = CStr(AppClass.FetchDBValue("SELECT ISNULL(idNo,'N/A') AS isNo FROM tblMember WHERE (memberNo=@no)"))
        AddParams("@no", MemberSearchLookUpEdit.EditValue)
        Dim phoneNo As String = CStr(AppClass.FetchDBValue("SELECT ISNULL(contact,'N/A') AS contact FROM tblMemberAddress WHERE (memberNo=@no)"))
        AddParams("@no", MemberSearchLookUpEdit.EditValue)
        Dim memberId As String = CStr(AppClass.FetchDBValue("SELECT ISNULL(memberID,'N/A') AS memberID FROM tblMember WHERE (memberNo=@no)"))
        Dim showBalance As Boolean = True

        Dim balance As Decimal = CDec(NetTextEdit.EditValue) - CDec(AmountPaidUnitTextEdit.EditValue)
        Dim TotalAmount As Decimal = CDec(TitleTextEdit.EditValue) + CDec(AmountPaidUnitTextEdit.EditValue)


        Dim Report As New LocalReport
        Dim myparam As ReportParameter
        Dim myParams As New List(Of ReportParameter)
        myparam = New ReportParameter("prmReceivedFrom", CStr(MemberSearchLookUpEdit.Text.ToString))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmDate", CDate(SaleDateEdit.EditValue).ToString("dd-MM-yy"))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmNo", ID)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmIdNo", CStr(idNo.ToUpper.ToString))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmPhoneNo", CStr(phoneNo).ToUpper.ToString)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmPaymentMethod", CStr(PaymethodLookUpEdit.Text).ToUpper.ToString)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmBalance", CDec(balance))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmPrintedBy", CStr(postby).ToUpper.ToString)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmAmountInWords", CStr(NumberToText(CInt(TotalAmount)).ToUpper.ToString & " ONLY"))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmReference", CStr(ReferenceTextEdit.EditValue).ToUpper.ToString)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmMemberNo", CStr(memberId).ToUpper.ToString)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmShowBalance", showBalance)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmShowStamp", ToDownLoad)
        myParams.Add(myparam)

        Report.ReportEmbeddedResource = "phcsl.rptUnitSaleReceipt.rdlc"
        Report.SetParameters(myParams)

        ds = New DSApp
        dt = New DSApp.unitsaleDataTable
        dt.Rows.Add("Payment For Project " & ProjectLookUpEdit.Text & " Plot " & UnitLookUpEdit.Text, TotalAmount)


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

        'Dim FileName As String = Nothing
        'If ToDownLoad Then
        '    Dim SFD As New XtraFolderBrowserDialog
        '    With SFD
        '        If .ShowDialog = DialogResult.OK Then
        '            FileName = .SelectedPath + "\" + ReceiptNoTextEdit.EditValue & ".pdf"
        '        End If
        '    End With
        '    SaveToPdf(Report, FileName)
        'End If
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
                    cmd.Parameters.Add(New SqlParameter("@no", SqlDbType.Int)).Value = MemberSearchLookUpEdit.EditValue
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
    'Sub SendMail()
    '    If Not CheckForInternetConnection() Then
    '        XtraMessageBox.Show("Internet Is Required To Send The Email", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information)
    '        Exit Sub
    '    End If
    '    Dim email, passwd As String
    '    Dim decryptedPwd As String
    '    If CInt(AppClass.FetchDBValue("SELECT COUNT(ID) FROM tblEmailSetting")) > 0 Then
    '        email = CStr(AppClass.FetchDBValue("SELECT email FROM tblEmailSetting")).Trim
    '        passwd = CStr(AppClass.FetchDBValue("SELECT emailPwd FROM tblEmailSetting"))
    '        decryptedPwd = CStr(AppClass.Decrypt(passwd)).Trim
    '    Else
    '        AppClass.ShowNotification("Email Credentials Not Set")
    '        Exit Sub
    '    End If
    '    AddParams("@no", MemberNo)
    '    Dim emailTo As String = CStr(AppClass.FetchDBValue("SELECT email FROM tblMemberAddress WHERE memberNo=@no")).Trim
    '    Try
    '        Dim spath As String = Application.StartupPath & "\Receipts\" & ReceiptNoTextEdit.EditValue.ToString & ".pdf"
    '        Dim Smtp_Server As New SmtpClient
    '        Dim e_mail As New MailMessage()
    '        Smtp_Server.UseDefaultCredentials = False
    '        Smtp_Server.Credentials = New Net.NetworkCredential(email, decryptedPwd)
    '        Smtp_Server.Port = 587
    '        Smtp_Server.EnableSsl = True
    '        Smtp_Server.Host = "smtp.gmail.com"

    '        Dim messageBody As String = "Attached Find Receipt from P.C.E.A Housing Cooperative"

    '        e_mail = New MailMessage With {
    '            .From = New MailAddress(email)
    '        }
    '        e_mail.To.Add(emailTo)
    '        e_mail.Subject = "Receipt From PCEA Housing - " & ReceiptNoTextEdit.EditValue.ToString
    '        e_mail.IsBodyHtml = False
    '        e_mail.Body = messageBody
    '        If FileExists(spath) Then
    '            Dim attach As Attachment
    '            attach = New Attachment(spath)
    '            e_mail.Attachments.Add(attach)
    '        End If

    '        Smtp_Server.Send(e_mail)

    '    Catch ex As Exception
    '        AppClass.ShowError(ex.Message)
    '    End Try
    'End Sub
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
