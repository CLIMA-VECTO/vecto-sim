Imports System.Windows.Forms

Public Class F_ADVfzp

    Private TimerTime As Int16

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub F_ADVfzp_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Click
        TimerAbort()
    End Sub

    Private Sub F_ADVfzp_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        TimerTime = 10
        Me.LbTimer.Text = "Closing Dialog in " & TimerTime & "s"
        Me.Timer1.Start()
    End Sub

    Private Sub TimerAbort()
        Me.Timer1.Stop()
        Me.LbTimer.Text = " "
    End Sub

    Public Function Answer(ByVal MsgText As String) As Int16
        Me.LbMsg.Text = MsgText
        If Me.ShowDialog() = Windows.Forms.DialogResult.OK Then
            If Me.RbUseSFZP.Checked Then
                Return 0
            ElseIf Me.RbUseFZP.Checked Then
                Return 1
            Else
                Return 2
            End If
        Else
            Return 2
        End If
    End Function

    Private Sub Timer1_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        TimerTime -= 1S
        If TimerTime = 0 Then
            Me.Timer1.Stop()
            Me.DialogResult = Windows.Forms.DialogResult.OK
            Me.Close()
        End If
        Me.LbTimer.Text = "Closing Dialog in " & TimerTime & "s"
    End Sub

    Private Sub RbUseSFZP_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RbUseSFZP.CheckedChanged
        TimerAbort()
    End Sub

    Private Sub RbUseFZP_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RbUseFZP.CheckedChanged
        TimerAbort()
    End Sub

    Private Sub LbTimer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LbTimer.Click
        TimerAbort()
    End Sub
End Class
