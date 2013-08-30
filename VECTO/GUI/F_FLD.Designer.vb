<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_FLD
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.OK_Button = New System.Windows.Forms.Button()
        Me.Cancel_Button = New System.Windows.Forms.Button()
        Me.BtFLDOpen = New System.Windows.Forms.Button()
        Me.BtFLD = New System.Windows.Forms.Button()
        Me.TbFLD = New System.Windows.Forms.TextBox()
        Me.CmOpenFile = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.OpenWithGRAPHiToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.OpenWithToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ShowInFolderToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.NumGearTo = New System.Windows.Forms.NumericUpDown()
        Me.NumGearFrom = New System.Windows.Forms.NumericUpDown()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.CmOpenFile.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        CType(Me.NumGearTo, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.NumGearFrom, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.OK_Button, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Cancel_Button, 1, 0)
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(345, 131)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 1
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(146, 29)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'OK_Button
        '
        Me.OK_Button.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.OK_Button.Location = New System.Drawing.Point(3, 3)
        Me.OK_Button.Name = "OK_Button"
        Me.OK_Button.Size = New System.Drawing.Size(67, 23)
        Me.OK_Button.TabIndex = 0
        Me.OK_Button.Text = "OK"
        '
        'Cancel_Button
        '
        Me.Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel_Button.Location = New System.Drawing.Point(76, 3)
        Me.Cancel_Button.Name = "Cancel_Button"
        Me.Cancel_Button.Size = New System.Drawing.Size(67, 23)
        Me.Cancel_Button.TabIndex = 1
        Me.Cancel_Button.Text = "Cancel"
        '
        'BtFLDOpen
        '
        Me.BtFLDOpen.Image = Global.VECTO.My.Resources.Resources.application_export_icon_small
        Me.BtFLDOpen.Location = New System.Drawing.Point(470, 23)
        Me.BtFLDOpen.Name = "BtFLDOpen"
        Me.BtFLDOpen.Size = New System.Drawing.Size(23, 23)
        Me.BtFLDOpen.TabIndex = 43
        Me.BtFLDOpen.TabStop = False
        Me.BtFLDOpen.UseVisualStyleBackColor = True
        '
        'BtFLD
        '
        Me.BtFLD.Location = New System.Drawing.Point(436, 23)
        Me.BtFLD.Name = "BtFLD"
        Me.BtFLD.Size = New System.Drawing.Size(28, 23)
        Me.BtFLD.TabIndex = 42
        Me.BtFLD.TabStop = False
        Me.BtFLD.Text = "..."
        Me.BtFLD.UseVisualStyleBackColor = True
        '
        'TbFLD
        '
        Me.TbFLD.Location = New System.Drawing.Point(12, 25)
        Me.TbFLD.Name = "TbFLD"
        Me.TbFLD.Size = New System.Drawing.Size(418, 20)
        Me.TbFLD.TabIndex = 41
        '
        'CmOpenFile
        '
        Me.CmOpenFile.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OpenWithGRAPHiToolStripMenuItem, Me.OpenWithToolStripMenuItem, Me.ShowInFolderToolStripMenuItem})
        Me.CmOpenFile.Name = "CmOpenFile"
        Me.CmOpenFile.Size = New System.Drawing.Size(175, 70)
        '
        'OpenWithGRAPHiToolStripMenuItem
        '
        Me.OpenWithGRAPHiToolStripMenuItem.Name = "OpenWithGRAPHiToolStripMenuItem"
        Me.OpenWithGRAPHiToolStripMenuItem.Size = New System.Drawing.Size(174, 22)
        Me.OpenWithGRAPHiToolStripMenuItem.Text = "Open with GRAPHi"
        '
        'OpenWithToolStripMenuItem
        '
        Me.OpenWithToolStripMenuItem.Name = "OpenWithToolStripMenuItem"
        Me.OpenWithToolStripMenuItem.Size = New System.Drawing.Size(174, 22)
        Me.OpenWithToolStripMenuItem.Text = "Open with ..."
        '
        'ShowInFolderToolStripMenuItem
        '
        Me.ShowInFolderToolStripMenuItem.Name = "ShowInFolderToolStripMenuItem"
        Me.ShowInFolderToolStripMenuItem.Size = New System.Drawing.Size(174, 22)
        Me.ShowInFolderToolStripMenuItem.Text = "Show in Folder"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 9)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(157, 13)
        Me.Label4.TabIndex = 45
        Me.Label4.Text = "Full Load and Drag Curve (.vfld)"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.NumGearTo)
        Me.GroupBox1.Controls.Add(Me.NumGearFrom)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 51)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(479, 64)
        Me.GroupBox1.TabIndex = 46
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Assigned Gears"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(129, 31)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(20, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "To"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(14, 31)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(30, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "From"
        '
        'NumGearTo
        '
        Me.NumGearTo.Location = New System.Drawing.Point(155, 29)
        Me.NumGearTo.Maximum = New Decimal(New Integer() {99, 0, 0, 0})
        Me.NumGearTo.Name = "NumGearTo"
        Me.NumGearTo.Size = New System.Drawing.Size(51, 20)
        Me.NumGearTo.TabIndex = 0
        '
        'NumGearFrom
        '
        Me.NumGearFrom.Location = New System.Drawing.Point(50, 29)
        Me.NumGearFrom.Maximum = New Decimal(New Integer() {99, 0, 0, 0})
        Me.NumGearFrom.Name = "NumGearFrom"
        Me.NumGearFrom.Size = New System.Drawing.Size(51, 20)
        Me.NumGearFrom.TabIndex = 0
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(225, 31)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(245, 13)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Gear 0 file is used for Idling and Engine Only Mode"
        '
        'F_FLD
        '
        Me.AcceptButton = Me.OK_Button
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Cancel_Button
        Me.ClientSize = New System.Drawing.Size(503, 172)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.BtFLDOpen)
        Me.Controls.Add(Me.BtFLD)
        Me.Controls.Add(Me.TbFLD)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "F_FLD"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Full load and Drag Curve"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.CmOpenFile.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.NumGearTo, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.NumGearFrom, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents BtFLDOpen As System.Windows.Forms.Button
    Friend WithEvents BtFLD As System.Windows.Forms.Button
    Friend WithEvents TbFLD As System.Windows.Forms.TextBox
    Friend WithEvents CmOpenFile As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents OpenWithGRAPHiToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenWithToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ShowInFolderToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents NumGearTo As System.Windows.Forms.NumericUpDown
    Friend WithEvents NumGearFrom As System.Windows.Forms.NumericUpDown
    Friend WithEvents Label3 As System.Windows.Forms.Label

End Class
