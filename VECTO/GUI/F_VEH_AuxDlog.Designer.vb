<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_VEH_AuxDlog
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
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.OK_Button = New System.Windows.Forms.Button()
        Me.Cancel_Button = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.TbID = New System.Windows.Forms.TextBox()
        Me.TbPath = New System.Windows.Forms.TextBox()
        Me.BtBrowse = New System.Windows.Forms.Button()
        Me.CbType = New System.Windows.Forms.ComboBox()
        Me.LbIDhelp = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.CbTech = New System.Windows.Forms.ComboBox()
        Me.PnTech = New System.Windows.Forms.Panel()
        Me.PnFile = New System.Windows.Forms.Panel()
        Me.LVTech = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.TableLayoutPanel1.SuspendLayout()
        Me.PnTech.SuspendLayout()
        Me.PnFile.SuspendLayout()
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
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(301, 388)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 1
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(146, 29)
        Me.TableLayoutPanel1.TabIndex = 25
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
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(171, 10)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(18, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "ID"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(9, 10)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(31, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Type"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(7, 39)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(50, 13)
        Me.Label3.TabIndex = 1
        Me.Label3.Text = "Input File"
        '
        'TbID
        '
        Me.TbID.Location = New System.Drawing.Point(195, 7)
        Me.TbID.Name = "TbID"
        Me.TbID.Size = New System.Drawing.Size(39, 20)
        Me.TbID.TabIndex = 5
        '
        'TbPath
        '
        Me.TbPath.Location = New System.Drawing.Point(7, 55)
        Me.TbPath.Name = "TbPath"
        Me.TbPath.Size = New System.Drawing.Size(383, 20)
        Me.TbPath.TabIndex = 10
        '
        'BtBrowse
        '
        Me.BtBrowse.Location = New System.Drawing.Point(396, 53)
        Me.BtBrowse.Name = "BtBrowse"
        Me.BtBrowse.Size = New System.Drawing.Size(32, 23)
        Me.BtBrowse.TabIndex = 15
        Me.BtBrowse.Text = "..."
        Me.BtBrowse.UseVisualStyleBackColor = True
        '
        'CbType
        '
        Me.CbType.FormattingEnabled = True
        Me.CbType.Location = New System.Drawing.Point(46, 7)
        Me.CbType.Name = "CbType"
        Me.CbType.Size = New System.Drawing.Size(109, 21)
        Me.CbType.TabIndex = 0
        '
        'LbIDhelp
        '
        Me.LbIDhelp.AutoSize = True
        Me.LbIDhelp.Location = New System.Drawing.Point(240, 10)
        Me.LbIDhelp.Name = "LbIDhelp"
        Me.LbIDhelp.Size = New System.Drawing.Size(0, 13)
        Me.LbIDhelp.TabIndex = 26
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(6, 9)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(63, 13)
        Me.Label4.TabIndex = 1
        Me.Label4.Text = "Technology"
        '
        'CbTech
        '
        Me.CbTech.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CbTech.FormattingEnabled = True
        Me.CbTech.Location = New System.Drawing.Point(75, 6)
        Me.CbTech.Name = "CbTech"
        Me.CbTech.Size = New System.Drawing.Size(352, 21)
        Me.CbTech.TabIndex = 27
        '
        'PnTech
        '
        Me.PnTech.Controls.Add(Me.CbTech)
        Me.PnTech.Controls.Add(Me.Label4)
        Me.PnTech.Location = New System.Drawing.Point(12, 12)
        Me.PnTech.Name = "PnTech"
        Me.PnTech.Size = New System.Drawing.Size(435, 34)
        Me.PnTech.TabIndex = 28
        '
        'PnFile
        '
        Me.PnFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.PnFile.Controls.Add(Me.LbIDhelp)
        Me.PnFile.Controls.Add(Me.BtBrowse)
        Me.PnFile.Controls.Add(Me.CbType)
        Me.PnFile.Controls.Add(Me.TbID)
        Me.PnFile.Controls.Add(Me.TbPath)
        Me.PnFile.Controls.Add(Me.Label2)
        Me.PnFile.Controls.Add(Me.Label3)
        Me.PnFile.Controls.Add(Me.Label1)
        Me.PnFile.Location = New System.Drawing.Point(12, 293)
        Me.PnFile.Name = "PnFile"
        Me.PnFile.Size = New System.Drawing.Size(435, 89)
        Me.PnFile.TabIndex = 29
        '
        'LVTech
        '
        Me.LVTech.CheckBoxes = True
        Me.LVTech.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1})
        Me.LVTech.FullRowSelect = True
        Me.LVTech.GridLines = True
        Me.LVTech.Location = New System.Drawing.Point(12, 52)
        Me.LVTech.Name = "LVTech"
        Me.LVTech.Size = New System.Drawing.Size(435, 235)
        Me.LVTech.TabIndex = 30
        Me.LVTech.UseCompatibleStateImageBehavior = False
        Me.LVTech.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Technologies"
        Me.ColumnHeader1.Width = 420
        '
        'F_VEH_AuxDlog
        '
        Me.AcceptButton = Me.OK_Button
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Cancel_Button
        Me.ClientSize = New System.Drawing.Size(459, 429)
        Me.Controls.Add(Me.LVTech)
        Me.Controls.Add(Me.PnFile)
        Me.Controls.Add(Me.PnTech)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "F_VEH_AuxDlog"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Auxiliary"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.PnTech.ResumeLayout(False)
        Me.PnTech.PerformLayout()
        Me.PnFile.ResumeLayout(False)
        Me.PnFile.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents TbID As System.Windows.Forms.TextBox
    Friend WithEvents TbPath As System.Windows.Forms.TextBox
    Friend WithEvents BtBrowse As System.Windows.Forms.Button
    Friend WithEvents CbType As System.Windows.Forms.ComboBox
    Friend WithEvents LbIDhelp As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents CbTech As System.Windows.Forms.ComboBox
    Friend WithEvents PnTech As System.Windows.Forms.Panel
    Friend WithEvents PnFile As System.Windows.Forms.Panel
    Friend WithEvents LVTech As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader

End Class
