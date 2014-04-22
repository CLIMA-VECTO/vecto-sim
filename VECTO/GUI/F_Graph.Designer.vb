<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_Graph
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(F_Graph))
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.BtRemCh = New System.Windows.Forms.Button()
        Me.BtAddCh = New System.Windows.Forms.Button()
        Me.ListView1 = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.CbXaxis = New System.Windows.Forms.ComboBox()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.ToolStripBtOpen = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton2 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
        Me.BtZoomIn = New System.Windows.Forms.Button()
        Me.BtZoomOut = New System.Windows.Forms.Button()
        Me.BtLeft = New System.Windows.Forms.Button()
        Me.BtRight = New System.Windows.Forms.Button()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.ToolStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.LightGray
        Me.PictureBox1.Location = New System.Drawing.Point(262, 28)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(984, 208)
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.BtRemCh)
        Me.GroupBox1.Controls.Add(Me.BtAddCh)
        Me.GroupBox1.Controls.Add(Me.ListView1)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 28)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(244, 237)
        Me.GroupBox1.TabIndex = 1
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Channels"
        '
        'BtRemCh
        '
        Me.BtRemCh.Image = Global.VECTO.My.Resources.Resources.minus_circle_icon
        Me.BtRemCh.Location = New System.Drawing.Point(43, 209)
        Me.BtRemCh.Name = "BtRemCh"
        Me.BtRemCh.Size = New System.Drawing.Size(29, 23)
        Me.BtRemCh.TabIndex = 4
        Me.BtRemCh.UseVisualStyleBackColor = True
        '
        'BtAddCh
        '
        Me.BtAddCh.Image = Global.VECTO.My.Resources.Resources.plus_circle_icon
        Me.BtAddCh.Location = New System.Drawing.Point(6, 209)
        Me.BtAddCh.Name = "BtAddCh"
        Me.BtAddCh.Size = New System.Drawing.Size(29, 23)
        Me.BtAddCh.TabIndex = 3
        Me.BtAddCh.UseVisualStyleBackColor = True
        '
        'ListView1
        '
        Me.ListView1.BackColor = System.Drawing.Color.GhostWhite
        Me.ListView1.CheckBoxes = True
        Me.ListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3})
        Me.ListView1.FullRowSelect = True
        Me.ListView1.GridLines = True
        Me.ListView1.Location = New System.Drawing.Point(6, 19)
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(232, 184)
        Me.ListView1.TabIndex = 0
        Me.ListView1.UseCompatibleStateImageBehavior = False
        Me.ListView1.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Channel"
        Me.ColumnHeader1.Width = 121
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Unit"
        Me.ColumnHeader2.Width = 45
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Y Axis"
        Me.ColumnHeader3.Width = 42
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.CbXaxis)
        Me.GroupBox2.Location = New System.Drawing.Point(16, 309)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(146, 101)
        Me.GroupBox2.TabIndex = 4
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "X Axis"
        '
        'CbXaxis
        '
        Me.CbXaxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CbXaxis.FormattingEnabled = True
        Me.CbXaxis.Items.AddRange(New Object() {"Distance", "Time"})
        Me.CbXaxis.Location = New System.Drawing.Point(6, 19)
        Me.CbXaxis.Name = "CbXaxis"
        Me.CbXaxis.Size = New System.Drawing.Size(121, 21)
        Me.CbXaxis.TabIndex = 0
        '
        'GroupBox3
        '
        Me.GroupBox3.Location = New System.Drawing.Point(222, 310)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(200, 100)
        Me.GroupBox3.TabIndex = 4
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Left Y Axis"
        '
        'GroupBox4
        '
        Me.GroupBox4.Location = New System.Drawing.Point(428, 310)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(200, 100)
        Me.GroupBox4.TabIndex = 4
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "Right Y Axis"
        '
        'ToolStrip1
        '
        Me.ToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripBtOpen, Me.ToolStripButton2, Me.ToolStripSeparator1, Me.ToolStripButton1})
        Me.ToolStrip1.Location = New System.Drawing.Point(0, 0)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(1258, 25)
        Me.ToolStrip1.TabIndex = 31
        Me.ToolStrip1.Text = "ToolStrip1"
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
        'ToolStripButton2
        '
        Me.ToolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton2.Image = Global.VECTO.My.Resources.Resources.Refresh_icon
        Me.ToolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton2.Name = "ToolStripButton2"
        Me.ToolStripButton2.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton2.Text = "ToolStripButton2"
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
        'BtZoomIn
        '
        Me.BtZoomIn.Location = New System.Drawing.Point(634, 242)
        Me.BtZoomIn.Name = "BtZoomIn"
        Me.BtZoomIn.Size = New System.Drawing.Size(75, 23)
        Me.BtZoomIn.TabIndex = 32
        Me.BtZoomIn.Text = "In"
        Me.BtZoomIn.UseVisualStyleBackColor = True
        '
        'BtZoomOut
        '
        Me.BtZoomOut.Location = New System.Drawing.Point(715, 242)
        Me.BtZoomOut.Name = "BtZoomOut"
        Me.BtZoomOut.Size = New System.Drawing.Size(75, 23)
        Me.BtZoomOut.TabIndex = 32
        Me.BtZoomOut.Text = "Out"
        Me.BtZoomOut.UseVisualStyleBackColor = True
        '
        'BtLeft
        '
        Me.BtLeft.Location = New System.Drawing.Point(262, 242)
        Me.BtLeft.Name = "BtLeft"
        Me.BtLeft.Size = New System.Drawing.Size(180, 23)
        Me.BtLeft.TabIndex = 33
        Me.BtLeft.Text = "<"
        Me.BtLeft.UseVisualStyleBackColor = True
        '
        'BtRight
        '
        Me.BtRight.Location = New System.Drawing.Point(448, 242)
        Me.BtRight.Name = "BtRight"
        Me.BtRight.Size = New System.Drawing.Size(180, 23)
        Me.BtRight.TabIndex = 33
        Me.BtRight.Text = ">"
        Me.BtRight.UseVisualStyleBackColor = True
        '
        'F_Graph
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1258, 454)
        Me.Controls.Add(Me.BtRight)
        Me.Controls.Add(Me.BtLeft)
        Me.Controls.Add(Me.BtZoomOut)
        Me.Controls.Add(Me.BtZoomIn)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.PictureBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "F_Graph"
        Me.Text = "Graph"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents ListView1 As System.Windows.Forms.ListView
    Friend WithEvents BtRemCh As System.Windows.Forms.Button
    Friend WithEvents BtAddCh As System.Windows.Forms.Button
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripBtOpen As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButton2 As System.Windows.Forms.ToolStripButton
    Friend WithEvents CbXaxis As System.Windows.Forms.ComboBox
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents BtZoomIn As System.Windows.Forms.Button
    Friend WithEvents BtZoomOut As System.Windows.Forms.Button
    Friend WithEvents BtLeft As System.Windows.Forms.Button
    Friend WithEvents BtRight As System.Windows.Forms.Button
End Class
