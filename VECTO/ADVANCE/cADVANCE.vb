Imports System.Collections.Generic

Public Class cADVANCE_V3

    Public FadvSUM As System.IO.StreamWriter
    Public RndSeed As Int16
    Public MISKAMout As Boolean

    Public ADVANCEdone As Boolean

    'TEM (TODO)
    Public ADVkalt As Boolean
    Public ADVtkw As Single
    Public ADVtkat As Single
    Public vtumg As Single

    'ADV
    Private FLTpath As String
    Private TEMpath As String
    Private STRfilter As Boolean
    Private STRroute As Boolean

    'FLT-Collection
    Private FLTfleet As Dictionary(Of String, cFLTfleet)

    Public iadvance As Integer
    Public aVehNr As Int32
    Public aVehType As String
    Public aWorldX As List(Of Single)
    Public aWorldY As List(Of Single)
    Public aStrId As List(Of Int32)


    Private EMlist As List(Of String)
    Private EMdim As Integer

    Private AusgVisInit As Boolean


#Region "FZP Variablen"
    Private FZPpath As String
    Private FZPlen As Int32
    Private FZPsortPath As String
    Private vt() As Int32       '<< Bei Int16: musste Sim-Zeit unter 32767s (~9h) sein => Bei Kreisverkehr-Projekt zu wenig!
    Private t0 As Int32
    Private vX() As Single
    Private vY() As Single
    Private vNr() As Int32      '<< Int16 zu klein (EkoZara: vNr's bis ~ 48000). Möglich wäre allerdings Int16 von -32767 bis +32767 verwenden d.h. Max = 65534
    Private vNrSub() As Int16   '<< Evtl. weghaun...?
    Private vVm() As Single
    Private vStg() As Single
    Private vID() As Int16
    Private vSID() As Int32     '<< Int16 zu klein. (Int16 mit negativem Bereich (2*32767) wahrscheinlich auch noch zu klein)
    Private FZProwT As Int16 = 0
    Private FZProwWeltX As Int16 = 1
    Private FZProwWeltY As Int16 = 2
    Private FZProwFzNr As Int16 = 3
    Private FZProwV As Int16 = 4
    Private FZProwSteig As Int16 = 5
    Private FZProwTyp As Int16 = 6
    Private FZProwStr As Int16 = 7
#End Region

#Region "STR Variablen"

    Private sSIDdim As Int16
    Private STRpaths As List(Of String)     'Liste der STR-Dateien
    Private sSIDlist As List(Of Int32)      'Streckennummer-Liste, aufsteigend sortiert, keine Duplikate
    Private sSIDfiles As List(Of Int32)     'Dateinummern, gleich sortiert wie sSIDlist

    'Felder für Streckennummer, Spur und Koordinaten für alle STR-Dateien 
    Private STRlen As Int32
    Private sStr As List(Of Int32)
    Private sSp As List(Of Int16)
    Private sSegAnX As List(Of Single)
    Private sSegEnX As List(Of Single)
    Private sSegAnY As List(Of Single)
    Private sSegEnY As List(Of Single)

    'STR.SUM
    Private STRerg As List(Of cSTRerg)
    Private STRdistTol As Single


#End Region

#Region "SD3 Variablen"
    Private SD3path As String
    Private sFC() As Single
    Private sNOx() As Single
    Private sCO() As Single
    Private sHC() As Single
    Private sPM() As Single
    Private sBe() As Single
    Private sRu() As Single
    Private sSO2() As Single
    Private ala() As Single
    Private vDTV() As Single
    Private ALKW() As Single
#End Region


    'Berechnung
    Private zFZP As Int32           'Globale FZP-Zeile
    Private LastGen As String       'Letzte .gen-Datei
    Private VehStrAlt As String     'VehStr = Fahrzeug ID: bestehend aus Fahrzeugnummer (aus VISSIM) und SubNr um Zeitlücken zu korrigieren
    Private vNrAlt As Int32
    Private fzpL10 As Int32
    Private fzpL As Int32

    Private SUMpath As String

    'ADVANCE Initialisierung
    Public Function ADVANCE_V2_Init() As Boolean

        Dim msgtext As String
        Dim FZPs As Boolean
        Dim MsgSrc As String

        MsgSrc = "ADV/Init"

        AusgVisInit = False
        ADVANCEdone = False
        FZPs = Cfg.FZPsort

        'ADV Datei einlesen
        If Not ADVread() Then Return False

        'Check ob FLT vorhanden
        If Not IO.File.Exists(FLTpath) Then
            WorkerMsg(tMsgID.Err, "Fleet composition file not found! (" & FLTpath & ")", MsgSrc)
            Return False
        End If

        'Check ob sortierte FZP vorhanden
        If FZPs And Cfg.FZPsortExp Then
            If STRfilter Then
                FZPsortPath = IO.Path.GetDirectoryName(FZPpath) & "\" & IO.Path.GetFileNameWithoutExtension(FZPpath) & "_" & IO.Path.GetFileNameWithoutExtension(STRpaths(0)) & ".sfzp"
            Else
                FZPsortPath = IO.Path.GetDirectoryName(FZPpath) & "\" & IO.Path.GetFileNameWithoutExtension(FZPpath) & ".sorted" & fEXT(FZPpath)
            End If
            If IO.File.Exists(FZPsortPath) Then
                msgtext = "File '" & IO.Path.GetFileName(FZPsortPath).ToString & "' already exists!"
                Select Case F_ADVfzp.Answer(msgtext)
                    Case 0
                        WorkerMsg(tMsgID.Warn, "New Traffic Data path: " & FZPsortPath, MsgSrc)
                        WorkerMsg(tMsgID.Warn, "Traffic Data Sort/Export temporarily disabled.", MsgSrc)
                        FZPpath = FZPsortPath
                        FZPs = False
                    Case 1
                        'GO!
                    Case 2
                        ADVANCEdone = True
                        Return True
                End Select
            End If
        End If

        'FLT einlesen (muss vor STR sein wegen cSTRerg)
        If Not FLTread() Then Return False

        If Not FLTcheck() Then Return False

        'EMlist erstellen
        EMlist = New List(Of String)

        If EVcheck() Then
            EMlist.Add("\\EC")
        Else
            EMlist.Add(sKey.MAP.FC)
            EMlist.Add(sKey.MAP.NOx)
            EMlist.Add(sKey.MAP.CO)
            EMlist.Add(sKey.MAP.HC)
            EMlist.Add(sKey.MAP.PM)
            EMlist.Add(sKey.MAP.PN)
            EMlist.Add(sKey.MAP.NO)
        End If

        EMdim = EMlist.Count - 1



        'STR einlesen
        If STRfilter Then

            If Not STRread() Then Return False

            SD3path = fFileWoExt(JobFile) & ".sd3"        'fPATH(JobFile) & fFILE(JobFile, False) & ".sd3"
            If MISKAMout Then
                ReDim sFC(STRlen)
                ReDim sNOx(STRlen)
                ReDim sCO(STRlen)
                ReDim sHC(STRlen)
                ReDim sPM(STRlen)
                ReDim sBe(STRlen)
                ReDim sRu(STRlen)
                ReDim sSO2(STRlen)
                ReDim ala(STRlen)
                ReDim vDTV(STRlen)
                ReDim ALKW(STRlen)
            End If
        End If

        'Listen erstellen
        aWorldX = New List(Of Single)
        aWorldY = New List(Of Single)
        aStrId = New List(Of Int32)

        'FZP einlesen
        If Not FZPread() Then Return False
        If PHEMworker.CancellationPending Then Return True

        'FZP sortieren (und exportieren)
        If FZPs Then
            ProgBarCtrl.ProgJobInt = 0
            FZPsortieren()
            If Cfg.FZPsortExp Then
                If Not FZPexport() Then Return False
            End If
            If PHEMworker.CancellationPending Then Return True
        End If

        'FZP Check
        FZPcheck()

        'TEM (TODO)
        ADVkalt = False

        'Random-Init
        Rnd(-1)
        Randomize(RndSeed)

        'Für AusgVis
        iadvance = FZPlen

        'Ausgabe-Initialisierung

        '   Dateinamen
        SUMpath = fFileWoExt(JobFile) & ".vehicle.sum"  'fPATH(JobFile) & fFILE(JobFile, False) & ".sum"

        ' Modale Ausgabe
        If Cfg.ModOut Then
            If Not AusgMOD_Init() Then Return False
        End If

        'Startwerte
        zFZP = 0
        VehStrAlt = vNr(1) & "_" & vNrSub(1)
        vNrAlt = -1
        LastGen = ""
        fzpL10 = Math.Max(FZPlen / 100, 1000)
        fzpL = 0
        ProgBarCtrl.ProgJobInt = 0
        WorkerMsg(tMsgID.Normal, "Calculation running... ", MsgSrc)

        Return True

    End Function

    'ADVANCE Speicher frei geben
    Public Sub ADVANCE_V2_Close()
        Dim MsgSrc As String

        MsgSrc = "ADV/Close"

        ProgBarCtrl.ProgJobInt = 0

        WorkerMsg(tMsgID.Normal, "Finalizing... ", MsgSrc)

        'Ausgabe
        AusgVis_V3_Close()

        'Speicher freigeben
        vt = Nothing      'ReDim vt(-1)
        vX = Nothing
        vY = Nothing
        vNr = Nothing
        vNrSub = Nothing
        vVm = Nothing
        vStg = Nothing
        vID = Nothing
        vSID = Nothing
        sStr = Nothing
        sSIDlist = Nothing
        sSIDfiles = Nothing
        sSp = Nothing
        sSegAnX = Nothing
        sSegEnX = Nothing
        sSegAnY = Nothing
        sSegEnY = Nothing
        'ReDim vX(-1)
        'ReDim vY(-1)
        'ReDim vNr(-1)
        'ReDim vNrSub(-1)
        'ReDim vVm(-1)
        'ReDim vStg(-1)
        'ReDim vID(-1)
        'ReDim vSID(-1)
        'ReDim sStr(-1)
        'ReDim sSIDlist(-1)
        'ReDim sSp(-1)
        'ReDim sSegAnX(-1)
        'ReDim sSegEnX(-1)
        'ReDim sSegAnY(-1)
        'ReDim sSegEnY(-1)
        FLTfleet = Nothing 'ReDim FLTfleet(-1)
        If MISKAMout Then
            sFC = Nothing
            sNOx = Nothing
            sCO = Nothing
            sHC = Nothing
            sPM = Nothing
            sBe = Nothing
            sRu = Nothing
            sSO2 = Nothing
            ala = Nothing
            vDTV = Nothing
            ALKW = Nothing
            'ReDim sFC(-1)
            'ReDim sNOx(-1)
            'ReDim sCO(-1)
            'ReDim sHC(-1)
            'ReDim sPM(-1)
            'ReDim sBe(-1)
            'ReDim sRu(-1)
            'ReDim sSO2(-1)
            'ReDim ala(-1)
            'ReDim vDTV(-1)
            'ReDim ALKW(-1)
        End If
        iadvance = 0

        'Listen löschen
        aWorldX = Nothing
        aWorldY = Nothing
        aStrId = Nothing

        AusgVisInit = False

        'Garbage Collection - Soll "System Out of Memory" Exception verhindern (tut's aber nicht!)
        GC.Collect()


    End Sub

    'ADVANCE Fahrzeugberechnung
    Public Function ADVANCE_V2_Next() As Boolean

        Dim VehStr As String
        Dim vNrAkt As Int32
        Dim vIDi As Int16
        Dim z As Int32
        Dim vList As System.Collections.Generic.List(Of Double)
        Dim stgList As System.Collections.Generic.List(Of Double)
        Dim MsgSrc As String

        MsgSrc = "ADV"

lbStart:

        'Check ob Fertig
        If zFZP = FZPlen Then
            ADVANCEdone = True
            WorkerMsg(tMsgID.Normal, "Calculation done", MsgSrc)
            Return True
        End If

        'Zyklus-Klasse initialisieren
        DRI = New cDRI
        DRI.ADVinit()
        vList = DRI.Values(tDriComp.V)
        stgList = DRI.Values(tDriComp.Grad)

        'Fahzeugnummer und Typ definieren (bleibt hier konstant)
        '   Altes Fahrzeug ist VehStrAlt, Aktuelles ist VehStr
        vNrAkt = vNr(zFZP + 1)
        vIDi = vID(zFZP + 1)
        VehStr = vNr(zFZP + 1) & "_" & vNrSub(zFZP + 1)

        'Listen erstellen
        aWorldX.Clear()
        aWorldY.Clear()
        aStrId.Clear()

        't0
        DRI.t0 = vt(zFZP + 1)

        'Definiere Felder für Fahrzeug-Berechnung
        z = -1
        Do
            'Check ob neues Fahrzeug 
            If VehStr <> vNr(zFZP + 1) & "_" & vNrSub(zFZP + 1) Then Exit Do
            z += 1
            zFZP += 1

            'Allgemeiner Fahrzyklus
            vList.Add(vVm(zFZP) / 3.6)
            stgList.Add(vStg(zFZP))

            'ADVANCE
            '   Strecken-Auswertung (MISKAM)
            aWorldX.Add(vX(zFZP))
            aWorldY.Add(vY(zFZP))
            aStrId.Add(vSID(zFZP))

            fzpL += 1
            If fzpL = fzpL10 Then
                ''StatusMSG(7, "Calculation running... " & CInt(zFZP / FZPlen * 100).ToString("00") & "%", False)
                ProgBarCtrl.ProgJobInt = zFZP / FZPlen * 100
                fzpL = 0
            End If

        Loop Until zFZP = FZPlen

        '   Fahrzeug-Identifizierung
        aVehNr = vNrAkt
        aVehType = CStr(vIDi)

        'Check ob Zyklus zu kurz => Überspringen
        If z < 1 Then GoTo lbStart

        'Fahrzeuganzahl je Typ erhöhen
        FLTfleet(aVehType).VehCount += 1

        DRI.tDim = z

        'Check ob letzte Gen-Datei neu zu verwenden ist, sonst Belegung durch FLT
        If vNrAlt = vNrAkt Then
            GenFile = LastGen
        Else
            Try
                GenFile = FLTfleet(key:=CStr(vIDi)).GetGenFile
                If GenFile = "" Then Return False
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, vIDi & " is no valid vehicle type!", MsgSrc)
                Return False
            End Try
        End If

        'VehStr ist ab jetzt ALT
        VehStrAlt = VehStr
        vNrAlt = vNrAkt
        LastGen = GenFile

        Return True

    End Function

    'Abschnittsweise Berechnung
    Public Sub STRcalc()
        Dim t As Integer
        Dim STR0 As Integer
        Dim STR1 As Integer
        Dim t0 As Integer
        Dim dist0 As Double
        Dim time0 As Single
        Dim STRergRef As cSTRerg
        Dim e As Integer
        Dim EMsum() As Double

        If Not STRfilter Then Exit Sub

        STR0 = -1
        t0 = -1
        dist0 = 0
        time0 = 0
        ReDim EMsum(EMdim)

        'Zyklus durchschleifen
        For t = 0 To MODdata.tDim

            'Nr. der STR-Datei
            STR1 = getSTRfileNr(aStrId(t))

            'Falls Wechsel in STR-Nr:
            '   Alte STR-Nr abschließen
            If STR0 <> STR1 Then

                If t0 > -1 Then ' STR0 > -1 Then

                    STRergRef = STRerg(STR0)

                    If dist0 >= STRdistTol * STRergRef.Distance Then

                        'Distanz (gefahrene km)
                        STRergRef.VehDist(aVehType) += dist0

                        'Reisezeit in h
                        STRergRef.VehTime(aVehType) += time0 / 3600

                        'Kfz-Anz
                        STRergRef.VehCount(aVehType) += 1

                        'Summen-Emissionen
                        For e = 0 To EMdim
                            STRergRef.EMsum(aVehType)(e) += EMsum(e) / 3600
                        Next

                    End If

                End If

                t0 = t
                STR0 = STR1
                dist0 = 0
                time0 = 0
                For e = 0 To EMdim
                    EMsum(e) = 0
                Next

            End If

            'Aufsummieren
            dist0 += MODdata.Vh.V(t) / 1000
            time0 += 1
            If GEN.VehMode = tVehMode.EV Then
                EMsum(0) += MODdata.Px.PiBat(t)
            Else
                For e = 0 To EMdim
                    EMsum(e) += Math.Max(MODdata.Em.EmComp(EMlist(e)).FinalVals(t), 0)
                Next
            End If


        Next

        'Letzte STR abschließen
        If t0 > -1 Then

            STRergRef = STRerg(STR0)

            If dist0 >= STRdistTol * STRergRef.Distance Then

                'Distanz (gefahrene km)
                STRergRef.VehDist(aVehType) += dist0

                'Reisezeit in h
                STRergRef.VehTime(aVehType) += time0 / 3600

                'Kfz-Anz
                STRergRef.VehCount(aVehType) += 1

                'Summen-Emissionen
                For e = 0 To EMdim
                    STRergRef.EMsum(aVehType)(e) += EMsum(e) / 3600
                Next

            End If

        End If




    End Sub

    Private Function getSTRfileNr(ByVal SID As Integer) As Integer
        Dim x As Integer
        For x = 0 To sSIDdim
            If sSIDlist(x) = SID Then Return sSIDfiles(x)
        Next
        Return -1
    End Function

    'FLT einlesen
    Private Function FLTread() As Boolean

        Dim fFLT As cFile_V3
        Dim ArP() As Single
        Dim ArGen() As String
        Dim FLTdim As Int16
        Dim IDstr As String
        Dim lastIDstr As String
        Dim ft As cFLTfleet
        Dim lA As String()

        Dim path0 As String
        Dim MsgSrc As String

        MsgSrc = "ADV/Init/FLTread"



        WorkerMsg(tMsgID.Normal, "Reading FLT file...", MsgSrc)

        fFLT = New cFile_V3
        If Not fFLT.OpenRead(FLTpath) Then
            WorkerMsg(tMsgID.Err, "Failed to open .flt file (" & FLTpath & ")", MsgSrc)
            fFLT = Nothing
            Return False
        End If

        ReDim ArP(999)
        ReDim ArGen(999)
        FLTfleet = New Dictionary(Of String, cFLTfleet)
        lastIDstr = ""
        path0 = fPATH(FLTpath)

        Do While Not fFLT.EndOfFile
            lA = fFLT.ReadLine
            IDstr = Trim(lA(0))
            If IDstr <> lastIDstr Then
                If lastIDstr <> "" Then
                    ft = New cFLTfleet(lastIDstr)
                    If Not ft.Init(ArGen, ArP, FLTdim) Then Return False
                    FLTfleet.Add(lastIDstr, ft)
                End If
                lastIDstr = IDstr
                FLTdim = -1
            End If
            FLTdim += 1
            ArGen(FLTdim) = fFileRepl(lA(1), path0)
            ArP(FLTdim) = CSng(lA(2))
        Loop
        If lastIDstr <> "" Then
            ft = New cFLTfleet(lastIDstr)
            If Not ft.Init(ArGen, ArP, FLTdim) Then Return False
            FLTfleet.Add(lastIDstr, ft)
        End If
        ft = Nothing

        fFLT.Close()
        fFLT = Nothing

        ArP = Nothing
        ArGen = Nothing
        FLTdim = Nothing

        Return True

    End Function

    Private Function FLTcheck() As Boolean
        Dim FLTref As cFLTfleet
        Dim check As Boolean

        check = True

        For Each FLTref In FLTfleet.Values
            If Not FLTref.FileCheck() Then check = False
        Next

        Return check

    End Function

    Private Function EVcheck() As Boolean
        Dim GEN0 As cGEN
        Dim flt0 As cFLTfleet

        GEN0 = New cGEN

        For Each flt0 In FLTfleet.Values

            GEN0.FilePath = flt0.GetGenFile

            If Not GEN0.ReadFile Then Return False

            Return (GEN0.VehMode = tVehMode.EV)

        Next

        Return False

    End Function

    'FZP einlesen
    Private Function FZPread() As Boolean

        Dim fReader As Microsoft.VisualBasic.FileIO.TextFieldParser
        Dim f As System.IO.StreamReader
        Dim l As Int32
        Dim lc As Int32
        Dim lc10 As Int32
        Dim line() As String
        Dim line0 As String
        Dim ComDone As Boolean
        Dim x As Int32
        Dim LineSkip As Int16
        Dim lastSID As Int32
        Dim SIDisBad As Boolean
        Dim s As Int32
        Dim FZPlenOgl As Int32
        Dim MsgSrc As String

        MsgSrc = "ADV/Init/TFread"

        If Not IO.File.Exists(FZPpath) Then
            WorkerMsg(tMsgID.Err, "Traffic File not found (" & FZPpath & ")", MsgSrc)
            Return False
        End If

        WorkerMsg(tMsgID.Normal, "Traffic File: Counting lines...", MsgSrc)


        'Dateilänge bestimmen
        Try
            f = New System.IO.StreamReader(FZPpath)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot access Traffic File (" & FZPpath & ")", MsgSrc)
            Return False
        End Try

        x = 0
        LineSkip = 0
        ComDone = False
        Do While Not f.EndOfStream
            line0 = f.ReadLine()
            If Not ComDone Then
                If LineSkip > 200 Then
                    WorkerMsg(tMsgID.Err, "Error in Traffic File: Number of Columns < 8", MsgSrc)
                    Return False
                End If
                If IsNumeric(line0.Split(";")(0)) And UBound(line0.Split(";")) > 6 Then
                    ComDone = True
                Else
                    LineSkip += 1
                    Continue Do
                End If
            End If
            x += 1
        Loop
        FZPlen = x
        f.Close()
        f.Dispose()
        f = Nothing

        If PHEMworker.CancellationPending Then Return True

        'Arrays dimensionieren
        ReDim vt(FZPlen)
        ReDim vX(FZPlen)
        ReDim vY(FZPlen)
        ReDim vNr(FZPlen)
        ReDim vNrSub(FZPlen)
        ReDim vVm(FZPlen)
        ReDim vStg(FZPlen)
        ReDim vID(FZPlen)
        ReDim vSID(FZPlen)

        FZPlenOgl = FZPlen


        If STRfilter Then
            WorkerMsg(tMsgID.Normal, "Traffic File: Reading & Filtering...", MsgSrc)
        Else
            WorkerMsg(tMsgID.Normal, "Traffic File: Reading...", MsgSrc)
        End If

        'Datei einlesen
        fReader = New Microsoft.VisualBasic.FileIO.TextFieldParser(FZPpath, FileFormat)
        fReader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited
        fReader.Delimiters = New String() {";"}

        'Kommentare überspringen
        For l = 1 To LineSkip
            fReader.ReadLine()
        Next
        l = 0
        lc = 0
        lastSID = -1
        lc10 = Math.Max(FZPlen / 100, 1000)
        Do While Not fReader.EndOfData
            line = fReader.ReadFields()
            'Strecken die nicht in der STR Datei aufgeführt sind werden hier entfernt
            If STRfilter Then
                If line(FZProwStr) <> lastSID Then
                    lastSID = line(FZProwStr)
                    SIDisBad = True
                    For s = 0 To sSIDdim
                        If sSIDlist(s) = lastSID Then
                            SIDisBad = False
                            Exit For
                        End If
                    Next
                End If
                If SIDisBad Then
                    FZPlen -= 1
                    lc10 = Math.Max(FZPlen / 100, 1000)
                    GoTo lbProg1    'Continue Do
                End If
            End If
            'Arrays belegen
            l += 1
            vt(l) = line(FZProwT)
            vX(l) = line(FZProwWeltX)
            vY(l) = line(FZProwWeltY)
            vNr(l) = line(FZProwFzNr)
            vVm(l) = line(FZProwV)
            vStg(l) = line(FZProwSteig)
            vID(l) = CInt(line(FZProwTyp))
            vSID(l) = line(FZProwStr)
            vNrSub(l) = 0
            'Progress-Ausgabe
lbProg1:
            lc += 1
            If lc >= lc10 Then
                ProgBarCtrl.ProgJobInt = l / FZPlen * 100
                lc = 0
                If PHEMworker.CancellationPending Then Return True
            End If
        Loop

        'Speicher freigeben
        fReader.Close()
        fReader.Dispose()
        fReader = Nothing

        If FZPlenOgl <> FZPlen Then
            If FZPlen = 0 Then
                WorkerMsg(tMsgID.Err, "Street ID's in traffic file not found in segment file.", MsgSrc)
                Return False
            End If
            'Arrays neu dimensionieren
            ReDim Preserve vt(FZPlen)
            ReDim Preserve vX(FZPlen)
            ReDim Preserve vY(FZPlen)
            ReDim Preserve vNr(FZPlen)
            ReDim Preserve vNrSub(FZPlen)
            ReDim Preserve vVm(FZPlen)
            ReDim Preserve vStg(FZPlen)
            ReDim Preserve vID(FZPlen)
            ReDim Preserve vSID(FZPlen)
        End If

        Return True

    End Function

    'FZP sortiren
    Private Sub FZPsortieren()

        'Dim x0 As Int32
        Dim x1 As Int32
        Dim x2 As Int32
        Dim vNri As Int16
        Dim vNrAnz As Int16
        Dim vNr1 As Int32
        Dim vNr2 As Int32
        Dim vNrSort() As Int32
        Dim vts As Int32
        Dim vXs As Single
        Dim vYs As Single
        Dim vNrs As Int32
        Dim vVms As Single
        Dim vStgs As Single
        Dim vIDs As Int16
        Dim vSIDs As Int32
        Dim lc As Int32
        Dim lc10 As Int32

        Dim x0 As Int32
        Dim xx1 As Int32
        Dim xx2 As Int32
        Dim t1 As Int32
        Dim t2 As Int32
        Dim MsgSrc As String

        MsgSrc = "ADV/Init/TFsort"

        WorkerMsg(tMsgID.Normal, "Sorting traffic data...", MsgSrc)

        ReDim Preserve vNr(FZPlen)
        vNrSort = vNr.Clone
        Array.Sort(vNrSort)

        x1 = 1
        lc10 = Math.Max(FZPlen / 100, 1000)
        lc = 0
        x0 = 1
        Do
            'Aktuelles Fahrzeug ist vNr1
            vNr1 = vNrSort(x1)
            'Zähle Fahrzeuge mit vNr = vNr1
            x2 = x1
            vNrAnz = 0
            Do
                x2 += 1
                vNrAnz += 1
                If x2 > FZPlen Then Exit Do
                vNr2 = vNrSort(x2)
            Loop Until vNr1 <> vNr2
            'vNrAnz = Anzahl der Fahrzeuge mit vNr = vNr1
            'Sortiere alle Fahrzeuge mit vNr = vNr1 nach Fahrzeugnummer
            x2 = x1 - 1
            vNri = 0
            Do
                x2 += 1
                vNr2 = vNr(x2)
                If vNr2 = vNr1 Then
                    If x1 <> x2 Then
                        'Zwischenspeicher = Zeile x1
                        vts = vt(x1)
                        vXs = vX(x1)
                        vYs = vY(x1)
                        vNrs = vNr(x1)
                        vVms = vVm(x1)
                        vStgs = vStg(x1)
                        vIDs = vID(x1)
                        vSIDs = vSID(x1)
                        'Zeile x1 = Zeile x2
                        vt(x1) = vt(x2)
                        vX(x1) = vX(x2)
                        vY(x1) = vY(x2)
                        vNr(x1) = vNr2
                        vVm(x1) = vVm(x2)
                        vStg(x1) = vStg(x2)
                        vID(x1) = vID(x2)
                        vSID(x1) = vSID(x2)
                        'Zeile x2 = Zwischenspeicher
                        vt(x2) = vts
                        vX(x2) = vXs
                        vY(x2) = vYs
                        vNr(x2) = vNrs
                        vVm(x2) = vVms
                        vStg(x2) = vStgs
                        vID(x2) = vIDs
                        vSID(x2) = vSIDs
                    End If
                    vNri += 1
                    x1 += 1
                End If
            Loop Until vNri = vNrAnz
            'vNr1 nach Zeit sortieren
            If x0 < FZPlen Then
                For xx1 = x0 To x1 - 2
                    t1 = vt(xx1)
                    For xx2 = xx1 + 1 To x1 - 1
                        t2 = vt(xx2)
                        If t2 < t1 Then
                            'Zwischenspeicher = Zeile xx1
                            vXs = vX(xx1)
                            vYs = vY(xx1)
                            vVms = vVm(xx1)
                            vStgs = vStg(xx1)
                            vIDs = vID(xx1)
                            vSIDs = vSID(xx1)
                            'Zeile xx1 = Zeile xx2
                            vt(xx1) = t2
                            vX(xx1) = vX(xx2)
                            vY(xx1) = vY(xx2)
                            vVm(xx1) = vVm(xx2)
                            vStg(xx1) = vStg(xx2)
                            vID(xx1) = vID(xx2)
                            vSID(xx1) = vSID(xx2)
                            'Zeile x2 = Zwischenspeicher
                            vt(xx2) = t1
                            vX(xx2) = vXs
                            vY(xx2) = vYs
                            vVm(xx2) = vVms
                            vStg(xx2) = vStgs
                            vID(xx2) = vIDs
                            vSID(xx2) = vSIDs
                            t1 = vt(xx1)
                        End If
                    Next
                Next
            End If
            x0 = x1
            'Status-Ausgabe
            lc += 1
            If lc >= lc10 Then
                '' StatusMSG(7, "Sorting FZP..." & CInt(x1 / FZPlen * 100).ToString("00") & "%", False)
                ProgBarCtrl.ProgJobInt = x1 / FZPlen * 100
                lc = 0
                If PHEMworker.CancellationPending Then Exit Sub
            End If
        Loop Until x1 > FZPlen

        vNrSort = Nothing

    End Sub

    'FZP exportieren
    Private Function FZPexport() As Boolean

        Dim f As System.IO.StreamWriter
        Dim x As Int32
        Dim line() As String
        Dim line0 As String
        Dim s As Int16
        Dim FZPsDim As Int16 = 7
        Dim MsgSrc As String

        MsgSrc = "ADV/Init/TFexport"


        WorkerMsg(tMsgID.Normal, "Exporting sorted traffic data...", MsgSrc)

        Try
            f = My.Computer.FileSystem.OpenTextFileWriter(FZPsortPath, False, FileFormat)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot write to " & FZPsortPath, MsgSrc)
            Return False
        End Try

        ReDim line(FZPsDim)

        line(FZProwT) = "t"
        line(FZProwWeltX) = "WeltX"
        line(FZProwWeltY) = "WeltY"
        line(FZProwFzNr) = "FzNr"
        line(FZProwV) = "v"
        line(FZProwSteig) = "Steig"
        line(FZProwTyp) = "Typ"
        line(FZProwStr) = "Str"

        line0 = line(0)
        For s = 1 To FZPsDim
            line0 &= ";" & line(s)
        Next

        'Header
        f.WriteLine(line0)

        'Daten
        For x = 1 To FZPlen
            line(FZProwT) = vt(x)
            line(FZProwWeltX) = vX(x)
            line(FZProwWeltY) = vY(x)
            line(FZProwFzNr) = vNr(x)
            line(FZProwV) = vVm(x)
            line(FZProwSteig) = vStg(x)
            line(FZProwTyp) = vID(x)
            line(FZProwStr) = vSID(x)
            line0 = line(0)
            For s = 1 To FZPsDim
                line0 &= ";" & line(s)
            Next
            f.WriteLine(line0)
            'f.WriteLine(vt(x) & ";" & vX(x) & ";" & vY(x) & ";" & vNr(x) & ";" & vVm(x) & ";" & vStg(x) & ";" & vID(x) & ";" & vSID(x))
        Next

        f.Close()
        f.Dispose()
        f = Nothing

        Return True

    End Function

    'STR einlesen
    Private Function STRread() As Boolean
        Dim fReader As Microsoft.VisualBasic.FileIO.TextFieldParser
        Dim line() As String
        Dim ComDone As Boolean
        Dim s As Int32
        Dim x As Int32
        Dim path As String
        Dim path0 As String
        Dim pathADV As String
        Dim StrClone As Int32()
        Dim StrFileClone As Int16()
        Dim sSTRfile As List(Of Int16)

        Dim STRerg0 As cSTRerg
        Dim STRdist As Double
        Dim MsgSrc As String

        MsgSrc = "ADV/Init/SFread"

        WorkerMsg(tMsgID.Normal, "Reading segment files...", MsgSrc)

        pathADV = fPATH(JobFile)

        sSTRfile = New List(Of Short)
        sStr = New List(Of Integer)
        sSp = New List(Of Short)
        sSegAnX = New List(Of Single)
        sSegEnX = New List(Of Single)
        sSegAnY = New List(Of Single)
        sSegEnY = New List(Of Single)
        sSIDlist = New List(Of Integer)
        sSIDfiles = New List(Of Integer)
        STRlen = -1
        x = -1

        STRerg = New List(Of cSTRerg)

        For Each path0 In STRpaths

            path = fFileRepl(path0, pathADV)

            If Not IO.File.Exists(path) Then
                WorkerMsg(tMsgID.Err, "Segments File not found (" & path & ")", MsgSrc)
                Return False
            End If

            x += 1
            STRerg0 = New cSTRerg
            STRerg0.Init()
            STRerg0.MySTRpath = path

            STRdist = 0

            fReader = New Microsoft.VisualBasic.FileIO.TextFieldParser(path, FileFormat)
            fReader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited
            fReader.Delimiters = New String() {";"}

            ComDone = False
            Do While Not fReader.EndOfData
                line = fReader.ReadFields
                If Not ComDone Then
                    If IsNumeric(line(0)) Then
                        ComDone = True
                    Else
                        Continue Do
                    End If
                End If
                STRlen += 1
                sStr.Add(line(0))
                sSp.Add(line(1))
                sSegAnX.Add(line(2))
                sSegEnX.Add(line(3))
                sSegAnY.Add(line(4))
                sSegEnY.Add(line(5))
                sSTRfile.Add(x)

                STRdist += ((sSegEnY(STRlen) - sSegAnY(STRlen)) ^ 2 + (sSegEnX(STRlen) - sSegAnX(STRlen)) ^ 2) ^ 0.5

            Loop

            fReader.Close()
            fReader.Dispose()

            STRerg0.Distance = CSng(STRdist / 1000)
            STRerg.Add(STRerg0)

        Next

        fReader = Nothing

        'SID-Liste erstellen
        ReDim StrClone(STRlen)
        ReDim StrFileClone(STRlen)
        sStr.CopyTo(StrClone)
        sSTRfile.CopyTo(StrFileClone)

        Array.Sort(StrClone, StrFileClone)

        s = StrClone(0)
        sSIDlist.Add(s)
        sSIDfiles.Add(StrFileClone(0))
        sSIDdim = 0
        For x = 1 To STRlen
            If s <> StrClone(x) Then
                s = StrClone(x)
                sSIDdim += 1
                sSIDlist.Add(s)
                sSIDfiles.Add(StrFileClone(x))
            End If
        Next

        sSTRfile = Nothing

        Return True

    End Function


    'FZP Check:
    '   ...Fahrzeuge aufteilen, die Lücke im Zeitverlauf haben
    Private Sub FZPcheck()

        Dim z As Int32
        Dim t1 As Int32
        Dim vnr1 As Int32
        Dim SubNr As Int16
        Dim MsgSrc As String

        MsgSrc = "ADV/Init/TFcheck"

        WorkerMsg(tMsgID.Normal, "Searching for time gaps > 1s...", MsgSrc)

        t1 = vt(1)
        vnr1 = vNr(1)
        SubNr = 0
        For z = 2 To FZPlen
            If vNr(z) = vnr1 Then
                If vt(z) - t1 > 1.1 Then
                    WorkerMsg(tMsgID.Warn, "Time gap found! Vehicle " & vnr1 & " at t= " & vt(z), MsgSrc)
                    SubNr += 1
                End If
                If SubNr > 0 Then vNrSub(z) = SubNr
            Else
                SubNr = 0
            End If
            vnr1 = vNr(z)
            t1 = vt(z)
        Next

    End Sub

    'ADV einlesen
    Private Function ADVread() As Boolean

        Dim fADV As cADV
        Dim line As String = ""
        Dim s As String = ""
        Dim i As Integer
        Dim MsgSrc As String

        MsgSrc = "ADV/Init/ADVread"

        fADV = New cADV
        fADV.FilePath = JobFile

        '********** .ADV Datei einlesen ********
        If Not fADV.ReadFile Then
            WorkerMsg(tMsgID.Err, "Failed to open .adv file (" & JobFile & ")", MsgSrc)
            fADV = Nothing
            Return False
        End If

        STRpaths = New List(Of String)

        'Zeile 1: FZP-Datei
        FZPpath = fADV.FZPpath

        'Zeile 2: FLT-Datei
        FLTpath = fADV.FLTpath

        'Zeile 3: TEM-Datei
        TEMpath = fADV.TEMpath

        'Zeile 4: RndSeed
        RndSeed = fADV.RndSeed

        'Zeile 5: MISKAMout True/False
        MISKAMout = fADV.SD3out

        'Zeile 6: STRfilter True/False
        STRfilter = fADV.STRfilter

        'Zeile 7: STR.SUM Streckenfilter
        STRdistTol = fADV.STRSUMdistflt / 100

        'Zeile 8+: STR Dateien
        For i = 1 To fADV.STRcount
            STRpaths.Add(fADV.STRpaths(i - 1))
        Next

        '*****************************************

        fADV = Nothing

        Return True

    End Function

    Private Function AusgMOD_Init() As Boolean

        Dim f As cFile_V3
        Dim VehType As String

        For Each VehType In FLTfleet.Keys

            'Ausgabepfad definieren
            MODdata.ModOutpName = fFileWoExt(JobFile)

            f = New cFile_V3

            If Not f.OpenWrite(MODdata.ModOutpName & "_" & VehType & ".mod") Then Return False

            'Header:
            f.WriteLine("VECTO " & VECTOvers)
            f.WriteLine(Now.ToString)
            f.WriteLine("Input File: " & JobFile)
            f.WriteLine("Modal Results For Type " & VehType)
            f.WriteLine(" ")

            f.Close()

        Next

        Return True

    End Function

    Private Function AusgSUM_Init() As Boolean
        Dim MsgSrc As String

        MsgSrc = "ADV/OutpInit/SumInit"

        ' *********************************** .sum *************************************
        'File mit Summen Ergebnissen ueber alle Kfz:
        Try
            FadvSUM = My.Computer.FileSystem.OpenTextFileWriter(SUMpath, False, FileFormat)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot access .sum file (" & SUMpath & ")", MsgSrc)
            Return False
        End Try
        FadvSUM.AutoFlush = True

        'Header
        FadvSUM.WriteLine("VECTO ADVANCE Average results per vehicle")
        FadvSUM.WriteLine("VECTO " & VECTOvers)
        FadvSUM.WriteLine("Inputfile: " & JobFile)
        FadvSUM.WriteLine(ERG.ERGinfo)
        FadvSUM.WriteLine("VehNr,VehType,GEN," & ERG.ErgHead())
        FadvSUM.WriteLine("[-],[-],[-]," & ERG.ErgUnits())
        FadvSUM.Close()

        Return True

    End Function

    'Ausgabe Schließen
    Private Function AusgVis_V3_Close() As Boolean
        Dim F611 As New cFile_V3
        Dim secvis As Single
        Dim j As Int32
        Dim js As Int32, ivd As Int32
        Dim axi As Single, ayi As Single
        Dim amaz As Int32
        Dim amiz As Int32

        FadvSUM.Dispose()
        FadvSUM = Nothing

        If MISKAMout Then
            amaz = vt(1)
            amiz = vt(1)
            For j = 2 To FZPlen
                If vt(j) > amaz Then amaz = vt(j)
                If vt(j) < amiz Then amiz = vt(j)
            Next
            secvis = amaz - amiz

            'C
            'C      Umrechnung auf g Emissionen je Sekunde sowie Kfz/Tag
            For j = 1 To STRlen
                sFC(j) = sFC(j) / secvis
                sNOx(j) = sNOx(j) / secvis
                sCO(j) = sCO(j) / secvis
                sHC(j) = sHC(j) / secvis
                sPM(j) = sPM(j) / secvis
                sBe(j) = sBe(j) / secvis
                sRu(j) = sRu(j) / secvis
                sSO2(j) = sSO2(j) / secvis
                vDTV(j) = vDTV(j) * 86400 / secvis
                ALKW(j) = ALKW(j) * 86400 / secvis
                'C
            Next j
            'C
            'C      Umrechnung auf mg Emissionen je Meter
            'C     Streckenlaenge [m] wird aus Gerade zwischen Anfangs- und Endpunkt berechnet:
            'C
            For j = 1 To STRlen
                axi = (sSegEnX(j) - sSegAnX(j)) ^ 2
                ayi = (sSegEnY(j) - sSegAnY(j)) ^ 2
                ala(j) = (axi + ayi) ^ 0.5
                sFC(j) = 1000 * sFC(j) / ala(j)
                sNOx(j) = 1000 * sNOx(j) / ala(j)
                sCO(j) = 1000 * sCO(j) / ala(j)
                sHC(j) = 1000 * sHC(j) / ala(j)
                sPM(j) = 1000 * sPM(j) / ala(j)
                sBe(j) = 1000 * sBe(j) / ala(j)
                sRu(j) = 1000 * sRu(j) / ala(j)
                sSO2(j) = 1000 * sSO2(j) / ala(j)
            Next j
            'C
            'C      Ausgabe der Ergebnisse
            'C
            'C      Filename fuer Ergebnisfile = Name Eingabefile aber mit *.sd3:
            'C

            'C
            'C       File mit Summen Ergebnissen ueber alle Streckenstuecke:
            Call F611.OpenWrite(SD3path, ";")
            'C
            Call F611.WriteLine("Results from VECTO ADVANCE")   ' Format 630
            Call F611.WriteLine("VECTO " & VECTOvers)   ' Format '(a25)'
            Call F611.WriteLine("Average results per line over total time interval [sec]:", secvis)   ' Format 640
            Call F611.WriteLine("Cold start extra emissions are included")   ' Format *
            Call F611.WriteLine("lfd.; x1; y1; x2; y2;Strassen;; mittl.Emiss. [mg/(m*s)];;; ; ; ; ; ; ; ;DTV;Lkw-An-; Fahr-; Qh/;Typ; S_z0;Str-;; lNfz; lBus; F; L; W;Abschn.-")   ' Format '(A150)'
            Call F611.WriteLine("Nr.;[m];[m];[m];[m];Breite[m];Höhe[m];NOx;BENZOL;RUSS;PM10;KW;CO;CO2;SO2;Stoff 9; Stoff 10;[Kfz/d];teil[-];muster;SBr[m];[-];[m];Nr;Strassenkategorie;[-];[-]; S; N; O;Laenge[m]")   ' Format '(A180)'
            'C
            For js = 1 To STRlen
                ivd = Math.Round(vDTV(js), 0, MidpointRounding.AwayFromZero)
                Call F611.WriteLine(js, sSegAnX(js), sSegAnY(js), sSegEnX(js), sSegEnY(js), -99.9, -99.9, sNOx(js), sBe(js), sRu(js), sPM(js), sHC(js), sCO(js), sFC(js) * 3.14, sSO2(js), "-99", "-99", ivd, ALKW(js), " ", "-99", "-99", "-99", sStr(js), " ", " ", " ", " ", " ", " ", ala(js))   ' Format 620
            Next js

            Call F611.Close()
            F611 = Nothing
        End If

        If STRfilter Then
            If Not AusgSTRerg() Then Return False
        End If

        Return True

    End Function

    Public Function AusgVis_V3() As Boolean
        Dim sumNr As Int32
        Dim ie As Short
        Dim MsgSrc As String

        MsgSrc = "ADV/OutpInit"


        If Not AusgVisInit Then
            AusgVisInit = True
            If Not AusgSUM_Init() Then Return False
        End If

        If MISKAMout Then Call AusMISK_V2()

        sumNr = 0

        For ie = 0 To 1
            Try
                FadvSUM = My.Computer.FileSystem.OpenTextFileWriter(SUMpath, True, FileFormat)
                FadvSUM.AutoFlush = True
                Exit For
            Catch ex As Exception
                If ie = 0 Then
                    System.Threading.Thread.Sleep(2000)
                Else
                    WorkerMsg(tMsgID.Err, "Failed to access .sum file (" & SUMpath & ")", MsgSrc)
                    Return False
                End If
            End Try
        Next

        FadvSUM.WriteLine(aVehNr & "," & aVehType & "," & fFILE(GenFile, True) & "," & ERG.ErgLine())   ' Format 661

        FadvSUM.Close()
        FadvSUM.Dispose()

        Return True

    End Function


    Private Sub AusMISK_V2()
        'C     Unterprogramm zu PHEM/Advance zur Ausgabe der Ergebnisse an MISKAM Luftguetemodell von Lohmeyer
        'C     Daten jeweils durch Semikolon getrennt ausgegeben
        'C
        'C
        'C
        ' include "com.inc"<<<<<<<<<<<<<<<<<<<<<<<<<<
        Dim chari As String = ""
        Dim ami As Single, ax As Single, ay As Single, akabst As Single, C1 As Single, C2 As Single
        Dim jm1 As Long, jist As Long
        Dim j As Int32
        Dim jz As Int32
        'C     -------------------------------------------------------------------------------------------------
        'C
        'C     Aufaddieren der Emissionsdaten nach Streckenabschnitten, ueber jede Sekunde fuer jedes Fahrzeug
        'C     Achtung: jeweils 2 Richtungen je Abschnitt moeglich aber nur StrId in *.fzp uebergeben -> auf "naechstgelegene"
        'C     Richtung zugewiesen
        'C


        For jz = 0 To MODdata.tDim
            'C
            ami = 1000000000000
            For j = 0 To STRlen
                'C
                If (aStrId(jz) = sStr(j)) Then
                    'C      Suche nach naechstgelegenem Strassenteilstueck
                    'C      Koordinaten Kfz:
                    ax = aWorldX(jz)
                    ay = aWorldY(jz)
                    'C      Koordinaten Strecke: sSegAnX(j), sSegEnX(j), sSegAnY(j), sSegEnY(j) aus Eingabe
                    'C      Abstandsumme zu Anfang- und Endpunkt des Streckenabschnittes j
                    C1 = ((ax - sSegAnX(j)) ^ 2 + (ay - sSegAnY(j)) ^ 2) ^ 0.5
                    C2 = ((ax - sSegEnX(j)) ^ 2 + (ay - sSegEnY(j)) ^ 2) ^ 0.5
                    akabst = C1 + C2
                    If (akabst <= ami) Then
                        ami = akabst
                        jist = j
                    End If
                End If
            Next j
            'C      Falls Streckennummer auf *.fzp File in *.str File nicht existiert, wird auf naechstgelegenes Stueck zugewiesen (gleiches System wie oben):
            If (ami = 1000000000000) Then
                If (STRlen >= 1) Then
                    For j = 1 To STRlen
                        ax = aWorldX(jz)
                        ay = aWorldY(jz)
                        C1 = ((ax - sSegAnX(j)) ^ 2 + (ay - sSegAnY(j)) ^ 2) ^ 0.5
                        C2 = ((ax - sSegEnX(j)) ^ 2 + (ay - sSegEnY(j)) ^ 2) ^ 0.5
                        akabst = C1 + C2
                        If (akabst <= ami) Then
                            ami = akabst
                            jist = j
                        End If
                    Next j
                End If
            End If
            'C     Aufsummierung der Emissionen auf den jeweils zugehoerigen Streckenabschnitten:
            'C     berechnung sekuendlich in (g/h)/3600 -> g/Strecke ueber gesamte Zeit
            sFC(jist) = sFC(jist) + (MODdata.Em.EmDefComp(tMapComp.FC).FinalVals(jz)) / 3600
            sNOx(jist) = sNOx(jist) + (MODdata.Em.EmDefComp(tMapComp.NOx).FinalVals(jz)) / 3600
            sCO(jist) = sCO(jist) + (MODdata.Em.EmDefComp(tMapComp.CO).FinalVals(jz)) / 3600
            sHC(jist) = sHC(jist) + (MODdata.Em.EmDefComp(tMapComp.HC).FinalVals(jz)) / 3600
            sPM(jist) = sPM(jist) + (MODdata.Em.EmDefComp(tMapComp.PM).FinalVals(jz)) / 3600

            'C      Zaehlen der Kfz fuer DTV (nur wenn nicht in voriger Sekunde auch schon auf der Strecke
            If (jist <> jm1) Then
                vDTV(jist) = vDTV(jist) + 1
                If Not GEN.PKWja Then
                    ALKW(jist) = ALKW(jist)
                End If
            End If
            'C    ,
            'C     Grobe Rechnung Benzol nach GLOBEMI (HBEFA):
            'C      Trennung nach Otto, Diesel, LKW, PKW,vor/nach EURO 1
            'C     Otto:
            If GEN.ottoJa Then
                If GEN.PKWja Then
                    If (GEN.eklasse < 1) Then
                        sBe(jist) = sBe(jist) + (MODdata.Em.EmDefComp(tMapComp.HC).FinalVals(jz) * 0.0438) / 3600
                    Else
                        sBe(jist) = sBe(jist) + (MODdata.Em.EmDefComp(tMapComp.HC).FinalVals(jz) * 0.1293) / 3600
                        'Continue For
                    End If
                Else
                    sBe(jist) = sBe(jist) + (MODdata.Em.EmDefComp(tMapComp.HC).FinalVals(jz) * 0.05) / 3600
                End If
                'C     Diesel
            Else
                If GEN.PKWja Then
                    sBe(jist) = sBe(jist) + (MODdata.Em.EmDefComp(tMapComp.HC).FinalVals(jz) * 0.0167) / 3600
                Else
                    sBe(jist) = sBe(jist) + (MODdata.Em.EmDefComp(tMapComp.HC).FinalVals(jz) * 0.0167) / 3600
                End If
            End If
            'C
            'C     Grobe Rechnung Russ, Anteile Russ an PM derzeit nur Schaetzwerte!!!!:
            'C      Trennung nach Otto, Diesel, LKW, PKW,vor/nach EURO 1
            'C     Diesel:
            If Not GEN.ottoJa Then
                If GEN.PKWja Then
                    If (GEN.eklasse < 2) Then
                        sRu(jist) = sRu(jist) + (MODdata.Em.EmDefComp(tMapComp.PM).FinalVals(jz) * 0.5) / 3600
                    ElseIf (GEN.eklasse > 4) Then
                        sRu(jist) = sRu(jist) + (MODdata.Em.EmDefComp(tMapComp.PM).FinalVals(jz) * 0.1) / 3600
                    Else
                        sRu(jist) = sRu(jist) + (MODdata.Em.EmDefComp(tMapComp.PM).FinalVals(jz) * 0.75) / 3600
                    End If
                Else
                    If (GEN.eklasse > 3) Then
                        sRu(jist) = sRu(jist) + (MODdata.Em.EmDefComp(tMapComp.PM).FinalVals(jz) * 0.7) / 3600
                    Else
                        sRu(jist) = sRu(jist) + (MODdata.Em.EmDefComp(tMapComp.PM).FinalVals(jz) * 0.5) / 3600
                    End If
                End If
                'C     Otto
            Else
                If GEN.PKWja Then
                    sRu(jist) = sRu(jist) + (MODdata.Em.EmDefComp(tMapComp.PM).FinalVals(jz) * 0.1) / 3600
                Else
                    sRu(jist) = sRu(jist) + (MODdata.Em.EmDefComp(tMapComp.PM).FinalVals(jz) * 0.1) / 3600
                End If
            End If
            'C
            'C     SO2-Emissionen aus dem  im Kraftstoff enthaltenen
            'C     Schwefel gerechnet. Mit Masse SO2 = (Masse% S / 100) * 2
            'C     Diesel_
            If Not GEN.ottoJa Then
                sSO2(jist) = sSO2(jist) + (MODdata.Em.EmDefComp(tMapComp.FC).FinalVals(jz) * 0.001 / 50) / 3600
                'C     Otto:
            Else
                sSO2(jist) = sSO2(jist) + (MODdata.Em.EmDefComp(tMapComp.FC).FinalVals(jz) * 0.0005 / 50) / 3600
            End If
            'C
            jm1 = jist
            'C
        Next jz

    End Sub

    Private Function AusgSTRerg() As Boolean
        Dim s As System.Text.StringBuilder
        Dim Sepp As String = ","
        Dim STRergRef As cSTRerg
        Dim VehType As String
        Dim km As Double
        Dim sum As Double
        Dim e As Integer
        Dim fSSUM As cFile_V3
        Dim Fleet0 As cFLTfleet
        Dim STRSUMpath As String
        Dim MsgSrc As String

        MsgSrc = "ADV/Outp/SegSum"

        fSSUM = New cFile_V3
        s = New System.Text.StringBuilder

        '******** Ausgabe der einzelnen STR-Ergebnisse '*********
        For Each STRergRef In STRerg
            If Not STRergRef.Ausg() Then Return False
        Next

        '********* Ausgabe Summen über alle STR's '*********

        STRSUMpath = fFileWoExt(JobFile) & ".segment.sum"

        If Not fSSUM.OpenWrite(STRSUMpath) Then
            WorkerMsg(tMsgID.Err, "Failed to write to '" & STRSUMpath & "'!", MsgSrc)
            Return False
        End If


        '** Datei-Header
        fSSUM.WriteLine("VECTO ADVANCE Sum Results by Segments and Vehicle Types")
        fSSUM.WriteLine("VECTO " & VECTOvers)
        fSSUM.WriteLine(Now.ToString)
        fSSUM.WriteLine("Inputfile: " & JobFile)

        '****************************************************************
        '*************************** Em je km ***************************

        'Header            
        s.Length = 0
        s.Append("*** Results per km  ***,Vehicles,Travel Time,Travelled Distance, Average Speed")
        If GEN.VehMode = tVehMode.EV Then
            s.Append(Sepp & "EC")
        Else
            For e = 0 To EMdim
                s.Append(Sepp & MODdata.Em.EmComp(EMlist(e)).Name)
            Next
        End If
        fSSUM.WriteLine(s.ToString)

        'Untits
        s.Length = 0
        s.Append("Segment / Vehicle Type,[-],[h],[km],[km/h]")
        If GEN.VehMode = tVehMode.EV Then
            s.Append(Sepp & "[kWh/km]")
        Else
            s.Append(Sepp & "[g/km]")
            s.Append(Sepp & "[g/km]")
            s.Append(Sepp & "[g/km]")
            s.Append(Sepp & "[g/km]")
            s.Append(Sepp & "[g/km]")
            s.Append(Sepp & "[#/km]")
            s.Append(Sepp & "[g/km]")
        End If
        fSSUM.WriteLine(s.ToString)

        '** Em je Segment
        For Each STRergRef In STRerg

            s.Length = 0

            'Segment-Name
            s.Append(fFILE(STRergRef.MySTRpath, True))

            'Anzahl Fahrzeuge
            s.Append(Sepp & STRergRef.AllVehCount)

            'Reisezeit
            sum = STRergRef.AllVehTime
            s.Append(Sepp & sum)

            'km
            km = STRergRef.AllVehDist
            s.Append(Sepp & km)

            'Speed
            If sum = 0 Then
                s.Append(Sepp & "-")
            Else
                s.Append(Sepp & km / sum)
            End If

            'Em je km
            For e = 0 To EMdim
                If km = 0 Then
                    s.Append(Sepp & "-")
                Else
                    s.Append(Sepp & STRergRef.AllEm(e) / km)
                End If
            Next

            'Schreiben
            fSSUM.WriteLine(s.ToString)

        Next

        '** Em je Vehicle Type
        For Each VehType In FLTfleet.Keys

            s.Length = 0

            'Type
            s.Append("Veh " & VehType)

            'Anzahl Fahrzeuge
            s.Append(Sepp & FLTfleet(VehType).VehCount)

            'Reisezeit, Strecke, Avg.Speed
            sum = 0
            km = 0
            For Each STRergRef In STRerg
                sum += STRergRef.VehTime(VehType)
                km += STRergRef.VehDist(VehType)
            Next
            s.Append(Sepp & sum)
            s.Append(Sepp & km)
            If sum = 0 Then
                s.Append(Sepp & "-")
            Else
                s.Append(Sepp & km / sum)
            End If

            'Em
            For e = 0 To EMdim
                If km = 0 Then
                    s.Append(Sepp & "-")
                Else
                    sum = 0
                    For Each STRergRef In STRerg
                        sum += STRergRef.EMsum(VehType)(e)
                    Next
                    s.Append(Sepp & sum / km)
                End If
            Next

            'Schreiben
            fSSUM.WriteLine(s.ToString)

        Next

        '** Summe
        s.Length = 0

        'Segment
        s.Append("Sum")

        'Anzahl Fahrzeuge wird nicht aus STRerg berechnet (macht keinen Sinn) sondern aus cFLTfleet-Aufzeichnung
        sum = 0
        For Each Fleet0 In FLTfleet.Values
            sum += Fleet0.VehCount
        Next
        s.Append(Sepp & sum)

        'Reisezeit
        sum = 0
        For Each STRergRef In STRerg
            sum += STRergRef.AllVehTime
        Next
        s.Append(Sepp & sum)

        'km
        km = 0
        For Each STRergRef In STRerg
            km += STRergRef.AllVehDist
        Next
        s.Append(Sepp & km)

        'Speed
        If sum = 0 Then
            s.Append(Sepp & "-")
        Else
            s.Append(Sepp & km / sum)
        End If

        'Em je km
        For e = 0 To EMdim
            If km = 0 Then
                s.Append(Sepp & "-")
            Else
                sum = 0
                For Each STRergRef In STRerg
                    sum += STRergRef.AllEm(e)
                Next
                s.Append(Sepp & sum / km)
            End If
        Next

        'Schreiben
        fSSUM.WriteLine(s.ToString)

        '****************************************************************
        '**************************** Em abs ****************************

        fSSUM.WriteLine(" ")

        'Header            
        s.Length = 0
        s.Append("*** Absolute Values ***")
        If GEN.VehMode = tVehMode.EV Then
            s.Append(Sepp & "EC")
        Else
            For e = 0 To EMdim
                s.Append(Sepp & MODdata.Em.EmComp(EMlist(e)).Name)
            Next
        End If
        fSSUM.WriteLine(s.ToString)

        'Untits
        s.Length = 0
        s.Append("Segment / Vehicle Type")
        If GEN.VehMode = tVehMode.EV Then
            s.Append(Sepp & "[kWh]")
        Else
            s.Append(Sepp & "[g]")
            s.Append(Sepp & "[g]")
            s.Append(Sepp & "[g]")
            s.Append(Sepp & "[g]")
            s.Append(Sepp & "[g]")
            s.Append(Sepp & "[#]")
            s.Append(Sepp & "[g]")
        End If
        fSSUM.WriteLine(s.ToString)

        '** Em je Segment
        For Each STRergRef In STRerg

            s.Length = 0

            'Segment-Name
            s.Append(fFILE(STRergRef.MySTRpath, True))

            'Em abs
            For e = 0 To EMdim
                s.Append(Sepp & STRergRef.AllEm(e))
            Next

            'Schreiben
            fSSUM.WriteLine(s.ToString)

        Next

        '** Em je Vehicle Type
        For Each VehType In FLTfleet.Keys

            s.Length = 0

            'Type
            s.Append("Veh " & VehType)

            'Em
            For e = 0 To EMdim
                sum = 0
                For Each STRergRef In STRerg
                    sum += STRergRef.EMsum(VehType)(e)
                Next
                s.Append(Sepp & sum)
            Next

            'Schreiben
            fSSUM.WriteLine(s.ToString)

        Next

        '** Summe
        s.Length = 0

        'Segment
        s.Append("Sum")

        'Em abs
        For e = 0 To EMdim
            sum = 0
            For Each STRergRef In STRerg
                sum += STRergRef.AllEm(e)
            Next
            s.Append(Sepp & sum)
        Next

        'Schreiben
        fSSUM.WriteLine(s.ToString)

        fSSUM.Close()

        Return True

    End Function

    '.mod-Datei zerlegen
    Public Sub AusgModCut(ByVal InPath As String, ByVal VehList As Int32())
        Dim f As System.IO.StreamReader
        Dim VehNr As Int32
        Dim x As Int16
        Dim x1 As Int16
        Dim fOut As cFile_V3
        Dim laststring As String
        Dim foundNr As Int32
        Dim CheckLast As Boolean
        Dim MsgSrc As String

        MsgSrc = "ADV/ModCut"

        WorkerMsg(tMsgID.Normal, "Starting .mod split.", MsgSrc)

        'InFile öffnen
        Try
            f = New System.IO.StreamReader(InPath)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot access .mod file (" & InPath & ")", MsgSrc)
            WorkerMsg(tMsgID.Normal, "Aborted!", MsgSrc)
            Exit Sub
        End Try

        WorkerMsg(tMsgID.Normal, "File " & InPath, MsgSrc)
        WorkerStatus(".mod split running. File " & fFILE(InPath, True))

        fOut = New cFile_V3
        CheckLast = False

        'Anzahl VehNummern
        x1 = 1 + UBound(VehList) - x
        x = 0
        laststring = ""

        'Schleife durch alle VehNummern in Liste
        For Each VehNr In VehList

            'Abbruch falls Userabbruch
            If PHEMworker.CancellationPending Then
                WorkerMsg(tMsgID.Normal, "Aborted by User.", MsgSrc)
                GoTo lbDone
            End If

            'Abbruch falls Datei fertig
            If f.EndOfStream Then
                WorkerMsg(tMsgID.Err, "Reached end of file before last vehicle number was found.", MsgSrc)
                WorkerMsg(tMsgID.Normal, "Aborted! (Reached end of file)", MsgSrc)
                GoTo lbDone
            End If

            'Ausgabedatei öffnen / Abbruch wenn Fehler beim Öffnen
            If Not fOut.OpenWrite(fFileWoExt(InPath) & "_Veh" & VehNr & ".mod") Then
                WorkerMsg(tMsgID.Err, "Cannot create output file. Veh.Nr. " & VehNr, MsgSrc)
                GoTo lbDone
            End If

            'Schleife durch Datei
            Do While Not f.EndOfStream

                'Falls String schon eine VehNr enthält wird Zeile nicht nochmal eingelesen (siehe unten)
                If CheckLast Then
                    CheckLast = False
                Else
                    'Zeile einlesen
                    laststring = f.ReadLine
                End If

                'Falls Zeile mit VehNr gefunden: VehNr rausholen
                If InStr(laststring, "VehNr:", CompareMethod.Text) > 0 Then
                    foundNr = GetVehNr(laststring)

                    'Falls VehNr die gesuchte ist: Ausgabedatei schreiben
                    If foundNr = VehNr Then

                        'Erste Zeile schreiben ("VehNr: ...")
                        fOut.WriteLine(laststring)

                        'Nächste Zeile lesen (sonst wird Do While übersprungen)
                        laststring = f.ReadLine

                        'Schleife bis nächste VehNr / Dateiende
                        Do While Not f.EndOfStream

                            'Wenn nächstes Fahrzeug:
                            If InStr(laststring, "VehNr:", CompareMethod.Text) > 0 Then
                                If GetVehNr(laststring) = VehNr Then
                                    'Wenn gleiche Fahrzeugnummer: Datei weiter schreiben
                                    'Header und Units überspringen
                                    f.ReadLine()
                                    f.ReadLine()
                                    laststring = f.ReadLine
                                Else
                                    'Sonst: Raus aus Schleife
                                    Exit Do
                                End If
                            End If

                            'Zeile schreiben
                            fOut.WriteLine(laststring)

                            'Zeile lesen
                            laststring = f.ReadLine

                        Loop

                        'Falls nicht EndOfStream Flag setzen damit nächste VehNr nicht übersprungen wird
                        If Not f.EndOfStream Then CheckLast = True

                        'Ausgabedatei schließen
                        fOut.Close()

                        'Aus der VehNr-Such-Schleife raus springen
                        Exit Do

                    End If

                End If

            Loop

            'ProgBar
            x += 1
            ProgBarCtrl.ProgJobInt = CInt(100 * x / x1)

        Next

        WorkerMsg(tMsgID.Normal, "done", MsgSrc)

lbDone:

        f.Close()
        f.Dispose()
        f = Nothing
        fOut.Close()
        fOut = Nothing

    End Sub

    Private Function GetVehNr(ByVal Str As String) As Integer
        Dim foundStr As String
        foundStr = Str.Split(",")(0)
        Return CInt(Right(foundStr, Len(foundStr) - 7))
    End Function


    'FLT Klasse
    Private Class cFLTfleet

        Private MyP() As Single             'Percentage
        Private MyGen() As String           'GEN Datei
        Private MyDim As Int16              'Dim
        Private MyID As String              'Vehicle Type

        Public VehCount As Integer             'Anzahl Fahrzeuge

        'New
        Public Sub New(ByVal ID As String)
            MyID = ID
            MyDim = -1
        End Sub

        'Initialisieren
        Public Function Init(ByVal GenFiles As String(), ByVal Shares As Single(), ByVal ArDim As Int16) As Boolean
            Dim x As Int16
            Dim xsum As Single
            Dim MsgSrc As String

            MsgSrc = "ADV/FLT/Init"

            MyDim = ArDim

            If MyDim > -1 Then
                ReDim MyGen(ArDim)
                ReDim MyP(ArDim)
                xsum = 0
                For x = 0 To ArDim
                    MyGen(x) = GenFiles(x)
                    MyP(x) = Shares(x)
                    xsum += Shares(x)
                Next
                If Math.Abs(1 - xsum) > 0.0001 Then
                    WorkerMsg(tMsgID.Err, "Vehicle Type " & MyID & ": Share sum is not 100%!", MsgSrc)
                    Return False
                End If

                For x = 1 To ArDim
                    MyP(x) += MyP(x - 1)
                Next
            End If

            Return True

        End Function

        'GenDatei Zufallsgenerator
        Public Function GetGenFile() As String
            Dim r As Single
            Dim x As Int16
            Dim MsgSrc As String

            MsgSrc = "ADV/FLT/GetGenFile"

            Select Case MyDim
                Case -1
                    WorkerMsg(tMsgID.Err, "Vehicle type " & MyID & " is not specified in .FLT file. Cannot assign .GEN file.", MsgSrc)
                    Return ""
                Case 0
                    Return MyGen(0)
                Case Else
                    r = 1 - Rnd()
                    x = MyDim
                    Do While r <= MyP(x - 1)
                        x -= 1
                        If x = 0 Then Exit Do
                    Loop
                    Return MyGen(x)
            End Select
        End Function

        Public Function FileCheck() As Boolean
            Dim str As String
            Dim check As Boolean
            Dim MsgSrc As String

            MsgSrc = "ADV/FLT/" & MyID

            check = True

            For Each str In MyGen
                If str = "" OrElse Not IO.File.Exists(str) Then
                    WorkerMsg(tMsgID.Err, "File not found (" & str & ")!", MsgSrc)
                    check = False
                End If
            Next

            Return check

        End Function




    End Class

    'Klasse für abschnittsweise Auswertung
    Private Class cSTRerg

        Public MySTRpath As String

        Public VehCount As Dictionary(Of String, Integer)           'Fahrzeuganzahl je Fahrzeugtyp
        Public VehDist As Dictionary(Of String, Double)             'Verkehrsleistung je Fahrzeugtyp
        Public VehTime As Dictionary(Of String, Double)             'Reisezeit je Fahrzeugtyp

        Public EMsum As Dictionary(Of String, List(Of Double))      'Summen-Emissionen je Fahrzeugtyp und Em-Komponente Absolut [g]

        Public Distance As Single                                   'STR-Länge für Filter in km

        Public Sub Init()
            Dim KV As KeyValuePair(Of String, cFLTfleet)
            Dim dic0 As List(Of Double)
            Dim x As Integer

            VehCount = New Dictionary(Of String, Integer)
            VehDist = New Dictionary(Of String, Double)
            VehTime = New Dictionary(Of String, Double)

            EMsum = New Dictionary(Of String, List(Of Double))

            For Each KV In ADV.FLTfleet
                VehCount.Add(KV.Key, 0)
                VehDist.Add(KV.Key, 0)
                VehTime.Add(KV.Key, 0)

                dic0 = New List(Of Double)

                For x = 0 To ADV.EMdim
                    dic0.Add(0)
                Next

                EMsum.Add(KV.Key, dic0)

            Next

        End Sub

        Public Function Ausg() As Boolean
            Dim s As System.Text.StringBuilder
            Dim Sepp As String = ","
            Dim VehType As String
            Dim km As Double
            Dim KVl As KeyValuePair(Of String, List(Of Double))
            Dim sum As Double
            Dim e As Integer
            Dim fSSUM As cFile_V3
            Dim STRSUMpath As String
            Dim MsgSrc As String

            MsgSrc = "ADV/Outp/Seg"

            fSSUM = New cFile_V3

            STRSUMpath = fFileWoExt(JobFile) & "_" & fFILE(MySTRpath, False) & ".sum"

            If Not fSSUM.OpenWrite(STRSUMpath) Then
                WorkerMsg(tMsgID.Err, "Failed to write to '" & STRSUMpath & "'!", MsgSrc)
                Return False
            End If

            s = New System.Text.StringBuilder

            'Datei-Header
            fSSUM.WriteLine("VECTO ADVANCE Sum Results per Segment")
            fSSUM.WriteLine("VECTO " & VECTOvers)
            fSSUM.WriteLine("Inputfile: " & JobFile)
            fSSUM.WriteLine("Segment File: " & MySTRpath)
            fSSUM.WriteLine("Calculated Segment Length: " & Distance * 1000 & "m")
            fSSUM.WriteLine("Minimum Travelled Distance: " & 1000 * ADV.STRdistTol * Distance & "m (" & ADV.STRdistTol * 100 & "%)")

            fSSUM.WriteLine(" ")

            '****************************************************************
            '*************************** Em je km ***************************

            'Header
            s.Length = 0
            s.Append("*** Results per km  ***,Vehicles,Travel Time,Travelled Distance, Average Speed")
            If GEN.VehMode = tVehMode.EV Then
                s.Append(Sepp & "EC")
            Else
                For e = 0 To ADV.EMdim
                    s.Append(Sepp & MODdata.Em.EmComp(ADV.EMlist(e)).Name)
                Next
            End If
            fSSUM.WriteLine(s.ToString)

            'Untits
            s.Length = 0
            s.Append("Vehicle Type,[-],[h],[km],[km/h]")
            If GEN.VehMode = tVehMode.EV Then
                s.Append(Sepp & "[kWh/km]")
            Else
                s.Append(Sepp & "[g/km]")
                s.Append(Sepp & "[g/km]")
                s.Append(Sepp & "[g/km]")
                s.Append(Sepp & "[g/km]")
                s.Append(Sepp & "[g/km]")
                s.Append(Sepp & "[#/km]")
                s.Append(Sepp & "[g/km]")
            End If
            fSSUM.WriteLine(s.ToString)

            '** Ergebnisse je Veh-Type
            For Each VehType In ADV.FLTfleet.Keys

                s.Length = 0

                'Type
                s.Append(VehType)

                'Anzahl Fahrzeuge
                s.Append(Sepp & VehCount(VehType))

                'Reisezeit
                sum = VehTime(VehType)
                s.Append(Sepp & sum)

                'km
                km = VehDist(VehType)
                s.Append(Sepp & km)

                'Speed
                If sum = 0 Then
                    s.Append(Sepp & "-")
                Else
                    s.Append(Sepp & km / sum)
                End If

                'Em
                For e = 0 To ADV.EMdim
                    If km = 0 Then
                        s.Append(Sepp & "-")
                    Else
                        s.Append(Sepp & EMsum(VehType)(e) / km)
                    End If
                Next

                'Schreiben
                fSSUM.WriteLine(s.ToString)

            Next

            '** Summe
            s.Length = 0

            'Type
            s.Append("Sum")

            'Anzahl Fahrzeuge
            s.Append(Sepp & CStr(AllVehCount()))

            'Reisezeit
            sum = AllVehTime()
            s.Append(Sepp & CStr(sum))

            'km
            km = AllVehDist()
            s.Append(Sepp & km)

            'Speed
            If sum = 0 Then
                s.Append(Sepp & "-")
            Else
                s.Append(Sepp & km / sum)
            End If

            'Em
            For e = 0 To ADV.EMdim
                If km = 0 Then
                    s.Append(Sepp & "-")
                Else
                    sum = 0
                    For Each KVl In EMsum
                        sum += KVl.Value(e)
                    Next
                    s.Append(Sepp & sum / km)
                End If
            Next

            'Schreiben
            fSSUM.WriteLine(s.ToString)

            fSSUM.WriteLine(" ")

            '****************************************************************
            '*************************** Em absolut *************************

            'Header
            s.Length = 0
            s.Append("*** Absolute Values ***")
            If GEN.VehMode = tVehMode.EV Then
                s.Append(Sepp & "EC")
            Else
                For e = 0 To ADV.EMdim
                    s.Append(Sepp & MODdata.Em.EmComp(ADV.EMlist(e)).Name)
                Next
            End If
            fSSUM.WriteLine(s.ToString)

            'Untits
            s.Length = 0
            s.Append("Vehicle Type")
            If GEN.VehMode = tVehMode.EV Then
                s.Append(Sepp & "[kWh]")
            Else
                s.Append(Sepp & "[g]")
                s.Append(Sepp & "[g]")
                s.Append(Sepp & "[g]")
                s.Append(Sepp & "[g]")
                s.Append(Sepp & "[g]")
                s.Append(Sepp & "[#]")
                s.Append(Sepp & "[g]")
            End If
            fSSUM.WriteLine(s.ToString)

            '** Ergebnisse je Veh-Type
            For Each VehType In ADV.FLTfleet.Keys

                s.Length = 0

                'Type
                s.Append(VehType)

                'Em
                For e = 0 To ADV.EMdim
                    s.Append(Sepp & EMsum(VehType)(e))
                Next

                'Schreiben
                fSSUM.WriteLine(s.ToString)

            Next

            '** Summe
            s.Length = 0

            'Type
            s.Append("Sum")

            'Em
            For e = 0 To ADV.EMdim
                sum = 0
                For Each KVl In EMsum
                    sum += KVl.Value(e)
                Next
                s.Append(Sepp & sum)
            Next

            'Schreiben
            fSSUM.WriteLine(s.ToString)

            'Datei schließen
            fSSUM.Close()

            Return True


        End Function

        Public Function AllVehCount() As Integer
            Dim sum As Integer
            Dim KV As KeyValuePair(Of String, Integer)
            sum = 0
            For Each KV In VehCount
                sum += KV.Value
            Next
            Return sum
        End Function

        Public Function AllVehDist() As Double
            Dim sum As Double
            Dim KV As KeyValuePair(Of String, Double)
            sum = 0
            For Each KV In VehDist
                sum += KV.Value
            Next
            Return sum
        End Function

        Public Function AllVehTime() As Double
            Dim sum As Double
            Dim KV As KeyValuePair(Of String, Double)
            sum = 0
            For Each KV In VehTime
                sum += KV.Value
            Next
            Return sum
        End Function

        Public Function AllEm(ByVal EmKompNr As String) As Double
            Dim sum As Double
            Dim KV As KeyValuePair(Of String, List(Of Double))
            sum = 0
            For Each KV In EMsum
                sum += KV.Value(EmKompNr)
            Next
            Return sum
        End Function



    End Class




End Class

