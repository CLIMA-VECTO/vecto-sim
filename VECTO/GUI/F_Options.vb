Public Class F_Options

    Dim WD As String = " "

    'Initialize - load config
    Private Sub F03_Options_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        LoadConfig()
    End Sub

    Private Sub F_Options_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        If Not DEV.Options("TestOptions").BoolVal Then Me.TabControl1.Controls.Remove(TabPgTest)
    End Sub

    'Load Config
    Private Sub LoadConfig()
        Me.TextBoxWorDir.Text = Cfg.WorkDPath
        WD = Cfg.WorkDPath
        Me.TextBoxTD.Text = Cfg.TEMpath
        Me.TextBoxEAVal.Text = Cfg.EAAvInt
        Me.TextBoxLogSize.Text = Cfg.LogSize
        Me.TbAirDensity.Text = CStr(Cfg.AirDensity)
        Me.TbnnormEngStop.Text = CStr(Cfg.nnormEngStop)
        Me.TbOpenCmd.Text = Cfg.OpenCmd
        Me.TbOpenCmdName.Text = Cfg.OpenCmdName
        Me.TbFuelDens.Text = Cfg.FuelDens.ToString
        Me.TbCO2toFC.Text = Cfg.CO2perFC.ToString
    End Sub

    'Reset Button
    Private Sub ButReset_Click(sender As System.Object, e As System.EventArgs) Handles ButReset.Click
        If MsgBox("This will reset all application settings including the Options Tab. Filehistory will not be deleted." & vbCrLf & vbCrLf & "Continue ?", MsgBoxStyle.YesNo, "Reset Application Settings") = MsgBoxResult.Yes Then
            Cfg.SetDefault()
            LoadConfig()
            F_MAINForm.LoadOptions()
        End If
    End Sub

    'OK
    Private Sub ButtonOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonOK.Click
        Cfg.SetWorkDir(Me.TextBoxWorDir.Text)
        Cfg.TEMpath = Me.TextBoxTD.Text
        Cfg.EAAvInt = CShort(Me.TextBoxEAVal.Text)
        Cfg.LogSize = CSng(Me.TextBoxLogSize.Text)
        Cfg.AirDensity = CSng(Me.TbAirDensity.Text)
        Cfg.nnormEngStop = CSng(Me.TbnnormEngStop.Text)
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

    Private Sub ButtonTD_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim p0 As String
        If UCase(Trim(Cfg.TEMpath)) = "<DEFAULT>" Then
            p0 = ""
        Else
            p0 = Cfg.TEMpath
        End If
        If fbTEM.OpenDialog(p0, False, "csv") Then
            Me.TextBoxTD.Text = fbTEM.Files(0)
        End If
    End Sub

    Private Sub ButTDdefault_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.TextBoxTD.Text = "<default>"
    End Sub

    Private Sub BtHelp_Click(sender As System.Object, e As System.EventArgs) Handles BtHelp.Click
        If IO.File.Exists(MyAppPath & "User Manual\GUI\settings.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\GUI\settings.html")
        Else
            MsgBox("User Manual not found!", MsgBoxStyle.Critical)
        End If
    End Sub

End Class
