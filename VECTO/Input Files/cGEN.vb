Imports System.Collections.Generic

Public Class cGEN

    Private Const FormatVersion As String = "1.0"
    Private FileVersion As String

    Private sFilePath As String

    Private MyPath As String

    'Mode
    Public VehMode As tVehMode
    Public EngAnalysis As Boolean
    Public CreateMap As Boolean
    Public ModeHorEV As Boolean

    Private boPKWja As Boolean
    Private bodynkorja As Boolean
    Private ineklasse As Int16
    Private inizykwael As Int16
    Private inPschrit As Int16
    Private innschrit As Int16
    Private boMapSchaltja As Boolean
    Private iniMsek As Int16
    Private boottoJa As Boolean
    Private bokaltst1 As Boolean
    Private sitkat1 As Single
    Private sitkw1 As Single
    Private sihsstart As Single

    Private stPathVEH As cSubPath
    Private stPathENG As cSubPath
    Private stPathGBX As cSubPath
    Private stdynspez As cSubPath

    Private stkatmap As cSubPath
    Private stkwmap As cSubPath
    Private stkatkurv As cSubPath
    Private stkwkurv As cSubPath
    Private stcooldown As cSubPath
    Private sttumgebung As cSubPath

    Private stBatfile As cSubPath
    Private stEmospez As cSubPath
    Private stEANfile As cSubPath
    Private stGetspez As cSubPath
    Private stSTEnam As cSubPath
    Private stEKFnam As cSubPath

    Private stPathExs As cSubPath

    Private boEXSja As Boolean
    Private boStartStop As Boolean
    Private siStStV As Single
    Private siStStT As Single
    Public StStDelay As Integer
    Private boSOCnJa As Boolean
    Private siSOCstart As Single
    Private GetrMod As tTransLossModel

    Private bKFcutFull As Boolean
    Private bKFcutDrag As Boolean
    Private bKFinsertDrag As Boolean
    Private bKFDragIntp As Boolean

    Public CoolantsimJa As Boolean
    Private stCoolantSimPath As cSubPath

    Public DesMaxJa As Boolean
    Private stDesMaxFile As cSubPath
    Private laDesV As List(Of Single)
    Private laDesMax As List(Of Single)
    Private laDesMin As List(Of Single)
    Private DesMaxDim As Integer

    Public AuxPaths As Dictionary(Of String, cVEH.cAuxEntry)
    Public AuxDef As Boolean                               'True wenn ein oder mehrere Nebenverbraucher definiert sind

    Public hinauf As Single
    Public hinunter As Single
    Public lhinauf As Single
    Public lhinunter As Single
    Public pspar As Single
    Public pmodell As Single

    Public CycleFiles As List(Of cSubPath)

    Public EngOnly As Boolean

    Public a_lookahead As Single
    Public vMin As Single
    Public vMinLA As Single
    Public LookAheadOn As Boolean
    Public OverSpeedOn As Boolean
    Public OverSpeed As Single
    Public UnderSpeed As Single
    Public EcoRollOn As Boolean

    Public NoJSON As Boolean

    Private MyFileList As List(Of String)

    Public Function CreateFileList() As Boolean
        Dim Aux0 As cVEH.cAuxEntry
        Dim sb As cSubPath
        Dim VEH0 As cVEH
        Dim ENG0 As cENG
        Dim GBX0 As cGBX
        Dim str As String

        If Not Me.ReadFile Then Return False

        MyFileList = New List(Of String)

        '.vecto
        MyFileList.Add(Me.sFilePath)

        'Veh
        If Not Me.EngOnly Then
            MyFileList.Add(Me.PathVEH)

            VEH0 = New cVEH
            VEH0.FilePath = Me.PathVEH
            If Not VEH0.CreateFileList Then Return False
            For Each str In VEH0.FileList
                MyFileList.Add(str)
            Next
        End If

        'Eng
        MyFileList.Add(Me.PathENG)

        ENG0 = New cENG
        ENG0.FilePath = Me.PathENG
        If Not ENG0.CreateFileList Then Return False
        For Each str In ENG0.FileList
            MyFileList.Add(str)
        Next

        If Not Me.EngOnly Then

            'Gbx
            MyFileList.Add(Me.PathGBX)

            GBX0 = New cGBX
            GBX0.FilePath = Me.PathGBX
            If Not GBX0.CreateFileList Then Return False
            For Each str In GBX0.FileList
                MyFileList.Add(str)
            Next

            'Aux
            If AuxDef Then
                For Each Aux0 In Me.AuxPaths.Values
                    MyFileList.Add(Aux0.Path.FullPath)
                Next
            End If

            '.vacc
            MyFileList.Add(Me.stDesMaxFile.FullPath)

        End If

        'Cycles
        For Each sb In Me.CycleFiles
            MyFileList.Add(sb.FullPath)
        Next


        Return True

    End Function

    Public Sub New()

        MyPath = ""
        sFilePath = ""

        stPathVEH = New cSubPath
        stPathENG = New cSubPath
        stPathGBX = New cSubPath
        stdynspez = New cSubPath

        stkatmap = New cSubPath
        stkwmap = New cSubPath
        stkatkurv = New cSubPath
        stkwkurv = New cSubPath
        stcooldown = New cSubPath
        sttumgebung = New cSubPath

        stBatfile = New cSubPath
        stEmospez = New cSubPath
        stEANfile = New cSubPath
        stGetspez = New cSubPath
        stSTEnam = New cSubPath
        stEKFnam = New cSubPath

        stPathExs = New cSubPath

        stCoolantSimPath = New cSubPath

        stDesMaxFile = New cSubPath

        laDesV = New List(Of Single)
        laDesMax = New List(Of Single)
        laDesMin = New List(Of Single)

        AuxPaths = New Dictionary(Of String, cVEH.cAuxEntry)
        AuxDef = False

        CycleFiles = New List(Of cSubPath)

    End Sub

    Private Function ReadFileOld() As Boolean
        Dim file As cFile_V3
        Dim line As String()
        'Dim txt As String
        Dim AuxEntry As cVEH.cAuxEntry
        Dim AuxID As String
        Dim MsgSrc As String
        Dim SubPath As cSubPath

        MsgSrc = "Main/ReadInp/GEN"

        If sFilePath = "" Then Return False
        If Not IO.File.Exists(sFilePath) Then Return False

        file = New cFile_V3

        SetDefault()

        If Not file.OpenRead(sFilePath) Then
            file = Nothing
            Return False
        End If

        ''***
        ''*** First line: Version
        'line = file.ReadLine
        'txt = Trim(UCase(line(0)))
        'If Microsoft.VisualBasic.Left(txt, 1) = "V" Then
        '    ' "V" entfernen => Zahl bleibt übrig
        '    txt = txt.Replace("V", "")
        '    If Not IsNumeric(txt) Then
        '        'Falls Version ungültig: Abbruch
        '        GoTo lbEr
        '    Else
        '        'Version festgelegt
        '        FileVersion = CInt(txt)
        '    End If
        'Else
        '    file.Close()
        '    Return ReadOldFormat()
        'End If

        ''Version Check: abort if Input-file's format is newer than PHEM-version
        'If FileVersion > FormatVersion Then
        '    WorkerMsg(tMsgID.Err, "File Version not supported!", MsgSrc)
        '    GoTo lbEr
        'End If

        '**** GEN Datei einlesen **** |@@| Read GEN file ****

        'Allgemein |@@| Common
        'boPKWja = CBool(file.ReadLine(0))
        'bodynkorja = CBool(file.ReadLine(0))
        'ineklasse = CShort(file.ReadLine(0))
        'inizykwael = CShort(file.ReadLine(0))

        'line = file.ReadLine
        'If UBound(line) < 2 Then
        '    WorkerMsg(tMsgID.Err, "File Format invalid (" & sFilePath & ")!", MsgSrc)
        '    GoTo lbEr
        'End If

        'Select Case CShort(line(0))
        '    Case 0
        '        VehMode = tVehMode.StandardMode
        '    Case 1
        '        VehMode = tVehMode.EngineOnly
        '    Case 2
        '        VehMode = tVehMode.HEV
        '    Case Else '3    
        '        VehMode = tVehMode.EV
        'End Select

        'EngAnalysis = CBool(line(1))
        'CreateMap = CBool(line(2))
        'ModeHorEV = (VehMode = tVehMode.HEV Or VehMode = tVehMode.EV)


        'KF creation
        'line = file.ReadLine
        'inPschrit = CShort(line(0))
        'innschrit = CShort(line(1))

        'line = file.ReadLine
        'bKFcutFull = CBool(line(0))
        'bKFcutDrag = CBool(line(1))
        'bKFinsertDrag = CBool(line(2))
        'bKFDragIntp = CBool(line(3))

        'boMapSchaltja = CBool(file.ReadLine(0))

        'iniMsek = CShort(file.ReadLine(0))

        'boottoJa = CBool(file.ReadLine(0))

        'bokaltst1 = CBool(file.ReadLine(0))

        'sitkat1 = CSng(file.ReadLine(0))
        'sitkw1 = CSng(file.ReadLine(0))
        'sihsstart = CSng(file.ReadLine(0))

        stPathVEH.Init(MyPath, file.ReadLine(0))

        stPathENG.Init(MyPath, file.ReadLine(0))

        stPathGBX.Init(MyPath, file.ReadLine(0))

        Do While Not file.EndOfFile

            line = file.ReadLine

            If line(0) = sKey.Break Then Exit Do

            SubPath = New cSubPath

            SubPath.Init(MyPath, line(0))

            CycleFiles.Add(SubPath)

        Loop

        'stdynspez.Init(MyPath, file.ReadLine(0))

        'Cold start
        'stkatmap.Init(MyPath, file.ReadLine(0))
        'stkwmap.Init(MyPath, file.ReadLine(0))
        'stkatkurv.Init(MyPath, file.ReadLine(0))
        'stkwkurv.Init(MyPath, file.ReadLine(0))
        'stcooldown.Init(MyPath, file.ReadLine(0))
        'sttumgebung.Init(MyPath, file.ReadLine(0))

        'If file.EndOfFile Then GoTo lbClose

        'HEV
        'stBatfile.Init(MyPath, file.ReadLine(0))
        'stEmospez.Init(MyPath, file.ReadLine(0))
        'stEANfile.Init(MyPath, file.ReadLine(0))
        'stGetspez.Init(MyPath, file.ReadLine(0))
        'stSTEnam.Init(MyPath, file.ReadLine(0))
        'stEKFnam.Init(MyPath, file.ReadLine(0))

        'If file.EndOfFile Then GoTo lbClose

        'EXS
        'boEXSja = CBool(file.ReadLine(0))
        'stPathExs.Init(MyPath, file.ReadLine(0))

        'boSOCnJa = CBool(file.ReadLine(0))
        'siSOCstart = CSng(file.ReadLine(0))

        'If file.EndOfFile Then GoTo lbClose

        'GetrMod = CShort(file.ReadLine(0))

        'If file.EndOfFile Then GoTo lbClose

        'CoolantsimJa = CBool(file.ReadLine(0))
        'stCoolantSimPath.Init(MyPath, file.ReadLine(0))

        'If file.EndOfFile Then GoTo lbClose

        'Einzelne Nebenverbraucher |@@| Individual next consumer
        Do While Not file.EndOfFile

            line = file.ReadLine

            If line(0) = sKey.Break Then Exit Do

            AuxID = UCase(Trim(line(0)))

            If AuxPaths.ContainsKey(AuxID) Then
                WorkerMsg(tMsgID.Err, "Multiple definitions of the same auxiliary type (" & line(0) & ")!", MsgSrc)
                file.Close()
                Return False
            End If

            AuxEntry = New cVEH.cAuxEntry

            AuxEntry.Type = line(1)
            AuxEntry.Path.Init(MyPath, line(2))

            AuxPaths.Add(AuxID, AuxEntry)

            AuxDef = True

        Loop

        'DesMaxJa = CBool(file.ReadLine(0))
        stDesMaxFile.Init(MyPath, file.ReadLine(0))

        'hinauf = CSng(file.ReadLine(0))
        'hinunter = CSng(file.ReadLine(0))
        'lhinauf = CSng(file.ReadLine(0))
        'lhinunter = CSng(file.ReadLine(0))
        'pspar = CSng(file.ReadLine(0))
        'pmodell = CSng(file.ReadLine(0))

        ''Schaltmodell-Verteilung |@@| Gear-shifting Model Distribution
        'If (pspar > 1) Then
        '    pspar = 1
        'ElseIf (pspar < 0) Then
        '    pspar = 0
        'End If
        'If (pmodell > 1) Then
        '    pmodell = 1
        'ElseIf (pmodell < 0) Then
        '    pmodell = 0
        'End If
        'If ((pspar + pmodell) > 1.0) Then pmodell = 1.0 - pspar


        If Not file.EndOfFile Then EngOnly = CBool(file.ReadLine(0))

        If EngOnly Then
            VehMode = tVehMode.EngineOnly
        Else
            VehMode = tVehMode.StandardMode
        End If

        If file.EndOfFile Then GoTo lbClose

        boStartStop = CBool(file.ReadLine(0))
        siStStV = CSng(file.ReadLine(0))
        siStStT = CSng(file.ReadLine(0))

        If file.EndOfFile Then GoTo lbClose

        a_lookahead = CSng(file.ReadLine(0))
        vMin = CSng(file.ReadLine(0))
        LookAheadOn = CBool(file.ReadLine(0))
        OverSpeedOn = CBool(file.ReadLine(0))
        EcoRollOn = CBool(file.ReadLine(0))
        OverSpeed = CSng(file.ReadLine(0))
        UnderSpeed = CSng(file.ReadLine(0))
        vMinLA = CSng(file.ReadLine(0))

        If file.EndOfFile Then GoTo lbClose

        StStDelay = CSng(file.ReadLine(0))

lbClose:

        file.Close()
        file = Nothing

        Return True


        'ERROR-label for clean Abort
lbEr:
        file.Close()
        file = Nothing

        Return False


    End Function

    Private Function SaveFileOld() As Boolean
        Dim fGEN As New cFile_V3
        Dim AuxEntryKV As KeyValuePair(Of String, cVEH.cAuxEntry)
        'Dim s As String
        Dim sb As cSubPath

        If Not fGEN.OpenWrite(sFilePath) Then Return False

        'fGEN.WriteLine("V" & FormatVersion)

        fGEN.WriteLine("c VECTO Input File")
        fGEN.WriteLine("c VECTO " & VECTOvers)
        fGEN.WriteLine("c " & Now.ToString)

        'fGEN.WriteLine("c Heavy Duty (0) or Passenger Car (1)")
        'fGEN.WriteLine(Math.Abs(CInt(boPKWja)))

        'fGEN.WriteLine("c Transient emission correction (1/0)")
        'fGEN.WriteLine(Math.Abs(CInt(bodynkorja)))

        'fGEN.WriteLine("c Emission Class (EURO ..)")
        'fGEN.WriteLine(ineklasse)

        'fGEN.WriteLine("c Gear Shift Mode: NEDC (0), FTP (1), Model - MT (2)")
        'fGEN.WriteLine(inizykwael)

        'fGEN.WriteLine("c Calculation Mode, EngAnalysis, CreateMap")
        'Select Case VehMode
        '    Case tVehMode.StandardMode
        '        s = "0"
        '    Case tVehMode.EngineOnly
        '        s = "1"
        '    Case tVehMode.HEV
        '        s = "2"
        '    Case Else   'tVehMode.EV
        '        s = "3"
        'End Select
        's &= "," & Math.Abs(CInt(EngAnalysis))
        's &= "," & Math.Abs(CInt(CreateMap))
        'fGEN.WriteLine(s)

        'Map creation ------------------------------------------------ ------
        'fGEN.WriteLine("c Settings for Emission Map Creation Mode:")
        'fGEN.WriteLine("c Increment Pe, n:")
        'fGEN.WriteLine(inPschrit & "," & innschrit)

        'fGEN.WriteLine("c CutFull,CutDrag,InsertDrag,DragIntp:")
        'fGEN.WriteLine(Math.Abs(CInt(bKFcutFull)) & "," & Math.Abs(CInt(bKFcutDrag)) & "," & Math.Abs(CInt(bKFinsertDrag)) & "," & Math.Abs(CInt(bKFDragIntp)))

        'fGEN.WriteLine("c Include Gear Shifts (1/0, Standard = 1)")
        'fGEN.WriteLine(Math.Abs(CInt(boMapSchaltja)))

        'fGEN.WriteLine("c Averageing Period for Modal Values")
        'fGEN.WriteLine(iniMsek)

        'fGEN.WriteLine("c ICE Type (Otto = 1, Diesel = 0")
        'fGEN.WriteLine(Math.Abs(CInt(boottoJa)))

        'Kalt Start---------------------------------------------------------------
        'fGEN.WriteLine("c Cold Start (1/0)")
        'fGEN.WriteLine(Math.Abs(CInt(bokaltst1)))

        'fGEN.WriteLine("c t cat start [°C]")
        'fGEN.WriteLine(sitkat1)

        'fGEN.WriteLine("c t coolant start [°C]")
        'fGEN.WriteLine(sitkw1)

        'fGEN.WriteLine("c time of start [h.sec]")
        'fGEN.WriteLine(sihsstart)

        'Dateien------------------------------------------------------------------
        fGEN.WriteLine("c Vehicle (.vveh):")
        fGEN.WriteLine(stPathVEH.PathOrDummy)

        fGEN.WriteLine("c Engine (.veng):")
        fGEN.WriteLine(stPathENG.PathOrDummy)

        fGEN.WriteLine("c Gearbox (*.vgbx):")
        fGEN.WriteLine(stPathGBX.PathOrDummy)

        fGEN.WriteLine("c Driving Cycles (.vdri):")
        For Each sb In CycleFiles
            fGEN.WriteLine(sb.PathOrDummy)
        Next
        fGEN.WriteLine(sKey.Break)

        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing transient correction parameters (*.trs):")
        'fGEN.WriteLine(stdynspez.PathOrDummy)

        'Kalt Start
        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing the catalyst map (*.maa):")
        'fGEN.WriteLine(stkatmap.PathOrDummy)

        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing the map of cooling water (*.mac):")
        'fGEN.WriteLine(stkwmap.PathOrDummy)

        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing the catalyst warm-up (*.wua):")
        'fGEN.WriteLine(stkatkurv.PathOrDummy)

        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing the engine coolant warm-up (*.wuc):")
        'fGEN.WriteLine(stkwkurv.PathOrDummy)

        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing the cooling parameters for catalyst and engine coolant (*.cdw):")
        'fGEN.WriteLine(stcooldown.PathOrDummy)

        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing the ambient parameters (*.atc)")
        'fGEN.WriteLine(sttumgebung.PathOrDummy)

        'HEV
        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing battery specifications for HEV (*.bat)")
        'fGEN.WriteLine(stBatfile.PathOrDummy)

        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing specifications of the E-motor for HEV (*emo)")
        'fGEN.WriteLine(stEmospez.PathOrDummy)

        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing the pattern of E-motor on/off for HEV  (*ean)")
        'fGEN.WriteLine(stEANfile.PathOrDummy)

        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing the efficiency of additional gearbox for HEV (*get)")
        'fGEN.WriteLine(stGetspez.PathOrDummy)

        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing the control efficiency-File for HEV (*.ste)")
        'fGEN.WriteLine(stSTEnam.PathOrDummy)

        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c File containing the efficiency-maps for HEV-strategy control (*.ekf)")
        'fGEN.WriteLine(stEKFnam.PathOrDummy)

        'EXS
        'fGEN.WriteLine("c ")
        'fGEN.WriteLine("c Exhaust System Simulation (1/0)")
        'fGEN.WriteLine(Math.Abs(CInt(boEXSja)))

        'fGEN.WriteLine("c Exhaust System Simulation Configuration File")
        'fGEN.WriteLine(stPathExs.PathOrDummy)

        'SOC-Start Iteration
        'fGEN.WriteLine("c SOC Start Iteration (1/0) - HEV only")
        'fGEN.WriteLine(Math.Abs(CInt(boSOCnJa)))

        ''SOC-Start
        'fGEN.WriteLine("c SOC Start - (H)EV only")
        'fGEN.WriteLine(siSOCstart)

        ''Getriebe-Verluste-Modell
        'fGEN.WriteLine("c Transmission Loss Model")
        'fGEN.WriteLine(CStr(GetrMod))

        'Coolantsim
        'fGEN.WriteLine("c Coolant System Simulation (1/0)")
        'fGEN.WriteLine(Math.Abs(CInt(CoolantsimJa)))
        'fGEN.WriteLine("c Coolant System Simulation Configuration File")
        'fGEN.WriteLine(stCoolantSimPath.PathOrDummy)

        fGEN.WriteLine("c Auxiliaries (.vaux)")
        For Each AuxEntryKV In AuxPaths
            fGEN.WriteLine(Trim(UCase(AuxEntryKV.Key)) & "," & AuxEntryKV.Value.Type & "," & AuxEntryKV.Value.Path.PathOrDummy)
        Next
        fGEN.WriteLine(sKey.Break)

        'a_DesMax
        fGEN.WriteLine("c Speed Dependent Desired Acceleration and Braking (.vacc)")
        'fGEN.WriteLine(Math.Abs(CInt(DesMaxJa)))
        fGEN.WriteLine(stDesMaxFile.PathOrDummy)

        'fGEN.WriteLine("c Gear shift behaviour:")
        'fGEN.WriteLine("c Gearshift model (Version fast driver)")
        'fGEN.WriteLine("c shift up at ratio rpm/rated rpm in actual gear greater than")
        'fGEN.WriteLine(CStr(hinauf))
        'fGEN.WriteLine("c shift down when rpm/rated rpm in lower gear is higher than")
        'fGEN.WriteLine(CStr(hinunter))
        'fGEN.WriteLine("c Gearshift model (Version economic driver)")
        'fGEN.WriteLine("c shift up at ratio rpm/rated rpm in higher gear greater than")
        'fGEN.WriteLine(CStr(lhinauf))
        'fGEN.WriteLine("c Shift down when ratio rpm/rated rpm in actual gear is lower than")
        'fGEN.WriteLine(CStr(lhinunter))
        'fGEN.WriteLine("c Share of version economic driver (0 to 1)")
        'fGEN.WriteLine(CStr(pspar))
        'fGEN.WriteLine("c Share of version mixed model (0 to 1)")
        'fGEN.WriteLine(CStr(pmodell))

        fGEN.WriteLine("c Engine Only Mode (1/0)")
        fGEN.WriteLine(CStr(Math.Abs(CInt(EngOnly))))

        'Start/Stop
        fGEN.WriteLine("c ICE Auto-Start/Stop (1/0) - Non HEV only")
        fGEN.WriteLine(Math.Abs(CInt(boStartStop)))
        fGEN.WriteLine("c Start/Stop Max Speed [km/h]")
        fGEN.WriteLine(siStStV)
        fGEN.WriteLine("c Start/Stop Min ICE-On Time [s]")
        fGEN.WriteLine(siStStT)


        fGEN.WriteLine("c Look Ahead reference deceleration [m/2²]")
        fGEN.WriteLine(CStr(a_lookahead))
        fGEN.WriteLine("c Minimum target speed for Overspeed/Eco-Roll [km/h]")
        fGEN.WriteLine(CStr(vMin))
        fGEN.WriteLine("c Look-Ahead with Coasting 1/0")
        fGEN.WriteLine(CStr(Math.Abs(CInt(LookAheadOn))))
        fGEN.WriteLine("c Overspeed 1/0")
        fGEN.WriteLine(CStr(Math.Abs(CInt(OverSpeedOn))))
        fGEN.WriteLine("c Eco-Roll 1/0")
        fGEN.WriteLine(CStr(Math.Abs(CInt(EcoRollOn))))
        fGEN.WriteLine("c Allowed OverSpeed [km/h]")
        fGEN.WriteLine(CStr(OverSpeed))
        fGEN.WriteLine("c Allowed UnderSpeed [km/h]")
        fGEN.WriteLine(CStr(UnderSpeed))
        fGEN.WriteLine("c Minimum target speed for Look-Ahead with Coasting [km/h]")
        fGEN.WriteLine(CStr(vMinLA))


        fGEN.WriteLine("c Start/Stop activation delay time [s]")
        fGEN.WriteLine(StStDelay)


        fGEN.Close()
        fGEN = Nothing

        Return True

    End Function

    Public Function SaveFile() As Boolean
        Dim AuxEntryKV As KeyValuePair(Of String, cVEH.cAuxEntry)
        'Dim s As String
        Dim sb As cSubPath
        Dim JSON As New cJSON
        Dim ls As List(Of Object)
        Dim dic As Dictionary(Of String, Object)
        Dim dic0 As Dictionary(Of String, Object)

        If Not Cfg.JSON Then Return SaveFileOld()

        'Header
        dic = New Dictionary(Of String, Object)
        dic.Add("CreatedBy", Lic.LicString & " (" & Lic.GUID & ")")
        dic.Add("Date", Now.ToString)
        dic.Add("AppVersion", VECTOvers)
        dic.Add("FileVersion", FormatVersion)
        JSON.Content.Add("Header", dic)

        'Body
        dic0 = New Dictionary(Of String, Object)

        'Main Files
        dic0.Add("VehicleFile", stPathVEH.PathOrDummy)
        dic0.Add("EngineFile", stPathENG.PathOrDummy)
        dic0.Add("GearboxFile", stPathGBX.PathOrDummy)

        'Cycles
        If CycleFiles.Count > 0 Then
            ls = New List(Of Object)
            For Each sb In CycleFiles
                ls.Add(sb.PathOrDummy)
            Next
            dic0.Add("Cycles", ls)
        End If

        'Aux
        If AuxPaths.Count > 0 Then
            ls = New List(Of Object)
            For Each AuxEntryKV In AuxPaths
                dic = New Dictionary(Of String, Object)
                dic.Add("ID", Trim(UCase(AuxEntryKV.Key)))
                dic.Add("Type", AuxEntryKV.Value.Type)
                dic.Add("Path", AuxEntryKV.Value.Path.PathOrDummy)
                ls.Add(dic)
            Next
            dic0.Add("Aux", ls)
        End If

        'VACC
        dic0.Add("VACC", stDesMaxFile.PathOrDummy)

        'EngineOnlyMode
        dic0.Add("EngineOnlyMode", EngOnly)

        'Start Stop
        dic = New Dictionary(Of String, Object)
        dic.Add("Enabled", boStartStop)
        dic.Add("MaxSpeed", siStStV)
        dic.Add("MinTime", siStStT)
        dic.Add("Delay", StStDelay)
        dic0.Add("StartStop", dic)

        'LAC
        dic = New Dictionary(Of String, Object)
        dic.Add("Enabled", LookAheadOn)
        dic.Add("Dec", a_lookahead)
        dic.Add("MinSpeed", vMinLA)
        dic0.Add("LAC", dic)

        'Overspeed / EcoRoll
        dic = New Dictionary(Of String, Object)
        If EcoRollOn Then
            dic.Add("Mode", "EcoRoll")
        ElseIf OverSpeedOn Then
            dic.Add("Mode", "OverSpeed")
        Else
            dic.Add("Mode", "Off")
        End If
        dic.Add("MinSpeed", vMin)
        dic.Add("OverSpeed", OverSpeed)
        dic.Add("UnderSpeed", UnderSpeed)
        dic0.Add("OverSpeedEcoRoll", dic)


        JSON.Content.Add("Body", dic0)

        Return JSON.WriteFile(sFilePath)

    End Function

    Public Function ReadFile() As Boolean
        Dim AuxEntry As cVEH.cAuxEntry
        Dim AuxID As String
        Dim MsgSrc As String
        Dim SubPath As cSubPath
        Dim JSON As New cJSON
        Dim str As String
        Dim dic As Object

        MsgSrc = "Main/ReadInp/GEN"

        'Flag for "File is not JSON" Warnings        
        NoJSON = False

        SetDefault()

        If Cfg.JSON Then
            If Not JSON.ReadFile(sFilePath) Then
                NoJSON = True
                Try
                    Return ReadFileOld()
                Catch ex As Exception
                    Return False
                End Try
            End If
        Else
            Try
                Return ReadFileOld()
            Catch ex As Exception
                Return False
            End Try
        End If

        Try

            FileVersion = JSON.Content("Header")("FileVersion")

            If Not JSON.Content("Body")("VehicleFile") Is Nothing Then stPathVEH.Init(MyPath, JSON.Content("Body")("VehicleFile"))

            stPathENG.Init(MyPath, JSON.Content("Body")("EngineFile"))

            If Not JSON.Content("Body")("GearboxFile") Is Nothing Then stPathGBX.Init(MyPath, JSON.Content("Body")("GearboxFile"))

            If Not JSON.Content("Body")("Cycles") Is Nothing Then
                For Each str In JSON.Content("Body")("Cycles")
                    SubPath = New cSubPath
                    SubPath.Init(MyPath, str)
                    CycleFiles.Add(SubPath)
                Next
            End If

            If Not JSON.Content("Body")("Aux") Is Nothing Then
                For Each dic In JSON.Content("Body")("Aux")

                    AuxID = UCase(Trim(dic("ID").ToString))

                    If AuxPaths.ContainsKey(AuxID) Then
                        WorkerMsg(tMsgID.Err, "Multiple definitions of the same auxiliary type (" & AuxID & ")!", MsgSrc)
                        Return False
                    End If

                    AuxEntry = New cVEH.cAuxEntry

                    AuxEntry.Type = dic("Type")
                    AuxEntry.Path.Init(MyPath, dic("Path"))

                    AuxPaths.Add(AuxID, AuxEntry)

                    AuxDef = True

                Next
            End If

            If Not JSON.Content("Body")("VACC") Is Nothing Then stDesMaxFile.Init(MyPath, JSON.Content("Body")("VACC"))

            EngOnly = JSON.Content("Body")("EngineOnlyMode")

            If EngOnly Then
                VehMode = tVehMode.EngineOnly
            Else
                VehMode = tVehMode.StandardMode
            End If

            If Not JSON.Content("Body")("StartStop") Is Nothing Then
                dic = JSON.Content("Body")("StartStop")
                boStartStop = dic("Enabled")
                siStStV = dic("MaxSpeed")
                siStStT = dic("MinTime")
                StStDelay = dic("Delay")
            Else
                boStartStop = False
            End If

            If Not JSON.Content("Body")("LAC") Is Nothing Then
                dic = JSON.Content("Body")("LAC")
                LookAheadOn = dic("Enabled")
                a_lookahead = dic("Dec")
                vMinLA = dic("MinSpeed")
            Else
                LookAheadOn = False
            End If

            If Not JSON.Content("Body")("OverSpeedEcoRoll") Is Nothing Then

                dic = JSON.Content("Body")("OverSpeedEcoRoll")

                Select Case UCase(dic("Mode").ToString).Trim
                    Case "ECOROLL"
                        OverSpeedOn = False
                        EcoRollOn = True

                    Case "OVERSPEED"
                        OverSpeedOn = True
                        EcoRollOn = False

                    Case "OFF"
                        OverSpeedOn = False
                        EcoRollOn = False

                    Case Else
                        WorkerMsg(tMsgID.Err, "Value '" & dic("Mode") & "' is not valid for OverSpeedEcoRoll/Mode!", MsgSrc)
                        Return False
                End Select

                vMin = dic("MinSpeed")
                OverSpeed = dic("OverSpeed")
                If Not dic("UnderSpeed") Is Nothing Then UnderSpeed = dic("UnderSpeed")

            Else
                OverSpeedOn = False
                EcoRollOn = False
            End If

        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to read VECTO file! " & ex.Message, MsgSrc)
            Return False
        End Try


        Return True


    End Function

    Private Sub SetDefault()
        boPKWja = False
        bodynkorja = False
        ineklasse = 6
        inizykwael = 2
        inPschrit = 20
        innschrit = 20
        boMapSchaltja = True
        iniMsek = 3
        boottoJa = False
        bokaltst1 = False
        sitkat1 = 20
        sitkw1 = 20
        sihsstart = 1
        boEXSja = False
        boStartStop = False
        siStStV = 5
        siStStT = 5
        StStDelay = 0
        boSOCnJa = False
        siSOCstart = 0.5
        GetrMod = tTransLossModel.Detailed
        bKFcutDrag = True
        bKFcutFull = True
        bKFinsertDrag = True
        bKFDragIntp = True
        'FileVersion = 0
        VehMode = tVehMode.StandardMode
        EngAnalysis = False
        CreateMap = False
        ModeHorEV = False
        CoolantsimJa = False

        stPathVEH.Clear()
        stPathENG.Clear()
        CycleFiles.Clear()
        stPathGBX.Clear()
        stdynspez.Clear()

        stkatmap.Clear()
        stkwmap.Clear()
        stkatkurv.Clear()
        stkwkurv.Clear()
        stcooldown.Clear()
        sttumgebung.Clear()

        stBatfile.Clear()
        stEmospez.Clear()
        stEANfile.Clear()
        stGetspez.Clear()
        stSTEnam.Clear()
        stEKFnam.Clear()

        stPathExs.Clear()

        stCoolantSimPath.Clear()

        DesMaxJa = True
        stDesMaxFile.Clear()
        laDesV.Clear()
        laDesMax.Clear()
        laDesMin.Clear()
        DesMaxDim = -1

        AuxPaths.Clear()
        AuxDef = False

        hinauf = 0
        hinunter = 0
        lhinauf = 0
        lhinunter = 0
        pspar = 0
        pmodell = 0

        EngOnly = False

        a_lookahead = 0
        vMin = 0
        LookAheadOn = True
        OverSpeedOn = False
        EcoRollOn = False
        OverSpeed = 0
        UnderSpeed = 0
        vMinLA = 0

    End Sub

    'This Sub reads those Input-files that do not have their own class, etc.
    Public Function Init() As Boolean
        Dim file As cFile_V3
        Dim line As String()

        Dim MsgSrc As String

        MsgSrc = "GEN/Init"

        If DesMaxJa And Not EngOnly Then

            file = New cFile_V3

            If Not file.OpenRead(stDesMaxFile.FullPath) Then
                WorkerMsg(tMsgID.Err, "Can't read .vacc file (" & stDesMaxFile.FullPath & ")", MsgSrc)
                Return False
            End If

            laDesV.Clear()
            laDesMax.Clear()
            laDesMin.Clear()
            DesMaxDim = -1
            Try

                Do While Not file.EndOfFile

                    DesMaxDim += 1

                    line = file.ReadLine

                    laDesV.Add(CSng(line(0)) / 3.6)   'km/h => m/s !!!!
                    laDesMax.Add(CSng(line(1)))
                    laDesMin.Add(CSng(line(2)))

                Loop

            Catch ex As Exception

                file.Close()
                WorkerMsg(tMsgID.Err, "Error in .vacc file. " & ex.Message & " (" & stDesMaxFile.FullPath & ")", MsgSrc)
                Return False

            End Try

            file.Close()

        End If



        Return True

    End Function


#Region "Properties"

    Public ReadOnly Property FileList As List(Of String)
        Get
            Return MyFileList
        End Get
    End Property

    Public Property KFcutFull As Boolean
        Get
            Return bKFcutFull
        End Get
        Set(value As Boolean)
            bKFcutFull = value
        End Set
    End Property

    Public Property KFinsertDrag As Boolean
        Get
            Return bKFinsertDrag
        End Get
        Set(value As Boolean)
            bKFinsertDrag = value
        End Set
    End Property

    Public Property KFcutDrag As Boolean
        Get
            Return bKFcutDrag
        End Get
        Set(value As Boolean)
            bKFcutDrag = value
        End Set
    End Property

    Public Property KFDragIntp As Boolean
        Get
            Return bKFDragIntp
        End Get
        Set(value As Boolean)
            bKFDragIntp = value
        End Set
    End Property

    Public Property TransmModel() As tTransLossModel
        Get
            Return GetrMod
        End Get
        Set(ByVal value As tTransLossModel)
            GetrMod = value
        End Set
    End Property

    Public Property FilePath() As String
        Get
            Return sFilePath
        End Get
        Set(ByVal value As String)
            sFilePath = value
            MyPath = IO.Path.GetDirectoryName(sFilePath) & "\"
        End Set
    End Property

    Public Property PKWja() As Boolean
        Get
            Return boPKWja
        End Get
        Set(ByVal value As Boolean)
            boPKWja = value
        End Set
    End Property

    Public Property dynkorja() As Boolean
        Get
            Return bodynkorja
        End Get
        Set(ByVal value As Boolean)
            bodynkorja = value
        End Set
    End Property

    Public Property eklasse() As Int16
        Get
            Return ineklasse
        End Get
        Set(ByVal value As Int16)
            ineklasse = value
        End Set
    End Property

    Public Property izykwael() As Int16
        Get
            Return inizykwael
        End Get
        Set(ByVal value As Int16)
            inizykwael = value
        End Set
    End Property

    Public Property Pschrit() As Int16
        Get
            Return inPschrit
        End Get
        Set(ByVal value As Int16)
            inPschrit = value
        End Set
    End Property

    Public Property nschrit() As Int16
        Get
            Return innschrit
        End Get
        Set(ByVal value As Int16)
            innschrit = value
        End Set
    End Property

    Public Property MapSchaltja() As Boolean
        Get
            Return boMapSchaltja
        End Get
        Set(ByVal value As Boolean)
            boMapSchaltja = value
        End Set
    End Property

    Public Property iMsek() As Int16
        Get
            Return iniMsek
        End Get
        Set(ByVal value As Int16)
            iniMsek = value
        End Set
    End Property

    Public Property ottoJa() As Boolean
        Get
            Return boottoJa
        End Get
        Set(ByVal value As Boolean)
            boottoJa = value
        End Set
    End Property

    Public Property kaltst1() As Boolean
        Get
            Return bokaltst1
        End Get
        Set(ByVal value As Boolean)
            bokaltst1 = value
        End Set
    End Property

    Public Property tkat1() As Single
        Get
            Return sitkat1
        End Get
        Set(ByVal value As Single)
            sitkat1 = value
        End Set
    End Property

    Public Property tkw1() As Single
        Get
            Return sitkw1
        End Get
        Set(ByVal value As Single)
            sitkw1 = value
        End Set
    End Property

    Public Property hsstart() As Single
        Get
            Return sihsstart
        End Get
        Set(ByVal value As Single)
            sihsstart = value
        End Set
    End Property

    Public Property PathVEH(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stPathVEH.OriginalPath
            Else
                Return stPathVEH.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stPathVEH.Init(MyPath, value)
        End Set
    End Property

    Public Property PathENG(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stPathENG.OriginalPath
            Else
                Return stPathENG.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stPathENG.Init(MyPath, value)
        End Set
    End Property

    Public Property PathGBX(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stPathGBX.OriginalPath
            Else
                Return stPathGBX.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stPathGBX.Init(MyPath, value)
        End Set
    End Property

    Public Property dynspez(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stdynspez.OriginalPath
            Else
                Return stdynspez.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stdynspez.Init(MyPath, value)
        End Set
    End Property

    Public Property katmap(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stkatmap.OriginalPath
            Else
                Return stkatmap.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stkatmap.Init(MyPath, value)
        End Set
    End Property

    Public Property kwmap(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stkwmap.OriginalPath
            Else
                Return stkwmap.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stkwmap.Init(MyPath, value)
        End Set
    End Property

    Public Property katkurv(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stkatkurv.OriginalPath
            Else
                Return stkatkurv.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stkatkurv.Init(MyPath, value)
        End Set
    End Property

    Public Property kwkurv(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stkwkurv.OriginalPath
            Else
                Return stkwkurv.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stkwkurv.Init(MyPath, value)
        End Set
    End Property

    Public Property cooldown(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stcooldown.OriginalPath
            Else
                Return stcooldown.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stcooldown.Init(MyPath, value)
        End Set
    End Property

    Public Property tumgebung(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return sttumgebung.OriginalPath
            Else
                Return sttumgebung.FullPath
            End If
        End Get
        Set(ByVal value As String)
            sttumgebung.Init(MyPath, value)
        End Set
    End Property

    Public Property Batfile(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stBatfile.OriginalPath
            Else
                Return stBatfile.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stBatfile.Init(MyPath, value)
        End Set
    End Property

    Public Property Emospez(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stEmospez.OriginalPath
            Else
                Return stEmospez.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stEmospez.Init(MyPath, value)
        End Set
    End Property

    Public Property EANfile(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stEANfile.OriginalPath
            Else
                Return stEANfile.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stEANfile.Init(MyPath, value)
        End Set
    End Property

    Public Property Getspez(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stGetspez.OriginalPath
            Else
                Return stGetspez.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stGetspez.Init(MyPath, value)
        End Set
    End Property

    Public Property STEnam(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stSTEnam.OriginalPath
            Else
                Return stSTEnam.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stSTEnam.Init(MyPath, value)
        End Set
    End Property

    Public Property EKFnam(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stEKFnam.OriginalPath
            Else
                Return stEKFnam.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stEKFnam.Init(MyPath, value)
        End Set
    End Property

    Public Property PathExs(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stPathExs.OriginalPath
            Else
                Return stPathExs.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stPathExs.Init(MyPath, value)
        End Set
    End Property

    Public Property EXSja() As Boolean
        Get
            Return boEXSja
        End Get
        Set(ByVal value As Boolean)
            boEXSja = value
        End Set
    End Property



    Public Property StartStop() As Boolean
        Get
            Return boStartStop
        End Get
        Set(ByVal value As Boolean)
            boStartStop = value
        End Set
    End Property

    Public Property StStV() As Single
        Get
            Return siStStV
        End Get
        Set(ByVal value As Single)
            siStStV = value
        End Set
    End Property

    Public Property StStT() As Single
        Get
            Return siStStT
        End Get
        Set(ByVal value As Single)
            siStStT = value
        End Set
    End Property

    Public Property SOCnJa() As Boolean
        Get
            Return boSOCnJa
        End Get
        Set(ByVal value As Boolean)
            boSOCnJa = value
        End Set
    End Property

    Public Property SOCstart() As Single
        Get
            Return siSOCstart
        End Get
        Set(ByVal value As Single)
            siSOCstart = value
        End Set
    End Property

    Public Property CoolantSimPath(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stCoolantSimPath.OriginalPath
            Else
                Return stCoolantSimPath.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stCoolantSimPath.Init(MyPath, value)
        End Set
    End Property

    Public Property DesMaxFile(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stDesMaxFile.OriginalPath
            Else
                Return stDesMaxFile.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stDesMaxFile.Init(MyPath, value)
        End Set
    End Property

#End Region

    Public Function aDesMax(ByVal v As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If laDesV(0) >= v Then
            If laDesV(0) > v Then MODdata.ModErrors.DesMaxExtr = "v= " & v * 3.6 & "[km/h]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While laDesV(i) < v And i < DesMaxDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If laDesV(i) < v Then
            MODdata.ModErrors.DesMaxExtr = "v= " & v * 3.6 & "[km/h]"
        End If

lbInt:
        'Interpolation
        Return (v - laDesV(i - 1)) * (laDesMax(i) - laDesMax(i - 1)) / (laDesV(i) - laDesV(i - 1)) + laDesMax(i - 1)

    End Function

    Public Function aDesMin(ByVal v As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If laDesV(0) >= v Then
            If laDesV(0) > v Then MODdata.ModErrors.DesMaxExtr = "v= " & v * 3.6 & "[km/h]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While laDesV(i) < v And i < DesMaxDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If laDesV(i) < v Then
            MODdata.ModErrors.DesMaxExtr = "v= " & v * 3.6 & "[km/h]"
        End If

lbInt:
        'Interpolation
        Return (v - laDesV(i - 1)) * (laDesMin(i) - laDesMin(i - 1)) / (laDesV(i) - laDesV(i - 1)) + laDesMin(i - 1)

    End Function


End Class

