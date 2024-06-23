<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits DevExpress.XtraBars.FluentDesignSystem.FluentDesignForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Me.AppContainer = New DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormContainer()
        Me.AccordionControl1 = New DevExpress.XtraBars.Navigation.AccordionControl()
        Me.aceHome = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.AceUsers = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.AceUserRights = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.AceCloneRights = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.AceDatabase = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.AccordionControlSeparator1 = New DevExpress.XtraBars.Navigation.AccordionControlSeparator()
        Me.AceChangePassword = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.AccordionControlSeparator2 = New DevExpress.XtraBars.Navigation.AccordionControlSeparator()
        Me.aceMessaging = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.aceGeneralMessage = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.aceProjects = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.AceMembers = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.AceAssignMemberNo = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.AccordionControlSeparator3 = New DevExpress.XtraBars.Navigation.AccordionControlSeparator()
        Me.AceProjectsForm = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.aceFinance = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.AceMemberContributions = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.AceDividentPayments = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.aceReports = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.FluentDesignFormControl1 = New DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl()
        Me.TopBarItem = New DevExpress.XtraBars.BarStaticItem()
        Me.FluentFormDefaultManager1 = New DevExpress.XtraBars.FluentDesignSystem.FluentFormDefaultManager(Me.components)
        Me.AccordionControlElement1 = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        Me.AceImportExpenses = New DevExpress.XtraBars.Navigation.AccordionControlElement()
        CType(Me.AccordionControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.FluentDesignFormControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.FluentFormDefaultManager1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'AppContainer
        '
        Me.AppContainer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.AppContainer.Location = New System.Drawing.Point(292, 46)
        Me.AppContainer.Margin = New System.Windows.Forms.Padding(2)
        Me.AppContainer.Name = "AppContainer"
        Me.AppContainer.Size = New System.Drawing.Size(1092, 656)
        Me.AppContainer.TabIndex = 0
        '
        'AccordionControl1
        '
        Me.AccordionControl1.Appearance.Item.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AccordionControl1.Appearance.Item.Disabled.Options.UseFont = True
        Me.AccordionControl1.Appearance.Item.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AccordionControl1.Appearance.Item.Hovered.Options.UseFont = True
        Me.AccordionControl1.Appearance.Item.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AccordionControl1.Appearance.Item.Normal.Options.UseFont = True
        Me.AccordionControl1.Appearance.Item.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AccordionControl1.Appearance.Item.Pressed.Options.UseFont = True
        Me.AccordionControl1.Dock = System.Windows.Forms.DockStyle.Left
        Me.AccordionControl1.Elements.AddRange(New DevExpress.XtraBars.Navigation.AccordionControlElement() {Me.aceHome, Me.aceProjects, Me.aceFinance, Me.aceReports})
        Me.AccordionControl1.Location = New System.Drawing.Point(0, 46)
        Me.AccordionControl1.Margin = New System.Windows.Forms.Padding(2)
        Me.AccordionControl1.Name = "AccordionControl1"
        Me.AccordionControl1.ScrollBarMode = DevExpress.XtraBars.Navigation.ScrollBarMode.Touch
        Me.AccordionControl1.Size = New System.Drawing.Size(292, 656)
        Me.AccordionControl1.TabIndex = 1
        Me.AccordionControl1.ViewType = DevExpress.XtraBars.Navigation.AccordionControlViewType.HamburgerMenu
        '
        'aceHome
        '
        Me.aceHome.Elements.AddRange(New DevExpress.XtraBars.Navigation.AccordionControlElement() {Me.AceUsers, Me.AceUserRights, Me.AceCloneRights, Me.AceDatabase, Me.AccordionControlSeparator1, Me.AceChangePassword, Me.AccordionControlSeparator2, Me.aceMessaging, Me.aceGeneralMessage})
        Me.aceHome.Name = "aceHome"
        Me.aceHome.Text = "Home"
        '
        'AceUsers
        '
        Me.AceUsers.Appearance.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceUsers.Appearance.Disabled.Options.UseFont = True
        Me.AceUsers.Appearance.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceUsers.Appearance.Hovered.Options.UseFont = True
        Me.AceUsers.Appearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceUsers.Appearance.Normal.Options.UseFont = True
        Me.AceUsers.Appearance.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceUsers.Appearance.Pressed.Options.UseFont = True
        Me.AceUsers.ImageOptions.Image = Global.phcsl.My.Resources.Resources.icons8_user_16
        Me.AceUsers.Name = "AceUsers"
        Me.AceUsers.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.AceUsers.Text = "Users"
        '
        'AceUserRights
        '
        Me.AceUserRights.Appearance.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceUserRights.Appearance.Disabled.Options.UseFont = True
        Me.AceUserRights.Appearance.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceUserRights.Appearance.Hovered.Options.UseFont = True
        Me.AceUserRights.Appearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceUserRights.Appearance.Normal.Options.UseFont = True
        Me.AceUserRights.Appearance.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceUserRights.Appearance.Pressed.Options.UseFont = True
        Me.AceUserRights.ImageOptions.Image = Global.phcsl.My.Resources.Resources.icons8_protect_16
        Me.AceUserRights.Name = "AceUserRights"
        Me.AceUserRights.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.AceUserRights.Text = "User Rights"
        '
        'AceCloneRights
        '
        Me.AceCloneRights.Appearance.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceCloneRights.Appearance.Disabled.Options.UseFont = True
        Me.AceCloneRights.Appearance.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceCloneRights.Appearance.Hovered.Options.UseFont = True
        Me.AceCloneRights.Appearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceCloneRights.Appearance.Normal.Options.UseFont = True
        Me.AceCloneRights.Appearance.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceCloneRights.Appearance.Pressed.Options.UseFont = True
        Me.AceCloneRights.ImageOptions.Image = Global.phcsl.My.Resources.Resources.icons8_copy_16
        Me.AceCloneRights.Name = "AceCloneRights"
        Me.AceCloneRights.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.AceCloneRights.Text = "Clone Rights"
        '
        'AceDatabase
        '
        Me.AceDatabase.Appearance.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceDatabase.Appearance.Disabled.Options.UseFont = True
        Me.AceDatabase.Appearance.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceDatabase.Appearance.Hovered.Options.UseFont = True
        Me.AceDatabase.Appearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceDatabase.Appearance.Normal.Options.UseFont = True
        Me.AceDatabase.Appearance.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceDatabase.Appearance.Pressed.Options.UseFont = True
        Me.AceDatabase.ImageOptions.Image = Global.phcsl.My.Resources.Resources.icons8_database_administrator_16
        Me.AceDatabase.Name = "AceDatabase"
        Me.AceDatabase.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.AceDatabase.Text = "Database"
        '
        'AccordionControlSeparator1
        '
        Me.AccordionControlSeparator1.Name = "AccordionControlSeparator1"
        '
        'AceChangePassword
        '
        Me.AceChangePassword.Appearance.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceChangePassword.Appearance.Disabled.Options.UseFont = True
        Me.AceChangePassword.Appearance.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceChangePassword.Appearance.Hovered.Options.UseFont = True
        Me.AceChangePassword.Appearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceChangePassword.Appearance.Normal.Options.UseFont = True
        Me.AceChangePassword.Appearance.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceChangePassword.Appearance.Pressed.Options.UseFont = True
        Me.AceChangePassword.ImageOptions.Image = Global.phcsl.My.Resources.Resources.icons8_password_16
        Me.AceChangePassword.Name = "AceChangePassword"
        Me.AceChangePassword.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.AceChangePassword.Text = "Change Password"
        '
        'AccordionControlSeparator2
        '
        Me.AccordionControlSeparator2.Name = "AccordionControlSeparator2"
        '
        'aceMessaging
        '
        Me.aceMessaging.Appearance.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.aceMessaging.Appearance.Disabled.Options.UseFont = True
        Me.aceMessaging.Appearance.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.aceMessaging.Appearance.Hovered.Options.UseFont = True
        Me.aceMessaging.Appearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.aceMessaging.Appearance.Normal.Options.UseFont = True
        Me.aceMessaging.Appearance.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.aceMessaging.Appearance.Pressed.Options.UseFont = True
        Me.aceMessaging.ImageOptions.Image = Global.phcsl.My.Resources.Resources.icons8_bell_16
        Me.aceMessaging.Name = "aceMessaging"
        Me.aceMessaging.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.aceMessaging.Text = "Payment Reminders"
        '
        'aceGeneralMessage
        '
        Me.aceGeneralMessage.Appearance.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.aceGeneralMessage.Appearance.Disabled.Options.UseFont = True
        Me.aceGeneralMessage.Appearance.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.aceGeneralMessage.Appearance.Hovered.Options.UseFont = True
        Me.aceGeneralMessage.Appearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.aceGeneralMessage.Appearance.Normal.Options.UseFont = True
        Me.aceGeneralMessage.Appearance.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.aceGeneralMessage.Appearance.Pressed.Options.UseFont = True
        Me.aceGeneralMessage.ImageOptions.Image = Global.phcsl.My.Resources.Resources.icons8_message_16
        Me.aceGeneralMessage.Name = "aceGeneralMessage"
        Me.aceGeneralMessage.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.aceGeneralMessage.Text = "General Message"
        '
        'aceProjects
        '
        Me.aceProjects.Elements.AddRange(New DevExpress.XtraBars.Navigation.AccordionControlElement() {Me.AceMembers, Me.AceAssignMemberNo, Me.AccordionControlSeparator3, Me.AceProjectsForm})
        Me.aceProjects.Name = "aceProjects"
        Me.aceProjects.Text = "Projects"
        '
        'AceMembers
        '
        Me.AceMembers.Appearance.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceMembers.Appearance.Disabled.Options.UseFont = True
        Me.AceMembers.Appearance.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceMembers.Appearance.Hovered.Options.UseFont = True
        Me.AceMembers.Appearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceMembers.Appearance.Normal.Options.UseFont = True
        Me.AceMembers.Appearance.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceMembers.Appearance.Pressed.Options.UseFont = True
        Me.AceMembers.ImageOptions.Image = CType(resources.GetObject("AceMembers.ImageOptions.Image"), System.Drawing.Image)
        Me.AceMembers.Name = "AceMembers"
        Me.AceMembers.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.AceMembers.Text = "Members"
        '
        'AceAssignMemberNo
        '
        Me.AceAssignMemberNo.Appearance.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceAssignMemberNo.Appearance.Disabled.Options.UseFont = True
        Me.AceAssignMemberNo.Appearance.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceAssignMemberNo.Appearance.Hovered.Options.UseFont = True
        Me.AceAssignMemberNo.Appearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceAssignMemberNo.Appearance.Normal.Options.UseFont = True
        Me.AceAssignMemberNo.Appearance.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceAssignMemberNo.Appearance.Pressed.Options.UseFont = True
        Me.AceAssignMemberNo.ImageOptions.Image = Global.phcsl.My.Resources.Resources.icons8_number_16
        Me.AceAssignMemberNo.Name = "AceAssignMemberNo"
        Me.AceAssignMemberNo.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.AceAssignMemberNo.Text = "Assign Member No"
        '
        'AccordionControlSeparator3
        '
        Me.AccordionControlSeparator3.Name = "AccordionControlSeparator3"
        '
        'AceProjectsForm
        '
        Me.AceProjectsForm.Appearance.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceProjectsForm.Appearance.Disabled.Options.UseFont = True
        Me.AceProjectsForm.Appearance.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceProjectsForm.Appearance.Hovered.Options.UseFont = True
        Me.AceProjectsForm.Appearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceProjectsForm.Appearance.Normal.Options.UseFont = True
        Me.AceProjectsForm.Appearance.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceProjectsForm.Appearance.Pressed.Options.UseFont = True
        Me.AceProjectsForm.ImageOptions.Image = Global.phcsl.My.Resources.Resources.icons8_land_16
        Me.AceProjectsForm.Name = "AceProjectsForm"
        Me.AceProjectsForm.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.AceProjectsForm.Text = "Projects"
        '
        'aceFinance
        '
        Me.aceFinance.Elements.AddRange(New DevExpress.XtraBars.Navigation.AccordionControlElement() {Me.AceMemberContributions, Me.AceDividentPayments, Me.AceImportExpenses})
        Me.aceFinance.Expanded = True
        Me.aceFinance.Name = "aceFinance"
        Me.aceFinance.Text = "Finance"
        '
        'AceMemberContributions
        '
        Me.AceMemberContributions.Appearance.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.AceMemberContributions.Appearance.Disabled.Options.UseFont = True
        Me.AceMemberContributions.Appearance.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceMemberContributions.Appearance.Hovered.Options.UseFont = True
        Me.AceMemberContributions.Appearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceMemberContributions.Appearance.Normal.Options.UseFont = True
        Me.AceMemberContributions.Appearance.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceMemberContributions.Appearance.Pressed.Options.UseFont = True
        Me.AceMemberContributions.ImageOptions.Image = Global.phcsl.My.Resources.Resources.icons8_pay_16
        Me.AceMemberContributions.Name = "AceMemberContributions"
        Me.AceMemberContributions.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.AceMemberContributions.Text = "Member Contributions"
        '
        'AceDividentPayments
        '
        Me.AceDividentPayments.Appearance.Disabled.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceDividentPayments.Appearance.Disabled.Options.UseFont = True
        Me.AceDividentPayments.Appearance.Hovered.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceDividentPayments.Appearance.Hovered.Options.UseFont = True
        Me.AceDividentPayments.Appearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceDividentPayments.Appearance.Normal.Options.UseFont = True
        Me.AceDividentPayments.Appearance.Pressed.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold)
        Me.AceDividentPayments.Appearance.Pressed.Options.UseFont = True
        Me.AceDividentPayments.ImageOptions.Image = Global.phcsl.My.Resources.Resources.icons8_money_16
        Me.AceDividentPayments.Name = "AceDividentPayments"
        Me.AceDividentPayments.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.AceDividentPayments.Text = "Dividends Payments"
        '
        'aceReports
        '
        Me.aceReports.Name = "aceReports"
        Me.aceReports.Text = "Reports"
        '
        'FluentDesignFormControl1
        '
        Me.FluentDesignFormControl1.FluentDesignForm = Me
        Me.FluentDesignFormControl1.Items.AddRange(New DevExpress.XtraBars.BarItem() {Me.TopBarItem})
        Me.FluentDesignFormControl1.Location = New System.Drawing.Point(0, 0)
        Me.FluentDesignFormControl1.Manager = Me.FluentFormDefaultManager1
        Me.FluentDesignFormControl1.Margin = New System.Windows.Forms.Padding(2)
        Me.FluentDesignFormControl1.Name = "FluentDesignFormControl1"
        Me.FluentDesignFormControl1.Size = New System.Drawing.Size(1384, 46)
        Me.FluentDesignFormControl1.TabIndex = 2
        Me.FluentDesignFormControl1.TabStop = False
        Me.FluentDesignFormControl1.TitleItemLinks.Add(Me.TopBarItem)
        '
        'TopBarItem
        '
        Me.TopBarItem.Id = 0
        Me.TopBarItem.ItemAppearance.Normal.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TopBarItem.ItemAppearance.Normal.Options.UseFont = True
        Me.TopBarItem.Name = "TopBarItem"
        '
        'FluentFormDefaultManager1
        '
        Me.FluentFormDefaultManager1.DockingEnabled = False
        Me.FluentFormDefaultManager1.Form = Me
        Me.FluentFormDefaultManager1.Items.AddRange(New DevExpress.XtraBars.BarItem() {Me.TopBarItem})
        Me.FluentFormDefaultManager1.MaxItemId = 1
        '
        'AccordionControlElement1
        '
        Me.AccordionControlElement1.Name = "AccordionControlElement1"
        Me.AccordionControlElement1.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.AccordionControlElement1.Text = "Element1"
        '
        'AceImportExpenses
        '
        Me.AceImportExpenses.ImageOptions.Image = Global.phcsl.My.Resources.Resources.icons8_import_16
        Me.AceImportExpenses.Name = "AceImportExpenses"
        Me.AceImportExpenses.Style = DevExpress.XtraBars.Navigation.ElementStyle.Item
        Me.AceImportExpenses.Text = "Import Expenses"
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1384, 702)
        Me.ControlContainer = Me.AppContainer
        Me.Controls.Add(Me.AppContainer)
        Me.Controls.Add(Me.AccordionControl1)
        Me.Controls.Add(Me.FluentDesignFormControl1)
        Me.FluentDesignFormControl = Me.FluentDesignFormControl1
        Me.IconOptions.Image = Global.phcsl.My.Resources.Resources.logo
        Me.Margin = New System.Windows.Forms.Padding(2)
        Me.Name = "MainForm"
        Me.NavigationControl = Me.AccordionControl1
        Me.Text = "Main Form"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        CType(Me.AccordionControl1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.FluentDesignFormControl1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.FluentFormDefaultManager1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents AppContainer As DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormContainer
    Friend WithEvents AccordionControl1 As DevExpress.XtraBars.Navigation.AccordionControl
    Friend WithEvents aceHome As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents FluentDesignFormControl1 As DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl
    Friend WithEvents FluentFormDefaultManager1 As DevExpress.XtraBars.FluentDesignSystem.FluentFormDefaultManager
    Friend WithEvents aceProjects As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents aceFinance As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents aceReports As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents aceMessaging As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents TopBarItem As DevExpress.XtraBars.BarStaticItem
    Friend WithEvents AceMemberContributions As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents AceUsers As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents AceUserRights As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents AceCloneRights As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents AccordionControlSeparator1 As DevExpress.XtraBars.Navigation.AccordionControlSeparator
    Friend WithEvents AccordionControlElement1 As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents AceDatabase As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents AceChangePassword As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents AccordionControlSeparator2 As DevExpress.XtraBars.Navigation.AccordionControlSeparator
    Friend WithEvents AceMembers As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents AceAssignMemberNo As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents AccordionControlSeparator3 As DevExpress.XtraBars.Navigation.AccordionControlSeparator
    Friend WithEvents AceProjectsForm As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents aceGeneralMessage As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents AceDividentPayments As DevExpress.XtraBars.Navigation.AccordionControlElement
    Friend WithEvents AceImportExpenses As DevExpress.XtraBars.Navigation.AccordionControlElement
End Class
