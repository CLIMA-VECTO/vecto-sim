Imports System.Collections.Generic

Public Class F_GEN

    Private Genfile As String
    Private Changed As Boolean = False

    Private pgDriver As TabPage

    Private pgDriverON As Boolean = True

    Private AuxDlog As F_VEH_AuxDlog

    'Initialize form (Load Drives, Combo-lists, ...)
    Private Sub F02_GEN_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim x As Int16

        AuxDlog = New F_VEH_AuxDlog

        pgDriver = Me.TabPgDriver

        'Damit Combobox-Inhalte aktuell sind |@@| So Combo-content is current
        For x = 0 To Me.TabControl1.TabCount - 1
            Me.TabControl1.TabPages(x).Show()
        Next

        Me.LvAux.Columns(2).Width = -2

        Changed = False

    End Sub

    'Close
    Private Sub F02_GEN_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.ApplicationExitCall And e.CloseReason <> CloseReason.WindowsShutDown Then
            e.Cancel = ChangeCheckCancel()
        End If
    End Sub


#Region "Tabs"


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

        'Update Form

        'Files -----------------------------
        TextBoxVEH.Text = Gfile.PathVEH(True)
        TbENG.Text = Gfile.PathENG(True)
        TbGBX.Text = Gfile.PathGBX(True)

        'Start/Stop
        Me.ChBStartStop.Checked = Gfile.StartStop
        Me.TBSSspeed.Text = Gfile.StStV
        Me.TBSStime.Text = Gfile.StStT
        Me.TbStStDelay.Text = Gfile.StStDelay

        'VACC
        Me.TbDesMaxFile.Text = Gfile.DesMaxFile(True)

        Me.LvAux.Items.Clear()
        For Each AuxEntryKV In Gfile.AuxPaths
            LV0 = New ListViewItem
            LV0.SubItems(0).Text = AuxEntryKV.Key
            LV0.SubItems.Add(AuxEntryKV.Value.Type)
            LV0.SubItems.Add(AuxEntryKV.Value.Path.OriginalPath)
            LvAux.Items.Add(LV0)
        Next

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

        Genfile = file

        x = Len(file)
        While Mid(file, x, 1) <> "\" And x > 0
            x = x - 1
        End While
        Me.Text = Mid(file, x + 1, Len(file) - x)
        Changed = False
        Me.ToolStripStatusLabelGEN.Text = ""    'file & " opened."


        '-------------------------------------------------------------

        If Gfile.NoJSON Then
            If MsgBox("File is not in JSON format!" & vbCrLf & vbCrLf & "Convert now?" & vbCrLf & "(Backup will be created with '.ORIG' extension)", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                IO.File.Copy(Genfile, Genfile & ".ORIG", True)
                Save()
            End If
        End If

    End Sub

    'GEN save from form
    Private Function GENsave(ByVal file As String) As Boolean

        Dim g As cGEN
        Dim AuxEntry As cVEH.cAuxEntry
        Dim LV0 As ListViewItem
        Dim sb As cSubPath

        g = New cGEN
        g.FilePath = file

        'Files ------------------------------------------------- -----------------

        g.PathVEH = Me.TextBoxVEH.Text
        g.PathENG = Me.TbENG.Text

        For Each LV0 In LvCycles.Items
            sb = New cSubPath
            sb.Init(fPATH(file), LV0.Text)
            g.CycleFiles.Add(sb)
        Next

        g.PathGBX = Me.TbGBX.Text


        'Start/Stop
        g.StartStop = Me.ChBStartStop.Checked
        g.StStV = CSng(fTextboxToNumString(Me.TBSSspeed.Text))
        g.StStT = CSng(fTextboxToNumString(Me.TBSStime.Text))
        g.StStDelay = CInt(fTextboxToNumString(Me.TbStStDelay.Text))

        'a_DesMax
        g.DesMaxFile = Me.TbDesMaxFile.Text

        For Each LV0 In LvAux.Items
            AuxEntry = New cVEH.cAuxEntry
            AuxEntry.Path.Init(fPATH(file), LV0.SubItems(2).Text)
            AuxEntry.Type = LV0.SubItems(1).Text
            g.AuxPaths.Add(LV0.SubItems(0).Text, AuxEntry)
        Next

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

        'Files
        Me.TextBoxVEH.Text = ""
        Me.TbENG.Text = ""
        Me.LvCycles.Items.Clear()
        Me.TbGBX.Text = ""
        Me.TbDesMaxFile.Text = ""

        'Start/Stop
        Me.TBSSspeed.Text = "5"
        Me.TBSStime.Text = "5"
        Me.ChBStartStop.Checked = False

        Me.LvAux.Items.Clear()

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
    Private Sub FormChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
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
   

    Private Sub TbDesMaxFile_TextChanged_1(sender As System.Object, e As System.EventArgs) Handles TbDesMaxFile.TextChanged
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
