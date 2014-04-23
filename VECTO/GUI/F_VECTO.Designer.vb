<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_VECTO
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(F_VECTO))
        Me.TabPgGen = New System.Windows.Forms.TabPage()
        Me.GrCycles = New System.Windows.Forms.GroupBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.LvCycles = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.BtDRIrem = New System.Windows.Forms.Button()
        Me.BtDRIadd = New System.Windows.Forms.Button()
        Me.GrAux = New System.Windows.Forms.GroupBox()
        Me.Label32 = New System.Windows.Forms.Label()
        Me.LvAux = New System.Windows.Forms.ListView()
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader6 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ButAuxRem = New System.Windows.Forms.Button()
        Me.ButAuxAdd = New System.Windows.Forms.Button()
        Me.TbGBX = New System.Windows.Forms.TextBox()
        Me.TbENG = New System.Windows.Forms.TextBox()
        Me.TbVEH = New System.Windows.Forms.TextBox()
        Me.ButOpenGBX = New System.Windows.Forms.Button()
        Me.ButOpenENG = New System.Windows.Forms.Button()
        Me.ButOpenVEH = New System.Windows.Forms.Button()
        Me.ButtonVEH = New System.Windows.Forms.Button()
        Me.ButtonGBX = New System.Windows.Forms.Button()
        Me.ButtonMAP = New System.Windows.Forms.Button()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPgDriver = New System.Windows.Forms.TabPage()
        Me.GrVACC = New System.Windows.Forms.GroupBox()
        Me.TbDesMaxFile = New System.Windows.Forms.TextBox()
        Me.BtDesMaxBr = New System.Windows.Forms.Button()
        Me.BtAccOpen = New System.Windows.Forms.Button()
        Me.GrLAC = New System.Windows.Forms.GroupBox()
        Me.PnLookAhead = New System.Windows.Forms.Panel()
        Me.Label29 = New System.Windows.Forms.Label()
        Me.Label25 = New System.Windows.Forms.Label()
        Me.TbAlookahead = New System.Windows.Forms.TextBox()
        Me.Label24 = New System.Windows.Forms.Label()
        Me.TbVminLA = New System.Windows.Forms.TextBox()
        Me.Label28 = New System.Windows.Forms.Label()
        Me.CbLookAhead = New System.Windows.Forms.CheckBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.PnEcoRoll = New System.Windows.Forms.Panel()
        Me.Label21 = New System.Windows.Forms.Label()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.TbVmin = New System.Windows.Forms.TextBox()
        Me.TbUnderSpeed = New System.Windows.Forms.TextBox()
        Me.TbOverspeed = New System.Windows.Forms.TextBox()
        Me.Label23 = New System.Windows.Forms.Label()
        Me.Label22 = New System.Windows.Forms.Label()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.RdEcoRoll = New System.Windows.Forms.RadioButton()
        Me.RdOverspeed = New System.Windows.Forms.RadioButton()
        Me.RdOff = New System.Windows.Forms.RadioButton()
        Me.GrStartStop = New System.Windows.Forms.GroupBox()
        Me.PnStartStop = New System.Windows.Forms.Panel()
        Me.Label31 = New System.Windows.Forms.Label()
        Me.Label27 = New System.Windows.Forms.Label()
        Me.TbSSspeed = New System.Windows.Forms.TextBox()
        Me.LabelSSspeed = New System.Windows.Forms.Label()
        Me.Label26 = New System.Windows.Forms.Label()
        Me.Label30 = New System.Windows.Forms.Label()
        Me.LabelSStime = New System.Windows.Forms.Label()
        Me.TbSSdelay = New System.Windows.Forms.TextBox()
        Me.TbSStime = New System.Windows.Forms.TextBox()
        Me.ChBStartStop = New System.Windows.Forms.CheckBox()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ToolStripStatusLabelGEN = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ButOK = New System.Windows.Forms.Button()
        Me.ButCancel = New System.Windows.Forms.Button()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.ToolStripBtNew = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripBtOpen = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripBtSave = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripBtSaveAs = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripBtSendTo = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.CbEngOnly = New System.Windows.Forms.CheckBox()
        Me.CmOpenFile = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.OpenWithToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ShowInFolderToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.PicVehicle = New System.Windows.Forms.PictureBox()
        Me.PicBox = New System.Windows.Forms.PictureBox()
        Me.TbEngTxt = New System.Windows.Forms.TextBox()
        Me.TbVehCat = New System.Windows.Forms.TextBox()
        Me.TbAxleConf = New System.Windows.Forms.TextBox()
        Me.TbHVCclass = New System.Windows.Forms.TextBox()
        Me.TbGbxTxt = New System.Windows.Forms.TextBox()
        Me.TbMass = New System.Windows.Forms.TextBox()
        Me.TabPgGen.SuspendLayout()
        Me.GrCycles.SuspendLayout()
        Me.GrAux.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.TabPgDriver.SuspendLayout()
        Me.GrVACC.SuspendLayout()
        Me.GrLAC.SuspendLayout()
        Me.PnLookAhead.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.PnEcoRoll.SuspendLayout()
        Me.GrStartStop.SuspendLayout()
        Me.PnStartStop.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.ToolStrip1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.CmOpenFile.SuspendLayout()
        CType(Me.PicVehicle, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PicBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TabPgGen
        '
        Me.TabPgGen.Controls.Add(Me.GrCycles)
        Me.TabPgGen.Controls.Add(Me.GrAux)
        Me.TabPgGen.Controls.Add(Me.TbGBX)
        Me.TabPgGen.Controls.Add(Me.TbENG)
        Me.TabPgGen.Controls.Add(Me.TbVEH)
        Me.TabPgGen.Controls.Add(Me.ButOpenGBX)
        Me.TabPgGen.Controls.Add(Me.ButOpenENG)
        Me.TabPgGen.Controls.Add(Me.ButOpenVEH)
        Me.TabPgGen.Controls.Add(Me.ButtonVEH)
        Me.TabPgGen.Controls.Add(Me.ButtonGBX)
        Me.TabPgGen.Controls.Add(Me.ButtonMAP)
        Me.TabPgGen.Location = New System.Drawing.Point(4, 22)
        Me.TabPgGen.Name = "TabPgGen"
        Me.TabPgGen.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPgGen.Size = New System.Drawing.Size(527, 491)
        Me.TabPgGen.TabIndex = 0
        Me.TabPgGen.Text = "General"
        Me.TabPgGen.UseVisualStyleBackColor = True
        '
        'GrCycles
        '
        Me.GrCycles.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GrCycles.Controls.Add(Me.Label2)
        Me.GrCycles.Controls.Add(Me.LvCycles)
        Me.GrCycles.Controls.Add(Me.BtDRIrem)
        Me.GrCycles.Controls.Add(Me.BtDRIadd)
        Me.GrCycles.Location = New System.Drawing.Point(7, 271)
        Me.GrCycles.Name = "GrCycles"
        Me.GrCycles.Size = New System.Drawing.Size(515, 208)
        Me.GrCycles.TabIndex = 34
        Me.GrCycles.TabStop = False
        Me.GrCycles.Text = "Cycles"
        '
        'Label2
        '
        Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(387, 153)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(122, 13)
        Me.Label2.TabIndex = 33
        Me.Label2.Text = "Double-Click to open file"
        '
        'LvCycles
        '
        Me.LvCycles.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LvCycles.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1})
        Me.LvCycles.FullRowSelect = True
        Me.LvCycles.GridLines = True
        Me.LvCycles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None
        Me.LvCycles.HideSelection = False
        Me.LvCycles.LabelEdit = True
        Me.LvCycles.Location = New System.Drawing.Point(6, 19)
        Me.LvCycles.MultiSelect = False
        Me.LvCycles.Name = "LvCycles"
        Me.LvCycles.Size = New System.Drawing.Size(503, 123)
        Me.LvCycles.TabIndex = 0
        Me.LvCycles.TabStop = False
        Me.LvCycles.UseCompatibleStateImageBehavior = False
        Me.LvCycles.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Cycle path"
        Me.ColumnHeader1.Width = 470
        '
        'BtDRIrem
        '
        Me.BtDRIrem.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.BtDRIrem.Image = Global.VECTO.My.Resources.Resources.minus_circle_icon
        Me.BtDRIrem.Location = New System.Drawing.Point(41, 179)
        Me.BtDRIrem.Name = "BtDRIrem"
        Me.BtDRIrem.Size = New System.Drawing.Size(29, 23)
        Me.BtDRIrem.TabIndex = 32
        Me.BtDRIrem.UseVisualStyleBackColor = True
        '
        'BtDRIadd
        '
        Me.BtDRIadd.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.BtDRIadd.Image = Global.VECTO.My.Resources.Resources.plus_circle_icon
        Me.BtDRIadd.Location = New System.Drawing.Point(6, 179)
        Me.BtDRIadd.Name = "BtDRIadd"
        Me.BtDRIadd.Size = New System.Drawing.Size(29, 23)
        Me.BtDRIadd.TabIndex = 31
        Me.BtDRIadd.UseVisualStyleBackColor = True
        '
        'GrAux
        '
        Me.GrAux.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GrAux.Controls.Add(Me.Label32)
        Me.GrAux.Controls.Add(Me.LvAux)
        Me.GrAux.Controls.Add(Me.ButAuxRem)
        Me.GrAux.Controls.Add(Me.ButAuxAdd)
        Me.GrAux.Location = New System.Drawing.Point(6, 87)
        Me.GrAux.Name = "GrAux"
        Me.GrAux.Size = New System.Drawing.Size(515, 209)
        Me.GrAux.TabIndex = 33
        Me.GrAux.TabStop = False
        Me.GrAux.Text = "Auxiliaries"
        '
        'Label32
        '
        Me.Label32.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label32.AutoSize = True
        Me.Label32.Location = New System.Drawing.Point(369, 147)
        Me.Label32.Name = "Label32"
        Me.Label32.Size = New System.Drawing.Size(139, 13)
        Me.Label32.TabIndex = 34
        Me.Label32.Text = "Double-Click to edit auxiliary"
        '
        'LvAux
        '
        Me.LvAux.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LvAux.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader4, Me.ColumnHeader5, Me.ColumnHeader6})
        Me.LvAux.FullRowSelect = True
        Me.LvAux.GridLines = True
        Me.LvAux.HideSelection = False
        Me.LvAux.Location = New System.Drawing.Point(6, 19)
        Me.LvAux.MultiSelect = False
        Me.LvAux.Name = "LvAux"
        Me.LvAux.Size = New System.Drawing.Size(503, 117)
        Me.LvAux.TabIndex = 30
        Me.LvAux.TabStop = False
        Me.LvAux.UseCompatibleStateImageBehavior = False
        Me.LvAux.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader4
        '
        Me.ColumnHeader4.Text = "ID"
        Me.ColumnHeader4.Width = 45
        '
        'ColumnHeader5
        '
        Me.ColumnHeader5.Text = "Type"
        Me.ColumnHeader5.Width = 108
        '
        'ColumnHeader6
        '
        Me.ColumnHeader6.Text = "Input File"
        Me.ColumnHeader6.Width = 331
        '
        'ButAuxRem
        '
        Me.ButAuxRem.Image = Global.VECTO.My.Resources.Resources.minus_circle_icon
        Me.ButAuxRem.Location = New System.Drawing.Point(42, 142)
        Me.ButAuxRem.Name = "ButAuxRem"
        Me.ButAuxRem.Size = New System.Drawing.Size(29, 23)
        Me.ButAuxRem.TabIndex = 32
        Me.ButAuxRem.UseVisualStyleBackColor = True
        '
        'ButAuxAdd
        '
        Me.ButAuxAdd.Image = Global.VECTO.My.Resources.Resources.plus_circle_icon
        Me.ButAuxAdd.Location = New System.Drawing.Point(7, 142)
        Me.ButAuxAdd.Name = "ButAuxAdd"
        Me.ButAuxAdd.Size = New System.Drawing.Size(29, 23)
        Me.ButAuxAdd.TabIndex = 31
        Me.ButAuxAdd.UseVisualStyleBackColor = True
        '
        'TbGBX
        '
        Me.TbGBX.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TbGBX.Location = New System.Drawing.Point(84, 60)
        Me.TbGBX.Name = "TbGBX"
        Me.TbGBX.Size = New System.Drawing.Size(406, 20)
        Me.TbGBX.TabIndex = 3
        '
        'TbENG
        '
        Me.TbENG.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TbENG.Location = New System.Drawing.Point(84, 33)
        Me.TbENG.Name = "TbENG"
        Me.TbENG.Size = New System.Drawing.Size(406, 20)
        Me.TbENG.TabIndex = 2
        '
        'TbVEH
        '
        Me.TbVEH.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TbVEH.Location = New System.Drawing.Point(84, 7)
        Me.TbVEH.Name = "TbVEH"
        Me.TbVEH.Size = New System.Drawing.Size(406, 20)
        Me.TbVEH.TabIndex = 1
        '
        'ButOpenGBX
        '
        Me.ButOpenGBX.Location = New System.Drawing.Point(6, 60)
        Me.ButOpenGBX.Name = "ButOpenGBX"
        Me.ButOpenGBX.Size = New System.Drawing.Size(72, 21)
        Me.ButOpenGBX.TabIndex = 22
        Me.ButOpenGBX.TabStop = False
        Me.ButOpenGBX.Text = "Gearbox"
        Me.ButOpenGBX.UseVisualStyleBackColor = True
        '
        'ButOpenENG
        '
        Me.ButOpenENG.Location = New System.Drawing.Point(6, 33)
        Me.ButOpenENG.Name = "ButOpenENG"
        Me.ButOpenENG.Size = New System.Drawing.Size(72, 21)
        Me.ButOpenENG.TabIndex = 21
        Me.ButOpenENG.TabStop = False
        Me.ButOpenENG.Text = "Engine"
        Me.ButOpenENG.UseVisualStyleBackColor = True
        '
        'ButOpenVEH
        '
        Me.ButOpenVEH.Location = New System.Drawing.Point(6, 6)
        Me.ButOpenVEH.Name = "ButOpenVEH"
        Me.ButOpenVEH.Size = New System.Drawing.Size(72, 21)
        Me.ButOpenVEH.TabIndex = 20
        Me.ButOpenVEH.TabStop = False
        Me.ButOpenVEH.Text = "Vehicle"
        Me.ButOpenVEH.UseVisualStyleBackColor = True
        '
        'ButtonVEH
        '
        Me.ButtonVEH.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonVEH.Location = New System.Drawing.Point(496, 6)
        Me.ButtonVEH.Name = "ButtonVEH"
        Me.ButtonVEH.Size = New System.Drawing.Size(28, 21)
        Me.ButtonVEH.TabIndex = 11
        Me.ButtonVEH.TabStop = False
        Me.ButtonVEH.Text = "..."
        Me.ButtonVEH.UseVisualStyleBackColor = True
        '
        'ButtonGBX
        '
        Me.ButtonGBX.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonGBX.Location = New System.Drawing.Point(496, 60)
        Me.ButtonGBX.Name = "ButtonGBX"
        Me.ButtonGBX.Size = New System.Drawing.Size(28, 21)
        Me.ButtonGBX.TabIndex = 13
        Me.ButtonGBX.TabStop = False
        Me.ButtonGBX.Text = "..."
        Me.ButtonGBX.UseVisualStyleBackColor = True
        '
        'ButtonMAP
        '
        Me.ButtonMAP.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonMAP.Location = New System.Drawing.Point(496, 33)
        Me.ButtonMAP.Name = "ButtonMAP"
        Me.ButtonMAP.Size = New System.Drawing.Size(28, 21)
        Me.ButtonMAP.TabIndex = 12
        Me.ButtonMAP.TabStop = False
        Me.ButtonMAP.Text = "..."
        Me.ButtonMAP.UseVisualStyleBackColor = True
        '
        'TabControl1
        '
        Me.TabControl1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TabControl1.Controls.Add(Me.TabPgGen)
        Me.TabControl1.Controls.Add(Me.TabPgDriver)
        Me.TabControl1.Location = New System.Drawing.Point(1, 107)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(535, 517)
        Me.TabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed
        Me.TabControl1.TabIndex = 1
        '
        'TabPgDriver
        '
        Me.TabPgDriver.Controls.Add(Me.GrVACC)
        Me.TabPgDriver.Controls.Add(Me.GrLAC)
        Me.TabPgDriver.Controls.Add(Me.GroupBox1)
        Me.TabPgDriver.Controls.Add(Me.GrStartStop)
        Me.TabPgDriver.Location = New System.Drawing.Point(4, 22)
        Me.TabPgDriver.Name = "TabPgDriver"
        Me.TabPgDriver.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPgDriver.Size = New System.Drawing.Size(527, 491)
        Me.TabPgDriver.TabIndex = 7
        Me.TabPgDriver.Text = "Driver Assist"
        Me.TabPgDriver.UseVisualStyleBackColor = True
        '
        'GrVACC
        '
        Me.GrVACC.Controls.Add(Me.TbDesMaxFile)
        Me.GrVACC.Controls.Add(Me.BtDesMaxBr)
        Me.GrVACC.Controls.Add(Me.BtAccOpen)
        Me.GrVACC.Location = New System.Drawing.Point(6, 401)
        Me.GrVACC.Name = "GrVACC"
        Me.GrVACC.Size = New System.Drawing.Size(515, 84)
        Me.GrVACC.TabIndex = 34
        Me.GrVACC.TabStop = False
        Me.GrVACC.Text = "Max. acceleration and brake curves"
        '
        'TbDesMaxFile
        '
        Me.TbDesMaxFile.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TbDesMaxFile.Location = New System.Drawing.Point(6, 29)
        Me.TbDesMaxFile.Name = "TbDesMaxFile"
        Me.TbDesMaxFile.Size = New System.Drawing.Size(440, 20)
        Me.TbDesMaxFile.TabIndex = 0
        '
        'BtDesMaxBr
        '
        Me.BtDesMaxBr.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtDesMaxBr.Location = New System.Drawing.Point(452, 28)
        Me.BtDesMaxBr.Name = "BtDesMaxBr"
        Me.BtDesMaxBr.Size = New System.Drawing.Size(28, 21)
        Me.BtDesMaxBr.TabIndex = 1
        Me.BtDesMaxBr.Text = "..."
        Me.BtDesMaxBr.UseVisualStyleBackColor = True
        '
        'BtAccOpen
        '
        Me.BtAccOpen.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtAccOpen.Image = Global.VECTO.My.Resources.Resources.application_export_icon_small
        Me.BtAccOpen.Location = New System.Drawing.Point(486, 27)
        Me.BtAccOpen.Name = "BtAccOpen"
        Me.BtAccOpen.Size = New System.Drawing.Size(23, 23)
        Me.BtAccOpen.TabIndex = 2
        Me.BtAccOpen.TabStop = False
        Me.BtAccOpen.UseVisualStyleBackColor = True
        '
        'GrLAC
        '
        Me.GrLAC.Controls.Add(Me.PnLookAhead)
        Me.GrLAC.Controls.Add(Me.CbLookAhead)
        Me.GrLAC.Location = New System.Drawing.Point(7, 290)
        Me.GrLAC.Name = "GrLAC"
        Me.GrLAC.Size = New System.Drawing.Size(514, 105)
        Me.GrLAC.TabIndex = 5
        Me.GrLAC.TabStop = False
        Me.GrLAC.Text = "Look-Ahead Coasting"
        '
        'PnLookAhead
        '
        Me.PnLookAhead.Controls.Add(Me.Label29)
        Me.PnLookAhead.Controls.Add(Me.Label25)
        Me.PnLookAhead.Controls.Add(Me.TbAlookahead)
        Me.PnLookAhead.Controls.Add(Me.Label24)
        Me.PnLookAhead.Controls.Add(Me.TbVminLA)
        Me.PnLookAhead.Controls.Add(Me.Label28)
        Me.PnLookAhead.Location = New System.Drawing.Point(117, 10)
        Me.PnLookAhead.Name = "PnLookAhead"
        Me.PnLookAhead.Size = New System.Drawing.Size(252, 71)
        Me.PnLookAhead.TabIndex = 1
        '
        'Label29
        '
        Me.Label29.AutoSize = True
        Me.Label29.Location = New System.Drawing.Point(197, 38)
        Me.Label29.Name = "Label29"
        Me.Label29.Size = New System.Drawing.Size(38, 13)
        Me.Label29.TabIndex = 3
        Me.Label29.Text = "[km/h]"
        '
        'Label25
        '
        Me.Label25.AutoSize = True
        Me.Label25.Location = New System.Drawing.Point(197, 12)
        Me.Label25.Name = "Label25"
        Me.Label25.Size = New System.Drawing.Size(34, 13)
        Me.Label25.TabIndex = 3
        Me.Label25.Text = "[m/s²]"
        '
        'TbAlookahead
        '
        Me.TbAlookahead.Location = New System.Drawing.Point(127, 9)
        Me.TbAlookahead.Name = "TbAlookahead"
        Me.TbAlookahead.Size = New System.Drawing.Size(64, 20)
        Me.TbAlookahead.TabIndex = 0
        '
        'Label24
        '
        Me.Label24.AutoSize = True
        Me.Label24.Location = New System.Drawing.Point(25, 12)
        Me.Label24.Name = "Label24"
        Me.Label24.Size = New System.Drawing.Size(96, 13)
        Me.Label24.TabIndex = 1
        Me.Label24.Text = "Target Retardation"
        '
        'TbVminLA
        '
        Me.TbVminLA.Location = New System.Drawing.Point(127, 35)
        Me.TbVminLA.Name = "TbVminLA"
        Me.TbVminLA.Size = New System.Drawing.Size(64, 20)
        Me.TbVminLA.TabIndex = 1
        '
        'Label28
        '
        Me.Label28.AutoSize = True
        Me.Label28.Location = New System.Drawing.Point(41, 38)
        Me.Label28.Name = "Label28"
        Me.Label28.Size = New System.Drawing.Size(80, 13)
        Me.Label28.TabIndex = 1
        Me.Label28.Text = "Minimum speed"
        '
        'CbLookAhead
        '
        Me.CbLookAhead.AutoSize = True
        Me.CbLookAhead.Checked = True
        Me.CbLookAhead.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CbLookAhead.Location = New System.Drawing.Point(16, 21)
        Me.CbLookAhead.Name = "CbLookAhead"
        Me.CbLookAhead.Size = New System.Drawing.Size(65, 17)
        Me.CbLookAhead.TabIndex = 0
        Me.CbLookAhead.Text = "Enabled"
        Me.CbLookAhead.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.PnEcoRoll)
        Me.GroupBox1.Controls.Add(Me.RdEcoRoll)
        Me.GroupBox1.Controls.Add(Me.RdOverspeed)
        Me.GroupBox1.Controls.Add(Me.RdOff)
        Me.GroupBox1.Location = New System.Drawing.Point(6, 149)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(515, 135)
        Me.GroupBox1.TabIndex = 4
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Overspeed / Eco-Roll"
        '
        'PnEcoRoll
        '
        Me.PnEcoRoll.Controls.Add(Me.Label21)
        Me.PnEcoRoll.Controls.Add(Me.Label20)
        Me.PnEcoRoll.Controls.Add(Me.Label14)
        Me.PnEcoRoll.Controls.Add(Me.TbVmin)
        Me.PnEcoRoll.Controls.Add(Me.TbUnderSpeed)
        Me.PnEcoRoll.Controls.Add(Me.TbOverspeed)
        Me.PnEcoRoll.Controls.Add(Me.Label23)
        Me.PnEcoRoll.Controls.Add(Me.Label22)
        Me.PnEcoRoll.Controls.Add(Me.Label13)
        Me.PnEcoRoll.Location = New System.Drawing.Point(137, 16)
        Me.PnEcoRoll.Name = "PnEcoRoll"
        Me.PnEcoRoll.Size = New System.Drawing.Size(232, 102)
        Me.PnEcoRoll.TabIndex = 6
        '
        'Label21
        '
        Me.Label21.AutoSize = True
        Me.Label21.Location = New System.Drawing.Point(178, 61)
        Me.Label21.Name = "Label21"
        Me.Label21.Size = New System.Drawing.Size(38, 13)
        Me.Label21.TabIndex = 3
        Me.Label21.Text = "[km/h]"
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(178, 35)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(38, 13)
        Me.Label20.TabIndex = 3
        Me.Label20.Text = "[km/h]"
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Location = New System.Drawing.Point(178, 9)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(38, 13)
        Me.Label14.TabIndex = 3
        Me.Label14.Text = "[km/h]"
        '
        'TbVmin
        '
        Me.TbVmin.Location = New System.Drawing.Point(108, 58)
        Me.TbVmin.Name = "TbVmin"
        Me.TbVmin.Size = New System.Drawing.Size(64, 20)
        Me.TbVmin.TabIndex = 5
        '
        'TbUnderSpeed
        '
        Me.TbUnderSpeed.Location = New System.Drawing.Point(108, 32)
        Me.TbUnderSpeed.Name = "TbUnderSpeed"
        Me.TbUnderSpeed.Size = New System.Drawing.Size(64, 20)
        Me.TbUnderSpeed.TabIndex = 4
        '
        'TbOverspeed
        '
        Me.TbOverspeed.Location = New System.Drawing.Point(108, 6)
        Me.TbOverspeed.Name = "TbOverspeed"
        Me.TbOverspeed.Size = New System.Drawing.Size(64, 20)
        Me.TbOverspeed.TabIndex = 3
        '
        'Label23
        '
        Me.Label23.AutoSize = True
        Me.Label23.Location = New System.Drawing.Point(22, 61)
        Me.Label23.Name = "Label23"
        Me.Label23.Size = New System.Drawing.Size(80, 13)
        Me.Label23.TabIndex = 1
        Me.Label23.Text = "Minimum speed"
        '
        'Label22
        '
        Me.Label22.AutoSize = True
        Me.Label22.Location = New System.Drawing.Point(11, 35)
        Me.Label22.Name = "Label22"
        Me.Label22.Size = New System.Drawing.Size(91, 13)
        Me.Label22.TabIndex = 1
        Me.Label22.Text = "Max. Underspeed"
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(17, 9)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(85, 13)
        Me.Label13.TabIndex = 1
        Me.Label13.Text = "Max. Overspeed"
        '
        'RdEcoRoll
        '
        Me.RdEcoRoll.AutoSize = True
        Me.RdEcoRoll.Checked = True
        Me.RdEcoRoll.Location = New System.Drawing.Point(13, 68)
        Me.RdEcoRoll.Name = "RdEcoRoll"
        Me.RdEcoRoll.Size = New System.Drawing.Size(65, 17)
        Me.RdEcoRoll.TabIndex = 2
        Me.RdEcoRoll.TabStop = True
        Me.RdEcoRoll.Text = "Eco-Roll"
        Me.RdEcoRoll.UseVisualStyleBackColor = True
        '
        'RdOverspeed
        '
        Me.RdOverspeed.AutoSize = True
        Me.RdOverspeed.Location = New System.Drawing.Point(13, 45)
        Me.RdOverspeed.Name = "RdOverspeed"
        Me.RdOverspeed.Size = New System.Drawing.Size(77, 17)
        Me.RdOverspeed.TabIndex = 1
        Me.RdOverspeed.Text = "Overspeed"
        Me.RdOverspeed.UseVisualStyleBackColor = True
        '
        'RdOff
        '
        Me.RdOff.AutoSize = True
        Me.RdOff.Location = New System.Drawing.Point(13, 22)
        Me.RdOff.Name = "RdOff"
        Me.RdOff.Size = New System.Drawing.Size(39, 17)
        Me.RdOff.TabIndex = 0
        Me.RdOff.Text = "Off"
        Me.RdOff.UseVisualStyleBackColor = True
        '
        'GrStartStop
        '
        Me.GrStartStop.Controls.Add(Me.PnStartStop)
        Me.GrStartStop.Controls.Add(Me.ChBStartStop)
        Me.GrStartStop.Location = New System.Drawing.Point(6, 6)
        Me.GrStartStop.Name = "GrStartStop"
        Me.GrStartStop.Size = New System.Drawing.Size(515, 137)
        Me.GrStartStop.TabIndex = 3
        Me.GrStartStop.TabStop = False
        Me.GrStartStop.Text = "Engine Start Stop"
        '
        'PnStartStop
        '
        Me.PnStartStop.Controls.Add(Me.Label31)
        Me.PnStartStop.Controls.Add(Me.Label27)
        Me.PnStartStop.Controls.Add(Me.TbSSspeed)
        Me.PnStartStop.Controls.Add(Me.LabelSSspeed)
        Me.PnStartStop.Controls.Add(Me.Label26)
        Me.PnStartStop.Controls.Add(Me.Label30)
        Me.PnStartStop.Controls.Add(Me.LabelSStime)
        Me.PnStartStop.Controls.Add(Me.TbSSdelay)
        Me.PnStartStop.Controls.Add(Me.TbSStime)
        Me.PnStartStop.Location = New System.Drawing.Point(87, 21)
        Me.PnStartStop.Name = "PnStartStop"
        Me.PnStartStop.Size = New System.Drawing.Size(422, 95)
        Me.PnStartStop.TabIndex = 1
        '
        'Label31
        '
        Me.Label31.AutoSize = True
        Me.Label31.Location = New System.Drawing.Point(228, 58)
        Me.Label31.Name = "Label31"
        Me.Label31.Size = New System.Drawing.Size(18, 13)
        Me.Label31.TabIndex = 38
        Me.Label31.Text = "[s]"
        '
        'Label27
        '
        Me.Label27.AutoSize = True
        Me.Label27.Location = New System.Drawing.Point(228, 32)
        Me.Label27.Name = "Label27"
        Me.Label27.Size = New System.Drawing.Size(18, 13)
        Me.Label27.TabIndex = 38
        Me.Label27.Text = "[s]"
        '
        'TbSSspeed
        '
        Me.TbSSspeed.Location = New System.Drawing.Point(158, 3)
        Me.TbSSspeed.Name = "TbSSspeed"
        Me.TbSSspeed.Size = New System.Drawing.Size(64, 20)
        Me.TbSSspeed.TabIndex = 0
        '
        'LabelSSspeed
        '
        Me.LabelSSspeed.AutoSize = True
        Me.LabelSSspeed.Location = New System.Drawing.Point(91, 6)
        Me.LabelSSspeed.Name = "LabelSSspeed"
        Me.LabelSSspeed.Size = New System.Drawing.Size(61, 13)
        Me.LabelSSspeed.TabIndex = 37
        Me.LabelSSspeed.Text = "Max Speed"
        '
        'Label26
        '
        Me.Label26.AutoSize = True
        Me.Label26.Location = New System.Drawing.Point(228, 6)
        Me.Label26.Name = "Label26"
        Me.Label26.Size = New System.Drawing.Size(38, 13)
        Me.Label26.TabIndex = 38
        Me.Label26.Text = "[km/h]"
        '
        'Label30
        '
        Me.Label30.AutoSize = True
        Me.Label30.Location = New System.Drawing.Point(68, 58)
        Me.Label30.Name = "Label30"
        Me.Label30.Size = New System.Drawing.Size(84, 13)
        Me.Label30.TabIndex = 35
        Me.Label30.Text = "Activation Delay"
        '
        'LabelSStime
        '
        Me.LabelSStime.AutoSize = True
        Me.LabelSStime.Location = New System.Drawing.Point(65, 32)
        Me.LabelSStime.Name = "LabelSStime"
        Me.LabelSStime.Size = New System.Drawing.Size(87, 13)
        Me.LabelSStime.TabIndex = 35
        Me.LabelSStime.Text = "Min ICE-On Time"
        '
        'TbSSdelay
        '
        Me.TbSSdelay.Location = New System.Drawing.Point(158, 55)
        Me.TbSSdelay.Name = "TbSSdelay"
        Me.TbSSdelay.Size = New System.Drawing.Size(64, 20)
        Me.TbSSdelay.TabIndex = 1
        '
        'TbSStime
        '
        Me.TbSStime.Location = New System.Drawing.Point(158, 29)
        Me.TbSStime.Name = "TbSStime"
        Me.TbSStime.Size = New System.Drawing.Size(64, 20)
        Me.TbSStime.TabIndex = 1
        '
        'ChBStartStop
        '
        Me.ChBStartStop.AutoSize = True
        Me.ChBStartStop.Checked = True
        Me.ChBStartStop.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ChBStartStop.Location = New System.Drawing.Point(16, 21)
        Me.ChBStartStop.Name = "ChBStartStop"
        Me.ChBStartStop.Size = New System.Drawing.Size(65, 17)
        Me.ChBStartStop.TabIndex = 0
        Me.ChBStartStop.Text = "Enabled"
        Me.ChBStartStop.UseVisualStyleBackColor = True
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripStatusLabelGEN})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 627)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(944, 22)
        Me.StatusStrip1.SizingGrip = False
        Me.StatusStrip1.TabIndex = 6
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'ToolStripStatusLabelGEN
        '
        Me.ToolStripStatusLabelGEN.Name = "ToolStripStatusLabelGEN"
        Me.ToolStripStatusLabelGEN.Size = New System.Drawing.Size(121, 17)
        Me.ToolStripStatusLabelGEN.Text = "ToolStripStatusLabel1"
        '
        'ButOK
        '
        Me.ButOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButOK.Location = New System.Drawing.Point(779, 601)
        Me.ButOK.Name = "ButOK"
        Me.ButOK.Size = New System.Drawing.Size(75, 23)
        Me.ButOK.TabIndex = 11
        Me.ButOK.Text = "OK"
        Me.ButOK.UseVisualStyleBackColor = True
        '
        'ButCancel
        '
        Me.ButCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ButCancel.Location = New System.Drawing.Point(860, 601)
        Me.ButCancel.Name = "ButCancel"
        Me.ButCancel.Size = New System.Drawing.Size(75, 23)
        Me.ButCancel.TabIndex = 19
        Me.ButCancel.Text = "Cancel"
        Me.ButCancel.UseVisualStyleBackColor = True
        '
        'ToolStrip1
        '
        Me.ToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripBtNew, Me.ToolStripBtOpen, Me.ToolStripBtSave, Me.ToolStripBtSaveAs, Me.ToolStripSeparator1, Me.ToolStripBtSendTo, Me.ToolStripSeparator2, Me.ToolStripButton1})
        Me.ToolStrip1.Location = New System.Drawing.Point(0, 0)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(944, 25)
        Me.ToolStrip1.TabIndex = 20
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'ToolStripBtNew
        '
        Me.ToolStripBtNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripBtNew.Image = Global.VECTO.My.Resources.Resources.blue_document_icon
        Me.ToolStripBtNew.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripBtNew.Name = "ToolStripBtNew"
        Me.ToolStripBtNew.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripBtNew.Text = "New"
        Me.ToolStripBtNew.ToolTipText = "New"
        '
        'ToolStripBtOpen
        '
        Me.ToolStripBtOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripBtOpen.Image = Global.VECTO.My.Resources.Resources.Open_icon
        Me.ToolStripBtOpen.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripBtOpen.Name = "ToolStripBtOpen"
        Me.ToolStripBtOpen.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripBtOpen.Text = "Open"
        Me.ToolStripBtOpen.ToolTipText = "Open..."
        '
        'ToolStripBtSave
        '
        Me.ToolStripBtSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripBtSave.Image = Global.VECTO.My.Resources.Resources.Actions_document_save_icon
        Me.ToolStripBtSave.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripBtSave.Name = "ToolStripBtSave"
        Me.ToolStripBtSave.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripBtSave.Text = "Save"
        Me.ToolStripBtSave.ToolTipText = "Save"
        '
        'ToolStripBtSaveAs
        '
        Me.ToolStripBtSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripBtSaveAs.Image = Global.VECTO.My.Resources.Resources.Actions_document_save_as_icon
        Me.ToolStripBtSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripBtSaveAs.Name = "ToolStripBtSaveAs"
        Me.ToolStripBtSaveAs.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripBtSaveAs.Text = "Save As"
        Me.ToolStripBtSaveAs.ToolTipText = "Save As..."
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 25)
        '
        'ToolStripBtSendTo
        '
        Me.ToolStripBtSendTo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripBtSendTo.Image = Global.VECTO.My.Resources.Resources.export_icon
        Me.ToolStripBtSendTo.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripBtSendTo.Name = "ToolStripBtSendTo"
        Me.ToolStripBtSendTo.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripBtSendTo.Text = "Send to Job List"
        Me.ToolStripBtSendTo.ToolTipText = "Send to Job List"
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        Me.ToolStripSeparator2.Size = New System.Drawing.Size(6, 25)
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
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.White
        Me.PictureBox1.Image = Global.VECTO.My.Resources.Resources.VECTO_VECTO
        Me.PictureBox1.Location = New System.Drawing.Point(12, 28)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(920, 40)
        Me.PictureBox1.TabIndex = 21
        Me.PictureBox1.TabStop = False
        '
        'CbEngOnly
        '
        Me.CbEngOnly.AutoSize = True
        Me.CbEngOnly.Location = New System.Drawing.Point(17, 84)
        Me.CbEngOnly.Name = "CbEngOnly"
        Me.CbEngOnly.Size = New System.Drawing.Size(113, 17)
        Me.CbEngOnly.TabIndex = 35
        Me.CbEngOnly.Text = "Engine Only Mode"
        Me.CbEngOnly.UseVisualStyleBackColor = True
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
        'PicVehicle
        '
        Me.PicVehicle.BackColor = System.Drawing.Color.LightGray
        Me.PicVehicle.Location = New System.Drawing.Point(542, 122)
        Me.PicVehicle.Name = "PicVehicle"
        Me.PicVehicle.Size = New System.Drawing.Size(300, 88)
        Me.PicVehicle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PicVehicle.TabIndex = 36
        Me.PicVehicle.TabStop = False
        '
        'PicBox
        '
        Me.PicBox.BackColor = System.Drawing.Color.LightGray
        Me.PicBox.Location = New System.Drawing.Point(542, 268)
        Me.PicBox.Name = "PicBox"
        Me.PicBox.Size = New System.Drawing.Size(390, 327)
        Me.PicBox.TabIndex = 36
        Me.PicBox.TabStop = False
        '
        'TbEngTxt
        '
        Me.TbEngTxt.Location = New System.Drawing.Point(542, 216)
        Me.TbEngTxt.Name = "TbEngTxt"
        Me.TbEngTxt.ReadOnly = True
        Me.TbEngTxt.Size = New System.Drawing.Size(390, 20)
        Me.TbEngTxt.TabIndex = 37
        '
        'TbVehCat
        '
        Me.TbVehCat.Location = New System.Drawing.Point(848, 126)
        Me.TbVehCat.Name = "TbVehCat"
        Me.TbVehCat.ReadOnly = True
        Me.TbVehCat.Size = New System.Drawing.Size(87, 20)
        Me.TbVehCat.TabIndex = 37
        '
        'TbAxleConf
        '
        Me.TbAxleConf.Location = New System.Drawing.Point(894, 155)
        Me.TbAxleConf.Name = "TbAxleConf"
        Me.TbAxleConf.ReadOnly = True
        Me.TbAxleConf.Size = New System.Drawing.Size(41, 20)
        Me.TbAxleConf.TabIndex = 37
        '
        'TbHVCclass
        '
        Me.TbHVCclass.Location = New System.Drawing.Point(848, 184)
        Me.TbHVCclass.Name = "TbHVCclass"
        Me.TbHVCclass.ReadOnly = True
        Me.TbHVCclass.Size = New System.Drawing.Size(87, 20)
        Me.TbHVCclass.TabIndex = 37
        '
        'TbGbxTxt
        '
        Me.TbGbxTxt.Location = New System.Drawing.Point(542, 242)
        Me.TbGbxTxt.Name = "TbGbxTxt"
        Me.TbGbxTxt.ReadOnly = True
        Me.TbGbxTxt.Size = New System.Drawing.Size(390, 20)
        Me.TbGbxTxt.TabIndex = 37
        '
        'TbMass
        '
        Me.TbMass.Location = New System.Drawing.Point(848, 155)
        Me.TbMass.Name = "TbMass"
        Me.TbMass.ReadOnly = True
        Me.TbMass.Size = New System.Drawing.Size(40, 20)
        Me.TbMass.TabIndex = 37
        '
        'F_VECTO
        '
        Me.AcceptButton = Me.ButOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ButCancel
        Me.ClientSize = New System.Drawing.Size(944, 649)
        Me.Controls.Add(Me.TbHVCclass)
        Me.Controls.Add(Me.TbMass)
        Me.Controls.Add(Me.TbAxleConf)
        Me.Controls.Add(Me.TbVehCat)
        Me.Controls.Add(Me.TbGbxTxt)
        Me.Controls.Add(Me.TbEngTxt)
        Me.Controls.Add(Me.PicBox)
        Me.Controls.Add(Me.PicVehicle)
        Me.Controls.Add(Me.CbEngOnly)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Controls.Add(Me.ButCancel)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.ButOK)
        Me.Controls.Add(Me.StatusStrip1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "F_VECTO"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "VECTO Editor"
        Me.TabPgGen.ResumeLayout(False)
        Me.TabPgGen.PerformLayout()
        Me.GrCycles.ResumeLayout(False)
        Me.GrCycles.PerformLayout()
        Me.GrAux.ResumeLayout(False)
        Me.GrAux.PerformLayout()
        Me.TabControl1.ResumeLayout(False)
        Me.TabPgDriver.ResumeLayout(False)
        Me.GrVACC.ResumeLayout(False)
        Me.GrVACC.PerformLayout()
        Me.GrLAC.ResumeLayout(False)
        Me.GrLAC.PerformLayout()
        Me.PnLookAhead.ResumeLayout(False)
        Me.PnLookAhead.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.PnEcoRoll.ResumeLayout(False)
        Me.PnEcoRoll.PerformLayout()
        Me.GrStartStop.ResumeLayout(False)
        Me.GrStartStop.PerformLayout()
        Me.PnStartStop.ResumeLayout(False)
        Me.PnStartStop.PerformLayout()
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.CmOpenFile.ResumeLayout(False)
        CType(Me.PicVehicle, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PicBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TabPgGen As System.Windows.Forms.TabPage
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents ButtonVEH As System.Windows.Forms.Button
    Friend WithEvents ButtonMAP As System.Windows.Forms.Button
    Friend WithEvents ButtonGBX As System.Windows.Forms.Button
    Friend WithEvents ButOpenVEH As System.Windows.Forms.Button
    Friend WithEvents ButOpenGBX As System.Windows.Forms.Button
    Friend WithEvents ButOpenENG As System.Windows.Forms.Button
    Friend WithEvents ToolStripStatusLabelGEN As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents ButOK As System.Windows.Forms.Button
    Friend WithEvents TbGBX As System.Windows.Forms.TextBox
    Friend WithEvents TbENG As System.Windows.Forms.TextBox
    Friend WithEvents TbVEH As System.Windows.Forms.TextBox
    Friend WithEvents ButCancel As System.Windows.Forms.Button
    Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripBtNew As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripBtOpen As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripBtSave As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripBtSaveAs As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripBtSendTo As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents GrAux As System.Windows.Forms.GroupBox
    Friend WithEvents LvAux As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader5 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader6 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ButAuxRem As System.Windows.Forms.Button
    Friend WithEvents ButAuxAdd As System.Windows.Forms.Button
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents TabPgDriver As System.Windows.Forms.TabPage
    Friend WithEvents BtDesMaxBr As System.Windows.Forms.Button
    Friend WithEvents TbDesMaxFile As System.Windows.Forms.TextBox
    Friend WithEvents GrCycles As System.Windows.Forms.GroupBox
    Friend WithEvents LvCycles As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents BtDRIrem As System.Windows.Forms.Button
    Friend WithEvents BtDRIadd As System.Windows.Forms.Button
    Friend WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Friend WithEvents CbEngOnly As System.Windows.Forms.CheckBox
    Friend WithEvents BtAccOpen As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents CmOpenFile As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents OpenWithToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ShowInFolderToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ChBStartStop As System.Windows.Forms.CheckBox
    Friend WithEvents GrStartStop As System.Windows.Forms.GroupBox
    Friend WithEvents TbSSspeed As System.Windows.Forms.TextBox
    Friend WithEvents TbSStime As System.Windows.Forms.TextBox
    Friend WithEvents LabelSStime As System.Windows.Forms.Label
    Friend WithEvents LabelSSspeed As System.Windows.Forms.Label
    Friend WithEvents GrLAC As System.Windows.Forms.GroupBox
    Friend WithEvents Label25 As System.Windows.Forms.Label
    Friend WithEvents CbLookAhead As System.Windows.Forms.CheckBox
    Friend WithEvents TbAlookahead As System.Windows.Forms.TextBox
    Friend WithEvents Label24 As System.Windows.Forms.Label
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label21 As System.Windows.Forms.Label
    Friend WithEvents Label20 As System.Windows.Forms.Label
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents TbVmin As System.Windows.Forms.TextBox
    Friend WithEvents TbUnderSpeed As System.Windows.Forms.TextBox
    Friend WithEvents TbOverspeed As System.Windows.Forms.TextBox
    Friend WithEvents Label23 As System.Windows.Forms.Label
    Friend WithEvents Label22 As System.Windows.Forms.Label
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents RdEcoRoll As System.Windows.Forms.RadioButton
    Friend WithEvents RdOverspeed As System.Windows.Forms.RadioButton
    Friend WithEvents RdOff As System.Windows.Forms.RadioButton
    Friend WithEvents PnStartStop As System.Windows.Forms.Panel
    Friend WithEvents Label27 As System.Windows.Forms.Label
    Friend WithEvents Label26 As System.Windows.Forms.Label
    Friend WithEvents PnLookAhead As System.Windows.Forms.Panel
    Friend WithEvents Label29 As System.Windows.Forms.Label
    Friend WithEvents TbVminLA As System.Windows.Forms.TextBox
    Friend WithEvents Label28 As System.Windows.Forms.Label
    Friend WithEvents Label31 As System.Windows.Forms.Label
    Friend WithEvents Label30 As System.Windows.Forms.Label
    Friend WithEvents TbSSdelay As System.Windows.Forms.TextBox
    Friend WithEvents Label32 As System.Windows.Forms.Label
    Friend WithEvents PnEcoRoll As System.Windows.Forms.Panel
    Friend WithEvents PicVehicle As System.Windows.Forms.PictureBox
    Friend WithEvents PicBox As System.Windows.Forms.PictureBox
    Friend WithEvents TbEngTxt As System.Windows.Forms.TextBox
    Friend WithEvents TbVehCat As System.Windows.Forms.TextBox
    Friend WithEvents TbAxleConf As System.Windows.Forms.TextBox
    Friend WithEvents TbHVCclass As System.Windows.Forms.TextBox
    Friend WithEvents TbGbxTxt As System.Windows.Forms.TextBox
    Friend WithEvents TbMass As System.Windows.Forms.TextBox
    Friend WithEvents GrVACC As System.Windows.Forms.GroupBox
End Class
