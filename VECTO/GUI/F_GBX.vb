﻿Public Class F_GBX

    Private GbxFile As String = ""
    Public AutoSendTo As Boolean = False
    Public GenDir As String = ""
    Private GearDia As F_VEH_GearDlog

    Private Init As Boolean = False

    Private Changed As Boolean = False

    Private Sub F_GBX_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.ApplicationExitCall And e.CloseReason <> CloseReason.WindowsShutDown Then
            e.Cancel = ChangeCheckCancel()
        End If
    End Sub

    Private Sub F_GBX_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Dim lvi As ListViewItem
        Dim i As Short

        Init = False
        GearDia = New F_VEH_GearDlog

        lvi = New ListViewItem("A")
        lvi.SubItems.Add("")
        lvi.SubItems.Add("")
        Me.LvGears.Items.Add(lvi)

        For i = 1 To 16
            lvi = New ListViewItem(i.ToString("00"))
            lvi.SubItems.Add("")
            lvi.SubItems.Add("")
            Me.LvGears.Items.Add(lvi)
        Next

        Init = True

        Changed = False
        newGBX()

    End Sub

#Region "ToolStrip"

    Private Sub ToolStripBtNew_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtNew.Click
        newGBX()
    End Sub

    Private Sub ToolStripBtOpen_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtOpen.Click
        If fbGBX.OpenDialog(GbxFile) Then openGBX(fbGBX.Files(0))
    End Sub

    Private Sub ToolStripBtSave_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSave.Click
        SaveOrSaveAs(False)
    End Sub

    Private Sub ToolStripBtSaveAs_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSaveAs.Click
        SaveOrSaveAs(True)
    End Sub

    Private Sub ToolStripBtSendTo_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSendTo.Click

        If ChangeCheckCancel() Then Exit Sub

        If GbxFile = "" Then
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

        F_GEN.TbGBX.Text = fFileWoDir(GbxFile, GenDir)

    End Sub

    'Help
    Private Sub ToolStripButton1_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButton1.Click
        If IO.File.Exists(MyAppPath & "User Manual\GUI\GBX-Editor.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\GUI\GBX-Editor.html")
        Else
            MsgBox("User Manual not found!", MsgBoxStyle.Critical)
        End If
    End Sub

#End Region

    Private Sub newGBX()
        Dim lvi As ListViewItem

        If ChangeCheckCancel() Then Exit Sub

        Me.CbGStype.SelectedIndex = 0

        Me.TbName.Text = ""
        Me.TbTracInt.Text = ""
        Me.TBI_getr.Text = ""

        For Each lvi In LvGears.Items
            lvi.SubItems(1).Text = "0"
            lvi.SubItems(2).Text = ""
            lvi.ForeColor = Color.Gray
        Next

        Me.TbShiftPolyFile.Text = ""
        Me.ChSkipGears.Checked = False
        Me.ChShiftInside.Checked = False
        Me.TbTqResv.Text = ""
        Me.TbShiftTime.Text = ""
        Me.TbTqResvStart.Text = ""
        Me.TbStartSpeed.Text = ""
        Me.TbStartAcc.Text = ""

        Me.ChTCon.Checked = False
        Me.TbTCfile.Text = ""
        Me.TbTCrefrpm.Text = ""


        GbxFile = ""
        Me.Text = "GBX Editor"
        Me.LbStatus.Text = ""

        Changed = False

    End Sub

    Public Sub openGBX(ByVal file As String)
        Dim GBX0 As cGBX
        Dim i As Integer

        If ChangeCheckCancel() Then Exit Sub

        GBX0 = New cGBX

        GBX0.FilePath = file

        If Not GBX0.ReadFile Then
            MsgBox("Cannot read " & file & "!")
            Exit Sub
        End If

        Me.TbName.Text = GBX0.ModelName
        Me.TbTracInt.Text = GBX0.TracIntrSi.ToString
        Me.TBI_getr.Text = GBX0.I_Getriebe.ToString

        For i = 0 To 16
            Me.LvGears.Items(i).SubItems(1).Text = GBX0.GetrI(i)
            Me.LvGears.Items(i).SubItems(2).Text = GBX0.GetrMap(i, True)
            If GBX0.GetrI(i) = 0 Then
                Me.LvGears.Items(i).ForeColor = Color.Gray
            Else
                Me.LvGears.Items(i).ForeColor = Color.Black
            End If
        Next

        Me.TbShiftPolyFile.Text = GBX0.gsFile(True)
        Me.ChSkipGears.Checked = GBX0.gs_SkipGears
        Me.TbTqResv.Text = GBX0.gs_TorqueResv.ToString
        Me.TbShiftTime.Text = GBX0.gs_ShiftTime.ToString
        Me.TbTqResvStart.Text = GBX0.gs_TorqueResvStart.ToString
        Me.TbStartSpeed.Text = GBX0.gs_StartSpeed.ToString
        Me.TbStartAcc.Text = GBX0.gs_StartAcc.ToString
        Me.ChShiftInside.Checked = GBX0.gs_ShiftInside

        Me.ChTCon.Checked = GBX0.TCon
        Me.TbTCfile.Text = GBX0.TCfile(True)
        Me.TbTCrefrpm.Text = GBX0.TCrefrpm

        Me.CbGStype.SelectedIndex = CType(GBX0.gs_Type, Integer)


        fbGBX.UpdateHistory(file)
        Me.Text = fFILE(file, True)
        Me.LbStatus.Text = ""
        GbxFile = file
        Me.Activate()

        Changed = False

    End Sub

    'Save or Save As function = true if file is saved
    Private Function SaveOrSaveAs(ByVal SaveAs As Boolean) As Boolean
        If GbxFile = "" Or SaveAs Then
            If fbGBX.SaveDialog(GbxFile) Then
                GbxFile = fbGBX.Files(0)
            Else
                Return False
            End If
        End If
        Return saveGBX(GbxFile)
    End Function

    Private Function saveGBX(ByVal file As String) As Boolean
        Dim GBX0 As cGBX
        Dim i As Int16

        GBX0 = New cGBX
        GBX0.FilePath = file

        GBX0.ModelName = Me.TbName.Text
        If Trim(GBX0.ModelName) = "" Then GBX0.ModelName = "Undefined"

        GBX0.TracIntrSi = fTextboxToNumString(Me.TbTracInt.Text)
        GBX0.I_Getriebe = fTextboxToNumString(Me.TBI_getr.Text)

        For i = 0 To 16
            GBX0.GetrI(i) = CSng(Me.LvGears.Items(i).SubItems(1).Text)
            GBX0.GetrMap(i) = Me.LvGears.Items(i).SubItems(2).Text
        Next

        GBX0.gsFile = Me.TbShiftPolyFile.Text
        GBX0.gs_TorqueResv = fTextboxToNumString(Me.TbTqResv.Text)
        GBX0.gs_SkipGears = Me.ChSkipGears.Checked
        GBX0.gs_ShiftTime = fTextboxToNumString(Me.TbShiftTime.Text)
        GBX0.gs_TorqueResvStart = fTextboxToNumString(Me.TbTqResvStart.Text)
        GBX0.gs_StartSpeed = fTextboxToNumString(Me.TbStartSpeed.Text)
        GBX0.gs_StartAcc = fTextboxToNumString(Me.TbStartAcc.Text)
        GBX0.gs_ShiftInside = Me.ChShiftInside.Checked

        GBX0.gs_Type = CType(Me.CbGStype.SelectedIndex, tGearbox)

        GBX0.TCon = Me.ChTCon.Checked
        GBX0.TCfile = Me.TbTCfile.Text
        GBX0.TCrefrpm = Me.TbTCrefrpm.Text


        If Not GBX0.SaveFile Then
            MsgBox("Cannot safe to " & file, MsgBoxStyle.Critical)
            Return False
        End If

        If Not GenDir = "" Or AutoSendTo Then
            If F_GEN.Visible And UCase(fFileRepl(F_GEN.TbGBX.Text, GenDir)) <> UCase(file) Then F_GEN.TbGBX.Text = fFileWoDir(file, GenDir)
        End If

        fbGBX.UpdateHistory(file)
        Me.Text = fFILE(file, True)
        Me.LbStatus.Text = ""

        Changed = False

        Return True

    End Function

#Region "Change Events"

    'Change Status ändern |@@| Change Status change
    Private Sub Change()
        If Not Changed Then
            Me.LbStatus.Text = "Unsaved changes in current file"
            Changed = True
        End If
    End Sub

    ' "Save changes ?" ...liefert True wenn User Vorgang abbricht |@@| Save changes? "... Returns True if user aborts
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

    Private Sub TbName_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbName.TextChanged
        Change()
    End Sub

    Private Sub TBI_getr_TextChanged(sender As System.Object, e As System.EventArgs) Handles TBI_getr.TextChanged
        Change()
    End Sub

    Private Sub TbTracInt_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbTracInt.TextChanged
        Change()
    End Sub

    Private Sub TbShiftPolyFile_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbShiftPolyFile.TextChanged
        Change()
    End Sub

    Private Sub ChSkipGears_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles ChSkipGears.CheckedChanged
        CheckEnableTorqRes()
        Change()
    End Sub

    Private Sub ChShiftInside_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles ChShiftInside.CheckedChanged
        CheckEnableTorqRes()
        Change()
    End Sub

    Private Sub TbTqResv_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbTqResv.TextChanged
        Change()
    End Sub

    Private Sub TbShiftTime_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbShiftTime.TextChanged
        Change()
    End Sub

    Private Sub TbTqResvStart_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbTqResvStart.TextChanged
        Change()
    End Sub

    Private Sub TbStartSpeed_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbStartSpeed.TextChanged
        Change()
    End Sub

    Private Sub TbStartAcc_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbStartAcc.TextChanged
        Change()
    End Sub

    Private Sub TbTCfile_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbTCfile.TextChanged
        Change()
    End Sub

    Private Sub TbTCrefrpm_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbTCrefrpm.TextChanged
        Change()
    End Sub


    Private Sub CheckEnableTorqRes()
        If Me.ChShiftInside.Checked Or Me.ChSkipGears.Checked Then
            Me.PnTorqRes.Enabled = True
        Else
            Me.PnTorqRes.Enabled = False
        End If
    End Sub




#End Region

    Private Sub ButOK_Click(sender As System.Object, e As System.EventArgs) Handles ButOK.Click
        If SaveOrSaveAs(False) Then Me.Close()
    End Sub

    Private Sub ButCancel_Click(sender As System.Object, e As System.EventArgs) Handles ButCancel.Click
        Me.Close()
    End Sub


    'Enable/Disable settings for specific transmission types
    Private Sub CbGStype_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles CbGStype.SelectedIndexChanged
        Dim GStype As tGearbox

        Change()

        GStype = CType(Me.CbGStype.SelectedIndex, tGearbox)

        Select Case GStype
            Case tGearbox.Manual
                Me.ChShiftInside.Enabled = False
                Me.ChShiftInside.Checked = False
                Me.ChSkipGears.Enabled = False
                Me.ChSkipGears.Checked = True
                Me.ChTCon.Enabled = False
                Me.ChTCon.Checked = False

            Case tGearbox.SemiAutomatic
                Me.ChShiftInside.Enabled = False
                Me.ChShiftInside.Checked = True
                Me.ChSkipGears.Enabled = False
                Me.ChSkipGears.Checked = True
                Me.ChTCon.Enabled = False
                Me.ChTCon.Checked = False

            Case tGearbox.Automatic
                Me.ChShiftInside.Enabled = False
                Me.ChShiftInside.Checked = False
                Me.ChSkipGears.Enabled = False
                Me.ChSkipGears.Checked = False
                Me.ChTCon.Enabled = False
                Me.ChTCon.Checked = True

            Case tGearbox.Custom
                Me.ChShiftInside.Enabled = True
                Me.ChSkipGears.Enabled = True
                Me.ChTCon.Enabled = True

        End Select

    End Sub



#Region "Gears"

    'Gear-DoubleClick
    Private Sub LvGears_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles LvGears.MouseDoubleClick
        EditGear()
    End Sub

    'Gear-KeyDown
    Private Sub LvGears_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles LvGears.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete, Keys.Back
                ClearGear()
            Case Keys.Enter
                EditGear()
        End Select
    End Sub

    'Clear Gear Button
    Private Sub BtClearGear_Click(sender As System.Object, e As System.EventArgs) Handles BtClearGear.Click
        ClearGear()
    End Sub

    'Edit Gear
    Private Sub EditGear()

        Do
            GearDia.GbxPath = fPATH(GbxFile)
            GearDia.TbGear.Text = Me.LvGears.SelectedItems(0).SubItems(0).Text
            GearDia.TbRatio.Text = Me.LvGears.SelectedItems(0).SubItems(1).Text
            GearDia.TbMapPath.Text = Me.LvGears.SelectedItems(0).SubItems(2).Text

            GearDia.BtNext.Enabled = (Me.LvGears.SelectedIndices(0) < Me.LvGears.Items.Count - 1)

            If GearDia.ShowDialog = Windows.Forms.DialogResult.OK Then
                Me.LvGears.SelectedItems(0).SubItems(1).Text = GearDia.TbRatio.Text
                Me.LvGears.SelectedItems(0).SubItems(2).Text = GearDia.TbMapPath.Text
                If GearDia.TbRatio.Text = "0" Then
                    Me.LvGears.SelectedItems(0).ForeColor = Color.Gray
                Else
                    Me.LvGears.SelectedItems(0).ForeColor = Color.Black
                End If

                Change()

            End If

            If GearDia.NextGear Then Me.LvGears.Items(Me.LvGears.SelectedIndices(0) + 1).Selected = True

        Loop Until Not GearDia.NextGear

    End Sub

    'Clear Gear
    Private Sub ClearGear()
        Dim lv0 As ListViewItem

        If Me.LvGears.SelectedItems.Count = 0 Then Exit Sub

        lv0 = Me.LvGears.SelectedItems(0)

        lv0.SubItems(1).Text = "0"
        lv0.SubItems(2).Text = ""
        lv0.ForeColor = Color.Gray

        If lv0.Index < Me.LvGears.Items.Count - 1 Then
            Me.LvGears.Items(lv0.Index + 1).Selected = True
            Me.LvGears.Items(lv0.Index + 1).EnsureVisible()
        End If

        Me.LvGears.Focus()

        Change()

    End Sub


#End Region

    'Browse Shift Polygon File
    Private Sub BtShiftPolyBrowse_Click(sender As System.Object, e As System.EventArgs) Handles BtShiftPolyBrowse.Click
        Dim fb As cFileBrowser
        fb = New cFileBrowser("ShiftPolygon", False, True)
        fb.Extensions = New String() {"vgbs"}
        If fb.OpenDialog(fFileRepl(Me.TbShiftPolyFile.Text, fPATH(GbxFile))) Then
            Me.TbShiftPolyFile.Text = fFileWoDir(fb.Files(0), fPATH(GbxFile))
        End If
    End Sub


    Private Sub BtShiftPolyOpen_Click(sender As System.Object, e As System.EventArgs) Handles BtShiftPolyOpen.Click
        OpenFiles(fFileRepl(Me.TbShiftPolyFile.Text, fPATH(GbxFile)))
    End Sub


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

#Region "Torque Converter"

    'TC on/off
    Private Sub ChTCon_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles ChTCon.CheckedChanged
        Change()
        CheckGearTC()
        PnTC.Enabled = ChTCon.Checked
    End Sub

    'Browse TC file
    Private Sub BtTCfileBrowse_Click(sender As System.Object, e As System.EventArgs) Handles BtTCfileBrowse.Click
        Dim fb As cFileBrowser
        fb = New cFileBrowser("TCfile", False, True)
        fb.Extensions = New String() {"vtcc"}
        If fb.OpenDialog(fFileRepl(Me.TbTCfile.Text, fPATH(GbxFile))) Then
            Me.TbTCfile.Text = fFileWoDir(fb.Files(0), fPATH(GbxFile))
        End If
    End Sub

    'Open TC file
    Private Sub BtTCfileOpen_Click(sender As System.Object, e As System.EventArgs) Handles BtTCfileOpen.Click
        OpenFiles(fFileRepl(Me.TbTCfile.Text, fPATH(GbxFile)))
    End Sub

    Private Sub CheckGearTC()
        Dim i As Short

        If Not Init Then Exit Sub

        If Me.ChTCon.Checked Then

            Me.LvGears.Items(1).SubItems(0).Text = "TC"

            For i = 2 To 16
                Me.LvGears.Items(i).SubItems(0).Text = (i - 1).ToString("00")
            Next

        Else
            For i = 1 To 16
                Me.LvGears.Items(i).SubItems(0).Text = i.ToString("00")
            Next

        End If

    End Sub


#End Region

   

End Class
