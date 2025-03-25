Imports System.Data.SqlClient
Public Class UCUnits
    Private Shared _instance As UCUnits
    Private IsEdit As Boolean
    Public Shared ReadOnly Property Instance As UCUnits
        Get
            If _instance Is Nothing Then _instance = New UCUnits()
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
        AppClass.FormatDatagridView(DataGridView)
        AddParams("@soldout", False)
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(projectName) as ProjectName FROM tblProjects WHERE (soldOut=@soldout) AND (coalesce(unitsCreated,0) < noOfUnits) ORDER BY ProjectName",
                                  ProjectsLookUpEdit, "ProjectName", "ID")
        AppClass.ClearItems(LayoutControl1)
        IsEdit = False
        DataGridView.Rows.Clear()
        DeleteSimpleButton.Enabled = False
        ProjectsLookUpEdit.Properties.ReadOnly = False
    End Sub
    Sub FillGridWithValues(index As Int32, value As String)
        For Each row As DataGridViewRow In DataGridView.Rows
            row.Cells(index).Value = value
        Next
    End Sub
    Function Datavalidation() As Boolean
        errmsg = Nothing
        If ProjectsLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select project"
            ProjectsLookUpEdit.Focus()
        ElseIf UnitsToCreateTextEdit.EditValue Is Nothing Then
            errmsg = "Enter units to create"
            UnitsToCreateTextEdit.Focus()
        ElseIf CInt(UnitsToCreateTextEdit.EditValue) > CInt(UnitsTotalTextEdit.EditValue) Then
            errmsg = "Cannot create more units than available"
            With UnitsTotalTextEdit
                .Focus()
                .SelectAll()
            End With
        ElseIf DataGridView.Rows.Count = 0 Then
            errmsg = "No Units to add"
        ElseIf CommissionCriteriaComboBoxEdit.EditValue IsNot Nothing AndAlso CommissionValueTextEdit.EditValue Is Nothing Then
            errmsg = "Enter commission amount or percentage value"
            CommissionValueTextEdit.Focus()
        End If

        If errmsg IsNot Nothing Then
            AppClass.ShowError(errmsg)
            Return False
        Else
            Return True
        End If
    End Function
    Public Function ValidateDataGridViewColumn(dgv As DataGridView, columnIndex As Integer, errorMessage As String, Optional checkNumber As Boolean = False) As Boolean
        For Each row As DataGridViewRow In dgv.Rows
            If row.IsNewRow Then Continue For

            Dim cellValue As Object = row.Cells(columnIndex).Value

            If IsDBNull(cellValue) OrElse cellValue Is Nothing OrElse String.IsNullOrWhiteSpace(cellValue.ToString()) Then
                AppClass.ShowError(errorMessage)
                dgv.CurrentCell = row.Cells(columnIndex)
                dgv.BeginEdit(True)
                Return False
            End If

            If checkNumber AndAlso Not IsNumeric(cellValue) Then
                AppClass.ShowError("Please enter a valid number.")
                dgv.CurrentCell = row.Cells(columnIndex)
                dgv.BeginEdit(True)
                Return False
            End If
        Next
        Return True
    End Function
    Sub Save()
        Dim FirstId As Integer = AppClass.GenerateDBID("tblUnits")
        Using Connection As New SqlConnection(connstr)
            With Connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With
            Using MyTransaction As SqlTransaction = Connection.BeginTransaction
                Try

                    For i = 0 To DataGridView.Rows.Count - 1
                        sql = "INSERT INTO tblUnits (ID,projectId,unitName,unitBuyingPrice,unitSellingPriceMember,unitSellingPriceNonMember,titlePrice,landSize,commisionType,commission,sellingPriceSixPlan,sellingPriceTwelvePlan) "
                        sql &= "VALUES(@id,@project,@name,@bp,@member,@nonmember,@title,@size,@commtype,@comm,@six,@twelve)"
                        Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = FirstId + i
                                .Parameters.Add(New SqlParameter("@project", SqlDbType.Int)).Value = CInt(ProjectsLookUpEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@name", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(0).Value
                                .Parameters.Add(New SqlParameter("@bp", SqlDbType.Decimal)).Value = ProcessEditValue(UnitBuyingPriceTextEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@member", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(2).Value
                                .Parameters.Add(New SqlParameter("@nonmember", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(3).Value
                                .Parameters.Add(New SqlParameter("@title", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(1).Value
                                .Parameters.Add(New SqlParameter("@size", SqlDbType.VarChar)).Value = DataGridView.Rows(i).Cells(4).Value
                                .Parameters.Add(New SqlParameter("@commtype", SqlDbType.Decimal)).Value = If(CommissionCriteriaComboBoxEdit.EditValue IsNot Nothing, CommissionCriteriaComboBoxEdit.SelectedIndex + 1, DBNull.Value)
                                .Parameters.Add(New SqlParameter("@comm", SqlDbType.Decimal)).Value = ProcessEditValue(CommissionValueTextEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@six", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(5).Value
                                .Parameters.Add(New SqlParameter("@twelve", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(6).Value
                                .ExecuteNonQuery()
                            End With
                        End Using
                    Next

                    sql = "UPDATE tblProjects SET unitsCreated=@created WHERE ID=@id"
                    Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@created", SqlDbType.Int)).Value = ProcessEditValue(UnitsToCreateTextEdit.EditValue)
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = CInt(ProjectsLookUpEdit.EditValue)
                            .ExecuteNonQuery()
                        End With
                    End Using

                    MyTransaction.Commit()
                    AppClass.ShowNotification("Saved Successfully")
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
    Sub Find(fid As Integer)
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(projectName) as ProjectName FROM tblProjects ORDER BY ProjectName",
                                  ProjectsLookUpEdit, "ProjectName", "ID")
        With ProjectsLookUpEdit
            .EditValue = fid
            .Properties.ReadOnly = True
        End With
        'ProjectsLookUpEdit.EditValue = fid
        AddParams("@pid", CInt(ProjectsLookUpEdit.EditValue))
        UnitsToCreateTextEdit.EditValue = CInt(AppClass.FetchDBValue("select count(ID) from tblUnits WHERE projectId=@pid"))
        RemainingUnitsTextEdit.EditValue = CInt(UnitsTotalTextEdit.EditValue) - CInt(UnitsToCreateTextEdit.EditValue)
        sql = "SELECT TOP 1 commisionType,commission,unitBuyingPrice FROM tblUnits WHERE projectId=@pid"
        AddParams("@pid", CInt(ProjectsLookUpEdit.EditValue))
        Dim Searchdt As DataTable = AppClass.LoadToDatatable(sql, False)
        If IsDBNull(Searchdt.Rows(0)(0)) Then
            CommissionCriteriaComboBoxEdit.EditValue = Nothing
        Else
            CommissionCriteriaComboBoxEdit.SelectedIndex = CInt(Searchdt.Rows(0)(0)) - 1
        End If
        If IsDBNull(Searchdt.Rows(0)(1)) Then
            CommissionValueTextEdit.EditValue = Nothing
        Else
            CommissionValueTextEdit.EditValue = CDec(Searchdt.Rows(0)(1))
        End If
        If IsDBNull(Searchdt.Rows(0)(2)) Then
            UnitBuyingPriceTextEdit.EditValue = Nothing
        Else
            UnitBuyingPriceTextEdit.EditValue = CDec(Searchdt.Rows(0)(2))
        End If
        dt = New DataTable
        sql = "SELECT unitName,coalesce(titlePrice,0),unitSellingPriceMember,unitSellingPriceNonMember,landSize,coalesce(sellingPriceSixPlan,0),coalesce(sellingPriceTwelvePlan,0),ID "
        sql &= "FROM tblUnits WHERE projectId=@pid AND (sold = 0)"
        AddParams("@pid", CInt(ProjectsLookUpEdit.EditValue))
        dt = AppClass.LoadToDatatable(sql)
        For Each row In dt.Rows
            Populate(row(0), row(1), row(2), row(3), row(4), row(5), row(6), row(7))
        Next
        IsEdit = True
        DeleteSimpleButton.Enabled = True
    End Sub
    Sub Populate(UnitName As String, TitlePrice As Decimal, MemberSale As Decimal, NonMemberSale As Decimal, LandSize As String, SixPlan As Decimal, TwelvePlan As Decimal, UnitId As Integer)
        Dim row As String() = New String() {UnitName.ToString, TitlePrice.ToString("N"), MemberSale.ToString("N"), NonMemberSale.ToString("N"), LandSize, SixPlan.ToString("N"), TwelvePlan.ToString("N"), UnitId}
        DataGridView.Rows.Add(row)
    End Sub
    Sub Edit()
        Using Connection As New SqlConnection(connstr)
            With Connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With
            Using MyTransaction As SqlTransaction = Connection.BeginTransaction
                Try

                    For i = 0 To DataGridView.Rows.Count - 1
                        sql = "UPDATE tblUnits SET unitName=@name,unitBuyingPrice=@bp,unitSellingPriceMember=@member,unitSellingPriceNonMember=@nonmember, "
                        sql &= "titlePrice=@title,landSize=@size,commisionType=@commtype,commission=@comm,sellingPriceSixPlan=@six,sellingPriceTwelvePlan=@twelve "
                        sql &= "WHERE ID=@id"
                        Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@name", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(0).Value
                                .Parameters.Add(New SqlParameter("@bp", SqlDbType.Decimal)).Value = ProcessEditValue(UnitBuyingPriceTextEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@member", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(2).Value
                                .Parameters.Add(New SqlParameter("@nonmember", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(3).Value
                                .Parameters.Add(New SqlParameter("@title", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(1).Value
                                .Parameters.Add(New SqlParameter("@size", SqlDbType.VarChar)).Value = DataGridView.Rows(i).Cells(4).Value
                                .Parameters.Add(New SqlParameter("@commtype", SqlDbType.Decimal)).Value = If(CommissionCriteriaComboBoxEdit.EditValue IsNot Nothing, CommissionCriteriaComboBoxEdit.SelectedIndex + 1, DBNull.Value)
                                .Parameters.Add(New SqlParameter("@comm", SqlDbType.Decimal)).Value = ProcessEditValue(CommissionValueTextEdit.EditValue)
                                .Parameters.Add(New SqlParameter("@six", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(5).Value
                                .Parameters.Add(New SqlParameter("@twelve", SqlDbType.Decimal)).Value = DataGridView.Rows(i).Cells(6).Value
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(7).Value
                                .ExecuteNonQuery()
                            End With
                        End Using
                    Next

                    MyTransaction.Commit()
                    AppClass.ShowNotification("Edited Successfully")
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
    Sub Delete()
        Using Connection As New SqlConnection(connstr)
            With Connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With
            Using MyTransaction As SqlTransaction = Connection.BeginTransaction
                Try

                    For i = 0 To DataGridView.Rows.Count - 1
                        sql = "DELETE FROM tblUnits WHERE ID=@id"
                        Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = DataGridView.Rows(i).Cells(7).Value
                                .ExecuteNonQuery()
                            End With
                        End Using
                    Next

                    Dim UnitsTotal As Integer = 0
                    sql = "SELECT COUNT(ID) FROM tblUnits WHERE projectId=@pid"
                    Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                        cmd.Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = CInt(ProjectsLookUpEdit.EditValue)
                        Dim result As Object = cmd.ExecuteScalar()

                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            UnitsTotal = Convert.ToInt32(result)
                        End If
                    End Using

                    sql = "UPDATE tblProjects SET unitsCreated=@created WHERE ID=@id"
                    Using cmd As New SqlCommand(sql, Connection, MyTransaction)
                        With cmd
                            .Parameters.Add(New SqlParameter("@created", SqlDbType.Int)).Value = UnitsTotal
                            .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = CInt(ProjectsLookUpEdit.EditValue)
                            .ExecuteNonQuery()
                        End With
                    End Using

                    MyTransaction.Commit()
                    AppClass.ShowNotification("Deleted Successfully")
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
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
    Private Sub ProjectsLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles ProjectsLookUpEdit.EditValueChanged
        If ProjectsLookUpEdit.EditValue IsNot Nothing Then
            AddParams("@id", CInt(ProjectsLookUpEdit.EditValue))
            UnitsTotalTextEdit.EditValue = CInt(AppClass.FetchDBValue("SELECT noOfUnits FROM tblProjects WHERE (ID=@id)"))
            AddParams("@id", CInt(ProjectsLookUpEdit.EditValue))
            CreatedUnitsTextEdit.EditValue = CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblUnits WHERE (projectId=@id)"))
        End If
    End Sub
    Private Sub UnitsToCreateTextEdit_EditValueChanged(sender As Object, e As EventArgs) Handles UnitsToCreateTextEdit.Leave
        If UnitsToCreateTextEdit.EditValue IsNot Nothing AndAlso UnitsTotalTextEdit.EditValue IsNot Nothing Then
            If (CInt(UnitsToCreateTextEdit.EditValue) > CInt(UnitsTotalTextEdit.EditValue)) Then
                AppClass.ShowError("Cannot create more units than available")
                Return
            End If
            RemainingUnitsTextEdit.EditValue = CInt(UnitsTotalTextEdit.EditValue) - CInt(UnitsToCreateTextEdit.EditValue)
            DataGridView.Rows.Clear()
            For i = 0 To CInt(UnitsToCreateTextEdit.EditValue) - 1
                Dim row As String() = New String() {i + 1}
                DataGridView.Rows.Add(row)
            Next
        End If
    End Sub
    Private Sub MemberSellingPriceTextEdit_Leave(sender As Object, e As EventArgs) Handles MemberSellingPriceTextEdit.Leave
        If MemberSellingPriceTextEdit.EditValue IsNot Nothing AndAlso DataGridView.Rows.Count > 0 Then
            FillGridWithValues(2, CDec(MemberSellingPriceTextEdit.EditValue).ToString("N"))
        End If
    End Sub
    Private Sub NoneMemberSellingPriceTextEdit_EditValueChanged(sender As Object, e As EventArgs) Handles NoneMemberSellingPriceTextEdit.Leave
        If NoneMemberSellingPriceTextEdit.EditValue IsNot Nothing AndAlso DataGridView.Rows.Count > 0 Then
            FillGridWithValues(3, CDec(NoneMemberSellingPriceTextEdit.EditValue).ToString("N"))
        End If
    End Sub
    Private Sub SellingPrice6monthPlanTextEdit_EditValueChanged(sender As Object, e As EventArgs) Handles SellingPrice6monthPlanTextEdit.Leave
        If SellingPrice6monthPlanTextEdit.EditValue IsNot Nothing AndAlso DataGridView.Rows.Count > 0 Then
            FillGridWithValues(5, CDec(SellingPrice6monthPlanTextEdit.EditValue).ToString("N"))
        End If
    End Sub
    Private Sub SellingPrice12MonthPlanTextEdit_EditValueChanged(sender As Object, e As EventArgs) Handles SellingPrice12MonthPlanTextEdit.Leave
        If SellingPrice12MonthPlanTextEdit.EditValue IsNot Nothing AndAlso DataGridView.Rows.Count > 0 Then
            FillGridWithValues(6, CDec(SellingPrice12MonthPlanTextEdit.EditValue).ToString("N"))
        End If
    End Sub
    Private Sub TitleFeeTextEdit_EditValueChanged(sender As Object, e As EventArgs) Handles TitleFeeTextEdit.Leave
        If TitleFeeTextEdit.EditValue IsNot Nothing AndAlso DataGridView.Rows.Count > 0 Then
            FillGridWithValues(1, CDec(TitleFeeTextEdit.EditValue).ToString("N"))
        End If
    End Sub
    Private Sub LandSizeTextEdit_Leave(sender As Object, e As EventArgs) Handles LandSizeTextEdit.Leave
        If LandSizeTextEdit.EditValue IsNot Nothing AndAlso DataGridView.Rows.Count > 0 Then
            FillGridWithValues(4, LandSizeTextEdit.EditValue.ToString)
        End If
    End Sub
    Private Sub CommissionCriteriaComboBoxEdit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CommissionCriteriaComboBoxEdit.SelectedIndexChanged
        If CommissionCriteriaComboBoxEdit.EditValue IsNot Nothing Then
            LayoutControlItem13.Text = If(CommissionCriteriaComboBoxEdit.SelectedIndex = 0, "Enter Amount:", "Enter percentage value:")
        End If
    End Sub
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If Not Datavalidation() Then
            Return
        End If

        If Not ValidateDataGridViewColumn(DataGridView, 1, "Title fee cannot be empty", True) Then
            Exit Sub
        End If
        If Not ValidateDataGridViewColumn(DataGridView, 2, "Member selling fee cannot be empty", True) Then
            Exit Sub
        End If
        If Not ValidateDataGridViewColumn(DataGridView, 3, "None Member selling fee cannot be empty", True) Then
            Exit Sub
        End If
        If Not ValidateDataGridViewColumn(DataGridView, 4, "Size cannot be empty", False) Then
            Exit Sub
        End If
        If Not ValidateDataGridViewColumn(DataGridView, 5, "6 month plan selling fee cannot be empty", True) Then
            Exit Sub
        End If
        If Not ValidateDataGridViewColumn(DataGridView, 6, "12 month plan selling fee cannot be empty", True) Then
            Exit Sub
        End If

        If Not IsEdit Then
            Save()
        Else
            Edit()
        End If
    End Sub
    Private Sub FindSimpleButton_Click(sender As Object, e As EventArgs) Handles FindSimpleButton.Click
        Dim SearchBy As String = "Search by project name"
        Using frm As New SearchForm(SearchBy, "spSearchUnits")
            If frm.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                Find(frm.DataGridView.CurrentRow.Cells(0).Value)
            End If
        End Using
    End Sub
    Private Sub DeleteSimpleButton_Click(sender As Object, e As EventArgs) Handles DeleteSimpleButton.Click
        For i = 0 To DataGridView.Rows.Count - 1
            AddParams("@id", DataGridView.Rows(i).Cells(7).Value)
            Dim IsAchrived As Boolean = AppClass.FetchDBValue("SELECT archived FROM tblUnits WHERE ID=@id")

            AddParams("@uid", DataGridView.Rows(i).Cells(7).Value)
            Dim IsReserved As Boolean = CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblReservations WHERE (unitId=@uid) AND (released=0)")) > 0

            If IsAchrived OrElse IsReserved Then
                AppClass.ShowError(DataGridView.Rows(i).Cells(0).Value.ToString & " cannot be deleted as its either archived or reserved")
                Return
            End If
        Next
        If AppClass.AlertQuestion("Are you sure you want to delete this units?") = DialogResult.Yes Then
            Delete()
        End If
    End Sub
#End Region
End Class
