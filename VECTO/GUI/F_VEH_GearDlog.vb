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

Public Class F_VEH_GearDlog

    Public NextGear As Boolean
    Public GbxPath As String

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click

        If Not IsNumeric(Me.TbRatio.Text) Then
            MsgBox("Gear ratio is invalid!")
            Me.TbRatio.Focus()
            Me.TbRatio.SelectAll()
            Exit Sub
        End If

        NextGear = False
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        NextGear = False
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub BtNext_Click(sender As System.Object, e As System.EventArgs) Handles BtNext.Click

        If Not IsNumeric(Me.TbRatio.Text) Then
            MsgBox("Gear ratio is invalid!")
            Me.TbRatio.Focus()
            Me.TbRatio.SelectAll()
            Exit Sub
        End If

        NextGear = True
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub BtBrowse_Click(sender As System.Object, e As System.EventArgs) Handles BtBrowse.Click
        Dim fb As cFileBrowser
        fb = New cFileBrowser("GetrMap", False, True)
        If fb.OpenDialog(fFileRepl(Me.TbMapPath.Text, GbxPath)) Then
            Me.TbMapPath.Text = fFileWoDir(fb.Files(0), GbxPath)
        End If
    End Sub

End Class
