Imports System.Collections.Generic

Public Class cDeclaration
    Public Active As Boolean
    Public CurrentMission As cMission
    Public Missions As Dictionary(Of tMission, cMission)
    Public SegmentTable As cSegmentTable
    Public SegRef As cSegmentTableEntry

    Public Const SSspeed As Single = 5
    Public Const SStime As Single = 5
    Public Const SSdelay As Single = 5
    Public Const LACa As Single = -0.5
    Public Const LACvmin As Single = 50

    Public Const TqResv As Single = 20
    Public Const TqResvStart As Single = 20
    Public Const StartSpeed As Single = 2
    Public Const StartAcc As Single = 0.6
    Public Const GbInertia As Single = 0

    Public Const RRCTr As Single = 0.00555
    Public Const FzISOTr As Single = 37500

    Public Const AirDensity As Single = 1.17    'HILS Default Silberholz
    Public Const FuelDens As Single = 0.832
    Public Const CO2perFC As Single = 3.16

    Public WHTCresults As Dictionary(Of tWHTCpart, Single)

    Public Sub New()
        Active = True
    End Sub

    Public Function Init() As Boolean

        Dim file As New cFile_V3
        Dim mc0 As cMission
        Dim mt0 As tMission
        Dim ste0 As cSegmentTableEntry
        Dim line As String()
        Dim i As Integer
        Dim a As Integer
        Dim s0 As String
        Dim s As String()
        Dim TrS As Single
        Dim TrA As Single

        Dim BodyTrWeightList As List(Of String)
        Dim LoadingList As List(Of String)
        Dim AxleShares As List(Of String)
        Dim AxleSharesTr As List(Of String)
        Dim l0 As List(Of Single)
        Dim WHTCWF As List(Of String)
        Dim dWHTCWF As Dictionary(Of tWHTCpart, Single)

        'Initialize
        Missions = New Dictionary(Of tMission, cMission)
        SegmentTable = New cSegmentTable

        If Not IO.Directory.Exists(MyDeclPath) Then
            GUImsg(tMsgID.Err, "Failed to load Declaration Config!")
            Return False
        End If

        'Init Missionlist
        mc0 = New cMission
        mc0.MissionID = tMission.LongHaul
        mc0.NameStr = "Long Haul"
        mc0.CyclePath = MyDeclPath & "MissionCycles\Long_Haul.vdri"
        Missions.Add(mc0.MissionID, mc0)
        SegmentTable.MissionList.Add(mc0.MissionID)

        mc0 = New cMission
        mc0.MissionID = tMission.RegionalDelivery
        mc0.NameStr = "Regional Delivery"
        mc0.CyclePath = MyDeclPath & "MissionCycles\Regional_Delivery.vdri"
        Missions.Add(mc0.MissionID, mc0)
        SegmentTable.MissionList.Add(mc0.MissionID)

        mc0 = New cMission
        mc0.MissionID = tMission.UrbanDelivery
        mc0.NameStr = "Urban Delivery"
        mc0.CyclePath = MyDeclPath & "MissionCycles\Urban_Delivery.vdri"
        Missions.Add(mc0.MissionID, mc0)
        SegmentTable.MissionList.Add(mc0.MissionID)

        mc0 = New cMission
        mc0.MissionID = tMission.MunicipalUtility
        mc0.NameStr = "Municipal Utility"
        mc0.CyclePath = MyDeclPath & "MissionCycles\Municipal_Utility.vdri"
        Missions.Add(mc0.MissionID, mc0)
        SegmentTable.MissionList.Add(mc0.MissionID)

        mc0 = New cMission
        mc0.MissionID = tMission.Construction
        mc0.NameStr = "Construction"
        mc0.CyclePath = MyDeclPath & "MissionCycles\Construction.vdri"
        Missions.Add(mc0.MissionID, mc0)
        SegmentTable.MissionList.Add(mc0.MissionID)

        mc0 = New cMission
        mc0.MissionID = tMission.HeavyUrban
        mc0.NameStr = "HeavyUrban"
        mc0.CyclePath = MyDeclPath & "MissionCycles\Citybus_Heavy_Urban.vdri"
        Missions.Add(mc0.MissionID, mc0)
        SegmentTable.MissionList.Add(mc0.MissionID)

        mc0 = New cMission
        mc0.MissionID = tMission.Urban
        mc0.NameStr = "Urban"
        mc0.CyclePath = MyDeclPath & "MissionCycles\Citybus_Urban.vdri"
        Missions.Add(mc0.MissionID, mc0)
        SegmentTable.MissionList.Add(mc0.MissionID)


        mc0 = New cMission
        mc0.MissionID = tMission.Suburban
        mc0.NameStr = "Suburban"
        mc0.CyclePath = MyDeclPath & "MissionCycles\Citybus_Suburban.vdri"
        Missions.Add(mc0.MissionID, mc0)
        SegmentTable.MissionList.Add(mc0.MissionID)

        mc0 = New cMission
        mc0.MissionID = tMission.Interurban
        mc0.NameStr = "Interurban"
        mc0.CyclePath = MyDeclPath & "MissionCycles\Interurban_Bus.vdri"
        Missions.Add(mc0.MissionID, mc0)
        SegmentTable.MissionList.Add(mc0.MissionID)


        mc0 = New cMission
        mc0.MissionID = tMission.Coach
        mc0.NameStr = "Coach"
        mc0.CyclePath = MyDeclPath & "MissionCycles\Coach.vdri"
        Missions.Add(mc0.MissionID, mc0)
        SegmentTable.MissionList.Add(mc0.MissionID)


        'Segment Table
        If Not file.OpenRead(MyDeclPath & "SegmentTable.csv") Then
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Segment Table)!")
            Return False
        End If

        Try
            'Header
            line = file.ReadLine

            'Data
            Do While Not file.EndOfFile
                line = file.ReadLine

                If CBool(line(0)) Then
                    ste0 = New cSegmentTableEntry
                    BodyTrWeightList = New List(Of String)
                    LoadingList = New List(Of String)
                    AxleShares = New List(Of String)
                    AxleSharesTr = New List(Of String)
                    WHTCWF = New List(Of String)

                    ste0.VehCat = ConvVehCat(line(1))

                    If ste0.VehCat = tVehCat.Undef Then
                        file.Close()
                        GUImsg(tMsgID.Err, "Failed to load Declaration Config (Segment Table)! " & line(1) & " is no valid Vehicle Configuration.")
                        Return False
                    End If

                    ste0.AxleConf = ConvAxleConf(line(2))

                    If ste0.AxleConf = tAxleConf.Undef Then
                        file.Close()
                        GUImsg(tMsgID.Err, "Failed to load Declaration Config (Segment Table)! " & line(2) & " is no valid Axle Configuration.")
                        Return False
                    End If

                    ste0.MinGVW = CSng(line(3))
                    ste0.MaxGVW = CSng(line(4))
                    ste0.HDVclass = line(5)
                    ste0.VACCfile = MyDeclPath & "VACC\" & line(6)
                    ste0.VCDVfile = MyDeclPath & "VCDV\" & line(7)

                    AxleShares.Add(line(8))    'Long Haul
                    For Each mt0 In SegmentTable.MissionList  'Other cycles
                        If mt0 <> tMission.LongHaul Then AxleShares.Add(line(9))
                    Next

                    AxleSharesTr.Add(line(10))   'Long Haul
                    For Each mt0 In SegmentTable.MissionList  'Other cycles
                        If mt0 <> tMission.LongHaul Then AxleSharesTr.Add(line(11))
                    Next

                    ste0.LongHaulRigidTrailer = (Trim(line(10)) <> "0/0" And Trim(line(11)) = "0/0" And ste0.VehCat = tVehCat.RigidTruck)

                    i = 11
                    For Each mt0 In SegmentTable.MissionList
                        i += 1
                        ste0.UseMission.Add(CBool(line(i)))
                    Next
                    For Each mt0 In SegmentTable.MissionList
                        i += 1
                        BodyTrWeightList.Add(line(i))
                    Next
                    For Each mt0 In SegmentTable.MissionList
                        i += 1
                        LoadingList.Add(line(i))
                    Next

                    For Each mt0 In SegmentTable.MissionList
                        i += 1
                        WHTCWF.Add(line(i))
                    Next

                    For i = 0 To SegmentTable.MissionList.Count - 1
                        If ste0.UseMission(i) Then
                            ste0.Missions.Add(SegmentTable.MissionList(i))
                            ste0.Loading.Add(SegmentTable.MissionList(i), LoadingList(i))
                            ste0.BodyTrWeight.Add(SegmentTable.MissionList(i), BodyTrWeightList(i))

                            l0 = New List(Of Single)
                            For Each s0 In AxleShares(i).Split("/")
                                l0.Add(CSng(s0))
                            Next
                            ste0.AxleShares.Add(SegmentTable.MissionList(i), l0)

                            l0 = New List(Of Single)

                            TrS = AxleSharesTr(i).Split("/")(0)
                            TrA = AxleSharesTr(i).Split("/")(1)

                            For a = 1 To TrA
                                l0.Add(TrS / TrA)
                            Next

                            ste0.AxleSharesTr.Add(SegmentTable.MissionList(i), l0)

                            s = WHTCWF(i).Split("/")
                            dWHTCWF = New Dictionary(Of tWHTCpart, Single)
                            dWHTCWF.Add(tWHTCpart.Urban, CSng(s(0)) / 100)
                            dWHTCWF.Add(tWHTCpart.Rural, CSng(s(1)) / 100)
                            dWHTCWF.Add(tWHTCpart.Motorway, CSng(s(2)) / 100)

                            ste0.WHTCWF.Add(SegmentTable.MissionList(i), dWHTCWF)

                        End If
                    Next

                    SegmentTable.SegTableEntries.Add(ste0)

                End If

            Loop
        Catch ex As Exception
            file.Close()
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Segment Table)! " & ex.Message)
            Return False
        End Try

        Cfg.DeclInit()

        GUImsg(tMsgID.Normal, "Declaration Config loaded.")

        Return True

    End Function

    Public Function GetEngInertia(ByVal Displ As Single) As Single
        Return 0.41 + 0.27 * (Displ / 1000)
    End Function

    Public Function TracInt(ByVal Gearbox As tGearbox) As Single
        Select Case Gearbox
            Case tGearbox.Manual
                Return 1

            Case tGearbox.SemiAutomatic
                Return 0.8

            Case Else 'tGearbox.Automatic
                Return 0.8

        End Select
    End Function

    Public Function SkipGears(ByVal Gearbox As tGearbox) As Boolean
        If Gearbox = tGearbox.Automatic Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Function ShiftInside(ByVal Gearbox As tGearbox) As Boolean
        If Gearbox = tGearbox.SemiAutomatic Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function ShiftTime(ByVal Gearbox As tGearbox) As Single
        Select Case Gearbox
            Case tGearbox.Manual
                Return 3

            Case tGearbox.SemiAutomatic
                Return 2

            Case Else 'tGearbox.Automatic
                Return 2

        End Select
    End Function

    Public Function SetRef() As Boolean
        Return SegmentTable.SetRef(SegRef, VEH.VehCat, VEH.AxleConf, VEH.MassMax)
    End Function


    ''' <summary>
    ''' Init Vehicle for current mission. Must happen before setting loading in CalcInitLoad
    ''' </summary>
    ''' <param name="CycleIndex"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CalcInitCycle(ByVal CycleIndex As Integer) As Boolean

        CurrentMission = Missions(SegRef.Missions(CycleIndex))

        If Not VEH.DeclInit Then Return False

        Return True

    End Function

    ''' <summary>
    ''' Set Loading. Mission-based initialisation (CalcInitCycle) must already be done before running this.
    ''' </summary>
    ''' <param name="Loading"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CalcInitLoad(ByVal Loading As tLoading) As Boolean

        If Not VEH.DeclInitLoad(Loading) Then Return False

        Return True

    End Function


    Public Function WHTCinit() As Boolean
        Dim path As String
        Dim file As New cFile_V3
        Dim line As String()
        Dim nU As Single
        Dim Tq As Single
        Dim MsgSrc As String

        MsgSrc = "WHTCcor/Init"

        path = MyDeclPath & "WHTC.csv"
        If Not file.OpenRead(path) Then
            WorkerMsg(tMsgID.Err, "Failed to load WHTC cycle!", MsgSrc)
            Return False
        End If

        DRI = New cDRI
        DRI.Values = New Dictionary(Of tDriComp, List(Of Double))
        DRI.Values.Add(tDriComp.nU, New List(Of Double))
        DRI.Values.Add(tDriComp.Pe, New List(Of Double))
        DRI.Nvorg = True
        DRI.Pvorg = True

        Do While Not file.EndOfFile
            line = file.ReadLine

            nU = line(1)
            Tq = line(2)

            'Denorm
            nU = nU * 0.01 * (0.45 * ENG.Nlo + 0.45 * ENG.Npref + 0.1 * ENG.nHi - ENG.Nidle) * 2.0327 + ENG.Nidle

            If Tq < 0 Then
                Tq = -90000000000.0
            Else
                Tq = Tq * 0.01 * (FLD(0).Tq(nU))
            End If

            DRI.Values(tDriComp.nU).Add(nU)
            DRI.Values(tDriComp.Pe).Add(nMtoPe(nU, Tq))

        Loop

        DRI.tDim = DRI.Values(tDriComp.nU).Count - 1

        Return True

    End Function

    Public Sub WHTCcorrCalc()
        Dim sum As Double
        Dim Psum As Double
        Dim i As Integer
        Dim FC As List(Of Single)

        WHTCresults = New Dictionary(Of tWHTCpart, Single)

        FC = MODdata.lFC


        'Urban
        sum = 0
        Psum = 0
        For i = 0 To 899
            sum += FC(i)
            Psum += Math.Max(0, MODdata.Pe(i))
        Next
        WHTCresults.Add(tWHTCpart.Urban, sum / Psum)

        'Rural
        sum = 0
        Psum = 0
        For i = 900 To 1379
            sum += FC(i)
            Psum += Math.Max(0, MODdata.Pe(i))
        Next
        WHTCresults.Add(tWHTCpart.Rural, sum / Psum)

        'Motorway
        sum = 0
        Psum = 0
        For i = 1380 To 1799
            sum += FC(i)
            Psum += Math.Max(0, MODdata.Pe(i))
        Next
        WHTCresults.Add(tWHTCpart.Motorway, sum / Psum)

    End Sub

End Class

Public Class cMission
    Public MissionID As tMission
    Public NameStr As String
    Public CyclePath As String
End Class

Public Class cSegmentTable
    Public SegTableEntries As New List(Of cSegmentTableEntry)
    Public MissionList As New List(Of tMission)

    Public Function SetRef(ByRef SegTableEntryRef As cSegmentTableEntry, ByVal VehCat As tVehCat, ByVal AxleConf As tAxleConf, ByVal MaxMass As Single) As Boolean
        Dim s0 As cSegmentTableEntry

        For Each s0 In SegTableEntries
            If s0.VehCat = VehCat And s0.AxleConf = AxleConf And s0.MaxGVW > MaxMass And s0.MinGVW <= MaxMass Then
                SegTableEntryRef = s0
                Return True
            End If
        Next

        SegTableEntryRef = Nothing
        Return False

    End Function

End Class

Public Class cSegmentTableEntry
    Public VehCat As tVehCat
    Public AxleConf As tAxleConf
    Public MinGVW As Single
    Public MaxGVW As Single
    Public VehClass As String
    Public Missions As New List(Of tMission)
    Public UseMission As New List(Of Boolean)
    Public HDVclass As String
    Public VACCfile As String
    Public VCDVfile As String
    Public BodyTrWeight As New Dictionary(Of tMission, String)
    Public Loading As New Dictionary(Of tMission, String)
    Public AxleShares As New Dictionary(Of tMission, List(Of Single))
    Public AxleSharesTr As New Dictionary(Of tMission, List(Of Single))
    Public WHTCWF As New Dictionary(Of tMission, Dictionary(Of tWHTCpart, Single))
    Public LongHaulRigidTrailer As Boolean

    Public Function GetCycles() As List(Of String)
        Dim l As New List(Of String)
        Dim m As tMission

        For Each m In Missions
            l.Add(Declaration.Missions(m).CyclePath)
        Next

        Return l

    End Function

    Public Function GetBodyTrWeight(ByVal Mission As tMission) As Single

        'Check if Config is valid
        If BodyTrWeight.ContainsKey(Mission) AndAlso IsNumeric(BodyTrWeight(Mission)) Then
            Return CSng(BodyTrWeight(Mission))
        Else
            Return -1
        End If

    End Function

    Public Function GetLoading(ByVal Mission As tMission, ByVal MassMax As Single) As Single

        'Check if Config is valid
        If Loading.ContainsKey(Mission) Then
            If Not (Loading(Mission) = "f" OrElse IsNumeric(Loading(Mission))) Then
                Return -1
            End If
        Else
            Return -1
        End If

        'Return Loading
        If HDVclass < 4 Then
            If Mission = tMission.LongHaul Then
                Return 588.2 * MassMax * 1000 - 2511.8
            Else
                Return 394.1 * MassMax * 1000 - 1705.9
            End If
        Else
            Return CSng(Loading(Mission))
        End If

    End Function



End Class

Public Class cReport
    Private MyChart As
End Class

