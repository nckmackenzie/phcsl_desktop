Imports System.Data.SqlClient

Public Class UCDatabase
    Private Shared _instance As UCDatabase
    Public Shared ReadOnly Property Instance As UCDatabase
        Get
            If _instance Is Nothing Then _instance = New UCDatabase()
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
    End Sub
#End Region
#Region "EVENTS"
    Private Sub BackupBrowseSimpleButton_Click(sender As Object, e As EventArgs) Handles BackupBrowseSimpleButton.Click
        Dim fbd As New FolderBrowserDialog With {
            .Description = "Select Destination Folder"
        }
        If fbd.ShowDialog = System.Windows.Forms.DialogResult.OK Then
            BackupLocationTextEdit.EditValue = fbd.SelectedPath
        End If
    End Sub
    Private Sub BackupSimpleButton_Click(sender As Object, e As EventArgs) Handles BackupSimpleButton.Click
        Try
            If String.IsNullOrEmpty(BackupLocationTextEdit.EditValue) Then
                AppClass.ShowNotification("Backup Location Not Set!")
                BackupBrowseSimpleButton.Focus()
                Exit Sub
            End If
            Using connection = New SqlConnection(connstr)
                With connection
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With

                Dim db As String = connection.Database.ToString

                sql = "BACKUP DATABASE [" + db + "] TO DISK='" + BackupLocationTextEdit.EditValue + "\\" + "Database_Backup " + Date.Now.ToString("dd-MM-yy--HH-mm-ss") + ".bak'"
                Using cmd = New SqlCommand(sql, connection)
                    cmd.ExecuteNonQuery()
                    AppClass.ShowNotification("Backup Successful")
                End Using
            End Using
        Catch ex As Exception
            AppClass.ShowError(ex.Message)
        End Try
    End Sub
    Private Sub BrowseRestoreSimpleButton_Click(sender As Object, e As EventArgs) Handles BrowseRestoreSimpleButton.Click
        Dim path As String = Nothing
        Dim OFD As New OpenFileDialog
        With OFD
            .Filter = "SQL SERVER DATABASE BACKUP FILES Files(*.bak)|*.bak" '//set exetensions to filter
            .Title = "Restore Database"
            .ValidateNames = True
            .Multiselect = False
            If .ShowDialog = System.Windows.Forms.DialogResult.OK Then
                RestoreTextEdit.EditValue = .FileName
            End If
        End With
    End Sub
    Private Sub RestoreSimpleButton_Click(sender As Object, e As EventArgs) Handles RestoreSimpleButton.Click
        If String.IsNullOrEmpty(RestoreTextEdit.EditValue) Then
            AppClass.ShowNotification("Select File To Restore")
            BrowseRestoreSimpleButton.Focus()
            Exit Sub
        End If
        Try
            Using connection = New SqlConnection(connstr)
                With connection
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With

                Dim db As String = connection.Database.ToString

                sql = "ALTER DATABASE [" + db + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
                Using CMD = New SqlCommand(sql, connection)
                    CMD.ExecuteNonQuery()
                End Using

                sql = "USE MASTER RESTORE DATABASE [" + db + "] FROM DISK='" + RestoreTextEdit.EditValue + "'WITH REPLACE;"
                Using cmd = New SqlCommand(sql, connection)
                    cmd.ExecuteNonQuery()
                End Using

                sql = "ALTER DATABASE [" + db + "] SET MULTI_USER"
                Using cmd = New SqlCommand(sql, connection)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            AppClass.ShowNotification("Database Restored Successfully!")
            Reset()
        Catch ex As Exception
            AppClass.ShowError(ex.Message)
        End Try
    End Sub
#End Region
End Class
