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

    'Sekündliche Daten
    Private Clutch As tEngClutch
    Private VehState0 As tVehState
    Private EngState0 As tEngState
    Private Pplus As Boolean
    Private Pminus As Boolean
    Private GVmax As Single
    Private PvorD As Single
    Private Vist As Single
    Private aist As Single


    'Zugkraftunterbrechung
    Private TracIntrI As Integer
    Private TracIntrIx As Integer
    Private TracIntrOn As Boolean
    Private TracIntrTurnOff As Boolean
    Private TracIntrGear As Integer

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

    'Rekuperation
    '   Projekt HERO - BMW Mini Hybrid
    '   Standard Mini One D Radstand 2467 mm
    Private lSHv As Single = 1.2335    'Annahme 50/50 (Laut Internet Gewichtsverteilung vom MINI E)
    Private lSHh As Single = 1.2335
    'Annahme für Schwerpunkthöhe
    '   nach http://www.colliseum.net/wiki/Schwerpunkth%C3%B6he
    '   X = 0.2 * m / 1000 = h/R
    '       mit R = 2.467 [m] und m = 1335 [kg]
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

    'Bat einlesen
    Private Function BATread() As Boolean
        BAT.FilePath = GEN.Batfile
        SOCstart = GEN.SOCstart
        Return BAT.Bat_Init()
    End Function

    'Maximale effektive EM-Leistung beim Antreiben abhängig von Überlast und Batteriezustand
    Private Function fPeEMmax(ByVal nn As Single) As Single
        Dim PeFLD As Single
        Dim PeBAT As Single

        'Basis: Volllastkurve
        PeFLD = FLD.Pfull(nn)

        'Falls Überlast möglich: auf ÜL-Leistung hochskalieren
        If ULok Then PeFLD *= PeUL / VEH.Pnenn

        '=> PeFLD = maximale EM-Leistung nach FLD und ÜL

        'PeMax aus PeBatMax berechnen
        PeBAT = 0.9 * fPefromPi(nn, PeBatMax)

        '=> PeBAT = maximale EM-Leistung nach Batterie

        'Übergeben wird maximal die Leistung die die Batterie erlaubt
        Return Math.Min(PeFLD, PeBAT)

    End Function

    'Maximale effektive EM-Leistung beim Laden abhängig von Überlast und Batteriezustand
    Private Function fPeEMmin(ByVal nn As Single) As Single
        Dim PeFLD As Single
        Dim PeBAT As Single

        'Basis: Schleppkurve
        PeFLD = FLD.Pdrag(nn)

        'Falls Überlast möglich: auf ÜL-Leistung hochskalieren
        If ULok Then PeFLD *= PeUL / VEH.Pnenn

        '=> PeFLD = maximale EM-Leistung nach FLD und ÜL

        'PeMax aus PeBatMax berechnen
        PeBAT = 0.9 * fPefromPi(nn, PeBatMin)

        '=> PeBAT = maximale EM-Leistung nach Batterie

        'Übergeben wird maximal die Leistung die die Batterie erlaubt
        Return Math.Max(PeFLD, PeBAT)



    End Function

    'Umrechnung von PeBat(=PiEM) auf PeEM
    Private Function fPefromPi(ByVal nn As Single, ByVal Pi As Single) As Single
        Dim wisum As Double = 0
        Dim sumo As Double = 0
        Dim ab As Double = 0
        Dim Eta As Single
        Dim i As Int32
        Dim Pinorm As Single

        Pinorm = Pi / VEH.Pnenn

        For i = 0 To Ldim
            'Wenn Vorzeichen von x oder y nicht gleich Vorzeichen von xA(i) oder yA(i) dann wird Zeile i übersprungen
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

    'Umrechnung von PeEM auf PeBat(=PiEM)
    Private Function fPifromPe(ByVal nn As Single, ByVal Pe As Single) As Single
        Dim wisum As Double = 0
        Dim sumo As Double = 0
        Dim ab As Double = 0
        Dim Eta As Single
        Dim i As Int32
        Dim Penorm As Single

        Penorm = Pe / VEH.Pnenn

        For i = 0 To Ldim
            'Wenn Vorzeichen von x oder y nicht gleich Vorzeichen von xA(i) oder yA(i) dann wird Zeile i übersprungen
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

    'Maximale Rekup-Leistung
    Private Function fPrekupMax() As Single
        Dim Fz As Single
        Dim Fx As Single
        Dim Prekup As Single

        'Falls unter V-Untergrenze dann gleich Null übergeben
        If Vist < RekupVu Then Return 0

        'Radaufstandskraft
        If RekupVorne Then
            Fz = ((VEH.Mass + VEH.MassExtra + VEH.Loading) * (9.81F * lSHh - aist * hSH)) / (lSHv + lSHh)
        Else
            Fz = ((VEH.Mass + VEH.MassExtra + VEH.Loading) * (9.81F * lSHv + aist * hSH)) / (lSHv + lSHh)
        End If

        'Vorzeichen "sollte" immer + sein
        Fz = Math.Max(0, Fz)

        'Längskraft am Reifen
        Fx = Fz * muReifStr

        'Sicherheitsfaktor mitnehmen
        Fx /= RekupS

        'Leistung 
        Prekup = -Fx * Vist / 1000

        'Falls unter V-Obergrenze dann linear runter skalieren
        If Vist <= RekupVo Then Prekup *= (Vist - RekupVu) / (RekupVo - RekupVu)

        Return Prekup

    End Function

    ''PeEM-Max reduzieren bis es die Batterie aushaltet
    'Private Function RedPiToPbatMax(ByVal PeEM As Single) As Single

    '    Dim PiEM As Single

    '    PiEM = EMO.PiEM(PeEM)

    '    Do While PiEM > PeBatMax

    '        PeEM *= 0.99
    '        PiEM = EMO.PiEM(PeEM)

    '    Loop

    '    Return PiEM

    'End Function

    ''PeEM-Min reduzieren bis es die Batterie aushaltet
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

        'Start/Stop Steuerung
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
        Dim MsgSrc As String

        MsgSrc = "Power/Calc"

        StdMode = (PHEMmode = tPHEMmode.ModeSTANDARD)
        NotAdvMode = Not (PHEMmode = tPHEMmode.ModeADVANCE)

        'Abbruch wenn keine Geschw. gegeben
        If Not DRI.Vvorg Then
            WorkerMsg(tMsgID.Err, "Driving cycle is not valid! Vehicle Speed required.", MsgSrc)
            Return False
        End If

        '   Initialisieren
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

        'Schaltpunkte für NEDC/FTP Schaltung
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
                'Schaltparameter initialisieren

                'Standard
                Aaufi = 0.3
                Baufi = 0.3
                Caufi = 0.4
                Aobi = 0.18
                Bobi = 0.28
                Cobi = 0.46

        End Select

        'Theoretische Höchstgeschwindigkeit [m/s] - gesetzt auf Geschw. bei 1.2 x NennDrehzahl im höchsten Gang
        GVmax = 1.2 * VEH.nNenn * VEH.Dreifen * Math.PI / (VEH.AchsI * VEH.Igetr(VEH.ganganz) * 60)

        jz = -1

        '***********************************************************************************************
        '***********************************    Zeitschleife    ****************************************
        '***********************************************************************************************

        Do
            jz += 1

            MODdata.ModErrors.ResetAll()

            GVset = False
            FirstSecItar = True

            'Sekundäre Progressbar
            If NotAdvMode Then ProgBarCtrl.ProgJobInt = CInt(100 * jz / MODdata.tDim)

            '   Zustand bestimmen
lbGschw:

            'Reset der sekündlichen Errors 
            MODdata.ModErrors.GeschRedReset()

            'Geschw. / Beschl. berechnen-------------------
            'Jetzt durch DRI-Klasse
            Vist = Vh.V(jz)
            aist = Vh.a(jz)

            'Wenn Geschw. über theoretischer Höchstgeschw. => Reduzieren
            If Vist > GVmax + 0.0001 And Not GVset Then
                Vh.SetSpeed0(jz, GVmax)
                GVset = True
                GoTo lbGschw
            End If

            'a_DesMax
            If GEN.DesMaxJa Then

                'Check ob Beschleunigung zu hoch

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

                    '- Verzögerung limitieren ---------------------------
                    'Else
                    '    'Check ob Verzögerung zu hoch
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




            'Aus Leistg-----
            If aist < 0 Then
                If (Vist < 0.025) Then
                    'Vh.SetSpeed(jz, 0)
                    'GoTo lbGschw
                    Vist = 0
                End If
            End If
            '---------------

            'Fahrzustand bestimmen-------------------------
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

            'Schneller Check ob Leistung viel zu hoch
            If PvorD > 2 * VEH.Pnenn Then
                Vh.ReduceSpeed(jz, 0.98)
                GoTo lbGschw
            End If


            '************************************ Gangwahl ************************************
            If VehState0 = tVehState.Stopped Or TracIntrOn Then

                If TracIntrTurnOff Then

                    Gear = TracIntrGear

                    If fnn(Vist, Gear, False) < Kuppln_norm And Pplus Then
                        Clutch = tEngClutch.Slipping
                    Else
                        Clutch = tEngClutch.Closed
                    End If

                Else
                    Gear = 0
                    Clutch = tEngClutch.Opened
                End If

            Else
                'Checken ob Kupplung schleift (wichtig für Schaltmodell):
                If fnn(Vist, 1, False) < Kuppln_norm And Pplus Then
                    Clutch = tEngClutch.Slipping
                Else
                    Clutch = tEngClutch.Closed
                End If

                If Gvorg Then
                    'Gang-Vorgabe
                    Gear = Math.Min(Vh.GearVorg(jz), VEH.ganganz)
                ElseIf Nvorg Then
                    'Drehzahlvorgabe
                    Gear = fGearByU(MODdata.nUvorg(jz), Vist)
                ElseIf VEH.ganganz = 1 Then
                    Gear = 1
                Else
                    If GnachV Then
                        'Gang nach Geschwindigkeit
                        Gear = fGearBySpeed(jz)
                    Else

                        'Schaltmodell
                        If PKWja Then
                            Gear = fGearPKW(jz)
                        Else
                            Gear = fGearLKW(jz)
                        End If

                        'Muss hier nochmal zurück gesetzt werden weil im Schaltmodell kann (und darf) das passieren
                        MODdata.ModErrors.PxReset()

                    End If
                End If

                'Schaltmodell/Vorgabe kann Clutch öffnen 
                If Gear < 1 Then Clutch = tEngClutch.Opened

            End If

            If Gear = -1 Then
                WorkerMsg(tMsgID.Err, "Error in Gear Shift Model!", MsgSrc & "/t= " & jz + 1)
                Return False
            End If


            ' Wichtige Checks
lbCheck:

            'Checken ob Geschwindigkeit reduzieren
            ''If GeschwRed Then GoTo lbGeschwRed    <= stattdessen neues Konzept: Leistung wird "normal" berechnet und erst später überprüft ob Pe > Pmax... ?

            'Checken ob Kupplung offen:
            ''bKupplOffen = (bStehen Or Gang(jz) = 0) <= bereits durch Clutch bekannt

            'Falls konventionell dann ICE-Kupplung = Hauptkupplung
            ''bICEKupOffen = bKupplOffen <= Brauch i nix mehr

            'Falls vor Gangwahl festgestellt wurde, dass nicht KupplSchleif, dann bei zu niedriger Drehzahl runterschalten:
            If Clutch = tEngClutch.Closed Then
                If fnn(Vist, Gear, False) < Kuppln_norm And Not VehState0 = tVehState.Dec And Gear > 1 Then Gear -= 1
            End If

            'Checken ob Leerlauf obwohl Leistung > 0
            '   wenn Leistung vor Diff > 0.1% von Nennleistung dann Korrigieren!
            If Clutch = tEngClutch.Opened Then
                If PvorD > 0.001 * VEH.Pnenn Then

                    If TracIntrOn Then
                        Gear = TracIntrGear
                    Else
                        Gear = 1
                    End If

                    If fnn(Vist, Gear, False) < Kuppln_norm Then
                        Clutch = tEngClutch.Slipping
                    Else
                        Clutch = tEngClutch.Closed
                    End If

                    GoTo lbCheck

                End If
            End If

            '************************************ Drehzahl ************************************

            '*** Wenn Drehzahl vorgegeben dann wird der nächste Block übersprungen ***
            If Nvorg Then

                nn = (MODdata.nUvorg(jz) - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                'Falls Start/Stop dann wird gleich bei nn < -0.05 auf nU = 0 gesetzt
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

            'Drehzahlabfall beim Auskuppeln
            If Clutch = tEngClutch.Opened Then
                If jz = 0 Then
                    nn = 0
                Else

                    If MODdata.nn(jz - 1) <= 0.00001 Then
                        nn = 0
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
                        'Leistungsabfall limitieren auf Pe(t-1) minus 75% von (Pe(t-1) - Pschlepp)
                        '   aus Auswertung ETC des Motors mit dem dynamische Volllast parametriert wurde
                        '   Einfluss auf Beschleunigungsvermögen gering (Einfluss durch Pe(t-1) bei dynamischer Volllast mit PT1)
                        '   Luz/Rexeis 21.08.2012
                        '   Iterations-Schleife: 01.10.2012    
                        P = MODdata.Pe(jz - 1) * VEH.Pnenn - 0.75 * (MODdata.Pe(jz - 1) * VEH.Pnenn - Pmin)
                        M = -P * 1000 * 60 / (2 * Math.PI * nU)
                        'original: M = -Pmin * 1000 * 60 / (2 * Math.PI * ((nU + nUx) / 2))
                        omega_p = M / VEH.I_mot
                        omega2 = omega1 - omega_p
                        nU = omega2 * 60 / (2 * Math.PI)
                        i += 1
                        '01.10.12 Luz: Drehzahl darf nicht höher werden als zuvor 
                        If nU > nUx Then
                            nU = nUx
                            Exit Do
                        End If
                    Loop Until Math.Abs(Pmin - PminX) < 0.001 Or nU <= VEH.nLeerl Or i = 999

                    'TODO: Auschalten?!
                    If i = 999 Then WorkerMsg(tMsgID.Warn, "i=999", MsgSrc & "/t= " & jz + 1)

                    nn = (Math.Max(VEH.nLeerl, nU) - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                    MODdata.ModErrors.FLDextrapol = ""

                End If

            Else

                nn = fnn(Vist, Gear, Clutch = tEngClutch.Slipping)

                '*** Beginn: Drehzahl-Check

                'Checken ob Drehzahl zu hoch! =>  Hochschalten
                Do While nn > 1.2 And Gear < VEH.ganganz
                    Gear += 1
                    nn = fnn(Vist, Gear, Clutch = tEngClutch.Slipping)
                Loop

                'Checken ob Drehzahl zu niedrig mit geschlossener Kupplung
                If Clutch = tEngClutch.Closed Then
                    If nn < 0.0001 Then
                        Gear -= 1
                        nn = fnn(Vist, Gear, Clutch = tEngClutch.Slipping)
                    End If
                End If


            End If




lb_nOK:


            '************************************ Motor-Zustand bestimmen ************************************
            ' nn ist ab hier fix!
            nU = nn * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl

            'Nebenverbrauch bestimmen (aus VEH und DRI)
            Paux = fPaux(jz, nU)

            'ICE-Trägheit
            If Clutch = tEngClutch.Opened Then
                If jz = 0 Then
                    PaMot = 0
                Else
                    'Nicht optimal da jz-1 bis jz nicht der richtige Zeitraum
                    PaMot = (VEH.I_mot * (nU - MODdata.nU(jz - 1)) * 0.01096 * nU) * 0.001
                End If
            Else
                If Nvorg Then
                    'Drehzahlvorgabe
                    PaMot = (VEH.I_mot * MODdata.dnUvorg(jz) * 0.01096 * MODdata.nUvorg(jz)) * 0.001
                Else
                    PaMot = ((VEH.I_mot * (VEH.AchsI * VEH.Igetr(Gear) / (0.5 * VEH.Dreifen)) ^ 2) * aist * Vist) * 0.001
                End If
            End If

            'Gesamt-Motorleistung
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
                    PlossGB = fPlossGB(PvorD, Vist, Gear)
                    PlossDiff = fPlossDiff(PvorD, Vist)
                    PlossRt = fPlossRt(Vist, Gear)
                    PaGetr = fPaG(Vist, aist)
                    Pkup = PvorD + PlossGB + PlossDiff + PaGetr + PlossRt
                    P = Pkup + Paux + PaMot
                Case Else 'tEngClutch.Slipping
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



            '*************** Leistungsverteilung usw. ******************

            'Volllast- / Schleppkurve
            If EngState0 = tEngState.Stopped Then

                Pmin = 0
                Pmax = 0

                'Drehzahl korrigieren
                nU = 0
                nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

            Else

                Pmin = FLD.Pdrag(nn)

                If jz = 0 Then
                    Pmax = FLD.Pfull(nn)
                Else
                    Pmax = FLD.Pfull(nn, MODdata.Pe(jz - 1))
                End If

                'Falls Pmax < 0 oder Pmin > 0 dann Abbruch mit Error!
                If Pmin >= 0 And P < 0 Then
                    WorkerMsg(tMsgID.Err, "Pe_drag > 0! n_norm= " & nn, MsgSrc & "/t= " & jz + 1)
                    Return False
                ElseIf Pmax <= 0 And P > 0 Then
                    WorkerMsg(tMsgID.Err, "Pe_full < 0! n_norm= " & nn, MsgSrc & "/t= " & jz + 1)
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
                    If Math.Abs(P / Pmax - 1) < 0.02 Then EngState0 = tEngState.FullLoad
                Else    ' tEngState.Drag (tEngState.Idle, tEngState.Stopped kann's hier nicht geben weil Clutch <> Closed)
                    If P < Pmin Then

                        MODdata.ModErrors.TrLossMapExtr = ""

                        'VKM an Schleppkurve
                        P = Pmin

                        'Forwärtsrechnung bis Rad (PvorD)
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

            'Check ob Abbruch (vor Geschw.Red-Iteration sonst kann sichs aufhängen)
            If PHEMworker.CancellationPending Then Return True

            'Check ob P über Volllast => Geschw.-Reduktion
            If Pplus And P > Pmax Then
                If EngState0 = tEngState.Load Or EngState0 = tEngState.FullLoad Then
                    If Vist > 0.01 Then
                        Select Case P / Pmax
                            Case Is > 1.6
                                Vh.ReduceSpeed(jz, 0.99)
                            Case Is > 1.3
                                Vh.ReduceSpeed(jz, 0.995)
                            Case Else
                                Vh.ReduceSpeed(jz, 0.999)
                        End Select
                        FirstSecItar = False
                        GoTo lbGschw
                    Else
                        'FEHLER: Geschw.-Red. bringt nix?!...
                        WorkerMsg(tMsgID.Err, "Speed reduction failed!", MsgSrc & "/t= " & jz + 1)
                        Return False
                    End If
                Else 'tEngState.Idle, tEngState.Stopped, tEngState.Drag
                    'FEHLER: Motor nicht in Antrieb ...kann nicht sein?!
                    If FirstSecItar Then
                        If P > 0.1 Then WorkerMsg(tMsgID.Warn, "Pwheel > 0 but EngState undefined ?!", MsgSrc & "/t= " & jz + 1)
                    End If
                End If
            End If


            'Zugkraftunterbrechung
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
            '   Sekunde abschließen

            'Start / Stop - Aktivierung-Geschw. Steuerung
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

            'Modalwerte-Felder schreiben
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


            If Cfg.WegKorJa Then Vh.DistCorrection(jz)

            'Zugkraftunterbrechung
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

            'Meldungen (Abbruch falls Error)
            If MODdata.ModErrors.MsgOutputAbort(jz + 1, MsgSrc) Then Return False

            If Clutch = tEngClutch.Closed And Nvorg Then
                If Math.Abs(nU - fnU(Vist, Gear, False)) > 0.2 * VEH.nNenn Then
                    WorkerMsg(tMsgID.Warn, "Target rpm =" & nU & ", calculated rpm(gear " & Gear & ")= " & fnU(Vist, Gear, False), MsgSrc & "/t= " & jz + 1)
                End If
            End If


        Loop Until jz >= MODdata.tDim

        '***********************************************************************************************
        '***********************************    Zeitschleife ENDE    ***********************************
        '***********************************************************************************************

        'Meldungen (wenn nicht ADVANCE)
        If NotAdvMode Then

            If Cfg.WegKorJa Then
                If MODdata.tDim > MODdata.tDimOgl Then WorkerMsg(tMsgID.Normal, "Cycle extended by " & MODdata.tDim - MODdata.tDimOgl & " seconds.", MsgSrc)
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

        MsgSrc = "Power/Eng_Calc"

        StdMode = (PHEMmode = tPHEMmode.ModeSTANDARD)
        NotAdvMode = Not (PHEMmode = tPHEMmode.ModeADVANCE)

        'Abbruch falls Leistung/Drehzahl nicht gegeben
        If Not (DRI.Nvorg And DRI.Pvorg) Then
            WorkerMsg(tMsgID.Err, "Load cycle is not valid! rpm and load required.", MsgSrc)
            Return False
        End If

        PcorCount = 0
        t1 = MODdata.tDim
        nnDRI = DRI.Values(tDriComp.nn)
        PeDRI = DRI.Values(tDriComp.Pe)

        'Drehzahlen vorher weil sonst scheitert die Pmr-Berechnung bei MODdata.nU(t + 1)
        For t = 0 To t1
            'Modalwerte-Felder schreiben
            '   MODdata.Pe wird unten belegt
            MODdata.nn.Add(nnDRI(t))
            MODdata.nU.Add(Math.Max(0, nnDRI(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl))
        Next

        'Leistung berechnen
        For t = 0 To t1

            'Sekundäre Progressbar
            If NotAdvMode Then ProgBarCtrl.ProgJobInt = CInt(100 * t / t1)

            'Reset der sekündlichen Errors 
            MODdata.ModErrors.ResetAll()

            'ALT und falsch weil Zeit nicht verschoben: P_mr(jz) = 0.001 * (I_mot * 0.0109662 * (n(jz) * nnenn) * nnenn * (n(jz) - n(jz - 1))) / Pnenn
            If t > 0 And t < t1 Then
                Pmr = 0.001 * (VEH.I_mot * (2 * Math.PI / 60) ^ 2 * MODdata.nU(t) * 0.5 * (MODdata.nU(t + 1) - MODdata.nU(t - 1))) / VEH.Pnenn
            Else
                Pmr = 0
            End If

            'Leistung aus Zyklus korrigiert um P_clutch
            MODdata.Pe.Add(PeDRI(t) + Pmr)

            'Drehzhal aus Zyklus => Durch CycleInit bestimmt
            'Falls Drehzahl unter Leerlauf wir Motor als abgestellt angenommen
            If MODdata.nn(t) < Cfg.nnormEngStop Then
                EngState0 = tEngState.Stopped
            Else
                PminN = FLD.Pdrag(MODdata.nn(t)) / VEH.Pnenn

                If t = 0 Then
                    PmaxN = FLD.Pfull(MODdata.nn(t)) / VEH.Pnenn
                Else
                    PmaxN = FLD.Pfull(MODdata.nn(t), MODdata.Pe(t - 1)) / VEH.Pnenn
                End If

                'Falls Pmax < 0 oder Pmin > 0 dann Abbruch mit Error!
                If PminN >= 0 AndAlso MODdata.Pe(t) < 0 Then
                    WorkerMsg(tMsgID.Err, "Pe_drag > 0! n_norm= " & MODdata.nn(t), MsgSrc & "/t= " & t + 1)
                    Return False
                ElseIf PmaxN <= 0 AndAlso MODdata.Pe(t) > 0 Then
                    WorkerMsg(tMsgID.Err, "Pe_full < 0! n_norm= " & MODdata.nn(t), MsgSrc & "/t= " & t + 1)
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

            'Meldungen
            If MODdata.ModErrors.MsgOutputAbort(t + 1, MsgSrc) Then Return False

        Next

        If PcorCount > 0 Then WorkerMsg(tMsgID.Warn, "Power corrected (>5%) in " & PcorCount & " time steps.", MsgSrc)

        Return True

    End Function

    Public Function EV_Calc() As Boolean

        Dim M As Single
        Dim nn As Single
        Dim nU As Single
        Dim omega_p As Single
        Dim omega1 As Single, omega2 As Single
        Dim TzyklOgl As Int32
        Dim jz As Integer
        Dim Tzykl As Integer

        'Start/Stop Steuerung
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
        Dim Paux As Single
        Dim Pbrake As Single
        Dim PlossGB As Single
        Dim PlossDiff As Single
        Dim PlossRt As Single

        Dim Pmax As Single
        Dim Pmin As Single
        Dim PrekupMax As Single
        Dim PbrakeRek As Single

        Dim PeBat0 As Single
        Dim PiBat0 As Single
        Dim PiEM As Single

        Dim GVset As Boolean

        'WegKorrektur
        Dim WegIst As Single
        Dim WegX As Integer
        Dim SecSpeedRed As Integer

        Dim StdMode As Boolean
        Dim NotAdvMode As Boolean
        Dim MsgSrc As String

        MsgSrc = "Power/EV_Calc"

        StdMode = (PHEMmode = tPHEMmode.ModeSTANDARD)
        NotAdvMode = Not (PHEMmode = tPHEMmode.ModeADVANCE)

        'Abbruch wenn keine Geschw. gegeben
        If Not DRI.Vvorg Then
            WorkerMsg(tMsgID.Err, "Driving cycle is not valid! Vehicle Speed required.", MsgSrc)
            Return False
        End If

        '   Initialisieren
        Vh = MODdata.Vh
        WegIst = 0
        WegX = 0
        SecSpeedRed = 0


        PeUL = VEH.Pnenn

        If Cfg.GnVorgab Then
            Gvorg = DRI.Gvorg
            Nvorg = DRI.Nvorg
        Else
            Gvorg = False
            Nvorg = False
        End If
        PKWja = GEN.PKWja
        StStAus = False
        StStTx = 0

        If GEN.izykwael = 4 Then
            Kuppln_norm = 0.05
            KupplEta = 0.4
        Else
            Kuppln_norm = 0.03
            KupplEta = 1
        End If

        'WG-Kennfeld aus MAP übernehmen und Pi-Liste berechnen
        PeL = MAP.LPe
        nnL = MAP.Lnn
        Ldim = PeL.Count - 1
        PiL = New List(Of Single)
        WGL = New List(Of Single)
        For jz = 0 To Ldim
            WGL.Add(MAP.EmDefRef(tMapComp.Eta).RawVals(jz))
            If PeL(jz) > 0 Then     'Antrieb    => 0 < Pe < Pi
                PiL.Add(PeL(jz) / WGL(jz))
            ElseIf PeL(jz) < 0 Then 'Laden      => 0 > Pi > Pe
                PiL.Add(PeL(jz) * WGL(jz))
            Else
                PiL.Add(0)
            End If
        Next

        'Schaltpunkte für NEDC/FTP Schaltung
        Select Case GEN.izykwael
            Case 0  'Nefzja = True
                GnachV = True
            Case 1  'Ftpja = True 
                GnachV = True
            Case Else
                GnachV = False
                Aaufi = 0.3
                Baufi = 0.3
                Caufi = 0.4
                Aobi = 0.18
                Bobi = 0.28
                Cobi = 0.46
        End Select


        'Theoretische Höchstgeschwindigkeit [m/s]
        GVmax = VEH.nNenn * VEH.Dreifen * Math.PI / (VEH.AchsI * VEH.Igetr(VEH.ganganz) * 60)

        jz = -1
        Tzykl = MODdata.tDim
        TzyklOgl = Tzykl

        '***********************************************************************************************
        '***********************************    Zeitschleife    ****************************************
        '***********************************************************************************************

        Do
            jz += 1

            GVset = False

            'Sekundäre Progressbar
            If NotAdvMode Then ProgBarCtrl.ProgJobInt = CInt(100 * jz / MODdata.tDim)

            '   Zustand bestimmen

lbGschw:

            'Geschw. / Beschl. berechnen-------------------
            'Jetzt durch DRI-Klasse
            Vist = Vh.V(jz)
            aist = Vh.a(jz)

            'Wenn Geschw. über theoretischer Höchstgeschw. => Reduzieren
            If Vist > GVmax + 0.0001 And Not GVset Then
                Vh.SetSpeed0(jz, GVmax)
                GVset = True
                GoTo lbGschw
            End If

            'Aus Leistg-----
            If aist < 0 Then
                If (Vist < 0.025) Then
                    Vist = 0
                End If
            End If
            '---------------

            PbrakeRek = 0

            'Fahrzustand bestimmen-------------------------
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

            'Maximal zulässige Batterieleistung
            BAT.Bat_Pzul(jz)
            PeBatMax = BAT.PmaxAntr
            PeBatMin = BAT.PmaxLaden

            '************************************ Gangwahl ************************************
            If VehState0 = tVehState.Stopped Then
                Gear = 0
                Clutch = tEngClutch.Opened
            Else
                'Checken ob Kupplung schleift (wichtig für Schaltmodell):
                If fnn(Vist, 1, False) < Kuppln_norm And Pplus Then
                    Clutch = tEngClutch.Slipping
                Else
                    Clutch = tEngClutch.Closed
                End If

                If Gvorg Then
                    'Gang-Vorgabe
                    Gear = Math.Min(Vh.GearVorg(jz), VEH.ganganz)
                ElseIf Nvorg Then
                    'Drehzahlvorgabe
                    Gear = fGearByU(MODdata.nUvorg(jz), Vist)
                ElseIf VEH.ganganz = 1 Then
                    Gear = 1
                Else
                    If GnachV Then
                        'Gang nach Geschwindigkeit wird hier nicht unterstützt
                        WorkerMsg(tMsgID.Err, "Geary-By-Speed not supported in EV mode!", MsgSrc)
                        Return False
                    Else
                        'Schaltmodell
                        Gear = fGearEV(jz)

                        'EV: Kein Leerlauf wegen Rekuperation
                        If Gear = 0 Then Gear = 1
                    End If
                End If
            End If

            If Gear = -1 Then
                WorkerMsg(tMsgID.Err, "Error in Gear Shift Model!", MsgSrc & "/t= " & jz + 1)
                Return False
            End If

            If Gear < 1 Then Clutch = tEngClutch.Opened

            'Falls Rekuperation laut Radleistung möglich: PrekupMax berechnen
            If Pminus And Clutch = tEngClutch.Closed Then
                'Maximale Rekuperations-Leistung berechnen (abh. von Radlast/Reibkoef.)
                PrekupMax = fPrekupMax()
                If PrekupMax > -0.000001 Then Clutch = tEngClutch.Opened
            End If

            'Falls vor Gangwahl festgestellt wurde, dass nicht KupplSchleif, dann bei zu niedriger Drehzahl runterschalten:
            If Clutch = tEngClutch.Closed Then
                If fnn(Vist, Gear, False) < Kuppln_norm And Not VehState0 = tVehState.Dec And Gear > 1 Then Gear -= 1
            End If

            'Checken ob Leerlauf obwohl Leistung > 0
            '   wenn Leistung vor Diff > 0.1% von Nennleistung dann Korrigieren!
            If Clutch = tEngClutch.Opened Then
                If PvorD > 0.001 * VEH.Pnenn Then
                    Gear = 1
                    If fnn(Vist, Gear, False) < Kuppln_norm Then
                        Clutch = tEngClutch.Slipping
                    Else
                        Clutch = tEngClutch.Closed
                    End If
                End If
            End If

            '************************************ Drehzahl ************************************

            '*** Wenn Drehzahl vorgegeben (Gemess = 2) dann wird der nächste Block übersprungen ***
            If Nvorg Then
                nn = (MODdata.nUvorg(jz) - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)
                GoTo lb_nOK
            End If

lb10:

            'Drehzahlabfall beim Auskuppeln
            If Clutch = tEngClutch.Opened Then
                If jz = 0 Then
                    nn = 0
                Else
                    nn = MODdata.nn(jz - 1)
                    If nn <= 0.00001 Then GoTo lb_nOK
                    Pmin = FLD.Pdrag(nn)
                    nU = nn * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl
                    omega1 = nU * 2 * Math.PI / 60
                    M = -Pmin * 1000 * 60 / (2 * Math.PI * nU)
                    omega_p = M / VEH.I_mot
                    omega2 = omega1 - omega_p
                    nU = Math.Max(VEH.nLeerl, omega2 * 60 / (2 * Math.PI))
                    nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)
                End If

            Else

                nn = fnn(Vist, Gear, Clutch = tEngClutch.Slipping)

                '*** Beginn: Drehzahl-Check wenn keine Vorgabe

                'Checken ob Drehzahl zu hoch! =>  Hochschalten
                Do While nn > 1 And Gear < VEH.ganganz
                    Gear += 1
                    nn = fnn(Vist, Gear, Clutch = tEngClutch.Slipping)
                Loop

                'Checken ob Drehzahl zu niedrig mit geschlossener Kupplung
                If Clutch = tEngClutch.Closed Then
                    If nn < 0.001 Then
                        Gear = 0
                        Clutch = tEngClutch.Opened
                    End If
                End If

            End If


lb_nOK:
            '************************************ Motor-Zustand bestimmen ************************************
            ' nn ist ab hier fix!
            nU = nn * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl

            'Nebenverbrauch bestimmen (aus VEH und DRI)
            Paux = fPaux(jz, nU)

            'MotorTrägheit
            If Clutch = tEngClutch.Opened Then
                If jz = 0 Then
                    PaMot = 0
                Else
                    'Nicht optimal da jz-1 bis jz nicht der richtige Zeitraum
                    PaMot = (VEH.I_mot * (nU - MODdata.nU(jz - 1)) * 0.01096 * (nU + MODdata.nU(jz - 1) / 2)) * 0.001
                End If
            Else
                If Nvorg Then
                    'Drehzahlvorgabe
                    PaMot = (VEH.I_mot * MODdata.dnUvorg(jz) * 0.01096 * MODdata.nUvorg(jz)) * 0.001
                Else
                    PaMot = ((VEH.I_mot * (VEH.AchsI * VEH.Igetr(Gear) / (0.5 * VEH.Dreifen)) ^ 2) * aist * Vist) * 0.001
                End If
            End If

            'Gesamt-Motorleistung
            '   => Pantr
            '   => P
            '   => Pkup
            Select Case Clutch
                Case tEngClutch.Opened
                    P = PaMot
                    Pkup = 0
                    PlossGB = 0
                    PlossDiff = 0
                    PlossRt = 0
                    PaGetr = 0
                Case tEngClutch.Closed
                    PlossGB = fPlossGB(PvorD, Vist, Gear)
                    PlossDiff = fPlossDiff(PvorD, Vist)
                    PlossRt = fPlossRt(Vist, Gear)
                    PaGetr = fPaG(Vist, aist)
                    Pkup = PvorD + PlossGB + PlossDiff + PaGetr + PlossRt
                    P = Pkup + PaMot
                Case tEngClutch.Slipping
                    PlossGB = fPlossGB(PvorD, Vist, Gear)
                    PlossDiff = fPlossDiff(PvorD, Vist)
                    PlossRt = fPlossRt(Vist, Gear)
                    PaGetr = fPaG(Vist, aist)
                    Pkup = (PvorD + PlossGB + PlossDiff + PaGetr + PlossRt) / KupplEta
                    P = Pkup + PaMot
            End Select

            'EngState      
            Select Case P
                Case Is > 0.0001    'Antrieb
                    EngState0 = tEngState.Load

                Case Is < -0.0001   'Rekuperieren
                    EngState0 = tEngState.Drag

                Case Else           'Idle/Stop
                    EngState0 = tEngState.Stopped
            End Select


            '*************** Leistungsverteilung usw. ******************

            'Volllast- / Schleppkurve
            If EngState0 = tEngState.Stopped Then
                Pmin = 0
                Pmax = 0
                PeBatMax = 0
                PeBatMax = 0
                nU = 0
                nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)
            Else
                Pmax = fPeEMmax(nn)
                Pmin = fPeEMmin(nn)
            End If

            '   => Pbrake
            Select Case EngState0
                Case tEngState.Load
                    Pbrake = 0
                    If Math.Abs(P / Pmax - 1) < 0.02 Then EngState0 = tEngState.FullLoad
                Case tEngState.Drag

                    'Maximale Rekuperations-Leistung berechnen (abh. von Radlast/Reibkoef.)
                    'PrekupMax = fPrekupMax()

                    'Falls RekupMax überschritten muss Pe neu berechnet werden
                    If PrekupMax > PvorD And Clutch = tEngClutch.Closed Then

                        'PbrakeRek = Leistung die zusätzlich mechanisch gebremst werden muss wegen Überschreitung von RekupMax
                        'wird schon oben nach Gangwahl erledigt: PbrakeRek = Pantr - PrekupMax

                        'Neue EM-Leistung
                        P = PrekupMax + fPlossGB(PrekupMax, Vist, Gear) + fPlossDiff(PrekupMax, Vist) + fPaG(Vist, aist) + PaMot

                    End If

                    'Check ob Leistung aufgenommen werden kann (abh. von FLD und Batterie). Bremsleistung berechnen.
                    If P < Pmin Then
                        Pbrake = P - Pmin
                        P = Pmin
                        EngState0 = tEngState.FullDrag
                    Else
                        Pbrake = 0
                    End If

                    'RekupMax-Bremsleistung dazu addieren
                    Pbrake += PbrakeRek

                Case Else 'tEngState.Idle, tEngState.Stopped
                    If PvorD < -0.00001 Then
                        Pbrake = PvorD
                    Else
                        Pbrake = 0
                    End If

            End Select

            'Check ob P über Volllast => Geschw.-Reduktion
            If Pplus And P > Pmax Then
                If EngState0 = tEngState.Load Or EngState0 = tEngState.FullLoad Then

                    'Falls Pmax=0 muss Batterie leer sein
                    If Pmax < 0.0001 Then GoTo lbBatLeer

                    If Vist > 0.01 Then
                        Select Case P / Pmax
                            Case Is > 1.6
                                Vh.ReduceSpeed(jz, 0.99)
                            Case Is > 1.3
                                Vh.ReduceSpeed(jz, 0.995)
                            Case Else
                                Vh.ReduceSpeed(jz, 0.999)
                        End Select
                        GoTo lbGschw
                    Else
                        'FEHLER: Geschw.-Red. bringt nix?!...
                        WorkerMsg(tMsgID.Err, "P > Pmax ?!", MsgSrc)
                        Return False
                    End If
                End If
            End If

            '-------------------------------------------------------
            '------------------------- PNR -------------------------
            '-------------------------------------------------------
            '   Sekunde abschließen

            If PHEMworker.CancellationPending Then Return True

            '**************************************************
            '******************** EM **************************
            If P = 0 Then
                PiEM = 0
            Else
                PiEM = fPifromPe(nn, P)
            End If

            '**************************************************
            '***************** Batterie ***********************
            If EngState0 = tEngState.Stopped Or P = 0 Then
                PeBat0 = Paux
            Else
                PeBat0 = fPifromPe(nn, P) + Paux
            End If

            BAT.Bat_Calc(PeBat0, jz)

            PiBat0 = PeBat0 + BAT.PbatV

            '*************************************************
            '****** Modalwerte-Felder schreiben **************
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

            PeEMot.Add(P)
            PiEMot.Add(PiEM)
            PeBat.Add(PeBat0)
            PiBat.Add(PiBat0)
            Ubat.Add(BAT.Ubat)
            Ibat.Add(BAT.Ibat)
            SOC.Add(BAT.SOC)
            TempBat.Add(BAT.TempBat)

            If SOC(jz) <= BAT.SOC_MIN Then GoTo lbBatLeer

            If Cfg.WegKorJa Then Vh.DistCorrection(jz)


            If Vh.Vsoll(jz) - Vist > 1.5 Then SecSpeedRed += 1

            'Meldungen
            If MODdata.ModErrors.MsgOutputAbort(jz + 1, MsgSrc) Then Return False

        Loop Until jz >= Tzykl

        '***********************************************************************************************
        '*********************************    Zeitschleife ENDE    *************************************
        '***********************************************************************************************


        'Meldungen (nicht ADV)
        If NotAdvMode Then

            If Cfg.WegKorJa Then
                If MODdata.tDim > TzyklOgl Then WorkerMsg(tMsgID.Warn, "Cycle extended by " & MODdata.tDim - TzyklOgl & " seconds.", MsgSrc)
            End If

            If SecSpeedRed > 0 Then WorkerMsg(tMsgID.Warn, "Speed reduction > 1.5 m/s in " & SecSpeedRed & " time steps.", MsgSrc)

        End If


        GoTo lbDone



lbBatLeer:

        'TODO.... Fehlermeldung etc
        MODdata.tDim = jz - 1
        WorkerMsg(tMsgID.Warn, "SOC-Min reached! Calculation aborted!", MsgSrc)

lbDone:

        'CleanUp
        Vh = Nothing
        PeL = Nothing
        PiL = Nothing
        nnL = Nothing
        WGL = Nothing

        Return True

    End Function

    Public Function HEV_Calc() As Boolean

        Dim M As Single
        Dim nn As Single
        Dim nU As Single
        Dim omega_p As Single
        Dim omega1 As Single, omega2 As Single
        Dim TzyklOgl As Int32
        Dim jz As Integer

        Dim Vh As cVh

        Dim Gear As Integer
        Dim GnachV As Boolean
        Dim PKWja As Boolean

        Dim P As Single
        Dim Pkup As Single
        Dim PaGetr As Single
        Dim ICEmin As Single
        Dim ICEmax As Single
        Dim Paux As Single
        Dim Pbrake As Single
        Dim PlossGB As Single
        Dim PlossDiff As Single
        Dim PlossRt As Single

        Dim GVset As Boolean

        'WegKorrektur
        Dim WegIst As Single
        Dim WegX As Integer

        Dim SecSpeedRed As Integer

        Dim nU0 As Single

        'HEV
        Dim SOC0 As Single
        Dim SOCo As Single
        Dim SOCu As Single
        Dim EMmin As Single
        Dim EMmax As Single

        Dim BatLvl As tBatLvl
        Dim ICElock As tICElock
        Dim LockCount As Integer

        Dim PaICE As Single
        Dim PaEM As Single

        Dim PeEV As Single
        Dim PeICEonly As Single
        Dim PeKomb As Single

        Dim FirstSec As Boolean

        Dim LastHEVmode As tHEVparMode
        Dim LastHEVnotPossible As Boolean
        Dim LastEngState As tEngState
        Dim LastPeICE As Single
        Dim StateChange As Boolean

        Dim ModeCheck As Dictionary(Of tHEVparMode, Boolean)
        Dim EVcrit As Boolean
        Dim HEVmode0 As tHEVparMode
        Dim HEVmodeByKe As tHEVparMode
        Dim NeedBoost As Boolean
        Dim Under_vEVu As Boolean
        Dim Under_vEVo As Boolean

        Dim KeA As Single
        Dim KeG As Single
        Dim KeSTE As Single
        Dim KeFail As Boolean
        Dim DeltaG As Single
        Dim DeltaA As Single

        Dim Pel As Single
        Dim PelStep As Single = 0.5
        Dim FCbasis As Single
        Dim Pvkm As Single
        Dim PvkmAlt As Single
        Dim KeAmin As Single
        Dim KeGmax As Single
        Dim PelOpt As Single
        Dim FCminus As Single
        Dim FCplus As Single
        Dim Pmot As Single
        Dim Pbat As Single
        Dim PeKeAoptEM As Single
        Dim PeKeGoptEM As Single
        Dim PeKeAoptICE As Single
        Dim PeKeGoptICE As Single
        Dim bAbort As Boolean

        Dim PeBat0 As Single
        Dim PiBat0 As Single

        Dim PeICE As Single
        Dim nnICE As Single
        Dim nUICE As Single

        Dim PeEM As Single
        Dim PiEM As Single
        Dim nUEM As Single

        Dim PrekupMax As Single

        Dim ICEclutch As Boolean
        Dim EMclutch As Boolean

        Dim StdMode As Boolean
        Dim NotAdvMode As Boolean
        Dim MsgSrc As String

        MsgSrc = "Power/HEV_Calc"

        StdMode = (PHEMmode = tPHEMmode.ModeSTANDARD)
        NotAdvMode = Not (PHEMmode = tPHEMmode.ModeADVANCE)


        ModeCheck = New Dictionary(Of tHEVparMode, Boolean)
        ModeCheck.Add(tHEVparMode.Assist, False)
        ModeCheck.Add(tHEVparMode.LPI, False)
        ModeCheck.Add(tHEVparMode.EV, False)
        ModeCheck.Add(tHEVparMode.ICEonly, False)
        ModeCheck.Add(tHEVparMode.Rekup, False)

        'Abbruch wenn keine Geschw. gegeben
        If Not DRI.Vvorg Then
            WorkerMsg(tMsgID.Err, "Driving cycle is not valid! Vehicle Speed required.", MsgSrc)
            Return False
        End If

        '   Initialisieren
        Vh = MODdata.Vh
        If Cfg.GnVorgab Then
            Gvorg = DRI.Gvorg
            Nvorg = DRI.Nvorg
        Else
            Gvorg = False
            Nvorg = False
        End If
        PKWja = GEN.PKWja
        LockCount = 0
        WegIst = 0
        WegX = 0
        SecSpeedRed = 0

        If GEN.izykwael = 4 Then
            Kuppln_norm = 0.05
            KupplEta = 0.4
        Else
            Kuppln_norm = 0.03
            KupplEta = 1
        End If

        'Schaltpunkte für NEDC/FTP Schaltung
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
                'Schaltparameter initialisieren
                Aaufi = 0.3
                Baufi = 0.3
                Caufi = 0.4
                Aobi = 0.18
                Bobi = 0.28
                Cobi = 0.46
        End Select

        'Theoretische Höchstgeschwindigkeit [m/s]
        GVmax = VEH.nNenn * VEH.Dreifen * Math.PI / (VEH.AchsI * VEH.Igetr(VEH.ganganz) * 60)

        'HEV
        SOC0 = BAT.SOC
        FirstSec = True
        StateChange = True
        ICElock = tICElock.NoLock
        LockCount = 0
        LastPeICE = 0
        LastHEVmode = tHEVparMode.Undefined
        LastHEVnotPossible = True
        LastEngState = tEngState.Idle
        BatLvl = tBatLvl.OK
        Under_vEVu = False
        Under_vEVo = False

        jz = -1
        TzyklOgl = MODdata.tDim

        '***********************************************************************************************
        '***********************************    Zeitschleife    ****************************************
        '***********************************************************************************************

        Do
            jz += 1
            GVset = False

            'Sekundäre Progressbar
            If NotAdvMode Then ProgBarCtrl.ProgJobInt = CInt(100 * jz / MODdata.tDim)

            '   Zustand bestimmen

lbGschw:

            'Geschw. / Beschl. berechnen-------------------
            'Jetzt durch DRI-Klasse
            Vist = Vh.V(jz)
            aist = Vh.a(jz)

            'Wenn Geschw. über theoretischer Höchstgeschw. => Reduzieren
            If Vist > GVmax + 0.0001 And Not GVset Then
                Vh.SetSpeed0(jz, GVmax)
                GVset = True
                GoTo lbGschw
            End If

            'Aus Leistg-----
            If aist < 0 Then
                If (Vist < 0.025) Then
                    'Vh.SetSpeed(jz, 0)
                    'GoTo lbGschw
                    Vist = 0
                End If
            End If
            '---------------

            '*******************************************************************************
            '*************************** Fahrzustand bestimmen *****************************

            'Fahrzustand bestimmen-------------------------
            Pplus = False
            Pminus = False

            If Vist < 0.0001 Then
                VehState0 = tVehState.Stopped
            Else
                If aist >= 0.1 Then
                    VehState0 = tVehState.Acc
                ElseIf aist < -0.1 Then
                    VehState0 = tVehState.Dec
                Else
                    VehState0 = tVehState.Cruise
                End If
            End If


            '*******************************************************************************
            '************************************* HEV *************************************
            Select Case BatLvl
                Case tBatLvl.Full, tBatLvl.High
                    SOCo = 0.95 * HEV.SOC_OW
                    SOCu = 0.95 * HEV.SOC_UW
                Case tBatLvl.Empty, tBatLvl.Low
                    SOCo = 1.05 * HEV.SOC_OW
                    SOCu = 1.05 * HEV.SOC_UW
                Case Else
                    SOCo = 1.05 * HEV.SOC_OW
                    SOCu = 0.95 * HEV.SOC_UW
            End Select

            Select Case SOC0
                Case Is >= BAT.SOC_MAX
                    BatLvl = tBatLvl.Full
                Case Is > SOCo
                    BatLvl = tBatLvl.High
                Case Is > SOCu
                    BatLvl = tBatLvl.OK
                Case Is > BAT.SOC_MIN
                    BatLvl = tBatLvl.Low
                Case Else
                    BatLvl = tBatLvl.Empty
            End Select

            ModeCheck(tHEVparMode.Assist) = False
            ModeCheck(tHEVparMode.LPI) = False
            ModeCheck(tHEVparMode.EV) = False
            ModeCheck(tHEVparMode.ICEonly) = False
            ModeCheck(tHEVparMode.Rekup) = False
            NeedBoost = False
            HEVmode0 = tHEVparMode.Undefined
            ICEclutch = False
            EMclutch = False
            PeICE = 0
            PeEM = 0
            EVcrit = False
            KeFail = False

            If Under_vEVu Then
                If Vist >= 1.05 * HEV.vEVu Then Under_vEVu = False 'Größer-Gleich damit bei vEVu = 0 immer Under_vEVu = False
            Else
                If Vist < 0.95 * HEV.vEVu Then Under_vEVu = True
            End If

            If Under_vEVo Then
                If Vist >= 1.05 * HEV.vEVo Then Under_vEVo = False 'Größer-Gleich damit bei vEVo = 0 immer Under_vEVu = False
            Else
                If Vist < 0.95 * HEV.vEVo Then Under_vEVo = True
            End If

            If Not FirstSec Then StateChange = (VehState0 <> MODdata.VehState(jz - 1))

            '********************* PvorD = PanRad ************************

            PvorD = fPvD(jz)

            Select Case PvorD
                Case Is > 0.0001
                    Pplus = True
                Case Is < -0.0001
                    Pminus = True
            End Select

            '************************************ Gangwahl ************************************
            If VehState0 = tVehState.Stopped Then
                Gear = 0
                Clutch = tEngClutch.Opened
            Else
                'Checken ob Kupplung schleift (wichtig für Schaltmodell):
                If fnn(Vist, 1, False) < Kuppln_norm And Pplus Then
                    Clutch = tEngClutch.Slipping
                Else
                    Clutch = tEngClutch.Closed
                End If

                If Gvorg Then
                    'Gang-Vorgabe
                    Gear = Math.Min(Vh.GearVorg(jz), VEH.ganganz)
                ElseIf Nvorg Then
                    'Drehzahlvorgabe
                    Gear = fGearByU(MODdata.nUvorg(jz), Vist)
                ElseIf VEH.ganganz = 1 Then
                    Gear = 1
                Else
                    If GnachV Then
                        'Gang nach Geschwindigkeit
                        Gear = fGearBySpeed(jz)
                    Else
                        'Schaltmodell
                        If PKWja Then
                            Gear = fGearPKW(jz)
                        Else
                            Gear = fGearLKW(jz)
                        End If
                    End If
                End If
            End If

            If Gear = -1 Then
                WorkerMsg(tMsgID.Err, "Error in Gear Shift Model!", MsgSrc & "/t= " & jz + 1)
                Return False
            End If

            'Nebenverbrauch bestimmen (aus VEH und DRI)
            Paux = fPaux(jz, 0)

            'HEV-Teil kommt erst nach Gangwahl weil Getr./Diff-Loss den benötigt und EM zwischen ICE und GB liegt
            If VehState0 = tVehState.Stopped Then

                PeICE = 0
                PeEM = 0
                ICEclutch = False
                EMclutch = False

                'Nebenverbrauch bestimmen (aus VEH und DRI)
                Paux = fPaux(jz, 0)

            Else

                If Nvorg Then
                    'Wenn Drehzahl vorgegeben
                    nU = MODdata.nUvorg(jz)
                Else
                    'Sonst aus Vist und Gear
                    nU = fnU(Vist, Gear, Clutch = tEngClutch.Slipping)
                End If

                'Normierte Drehzahl
                nn = (nU - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

                'Maximale Leistung der ICE
                If jz = 0 Then
                    ICEmax = FLD.Pfull(nn)
                Else
                    ICEmax = FLD.Pfull(nn, MODdata.Pe(jz - 1))
                End If

                ICEmin = FLD.Pdrag(nn)

                'Nebenverbrauch bestimmen (aus VEH und DRI)
                Paux = fPaux(jz, nU)

                'Maximal zulässige Batterieleistung
                BAT.Bat_Pzul(jz)
                PeBatMax = Math.Max(BAT.PmaxAntr - Paux, 0)     'Paux reduziert maximale Entlade-Leistung
                PeBatMin = Math.Min(BAT.PmaxLaden - Paux, 0)    'Paux erhöht (!) maximale Lade-Leistung

                'Maximale EM-Leistung. (Batterieleistung limitiert EM-Leistung)
                EMmax = EMO.PeMax(nU)
                Do While EMO.PiEM(nU, EMmax) > PeBatMax
                    EMmax *= 0.995
                Loop

                EMmin = EMO.PeMin(nU)
                Do While EMO.PiEM(nU, EMmax) < PeBatMin
                    EMmax *= 0.995
                Loop

                'Leistung bis ICE/EM (= an Kupplung) berechnen
                PlossGB = fPlossGB(PvorD, Vist, Gear)
                PlossDiff = fPlossDiff(PvorD, Vist)
                PlossRt = fPlossRt(Vist, Gear)
                PaGetr = fPaG(Vist, aist)

                'Leistung an Kupplung
                Pkup = PvorD + PlossGB + PlossDiff + PaGetr + PlossRt

                'PaMot
                If Nvorg Then
                    'Drehzahlvorgabe
                    PaICE = (VEH.I_mot * MODdata.dnUvorg(jz) * 0.01096 * MODdata.nUvorg(jz)) * 0.001
                    PaEM = (EMO.I_mot * MODdata.dnUvorg(jz) * 0.01096 * MODdata.nUvorg(jz)) * 0.001
                Else
                    PaICE = ((VEH.I_mot * (VEH.AchsI * VEH.Igetr(Gear) / (0.5 * VEH.Dreifen)) ^ 2) * aist * Vist) * 0.001
                    PaEM = ((EMO.I_mot * (VEH.AchsI * VEH.Igetr(Gear) / (0.5 * VEH.Dreifen)) ^ 2) * aist * Vist) * 0.001
                End If

                '***** Notwendige Leistung im EV-Betrieb
                '   Leistung an Kupplung plus EM-Trägheit
                PeEV = Pkup + PaEM
                '! ACHTUNG: Wenn ICE eingekuppelt dann muss PaICE auch noch dazu ! => Später bei Leistungsverteilung

                '***** Notwendige Leistung im ICE+EM-Betrieb
                PeKomb = Pkup + PaICE + PaEM

                '***** Notwendige Leistung im ICE-Betrieb
                PeICEonly = Pkup + PaICE

                '***** Check ob EV möglich
                '   => Bat <> Low
                '   => EM-Leistung >= Antriebsleistung
                '   => v < vEVo
                If BatLvl <> tBatLvl.Empty And BatLvl <> tBatLvl.Low And PeEV <= EMmax And Under_vEVo Then ModeCheck(tHEVparMode.EV) = True

                '***** Falls EV möglich: Check ob kritisch
                If ModeCheck(tHEVparMode.EV) And PeEV > 0.9 * EMmax Then EVcrit = True

                '***** Check ob Assist / LPI / ICEonly möglich und ob Boost erforderlich
                '   => ICE-Ein muss möglich sein (ICElock)
                If ICElock <> tICElock.OffLock Then

                    'Assist / Boost
                    '   => SOC > MIN
                    If BatLvl <> tBatLvl.Empty Then
                        ModeCheck(tHEVparMode.Assist) = True
                        'Boost
                        '   => ICE an Volllast
                        If PeICEonly > ICEmax Then NeedBoost = True
                    End If

                    'LPI
                    '   => Bat <> High
                    If BatLvl <> tBatLvl.High Then ModeCheck(tHEVparMode.LPI) = True

                    'ICEonly
                    ModeCheck(tHEVparMode.ICEonly) = True

                End If

                '***** Check ob Rekup möglich
                ModeCheck(tHEVparMode.Rekup) = (BatLvl <> tBatLvl.Full)
                Pbrake = 0


                '***********************************************************************************************
                '********************************* Fahrzustands-Unterscheidung *********************************
                '***********************************************************************************************

                If Pplus Then

                    '****************************************************************************************
                    '***************************** Betriebsmodus Vor-Auswahl ********************************

                    If NeedBoost Then

                        'Wenn Boost notwendig (und möglich), dann keine Wahl
                        HEVmode0 = tHEVparMode.Assist

                    ElseIf ModeCheck(tHEVparMode.EV) Then

                        'EV Mode wenn...
                        If Not ModeCheck(tHEVparMode.ICEonly) Or Under_vEVu Or BatLvl = tBatLvl.High Then  'ICE nicht möglich, Vist unter EVu oder Batterieladung hoch 

                            HEVmode0 = tHEVparMode.EV

                        End If

                    Else

                        'Wenn EV & ICE nicht möglich dann EV-Modus bis ICE wieder startet.... sollte nie vorkommen weil keine ICE-Abschaltung wenn wenig SOC
                        If Not ModeCheck(tHEVparMode.ICEonly) Then

                            HEVmode0 = tHEVparMode.EV

                        End If

                    End If


                    'Falls Einsatz von HEV-Strategie:
                    '   Ke's berechnen
                    If HEVmode0 = tHEVparMode.Undefined Then

                        '************** Optimale Ke's berechnen und Leistungen dazu ************

                        'Emission/Verbrauch in g/h
                        FCbasis = MAP.GetSingleValue(tMapComp.FC, nn, PeICEonly / VEH.Pnenn)

                        '***********************************************************************
                        '******************************* KeSTE *********************************
                        '***********************************************************************

                        'KeSTE aus STE-Kurve...
                        KeSTE = HEV.STEintp(SOC0)


                        '***********************************************************************
                        '******************************** KeA **********************************
                        '***********************************************************************

                        If ModeCheck(tHEVparMode.Assist) Then
                            bAbort = False
                            Pel = 0
                            PelOpt = 0
                            PeKeAoptICE = 0
                            KeAmin = 999999
                            Do While Pel + PelStep < EMmax
                                Pel += PelStep
                                Pvkm = PeKomb - Pel

                                'Unter Max-Pe bleiben
                                If Pvkm > ICEmax Then Continue Do

                                'Wenn Pvkm negativ: Berechnung für reinen EM-Betrieb
                                If Pvkm <= 0 Then

                                    '    ...nicht gültig falls Batlvl <= Low oder ICEonLock
                                    If Not ModeCheck(tHEVparMode.EV) Or ICElock = tICElock.OnLock Then Exit Do

                                    'EM-Leistung = P-Antrieb
                                    Pel = PeEV
                                    Pvkm = 0

                                    'Verbrauchseinsparung in g/h 
                                    FCminus = FCbasis

                                    'Abbruch nach dieser Berechnung (mehr EM-Leistung macht keinen Sinn da hier schon rein elektrischer Antrieb)
                                    bAbort = True

                                Else

                                    'Verbrauchseinsparung in g/h 
                                    FCminus = FCbasis - MAP.GetSingleValue(tMapComp.FC, nn, Pvkm / VEH.Pnenn)

                                End If

                                'Leistung nach E-Motor in kW
                                Pmot = EMO.PiEM(nU, Pel)

                                If Pmot > PeBatMax Then
                                    If bAbort Then
                                        Exit Do
                                    Else
                                        Continue Do
                                    End If
                                End If

                                'Leistung in Batterie
                                Pbat = BAT.PiBat(Pmot)

                                If Pbat = 0 Then Stop

                                'Div/0 und Vorzeichen-Check
                                If FCminus > 0 Then

                                    'KeA berechnen in kWh/kg
                                    KeA = 1000 * Pbat / FCminus

                                    'Check ob Optimum
                                    If KeA < KeAmin Then
                                        KeAmin = KeA
                                        PelOpt = Pel
                                        PeKeAoptICE = Pvkm
                                    End If
                                End If

                                'Abbruch falls reiner EM-Betrieb schon erreicht
                                If bAbort Then Exit Do

                            Loop

                            PeKeAoptEM = PelOpt

                            KeA = KeAmin

                            DeltaA = KeSTE - KeA

                        Else

                            DeltaA = -1

                        End If


                        '***********************************************************************
                        '******************************** KeG **********************************
                        '***********************************************************************

                        If ModeCheck(tHEVparMode.LPI) Then
                            bAbort = False
                            Pel = 0
                            PelOpt = 0
                            PeKeGoptICE = 0
                            KeGmax = 0
                            Do While Pel - PelStep > EMmin

                                Pel -= PelStep
                                Pvkm = PeKomb - Pel

                                'Falls Pvkm an Volllast:
                                If Pvkm > 0.99 * ICEmax Then

                                    'Pvkm auf Volllast setzen
                                    Pvkm = 0.99F * ICEmax

                                    'Elektromotor in Generatorbetrieb (Pges1 - Pvkm < 0)
                                    Pel = PeKomb - Pvkm

                                    'Abbruch nach diesem Durchgang weil mehr Generieren nicht möglich
                                    bAbort = True

                                End If

                                'Zusatzverbrauch in g/h 
                                FCplus = MAP.GetSingleValue(tMapComp.FC, nn, Pvkm / VEH.Pnenn) - FCbasis

                                'Leistung nach E-Motor in kW
                                Pmot = EMO.PiEM(nU, Pel)

                                If Pmot < PeBatMin Then
                                    If bAbort Then
                                        Exit Do
                                    Else
                                        Continue Do
                                    End If
                                End If

                                'Leistung in Batterie
                                Pbat = BAT.PiBat(Pmot)

                                If Pbat = 0 Then Exit Do

                                'Div/0 und Vorzeichen-Check
                                If FCplus > 0 Then

                                    'KeG berechnen in kWh/kg
                                    KeG = -1000 * Pbat / FCplus

                                    'Check ob Optimum
                                    If KeG > KeGmax Then
                                        KeGmax = KeG
                                        PelOpt = Pel
                                        PeKeGoptICE = Pvkm
                                    End If

                                End If

                                'Abbruch falls VKM-Volllast schon erreicht
                                If bAbort Then Exit Do

                            Loop

                            PeKeGoptEM = PelOpt

                            KeG = KeGmax

                            'Abstand Eta zu Kurve berechnen
                            DeltaG = KeG - KeSTE

                        Else

                            DeltaG = -1

                        End If

                        '***********************************************************************
                        '********************** KeSTE, Deltas auswerten ************************
                        '***********************************************************************

                        If DeltaG < 0 And DeltaA < 0 Then
                            KeFail = True
                        Else
                            If (DeltaA > DeltaG) And ModeCheck(tHEVparMode.Assist) Then
                                HEVmodeByKe = tHEVparMode.Assist
                            ElseIf DeltaG > 0 And ModeCheck(tHEVparMode.LPI) Then
                                HEVmodeByKe = tHEVparMode.LPI
                            Else
                                KeFail = True
                            End If
                        End If

                        If KeFail Then
                            If ModeCheck(tHEVparMode.EV) Then
                                If LastHEVmode = tHEVparMode.EV Or LastEngState = tEngState.Stopped Or LastEngState = tEngState.Idle Then
                                    HEVmodeByKe = tHEVparMode.EV
                                Else
                                    HEVmodeByKe = tHEVparMode.ICEonly
                                End If
                            Else
                                HEVmodeByKe = tHEVparMode.ICEonly
                            End If
                        End If


                        '***********************************************************************
                        '************************* Betriebsstrategie ***************************
                        '***********************************************************************

                        'LastHEVmode-Check
                        If Not FirstSec Then


                            Select Case LastHEVmode
                                Case tHEVparMode.Undefined, tHEVparMode.Rekup
                                    LastHEVnotPossible = True
                                    PvkmAlt = 0
                                Case Else
                                    LastHEVnotPossible = Not ModeCheck(LastHEVmode)

                                    Select Case LastHEVmode

                                        Case tHEVparMode.Assist
                                            PvkmAlt = PeKeAoptICE
                                        Case tHEVparMode.LPI
                                            PvkmAlt = PeKeGoptICE
                                        Case Else    ' tHEVparMode.ICEonly
                                            PvkmAlt = PeICEonly

                                    End Select

                            End Select

                        End If

                        '** Ke-Modus einsetzen wenn...
                        If HEVmodeByKe = LastHEVmode Or StateChange Or LastHEVnotPossible Then

                            '...Gleicher Modus wie vorher, Fahrzustands-Änderung oder Letzter Modus unmöglich
                            HEVmode0 = HEVmodeByKe

                        Else

                            '...wenn Motor nicht läuft
                            If LastEngState = tEngState.Stopped Then

                                HEVmode0 = HEVmodeByKe

                                '...wenn PeICE-Leistungsänderung mit neuen Modus am geringsten
                            Else

                                Select Case HEVmodeByKe
                                    Case tHEVparMode.Assist
                                        Pvkm = PeKeAoptICE
                                    Case tHEVparMode.LPI
                                        Pvkm = PeKeGoptICE
                                    Case tHEVparMode.ICEonly
                                        Pvkm = PeICEonly
                                End Select

                                If Math.Abs(Pvkm - LastPeICE) < Math.Abs(PvkmAlt - LastPeICE) Then
                                    HEVmode0 = HEVmodeByKe
                                Else
                                    HEVmode0 = LastHEVmode
                                End If

                            End If

                        End If

                    End If

                    '********************************************************************************************
                    '****************************** Leistungen je nach Modus verteilen **************************
                    '********************************************************************************************

                    Select Case HEVmode0

                        Case tHEVparMode.EV

                            'EM übernimmt gesamte Leistung
                            PeEM = PeEV

                            'Geschw. reduzieren falls Leistung zu hoch für EM oder Bat
                            If PeEM > EMmax Or EMO.PiEM(nU, PeEM) > PeBatMax Then
                                Vh.ReduceSpeed(jz, 0.995)
                                GoTo lbGschw
                            End If

                            'Falls ICElock oder EVcrit dann ICE ein (aber ausgekuppelt)
                            If ICElock = tICElock.OnLock Or (EVcrit And ICElock <> tICElock.OffLock) Then
                                EngState0 = tEngState.Idle
                            Else
                                EngState0 = tEngState.Stopped
                            End If

                            PeICE = 0

                            ICEclutch = False
                            EMclutch = True

                        Case tHEVparMode.ICEonly

                            'ICE übernimmt gesamten Antrieb
                            If PeICEonly > ICEmax Then
                                Vh.ReduceSpeed(jz, 0.995)
                                GoTo lbGschw
                            End If

                            PeEM = 0

                            PeICE = PeICEonly

                            EngState0 = tEngState.Load

                            ICEclutch = True
                            EMclutch = False

                        Case tHEVparMode.Assist

                            If NeedBoost Then

                                If EMmax + PeKeAoptICE > PeKomb Then

                                    PeICE = PeKeAoptICE
                                    PeEM = PeKomb - PeICE

                                ElseIf EMmax + ICEmax > PeKomb Then

                                    PeICE = ICEmax
                                    PeEM = PeKomb - PeICE

                                Else

                                    Vh.ReduceSpeed(jz, 0.995)
                                    GoTo lbGschw

                                End If

                            Else

                                PeICE = PeKeAoptICE
                                PeEM = PeKeAoptEM

                            End If

                            EngState0 = tEngState.Load

                            ICEclutch = True
                            EMclutch = True

                        Case tHEVparMode.LPI

                            PeICE = PeKeGoptICE
                            PeEM = PeKeGoptEM

                            EngState0 = tEngState.Load

                            ICEclutch = True
                            EMclutch = True

                    End Select

                    If EngState0 = tEngState.Load Then
                        If Math.Abs(P / ICEmax - 1) < 0.02 Then EngState0 = tEngState.FullLoad
                    End If

                ElseIf Pminus Then

                    HEVmode0 = tHEVparMode.Rekup

                    'ACHTUNG: ICEclutch ist standardmäßig 'False' deshalb hier keine weiteren Statements mehr

                    'Maximale Rekup-Leistung berechnen
                    PrekupMax = fPrekupMax()

                    If PrekupMax > PvorD Then 'Falls Rekup-Limit erreicht

                        'Mit PrekupMax auf EM/ICE zurück rechnen
                        PlossGB = fPlossGB(PrekupMax, Vist, Gear)
                        PlossDiff = fPlossDiff(PrekupMax, Vist)
                        PlossRt = fPlossRt(Vist, Gear)
                        Pkup = PvorD + PlossGB + PlossDiff + PaGetr + PlossRt
                        PeKomb = Pkup + PaICE + PaEM
                        PeICEonly = Pkup + PaICE
                        PeEV = Pkup + PaEM

                        'Den Rest gleich auf die Bremse 
                        Pbrake = PvorD - PrekupMax

                    End If

                    'Default für ICE (damit man sich die "Else"-Statements spart)
                    PeICE = 0
                    EngState0 = tEngState.Stopped

                    If ModeCheck(tHEVparMode.Rekup) Then    'Falls Rekuperation möglich...

                        If ICElock = tICElock.OnLock Then   'Falls ICElock, dann ICE-Schub dazu wenn EM nicht ausreicht. Basis ist immer PeKomb

                            'EM-Leistung berechnen
                            PeEM = PeKomb

                            If PeEM < EMmin Then    'Falls maximale EM-Max-Rekup überschritten...

                                'EM-Leistung neu
                                PeEM = EMmin

                                'Restliche Leistung an ICE
                                PeICE = PeKomb - PeEM
                                EngState0 = tEngState.Drag

                                'Falls ICE über Schleppkurve
                                If PeICE < ICEmin Then

                                    'ICE-Leistung neu
                                    PeICE = ICEmin

                                    'Rest an Bremse
                                    Pbrake += PeKomb - PeEM - PeICE

                                End If

                            Else    'Falls EM-Max-Rekup nicht überschritten

                                'ICE in Leerlauf (weil On-Lock)
                                EngState0 = tEngState.Idle

                            End If

                            ICEclutch = True

                        Else    'Falls nicht ICElock, dann ICE-Schub nur dazu wenn ICE nicht aus. Basis ist PeEV

                            'EM-Leistung berechnen
                            PeEM = PeEV

                            If PeEM < EMmin Then    'Falls maximale EM-Max-Rekup überschritten...

                                'EM-Leistung neu
                                PeEM = EMmin

                                If LastEngState <> tEngState.Stopped And PeKomb < EMmin Then    'Falls ICE nicht aus und kombinierte Leistung unter EM-Max-Rekup dann ICE in Schub

                                    'ICE im Schubbetrieb
                                    PeICE = PeKomb - PeEM
                                    EngState0 = tEngState.Drag

                                    'Falls ICE über Schleppkurve
                                    If PeICE < ICEmin Then

                                        'ICE-Leistung neu
                                        PeICE = ICEmin

                                        'Rest an Bremse
                                        Pbrake += PeKomb - PeEM - PeICE

                                    End If

                                    ICEclutch = True

                                Else    'Falls ICE nicht verfügbar dann mechanisch dazu bremsen

                                    Pbrake += PeEV - PeEM

                                End If

                            End If

                        End If

                        EMclutch = True

                    Else    'Falls Rekuperation nicht möglich

                        If LastEngState <> tEngState.Stopped Then  'Falls ICE nicht aus dann Schub

                            'ICE im Schubbetrieb
                            PeICE = PeICEonly
                            EngState0 = tEngState.Drag

                            'Falls ICE über Schleppkurve
                            If PeICE < ICEmin Then

                                'ICE-Leistung neu
                                PeICE = ICEmin

                                'Rest an Bremse
                                Pbrake += PeKomb - PeEM - PeICE

                            End If

                        Else    'Falls ICE aus dann alles mechanisch bremsen

                            Pbrake = PvorD

                        End If


                    End If

                    If EngState0 = tEngState.Drag Then
                        If Math.Abs(P / ICEmin - 1) < 0.02 Then EngState0 = tEngState.FullDrag
                    End If

                Else

                    'Leistung Null
                    PeICE = 0
                    PeEM = 0
                    ICEclutch = False
                    EMclutch = False

                End If

            End If

            '************************************************************************************
            '****************************** Clutch und Drehzahlen *******************************
            '************************************************************************************

            'Haupt-Kupplung => muss hier schon bekannt sein!
            'If ICEclutch Then
            '    If PeICE > 0 And fnn(Vist, Gear, False) < Kuppln_norm Then
            '        Clutch = tEngClutch.Slipping
            '    Else
            '        Clutch = tEngClutch.Closed
            '    End If
            'ElseIf EMclutch Then
            '    Clutch = tEngClutch.Closed
            'Else
            '    Clutch = tEngClutch.Opened
            'End If

            '************************************ Drehzahl ICE ************************************
            If ICEclutch Then

                nnICE = fnn(Vist, Gear, Clutch = tEngClutch.Slipping)
                nUICE = nnICE * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl

            Else

                If EngState0 = tEngState.Stopped Then
                    nU0 = 0
                ElseIf EngState0 = tEngState.Idle Then
                    nU0 = VEH.nLeerl
                Else

                    Stop

                End If


                If FirstSec Then
                    nUICE = nU0
                Else

                    If Math.Abs(nU0 - MODdata.nU(jz - 1)) < 0.00001 Then

                        nUICE = MODdata.nU(jz - 1)

                    Else

                        nUICE = MODdata.nU(jz - 1)
                        P = FLD.Pdrag(MODdata.nn(jz - 1))
                        omega1 = nUICE * 2 * Math.PI / 60
                        M = -P * 1000 * 60 / (2 * Math.PI * nUICE)
                        omega_p = M / VEH.I_mot
                        omega2 = omega1 - omega_p
                        nUICE = Math.Max(nU0, omega2 * 60 / (2 * Math.PI))

                    End If

                End If

                nnICE = (nUICE - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)

            End If

            '************************************ Drehzahl EM *************************************
            If EMclutch Then
                nUEM = fnU(Vist, Gear, Clutch = tEngClutch.Slipping)
            Else
                nUEM = 0
            End If

            '**************************************************************************************
            '********************************* EM => Batterie *************************************
            '**************************************************************************************

            PiEM = EMO.PiEM(nUEM, PeEM)

            PeBat0 = Paux + PiEM

            BAT.Bat_Calc(PeBat0, jz)

            PiBat0 = PeBat0 + BAT.PbatV


            '-------------------------------------------------------
            '------------------------- PNR -------------------------
            '-------------------------------------------------------

            If PHEMworker.CancellationPending Then Return True

            '   Sekunde abschließen

            If FirstSec Then FirstSec = False

            'ICE-Lock
            '   ACHTUNG: Die beiden If-Schleifen nicht verbinden weil LockCount sich sonst verzählt
            If ICElock <> tICElock.NoLock Then
                LockCount += 1
                If LockCount > GEN.StStT Then
                    LockCount = 0
                    ICElock = tICElock.NoLock
                End If
            End If

            If ICElock = tICElock.NoLock Then
                If EngState0 = tEngState.Stopped Then
                    If LastEngState <> tEngState.Stopped Then ICElock = tICElock.OffLock
                Else
                    If LastEngState = tEngState.Stopped Then ICElock = tICElock.OnLock
                End If
            End If

            'Modalwerte-Felder schreiben
            MODdata.Pe.Add(PeICE / VEH.Pnenn)
            MODdata.nn.Add(nnICE)
            MODdata.nU.Add(nUICE)
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
            MODdata.PaEng.Add(PaICE)
            MODdata.PaGB.Add(PaGetr)

            MODdata.VehState.Add(VehState0)
            MODdata.Gear.Add(Gear)

            PeEMot.Add(PeEM)
            PiEMot.Add(PiEM)
            PeBat.Add(PeBat0)
            PiBat.Add(PiBat0)
            Ubat.Add(BAT.Ubat)
            Ibat.Add(BAT.Ibat)
            SOC.Add(BAT.SOC)
            TempBat.Add(BAT.TempBat)

            SOC0 = BAT.SOC


            If Cfg.WegKorJa Then Vh.DistCorrection(jz)


            LastEngState = EngState0
            LastHEVmode = HEVmode0
            LastPeICE = PeICE

            If Vh.Vsoll(jz) - Vist > 1.5 Then SecSpeedRed += 1

            'Meldungen
            If MODdata.ModErrors.MsgOutputAbort(jz + 1, MsgSrc) Then Return False

        Loop Until jz >= MODdata.tDim

        '***********************************************************************************************
        '*********************************    Zeitschleife ENDE   **************************************
        '***********************************************************************************************

        'Meldungen (Nicht ADVANCE)
        If NotAdvMode Then

            If Cfg.WegKorJa Then
                If MODdata.tDim > TzyklOgl Then WorkerMsg(tMsgID.Warn, "Cycle extended by " & MODdata.tDim - TzyklOgl & " seconds.", MsgSrc)
            End If

            If SecSpeedRed > 0 Then WorkerMsg(tMsgID.Warn, "Speed reduction > 1.5 m/s in " & SecSpeedRed & " time steps.", MsgSrc)

        End If

        'CleanUp
        Vh = Nothing

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
        eps = 0.001
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

            If v < vmin Then

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


#Region "Schaltmodelle"

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

        '-----------------------------------Sekunde 1--------------------------------------
        'Erste Sekunde: Gang finden / Initialisierung
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

        '--------------------------------Ab Sekunde 2--------------------------------------

        '---------Startwerte---------
        'gangX = Letzter Gang d.h. Ausgangsbasis für Schaltmodell
        LastGear = MODdata.Gear(t - 1)
        gangX = LastGear
        Gear = LastGear
        t1 = MODdata.tDim
        itgangwL = -1

        'Kuppelschleif-check  << Schon passiert in Power.Calc
        ''If bPplus And fn_norm(1) < Kuppln_norm Then bKupplSchleif = True

        '-------------------Gang für die nächsten 6 Sekunden berechnen---------------------
        '-------------------------------------------------------------

        Pvorher = MODdata.Pe(t - 1)

        tx = t
        Do
            '-----------Schaltfunktion----------
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
                    Pjetzt = fPeGearMod(gangX, t)
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

            'Drehzahlschranke Hinaufschalten  n_normiert (leerl = 0, Nenndrehzahl = 1)
            nnsaufi = Aaufi + Baufi * V_norm + Caufi * AP10
            If (nnsaufi > 0.95) Then nnsaufi = 0.95
            'Drehzahlschranke Hinunterschalten  n_normiert (leerl = 0, Nenndrehzahl = 1)
            nnsobi = Aobi + Bobi * V_norm + Cobi * AP10
            'Gelöscht LUZ 13.07.10: If (nnsaufi > 0.85) Then nnsaufi = 0.85
            'Umrechnung in hier verwendete Drehzahleinheit (n/n_nenn):
            nsa = (VEH.nLeerl / VEH.nNenn) + nnsaufi * (1 - (VEH.nLeerl / VEH.nNenn))
            nsd = (VEH.nLeerl / VEH.nNenn) + nnsobi * (1 - (VEH.nLeerl / VEH.nNenn))
            'Drehzahl mit letzten Gang (gangX)
            'nx = fnU(Vist, gangX, Clutch = tEngClutch.Slipping) / VEH.nNenn
            '-----------------------------------

            ' ''Schalten Maximal alle 2 Sekunden schalten zulaessig:
            If (t - itgangwL) < 3 And itgangwL > -1 Then GoTo lb10

            'Ueberpruefen ob hinunterschalten, nur wenn Geschw. abnimmt oder leistung zunimmt
            bCheck = False
            Pjetzt = fPeGearMod(gangX, t)
            If Vist0 < Valt Then bCheck = True
            If (Pjetzt > Pvorher) Then bCheck = True
            If bCheck Then
                If nx < nsd Then gangX -= 1
            End If

            'Ueberpruefen ob hinaufschalten, nur wenn Geschw. zunimmt oder leistung abnimmt
            bCheck = False
            If (Vist0 > Valt) Then bCheck = True
            If (Pjetzt < Pvorher) Then bCheck = True
            If bCheck Then
                If nx > nsa Then gangX += 1
            End If

            'Gangwahl korrigieren
            If gangX > VEH.ganganz Then
                gangX = VEH.ganganz
            ElseIf gangX < 1 Then
                gangX = 1
            End If

lb10:

            'Kein Leerlauf wenn Leistung > 0
            If gangX = 0 Then
                If Pjzx > 0.001 Then gangX = 1
            End If

            'Drehzahl neu
            ''nn = fnn(Vist, gangX, Clutch = tEngClutch.Slipping)
            nn = fnn(Vist0, gangX, False)

            'Überprüfe ob Gang innerhalb der Leistungs-/Drehzahl Grenzen. Schleppbetrieb wird nicht beachtet
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

            'Speichere Gänge in Feld für spätere Checks
            Gcheck(t - tx) = gangX

            If gangX <> Gear Then itgangwL = t

            Gear = gangX

            t += 1

        Loop Until t = tx + 11 Or t > t1
        t = tx
        '-------------------------------------------------------------
        'Gang wird übernommen
        gangX = Gcheck(0)
        For ix = t To t + 10
            Gears(ix - t) = Gcheck(ix - t)
        Next

        Gear = Gears(0)

        'Gang-Verlauf hinzufügen

        '----------------------------------------------------------------------------------

        '--------------------------------Checks Teil 1-------------------------------------
        'Checks zur Saeuberung unsinniger Schlatmanoever:

        ''Unterteilung in "iphase(j)" Beschleunigungs- (=1), Verzoegerungs- (=2) und Cruise (=3) -Phasen:
        'iphase = 0
        'Select Case (beschl(jz - 2) + beschl(jz - 1) + beschl(jz)) / 3
        '    Case Is >= 0.125
        '        iphase = 1
        '    Case Is <= -0.125
        '        iphase = 2
        '    Case Else
        '        iphase = 3
        'End Select
        '   ============>> Schon bestimmt durch VehState0

        'Suche nach letztem Gangwechsel
        itgangwL = -1
        If t > 2 Then
            For ix = t - 1 To 1 Step -1
                If MODdata.Gear(ix) <> MODdata.Gear(ix - 1) Then
                    itgangwL = ix
                    Exit For
                End If
            Next
        End If

        'Schaltvorgaenge max. alle 3 Sekunden zulaessig:
        If t - itgangwL <= 2 And t > 2 And LastGear <> 0 Then
            Return LastGear    '<<< keine weiteren Checks!!!
        End If

        If Gear <> LastGear Then
            'Cruise-Phasen:
            'Solange Geschwindigkeitsaenderung seit letztem Gangwechsel unter 6% und Pe/Pnenn aenderung unter 6% wird nicht geschaltet:
            'Verzoegerungsphasen: Hochschalten wird unterdrückt
            'Beschleunigungsphasen: Zurückschalten wird unterdrückt
            If itgangwL = -1 Then itgangwL = 0
            bCheck = False
            Pjetzt = fPeGearMod(Gear, t)
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
                'Wenn innerhalb von 6 Sekunden wieder in vorigen Gang zurueck geschaltet wird, wird der vorige Gang
                'durchgehend beibehalten
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
                    'Wenn innerhalb von 6 Sekunden einmal höher und einmal niedriger als voriger Gang
                    'geschaltet wird, wird voriger Gang durchgehend beibehalten
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

        '--------------------------------Checks Teil 2-------------------------------------
        'Schalten von 2. in 1. Gang wird bei v > 2,5 m/s unterdrueckt
        'NEU LUZ 040210: Hochschalten nur wenn im 2. Gang über Kuppeldrehzahl
        If Gear = 1 And LastGear > 1 And Vist >= 2.5 Then
            If fnn(Vist, 2, False) > Kuppln_norm Then Gear = 2
        End If

        'bei verzoegerungsvorgaengen unter 2,5 m/s wird in Leerlauf geschaltet
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

        'wenn v mehr als 1 Sek. < 0.1 m/s wird auf Gang=0 geschaltet
        If t < t1 Then
            If (Vist < 0.1 And MODdata.Vh.V(t + 1) < 0.1) Then
                Return 0    '<<< keine weiteren Checks!!!
            End If
        End If

        'bei Beschleunigungsvorgaengen unter 1,5 m/s wird in 1. Gang geschaltet
        If Vist < 1.5 And t < t1 Then
            If (Vist > 0.01 + MODdata.Vh.V(t - 1) And MODdata.Vh.V(t + 1) > 0.01 + Vist) Then
                Gear = 1
            End If
        End If

        'ueberpruefung, ob Drehzahl ueber nenndrehzahl, dann muss immer hochgeschaltet werden
        'sonst fehlt die leistung!
        Do While fnn(Vist, Gear, Clutch = tEngClutch.Slipping) > 1 And Gear < VEH.ganganz
            Gear += 1
        Loop

        Return Gear

    End Function

    'EV-Schaltmodell (auf Basis PKW)
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

        '-----------------------------------Sekunde 1--------------------------------------
        'Erste Sekunde: Gang finden / Initialisierung
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

        '--------------------------------Ab Sekunde 2--------------------------------------

        '---------Startwerte---------
        'gangX = Letzter Gang d.h. Ausgangsbasis für Schaltmodell
        LastGear = MODdata.Gear(t - 1)
        gangX = LastGear
        Gear = LastGear
        t1 = MODdata.tDim
        itgangwL = -1

        'Kuppelschleif-check  << Schon passiert in Power.Calc
        ''If bPplus And fn_norm(1) < Kuppln_norm Then bKupplSchleif = True

        '-------------------Gang für die nächsten 6 Sekunden berechnen---------------------
        '-------------------------------------------------------------

        Pvorher = MODdata.Pe(t - 1)

        tx = t
        Do
            '-----------Schaltfunktion----------
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

            'Drehzahlschranke Hinaufschalten  n_normiert (leerl = 0, Nenndrehzahl = 1)
            nnsaufi = Aaufi + Baufi * V_norm + Caufi * AP10
            If (nnsaufi > 0.95) Then nnsaufi = 0.95
            'Drehzahlschranke Hinunterschalten  n_normiert (leerl = 0, Nenndrehzahl = 1)
            nnsobi = Aobi + Bobi * V_norm + Cobi * AP10
            'Gelöscht LUZ 13.07.10: If (nnsaufi > 0.85) Then nnsaufi = 0.85
            'Umrechnung in hier verwendete Drehzahleinheit (n/n_nenn):
            nsa = (VEH.nLeerl / VEH.nNenn) + nnsaufi * (1 - (VEH.nLeerl / VEH.nNenn))
            nsd = (VEH.nLeerl / VEH.nNenn) + nnsobi * (1 - (VEH.nLeerl / VEH.nNenn))
            'Drehzahl mit letzten Gang (gangX)
            'nx = fnU(Vist, gangX, Clutch = tEngClutch.Slipping) / VEH.nNenn
            '-----------------------------------

            ' ''Schalten Maximal alle 2 Sekunden schalten zulaessig:
            If (t - itgangwL) < 3 And itgangwL > -1 Then GoTo lb10

            'Ueberpruefen ob hinunterschalten, nur wenn Geschw. abnimmt oder leistung zunimmt
            bCheck = False
            Pjetzt = fPeGearEV(gangX, t)
            If Vist0 < Valt Then bCheck = True
            If (Pjetzt > Pvorher) Then bCheck = True
            If bCheck Then
                If nx < nsd Then gangX -= 1
            End If

            'Ueberpruefen ob hinaufschalten, nur wenn Geschw. zunimmt oder leistung abnimmt
            bCheck = False
            If (Vist0 > Valt) Then bCheck = True
            If (Pjetzt < Pvorher) Then bCheck = True
            If bCheck Then
                If nx > nsa Then gangX += 1
            End If

            'Gangwahl korrigieren
            If gangX > VEH.ganganz Then
                gangX = VEH.ganganz
            ElseIf gangX < 1 Then
                gangX = 1
            End If

lb10:

            'Kein Leerlauf wenn Leistung > 0
            If gangX = 0 Then
                If Pjzx > 0.001 Then gangX = 1
            End If

            'Drehzahl neu
            ''nn = fnn(Vist, gangX, Clutch = tEngClutch.Slipping)
            nn = fnn(Vist0, gangX, False)

            'Überprüfe ob Gang innerhalb der Leistungs-/Drehzahl Grenzen. Schleppbetrieb wird nicht beachtet
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

            'Speichere Gänge in Feld für spätere Checks
            Gcheck(t - tx) = gangX

            If gangX <> Gear Then itgangwL = t

            Gear = gangX

            t += 1

        Loop Until t = tx + 11 Or t > t1
        t = tx
        '-------------------------------------------------------------
        'Gang wird übernommen
        gangX = Gcheck(0)
        For ix = t To t + 10
            Gears(ix - t) = Gcheck(ix - t)
        Next

        Gear = Gears(0)

        'Gang-Verlauf hinzufügen

        '----------------------------------------------------------------------------------

        '--------------------------------Checks Teil 1-------------------------------------
        'Checks zur Saeuberung unsinniger Schlatmanoever:

        ''Unterteilung in "iphase(j)" Beschleunigungs- (=1), Verzoegerungs- (=2) und Cruise (=3) -Phasen:
        'iphase = 0
        'Select Case (beschl(jz - 2) + beschl(jz - 1) + beschl(jz)) / 3
        '    Case Is >= 0.125
        '        iphase = 1
        '    Case Is <= -0.125
        '        iphase = 2
        '    Case Else
        '        iphase = 3
        'End Select
        '   ============>> Schon bestimmt durch VehState0

        'Suche nach letztem Gangwechsel
        itgangwL = -1
        If t > 2 Then
            For ix = t - 1 To 1 Step -1
                If MODdata.Gear(ix) <> MODdata.Gear(ix - 1) Then
                    itgangwL = ix
                    Exit For
                End If
            Next
        End If

        'Schaltvorgaenge max. alle 3 Sekunden zulaessig:
        If t - itgangwL <= 2 And t > 2 And LastGear <> 0 Then
            Return LastGear    '<<< keine weiteren Checks!!!
        End If

        If Gear <> LastGear Then
            'Cruise-Phasen:
            'Solange Geschwindigkeitsaenderung seit letztem Gangwechsel unter 6% und Pe/Pnenn aenderung unter 6% wird nicht geschaltet:
            'Verzoegerungsphasen: Hochschalten wird unterdrückt
            'Beschleunigungsphasen: Zurückschalten wird unterdrückt
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
                'Wenn innerhalb von 6 Sekunden wieder in vorigen Gang zurueck geschaltet wird, wird der vorige Gang
                'durchgehend beibehalten
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
                    'Wenn innerhalb von 6 Sekunden einmal höher und einmal niedriger als voriger Gang
                    'geschaltet wird, wird voriger Gang durchgehend beibehalten
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

        '--------------------------------Checks Teil 2-------------------------------------
        'Schalten von 2. in 1. Gang wird bei v > 2,5 m/s unterdrueckt
        'NEU LUZ 040210: Hochschalten nur wenn im 2. Gang über Kuppeldrehzahl
        If Gear = 1 And LastGear > 1 And Vist >= 2.5 Then
            If fnn(Vist, 2, False) > Kuppln_norm Then Gear = 2
        End If

        'bei verzoegerungsvorgaengen unter 2,5 m/s wird in Leerlauf geschaltet
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

        'wenn v mehr als 1 Sek. < 0.1 m/s wird auf Gang=0 geschaltet
        If t < t1 Then
            If (Vist < 0.1 And MODdata.Vh.V(t + 1) < 0.1) Then
                Return 0    '<<< keine weiteren Checks!!!
            End If
        End If

        'bei Beschleunigungsvorgaengen unter 1,5 m/s wird in 1. Gang geschaltet
        If Vist < 1.5 And t < t1 Then
            If (Vist > 0.01 + MODdata.Vh.V(t - 1) And MODdata.Vh.V(t + 1) > 0.01 + Vist) Then
                Gear = 1
            End If
        End If

        'ueberpruefung, ob Drehzahl ueber nenndrehzahl, dann muss immer hochgeschaltet werden
        'sonst fehlt die leistung!
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



        '-----------------------------------Sekunde 1--------------------------------------
        'Erste Sekunde: Gang finden / Initialisierung
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

        '--------------------------------Ab Sekunde 2--------------------------------------

        '---------Startwerte---------
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



        'Leistung berechnen von jz bis jz + 6-----------------
        tx = t
        For t = tx To tx + 6
            If t > t1 Then Exit For

            Vist0 = MODdata.Vh.V(t)

            Pe0 = fPeGearMod(gangX, t)

            n0 = fnU(MODdata.Vh.V(t), gangX, False) / VEH.nNenn

            '-----------------------------------------------------------------
            '
            '     Berechnung nach Drehzahl/Leistung-Modell
            '
            '    --------------------------------------------------------------
            '(1) Nach Variante "schnelle Fahrweise"

            'Schaltmanoever erfolgt nur, wenn v-aenderung 5% seit letztem Gangwechsel,
            'VECTO: Auskommentiert START
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
            'VECTO: Auskommentiert ENDE

            'in ersten 10 Zyklussekunden kann zum Einregulieren immer geschlatet werden:
            If (t <= 9) Then achek = -1

            'Bei Aenderung der Steigung kann ebenfalls immer geschaltet werden:
            achstg = Math.Abs(MODdata.Vh.Grad(t) - MODdata.Vh.Grad(itgangwH))
            If (achstg > 0.001) Then achek = -1

            'Hinunterschalten:
            If (n0 <= VEH.hinunter) Then
                If (achek < 0.9) Then
                    gangX = gangX - 1
                End If
            End If

            'Hochschalten:
            ' bei Steigungszyklen mit zu hohen Geschwindigkeiten wird geschw. i+1 erst nach gangwahl berechnet
            ' dabei manchmal zu hoher gang -> Drehzahl und P_max viel zu nieder, daher nu bei niederen leistungen
            ' hochschalten erlaubt:
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
            '(2) Nach Variante "sparsame Fahrweise"

            '   Zurueckschalten erfolgt nur, wenn Geschwindigkeitsaenderung > 6%
            '   Hochschalten geht immer
            'VECTO: Auskommentiert START
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
            'VECTO: Auskommentiert ENDE

            '       in ersten 10 Zyklussekunden kann zum einregulieren immer geschlatet werden:
            If (t <= 9) Then achek = -1
            '       Bei Aenderung der Steigung kann ebenfalls immer geschaltet werden:
            achstg = Math.Abs(MODdata.Vh.Grad(t) - MODdata.Vh.Grad(itgangwL))
            If (achstg > 0.001) Then achek = -1

            '    Hinunterschalten:
            If (GangXl > 1) Then
                cnl = fnU(Vist0, GangXl, False) / VEH.nNenn
                If (cnl <= VEH.lhinunter) Then
                    If (achek < 0.9) Then
                        GangXl = GangXl - 1
                    End If
                End If
            End If

            'Hinaufschalten, nur ueberprueft wenn nicht schon hoechster Gang:
            If (GangXl < VEH.ganganz) Then
                'C
                If (Pe0 > 0.8) Then
                    If (n0 < 0.85) Then
                        achek = 1.0
                    End If
                End If
                'C
                'C     relevante Drehzahlen:
                cnl = fnU(Vist0, GangXl + 1, False) / VEH.nNenn


                If (cnl >= VEH.lhinauf) Then
                    GangXl = GangXl + 1
                End If
            End If

            GangL(t) = GangXl

            ' Auswahl des Drehzahlverhaeltnisses aus der "schnellen (..h)" und
            ' der "sparsamen (..l)" Variante:

            '   Drehzahlverhhealtnisse nach "Modellmix":
            '   anhand der erforderlichen maximalen Motorleistung ueber die
            '   naechsten 6 Sekunden
            P_maxg = Pe0
            For ix = t To t + 5
                If ix > t1 Then Exit For
                PeX = fPeGearMod(gangX, ix)
                If (PeX > P_maxg) Then P_maxg = PeX
            Next

            '     Festlegung der Anteile von schneller und sparsamer Fahrweise
            '      (Modell Hausberger):
            pschnellm = 3.3333 * P_maxg - 1.6667

            If (pschnellm > 1) Then pschnellm = 1
            If (pschnellm < 0) Then pschnellm = 0
            psparm = 1.0 - pschnellm

            '     Mix der berechneten Gaenge gemaess Vorgabe in Eingabefile:
            '     (pmodell wird aus Eingabefile gelesen, = Anteil, zu der die Drehzahl
            '      nach "reales Modell" bestehen soll)

            psparist = VEH.pspar + psparm * VEH.pmodell
            pschist = 1.0 - psparist

            '      Ermittlung des "virtuellen" aktuellen Ganges nach Modell
            Gear = Math.Round((pschist) * GangH(t) + (psparist) * GangL(t), 0, MidpointRounding.AwayFromZero)
            If (Gear > VEH.ganganz) Then Gear = VEH.ganganz
            If (Gear < 1) Then Gear = 1

            '    ueberpruefung, ob Drehzahl ueber nenndrehzahl, dann muss immer hochgeschaltet werden
            '    sonst fehlt die leistung!
lb88:
            n0 = fnU(Vist0, Gear, False) / VEH.nNenn
            If (n0 > 1) Then
                If (Gear < VEH.ganganz) Then
                    Gear = Gear + 1
                    GoTo lb88
                End If
            End If

            '    ueberpruefung, ob erforderliche leistung ueber P_max(n) liegt
            '    dann wird zurueckgeschaltet:

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

            '    Eigentliche Ueberpruefung ob ueber P_max(n)

lb909:

            Pivoll = FLD.Pfull(fnn(MODdata.Vh.V(t), Gear, ClutchSlip), LastPe) / VEH.Pnenn

            'falls schlechte Vollastkurve ohne Moment in leerlauf wird korrigiert:
            If (Pivoll < 0.06) Then Pivoll = 0.06

            '    Ueberpruefung, ob hoehere Leistung erforderlich ist als Maximalleistung bei nh
            '     dann wird zurueckgeschaltet:
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

        'c     Ende "Modell"-Basisgangwahl
        '---------------------------------------------------


        'Kuppelschleif-check
        ''If bPplus And fn_norm(1) < Kuppln_norm Then bKupplSchleif = True

        '--------------------------------Checks Teil 1-------------------------------------
        'Checks zur Saeuberung unsinniger Schlatmanoever:
        'Unterteilung in "iphase(j)" Beschleunigungs- (=1), Verzoegerungs- (=2) und Cruise (=3) -Phasen:
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

        'Suche nach letztem Gangwechsel
        itgangwL = 0
        For ix = t - 1 To 1 Step -1
            If MODdata.Gear(ix) <> MODdata.Gear(ix - 1) Then
                itgangwL = ix
                Exit For
            End If
        Next

        'Schaltvorgaenge max. alle 3 Sekunden zulaessig:
        If t - itgangwL <= 3 And t > 2 Then
            Return LastGear    '<<< keine weiteren Checks!!!
        End If

        'Cruise-Phasen:
        'Solange Geschwindigkeitsaenderung seit letztem Gangwechsel unter 6% und Pe/Pnenn aenderung unter 6% wird nicht geschaltet:
        'Verzoegerungsphasen: Hochschalten wird unterdrückt
        'Beschleunigungsphasen: Zurückschalten wird unterdrückt
        bCheck = False
        Pjetzt = fPeGearMod(Gear, t)
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

        'Wenn innerhalb von 6 Sekunden wieder in vorigen Gang zurueck geschaltet wird, wird der vorige Gang
        'durchgehend beibehalten
        'VECTO: Ausnahme: Über Volllastkurve
        If Not OverPfull Then
            bCheck = False
            For ix = t + 1 To t + 6
                If ix > t1 Then Exit For
                If (Gears(ix - t) = LastGear) Then bCheck = True
            Next
            If bCheck Then Gear = LastGear
        End If


        'Wenn innerhalb von 6 Sekunden einmal höher und einmal niedriger als voriger Gang
        'geschaltet wird, wird voriger Gang durchgehend beibehalten
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

        '--------------------------------Checks Teil 2-------------------------------------
        'Schalten von 2. in 1. Gang wird bei v > 1.5 m/s unterdrueckt
        'NEU LUZ 040210: Hochschalten nur wenn im 2. Gang über Kuppeldrehzahl
        If Gear = 1 And LastGear > 1 And Vist >= 1.5 Then
            If fnn(Vist, 2, False) > Kuppln_norm Then Gear = 2
        End If

        'bei verzoegerungsvorgaengen unter 1.5 m/s wird in Leerlauf geschaltet
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

        'wenn v mehr als 1 Sek. < 0.1 m/s wird auf Gang=0 geschaltet
        If t < t1 Then
            If (Vist < 0.1 And MODdata.Vh.V(t + 1) < 0.1) Then
                Return 0    '<<< keine weiteren Checks!!!
            End If
        End If


        'ueberpruefung, ob Drehzahl ueber nenndrehzahl, dann muss immer hochgeschaltet werden
        'sonst fehlt die leistung!
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

        'Geschwindigkeit vorausschauen
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

        'Checks aus PKWgear....
        'Schalten von 2. in 1. Gang wird bei v>2,5 m/s unterdrueckt
        If (g = 1) Then
            If (g0 > g) Then
                If (v >= 2.5) Then
                    g = 2
                End If
            End If
        End If

        'Bei verzoegerungsvorgaengen unter 2,5 m/s wird in Leerlauf geschaltet
        If (v < 2.5) Then
            If (v <= v0 And v1 <= v) Then
                If (v2 <= v1) Then
                    g = 0
                End If
            End If
        End If

        'Wenn v mehr als 1 Sek. <0.1 m/s wird auf Gang=0 geschaltet
        If (v < 0.1 And v1 < 0.1) Then
            g = 0
        End If

        'Bei Beschleunigungsvorgaengen unter 1,5 m/s wird in 1. Gang geschaltet
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

    'Funktion zur einfachen Leistungsberechnung für Schaltmodelle
    Private Function fPeGearMod(ByVal Gear As Integer, ByVal t As Integer) As Single
        Return fPvD(t) / VEH.Pnenn
    End Function

    'Funktion zur einfachen Leistungsberechnung für EV-Schaltmodell
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
            'Drehzahlvorgabe
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
#End Region

#Region "Leistungsberechnung"

    '--------------Leistung vor Diff = Am Rad-------------
    Private Function fPvD(ByVal t As Integer) As Single
        Return fPr(MODdata.Vh.V(t)) + fPair(MODdata.Vh.V(t), t) + fPaFZ(MODdata.Vh.V(t), MODdata.Vh.a(t)) + fPs(MODdata.Vh.V(t), t)
    End Function

    Private Function fPvD(ByVal t As Integer, ByVal v As Single, ByVal a As Single) As Single
        Return fPr(v) + fPair(v, t) + fPaFZ(v, a) + fPs(v, t)
    End Function

    '----------------Rollwiderstand----------------
    Private Function fPr(ByVal v As Single) As Single
        Return CSng((VEH.Loading + VEH.Mass + VEH.MassExtra) * 9.81 * (VEH.Fr0 + VEH.Fr1 * v + VEH.Fr2 * v ^ 2 + VEH.Fr3 * v ^ 3 + VEH.Fr4 * v ^ 4) * v * 0.001)
    End Function

    '----------------Luftwiderstand----------------
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

    '--------Beschleunigungsleistung Fahrzeug--------
    Private Function fPaFZ(ByVal v As Single, ByVal a As Single) As Single
        'Früher (PHEM 10.4.2 und älter) wurde statt m_raeder Massered verwendet, mit Massered = m_raeder + I_Getriebe * (Iachs / (0.5 * Dreifen)) ^ 2
        '   Der fehlende Teil (I_Getriebe * (Iachs / (0.5 * Dreifen)) ^ 2) wird jetzt durch fPaG(V,a) mit berücksichtigt
        Return CSng(((VEH.Mass + VEH.MassExtra + VEH.m_raeder_red + VEH.Loading) * a * v) * 0.001)
    End Function

    '----------------Steigungswiderstand----------------
    Private Function fPs(ByVal v As Single, ByVal t As Integer) As Single
        fPs = CSng(((VEH.Loading + VEH.Mass + VEH.MassExtra) * 9.81 * MODdata.Vh.Grad(t) * 0.01 * v) * 0.001)
    End Function

    '----------------Nebenaggregate----------------
    Private Function fPaux(ByVal t As Integer, ByVal nU As Single) As Single
        Return CSng(VEH.Paux0 * VEH.Pnenn + MODdata.Vh.Padd(t) + VEH.PauxSum(t, nU))
    End Function

    '-------------------Getriebe-------------------
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

                'Leistung nach Getriebe (Getriebeausgang)
                P = Math.Abs(PvD) + Pdiff

                'Verluste berechnet (eignet sich nur für Schaltgetriebe)
                '       Interpolation der Getriebeverlustleistung
                '       Zwischen 1. und 8. gang sowie 9. und 16. Gang:
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

                '***Differenzial
                '   Leistung nach Diff (vor Getriebe)
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

                '***Differenzial
                '   Leistung vor Diff
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

    '----------------Getriebe Trägheit----------------
    Private Function fPaG(ByVal V As Single, ByVal a As Single) As Single
        Dim Mred As Single
        Mred = CSng(VEH.I_Getriebe * (VEH.AchsI / (0.5 * VEH.Dreifen)) ^ 2)
        Return CSng((Mred * a * V) * 0.001)
    End Function

#End Region


End Class

