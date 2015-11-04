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
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports TUGraz.VectoCore.Models.Simulation.Data
Imports TUGraz.VectoCore.Models.Simulation.Impl
Imports TUGraz.VectoCore
Imports TUGraz.VectoCore.Configuration
Imports TUGraz.VectoCore.Models.Simulation

''' <summary>
''' Main application form. Loads at application start. Closing form ends application.
''' </summary>
''' <remarks></remarks>

	Public Class F_MAINForm
	Private JobListView As cFileListView
	Private CycleListView As cFileListView

	Private LastModeName As String
	Private ConMenTarget As ListView
	Private ConMenTarJob As Boolean

	Private MODpath As String
	Private MODVehList As Int32()

	Private CycleTabPage As TabPage
	Private CycleTabPageVisible As Boolean

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

	Private CbDeclLock As Boolean = False


#Region "SLEEP Control - Prevent sleep while VECTO is running"

	Private Declare Function SetThreadExecutionState Lib "kernel32"(ByVal esFlags As Long) As Long

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
		fbFolder = New cFileBrowser("WorkDir", True)
		fbFileLists = New cFileBrowser("FileLists")
		fbVECTO = New cFileBrowser("vecto")
		fbVEH = New cFileBrowser("vveh")
		fbMAP = New cFileBrowser("vmap")
		fbDRI = New cFileBrowser("vdri")
		fbFLD = New cFileBrowser("vfld")
		fbENG = New cFileBrowser("veng")
		fbGBX = New cFileBrowser("vgbx")
		fbACC = New cFileBrowser("vacc")
		fbAUX = New cFileBrowser("vaux")
		fbGBS = New cFileBrowser("vgbs")
		fbRLM = New cFileBrowser("vrlm")
		fbTLM = New cFileBrowser("vtlm")
		fbTCC = New cFileBrowser("vtcc")
		fbCDx = New cFileBrowser("vcdx")

		fbVMOD = New cFileBrowser("vmod")


		'-------------------------------------------------------
		fbFileLists.Extensions = New String() {"txt"}
		fbVECTO.Extensions = New String() {"vecto"}
		fbVEH.Extensions = New String() {"vveh"}
		fbMAP.Extensions = New String() {"vmap"}
		fbDRI.Extensions = New String() {"vdri"}
		fbFLD.Extensions = New String() {"vfld"}
		fbENG.Extensions = New String() {"veng"}
		fbGBX.Extensions = New String() {"vgbx"}
		fbACC.Extensions = New String() {"vacc"}
		fbAUX.Extensions = New String() {"vaux"}
		fbGBS.Extensions = New String() {"vgbs"}
		fbRLM.Extensions = New String() {"vrlm"}
		fbTLM.Extensions = New String() {"vtlm"}
		fbTCC.Extensions = New String() {"vtcc"}
		fbCDx.Extensions = New String() {"vcdv", "vcdb"}

		fbVMOD.Extensions = New String() {"vmod"}
	End Sub

	Private Sub FB_Close()
		fbFolder.Close()
		fbFileLists.Close()
		fbVECTO.Close()
		fbVEH.Close()
		fbMAP.Close()
		fbDRI.Close()
		fbFLD.Close()
		fbENG.Close()
		fbGBX.Close()
		fbACC.Close()
		fbAUX.Close()
		fbGBS.Close()
		fbRLM.Close()
		fbTLM.Close()
		fbTCC.Close()
		fbCDx.Close()
		fbVMOD.Close()
	End Sub

#End Region

#Region "VECTO-Worker"

	'VECTO-Launcher
	Public Sub VECTO_Launcher()
		Dim ProgOverall As Boolean
		Dim GEN0 As cVECTO

		'Called when VECTO already running
		If VECTOworker.IsBusy Then
			GUImsg(tMsgID.Err, "VECTO is already running!")
			Exit Sub
		End If

		'Delete GENlist-Selection
		Me.LvGEN.SelectedItems.Clear()

		'If more than 100 calculations, ask whether to write by-second results
		If Cfg.BatchMode And ((Me.LvGEN.CheckedItems.Count)*(Me.LvDRI.CheckedItems.Count) > 100) And Me.ChBoxModOut.Checked _
			Then
			Select Case _
				MsgBox(
					"You are about to run Batch Mode with " & (Me.LvGEN.CheckedItems.Count)*(Me.LvDRI.CheckedItems.Count) &
					" calculations!" & ChrW(10) & "Do you still want to write modal results?", MsgBoxStyle.YesNoCancel)
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
		If Cfg.BatchMode Or JobFileList.Count > 1 Or Cfg.DeclMode Then
			ProgOverall = True
		Else
			GEN0 = New cVECTO
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

	'Lock certain GUI elements while VECTO is running
	Private Sub LockGUI(ByVal Lock As Boolean)
		GUIlocked = Lock

		Me.PanelOptAllg.Enabled = Not Lock
		Me.GrBoxSTD.Enabled = Not Lock
		Me.GrBoxBATCH.Enabled = Not Lock

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

		Button1.Enabled = Not Lock
		Button2.Enabled = Not Lock

		If DEV.Enabled Then
			Me.LvDEVoptions.Enabled = Not Lock
		End If
	End Sub

	'Define job file list
	Private Sub SetJobList()
		Dim LV0 As ListViewItem
		Dim x As Integer

		JobFileList.Clear()
		CheckedItems.Clear()

		x = - 1
		For Each LV0 In Me.LvGEN.CheckedItems
			x += 1
			LV0.SubItems(1).Text = ""
			CheckedItems.Add(LV0)
			SetCheckedItemColor(x, tJobStatus.Queued)
			JobFileList.Add(fFileRepl(LV0.SubItems(0).Text))
		Next
	End Sub

	'Define cycle list (BATCH mode only)
	Private Sub SetCycleList()
		Dim LV0 As ListViewItem

		JobCycleList.Clear()

		If Cfg.BatchMode Then
			For Each LV0 In Me.LvDRI.CheckedItems
				JobCycleList.Add(fFileRepl(LV0.SubItems(0).Text))
			Next
		End If
	End Sub

	'Job Launcher
	Private Sub Job_Launcher(ByVal ProgOverallEnabled As Boolean)

		If VECTOworker.IsBusy Then Exit Sub

		'Load Options from Options Tab
		SetOptions()

		'Save Config
		Cfg.ConfigSAVE()

		If DEV.Enabled Then DEV.SaveToFile()

		'Reset Msg-output
		ClearMSG()

		'Disable Options
		LockGUI(True)

		'Button switch
		Button1.Enabled = True
		Me.Button1.Text = "STOP"
		Me.Button1.Image = My.Resources.Stop_icon

		'ProgBars start
		If ProgOverallEnabled Then
			Me.ToolStripProgBarOverall.Value = 0
			Me.ToolStripProgBarOverall.Style = ProgressBarStyle.Marquee
			Me.ToolStripProgBarOverall.Visible = True
		End If

		ProgBarCtrl.ProgJobInt = 0
		ProgSecStart()

		'BG-Worker start
		VECTOworker.RunWorkerAsync()
	End Sub

	'Abort Job
	Private Sub JobAbort()
		Me.Button1.Enabled = False
		Me.Button1.Text = "Aborting..."
		Me.Button1.Image = My.Resources.Play_icon_gray
		VECTOworker.CancelAsync()
	End Sub


#Region "BackgroundWorker Events"

	'DoWork - Start Calculations
	Private Sub BackgroundWorker1_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) _
		Handles BackgroundWorker1.DoWork

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

	'Progress Report - Progressbar, Messages, etc.
	Private Sub BackgroundWorker1_ProgressChanged(ByVal sender As Object,
												ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
		Dim x As cWorkProg
		x = e.UserState

		Select Case x.Target
			Case tWorkMsgType.StatusListBox
				MSGtoForm(e.UserState.ID, e.UserState.Msg, x.Source, x.Link)

			Case tWorkMsgType.StatusBar
				Status(e.UserState.Msg)

			Case tWorkMsgType.ProgBars
				Me.ToolStripProgBarOverall.Value = e.ProgressPercentage
				ProgSecStart()

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
	Private Sub BackgroundWorker1_RunWorkerCompleted(ByVal sender As Object,
													ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted

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
			MsgBox("An Unexpected Error occurred!" & ChrW(10) & ChrW(10) &
					e.Error.Message.ToString, MsgBoxStyle.Critical, "Unexpected Error")
			LogFile.WriteToLog(tMsgID.Err, ">>>Unexpected Error:" & e.Error.ToString())
			Me.Close()
		End If

		'Options enable / GUI reset
		LockGUI(False)
		Me.Button1.Text = "START V2.2"
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

#Region "Form Init/Close"

	'Initialise
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

		CycleTabPageVisible = True
		CycleTabPage = Me.TabPageDRI

		DEVpage = Me.TabPageDEV
		'Me.TabControl1.Controls.Remove(DEVpage)

		LastModeName = ""

		ComLineShutDown = False

		FB_Initialize()

		Me.Text = "VECTO " & VECTOvers


		'FileLists
		JobListView = New cFileListView(MyConfPath & "joblist.txt")
		JobListView.LVbox = Me.LvGEN
		CycleListView = New cFileListView(MyConfPath & "cyclelist.txt")
		CycleListView.LVbox = Me.LvDRI

		JobListView.LoadList()

		'Load GUI Options (here, the GEN/ADV/DRI lists are loaded)
		LoadOptions()

		'Resize columns ... after Loading the @file-lists
		Me.LvGEN.Columns(1).Width = - 2
		Me.LvDRI.Columns(1).Width = - 2
		Me.LvMsg.Columns(2).Width = - 2

		'Initialize BackgroundWorker
		VECTOworker = Me.BackgroundWorker1
		VECTOworker.WorkerReportsProgress = True
		VECTOworker.WorkerSupportsCancellation = True

		VECTOworkerV3 = New BackgroundWorker()
		AddHandler VECTOworkerV3.DoWork, AddressOf VectoWorkerV3_OnDoWork
		AddHandler VECTOworkerV3.ProgressChanged, AddressOf VectoWorkerV3_OnProgressChanged
		AddHandler VECTOworkerV3.RunWorkerCompleted, AddressOf VectoWorkerV3_OnRunWorkerCompleted

		VECTOworkerV3.WorkerReportsProgress = True
		VECTOworkerV3.WorkerSupportsCancellation = True


		'Set mode (Batch/Standard)
		ModeUpdate()

		'License check
		'TODO uncomment license check
		If False And Not Lic.LICcheck() Then
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

		DEV.Enabled = True ' Lic.LicFeature(9)

		If DEV.Enabled Then
			DEV.LoadFromFile()
			LoadDEVconfigs()
		End If

		DeclOnOff()
	End Sub

	'Declaration mode GUI settings
	Private Sub DeclOnOff()

		If Cfg.DeclMode Then
			Me.Text = "VECTO " & VECTOvers & " - Declaration Mode"
			Me.CbBatch.Checked = False
			Cfg.DeclInit()
		Else
			Me.Text = "VECTO " & VECTOvers
		End If

		If Cfg.DeclMode Then
			LastModeName = "Declaration"
		Else
			If Cfg.BatchMode Then
				LastModeName = "Batch"
			Else
				LastModeName = "Engineering"
			End If
		End If

		If DEV.Enabled Then
			If Not Cfg.DeclMode Then
				If Not Me.TabControl1.TabPages.Contains(DEVpage) Then _
					Me.TabControl1.TabPages.Insert(Me.TabControl1.TabPages.Count, DEVpage)
				LoadDEVconfigs()
			Else
				If Me.TabControl1.TabPages.Contains(DEVpage) Then Me.TabControl1.Controls.Remove(DEVpage)
				DEV.SetDefault()
			End If
		End If

		Status(LastModeName & " Mode")

		Me.LoadOptions()

		Me.LbDecl.Visible = Cfg.DeclMode

		Me.PnDeclOpt.Enabled = Not Cfg.DeclMode
	End Sub

	'Shown Event (Form-Load finished) ... here StartUp Forms are loaded (DEV, GEN/ADV- Editor ..)
	Private Sub F01_MAINForm_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
		Dim fwelcome As F_Welcome

		'DEV Form
		'If DEV.Enabled And Not Cfg.DeclMode Then
		'	Me.TabControl1.TabPages.Insert(Me.TabControl1.TabPages.Count, DEVpage)
		'End If

		'VECTO Init
		Declaration.Init()

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

	'Open file
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
			Me.CbBatch.Checked = True

			If driFiles.Count > 0 Then
				LvDRI.Items.Clear()
				AddToCycleListView(driFiles.ToArray)
			End If

		Else
			Me.CbBatch.Checked = False
		End If

		'Load Vecto files or open editor (if only one file)
		If vecFiles.Count > 0 Then
			If vecFiles.Count > 1 Or bRUN Then
				LvGEN.Items.Clear()
				AddToJobListView(vecFiles.ToArray)
			Else
				ComFile = vecFiles(0)
			End If
		End If

		'Run or open file editor if file is specified
		If bRUN Then
			VECTO_Launcher()
		Else
			If ComFile <> sKey.NoFile Then OpenVectoFile(ComFile)
		End If
	End Sub

	'Close
	Private Sub F01_MAINForm_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) _
		Handles Me.FormClosing

		'Save File-Lists
		SaveFileLists()

		'Close log
		LogFile.CloseLog()

		'Config save
		SetOptions()
		Cfg.ConfigSAVE()
		If DEV.Enabled Then DEV.SaveToFile()

		'File browser instances close
		FB_Close()
	End Sub

#End Region

	'Open file - Job, vehicle, engine, gearbox or signature file
	Public Sub OpenVectoFile(ByVal File As String)

		If Not IO.File.Exists(File) Then

			GUImsg(tMsgID.Err, "File not found! (" & File & ")")
			MsgBox("File not found! (" & File & ")", MsgBoxStyle.Critical)

		Else

			Select Case UCase(fEXT(File))
				Case ".VGBX"
					If Not F_GBX.Visible Then
						F_GBX.Show()
					Else
						F_GBX.JobDir = ""
						If F_GBX.WindowState = FormWindowState.Minimized Then F_GBX.WindowState = FormWindowState.Normal
						F_GBX.BringToFront()
					End If
					F_GBX.openGBX(File)
				Case ".VVEH"
					If Not F_VEH.Visible Then
						F_VEH.Show()
					Else
						F_VEH.JobDir = ""
						If F_VEH.WindowState = FormWindowState.Minimized Then F_VEH.WindowState = FormWindowState.Normal
						F_VEH.BringToFront()
					End If
					F_VEH.openVEH(File)
				Case ".VENG"
					If Not F_ENG.Visible Then
						F_ENG.Show()
					Else
						F_ENG.JobDir = ""
						If F_ENG.WindowState = FormWindowState.Minimized Then F_ENG.WindowState = FormWindowState.Normal
						F_ENG.BringToFront()
					End If
					F_ENG.openENG(File)
				Case ".VECTO"
					OpenVECTOeditor(File)
				Case ".VSIG"
					OpenSigFile(File)
				Case Else
					MsgBox("Type '" & fEXT(File) & "' unknown!", MsgBoxStyle.Critical)
			End Select

		End If
	End Sub

#Region "Job file list"

#Region "Events"

	Private Sub ButtonGENremove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles ButtonGENremove.Click
		RemoveJobFile()
	End Sub

	Private Sub ButtonGENadd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonGENadd.Click
		AddJobFile()
	End Sub

	Private Sub ButtonGENoptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles ButtonGENopt.Click
		ConMenTarget = Me.LvGEN
		ConMenTarJob = True

		'Locked functions show/hide
		Me.LoadListToolStripMenuItem.Enabled = Not GUIlocked
		Me.LoadDefaultListToolStripMenuItem.Enabled = Not GUIlocked
		Me.ClearListToolStripMenuItem.Enabled = Not GUIlocked

		Me.ConMenFilelist.Show(Control.MousePosition)
	End Sub

	Private Sub ListViewGEN_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) _
		Handles LvGEN.KeyDown
		Select Case e.KeyCode
			Case Keys.Delete, Keys.Back
				If Not GUIlocked Then RemoveJobFile()
			Case Keys.Enter
				OpenJobFile()
		End Select
	End Sub

	Private Sub ListViewGEN_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles LvGEN.DoubleClick
		If Me.LvGEN.SelectedItems.Count > 0 Then
			Me.LvGEN.SelectedItems(0).Checked = Not Me.LvGEN.SelectedItems(0).Checked
			OpenJobFile()
		End If
	End Sub

	Private Sub LvGEN_ItemChecked(sender As Object, e As System.Windows.Forms.ItemCheckedEventArgs) _
		Handles LvGEN.ItemChecked

		If e.Item.Checked Then
			GENchecked += 1
		Else
			GENchecked -= 1
		End If

		If CheckLock Then Exit Sub
		UpdateJobTabText()
	End Sub

	Private Sub ChBoxAllGEN_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles ChBoxAllGEN.CheckedChanged

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
		UpdateJobTabText()
	End Sub

	Private Sub ListGEN_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) _
		Handles LvGEN.DragEnter
		If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then
			e.Effect = DragDropEffects.Copy
		End If
	End Sub

	Private Sub ListGEN_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) _
		Handles LvGEN.DragDrop
		Dim f As String()
		f = CType(e.Data.GetData(DataFormats.FileDrop), Array)
		AddToJobListView(f)
	End Sub

	Private Sub BtGENup_Click(sender As System.Object, e As System.EventArgs) Handles BtGENup.Click
		MoveItem(LvGEN, True)
	End Sub

	Private Sub BtGENdown_Click(sender As System.Object, e As System.EventArgs) Handles BtGENdown.Click
		MoveItem(LvGEN, False)
	End Sub

#End Region

	'Remove selected file(s) from job list
	Private Sub RemoveJobFile()
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

		For i = UBound(SelIx) To 0 Step - 1
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
		UpdateJobTabText()
	End Sub

	'Browse for job file(s) and add to job list with AddToJobListView
	Private Sub AddJobFile()
		Dim x As String()
		Dim Chck As Boolean = False

		x = New String() {""}

		'STANDARD/BATCH
		If fbVECTO.OpenDialog("", True, "vecto") Then
			Chck = True
			x = fbVECTO.Files
		End If

		If Chck Then AddToJobListView(x)
	End Sub

	'Open file in list
	Private Sub OpenJobFile()
		Dim f As String

		If Me.LvGEN.SelectedItems.Count < 1 Then
			If Me.LvGEN.Items.Count = 1 Then
				Me.LvGEN.Items(0).Selected = True
			Else
				Exit Sub
			End If
		End If

		f = Me.LvGEN.SelectedItems(0).SubItems(0).Text
		f = fFileRepl(f)
		If Not IO.File.Exists(f) Then
			MsgBox(f & " not found!")
		Else
			OpenVECTOeditor(f)
		End If
	End Sub

	'Add File to job listview (multiple files)
	Private Sub AddToJobListView(ByVal Path As String(), Optional ByVal Txt As String = " ")
		Dim pDim As Int16
		Dim p As Int16
		Dim f As Int16
		Dim fList As String()
		Dim fListDim As Int16 = - 1
		Dim ListViewItem0 As ListViewItem

		'If VECTO runs: Cancel operation (because Mode-change during calculation is not very clever)
		If VECTOworker.IsBusy Then Exit Sub

		pDim = UBound(Path)
		ReDim fList(0)	   'um Nullverweisausnahme-Warnung zu verhindern

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
			ListViewItem0 = New ListViewItem(Path(p))	'fFileWD(Path(p)))
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
		UpdateJobTabText()
	End Sub

	'Add File to job listview (single file)
	Public Sub AddToJobListView(ByVal Path As String, Optional ByVal Txt As String = " ")
		Dim p(0) As String
		p(0) = Path
		AddToJobListView(p, Txt)
	End Sub

	'Update job files counter in tab titel
	Private Sub UpdateJobTabText()
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

#Region "Cycle list (BATCH)"


#Region "Events"

	Private Sub ButtonDRIadd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDRIadd.Click
		AddCycle()
	End Sub

	Private Sub ButtonDRIremove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles ButtonDRIremove.Click
		RemoveCycle()
	End Sub

	Private Sub ButtonDRIoptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles ButtonDRIedit.Click
		ConMenTarget = Me.LvDRI
		ConMenTarJob = False
		Me.ConMenFilelist.Show(Control.MousePosition)
	End Sub

	Private Sub LvDRI_ItemChecked(sender As Object, e As System.Windows.Forms.ItemCheckedEventArgs) _
		Handles LvDRI.ItemChecked

		If e.Item.Checked Then
			DRIchecked += 1
		Else
			DRIchecked -= 1
		End If

		If CheckLock Then Exit Sub
		UpdateCycleTabText()
	End Sub

	Private Sub ChBoxAllDRI_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles ChBoxAllDRI.CheckedChanged
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
		UpdateCycleTabText()
	End Sub

	Private Sub ListViewDRI_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles LvDRI.DoubleClick
		If Me.LvDRI.SelectedItems.Count > 0 Then
			Me.LvDRI.SelectedItems(0).Checked = Not Me.LvDRI.SelectedItems(0).Checked
			OpenCycle()
		Else
			AddCycle()
		End If
	End Sub

	Private Sub ListViewDRI_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) _
		Handles LvDRI.KeyDown
		Select Case e.KeyCode
			Case Keys.Delete, Keys.Back
				If Not GUIlocked Then RemoveCycle()
			Case Keys.Enter
				OpenCycle()
		End Select
	End Sub

	'Drag n' Drop
	Private Sub ListDRI_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) _
		Handles LvDRI.DragEnter
		If (e.Data.GetDataPresent(DataFormats.FileDrop)) Then e.Effect = DragDropEffects.Copy
	End Sub

	Private Sub ListDRI_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) _
		Handles LvDRI.DragDrop
		Dim f As String()
		f = CType(e.Data.GetData(DataFormats.FileDrop), Array)
		AddToCycleListView(f)
		If LvDRI.Items.Count > 0 Then LvDRI.Items(LvDRI.Items.Count - 1).Selected = True
	End Sub

	Private Sub BtDRIup_Click(sender As System.Object, e As System.EventArgs) Handles BtDRIup.Click
		MoveItem(LvDRI, True)
	End Sub

	Private Sub BtDRIdown_Click(sender As System.Object, e As System.EventArgs) Handles BtDRIdown.Click
		MoveItem(LvDRI, False)
	End Sub

#End Region

	'Remove selected file(s) from cycle list
	Private Sub RemoveCycle()
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

		For i = UBound(SelIx) To 0 Step - 1
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
		UpdateCycleTabText()
	End Sub

	'Browse for cycle file(s) and add to cycle list with AddToCycleListView
	Private Sub AddCycle()
		If fbDRI.OpenDialog("", True) Then
			AddToCycleListView(fbDRI.Files)
			If LvDRI.Items.Count > 0 Then LvDRI.Items(LvDRI.Items.Count - 1).Selected = True
		End If
	End Sub

	'Open cycle in list
	Private Sub OpenCycle()

		If Me.LvDRI.SelectedItems.Count < 1 Then
			If Me.LvDRI.Items.Count = 1 Then
				Me.LvDRI.Items(0).Selected = True
			Else
				Exit Sub
			End If
		End If

		OpenFiles(fFileRepl(Me.LvDRI.SelectedItems(0).SubItems(0).Text))
	End Sub

	'Add File to cycle listview (multiple files)
	Private Sub AddToCycleListView(ByVal Path As String())
		Dim pDim As Int16
		Dim p As Int16
		Dim ListViewItem0 As ListViewItem

		pDim = UBound(Path)

		Me.LvDRI.BeginUpdate()
		CheckLock = True

		'Mode switch if necessary
		If Not Me.CbBatch.Checked Then Me.CbBatch.Checked = True

		For p = 0 To pDim
			ListViewItem0 = New ListViewItem(Path(p))	'fFileWD(Path(p)))
			ListViewItem0.SubItems.Add(" ")
			ListViewItem0.Checked = True
			Me.LvDRI.Items.Add(ListViewItem0)
			lbFound:
		Next

		Me.LvDRI.EndUpdate()
		CheckLock = False

		'Number update
		DRIchecked = Me.LvDRI.CheckedItems.Count
		UpdateCycleTabText()
	End Sub

	'Add File to cycle listview (single file)
	Private Sub AddToCycleListView(ByVal Path As String)
		Dim p(0) As String
		p(0) = Path
		AddToCycleListView(p)
	End Sub

	'Update cycle files counter in tab titel
	Private Sub UpdateCycleTabText()
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

	'New Job file
	Private Sub ToolStripBtNew_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtNew.Click
		OpenVECTOeditor("<New>")
	End Sub

	'Open input file
	Private Sub ToolStripBtOpen_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtOpen.Click

		If fbVECTO.OpenDialog("", False, "vecto,vveh,vgbx,veng,vsig") Then
			OpenVectoFile(fbVECTO.Files(0))
		End If
	End Sub

	Private Sub GENEditorToolStripMenuItem1_Click(sender As System.Object, e As System.EventArgs) _
		Handles GENEditorToolStripMenuItem1.Click
		OpenVECTOeditor("<New>")
	End Sub

	Private Sub VEHEditorToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles VEHEditorToolStripMenuItem.Click
		If Not F_VEH.Visible Then
			F_VEH.Show()
		Else
			If F_VEH.WindowState = FormWindowState.Minimized Then F_VEH.WindowState = FormWindowState.Normal
			F_VEH.BringToFront()
		End If
	End Sub

	Private Sub EngineEditorToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles EngineEditorToolStripMenuItem.Click
		If Not F_ENG.Visible Then
			F_ENG.Show()
		Else
			If F_ENG.WindowState = FormWindowState.Minimized Then F_ENG.WindowState = FormWindowState.Normal
			F_ENG.BringToFront()
		End If
	End Sub

	Private Sub GearboxEditorToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles GearboxEditorToolStripMenuItem.Click
		If Not F_GBX.Visible Then
			F_GBX.Show()
		Else
			If F_GBX.WindowState = FormWindowState.Minimized Then F_GBX.WindowState = FormWindowState.Normal
			F_GBX.BringToFront()
		End If
	End Sub

	Private Sub GraphToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles GraphToolStripMenuItem.Click
		Dim FGraph As New F_Graph
		FGraph.Show()
	End Sub

	Private Sub SignOrVerifyFilesToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles SignOrVerifyFilesToolStripMenuItem.Click
		If Not F_FileSign.Visible Then
			F_FileSign.Show()
		Else
			If F_FileSign.WindowState = FormWindowState.Minimized Then F_FileSign.WindowState = FormWindowState.Normal
			F_FileSign.BringToFront()
		End If
	End Sub

	Private Sub OpenLogToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles OpenLogToolStripMenuItem.Click
		System.Diagnostics.Process.Start(MyAppPath & "log.txt")
	End Sub

	Private Sub SettingsToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles SettingsToolStripMenuItem.Click
		F_Settings.ShowDialog()
	End Sub

	Private Sub UserManualToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles UserManualToolStripMenuItem.Click
		If IO.File.Exists(MyAppPath & "User Manual\usermanual.html") Then
			System.Diagnostics.Process.Start(MyAppPath & "User Manual\usermanual.html")
		Else
			MsgBox("User Manual not found!", MsgBoxStyle.Critical)
		End If
	End Sub

	Private Sub UpdateNotesToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles UpdateNotesToolStripMenuItem.Click
		If IO.File.Exists(MyAppPath & "User Manual\Release Notes.pdf") Then
			System.Diagnostics.Process.Start(MyAppPath & "User Manual\Release Notes.pdf")
		Else
			MsgBox("Release Notes not found!", MsgBoxStyle.Critical)
		End If
	End Sub

	Private Sub ReportBugViaCITnetToolStripMenuItem_Click(sender As Object, e As EventArgs) _
		Handles ReportBugViaCITnetToolStripMenuItem.Click
		F_JIRA.ShowDialog()
	End Sub

	Private Sub CreateActivationFileToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles CreateActivationFileToolStripMenuItem.Click
		If MsgBox("Create Activation File ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
			If Lic.CreateActFile(MyAppPath & "ActivationCode.dat") Then
				GUImsg(tMsgID.Normal, "Activation File created.")
			Else
				GUImsg(tMsgID.Err, "Failed to create Activation File!")
				MsgBox("ERROR! Failed to create Activation File!", MsgBoxStyle.Critical)
			End If
		End If
	End Sub

	Private Sub AboutVECTOToolStripMenuItem1_Click(sender As System.Object, e As System.EventArgs) _
		Handles AboutVECTOToolStripMenuItem1.Click
		F_AboutBox.ShowDialog()
	End Sub


#End Region

	'Move job/cycle file up or down in list view
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


#Region "job/cycle file List - Context Menu"

	'Save List
	Private Sub SaveListToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles SaveListToolStripMenuItem.Click
		If fbFileLists.SaveDialog("") Then
			If ConMenTarJob Then
				JobListView.SaveList(fbFileLists.Files(0))
			Else
				CycleListView.SaveList(fbFileLists.Files(0))
			End If
		End If
	End Sub

	'Load List
	Private Sub LoadListToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles LoadListToolStripMenuItem.Click

		If GUIlocked Then Exit Sub

		If fbFileLists.OpenDialog("") Then

			If ConMenTarJob Then 'GEN
				JobListView.LoadList(fbFileLists.Files(0))
				GENchecked = Me.LvGEN.CheckedItems.Count
				UpdateJobTabText()
			Else 'DRI
				'Mode toggle 
				If Not Me.CbBatch.Checked Then Me.CbBatch.Checked = True
				CycleListView.LoadList(fbFileLists.Files(0))
				DRIchecked = Me.LvDRI.CheckedItems.Count
				UpdateCycleTabText()
			End If

		End If
	End Sub

	'Load Default List
	Private Sub LoadDefaultListToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles LoadDefaultListToolStripMenuItem.Click

		If GUIlocked Then Exit Sub

		If ConMenTarJob Then

			JobListView.LoadList()

			GENchecked = Me.LvGEN.CheckedItems.Count
			UpdateJobTabText()
		Else
			CycleListView.LoadList()
			DRIchecked = Me.LvDRI.CheckedItems.Count
			UpdateCycleTabText()
		End If
	End Sub

	'Clear List
	Private Sub ClearListToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles ClearListToolStripMenuItem.Click
		'Dim ListViewItem0 As ListViewItem
		'For Each ListViewItem0 In ConMenTarget.SelectedItems
		'    ListViewItem0.Remove()
		'Next
		If GUIlocked Then Exit Sub

		ConMenTarget.Items.Clear()
		If ConMenTarJob Then
			GENchecked = Me.LvGEN.CheckedItems.Count
			UpdateJobTabText()
		Else
			DRIchecked = Me.LvDRI.CheckedItems.Count
			UpdateCycleTabText()
		End If
	End Sub

#End Region

	'VECTO Start button - Calls VECTO_Launcher or aborts calculation
	Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

		'VECTO Start/Stop
		If VECTOworker.IsBusy Then

			'If VECTO already running: STOP
			ComLineShutDown = False
			JobAbort()

		Else

			'...Otherwise: START

			'Save Lists if Crash
			SaveFileLists()

			'Start
			VECTO_Launcher()

		End If
	End Sub

	Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
		If Not VECTOworkerV3.IsBusy Then
			'Save Lists for Crash
			SaveFileLists()

			LvGEN.SelectedItems.Clear()

			If LvGEN.CheckedItems.Count = 0 Then
				GUImsg(tMsgID.Err, "No job file selected!")
				Exit Sub
			End If

			Status("Launching VECTO V3...")
			JobFileList.Clear()
			JobFileList.AddRange(From listViewItem In LvGEN.CheckedItems Select fFileRepl(listViewItem.SubItems(0).Text))

			SetOptions()
			Cfg.ConfigSAVE()
			ClearMSG()

			LockGUI(True)
			Button2.Enabled = True
			Button2.Text = "STOP"
			Button2.Image = My.Resources.Stop_icon

			ToolStripProgBarOverall.Value = 0
			ToolStripProgBarOverall.Style = ProgressBarStyle.Continuous
			ToolStripProgBarOverall.Visible = True

			VECTOworkerV3.RunWorkerAsync()
		Else
			Button2.Enabled = False
			Button2.Text = "Aborting..."
			Button2.Image = My.Resources.Play_icon_gray
			VECTOworkerV3.CancelAsync()
		End If
	End Sub

	Private Sub VectoWorkerV3_OnDoWork(sender As BackgroundWorker, e As DoWorkEventArgs)
		AllowSleepOFF()

		Dim sumFileName As String = Path.GetFileNameWithoutExtension(JobFileList(0) + Constants.FileExtensions.SumFile)
		Dim sumWriter As SummaryFileWriter = New SummaryFileWriter(sumFileName)
		Dim jobContainer As JobContainer = New JobContainer(sumWriter)

		For Each jobFile As String In JobFileList
			sender.ReportProgress(0, New With {.Target = "ListBox", .Message = "Reading File " + jobFile})
			Dim runsFactory As SimulatorFactory = New SimulatorFactory(SimulatorFactory.FactoryMode.DeclarationMode, jobFile)
			jobContainer.AddRuns(runsFactory)
			sender.ReportProgress(0, New With {.Target = "ListBox", .Message = "Finished Reading File " + jobFile})
		Next


		sender.ReportProgress(0,
							New _
								With {.Target = "ListBox",
								.Message = _
								String.Format("Starting Simulation ({0} Jobs, {1} Runs)", JobFileList.Count, jobContainer.GetProgress().Count)})

		jobContainer.Execute(True)
		Dim start As DateTime
		While Not jobContainer.AllCompleted
			If sender.CancellationPending Then
				jobContainer.Cancel()
				Return
			End If

			Dim progress As Dictionary(Of String, JobContainer.ProgressEntry) = jobContainer.GetProgress()

			Dim NumLines As Double = progress.Count
			Dim sumProgress As Double = progress.Sum(Function(pair) pair.Value.Progress)


			If sumProgress > 0 And start = DateTime.MinValue Then
				start = DateTime.Now()
			End If

			Dim sumPercent As Integer = Int(sumProgress/NumLines*100)

			Dim progString As String = String.Join(", ", progress.Select(Function(pair) String.Format("{0,4:P}", pair.Value)))

			Dim duration As Double = 0.0
			Dim remainingDuration As Double = 0
			If start > DateTime.MinValue Then
				duration = (DateTime.Now() - start).TotalSeconds
				remainingDuration = duration/(sumProgress/NumLines) - duration
			End If


			sender.ReportProgress(sumPercent,
								New _
									With {.Target = "Status",
									.Message = String.Format("Time: {0:0}s, Remaining: {1:0}s, Current Progress: {2:P} ({3})",
															duration, remainingDuration, sumPercent/100, progString)
									})
			Thread.Sleep(1000)
		End While
	End Sub

	Private Sub VectoWorkerV3_OnProgressChanged(sender As Object, e As ProgressChangedEventArgs)
		ToolStripProgBarOverall.Value = e.ProgressPercentage
		If e.UserState.Target = "ListBox" Then
			MSGtoForm(tMsgID.Normal, e.UserState.Message, "", "")
		ElseIf e.UserState.Target = "Status" Then
			Status(e.UserState.Message)
		End If
	End Sub

	Private Sub VectoWorkerV3_OnRunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs)
		Dim Result As tCalcResult

		'Progbar reset
		ToolStripProgBarOverall.Visible = False
		ToolStripProgBarOverall.Style = ProgressBarStyle.Continuous
		ToolStripProgBarOverall.Value = 0
		ProgSecStop()

		LvGEN.SelectedIndices.Clear()

		Result = e.Result

		'ShutDown when Unexpected Error
		If e.Error IsNot Nothing Then
			MsgBox("An Unexpected Error occurred!" & ChrW(10) & ChrW(10) &
					e.Error.Message.ToString, MsgBoxStyle.Critical, "Unexpected Error")
			LogFile.WriteToLog(tMsgID.Err, ">>>Unexpected Error:" & e.Error.ToString())
		End If

		'Options enable / GUI reset
		LockGUI(False)
		Button2.Text = "START V3"
		Button2.Image = My.Resources.Play_icon
		Status(LastModeName & " Mode")

		'SLEEP reactivate
		AllowSleepON()
	End Sub

	'Mode Change (STANDARD/BATCH)
	Private Sub CbBatch_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles CbBatch.CheckedChanged
		ModeUpdate()
	End Sub

	Private Sub ModeUpdate()

		'Save lists
		JobListView.SaveList()
		If Cfg.BatchMode Then CycleListView.SaveList()

		'New mode
		Cfg.BatchMode = Me.CbBatch.Checked

		'GUI changes according to current mode

		If Cfg.BatchMode Then

			LastModeName = "Batch"

			'Load cycle list
			CycleListView.LoadList()

			'Update cycle counter
			DRIchecked = Me.LvDRI.CheckedItems.Count
			UpdateCycleTabText()

			'Show mode-specific settings
			Me.GrBoxSTD.Visible = False
			Me.GrBoxBATCH.Visible = True

			'Show Cycle Tab Page
			If Not CycleTabPageVisible Then
				Me.TabControl1.TabPages.Insert(1, CycleTabPage)
				CycleTabPageVisible = True
			End If

		Else

			If Cfg.DeclMode Then
				LastModeName = "Declaration"
			Else
				LastModeName = "Engineering"
			End If

			'Show mode-specific settings
			Me.GrBoxSTD.Visible = False	 'Currently no specific settings for STANDARD mode, therefore always 'False'
			Me.GrBoxBATCH.Visible = False

			'Hide Cycle Tab Page
			If CycleTabPageVisible Then
				Me.TabControl1.Controls.Remove(CycleTabPage)
				CycleTabPageVisible = False
			End If

		End If

		'Update job counter
		GENchecked = Me.LvGEN.CheckedItems.Count
		UpdateJobTabText()

		'Status label
		Status(LastModeName & " Mode")
	End Sub

	'Class for ListView control - Job and cycle lists
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
	End Class

	'Set color of job files in list (Error, Warnings, Currently running, etc.)
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
				lv0.ForeColor = Color.DarkBlue		'FromArgb(218, 125, 0) 'DarkOrange 'OrangeRed
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

	'Open Job Editor and open file (or new file)
	Friend Sub OpenVECTOeditor(ByVal x As String)

		If Not F_VECTO.Visible Then
			F_VECTO.Show()
		Else
			If F_VECTO.WindowState = FormWindowState.Minimized Then F_VECTO.WindowState = FormWindowState.Normal
			F_VECTO.BringToFront()
		End If

		If x = "<New>" Then
			F_VECTO.VECTOnew()
		Else
			F_VECTO.VECTOload2Form(x)
		End If

		F_VECTO.Activate()
	End Sub

	'Open signature file (.vsig)
	Friend Sub OpenSigFile(ByVal file As String)
		If Not F_FileSign.Visible Then
			F_FileSign.Show()

		End If
		F_FileSign.WindowState = FormWindowState.Normal
		F_FileSign.TbSigFile.Text = file
		F_FileSign.VerifySigFile()
		F_FileSign.Activate()
	End Sub

	'Save job and cycle file lists
	Private Sub SaveFileLists()
		JobListView.SaveList()
		If Cfg.BatchMode Then CycleListView.SaveList()
	End Sub


#Region "Progressbar controls"

	'Initialise progress bar (Start of next job in calculation)
	Private Sub ProgSecStart()
		Me.ToolStripProgBarJob.Value = 0
		Me.ToolStripProgBarJob.Style = ProgressBarStyle.Marquee
		Me.ToolStripProgBarJob.Visible = True
		Me.TmProgSec.Start()
	End Sub

	'Stop - Hide progress bar
	Private Sub ProgSecStop()
		Me.TmProgSec.Stop()
		Me.ToolStripProgBarJob.Visible = False
		Me.ToolStripProgBarJob.Value = 0
	End Sub

	'Timer to update progress bar regularly
	Private Sub TmProgSec_Tick(sender As Object, e As System.EventArgs) Handles TmProgSec.Tick
		If GUItest0.TestActive Then
			Call GUItest0.TestTick()
			Exit Sub
		Else
			If Not ProgBarCtrl.ProgLock Then ProgSecUpdate()
		End If
	End Sub

	'Update progress bar (timer controlled)
	Private Sub ProgSecUpdate()

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

			If .ProgOverallStartInt > - 1 Then
				Me.ToolStripProgBarOverall.Value =
					CInt(.ProgOverallStartInt + (.PgroOverallEndInt - .ProgOverallStartInt)*.ProgJobInt/100)
			End If

		End With
	End Sub


#End Region

#Region "Options Tab"

	'Load options from config class
	Public Sub LoadOptions()
		Me.ChBoxCyclDistCor.Checked = Cfg.DistCorr
		Me.ChBoxUseGears.Checked = Cfg.GnUfromCycle
		Me.ChBoxModOut.Checked = Cfg.ModOut
		CbBOmode.SelectedIndex = - 1
		Select Case UCase(Cfg.BATCHoutpath)
			Case sKey.JobPath
				CbBOmode.SelectedIndex = 0
			Case Else
				CbBOmode.SelectedIndex = 1
				Me.TbBOpath.Text = Cfg.BATCHoutpath
		End Select
		Me.ChBoxBatchSubD.Checked = Cfg.BATCHoutSubD

		'Set Mode
		If Not Cfg.DeclMode Then Me.CbBatch.Checked = Cfg.BatchMode

		Me.RbDecl.Checked = Cfg.DeclMode
	End Sub

	'Update config class from options in GUI, e.g. before running calculations 
	Private Sub SetOptions()

		'General(Allgemein)
		Cfg.DistCorr = Me.ChBoxCyclDistCor.Checked
		Cfg.GnUfromCycle = Me.ChBoxUseGears.Checked

		'BATCH
		Cfg.ModOut = Me.ChBoxModOut.Checked
		Select Case CbBOmode.SelectedIndex
			Case 0
				Cfg.BATCHoutpath = sKey.JobPath
			Case Else
				Cfg.BATCHoutpath = Trim(Me.TbBOpath.Text)
				If Microsoft.VisualBasic.Right(Cfg.BATCHoutpath, 1) <> "\" Then Cfg.BATCHoutpath &= "\"
		End Select
		Cfg.BATCHoutSubD = Me.ChBoxBatchSubD.Checked

		DEV.SetOptions()
	End Sub

#Region "Events"

	Private Sub ChBoxAutoSD_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles ChBoxAutoSD.CheckedChanged
		Me.LbAutoShDown.Visible = Me.ChBoxAutoSD.Checked
	End Sub

	Private Sub CbBOmode_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles CbBOmode.SelectedIndexChanged
		Me.TbBOpath.Visible = (Me.CbBOmode.SelectedIndex = 2)
		Me.ButBObrowse.Visible = (Me.CbBOmode.SelectedIndex = 2)
	End Sub

	Private Sub ButBObrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButBObrowse.Click
		If fbFolder.OpenDialog(Me.TbBOpath.Text) Then
			Me.TbBOpath.Text = fbFolder.Files(0)
		End If
	End Sub

	Private Sub ChBoxBatchOut_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
		Handles ChBoxModOut.CheckedChanged
		Me.ChBoxBatchSubD.Enabled = Me.ChBoxModOut.Checked
	End Sub

#End Region


#End Region

#Region "Developer options (DEV) Tab"

	'Load DEV options
	Private Sub LoadDEVconfigs()
		Dim LV0 As ListViewItem
		Dim i As Integer
		Dim Config0 As KeyValuePair(Of String, cDEVoption)

		Me.LvDEVoptions.Items.Clear()

		i = - 1
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

	'Update value of specific DEV option
	Private Sub UpdateDEVconfigs(ByRef LV0 As ListViewItem)
		DEV.UpdateDevConfigs()
	End Sub

	'Change value of DEV option or execute action-type DEV options
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

				i = - 1
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

	'Context menu for selection-type DEV options
	Private Sub CmDEV_ItemClicked(sender As Object, e As System.Windows.Forms.ToolStripItemClickedEventArgs) _
		Handles CmDEV.ItemClicked
		Dim i As Integer

		i = e.ClickedItem.Tag

		DEV.Options(CmDEVitem.Tag).ModeIndex = i

		UpdateDEVconfigs(CmDEVitem)
	End Sub


#End Region

	'Add message to message list
	Public Sub MSGtoForm(ByVal ID As tMsgID, ByVal Msg As String, ByVal Source As String, ByVal Link As String)

		Dim lv0 As ListViewItem

		lv0 = New ListViewItem
		lv0.Text = Msg
		lv0.SubItems.Add(Now.ToString)
		lv0.SubItems.Add(Source)

		If LvMsg.Items.Count > 9999 Then LvMsg.Items.RemoveAt(0)

		LogFile.WriteToLog(ID, Msg & vbTab & Source)

		Select Case ID

			Case tMsgID.Err

				lv0.BackColor = Color.Red
				lv0.ForeColor = Color.White

			Case tMsgID.Warn

				lv0.BackColor = Color.Khaki				 'FromArgb(218, 125, 0) 'DarkOrange
				lv0.ForeColor = Color.Black

			Case Else

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


	'Open link in message list
	Private Sub LvMsg_MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles LvMsg.MouseClick
		Dim txt As String
		If Me.LvMsg.SelectedIndices.Count > 0 Then
			If Not Me.LvMsg.SelectedItems(0).Tag Is Nothing Then
				If _
					Len(CStr(Me.LvMsg.SelectedItems(0).Tag)) > 4 AndAlso
					Microsoft.VisualBasic.Left(CStr(Me.LvMsg.SelectedItems(0).Tag), 4) = "<UM>" Then
					txt = CStr(Me.LvMsg.SelectedItems(0).Tag).Replace("<UM>", MyAppPath & "User Manual")
					txt = txt.Replace(" ", "%20")
					txt = txt.Replace("\", "/")
					txt = "file:///" & txt
					Try
						System.Diagnostics.Process.Start(txt)
					Catch ex As Exception
						MsgBox("Cannot open link! (-_-;)")
					End Try
				ElseIf _
					Len(CStr(Me.LvMsg.SelectedItems(0).Tag)) > 5 AndAlso
					Microsoft.VisualBasic.Left(CStr(Me.LvMsg.SelectedItems(0).Tag), 5) = "<GUI>" Then
					txt = CStr(Me.LvMsg.SelectedItems(0).Tag).Replace("<GUI>", "")
					OpenVectoFile(txt)
				ElseIf _
					Len(CStr(Me.LvMsg.SelectedItems(0).Tag)) > 5 AndAlso
					Microsoft.VisualBasic.Left(CStr(Me.LvMsg.SelectedItems(0).Tag), 5) = "<RUN>" Then
					txt = CStr(Me.LvMsg.SelectedItems(0).Tag).Replace("<RUN>", "")
					Try
						Process.Start(txt)
					Catch ex As Exception
						GUImsg(tMsgID.Err, "Could not run '" & txt & "'!")
					End Try
				Else
					OpenFiles(CStr(Me.LvMsg.SelectedItems(0).Tag))
				End If
			End If
		End If
	End Sub

	'Link-cursor (Hand) for links
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

	'Initialise and open context menu
	Private Sub OpenFiles(ParamArray files() As String)

		If files.Length = 0 Then Exit Sub

		CmFiles = files

		Me.OpenInGraphWindowToolStripMenuItem.Enabled = (UCase(fEXT(CmFiles(0))) = ".VMOD")


		OpenWithToolStripMenuItem.Text = "Open with " & Cfg.OpenCmdName

		CmOpenFile.Show(Cursor.Position)
	End Sub

	'Open with tool defined in Settings
	Private Sub OpenWithToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles OpenWithToolStripMenuItem.Click
		If Not FileOpenAlt(CmFiles(0)) Then MsgBox("Failed to open file!")
	End Sub

	Private Sub OpenInGraphWindowToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles OpenInGraphWindowToolStripMenuItem.Click
		Dim FGraph As New F_Graph
		FGraph.Show()
		FGraph.LoadNewFile(CmFiles(0))
	End Sub

	'Show in folder
	Private Sub ShowInFolderToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) _
		Handles ShowInFolderToolStripMenuItem.Click
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

	'Change Declaraion Mode
	Private Sub RbDecl_CheckedChanged(sender As Object, e As System.EventArgs) Handles RbDecl.CheckedChanged
		If CbDeclLock Then Exit Sub

		If F_VECTO.Visible Or F_VEH.Visible Or F_GBX.Visible Or F_ENG.Visible Then
			CbDeclLock = True
			Me.RbDecl.Checked = Not Me.RbDecl.Checked
			CbDeclLock = False
			MsgBox("Please close all dialog windows (e.g. Job Editor) before changing mode!")
		Else
			Cfg.DeclMode = Me.RbDecl.Checked
			Me.RbDev.Checked = Not Me.RbDecl.Checked
			DeclOnOff()
		End If
	End Sub


#Region "GUI Tests"

	Private GUItest0 As New GUItest(Me)

	Private Class GUItest
		Private RowLim As Int16 = 9
		Private ColLim As Int16 = 45
		Public TestActive As Boolean = False
		Private TestAborted As Boolean
		Private xCtrl As Int16
		Private xPanel As Int16
		Private Scr As Int32
		Private PRbAlt As Boolean
		Private Ctrls(RowLim + 1) As Int16
		Private Pnls(RowLim + 1) As Int16
		Private CtrlC As Int16
		Private CtrlCL As Int16
		Private PnDir As Int16
		Private PnDirC As Int16
		Private PnDirCL As Int16
		Private PnDirRnd As Int16
		Private CtrlRnd As Int16
		Private DiffC As Int16
		Private DiffLvl As Int16
		Private bInit As Int16
		Private MyForm As F_MAINForm
		Private KeyCode As List(Of Integer)

		Private Sub TestRun()

			Dim z As Int16

			xPanel = ColLim - 10
			xCtrl = ColLim - 10
			PRbAlt = False
			Scr = 0
			PnDir = 0
			PnDirCL = 10
			PnDirC = 0 ' StrDirCL
			CtrlCL = 5
			CtrlC = CtrlCL
			PnDirRnd = 5
			CtrlRnd = 8
			DiffC = 0
			DiffLvl = 1
			bInit = 0
			TestAborted = False
			Randomize()


			MyForm.LvMsg.Items.Clear()
			MyForm.ToolStripLbStatus.Text = "Score: 0000             Press <Esc> to Quit"

			For z = 1 To RowLim - 6
				PRbAlt = Not PRbAlt
				If Not PRbAlt Then
					MyForm.LvMsg.Items.Add(Space(ColLim - 11) & "*|       |*")
				Else
					MyForm.LvMsg.Items.Add(Space(ColLim - 11) & "*|   |   |*")
				End If
			Next

			PRbAlt = False

			MyForm.LvMsg.Items.Add("  VECTO Interactive Mode" & Space(ColLim - 35) & "*|       |*")
			MyForm.LvMsg.Items.Add(Space(ColLim - 11) & "*|   |   |*")
			MyForm.LvMsg.Items.Add(Space(ColLim - 11) & "*|       |*")
			MyForm.LvMsg.Items.Add(Space(ColLim - 11) & "*|   |   |*")
			MyForm.LvMsg.Items.Add(Space(ColLim - 11) & "*|       |*")
			MyForm.LvMsg.Items.Add(Space(ColLim - 11) & "*|   ∆   |*")

			For z = 1 To RowLim + 1
				Pnls(z) = ColLim - 10
				Ctrls(z) = 0
			Next

			MyForm.TmProgSec.Interval = 200

			MyForm.LvMsg.Focus()

			MyForm.TmProgSec.Start()
		End Sub

		Public Sub TestStop()
			MyForm.TmProgSec.Stop()
			TestActive = False
			MyForm.LvMsg.Items.Clear()
			CtrlC = 0
			MyForm.ToolStripLbStatus.Text = MyForm.LastModeName & " Mode"
		End Sub

		Public Sub TestTick()

			If bInit = 24 Then GoTo LbRace
			bInit += 1

			Select Case bInit
				Case 10
					MyForm.LvMsg.Items.RemoveAt(RowLim - 6)
					MyForm.LvMsg.Items.RemoveAt(RowLim - 5)
					MyForm.LvMsg.Items.Insert(RowLim - 6, Space(ColLim - 11) & "*|       |*")
					MyForm.LvMsg.Items.Insert(RowLim - 4, Space(ColLim - 30) & "  3      " & Space(10) & "*|       |*")
				Case 14
					MyForm.LvMsg.Items.RemoveAt(RowLim - 4)
					MyForm.LvMsg.Items.Insert(RowLim - 4, Space(ColLim - 30) & "  2      " & Space(10) & "*|       |*")
				Case 18
					MyForm.LvMsg.Items.RemoveAt(RowLim - 4)
					MyForm.LvMsg.Items.Insert(RowLim - 4, Space(ColLim - 30) & "  1      " & Space(10) & "*|       |*")
				Case 22
					MyForm.LvMsg.Items.RemoveAt(RowLim - 4)
					MyForm.LvMsg.Items.Insert(RowLim - 4, Space(ColLim - 30) & " Go!     " & Space(10) & "*|       |*")
				Case 24
					MyForm.LvMsg.Items.RemoveAt(RowLim - 4)
					MyForm.LvMsg.Items.Insert(RowLim - 4, Space(ColLim - 30) & "         " & Space(10) & "*|       |*")
			End Select
			Exit Sub
			LbRace:

			PRbAlt = Not PRbAlt

			MyForm.LvMsg.BeginUpdate()

			sLists()

			sAlign()

			sSetCtrl()

			sSetPanel()

			MyForm.LvMsg.Items.RemoveAt(RowLim)

			sUpdateCtrl()

			MyForm.LvMsg.EndUpdate()

			If Math.Abs(xCtrl - Pnls(2)) > 4 Then
				sAbort()
				Exit Sub
			ElseIf Ctrls(2) <> 0 Then
				If xCtrl = Pnls(2) + Ctrls(2) - 4 Then
					sAbort()
					Exit Sub
				End If
				Scr += 5*DiffLvl
			End If

			Scr += DiffLvl
			DiffC += 1

			'Erhöhe Schwierigkeitsgrad
			If DiffC = (DiffLvl + 3)*4 Then
				DiffC = 0
				DiffLvl += 1
				If DiffLvl > 2 And DiffLvl < 7 Then MyForm.TmProgSec.Interval = 300 - (DiffLvl)*30
				Scr += 100
				Select Case DiffLvl
					Case 3
						PnDirCL = 3
						CtrlCL = 4
						CtrlRnd = 6
					Case 5
						PnDirCL = 2
						PnDirRnd = 4
					Case 8
						CtrlCL = 2
					Case 10
						CtrlRnd = 4
						PnDirRnd = 3
				End Select
			End If
		End Sub

		Public Sub TestKey(ByVal Key0 As Integer)

			If TestActive Then
				Select Case Key0
					Case Keys.Left
						xCtrl -= 1
						sUpdateCtrl()
					Case Keys.Right
						xCtrl += 1
						sUpdateCtrl()
					Case Keys.Escape
						TestStop()
				End Select
			Else

				If KeyCode(CtrlC) = Key0 Then
					CtrlC += 1
					If CtrlC = KeyCode.Count Then
						TestActive = True
						TestRun()
					End If
				Else
					CtrlC = 0
				End If

			End If
		End Sub

		Private Sub sAbort()

			Dim s As String, s1 As String

			If TestAborted Then Exit Sub

			TestAborted = True

			MyForm.TmProgSec.Stop()

			MyForm.LvMsg.BeginUpdate()

			s = MyForm.LvMsg.Items(0).Text
			MyForm.LvMsg.Items.RemoveAt(0)
			MyForm.LvMsg.Items.Insert(0, "You crashed!" & Microsoft.VisualBasic.Right(s, Len(s) - 12))

			s = MyForm.LvMsg.Items(1).Text
			s1 = "Score: " & Scr & " "
			MyForm.LvMsg.Items.RemoveAt(1)
			MyForm.LvMsg.Items.Insert(1, s1 & Microsoft.VisualBasic.Right(s, Len(s) - Len(s1)))

			MyForm.LvMsg.EndUpdate()

			LogFile.WriteToLog(tMsgID.Normal, "*** Race Score: " & Scr.ToString("0000") & " ***")

			CtrlC = 0
			TestActive = False

			MyForm.ToolStripLbStatus.Text = MyForm.LastModeName & " Mode"
		End Sub

		Private Sub sSetCtrl()
			Dim x As Int16
			If Scr < 10 Then Exit Sub
			Ctrls(RowLim + 1) = 0
			CtrlC += 1
			If CtrlC < CtrlCL Then Exit Sub
			Select Case CInt(Int((CtrlRnd*Rnd()) + 1))
				Case 1, 2
					CtrlC = 0
					x = CInt(Int((7*Rnd()) + 1))
					Ctrls(RowLim + 1) = x
				Case Else
			End Select
		End Sub

		Private Sub sUpdateCtrl()
			Dim s As String
			If bInit < 21 Then
				xCtrl = ColLim - 10
				Exit Sub
			End If
			If Math.Abs(xCtrl - Pnls(1)) > 5 Then
				sAbort()
				Exit Sub
			End If
			s = Replace(MyForm.LvMsg.Items(RowLim - 1).Text.ToString, "∆", " ") & "   "
			s = Microsoft.VisualBasic.Left(s, ColLim + 15)
			's = s.Remove(0, 20)
			's = "Press <Esc> to Quit " & s
			If Mid(s, xCtrl + 5, 1) = "X" Then
				sAbort()
				Exit Sub
			End If
			s = s.Remove(xCtrl + 4, 1)
			's = Trim(s.Insert(xCar + 4, "∆")) & Space(ColLim + 5 - Streets(2)) & "Pts: " & Pts & " Lv: " & DiffLvl
			s = Space(Pnls(2) - 1) & Trim(s.Insert(xCtrl + 4, "∆"))
			MyForm.LvMsg.Items.RemoveAt(RowLim - 1)
			MyForm.LvMsg.Items.Insert(RowLim - 1, s)
			MyForm.ToolStripLbStatus.Text = "Score: " & Scr.ToString("0000") & "             Press <Esc> to Quit"
		End Sub

		Private Sub sSetPanel()
			Dim s As String
			s = "*|   |   |*"
			If PRbAlt Then
				s = s.Remove(5, 1)
				s = s.Insert(5, " ")
			End If
			If Ctrls(RowLim + 1) <> 0 Then
				s = s.Remove(Ctrls(RowLim + 1) + 1, 1)
				s = s.Insert(Ctrls(RowLim + 1) + 1, "X")
			End If
			Select Case xPanel - Pnls(RowLim)
				Case - 1
					s = Replace(s, "|", "\")
				Case 1
					s = Replace(s, "|", "/")
			End Select
			MyForm.LvMsg.Items.Insert(0, Space(xPanel - 1) & s)
		End Sub

		Private Sub sAlign()
			PnDirC += 1
			If PnDirC < PnDirCL Then GoTo Lb1
			PnDirC = 0
			Select Case CInt(Int((PnDirRnd*Rnd()) + 1))
				Case 1
					PnDir = 1
				Case 2
					PnDir = - 1
				Case Else
					PnDir = 0
			End Select
			Lb1:
			xPanel += PnDir
			If xPanel > ColLim Then
				xPanel = ColLim
			ElseIf xPanel < 22 Then
				xPanel = 22
			End If
			Pnls(RowLim + 1) = xPanel
		End Sub

		Private Sub sLists()
			Dim x As Int16
			For x = 2 To RowLim + 1
				Ctrls(x - 1) = Ctrls(x)
				Pnls(x - 1) = Pnls(x)
			Next
		End Sub

		Public Sub New(ByVal Form As F_MAINForm)
			MyForm = Form
			KeyCode = New List(Of Integer)
			KeyCode.Add(Keys.Up)
			KeyCode.Add(Keys.Up)
			KeyCode.Add(Keys.Down)
			KeyCode.Add(Keys.Down)
			KeyCode.Add(Keys.Left)
			KeyCode.Add(Keys.Right)
			KeyCode.Add(Keys.Left)
			KeyCode.Add(Keys.Right)
			KeyCode.Add(Keys.B)
			KeyCode.Add(Keys.A)
			CtrlC = 0
		End Sub
	End Class

	Private Sub LvMsg_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles LvMsg.KeyDown
		GUItest0.TestKey(e.KeyValue)
		If GUItest0.TestActive Then e.SuppressKeyPress = True
	End Sub

	Private Sub LvMsg_LostFocus(sender As Object, e As System.EventArgs) Handles LvMsg.LostFocus
		If GUItest0.TestActive Then GUItest0.TestStop()
	End Sub

#End Region
End Class
