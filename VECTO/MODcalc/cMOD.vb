Imports System.Collections.Generic
Public Class cMOD

    Public Pe As List(Of Single)
    Public nU As List(Of Single)
    Public nUvorg As List(Of Single)
    Public dnUvorg As List(Of Single)
    Public tDim As Integer
    Public tDimOgl As Integer
    Public Px As cPower
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
    Public Pclutch As List(Of Single)
    Public Grad As List(Of Single)

    Public EngState As List(Of tEngState)

    'Vehicle
    Public Gear As List(Of Single)
    Public VehState As List(Of tVehState)

    Public TCnu As List(Of Single)
    Public TCmu As List(Of Single)
    Public TCMout As List(Of Single)
    Public TCnOut As List(Of Single)

    'FC
    Public FCerror As Boolean
    Public lFC As List(Of Single)
    Public lFCAUXc As List(Of Single)
    Public lFCWHTCc As List(Of Single)
    Public FCavg As Single
    Public FCavgAUXc As Single
    Public FCavgWHTCc As Single

    Public FCAUXcSet As Boolean

    Private bInit As Boolean

    Public Sub New()
        bInit = False
    End Sub

    Public Sub Init()
        Pe = New List(Of Single)
        nU = New List(Of Single)
        Px = New cPower
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
        Pclutch = New List(Of Single)
        Grad = New List(Of Single)

        EngState = New List(Of tEngState)

        Gear = New List(Of Single)
        VehState = New List(Of tVehState)

        TCnu = New List(Of Single)
        TCmu = New List(Of Single)
        TCMout = New List(Of Single)
        TCnOut = New List(Of Single)

        lFC = New List(Of Single)
        lFCAUXc = New List(Of Single)
        lFCWHTCc = New List(Of Single)
        FCAUXcSet = False

        FCerror = False


        Vh.Init()
        ModErrors = New cModErrors


        bInit = True
    End Sub

    Public Sub CleanUp()
        If bInit Then
            lFC = Nothing
            lFCAUXc = Nothing
            lFCWHTCc = Nothing

            Vh.CleanUp()
            Px = Nothing
            Vh = Nothing
            Pe = Nothing
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
            Pclutch = Nothing
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
        Dim AuxKV As KeyValuePair(Of String, List(Of Single))

        If DRI.Nvorg Then
            nUvorg.Insert(t, nUvorg(t))
            dnUvorg.Insert(t, dnUvorg(t))
        End If

        If DRI.AuxDef Then
            For Each AuxKV In DRI.AuxComponents
                AuxKV.Value.Insert(t, AuxKV.Value(t))
            Next
        End If

    End Sub

    Public Sub Cut(ByVal t As Integer)
        Dim AuxKV As KeyValuePair(Of String, List(Of Single))

        If DRI.Nvorg Then
            nUvorg.RemoveAt(t)
            dnUvorg.RemoveAt(t)
        End If

        If DRI.AuxDef Then
            For Each AuxKV In DRI.AuxComponents
                AuxKV.Value.RemoveAt(t)
            Next
        End If

    End Sub



    Public Sub CycleInit()

        If VEC.EngOnly Then
            EngCycleInit()
        Else
            VehCycleInit()
        End If

        tDimOgl = tDim

    End Sub

    Private Sub VehCycleInit()
        Dim s As Integer
        Dim L As List(Of Double)
        Dim AuxKV As KeyValuePair(Of String, List(Of Single))
        Dim st As String

        'Define Cycle-length (shorter by 1sec than original because of Interim-seconds)
        tDim = DRI.tDim - 1

        'Here the actual cycle is read:
        Vh.VehCylceInit()

        'Revolutions-setting
        If DRI.Nvorg Then

            MODdata.nUvorg = New List(Of Single)
            MODdata.dnUvorg = New List(Of Single)

            L = DRI.Values(tDriComp.nU)

            'Revolutions
            For s = 0 To tDim
                MODdata.nUvorg.Add(((L(s + 1) + L(s)) / 2))
            Next

            'Angular acceleration
            For s = 0 To tDim
                MODdata.dnUvorg.Add(L(s + 1) - L(s))
            Next

        End If

        'Specify average Aux and Aux-lists, when Aux present in DRI and VEH
        If Cfg.DeclMode Then

            For Each st In VEC.AuxPaths.Keys
                MODdata.Paux.Add(st, New List(Of Single))
            Next

        Else

            If DRI.AuxDef Then
                For Each AuxKV In DRI.AuxComponents

                    For s = 0 To tDim
                        AuxKV.Value(s) = (AuxKV.Value(s + 1) + AuxKV.Value(s)) / 2
                    Next

                    If VEC.AuxPaths.ContainsKey(AuxKV.Key) Then MODdata.Paux.Add(AuxKV.Key, New List(Of Single))

                Next
            End If

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

            L = DRI.Values(tDriComp.nU)

            'Revolutions
            For s = 0 To MODdata.tDim
                MODdata.nUvorg.Add(L(s))
            Next

            'Angular acceleration
            MODdata.dnUvorg.Add(L(1) - L(0))
            For s = 1 To MODdata.tDim - 1
                MODdata.dnUvorg.Add((L(s + 1) - L(s - 1)) / 2)
            Next
            MODdata.dnUvorg.Add(L(MODdata.tDim) - L(MODdata.tDim - 1))

        End If

    End Sub



    Public Sub FCcalc(ByVal WHTCcorrection As Boolean)
        Dim v As Single
        Dim i As Integer
        Dim Result As Boolean
        Dim x As Single
        Dim sum As Double
        Dim LostEnergy As Double
        Dim EngOnTime As Integer
        Dim AddEngLoad As Single
        Dim info As cRegression.RegressionProcessInfo
        Dim reg As cRegression
        Dim rx As List(Of Double)
        Dim ry As List(Of Double)
        Dim rR2 As Single
        Dim rA As Double
        Dim rB As Double
        Dim rSE As Double
        Dim PeAdd As Double

        Dim MsgSrc As String

        MsgSrc = "MAP/FC_Intp"

        FCerror = False
        Result = True
        LostEnergy = 0
        EngOnTime = 0
        rx = New List(Of Double)
        ry = New List(Of Double)

        For i = 0 To MODdata.tDim

            Select Case MODdata.EngState(i)

                Case tEngState.Stopped

                    lFC.Add(0)
                    LostEnergy += MODdata.PauxSum(i) / 3600

                Case Else   '<= Idle / Drag / FullLoad-Unterscheidung...?


                    'Delaunay
                    v = MAP.fFCdelaunay_Intp(MODdata.nU(i), nPeToM(MODdata.nU(i), MODdata.Pe(i)))

                    If v < 0 And v > -999 Then v = 0

                    If Result Then
                        If v < -999 Then Result = False
                    End If
                    lFC.Add(v)

                    EngOnTime += 1
                    rx.Add(MODdata.Pe(i))
                    ry.Add(v)

            End Select

        Next

        'Calc average FC
        sum = 0
        For Each x In lFC
            sum += x
        Next
        FCavg = CSng(sum / lFC.Count)

        'Start/Stop-Aux - Correction
        If Result AndAlso LostEnergy > 0 Then

            WorkerMsg(tMsgID.Normal, "Correcting FC due to wrong aux energy balance during engine stop times", MsgSrc)
            WorkerMsg(tMsgID.Normal, " > Error in aux energy balance: " & LostEnergy.ToString("0.000") & " [kWh]", MsgSrc)

            If EngOnTime < 1 Then
                WorkerMsg(tMsgID.Err, " > ERROR: Engine-On Time = 0!", MsgSrc)
                FCerror = True
                Exit Sub
            End If

            'Linear regression of FC=f(Pe)
            reg = New cRegression

            info = reg.Regress(rx.ToArray, ry.ToArray)
            rR2 = info.PearsonsR ^ 2
            rA = info.a
            rB = info.b
            rSE = info.StandardError

            If rB <= 0 Then
                WorkerMsg(tMsgID.Err, " > ERROR in linear regression ( b=" & rB & ")!", MsgSrc)
                FCerror = True
                Exit Sub
            End If

            'Additional engine load due to lost Aux energy: [kW] = [kWh]/[h]
            AddEngLoad = LostEnergy / (EngOnTime / 3600)

            WorkerMsg(tMsgID.Normal, " > Additional engine load: " & AddEngLoad.ToString("0.000") & " [kW]", MsgSrc)

            For i = 0 To MODdata.tDim
                lFCAUXc.Add(lFC(i))
                If MODdata.EngState(i) <> tEngState.Stopped Then
                    PeAdd = AddEngLoad + MODdata.Pbrake(i)
                    If PeAdd > 0 Then
                        lFCAUXc(i) += rB * PeAdd
                    End If
                End If
            Next

            'average
            sum = 0
            For Each x In lFCAUXc
                sum += x
            Next
            FCavgAUXc = CSng(sum / lFC.Count)

            FCAUXcSet = True

        End If

        'WHTC Correction
        If Cfg.DeclMode Then

            For i = 0 To MODdata.tDim
                lFCWHTCc.Add(lFC(i) * Declaration.WHTCcorrFactor)
            Next

            sum = 0
            For Each x In lFCWHTCc
                sum += x
            Next
            FCavgWHTCc = CSng(sum / lFC.Count)

        End If

        If Not Result Then FCerror = True

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

        Dim StrKey As String

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
        If VEC.EngOnly Then
            tdelta = 0
        Else
            tdelta = 0.5
        End If


        '********** Aux-List ************
        For Each StrKey In VEC.AuxRefs.Keys     'Wenn Engine Only dann wird das garnicht verwendet
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

        If Not VEC.EngOnly Then

            s.Append(",dist,v_act,v_targ,acc,grad")
            sU.Append(",[m],[km/h],[km/h],[m/s^2],[%]")
            dist = 0

        End If

        s.Append(",n,Tq_eng,Tq_clutch,Tq_full,Tq_drag,Pe_eng,Pe_full,Pe_drag,Pe_clutch,Pa Eng,Paux")
        sU.Append(",[1/min],[Nm],[Nm],[Nm],[Nm],[kW],[kW],[kW],[kW],[kW],[kW]")

        If Not VEC.EngOnly Then

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


        'FC
        s.Append(Sepp & "FC")
        sU.Append(Sepp & "[g/h]")

        s.Append(Sepp & "FC-AUXc")
        sU.Append(Sepp & "[g/h]")

        s.Append(Sepp & "FC-WHTCc")
        sU.Append(Sepp & "[g/h]")


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
                If VEC.EngOnly Then
                    Gear = 0
                Else
                    Gear = .Gear(t)
                End If


                s.Length = 0

                'Time
                s.Append(t + DRI.t0 + tdelta)

                If Not VEC.EngOnly Then

                    'distance
                    dist += .Vh.V(t)
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

                'Revolutions
                s.Append(Sepp & .nU(t))

                If Math.Abs(2 * Math.PI * .nU(t) / 60) < 0.00001 Then
                    s.Append(Sepp & "0" & Sepp & "0" & Sepp & "0" & Sepp & "0")
                Else

                    'Torque
                    s.Append(Sepp & nPeToM(.nU(t), .Pe(t)))

                    'Torque at clutch
                    s.Append(Sepp & nPeToM(.nU(t), .Pclutch(t)))

                    'Full-load and Drag torque
                    If .EngState(t) = tEngState.Stopped Then
                        s.Append(Sepp & "0" & Sepp & "0")
                    Else
                        If t = 0 Then
                            s.Append(Sepp & nPeToM(.nU(t), FLD(Gear).Pfull(.nU(t))) & Sepp & nPeToM(.nU(t), FLD(Gear).Pdrag(.nU(t))))
                        Else
                            s.Append(Sepp & nPeToM(.nU(t), FLD(Gear).Pfull(.nU(t), .Pe(t - 1))) & Sepp & nPeToM(.nU(t), FLD(Gear).Pdrag(.nU(t))))
                        End If
                    End If

                End If

                'Power
                s.Append(Sepp & .Pe(t))

                'Revolutions normalized
                's.Append(Sepp & .nn(t))

                'Power normalized
                's.Append(Sepp & .Pe(t))

                'Full-load and Drag
                If .EngState(t) = tEngState.Stopped Then
                    s.Append(Sepp & "-" & Sepp & "-")
                Else
                    If t = 0 Then
                        s.Append(Sepp & FLD(Gear).Pfull(.nU(t)) & Sepp & FLD(Gear).Pdrag(.nU(t)))
                    Else
                        s.Append(Sepp & FLD(Gear).Pfull(.nU(t), .Pe(t - 1)) & Sepp & FLD(Gear).Pdrag(.nU(t)))
                    End If
                End If

                'Power at Clutch
                s.Append(Sepp & .Pclutch(t))

                'PaEng
                s.Append(Sepp & .PaEng(t))

                'Aux..
                s.Append(Sepp & .PauxSum(t))



                If Not VEC.EngOnly Then

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

                'FC
                If .lFC(t) > -0.0001 Then
                    s.Append(Sepp & .lFC(t))
                Else
                    s.Append(Sepp & "ERROR")
                End If

                If FCAUXcSet Then
                    If .lFCAUXc(t) > -0.0001 Then
                        s.Append(Sepp & .lFCAUXc(t))
                    Else
                        s.Append(Sepp & "ERROR")
                    End If
                Else
                    s.Append(Sepp & "-")
                End If


                If Cfg.DeclMode Then
                    If .lFCWHTCc(t) > -0.0001 Then
                        s.Append(Sepp & .lFCWHTCc(t))
                    Else
                        s.Append(Sepp & "ERROR")
                    End If
                Else
                    s.Append(Sepp & "-")
                End If

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

            If TCextrapol <> "" Then
                WorkerMsg(tMsgID.Warn, "Extrapolation in Torque Converter file (" & TCextrapol & ")!", MsgSrc & "/t= " & Second)
            End If

            Return Abort

        End Function

    End Class










End Class




