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
