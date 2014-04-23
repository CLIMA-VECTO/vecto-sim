' Copyright 2014 European Union.
' Licensed under the EUPL (the 'Licence');
'
' * You may not use this work except in compliance with the Licence.
' * You may obtain a copy of the Licence at: http://ec.europa.eu/idabc/eupl
' * Unless required by applicable law or agreed to in writing,
'   software distributed under the Licence is distributed on an "AS IS" basis,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'
' See the LICENSE.txt for the specific language governing permissions and limitations.
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
