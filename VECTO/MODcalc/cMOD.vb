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
Public Class cMOD

    Public Pe As List(Of Single)
    Public nn As List(Of Single)
    Public nU As List(Of Single)
    Public nUvorg As List(Of Single)
    Public dnUvorg As List(Of Single)
    Public tDim As Integer
    Public tDimOgl As Integer
    Public Em As cEm
    Public Px As cPower
    Public TC As cTC
    Public Vh As cVh
    Public CylceKin As cCycleKin
    Public ModOutpName As String
    Public ModErrors As cModErrors

    'Power
    Public Psum As List(Of Single)
    Public Proll As List(Of Single)
    Public Pstg As List(Of Single)
    Public Pluft As List(Of Single)
    Public Pa As List(Of Single)
    Public Pbrake As List(Of Single)
    Public PauxSum As List(Of Single)
    Public PlossGB As List(Of Single)
    Public PlossDiff As List(Of Single)
    Public PlossRt As List(Of Single)
    Public PaEng As List(Of Single)
    Public PaGB As List(Of Single)
    Public Paux As Dictionary(Of String, List(Of Single))
    Public Grad As List(Of Single)

    Public EngState As List(Of tEngState)

    'Vehicle
    Public Gear As List(Of Single)
    Public VehState As List(Of tVehState)

    Public TCnu As List(Of Single)
    Public TCmu As List(Of Single)
    Public TCMout As List(Of Single)
    Public TCnOut As List(Of Single)


    Private bInit As Boolean

    Public Sub New()
        bInit = False
    End Sub

    Public Sub Init()
        Pe = New List(Of Single)
        nn = New List(Of Single)
        nU = New List(Of Single)
        Em = New cEm
        Px = New cPower
        TC = New cTC
        Vh = New cVh
        CylceKin = New cCycleKin

        Proll = New List(Of Single)
        Psum = New List(Of Single)
        Pstg = New List(Of Single)
        Pbrake = New List(Of Single)
        Pluft = New List(Of Single)
        Pa = New List(Of Single)
        PauxSum = New List(Of Single)
        PlossGB = New List(Of Single)
        PlossDiff = New List(Of Single)
        PlossRt = New List(Of Single)
        PaEng = New List(Of Single)
        PaGB = New List(Of Single)
        Paux = New Dictionary(Of String, List(Of Single))
        Grad = New List(Of Single)

        EngState = New List(Of tEngState)

        Gear = New List(Of Single)
        VehState = New List(Of tVehState)

        TCnu = New List(Of Single)
        TCmu = New List(Of Single)
        TCMout = New List(Of Single)
        TCnOut = New List(Of Single)

        Em.Init()
        TC.Init()
        Vh.Init()
        Px.Init()
        ModErrors = New cModErrors
        bInit = True
    End Sub

    Public Sub CleanUp()
        If bInit Then
            Em.CleanUp()
            Px.CleanUp()
            TC.CleanUp()
            Vh.CleanUp()
            Em = Nothing
            Px = Nothing
            TC = Nothing
            Vh = Nothing
            Pe = Nothing
            nn = Nothing
            nU = Nothing

            Proll = Nothing
            Psum = Nothing
            Pstg = Nothing
            Pluft = Nothing
            Pa = Nothing
            Pbrake = Nothing
            PauxSum = Nothing
            PlossGB = Nothing
            PlossDiff = Nothing
            PlossRt = Nothing
            PaEng = Nothing
            PaGB = Nothing
            Paux = Nothing
            Grad = Nothing

            EngState = Nothing

            Gear = Nothing
            VehState = Nothing

            TCnu = Nothing
            TCmu = Nothing
            TCMout = Nothing
            TCnOut = Nothing

            CylceKin = Nothing
            ModErrors = Nothing
            bInit = False
        End If
    End Sub

    Public Sub Duplicate(ByVal t As Integer)
        Dim EmKV As KeyValuePair(Of String, cEmComp)
        Dim AuxKV As KeyValuePair(Of String, List(Of Single))

        If DRI.Nvorg Then
            nUvorg.Insert(t, nUvorg(t))
            dnUvorg.Insert(t, dnUvorg(t))
        End If

        If DRI.EmCompDef Then
            For Each EmKV In DRI.EmComponents
                EmKV.Value.RawVals.Insert(t, EmKV.Value.RawVals(t))
            Next
        End If

        If DRI.AuxDef Then
            For Each AuxKV In DRI.AuxComponents
                AuxKV.Value.Insert(t, AuxKV.Value(t))
            Next
        End If

    End Sub

    Public Sub Cut(ByVal t As Integer)
        Dim EmKV As KeyValuePair(Of String, cEmComp)
        Dim AuxKV As KeyValuePair(Of String, List(Of Single))

        If DRI.Nvorg Then
            nUvorg.RemoveAt(t)
            dnUvorg.RemoveAt(t)
        End If

        If DRI.EmCompDef Then
            For Each EmKV In DRI.EmComponents
                EmKV.Value.RawVals.RemoveAt(t)
            Next
        End If

        If DRI.AuxDef Then
            For Each AuxKV In DRI.AuxComponents
                AuxKV.Value.RemoveAt(t)
            Next
        End If

    End Sub



    Public Sub CycleInit()

        If GEN.VehMode = tVehMode.EngineOnly Then
            EngCycleInit()
        Else
            VehCycleInit()
        End If

        tDimOgl = tDim

    End Sub

    Private Sub VehCycleInit()
        Dim s As Integer
        Dim L As List(Of Double)
        Dim EmKV As KeyValuePair(Of String, cEmComp)
        Dim ExsKV As KeyValuePair(Of tExsComp, Dictionary(Of Short, List(Of Single)))
        Dim Exs0 As List(Of Single)
        Dim AuxKV As KeyValuePair(Of String, List(Of Single))

        'Define Cycle-length (shorter by 1sec than original because of Interim-seconds)
        tDim = DRI.tDim - 1

        'Here the actual cycle is read:
        Vh.VehCylceInit()

        'Revolutions-setting
        If DRI.Nvorg Then

            MODdata.nUvorg = New List(Of Single)
            MODdata.dnUvorg = New List(Of Single)

            L = DRI.Values(tDriComp.nn)

            'Revolutions
            For s = 0 To tDim
                MODdata.nUvorg.Add(((L(s + 1) + L(s)) / 2) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl)
            Next

            'Angular acceleration
            For s = 0 To tDim
                MODdata.dnUvorg.Add((L(s + 1) - L(s)) * (VEH.nNenn - VEH.nLeerl))
            Next

        End If

        'Average EM-components (between-seconds) for KF-creation or Eng-Analysis
        If DRI.EmCompDef Then
            For Each EmKV In DRI.EmComponents
                For s = 0 To tDim
                    EmKV.Value.RawVals(s) = (EmKV.Value.RawVals(s + 1) + EmKV.Value.RawVals(s)) / 2
                Next
            Next
        End If

        'Specify average EXS
        If DRI.ExsCompDef Then
            For Each ExsKV In DRI.ExsComponents
                For Each Exs0 In ExsKV.Value.Values
                    For s = 0 To tDim
                        Exs0(s) = (Exs0(s + 1) + Exs0(s)) / 2
                    Next
                Next
            Next
        End If

        'Specify average Aux and Aux-lists, when Au8x present in DRI and VEH
        If DRI.AuxDef Then
            For Each AuxKV In DRI.AuxComponents

                For s = 0 To tDim
                    AuxKV.Value(s) = (AuxKV.Value(s + 1) + AuxKV.Value(s)) / 2
                Next

                If VEH.AuxPaths.ContainsKey(AuxKV.Key) Then MODdata.Paux.Add(AuxKV.Key, New List(Of Single))

            Next
        End If


    End Sub

    Private Sub EngCycleInit()
        Dim s As Integer
        Dim L As List(Of Double)

        'Zykluslänge definieren: Gleiche Länge wie Zyklus (nicht reduziert weil keine "Zwischensekunden") |@@| Define Cycle-length: Same length as Cycle (not reduced because no "interim seconds")
        tDim = DRI.tDim

        'Here the actual cycle is read:
        Vh.EngCylceInit()

        'Revolutions-setting
        If DRI.Nvorg Then

            MODdata.nUvorg = New List(Of Single)
            MODdata.dnUvorg = New List(Of Single)

            L = DRI.Values(tDriComp.nn)

            'Revolutions
            For s = 0 To MODdata.tDim
                MODdata.nUvorg.Add(L(s) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl)
            Next

            'Angular acceleration
            MODdata.dnUvorg.Add(L(1) - L(0))
            For s = 1 To MODdata.tDim - 1
                MODdata.dnUvorg.Add((L(s + 1) - L(s - 1)) / 2 * (VEH.nNenn - VEH.nLeerl))
            Next
            MODdata.dnUvorg.Add(L(MODdata.tDim) - L(MODdata.tDim - 1))

        End If

    End Sub

    Public Function Output() As Boolean

        Dim f As cFile_V3
        Dim s As System.Text.StringBuilder
        Dim sU As System.Text.StringBuilder
        Dim t As Integer
        Dim t1 As Integer

        Dim Sepp As String
        Dim path As String
        Dim dist As Double
        Dim MsgSrc As String
        Dim tdelta As Single

        Dim EmList As New List(Of String)
        Dim Em0 As cEmComp
        Dim StrKey As String

        Dim TcList As New List(Of tMapComp)
        'Dim TC0 As List(Of Single)
        Dim TcKey As tMapComp

        Dim AuxList As New List(Of String)

        Dim Gear As Integer

        MsgSrc = "MOD/Output"

        '*********** Initialization / Open File **************
        If ModOutpName = "" Then
            WorkerMsg(tMsgID.Err, "Invalid output path!", MsgSrc)
            Return False
        End If

        f = New cFile_V3

        path = ModOutpName & ".vmod"

        If Not f.OpenWrite(path, ",", False) Then
            WorkerMsg(tMsgID.Err, "Can't write to " & path, MsgSrc)
            Return False
        End If

        s = New System.Text.StringBuilder
        sU = New System.Text.StringBuilder

        '*********** Settings **************
        Sepp = ","
        t1 = MODdata.tDim
        If GEN.VehMode = tVehMode.EngineOnly Then
            tdelta = 0
        Else
            tdelta = 0.5
        End If


        '********** Key-Listen ************
        For Each StrKey In Em.EmComp.Keys
            EmList.Add(StrKey)
        Next

        If TC.Calculated Then
            For Each TcKey In MODdata.TC.TCcomponents.Keys
                TcList.Add(TcKey)
            Next
        End If

        For Each StrKey In VEH.AuxRefs.Keys     'Wenn Engine Only dann wird das garnicht verwendet
            AuxList.Add(StrKey)
        Next




        f.WriteLine("VECTO modal results")
        f.WriteLine("VECTO " & VECTOvers)
        f.WriteLine(Now.ToString)
        f.WriteLine("Input File: " & JobFile)


        '***********************************************************************************************
        '***********************************************************************************************
        '***********************************************************************************************
        '*** Header & Units ****************************************************************************
        s.Length = 0

        s.Append("time")
        sU.Append("[s]")

        If Not GEN.VehMode = tVehMode.EngineOnly Then

            s.Append(",dist,v_act,v_targ,acc,grad")
            sU.Append(",[km],[km/h],[km/h],[m/s^2],[%]")
            dist = 0

        End If

        If GEN.ModeHorEV Then

            If Not GEN.VehMode = tVehMode.EV Then
                s.Append(",engine speed,Pe,n_norm,Pe_norm")
                sU.Append(",[1/min],[kW],[-],[-]")
            End If

            s.Append(",engine speed,PeEM,PeBat,PiBat,Ubat,Ibat,SOC")
            sU.Append(",[1/min],[kW],[kW],[kW],[V],[A],[-]")

        Else

            s.Append(",n,Tq_eng,Tq_clutch,Tq_full,Tq_drag,Pe_eng,Pe_full,Pe_drag,Pe_clutch,Pa Eng,Paux")
            sU.Append(",[1/min],[Nm],[Nm],[Nm],[Nm],[kW],[kW],[kW],[kW],[kW],[kW]")

        End If

        If Not GEN.VehMode = tVehMode.EngineOnly Then

            s.Append(",Gear,Ploss GB,Ploss Diff,Ploss Retarder,Pa GB,Pa Veh,Proll,Pair,Pgrad,Pwheel,Pbrake")
            sU.Append(",[-],[kW],[kW],[kW],[kW],[kW],[kW],[kW],[kW],[kW],[kW]")

            If GBX.TCon Then
                s.Append(",TCν,TCμ,TC_T_Out,TC_n_Out")
                sU.Append(",[-],[-],[Nm],[1/min]")
            End If

            'Auxiliaries
            For Each StrKey In AuxList

                s.Append(",Paux_" & StrKey)
                sU.Append(",[kW]")

            Next

        End If


        If Cfg.FinalEmOnly Then

            For Each StrKey In EmList

                Em0 = Em.EmComp(StrKey)

                If Em0.WriteOutput Then
                    s.Append(Sepp & Em0.Name)
                    sU.Append(Sepp & Em0.Unit)
                End If

            Next

        Else

            For Each StrKey In EmList

                Em0 = Em.EmComp(StrKey)

                If Em0.WriteOutput Then

                    s.Append(Sepp & Em0.Name & "_Raw")
                    sU.Append(Sepp & Em0.Unit)

                    If Em0.TCdef Then
                        s.Append(Sepp & Em0.Name & "_TC")
                        sU.Append(Sepp & Em0.Unit)
                    End If

                    If Em0.ATdef Then
                        s.Append(Sepp & Em0.Name & "_AT")
                        sU.Append(Sepp & Em0.Unit)
                    End If

                End If

            Next

        End If

        'Berechnete Dynamikparameter (Diff zu Kennfeld) |@@| Calculated dynamics parameters (Diff to Map)
        'If TC.Calculated Then
        '    For Each TcKey In TcList
        '        s.Append(Sepp & fMapCompName(TcKey))
        '        sU.Append(Sepp & "-")
        '    Next
        'End If


        'Write to File
        '   Header
        f.WriteLine(s.ToString)
        '   Units
        f.WriteLine(sU.ToString)

        '***********************************************************************************************
        '***********************************************************************************************
        '***********************************************************************************************
        '*** Values *************************************************************************************

        With MODdata

            For t = 0 To t1

                'Predefine Gear for FLD assignment
                If GEN.VehMode = tVehMode.EngineOnly Then
                    Gear = 0
                Else
                    Gear = .Gear(t)
                End If


                s.Length = 0

                'Time
                s.Append(t + DRI.t0 + tdelta)

                If Not GEN.VehMode = tVehMode.EngineOnly Then

                    'Strecke |@@| Route
                    dist += .Vh.V(t) / 1000
                    s.Append(Sepp & dist)

                    'Actual-speed.
                    s.Append(Sepp & .Vh.V(t) * 3.6)

                    'Target-speed
                    s.Append(Sepp & .Vh.Vsoll(t) * 3.6)

                    'Acc.
                    s.Append(Sepp & .Vh.a(t))

                    'Slope
                    s.Append(Sepp & .Grad(t))

                End If

                If GEN.ModeHorEV Then

                    If Not GEN.VehMode = tVehMode.EV Then

                        'Revolutions
                        s.Append(Sepp & .nn(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl)

                        'Power
                        s.Append(Sepp & .Pe(t) * VEH.Pnenn)

                        'Revolutions normalized
                        s.Append(Sepp & .nn(t))

                        'Power normalized
                        s.Append(Sepp & .Pe(t))

                    End If

                    'Revolutions in U/min
                    s.Append(Sepp & .nU(t))

                    'EM-power in kW
                    s.Append(Sepp & .Px.PeEMot(t))

                    'Effective Battery-power
                    s.Append(Sepp & .Px.PeBat(t))

                    'Internal Battery-power
                    s.Append(Sepp & .Px.PiBat(t))

                    'Battery-voltage
                    s.Append(Sepp & .Px.Ubat(t))

                    'Battery-Power
                    s.Append(Sepp & .Px.Ibat(t))

                    'SOC
                    s.Append(Sepp & .Px.SOC(t))

                Else

                    'Revolutions
                    s.Append(Sepp & .nn(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl)

                    If Math.Abs(2 * Math.PI * (.nn(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl) / 60) < 0.00001 Then
                        s.Append(Sepp & "-" & Sepp & "-" & Sepp & "-" & Sepp & "-")
                    Else

                        'Torque
                        s.Append(Sepp & 1000 * .Pe(t) * VEH.Pnenn / (2 * Math.PI * (.nn(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl) / 60))

                        'Torque at clutch
                        s.Append(Sepp & 1000 * (.Pe(t) * VEH.Pnenn - .PaEng(t) - .PauxSum(t)) / (2 * Math.PI * (.nn(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl) / 60))

                        'Full-load and Drag torque
                        If .EngState(t) = tEngState.Stopped Then
                            s.Append(Sepp & "-" & Sepp & "-")
                        Else
                            If t = 0 Then
                                s.Append(Sepp & 1000 * FLD(Gear).Pfull(.nn(t)) / (2 * Math.PI * (.nn(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl) / 60) & Sepp & 1000 * FLD(Gear).Pdrag(.nn(t)) / (2 * Math.PI * (.nn(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl) / 60))
                            Else
                                s.Append(Sepp & 1000 * FLD(Gear).Pfull(.nn(t), .Pe(t - 1)) / (2 * Math.PI * (.nn(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl) / 60) & Sepp & 1000 * FLD(Gear).Pdrag(.nn(t)) / (2 * Math.PI * (.nn(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl) / 60))
                            End If
                        End If

                    End If

                    'Power
                    s.Append(Sepp & .Pe(t) * VEH.Pnenn)

                    'Revolutions normalized
                    's.Append(Sepp & .nn(t))

                    'Power normalized
                    's.Append(Sepp & .Pe(t))

                    'Full-load and Drag
                    If .EngState(t) = tEngState.Stopped Then
                        s.Append(Sepp & "-" & Sepp & "-")
                    Else
                        If t = 0 Then
                            s.Append(Sepp & FLD(Gear).Pfull(.nn(t)) & Sepp & FLD(Gear).Pdrag(.nn(t)))
                        Else
                            s.Append(Sepp & FLD(Gear).Pfull(.nn(t), .Pe(t - 1)) & Sepp & FLD(Gear).Pdrag(.nn(t)))
                        End If
                    End If

                    'Power at Clutch
                    s.Append(Sepp & .Pe(t) * VEH.Pnenn - .PaEng(t) - .PauxSum(t))

                    'PaEng
                    s.Append(Sepp & .PaEng(t))

                    'Aux..
                    s.Append(Sepp & .PauxSum(t))

                End If

                If Not GEN.VehMode = tVehMode.EngineOnly Then

                    'Gear
                    s.Append(Sepp & .Gear(t))

                    'Transmission-losses
                    s.Append(Sepp & .PlossGB(t))

                    'Diff-losses
                    s.Append(Sepp & .PlossDiff(t))

                    'Retarder-losses
                    s.Append(Sepp & .PlossRt(t))

                    'PaGB
                    s.Append(Sepp & .PaGB(t))

                    'Pa Veh
                    s.Append(Sepp & .Pa(t))

                    'Roll..
                    s.Append(Sepp & .Proll(t))

                    'Drag
                    s.Append(Sepp & .Pluft(t))

                    'Slope ..
                    s.Append(Sepp & .Pstg(t))

                    'Wheel-power
                    s.Append(Sepp & .Psum(t))

                    'Brake
                    s.Append(Sepp & .Pbrake(t))

                    'Torque Converter output
                    If GBX.TCon Then s.Append(Sepp & .TCnu(t) & Sepp & .TCmu(t) & Sepp & .TCMout(t) & Sepp & .TCnOut(t))

                    'Auxiliaries
                    For Each StrKey In AuxList
                        s.Append(Sepp & .Paux(StrKey)(t))
                    Next

                End If

                If Cfg.FinalEmOnly Then

                    'Final-emissions (tailpipe)
                    For Each StrKey In EmList

                        Em0 = .Em.EmComp(StrKey)

                        If Em0.WriteOutput Then

                            If Em0.FinalVals(t) > -0.0001 Then
                                s.Append(Sepp & Em0.FinalVals(t))
                            Else
                                s.Append(Sepp & "ERROR")
                            End If

                        End If

                    Next

                Else

                    For Each StrKey In EmList

                        Em0 = .Em.EmComp(StrKey)

                        If Em0.WriteOutput Then
                            'Raw-emissions
                            s.Append(Sepp & Em0.RawVals(t))

                            'TC-Emissions
                            If Em0.TCdef Then s.Append(Sepp & Em0.TCVals(t))

                            'AT-Emissions (EXS)
                            If Em0.ATdef Then s.Append(Sepp & Em0.ATVals(t))
                        End If

                    Next

                End If

                'Calculated Dynamics-parameters (Diff from(zu) Map)
                'If TC.Calculated Then
                '    For Each TcKey In TcList
                '        TC0 = MODdata.TC.TCcomponents(TcKey)
                '        s.Append(Sepp & TC0(t))
                '    Next
                'End If

                'Write to File
                f.WriteLine(s.ToString)

            Next

        End With

        f.Close()

        'Add file to signing list
        Lic.FileSigning.AddFile(path)

        Return True

    End Function

    'Errors/Warnings die sekündlich auftreten können |@@| Errors/Warnings occuring every second
    Public Class cModErrors
        Public TrLossMapExtr As String
        Public AuxMapExtr As String
        Public AuxNegative As String
        Public FLDextrapol As String
        Public CdExtrapol As String
        Public RtExtrapol As String
        Public DesMaxExtr As String
        Public GSextrapol As String
        Public TCextrapol As String

        Public Sub New()
            ResetAll()
        End Sub


        'Reset-Hierarchie:
        ' ResetAll
        '   DesMaxExtr
        '   -GeschRedReset(Speed-Reduce-Reset)
        '       CdExtrapol        
        '       -PxReset
        '           TrLossMapExtr 
        '           AuxMapExtr 
        '           AuxNegative
        '           FLDextrapol

        'Full reset (at the beginning of each second step)
        Public Sub ResetAll()
            DesMaxExtr = ""
            GeschRedReset()
        End Sub

        'Reset Errors related to Speed Reduction (within iteration)
        Public Sub GeschRedReset()
            CdExtrapol = ""
            RtExtrapol = ""
            GSextrapol = ""
            TCextrapol = ""
            PxReset()
        End Sub

        'Reset von Errors die mit der Leistungsberechnung zu tun haben (nach Schaltmodell durchzuführen) |@@| Reset errors related to Power-calculation (towards performing the Gear-shifting model)
        Public Sub PxReset()
            TrLossMapExtr = ""
            AuxMapExtr = ""
            AuxNegative = ""
            FLDextrapol = ""
        End Sub

        'Emit Errors
        Public Function MsgOutputAbort(ByVal Second As String, ByVal MsgSrc As String) As Boolean
            Dim Abort As Boolean

            Abort = False

            If TrLossMapExtr <> "" Then
                WorkerMsg(tMsgID.Err, "Invalid extrapolation in Transmission Loss Map (" & TrLossMapExtr & ")!", MsgSrc & "/t= " & Second)
            End If

            If AuxMapExtr <> "" Then
                WorkerMsg(tMsgID.Err, "Invalid extrapolation in Auxiliary Map (" & AuxMapExtr & ")!", MsgSrc & "/t= " & Second)
            End If

            If AuxNegative <> "" Then
                WorkerMsg(tMsgID.Err, "Aux power < 0 (" & AuxNegative & ") Check cycle and aux efficiency map!", MsgSrc & "/t= " & Second)
                Abort = True
            End If

            If FLDextrapol <> "" Then
                WorkerMsg(tMsgID.Warn, "Extrapolation of Full load / drag curve (" & FLDextrapol & ")!", MsgSrc & "/t= " & Second)
            End If

            If CdExtrapol <> "" Then
                WorkerMsg(tMsgID.Warn, "Extrapolation in Cd input file (" & CdExtrapol & ")!", MsgSrc & "/t= " & Second)
            End If

            If DesMaxExtr <> "" Then
                WorkerMsg(tMsgID.Warn, "Extrapolation in .vacc input file (" & DesMaxExtr & ")!", MsgSrc & "/t= " & Second)
            End If

            If RtExtrapol <> "" Then
                WorkerMsg(tMsgID.Warn, "Extrapolation in Retarder input file (" & RtExtrapol & ")!", MsgSrc & "/t= " & Second)
            End If

            If GSextrapol <> "" Then
                WorkerMsg(tMsgID.Warn, "Extrapolation in Gear Shift Polygon file (" & GSextrapol & ")!", MsgSrc & "/t= " & Second)
            End If

            If TCextrapol <> "" Then
                WorkerMsg(tMsgID.Warn, "Extrapolation in Torque Converter file (" & TCextrapol & ")!", MsgSrc & "/t= " & Second)
            End If

            Return Abort

        End Function

    End Class

   








End Class




