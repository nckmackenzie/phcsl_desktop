Imports System.ComponentModel
Imports DevExpress.XtraBars.Navigation

Public Class MainForm
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        LocationSet = CBool(AppClass.FetchDBValue("SELECT LocationSet FROM tblDatabaseSettings"))
        If LocationSet Then
            BackupLocation = CStr(AppClass.FetchDBValue("SELECT BackupLocation FROM tblDatabaseSettings"))
        Else
            BackupLocation = Nothing
        End If
        aceHome.Expanded = False
        aceProjects.Expanded = False
        aceFinance.Expanded = False
        aceReports.Expanded = False
    End Sub
    Sub Backup()
        BackupLocation = CStr(AppClass.FetchDBValue("SELECT BackupLocation FROM tblDatabaseSettings"))
        Try
            Using connection = New System.Data.SqlClient.SqlConnection(connstr)
                With connection
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With

                Dim db As String = connection.Database.ToString

                sql = "BACKUP DATABASE [" + db + "] TO DISK='" + BackUpLocation.ToString + "\\" + "Database_Backup " + Date.Now.ToString("dd-MM-yy--HH-mm-ss") + ".bak'"
                Using cmd = New System.Data.SqlClient.SqlCommand(sql, connection)
                    cmd.ExecuteNonQuery()
                    ' XtraMessageBox.Show("Backup Successful", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End Using
            End Using
        Catch ex As Exception
            AppClass.ShowError(ex.Message)
        End Try
    End Sub
    Private Sub AceMessaging_Click(sender As Object, e As EventArgs) Handles aceMessaging.Click
        If Not AppContainer.Controls.Contains(UCPaymentReminders.Instance) Then
            AppContainer.Controls.Add(UCPaymentReminders.Instance)
            UCPaymentReminders.Instance.Dock = DockStyle.Fill
            UCPaymentReminders.Instance.BringToFront()
        End If
        UCPaymentReminders.Instance.BringToFront()
        TopBarItem.Caption = aceMessaging.Text.ToString
    End Sub
    Private Sub AceMemberContributions_Click(sender As Object, e As EventArgs) Handles AceMemberContributions.Click
        If Not AppContainer.Controls.Contains(UCMemberCollections.Instance) Then
            AppContainer.Controls.Add(UCMemberCollections.Instance)
            UCMemberCollections.Instance.Dock = DockStyle.Fill
            UCMemberCollections.Instance.BringToFront()
        End If
        UCMemberCollections.Instance.BringToFront()
        TopBarItem.Caption = AceMemberContributions.Text.ToString
    End Sub

    Private Sub MainForm_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If AppClass.AlertQuestion("Exit Application?") = DialogResult.Yes Then
            If LocationSet Then
                Backup()
            End If
        Else
            e.Cancel = True
            Exit Sub
        End If
    End Sub
    Private Sub MainForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Application.Exit()
    End Sub
    Private Sub AceUsers_Click(sender As Object, e As EventArgs) Handles AceUsers.Click
        If Not AppContainer.Controls.Contains(UCUsers.Instance) Then
            AppContainer.Controls.Add(UCUsers.Instance)
            UCUsers.Instance.Dock = DockStyle.Fill
            UCUsers.Instance.BringToFront()
        End If
        UCUsers.Instance.BringToFront()
        TopBarItem.Caption = AceUsers.Text.ToString
    End Sub
    Private Sub AceUserRights_Click(sender As Object, e As EventArgs) Handles AceUserRights.Click
        If Not AppContainer.Controls.Contains(UCUserRights.Instance) Then
            AppContainer.Controls.Add(UCUserRights.Instance)
            UCUserRights.Instance.Dock = DockStyle.Fill
            UCUserRights.Instance.BringToFront()
        End If
        UCUserRights.Instance.BringToFront()
        TopBarItem.Caption = AceUserRights.Text.ToString
    End Sub
    Private Sub AceCloneRights_Click(sender As Object, e As EventArgs) Handles AceCloneRights.Click
        If Not AppContainer.Controls.Contains(UCCloneRights.Instance) Then
            AppContainer.Controls.Add(UCCloneRights.Instance)
            UCCloneRights.Instance.Dock = DockStyle.Fill
            UCCloneRights.Instance.BringToFront()
        End If
        UCCloneRights.Instance.BringToFront()
        TopBarItem.Caption = AceCloneRights.Text.ToString
    End Sub
    Private Sub AceDatabase_Click(sender As Object, e As EventArgs) Handles AceDatabase.Click
        If Not AppContainer.Controls.Contains(UCDatabase.Instance) Then
            AppContainer.Controls.Add(UCDatabase.Instance)
            UCDatabase.Instance.Dock = DockStyle.Fill
            UCDatabase.Instance.BringToFront()
        End If
        UCDatabase.Instance.BringToFront()
        TopBarItem.Caption = AceDatabase.Text.ToString
    End Sub
    Private Sub AceChangePassword_Click(sender As Object, e As EventArgs) Handles AceChangePassword.Click
        If Not AppContainer.Controls.Contains(UCChangePassword.Instance) Then
            AppContainer.Controls.Add(UCChangePassword.Instance)
            UCChangePassword.Instance.Dock = DockStyle.Fill
            UCChangePassword.Instance.BringToFront()
        End If
        UCChangePassword.Instance.BringToFront()
        TopBarItem.Caption = AceChangePassword.Text.ToString
    End Sub
    Private Sub AceMembers_Click(sender As Object, e As EventArgs) Handles AceMembers.Click
        If Not AppContainer.Controls.Contains(UCMembers.Instance) Then
            AppContainer.Controls.Add(UCMembers.Instance)
            UCMembers.Instance.Dock = DockStyle.Fill
            UCMembers.Instance.BringToFront()
        End If
        UCMembers.Instance.BringToFront()
        TopBarItem.Caption = AceMembers.Text.ToString
    End Sub
    Private Sub AceAssignMemberNo_Click(sender As Object, e As EventArgs) Handles AceAssignMemberNo.Click
        If Not AppContainer.Controls.Contains(UCAssignMemberNo.Instance) Then
            AppContainer.Controls.Add(UCAssignMemberNo.Instance)
            UCAssignMemberNo.Instance.Dock = DockStyle.Fill
            UCAssignMemberNo.Instance.BringToFront()
        End If
        UCAssignMemberNo.Instance.BringToFront()
        TopBarItem.Caption = AceAssignMemberNo.Text.ToString
    End Sub
    Private Sub AceProjectsForm_Click(sender As Object, e As EventArgs) Handles AceProjectsForm.Click
        If Not AppContainer.Controls.Contains(UCProjects.Instance) Then
            AppContainer.Controls.Add(UCProjects.Instance)
            UCProjects.Instance.Dock = DockStyle.Fill
            UCProjects.Instance.BringToFront()
        End If
        UCProjects.Instance.BringToFront()
        TopBarItem.Caption = AceProjectsForm.Text.ToString
    End Sub
    Private Sub AceGeneralMessage_Click(sender As Object, e As EventArgs) Handles aceGeneralMessage.Click
        If Not AppContainer.Controls.Contains(UCGeneralMessage.Instance) Then
            AppContainer.Controls.Add(UCGeneralMessage.Instance)
            UCGeneralMessage.Instance.Dock = DockStyle.Fill
            UCGeneralMessage.Instance.BringToFront()
        End If
        UCGeneralMessage.Instance.BringToFront()
        TopBarItem.Caption = aceGeneralMessage.Text.ToString
    End Sub
    Private Sub AceDividentPayments_Click(sender As Object, e As EventArgs) Handles AceDividentPayments.Click
        If Not AppContainer.Controls.Contains(UCDividendsPayments.Instance) Then
            AppContainer.Controls.Add(UCDividendsPayments.Instance)
            UCDividendsPayments.Instance.Dock = DockStyle.Fill
            UCDividendsPayments.Instance.BringToFront()
        End If
        UCDividendsPayments.Instance.BringToFront()
        TopBarItem.Caption = AceDividentPayments.Text.ToString
    End Sub

    Private Sub AceImportExpenses_Click(sender As Object, e As EventArgs) Handles AceImportExpenses.Click
        If Not AppContainer.Controls.Contains(UCImportExpenses.Instance) Then
            AppContainer.Controls.Add(UCImportExpenses.Instance)
            UCImportExpenses.Instance.Dock = DockStyle.Fill
            UCImportExpenses.Instance.BringToFront()
        End If
        UCImportExpenses.Instance.BringToFront()
        TopBarItem.Caption = AceImportExpenses.Text.ToString
    End Sub

    Private Sub AceReleaseUnit_Click(sender As Object, e As EventArgs) Handles AceReleaseUnit.Click
        If Not AppContainer.Controls.Contains(UCReleaseUnit.Instance) Then
            AppContainer.Controls.Add(UCReleaseUnit.Instance)
            UCReleaseUnit.Instance.Dock = DockStyle.Fill
            UCReleaseUnit.Instance.BringToFront()
        End If
        UCReleaseUnit.Instance.BringToFront()
        TopBarItem.Caption = AceReleaseUnit.Text.ToString
    End Sub
End Class
