<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UCChangePassword
    Inherits DevExpress.XtraEditors.XtraUserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.PanelControl1 = New DevExpress.XtraEditors.PanelControl()
        Me.SaveSimpleButton = New DevExpress.XtraEditors.SimpleButton()
        Me.LayoutControl1 = New DevExpress.XtraLayout.LayoutControl()
        Me.Root = New DevExpress.XtraLayout.LayoutControlGroup()
        Me.UserNameTextEdit = New DevExpress.XtraEditors.TextEdit()
        Me.LayoutControlItem1 = New DevExpress.XtraLayout.LayoutControlItem()
        Me.EmptySpaceItem1 = New DevExpress.XtraLayout.EmptySpaceItem()
        Me.OldPasswordTextEdit = New DevExpress.XtraEditors.TextEdit()
        Me.LayoutControlItem2 = New DevExpress.XtraLayout.LayoutControlItem()
        Me.NewPasswordTextEdit = New DevExpress.XtraEditors.TextEdit()
        Me.LayoutControlItem3 = New DevExpress.XtraLayout.LayoutControlItem()
        Me.ConfirmTextEdit = New DevExpress.XtraEditors.TextEdit()
        Me.LayoutControlItem4 = New DevExpress.XtraLayout.LayoutControlItem()
        CType(Me.PanelControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PanelControl1.SuspendLayout()
        CType(Me.LayoutControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.LayoutControl1.SuspendLayout()
        CType(Me.Root, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.UserNameTextEdit.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.EmptySpaceItem1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.OldPasswordTextEdit.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NewPasswordTextEdit.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.ConfirmTextEdit.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem4, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PanelControl1
        '
        Me.PanelControl1.Controls.Add(Me.SaveSimpleButton)
        Me.PanelControl1.Dock = System.Windows.Forms.DockStyle.Top
        Me.PanelControl1.Location = New System.Drawing.Point(0, 0)
        Me.PanelControl1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.PanelControl1.Name = "PanelControl1"
        Me.PanelControl1.Size = New System.Drawing.Size(1020, 33)
        Me.PanelControl1.TabIndex = 6
        '
        'SaveSimpleButton
        '
        Me.SaveSimpleButton.Appearance.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.SaveSimpleButton.Appearance.Options.UseFont = True
        Me.SaveSimpleButton.Location = New System.Drawing.Point(4, 4)
        Me.SaveSimpleButton.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.SaveSimpleButton.Name = "SaveSimpleButton"
        Me.SaveSimpleButton.Size = New System.Drawing.Size(101, 24)
        Me.SaveSimpleButton.TabIndex = 0
        Me.SaveSimpleButton.Text = "Change Password"
        '
        'LayoutControl1
        '
        Me.LayoutControl1.Controls.Add(Me.ConfirmTextEdit)
        Me.LayoutControl1.Controls.Add(Me.NewPasswordTextEdit)
        Me.LayoutControl1.Controls.Add(Me.OldPasswordTextEdit)
        Me.LayoutControl1.Controls.Add(Me.UserNameTextEdit)
        Me.LayoutControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LayoutControl1.Location = New System.Drawing.Point(0, 33)
        Me.LayoutControl1.Name = "LayoutControl1"
        Me.LayoutControl1.Root = Me.Root
        Me.LayoutControl1.Size = New System.Drawing.Size(1020, 578)
        Me.LayoutControl1.TabIndex = 7
        Me.LayoutControl1.Text = "LayoutControl1"
        '
        'Root
        '
        Me.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.[True]
        Me.Root.GroupBordersVisible = False
        Me.Root.Items.AddRange(New DevExpress.XtraLayout.BaseLayoutItem() {Me.EmptySpaceItem1, Me.LayoutControlItem3, Me.LayoutControlItem4, Me.LayoutControlItem2, Me.LayoutControlItem1})
        Me.Root.Name = "Root"
        Me.Root.Size = New System.Drawing.Size(1020, 578)
        Me.Root.TextVisible = False
        '
        'UserNameTextEdit
        '
        Me.UserNameTextEdit.Location = New System.Drawing.Point(106, 12)
        Me.UserNameTextEdit.Name = "UserNameTextEdit"
        Me.UserNameTextEdit.Properties.ReadOnly = True
        Me.UserNameTextEdit.Size = New System.Drawing.Size(902, 20)
        Me.UserNameTextEdit.StyleController = Me.LayoutControl1
        Me.UserNameTextEdit.TabIndex = 4
        '
        'LayoutControlItem1
        '
        Me.LayoutControlItem1.Control = Me.UserNameTextEdit
        Me.LayoutControlItem1.Location = New System.Drawing.Point(0, 0)
        Me.LayoutControlItem1.Name = "LayoutControlItem1"
        Me.LayoutControlItem1.Size = New System.Drawing.Size(1000, 24)
        Me.LayoutControlItem1.Text = "User:"
        Me.LayoutControlItem1.TextSize = New System.Drawing.Size(90, 13)
        '
        'EmptySpaceItem1
        '
        Me.EmptySpaceItem1.AllowHotTrack = False
        Me.EmptySpaceItem1.Location = New System.Drawing.Point(0, 72)
        Me.EmptySpaceItem1.Name = "EmptySpaceItem1"
        Me.EmptySpaceItem1.Size = New System.Drawing.Size(1000, 486)
        Me.EmptySpaceItem1.TextSize = New System.Drawing.Size(0, 0)
        '
        'OldPasswordTextEdit
        '
        Me.OldPasswordTextEdit.Location = New System.Drawing.Point(106, 36)
        Me.OldPasswordTextEdit.Name = "OldPasswordTextEdit"
        Me.OldPasswordTextEdit.Properties.UseSystemPasswordChar = True
        Me.OldPasswordTextEdit.Size = New System.Drawing.Size(902, 20)
        Me.OldPasswordTextEdit.StyleController = Me.LayoutControl1
        Me.OldPasswordTextEdit.TabIndex = 5
        '
        'LayoutControlItem2
        '
        Me.LayoutControlItem2.Control = Me.OldPasswordTextEdit
        Me.LayoutControlItem2.Location = New System.Drawing.Point(0, 24)
        Me.LayoutControlItem2.Name = "LayoutControlItem2"
        Me.LayoutControlItem2.Size = New System.Drawing.Size(1000, 24)
        Me.LayoutControlItem2.Text = "Old Password:"
        Me.LayoutControlItem2.TextSize = New System.Drawing.Size(90, 13)
        '
        'NewPasswordTextEdit
        '
        Me.NewPasswordTextEdit.Location = New System.Drawing.Point(106, 60)
        Me.NewPasswordTextEdit.Name = "NewPasswordTextEdit"
        Me.NewPasswordTextEdit.Properties.UseSystemPasswordChar = True
        Me.NewPasswordTextEdit.Size = New System.Drawing.Size(402, 20)
        Me.NewPasswordTextEdit.StyleController = Me.LayoutControl1
        Me.NewPasswordTextEdit.TabIndex = 6
        '
        'LayoutControlItem3
        '
        Me.LayoutControlItem3.Control = Me.NewPasswordTextEdit
        Me.LayoutControlItem3.Location = New System.Drawing.Point(0, 48)
        Me.LayoutControlItem3.Name = "LayoutControlItem3"
        Me.LayoutControlItem3.Size = New System.Drawing.Size(500, 24)
        Me.LayoutControlItem3.Text = "New Password:"
        Me.LayoutControlItem3.TextSize = New System.Drawing.Size(90, 13)
        '
        'ConfirmTextEdit
        '
        Me.ConfirmTextEdit.Location = New System.Drawing.Point(606, 60)
        Me.ConfirmTextEdit.Name = "ConfirmTextEdit"
        Me.ConfirmTextEdit.Properties.UseSystemPasswordChar = True
        Me.ConfirmTextEdit.Size = New System.Drawing.Size(402, 20)
        Me.ConfirmTextEdit.StyleController = Me.LayoutControl1
        Me.ConfirmTextEdit.TabIndex = 7
        '
        'LayoutControlItem4
        '
        Me.LayoutControlItem4.Control = Me.ConfirmTextEdit
        Me.LayoutControlItem4.Location = New System.Drawing.Point(500, 48)
        Me.LayoutControlItem4.Name = "LayoutControlItem4"
        Me.LayoutControlItem4.Size = New System.Drawing.Size(500, 24)
        Me.LayoutControlItem4.Text = "Confirm Password:"
        Me.LayoutControlItem4.TextSize = New System.Drawing.Size(90, 13)
        '
        'UCChangePassword
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.LayoutControl1)
        Me.Controls.Add(Me.PanelControl1)
        Me.Name = "UCChangePassword"
        Me.Size = New System.Drawing.Size(1020, 611)
        CType(Me.PanelControl1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PanelControl1.ResumeLayout(False)
        CType(Me.LayoutControl1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.LayoutControl1.ResumeLayout(False)
        CType(Me.Root, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.UserNameTextEdit.Properties, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.EmptySpaceItem1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.OldPasswordTextEdit.Properties, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NewPasswordTextEdit.Properties, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.ConfirmTextEdit.Properties, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem4, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents PanelControl1 As DevExpress.XtraEditors.PanelControl
    Friend WithEvents SaveSimpleButton As DevExpress.XtraEditors.SimpleButton
    Friend WithEvents LayoutControl1 As DevExpress.XtraLayout.LayoutControl
    Friend WithEvents ConfirmTextEdit As DevExpress.XtraEditors.TextEdit
    Friend WithEvents NewPasswordTextEdit As DevExpress.XtraEditors.TextEdit
    Friend WithEvents OldPasswordTextEdit As DevExpress.XtraEditors.TextEdit
    Friend WithEvents UserNameTextEdit As DevExpress.XtraEditors.TextEdit
    Friend WithEvents Root As DevExpress.XtraLayout.LayoutControlGroup
    Friend WithEvents EmptySpaceItem1 As DevExpress.XtraLayout.EmptySpaceItem
    Friend WithEvents LayoutControlItem3 As DevExpress.XtraLayout.LayoutControlItem
    Friend WithEvents LayoutControlItem4 As DevExpress.XtraLayout.LayoutControlItem
    Friend WithEvents LayoutControlItem2 As DevExpress.XtraLayout.LayoutControlItem
    Friend WithEvents LayoutControlItem1 As DevExpress.XtraLayout.LayoutControlItem
End Class
