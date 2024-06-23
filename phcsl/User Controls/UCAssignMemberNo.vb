Public Class UCAssignMemberNo
    Private Shared _instance As UCAssignMemberNo
    Public Shared ReadOnly Property Instance As UCAssignMemberNo
        Get
            If _instance Is Nothing Then _instance = New UCAssignMemberNo()
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
        AppClass.LoadToLookUpEdit("SELECT memberNo,UPPER(memberName) AS memberName FROM tblMember WHERE (memberID IS NULL) ORDER BY memberName" _
                         , MemberLookUpEdit, "memberName", "memberNo")
        IDTextEdit.EditValue = Nothing
    End Sub
    Function Datavalidation() As Boolean
        errmsg = ""
        If MemberLookUpEdit.EditValue = Nothing Then
            errmsg = "Select Member To Assign Member No"
            MemberLookUpEdit.Focus()
        ElseIf IDTextEdit.EditValue Is Nothing Then
            errmsg = "Enter Member No"
            IDTextEdit.Focus()
        ElseIf IDTextEdit.EditValue.ToString.Length <> 4 Then
            errmsg = "Invalid ID entered"
            IDTextEdit.Focus()
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

        Dim full As String = "PHCSL/" & Trim(IDTextEdit.EditValue)

        AddParams("@mid", full)
        If CInt(AppClass.FetchDBValue("SELECT COUNT(memberNo) FROM tblMember WHERE (memberID=@mid)")) > 0 Then
            AppClass.ShowNotification("Member No Already Assigned!")
            With IDTextEdit
                .Focus()
                .SelectAll()
            End With
            Exit Sub
        End If

        AddParams("@id", Trim(full.ToUpper.ToString))
        AddParams("@no", CInt(MemberLookUpEdit.EditValue))
        AppClass.ExecQuery("UPDATE tblMember SET memberID=@id WHERE memberNo=@no")
        If RecordCount > 0 Then
            AddParams("@action", "insert")
            AddParams("@userid", LogedUserID)
            AddParams("@activity", "Assigned Member No " & Trim(full) & " To " & MemberLookUpEdit.Text)
            AddParams("@actDate", Date.Now.Date)
            AppClass.ExecSP("spLogs")
            AppClass.ShowNotification("Saved Successfully!")
            Reset()
        End If
    End Sub
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
#End Region
End Class
