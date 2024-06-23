Imports System.Data.SqlClient
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Text.RegularExpressions
Imports DevExpress.XtraEditors

Public Class UCMembers
    Private Shared _instance As UCMembers
    Public Shared ReadOnly Property Instance As UCMembers
        Get
            If _instance Is Nothing Then _instance = New UCMembers()
            Return _instance
        End Get
    End Property
    Private MemberNo As Integer
    Private IsEdit As Boolean
#Region "SUBS"
    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Reset()
    End Sub
    Sub Reset()
        DataGridView.Rows.Clear()
        AppClass.ClearItems(LayoutControl1)
        AppClass.FormatDatagridView(DataGridView)
        NewRegistrationRadioButton.Checked = True
        MemberLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always
        NonMemberLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        ReferedCheckEdit.Checked = False
        ReferTypeLayoutControlItem.Enabled = False
        AgentLayoutControlItem.Enabled = False
        'RegistrationDateEdit.EditValue = Date.Now
        AppClass.LoadToLookUpEdit("SELECT ID,genderName FROM tblGender", GenderComboBoxEdit, "genderName", "ID")
        AppClass.LoadToLookUpEdit("SELECT ID,statusName FROM tblMemberStatus", StatusComboBoxEdit, "statusName", "ID")
        AppClass.LoadToLookUpEdit("SELECT distinct upper(nonMemberName) As nonMemberName from tblUnits WHERE nonMemberName IS NOT NULL ORDER BY nonMemberName", NonMemberLookUpEdit, "nonMemberName", "nonMemberName")
        IsEdit = False
        DeleteSimpleButton.Enabled = False
        StatusComboBoxEdit.EditValue = 1
        MemberNoTextEdit.Text = "Unassigned"
        MemberNo = AppClass.GenerateDBID("tblMember", "memberNo")
        AddSimpleButton.Text = "Add Nominee"
        EmployementDetailsTextEdit.Properties.ReadOnly = True
    End Sub
    Function Datavalidation() As Boolean
        errmsg = ""
        If NewRegistrationRadioButton.Checked AndAlso MemberNameTextEdit.EditValue Is Nothing Then
            errmsg = "Enter Member Name"
            MemberNameTextEdit.Focus()
        ElseIf NonMemberRadioButton.Checked AndAlso NonMemberLookUpEdit.EditValue Is Nothing Then
            errmsg = "Select non member from dropdown"
            NonMemberLookUpEdit.Focus()
        ElseIf IDNoTextEdit.EditValue Is Nothing Then
            errmsg = "Enter ID Number"
            IDNoTextEdit.Focus()
        ElseIf ContactTextEdit.EditValue Is Nothing Then
            errmsg = "Enter Contact Information"
            ContactTextEdit.Focus()
        ElseIf SourceOfIncomeComboBoxEdit.EditValue Is Nothing Then
            errmsg = "Select Whether Business or Employed"
            SourceOfIncomeComboBoxEdit.Focus()
        ElseIf CDate(DobDateEdit.EditValue).Date > Date.Now Then
            errmsg = "Invalid Date Of Birth"
            DobDateEdit.Focus()
        ElseIf CDate(RegistrationDateEdit.EditValue).Date > Date.Now.Date Then
            errmsg = "Invalid Registration Date"
        ElseIf EmailTextEdit.EditValue IsNot Nothing AndAlso IsValidEmailFormat(EmailTextEdit.EditValue) = False Then
            errmsg = "Invalid Email Address Entered!"
            With EmailTextEdit
                .Focus()
                .SelectAll()
            End With
        ElseIf GenderComboBoxEdit.EditValue Is Nothing Then
            errmsg = "Select gender"
            GenderComboBoxEdit.Focus()
        ElseIf MembershipTypeComboBoxEdit.EditValue Is Nothing Then
            errmsg = "Select membership type"
            MembershipTypeComboBoxEdit.Focus()
        ElseIf DataGridView.Rows.Count > 0 AndAlso CDec(AppClass.GetGridTotal(DataGridView, 3)) < 100 Then
            errmsg = "Total Share For Nominee Should equal to 100%!"
        End If

        If errmsg <> "" Then
            AppClass.ShowNotification(errmsg.ToString)
            Return False
        Else
            Return True
        End If
    End Function
    Function IsValidEmailFormat(ByVal s As String) As Boolean
        Return Regex.IsMatch(s, "^([0-9a-zA-Z]([-\.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$")
    End Function
    Sub Save()
        Dim ReferalId As Integer = CInt(AppClass.GenerateDBID("tblReferals"))
        Using connection As New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Using MyTransaction As SqlTransaction = connection.BeginTransaction
                Try
                    sql = "INSERT INTO tblMember (memberNo,memberName,idNo,incomeSource,businessType,employmentDetails,membershipType"
                    sql &= ",photo,memberStatusId,dob,registrationDate,gender,refered,referalType,referal) VALUES(@no,@name,@id,@source,@businesstype,@edetails,@membertype,@photo"
                    sql &= ",@status,@dob,@regdate,@gender,@refered,@rtype,@referee)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@no", SqlDbType.Int).Value = MemberNo
                            If NewRegistrationRadioButton.Checked Then
                                .Parameters.Add("@name", SqlDbType.VarChar).Value = Trim(MemberNameTextEdit.EditValue.ToLower.ToString)
                            Else
                                .Parameters.Add("@name", SqlDbType.VarChar).Value = NonMemberLookUpEdit.EditValue.ToLower.ToString
                            End If
                            .Parameters.Add("@id", SqlDbType.VarChar).Value = Trim(IDNoTextEdit.EditValue.ToLower.ToString)
                            .Parameters.Add("@source", SqlDbType.Int).Value = SourceOfIncomeComboBoxEdit.SelectedIndex + 1
                            If KraPinTextEdit.EditValue IsNot Nothing Then
                                .Parameters.Add("@businesstype", SqlDbType.VarChar).Value = Trim(KraPinTextEdit.EditValue.ToLower.ToString)
                            Else
                                .Parameters.Add("@businesstype", SqlDbType.VarChar).Value = DBNull.Value
                            End If
                            If EmployementDetailsTextEdit.EditValue IsNot Nothing Then
                                .Parameters.Add("@edetails", SqlDbType.VarChar).Value = Trim(EmployementDetailsTextEdit.EditValue.ToLower.ToString)
                            Else
                                .Parameters.Add("@edetails", SqlDbType.VarChar).Value = DBNull.Value
                            End If
                            .Parameters.Add("@membertype", SqlDbType.Int).Value = MembershipTypeComboBoxEdit.SelectedIndex + 1
                            If Not (PictureEdit.Image Is Nothing) Then
                                Dim memstream As New MemoryStream()
                                PictureEdit.Image.Save(memstream, ImageFormat.Jpeg)
                                Dim pic() As Byte = memstream.ToArray()
                                .Parameters.Add("@photo", SqlDbType.Image).Value = pic
                            Else
                                .Parameters.Add("@photo", SqlDbType.Image).Value = DBNull.Value
                            End If
                            .Parameters.Add("@status", SqlDbType.Int).Value = StatusComboBoxEdit.EditValue
                            .Parameters.Add("@dob", SqlDbType.Date).Value = IIf(DobDateEdit.EditValue IsNot Nothing, CDate(DobDateEdit.EditValue).Date, DBNull.Value)
                            .Parameters.Add("@regdate", SqlDbType.Date).Value = IIf(RegistrationDateEdit.EditValue IsNot Nothing, CDate(RegistrationDateEdit.EditValue).Date, DBNull.Value)
                            .Parameters.Add("@gender", SqlDbType.Int).Value = GenderComboBoxEdit.EditValue
                            .Parameters.Add("@refered", SqlDbType.Bit).Value = ReferedCheckEdit.Checked
                            If ReferedCheckEdit.Checked = False Then
                                .Parameters.Add("@rtype", SqlDbType.Int).Value = DBNull.Value
                                .Parameters.Add("@referee", SqlDbType.Int).Value = DBNull.Value
                            Else
                                .Parameters.Add("@rtype", SqlDbType.Int).Value = ReferalTypeComboBoxEdit.SelectedIndex + 1
                                .Parameters.Add("@referee", SqlDbType.Int).Value = RefererLookUpEdit.EditValue
                            End If
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "INSERT INTO tblMemberAddress (memberNo,contact,email,postaladdress,alternativeContact,physicalAddress) "
                    sql &= "VALUES(@no,@contact,@email,@add,@alt,@physical)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@no", SqlDbType.Int).Value = MemberNo
                            .Parameters.Add("@contact", SqlDbType.VarChar).Value = Trim(ContactTextEdit.EditValue)
                            .Parameters.Add("@email", SqlDbType.VarChar).Value = IIf(EmailTextEdit.EditValue IsNot Nothing, EmailTextEdit.EditValue, DBNull.Value)
                            .Parameters.Add("@add", SqlDbType.VarChar).Value = IIf(AddressTextEdit.EditValue IsNot Nothing, AddressTextEdit.EditValue, DBNull.Value)
                            .Parameters.Add("@alt", SqlDbType.VarChar).Value = IIf(AlternativeContactTextEdit.EditValue IsNot Nothing, AlternativeContactTextEdit.EditValue, DBNull.Value)
                            .Parameters.Add("@physical", SqlDbType.VarChar).Value = IIf(PhyiscalAddressTextEdit.EditValue IsNot Nothing, PhyiscalAddressTextEdit.EditValue, DBNull.Value)
                            .ExecuteNonQuery()
                        End With
                    End Using

                    If ReferedCheckEdit.Checked = True AndAlso ReferalTypeComboBoxEdit.SelectedIndex = 1 Then
                        sql = "INSERT INTO tblReferals (ID,memberNo,paid,referedMember,amount) VALUES(@id,@no,@paid,@rm,@amnt)"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = ReferalId
                                .Parameters.Add(New SqlParameter("@no", SqlDbType.Int)).Value = RefererLookUpEdit.EditValue
                                .Parameters.Add(New SqlParameter("@paid", SqlDbType.Bit)).Value = False
                                .Parameters.Add(New SqlParameter("@rm", SqlDbType.Int)).Value = MemberNo
                                .Parameters.Add(New SqlParameter("@amnt", SqlDbType.Decimal)).Value = 100
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If

                    If ReferedCheckEdit.Checked = True AndAlso ReferalTypeComboBoxEdit.SelectedIndex = 0 Then
                        sql = "INSERT INTO tblReferals (ID,memberNo,paid,referedMember,amount) VALUES(@id,@no,@paid,@rm,@amnt)"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = ReferalId
                                .Parameters.Add(New SqlParameter("@no", SqlDbType.Int)).Value = RefererLookUpEdit.EditValue
                                .Parameters.Add(New SqlParameter("@paid", SqlDbType.Bit)).Value = False
                                .Parameters.Add(New SqlParameter("@rm", SqlDbType.Int)).Value = MemberNo
                                .Parameters.Add(New SqlParameter("@amnt", SqlDbType.Decimal)).Value = 100
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If

                    If DataGridView.Rows.Count > 0 Then
                        For i = 0 To DataGridView.Rows.Count - 1
                            sql = "INSERT INTO tblMemberNOK (memberNo,name,idNo,contact,share) VALUES(@no,@name,@id,@contact,@share)"
                            Using cmd = New SqlCommand(sql, connection, MyTransaction)
                                With cmd
                                    .Parameters.Clear()
                                    .Parameters.Add("@no", SqlDbType.Int).Value = MemberNo
                                    .Parameters.Add("@name", SqlDbType.VarChar).Value = DataGridView.Rows(i).Cells(0).Value
                                    .Parameters.Add("@id", SqlDbType.VarChar).Value = DataGridView.Rows(i).Cells(1).Value
                                    .Parameters.Add("@contact", SqlDbType.VarChar).Value = DataGridView.Rows(i).Cells(2).Value
                                    .Parameters.Add("@share", SqlDbType.Decimal).Value = CDec(DataGridView.Rows(i).Cells(3).Value)
                                    .ExecuteNonQuery()
                                End With
                            End Using
                        Next
                    End If

                    If NonMemberRadioButton.Checked Then
                        sql = "UPDATE tblUnits SET memberNo=@no,nonMemberName=@null WHERE nonMemberName=@name"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Clear()
                                .Parameters.Add("@no", SqlDbType.Int).Value = MemberNo
                                .Parameters.Add("@null", SqlDbType.VarChar).Value = DBNull.Value
                                .Parameters.Add("@name", SqlDbType.VarChar).Value = CStr(NonMemberLookUpEdit.EditValue).ToLower.ToString
                                .ExecuteNonQuery()
                            End With
                        End Using

                        sql = "UPDATE tblTitlePay SET memberType=@type,memberId=@no,nonMember=@null WHERE nonMember=@name"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Clear()
                                .Parameters.Add("@type", SqlDbType.Int).Value = 1
                                .Parameters.Add("@no", SqlDbType.Int).Value = MemberNo
                                .Parameters.Add("@null", SqlDbType.VarChar).Value = DBNull.Value
                                .Parameters.Add("@name", SqlDbType.VarChar).Value = CStr(NonMemberLookUpEdit.EditValue).ToUpper.ToString
                                .ExecuteNonQuery()
                            End With
                        End Using

                    End If

                    sql = "INSERT INTO tblLogs VALUES(@uid,@act,@actdate)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@uid", SqlDbType.Int).Value = LogedUserID
                            .Parameters.Add("@act", SqlDbType.NVarChar).Value = "Created New Member " & IIf(NewRegistrationRadioButton.Checked, Trim(MemberNameTextEdit.EditValue), NonMemberLookUpEdit.Text)
                            .Parameters.Add("@actdate", SqlDbType.Date).Value = Date.Now.Date
                            .ExecuteNonQuery()
                        End With
                    End Using

                    MyTransaction.Commit()
                    AppClass.ShowNotification("Saved successfully")
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
    Sub Edit()
        Dim ReferalId As Integer = CInt(AppClass.GenerateDBID("tblReferals"))
        Using connection = New SqlConnection(connstr)
            With connection
                If .State = ConnectionState.Closed Then
                    .Open()
                End If
            End With

            Using MyTransaction = connection.BeginTransaction
                Try
                    sql = "UPDATE tblMember SET memberName=@name,idNo=@id,incomeSource=@source,businessType=@businesstype,"
                    sql &= "employmentDetails=@edetails,membershipType=@membertype,photo=@photo,memberStatusId=@status,dob=@dob,"
                    sql &= "registrationDate=@regdate,gender=@gender,refered=@refered,referalType=@rtype,referal=@referee "
                    sql &= "WHERE memberNo=@no"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@name", SqlDbType.VarChar).Value = Trim(MemberNameTextEdit.EditValue.ToLower.ToString)
                            .Parameters.Add("@id", SqlDbType.VarChar).Value = Trim(IDNoTextEdit.EditValue.ToLower.ToString)
                            .Parameters.Add("@source", SqlDbType.Int).Value = SourceOfIncomeComboBoxEdit.SelectedIndex + 1
                            If KraPinTextEdit.EditValue IsNot Nothing Then
                                .Parameters.Add("@businesstype", SqlDbType.VarChar).Value = Trim(KraPinTextEdit.EditValue.ToLower.ToString)
                            Else
                                .Parameters.Add("@businesstype", SqlDbType.VarChar).Value = DBNull.Value
                            End If
                            If EmployementDetailsTextEdit.EditValue IsNot Nothing Then
                                .Parameters.Add("@edetails", SqlDbType.VarChar).Value = Trim(EmployementDetailsTextEdit.EditValue.ToLower.ToString)
                            Else
                                .Parameters.Add("@edetails", SqlDbType.VarChar).Value = DBNull.Value
                            End If
                            .Parameters.Add("@membertype", SqlDbType.Int).Value = MembershipTypeComboBoxEdit.SelectedIndex + 1
                            If Not (PictureEdit.Image Is Nothing) Then
                                Dim memstream As New MemoryStream()
                                PictureEdit.Image.Save(memstream, ImageFormat.Jpeg)
                                Dim pic() As Byte = memstream.ToArray()
                                .Parameters.Add("@photo", SqlDbType.Image).Value = pic
                            Else
                                .Parameters.Add("@photo", SqlDbType.Image).Value = DBNull.Value
                            End If
                            .Parameters.Add("@status", SqlDbType.Int).Value = StatusComboBoxEdit.EditValue
                            If Not DobDateEdit.EditValue = Nothing Then
                                .Parameters.Add("@dob", SqlDbType.Date).Value = CDate(DobDateEdit.EditValue).Date
                            Else
                                .Parameters.Add("@dob", SqlDbType.Date).Value = DBNull.Value
                            End If
                            If Not RegistrationDateEdit.EditValue = Nothing Then
                                .Parameters.Add("@regdate", SqlDbType.Date).Value = CDate(RegistrationDateEdit.EditValue).Date
                            Else
                                .Parameters.Add("@regdate", SqlDbType.Date).Value = DBNull.Value
                            End If
                            .Parameters.Add("@gender", SqlDbType.Int).Value = GenderComboBoxEdit.EditValue
                            .Parameters.Add("@refered", SqlDbType.Bit).Value = ReferedCheckEdit.Checked
                            If ReferedCheckEdit.Checked = False Then
                                .Parameters.Add("@rtype", SqlDbType.Int).Value = DBNull.Value
                                .Parameters.Add("@referee", SqlDbType.Int).Value = DBNull.Value
                            Else
                                .Parameters.Add("@rtype", SqlDbType.Int).Value = ReferalTypeComboBoxEdit.SelectedIndex + 1
                                .Parameters.Add("@referee", SqlDbType.Int).Value = RefererLookUpEdit.EditValue
                            End If
                            .Parameters.Add("@no", SqlDbType.Int).Value = MemberNo
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "UPDATE tblMemberAddress SET contact=@contact,email=@email,postaladdress=@add,alternativeContact=@alt,physicalAddress"
                    sql &= "=@physical WHERE  memberNo=@no"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@contact", SqlDbType.VarChar).Value = Trim(ContactTextEdit.EditValue)
                            If EmailTextEdit.EditValue IsNot Nothing Then
                                .Parameters.Add("@email", SqlDbType.VarChar).Value = Trim(EmailTextEdit.EditValue)
                            Else
                                .Parameters.Add("@email", SqlDbType.VarChar).Value = DBNull.Value
                            End If
                            If AddressTextEdit.EditValue IsNot Nothing Then
                                .Parameters.Add("@add", SqlDbType.VarChar).Value = Trim(AddressTextEdit.EditValue.ToLower.ToString)
                            Else
                                .Parameters.Add("@add", SqlDbType.VarChar).Value = DBNull.Value
                            End If
                            If AlternativeContactTextEdit.EditValue IsNot Nothing Then
                                .Parameters.Add("@alt", SqlDbType.VarChar).Value = Trim(AlternativeContactTextEdit.EditValue)
                            Else
                                .Parameters.Add("@alt", SqlDbType.VarChar).Value = DBNull.Value
                            End If
                            If PhyiscalAddressTextEdit.EditValue IsNot Nothing Then
                                .Parameters.Add("@physical", SqlDbType.VarChar).Value = Trim(PhyiscalAddressTextEdit.EditValue.ToLower.ToString)
                            Else
                                .Parameters.Add("@physical", SqlDbType.VarChar).Value = DBNull.Value
                            End If
                            .Parameters.Add("@no", SqlDbType.Int).Value = MemberNo
                            .ExecuteNonQuery()
                        End With
                    End Using

                    sql = "DELETE FROM tblReferals WHERE (referedMember=@no)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        cmd.Parameters.Add(New SqlParameter("@no", SqlDbType.Int)).Value = MemberNo
                        cmd.ExecuteNonQuery()
                    End Using


                    If ReferedCheckEdit.Checked = True AndAlso ReferalTypeComboBoxEdit.SelectedIndex = 1 Then
                        sql = "INSERT INTO tblReferals (ID,memberNo,paid,referedMember,amount) VALUES(@id,@no,@paid,@rm,@amnt)"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = ReferalId
                                .Parameters.Add(New SqlParameter("@no", SqlDbType.Int)).Value = RefererLookUpEdit.EditValue
                                .Parameters.Add(New SqlParameter("@paid", SqlDbType.Bit)).Value = False
                                .Parameters.Add(New SqlParameter("@rm", SqlDbType.Int)).Value = MemberNo
                                .Parameters.Add(New SqlParameter("@amnt", SqlDbType.Decimal)).Value = 100
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If
                    If ReferedCheckEdit.Checked = True AndAlso ReferalTypeComboBoxEdit.SelectedIndex = 0 Then
                        sql = "INSERT INTO tblReferals (ID,memberNo,paid,referedMember,amount) VALUES(@id,@no,@paid,@rm,@amnt)"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Add(New SqlParameter("@id", SqlDbType.Int)).Value = ReferalId
                                .Parameters.Add(New SqlParameter("@no", SqlDbType.Int)).Value = RefererLookUpEdit.EditValue
                                .Parameters.Add(New SqlParameter("@paid", SqlDbType.Bit)).Value = False
                                .Parameters.Add(New SqlParameter("@rm", SqlDbType.Int)).Value = MemberNo
                                .Parameters.Add(New SqlParameter("@amnt", SqlDbType.Decimal)).Value = 100
                                .ExecuteNonQuery()
                            End With
                        End Using
                    End If

                    If DataGridView.Rows.Count > 0 Then
                        sql = "DELETE FROM tblMemberNOK WHERE memberNo=@no"
                        Using cmd = New SqlCommand(sql, connection, MyTransaction)
                            With cmd
                                .Parameters.Clear()
                                .Parameters.Add("@no", SqlDbType.Int).Value = MemberNo
                                .ExecuteNonQuery()
                            End With
                        End Using

                        For i = 0 To DataGridView.Rows.Count - 1
                            sql = "INSERT INTO tblMemberNOK (memberNo,name,idNo,contact,share) VALUES(@no,@name,@id,@contact,@share)"
                            Using cmd = New SqlCommand(sql, connection, MyTransaction)
                                With cmd
                                    .Parameters.Clear()
                                    .Parameters.Add("@no", SqlDbType.Int).Value = MemberNo
                                    .Parameters.Add("@name", SqlDbType.VarChar).Value = DataGridView.Rows(i).Cells(0).Value
                                    .Parameters.Add("@id", SqlDbType.VarChar).Value = DataGridView.Rows(i).Cells(1).Value
                                    .Parameters.Add("@contact", SqlDbType.VarChar).Value = DataGridView.Rows(i).Cells(2).Value
                                    .Parameters.Add("@share", SqlDbType.Decimal).Value = CDec(DataGridView.Rows(i).Cells(3).Value)
                                    .ExecuteNonQuery()
                                End With
                            End Using
                        Next
                    End If

                    sql = "INSERT INTO tblLogs VALUES(@uid,@act,@actdate)"
                    Using cmd = New SqlCommand(sql, connection, MyTransaction)
                        With cmd
                            .Parameters.Clear()
                            .Parameters.Add("@uid", SqlDbType.Int).Value = LogedUserID
                            .Parameters.Add("@act", SqlDbType.NVarChar).Value = "Edited New Member " & IIf(NewRegistrationRadioButton.Checked, Trim(MemberNameTextEdit.EditValue), NonMemberLookUpEdit.Text)
                            .Parameters.Add("@actdate", SqlDbType.Date).Value = Date.Now.Date
                            .ExecuteNonQuery()
                        End With
                    End Using

                    MyTransaction.Commit()
                    AppClass.ShowNotification("Edited successfully")
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
        MemberNo = Nothing
        IsEdit = True
        AppClass.ClearItems(LayoutControl1)
        AddParams("@memberNo", fid)
        Dim Searchdt As DataTable = AppClass.LoadToDatatable("spFindMember", True)
        MemberNo = CInt(Searchdt.Rows(0)(0))
        MemberNameTextEdit.EditValue = CStr(Searchdt.Rows(0)(1))
        IDNoTextEdit.EditValue = CStr(Searchdt.Rows(0)(2))
        SourceOfIncomeComboBoxEdit.SelectedIndex = CInt(Searchdt.Rows(0)(3)) - 1
        If Not IsDBNull(Searchdt.Rows(0)(5)) Then
            EmployementDetailsTextEdit.EditValue = CStr(Searchdt.Rows(0)(5))
        Else
            EmployementDetailsTextEdit.EditValue = Nothing
        End If
        If Not IsDBNull(Searchdt.Rows(0)(4)) Then
            KraPinTextEdit.EditValue = CStr(Searchdt.Rows(0)(4))
        Else
            KraPinTextEdit.EditValue = Nothing
        End If
        MembershipTypeComboBoxEdit.SelectedIndex = CInt(Searchdt.Rows(0)(6)) - 1
        If Not IsDBNull(Searchdt.Rows(0)(7)) Then
            Dim bytBLOBData() As Byte = Searchdt.Rows(0)(7)
            Dim stmBLOBData As New MemoryStream(bytBLOBData)
            PictureEdit.Image = Image.FromStream(stmBLOBData)
        End If
        StatusComboBoxEdit.EditValue = CInt(Searchdt.Rows(0)(8))
        If Not IsDBNull(Searchdt.Rows(0)(9)) Then
            DobDateEdit.EditValue = CDate(Searchdt.Rows(0)(9))
        Else
            DobDateEdit.EditValue = Nothing
        End If
        If Not IsDBNull(Searchdt.Rows(0)(10)) Then
            RegistrationDateEdit.EditValue = CDate(Searchdt.Rows(0)(10))
        Else
            RegistrationDateEdit.EditValue = Nothing
        End If
        ContactTextEdit.EditValue = CStr(Searchdt.Rows(0)(11))
        EmailTextEdit.EditValue = CStr(Searchdt.Rows(0)(12))
        AddressTextEdit.EditValue = CStr(Searchdt.Rows(0)(13))
        AlternativeContactTextEdit.EditValue = CStr(Searchdt.Rows(0)(14))
        PhyiscalAddressTextEdit.EditValue = CStr(Searchdt.Rows(0)(15))
        ReferedCheckEdit.Checked = CBool(Searchdt.Rows(0)(16))
        If ReferedCheckEdit.Checked = True Then
            If Not IsDBNull(Searchdt.Rows(0)(17)) Then
                ReferalTypeComboBoxEdit.SelectedIndex = CInt(Searchdt.Rows(0)(17)) - 1
            End If
            If Not IsDBNull(Searchdt.Rows(0)(18)) Then
                RefererLookUpEdit.EditValue = CInt(Searchdt.Rows(0)(18))
            End If
        End If
        For Each txt In LayoutControl1.Controls.OfType(Of TextEdit)()
            With txt
                If .Text = "[None]" Then
                    .Text = Nothing
                End If
            End With
        Next
        DeleteSimpleButton.Enabled = True
        AddParams("@no", MemberNo)
        MemberNoTextEdit.EditValue = CStr(AppClass.FetchDBValue("SELECT COALESCE(memberID,'Unassigned') FROM tblMember WHERE (memberNo=@no)"))
        AddParams("@no", MemberNo)
        GenderComboBoxEdit.EditValue = CInt(AppClass.FetchDBValue("SELECT gender FROM tblMember WHERE (memberNo=@no)"))

        AddParams("@no", MemberNo)
        If CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblMemberNOK WHERE memberNo=@no")) > 0 Then
            AddParams("@no", MemberNo)
            sql = "SELECT UPPER(name),idNo,contact,share FROM tblMemberNOK WHERE memberNo=@no"
            dt = New DataTable
            dt = AppClass.LoadToDatatable(sql)
            For Each row In dt.Rows
                PopulateNOK(row(0), row(1), row(2), row(3))
            Next
        End If
    End Sub
    Sub PopulateNOK(name As String, id As String, contact As String, share As Decimal)
        Dim row As String() = New String() {name, id, contact, share}
        DataGridView.Rows.Add(row)
    End Sub
#End Region
#Region "EVENTS"
    Private Sub NonMemberRadioButton_CheckedChanged(sender As Object, e As EventArgs) Handles NewRegistrationRadioButton.CheckedChanged, NonMemberRadioButton.CheckedChanged
        If NewRegistrationRadioButton.Checked Then
            MemberLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always
            NonMemberLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        Else
            MemberLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
            NonMemberLayoutControlItem.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always
        End If
    End Sub
    Private Sub SaveSimpleButton_Click(sender As Object, e As EventArgs) Handles SaveSimpleButton.Click
        If Not Datavalidation() Then
            Exit Sub
        End If

        AddParams("@id", Trim(IDNoTextEdit.EditValue))
        AddParams("@no", MemberNo)
        If CInt(AppClass.FetchDBValue("SELECT COUNT(*) FROM tblMember WHERE idNo=@id AND memberNo <> @no")) > 0 Then
            AppClass.ShowNotification("ID Already Exists in the System!")
            With IDNoTextEdit
                .Focus()
                .SelectAll()
            End With
            Exit Sub
        End If

        If Not IsEdit Then
            Save()
        Else
            Edit()
        End If

    End Sub
    Private Sub AddSimpleButton_Click(sender As Object, e As EventArgs) Handles AddSimpleButton.Click
        If NomineeNameTextEdit.EditValue Is Nothing Then
            AppClass.ShowNotification("Enter NOK Name!")
            NomineeNameTextEdit.Focus()
            Exit Sub
        ElseIf SharesTextEdit.EditValue Is Nothing Then
            AppClass.ShowNotification("Enter Shares!")
            SharesTextEdit.Focus()
            Exit Sub
        ElseIf CDec(AppClass.GetGridTotal(DataGridView, 3) + CDec(SharesTextEdit.EditValue)) > 100 Then
            AppClass.ShowNotification("Total Shares Will Exceed 100%!")
            With SharesTextEdit
                .Focus()
                .SelectAll()
            End With
            Exit Sub
        End If

        Select Case AddSimpleButton.Text
            Case "Add Nominee"
                For i = 0 To DataGridView.Rows.Count - 1
                    If NokIdTextEdit.EditValue IsNot Nothing Then
                        If CStr(Trim(DataGridView.Rows(i).Cells(1).Value)) = Trim(NokIdTextEdit.EditValue) Then
                            AppClass.ShowNotification("Nok Id No Already Entered!")
                            With NokIdTextEdit
                                .Focus()
                                .SelectAll()
                            End With
                            Exit Sub
                        End If
                    End If
                Next

                Dim nokId, nokContact As String
                If NokIdTextEdit.EditValue IsNot Nothing Then
                    nokId = Trim(NokIdTextEdit.EditValue)
                Else
                    nokId = "[None]"
                End If
                If NokContactTextEdit.EditValue IsNot Nothing Then
                    nokContact = Trim(NokContactTextEdit.EditValue)
                Else
                    nokContact = "[None]"
                End If
                Dim row As String() = New String() {CStr(DataGridView.Rows.Count + 1), Trim(NomineeNameTextEdit.EditValue), nokId, nokContact, SharesTextEdit.EditValue}
                DataGridView.Rows.Add(row)
                NomineeNameTextEdit.EditValue = Nothing
                NokContactTextEdit.EditValue = Nothing
                NokIdTextEdit.EditValue = Nothing
                SharesTextEdit.EditValue = Nothing
        End Select
    End Sub
    Private Sub ResetSimpleButton_Click(sender As Object, e As EventArgs) Handles ResetSimpleButton.Click
        Reset()
    End Sub
    Private Sub DeleteSimpleButton_Click(sender As Object, e As EventArgs) Handles DeleteSimpleButton.Click
        If AppClass.AlertQuestion("Are You Sure You Want To Delete This Member") = System.Windows.Forms.DialogResult.Yes Then
            AddParams("@no", MemberNo)
            If CInt(AppClass.FetchDBValue("SELECT COUNT(ID) FROM tblIncomeDetails WHERE (memberNo=@no)")) > 0 Then
                AppClass.ShowNotification("Cannot Delete As Its Referenced Elsewhere")
                Exit Sub
            End If

            AddParams("@no", MemberNo)
            AppClass.ExecQuery("DELETE FROM tblMemberNOK WHERE (memberNo=@no)")
            AddParams("@no", MemberNo)
            AppClass.ExecQuery("DELETE FROM tblMemberAddress WHERE (memberNo=@no)")
            AddParams("@no", MemberNo)
            AppClass.ExecQuery("DELETE FROM tblMember WHERE (memberNo=@no)")
            If RecordCount > 0 Then
                AddParams("@action", "insert")
                AddParams("@userid", LogedUserID)
                AddParams("@activity", "Deleted Member " & Trim(MemberNameTextEdit.EditValue))
                AddParams("@actDate", Date.Now.Date)
                AppClass.ExecSP("spLogs")
                AppClass.ShowNotification("Deleted Successfully!")
                Reset()
            End If
        End If
    End Sub
    Private Sub ReferedCheckEdit_CheckedChanged(sender As Object, e As EventArgs) Handles ReferedCheckEdit.CheckedChanged
        If ReferedCheckEdit.Checked = True Then
            ReferTypeLayoutControlItem.Enabled = True
            AgentLayoutControlItem.Enabled = True
            AgentLayoutControlItem.Text = "Select Agent:"
        Else
            ReferTypeLayoutControlItem.Enabled = False
            AgentLayoutControlItem.Enabled = False
            AgentLayoutControlItem.Text = "Select Agent:"
            ReferalTypeComboBoxEdit.EditValue = Nothing
            RefererLookUpEdit.EditValue = Nothing
        End If
    End Sub
    Private Sub ReferalTypeComboBoxEdit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ReferalTypeComboBoxEdit.SelectedIndexChanged
        If ReferalTypeComboBoxEdit.EditValue IsNot Nothing Then
            RefererLookUpEdit.Properties.DataSource = Nothing
            Select Case ReferalTypeComboBoxEdit.SelectedIndex
                Case 0
                    LayoutControlItem21.Text = "Select Agent:"
                    AddParams("@tru", True)
                    AppClass.LoadToLookUpEdit("SELECT ID,UPPER(agentName) AS referName FROM tblAgents WHERE (active=@tru) ORDER BY agentName", RefererLookUpEdit _
                                    , "referName", "ID")
                Case 1
                    LayoutControlItem21.Text = "Select Member:"
                    AppClass.LoadToLookUpEdit("SELECT memberNo,UPPER(memberName) AS referName FROM tblMember ORDER BY memberName", RefererLookUpEdit, "referName", "memberNo")
            End Select
        End If
    End Sub
    Private Sub FindSimpleButton_Click(sender As Object, e As EventArgs) Handles FindSimpleButton.Click
        Dim SearchBy As String = "Search by name,contact,ID No or Member No..."
        Using frm As New SearchForm(SearchBy, "spSearchMember")
            If frm.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                Find(frm.DataGridView.CurrentRow.Cells(0).Value)
            End If
        End Using
    End Sub
    Private Sub SourceOfIncomeComboBoxEdit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles SourceOfIncomeComboBoxEdit.SelectedIndexChanged
        If SourceOfIncomeComboBoxEdit.SelectedIndex = 0 Then
            EmployementDetailsTextEdit.Properties.ReadOnly = False
        Else
            EmployementDetailsTextEdit.Properties.ReadOnly = True
        End If
    End Sub
#End Region
End Class
