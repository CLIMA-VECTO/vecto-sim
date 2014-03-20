Imports System.Collections.Generic

''' <summary>
''' Main calculation routines.
''' </summary>
''' <remarks></remarks>
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

    Private SigFile As String

    ''' <summary>
    ''' Main calculation routine. Launched by PHEMworker via Mainform's Start button or command line
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function VECTO() As tCalcResult

        Dim MsgStrBuilder As System.Text.StringBuilder

        Dim i As Integer
        Dim path0 As String
        Dim JobAbortedByErr As Boolean
        Dim CyclAbrtedByErr As Boolean
        Dim MsgOut As Boolean
        Dim MsgSrc As String
        Dim loading As tLoading
        Dim LoadList As New List(Of tLoading)



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
        VSUM = New cVSUM
        VRES = New cVRES
        If Not VSUM.Init(JobFileList(0)) Then GoTo lbErrBefore

        SigFile = Left(VSUM.VSUMfile, VSUM.VSUMfile.Length - 5) & ".vsig"

        'Warning on invalid/unrealistic settings
        If Cfg.AirDensity > 2 Then WorkerMsg(tMsgID.Err, "Air Density = " & Cfg.AirDensity & " ?!", MsgSrc)

        If Declaration.Active Then
            LoadList.Add(tLoading.EmptyLoaded)
            LoadList.Add(tLoading.RefLoaded)
            LoadList.Add(tLoading.FullLoaded)
        Else
            LoadList.Add(tLoading.UserDefLoaded)
        End If

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

            WorkerMsg(tMsgID.NewJob, "Job: " & (jgen * (CyclesDim + 1) + jzkl + 1) & " / " & ((FilesDim + 1) * (CyclesDim + 1)) & " | " & fFILE(JobFile, True), MsgSrc)
            WorkerStatus("Current Job: " & (jgen * (CyclesDim + 1) + jzkl + 1) & " / " & ((FilesDim + 1) * (CyclesDim + 1)) & " | " & fFILE(JobFile, True))
            WorkerJobStatus(jgen, "initialising... ", tJobStatus.Running)

            JobAbortedByErr = False
            MSGwarn = 0
            MSGerror = 0

            'Check if Abort
            If PHEMworker.CancellationPending Then GoTo lbAbort

            'If error when read GEN
            CurrentCycleFile = ""

            'Reading the input files
            WorkerMsg(tMsgID.Normal, "Reading input files", MsgSrc)
            If Not ReadFiles() Then
                JobAbortedByErr = True
                GoTo lbNextJob
            End If

            'WHTC Correction
            If Declaration.Active Then

                WorkerMsg(tMsgID.Normal, "WHTC Correction", MsgSrc)

                VEC.EngOnly = True

                MODdata.Init()

                If Not Declaration.WHTCinit Then
                    JobAbortedByErr = True
                    GoTo lbNextJob
                End If

                MODdata.CycleInit()

                If Not MODdata.Px.Eng_Calc() Then
                    JobAbortedByErr = True
                    GoTo lbNextJob
                End If

                MODdata.FCcalc(False)
                If MODdata.FCerror Then
                    WorkerMsg(tMsgID.Err, "WHTC FC calculcation failed!", MsgSrc)
                    JobAbortedByErr = True
                    GoTo lbNextJob
                End If

                Declaration.WHTCcorrCalc()

                VEC.EngOnly = False

            End If

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

                    'BATCH mode: Cycles from DRI list
                    CycleFiles.Clear()
                    CycleFiles.Add(fFileRepl(JobCycleList(jzkl)))

                    'Status
                    WorkerMsg(tMsgID.NewJob, "Cycle: " & (jgen * (CyclesDim + 1) + jzkl + 1) & " / " & ((FilesDim + 1) * (CyclesDim + 1)) & " | " & fFILE(JobFile, True) & " | " & fFILE(CycleFiles(0), True), MsgSrc)
                    WorkerStatus("Current Job: " & (jgen * (CyclesDim + 1) + jzkl + 1) & " / " & ((FilesDim + 1) * (CyclesDim + 1)) & " | " & fFILE(JobFile, True) & " | " & fFILE(CycleFiles(0), True))
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

                    If Declaration.Active Then
                        If Not Declaration.CalcInitCycle(jsubcycle - 1) Then
                            JobAbortedByErr = True
                            GoTo lbNextJob
                        End If
                    End If

                    If PHEMmode = tPHEMmode.ModeSTANDARD Then

                        'ProgBar
                        ProgBarCtrl.ProgOverallStartInt = 100 * (jgen) / (FilesDim + 1) + 100 * (jsubcycle - 1) / jsubcyclecount * 1 / (FilesDim + 1)
                        ProgBarCtrl.PgroOverallEndInt = 100 * (jgen) / (FilesDim + 1) + 100 * jsubcycle / jsubcyclecount * 1 / (FilesDim + 1)

                        MODdata.ModOutpName = fFileWoExt(JobFile) & "_" & fFILE(CurrentCycleFile, False)

                        WorkerMsg(tMsgID.NewJob, "Cycle: " & jsubcycle & " / " & jsubcyclecount & " | " & fFILE(CurrentCycleFile, True), MsgSrc)
                        WorkerStatus("Current Job: " & (jgen * (CyclesDim + 1) + jzkl + 1) & " / " & (FilesDim + 1) & " | " & fFILE(JobFile, True) & " | " & fFILE(CurrentCycleFile, True))

                        WorkerJobStatus(jgen, "running... " & jsubcycle & "/" & jsubcyclecount, tJobStatus.Running)

                    End If

                    VRES.ResetMe()
                    VRES.FilePath = MODdata.ModOutpName


                    '***************************** VECTO-loading-loop *********************************
                    '**************************************************************************************

                    For Each loading In LoadList

                        If Declaration.Active Then

                            'Results filename with loading
                            MODdata.ModOutpName = fFileWoExt(JobFile) & "_" & fFILE(CurrentCycleFile, False) & "_" & ConvLoading(loading)


                            WorkerMsg(tMsgID.NewJob, "Loading: " & ConvLoading(loading), MsgSrc)

                            If Not Declaration.CalcInitLoad(loading) Then
                                JobAbortedByErr = True
                                GoTo lbNextJob
                            End If

                        End If

                        'Clean up
                        MODdata.Init()

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


                        '----------------------------------------------------------------------------
                        '----------------------------------------------------------------------------

                        'Initialize Cycle-specs (Speed, Accel, ...)
                        MODdata.CycleInit()

                        If VEC.EngOnly Then

                            If MsgOut Then WorkerMsg(tMsgID.Normal, "Engine Only Calc", MsgSrc)

                            'Rechne .npi-Leistung in Pe und P_clutch um |@@| Expect Npi-Power into Pe and P_clutch
                            If Not MODdata.Px.Eng_Calc() Then
                                CyclAbrtedByErr = True
                                GoTo lbAusg
                            End If

                        Else

                            'Init auxiliaries
                            If Not VEC.AuxInit() Then
                                'Error-notification within AuxInit()
                                JobAbortedByErr = True
                                GoTo lbNextJob
                            End If

                            'CAUTION: VehmodeInit() requires information from VECTO and DRI!
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


                            If PHEMworker.CancellationPending Then GoTo lbAbort

                            'Calculate CycleKin (for erg/sum, etc.)
                            MODdata.CylceKin.Calc()

                        End If
                        '----------------------------------------------------------------------------
                        '----------------------------------------------------------------------------


                        If MsgOut Then WorkerMsg(tMsgID.Normal, "FC Interpolation", MsgSrc)

                        'Calculate FC
                        MODdata.FCcalc(True)

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
                        If Not VEC.EngOnly Then
                            If Not VRES.SetVals(loading) Then
                                CyclAbrtedByErr = True
                                GoTo lbAusg
                            End If
                        End If

                        'Output for BATCH and ADVANCE
lbAusg:

                        If PHEMworker.CancellationPending Then GoTo lbAbort

                        'VSUM Output (first Calculation - Initialization & Header)
                        If Not VSUM.AusgVSUM(jgen * (CyclesDim + 1) + jzkl + 1, fFILE(GenFile, True), fFILE(CurrentCycleFile, True), CyclAbrtedByErr) Then GoTo lbErrInJobLoop

                        'Data Cleanup
                        MODdata.CleanUp()

                        'Status-Update
                        If PHEMmode = tPHEMmode.ModeSTANDARD Then
                            WorkerProg(100 * (jgen) / (FilesDim + 1) + 100 * jsubcycle / jsubcyclecount * 1 / (FilesDim + 1), 0)
                        ElseIf PHEMmode = tPHEMmode.ModeBATCH Then
                            WorkerProg(100 * (jgen * (CyclesDim + 1) + jzkl + 1) / ((FilesDim + 1) * (CyclesDim + 1)), 0)
                        End If

                    Next

                    '******************** END *** VECTO-loading loop *** END ************************
                    '**************************************************************************************

                    If Not VEC.EngOnly Then
                        If Not VRES.Output() Then
                            CyclAbrtedByErr = True
                            GoTo lbNextJob
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
                        VSUM.AusgVSUM(((jgen + 1) * (CyclesDim + 1)).ToString, fFILE(GenFile, True), "-", True)
                    Else
                        VSUM.AusgVSUM((jgen * (CyclesDim + 1) + 1).ToString & ".." & ((jgen + 1) * (CyclesDim + 1)).ToString, fFILE(GenFile, True), "-", True)
                    End If
                End If

                WorkerJobStatus(jgen, "Aborted due to error!", tJobStatus.Err)

            Else

                MsgStrBuilder.Length = 0
                MsgStrBuilder.Append("done")
                'If GEN.irechwahl = tCalcMode.cmHEV Then MsgStrBuilder.Append(" (dSOC = " & SOC(MODdata.tDim) - SOC(0) & ")")

                'Add input file list to signature list
                If VEC.CreateFileList Then
                    For i = 0 To VEC.FileList.Count - 1
                        Lic.FileSigning.AddFile(VEC.FileList(i))
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

        WorkerMsg(tMsgID.Normal, "Summary Results written to: " & fFILE(VSUM.VSUMfile, True), MsgSrc, VSUM.VSUMfile)

        'JSON Erg Output
        If VSUM.WriteJSON() Then
            WorkerMsg(tMsgID.Normal, "Summary Results (JSON) written to: " & fFILE(VSUM.VSUMfile & ".json", True), MsgSrc, VSUM.VSUMfile & ".json")
        Else
            WorkerMsg(tMsgID.Err, "Failed to write JSON Summary Results!", MsgSrc)
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
        VEC = Nothing
        VEH = Nothing
        FLD = Nothing
        MAP = Nothing
        DRI = Nothing
        MODdata = Nothing
        VSUM = Nothing

        ENG = Nothing
        GBX = Nothing

    End Function

    Public Function ReadFiles() As Boolean
        Dim i As Integer
        Dim sb As cSubPath


        Dim MsgSrc As String

        MsgSrc = "Main/ReadInp"

        '-----------------------------    ~GEN~    -----------------------------
        'Read GEN
        If UCase(fEXT(GenFile)) <> ".VECTO" Then
            WorkerMsg(tMsgID.Err, "Only .VECTO files are supported in this mode", MsgSrc)
            Return False
        End If

        VEC = New cVECTO
        VEC.FilePath = GenFile

        Try
            If Not VEC.ReadFile() Then
                WorkerMsg(tMsgID.Err, "Cannot read .gen file (" & GenFile & ")", MsgSrc)
                Return False
            End If
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "File read error! (" & GenFile & ")", MsgSrc, GenFile)
            Return False
        End Try

        If VEC.NoJSON Then WorkerMsg(tMsgID.Warn, "VECTO file format is outdated! CLICK HERE to convert to current format!", MsgSrc, "<GUI>" & GenFile)


        '-----------------------------    ~VEH~    -----------------------------
        VEH = New cVEH

        'Read vehicle specifications
        If Not VEC.EngOnly Then
            VEH.FilePath = VEC.PathVEH
            Try
                If Not VEH.ReadFile Then Return False
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & VEC.PathVEH & ")", MsgSrc, VEC.PathVEH)
                Return False
            End Try

        End If

        If VEH.NoJSON Then WorkerMsg(tMsgID.Warn, "Vehicle file format is outdated! CLICK HERE to convert to current format!", MsgSrc, "<GUI>" & VEC.PathVEH)

        If Declaration.Active Then
            If Not Declaration.SetRef() Then
                WorkerMsg(tMsgID.Err, "Vehicle Configuration not found in Segment Table!", MsgSrc)
                Return False
            End If
        End If


        If Declaration.Active Then VEC.DeclInit()


        CycleFiles.Clear()
        For Each sb In VEC.CycleFiles
            CycleFiles.Add(sb.FullPath)
        Next

        'Error message in init()
        If Not VEC.Init Then Return False



        '-----------------------------    ~ENG~    -----------------------------
        ENG = New cENG
        ENG.FilePath = VEC.PathENG
        Try
            If Not ENG.ReadFile Then Return False
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "File read error! (" & VEC.PathENG & ")", MsgSrc, VEC.PathENG)
            Return False
        End Try

        If ENG.NoJSON Then WorkerMsg(tMsgID.Warn, "Engine file format is outdated! CLICK HERE to convert to current format!", MsgSrc, "<GUI>" & VEC.PathENG)

        If Declaration.Active Then ENG.DeclInit()


        '-----------------------------    ~GBX~    -----------------------------
        GBX = New cGBX

        If Not VEC.EngOnly Then
            GBX.FilePath = VEC.PathGBX
            Try
                If Not GBX.ReadFile Then Return False
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & VEC.PathGBX & ")", MsgSrc, VEC.PathGBX)
                Return False
            End Try
        End If

        If GBX.NoJSON Then WorkerMsg(tMsgID.Warn, "Gearbox file format is outdated! CLICK HERE to convert to current format!", MsgSrc, "<GUI>" & VEC.PathGBX)



        '-----------------------------    VECTO    -----------------------------

        'GBX => VEH
        For i = 0 To GBX.GetrI.Count - 1
            VEH.siGetrI.Add(GBX.GetrI(i))
            VEH.GetrMap.Add(New cSubPath)
            If IsNumeric(GBX.GetrMap(i, True)) Then
                VEH.GetrEffDef.Add(True)
                VEH.GetrEff.Add(CSng(GBX.GetrMap(i, True)))
            Else
                VEH.GetrEffDef.Add(False)
                VEH.GetrEff.Add(0)
                VEH.GetrMap(i).Init(fPATH(VEC.PathGBX), GBX.GetrMap(i, True))
            End If
        Next

        ENG.Init()


        '-----------------------------    ~FLD~    -----------------------------
        'Now in cENG.Init


        '-----------------------------    ~MAP~    -----------------------------
        'Now in cENG.Init

        'Must be after ENG.Init()
        If Declaration.Active Then GBX.DeclInit()



        Return True

    End Function


    '---------------------------------------------------------------------------


End Module
