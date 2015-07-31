Imports System.Windows.Forms

Public Class F_JIRA

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

	Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
		Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Close()
	End Sub

	Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
		If IO.File.Exists(MyAppPath & "User Manual\JIRA Quick Start Guide.pdf") Then
			System.Diagnostics.Process.Start(MyAppPath & "User Manual\JIRA Quick Start Guide.pdf")
		Else
			MsgBox("File not found!", MsgBoxStyle.Critical)
		End If
	End Sub

	Private Sub LinkLabel3_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel3.LinkClicked
		Dim bodytext As String

		bodytext = "Please provide the following information:" & "%0A" & _
				   "- Email" & "%0A" & _
				   "- Name, Surname" & "%0A" & _
				   "- Country of workplace" & "%0A" & _
				   "- Position"

		System.Diagnostics.Process.Start("mailto:vecto@jrc.ec.europa.eu?subject=CITnet%20account&body=" & bodytext)

	End Sub

	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		System.Diagnostics.Process.Start("https://webgate.ec.europa.eu/CITnet/jira/browse/VECTO")
	End Sub

End Class
