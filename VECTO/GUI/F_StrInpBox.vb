Imports System.Windows.Forms

''' <summary>
''' String input dialog for changing DEV option values. Default input box cannot process empty strings (emtpy string = Cancel). 
''' </summary>
''' <remarks></remarks>
Public Class F_StrInpBox

    Public Function ShowDlog(ByVal Prompt As String, ByVal DefaultRespone As String) As String
        Me.Label1.Text = Prompt
        Me.TextBox1.Text = DefaultRespone
        If Me.ShowDialog = Windows.Forms.DialogResult.OK Then
            DefaultRespone = Me.TextBox1.Text
        End If
        Return DefaultRespone
    End Function

    Private Sub F_StrInpBox_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        Me.TextBox1.SelectAll()
        Me.TextBox1.Focus()
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

 
   
End Class
