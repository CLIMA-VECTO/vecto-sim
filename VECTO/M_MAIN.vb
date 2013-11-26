Imports System.Collections.Generic

Module M_MAIN

    Public PHEMmode As tPHEMmode
    Public JobFileList As List(Of String)
    Public JobCycleList As List(Of String)

    Public JobFile As String
    Public GenFile As String
    Public CycleFiles As New List(Of String)
    Public CurrentCycleFile As String

    Private jgen As Integer
    Private jzkl As Integer
    Private CyclesDim As Integer
    Private FilesDim As Integer
    Private jsubcycle As Integer
    Private jsubcyclecount As Integer

    Public FCerror As Boolean

    Private SigFile As String

    Friend Function NrOfRunStr() As String
        If PHEMmode = tPHEMmode.ModeSTANDARD Then
            'Return CStr(jgen * (CyclesDim + 1) + jzkl + 1) & "-" & CStr(jsubcycle)
            Return CStr(jgen * (CyclesDim + 1) + jzkl + 1) & "(" & CStr(jsubcycle) & ")"
        Else
            Return CStr(jgen * (CyclesDim + 1) + jzkl + 1)
        End If
    End Function

    Public Function VECTO() As tCalcResult

        'Main program for all modes

        Dim MsgStrBuilder As System.Text.StringBuilder

        Dim i As Integer
        Dim path0 As String
        Dim JobAbortedByErr As Boolean
        Dim CyclAbrtedByErr As Boolean
        Dim MsgOut As Boolean
        Dim MsgSrc As String


        MsgSrc = "Main"

        MsgStrBuilder = New System.Text.StringBuilder

        'If there are any "unplanned" Aborts
        VECTO = tCalcResult.Err

        'Reset the fault
        ''ClearErrors()

        'Specify Mode and Notification-msg
        Select Case PHEMmode
            Case tPHEMmode.ModeSTANDARD
                WorkerMsg(tMsgID.Normal, "Starting VECTO STANDARD...", MsgSrc)
                CyclesDim = 0
            Case tPHEMmode.ModeBATCH
                WorkerMsg(tMsgID.Normal, "Starting VECTO BATCH...", MsgSrc)
                CyclesDim = JobCycleList.Count - 1
        End Select
        FilesDim = JobFileList.Count - 1

        MsgOut = (PHEMmode = tPHEMmode.ModeSTANDARD)

        'License check

        If (PHEMmode = tPHEMmode.ModeBATCH) Then
            If Not Lic.LicFeature(1) Then
                WorkerMsg(tMsgID.Err, "Your license does not support BATCH mode!", MsgSrc)
                GoTo lbErrBefore
            End If
        End If

        If FilesDim = -1 Then
            WorkerMsg(tMsgID.Err, "No Job Files defined.", MsgSrc)
            GoTo lbErrBefore
        End If

        If CyclesDim = -1 And (PHEMmode = tPHEMmode.ModeBATCH) Then
            WorkerMsg(tMsgID.Err, "No Driving Cycles defined.", MsgSrc)
            GoTo lbErrBefore
        End If

        'Create BATCH Output-folder if necessary
        If (PHEMmode = tPHEMmode.ModeBATCH) Then
            Select Case UCase(Cfg.BATCHoutpath)
                Case sKey.WorkDir
                    path0 = Cfg.WorkDPath
                Case sKey.GenPath
                    GoTo lbSkip0
                Case Else
                    path0 = Cfg.BATCHoutpath
            End Select
            If Not IO.Directory.Exists(path0) Then
                Try
                    IO.Directory.CreateDirectory(path0)
                Catch ex As Exception
                    WorkerMsg(tMsgID.Err, "Failed to create output directory " & path0 & " !", MsgSrc)
                    GoTo lbErrBefore
                End Try
            End If
        End If
lbSkip0:

        'MOD-Data class initialization
        MODdata = New cMOD

        'New signature file
        Lic.FileSigning.NewFile()

        'ERG-class initialization
        WorkerMsg(tMsgID.Normal, "Analyzing input files", MsgSrc)
        ERG = New cERG
        VSUM = New cVSUM
        If Not ERG.Init(JobFileList(0)) Then GoTo lbErrBefore

        SigFile = Left(ERG.ErgFile, ERG.ErgFile.Length - 5) & ".vsig"

        'Warning on invalid/unrealistic settings
        If Cfg.AirDensity > 2 Then WorkerMsg(tMsgID.Err, "Air Density = " & Cfg.AirDensity & " ?!", MsgSrc)

        'Notify
        If Cfg.FCcorrection Then WorkerMsg(tMsgID.Normal, "HDV FC Correction ON", MsgSrc)

        'Progbar-Init
        WorkerProgInit()

        '--------------------------------------------------------------------------------------------
        '       Calculation Loop for all Preset-cycles and Vehicles:
        '
        '**********************************************************************************************
        '**************************************** Job loop ****************************************
        '**********************************************************************************************
        For jgen = 0 To FilesDim

            jzkl = 0    '<= Damit NrOfRun stimmt

            JobFile = fFileRepl(JobFileList(jgen))

            GenFile = JobFile

            WorkerJobStatus(jgen, "running...", tJobStatus.Running)

            JobAbortedByErr = False
            MSGwarn = 0
            MSGerror = 0

            'Check if Abort
            If PHEMworker.CancellationPending Then GoTo lbAbort

            'If error when read GEN
            CurrentCycleFile = ""

            'Reading the input files
            '   BATCH: Cycle from DRI list
            '   ADVANCE: Cycle is not read
            If Not LESE() Then
                JobAbortedByErr = True
                GoTo lbNextJob
            End If

            'If optimizer is active, then read parameters here
            If bOptOn Then OptInput()

            'BATCH: Create Output-sub-folder
            If (PHEMmode = tPHEMmode.ModeBATCH) And Cfg.ModOut And Cfg.BATCHoutSubD Then
                Select Case UCase(Cfg.BATCHoutpath)
                    Case sKey.WorkDir
                        path0 = Cfg.WorkDPath
                    Case sKey.GenPath
                        path0 = fPATH(JobFile)
                    Case Else
                        path0 = Cfg.BATCHoutpath
                End Select
                path0 &= fFILE(JobFile, False) & "\"
                If Not IO.Directory.Exists(path0) Then
                    Try
                        IO.Directory.CreateDirectory(path0)
                    Catch ex As Exception
                        WorkerMsg(tMsgID.Err, "Failed to create output directory " & path0 & " !", MsgSrc)
                        JobAbortedByErr = True
                        GoTo lbNextJob
                    End Try
                End If
            End If

            '**********************************************************************************************
            '************************************** Cycle-loop ****************************************
            '**********************************************************************************************
            For jzkl = 0 To CyclesDim


                CyclAbrtedByErr = False

                If PHEMmode = tPHEMmode.ModeBATCH Then

                    'ProgBar
                    ProgBarCtrl.ProgOverallStartInt = 100 * (jgen * (CyclesDim + 1) + jzkl) / ((FilesDim + 1) * (CyclesDim + 1))
                    ProgBarCtrl.PgroOverallEndInt = 100 * (jgen * (CyclesDim + 1) + jzkl + 1) / ((FilesDim + 1) * (CyclesDim + 1))

                    'BATCH mode: Cycle from GEN-file but not from DRI list
                    CycleFiles.Clear()
                    CycleFiles.Add(fFileRepl(JobCycleList(jzkl)))

                    'Status
                    WorkerMsg(tMsgID.NewJob, "Job: " & (NrOfRunStr()) & " / " & ((FilesDim + 1) * (CyclesDim + 1)) & " | " & fFILE(JobFile, True) & " | " & fFILE(CycleFiles(0), True), MsgSrc)
                    WorkerStatus("Current Job: " & (NrOfRunStr()) & " / " & ((FilesDim + 1) * (CyclesDim + 1)) & " | " & fFILE(JobFile, True) & " | " & fFILE(CycleFiles(0), True))
                    WorkerJobStatus(jgen, "running... " & jzkl + 1 & "/" & (CyclesDim + 1), tJobStatus.Running)

                    'Output name definition
                    Select Case UCase(Cfg.BATCHoutpath)
                        Case sKey.WorkDir
                            path0 = Cfg.WorkDPath
                        Case sKey.GenPath
                            path0 = fPATH(JobFile)
                        Case Else
                            path0 = Cfg.BATCHoutpath
                    End Select

                    If Cfg.BATCHoutSubD Then
                        MODdata.ModOutpName = path0 & fFILE(JobFile, False) & "\" & fFILE(JobFile, False) & "_" & fFILE(CycleFiles(0), False)
                    Else
                        MODdata.ModOutpName = path0 & fFILE(JobFile, False) & "_" & fFILE(CycleFiles(0), False)
                    End If

                End If

                '******************************************************************************************
                '********************************** VECTO-Cycle-loop **********************************
                jsubcyclecount = CycleFiles.Count

                If jsubcyclecount = 0 Then
                    WorkerMsg(tMsgID.Err, "No driving cycle defined!", MsgSrc)
                    JobAbortedByErr = True
                    GoTo lbNextJob
                End If

                jsubcycle = 0
                For Each CurrentCycleFile In CycleFiles
                    jsubcycle += 1

                    ProgBarCtrl.ProgJobInt = 0

                    If PHEMmode = tPHEMmode.ModeSTANDARD Then

                        'ProgBar
                        ProgBarCtrl.ProgOverallStartInt = 100 * (jgen) / (FilesDim + 1) + 100 * (jsubcycle - 1) / jsubcyclecount * 1 / (FilesDim + 1)
                        ProgBarCtrl.PgroOverallEndInt = 100 * (jgen) / (FilesDim + 1) + 100 * jsubcycle / jsubcyclecount * 1 / (FilesDim + 1)

                        MODdata.ModOutpName = fFileWoExt(JobFile) & "_" & fFILE(CurrentCycleFile, False)

                        WorkerMsg(tMsgID.NewJob, "Job: " & NrOfRunStr() & " / " & ((FilesDim + 1) * (CyclesDim + 1)) & " | " & fFILE(JobFile, True) & " | " & fFILE(CurrentCycleFile, True), MsgSrc)
                        WorkerStatus("Current Job: " & NrOfRunStr() & " / " & ((FilesDim + 1) * (CyclesDim + 1)) & " | " & fFILE(JobFile, True) & " | " & fFILE(CurrentCycleFile, True))

                        WorkerJobStatus(jgen, "running... " & jsubcycle & "/" & jsubcyclecount, tJobStatus.Running)

                    End If

                    VSUM.ResetMe()
                    VSUM.FilePath = MODdata.ModOutpName & ".vres"


                    'TODO: Loading-loop
                    '***************************** VECTO-loading-loop *********************************
                    '**************************************************************************************

                    'Entry point for SOC-start iteration
                    If GEN.ModeHorEV And SOCnJa Then SOCfirst = True

                    'Clean up
                    MODdata.Init()

                    FCerror = False

                    'Read cycle
                    DRI = New cDRI
                    DRI.FilePath = CurrentCycleFile

                    If Not DRI.ReadFile Then
                        CyclAbrtedByErr = True
                        GoTo lbAusg
                    End If

                    'Grad to Alt
                    DRI.GradToAlt()

                    'Convert v(s) into v(t) (optional)
                    If DRI.Scycle Then

                        MODdata.Vh.SetAlt()


                        If MsgOut Then WorkerMsg(tMsgID.Normal, "Converting cycle (v(s) => v(t))", MsgSrc)

                        If Not DRI.ConvStoT() Then
                            CyclAbrtedByErr = True
                            GoTo lbAusg
                        End If
                    End If

                    'If first time step is Zero then duplicate first values to start cycle with vehicle standing.
                    If DRI.Vvorg AndAlso DRI.tDim > 1 AndAlso DRI.Values(tDriComp.V)(0) < 0.0001 AndAlso DRI.Values(tDriComp.V)(1) >= 0.0001 Then
                        DRI.FirstZero()
                    End If

                    'Convert to 1Hz (optional) - does not apply to v(s) cycles because timestep is missing
                    If DRI.Tvorg Then
                        If MsgOut Then WorkerMsg(tMsgID.Normal, "Converting cycle to 1Hz", MsgSrc)
                        If Not DRI.ConvTo1Hz() Then
                            'Error-notification in DRI.Convert()
                            CyclAbrtedByErr = True
                            GoTo lbAusg
                        End If
                    End If

                    'De-normalize
                    DRI.DeNorm()



                    '----------------------------------------------------------------------------
                    '----------------------------------------------------------------------------

                    'Initialize Cycle-specs (Speed, Accel, ...)
                    MODdata.CycleInit()

                    If GEN.VehMode = tVehMode.EngineOnly Then

                        If MsgOut Then WorkerMsg(tMsgID.Normal, "Engine Only Calc", MsgSrc)

                        'Rechne .npi-Leistung in Pe und P_clutch um |@@| Expect Npi-Power into Pe and P_clutch
                        If Not MODdata.Px.Eng_Calc() Then
                            CyclAbrtedByErr = True
                            GoTo lbAusg
                        End If

                    Else

                        'CAUTION: VehmodeInit() requires information from GEN and DRI!
                        If Not VEH.VehmodeInit() Then
                            'Error-notification within VehmodeInit()
                            JobAbortedByErr = True
                            GoTo lbNextJob
                        End If

                        If Not GBX.GSinit Then
                            'Error-notification within GSinit()
                            JobAbortedByErr = True
                            GoTo lbNextJob
                        End If

                        If GBX.TCon Then
                            If Not GBX.TCinit Then
                                'Error-notification within TCinit()
                                JobAbortedByErr = True
                                GoTo lbNextJob
                            End If
                        End If

                        If GEN.ModeHorEV Then

                            WorkerMsg(tMsgID.Err, "(H)EV mode is not available!", MsgSrc)
                            JobAbortedByErr = True
                            GoTo lbNextJob

                        Else

                            If DEV.PreRun Then

                                If MsgOut Then WorkerMsg(tMsgID.Normal, "Driving Cycle Preprocessing", MsgSrc)
                                If Not MODdata.Px.PreRun Then
                                    CyclAbrtedByErr = True
                                    GoTo lbAusg
                                End If

                                If PHEMworker.CancellationPending Then GoTo lbAbort

                            End If

                            If MsgOut Then WorkerMsg(tMsgID.Normal, "Vehicle Calc", MsgSrc)

                            MODdata.Vh.DistCorrInit()

                            If Not MODdata.Px.Calc() Then
                                CyclAbrtedByErr = True
                                GoTo lbAusg
                            End If

                        End If

                        If PHEMworker.CancellationPending Then GoTo lbAbort

                        'Calculate CycleKin (for erg/sum, etc.)
                        MODdata.CylceKin.Calc()

                    End If
                    '----------------------------------------------------------------------------
                    '----------------------------------------------------------------------------


                    'Emissionen und Nachbehandlung - wird bei EV-Modus nicht ausgeführt |@@| Emissions and After-treatment - it will not run in EV mode
                    If Not GEN.VehMode = tVehMode.EV Then

                        'If MsgOut Then WorkerMsg(tMsgID.Normal, "Calculating Transient Correction Factors", MsgSrc)

                        ''Determine TC parameters per second
                        'MODdata.TC.Calc()

                        'Map creation
                        If GEN.CreateMap Then

                            If MsgOut Then WorkerMsg(tMsgID.Normal, "Creating Emission Map", MsgSrc)

                            MAP = New cMAP(GEN.PKWja)   'PKWja ist hier nicht relevant
                            MAP.FilePath = fFileWoExt(JobFile) & ".v_map"
                            If Not MAP.CreateMAP() Then
                                CyclAbrtedByErr = True
                                GoTo lbAusg
                            End If
                            MAP.Norm()
                        End If

                        If MsgOut Then WorkerMsg(tMsgID.Normal, "FC Interpolation", MsgSrc)

                        'Calculate Raw emissions
                        If Not MODdata.Em.Raw_Calc() Then
                            'If Not DEV.IgnoreFCextrapol Then
                            '    CyclAbrtedByErr = True
                            '    WorkerMsg(tMsgID.Normal, "Calculation aborted!", MsgSrc)
                            '    GoTo lbAusg
                            'End If

                            FCerror = True

                        End If

                        'TC Parameter umrechnen in Differenz zu Kennfeld-TC-Parameter |@@| Convert TC parameters to differences with Map-TC-parameters
                        If MAP.TransMap Then MODdata.TC.CalcDiff()

                        'Dynamic correction
                        If GEN.dynkorja Then
                            If MsgOut Then WorkerMsg(tMsgID.Normal, "Em Calc: Transient Correction", MsgSrc)
                            MODdata.Em.TC_Calc()
                        End If

                        'Korrektur der Verbrauchswerte kleiner LKW-Motoren bei HBEFA |@@| Correction of consumption values smaller HDV(LKW) engines by HBEFA
                        If (Not GEN.PKWja) And Cfg.FCcorrection Then
                            If MsgOut Then WorkerMsg(tMsgID.Normal, "Em Calc: FC-Correction", MsgSrc)
                            FcCorr()
                        End If

                        'Exhaust system simulation
                        If GEN.EXSja Then
                            If MsgOut Then WorkerMsg(tMsgID.Normal, "Em Calc: EXS", MsgSrc)
                            EXS = New cEXS
                            If Not EXS.Exs_Main() Then
                                CyclAbrtedByErr = True
                                GoTo lbAusg
                            End If
                        End If

                        'Totals / Averages form
                        MODdata.Em.SumCalc()

                        'Engine Analysis
                        If GEN.EngAnalysis Then
                            If MsgOut Then WorkerMsg(tMsgID.Normal, "Engine Analysis", MsgSrc)
                            If Not MODdata.Em.EngAnalysis() Then
                                CyclAbrtedByErr = True
                                GoTo lbAusg
                            End If
                        End If

                    End If

                    If PHEMworker.CancellationPending Then GoTo lbAbort

                    '*** second-by-second output ***
                    If Cfg.ModOut Then
                        If MsgOut Then WorkerMsg(tMsgID.Normal, "Writing modal output", MsgSrc)
                        If Not MODdata.Output() Then
                            CyclAbrtedByErr = True
                            GoTo lbAusg
                        End If

                        WorkerMsg(tMsgID.Normal, "Modal Results written to: " & fFILE(MODdata.ModOutpName & ".vmod", True), MsgSrc, MODdata.ModOutpName & ".vmod")

                    End If

                    'VECTO Output
                    'TODO: Loadings...
                    If Not GEN.EngOnly Then
                        If Not VSUM.SetVals(tVSUM.UserDefLoaded) Then
                            CyclAbrtedByErr = True
                            GoTo lbAusg
                        End If
                    End If

                    'Output for BATCH and ADVANCE
lbAusg:

                    If PHEMworker.CancellationPending Then GoTo lbAbort
             
                    'Output in Erg (first Calculation - Initialization & Header)
                    If Not ERG.AusgERG(NrOfRunStr, fFILE(GenFile, True), fFILE(CurrentCycleFile, True), CyclAbrtedByErr) Then GoTo lbErrInJobLoop

                    'Data Cleanup
                    MODdata.CleanUp()

                    'Status-Update
                    If PHEMmode = tPHEMmode.ModeSTANDARD Then
                        WorkerProg(100 * (jgen) / (FilesDim + 1) + 100 * jsubcycle / jsubcyclecount * 1 / (FilesDim + 1), 0)
                    ElseIf PHEMmode = tPHEMmode.ModeBATCH Then
                        WorkerProg(100 * (jgen * (CyclesDim + 1) + jzkl + 1) / ((FilesDim + 1) * (CyclesDim + 1)), 0)
                    End If

                    'TODO: Loading Loop
                    '******************** END *** VECTO-loading loop *** END ************************
                    '**************************************************************************************

                    If Not GEN.EngOnly Then
                        If Not VSUM.Output() Then
                            CyclAbrtedByErr = True
                            GoTo lbAusg
                        End If
                    End If

                Next

                '************************* END *** VECTO Cycle-loop *** END *************************
                '******************************************************************************************



            Next
            '**********************************************************************************************
            '****************************** END *** Cycle-loop *** END ******************************
            '**********************************************************************************************
lbNextJob:

            If JobAbortedByErr Or (CyclAbrtedByErr And CyclesDim = 0) Then

                If JobAbortedByErr Then
                    If CInt(jgen * (CyclesDim + 1) + 1) = CInt((jgen + 1) * (CyclesDim + 1)) Then
                        ERG.AusgERG(((jgen + 1) * (CyclesDim + 1)).ToString, fFILE(GenFile, True), "-", True)
                    Else
                        ERG.AusgERG((jgen * (CyclesDim + 1) + 1).ToString & ".." & ((jgen + 1) * (CyclesDim + 1)).ToString, fFILE(GenFile, True), "-", True)
                    End If
                End If

                WorkerJobStatus(jgen, "Aborted due to error!", tJobStatus.Err)

            Else

                MsgStrBuilder.Length = 0
                MsgStrBuilder.Append("done")
                'If GEN.irechwahl = tCalcMode.cmHEV Then MsgStrBuilder.Append(" (dSOC = " & SOC(MODdata.tDim) - SOC(0) & ")")

                'Add input file list to signature list
                If GEN.CreateFileList Then
                    For i = 0 To GEN.FileList.Count - 1
                        Lic.FileSigning.AddFile(GEN.FileList(i))
                    Next
                Else
                    WorkerMsg(tMsgID.Err, "Could not create file list for signing!", MsgSrc)
                End If

                If MSGwarn > 0 Then
                    MsgStrBuilder.Append(". " & MSGwarn & " Warning")
                    If MSGwarn > 1 Then MsgStrBuilder.Append("s")
                End If

                If MSGerror > 0 Then
                    MsgStrBuilder.Append(". " & MSGerror & " Error")
                    If MSGerror > 1 Then MsgStrBuilder.Append("s")
                End If

                If MSGerror > 0 Then
                    WorkerJobStatus(jgen, MsgStrBuilder.ToString & ".", tJobStatus.Warn)
                Else
                    WorkerJobStatus(jgen, MsgStrBuilder.ToString & ".", tJobStatus.OK)
                End If

            End If

            'Check whether Abort
            If PHEMworker.CancellationPending Then GoTo lbAbort

        Next

        '**********************************************************************************************
        '******************************* END *** Job loop *** END *******************************
        '**********************************************************************************************

        WorkerMsg(tMsgID.Normal, "Summary Results written to: " & fFILE(ERG.ErgFile, True), MsgSrc, ERG.ErgFile)

        'JSON Erg Output
        If Cfg.JSON Then
            If ERG.WriteJSON() Then
                WorkerMsg(tMsgID.Normal, "Summary Results (JSON) written to: " & fFILE(ERG.ErgFile & ".json", True), MsgSrc, ERG.ErgFile & ".json")
            Else
                WorkerMsg(tMsgID.Err, "Failed to write JSON Summary Results!", MsgSrc)
            End If
        End If

        'Write file signatures
        WorkerMsg(tMsgID.Normal, "Signing files", MsgSrc)
        Lic.FileSigning.Mode = vectolic.cFileSigning.tMode.Auto

        If Lic.FileSigning.WriteSigFile(SigFile, LicSigAppCode) Then
            WorkerMsg(tMsgID.Normal, "Files signed successfully: " & fFILE(SigFile, True), MsgSrc, SigFile)
        Else
            WorkerMsg(tMsgID.Err, "Failed to sign files! " & Lic.FileSigning.ErrorMsg, MsgSrc)
        End If

        WorkerMsg(tMsgID.Normal, "done", MsgSrc)
        VECTO = tCalcResult.Done
        GoTo lbExit


lbErrBefore:  '!!!!!!!!!! Abbruch bevor (!!!) der erste Job angefangen wurde !!!!!!!!!!!
        WorkerMsg(tMsgID.Normal, "aborted", MsgSrc)
        VECTO = tCalcResult.Err

        For i = 0 To FilesDim
            WorkerJobStatus(i, "", tJobStatus.Undef)
        Next

        GoTo lbExit


lbErrInJobLoop:
        WorkerMsg(tMsgID.Normal, "aborted", MsgSrc)
        WorkerJobStatus(jgen, "aborted", tJobStatus.Err)
        VECTO = tCalcResult.Err

        For i = jgen + 1 To FilesDim
            WorkerJobStatus(i, "", tJobStatus.Undef)
        Next

        MODdata.CleanUp()

        GoTo lbExit

lbAbort:
        WorkerMsg(tMsgID.Normal, "aborted", MsgSrc)
        WorkerJobStatus(jgen, "aborted", tJobStatus.Warn)
        VECTO = tCalcResult.Abort

        For i = jgen + 1 To FilesDim
            WorkerJobStatus(i, "", tJobStatus.Undef)
        Next

        MODdata.CleanUp()

lbExit:
        GEN = Nothing
        VEH = Nothing
        FLD = Nothing
        MAP = Nothing
        TRS = Nothing
        DRI = Nothing
        MODdata = Nothing
        ERG = Nothing

        ENG = Nothing
        GBX = Nothing

    End Function

    '---------------------------------------------------------------------------


End Module
