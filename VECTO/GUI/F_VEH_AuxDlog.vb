Imports System.Windows.Forms

Public Class F_VEH_AuxDlog

    Public VehPath As String = ""

    'Private Sub F_VEH_AuxDlog_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
    '    Stop

    'End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub F_VEH_AuxDlog_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.WindowsShutDown And Me.DialogResult <> Windows.Forms.DialogResult.Cancel Then

            If Trim(Me.TbID.Text) = "" Or Trim(Me.CbType.Text) = "" Or Trim(Me.TbPath.Text) = "" Then
                MsgBox("Form is incomplete!", MsgBoxStyle.Critical)
                e.Cancel = True
            End If

            If Me.TbID.Text.Contains(",") Or Me.CbType.Text.Contains(",") Or Me.TbPath.Text.Contains(",") Then
                MsgBox("',' is no valid character!", MsgBoxStyle.Critical)
                e.Cancel = True
            End If

        End If
    End Sub

    Private Sub BtBrowse_Click(sender As System.Object, e As System.EventArgs) Handles BtBrowse.Click
        If fbAUX.OpenDialog(fFileRepl(Me.TbPath.Text, VehPath)) Then Me.TbPath.Text = fFileWoDir(fbAUX.Files(0), VehPath)
    End Sub

    Private Sub CbType_TextChanged(sender As Object, e As System.EventArgs) Handles CbType.TextChanged

        If Me.CbType.Text = "" Then
            Me.TbID.Text = ""
        Else
            Me.TbID.Text = Trim(UCase(Me.CbType.Text.Substring(0, CInt(Math.Min(Me.CbType.Text.Length, 3)))))
        End If

    End Sub

    Private Sub TbID_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbID.TextChanged
        If Trim(Me.TbID.Text) = "" Then
            Me.LbIDhelp.Text = ""
        Else
            Me.LbIDhelp.Text = "Header in Driving cycle: <AUX_" & Trim(Me.TbID.Text) & ">"
        End If
    End Sub


End Class
