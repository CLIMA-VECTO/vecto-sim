Imports System.Collections.Generic

''' <summary>
''' Vehicle Editor.
''' </summary>
''' <remarks></remarks>
Public Class F_VEH

    Dim AxlDlog As F_VEH_Axle
    Dim VehFile As String
    Public AutoSendTo As Boolean = False
    Public JobDir As String = ""

    Private Changed As Boolean = False


    'Close - Check for unsaved changes
    Private Sub F_VEH_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.ApplicationExitCall And e.CloseReason <> CloseReason.WindowsShutDown Then
            e.Cancel = ChangeCheckCancel()
        End If
    End Sub

    'Initialise form
    Private Sub F05_VEH_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim txt As String

        Me.TbLoadingMax.Text = "-"
        Me.PnLoad.Enabled = Not Cfg.DeclMode
        Me.ButAxlAdd.Enabled = Not Cfg.DeclMode
        Me.ButAxlRem.Enabled = Not Cfg.DeclMode
        Me.CbCdMode.Enabled = Not Cfg.DeclMode
        Me.PnCdARig.Visible = Cfg.DeclMode
        Me.LbCdATr.Visible = Cfg.DeclMode
        Me.PnWheelDiam.Enabled = Not Cfg.DeclMode

        If Cfg.DeclMode Then
            Me.PnCdATrTr.Width = 64
        Else
            Me.PnCdATrTr.Width = 132
        End If

        AxlDlog = New F_VEH_Axle

        Me.CbRim.Items.Add("-")
        For Each txt In Declaration.RimsList
            Me.CbRim.Items.Add(txt)
        Next

        Changed = False

        newVEH()

    End Sub

    'Set HDVclasss
    Private Sub SetHDVclass()
        Dim s0 As cSegmentTableEntry = Nothing
        Dim VehC As tVehCat
        Dim AxlC As tAxleConf
        Dim MaxMass As Single
        Dim HDVclass As String

        VehC = CType(Me.CbCat.SelectedIndex, tVehCat)

        AxlC = CType(Me.CbAxleConfig.SelectedIndex, tAxleConf)

        MaxMass = CSng(fTextboxToNumString(Me.TbMassMass.Text))

        If Declaration.SegmentTable.SetRef(s0, VehC, AxlC, MaxMass) Then
            HDVclass = s0.HDVclass
        Else
            HDVclass = "-"
        End If

        Me.TbHDVclass.Text = HDVclass

        Me.PicVehicle.Image = Image.FromFile(Declaration.ConvPicPath(HDVclass, False))

    End Sub


    'Set generic values for Declaration mode
    Private Sub DeclInit()
        Dim VehC As tVehCat
        Dim AxlC As tAxleConf
        Dim MaxMass As Single
        Dim HDVclass As String
        Dim s0 As cSegmentTableEntry = Nothing
        Dim i As Int16
        Dim i0 As Int16
        Dim AxleCount As Int16
        Dim lvi As ListViewItem
        Dim RigEnabled As Boolean
        Dim TrTrEnabled As Boolean
        Dim rdyn As Single

        If Not Cfg.DeclMode Then Exit Sub

        VehC = CType(Me.CbCat.SelectedIndex, tVehCat)

        AxlC = CType(Me.CbAxleConfig.SelectedIndex, tAxleConf)

        MaxMass = CSng(fTextboxToNumString(Me.TbMassMass.Text))

        If Declaration.SegmentTable.SetRef(s0, VehC, AxlC, MaxMass) Then
            HDVclass = s0.HDVclass

            AxleCount = s0.AxleShares(s0.Missions(0)).Count
            i0 = LvRRC.Items.Count

            If AxleCount > i0 Then
                For i = 1 To AxleCount - LvRRC.Items.Count
                    lvi = New ListViewItem
                    lvi.SubItems(0).Text = (i + i0).ToString
                    lvi.SubItems.Add("-")
                    lvi.SubItems.Add("no")
                    lvi.SubItems.Add("")
                    lvi.SubItems.Add("")
                    lvi.SubItems.Add("-")
                    lvi.SubItems.Add("-")
                    LvRRC.Items.Add(lvi)
                Next

            ElseIf AxleCount < LvRRC.Items.Count Then
                For i = AxleCount To LvRRC.Items.Count - 1
                    LvRRC.Items.RemoveAt(LvRRC.Items.Count - 1)
                    'LvRRC.Items(i).ForeColor = Color.Red
                Next
            End If

            If s0.LongHaulRigidTrailer Then

                RigEnabled = True
                TrTrEnabled = True

            Else

                If s0.VehCat = tVehCat.RigidTruck Then
                    RigEnabled = True
                    TrTrEnabled = False
                Else
                    RigEnabled = False
                    TrTrEnabled = True
                End If

            End If

            Me.PnAll.Enabled = True

        Else
            Me.PnAll.Enabled = False
            HDVclass = "-"
            RigEnabled = False
            TrTrEnabled = False
        End If


        If RigEnabled Then
            Me.PnCdARig.Enabled = True
            If Me.TBcwRig.Text = "-" Then Me.TBcwRig.Text = ""
            If Me.TBAquersRig.Text = "-" Then Me.TBAquersRig.Text = ""
        Else
            Me.PnCdARig.Enabled = False
            Me.TBcwRig.Text = "-"
            Me.TBAquersRig.Text = "-"
        End If

        If TrTrEnabled Then
            Me.PnCdATrTr.Enabled = True
            If Me.TBcdTrTr.Text = "-" Then Me.TBcdTrTr.Text = ""
            If Me.TBAquersTrTr.Text = "-" Then Me.TBAquersTrTr.Text = ""
        Else
            Me.PnCdATrTr.Enabled = False
            Me.TBcdTrTr.Text = "-"
            Me.TBAquersTrTr.Text = "-"
        End If

        Me.TbMassExtra.Text = "-"
        Me.TbLoad.Text = "-"
        Me.CbCdMode.SelectedIndex = 1

        If Me.LvRRC.Items.Count > 0 Then
            rdyn = Declaration.rdyn(Me.LvRRC.Items(1).SubItems(5).Text, Me.CbRim.Text)
        Else
            rdyn = -1
        End If

        If rdyn < 0 Then
            Me.TBrdyn.Text = "-"
        Else
            Me.TBrdyn.Text = rdyn
        End If

    End Sub



#Region "Toolbar"

    'New
    Private Sub ToolStripBtNew_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtNew.Click
        newVEH()
    End Sub

    'Open
    Private Sub ToolStripBtOpen_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtOpen.Click
        If fbVEH.OpenDialog(VehFile) Then openVEH(fbVEH.Files(0))
    End Sub

    'Save
    Private Sub ToolStripBtSave_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSave.Click
        SaveOrSaveAs(False)
    End Sub

    'Save As
    Private Sub ToolStripBtSaveAs_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSaveAs.Click
        SaveOrSaveAs(True)
    End Sub

    'Send to VECTO Editor
    Private Sub ToolStripBtSendTo_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSendTo.Click

        If ChangeCheckCancel() Then Exit Sub

        If VehFile = "" Then
            If MsgBox("Save file now?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                If Not SaveOrSaveAs(True) Then Exit Sub
            Else
                Exit Sub
            End If
        End If


        If Not F_VECTO.Visible Then
            JobDir = ""
            F_VECTO.Show()
            F_VECTO.VECTOnew()
        Else
            F_VECTO.WindowState = FormWindowState.Normal
        End If

        F_VECTO.TbVEH.Text = fFileWoDir(VehFile, JobDir)

    End Sub

    'Help
    Private Sub ToolStripButton1_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButton1.Click
        If IO.File.Exists(MyAppPath & "User Manual\GUI\VEH-Editor.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\GUI\VEH-Editor.html")
        Else
            MsgBox("User Manual not found!", MsgBoxStyle.Critical)
        End If
    End Sub

#End Region

    'Save and Close
    Private Sub ButOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOK.Click
        If SaveOrSaveAs(False) Then Me.Close()
    End Sub

    'Cancel
    Private Sub ButCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButCancel.Click
        Me.Close()
    End Sub

    'Save or Save As function = true if file is saved
    Private Function SaveOrSaveAs(ByVal SaveAs As Boolean) As Boolean
        If VehFile = "" Or SaveAs Then
            If fbVEH.SaveDialog(VehFile) Then
                VehFile = fbVEH.Files(0)
            Else
                Return False
            End If
        End If
        Return saveVEH(VehFile)
    End Function

    'New VEH
    Private Sub newVEH()

        If ChangeCheckCancel() Then Exit Sub

        Me.TbMass.Text = ""
        Me.TbLoad.Text = ""
        Me.TBrdyn.Text = ""
        Me.TBcdTrTr.Text = ""
        Me.TBAquersTrTr.Text = ""
        Me.TBcwRig.Text = ""
        Me.TBAquersRig.Text = ""

        Me.CbCdMode.SelectedIndex = 0
        Me.TbCdFile.Text = ""

        Me.CbRtType.SelectedIndex = 0
        Me.TbRtRatio.Text = "1"
        Me.TbRtPath.Text = ""

        Me.CbCat.SelectedIndex = 0

        Me.LvRRC.Items.Clear()

        Me.TbMassMass.Text = ""
        Me.TbMassExtra.Text = ""
        Me.CbAxleConfig.SelectedIndex = 0

        Me.CbRim.SelectedIndex = 0


        DeclInit()



        VehFile = ""
        Me.Text = "VEH Editor"
        Me.LbStatus.Text = ""

        Changed = False

    End Sub

    'Open VEH
    Sub openVEH(ByVal file As String)
        Dim i As Int16
        Dim VEH0 As cVEH
        Dim inertia As Single

        Dim a0 As cVEH.cAxle
        Dim lvi As ListViewItem

        If ChangeCheckCancel() Then Exit Sub

        VEH0 = New cVEH

        VEH0.FilePath = file

        If Not VEH0.ReadFile Then
            MsgBox("Cannot read " & file & "!")
            Exit Sub
        End If

        Me.TbMass.Text = VEH0.Mass
        Me.TbMassExtra.Text = VEH0.MassExtra
        Me.TbLoad.Text = VEH0.Loading
        Me.TBrdyn.Text = VEH0.rdyn
        Me.CbRim.Text = VEH0.Rim


        Me.CbCdMode.SelectedIndex = CType(VEH0.CdMode, Integer)
        Me.TbCdFile.Text = VEH0.CdFile.OriginalPath

        Me.CbRtType.SelectedIndex = CType(VEH0.RtType, Integer)
        Me.TbRtRatio.Text = CStr(VEH0.RtRatio)
        Me.TbRtPath.Text = CStr(VEH0.RtFile.OriginalPath)


        Me.CbCat.SelectedIndex = CType(VEH0.VehCat, Integer)


        Me.LvRRC.Items.Clear()
        i = 0
        For Each a0 In VEH0.Axles
            i += 1
            lvi = New ListViewItem
            lvi.SubItems(0).Text = i.ToString

            If Cfg.DeclMode Then
                lvi.SubItems.Add("-")
            Else
                lvi.SubItems.Add(a0.Share)
            End If

            If a0.TwinTire Then
                lvi.SubItems.Add("yes")
            Else
                lvi.SubItems.Add("no")
            End If
            lvi.SubItems.Add(a0.RRC)
            lvi.SubItems.Add(a0.FzISO)
            lvi.SubItems.Add(a0.Wheels)

            If Cfg.DeclMode Then
                inertia = Declaration.WheelsInertia(a0.Wheels)
                If inertia < 0 Then
                    lvi.SubItems.Add("-")
                Else
                    lvi.SubItems.Add(inertia)
                End If
            Else
                lvi.SubItems.Add(a0.Inertia)
            End If

            LvRRC.Items.Add(lvi)
        Next

        Me.TbMassMass.Text = VEH0.MassMax
        Me.TbMassExtra.Text = VEH0.MassExtra

        Me.CbAxleConfig.SelectedIndex = CType(VEH0.AxleConf, Integer)

        Me.TBcdTrTr.Text = VEH0.Cd0Tr
        Me.TBAquersTrTr.Text = VEH0.Aquers0Tr
        Me.TBcwRig.Text = VEH0.Cd0Rig
        Me.TBAquersRig.Text = VEH0.Aquers0Rig

        DeclInit()

        fbVEH.UpdateHistory(file)
        Me.Text = fFILE(file, True)
        Me.LbStatus.Text = ""
        VehFile = file
        Me.Activate()

        Changed = False

        If VEH0.NoJSON Then
            If MsgBox("File is not in JSON format!" & vbCrLf & vbCrLf & "Convert now?" & vbCrLf & "(Backup will be created with '.ORIG' extension)", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                IO.File.Copy(file, file & ".ORIG", True)
                SaveOrSaveAs(False)
            End If
        End If

    End Sub

    'Save VEH
    Private Function saveVEH(ByVal file As String) As Boolean
        Dim a0 As cVEH.cAxle
        Dim VEH0 As cVEH
        Dim LV0 As ListViewItem

        VEH0 = New cVEH
        VEH0.FilePath = file

        VEH0.Mass = CSng(fTextboxToNumString(Me.TbMass.Text))
        VEH0.MassExtra = CSng(fTextboxToNumString(Me.TbMassExtra.Text))
        VEH0.Loading = CSng(fTextboxToNumString(Me.TbLoad.Text))
        VEH0.Cd0Tr = CSng(fTextboxToNumString(Me.TBcdTrTr.Text))
        VEH0.Aquers0Tr = CSng(fTextboxToNumString(Me.TBAquersTrTr.Text))
        VEH0.Cd0Rig = CSng(fTextboxToNumString(Me.TBcwRig.Text))
        VEH0.Aquers0Rig = CSng(fTextboxToNumString(Me.TBAquersRig.Text))

        VEH0.Rim = Me.CbRim.Text

        VEH0.rdyn = CSng(fTextboxToNumString(Me.TBrdyn.Text))


        VEH0.CdMode = CType(Me.CbCdMode.SelectedIndex, tCdMode)
        VEH0.CdFile.Init(fPATH(file), Me.TbCdFile.Text)

        VEH0.RtType = CType(Me.CbRtType.SelectedIndex, tRtType)
        VEH0.RtRatio = CSng(fTextboxToNumString(Me.TbRtRatio.Text))
        VEH0.RtFile.Init(fPATH(file), Me.TbRtPath.Text)

        VEH0.VehCat = CType(Me.CbCat.SelectedIndex, tVehCat)

        For Each LV0 In LvRRC.Items

            a0 = New cVEH.cAxle

            a0.Share = fTextboxToNumString(LV0.SubItems(1).Text)
            a0.TwinTire = (LV0.SubItems(2).Text = "yes")
            a0.RRC = fTextboxToNumString(LV0.SubItems(3).Text)
            a0.FzISO = fTextboxToNumString(LV0.SubItems(4).Text)
            a0.Wheels = LV0.SubItems(5).Text
            a0.Inertia = fTextboxToNumString(LV0.SubItems(6).Text)

            VEH0.Axles.Add(a0)

        Next

        VEH0.MassMax = CSng(fTextboxToNumString(Me.TbMassMass.Text))
        VEH0.MassExtra = CSng(fTextboxToNumString(Me.TbMassExtra.Text))
        VEH0.AxleConf = CType(Me.CbAxleConfig.SelectedIndex, tAxleConf)

        '---------------------------------------------------------------------------------

        If Not VEH0.SaveFile Then
            MsgBox("Cannot safe to " & file, MsgBoxStyle.Critical)
            Return False
        End If

        If AutoSendTo Then
            If F_VECTO.Visible Then
                If UCase(fFileRepl(F_VECTO.TbVEH.Text, JobDir)) <> UCase(file) Then F_VECTO.TbVEH.Text = fFileWoDir(file, JobDir)
                F_VECTO.UpdatePic()
            End If
        End If

        fbVEH.UpdateHistory(file)
        Me.Text = fFILE(file, True)
        Me.LbStatus.Text = ""

        Changed = False

        Return True

    End Function

#Region "Cd"

    'Cd Mode Change
    Private Sub CbCdMode_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles CbCdMode.SelectedIndexChanged
        Dim bEnabled As Boolean

        Select Case CType(Me.CbCdMode.SelectedIndex, tCdMode)

            Case tCdMode.ConstCd0
                bEnabled = False
                Me.LbCdMode.Text = ""

            Case tCdMode.CdOfV
                bEnabled = True
                Me.LbCdMode.Text = "Input file: Vehicle Speed [km/h], Cd Scaling Factor [-]"

            Case Else ' tCdMode.CdOfBeta
                bEnabled = True
                Me.LbCdMode.Text = "Input file: Yaw Angle [°], Cd Scaling Factor [-]"

        End Select

        If Not Cfg.DeclMode Then
            Me.TbCdFile.Enabled = bEnabled
            Me.BtCdFileBrowse.Enabled = bEnabled
            Me.BtCdFileOpen.Enabled = bEnabled
        End If

        Change()
    End Sub

    'Cd File Browse
    Private Sub BtCdFileBrowse_Click(sender As System.Object, e As System.EventArgs) Handles BtCdFileBrowse.Click
        Dim ex As String

        If Me.CbCdMode.SelectedIndex = 1 Then
            ex = "vcdv"
        Else
            ex = "vcdb"
        End If

        If fbCDx.OpenDialog(fFileRepl(Me.TbCdFile.Text, fPATH(VehFile)), False, ex) Then TbCdFile.Text = fFileWoDir(fbCDx.Files(0), fPATH(VehFile))

    End Sub

    'Open Cd File
    Private Sub BtCdFileOpen_Click(sender As System.Object, e As System.EventArgs) Handles BtCdFileOpen.Click
        OpenFiles(fFileRepl(Me.TbCdFile.Text, fPATH(VehFile)))
    End Sub

#End Region

#Region "Retarder"

    'Rt Type Change
    Private Sub CbRtType_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles CbRtType.SelectedIndexChanged
        Select Case Me.CbRtType.SelectedIndex
            Case 1 'Primary
                Me.LbRtRatio.Text = "Ratio to engine speed"
                Me.TbRtPath.Enabled = True
                Me.BtRtBrowse.Enabled = True
                Me.PnRt.Enabled = True
            Case 2 'Secondary
                Me.LbRtRatio.Text = "Ratio to cardan shaft speed"
                Me.TbRtPath.Enabled = True
                Me.BtRtBrowse.Enabled = True
                Me.PnRt.Enabled = True
            Case Else '0 None
                Me.LbRtRatio.Text = "Ratio"
                Me.TbRtPath.Enabled = False
                Me.BtRtBrowse.Enabled = False
                Me.PnRt.Enabled = False
        End Select

        Change()

    End Sub

    'Rt File Browse
    Private Sub BtRtBrowse_Click(sender As System.Object, e As System.EventArgs) Handles BtRtBrowse.Click

        If fbRLM.OpenDialog(fFileRepl(Me.TbRtPath.Text, fPATH(VehFile))) Then TbRtPath.Text = fFileWoDir(fbRLM.Files(0), fPATH(VehFile))

    End Sub

#End Region

#Region "Track changes"

    Private Sub Change()
        If Not Changed Then
            Me.LbStatus.Text = "Unsaved changes in current file"
            Changed = True
        End If
    End Sub

    ' "Save changes? "... Returns True if user aborts
    Private Function ChangeCheckCancel() As Boolean

        If Changed Then
            Select Case MsgBox("Save changes ?", MsgBoxStyle.YesNoCancel)
                Case MsgBoxResult.Yes
                    Return Not SaveOrSaveAs(False)
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

    Private Sub TBmass_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbMass.TextChanged
        SetMaxLoad()
        Change()
    End Sub

    Private Sub TBston_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbLoad.TextChanged
        Change()
    End Sub

    Private Sub TBDreifen_TextChanged(sender As System.Object, e As System.EventArgs) Handles TBrdyn.TextChanged
        Change()
    End Sub

    Private Sub CbRim_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles CbRim.SelectedIndexChanged
        Change()
        DeclInit()
    End Sub

    Private Sub TBcw_TextChanged(sender As System.Object, e As System.EventArgs) Handles TBcdTrTr.TextChanged, TBcwRig.TextChanged
        Change()
    End Sub

    Private Sub TBAquers_TextChanged(sender As System.Object, e As System.EventArgs) Handles TBAquersTrTr.TextChanged, TBAquersRig.TextChanged
        Change()
    End Sub


    Private Sub TbCdFile_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbCdFile.TextChanged
        Change()
    End Sub

    Private Sub TbRtPath_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbRtPath.TextChanged
        Change()
    End Sub

    Private Sub TbRtRatio_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbRtRatio.TextChanged
        Change()
    End Sub

    Private Sub CbCat_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles CbCat.SelectedIndexChanged
        Change()
        SetHDVclass()
        DeclInit()
    End Sub

    Private Sub TbMassTrailer_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbMassExtra.TextChanged
        SetMaxLoad()
        Change()
    End Sub

    Private Sub TbMassMax_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbMassMass.TextChanged
        SetMaxLoad()
        Change()
        SetHDVclass()
        DeclInit()
    End Sub

    Private Sub CbAxleConfig_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles CbAxleConfig.SelectedIndexChanged
        Change()
        SetHDVclass()
        DeclInit()
    End Sub

#End Region

    'Update maximum load when truck/trailer mass was changed
    Private Sub SetMaxLoad()
        If Not Cfg.DeclMode Then
            If IsNumeric(Me.TbMass.Text) And IsNumeric(Me.TbMassExtra.Text) And IsNumeric(Me.TbMassMass.Text) Then
                Me.TbLoadingMax.Text = CStr(CSng(Me.TbMassMass.Text) * 1000 - CSng(Me.TbMass.Text) - CSng(Me.TbMassExtra.Text))
            Else
                Me.TbLoadingMax.Text = ""
            End If
        End If
    End Sub

#Region "Axle Configuration"

    Private Sub ButAxlAdd_Click(sender As System.Object, e As System.EventArgs) Handles ButAxlAdd.Click
        Dim lv0 As ListViewItem

        AxlDlog.Clear()

        If AxlDlog.ShowDialog = Windows.Forms.DialogResult.OK Then
            lv0 = New ListViewItem

            lv0.SubItems(0).Text = Me.LvRRC.Items.Count + 1
            lv0.SubItems.Add(Trim(AxlDlog.TbAxleShare.Text))
            If AxlDlog.CbTwinT.Checked Then
                lv0.SubItems.Add("yes")
            Else
                lv0.SubItems.Add("no")
            End If
            lv0.SubItems.Add(Trim(AxlDlog.TbRRC.Text))
            lv0.SubItems.Add(Trim(AxlDlog.TbFzISO.Text))
            lv0.SubItems.Add(Trim(AxlDlog.CbWheels.Text))
            lv0.SubItems.Add(Trim(AxlDlog.TbI_wheels.Text))

            Me.LvRRC.Items.Add(lv0)

            Change()
            DeclInit()

        End If


    End Sub

    Private Sub ButAxlRem_Click(sender As System.Object, e As System.EventArgs) Handles ButAxlRem.Click
        RemoveAxleItem()
    End Sub

    Private Sub LvAxle_DoubleClick(sender As Object, e As System.EventArgs) Handles LvRRC.DoubleClick
        EditAxleItem()
    End Sub

    Private Sub LvAxle_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles LvRRC.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete, Keys.Back
                If Not Cfg.DeclMode Then RemoveAxleItem()
            Case Keys.Enter
                EditAxleItem()
        End Select
    End Sub

    Private Sub RemoveAxleItem()
        Dim lv0 As ListViewItem
        Dim i As Integer

        If LvRRC.SelectedItems.Count = 0 Then
            If LvRRC.Items.Count = 0 Then
                Exit Sub
            Else
                LvRRC.Items(LvRRC.Items.Count - 1).Selected = True
            End If
        End If

        LvRRC.SelectedItems(0).Remove()

        If LvRRC.Items.Count > 0 Then

            i = 0
            For Each lv0 In LvRRC.Items
                i += 1
                lv0.SubItems(0).Text = i.ToString
            Next

            LvRRC.Items(LvRRC.Items.Count - 1).Selected = True
            LvRRC.Focus()
        End If

        Change()

    End Sub

    Private Sub EditAxleItem()
        Dim LV0 As ListViewItem

        If LvRRC.SelectedItems.Count = 0 Then Exit Sub

        LV0 = LvRRC.SelectedItems(0)

        AxlDlog.TbAxleShare.Text = LV0.SubItems(1).Text
        AxlDlog.CbTwinT.Checked = (LV0.SubItems(2).Text = "yes")
        AxlDlog.TbRRC.Text = LV0.SubItems(3).Text
        AxlDlog.TbFzISO.Text = LV0.SubItems(4).Text
        AxlDlog.TbI_wheels.Text = LV0.SubItems(6).Text
        AxlDlog.CbWheels.Text = LV0.SubItems(5).Text

        If AxlDlog.ShowDialog = Windows.Forms.DialogResult.OK Then
            LV0.SubItems(1).Text = AxlDlog.TbAxleShare.Text
            If AxlDlog.CbTwinT.Checked Then
                LV0.SubItems(2).Text = "yes"
            Else
                LV0.SubItems(2).Text = "no"
            End If
            LV0.SubItems(3).Text = AxlDlog.TbRRC.Text
            LV0.SubItems(4).Text = AxlDlog.TbFzISO.Text
            LV0.SubItems(5).Text = AxlDlog.CbWheels.Text
            LV0.SubItems(6).Text = AxlDlog.TbI_wheels.Text

            Change()
            DeclInit()

        End If


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
