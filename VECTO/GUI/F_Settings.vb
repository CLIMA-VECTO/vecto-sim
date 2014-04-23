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

''' <summary>
''' Settings form
''' </summary>
''' <remarks></remarks>
Public Class F_Settings

    'Initialize - load config
    Private Sub F03_Options_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        'Load Config
        Me.TextBoxLogSize.Text = Cfg.LogSize
        Me.TbAirDensity.Text = CStr(Cfg.AirDensity)
        Me.TbOpenCmd.Text = Cfg.OpenCmd
        Me.TbOpenCmdName.Text = Cfg.OpenCmdName
        Me.TbFuelDens.Text = Cfg.FuelDens.ToString
        Me.TbCO2toFC.Text = Cfg.CO2perFC.ToString

        Me.GrCalc.Enabled = Not Cfg.DeclMode

    End Sub

    'Reset Button
    Private Sub ButReset_Click(sender As System.Object, e As System.EventArgs) Handles ButReset.Click
        If MsgBox("This will reset all application settings including the Options Tab. Filehistory will not be deleted." & vbCrLf & vbCrLf & "Continue ?", MsgBoxStyle.YesNo, "Reset Application Settings") = MsgBoxResult.Yes Then
            Cfg.SetDefault()
            If Cfg.DeclMode Then Cfg.DeclInit()
            F_MAINForm.LoadOptions()
            Me.Close()
        End If
    End Sub

    'Save and close
    Private Sub ButtonOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonOK.Click
        Cfg.LogSize = CSng(Me.TextBoxLogSize.Text)
        Cfg.AirDensity = CSng(Me.TbAirDensity.Text)
        Cfg.OpenCmd = Me.TbOpenCmd.Text
        Cfg.OpenCmdName = Me.TbOpenCmdName.Text
        Cfg.FuelDens = CSng(Me.TbFuelDens.Text)
        Cfg.CO2perFC = CSng(Me.TbCO2toFC.Text)
        '----------------------------------------------------

        Cfg.ConfigSAVE()

        Me.Close()
    End Sub

    'Cancel
    Private Sub ButtonCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCancel.Click
        Me.Close()
    End Sub

    'Help button
    Private Sub BtHelp_Click(sender As System.Object, e As System.EventArgs) Handles BtHelp.Click
        If IO.File.Exists(MyAppPath & "User Manual\GUI\settings.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\GUI\settings.html")
        Else
            MsgBox("User Manual not found!", MsgBoxStyle.Critical)
        End If
    End Sub

End Class
