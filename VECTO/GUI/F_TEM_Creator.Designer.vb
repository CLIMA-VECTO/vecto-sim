<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_TEM_Creator
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
        Me.TextBoxTemp = New System.Windows.Forms.TextBox
        Me.ComboBoxDrCyc = New System.Windows.Forms.ComboBox
        Me.TextBoxPath = New System.Windows.Forms.TextBox
        Me.ButtonBrowse = New System.Windows.Forms.Button
        Me.ButtonOK = New System.Windows.Forms.Button
        Me.ButtonCancel = New System.Windows.Forms.Button
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label4 = New System.Windows.Forms.Label
        Me.CheckBoxEXL = New System.Windows.Forms.CheckBox
        Me.SuspendLayout()
        '
        'TextBoxTemp
        '
        Me.TextBoxTemp.Location = New System.Drawing.Point(94, 15)
        Me.TextBoxTemp.Name = "TextBoxTemp"
        Me.TextBoxTemp.Size = New System.Drawing.Size(39, 20)
        Me.TextBoxTemp.TabIndex = 0
        '
        'ComboBoxDrCyc
        '
        Me.ComboBoxDrCyc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxDrCyc.FormattingEnabled = True
        Me.ComboBoxDrCyc.Location = New System.Drawing.Point(94, 48)
        Me.ComboBoxDrCyc.Name = "ComboBoxDrCyc"
        Me.ComboBoxDrCyc.Size = New System.Drawing.Size(133, 21)
        Me.ComboBoxDrCyc.TabIndex = 1
        '
        'TextBoxPath
        '
        Me.TextBoxPath.Location = New System.Drawing.Point(41, 82)
        Me.TextBoxPath.Name = "TextBoxPath"
        Me.TextBoxPath.Size = New System.Drawing.Size(208, 20)
        Me.TextBoxPath.TabIndex = 2
        '
        'ButtonBrowse
        '
        Me.ButtonBrowse.Location = New System.Drawing.Point(255, 82)
        Me.ButtonBrowse.Name = "ButtonBrowse"
        Me.ButtonBrowse.Size = New System.Drawing.Size(25, 20)
        Me.ButtonBrowse.TabIndex = 3
        Me.ButtonBrowse.Text = "..."
        Me.ButtonBrowse.UseVisualStyleBackColor = True
        '
        'ButtonOK
        '
        Me.ButtonOK.Location = New System.Drawing.Point(124, 152)
        Me.ButtonOK.Name = "ButtonOK"
        Me.ButtonOK.Size = New System.Drawing.Size(75, 23)
        Me.ButtonOK.TabIndex = 4
        Me.ButtonOK.Text = "Create"
        Me.ButtonOK.UseVisualStyleBackColor = True
        '
        'ButtonCancel
        '
        Me.ButtonCancel.Location = New System.Drawing.Point(205, 152)
        Me.ButtonCancel.Name = "ButtonCancel"
        Me.ButtonCancel.Size = New System.Drawing.Size(75, 23)
        Me.ButtonCancel.TabIndex = 5
        Me.ButtonCancel.Text = "Cancel"
        Me.ButtonCancel.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(21, 18)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(67, 13)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "Temperature"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(139, 18)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(18, 13)
        Me.Label2.TabIndex = 7
        Me.Label2.Text = "Â°C"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(47, 51)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(41, 13)
        Me.Label3.TabIndex = 8
        Me.Label3.Text = "Region"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 85)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(23, 13)
        Me.Label4.TabIndex = 9
        Me.Label4.Text = "File"
        '
        'CheckBoxEXL
        '
        Me.CheckBoxEXL.AutoSize = True
        Me.CheckBoxEXL.Location = New System.Drawing.Point(15, 119)
        Me.CheckBoxEXL.Name = "CheckBoxEXL"
        Me.CheckBoxEXL.Size = New System.Drawing.Size(197, 17)
        Me.CheckBoxEXL.TabIndex = 10
        Me.CheckBoxEXL.Text = "Open with MS Excel when complete"
        Me.CheckBoxEXL.UseVisualStyleBackColor = True
        '
        'F04_TEM_Creator
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(290, 185)
        Me.Controls.Add(Me.CheckBoxEXL)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.ButtonCancel)
        Me.Controls.Add(Me.ButtonOK)
        Me.Controls.Add(Me.ButtonBrowse)
        Me.Controls.Add(Me.TextBoxPath)
        Me.Controls.Add(Me.ComboBoxDrCyc)
        Me.Controls.Add(Me.TextBoxTemp)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = False
        Me.Name = "F04_TEM_Creator"
        Me.Text = "Create new TEM file"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TextBoxTemp As System.Windows.Forms.TextBox
    Friend WithEvents ComboBoxDrCyc As System.Windows.Forms.ComboBox
    Friend WithEvents TextBoxPath As System.Windows.Forms.TextBox
    Friend WithEvents ButtonBrowse As System.Windows.Forms.Button
    Friend WithEvents ButtonOK As System.Windows.Forms.Button
    Friend WithEvents ButtonCancel As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents CheckBoxEXL As System.Windows.Forms.CheckBox
End Class
