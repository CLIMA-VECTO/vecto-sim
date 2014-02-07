Imports System.Collections.Generic

Public Class F_MAINForm

    Private GenList As cFileListView
    Private DriList As cFileListView
    Private BatchGenList As cFileListView

    Private LastModeIndex As Int16
    Private LastModeName As String
    Private ConMenTarget As ListView
    Private ConMenTarGEN As Boolean

    Private MODpath As String
    Private MODVehList As Int32()

    Private DRIpage As TabPage
    Private DRIpageHere As Boolean

    Private ComLineShutDown As Boolean

    Private GUIlocked As Boolean

    Private CheckedItems As List(Of ListViewItem)

    Private DEVpage As TabPage
    Private CmDEVitem As ListViewItem

    Private CheckLock As Boolean
    Private GENchecked As Integer
    Private DRIchecked As Integer
    Private GENcheckAllLock As Boolean
    Private DRIcheckAllLock As Boolean

#Region "SLEEP Steuerung"

    Private Declare Function SetThreadExecutionState Lib "kernel32" (ByVal esFlags As Long) As Long

    Private Sub AllowSleepOFF()
#If Not PLATFORM = "x86" Then
        SetThreadExecutionState(tEXECUTION_STATE.ES_CONTINUOUS Or tEXECUTION_STATE.ES_SYSTEM_REQUIRED)
#End If
    End Sub

    Private Sub AllowSleepON()
#If Not PLATFORM = "x86" Then
         SetThreadExecutionState(tEXECUTION_STATE.ES_CONTINUOUS)
#End If
    End Sub

    Private Enum tEXECUTION_STATE As Integer
        ''' Informs the system that the state being set should remain in effect until the next call that uses ES_CONTINUOUS and one of the other state flags is cleared.
        ES_CONTINUOUS = &H80000000
        ''' Forces the display to be on by resetting the display idle timer.
        ES_DISPLAY_REQUIRED = &H2
        ''' Forces the system to be in the working state by resetting the system idle timer.
        ES_SYSTEM_REQUIRED = &H1
    End Enum

#End Region

#Region "FileBrowser Init/Close"
    Private Sub FB_Initialize()
        FB_Init = False
        fbWorkDir = New cFileBrowser("WorkDir", True)
        fbFileLists = New cFileBrowser("FileLists")
        fbGEN = New cFileBrowser("gen")
        fbVEH = New cFileBrowser("vveh")
        fbMAP = New cFileBrowser("vmap")
        fbDRI = New cFileBrowser("vdri")
        fbFLD = New cFileBrowser("vfld")
        fbTRS = New cFileBrowser("trs")
        fbMAA = New cFileBrowser("maa")
        fbMAC = New cFileBrowser("mac")
        fbWUA = New cFileBrowser("wua")
        fbWUC = New cFileBrowser("wuc")
        fbCDW = New cFileBrowser("cdw")
        fbATC = New cFileBrowser("atc")
        fbBAT = New cFileBrowser("bat")
        fbEMO = New cFileBrowser("emo")
        fbEAN = New cFileBrowser("ean")
        fbGET = New cFileBrowser("get")
        fbSTE = New cFileBrowser("ste")
        fbEKF = New cFileBrowser("ekf")
        fbEXS = New cFileBrowser("exs")
        fbFZP = New cFileBrowser("fzp")
        fbFLT = New cFileBrowser("flt")
        fbTEM = New cFileBrowser("tem")
        fbSTR = New cFileBrowser("str")

        fbENG = New cFileBrowser("veng")
        fbWHTC = New cFileBrowser("vwhtc")
        fbGBX = New cFileBrowser("vgbx")
        fbACC = New cFileBrowser("vacc")
        fbAUX = New cFileBrowser("vaux")


        '-------------------------------------------------------
        fbFileLists.Extensions = New String() {"txt"}
        fbGEN.Extensions = New String() {"vecto"}
        fbVEH.Extensions = New String() {"vveh"}
        fbMAP.Extensions = New String() {"vmap"}
        fbDRI.Extensions = New String() {"vdri"}
        fbFLD.Extensions = New String() {"vfld"}
        fbTRS.Extensions = New String() {"trs"}
        fbMAA.Extensions = New String() {"maa"}
        fbMAC.Extensions = New String() {"mac"}
        fbWUA.Extensions = New String() {"wua"}
        fbWUC.Extensions = New String() {"wuc"}
        fbCDW.Extensions = New String() {"cdw"}
        fbATC.Extensions = New String() {"atc"}
        fbBAT.Extensions = New String() {"bat"}
        fbEMO.Extensions = New String() {"emo"}
        fbEAN.Extensions = New String() {"ean"}
        fbGET.Extensions = New String() {"get"}
        fbSTE.Extensions = New String() {"ste"}
        fbEKF.Extensions = New String() {"ekf"}
        fbEXS.Extensions = New String() {"exs"}
        fbFZP.Extensions = New String() {"fzp"}
        fbFLT.Extensions = New String() {"flt"}
        fbTEM.Extensions = New String() {"tem"}
        fbSTR.Extensions = New String() {"str"}

        fbENG.Extensions = New String() {"veng"}
        fbWHTC.Extensions = New String() {"vwhtc"}
        fbGBX.Extensions = New String() {"vgbx"}
        fbACC.Extensions = New String() {"vacc"}
        fbAUX.Extensions = New String() {"vaux"}

    End Sub
    Private Sub FB_Close()
        fbWorkDir.Close()
        fbFileLists.Close()
        fbGEN.Close()
        fbVEH.Close()
        fbMAP.Close()
        fbDRI.Close()
        fbFLD.Close()
        fbTRS.Close()
        fbMAA.Close()
        fbMAC.Close()
        fbWUA.Close()
        fbWUC.Close()
        fbCDW.Close()
        fbATC.Close()
        fbBAT.Close()
        fbEMO.Close()
        fbEAN.Close()
        fbGET.Close()
        fbSTE.Close()
        fbEKF.Close()
        fbEXS.Close()
        fbFZP.Close()
        fbFLT.Close()
        fbTEM.Close()
        fbSTR.Close()

        fbENG.Close()
        fbWHTC.Close()
        fbGBX.Close()
        fbACC.Close()
        fbAUX.Close()

    End Sub
#End Region

#Region "PHEM-Worker"

    'PHEM-Launcher
    Public Sub PHEM_Launcher()
        Dim ProgOverall As Boolean
        Dim GEN0 As cGEN

        'Called when PHEM already running
        If PHEMworker.IsBusy Then
            GUImsg(tMsgID.Err, "VECTO is already running!")
            Exit Sub
        End If

        'Delete GENlist-Selection
        Me.LvGEN.SelectedItems.Clear()

        'Set Mode
        Select Case LastModeIndex
            Case 0
                PHEMmode = tPHEMmode.ModeSTANDARD
            Case 1
                PHEMmode = tPHEMmode.ModeBATCH
        End Select

        'Wenn mehr als 100 Kombinationen in Batch fragen ob sekündliche Ausgabe |@@| When Batch resulting in more than 100 combinations per second, ask whether to dump-output  per second
        If (PHEMmode = tPHEMmode.ModeBATCH) And ((Me.LvGEN.CheckedItems.Count) * (Me.LvDRI.CheckedItems.Count) > 100) And Me.ChBoxModOut.Checked Then
            Select Case MsgBox("You are about to run BATCH Mode with " & (Me.LvGEN.CheckedItems.Count) * (Me.LvDRI.CheckedItems.Count) & " calculations!" & ChrW(10) & "Do you still want to write second-by-second results?", MsgBoxStyle.YesNoCancel)
                Case MsgBoxResult.No
                    Me.ChBoxModOut.Checked = False
                Case MsgBoxResult.Cancel
                    GUImsg(tMsgID.Normal, "Aborted by User")
                    Exit Sub
            End Select
        End If

        'Status
        Status("Launching VECTO...")


        'Define Job-0list

        'Define File / Cycle list
        SetJobList()

        'Zyklus-Liste definieren (falls nicht BATCH-Modus wird in SetCycleList nur die Liste gelöscht und nicht neu belegt) |@@| Define Cycle-list (if not BATCH mode in SetCycleList deleted only the list and not reassigned)
        SetCycleList()

        If JobFileList.Count = 0 Then
            GUImsg(tMsgID.Err, "No job file selected!")
            Exit Sub
        End If

        'Check whether Overall-progbar is needed
        If PHEMmode = tPHEMmode.ModeBATCH Or JobFileList.Count > 1 Then
            ProgOverall = True
        Else
            GEN0 = New cGEN
            GEN0.FilePath = JobFileList(0)
            If Not GEN0.ReadFile Then
                GUImsg(tMsgID.Err, "Failed to job file (" & fFILE(JobFileList(0), True) & ")!")
                Exit Sub
            End If
            ProgOverall = (GEN0.CycleFiles.Count > 1)
        End If

        'Launch through Job_Launcher
        Job_Launcher(ProgOverall)

    End Sub

    Private Sub LockGUI(ByVal Lock As Boolean)
        GUIlocked = Lock

        Me.PanelOptAllg.Enabled = Not Lock
        Me.GrBoxSTD.Enabled = Not Lock
        Me.GrBoxBATCH.Enabled = Not Lock
        Me.GrBoxADV.Enabled = Not Lock

        Me.BtGENup.Enabled = Not Lock
        Me.BtGENdown.Enabled = Not Lock
        Me.ButtonGENadd.Enabled = Not Lock
        Me.ButtonGENremove.Enabled = Not Lock
        Me.LvGEN.LabelEdit = Not Lock
        Me.ChBoxAllGEN.Enabled = Not Lock

        Me.BtDRIup.Enabled = Not Lock
        Me.BtDRIdown.Enabled = Not Lock
        Me.ButtonDRIadd.Enabled = Not Lock
        Me.ButtonDRIremove.Enabled = Not Lock
        Me.LvDRI.LabelEdit = Not Lock
        Me.ChBoxAllDRI.Enabled = Not Lock

        If DEV.Enabled Then
            Me.LvDEVoptions.Enabled = Not Lock
        End If

    End Sub

    'Define File-lists
    Private Sub SetJobList()
        Dim LV0 As ListViewItem
        Dim x As Integer

        JobFileList.Clear()
        CheckedItems.Clear()

        x = -1
        For Each LV0 In Me.LvGEN.CheckedItems
            x += 1
            LV0.SubItems(1).Text = ""
            CheckedItems.Add(LV0)
            SetCheckedItemColor(x, tJobStatus.Queued)
            JobFileList.Add(fFileRepl(LV0.SubItems(0).Text, Cfg.WorkDPath))
        Next

    End Sub

    Private Sub SetCycleList()
        Dim LV0 As ListViewItem

        JobCycleList.Clear()

        If PHEMmode = tPHEMmode.ModeBATCH Then
            For Each LV0 In Me.LvDRI.CheckedItems
                JobCycleList.Add(fFileRepl(LV0.SubItems(0).Text, Cfg.WorkDPath))
            Next
        End If

    End Sub

    'Job Launcher
    Private Sub Job_Launcher(ByVal ProgOverallEnabled As Boolean)

        If PHEMworker.IsBusy Then Exit Sub

        'Load Options from Options Tab
        SetOptions()

        'Save Config
        Cfg.ConfigSAVE()

        If DEV.Enabled Then DEV.SaveToFile()

        'Reset Msg-output
        ClearMSG()

        'Button switch
        Me.Button1.Text = "STOP"
        Me.Button1.Image = My.Resources.Stop_icon

        'Disable Options
        LockGUI(True)

        'ProgBars start
        If ProgOverallEnabled Then
            Me.ToolStripProgBarOverall.Value = 0
            Me.ToolStripProgBarOverall.Style = ProgressBarStyle.Marquee
            Me.ToolStripProgBarOverall.Visible = True
        End If

        ProgBarCtrl.ProgJobInt = 0
        ProgSecStart()

        'BG-Worker start
        PHEMworker.RunWorkerAsync()

    End Sub

    'Abort Job
    Private Sub JobAbort()
        Me.Button1.Enabled = False
        Me.Button1.Text = "Aborting..."
        Me.Button1.Image = My.Resources.Play_icon_gray
        PHEMworker.CancelAsync()
    End Sub


#Region "BackgroundWorker1"

    'Begin work
    Private Sub BackgroundWorker1_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork

        'Prevent SLEEP
        AllowSleepOFF()

        If SetCulture Then
            Try
                System.Threading.Thread.CurrentThread.CurrentCulture = New System.Globalization.CultureInfo("en-US")
            Catch ex As Exception
                GUImsg(tMsgID.Err, "Failed to set thread culture 'en-US'! Check system decimal- and group- separators!")
            End Try
        End If

        e.Result = VECTO()


    End Sub

    'Progress Report
    Private Sub BackgroundWorker1_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        Dim x As cWorkProg
        x = e.UserState

        Select Case x.Target
            Case tWorkMsgType.StatusListBox
                MSGtoForm(e.UserState.ID, e.UserState.Msg, x.Source, x.Link)

            Case tWorkMsgType.StatusBar
                Status(e.UserState.Msg)

            Case tWorkMsgType.ProgBars
                Me.ToolStripProgBarOverall.Value = e.ProgressPercentage
                'At x.ProgSec = -1 no update of ProgBarSec
                If x.ProgSec = 0 Then
                    ProgSecStart()
                ElseIf x.ProgSec > 0 Then
                    ProgBarCtrl.ProgJobInt = x.ProgSec
                    ProgSecUpdate(False)
                End If

            Case tWorkMsgType.JobStatus
                CheckedItems(x.FileIndex).SubItems(1).Text = x.Msg
                SetCheckedItemColor(x.FileIndex, x.Status)

            Case tWorkMsgType.CycleStatus
                Try
                    Me.LvDRI.CheckedItems(x.FileIndex).SubItems(1).Text = x.Msg
                Catch ex As Exception
                End Try

            Case tWorkMsgType.InitProgBar
                Me.ToolStripProgBarOverall.Style = ProgressBarStyle.Continuous

            Case Else ' tWorkMsgType.Abort
                JobAbort()

        End Select
    End Sub

    'Work completed
    Private Sub BackgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted

        Dim Result As tCalcResult

        'Progbar reset
        Me.ToolStripProgBarOverall.Visible = False
        Me.ToolStripProgBarOverall.Style = ProgressBarStyle.Continuous
        Me.ToolStripProgBarOverall.Value = 0
        ProgSecStop()

        'So ListView-Item Colors (Warning = Yellow, etc..) are correctly visible
        Me.LvGEN.SelectedIndices.Clear()

        Result = e.Result

        'ShutDown when Unexpected Error
        If e.Error IsNot Nothing Then
            MsgBox("An Unexpected Error occurred!" & ChrW(10) & ChrW(10) & _
                     e.Error.Message.ToString, MsgBoxStyle.Critical, "Unexpected Error")
            LOGfile.WriteLine(">>>Unexpected Error:" & e.Error.ToString())
            Me.Close()
        End If

        'Options enable / GUI reset
        LockGUI(False)
        Me.Button1.Enabled = True
        Me.Button1.Text = "START"
        Me.Button1.Image = My.Resources.Play_icon
        Status(LastModeName & " Mode")

        'Command Line Shutdown
        If ComLineShutDown Then Me.Close()

        'Auto Shutdown
        If Me.ChBoxAutoSD.Checked Then
            Me.ChBoxAutoSD.Checked = False
            If Not Result = tCalcResult.Abort Then
                If F_ShutDown.ShutDown Then
                    GUImsg(tMsgID.Warn, "Shutting down...")
                    Me.Close()
                End If
            End If
        End If

        'SLEEP reactivate
        AllowSleepON()

    End Sub

#End Region

#End Region

#Region "Form-Events (Init, Buttons,...)"

#Region "Form Init/Close"

    'Initialize
    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim x As Integer

        GUIlocked = False
        CheckLock = False
        GENcheckAllLock = False
        DRIcheckAllLock = False
        DRIchecked = 0
        GENchecked = 0

        CheckedItems = New List(Of ListViewItem)

        'Load Tabs properly (otherwise problem with ListViews)
        For x = 0 To Me.TabControl1.TabCount - 1
            Me.TabControl1.TabPages(x).Show()
        Next

        DRIpageHere = True
        DRIpage = Me.TabPageDRI

        DEVpage = Me.TabPageDEV
        Me.TabControl1.Controls.Remove(DEVpage)

        LastModeIndex = 3
        LastModeName = ""

        ComLineShutDown = False

        FB_Initialize()

        Me.Text = "VECTO " & VECTOvers


        'FileLists
        GenList = New cFileListView(MyConfPath & "joblist.txt")
        GenList.LVbox = Me.LvGEN
        DriList = New cFileListView(MyConfPath & "cyclelist.txt")
        DriList.LVbox = Me.LvDRI
        BatchGenList = New cFileListView(MyConfPath & "joblist.txt")
        BatchGenList.LVbox = Me.LvGEN

        'Load GUI Options (here, the GEN/ADV/DRI lists are loaded)
        LoadOptions()

        'Resize columns ... after Loading the @file-lists
        Me.LvGEN.Columns(1).Width = -2
        Me.LvDRI.Columns(1).Width = -2
        Me.LvMsg.Columns(2).Width = -2

        'Initialize BackgroundWorker
        PHEMworker = Me.BackgroundWorker1
        PHEMworker.WorkerReportsProgress = True
        PHEMworker.WorkerSupportsCancellation = True

        'License check
        If Not Lic.LICcheck() Then
            MsgBox("License File invalid!" & vbCrLf & vbCrLf & Lic.FailMsg)
            If Lic.CreateActFile(MyAppPath & "ActivationCode.dat") Then
                MsgBox("Activation File created.")
            Else
                MsgBox("Failed to create Activation File! Is Directory Read-Only?")
            End If
            Me.Close()
        Else
            GUImsg(tMsgID.Normal, "License File validated.")
            If Lic.TimeWarn Then GUImsg(tMsgID.Warn, "License expiring date (y/m/d): " & Lic.ExpTime)
        End If

        DEV.Enabled = Lic.LicFeature(9)

        If DEV.Enabled Then
            DEV.LoadFromFile()
            LoadDEVconfigs()
        End If

    End Sub

    'Shown Event (Form-Load finished) ... here StartUp Forms are loaded (DEV, GEN/ADV- Editor ..)
    Private Sub F01_MAINForm_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Dim fwelcome As F_Welcome
        'DEV Form
        If DEV.Enabled Then
            Me.TabControl1.TabPages.Insert(Me.TabControl1.TabPages.Count, DEVpage)
        End If

        'VECTO Init
        'VEC.Init()

        'Command Line Args
        If Command() <> "" Then
            CmdLineCtrl(My.Application.CommandLineArgs)
        Else
            If Cfg.FirstRun Then
                Cfg.FirstRun = False
                fwelcome = New F_Welcome
                fwelcome.ShowDialog()
            End If
        End If

    End Sub

    'Open file with PHEM
    Private Sub CmdLineCtrl(ByVal ComLineArgs As System.Collections.ObjectModel.ReadOnlyCollection(Of String))
        Dim bBATCH As Boolean
        Dim bRUN As Boolean
        Dim x As Object
        Dim str As String
        Dim ComFile As String = ""
        Dim vecFiles As New List(Of String)
        Dim driFiles As New List(Of String)

        bBATCH = False
        bRUN = False
        ComFile = sKey.NoFile

        'Read Command-Line Args
        For Each x In ComLineArgs
            str = Trim(Replace(x.ToString, ChrW(34), ""))
            Select Case UCase(str)
                Case "-BATCH"
                    bBATCH = True
                Case "-CLOSE"
                    ComLineShutDown = True
                Case "-RUN"
                    bRUN = True
                Case Else
                    Select Case UCase(fEXT(str))
                        Case ".VECTO"
                            vecFiles.Add(fFileRepl(str))
                        Case ".VDRI"
                            driFiles.Add(fFileRepl(str))
                        Case Else
                            ComFile = fFileRepl(str)
                    End Select
            End Select
        Next

        'Mode switch and load Driving Cycles
        If bBATCH Then
            Me.CBoxMODE.SelectedIndex = 1

            If driFiles.Count > 0 Then
                LvDRI.Items.Clear()
                AddToListViewDRI(driFiles.ToArray)
            End If

        Else
            Me.CBoxMODE.SelectedIndex = 0
        End If

        'Load Vecto files or open editor (if only one file)
        If vecFiles.Count > 0 Then
            If vecFiles.Count > 1 Or bRUN Then
                LvGEN.Items.Clear()
                AddToListViewGEN(vecFiles.ToArray)
            Else
                ComFile = vecFiles(0)
            End If
        End If

        'Run or open file editor if file is specified
        If bRUN Then
            PHEM_Launcher()
        Else
            If ComFile <> sKey.NoFile Then OpenVectoFile(ComFile)
        End If

    End Sub

    'Close
    Private Sub F01_MAINForm_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        'Save File-Lists
        SaveFileLists()

        'Login close
        Try
            LOGfile.WriteLine("Closing Session " & Now)
            LOGfile.WriteLine("------------------------------------------------------------------------------------------")
            LOGfile.Close()
        Catch ex As Exception
        End Try


        'Config save
        SetOptions()
        Cfg.ConfigSAVE()
        If DEV.Enabled Then DEV.SaveToFile()

        'File browser instances close
        FB_Close()

    End Sub

#End Region

    Private Sub OpenVectoFile(ByVal File As String)

        If Not IO.File.Exists(File) Then

            GUImsg(tMsgID.Err, "File not found! (" & File & ")")
            MsgBox("File not found! (" & File & ")", MsgBoxStyle.Critical)

        Else

            Select Case UCase(fEXT(File))
                Case ".VGBX"
                    If Not F_GBX.Visible Then
                        F_GBX.Show()
                    Else
                        F_GBX.GenDir = ""
                        If F_GBX.WindowState = FormWindowState.Minimized Then F_GBX.WindowState = FormWindowState.Normal
                        F_GBX.BringToFront()
                    End If
                    F_GBX.openGBX(File)
                Case ".VVEH"
                    If Not F_VEH.Visible Then
                        F_VEH.Show()
                    Else
                        F_VEH.GenDir = ""
                        If F_VEH.WindowState = FormWindowState.Minimized Then F_VEH.WindowState = FormWindowState.Normal
                        F_VEH.BringToFront()
                    End If
                    F_VEH.openVEH(File)
                Case ".VENG"
                    If Not F_ENG.Visible Then
                        F_ENG.Show()
                    Else
                        F_ENG.GenDir = ""
                        If F_ENG.WindowState = FormWindowState.Minimized Then F_ENG.WindowState = FormWindowState.Normal
                        F_ENG.BringToFront()
                    End If
                    F_ENG.openENG(File)
                Case ".VECTO"
                    OpenGENEditor(File)
                Case ".VSIG"
                    OpenSigFile(File)
                Case Else
                    MsgBox("Type '" & fEXT(File) & "' unknown!", MsgBoxStyle.Critical)
            End Select

        End If

    End Sub

#Region "GEN Liste"

#Region "Events"

    Private Sub ButtonGENremove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonGENremove.Click
        removeGEN()
    End Sub

    Private Sub ButtonGENadd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonGENadd.Click
        addGEN()
    End Sub

    Private Sub ButtonGENoptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonGENopt.Click
        ConMenTarget = Me.LvGEN
        ConMenTarGEN = True

        'Locked functions show/hide
        Me.LoadListToolStripMenuItem.Enabled = Not GUIlocked
        Me.LoadDefaultListToolStripMenuItem.Enabled = Not GUIlocked
        Me.ClearListToolStripMenuItem.Enabled = Not GUIlocked
        Me.RemovePathsToolStripMenuItem2.Enabled = Not GUIlocked

        Me.ConMenFilelist.Show(Control.MousePosition)
    End Sub

    Private Sub ListViewGEN_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles LvGEN.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete, Keys.Back
                If Not GUIlocked Then removeGEN()
            Case Keys.Enter
                OpenGenOrAdv()
        End Select
    End Sub

    Private Sub ListViewGEN_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles LvGEN.DoubleClick
        If Me.LvGEN.SelectedItems.Count > 0 Then
            Me.LvGEN.SelectedItems(0).Checked = Not Me.LvGEN.SelectedItems(0).Checked
            OpenGenOrAdv()
        End If
    End Sub

    Private Sub LvGEN_ItemChecked(sender As Object, e As System.Windows.Forms.ItemCheckedEventArgs) Handles LvGEN.ItemChecked

        If e.Item.Checked Then
            GENchecked += 1
        Else
            GENchecked -= 1
        End If

        If CheckLock Then Exit Sub
        UpdateGENTabText()

    End Sub

    Private Sub ChBoxAllGEN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChBoxAllGEN.CheckedChanged

        If GENcheckAllLock And Me.ChBoxAllGEN.CheckState = CheckState.Indeterminate Then Exit Sub

        CheckAllGEN(Me.ChBoxAllGEN.Checked)
    End Sub
    Private Sub CheckAllGEN(ByVal Check As Boolean)
        Dim x As ListViewItem

        CheckLock = True
        Me.LvGEN.BeginUpdate()

        For Each x In Me.LvGEN.Items
            x.Checked = Check
        Next

        Me.LvGEN.EndUpdate()
        CheckLock = False

        GENchecked = Me.LvGEN.CheckedItems.Count
        UpdateGENTabText()
    End Sub

    Private Sub ListGEN_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles LvGEN.DragEnter
        If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
            e.Effect = DragDropEffects.Copy
        End If
    End Sub
    Private Sub ListGEN_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles LvGEN.DragDrop
        Dim f As String()
        f = CType(e.Data.GetData(DataFormats.FileDrop), Array)
        AddToListViewGEN(f)
    End Sub

    Private Sub BtGENup_Click(sender As System.Object, e As System.EventArgs) Handles BtGENup.Click
        MoveItem(LvGEN, True)
    End Sub

    Private Sub BtGENdown_Click(sender As System.Object, e As System.EventArgs) Handles BtGENdown.Click
        MoveItem(LvGEN, False)
    End Sub





#End Region

    'Remove File from list
    Private Sub removeGEN()
        Dim lastindx As Integer
        Dim SelIx() As Integer
        Dim i As Integer

        If Me.LvGEN.SelectedItems.Count < 1 Then
            If Me.LvGEN.Items.Count = 1 Then
                Me.LvGEN.Items(0).Selected = True
            Else
                Exit Sub
            End If
        End If

        LvGEN.BeginUpdate()
        CheckLock = True

        ReDim SelIx(LvGEN.SelectedItems.Count - 1)
        LvGEN.SelectedIndices.CopyTo(SelIx, 0)

        lastindx = LvGEN.SelectedIndices(LvGEN.SelectedItems.Count - 1)

        For i = UBound(SelIx) To 0 Step -1
            LvGEN.Items.RemoveAt(SelIx(i))
        Next

        If lastindx < LvGEN.Items.Count Then
            LvGEN.Items(lastindx).Selected = True
        Else
            If LvGEN.Items.Count > 0 Then LvGEN.Items(LvGEN.Items.Count - 1).Selected = True
        End If

        LvGEN.EndUpdate()
        CheckLock = False

        GENchecked = LvGEN.CheckedItems.Count
        UpdateGENTabText()
    End Sub

    'Append File to List
    Private Sub addGEN()
        Dim x As String()
        Dim Chck As Boolean = False

        x = New String() {""}

        'STANDARD/BATCH
        If fbGEN.OpenDialog("", True, "vecto") Then
            Chck = True
            x = fbGEN.Files
        End If
   
        If Chck Then AddToListViewGEN(x)

    End Sub

    'Open file
    Private Sub OpenGenOrAdv()
        Dim f As String

        If Me.LvGEN.SelectedItems.Count < 1 Then
            If Me.LvGEN.Items.Count = 1 Then
                Me.LvGEN.Items(0).Selected = True
            Else
                Exit Sub
            End If
        End If

        f = Me.LvGEN.SelectedItems(0).SubItems(0).Text
        f = fFileRepl(f, Cfg.WorkDPath)
        If Not IO.File.Exists(f) Then
            MsgBox(f & " not found!")
        Else
            OpenGENEditor(f)
        End If
    End Sub

    'GEN/ADV list: Add File
    Private Sub AddToListViewGEN(ByVal Path As String(), Optional ByVal Txt As String = " ")
        Dim pDim As Int16
        Dim p As Int16
        Dim f As Int16
        Dim fList As String()
        Dim fListDim As Int16 = -1
        Dim ListViewItem0 As ListViewItem

        'If PHEM runs: Cancel operation (because Mode-change during calculation is not very clever)
        If PHEMworker.IsBusy Then Exit Sub

        pDim = UBound(Path)
        ReDim fList(0)     'um Nullverweisausnahme-Warnung zu verhindern

        '******************************************* Begin Update '*******************************************
        Me.LvGEN.BeginUpdate()
        CheckLock = True

        Me.LvGEN.SelectedIndices.Clear()

        If pDim = 0 Then
            fListDim = Me.LvGEN.Items.Count - 1
            ReDim fList(fListDim)
            For f = 0 To fListDim
                fList(f) = fFileRepl(Me.LvGEN.Items(f).SubItems(0).Text)
            Next
        End If

        For p = 0 To pDim

            If pDim = 0 Then

                For f = 0 To fListDim

                    'If file already exists in the list: Do not append (only when a single file)
                    If UCase(Path(p)) = UCase(fList(f)) Then

                        'Status reset
                        Me.LvGEN.Items(f).SubItems(1).Text = Txt
                        Me.LvGEN.Items(f).BackColor = Color.FromKnownColor(KnownColor.Window)
                        Me.LvGEN.Items(f).ForeColor = Color.FromKnownColor(KnownColor.WindowText)

                        'Element auswählen und anhaken |@@| Element selection and hook
                        Me.LvGEN.Items(f).Selected = True
                        Me.LvGEN.Items(f).Checked = True
                        Me.LvGEN.Items(f).EnsureVisible()

                        GoTo lbFound
                    End If
                Next

            End If

            'Otherwise: Add File (without WorkDir)
            ListViewItem0 = New ListViewItem(Path(p))   'fFileWD(Path(p)))
            ListViewItem0.SubItems.Add(" ")
            ListViewItem0.Checked = True
            ListViewItem0.Selected = True
            Me.LvGEN.Items.Add(ListViewItem0)
            ListViewItem0.EnsureVisible()
lbFound:
        Next

        Me.LvGEN.EndUpdate()
        CheckLock = False
        '******************************************* End Update '*******************************************

        'Number update
        GENchecked = Me.LvGEN.CheckedItems.Count
        UpdateGENTabText()


    End Sub

    Public Sub AddToListViewGEN(ByVal Path As String, Optional ByVal Txt As String = " ")
        Dim p(0) As String
        p(0) = Path
        AddToListViewGEN(p, Txt)
    End Sub

    Private Sub UpdateGENTabText()
        Dim c As Integer
        c = Me.LvGEN.Items.Count

        Me.TabPageGEN.Text = "Job Files ( " & GENchecked & " / " & c & " )"
        'Me.TabPageGEN.Text = "Job Files (" & c & ")"

        GENcheckAllLock = True

        If GENchecked = 0 Then
            Me.ChBoxAllGEN.CheckState = CheckState.Unchecked
        ElseIf GENchecked = c Then
            Me.ChBoxAllGEN.CheckState = CheckState.Checked
        Else
            Me.ChBoxAllGEN.CheckState = CheckState.Indeterminate
        End If

        GENcheckAllLock = False

    End Sub

#End Region

#Region "DRI Liste"


#Region "Events"

    Private Sub ButtonDRIadd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDRIadd.Click
        addDRI()
    End Sub

    Private Sub ButtonDRIremove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDRIremove.Click
        removeDRI()
    End Sub

    Private Sub ButtonDRIoptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDRIedit.Click
        ConMenTarget = Me.LvDRI
        ConMenTarGEN = False
        Me.ConMenFilelist.Show(Control.MousePosition)
    End Sub

    Private Sub LvDRI_ItemCheck(sender As Object, e As System.Windows.Forms.ItemCheckEventArgs) Handles LvDRI.ItemCheck

        'If e.CurrentValue = e.NewValue Then Exit Sub

        'If e.NewValue = CheckState.Checked Then
        '    DRIchecked += 1
        'Else
        '    DRIchecked -= 1
        'End If

        'If CheckLock Then Exit Sub
        'UpdateDRITabText()


    End Sub

    Private Sub LvDRI_ItemChecked(sender As Object, e As System.Windows.Forms.ItemCheckedEventArgs) Handles LvDRI.ItemChecked

        If e.Item.Checked Then
            DRIchecked += 1
        Else
            DRIchecked -= 1
        End If

        If CheckLock Then Exit Sub
        UpdateDRITabText()

    End Sub

    Private Sub ChBoxAllDRI_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChBoxAllDRI.CheckedChanged
        Dim Check As Boolean
        Dim x As ListViewItem

        If DRIcheckAllLock Or Me.ChBoxAllDRI.CheckState = CheckState.Indeterminate Then Exit Sub

        Check = Me.ChBoxAllDRI.Checked

        CheckLock = True
        Me.LvDRI.BeginUpdate()

        For Each x In Me.LvDRI.Items
            x.Checked = Check
        Next

        Me.LvDRI.EndUpdate()
        CheckLock = False

        DRIchecked = Me.LvDRI.CheckedItems.Count
        UpdateDRITabText()

    End Sub

    Private Sub ListViewDRI_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles LvDRI.DoubleClick
        If Me.LvDRI.SelectedItems.Count > 0 Then
            Me.LvDRI.SelectedItems(0).Checked = Not Me.LvDRI.SelectedItems(0).Checked
            openDRI()
        Else
            addDRI()
        End If
    End Sub

    Private Sub ListViewDRI_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles LvDRI.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete, Keys.Back
                If Not GUIlocked Then removeDRI()
            Case Keys.Enter
                openDRI()
        End Select
    End Sub

    'Drag n' Drop
    Private Sub ListDRI_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles LvDRI.DragEnter
        If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then e.Effect = DragDropEffects.Copy
    End Sub
    Private Sub ListDRI_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles LvDRI.DragDrop
        Dim f As String()
        f = CType(e.Data.GetData(DataFormats.FileDrop), Array)
        AddToListViewDRI(f)
        If LvDRI.Items.Count > 0 Then LvDRI.Items(LvDRI.Items.Count - 1).Selected = True
    End Sub

    Private Sub BtDRIup_Click(sender As System.Object, e As System.EventArgs) Handles BtDRIup.Click
        MoveItem(LvDRI, True)
    End Sub

    Private Sub BtDRIdown_Click(sender As System.Object, e As System.EventArgs) Handles BtDRIdown.Click
        MoveItem(LvDRI, False)
    End Sub

#End Region

    Private Sub removeDRI()
        Dim lastindx As Integer
        Dim SelIx() As Integer
        Dim i As Integer

        If Me.LvDRI.SelectedItems.Count < 1 Then
            If Me.LvDRI.Items.Count = 1 Then
                Me.LvDRI.Items(0).Selected = True
            Else
                Exit Sub
            End If
        End If

        CheckLock = True
        LvDRI.BeginUpdate()

        ReDim SelIx(LvDRI.SelectedItems.Count - 1)
        LvDRI.SelectedIndices.CopyTo(SelIx, 0)

        lastindx = LvDRI.SelectedIndices(LvDRI.SelectedItems.Count - 1)

        For i = UBound(SelIx) To 0 Step -1
            LvDRI.Items.RemoveAt(SelIx(i))
        Next

        If lastindx < LvDRI.Items.Count Then
            LvDRI.Items(lastindx).Selected = True
        Else
            If LvDRI.Items.Count > 0 Then LvDRI.Items(LvDRI.Items.Count - 1).Selected = True
        End If

        CheckLock = False
        LvDRI.EndUpdate()

        DRIchecked = LvDRI.CheckedItems.Count
        UpdateDRITabText()
    End Sub

    Private Sub addDRI()
        If fbDRI.OpenDialog("", True) Then
            AddToListViewDRI(fbDRI.Files)
            If LvDRI.Items.Count > 0 Then LvDRI.Items(LvDRI.Items.Count - 1).Selected = True
        End If
    End Sub

    Private Sub openDRI()

        If Me.LvDRI.SelectedItems.Count < 1 Then
            If Me.LvDRI.Items.Count = 1 Then
                Me.LvDRI.Items(0).Selected = True
            Else
                Exit Sub
            End If
        End If

        OpenFiles(fFileRepl(Me.LvDRI.SelectedItems(0).SubItems(0).Text, Cfg.WorkDPath))
    End Sub

    'DRI list: Add File
    Private Sub AddToListViewDRI(ByVal Path As String())
        Dim pDim As Int16
        Dim p As Int16
        Dim ListViewItem0 As ListViewItem

        pDim = UBound(Path)

        Me.LvDRI.BeginUpdate()
        CheckLock = True

        'Mode switch if necessary
        If (LastModeIndex <> 1) Then Me.CBoxMODE.SelectedIndex = 1

        For p = 0 To pDim
            ListViewItem0 = New ListViewItem(Path(p))   'fFileWD(Path(p)))
            ListViewItem0.SubItems.Add(" ")
            ListViewItem0.Checked = True
            Me.LvDRI.Items.Add(ListViewItem0)
lbFound:
        Next

        Me.LvDRI.EndUpdate()
        CheckLock = False

        'Number update
        DRIchecked = Me.LvDRI.CheckedItems.Count
        UpdateDRITabText()

    End Sub

    Private Sub AddToListViewDRI(ByVal Path As String)
        Dim p(0) As String
        p(0) = Path
        AddToListViewDRI(p)
    End Sub

    Private Sub UpdateDRITabText()
        Dim c As Integer
        c = Me.LvDRI.Items.Count

        Me.TabPageDRI.Text = "Driving Cycles ( " & DRIchecked & " / " & c & " )"
        'Me.TabPageDRI.Text = "Driving Cycles (" & c & ")"

        DRIcheckAllLock = True

        If DRIchecked = 0 Then
            Me.ChBoxAllDRI.CheckState = CheckState.Unchecked
        ElseIf DRIchecked = c Then
            Me.ChBoxAllDRI.CheckState = CheckState.Checked
        Else
            Me.ChBoxAllDRI.CheckState = CheckState.Indeterminate
        End If

        DRIcheckAllLock = False

    End Sub

#End Region

#Region "Toolstrip"

    'New GEN/ADV
    Private Sub ToolStripBtNew_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtNew.Click
        OpenGENEditor("<New>")
    End Sub

    'Open GEN/ADV
    Private Sub ToolStripBtOpen_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtOpen.Click
     
        If fbGEN.OpenDialog("", False, "vecto,vveh,vgbx,veng,vsig") Then
            OpenVectoFile(fbGEN.Files(0))
        End If

    End Sub

    Private Sub GENEditorToolStripMenuItem1_Click(sender As System.Object, e As System.EventArgs) Handles GENEditorToolStripMenuItem1.Click
        OpenGENEditor("<New>")
    End Sub

    Private Sub VEHEditorToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles VEHEditorToolStripMenuItem.Click
        If Not F_VEH.Visible Then
            F_VEH.Show()
        Else
            If F_VEH.WindowState = FormWindowState.Minimized Then F_VEH.WindowState = FormWindowState.Normal
            F_VEH.BringToFront()
        End If
    End Sub

    Private Sub EngineEditorToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles EngineEditorToolStripMenuItem.Click
        If Not F_ENG.Visible Then
            F_ENG.Show()
        Else
            If F_ENG.WindowState = FormWindowState.Minimized Then F_ENG.WindowState = FormWindowState.Normal
            F_ENG.BringToFront()
        End If
    End Sub

    Private Sub GearboxEditorToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles GearboxEditorToolStripMenuItem.Click
        If Not F_GBX.Visible Then
            F_GBX.Show()
        Else
            If F_GBX.WindowState = FormWindowState.Minimized Then F_GBX.WindowState = FormWindowState.Normal
            F_GBX.BringToFront()
        End If
    End Sub


    Private Sub SignOrVerifyFilesToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles SignOrVerifyFilesToolStripMenuItem.Click
        If Not F_FileSign.Visible Then
            F_FileSign.Show()
        Else
            If F_FileSign.WindowState = FormWindowState.Minimized Then F_FileSign.WindowState = FormWindowState.Normal
            F_FileSign.BringToFront()
        End If
    End Sub

    Private Sub GRAPHiToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles GRAPHiToolStripMenuItem.Click
        Dim PSI As New ProcessStartInfo
        Dim fileStr As String = ""
        PSI.FileName = ChrW(34) & MyAppPath & "GRAPHi\GRAPHi.exe" & ChrW(34)
        Try
            Process.Start(PSI)
        Catch ex As Exception
            MsgBox("Failed to open GRAPHi!", MsgBoxStyle.Critical)
        End Try
    End Sub


    Private Sub OpenLogToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles OpenLogToolStripMenuItem.Click
        System.Diagnostics.Process.Start(MyAppPath & "log.txt")
    End Sub

    Private Sub ChangeWorkingDirectoryToolStripMenuItem1_Click(sender As System.Object, e As System.EventArgs) Handles ChangeWorkingDirectoryToolStripMenuItem1.Click
        Dim WD As String
        WD = Cfg.WorkDPath
        If fbWorkDir.OpenDialog(Cfg.WorkDPath) Then
            Cfg.SetWorkDir(fbWorkDir.Files(0))
            If WD <> Cfg.WorkDPath Then GUImsg(tMsgID.Normal, "Working Directory changed to " & Cfg.WorkDPath)
        End If
    End Sub

    Private Sub SettingsToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles SettingsToolStripMenuItem.Click
        F_Options.Show()
    End Sub

    Private Sub UserManualToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles UserManualToolStripMenuItem.Click
        If IO.File.Exists(MyAppPath & "User Manual\usermanual.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\usermanual.html")
        Else
            MsgBox("User Manual not found!", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub QuickStartGuideToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles QuickStartGuideToolStripMenuItem.Click
        If IO.File.Exists(MyAppPath & "User Manual\qsg\quickstartApp.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\qsg\quickstartApp.html")
        Else
            MsgBox("Quick Start Guide not found!", MsgBoxStyle.Critical)
        End If
    End Sub


    Private Sub UpdateNotesToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles UpdateNotesToolStripMenuItem.Click
        If IO.File.Exists(MyAppPath & "User Manual\Update Notes.pdf") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\Update Notes.pdf")
        Else
            MsgBox("Update Notes not found!", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub SupportToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles SupportToolStripMenuItem.Click
        If IO.File.Exists(MyAppPath & "User Manual\contact.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\contact.html")
        Else
            MsgBox("User Manual not found!", MsgBoxStyle.Critical)
        End If
    End Sub


    Private Sub CreateActivationFileToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles CreateActivationFileToolStripMenuItem.Click
        If MsgBox("Create Activation File ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            If Lic.CreateActFile(MyAppPath & "ActivationCode.dat") Then
                GUImsg(tMsgID.Normal, "Activation File created.")
            Else
                GUImsg(tMsgID.Err, "Failed to create Activation File!")
                MsgBox("ERROR! Failed to create Activation File!", MsgBoxStyle.Critical)
            End If
        End If
    End Sub

    Private Sub AboutPHEMToolStripMenuItem1_Click(sender As System.Object, e As System.EventArgs) Handles AboutPHEMToolStripMenuItem1.Click
        F_AboutBox.ShowDialog()
    End Sub




#End Region


    Private Sub MoveItem(ByRef ListV As ListView, ByVal MoveUp As Boolean)
        Dim x As Int32
        Dim y As Int32
        Dim y1 As Int32
        Dim items() As String
        Dim check() As Boolean
        Dim index() As Integer
        Dim ListViewItem0 As ListViewItem

        If GUIlocked Then Exit Sub

        'Cache Selected Items
        y1 = ListV.SelectedItems.Count - 1
        ReDim items(y1)
        ReDim check(y1)
        ReDim index(y1)
        y = 0
        For Each x In ListV.SelectedIndices
            items(y) = ListV.Items(x).SubItems(0).Text
            check(y) = ListV.Items(x).Checked
            If MoveUp Then
                If x = 0 Then Exit Sub
                index(y) = x - 1
            Else
                If x = ListV.Items.Count - 1 Then Exit Sub
                index(y) = x + 1
            End If
            y += 1
        Next

        ListV.BeginUpdate()

        'Delete Selected Items
        For Each ListViewItem0 In ListV.SelectedItems
            ListViewItem0.Remove()
        Next

        'Items select and Insert
        'For y = y1 To 0 Step -1
        For y = 0 To y1
            If Not check(y) Then GENchecked += 1
            ListViewItem0 = ListV.Items.Insert(index(y), items(y))
            ListViewItem0.SubItems.Add(" ")
            ListViewItem0.Checked = check(y)
            ListV.SelectedIndices.Add(index(y))
        Next

        ListV.EndUpdate()

    End Sub


#Region "FileList - Context Menu"

    'Save List
    Private Sub SaveListToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveListToolStripMenuItem.Click
        If fbFileLists.SaveDialog("") Then
            If ConMenTarGEN Then
                GenList.SaveList(fbFileLists.Files(0))
            Else
                DriList.SaveList(fbFileLists.Files(0))
            End If
        End If
    End Sub

    'Load List
    Private Sub LoadListToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LoadListToolStripMenuItem.Click

        If GUIlocked Then Exit Sub

        If fbFileLists.OpenDialog("") Then

            If ConMenTarGEN Then    'GEN
                GenList.LoadList(fbFileLists.Files(0))
                GENchecked = Me.LvGEN.CheckedItems.Count
                UpdateGENTabText()
            Else                    'DRI
                'Mode toggle (from(auf) BATCH)
                If (LastModeIndex <> 1) Then Me.CBoxMODE.SelectedIndex = 1
                DriList.LoadList(fbFileLists.Files(0))
                DRIchecked = Me.LvDRI.CheckedItems.Count
                UpdateDRITabText()
            End If

        End If

    End Sub

    'Load Default
    Private Sub LoadDefaultListToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LoadDefaultListToolStripMenuItem.Click

        If GUIlocked Then Exit Sub

        If ConMenTarGEN Then
            Select Case LastModeIndex
                Case 0
                    GenList.LoadList()
                Case 1
                    BatchGenList.LoadList()
            End Select
            GENchecked = Me.LvGEN.CheckedItems.Count
            UpdateGENTabText()
        Else
            DriList.LoadList()
            DRIchecked = Me.LvDRI.CheckedItems.Count
            UpdateDRITabText()
        End If
    End Sub

    'Clear List
    Private Sub ClearListToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ClearListToolStripMenuItem.Click
        'Dim ListViewItem0 As ListViewItem
        'For Each ListViewItem0 In ConMenTarget.SelectedItems
        '    ListViewItem0.Remove()
        'Next
        If GUIlocked Then Exit Sub

        ConMenTarget.Items.Clear()
        If ConMenTarGEN Then
            GENchecked = Me.LvGEN.CheckedItems.Count
            UpdateGENTabText()
        Else
            DRIchecked = Me.LvDRI.CheckedItems.Count
            UpdateDRITabText()
        End If
    End Sub

    'Remove Paths
    Private Sub RemovePathsToolStripMenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RemovePathsToolStripMenuItem2.Click

        If GUIlocked Then Exit Sub

        If ConMenTarGEN Then
            GenList.RemovePaths()
        Else
            DriList.RemovePaths()
        End If
    End Sub

#End Region

    'PHEM Start
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        'PHEM Start/Stop
        If PHEMworker.IsBusy Then
            'If PHEM already running: STOP
            ComLineShutDown = False
            JobAbort()
        Else
            '...Otherwise: START

            'Save Lists if Crash
            SaveFileLists()

            'Start
            PHEM_Launcher()

        End If

    End Sub

    'Mode Change
    Private Sub CBoxMODE_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CBoxMODE.SelectedIndexChanged
        Dim xB As Boolean, xA As Boolean
        xB = (Me.CBoxMODE.SelectedIndex = 1)
        xA = (Me.CBoxMODE.SelectedIndex = 2)

        'Save Old list
        Select Case LastModeIndex
            Case 0  'Standard
                GenList.SaveList()
            Case 1  'Batch
                BatchGenList.SaveList()
                DriList.SaveList()
        End Select

        LastModeIndex = Me.CBoxMODE.SelectedIndex

        Select Case LastModeIndex
            Case 0
                PHEMmode = tPHEMmode.ModeSTANDARD
            Case 1
                PHEMmode = tPHEMmode.ModeBATCH
        End Select

        'Load New List
        Select Case LastModeIndex
            Case 0  'Standard
                LastModeName = "STANDARD"
                GenList.LoadList()

                'Me.GrBoxSTD.BringToFront()

                Me.GrBoxSTD.Visible = False  'weil GroupBox leer!!! sonst 'True
                Me.GrBoxBATCH.Visible = False
                Me.GrBoxADV.Visible = False

                If DRIpageHere Then
                    Me.TabControl1.Controls.Remove(DRIpage)
                    DRIpageHere = False
                End If
            Case 1  'Batch
                LastModeName = "BATCH"
                BatchGenList.LoadList()
                DriList.LoadList()
                DRIchecked = Me.LvDRI.CheckedItems.Count
                UpdateDRITabText()

                'Me.GrBoxBATCH.BringToFront()

                Me.GrBoxSTD.Visible = False
                Me.GrBoxBATCH.Visible = True
                Me.GrBoxADV.Visible = False

                If Not DRIpageHere Then
                    'Me.TabControl1.Controls.Add(DRIpage)
                    Me.TabControl1.TabPages.Insert(1, DRIpage)
                    DRIpageHere = True
                End If

        End Select

        GENchecked = Me.LvGEN.CheckedItems.Count
        UpdateGENTabText()
        Status(LastModeName & " Mode")

    End Sub



#End Region

    Private Class cFileListView

        Private FilePath As String
        Private LoadedDefault As Boolean
        Public LVbox As ListView

        Public Sub New(ByVal Path As String)
            FilePath = Path
            LoadedDefault = False
        End Sub

        Public Sub SaveList(Optional ByVal Path As String = "")
            Dim x As Int32
            Dim file As cFile_V3
            'If LVbox.Items.Count = 0 Then Exit Sub
            file = New cFile_V3
            If Path = "" Then
                If Not LoadedDefault Then Exit Sub
                Path = FilePath
            End If
            file.OpenWrite(Path, "?")
            For x = 1 To LVbox.Items.Count
                file.WriteLine(LVbox.Items(x - 1).SubItems(0).Text, Math.Abs(CInt(LVbox.Items(x - 1).Checked)))
            Next
            file.Close()
            file = Nothing
        End Sub

        Public Sub LoadList(Optional ByVal Path As String = "")
            Dim line As String()
            Dim NoCheck As Boolean
            Dim file As cFile_V3
            Dim ListViewItem0 As ListViewItem

            If Path = "" Then
                Path = FilePath
                LoadedDefault = True
            End If

            file = New cFile_V3

            If Not file.OpenRead(Path, "?") Then
                If Not LoadedDefault Then GUImsg(tMsgID.Err, "Cannot open file (" & Path & ")!")
                Exit Sub
            End If

            F_MAINForm.CheckLock = True
            LVbox.BeginUpdate()

            LVbox.Items.Clear()

            NoCheck = False
            Do While Not file.EndOfFile
                line = file.ReadLine

                ListViewItem0 = New ListViewItem(line(0))
                ListViewItem0.SubItems.Add(" ")

                If NoCheck Then
                    ListViewItem0.Checked = True
                Else
                    If UBound(line) < 1 Then
                        NoCheck = True
                        ListViewItem0.Checked = True
                    Else
                        If IsNumeric(line(1)) Then
                            ListViewItem0.Checked = CBool(line(1))
                        Else
                            ListViewItem0.Checked = True
                        End If
                    End If
                End If
                LVbox.Items.Add(ListViewItem0)
            Loop

            file.Close()

            LVbox.EndUpdate()
            F_MAINForm.CheckLock = False

            If LVbox.Items.Count > 0 Then LVbox.Items(LVbox.Items.Count - 1).EnsureVisible()

        End Sub

        Public Sub RemovePaths()
            Dim x As Int32
            For x = 1 To LVbox.Items.Count
                Me.RemovePath(x - 1)
            Next
        End Sub

        Private Sub RemovePath(ByVal i As Int32)
            Dim Path As String
            Path = fFILE(LVbox.Items(i).SubItems(0).Text, True)
            LVbox.Items(i).SubItems(0).Text = Path
        End Sub

    End Class

    Private Sub SetCheckedItemColor(ByVal LvID As Integer, ByVal Status As tJobStatus)
        Dim lv0 As ListViewItem

        lv0 = CheckedItems(LvID)

        Select Case Status
            Case tJobStatus.Err
                lv0.BackColor = Color.Red
                lv0.ForeColor = Color.White
            Case tJobStatus.OK
                lv0.BackColor = Color.White
                lv0.ForeColor = Color.DarkGreen
            Case tJobStatus.Warn
                lv0.BackColor = Color.Khaki
                lv0.ForeColor = Color.DarkBlue      'FromArgb(218, 125, 0) 'DarkOrange 'OrangeRed
            Case tJobStatus.Queued
                lv0.BackColor = Color.LightGray
                lv0.ForeColor = Color.DarkBlue
            Case tJobStatus.Running
                lv0.BackColor = Color.DarkBlue
                lv0.ForeColor = Color.White
            Case tJobStatus.Undef
                lv0.BackColor = Color.FromKnownColor(KnownColor.Window)
                lv0.ForeColor = Color.FromKnownColor(KnownColor.WindowText)
        End Select

    End Sub

    'Open GEN-editor and load File
    Friend Sub OpenGENEditor(ByVal x As String)

        If Not F_GEN.Visible Then
            F_GEN.Show()
        Else
            If F_GEN.WindowState = FormWindowState.Minimized Then F_GEN.WindowState = FormWindowState.Normal
            F_GEN.BringToFront()
        End If

        If x = "<New>" Then
            F_GEN.GENnew()
        Else
            F_GEN.GENload2Form(x)
        End If

        F_GEN.Activate()

    End Sub

    Friend Sub OpenSigFile(ByVal file As String)
        If Not F_FileSign.Visible Then
            F_FileSign.Show()

        End If
        F_FileSign.WindowState = FormWindowState.Normal
        F_FileSign.TbSigFile.Text = file
        F_FileSign.VerifySigFile()
        F_FileSign.Activate()
    End Sub

    'Save File-Lists
    Private Sub SaveFileLists()
        Select Case LastModeIndex
            Case 0
                GenList.SaveList()
            Case 1
                BatchGenList.SaveList()
                DriList.SaveList()
        End Select
    End Sub


#Region "Sekundäre Progressbar (ToolStripProgBarSec) ...auch Update von ToolStripProgBarPrim möglich"

    Private Sub ProgSecStart()
        Me.ToolStripProgBarJob.Value = 0
        Me.ToolStripProgBarJob.Style = ProgressBarStyle.Marquee
        Me.ToolStripProgBarJob.Visible = True
        Me.TmProgSec.Start()
    End Sub

    Private Sub ProgSecStop()
        Me.TmProgSec.Stop()
        Me.ToolStripProgBarJob.Visible = False
        Me.ToolStripProgBarJob.Value = 0
    End Sub

    Private Sub TmProgSec_Tick(sender As Object, e As System.EventArgs) Handles TmProgSec.Tick
        ProgSecUpdate(True)
    End Sub

    Private Sub ProgSecUpdate(ByVal ProgPrimUpdate As Boolean)

        With ProgBarCtrl

            If .ProgJobInt > 0 AndAlso Me.ToolStripProgBarJob.Style = ProgressBarStyle.Marquee Then
                Me.ToolStripProgBarJob.Style = ProgressBarStyle.Continuous
            End If

            If .ProgJobInt < 0 Then
                .ProgJobInt = 0
            ElseIf .ProgJobInt > 100 Then
                .ProgJobInt = 100
            End If

            Me.ToolStripProgBarJob.Value = .ProgJobInt

            If ProgPrimUpdate And .ProgOverallStartInt > -1 Then
                Me.ToolStripProgBarOverall.Value = CInt(.ProgOverallStartInt + (.PgroOverallEndInt - .ProgOverallStartInt) * .ProgJobInt / 100)
            End If

        End With

    End Sub


#End Region

#Region "Options Tab"

    Public Sub LoadOptions()
        Me.ChBoxFzpSort.Checked = Cfg.FZPsort
        Me.ChBoxCyclDistCor.Checked = Cfg.WegKorJa
        Me.ChBoxUseGears.Checked = Cfg.GnVorgab
        Me.ChBoxModOut.Checked = Cfg.ModOut
        Me.ChBoxFzpExport.Checked = Cfg.FZPsortExp
        CbBOmode.SelectedIndex = -1
        Select Case UCase(Cfg.BATCHoutpath)
            Case sKey.WorkDir
                CbBOmode.SelectedIndex = 0
            Case sKey.GenPath
                CbBOmode.SelectedIndex = 1
            Case Else
                CbBOmode.SelectedIndex = 2
                Me.TbBOpath.Text = Cfg.BATCHoutpath
        End Select
        Me.ChBoxBatchSubD.Checked = Cfg.BATCHoutSubD

        'Set Mode
        LastModeIndex = Cfg.LastMode
        Me.CBoxMODE.SelectedIndex = Cfg.LastMode

        Select Case LastModeIndex
            Case 0
                PHEMmode = tPHEMmode.ModeSTANDARD
            Case 1
                PHEMmode = tPHEMmode.ModeBATCH
        End Select

    End Sub



    Private Sub SetOptions()

        'General(Allgemein)
        Cfg.WegKorJa = Me.ChBoxCyclDistCor.Checked
        Cfg.GnVorgab = Me.ChBoxUseGears.Checked

        'ADVANCE
        Cfg.FZPsortExp = Me.ChBoxFzpExport.Checked
        Cfg.FZPsort = Me.ChBoxFzpSort.Checked

        'BATCH
        Cfg.ModOut = Me.ChBoxModOut.Checked
        Select Case CbBOmode.SelectedIndex
            Case 0
                Cfg.BATCHoutpath = sKey.WorkDir
            Case 1
                Cfg.BATCHoutpath = sKey.GenPath
            Case Else
                Cfg.BATCHoutpath = Trim(Me.TbBOpath.Text)
                If Microsoft.VisualBasic.Right(Cfg.BATCHoutpath, 1) <> "\" Then Cfg.BATCHoutpath &= "\"
        End Select
        Cfg.BATCHoutSubD = Me.ChBoxBatchSubD.Checked

        DEV.SetOptions()


    End Sub



#Region "Events"

    Private Sub ChBoxAdvSort_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChBoxFzpSort.CheckedChanged
        Me.ChBoxFzpExport.Enabled = Me.ChBoxFzpSort.Checked
    End Sub

    Private Sub ChBoxAutoSD_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChBoxAutoSD.CheckedChanged
        Me.LbAutoShDown.Visible = Me.ChBoxAutoSD.Checked
    End Sub

    Private Sub CbBOmode_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CbBOmode.SelectedIndexChanged
        Me.TbBOpath.Visible = (Me.CbBOmode.SelectedIndex = 2)
        Me.ButBObrowse.Visible = (Me.CbBOmode.SelectedIndex = 2)
    End Sub

    Private Sub ButBObrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButBObrowse.Click
        If fbWorkDir.OpenDialog(Me.TbBOpath.Text) Then
            Me.TbBOpath.Text = fbWorkDir.Files(0)
        End If
    End Sub

    Private Sub ChBoxBatchOut_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChBoxModOut.CheckedChanged
        Me.ChBoxBatchSubD.Enabled = Me.ChBoxModOut.Checked
    End Sub

#End Region


#End Region

#Region "DEV Tab"

    Private Sub LoadDEVconfigs()
        Dim LV0 As ListViewItem
        Dim i As Integer
        Dim Config0 As KeyValuePair(Of String, cDEVoption)

        Me.LvDEVoptions.Items.Clear()

        i = -1
        For Each Config0 In DEV.Options
            i += 1

            LV0 = New ListViewItem
            LV0.SubItems(0).Text = Config0.Key
            LV0.SubItems.Add(Config0.Value.Description)
            LV0.SubItems.Add(Config0.Value.TypeString)
            LV0.SubItems.Add("")
            LV0.SubItems.Add(Config0.Value.ValTextDef)

            If Config0.Value.ConfigType = tDEVconfType.tAction Then
                LV0.SubItems.Add("")
            Else
                If Config0.Value.SaveInFile Then
                    LV0.SubItems.Add("True")
                Else
                    LV0.SubItems.Add("False")
                End If
            End If



            LV0.Tag = Config0.Key

            If Not Config0.Value.Enabled Then
                LV0.ForeColor = Color.DarkGray
            End If

            Me.LvDEVoptions.Items.Add(LV0)

            UpdateDEVconfigs(LV0)

        Next

    End Sub

    Private Sub UpdateDEVconfigs(ByRef LV0 As ListViewItem)
        LV0.SubItems(3).Text = DEV.Options(LV0.Tag).ValText
    End Sub

    Private Sub LvDEVoptions_DoubleClick(sender As Object, e As System.EventArgs) Handles LvDEVoptions.DoubleClick
        Dim Config0 As cDEVoption
        Dim str As String
        Dim i As Integer

        Config0 = DEV.Options(Me.LvDEVoptions.SelectedItems(0).Tag)

        If Not Config0.Enabled Then Exit Sub

        Select Case Config0.ConfigType
            Case tDEVconfType.tAction
                Config0.DoAction()

            Case tDEVconfType.tBoolean
                Config0.BoolVal = Not Config0.BoolVal

            Case tDEVconfType.tSelection

                CmDEVitem = Me.LvDEVoptions.SelectedItems(0)

                CmDEV.Items.Clear()

                i = -1
                For Each str In Config0.Modes
                    i += 1
                    CmDEV.Items.Add("(" & i & ") " & str)
                    CmDEV.Items(i).Tag = i
                Next

                CmDEV.Show(Cursor.Position)

            Case tDEVconfType.tIntVal
                str = InputBox("New Value <" & Config0.TypeString & "> =", , Config0.ValToString)
                If str <> "" AndAlso IsNumeric(str) Then
                    Config0.IntVal = CInt(str)
                End If

            Case tDEVconfType.tSingleVal
                str = InputBox("New Value <" & Config0.TypeString & "> =", , Config0.ValToString)
                If str <> "" AndAlso IsNumeric(str) Then
                    Config0.SingleVal = CSng(str)
                End If

            Case Else 'tDEVconfType.tStringVal
                Dim dlg As New F_StrInpBox
                Config0.StringVal = dlg.ShowDlog("New Value <" & Config0.TypeString & "> =", Config0.StringVal)
        End Select

        UpdateDEVconfigs(Me.LvDEVoptions.SelectedItems(0))

    End Sub

    Private Sub CmDEV_ItemClicked(sender As Object, e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles CmDEV.ItemClicked
        Dim i As Integer

        i = e.ClickedItem.Tag

        DEV.Options(CmDEVitem.Tag).ModeIndex = i

        UpdateDEVconfigs(CmDEVitem)

    End Sub

#End Region

    Public Sub MSGtoForm(ByVal ID As tMsgID, ByVal Msg As String, ByVal Source As String, ByVal Link As String)

        Dim lv0 As ListViewItem
        Dim TimeStr As String
        Dim LogStr As String

        TimeStr = Now.ToString

        lv0 = New ListViewItem
        lv0.Text = Msg
        lv0.SubItems.Add(TimeStr)
        lv0.SubItems.Add(Source)

        LogStr = Msg & " | " & TimeStr & " | " & Source

        If LvMsg.Items.Count > 9999 Then LvMsg.Items.RemoveAt(0)

        Select Case ID

            Case tMsgID.Err

                'Log
                LOGfile.WriteLine("ERROR   | " & LogStr)

                lv0.BackColor = Color.Red
                lv0.ForeColor = Color.White

            Case tMsgID.Warn

                'Log
                LOGfile.WriteLine("WARNING | " & LogStr)

                lv0.BackColor = Color.Khaki              'FromArgb(218, 125, 0) 'DarkOrange
                lv0.ForeColor = Color.Black

            Case Else

                'Log
                LOGfile.WriteLine(LogStr)

                If ID = tMsgID.NewJob Then
                    lv0.BackColor = Color.LightGray
                    lv0.ForeColor = Color.DarkBlue
                End If

        End Select

        If Link <> "" Then
            If Not ID = tMsgID.Err Then lv0.ForeColor = Color.Blue
            lv0.SubItems(0).Font = New Font(Me.LvMsg.Font, FontStyle.Underline)
            lv0.Tag = Link
        End If


        LvMsg.Items.Add(lv0)

        lv0.EnsureVisible()

    End Sub

 



    'Private Sub LvMsg_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles LvMsg.SelectedIndexChanged
    '    If LvMsg.SelectedItems.Count > 0 Then LvMsg.SelectedItems.Clear()
    'End Sub

    'If it is a Link => Open it
    Private Sub LvMsg_MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles LvMsg.MouseClick
        Dim txt As String
        If Me.LvMsg.SelectedIndices.Count > 0 Then
            If Not Me.LvMsg.SelectedItems(0).Tag Is Nothing Then
                If Len(CStr(Me.LvMsg.SelectedItems(0).Tag)) > 4 AndAlso Microsoft.VisualBasic.Left(CStr(Me.LvMsg.SelectedItems(0).Tag), 4) = "<UM>" Then
                    txt = CStr(Me.LvMsg.SelectedItems(0).Tag).Replace("<UM>", MyAppPath & "User Manual")
                    txt = txt.Replace(" ", "%20")
                    txt = txt.Replace("\", "/")
                    txt = "file:///" & txt
                    Try
                        System.Diagnostics.Process.Start(txt)
                    Catch ex As Exception
                        MsgBox("Cannot open link! (-_-;)")
                    End Try
                ElseIf Len(CStr(Me.LvMsg.SelectedItems(0).Tag)) > 5 AndAlso Microsoft.VisualBasic.Left(CStr(Me.LvMsg.SelectedItems(0).Tag), 5) = "<GUI>" Then
                    txt = CStr(Me.LvMsg.SelectedItems(0).Tag).Replace("<GUI>", "")
                    OpenVectoFile(txt)
                Else
                    OpenFiles(CStr(Me.LvMsg.SelectedItems(0).Tag))
                End If
            End If
        End If
    End Sub

    'Hand cursor for links
    Private Sub LvMsg_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles LvMsg.MouseMove
        Dim lv0 As ListViewItem
        lv0 = Me.LvMsg.GetItemAt(e.Location.X, e.Location.Y)
        If lv0 Is Nothing OrElse lv0.Tag Is Nothing Then
            LvMsg.Cursor = Cursors.Arrow
        Else
            LvMsg.Cursor = Cursors.Hand
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
