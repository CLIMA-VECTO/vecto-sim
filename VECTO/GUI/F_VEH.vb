Imports System.Collections.Generic

Public Class F_VEH

    Dim VehFile As String


    Public AutoSendTo As Boolean = False
    Public GenDir As String = ""

    Private Changed As Boolean = False

    Private Sub F_VEH_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.ApplicationExitCall And e.CloseReason <> CloseReason.WindowsShutDown Then
            e.Cancel = ChangeCheckCancel()
        End If
    End Sub

    'Init
    Private Sub F05_VEH_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Changed = False

        newVEH()

    End Sub

#Region "Menü / Toolstrip"

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

    'Send to GEN Editor
    Private Sub ToolStripBtSendTo_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSendTo.Click

        If ChangeCheckCancel() Then Exit Sub

        If VehFile = "" Then
            If MsgBox("Save file now?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                If Not SaveOrSaveAs(True) Then Exit Sub
            Else
                Exit Sub
            End If
        End If


        If Not F_GEN.Visible Then
            GenDir = ""
            F_GEN.Show()
            F_GEN.GENnew()
        Else
            F_GEN.WindowState = FormWindowState.Normal
        End If

        F_GEN.TextBoxVEH.Text = fFileWoDir(VehFile, GenDir)

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

#Region "Speichern/Laden/Neue Datei"

    'Speichern oder Speichern als Function = true wenn Datei gespeichert
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

    'Neue VEH
    Private Sub newVEH()
        Dim i As Integer
        Dim lvi As ListViewItem

        If ChangeCheckCancel() Then Exit Sub

        Me.TbMass.Text = ""
        Me.TbLoad.Text = ""
        Me.TbI_wheels.Text = ""
        Me.TBDreifen.Text = ""
        Me.TBcw.Text = ""
        Me.TBAquers.Text = ""
        'Me.TBlhinauf.Text = ""
        'Me.TBlhinunter.Text = ""
        'Me.TBhinauf.Text = ""
        'Me.TBhinunter.Text = ""
        'Me.TBpspar.Text = ""
        'Me.TBpfast.Text = ""

        Me.CbCdMode.SelectedIndex = 0
        Me.TbCdFile.Text = ""

        Me.CbRtType.SelectedIndex = 0
        Me.TbRtRatio.Text = "1"
        Me.TbRtPath.Text = ""

        Me.CbCat.SelectedIndex = 0

        Me.LvRRC.Items.Clear()
        For i = 1 To 2
            lvi = New ListViewItem
            lvi.SubItems(0).Text = i.ToString
            lvi.SubItems.Add("0")
            lvi.SubItems.Add("0")
            LvRRC.Items.Add(lvi)
        Next

        Me.TbMassMax.Text = ""
        Me.TbMassExtra.Text = ""
        Me.CbAxleConfig.SelectedIndex = 0
        'TODO: AUTO: Me.TbHDVclass.Text
        'AUTO: Me.TbLoadingMax.Text


        VehFile = ""
        Me.Text = "VEH Editor"
        Me.LbStatus.Text = ""

        Changed = False

    End Sub

    'VEH öffnen
    Sub openVEH(ByVal file As String)
        Dim i As Int16
        Dim VEH0 As cVEH

        Dim sl As Single()
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
        Me.TbI_wheels.Text = VEH0.I_wheels
        Me.TBDreifen.Text = VEH0.Dreifen
        Me.TBcw.Text = VEH0.Cd0
        Me.TBAquers.Text = VEH0.Aquers
        'Me.TBhinauf.Text = VEH0.hinauf
        'Me.TBhinunter.Text = VEH0.hinunter
        'Me.TBlhinauf.Text = VEH0.lhinauf
        'Me.TBlhinunter.Text = VEH0.lhinunter
        'Me.TBpspar.Text = VEH0.pspar
        'Me.TBpfast.Text = 1 - VEH0.pmodell - VEH0.pspar

        Me.CbCdMode.SelectedIndex = CType(VEH0.CdMode, Integer)
        Me.TbCdFile.Text = VEH0.CdFile.OriginalPath

        Me.CbRtType.SelectedIndex = CType(VEH0.RtType, Integer)
        Me.TbRtRatio.Text = CStr(VEH0.RtRatio)
        Me.TbRtPath.Text = CStr(VEH0.RtFile.OriginalPath)


        Me.CbCat.SelectedIndex = CType(VEH0.VehCat, Integer)

        Me.LvRRC.Items.Clear()
        i = 0
        For Each sl In VEH0.RRCs
            i += 1
            lvi = New ListViewItem
            lvi.SubItems(0).Text = i.ToString
            lvi.SubItems.Add(sl(0))
            lvi.SubItems.Add(sl(1))
            LvRRC.Items.Add(lvi)
        Next

        Me.TbMassMax.Text = VEH0.MassMax
        Me.TbMassExtra.Text = VEH0.MassExtra
        Me.CbAxleConfig.SelectedIndex = CType(VEH0.AxcleConf, Integer)
        'TODO: AUTO: Me.TbHDVclass.Text
        'AUTO: Me.TbLoadingMax.Text

        fbVEH.UpdateHistory(file)
        Me.Text = fFILE(file, True)
        Me.LbStatus.Text = ""
        VehFile = file
        Me.Activate()

        Changed = False

    End Sub

    'VEH speichern
    Private Function saveVEH(ByVal file As String) As Boolean

        Dim VEH0 As cVEH
        Dim LV0 As ListViewItem

        VEH0 = New cVEH
        VEH0.FilePath = file

        VEH0.Mass = CSng(fTextboxToNumString(Me.TbMass.Text))
        VEH0.MassExtra = CSng(fTextboxToNumString(Me.TbMassExtra.Text))
        VEH0.Loading = CSng(fTextboxToNumString(Me.TbLoad.Text))
        VEH0.Cd0 = CSng(fTextboxToNumString(Me.TBcw.Text))
        VEH0.Aquers = CSng(fTextboxToNumString(Me.TBAquers.Text))

        'VEH0.I_mot = CSng(fTextboxToNumString(Me.TBI_mot.Text))
        VEH0.I_wheels = CSng(fTextboxToNumString(Me.TbI_wheels.Text))
        'VEH0.I_Getriebe = CSng(fTextboxToNumString(Me.TBI_getr.Text))
        'VEH0.Paux0 = CSng(fTextboxToNumString(Me.TbPaux0.Text))
        'VEH0.Pnenn = CSng(fTextboxToNumString(Me.TBPnenn.Text))
        'VEH0.nNenn = CSng(fTextboxToNumString(Me.TBnnenn.Text))
        'VEH0.nLeerl = CSng(fTextboxToNumString(Me.TBnleerl.Text))

        'VEH0.fGetr = CSng(fTextboxToNumString(Me.TBfGetr.Text))

        'VEH0.AchsI = CSng(Me.LvGears.Items(0).SubItems(1).Text)
        'VEH0.AchsMap = Me.LvGears.Items(0).SubItems(2).Text

        VEH0.Dreifen = CSng(fTextboxToNumString(Me.TBDreifen.Text))

        'For i = 1 To 16
        '    VEH0.Igetr(i) = CSng(Me.LvGears.Items(i).SubItems(1).Text)
        '    VEH0.GetrMap(i) = Me.LvGears.Items(i).SubItems(2).Text
        'Next

        'VEH0.hinauf = CSng(fTextboxToNumString(Me.TBhinauf.Text))
        'VEH0.hinunter = CSng(fTextboxToNumString(Me.TBhinunter.Text))
        'VEH0.lhinauf = CSng(fTextboxToNumString(Me.TBlhinauf.Text))
        'VEH0.lhinunter = CSng(fTextboxToNumString(Me.TBlhinunter.Text))
        'VEH0.pspar = CSng(fTextboxToNumString(Me.TBpspar.Text))
        'VEH0.pmodell = CSng(1 - CSng(fTextboxToNumString(Me.TBpfast.Text)) - CSng(fTextboxToNumString(Me.TBpspar.Text)))

        'VEH0.TracIntrSi = fTextboxToNumString(Me.TbTracInt.Text)

        VEH0.CdMode = CType(Me.CbCdMode.SelectedIndex, tCdMode)
        VEH0.CdFile.Init(fPATH(file), Me.TbCdFile.Text)

        VEH0.RtType = CType(Me.CbRtType.SelectedIndex, tRtType)
        VEH0.RtRatio = CSng(fTextboxToNumString(Me.TbRtRatio.Text))
        VEH0.RtFile.Init(fPATH(file), Me.TbRtPath.Text)

        VEH0.VehCat = CType(Me.CbCat.SelectedIndex, tVehCat)
        VEH0.RRCs.Clear()
        For Each LV0 In LvRRC.Items
            VEH0.RRCs.Add(New Single() {CSng(LV0.SubItems(1).Text), CSng(LV0.SubItems(2).Text)})
        Next

        VEH0.MassMax = CSng(fTextboxToNumString(Me.TbMassMax.Text))
        VEH0.MassExtra = CSng(fTextboxToNumString(Me.TbMassExtra.Text))
        VEH0.AxcleConf = CType(Me.CbAxleConfig.SelectedIndex, tAxleConf)

        '---------------------------------------------------------------------------------

        If Not VEH0.SaveFile Then
            MsgBox("Cannot safe to " & file, MsgBoxStyle.Critical)
            Return False
        End If

        If Not GenDir = "" Or AutoSendTo Then
            If F_GEN.Visible And UCase(fFileRepl(F_GEN.TextBoxVEH.Text, GenDir)) <> UCase(file) Then F_GEN.TextBoxVEH.Text = fFileWoDir(file, GenDir)
        End If

        fbVEH.UpdateHistory(file)
        Me.Text = fFILE(file, True)
        Me.LbStatus.Text = ""

        Changed = False

        Return True

    End Function

#End Region


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

        Me.TbCdFile.Enabled = bEnabled
        Me.BtCdFileBrowse.Enabled = bEnabled

        Change()
    End Sub

    'Cd File Browse
    Private Sub BtCdFileBrowse_Click(sender As System.Object, e As System.EventArgs) Handles BtCdFileBrowse.Click
        Dim fb As cFileBrowser
        fb = New cFileBrowser("CdFile", False, True)

        If Me.CbCdMode.SelectedIndex = 1 Then
            fb.Extensions = New String() {"vcdv"}
        Else
            fb.Extensions = New String() {"vcdb"}
        End If

        If fb.OpenDialog(fFileRepl(Me.TbCdFile.Text, fPATH(VehFile))) Then TbCdFile.Text = fFileWoDir(fb.Files(0), fPATH(VehFile))

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
        Dim fb As cFileBrowser
        fb = New cFileBrowser("RtFile", False, True)

        If fb.OpenDialog(fFileRepl(Me.TbRtPath.Text, fPATH(VehFile))) Then TbRtPath.Text = fFileWoDir(fb.Files(0), fPATH(VehFile))

    End Sub

#End Region

#Region "Change Events"

    'Change Status ändern
    Private Sub Change()
        If Not Changed Then
            Me.LbStatus.Text = "Unsaved changes in current file"
            Changed = True
        End If
    End Sub

    ' "Save changes ?" ...liefert True wenn User Vorgang abbricht
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

    Private Sub TBmrad_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbI_wheels.TextChanged
        Change()
    End Sub

    Private Sub TBDreifen_TextChanged(sender As System.Object, e As System.EventArgs) Handles TBDreifen.TextChanged
        Change()
    End Sub

    Private Sub TBcw_TextChanged(sender As System.Object, e As System.EventArgs) Handles TBcw.TextChanged
        Change()
    End Sub

    Private Sub TBAquers_TextChanged(sender As System.Object, e As System.EventArgs) Handles TBAquers.TextChanged
        Change()
    End Sub

    Private Sub TBFr0_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBFr1_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBFr2_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBFr3_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBFr4_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBPnenn_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBnnenn_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBnleerl_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBI_mot_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBhinauf_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBhinunter_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBpfast_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBlhinauf_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBlhinunter_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBpspar_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TbCdFile_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbCdFile.TextChanged
        Change()
    End Sub

    Private Sub TBI_getr_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBfGetr_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TbTracInt_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TBP0_TextChanged(sender As System.Object, e As System.EventArgs)
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
    End Sub

    Private Sub TbMassTrailer_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbMassExtra.TextChanged
        SetMaxLoad()
        Change()
    End Sub

    Private Sub TbMassMax_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbMassMax.TextChanged
        SetMaxLoad()
        Change()
    End Sub

    Private Sub CbAxleConfig_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles CbAxleConfig.SelectedIndexChanged
        Change()
    End Sub

#End Region

    Private Sub SetMaxLoad()
        If IsNumeric(Me.TbMass.Text) And IsNumeric(Me.TbMassExtra.Text) And IsNumeric(Me.TbMassMax.Text) Then
            Me.TbLoadingMax.Text = CStr(CSng(Me.TbMassMax.Text) - CSng(Me.TbMass.Text) - CSng(Me.TbMassExtra.Text))
        Else
            Me.TbLoadingMax.Text = ""
        End If
    End Sub

#Region "Axle Configuration"


    Private Sub ButAxlAdd_Click(sender As System.Object, e As System.EventArgs) Handles ButAxlAdd.Click
        Dim dlog As New F_VEH_Axle
        Dim lv0 As ListViewItem

        If dlog.ShowDialog = Windows.Forms.DialogResult.OK Then
            lv0 = New ListViewItem

            lv0.SubItems(0).Text = Me.LvRRC.Items.Count + 1
            lv0.SubItems.Add(Trim(dlog.TbWeight.Text))
            lv0.SubItems.Add(Trim(dlog.TbRRC.Text))

            Me.LvRRC.Items.Add(lv0)

            Change()

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
                RemoveAxleItem()
            Case Keys.Enter
                EditAxleItem()
        End Select
    End Sub

    Private Sub RemoveAxleItem()
        Dim lv0 As ListViewItem
        Dim i As Integer

        If LvRRC.Items.Count = 2 Then Exit Sub

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
        Dim dlog As New F_VEH_Axle

        If LvRRC.SelectedItems.Count = 0 Then Exit Sub

        LV0 = LvRRC.SelectedItems(0)

        dlog.TbWeight.Text = LV0.SubItems(1).Text
        dlog.TbRRC.Text = LV0.SubItems(2).Text

        If dlog.ShowDialog = Windows.Forms.DialogResult.OK Then
            LV0.SubItems(1).Text = dlog.TbWeight.Text
            LV0.SubItems(2).Text = dlog.TbRRC.Text

            Change()

        End If


    End Sub

#End Region
  


End Class