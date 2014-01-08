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
Partial Class FB_Dialog
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
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.ButtonNewDir = New System.Windows.Forms.Button()
        Me.ButtonDesktop = New System.Windows.Forms.Button()
        Me.ButtonWorkDir = New System.Windows.Forms.Button()
        Me.ButtonHisFolder = New System.Windows.Forms.Button()
        Me.ButtonFolderBack = New System.Windows.Forms.Button()
        Me.TextBoxSearchFolder = New System.Windows.Forms.TextBox()
        Me.ListViewFolder = New System.Windows.Forms.ListView()
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ComboBoxDrive = New System.Windows.Forms.ComboBox()
        Me.ButtonAll = New System.Windows.Forms.Button()
        Me.LabelFileAnz = New System.Windows.Forms.Label()
        Me.ComboBoxExt = New System.Windows.Forms.ComboBox()
        Me.ButtonHisFile = New System.Windows.Forms.Button()
        Me.TextBoxSearchFile = New System.Windows.Forms.TextBox()
        Me.ListViewFiles = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ContextMenuFile = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.RenameFileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DeleteFileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TextBoxPath = New System.Windows.Forms.TextBox()
        Me.ContextMenuHisFolder = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ContextMenuHisFile = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ButtonOK = New System.Windows.Forms.Button()
        Me.ButtonCancel = New System.Windows.Forms.Button()
        Me.TextBoxCurrent = New System.Windows.Forms.TextBox()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.ContextMenuFile.SuspendLayout()
        Me.SuspendLayout()
        '
        'SplitContainer1
        '
        Me.SplitContainer1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.SplitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.SplitContainer1.Location = New System.Drawing.Point(12, 36)
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.ButtonNewDir)
        Me.SplitContainer1.Panel1.Controls.Add(Me.ButtonDesktop)
        Me.SplitContainer1.Panel1.Controls.Add(Me.ButtonWorkDir)
        Me.SplitContainer1.Panel1.Controls.Add(Me.ButtonHisFolder)
        Me.SplitContainer1.Panel1.Controls.Add(Me.ButtonFolderBack)
        Me.SplitContainer1.Panel1.Controls.Add(Me.TextBoxSearchFolder)
        Me.SplitContainer1.Panel1.Controls.Add(Me.ListViewFolder)
        Me.SplitContainer1.Panel1.Controls.Add(Me.ComboBoxDrive)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.ButtonAll)
        Me.SplitContainer1.Panel2.Controls.Add(Me.LabelFileAnz)
        Me.SplitContainer1.Panel2.Controls.Add(Me.ComboBoxExt)
        Me.SplitContainer1.Panel2.Controls.Add(Me.ButtonHisFile)
        Me.SplitContainer1.Panel2.Controls.Add(Me.TextBoxSearchFile)
        Me.SplitContainer1.Panel2.Controls.Add(Me.ListViewFiles)
        Me.SplitContainer1.Size = New System.Drawing.Size(764, 293)
        Me.SplitContainer1.SplitterDistance = 382
        Me.SplitContainer1.TabIndex = 5
        Me.SplitContainer1.TabStop = False
        '
        'ButtonNewDir
        '
        Me.ButtonNewDir.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonNewDir.Location = New System.Drawing.Point(139, 3)
        Me.ButtonNewDir.Name = "ButtonNewDir"
        Me.ButtonNewDir.Size = New System.Drawing.Size(38, 21)
        Me.ButtonNewDir.TabIndex = 21
        Me.ButtonNewDir.TabStop = False
        Me.ButtonNewDir.Text = "New"
        Me.ButtonNewDir.UseVisualStyleBackColor = True
        '
        'ButtonDesktop
        '
        Me.ButtonDesktop.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonDesktop.Location = New System.Drawing.Point(183, 3)
        Me.ButtonDesktop.Name = "ButtonDesktop"
        Me.ButtonDesktop.Size = New System.Drawing.Size(57, 21)
        Me.ButtonDesktop.TabIndex = 22
        Me.ButtonDesktop.TabStop = False
        Me.ButtonDesktop.Text = "Desktop"
        Me.ButtonDesktop.UseVisualStyleBackColor = True
        '
        'ButtonWorkDir
        '
        Me.ButtonWorkDir.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonWorkDir.Location = New System.Drawing.Point(246, 3)
        Me.ButtonWorkDir.Name = "ButtonWorkDir"
        Me.ButtonWorkDir.Size = New System.Drawing.Size(57, 21)
        Me.ButtonWorkDir.TabIndex = 23
        Me.ButtonWorkDir.TabStop = False
        Me.ButtonWorkDir.Text = "Work-Dir"
        Me.ButtonWorkDir.UseVisualStyleBackColor = True
        '
        'ButtonHisFolder
        '
        Me.ButtonHisFolder.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonHisFolder.Location = New System.Drawing.Point(309, 3)
        Me.ButtonHisFolder.Name = "ButtonHisFolder"
        Me.ButtonHisFolder.Size = New System.Drawing.Size(68, 21)
        Me.ButtonHisFolder.TabIndex = 24
        Me.ButtonHisFolder.TabStop = False
        Me.ButtonHisFolder.Text = "His / Fav"
        Me.ButtonHisFolder.UseVisualStyleBackColor = True
        '
        'ButtonFolderBack
        '
        Me.ButtonFolderBack.Location = New System.Drawing.Point(63, 3)
        Me.ButtonFolderBack.Name = "ButtonFolderBack"
        Me.ButtonFolderBack.Size = New System.Drawing.Size(28, 21)
        Me.ButtonFolderBack.TabIndex = 20
        Me.ButtonFolderBack.TabStop = False
        Me.ButtonFolderBack.Text = "<"
        Me.ButtonFolderBack.UseVisualStyleBackColor = True
        '
        'TextBoxSearchFolder
        '
        Me.TextBoxSearchFolder.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBoxSearchFolder.Location = New System.Drawing.Point(3, 266)
        Me.TextBoxSearchFolder.Name = "TextBoxSearchFolder"
        Me.TextBoxSearchFolder.Size = New System.Drawing.Size(374, 20)
        Me.TextBoxSearchFolder.TabIndex = 15
        '
        'ListViewFolder
        '
        Me.ListViewFolder.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ListViewFolder.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader3})
        Me.ListViewFolder.FullRowSelect = True
        Me.ListViewFolder.GridLines = True
        Me.ListViewFolder.HideSelection = False
        Me.ListViewFolder.Location = New System.Drawing.Point(3, 30)
        Me.ListViewFolder.MultiSelect = False
        Me.ListViewFolder.Name = "ListViewFolder"
        Me.ListViewFolder.Size = New System.Drawing.Size(374, 230)
        Me.ListViewFolder.TabIndex = 10
        Me.ListViewFolder.UseCompatibleStateImageBehavior = False
        Me.ListViewFolder.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Directory"
        '
        'ComboBoxDrive
        '
        Me.ComboBoxDrive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxDrive.FormattingEnabled = True
        Me.ComboBoxDrive.Location = New System.Drawing.Point(3, 3)
        Me.ComboBoxDrive.Name = "ComboBoxDrive"
        Me.ComboBoxDrive.Size = New System.Drawing.Size(54, 21)
        Me.ComboBoxDrive.TabIndex = 5
        '
        'ButtonAll
        '
        Me.ButtonAll.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonAll.Location = New System.Drawing.Point(239, 3)
        Me.ButtonAll.Name = "ButtonAll"
        Me.ButtonAll.Size = New System.Drawing.Size(71, 21)
        Me.ButtonAll.TabIndex = 19
        Me.ButtonAll.Text = "Select All"
        Me.ButtonAll.UseVisualStyleBackColor = True
        '
        'LabelFileAnz
        '
        Me.LabelFileAnz.AutoSize = True
        Me.LabelFileAnz.Location = New System.Drawing.Point(3, 7)
        Me.LabelFileAnz.Name = "LabelFileAnz"
        Me.LabelFileAnz.Size = New System.Drawing.Size(37, 13)
        Me.LabelFileAnz.TabIndex = 7
        Me.LabelFileAnz.Text = "0 Files"
        '
        'ComboBoxExt
        '
        Me.ComboBoxExt.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ComboBoxExt.FormattingEnabled = True
        Me.ComboBoxExt.Location = New System.Drawing.Point(291, 266)
        Me.ComboBoxExt.Name = "ComboBoxExt"
        Me.ComboBoxExt.Size = New System.Drawing.Size(82, 21)
        Me.ComboBoxExt.TabIndex = 20
        '
        'ButtonHisFile
        '
        Me.ButtonHisFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonHisFile.Location = New System.Drawing.Point(316, 3)
        Me.ButtonHisFile.Name = "ButtonHisFile"
        Me.ButtonHisFile.Size = New System.Drawing.Size(57, 21)
        Me.ButtonHisFile.TabIndex = 24
        Me.ButtonHisFile.TabStop = False
        Me.ButtonHisFile.Text = "History"
        Me.ButtonHisFile.UseVisualStyleBackColor = True
        '
        'TextBoxSearchFile
        '
        Me.TextBoxSearchFile.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBoxSearchFile.Location = New System.Drawing.Point(3, 266)
        Me.TextBoxSearchFile.Name = "TextBoxSearchFile"
        Me.TextBoxSearchFile.Size = New System.Drawing.Size(282, 20)
        Me.TextBoxSearchFile.TabIndex = 15
        '
        'ListViewFiles
        '
        Me.ListViewFiles.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ListViewFiles.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1})
        Me.ListViewFiles.ContextMenuStrip = Me.ContextMenuFile
        Me.ListViewFiles.FullRowSelect = True
        Me.ListViewFiles.GridLines = True
        Me.ListViewFiles.HideSelection = False
        Me.ListViewFiles.Location = New System.Drawing.Point(3, 30)
        Me.ListViewFiles.Name = "ListViewFiles"
        Me.ListViewFiles.Size = New System.Drawing.Size(370, 230)
        Me.ListViewFiles.TabIndex = 10
        Me.ListViewFiles.UseCompatibleStateImageBehavior = False
        Me.ListViewFiles.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Filename"
        Me.ColumnHeader1.Width = 251
        '
        'ContextMenuFile
        '
        Me.ContextMenuFile.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.RenameFileToolStripMenuItem, Me.DeleteFileToolStripMenuItem})
        Me.ContextMenuFile.Name = "ContextMenuFile"
        Me.ContextMenuFile.Size = New System.Drawing.Size(156, 48)
        '
        'RenameFileToolStripMenuItem
        '
        Me.RenameFileToolStripMenuItem.Name = "RenameFileToolStripMenuItem"
        Me.RenameFileToolStripMenuItem.Size = New System.Drawing.Size(155, 22)
        Me.RenameFileToolStripMenuItem.Text = "Rename File..."
        '
        'DeleteFileToolStripMenuItem
        '
        Me.DeleteFileToolStripMenuItem.Name = "DeleteFileToolStripMenuItem"
        Me.DeleteFileToolStripMenuItem.Size = New System.Drawing.Size(155, 22)
        Me.DeleteFileToolStripMenuItem.Text = "Delete File..."
        '
        'TextBoxPath
        '
        Me.TextBoxPath.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBoxPath.Location = New System.Drawing.Point(12, 335)
        Me.TextBoxPath.Name = "TextBoxPath"
        Me.TextBoxPath.Size = New System.Drawing.Size(764, 20)
        Me.TextBoxPath.TabIndex = 15
        '
        'ContextMenuHisFolder
        '
        Me.ContextMenuHisFolder.Name = "ContextMenuFolderHis"
        Me.ContextMenuHisFolder.Size = New System.Drawing.Size(61, 4)
        '
        'ContextMenuHisFile
        '
        Me.ContextMenuHisFile.Name = "ContextMenuFileHis"
        Me.ContextMenuHisFile.Size = New System.Drawing.Size(61, 4)
        '
        'ButtonOK
        '
        Me.ButtonOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.ButtonOK.Location = New System.Drawing.Point(620, 361)
        Me.ButtonOK.Name = "ButtonOK"
        Me.ButtonOK.Size = New System.Drawing.Size(75, 23)
        Me.ButtonOK.TabIndex = 20
        Me.ButtonOK.TabStop = False
        Me.ButtonOK.Text = "OK"
        Me.ButtonOK.UseVisualStyleBackColor = True
        '
        'ButtonCancel
        '
        Me.ButtonCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ButtonCancel.Location = New System.Drawing.Point(701, 361)
        Me.ButtonCancel.Name = "ButtonCancel"
        Me.ButtonCancel.Size = New System.Drawing.Size(75, 23)
        Me.ButtonCancel.TabIndex = 25
        Me.ButtonCancel.TabStop = False
        Me.ButtonCancel.Text = "Cancel"
        Me.ButtonCancel.UseVisualStyleBackColor = True
        '
        'TextBoxCurrent
        '
        Me.TextBoxCurrent.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBoxCurrent.Cursor = System.Windows.Forms.Cursors.Hand
        Me.TextBoxCurrent.Location = New System.Drawing.Point(12, 14)
        Me.TextBoxCurrent.Name = "TextBoxCurrent"
        Me.TextBoxCurrent.ReadOnly = True
        Me.TextBoxCurrent.Size = New System.Drawing.Size(764, 20)
        Me.TextBoxCurrent.TabIndex = 0
        Me.TextBoxCurrent.TabStop = False
        '
        'FB_Dialog
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ButtonCancel
        Me.ClientSize = New System.Drawing.Size(788, 391)
        Me.Controls.Add(Me.TextBoxCurrent)
        Me.Controls.Add(Me.ButtonCancel)
        Me.Controls.Add(Me.ButtonOK)
        Me.Controls.Add(Me.TextBoxPath)
        Me.Controls.Add(Me.SplitContainer1)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FB_Dialog"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "File Browser"
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel1.PerformLayout()
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        Me.SplitContainer1.Panel2.PerformLayout()
        Me.SplitContainer1.ResumeLayout(False)
        Me.ContextMenuFile.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents ComboBoxDrive As System.Windows.Forms.ComboBox
    Friend WithEvents ListViewFolder As System.Windows.Forms.ListView
    Friend WithEvents ListViewFiles As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents TextBoxSearchFolder As System.Windows.Forms.TextBox
    Friend WithEvents TextBoxSearchFile As System.Windows.Forms.TextBox
    Friend WithEvents TextBoxPath As System.Windows.Forms.TextBox
    Friend WithEvents ButtonFolderBack As System.Windows.Forms.Button
    Friend WithEvents ContextMenuHisFolder As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ContextMenuHisFile As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ButtonHisFolder As System.Windows.Forms.Button
    Friend WithEvents ButtonHisFile As System.Windows.Forms.Button
    Friend WithEvents ButtonOK As System.Windows.Forms.Button
    Friend WithEvents ButtonCancel As System.Windows.Forms.Button
    Friend WithEvents ComboBoxExt As System.Windows.Forms.ComboBox
    Friend WithEvents TextBoxCurrent As System.Windows.Forms.TextBox
    Friend WithEvents ButtonWorkDir As System.Windows.Forms.Button
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents LabelFileAnz As System.Windows.Forms.Label
    Friend WithEvents ButtonDesktop As System.Windows.Forms.Button
    Friend WithEvents ContextMenuFile As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents RenameFileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DeleteFileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ButtonNewDir As System.Windows.Forms.Button
    Friend WithEvents ButtonAll As System.Windows.Forms.Button

End Class
