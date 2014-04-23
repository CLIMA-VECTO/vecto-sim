<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_GBX_GearDlog
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
        Me.TbGear = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TbRatio = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TbMapPath = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.BtBrowse = New System.Windows.Forms.Button()
        Me.BtNext = New System.Windows.Forms.Button()
        Me.ChIsTCgear = New System.Windows.Forms.CheckBox()
        Me.TbShiftPolyFile = New System.Windows.Forms.TextBox()
        Me.BtShiftPolyBrowse = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.PnShiftPoly = New System.Windows.Forms.Panel()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.PnShiftPoly.SuspendLayout()
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
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(380, 146)
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
        'TbGear
        '
        Me.TbGear.Location = New System.Drawing.Point(48, 6)
        Me.TbGear.Name = "TbGear"
        Me.TbGear.ReadOnly = True
        Me.TbGear.Size = New System.Drawing.Size(37, 20)
        Me.TbGear.TabIndex = 1
        Me.TbGear.TabStop = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(30, 13)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Gear"
        '
        'TbRatio
        '
        Me.TbRatio.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TbRatio.Location = New System.Drawing.Point(149, 6)
        Me.TbRatio.Name = "TbRatio"
        Me.TbRatio.Size = New System.Drawing.Size(66, 20)
        Me.TbRatio.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(111, 9)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(32, 13)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Ratio"
        '
        'TbMapPath
        '
        Me.TbMapPath.Location = New System.Drawing.Point(12, 61)
        Me.TbMapPath.Name = "TbMapPath"
        Me.TbMapPath.Size = New System.Drawing.Size(476, 20)
        Me.TbMapPath.TabIndex = 5
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 45)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(126, 13)
        Me.Label3.TabIndex = 7
        Me.Label3.Text = "Loss Map or Efficiency [-]"
        '
        'BtBrowse
        '
        Me.BtBrowse.Location = New System.Drawing.Point(494, 59)
        Me.BtBrowse.Name = "BtBrowse"
        Me.BtBrowse.Size = New System.Drawing.Size(32, 23)
        Me.BtBrowse.TabIndex = 8
        Me.BtBrowse.Text = "..."
        Me.BtBrowse.UseVisualStyleBackColor = True
        '
        'BtNext
        '
        Me.BtNext.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.BtNext.Location = New System.Drawing.Point(307, 149)
        Me.BtNext.Name = "BtNext"
        Me.BtNext.Size = New System.Drawing.Size(67, 23)
        Me.BtNext.TabIndex = 9
        Me.BtNext.Text = "&Next"
        Me.BtNext.UseVisualStyleBackColor = True
        '
        'ChIsTCgear
        '
        Me.ChIsTCgear.AutoSize = True
        Me.ChIsTCgear.Location = New System.Drawing.Point(236, 8)
        Me.ChIsTCgear.Name = "ChIsTCgear"
        Me.ChIsTCgear.Size = New System.Drawing.Size(241, 17)
        Me.ChIsTCgear.TabIndex = 10
        Me.ChIsTCgear.Text = "Torque Conveter active (lock-up clutch open)"
        Me.ChIsTCgear.UseVisualStyleBackColor = True
        '
        'TbShiftPolyFile
        '
        Me.TbShiftPolyFile.Location = New System.Drawing.Point(2, 17)
        Me.TbShiftPolyFile.Name = "TbShiftPolyFile"
        Me.TbShiftPolyFile.Size = New System.Drawing.Size(476, 20)
        Me.TbShiftPolyFile.TabIndex = 12
        '
        'BtShiftPolyBrowse
        '
        Me.BtShiftPolyBrowse.Location = New System.Drawing.Point(484, 15)
        Me.BtShiftPolyBrowse.Name = "BtShiftPolyBrowse"
        Me.BtShiftPolyBrowse.Size = New System.Drawing.Size(32, 23)
        Me.BtShiftPolyBrowse.TabIndex = 13
        Me.BtShiftPolyBrowse.TabStop = False
        Me.BtShiftPolyBrowse.Text = "..."
        Me.BtShiftPolyBrowse.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(2, 1)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(89, 13)
        Me.Label4.TabIndex = 11
        Me.Label4.Text = "Shift polygons file"
        '
        'PnShiftPoly
        '
        Me.PnShiftPoly.Controls.Add(Me.TbShiftPolyFile)
        Me.PnShiftPoly.Controls.Add(Me.BtShiftPolyBrowse)
        Me.PnShiftPoly.Controls.Add(Me.Label4)
        Me.PnShiftPoly.Location = New System.Drawing.Point(10, 90)
        Me.PnShiftPoly.Name = "PnShiftPoly"
        Me.PnShiftPoly.Size = New System.Drawing.Size(525, 46)
        Me.PnShiftPoly.TabIndex = 14
        '
        'F_GBX_GearDlog
        '
        Me.AcceptButton = Me.OK_Button
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Cancel_Button
        Me.ClientSize = New System.Drawing.Size(538, 187)
        Me.Controls.Add(Me.PnShiftPoly)
        Me.Controls.Add(Me.ChIsTCgear)
        Me.Controls.Add(Me.BtNext)
        Me.Controls.Add(Me.BtBrowse)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.TbMapPath)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.TbRatio)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TbGear)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "F_GBX_GearDlog"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Edit Gears"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.PnShiftPoly.ResumeLayout(False)
        Me.PnShiftPoly.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents TbGear As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents TbRatio As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TbMapPath As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents BtBrowse As System.Windows.Forms.Button
    Friend WithEvents BtNext As System.Windows.Forms.Button
    Friend WithEvents ChIsTCgear As System.Windows.Forms.CheckBox
    Friend WithEvents TbShiftPolyFile As System.Windows.Forms.TextBox
    Friend WithEvents BtShiftPolyBrowse As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents PnShiftPoly As System.Windows.Forms.Panel

End Class
