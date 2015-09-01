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

Imports System.Collections.Generic

''' <summary>
''' Job Editor. Create/Edit VECTO job files (.vecto)
''' </summary>
''' <remarks></remarks>
Public Class F_VECTO

    Private VECTOfile As String
    Private Changed As Boolean = False

    Private pgDriver As TabPage

    Private pgDriverON As Boolean = True

    Private AuxDlog As F_VEH_AuxDlog

	Private EStechs As New List(Of String)

	Public n_idle As Single
	Public FLDfile As String

    'Initialise form
    Private Sub F02_GEN_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
		Dim x As Int16

		n_idle = -1
		FLDfile = ""

        AuxDlog = New F_VEH_AuxDlog

        pgDriver = Me.TabPgDriver

        For x = 0 To Me.TabControl1.TabCount - 1
            Me.TabControl1.TabPages(x).Show()
        Next

        Me.LvAux.Columns(2).Width = -2

        'Declaration Mode
        If Cfg.DeclMode Then
            Me.LvAux.Columns(2).Text = "Technology"
        Else
            Me.LvAux.Columns(2).Text = "Input File"
        End If

        Me.CbEngOnly.Enabled = Not Cfg.DeclMode
        Me.GrCycles.Enabled = Not Cfg.DeclMode
        Me.GrVACC.Enabled = Not Cfg.DeclMode
        Me.PnStartStop.Enabled = Not Cfg.DeclMode
        Me.RdOff.Enabled = Not Cfg.DeclMode
        Me.GrLAC.Enabled = Not Cfg.DeclMode
        Me.ButAuxAdd.Enabled = Not Cfg.DeclMode
        Me.ButAuxRem.Enabled = Not Cfg.DeclMode
        Me.PnEcoRoll.Enabled = Not Cfg.DeclMode

        Changed = False

    End Sub

    'Close - Check for unsaved changes
    Private Sub F02_GEN_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.ApplicationExitCall And e.CloseReason <> CloseReason.WindowsShutDown Then
            e.Cancel = ChangeCheckCancel()
        End If
    End Sub

    'Set generic values for Declaration mode
    Private Sub DeclInit()
        Dim LV0 As ListViewItem

        If Not Cfg.DeclMode Then Exit Sub

        Me.LvCycles.Items.Clear()
        Me.CbEngOnly.Checked = False
        Me.TbDesMaxFile.Text = ""
        If Not Me.RdEcoRoll.Checked Then Me.RdOverspeed.Checked = True
        Me.CbLookAhead.Checked = True

        Me.TbSSspeed.Text = cDeclaration.SSspeed
        Me.TbSStime.Text = cDeclaration.SStime
        Me.TbSSdelay.Text = cDeclaration.SSdelay
        Me.TbAlookahead.Text = cDeclaration.LACa
        Me.TbVminLA.Text = cDeclaration.LACvmin

        Me.TbOverspeed.Text = cDeclaration.Overspeed
        Me.TbUnderSpeed.Text = cDeclaration.Underspeed
        Me.TbVmin.Text = cDeclaration.ECvmin

        If LvAux.Items.Count <> 5 OrElse (Me.LvAux.Items(0).Text <> sKey.AUX.Fan OrElse Me.LvAux.Items(1).Text <> sKey.AUX.SteerPump OrElse Me.LvAux.Items(2).Text <> sKey.AUX.HVAC OrElse Me.LvAux.Items(3).Text <> sKey.AUX.ElecSys OrElse Me.LvAux.Items(4).Text <> sKey.AUX.PneumSys) Then
            Me.LvAux.Items.Clear()

            LV0 = New ListViewItem(sKey.AUX.Fan)
            LV0.SubItems.Add("Fan")
            If Declaration.AuxTechs(tAux.Fan).Count > 1 Then
                LV0.SubItems.Add("")
            Else
                LV0.SubItems.Add(Declaration.AuxTechs(tAux.Fan)(0))
            End If
            Me.LvAux.Items.Add(LV0)

            LV0 = New ListViewItem(sKey.AUX.SteerPump)
            LV0.SubItems.Add("Steering pump")
            If Declaration.AuxTechs(tAux.SteerPump).Count > 1 Then
                LV0.SubItems.Add("")
            Else
                LV0.SubItems.Add(Declaration.AuxTechs(tAux.SteerPump)(0))
            End If
            Me.LvAux.Items.Add(LV0)

            LV0 = New ListViewItem(sKey.AUX.HVAC)
            LV0.SubItems.Add("HVAC")
            If Declaration.AuxTechs(tAux.HVAC).Count > 1 Then
                LV0.SubItems.Add("")
            Else
                LV0.SubItems.Add(Declaration.AuxTechs(tAux.HVAC)(0))
            End If
            Me.LvAux.Items.Add(LV0)

            LV0 = New ListViewItem(sKey.AUX.ElecSys)
            LV0.SubItems.Add("Electric System")
            If Declaration.AuxTechs(tAux.ElectricSys).Count > 1 Then
                LV0.SubItems.Add("")
            Else
                LV0.SubItems.Add(Declaration.AuxTechs(tAux.ElectricSys)(0))
            End If
            Me.LvAux.Items.Add(LV0)

            LV0 = New ListViewItem(sKey.AUX.PneumSys)
            LV0.SubItems.Add("Pneumatic System")
            If Declaration.AuxTechs(tAux.PneumSys).Count > 1 Then
                LV0.SubItems.Add("")
            Else
                LV0.SubItems.Add(Declaration.AuxTechs(tAux.PneumSys)(0))
            End If
            Me.LvAux.Items.Add(LV0)

        End If


    End Sub


    'Show/Hide "Driver Assist" Tab
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


#Region "Browse Buttons"

    Private Sub ButtonVEH_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonVEH.Click
        If fbVEH.OpenDialog(fFileRepl(Me.TbVEH.Text, fPATH(VECTOfile))) Then Me.TbVEH.Text = fFileWoDir(fbVEH.Files(0), fPATH(VECTOfile))
    End Sub

    Private Sub ButtonMAP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMAP.Click
        If fbENG.OpenDialog(fFileRepl(Me.TbENG.Text, fPATH(VECTOfile))) Then Me.TbENG.Text = fFileWoDir(fbENG.Files(0), fPATH(VECTOfile))
    End Sub

    Private Sub ButtonGBX_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonGBX.Click
        If fbGBX.OpenDialog(fFileRepl(Me.TbGBX.Text, fPATH(VECTOfile))) Then Me.TbGBX.Text = fFileWoDir(fbGBX.Files(0), fPATH(VECTOfile))
    End Sub

    Private Sub BtDesMaxBr_Click_1(sender As System.Object, e As System.EventArgs) Handles BtDesMaxBr.Click
        If fbACC.OpenDialog(fFileRepl(Me.TbDesMaxFile.Text, fPATH(VECTOfile))) Then Me.TbDesMaxFile.Text = fFileWoDir(fbACC.Files(0), fPATH(VECTOfile))
    End Sub

    Private Sub BtAccOpen_Click(sender As System.Object, e As System.EventArgs) Handles BtAccOpen.Click
        OpenFiles(fFileRepl(Me.TbDesMaxFile.Text, fPATH(VECTOfile)))
    End Sub

#End Region

#Region "Open Buttons"

    'Open Vehicle Editor
    Private Sub ButOpenVEH_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenVEH.Click
        Dim f As String
        f = fFileRepl(TbVEH.Text, fPATH(VECTOfile))

        'Thus Veh-file is returned
        F_VEH.JobDir = fPATH(VECTOfile)
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

    'Open Engine Editor
    Private Sub ButOpenENG_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenENG.Click
        Dim f As String
        f = fFileRepl(TbENG.Text, fPATH(VECTOfile))

        'Thus Veh-file is returned
        F_ENG.JobDir = fPATH(VECTOfile)
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

    'Open Gearbox Editor
    Private Sub ButOpenGBX_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOpenGBX.Click
        Dim f As String
        f = fFileRepl(TbGBX.Text, fPATH(VECTOfile))

        'Thus Veh-file is returned
        F_GBX.JobDir = fPATH(VECTOfile)
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

#Region "Toolbar"

    'New
    Private Sub ToolStripBtNew_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtNew.Click
        VECTOnew()
    End Sub

    'Open
    Private Sub ToolStripBtOpen_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtOpen.Click
        If fbVECTO.OpenDialog(VECTOfile, False, "vecto") Then VECTOload2Form(fbVECTO.Files(0))
    End Sub

    'Save
    Private Sub ToolStripBtSave_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSave.Click
        Save()
    End Sub

    'Save As
    Private Sub ToolStripBtSaveAs_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSaveAs.Click
        If fbVECTO.SaveDialog(VECTOfile) Then Call VECTOsave(fbVECTO.Files(0))
    End Sub

    'Send to Job file list in main form
    Private Sub ToolStripBtSendTo_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSendTo.Click
        If ChangeCheckCancel() Then Exit Sub
        If VECTOfile = "" Then
            MsgBox("File not found!" & ChrW(10) & ChrW(10) & "Save file and try again.")
        Else
            F_MAINForm.AddToJobListView(VECTOfile)
        End If
    End Sub

    'Help
    Private Sub ToolStripButton1_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButton1.Click
        If IO.File.Exists(MyAppPath & "User Manual\GUI\GUI_Calls\VECTO.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\GUI\GUI_Calls\VECTO.html")
        Else
            MsgBox("User Manual not found!", MsgBoxStyle.Critical)
        End If
    End Sub


#End Region

    'Save ("Save" or "Save As" when new file)
    Private Function Save() As Boolean
        If VECTOfile = "" Then
            If fbVECTO.SaveDialog("") Then
                VECTOfile = fbVECTO.Files(0)
            Else
                Return False
            End If
        End If
        Return VECTOsave(VECTOfile)
    End Function

    'Open file
    Public Sub VECTOload2Form(ByVal file As String)
        Dim x As Int16
        Dim VEC0 As cVECTO
        Dim AuxEntryKV As KeyValuePair(Of String, cVECTO.cAuxEntry)
        Dim LV0 As ListViewItem
        Dim sb As cSubPath

        If ChangeCheckCancel() Then Exit Sub

        VECTOnew()

        'Read GEN
        VEC0 = New cVECTO
        VEC0.FilePath = file
        Try
            If Not VEC0.ReadFile() Then
                VEC0 = Nothing
                MsgBox("Failed to load " & fFILE(file, True) & "!")
                Exit Sub
            End If
        Catch ex As Exception
            MsgBox("Failed to load " & fFILE(file, True) & "!")
            Exit Sub
        End Try

        If Cfg.DeclMode <> VEC0.SavedInDeclMode Then
            Select Case WrongMode()
                Case 1
                    Me.Close()
                    F_MAINForm.RbDecl.Checked = Not F_MAINForm.RbDecl.Checked
                    F_MAINForm.OpenVectoFile(file)
                Case -1
                    Exit Sub
                Case Else '0
                    'Continue...
            End Select
        End If


        'Update Form

        'Files -----------------------------
        TbVEH.Text = VEC0.PathVEH(True)
        TbENG.Text = VEC0.PathENG(True)
        TbGBX.Text = VEC0.PathGBX(True)

        'Start/Stop
        Me.ChBStartStop.Checked = VEC0.StartStop
        Me.TbSSspeed.Text = VEC0.StStV
        Me.TbSStime.Text = VEC0.StStT
        Me.TbSSdelay.Text = VEC0.StStDelay

        'VACC
        Me.TbDesMaxFile.Text = VEC0.DesMaxFile(True)

        Me.LvAux.Items.Clear()
        For Each AuxEntryKV In VEC0.AuxPaths
            LV0 = New ListViewItem
            LV0.SubItems(0).Text = AuxEntryKV.Key
            LV0.SubItems.Add(AuxEntryKV.Value.Type)
            If Cfg.DeclMode Then
                LV0.SubItems.Add(AuxEntryKV.Value.TechStr)
            Else
                LV0.SubItems.Add(AuxEntryKV.Value.Path.OriginalPath)
            End If
            LvAux.Items.Add(LV0)
        Next

        EStechs = VEC0.EStechs

        For Each sb In VEC0.CycleFiles
            LV0 = New ListViewItem
            LV0.Text = sb.OriginalPath
            LvCycles.Items.Add(LV0)
        Next

        Me.CbEngOnly.Checked = VEC0.EngOnly

        If VEC0.EcoRollOn Then
            Me.RdEcoRoll.Checked = True
        ElseIf VEC0.OverSpeedOn Then
            Me.RdOverspeed.Checked = True
        Else
            Me.RdOff.Checked = True
        End If
        Me.TbOverspeed.Text = CStr(VEC0.OverSpeed)
        Me.TbUnderSpeed.Text = CStr(VEC0.UnderSpeed)
        Me.TbVmin.Text = CStr(VEC0.vMin)
        Me.CbLookAhead.Checked = VEC0.LookAheadOn
        Me.TbAlookahead.Text = CStr(VEC0.a_lookahead)
        Me.TbVminLA.Text = CStr(VEC0.vMinLA)


        '-------------------------------------------------------------

        DeclInit()


        F_ENG.AutoSendTo = False
        F_GBX.AutoSendTo = False
        F_VEH.AutoSendTo = False


        VECTOfile = file

        x = Len(file)
        While Mid(file, x, 1) <> "\" And x > 0
            x = x - 1
        End While
        Me.Text = Mid(file, x + 1, Len(file) - x)
        Changed = False
        Me.ToolStripStatusLabelGEN.Text = ""    'file & " opened."

        UpdatePic()

        '-------------------------------------------------------------

    End Sub

    'Save file
    Private Function VECTOsave(ByVal file As String) As Boolean

        Dim VEC0 As cVECTO
        Dim AuxEntry As cVECTO.cAuxEntry
        Dim LV0 As ListViewItem
        Dim sb As cSubPath

        VEC0 = New cVECTO
        VEC0.FilePath = file

        'Files ------------------------------------------------- -----------------

        VEC0.PathVEH = Me.TbVEH.Text
        VEC0.PathENG = Me.TbENG.Text

        For Each LV0 In LvCycles.Items
            sb = New cSubPath
            sb.Init(fPATH(file), LV0.Text)
            VEC0.CycleFiles.Add(sb)
        Next

        VEC0.PathGBX = Me.TbGBX.Text


        'Start/Stop
        VEC0.StartStop = Me.ChBStartStop.Checked
        VEC0.StStV = CSng(fTextboxToNumString(Me.TbSSspeed.Text))
        VEC0.StStT = CSng(fTextboxToNumString(Me.TbSStime.Text))
        VEC0.StStDelay = CInt(fTextboxToNumString(Me.TbSSdelay.Text))

        'a_DesMax
        VEC0.DesMaxFile = Me.TbDesMaxFile.Text

        For Each LV0 In LvAux.Items
            AuxEntry = New cVECTO.cAuxEntry

            If Cfg.DeclMode Then
                AuxEntry.TechStr = LV0.SubItems(2).Text
            Else
                AuxEntry.Path.Init(fPATH(file), LV0.SubItems(2).Text)
            End If

            AuxEntry.Type = LV0.SubItems(1).Text
            VEC0.AuxPaths.Add(LV0.SubItems(0).Text, AuxEntry)
        Next

        VEC0.EStechs = EStechs


        VEC0.EngOnly = Me.CbEngOnly.Checked

        VEC0.EcoRollOn = RdEcoRoll.Checked
        VEC0.OverSpeedOn = RdOverspeed.Checked
        VEC0.OverSpeed = CSng(fTextboxToNumString(Me.TbOverspeed.Text))
        VEC0.UnderSpeed = CSng(fTextboxToNumString(Me.TbUnderSpeed.Text))
        VEC0.vMin = CSng(fTextboxToNumString(Me.TbVmin.Text))
        VEC0.LookAheadOn = Me.CbLookAhead.Checked
        VEC0.a_lookahead = CSng(fTextboxToNumString(Me.TbAlookahead.Text))
        VEC0.vMinLA = CSng(fTextboxToNumString(Me.TbVminLA.Text))


        '------------------------------------------------------------

        'SAVE
        If Not VEC0.SaveFile Then
            MsgBox("Cannot safe to " & file, MsgBoxStyle.Critical)
            Return False
        End If

        VECTOfile = file

        file = fFILE(VECTOfile, True)

        Me.Text = file

        Me.ToolStripStatusLabelGEN.Text = ""

        F_MAINForm.AddToJobListView(VECTOfile)

        Changed = False

        Return True

    End Function

    'New file
    Public Sub VECTOnew()

        If ChangeCheckCancel() Then Exit Sub

		n_idle = -1
		FLDfile = ""

        'Files
        Me.TbVEH.Text = ""
        Me.TbENG.Text = ""
        Me.LvCycles.Items.Clear()
        Me.TbGBX.Text = ""
        Me.TbDesMaxFile.Text = ""

        'Start/Stop
        Me.TbSSspeed.Text = "5"
        Me.TbSStime.Text = "5"
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

        DeclInit()

        F_ENG.AutoSendTo = False

        VECTOfile = ""
        Me.Text = "Job Editor"
        Me.ToolStripStatusLabelGEN.Text = ""
        Changed = False
        UpdatePic()

    End Sub


#Region "Track changes"

#Region "'Change' Events"

    Private Sub TextBoxVEH_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TbVEH.TextChanged
        UpdatePic()
        Change()
    End Sub
    Private Sub TextBoxMAP_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TbENG.TextChanged
        UpdatePic()
        Change()
    End Sub

    Private Sub TextBoxFLD_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TbGBX.TextChanged
        UpdatePic()
        Change()
    End Sub

    Private Sub TbDesMaxFile_TextChanged_1(sender As System.Object, e As System.EventArgs) Handles TbDesMaxFile.TextChanged
        Change()
    End Sub


    Private Sub TBSSspeed_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbSSspeed.TextChanged
        Change()
    End Sub

    Private Sub TBSStime_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbSStime.TextChanged, TbSSdelay.TextChanged
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

    Private Sub LvCycles_AfterLabelEdit(sender As Object, e As System.Windows.Forms.LabelEditEventArgs) Handles LvCycles.AfterLabelEdit
        Change()
    End Sub


#End Region

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

        AuxDlog.VehPath = fPATH(VECTOfile)
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

            If ID = sKey.AUX.ElecSys Then
                EStechs.Clear()
                For Each LV0 In AuxDlog.LVTech.CheckedItems
                    EStechs.Add(LV0.Text)
                Next
            End If

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
                If Not Cfg.DeclMode Then RemoveAuxItem()
            Case Keys.Enter
                EditAuxItem()
        End Select
    End Sub

    Private Sub EditAuxItem()
        Dim SelItem As ListViewItem
        Dim LV0 As ListViewItem

        If LvAux.SelectedItems.Count = 0 Then Exit Sub

        SelItem = LvAux.SelectedItems(0)

        AuxDlog.VehPath = fPATH(VECTOfile)
        AuxDlog.CbType.SelectedIndex = -1
        AuxDlog.CbType.Text = SelItem.SubItems(1).Text
        AuxDlog.TbID.Text = SelItem.SubItems(0).Text    'After Type-set!

        If Cfg.DeclMode Then
            AuxDlog.CbTech.Text = SelItem.SubItems(2).Text
            AuxDlog.TbPath.Text = ""

            If AuxDlog.TbID.Text = sKey.AUX.ElecSys Then
                For Each LV0 In AuxDlog.LVTech.Items
                    If EStechs.Contains(LV0.Text) Then
                        LV0.Checked = True
                    Else
                        LV0.Checked = False
                    End If
                Next
            End If

        Else
            AuxDlog.CbTech.SelectedIndex = -1
            AuxDlog.TbPath.Text = SelItem.SubItems(2).Text
        End If

        If AuxDlog.ShowDialog = Windows.Forms.DialogResult.OK Then
            SelItem.SubItems(0).Text = UCase(Trim(AuxDlog.TbID.Text))
            SelItem.SubItems(1).Text = Trim(AuxDlog.CbType.Text)

            If Cfg.DeclMode Then
                SelItem.SubItems(2).Text = Trim(AuxDlog.CbTech.Text)
            Else
                SelItem.SubItems(2).Text = Trim(AuxDlog.TbPath.Text)
            End If

            If UCase(Trim(AuxDlog.TbID.Text)) = sKey.AUX.ElecSys Then
                EStechs.Clear()
                For Each LV0 In AuxDlog.LVTech.CheckedItems
                    EStechs.Add(LV0.Text)
                Next
            End If

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

#Region "Cycle list"

    Private Sub LvCycles_DoubleClick(sender As Object, e As System.EventArgs) Handles LvCycles.DoubleClick
        If Me.LvCycles.SelectedItems.Count > 0 Then OpenFiles(fFileRepl(Me.LvCycles.SelectedItems(0).SubItems(0).Text, fPATH(VECTOfile)))
    End Sub

    Private Sub LvCycles_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles LvCycles.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete, Keys.Back
                RemoveCycle()
            Case Keys.Enter
                If Me.LvCycles.SelectedItems.Count > 0 Then Me.LvCycles.SelectedItems(0).BeginEdit()
        End Select
    End Sub


    Private Sub BtDRIadd_Click(sender As System.Object, e As System.EventArgs) Handles BtDRIadd.Click
        Dim str As String
        Dim GenDir As String

        GenDir = fPATH(VECTOfile)

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

#End Region

#Region "Enable/Disable GUI controls"

    'Engine only mode changed
    Private Sub CbEngOnly_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CbEngOnly.CheckedChanged
        CheckEngOnly()
        Change()
    End Sub

    Private Sub CheckEngOnly()
        Dim OnOff As Boolean

        OnOff = Not CbEngOnly.Checked

        SetDrivertab(OnOff)

        ButOpenVEH.Enabled = OnOff
        TbVEH.Enabled = OnOff
        ButtonVEH.Enabled = OnOff
        ButOpenGBX.Enabled = OnOff
        TbGBX.Enabled = OnOff
        ButtonGBX.Enabled = OnOff
        GrAux.Enabled = OnOff

    End Sub

    'Start/Stop changed 
    Private Sub ChBStartStop_CheckedChanged_1(sender As System.Object, e As System.EventArgs) Handles ChBStartStop.CheckedChanged
        Change()
        If Not Cfg.DeclMode Then Me.PnStartStop.Enabled = Me.ChBStartStop.Checked
    End Sub

    'LAC changed
    Private Sub CbLookAhead_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CbLookAhead.CheckedChanged
        Change()
        Me.PnLookAhead.Enabled = CbLookAhead.Checked
    End Sub

    'EcoRoll / Overspeed changed
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

    Public Sub UpdatePic()
        Dim VEH0 As New cVEH
        Dim ENG0 As cENG
        Dim GBX0 As cGBX
        Dim FLD0 As cFLD
		Dim Shiftpoly As cGBX.cShiftPolygon
		Dim MAP0 As cMAP
		Dim OkCount As Integer
		Dim i As Integer
		Dim pmax As Single

		Dim f As cFile_V3 = Nothing
		Dim lM As List(Of Single)
		Dim lup As List(Of Single)
		Dim ldown As List(Of Single)
		Dim line As String() = Nothing

		Dim s0 As cSegmentTableEntry = Nothing
		Dim HDVclass As String
		Dim m0 As tMission

		Dim MyChart As System.Windows.Forms.DataVisualization.Charting.Chart
		Dim s As System.Windows.Forms.DataVisualization.Charting.Series
		Dim a As System.Windows.Forms.DataVisualization.Charting.ChartArea
		Dim img As Image

		Dim EngOK As Boolean = False

		Me.TbHVCclass.Text = ""
		Me.TbVehCat.Text = ""
		Me.TbMass.Text = ""
		Me.TbAxleConf.Text = ""
		Me.TbEngTxt.Text = ""
		Me.TbGbxTxt.Text = ""
		Me.PicVehicle.Image = Nothing
		Me.PicBox.Image = Nothing


		VEH0.FilePath = fFileRepl(Me.TbVEH.Text, fPATH(VECTOfile))
		If VEH0.ReadFile(False) Then

			If Declaration.SegmentTable.SetRef(s0, VEH0.VehCat, VEH0.AxleConf, VEH0.MassMax) Then
				HDVclass = s0.HDVclass

				If Cfg.DeclMode Then
					Me.LvCycles.Items.Clear()
					For Each m0 In s0.Missions
						Me.LvCycles.Items.Add(Declaration.Missions(m0).NameStr)
					Next
				End If

			Else
				HDVclass = "-"
			End If

			Me.PicVehicle.Image = Image.FromFile(Declaration.ConvPicPath(HDVclass, False))

			Me.TbHVCclass.Text = "HDV Class " & HDVclass
			Me.TbVehCat.Text = ConvVehCat(VEH0.VehCat, True)
			Me.TbMass.Text = VEH0.MassMax & " t"
			Me.TbAxleConf.Text = ConvAxleConf(VEH0.AxleConf)

		End If


		OkCount = 0

		ENG0 = New cENG
		ENG0.FilePath = fFileRepl(Me.TbENG.Text, fPATH(VECTOfile))

		'Create plot
		MyChart = New System.Windows.Forms.DataVisualization.Charting.Chart
		MyChart.Width = Me.PicBox.Width
		MyChart.Height = Me.PicBox.Height

		a = New System.Windows.Forms.DataVisualization.Charting.ChartArea

		FLD0 = New cFLD

		If ENG0.ReadFile(False) Then

			n_idle = ENG0.Nidle
			FLDfile = ENG0.PathFLD

			EngOK = True
			FLD0.FilePath = ENG0.PathFLD

			If FLD0.ReadFile(False, False) Then

				s = New System.Windows.Forms.DataVisualization.Charting.Series
				s.Points.DataBindXY(FLD0.LnU, FLD0.LTq)
				s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
				s.BorderWidth = 2
				s.Color = Color.DarkBlue
				s.Name = "Full load"
				MyChart.Series.Add(s)

				s = New System.Windows.Forms.DataVisualization.Charting.Series
				s.Points.DataBindXY(FLD0.LnU, FLD0.LTqDrag)
				s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
				s.BorderWidth = 2
				s.Color = Color.Blue
				s.Name = "Motoring"
				MyChart.Series.Add(s)

				OkCount += 1

				pmax = FLD0.Pfull(FLD0.fnUrated)

			End If

			Me.TbEngTxt.Text = (ENG0.Displ / 1000).ToString("0.0") & " l " & pmax.ToString("#") & " kW  " & ENG0.ModelName


			MAP0 = New cMAP
			MAP0.FilePath = ENG0.PathMAP

			If MAP0.ReadFile(False) Then

				s = New System.Windows.Forms.DataVisualization.Charting.Series
				s.Points.DataBindXY(MAP0.nU, MAP0.Tq)
				s.ChartType = DataVisualization.Charting.SeriesChartType.Point
				s.MarkerSize = 3
				s.Color = Color.Red
				s.Name = "Map"
				MyChart.Series.Add(s)

				OkCount += 1

			End If

		End If

        GBX0 = New cGBX
        GBX0.FilePath = fFileRepl(Me.TbGBX.Text, fPATH(VECTOfile))

        If GBX0.ReadFile(False) Then

            Me.TbGbxTxt.Text = GBX0.GearCount & "-Speed " & GearboxConv(GBX0.gs_Type) & "  " & GBX0.ModelName

			If Cfg.DeclMode Then

				If EngOK Then

					For i = 1 To GBX0.GearCount

						FLD0.FilePath = GBX0.FldFile(i)
						If FLD0.FilePath.Trim = "" Then FLD0.FilePath = ENG0.PathFLD

						If FLD0.ReadFile(True, False) Then

							If FLD0.Init(ENG0.Nidle) Then
								Shiftpoly = New cGBX.cShiftPolygon("", 0)
								Shiftpoly.SetGenericShiftPoly(FLD0, ENG0.Nidle)

								s = New System.Windows.Forms.DataVisualization.Charting.Series
								s.Points.DataBindXY(Shiftpoly.gs_nUup, Shiftpoly.gs_TqUp)
								s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
								s.BorderWidth = 2
								s.Color = Color.DarkRed
								s.Name = "Upshift curve (" & i & ")"
								MyChart.Series.Add(s)

								s = New System.Windows.Forms.DataVisualization.Charting.Series
								s.Points.DataBindXY(Shiftpoly.gs_nUdown, Shiftpoly.gs_TqDown)
								s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
								s.BorderWidth = 2
								s.Color = Color.DarkRed
								s.Name = "Downshift curve (" & i & ")"
								MyChart.Series.Add(s)
							End If


							OkCount += 1

							pmax = FLD0.Pfull(FLD0.fnUrated)

						End If

					Next

				End If

			Else

				f = New cFile_V3
				For i = 1 To GBX0.GearCount

					lM = New List(Of Single)
					lup = New List(Of Single)
					ldown = New List(Of Single)

					If f.OpenRead(GBX0.gsFile(i)) Then

						f.ReadLine()

						Try

							Do While Not f.EndOfFile
								line = f.ReadLine
								lM.Add(CSng(line(0)))
								lup.Add(CSng(line(1)))
								ldown.Add(CSng(line(2)))
							Loop

							s = New System.Windows.Forms.DataVisualization.Charting.Series
							s.Points.DataBindXY(lup, lM)
							s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
							s.BorderWidth = 2
							s.Color = Color.DarkRed
							s.Name = "Upshift curve"
							MyChart.Series.Add(s)

							s = New System.Windows.Forms.DataVisualization.Charting.Series
							s.Points.DataBindXY(ldown, lM)
							s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
							s.BorderWidth = 2
							s.Color = Color.DarkRed
							s.Name = "Downshift curve"
							MyChart.Series.Add(s)

							OkCount += 1

							f.Close()

						Catch ex As Exception
							f.Close()
						End Try

					End If

				Next

			End If

		End If

		If OkCount > 0 Then

			a.Name = "main"

			a.AxisX.Title = "engine speed [1/min]"
			a.AxisX.TitleFont = New Font("Helvetica", 10)
			a.AxisX.LabelStyle.Font = New Font("Helvetica", 8)
			a.AxisX.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None
			a.AxisX.MajorGrid.LineDashStyle = DataVisualization.Charting.ChartDashStyle.Dot

			a.AxisY.Title = "engine torque [Nm]"
			a.AxisY.TitleFont = New Font("Helvetica", 10)
			a.AxisY.LabelStyle.Font = New Font("Helvetica", 8)
			a.AxisY.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None
			a.AxisY.MajorGrid.LineDashStyle = DataVisualization.Charting.ChartDashStyle.Dot

			a.AxisX.Minimum = 300
			a.BorderDashStyle = DataVisualization.Charting.ChartDashStyle.Solid
			a.BorderWidth = 1

			a.BackColor = Color.GhostWhite

			MyChart.ChartAreas.Add(a)

			MyChart.Update()

			img = New Bitmap(MyChart.Width, MyChart.Height, Imaging.PixelFormat.Format32bppArgb)
			MyChart.DrawToBitmap(img, New Rectangle(0, 0, Me.PicBox.Width, Me.PicBox.Height))

			Me.PicBox.Image = img


		End If

    End Sub


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
