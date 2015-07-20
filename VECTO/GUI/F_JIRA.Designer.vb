<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_JIRA
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
		Me.OK_Button = New System.Windows.Forms.Button()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
		Me.GroupBox1 = New System.Windows.Forms.GroupBox()
		Me.LinkLabel3 = New System.Windows.Forms.LinkLabel()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.Button1 = New System.Windows.Forms.Button()
		Me.GroupBox1.SuspendLayout()
		Me.SuspendLayout()
		'
		'OK_Button
		'
		Me.OK_Button.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.OK_Button.Location = New System.Drawing.Point(356, 205)
		Me.OK_Button.Name = "OK_Button"
		Me.OK_Button.Size = New System.Drawing.Size(67, 23)
		Me.OK_Button.TabIndex = 0
		Me.OK_Button.Text = "Close"
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(6, 16)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(272, 13)
		Me.Label1.TabIndex = 1
		Me.Label1.Text = "You need a user account for CITnet create a new issue."
		'
		'LinkLabel1
		'
		Me.LinkLabel1.AutoSize = True
		Me.LinkLabel1.Location = New System.Drawing.Point(6, 65)
		Me.LinkLabel1.Name = "LinkLabel1"
		Me.LinkLabel1.Size = New System.Drawing.Size(141, 13)
		Me.LinkLabel1.TabIndex = 2
		Me.LinkLabel1.TabStop = True
		Me.LinkLabel1.Text = "JIRA Quick Start Guide (pdf)"
		'
		'GroupBox1
		'
		Me.GroupBox1.Controls.Add(Me.LinkLabel3)
		Me.GroupBox1.Controls.Add(Me.Label1)
		Me.GroupBox1.Controls.Add(Me.Label2)
		Me.GroupBox1.Controls.Add(Me.LinkLabel1)
		Me.GroupBox1.Location = New System.Drawing.Point(12, 86)
		Me.GroupBox1.Name = "GroupBox1"
		Me.GroupBox1.Size = New System.Drawing.Size(411, 100)
		Me.GroupBox1.TabIndex = 3
		Me.GroupBox1.TabStop = False
		Me.GroupBox1.Text = "Help"
		'
		'LinkLabel3
		'
		Me.LinkLabel3.AutoSize = True
		Me.LinkLabel3.Location = New System.Drawing.Point(274, 39)
		Me.LinkLabel3.Name = "LinkLabel3"
		Me.LinkLabel3.Size = New System.Drawing.Size(122, 13)
		Me.LinkLabel3.TabIndex = 18
		Me.LinkLabel3.TabStop = True
		Me.LinkLabel3.Text = "vecto@jrc.ec.europa.eu"
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(6, 39)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(275, 13)
		Me.Label2.TabIndex = 3
		Me.Label2.Text = "If you don't have CITnet access please contact support: "
		'
		'Button1
		'
		Me.Button1.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.Button1.Location = New System.Drawing.Point(12, 12)
		Me.Button1.Name = "Button1"
		Me.Button1.Size = New System.Drawing.Size(411, 54)
		Me.Button1.TabIndex = 4
		Me.Button1.Text = "Create JIRA Issue"
		Me.Button1.UseVisualStyleBackColor = True
		'
		'F_JIRA
		'
		Me.AcceptButton = Me.OK_Button
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(435, 240)
		Me.Controls.Add(Me.Button1)
		Me.Controls.Add(Me.GroupBox1)
		Me.Controls.Add(Me.OK_Button)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.Name = "F_JIRA"
		Me.ShowInTaskbar = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "Report Issue via CITnet"
		Me.GroupBox1.ResumeLayout(False)
		Me.GroupBox1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
	Friend WithEvents OK_Button As System.Windows.Forms.Button
	Friend WithEvents Label1 As System.Windows.Forms.Label
	Friend WithEvents LinkLabel1 As System.Windows.Forms.LinkLabel
	Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
	Friend WithEvents Label2 As System.Windows.Forms.Label
	Friend WithEvents LinkLabel3 As System.Windows.Forms.LinkLabel
	Friend WithEvents Button1 As System.Windows.Forms.Button

End Class
