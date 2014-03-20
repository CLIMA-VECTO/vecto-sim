Public Class F_Options

    Dim WD As String = " "

    'Initialize - load config
    Private Sub F03_Options_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        LoadConfig()

        If Declaration.Active Then
            Me.GrCalc.Enabled = False
            Me.TbAirDensity.Text = cDeclaration.AirDensity
            Me.TbFuelDens.Text = cDeclaration.FuelDens
            Me.TbCO2toFC.Text = cDeclaration.CO2perFC
        End If



    End Sub

    'Load Config
    Private Sub LoadConfig()
        Me.TextBoxWorDir.Text = Cfg.WorkDPath
        WD = Cfg.WorkDPath
        Me.TextBoxLogSize.Text = Cfg.LogSize
        Me.TbAirDensity.Text = CStr(Cfg.AirDensity)
        Me.TbOpenCmd.Text = Cfg.OpenCmd
        Me.TbOpenCmdName.Text = Cfg.OpenCmdName
        Me.TbFuelDens.Text = Cfg.FuelDens.ToString
        Me.TbCO2toFC.Text = Cfg.CO2perFC.ToString
    End Sub

    'Reset Button
    Private Sub ButReset_Click(sender As System.Object, e As System.EventArgs) Handles ButReset.Click
        If MsgBox("This will reset all application settings including the Options Tab. Filehistory will not be deleted." & vbCrLf & vbCrLf & "Continue ?", MsgBoxStyle.YesNo, "Reset Application Settings") = MsgBoxResult.Yes Then
            Cfg.SetDefault()
            If Declaration.Active Then Cfg.DeclInit()
            LoadConfig()
            F_MAINForm.LoadOptions()
        End If
    End Sub

    'OK
    Private Sub ButtonOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonOK.Click
        Cfg.SetWorkDir(Me.TextBoxWorDir.Text)
        Cfg.LogSize = CSng(Me.TextBoxLogSize.Text)
        Cfg.AirDensity = CSng(Me.TbAirDensity.Text)
        Cfg.OpenCmd = Me.TbOpenCmd.Text
        Cfg.OpenCmdName = Me.TbOpenCmdName.Text
        Cfg.FuelDens = CSng(Me.TbFuelDens.Text)
        Cfg.CO2perFC = CSng(Me.TbCO2toFC.Text)
        '----------------------------------------------------

        Call Cfg.ConfigSAVE()

        If WD <> Cfg.WorkDPath Then GUImsg(tMsgID.Normal, "Working Directory changed to " & Cfg.WorkDPath)
        Me.Close()
    End Sub

    'Cancel
    Private Sub ButtonCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCancel.Click
        Me.Close()
    End Sub

    'Options-----------------------------------
    Private Sub ButtonWorDir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonWorDir.Click
        If fbWorkDir.OpenDialog(Me.TextBoxWorDir.Text) Then
            Me.TextBoxWorDir.Text = fbWorkDir.Files(0)
        End If
    End Sub

    Private Sub BtHelp_Click(sender As System.Object, e As System.EventArgs) Handles BtHelp.Click
        If IO.File.Exists(MyAppPath & "User Manual\GUI\settings.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\GUI\settings.html")
        Else
            MsgBox("User Manual not found!", MsgBoxStyle.Critical)
        End If
    End Sub

End Class
