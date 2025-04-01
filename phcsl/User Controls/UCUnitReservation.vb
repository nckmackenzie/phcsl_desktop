Public Class UCUnitReservation
    Private Shared _instance As UCUnitReservation
    Private IsEdit As Boolean
    Private ID As Integer
    Public Shared ReadOnly Property Instance As UCUnitReservation
        Get
            If _instance Is Nothing Then _instance = New UCUnitReservation()
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
        sql = "SELECT p.ID,UPPER(p.projectName) as ProjectName "
        sql &= "FROM tblProjects p WHERE (SELECT COUNT(*) FROM tblUnits WHERE sold=1 and projectId = p.ID) < noOfUnits"
        AppClass.LoadToLookUpEdit(sql, ProjectLookUpEdit, "ProjectName", "ID")
        sql = "SELECT M.memberNo as MemberNo,M.memberID AS [MemberID],UPPER(M.memberName) AS [MemberName],M.idNo AS [IDNumber],"
        sql &= "A.contact AS [Contact] FROM tblMember M LEFT OUTER JOIN tblMemberAddress A ON M.memberNo=A.memberNo ORDER BY M.memberID"
        AppClass.LoadToSearchLookUpEdit(sql, MemberSearchLookUpEdit, "MemberName", "MemberNo")
        IsEdit = False
        DeleteSimpleButton.Enabled = False
        ReleaseSimpleButton.Enabled = False
        ProjectLookUpEdit.Properties.ReadOnly = False
        UnitLookUpEdit.Properties.ReadOnly = False
        ID = Nothing
        AppClass.ClearItems(LayoutControl1)
    End Sub
    Function GetDays() As Integer
        If FromDateEdit.EditValue Is Nothing OrElse ToDateEdit.EditValue Is Nothing OrElse Not AppClass.IsValidDateRange(CDate(FromDateEdit.EditValue), CDate(ToDateEdit.EditValue)) Then
            Return False
        End If

        Return DateDiff(DateInterval.Day, CDate(FromDateEdit.EditValue).Date, CDate(ToDateEdit.EditValue).Date) + 1
    End Function
    Function Datavalidation() As Boolean
        errmsg = Nothing
        If MemberSearchLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select member"
            MemberSearchLookUpEdit.Focus()
        ElseIf ProjectLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select project"
            ProjectLookUpEdit.Focus()
        ElseIf UnitLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select unit"
            UnitLookUpEdit.Focus()
        ElseIf FromDateEdit.EditValue Is Nothing Then
            errmsg = "Select date reserving from"
            FromDateEdit.Focus()
        ElseIf ToDateEdit.EditValue Is Nothing Then
            errmsg = "Select date reserving to"
            ToDateEdit.Focus()
        ElseIf Not AppClass.IsValidDateRange(CDate(FromDateEdit.EditValue), CDate(ToDateEdit.EditValue)) Then
            errmsg = "Invalid date range selected"
            FromDateEdit.Focus()
        End If

        If errmsg IsNot Nothing Then
            AppClass.ShowError(errmsg, True)
            Return False
        Else
            Return True
        End If
    End Function
    Sub Find(fid As Integer)
        IsEdit = True
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(projectName) as ProjectName FROM tblProjects ORDER BY ProjectName",
                                  ProjectLookUpEdit, "ProjectName", "ID")
        AddParams("@id", fid)
        Dim Searchdt As DataTable = AppClass.LoadToDatatable("select * from tblReservations WHERE ID=@id", False)

        ID = Searchdt.Rows(0)(0)
        MemberSearchLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(1))
        ProjectLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(2))
        UnitLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(3))
        FromDateEdit.EditValue = CDate(Searchdt.Rows(0)(4))
        ToDateEdit.EditValue = CDate(Searchdt.Rows(0)(5))

        ReleaseSimpleButton.Enabled = True
        ProjectLookUpEdit.Properties.ReadOnly = True
        UnitLookUpEdit.Properties.ReadOnly = True
        DeleteSimpleButton.Enabled = True
    End Sub
#Region "EVENTS"
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
    Private Sub FromDateEdit_EditValueChanged(sender As Object, e As EventArgs) Handles FromDateEdit.EditValueChanged, ToDateEdit.EditValueChanged
        DaysTextEdit.EditValue = GetDays()
    End Sub
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If Not Datavalidation() Then
            Return
        End If


        AddParams("@member", MemberSearchLookUpEdit.EditValue)
        AddParams("@pid", ProjectLookUpEdit.EditValue)
        AddParams("@uid", UnitLookUpEdit.EditValue)
        AddParams("@from", CDate(FromDateEdit.EditValue))
        AddParams("@to", CDate(ToDateEdit.EditValue))
        If Not IsEdit Then
            AddParams("@released", 0)
            sql = "INSERT INTO tblReservations (memberNo,projectId,unitId,reserveDate,reserveTill,released) VALUES(@member,@pid,@uid,@from,@to,@released)"
        Else
            AddParams("@id", ID)
            sql = "UPDATE tblReservations SET memberNo=@member,projectId=@pid,unitId=@uid,reserveDate=@from,reserveTill=@to WHERE (ID=@id)"
        End If

        AppClass.ExecQuery(sql)
        If RecordCount > 0 Then
            AppClass.ShowNotification(If(IsEdit, "Edited Successfully", "Saved successfully"))
            Reset()
        End If
    End Sub
    Private Sub ProjectLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles ProjectLookUpEdit.EditValueChanged
        If ProjectLookUpEdit.EditValue IsNot Nothing Then
            sql = "SELECT ID,UPPER(unitName) AS UnitName FROM tblUnits "
            sql &= "WHERE projectId=@pid "
            If Not IsEdit Then
                sql &= "AND sold=0 AND archived=0 AND ID NOT IN (SELECT unitId FROM tblReservations WHERE projectId=@pid AND (released=0 OR reserveTill >= @today))"
            End If
            AddParams("@pid", CInt(ProjectLookUpEdit.EditValue))
            If Not IsEdit Then
                AddParams("@today", Date.Now.Date)
            End If
            AppClass.LoadToLookUpEdit(sql, UnitLookUpEdit, "UnitName", "ID")
        End If
    End Sub
    Private Sub FindSimpleButton_Click(sender As Object, e As EventArgs) Handles FindSimpleButton.Click
        Dim SearchBy As String = "Search by project name, unit no, member name"
        Using frm As New SearchForm(SearchBy, "spSearchReservations")
            If frm.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                Find(frm.DataGridView.CurrentRow.Cells(0).Value)
            End If
        End Using
    End Sub
    Private Sub ReleaseSimpleButton_Click(sender As Object, e As EventArgs) Handles ReleaseSimpleButton.Click
        If AppClass.AlertQuestion("Are you sure you want to release this unit from reservation? This cannot be undone") = DialogResult.Yes Then
            AddParams("@id", ID)
            AppClass.ExecQuery("UPDATE tblReservations SET released=1 WHERE (ID=@id) ")
            If RecordCount > 0 Then
                AppClass.ShowNotification("Released Successfully")
                Reset()
            End If
        End If
    End Sub
    Private Sub DeleteSimpleButton_Click(sender As Object, e As EventArgs) Handles DeleteSimpleButton.Click
        If AppClass.AlertQuestion("Are you sure you want to delete this reservation?") = DialogResult.Yes Then
            AddParams("@id", ID)
            AppClass.ExecQuery("DELETE FROM tblReservations WHERE (ID=@id) ")
            If RecordCount > 0 Then
                AppClass.ShowNotification("Deleted Successfully")
                Reset()
            End If
        End If
    End Sub
#End Region
#End Region
End Class
