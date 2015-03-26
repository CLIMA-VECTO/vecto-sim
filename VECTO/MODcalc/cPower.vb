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

Public Class cPower

    Private ClutchNorm As Single    'Normalized clutch speed
    Private ClutchEta As Single       'clutch efficiency

    'Settings
    Private GearInput As Boolean
    Private RpmInput As Boolean


    'Per-second Data
    Private Clutch As tEngClutch
    Private VehState0 As tVehState
    Private EngState0 As tEngState
    Private Pplus As Boolean
    Private Pminus As Boolean
    Private GVmax As Single
    Private Pwheel As Single
    Private Vact As Single
    Private aact As Single

    'Interruption of traction
    Private TracIntrI As Integer
    Private TracIntrIx As Integer
    Private TracIntrOn As Boolean
    Private TracIntrTurnOff As Boolean
    Private TracIntrGear As Integer

    Private LastGearChange As Integer
    Private LastClutch As tEngClutch

    Public Positions As New List(Of Short)

    Private EngSideInertia As Single

    Public Function PreRun() As Boolean
        Dim i As Integer
        Dim i0 As Integer
        Dim Vh As cVh
        Dim P As Single
        Dim Pmin As Single
        Dim PlossGB As Single
        Dim PlossDiff As Single
        Dim PlossRt As Single
        Dim PaMot As Single
        Dim PaGetr As Single
        Dim Pkup As Single
        Dim Paux As Single
        Dim Gear As Integer
        Dim nU As Single
        Dim vCoasting As Single
        Dim Vmax As Single
        Dim Vmin As Single
        Dim Tlookahead As Integer
        Dim vset1 As Single
        Dim vset2 As Single
        Dim j As Integer
        Dim t As Integer
        Dim adec As Single
        Dim LookAheadDone As Boolean
        Dim aCoasting As Single
        Dim Gears As New List(Of Integer)
        Dim vRollout As Single
        Dim ProgBarShare As Int16
        Dim ProgBarLACpart As Int16
        Dim dist As New List(Of Double)
        Dim LastnU As Single = 0

        Dim MsgSrc As String


        MsgSrc = "Power/PreRun"

        'Check Input
        If VEC.LookAheadOn AndAlso VEC.a_lookahead >= 0 Then
            WorkerMsg(tMsgID.Err, "Lookahead deceleration invalid! Value must be below zero.", MsgSrc)
            Return False
        End If

        If VEC.OverSpeedOn And VEC.EcoRollOn Then
            WorkerMsg(tMsgID.Err, "Overrun and Ecoroll can't be enabled both at the same time!", MsgSrc)
            Return False
        End If

        '   Initialize
        Vh = MODdata.Vh
        GearInput = DRI.Gvorg
        RpmInput = DRI.Nvorg

        If VEC.EcoRollOn Or VEC.OverSpeedOn Then
            If VEC.LookAheadOn Then
                ProgBarShare = 4
                ProgBarLACpart = 2
            Else
                ProgBarShare = 2
                ProgBarLACpart = 0  '0=OFF
            End If
        Else
            If VEC.LookAheadOn Then
                ProgBarShare = 2
                ProgBarLACpart = 1
            Else
                ProgBarShare = 0
                ProgBarLACpart = 0  '0=OFF
            End If
        End If

        Positions = New List(Of Short)

        If GBX.TCon Then
            EngSideInertia = ENG.I_mot + GBX.TCinertia
        Else
            EngSideInertia = ENG.I_mot
        End If

        'Distance over time
        dist.Add(0)
        For i = 1 To MODdata.tDim
            dist.Add(dist(i - 1) + Vh.V(i))
        Next


        'Generate Positions List
        For i = 0 To MODdata.tDim
            Positions.Add(0)
        Next

        '*** Positions ***
        '0... Normal (Cruise/Acc)
        '1... Brake or Coasting
        '2... Brake corrected with v(a) (.vacc file)
        '3... Coasting
        '4... Eco-Roll

        'Overspeed / Eco-Roll Loop (Forward)
        i = -1
        Do
            i += 1

            'Check if cancellation pending 
            If VECTOworker.CancellationPending Then Return True

            Vact = Vh.V(i)
            aact = Vh.a(i)

            'Determine Driving-state  -------------------------
            Pplus = False
            Pminus = False

            If Vact < 0.0001 Then
                VehState0 = tVehState.Stopped
            Else
                If aact >= 0.01 Then
                    VehState0 = tVehState.Acc
                ElseIf aact < -0.01 Then
                    VehState0 = tVehState.Dec
                Else
                    VehState0 = tVehState.Cruise
                End If
            End If


            'Wheel-Power
            Pwheel = fPwheel(i, Vh.fGrad(dist(i)))

            Select Case Pwheel
                Case Is > 0.0001
                    Pplus = True
                Case Is < -0.0001
                    Pminus = True
                Case Else
                    P = 0
            End Select

            'Gear
            If VehState0 = tVehState.Stopped Then
                Gear = 0

                'Engine Speed
                If RpmInput Then
                    nU = MODdata.nUvorg(i)
                Else
                    nU = ENG.Nidle
                End If

            Else

                If GearInput Then
                    Gear = Math.Min(Vh.GearVorg(i), GBX.GearCount)
                Else
                    Gear = fFastGearCalc(Vact, Pwheel)
                End If

                'Engine Speed
                If RpmInput Then
                    nU = MODdata.nUvorg(i)
                Else
                    nU = fnU(Vact, Gear, False)
                End If

            End If

            'ICE-inertia   
            If i = 0 Then
                PaMot = 0
            Else
                PaMot = fPaMot(nU, LastnU)
            End If



            'Calculate powertrain losses => power at clutch
            If Pplus Or Pminus Then
                PlossGB = fPlossGB(Pwheel, Vact, Gear, True)
                PlossDiff = fPlossDiff(Pwheel, Vact, True)
                PlossRt = fPlossRt(Vact, Gear)
                PaGetr = fPaG(Vact, aact)
                Pkup = Pwheel + PlossGB + PlossDiff + PaGetr + PlossRt
            Else
                Pkup = 0
            End If

            'Total aux power
            '[kW]
            Paux = fPaux(i, nU)

            'Internal Engine Power (Pclutch plus Aux plus Inertia)
            P = Pkup + Paux + PaMot


            'Full load / motoring
            Pmin = FLD(Gear).Pdrag(nU)

            If Vact >= VEC.vMin / 3.6 Then

                If VEC.EcoRollOn Then

                    'Secondary Progressbar
                    ProgBarCtrl.ProgJobInt = CInt((100 / ProgBarShare) * i / MODdata.tDim)

                    If Pwheel < 0 Or (i > 0 AndAlso Vh.EcoRoll(i - 1)) Then

                        Vmax = MODdata.Vh.Vsoll(i) + VEC.OverSpeed / 3.6
                        Vmin = Math.Max(0, MODdata.Vh.Vsoll(i) - VEC.UnderSpeed / 3.6)
                        vRollout = fRolloutSpeed(i, 1, Vh.fGrad(dist(i)))

                        If vRollout < Vmin Then

                            'Eco-Roll deactivated

                        ElseIf vRollout <= Vmax Then

                            If 2 * vRollout - Vh.V0(i) > Vmax Then
                                Vh.SetSpeed0(i, Vmax)
                            ElseIf 2 * vRollout - Vh.V0(i) < Vmin Then
                                Vh.SetSpeed0(i, Vmin)
                            Else
                                Vh.SetSpeed(i, vRollout)
                                'Vh.SetAcc(i, aRollout)
                            End If

                            Positions(i) = 4

                            'Mark position for Calc
                            Vh.EcoRoll(i) = True

                        Else

                            If 2 * Vmax - Vh.V0(i) >= Vmax Then
                                Vh.SetSpeed0(i, Vmax)
                            Else
                                Vh.SetSpeed(i, Vmax)
                            End If

                            Positions(i) = 1

                            'Do NOT mark position for Calc => Motoring NOT Idling
                            'Vh.EcoRoll(i) = True

                        End If


                    End If

                Else

                    If P < Pmin Then

                        If VEC.OverSpeedOn Then

                            'Secondary Progressbar
                            ProgBarCtrl.ProgJobInt = CInt((100 / ProgBarShare) * i / MODdata.tDim)

                            vCoasting = fCoastingSpeed(i, dist(i), Gear)
                            Vmax = MODdata.Vh.Vsoll(i) + VEC.OverSpeed / 3.6

                            If vCoasting <= Vmax Then

                                If 2 * vCoasting - Vh.V0(i) > Vmax Then
                                    Vh.SetSpeed0(i, Vmax)
                                Else
                                    Vh.SetSpeed(i, vCoasting)
                                End If

                            Else

                                If 2 * Vmax - Vh.V0(i) > Vmax Then
                                    Vh.SetSpeed0(i, Vmax)
                                Else
                                    Vh.SetSpeed(i, Vmax)
                                End If

                            End If

                        End If

                    End If

                End If

            End If

            LastnU = nU

            Gears.Add(Gear)

        Loop Until i >= MODdata.tDim


        'Look Ahead & Limit Acc (Backward)

        'Mark Brake Positions
        For i = MODdata.tDim To 1 Step -1
            If Vh.V(i - 1) - Vh.V(i) > 0.0001 And Not Positions(i) = 4 Then Positions(i) = 1
        Next

        'Look-Ahead Coasting
        i = MODdata.tDim + 1
        Do
            i -= 1

            'Secondary Progressbar
            If ProgBarLACpart > 0 Then ProgBarCtrl.ProgJobInt = CInt((100 / ProgBarShare) * (MODdata.tDim - i) / MODdata.tDim + (ProgBarLACpart - 1) * (100 / ProgBarShare))

            'Check if cancellation pending 
            If VECTOworker.CancellationPending Then Return True

            If Positions(i) = 1 Then
                vset2 = Vh.V(i)
                For j = i To 0 Step -1
                    If Positions(j) = 0 Or Positions(j) = 4 Then
                        vset1 = Vh.V(j)
                        Exit For
                    End If
                Next

                'Calc Coasting-Start time step
                If VEC.LookAheadOn Then
                    Tlookahead = CInt((vset2 - vset1) / VEC.a_lookahead)
                    t = Math.Max(0, i - Tlookahead)
                End If

                'Check if target-speed change inside of Coasting Phase
                For i0 = i To t Step -1
                    If i0 = 0 Then Exit For
                    If Vh.Vsoll(i0) - Vh.Vsoll(i0 - 1) > 0.0001 Then
                        t = Math.Min(i0 + 1, i)
                        Exit For
                    End If
                Next

                LookAheadDone = False

                'Limit deceleration
                adec = VEC.aDesMin(Vact)
                If Vh.a(i) < adec Then Vh.SetMinAccBackw(i)

                i0 = i

                'If vehicle stops too early reduce coasting time, i.e. set  Coasting-Start later
                If VEC.LookAheadOn Then
                    Do While i0 > t AndAlso fCoastingSpeed(t, dist(t), Gears(t), i0 - t) < Vh.V(i0)
                        t += 1
                    Loop
                End If


                Do
                    i -= 1
                    aact = Vh.a(i)
                    Vact = Vh.V(i)
                    adec = VEC.aDesMin(Vact)

                    If aact < adec Then
                        Vh.SetMinAccBackw(i)
                        Positions(i) = 2
                    Else
                        'Coasting (Forward)
                        If VEC.LookAheadOn And Vact >= VEC.vMinLA / 3.6 Then

                            For j = t To i0
                                Vact = Vh.V(j)
                                vCoasting = fCoastingSpeed(j, dist(j), Gears(j))
                                aCoasting = (2 * vCoasting - Vh.V0(j)) - Vh.V0(j)
                                If vCoasting < Vact And aCoasting >= VEC.aDesMin(Vact) Then
                                    'If Vrollout < Vist Then
                                    Vh.SetSpeed(j, vCoasting)
                                    Positions(j) = 3
                                    '   Vh.NoDistCorr(j) = True
                                Else
                                    Exit For
                                End If
                            Next

                        End If

                        LookAheadDone = True
                    End If

                Loop Until LookAheadDone Or i = 0

                i = i0

            End If

        Loop Until i = 0

        Return True

    End Function

    Public Function Calc() As Boolean

        Dim i As Integer
        Dim M As Single
        Dim nU As Single
        Dim omega_p As Single
        Dim omega1 As Single
        Dim omega2 As Single
        Dim nUx As Single
        Dim PminX As Single

        Dim jz As Integer

        'Start/Stop Control
        Dim StStOff As Boolean
        Dim StStTx As Single
        Dim StStDelayTx As Integer
        Dim StStPossible As Boolean

        Dim Vh As cVh

        Dim Gear As Integer

        Dim P As Single
        Dim Pclutch As Single
        Dim PaMot As Single
        Dim PaGbx As Single
        Dim Pmin As Single
        Dim Pmax As Single
        Dim Paux As Single
        Dim Pbrake As Single
        Dim PlossGB As Single
        Dim PlossDiff As Single
        Dim PlossRt As Single
        Dim PlossTC As Single
        Dim GVset As Boolean
        Dim Vrollout As Single
        Dim SecSpeedRed As Integer
        Dim FirstSecItar As Boolean
        Dim TracIntrIs As Single
        Dim amax As Single
        Dim ProgBarShare As Int16
        Dim LastPmax As Single
        Dim dist As Double
        Dim dist0 As Double
        Dim Tq As Single
        Dim LastGear As Integer

        Dim MsgSrc As String

        MsgSrc = "Power/Calc"

        'Abort if no speed given
        If Not DRI.Vvorg Then
            WorkerMsg(tMsgID.Err, "Driving cycle is not valid! Vehicle Speed required.", MsgSrc)
            Return False
        End If

        'Messages
        If Not Cfg.DistCorr Then WorkerMsg(tMsgID.Warn, "Distance Correction is disabled!", MsgSrc)

        '   Initialize
        Vh = MODdata.Vh

        If VEC.EcoRollOn Or VEC.OverSpeedOn Or VEC.LookAheadOn Then
            ProgBarShare = 2
        Else
            ProgBarShare = 1
        End If

        If GBX.TCon Then
            EngSideInertia = ENG.I_mot + GBX.TCinertia
        Else
            EngSideInertia = ENG.I_mot
        End If

        If Cfg.GnUfromCycle Then
            GearInput = DRI.Gvorg
            RpmInput = DRI.Nvorg
            If Not Cfg.BatchMode Then
                If GearInput Then WorkerMsg(tMsgID.Normal, "Using gears from driving cycle", MsgSrc)
                If RpmInput Then WorkerMsg(tMsgID.Normal, "Using rpm from driving cycle", MsgSrc)
            End If
        Else
            If (DRI.Gvorg Or DRI.Nvorg) And Not Cfg.BatchMode Then WorkerMsg(tMsgID.Warn, "Gears/rpm from driving cycle ignored.", MsgSrc)
            GearInput = False
            RpmInput = False
        End If
        StStOff = False
        StStTx = 0
        StStDelayTx = 0
        SecSpeedRed = 0

        If GBX.TracIntrSi < 0.001 Then
            TracIntrI = 0
        Else
            TracIntrI = CInt(Math.Max(1, Math.Round(GBX.TracIntrSi, 0, MidpointRounding.AwayFromZero)))
        End If
        TracIntrIx = 0
        TracIntrOn = False
        TracIntrTurnOff = False

        ClutchNorm = 0.03
        ClutchEta = 1

        Tq = 0
        LastGear = 0

        LastClutch = tEngClutch.Opened

        'Theoretical maximum speed [m/s] - set to Speed ​​at 1.2 x Nominal-Revolutions in top-Gear
        GVmax = 1.2 * ENG.Nrated * 2 * VEH.rdyn * Math.PI / (1000 * GBX.Igetr(0) * GBX.Igetr(GBX.GearCount) * 60)

        dist = 0
        dist0 = 0

        jz = -1

        '***********************************************************************************************
        '***********************************     Time-loop      ****************************************
        '***********************************************************************************************

        Do
            jz += 1

            MODdata.ModErrors.ResetAll()

            GVset = False
            FirstSecItar = True

            'Secondary Progressbar
            ProgBarCtrl.ProgJobInt = CInt((100 / ProgBarShare) * jz / MODdata.tDim + (100 - 100 / ProgBarShare))


            '   Determine State
lbGschw:

            'Reset the second by second Errors
            MODdata.ModErrors.GeschRedReset()

            'Calculate Speed​/Acceleration -------------------
            'Now through DRI-class
            Vact = Vh.V(jz)
            aact = Vh.a(jz)

            'distance 
            dist = dist0 + Vact

            StStPossible = False
            EngState0 = tEngState.Undef

            'If Speed over Top theoretical Speed => Reduce
            If Vact > GVmax + 0.0001 And Not GVset Then
                Vh.SetSpeed0(jz, GVmax)
                GVset = True
                GoTo lbGschw
            End If

            'Check if Acceleration is too high
            amax = VEC.aDesMax(Vact)

            If amax < 0.0001 Then
                WorkerMsg(tMsgID.Err, "aDesMax(acc) invalid! v= " & Vact & ", aDesMax(acc) =" & amax, MsgSrc)
                Return False
            End If

            If aact > amax + 0.0001 Then

                'Vh.SetSpeed0(jz, Vh.V0(jz) + amax)
                Vh.SetMaxAcc(jz)

                GoTo lbGschw


            ElseIf FirstSecItar Then    'this is necessary to avoid speed reduction failure

                'Check whether Deceleration too high
                amax = VEC.aDesMin(Vact)
                If amax > -0.001 Then
                    WorkerMsg(tMsgID.Err, "aDesMax(dec) invalid! v= " & Vact & ", aDesMax(dec) =" & amax, MsgSrc)
                    Return False
                End If
                If aact < amax - 0.0001 And Not Vh.EcoRoll(jz) Then
                    Vh.SetSpeed0(jz, Vh.V0(jz) + amax)
                    GoTo lbGschw
                End If


            End If


            'From Power -----
            If aact < 0 Then
                If (Vact < 0.025) Then
                    'Vh.SetSpeed(jz, 0)
                    'GoTo lbGschw
                    Vact = 0
                End If
            End If
            '---------------

            'Determine Driving-state  -------------------------
            Pplus = False
            Pminus = False

            If Vact < 0.0001 Then
                VehState0 = tVehState.Stopped
            Else
                If aact >= 0.01 Then
                    VehState0 = tVehState.Acc
                ElseIf aact < -0.01 Then
                    VehState0 = tVehState.Dec
                Else
                    VehState0 = tVehState.Cruise
                End If
            End If

            Pwheel = fPwheel(jz, Vh.fGrad(dist))

            Select Case Pwheel
                Case Is > 0.0001
                    Pplus = True
                Case Is < -0.0001
                    Pminus = True
            End Select

            'Eco-Roll Speed Correction (because PreRun speed profile might still be too high or speed might generally be too low)
            If Vh.EcoRoll(jz) AndAlso Vact > MODdata.Vh.Vsoll(jz) - VEC.UnderSpeed / 3.6 AndAlso Not VehState0 = tVehState.Stopped AndAlso Pplus Then
                If Not Vh.ReduceSpeed(jz, 0.9999) Then
                    WorkerMsg(tMsgID.Err, "Engine full load too low for vehicle start! Road gradient = " & Vh.fGrad(dist) & "[%] at " & dist.ToString("#.0") & "[m]", MsgSrc & "/t= " & jz + 1)
                    Return False
                End If
                FirstSecItar = False
                GoTo lbGschw
            End If


            '************************************ Gear selection ************************************
            If VehState0 = tVehState.Stopped Or TracIntrOn Then

                If TracIntrTurnOff And Not VehState0 = tVehState.Stopped Then

                    Gear = TracIntrGear

                    If Not GBX.TCon AndAlso fnn(Vact, Gear, False) < ClutchNorm And Pplus Then
                        Clutch = tEngClutch.Slipping
                    Else
                        Clutch = tEngClutch.Closed
                    End If

                Else
                    Gear = 0
                    Clutch = tEngClutch.Opened
                End If

            Else

                'Check whether Clutch will slip (important for Gear-shifting model):
                If Not GBX.TCon AndAlso fnn(Vact, 1, False) < ClutchNorm And Pplus Then
                    Clutch = tEngClutch.Slipping
                Else
                    Clutch = tEngClutch.Closed
                End If

                If GearInput Then
                    'Gear-settings
                    Gear = Math.Min(Vh.GearVorg(jz), GBX.GearCount)
                ElseIf RpmInput Then
                    'Revolutions-setting
                    Gear = fGearByU(MODdata.nUvorg(jz), Vact)
                ElseIf GBX.GearCount = 1 Then
                    Gear = 1
                Else

                    'Gear-shifting Model
                    If GBX.TCon Then

                        If jz > 0 Then Tq = nPeToM(fnU(Vact, LastGear, False), fPeGearMod(LastGear, jz, Vh.fGrad(dist)))

                        Gear = fGearTC(jz, Vh.fGrad(dist), Tq)
                    Else
                        Gear = fGearVECTO(jz, Vh.fGrad(dist))
                    End If

                    'Must be reset here because the Gear-shifting model may cause changes
                    MODdata.ModErrors.PxReset()

                End If

                'Gear shifting-model / gear input can open Clutch
                If Gear < 1 Then

                    Clutch = tEngClutch.Opened

                Else

                    If Not GBX.TCon AndAlso fnn(Vact, Gear, False) < ClutchNorm And Pplus And Not VehState0 = tVehState.Dec Then
                        Clutch = tEngClutch.Slipping
                    Else
                        Clutch = tEngClutch.Closed
                    End If

                End If

            End If

            If Gear = -1 Then
                WorkerMsg(tMsgID.Err, "Error in Gear Shift Model!", MsgSrc & "/t= " & jz + 1)
                Return False
            End If

            'Eco-Roll (triggers if Pwheel < 2 [kW])
            If Vh.EcoRoll(jz) AndAlso Pwheel <= 0 Then
                Clutch = tEngClutch.Opened
                Gear = 0
            End If

            If Gear = 1 And Pminus And Vact <= 5 / 3.6 Then
                Clutch = tEngClutch.Opened
                Gear = 0
            End If

            ' Important checks
lbCheck:

            'Falls vor Gangwahl festgestellt wurde, dass nicht KupplSchleif, dann bei zu niedriger Drehzahl runterschalten: |@@| If before?(vor) Gear-shift is detected that Clutch does not Lock, then Downshift at too low Revolutions:
            If Not GBX.TCon Then
                If Clutch = tEngClutch.Closed Then
                    If fnn(Vact, Gear, False) < ClutchNorm And Not VehState0 = tVehState.Dec And Gear > 1 Then Gear -= 1
                End If
            End If


            'Check whether idling although Power > 0
            '   if power at wheels > 0.2 [kW], then clutch in
            If Clutch = tEngClutch.Opened Then
                If Pwheel > 0.2 Then

                    If TracIntrOn Then
                        Gear = TracIntrGear
                    Else
                        Gear = 1
                    End If


                    If Not GBX.TCon AndAlso fnn(Vact, Gear, False) < ClutchNorm Then
                        Clutch = tEngClutch.Slipping
                    Else
                        Clutch = tEngClutch.Closed
                    End If

                    GoTo lbCheck

                End If
            End If

            '************************************ Revolutions ************************************

            '*** If Revolutions specified then the next block is skipped ***
            If RpmInput Then

                nU = MODdata.nUvorg(jz)

                'If Start/Stop then it will be set at the same nn < -0.05 to nU = 0
                If VEC.StartStop And nU < ENG.Nidle - 100 Then
                    If Pplus Then
                        nU = ENG.Nidle
                        If FirstSecItar Then WorkerMsg(tMsgID.Warn, "target rpm < rpm_idle while power demand > 0", MsgSrc & "/t= " & jz + 1)
                    Else
                        nU = 0
                    End If
                End If

                If nU < ENG.Nidle - 100 And Not VEC.StartStop Then
                    If FirstSecItar Then WorkerMsg(tMsgID.Warn, "target rpm < rpm_idle (Start/Stop disabled)", MsgSrc & "/t= " & jz + 1)
                End If

                GoTo lb_nOK

            End If

            'Revolutions drop when decoupling
            If Clutch = tEngClutch.Opened Then
                If jz = 0 Then
                    nU = ENG.Nidle
                Else

                    If MODdata.nU(jz - 1) <= ENG.Nidle + 0.00001 Then
                        nU = MODdata.nU(jz - 1)
                        GoTo lb_nOK
                    End If


                    nUx = MODdata.nU(jz - 1)
                    omega1 = nUx * 2 * Math.PI / 60
                    Pmin = 0
                    nU = nUx
                    i = 0
                    Do
                        PminX = Pmin
                        Pmin = FLD(Gear).Pdrag(nU)

                        'Limit Power-drop to 75%
                        P = (MODdata.Pe(jz - 1) - MODdata.PauxSum(jz - 1)) - 0.75 * ((MODdata.Pe(jz - 1) - MODdata.PauxSum(jz - 1)) - Pmin)

                        M = -P * 1000 * 60 / (2 * Math.PI * nU)
                        omega_p = M / EngSideInertia
                        omega2 = omega1 - omega_p
                        nU = omega2 * 60 / (2 * Math.PI)
                        i += 1
                        '01:10:12 Luz: Revolutions must not be higher than previously
                        If nU > nUx Then
                            nU = nUx
                            Exit Do
                        End If
                    Loop Until Math.Abs(Pmin - PminX) < 0.001 Or nU <= ENG.Nidle Or i = 999

                    'If i = 999 Then WorkerMsg(tMsgID.Warn, "i=999", MsgSrc & "/t= " & jz + 1)

                    nU = Math.Max(ENG.Nidle, nU)

                    MODdata.ModErrors.FLDextrapol = ""

                End If

            Else

                If GBX.TCon And GBX.IsTCgear(Gear) Then

                    PlossGB = fPlossGB(Pwheel, Vact, Gear, False)
                    PlossDiff = fPlossDiff(Pwheel, Vact, False)
                    PlossRt = fPlossRt(Vact, Gear)
                    PaGbx = fPaG(Vact, aact)
                    Pclutch = Pwheel + PlossGB + PlossDiff + PaGbx + PlossRt

                    If jz = 0 Then
                        If Not GBX.TCiteration(Gear, fnUout(Vact, Gear), Pclutch, jz) Then
                            WorkerMsg(tMsgID.Err, "TC Iteration failed!", MsgSrc & "/t= " & jz + 1)
                            Return False
                        End If
                    Else
                        If Not GBX.TCiteration(Gear, fnUout(Vact, Gear), Pclutch, jz, MODdata.nU(jz - 1), MODdata.Pe(jz - 1)) Then
                            WorkerMsg(tMsgID.Err, "TC Iteration failed!", MsgSrc & "/t= " & jz + 1)
                            Return False
                        End If
                    End If

                    If GBX.TCNeutral Then
                        Gear = 0
                        Clutch = tEngClutch.Opened
                        GoTo lbCheck
                    End If

                    If GBX.TCReduce Then
                        If Not Vh.ReduceSpeed(jz, 0.999) Then
                            WorkerMsg(tMsgID.Err, "Engine full load too low for vehicle start (speed reduction failed) !", MsgSrc & "/t= " & jz + 1)
                            Return False
                        End If
                        FirstSecItar = False
                        GoTo lbGschw
                    End If

                    nU = GBX.TCnUin

                Else

                    nU = fnU(Vact, Gear, Clutch = tEngClutch.Slipping)

                    '*** Start: Revolutions Check

                    'Check whether Revolutions too high! => Speed Reduction
                    Do While Gear < GBX.GearCount AndAlso nU > 1.2 * (ENG.Nrated - ENG.Nidle) + ENG.Nidle
                        Gear += 1
                        nU = fnU(Vact, Gear, Clutch = tEngClutch.Slipping)
                    Loop

                    'Check whether Revolutions too low with the Clutch closed
                    If Clutch = tEngClutch.Closed Then
                        If nU < ENG.Nidle + 0.0001 Then
                            Gear -= 1
                            If Gear = 0 Then Clutch = tEngClutch.Opened
                            GoTo lbCheck
                            'nU = fnU(Vact, Gear, Clutch = tEngClutch.Slipping)
                        End If
                    End If

                End If

            End If




lb_nOK:


            '************************************ Determine Engine-state ************************************
            ' nU is final here!

            'Power at clutch
            Select Case Clutch
                Case tEngClutch.Opened
                    Pclutch = 0
                    PlossGB = 0
                    PlossDiff = 0
                    PlossRt = 0
                    PlossTC = 0
                    PaGbx = 0
                Case tEngClutch.Closed

                    If GBX.TCon And GBX.IsTCgear(Gear) Then

                        Pclutch = nMtoPe(nU, GBX.TCMin)

                        If Pclutch >= 0 Then
                            PlossTC = Math.Abs(nMtoPe(GBX.TCnUin, GBX.TCMin) * (1 - GBX.TC_mu * GBX.TC_nu))
                        Else
                            PlossTC = Math.Abs(nMtoPe(GBX.TCnUout, GBX.TCMout) * (1 - GBX.TC_mu * GBX.TC_nu))
                        End If

                    Else

                        PlossGB = fPlossGB(Pwheel, Vact, Gear, False)
                        PlossDiff = fPlossDiff(Pwheel, Vact, False)
                        PlossRt = fPlossRt(Vact, Gear)
                        PlossTC = 0
                        PaGbx = fPaG(Vact, aact)
                        Pclutch = Pwheel + PlossGB + PlossDiff + PaGbx + PlossRt

                    End If
                Case Else 'tEngClutch.Slipping: never in AT mode!
                    PlossGB = fPlossGB(Pwheel, Vact, Gear, False)
                    PlossDiff = fPlossDiff(Pwheel, Vact, False)
                    PlossRt = fPlossRt(Vact, Gear)
                    PlossTC = 0
                    PaGbx = fPaG(Vact, aact)
                    Pclutch = (Pwheel + PlossGB + PlossDiff + PaGbx + PlossRt) / ClutchEta
            End Select


            'Total aux power
            '[kW]
            Paux = fPaux(jz, Math.Max(nU, ENG.Nidle))


            'ICE-inertia
            If jz = 0 Then
                PaMot = 0
            Else
                'Not optimal since jz-1 to jz not the right interval
                PaMot = fPaMot(nU, MODdata.nU(jz - 1))
            End If

            'Internal Engine Power (Pclutch plus Aux plus Inertia)
            P = Pclutch + Paux + PaMot
            Tq = nPeToM(nU, P)

            'EngState
            If Clutch = tEngClutch.Opened Then

                'Start/Stop >>> tEngState.Stopped
                If VEC.StartStop AndAlso Vact <= VEC.StStV / 3.6 AndAlso Math.Abs(PaMot) < 0.0001 Then
                    StStPossible = True
                    If StStOff And jz > 0 Then
                        If MODdata.EngState(jz - 1) = tEngState.Stopped Then
                            P = 0
                            EngState0 = tEngState.Stopped
                        End If
                    Else
                        P = 0
                        EngState0 = tEngState.Stopped
                    End If
                End If

                Select Case P
                    Case Is > 0.0001    'Antrieb
                        EngState0 = tEngState.Load

                    Case Is < -0.0001   'Schlepp
                        EngState0 = tEngState.Drag

                    Case Else           'Idle/Stop
                        If Not EngState0 = tEngState.Stopped Then EngState0 = tEngState.Idle
                End Select

            Else

                If P < 0 Then
                    EngState0 = tEngState.Drag
                Else
                    EngState0 = tEngState.Load
                End If

            End If



            '*************** Leistungsverteilung usw. ****************** |@@| Power distribution, etc. ******************

            'Full-Load/Drag curve
            If EngState0 = tEngState.Stopped Then

                Pmin = 0
                Pmax = 0

                'Revolutions Correction
                nU = 0

            Else

                Pmin = FLD(Gear).Pdrag(nU)

                If jz = 0 Then
                    Pmax = FLD(Gear).Pfull(nU)
                Else
                    Pmax = FLD(Gear).Pfull(nU, MODdata.Pe(jz - 1))
                End If

                'If Pmax < 0 or Pmin > 0 then Abort with Error!
                If Pmin >= 0 And P < 0 Then
                    WorkerMsg(tMsgID.Err, "Pe_drag > 0! n= " & nU & " [1/min]", MsgSrc & "/t= " & jz + 1, FLD(Gear).FilePath)
                    Return False
                ElseIf Pmax <= 0 And P > 0 Then
                    WorkerMsg(tMsgID.Err, "Pe_full < 0! n= " & nU & " [1/min]", MsgSrc & "/t= " & jz + 1, FLD(Gear).FilePath)
                    Return False
                End If

            End If

            '   => Pbrake
            If Clutch = tEngClutch.Opened Then
                If Pwheel < -0.00001 Then
                    Pbrake = Pwheel
                Else
                    Pbrake = 0
                End If

                If P < Pmin Then P = Pmin

            Else
                If EngState0 = tEngState.Load Then
                    Pbrake = 0
                    If GBX.TCon And GBX.IsTCgear(Gear) Then Pbrake = GBX.TC_PeBrake
                    If Math.Abs(P / Pmax - 1) < 0.02 Then EngState0 = tEngState.FullLoad
                Else    ' tEngState.Drag (tEngState.Idle, tEngState.Stopped kann's hier nicht geben weil Clutch <> Closed)
                    If P < Pmin Then

                        'Overspeed
                        'If Not OvrSpeed AndAlso Not VehState0 = tVehState.Dec Then

                        '    OvrSpeed = True

                        '    If DEV.OverSpeedOn And (Pmin - P) / VEH.Pnenn > DEV.SpeedPeEps Then

                        '        Vcoasting = fCoastingSpeed(jz, Gear)

                        '        If Vcoasting <= MODdata.Vh.Vsoll(jz) + DEV.OverSpeed / 3.6 Then
                        '            Vh.SetSpeed(jz, Vcoasting)
                        '            GoTo lbGschw
                        '        ElseIf Vist < 0.999 * (MODdata.Vh.Vsoll(jz) + DEV.OverSpeed / 3.6) Then
                        '            Vh.SetSpeed(jz, MODdata.Vh.Vsoll(jz) + DEV.OverSpeed / 3.6)
                        '            GoTo lbGschw
                        '        End If

                        '    End If
                        'End If

                        MODdata.ModErrors.TrLossMapExtr = ""

                        'VKM to Drag-curve
                        P = Pmin

                        'Forward-calculation to Wheel (PvorD)
                        Pclutch = P - Paux - PaMot
                        PaGbx = fPaG(Vact, aact)
                        PlossGB = fPlossGBfwd(Pclutch, Vact, Gear, False)
                        PlossRt = fPlossRt(Vact, Gear)
                        PlossDiff = fPlossDiffFwd(Pclutch - PlossGB - PlossRt, Vact, False)

                        Pbrake = Pwheel - (Pclutch - PlossGB - PlossDiff - PaGbx - PlossRt)

                        EngState0 = tEngState.FullDrag
                    Else
                        Pbrake = 0
                    End If
                End If
            End If

            'Check if cancellation pending (before Speed-reduce-iteration, otherwise it hangs)
            If VECTOworker.CancellationPending Then Return True

            'Check whether P above Full-load => Reduce Speed
            If P > Pmax Then
                If EngState0 = tEngState.Load Or EngState0 = tEngState.FullLoad Then
                    If Vact > 0.01 Then
                        Vh.ReduceSpeed(jz, 0.9999)
                        FirstSecItar = False
                        GoTo lbGschw
                    Else
                        'ERROR: Speed Reduction failed. (Road gradient too high)
                        WorkerMsg(tMsgID.Err, "Engine full load too low for vehicle start! Road gradient = " & Vh.fGrad(dist) & "[%] at " & dist.ToString("#.0") & "[m]", MsgSrc & "/t= " & jz + 1)
                        Return False
                    End If
                Else 'tEngState.Idle, tEngState.Stopped, tEngState.Drag
                    If FirstSecItar Then
                        If P > 0.1 Then WorkerMsg(tMsgID.Warn, "Pwheel > 0 but EngState undefined ?!", MsgSrc & "/t= " & jz + 1)
                    End If
                End If
            End If


            'Interruption of traction(Zugkraftunterbrechung)
            If TracIntrI > 0 Then

                If Not TracIntrOn Then

                    If jz > 0 AndAlso Gear > 0 AndAlso MODdata.Gear(jz - 1) > 0 AndAlso Gear <> MODdata.Gear(jz - 1) Then

                        TracIntrGear = Gear
                        Gear = 0
                        Clutch = tEngClutch.Opened
                        TracIntrIx = 0
                        TracIntrOn = True

                        If TracIntrIx + 1 = TracIntrI Then
                            TracIntrIs = GBX.TracIntrSi - CSng(TracIntrIx)
                        Else
                            TracIntrIs = 1
                        End If

                        Vrollout = fRolloutSpeed(jz, TracIntrIs, Vh.fGrad(dist))

                        If Vrollout < Vact Or VehState0 <> tVehState.Dec Then Vh.SetSpeed(jz, Vrollout)

                        GoTo lbGschw

                    End If

                End If

            End If

            '--------------------------------------------------------------------------------------------------
            '------------------------- PNR --------------------------------------------------------------------
            '--------------------------------------------------------------------------------------------------
            '   Finish Second


            'If Gear = GBX.GearCount Then
            '    Debug.Print(jz + 1 & ",-")
            'Else
            '    Debug.Print(jz + 1 & "," & fnU(Vact, Gear + 1, False))
            'End If

            'distance 
            dist0 += Vact

            'Start / Stop - Activation-Speed Control
            If VEC.StartStop Then
                If StStPossible Then
                    StStDelayTx += 1
                Else
                    StStDelayTx = 0
                End If
                If StStOff Then
                    If Not EngState0 = tEngState.Stopped Then
                        StStTx += 1
                        If StStTx > VEC.StStT And StStDelayTx >= VEC.StStDelay Then
                            StStTx = 1
                            StStOff = False
                        End If
                    End If
                Else
                    If EngState0 = tEngState.Stopped Or StStDelayTx < VEC.StStDelay Then StStOff = True
                End If
            End If

            'Write Modal-values Fields
            MODdata.Pe.Add(P)
            MODdata.nU.Add(nU)

            MODdata.EngState.Add(EngState0)

            MODdata.Pa.Add(fPaFZ(MODdata.Vh.V(jz), MODdata.Vh.a(jz)))
            MODdata.Pair.Add(fPair(MODdata.Vh.V(jz), jz))
            MODdata.Proll.Add(fPr(MODdata.Vh.V(jz), Vh.fGrad(dist)))
            MODdata.Pstg.Add(fPs(MODdata.Vh.V(jz), Vh.fGrad(dist)))
            MODdata.Pbrake.Add(Pbrake)
            MODdata.Psum.Add(Pwheel)
            MODdata.PauxSum.Add(Paux)
            MODdata.Grad.Add(Vh.fGrad(dist))

            For Each AuxID As String In VEC.AuxRefs.Keys
                MODdata.Paux(AuxID).Add(VEC.Paux(AuxID, jz, Math.Max(nU, ENG.Nidle)))
            Next

            MODdata.PlossGB.Add(PlossGB)
            MODdata.PlossDiff.Add(PlossDiff)
            MODdata.PlossRt.Add(PlossRt)
            MODdata.PlossTC.Add(PlossTC)
            MODdata.PaEng.Add(PaMot)
            MODdata.PaGB.Add(PaGbx)
            MODdata.Pclutch.Add(Pclutch)

            MODdata.VehState.Add(VehState0)
            MODdata.Gear.Add(Gear)

            'Torque Converter output
            If GBX.TCon Then
                If GBX.IsTCgear(Gear) Then
                    If nU = 0 Then
                        MODdata.TCnu.Add(0)
                    Else
                        MODdata.TCnu.Add(GBX.TCnUout / nU)
                    End If
                    If GBX.TCMin = 0 Then
                        MODdata.TCmu.Add(0)
                    Else
                        MODdata.TCmu.Add(GBX.TCMout / GBX.TCMin)
                    End If
                    MODdata.TCMout.Add(GBX.TCMout)
                    MODdata.TCnOut.Add(GBX.TCnUout)
                Else
                    If Clutch = tEngClutch.Opened Then
                        MODdata.TCnu.Add(0)
                        MODdata.TCmu.Add(0)
                        MODdata.TCMout.Add(0)
                        MODdata.TCnOut.Add(0)
                    Else
                        MODdata.TCnu.Add(1)
                        MODdata.TCmu.Add(1)
                        MODdata.TCMout.Add(nPeToM(nU, Pclutch))
                        MODdata.TCnOut.Add(nU)
                    End If
                End If
            End If

            Vh.DistCorrection(jz, VehState0)

            'Traction Interruption
            If TracIntrTurnOff Then

                TracIntrOn = False
                TracIntrTurnOff = False

            ElseIf TracIntrOn Then

                TracIntrIx += 1

                If TracIntrIx = TracIntrI Then

                    TracIntrTurnOff = True

                ElseIf jz < MODdata.tDim Then

                    If TracIntrIx + 1 = TracIntrI Then
                        TracIntrIs = GBX.TracIntrSi - CSng(TracIntrIx)
                    Else
                        TracIntrIs = 1
                    End If

                    Vrollout = fRolloutSpeed(jz + 1, TracIntrIs, Vh.fGrad(dist))
                    If Vrollout < Vh.V(jz + 1) Or VehState0 <> tVehState.Dec Then Vh.SetSpeed(jz + 1, Vrollout)

                End If

            End If

            If Vh.Vsoll(jz) - Vact > 1.5 Then SecSpeedRed += 1


            LastGearChange = -1
            For i = jz - 1 To 0 Step -1
                If MODdata.Gear(i) <> 0 Then
                    If MODdata.Gear(i) <> Gear Then
                        LastGearChange = i
                        Exit For
                    End If
                End If
            Next


            LastClutch = Clutch

            'Messages
            If MODdata.ModErrors.MsgOutputAbort(jz + 1, MsgSrc) Then Return False

            If Clutch = tEngClutch.Closed And RpmInput Then
                If Math.Abs(nU - fnU(Vact, Gear, False)) > 0.2 * ENG.Nrated Then
                    WorkerMsg(tMsgID.Warn, "Target rpm =" & nU & ", calculated rpm(gear " & Gear & ")= " & fnU(Vact, Gear, False), MsgSrc & "/t= " & jz + 1)
                End If
            End If

            LastPmax = Pmax

            LastGear = Gear

        Loop Until jz >= MODdata.tDim

        '***********************************************************************************************
        '***********************************    Time loop END ***********************************
        '***********************************************************************************************

        'Notify
        If Cfg.DistCorr Then
            If MODdata.tDim > MODdata.tDimOgl Then WorkerMsg(tMsgID.Normal, "Cycle extended by " & MODdata.tDim - MODdata.tDimOgl & " seconds to meet target distance.", MsgSrc)

            If Math.Abs(Vh.WegIst - Vh.WegSoll) > 80 Then
                WorkerMsg(tMsgID.Warn, "Target distance= " & (Vh.WegSoll / 1000).ToString("#.000") & "[km], Actual distance= " & (Vh.WegIst / 1000).ToString("#.000") & "[km], Error= " & Math.Abs(Vh.WegIst - Vh.WegSoll).ToString("#.0") & "[m]", MsgSrc)
            Else
                WorkerMsg(tMsgID.Normal, "Target distance= " & (Vh.WegSoll / 1000).ToString("#.000") & "[km], Actual distance= " & (Vh.WegIst / 1000).ToString("#.000") & "[km], Error= " & Math.Abs(Vh.WegIst - Vh.WegSoll).ToString("#.0") & "[m]", MsgSrc)
            End If
        End If

        If SecSpeedRed > 0 Then WorkerMsg(tMsgID.Normal, "Speed reduction > 1.5 m/s in " & SecSpeedRed & " time steps.", MsgSrc)


        'CleanUp
        Vh = Nothing

        Return True

    End Function

    Public Function Eng_Calc(ByVal NoWarnings As Boolean) As Boolean

        Dim Pmr As Single
        Dim t As Integer
        Dim t1 As Integer
        Dim Pmin As Single
        Dim Pmax As Single
        Dim nUDRI As List(Of Double)
        Dim PeDRI As List(Of Double)
        Dim PcorCount As Integer
        Dim MsgSrc As String
        Dim Padd As Single

        MsgSrc = "Power/Eng_Calc"

        'Abort if Power/Revolutions not given
        If Not (DRI.Nvorg And DRI.Pvorg) Then
            WorkerMsg(tMsgID.Err, "Load cycle is not valid! rpm and load required.", MsgSrc)
            Return False
        End If

        PcorCount = 0
        t1 = MODdata.tDim
        nUDRI = DRI.Values(tDriComp.nU)
        PeDRI = DRI.Values(tDriComp.Pe)

        'Drehzahlen vorher weil sonst scheitert die Pmr-Berechnung bei MODdata.nU(t + 1) |@@| Revolutions previously, otherwise Pmr-calculation fails at MODdata.nU(t + 1)
        For t = 0 To t1
            MODdata.nU.Add(Math.Max(0, nUDRI(t)))
        Next

        'Power calculation
        For t = 0 To t1

            'Secondary Progressbar
            ProgBarCtrl.ProgJobInt = CInt(100 * t / t1)

            'Reset the second-by-second Errors
            MODdata.ModErrors.ResetAll()

            'OLD and wrong because not time shifted: P_mr(jz) = 0.001 * (I_mot * 0.0109662 * (n(jz) * nnrom) * nnrom * (n(jz) - n(jz - 1))) 
            If t > 0 And t < t1 Then
                Pmr = 0.001 * (ENG.I_mot * (2 * Math.PI / 60) ^ 2 * ((MODdata.nU(t + 1) + MODdata.nU(t - 1)) / 2) * 0.5 * (MODdata.nU(t + 1) - MODdata.nU(t - 1)))
            Else
                Pmr = 0
            End If

            Padd = MODdata.Vh.Padd(t)

            'Power = P_clutch + + Pa_eng + Padd
            MODdata.Pe.Add(PeDRI(t) + (Pmr + Padd))

            'Revolutions of the Cycle => Determined in Cycle-init
            'If Revolutions under idle, assume Engine is stopped
            If MODdata.nU(t) < ENG.Nidle - 100 Then
                EngState0 = tEngState.Stopped
            Else
                Pmin = FLD(0).Pdrag(MODdata.nU(t))

                If t = 0 Then
                    Pmax = FLD(0).Pfull(MODdata.nU(t))
                Else
                    Pmax = FLD(0).Pfull(MODdata.nU(t), MODdata.Pe(t - 1))
                End If

                'If Pmax < 0 or Pmin >  0 then Abort with Error!
                If Pmin >= 0 AndAlso MODdata.Pe(t) < 0 Then
                    WorkerMsg(tMsgID.Err, "Pe_drag > 0! n= " & MODdata.nU(t) & " [1/min]", MsgSrc & "/t= " & t + 1, FLD(0).FilePath)
                    Return False
                ElseIf Pmax <= 0 AndAlso MODdata.Pe(t) > 0 Then
                    WorkerMsg(tMsgID.Err, "Pe_full < 0! n= " & MODdata.nU(t) & " [1/min]", MsgSrc & "/t= " & t + 1, FLD(0).FilePath)
                    Return False
                End If

                'FLD Check
                If MODdata.Pe(t) > Pmax Then
                    If MODdata.Pe(t) / Pmax > 1.05 Then PcorCount += 1
                    MODdata.Pe(t) = Pmax
                ElseIf MODdata.Pe(t) < Pmin Then
                    If MODdata.Pe(t) / Pmin > 1.05 And MODdata.Pe(t) > -99999 Then PcorCount += 1
                    MODdata.Pe(t) = Pmin
                End If

                Select Case MODdata.Pe(t)
                    Case Is > 0.0001  'Antrieb
                        If Math.Abs(MODdata.Pe(t) / Pmax - 1) < 0.01 Then
                            EngState0 = tEngState.FullLoad
                        Else
                            EngState0 = tEngState.Load
                        End If
                    Case Is < -0.0001   'Schlepp
                        If Math.Abs(MODdata.Pe(t) / Pmin - 1) < 0.01 Then
                            EngState0 = tEngState.FullDrag
                        Else
                            EngState0 = tEngState.Drag
                        End If
                    Case Else
                        EngState0 = tEngState.Idle
                End Select
            End If

            MODdata.EngState.Add(EngState0)
            MODdata.PaEng.Add(Pmr)
            MODdata.Pclutch.Add(MODdata.Pe(t) - (Pmr + Padd))
            MODdata.PauxSum.Add(Padd)

            'Notify
            If MODdata.ModErrors.MsgOutputAbort(t + 1, MsgSrc) Then Return False

        Next

        If PcorCount > 0 And Not NoWarnings Then WorkerMsg(tMsgID.Warn, "Power corrected (>5%) in " & PcorCount & " time steps.", MsgSrc)

        Return True

    End Function

    Private Function fTracIntPower(ByVal t As Single, ByVal Gear As Integer) As Single
        Dim PminX As Single
        Dim P As Single
        Dim M As Single
        Dim Pmin As Single
        Dim nU As Single
        Dim omega_p As Single
        Dim omega1 As Single
        Dim omega2 As Single
        Dim nUx As Single
        Dim i As Integer


        nUx = MODdata.nU(t - 1)
        omega1 = nUx * 2 * Math.PI / 60
        Pmin = 0
        nU = nUx
        i = 0

        Do
            PminX = Pmin
            Pmin = FLD(Gear).Pdrag(nU)
            'Leistungsabfall limitieren auf Pe(t-1) minus 75% von (Pe(t-1) - Pschlepp) |@@| Limit Power-drop to Pe(t-1) minus 75% of (Pe(t-1) - Pdrag)
            '   aus Auswertung ETC des Motors mit dem dynamische Volllast parametriert wurde |@@| of the evaluated ETC of the Enginges with the dynamic parametrized Full-load
            '   Einfluss auf Beschleunigungsvermögen gering (Einfluss durch Pe(t-1) bei dynamischer Volllast mit PT1) |@@| Influence at low acceleration (influence dynamic Full-load through Pe(t-1) with PT1)
            '   Luz/Rexeis 21.08.2012
            '   Iteration loop: 01.10.2012
            P = MODdata.Pe(t - 1) - 0.75 * (MODdata.Pe(t - 1) - Pmin)
            M = -P * 1000 * 60 / (2 * Math.PI * nU)
            'original: M = -Pmin * 1000 * 60 / (2 * Math.PI * ((nU + nUx) / 2))
            omega_p = M / EngSideInertia
            omega2 = omega1 - omega_p
            nU = omega2 * 60 / (2 * Math.PI)
            i += 1
            '01:10:12 Luz: Revolutions must not be higher than previously
            If nU > nUx Then
                nU = nUx
                Exit Do
            End If
        Loop Until Math.Abs(Pmin - PminX) < 0.001 Or nU <= ENG.Nidle Or i = 999

        Return P


    End Function

    Private Function fRolloutSpeed(ByVal t As Integer, ByVal dt As Single, ByVal Grad As Single) As Single

        Dim vstep As Double
        Dim vVorz As Integer
        Dim PvD As Single
        Dim a As Single
        Dim v As Single
        Dim eps As Single
        Dim LastPvD As Single
        Dim vMin As Single

        v = MODdata.Vh.V(t)

        vMin = (MODdata.Vh.V0(t) + 0) / 2

        If v <= vMin Then Return vMin

        vstep = 0.1
        eps = 0.00005
        a = MODdata.Vh.a(t)

        PvD = fPwheel(t, v, a, Grad)

        If PvD > eps Then
            vVorz = -1
        ElseIf PvD < -eps Then
            vVorz = 1
        Else
            Return v
        End If

        LastPvD = PvD + 10 * eps

        Do While Math.Abs(PvD) > eps And Math.Abs(LastPvD - PvD) > eps

            If Math.Abs(LastPvD) < Math.Abs(PvD) Then
                vVorz *= -1
                vstep *= 0.5

                If vstep = 0 Then Exit Do

            End If

            v += vVorz * vstep

            If v < vMin Then

                LastPvD = 0
                v -= vVorz * vstep

            Else

                a = 2 * (v - MODdata.Vh.V0(t)) / dt

                LastPvD = PvD

                PvD = fPwheel(t, v, a, Grad)

            End If

        Loop

        Return v

    End Function

    Private Function fCoastingSpeed(ByVal t As Integer, ByVal s As Double, ByVal Gear As Integer) As Single

        Return fCoastingSpeed(t, s, Gear, MODdata.Vh.V(t), MODdata.Vh.a(t))

    End Function

    Private Function fCoastingSpeed(ByVal t As Integer, ByVal s As Double, ByVal Gear As Integer, ByVal v As Single, ByVal a As Single, Optional ByVal v0 As Single? = Nothing) As Single

        Dim vstep As Double
        Dim vSign As Integer
        Dim Pe As Single
        Dim Pwheel As Single
        Dim LastDiff As Single
        Dim Diff As Single
        Dim nU As Single
        Dim Pdrag As Single
        Dim Grad As Single


        vstep = 5
        nU = fnU(v, Gear, False)
        Pdrag = FLD(Gear).Pdrag(nU)

        'Do not allow positive road gradients     
        Grad = MODdata.Vh.fGrad(s)


        Pwheel = fPwheel(t, v, a, Grad)
        Pe = Pwheel + fPlossGB(Pwheel, v, Gear, True) + fPlossDiff(Pwheel, v, True) + fPaG(v, a) + fPlossRt(v, Gear) + fPaux(t, nU) + fPaMotSimple(t, Gear, v, a)

        Diff = Math.Abs(Pdrag - Pe)

        If Diff > 0.0001 Then
            vSign = 1
        Else
            Return v
        End If

        LastDiff = Diff + 10 * 0.0001

        Do While Diff > 0.0001 'And Math.Abs(LastDiff - Diff) > eps

            If LastDiff < Diff Or v + vSign * vstep <= 0.0001 Then
                vSign *= -1
                vstep *= 0.5

                If vstep < 0.00001 Then Exit Do

            End If

            v += vSign * vstep

            If v0 Is Nothing Then
                a = 2 * (v - MODdata.Vh.V0(t)) / 1  'dt = 1[s]
            Else
                a = 2 * (v - v0) / 1  'dt = 1[s]
            End If

            nU = fnU(v, Gear, False)
            Pdrag = FLD(Gear).Pdrag(nU)

            LastDiff = Diff

            Pwheel = fPwheel(t, v, a, Grad)
            Pe = Pwheel + fPlossGB(Pwheel, v, Gear, True) + fPlossDiff(Pwheel, v, True) + fPaG(v, a) + fPlossRt(v, Gear) + fPaux(t, nU) + fPaMotSimple(t, Gear, v, a)

            Diff = Math.Abs(Pdrag - Pe)

        Loop

        Return v

    End Function

    Private Function fCoastingSpeed(ByVal t As Integer, ByVal s As Double, ByVal Gear As Integer, ByVal dt As Integer) As Single
        Dim a As Single
        Dim v As Single
        Dim vtemp As Single
        Dim t0 As Integer
        Dim v0 As Single
        Dim v0p As Single

        v0 = MODdata.Vh.V0(t)
        v = MODdata.Vh.V(t)
        a = MODdata.Vh.a(t)

        If t + dt > MODdata.tDim - 1 Then
            dt = MODdata.tDim - 1 - t
        End If

        For t0 = t To t + dt
            vtemp = fCoastingSpeed(t0, s, Gear, v, a, v0)

            If 2 * vtemp - v0 < 0 Then vtemp = v0 / 2

            v0p = 2 * vtemp - v0
            a = v0p - v0

            v = (MODdata.Vh.V0(t0 + 2) + v0p) / 2
            a = MODdata.Vh.V0(t0 + 2) - v0p

            v0 = v0p

        Next

        Return vtemp

    End Function

#Region "Gear Shift Methods"

    Private Function fFastGearCalc(ByVal V As Single, ByVal Pe As Single) As Integer
        Dim Gear As Integer
        Dim Tq As Single
        Dim nU As Single
        Dim nUup As Single
        Dim nUdown As Single

        For Gear = GBX.GearCount To 1 Step -1

            nU = CSng(Vact * 60.0 * GBX.Igetr(0) * GBX.Igetr(Gear) / (2 * VEH.rdyn * Math.PI / 1000))

            'Current torque demand with previous gear
            Tq = Pe * 1000 / (nU * 2 * Math.PI / 60)

            'Up/Downshift rpms
            nUup = GBX.Shiftpolygons(Gear).fGSnUup(Tq)
            nUdown = GBX.Shiftpolygons(Gear).fGSnUdown(Tq)

            If nU > nUdown Then Return Gear

        Next

        Return 1

    End Function

    Private Function fStartGear(ByVal t As Integer, ByVal Grad As Single) As Integer
        Dim Gear As Integer
        Dim MsgSrc As String
        Dim nU As Single
        Dim nUup As Single
        Dim nUdown As Single
        Dim Tq As Single
        Dim Pe As Single
        Dim MdMax As Single
        Dim Pmax As Single

        MsgSrc = "StartGear"

        If GBX.TCon Then Return 1

        If t = 0 AndAlso VehState0 <> tVehState.Stopped Then

            'Calculate gear when cycle starts with speed > 0
            For Gear = GBX.GearCount To 1 Step -1

                'rpm
                nU = fnU(Vact, Gear, Clutch = tEngClutch.Slipping)

                'full load
                Pmax = FLD(Gear).Pfull(nU)

                'power demand - cut at full load / drag so that fGSnnUp and fGSnnDown don't extrapolate
                Pe = Math.Min(fPeGearMod(Gear, t, Grad), Pmax)
                Pe = Math.Max(Pe, FLD(Gear).Pdrag(nU))

                'torque demand
                Tq = Pe * 1000 / (nU * 2 * Math.PI / 60)

                'Up/Downshift rpms
                nUup = GBX.Shiftpolygons(Gear).fGSnUup(Tq)
                nUdown = GBX.Shiftpolygons(Gear).fGSnUdown(Tq)

                'Max torque
                MdMax = Pmax * 1000 / (nU * 2 * Math.PI / 60)

                'Find highest gear with rpm below Upshift-rpm and with enough torque reserve 
                If nU < nUup And nU > nUdown And 1 - Tq / MdMax >= GBX.gs_TorqueResv / 100 Then
                    Exit For
                ElseIf nU > nUup And Gear < GBX.GearCount Then
                    Return Gear + 1
                End If

            Next

        Else

            'Calculate Start Gear 
            For Gear = GBX.GearCount To 1 Step -1

                'rpm at StartSpeed  [m/s]
                nU = GBX.gs_StartSpeed * 60.0 * GBX.Igetr(0) * GBX.Igetr(Gear) / (2 * VEH.rdyn * Math.PI / 1000)

                'full load
                Pmax = FLD(Gear).Pfull(nU)

                'Max torque
                MdMax = Pmax * 1000 / (nU * 2 * Math.PI / 60)

                'power demand
                Pe = Math.Min(fPeGearMod(Gear, t, GBX.gs_StartSpeed, GBX.gs_StartAcc, Grad), Pmax)
                Pe = Math.Max(Pe, FLD(Gear).Pdrag(nU))

                'torque demand
                Tq = Pe * 1000 / (nU * 2 * Math.PI / 60)

                'Up/Downshift rpms
                nUup = GBX.Shiftpolygons(Gear).fGSnUup(Tq)
                nUdown = GBX.Shiftpolygons(Gear).fGSnUdown(Tq)

                If nU > nUdown And nU >= ENG.Nidle And (1 - Tq / MdMax >= GBX.gs_TorqueResvStart / 100 Or Tq < 0) Then Exit For

            Next

        End If

        Return Gear


    End Function

    Private Function fGearTC(ByVal t As Int16, ByVal Grad As Single, ByVal Tq As Single) As Integer
        Dim LastGear As Int16
        Dim tx As Int16
        Dim nU As Single
        Dim nUup As Single
        Dim nUdown As Single
        Dim Pe As Single
        Dim nUnext As Single
        Dim OutOfRpmRange As Boolean
        Dim PlusGearLockUp As Boolean
        Dim MinusGearTC As Boolean
        Dim iRatio As Single
        Dim n As Single

        'First time step (for vehicles with TC always the first gear is used)
        If t = 0 Then Return fStartGear(0, Grad)

        If MODdata.VehState(t - 1) = tVehState.Stopped Then Return 1

        'Previous Gear
        tx = 1
        LastGear = 0
        Do While LastGear = 0 And t - tx > -1
            LastGear = MODdata.Gear(t - tx)
            tx += 1
        Loop

        'If idling (but no vehicle stop...?) then
        If LastGear = 0 Then Return 1

        If LastGear < GBX.GearCount Then
            PlusGearLockUp = Not GBX.IsTCgear(LastGear + 1)
        Else
            PlusGearLockUp = False
        End If

        If LastGear > 1 Then
            MinusGearTC = GBX.IsTCgear(LastGear - 1)
        Else
            MinusGearTC = False
        End If

        '2C-to-1C
        If MinusGearTC And GBX.IsTCgear(LastGear) Then
            If fnUout(Vact, LastGear) <= ENG.Nidle Then
                Return LastGear - 1
            End If
        End If

        If LastGear < GBX.GearCount AndAlso PlusGearLockUp Then
            nUnext = Vact * 60.0 * GBX.Igetr(0) * GBX.Igetr(LastGear + 1) / (2 * VEH.rdyn * Math.PI / 1000)
        Else
            nUnext = 0
        End If

        'nU
        If GBX.IsTCgear(LastGear) Then
            n = MODdata.TCnu(t - 1)
            nU = (Vact * 60.0 * GBX.Igetr(0) * GBX.Igetr(LastGear) / (2 * VEH.rdyn * Math.PI / 1000)) / n
        Else
            nU = Vact * 60.0 * GBX.Igetr(0) * GBX.Igetr(LastGear) / (2 * VEH.rdyn * Math.PI / 1000)
            OutOfRpmRange = (nU >= 1.2 * (ENG.Nrated - ENG.Nidle) + ENG.Nidle) Or nU < ENG.Nidle
            'No gear change 3s after last one -except rpm out of range
            If Not OutOfRpmRange AndAlso t - LastGearChange <= GBX.gs_ShiftTime And t > GBX.gs_ShiftTime - 1 Then Return LastGear
        End If

        Pe = Tq * (nU * 2 * Math.PI / 60) / 1000

        'Up/Downshift rpms
        nUup = GBX.Shiftpolygons(LastGear).fGSnUup(Tq)
        nUdown = GBX.Shiftpolygons(LastGear).fGSnUdown(Tq)

        'Upshift
        If PlusGearLockUp Then
            If nUnext > nUup AndAlso Pe <= FLD(LastGear + 1).Pfull(nUnext) Then
                Return LastGear + 1
            End If
        Else
            '1C-to-2C
            If LastGear < GBX.GearCount Then

                iRatio = GBX.Igetr(LastGear + 1) / GBX.Igetr(LastGear)

                If fnUout(Vact, LastGear + 1) > Math.Min(900, iRatio * (FLD(LastGear).N80h - 150)) AndAlso FLD(LastGear + 1).Pfull(nU * iRatio) > 0.7 * FLD(LastGear).Pfull(nU) Then
                    Return LastGear + 1
                End If
            End If
        End If


        'Downshift
        If MinusGearTC Then
            If nU < ENG.Nidle Then
                Return LastGear - 1
            End If
        Else
            If nU < nUdown Then
                Return LastGear - 1
            End If
        End If



        Return LastGear

    End Function

    Private Function fGearVECTO(ByVal t As Integer, ByVal Grad As Single) As Integer
        Dim nU As Single
        Dim nnUp As Single
        Dim nnDown As Single
        Dim Tq As Single
        Dim Pe As Single
        Dim LastGear As Int16
        Dim Gear As Int16
        Dim MdMax As Single
        Dim LastPeNorm As Single

        Dim tx As Int16
        Dim OutOfRpmRange As Boolean

        'First time step OR first time step after stand still
        If t = 0 OrElse MODdata.VehState(t - 1) = tVehState.Stopped Then Return fStartGear(t, Grad)


        '********* Gear Shift Polygon Model ********* 

        'Previous normalized engine power
        LastPeNorm = MODdata.Pe(t - 1)

        'Previous Gear
        tx = 1
        LastGear = 0
        Do While LastGear = 0 And t - tx > -1
            LastGear = MODdata.Gear(t - tx)
            tx += 1
        Loop

        'First time step after stand still
        If LastGear = 0 Then Return fStartGear(t, Grad)

        nU = CSng(Vact * 60.0 * GBX.Igetr(0) * GBX.Igetr(LastGear) / (2 * VEH.rdyn * Math.PI / 1000))

        OutOfRpmRange = ((nU - ENG.Nidle) / (ENG.Nrated - ENG.Nidle) >= 1.2 Or nU < ENG.Nidle)

        'No gear change 3s after last one -except rpm out of range
        If Not OutOfRpmRange AndAlso t - LastGearChange <= GBX.gs_ShiftTime And t > GBX.gs_ShiftTime - 1 Then Return LastGear

        'During start (clutch slipping) no gear shift
        If LastClutch = tEngClutch.Slipping And VehState0 = tVehState.Acc Then Return LastGear

        ''Search for last Gear-change
        'itgangw = 0
        'For i = t - 1 To 1 Step -1
        '    If MODdata.Gear(i) <> MODdata.Gear(i - 1) Then
        '        itgangw = i
        '        Exit For
        '    End If
        'Next

        ''Maximum permissible Gear-shifts every 3 seconds:
        'If t - itgangw <= 3 And t > 2 Then
        '    Return LastGear    '<<< no further checks!!!
        'End If

        'Current rpm with previous gear
        nU = fnU(Vact, LastGear, Clutch = tEngClutch.Slipping)

        'Current power demand with previous gear
        Pe = Math.Min(fPeGearMod(LastGear, t, Grad), FLD(LastGear).Pfull(nU))
        Pe = Math.Max(Pe, FLD(LastGear).Pdrag(nU))

        'Current torque demand with previous gear
        Tq = Pe * 1000 / (nU * 2 * Math.PI / 60)
        MdMax = FLD(LastGear).Pfull(nU, LastPeNorm) * 1000 / (nU * 2 * Math.PI / 60)

        'Up/Downshift rpms
        nnUp = GBX.Shiftpolygons(LastGear).fGSnUup(Tq)
        nnDown = GBX.Shiftpolygons(LastGear).fGSnUdown(Tq)

        'Compare rpm with Up/Downshift rpms 
        If nU <= nnDown And LastGear > 1 Then

            'Shift DOWN
            Gear = LastGear - 1

            'Skip Gears
            If GBX.gs_SkipGears AndAlso Gear > 1 Then

                'Calculate Shift-rpm for lower gear
                nU = fnU(Vact, Gear - 1, False)

                'Continue only if rpm (for lower gear) is above idling
                If nU >= ENG.Nidle Then
                    Pe = Math.Min(fPeGearMod(Gear - 1, t, Grad), FLD(Gear - 1).Pfull(nU))
                    Pe = Math.Max(Pe, FLD(Gear - 1).Pdrag(nU))
                    Tq = Pe * 1000 / (nU * 2 * Math.PI / 60)
                    nnUp = GBX.Shiftpolygons(Gear - 1).fGSnUup(Tq)
                    nnDown = GBX.Shiftpolygons(Gear - 1).fGSnUdown(Tq)

                    'Shift down as long as Gear > 1 and rpm is below UpShift-rpm
                    Do While Gear > 1 AndAlso nU < nnUp

                        'Shift DOWN
                        Gear -= 1

                        'Continue only if Gear > 1
                        If Gear = 1 Then Exit Do

                        'Calculate Shift-rpm for lower gear
                        nU = fnU(Vact, Gear - 1, False)

                        'Continue only if rpm (for lower gear) is above idling
                        If nU < ENG.Nidle Then Exit Do

                        Pe = Math.Min(fPeGearMod(Gear - 1, t, Grad), FLD(Gear - 1).Pfull(nU))
                        Pe = Math.Max(Pe, FLD(Gear - 1).Pdrag(nU))
                        Tq = Pe * 1000 / (nU * 2 * Math.PI / 60)
                        nnUp = GBX.Shiftpolygons(Gear - 1).fGSnUup(Tq)
                        nnDown = GBX.Shiftpolygons(Gear - 1).fGSnUdown(Tq)

                    Loop

                End If

            End If

        ElseIf LastGear < GBX.GearCount And nU > nnUp Then

            'Shift UP
            Gear = LastGear + 1

            'Skip Gears
            If GBX.gs_SkipGears AndAlso Gear < GBX.GearCount Then

                If GBX.TracIntrSi > 0.001 Then
                    LastPeNorm = fTracIntPower(t, Gear)
                End If

                'Calculate Shift-rpm for higher gear
                nU = fnU(Vact, Gear + 1, False)

                Pe = Math.Min(fPeGearMod(Gear + 1, t, Grad), FLD(Gear + 1).Pfull(nU))
                Pe = Math.Max(Pe, FLD(Gear + 1).Pdrag(nU))
                Tq = Pe * 1000 / (nU * 2 * Math.PI / 60)
                nnUp = GBX.Shiftpolygons(Gear + 1).fGSnUup(Tq)
                nnDown = GBX.Shiftpolygons(Gear + 1).fGSnUdown(Tq)

                'Max Torque
                MdMax = FLD(Gear + 1).Pfull(nU, LastPeNorm) * 1000 / (nU * 2 * Math.PI / 60)

                'Shift up as long as Torque reserve is okay and Gear < Max-Gear and rpm is above DownShift-rpm
                Do While Gear < GBX.GearCount AndAlso 1 - Tq / MdMax >= GBX.gs_TorqueResv / 100 AndAlso nU > nnDown '+ 0.1 * (nnUp - nnDown)

                    'Shift UP
                    Gear += 1

                    'Continue only if Gear < Max-Gear
                    If Gear = GBX.GearCount Then Exit Do

                    'Calculate Shift-rpm for higher gear
                    nU = fnU(Vact, Gear + 1, False)

                    'Continue only if rpm (for higher gear) is below rated rpm
                    If nU > ENG.Nrated Then Exit Do

                    Pe = Math.Min(fPeGearMod(Gear + 1, t, Grad), FLD(Gear + 1).Pfull(nU))
                    Pe = Math.Max(Pe, FLD(Gear + 1).Pdrag(nU))
                    Tq = Pe * 1000 / (nU * 2 * Math.PI / 60)
                    nnUp = GBX.Shiftpolygons(Gear + 1).fGSnUup(Tq)
                    nnDown = GBX.Shiftpolygons(Gear + 1).fGSnUdown(Tq)

                    'Max Torque
                    MdMax = FLD(Gear + 1).Pfull(nU, LastPeNorm) * 1000 / (nU * 2 * Math.PI / 60)

                Loop

            End If

        Else

            'Keep last gear
            Gear = LastGear

            'Shift UP inside shift polygons
            If GBX.gs_ShiftInside And LastGear < GBX.GearCount Then

                'Calculate Shift-rpm for higher gear
                nU = fnU(Vact, Gear + 1, False)

                'Continue only if rpm (for higher gear) is below rated rpm
                If nU <= ENG.Nrated Then
                    Pe = Math.Min(fPeGearMod(Gear + 1, t, Grad), FLD(Gear + 1).Pfull(nU))
                    Pe = Math.Max(Pe, FLD(Gear + 1).Pdrag(nU))
                    Tq = Pe * 1000 / (nU * 2 * Math.PI / 60)
                    nnUp = GBX.Shiftpolygons(Gear + 1).fGSnUup(Tq)
                    nnDown = GBX.Shiftpolygons(Gear + 1).fGSnUdown(Tq)

                    'Max Torque
                    MdMax = FLD(Gear + 1).Pfull(nU, LastPeNorm) * 1000 / (nU * 2 * Math.PI / 60)

                    'Shift up as long as Torque reserve is okay and Gear < Max-Gear and rpm is above DownShift-rpm
                    Do While Gear < GBX.GearCount AndAlso 1 - Tq / MdMax >= GBX.gs_TorqueResv / 100 AndAlso nU > nnDown '+ 0.1 * (nnUp - nnDown)

                        'Shift UP
                        Gear += 1

                        'Continue only if Gear < Max-Gear
                        If Gear = GBX.GearCount Then Exit Do

                        'Calculate Shift-rpm for higher gear
                        nU = fnU(Vact, Gear + 1, False)

                        'Continue only if rpm (for higher gear) is below rated rpm
                        If nU > ENG.Nrated Then Exit Do

                        Pe = Math.Min(fPeGearMod(Gear + 1, t, Grad), FLD(Gear + 1).Pfull(nU))
                        Pe = Math.Max(Pe, FLD(Gear + 1).Pdrag(nU))
                        Tq = Pe * 1000 / (nU * 2 * Math.PI / 60)
                        nnUp = GBX.Shiftpolygons(Gear + 1).fGSnUup(Tq)
                        nnDown = GBX.Shiftpolygons(Gear + 1).fGSnUdown(Tq)

                        'Max Torque
                        MdMax = FLD(Gear + 1).Pfull(nU, LastPeNorm) * 1000 / (nU * 2 * Math.PI / 60)

                    Loop


                End If

            End If

        End If

lb10:
        '*** Error-Msg-Check ***
        'Current rpm 
        nU = fnU(Vact, Gear, Clutch = tEngClutch.Slipping)
        'Current power demand
        Pe = Math.Min(fPeGearMod(Gear, t, Grad), FLD(Gear).Pfull(nU))
        Pe = Math.Max(Pe, FLD(Gear).Pdrag(nU))
        'Current torque demand
        Tq = Pe * 1000 / (nU * 2 * Math.PI / 60)

        'If GearCorrection is OFF then return here
        Return Gear

    End Function

    Private Function fGearByU(ByVal nU As Single, ByVal V As Single) As Integer
        Dim Dif As Single
        Dim DifMin As Single
        Dim g As Int16
        Dim g0 As Integer
        DifMin = 9999
        For g = 1 To GBX.GearCount
            Dif = Math.Abs(GBX.Igetr(g) - nU * (2 * VEH.rdyn * Math.PI) / (1000 * V * 60.0 * GBX.Igetr(0)))
            If Dif <= DifMin Then
                g0 = g
                DifMin = Dif
            End If
        Next
        Return g0
    End Function

    'Function calculating the Power easily for Gear-shift-model
    Private Function fPeGearModvD(ByVal t As Integer, ByVal Grad As Single) As Single
        Return fPwheel(t, Grad)
    End Function

    Private Function fPeGearMod(ByVal Gear As Integer, ByVal t As Integer, ByVal V As Single, ByVal a As Single, ByVal Grad As Single) As Single
        Dim PaM As Single
        Dim nU As Single
        Dim Pwheel As Single

        Pwheel = fPwheel(t, V, a, Grad)

        nU = fnU(V, Gear, False)

        If t = 0 Then
            PaM = 0
        Else
            PaM = fPaMot(nU, MODdata.nU(t - 1))
        End If


        If Clutch = tEngClutch.Closed Then
            Return (Pwheel + fPlossGB(Pwheel, V, Gear, True) + fPlossDiff(Pwheel, V, True) + fPaG(V, a) + fPaux(t, nU) + PaM)
        Else    'Clutch = tEngClutch.Slipping
            Return ((Pwheel + fPlossGB(Pwheel, V, Gear, True) + fPlossDiff(Pwheel, V, True) + fPaG(V, a)) / ClutchEta + fPaux(t, nU) + PaM)
        End If

    End Function

    Private Function fPeGearMod(ByVal Gear As Integer, ByVal t As Integer, ByVal Grad As Single) As Single
        Return fPeGearMod(Gear, t, MODdata.Vh.V(t), MODdata.Vh.a(t), Grad)
    End Function


#End Region

#Region "Engine Speed Calculation"

    Private Function fnn(ByVal V As Single, ByVal Gear As Integer, ByVal ClutchSlip As Boolean) As Single
        Return (fnU(V, Gear, ClutchSlip) - ENG.Nidle) / (ENG.Nrated - ENG.Nidle)
    End Function

    Private Function fnU(ByVal V As Single, ByVal Gear As Integer, ByVal ClutchSlip As Boolean) As Single
        Dim akn As Single
        Dim U As Single
        U = CSng(V * 60.0 * GBX.Igetr(0) * GBX.Igetr(Gear) / (2 * VEH.rdyn * Math.PI / 1000))
        If U < ENG.Nidle Then U = ENG.Nidle
        If ClutchSlip Then
            akn = ClutchNorm / ((ENG.Nidle + ClutchNorm * (ENG.Nrated - ENG.Nidle)) / ENG.Nrated)
            U = (akn * U / ENG.Nrated) * (ENG.Nrated - ENG.Nidle) + ENG.Nidle
        End If
        Return U
    End Function

    Private Function fnUout(ByVal V As Single, ByVal Gear As Integer) As Single
        Return V * 60.0 * GBX.Igetr(0) * GBX.Igetr(Gear) / (2 * VEH.rdyn * Math.PI / 1000)
    End Function

#End Region

#Region "Power Calculation"

    '--------------Power before Diff = At Wheel -------------
    Private Function fPwheel(ByVal t As Integer, ByVal Grad As Single) As Single
        Return fPr(MODdata.Vh.V(t), Grad) + fPair(MODdata.Vh.V(t), t) + fPaFZ(MODdata.Vh.V(t), MODdata.Vh.a(t)) + fPs(MODdata.Vh.V(t), Grad)
    End Function

    Private Function fPwheel(ByVal t As Integer, ByVal v As Single, ByVal a As Single, ByVal Grad As Single) As Single
        Return fPr(v, Grad) + fPair(v, t) + fPaFZ(v, a) + fPs(v, Grad)
    End Function

    '----------------Rolling-resistance----------------
    Private Function fPr(ByVal v As Single, ByVal Grad As Single) As Single
        Return CSng(Math.Cos(Math.Atan(Grad * 0.01)) * (VEH.Loading + VEH.Mass + VEH.MassExtra) * 9.81 * VEH.Fr0 * v * 0.001)
    End Function

    '----------------Drag-resistance----------------
    Private Function fPair(ByVal v As Single, ByVal t As Integer) As Single
        Dim vair As Single
        Dim Cd As Single

        Select Case VEH.CdMode

            Case tCdMode.ConstCd0
                vair = v
                Cd = VEH.Cd

            Case tCdMode.CdOfV
                vair = v
                Cd = VEH.Cd(v)

            Case Else   'tCdType.CdOfBeta
                vair = MODdata.Vh.VairVres(t)
                Cd = VEH.Cd(Math.Abs(MODdata.Vh.VairBeta(t)))

        End Select

        Return CSng((Cd * VEH.CrossSecArea * Cfg.AirDensity / 2 * ((vair) ^ 2)) * v * 0.001)

    End Function

    '--------Vehicle Acceleration-capability(Beschleunigungsleistung) --------
    Private Function fPaFZ(ByVal v As Single, ByVal a As Single) As Single
        Return CSng(((VEH.Mass + VEH.MassExtra + VEH.m_red + VEH.Loading) * a * v) * 0.001)
    End Function

    Private Function fPaMotSimple(ByVal t As Integer, ByVal Gear As Integer, ByVal v As Single, ByVal a As Single) As Single
        Return ((ENG.I_mot * (GBX.Igetr(0) * GBX.Igetr(Gear) / (VEH.rdyn / 1000)) ^ 2) * a * v) * 0.001
    End Function

    Public Function fPaMot(ByVal nU As Single, ByVal nUBefore As Single) As Single
        If GBX.TCon Then
            Return ((ENG.I_mot + GBX.TCinertia) * (nU - nUBefore) * 0.01096 * ((nU + nUBefore) / 2)) * 0.001
        Else
            Return (ENG.I_mot * (nU - nUBefore) * 0.01096 * ((nU + nUBefore) / 2)) * 0.001
        End If
    End Function

    '----------------Slope resistance ----------------
    Private Function fPs(ByVal v As Single, ByVal Grad As Single) As Single
        Return CSng(((VEH.Loading + VEH.Mass + VEH.MassExtra) * 9.81 * Math.Sin(Math.Atan(Grad * 0.01)) * v) * 0.001)
    End Function

    '----------------Auxillaries(Nebenaggregate) ----------------
    Public Function fPaux(ByVal t As Integer, ByVal nU As Single) As Single
        Return CSng(MODdata.Vh.Padd(t) + VEC.PauxSum(t, nU))
    End Function

    '-------------------Transmission(Getriebe)-------------------
    Private Function fPlossGB(ByVal PvD As Single, ByVal V As Single, ByVal Gear As Integer, ByVal TrLossApprox As Boolean) As Single
        Dim Pdiff As Single
        Dim Prt As Single
        Dim P As Single
        Dim nU As Single

        If Gear = 0 Then Return 0

        nU = (60 * V) / (2 * VEH.rdyn * Math.PI / 1000) * GBX.Igetr(0) * GBX.Igetr(Gear)

        'Pdiff
        Pdiff = fPlossDiff(PvD, V, TrLossApprox)

        If VEH.RtType = tRtType.Secondary Then
            Prt = fPlossRt(V, Gear)
        Else
            Prt = 0
        End If

        '***Differential
        '   Power before Transmission; after Differential and Retarder (if Type=Secondary)
        P = PvD + Pdiff + Prt

        Return Math.Max(GBX.IntpolPeLoss(Gear, nU, P, TrLossApprox), 0)


    End Function

    Private Function fPlossDiff(ByVal PvD As Single, ByVal V As Single, ByVal TrLossApprox As Boolean) As Single

        '***Differential
        '   Power before Differential
        Return Math.Max(GBX.IntpolPeLoss(0, (60 * V) / (2 * VEH.rdyn * Math.PI / 1000) * GBX.Igetr(0), PvD, TrLossApprox), 0)

    End Function

    Private Function fPlossGBfwd(ByVal PeICE As Single, ByVal V As Single, ByVal Gear As Integer, ByVal TrLossApprox As Boolean) As Single
        Dim nU As Single
        Dim Prt As Single

        If Gear = 0 Then Return 0

        If VEH.RtType = tRtType.Primary Then
            Prt = fPlossRt(V, Gear)
        Else
            Prt = 0
        End If

        nU = (60 * V) / (2 * VEH.rdyn * Math.PI / 1000) * GBX.Igetr(0) * GBX.Igetr(Gear)

        Return Math.Max(GBX.IntpolPeLossFwd(Gear, nU, PeICE + Prt, TrLossApprox), 0)

    End Function

    Private Function fPlossDiffFwd(ByVal PeIn As Single, ByVal V As Single, ByVal TrLossApprox As Boolean) As Single
        Dim nU As Single

        nU = (60 * V) / (2 * VEH.rdyn * Math.PI / 1000) * GBX.Igetr(0)

        Return Math.Max(GBX.IntpolPeLossFwd(0, nU, PeIn, TrLossApprox), 0)

    End Function

    Private Function fPlossRt(ByVal Vist As Single, ByVal Gear As Integer) As Single
        Return VEH.RtPeLoss(Vist, Gear)
    End Function

    '----------------Gearbox inertia ----------------
    Private Function fPaG(ByVal V As Single, ByVal a As Single) As Single
        Dim Mred As Single
        Mred = GBX.GbxInertia * (GBX.Igetr(0) / (VEH.rdyn / 1000)) ^ 2
        Return (Mred * a * V) * 0.001
    End Function

#End Region


End Class

