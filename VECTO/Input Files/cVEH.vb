Imports System.Collections.Generic

Public Class cVEH

    'V1.1 MassMax is now saved in [t] instead of [kg]

    Private Const FormatVersion As String = "1.1"
    Private FileVersion As String

    Private sFilePath As String
    Private MyPath As String

    Public Mass As Single
    Public Loading As Single
    Private siI_wheels As Single
    Private siFr0 As Single      '<= wird aus Achs-RRC-Werten berechnet
    Private siDreifen As Single

    Public Cd0Tr As Single
    Public Aquers0Tr As Single

    Public Cd0Rig As Single
    Public Aquers0Rig As Single

    Private Cd0Act As Single
    Private AquersAct As Single


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

        MyFileList = New List(Of String)

        '.vcdv  / .vcdb
        If Me.CdMode <> tCdMode.ConstCd0 Then MyFileList.Add(Me.CdFile.FullPath)

        'Retarder
        If Me.RtType <> tRtType.None Then MyFileList.Add(Me.RtFile.FullPath)

        Return True

    End Function



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
        Mass = 0
        MassExtra = 0
        Loading = 0
        Cd0Tr = 0
        Aquers0Tr = 0
        Cd0Act = Cd0Tr
        AquersAct = Aquers0Tr
        Cd0Rig = 0
        Aquers0Rig = 0
        CdFile.Clear()
        CdMode = tCdMode.ConstCd0
        CdX.Clear()
        CdY.Clear()
        CdDim = -1
        siI_wheels = 0

        siFr0 = 0
        siDreifen = 0

        siGetrI = New List(Of Single)
        GetrEffDef = New List(Of Boolean)
        GetrEff = New List(Of Single)
        GetrMap = New List(Of cSubPath)

        MyGBmaps = Nothing
   
        RtType = tRtType.None
        RtRatio = 0
        RtnU.Clear()
        RtM.Clear()
        RtFile.Clear()
        RRCs.Clear()
        VehCat = tVehCat.RigidTruck
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

        Mass = CSng(file.ReadLine(0))
        MassExtra = CSng(file.ReadLine(0))
        Loading = CSng(file.ReadLine(0))

        Cd0Tr = CSng(file.ReadLine(0))
        Aquers0Tr = CSng(file.ReadLine(0))

        Cd0Act = Cd0Tr
        AquersAct = Aquers0Tr

        siI_wheels = CSng(file.ReadLine(0))
        siDreifen = CSng(file.ReadLine(0))

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
            MassMax = CSng(file.ReadLine(0)) / 1000
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

    Public Function ReadFile() As Boolean
        Dim JSON As New cJSON
        Dim dic As Object

        Dim MsgSrc As String


        MsgSrc = "VEH/ReadFile"

        'Flag for "File is not JSON" Warnings        
        NoJSON = False

        SetDefault()

        If Not JSON.ReadFile(sFilePath) Then
            NoJSON = True
            Try
                Return ReadFileOld()
            Catch ex As Exception
                Return False
            End Try
        End If

        Try

            FileVersion = JSON.Content("Header")("FileVersion")

            Mass = JSON.Content("Body")("CurbWeight")
            MassExtra = JSON.Content("Body")("CurbWeightExtra")
            Loading = JSON.Content("Body")("Loading")
            MassMax = JSON.Content("Body")("MassMax")
            If FileVersion = "1.0" Then MassMax /= 1000

            Cd0Tr = JSON.Content("Body")("Cd")
            Aquers0Tr = JSON.Content("Body")("CrossSecArea")

            If Not JSON.Content("Body")("CdRigid") Is Nothing Then Cd0Rig = JSON.Content("Body")("CdRigid")
            If Not JSON.Content("Body")("CrossSecAreaRigid") Is Nothing Then Aquers0Rig = JSON.Content("Body")("CrossSecAreaRigid")

            Cd0Act = Cd0Tr
            AquersAct = Aquers0Tr

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

            AxleConf = ConvAxleConf(JSON.Content("Body")("AxleConfig")("Type").ToString)
            For Each dic In JSON.Content("Body")("AxleConfig")("Axles")
                RRCs.Add(New Single() {dic("AxleWeightShare"), CSng(dic("TwinTyres")), dic("RRCISO"), dic("FzISO")})
            Next

            VehCat = ConvVehCat(JSON.Content("Body")("VehCat").ToString)

        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to read Vehicle file! " & ex.Message, MsgSrc)
            Return False
        End Try

        Return True



    End Function

    Public Function DeclInit() As Boolean
        Dim al As List(Of Single)
        Dim i As Integer
        Dim a As Single
        Dim MissionID As tMission
        Dim MsgSrc As String

        MsgSrc = "VEH/DeclInit"

        MissionID = Declaration.CurrentMission.MissionID

        MassExtra = Declaration.SegRef.GetBodyTrWeight(MissionID)


        al = Declaration.SegRef.AxleShares(MissionID)

        If al.Count > RRCs.Count Then
            WorkerMsg(tMsgID.Err, "Invalid number of axles! Defined: " & RRCs.Count & ", required: " & al.Count, MsgSrc)
            Return False
        End If

        i = -1
        For Each a In al
            i += 1
            RRCs(i)(0) = a / 100
        Next

        'Remove non-Truck axles
        Do While RRCs.Count > al.Count
            RRCs.RemoveAt(RRCs.Count - 1)
        Loop


        '(Semi-) Trailer
        If Not Declaration.SegRef.LongHaulRigidTrailer OrElse MissionID = tMission.LongHaul Then
            al = Declaration.SegRef.AxleSharesTr(MissionID)
            For Each a In al
                RRCs.Add(New Single() {a / 100, 0, cDeclaration.RRCTr, cDeclaration.FzISOTr})
            Next
        End If



        CdMode = tCdMode.CdOfV

        CdFile.Init(MyPath, Declaration.SegRef.VCDVfile)

        If Declaration.SegRef.LongHaulRigidTrailer Then

            If MissionID = tMission.LongHaul Then
                Cd0Act = Cd0Tr
                AquersAct = Aquers0Tr
            Else
                Cd0Act = Cd0Rig
                AquersAct = Aquers0Rig
            End If

        Else

            If Declaration.SegRef.VehCat = tVehCat.RigidTruck Then
                Cd0Act = Cd0Rig
                AquersAct = Aquers0Rig
            Else
                Cd0Act = Cd0Tr
                AquersAct = Aquers0Tr
            End If

        End If

        Return True

    End Function

    Public Function DeclInitLoad(ByVal LoadingID As tLoading) As Boolean
        Dim lmax As Single
        Dim MissionID As tMission
        Dim MsgSrc As String

        MsgSrc = "VEH/DeclInit"

        MissionID = Declaration.CurrentMission.MissionID


        lmax = MassMax * 1000 - Mass - MassExtra

        Select Case LoadingID
            Case tLoading.FullLoaded
                Loading = lmax

            Case tLoading.RefLoaded
                Loading = Declaration.SegRef.GetLoading(MissionID, MassMax)
                If Loading < 0 Then
                    WorkerMsg(tMsgID.Err, "Invalid loading in segement table!", MsgSrc)
                    Return False
                End If

                If Loading > lmax Then
                    WorkerMsg(tMsgID.Warn, "Reference loading > Max. loading! Using max. loading.", MsgSrc)
                    Loading = lmax
                End If

            Case tLoading.EmptyLoaded
                Loading = 0

            Case Else ' tLoading.EmptyLoaded
                WorkerMsg(tMsgID.Err, "tLoading.UserDefLoaded not allowed!", MsgSrc)
                Return False

        End Select

        Return True

    End Function


    Public Function SaveFile() As Boolean
        Dim sl As Single()
        Dim dic As Dictionary(Of String, Object)
        Dim dic0 As Dictionary(Of String, Object)
        Dim ls As List(Of Dictionary(Of String, Object))
        Dim JSON As New cJSON


        'Header
        dic = New Dictionary(Of String, Object)
        dic.Add("CreatedBy", Lic.LicString & " (" & Lic.GUID & ")")
        dic.Add("Date", Now.ToString)
        dic.Add("AppVersion", VECTOvers)
        dic.Add("FileVersion", FormatVersion)
        JSON.Content.Add("Header", dic)

        'Body
        dic = New Dictionary(Of String, Object)

        dic.Add("VehCat", ConvVehCat(VehCat))

        dic.Add("CurbWeight", Mass)
        dic.Add("CurbWeightExtra", MassExtra)
        dic.Add("Loading", Loading)
        dic.Add("MassMax", MassMax)

        dic.Add("Cd", Cd0Tr)
        dic.Add("CrossSecArea", Aquers0Tr)

        dic.Add("CdRigid", Cd0Rig)
        dic.Add("CrossSecAreaRigid", Aquers0Rig)

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
        dic0.Add("Type", ConvAxleConf(AxleConf))
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

        'Cd-Init
        If Not CdInit() Then Return False

        'Transmission Loss Maps
        If Not VEH.TrLossMapInit Then
            WorkerMsg(tMsgID.Err, "Failed to initialize Transmission Loss Maps!", MsgSrc)
            Return False
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

            If sl(2) < -0.000001 Then
                WorkerMsg(tMsgID.Err, "Invalid RRC value! (" & sl(2) & ")", MsgSrc, "<GUI>" & sFilePath)
                Return False
            End If

            If sl(3) < 0.00001 Then
                WorkerMsg(tMsgID.Err, "Invalid FzISO value! (" & sl(3) & ")", MsgSrc, "<GUI>" & sFilePath)
                Return False
            End If

            If CBool(sl(1)) Then
                nrwheels = 4
            Else
                nrwheels = 2
            End If
            RRC += sl(0) * (sl(2) * ((Loading + Mass + MassExtra) * sl(0) * 9.81 / (sl(3) * nrwheels)) ^ (0.9 - 1))     'Beta=0.9
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
        Dim dM As Single
        Dim P_In As Single
        Dim P_Loss As Single
        Dim EffSum As Single
        Dim Anz As Integer
        Dim EffDiffSum As Single = 0
        Dim AnzDiff As Integer = 0

        Dim MinG As Single
        Dim plossG As Single

        Dim MsgSrc As String

        MyGBmaps = New List(Of cDelaunayMap)
        file = New cFile_V3

        For i = 0 To GBX.GearCount

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
                        'PHEM:    n, PeIn, PeOut => x=n, y=PeOut, z=PeIn
                        'PHEM: GBmap0.AddPoints(CDbl(line(0)) * n_norm, CDbl(line(2)) * Pe_norm, CDbl(line(1)) * Pe_norm) 
                        'old version: VECTO: n, M_in, M_loss => x=n, y=PeOut, z=PeIn 
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

                    If GBX.IsTCgear(i) Then

                        GetrEff(i) = -1

                    Else

                        EffSum = 0
                        Anz = 0

                        dnU = (2 / 3) * (ENG.Nrated - ENG.Nidle) / 10
                        nU = ENG.Nidle + dnU

                        Do While nU <= ENG.Nrated

                            dM = nPeToM(nU, (2 / 3) * FLD(i).Pfull(nU) / 10)
                            M_in = nPeToM(nU, (1 / 3) * FLD(i).Pfull(nU))

                            Do While M_in <= nPeToM(nU, FLD(i).Pfull(nU))

                                P_In = nMtoPe(nU, M_in)

                                P_Loss = IntpolPeLossFwd(i, nU, P_In, False)

                                EffSum += (P_In - P_Loss) / P_In
                                Anz += 1


                                plossG = P_Loss
                                MinG = M_in


                                'Axle
                                P_In -= P_Loss
                                P_Loss = IntpolPeLossFwd(0, nU / VEH.Igetr(i), P_In, False)
                                EffDiffSum += (P_In - P_Loss) / P_In
                                AnzDiff += 1

                                If MODdata.ModErrors.TrLossMapExtr <> "" Then
                                    WorkerMsg(tMsgID.Err, "Transmission loss map does not cover full engine operating range!", MsgSrc)
                                    WorkerMsg(tMsgID.Err, MODdata.ModErrors.TrLossMapExtr, MsgSrc)
                                    WorkerMsg(tMsgID.Err, "nU_In(GB)=" & nU & " [1/min]", MsgSrc)
                                    WorkerMsg(tMsgID.Err, "M_In(GB)=" & MinG & " [Nm]", MsgSrc)
                                    WorkerMsg(tMsgID.Err, "P_Loss(GB)=" & plossG & " [kW]", MsgSrc)
                                    WorkerMsg(tMsgID.Err, "nU_In(axle)=" & CStr(nU / VEH.Igetr(i)) & " [1/min]", MsgSrc)
                                    WorkerMsg(tMsgID.Err, "M_In(axle)=" & CStr(nPeToM(nU / VEH.Igetr(i), P_In)) & " [Nm]", MsgSrc)
                                    WorkerMsg(tMsgID.Err, "P_Loss(axle)=" & P_Loss & " [kW]", MsgSrc)
                                    Return False
                                End If

                                M_in += dM
                            Loop


                            nU += dnU
                        Loop

                        If Anz = 0 Then
                            WorkerMsg(tMsgID.Err, "Failed to calculate approx. transmission losses!", MsgSrc)
                            Return False
                        End If

                        GetrEff(i) = EffSum / Anz

                    End If

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
        Dim Ploss As Single

        Dim MsgSrc As String

        MsgSrc = "VEH/TrLossMapInterpol/G" & Gear

        If Gear = 0 Then
            GrTxt = "A"
        Else
            GrTxt = Gear.ToString
        End If

        If GetrEffDef(Gear) Or (Approx And DEV.AllowAprxTrLoss AndAlso GetrEff(Gear) > 0) Then

            If PeOut > 0 Then
                PeIn = PeOut / GetrEff(Gear)
            Else
                PeIn = PeOut * GetrEff(Gear)
            End If
            Ploss = PeIn - PeOut

        Else

            GBmap = MyGBmaps(Gear)

            Try
                'Interpolate with Original Values
                PeIn = nMtoPe(nU, GBmap.Intpol(nU, nPeToM(nU, PeOut)))
                Ploss = PeIn - PeOut

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

                        WG = PeOutX / PeIn
                        PeIn = PeOut / WG
                        Ploss = PeIn - PeOut

                    Else

                        'Drag => Drive: ERROR!
                        WorkerMsg(tMsgID.Err, "Transmission Loss Map invalid! Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" & PeIn.ToString("0.0") & " [kW], PeOut=" & PeOutX.ToString("0.0") & " [kW]", MsgSrc)
                        WorkerAbort()
                        Return 0

                    End If

                ElseIf PeOutX < 0 Then

                    If PeIn > 0 Then

                        WG = (PeIn - (PeIn - PeOutX)) / PeIn
                        PeIn = PeOut / WG
                        Ploss = PeIn - PeOut

                    ElseIf PeIn < 0 Then

                        WG = PeIn / PeOutX
                        PeIn = PeOut * WG
                        Ploss = PeIn - PeOut

                    Else

                        Ploss = Math.Abs(PeOut)

                    End If


                Else

                    If PeIn > 0 Then

                        Ploss = PeIn

                    ElseIf PeIn < 0 Then

                        'Drag => Zero: ERROR!
                        WorkerMsg(tMsgID.Err, "Transmission Loss Map invalid! Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" & PeIn.ToString("0.0") & " [kW], PeOut=" & PeOutX.ToString("0.0") & " [kW]", MsgSrc)
                        WorkerAbort()
                        Return 0
                    Else

                        Ploss = Math.Abs(PeOut)

                    End If

                End If

                MODdata.ModErrors.TrLossMapExtr = "Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], MeOut=" & nPeToM(nU, PeOut).ToString("0.00") & " [Nm]"

            End Try

        End If

        Return Math.Max(Ploss, 0)


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

        If GetrEffDef(Gear) Or (Approx And DEV.AllowAprxTrLoss AndAlso GetrEff(Gear) > 0) Then

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
        Return CdIntpol(x) * Cd0Act
    End Function

    Public Function Cd() As Single
        Return Cd0Act
    End Function

    Public Function Aquers() As Single
        Return AquersAct
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

    Public ReadOnly Property Igetr(ByVal x As Short) As Single
        Get
            Return siGetrI(x)
        End Get
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


    Public Property Fr0 As Single
        Get
            Return siFr0
        End Get
        Set(ByVal value As Single)
            siFr0 = value
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


    Public Property FilePath() As String
        Get
            Return sFilePath
        End Get
        Set(ByVal value As String)
            sFilePath = value
            MyPath = IO.Path.GetDirectoryName(sFilePath) & "\"
        End Set
    End Property










#End Region


End Class

