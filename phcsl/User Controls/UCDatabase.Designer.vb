<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UCDatabase
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
        Me.LayoutControl1 = New DevExpress.XtraLayout.LayoutControl()
        Me.RestoreSimpleButton = New DevExpress.XtraEditors.SimpleButton()
        Me.BrowseRestoreSimpleButton = New DevExpress.XtraEditors.SimpleButton()
        Me.RestoreTextEdit = New DevExpress.XtraEditors.TextEdit()
        Me.BackupSimpleButton = New DevExpress.XtraEditors.SimpleButton()
        Me.BackupBrowseSimpleButton = New DevExpress.XtraEditors.SimpleButton()
        Me.BackupLocationTextEdit = New DevExpress.XtraEditors.TextEdit()
        Me.Root = New DevExpress.XtraLayout.LayoutControlGroup()
        Me.EmptySpaceItem1 = New DevExpress.XtraLayout.EmptySpaceItem()
        Me.LayoutControlGroup1 = New DevExpress.XtraLayout.LayoutControlGroup()
        Me.LayoutControlItem4 = New DevExpress.XtraLayout.LayoutControlItem()
        Me.LayoutControlItem1 = New DevExpress.XtraLayout.LayoutControlItem()
        Me.LayoutControlItem2 = New DevExpress.XtraLayout.LayoutControlItem()
        Me.LayoutControlItem3 = New DevExpress.XtraLayout.LayoutControlItem()
        Me.LayoutControlItem5 = New DevExpress.XtraLayout.LayoutControlItem()
        Me.LayoutControlItem6 = New DevExpress.XtraLayout.LayoutControlItem()
        CType(Me.LayoutControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.LayoutControl1.SuspendLayout()
        CType(Me.RestoreTextEdit.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.BackupLocationTextEdit.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Root, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.EmptySpaceItem1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlGroup1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem4, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem5, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem6, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'LayoutControl1
        '
        Me.LayoutControl1.Controls.Add(Me.RestoreSimpleButton)
        Me.LayoutControl1.Controls.Add(Me.BrowseRestoreSimpleButton)
        Me.LayoutControl1.Controls.Add(Me.RestoreTextEdit)
        Me.LayoutControl1.Controls.Add(Me.BackupSimpleButton)
        Me.LayoutControl1.Controls.Add(Me.BackupBrowseSimpleButton)
        Me.LayoutControl1.Controls.Add(Me.BackupLocationTextEdit)
        Me.LayoutControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LayoutControl1.Location = New System.Drawing.Point(0, 0)
        Me.LayoutControl1.Name = "LayoutControl1"
        Me.LayoutControl1.Root = Me.Root
        Me.LayoutControl1.Size = New System.Drawing.Size(559, 423)
        Me.LayoutControl1.TabIndex = 0
        Me.LayoutControl1.Text = "LayoutControl1"
        '
        'RestoreSimpleButton
        '
        Me.RestoreSimpleButton.Appearance.Font = New System.Drawing.Font("Trebuchet MS", 8.25!, System.Drawing.FontStyle.Bold)
        Me.RestoreSimpleButton.Appearance.Options.UseFont = True
        Me.RestoreSimpleButton.Location = New System.Drawing.Point(453, 69)
        Me.RestoreSimpleButton.Name = "RestoreSimpleButton"
        Me.RestoreSimpleButton.Size = New System.Drawing.Size(82, 22)
        Me.RestoreSimpleButton.StyleController = Me.LayoutControl1
        Me.RestoreSimpleButton.TabIndex = 9
        Me.RestoreSimpleButton.Text = "Restore"
        '
        'BrowseRestoreSimpleButton
        '
        Me.BrowseRestoreSimpleButton.Appearance.Font = New System.Drawing.Font("Trebuchet MS", 8.25!, System.Drawing.FontStyle.Bold)
        Me.BrowseRestoreSimpleButton.Appearance.Options.UseFont = True
        Me.BrowseRestoreSimpleButton.Location = New System.Drawing.Point(378, 69)
        Me.BrowseRestoreSimpleButton.Name = "BrowseRestoreSimpleButton"
        Me.BrowseRestoreSimpleButton.Size = New System.Drawing.Size(71, 22)
        Me.BrowseRestoreSimpleButton.StyleController = Me.LayoutControl1
        Me.BrowseRestoreSimpleButton.TabIndex = 8
        Me.BrowseRestoreSimpleButton.Text = "Browse"
        '
        'RestoreTextEdit
        '
        Me.RestoreTextEdit.Location = New System.Drawing.Point(108, 69)
        Me.RestoreTextEdit.Name = "RestoreTextEdit"
        Me.RestoreTextEdit.Properties.ReadOnly = True
        Me.RestoreTextEdit.Size = New System.Drawing.Size(266, 20)
        Me.RestoreTextEdit.StyleController = Me.LayoutControl1
        Me.RestoreTextEdit.TabIndex = 7
        '
        'BackupSimpleButton
        '
        Me.BackupSimpleButton.Appearance.Font = New System.Drawing.Font("Trebuchet MS", 8.25!, System.Drawing.FontStyle.Bold)
        Me.BackupSimpleButton.Appearance.Options.UseFont = True
        Me.BackupSimpleButton.Location = New System.Drawing.Point(453, 43)
        Me.BackupSimpleButton.Name = "BackupSimpleButton"
        Me.BackupSimpleButton.Size = New System.Drawing.Size(82, 22)
        Me.BackupSimpleButton.StyleController = Me.LayoutControl1
        Me.BackupSimpleButton.TabIndex = 6
        Me.BackupSimpleButton.Text = "Backup"
        '
        'BackupBrowseSimpleButton
        '
        Me.BackupBrowseSimpleButton.Appearance.Font = New System.Drawing.Font("Trebuchet MS", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BackupBrowseSimpleButton.Appearance.Options.UseFont = True
        Me.BackupBrowseSimpleButton.Location = New System.Drawing.Point(378, 43)
        Me.BackupBrowseSimpleButton.Name = "BackupBrowseSimpleButton"
        Me.BackupBrowseSimpleButton.Size = New System.Drawing.Size(71, 22)
        Me.BackupBrowseSimpleButton.StyleController = Me.LayoutControl1
        Me.BackupBrowseSimpleButton.TabIndex = 5
        Me.BackupBrowseSimpleButton.Text = "Browse"
        '
        'BackupLocationTextEdit
        '
        Me.BackupLocationTextEdit.Location = New System.Drawing.Point(108, 43)
        Me.BackupLocationTextEdit.Name = "BackupLocationTextEdit"
        Me.BackupLocationTextEdit.Properties.ReadOnly = True
        Me.BackupLocationTextEdit.Size = New System.Drawing.Size(266, 20)
        Me.BackupLocationTextEdit.StyleController = Me.LayoutControl1
        Me.BackupLocationTextEdit.TabIndex = 4
        '
        'Root
        '
        Me.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.[True]
        Me.Root.GroupBordersVisible = False
        Me.Root.Items.AddRange(New DevExpress.XtraLayout.BaseLayoutItem() {Me.EmptySpaceItem1, Me.LayoutControlGroup1})
        Me.Root.Name = "Root"
        Me.Root.Size = New System.Drawing.Size(559, 423)
        Me.Root.TextVisible = False
        '
        'EmptySpaceItem1
        '
        Me.EmptySpaceItem1.AllowHotTrack = False
        Me.EmptySpaceItem1.Location = New System.Drawing.Point(0, 95)
        Me.EmptySpaceItem1.Name = "EmptySpaceItem1"
        Me.EmptySpaceItem1.Size = New System.Drawing.Size(539, 308)
        Me.EmptySpaceItem1.TextSize = New System.Drawing.Size(0, 0)
        '
        'LayoutControlGroup1
        '
        Me.LayoutControlGroup1.Items.AddRange(New DevExpress.XtraLayout.BaseLayoutItem() {Me.LayoutControlItem4, Me.LayoutControlItem1, Me.LayoutControlItem2, Me.LayoutControlItem3, Me.LayoutControlItem5, Me.LayoutControlItem6})
        Me.LayoutControlGroup1.Location = New System.Drawing.Point(0, 0)
        Me.LayoutControlGroup1.Name = "LayoutControlGroup1"
        Me.LayoutControlGroup1.Size = New System.Drawing.Size(539, 95)
        Me.LayoutControlGroup1.Text = "Database Configurations"
        '
        'LayoutControlItem4
        '
        Me.LayoutControlItem4.Control = Me.RestoreTextEdit
        Me.LayoutControlItem4.Location = New System.Drawing.Point(0, 26)
        Me.LayoutControlItem4.Name = "LayoutControlItem4"
        Me.LayoutControlItem4.Size = New System.Drawing.Size(354, 26)
        Me.LayoutControlItem4.Text = "Restore Path:"
        Me.LayoutControlItem4.TextSize = New System.Drawing.Size(81, 13)
        '
        'LayoutControlItem1
        '
        Me.LayoutControlItem1.Control = Me.BackupLocationTextEdit
        Me.LayoutControlItem1.Location = New System.Drawing.Point(0, 0)
        Me.LayoutControlItem1.Name = "LayoutControlItem1"
        Me.LayoutControlItem1.Size = New System.Drawing.Size(354, 26)
        Me.LayoutControlItem1.Text = "Backup Location:"
        Me.LayoutControlItem1.TextSize = New System.Drawing.Size(81, 13)
        '
        'LayoutControlItem2
        '
        Me.LayoutControlItem2.Control = Me.BackupBrowseSimpleButton
        Me.LayoutControlItem2.Location = New System.Drawing.Point(354, 0)
        Me.LayoutControlItem2.Name = "LayoutControlItem2"
        Me.LayoutControlItem2.Size = New System.Drawing.Size(75, 26)
        Me.LayoutControlItem2.TextSize = New System.Drawing.Size(0, 0)
        Me.LayoutControlItem2.TextVisible = False
        '
        'LayoutControlItem3
        '
        Me.LayoutControlItem3.Control = Me.BackupSimpleButton
        Me.LayoutControlItem3.Location = New System.Drawing.Point(429, 0)
        Me.LayoutControlItem3.Name = "LayoutControlItem3"
        Me.LayoutControlItem3.Size = New System.Drawing.Size(86, 26)
        Me.LayoutControlItem3.TextSize = New System.Drawing.Size(0, 0)
        Me.LayoutControlItem3.TextVisible = False
        '
        'LayoutControlItem5
        '
        Me.LayoutControlItem5.Control = Me.BrowseRestoreSimpleButton
        Me.LayoutControlItem5.Location = New System.Drawing.Point(354, 26)
        Me.LayoutControlItem5.Name = "LayoutControlItem5"
        Me.LayoutControlItem5.Size = New System.Drawing.Size(75, 26)
        Me.LayoutControlItem5.TextSize = New System.Drawing.Size(0, 0)
        Me.LayoutControlItem5.TextVisible = False
        '
        'LayoutControlItem6
        '
        Me.LayoutControlItem6.Control = Me.RestoreSimpleButton
        Me.LayoutControlItem6.Location = New System.Drawing.Point(429, 26)
        Me.LayoutControlItem6.Name = "LayoutControlItem6"
        Me.LayoutControlItem6.Size = New System.Drawing.Size(86, 26)
        Me.LayoutControlItem6.TextSize = New System.Drawing.Size(0, 0)
        Me.LayoutControlItem6.TextVisible = False
        '
        'UCDatabase
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.LayoutControl1)
        Me.Name = "UCDatabase"
        Me.Size = New System.Drawing.Size(559, 423)
        CType(Me.LayoutControl1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.LayoutControl1.ResumeLayout(False)
        CType(Me.RestoreTextEdit.Properties, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.BackupLocationTextEdit.Properties, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Root, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.EmptySpaceItem1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlGroup1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem4, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem5, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem6, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents LayoutControl1 As DevExpress.XtraLayout.LayoutControl
    Friend WithEvents RestoreSimpleButton As DevExpress.XtraEditors.SimpleButton
    Friend WithEvents BrowseRestoreSimpleButton As DevExpress.XtraEditors.SimpleButton
    Friend WithEvents RestoreTextEdit As DevExpress.XtraEditors.TextEdit
    Friend WithEvents BackupSimpleButton As DevExpress.XtraEditors.SimpleButton
    Friend WithEvents BackupBrowseSimpleButton As DevExpress.XtraEditors.SimpleButton
    Friend WithEvents BackupLocationTextEdit As DevExpress.XtraEditors.TextEdit
    Friend WithEvents Root As DevExpress.XtraLayout.LayoutControlGroup
    Friend WithEvents EmptySpaceItem1 As DevExpress.XtraLayout.EmptySpaceItem
    Friend WithEvents LayoutControlGroup1 As DevExpress.XtraLayout.LayoutControlGroup
    Friend WithEvents LayoutControlItem4 As DevExpress.XtraLayout.LayoutControlItem
    Friend WithEvents LayoutControlItem1 As DevExpress.XtraLayout.LayoutControlItem
    Friend WithEvents LayoutControlItem2 As DevExpress.XtraLayout.LayoutControlItem
    Friend WithEvents LayoutControlItem3 As DevExpress.XtraLayout.LayoutControlItem
    Friend WithEvents LayoutControlItem5 As DevExpress.XtraLayout.LayoutControlItem
    Friend WithEvents LayoutControlItem6 As DevExpress.XtraLayout.LayoutControlItem
End Class
