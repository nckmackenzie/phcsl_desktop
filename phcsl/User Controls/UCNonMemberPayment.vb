Imports System.Reflection
Imports System.Data.SqlClient
Imports System.Data.Common
Imports System.IO
Imports System.Text
Imports System.Drawing.Printing
Imports System.Drawing.Imaging
Imports Microsoft.Reporting.WinForms
Imports System.Net.Mail
Imports System.Net
Imports DevExpress.XtraEditors
Imports DevExpress.Utils.Extensions
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox
Public Class UCNonMemberPayment
    Private Shared _instance As UCNonMemberPayment
    Private IsEdit As Boolean
    Private BankRequired As Boolean
    Private ID As Integer
    Private Shared M_streams As List(Of Stream)
    Private Shared M_currentPageIndex As Integer = 0
    Public Shared ReadOnly Property Instance As UCNonMemberPayment
        Get
            If _instance Is Nothing Then _instance = New UCNonMemberPayment()
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
        AppClass.ClearItems(LayoutControl1)
        BankLookUpEdit.Properties.ReadOnly = True
        AppClass.LoadToLookUpEdit("SELECT ID,methodName FROM tblPaymentMethod WHERE ID < 5", PaymentMethodLookUpEdit, "methodName", "ID")
        AddParams("@true", True)
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(bankName) AS bankName FROM tblBanks WHERE active=@true", BankLookUpEdit, "bankName", "ID")
        sql = "SELECT DISTINCT upper(nonMemberName) as NonMemberName FROM tblUnits WHERE nonMemberName IS NOT NULL"
        AppClass.LoadToLookUpEdit(sql, NonMemberLookUpEdit, "NonMemberName", "NonMemberName")
        AppClass.GenerateID("SELECT MAX(RIGHT(ReceiptNo,5)) As receiptNo FROM tblNonMemberUnitPayments", "receiptNo", ReceiptNoTextEdit, "00001", "00000")
        DeleteSimpleButton.Enabled = False
        ReprintSimpleButton.Enabled = False
        ProjectLookUpEdit.Properties.ReadOnly = False
        UnitLookUpEdit.Properties.ReadOnly = False
        LayoutControlItem1.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always
        LayoutControlItem11.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        ID = Nothing
        IsEdit = False
    End Sub
    Function Datavalidation() As Boolean
        errmsg = Nothing
        If PaymentDateEdit.EditValue Is Nothing Then
            errmsg = "Select payment date"
            PaymentDateEdit.Focus()
        ElseIf MemberNameTextEdit.EditValue Is Nothing Then
            errmsg = "Select non member name"
            NonMemberLookUpEdit.Focus()
        ElseIf ProjectLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select project name"
            ProjectLookUpEdit.Focus()
        ElseIf UnitLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select unit name"
            UnitLookUpEdit.Focus()
        ElseIf PaymentMethodLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select payment method"
            PaymentMethodLookUpEdit.Focus()
        ElseIf AmountPaidTextEdit.EditValue Is Nothing Then
            errmsg = "Enter amount"
            AmountPaidTextEdit.Focus()
        ElseIf CDec(AmountPaidTextEdit.EditValue) <= 0 Then
            errmsg = "Enter valid amount"
            AmountPaidTextEdit.Focus()
        ElseIf BankRequired AndAlso BankLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select bank"
            BankLookUpEdit.Focus()
        End If
        If errmsg IsNot Nothing Then
            AppClass.ShowError(errmsg)
            Return False
        End If
        If CDec(AmountPaidTextEdit.EditValue) > CDec(BalanceTextEdit.EditValue) Then
            AppClass.ShowError("Amount paid cannot be greater than balance")
            AmountPaidTextEdit.Focus()
            Return False
        End If
        Return True
    End Function
    Sub Save()
        ID = AppClass.GenerateDBID("tblNonMemberUnitPayments")
        AppClass.GenerateID("SELECT MAX(RIGHT(ReceiptNo,5)) As receiptNo FROM tblNonMemberUnitPayments", "receiptNo", ReceiptNoTextEdit, "00001", "00000")
        Using connection As New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Using Transaction As SqlTransaction = connection.BeginTransaction
                Try
                    sql = "INSERT INTO tblNonMemberUnitPayments(ID,ReceiptNo,PaymentDate,NonMemberName,ProjectId,UnitId,BalanceAsOfTime,AmountPaid,PaymentMethodId,BankId,PaymentReference) " &
                          "VALUES(@id,@receiptno,@pdate,@name,@projectId,@unitId,@balance,@amountPaid,@paymentMethodId,@bankId,@reference)"
                    Using cmd As New SqlCommand(sql, connection, Transaction)
                        With cmd.Parameters
                            .AddWithValue("@id", ID)
                            .AddWithValue("@receiptno", ReceiptNoTextEdit.EditValue)
                            .AddWithValue("@pdate", PaymentDateEdit.EditValue)
                            .AddWithValue("@name", MemberNameTextEdit.EditValue.ToString.ToLower)
                            .AddWithValue("@projectId", ProjectLookUpEdit.EditValue)
                            .AddWithValue("@unitId", UnitLookUpEdit.EditValue)
                            .AddWithValue("@balance", CDec(BalanceTextEdit.EditValue))
                            .AddWithValue("@amountPaid", AmountPaidTextEdit.EditValue)
                            .AddWithValue("@paymentMethodId", PaymentMethodLookUpEdit.EditValue)
                            If BankRequired Then
                                .AddWithValue("@bankId", BankLookUpEdit.EditValue)
                            Else
                                .AddWithValue("@bankId", DBNull.Value)
                            End If
                            .AddWithValue("@reference", If(ReferenceTextEdit.EditValue, DBNull.Value))
                        End With
                        cmd.ExecuteNonQuery()
                    End Using

                    sql = "INSERT INTO tblUnitPayment (unitId,openingBal,paymentDate,amountPaid,closingBal,transactionType,transactionID) VALUES(@uid,@obal,@date,@paid,@cbal,@type,@tid)"
                    Using cmd = New SqlCommand(sql, connection, Transaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@uid", SqlDbType.Int).Value = UnitLookUpEdit.EditValue
                            .Parameters.Add("@obal", SqlDbType.Decimal).Value = CDec(BalanceTextEdit.EditValue)
                            .Parameters.Add("@date", SqlDbType.Date).Value = CDate(PaymentDateEdit.EditValue).Date
                            .Parameters.Add("@paid", SqlDbType.Decimal).Value = CDec(AmountPaidTextEdit.EditValue)
                            Dim bal As Decimal = CDec(BalanceTextEdit.EditValue) - CDec(AmountPaidTextEdit.EditValue)
                            .Parameters.Add("@cbal", SqlDbType.Decimal).Value = bal
                            .Parameters.Add("@type", SqlDbType.Int).Value = 20
                            .Parameters.Add("@tid", SqlDbType.Int).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    Transaction.Commit()

                    If PaymentMethodLookUpEdit.EditValue = 1 Then
                        AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "cash account", "Project Payment", CDec(AmountPaidTextEdit.EditValue), 0, ProjectLookUpEdit.EditValue, 2, 20, ID)
                        AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "unit payment", "Project Payment", 0, CDec(AmountPaidTextEdit.EditValue), ProjectLookUpEdit.EditValue, 5, 2, ID)
                    Else
                        AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "bank account", "Project Payment", CDec(AmountPaidTextEdit.EditValue), 0, ProjectLookUpEdit.EditValue, 2, 20, ID)
                        AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "unit payment", "Project Payment", 0, CDec(AmountPaidTextEdit.EditValue), ProjectLookUpEdit.EditValue, 5, 20, ID)
                    End If

                    If PaymentMethodLookUpEdit.EditValue = 3 OrElse PaymentMethodLookUpEdit.EditValue = 4 Then
                        AppClass.BankPosting(CDate(PaymentDateEdit.EditValue).Date, BankLookUpEdit.EditValue, CDec(AmountPaidTextEdit.EditValue), 0, "Project Payment-" & NonMemberLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                            , PaymentMethodLookUpEdit.EditValue, False, 20, ID)
                    End If

                    AppClass.ShowNotification("Saved Successfully!")
                    If AppClass.AlertQuestion("Do You Wish To Print A Receipt For This Transaction?") = System.Windows.Forms.DialogResult.Yes Then
                        If AppClass.AlertQuestion("Do You Wish To Download The Receipt To Your PC?") = System.Windows.Forms.DialogResult.Yes Then
                            ReportStuff(True)
                        Else
                            ReportStuff()
                        End If
                    Else
                        If AppClass.AlertQuestion("Do You Wish To Download The Receipt To Your PC?") = System.Windows.Forms.DialogResult.Yes Then
                            ReportStuff(True, False)
                        End If
                    End If

                    Reset()

                Catch ex As InvalidOperationException
                    AppClass.ShowError(ex.Message)
                    Transaction.Rollback()
                Catch ex As SqlException
                    AppClass.ShowError(ex.Message)
                    Transaction.Rollback()
                Catch ex As Exception
                    AppClass.ShowError(ex.Message)
                    Transaction.Rollback()
                End Try
            End Using
        End Using
    End Sub
    Sub Edit()

        Using connection As New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Using Transaction As SqlTransaction = connection.BeginTransaction
                Try
                    sql = "UPDATE tblNonMemberUnitPayments SET PaymentDate=@pdate,AmountPaid=@amountPaid,PaymentMethodId=@paymentMethodId," &
                          "BankId=@bankId,PaymentReference=@reference WHERE ID=@id"
                    Using cmd As New SqlCommand(sql, connection, Transaction)
                        With cmd.Parameters
                            .Clear()
                            .AddWithValue("@pdate", PaymentDateEdit.EditValue)
                            .AddWithValue("@amountPaid", AmountPaidTextEdit.EditValue)
                            .AddWithValue("@paymentMethodId", PaymentMethodLookUpEdit.EditValue)
                            If BankRequired Then
                                .AddWithValue("@bankId", BankLookUpEdit.EditValue)
                            Else
                                .AddWithValue("@bankId", DBNull.Value)
                            End If
                            .AddWithValue("@reference", If(ReferenceTextEdit.EditValue, DBNull.Value))
                            .AddWithValue("@id", ID)
                        End With
                        cmd.ExecuteNonQuery()
                    End Using

                    sql = "UPDATE tblUnitPayment SET paymentDate=@date,amountPaid=@paid,closingBal=@cbal WHERE transactionType=20 AND transactionID=@id"
                    Using cmd = New SqlCommand(sql, connection, Transaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@date", SqlDbType.Date).Value = CDate(PaymentDateEdit.EditValue).Date
                            .Parameters.Add("@paid", SqlDbType.Decimal).Value = CDec(AmountPaidTextEdit.EditValue)
                            Dim bal As Decimal = CDec(BalanceTextEdit.EditValue) - CDec(AmountPaidTextEdit.EditValue)
                            .Parameters.Add("@cbal", SqlDbType.Decimal).Value = bal
                            .Parameters.Add("@id", SqlDbType.Int).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "DELETE FROM tblLedger WHERE (transactionType=@type) AND (transactionID=@tid)"
                    Using cmd = New SqlCommand(sql, connection, Transaction)
                        cmd.Parameters.Add("@type", SqlDbType.Int).Value = 20
                        cmd.Parameters.Add("@tid", SqlDbType.Int).Value = ID
                        cmd.ExecuteNonQuery()
                    End Using
                    sql = "DELETE FROM tblBankPostings WHERE (transactionType=@type) AND (transactionID=@tid)"
                    Using cmd = New SqlCommand(sql, connection, Transaction)
                        cmd.Parameters.Add("@type", SqlDbType.Int).Value = 20
                        cmd.Parameters.Add("@tid", SqlDbType.Int).Value = ID
                        cmd.ExecuteNonQuery()
                    End Using

                    Transaction.Commit()

                    If PaymentMethodLookUpEdit.EditValue = 1 Then
                        AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "cash account", "Project Payment", CDec(AmountPaidTextEdit.EditValue), 0, ProjectLookUpEdit.EditValue, 2, 20, ID)
                        AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "unit payment", "Project Payment", 0, CDec(AmountPaidTextEdit.EditValue), ProjectLookUpEdit.EditValue, 5, 2, ID)
                    Else
                        AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "bank account", "Project Payment", CDec(AmountPaidTextEdit.EditValue), 0, ProjectLookUpEdit.EditValue, 2, 20, ID)
                        AppClass.AddToLedger(CDate(PaymentDateEdit.EditValue).Date, "unit payment", "Project Payment", 0, CDec(AmountPaidTextEdit.EditValue), ProjectLookUpEdit.EditValue, 5, 20, ID)
                    End If

                    If PaymentMethodLookUpEdit.EditValue = 3 OrElse PaymentMethodLookUpEdit.EditValue = 4 Then
                        AppClass.BankPosting(CDate(PaymentDateEdit.EditValue).Date, BankLookUpEdit.EditValue, CDec(AmountPaidTextEdit.EditValue), 0, "Project Payment-" & NonMemberLookUpEdit.Text.ToString, ReferenceTextEdit.EditValue.ToString _
                                            , PaymentMethodLookUpEdit.EditValue, False, 20, ID)
                    End If

                    AppClass.ShowNotification("Edited Successfully!")
                    If AppClass.AlertQuestion("Do You Wish To Print A Receipt For This Transaction?") = System.Windows.Forms.DialogResult.Yes Then
                        If AppClass.AlertQuestion("Do You Wish To Download The Receipt To Your PC?") = System.Windows.Forms.DialogResult.Yes Then
                            ReportStuff(True)
                        Else
                            ReportStuff()
                        End If
                    Else
                        If AppClass.AlertQuestion("Do You Wish To Download The Receipt To Your PC?") = System.Windows.Forms.DialogResult.Yes Then
                            ReportStuff(True, False)
                        End If
                    End If

                    Reset()

                Catch ex As InvalidOperationException
                    AppClass.ShowError(ex.Message)
                    Transaction.Rollback()
                Catch ex As SqlException
                    AppClass.ShowError(ex.Message)
                    Transaction.Rollback()
                Catch ex As Exception
                    AppClass.ShowError(ex.Message)
                    Transaction.Rollback()
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

            Using Transaction As SqlTransaction = connection.BeginTransaction
                Try
                    sql = "DELETE FROM tblNonMemberUnitPayments WHERE ID=@id"
                    Using cmd As New SqlCommand(sql, connection, Transaction)
                        With cmd.Parameters
                            .Clear()
                            .AddWithValue("@id", ID)
                        End With
                        cmd.ExecuteNonQuery()
                    End Using

                    sql = "DELETE FROM tblUnitPayment WHERE transactionType=20 AND transactionID=@id"
                    Using cmd = New SqlCommand(sql, connection, Transaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@id", SqlDbType.Int).Value = ID
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "DELETE FROM tblLedger WHERE (transactionType=@type) AND (transactionID=@tid)"
                    Using cmd = New SqlCommand(sql, connection, Transaction)
                        cmd.Parameters.Add("@type", SqlDbType.Int).Value = 20
                        cmd.Parameters.Add("@tid", SqlDbType.Int).Value = ID
                        cmd.ExecuteNonQuery()
                    End Using

                    sql = "DELETE FROM tblBankPostings WHERE (transactionType=@type) AND (transactionID=@tid)"
                    Using cmd = New SqlCommand(sql, connection, Transaction)
                        cmd.Parameters.Add("@type", SqlDbType.Int).Value = 20
                        cmd.Parameters.Add("@tid", SqlDbType.Int).Value = ID
                        cmd.ExecuteNonQuery()
                    End Using

                    Transaction.Commit()
                    AppClass.ShowNotification("Deleted Successfully!")
                    Reset()

                Catch ex As InvalidOperationException
                    AppClass.ShowError(ex.Message)
                    Transaction.Rollback()
                Catch ex As SqlException
                    AppClass.ShowError(ex.Message)
                    Transaction.Rollback()
                Catch ex As Exception
                    AppClass.ShowError(ex.Message)
                    Transaction.Rollback()
                End Try
            End Using
        End Using
    End Sub
    Sub Find(fid As Integer)
        IsEdit = True
        AppClass.ClearItems(LayoutControl1)
        AddParams("@id", fid)
        Dim Searchdt As DataTable = AppClass.LoadToDatatable("SELECT * FROM tblNonMemberUnitPayments WHERE ID=@id", False)
        ID = CInt(Searchdt.Rows(0)(0))
        ReceiptNoTextEdit.EditValue = CStr(Searchdt.Rows(0)(1)).ToUpper
        PaymentDateEdit.EditValue = CDate(Searchdt.Rows(0)(2))
        MemberNameTextEdit.EditValue = CStr(Searchdt.Rows(0)(3)).ToUpper
        ProjectLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(4))
        UnitLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(5))
        BalanceTextEdit.EditValue = CDec(Searchdt.Rows(0)(6))
        AmountPaidTextEdit.EditValue = CDec(Searchdt.Rows(0)(7))
        PaymentMethodLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(8))
        If IsDBNull(Searchdt.Rows(0)(9)) Then
            BankLookUpEdit.EditValue = Nothing
        Else
            BankLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(9))
        End If
        ReferenceTextEdit.EditValue = CStr(Searchdt.Rows(0)(10)).ToUpper
        DeleteSimpleButton.Enabled = True
        ReprintSimpleButton.Enabled = True
        ProjectLookUpEdit.Properties.ReadOnly = True
        UnitLookUpEdit.Properties.ReadOnly = True
        LayoutControlItem1.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        LayoutControlItem11.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always
    End Sub
#End Region
#Region "EVENTS"
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
    Private Sub NonMemberLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles NonMemberLookUpEdit.EditValueChanged
        If NonMemberLookUpEdit.EditValue IsNot Nothing Then
            MemberNameTextEdit.EditValue = NonMemberLookUpEdit.EditValue.ToString.ToUpper
            AddParams("@name", NonMemberLookUpEdit.EditValue.ToString.ToLower)
            sql = "SELECT DISTINCT P.ID AS ID,UPPER(P.projectName) AS projectName " &
                                      "FROM   tblUnits AS U INNER JOIN tblProjects AS P ON U.projectId = P.ID INNER JOIN tblUnitSale S ON U.ID=S.unitId " &
                                      "WHERE  (U.nonMemberName = @name) "
            If Not IsEdit Then
                sql &= "  AND (dbo.fnGetPaid(U.ID) < S.netAmount) "
            End If
            AppClass.LoadToLookUpEdit(sql, ProjectLookUpEdit, "projectName", "ID")
        End If
    End Sub
    Private Sub ProjectLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles ProjectLookUpEdit.EditValueChanged
        If ProjectLookUpEdit.EditValue IsNot Nothing Then
            AddParams("@id", ProjectLookUpEdit.EditValue)
            AddParams("@name", MemberNameTextEdit.EditValue.ToString.ToLower)
            AppClass.LoadToLookUpEdit("SELECT ID,UPPER(unitName) AS unitName FROM tblUnits WHERE (projectId=@id) AND (nonMemberName=@name)", UnitLookUpEdit, "unitName", "ID")
        End If
    End Sub
    Private Sub UnitLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles UnitLookUpEdit.EditValueChanged
        If UnitLookUpEdit.EditValue IsNot Nothing Then
            AddParams("@id", UnitLookUpEdit.EditValue)
            BalanceTextEdit.EditValue = CDec(AppClass.FetchDBValue("SELECT dbo.fnGetUnitPaymentBalance(@id) as balance")).ToString("N2")
        End If
    End Sub
    Private Sub PaymentMethodLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles PaymentMethodLookUpEdit.EditValueChanged
        If PaymentMethodLookUpEdit.EditValue IsNot Nothing Then
            AddParams("@id", PaymentMethodLookUpEdit.EditValue)
            BankRequired = CBool(AppClass.FetchDBValue("SELECT bankRequired FROM tblPaymentMethod WHERE ID=@id"))
            With BankLookUpEdit
                .Properties.ReadOnly = Not BankRequired
                .EditValue = Nothing
            End With
        End If
    End Sub
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If Not Datavalidation() Then
            Return
        End If

        If Not IsEdit Then
            Save()
        Else
            Edit()
        End If
    End Sub
    Private Sub FindSimpleButton_Click(sender As Object, e As EventArgs) Handles FindSimpleButton.Click
        Dim SearchBy As String = "Search by project,unit,payment reference or amount"
        Using frm As New SearchForm(SearchBy, "spSearchNonMemberPayment")
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
    Private Sub ReprintSimpleButton_Click(sender As Object, e As EventArgs) Handles ReprintSimpleButton.Click
        If AppClass.AlertQuestion("Do You Wish To Print A Receipt For This Transaction?") = System.Windows.Forms.DialogResult.Yes Then
            If AppClass.AlertQuestion("Do You Wish To Download The Receipt To Your PC?") = System.Windows.Forms.DialogResult.Yes Then
                ReportStuff(True)
            Else
                ReportStuff()
            End If
        Else
            If AppClass.AlertQuestion("Do You Wish To Download The Receipt To Your PC?") = System.Windows.Forms.DialogResult.Yes Then
                ReportStuff(True, False)
            End If
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
        Dim ShowBalance As Boolean = True

        receivedFrom = CStr(MemberNameTextEdit.EditValue.ToString.ToUpper)

        Dim balance As Decimal = CDec(BalanceTextEdit.EditValue) - CDec(AmountPaidTextEdit.EditValue)
        ShowBalance = True

        Dim Report As New LocalReport
        Dim myparam As ReportParameter
        Dim myParams As New List(Of ReportParameter)
        myparam = New ReportParameter("prmReceivedFrom", CStr(receivedFrom))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmDate", CDate(PaymentDateEdit.EditValue).ToString("dd-MM-yy"))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmNo", CInt(ReceiptNoTextEdit.Text))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmPaymentMethod", CStr(PaymentMethodLookUpEdit.Text).ToUpper.ToString)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmPrintedBy", CStr(postby).ToUpper.ToString)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmAmountInWords", CStr(AppClass.NumberToText(CInt(AmountPaidTextEdit.EditValue)).ToUpper.ToString & " ONLY"))
        myParams.Add(myparam)
        myparam = New ReportParameter("prmReference", CStr(ReferenceTextEdit.EditValue).ToUpper.ToString)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmShowBalance", ShowBalance)
        myParams.Add(myparam)
        myparam = New ReportParameter("prmShowStamp", ToDownLoad)
        myParams.Add(myparam)

        Report.ReportEmbeddedResource = "phcsl.NonMemberReceipt.rdlc"
        Report.SetParameters(myParams)

        ds = New DSApp
        dt = New DSApp.unitsaleDataTable

        dt.Rows.Add("Payment For Project " & ProjectLookUpEdit.Text & " Plot " & UnitLookUpEdit.Text, CDec(AmountPaidTextEdit.EditValue))


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
    Public Sub SaveToPdf(ByVal viewer As LocalReport, ByVal savePath As String)
        Dim Bytes() As Byte = viewer.Render("PDF", "", Nothing, Nothing, Nothing, Nothing, Nothing)

        Using Stream As New FileStream(savePath, FileMode.Create)
            Stream.Write(Bytes, 0, Bytes.Length)
        End Using
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
