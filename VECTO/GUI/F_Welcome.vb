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

Public Class F_Welcome

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub F_Welcome_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Me.Text = "VECTO " & VECTOvers
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        If IO.File.Exists(MyAppPath & "User Manual\Update Notes.pdf") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\Update Notes.pdf")
        Else
            MsgBox("Update Notes not found!", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        If IO.File.Exists(MyAppPath & "User Manual\qsg\quickstartApp.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\qsg\quickstartApp.html")
        Else
            MsgBox("Quick Start Guide not found!", MsgBoxStyle.Critical)
        End If
    End Sub
End Class
