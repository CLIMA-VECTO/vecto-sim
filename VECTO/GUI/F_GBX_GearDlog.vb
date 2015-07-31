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
''' Gear Editor (Vehicle Editor sub-dialog)
''' </summary>
''' <remarks></remarks>
Public Class F_GBX_GearDlog

    Public NextGear As Boolean
	Public GbxPath As String

    'Save and Close
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

    'Cancel
    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        NextGear = False
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    'Next Gear button - Close dialog and open for next gear
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

    'Browse for transmission loss map
    Private Sub BtBrowse_Click(sender As System.Object, e As System.EventArgs) Handles BtBrowse.Click
        If fbTLM.OpenDialog(fFileRepl(Me.TbMapPath.Text, GbxPath)) Then
            Me.TbMapPath.Text = fFileWoDir(fbTLM.Files(0), GbxPath)
        End If
    End Sub

    'Browse for shift polygons file
	Private Sub BtShiftPolyBrowse_Click(sender As System.Object, e As System.EventArgs) Handles BtShiftPolyBrowse.Click
		If fbGBS.OpenDialog(fFileRepl(Me.TbShiftPolyFile.Text, GbxPath)) Then
			Me.TbShiftPolyFile.Text = fFileWoDir(fbGBS.Files(0), GbxPath)
		End If
	End Sub

	Private Sub BtBrowseFld_Click(sender As Object, e As EventArgs) Handles BtBrowseFld.Click
		If fbFLD.OpenDialog(fFileRepl(Me.TbFld.Text, GbxPath)) Then
			Me.TbFld.Text = fFileWoDir(fbFLD.Files(0), GbxPath)
		End If
	End Sub

End Class
