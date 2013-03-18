<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_ENG
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(F_ENG))
        Me.TbNleerl = New System.Windows.Forms.TextBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.TbNnenn = New System.Windows.Forms.TextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.TbInertia = New System.Windows.Forms.TextBox()
        Me.Label41 = New System.Windows.Forms.Label()
        Me.Label40 = New System.Windows.Forms.Label()
        Me.Label39 = New System.Windows.Forms.Label()
        Me.Label36 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.TbPnenn = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.ButCancel = New System.Windows.Forms.Button()
        Me.ButOK = New System.Windows.Forms.Button()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.ToolStripBtNew = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripBtOpen = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripBtSave = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripBtSaveAs = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripBtSendTo = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.LbStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TbDispl = New System.Windows.Forms.TextBox()
        Me.TbName = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.TbFLD = New System.Windows.Forms.TextBox()
        Me.BtFLD = New System.Windows.Forms.Button()
        Me.TbMAP = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.BtMAP = New System.Windows.Forms.Button()
        Me.TbWHTC = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.BtWHTC = New System.Windows.Forms.Button()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.CmOpenFile = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.OpenWithGRAPHiToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.OpenWithToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ShowInFolderToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.BtFLDOpen = New System.Windows.Forms.Button()
        Me.BtMAPopen = New System.Windows.Forms.Button()
        Me.ToolStrip1.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.CmOpenFile.SuspendLayout()
        Me.SuspendLayout()
        '
        'TbNleerl
        '
        Me.TbNleerl.Location = New System.Drawing.Point(400, 118)
        Me.TbNleerl.Name = "TbNleerl"
        Me.TbNleerl.Size = New System.Drawing.Size(57, 20)
        Me.TbNleerl.TabIndex = 4
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(292, 121)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(102, 13)
        Me.Label11.TabIndex = 15
        Me.Label11.Text = "Idling Engine Speed"
        '
        'TbNnenn
        '
        Me.TbNnenn.Location = New System.Drawing.Point(400, 144)
        Me.TbNnenn.Name = "TbNnenn"
        Me.TbNnenn.Size = New System.Drawing.Size(57, 20)
        Me.TbNnenn.TabIndex = 5
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(288, 147)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(106, 13)
        Me.Label10.TabIndex = 13
        Me.Label10.Text = "Rated Engine Speed"
        '
        'TbInertia
        '
        Me.TbInertia.Location = New System.Drawing.Point(123, 170)
        Me.TbInertia.Name = "TbInertia"
        Me.TbInertia.Size = New System.Drawing.Size(57, 20)
        Me.TbInertia.TabIndex = 3
        '
        'Label41
        '
        Me.Label41.AutoSize = True
        Me.Label41.Location = New System.Drawing.Point(186, 173)
        Me.Label41.Name = "Label41"
        Me.Label41.Size = New System.Drawing.Size(36, 13)
        Me.Label41.TabIndex = 24
        Me.Label41.Text = "[kgm²]"
        '
        'Label40
        '
        Me.Label40.AutoSize = True
        Me.Label40.Location = New System.Drawing.Point(463, 121)
        Me.Label40.Name = "Label40"
        Me.Label40.Size = New System.Drawing.Size(30, 13)
        Me.Label40.TabIndex = 24
        Me.Label40.Text = "[rpm]"
        '
        'Label39
        '
        Me.Label39.AutoSize = True
        Me.Label39.Location = New System.Drawing.Point(463, 147)
        Me.Label39.Name = "Label39"
        Me.Label39.Size = New System.Drawing.Size(30, 13)
        Me.Label39.TabIndex = 24
        Me.Label39.Text = "[rpm]"
        '
        'Label36
        '
        Me.Label36.AutoSize = True
        Me.Label36.Location = New System.Drawing.Point(186, 121)
        Me.Label36.Name = "Label36"
        Me.Label36.Size = New System.Drawing.Size(30, 13)
        Me.Label36.TabIndex = 24
        Me.Label36.Text = "[kW]"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(15, 173)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(102, 13)
        Me.Label5.TabIndex = 0
        Me.Label5.Text = "Inertia incl. Flywheel"
        '
        'TbPnenn
        '
        Me.TbPnenn.Location = New System.Drawing.Point(123, 118)
        Me.TbPnenn.Name = "TbPnenn"
        Me.TbPnenn.Size = New System.Drawing.Size(57, 20)
        Me.TbPnenn.TabIndex = 1
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(12, 121)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(105, 13)
        Me.Label9.TabIndex = 11
        Me.Label9.Text = "Rated Engine Power"
        '
        'ButCancel
        '
        Me.ButCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ButCancel.Location = New System.Drawing.Point(418, 360)
        Me.ButCancel.Name = "ButCancel"
        Me.ButCancel.Size = New System.Drawing.Size(75, 23)
        Me.ButCancel.TabIndex = 13
        Me.ButCancel.Text = "Cancel"
        Me.ButCancel.UseVisualStyleBackColor = True
        '
        'ButOK
        '
        Me.ButOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButOK.Location = New System.Drawing.Point(337, 360)
        Me.ButOK.Name = "ButOK"
        Me.ButOK.Size = New System.Drawing.Size(75, 23)
        Me.ButOK.TabIndex = 12
        Me.ButOK.Text = "OK"
        Me.ButOK.UseVisualStyleBackColor = True
        '
        'ToolStrip1
        '
        Me.ToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripBtNew, Me.ToolStripBtOpen, Me.ToolStripBtSave, Me.ToolStripBtSaveAs, Me.ToolStripSeparator3, Me.ToolStripBtSendTo, Me.ToolStripSeparator1, Me.ToolStripButton1})
        Me.ToolStrip1.Location = New System.Drawing.Point(0, 0)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(505, 25)
        Me.ToolStrip1.TabIndex = 30
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'ToolStripBtNew
        '
        Me.ToolStripBtNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripBtNew.Image = Global.VECTO.My.Resources.Resources.blue_document_icon
        Me.ToolStripBtNew.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripBtNew.Name = "ToolStripBtNew"
        Me.ToolStripBtNew.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripBtNew.Text = "ToolStripButton1"
        Me.ToolStripBtNew.ToolTipText = "New"
        '
        'ToolStripBtOpen
        '
        Me.ToolStripBtOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripBtOpen.Image = Global.VECTO.My.Resources.Resources.Open_icon
        Me.ToolStripBtOpen.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripBtOpen.Name = "ToolStripBtOpen"
        Me.ToolStripBtOpen.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripBtOpen.Text = "ToolStripButton1"
        Me.ToolStripBtOpen.ToolTipText = "Open..."
        '
        'ToolStripBtSave
        '
        Me.ToolStripBtSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripBtSave.Image = Global.VECTO.My.Resources.Resources.Actions_document_save_icon
        Me.ToolStripBtSave.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripBtSave.Name = "ToolStripBtSave"
        Me.ToolStripBtSave.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripBtSave.Text = "ToolStripButton1"
        Me.ToolStripBtSave.ToolTipText = "Save"
        '
        'ToolStripBtSaveAs
        '
        Me.ToolStripBtSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripBtSaveAs.Image = Global.VECTO.My.Resources.Resources.Actions_document_save_as_icon
        Me.ToolStripBtSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripBtSaveAs.Name = "ToolStripBtSaveAs"
        Me.ToolStripBtSaveAs.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripBtSaveAs.Text = "ToolStripButton1"
        Me.ToolStripBtSaveAs.ToolTipText = "Save As..."
        '
        'ToolStripSeparator3
        '
        Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
        Me.ToolStripSeparator3.Size = New System.Drawing.Size(6, 25)
        '
        'ToolStripBtSendTo
        '
        Me.ToolStripBtSendTo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripBtSendTo.Image = Global.VECTO.My.Resources.Resources.export_icon
        Me.ToolStripBtSendTo.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripBtSendTo.Name = "ToolStripBtSendTo"
        Me.ToolStripBtSendTo.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripBtSendTo.Text = "Send to GEN Editor"
        Me.ToolStripBtSendTo.ToolTipText = "Send to GEN Editor"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 25)
        '
        'ToolStripButton1
        '
        Me.ToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton1.Image = Global.VECTO.My.Resources.Resources.Help_icon
        Me.ToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton1.Name = "ToolStripButton1"
        Me.ToolStripButton1.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton1.Text = "Help"
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.LbStatus})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 386)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(505, 22)
        Me.StatusStrip1.SizingGrip = False
        Me.StatusStrip1.TabIndex = 37
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'LbStatus
        '
        Me.LbStatus.Name = "LbStatus"
        Me.LbStatus.Size = New System.Drawing.Size(39, 17)
        Me.LbStatus.Text = "Status"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(186, 147)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(33, 13)
        Me.Label1.TabIndex = 24
        Me.Label1.Text = "[ccm]"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(46, 147)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(71, 13)
        Me.Label2.TabIndex = 13
        Me.Label2.Text = "Displacement"
        '
        'TbDispl
        '
        Me.TbDispl.Location = New System.Drawing.Point(123, 144)
        Me.TbDispl.Name = "TbDispl"
        Me.TbDispl.Size = New System.Drawing.Size(57, 20)
        Me.TbDispl.TabIndex = 2
        '
        'TbName
        '
        Me.TbName.Location = New System.Drawing.Point(123, 82)
        Me.TbName.Name = "TbName"
        Me.TbName.Size = New System.Drawing.Size(370, 20)
        Me.TbName.TabIndex = 0
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(30, 85)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(87, 13)
        Me.Label3.TabIndex = 11
        Me.Label3.Text = "Make and Model"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 205)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(128, 13)
        Me.Label4.TabIndex = 38
        Me.Label4.Text = "Full Load and Drag Curve"
        '
        'TbFLD
        '
        Me.TbFLD.Location = New System.Drawing.Point(12, 221)
        Me.TbFLD.Name = "TbFLD"
        Me.TbFLD.Size = New System.Drawing.Size(418, 20)
        Me.TbFLD.TabIndex = 6
        '
        'BtFLD
        '
        Me.BtFLD.Location = New System.Drawing.Point(436, 219)
        Me.BtFLD.Name = "BtFLD"
        Me.BtFLD.Size = New System.Drawing.Size(28, 23)
        Me.BtFLD.TabIndex = 7
        Me.BtFLD.TabStop = False
        Me.BtFLD.Text = "..."
        Me.BtFLD.UseVisualStyleBackColor = True
        '
        'TbMAP
        '
        Me.TbMAP.Location = New System.Drawing.Point(12, 271)
        Me.TbMAP.Name = "TbMAP"
        Me.TbMAP.Size = New System.Drawing.Size(418, 20)
        Me.TbMAP.TabIndex = 8
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(12, 255)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(115, 13)
        Me.Label6.TabIndex = 38
        Me.Label6.Text = "Fuel Consumption Map"
        '
        'BtMAP
        '
        Me.BtMAP.Location = New System.Drawing.Point(436, 269)
        Me.BtMAP.Name = "BtMAP"
        Me.BtMAP.Size = New System.Drawing.Size(28, 23)
        Me.BtMAP.TabIndex = 9
        Me.BtMAP.TabStop = False
        Me.BtMAP.Text = "..."
        Me.BtMAP.UseVisualStyleBackColor = True
        '
        'TbWHTC
        '
        Me.TbWHTC.Location = New System.Drawing.Point(12, 321)
        Me.TbWHTC.Name = "TbWHTC"
        Me.TbWHTC.Size = New System.Drawing.Size(447, 20)
        Me.TbWHTC.TabIndex = 10
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(12, 305)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(109, 13)
        Me.Label7.TabIndex = 38
        Me.Label7.Text = "Calibration Test Cycle"
        '
        'BtWHTC
        '
        Me.BtWHTC.Location = New System.Drawing.Point(465, 319)
        Me.BtWHTC.Name = "BtWHTC"
        Me.BtWHTC.Size = New System.Drawing.Size(28, 23)
        Me.BtWHTC.TabIndex = 11
        Me.BtWHTC.TabStop = False
        Me.BtWHTC.Text = "..."
        Me.BtWHTC.UseVisualStyleBackColor = True
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.White
        Me.PictureBox1.Image = Global.VECTO.My.Resources.Resources.VECTO_ENG
        Me.PictureBox1.Location = New System.Drawing.Point(12, 28)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(481, 40)
        Me.PictureBox1.TabIndex = 39
        Me.PictureBox1.TabStop = False
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
        'BtFLDOpen
        '
        Me.BtFLDOpen.Image = Global.VECTO.My.Resources.Resources.application_export_icon_small
        Me.BtFLDOpen.Location = New System.Drawing.Point(470, 219)
        Me.BtFLDOpen.Name = "BtFLDOpen"
        Me.BtFLDOpen.Size = New System.Drawing.Size(23, 23)
        Me.BtFLDOpen.TabIndex = 40
        Me.BtFLDOpen.TabStop = False
        Me.BtFLDOpen.UseVisualStyleBackColor = True
        '
        'BtMAPopen
        '
        Me.BtMAPopen.Image = Global.VECTO.My.Resources.Resources.application_export_icon_small
        Me.BtMAPopen.Location = New System.Drawing.Point(470, 269)
        Me.BtMAPopen.Name = "BtMAPopen"
        Me.BtMAPopen.Size = New System.Drawing.Size(23, 23)
        Me.BtMAPopen.TabIndex = 40
        Me.BtMAPopen.TabStop = False
        Me.BtMAPopen.UseVisualStyleBackColor = True
        '
        'F_ENG
        '
        Me.AcceptButton = Me.ButOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ButCancel
        Me.ClientSize = New System.Drawing.Size(505, 408)
        Me.Controls.Add(Me.BtMAPopen)
        Me.Controls.Add(Me.BtFLDOpen)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.BtWHTC)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.BtMAP)
        Me.Controls.Add(Me.BtFLD)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.TbNleerl)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.Label11)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Controls.Add(Me.TbDispl)
        Me.Controls.Add(Me.TbNnenn)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.ButCancel)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.TbWHTC)
        Me.Controls.Add(Me.TbMAP)
        Me.Controls.Add(Me.ButOK)
        Me.Controls.Add(Me.TbFLD)
        Me.Controls.Add(Me.TbInertia)
        Me.Controls.Add(Me.Label41)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.Label40)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TbName)
        Me.Controls.Add(Me.TbPnenn)
        Me.Controls.Add(Me.Label39)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label36)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "F_ENG"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "F_ENG"
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.CmOpenFile.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TbNleerl As System.Windows.Forms.TextBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents TbNnenn As System.Windows.Forms.TextBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents TbInertia As System.Windows.Forms.TextBox
    Friend WithEvents Label41 As System.Windows.Forms.Label
    Friend WithEvents Label40 As System.Windows.Forms.Label
    Friend WithEvents Label39 As System.Windows.Forms.Label
    Friend WithEvents Label36 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents TbPnenn As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents ButCancel As System.Windows.Forms.Button
    Friend WithEvents ButOK As System.Windows.Forms.Button
    Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripBtNew As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripBtOpen As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripBtSave As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripBtSaveAs As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator3 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripBtSendTo As System.Windows.Forms.ToolStripButton
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents LbStatus As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TbDispl As System.Windows.Forms.TextBox
    Friend WithEvents TbName As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents TbFLD As System.Windows.Forms.TextBox
    Friend WithEvents BtFLD As System.Windows.Forms.Button
    Friend WithEvents TbMAP As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents BtMAP As System.Windows.Forms.Button
    Friend WithEvents TbWHTC As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents BtWHTC As System.Windows.Forms.Button
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Friend WithEvents CmOpenFile As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents OpenWithGRAPHiToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenWithToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ShowInFolderToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents BtFLDOpen As System.Windows.Forms.Button
    Friend WithEvents BtMAPopen As System.Windows.Forms.Button
End Class
