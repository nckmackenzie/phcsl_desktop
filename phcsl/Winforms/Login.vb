Imports System.Data.SqlClient
Imports System.Text
Imports System.Security.Cryptography
Public Class LoginForm
#Region "SUBS"
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        DatabaseComboBoxEdit.SelectedIndex = 0
    End Sub
    Sub Login()
        LogedUserID = Nothing
        connstr = Nothing
        'If DatabaseComboBoxEdit.SelectedIndex = 0 Then
        '    connstr = "Data Source=localhost\SQLEXPRESS;Initial Catalog=schoolDB;User ID=sa;Password=NA-b$H12;"
        'ElseIf DatabaseComboBoxEdit.SelectedIndex = 1 Then
        '    connstr = "Data Source=localhost\SQLEXPRESS;Initial Catalog=schoolTest;User ID=sa;Password=NA-b$H12;"
        'End If
        Dim Active As Boolean
        Dim promptpwd As Boolean
        Dim pwddatechng As Date
        Using connection = New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Dim readSaltQuery As String = "SELECT * FROM tblUsers WHERE UserID=@id"
            Dim sqlcmd As New SqlCommand(readSaltQuery, connection)
            sqlcmd.Parameters.AddWithValue("@id", UserNameTextEdit.EditValue.ToString)

            Dim reader As SqlDataReader = sqlcmd.ExecuteReader
            While reader.Read
                LogedUserID = CInt(reader("ID"))
                promptpwd = CBool(reader("promptPasswordChange"))
                pwddatechng = CDate(reader("passwordChangeDate"))
                Active = CBool(reader("active"))
            End While
            reader.Close()
            Dim checkQuery As String = "SELECT COUNT(*) FROM tblUsers WHERE UserID=@user AND userPwd=@pass"
            Dim sqlcmd0 As New SqlCommand(checkQuery, connection)
            sqlcmd0.Parameters.AddWithValue("@user", UserNameTextEdit.EditValue.ToString)
            sqlcmd0.Parameters.AddWithValue("@pass", Encrypt)
            Dim result As Integer = Convert.ToInt32(sqlcmd0.ExecuteScalar())
            If result = 1 Then
                MainForm.Show()
                Me.Hide()
            Else
                MessageBox.Show("Password Entered Is Incorrect", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information)
                With PasswordTextEdit
                    .Focus()
                    .SelectAll()
                End With
            End If
            sqlcmd0.Dispose()
            sqlcmd.Dispose()
        End Using
    End Sub
    Function Encrypt() As String
        Dim encytptedString As String
        Dim data As Byte() = UTF8Encoding.UTF8.GetBytes(PasswordTextEdit.EditValue.ToString)
        Using md5 As New MD5CryptoServiceProvider()
            Dim keys As Byte() = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash))
            Using tripDes As New TripleDESCryptoServiceProvider()
                tripDes.Key = keys
                tripDes.Mode = CipherMode.ECB
                tripDes.Padding = PaddingMode.PKCS7
                Dim transform As ICryptoTransform = tripDes.CreateEncryptor()
                Dim results As Byte() = transform.TransformFinalBlock(data, 0, data.Length)
                encytptedString = Convert.ToBase64String(results, 0, results.Length)
            End Using
        End Using
        Return encytptedString
    End Function
#End Region
#Region "EVENTS"
    Private Sub LoginBtn_Click(sender As Object, e As EventArgs) Handles LoginBtn.Click
        If String.IsNullOrEmpty(UserNameTextEdit.EditValue.ToString) Then
            AppClass.ShowNotification("Enter Username!")
            UserNameTextEdit.Focus()
            Exit Sub
        End If

        If String.IsNullOrEmpty(PasswordTextEdit.EditValue.ToString) Then
            AppClass.ShowNotification("Enter Password!")
            PasswordTextEdit.Focus()
            Exit Sub
        End If
        AppClass.UserLogin(UserNameTextEdit.Text, PasswordTextEdit.EditValue.ToString, PasswordTextEdit)
        'connstr = Nothing
        'If DatabaseComboBoxEdit.SelectedIndex = 0 Then
        '    connstr = "Data Source=localhost\SQLEXPRESS;Initial Catalog=housingDB;User ID=sa;Password=NA-b$H12;"
        'ElseIf DatabaseComboBoxEdit.SelectedIndex = 1 Then
        '    connstr = "Data Source=localhost\SQLEXPRESS;Initial Catalog=housingTest;User ID=sa;Password=NA-b$H12;"
        'End If
    End Sub
#End Region
End Class