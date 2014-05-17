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
Partial Class F_MAINForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(F_MAINForm))
        Me.StatusBAR = New System.Windows.Forms.StatusStrip()
        Me.ToolStripLbStatus = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ToolStripProgBarJob = New System.Windows.Forms.ToolStripProgressBar()
        Me.ToolStripProgBarOverall = New System.Windows.Forms.ToolStripProgressBar()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPageGEN = New System.Windows.Forms.TabPage()
        Me.LbDecl = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.BtGENdown = New System.Windows.Forms.Button()
        Me.BtGENup = New System.Windows.Forms.Button()
        Me.LbAutoShDown = New System.Windows.Forms.Label()
        Me.ChBoxAllGEN = New System.Windows.Forms.CheckBox()
        Me.LvGEN = New System.Windows.Forms.ListView()
        Me.ColGENpath = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColGENstatus = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ButtonGENopt = New System.Windows.Forms.Button()
        Me.ButtonGENremove = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.ButtonGENadd = New System.Windows.Forms.Button()
        Me.TabPageDRI = New System.Windows.Forms.TabPage()
        Me.BtDRIdown = New System.Windows.Forms.Button()
        Me.BtDRIup = New System.Windows.Forms.Button()
        Me.ChBoxAllDRI = New System.Windows.Forms.CheckBox()
        Me.LvDRI = New System.Windows.Forms.ListView()
        Me.ColDRIpath = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColDRIstatus = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ButtonDRIedit = New System.Windows.Forms.Button()
        Me.ButtonDRIremove = New System.Windows.Forms.Button()
        Me.ButtonDRIadd = New System.Windows.Forms.Button()
        Me.TabPgOptions = New System.Windows.Forms.TabPage()
        Me.GrBoxSTD = New System.Windows.Forms.GroupBox()
        Me.GrBoxBATCH = New System.Windows.Forms.GroupBox()
        Me.ChBoxBatchSubD = New System.Windows.Forms.CheckBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.ButBObrowse = New System.Windows.Forms.Button()
        Me.CbBOmode = New System.Windows.Forms.ComboBox()
        Me.TbBOpath = New System.Windows.Forms.TextBox()
        Me.ChBoxAutoSD = New System.Windows.Forms.CheckBox()
        Me.PanelOptAllg = New System.Windows.Forms.Panel()
        Me.CbDecl = New System.Windows.Forms.CheckBox()
        Me.PnDeclOpt = New System.Windows.Forms.Panel()
        Me.CbBatch = New System.Windows.Forms.CheckBox()
        Me.ChBoxCyclDistCor = New System.Windows.Forms.CheckBox()
        Me.ChBoxUseGears = New System.Windows.Forms.CheckBox()
        Me.ChBoxModOut = New System.Windows.Forms.CheckBox()
        Me.TabPageDEV = New System.Windows.Forms.TabPage()
        Me.LvDEVoptions = New System.Windows.Forms.ListView()
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader7 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader6 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader8 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader9 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ConMenFilelist = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.SaveListToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LoadListToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LoadDefaultListToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ClearListToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
        Me.LvMsg = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.ToolStripBtNew = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripBtOpen = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripDrDnBtTools = New System.Windows.Forms.ToolStripDropDownButton()
        Me.GENEditorToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.VEHEditorToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.EngineEditorToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.GearboxEditorToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.GraphToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator6 = New System.Windows.Forms.ToolStripSeparator()
        Me.SignOrVerifyFilesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator4 = New System.Windows.Forms.ToolStripSeparator()
        Me.OpenLogToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SettingsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripDrDnBtInfo = New System.Windows.Forms.ToolStripDropDownButton()
        Me.UserManualToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UpdateNotesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator()
        Me.CreateActivationFileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AboutVECTOToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.CmDEV = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.TmProgSec = New System.Windows.Forms.Timer(Me.components)
        Me.CmOpenFile = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.OpenWithToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.OpenInGraphWindowToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ShowInFolderToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.StatusBAR.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.TabPageGEN.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPageDRI.SuspendLayout()
        Me.TabPgOptions.SuspendLayout()
        Me.GrBoxBATCH.SuspendLayout()
        Me.PanelOptAllg.SuspendLayout()
        Me.PnDeclOpt.SuspendLayout()
        Me.TabPageDEV.SuspendLayout()
        Me.ConMenFilelist.SuspendLayout()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.ToolStrip1.SuspendLayout()
        Me.CmOpenFile.SuspendLayout()
        Me.SuspendLayout()
        '
        'StatusBAR
        '
        Me.StatusBAR.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripLbStatus, Me.ToolStripProgBarJob, Me.ToolStripProgBarOverall})
        Me.StatusBAR.Location = New System.Drawing.Point(0, 616)
        Me.StatusBAR.Name = "StatusBAR"
        Me.StatusBAR.Size = New System.Drawing.Size(1136, 22)
        Me.StatusBAR.TabIndex = 7
        Me.StatusBAR.Text = "StatusBAR"
        '
        'ToolStripLbStatus
        '
        Me.ToolStripLbStatus.Name = "ToolStripLbStatus"
        Me.ToolStripLbStatus.Size = New System.Drawing.Size(1121, 17)
        Me.ToolStripLbStatus.Spring = True
        Me.ToolStripLbStatus.Text = "Status Text"
        Me.ToolStripLbStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ToolStripProgBarJob
        '
        Me.ToolStripProgBarJob.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.ToolStripProgBarJob.AutoSize = False
        Me.ToolStripProgBarJob.Name = "ToolStripProgBarJob"
        Me.ToolStripProgBarJob.Size = New System.Drawing.Size(100, 16)
        Me.ToolStripProgBarJob.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        Me.ToolStripProgBarJob.ToolTipText = "overall progress"
        Me.ToolStripProgBarJob.Visible = False
        '
        'ToolStripProgBarOverall
        '
        Me.ToolStripProgBarOverall.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.ToolStripProgBarOverall.AutoSize = False
        Me.ToolStripProgBarOverall.Name = "ToolStripProgBarOverall"
        Me.ToolStripProgBarOverall.Size = New System.Drawing.Size(100, 16)
        Me.ToolStripProgBarOverall.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        Me.ToolStripProgBarOverall.ToolTipText = "job progress"
        Me.ToolStripProgBarOverall.Visible = False
        '
        'TabControl1
        '
        Me.TabControl1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TabControl1.Controls.Add(Me.TabPageGEN)
        Me.TabControl1.Controls.Add(Me.TabPageDRI)
        Me.TabControl1.Controls.Add(Me.TabPgOptions)
        Me.TabControl1.Controls.Add(Me.TabPageDEV)
        Me.TabControl1.Location = New System.Drawing.Point(3, 3)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(1126, 372)
        Me.TabControl1.TabIndex = 10
        '
        'TabPageGEN
        '
        Me.TabPageGEN.Controls.Add(Me.LbDecl)
        Me.TabPageGEN.Controls.Add(Me.PictureBox1)
        Me.TabPageGEN.Controls.Add(Me.BtGENdown)
        Me.TabPageGEN.Controls.Add(Me.BtGENup)
        Me.TabPageGEN.Controls.Add(Me.LbAutoShDown)
        Me.TabPageGEN.Controls.Add(Me.ChBoxAllGEN)
        Me.TabPageGEN.Controls.Add(Me.LvGEN)
        Me.TabPageGEN.Controls.Add(Me.ButtonGENopt)
        Me.TabPageGEN.Controls.Add(Me.ButtonGENremove)
        Me.TabPageGEN.Controls.Add(Me.Button1)
        Me.TabPageGEN.Controls.Add(Me.ButtonGENadd)
        Me.TabPageGEN.Location = New System.Drawing.Point(4, 22)
        Me.TabPageGEN.Name = "TabPageGEN"
        Me.TabPageGEN.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageGEN.Size = New System.Drawing.Size(1118, 346)
        Me.TabPageGEN.TabIndex = 0
        Me.TabPageGEN.Text = "Job Files"
        Me.TabPageGEN.UseVisualStyleBackColor = True
        '
        'LbDecl
        '
        Me.LbDecl.AutoSize = True
        Me.LbDecl.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LbDecl.Location = New System.Drawing.Point(6, 97)
        Me.LbDecl.Name = "LbDecl"
        Me.LbDecl.Size = New System.Drawing.Size(107, 13)
        Me.LbDecl.TabIndex = 19
        Me.LbDecl.Text = "Declaration Mode"
        Me.LbDecl.Visible = False
        '
        'PictureBox1
        '
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(6, 6)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(105, 44)
        Me.PictureBox1.TabIndex = 18
        Me.PictureBox1.TabStop = False
        '
        'BtGENdown
        '
        Me.BtGENdown.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtGENdown.Image = Global.VECTO.My.Resources.Resources.Actions_arrow_down_icon
        Me.BtGENdown.Location = New System.Drawing.Point(996, 317)
        Me.BtGENdown.Name = "BtGENdown"
        Me.BtGENdown.Size = New System.Drawing.Size(30, 23)
        Me.BtGENdown.TabIndex = 6
        Me.BtGENdown.UseVisualStyleBackColor = True
        '
        'BtGENup
        '
        Me.BtGENup.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtGENup.Image = Global.VECTO.My.Resources.Resources.Actions_arrow_up_icon
        Me.BtGENup.Location = New System.Drawing.Point(960, 317)
        Me.BtGENup.Name = "BtGENup"
        Me.BtGENup.Size = New System.Drawing.Size(30, 23)
        Me.BtGENup.TabIndex = 4
        Me.BtGENup.UseVisualStyleBackColor = True
        '
        'LbAutoShDown
        '
        Me.LbAutoShDown.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LbAutoShDown.AutoSize = True
        Me.LbAutoShDown.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LbAutoShDown.ForeColor = System.Drawing.Color.Red
        Me.LbAutoShDown.Location = New System.Drawing.Point(253, 322)
        Me.LbAutoShDown.Name = "LbAutoShDown"
        Me.LbAutoShDown.Size = New System.Drawing.Size(225, 13)
        Me.LbAutoShDown.TabIndex = 17
        Me.LbAutoShDown.Text = "!!! Automatic Shutdown is activated !!!"
        Me.LbAutoShDown.Visible = False
        '
        'ChBoxAllGEN
        '
        Me.ChBoxAllGEN.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ChBoxAllGEN.AutoSize = True
        Me.ChBoxAllGEN.Location = New System.Drawing.Point(129, 321)
        Me.ChBoxAllGEN.Name = "ChBoxAllGEN"
        Me.ChBoxAllGEN.Size = New System.Drawing.Size(37, 17)
        Me.ChBoxAllGEN.TabIndex = 16
        Me.ChBoxAllGEN.Text = "All"
        Me.ChBoxAllGEN.UseVisualStyleBackColor = True
        '
        'LvGEN
        '
        Me.LvGEN.AllowDrop = True
        Me.LvGEN.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LvGEN.CheckBoxes = True
        Me.LvGEN.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColGENpath, Me.ColGENstatus})
        Me.LvGEN.FullRowSelect = True
        Me.LvGEN.GridLines = True
        Me.LvGEN.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.LvGEN.HideSelection = False
        Me.LvGEN.LabelEdit = True
        Me.LvGEN.Location = New System.Drawing.Point(117, 6)
        Me.LvGEN.Name = "LvGEN"
        Me.LvGEN.Size = New System.Drawing.Size(995, 305)
        Me.LvGEN.TabIndex = 14
        Me.LvGEN.UseCompatibleStateImageBehavior = False
        Me.LvGEN.View = System.Windows.Forms.View.Details
        '
        'ColGENpath
        '
        Me.ColGENpath.Text = "Filepath"
        Me.ColGENpath.Width = 797
        '
        'ColGENstatus
        '
        Me.ColGENstatus.Text = ""
        Me.ColGENstatus.Width = 175
        '
        'ButtonGENopt
        '
        Me.ButtonGENopt.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonGENopt.Location = New System.Drawing.Point(1032, 317)
        Me.ButtonGENopt.Name = "ButtonGENopt"
        Me.ButtonGENopt.Size = New System.Drawing.Size(80, 23)
        Me.ButtonGENopt.TabIndex = 8
        Me.ButtonGENopt.Text = "List Options"
        Me.ButtonGENopt.UseVisualStyleBackColor = True
        '
        'ButtonGENremove
        '
        Me.ButtonGENremove.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonGENremove.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ButtonGENremove.Image = Global.VECTO.My.Resources.Resources.minus_circle_icon
        Me.ButtonGENremove.Location = New System.Drawing.Point(924, 317)
        Me.ButtonGENremove.Name = "ButtonGENremove"
        Me.ButtonGENremove.Size = New System.Drawing.Size(30, 23)
        Me.ButtonGENremove.TabIndex = 2
        Me.ButtonGENremove.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.Image = Global.VECTO.My.Resources.Resources.Play_icon
        Me.Button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.Button1.Location = New System.Drawing.Point(6, 53)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(105, 41)
        Me.Button1.TabIndex = 12
        Me.Button1.Text = "START"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'ButtonGENadd
        '
        Me.ButtonGENadd.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonGENadd.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ButtonGENadd.Image = Global.VECTO.My.Resources.Resources.plus_circle_icon
        Me.ButtonGENadd.Location = New System.Drawing.Point(888, 317)
        Me.ButtonGENadd.Name = "ButtonGENadd"
        Me.ButtonGENadd.Size = New System.Drawing.Size(30, 23)
        Me.ButtonGENadd.TabIndex = 1
        Me.ButtonGENadd.UseVisualStyleBackColor = True
        '
        'TabPageDRI
        '
        Me.TabPageDRI.Controls.Add(Me.BtDRIdown)
        Me.TabPageDRI.Controls.Add(Me.BtDRIup)
        Me.TabPageDRI.Controls.Add(Me.ChBoxAllDRI)
        Me.TabPageDRI.Controls.Add(Me.LvDRI)
        Me.TabPageDRI.Controls.Add(Me.ButtonDRIedit)
        Me.TabPageDRI.Controls.Add(Me.ButtonDRIremove)
        Me.TabPageDRI.Controls.Add(Me.ButtonDRIadd)
        Me.TabPageDRI.Location = New System.Drawing.Point(4, 22)
        Me.TabPageDRI.Name = "TabPageDRI"
        Me.TabPageDRI.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageDRI.Size = New System.Drawing.Size(1118, 346)
        Me.TabPageDRI.TabIndex = 1
        Me.TabPageDRI.Text = "Driving Cycles"
        Me.TabPageDRI.UseVisualStyleBackColor = True
        '
        'BtDRIdown
        '
        Me.BtDRIdown.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtDRIdown.Image = Global.VECTO.My.Resources.Resources.Actions_arrow_down_icon
        Me.BtDRIdown.Location = New System.Drawing.Point(996, 317)
        Me.BtDRIdown.Name = "BtDRIdown"
        Me.BtDRIdown.Size = New System.Drawing.Size(30, 23)
        Me.BtDRIdown.TabIndex = 3
        Me.BtDRIdown.UseVisualStyleBackColor = True
        '
        'BtDRIup
        '
        Me.BtDRIup.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtDRIup.Image = Global.VECTO.My.Resources.Resources.Actions_arrow_up_icon
        Me.BtDRIup.Location = New System.Drawing.Point(960, 317)
        Me.BtDRIup.Name = "BtDRIup"
        Me.BtDRIup.Size = New System.Drawing.Size(30, 23)
        Me.BtDRIup.TabIndex = 2
        Me.BtDRIup.UseVisualStyleBackColor = True
        '
        'ChBoxAllDRI
        '
        Me.ChBoxAllDRI.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.ChBoxAllDRI.AutoSize = True
        Me.ChBoxAllDRI.Location = New System.Drawing.Point(15, 321)
        Me.ChBoxAllDRI.Name = "ChBoxAllDRI"
        Me.ChBoxAllDRI.Size = New System.Drawing.Size(37, 17)
        Me.ChBoxAllDRI.TabIndex = 7
        Me.ChBoxAllDRI.Text = "All"
        Me.ChBoxAllDRI.UseVisualStyleBackColor = True
        '
        'LvDRI
        '
        Me.LvDRI.AllowDrop = True
        Me.LvDRI.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LvDRI.CheckBoxes = True
        Me.LvDRI.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColDRIpath, Me.ColDRIstatus})
        Me.LvDRI.FullRowSelect = True
        Me.LvDRI.GridLines = True
        Me.LvDRI.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.LvDRI.HideSelection = False
        Me.LvDRI.LabelEdit = True
        Me.LvDRI.Location = New System.Drawing.Point(6, 6)
        Me.LvDRI.Name = "LvDRI"
        Me.LvDRI.Size = New System.Drawing.Size(1106, 305)
        Me.LvDRI.TabIndex = 6
        Me.LvDRI.UseCompatibleStateImageBehavior = False
        Me.LvDRI.View = System.Windows.Forms.View.Details
        '
        'ColDRIpath
        '
        Me.ColDRIpath.Text = "Filepath"
        Me.ColDRIpath.Width = 915
        '
        'ColDRIstatus
        '
        Me.ColDRIstatus.Text = ""
        Me.ColDRIstatus.Width = 150
        '
        'ButtonDRIedit
        '
        Me.ButtonDRIedit.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonDRIedit.Location = New System.Drawing.Point(1032, 317)
        Me.ButtonDRIedit.Name = "ButtonDRIedit"
        Me.ButtonDRIedit.Size = New System.Drawing.Size(80, 23)
        Me.ButtonDRIedit.TabIndex = 4
        Me.ButtonDRIedit.Text = "List Options"
        Me.ButtonDRIedit.UseVisualStyleBackColor = True
        '
        'ButtonDRIremove
        '
        Me.ButtonDRIremove.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonDRIremove.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ButtonDRIremove.Image = Global.VECTO.My.Resources.Resources.minus_circle_icon
        Me.ButtonDRIremove.Location = New System.Drawing.Point(924, 317)
        Me.ButtonDRIremove.Name = "ButtonDRIremove"
        Me.ButtonDRIremove.Size = New System.Drawing.Size(30, 23)
        Me.ButtonDRIremove.TabIndex = 1
        Me.ButtonDRIremove.UseVisualStyleBackColor = True
        '
        'ButtonDRIadd
        '
        Me.ButtonDRIadd.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonDRIadd.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ButtonDRIadd.Image = Global.VECTO.My.Resources.Resources.plus_circle_icon
        Me.ButtonDRIadd.Location = New System.Drawing.Point(888, 317)
        Me.ButtonDRIadd.Name = "ButtonDRIadd"
        Me.ButtonDRIadd.Size = New System.Drawing.Size(30, 23)
        Me.ButtonDRIadd.TabIndex = 0
        Me.ButtonDRIadd.UseVisualStyleBackColor = True
        '
        'TabPgOptions
        '
        Me.TabPgOptions.Controls.Add(Me.GrBoxSTD)
        Me.TabPgOptions.Controls.Add(Me.GrBoxBATCH)
        Me.TabPgOptions.Controls.Add(Me.ChBoxAutoSD)
        Me.TabPgOptions.Controls.Add(Me.PanelOptAllg)
        Me.TabPgOptions.Location = New System.Drawing.Point(4, 22)
        Me.TabPgOptions.Name = "TabPgOptions"
        Me.TabPgOptions.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPgOptions.Size = New System.Drawing.Size(1118, 346)
        Me.TabPgOptions.TabIndex = 2
        Me.TabPgOptions.Text = "Options"
        Me.TabPgOptions.UseVisualStyleBackColor = True
        '
        'GrBoxSTD
        '
        Me.GrBoxSTD.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GrBoxSTD.Location = New System.Drawing.Point(6, 182)
        Me.GrBoxSTD.Name = "GrBoxSTD"
        Me.GrBoxSTD.Size = New System.Drawing.Size(1106, 158)
        Me.GrBoxSTD.TabIndex = 14
        Me.GrBoxSTD.TabStop = False
        Me.GrBoxSTD.Text = "Standard Options"
        '
        'GrBoxBATCH
        '
        Me.GrBoxBATCH.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GrBoxBATCH.Controls.Add(Me.ChBoxBatchSubD)
        Me.GrBoxBATCH.Controls.Add(Me.Label2)
        Me.GrBoxBATCH.Controls.Add(Me.ButBObrowse)
        Me.GrBoxBATCH.Controls.Add(Me.CbBOmode)
        Me.GrBoxBATCH.Controls.Add(Me.TbBOpath)
        Me.GrBoxBATCH.Location = New System.Drawing.Point(6, 182)
        Me.GrBoxBATCH.Name = "GrBoxBATCH"
        Me.GrBoxBATCH.Size = New System.Drawing.Size(1106, 158)
        Me.GrBoxBATCH.TabIndex = 5
        Me.GrBoxBATCH.TabStop = False
        Me.GrBoxBATCH.Text = "Batch Options"
        '
        'ChBoxBatchSubD
        '
        Me.ChBoxBatchSubD.AutoSize = True
        Me.ChBoxBatchSubD.Location = New System.Drawing.Point(14, 46)
        Me.ChBoxBatchSubD.Name = "ChBoxBatchSubD"
        Me.ChBoxBatchSubD.Size = New System.Drawing.Size(206, 17)
        Me.ChBoxBatchSubD.TabIndex = 4
        Me.ChBoxBatchSubD.Text = "Create Subdirectories for modal results"
        Me.ChBoxBatchSubD.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(11, 22)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(64, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Output Path"
        '
        'ButBObrowse
        '
        Me.ButBObrowse.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButBObrowse.Location = New System.Drawing.Point(1051, 18)
        Me.ButBObrowse.Name = "ButBObrowse"
        Me.ButBObrowse.Size = New System.Drawing.Size(28, 20)
        Me.ButBObrowse.TabIndex = 3
        Me.ButBObrowse.Text = "..."
        Me.ButBObrowse.UseVisualStyleBackColor = True
        '
        'CbBOmode
        '
        Me.CbBOmode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CbBOmode.FormattingEnabled = True
        Me.CbBOmode.Items.AddRange(New Object() {"Directory of .vecto File", "Custom Directory"})
        Me.CbBOmode.Location = New System.Drawing.Point(81, 19)
        Me.CbBOmode.Name = "CbBOmode"
        Me.CbBOmode.Size = New System.Drawing.Size(140, 21)
        Me.CbBOmode.TabIndex = 0
        '
        'TbBOpath
        '
        Me.TbBOpath.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TbBOpath.Location = New System.Drawing.Point(227, 18)
        Me.TbBOpath.Name = "TbBOpath"
        Me.TbBOpath.Size = New System.Drawing.Size(818, 20)
        Me.TbBOpath.TabIndex = 1
        '
        'ChBoxAutoSD
        '
        Me.ChBoxAutoSD.AutoSize = True
        Me.ChBoxAutoSD.Location = New System.Drawing.Point(246, 16)
        Me.ChBoxAutoSD.Name = "ChBoxAutoSD"
        Me.ChBoxAutoSD.Size = New System.Drawing.Size(169, 17)
        Me.ChBoxAutoSD.TabIndex = 13
        Me.ChBoxAutoSD.Text = "Shutdown system after last job"
        Me.ChBoxAutoSD.UseVisualStyleBackColor = True
        '
        'PanelOptAllg
        '
        Me.PanelOptAllg.Controls.Add(Me.CbDecl)
        Me.PanelOptAllg.Controls.Add(Me.PnDeclOpt)
        Me.PanelOptAllg.Controls.Add(Me.ChBoxModOut)
        Me.PanelOptAllg.Location = New System.Drawing.Point(6, 6)
        Me.PanelOptAllg.Name = "PanelOptAllg"
        Me.PanelOptAllg.Size = New System.Drawing.Size(220, 170)
        Me.PanelOptAllg.TabIndex = 0
        '
        'CbDecl
        '
        Me.CbDecl.AutoSize = True
        Me.CbDecl.Location = New System.Drawing.Point(14, 10)
        Me.CbDecl.Name = "CbDecl"
        Me.CbDecl.Size = New System.Drawing.Size(110, 17)
        Me.CbDecl.TabIndex = 15
        Me.CbDecl.Text = "Declaration Mode"
        Me.CbDecl.UseVisualStyleBackColor = True
        '
        'PnDeclOpt
        '
        Me.PnDeclOpt.Controls.Add(Me.CbBatch)
        Me.PnDeclOpt.Controls.Add(Me.ChBoxCyclDistCor)
        Me.PnDeclOpt.Controls.Add(Me.ChBoxUseGears)
        Me.PnDeclOpt.Location = New System.Drawing.Point(8, 38)
        Me.PnDeclOpt.Name = "PnDeclOpt"
        Me.PnDeclOpt.Size = New System.Drawing.Size(202, 93)
        Me.PnDeclOpt.TabIndex = 13
        '
        'CbBatch
        '
        Me.CbBatch.AutoSize = True
        Me.CbBatch.Location = New System.Drawing.Point(6, 3)
        Me.CbBatch.Name = "CbBatch"
        Me.CbBatch.Size = New System.Drawing.Size(84, 17)
        Me.CbBatch.TabIndex = 15
        Me.CbBatch.Text = "Batch Mode"
        Me.CbBatch.UseVisualStyleBackColor = True
        '
        'ChBoxCyclDistCor
        '
        Me.ChBoxCyclDistCor.AutoSize = True
        Me.ChBoxCyclDistCor.Location = New System.Drawing.Point(6, 41)
        Me.ChBoxCyclDistCor.Name = "ChBoxCyclDistCor"
        Me.ChBoxCyclDistCor.Size = New System.Drawing.Size(148, 17)
        Me.ChBoxCyclDistCor.TabIndex = 0
        Me.ChBoxCyclDistCor.Text = "Cycle Distance Correction"
        Me.ChBoxCyclDistCor.UseVisualStyleBackColor = True
        '
        'ChBoxUseGears
        '
        Me.ChBoxUseGears.AutoSize = True
        Me.ChBoxUseGears.Location = New System.Drawing.Point(6, 64)
        Me.ChBoxUseGears.Name = "ChBoxUseGears"
        Me.ChBoxUseGears.Size = New System.Drawing.Size(188, 17)
        Me.ChBoxUseGears.TabIndex = 0
        Me.ChBoxUseGears.Text = "Use gears/rpm's form driving cycle"
        Me.ChBoxUseGears.UseVisualStyleBackColor = True
        '
        'ChBoxModOut
        '
        Me.ChBoxModOut.AutoSize = True
        Me.ChBoxModOut.Checked = True
        Me.ChBoxModOut.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ChBoxModOut.Location = New System.Drawing.Point(14, 137)
        Me.ChBoxModOut.Name = "ChBoxModOut"
        Me.ChBoxModOut.Size = New System.Drawing.Size(115, 17)
        Me.ChBoxModOut.TabIndex = 0
        Me.ChBoxModOut.Text = "Write modal results"
        Me.ChBoxModOut.UseVisualStyleBackColor = True
        '
        'TabPageDEV
        '
        Me.TabPageDEV.Controls.Add(Me.LvDEVoptions)
        Me.TabPageDEV.Location = New System.Drawing.Point(4, 22)
        Me.TabPageDEV.Name = "TabPageDEV"
        Me.TabPageDEV.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageDEV.Size = New System.Drawing.Size(1118, 346)
        Me.TabPageDEV.TabIndex = 3
        Me.TabPageDEV.Text = "DEV"
        Me.TabPageDEV.UseVisualStyleBackColor = True
        '
        'LvDEVoptions
        '
        Me.LvDEVoptions.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LvDEVoptions.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader4, Me.ColumnHeader7, Me.ColumnHeader5, Me.ColumnHeader6, Me.ColumnHeader8, Me.ColumnHeader9})
        Me.LvDEVoptions.FullRowSelect = True
        Me.LvDEVoptions.GridLines = True
        Me.LvDEVoptions.Location = New System.Drawing.Point(6, 6)
        Me.LvDEVoptions.MultiSelect = False
        Me.LvDEVoptions.Name = "LvDEVoptions"
        Me.LvDEVoptions.Size = New System.Drawing.Size(1106, 334)
        Me.LvDEVoptions.TabIndex = 0
        Me.LvDEVoptions.UseCompatibleStateImageBehavior = False
        Me.LvDEVoptions.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader4
        '
        Me.ColumnHeader4.Text = "Property"
        Me.ColumnHeader4.Width = 89
        '
        'ColumnHeader7
        '
        Me.ColumnHeader7.Text = "Description"
        Me.ColumnHeader7.Width = 527
        '
        'ColumnHeader5
        '
        Me.ColumnHeader5.Text = "Type"
        Me.ColumnHeader5.Width = 82
        '
        'ColumnHeader6
        '
        Me.ColumnHeader6.Text = "Value"
        Me.ColumnHeader6.Width = 134
        '
        'ColumnHeader8
        '
        Me.ColumnHeader8.Text = "Default"
        Me.ColumnHeader8.Width = 120
        '
        'ColumnHeader9
        '
        Me.ColumnHeader9.Text = "Saved In DEVconfig.txt"
        Me.ColumnHeader9.Width = 129
        '
        'ConMenFilelist
        '
        Me.ConMenFilelist.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SaveListToolStripMenuItem, Me.LoadListToolStripMenuItem, Me.LoadDefaultListToolStripMenuItem, Me.ClearListToolStripMenuItem})
        Me.ConMenFilelist.Name = "ConMenFilelist"
        Me.ConMenFilelist.Size = New System.Drawing.Size(176, 92)
        '
        'SaveListToolStripMenuItem
        '
        Me.SaveListToolStripMenuItem.Name = "SaveListToolStripMenuItem"
        Me.SaveListToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.SaveListToolStripMenuItem.Text = "Save List..."
        '
        'LoadListToolStripMenuItem
        '
        Me.LoadListToolStripMenuItem.Name = "LoadListToolStripMenuItem"
        Me.LoadListToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.LoadListToolStripMenuItem.Text = "Load List..."
        '
        'LoadDefaultListToolStripMenuItem
        '
        Me.LoadDefaultListToolStripMenuItem.Name = "LoadDefaultListToolStripMenuItem"
        Me.LoadDefaultListToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.LoadDefaultListToolStripMenuItem.Text = "Load Autosave-List"
        '
        'ClearListToolStripMenuItem
        '
        Me.ClearListToolStripMenuItem.Name = "ClearListToolStripMenuItem"
        Me.ClearListToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.ClearListToolStripMenuItem.Text = "Clear List"
        '
        'BackgroundWorker1
        '
        '
        'LvMsg
        '
        Me.LvMsg.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LvMsg.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3})
        Me.LvMsg.Font = New System.Drawing.Font("Courier New", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LvMsg.FullRowSelect = True
        Me.LvMsg.GridLines = True
        Me.LvMsg.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.LvMsg.LabelWrap = False
        Me.LvMsg.Location = New System.Drawing.Point(3, 3)
        Me.LvMsg.MultiSelect = False
        Me.LvMsg.Name = "LvMsg"
        Me.LvMsg.Size = New System.Drawing.Size(1126, 190)
        Me.LvMsg.TabIndex = 0
        Me.LvMsg.UseCompatibleStateImageBehavior = False
        Me.LvMsg.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Message"
        Me.ColumnHeader1.Width = 779
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Time"
        Me.ColumnHeader2.Width = 151
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Source"
        Me.ColumnHeader3.Width = 138
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.SplitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.SplitContainer1.Location = New System.Drawing.Point(0, 27)
        Me.SplitContainer1.Name = "SplitContainer1"
        Me.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.TabControl1)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.LvMsg)
        Me.SplitContainer1.Size = New System.Drawing.Size(1136, 586)
        Me.SplitContainer1.SplitterDistance = 382
        Me.SplitContainer1.TabIndex = 12
        '
        'ToolStrip1
        '
        Me.ToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripBtNew, Me.ToolStripBtOpen, Me.ToolStripSeparator2, Me.ToolStripDrDnBtTools, Me.ToolStripDrDnBtInfo})
        Me.ToolStrip1.Location = New System.Drawing.Point(0, 0)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(1136, 25)
        Me.ToolStrip1.TabIndex = 11
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'ToolStripBtNew
        '
        Me.ToolStripBtNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripBtNew.Image = Global.VECTO.My.Resources.Resources.blue_document_icon
        Me.ToolStripBtNew.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripBtNew.Name = "ToolStripBtNew"
        Me.ToolStripBtNew.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripBtNew.Text = "ToolStripBtNew"
        Me.ToolStripBtNew.ToolTipText = "New Job File"
        '
        'ToolStripBtOpen
        '
        Me.ToolStripBtOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripBtOpen.Image = Global.VECTO.My.Resources.Resources.Open_icon
        Me.ToolStripBtOpen.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripBtOpen.Name = "ToolStripBtOpen"
        Me.ToolStripBtOpen.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripBtOpen.Text = "ToolStripButton1"
        Me.ToolStripBtOpen.ToolTipText = "Open File..."
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        Me.ToolStripSeparator2.Size = New System.Drawing.Size(6, 25)
        '
        'ToolStripDrDnBtTools
        '
        Me.ToolStripDrDnBtTools.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.GENEditorToolStripMenuItem1, Me.VEHEditorToolStripMenuItem, Me.EngineEditorToolStripMenuItem, Me.GearboxEditorToolStripMenuItem, Me.GraphToolStripMenuItem, Me.ToolStripSeparator6, Me.SignOrVerifyFilesToolStripMenuItem, Me.ToolStripSeparator4, Me.OpenLogToolStripMenuItem, Me.SettingsToolStripMenuItem})
        Me.ToolStripDrDnBtTools.Image = Global.VECTO.My.Resources.Resources.Misc_Tools_icon
        Me.ToolStripDrDnBtTools.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripDrDnBtTools.Name = "ToolStripDrDnBtTools"
        Me.ToolStripDrDnBtTools.Size = New System.Drawing.Size(65, 22)
        Me.ToolStripDrDnBtTools.Text = "Tools"
        '
        'GENEditorToolStripMenuItem1
        '
        Me.GENEditorToolStripMenuItem1.Image = Global.VECTO.My.Resources.Resources.F_VECTO
        Me.GENEditorToolStripMenuItem1.Name = "GENEditorToolStripMenuItem1"
        Me.GENEditorToolStripMenuItem1.Size = New System.Drawing.Size(170, 22)
        Me.GENEditorToolStripMenuItem1.Text = "Job Editor"
        '
        'VEHEditorToolStripMenuItem
        '
        Me.VEHEditorToolStripMenuItem.Image = Global.VECTO.My.Resources.Resources.F_VEH
        Me.VEHEditorToolStripMenuItem.Name = "VEHEditorToolStripMenuItem"
        Me.VEHEditorToolStripMenuItem.Size = New System.Drawing.Size(170, 22)
        Me.VEHEditorToolStripMenuItem.Text = "Vehicle Editor"
        '
        'EngineEditorToolStripMenuItem
        '
        Me.EngineEditorToolStripMenuItem.Image = Global.VECTO.My.Resources.Resources.F_ENG
        Me.EngineEditorToolStripMenuItem.Name = "EngineEditorToolStripMenuItem"
        Me.EngineEditorToolStripMenuItem.Size = New System.Drawing.Size(170, 22)
        Me.EngineEditorToolStripMenuItem.Text = "Engine Editor"
        '
        'GearboxEditorToolStripMenuItem
        '
        Me.GearboxEditorToolStripMenuItem.Image = Global.VECTO.My.Resources.Resources.F_GBX
        Me.GearboxEditorToolStripMenuItem.Name = "GearboxEditorToolStripMenuItem"
        Me.GearboxEditorToolStripMenuItem.Size = New System.Drawing.Size(170, 22)
        Me.GearboxEditorToolStripMenuItem.Text = "Gearbox Editor"
        '
        'GraphToolStripMenuItem
        '
        Me.GraphToolStripMenuItem.Image = Global.VECTO.My.Resources.Resources.F_Graph
        Me.GraphToolStripMenuItem.Name = "GraphToolStripMenuItem"
        Me.GraphToolStripMenuItem.Size = New System.Drawing.Size(170, 22)
        Me.GraphToolStripMenuItem.Text = "Graph"
        '
        'ToolStripSeparator6
        '
        Me.ToolStripSeparator6.Name = "ToolStripSeparator6"
        Me.ToolStripSeparator6.Size = New System.Drawing.Size(167, 6)
        '
        'SignOrVerifyFilesToolStripMenuItem
        '
        Me.SignOrVerifyFilesToolStripMenuItem.Image = Global.VECTO.My.Resources.Resources.Status_dialog_password_icon
        Me.SignOrVerifyFilesToolStripMenuItem.Name = "SignOrVerifyFilesToolStripMenuItem"
        Me.SignOrVerifyFilesToolStripMenuItem.Size = New System.Drawing.Size(170, 22)
        Me.SignOrVerifyFilesToolStripMenuItem.Text = "Sign or Verify Files"
        '
        'ToolStripSeparator4
        '
        Me.ToolStripSeparator4.Name = "ToolStripSeparator4"
        Me.ToolStripSeparator4.Size = New System.Drawing.Size(167, 6)
        Me.ToolStripSeparator4.Visible = False
        '
        'OpenLogToolStripMenuItem
        '
        Me.OpenLogToolStripMenuItem.Name = "OpenLogToolStripMenuItem"
        Me.OpenLogToolStripMenuItem.Size = New System.Drawing.Size(170, 22)
        Me.OpenLogToolStripMenuItem.Text = "Open Log"
        '
        'SettingsToolStripMenuItem
        '
        Me.SettingsToolStripMenuItem.Name = "SettingsToolStripMenuItem"
        Me.SettingsToolStripMenuItem.Size = New System.Drawing.Size(170, 22)
        Me.SettingsToolStripMenuItem.Text = "Settings"
        '
        'ToolStripDrDnBtInfo
        '
        Me.ToolStripDrDnBtInfo.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.UserManualToolStripMenuItem, Me.UpdateNotesToolStripMenuItem, Me.ToolStripSeparator3, Me.CreateActivationFileToolStripMenuItem, Me.AboutVECTOToolStripMenuItem1})
        Me.ToolStripDrDnBtInfo.Image = Global.VECTO.My.Resources.Resources.Help_icon
        Me.ToolStripDrDnBtInfo.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripDrDnBtInfo.Name = "ToolStripDrDnBtInfo"
        Me.ToolStripDrDnBtInfo.Size = New System.Drawing.Size(61, 22)
        Me.ToolStripDrDnBtInfo.Text = "Help"
        '
        'UserManualToolStripMenuItem
        '
        Me.UserManualToolStripMenuItem.Name = "UserManualToolStripMenuItem"
        Me.UserManualToolStripMenuItem.Size = New System.Drawing.Size(186, 22)
        Me.UserManualToolStripMenuItem.Text = "User Manual"
        '
        'UpdateNotesToolStripMenuItem
        '
        Me.UpdateNotesToolStripMenuItem.Name = "UpdateNotesToolStripMenuItem"
        Me.UpdateNotesToolStripMenuItem.Size = New System.Drawing.Size(186, 22)
        Me.UpdateNotesToolStripMenuItem.Text = "Release Notes"
        '
        'ToolStripSeparator3
        '
        Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
        Me.ToolStripSeparator3.Size = New System.Drawing.Size(183, 6)
        '
        'CreateActivationFileToolStripMenuItem
        '
        Me.CreateActivationFileToolStripMenuItem.Name = "CreateActivationFileToolStripMenuItem"
        Me.CreateActivationFileToolStripMenuItem.Size = New System.Drawing.Size(186, 22)
        Me.CreateActivationFileToolStripMenuItem.Text = "Create Activation File"
        '
        'AboutVECTOToolStripMenuItem1
        '
        Me.AboutVECTOToolStripMenuItem1.Name = "AboutVECTOToolStripMenuItem1"
        Me.AboutVECTOToolStripMenuItem1.Size = New System.Drawing.Size(186, 22)
        Me.AboutVECTOToolStripMenuItem1.Text = "About VECTO"
        '
        'CmDEV
        '
        Me.CmDEV.Name = "CmDEV"
        Me.CmDEV.Size = New System.Drawing.Size(61, 4)
        '
        'TmProgSec
        '
        Me.TmProgSec.Interval = 1000
        '
        'CmOpenFile
        '
        Me.CmOpenFile.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OpenWithToolStripMenuItem, Me.OpenInGraphWindowToolStripMenuItem, Me.ShowInFolderToolStripMenuItem})
        Me.CmOpenFile.Name = "CmOpenFile"
        Me.CmOpenFile.Size = New System.Drawing.Size(199, 70)
        '
        'OpenWithToolStripMenuItem
        '
        Me.OpenWithToolStripMenuItem.Name = "OpenWithToolStripMenuItem"
        Me.OpenWithToolStripMenuItem.Size = New System.Drawing.Size(198, 22)
        Me.OpenWithToolStripMenuItem.Text = "Open with ..."
        '
        'OpenInGraphWindowToolStripMenuItem
        '
        Me.OpenInGraphWindowToolStripMenuItem.Name = "OpenInGraphWindowToolStripMenuItem"
        Me.OpenInGraphWindowToolStripMenuItem.Size = New System.Drawing.Size(198, 22)
        Me.OpenInGraphWindowToolStripMenuItem.Text = "Open in Graph Window"
        '
        'ShowInFolderToolStripMenuItem
        '
        Me.ShowInFolderToolStripMenuItem.Name = "ShowInFolderToolStripMenuItem"
        Me.ShowInFolderToolStripMenuItem.Size = New System.Drawing.Size(198, 22)
        Me.ShowInFolderToolStripMenuItem.Text = "Show in Folder"
        '
        'F_MAINForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1136, 638)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Controls.Add(Me.SplitContainer1)
        Me.Controls.Add(Me.StatusBAR)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimumSize = New System.Drawing.Size(785, 485)
        Me.Name = "F_MAINForm"
        Me.Text = "VECTO"
        Me.StatusBAR.ResumeLayout(False)
        Me.StatusBAR.PerformLayout()
        Me.TabControl1.ResumeLayout(False)
        Me.TabPageGEN.ResumeLayout(False)
        Me.TabPageGEN.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPageDRI.ResumeLayout(False)
        Me.TabPageDRI.PerformLayout()
        Me.TabPgOptions.ResumeLayout(False)
        Me.TabPgOptions.PerformLayout()
        Me.GrBoxBATCH.ResumeLayout(False)
        Me.GrBoxBATCH.PerformLayout()
        Me.PanelOptAllg.ResumeLayout(False)
        Me.PanelOptAllg.PerformLayout()
        Me.PnDeclOpt.ResumeLayout(False)
        Me.PnDeclOpt.PerformLayout()
        Me.TabPageDEV.ResumeLayout(False)
        Me.ConMenFilelist.ResumeLayout(False)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.CmOpenFile.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents StatusBAR As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripLbStatus As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPageGEN As System.Windows.Forms.TabPage
    Friend WithEvents TabPageDRI As System.Windows.Forms.TabPage
    Friend WithEvents ButtonGENadd As System.Windows.Forms.Button
    Friend WithEvents ButtonGENremove As System.Windows.Forms.Button
    Friend WithEvents ButtonDRIremove As System.Windows.Forms.Button
    Friend WithEvents ButtonDRIadd As System.Windows.Forms.Button
    Friend WithEvents ButtonGENopt As System.Windows.Forms.Button
    Friend WithEvents ButtonDRIedit As System.Windows.Forms.Button
    Friend WithEvents ConMenFilelist As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents SaveListToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents LoadListToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents LoadDefaultListToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ClearListToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    Friend WithEvents ToolStripProgBarOverall As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents LvGEN As System.Windows.Forms.ListView
    Friend WithEvents ColGENpath As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColGENstatus As System.Windows.Forms.ColumnHeader
    Friend WithEvents LvDRI As System.Windows.Forms.ListView
    Friend WithEvents ColDRIpath As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColDRIstatus As System.Windows.Forms.ColumnHeader
    Friend WithEvents ChBoxAllGEN As System.Windows.Forms.CheckBox
    Friend WithEvents ChBoxAllDRI As System.Windows.Forms.CheckBox
    Friend WithEvents TabPgOptions As System.Windows.Forms.TabPage
    Friend WithEvents ChBoxModOut As System.Windows.Forms.CheckBox
    Friend WithEvents ChBoxUseGears As System.Windows.Forms.CheckBox
    Friend WithEvents ChBoxCyclDistCor As System.Windows.Forms.CheckBox
    Friend WithEvents PanelOptAllg As System.Windows.Forms.Panel
    Friend WithEvents LbAutoShDown As System.Windows.Forms.Label
    Friend WithEvents ChBoxAutoSD As System.Windows.Forms.CheckBox
    Friend WithEvents TbBOpath As System.Windows.Forms.TextBox
    Friend WithEvents CbBOmode As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents ButBObrowse As System.Windows.Forms.Button
    Friend WithEvents ChBoxBatchSubD As System.Windows.Forms.CheckBox
    Friend WithEvents LvMsg As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents GrBoxBATCH As System.Windows.Forms.GroupBox
    Friend WithEvents TabPageDEV As System.Windows.Forms.TabPage
    Friend WithEvents LvDEVoptions As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader5 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader6 As System.Windows.Forms.ColumnHeader
    Friend WithEvents CmDEV As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ColumnHeader7 As System.Windows.Forms.ColumnHeader
    Friend WithEvents BtGENup As System.Windows.Forms.Button
    Friend WithEvents BtGENdown As System.Windows.Forms.Button
    Friend WithEvents BtDRIdown As System.Windows.Forms.Button
    Friend WithEvents BtDRIup As System.Windows.Forms.Button
    Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripBtNew As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripBtOpen As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripDrDnBtTools As System.Windows.Forms.ToolStripDropDownButton
    Friend WithEvents GENEditorToolStripMenuItem1 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents VEHEditorToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator4 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents OpenLogToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SettingsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripDrDnBtInfo As System.Windows.Forms.ToolStripDropDownButton
    Friend WithEvents CreateActivationFileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AboutVECTOToolStripMenuItem1 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripProgBarJob As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents TmProgSec As System.Windows.Forms.Timer
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents EngineEditorToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GearboxEditorToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UserManualToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator3 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents CmOpenFile As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents OpenWithToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ShowInFolderToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ColumnHeader8 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader9 As System.Windows.Forms.ColumnHeader
    Friend WithEvents UpdateNotesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SignOrVerifyFilesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator6 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents GrBoxSTD As System.Windows.Forms.GroupBox
    Friend WithEvents PnDeclOpt As System.Windows.Forms.Panel
    Friend WithEvents LbDecl As System.Windows.Forms.Label
    Friend WithEvents CbDecl As System.Windows.Forms.CheckBox
    Friend WithEvents GraphToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenInGraphWindowToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CbBatch As System.Windows.Forms.CheckBox

End Class
