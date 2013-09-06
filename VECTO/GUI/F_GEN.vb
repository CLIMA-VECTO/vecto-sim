Imports System.Collections.Generic

Public Class F_GEN

    Private Genfile As String
    Private Changed As Boolean = False
    Private DRIext As String

    Private pgColdSt As TabPage
    Private pgHEV As TabPage
    Private pgMapCr As TabPage
    Private pgTest As TabPage
    Private pgDriver As TabPage

    Private pgColdStON As Boolean = True
    Private pgHevON As Boolean = True
    Private pgMapCrON As Boolean = True
    Private pgStStopON As Boolean = True
    Private pgTestON As Boolean = True
    Private pgDriverON As Boolean = True

    Private MyVehMode As tVehMode

    'Cache Coolant System Simulation
    Private CoolantsimJa As Boolean = False
    Private CoolantSimPath As String = ""

    Private AuxDlog As F_VEH_AuxDlog

    'Initialize form (Load Drives, Combo-lists, ...)
    Private Sub F02_GEN_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim x As Int16

        AuxDlog = New F_VEH_AuxDlog

        pgColdSt = Me.TabPgColdSt
        pgHEV = Me.TabPgHEV
        pgMapCr = Me.TabPgKF
        pgTest = Me.TabPgTEST
        pgDriver = Me.TabPgDriver

        MyVehMode = tVehMode.StandardMode

        'Damit Combobox-Inhalte aktuell sind |@@| So Combo-content is current
        For x = 0 To Me.TabControl1.TabCount - 1
            Me.TabControl1.TabPages(x).Show()
        Next

        Me.LvAux.Columns(2).Width = -2


        HEVCheck()

        Changed = False

    End Sub

    'Close
    Private Sub F02_GEN_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.ApplicationExitCall And e.CloseReason <> CloseReason.WindowsShutDown Then
            e.Cancel = ChangeCheckCancel()
        End If
    End Sub

    'Shown
    Private Sub F_GEN_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown

        If Not DEV.Options("TestOptions").BoolVal Then Me.TabControl1.Controls.Remove(pgTest)

    End Sub


#Region "Schaltflächen Ein-/Ausblenden (und ggf. Change() aufrufen)"
    'Change the DynKor checkbox
    Private Sub CheckBoxDynKor_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxDynKor.CheckedChanged
        Me.ButOpenTRS.Enabled = Me.CheckBoxDynKor.Checked
        Me.TextBoxTRS.Enabled = Me.CheckBoxDynKor.Checked
        Me.ButtonTRS.Enabled = Me.CheckBoxDynKor.Checked
        Call Change()
    End Sub

    'Change the Cold-start checkbox
    Private Sub CheckBoxColdSt_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxColdSt.CheckedChanged
        SetCStab(Me.CheckBoxColdSt.Checked)
        Call Change()
    End Sub

    'Change the SCR checkbox
    Private Sub CheckBoxSCR_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxSCR.CheckedChanged
        Me.ButOpenEXS.Enabled = Me.CheckBoxSCR.Checked
        Me.TextBoxEXS.Enabled = Me.CheckBoxSCR.Checked
        Me.ButtonEXS.Enabled = Me.CheckBoxSCR.Checked
        Call Change()
    End Sub

    Private Sub CbVehMode_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles CbVehMode.SelectedIndexChanged

        Select Case Me.CbVehMode.SelectedIndex
            Case 0
                MyVehMode = tVehMode.StandardMode
            Case 1
                MyVehMode = tVehMode.EngineOnly
            Case 2
                MyVehMode = tVehMode.HEV
            Case Else '3    
                MyVehMode = tVehMode.EV
        End Select

        Select Case MyVehMode
            Case tVehMode.EV
                If Me.ChCreateMap.Checked Then Me.ChCreateMap.Checked = False
                If Me.ChEngAnalysis.Checked Then Me.ChEngAnalysis.Checked = False
                Me.ChCreateMap.Enabled = False
                Me.ChEngAnalysis.Enabled = False
            Case tVehMode.HEV
                If Me.ChCreateMap.Checked Then Me.ChCreateMap.Checked = False
                Me.ChCreateMap.Enabled = False
                Me.ChEngAnalysis.Enabled = True
            Case Else
                Me.ChCreateMap.Enabled = True
                Me.ChEngAnalysis.Enabled = True
        End Select

        ModeCheck()
        HEVCheck()

        Change()

    End Sub

    Private Sub ModeCheck()

        Dim CMmode As Boolean

        If MyVehMode = tVehMode.EngineOnly Then
            DRIext = "npi"
        Else
            If Me.ChEngAnalysis.Checked Or Me.ChCreateMap.Checked Then
                DRIext = "mes"
            Else
                DRIext = "vdri"
            End If
        End If

        CMmode = Me.ChCreateMap.Checked

        SetMapCtab(CMmode)
        Me.TabPgKF.Enabled = CMmode

        Me.ButOpenENG.Enabled = Not CMmode
        Me.TbENG.Enabled = Not CMmode
        Me.ButtonMAP.Enabled = Not CMmode

        If CMmode Then Me.TbENG.Text = ""

    End Sub

    Private Sub HEVCheck()
        Dim HEVja As Boolean
        Dim EVja As Boolean
        If MyVehMode = tVehMode.HEV Then
            EVja = True
            HEVja = True
            SetHEVtab(True)
        ElseIf MyVehMode = tVehMode.EV Then
            EVja = True
            HEVja = False
            SetHEVtab(True)
        Else
            EVja = False
            HEVja = False
            SetHEVtab(False)
        End If
        Me.ButOpenBAT.Enabled = EVja
        Me.ButOpenEMO.Enabled = EVja
        Me.ButOpenEAN.Enabled = HEVja
        Me.ButOpenGET.Enabled = EVja
        Me.ButOpenSTE.Enabled = HEVja
        Me.ButOpenEKF.Enabled = HEVja
        Me.TextBoxBAT.Enabled = EVja
        Me.TextBoxEMO.Enabled = EVja
        Me.TextBoxEAN.Enabled = HEVja
        Me.TextBoxGET.Enabled = EVja
        Me.TextBoxSTE.Enabled = HEVja
        Me.TextBoxEKF.Enabled = HEVja
        Me.ButtonBAT.Enabled = EVja
        Me.ButtonEMO.Enabled = EVja
        Me.ButtonEAN.Enabled = HEVja
        Me.ButtonGET.Enabled = EVja
        Me.ButtonSTE.Enabled = HEVja
        Me.ButtonEKF.Enabled = HEVja

        Me.TbSOCstart.Enabled = EVja
        Me.CbSOCnIter.Enabled = HEVja

        If EVja Then
            Me.ChBStartStop.Checked = False
            Me.CbSOCnIter.Checked = False
        End If

        Me.ChBStartStop.Enabled = Not EVja

    End Sub


    Private Sub ChEngAnalysis_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles ChEngAnalysis.CheckedChanged
        ModeCheck()
        Change()
    End Sub

    Private Sub ChCreateMap_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles ChCreateMap.CheckedChanged
        ModeCheck()
        Change()
    End Sub


#End Region

#Region "Tabs"
    Private Sub SetHEVtab(ByVal OnOff As Boolean)
        If OnOff Then
            If Not pgHevON Then
                pgHevON = True
                Me.TabControl1.TabPages.Insert(1, pgHEV)
            End If
        Else
            If pgHevON Then
                pgHevON = False
                Me.TabControl1.Controls.Remove(pgHEV)
            End If
        End If
    End Sub

    Private Sub SetCStab(ByVal OnOff As Boolean)
        If OnOff Then
            If Not pgColdStON Then
                pgColdStON = True
                Me.TabControl1.TabPages.Insert(1, pgColdSt)
            End If
        Else
            If pgColdStON Then
                pgColdStON = False
                Me.TabControl1.Controls.Remove(pgColdSt)
            End If
        End If
    End Sub

    Private Sub SetMapCtab(ByVal OnOff As Boolean)
        If OnOff Then
            If Not pgMapCrON Then
                pgMapCrON = True
                Me.TabControl1.TabPages.Insert(1, pgMapCr)
            End If
        Else
            If pgMapCrON Then
                pgMapCrON = False
                Me.TabControl1.Controls.Remove(pgMapCr)
            End If
        End If
    End Sub

    Private Sub SetDrivertab(ByVal OnOff As Boolean)
        If OnOff Then
            If Not pgDriverON Then
                pgDriverON = True
                Me.TabControl1.TabPages.Insert(1, pgDriver)
            End If
        Else
            If pgDriverON Then
                pgDriverON = False
                Me.TabControl1.Controls.Remove(pgDriver)
            End If
        End If
    End Sub

#End Region

#Region "Browse Buttons"

    'General
    Private Sub ButtonVEH_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonVEH.Click
        If fbVEH.OpenDialog(fFileRepl(Me.TextBoxVEH.Text, fPATH(Genfile))) Then Me.TextBoxVEH.Text = fFileWoDir(fbVEH.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonMAP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMAP.Click
        If fbENG.OpenDialog(fFileRepl(Me.TbENG.Text, fPATH(Genfile))) Then Me.TbENG.Text = fFileWoDir(fbENG.Files(0), fPATH(Genfile))
    End Sub
  
    Private Sub ButtonFLD_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonGBX.Click
        If fbGBX.OpenDialog(fFileRepl(Me.TbGBX.Text, fPATH(Genfile))) Then Me.TbGBX.Text = fFileWoDir(fbGBX.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonTRS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonTRS.Click
        If fbTRS.OpenDialog(fFileRepl(Me.TextBoxTRS.Text, fPATH(Genfile))) Then Me.TextBoxTRS.Text = fFileWoDir(fbTRS.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonEXS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonEXS.Click
        If fbEXS.OpenDialog(fFileRepl(Me.TextBoxEXS.Text, fPATH(Genfile))) Then Me.TextBoxEXS.Text = fFileWoDir(fbEXS.Files(0), fPATH(Genfile))
    End Sub

    'Cold Start
    Private Sub ButtonMAA_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMAA.Click
        If fbMAA.OpenDialog(fFileRepl(Me.TextBoxMAA.Text, fPATH(Genfile))) Then Me.TextBoxMAA.Text = fFileWoDir(fbMAA.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonMAC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMAC.Click
        If fbMAC.OpenDialog(fFileRepl(Me.TextBoxMAC.Text, fPATH(Genfile))) Then Me.TextBoxMAC.Text = fFileWoDir(fbMAC.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonWUA_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonWUA.Click
        If fbWUA.OpenDialog(fFileRepl(Me.TextBoxWUA.Text, fPATH(Genfile))) Then Me.TextBoxWUA.Text = fFileWoDir(fbWUA.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonWUC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonWUC.Click
        If fbWUC.OpenDialog(fFileRepl(Me.TextBoxWUC.Text, fPATH(Genfile))) Then Me.TextBoxWUC.Text = fFileWoDir(fbWUC.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonCDW_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCDW.Click
        If fbCDW.OpenDialog(fFileRepl(Me.TextBoxCDW.Text, fPATH(Genfile))) Then Me.TextBoxCDW.Text = fFileWoDir(fbCDW.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonATC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonATC.Click
        If fbATC.OpenDialog(fFileRepl(Me.TextBoxATC.Text, fPATH(Genfile))) Then Me.TextBoxATC.Text = fFileWoDir(fbATC.Files(0), fPATH(Genfile))
    End Sub

    'HEV
    Private Sub ButtonBAT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonBAT.Click
        If fbBAT.OpenDialog(fFileRepl(Me.TextBoxBAT.Text, fPATH(Genfile))) Then Me.TextBoxBAT.Text = fFileWoDir(fbBAT.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonEMO_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonEMO.Click
        If fbEMO.OpenDialog(fFileRepl(Me.TextBoxEMO.Text, fPATH(Genfile))) Then Me.TextBoxEMO.Text = fFileWoDir(fbEMO.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonEAN_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonEAN.Click
        If fbEAN.OpenDialog(fFileRepl(Me.TextBoxEAN.Text, fPATH(Genfile))) Then Me.TextBoxEAN.Text = fFileWoDir(fbEAN.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonGET_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonGET.Click
        If fbGET.OpenDialog(fFileRepl(Me.TextBoxGET.Text, fPATH(Genfile))) Then Me.TextBoxGET.Text = fFileWoDir(fbGET.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonSTE_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSTE.Click
        If fbSTE.OpenDialog(fFileRepl(Me.TextBoxSTE.Text, fPATH(Genfile))) Then Me.TextBoxSTE.Text = fFileWoDir(fbSTE.Files(0), fPATH(Genfile))
    End Sub
    Private Sub ButtonEKF_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonEKF.Click
        If fbEKF.OpenDialog(fFileRepl(Me.TextBoxEKF.Text, fPATH(Genfile))) Then Me.TextBoxEKF.Text = fFileWoDir(fbEKF.Files(0), fPATH(Genfile))
    End Sub

    'a_DesMax
    Private Sub BtDesMaxBr_Click_1(sender As System.Object, e As System.EventArgs) Handles BtDesMaxBr.Click
        If fbACC.OpenDialog(fFileRepl(Me.TbDesMaxFile.Text, fPATH(Genfile))) Then Me.TbDesMaxFile.Text = fFileWoDir(fbACC.Files(0), fPATH(Genfile))
    End Sub

    Private Sub BtAccOpen_Click(sender As System.Object, e As System.EventArgs) Handles BtAccOpen.Click
        OpenFiles(fFileRepl(Me.TbDesMaxFile.Text, fPATH(Genfile)))
    End Sub

#End Region

#Region "Open Buttons"

    'General
    Private Sub ButOpenVEH_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenVEH.Click
        Dim f As String
        f = fFileRepl(TextBoxVEH.Text, fPATH(Genfile))

        'Thus Veh-file is returned
        F_VEH.GenDir = fPATH(Genfile)
        F_VEH.AutoSendTo = True

        If Not Trim(f) = "" Then
            If Not IO.File.Exists(f) Then
                MsgBox("File not found!")
                Exit Sub
            End If
        End If

        If Not F_VEH.Visible Then
            F_VEH.Show()
        Else
            If F_VEH.WindowState = FormWindowState.Minimized Then F_VEH.WindowState = FormWindowState.Normal
            F_VEH.BringToFront()
        End If

        If Not Trim(f) = "" Then F_VEH.openVEH(f)

    End Sub

    Private Sub ButOpenENG_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenENG.Click
        Dim f As String
        f = fFileRepl(TbENG.Text, fPATH(Genfile))

        'Thus Veh-file is returned
        F_ENG.GenDir = fPATH(Genfile)
        F_ENG.AutoSendTo = True

        If Not Trim(f) = "" Then
            If Not IO.File.Exists(f) Then
                MsgBox("File not found!")
                Exit Sub
            End If
        End If

        If Not F_ENG.Visible Then
            F_ENG.Show()
        Else
            If F_ENG.WindowState = FormWindowState.Minimized Then F_ENG.WindowState = FormWindowState.Normal
            F_ENG.BringToFront()
        End If

        If Not Trim(f) = "" Then F_ENG.openENG(f)

    End Sub

    Private Sub ButOpenGBX_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenGBX.Click
        Dim f As String
        f = fFileRepl(TbGBX.Text, fPATH(Genfile))

        'Thus Veh-file is returned
        F_GBX.GenDir = fPATH(Genfile)
        F_GBX.AutoSendTo = True

        If Not Trim(f) = "" Then
            If Not IO.File.Exists(f) Then
                MsgBox("File not found!")
                Exit Sub
            End If
        End If

        If Not F_GBX.Visible Then
            F_GBX.Show()
        Else
            If F_GBX.WindowState = FormWindowState.Minimized Then F_GBX.WindowState = FormWindowState.Normal
            F_GBX.BringToFront()
        End If

        If Not Trim(f) = "" Then F_GBX.openGBX(f)

    End Sub

    Private Sub ButOpenTRS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenTRS.Click
        FileOpenAlt(fFileRepl(Me.TextBoxTRS.Text, fPATH(Genfile)))
    End Sub
    Private Sub ButOpenEXS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenEXS.Click
        FileOpenAlt(fFileRepl(Me.TextBoxEXS.Text, fPATH(Genfile)))
    End Sub
    'Cold Start
    Private Sub ButOpenMAA_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenMAA.Click
        FileOpenAlt(fFileRepl(Me.TextBoxMAA.Text, fPATH(Genfile)))
    End Sub
    Private Sub ButOpenMAC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenMAC.Click
        FileOpenAlt(fFileRepl(Me.TextBoxMAC.Text, fPATH(Genfile)))
    End Sub
    Private Sub ButOpenWUA_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenWUA.Click
        FileOpenAlt(fFileRepl(Me.TextBoxWUA.Text, fPATH(Genfile)))
    End Sub
    Private Sub ButOpenWUC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenWUC.Click
        FileOpenAlt(fFileRepl(Me.TextBoxWUC.Text, fPATH(Genfile)))
    End Sub
    Private Sub ButOpenCDW_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenCDW.Click
        FileOpenAlt(fFileRepl(Me.TextBoxCDW.Text, fPATH(Genfile)))
    End Sub
    Private Sub ButOpenATC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenATC.Click
        FileOpenAlt(fFileRepl(Me.TextBoxATC.Text, fPATH(Genfile)))
    End Sub
    'HEV
    Private Sub ButOpenBAT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenBAT.Click
        FileOpenAlt(fFileRepl(Me.TextBoxBAT.Text, fPATH(Genfile)))
    End Sub
    Private Sub ButOpenEMO_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenEMO.Click
        FileOpenAlt(fFileRepl(Me.TextBoxEMO.Text, fPATH(Genfile)))
    End Sub
    Private Sub ButOpenEAN_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenEAN.Click
        FileOpenAlt(fFileRepl(Me.TextBoxEAN.Text, fPATH(Genfile)))
    End Sub
    Private Sub ButOpenGET_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenGET.Click
        FileOpenAlt(fFileRepl(Me.TextBoxGET.Text, fPATH(Genfile)))
    End Sub
    Private Sub ButOpenSTE_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenSTE.Click
        FileOpenAlt(fFileRepl(Me.TextBoxSTE.Text, fPATH(Genfile)))
    End Sub
    Private Sub ButOpenEKF_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenEKF.Click
        FileOpenAlt(fFileRepl(Me.TextBoxEKF.Text, fPATH(Genfile)))
    End Sub
#End Region

#Region "Menüleiste / Toolbar"

    'New
    Private Sub ToolStripBtNew_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtNew.Click
        GENnew()
    End Sub

    'Open
    Private Sub ToolStripBtOpen_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtOpen.Click
        If fbGEN.OpenDialog(Genfile, False, "vecto") Then GENload2Form(fbGEN.Files(0))
    End Sub

    'Save
    Private Sub ToolStripBtSave_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSave.Click
        Save()
    End Sub

    'Save As
    Private Sub ToolStripBtSaveAs_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSaveAs.Click
        If fbGEN.SaveDialog(Genfile) Then Call GENsave(fbGEN.Files(0))
    End Sub


    'Send to GEN List
    Private Sub ToolStripBtSendTo_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSendTo.Click
        If ChangeCheckCancel() Then Exit Sub
        If Genfile = "" Then
            MsgBox("File not found!" & ChrW(10) & ChrW(10) & "Save file and try again.")
        Else
            F_MAINForm.AddToListViewGEN(Genfile)
            'Me.ToolStripStatusLabelGEN.Text = fFILE(Genfile, True) & " sent to GEN List."
        End If
    End Sub

    'Help
    Private Sub ToolStripButton1_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButton1.Click
        If IO.File.Exists(MyAppPath & "User Manual\GUI\VECTO-Editor.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\GUI\VECTO-Editor.html")
        Else
            MsgBox("User Manual not found!", MsgBoxStyle.Critical)
        End If
    End Sub


#End Region

#Region "Datei Funktionen"

    'Save ("Save" or "Save As" when new file)
    Private Function Save() As Boolean
        If Genfile = "" Then
            If fbGEN.SaveDialog("") Then
                Genfile = fbGEN.Files(0)
            Else
                Return False
            End If
        End If
        Return GENsave(Genfile)
    End Function

    'Load GEN in the form
    Public Sub GENload2Form(ByVal file As String)
        Dim x As Int16
        Dim Gfile As cGEN
        Dim AuxEntryKV As KeyValuePair(Of String, cVEH.cAuxEntry)
        Dim LV0 As ListViewItem
        Dim sb As cSubPath

        If ChangeCheckCancel() Then Exit Sub

        GENnew()

        'Read GEN
        Gfile = New cGEN
        Gfile.FilePath = file
        Try
            If Not Gfile.ReadFile() Then
                Gfile = Nothing
                MsgBox("Failed to load " & fFILE(file, True) & "!")
                Exit Sub
            End If
        Catch ex As Exception
            MsgBox("Failed to load " & fFILE(file, True) & "!")
            Exit Sub
        End Try

        If Gfile.NoJSON Then MsgBox("File format is outdated! Save file again to update to current format!")


        'Update Form
        If Gfile.PKWja Then
            Me.ComboBoxVehType.SelectedIndex = 1
        Else
            Me.ComboBoxVehType.SelectedIndex = 0
        End If
        Me.CheckBoxDynKor.Checked = Gfile.dynkorja
        Me.ComboBoxEclass.SelectedIndex = Gfile.eklasse
        Me.ComboBoxGearShift.SelectedIndex = Gfile.izykwael
        If Gfile.ottoJa Then
            Me.ComboBoxEngType.SelectedIndex = 0
        Else
            Me.ComboBoxEngType.SelectedIndex = 1
        End If

        Select Case Gfile.VehMode
            Case tVehMode.StandardMode
                Me.CbVehMode.SelectedIndex = 0
            Case tVehMode.EngineOnly
                Me.CbVehMode.SelectedIndex = 1
            Case tVehMode.HEV
                Me.CbVehMode.SelectedIndex = 2
            Case tVehMode.EV
                Me.CbVehMode.SelectedIndex = 3
        End Select
        Me.ChCreateMap.Checked = Gfile.CreateMap
        Me.ChEngAnalysis.Checked = Gfile.EngAnalysis

        'Map creation -----------------
        Me.TextBoxIncPe.Text = Gfile.Pschrit
        Me.TextBoxIncn.Text = Gfile.nschrit
        Me.CheckBoxGS.Checked = Gfile.MapSchaltja
        Me.TextBoxAvPerofModVal.Text = Gfile.iMsek
        Me.ChCutFull.Checked = Gfile.KFcutFull
        Me.ChCutDrag.Checked = Gfile.KFcutDrag
        Me.ChInsertDrag.Checked = Gfile.KFinsertDrag
        If Gfile.KFDragIntp Then
            Me.CbDragIntp.SelectedIndex = 1
        Else
            Me.CbDragIntp.SelectedIndex = 0
        End If

        'Cold start --------------------------
        If Gfile.kaltst1 Then MsgBox("Cold Start is not supported in this version!")
        Me.CheckBoxColdSt.Checked = False       ' Gfile.kaltst1
        Me.TextBoxTKat.Text = Gfile.tkat1
        Me.TextBoxTKW.Text = Gfile.tkw1
        Me.TextBoxTofSt.Text = Gfile.hsstart

        'Files -----------------------------
        TextBoxVEH.Text = Gfile.PathVEH(True)
        TbENG.Text = Gfile.PathENG(True)



        TbGBX.Text = Gfile.PathGBX(True)
        TextBoxTRS.Text = Gfile.dynspez(True)

        'Cold start
        TextBoxMAA.Text = Gfile.katmap(True)
        TextBoxMAC.Text = Gfile.kwmap(True)
        TextBoxWUA.Text = Gfile.katkurv(True)
        TextBoxWUC.Text = Gfile.kwkurv(True)
        TextBoxCDW.Text = Gfile.cooldown(True)
        TextBoxATC.Text = Gfile.tumgebung(True)

        'HEV
        TextBoxBAT.Text = Gfile.Batfile(True)
        TextBoxEMO.Text = Gfile.Emospez(True)
        TextBoxEAN.Text = Gfile.EANfile(True)
        TextBoxGET.Text = Gfile.Getspez(True)
        TextBoxSTE.Text = Gfile.STEnam(True)
        TextBoxEKF.Text = Gfile.EKFnam(True)
        'EXS
        Me.CheckBoxSCR.Checked = Gfile.EXSja
        TextBoxEXS.Text = Gfile.PathExs(True)

        'Start/Stop
        Me.ChBStartStop.Checked = Gfile.StartStop
        Me.TBSSspeed.Text = Gfile.StStV
        Me.TBSStime.Text = Gfile.StStT
        Me.TbStStDelay.Text = Gfile.StStDelay

        'SOC Start/Iteration
        Me.TbSOCstart.Text = Gfile.SOCstart
        Me.CbSOCnIter.Checked = Gfile.SOCnJa

        'Transm.Loss Model
        Select Case Gfile.TransmModel
            Case tTransLossModel.Basic
                Me.CbTransLossModel.SelectedIndex = 0
            Case tTransLossModel.Detailed
                Me.CbTransLossModel.SelectedIndex = 1
        End Select

        'Coolant Sim
        CoolantsimJa = Gfile.CoolantsimJa
        CoolantSimPath = Gfile.CoolantSimPath(True)

        'a_DesMax
        Me.ChbDesMax.Checked = Gfile.DesMaxJa
        Me.TbDesMaxFile.Text = Gfile.DesMaxFile(True)

        Me.LvAux.Items.Clear()
        For Each AuxEntryKV In Gfile.AuxPaths
            LV0 = New ListViewItem
            LV0.SubItems(0).Text = AuxEntryKV.Key
            LV0.SubItems.Add(AuxEntryKV.Value.Type)
            LV0.SubItems.Add(AuxEntryKV.Value.Path.OriginalPath)
            LvAux.Items.Add(LV0)
        Next

        Me.TBhinauf.Text = Gfile.hinauf
        Me.TBhinunter.Text = Gfile.hinunter
        Me.TBlhinauf.Text = Gfile.lhinauf
        Me.TBlhinunter.Text = Gfile.lhinunter
        Me.TBpspar.Text = Gfile.pspar
        Me.TBpfast.Text = 1 - Gfile.pmodell - Gfile.pspar

        For Each sb In Gfile.CycleFiles
            LV0 = New ListViewItem
            LV0.Text = sb.OriginalPath
            LvCycles.Items.Add(LV0)
        Next

        Me.CbEngOnly.Checked = Gfile.EngOnly

        If Gfile.EcoRollOn Then
            Me.RdEcoRoll.Checked = True
        ElseIf Gfile.OverSpeedOn Then
            Me.RdOverspeed.Checked = True
        Else
            Me.RdOff.Checked = True
        End If
        Me.TbOverspeed.Text = CStr(Gfile.OverSpeed)
        Me.TbUnderSpeed.Text = CStr(Gfile.UnderSpeed)
        Me.TbVmin.Text = CStr(Gfile.vMin)
        Me.CbLookAhead.Checked = Gfile.LookAheadOn
        Me.TbAlookahead.Text = CStr(Gfile.a_lookahead)
        Me.TbVminLA.Text = CStr(Gfile.vMinLA)



        '-------------------------------------------------------------

        Gfile = Nothing

        Genfile = file

        x = Len(file)
        While Mid(file, x, 1) <> "\" And x > 0
            x = x - 1
        End While
        Me.Text = Mid(file, x + 1, Len(file) - x)
        Changed = False
        Me.ToolStripStatusLabelGEN.Text = ""    'file & " opened."

    End Sub

    'GEN save from form
    Private Function GENsave(ByVal file As String) As Boolean

        Dim g As cGEN
        Dim AuxEntry As cVEH.cAuxEntry
        Dim LV0 As ListViewItem
        Dim sb As cSubPath

        g = New cGEN
        g.FilePath = file

        g.PKWja = (Me.ComboBoxVehType.SelectedIndex = 1)
        g.dynkorja = Me.CheckBoxDynKor.Checked
        g.eklasse = Me.ComboBoxEclass.SelectedIndex
        g.izykwael = Me.ComboBoxGearShift.SelectedIndex
        g.ottoJa = (Me.ComboBoxEngType.SelectedIndex = 0)
        g.VehMode = MyVehMode
        g.CreateMap = Me.ChCreateMap.Checked
        g.EngAnalysis = Me.ChEngAnalysis.Checked


        'Map creation ------------------------------------------------ ------
        g.Pschrit = CShort(fTextboxToNumString(Me.TextBoxIncPe.Text))
        g.nschrit = CShort(fTextboxToNumString(Me.TextBoxIncn.Text))
        g.MapSchaltja = Math.Abs(CInt(Me.CheckBoxGS.Checked))
        g.iMsek = CShort(fTextboxToNumString(Me.TextBoxAvPerofModVal.Text))
        g.KFcutFull = Me.ChCutFull.Checked
        g.KFcutDrag = Me.ChCutDrag.Checked
        g.KFinsertDrag = Me.ChInsertDrag.Checked
        g.KFDragIntp = (Me.CbDragIntp.SelectedIndex = 1)

        'Cold start ------------------------------------------------ ---------------
        g.kaltst1 = Me.CheckBoxColdSt.Checked
        g.tkat1 = CSng(fTextboxToNumString(Me.TextBoxTKat.Text))
        g.tkw1 = CSng(fTextboxToNumString(Me.TextBoxTKW.Text))
        g.hsstart = CSng(fTextboxToNumString(Me.TextBoxTofSt.Text))

        'Files ------------------------------------------------- -----------------

        g.PathVEH = Me.TextBoxVEH.Text
        g.PathENG = Me.TbENG.Text

        For Each LV0 In LvCycles.Items
            sb = New cSubPath
            sb.Init(fPATH(file), LV0.Text)
            g.CycleFiles.Add(sb)
        Next

        g.PathGBX = Me.TbGBX.Text
        g.dynspez = Me.TextBoxTRS.Text

        'Cold start
        g.katmap = Me.TextBoxMAA.Text
        g.kwmap = Me.TextBoxMAC.Text
        g.katkurv = Me.TextBoxWUA.Text
        g.kwkurv = Me.TextBoxWUC.Text
        g.cooldown = Me.TextBoxCDW.Text
        g.tumgebung = Me.TextBoxATC.Text

        'HEV
        g.Batfile = Me.TextBoxBAT.Text
        g.Emospez = Me.TextBoxEMO.Text
        g.EANfile = Me.TextBoxEAN.Text
        g.Getspez = Me.TextBoxGET.Text
        g.STEnam = Me.TextBoxSTE.Text
        g.EKFnam = Me.TextBoxEKF.Text

        'EXS
        g.EXSja = Me.CheckBoxSCR.Checked
        g.PathExs = Me.TextBoxEXS.Text

        'Start/Stop
        g.StartStop = Me.ChBStartStop.Checked
        g.StStV = CSng(fTextboxToNumString(Me.TBSSspeed.Text))
        g.StStT = CSng(fTextboxToNumString(Me.TBSStime.Text))
        g.StStDelay = CInt(fTextboxToNumString(Me.TbStStDelay.Text))

        'SOC
        g.SOCnJa = Me.CbSOCnIter.Checked
        g.SOCstart = CSng(fTextboxToNumString(Me.TbSOCstart.Text))

        'Transm.Loss Model
        Select Case Me.CbTransLossModel.SelectedIndex
            Case 0
                g.TransmModel = tTransLossModel.Basic
            Case 1
                g.TransmModel = tTransLossModel.Detailed
        End Select

        'Coolant Sim
        g.CoolantsimJa = CoolantsimJa
        g.CoolantSimPath = CoolantSimPath

        'a_DesMax
        g.DesMaxJa = Me.ChbDesMax.Checked
        g.DesMaxFile = Me.TbDesMaxFile.Text

        For Each LV0 In LvAux.Items
            AuxEntry = New cVEH.cAuxEntry
            AuxEntry.Path.Init(fPATH(file), LV0.SubItems(2).Text)
            AuxEntry.Type = LV0.SubItems(1).Text
            g.AuxPaths.Add(LV0.SubItems(0).Text, AuxEntry)
        Next

        g.hinauf = CSng(fTextboxToNumString(Me.TBhinauf.Text))
        g.hinunter = CSng(fTextboxToNumString(Me.TBhinunter.Text))
        g.lhinauf = CSng(fTextboxToNumString(Me.TBlhinauf.Text))
        g.lhinunter = CSng(fTextboxToNumString(Me.TBlhinunter.Text))
        g.pspar = CSng(fTextboxToNumString(Me.TBpspar.Text))
        g.pmodell = CSng(1 - CSng(fTextboxToNumString(Me.TBpfast.Text)) - CSng(fTextboxToNumString(Me.TBpspar.Text)))

        g.EngOnly = Me.CbEngOnly.Checked

        g.EcoRollOn = RdEcoRoll.Checked
        g.OverSpeedOn = RdOverspeed.Checked
        g.OverSpeed = CSng(fTextboxToNumString(Me.TbOverspeed.Text))
        g.UnderSpeed = CSng(fTextboxToNumString(Me.TbUnderSpeed.Text))
        g.vMin = CSng(fTextboxToNumString(Me.TbVmin.Text))
        g.LookAheadOn = Me.CbLookAhead.Checked
        g.a_lookahead = CSng(fTextboxToNumString(Me.TbAlookahead.Text))
        g.vMinLA = CSng(fTextboxToNumString(Me.TbVminLA.Text))


        '------------------------------------------------------------

        'SAVE
        If Not g.SaveFile Then
            MsgBox("Cannot safe to " & file, MsgBoxStyle.Critical)
            Return False
        End If

        Genfile = file

        file = fFILE(Genfile, True)

        Me.Text = file

        Me.ToolStripStatusLabelGEN.Text = ""

        F_MAINForm.AddToListViewGEN(Genfile)

        Changed = False

        Return True

    End Function

    'New BlankGEN
    Public Sub GENnew()

        If ChangeCheckCancel() Then Exit Sub

        'General-----------------------------
        Me.ComboBoxVehType.SelectedIndex = 0
        Me.CheckBoxDynKor.Checked = False
        Me.ComboBoxEclass.SelectedIndex = 6
        Me.ComboBoxGearShift.SelectedIndex = 2
        Me.ComboBoxEngType.SelectedIndex = 1
        Me.CbVehMode.SelectedIndex = 0
        Me.ChEngAnalysis.Checked = False
        Me.ChCreateMap.Checked = False

        'Map creation -----------------
        Me.TextBoxIncPe.Text = 20
        Me.TextBoxIncn.Text = 20
        Me.CheckBoxGS.Checked = True
        Me.TextBoxAvPerofModVal.Text = 3
        Me.ChCutFull.Checked = True
        Me.ChCutDrag.Checked = True
        Me.ChInsertDrag.Checked = True
        Me.CbDragIntp.SelectedIndex = 1

        'Cold start --------------------------
        Me.CheckBoxColdSt.Checked = False
        Me.TextBoxTKat.Text = 20
        Me.TextBoxTKW.Text = 20
        Me.TextBoxTofSt.Text = 1

        'Files -----------------------------
        'Cold start

        'HEV
        Me.TbSOCstart.Text = "0.5"
        Me.CbSOCnIter.Checked = True

        'SCR
        Me.CheckBoxSCR.Checked = False

        Me.TextBoxVEH.Text = ""
        Me.TbENG.Text = ""
        Me.LvCycles.Items.Clear()
        Me.TbGBX.Text = ""
        Me.TextBoxTRS.Text = ""
        Me.TextBoxMAA.Text = ""
        Me.TextBoxMAC.Text = ""
        Me.TextBoxWUA.Text = ""
        Me.TextBoxWUC.Text = ""
        Me.TextBoxCDW.Text = ""
        Me.TextBoxATC.Text = ""
        Me.TextBoxBAT.Text = ""
        Me.TextBoxEMO.Text = ""
        Me.TextBoxEAN.Text = ""
        Me.TextBoxGET.Text = ""
        Me.TextBoxSTE.Text = ""
        Me.TextBoxEKF.Text = ""
        Me.TextBoxEXS.Text = ""

        'Start/Stop
        Me.TBSSspeed.Text = "5"
        Me.TBSStime.Text = "5"
        Me.ChBStartStop.Checked = False

        'Transm.Loss Model
        Me.CbTransLossModel.SelectedIndex = 1

        'Coolant Sim
        CoolantsimJa = False
        CoolantSimPath = ""

        'a_Desmax
        Me.ChbDesMax.Checked = True
        Me.TbDesMaxFile.Text = ""

        Me.LvAux.Items.Clear()

        Me.TBlhinauf.Text = "0.45"
        Me.TBlhinunter.Text = "0.40"
        Me.TBhinauf.Text = "0.73"
        Me.TBhinunter.Text = "0.51"
        Me.TBpspar.Text = "1"
        Me.TBpfast.Text = "0"

        Me.CbEngOnly.Checked = False

        Me.RdOff.Checked = True
        Me.CbLookAhead.Checked = True
        Me.TbAlookahead.Text = "-0.5"
        Me.TbOverspeed.Text = ""
        Me.TbUnderSpeed.Text = ""
        Me.TbVmin.Text = ""
        Me.TbVminLA.Text = "50"

        '---------------------------------------------------

        Genfile = ""
        Me.Text = "VECTO Editor"
        Me.ToolStripStatusLabelGEN.Text = ""
        Changed = False

    End Sub
#End Region


#Region "Formular Änderungen (Change() Aufruf) und ggf. Tabs ein/ausblenden"

#Region "Event Handler für Formänderungen"

    'Event handler for the form changes
    Private Sub FormChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _
    ComboBoxVehType.SelectedIndexChanged, _
    ComboBoxGearShift.SelectedIndexChanged, _
    ComboBoxEclass.SelectedIndexChanged, _
    TextBoxTKat.TextChanged, _
    TextBoxTKW.TextChanged, _
    TextBoxTofSt.TextChanged, _
    TextBoxIncPe.TextChanged, _
    TextBoxIncn.TextChanged, _
    CheckBoxGS.CheckedChanged, _
    TextBoxAvPerofModVal.TextChanged, _
    ComboBoxEngType.SelectedIndexChanged
        Call Change()
    End Sub


    'TextBox.TextChanged Events => Change()
    Private Sub TextBoxVEH_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxVEH.TextChanged
        Change()
    End Sub
    Private Sub TextBoxMAP_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TbENG.TextChanged
        Change()
    End Sub

    Private Sub TextBoxFLD_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TbGBX.TextChanged
        Change()
    End Sub
    Private Sub TextBoxTRS_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxTRS.TextChanged
        Change()
    End Sub
    Private Sub TextBoxEXS_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxEXS.TextChanged
        Change()
    End Sub
    Private Sub TextBoxMAA_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxMAA.TextChanged
        Change()
    End Sub
    Private Sub TextBoxMAC_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxMAC.TextChanged
        Change()
    End Sub
    Private Sub TextBoxWUA_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxWUA.TextChanged
        Change()
    End Sub
    Private Sub TextBoxWUC_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxWUC.TextChanged
        Change()
    End Sub
    Private Sub TextBoxCDW_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxCDW.TextChanged
        Change()
    End Sub
    Private Sub TextBoxATC_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxATC.TextChanged
        Change()
    End Sub
    Private Sub TextBoxBAT_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxBAT.TextChanged
        Change()
    End Sub
    Private Sub TextBoxEMO_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxEMO.TextChanged
        Change()
    End Sub
    Private Sub TextBoxEAN_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxEAN.TextChanged
        Change()
    End Sub
    Private Sub TextBoxGET_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxGET.TextChanged
        Change()
    End Sub
    Private Sub TextBoxSTE_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxSTE.TextChanged
        Change()
    End Sub
    Private Sub TextBoxEKF_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxEKF.TextChanged
        Change()
    End Sub
    Private Sub CbSOCnIter_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CbSOCnIter.CheckedChanged
        Change()
    End Sub
    Private Sub TbSOCstart_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TbSOCstart.TextChanged
        Change()
    End Sub

    Private Sub CbTransLossModel_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CbTransLossModel.SelectedIndexChanged
        Change()
    End Sub

    Private Sub ChCutFull_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles ChCutFull.CheckedChanged, ChCutDrag.CheckedChanged, ChInsertDrag.CheckedChanged
        Change()
    End Sub

    Private Sub CbDragIntp_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles CbDragIntp.SelectedIndexChanged
        Change()
    End Sub

    Private Sub ChbDesMax_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles ChbDesMax.CheckedChanged
        Change()
        Me.TbDesMaxFile.Enabled = Me.ChbDesMax.Checked
        Me.BtDesMaxBr.Enabled = Me.ChbDesMax.Checked
    End Sub

    Private Sub TbDesMaxFile_TextChanged_1(sender As System.Object, e As System.EventArgs) Handles TbDesMaxFile.TextChanged
        Change()
    End Sub


    Private Sub TBhinauf_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBlhinauf_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBhinunter_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBlhinunter_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBpfast_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBpspar_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBSSspeed_TextChanged(sender As System.Object, e As System.EventArgs) Handles TBSSspeed.TextChanged
        Change()
    End Sub

    Private Sub TBSStime_TextChanged(sender As System.Object, e As System.EventArgs) Handles TBSStime.TextChanged, TbStStDelay.TextChanged
        Change()
    End Sub

    Private Sub TbOverspeed_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbOverspeed.TextChanged
        Change()
    End Sub

    Private Sub TbUnderSpeed_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbUnderSpeed.TextChanged
        Change()
    End Sub

    Private Sub TbVmin_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbVmin.TextChanged, TbVminLA.TextChanged
        Change()
    End Sub

    Private Sub TbAlookahead_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbAlookahead.TextChanged
        Change()
    End Sub


#End Region

    'Change Status Change
    Private Sub Change()
        If Not Changed Then
            Me.ToolStripStatusLabelGEN.Text = "Unsaved changes in current file"
            Changed = True
        End If
    End Sub

    ' "Save changes? "... Returns True if User aborts
    Private Function ChangeCheckCancel() As Boolean

        If Changed Then

            Select Case MsgBox("Save changes ?", MsgBoxStyle.YesNoCancel)
                Case MsgBoxResult.Yes
                    Return Not Save()
                Case MsgBoxResult.Cancel
                    Return True
                Case Else 'MsgBoxResult.No
                    Changed = False
                    Return False
            End Select

        Else

            Return False

        End If

    End Function




#End Region

#Region "Aux Listview"

    Private Sub ButAuxAdd_Click(sender As System.Object, e As System.EventArgs) Handles ButAuxAdd.Click
        Dim LV0 As ListViewItem
        Dim ID As String

        AuxDlog.VehPath = fPATH(Genfile)
        AuxDlog.TbPath.Text = ""
        AuxDlog.CbType.SelectedIndex = -1
        AuxDlog.CbType.Text = ""
        AuxDlog.TbID.Text = ""       '!!! Vorher Type setzen weil ID beim ändern von Type überschrieben wird !!!"

lbDlog:
        If AuxDlog.ShowDialog = Windows.Forms.DialogResult.OK Then

            ID = UCase(Trim(AuxDlog.TbID.Text))

            For Each LV0 In LvAux.Items
                If LV0.SubItems(0).Text = ID Then
                    MsgBox("ID '" & ID & "' already defined!", MsgBoxStyle.Critical)
                    AuxDlog.TbID.SelectAll()
                    AuxDlog.TbID.Focus()
                    GoTo lbDlog
                End If
            Next

            LV0 = New ListViewItem
            LV0.SubItems(0).Text = UCase(Trim(AuxDlog.TbID.Text))
            LV0.SubItems.Add(Trim(AuxDlog.CbType.Text))
            LV0.SubItems.Add(Trim(AuxDlog.TbPath.Text))

            LvAux.Items.Add(LV0)

            Change()

        End If

    End Sub

    Private Sub ButAuxRem_Click(sender As System.Object, e As System.EventArgs) Handles ButAuxRem.Click
        RemoveAuxItem()
    End Sub

    Private Sub LvAux_DoubleClick(sender As Object, e As System.EventArgs) Handles LvAux.DoubleClick
        EditAuxItem()
    End Sub

    Private Sub LvAux_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles LvAux.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete, Keys.Back
                RemoveAuxItem()
            Case Keys.Enter
                EditAuxItem()
        End Select
    End Sub

    Private Sub EditAuxItem()
        Dim LV0 As ListViewItem

        If LvAux.SelectedItems.Count = 0 Then Exit Sub

        LV0 = LvAux.SelectedItems(0)

        AuxDlog.VehPath = fPATH(Genfile)
        AuxDlog.TbPath.Text = LV0.SubItems(2).Text
        AuxDlog.CbType.SelectedIndex = -1
        AuxDlog.CbType.Text = LV0.SubItems(1).Text
        AuxDlog.TbID.Text = LV0.SubItems(0).Text        '!!! Vorher Type setzen weil ID beim ändern von Type überschrieben wird !!!

        If AuxDlog.ShowDialog = Windows.Forms.DialogResult.OK Then
            LV0.SubItems(0).Text = UCase(Trim(AuxDlog.TbID.Text))
            LV0.SubItems(1).Text = Trim(AuxDlog.CbType.Text)
            LV0.SubItems(2).Text = Trim(AuxDlog.TbPath.Text)

            Change()

        End If

    End Sub

    Private Sub RemoveAuxItem()
        Dim i As Integer

        If LvAux.SelectedItems.Count = 0 Then
            If LvAux.Items.Count = 0 Then
                Exit Sub
            Else
                LvAux.Items(LvAux.Items.Count - 1).Selected = True
            End If
        End If

        i = LvAux.SelectedItems(0).Index

        LvAux.SelectedItems(0).Remove()

        If LvAux.Items.Count > 0 Then
            If i < LvAux.Items.Count Then
                LvAux.Items(i).Selected = True
            Else
                LvAux.Items(LvAux.Items.Count - 1).Selected = True
            End If
            LvAux.Focus()
        End If

        Change()

    End Sub

#End Region

    'OK (Save & Close)
    Private Sub ButSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOK.Click
        If Not Save() Then Exit Sub
        Me.Close()
    End Sub

    'Cancel
    Private Sub ButCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButCancel.Click
        Me.Close()
    End Sub

#Region "DRI List"

    Private Sub LvCycles_AfterLabelEdit(sender As Object, e As System.Windows.Forms.LabelEditEventArgs) Handles LvCycles.AfterLabelEdit
        Change()
    End Sub

    Private Sub LvCycles_DoubleClick(sender As Object, e As System.EventArgs) Handles LvCycles.DoubleClick
        If Me.LvCycles.SelectedItems.Count > 0 Then OpenFiles(fFileRepl(Me.LvCycles.SelectedItems(0).SubItems(0).Text, fPATH(Genfile)))
    End Sub

    Private Sub LvCycles_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles LvCycles.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete, Keys.Back
                RemoveCycle()
            Case Keys.Enter
                If Me.LvCycles.SelectedItems.Count > 0 Then Me.LvCycles.SelectedItems(0).BeginEdit()
        End Select
    End Sub
#End Region

    Private Sub BtDRIadd_Click(sender As System.Object, e As System.EventArgs) Handles BtDRIadd.Click
        Dim str As String
        Dim GenDir As String

        GenDir = fPATH(Genfile)

        If fbDRI.OpenDialog("", True) Then

            For Each str In fbDRI.Files
                Me.LvCycles.Items.Add(fFileWoDir(str, GenDir))
            Next

            Change()

        End If

    End Sub

    Private Sub BtDRIrem_Click(sender As System.Object, e As System.EventArgs) Handles BtDRIrem.Click
        RemoveCycle()
    End Sub

    Private Sub RemoveCycle()
        Dim i As Integer

        If LvCycles.SelectedItems.Count = 0 Then
            If LvCycles.Items.Count = 0 Then
                Exit Sub
            Else
                LvCycles.Items(LvCycles.Items.Count - 1).Selected = True
            End If
        End If

        i = LvCycles.SelectedItems(0).Index

        LvCycles.SelectedItems(0).Remove()

        If LvCycles.Items.Count > 0 Then
            If i < LvCycles.Items.Count Then
                LvCycles.Items(i).Selected = True
            Else
                LvCycles.Items(LvCycles.Items.Count - 1).Selected = True
            End If

            LvCycles.Focus()
        End If

        Change()

    End Sub

    Private Sub CbEngOnly_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CbEngOnly.CheckedChanged
        CheckEngOnly()
        Change()
    End Sub

    Private Sub CheckEngOnly()
        Dim OnOff As Boolean

        OnOff = Not CbEngOnly.Checked

        SetDrivertab(OnOff)

        ButOpenVEH.Enabled = OnOff
        TextBoxVEH.Enabled = OnOff
        ButtonVEH.Enabled = OnOff
        ButOpenGBX.Enabled = OnOff
        TbGBX.Enabled = OnOff
        ButtonGBX.Enabled = OnOff
        GrAux.Enabled = OnOff

    End Sub

    Private Sub ChBStartStop_CheckedChanged_1(sender As System.Object, e As System.EventArgs) Handles ChBStartStop.CheckedChanged
        Change()
        Me.PnStartStop.Enabled = Me.ChBStartStop.Checked
    End Sub


#Region "Overspeed / Eco-Roll / Look Ahead"

    Private Sub CbLookAhead_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CbLookAhead.CheckedChanged
        Change()
        Me.PnLookAhead.Enabled = CbLookAhead.Checked

    End Sub

    Private Sub RdOff_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles RdOff.CheckedChanged, RdOverspeed.CheckedChanged, RdEcoRoll.CheckedChanged
        Dim EcoR As Boolean
        Dim Ovr As Boolean

        Change()

        EcoR = Me.RdEcoRoll.Checked
        Ovr = Me.RdOverspeed.Checked

        Me.TbOverspeed.Enabled = Ovr Or EcoR
        Me.Label13.Enabled = Ovr Or EcoR
        Me.Label14.Enabled = Ovr Or EcoR

        Me.TbUnderSpeed.Enabled = EcoR
        Me.Label22.Enabled = EcoR
        Me.Label20.Enabled = EcoR

        Me.TbVmin.Enabled = Ovr Or EcoR
        Me.Label23.Enabled = Ovr Or EcoR
        Me.Label21.Enabled = Ovr Or EcoR

    End Sub

#End Region


#Region "Open File Context Menu"

    Private CmFiles As String()

    Private Sub OpenFiles(ParamArray files() As String)

        If files.Length = 0 Then Exit Sub

        CmFiles = files

        OpenWithToolStripMenuItem.Text = "Open with " & Cfg.OpenCmdName

        CmOpenFile.Show(Cursor.Position)

    End Sub

    Private Sub OpenWithGRAPHiToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles OpenWithGRAPHiToolStripMenuItem.Click
        If Not FileOpenGRAPHi(CmFiles) Then MsgBox("Failed to open file!")
    End Sub

    Private Sub OpenWithToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles OpenWithToolStripMenuItem.Click
        If Not FileOpenAlt(CmFiles(0)) Then MsgBox("Failed to open file!")
    End Sub

    Private Sub ShowInFolderToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ShowInFolderToolStripMenuItem.Click
        If IO.File.Exists(CmFiles(0)) Then
            Try
                System.Diagnostics.Process.Start("explorer", "/select,""" & CmFiles(0) & "")
            Catch ex As Exception
                MsgBox("Failed to open file!")
            End Try
        Else
            MsgBox("File not found!")
        End If
    End Sub

#End Region

    


End Class
