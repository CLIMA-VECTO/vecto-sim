' Copyright 2014 European Union.
' Licensed under the EUPL (the 'Licence');
'
' * You may not use this work except in compliance with the Licence.
' * You may obtain a copy of the Licence at: http://ec.europa.eu/idabc/eupl
' * Unless required by applicable law or agreed to in writing,
'   software distributed under the Licence is distributed on an "AS IS" basis,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'
' See the LICENSE.txt for the specific language governing permissions and limitations.
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
		Me.TbInertia = New System.Windows.Forms.TextBox()
		Me.Label41 = New System.Windows.Forms.Label()
		Me.Label40 = New System.Windows.Forms.Label()
		Me.Label5 = New System.Windows.Forms.Label()
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
		Me.TbMAP = New System.Windows.Forms.TextBox()
		Me.Label6 = New System.Windows.Forms.Label()
		Me.BtMAP = New System.Windows.Forms.Button()
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		Me.CmOpenFile = New System.Windows.Forms.ContextMenuStrip(Me.components)
		Me.OpenWithToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
		Me.ShowInFolderToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
		Me.BtMAPopen = New System.Windows.Forms.Button()
		Me.PnInertia = New System.Windows.Forms.Panel()
		Me.GrWHTC = New System.Windows.Forms.GroupBox()
		Me.Label13 = New System.Windows.Forms.Label()
		Me.TbWHTCmw = New System.Windows.Forms.TextBox()
		Me.TbWHTCrural = New System.Windows.Forms.TextBox()
		Me.TbWHTCurban = New System.Windows.Forms.TextBox()
		Me.Label8 = New System.Windows.Forms.Label()
		Me.Label7 = New System.Windows.Forms.Label()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.PicBox = New System.Windows.Forms.PictureBox()
		Me.TbFLD = New System.Windows.Forms.TextBox()
		Me.Label14 = New System.Windows.Forms.Label()
		Me.BtFLD = New System.Windows.Forms.Button()
		Me.BtFLDopen = New System.Windows.Forms.Button()
		Me.ToolStrip1.SuspendLayout()
		Me.StatusStrip1.SuspendLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.CmOpenFile.SuspendLayout()
		Me.PnInertia.SuspendLayout()
		Me.GrWHTC.SuspendLayout()
		CType(Me.PicBox, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'TbNleerl
		'
		Me.TbNleerl.Location = New System.Drawing.Point(123, 108)
		Me.TbNleerl.Name = "TbNleerl"
		Me.TbNleerl.Size = New System.Drawing.Size(57, 20)
		Me.TbNleerl.TabIndex = 1
		'
		'Label11
		'
		Me.Label11.AutoSize = True
		Me.Label11.Location = New System.Drawing.Point(15, 111)
		Me.Label11.Name = "Label11"
		Me.Label11.Size = New System.Drawing.Size(102, 13)
		Me.Label11.TabIndex = 15
		Me.Label11.Text = "Idling Engine Speed"
		'
		'TbInertia
		'
		Me.TbInertia.Location = New System.Drawing.Point(120, 2)
		Me.TbInertia.Name = "TbInertia"
		Me.TbInertia.Size = New System.Drawing.Size(57, 20)
		Me.TbInertia.TabIndex = 3
		'
		'Label41
		'
		Me.Label41.AutoSize = True
		Me.Label41.Location = New System.Drawing.Point(183, 5)
		Me.Label41.Name = "Label41"
		Me.Label41.Size = New System.Drawing.Size(36, 13)
		Me.Label41.TabIndex = 24
		Me.Label41.Text = "[kgm²]"
		'
		'Label40
		'
		Me.Label40.AutoSize = True
		Me.Label40.Location = New System.Drawing.Point(186, 111)
		Me.Label40.Name = "Label40"
		Me.Label40.Size = New System.Drawing.Size(30, 13)
		Me.Label40.TabIndex = 24
		Me.Label40.Text = "[rpm]"
		'
		'Label5
		'
		Me.Label5.AutoSize = True
		Me.Label5.Location = New System.Drawing.Point(12, 5)
		Me.Label5.Name = "Label5"
		Me.Label5.Size = New System.Drawing.Size(102, 13)
		Me.Label5.TabIndex = 0
		Me.Label5.Text = "Inertia incl. Flywheel"
		'
		'ButCancel
		'
		Me.ButCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.ButCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.ButCancel.Location = New System.Drawing.Point(898, 469)
		Me.ButCancel.Name = "ButCancel"
		Me.ButCancel.Size = New System.Drawing.Size(75, 23)
		Me.ButCancel.TabIndex = 13
		Me.ButCancel.Text = "Cancel"
		Me.ButCancel.UseVisualStyleBackColor = True
		'
		'ButOK
		'
		Me.ButOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.ButOK.Location = New System.Drawing.Point(817, 469)
		Me.ButOK.Name = "ButOK"
		Me.ButOK.Size = New System.Drawing.Size(75, 23)
		Me.ButOK.TabIndex = 12
		Me.ButOK.Text = "Save"
		Me.ButOK.UseVisualStyleBackColor = True
		'
		'ToolStrip1
		'
		Me.ToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
		Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripBtNew, Me.ToolStripBtOpen, Me.ToolStripBtSave, Me.ToolStripBtSaveAs, Me.ToolStripSeparator3, Me.ToolStripBtSendTo, Me.ToolStripSeparator1, Me.ToolStripButton1})
		Me.ToolStrip1.Location = New System.Drawing.Point(0, 0)
		Me.ToolStrip1.Name = "ToolStrip1"
		Me.ToolStrip1.Size = New System.Drawing.Size(985, 25)
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
		Me.ToolStripBtSendTo.Text = "Send to Job Editor"
		Me.ToolStripBtSendTo.ToolTipText = "Send to Job Editor"
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
		Me.StatusStrip1.Location = New System.Drawing.Point(0, 495)
		Me.StatusStrip1.Name = "StatusStrip1"
		Me.StatusStrip1.Size = New System.Drawing.Size(985, 22)
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
		Me.Label1.Location = New System.Drawing.Point(186, 137)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(33, 13)
		Me.Label1.TabIndex = 24
		Me.Label1.Text = "[ccm]"
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(46, 137)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(71, 13)
		Me.Label2.TabIndex = 13
		Me.Label2.Text = "Displacement"
		'
		'TbDispl
		'
		Me.TbDispl.Location = New System.Drawing.Point(123, 134)
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
		'TbMAP
		'
		Me.TbMAP.Location = New System.Drawing.Point(12, 259)
		Me.TbMAP.Name = "TbMAP"
		Me.TbMAP.Size = New System.Drawing.Size(418, 20)
		Me.TbMAP.TabIndex = 5
		'
		'Label6
		'
		Me.Label6.AutoSize = True
		Me.Label6.Location = New System.Drawing.Point(12, 243)
		Me.Label6.Name = "Label6"
		Me.Label6.Size = New System.Drawing.Size(115, 13)
		Me.Label6.TabIndex = 38
		Me.Label6.Text = "Fuel Consumption Map"
		'
		'BtMAP
		'
		Me.BtMAP.Location = New System.Drawing.Point(436, 257)
		Me.BtMAP.Name = "BtMAP"
		Me.BtMAP.Size = New System.Drawing.Size(28, 23)
		Me.BtMAP.TabIndex = 6
		Me.BtMAP.TabStop = False
		Me.BtMAP.Text = "..."
		Me.BtMAP.UseVisualStyleBackColor = True
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
		Me.CmOpenFile.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OpenWithToolStripMenuItem, Me.ShowInFolderToolStripMenuItem})
		Me.CmOpenFile.Name = "CmOpenFile"
		Me.CmOpenFile.Size = New System.Drawing.Size(153, 48)
		'
		'OpenWithToolStripMenuItem
		'
		Me.OpenWithToolStripMenuItem.Name = "OpenWithToolStripMenuItem"
		Me.OpenWithToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
		Me.OpenWithToolStripMenuItem.Text = "Open with ..."
		'
		'ShowInFolderToolStripMenuItem
		'
		Me.ShowInFolderToolStripMenuItem.Name = "ShowInFolderToolStripMenuItem"
		Me.ShowInFolderToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
		Me.ShowInFolderToolStripMenuItem.Text = "Show in Folder"
		'
		'BtMAPopen
		'
		Me.BtMAPopen.Image = Global.VECTO.My.Resources.Resources.application_export_icon_small
		Me.BtMAPopen.Location = New System.Drawing.Point(470, 257)
		Me.BtMAPopen.Name = "BtMAPopen"
		Me.BtMAPopen.Size = New System.Drawing.Size(23, 23)
		Me.BtMAPopen.TabIndex = 7
		Me.BtMAPopen.TabStop = False
		Me.BtMAPopen.UseVisualStyleBackColor = True
		'
		'PnInertia
		'
		Me.PnInertia.Controls.Add(Me.Label5)
		Me.PnInertia.Controls.Add(Me.Label41)
		Me.PnInertia.Controls.Add(Me.TbInertia)
		Me.PnInertia.Location = New System.Drawing.Point(264, 106)
		Me.PnInertia.Name = "PnInertia"
		Me.PnInertia.Size = New System.Drawing.Size(229, 32)
		Me.PnInertia.TabIndex = 3
		'
		'GrWHTC
		'
		Me.GrWHTC.Controls.Add(Me.Label13)
		Me.GrWHTC.Controls.Add(Me.TbWHTCmw)
		Me.GrWHTC.Controls.Add(Me.TbWHTCrural)
		Me.GrWHTC.Controls.Add(Me.TbWHTCurban)
		Me.GrWHTC.Controls.Add(Me.Label8)
		Me.GrWHTC.Controls.Add(Me.Label7)
		Me.GrWHTC.Controls.Add(Me.Label4)
		Me.GrWHTC.Enabled = False
		Me.GrWHTC.Location = New System.Drawing.Point(12, 300)
		Me.GrWHTC.Name = "GrWHTC"
		Me.GrWHTC.Size = New System.Drawing.Size(481, 91)
		Me.GrWHTC.TabIndex = 9
		Me.GrWHTC.TabStop = False
		Me.GrWHTC.Text = "WHTC Correction"
		'
		'Label13
		'
		Me.Label13.AutoSize = True
		Me.Label13.Location = New System.Drawing.Point(6, 27)
		Me.Label13.Name = "Label13"
		Me.Label13.Size = New System.Drawing.Size(242, 13)
		Me.Label13.TabIndex = 3
		Me.Label13.Text = "Correction Factors calculated with VECTO-Engine"
		'
		'TbWHTCmw
		'
		Me.TbWHTCmw.Location = New System.Drawing.Point(348, 56)
		Me.TbWHTCmw.Name = "TbWHTCmw"
		Me.TbWHTCmw.Size = New System.Drawing.Size(57, 20)
		Me.TbWHTCmw.TabIndex = 2
		'
		'TbWHTCrural
		'
		Me.TbWHTCrural.Location = New System.Drawing.Point(192, 56)
		Me.TbWHTCrural.Name = "TbWHTCrural"
		Me.TbWHTCrural.Size = New System.Drawing.Size(57, 20)
		Me.TbWHTCrural.TabIndex = 1
		'
		'TbWHTCurban
		'
		Me.TbWHTCurban.Location = New System.Drawing.Point(48, 56)
		Me.TbWHTCurban.Name = "TbWHTCurban"
		Me.TbWHTCurban.Size = New System.Drawing.Size(57, 20)
		Me.TbWHTCurban.TabIndex = 0
		'
		'Label8
		'
		Me.Label8.AutoSize = True
		Me.Label8.Location = New System.Drawing.Point(289, 59)
		Me.Label8.Name = "Label8"
		Me.Label8.Size = New System.Drawing.Size(53, 13)
		Me.Label8.TabIndex = 0
		Me.Label8.Text = "Motorway"
		'
		'Label7
		'
		Me.Label7.AutoSize = True
		Me.Label7.Location = New System.Drawing.Point(154, 59)
		Me.Label7.Name = "Label7"
		Me.Label7.Size = New System.Drawing.Size(32, 13)
		Me.Label7.TabIndex = 0
		Me.Label7.Text = "Rural"
		'
		'Label4
		'
		Me.Label4.AutoSize = True
		Me.Label4.Location = New System.Drawing.Point(6, 59)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(36, 13)
		Me.Label4.TabIndex = 0
		Me.Label4.Text = "Urban"
		'
		'PicBox
		'
		Me.PicBox.BackColor = System.Drawing.Color.LightGray
		Me.PicBox.Location = New System.Drawing.Point(499, 28)
		Me.PicBox.Name = "PicBox"
		Me.PicBox.Size = New System.Drawing.Size(474, 425)
		Me.PicBox.TabIndex = 40
		Me.PicBox.TabStop = False
		'
		'TbFLD
		'
		Me.TbFLD.Location = New System.Drawing.Point(12, 202)
		Me.TbFLD.Name = "TbFLD"
		Me.TbFLD.Size = New System.Drawing.Size(418, 20)
		Me.TbFLD.TabIndex = 5
		'
		'Label14
		'
		Me.Label14.AutoSize = True
		Me.Label14.Location = New System.Drawing.Point(12, 186)
		Me.Label14.Name = "Label14"
		Me.Label14.Size = New System.Drawing.Size(128, 13)
		Me.Label14.TabIndex = 38
		Me.Label14.Text = "Full Load and Drag Curve"
		'
		'BtFLD
		'
		Me.BtFLD.Location = New System.Drawing.Point(436, 200)
		Me.BtFLD.Name = "BtFLD"
		Me.BtFLD.Size = New System.Drawing.Size(28, 23)
		Me.BtFLD.TabIndex = 6
		Me.BtFLD.TabStop = False
		Me.BtFLD.Text = "..."
		Me.BtFLD.UseVisualStyleBackColor = True
		'
		'BtFLDopen
		'
		Me.BtFLDopen.Image = Global.VECTO.My.Resources.Resources.application_export_icon_small
		Me.BtFLDopen.Location = New System.Drawing.Point(470, 200)
		Me.BtFLDopen.Name = "BtFLDopen"
		Me.BtFLDopen.Size = New System.Drawing.Size(23, 23)
		Me.BtFLDopen.TabIndex = 7
		Me.BtFLDopen.TabStop = False
		Me.BtFLDopen.UseVisualStyleBackColor = True
		'
		'F_ENG
		'
		Me.AcceptButton = Me.ButOK
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.CancelButton = Me.ButCancel
		Me.ClientSize = New System.Drawing.Size(985, 517)
		Me.Controls.Add(Me.PicBox)
		Me.Controls.Add(Me.GrWHTC)
		Me.Controls.Add(Me.PnInertia)
		Me.Controls.Add(Me.BtFLDopen)
		Me.Controls.Add(Me.BtMAPopen)
		Me.Controls.Add(Me.PictureBox1)
		Me.Controls.Add(Me.BtFLD)
		Me.Controls.Add(Me.BtMAP)
		Me.Controls.Add(Me.Label14)
		Me.Controls.Add(Me.Label6)
		Me.Controls.Add(Me.TbNleerl)
		Me.Controls.Add(Me.StatusStrip1)
		Me.Controls.Add(Me.Label11)
		Me.Controls.Add(Me.ToolStrip1)
		Me.Controls.Add(Me.TbDispl)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.ButCancel)
		Me.Controls.Add(Me.TbFLD)
		Me.Controls.Add(Me.ButOK)
		Me.Controls.Add(Me.TbMAP)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.Label40)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.TbName)
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
		Me.PnInertia.ResumeLayout(False)
		Me.PnInertia.PerformLayout()
		Me.GrWHTC.ResumeLayout(False)
		Me.GrWHTC.PerformLayout()
		CType(Me.PicBox, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
	Friend WithEvents TbNleerl As System.Windows.Forms.TextBox
	Friend WithEvents Label11 As System.Windows.Forms.Label
	Friend WithEvents TbInertia As System.Windows.Forms.TextBox
	Friend WithEvents Label41 As System.Windows.Forms.Label
	Friend WithEvents Label40 As System.Windows.Forms.Label
	Friend WithEvents Label5 As System.Windows.Forms.Label
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
	Friend WithEvents TbMAP As System.Windows.Forms.TextBox
	Friend WithEvents Label6 As System.Windows.Forms.Label
	Friend WithEvents BtMAP As System.Windows.Forms.Button
	Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
	Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
	Friend WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
	Friend WithEvents CmOpenFile As System.Windows.Forms.ContextMenuStrip
	Friend WithEvents OpenWithToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
	Friend WithEvents ShowInFolderToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
	Friend WithEvents BtMAPopen As System.Windows.Forms.Button
	Friend WithEvents PnInertia As System.Windows.Forms.Panel
	Friend WithEvents GrWHTC As System.Windows.Forms.GroupBox
	Friend WithEvents TbWHTCmw As System.Windows.Forms.TextBox
	Friend WithEvents TbWHTCrural As System.Windows.Forms.TextBox
	Friend WithEvents TbWHTCurban As System.Windows.Forms.TextBox
	Friend WithEvents Label8 As System.Windows.Forms.Label
	Friend WithEvents Label7 As System.Windows.Forms.Label
	Friend WithEvents Label4 As System.Windows.Forms.Label
	Friend WithEvents PicBox As System.Windows.Forms.PictureBox
	Friend WithEvents Label13 As System.Windows.Forms.Label
	Friend WithEvents TbFLD As System.Windows.Forms.TextBox
	Friend WithEvents Label14 As System.Windows.Forms.Label
	Friend WithEvents BtFLD As System.Windows.Forms.Button
	Friend WithEvents BtFLDopen As System.Windows.Forms.Button
End Class
