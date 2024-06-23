Public Class UCPaymentReminders
    Private Shared _instance As UCPaymentReminders
    Public Shared ReadOnly Property Instance As UCPaymentReminders
        Get
            If _instance Is Nothing Then _instance = New UCPaymentReminders()
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
        AppClass.FormatDatagridView(DataGridView)
        LoadData()
    End Sub
    Sub LoadData()
        DataGridView.Rows.Clear()
        dt = New DataTable
        dt = AppClass.LoadToDatatable("sp_overdue_with_contact", True)
        For Each row In dt.Rows
            Populate(row(0), row(1), row(2), row(3), row(4), row(5), "Preview message")
        Next
    End Sub
    Sub Populate(check As Boolean, unit As String, project As String, soldto As String, contact As String, bal As Decimal, Message As String)
        Dim row As String() = New String() {check, unit, project, soldto, contact, bal.ToString("N"), Message}
        DataGridView.Rows.Add(row)
    End Sub
    Function CapitalizeFirstWord(str As String)
        Return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str)
    End Function
    Function Message(Name As String, Bal As Decimal, Project As String, Unit As String) As String
        Dim Msg As String = Name & ", kindly make payments towards your outstanding balance of Ksh. " & CDec(Bal).ToString("N") _
            & " for " & Project.ToString & " unit " & Unit
        Return Msg
    End Function
#End Region
#Region "EVENTS"
    Private Sub SelectAllSimpleButton_Click(sender As Object, e As EventArgs) Handles SelectAllSimpleButton.Click
        Select Case SelectAllSimpleButton.Text
            Case "Select All"
                For Each row As DataGridViewRow In DataGridView.Rows
                    row.Cells(0).Value = True
                Next
                SelectAllSimpleButton.Text = "Deselect All"
            Case "Deselect All"
                For Each row As DataGridViewRow In DataGridView.Rows
                    row.Cells(0).Value = False
                Next
                SelectAllSimpleButton.Text = "Select All"
        End Select
    End Sub
    Private Sub DataGridView_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView.CellClick
        If e.ColumnIndex = 6 Then
            Dim Project As String = CapitalizeFirstWord(DataGridView.CurrentRow.Cells(2).Value.ToString.ToLower)
            Dim Unit As String = DataGridView.CurrentRow.Cells(1).Value
            Dim Balance As Decimal = CDec(DataGridView.CurrentRow.Cells(5).Value)
            Dim Name As String = CapitalizeFirstWord(AppClass.GetFirstName(DataGridView.CurrentRow.Cells(3).Value.ToString.ToLower))
            Dim FullMsg As String = Message(Name, Balance, Project, Unit)
            AppClass.ShowNotification(FullMsg)
        End If
    End Sub
    Private Sub SendSimpleButton_Click(sender As Object, e As EventArgs) Handles SendSimpleButton.Click
        Dim count As String = 0
        For Each row As DataGridViewRow In DataGridView.Rows
            If (Convert.ToBoolean(row.Cells(0).Value)) = True Then
                count += 1
            End If
        Next
        If count = 0 Then
            AppClass.ShowNotification("Select at least one member")
            Exit Sub
        End If
        SendSimpleButton.Enabled = False
        SendSimpleButton.Text = "Sending..."
        For Each row As DataGridViewRow In DataGridView.Rows
            If (Convert.ToBoolean(row.Cells(0).Value)) = True Then
                Dim Project As String = CapitalizeFirstWord(row.Cells(2).Value.ToString.ToLower)
                Dim Unit As String = row.Cells(1).Value
                Dim Balance As Decimal = CDec(row.Cells(5).Value)
                Dim Name As String = CapitalizeFirstWord(AppClass.GetFirstName(row.Cells(3).Value.ToString.ToLower))
                Dim Contact As String = row.Cells(4).Value.ToString.Trim
                Dim MessageToBeSent As String = Message(Name, Balance, Project, Unit)
                AppClass.SendMessage(Contact, MessageToBeSent)
            End If
        Next
        SendSimpleButton.Enabled = True
        SendSimpleButton.Text = "Send To Selected"
    End Sub
#End Region
End Class
