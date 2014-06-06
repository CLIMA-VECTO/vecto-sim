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
Imports iTextSharp.text.pdf
Imports System.IO

Public Class cDeclaration
    Public CurrentMission As cMission
    Public CurrentLoading As tLoading
    Public Missions As Dictionary(Of tMission, cMission)
    Public SegmentTable As cSegmentTable
    Public SegRef As cSegmentTableEntry

    Public Const SSspeed As Single = 5
    Public Const SStime As Single = 5
    Public Const SSdelay As Single = 5
    Public Const LACa As Single = -0.5
    Public Const LACvmin As Single = 50
    Public Const Overspeed As Single = 5
    Public Const Underspeed As Single = 5
    Public Const ECvmin As Single = 50

    Public Const TqResv As Single = 20
    Public Const TqResvStart As Single = 20
    Public Const StartSpeed As Single = 2
    Public Const StartAcc As Single = 0.6
    Public Const GbInertia As Single = 0

    Public Const RRCTr As Single = 0.00555
    Public Const FzISOTr As Single = 37500

    Public Const AirDensity As Single = 1.188
    Public Const FuelDens As Single = 0.832
    Public Const CO2perFC As Single = 3.16

    Public Const AuxESeff As Single = 0.7

    Private lPT1nU As List(Of Single)
    Private lPT1 As List(Of Single)
    Private PT1dim As Integer


    Public WHTCresults As Dictionary(Of tWHTCpart, Single)
    Public WHTCcorrFactor As Single

    Public Report As cReport

    Public AuxTechs As Dictionary(Of tAux, List(Of String))
    Public AuxPower As Dictionary(Of String, Single)

    Private AuxFanPower As Dictionary(Of String, Dictionary(Of tMission, Single))

    Private AuxSteerPumpPower As Dictionary(Of String, Dictionary(Of tMission, Single()))
    Private AuxSteepPumpFactors As Dictionary(Of String, Single())

    Private AuxHVACPower As Dictionary(Of String, Dictionary(Of tMission, Single))

    Private AuxESbase As Dictionary(Of tMission, Single)
    Public AuxESpower As Dictionary(Of String, Dictionary(Of tMission, Single))

    Private AuxPSpower As Dictionary(Of String, Dictionary(Of tMission, Single))


    Private Wheels As Dictionary(Of String, cWheel)
    Private Rims As Dictionary(Of String, cRim)

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
        Dim stl As String()
        Dim First As Boolean

        Dim BodyTrWeightList As List(Of String)
        Dim LoadingList As List(Of String)
        Dim AxleShares As List(Of String)
        Dim AxleSharesTr As List(Of String)
        Dim l0 As List(Of Single)
        Dim WHTCWF As List(Of String)
        Dim dWHTCWF As Dictionary(Of tWHTCpart, Single)

        Dim at0 As List(Of String)
        Dim AuxPower0 As Dictionary(Of tMission, Single)
        Dim STEpower0 As Dictionary(Of tMission, Single())

        Dim w0 As cWheel
        Dim r0 As cRim

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

                    ste0.TrailerOnlyInLongHaul = (Trim(line(10)) <> "0/0" And Trim(line(11)) = "0/0" And ste0.VehCat = tVehCat.RigidTruck)

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

            file.Close()

        Catch ex As Exception
            file.Close()
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Segment Table)! " & ex.Message)
            Return False
        End Try


        'Aux
        AuxTechs = New Dictionary(Of tAux, List(Of String))



        'Aux - Fan
        AuxFanPower = New Dictionary(Of String, Dictionary(Of tMission, Single))

        Try

            If Not file.OpenRead(MyDeclPath & "VAUX\Fan-Tech.csv") Then
                GUImsg(tMsgID.Err, "Failed to load Declaration Config (Fan aux config)!")
                Return False
            End If

            'Skip Header
            file.ReadLine()

            at0 = New List(Of String)

            Do While Not file.EndOfFile

                line = file.ReadLine

                at0.Add(line(0))

                AuxPower0 = New Dictionary(Of tMission, Single)

                i = 0
                For Each mt0 In SegmentTable.MissionList
                    i += 1
                    AuxPower0.Add(mt0, line(i))
                Next

                AuxFanPower.Add(line(0), AuxPower0)
            Loop

            AuxTechs.Add(tAux.Fan, at0)

            file.Close()

        Catch ex As Exception
            file.Close()
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Fan aux config)!" & ex.Message)
            Return False
        End Try



        'Aux - Steering Pump
        AuxSteerPumpPower = New Dictionary(Of String, Dictionary(Of tMission, Single()))
        AuxSteepPumpFactors = New Dictionary(Of String, Single())

        Try

            If Not file.OpenRead(MyDeclPath & "VAUX\SP-Tech.csv") Then
                GUImsg(tMsgID.Err, "Failed to load Declaration Config (Steering pump config)!")
                Return False
            End If

            'Skip Header
            file.ReadLine()

            at0 = New List(Of String)

            Do While Not file.EndOfFile

                line = file.ReadLine

                at0.Add(line(0))

                AuxSteepPumpFactors.Add(line(0), New Single() {CSng(line(1)), CSng(line(2)), CSng(line(3)), CSng(line(4))})

            Loop

            file.Close()

            If Not file.OpenRead(MyDeclPath & "VAUX\SP-Table.csv") Then
                GUImsg(tMsgID.Err, "Failed to load Declaration Config (Steering pump config)!")
                Return False
            End If

            'Skip Header
            file.ReadLine()

            Do While Not file.EndOfFile

                line = file.ReadLine

                STEpower0 = New Dictionary(Of tMission, Single())

                i = 0
                For Each mt0 In SegmentTable.MissionList
                    i += 1

                    If line(i) = "0" Then
                        STEpower0.Add(mt0, New Single() {0})
                    Else
                        stl = line(i).Split("/")
                        STEpower0.Add(mt0, New Single() {CSng(stl(0)), CSng(stl(1)), CSng(stl(2)), CSng(stl(3))})
                    End If

                Next

                AuxSteerPumpPower.Add(line(0), STEpower0)

            Loop

            AuxTechs.Add(tAux.SteerPump, at0)

            file.Close()

        Catch ex As Exception
            file.Close()
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Steering pump config)!" & ex.Message)
            Return False
        End Try


        'Aux - HVAC
        AuxHVACPower = New Dictionary(Of String, Dictionary(Of tMission, Single))

        Try

            If Not file.OpenRead(MyDeclPath & "VAUX\HVAC-Table.csv") Then
                GUImsg(tMsgID.Err, "Failed to load Declaration Config (HVAC config)!")
                Return False
            End If

            'Skip Header
            file.ReadLine()

            Do While Not file.EndOfFile

                line = file.ReadLine

                AuxPower0 = New Dictionary(Of tMission, Single)

                i = 0
                For Each mt0 In SegmentTable.MissionList
                    i += 1
                    AuxPower0.Add(mt0, line(i))
                Next

                AuxHVACPower.Add(line(0), AuxPower0)

            Loop

        Catch ex As Exception
            file.Close()
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (HVAC config)!" & ex.Message)
            Return False
        End Try

        at0 = New List(Of String)
        at0.Add("Default")
        AuxTechs.Add(tAux.HVAC, at0)


        'Aux - Electric System
        AuxESbase = New Dictionary(Of tMission, Single)
        AuxESpower = New Dictionary(Of String, Dictionary(Of tMission, Single))

        Try

            If Not file.OpenRead(MyDeclPath & "VAUX\ES-Tech.csv") Then
                GUImsg(tMsgID.Err, "Failed to load Declaration Config (Electric system config)!")
                Return False
            End If

            'Skip Header
            file.ReadLine()

            First = True
            Do While Not file.EndOfFile

                line = file.ReadLine

                AuxPower0 = New Dictionary(Of tMission, Single)

                i = 0
                For Each mt0 In SegmentTable.MissionList
                    i += 1
                    AuxPower0.Add(mt0, line(i))
                Next

                If First Then
                    AuxESbase = AuxPower0
                    First = False
                Else
                    AuxESpower.Add(line(0), AuxPower0)
                End If

            Loop

            file.Close()

        Catch ex As Exception
            file.Close()
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Electric system config)!" & ex.Message)
            Return False
        End Try

        at0 = New List(Of String)
        at0.Add("Custom Technology List")
        AuxTechs.Add(tAux.ElectricSys, at0)





        'Aux - Pneumatic System
        AuxPSpower = New Dictionary(Of String, Dictionary(Of tMission, Single))

        Try

            If Not file.OpenRead(MyDeclPath & "VAUX\PS-Table.csv") Then
                GUImsg(tMsgID.Err, "Failed to load Declaration Config (Pneumatic system config)!")
                Return False
            End If

            'Skip Header
            file.ReadLine()

            Do While Not file.EndOfFile

                line = file.ReadLine

                AuxPower0 = New Dictionary(Of tMission, Single)

                i = 0
                For Each mt0 In SegmentTable.MissionList
                    i += 1
                    AuxPower0.Add(mt0, line(i))
                Next

                AuxPSpower.Add(line(0), AuxPower0)

            Loop

        Catch ex As Exception
            file.Close()
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Pneumatic system config)!" & ex.Message)
            Return False
        End Try

        at0 = New List(Of String)
        at0.Add("Default")
        AuxTechs.Add(tAux.PneumSys, at0)





        'Default PT1 values
        lPT1nU = New List(Of Single)
        lPT1 = New List(Of Single)
        PT1dim = -1

        If Not file.OpenRead(MyDeclPath & "PT1.csv") Then
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (PT1 table)!")
            Return False
        End If

        Try

            Do While Not file.EndOfFile
                line = file.ReadLine
                PT1dim += 1
                lPT1nU.Add(CDbl(line(0)))
                lPT1.Add(CDbl(line(1)))
            Loop

        Catch ex As Exception
            file.Close()
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (PT1 table)!" & ex.Message)
            Return False
        End Try

        file.Close()



        'Wheels
        Wheels = New Dictionary(Of String, cWheel)

        If Not file.OpenRead(MyDeclPath & "wheels.csv") Then
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Wheels table)!")
            Return False
        End If

        Try

            Do While Not file.EndOfFile
                line = file.ReadLine

                w0 = New cWheel
                w0.Inertia = CSng(line(1))
                w0.Diam = CSng(line(2))
                w0.SizeA = (CInt(line(3)) = 1)

                Wheels.Add(line(0), w0)

            Loop

        Catch ex As Exception
            file.Close()
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Wheels table)!" & ex.Message)
            Return False
        End Try

        file.Close()



        'Rims
        Rims = New Dictionary(Of String, cRim)

        If Not file.OpenRead(MyDeclPath & "rims.csv") Then
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Rims table)!")
            Return False
        End If

        Try

            Do While Not file.EndOfFile
                line = file.ReadLine

                r0 = New cRim
                r0.Fa = CSng(line(1))
                r0.Fb = CSng(line(2))

                Rims.Add(line(0), r0)

            Loop

        Catch ex As Exception
            file.Close()
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Rims table)!" & ex.Message)
            Return False
        End Try

        file.Close()



        GUImsg(tMsgID.Normal, "Declaration Config loaded.")

        Return True

    End Function

    Public Function EngInertia(ByVal Displ As Single) As Single
        Return 1.3 + 0.41 + 0.27 * (Displ / 1000)
    End Function

    Public Function TracInt(ByVal Gearbox As tGearbox) As Single
        Select Case Gearbox
            Case tGearbox.Manual
                Return 2

            Case tGearbox.SemiAutomatic
                Return 1

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

    Public Function WheelsInertia(ByVal Wheel As String) As Single

        If Wheels.ContainsKey(Wheel) Then
            Return Wheels(Wheel).Inertia
        Else
            Return -1
        End If

    End Function

    Public Function rdyn(ByVal Wheel As String, ByVal Rim As String) As Single
        Dim F As Single
        Dim w As cWheel

        If Not Wheels.ContainsKey(Wheel) Then
            Return -1
        End If


        If Not Rims.ContainsKey(Rim) Then
            Return -1
        End If

        w = Wheels(Wheel)

        If w.SizeA Then
            F = Rims(Rim).Fa
        Else
            F = Rims(Rim).Fb
        End If

        Return (F * w.Diam) / (2 * Math.PI)

    End Function

    Public ReadOnly Property WheelsList As Dictionary(Of String, cWheel).KeyCollection
        Get
            Return Wheels.Keys
        End Get
    End Property

    Public ReadOnly Property RimsList As Dictionary(Of String, cRim).KeyCollection
        Get
            Return Rims.Keys
        End Get
    End Property

    Public Function ConvPicPath(ByVal HDVclass As String, ByVal LongHaul As Boolean) As String

        Select Case HDVclass

            Case 1, 2, 3
                Return MyDeclPath & "Reports\4x2r.png"

            Case 4
                If LongHaul Then
                    Return MyDeclPath & "Reports\4x2rt.png"
                Else
                    Return MyDeclPath & "Reports\4x2r.png"
                End If

            Case 5
                Return MyDeclPath & "Reports\4x2tt.png"

            Case 9
                If LongHaul Then
                    Return MyDeclPath & "Reports\6x2rt.png"
                Else
                    Return MyDeclPath & "Reports\6x2r.png"
                End If

            Case 10
                Return MyDeclPath & "Reports\6x2tt.png"

            Case Else
                Return MyDeclPath & "Reports\Undef.png"

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

        WHTCcorrFactor = Declaration.SegRef.WHTCWF(Declaration.CurrentMission.MissionID)(tWHTCpart.Urban) * ENG.WHTCurban / Declaration.WHTCresults(tWHTCpart.Urban) _
            + Declaration.SegRef.WHTCWF(Declaration.CurrentMission.MissionID)(tWHTCpart.Rural) * ENG.WHTCrural / Declaration.WHTCresults(tWHTCpart.Rural) _
            + Declaration.SegRef.WHTCWF(Declaration.CurrentMission.MissionID)(tWHTCpart.Motorway) * ENG.WHTCmw / Declaration.WHTCresults(tWHTCpart.Motorway)


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
        Dim MsgSrc As String
        Dim U As Single
        Dim F As Single
        Dim B As Single
        Dim S As Single
        Dim fU As Single
        Dim fF As Single
        Dim fB As Single
        Dim fS As Single
        Dim sl As Single()
        Dim Result As Boolean
        Dim ESsum As Single
        Dim EStech As String

        MsgSrc = "DeclInit"

        CurrentLoading = Loading

        If Not VEH.DeclInitLoad(Loading) Then Return False

        'AuxPower
        AuxPower = New Dictionary(Of String, Single)

        Result = True

        'Fan
        Try
            AuxPower.Add(sKey.AUX.Fan, AuxFanPower(VEC.AuxPaths(sKey.AUX.Fan).TechStr)(CurrentMission.MissionID) / 1000)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to initialise fan! " & ex.Message, MsgSrc)
            Result = False
        End Try

        'Steering pump
        Try
            sl = AuxSteerPumpPower(SegRef.HDVclass)(CurrentMission.MissionID)
            U = sl(0)
            F = sl(1)
            B = sl(2)
            S = sl(3)
            sl = AuxSteepPumpFactors(VEC.AuxPaths(sKey.AUX.SteerPump).TechStr)
            fU = sl(0)
            fF = sl(1)
            fB = sl(2)
            fS = sl(3)
            AuxPower.Add(sKey.AUX.SteerPump, (U * fU + F * fF + B * fB + S * fS) / 1000)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to initialise steering pump! " & ex.Message, MsgSrc)
            Result = False
        End Try

        'HVAC
        Try
            AuxPower.Add(sKey.AUX.HVAC, AuxHVACPower(SegRef.HDVclass)(CurrentMission.MissionID) / 1000)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to initialise HVAC! " & ex.Message, MsgSrc)
            Result = False
        End Try

        'Electric System
        Try

            ESsum = AuxESbase(CurrentMission.MissionID)

            For Each EStech In VEC.EStechs

                If Not AuxESpower.ContainsKey(EStech) Then
                    WorkerMsg(tMsgID.Err, "Electric system '" & EStech & "' is not supported! ", MsgSrc)
                    Result = False
                End If

                ESsum -= AuxESpower(EStech)(CurrentMission.MissionID)

            Next

            AuxPower.Add(sKey.AUX.ElecSys, ESsum / (1000 * AuxESeff))

        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to initialise electric system! " & ex.Message, MsgSrc)
            Result = False
        End Try


        'PS
        Try
            AuxPower.Add(sKey.AUX.PneumSys, AuxPSpower(SegRef.HDVclass)(CurrentMission.MissionID) / 1000)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to initialise pneumatic system! " & ex.Message, MsgSrc)
            Result = False
        End Try


        Return Result

    End Function

    Public Function PT1(ByVal nU As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If lPT1nU(0) >= nU Then
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While lPT1nU(i) < nU And i < PT1dim
            i += 1
        Loop

lbInt:
        'Interpolation
        Return (nU - lPT1nU(i - 1)) * (lPT1(i) - lPT1(i - 1)) / (lPT1nU(i) - lPT1nU(i - 1)) + lPT1(i - 1)

    End Function

    Public Sub ReportInit()

        Report = New cReport

        With Report
            .Filepath = fFileWoExt(JobFile) & ".pdf"
            .HDVclassStr = SegRef.HDVclass
            .VehCat = SegRef.VehCat
            .AxleConf = SegRef.AxleConf
            .MassMaxStr = VEH.MassMax & "t"
            .JobFile = fFILE(JobFile, True)
            .DateStr = Now.ToString
            .Creator = Lic.LicString
            .EngStr = (ENG.Displ / 1000).ToString("0.0") & " l  " & Math.Round(ENG.Pmax, 0).ToString("#") & " kW"
            .EngModelStr = ENG.ModelName
            .GbxStr = GBX.GearCount & "-Speed " & GearboxConv(GBX.gs_Type)
            .GbxModelStr = GBX.ModelName
        End With

    End Sub

    Public Sub ReportAddCycle()
        Dim mr As New cReport.cMissionResults

        mr.MissionRef = CurrentMission

        Report.CurrentMR = mr
        Report.MissionResults.Add(mr)

    End Sub

    Public Sub ReportAddResults()
        Dim lr As New cReport.cLoadingResults
        Dim t1 As Integer
        Dim t As Integer
        Dim Vquer As Single
        Dim sum As Double
        Dim d As Double

        t1 = MODdata.tDim

        'Average Speed calculation
        sum = 0
        For t = 0 To t1
            sum += MODdata.Vh.V(t)
        Next
        Vquer = 3.6 * sum / (t1 + 1)

        With lr

            .Loading = VEH.Loading / 1000
            .Speed = Vquer
            .FCkm = (100 * MODdata.FCavgFinal / Vquer) / (Cfg.FuelDens * 1000)
            .CO2km = Cfg.CO2perFC * (MODdata.FCavgFinal / Vquer)
            If VEH.Loading > 0 Then
                .FCtkm = .FCkm / .Loading
                .CO2tkm = .CO2km / .Loading
            End If
            .FCerror = MODdata.FCerror

            d = 0
            MODdata.Vh.AltIntp(d, True)
            For t = 0 To t1
                .ActualSpeed.Add(MODdata.Vh.V(t) * 3.6)
                .TargetSpeed.Add(MODdata.Vh.Vsoll(t) * 3.6)
                d += MODdata.Vh.V(t)
                .Distance.Add(CSng(d / 1000))
                .Alt.Add(MODdata.Vh.AltIntp(d, False))
                .nU.Add(MODdata.nU(t))
                .Tq.Add(nPeToM(MODdata.nU(t), MODdata.Pe(t)))
            Next

        End With

        Report.CurrentMR.Results.Add(CurrentLoading, lr)

    End Sub

    Public Function WriteReport() As Boolean

        Report.CreateCharts()

        Return Report.WritePdfs


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

        'Skip header
        file.ReadLine()

        Do While Not file.EndOfFile
            line = file.ReadLine

            nU = line(1)
            Tq = line(2)

            'Denorm
            nU = nU * 0.01 * (0.45 * FLD(0).Nlo + 0.45 * FLD(0).Npref + 0.1 * FLD(0).Nhi - ENG.Nidle) * 2.0327 + ENG.Nidle

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
        Dim MsgSrc As String

        MsgSrc = "WHTCcor/Calc"

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
        WorkerMsg(tMsgID.Normal, " > Urban: " & (sum / Psum).ToString("0.0") & " [g/kWh]", MsgSrc)

        'Rural
        sum = 0
        Psum = 0
        For i = 900 To 1379
            sum += FC(i)
            Psum += Math.Max(0, MODdata.Pe(i))
        Next
        WHTCresults.Add(tWHTCpart.Rural, sum / Psum)
        WorkerMsg(tMsgID.Normal, " > Rural: " & (sum / Psum).ToString("0.0") & " [g/kWh]", MsgSrc)

        'Motorway
        sum = 0
        Psum = 0
        For i = 1380 To 1799
            sum += FC(i)
            Psum += Math.Max(0, MODdata.Pe(i))
        Next
        WHTCresults.Add(tWHTCpart.Motorway, sum / Psum)
        WorkerMsg(tMsgID.Normal, " > Motorway: " & (sum / Psum).ToString("0.0") & " [g/kWh]", MsgSrc)


    End Sub

End Class

Public Class cWheel
    Public Inertia As Single
    Public Diam As Single
    Public SizeA As Boolean
End Class

Public Class cRim
    Public Fa As Single
    Public Fb As Single
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
    Public TrailerOnlyInLongHaul As Boolean

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
                Return 588.2 * MassMax - 2511.8
            Else
                Return 394.1 * MassMax - 1705.9
            End If
        Else
            Return CSng(Loading(Mission))
        End If

    End Function



End Class

Public Class cReport

    Public Filepath As String = ""
    Public CurrentMR As cMissionResults
    Public MissionResults As List(Of cMissionResults)
    Public HDVclassStr As String = ""
    Public VehCat As tVehCat = tVehCat.Undef
    Public AxleConf As tAxleConf = tAxleConf.Undef
    Public MassMaxStr As String = ""
    Public JobFile As String = ""
    Public DateStr As String = ""
    Public Creator As String = ""
    Public EngStr As String = ""
    Public EngModelStr As String = ""
    Public GbxStr As String = ""
    Public GbxModelStr As String = ""

    Public ChartCO2tkm As Image
    Public ChartCO2speed As Image


    Public Sub New()
        MissionResults = New List(Of cMissionResults)
    End Sub

    Public Sub CreateCharts()
        Dim mr As cMissionResults
        Dim lr As KeyValuePair(Of tLoading, cLoadingResults)
        Dim MyChart As System.Windows.Forms.DataVisualization.Charting.Chart
        Dim s As System.Windows.Forms.DataVisualization.Charting.Series
        Dim a As System.Windows.Forms.DataVisualization.Charting.ChartArea
        Dim i As Int16

        'Torque, rpm
        For Each mr In MissionResults

            MyChart = New System.Windows.Forms.DataVisualization.Charting.Chart
            MyChart.Width = 1000
            MyChart.Height = 427

            a = New System.Windows.Forms.DataVisualization.Charting.ChartArea

            s = New System.Windows.Forms.DataVisualization.Charting.Series
            s.Points.DataBindXY(FLD(0).LnU, FLD(0).LTq)
            s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
            s.BorderWidth = 3
            s.Color = Color.DarkBlue
            s.Name = "Full load curve"
            MyChart.Series.Add(s)

            s = New System.Windows.Forms.DataVisualization.Charting.Series
            s.Points.DataBindXY(FLD(0).LnU, FLD(0).LTqDrag)
            s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
            s.BorderWidth = 3
            s.Color = Color.Blue
            s.Name = "Drag curve"
            MyChart.Series.Add(s)

            's = New System.Windows.Forms.DataVisualization.Charting.Series
            's.Points.DataBindXY(GBX.Shiftpolygons(GBX.GearCount).gs_nUdown, GBX.Shiftpolygons(GBX.GearCount).gs_Mdown)
            's.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
            's.Color = Color.DarkGray
            's.BorderWidth = 3
            's.Name = "Down-Shift threshold"
            'MyChart.Series.Add(s)

            's = New System.Windows.Forms.DataVisualization.Charting.Series
            's.Points.DataBindXY(GBX.Shiftpolygons(GBX.GearCount).gs_nUup, GBX.Shiftpolygons(GBX.GearCount).gs_Mup)
            's.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
            's.Color = Color.Gray
            's.BorderWidth = 3
            's.Name = "Up-Shift threshold"
            'MyChart.Series.Add(s)

            s = New System.Windows.Forms.DataVisualization.Charting.Series
            s.Points.DataBindXY(mr.Results(tLoading.RefLoaded).nU, mr.Results(tLoading.RefLoaded).Tq)
            s.ChartType = DataVisualization.Charting.SeriesChartType.Point
            s.Color = Color.Red
            s.Name = "load points (Ref. load.)"
            MyChart.Series.Add(s)


            a.Name = "main"

            a.AxisX.Title = "engine speed [1/min]"
            a.AxisX.TitleFont = New Font("Helvetica", 20)
            a.AxisX.LabelStyle.Font = New Font("Helvetica", 20)
            a.AxisX.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None

            a.AxisY.Title = "engine torque [Nm]"
            a.AxisY.TitleFont = New Font("Helvetica", 20)
            a.AxisY.LabelStyle.Font = New Font("Helvetica", 20)
            a.AxisY.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None

            a.AxisX.Minimum = 300
            a.BorderDashStyle = DataVisualization.Charting.ChartDashStyle.Solid
            a.BorderWidth = 3


            MyChart.ChartAreas.Add(a)

            With MyChart.ChartAreas(0)
                .Position.X = 0
                .Position.Y = 0
                .Position.Width = 70
                .Position.Height = 100
            End With

            MyChart.Legends.Add("main")
            MyChart.Legends(0).Font = New Font("Helvetica", 14)
            MyChart.Legends(0).BorderColor = Color.Black
            MyChart.Legends(0).BorderWidth = 3


            'MyChart.Titles.Add("CO2 Results [g/km]")
            'MyChart.Titles(0).Font = New Font("Helvetica", 30, FontStyle.Bold)

        


            MyChart.Update()

            mr.ChartTqN = New Bitmap(MyChart.Width, MyChart.Height, Imaging.PixelFormat.Format32bppArgb)
            MyChart.DrawToBitmap(mr.ChartTqN, New Rectangle(0, 0, mr.ChartTqN.Size.Width, mr.ChartTqN.Size.Height))

        Next

        'Speed over distance
        For Each mr In MissionResults

            MyChart = New System.Windows.Forms.DataVisualization.Charting.Chart
            MyChart.Width = 2000
            MyChart.Height = 400

            a = New System.Windows.Forms.DataVisualization.Charting.ChartArea

            'Altitude
            s = New System.Windows.Forms.DataVisualization.Charting.Series
            s.Points.DataBindXY(mr.Results(tLoading.RefLoaded).Distance, mr.Results(tLoading.RefLoaded).Alt)
            s.ChartType = DataVisualization.Charting.SeriesChartType.Area
            s.Color = Color.Lavender
            s.Name = "Altitude"
            s.YAxisType = DataVisualization.Charting.AxisType.Secondary
            MyChart.Series.Add(s)

            'Target speed
            s = New System.Windows.Forms.DataVisualization.Charting.Series
            s.Points.DataBindXY(mr.Results(tLoading.RefLoaded).Distance, mr.Results(tLoading.RefLoaded).TargetSpeed)
            s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
            s.BorderWidth = 3
            s.Name = "Target speed"
            MyChart.Series.Add(s)

            For Each lr In mr.Results

                s = New System.Windows.Forms.DataVisualization.Charting.Series
                s.Points.DataBindXY(lr.Value.Distance, lr.Value.ActualSpeed)
                s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
                s.Name = ConvLoading(lr.Key)
                MyChart.Series.Add(s)

            Next

            a.Name = "main"

            a.AxisX.Title = "distance [km]"
            a.AxisX.TitleFont = New Font("Helvetica", 16)
            a.AxisX.LabelStyle.Font = New Font("Helvetica", 16)
            a.AxisX.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None
            a.AxisX.LabelStyle.Format = "0.0"

            a.AxisY.Title = "vehicle speed [km/h]"
            a.AxisY.TitleFont = New Font("Helvetica", 16)
            a.AxisY.LabelStyle.Font = New Font("Helvetica", 16)
            a.AxisY.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None

            a.AxisY2.Title = "altitude [m]"
            a.AxisY2.TitleFont = New Font("Helvetica", 16)
            a.AxisY2.LabelStyle.Font = New Font("Helvetica", 16)
            a.AxisY2.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None
            a.AxisY2.MinorGrid.Enabled = False
            a.AxisY2.MajorGrid.Enabled = False

            a.AxisX.Minimum = 0
            a.BorderDashStyle = DataVisualization.Charting.ChartDashStyle.Solid
            a.BorderWidth = 3

            MyChart.ChartAreas.Add(a)

            With MyChart.ChartAreas(0)
                .Position.X = 0
                .Position.Y = 0
                .Position.Width = 90
                .Position.Height = 100
            End With


            MyChart.Legends.Add("main")
            MyChart.Legends(0).Font = New Font("Helvetica", 14)
            MyChart.Legends(0).BorderColor = Color.Black
            MyChart.Legends(0).BorderWidth = 3
            'MyChart.Legends(0).Position.Auto = False
            MyChart.Legends(0).Position.X = 97
            MyChart.Legends(0).Position.Y = 3
            MyChart.Legends(0).Position.Width = 10
            MyChart.Legends(0).Position.Height = 40


            'MyChart.Titles.Add("CO2 Results [g/km]")
            'MyChart.Titles(0).Font = New Font("Helvetica", 30, FontStyle.Bold)



            MyChart.Update()

            mr.ChartSpeed = New Bitmap(MyChart.Width, MyChart.Height, Imaging.PixelFormat.Format32bppArgb)
            MyChart.DrawToBitmap(mr.ChartSpeed, New Rectangle(0, 0, mr.ChartSpeed.Size.Width, mr.ChartSpeed.Size.Height))

        Next

        'CO2 Bars
        MyChart = New System.Windows.Forms.DataVisualization.Charting.Chart
        a = New System.Windows.Forms.DataVisualization.Charting.ChartArea

        For Each mr In MissionResults
            s = New System.Windows.Forms.DataVisualization.Charting.Series
            s.Points.AddXY(mr.MissionRef.NameStr, mr.Results(tLoading.RefLoaded).CO2tkm)
            's.IsValueShownAsLabel = True
            s.Points(0).Label = s.Points(0).YValues(0).ToString("0.0") & " [g/tkm]"
            s.Points(0).Font = New Font("Helvetica", 20)
            s.Points(0).LabelBackColor = Color.White
            s.Name = mr.MissionRef.NameStr & " (Ref. load.)"
            MyChart.Series.Add(s)
        Next

        a.Name = "main"

        a.AxisX.Title = "Missions"
        a.AxisX.TitleFont = New Font("Helvetica", 20)
        a.AxisX.LabelStyle.Enabled = False

        a.AxisY.Title = "CO2 [g/tkm]"
        a.AxisY.TitleFont = New Font("Helvetica", 20)
        a.AxisY.LabelStyle.Font = New Font("Helvetica", 20)
        a.AxisY.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None

        a.BorderDashStyle = DataVisualization.Charting.ChartDashStyle.Solid
        a.BorderWidth = 3

        MyChart.ChartAreas.Add(a)

        MyChart.Legends.Add("main")
        MyChart.Legends(0).Font = New Font("Helvetica", 20)
        MyChart.Legends(0).BorderColor = Color.Black
        MyChart.Legends(0).BorderWidth = 3

        MyChart.Width = 1500
        MyChart.Height = 700

        MyChart.Update()

        ChartCO2tkm = New Bitmap(MyChart.Width, MyChart.Height, Imaging.PixelFormat.Format32bppArgb)
        MyChart.DrawToBitmap(ChartCO2tkm, New Rectangle(0, 0, ChartCO2tkm.Size.Width, ChartCO2tkm.Size.Height))


        'CO2 & Speed
        MyChart = New System.Windows.Forms.DataVisualization.Charting.Chart
        a = New System.Windows.Forms.DataVisualization.Charting.ChartArea

        For Each mr In MissionResults
            s = New System.Windows.Forms.DataVisualization.Charting.Series
            s.MarkerSize = 15
            s.MarkerStyle = DataVisualization.Charting.MarkerStyle.Circle
            s.ChartType = DataVisualization.Charting.SeriesChartType.Point
            i = -1
            For Each lr In mr.Results
                i += 1
                s.Points.AddXY(lr.Value.Speed, lr.Value.CO2km)
                s.Points(i).Label = lr.Value.Loading.ToString("0.0") & " t"
                If lr.Key = tLoading.RefLoaded Then
                    s.Points(i).Font = New Font("Helvetica", 16)
                Else
                    s.Points(i).MarkerSize = 10
                    s.Points(i).Font = New Font("Helvetica", 14)
                End If
                s.Points(i).LabelBackColor = Color.White
            Next
            s.Name = mr.MissionRef.NameStr
            MyChart.Series.Add(s)
        Next

        a.Name = "main"

        a.AxisX.Title = "vehicle speed [km/h]"
        a.AxisX.TitleFont = New Font("Helvetica", 20)
        a.AxisX.LabelStyle.Font = New Font("Helvetica", 20)
        a.AxisX.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None

        a.AxisY.Title = "CO2 [g/km]"
        a.AxisY.TitleFont = New Font("Helvetica", 20)
        a.AxisY.LabelStyle.Font = New Font("Helvetica", 20)
        a.AxisY.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None

        a.AxisX.Minimum = 20
        a.BorderDashStyle = DataVisualization.Charting.ChartDashStyle.Solid
        a.BorderWidth = 3

        MyChart.ChartAreas.Add(a)

        MyChart.Legends.Add("main")
        MyChart.Legends(0).Font = New Font("Helvetica", 20)
        MyChart.Legends(0).BorderColor = Color.Black
        MyChart.Legends(0).BorderWidth = 3

        'MyChart.Titles.Add("CO2 Results [g/km]")
        'MyChart.Titles(0).Font = New Font("Helvetica", 30, FontStyle.Bold)

        MyChart.Width = 1500
        MyChart.Height = 700

        MyChart.Update()

        ChartCO2speed = New Bitmap(MyChart.Width, MyChart.Height, Imaging.PixelFormat.Format32bppArgb)
        MyChart.DrawToBitmap(ChartCO2speed, New Rectangle(0, 0, ChartCO2speed.Size.Width, ChartCO2speed.Size.Height))


    End Sub

    Public Function WritePdfs() As Boolean
        Dim pdfReader As PdfReader
        Dim pdfStamper As PdfStamper
        Dim PdfTemp As String = ""
        Dim PdfTempMR As String = ""
        Dim i As Integer
        Dim pgMax As Integer
        Dim imgp As iTextSharp.text.Image
        Dim pdfContentByte As iTextSharp.text.pdf.PdfContentByte
        Dim doc As iTextSharp.text.Document
        Dim pdfpage As PdfImportedPage
        Dim pdfWriter As PdfWriter
        Dim mr As cMissionResults
        Dim lr As KeyValuePair(Of tLoading, cLoadingResults)
        Dim lstr As String = ""
        Dim temppdfs As New List(Of String)
        Dim temppath As String
        Dim pdfFormFields As AcroFields

        Select Case MissionResults.Count
            Case 2
                PdfTemp = MyDeclPath & "Reports\rep2C.pdf"
                pgMax = 3
            Case 3
                PdfTemp = MyDeclPath & "Reports\rep3C.pdf"
                pgMax = 4
        End Select

        PdfTempMR = MyDeclPath & "Reports\repMR.pdf"

        Try

            temppath = MyDeclPath & "Reports\temp0.pdf"
            temppdfs.Add(temppath)

            pdfReader = New PdfReader(PdfTemp)
            pdfStamper = New PdfStamper(pdfReader, New FileStream(temppath, FileMode.Create))

            pdfFormFields = pdfStamper.AcroFields

            pdfFormFields.SetField("version", VECTOvers)
            pdfFormFields.SetField("Job", JobFile)
            pdfFormFields.SetField("Date", DateStr)
            pdfFormFields.SetField("Created", Creator)
            pdfFormFields.SetField("Config", MassMaxStr & " " & ConvAxleConf(AxleConf) & " " & ConvVehCat(VehCat, True))
            pdfFormFields.SetField("HDVclass", "HDV Class " & HDVclassStr)
            pdfFormFields.SetField("Engine", EngStr)
            pdfFormFields.SetField("EngM", EngModelStr)
            pdfFormFields.SetField("Gearbox", GbxStr)
            pdfFormFields.SetField("GbxM", GbxModelStr)
            pdfFormFields.SetField("PageNr", "Page 1 of " & pgMax)

            i = 0
            For Each mr In MissionResults
                i += 1
                pdfFormFields.SetField("Mission" & i, mr.MissionRef.NameStr)
                With mr.Results(tLoading.RefLoaded)
                    pdfFormFields.SetField("Loading" & i, .Loading.ToString("0.0") & " t")
                    pdfFormFields.SetField("Speed" & i, .Speed.ToString("0.0") & " km/h")
                    pdfFormFields.SetField("FC" & i, .FCkm.ToString("0.0"))
                    pdfFormFields.SetField("FCt" & i, .FCtkm.ToString("0.0"))
                    pdfFormFields.SetField("CO2" & i, .CO2km.ToString("0.0"))
                    pdfFormFields.SetField("CO2t" & i, .CO2tkm.ToString("0.0"))
                End With
            Next


            'Add Images
            pdfContentByte = pdfStamper.GetOverContent(1)

            imgp = iTextSharp.text.Image.GetInstance(ChartCO2tkm, iTextSharp.text.Color.WHITE)
            imgp.ScaleAbsolute(440, 195)
            imgp.SetAbsolutePosition(360, 270)
            pdfContentByte.AddImage(imgp)

            imgp = iTextSharp.text.Image.GetInstance(ChartCO2speed, iTextSharp.text.Color.WHITE)
            imgp.ScaleAbsolute(440, 195)
            imgp.SetAbsolutePosition(360, 75)
            pdfContentByte.AddImage(imgp)

            imgp = iTextSharp.text.Image.GetInstance(Declaration.ConvPicPath(HDVclassStr, True))
            imgp.ScaleAbsolute(180, 50)
            imgp.SetAbsolutePosition(30, 475)
            pdfContentByte.AddImage(imgp)

            ' flatten the form to remove editting options, set it to false  to leave the form open to subsequent manual edits
            pdfStamper.FormFlattening = True

            ' close the pdf
            pdfStamper.Close()


            i = 0
            For Each mr In MissionResults
                i += 1

                temppath = MyDeclPath & "Reports\temp" & i & ".pdf"
                temppdfs.Add(temppath)

                pdfReader = New PdfReader(PdfTempMR)
                pdfStamper = New PdfStamper(pdfReader, New FileStream(temppath, FileMode.Create))

                pdfFormFields = pdfStamper.AcroFields

                pdfFormFields.SetField("version", VECTOvers)
                pdfFormFields.SetField("Job", JobFile)
                pdfFormFields.SetField("Date", DateStr)
                pdfFormFields.SetField("Created", Creator)
                pdfFormFields.SetField("Config", MassMaxStr & " " & ConvAxleConf(AxleConf) & " " & ConvVehCat(VehCat, True))
                pdfFormFields.SetField("HDVclass", "HDV Class " & HDVclassStr)
                pdfFormFields.SetField("PageNr", "Page " & i + 1 & " of " & pgMax)

                pdfFormFields.SetField("Mission", mr.MissionRef.NameStr)

                For Each lr In mr.Results

                    Select Case lr.Key
                        Case tLoading.EmptyLoaded
                            lstr = "E"
                        Case tLoading.RefLoaded
                            lstr = "R"
                        Case tLoading.FullLoaded
                            lstr = "F"
                        Case Else
                            Return False
                    End Select

                    With lr.Value
                        pdfFormFields.SetField("Load" & lstr, .Loading.ToString("0.0") & " t")
                        pdfFormFields.SetField("Speed" & lstr, .Speed.ToString("0.0"))
                        pdfFormFields.SetField("FCkm" & lstr, .FCkm.ToString("0.0"))
                        If .Loading = 0 Then
                            pdfFormFields.SetField("FCtkm" & lstr, "-")
                            pdfFormFields.SetField("CO2tkm" & lstr, "-")
                        Else
                            pdfFormFields.SetField("FCtkm" & lstr, .FCtkm.ToString("0.0"))
                            pdfFormFields.SetField("CO2tkm" & lstr, .CO2tkm.ToString("0.0"))
                        End If
                        pdfFormFields.SetField("CO2km" & lstr, .CO2km.ToString("0.0"))
                    End With

                Next

                'Add Images
                pdfContentByte = pdfStamper.GetOverContent(1)

                imgp = iTextSharp.text.Image.GetInstance(Declaration.ConvPicPath(HDVclassStr, (mr.MissionRef.MissionID = tMission.LongHaul)))
                imgp.ScaleAbsolute(180, 50)
                imgp.SetAbsolutePosition(600, 475)
                pdfContentByte.AddImage(imgp)

                imgp = iTextSharp.text.Image.GetInstance(mr.ChartSpeed, iTextSharp.text.Color.WHITE)
                imgp.ScaleAbsolute(780, 156)
                imgp.SetAbsolutePosition(17, 270)
                pdfContentByte.AddImage(imgp)

                imgp = iTextSharp.text.Image.GetInstance(mr.ChartTqN, iTextSharp.text.Color.WHITE)
                imgp.ScaleAbsolute(420, 178)
                imgp.SetAbsolutePosition(375, 75)
                pdfContentByte.AddImage(imgp)

                ' flatten the form to remove editting options, set it to false  to leave the form open to subsequent manual edits
                pdfStamper.FormFlattening = True

                ' close the pdf
                pdfStamper.Close()

            Next

            'Merge files
            doc = New iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate, 12, 12, 12, 12)

            pdfWriter = pdfWriter.GetInstance(doc, New FileStream(Filepath, FileMode.Create))

            doc.Open()

            For Each temppath In temppdfs
                pdfReader = New PdfReader(temppath)
                pdfpage = pdfWriter.GetImportedPage(pdfReader, 1)
                doc.Add(iTextSharp.text.Image.GetInstance(pdfpage))
            Next

            doc.Close()

            'Delete temp files
            For Each temppath In temppdfs
                IO.File.Delete(temppath)
            Next

        Catch ex As Exception

            Return False

        End Try



        Return True


    End Function


    Public Class cMissionResults

        Public Results As Dictionary(Of tLoading, cLoadingResults)

        Public MissionRef As cMission

        Public ChartSpeed As Image
        Public ChartTqN As Image


        Public Sub New()
            Results = New Dictionary(Of tLoading, cLoadingResults)
        End Sub



    End Class

    Public Class cLoadingResults
        Public Loading As Single = 0
        Public Speed As Single = 0
        Public FCkm As Single = 0
        Public FCtkm As Single = 0
        Public CO2km As Single = 0
        Public CO2tkm As Single = 0
        Public FCerror As Boolean = False

        Public TargetSpeed As List(Of Single)
        Public ActualSpeed As List(Of Single)
        Public Distance As List(Of Single)
        Public Alt As List(Of Single)
        Public Tq As List(Of Single)
        Public nU As List(Of Single)


        Public Sub New()
            TargetSpeed = New List(Of Single)
            ActualSpeed = New List(Of Single)
            Distance = New List(Of Single)
            Alt = New List(Of Single)
            Tq = New List(Of Single)
            nU = New List(Of Single)
        End Sub



       
    End Class


End Class

