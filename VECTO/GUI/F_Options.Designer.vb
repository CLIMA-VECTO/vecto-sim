<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_Options
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
        Me.TextBoxWorDir = New System.Windows.Forms.TextBox()
        Me.ButtonWorDir = New System.Windows.Forms.Button()
        Me.GroupBoxWorDir = New System.Windows.Forms.GroupBox()
        Me.ButtonOK = New System.Windows.Forms.Button()
        Me.ButtonCancel = New System.Windows.Forms.Button()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.TbOpenCmdName = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.TbOpenCmd = New System.Windows.Forms.TextBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.TextBoxLogSize = New System.Windows.Forms.TextBox()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.TbCO2toFC = New System.Windows.Forms.TextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.TbFuelDens = New System.Windows.Forms.TextBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.TbAirDensity = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TabPgTest = New System.Windows.Forms.TabPage()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.TbnnormEngStop = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.ButtonTD = New System.Windows.Forms.Button()
        Me.ButTDdefault = New System.Windows.Forms.Button()
        Me.TextBoxTD = New System.Windows.Forms.TextBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.LabelEAVal = New System.Windows.Forms.Label()
        Me.TextBoxEAVal = New System.Windows.Forms.TextBox()
        Me.ButReset = New System.Windows.Forms.Button()
        Me.BtHelp = New System.Windows.Forms.Button()
        Me.GroupBoxWorDir.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.TabPgTest.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'TextBoxWorDir
        '
        Me.TextBoxWorDir.Location = New System.Drawing.Point(6, 19)
        Me.TextBoxWorDir.Name = "TextBoxWorDir"
        Me.TextBoxWorDir.Size = New System.Drawing.Size(444, 20)
        Me.TextBoxWorDir.TabIndex = 1
        '
        'ButtonWorDir
        '
        Me.ButtonWorDir.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ButtonWorDir.Location = New System.Drawing.Point(456, 19)
        Me.ButtonWorDir.Name = "ButtonWorDir"
        Me.ButtonWorDir.Size = New System.Drawing.Size(26, 20)
        Me.ButtonWorDir.TabIndex = 2
        Me.ButtonWorDir.Text = "..."
        Me.ButtonWorDir.UseVisualStyleBackColor = True
        '
        'GroupBoxWorDir
        '
        Me.GroupBoxWorDir.Controls.Add(Me.ButtonWorDir)
        Me.GroupBoxWorDir.Controls.Add(Me.TextBoxWorDir)
        Me.GroupBoxWorDir.Location = New System.Drawing.Point(5, 6)
        Me.GroupBoxWorDir.Name = "GroupBoxWorDir"
        Me.GroupBoxWorDir.Size = New System.Drawing.Size(490, 51)
        Me.GroupBoxWorDir.TabIndex = 2
        Me.GroupBoxWorDir.TabStop = False
        Me.GroupBoxWorDir.Text = "Working Directory"
        '
        'ButtonOK
        '
        Me.ButtonOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonOK.Location = New System.Drawing.Point(347, 337)
        Me.ButtonOK.Name = "ButtonOK"
        Me.ButtonOK.Size = New System.Drawing.Size(75, 26)
        Me.ButtonOK.TabIndex = 0
        Me.ButtonOK.Text = "OK"
        Me.ButtonOK.UseVisualStyleBackColor = True
        '
        'ButtonCancel
        '
        Me.ButtonCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ButtonCancel.Location = New System.Drawing.Point(428, 337)
        Me.ButtonCancel.Name = "ButtonCancel"
        Me.ButtonCancel.Size = New System.Drawing.Size(75, 26)
        Me.ButtonCancel.TabIndex = 1
        Me.ButtonCancel.Text = "Cancel"
        Me.ButtonCancel.UseVisualStyleBackColor = True
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.GroupBox5)
        Me.GroupBox3.Controls.Add(Me.TextBoxLogSize)
        Me.GroupBox3.Controls.Add(Me.Label16)
        Me.GroupBox3.Location = New System.Drawing.Point(5, 63)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(489, 124)
        Me.GroupBox3.TabIndex = 11
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Interface"
        '
        'GroupBox5
        '
        Me.GroupBox5.Controls.Add(Me.TbOpenCmdName)
        Me.GroupBox5.Controls.Add(Me.Label7)
        Me.GroupBox5.Controls.Add(Me.TbOpenCmd)
        Me.GroupBox5.Controls.Add(Me.Label12)
        Me.GroupBox5.Location = New System.Drawing.Point(230, 19)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Size = New System.Drawing.Size(253, 96)
        Me.GroupBox5.TabIndex = 14
        Me.GroupBox5.TabStop = False
        Me.GroupBox5.Text = "File Open Command"
        '
        'TbOpenCmdName
        '
        Me.TbOpenCmdName.Location = New System.Drawing.Point(66, 19)
        Me.TbOpenCmdName.Name = "TbOpenCmdName"
        Me.TbOpenCmdName.Size = New System.Drawing.Size(174, 20)
        Me.TbOpenCmdName.TabIndex = 13
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(6, 48)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(54, 13)
        Me.Label7.TabIndex = 12
        Me.Label7.Text = "Command"
        '
        'TbOpenCmd
        '
        Me.TbOpenCmd.Location = New System.Drawing.Point(66, 45)
        Me.TbOpenCmd.Name = "TbOpenCmd"
        Me.TbOpenCmd.Size = New System.Drawing.Size(174, 20)
        Me.TbOpenCmd.TabIndex = 13
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(25, 22)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(35, 13)
        Me.Label12.TabIndex = 12
        Me.Label12.Text = "Name"
        '
        'TextBoxLogSize
        '
        Me.TextBoxLogSize.Location = New System.Drawing.Point(134, 38)
        Me.TextBoxLogSize.Name = "TextBoxLogSize"
        Me.TextBoxLogSize.Size = New System.Drawing.Size(36, 20)
        Me.TextBoxLogSize.TabIndex = 11
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Location = New System.Drawing.Point(18, 41)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(110, 13)
        Me.Label16.TabIndex = 10
        Me.Label16.Text = "Logfile Size Limit [MB]"
        '
        'TabControl1
        '
        Me.TabControl1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Controls.Add(Me.TabPgTest)
        Me.TabControl1.Location = New System.Drawing.Point(3, 3)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(508, 328)
        Me.TabControl1.TabIndex = 12
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.GroupBox4)
        Me.TabPage2.Controls.Add(Me.GroupBoxWorDir)
        Me.TabPage2.Controls.Add(Me.GroupBox3)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(500, 302)
        Me.TabPage2.TabIndex = 0
        Me.TabPage2.Text = "VECTO"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'GroupBox4
        '
        Me.GroupBox4.Controls.Add(Me.Label11)
        Me.GroupBox4.Controls.Add(Me.Label9)
        Me.GroupBox4.Controls.Add(Me.Label3)
        Me.GroupBox4.Controls.Add(Me.TbCO2toFC)
        Me.GroupBox4.Controls.Add(Me.Label10)
        Me.GroupBox4.Controls.Add(Me.TbFuelDens)
        Me.GroupBox4.Controls.Add(Me.Label8)
        Me.GroupBox4.Controls.Add(Me.TbAirDensity)
        Me.GroupBox4.Controls.Add(Me.Label2)
        Me.GroupBox4.Location = New System.Drawing.Point(6, 193)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(488, 103)
        Me.GroupBox4.TabIndex = 12
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "Calculation"
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(164, 66)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(77, 13)
        Me.Label11.TabIndex = 16
        Me.Label11.Text = "[kgCO2/KgFC]"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(309, 22)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(32, 13)
        Me.Label9.TabIndex = 16
        Me.Label9.Text = "[kg/l]"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(130, 22)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(41, 13)
        Me.Label3.TabIndex = 16
        Me.Label3.Text = "[kg/m²]"
        '
        'TbCO2toFC
        '
        Me.TbCO2toFC.Location = New System.Drawing.Point(108, 63)
        Me.TbCO2toFC.Name = "TbCO2toFC"
        Me.TbCO2toFC.Size = New System.Drawing.Size(50, 20)
        Me.TbCO2toFC.TabIndex = 15
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(11, 66)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(91, 13)
        Me.Label10.TabIndex = 14
        Me.Label10.Text = "CO2-to-Fuel Ratio"
        '
        'TbFuelDens
        '
        Me.TbFuelDens.Location = New System.Drawing.Point(253, 19)
        Me.TbFuelDens.Name = "TbFuelDens"
        Me.TbFuelDens.Size = New System.Drawing.Size(50, 20)
        Me.TbFuelDens.TabIndex = 15
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(184, 22)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(63, 13)
        Me.Label8.TabIndex = 14
        Me.Label8.Text = "Fuel density"
        '
        'TbAirDensity
        '
        Me.TbAirDensity.Location = New System.Drawing.Point(74, 19)
        Me.TbAirDensity.Name = "TbAirDensity"
        Me.TbAirDensity.Size = New System.Drawing.Size(50, 20)
        Me.TbAirDensity.TabIndex = 15
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(11, 22)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(57, 13)
        Me.Label2.TabIndex = 14
        Me.Label2.Text = "Air Density"
        '
        'TabPgTest
        '
        Me.TabPgTest.Controls.Add(Me.Label6)
        Me.TabPgTest.Controls.Add(Me.TbnnormEngStop)
        Me.TabPgTest.Controls.Add(Me.Label5)
        Me.TabPgTest.Controls.Add(Me.Label4)
        Me.TabPgTest.Controls.Add(Me.GroupBox1)
        Me.TabPgTest.Controls.Add(Me.GroupBox2)
        Me.TabPgTest.Location = New System.Drawing.Point(4, 22)
        Me.TabPgTest.Name = "TabPgTest"
        Me.TabPgTest.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPgTest.Size = New System.Drawing.Size(500, 302)
        Me.TabPgTest.TabIndex = 2
        Me.TabPgTest.Text = "TEST"
        Me.TabPgTest.UseVisualStyleBackColor = True
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(11, 160)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(169, 13)
        Me.Label6.TabIndex = 22
        Me.Label6.Text = "(ICE Start/Stop must be activated)" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'TbnnormEngStop
        '
        Me.TbnnormEngStop.Location = New System.Drawing.Point(217, 138)
        Me.TbnnormEngStop.Name = "TbnnormEngStop"
        Me.TbnnormEngStop.Size = New System.Drawing.Size(54, 20)
        Me.TbnnormEngStop.TabIndex = 21
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(277, 141)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(16, 13)
        Me.Label5.TabIndex = 17
        Me.Label5.Text = "[-]"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(11, 141)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(200, 13)
        Me.Label4.TabIndex = 20
        Me.Label4.Text = "Engine stop when input n_norm is below "
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.ButtonTD)
        Me.GroupBox1.Controls.Add(Me.ButTDdefault)
        Me.GroupBox1.Controls.Add(Me.TextBoxTD)
        Me.GroupBox1.Location = New System.Drawing.Point(5, 16)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(490, 53)
        Me.GroupBox1.TabIndex = 18
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Temperature-Table for TEM creation"
        '
        'ButtonTD
        '
        Me.ButtonTD.Location = New System.Drawing.Point(456, 19)
        Me.ButtonTD.Name = "ButtonTD"
        Me.ButtonTD.Size = New System.Drawing.Size(26, 20)
        Me.ButtonTD.TabIndex = 2
        Me.ButtonTD.Text = "..."
        Me.ButtonTD.UseVisualStyleBackColor = True
        '
        'ButTDdefault
        '
        Me.ButTDdefault.Location = New System.Drawing.Point(9, 19)
        Me.ButTDdefault.Name = "ButTDdefault"
        Me.ButTDdefault.Size = New System.Drawing.Size(99, 20)
        Me.ButTDdefault.TabIndex = 8
        Me.ButTDdefault.Text = "Set To Default"
        Me.ButTDdefault.UseVisualStyleBackColor = True
        '
        'TextBoxTD
        '
        Me.TextBoxTD.Location = New System.Drawing.Point(119, 19)
        Me.TextBoxTD.Name = "TextBoxTD"
        Me.TextBoxTD.Size = New System.Drawing.Size(331, 20)
        Me.TextBoxTD.TabIndex = 6
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.Label1)
        Me.GroupBox2.Controls.Add(Me.LabelEAVal)
        Me.GroupBox2.Controls.Add(Me.TextBoxEAVal)
        Me.GroupBox2.Location = New System.Drawing.Point(6, 75)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(184, 50)
        Me.GroupBox2.TabIndex = 19
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Engine Analysis"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(6, 23)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(93, 13)
        Me.Label1.TabIndex = 14
        Me.Label1.Text = "Averaging Range:"
        '
        'LabelEAVal
        '
        Me.LabelEAVal.AutoSize = True
        Me.LabelEAVal.Location = New System.Drawing.Point(155, 22)
        Me.LabelEAVal.Name = "LabelEAVal"
        Me.LabelEAVal.Size = New System.Drawing.Size(18, 13)
        Me.LabelEAVal.TabIndex = 3
        Me.LabelEAVal.Text = "[s]"
        '
        'TextBoxEAVal
        '
        Me.TextBoxEAVal.Location = New System.Drawing.Point(105, 19)
        Me.TextBoxEAVal.Name = "TextBoxEAVal"
        Me.TextBoxEAVal.Size = New System.Drawing.Size(44, 20)
        Me.TextBoxEAVal.TabIndex = 2
        '
        'ButReset
        '
        Me.ButReset.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ButReset.Location = New System.Drawing.Point(35, 337)
        Me.ButReset.Name = "ButReset"
        Me.ButReset.Size = New System.Drawing.Size(108, 26)
        Me.ButReset.TabIndex = 13
        Me.ButReset.Text = "Reset All Settings"
        Me.ButReset.UseVisualStyleBackColor = True
        '
        'BtHelp
        '
        Me.BtHelp.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.BtHelp.Image = Global.VECTO.My.Resources.Resources.Help_icon
        Me.BtHelp.Location = New System.Drawing.Point(3, 337)
        Me.BtHelp.Name = "BtHelp"
        Me.BtHelp.Size = New System.Drawing.Size(26, 26)
        Me.BtHelp.TabIndex = 14
        Me.BtHelp.UseVisualStyleBackColor = True
        '
        'F_Options
        '
        Me.AcceptButton = Me.ButtonOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ButtonCancel
        Me.ClientSize = New System.Drawing.Size(515, 375)
        Me.Controls.Add(Me.BtHelp)
        Me.Controls.Add(Me.ButReset)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.ButtonCancel)
        Me.Controls.Add(Me.ButtonOK)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "F_Options"
        Me.Text = "Settings"
        Me.GroupBoxWorDir.ResumeLayout(False)
        Me.GroupBoxWorDir.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage2.ResumeLayout(False)
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.TabPgTest.ResumeLayout(False)
        Me.TabPgTest.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents TextBoxWorDir As System.Windows.Forms.TextBox
    Friend WithEvents ButtonWorDir As System.Windows.Forms.Button
    Friend WithEvents GroupBoxWorDir As System.Windows.Forms.GroupBox
    Friend WithEvents ButtonOK As System.Windows.Forms.Button
    Friend WithEvents ButtonCancel As System.Windows.Forms.Button
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents TextBoxLogSize As System.Windows.Forms.TextBox
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents ButReset As System.Windows.Forms.Button
    Friend WithEvents TbOpenCmd As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents TbAirDensity As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TabPgTest As System.Windows.Forms.TabPage
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents TbnnormEngStop As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents ButtonTD As System.Windows.Forms.Button
    Friend WithEvents ButTDdefault As System.Windows.Forms.Button
    Friend WithEvents TextBoxTD As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents LabelEAVal As System.Windows.Forms.Label
    Friend WithEvents TextBoxEAVal As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents TbFuelDens As System.Windows.Forms.TextBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents TbCO2toFC As System.Windows.Forms.TextBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents BtHelp As System.Windows.Forms.Button
    Friend WithEvents GroupBox5 As System.Windows.Forms.GroupBox
    Friend WithEvents TbOpenCmdName As System.Windows.Forms.TextBox
    Friend WithEvents Label12 As System.Windows.Forms.Label
End Class
