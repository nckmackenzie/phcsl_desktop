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

                sql = "BACKUP DATABASE [" + db + "] TO DISK='" + BackupLocation.ToString + "\\" + "Database_Backup " + Date.Now.ToString("dd-MM-yy--HH-mm-ss") + ".bak'"
                Using cmd = New System.Data.SqlClient.SqlCommand(sql, connection)
                    cmd.ExecuteNonQuery()
                    ' XtraMessageBox.Show("Backup Successful", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End Using
            End Using
        Catch ex As Exception
            AppClass.ShowError(ex.Message)
        End Try
    End Sub
    Public Sub AddUserControlToContainer(ByVal ControlInstance As Control, ByVal HeaderText As DevExpress.XtraBars.Navigation.AccordionControlElement)
        If Not AppContainer.Controls.Contains(ControlInstance) Then
            AppContainer.Controls.Add(ControlInstance)
            ControlInstance.Dock = DockStyle.Fill
        End If
        ControlInstance.BringToFront()
        Me.Text = HeaderText.Text.ToString & If(CompanyName IsNot Nothing, " / " & CompanyName, "")
        TopBarItem.Caption = HeaderText.Text.ToString
    End Sub
    Private Sub AceMessaging_Click(sender As Object, e As EventArgs) Handles aceMessaging.Click
        AddUserControlToContainer(UCPaymentReminders.Instance, aceMessaging)
    End Sub
    Private Sub AceMemberContributions_Click(sender As Object, e As EventArgs) Handles AceMemberContributions.Click
        AddUserControlToContainer(UCMemberCollections.Instance, AceMemberContributions)
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
        AddUserControlToContainer(UCUsers.Instance, AceUsers)
    End Sub
    Private Sub AceUserRights_Click(sender As Object, e As EventArgs) Handles AceUserRights.Click
        AddUserControlToContainer(UCUserRights.Instance, AceUserRights)
    End Sub
    Private Sub AceCloneRights_Click(sender As Object, e As EventArgs) Handles AceCloneRights.Click
        AddUserControlToContainer(UCCloneRights.Instance, AceCloneRights)
    End Sub
    Private Sub AceDatabase_Click(sender As Object, e As EventArgs) Handles AceDatabase.Click
        AddUserControlToContainer(UCDatabase.Instance, AceDatabase)
    End Sub
    Private Sub AceChangePassword_Click(sender As Object, e As EventArgs) Handles AceChangePassword.Click
        AddUserControlToContainer(UCChangePassword.Instance, AceChangePassword)
    End Sub
    Private Sub AceMembers_Click(sender As Object, e As EventArgs) Handles AceMembers.Click
        AddUserControlToContainer(UCMembers.Instance, AceMembers)
    End Sub
    Private Sub AceAssignMemberNo_Click(sender As Object, e As EventArgs) Handles AceAssignMemberNo.Click
        AddUserControlToContainer(UCAssignMemberNo.Instance, AceAssignMemberNo)
    End Sub
    Private Sub AceProjectsForm_Click(sender As Object, e As EventArgs) Handles AceProjectsForm.Click
        AddUserControlToContainer(UCProjects.Instance, AceProjectsForm)
    End Sub
    Private Sub AceGeneralMessage_Click(sender As Object, e As EventArgs) Handles aceGeneralMessage.Click
        AddUserControlToContainer(UCGeneralMessage.Instance, aceGeneralMessage)
    End Sub
    Private Sub AceDividentPayments_Click(sender As Object, e As EventArgs) Handles AceDividentPayments.Click
        AddUserControlToContainer(UCDividendsPayments.Instance, AceDividentPayments)
    End Sub
    Private Sub AceImportExpenses_Click(sender As Object, e As EventArgs) Handles AceImportExpenses.Click
        AddUserControlToContainer(UCImportExpenses.Instance, AceImportExpenses)
    End Sub
    Private Sub AceReleaseUnit_Click(sender As Object, e As EventArgs) Handles AceReleaseUnit.Click
        AddUserControlToContainer(UCReleaseUnit.Instance, AceReleaseUnit)
    End Sub
    Private Sub AcepettyCashUtilization_Click(sender As Object, e As EventArgs) Handles AcepettyCashUtilization.Click
        AddUserControlToContainer(UCPettyCashReport.Instance, AcepettyCashUtilization)
    End Sub
    Private Sub AceUnits_Click(sender As Object, e As EventArgs) Handles AceUnits.Click
        AddUserControlToContainer(UCUnits.Instance, AceUnits)
    End Sub
    Private Sub AceUnitSale_Click(sender As Object, e As EventArgs) Handles AceUnitSale.Click
        AddUserControlToContainer(UCUnitSale.Instance, AceUnitSale)
    End Sub
End Class
