<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UCGeneralMessage
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
        Me.ResetSimpleButton = New DevExpress.XtraEditors.SimpleButton()
        Me.SaveSimpleButton = New DevExpress.XtraEditors.SimpleButton()
        Me.LayoutControl1 = New DevExpress.XtraLayout.LayoutControl()
        Me.Root = New DevExpress.XtraLayout.LayoutControlGroup()
        Me.MembersCheckedComboBoxEdit = New DevExpress.XtraEditors.CheckedComboBoxEdit()
        Me.LayoutControlItem1 = New DevExpress.XtraLayout.LayoutControlItem()
        Me.EmptySpaceItem1 = New DevExpress.XtraLayout.EmptySpaceItem()
        Me.MessageMemoEdit = New DevExpress.XtraEditors.MemoEdit()
        Me.LayoutControlItem2 = New DevExpress.XtraLayout.LayoutControlItem()
        CType(Me.PanelControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PanelControl1.SuspendLayout()
        CType(Me.LayoutControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.LayoutControl1.SuspendLayout()
        CType(Me.Root, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.MembersCheckedComboBoxEdit.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.EmptySpaceItem1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.MessageMemoEdit.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PanelControl1
        '
        Me.PanelControl1.Controls.Add(Me.ResetSimpleButton)
        Me.PanelControl1.Controls.Add(Me.SaveSimpleButton)
        Me.PanelControl1.Dock = System.Windows.Forms.DockStyle.Top
        Me.PanelControl1.Location = New System.Drawing.Point(0, 0)
        Me.PanelControl1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.PanelControl1.Name = "PanelControl1"
        Me.PanelControl1.Size = New System.Drawing.Size(1126, 42)
        Me.PanelControl1.TabIndex = 8
        '
        'ResetSimpleButton
        '
        Me.ResetSimpleButton.Appearance.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ResetSimpleButton.Appearance.Options.UseFont = True
        Me.ResetSimpleButton.Location = New System.Drawing.Point(102, 5)
        Me.ResetSimpleButton.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.ResetSimpleButton.Name = "ResetSimpleButton"
        Me.ResetSimpleButton.Size = New System.Drawing.Size(94, 30)
        Me.ResetSimpleButton.TabIndex = 6
        Me.ResetSimpleButton.Text = "&Reset"
        '
        'SaveSimpleButton
        '
        Me.SaveSimpleButton.Appearance.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.SaveSimpleButton.Appearance.Options.UseFont = True
        Me.SaveSimpleButton.Location = New System.Drawing.Point(5, 5)
        Me.SaveSimpleButton.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.SaveSimpleButton.Name = "SaveSimpleButton"
        Me.SaveSimpleButton.Size = New System.Drawing.Size(94, 30)
        Me.SaveSimpleButton.TabIndex = 0
        Me.SaveSimpleButton.Text = "Se&nd"
        '
        'LayoutControl1
        '
        Me.LayoutControl1.Controls.Add(Me.MessageMemoEdit)
        Me.LayoutControl1.Controls.Add(Me.MembersCheckedComboBoxEdit)
        Me.LayoutControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LayoutControl1.Location = New System.Drawing.Point(0, 42)
        Me.LayoutControl1.Name = "LayoutControl1"
        Me.LayoutControl1.Root = Me.Root
        Me.LayoutControl1.Size = New System.Drawing.Size(1126, 675)
        Me.LayoutControl1.TabIndex = 9
        Me.LayoutControl1.Text = "LayoutControl1"
        '
        'Root
        '
        Me.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.[True]
        Me.Root.GroupBordersVisible = False
        Me.Root.Items.AddRange(New DevExpress.XtraLayout.BaseLayoutItem() {Me.LayoutControlItem1, Me.EmptySpaceItem1, Me.LayoutControlItem2})
        Me.Root.Name = "Root"
        Me.Root.Size = New System.Drawing.Size(1126, 675)
        Me.Root.TextVisible = False
        '
        'MembersCheckedComboBoxEdit
        '
        Me.MembersCheckedComboBoxEdit.Location = New System.Drawing.Point(74, 12)
        Me.MembersCheckedComboBoxEdit.Name = "MembersCheckedComboBoxEdit"
        Me.MembersCheckedComboBoxEdit.Properties.Buttons.AddRange(New DevExpress.XtraEditors.Controls.EditorButton() {New DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)})
        Me.MembersCheckedComboBoxEdit.Size = New System.Drawing.Size(1040, 22)
        Me.MembersCheckedComboBoxEdit.StyleController = Me.LayoutControl1
        Me.MembersCheckedComboBoxEdit.TabIndex = 4
        '
        'LayoutControlItem1
        '
        Me.LayoutControlItem1.Control = Me.MembersCheckedComboBoxEdit
        Me.LayoutControlItem1.Location = New System.Drawing.Point(0, 0)
        Me.LayoutControlItem1.Name = "LayoutControlItem1"
        Me.LayoutControlItem1.Size = New System.Drawing.Size(1106, 26)
        Me.LayoutControlItem1.Text = "Members:"
        Me.LayoutControlItem1.TextSize = New System.Drawing.Size(58, 16)
        '
        'EmptySpaceItem1
        '
        Me.EmptySpaceItem1.AllowHotTrack = False
        Me.EmptySpaceItem1.Location = New System.Drawing.Point(0, 295)
        Me.EmptySpaceItem1.Name = "EmptySpaceItem1"
        Me.EmptySpaceItem1.Size = New System.Drawing.Size(1106, 360)
        Me.EmptySpaceItem1.TextSize = New System.Drawing.Size(0, 0)
        '
        'MessageMemoEdit
        '
        Me.MessageMemoEdit.Location = New System.Drawing.Point(74, 38)
        Me.MessageMemoEdit.Name = "MessageMemoEdit"
        Me.MessageMemoEdit.Size = New System.Drawing.Size(1040, 265)
        Me.MessageMemoEdit.StyleController = Me.LayoutControl1
        Me.MessageMemoEdit.TabIndex = 5
        '
        'LayoutControlItem2
        '
        Me.LayoutControlItem2.Control = Me.MessageMemoEdit
        Me.LayoutControlItem2.Location = New System.Drawing.Point(0, 26)
        Me.LayoutControlItem2.Name = "LayoutControlItem2"
        Me.LayoutControlItem2.Size = New System.Drawing.Size(1106, 269)
        Me.LayoutControlItem2.Text = "Message:"
        Me.LayoutControlItem2.TextSize = New System.Drawing.Size(58, 16)
        '
        'UCGeneralMessage
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.LayoutControl1)
        Me.Controls.Add(Me.PanelControl1)
        Me.Name = "UCGeneralMessage"
        Me.Size = New System.Drawing.Size(1126, 717)
        CType(Me.PanelControl1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PanelControl1.ResumeLayout(False)
        CType(Me.LayoutControl1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.LayoutControl1.ResumeLayout(False)
        CType(Me.Root, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.MembersCheckedComboBoxEdit.Properties, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.EmptySpaceItem1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.MessageMemoEdit.Properties, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents PanelControl1 As DevExpress.XtraEditors.PanelControl
    Friend WithEvents ResetSimpleButton As DevExpress.XtraEditors.SimpleButton
    Friend WithEvents SaveSimpleButton As DevExpress.XtraEditors.SimpleButton
    Friend WithEvents LayoutControl1 As DevExpress.XtraLayout.LayoutControl
    Friend WithEvents MessageMemoEdit As DevExpress.XtraEditors.MemoEdit
    Friend WithEvents MembersCheckedComboBoxEdit As DevExpress.XtraEditors.CheckedComboBoxEdit
    Friend WithEvents Root As DevExpress.XtraLayout.LayoutControlGroup
    Friend WithEvents LayoutControlItem1 As DevExpress.XtraLayout.LayoutControlItem
    Friend WithEvents EmptySpaceItem1 As DevExpress.XtraLayout.EmptySpaceItem
    Friend WithEvents LayoutControlItem2 As DevExpress.XtraLayout.LayoutControlItem
End Class
