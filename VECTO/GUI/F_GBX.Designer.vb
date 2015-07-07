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
Partial Class F_GBX
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(F_GBX))
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
		Me.ButCancel = New System.Windows.Forms.Button()
		Me.ButOK = New System.Windows.Forms.Button()
		Me.TbTracInt = New System.Windows.Forms.TextBox()
		Me.LvGears = New System.Windows.Forms.ListView()
		Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.ColumnHeader6 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
		Me.TBI_getr = New System.Windows.Forms.TextBox()
		Me.Label49 = New System.Windows.Forms.Label()
		Me.Label33 = New System.Windows.Forms.Label()
		Me.Label48 = New System.Windows.Forms.Label()
		Me.Label6 = New System.Windows.Forms.Label()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.TbName = New System.Windows.Forms.TextBox()
		Me.PictureBox1 = New System.Windows.Forms.PictureBox()
		Me.BtRemGear = New System.Windows.Forms.Button()
		Me.GrGearShift = New System.Windows.Forms.GroupBox()
		Me.PnTorqRes = New System.Windows.Forms.Panel()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.TbTqResv = New System.Windows.Forms.TextBox()
		Me.ChShiftInside = New System.Windows.Forms.CheckBox()
		Me.TbShiftTime = New System.Windows.Forms.TextBox()
		Me.Label12 = New System.Windows.Forms.Label()
		Me.Label13 = New System.Windows.Forms.Label()
		Me.ChSkipGears = New System.Windows.Forms.CheckBox()
		Me.GroupBox2 = New System.Windows.Forms.GroupBox()
		Me.TbStartAcc = New System.Windows.Forms.TextBox()
		Me.Label11 = New System.Windows.Forms.Label()
		Me.TbStartSpeed = New System.Windows.Forms.TextBox()
		Me.Label9 = New System.Windows.Forms.Label()
		Me.Label10 = New System.Windows.Forms.Label()
		Me.TbTqResvStart = New System.Windows.Forms.TextBox()
		Me.Label8 = New System.Windows.Forms.Label()
		Me.Label5 = New System.Windows.Forms.Label()
		Me.Label7 = New System.Windows.Forms.Label()
		Me.CmOpenFile = New System.Windows.Forms.ContextMenuStrip(Me.components)
		Me.OpenWithToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
		Me.ShowInFolderToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
		Me.GroupBox3 = New System.Windows.Forms.GroupBox()
		Me.PnTC = New System.Windows.Forms.Panel()
		Me.Label17 = New System.Windows.Forms.Label()
		Me.Label18 = New System.Windows.Forms.Label()
		Me.Label15 = New System.Windows.Forms.Label()
		Me.TbTCinertia = New System.Windows.Forms.TextBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Label14 = New System.Windows.Forms.Label()
		Me.BtTCfileBrowse = New System.Windows.Forms.Button()
		Me.TbTCfile = New System.Windows.Forms.TextBox()
		Me.BtTCfileOpen = New System.Windows.Forms.Button()
		Me.TbTCrefrpm = New System.Windows.Forms.TextBox()
		Me.ChTCon = New System.Windows.Forms.CheckBox()
		Me.Label16 = New System.Windows.Forms.Label()
		Me.CbGStype = New System.Windows.Forms.ComboBox()
		Me.BtAddGear = New System.Windows.Forms.Button()
		Me.GroupBox4 = New System.Windows.Forms.GroupBox()
		Me.Label32 = New System.Windows.Forms.Label()
		Me.PnInertiaTI = New System.Windows.Forms.Panel()
		Me.PicBox = New System.Windows.Forms.PictureBox()
		Me.ToolStrip1.SuspendLayout()
		Me.StatusStrip1.SuspendLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.GrGearShift.SuspendLayout()
		Me.PnTorqRes.SuspendLayout()
		Me.GroupBox2.SuspendLayout()
		Me.CmOpenFile.SuspendLayout()
		Me.GroupBox3.SuspendLayout()
		Me.PnTC.SuspendLayout()
		Me.GroupBox4.SuspendLayout()
		Me.PnInertiaTI.SuspendLayout()
		CType(Me.PicBox, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.SuspendLayout()
		'
		'ToolStrip1
		'
		Me.ToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
		Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripBtNew, Me.ToolStripBtOpen, Me.ToolStripBtSave, Me.ToolStripBtSaveAs, Me.ToolStripSeparator3, Me.ToolStripBtSendTo, Me.ToolStripSeparator1, Me.ToolStripButton1})
		Me.ToolStrip1.Location = New System.Drawing.Point(0, 0)
		Me.ToolStrip1.Name = "ToolStrip1"
		Me.ToolStrip1.Size = New System.Drawing.Size(877, 25)
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
		Me.StatusStrip1.Location = New System.Drawing.Point(0, 595)
		Me.StatusStrip1.Name = "StatusStrip1"
		Me.StatusStrip1.Size = New System.Drawing.Size(877, 22)
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
		'ButCancel
		'
		Me.ButCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.ButCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.ButCancel.Location = New System.Drawing.Point(790, 569)
		Me.ButCancel.Name = "ButCancel"
		Me.ButCancel.Size = New System.Drawing.Size(75, 23)
		Me.ButCancel.TabIndex = 7
		Me.ButCancel.Text = "Cancel"
		Me.ButCancel.UseVisualStyleBackColor = True
		'
		'ButOK
		'
		Me.ButOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.ButOK.Location = New System.Drawing.Point(709, 569)
		Me.ButOK.Name = "ButOK"
		Me.ButOK.Size = New System.Drawing.Size(75, 23)
		Me.ButOK.TabIndex = 6
		Me.ButOK.Text = "Save"
		Me.ButOK.UseVisualStyleBackColor = True
		'
		'TbTracInt
		'
		Me.TbTracInt.Location = New System.Drawing.Point(303, 3)
		Me.TbTracInt.Name = "TbTracInt"
		Me.TbTracInt.Size = New System.Drawing.Size(40, 20)
		Me.TbTracInt.TabIndex = 1
		'
		'LvGears
		'
		Me.LvGears.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader4, Me.ColumnHeader2, Me.ColumnHeader3, Me.ColumnHeader5, Me.ColumnHeader6})
		Me.LvGears.FullRowSelect = True
		Me.LvGears.GridLines = True
		Me.LvGears.HideSelection = False
		Me.LvGears.Location = New System.Drawing.Point(6, 18)
		Me.LvGears.MultiSelect = False
		Me.LvGears.Name = "LvGears"
		Me.LvGears.Size = New System.Drawing.Size(429, 183)
		Me.LvGears.TabIndex = 0
		Me.LvGears.TabStop = False
		Me.LvGears.UseCompatibleStateImageBehavior = False
		Me.LvGears.View = System.Windows.Forms.View.Details
		'
		'ColumnHeader1
		'
		Me.ColumnHeader1.Text = "Gear"
		Me.ColumnHeader1.Width = 40
		'
		'ColumnHeader4
		'
		Me.ColumnHeader4.Text = "TC"
		Me.ColumnHeader4.Width = 31
		'
		'ColumnHeader2
		'
		Me.ColumnHeader2.Text = "Ratio"
		Me.ColumnHeader2.Width = 43
		'
		'ColumnHeader3
		'
		Me.ColumnHeader3.Text = "Loss Map or Efficiency [-]"
		Me.ColumnHeader3.Width = 137
		'
		'ColumnHeader5
		'
		Me.ColumnHeader5.Text = "Shift Polygons"
		Me.ColumnHeader5.Width = 97
		'
		'ColumnHeader6
		'
		Me.ColumnHeader6.Text = "Full Load Curve"
		Me.ColumnHeader6.Width = 89
		'
		'TBI_getr
		'
		Me.TBI_getr.Location = New System.Drawing.Point(53, 3)
		Me.TBI_getr.Name = "TBI_getr"
		Me.TBI_getr.Size = New System.Drawing.Size(57, 20)
		Me.TBI_getr.TabIndex = 0
		'
		'Label49
		'
		Me.Label49.AutoSize = True
		Me.Label49.Location = New System.Drawing.Point(349, 6)
		Me.Label49.Name = "Label49"
		Me.Label49.Size = New System.Drawing.Size(18, 13)
		Me.Label49.TabIndex = 2
		Me.Label49.Text = "[s]"
		'
		'Label33
		'
		Me.Label33.AutoSize = True
		Me.Label33.Location = New System.Drawing.Point(116, 6)
		Me.Label33.Name = "Label33"
		Me.Label33.Size = New System.Drawing.Size(36, 13)
		Me.Label33.TabIndex = 2
		Me.Label33.Text = "[kgm²]"
		'
		'Label48
		'
		Me.Label48.AutoSize = True
		Me.Label48.Location = New System.Drawing.Point(195, 6)
		Me.Label48.Name = "Label48"
		Me.Label48.Size = New System.Drawing.Size(102, 13)
		Me.Label48.TabIndex = 2
		Me.Label48.Text = "Traction Interruption"
		'
		'Label6
		'
		Me.Label6.AutoSize = True
		Me.Label6.Location = New System.Drawing.Point(11, 6)
		Me.Label6.Name = "Label6"
		Me.Label6.Size = New System.Drawing.Size(36, 13)
		Me.Label6.TabIndex = 2
		Me.Label6.Text = "Inertia"
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(26, 85)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(87, 13)
		Me.Label3.TabIndex = 42
		Me.Label3.Text = "Make and Model"
		'
		'TbName
		'
		Me.TbName.Location = New System.Drawing.Point(119, 82)
		Me.TbName.Name = "TbName"
		Me.TbName.Size = New System.Drawing.Size(334, 20)
		Me.TbName.TabIndex = 0
		'
		'PictureBox1
		'
		Me.PictureBox1.BackColor = System.Drawing.Color.White
		Me.PictureBox1.Image = Global.VECTO.My.Resources.Resources.VECTO_GBX
		Me.PictureBox1.Location = New System.Drawing.Point(12, 28)
		Me.PictureBox1.Name = "PictureBox1"
		Me.PictureBox1.Size = New System.Drawing.Size(441, 40)
		Me.PictureBox1.TabIndex = 43
		Me.PictureBox1.TabStop = False
		'
		'BtRemGear
		'
		Me.BtRemGear.Image = Global.VECTO.My.Resources.Resources.minus_circle_icon
		Me.BtRemGear.Location = New System.Drawing.Point(41, 207)
		Me.BtRemGear.Name = "BtRemGear"
		Me.BtRemGear.Size = New System.Drawing.Size(29, 23)
		Me.BtRemGear.TabIndex = 2
		Me.BtRemGear.UseVisualStyleBackColor = True
		'
		'GrGearShift
		'
		Me.GrGearShift.Controls.Add(Me.PnTorqRes)
		Me.GrGearShift.Controls.Add(Me.ChShiftInside)
		Me.GrGearShift.Controls.Add(Me.TbShiftTime)
		Me.GrGearShift.Controls.Add(Me.Label12)
		Me.GrGearShift.Controls.Add(Me.Label13)
		Me.GrGearShift.Controls.Add(Me.ChSkipGears)
		Me.GrGearShift.Controls.Add(Me.GroupBox2)
		Me.GrGearShift.Location = New System.Drawing.Point(12, 377)
		Me.GrGearShift.Name = "GrGearShift"
		Me.GrGearShift.Size = New System.Drawing.Size(441, 182)
		Me.GrGearShift.TabIndex = 3
		Me.GrGearShift.TabStop = False
		Me.GrGearShift.Text = "Gear shift parameters"
		'
		'PnTorqRes
		'
		Me.PnTorqRes.Controls.Add(Me.Label2)
		Me.PnTorqRes.Controls.Add(Me.Label4)
		Me.PnTorqRes.Controls.Add(Me.TbTqResv)
		Me.PnTorqRes.Location = New System.Drawing.Point(230, 19)
		Me.PnTorqRes.Name = "PnTorqRes"
		Me.PnTorqRes.Size = New System.Drawing.Size(190, 32)
		Me.PnTorqRes.TabIndex = 2
		'
		'Label2
		'
		Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(10, 9)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(79, 13)
		Me.Label2.TabIndex = 0
		Me.Label2.Text = "Torque reserve"
		'
		'Label4
		'
		Me.Label4.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Label4.AutoSize = True
		Me.Label4.Location = New System.Drawing.Point(158, 9)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(21, 13)
		Me.Label4.TabIndex = 0
		Me.Label4.Text = "[%]"
		'
		'TbTqResv
		'
		Me.TbTqResv.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.TbTqResv.Location = New System.Drawing.Point(95, 6)
		Me.TbTqResv.Name = "TbTqResv"
		Me.TbTqResv.Size = New System.Drawing.Size(57, 20)
		Me.TbTqResv.TabIndex = 0
		'
		'ChShiftInside
		'
		Me.ChShiftInside.AutoSize = True
		Me.ChShiftInside.Checked = True
		Me.ChShiftInside.CheckState = System.Windows.Forms.CheckState.Checked
		Me.ChShiftInside.Location = New System.Drawing.Point(9, 27)
		Me.ChShiftInside.Name = "ChShiftInside"
		Me.ChShiftInside.Size = New System.Drawing.Size(195, 17)
		Me.ChShiftInside.TabIndex = 0
		Me.ChShiftInside.Text = "Allow shift-up inside polygons (AMT)"
		Me.ChShiftInside.UseVisualStyleBackColor = True
		'
		'TbShiftTime
		'
		Me.TbShiftTime.Location = New System.Drawing.Point(325, 51)
		Me.TbShiftTime.Name = "TbShiftTime"
		Me.TbShiftTime.Size = New System.Drawing.Size(57, 20)
		Me.TbShiftTime.TabIndex = 3
		'
		'Label12
		'
		Me.Label12.AutoSize = True
		Me.Label12.Location = New System.Drawing.Point(388, 54)
		Me.Label12.Name = "Label12"
		Me.Label12.Size = New System.Drawing.Size(18, 13)
		Me.Label12.TabIndex = 9
		Me.Label12.Text = "[s]"
		'
		'Label13
		'
		Me.Label13.AutoSize = True
		Me.Label13.Location = New System.Drawing.Point(227, 54)
		Me.Label13.Name = "Label13"
		Me.Label13.Size = New System.Drawing.Size(92, 13)
		Me.Label13.TabIndex = 8
		Me.Label13.Text = "Minimum shift time"
		'
		'ChSkipGears
		'
		Me.ChSkipGears.AutoSize = True
		Me.ChSkipGears.Location = New System.Drawing.Point(9, 53)
		Me.ChSkipGears.Name = "ChSkipGears"
		Me.ChSkipGears.Size = New System.Drawing.Size(132, 17)
		Me.ChSkipGears.TabIndex = 1
		Me.ChSkipGears.Text = "Skip Gears (MT, AMT)"
		Me.ChSkipGears.UseVisualStyleBackColor = True
		'
		'GroupBox2
		'
		Me.GroupBox2.Controls.Add(Me.TbStartAcc)
		Me.GroupBox2.Controls.Add(Me.Label11)
		Me.GroupBox2.Controls.Add(Me.TbStartSpeed)
		Me.GroupBox2.Controls.Add(Me.Label9)
		Me.GroupBox2.Controls.Add(Me.Label10)
		Me.GroupBox2.Controls.Add(Me.TbTqResvStart)
		Me.GroupBox2.Controls.Add(Me.Label8)
		Me.GroupBox2.Controls.Add(Me.Label5)
		Me.GroupBox2.Controls.Add(Me.Label7)
		Me.GroupBox2.Location = New System.Drawing.Point(6, 77)
		Me.GroupBox2.Name = "GroupBox2"
		Me.GroupBox2.Size = New System.Drawing.Size(429, 99)
		Me.GroupBox2.TabIndex = 4
		Me.GroupBox2.TabStop = False
		Me.GroupBox2.Text = "Start Gear"
		'
		'TbStartAcc
		'
		Me.TbStartAcc.Location = New System.Drawing.Point(209, 71)
		Me.TbStartAcc.Name = "TbStartAcc"
		Me.TbStartAcc.Size = New System.Drawing.Size(57, 20)
		Me.TbStartAcc.TabIndex = 2
		'
		'Label11
		'
		Me.Label11.AutoSize = True
		Me.Label11.Location = New System.Drawing.Point(20, 74)
		Me.Label11.Name = "Label11"
		Me.Label11.Size = New System.Drawing.Size(173, 13)
		Me.Label11.TabIndex = 0
		Me.Label11.Text = "Reference acceleration at clutch-in"
		'
		'TbStartSpeed
		'
		Me.TbStartSpeed.Location = New System.Drawing.Point(209, 45)
		Me.TbStartSpeed.Name = "TbStartSpeed"
		Me.TbStartSpeed.Size = New System.Drawing.Size(57, 20)
		Me.TbStartSpeed.TabIndex = 1
		'
		'Label9
		'
		Me.Label9.AutoSize = True
		Me.Label9.Location = New System.Drawing.Point(20, 48)
		Me.Label9.Name = "Label9"
		Me.Label9.Size = New System.Drawing.Size(181, 13)
		Me.Label9.TabIndex = 0
		Me.Label9.Text = "Reference vehicle speed at clutch-in"
		'
		'Label10
		'
		Me.Label10.AutoSize = True
		Me.Label10.Location = New System.Drawing.Point(272, 74)
		Me.Label10.Name = "Label10"
		Me.Label10.Size = New System.Drawing.Size(34, 13)
		Me.Label10.TabIndex = 0
		Me.Label10.Text = "[m/s²]"
		'
		'TbTqResvStart
		'
		Me.TbTqResvStart.Location = New System.Drawing.Point(209, 19)
		Me.TbTqResvStart.Name = "TbTqResvStart"
		Me.TbTqResvStart.Size = New System.Drawing.Size(57, 20)
		Me.TbTqResvStart.TabIndex = 0
		'
		'Label8
		'
		Me.Label8.AutoSize = True
		Me.Label8.Location = New System.Drawing.Point(272, 48)
		Me.Label8.Name = "Label8"
		Me.Label8.Size = New System.Drawing.Size(31, 13)
		Me.Label8.TabIndex = 0
		Me.Label8.Text = "[m/s]"
		'
		'Label5
		'
		Me.Label5.AutoSize = True
		Me.Label5.Location = New System.Drawing.Point(20, 22)
		Me.Label5.Name = "Label5"
		Me.Label5.Size = New System.Drawing.Size(79, 13)
		Me.Label5.TabIndex = 0
		Me.Label5.Text = "Torque reserve"
		'
		'Label7
		'
		Me.Label7.AutoSize = True
		Me.Label7.Location = New System.Drawing.Point(272, 22)
		Me.Label7.Name = "Label7"
		Me.Label7.Size = New System.Drawing.Size(21, 13)
		Me.Label7.TabIndex = 0
		Me.Label7.Text = "[%]"
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
		'GroupBox3
		'
		Me.GroupBox3.Controls.Add(Me.PnTC)
		Me.GroupBox3.Controls.Add(Me.ChTCon)
		Me.GroupBox3.Location = New System.Drawing.Point(459, 440)
		Me.GroupBox3.Name = "GroupBox3"
		Me.GroupBox3.Size = New System.Drawing.Size(414, 119)
		Me.GroupBox3.TabIndex = 5
		Me.GroupBox3.TabStop = False
		Me.GroupBox3.Text = "Torque Converter"
		'
		'PnTC
		'
		Me.PnTC.Controls.Add(Me.Label17)
		Me.PnTC.Controls.Add(Me.Label18)
		Me.PnTC.Controls.Add(Me.Label15)
		Me.PnTC.Controls.Add(Me.TbTCinertia)
		Me.PnTC.Controls.Add(Me.Label1)
		Me.PnTC.Controls.Add(Me.Label14)
		Me.PnTC.Controls.Add(Me.BtTCfileBrowse)
		Me.PnTC.Controls.Add(Me.TbTCfile)
		Me.PnTC.Controls.Add(Me.BtTCfileOpen)
		Me.PnTC.Controls.Add(Me.TbTCrefrpm)
		Me.PnTC.Location = New System.Drawing.Point(6, 39)
		Me.PnTC.Name = "PnTC"
		Me.PnTC.Size = New System.Drawing.Size(402, 72)
		Me.PnTC.TabIndex = 36
		'
		'Label17
		'
		Me.Label17.AutoSize = True
		Me.Label17.Location = New System.Drawing.Point(0, 5)
		Me.Label17.Name = "Label17"
		Me.Label17.Size = New System.Drawing.Size(176, 13)
		Me.Label17.TabIndex = 0
		Me.Label17.Text = "Torque converter characteristics file"
		'
		'Label18
		'
		Me.Label18.AutoSize = True
		Me.Label18.Location = New System.Drawing.Point(5, 50)
		Me.Label18.Name = "Label18"
		Me.Label18.Size = New System.Drawing.Size(36, 13)
		Me.Label18.TabIndex = 2
		Me.Label18.Text = "Inertia"
		'
		'Label15
		'
		Me.Label15.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Label15.AutoSize = True
		Me.Label15.Location = New System.Drawing.Point(185, 50)
		Me.Label15.Name = "Label15"
		Me.Label15.Size = New System.Drawing.Size(77, 13)
		Me.Label15.TabIndex = 0
		Me.Label15.Text = "Reference rpm"
		'
		'TbTCinertia
		'
		Me.TbTCinertia.Location = New System.Drawing.Point(47, 47)
		Me.TbTCinertia.Name = "TbTCinertia"
		Me.TbTCinertia.Size = New System.Drawing.Size(67, 20)
		Me.TbTCinertia.TabIndex = 3
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(120, 50)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(36, 13)
		Me.Label1.TabIndex = 2
		Me.Label1.Text = "[kgm²]"
		'
		'Label14
		'
		Me.Label14.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.Label14.AutoSize = True
		Me.Label14.Location = New System.Drawing.Point(341, 50)
		Me.Label14.Name = "Label14"
		Me.Label14.Size = New System.Drawing.Size(40, 13)
		Me.Label14.TabIndex = 0
		Me.Label14.Text = "[1/min]"
		'
		'BtTCfileBrowse
		'
		Me.BtTCfileBrowse.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.BtTCfileBrowse.Location = New System.Drawing.Point(341, 19)
		Me.BtTCfileBrowse.Name = "BtTCfileBrowse"
		Me.BtTCfileBrowse.Size = New System.Drawing.Size(32, 23)
		Me.BtTCfileBrowse.TabIndex = 1
		Me.BtTCfileBrowse.TabStop = False
		Me.BtTCfileBrowse.Text = "..."
		Me.BtTCfileBrowse.UseVisualStyleBackColor = True
		'
		'TbTCfile
		'
		Me.TbTCfile.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
			Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.TbTCfile.Location = New System.Drawing.Point(0, 21)
		Me.TbTCfile.Name = "TbTCfile"
		Me.TbTCfile.Size = New System.Drawing.Size(335, 20)
		Me.TbTCfile.TabIndex = 0
		'
		'BtTCfileOpen
		'
		Me.BtTCfileOpen.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.BtTCfileOpen.Image = Global.VECTO.My.Resources.Resources.application_export_icon_small
		Me.BtTCfileOpen.Location = New System.Drawing.Point(379, 19)
		Me.BtTCfileOpen.Name = "BtTCfileOpen"
		Me.BtTCfileOpen.Size = New System.Drawing.Size(23, 23)
		Me.BtTCfileOpen.TabIndex = 2
		Me.BtTCfileOpen.TabStop = False
		Me.BtTCfileOpen.UseVisualStyleBackColor = True
		'
		'TbTCrefrpm
		'
		Me.TbTCrefrpm.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
		Me.TbTCrefrpm.Location = New System.Drawing.Point(268, 47)
		Me.TbTCrefrpm.Name = "TbTCrefrpm"
		Me.TbTCrefrpm.Size = New System.Drawing.Size(67, 20)
		Me.TbTCrefrpm.TabIndex = 4
		'
		'ChTCon
		'
		Me.ChTCon.AutoSize = True
		Me.ChTCon.Checked = True
		Me.ChTCon.CheckState = System.Windows.Forms.CheckState.Checked
		Me.ChTCon.Location = New System.Drawing.Point(9, 19)
		Me.ChTCon.Name = "ChTCon"
		Me.ChTCon.Size = New System.Drawing.Size(88, 17)
		Me.ChTCon.TabIndex = 0
		Me.ChTCon.Text = "Installed (AT)"
		Me.ChTCon.UseVisualStyleBackColor = True
		'
		'Label16
		'
		Me.Label16.AutoSize = True
		Me.Label16.Location = New System.Drawing.Point(18, 111)
		Me.Label16.Name = "Label16"
		Me.Label16.Size = New System.Drawing.Size(95, 13)
		Me.Label16.TabIndex = 45
		Me.Label16.Text = "Transmission Type"
		'
		'CbGStype
		'
		Me.CbGStype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
		Me.CbGStype.FormattingEnabled = True
		Me.CbGStype.Location = New System.Drawing.Point(119, 108)
		Me.CbGStype.Name = "CbGStype"
		Me.CbGStype.Size = New System.Drawing.Size(227, 21)
		Me.CbGStype.TabIndex = 1
		'
		'BtAddGear
		'
		Me.BtAddGear.Image = Global.VECTO.My.Resources.Resources.plus_circle_icon
		Me.BtAddGear.Location = New System.Drawing.Point(6, 207)
		Me.BtAddGear.Name = "BtAddGear"
		Me.BtAddGear.Size = New System.Drawing.Size(29, 23)
		Me.BtAddGear.TabIndex = 1
		Me.BtAddGear.UseVisualStyleBackColor = True
		'
		'GroupBox4
		'
		Me.GroupBox4.Controls.Add(Me.Label32)
		Me.GroupBox4.Controls.Add(Me.LvGears)
		Me.GroupBox4.Controls.Add(Me.BtRemGear)
		Me.GroupBox4.Controls.Add(Me.BtAddGear)
		Me.GroupBox4.Location = New System.Drawing.Point(12, 135)
		Me.GroupBox4.Name = "GroupBox4"
		Me.GroupBox4.Size = New System.Drawing.Size(441, 236)
		Me.GroupBox4.TabIndex = 2
		Me.GroupBox4.TabStop = False
		Me.GroupBox4.Text = "Gears"
		'
		'Label32
		'
		Me.Label32.AutoSize = True
		Me.Label32.Location = New System.Drawing.Point(312, 212)
		Me.Label32.Name = "Label32"
		Me.Label32.Size = New System.Drawing.Size(123, 13)
		Me.Label32.TabIndex = 3
		Me.Label32.Text = "Double-Click to edit gear"
		'
		'PnInertiaTI
		'
		Me.PnInertiaTI.Controls.Add(Me.Label6)
		Me.PnInertiaTI.Controls.Add(Me.TBI_getr)
		Me.PnInertiaTI.Controls.Add(Me.Label33)
		Me.PnInertiaTI.Controls.Add(Me.Label48)
		Me.PnInertiaTI.Controls.Add(Me.Label49)
		Me.PnInertiaTI.Controls.Add(Me.TbTracInt)
		Me.PnInertiaTI.Location = New System.Drawing.Point(469, 406)
		Me.PnInertiaTI.Name = "PnInertiaTI"
		Me.PnInertiaTI.Size = New System.Drawing.Size(398, 30)
		Me.PnInertiaTI.TabIndex = 4
		'
		'PicBox
		'
		Me.PicBox.BackColor = System.Drawing.Color.LightGray
		Me.PicBox.Location = New System.Drawing.Point(459, 28)
		Me.PicBox.Name = "PicBox"
		Me.PicBox.Size = New System.Drawing.Size(406, 372)
		Me.PicBox.TabIndex = 48
		Me.PicBox.TabStop = False
		'
		'F_GBX
		'
		Me.AcceptButton = Me.ButOK
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.CancelButton = Me.ButCancel
		Me.ClientSize = New System.Drawing.Size(877, 617)
		Me.Controls.Add(Me.PicBox)
		Me.Controls.Add(Me.PnInertiaTI)
		Me.Controls.Add(Me.GroupBox4)
		Me.Controls.Add(Me.CbGStype)
		Me.Controls.Add(Me.Label16)
		Me.Controls.Add(Me.PictureBox1)
		Me.Controls.Add(Me.GrGearShift)
		Me.Controls.Add(Me.GroupBox3)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.TbName)
		Me.Controls.Add(Me.ButCancel)
		Me.Controls.Add(Me.ButOK)
		Me.Controls.Add(Me.StatusStrip1)
		Me.Controls.Add(Me.ToolStrip1)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.Name = "F_GBX"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.Text = "F_GBX"
		Me.ToolStrip1.ResumeLayout(False)
		Me.ToolStrip1.PerformLayout()
		Me.StatusStrip1.ResumeLayout(False)
		Me.StatusStrip1.PerformLayout()
		CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
		Me.GrGearShift.ResumeLayout(False)
		Me.GrGearShift.PerformLayout()
		Me.PnTorqRes.ResumeLayout(False)
		Me.PnTorqRes.PerformLayout()
		Me.GroupBox2.ResumeLayout(False)
		Me.GroupBox2.PerformLayout()
		Me.CmOpenFile.ResumeLayout(False)
		Me.GroupBox3.ResumeLayout(False)
		Me.GroupBox3.PerformLayout()
		Me.PnTC.ResumeLayout(False)
		Me.PnTC.PerformLayout()
		Me.GroupBox4.ResumeLayout(False)
		Me.GroupBox4.PerformLayout()
		Me.PnInertiaTI.ResumeLayout(False)
		Me.PnInertiaTI.PerformLayout()
		CType(Me.PicBox, System.ComponentModel.ISupportInitialize).EndInit()
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
    Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripBtNew As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripBtOpen As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripBtSave As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripBtSaveAs As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator3 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripBtSendTo As System.Windows.Forms.ToolStripButton
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents LbStatus As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ButCancel As System.Windows.Forms.Button
    Friend WithEvents ButOK As System.Windows.Forms.Button
    Friend WithEvents BtRemGear As System.Windows.Forms.Button
    Friend WithEvents TbTracInt As System.Windows.Forms.TextBox
    Friend WithEvents LvGears As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents TBI_getr As System.Windows.Forms.TextBox
    Friend WithEvents Label49 As System.Windows.Forms.Label
    Friend WithEvents Label33 As System.Windows.Forms.Label
    Friend WithEvents Label48 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents TbName As System.Windows.Forms.TextBox
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Friend WithEvents GrGearShift As System.Windows.Forms.GroupBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents TbTqResvStart As System.Windows.Forms.TextBox
    Friend WithEvents TbTqResv As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents TbStartAcc As System.Windows.Forms.TextBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents TbStartSpeed As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents TbShiftTime As System.Windows.Forms.TextBox
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents ChSkipGears As System.Windows.Forms.CheckBox
    Friend WithEvents ChShiftInside As System.Windows.Forms.CheckBox
    Friend WithEvents CmOpenFile As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents OpenWithToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ShowInFolderToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents TbTCfile As System.Windows.Forms.TextBox
    Friend WithEvents ChTCon As System.Windows.Forms.CheckBox
    Friend WithEvents BtTCfileBrowse As System.Windows.Forms.Button
    Friend WithEvents BtTCfileOpen As System.Windows.Forms.Button
    Friend WithEvents TbTCrefrpm As System.Windows.Forms.TextBox
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents Label15 As System.Windows.Forms.Label
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents CbGStype As System.Windows.Forms.ComboBox
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents PnTC As System.Windows.Forms.Panel
    Friend WithEvents PnTorqRes As System.Windows.Forms.Panel
    Friend WithEvents BtAddGear As System.Windows.Forms.Button
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents Label32 As System.Windows.Forms.Label
    Friend WithEvents PnInertiaTI As System.Windows.Forms.Panel
    Friend WithEvents ColumnHeader5 As System.Windows.Forms.ColumnHeader
    Friend WithEvents PicBox As System.Windows.Forms.PictureBox
    Friend WithEvents TbTCinertia As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
	Friend WithEvents Label18 As System.Windows.Forms.Label
	Friend WithEvents ColumnHeader6 As System.Windows.Forms.ColumnHeader
End Class
