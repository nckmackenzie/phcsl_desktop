Imports System.Data.SqlClient
Public Class UCChangePassword
    Private Shared _instance As UCChangePassword
    Public Shared ReadOnly Property Instance As UCChangePassword
        Get
            If _instance Is Nothing Then _instance = New UCChangePassword()
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
        AddParams("@id", LogedUserID)
        UserNameTextEdit.EditValue = CStr(AppClass.FetchDBValue("SELECT UPPER(userName) FROM tblUsers WHERE ID=@id"))
    End Sub
    Function Datavalidation() As Boolean
        errmsg = ""
        If String.IsNullOrEmpty(OldPasswordTextEdit.EditValue) Then
            errmsg = "Enter Old Password"
            OldPasswordTextEdit.Focus()
        ElseIf String.IsNullOrEmpty(NewPasswordTextEdit.EditValue) Then
            errmsg = "Enter New Password"
            NewPasswordTextEdit.Focus()
        ElseIf String.IsNullOrEmpty(ConfirmTextEdit.EditValue) Then
            errmsg = "Confirm Password"
            ConfirmTextEdit.Focus()
        ElseIf String.Compare(NewPasswordTextEdit.EditValue, ConfirmTextEdit.EditValue, True) <> 0 Then
            errmsg = "Passwords Don't Match"
            With NewPasswordTextEdit
                .Focus()
                .SelectAll()
            End With
        ElseIf String.Compare(NewPasswordTextEdit.EditValue, OldPasswordTextEdit.EditValue, True) = 0 Then
            errmsg = "New and Old Passwords Have To Be Different"
            With NewPasswordTextEdit
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
    Function CheckPassword() As Boolean
        Dim pass = Encryption.HashString(OldPasswordTextEdit.EditValue)
        AddParams("@id", LogedUserID)
        Dim salt As String = AppClass.FetchDBValue("SELECT salt FROM tblUsers WHERE ID=@id")
        Dim HashedAndSalted = Encryption.HashString(String.Format("{0}{1}", pass, salt))

        AddParams("@user", LogedUserID)
        AddParams("@pass", HashedAndSalted)
        Dim result As Integer = AppClass.FetchDBValue("SELECT COUNT(*) FROM tblUsers WHERE ID=@user AND userPwd=@pass")
        If result = 0 Then
            AppClass.ShowNotification("Old password entered is incorrect")
            Return False
        Else
            Return True
        End If
    End Function
#End Region
#Region "EVENTS"
    Private Sub OldPasswordTextEdit_Leave(sender As Object, e As EventArgs) Handles OldPasswordTextEdit.Leave
        If OldPasswordTextEdit.EditValue IsNot Nothing Then
            If Not CheckPassword() Then
                With OldPasswordTextEdit
                    .Focus()
                    .SelectAll()
                End With
                Exit Sub
            End If
        End If
    End Sub
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If Not Datavalidation() Then
            Exit Sub
        End If
        Dim pwd = Encryption.HashString(NewPasswordTextEdit.EditValue)
        Dim salt = Encryption.GenerateSalt
        Dim hashsalted = Encryption.HashString(String.Format("{0}{1}", pwd, salt))

        AddParams("@pwd", hashsalted)
        AddParams("@salt", salt)
        AddParams("@id", LogedUserID)
        AppClass.ExecQuery("UPDATE tblUsers SET userPwd=@pwd,salt=@salt WHERE ID=@id")
        If RecordCount > 0 Then
            AppClass.ShowNotification("Changed Succssfully!")
            Reset()
        End If
    End Sub
#End Region
End Class
