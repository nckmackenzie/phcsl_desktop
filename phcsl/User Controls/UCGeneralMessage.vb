Imports DevExpress.XtraEditors.Controls

Public Class UCGeneralMessage
    Private Shared _instance As UCGeneralMessage
    Public Shared ReadOnly Property Instance As UCGeneralMessage
        Get
            If _instance Is Nothing Then _instance = New UCGeneralMessage()
            Return _instance
        End Get
    End Property
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Reset()
    End Sub
    Sub Reset()
        AppClass.ClearItems(LayoutControl1)
        sql = "SELECT 
                contact,
                UPPER(memberName) AS memberName
               FROM tblMember m join tblMemberAddress a on m.memberNo = a.memberNo
               WHERE (m.memberStatusId = 1) 
               ORDER BY memberName"
        AppClass.LoadToControl(sql, MembersCheckedComboBoxEdit, "tblMember", "memberName", "contact")
    End Sub
    Function GetSelectedCount() As Int16
        Dim Count As Int16 = 0
        For Each item As CheckedListBoxItem In MembersCheckedComboBoxEdit.Properties.GetItems()
            If item.CheckState = CheckState.Checked Then
                Count += 1
            End If
        Next
        Return Count
    End Function
    Function GetFormattedContactList() As String
        Dim FormattedContacts As New List(Of String)

        For Each item As CheckedListBoxItem In MembersCheckedComboBoxEdit.Properties.GetItems()
            If item.CheckState = CheckState.Checked Then
                Dim Contact As String = item.Value.ToString()
                Dim RemovedZero As String = Contact.Remove(0, 1)
                Dim FormattedContact As String = "+254" & RemovedZero
                FormattedContacts.Add(FormattedContact)
            End If
        Next

        Return String.Join(",", FormattedContacts.ToArray())
    End Function
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If GetSelectedCount() = 0 Then
            AppClass.ShowNotification("No member selected")
            Exit Sub
        End If
        If MessageMemoEdit.EditValue Is Nothing Then
            AppClass.ShowNotification("Enter message")
            Exit Sub
        End If

        Dim TotalSent As String = AppClass.SendBulkSMS(GetFormattedContactList(), MessageMemoEdit.EditValue)
        ' AppClass.ShowNotification(TotalSent)
    End Sub
End Class
