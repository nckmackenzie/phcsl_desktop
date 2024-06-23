Public Class UCCloneRights
    Private Shared _instance As UCCloneRights
    Public Shared ReadOnly Property Instance As UCCloneRights
        Get
            If _instance Is Nothing Then _instance = New UCCloneRights()
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
        AddParams("@uid", 1)
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(userName) As userName FROM tblUsers WHERE (userTypeId > @uid) ORDER BY userName" _
                        , FromLookUpEdit, "userName", "ID")
        AddParams("@uid", 1)
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(userName) As userName FROM tblUsers WHERE (userTypeId > @uid) ORDER BY userName" _
                        , ToLookUpEdit, "userName", "ID")
        AppClass.ClearItems(LayoutControl1)
    End Sub
    Function Datavalidation() As Boolean
        errmsg = ""
        If FromLookUpEdit.EditValue = Nothing Then
            errmsg = "Selected Users To Copy From!"
            FromLookUpEdit.Focus()
        ElseIf ToLookUpEdit.EditValue = Nothing Then
            errmsg = "Selected Users To Copy To!"
            ToLookUpEdit.Focus()
        ElseIf FromLookUpEdit.EditValue = ToLookUpEdit.EditValue Then
            errmsg = "Selected Users Are The Same!"
            ToLookUpEdit.Focus()
        End If

        If errmsg <> "" Then
            AppClass.ShowNotification(errmsg)
            Return False
        Else
            Return True
        End If
    End Function
#End Region
#Region "EVENTS"
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If Not Datavalidation() Then
            Exit Sub
        End If
        AddParams("@user1", FromLookUpEdit.EditValue)
        AddParams("@user2", ToLookUpEdit.EditValue)
        AppClass.ExecSP("spMenuPairing")
        If RecordCount > 0 Then
            AppClass.ShowNotification("Paired Successfully!")
            Reset()
        End If
    End Sub
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
#End Region
End Class
