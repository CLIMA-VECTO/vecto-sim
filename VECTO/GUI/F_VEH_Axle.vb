Imports System.Windows.Forms

Public Class F_VEH_Axle

    Private Sub F_VEH_Axle_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        'Declaration Mode
        If Declaration.Active Then
            Me.PnAxle.Enabled = False
        End If
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click

        If Not Declaration.Active Then
            If Not IsNumeric(Me.TbAxleShare.Text) OrElse Trim(Me.TbAxleShare.Text) = "" Then
                MsgBox("Weight input is not valid!")
                Exit Sub
            End If
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
