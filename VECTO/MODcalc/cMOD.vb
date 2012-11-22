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

    'Leistungen
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

    Public EngState As List(Of tEngState)

    'Fahrzeug
    Public Gear As List(Of Single)
    Public VehState As List(Of tVehState)



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

        EngState = New List(Of tEngState)

        Gear = New List(Of Single)
        VehState = New List(Of tVehState)

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

            EngState = Nothing

            Gear = Nothing
            VehState = Nothing

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

        'Zykluslänge definieren (Um 1s kürzer als Original weil Zwischensekunden)
        tDim = DRI.tDim - 1

        'Hier wird der eigentliche Zyklus eingelesen:
        Vh.VehCylceInit()

        'Drehzahl-Vorgabe
        If DRI.Nvorg Then

            MODdata.nUvorg = New List(Of Single)
            MODdata.dnUvorg = New List(Of Single)

            L = DRI.Values(tDriComp.nn)

            'Drehzahl
            For s = 0 To tDim
                MODdata.nUvorg.Add(((L(s + 1) + L(s)) / 2) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl)
            Next

            'Winkelbeschleunigung
            For s = 0 To tDim
                MODdata.dnUvorg.Add(L(s + 1) - L(s))
            Next

        End If

        'Em-Komponenten mitteln (Zwischensekunden) für KF-Erstellung oder Eng-Analysis
        If DRI.EmCompDef Then
            For Each EmKV In DRI.EmComponents
                For s = 0 To tDim
                    EmKV.Value.RawVals(s) = (EmKV.Value.RawVals(s + 1) + EmKV.Value.RawVals(s)) / 2
                Next
            Next
        End If

        'EXS Vorgaben mitteln
        If DRI.ExsCompDef Then
            For Each ExsKV In DRI.ExsComponents
                For Each Exs0 In ExsKV.Value.Values
                    For s = 0 To tDim
                        Exs0(s) = (Exs0(s + 1) + Exs0(s)) / 2
                    Next
                Next
            Next
        End If

        'Aux Vorgaben mitteln und Aux-Listen falls Aux in Dri und Veh vorhanden
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

        'Zykluslänge definieren: Gleiche Länge wie Zyklus (nicht reduziert weil keine "Zwischensekunden")
        tDim = DRI.tDim - 1

        'Hier wird der eigentliche Zyklus eingelesen:
        Vh.EngCylceInit()

        'Drehzahl-Vorgabe
        If DRI.Nvorg Then

            MODdata.nUvorg = New List(Of Single)
            MODdata.dnUvorg = New List(Of Single)

            L = DRI.Values(tDriComp.nn)

            'Drehzahl
            For s = 0 To MODdata.tDim
                MODdata.nUvorg.Add(L(s) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl)
            Next

            'Winkelbeschleunigung
            MODdata.dnUvorg.Add(L(1) - L(0))
            For s = 1 To MODdata.tDim - 1
                MODdata.dnUvorg.Add((L(s + 1) - L(s - 1)) / 2)
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
        Dim ADVmode As Boolean
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

        MsgSrc = "MOD/Output"

        '*********** Initialisierung / Datei öffnen **************
        If ModOutpName = "" Then
            WorkerMsg(tMsgID.Err, "Invalid output path!", MsgSrc)
            Return False
        End If

        f = New cFile_V3

        ADVmode = (PHEMmode = tPHEMmode.ModeADVANCE)

        If ADVmode Then
            path = ModOutpName & "_" & ADV.aVehType & ".mod"
        Else
            If GEN.dynkorja Then
                path = ModOutpName & ".vmod"
            Else
                path = ModOutpName & ".vmod"
            End If
        End If

        If Not f.OpenWrite(path, ",", False, ADVmode) Then
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



        '*** ID-Zeile (Nur ADVANCE)
        If ADVmode Then
            s.Append("VehNr: " & ADV.aVehNr)
            s.Append(",Input File: " & fFILE(GenFile, True))
            f.WriteLine(s.ToString)
        Else
            f.WriteLine("VECTO modal results")
            f.WriteLine("VECTO " & VECTOvers)
            f.WriteLine(Now.ToString)
            f.WriteLine("Input File: " & JobFile)
        End If

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
                sU.Append(",[rpm],[kW],[-],[-]")
            End If

            s.Append(",engine speed,PeEM,PeBat,PiBat,Ubat,Ibat,SOC")
            sU.Append(",[rpm],[kW],[kW],[kW],[V],[A],[-]")

        Else

            s.Append(",engine speed,Pe,n_norm,Pe_norm,Pe_full,Pe_drag,Pe_clutch")
            sU.Append(",[rpm],[kW],[-],[-],[kW],[kW],[kW]")

        End If

        If Not GEN.VehMode = tVehMode.EngineOnly Then

            s.Append(",Gear,Ploss GB,Ploss Diff,Ploss Retarder,Pa Eng,Pa GB,Pa Veh,Proll,Pair,Pgrad,Paux,Pwheel,Pbrake")
            sU.Append(",[-],[kW],[kW],[kW],[kW],[kW],[kW],[kW],[kW],[kW],[kW],[kW],[kW]")

            'Auxiliaries
            For Each StrKey In AuxList

                s.Append(",Paux_" & StrKey)
                sU.Append(",[kW]")

            Next

        End If

        'ADVANCE-spezifisch
        If ADVmode Then

            s.Append(",WorldX,WorldY,StrId")
            sU.Append(",[m],[m],[-]")

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

        'Berechnete Dynamikparameter (Diff zu Kennfeld)
        'If TC.Calculated Then
        '    For Each TcKey In TcList
        '        s.Append(Sepp & fMapCompName(TcKey))
        '        sU.Append(Sepp & "-")
        '    Next
        'End If


        'In Datei schreiben
        '   Header
        f.WriteLine(s.ToString)
        '   Units
        f.WriteLine(sU.ToString)

        '***********************************************************************************************
        '***********************************************************************************************
        '***********************************************************************************************
        '*** Werte *************************************************************************************

        With MODdata

            For t = 0 To t1

                s.Length = 0

                'Zeit
                s.Append(t + DRI.t0 + tdelta)

                If Not GEN.VehMode = tVehMode.EngineOnly Then

                    'Strecke
                    dist += .Vh.V(t) / 1000
                    s.Append(Sepp & dist)

                    'Ist-Geschw.
                    s.Append(Sepp & .Vh.V(t) * 3.6)

                    'Soll-Geschw.
                    s.Append(Sepp & .Vh.Vsoll(t) * 3.6)

                    'Beschl.
                    s.Append(Sepp & .Vh.a(t))

                    'Steigung
                    s.Append(Sepp & .Vh.Grad(t))

                End If

                If GEN.ModeHorEV Then

                    If Not GEN.VehMode = tVehMode.EV Then

                        'Drehzahl
                        s.Append(Sepp & .nn(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl)

                        'Leistung 
                        s.Append(Sepp & .Pe(t) * VEH.Pnenn)

                        'Drehzahl normiert
                        s.Append(Sepp & .nn(t))

                        'Leistung normiert
                        s.Append(Sepp & .Pe(t))

                    End If

                    'Drehzahl in U/min
                    s.Append(Sepp & .nU(t))

                    'EM-Leistung in kW
                    s.Append(Sepp & .Px.PeEMot(t))

                    'Effektive Batterieleistung
                    s.Append(Sepp & .Px.PeBat(t))

                    'Innere Batterieleistung
                    s.Append(Sepp & .Px.PiBat(t))

                    'Batteriespannung
                    s.Append(Sepp & .Px.Ubat(t))

                    'Batteriestrom
                    s.Append(Sepp & .Px.Ibat(t))

                    'SOC
                    s.Append(Sepp & .Px.SOC(t))

                Else

                    'Drehzahl
                    s.Append(Sepp & .nn(t) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl)

                    'Leistung 
                    s.Append(Sepp & .Pe(t) * VEH.Pnenn)

                    'Drehzahl normiert
                    s.Append(Sepp & .nn(t))

                    'Leistung normiert
                    s.Append(Sepp & .Pe(t))

                    'Volllast und Schlepp
                    If .EngState(t) = tEngState.Stopped Then
                        s.Append(Sepp & "-" & Sepp & "-")
                    Else
                        If t = 0 Then
                            s.Append(Sepp & FLD.Pfull(.nn(t)) & Sepp & FLD.Pdrag(.nn(t)))
                        Else
                            s.Append(Sepp & FLD.Pfull(.nn(t), .Pe(t - 1)) & Sepp & FLD.Pdrag(.nn(t)))
                        End If
                    End If

                    'Leistung an Kupplung
                    s.Append(Sepp & .Pe(t) * VEH.Pnenn - .PaEng(t) - .PauxSum(t))


                End If

                If Not GEN.VehMode = tVehMode.EngineOnly Then

                    'Gang
                    s.Append(Sepp & .Gear(t))

                    'Getriebeverluste
                    s.Append(Sepp & .PlossGB(t))

                    'Diff-Verluste
                    s.Append(Sepp & .PlossDiff(t))

                    'Retarder-Verluste
                    s.Append(Sepp & .PlossRt(t))

                    'PaEng
                    s.Append(Sepp & .PaEng(t))

                    'PaGB
                    s.Append(Sepp & .PaGB(t))

                    'Pa Veh
                    s.Append(Sepp & .Pa(t))

                    'Roll..
                    s.Append(Sepp & .Proll(t))

                    'Luft..
                    s.Append(Sepp & .Pluft(t))

                    'Steigung..
                    s.Append(Sepp & .Pstg(t))

                    'Aux..
                    s.Append(Sepp & .PauxSum(t))

                    'Radleistung
                    s.Append(Sepp & .Psum(t))

                    'Bremse
                    s.Append(Sepp & .Pbrake(t))

                    'Auxiliaries
                    For Each StrKey In AuxList
                        s.Append(Sepp & .Paux(StrKey)(t))
                    Next

                End If

                'ADVANCE-spezifisch
                If ADVmode Then

                    'X
                    s.Append(Sepp & ADV.aWorldX(t))

                    'Y
                    s.Append(Sepp & ADV.aWorldY(t))

                    'StrId
                    s.Append(Sepp & ADV.aStrId(t))

                End If

                If Cfg.FinalEmOnly Then

                    'Final-Emissionen (Tailpipe)
                    For Each StrKey In EmList

                        Em0 = .Em.EmComp(StrKey)

                        If Em0.WriteOutput Then
                            s.Append(Sepp & Em0.FinalVals(t))
                        End If

                    Next

                Else

                    For Each StrKey In EmList

                        Em0 = .Em.EmComp(StrKey)

                        If Em0.WriteOutput Then
                            'Roh-Emissionen
                            s.Append(Sepp & Em0.RawVals(t))

                            'TC-Emissionen
                            If Em0.TCdef Then s.Append(Sepp & Em0.TCVals(t))

                            'AT-Emissionen (EXS)
                            If Em0.ATdef Then s.Append(Sepp & Em0.ATVals(t))
                        End If

                    Next

                End If

                'Berechnete Dynamikparameter (Diff zu Kennfeld)
                'If TC.Calculated Then
                '    For Each TcKey In TcList
                '        TC0 = MODdata.TC.TCcomponents(TcKey)
                '        s.Append(Sepp & TC0(t))
                '    Next
                'End If

                'In Datei schreiben
                f.WriteLine(s.ToString)

            Next

        End With


        f.Close()

        Return True

    End Function

    'Errors/Warnings die sekündlich auftreten können
    Public Class cModErrors
        Public TrLossMapExtr As String
        Public AuxMapExtr As String
        Public AuxNegative As String
        Public FLDextrapol As String
        Public CdExtrapol As String
        Public RtExtrapol As String
        Public DesMaxExtr As String

        Public Sub New()
            Reset()
        End Sub


        'Reset-Hierarchie:
        ' ResetAll
        '   DesMaxExtr
        '   -GeschRedReset
        '       CdExtrapol        
        '       -PxReset
        '           TrLossMapExtr 
        '           AuxMapExtr 
        '           AuxNegative
        '           FLDextrapol

        'Kompletter Reset (am Beginn jedes Sekundenschritts)
        Public Sub ResetAll()
            DesMaxExtr = ""
            GeschRedReset()
        End Sub

        'Reset von Errors nach Geschw.-Reduktion (innerhalb Iteration)
        Public Sub GeschRedReset()
            CdExtrapol = ""
            RtExtrapol = ""
            PxReset()
        End Sub

        'Reset von Errors die mit der Leistungsberechnung zu tun haben (nach Schaltmodell durchzuführen)
        Public Sub PxReset()
            TrLossMapExtr = ""
            AuxMapExtr = ""
            AuxNegative = ""
            FLDextrapol = ""
        End Sub

        'Errors ausgeben
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

            Return Abort

        End Function

    End Class

   








End Class




