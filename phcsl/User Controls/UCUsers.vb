Public Class UCUsers
    Private ID As Integer
    Private IsEdit As Boolean
    Private Shared _instance As UCUsers
    Public Shared ReadOnly Property Instance As UCUsers
        Get
            If _instance Is Nothing Then _instance = New UCUsers()
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
        With ActiveCheckEdit
            .Checked = True
            .Enabled = False
        End With
        UserTypeComboBoxEdit.SelectedIndex = 1
        IsEdit = False
        DeleteSimpleButton.Enabled = False
    End Sub
    Function Datavalidation() As Boolean
        errmsg = ""
        If String.IsNullOrEmpty(UserIdTextEdit.EditValue) Then
            errmsg = "Enter User ID"
            UserIdTextEdit.Focus()
        ElseIf String.IsNullOrEmpty(UserNameTextEdit.EditValue) Then
            errmsg = "Enter User Name"
            UserNameTextEdit.Focus()
        ElseIf String.IsNullOrEmpty(ContactTextEdit.EditValue) Then
            errmsg = "Enter Contact"
            ContactTextEdit.Focus()
        ElseIf Not IsEdit AndAlso String.IsNullOrEmpty(PasswordTextEdit.EditValue) Then
            errmsg = "Enter Password"
            PasswordTextEdit.Focus()
        ElseIf Not IsEdit AndAlso String.IsNullOrEmpty(ConfirmPasswordTextEdit.EditValue) Then
            errmsg = "Confirm Password"
            ConfirmPasswordTextEdit.Focus()
        ElseIf IsEdit AndAlso PasswordTextEdit.EditValue IsNot Nothing AndAlso ConfirmPasswordTextEdit.EditValue Is Nothing Then
            errmsg = "Confirm Password"
            ConfirmPasswordTextEdit.Focus()
        ElseIf IsEdit AndAlso PasswordTextEdit.EditValue Is Nothing AndAlso ConfirmPasswordTextEdit.EditValue IsNot Nothing Then
            errmsg = "Enter Password"
            PasswordTextEdit.Focus()
        ElseIf String.Compare(PasswordTextEdit.EditValue, ConfirmPasswordTextEdit.EditValue, True) <> 0 Then
            errmsg = "Passwords Don't Match"
            With PasswordTextEdit
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
    Sub Save()
        Dim dte As Date = CDate(DateAdd(DateInterval.Month, 1, Date.Now.Date)).Date
        Dim pwd = Encryption.HashString(PasswordTextEdit.EditValue)
        Dim salt = Encryption.GenerateSalt
        Dim hashsalted = Encryption.HashString(String.Format("{0}{1}", pwd, salt))
        AppClass.SaveUser(UserIdTextEdit.EditValue.ToLower.ToString.Trim, UserNameTextEdit.EditValue.ToLower.ToString.Trim, UserTypeComboBoxEdit.SelectedIndex + 1,
                          hashsalted, salt, ContactTextEdit.EditValue.ToString.Trim, ActiveCheckEdit.Checked, dte, False)
        AddParams("@action", "insert")
        AddParams("@userid", LogedUserID)
        AddParams("@activity", "Created New User:" & CStr(UserNameTextEdit.EditValue.ToUpper.ToString))
        AddParams("@actDate", Date.Now.Date)
        AppClass.ExecSP("spLogs")
        AppClass.ShowNotification("Saved Successfully!")
    End Sub
    Sub Edit()
        If Not String.IsNullOrEmpty(PasswordTextEdit.EditValue) Then
            Dim pwd = Encryption.HashString(PasswordTextEdit.EditValue)
            Dim salt = Encryption.GenerateSalt
            Dim hashsalted = Encryption.HashString(String.Format("{0}{1}", pwd, salt))
            AddParams("@action", "updatepwd")
            AddParams("@username", Trim(UserNameTextEdit.EditValue.ToLower.ToString))
            AddParams("@userTypeId", UserTypeComboBoxEdit.SelectedIndex + 1)
            AddParams("@pwd", hashsalted)
            AddParams("@hash", salt)
            AddParams("@contact", ContactTextEdit.EditValue)
            AddParams("@active", ActiveCheckEdit.Checked)
            AddParams("@promptpwdchange", False)
            AddParams("@ID", ID)
            AppClass.ExecSP("spCrudUsers")
            AppClass.ShowNotification("Edited Successfully!")
        Else
            AddParams("@action", "update")
            AddParams("@username", Trim(UserNameTextEdit.EditValue.ToLower.ToString.Trim))
            AddParams("@userTypeId", UserTypeComboBoxEdit.SelectedIndex + 1)
            AddParams("@contact", ContactTextEdit.EditValue)
            AddParams("@active", ActiveCheckEdit.Checked)
            AddParams("@promptpwdchange", False)
            AddParams("@ID", ID)
            AppClass.ExecSP("spCrudUsers")
            AppClass.ShowNotification("Edited Successfully!")
        End If
    End Sub
#End Region
#Region "EVENTS"
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If Not Datavalidation() Then
            Exit Sub
        End If

        AddParams("@uid", UserIdTextEdit.EditValue.ToString.Trim)
        AddParams("@id", ID)
        If CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblUsers WHERE userID=@uid AND ID <> @id")) Then
            AppClass.ShowNotification("UserID Already Exists!")
            With UserIdTextEdit
                .Focus()
                .SelectAll()
            End With
            Exit Sub
        End If

        If Not IsEdit Then
            Save()
        Else
            Edit()
        End If
        Reset()
    End Sub
    Private Sub FindSimpleButton_Click(sender As Object, e As EventArgs) Handles FindSimpleButton.Click
        SearchString = "Search by name, userid or contact..."
        Using frm As New SearchForm(SearchString, "spSearchUsers")
            sql = "SELECT * FROM tblUsers WHERE ID=@id"
            If frm.ShowDialog() = DialogResult.OK Then
                Dim FindDt As DataTable = AppClass.Find(CInt(frm.DataGridView.CurrentRow.Cells(0).Value), sql)
                ID = CInt(FindDt.Rows(0)(0))
                UserIdTextEdit.EditValue = AppClass.TitleCase(FindDt.Rows(0)(1))
                UserNameTextEdit.EditValue = AppClass.TitleCase(FindDt.Rows(0)(2))
                UserTypeComboBoxEdit.SelectedIndex = CInt(FindDt.Rows(0)(3)) - 1
                ContactTextEdit.EditValue = FindDt.Rows(0)(6)
                With ActiveCheckEdit
                    .Enabled = True
                    .Checked = CBool(FindDt.Rows(0)(7))
                End With
                IsEdit = True
                DeleteSimpleButton.Enabled = True
            End If
        End Using
    End Sub
    Private Sub DeleteSimpleButton_Click(sender As Object, e As EventArgs) Handles DeleteSimpleButton.Click
        If AppClass.AlertQuestion("Are you sure you want to delete this user?") = DialogResult.Yes Then
            AddParams("@uid", ID)
            If CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblLogs WHERE userID=@uid")) > 0 Then
                AppClass.ShowNotification("Cannot Delete as user referenced elsewhere")
                Exit Sub
            End If
            AddParams("@uid", ID)
            If CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblIncomeHeader WHERE postBy=@uid")) > 0 Then
                AppClass.ShowNotification("Cannot Delete as user referenced elsewhere")
                Exit Sub
            End If

            AddParams("@action", "delete")
            AddParams("@ID", ID)
            AppClass.ExecSP("spCrudUsers")
            AppClass.ShowNotification("Deleted Successfully!")
            Reset()
        End If
    End Sub
#End Region
End Class
