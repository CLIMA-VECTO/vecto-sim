<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_ShutDown
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
        Me.Cancel_Button = New System.Windows.Forms.Button
        Me.Label1 = New System.Windows.Forms.Label
        Me.LbTime = New System.Windows.Forms.Label
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'Cancel_Button
        '
        Me.Cancel_Button.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel_Button.Location = New System.Drawing.Point(12, 54)
        Me.Cancel_Button.Name = "Cancel_Button"
        Me.Cancel_Button.Size = New System.Drawing.Size(191, 49)
        Me.Cancel_Button.TabIndex = 1
        Me.Cancel_Button.Text = "ABORT"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 21)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(127, 13)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "System will shut down in: "
        '
        'LbTime
        '
        Me.LbTime.AutoSize = True
        Me.LbTime.Location = New System.Drawing.Point(145, 21)
        Me.LbTime.Name = "LbTime"
        Me.LbTime.Size = New System.Drawing.Size(19, 13)
        Me.LbTime.TabIndex = 3
        Me.LbTime.Text = "00"
        '
        'Timer1
        '
        Me.Timer1.Interval = 1000
        '
        'F_ShutDown
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.Cancel_Button
        Me.ClientSize = New System.Drawing.Size(215, 115)
        Me.ControlBox = False
        Me.Controls.Add(Me.LbTime)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Cancel_Button)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "F_ShutDown"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Automatic Shutdown"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents LbTime As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer

End Class
