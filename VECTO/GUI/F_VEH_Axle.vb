Imports System.Windows.Forms

Public Class F_VEH_Axle

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click

        If Not IsNumeric(Me.TbWeight.Text) OrElse Trim(Me.TbWeight.Text) = "" Then
            MsgBox("Weight input is not valid!")
            Exit Sub
        End If

        If Not IsNumeric(Me.TbRRC.Text) OrElse Trim(Me.TbRRC.Text) = "" Then
            MsgBox("RRC input is not valid!")
            Exit Sub
        End If

        If Not IsNumeric(Me.TbFzISO.Text) OrElse Trim(Me.TbFzISO.Text) = "" Then
            MsgBox("Fz ISO input is not valid!")
            Exit Sub
        End If

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

End Class
