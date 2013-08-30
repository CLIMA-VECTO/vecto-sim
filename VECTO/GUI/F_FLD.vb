﻿Imports System.Windows.Forms

Public Class F_FLD

    Public EngFile As String = ""

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

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub BtFLD_Click(sender As System.Object, e As System.EventArgs) Handles BtFLD.Click
        If fbFLD.OpenDialog(fFileRepl(Me.TbFLD.Text, fPATH(EngFile))) Then Me.TbFLD.Text = fFileWoDir(fbFLD.Files(0), fPATH(EngFile))
    End Sub

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

    Private Sub OpenWithGRAPHiToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles OpenWithGRAPHiToolStripMenuItem.Click
        If Not FileOpenGRAPHi(CmFiles) Then MsgBox("Failed to open file!")
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
