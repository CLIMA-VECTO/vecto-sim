Imports System.Windows.Forms

Public Class F_VEH_GearDlog

    Public NextGear As Boolean
    Public GbxPath As String

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click

        If Not IsNumeric(Me.TbRatio.Text) Then
            MsgBox("Gear ratio is invalid!")
            Me.TbRatio.Focus()
            Me.TbRatio.SelectAll()
            Exit Sub
        End If

        NextGear = False
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        NextGear = False
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub BtNext_Click(sender As System.Object, e As System.EventArgs) Handles BtNext.Click

        If Not IsNumeric(Me.TbRatio.Text) Then
            MsgBox("Gear ratio is invalid!")
            Me.TbRatio.Focus()
            Me.TbRatio.SelectAll()
            Exit Sub
        End If

        NextGear = True
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub BtBrowse_Click(sender As System.Object, e As System.EventArgs) Handles BtBrowse.Click
        Dim fb As cFileBrowser
        fb = New cFileBrowser("GetrMap", False, True)
        If fb.OpenDialog(fFileRepl(Me.TbMapPath.Text, GbxPath)) Then
            Me.TbMapPath.Text = fFileWoDir(fb.Files(0), GbxPath)
        End If
    End Sub

End Class
