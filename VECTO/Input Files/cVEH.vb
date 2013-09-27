Imports System.Collections.Generic

Public Class cVEH

    Private Const FormatVersion As String = "1.0"
    Private FileVersion As String

    Private sFilePath As String
    Private MyPath As String

    Private siMass As Single
    Private siLoading As Single
    Private siAquers As Single
    'Private siDelta As Single
    Private siI_mot As Single
    Private siI_wheels As Single
    Private siI_Getriebe As Single
    Private siPaux0 As Single
    Private siPnenn As Single
    Private sinNenn As Single
    Private sinLeerl As Single
    Private siFr0 As Single      '<= wird aus Achs-RRC-Werten berechnet
    Private siFr1 As Single
    Private siFr2 As Single
    Private siFr3 As Single
    Private siFr4 As Single
    Private sifGetr As Single
    Private siDreifen As Single
    Private sihinauf As Single
    Private sihinunter As Single
    Private silhinauf As Single
    Private silhinunter As Single
    Private sipspar As Single
    Private sipmodell As Single

    Private siCd0 As Single
    Public CdMode As tCdMode
    Public CdFile As cSubPath
    Private CdX As List(Of Single)
    Private CdY As List(Of Single)
    Private CdDim As Integer

    Public siGetrI As List(Of Single)
    Public GetrMap As List(Of cSubPath)
    Private MyGBmaps As List(Of cDelaunayMap)
    Public GetrEffDef As List(Of Boolean)
    Public GetrEff As List(Of Single)
    Public IsTCgear As List(Of Boolean)

    Private iganganz As Short

    Public AuxPaths As Dictionary(Of String, cAuxEntry)     'Alle Nebenverbraucher der Veh-Datei
    Public AuxRefs As Dictionary(Of String, cAux)          'Alle Nebenverbraucher die in der Veh-Datei UND im Zyklus definiert sind
    Public AuxDef As Boolean                               'True wenn ein oder mehrere Nebenverbraucher definiert sind

    Public TracIntrSi As Single

    Public RtType As tRtType '0=None, 1=Primary, 2=Secondary
    Public RtRatio As Single = 0
    Public RtFile As cSubPath
    Private RtDim As Integer
    Private RtnU As List(Of Single)
    Private RtM As List(Of Single)

    Public RRCs As List(Of Single())
    Public VehCat As tVehCat
    Public MassExtra As Single
    Public MassMax As Single
    Public AxleConf As tAxleConf

    Public NoJSON As Boolean

    Private MyFileList As List(Of String)


    Public Function CreateFileList() As Boolean

        If Not Me.ReadFile Then Return False

        MyFileList = New List(Of String)

        '.vcdv  / .vcdb
        If Me.CdMode <> tCdMode.ConstCd0 Then MyFileList.Add(Me.CdFile.FullPath)

        'Retarder
        If Me.RtType <> tRtType.None Then MyFileList.Add(Me.RtFile.FullPath)

        Return True

    End Function


    Public Class cAuxEntry
        Public Type As String
        Public Path As cSubPath
        Public Sub New()
            Path = New cSubPath
        End Sub
    End Class

    Public Sub New()
        MyPath = ""
        sFilePath = ""
        CdFile = New cSubPath
        CdX = New List(Of Single)
        CdY = New List(Of Single)
        RtFile = New cSubPath
        RtnU = New List(Of Single)
        RtM = New List(Of Single)
        RRCs = New List(Of Single())
        SetDefault()
    End Sub

    Private Sub SetDefault()
        siMass = 0
        MassExtra = 0
        siLoading = 0
        siCd0 = 0
        CdFile.Clear()
        CdMode = tCdMode.ConstCd0
        CdX.Clear()
        CdY.Clear()
        CdDim = -1
        siAquers = 0
        siI_mot = 0
        siI_wheels = 0
        siI_Getriebe = 0
        siPaux0 = 0
        siPnenn = 0
        sinNenn = 0
        sinLeerl = 0
        siFr0 = 0
        siFr1 = 0
        siFr2 = 0
        siFr3 = 0
        siFr4 = 0
        sifGetr = 0
        siDreifen = 0
        sihinauf = 0
        sihinunter = 0
        silhinauf = 0
        silhinunter = 0
        sipspar = 0
        sipmodell = 0

        siGetrI = New List(Of Single)
        GetrEffDef = New List(Of Boolean)
        GetrEff = New List(Of Single)
        GetrMap = New List(Of cSubPath)
        IsTCgear = New List(Of Boolean)

        MyGBmaps = Nothing
        iganganz = 0
        AuxPaths = New Dictionary(Of String, cAuxEntry)
        AuxRefs = New Dictionary(Of String, cAux)
        AuxDef = False
        TracIntrSi = 0
        RtType = tRtType.None
        RtRatio = 0
        RtnU.Clear()
        RtM.Clear()
        RtFile.Clear()
        RRCs.Clear()
        VehCat = tVehCat.Rigid
        MassMax = 0
        AxleConf = tAxleConf.a4x2
    End Sub

    Private Function ReadFileOld() As Boolean
        Dim file As cFile_V3
        Dim line() As String
        Dim MsgSrc As String

        MsgSrc = "VEH/ReadFile"

        SetDefault()

        If sFilePath = "" Or Not IO.File.Exists(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Vehicle file not found (" & sFilePath & ") !", MsgSrc)
            Return False
        End If

        file = New cFile_V3

        If Not file.OpenRead(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Failed to open file (" & sFilePath & ") !", MsgSrc)
            file = Nothing
            Return False
        End If

        siMass = CSng(file.ReadLine(0))
        MassExtra = CSng(file.ReadLine(0))
        siLoading = CSng(file.ReadLine(0))
        siCd0 = CSng(file.ReadLine(0))
        siAquers = CSng(file.ReadLine(0))
        'siI_mot = CSng(file.ReadLine(0))
        siI_wheels = CSng(file.ReadLine(0))
        'siI_Getriebe = CSng(file.ReadLine(0))
        'siPaux0 = CSng(file.ReadLine(0))
        'siPnenn = CSng(file.ReadLine(0))
        'sinNenn = CSng(file.ReadLine(0))
        'sinLeerl = CSng(file.ReadLine(0))
        'file.ReadLine()     'Früher: Getr-Verl Kennfeld
        'siFr0 = CSng(file.ReadLine(0))
        'file.ReadLine()     'Früher: Getr-Verl Kennfeld verwenden Ja/Nein
        'sifGetr = CSng(file.ReadLine(0))
        'Achs-Übersetzung-TransLoss Parameter
        'line = file.ReadLine
        'siGetrI(0) = CSng(line(0))
        'If UBound(line) > 0 Then sGetrMaps(0).Init(MyPath, line(1))

        siDreifen = CSng(file.ReadLine(0))
        'iganganz = 0
        'For i = 1 To 16
        '    line = file.ReadLine
        '    siGetrI(i) = CSng(line(0))
        '    'Falls keine GetrModell-Parameter vorhanden Dummy-Belegung durch SetDefault
        '    If UBound(line) > 0 Then sGetrMaps(i).Init(MyPath, line(1))
        '    If Igetr(i) > 0.0001 Then iganganz = i
        'Next
        'sihinauf = CSng(file.ReadLine(0))
        'sihinunter = CSng(file.ReadLine(0))
        'silhinauf = CSng(file.ReadLine(0))
        'silhinunter = CSng(file.ReadLine(0))
        'sipspar = CSng(file.ReadLine(0))
        'sipmodell = CSng(file.ReadLine(0))

        ''Schaltmodell-Verteilung
        'If (sipspar > 1) Then
        '    sipspar = 1
        'ElseIf (sipspar < 0) Then
        '    sipspar = 0
        'End If
        'If (sipmodell > 1) Then
        '    sipmodell = 1
        'ElseIf (sipmodell < 0) Then
        '    sipmodell = 0
        'End If
        'If ((sipspar + sipmodell) > 1.0) Then sipmodell = 1.0 - sipspar

        'Update 07.08.2012 (CO2 Demo)
        'Einzelne Nebenverbraucher |@@| Individual next consumer
        'Do While Not file.EndOfFile

        '    line = file.ReadLine

        '    If line(0) = sKey.Break Then Exit Do

        '    AuxID = UCase(Trim(line(0)))

        '    If AuxPaths.ContainsKey(AuxID) Then
        '        WorkerMsg(tMsgID.Err, "Multiple definitions of the same auxiliary type (" & line(0) & ")!", MsgSrc)
        '        file.Close()
        '        Return False
        '    End If

        '    AuxEntry = New cAuxEntry

        '    AuxEntry.Type = line(1)
        '    AuxEntry.Path.Init(MyPath, line(2))

        '    AuxPaths.Add(AuxID, AuxEntry)

        '    AuxDef = True

        'Loop

        If file.EndOfFile Then GoTo lbError

        'Zugkraftunterbrechung - Update 09.08.2012 (CO2 Demo) |@@| Interruption of Traction - Update 09/08/2012 (CO2 demo)
        'Try
        '    TracIntrSi = CSng(file.ReadLine(0))
        'Catch ex As Exception
        '    WorkerMsg(tMsgID.Err, ex.Message, MsgSrc)
        '    file.Close()
        '    Return False
        'End Try

        If file.EndOfFile Then GoTo lbError

        'Cd mode / Input File - Update 08/14/2012 (CO2 demo)
        Try
            line = file.ReadLine
            CdMode = CType(CInt(line(0)), tCdMode)
            CdFile.Init(MyPath, line(1))
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, ex.Message, MsgSrc)
            file.Close()
            Return False
        End Try

        If file.EndOfFile Then GoTo lbError

        'Retarder - Update 02.10.2012 (CO2 Demo)
        Try
            RtType = CType(CInt(file.ReadLine(0)), tRtType)
            RtRatio = CSng(file.ReadLine(0))
            RtFile.Init(MyPath, file.ReadLine(0))
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, ex.Message, MsgSrc)
            file.Close()
            Return False
        End Try

        'Axle configuration - Update 16.10.2012
        Do While Not file.EndOfFile

            line = file.ReadLine

            If line(0) = sKey.Break Then Exit Do

            Try
                If UBound(line) > 2 Then
                    RRCs.Add(New Single() {CSng(line(0)), CSng(line(1)), CSng(line(2)), CSng(line(3))})
                Else
                    RRCs.Add(New Single() {0.0, CSng(False), CSng(line(1)), 0.0})
                End If
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, ex.Message, MsgSrc)
                file.Close()
                Return False
            End Try


        Loop

        If file.EndOfFile Then GoTo lbError

        Try
            VehCat = CType(CInt(file.ReadLine(0)), tVehCat)
            MassExtra = CSng(file.ReadLine(0))
            MassMax = CSng(file.ReadLine(0))
            AxleConf = CType(CInt(file.ReadLine(0)), tAxleConf)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, ex.Message, MsgSrc)
            file.Close()
            Return False
        End Try


        '************************ End reading ****************************

        file.Close()
        Return True


lbError:
        file.Close()
        WorkerMsg(tMsgID.Err, "Unexpected end of file!", MsgSrc)
        Return False

    End Function

    Private Function SaveFileOld() As Boolean
        Dim file As cFile_V3
        Dim sl As Single()

        file = New cFile_V3

        If sFilePath = "" Then Return False

        If Not file.OpenWrite(sFilePath) Then Return False

        file.WriteLine("c VECTO Vehicle Input File")
        file.WriteLine("c VECTO " & VECTOvers)
        file.WriteLine("c " & Now.ToString)

        file.WriteLine("c Curb weight vehicle [kg]")
        file.WriteLine(CStr(siMass))
        file.WriteLine("c Curb weight extra trailer/body [kg]")
        file.WriteLine(CStr(MassExtra))
        file.WriteLine("c Vehicle loading [kg]")
        file.WriteLine(CStr(siLoading))
        file.WriteLine("c Cd value [-]")
        file.WriteLine(CStr(siCd0))
        file.WriteLine("c Cross sectional area [m^2]")
        file.WriteLine(CStr(siAquers))
        'file.WriteLine("c Engine rotational inertia [kg*m^2]")
        'file.WriteLine(CStr(siI_mot))
        file.WriteLine("c Wheels inertia [kgm^2]")
        file.WriteLine(CStr(siI_wheels))
        'file.WriteLine("c Gearbox rotational inertia [kg*m^2]")
        'file.WriteLine(CStr(siI_Getriebe))
        'file.WriteLine("c Auxiliaries base power demand (normalized) [-]")
        'file.WriteLine(CStr(siPaux0))
        'file.WriteLine("c Engine rated power [kW]")
        'file.WriteLine(CStr(siPnenn))
        'file.WriteLine("c Engine rated speed [rpm]")
        'file.WriteLine(CStr(sinNenn))
        'file.WriteLine("c Engine idling speed [rpm]")
        'file.WriteLine(CStr(sinLeerl))
        'file.WriteLine("c Transmission loss factor")
        'file.WriteLine(CStr(sifGetr))
        'file.WriteLine("c Transmission")
        'file.WriteLine("c Axle ratio [-], path to efficiency map file (optional)")
        'file.WriteLine(CStr(siGetrI(0)), sGetrMaps(0).PathOrDummy)
        file.WriteLine("c Wheel effective diameter [m]")
        file.WriteLine(CStr(siDreifen))
        'file.WriteLine("c Transmission gears: Ratio [-], path to efficiency map file (optional)")
        'For i = 1 To 16
        '    file.WriteLine("c Gear " & i)
        '    file.WriteLine(CStr(siGetrI(i)), sGetrMaps(i).PathOrDummy)
        'Next
        'file.WriteLine("c Gear shift behaviour:")
        'file.WriteLine("c Gearshift model (Version fast driver)")
        'file.WriteLine("c shift up at ratio rpm/rated rpm in actual gear greater than")
        'file.WriteLine(CStr(sihinauf))
        'file.WriteLine("c shift down when rpm/rated rpm in lower gear is higher than")
        'file.WriteLine(CStr(sihinunter))
        'file.WriteLine("c Gearshift model (Version economic driver)")
        'file.WriteLine("c shift up at ratio rpm/rated rpm in higher gear greater than")
        'file.WriteLine(CStr(silhinauf))
        'file.WriteLine("c Shift down when ratio rpm/rated rpm in actual gear is lower than")
        'file.WriteLine(CStr(silhinunter))
        'file.WriteLine("c Share of version economic driver (0 to 1)")
        'file.WriteLine(CStr(sipspar))
        'file.WriteLine("c Share of version mixed model (0 to 1)")
        'file.WriteLine(CStr(sipmodell))

        'file.WriteLine("c Auxiliaries")
        'For Each AuxEntryKV In AuxPaths
        '    file.WriteLine(Trim(UCase(AuxEntryKV.Key)) & "," & AuxEntryKV.Value.Type & "," & AuxEntryKV.Value.Path.PathOrDummy)
        'Next
        'file.WriteLine(sKey.Break)

        'Interruption of traction (Update 09.08.2012 - CO2 demo)
        'file.WriteLine("c Traction Interruption")
        'file.WriteLine(CStr(TracIntrSi))

        'Cd Modus / Input Datei (Update 14.08.2012 - CO2 Demo)
        file.WriteLine("c Cd Mode, Input File")
        file.WriteLine(CStr(CType(CdMode, Integer)), CdFile.PathOrDummy)

        'Retarder (Update 02.10.2012 - CO2 Demo)
        file.WriteLine("c Retarder Type (0=None, 1=Primary, 2=Secondary)")
        file.WriteLine(CStr(CType(RtType, Integer)))
        file.WriteLine("c Retarder Ratio")
        file.WriteLine(CStr(RtRatio))
        file.WriteLine("c Retarder File")
        file.WriteLine(CStr(RtFile.PathOrDummy))

        'Axle configuration - Update 16.10.2012
        file.WriteLine("c Axle configurations")
        file.WriteLine("c Axle weight share [-], RRC [N/N],Fz ISO [N]")
        For Each sl In RRCs
            file.WriteLine(CStr(sl(0)), CStr(sl(1)), CStr(sl(2)), CStr(sl(3)))
        Next

        file.WriteLine(sKey.Break)

        file.WriteLine("c VehCat")
        file.WriteLine(CStr(CType(VehCat, Integer)))
        file.WriteLine("c MassExtra")
        file.WriteLine(CStr(MassExtra))
        file.WriteLine("c MassMax")
        file.WriteLine(CStr(MassMax))
        file.WriteLine("c Axle Configuration")
        file.WriteLine(CStr(CType(AxleConf, Integer)))

        file.Close()
        file = Nothing

        Return True

    End Function


    Public Function ReadFile() As Boolean
        Dim JSON As New cJSON
        Dim dic As Object

        Dim MsgSrc As String


        MsgSrc = "VEH/ReadFile"

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

            siMass = JSON.Content("Body")("CurbWeight")
            MassExtra = JSON.Content("Body")("CurbWeightExtra")
            siLoading = JSON.Content("Body")("Loading")
            MassMax = JSON.Content("Body")("MassMax")

            siCd0 = JSON.Content("Body")("Cd")
            siAquers = JSON.Content("Body")("CrossSecArea")

            siI_wheels = JSON.Content("Body")("WheelsInertia")
            siDreifen = JSON.Content("Body")("WheelsDiaEff")

            CdMode = CdModeConv(JSON.Content("Body")("CdCorrMode").ToString)
            If Not JSON.Content("Body")("CdCorrFile") Is Nothing Then CdFile.Init(MyPath, JSON.Content("Body")("CdCorrFile"))

            If JSON.Content("Body")("Retarder") Is Nothing Then
                RtType = tRtType.None
            Else
                RtType = RtTypeConv(JSON.Content("Body")("Retarder")("Type").ToString)
                If Not JSON.Content("Body")("Retarder")("Ratio") Is Nothing Then RtRatio = JSON.Content("Body")("Retarder")("Ratio")
                If Not JSON.Content("Body")("Retarder")("File") Is Nothing Then RtFile.Init(MyPath, JSON.Content("Body")("Retarder")("File"))
            End If

            AxleConf = AxleConfConv(JSON.Content("Body")("AxleConfig")("Type").ToString)
            For Each dic In JSON.Content("Body")("AxleConfig")("Axles")
                RRCs.Add(New Single() {dic("AxleWeightShare"), CSng(dic("TwinTyres")), dic("RRCISO"), dic("FzISO")})
            Next

            VehCat = VehCatConv(JSON.Content("Body")("VehCat").ToString)

        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to read Vehicle file! " & ex.Message, MsgSrc)
            Return False
        End Try

        Return True



    End Function

    Public Function SaveFile() As Boolean
        Dim sl As Single()
        Dim dic As Dictionary(Of String, Object)
        Dim dic0 As Dictionary(Of String, Object)
        Dim ls As List(Of Dictionary(Of String, Object))
        Dim JSON As New cJSON


        If Not Cfg.JSON Then Return SaveFileOld()

        'Header
        dic = New Dictionary(Of String, Object)
        dic.Add("CreatedBy", Lic.LicString & " (" & Lic.GUID & ")")
        dic.Add("Date", Now.ToString)
        dic.Add("AppVersion", VECTOvers)
        dic.Add("FileVersion", FormatVersion)
        JSON.Content.Add("Header", dic)

        'Body
        dic = New Dictionary(Of String, Object)

        dic.Add("VehCat", VehCatConv(VehCat))

        dic.Add("CurbWeight", siMass)
        dic.Add("CurbWeightExtra", MassExtra)
        dic.Add("Loading", siLoading)
        dic.Add("MassMax", MassMax)

        dic.Add("Cd", siCd0)
        dic.Add("CrossSecArea", siAquers)

        dic.Add("WheelsInertia", siI_wheels)
        dic.Add("WheelsDiaEff", siDreifen)

        dic.Add("CdCorrMode", CdModeConv(CdMode))
        dic.Add("CdCorrFile", CdFile.PathOrDummy)

        dic0 = New Dictionary(Of String, Object)
        dic0.Add("Type", RtTypeConv(RtType))
        dic0.Add("Ratio", RtRatio)
        dic0.Add("File", RtFile.PathOrDummy)
        dic.Add("Retarder", dic0)

        ls = New List(Of Dictionary(Of String, Object))
        For Each sl In RRCs
            dic0 = New Dictionary(Of String, Object)
            dic0.Add("AxleWeightShare", sl(0))
            dic0.Add("TwinTyres", CBool(sl(1)))
            dic0.Add("RRCISO", sl(2))
            dic0.Add("FzISO", sl(3))
            ls.Add(dic0)
        Next

        dic0 = New Dictionary(Of String, Object)
        dic0.Add("Type", AxleConfConv(AxleConf))
        dic0.Add("Axles", ls)
        dic.Add("AxleConfig", dic0)

        JSON.Content.Add("Body", dic)

        Return JSON.WriteFile(sFilePath)


    End Function


    Public Function VehmodeInit() As Boolean

        Dim MsgSrc As String
        Dim sl As Single()
        Dim ShareSum As Double
        Dim RRC As Double
        Dim nrwheels As Single


        MsgSrc = "VEH/Init"

        'Error-message within AuxInit
        If Not AuxInit() Then Return False

        'Cd-Init
        If Not CdInit() Then Return False

        'Transmission Loss Maps
        If GEN.TransmModel = tTransLossModel.Detailed Then
            If Not VEH.TrLossMapInit Then
                WorkerMsg(tMsgID.Err, "Failed to initialize Transmission Loss Maps!", MsgSrc)
                Return False
            End If
        End If

        'Retarder
        If Not RtInit() Then Return False

        'Fr0
        If RRCs.Count < 2 Then
            WorkerMsg(tMsgID.Err, "At least 2 axle configurations are required!", MsgSrc, "<GUI>" & sFilePath)
            Return False
        End If

        'Check if sum=100%
        ShareSum = 0
        For Each sl In RRCs
            ShareSum += sl(0)
        Next

        If Math.Abs(ShareSum - 1) > 0.0001 Then
            WorkerMsg(tMsgID.Err, "Sum of relative axle shares is not 100%!", MsgSrc, "<GUI>" & sFilePath)
            Return False
        End If

        RRC = 0
        For Each sl In RRCs
            If CBool(sl(1)) Then
                nrwheels = 4
            Else
                nrwheels = 2
            End If
            RRC += sl(0) * (sl(2) * ((siLoading + siMass + MassExtra) * sl(0) * 9.81 / (sl(3) * nrwheels)) ^ (0.9 - 1))     'Beta=0.9
        Next

        siFr0 = RRC

        Return True

    End Function

#Region "Transmission Loss Maps"

    Private Function TrLossMapInit() As Boolean
        Dim i As Short
        Dim GBmap0 As cDelaunayMap
        'Dim n_norm As Double
        'Dim Pe_norm As Double
        Dim file As cFile_V3
        Dim path As String
        Dim line As String()
        Dim l As Integer
        Dim nU As Double
        Dim M_in As Double
        Dim M_loss As Double
        Dim M_out As Double

        Dim dnU As Single
        Dim nn As Single
        Dim dM As Single
        Dim P_In As Single
        Dim P_Loss As Single
        Dim EffSum As Single
        Dim Anz As Integer
        Dim EffDiffSum As Single = 0
        Dim AnzDiff As Integer = 0

        Dim MsgSrc As String

        MyGBmaps = New List(Of cDelaunayMap)
        file = New cFile_V3

        For i = 0 To iganganz

            MsgSrc = "VEH/TrLossMapInit/G" & i

            If GetrEffDef(i) Then

                If GetrEff(i) > 1 Or GetrEff(i) <= 0 Then
                    WorkerMsg(tMsgID.Err, "Gearboy efficiency '" & GetrEff(i) & "' invalid!", MsgSrc)
                    Return False
                End If

                MyGBmaps.Add(Nothing)

            Else

                path = GetrMap(i).FullPath

                If Not file.OpenRead(path) Then
                    WorkerMsg(tMsgID.Err, "Cannot read file '" & path & "'!", MsgSrc)
                    MyGBmaps = Nothing
                    Return False
                End If

                'Transmission Nominal-Revolutions
                'n_norm = CSng(file.ReadLine(0))

                'Transmission Nominal-Power
                'Pe_norm = CSng(file.ReadLine(0))

                'If nothing specified: Engine's Nominal-power and Nominal-Revolutions from Normalized ones
                'If n_norm < 0.0001 Then
                '    n_norm = sinNenn
                '    Pe_norm = siPnenn
                'End If

                GBmap0 = New cDelaunayMap
                GBmap0.DualMode = True

                l = 0   'Nur für Fehler-Ausgabe
                Do While Not file.EndOfFile
                    l += 1
                    line = file.ReadLine
                    Try
                        'PHEM:    n, PeIn, PeOut => x=n, y=PeOut, z=PeIn |@@| PHEM:    n, PeIn, PeOut => x=n, y=PeOut, z=PeIn
                        'PHEM: GBmap0.AddPoints(CDbl(line(0)) * n_norm, CDbl(line(2)) * Pe_norm, CDbl(line(1)) * Pe_norm) |@@| PHEM: GBmap0.AddPoints(CDbl(line(0)) * n_norm, CDbl(line(2)) * Pe_norm, CDbl(line(1)) * Pe_norm)
                        'old version: VECTO: n, M_in, M_loss => x=n, y=PeOut, z=PeIn |@@| VECTO: n, M_in, M_loss => x=n, y=PeOut, z=PeIn
                        'VECTO: n, M_in, M_loss => x=n, y=M_out, z=M_in
                        nU = CDbl(line(0))
                        M_in = CDbl(line(1))
                        M_loss = CDbl(line(2))

                        M_out = M_in - M_loss

                        'old version: Power instead of torque: GBmap0.AddPoints(nU, nMtoPe(nU, M_out), nMtoPe(nU, M_in))
                        GBmap0.AddPoints(nU, M_out, M_in)
                    Catch ex As Exception
                        WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & l & " (" & path & ")", MsgSrc, path)
                        file.Close()
                        MyGBmaps = Nothing
                        Return False
                    End Try
                Loop

                file.Close()

                If Not GBmap0.Triangulate Then
                    WorkerMsg(tMsgID.Err, "Map triangulation failed! File: " & path, MsgSrc, path)
                    MyGBmaps = Nothing
                    Return False
                End If

                MyGBmaps.Add(GBmap0)

                'Calculate average efficiency for fast approx. calculation
                If i > 0 Then

                    EffSum = 0
                    Anz = 0

                    dnU = (2 / 3) * (VEH.nNenn - VEH.nLeerl) / 10
                    nU = VEH.nLeerl + dnU

                    Do While nU <= nNenn
                        nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                        dM = nPeToM(nU, (2 / 3) * FLD(i).Pfull(nn) / 10)
                        M_in = nPeToM(nU, (1 / 3) * FLD(i).Pfull(nn))

                        Do While M_in <= nPeToM(nU, FLD(i).Pfull(nn))

                            P_In = nMtoPe(nU, M_in)

                            P_Loss = IntpolPeLossFwd(i, nU, P_In, False)

                            EffSum += (P_In - P_Loss) / P_In
                            Anz += 1

                            'Axle
                            P_In -= P_Loss
                            P_Loss = IntpolPeLossFwd(0, nU / VEH.Igetr(i), P_In, False)
                            EffDiffSum += (P_In - P_Loss) / P_In
                            AnzDiff += 1

                            M_in += dM
                        Loop


                        nU += dnU
                    Loop

                    If MODdata.ModErrors.TrLossMapExtr <> "" Then
                        WorkerMsg(tMsgID.Err, "Transmission loss map does not cover full engine range! File: " & path, MsgSrc, path)
                        Return False
                    End If

                    If Anz = 0 Then
                        WorkerMsg(tMsgID.Err, "Failed to calculate approx. transmission losses!", MsgSrc)
                        Return False
                    End If

                    GetrEff(i) = EffSum / Anz

                End If


            End If

        Next

        If Not GetrEffDef(0) Then
            GetrEff(0) = EffDiffSum / AnzDiff
        End If


        Return True

    End Function

    Public Function IntpolPeLoss(ByVal Gear As Integer, ByVal nU As Double, ByVal PeOut As Double, ByVal Approx As Boolean) As Double

        Dim PeIn As Double
        Dim WG As Double
        Dim GBmap As cDelaunayMap
        Dim i As Integer
        Dim Ab As Double
        Dim AbMin As Double
        Dim iMin As Integer
        Dim PeOutX As Double
        Dim GrTxt As String

        Dim MsgSrc As String

        MsgSrc = "VEH/TrLossMapInterpol/G" & Gear

        If Gear = 0 Then
            GrTxt = "A"
        Else
            GrTxt = Gear.ToString
        End If

        If GetrEffDef(Gear) Or (Approx And DEV.AllowAprxTrLoss) Then

            If PeOut > 0 Then
                PeIn = PeOut / GetrEff(Gear)
            Else
                PeIn = PeOut * GetrEff(Gear)
            End If

        Else

            GBmap = MyGBmaps(Gear)

            Try
                'Interpolate with Original Values
                PeIn = nMtoPe(nU, GBmap.Intpol(nU, nPeToM(nU, PeOut)))

            Catch ex As Exception

                'If error: try extrapolation

                'Search for the nearest Map point
                AbMin = ((GBmap.ptList(0).X - nU) ^ 2 + (GBmap.ptList(0).Y - nPeToM(nU, PeOut)) ^ 2) ^ 0.5
                iMin = 0
                For i = 1 To GBmap.ptDim
                    Ab = ((GBmap.ptList(i).X - nU) ^ 2 + (GBmap.ptList(i).Y - nPeToM(nU, PeOut)) ^ 2) ^ 0.5
                    If Ab < AbMin Then
                        AbMin = Ab
                        iMin = i
                    End If
                Next

                PeOutX = nMtoPe(nU, GBmap.ptList(iMin).Y)
                PeIn = nMtoPe(nU, GBmap.ptList(iMin).Z)

                'Efficiency
                If PeOutX > 0 Then
                    If PeIn > 0 Then

                        'Drivetrain=> Drivetrain
                        If PeIn = 0 Then Return 0

                        WG = PeOutX / PeIn

                    Else

                        'Drag => Drivetrain: ERROR!
                        WorkerMsg(tMsgID.Err, "Transmission Loss Map invalid! Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" & PeIn.ToString("0.0") & " [kW], PeOut=" & PeOutX.ToString("0.0") & " [kW]", MsgSrc)
                        WorkerAbort()
                        Return 0

                    End If

                Else
                    If PeIn > 0 Then

                        'WorkerMsg(tMsgID.Warn, "Change of sign in Transmission Loss Map! Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" & PeIn.ToString("0.00") & " [kW], PeOut=" & PeOutX.ToString("0.00") & " [kW]", MsgSrc)
                        'WorkerAbort()
                        WG = (PeIn - (PeIn - PeOutX)) / PeIn

                    Else

                        'Drag => Drag
                        If PeOutX = 0 Then Return 0

                        WG = PeIn / PeOutX

                    End If
                End If


                'Calculate efficiency with PeIn for original PeOut
                PeIn = PeOut / WG

                MODdata.ModErrors.TrLossMapExtr = "Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], MeOut=" & nPeToM(nU, PeOut).ToString("0.00") & " [Nm]"

            End Try

        End If

        Return Math.Max(PeIn - PeOut, 0)


    End Function

    Public Function IntpolPeLossFwd(ByVal Gear As Integer, ByVal nU As Double, ByVal PeIn As Double, ByVal Approx As Boolean) As Double

        Dim PeOut As Double
        Dim WG As Double
        Dim GBmap As cDelaunayMap
        Dim i As Integer
        Dim Ab As Double
        Dim AbMin As Double
        Dim iMin As Integer
        Dim PeInX As Double
        Dim GrTxt As String

        Dim MsgSrc As String

        MsgSrc = "VEH/TrLossMapInterpolFwd/G" & Gear

        If Gear = 0 Then
            GrTxt = "A"
        Else
            GrTxt = Gear.ToString
        End If

        If GetrEffDef(Gear) Or (Approx And DEV.AllowAprxTrLoss) Then

            If PeIn > 0 Then
                PeOut = PeIn * GetrEff(Gear)
            Else
                PeOut = PeIn / GetrEff(Gear)
            End If

        Else

            GBmap = MyGBmaps(Gear)

            Try
                'Interpolate with original values
                PeOut = nMtoPe(nU, GBmap.IntpolXZ(nU, nPeToM(nU, PeIn)))

            Catch ex As Exception

                'If error: try extrapolation

                'Search for the nearest Map-point
                AbMin = ((GBmap.ptList(0).X - nU) ^ 2 + (GBmap.ptList(0).Z - nPeToM(nU, PeIn)) ^ 2) ^ 0.5
                iMin = 0
                For i = 1 To GBmap.ptDim
                    Ab = ((GBmap.ptList(i).X - nU) ^ 2 + (GBmap.ptList(i).Z - nPeToM(nU, PeIn)) ^ 2) ^ 0.5
                    If Ab < AbMin Then
                        AbMin = Ab
                        iMin = i
                    End If
                Next

                PeInX = nMtoPe(nU, GBmap.ptList(iMin).Z)
                PeOut = nMtoPe(nU, GBmap.ptList(iMin).Y)

                'Efficiency
                If PeOut > 0 Then
                    If PeInX > 0 Then

                        'Drivetrain => Drivetrain
                        WG = PeOut / PeInX

                    Else

                        'Drag => Drivetrain: ERROR!
                        WorkerMsg(tMsgID.Err, "Transmission Loss Map invalid! Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" & PeInX.ToString("0.00") & " [kW], PeOut=" & PeOut.ToString("0.00") & " [kW] (fwd)", MsgSrc)
                        WorkerAbort()
                        Return 0

                    End If

                Else
                    If PeInX > 0 Then

                        WorkerMsg(tMsgID.Warn, "Change of sign in Transmission Loss Map! Set efficiency to 10%. Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" & PeInX.ToString("0.00") & " [kW], PeOut=" & PeOut.ToString("0.00") & " [kW] (fwd)", MsgSrc)
                        'WorkerAbort()
                        WG = 0.1

                    Else

                        'Drag => Drag
                        WG = PeInX / PeOut


                    End If
                End If

                'Calculate efficiency with PeIn for original PeOut
                PeOut = PeIn * WG

                MODdata.ModErrors.TrLossMapExtr = "Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], MeIn=" & nPeToM(nU, PeIn).ToString("0.00") & " [Nm] (fwd)"

            End Try

        End If

        Return Math.Max(PeIn - PeOut, 0)

    End Function



#End Region

#Region "Aux"

    Private Function AuxInit() As Boolean

        Dim Aux0 As cAux
        Dim AuxPathKV As KeyValuePair(Of String, cAuxEntry)
        Dim DRIauxcheck As New Dictionary(Of String, Boolean)
        Dim AuxID As String

        Dim MsgSrc As String

        MsgSrc = "VEH/AuxInit"

        AuxRefs = New Dictionary(Of String, cAux)

        If DRI.AuxDef Xor AuxDef Then

            If AuxDef Then
                WorkerMsg(tMsgID.Err, "No auxiliary input defined in driving cycle!", MsgSrc)
                Return False
            Else
                WorkerMsg(tMsgID.Warn, "No auxiliary defined in vehicle file! Psupply input will be ignored!", MsgSrc)
                Return True
            End If

        End If

        If Not (DRI.AuxDef Or AuxDef) Then Return True


        For Each AuxID In DRI.AuxComponents.Keys
            DRIauxcheck.Add(AuxID, False)
        Next

        For Each AuxPathKV In AuxPaths

            MsgSrc = "VEH/AuxInit/" & AuxPathKV.Key

            If Not DRI.AuxComponents.ContainsKey(AuxPathKV.Key) Then
                WorkerMsg(tMsgID.Err, "No Psupply input defined in driving cycle for auxiliary '" & AuxPathKV.Key & "'!", MsgSrc)
                Return False
            End If

            Aux0 = New cAux
            Aux0.Filepath = AuxPathKV.Value.Path.FullPath

            If Not Aux0.Readfile Then
                'Notificationin ReadFile()
                Return False
            End If

            AuxRefs.Add(AuxPathKV.Key, Aux0)

            DRIauxcheck(AuxPathKV.Key) = True

        Next

        MsgSrc = "VEH/AuxInit"

        For Each AuxID In DRI.AuxComponents.Keys
            If Not DRIauxcheck(AuxID) Then WorkerMsg(tMsgID.Warn, "Auxiliary '" & AuxID & "' not found! Psupply input will be ignored!", MsgSrc)
        Next

        Return True

    End Function

    Public Function Paux(ByVal AuxID As String, ByVal t As Integer, ByVal nU As Single) As Single
        Dim Psupply As Single
        Dim Px As Single
        Dim Aux0 As cAux

        Dim MsgSrc As String

        MsgSrc = "VEH/Paux"

        If AuxDef Then

            Aux0 = AuxRefs(AuxID)

            Psupply = DRI.AuxComponents(AuxID)(t)

            If Psupply < 0 Then GoTo lbAuxError

            Px = Aux0.Paux(nU, Psupply)

            If Px < 0 Then GoTo lbAuxError

            Return Px

        Else

            Return 0

        End If


lbAuxError:
        MODdata.ModErrors.AuxNegative = AuxID
        Return 0


    End Function

    Public Function PauxSum(ByVal t As Integer, ByVal nU As Single) As Single
        Dim sum As Single
        Dim AuxID As String

        Dim MsgSrc As String

        MsgSrc = "VEH/Paux"

        If AuxDef Then

            sum = 0

            For Each AuxID In AuxRefs.Keys

                sum += Paux(AuxID, t, nU)

            Next

            Return sum

        Else

            Return 0

        End If

    End Function

#End Region

#Region "Cd Funktionen"

    Private Function CdInit() As Boolean
        Dim file As cFile_V3
        Dim MsgSrc As String
        Dim line As String()

        MsgSrc = "VEH/CdInit"

        'Warn If Vair specified in DRI but CdType != CdOfBeta
        If DRI.VairVorg Xor CdMode = tCdMode.CdOfBeta Then

            If DRI.VairVorg Then
                WorkerMsg(tMsgID.Warn, "Vair input in driving cycle will be irgnored! (Side wind correction disabled in .veh file)", MsgSrc)
            Else
                WorkerMsg(tMsgID.Err, "No Vair input in driving cycle defined! Vres and Beta is required!", MsgSrc)
                Return False
            End If

        End If

        'If Cd-value is constant then do nothing
        If CdMode = tCdMode.ConstCd0 Then Return True

        'Read Inputfile
        file = New cFile_V3

        If Not file.OpenRead(CdFile.FullPath) Then
            WorkerMsg(tMsgID.Err, "Failed to read Cd input file! (" & CdFile.FullPath & ")", MsgSrc)
            Return False
        End If

        CdDim = -1
        Do While Not file.EndOfFile

            CdDim += 1
            line = file.ReadLine

            Try
                CdX.Add(CSng(line(0)))
                CdY.Add(CSng(line(1)))
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & CdDim + 1 & " (" & CdFile.FullPath & ")", MsgSrc, CdFile.FullPath)
                file.Close()
                Return False
            End Try

        Loop

        file.Close()

        If CdDim < 1 Then
            WorkerMsg(tMsgID.Err, "Cd input file invalid! Two or more lines required! (" & CdFile.FullPath & ")", MsgSrc, CdFile.FullPath)
            Return False
        End If

        Return True

    End Function

    Public Function Cd(ByVal x As Single) As Single
        Return CdIntpol(x) * siCd0
    End Function

    Public Function Cd() As Single
        Return siCd0
    End Function

    Private Function CdIntpol(ByVal x As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If CdX(0) >= x Then
            If CdX(0) > x Then
                If CdMode = tCdMode.CdOfBeta Then
                    MODdata.ModErrors.CdExtrapol = "β= " & x
                Else
                    MODdata.ModErrors.CdExtrapol = "v= " & x
                End If
            End If
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While CdX(i) < x And i < CdDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If CdX(i) < x Then
            If CdMode = tCdMode.CdOfBeta Then
                MODdata.ModErrors.CdExtrapol = "β= " & x
            Else
                MODdata.ModErrors.CdExtrapol = "v= " & x
            End If
        End If

lbInt:
        'Interpolation
        Return (x - CdX(i - 1)) * (CdY(i) - CdY(i - 1)) / (CdX(i) - CdX(i - 1)) + CdY(i - 1)

    End Function

#End Region

#Region "Retarder"

    Private Function RtInit() As Boolean
        Dim file As cFile_V3
        Dim MsgSrc As String
        Dim line As String()

        MsgSrc = "VEH/RtInit"

        If RtType = tRtType.None Then Return True

        'Read Inputfile
        file = New cFile_V3
        If Not file.OpenRead(RtFile.FullPath) Then
            WorkerMsg(tMsgID.Err, "Failed to read Retarder input file! (" & RtFile.FullPath & ")", MsgSrc)
            Return False
        End If

        RtDim = -1
        Do While Not file.EndOfFile

            RtDim += 1
            line = file.ReadLine

            Try
                RtnU.Add(CSng(line(0)))
                RtM.Add(CSng(line(1)))
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & RtDim + 1 & " (" & RtFile.FullPath & ")", MsgSrc, RtFile.FullPath)
                file.Close()
                Return False
            End Try

        Loop

        file.Close()

        If RtDim < 1 Then
            WorkerMsg(tMsgID.Err, "Retarder input file invalid! Two or more lines required! (" & RtFile.FullPath & ")", MsgSrc, RtFile.FullPath)
            Return False
        End If

        Return True

    End Function


    Public Function RtPeLoss(ByVal v As Single, ByVal Gear As Integer) As Single
        Dim M As Single
        Dim nU As Single

        Select Case RtType

            Case tRtType.Primary
                nU = (60 * v) / (VEH.Dreifen * Math.PI) * VEH.Igetr(0) * VEH.Igetr(Gear) * RtRatio

            Case tRtType.Secondary
                nU = (60 * v) / (VEH.Dreifen * Math.PI) * VEH.Igetr(0) * RtRatio

            Case Else 'tRtType.None
                Return 0

        End Select

        M = RtIntpol(nU)

        Return M * nU * 2 * Math.PI / 60 / 1000

    End Function

    Private Function RtIntpol(ByVal nU As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If RtnU(0) >= nU Then
            If RtnU(0) > nU Then MODdata.ModErrors.RtExtrapol = "n= " & nU & " [1/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While RtnU(i) < nU And i < RtDim
            i += 1
        Loop

        'Extrapolation for x> x(imax)
        If RtnU(i) < nU Then MODdata.ModErrors.RtExtrapol = "n= " & nU & " [1/min]"

lbInt:
        'Interpolation
        Return (nU - RtnU(i - 1)) * (RtM(i) - RtM(i - 1)) / (RtnU(i) - RtnU(i - 1)) + RtM(i - 1)

    End Function

#End Region

#Region "Properties"

    Public ReadOnly Property FileList As List(Of String)
        Get
            Return MyFileList
        End Get
    End Property

    Public Property AchsI As Single
        Get
            Return siGetrI(0)
        End Get
        Set(ByVal value As Single)
            siGetrI(0) = value
        End Set
    End Property

    Public Property ganganz As Short
        Get
            Return iganganz
        End Get
        Set(value As Short)
            iganganz = value
        End Set
    End Property

    Public ReadOnly Property Igetr(ByVal x As Short) As Single
        Get
            Return siGetrI(x)
        End Get
    End Property

    Public Property Mass As Single
        Get
            Return siMass
        End Get
        Set(ByVal value As Single)
            siMass = value
        End Set
    End Property


    Public Property Loading As Single
        Get
            Return siLoading
        End Get
        Set(ByVal value As Single)
            siLoading = value
        End Set
    End Property

    Public Property Cd0 As Single
        Get
            Return siCd0
        End Get
        Set(ByVal value As Single)
            siCd0 = value
        End Set
    End Property


    Public Property Aquers As Single
        Get
            Return siAquers
        End Get
        Set(ByVal value As Single)
            siAquers = value
        End Set
    End Property

    Public Property I_mot As Single
        Get
            Return siI_mot
        End Get
        Set(ByVal value As Single)
            siI_mot = value
        End Set
    End Property


    Public ReadOnly Property m_raeder_red As Single
        Get
            Return siI_wheels / ((siDreifen / 2) ^ 2)
        End Get
    End Property

    Public Property I_wheels As Single
        Get
            Return siI_wheels
        End Get
        Set(ByVal value As Single)
            siI_wheels = value
        End Set
    End Property


    Public Property I_Getriebe As Single
        Get
            Return siI_Getriebe
        End Get
        Set(ByVal value As Single)
            siI_Getriebe = value
        End Set
    End Property


    Public Property Paux0 As Single
        Get
            Return siPaux0
        End Get
        Set(ByVal value As Single)
            siPaux0 = value
        End Set
    End Property


    Public Property Pnenn As Single
        Get
            Return siPnenn
        End Get
        Set(ByVal value As Single)
            siPnenn = value
        End Set
    End Property


    Public Property nNenn As Single
        Get
            Return sinNenn
        End Get
        Set(ByVal value As Single)
            sinNenn = value
        End Set
    End Property


    Public Property nLeerl As Single
        Get
            Return sinLeerl
        End Get
        Set(ByVal value As Single)
            sinLeerl = value
        End Set
    End Property

    Public Property Fr0 As Single
        Get
            Return siFr0
        End Get
        Set(ByVal value As Single)
            siFr0 = value
        End Set
    End Property

    Public Property Fr1 As Single
        Get
            Return siFr1
        End Get
        Set(ByVal value As Single)
            siFr1 = value
        End Set
    End Property

    Public Property Fr2 As Single
        Get
            Return siFr2
        End Get
        Set(ByVal value As Single)
            siFr2 = value
        End Set
    End Property

    Public Property Fr3 As Single
        Get
            Return siFr3
        End Get
        Set(ByVal value As Single)
            siFr3 = value
        End Set
    End Property

    Public Property Fr4 As Single
        Get
            Return siFr4
        End Get
        Set(ByVal value As Single)
            siFr4 = value
        End Set
    End Property

    Public Property fGetr As Single
        Get
            Return sifGetr
        End Get
        Set(ByVal value As Single)
            sifGetr = value
        End Set
    End Property




    Public Property Dreifen As Single
        Get
            Return siDreifen
        End Get
        Set(ByVal value As Single)
            siDreifen = value
        End Set
    End Property


    Public Property hinauf As Single
        Get
            Return sihinauf
        End Get
        Set(ByVal value As Single)
            sihinauf = value
        End Set
    End Property


    Public Property hinunter As Single
        Get
            Return sihinunter
        End Get
        Set(ByVal value As Single)
            sihinunter = value
        End Set
    End Property


    Public Property lhinauf As Single
        Get
            Return silhinauf
        End Get
        Set(ByVal value As Single)
            silhinauf = value
        End Set
    End Property


    Public Property lhinunter As Single
        Get
            Return silhinunter
        End Get
        Set(ByVal value As Single)
            silhinunter = value
        End Set
    End Property


    Public Property pspar As Single
        Get
            Return sipspar
        End Get
        Set(ByVal value As Single)
            sipspar = value
        End Set
    End Property


    Public Property pmodell As Single
        Get
            Return sipmodell
        End Get
        Set(ByVal value As Single)
            sipmodell = value
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



    Public Function VehCatConv(ByVal VehCat As tVehCat) As String
        Select Case VehCat
            Case tVehCat.Citybus
                Return "Citybus"
            Case tVehCat.Coach
                Return "Coach"
            Case tVehCat.InterurbanBus
                Return "InterurbanBus"
            Case tVehCat.Rigid
                Return "Rigid"
            Case Else  'tVehCat.Tractor
                Return "Tractor"
        End Select
    End Function

    Public Function VehCatConv(ByVal VehCat As String) As tVehCat
        Select Case UCase(Trim(VehCat))
            Case "CITYBUS"
                Return tVehCat.Citybus
            Case "COACH"
                Return tVehCat.Coach
            Case "INTERURBANBUS"
                Return tVehCat.InterurbanBus
            Case "RIGID"
                Return tVehCat.Rigid
            Case Else  '"TRACTOR"
                Return tVehCat.Tractor
        End Select
    End Function


    Public Function CdModeConv(ByVal CdMode As tCdMode) As String
        Select Case CdMode
            Case tCdMode.CdOfBeta
                Return "CdOfBeta"
            Case tCdMode.CdOfV
                Return "CdOfV"
            Case Else  'tCdMode.ConstCd0
                Return "Off"
        End Select
    End Function

    Public Function CdModeConv(ByVal CdMode As String) As tCdMode
        Select Case UCase(Trim(CdMode))
            Case "CDOFBETA"
                Return tCdMode.CdOfBeta
            Case "CDOFV"
                Return tCdMode.CdOfV
            Case Else  '"OFF"
                Return tCdMode.ConstCd0
        End Select
    End Function

    Public Function AxleConfConv(ByVal AxleConf As tAxleConf) As String
        Select Case AxleConf
            Case tAxleConf.a4x2
                Return "4x2"
            Case tAxleConf.a4x4
                Return "4x4"
            Case tAxleConf.a6x2
                Return "6x2"
            Case tAxleConf.a6x4
                Return "6x4"
            Case tAxleConf.a6x6
                Return "6x6"
            Case tAxleConf.a8x2
                Return "8x2"
            Case tAxleConf.a8x4
                Return "8x4"
            Case tAxleConf.a8x6
                Return "8x6"
            Case Else  'tAxleConf.a8x8
                Return "8x8"
        End Select
    End Function

    Public Function AxleConfConv(ByVal AxleConf As String) As tAxleConf
        Select Case UCase(Trim(AxleConf))
            Case "4X2"
                Return tAxleConf.a4x2
            Case "4X4"
                Return tAxleConf.a4x4
            Case "6X2"
                Return tAxleConf.a6x2
            Case "6X4"
                Return tAxleConf.a6x4
            Case "6X6"
                Return tAxleConf.a6x6
            Case "8X2"
                Return tAxleConf.a8x2
            Case "8X4"
                Return tAxleConf.a8x4
            Case "8X6"
                Return tAxleConf.a8x6
            Case Else '"8X8"
                Return tAxleConf.a8x8
        End Select
    End Function

    Public Function RtTypeConv(ByVal RtType As tRtType) As String
        Select Case RtType
            Case tRtType.Primary
                Return "Primary"
            Case tRtType.Secondary
                Return "Secondary"
            Case Else 'tRtType.None
                Return "None"
        End Select
    End Function

    Public Function RtTypeConv(ByVal RtType As String) As tRtType
        Select Case UCase(Trim(RtType))
            Case "PRIMARY"
                Return tRtType.Primary
            Case "SECONDARY"
                Return tRtType.Secondary
            Case Else  '"NONE"
                Return tRtType.None
        End Select
    End Function







#End Region


End Class

