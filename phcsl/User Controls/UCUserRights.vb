Imports System.Data.SqlClient
Public Class UCUserRights
    Private Shared _instance As UCUserRights
    Private IsEdit As Boolean
    Public Shared ReadOnly Property Instance As UCUserRights
        Get
            If _instance Is Nothing Then _instance = New UCUserRights()
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
        IsEdit = False
        AddParams("@uid", 1)
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(userName) As userName FROM tblUsers WHERE (userTypeId > @uid) ORDER BY userName" _
                        , UsersLookUpEdit, "userName", "ID")
        AppClass.ClearItems(LayoutControl1)
        AppClass.FormatDatagridView(DataGridView)
        LoadForms()
    End Sub
    Sub LoadForms()
        DataGridView.Rows.Clear()
        dt = New DataTable
        dt = AppClass.LoadToDatatable("SELECT ID,formName FROM tblForms ORDER BY formName")
        For Each row In dt.Rows
            Populate(row(0), row(1))
        Next
    End Sub
    Sub Populate(id As Short, name As String)
        Dim row As String() = New String() {id, name}
        DataGridView.Rows.Add(row)
    End Sub
    Sub Save()
        Using connection As New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Using MyTransaction As SqlTransaction = connection.BeginTransaction
                Try

                    sql = "DELETE FROM tblUserRights WHERE userId=@id"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@id", SqlDbType.Int).Value = UsersLookUpEdit.EditValue
                            .ExecuteNonQuery()
                        End With
                    End Using

                    For i = 0 To DataGridView.Rows.Count - 1
                        If DataGridView.Rows(i).Cells(2).Value = True Then
                            sql = "INSERT INTO tblUserRights (userId,formId,access) VALUES(@uid,@fid,@acc)"
                            Using cmd = New SqlCommand(sql, connection, MyTransaction)
                                With cmd
                                    .Parameters.Clear()
                                    .Parameters.Add("@uid", SqlDbType.Int).Value = UsersLookUpEdit.EditValue
                                    .Parameters.Add("@fid", SqlDbType.Int).Value = DataGridView.Rows(i).Cells(0).Value
                                    .Parameters.Add("@acc", SqlDbType.Bit).Value = CBool(DataGridView.Rows(i).Cells(2).Value)
                                    .Parameters.Add("@save", SqlDbType.Bit).Value = True
                                    .Parameters.Add("@search", SqlDbType.Bit).Value = True
                                    .Parameters.Add("@edit", SqlDbType.Bit).Value = True
                                    .Parameters.Add("@delete", SqlDbType.Bit).Value = True
                                    .ExecuteNonQuery()
                                End With
                            End Using
                        End If
                    Next

                    sql = "INSERT INTO tblLogs (userID,[activity],activityDate) VALUES(@id,@act,@actdate)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@id", SqlDbType.NVarChar).Value = LogedUserID
                            .Parameters.Add("@act", SqlDbType.NVarChar).Value = "Assigned User Rights To " & UsersLookUpEdit.Text.ToString
                            .Parameters.Add("@actdate", SqlDbType.Date).Value = Date.Now.Date
                            .ExecuteNonQuery()
                        End With
                    End Using
                    MyTransaction.Commit()

                    AppClass.ShowNotification("Saved Successfully!")
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
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If UsersLookUpEdit.EditValue Is Nothing Then
            AppClass.ShowNotification("Select user")
            UsersLookUpEdit.Focus()
            Exit Sub
        End If
        Dim count As Integer
        For Each row As DataGridViewRow In DataGridView.Rows
            If CBool(row.Cells(2).Value) = True Then
                count += 1
            End If
        Next
        If count = 0 Then
            AppClass.ShowNotification("No rights assigned")
            Exit Sub
        End If

        Save()
    End Sub
    Private Sub UsersLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles UsersLookUpEdit.EditValueChanged
        If UsersLookUpEdit.EditValue IsNot Nothing Then
            Try
                Using connection = New SqlConnection(connstr)
                    With connection
                        If .State = ConnectionState.Closed Then
                            .Open()
                        End If
                    End With
                    sql = "SELECT COUNT(*) FROM tblUserRights WHERE userId=@id"
                    Using cmd = New SqlCommand(sql, connection)
                        cmd.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = UsersLookUpEdit.EditValue
                        Using rd = cmd.ExecuteReader
                            rd.Read()
                            If CInt(rd.Item(0)) > 0 Then
                                IsEdit = True
                            Else
                                IsEdit = False
                            End If
                        End Using
                    End Using

                    If IsEdit = True Then
                        For i = 0 To DataGridView.Rows.Count - 1
                            sql = "SELECT access FROM tblUserRights WHERE (userId)=@id AND (formId=@fid)"
                            Using da = New SqlDataAdapter(sql, connection)
                                da.SelectCommand.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = UsersLookUpEdit.EditValue
                                da.SelectCommand.Parameters.Add(New SqlParameter("@fid", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(0).Value
                                Using dt = New DataTable
                                    da.Fill(dt)
                                    If dt.Rows.Count > 0 Then
                                        DataGridView.Rows(i).Cells(2).Value = CBool(dt.Rows(0)(0))
                                    Else
                                        DataGridView.Rows(i).Cells(2).Value = False
                                    End If
                                End Using
                            End Using
                        Next
                    Else
                        LoadForms()
                    End If
                End Using
            Catch ex As Exception
                AppClass.ShowError(ex.Message)
            End Try
        End If
    End Sub
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
#End Region
End Class
