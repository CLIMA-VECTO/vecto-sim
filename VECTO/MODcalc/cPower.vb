Imports System.Collections.Generic

Public Class cPower
 
    Private Kuppln_norm As Single    '... Einschleifdrehzahl Kupplung (Modell nach Leistg/Schinagl)
    Private KupplEta As Single       '... Wirkungsgrad schleifende Kupplung

    Private bHEVinit As Boolean
    Private bHorEVinit As Boolean

    'Settings
    Private Gvorg As Boolean
    Private Nvorg As Boolean
    Private Aaufi As Single
    Private Baufi As Single
    Private Caufi As Single
    Private Aobi As Single
    Private Bobi As Single
    Private Cobi As Single

    'Per-second Data
    Private Clutch As tEngClutch
    Private VehState0 As tVehState
    Private EngState0 As tEngState
    Private Pplus As Boolean
    Private Pminus As Boolean
    Private GVmax As Single
    Private PvorD As Single
    Private Vist As Single
    Private aist As Single

    'Interruption of traction
    Private TracIntrI As Integer
    Private TracIntrIx As Integer
    Private TracIntrOn As Boolean
    Private TracIntrTurnOff As Boolean
    Private TracIntrGear As Integer

    Private LastGearChange As Integer
    Private LastClutch As tEngClutch

    Public Sub New()
        bHEVinit = False
        bHorEVinit = False
    End Sub


#Region "Schaltmodell Variablen"

    Private GangH() As Integer
    Private GangL() As Integer

    Private avh(20) As Single
    Private avl(20) As Single

#End Region

#Region "(H)EV Variablen"

    Private EMO As cEMO
    Private HEV As cHEVctrl

    Public PeEMot As List(Of Single)
    Public PiEMot As List(Of Single)
    Public PeBat As List(Of Single)
    Public PiBat As List(Of Single)
    Public SOC As List(Of Single)
    Public Ubat As List(Of Single)
    Public Ibat As List(Of Single)
    Public BAT As cBatModel
    Public TempBat As List(Of Single)
    Public HEVmode As List(Of tHEVparMode)

    Public SOCstart As Single

    Private PeBatMax As Single
    Private PeBatMin As Single

    Private ULok As Boolean = True

    Private PeUL As Single

    Private PeL As List(Of Single)
    Private PiL As List(Of Single)
    Private nnL As List(Of Single)
    Private WGL As List(Of Single)
    Private Ldim As Integer

    'Recuperation
    '   Project HERO - BMW Mini Hybrid
    '   Standard Mini One D Wheelbase 2467 mm
    Private lSHv As Single = 1.2335    'Annahme 50/50 (Laut Internet Gewichtsverteilung vom MINI E)
    Private lSHh As Single = 1.2335
    'Specification of Center-of-gravity height (approximation)
    '   from http://www.colliseum.net/wiki/Schwerpunkth% C3% B6he
    '   X = 0.2 * m / 1000 = h/R
    '       with R = 2467 [m], and m = 1335 [kg]
    Private hSH As Single = 0.659
    Private RekupVorne As Boolean = True
    Private muReifStr As Single = 1
    Private RekupS As Single = 1.25
    Private RekupVo As Single = 10 / 3.6    'Geschw. bei der Übergang zu rein mechanischen Bremsen beginnt
    Private RekupVu As Single = 6 / 3.6     'Geschw. bei der Übergang zu rein mechanischen Bremsen endet

#End Region

    Public Sub Init()

        ReDim GangH(40000)
        ReDim GangL(40000)
        bHEVinit = False
        bHorEVinit = False

        If GEN.ModeHorEV Then

            PeEMot = New List(Of Single)
            PiEMot = New List(Of Single)
            PeBat = New List(Of Single)
            PiBat = New List(Of Single)
            Ubat = New List(Of Single)
            Ibat = New List(Of Single)
            SOC = New List(Of Single)
            TempBat = New List(Of Single)
            BAT = New cBatModel(Me)
            bHorEVinit = True

            If Not GEN.VehMode = tVehMode.EV Then
                HEV = New cHEVctrl
                EMO = New cEMO
                bHEVinit = True
            End If

        End If

    End Sub

    Public Sub CleanUp()

        ReDim GangH(0)
        ReDim GangL(0)

        If bHorEVinit Then

            BAT.CleanUp()
            PeEMot = Nothing
            PiEMot = Nothing
            PeBat = Nothing
            PiBat = Nothing
            Ubat = Nothing
            Ibat = Nothing
            SOC = Nothing
            TempBat = Nothing
            BAT = Nothing
            SOCstart = 0
            bHorEVinit = False

            If bHEVinit Then
                HEV.CleanUp()
                HEV = Nothing
                EMO.CleanUp()
                EMO = Nothing
                bHEVinit = False
            End If

        End If
    End Sub

#Region "(H)EV Support-Methoden"

    Public Function EVinit() As Boolean
        Return BATread()
    End Function

    Public Function HEVinit() As Boolean

        'BAT
        If Not BATread() Then Return False

        'STE
        HEV.STEfile = GEN.STEnam
        If Not HEV.readSTE() Then Return False

        'EMO
        EMO.FilePath = GEN.Emospez
        If Not EMO.ReadFile Then Return False

        Return True

    End Function

    'Read Bat
    Private Function BATread() As Boolean
        BAT.FilePath = GEN.Batfile
        SOCstart = GEN.SOCstart
        Return BAT.Bat_Init()
    End Function

    'Maximum effective EM-Power in driving depends on Overload and Battery-status
    Private Function fPeEMmax(ByVal nn As Single) As Single
        Dim PeFLD As Single
        Dim PeBAT As Single

        'Based: Full-load-curve
        PeFLD = FLD.Pfull(nn)

        'If Overload possible, upscale Overload(ÜL)-power
        If ULok Then PeFLD *= PeUL / VEH.Pnenn

        '=> PeFLD = maximum EM-Power by(nach) FLD and Overload(ÜL)

        'Calculate PeMax from PeBatMax
        PeBAT = 0.9 * fPefromPi(nn, PeBatMax)

        '=> PeBAT = maximum EM-power to Battery

        'Return the maximum Power allowed by the Battery
        Return Math.Min(PeFLD, PeBAT)

    End Function

    'Maximum effective EM charging power depending on Overload and Battery-state
    Private Function fPeEMmin(ByVal nn As Single) As Single
        Dim PeFLD As Single
        Dim PeBAT As Single

        'Base: Drag-curve
        PeFLD = FLD.Pdrag(nn)

        'If Overload possible, upscale to Overload(ÜL)-power
        If ULok Then PeFLD *= PeUL / VEH.Pnenn

        '=> PeFLD = maximum EM-Power by(nach) FLD and Overload(ÜL)

        'Calculate PeMax from PeBatMax
        PeBAT = 0.9 * fPefromPi(nn, PeBatMin)

        '=> PeBAT = maximum EM-power to Battery

        'Return the maximum Power allowed by the Battery
        Return Math.Max(PeFLD, PeBAT)



    End Function

    'Conversion of PeBat (=PiEM) to PeEM
    Private Function fPefromPi(ByVal nn As Single, ByVal Pi As Single) As Single
        Dim wisum As Double = 0
        Dim sumo As Double = 0
        Dim ab As Double = 0
        Dim Eta As Single
        Dim i As Int32
        Dim Pinorm As Single

        Pinorm = Pi / VEH.Pnenn

        For i = 0 To Ldim
            'When sign of x an y is not-equal to the sign of xA(i) and yA(i) respectively, then skip Row i
            If nn * nnL(i) < 0 Or Pi * PiL(i) < 0 Then Continue For
            ab = (nn - nnL(i)) ^ 2 + (Pinorm - PiL(i)) ^ 2
            If ab = 0 Then
                Eta = WGL(i)
                GoTo lb10
            Else
                sumo = sumo + (WGL(i)) / ab
                wisum = wisum + 1 / ab
            End If
        Next
        If wisum = 0 Then
            Eta = 0
        Else
            Eta = sumo / wisum
        End If
lb10:
        If Pi > 0 Then
            Return Pi * Eta
        Else
            Return Pi / Eta
        End If

    End Function

    'Conversion of PeEM to PeBat (=piEM)
    Private Function fPifromPe(ByVal nn As Single, ByVal Pe As Single) As Single
        Dim wisum As Double = 0
        Dim sumo As Double = 0
        Dim ab As Double = 0
        Dim Eta As Single
        Dim i As Int32
        Dim Penorm As Single

        Penorm = Pe / VEH.Pnenn

        For i = 0 To Ldim
            'When sign of x and y is not-equal to the sign of xA(i) and yA(i) respectively, then skipp Row i
            If nn * nnL(i) < 0 Or Pe * PeL(i) < 0 Then Continue For
            ab = (nn - nnL(i)) ^ 2 + (Penorm - PeL(i)) ^ 2
            If ab = 0 Then
                Eta = WGL(i)
                GoTo lb10
            Else
                sumo = sumo + (WGL(i)) / ab
                wisum = wisum + 1 / ab
            End If
        Next
        If wisum = 0 Then
            Eta = 0
        Else
            Eta = sumo / wisum
        End If
lb10:
        If Pe > 0 Then
            Return Pe / Eta
        Else
            Return Pe * Eta
        End If

    End Function

    'Maximum Recuparation-power
    Private Function fPrekupMax() As Single
        Dim Fz As Single
        Dim Fx As Single
        Dim Prekup As Single

        'If speed is already under ceiling then return Zero
        If Vist < RekupVu Then Return 0

        'Wheel contact
        If RekupVorne Then
            Fz = ((VEH.Mass + VEH.MassExtra + VEH.Loading) * (9.81F * lSHh - aist * hSH)) / (lSHv + lSHh)
        Else
            Fz = ((VEH.Mass + VEH.MassExtra + VEH.Loading) * (9.81F * lSHv + aist * hSH)) / (lSHv + lSHh)
        End If

        'Sign "should" always be +
        Fz = Math.Max(0, Fz)

        'Longitudinal-force on the Tire
        Fx = Fz * muReifStr

        'Consider Safety-factor
        Fx /= RekupS

        'Power
        Prekup = -Fx * Vist / 1000

        'If below upper V-upper-limit, then scale down linearly
        If Vist <= RekupVo Then Prekup *= (Vist - RekupVu) / (RekupVo - RekupVu)

        Return Prekup

    End Function

    ''Reduce PeEM-Max until battery current is okay
    'Private Function RedPiToPbatMax(ByVal PeEM As Single) As Single

    '    Dim PiEM As Single

    '    PiEM = EMO.PiEM(PeEM)

    '    Do While PiEM > PeBatMax

    '        PeEM *= 0.99
    '        PiEM = EMO.PiEM(PeEM)

    '    Loop

    '    Return PiEM

    'End Function

    ''Reduce PeEM-Min until battery current is okay
    'Private Function RedPiToPbatMin(ByVal PeEM As Single) As Single

    '    Dim PiEM As Single

    '    PiEM = EMO.PiEM(PeEM)

    '    Do While PiEM < PeBatMin

    '        PeEM *= 0.99
    '        PiEM = EMO.PiEM(PeEM)

    '    Loop

    '    Return PiEM

    'End Function

#End Region

    Public Function PreRun() As Boolean
        Dim i As Integer
        Dim i0 As Integer
        Dim Vh As cVh
        Dim P As Single
        Dim Pmin As Single
        Dim Pmax As Single
        Dim PlossGB As Single
        Dim PlossDiff As Single
        Dim PlossRt As Single
        Dim PaMot As Single
        Dim PaGetr As Single
        Dim Pkup As Single
        Dim Paux As Single
        Dim Gear As Integer
        Dim nU As Single
        Dim nn As Single
        Dim vCoasting As Single
        Dim Positions As New List(Of Short)
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
        Dim aRollout As Single
        Dim Gears As New List(Of Integer)
        Dim vRollout As Single

        Dim MsgSrc As String

        MsgSrc = "Power/PreRun"

        'Check Input
        If GEN.a_lookahead >= 0 Then
            WorkerMsg(tMsgID.Err, "Lookahead deceleration invalid! Value must be below zero.", MsgSrc)
            Return False
        End If

        If GEN.OverSpeedOn And GEN.EcoRollOn Then
            WorkerMsg(tMsgID.Err, "Overrun and Ecoroll can't be enabled both at the same time!", MsgSrc)
            Return False
        End If

        '   Initialize
        Vh = MODdata.Vh
        Gvorg = DRI.Gvorg
        Nvorg = DRI.Nvorg

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

            Vist = Vh.V(i)
            aist = Vh.a(i)

            'Determine Driving-state  -------------------------
            Pplus = False
            Pminus = False

            If Vist < 0.0001 Then
                VehState0 = tVehState.Stopped
            Else
                If aist >= 0.01 Then
                    VehState0 = tVehState.Acc
                ElseIf aist < -0.01 Then
                    VehState0 = tVehState.Dec
                Else
                    VehState0 = tVehState.Cruise
                End If
            End If

            'Wheel-Power
            PvorD = fPvD(i)

            Select Case PvorD
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
            Else
                If Gvorg Then
                    Gear = Math.Min(Vh.GearVorg(i), VEH.ganganz)
                Else
                    Gear = fFastGearCalc(Vist, PvorD)
                End If
            End If

            'Engine Speed
            'ICE-inertia   
            If Nvorg Then
                nU = MODdata.nUvorg(i)
                PaMot = (VEH.I_mot * MODdata.dnUvorg(i) * 0.01096 * MODdata.nUvorg(i)) * 0.001
            Else
                nU = fnU(Vist, Gear, False)
                PaMot = ((VEH.I_mot * (VEH.AchsI * VEH.Igetr(Gear) / (0.5 * VEH.Dreifen)) ^ 2) * aist * Vist) * 0.001
            End If

            nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

            'Aux Demand
            Paux = fPaux(i, nU)

            'Engine Power (at Clutch)
            If Pplus Or Pminus Then

                PlossGB = fPlossGB(PvorD, Vist, Gear)
                PlossDiff = fPlossDiff(PvorD, Vist)
                PlossRt = fPlossRt(Vist, Gear)
                PaGetr = fPaG(Vist, aist)

                Pkup = PvorD + PlossGB + PlossDiff + PaGetr + PlossRt
                P = Pkup + Paux + PaMot

            Else

                Pkup = 0
                P = Paux + PaMot

            End If

            'Full load / motoring
            Pmax = FLD.Pfull(nn)
            Pmin = FLD.Pdrag(nn)

            If Vh.Vsoll(i) >= GEN.vMin / 3.6 Then

                If GEN.EcoRollOn Then

                    If Vh.Grad(i) < 0 Then

                        Vmax = MODdata.Vh.Vsoll(i) + GEN.OverSpeed / 3.6
                        Vmin = Math.Max(0, MODdata.Vh.Vsoll(i) - GEN.UnderSpeed / 3.6)
                        vRollout = fRolloutSpeed(i, 1)
                        aRollout = (2 * vRollout - Vh.V0(i)) - Vh.V0(i)

                        If vRollout <= Vmax Then

                            If 2 * vRollout - Vh.V0(i) > Vmax Then
                                Vh.SetSpeed0(i, Vmax)
                            ElseIf 2 * vRollout - Vh.V0(i) < Vmin Then
                                Vh.SetSpeed0(i, Vmin)
                            Else
                                Vh.SetSpeed(i, vRollout)
                                'Vh.SetAcc(i, aRollout)
                            End If

                            Positions(i) = 4
                            Vh.EcoRoll(i) = True

                        Else

                            If 2 * Vmax - Vh.V0(i) >= Vmax Then
                                Vh.SetSpeed0(i, Vmax)
                            Else
                                Vh.SetSpeed(i, Vmax)
                            End If

                            Positions(i) = 1

                        End If


                    End If

                Else

                    If P < Pmin Then

                        If GEN.OverSpeedOn Then

                            vCoasting = fCoastingSpeed(i, Gear, False)
                            Vmax = MODdata.Vh.Vsoll(i) + GEN.OverSpeed / 3.6

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


            Gears.Add(Gear)

        Loop Until i >= MODdata.tDim


        'Look Ahead & Limit Acc (Backward)

        'Mark Brake Positions
        For i = MODdata.tDim To 1 Step -1
            If Vh.V(i - 1) - Vh.V(i) > 0.0001 And Not Positions(i) = 4 Then Positions(i) = 1
        Next

        'Mark Look-Ahead Coasting Positions
        i = MODdata.tDim + 1
        Do
            i -= 1

            If Positions(i) = 1 Then
                vset2 = Vh.V(i)
                For j = i To 0 Step -1
                    If Positions(j) = 0 Or Positions(j) = 4 Then
                        vset1 = Vh.V(j)
                        Exit For
                    End If
                Next

                Tlookahead = CInt((vset2 - vset1) / GEN.a_lookahead)

                t = Math.Max(0, i - Tlookahead)
                LookAheadDone = False

                adec = GEN.aDesMin(Vist)
                If Vh.a(i) < adec Then Vh.SetAccBackw(i, adec)

                i0 = i

                Do
                    i -= 1
                    aist = Vh.a(i)
                    Vist = Vh.V(i)
                    adec = GEN.aDesMin(Vist)

                    If aist < adec Then
                        Vh.SetAccBackw(i, adec)
                        Positions(i) = 2
                    Else
                        'Coasting (Forward)
                        If GEN.LookAheadOn And Vh.Vsoll(i) >= GEN.vMinLA / 3.6 Then

                            For j = t To i0
                                Vist = Vh.V(j)
                                vCoasting = fCoastingSpeed(j, Gears(j), True)
                                aCoasting = (2 * vCoasting - Vh.V0(j)) - Vh.V0(j)
                                If vCoasting < Vist And aCoasting >= GEN.aDesMin(Vist) Then
                                    'If Vrollout < Vist Then
                                    Vh.SetSpeed(j, vCoasting)
                                    Positions(j) = 3
                                Else
                                    Exit For
                                End If
                            Next

                        End If

                        LookAheadDone = True
                    End If

                Loop Until LookAheadDone

                'Correct distance error
                'If t - 1 > 0 Then
                '    DistError = 0
                '    For i = t To i0
                '        DistError += (Vh.Vsoll(i) - Vh.V(i))
                '    Next

                '    DistError0 = DistError

                '    Do While DistError - Vh.V(t) > 0 'Math.Abs(DistError - Vh.V(t - 1)) < Math.Abs(DistError)
                '        Vh.DuplicatePreRun(t - 1)
                '        DistError -= Vh.V(t)
                '        MODdata.tDim += 1
                '    Loop



                '    'Vh.Weg(i0) += DistError0

                '    'For i = t To i0
                '    '    Vh.NoDistCorr(i) = True
                '    'Next

                '    'For i = i0 To 0 Step -1
                '    '    Vh.Weg(i) -= DistError0
                '    'Next

                'End If

                i = i0

            End If

        Loop Until i = 0

        Return True

    End Function

    Public Function Calc() As Boolean

        Dim i As Integer
        Dim M As Single
        Dim nn As Single
        Dim nU As Single
        Dim omega_p As Single
        Dim omega1 As Single
        Dim omega2 As Single
        Dim nnx As Single
        Dim nUx As Single
        Dim PminX As Single

        Dim jz As Integer

        'Start/Stop Control
        Dim StStAus As Boolean
        Dim StStTx As Single

        Dim Vh As cVh

        Dim Gear As Integer
        Dim GnachV As Boolean
        Dim PKWja As Boolean

        Dim P As Single
        Dim Pkup As Single
        Dim PaMot As Single
        Dim PaGetr As Single
        Dim Pmin As Single
        Dim Pmax As Single
        Dim Paux As Single
        Dim Pbrake As Single
        Dim PlossGB As Single
        Dim PlossDiff As Single
        Dim PlossRt As Single
        Dim GVset As Boolean
        Dim Vrollout As Single
        Dim SecSpeedRed As Integer
        Dim FirstSecItar As Boolean
        Dim ZgkrDt As Single

        Dim amax As Single

        Dim StdMode As Boolean
        Dim NotAdvMode As Boolean

        Dim LastPmax As Single

        Dim MsgSrc As String

        MsgSrc = "Power/Calc"

        StdMode = (PHEMmode = tPHEMmode.ModeSTANDARD)
        NotAdvMode = Not (PHEMmode = tPHEMmode.ModeADVANCE)

        'Abort if no speed given
        If Not DRI.Vvorg Then
            WorkerMsg(tMsgID.Err, "Driving cycle is not valid! Vehicle Speed required.", MsgSrc)
            Return False
        End If

        '   Initialize
        Vh = MODdata.Vh
        If Cfg.GnVorgab Then
            Gvorg = DRI.Gvorg
            Nvorg = DRI.Nvorg
            If StdMode Then
                If Gvorg Then WorkerMsg(tMsgID.Normal, "Using gears from driving cycle", MsgSrc)
                If Nvorg Then WorkerMsg(tMsgID.Normal, "Using rpm from driving cycle", MsgSrc)
            End If
        Else
            If (DRI.Gvorg Or DRI.Nvorg) And StdMode Then WorkerMsg(tMsgID.Warn, "Gears/rpm from driving cycle ignored.", MsgSrc)
            Gvorg = False
            Nvorg = False
        End If
        PKWja = GEN.PKWja
        StStAus = False
        StStTx = 0
        SecSpeedRed = 0

        If VEH.TracIntrSi < 0.001 Then
            TracIntrI = 0
        Else
            TracIntrI = CInt(Math.Max(1, Math.Round(VEH.TracIntrSi, 0, MidpointRounding.AwayFromZero)))
        End If
        TracIntrIx = 0
        TracIntrOn = False
        TracIntrTurnOff = False

        If GEN.izykwael = 4 Then
            Kuppln_norm = 0.05
            KupplEta = 0.4
        Else
            Kuppln_norm = 0.03
            KupplEta = 1
        End If

        LastClutch = tEngClutch.Opened

        'Gear-shifting points for NEDC / FTP
        Select Case GEN.izykwael
            Case 0  'Nefzja = True
                GnachV = True
                avh(1) = 16 / 3.6
                avh(2) = 34.0 / 3.6
                avh(3) = 51 / 3.6
                avh(4) = 69 / 3.6
                If (VEH.ganganz = 5) Then
                    avh(5) = 180 / 3.6
                Else
                    avh(5) = 99 / 3.6
                    avh(6) = 180 / 3.6
                End If
                avl(1) = -5.0 / 3.6
                avl(2) = 0.1 / 3.6
                avl(3) = 34.0 / 3.6
                avl(4) = 34.0 / 3.6
                avl(5) = 60.0 / 3.6
                avl(6) = 60.0 / 3.6
            Case 1  'Ftpja = True 
                GnachV = True
                avh(1) = 25 / 3.6
                avh(2) = 40 / 3.6
                avh(3) = 65 / 3.6
                avh(4) = 74 / 3.6
                avh(5) = 200 / 3.6
                avl(1) = 12.9 / 3.6
                avl(2) = 13.0 / 3.6
                avl(3) = 28 / 3.6
                avl(4) = 52 / 3.6
                avl(5) = 60 / 3.6
                avl(6) = 200 / 3.6
            Case Else
                GnachV = False
                'Initialize Gear-shifting parameters

                'Standard
                Aaufi = 0.3
                Baufi = 0.3
                Caufi = 0.4
                Aobi = 0.18
                Bobi = 0.28
                Cobi = 0.46

        End Select

        'Theoretical maximum speed [m/s] - set to Speed ​​at 1.2 x Nominal-Revolutions in top-Gear
        GVmax = 1.2 * VEH.nNenn * VEH.Dreifen * Math.PI / (VEH.AchsI * VEH.Igetr(VEH.ganganz) * 60)

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
            If NotAdvMode Then ProgBarCtrl.ProgJobInt = CInt(100 * jz / MODdata.tDim)


            '   Determine State
lbGschw:

            'Reset the second by second Errors
            MODdata.ModErrors.GeschRedReset()

            'Calculate Speed​/Acceleration -------------------
            'Now through DRI-class
            Vist = Vh.V(jz)
            aist = Vh.a(jz)

            'If Speed over Top theoretical Speed => Reduce
            If Vist > GVmax + 0.0001 And Not GVset Then
                Vh.SetSpeed0(jz, GVmax)
                GVset = True
                GoTo lbGschw
            End If

            'a_DesMax
            If GEN.DesMaxJa Then

                'Check if Acceleration is too high

                If jz = 0 Then
                    amax = GEN.aDesMax(Vist)
                Else
                    amax = GEN.aDesMax(Vh.V(jz - 1))
                End If

                If amax < 0.0001 Then
                    WorkerMsg(tMsgID.Err, "aDesMax(acc) invalid! v= " & Vist & ", aDesMax(acc) =" & amax, MsgSrc)
                    Return False
                End If

                If aist > amax + 0.0001 Then

                    Vh.SetSpeed0(jz, Vh.V0(jz) + amax)

                    GoTo lbGschw

                    '- Deceleration limit ---------------------------
                    'Else
                    '    'Check whether Deceleration too high
                    '    amax = GEN.aDesMin(Vist)
                    '    If amax > -0.001 Then
                    '        WorkerMsg(tMsgID.Err, "aDesMax(dec) invalid! v= " & Vist & ", aDesMax(dec) =" & amax, MsgSrc)
                    '        Return False
                    '    End If
                    '    If aist < amax - 0.0001 Then
                    '        Vh.SetSpeed0(jz, Vh.V0(jz) + amax)
                    '        GoTo lbGschw
                    '    End If
                    '----------------------------------------------------

                End If

            End If

            'From Power -----
            If aist < 0 Then
                If (Vist < 0.025) Then
                    'Vh.SetSpeed(jz, 0)
                    'GoTo lbGschw
                    Vist = 0
                End If
            End If
            '---------------

            'Determine Driving-state  -------------------------
            Pplus = False
            Pminus = False

            If Vist < 0.0001 Then
                VehState0 = tVehState.Stopped
            Else
                If aist >= 0.01 Then
                    VehState0 = tVehState.Acc
                ElseIf aist < -0.01 Then
                    VehState0 = tVehState.Dec
                Else
                    VehState0 = tVehState.Cruise
                End If
            End If

            PvorD = fPvD(jz)

            Select Case PvorD
                Case Is > 0.0001
                    Pplus = True
                Case Is < -0.0001
                    Pminus = True
            End Select

            'Faster check if Power is too high
            'If PvorD > 1.2 * VEH.Pnenn Then
            '    Vh.ReduceSpeed(jz, 0.9)
            '    GoTo lbGschw
            'End If

            'If jz > 0 AndAlso PvorD > 1.2 * LastPmax Then
            '    Vh.ReduceSpeed(jz, 0.95)
            '    GoTo lbGschw
            'End If


            '************************************ Gear selection ************************************
            If VehState0 = tVehState.Stopped Or TracIntrOn Then

                If TracIntrTurnOff Then

                    Gear = TracIntrGear
                    'If DEV.UseGearShiftPolygon Then
                    '    Gear = fGearVECTO(jz)
                    'Else
                    '    Gear = fGearLKW(jz)
                    'End If

                    If Not GBX.TCon AndAlso fnn(Vist, Gear, False) < Kuppln_norm And Pplus Then
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
                If Not GBX.TCon AndAlso fnn(Vist, 1, False) < Kuppln_norm And Pplus Then
                    Clutch = tEngClutch.Slipping
                Else
                    Clutch = tEngClutch.Closed
                End If

                If Gvorg Then
                    'Gear-settings
                    Gear = Math.Min(Vh.GearVorg(jz), VEH.ganganz)
                ElseIf Nvorg Then
                    'Revolutions-setting
                    Gear = fGearByU(MODdata.nUvorg(jz), Vist)
                ElseIf VEH.ganganz = 1 Then
                    Gear = 1
                Else
                    If GnachV Then
                        'Gear by speed dependent function
                        Gear = fGearBySpeed(jz)
                    Else

                        'Gear-shifting Model
                        If PKWja Then
                            Gear = fGearPKW(jz)
                        Else
                            Gear = fGearVECTO(jz)
                            'Gear = fGearLKW(jz)
                        End If

                        'Must be reset here because the Gear-shifting model may cause changes
                        MODdata.ModErrors.PxReset()

                    End If
                End If

                'Gear shifting-model / gear input can open Clutch
                If Gear < 1 Then

                    Clutch = tEngClutch.Opened

                Else

                    If Not GBX.TCon AndAlso fnn(Vist, Gear, False) < Kuppln_norm And Pplus And Not VehState0 = tVehState.Dec Then
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

            'Eco-Roll
            If Vh.EcoRoll(jz) AndAlso Math.Abs(PvorD) < 0.01 * VEH.Pnenn Then
                Clutch = tEngClutch.Opened
                Gear = 0

            End If

            ' Important checks
lbCheck:

            'Check whether to reduce speed
            ''If GeschwRed Then GoTo lbGeschwRed

            'Check whether Clutch is open:
            ''bKupplOffen = (bStehen Or Gear(jz) = 0) <= Already known by Clutch

            'If conventionall then ICE-clutch = master clutch
            ''bICEKupOffen = bKupplOffen <= i need nothing more

            'Falls vor Gangwahl festgestellt wurde, dass nicht KupplSchleif, dann bei zu niedriger Drehzahl runterschalten: |@@| If before?(vor) Gear-shift is detected that Clutch does not Lock, then Downshift at too low Revolutions:
            If Not GBX.TCon Then
                If Clutch = tEngClutch.Closed Then
                    If fnn(Vist, Gear, False) < Kuppln_norm And Not VehState0 = tVehState.Dec And Gear > 1 Then Gear -= 1
                End If
            End If


            'Check whether idling although Power > 0
            '   wenn Leistung vor Diff > 0.1% von Nennleistung dann Korrigieren! |@@| when Power before?(vor) Diff > 0.1% of Nominal-power, then Correct!
            If Clutch = tEngClutch.Opened Then
                If PvorD > 0.001 * VEH.Pnenn Then

                    If TracIntrOn Then
                        Gear = TracIntrGear
                    Else
                        Gear = 1
                    End If


                    If Not GBX.TCon AndAlso fnn(Vist, Gear, False) < Kuppln_norm Then
                        Clutch = tEngClutch.Slipping
                    Else
                        Clutch = tEngClutch.Closed
                    End If

                    GoTo lbCheck

                End If
            End If

            '************************************ Revolutions ************************************

            '*** If Revolutions specified then the next block is skipped ***
            If Nvorg Then

                nn = (MODdata.nUvorg(jz) - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                'If Start/Stop then it will be set at the same nn < -0.05 to nU = 0
                If GEN.StartStop And nn < Cfg.nnormEngStop Then
                    If Pplus Then
                        nn = 0
                        If FirstSecItar Then WorkerMsg(tMsgID.Warn, "target rpm < rpm_idle while power demand > 0", MsgSrc & "/t= " & jz + 1)
                    Else
                        nn = (0 - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)
                    End If
                End If

                If nn < -0.02 And Not GEN.StartStop Then
                    If FirstSecItar Then WorkerMsg(tMsgID.Warn, "target rpm < rpm_idle (Start/Stop disabled)", MsgSrc & "/t= " & jz + 1)
                End If

                GoTo lb_nOK

            End If

            'Revolutions drop when decoupling
            If Clutch = tEngClutch.Opened Then
                If jz = 0 Then
                    nn = 0
                Else

                    If MODdata.nn(jz - 1) <= 0.00001 Then
                        nn = MODdata.nn(jz - 1)
                        GoTo lb_nOK
                    End If



                    nnx = MODdata.nn(jz - 1)
                    nUx = MODdata.nU(jz - 1)
                    omega1 = nUx * 2 * Math.PI / 60
                    Pmin = 0
                    nU = nUx
                    i = 0
                    Do
                        PminX = Pmin
                        Pmin = FLD.Pdrag((nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl))
                        'Leistungsabfall limitieren auf Pe(t-1) minus 75% von (Pe(t-1) - Pschlepp) |@@| Limit Power-drop to Pe(t-1) minus 75% of (Pe(t-1) - Pdrag)
                        '   aus Auswertung ETC des Motors mit dem dynamische Volllast parametriert wurde |@@| of the evaluated ETC of the Enginges with the dynamic parametrized Full-load
                        '   Einfluss auf Beschleunigungsvermögen gering (Einfluss durch Pe(t-1) bei dynamischer Volllast mit PT1) |@@| Influence at low acceleration (influence dynamic Full-load through Pe(t-1) with PT1)
                        '   Luz/Rexeis 21.08.2012
                        '   Iteration loop: 01.10.2012
                        P = MODdata.Pe(jz - 1) * VEH.Pnenn - 0.75 * (MODdata.Pe(jz - 1) * VEH.Pnenn - Pmin)
                        M = -P * 1000 * 60 / (2 * Math.PI * nU)
                        'original: M = -Pmin * 1000 * 60 / (2 * Math.PI * ((nU + nUx) / 2))
                        omega_p = M / VEH.I_mot
                        omega2 = omega1 - omega_p
                        nU = omega2 * 60 / (2 * Math.PI)
                        i += 1
                        '01:10:12 Luz: Revolutions must not be higher than previously
                        If nU > nUx Then
                            nU = nUx
                            Exit Do
                        End If
                    Loop Until Math.Abs(Pmin - PminX) < 0.001 Or nU <= VEH.nLeerl Or i = 999

                    'TODO: Switch off?
                    If i = 999 Then WorkerMsg(tMsgID.Warn, "i=999", MsgSrc & "/t= " & jz + 1)

                    nn = (Math.Max(VEH.nLeerl, nU) - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                    MODdata.ModErrors.FLDextrapol = ""

                End If

            Else

                If GBX.TCon And Gear = 1 Then

                    PlossGB = fPlossGB(PvorD, Vist, Gear)
                    PlossDiff = fPlossDiff(PvorD, Vist)
                    PlossRt = fPlossRt(Vist, Gear)
                    PaGetr = fPaG(Vist, aist)
                    Pkup = PvorD + PlossGB + PlossDiff + PaGetr + PlossRt

                    If Not GBX.TCiteration(fnUout(Vist, Gear), Pkup) Then
                        WorkerMsg(tMsgID.Err, "TC Iteration failed!", MsgSrc & "/t= " & jz + 1)
                        Return False
                    End If

                    nn = (GBX.TCnUin - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                Else

                    nn = fnn(Vist, Gear, Clutch = tEngClutch.Slipping)

                    '*** Start: Revolutions Check

                    'Check whether Revolutions too high! => Upshift
                    Do While nn > 1.2 And Gear < VEH.ganganz
                        Gear += 1
                        nn = fnn(Vist, Gear, Clutch = tEngClutch.Slipping)
                    Loop

                    'Check whether Revolutions too low with the Clutch closed
                    If Clutch = tEngClutch.Closed Then
                        If nn < 0.0001 Then
                            Gear -= 1
                            nn = fnn(Vist, Gear, Clutch = tEngClutch.Slipping)
                        End If
                    End If

                End If

            End If




lb_nOK:


            '************************************ Determine Engine-state ************************************
            ' fix nn here!
            nU = nn * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl

            'Determine next Consumption (from VEH and DRI)
            Paux = fPaux(jz, nU)

            'ICE-inertia
            If Clutch = tEngClutch.Opened Then
                If jz = 0 Then
                    PaMot = 0
                Else
                    'Not optimal since jz-1 to jz not the right interval
                    PaMot = (VEH.I_mot * (nU - MODdata.nU(jz - 1)) * 0.01096 * nU) * 0.001
                End If
            Else
                If Nvorg Then
                    PaMot = (VEH.I_mot * MODdata.dnUvorg(jz) * 0.01096 * MODdata.nUvorg(jz)) * 0.001
                Else
                    PaMot = ((VEH.I_mot * (VEH.AchsI * VEH.Igetr(Gear) / (0.5 * VEH.Dreifen)) ^ 2) * aist * Vist) * 0.001
                End If
            End If

            'Total Engine-power
            '   => Pantr
            '   => P
            '   => Pkup
            Select Case Clutch
                Case tEngClutch.Opened
                    P = Paux + PaMot
                    Pkup = 0
                    PlossGB = 0
                    PlossDiff = 0
                    PlossRt = 0
                    PaGetr = 0
                Case tEngClutch.Closed

                    If GBX.TCon And Gear = 1 Then

                        P = nMtoPe(nU, GBX.TCMin) + Paux + PaMot

                    Else

                        PlossGB = fPlossGB(PvorD, Vist, Gear)
                        PlossDiff = fPlossDiff(PvorD, Vist)
                        PlossRt = fPlossRt(Vist, Gear)
                        PaGetr = fPaG(Vist, aist)
                        Pkup = PvorD + PlossGB + PlossDiff + PaGetr + PlossRt
                        P = Pkup + Paux + PaMot

                    End If
                Case Else 'tEngClutch.Slipping: never in AT mode!
                    PlossGB = fPlossGB(PvorD, Vist, Gear)
                    PlossDiff = fPlossDiff(PvorD, Vist)
                    PlossRt = fPlossRt(Vist, Gear)
                    PaGetr = fPaG(Vist, aist)
                    Pkup = (PvorD + PlossGB + PlossDiff + PaGetr + PlossRt) / KupplEta
                    P = Pkup + Paux + PaMot
            End Select

            'EngState
            If Clutch = tEngClutch.Opened Then

                Select Case P / VEH.Pnenn
                    Case Is > 0.0001    'Antrieb
                        EngState0 = tEngState.Load

                    Case Is < -0.0001   'Schlepp
                        EngState0 = tEngState.Drag

                    Case Else           'Idle/Stop
                        If GEN.StartStop Then
                            If Vist <= GEN.StStV / 3.6 Then
                                If StStAus And jz > 0 Then
                                    If MODdata.EngState(jz - 1) = tEngState.Stopped Then
                                        EngState0 = tEngState.Stopped
                                    Else
                                        EngState0 = tEngState.Idle
                                    End If
                                Else
                                    EngState0 = tEngState.Stopped
                                End If
                            Else
                                EngState0 = tEngState.Idle
                            End If
                        Else
                            EngState0 = tEngState.Idle
                        End If
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
                nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

            Else

                Pmin = FLD.Pdrag(nn)

                If jz = 0 Then
                    Pmax = FLD.Pfull(nn)
                Else
                    Pmax = FLD.Pfull(nn, MODdata.Pe(jz - 1))
                End If

                'If Pmax < 0 or Pmin > 0 then Abort with Error!
                If Pmin >= 0 And P < 0 Then
                    WorkerMsg(tMsgID.Err, "Pe_drag > 0! n= " & nU & " [1/min]", MsgSrc & "/t= " & jz + 1, FLD.FilePath)
                    Return False
                ElseIf Pmax <= 0 And P > 0 Then
                    WorkerMsg(tMsgID.Err, "Pe_full < 0! n= " & nU & " [1/min]", MsgSrc & "/t= " & jz + 1, FLD.FilePath)
                    Return False
                End If

            End If

            '   => Pbrake
            If Clutch = tEngClutch.Opened Then
                If PvorD < -0.00001 Then
                    Pbrake = PvorD
                Else
                    Pbrake = 0
                End If

                If P < Pmin Then P = Pmin

            Else
                If EngState0 = tEngState.Load Then
                    Pbrake = 0
                    If GBX.TCon And Gear = 1 Then Pbrake = GBX.TC_PeBrake
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
                        Pkup = P - Paux - PaMot
                        PaGetr = fPaG(Vist, aist)
                        PlossGB = fPlossGBfwd(Pkup, Vist, Gear)
                        PlossDiff = fPlossDiffFwd(Pkup - PlossGB, Vist)
                        PlossRt = fPlossRt(Vist, Gear)

                        Pbrake = PvorD - (Pkup - PlossGB - PlossDiff - PaGetr - PlossRt)

                        EngState0 = tEngState.FullDrag
                    Else
                        Pbrake = 0
                    End If
                End If
            End If

            'Check or Abort (before Speed-reduce-iteration, otherwise it hangs)
            If PHEMworker.CancellationPending Then Return True

            'Check whether P above Full-load => Reduce Speed
            If Pplus And P > Pmax Then
                If EngState0 = tEngState.Load Or EngState0 = tEngState.FullLoad Then
                    If Vist > 0.01 Then
                        Select Case P / Pmax
                            Case Is > 1.6   '1.6
                                Vh.ReduceSpeed(jz, 0.99)   '0.99
                            Case Is > 1.3   '1.3
                                Vh.ReduceSpeed(jz, 0.999)  '0.995
                            Case Else
                                Vh.ReduceSpeed(jz, 0.9999)  '0.999
                        End Select
                        FirstSecItar = False
                        GoTo lbGschw
                    Else
                        'ERROR: Speed Reduction brings nothing? ...
                        WorkerMsg(tMsgID.Err, "Speed reduction failed!", MsgSrc & "/t= " & jz + 1)
                        Return False
                    End If
                Else 'tEngState.Idle, tEngState.Stopped, tEngState.Drag
                    'ERROR:  Engine not in Drivetrain ... can it be?
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
                            ZgkrDt = VEH.TracIntrSi - CSng(TracIntrIx)
                        Else
                            ZgkrDt = 1
                        End If

                        Vrollout = fRolloutSpeed(jz, ZgkrDt)

                        If Vrollout < Vist Or VehState0 <> tVehState.Dec Then Vh.SetSpeed(jz, Vrollout)

                        GoTo lbGschw

                    End If

                End If

            End If

            '--------------------------------------------------------------------------------------------------
            '------------------------- PNR --------------------------------------------------------------------
            '--------------------------------------------------------------------------------------------------
            '   Finish Second

            'Start / Stop - Activation-Speed Control
            If GEN.StartStop Then
                If StStAus Then
                    If Not EngState0 = tEngState.Stopped Then
                        StStTx += 1
                        If StStTx > GEN.StStT Then
                            StStTx = 1
                            StStAus = False
                        End If
                    End If
                Else
                    If EngState0 = tEngState.Stopped Then StStAus = True
                End If
            End If

            'Write Modal-values Fields
            MODdata.Pe.Add(P / VEH.Pnenn)
            MODdata.nn.Add(nn)
            MODdata.nU.Add(nU)

            MODdata.EngState.Add(EngState0)

            MODdata.Pa.Add(fPaFZ(MODdata.Vh.V(jz), MODdata.Vh.a(jz)))
            MODdata.Pluft.Add(fPair(MODdata.Vh.V(jz), jz))
            MODdata.Proll.Add(fPr(MODdata.Vh.V(jz)))
            MODdata.Pstg.Add(fPs(MODdata.Vh.V(jz), jz))
            MODdata.Pbrake.Add(Pbrake)
            MODdata.Psum.Add(PvorD)
            MODdata.PauxSum.Add(Paux)

            For Each AuxID As String In VEH.AuxRefs.Keys
                MODdata.Paux(AuxID).Add(VEH.Paux(AuxID, jz, nU))
            Next

            MODdata.PlossGB.Add(PlossGB)
            MODdata.PlossDiff.Add(PlossDiff)
            MODdata.PlossRt.Add(PlossRt)
            MODdata.PaEng.Add(PaMot)
            MODdata.PaGB.Add(PaGetr)

            MODdata.VehState.Add(VehState0)
            MODdata.Gear.Add(Gear)

            'Torque Converter output
            If GBX.TCon Then
                If Gear = 1 Then
                    If nU = 0 Then
                        MODdata.TCnu.Add(0)
                    Else
                        MODdata.TCnu.Add(GBX.TCnout / nU)
                    End If
                    If GBX.TCMin = 0 Then
                        MODdata.TCmu.Add(0)
                    Else
                        MODdata.TCmu.Add(GBX.TCMout / GBX.TCMin)
                    End If
                    MODdata.TCMout.Add(GBX.TCMout)
                    MODdata.TCnOut.Add(GBX.TCnout)
                Else
                    MODdata.TCnu.Add(1)
                    MODdata.TCmu.Add(1)
                    MODdata.TCMout.Add(nPeToM(nU, P - Paux - PaMot))
                    MODdata.TCnOut.Add(nU)
                End If
            End If

            If Cfg.WegKorJa Then Vh.DistCorrection(jz, VehState0)

            'Interruption of traction(Zugkraftunterbrechung)
            If TracIntrTurnOff Then

                TracIntrOn = False
                TracIntrTurnOff = False

            ElseIf TracIntrOn Then

                TracIntrIx += 1

                If TracIntrIx = TracIntrI Then

                    TracIntrTurnOff = True

                ElseIf jz < MODdata.tDim Then

                    If TracIntrIx + 1 = TracIntrI Then
                        ZgkrDt = VEH.TracIntrSi - CSng(TracIntrIx)
                    Else
                        ZgkrDt = 1
                    End If

                    Vrollout = fRolloutSpeed(jz + 1, ZgkrDt)
                    If Vrollout < Vist Or VehState0 <> tVehState.Dec Then Vh.SetSpeed(jz + 1, Vrollout)

                End If

            End If

            If Vh.Vsoll(jz) - Vist > 1.5 Then SecSpeedRed += 1


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

            If Clutch = tEngClutch.Closed And Nvorg Then
                If Math.Abs(nU - fnU(Vist, Gear, False)) > 0.2 * VEH.nNenn Then
                    WorkerMsg(tMsgID.Warn, "Target rpm =" & nU & ", calculated rpm(gear " & Gear & ")= " & fnU(Vist, Gear, False), MsgSrc & "/t= " & jz + 1)
                End If
            End If

            LastPmax = Pmax

        Loop Until jz >= MODdata.tDim

        '***********************************************************************************************
        '***********************************    Time loop END ***********************************
        '***********************************************************************************************

        'Notify (When not ADVANCE)
        If NotAdvMode Then

            If Cfg.WegKorJa Then
                If MODdata.tDim > MODdata.tDimOgl Then WorkerMsg(tMsgID.Normal, "Cycle extended by " & MODdata.tDim - MODdata.tDimOgl & " seconds to meet target distance.", MsgSrc)
            End If

            If SecSpeedRed > 0 Then WorkerMsg(tMsgID.Normal, "Speed reduction > 1.5 m/s in " & SecSpeedRed & " time steps.", MsgSrc)

        End If

        'CleanUp
        Vh = Nothing

        Return True

    End Function

    Public Function Eng_Calc() As Boolean

        Dim Pmr As Single
        Dim t As Integer
        Dim t1 As Integer
        Dim PminN As Single
        Dim PmaxN As Single
        Dim nnDRI As List(Of Double)
        Dim PeDRI As List(Of Double)
        Dim PcorCount As Integer
        Dim StdMode As Boolean
        Dim NotAdvMode As Boolean
        Dim MsgSrc As String
        Dim Padd As Single

        MsgSrc = "Power/Eng_Calc"

        StdMode = (PHEMmode = tPHEMmode.ModeSTANDARD)
        NotAdvMode = Not (PHEMmode = tPHEMmode.ModeADVANCE)

        'Abort if Power/Revolutions not given
        If Not (DRI.Nvorg And DRI.Pvorg) Then
            WorkerMsg(tMsgID.Err, "Load cycle is not valid! rpm and load required.", MsgSrc)
            Return False
        End If

        PcorCount = 0
        t1 = MODdata.tDim
        nnDRI = DRI.Values(tDriComp.nn)
        PeDRI = DRI.Values(tDriComp.Pe)

        'Drehzahlen vorher weil sonst scheitert die Pmr-Berechnung bei MODdata.nU(t + 1) |@@| Revolutions previously, otherwise Pmr-calculation fails at MODdata.nU(t + 1)
        For t = 0 To t1
            MODdata.nn.Add(nnDRI(t))
            MODdata.nU.Add(Math.Max(0, nnDRI(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl))
        Next

        'Power calculation
        For t = 0 To t1

            'Secondary Progressbar
            If NotAdvMode Then ProgBarCtrl.ProgJobInt = CInt(100 * t / t1)

            'Reset the second-by-second Errors
            MODdata.ModErrors.ResetAll()

            'OLD and wrong because not time shifted: P_mr(jz) = 0.001 * (I_mot * 0.0109662 * (n(jz) * nnrom) * nnrom * (n(jz) - n(jz - 1))) 
            If t > 0 And t < t1 Then
                Pmr = 0.001 * (VEH.I_mot * (2 * Math.PI / 60) ^ 2 * MODdata.nU(t) * 0.5 * (MODdata.nU(t + 1) - MODdata.nU(t - 1)))
            Else
                Pmr = 0
            End If

            Padd = MODdata.Vh.Padd(t)

            'Power = P_clutch + + Pa_eng + Padd
            MODdata.Pe.Add(PeDRI(t) + (Pmr + Padd) / VEH.Pnenn)

            'Revolutions of the Cycle => Determined in Cycle-init
            'If Revolutions under idle, assume Engine is stopped
            If MODdata.nn(t) < Cfg.nnormEngStop Then
                EngState0 = tEngState.Stopped
            Else
                PminN = FLD.Pdrag(MODdata.nn(t)) / VEH.Pnenn

                If t = 0 Then
                    PmaxN = FLD.Pfull(MODdata.nn(t)) / VEH.Pnenn
                Else
                    PmaxN = FLD.Pfull(MODdata.nn(t), MODdata.Pe(t - 1)) / VEH.Pnenn
                End If

                'If Pmax < 0 or Pmin >  0 then Abort with Error!
                If PminN >= 0 AndAlso MODdata.Pe(t) < 0 Then
                    WorkerMsg(tMsgID.Err, "Pe_drag > 0! n= " & MODdata.nU(t) & " [1/min]", MsgSrc & "/t= " & t + 1, FLD.FilePath)
                    Return False
                ElseIf PmaxN <= 0 AndAlso MODdata.Pe(t) > 0 Then
                    WorkerMsg(tMsgID.Err, "Pe_full < 0! n= " & MODdata.nU(t) & " [1/min]", MsgSrc & "/t= " & t + 1, FLD.FilePath)
                    Return False
                End If

                'FLD Check
                If MODdata.Pe(t) > PmaxN Then
                    If MODdata.Pe(t) / PmaxN > 1.05 Then PcorCount += 1
                    MODdata.Pe(t) = PmaxN
                ElseIf MODdata.Pe(t) < PminN Then
                    If MODdata.Pe(t) / PminN > 1.05 Then PcorCount += 1
                    MODdata.Pe(t) = PminN
                End If

                Select Case MODdata.Pe(t)
                    Case Is > 0.0001  'Antrieb
                        If Math.Abs(MODdata.Pe(t) / PmaxN - 1) < 0.01 Then
                            EngState0 = tEngState.FullLoad
                        Else
                            EngState0 = tEngState.Load
                        End If
                    Case Is < -0.0001   'Schlepp
                        If MODdata.Pe(t) < 1.01 * PminN Then
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
            MODdata.PauxSum.Add(Padd)

            'Notify
            If MODdata.ModErrors.MsgOutputAbort(t + 1, MsgSrc) Then Return False

        Next

        If PcorCount > 0 Then WorkerMsg(tMsgID.Warn, "Power corrected (>5%) in " & PcorCount & " time steps.", MsgSrc)

        Return True

    End Function

    Private Function fRolloutSpeed(ByVal t As Integer, ByVal dt As Single) As Single

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

        PvD = fPvD(t, v, a)

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

                PvD = fPvD(t, v, a)

            End If

        Loop

        Return v

    End Function

    Private Function fCoastingSpeed(ByVal t As Integer, ByVal Gear As Integer, ByVal NoPosGrad As Boolean) As Single

        Dim vstep As Double
        Dim vVorz As Integer
        Dim Pe As Single
        Dim a As Single
        Dim v As Single
        Dim PvD As Single
        Dim LastDiff As Single
        Dim Diff As Single
        Dim nU As Single
        Dim Pdrag As Single
        Dim Grad As Single

        v = MODdata.Vh.V(t)

        vstep = 0.1
        a = MODdata.Vh.a(t)
        nU = fnU(v, Gear, False)
        Pdrag = FLD.Pdrag((nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl))

        'Do not allow positive road gradients
        If NoPosGrad Then
            Grad = Math.Min(MODdata.Vh.Grad(t), 0)
        Else
            Grad = MODdata.Vh.Grad(t)
        End If

        PvD = fPvD(t, v, a, Grad)
        Pe = PvD + fPlossGB(PvD, v, Gear) + fPlossDiff(PvD, v) + fPaG(v, a) + fPlossRt(v, Gear) + fPaux(t, nU) + fPaMot(t, Gear, v, a)

        Diff = Math.Abs(Pdrag - Pe) / VEH.Pnenn

        If Diff > DEV.SpeedPeEps Then
            vVorz = 1
        Else
            Return v
        End If

        LastDiff = Diff + 10 * DEV.SpeedPeEps

        Do While Diff > DEV.SpeedPeEps 'And Math.Abs(LastDiff - Diff) > eps

            If LastDiff < Diff Or v + vVorz * vstep <= 0.0001 Then
                vVorz *= -1
                vstep *= 0.5

                If vstep < 0.00001 Then Exit Do

            End If

            v += vVorz * vstep

            a = 2 * (v - MODdata.Vh.V0(t)) / 1  'dt = 1[s]

            nU = fnU(v, Gear, False)
            Pdrag = FLD.Pdrag((nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl))

            LastDiff = Diff

            PvD = fPvD(t, v, a, Grad)
            Pe = PvD + fPlossGB(PvD, v, Gear) + fPlossDiff(PvD, v) + fPaG(v, a) + fPlossRt(v, Gear) + fPaux(t, nU) + fPaMot(t, Gear, v, a)

            Diff = Math.Abs(Pdrag - Pe) / VEH.Pnenn

        Loop

        Return v

    End Function


#Region "Schaltmodelle"

    Private Function fFastGearCalc(ByVal V As Single, ByVal Pe As Single) As Integer
        Dim Gear As Integer
        Dim Md As Single
        Dim nU As Single
        Dim nn As Single
        Dim nnUp As Single
        Dim nnDown As Single

        For Gear = VEH.ganganz To 1 Step -1

            nU = CSng(Vist * 60.0 * VEH.AchsI * VEH.Igetr(Gear) / (VEH.Dreifen * Math.PI))
            nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

            'Current torque demand with previous gear
            Md = Pe * 1000 / (nU * 2 * Math.PI / 60)

            'Up/Downshift rpms
            nnUp = GBX.fGSnnUp(Md)
            nnDown = GBX.fGSnnDown(Md)

            If nn > nnDown Then Return Gear

        Next

        Return 1

        MODdata.ModErrors.GSextrapol = ""

    End Function

    Private Function fStartGear(ByVal t As Integer) As Integer
        Dim Gear As Integer
        Dim nn As Single
        Dim MsgSrc As String
        Dim nU As Single
        Dim nnUp As Single
        Dim nnDown As Single
        Dim Md As Single
        Dim Pe As Single
        Dim MdMax As Single
        Dim Pmax As Single

        MsgSrc = "StartGear"

        If t = 0 AndAlso VehState0 <> tVehState.Stopped Then

            'Calculate gear when cycle starts with speed > 0
            For Gear = VEH.ganganz To 1 Step -1

                'rpm
                nU = fnU(Vist, Gear, Clutch = tEngClutch.Slipping)

                'normalized rpm
                nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                'full load
                Pmax = FLD.Pfull(nn)

                'power demand - cut at full load / drag so that fGSnnUp and fGSnnDown don't extrapolate
                Pe = Math.Min(fPeGearMod(Gear, t) * VEH.Pnenn, Pmax)
                Pe = Math.Max(Pe, FLD.Pdrag(nn))

                'torque demand
                Md = Pe * 1000 / (nU * 2 * Math.PI / 60)

                'Up/Downshift rpms
                nnUp = GBX.fGSnnUp(Md)
                nnDown = GBX.fGSnnDown(Md)

                'Max torque
                MdMax = Pmax * 1000 / (nU * 2 * Math.PI / 60)

                'Find highest gear with rpm below Upshift-rpm and with enough torque reserve 
                If nn < nnUp And nn > nnDown And 1 - Md / MdMax >= GBX.gs_TorqueResv / 100 Then
                    Exit For
                ElseIf nn > nnUp And Gear < VEH.ganganz Then
                    MODdata.ModErrors.GSextrapol = ""
                    Return Gear + 1
                End If

            Next

        Else

            'Calculate Start Gear 
            For Gear = VEH.ganganz To 1 Step -1

                'rpm at StartSpeed  [m/s]
                nU = GBX.gs_StartSpeed * 60.0 * VEH.AchsI * VEH.Igetr(Gear) / (VEH.Dreifen * Math.PI)

                'normalized rpm
                nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                'full load
                Pmax = FLD.Pfull(nn)

                'Max torque
                MdMax = Pmax * 1000 / (nU * 2 * Math.PI / 60)

                'power demand
                Pe = Math.Min(fPeGearMod(Gear, t, GBX.gs_StartSpeed, GBX.gs_StartAcc) * VEH.Pnenn, Pmax)
                Pe = Math.Max(Pe, FLD.Pdrag(nn))

                'torque demand
                Md = Pe * 1000 / (nU * 2 * Math.PI / 60)

                'Up/Downshift rpms
                nnUp = GBX.fGSnnUp(Md)
                nnDown = GBX.fGSnnDown(Md)

                '   Debug.Print(CurrentCycleFile & "," & Gear & "," & Md & "," & MdMax & "," & nn & "," & nnDown & "," & nnUp)

                If nn > nnDown And nU >= VEH.nLeerl And (1 - Md / MdMax >= GBX.gs_TorqueResvStart / 100 Or Md < 0) Then Exit For

            Next

        End If

        MODdata.ModErrors.GSextrapol = ""

        Return Gear


    End Function

    Private Function fGearVECTO(ByVal t As Integer) As Integer
        Dim nU As Single
        Dim nn As Single
        Dim nnUp As Single
        Dim nnDown As Single
        Dim Md As Single
        Dim Pe As Single
        Dim LastGear As Int16
        Dim Gear As Int16
        Dim MdMax As Single
        Dim LastPeNorm As Single

        Dim iphase As Int16
        Dim iphase0 As Int16
        Dim itgangw As Integer
        Dim i As Integer
        Dim bCheck As Boolean
        Dim Pjetzt As Single
        Dim Pvorher As Single
        Dim a As Single
        Dim b As Single
        Dim tx As Int16
        Dim OutOfRpmRange As Boolean

        'First time step OR first time step after stand still
        If t = 0 OrElse MODdata.VehState(t - 1) = tVehState.Stopped Then Return fStartGear(t)


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

        nU = CSng(Vist * 60.0 * VEH.AchsI * VEH.Igetr(LastGear) / (VEH.Dreifen * Math.PI))
        nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

        OutOfRpmRange = (nn >= 1.2 Or nU < VEH.nLeerl)

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
        nU = fnU(Vist, LastGear, Clutch = tEngClutch.Slipping)

        'Current normalized rpm with previous gear
        nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

        'Current power demand with previous gear
        Pe = Math.Min(fPeGearMod(LastGear, t) * VEH.Pnenn, FLD.Pfull(nn))
        Pe = Math.Max(Pe, FLD.Pdrag(nn))

        'Current torque demand with previous gear
        Md = Pe * 1000 / (nU * 2 * Math.PI / 60)
        MdMax = FLD.Pfull(nn) * 1000 / (nU * 2 * Math.PI / 60)

        'Up/Downshift rpms
        nnUp = GBX.fGSnnUp(Md)
        nnDown = GBX.fGSnnDown(Md)

        'Compare rpm with Up/Downshift rpms 
        If nn <= nnDown And LastGear > 1 Then

            'Shift DOWN
            Gear = LastGear - 1

            'Skip Gears
            If GBX.gs_SkipGears AndAlso Gear > 1 Then

                'Calculate Shift-rpm for lower gear
                nU = fnU(Vist, Gear - 1, False)
                nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                'Continue only if rpm (for lower gear) is above idling
                If nn >= 0 Then
                    Pe = Math.Min(fPeGearMod(Gear - 1, t) * VEH.Pnenn, FLD.Pfull(nn))
                    Pe = Math.Max(Pe, FLD.Pdrag(nn))
                    Md = Pe * 1000 / (nU * 2 * Math.PI / 60)
                    nnUp = GBX.fGSnnUp(Md)
                    nnDown = GBX.fGSnnDown(Md)

                    'Shift down as long as Gear > 1 and rpm is below UpShift-rpm
                    Do While Gear > 1 AndAlso nn < nnUp

                        'Shift DOWN
                        Gear -= 1

                        'Continue only if Gear > 1
                        If Gear = 1 Then Exit Do

                        'Calculate Shift-rpm for lower gear
                        nU = fnU(Vist, Gear - 1, False)
                        nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                        'Continue only if rpm (for lower gear) is above idling
                        If nn < 0 Then Exit Do

                        Pe = Math.Min(fPeGearMod(Gear - 1, t) * VEH.Pnenn, FLD.Pfull(nn))
                        Pe = Math.Max(Pe, FLD.Pdrag(nn))
                        Md = Pe * 1000 / (nU * 2 * Math.PI / 60)
                        nnUp = GBX.fGSnnUp(Md)
                        nnDown = GBX.fGSnnDown(Md)

                    Loop

                End If

            End If

        ElseIf LastGear < VEH.ganganz And nn > nnUp Then

            'Shift UP
            Gear = LastGear + 1

            'Skip Gears
            If GBX.gs_SkipGears AndAlso Gear < VEH.ganganz Then

                'Calculate Shift-rpm for higher gear
                nU = fnU(Vist, Gear + 1, False)
                nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                'Continue only if rpm (for higher gear) is below rated rpm
                If nn <= 1 Then
                    Pe = Math.Min(fPeGearMod(Gear + 1, t) * VEH.Pnenn, FLD.Pfull(nn))
                    Pe = Math.Max(Pe, FLD.Pdrag(nn))
                    Md = Pe * 1000 / (nU * 2 * Math.PI / 60)
                    nnUp = GBX.fGSnnUp(Md)
                    nnDown = GBX.fGSnnDown(Md)

                    'Max Torque
                    MdMax = FLD.Pfull(nn, LastPeNorm) * 1000 / (nU * 2 * Math.PI / 60)

                    'Shift up as long as Torque reserve is okay and Gear < Max-Gear and rpm is above DownShift-rpm
                    Do While Gear < VEH.ganganz AndAlso 1 - Md / MdMax >= GBX.gs_TorqueResv / 100 AndAlso nn > nnDown '+ 0.1 * (nnUp - nnDown)

                        'Shift UP
                        Gear += 1

                        'Continue only if Gear < Max-Gear
                        If Gear = VEH.ganganz Then Exit Do

                        'Calculate Shift-rpm for higher gear
                        nU = fnU(Vist, Gear + 1, False)
                        nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                        'Continue only if rpm (for higher gear) is below rated rpm
                        If nn > 1 Then Exit Do

                        Pe = Math.Min(fPeGearMod(Gear + 1, t) * VEH.Pnenn, FLD.Pfull(nn))
                        Pe = Math.Max(Pe, FLD.Pdrag(nn))
                        Md = Pe * 1000 / (nU * 2 * Math.PI / 60)
                        nnUp = GBX.fGSnnUp(Md)
                        nnDown = GBX.fGSnnDown(Md)

                        'Max Torque
                        MdMax = FLD.Pfull(nn, LastPeNorm) * 1000 / (nU * 2 * Math.PI / 60)

                    Loop

                End If

            End If

        Else

            'Keep last gear
            Gear = LastGear

            'Shift UP inside shift polygons
            If GBX.gs_ShiftInside And LastGear < VEH.ganganz Then

                'Calculate Shift-rpm for higher gear
                nU = fnU(Vist, Gear + 1, False)
                nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                'Continue only if rpm (for higher gear) is below rated rpm
                If nn <= 1 Then
                    Pe = Math.Min(fPeGearMod(Gear + 1, t) * VEH.Pnenn, FLD.Pfull(nn))
                    Pe = Math.Max(Pe, FLD.Pdrag(nn))
                    Md = Pe * 1000 / (nU * 2 * Math.PI / 60)
                    nnUp = GBX.fGSnnUp(Md)
                    nnDown = GBX.fGSnnDown(Md)

                    'Max Torque
                    MdMax = FLD.Pfull(nn, LastPeNorm) * 1000 / (nU * 2 * Math.PI / 60)

                    'Shift up as long as Torque reserve is okay and Gear < Max-Gear and rpm is above DownShift-rpm
                    Do While Gear < VEH.ganganz AndAlso 1 - Md / MdMax >= GBX.gs_TorqueResv / 100 AndAlso nn > nnDown '+ 0.1 * (nnUp - nnDown)

                        'Shift UP
                        Gear += 1

                        'Continue only if Gear < Max-Gear
                        If Gear = VEH.ganganz Then Exit Do

                        'Calculate Shift-rpm for higher gear
                        nU = fnU(Vist, Gear + 1, False)
                        nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                        'Continue only if rpm (for higher gear) is below rated rpm
                        If nn > 1 Then Exit Do

                        Pe = Math.Min(fPeGearMod(Gear + 1, t) * VEH.Pnenn, FLD.Pfull(nn))
                        Pe = Math.Max(Pe, FLD.Pdrag(nn))
                        Md = Pe * 1000 / (nU * 2 * Math.PI / 60)
                        nnUp = GBX.fGSnnUp(Md)
                        nnDown = GBX.fGSnnDown(Md)

                        'Max Torque
                        MdMax = FLD.Pfull(nn, LastPeNorm) * 1000 / (nU * 2 * Math.PI / 60)

                    Loop


                End If

            End If

        End If

        '*** Error-Msg-Check ***
        'Current rpm 
        nU = fnU(Vist, Gear, Clutch = tEngClutch.Slipping)
        nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)
        'Current power demand
        Pe = Math.Min(fPeGearMod(Gear, t) * VEH.Pnenn, FLD.Pfull(nn))
        Pe = Math.Max(Pe, FLD.Pdrag(nn))
        'Current torque demand
        Md = Pe * 1000 / (nU * 2 * Math.PI / 60)
        'Clear old errors
        MODdata.ModErrors.GSextrapol = ""
        'Check for errors
        GBX.fGSnnDown(Md)
        GBX.fGSnnUp(Md)


        'If GearCorrection is OFF then return here
        If Not DEV.GearCorrection Then Return Gear

        'Search for last Gear-change
        itgangw = 0
        For i = t - 1 To 1 Step -1
            If MODdata.Gear(i) <> MODdata.Gear(i - 1) Then
                itgangw = i
                Exit For
            End If
        Next

        'Maximum permissible Gear-shifts every 3 seconds:
        If t - itgangw <= GBX.gs_ShiftTime And t > GBX.gs_ShiftTime - 1 Then
            Return LastGear    '<<< no further checks!!!
        End If

        'Checks to Purge non-sensible Gear-shift:
        'Division into "IPhase(j)" stages: Acceleration(=1), Deceleration(=2) and Cruise(=3):
        iphase = 0
        If t > 1 Then
            Select Case (MODdata.Vh.a(t - 2) + MODdata.Vh.a(t - 1) + MODdata.Vh.a(t)) / 3
                Case Is >= 0.125
                    iphase = 1
                Case Is <= -0.125
                    iphase = 2
                Case Else
                    iphase = 3
            End Select
        End If

        iphase0 = 0
        If t > 3 Then
            Select Case (MODdata.Vh.a(t - 3) + MODdata.Vh.a(t - 2) + MODdata.Vh.a(t - 1)) / 3
                Case Is >= 0.125
                    iphase0 = 1
                Case Is <= -0.125
                    iphase0 = 2
                Case Else
                    iphase0 = 3
            End Select
        End If

        'Cruise-phases:
        'As long Speed-change since last Gear-shift is below 6% and Pe/Pnom below 6% then do not Gear-shift:
        'Deceleration-phases: Upshift suppressed
        'Acceleration phases: Downshift suppressed
        bCheck = False
        Pjetzt = fPeGearModvD(t)
        Pvorher = MODdata.Pe(itgangw)
        If MODdata.Vh.V(itgangw) = 0 Then
            a = Math.Abs(Vist / 0.0001 - 1)
        Else
            a = Math.Abs(Vist / MODdata.Vh.V(itgangw) - 1)
        End If
        If Pvorher = 0 Then
            b = Math.Abs(Pjetzt / 0.0001 - 1)
        Else
            b = Math.Abs(Pjetzt / Pvorher - 1)
        End If
        If iphase = 3 And a < 0.06 And b < 0.06 Then bCheck = True
        If (iphase = 1) And Gear < MODdata.Gear(t - 1) And iphase0 = 1 Then bCheck = True
        If (iphase = 2) And Gear > MODdata.Gear(t - 1) Then bCheck = True
        If bCheck Then Gear = LastGear

        'Shifting from 2nd to 1st Gear is suppressed when v > 1.5 m/s
        'NEU LUZ 040210: Hochschalten nur wenn im 2. Gang über Kuppeldrehzahl |@@| NEW LUZ 040210: Upshifting only when in 2nd Gear over the Clutch-revolutions
        If Gear = 1 And LastGear > 1 And Vist >= 1.5 Then
            If fnn(Vist, 2, False) > Kuppln_norm Then Gear = 2
        End If

        Return Gear


    End Function

    Private Function fGearPKW(ByVal t As Integer) As Integer

        Dim gangX As Int16
        Dim Gear As Integer
        Dim ix As Integer
        Dim nn As Single
        Dim tx As Int32
        Dim t1 As Int32
        Dim V_norm As Single
        Dim jpm As Integer
        Dim AP10 As Single
        Dim Pjetzt As Single
        Dim Pvorher As Single
        Dim Pjzx As Single
        Dim nnsaufi As Single
        Dim nnsobi As Single
        Dim nsa As Single
        Dim nsd As Single
        Dim itgangwL As Integer
        Dim bCheck As Boolean
        Dim Gcheck(10) As Short
        Dim Gears(11) As Integer      '<<<<==== !!!!!!
        Dim LastGear As Integer
        Dim a As Single
        Dim b As Single
        Dim Valt As Single
        Dim nx As Single
        Dim Vist0 As Single

        '-----------------------------------Second 1 --------------------------------------
        'First second: find Gear / Initialization
        If t = 0 Then
            gangX = -1
            If (Vist <= 1.5) Then
                Gear = 1
            Else
                For ix = 1 To VEH.ganganz
                    nn = fnn(Vist, ix, False)

                    If (ix < VEH.ganganz) Then
                        If (nn <= 0.75) Then
                            gangX = ix
                            Exit For
                        End If
                    Else
                        If (nn <= 1.0) Then
                            gangX = ix
                            Exit For
                        End If
                    End If
                Next ix
                If gangX = -1 Then gangX = VEH.ganganz
                Gear = gangX
            End If
            Return Gear
        End If

        '--------------------------------From second 2 --------------------------------------

        '---------Start-values ---------
        'gangX = Last Gang ie Basis for Gear-shiftching model
        LastGear = MODdata.Gear(t - 1)
        gangX = LastGear
        Gear = LastGear
        t1 = MODdata.tDim
        itgangwL = -1

        'Clutch-lock(Kuppelschleif) check << already happened in Power.Calc
        ''If bPplus And fn_norm(1) < Kuppln_norm Then bKupplSchleif = True

        '-------------------Calculate Gear for the next 6 seconds ---------------------
        '-------------------------------------------------------------

        Pvorher = MODdata.Pe(t - 1)

        tx = t
        Do
            '-----------Gear-shifting function ----------
            Vist0 = MODdata.Vh.V(t)
            V_norm = Vist0 / GVmax
            nx = fnU(Vist0, gangX, False) / VEH.nNenn
            Valt = MODdata.Vh.V(t - 1)

            If (t >= 6) Then
                jpm = 5
            Else
                jpm = t - 1
            End If
            AP10 = 0

            t = t - jpm
            ix = 1
            Do
                If t < tx Then
                    Pjetzt = MODdata.Pe(t)
                Else
                    Pjetzt = fPeGearModvD(t)
                    If t = tx Then Pjzx = Pjetzt
                End If

                If t = t - ix + jpm Then
                    Pvorher = Pjetzt
                End If

                'Gelöscht LUZ 13.07.10: If (Pjetzt > 1) Then Pjetzt = 1.0
                'If ix = jpm Then Pvorher = Pjetzt



                AP10 = AP10 + Pjetzt / (jpm + 6)
                t += 1
                ix += 1
            Loop Until ix = 11 Or t > t1
            t = t - ix + jpm + 1

            'Revolutions-limit for Upshifting  n_normiert (Idle = 0, Nominal-revolutions = 1)
            nnsaufi = Aaufi + Baufi * V_norm + Caufi * AP10
            If (nnsaufi > 0.95) Then nnsaufi = 0.95
            'Revolutions-limit for Downhifting  n_normiert (Idle = 0, Nominal-revolutions = 1)
            nnsobi = Aobi + Bobi * V_norm + Cobi * AP10
            'Deleted by LUZ  13.07.10: If (nnsaufi > 0.85)  Then nnsaufi = 0.85
            'Convert here the Revolutions-units (n/n_nom):
            nsa = (VEH.nLeerl / VEH.nNenn) + nnsaufi * (1 - (VEH.nLeerl / VEH.nNenn))
            nsd = (VEH.nLeerl / VEH.nNenn) + nnsobi * (1 - (VEH.nLeerl / VEH.nNenn))
            'Revolutions with last Gear (gangX)
            'nx = fnU(Vist, gangX, Clutch = tEngClutch.Slipping) / VEH.nNenn
            '-----------------------------------

            ' ''Maximum permissible Gear-shift every 2 seconds:
            If (t - itgangwL) < 3 And itgangwL > -1 Then GoTo lb10

            'Check whether Downshift, only when Speed decreases or Power increases
            bCheck = False
            Pjetzt = fPeGearModvD(t)
            If Vist0 < Valt Then bCheck = True
            If (Pjetzt > Pvorher) Then bCheck = True
            If bCheck Then
                If nx < nsd Then gangX -= 1
            End If

            'Check whether Upshift, only when Speed increases or Power decreases
            bCheck = False
            If (Vist0 > Valt) Then bCheck = True
            If (Pjetzt < Pvorher) Then bCheck = True
            If bCheck Then
                If nx > nsa Then gangX += 1
            End If

            'Correct Gear-selection
            If gangX > VEH.ganganz Then
                gangX = VEH.ganganz
            ElseIf gangX < 1 Then
                gangX = 1
            End If

lb10:

            'Not Idle when Power > 0
            If gangX = 0 Then
                If Pjzx > 0.001 Then gangX = 1
            End If

            'New Revolutions
            ''nn = fnn(Vist, gangX, Clutch = tEngClutch.Slipping)
            nn = fnn(Vist0, gangX, False)

            'Check whether Gear within the Power/Revolutions limits. Drag is not respected
            Select Case nn
                Case Is < Kuppln_norm
                    If gangX > 1 Then
                        gangX -= 1
                        GoTo lb10
                    End If
                Case Is > 1
                    If gangX < VEH.ganganz Then
                        gangX += 1
                        GoTo lb10
                    End If
            End Select

            'Save for Gear in Field for further checks
            Gcheck(t - tx) = gangX

            If gangX <> Gear Then itgangwL = t

            Gear = gangX

            t += 1

        Loop Until t = tx + 11 Or t > t1
        t = tx
        '-------------------------------------------------------------
        'Gear accepted
        gangX = Gcheck(0)
        For ix = t To t + 10
            Gears(ix - t) = Gcheck(ix - t)
        Next

        Gear = Gears(0)

        'Gang-Verlauf hinzufügen |@@| Add to Gears-sequence

        '----------------------------------------------------------------------------------

        '--------------------------------Checks Part 1 -------------------------------------
        'Checks to Purge non-sensible Gear-shift:

        ''Division into "iphase(j)" stages: Acceleration(=1), Deceleration(=2) and Cruise(=3):
        'iphase = 0
        'Select Case (beschl(jz - 2) + beschl(jz - 1) + beschl(jz)) / 3
        '    Case Is >= 0.125
        '        iphase = 1
        '    Case Is <= -0.125
        '        iphase = 2
        '    Case Else
        '        iphase = 3
        'End Select
        '   ============>> Already determined by VehState0

        'Search by last Gear-change
        itgangwL = -1
        If t > 2 Then
            For ix = t - 1 To 1 Step -1
                If MODdata.Gear(ix) <> MODdata.Gear(ix - 1) Then
                    itgangwL = ix
                    Exit For
                End If
            Next
        End If

        'Maximum permissible Gear-shifts every 3 seconds:
        If t - itgangwL <= 2 And t > 2 And LastGear <> 0 Then
            Return LastGear    '<<< keine weiteren Checks!!!
        End If

        If Gear <> LastGear Then
            'Cruise-phases:
            'Do not change Gear for as long Speed-change since last Gear-shift is below 6% and Pe/Pnorm change is below 6%:
            'Deceleration-phases: Upshift is suppressed
            'Acceleration-phases: Downshift?(Zurückschalten) suppressed
            If itgangwL = -1 Then itgangwL = 0
            bCheck = False
            Pjetzt = fPeGearModvD(t)
            Pvorher = MODdata.Pe(itgangwL)
            If MODdata.Vh.V(itgangwL) = 0 Then
                a = Math.Abs(Vist / 0.0001 - 1)
            Else
                a = Math.Abs(Vist / MODdata.Vh.V(itgangwL) - 1)
            End If
            If Pvorher = 0 Then
                b = Math.Abs(Pjetzt / 0.0001 - 1)
            Else
                b = Math.Abs(Pjetzt / Pvorher - 1)
            End If
            If VehState0 = tVehState.Cruise And a < 0.06 And b < 0.06 Then bCheck = True
            If (VehState0 = tVehState.Acc) And Gear < LastGear Then bCheck = True
            If (VehState0 = tVehState.Dec) And Gear > LastGear And LastGear <> 0 Then bCheck = True
            If bCheck Then
                Gear = LastGear
            Else
                'If within 6 seconds it Shifts back to the previous-Gear,
                'then maintain the previous-Gear throughout.
                bCheck = False
                For ix = t + 1 To t + 6
                    If ix > t1 Then Exit For
                    If (Gears(ix - t) = LastGear) Then
                        bCheck = True
                        Exit For
                    End If
                Next
                If bCheck Then
                    Gear = LastGear
                Else
                    'If within 6 seconds it Shifts once above and once below the previous-Gear,
                    'then maintain the previous-Gear throughout.
                    a = 0
                    b = 0
                    For ix = t To t + 6
                        If Gears(ix - t) > LastGear Then
                            a = 1
                        ElseIf Gears(ix - t) < LastGear Then
                            b = 1
                        End If
                    Next
                    If a * b > 0 Then
                        Gear = LastGear
                    End If
                End If
            End If
        End If

        '--------------------------------Checks Part 2 -------------------------------------
        'Gear-shift from 2 to 1 are suppressed when v > 2.5 m/s
        'NEU LUZ 040210: Hochschalten nur wenn im 2. Gang über Kuppeldrehzahl |@@| NEW LUZ 040210: Upshifting only when in 2nd Gear over Cluch-Revolutions
        If Gear = 1 And LastGear > 1 And Vist >= 2.5 Then
            If fnn(Vist, 2, False) > Kuppln_norm Then Gear = 2
        End If

        'bei verzoegerungsvorgaengen unter 2,5 m/s wird in Leerlauf geschaltet |@@| at decelerations below 2.5 m/s, shift to idle
        bCheck = True
        For ix = t To t + 2
            If ix > t1 Then Exit For
            If MODdata.Vh.V(ix) > MODdata.Vh.V(ix - 1) Then
                bCheck = False
                Exit For
            End If
        Next

        If bCheck And Vist < 2.5 And VehState0 = tVehState.Dec Then
            Return 0  '<<< keine weiteren Checks!!!
        End If

        'If v <0.1 m/s for more than 1 sec then shift to Gear=0
        If t < t1 Then
            If (Vist < 0.1 And MODdata.Vh.V(t + 1) < 0.1) Then
                Return 0    '<<< keine weiteren Checks!!!
            End If
        End If

        'at Acceleration processes below 1.5 m/s, then shift to 1st Gear
        If Vist < 1.5 And t < t1 Then
            If (Vist > 0.01 + MODdata.Vh.V(t - 1) And MODdata.Vh.V(t + 1) > 0.01 + Vist) Then
                Gear = 1
            End If
        End If

        'checking if Revolutions above Nominal-Revolutions, then always Upshift
        'otherwise lack the power!
        Do While fnn(Vist, Gear, Clutch = tEngClutch.Slipping) > 1 And Gear < VEH.ganganz
            Gear += 1
        Loop

        Return Gear

    End Function

    'EV-Gear-shifting model (based on Cars(PKW))
    Private Function fGearEV(ByVal t As Integer) As Integer

        Dim gangX As Int16
        Dim Gear As Integer
        Dim ix As Integer
        Dim nn As Single
        Dim tx As Int32
        Dim t1 As Int32
        Dim V_norm As Single
        Dim jpm As Integer
        Dim AP10 As Single
        Dim Pjetzt As Single
        Dim Pvorher As Single
        Dim Pjzx As Single
        Dim nnsaufi As Single
        Dim nnsobi As Single
        Dim nsa As Single
        Dim nsd As Single
        Dim itgangwL As Integer
        Dim bCheck As Boolean
        Dim Gcheck(10) As Short
        Dim Gears(11) As Integer      '<<<<==== !!!!!!
        Dim LastGear As Integer
        Dim a As Single
        Dim b As Single
        Dim Valt As Single
        Dim nx As Single
        Dim Vist0 As Single

        '-----------------------------------Second 1 --------------------------------------
        'First second: Find Gear / initialization
        If t = 0 Then
            gangX = -1
            If (Vist <= 1.5) Then
                Gear = 1
            Else
                For ix = 1 To VEH.ganganz
                    nn = fnn(Vist, ix, False)

                    If (ix < VEH.ganganz) Then
                        If (nn <= 0.75) Then
                            gangX = ix
                            Exit For
                        End If
                    Else
                        If (nn <= 1.0) Then
                            gangX = ix
                            Exit For
                        End If
                    End If
                Next ix
                If ix > VEH.ganganz Then gangX = VEH.ganganz
                Gear = gangX
            End If
            Return Gear
        End If

        '--------------------------------From second 2 --------------------------------------

        '---------Start-values ---------
        'gangX = Last Gear ie Starting-base for Shifting-model
        LastGear = MODdata.Gear(t - 1)
        gangX = LastGear
        Gear = LastGear
        t1 = MODdata.tDim
        itgangwL = -1

        'Clutch-lock check << already happened in Power.Calc
        ''If bPplus And fn_norm(1) < Kuppln_norm Then bKupplSchleif = True

        '-------------------Calculate Gear for the next 6 seconds ---------------------
        '-------------------------------------------------------------

        Pvorher = MODdata.Pe(t - 1)

        tx = t
        Do
            '-----------Shifting-function ----------
            Vist0 = MODdata.Vh.V(t)
            V_norm = Vist0 / GVmax
            nx = fnU(Vist0, gangX, False) / VEH.nNenn
            Valt = MODdata.Vh.V(t - 1)

            If (t >= 6) Then
                jpm = 5
            Else
                jpm = t - 1
            End If
            AP10 = 0

            t = t - jpm
            ix = 1
            Do
                If t < tx Then
                    Pjetzt = MODdata.Pe(t)
                Else
                    Pjetzt = fPeGearEV(gangX, t)
                    If t = tx Then Pjzx = Pjetzt
                End If

                If t = t - ix + jpm Then
                    Pvorher = Pjetzt
                End If

                'Gelöscht LUZ 13.07.10: If (Pjetzt > 1) Then Pjetzt = 1.0
                'If ix = jpm Then Pvorher = Pjetzt



                AP10 = AP10 + Pjetzt / (jpm + 6)
                t += 1
                ix += 1
            Loop Until ix = 11 Or t > t1
            t = t - ix + jpm + 1

            'Revolutions-limit for Upshift, n_normiert (Idle = 0, Nominal-Revolutions = 1)
            nnsaufi = Aaufi + Baufi * V_norm + Caufi * AP10
            If (nnsaufi > 0.95) Then nnsaufi = 0.95
            'Revolutions-limit for Downshift, n_normiert (Idle = 0, Nominal-Revolutions = 1)
            nnsobi = Aobi + Bobi * V_norm + Cobi * AP10
            'Gelöscht LUZ 13.07.10: If (nnsaufi > 0.85) Then nnsaufi = 0.85
            'Convert here of Revolutions units to use (n/n_nominal):
            nsa = (VEH.nLeerl / VEH.nNenn) + nnsaufi * (1 - (VEH.nLeerl / VEH.nNenn))
            nsd = (VEH.nLeerl / VEH.nNenn) + nnsobi * (1 - (VEH.nLeerl / VEH.nNenn))
            'Revolutions with last Gear (gangX)
            'nx = fnU(Vist, gangX, Clutch = tEngClutch.Slipping) / VEH.nNenn
            '-----------------------------------

            ' ''Maximum permissible Gear-shifting every 2 seconds:
            If (t - itgangwL) < 3 And itgangwL > -1 Then GoTo lb10

            'Check whether Downshifting-gear, only when Revolutions decrease or Power increases
            bCheck = False
            Pjetzt = fPeGearEV(gangX, t)
            If Vist0 < Valt Then bCheck = True
            If (Pjetzt > Pvorher) Then bCheck = True
            If bCheck Then
                If nx < nsd Then gangX -= 1
            End If

            'Check whether Upshifting-gear, only when Revolutions increase or Power decreases
            bCheck = False
            If (Vist0 > Valt) Then bCheck = True
            If (Pjetzt < Pvorher) Then bCheck = True
            If bCheck Then
                If nx > nsa Then gangX += 1
            End If

            'Correct Gear-selection
            If gangX > VEH.ganganz Then
                gangX = VEH.ganganz
            ElseIf gangX < 1 Then
                gangX = 1
            End If

lb10:

            'Not idle when Power > 0
            If gangX = 0 Then
                If Pjzx > 0.001 Then gangX = 1
            End If

            'New Revolutions
            ''nn = fnn(Vist, gangX, Clutch = tEngClutch.Slipping)
            nn = fnn(Vist0, gangX, False)

            'Check if Gear within Power/Revolutions limits. Drag-operation is not respected
            Select Case nn
                Case Is < Kuppln_norm
                    If gangX > 1 Then
                        gangX -= 1
                        GoTo lb10
                    End If
                Case Is > 1
                    If gangX < VEH.ganganz Then
                        gangX += 1
                        GoTo lb10
                    End If
            End Select

            'Save Gears in field for later checks
            Gcheck(t - tx) = gangX

            If gangX <> Gear Then itgangwL = t

            Gear = gangX

            t += 1

        Loop Until t = tx + 11 Or t > t1
        t = tx
        '-------------------------------------------------------------
        'Accept Gear
        gangX = Gcheck(0)
        For ix = t To t + 10
            Gears(ix - t) = Gcheck(ix - t)
        Next

        Gear = Gears(0)

        'Gang-Verlauf hinzufügen |@@| Add to Gang-sequence

        '----------------------------------------------------------------------------------

        '--------------------------------Checks Part 1 -------------------------------------
        'Checks to Purge non-sensible Gear-shift:

        ''Division into "IPhase(j)" stages: acceleration(=1), Deceleration(=2) and Cruise(=3):
        'iphase = 0
        'Select Case (beschl(jz - 2) + beschl(jz - 1) + beschl(jz)) / 3
        '    Case Is >= 0.125
        '        iphase = 1
        '    Case Is <= -0.125
        '        iphase = 2
        '    Case Else
        '        iphase = 3
        'End Select
        '   ============>> Already determined by VehState0

        'Search by last Gear-change
        itgangwL = -1
        If t > 2 Then
            For ix = t - 1 To 1 Step -1
                If MODdata.Gear(ix) <> MODdata.Gear(ix - 1) Then
                    itgangwL = ix
                    Exit For
                End If
            Next
        End If

        'Max permissible Gear-change every 3 seconds:
        If t - itgangwL <= 2 And t > 2 And LastGear <> 0 Then
            Return LastGear    '<<< keine weiteren Checks!!!
        End If

        If Gear <> LastGear Then
            'Cruise-phases:
            'Solange Geschwindigkeitsaenderung seit letztem Gangwechsel unter 6% und Pe/Pnenn aenderung unter 6% wird nicht geschaltet: |@@| As long Speed-change since last Gear-shift is under 6% and Pe/Pnom below 6%, do not run:
            'Deceleration phases: Upshift suppressed
            'Acceleration phases: Downshift?(Zurückschalten) suppressed
            If itgangwL = -1 Then itgangwL = 0
            bCheck = False
            Pjetzt = fPeGearEV(Gear, t)
            Pvorher = MODdata.Pe(itgangwL)
            If MODdata.Vh.V(itgangwL) = 0 Then
                a = Math.Abs(Vist / 0.0001 - 1)
            Else
                a = Math.Abs(Vist / MODdata.Vh.V(itgangwL) - 1)
            End If
            If Pvorher = 0 Then
                b = Math.Abs(Pjetzt / 0.0001 - 1)
            Else
                b = Math.Abs(Pjetzt / Pvorher - 1)
            End If
            If VehState0 = tVehState.Cruise And a < 0.06 And b < 0.06 Then bCheck = True
            If (VehState0 = tVehState.Acc) And Gear < LastGear Then bCheck = True
            If (VehState0 = tVehState.Dec) And Gear > LastGear And LastGear <> 0 Then bCheck = True
            If bCheck Then
                Gear = LastGear
            Else
                'Wenn innerhalb von 6 Sekunden wieder in vorigen Gang zurueck geschaltet wird, wird der vorige Gang |@@| If within 6 seconds switched back again to the previous Gear, stick
                'durchgehend beibehalten |@@| to the previous Gear
                bCheck = False
                For ix = t + 1 To t + 6
                    If ix > t1 Then Exit For
                    If (Gears(ix - t) = LastGear) Then
                        bCheck = True
                        Exit For
                    End If
                Next
                If bCheck Then
                    Gear = LastGear
                Else
                    'Wenn innerhalb von 6 Sekunden einmal höher und einmal niedriger als voriger Gang |@@| If within 6 seconds it Shifts once above and once below the previous-Gear,
                    'geschaltet wird, wird voriger Gang durchgehend beibehalten |@@| then maintain the previous-Gear throughout.
                    a = 0
                    b = 0
                    For ix = t To t + 6
                        If Gears(ix - t) > LastGear Then
                            a = 1
                        ElseIf Gears(ix - t) < LastGear Then
                            b = 1
                        End If
                    Next
                    If a * b > 0 Then
                        Gear = LastGear
                    End If
                End If
            End If
        End If

        '--------------------------------Checks Part 2 -------------------------------------
        'Suppress Gear-shift from 2 to 1 when v > 2.5 m/s
        'NEU LUZ 040210: Hochschalten nur wenn im 2. Gang über Kuppeldrehzahl |@@| NEW LUZ 040210: Upshift only when in 2 Gear over Clutch-revolutions
        If Gear = 1 And LastGear > 1 And Vist >= 2.5 Then
            If fnn(Vist, 2, False) > Kuppln_norm Then Gear = 2
        End If

        'bei verzoegerungsvorgaengen unter 2,5 m/s wird in Leerlauf geschaltet |@@| at decelerations below 2.5 m/s, shift to Idle
        bCheck = True
        For ix = t To t + 2
            If ix > t1 Then Exit For
            If MODdata.Vh.V(ix) > MODdata.Vh.V(ix - 1) Then
                bCheck = False
                Exit For
            End If
        Next

        If bCheck And Vist < 2.5 And VehState0 = tVehState.Dec Then
            Return 0  '<<< keine weiteren Checks!!!
        End If

        'wenn v mehr als 1 Sek. < 0.1 m/s wird auf Gang=0 geschaltet |@@| If v < 0.1 m/s for more than 1 sec, then shift to Gear=0
        If t < t1 Then
            If (Vist < 0.1 And MODdata.Vh.V(t + 1) < 0.1) Then
                Return 0    '<<< keine weiteren Checks!!!
            End If
        End If

        'at Acceleration below 1.5 m/s, then shift to 1st Gear
        If Vist < 1.5 And t < t1 Then
            If (Vist > 0.01 + MODdata.Vh.V(t - 1) And MODdata.Vh.V(t + 1) > 0.01 + Vist) Then
                Gear = 1
            End If
        End If

        'Check whether Revolutions over Nominal-Revolutions, then should always Upshift,
        'otherwise Power not enough!
        Do While fnn(Vist, Gear, Clutch = tEngClutch.Slipping) > 1 And Gear < VEH.ganganz
            Gear += 1
        Loop

        Return Gear

    End Function

    Private Function fGearLKW(ByVal t As Integer) As Integer
        Dim gangX As Int16
        Dim GangXl As Int16
        Dim Gear As Integer
        Dim ix As Integer
        Dim nn As Single
        Dim tx As Int32
        Dim t1 As Int32
        Dim Pjetzt As Single
        Dim Pvorher As Single
        Dim itgangwL As Integer
        Dim itgangwH As Int32
        Dim bCheck As Boolean
        Dim Gears(11) As Integer      '<<<<==== !!!!!!
        Dim LastGear As Integer
        Dim a As Single
        Dim b As Single
        Dim Pe0 As Single
        Dim PeX As Single
        Dim achek As Integer
        'Dim avchek As Single
        Dim achstg As Single
        Dim cnl As Single
        Dim n0 As Single
        Dim P_maxg As Single
        Dim pschnellm As Single
        Dim psparm As Single
        Dim psparist As Single
        Dim pschist As Single
        Dim ClutchSlip As Boolean
        Dim P_maxi(16) As Single
        Dim maxPgang As Int16
        Dim Pivoll As Single
        Dim iphase As Int16
        Dim iphase0 As Int16
        Dim Vist0 As Single
        Dim LastPe As Single
        Dim OverPfull As Boolean = False
        Dim MsgSrc As String

        MsgSrc = "Power/HDV_Gear"

        '-----------------------------------Second 1 --------------------------------------
        'First second: Find Gear/Initialization
        If t = 0 Then
            gangX = -1
            If (MODdata.Vh.V(t) <= 1.5) Then
                Gear = 1
            Else
                For ix = 1 To VEH.ganganz
                    nn = fnn(Vist, ix, False)

                    If (ix < VEH.ganganz) Then
                        If (nn <= 0.8) Then
                            gangX = ix
                            Exit For
                        End If
                    Else
                        If (nn <= 1.06) Then
                            gangX = ix
                            Exit For
                        End If
                    End If
                Next ix
                If gangX = -1 Then
                    WorkerMsg(tMsgID.Err, "Failed to calculate intial gear!", MsgSrc)
                    Return -1
                End If
                Gear = gangX
            End If
            GangH(t) = Gear
            GangL(t) = Gear
            Return Gear

        Else

            LastPe = MODdata.Pe(t - 1)

        End If

        '--------------------------------From second 2 --------------------------------------

        '---------Start-values ---------
        gangX = MODdata.Gear(t - 1)
        LastGear = gangX
        GangXl = GangL(t - 1)

        ClutchSlip = (Clutch = tEngClutch.Slipping)

        t1 = MODdata.tDim

        itgangwH = 0
        itgangwL = 0

        If t > 1 Then
            For ix = t - 1 To 1 Step -1
                If GangH(ix) <> GangH(ix - 1) Then
                    itgangwH = ix
                    Exit For
                End If
            Next

            For ix = t - 1 To 1 Step -1
                If GangL(ix) <> GangL(ix - 1) Then
                    itgangwL = ix
                    Exit For
                End If
            Next
        End If



        'Compute power from jz to (jz + 6) -----------------
        tx = t
        For t = tx To tx + 6
            If t > t1 Then Exit For

            Vist0 = MODdata.Vh.V(t)

            Pe0 = fPeGearModvD(t)

            n0 = fnU(MODdata.Vh.V(t), gangX, False) / VEH.nNenn

            '-----------------------------------------------------------------
            '
            '     Berechnung nach Drehzahl/Leistung-Modell |@@| Calculated towards a Revolutions/Power model
            '
            '    --------------------------------------------------------------
            '(1) "Fast Driving" variant

            'Gear-shift only if v-change 5% since last Gear-shift
            'VECTO: Commented out START
            'achek = 1
            'If (MODdata.Vh.V(itgangwH) <> 0) Then
            '    avchek = Math.Abs(Vist0 / MODdata.Vh.V(itgangwH) - 1)
            'Else
            '    avchek = Math.Abs(Vist0 - MODdata.Vh.V(itgangwH))
            'End If
            'If (avchek >= 0.05) Then
            '    achek = -1
            'Else
            '    achek = 1
            'End If
            'VECTO: Commented out END

            'in ersten 10 Zyklussekunden kann zum Einregulieren immer geschlatet werden: |@@| the first 10 seconds of the cycle can always be used for balancing gear-shifting:
            If (t <= 9) Then achek = -1

            'A Change in the Slope can always result in Gear-shift:
            achstg = Math.Abs(MODdata.Vh.Grad(t) - MODdata.Vh.Grad(itgangwH))
            If (achstg > 0.001) Then achek = -1

            'Downshift:
            If (n0 <= VEH.hinunter) Then
                If (achek < 0.9) Then
                    gangX = gangX - 1
                End If
            End If

            'Upshift:
            ' bei Steigungszyklen mit zu hohen Geschwindigkeiten wird geschw. i+1 erst nach gangwahl berechnet |@@| at Sloped-cycles with excessive speed the Gear i +1 is calculated
            ' dabei manchmal zu hoher gang -> Drehzahl und P_max viel zu nieder, daher nu bei niederen leistungen |@@| sometimes Gear is too high -> Revolutions and P_max too low, so only at low Power
            ' hochschalten erlaubt: |@@| Upshift allowed:
            If (Pe0 > 0.8) Then
                If (n0 < 0.9) Then
                    achek = 1
                End If
            End If

            If (n0 >= VEH.hinauf) Then
                If (achek < 0.9) Then
                    gangX = gangX + 1
                End If
            End If

            If (gangX > VEH.ganganz) Then gangX = VEH.ganganz
            If (gangX < 1) Then gangX = 1

            GangH(t) = gangX

            '   -----------------------------------------------------------------
            '(2) "Economical Driving" Variant

            '   Downshift?(Zurueckschalten) happens only when Speed-change > 6%
            '   Always Upshift
            'VECTO: Commented out START
            'achek = 1
            'If (MODdata.Vh.V(itgangwL) <> 0) Then
            '    avchek = Math.Abs(Vist0 / MODdata.Vh.V(itgangwL) - 1)
            'Else
            '    avchek = Math.Abs(Vist0 - MODdata.Vh.V(itgangwL))
            'End If
            'If (avchek >= 0.06) Then
            '    achek = -1
            'Else
            '    achek = 1
            'End If
            'VECTO: Commented out END

            '       in ersten 10 Zyklussekunden kann zum einregulieren immer geschlatet werden: |@@| The first 10 seconds cycle can always be used for balancing Gear-shift:
            If (t <= 9) Then achek = -1
            '       When slope changes always may result in Gear-shift:
            achstg = Math.Abs(MODdata.Vh.Grad(t) - MODdata.Vh.Grad(itgangwL))
            If (achstg > 0.001) Then achek = -1

            '    Downshift:
            If (GangXl > 1) Then
                cnl = fnU(Vist0, GangXl, False) / VEH.nNenn
                If (cnl <= VEH.lhinunter) Then
                    If (achek < 0.9) Then
                        GangXl = GangXl - 1
                    End If
                End If
            End If

            'Upshift, only if checked not the highest Gear:
            If (GangXl < VEH.ganganz) Then
                'C
                If (Pe0 > 0.8) Then
                    If (n0 < 0.85) Then
                        achek = 1.0
                    End If
                End If
                'C
                'C     Relative Revolutions:
                cnl = fnU(Vist0, GangXl + 1, False) / VEH.nNenn


                If (cnl >= VEH.lhinauf) Then
                    GangXl = GangXl + 1
                End If
            End If

            GangL(t) = GangXl

            ' Auswahl des Drehzahlverhaeltnisses aus der "schnellen (..h)" und |@@| Select Revolutions-relationship for the "fast (h ..)" and
            ' der "sparsamen (..l)" Variante: |@@| the "economical (.. l)"  Variant:

            '   Drehzahlverhhealtnisse nach "Modellmix": |@@| Revolutions-relationship for "Modelmix":
            '   anhand der erforderlichen maximalen Motorleistung ueber die |@@| according to the required maximum Engine-power over the
            '   next 6 seconds
            P_maxg = Pe0
            For ix = t To t + 5
                If ix > t1 Then Exit For
                PeX = fPeGearModvD(t)
                If (PeX > P_maxg) Then P_maxg = PeX
            Next

            '     Determine the proportions between the Fast and the Economical Driving-style
            '      (Hausberger model):
            pschnellm = 3.3333 * P_maxg - 1.6667

            If (pschnellm > 1) Then pschnellm = 1
            If (pschnellm < 0) Then pschnellm = 0
            psparm = 1.0 - pschnellm

            '     Mix der berechneten Gaenge gemaess Vorgabe in Eingabefile: |@@| Mix the calculated Gears as specified in the input file:
            '     (pmodell wird aus Eingabefile gelesen, = Anteil, zu der die Drehzahl |@@| from the Input-file it is read the pmodell = ratios of the revolutions
            '      nach "reales Modell" bestehen soll) |@@| towards a "real model")

            psparist = VEH.pspar + psparm * VEH.pmodell
            pschist = 1.0 - psparist

            '      Ermittlung des "virtuellen" aktuellen Ganges nach Modell |@@| Determine the "virtual" up-to-date Gears from the Model
            Gear = Math.Round((pschist) * GangH(t) + (psparist) * GangL(t), 0, MidpointRounding.AwayFromZero)
            If (Gear > VEH.ganganz) Then Gear = VEH.ganganz
            If (Gear < 1) Then Gear = 1

            '    check if Revolutions over Nominal-Revolutions, then must always upshift,
            '    otherwise Power not enough!
lb88:
            n0 = fnU(Vist0, Gear, False) / VEH.nNenn
            If (n0 > 1) Then
                If (Gear < VEH.ganganz) Then
                    Gear = Gear + 1
                    GoTo lb88
                End If
            End If

            '    Check whether required Power is over P_max (s)
            '    then Downshift?(zurueckgeschaltet):

            For ix = 1 To VEH.ganganz
                n0 = fnU(Vist0, ix, False) / VEH.nNenn
                nn = fnn(Vist0, ix, False)
                If (n0 > 0.2 And n0 < 1.02) Then
                    P_maxi(ix) = FLD.Pfull(nn, LastPe) / VEH.Pnenn
                Else
                    P_maxi(ix) = 0
                End If
            Next

            P_maxg = 0
            For ix = 1 To VEH.ganganz
                If (P_maxi(ix) > P_maxg) Then
                    P_maxg = P_maxi(ix)
                    maxPgang = ix
                End If
            Next

            For ix = maxPgang To VEH.ganganz
                If (P_maxi(ix) >= (0.94 * P_maxg)) Then
                    maxPgang = ix
                End If
            Next

            '    Eigentliche Ueberpruefung ob ueber P_max(n) |@@| Check whether Actual over P_max (s)

lb909:

            Pivoll = FLD.Pfull(fnn(MODdata.Vh.V(t), Gear, ClutchSlip), LastPe) / VEH.Pnenn

            'falls schlechte Vollastkurve ohne Moment in leerlauf wird korrigiert: |@@| if bad Full-load-curve without Torque, then correct in Idle:
            If (Pivoll < 0.06) Then Pivoll = 0.06

            '    Ueberpruefung, ob hoehere Leistung erforderlich ist als Maximalleistung bei nh |@@| Checking whether required Power is higher than maximum power at nh
            '     dann wird zurueckgeschaltet: |@@| then Gear-shift-back?(zurueckgeschaltet):
            If (Pe0 > Pivoll) Then

                If t = tx Then OverPfull = True

                If (Gear > maxPgang) Then
                    cnl = fnU(Vist0, Gear - 1, False) / VEH.nNenn
                    If (cnl <= 1.02) Then
                        gangX = gangX - 1
                        GangXl = GangXl - 1
                        Gear = Gear - 1
                        GoTo lb909
                    End If
                End If
            End If

            If (gangX > VEH.ganganz) Then gangX = VEH.ganganz
            If (gangX < 1) Then gangX = 1
            If (GangXl > VEH.ganganz) Then GangXl = VEH.ganganz
            If (GangXl < 1) Then GangXl = 1
            If (Gear > VEH.ganganz) Then Gear = VEH.ganganz
            If (Gear < 1) Then Gear = 1

            Gears(t - tx) = Gear

        Next
        t = tx

        Gear = Gears(0)

        ''Pjetzt = fPantr(Gang(jz)) / (fGPvoll(fn(Gang(jz))) + 0.00000001)

        ''If Pjetzt > 1 Then
        ''    GeschwRed = True
        ''    GeschwRedPC = Pjetzt
        ''    Exit Function
        ''End If

        'c     Ende "Modell"-Basisgangwahl |@@| End "model"-Gear-selection basis
        '---------------------------------------------------


        'Kuppelschleif-check |@@| Clutch-lock check
        ''If bPplus And fn_norm(1) < Kuppln_norm Then bKupplSchleif = True

        '--------------------------------Checks Part 1 -------------------------------------
        'Checks to Purge non-sensible Gear-shift:
        'Division into "IPhase(j)" stages: Acceleration(=1), Deceleration(=2) and Cruise(=3):
        iphase = 0
        If t > 1 Then
            Select Case (MODdata.Vh.a(t - 2) + MODdata.Vh.a(t - 1) + MODdata.Vh.a(t)) / 3
                Case Is >= 0.125
                    iphase = 1
                Case Is <= -0.125
                    iphase = 2
                Case Else
                    iphase = 3
            End Select
        End If

        iphase0 = 0
        If t > 3 Then
            Select Case (MODdata.Vh.a(t - 3) + MODdata.Vh.a(t - 2) + MODdata.Vh.a(t - 1)) / 3
                Case Is >= 0.125
                    iphase0 = 1
                Case Is <= -0.125
                    iphase0 = 2
                Case Else
                    iphase0 = 3
            End Select
        End If

        'Search by last Gear-change
        itgangwL = 0
        For ix = t - 1 To 1 Step -1
            If MODdata.Gear(ix) <> MODdata.Gear(ix - 1) Then
                itgangwL = ix
                Exit For
            End If
        Next

        'Maximum permissible Gear-shifts every 3 seconds:
        If t - itgangwL <= 3 And t > 2 Then
            Return LastGear    '<<< keine weiteren Checks!!!
        End If

        'Cruise-phases:
        'As long Speed-change since last Gear-shift is below 6% and Pe/Pnom below 6% then do not Gear-shift:
        'Deceleration-phases: Upshift suppressed
        'Acceleration phases: Downshift?(Zurückschalten) suppressed
        bCheck = False
        Pjetzt = fPeGearModvD(t)
        Pvorher = MODdata.Pe(itgangwL)
        If MODdata.Vh.V(itgangwL) = 0 Then
            a = Math.Abs(Vist / 0.0001 - 1)
        Else
            a = Math.Abs(Vist / MODdata.Vh.V(itgangwL) - 1)
        End If
        If Pvorher = 0 Then
            b = Math.Abs(Pjetzt / 0.0001 - 1)
        Else
            b = Math.Abs(Pjetzt / Pvorher - 1)
        End If
        If iphase = 3 And a < 0.06 And b < 0.06 Then bCheck = True
        If (iphase = 1) And Gear < MODdata.Gear(t - 1) And iphase0 = 1 Then bCheck = True
        If (iphase = 2) And Gear > MODdata.Gear(t - 1) Then bCheck = True
        If bCheck Then Gear = LastGear

        'If within 6 seconds switched back again to the previous Gear, then
        'stick to previous Gear
        'VECTO: Exception: on Full-load curve
        If Not OverPfull Then
            bCheck = False
            For ix = t + 1 To t + 6
                If ix > t1 Then Exit For
                If (Gears(ix - t) = LastGear) Then bCheck = True
            Next
            If bCheck Then Gear = LastGear
        End If


        'If within the 6 seconds, it shifts once to higher and once to lower-Gear than the previous one, then
        'stick to the previous Gear.
        a = 0
        b = 0
        If (Gear <> LastGear) Then
            For ix = t To t + 6
                If Gears(ix - t) > LastGear Then
                    a = 1
                ElseIf Gears(ix - t) < LastGear Then
                    b = 1
                End If
            Next
            If a * b > 0 Then
                Gear = LastGear
            End If
        End If

        '--------------------------------Checks Part 2 -------------------------------------
        'Shifting from 2nd to 1st Gear is suppressed when v > 1.5 m/s
        'NEU LUZ 040210: Hochschalten nur wenn im 2. Gang über Kuppeldrehzahl |@@| NEW LUZ 040210: Upshifting only when in 2nd Gear over the Clutch-revolutions
        If Gear = 1 And LastGear > 1 And Vist >= 1.5 Then
            If fnn(Vist, 2, False) > Kuppln_norm Then Gear = 2
        End If

        'bei verzoegerungsvorgaengen unter 1.5 m/s wird in Leerlauf geschaltet |@@| at decelerations below 1.5 m/s, shift to Idle
        bCheck = False
        For ix = t To t + 2
            If ix > t1 Then Exit For
            If MODdata.Vh.V(ix) > MODdata.Vh.V(ix - 1) Then GoTo lb20
        Next
        bCheck = True
lb20:
        If bCheck And Vist < 1.5 And VehState0 = tVehState.Dec Then
            Return 0    '<<< keine weiteren Checks!!!
        End If

        'If v < 0.1 m/s for more than 1 sec, then shift to Gear=0
        If t < t1 Then
            If (Vist < 0.1 And MODdata.Vh.V(t + 1) < 0.1) Then
                Return 0    '<<< keine weiteren Checks!!!
            End If
        End If


        'Check if Revolutions over Nominal-revolutions, then should always Upshift,
        'otherwise Power not enough!
        Do While fnn(Vist, Gear, ClutchSlip) > 1 And Gear < VEH.ganganz
            Gear += 1
        Loop

        Return Gear

    End Function

    Private Function fGearBySpeed(ByVal t As Integer) As Integer
        Dim i As Int16
        Dim g0 As Int16
        Dim g As Int16
        Dim v0 As Single
        Dim v1 As Single
        Dim v2 As Single
        Dim v As Single

        v = MODdata.Vh.V(t)

        If t = 0 Then
            v0 = v
            g0 = VEH.ganganz
            For i = 1 To VEH.ganganz
                If v < avh(i) Then
                    g0 = i
                End If
            Next
        Else
            v0 = MODdata.Vh.V(t - 1)
            g0 = MODdata.Vh.GearVorg(t - 1)
            g = g0
            If g = -1 Then g = 1 '<= Sonst Absturz bei g=-1. 
            If v > v0 Then
                If v >= avh(g) And g < VEH.ganganz Then g += 1
            ElseIf v < v0 Then
                If v <= avl(g) And g > 0 Then g -= 1
            End If
        End If

        'Speed look-ahead
        If t = MODdata.tDim Then
            v1 = v
            v2 = v
        Else
            v1 = MODdata.Vh.V(t + 1)
            If t + 1 = MODdata.tDim Then
                v2 = v
            Else
                v2 = MODdata.Vh.V(t + 2)
            End If
        End If

        'Checks Gears for Cars(PKW) ....
        'Schalten von 2. in 1. Gang wird bei v>2,5 m/s unterdrueckt |@@| Gear-shifting from 2nd to 1st is suppressed at v > 2.5 m/s
        If (g = 1) Then
            If (g0 > g) Then
                If (v >= 2.5) Then
                    g = 2
                End If
            End If
        End If

        'Bei verzoegerungsvorgaengen unter 2,5 m/s wird in Leerlauf geschaltet |@@| At decelerations below 2.5 m/s, shift to Idle
        If (v < 2.5) Then
            If (v <= v0 And v1 <= v) Then
                If (v2 <= v1) Then
                    g = 0
                End If
            End If
        End If

        'If v < 0.1 m/s for  more than 1 sec, then shift to Gear=0
        If (v < 0.1 And v1 < 0.1) Then
            g = 0
        End If

        'When Acceleration below 1.5 m/s, then shift to 1st Gear
        If (v < 1.5) Then
            If v > v0 And v1 > v Then
                g = 1
            End If
        End If

        Return g

    End Function

    Private Function fGearByU(ByVal nU As Single, ByVal V As Single) As Integer
        Dim Dif As Single
        Dim DifMin As Single
        Dim g As Int16
        Dim g0 As Integer
        DifMin = 9999
        For g = 1 To VEH.ganganz
            Dif = Math.Abs(VEH.Igetr(g) - nU * (VEH.Dreifen * Math.PI) / (V * 60.0 * VEH.AchsI))
            If Dif <= DifMin Then
                g0 = g
                DifMin = Dif
            End If
        Next
        Return g0
    End Function

    'Function calculating the Power easily for Gear-shift-model
    Private Function fPeGearModvD(ByVal t As Integer) As Single
        Return fPvD(t) / VEH.Pnenn
    End Function

    Private Function fPeGearMod(ByVal Gear As Integer, ByVal t As Integer, ByVal V As Single, ByVal a As Single) As Single
        Dim PaM As Single
        Dim nU As Single
        Dim PvD As Single

        PvD = fPvD(t, V, a)

        nU = fnU(V, Gear, False)

        If Nvorg Then
            'Drehzahlvorgabe
            PaM = (VEH.I_mot * MODdata.dnUvorg(t) * 0.01096 * MODdata.nUvorg(t)) * 0.001
        Else
            PaM = ((VEH.I_mot * (VEH.AchsI * VEH.Igetr(Gear) / (0.5 * VEH.Dreifen)) ^ 2) * a * V) * 0.001
        End If
        If Clutch = tEngClutch.Closed Then
            Return (PvD + fPlossGB(PvD, V, Gear) + fPlossDiff(PvD, V) + fPaG(V, a) + fPaux(t, nU) + PaM) / VEH.Pnenn
        Else    'Clutch = tEngClutch.Slipping
            Return ((PvD + fPlossGB(PvD, V, Gear) + fPlossDiff(PvD, V) + fPaG(V, a)) / KupplEta + fPaux(t, nU) + PaM) / VEH.Pnenn
        End If


    End Function

    Private Function fPeGearMod(ByVal Gear As Integer, ByVal t As Integer) As Single
        Return fPeGearMod(Gear, t, MODdata.Vh.V(t), MODdata.Vh.a(t))
    End Function



    'Function calculating the Power easily for EV-shift-model
    Private Function fPeGearEV(ByVal Gear As Integer, ByVal t As Integer) As Single
        Dim PaM As Single
        Dim nU As Single
        Dim PvD As Single
        Dim V As Single
        Dim a As Single

        PvD = fPvD(t)
        V = MODdata.Vh.V(t)
        a = MODdata.Vh.a(t)

        nU = fnU(V, Gear, False)

        If Nvorg Then
            'Revolutions-setting
            PaM = (VEH.I_mot * MODdata.dnUvorg(t) * 0.01096 * MODdata.nUvorg(t)) * 0.001
        Else
            PaM = ((VEH.I_mot * (VEH.AchsI * VEH.Igetr(Gear) / (0.5 * VEH.Dreifen)) ^ 2) * a * V) * 0.001
        End If
        If Clutch = tEngClutch.Closed Then
            Return (PvD + fPlossGB(PvD, V, Gear) + fPlossDiff(PvD, V) + fPaG(V, a) + fPaux(t, nU) + PaM) / PeUL
        Else    'Clutch = tEngClutch.Slipping
            Return ((PvD + fPlossGB(PvD, V, Gear) + fPlossDiff(PvD, V) + fPaG(V, a)) / KupplEta + fPaux(t, nU) + PaM) / PeUL
        End If

    End Function





#End Region

#Region "Drehzahl"

    Private Function fnn(ByVal V As Single, ByVal Gear As Integer, ByVal ClutchSlip As Boolean) As Single
        Dim akn As Single
        Dim U As Single
        U = CSng(V * 60.0 * VEH.AchsI * VEH.Igetr(Gear) / (VEH.Dreifen * Math.PI))
        If U < VEH.nLeerl Then U = VEH.nLeerl
        If ClutchSlip Then
            akn = Kuppln_norm / ((VEH.nLeerl + Kuppln_norm * (VEH.nNenn - VEH.nLeerl)) / VEH.nNenn)
            U = (akn * U / VEH.nNenn) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl
        End If
        Return (U - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)
    End Function

    Private Function fnU(ByVal V As Single, ByVal Gear As Integer, ByVal ClutchSlip As Boolean) As Single
        Dim akn As Single
        Dim U As Single
        U = CSng(V * 60.0 * VEH.AchsI * VEH.Igetr(Gear) / (VEH.Dreifen * Math.PI))
        If U < VEH.nLeerl Then U = VEH.nLeerl
        If ClutchSlip Then
            akn = Kuppln_norm / ((VEH.nLeerl + Kuppln_norm * (VEH.nNenn - VEH.nLeerl)) / VEH.nNenn)
            U = (akn * U / VEH.nNenn) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl
        End If
        Return U
    End Function

    Private Function fnUout(ByVal V As Single, ByVal Gear As Integer) As Single
        Return CSng(V * 60.0 * VEH.AchsI * VEH.Igetr(Gear) / (VEH.Dreifen * Math.PI))
    End Function

#End Region

#Region "Leistungsberechnung"

    '--------------Power before Diff = At Wheel -------------
    Private Function fPvD(ByVal t As Integer) As Single
        Return fPr(MODdata.Vh.V(t)) + fPair(MODdata.Vh.V(t), t) + fPaFZ(MODdata.Vh.V(t), MODdata.Vh.a(t)) + fPs(MODdata.Vh.V(t), t)
    End Function

    Private Function fPvD(ByVal t As Integer, ByVal v As Single, ByVal a As Single) As Single
        Return fPr(v) + fPair(v, t) + fPaFZ(v, a) + fPs(v, t)
    End Function

    Private Function fPvD(ByVal t As Integer, ByVal v As Single, ByVal a As Single, ByVal Grad As Single) As Single
        Return fPr(v) + fPair(v, t) + fPaFZ(v, a) + fPs(v, Grad)
    End Function

    '----------------Rolling-resistance----------------
    Private Function fPr(ByVal v As Single) As Single
        Return CSng((VEH.Loading + VEH.Mass + VEH.MassExtra) * 9.81 * (VEH.Fr0 + VEH.Fr1 * v + VEH.Fr2 * v ^ 2 + VEH.Fr3 * v ^ 3 + VEH.Fr4 * v ^ 4) * v * 0.001)
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

        Return CSng((Cd * VEH.Aquers * Cfg.AirDensity / 2 * ((vair) ^ 2)) * v * 0.001)

    End Function

    '--------Vehicle Acceleration-capability(Beschleunigungsleistung) --------
    Private Function fPaFZ(ByVal v As Single, ByVal a As Single) As Single
        'Previously (PHEM 10.4.2 and older) the m_raeder was used for Massered instead, with Massered = m_raeder + I_Getriebe * (Iachs / (0.5 * Dreifen)) ^ 2
        '   The missing part (I_Getriebe * (Iachs / (0.5 * Dreifen)) ^ 2) is now considered by fPaG(V,a)
        Return CSng(((VEH.Mass + VEH.MassExtra + VEH.m_raeder_red + VEH.Loading) * a * v) * 0.001)
    End Function

    Private Function fPaMot(ByVal t As Integer, ByVal Gear As Integer) As Single
        If Nvorg Then
            Return (VEH.I_mot * MODdata.dnUvorg(t) * 0.01096 * MODdata.nUvorg(t)) * 0.001
        Else
            Return ((VEH.I_mot * (VEH.AchsI * VEH.Igetr(Gear) / (0.5 * VEH.Dreifen)) ^ 2) * aist * Vist) * 0.001
        End If
    End Function

    Private Function fPaMot(ByVal t As Integer, ByVal Gear As Integer, ByVal v As Single, ByVal a As Single) As Single
        If Nvorg Then
            Return (VEH.I_mot * MODdata.dnUvorg(t) * 0.01096 * MODdata.nUvorg(t)) * 0.001
        Else
            Return ((VEH.I_mot * (VEH.AchsI * VEH.Igetr(Gear) / (0.5 * VEH.Dreifen)) ^ 2) * a * v) * 0.001
        End If
    End Function

    '----------------Slope resistance ----------------
    Private Function fPs(ByVal v As Single, ByVal t As Integer) As Single
        Return CSng(((VEH.Loading + VEH.Mass + VEH.MassExtra) * 9.81 * MODdata.Vh.Grad(t) * 0.01 * v) * 0.001)
    End Function

    Private Function fPs(ByVal v As Single, ByVal Grad As Single) As Single
        Return CSng(((VEH.Loading + VEH.Mass + VEH.MassExtra) * 9.81 * Grad * 0.01 * v) * 0.001)
    End Function

    '----------------Ancillaries(Nebenaggregate) ----------------
    Private Function fPaux(ByVal t As Integer, ByVal nU As Single) As Single
        Return CSng(VEH.Paux0 * VEH.Pnenn + MODdata.Vh.Padd(t) + VEH.PauxSum(t, nU))
    End Function

    '-------------------Transmission(Getriebe)-------------------
    Private Function fPlossGB(ByVal PvD As Single, ByVal V As Single, ByVal Gear As Integer) As Single
        Dim Pdiff As Single
        Dim P As Single
        Dim P1_getr As Single
        Dim P8_getr As Single
        Dim P9_getr As Single
        Dim P16_getr As Single
        Dim n As Single
        Dim iTemp As Single
        Dim nU As Single

        If Gear = 0 Then Return 0

        nU = (60 * V) / (VEH.Dreifen * Math.PI) * VEH.Igetr(0) * VEH.Igetr(Gear)

        'Pdiff
        Pdiff = fPlossDiff(PvD, V)

        Select Case GEN.TransmModel

            Case tTransLossModel.Basic

                n = nU / VEH.nNenn

                'Leistung nach Getriebe (Getriebeausgang) |@@| Power to Transmission (Transmission-output)
                P = Math.Abs(PvD) + Pdiff

                'Calculate Losses (suitable only for Manual-transmission(Schaltgetriebe))
                '       Interpolation of the Transmission-power-loss
                '       Between 1 and 8 Gear, as well as between 9 and 16 Gear:
                If (Gear <= 8) Then
                    iTemp = VEH.Igetr(1)
                    P1_getr = VEH.Pnenn * 0.0025F * (-0.45F + 36.03F * (n / iTemp) + 14.97F * (P / VEH.Pnenn))
                    iTemp = VEH.Igetr(8)
                    If iTemp <= 0.2 Then iTemp = 0.6
                    P8_getr = VEH.Pnenn * 0.0025F * (-0.66F + 16.98F * (n / iTemp) + 5.33F * (P / VEH.Pnenn))
                    P = P8_getr + (8 - Gear) * (P1_getr - P8_getr) / 7
                Else
                    iTemp = VEH.Igetr(9)
                    If iTemp <= 0.2 Then iTemp = 0.6
                    P9_getr = VEH.Pnenn * 0.0025F * (-0.47F + 8.3F * (n / iTemp) + 9.53F * (P / VEH.Pnenn))
                    iTemp = VEH.Igetr(16)
                    If iTemp <= 0.2 Then iTemp = 0.6
                    P16_getr = VEH.Pnenn * 0.0025F * (-0.66F + 4.07F * (n / iTemp) + 0.000867F * (Math.Abs(P) / VEH.Pnenn))
                    P = P16_getr + (16 - Gear) * (P9_getr - P16_getr) / 7
                End If

                If (P <= 0) Then P = 0

                Return P * VEH.fGetr

            Case Else 'tTransLossModel.Detailed

                '***Differential
                '   Power after Differential (before Transmission)
                P = PvD + Pdiff

                Return Math.Max(VEH.IntpolPeLoss(Gear, nU, P), 0)

        End Select


    End Function

    Private Function fPlossDiff(ByVal PvD As Single, ByVal V As Single) As Single

        Dim Pdiff As Single
        Dim anrad As Single

        Select Case GEN.TransmModel

            Case tTransLossModel.Basic

                'Pdiff
                anrad = (60 * V) / (VEH.Dreifen * Math.PI)
                Pdiff = VEH.fGetr * VEH.Pnenn * 0.0025 * (-0.47 + 8.34 * (anrad / VEH.nNenn) + 9.53 * (Math.Abs(PvD) / VEH.Pnenn))
                If (Pdiff <= 0) Then Pdiff = 0

                Return Pdiff

            Case Else 'tTransLossModel.Detailed

                '***Differential
                '   Power before Differential
                Return Math.Max(VEH.IntpolPeLoss(0, (60 * V) / (VEH.Dreifen * Math.PI) * VEH.Igetr(0), PvD), 0)

        End Select

    End Function

    Private Function fPlossGBfwd(ByVal PeICE As Single, ByVal V As Single, ByVal Gear As Integer) As Single
        Dim nU As Single

        If Gear = 0 Then Return 0

        Select Case GEN.TransmModel

            Case tTransLossModel.Basic

                Return 0

            Case Else ' tTransLossModel.Detailed

                nU = (60 * V) / (VEH.Dreifen * Math.PI) * VEH.Igetr(0) * VEH.Igetr(Gear)

                Return Math.Max(VEH.IntpolPeLossFwd(Gear, nU, PeICE), 0)

        End Select

    End Function

    Private Function fPlossDiffFwd(ByVal PeIn As Single, ByVal V As Single) As Single
        Dim nU As Single

        Select Case GEN.TransmModel

            Case tTransLossModel.Basic

                Return 0

            Case Else ' tTransLossModel.Detailed

                nU = (60 * V) / (VEH.Dreifen * Math.PI) * VEH.Igetr(0)

                Return Math.Max(VEH.IntpolPeLossFwd(0, nU, PeIn), 0)

        End Select


    End Function

    Private Function fPlossRt(ByVal Vist As Single, ByVal Gear As Integer) As Single
        Return VEH.RtPeLoss(Vist, Gear)
    End Function

    '----------------Gearbox inertia ----------------
    Private Function fPaG(ByVal V As Single, ByVal a As Single) As Single
        Dim Mred As Single
        Mred = CSng(VEH.I_Getriebe * (VEH.AchsI / (0.5 * VEH.Dreifen)) ^ 2)
        Return CSng((Mred * a * V) * 0.001)
    End Function

#End Region


End Class

