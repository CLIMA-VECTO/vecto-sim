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

    Friend Function NrOfRunStr() As String
        If PHEMmode = tPHEMmode.ModeSTANDARD Then
            Return CStr(jgen * (CyclesDim + 1) + jzkl + 1) & "-" & CStr(jsubcycle)
        Else
            Return CStr(jgen * (CyclesDim + 1) + jzkl + 1)
        End If
    End Function

    Public Function PHEM() As tCalcResult

        'Hauptprogramm für alle Modi

        Dim MsgStrBuilder As System.Text.StringBuilder

        Dim i As Integer
        Dim path0 As String
        Dim LicErrorFeat As Short
        Dim JobAbortedByErr As Boolean
        Dim CyclAbrtedByErr As Boolean
        Dim MsgOut As Boolean
        Dim MsgSrc As String


        MsgSrc = "Main"

        MsgStrBuilder = New System.Text.StringBuilder

        'Falls es zu "ungeplanten" Abbruch kommt
        PHEM = tCalcResult.Err

        'Fehler zurücksetzen
        ''ClearErrors()

        'Modus festlegen und Meldung
        Select Case PHEMmode
            Case tPHEMmode.ModeSTANDARD
                WorkerMsg(tMsgID.Normal, "Starting PHEM STANDARD...", MsgSrc)
                CyclesDim = 0
            Case tPHEMmode.ModeBATCH
                WorkerMsg(tMsgID.Normal, "Starting PHEM BATCH...", MsgSrc)
                CyclesDim = JobCycleList.Count - 1
            Case tPHEMmode.ModeADVANCE
                WorkerMsg(tMsgID.Normal, "Starting PHEM ADVANCE...", MsgSrc)
                CyclesDim = 0
        End Select
        FilesDim = JobFileList.Count - 1

        MsgOut = (PHEMmode = tPHEMmode.ModeSTANDARD)

        'Lizenz checken
        LicErrorFeat = 0
        If (PHEMmode = tPHEMmode.ModeBATCH) Then
            If Not Lic.LicFeature(1) Then LicErrorFeat = 1
        ElseIf (PHEMmode = tPHEMmode.ModeADVANCE) Then
            If Not Lic.LicFeature(2) Then LicErrorFeat = 2
        End If

        If LicErrorFeat > 0 Then
            WorkerMsg(tMsgID.Err, "Your license does not support the selected mode (" & LicErrorFeat & ")", MsgSrc)
            GoTo lbErrBefore
        End If

        If FilesDim = -1 Then
            WorkerMsg(tMsgID.Err, "No Job Files defined.", MsgSrc)
            GoTo lbErrBefore
        End If

        If CyclesDim = -1 And (PHEMmode = tPHEMmode.ModeBATCH) Then
            WorkerMsg(tMsgID.Err, "No Driving Cycles defined.", MsgSrc)
            GoTo lbErrBefore
        End If

        'BATCH-Ausgabeordner erstellen falls notwendig
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

        'MOD-Daten Klasse initialisieren
        MODdata = New cMOD
        'TEST: Schaun ob's auch ohne dem geht: MODdata.Init()

        'ERG-Klasse initialisieren
        WorkerMsg(tMsgID.Normal, "Analyzing input files", MsgSrc)
        ERG = New cERG
        VSUM = New cVSUM
        If Not ERG.Init(JobFileList(0)) Then GoTo lbErrBefore

        If Not PHEMmode = tPHEMmode.ModeADVANCE Then
            If Not ERG.ErgEntryInit() Then GoTo lbErrBefore
        End If

        'Warnung wenn ungültige/unrealistische Einstellungen
        If Cfg.AirDensity > 2 Then WorkerMsg(tMsgID.Err, "Air Density = " & Cfg.AirDensity & " ?!", MsgSrc)

        'Meldungen
        If Cfg.FCcorrection Then WorkerMsg(tMsgID.Normal, "HDV FC Correction ON", MsgSrc)

        'Progbar-Init
        WorkerProgInit()


        '--------------------------------------------------------------------------------------------
        '       Schleife zur Berechnung fuer alle vorgegebenen Zyklen und Fahrzeuge:
        '
        '**********************************************************************************************
        '**************************************** Job-Schleife ****************************************
        '**********************************************************************************************
        For jgen = 0 To FilesDim

            jzkl = 0    '<= Damit NrOfRun stimmt

            JobFile = fFileRepl(JobFileList(jgen))

            If Not PHEMmode = tPHEMmode.ModeADVANCE Then GenFile = JobFile

            WorkerJobStatus(jgen, "running...", tJobStatus.Running)

            JobAbortedByErr = False
            MSGwarn = 0
            MSGerror = 0

            If (PHEMmode = tPHEMmode.ModeADVANCE) Then

                WorkerMsg(tMsgID.NewJob, "Job: " & NrOfRunStr() & " / " & ((FilesDim + 1) * (CyclesDim + 1)) & " | " & fFILE(JobFile, True), MsgSrc)
                WorkerStatus("Current Job: " & NrOfRunStr() & " / " & ((FilesDim + 1) * (CyclesDim + 1)) & " | " & fFILE(JobFile, True))


                If Not ERG.ErgEntryInit() Then GoTo lbNextJob

                'Status
                WorkerJobStatus(jgen, "processing input...", tJobStatus.Running)

                ADV = New cADVANCE_V3
                If Not ADV.ADVANCE_V2_Init() Then GoTo lbNextJob

                If ADV.ADVANCEdone Or PHEMworker.CancellationPending Then GoTo lbAbort

                'Status/ProgBar
                WorkerJobStatus(jgen, "running...", tJobStatus.Running)
                ProgBarCtrl.ProgOverallStartInt = 100 * (jgen) / (FilesDim + 1)
                ProgBarCtrl.PgroOverallEndInt = 100 * (jgen + 1) / (FilesDim + 1)

            End If
lbADV:
            If (PHEMmode = tPHEMmode.ModeADVANCE) Then
                If Not ADV.ADVANCE_V2_Next() Then
                    JobAbortedByErr = True
                    GoTo lbNextJob
                End If
                If ADV.ADVANCEdone Then
                    ADV.ADVANCE_V2_Close()
                    GoTo lbNextJob
                End If
            End If

            'Check ob Abbruch
            If PHEMworker.CancellationPending Then GoTo lbAbort

            'Falls Error beim GEN einlesen
            CurrentCycleFile = ""

            'Einlesen der Inputfiles
            '   BATCH: Zyklus aus DRI-Liste
            '   ADVANCE: Zyklus wird nicht eingelesen
            If Not LESE() Then
                JobAbortedByErr = True
                GoTo lbNextJob
            End If

            'Überprüfe ob alle Modi in der GEN Datei lizenziert sind
            LicErrorFeat = 0

            If GEN.VehMode = tVehMode.HEV Then
                If Not Lic.LicFeature(5) Then LicErrorFeat = 5
            ElseIf GEN.VehMode = tVehMode.EV Then
                If Not Lic.LicFeature(6) Then LicErrorFeat = 6
            End If

            If GEN.CreateMap And Not Lic.LicFeature(3) Then LicErrorFeat = 3

            If GEN.EngAnalysis And Not Lic.LicFeature(4) Then LicErrorFeat = 4

            If (GEN.dynkorja And Not Lic.LicFeature(7)) Then LicErrorFeat = 7

            If (GEN.EXSja And Not Lic.LicFeature(8)) Then LicErrorFeat = 8

            If LicErrorFeat > 0 Then
                WorkerMsg(tMsgID.Err, "Your license does not support the selected mode (" & LicErrorFeat & ")", MsgSrc)
                JobAbortedByErr = True
                GoTo lbNextJob
            End If

            'Falls Optimierer aktiv werden hier die Parameter eingelesen
            If bOptOn Then OptInput()

            'BATCH: Sub-Dir Ausgabeordner erstellen
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
            '************************************** Zylus-Schleife ****************************************
            '**********************************************************************************************
            For jzkl = 0 To CyclesDim


                CyclAbrtedByErr = False

                If PHEMmode = tPHEMmode.ModeBATCH Then

                    'ProgBar
                    ProgBarCtrl.ProgOverallStartInt = 100 * (jgen * (CyclesDim + 1) + jzkl) / ((FilesDim + 1) * (CyclesDim + 1))
                    ProgBarCtrl.PgroOverallEndInt = 100 * (jgen * (CyclesDim + 1) + jzkl + 1) / ((FilesDim + 1) * (CyclesDim + 1))

                    'BATCH-Modus: Fahrzyklus nicht aus GEN-Datei sondern aus DRI-Liste
                    CycleFiles.Clear()
                    CycleFiles.Add(fFileRepl(JobCycleList(jzkl)))

                    'Status
                    WorkerMsg(tMsgID.NewJob, "Job: " & (NrOfRunStr()) & " / " & ((FilesDim + 1) * (CyclesDim + 1)) & " | " & fFILE(JobFile, True) & " | " & fFILE(CycleFiles(0), True), MsgSrc)
                    WorkerStatus("Current Job: " & (NrOfRunStr()) & " / " & ((FilesDim + 1) * (CyclesDim + 1)) & " | " & fFILE(JobFile, True) & " | " & fFILE(CycleFiles(0), True))
                    WorkerJobStatus(jgen, "running... " & jzkl + 1 & "/" & (CyclesDim + 1), tJobStatus.Running)

                    'Ausgabe-Name definieren
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
                '********************************** VECTO-Zylus-Schleife **********************************
                jsubcyclecount = CycleFiles.Count
                jsubcycle = 0
                For Each CurrentCycleFile In CycleFiles
                    jsubcycle += 1

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


                    'TODO: Loading Schleife
                    '***************************** VECTO-Loading-Schleife *********************************
                    '**************************************************************************************


                    'Einstiegspunkt für SOC-Start Iteration
                    If GEN.ModeHorEV And SOCnJa Then SOCfirst = True

                    'Aufraumen
                    MODdata.Init()

                    'Zyklus einlesen
                    If (Not (PHEMmode = tPHEMmode.ModeADVANCE)) Then

                        'Einlesen
                        DRI = New cDRI
                        DRI.FilePath = CurrentCycleFile

                        If Not DRI.ReadFile Then
                            CyclAbrtedByErr = True
                            GoTo lbAusg
                        End If

                        'v(s) in v(t) konvertieren (optional)
                        If DRI.Scycle Then
                            If Not DRI.ConvStoT() Then
                                CyclAbrtedByErr = True
                                GoTo lbAusg
                            End If
                        End If

                        'In 1Hz konvertieren (optional)
                        If DRI.Tvorg Then
                            If MsgOut Then WorkerMsg(tMsgID.Normal, "Converting cycle to 1Hz", MsgSrc)
                            If Not DRI.ConvTo1Hz() Then
                                'Fehlermeldung in DRI.Convert()
                                CyclAbrtedByErr = True
                                GoTo lbAusg
                            End If
                        End If

                        'Entnormieren
                        DRI.DeNorm()

                    End If

                    '----------------------------------------------------------------------------
                    '----------------------------------------------------------------------------

                    'Zyklus-Vorgaben initialisieren (Geschw, Beschl, ...)
                    MODdata.CycleInit()

                    If GEN.VehMode = tVehMode.EngineOnly Then

                        If MsgOut Then WorkerMsg(tMsgID.Normal, "Engine Only Calc", MsgSrc)

                        'Rechne .npi-Leistung in Pe und P_clutch um
                        If Not MODdata.Px.Eng_Calc() Then
                            CyclAbrtedByErr = True
                            GoTo lbAusg
                        End If

                    Else

                        'ACHTUNG: VehmodeInit benötigt Infos aus GEN und DRI!
                        If Not VEH.VehmodeInit() Then
                            'Fehlermeldung innerhalb VehmodeInit()
                            JobAbortedByErr = True
                            GoTo lbNextJob
                        End If

                        If GEN.ModeHorEV Then

                            If GEN.VehMode = tVehMode.EV Then

                                If MsgOut Then WorkerMsg(tMsgID.Normal, "EV Init", MsgSrc)

                                If Not MODdata.Px.EVinit Then
                                    WorkerMsg(tMsgID.Err, "ERROR in BAT-Init!", MsgSrc)
                                    JobAbortedByErr = True
                                    GoTo lbNextJob
                                End If

                                If MsgOut Then WorkerMsg(tMsgID.Normal, "EV Calc", MsgSrc)

                                If Not MODdata.Px.EV_Calc() Then
                                    CyclAbrtedByErr = True
                                    GoTo lbAusg
                                End If

                            Else

                                If MsgOut Then WorkerMsg(tMsgID.Normal, "HEV Init", MsgSrc)

                                If Not MODdata.Px.HEVinit Then
                                    'TODO: Meldung...
                                    WorkerMsg(tMsgID.Err, "ERROR in HEV Init!", MsgSrc)
                                    JobAbortedByErr = True
                                    GoTo lbNextJob
                                End If

                                If MsgOut Then WorkerMsg(tMsgID.Normal, "HEV Calc", MsgSrc)

                                If Not MODdata.Px.HEV_Calc() Then
                                    CyclAbrtedByErr = True
                                    GoTo lbAusg
                                End If

                            End If

                        Else

                            If MsgOut Then WorkerMsg(tMsgID.Normal, "Vehicle Calc", MsgSrc)

                            If Not MODdata.Px.Calc() Then
                                CyclAbrtedByErr = True
                                GoTo lbAusg
                            End If

                        End If

                        If PHEMworker.CancellationPending Then GoTo lbAbort

                        'CycleKin (für erg/sum usw.) berechnen
                        MODdata.CylceKin.Calc()

                    End If
                    '----------------------------------------------------------------------------
                    '----------------------------------------------------------------------------


                    'Emissionen und Nachbehandlung - wird bei EV-Modus nicht ausgeführt
                    If Not GEN.VehMode = tVehMode.EV Then

                        If MsgOut Then WorkerMsg(tMsgID.Normal, "Calculating Transient Correction Factors", MsgSrc)

                        'Sekündliche TC Parameter ermittlen
                        MODdata.TC.Calc()

                        'Kennfeld Erstellung
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

                        If MsgOut Then WorkerMsg(tMsgID.Normal, "Em Calc: Raw", MsgSrc)

                        'Rohemissionen berechnen
                        MODdata.Em.Raw_Calc()

                        'TC Parameter umrechnen in Differenz zu Kennfeld-TC-Parameter
                        If MAP.TransMap Then MODdata.TC.CalcDiff()

                        'Dynamik-Korrektur
                        If GEN.dynkorja Then
                            If MsgOut Then WorkerMsg(tMsgID.Normal, "Em Calc: Transient Correction", MsgSrc)
                            MODdata.Em.TC_Calc()
                        End If

                        'Korrektur der Verbrauchswerte kleiner LKW-Motoren bei HBEFA
                        If (Not GEN.PKWja) And Cfg.FCcorrection Then
                            If MsgOut Then WorkerMsg(tMsgID.Normal, "Em Calc: FC-Correction", MsgSrc)
                            FcCorr()
                        End If

                        'Exhaust System Simulation
                        If GEN.EXSja Then
                            If MsgOut Then WorkerMsg(tMsgID.Normal, "Em Calc: EXS", MsgSrc)
                            EXS = New cEXS
                            If Not EXS.Exs_Main() Then
                                CyclAbrtedByErr = True
                                GoTo lbAusg
                            End If
                        End If

                        'Summen / Durchschnitte bilden
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

                    '*** Sekündliche Ausgabe ***
                    If Cfg.ModOut Then
                        If MsgOut Then WorkerMsg(tMsgID.Normal, "Writing modal output", MsgSrc)
                        If Not MODdata.Output() Then
                            CyclAbrtedByErr = True
                            GoTo lbAusg
                        End If

                        WorkerMsg(tMsgID.Normal, "Results written to: " & fFILE(MODdata.ModOutpName & ".vmod", True), MsgSrc, MODdata.ModOutpName & ".vmod")

                    End If

                    'VECTO Ausgabe
                    'TODO: Loadings umschalten...
                    If Not VSUM.SetVals(tVSUM.UserDefLoaded) Then
                        CyclAbrtedByErr = True
                        GoTo lbAusg
                    End If


                    'Ausgabe für BATCH und ADVANCE
lbAusg:
                    If (PHEMmode = tPHEMmode.ModeADVANCE) Then

                        'In ADVANCE ist Zyklus-Abbruch = Job-Abbruch
                        If CyclAbrtedByErr Then
                            JobAbortedByErr = True
                            GoTo lbNextJob
                        End If

                        ADV.STRcalc()
                        If Not ADV.AusgVis_V3() Then
                            JobAbortedByErr = True
                            GoTo lbNextJob
                        End If

                        GoTo lbadv

                    Else

                        'Ausgabe in Erg (Erste Berechnung - Initialisieren & Header)
                        If Not ERG.AusgERG(NrOfRunStr, fFILE(GenFile, True), fFILE(CurrentCycleFile, True), CyclAbrtedByErr) Then GoTo lbErrInJobLoop



                    End If

                    'Daten aufräumen
                    MODdata.CleanUp()

                    'Status-Update
                    If PHEMmode = tPHEMmode.ModeSTANDARD Then
                        WorkerProg(100 * (jgen) / (FilesDim + 1) + 100 * jsubcycle / jsubcyclecount * 1 / (FilesDim + 1), 0)
                    ElseIf PHEMmode = tPHEMmode.ModeBATCH Then
                        WorkerProg(100 * (jgen * (CyclesDim + 1) + jzkl + 1) / ((FilesDim + 1) * (CyclesDim + 1)), 0)
                    End If

                    'TODO: Loading Schleife
                    '******************** ENDE *** VECTO-Loading-Schleife *** ENDE ************************
                    '**************************************************************************************

                    If Not VSUM.Output() Then
                        CyclAbrtedByErr = True
                        GoTo lbAusg
                    End If

                Next

                '************************* ENDE *** VECTO-Zylus-Schleife *** ENDE *************************
                '******************************************************************************************

               

            Next
            '**********************************************************************************************
            '****************************** ENDE *** Zylus-Schleife *** ENDE ******************************
            '**********************************************************************************************
lbNextJob:

            If (PHEMmode = tPHEMmode.ModeADVANCE) Then
                ProgBarCtrl.ProgOverallStartInt = -1
                WorkerProg(100 * (jgen + 1) / (FilesDim + 1), 0)
            End If


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

            'Check ob Abbruch
            If PHEMworker.CancellationPending Then GoTo lbAbort

        Next

        '**********************************************************************************************
        '******************************* ENDE *** Job-Schleife *** ENDE *******************************
        '**********************************************************************************************

        WorkerMsg(tMsgID.Normal, "Summary Results written to: " & fFILE(ERG.ErgFile, True), MsgSrc, ERG.ErgFile)
        WorkerMsg(tMsgID.Normal, "done", MsgSrc)
        PHEM = tCalcResult.Done
        GoTo lbExit


lbErrBefore:  '!!!!!!!!!! Abbruch bevor (!!!) der erste Job angefangen wurde !!!!!!!!!!!
        WorkerMsg(tMsgID.Normal, "aborted", MsgSrc)
        PHEM = tCalcResult.Err

        For i = 0 To FilesDim
            WorkerJobStatus(i, "", tJobStatus.Undef)
        Next

        GoTo lbExit


lbErrInJobLoop:
        WorkerMsg(tMsgID.Normal, "aborted", MsgSrc)
        WorkerJobStatus(jgen, "aborted", tJobStatus.Err)
        PHEM = tCalcResult.Err

        For i = jgen + 1 To FilesDim
            WorkerJobStatus(i, "", tJobStatus.Undef)
        Next

        MODdata.CleanUp()

        GoTo lbExit

lbAbort:
        WorkerMsg(tMsgID.Normal, "aborted", MsgSrc)
        WorkerJobStatus(jgen, "aborted", tJobStatus.Warn)
        PHEM = tCalcResult.Abort

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
        ADV = Nothing
        MODdata = Nothing
        ERG = Nothing

        ENG = Nothing
        GBX = Nothing

    End Function

    '---------------------------------------------------------------------------


End Module
