<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_ADVfzp
    Inherits System.Windows.Forms.Form

    'Das Formular Ã¼berschreibt den LÃ¶schvorgang, um die Komponentenliste zu bereinigen.
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

    'Wird vom Windows Form-Designer benÃ¶tigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist fÃ¼r den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer mÃ¶glich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht mÃ¶glich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel
        Me.OK_Button = New System.Windows.Forms.Button
        Me.Cancel_Button = New System.Windows.Forms.Button
        Me.RbUseSFZP = New System.Windows.Forms.RadioButton
        Me.RbUseFZP = New System.Windows.Forms.RadioButton
        Me.RBAbort = New System.Windows.Forms.RadioButton
        Me.LbTimer = New System.Windows.Forms.Label
        Me.LbMsg = New System.Windows.Forms.Label
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.TableLayoutPanel1.SuspendLayout()
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
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(194, 134)
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
        'RbUseSFZP
        '
        Me.RbUseSFZP.AutoSize = True
        Me.RbUseSFZP.Checked = True
        Me.RbUseSFZP.Location = New System.Drawing.Point(20, 48)
        Me.RbUseSFZP.Name = "RbUseSFZP"
        Me.RbUseSFZP.Size = New System.Drawing.Size(187, 17)
        Me.RbUseSFZP.TabIndex = 1
        Me.RbUseSFZP.TabStop = True
        Me.RbUseSFZP.Text = "Use the .sfzp file. Sorting disabled."
        Me.RbUseSFZP.UseVisualStyleBackColor = True
        '
        'RbUseFZP
        '
        Me.RbUseFZP.AutoSize = True
        Me.RbUseFZP.Location = New System.Drawing.Point(20, 71)
        Me.RbUseFZP.Name = "RbUseFZP"
        Me.RbUseFZP.Size = New System.Drawing.Size(317, 17)
        Me.RbUseFZP.TabIndex = 2
        Me.RbUseFZP.Text = "Use the original .fzp file. The existing .sfzp file will be replaced."
        Me.RbUseFZP.UseVisualStyleBackColor = True
        '
        'RBAbort
        '
        Me.RBAbort.AutoSize = True
        Me.RBAbort.Location = New System.Drawing.Point(20, 94)
        Me.RBAbort.Name = "RBAbort"
        Me.RBAbort.Size = New System.Drawing.Size(53, 17)
        Me.RBAbort.TabIndex = 3
        Me.RBAbort.Text = "Abort."
        Me.RBAbort.UseVisualStyleBackColor = True
        '
        'LbTimer
        '
        Me.LbTimer.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LbTimer.AutoSize = True
        Me.LbTimer.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LbTimer.Location = New System.Drawing.Point(12, 142)
        Me.LbTimer.Name = "LbTimer"
        Me.LbTimer.Size = New System.Drawing.Size(74, 13)
        Me.LbTimer.TabIndex = 4
        Me.LbTimer.Text = "Closing Dialog"
        '
        'LbMsg
        '
        Me.LbMsg.AutoSize = True
        Me.LbMsg.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LbMsg.Location = New System.Drawing.Point(12, 19)
        Me.LbMsg.Name = "LbMsg"
        Me.LbMsg.Size = New System.Drawing.Size(57, 13)
        Me.LbMsg.TabIndex = 5
        Me.LbMsg.Text = "Message"
        '
        'Timer1
        '
        Me.Timer1.Interval = 1000
        '
        'F_ADVfzp
        '
        Me.AcceptButton = Me.OK_Button
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Cancel_Button
        Me.ClientSize = New System.Drawing.Size(352, 175)
        Me.Controls.Add(Me.LbMsg)
        Me.Controls.Add(Me.LbTimer)
        Me.Controls.Add(Me.RBAbort)
        Me.Controls.Add(Me.RbUseFZP)
        Me.Controls.Add(Me.RbUseSFZP)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "F_ADVfzp"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "PHEM ADVANCE"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents RbUseSFZP As System.Windows.Forms.RadioButton
    Friend WithEvents RbUseFZP As System.Windows.Forms.RadioButton
    Friend WithEvents RBAbort As System.Windows.Forms.RadioButton
    Friend WithEvents LbTimer As System.Windows.Forms.Label
    Friend WithEvents LbMsg As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer

End Class
