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
Partial Class F_FileSign
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(F_FileSign))
        Me.TbSigFile = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.BtBrowse = New System.Windows.Forms.Button()
        Me.lvFiles = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.BtSign = New System.Windows.Forms.Button()
        Me.BtClose = New System.Windows.Forms.Button()
        Me.BtRemFLD = New System.Windows.Forms.Button()
        Me.BtAddFLD = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.BtClearList = New System.Windows.Forms.Button()
        Me.BtReload = New System.Windows.Forms.Button()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.LbStatus = New System.Windows.Forms.Label()
        Me.LbMode = New System.Windows.Forms.Label()
        Me.LbDateStr = New System.Windows.Forms.Label()
        Me.TbPubKey = New System.Windows.Forms.TextBox()
        Me.TbLicStr = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'TbSigFile
        '
        Me.TbSigFile.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TbSigFile.Location = New System.Drawing.Point(12, 29)
        Me.TbSigFile.Name = "TbSigFile"
        Me.TbSigFile.Size = New System.Drawing.Size(509, 20)
        Me.TbSigFile.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 13)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(99, 13)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "Signature file (.vsig)"
        '
        'BtBrowse
        '
        Me.BtBrowse.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtBrowse.Location = New System.Drawing.Point(527, 27)
        Me.BtBrowse.Name = "BtBrowse"
        Me.BtBrowse.Size = New System.Drawing.Size(28, 23)
        Me.BtBrowse.TabIndex = 1
        Me.BtBrowse.Text = "..."
        Me.BtBrowse.UseVisualStyleBackColor = True
        '
        'lvFiles
        '
        Me.lvFiles.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lvFiles.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2})
        Me.lvFiles.FullRowSelect = True
        Me.lvFiles.GridLines = True
        Me.lvFiles.LabelEdit = True
        Me.lvFiles.Location = New System.Drawing.Point(6, 19)
        Me.lvFiles.MultiSelect = False
        Me.lvFiles.Name = "lvFiles"
        Me.lvFiles.Size = New System.Drawing.Size(565, 368)
        Me.lvFiles.TabIndex = 0
        Me.lvFiles.UseCompatibleStateImageBehavior = False
        Me.lvFiles.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "File"
        Me.ColumnHeader1.Width = 408
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Status"
        Me.ColumnHeader2.Width = 141
        '
        'BtSign
        '
        Me.BtSign.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.BtSign.Location = New System.Drawing.Point(12, 638)
        Me.BtSign.Name = "BtSign"
        Me.BtSign.Size = New System.Drawing.Size(126, 23)
        Me.BtSign.TabIndex = 5
        Me.BtSign.Text = "Create Signature File"
        Me.BtSign.UseVisualStyleBackColor = True
        '
        'BtClose
        '
        Me.BtClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtClose.Location = New System.Drawing.Point(522, 638)
        Me.BtClose.Name = "BtClose"
        Me.BtClose.Size = New System.Drawing.Size(67, 23)
        Me.BtClose.TabIndex = 6
        Me.BtClose.Text = "Close"
        Me.BtClose.UseVisualStyleBackColor = True
        '
        'BtRemFLD
        '
        Me.BtRemFLD.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.BtRemFLD.Image = Global.VECTO.My.Resources.Resources.minus_circle_icon
        Me.BtRemFLD.Location = New System.Drawing.Point(43, 393)
        Me.BtRemFLD.Name = "BtRemFLD"
        Me.BtRemFLD.Size = New System.Drawing.Size(29, 23)
        Me.BtRemFLD.TabIndex = 2
        Me.BtRemFLD.UseVisualStyleBackColor = True
        '
        'BtAddFLD
        '
        Me.BtAddFLD.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.BtAddFLD.Image = Global.VECTO.My.Resources.Resources.plus_circle_icon
        Me.BtAddFLD.Location = New System.Drawing.Point(6, 393)
        Me.BtAddFLD.Name = "BtAddFLD"
        Me.BtAddFLD.Size = New System.Drawing.Size(29, 23)
        Me.BtAddFLD.TabIndex = 1
        Me.BtAddFLD.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox1.Controls.Add(Me.lvFiles)
        Me.GroupBox1.Controls.Add(Me.BtClearList)
        Me.GroupBox1.Controls.Add(Me.BtRemFLD)
        Me.GroupBox1.Controls.Add(Me.BtAddFLD)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 210)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(577, 422)
        Me.GroupBox1.TabIndex = 4
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Files"
        '
        'BtClearList
        '
        Me.BtClearList.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.BtClearList.Location = New System.Drawing.Point(78, 393)
        Me.BtClearList.Name = "BtClearList"
        Me.BtClearList.Size = New System.Drawing.Size(63, 23)
        Me.BtClearList.TabIndex = 3
        Me.BtClearList.Text = "Clear List"
        Me.BtClearList.UseVisualStyleBackColor = True
        '
        'BtReload
        '
        Me.BtReload.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BtReload.Image = Global.VECTO.My.Resources.Resources.Refresh_icon
        Me.BtReload.Location = New System.Drawing.Point(561, 27)
        Me.BtReload.Name = "BtReload"
        Me.BtReload.Size = New System.Drawing.Size(28, 23)
        Me.BtReload.TabIndex = 2
        Me.BtReload.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox2.Controls.Add(Me.LbStatus)
        Me.GroupBox2.Controls.Add(Me.LbMode)
        Me.GroupBox2.Controls.Add(Me.LbDateStr)
        Me.GroupBox2.Controls.Add(Me.TbPubKey)
        Me.GroupBox2.Controls.Add(Me.TbLicStr)
        Me.GroupBox2.Controls.Add(Me.Label5)
        Me.GroupBox2.Controls.Add(Me.Label6)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Controls.Add(Me.Label4)
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Location = New System.Drawing.Point(18, 55)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(571, 149)
        Me.GroupBox2.TabIndex = 3
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "File Information"
        '
        'LbStatus
        '
        Me.LbStatus.AutoSize = True
        Me.LbStatus.Location = New System.Drawing.Point(99, 116)
        Me.LbStatus.Name = "LbStatus"
        Me.LbStatus.Size = New System.Drawing.Size(0, 13)
        Me.LbStatus.TabIndex = 4
        '
        'LbMode
        '
        Me.LbMode.AutoSize = True
        Me.LbMode.Location = New System.Drawing.Point(99, 87)
        Me.LbMode.Name = "LbMode"
        Me.LbMode.Size = New System.Drawing.Size(0, 13)
        Me.LbMode.TabIndex = 3
        '
        'LbDateStr
        '
        Me.LbDateStr.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LbDateStr.AutoSize = True
        Me.LbDateStr.Location = New System.Drawing.Point(415, 87)
        Me.LbDateStr.Name = "LbDateStr"
        Me.LbDateStr.Size = New System.Drawing.Size(0, 13)
        Me.LbDateStr.TabIndex = 2
        '
        'TbPubKey
        '
        Me.TbPubKey.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TbPubKey.Location = New System.Drawing.Point(99, 48)
        Me.TbPubKey.Name = "TbPubKey"
        Me.TbPubKey.ReadOnly = True
        Me.TbPubKey.Size = New System.Drawing.Size(466, 20)
        Me.TbPubKey.TabIndex = 1
        '
        'TbLicStr
        '
        Me.TbLicStr.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TbLicStr.Location = New System.Drawing.Point(99, 22)
        Me.TbLicStr.Name = "TbLicStr"
        Me.TbLicStr.ReadOnly = True
        Me.TbLicStr.Size = New System.Drawing.Size(466, 20)
        Me.TbLicStr.TabIndex = 0
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(33, 51)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(60, 13)
        Me.Label5.TabIndex = 0
        Me.Label5.Text = "Public Key:"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(53, 116)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(40, 13)
        Me.Label6.TabIndex = 0
        Me.Label6.Text = "Status:"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(56, 87)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(37, 13)
        Me.Label3.TabIndex = 0
        Me.Label3.Text = "Mode:"
        '
        'Label4
        '
        Me.Label4.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(334, 87)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(75, 13)
        Me.Label4.TabIndex = 0
        Me.Label4.Text = "Creation Time:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 25)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(81, 13)
        Me.Label2.TabIndex = 0
        Me.Label2.Text = "License Owner:"
        '
        'F_FileSign
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(601, 673)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.BtReload)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.BtClose)
        Me.Controls.Add(Me.BtSign)
        Me.Controls.Add(Me.BtBrowse)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TbSigFile)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(480, 400)
        Me.Name = "F_FileSign"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Sign & Verify Files"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TbSigFile As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents BtBrowse As System.Windows.Forms.Button
    Friend WithEvents lvFiles As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents BtSign As System.Windows.Forms.Button
    Friend WithEvents BtClose As System.Windows.Forms.Button
    Friend WithEvents BtRemFLD As System.Windows.Forms.Button
    Friend WithEvents BtAddFLD As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents BtClearList As System.Windows.Forms.Button
    Friend WithEvents BtReload As System.Windows.Forms.Button
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents LbMode As System.Windows.Forms.Label
    Friend WithEvents LbDateStr As System.Windows.Forms.Label
    Friend WithEvents TbPubKey As System.Windows.Forms.TextBox
    Friend WithEvents TbLicStr As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents LbStatus As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label

End Class
