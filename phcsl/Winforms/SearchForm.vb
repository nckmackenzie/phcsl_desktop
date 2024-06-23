Public Class SearchForm
    Private SP As String
    Sub New(Searchby As String, StoredProcedure As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        AppClass.FormatDatagridView(DataGridView)
        SearchControl1.Properties.NullValuePrompt = Searchby.ToString
        SP = StoredProcedure
        OKSimpleButton.Enabled = False
        AddParams("@action", "all")
        AppClass.LoadToGrid(SP, DataGridView, True)
        If CInt(DataGridView.Rows.Count > 0) Then
            For Each col As DataGridViewColumn In DataGridView.Columns
                col.Width = 140
            Next
            DataGridView.Columns(0).Visible = False
        End If
    End Sub
    Private Sub SearchControl1_TextChanged(sender As Object, e As EventArgs) Handles SearchControl1.TextChanged
        AddParams("@action", "search")
        AddParams("@param", SearchControl1.EditValue)
        AppClass.LoadToGrid(SP, DataGridView, True)
    End Sub
    Private Sub DataGridView_RowsAdded(sender As Object, e As DataGridViewRowsAddedEventArgs) Handles DataGridView.RowsAdded
        If CInt(DataGridView.Rows.Count) > 0 Then
            OKSimpleButton.Enabled = True
        Else
            OKSimpleButton.Enabled = False
        End If
    End Sub
    Private Sub DataGridView_RowsRemoved(sender As Object, e As DataGridViewRowsRemovedEventArgs) Handles DataGridView.RowsRemoved
        If CInt(DataGridView.Rows.Count) > 0 Then
            OKSimpleButton.Enabled = True
        Else
            OKSimpleButton.Enabled = False
        End If
    End Sub
End Class