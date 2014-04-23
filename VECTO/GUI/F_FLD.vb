' Copyright 2014 European Union.
' Licensed under the EUPL (the 'Licence');
'
' * You may not use this work except in compliance with the Licence.
' * You may obtain a copy of the Licence at: http://ec.europa.eu/idabc/eupl
' * Unless required by applicable law or agreed to in writing,
'   software distributed under the Licence is distributed on an "AS IS" basis,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'
Imports System.Windows.Forms

' See the LICENSE.txt for the specific language governing permissions and limitations.
''' <summary>
''' Dialog for selecting VFLD files and assign Gears.
''' </summary>
''' <remarks></remarks>
Public Class F_FLD

    'Parent Engine file
    Public EngFile As String = ""

    'Save and close
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click

        If Trim(Me.TbFLD.Text) = "" Then
            MsgBox("No file defined!")
            Me.TbFLD.Focus()
            Me.TbFLD.SelectAll()
            Exit Sub
        End If

        If Me.NumGearTo.Value < Me.NumGearFrom.Value Then
            MsgBox("Invalid gear range!")
            Exit Sub
        End If

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    'Cancel
    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    'Browse for VFLD file
    Private Sub BtFLD_Click(sender As System.Object, e As System.EventArgs) Handles BtFLD.Click
        If fbFLD.OpenDialog(fFileRepl(Me.TbFLD.Text, fPATH(EngFile))) Then Me.TbFLD.Text = fFileWoDir(fbFLD.Files(0), fPATH(EngFile))
    End Sub

    'Open VFLD file
    Private Sub BtFLDOpen_Click(sender As System.Object, e As System.EventArgs) Handles BtFLDOpen.Click
        OpenFiles(fFileRepl(Me.TbFLD.Text, fPATH(EngFile)))
    End Sub

#Region "Open File Context Menu"

    Private CmFiles As String()

    Private Sub OpenFiles(ParamArray files() As String)

        If files.Length = 0 Then Exit Sub

        CmFiles = files

        OpenWithToolStripMenuItem.Text = "Open with " & Cfg.OpenCmdName

        CmOpenFile.Show(Cursor.Position)

    End Sub

    Private Sub OpenWithToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles OpenWithToolStripMenuItem.Click
        If Not FileOpenAlt(CmFiles(0)) Then MsgBox("Failed to open file!")
    End Sub

    Private Sub ShowInFolderToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ShowInFolderToolStripMenuItem.Click
        If IO.File.Exists(CmFiles(0)) Then
            Try
                System.Diagnostics.Process.Start("explorer", "/select,""" & CmFiles(0) & "")
            Catch ex As Exception
                MsgBox("Failed to open file!")
            End Try
        Else
            MsgBox("File not found!")
        End If
    End Sub

#End Region



End Class
