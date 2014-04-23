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
Imports System.Collections.Generic

''' <summary>
''' Aux Config Editor (Job Editor sub-dialog)
''' </summary>
''' <remarks></remarks>
Public Class F_VEH_AuxDlog

    Public VehPath As String = ""

    'New instance
    Public Sub New()

        ' Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent()

        ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Me.CbType.Items.Add("Fan")
        Me.CbType.Items.Add("Steering pump")
        Me.CbType.Items.Add("HVAC")
        Me.CbType.Items.Add("Electric System")

        Me.PnFile.Enabled = Not Cfg.DeclMode
        Me.PnTech.Enabled = Cfg.DeclMode


    End Sub

    'Initialise form
    Private Sub F_VEH_AuxDlog_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Me.Text = CbType.Text
    End Sub

    'Set generic values for Declaration mode
    Private Sub DeclInit()
        Dim txt As String
        Dim kv As KeyValuePair(Of String, Dictionary(Of tMission, Single))

        If Not Cfg.DeclMode Then Exit Sub

        Me.CbTech.Items.Clear()

        Select Case TbID.Text
            Case sKey.AUX.Fan
                For Each txt In Declaration.AuxTechs(tAux.Fan)
                    Me.CbTech.Items.Add(txt)
                Next

            Case sKey.AUX.SteerPump
                For Each txt In Declaration.AuxTechs(tAux.SteerPump)
                    Me.CbTech.Items.Add(txt)
                Next

            Case sKey.AUX.HVAC
                For Each txt In Declaration.AuxTechs(tAux.HVAC)
                    Me.CbTech.Items.Add(txt)
                Next
                Me.CbTech.SelectedIndex = 0

            Case sKey.AUX.ElecSys
                For Each txt In Declaration.AuxTechs(tAux.ElectricSys)
                    Me.CbTech.Items.Add(txt)
                Next
                Me.CbTech.SelectedIndex = 0


            Case Else    'sKey.AUX.PneumSys
                For Each txt In Declaration.AuxTechs(tAux.PneumSys)
                    Me.CbTech.Items.Add(txt)
                Next
                Me.CbTech.SelectedIndex = 0

        End Select


        If TbID.Text = sKey.AUX.ElecSys Then

            Me.LVTech.Items.Clear()
            For Each kv In Declaration.AuxESpower
                Me.LVTech.Items.Add(kv.Key)
            Next
            Me.LVTech.Visible = True

            Me.Height = 457

        Else

            Me.LVTech.Visible = False
            Me.Height = 220

        End If

    End Sub

    'Save and close
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    'Cancel
    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    'Close form. Check if form is complete and valid
    Private Sub F_VEH_AuxDlog_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.WindowsShutDown And Me.DialogResult <> Windows.Forms.DialogResult.Cancel Then

            If Trim(Me.TbID.Text) = "" Or Trim(Me.CbType.Text) = "" Then
                MsgBox("Form is incomplete!", MsgBoxStyle.Critical)
                e.Cancel = True
            End If

            If Me.TbID.Text.Contains(",") Or Me.CbType.Text.Contains(",") Or Me.TbPath.Text.Contains(",") Then
                MsgBox("',' is no valid character!", MsgBoxStyle.Critical)
                e.Cancel = True
            End If

            If Cfg.DeclMode Then

                If Me.CbTech.Text = "" Then
                    MsgBox("Form is incomplete!", MsgBoxStyle.Critical)
                    e.Cancel = True
                End If

            Else

                If Trim(Me.TbPath.Text) = "" Then
                    MsgBox("Form is incomplete!", MsgBoxStyle.Critical)
                    e.Cancel = True
                End If

            End If

        End If
    End Sub

    'Browse for .vaux files
    Private Sub BtBrowse_Click(sender As System.Object, e As System.EventArgs) Handles BtBrowse.Click
        If fbAUX.OpenDialog(fFileRepl(Me.TbPath.Text, VehPath)) Then Me.TbPath.Text = fFileWoDir(fbAUX.Files(0), VehPath)
    End Sub

    'Update ID when Aux Type was changed
    Private Sub CbType_TextChanged(sender As Object, e As System.EventArgs) Handles CbType.TextChanged

        If Me.CbType.Text = "" Then
            Me.TbID.Text = ""
        Else
            If Cfg.DeclMode Then
                Select Case Me.CbType.SelectedIndex
                    Case 0
                        Me.TbID.Text = sKey.AUX.Fan
                    Case 1
                        Me.TbID.Text = sKey.AUX.SteerPump

                    Case Else '2
                        Me.TbID.Text = sKey.AUX.HVAC

                End Select
            Else
                Me.TbID.Text = Trim(UCase(Me.CbType.Text.Substring(0, CInt(Math.Min(Me.CbType.Text.Length, 3)))))
            End If
        End If

    End Sub

    'Update help label if ID was changed
    Private Sub TbID_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbID.TextChanged

        DeclInit()

        If Trim(Me.TbID.Text) = "" Or Cfg.DeclMode Then
            Me.LbIDhelp.Text = ""
        Else
            Me.LbIDhelp.Text = "Header in Driving cycle: <AUX_" & Trim(Me.TbID.Text) & ">"
        End If

    End Sub


End Class
