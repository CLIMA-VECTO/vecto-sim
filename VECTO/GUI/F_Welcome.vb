Imports System.Windows.Forms

''' <summary>
''' Welcome screen. Shows only on the first time application start
''' </summary>
''' <remarks></remarks>
Public Class F_Welcome

    'Close
    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    'Init
    Private Sub F_Welcome_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Me.Text = "VECTO " & VECTOvers
    End Sub

    'Open Update Notes
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        If IO.File.Exists(MyAppPath & "User Manual\Update Notes.pdf") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\Update Notes.pdf")
        Else
            MsgBox("Update Notes not found!", MsgBoxStyle.Critical)
        End If
    End Sub

    'Open Quick Start Guide
    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        If IO.File.Exists(MyAppPath & "User Manual\qsg\quickstartApp.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\qsg\quickstartApp.html")
        Else
            MsgBox("Quick Start Guide not found!", MsgBoxStyle.Critical)
        End If
    End Sub
End Class
