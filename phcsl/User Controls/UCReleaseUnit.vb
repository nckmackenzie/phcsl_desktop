Imports System.Data.SqlClient
Imports DevExpress.XtraEditors

Public Class UCReleaseUnit
    Private Shared _instance As UCReleaseUnit
    Private memberNo As Integer
    Private id As Integer
    Public Shared ReadOnly Property Instance As UCReleaseUnit
        Get
            If _instance Is Nothing Then _instance = New UCReleaseUnit()
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
        memberNo = Nothing
        MemberSearchLookUpEdit.EditValue = Nothing
        ReleaseDateEdit.EditValue = Date.Now
        AppClass.ClearItems(LayoutControl1)
        sql = "SELECT M.memberNo,M.memberID AS [MemberID],UPPER(M.memberName) AS [MemberName],M.idNo AS [IDNo],"
        sql &= "A.contact AS [Contact] FROM tblMember M LEFT OUTER JOIN tblMemberAddress A ON M.memberNo=A.memberNo ORDER BY M.memberID"
        AppClass.LoadToSearchLookUpEdit(sql, MemberSearchLookUpEdit, "MemberName", "memberNo")
    End Sub
    Function Datavalidation() As Boolean
        errmsg = ""
        If memberNo = Nothing Then
            errmsg = "Select Member"
        ElseIf String.IsNullOrEmpty(AmountPaidTextEdit.Text) Then
            errmsg = "Select Unit"
            UnitLookUpEdit.Focus()
        ElseIf ProjectLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select Project"
            ProjectLookUpEdit.Focus()
        ElseIf UnitLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select Unit"
            UnitLookUpEdit.Focus()
        ElseIf PenaltyTextEdit.EditValue IsNot Nothing AndAlso (CDec(PenaltyTextEdit.EditValue) > CDec(AmountPaidTextEdit.EditValue)) Then
            errmsg = "Penalty cannot be more than amount paid"
            With PenaltyTextEdit
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
    Function GetAmountToRefund() As Decimal
        Dim AmountPaid As Decimal = CDec(AmountPaidTextEdit.EditValue)
        Dim Penalty As Decimal = If(PenaltyTextEdit.EditValue Is Nothing, 0, PenaltyTextEdit.EditValue)
        Dim RefundedAmount As Decimal = AmountPaid - Penalty
        Return RefundedAmount
    End Function
    Sub save()

        AddParams("@id", ProjectLookUpEdit.EditValue)
        Dim soldOut As Boolean = CBool(AppClass.FetchDBValue("SELECT soldOut From tblProjects WHERE (ID=@id)"))
        Dim transferType As Integer
        If TransferComboBoxEdit.SelectedIndex = 0 Then '//to deposit
            transferType = 11
        ElseIf TransferComboBoxEdit.SelectedIndex = 1 Then
            transferType = 10
        End If

        Dim myTransaction As SqlTransaction = Nothing
        'id = CInt(AppClass.GenerateDBID("SELECT COUNT(ID) FROM tblTransfers", "SELECT TOP 1 ID FROM tblTransfers ORDER BY ID DESC"))
        id = AppClass.GenerateDBID("tblTransfers")
        AppClass.GenerateID("SELECT MAX(RIGHT(reference,5)) AS ref FROM tblTransfers", "ref", txtRef, "00001", "00000")
        Try
            Using connection = New SqlConnection(connstr)
                With connection
                    If .State = ConnectionState.Closed Then
                        .Open()
                    End If
                End With

                myTransaction = connection.BeginTransaction
                sql = "INSERT INTO tblTransfers (ID,transferType,transferDate,transferFrom,transferTo,amount,projectId,unitId,reference) "
                sql &= "VALUES(@id,@type,@date,@from,@to,@amount,@pid,@uid,@ref)"
                Using cmd = New SqlCommand(sql, connection, myTransaction)
                    With cmd
                        .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = id
                        .Parameters.Add(New SqlParameter("@type", SqlDbType.Int)).Value = transferType
                        .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = CDate(ReleaseDateEdit.EditValue).Date
                        .Parameters.Add(New SqlParameter("@from", SqlDbType.Int)).Value = memberNo
                        .Parameters.Add(New SqlParameter("@to", SqlDbType.Int)).Value = memberNo
                        .Parameters.Add(New SqlParameter("@amount", SqlDbType.Decimal)).Value = CDec(RefundAmountTextEdit.Text)
                        .Parameters.Add(New SqlParameter("@pid", SqlDbType.Int)).Value = ProjectLookUpEdit.EditValue
                        .Parameters.Add(New SqlParameter("@uid", SqlDbType.Int)).Value = UnitLookUpEdit.EditValue
                        .Parameters.Add(New SqlParameter("@ref", SqlDbType.VarChar)).Value = txtRef.Text.ToLower.ToString
                        .ExecuteNonQuery()
                    End With
                End Using

                sql = "INSERT INTO tblReleaseUnitPenalties (TransferId,PenaltyImposed) VALUES(@tid,@penaty)"
                Using cmd = New SqlCommand(sql, connection, myTransaction)
                    With cmd
                        .Parameters.Add(New SqlParameter("@tid", SqlDbType.Int)).Value = id
                        .Parameters.Add(New SqlParameter("@penaty", SqlDbType.Decimal)).Value = CDec(PenaltyTextEdit.EditValue)
                        .ExecuteNonQuery()
                    End With
                End Using

                sql = "UPDATE tblUnits SET sold=@sold,soldDate=@date,memberNo=@no,nonMemberName=@name,titlePaid=@tpaid,"
                sql &= "unitPaid=@upaid WHERE (ID=@id)"
                Using cmd = New SqlCommand(sql, connection, myTransaction)
                    With cmd
                        .Parameters.Add(New SqlParameter("@sold", SqlDbType.Bit)).Value = False
                        .Parameters.Add(New SqlParameter("@date", SqlDbType.Date)).Value = DBNull.Value
                        .Parameters.Add(New SqlParameter("@no", SqlDbType.Int)).Value = DBNull.Value
                        .Parameters.Add(New SqlParameter("@name", SqlDbType.NVarChar)).Value = DBNull.Value
                        .Parameters.Add(New SqlParameter("@tpaid", SqlDbType.Decimal)).Value = 0
                        .Parameters.Add(New SqlParameter("@upaid", SqlDbType.Decimal)).Value = 0
                        .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = UnitLookUpEdit.EditValue
                        .ExecuteNonQuery()
                    End With
                End Using
                sql = "DELETE FROM tblUnitSale WHERE (unitId=@id)"
                Using cmd = New SqlCommand(sql, connection, myTransaction)
                    cmd.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = UnitLookUpEdit.EditValue
                    cmd.ExecuteNonQuery()
                End Using
                sql = "DELETE FROM tblUnitPayment WHERE (unitId=@id)"
                Using cmd = New SqlCommand(sql, connection, myTransaction)
                    cmd.Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = UnitLookUpEdit.EditValue
                    cmd.ExecuteNonQuery()
                End Using
                If soldOut = True Then
                    sql = "UPDATE tblProjects SET soldOut=@false WHERE (ID=@id)"
                    Using cmd = New SqlCommand(sql, connection, myTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@false", SqlDbType.Bit).Value = False
                            .Parameters.Add("@id", SqlDbType.Int).Value = ProjectLookUpEdit.EditValue
                            .ExecuteNonQuery()
                        End With
                    End Using
                End If
                sql = "INSERT INTO tblLogs VALUES(@uid,@act,@actdate)"
                Using cmd = New SqlCommand(sql, connection, myTransaction)
                    With cmd
                        .Parameters.Clear()
                        .Parameters.Add("@uid", SqlDbType.Int).Value = LogedUserID
                        .Parameters.Add("@act", SqlDbType.NVarChar).Value = "Released Unit " & UnitLookUpEdit.Text & " For Project " & ProjectLookUpEdit.Text & " From " & MemberSearchLookUpEdit.Text
                        .Parameters.Add("@actdate", SqlDbType.Date).Value = Date.Now.Date
                        .ExecuteNonQuery()
                    End With
                End Using
                myTransaction.Commit()
                myTransaction.Dispose()
            End Using
            If TransferComboBoxEdit.SelectedIndex = 0 Then '//to deposit
                AppClass.AddToLedger(CDate(ReleaseDateEdit.EditValue).Date, "unit payment", "released unit", 0, CDec(RefundAmountTextEdit.Text), ProjectLookUpEdit.EditValue, 5, 15, id)
                AppClass.AddToLedger(CDate(ReleaseDateEdit.EditValue).Date, "deposits", "released unit", CDec(RefundAmountTextEdit.Text), 0, ProjectLookUpEdit.EditValue, 3, 15, id)
            ElseIf TransferComboBoxEdit.SelectedIndex = 1 Then '//to shares
                AppClass.AddToLedger(CDate(ReleaseDateEdit.EditValue).Date, "unit payment", "released unit", 0, CDec(RefundAmountTextEdit.Text), ProjectLookUpEdit.EditValue, 5, 15, id)
                AppClass.AddToLedger(CDate(ReleaseDateEdit.EditValue).Date, "shares", "released unit", CDec(RefundAmountTextEdit.Text), 0, ProjectLookUpEdit.EditValue, 4, 15, id)
            End If
            AppClass.ShowNotification("Released Successfully!")
            Reset()
        Catch ex As Exception
            If myTransaction IsNot Nothing Then
                myTransaction.Rollback()
            End If
            AppClass.ShowError(ex.Message)
        End Try
    End Sub
#End Region
#Region "EVENTS"
    Private Sub MemberSearchLookUpEdit_EditValueChanged(sender As Object, e As EventArgs) Handles MemberSearchLookUpEdit.EditValueChanged
        If MemberSearchLookUpEdit.EditValue IsNot Nothing Then
            memberNo = MemberSearchLookUpEdit.EditValue
            AddParams("@no", memberNo)
            AppClass.LoadToLookUpEdit("SELECT DISTINCT P.ID AS ID,UPPER(P.projectName) AS projectName " &
                            "FROM   tblUnits AS U INNER JOIN tblProjects AS P ON U.projectId = P.ID INNER JOIN tblUnitSale S ON U.ID=S.unitId " &
                            "WHERE  (U.memberNo = @no)", ProjectLookUpEdit, "projectName", "ID")
        End If
    End Sub
    Private Sub ProjectLookUpEdit_EditValuehanged(sender As Object, e As EventArgs) Handles ProjectLookUpEdit.EditValueChanged
        If ProjectLookUpEdit IsNot Nothing AndAlso MemberSearchLookUpEdit.EditValue IsNot Nothing Then
            AddParams("@id", ProjectLookUpEdit.EditValue)
            AddParams("@no", memberNo)
            AddParams("@false", False)
            AppClass.LoadToLookUpEdit("SELECT ID,UPPER(unitName) AS unitName FROM tblUnits WHERE (projectId=@id) AND (memberNo=@no)", UnitLookUpEdit _
                            , "unitName", "ID")
        End If
    End Sub
    Private Sub cbxUnit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles UnitLookUpEdit.EditValueChanged
        If UnitLookUpEdit.EditValue IsNot Nothing Then
            AddParams("@id", UnitLookUpEdit.EditValue)
            AmountPaidTextEdit.EditValue = CDec(AppClass.FetchDBValue("SELECT ISNULL(SUM(amountPaid),0) FROM tblUnitPayment WHERE (unitId=@id)")).ToString("N")
        End If
    End Sub
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If Not Datavalidation() Then
            Exit Sub
        End If
        RefundAmountTextEdit.EditValue = GetAmountToRefund()
        If XtraMessageBox.Show("Are You Sure Yoy Want To Release This Unit? This Cannot Be Undone.Proceed?", "Alert" _
                               , MessageBoxButtons.YesNo, MessageBoxIcon.Question) = System.Windows.Forms.DialogResult.Yes Then
            save()
        End If
    End Sub
    Private Sub txtPenalty_TextChanged(sender As Object, e As EventArgs) Handles PenaltyTextEdit.EditValueChanged
        If PenaltyTextEdit.EditValue IsNot Nothing AndAlso AmountPaidTextEdit.EditValue IsNot Nothing Then
            RefundAmountTextEdit.EditValue = GetAmountToRefund.ToString("N")
        End If
    End Sub
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
#End Region
End Class
