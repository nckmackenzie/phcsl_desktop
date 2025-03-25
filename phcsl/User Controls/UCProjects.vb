Public Class UCProjects
    Private Shared _instance As UCProjects
    Private IsEdit As Boolean
    Private ID As Integer
    Public Shared ReadOnly Property Instance As UCProjects
        Get
            If _instance Is Nothing Then _instance = New UCProjects()
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
        DeleteSimpleButton.Enabled = False
        IsEdit = False
        ID = Nothing
        LandRadioButton.Checked = True
        TitleFeeInclusiveCheckEdit.Checked = False
        AppClass.LoadToLookUpEdit("SELECT ID,UPPER(vendorName) AS VendorName FROM tblVendor order by VendorName", VendorLookUpEdit, "VendorName", "ID")
        AppClass.ClearItems(LayoutControl1)
    End Sub
    Function Datavalidation() As Boolean
        errmsg = Nothing
        If NameTextEdit.EditValue Is Nothing Then
            errmsg = "Enter project name"
            NameTextEdit.Focus()
        ElseIf SerialNoTextEdit.EditValue Is Nothing Then
            errmsg = "Enter project serial no"
            SerialNoTextEdit.Focus()
        ElseIf ReferenceTextEdit.EditValue Is Nothing Then
            errmsg = "Enter project reference"
            SerialNoTextEdit.Focus()
        ElseIf NumberOfUnitsTextEdit.EditValue Is Nothing Then
            errmsg = "Enter No of units"
            NumberOfUnitsTextEdit.Focus()
        End If
        If errmsg IsNot Nothing Then
            AppClass.ShowError(errmsg)
            Return False
        Else
            Return True
        End If
    End Function
    Sub Find(fid As Integer)
        IsEdit = True
        AppClass.ClearItems(LayoutControl1)
        AddParams("@id", fid)
        Dim Searchdt As DataTable = AppClass.LoadToDatatable("SELECT * FROM tblProjects WHERE ID=@id", False)
        ID = CInt(Searchdt.Rows(0)(0))
        SerialNoTextEdit.EditValue = CStr(Searchdt.Rows(0)(1)).ToUpper
        NameTextEdit.EditValue = CStr(Searchdt.Rows(0)(2)).ToUpper
        If CInt(Searchdt.Rows(0)(3)) = 1 Then
            LandRadioButton.Checked = True
        Else
            HousingRadioButton.Checked = True
        End If
        NumberOfUnitsTextEdit.EditValue = Searchdt.Rows(0)(4)
        VendorLookUpEdit.EditValue = If(IsDBNull(Searchdt.Rows(0)(5)), Nothing, Searchdt.Rows(0)(5))
        ReferenceTextEdit.EditValue = If(IsDBNull(Searchdt.Rows(0)(8)), Nothing, Searchdt.Rows(0)(8).ToString.ToUpper)
        If IsDBNull(Searchdt.Rows(0)(9)) Then
            DownpaymentTextEdit.EditValue = Nothing
        Else
            DownpaymentTextEdit.EditValue = CDec(Searchdt.Rows(0)(9))
        End If
        If IsDBNull(Searchdt.Rows(0)(10)) Then
            AcreageTextEdit.EditValue = Nothing
        Else
            AcreageTextEdit.EditValue = CDec(Searchdt.Rows(0)(10))
        End If
        If IsDBNull(Searchdt.Rows(0)(11)) Then
            BuyingCostTextEdit.EditValue = Nothing
        Else
            BuyingCostTextEdit.EditValue = CDec(Searchdt.Rows(0)(11))
        End If
        If IsDBNull(Searchdt.Rows(0)(12)) Then
            FindersFeeTextEdit.EditValue = Nothing
        Else
            FindersFeeTextEdit.EditValue = CDec(Searchdt.Rows(0)(12))
        End If
        TitleFeeInclusiveCheckEdit.Checked = CBool(Searchdt.Rows(0)(13))
        'PurchaseDateEdit.EditValue = If(IsDBNull(Searchdt.Rows(0)(14)), Nothing, CDate(Searchdt.Rows(0)(14)))
        If IsDBNull(Searchdt.Rows(0)(14)) Then
            PurchaseDateEdit.EditValue = Nothing
        Else
            PurchaseDateEdit.EditValue = CDate(Searchdt.Rows(0)(14))
        End If
        DeleteSimpleButton.Enabled = True
    End Sub
#End Region
#Region "EVENTS"
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If Not Datavalidation() Then
            Return
        End If

        AddParams("@serial", Trim(SerialNoTextEdit.EditValue.ToString.ToLower))
        AddParams("@id", ID)
        If (CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblprojects WHERE trim(lower(serialNo))=@serial AND ID <> @id")) > 0) Then
            AppClass.ShowNotification("Project Serial No already exists!")
            SerialNoTextEdit.SelectAll()
            Return
        End If

        AddParams("@ref", Trim(ReferenceTextEdit.EditValue.ToString.ToLower))
        AddParams("@id", ID)
        If (CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblprojects WHERE trim(lower(projectReference))=@ref AND ID <> @id")) > 0) Then
            AppClass.ShowNotification("Project Serial No already exists!")
            SerialNoTextEdit.SelectAll()
            Return
        End If

        AddParams("@serial", ProcessEditValue(SerialNoTextEdit.EditValue))
        AddParams("@name", ProcessEditValue(NameTextEdit.EditValue))
        AddParams("@type", If(LandRadioButton.Checked, 1, 0))
        AddParams("@units", ProcessEditValue(NumberOfUnitsTextEdit.EditValue))
        AddParams("@vendor", ProcessEditValue(VendorLookUpEdit.EditValue))
        AddParams("@ref", ProcessEditValue(ReferenceTextEdit.EditValue))
        AddParams("@minimum", ProcessEditValue(DownpaymentTextEdit.EditValue))
        AddParams("@acr", ProcessEditValue(AcreageTextEdit.EditValue))
        AddParams("@buyingc", ProcessEditValue(BuyingCostTextEdit.EditValue))
        AddParams("@fee", ProcessEditValue(FindersFeeTextEdit.EditValue))
        AddParams("@inc", TitleFeeInclusiveCheckEdit.Checked)
        AddParams("@pdate", ProcessEditValue(PurchaseDateEdit.EditValue))
        If IsEdit Then
            AddParams("@id", ID)
        End If
        If Not IsEdit Then
            sql = "INSERT INTO tblProjects (serialNo,projectName,projectType,noOfUnits,vendorId,projectReference,minimumDownPayment,acreage,buyingCost,finderFee,titleFeeInclusive,purchaseDate)"
            sql &= "VALUES(@serial,@name,@type,@units,@vendor,@ref,@minimum,@acr,@buyingc,@fee,@inc,@pdate)"
        Else
            sql = "UPDATE tblProjects SET serialNo=@serial,projectName=@name,projectType=@type,noOfUnits=@units,vendorId=@vendor,"
            sql &= "projectReference=@ref,minimumDownPayment=@minimum,acreage=@acr,buyingCost=@buyingc,finderFee=@fee,titleFeeInclusive=@inc,purchaseDate=@pdate"
            sql &= " WHERE (ID=@id)"
        End If
        AppClass.ExecQuery(sql)
        If RecordCount > 0 Then
            AppClass.ShowNotification(If(IsEdit, "Edited Successfully", "Saved Successfully!"))
            Reset()
        End If
    End Sub
    Private Sub FindSimpleButton_Click(sender As Object, e As EventArgs) Handles FindSimpleButton.Click
        Dim SearchBy As String = "Search by name,serial no,reference..."
        Using frm As New SearchForm(SearchBy, "spSearchProjects")
            If frm.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                Find(frm.DataGridView.CurrentRow.Cells(0).Value)
            End If
        End Using
    End Sub
    Private Sub DeleteSimpleButton_Click(sender As Object, e As EventArgs) Handles DeleteSimpleButton.Click
        AddParams("@pid", ID)
        If CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblUnits WHERE projectId=@pid")) > 0 Then
            AppClass.ShowError("Cannot delete project as its linked to created units.")
            Return
        End If

        If AppClass.AlertQuestion("Are you sure you want to delete this project?") = DialogResult.Yes Then
            AddParams("@id", ID)
            AppClass.ExecQuery("DELETE FROM tblProjects WHERE (ID=@id)")
            If RecordCount > 0 Then
                AppClass.ShowNotification("Deleted Successfully")
                Reset()
            End If
        End If
    End Sub
#End Region
End Class
