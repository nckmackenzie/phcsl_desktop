<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UCCloneRights
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
        Me.FromLookUpEdit = New DevExpress.XtraEditors.LookUpEdit()
        Me.LayoutControlItem1 = New DevExpress.XtraLayout.LayoutControlItem()
        Me.EmptySpaceItem1 = New DevExpress.XtraLayout.EmptySpaceItem()
        Me.ToLookUpEdit = New DevExpress.XtraEditors.LookUpEdit()
        Me.LayoutControlItem2 = New DevExpress.XtraLayout.LayoutControlItem()
        Me.LayoutControlGroup1 = New DevExpress.XtraLayout.LayoutControlGroup()
        CType(Me.PanelControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.PanelControl1.SuspendLayout()
        CType(Me.LayoutControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.LayoutControl1.SuspendLayout()
        CType(Me.Root, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.FromLookUpEdit.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.EmptySpaceItem1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.ToLookUpEdit.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlItem2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.LayoutControlGroup1, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.PanelControl1.Size = New System.Drawing.Size(725, 33)
        Me.PanelControl1.TabIndex = 8
        '
        'ResetSimpleButton
        '
        Me.ResetSimpleButton.Appearance.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ResetSimpleButton.Appearance.Options.UseFont = True
        Me.ResetSimpleButton.Location = New System.Drawing.Point(90, 4)
        Me.ResetSimpleButton.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.ResetSimpleButton.Name = "ResetSimpleButton"
        Me.ResetSimpleButton.Size = New System.Drawing.Size(81, 24)
        Me.ResetSimpleButton.TabIndex = 6
        Me.ResetSimpleButton.Text = "&Reset"
        '
        'SaveSimpleButton
        '
        Me.SaveSimpleButton.Appearance.Font = New System.Drawing.Font("Trebuchet MS", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.SaveSimpleButton.Appearance.Options.UseFont = True
        Me.SaveSimpleButton.Location = New System.Drawing.Point(4, 4)
        Me.SaveSimpleButton.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.SaveSimpleButton.Name = "SaveSimpleButton"
        Me.SaveSimpleButton.Size = New System.Drawing.Size(81, 24)
        Me.SaveSimpleButton.TabIndex = 0
        Me.SaveSimpleButton.Text = "Sa&ve"
        '
        'LayoutControl1
        '
        Me.LayoutControl1.Controls.Add(Me.ToLookUpEdit)
        Me.LayoutControl1.Controls.Add(Me.FromLookUpEdit)
        Me.LayoutControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.LayoutControl1.Location = New System.Drawing.Point(0, 33)
        Me.LayoutControl1.Name = "LayoutControl1"
        Me.LayoutControl1.Root = Me.Root
        Me.LayoutControl1.Size = New System.Drawing.Size(725, 491)
        Me.LayoutControl1.TabIndex = 9
        Me.LayoutControl1.Text = "LayoutControl1"
        '
        'Root
        '
        Me.Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.[True]
        Me.Root.GroupBordersVisible = False
        Me.Root.Items.AddRange(New DevExpress.XtraLayout.BaseLayoutItem() {Me.EmptySpaceItem1, Me.LayoutControlGroup1})
        Me.Root.Name = "Root"
        Me.Root.Size = New System.Drawing.Size(725, 491)
        Me.Root.TextVisible = False
        '
        'FromLookUpEdit
        '
        Me.FromLookUpEdit.Location = New System.Drawing.Point(84, 43)
        Me.FromLookUpEdit.Name = "FromLookUpEdit"
        Me.FromLookUpEdit.Properties.Buttons.AddRange(New DevExpress.XtraEditors.Controls.EditorButton() {New DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)})
        Me.FromLookUpEdit.Properties.Columns.AddRange(New DevExpress.XtraEditors.Controls.LookUpColumnInfo() {New DevExpress.XtraEditors.Controls.LookUpColumnInfo("ID", "Name5", 20, DevExpress.Utils.FormatType.None, "", False, DevExpress.Utils.HorzAlignment.[Default], DevExpress.Data.ColumnSortOrder.None, DevExpress.Utils.DefaultBoolean.[Default]), New DevExpress.XtraEditors.Controls.LookUpColumnInfo("userName", "User")})
        Me.FromLookUpEdit.Properties.NullText = ""
        Me.FromLookUpEdit.Properties.NullValuePrompt = "Select user to copy rights from..."
        Me.FromLookUpEdit.Size = New System.Drawing.Size(617, 20)
        Me.FromLookUpEdit.StyleController = Me.LayoutControl1
        Me.FromLookUpEdit.TabIndex = 4
        '
        'LayoutControlItem1
        '
        Me.LayoutControlItem1.Control = Me.FromLookUpEdit
        Me.LayoutControlItem1.Location = New System.Drawing.Point(0, 0)
        Me.LayoutControlItem1.Name = "LayoutControlItem1"
        Me.LayoutControlItem1.Size = New System.Drawing.Size(681, 24)
        Me.LayoutControlItem1.Text = "Copy From:"
        Me.LayoutControlItem1.TextSize = New System.Drawing.Size(56, 13)
        '
        'EmptySpaceItem1
        '
        Me.EmptySpaceItem1.AllowHotTrack = False
        Me.EmptySpaceItem1.Location = New System.Drawing.Point(0, 91)
        Me.EmptySpaceItem1.Name = "EmptySpaceItem1"
        Me.EmptySpaceItem1.Size = New System.Drawing.Size(705, 380)
        Me.EmptySpaceItem1.TextSize = New System.Drawing.Size(0, 0)
        '
        'ToLookUpEdit
        '
        Me.ToLookUpEdit.Location = New System.Drawing.Point(84, 67)
        Me.ToLookUpEdit.Name = "ToLookUpEdit"
        Me.ToLookUpEdit.Properties.Buttons.AddRange(New DevExpress.XtraEditors.Controls.EditorButton() {New DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)})
        Me.ToLookUpEdit.Properties.Columns.AddRange(New DevExpress.XtraEditors.Controls.LookUpColumnInfo() {New DevExpress.XtraEditors.Controls.LookUpColumnInfo("ID", "Name7", 20, DevExpress.Utils.FormatType.None, "", False, DevExpress.Utils.HorzAlignment.[Default], DevExpress.Data.ColumnSortOrder.None, DevExpress.Utils.DefaultBoolean.[Default]), New DevExpress.XtraEditors.Controls.LookUpColumnInfo("userName", "User")})
        Me.ToLookUpEdit.Properties.NullText = ""
        Me.ToLookUpEdit.Properties.NullValuePrompt = "Select user to copy rights to..."
        Me.ToLookUpEdit.Size = New System.Drawing.Size(617, 20)
        Me.ToLookUpEdit.StyleController = Me.LayoutControl1
        Me.ToLookUpEdit.TabIndex = 5
        '
        'LayoutControlItem2
        '
        Me.LayoutControlItem2.Control = Me.ToLookUpEdit
        Me.LayoutControlItem2.Location = New System.Drawing.Point(0, 24)
        Me.LayoutControlItem2.Name = "LayoutControlItem2"
        Me.LayoutControlItem2.Size = New System.Drawing.Size(681, 24)
        Me.LayoutControlItem2.Text = "Copy To:"
        Me.LayoutControlItem2.TextSize = New System.Drawing.Size(56, 13)
        '
        'LayoutControlGroup1
        '
        Me.LayoutControlGroup1.Items.AddRange(New DevExpress.XtraLayout.BaseLayoutItem() {Me.LayoutControlItem2, Me.LayoutControlItem1})
        Me.LayoutControlGroup1.Location = New System.Drawing.Point(0, 0)
        Me.LayoutControlGroup1.Name = "LayoutControlGroup1"
        Me.LayoutControlGroup1.Size = New System.Drawing.Size(705, 91)
        Me.LayoutControlGroup1.Text = "Clone Rights"
        '
        'UCCloneRights
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.LayoutControl1)
        Me.Controls.Add(Me.PanelControl1)
        Me.Name = "UCCloneRights"
        Me.Size = New System.Drawing.Size(725, 524)
        CType(Me.PanelControl1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.PanelControl1.ResumeLayout(False)
        CType(Me.LayoutControl1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.LayoutControl1.ResumeLayout(False)
        CType(Me.Root, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.FromLookUpEdit.Properties, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.EmptySpaceItem1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.ToLookUpEdit.Properties, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlItem2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.LayoutControlGroup1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents PanelControl1 As DevExpress.XtraEditors.PanelControl
    Friend WithEvents ResetSimpleButton As DevExpress.XtraEditors.SimpleButton
    Friend WithEvents SaveSimpleButton As DevExpress.XtraEditors.SimpleButton
    Friend WithEvents LayoutControl1 As DevExpress.XtraLayout.LayoutControl
    Friend WithEvents ToLookUpEdit As DevExpress.XtraEditors.LookUpEdit
    Friend WithEvents FromLookUpEdit As DevExpress.XtraEditors.LookUpEdit
    Friend WithEvents Root As DevExpress.XtraLayout.LayoutControlGroup
    Friend WithEvents EmptySpaceItem1 As DevExpress.XtraLayout.EmptySpaceItem
    Friend WithEvents LayoutControlGroup1 As DevExpress.XtraLayout.LayoutControlGroup
    Friend WithEvents LayoutControlItem2 As DevExpress.XtraLayout.LayoutControlItem
    Friend WithEvents LayoutControlItem1 As DevExpress.XtraLayout.LayoutControlItem
End Class
