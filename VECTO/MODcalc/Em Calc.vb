﻿Imports System.Collections.Generic

Public Class cEm

    Public EmComp As Dictionary(Of String, cEmComp)
    Public EmDefComp As Dictionary(Of tMapComp, cEmComp)


    Public Sub Init()
        EmComp = New Dictionary(Of String, cEmComp)
        EmDefComp = New Dictionary(Of tMapComp, cEmComp)
    End Sub

    Public Sub CleanUp()
        EmComp = Nothing
        EmDefComp = Nothing
    End Sub

    Public Sub Raw_Calc()

        Dim i As Integer
        Dim KV As KeyValuePair(Of String, cEmComp)
        Dim Em0 As cEmComp

        For Each KV In MAP.EmComponents
            Em0 = New cEmComp
            Em0.Name = KV.Value.Name
            Em0.IDstring = KV.Value.IDstring
            Em0.MapCompID = KV.Value.MapCompID
            Em0.Unit = KV.Value.Unit
            Em0.NormID = KV.Value.NormID
            Em0.WriteOutput = KV.Value.WriteOutput
            EmComp.Add(KV.Key, Em0)
        Next

        For i = 0 To MODdata.tDim

            MAP.Intp_Init(MODdata.nn(i), MODdata.Pe(i))

            For Each KV In MAP.EmComponents

                Select Case MODdata.EngState(i)

                    Case tEngState.Stopped

                        EmComp(KV.Key).RawVals.Add(0)        '<= Soll das so bleiben?

                    Case Else   '<= Idle / Drag / FullLoad-Unterscheidung...?

                        If KV.Value.MapCompID = tMapComp.FC Then
                            'Delaunay
                            EmComp(KV.Key).RawVals.Add(MAP.fFCdelaunay_Intp(MODdata.nn(i), MODdata.Pe(i)))
                        Else
                            'Normale Interpolation
                            EmComp(KV.Key).RawVals.Add(MAP.fShep_Intp(KV.Value))
                        End If

                End Select

            Next
        Next

        For Each KV In EmComp
            If KV.Value.MapCompID <> tMapComp.Undefined Then
                EmDefComp.Add(KV.Value.MapCompID, KV.Value)
            End If
        Next

        KV = Nothing

    End Sub

    Public Sub TC_Calc()
        Dim dynkor As Double
        Dim Pnenn As Single
        Dim i As Integer
        Dim KV As KeyValuePair(Of String, cEmComp)
        Dim MapTC As Dictionary(Of tMapComp, Double)
        Dim ModTC As cTC
        Dim MsgSrc As String

        MsgSrc = "EmCalc/TC_Calc"

        Dim EmL As List(Of Single)

        If PHEMmode = tPHEMmode.ModeSTANDARD Then WorkerMsg(tMsgID.Normal, "Transient Correction", MsgSrc)

        Pnenn = VEH.Pnenn
        ModTC = MODdata.TC

        For Each KV In EmComp

            If MAP.EmComponents(KV.Key).TCdef Then

                MapTC = MAP.EmComponents(KV.Key).TCfactors

                KV.Value.InitTC()

                EmL = New List(Of Single)

                'Ersten zwei sekunden keine Korrektur:
                EmL.Add(KV.Value.RawVals(0))

                If MODdata.tDim > 0 Then
                    EmL.Add(KV.Value.RawVals(1))

                    For i = 2 To MODdata.tDim
                        dynkor = (MapTC(tMapComp.TCAmpl3s) * ModTC.Ampl3s(i) + MapTC(tMapComp.TCP40sABS) * ModTC.P40sABS(i) + MapTC(tMapComp.TCLW3p3s) * ModTC.LW3p3s(i) + MapTC(tMapComp.TCPneg3s) * ModTC.Pneg3s(i) + MapTC(tMapComp.TCPpos3s) * ModTC.Ppos3s(i) + MapTC(tMapComp.TCabsdn2s) * ModTC.abs_dn2s(i) + MapTC(tMapComp.TCP10sn10s3) * ModTC.P10s_n10s3(i) + MapTC(tMapComp.TCdP2s) * ModTC.dP_2s(i) + MapTC(tMapComp.TCdynV) * ModTC.dynV(i) + MapTC(tMapComp.TCdynAV) * ModTC.dynAV(i) + MapTC(tMapComp.TCdynDAV) * ModTC.dynDAV(i))
                        If MAP.EmComponents(KV.Key).NormID = tEmNorm.x_hPnenn Then dynkor *= Pnenn
                        EmL.Add(CSng((KV.Value.RawVals(i) + dynkor)))
                    Next
                End If

                KV.Value.TCVals = EmL
                KV.Value.FinalVals = KV.Value.TCVals

            End If
        Next

    End Sub

    Public Sub SumCalc()
        Dim KV As KeyValuePair(Of String, cEmComp)
        For Each KV In EmComp
            KV.Value.SumCalc()
        Next
    End Sub

    Public Function EngAnalysis() As Boolean

        Dim EmMesMW As Dictionary(Of String, Single)
        Dim EmSimMW As Dictionary(Of String, Single)
        Dim TCMW As Dictionary(Of String, Single)
        Dim EAcomp As List(Of String)
        Dim EAcomp0 As String

        Dim pe As Single
        Dim nn As Single

        Dim TCKV As KeyValuePair(Of tMapComp, List(Of Single))
        Dim EmKV0 As KeyValuePair(Of String, cEmComp)
        Dim DriEmRef As cEmComp

        Dim t As Integer
        Dim t1 As Integer

        Dim isinterv As Integer

        Dim s As System.Text.StringBuilder
        Dim sU As System.Text.StringBuilder
        Dim file As cFile_V3

        Dim reg As cRegression
        Dim reginfo As cRegression.RegressionProcessInfo
        Dim a As List(Of Double)
        Dim b As List(Of Double)
        Dim MsgSrc As String

        Dim UnitsErr As Dictionary(Of String, Boolean)

        MsgSrc = "EngAnalysis"

        EmMesMW = New Dictionary(Of String, Single)
        EmSimMW = New Dictionary(Of String, Single)
        TCMW = New Dictionary(Of String, Single)
        s = New System.Text.StringBuilder
        sU = New System.Text.StringBuilder
        EAcomp = New List(Of String)
        UnitsErr = New Dictionary(Of String, Boolean)

        isinterv = Cfg.EAAvInt

        If isinterv < 1 Then
            WorkerMsg(tMsgID.Err, "Range for average values '" & isinterv & "' is not valid!", MsgSrc)
            Return False
        End If

        file = New cFile_V3

        If Not file.OpenWrite(MODdata.ModOutpName & ".mwe") Then
            WorkerMsg(tMsgID.Err, "Can't write to " & MODdata.ModOutpName & ".mwe", MsgSrc)
            Return False
        End If

        'Dictionaries erstellen
        For Each EmKV0 In DRI.EmComponents
            If EmComp.ContainsKey(EmKV0.Key) Then
                EmMesMW.Add(EmKV0.Key, 0)
                EmSimMW.Add(EmKV0.Key, 0)
                EAcomp.Add(EmKV0.Key)

                'Unit-Check
                If UCase(EmKV0.Value.Unit) <> UCase(EmComp(EmKV0.Key).Unit) Then
                    WorkerMsg(tMsgID.Warn, "Unit mismatch! Cycle Component '" & EmKV0.Value.Name & "', Unit '" & EmKV0.Value.Unit & "' <> '" & EmComp(EmKV0.Key).Unit & "'", MsgSrc)
                    UnitsErr.Add(EmKV0.Key, True)   'True ist wurscht... es geht nur drum, dass der Key drinnen ist
                End If


            Else
                WorkerMsg(tMsgID.Warn, "Cycle Component '" & EmKV0.Value.Name & "' not found in Emission Map. Skipped for analysis.", MsgSrc)
            End If
        Next

        For Each TCKV In MODdata.TC.TCcomponents
            TCMW.Add(TCKV.Key, 0)
        Next

        'Summen ermitteln
        For Each EAcomp0 In EAcomp

            DriEmRef = DRI.EmComponents(EAcomp0)

            For t = 0 To MODdata.tDim
                EmMesMW(EAcomp0) += DriEmRef.RawVals(t)
                EmSimMW(EAcomp0) += EmComp(EAcomp0).FinalVals(t)
            Next

        Next

        'Mittelwerte
        For Each EAcomp0 In EAcomp

            EmMesMW(EAcomp0) /= MODdata.tDim + 1
            EmSimMW(EAcomp0) /= MODdata.tDim + 1

        Next

        '***************************************** 'Header '******************************************
        file.WriteLine("VECTO Engine Analysis averaged results")
        file.WriteLine("VECTO " & VECTOvers)
        file.WriteLine(Now.ToString)
        file.WriteLine("Input File: " & JobFile)
        file.WriteLine("Driving Cycle and Measurement Data: " & CurrentCycleFile)
        file.WriteLine(" ")
        file.WriteLine("Meas = Measured value (Input)")
        file.WriteLine("Sim = Calculated value (PHEM Output)")
        file.WriteLine("Diff = Sim - Meas")
        file.WriteLine("Delta = Sim / Meas - 1")
        file.WriteLine(" ")
        file.WriteLine("Cycle average results")
        file.WriteLine("EmComp,Unit,Meas,Sim,Diff,Delta,R" & ChrW(178))

        '************************************ 'Zyklus-Mittelwerte '************************************
        For Each EAcomp0 In EAcomp

            s.Length = 0

            '***** Name
            s.Append(DRI.EmComponents(EAcomp0).Name)

            '***** Unit
            If UnitsErr.ContainsKey(EAcomp0) Then
                s.Append(",?!")
            Else
                s.Append("," & DRI.EmComponents(EAcomp0).Unit)
            End If

            '***** Messwert
            s.Append("," & EmMesMW(EAcomp0))

            '***** PHEM-Wert
            s.Append("," & EmSimMW(EAcomp0))

            '***** Diff - ACHTUNG: Keine Pnenn-Normierung! Vorsicht bei Dynamik-Korrektur!
            s.Append("," & EmSimMW(EAcomp0) - EmMesMW(EAcomp0))

            '***** Delta
            If EmMesMW(EAcomp0) = 0 Then
                s.Append("," & "-")
            Else
                s.Append("," & (EmSimMW(EAcomp0) / EmMesMW(EAcomp0)) - 1)
            End If

            '***** R2
            reg = New cRegression
            a = New List(Of Double)
            b = New List(Of Double)

            'Über x Sekunden gemittelte Werte berechnen und sofort au
            For t = isinterv To MODdata.tDim Step isinterv

                'Null setzen
                EmMesMW(EAcomp0) = 0
                EmSimMW(EAcomp0) = 0

                'Aufsummieren
                For t1 = (t - isinterv) To t - 1
                    EmMesMW(EAcomp0) += DRI.EmComponents(EAcomp0).RawVals(t1) / isinterv
                    EmSimMW(EAcomp0) += EmComp(EAcomp0).FinalVals(t1) / isinterv
                Next

                'Messwert
                a.Add(CDbl(EmMesMW(EAcomp0)))

                'PHEM-Wert
                b.Add(CDbl(EmSimMW(EAcomp0)))

            Next

            reginfo = reg.Regress(a.ToArray, b.ToArray)

            s.Append("," & reginfo.PearsonsR ^ 2)

            reginfo = Nothing
            reg = Nothing

            file.WriteLine(s.ToString)

        Next

        '************************************ Modale Ausgabe '************************************
        file.WriteLine(" ")
        file.WriteLine("Modal results averaged over " & isinterv & " seconds.")

        'Header/Units
        s.Length = 0
        s.Append("t,n_norm,Pe_norm")
        sU.Append("[s],[-],[-]")

        For Each EAcomp0 In EAcomp

            DriEmRef = DRI.EmComponents(EAcomp0)

            'Messwert
            s.Append("," & DriEmRef.Name & "_Meas")
            sU.Append("," & DriEmRef.Unit)

            'PHEM-Wert
            s.Append("," & EmComp(EAcomp0).Name & "_PHEM")
            sU.Append("," & EmComp(EAcomp0).Unit)

            'Diff - ACHTUNG: Keine Pnenn-Normierung! Vorsicht bei Dynamik-Korrektur!
            s.Append("," & DriEmRef.Name & "_Diff")
            If UnitsErr.ContainsKey(EAcomp0) Then
                sU.Append(",?!")
            Else
                sU.Append("," & DriEmRef.Unit)
            End If

            'Delta
            s.Append("," & DriEmRef.Name & "_Delta")
            sU.Append("," & "[-]")

        Next

        For Each TCKV In MODdata.TC.TCcomponents
            s.Append("," & fMapCompName(TCKV.Key))
            sU.Append("," & "-")
        Next

        'Header und Units schreiben
        file.WriteLine(s.ToString)
        file.WriteLine(sU.ToString)

        'Über x Sekunden gemittelte Werte berechnen und sofort au
        For t = isinterv To MODdata.tDim Step isinterv

            'Null setzen
            For Each EAcomp0 In EAcomp
                EmMesMW(EAcomp0) = 0
                EmSimMW(EAcomp0) = 0
            Next

            pe = 0
            nn = 0

            For Each TCKV In MODdata.TC.TCcomponents
                TCMW(TCKV.Key) = 0
            Next

            'Aufsummieren
            For t1 = (t - isinterv) To t - 1

                pe += MODdata.Pe(t1) / isinterv
                nn += MODdata.nn(t1) / isinterv

                For Each EAcomp0 In EAcomp
                    EmMesMW(EAcomp0) += DRI.EmComponents(EAcomp0).RawVals(t1) / isinterv
                    EmSimMW(EAcomp0) += EmComp(EAcomp0).FinalVals(t1) / isinterv
                Next

                For Each TCKV In MODdata.TC.TCcomponents
                    TCMW(TCKV.Key) += TCKV.Value(t1) / isinterv
                Next

            Next

            'Ausgabe
            s.Length = 0

            s.Append(CStr(DRI.t0 + t - ((isinterv - 1) / 2) - 1))

            'Pe/nn
            s.Append("," & nn)
            s.Append("," & pe)

            For Each EAcomp0 In EAcomp

                'Messwert
                s.Append("," & EmMesMW(EAcomp0))

                'PHEM-Wert
                s.Append("," & EmSimMW(EAcomp0))

                'Diff
                If MAP.EmComponents(EAcomp0).NormID = tEmNorm.x_hPnenn Then
                    s.Append("," & (EmSimMW(EAcomp0) - EmMesMW(EAcomp0)) / VEH.Pnenn)
                Else
                    s.Append("," & EmSimMW(EAcomp0) - EmMesMW(EAcomp0))
                End If

                'Delta
                If EmMesMW(EAcomp0) = 0 Then
                    s.Append("," & "-")
                Else
                    s.Append("," & (EmSimMW(EAcomp0) / EmMesMW(EAcomp0)) - 1)
                End If

            Next


            'TC
            For Each TCKV In MODdata.TC.TCcomponents
                s.Append("," & TCMW(TCKV.Key))
            Next

            file.WriteLine(s.ToString)

        Next


        file.Close()

        Return True

    End Function


End Class

Public Class cTC

    Public TCcomponents As Dictionary(Of tMapComp, List(Of Single))

    Public Ppos3s As List(Of Single)
    Public Pneg3s As List(Of Single)
    Public Ampl3s As List(Of Single)
    Public Ppos As List(Of Single)
    Public Pneg As List(Of Single)
    Public dP_2s As List(Of Single)
    Public P10s_n10s3 As List(Of Single)
    Public P40sABS As List(Of Single)
    Public abs_dn2s As List(Of Single)
    Public LW3p3s As List(Of Single)
    Public dynV As List(Of Single)
    Public dynAV As List(Of Single)
    Public dynDAV As List(Of Single)

    Public Calculated As Boolean

    Public Sub Init()
        Ppos3s = New List(Of Single)
        Pneg3s = New List(Of Single)
        Ampl3s = New List(Of Single)
        Ppos = New List(Of Single)
        Pneg = New List(Of Single)
        dP_2s = New List(Of Single)
        P10s_n10s3 = New List(Of Single)
        P40sABS = New List(Of Single)
        abs_dn2s = New List(Of Single)
        LW3p3s = New List(Of Single)
        dynV = New List(Of Single)
        dynAV = New List(Of Single)
        dynDAV = New List(Of Single)

        TCcomponents = New Dictionary(Of tMapComp, List(Of Single))

        TCcomponents.Add(tMapComp.TCPpos3s, Ppos3s)
        TCcomponents.Add(tMapComp.TCPneg3s, Pneg3s)
        TCcomponents.Add(tMapComp.TCAmpl3s, Ampl3s)
        TCcomponents.Add(tMapComp.TCdP2s, dP_2s)
        TCcomponents.Add(tMapComp.TCP10sn10s3, P10s_n10s3)
        TCcomponents.Add(tMapComp.TCP40sABS, P40sABS)
        TCcomponents.Add(tMapComp.TCabsdn2s, abs_dn2s)
        TCcomponents.Add(tMapComp.TCLW3p3s, LW3p3s)
        TCcomponents.Add(tMapComp.TCdynV, dynV)
        TCcomponents.Add(tMapComp.TCdynAV, dynAV)
        TCcomponents.Add(tMapComp.TCdynDAV, dynDAV)

        Calculated = False

    End Sub

    Public Sub CleanUp()
        TCcomponents = Nothing
        Ppos3s = Nothing
        Pneg3s = Nothing
        Ampl3s = Nothing
        Ppos = Nothing
        Pneg = Nothing
        dP_2s = Nothing
        P10s_n10s3 = Nothing
        P40sABS = Nothing
        abs_dn2s = Nothing
        LW3p3s = Nothing
        dynV = Nothing
        dynAV = Nothing
        dynDAV = Nothing
        Calculated = False
    End Sub

    Public Sub Calc()
        Dim LWzahl As Long, Pfind As Long
        Dim LWja As Single
        Dim Persatz As Single
        Dim Ampl1 As Single
        Dim Ampl2 As Single
        Dim Ampl3 As Single
        Dim MW_P10s As Single
        Dim MW_n10s As Single
        Dim MW_P40s As Single
        Dim js As Int32
        Dim i As Integer
        Dim iZahl As Int32
        Dim jk As Int32
        Dim xcheck As Single

        Dim maxmin(MODdata.tDim) As Single
        Dim Ampl0(MODdata.tDim) As Single


        Dim Pe As List(Of Single)
        Dim nn As List(Of Single)

        Dim Pnenn As Single

        LWzahl = 0

        Pnenn = VEH.Pnenn

        Pe = MODdata.Pe
        nn = MODdata.nn

        For i = 0 To MODdata.tDim
            Ppos3s.Add(0)
            Pneg3s.Add(0)
            Ampl3s.Add(0)
            Ppos.Add(0)
            Pneg.Add(0)
            dP_2s.Add(0)
            P10s_n10s3.Add(0)
            P40sABS.Add(0)
            abs_dn2s.Add(0)
            LW3p3s.Add(0)
            dynV.Add(0)
            dynAV.Add(0)
            dynDAV.Add(0)
        Next

        For i = 1 To MODdata.tDim

            Pfind = 0
            'C      Lastwechsel (allgemeine Bedingung ausser bei Intervallen mit
            'C      Konstantfahrt:
            'C
            If i = MODdata.tDim Then
                LWja = 0
            Else
                LWja = (Pe(i + 1) - Pe(i)) * (Pe(i) - Pe(i - 1))
            End If
            'C
            'C       Damit werden Trapezfoermige Zyklen nicht als lastwechsel erkannt
            'C       da LWje = 0. In diesem fall wird voraus der naechste Wert gesucht,
            'C       der nicht gleich Pe(jz) ist. Dieser wird anstelle von Pe(jz+1)
            'C       gesetzt:
            If i = MODdata.tDim Then
                For js = i + 2 To MODdata.tDim
                    If (Pe(js) <> Pe(i)) Then
                        Persatz = Pe(js)
                        Pfind = 1
                        Exit For
                    End If
                Next js
                If (Pfind = 1) Then
                    LWja = (Persatz - Pe(i)) * (Pe(i) - Pe(i - 1))
                End If
            Else
                If (Pe(i + 1) = Pe(i)) Then
                    For js = i + 2 To MODdata.tDim
                        If (Pe(js) <> Pe(i)) Then
                            Persatz = Pe(js)
                            Pfind = 1
                            Exit For
                        End If
                    Next js
                    If (Pfind = 1) Then
                        LWja = (Persatz - Pe(i)) * (Pe(i) - Pe(i - 1))
                    End If
                End If
            End If

            'C
            'C      lastwechsel werden nur als solche gezaehlt, wenn sie mehr als 0.05% von
            'C      Pnenn betragen (sonst ist Ergebnis viel zu wackelig):
            'C       (Lastwechsel wird gezaehlt, wenn LWja < 0)
            'C
            If (LWja > -0.0005) Then LWja = 0.1
            'C
            'C     (1) Mittlere Amplitude vom Pe-Verlauf ("Ampl")
            'C         Zwischenrechnung fue Zyklusmittelwert:
            'C
            If (LWja < 0) Then
                LWzahl = LWzahl + 1
                maxmin(LWzahl) = Pe(i)
            End If
            'C
            'C       Berechnung der mittleren Amplitude in 3 Sekunden vor Emission (Ampl3s)
            'C       und der Anzahl der Pe-Sekundenschritten ueber 3% der Nennleistung
            'C       (LW3p3s):
            LW3p3s(i) = 0
            If (i < 2) Then
                Ampl3s(i) = Pe(i) - Pe(i - 1)
                If (Ampl3s(i) < 0) Then Ampl3s(i) = Ampl3s(i) * (-1)
                If (Ampl3s(i) >= 0.03) Then LW3p3s(i) = LW3p3s(i) + 1
            ElseIf (i < 3) Then
                Ampl1 = Pe(i) - Pe(i - 1)
                If (Ampl1 < 0) Then Ampl1 = Ampl1 * (-1)
                If (Ampl1 >= 0.03) Then LW3p3s(i) = LW3p3s(i) + 1
                Ampl2 = Pe(i - 1) - Pe(i - 2)
                If (Ampl2 < 0) Then Ampl2 = Ampl2 * (-1)
                If (Ampl2 >= 0.03) Then LW3p3s(i) = LW3p3s(i) + 1
                Ampl3s(i) = (Ampl1 + Ampl2) / 2
            Else
                Ampl1 = Pe(i) - Pe(i - 1)
                If (Ampl1 < 0) Then Ampl1 = Ampl1 * (-1)
                If (Ampl1 >= 0.03) Then LW3p3s(i) = LW3p3s(i) + 1
                Ampl2 = Pe(i - 1) - Pe(i - 2)
                If (Ampl2 < 0) Then Ampl2 = Ampl2 * (-1)
                If (Ampl2 >= 0.03) Then LW3p3s(i) = LW3p3s(i) + 1
                Ampl3 = Pe(i - 2) - Pe(i - 3)
                If (Ampl3 < 0) Then Ampl3 = Ampl3 * (-1)
                If (Ampl3 >= 0.03) Then LW3p3s(i) = LW3p3s(i) + 1
                Ampl3s(i) = (Ampl1 + Ampl2 + Ampl3) / 3
            End If
        Next i


        For i = 0 To MODdata.tDim
            'C
            'C     (2) Aenderung der aktuellen Motorleistung (dP_2s):
            'C
            If (i = 0) Then
                dP_2s(i) = 0
            ElseIf (i < 2) Then
                dP_2s(i) = Pe(i) - Pe(i - 1)
            Else
                dP_2s(i) = 0.5 * (Pe(i) - Pe(i - 2))
            End If

            Ppos(i) = Pe(i)
            If (Ppos(i) <= 0) Then
                Ppos(i) = 0
            End If
            'C     Mittelwert 3 sec. vor Emission:
            If (i >= 2) Then
                Ppos3s(i) = (Ppos(i) + Ppos(i - 1) + Ppos(i - 2)) / 3
            ElseIf (i = 2) Then
                Ppos3s(i) = (Ppos(i) + Ppos(i - 1)) / 2
            ElseIf (i = 1) Then
                Ppos3s(i) = Ppos(i)
            End If
            'C    Gezaehlt nur bei dynamischem betrieb:
            xcheck = dP_2s(i) * dP_2s(i)
            If (xcheck >= 0.0000001) Then
                Ppos3s(i) = Ppos3s(i)
            Else
                Ppos3s(i) = 0
            End If
            'C
            'C
            'C     (4) Mittelwert der negativen Motorleistung ("PnegMW"):
            'C
            Pneg(i) = Pe(i)
            If (Pneg(i) >= 0) Then
                Pneg(i) = 0
            End If
            'C     Mittelwert 3 sec. vor Emission:
            If (i >= 2) Then
                Pneg3s(i) = (Pneg(i) + Pneg(i - 1) + Pneg(i - 2)) / 3
            ElseIf (i = 2) Then
                Pneg3s(i) = (Pneg(i) + Pneg(i - 1)) / 2
            ElseIf (i = 1) Then
                Pneg3s(i) = Pneg(i)
            End If
            'C    Gezaehlt nur bei dynamischem betrieb:
            xcheck = dP_2s(i) * dP_2s(i)
            If (xcheck >= 0.0000001) Then
                Pneg3s(i) = Pneg3s(i)
            Else
                Pneg3s(i) = 0
            End If

        Next i

        'C
        'C
        'C     Berechnung der absoluten Dynamikkenngroessen:
        'C      Addition der Amplituden von Pe (1. Pe-Wert
        'C      wird fuer Amplitude auch als Maxima bzw. Minima gezaehlt)
        'C    1. Sekunde:
        If (LWzahl >= 1) Then

            'C
            'C     2. Sekunde bis Ende:
            For i = 2 To LWzahl
                Ampl0(i) = maxmin(i) - maxmin(i - 1)
                'C        Absolutwert:
                If (Ampl0(i) < 0) Then
                    Ampl0(i) = Ampl0(i) * (-1)
                End If

            Next i
            'C
        Else

        End If
        'C
        For i = 0 To MODdata.tDim
            'C
            'C
            'C     (8) WM_n10sn**3 * MW_P_10s:
            'c
            MW_P10s = 0
            MW_n10s = 0
            iZahl = 0
            If (i < 9) Then
                For jk = 0 To i
                    MW_P10s = MW_P10s + Pe(jk)
                    MW_n10s = MW_n10s + (nn(jk) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl) / VEH.nNenn
                    iZahl = iZahl + 1
                Next jk
                MW_P10s = MW_P10s / iZahl
                MW_n10s = MW_n10s / iZahl
                'c
            Else
                For jk = (i - 9) To i
                    MW_P10s = MW_P10s + Pe(jk)
                    MW_n10s = MW_n10s + (nn(jk) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl) / VEH.nNenn
                Next jk
                MW_P10s = MW_P10s * 0.1
                MW_n10s = MW_n10s * 0.1
                'c
            End If
            'C
            xcheck = dP_2s(i) * dP_2s(i)
            If (xcheck >= 0.0000001) Then
                P10s_n10s3(i) = MW_n10s ^ 3 * MW_P10s
            Else
                P10s_n10s3(i) = 0
            End If
            'c
            'C     (9) MW_P40sABS:
            'c
            MW_P40s = 0
            iZahl = 0
            If (i < 39) Then
                For jk = 0 To i
                    MW_P40s = MW_P40s + Pe(jk)
                    iZahl = iZahl + 1
                Next jk
                MW_P40s = MW_P40s / iZahl
            Else
                For jk = (i - 39) To i
                    MW_P40s = MW_P40s + Pe(jk)
                Next jk
                MW_P40s = MW_P40s * 0.025
            End If
            'C
            P40sABS(i) = Pe(i) - MW_P40s
            'c
            'C     (9) MW_abs_dn2s:
            'c
            If (i < 2) Then
                abs_dn2s(i) = 0
            Else
                abs_dn2s(i) = 0.5 * (nn(i) - nn(i - 2))
                abs_dn2s(i) = Math.Abs(abs_dn2s(i))
            End If

        Next i

        'Geschw./Beschl.-abhängige Parameter nur wenn nicht Eng-Only
        If Not GEN.VehMode = tVehMode.EngineOnly Then
            For i = 0 To MODdata.tDim
                dynV(i) = MODdata.Vh.V(i)
                dynAV(i) = MODdata.Vh.V(i) * MODdata.Vh.a(i)
                If (i >= 1) Then
                    dynDAV(i) = (dynAV(i) - dynAV(i - 1)) / 1.0
                Else
                    dynDAV(i) = 0
                End If
            Next i
        End If

        'Dynamikparameter als Differenz zu Dynamik in Kennfeld
        '   ...war früher hier. Jetzt eigene Methode weil bei KF-Erstellung ungültig

        Calculated = True

    End Sub

    'Dynamikparameter als Differenz zu Dynamik in Kennfeld:
    Public Sub CalcDiff()
        Dim i As Integer

        For i = 0 To MODdata.tDim
            Ampl3s(i) = Ampl3s(i) - MODdata.Em.EmDefComp(tMapComp.TCAmpl3s).RawVals(i)
            P40sABS(i) = P40sABS(i) - MODdata.Em.EmDefComp(tMapComp.TCP40sABS).RawVals(i)
            LW3p3s(i) = LW3p3s(i) - MODdata.Em.EmDefComp(tMapComp.TCLW3p3s).RawVals(i)
            Pneg3s(i) = Pneg3s(i) - MODdata.Em.EmDefComp(tMapComp.TCPneg3s).RawVals(i)
            Ppos3s(i) = Ppos3s(i) - MODdata.Em.EmDefComp(tMapComp.TCPpos3s).RawVals(i)
            abs_dn2s(i) = abs_dn2s(i) - MODdata.Em.EmDefComp(tMapComp.TCabsdn2s).RawVals(i)
            P10s_n10s3(i) = P10s_n10s3(i) - MODdata.Em.EmDefComp(tMapComp.TCP10sn10s3).RawVals(i)
            dP_2s(i) = dP_2s(i) - MODdata.Em.EmDefComp(tMapComp.TCdP2s).RawVals(i)
            dynV(i) = dynV(i) - MODdata.Em.EmDefComp(tMapComp.TCdynV).RawVals(i)
            dynAV(i) = dynAV(i) - MODdata.Em.EmDefComp(tMapComp.TCdynAV).RawVals(i)
            dynDAV(i) = dynDAV(i) - MODdata.Em.EmDefComp(tMapComp.TCdynDAV).RawVals(i)
        Next

    End Sub




End Class

''' <summary>
''' Klasse zur Berchnung der Abgastemperaturen
''' </summary>
''' <remarks></remarks>
Public Class cEXS

    Private TempMod() As cTempMod
    Private ModAnz As Int16

    '! Felder für Größen aus PHEM Hauptprogramm
    Private vehspe(izykt) As Single
    Private nnorm As List(Of Single)
    Private p_norm As List(Of Single)
    Private rpm As List(Of Single)
    Private fc As List(Of Single)
    Private lambda As List(Of Single)
    Private tqs As List(Of Single)
    Private p_rated As Single

    Private Pi_ As Single, R_air As Single, R_exh As Single, Pr_exh As Single, St_Boltz As Single
    Private cp_steel As Single, rho_steel As Single, hc_soot As Single
    Private H_reak_co As Single, H_reak_nox As Single, H_reak_hc As Single
    Private M_co As Single, M_nox As Single, M_hc As Single
    Private n_iter As Long, t_amb As Single, iter_mode As Boolean, iter_pos As Int16, iter_tol As Single

    Private DatExs As New cFile_V3    'PHEM inputfile with definition of exhaust system structure
    Private DatTer As New cFile_V3    'PHEM resultfile with 1Hz Temperatures in Exhaust System
    Private DatPath As New cFile_V3   'PHEM-DEV inputfile with paths for inputdata 
    Private DatPhe As New cFile_V3    'PHEM-DEV inputfile with data from PHEM main program

    'variables for coolant simulation
    Private surf_engine As Single, surf_temp_eng As Single, eng_mass1 As Single, eng_mass2 As Single
    Private h_cap_mass1 As Single, h_cap_mass2 As Single, alpha_A1 As Single, alpha_A2 As Single
    Private ratio_h_to_m1 As Single, ratio_h_to_m2 As Single, t_coolant As Single, t_start_m1_m2 As Single
    Private t_mass1(izykt) As Single 'temperatur für 1. künstl. masse
    Private t_mass2(izykt) As Single 'temperatur für 2. künstl. masse
    Private qp_coolant1(izykt) As Single 'wärmestrom zur 1. masse
    Private qp_coolant2(izykt) As Single 'wärmestrom zur 2. masse
    Private qp_loss1(izykt) As Single 'wärmestrom 1. masse -> kühlsystem
    Private qp_loss2(izykt) As Single 'wärmestrom 2. masse -> kühlsystem
    Private qp_loss(izykt) As Single 'gesamtwärmestrom ins kühlsystem
    Private qp_out(izykt) As Single 'wärmestrom nach außen (verlust der nicht ins kühlsystem geht)

    Private DatCsy As New cFile_V3 'input file for coolant simulation

    Private PathDev As String = ""
    Private PathPhe As String = ""
    Private PathTer As String = ""

    Public mpexh(izykt) As Single
    Public eRawNOx(izykt) As Single

    Private ExsErg As System.IO.StreamWriter
    Private ExsErgPath As String

    Public Xis(100) As Single
    Public Yis(100) As Single

    '**** Einlesen von tgas aus .npi (Projekt HERO) ****
    ' => überschreibt tgas(jz) aus HtMass()
    ' Luz/Rexeis 16.05.2011
    Private t_gas1npi() As Single
    Private t_gas2npi() As Single
    '***************************************************

    Private Function hc_exh(ByVal t_exh As Single) As Single 'Wärmeleitfähigkeit Abgas (Lambda, Druck vernachlässigt)
        hc_exh = 0.0245 + (0.0000609 * t_exh)
    End Function


    Private Function kv_exh(ByVal t_exh As Single) As Single 'kinematische Viskosität Abgas (Lambda, Druck vernachlässigt)
        kv_exh = 0.00000598 + (0.000000146 * t_exh)
    End Function

    ''' <summary>
    ''' Hauptroutine für EXS Modul
    ''' </summary>
    ''' <remarks></remarks>
    Public Function Exs_Main() As Boolean 'aufzurufen im PHEM-Ablauf nach Dynamikkorrektur und vor Summenbildung
        '! Aufruf von Exs_Main(true) -> Developer Version ohne PHEM Hauptprogramm

        Dim ii As Long, ij As Long, Diff As Single
        Dim t1 As Long

        '! Felder für Größen aus exs-File
        Dim iMod As Int16, iSchad As Int16
        Dim efm_mode As Int16, cap_norm As Single, t_inl As Single, p_rel_inl As Single, fp4_efm As Single
        Dim dummy As String = ""
        Dim line1 As String, line2 As String

        Dim mpexh_fc As Single, mpexh_mot As Single, cap As Single, Vpexh_mot As Single, zaehler As Single, nenner As Single

        Dim eNOxOK As Boolean
        Dim eHCOK As Boolean
        Dim eCOOK As Boolean
        Dim ePMOK As Boolean
        Dim eEk1OK As Boolean
        Dim eEk2OK As Boolean

        Dim jz As Integer
        Dim Em0 As List(Of Single)
        Dim MsgSrc As String

        Dim LambdaGegJa As Boolean
        Dim HC As List(Of Single)
        Dim NOx As List(Of Single)
        Dim CO As List(Of Single)

        Dim qp_coolant As List(Of Single) = Nothing


        MsgSrc = "EmCalc/EXS/Main"

        '------------------------------------------------------------------------------------------
        'Allgemeine Konstanten
        Pi_ = 3.1416
        St_Boltz = 0.0000000567 'Stephan-Boltzmann Konstante
        R_air = 287.0 '!Gaskonstante Luft [J/(kg*K)]
        '!Stoffwerte Abgas:
        '!unempfindlich gg. lambda, siehe "Stoffwerte_vollständigeVerbrennung_neu.xls"
        R_exh = 288.2 '!Gaskonstante Abgas [J/(kg*K)]
        'cp_exh = 1054.0 '!Wärmekapazität Abgas [J/(kg*K)], wird nicht mehr verwendet weil jetzt direkt in Abh. von T und Lambda berechnet
        Pr_exh = 0.73 '!Prandtlzahl [-]
        '!
        cp_steel = 460.0 '!Wärmekapazität Edelstahl [J/(kg*K)]
        rho_steel = 7860.0 '!Dichte Edelstahl [[kg/m3]
        hc_soot = 0.15 '!Wärmeleitfähigkeit Russ [W/(mK)] "in loser Schichtung"
        '!Anmerkung: Mittelwertwert aus Internetrecherche, in Literatur keine Angaben gefunden
        '!kalibriert anhand Test an Thermoelement unter Annahme von Schichtdicke 0.1mm

        'Reaktionsenthalpien in J/mol
        H_reak_co = 283200.0
        H_reak_nox = 1928000.0
        H_reak_hc = 483000.0

        'Molmassen
        M_co = 28.0
        M_nox = 46.0
        M_hc = 42.0

        'Kompatibilität mit alter EXS-Strkutur. Bevor neues Konzept für Em-Komponenten mit cMAP-Klasse, tMapComp, etc. eingeführt wurde
        eNOxOK = MODdata.Em.EmDefComp.ContainsKey(tMapComp.NOx)
        eHCOK = MODdata.Em.EmDefComp.ContainsKey(tMapComp.HC)
        eCOOK = MODdata.Em.EmDefComp.ContainsKey(tMapComp.CO)
        ePMOK = MODdata.Em.EmDefComp.ContainsKey(tMapComp.PM)
        eEk1OK = MODdata.Em.EmDefComp.ContainsKey(tMapComp.PN)
        eEk2OK = MODdata.Em.EmDefComp.ContainsKey(tMapComp.NO)

        'Verweise auf Emissionen: Gegebene falls vorhanden sonst die berechneten
        If DRI.EmComponents.ContainsKey(sKey.MAP.HC) Then
            HC = DRI.EmComponents(sKey.MAP.HC).FinalVals
        Else
            HC = MODdata.Em.EmDefComp(tMapComp.HC).FinalVals
        End If

        If DRI.EmComponents.ContainsKey(sKey.MAP.NOx) Then
            NOx = DRI.EmComponents(sKey.MAP.NOx).FinalVals
        Else
            NOx = MODdata.Em.EmDefComp(tMapComp.NOx).FinalVals
        End If

        If DRI.EmComponents.ContainsKey(sKey.MAP.CO) Then
            CO = DRI.EmComponents(sKey.MAP.CO).FinalVals
        Else
            CO = MODdata.Em.EmDefComp(tMapComp.CO).FinalVals
        End If

        t1 = MODdata.tDim

        'Dimensionieren:
        ReDim vehspe(t1)


        If GEN.CoolantsimJa Then
            ReDim qp_coolant1(t1)
            ReDim qp_coolant2(t1)
            ReDim qp_loss1(t1)
            ReDim qp_loss2(t1)
            ReDim qp_loss(t1)
            ReDim qp_out(t1)
            ReDim t_mass1(t1)
            ReDim t_mass2(t1)
        End If



        n_iter = 0 'Zählvariable für Iterationen zur Berechnung der zyklusrepräsentativen Starttemperaturen

        'Übergabe der relevanten Größen aus dem PHEM Hauptprogramm
        'In DEV direkt aus der Datei *.phe eingelesen

        PathTer = MODdata.ModOutpName & ".ter"   'Left(JobFile, Len(JobFile) - 4) & ".ter"
        p_rated = VEH.Pnenn

        For jz = 0 To t1
            If DRI.Vvorg Then
                vehspe(jz) = MODdata.Vh.V(jz) * 3.6
            Else
                vehspe(jz) = 0
            End If
        Next jz

        fc = MODdata.Em.EmDefComp(tMapComp.FC).FinalVals
        nnorm = MODdata.nn
        p_norm = MODdata.Pe
        rpm = MODdata.nU
        tqs = MODdata.Em.EmDefComp(tMapComp.ExhTemp).FinalVals

        'Lambda
        If MAP.EmDefRef.ContainsKey(tMapComp.Lambda) Then
            LambdaGegJa = True
            lambda = MODdata.Em.EmDefComp(tMapComp.Lambda).FinalVals
        Else
            LambdaGegJa = False
            'Wird weiter unten belegt weil mpexh vorhanden sein muss
            lambda = New List(Of Single)
        End If

        '------------------------------------------------------------------------------------------
        'Anfang exs-File einlesen
        If Not DatExs.OpenRead(GEN.PathExs) Then
            WorkerMsg(tMsgID.Err, "Failed to open file '" & GEN.PathExs & "'!", MsgSrc)
            DatExs = Nothing
            Return False
        End If

        ModAnz = CShort(DatExs.ReadLine(0))
        t_amb = CSng(DatExs.ReadLine(0))
        iter_mode = CBool(DatExs.ReadLine(0))
        iter_pos = CShort(DatExs.ReadLine(0))
        iter_tol = CSng(DatExs.ReadLine(0))
        efm_mode = CShort(DatExs.ReadLine(0))
        cap_norm = CSng(DatExs.ReadLine(0))
        t_inl = CSng(DatExs.ReadLine(0))
        p_rel_inl = CSng(DatExs.ReadLine(0))
        'dummy = DatExs.ReadLine(0) 'alte dummy zeilen: auf exs-file kompatibilität achten
        'dummy = DatExs.ReadLine(0)
        'dummy = DatExs.ReadLine(0)
        fp4_efm = CSng(DatExs.ReadLine(0))

        cap = cap_norm * p_rated    'Hubraum in [liter]

        'Initialisieren der entsprechenden Anzahl an Modulen
        ReDim TempMod(ModAnz)
        For iMod = 1 To ModAnz
            TempMod(iMod) = New cTempMod(iMod, Me)
        Next

        'Lesen der Datenblöcke je Modul
        For iMod = 1 To ModAnz
            If Not TempMod(iMod).Read(DatExs) Then 'Dabei werden auch KonvMods initialisiert & Fileköpfe der entsprechenden Ausgabefiles geschrieben
                'Fehlermelderung in TempMod(iMod).Read(DatExs)
                DatExs.Close()
                DatExs = Nothing
                Return False
            End If
        Next

        DatExs.Close()
        'Ende exs-File einlesen
        '------------------------------------------------------------------------------------------

        If GEN.CoolantsimJa Then
            'Anfang csy-File einlesen
            If Not DatCsy.OpenRead(GEN.CoolantSimPath) Then
                WorkerMsg(tMsgID.Err, "Failed to open file '" & GEN.CoolantSimPath & "'!", MsgSrc)
                Return False
            End If

            't_amb_coolant = ...
            surf_engine = CSng(DatCsy.ReadLine(0)) 'Gesamtoberfläche Motor
            surf_temp_eng = CSng(DatCsy.ReadLine(0)) 'Oberflächentemperatur Motor
            eng_mass1 = CSng(DatCsy.ReadLine(0)) 'Masse der 1. künstl. Zwischenmasse
            eng_mass2 = CSng(DatCsy.ReadLine(0)) 'Masse der 2. künstl. Zwischenmasse
            h_cap_mass1 = CSng(DatCsy.ReadLine(0)) 'Wärmekapazität 1. Masse
            h_cap_mass2 = CSng(DatCsy.ReadLine(0)) 'Wärmekapazität 2. Masse
            alpha_A1 = CSng(DatCsy.ReadLine(0)) 'Wärmeübergangskoeffizient * Oberfläche 1
            alpha_A2 = CSng(DatCsy.ReadLine(0)) 'Wärmeübergangskoeffizient * Oberfläche 2
            ratio_h_to_m1 = CSng(DatCsy.ReadLine(0)) 'Anteil des Wärmestroms (ins Kühlsystem aus Kennfeld) in Masse 1
            ratio_h_to_m2 = CSng(DatCsy.ReadLine(0)) 'Anteil des Wärmestroms (ins Kühlsystem aus Kennfeld) in Masse 2
            t_coolant = CSng(DatCsy.ReadLine(0)) 'Temperatur Kühlmittel (wird hier als konstant angenommen)
            t_start_m1_m2 = CSng(DatCsy.ReadLine(0)) 'Starttemperatur für Masse 1 und 2

            DatCsy.Close()
            'Ende csy-File einlesen
            '------------------------------------------------------------------------------------------
        End If




        '------------------------------------------------------------------------------------------


lb100:  'Rücksprunglabel für iterativen Berechnungsmodus
        '------------------------------------------------------------------------------------------
        'Berechnungsschleife: je Zeitschritt /je Modul: 1. Temperaturen, 2.Konvertierungen
        While True
            'Sekündliche Ergebnisse werden in jeder Iteration ausgegeben
            If Not (PHEMmode = tPHEMmode.ModeADVANCE) Then
                If Not (PHEMmode = tPHEMmode.ModeBATCH) Or Cfg.ModOut Then

                    ' Header *.ter schreiben
                    DatTer.OpenWrite(PathTer, ",")
                    DatTer.WriteLine("result-file for temperatures in the exhaust system")
                    DatTer.WriteLine("VECTO " & VECTOvers)
                    line1 = "time,vehicle speed,nnorm,P_norm,rpm,fuel consumption,lambda,m_p exhaust,ta_41_qs"
                    line2 = "[s],[km/h],[-],[-],[min-1],[g/h],[-],[kg/s],[°C]"
                    For ii = 1 To ModAnz
                        If TempMod(ii).TgasGegJa Then
                            line1 = line1 & ",tm_" & ii & "_output" & ",ttc_" & ii & "_output" & ",tgas_" & ii & "_output"
                        Else
                            line1 = line1 & ",tm_" & ii & ",ttc_" & ii & ",tgas_" & ii
                        End If
                        line2 = line2 & ",[°C],[°C],[°C]"
                    Next
                    line1 = line1 & ",Q_p coolant"
                    line2 = line2 & ",[W]"

                    DatTer.WriteLine(line1)
                    DatTer.WriteLine(line2)

                    ' Header der KonvMods schreiben
                    For ii = 1 To ModAnz
                        If TempMod(ii).ModTyp = 1 Or TempMod(ii).ModTyp = 2 Then
                            TempMod(ii).KonvMod.Header()
                        End If
                    Next

                End If
            End If

            'startwerte für kühlersimulation:
            t_mass1(0) = t_start_m1_m2
            t_mass2(0) = t_start_m1_m2


            'Wärmeeintrag ins Kühlsystem (Kennfeld)
            If GEN.CoolantsimJa Then
                If MODdata.Em.EmDefComp.ContainsKey(tMapComp.Qp_coolant) Then
                    qp_coolant = MODdata.Em.EmDefComp(tMapComp.Qp_coolant).FinalVals
                Else
                    WorkerMsg(tMsgID.Err, "qp_coolant not found!", MsgSrc)
                    Return False
                End If

            End If



            For jz = 0 To t1

                'Kühlsystem Simulation
                If GEN.CoolantsimJa Then
                    'Wärmeeinträge in Massen 1 und 2
                    qp_coolant1(jz) = ratio_h_to_m1 * qp_coolant(jz)
                    qp_coolant2(jz) = ratio_h_to_m2 * qp_coolant(jz)

                    'Wärmeübergang Masse 1 und 2 ins Kühlsystem
                    qp_loss1(jz) = alpha_A1 * (t_mass1(jz) - t_coolant)
                    qp_loss2(jz) = alpha_A2 * (t_mass2(jz) - t_coolant)

                    'Massentemperaturen für nächsten Zeitschritt
                    If jz <> t1 Then
                        t_mass1(jz + 1) = t_mass1(jz) + (qp_coolant1(jz) - qp_loss1(jz)) / (eng_mass1 * h_cap_mass1)
                        t_mass2(jz + 1) = t_mass2(jz) + (qp_coolant2(jz) - qp_loss2(jz)) / (eng_mass2 * h_cap_mass2)
                    End If

                    'Wärmeverlust nach außen
                    qp_out(jz) = 0.21 * vehspe(jz) * surf_engine * (surf_temp_eng - t_amb)

                    'Gesamtwärmeeintrag ins Kühlsystem (Ausgabewert der Kühlersimulation)
                    qp_loss(jz) = qp_loss1(jz) + qp_loss2(jz) - qp_out(jz)
                End If


                'EXS Simulation
                If MODdata.EngState(jz) = tEngState.Stopped Then
                    mpexh(jz) = 0
                Else
                    If efm_mode = 0 Then
                        'Berechnung Abgasmassenstrom aus gegebenem Kraftstoffverbrauch und Lambda
                        'nur zulässig bei Motoren ohne AGR
                        'Einheit mpexh.......[kg/s]
                        'Einheit Vpexh.......[m3/s]
                        'Fall 1: Berechnung aus Verbrauch und lambda
                        mpexh_fc = ((14.7 * lambda(jz)) + 1) * fc(jz) / 3600000
                        '!Fall 2: Berechnung aus durch Motor gepumpter Luftmenge
                        Vpexh_mot = ((cap / 1000) / 2) * (rpm(jz) / 60)
                        zaehler = ((1 + (p_rel_inl / 1000)) * 100000) * Vpexh_mot
                        nenner = (R_air * (t_inl + 273.15))
                        mpexh_mot = zaehler / nenner
                        mpexh(jz) = Math.Max(mpexh_fc, mpexh_mot)
                    ElseIf efm_mode = 1 Then
                        'Es fehlt: Methodik Massenstromberechnung für AGR Motoren BMW HERO Projekt
                        mpexh(jz) = MODdata.Em.EmDefComp(tMapComp.MassFlow).FinalVals(jz)
                    Else
                        WorkerMsg(tMsgID.Err, "Ungültige Auswahl für Abgasmassenstromberechnung", MsgSrc)
                        Return False
                    End If
                End If

                'Lambda berechnen falls nicht explizit gegeben
                If Not LambdaGegJa Then
                    If fc(jz) < 1 Then
                        lambda.Add(1000)
                    Else
                        lambda.Add((mpexh(jz) - fc(jz) / 3600000) / (14.6 * fc(jz) / 3600000))
                    End If
                End If

                '------------------------------------------------------------------------------------------
                '-!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!-
                '------------------------------------------------------------------------------------------

                'Erstes Modul im Abgasstrang kann kein katalytisch aktives Element sein,
                'daher sind die Emissionen immer gleich der Rohemissionen aus dem PHEM-Hauptprogramm
                If eNOxOK Then TempMod(1).eEmKomp(2, jz) = NOx(jz)
                If eHCOK Then TempMod(1).eEmKomp(3, jz) = HC(jz)
                If eCOOK Then TempMod(1).eEmKomp(4, jz) = CO(jz)
                If ePMOK Then TempMod(1).eEmKomp(5, jz) = MODdata.Em.EmDefComp(tMapComp.PM).FinalVals(jz)
                If eEk1OK Then TempMod(1).eEmKomp(6, jz) = MODdata.Em.EmDefComp(tMapComp.PN).FinalVals(jz)
                If eEk2OK Then TempMod(1).eEmKomp(7, jz) = MODdata.Em.EmDefComp(tMapComp.NO).FinalVals(jz)

                For iMod = 1 To ModAnz
                    TempMod(iMod).HtMass(jz) 'Berechnung WÜ zwischen Abgas und 0d-Massenelement
                    TempMod(iMod).HtTc(jz) 'Berechnung WÜ zwischen Abgas und Thermoelement
                    If TempMod(iMod).ModTyp = 1 Or TempMod(iMod).ModTyp = 2 Then 'SCR oder 3-W Kat

                        TempMod(iMod).KonvMod.Konv(jz) 'Berechnung Konvertierung der Emissionskomponenten

                        If (TempMod(iMod).ModTyp = 2) Then
                            'Qp_reak berechnen: massenstrom * konvertierungsrate * reaktionsenthalpie / molmasse
                            TempMod(iMod).Qp_reak(jz) = CO(jz) / 3600 * TempMod(iMod).KonvMod.KonvRate(tMapComp.CO)(jz) * H_reak_co / M_co + _
                                HC(jz) / 3600 * TempMod(iMod).KonvMod.KonvRate(tMapComp.HC)(jz) * H_reak_hc / M_hc + _
                                NOx(jz) / 3600 * TempMod(iMod).KonvMod.KonvRate(tMapComp.NOx)(jz) * H_reak_nox / M_nox
                            'Schadstoffkomponente berechnen
                            For iSchad = 2 To 8
                                TempMod(iMod).eEmKomp(iSchad, jz) = TempMod(iMod - 1).eEmKomp(iSchad, jz)
                            Next iSchad
                            'Konvertierung von NOx, CO, HC -> alter Wert * (1-Konvertierungsrate)
                            TempMod(iMod).eEmKomp(2, jz) = TempMod(iMod - 1).eEmKomp(2, jz) * (1 - TempMod(iMod).KonvMod.KonvRate(tMapComp.NOx)(jz))
                            TempMod(iMod).eEmKomp(3, jz) = TempMod(iMod - 1).eEmKomp(3, jz) * (1 - TempMod(iMod).KonvMod.KonvRate(tMapComp.HC)(jz))
                            TempMod(iMod).eEmKomp(4, jz) = TempMod(iMod - 1).eEmKomp(4, jz) * (1 - TempMod(iMod).KonvMod.KonvRate(tMapComp.CO)(jz))
                            TempMod(iMod).eEmKomp(7, jz) = TempMod(iMod - 1).eEmKomp(7, jz) * (1 - TempMod(iMod).KonvMod.KonvRate(tMapComp.NO)(jz))
                        End If

                        If Not (PHEMmode = tPHEMmode.ModeADVANCE) Then
                            If Not (PHEMmode = tPHEMmode.ModeBATCH) Or Cfg.ModOut Then
                                TempMod(iMod).KonvMod.Write(jz) 'Sekündliche Ausgabe
                            End If
                        End If

                    Else
                        'Falls Modul kein Konv-Element hat ändert sich nix (Anmerkung: Modul 1 hat immer ModTyp0)
                        If iMod > 1 Then

                            TempMod(iMod).Qp_reak(jz) = 0
                            For iSchad = 2 To 8
                                TempMod(iMod).eEmKomp(iSchad, jz) = TempMod(iMod - 1).eEmKomp(iSchad, jz)
                            Next iSchad

                        End If
                    End If

                Next

                'Zeile in *.ter schreiben
                If Not (PHEMmode = tPHEMmode.ModeADVANCE) Then
                    If Not (PHEMmode = tPHEMmode.ModeBATCH) Or Cfg.ModOut Then
                        line1 = jz & "," & vehspe(jz) & "," & nnorm(jz) & "," & p_norm(jz) & "," & rpm(jz) & "," & fc(jz) & "," & lambda(jz) & "," & mpexh(jz) & "," & tqs(jz)
                        For ij = 1 To ModAnz
                            line1 = line1 & "," & TempMod(ij).t_m(jz) & "," & TempMod(ij).t_tc(jz) & "," & TempMod(ij).t_gas(jz)
                        Next
                        If GEN.CoolantsimJa Then
                            line1 = line1 & "," & qp_loss(jz)
                        End If
                        DatTer.WriteLine(line1)
                    End If
                End If

                '------------------------------------------------------------------------------------------
                '-!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!-
                '------------------------------------------------------------------------------------------

            Next
            'Ende Berechnungsschleife
            '----------------------------------------------------------------------------------------------

            'Alle sekündlichen Ergebnisfiles zumachen
            If Not (PHEMmode = tPHEMmode.ModeADVANCE) Then
                If Not (PHEMmode = tPHEMmode.ModeBATCH) Or Cfg.ModOut Then
                    DatTer.Close()
                    For ii = 1 To ModAnz
                        If TempMod(ii).ModTyp = 1 Or TempMod(ii).ModTyp = 2 Then
                            TempMod(ii).KonvMod.Close()
                        End If
                    Next
                End If
            End If

            '---------- Abfrage Rücksprung im iterativen Berechnungsmodus für Starttemp -------------------
            If (iter_mode = True) Then
                'Abbruchbedingung: Temperatur des Massenelementes "t_m" des in "iter_pos" spezifizierten Moduls
                'am Beginn und am Ende des Zyklus innerhalb vorzugebender Bandbreite "iter_tol"
                Diff = Math.Abs(TempMod(iter_pos).t_m(t1) - TempMod(iter_pos).t_m(0))
                If (Diff > iter_tol) Then
                    n_iter = n_iter + 1
                    For ii = 1 To ModAnz
                        TempMod(ii).t_m(0) = TempMod(ii).t_m(t1)
                    Next ii
                Else
                    Exit While
                End If
            Else
                Exit While
            End If
        End While
        '!------------------------------------------------------------------------

        If eNOxOK Then
            MODdata.Em.EmDefComp(tMapComp.NOx).InitAT()
            Em0 = New List(Of Single)
            For jz = 0 To t1
                Em0.Add(TempMod(ModAnz).eEmKomp(2, jz))
            Next
            MODdata.Em.EmDefComp(tMapComp.NOx).ATVals = Em0
            MODdata.Em.EmDefComp(tMapComp.NOx).FinalVals = Em0
        End If

        If eHCOK Then
            MODdata.Em.EmDefComp(tMapComp.HC).InitAT()
            Em0 = New List(Of Single)
            For jz = 0 To t1
                Em0.Add(TempMod(ModAnz).eEmKomp(3, jz))
            Next
            MODdata.Em.EmDefComp(tMapComp.HC).ATVals = Em0
            MODdata.Em.EmDefComp(tMapComp.HC).FinalVals = Em0
        End If

        If eCOOK Then
            MODdata.Em.EmDefComp(tMapComp.CO).InitAT()
            Em0 = New List(Of Single)
            For jz = 0 To t1
                Em0.Add(TempMod(ModAnz).eEmKomp(4, jz))
            Next
            MODdata.Em.EmDefComp(tMapComp.CO).ATVals = Em0
            MODdata.Em.EmDefComp(tMapComp.CO).FinalVals = Em0
        End If

        If ePMOK Then
            MODdata.Em.EmDefComp(tMapComp.PM).InitAT()
            Em0 = New List(Of Single)
            For jz = 0 To t1
                Em0.Add(TempMod(ModAnz).eEmKomp(5, jz))
            Next
            MODdata.Em.EmDefComp(tMapComp.PM).ATVals = Em0
            MODdata.Em.EmDefComp(tMapComp.PM).FinalVals = Em0
        End If

        If eEk1OK Then
            MODdata.Em.EmDefComp(tMapComp.PN).InitAT()
            Em0 = New List(Of Single)
            For jz = 0 To t1
                Em0.Add(TempMod(ModAnz).eEmKomp(6, jz))
            Next
            MODdata.Em.EmDefComp(tMapComp.PN).ATVals = Em0
            MODdata.Em.EmDefComp(tMapComp.PN).FinalVals = Em0
        End If

        If eEk2OK Then
            MODdata.Em.EmDefComp(tMapComp.NO).InitAT()
            Em0 = New List(Of Single)
            For jz = 0 To t1
                Em0.Add(TempMod(ModAnz).eEmKomp(7, jz))
            Next
            MODdata.Em.EmDefComp(tMapComp.NO).ATVals = Em0
            MODdata.Em.EmDefComp(tMapComp.NO).FinalVals = Em0
        End If

        '--- Ausgabefile *.ter schreiben ----------------------------------------------------------

        '--- Ende Ausgabefile *.ter schreiben -----------------------------------------------------


        'Aufräumen
        TempMod = Nothing

        Return True

    End Function


    ''' <summary>
    ''' Klasse für Temperaturmodule
    ''' </summary>
    ''' <remarks>Art des Moduls wird mit ModTyp definiert</remarks>
    Class cTempMod
        Dim Id As Int16
        Public ModTyp As Int16, PathConv As String, m_norm As Single, heat_cap_m As Single, t_m_init As Single 'Normierte Masse, Wärmekapazität Masse, Starttemp. Masse
        Public ext_surface As Single, emissivity As Single, A_fak As Single, B_fak As Single, C_fak As Single 'Oberfl. außen, Emissivität der Masse, Faktoren für Wärmeübergang
        Public T_relation_k As Single, T_relation_d As Single
        Public FlowTyp As Int16, cht_norm As Single, crad_norm As Single, cs_norm As Single
        Public p_rel_bk As Single, d_tc As Single, d_soot As Single
        Public t_gas() As Single        'Temperatur Abgas an Stelle Thermoelement
        Public t_m() As Single          'Temperatur Masse
        Public t_tc() As Single         'Temperatur Thermoelement
        Public Qp_exh() As Single       'Wärmestrom Masse -> Abgas
        Public Qp_loss() As Single      'Wärmestrom Masse -> Umgebung
        Public Qp_reak() As Single      'Wärmestrom Reaktion -> Masse
        Public eEmKomp(,) As Single     'Emissionskomponenten

        Private T_m_Abk As cAbkKurv
        Private T_tc_Abk As cAbkKurv
        Private NoCoolDown As Boolean

        Public mass As Single, density As Single, cs_pipe As Single 'Masse, Dichte, Querschnitt
        Public d_pipe As Single, l_pipe As Single, thickness_pipe As Single, Cht As Single, Crad As Single 'Durchmesser, Länge, Dicke, Wärmeübergang Konvektion und Strahlung

        Public cl_cyl As Single, cs_tc As Single, m_tc As Single, surf_tc As Single 'Thermoelement: char. Länge, Querschnittsfl., Masse, Oberfläche

        Private dummy As String = ""
        Private ii As Long
        Private a As Single, b As Single

        Public KonvMod As KonvInterf

        Private myEXS As cEXS

        Private bTgasGegJa As Boolean 'True wenn Abgastemperatur für Modul vorgegeben (Seiteneingang)
        Private TgasGeg As List(Of Single) 'sekündliche Abgastemperatur für jeweiliges Modul aus npi bzw. dri


        Public ReadOnly Property TgasGegJa As Boolean
            Get
                Return bTgasGegJa
            End Get
        End Property

        Public Sub New(ByVal IDval As Int16, ByRef EXSref As cEXS)
            Id = IDval
            myEXS = EXSref
            ReDim t_gas(MODdata.tDim)
            ReDim t_m(MODdata.tDim)
            ReDim t_tc(MODdata.tDim)
            ReDim Qp_exh(MODdata.tDim)
            ReDim Qp_loss(MODdata.tDim)
            ReDim Qp_reak(MODdata.tDim)
            ReDim eEmKomp(8, MODdata.tDim)
            T_m_Abk = New cAbkKurv
            T_tc_Abk = New cAbkKurv
            TgasGeg = Nothing
        End Sub

        ''' <summary>
        ''' Einlesen der EXS-Datei
        ''' </summary>
        ''' <param name="Datei">Dateihandler</param>
        ''' <remarks></remarks>
        Public Function Read(ByVal Datei As cFile_V3) As Boolean
            Dim TmAbkPfad As String = ""
            Dim TtcAbkPfad As String = ""
            Dim MsgSrc As String

            MsgSrc = "EmCalc/EXS/Temp/Read"

            NoCoolDown = False
            bTgasGegJa = False

            If Id = 1 Then
                ModTyp = 0
            Else
                ModTyp = CShort(Datei.ReadLine(0))
            End If

            'Pfad für Konvertierungsraten bei Modulen mit Konvertierung
            If ModTyp = 1 Or ModTyp = 2 Then
                PathConv = Datei.ReadLine(0)
                PathConv = fFileRepl(PathConv, fPATH(GEN.PathExs))
            Else
                PathConv = ""
            End If

            'Initialisieren der Module & Einlesen des Parameterfiles je nach Modul
            Select Case ModTyp
                Case 0 'nach Turbolader
                    m_norm = CSng(Datei.ReadLine(0))
                    heat_cap_m = CSng(Datei.ReadLine(0))
                    t_m_init = CSng(Datei.ReadLine(0))
                    FlowTyp = CShort(Datei.ReadLine(0))
                    cht_norm = CSng(Datei.ReadLine(0))
                    crad_norm = CSng(Datei.ReadLine(0))
                    TmAbkPfad = Datei.ReadLine(0)
                    cs_norm = CSng(Datei.ReadLine(0))
                    p_rel_bk = CSng(Datei.ReadLine(0))
                    d_tc = CSng(Datei.ReadLine(0))
                    d_soot = CSng(Datei.ReadLine(0))
                    TtcAbkPfad = Datei.ReadLine(0)

                Case 1 'SCR
                    KonvMod = New cScrMod(Id, myEXS)
                    KonvMod.Read(PathConv)

                    m_norm = CSng(Datei.ReadLine(0))
                    heat_cap_m = CSng(Datei.ReadLine(0))
                    t_m_init = CSng(Datei.ReadLine(0))
                    FlowTyp = CShort(Datei.ReadLine(0))
                    cht_norm = CSng(Datei.ReadLine(0))
                    crad_norm = CSng(Datei.ReadLine(0))
                    TmAbkPfad = Datei.ReadLine(0)
                    cs_norm = CSng(Datei.ReadLine(0))
                    p_rel_bk = CSng(Datei.ReadLine(0))
                    d_tc = CSng(Datei.ReadLine(0))
                    d_soot = CSng(Datei.ReadLine(0))
                    TtcAbkPfad = Datei.ReadLine(0)

                Case 2 '3-Wege-Kat
                    KonvMod = New c3WayCatMod(Id, myEXS)
                    KonvMod.Read(PathConv)

                    m_norm = CSng(Datei.ReadLine(0))
                    heat_cap_m = CSng(Datei.ReadLine(0))
                    t_m_init = CSng(Datei.ReadLine(0))
                    FlowTyp = CShort(Datei.ReadLine(0))
                    'Wärmeübergangsfaktor
                    cht_norm = CSng(Datei.ReadLine(0))
                    'Oberfläche außen
                    ext_surface = CSng(Datei.ReadLine(0))
                    'Emissivität
                    emissivity = CSng(Datei.ReadLine(0))
                    'Faktoren für Wärmeübergänge nach außen
                    A_fak = CSng(Datei.ReadLine(0))
                    B_fak = CSng(Datei.ReadLine(0))
                    C_fak = CSng(Datei.ReadLine(0))
                    'Faktoren für Temperaturzusammenhang t_katsubstrat <-> t_kat_außen
                    T_relation_k = CSng(Datei.ReadLine(0))
                    T_relation_d = CSng(Datei.ReadLine(0))
                    'Abkühlkurve Masse
                    TmAbkPfad = Datei.ReadLine(0)
                    'normierte Querschnittsfläche
                    cs_norm = CSng(Datei.ReadLine(0))
                    'durchschnittlicher Gegendruck
                    p_rel_bk = CSng(Datei.ReadLine(0))
                    'Durchmesser Thermoelement
                    d_tc = CSng(Datei.ReadLine(0))
                    d_soot = CSng(Datei.ReadLine(0))
                    'Abkühlkurve Thermoelement
                    TtcAbkPfad = Datei.ReadLine(0)

                Case 3 'Rohrmodul
                    d_pipe = CSng(Datei.ReadLine(0))
                    l_pipe = CSng(Datei.ReadLine(0))
                    thickness_pipe = CSng(Datei.ReadLine(0))
                    density = CSng(Datei.ReadLine(0))
                    heat_cap_m = CSng(Datei.ReadLine(0))
                    t_m_init = CSng(Datei.ReadLine(0))
                    cht_norm = CSng(Datei.ReadLine(0))
                    emissivity = CSng(Datei.ReadLine(0))
                    'Faktoren für Wärmeübergänge nach außen
                    A_fak = CSng(Datei.ReadLine(0))
                    B_fak = CSng(Datei.ReadLine(0))
                    TmAbkPfad = Datei.ReadLine(0)
                    p_rel_bk = CSng(Datei.ReadLine(0))
                    d_tc = CSng(Datei.ReadLine(0))
                    d_soot = CSng(Datei.ReadLine(0))
                    TtcAbkPfad = Datei.ReadLine(0)

                Case 4 'SCR mit 10s Mittelwertbildung für HERO
                    KonvMod = New cScrMod_10sMW(Id, myEXS)

                    m_norm = CSng(Datei.ReadLine(0))
                    heat_cap_m = CSng(Datei.ReadLine(0))
                    t_m_init = CSng(Datei.ReadLine(0))
                    FlowTyp = CShort(Datei.ReadLine(0))
                    cht_norm = CSng(Datei.ReadLine(0))
                    crad_norm = CSng(Datei.ReadLine(0))
                    TmAbkPfad = Datei.ReadLine(0)
                    cs_norm = CSng(Datei.ReadLine(0))
                    p_rel_bk = CSng(Datei.ReadLine(0))
                    d_tc = CSng(Datei.ReadLine(0))
                    d_soot = CSng(Datei.ReadLine(0))
                    TtcAbkPfad = Datei.ReadLine(0)
            End Select

            'Check ob Tgas in Zyklus gegeben:
            If Id > 1 Then
                bTgasGegJa = DRI.ExsCompDef(tExsComp.Tgas, Id)
                If bTgasGegJa = True Then
                    TgasGeg = DRI.ExsComponents(tExsComp.Tgas)(Id)
                End If
            End If

            'Entnormierungen und Berechnung weiterer Größen


            If ModTyp <> 3 Then
                mass = m_norm * myEXS.p_rated
                cs_pipe = cs_norm * myEXS.p_rated 'Rohrquerschnitt prop. Nennleistung
                d_pipe = ((4 * cs_pipe) / myEXS.Pi_) ^ 0.5 'Rohrdurchmesser in mm
                Cht = cht_norm * d_pipe 'WÜ-Faktor prop. Rohrdurchmesser
            Else
                'Zusätzlich berechnete Parameter für Rohrmodule:
                ext_surface = d_pipe * 0.001 * myEXS.Pi_ * l_pipe * 0.001 'Oberfläche in m^2
                mass = ext_surface * thickness_pipe * 0.001 * density 'Masse in kg
                cs_pipe = (d_pipe * 0.5) ^ 2 * myEXS.Pi_ 'Querschnitt in mm^2

                Cht = cht_norm 'Faktor für Wärmeübergang, z.B. wg. Rohrkrümmung
            End If

            If ModTyp <> 2 And ModTyp <> 3 Then
                Crad = crad_norm * myEXS.p_rated 'Abstrahlung prop. Nennleistung (Begründung siehe Diss)
            End If

            'Für Strömungsberechnungen in SI-Einheiten wird Querschnittsfäche in m2 umgerechnet
            cs_pipe = cs_pipe * (0.001 ^ 2)

            'Geometrische Größen berechnen
            'Anmerkung: es wird davon ausgegangen, dass Temperatursensoren
            'mittig ins Rohr stehen
            cl_cyl = (d_tc * 0.001) * myEXS.Pi_ * 0.5 'char. Länge umström Zyl
            cs_tc = (((d_tc * 0.001) / 2) ^ 2) * myEXS.Pi_ 'QS-Fläche t-sensor [m2]
            m_tc = cs_tc * ((d_pipe / 2) * 0.001) * myEXS.rho_steel 'Masse t_sensor [kg]
            surf_tc = (d_tc * 0.001) * myEXS.Pi_ * (d_pipe * 0.001) / 2 'Oberfläche t-sensor
            ' Anmerkung: Kugelkalotte an t-sensor spitze wird in der Betrachtung als
            ' umströmter Zylinder vernachlässigt

            'Abkühlkurven einlesen
            If Trim(UCase(TmAbkPfad)) = sKey.NoFile Then
                If myEXS.n_iter = 0 And PHEMmode = tPHEMmode.ModeSTANDARD Then WorkerMsg(tMsgID.Warn, "EXS-Module " & Id & ": No cool down curve for mass component defined. Cool down calculation disabled.", MsgSrc)
                NoCoolDown = True
            Else
                If Not T_m_Abk.Read(TmAbkPfad) Then
                    If myEXS.n_iter = 0 Then WorkerMsg(tMsgID.Err, "Error while reading cool down curve " & TmAbkPfad, MsgSrc)
                    Return False
                End If
            End If

            If Not NoCoolDown Then
                If TtcAbkPfad = sKey.NoFile Then
                    If myEXS.n_iter = 0 And PHEMmode = tPHEMmode.ModeSTANDARD Then WorkerMsg(tMsgID.Err, "EXS-Module " & Id & ": No cool down curve for thermocouple defined. Cool down calculation disabled.", MsgSrc)
                    NoCoolDown = True
                Else
                    If Not T_tc_Abk.Read(TtcAbkPfad) Then
                        If myEXS.n_iter = 0 Then WorkerMsg(tMsgID.Err, "Error while reading cool down curve " & TtcAbkPfad, MsgSrc)
                        Return False
                    End If
                End If
            End If

            Return True

        End Function

        ''' <summary>
        ''' Wärmeübergang Masse
        ''' </summary>
        ''' <param name="jz">Zeit</param>
        ''' <remarks></remarks>
        Public Sub HtMass(ByVal jz As Integer)
            Dim t_gas_in As Single, t_gas_out As Single, t_gas_mid As Single, dt_m As Single
            Dim t_ober As Single, t_unter As Single, t_gas_out_alt As Single
            Dim delta_t As Single, delta_tin As Single, delta_tout As Single
            Dim ht_exp As Single, dt_schwell As Single, dt_konv As Single
            Dim vp_exh_mid As Single, ht_factor As Single, Qp_ht As Single, Qp_thd As Single, diffQp As Single, diffTein As Single
            Dim Qp_loss_rad As Single, Qp_loss_konv As Single
            Dim i_iter As Integer
            Dim AbbruchIter As Boolean
            Dim cp_exh As Single
            Dim T_aussen As Single
            Dim Nus_exh As Single, Re_exh As Single

            'Festlegung Schwellwert für Genauigkeit der Temperaturberechnung (wird wegen iterativem Berechnungsmodus benötigt)
            dt_schwell = 0.01

            'Übergabe Eintrittstemperatur des Abgases aus oberhalb liegendem Modul bzw. dem Motor
            If Id = 1 Then
                t_gas_in = myEXS.tqs(jz) 'Motor
            Else

                If bTgasGegJa Then
                    t_gas_in = TgasGeg(jz)
                Else
                    t_gas_in = myEXS.TempMod(Id - 1).t_gas(jz) 'oberhalb liegendes Modul
                End If

            End If

            If FlowTyp = 0 Then
                ht_exp = 0 'Laminare Strömung. Keine Abhängigkeit Wärmeübergang von Volume Flow
            Else
                ht_exp = 0.8  'Turbulente Strömung. Keine Abhängigkeit Wärmeübergang von Volume Flow 
            End If

            'Berechnung der aktuellen Massentemperatur
            If ((jz = 0) And (myEXS.n_iter = 0)) Then
                t_m(jz) = t_m_init '!Sekunde 1: Temperatur auf Startwert setzen
                '! bei n_iter > 0 ist bereits der Endwert der letzten Iteration zugewiesen
            ElseIf (jz > 0) Then
                dt_m = (Qp_exh(jz - 1) + Qp_loss(jz - 1) - Qp_reak(jz - 1)) / (mass * heat_cap_m) 'Reaktionswärme geht derzeit zu 100% in die Katmasse
                t_m(jz) = t_m(jz - 1) - dt_m
            End If

            'Falls Motor Aus wird nach Abkühlkurve gerechnet und Methode verlassen:
            If MODdata.EngState(jz) = tEngState.Stopped Then
                t_gas(jz) = -1
                Qp_exh(jz) = -1
                Qp_loss(jz) = -1
                myEXS.tqs(jz) = -1
                If jz > 0 Then
                    If NoCoolDown Then
                        t_m(jz) = t_m(jz - 1)
                    Else
                        t_m(jz) = T_m_Abk.Temp(t_m(jz - 1))
                    End If
                End If
                Exit Sub
            End If

            diffTein = Math.Abs(t_gas_in - t_m(jz))
            If diffTein < dt_schwell Then 'Abfrage of Temperaturdifferenz zwischen Abgas und Masse am Eintritt sehr klein -> das mag log-Algorithmus nicht und muss daher explizit abgefangen werden

                t_gas_out = 0.5 * (t_gas_in + t_m(jz))
                t_gas_mid = 0.5 * (t_gas_in + t_gas_out)

                'Wärmekapazität (vgl. Bogdanic)
                cp_exh = 144.7 * (-3 * (0.0975 + (0.0485 / myEXS.lambda(jz) ^ 0.75)) * (t_gas_mid) ^ 2 / (10 ^ 6) + 2 * (7.768 + 3.36 / (myEXS.lambda(jz) ^ 0.8)) * (t_gas_mid) / (10 ^ 4) + (4.896 + 0.464 / (myEXS.lambda(jz) ^ 0.93))) + 287.7
                Qp_exh(jz) = myEXS.mpexh(jz) * cp_exh * (t_gas_out - t_gas_in)
                t_gas(jz) = t_gas_out

            Else

                i_iter = 0
                dt_konv = dt_schwell + 1
                AbbruchIter = False

                If t_gas_in > t_m(jz) Then
                    t_ober = t_gas_in
                    t_unter = t_m(jz)
                Else
                    t_ober = t_m(jz)
                    t_unter = t_gas_in
                End If

                'Schleife für Iteration Wärmeübergang
                While True
                    i_iter = i_iter + 1
                    If i_iter > 1 Then
                        t_gas_out_alt = t_gas_out
                    End If

                    t_gas_out = 0.5 * (t_ober + t_unter) 'Solver halbiert fortschreitend Intervall, in dem die Temperatur des aus der Masse austretenden Abgases sein muss
                    '                                     Abbruchkriterium siehe unten

                    If i_iter > 1 Then
                        dt_konv = Math.Abs(t_gas_out - t_gas_out_alt)
                    End If

                    'Ermittlung der Temperatur des Abgases in der Mitte der Masse ("t_gas_mid") aus einem nichtlinearem (logaritmisch) Temperaturverlauf 
                    delta_tin = t_m(jz) - t_gas_in
                    delta_tout = t_m(jz) - t_gas_out
                    delta_t = (delta_tin - delta_tout) / Math.Log(delta_tin / delta_tout)
                    t_gas_mid = t_m(jz) - delta_t

                    vp_exh_mid = (myEXS.mpexh(jz) * myEXS.R_exh * (t_gas_mid + 273.15)) / ((1 + (p_rel_bk / 1000)) * 100000)

                    If ModTyp <> 3 Then
                        'Wärmeübergang Konvektion innen für alle Module (außer Rohr)
                        ht_factor = myEXS.hc_exh(t_gas_mid) * ((vp_exh_mid / myEXS.kv_exh(t_gas_mid)) ^ ht_exp)
                        Qp_ht = Cht * ht_factor * delta_t 'Cht -> hier: Faktor für Wärmeübergang
                    Else
                        'für Rohrmodule:
                        Re_exh = d_pipe * 0.001 * (vp_exh_mid / cs_pipe) / myEXS.kv_exh(t_gas_mid) 'd_pipe in m
                        'Nusselt Zahl: Dichte = 345/t_gas_mid, Term in Klammer: mu_Rohr / mu_Mitte
                        Nus_exh = 0.027 * Re_exh ^ 0.8 * myEXS.Pr_exh ^ 0.333 * ((myEXS.kv_exh(t_gas_mid) * 345 / t_gas_mid) / (myEXS.kv_exh(t_m(jz)) * 345 / t_m(jz))) ^ 0.14
                        'Wärmeübergang (Konvektion innen), d_pipe in m: char. Länge
                        Qp_ht = Cht * Nus_exh * ext_surface * (myEXS.hc_exh(t_gas_mid) / (d_pipe * 0.001)) * delta_t 'Cht -> hier: Faktor für Rohrkrümmung
                    End If

                    'Wärmekapazität (vgl. Bogdanic)
                    cp_exh = 144.7 * (-3 * (0.0975 + (0.0485 / myEXS.lambda(jz) ^ 0.75)) * (t_gas_mid) ^ 2 / 10 ^ 6 + 2 * (7.768 + 3.36 / myEXS.lambda(jz) ^ 0.8) * (t_gas_mid) / 10 ^ 4 + (4.896 + 0.464 / myEXS.lambda(jz) ^ 0.93)) + 287.7
                    Qp_thd = myEXS.mpexh(jz) * cp_exh * (t_gas_out - t_gas_in)
                    diffQp = Qp_ht - Qp_thd

                    'Abbruchkriterium: Änderung der Abgas-Austrittstemperatur im Vergleich zum letzten Iterationsschritt kleiner Schwellwert
                    If dt_konv < dt_schwell Then AbbruchIter = True

                    If AbbruchIter Then
                        Qp_exh(jz) = Qp_thd
                        t_gas(jz) = t_gas_out
                        Exit While
                    Else
                        If diffQp > 0 Then
                            t_unter = t_gas_out
                        ElseIf diffQp < 0 Then
                            t_ober = t_gas_out
                        End If
                    End If
                End While
            End If


            'Berechnung der Wärmeverluste der "thermischen Masse" nach außen
            If ModTyp = 2 Then '3-W Kat
                'Parameter werden aus EXS-Datei eingelesen:
                'Daten für MuD:
                '   Oberfl_Kat = 0.12 'Oberfläche für Wärmeübergang in m^2
                '   Emiss = 0.5 'Emissivität
                '   A = 7, B = 0.21, C = 8, k = 1, d = -340

                'Empirische Formel, passt für alle Rollentests recht gut
                T_aussen = t_m(jz) - 340
                'Anm.: Versuch mit direkter Abhängigkeit von t_m -> funktioniert nicht gut

                'Wärmeverlust durch Strahlung
                Qp_loss_rad = C_fak * ext_surface * myEXS.St_Boltz * emissivity * ((T_aussen + 273.15) ^ 4 - (myEXS.t_amb + 273.15) ^ 4)
                'Wärmeverlust durch Konvektion
                Qp_loss_konv = (B_fak * myEXS.vehspe(jz) + A_fak) * ext_surface * (T_aussen - myEXS.t_amb)

            ElseIf ModTyp = 3 Then 'Rohr
                'Parameter werden aus EXS-Datei eingelesen:
                'Daten für MuD:
                '   Modul Nr. 3:
                '       Oberfl_Mod3 = 0.169457508 'Oberfläche für Wärmeübergang in m^2
                '       Emiss = 0.5 'Emissivität
                '       A = 7
                '       B = 0.42
                '   Modul Nr. 4:
                '       Oberfl_Mod4 = 0.103596481 'Oberfläche für Wärmeübergang in m^2
                '       Emiss = 0.9 'Emissivität
                '       A = 7
                '       B = 0

                'Wärmeverlust durch Strahlung = Sichtfaktor * Emissivität * St.-Boltzm.-Konst * Oberfläche * (T_Rohr^4 - T_Umgebung^4)
                Qp_loss_rad = emissivity * myEXS.St_Boltz * ext_surface * ((t_m(jz) + 273.15) ^ 4 - (myEXS.t_amb + 273.15) ^ 4)
                'Wärmeverlust durch Konvektion = Wärmeübergangskoeffizient * Oberfläche * (T_Rohr - T_Umgebung)
                Qp_loss_konv = (B_fak * myEXS.vehspe(jz) + A_fak) * ext_surface * (t_m(jz) - myEXS.t_amb)
            Else
                'Standard: Crad konstant, keine Verluste durch Konvektion
                Qp_loss_rad = Crad * (((t_m(jz) + 273.15) ^ 4) - ((myEXS.t_amb + 273.15) ^ 4))
                Qp_loss_konv = 0
            End If

            'Gesamtwärmeverlust
            Qp_loss(jz) = Qp_loss_rad + Qp_loss_konv


        End Sub

        ''' <summary>
        ''' Wärmeübergang Thermoelement
        ''' </summary>
        ''' <param name="jz">Zeit</param>
        ''' <remarks></remarks>
        Public Sub HtTc(ByVal jz As Integer)
            Dim vp_exh_tc As Single, vms As Single
            Dim Re As Single, Nu_lam As Single, Nu_turb As Single, Nu_ave As Single, alpha_conv As Single, alpha_tot As Single
            Dim k_pt1 As Single

            If jz = 0 Then
                t_tc(0) = t_m(0) 'Als Startwert für das Thermoelement wird die Temperatur der thermischen Masse verwendet
            Else

                'Falls Motor Aus wird nach Abkühlkurve gerechnet und Methode verlassen:
                If MODdata.EngState(jz) = tEngState.Stopped Then
                    If NoCoolDown Then
                        t_tc(jz) = t_tc(jz - 1)
                    Else
                        t_tc(jz) = T_tc_Abk.Temp(t_tc(jz - 1))
                    End If
                    Exit Sub  '!!!!!!!!!
                End If

                vp_exh_tc = (myEXS.mpexh(jz) * myEXS.R_exh * (t_gas(jz) + 273.15)) / ((1 + (p_rel_bk / 1000)) * 100000) '!lokaler Volumenstrom im [m3/s]
                vms = vp_exh_tc / cs_pipe '!lokale Strömungsgeschwindigkeit [m/s]

                '!Formelwerk Berechnung Wärmeübergang am umströmten Zylinder
                Re = (cl_cyl * vms) / myEXS.kv_exh(t_gas(jz))
                Nu_lam = 0.664 * (Re ^ 0.5) * (myEXS.Pr_exh ^ 0.333) 'Nusselt laminar
                Nu_turb = 0.037 * ((Re ^ 0.8) * myEXS.Pr_exh) / (1 + 2.443 * (Re ^ (-0.1)) * ((myEXS.Pr_exh ^ 0.667) - 1)) 'Nusselt turbulent
                Nu_ave = (0.3 + (((Nu_lam ^ 2) + (Nu_turb ^ 2)) ^ 0.5)) * (((t_gas(jz) + 273.15) / (t_tc(jz - 1) + 273.15)) ^ 0.12) 'Nusselt durchschnittlich

                alpha_conv = Nu_ave * myEXS.hc_exh(t_gas(jz)) / cl_cyl
                alpha_tot = 1 / ((1 / alpha_conv) + ((d_soot * 0.001) / myEXS.hc_soot))

                'Vereinfachte Lösung der Wärmeflussgleichung für den t-sensor
                'entspricht einer Diffgl. für ein PT1 glied
                k_pt1 = (m_tc * myEXS.cp_steel) / (alpha_tot * surf_tc)
                'Zeitdiskrete Lösung der PT1-Diffgl
                t_tc(jz) = (1 / (k_pt1 + 1)) * (t_gas(jz) + (k_pt1 * t_tc(jz - 1)))
            End If

        End Sub

        Private Class cAbkKurv
            Private tAr As Single()
            Private TempAr As Single()
            Private Adim As Int16


            Public Function Read(ByVal sPath As String) As Boolean

                Dim f As cFile_V3
                Dim line As String()
                Dim tList As System.Collections.Generic.List(Of Single)
                Dim TempList As System.Collections.Generic.List(Of Single)


                sPath = fFileRepl(sPath, fPATH(GEN.PathExs))

                If Not IO.File.Exists(sPath) Then Return False

                f = New cFile_V3
                If Not f.OpenRead(sPath) Then
                    f = Nothing
                    Return False
                End If

                tList = New System.Collections.Generic.List(Of Single)
                TempList = New System.Collections.Generic.List(Of Single)

                Do While Not f.EndOfFile
                    line = f.ReadLine
                    tList.Add(line(0))
                    TempList.Add(line(1))
                Loop

                tAr = tList.ToArray.Clone
                TempAr = TempList.ToArray.Clone

                Adim = UBound(tAr)

                f.Close()
                f = Nothing

                tList = Nothing
                TempList = Nothing

                Return True

            End Function

            Public Function Temp(ByVal LastTemp As Single) As Single
                Dim i As Int32
                Dim t As Single

                If TempAr(0) >= LastTemp Then
                    i = 0
                    Do While TempAr(i) > LastTemp And i < Adim
                        i += 1
                    Loop
                Else
                    i = 1
                End If

                ''Extrapolation für LastTemp > TempAr(0)
                'If TempAr(0) < LastTemp Then
                '    'TODO: StatusMSG(8, "Extrapolation of Cool Down Temperature! t = " & jz & ", Temp(t-1) = " & LastTemp)
                '    i = 1
                '    GoTo lbInt
                'End If

                'i = 0
                'Do While TempAr(i) > LastTemp And i < Adim
                '    i += 1
                'Loop

                'Extrapolation für LastTemp < TempAr(Adim)

                'lbInt:
                'Interpolation
                If TempAr(i) - TempAr(i - 1) = 0 Then
                    t = tAr(i - 1)
                Else
                    t = (LastTemp - TempAr(i - 1)) * (tAr(i) - tAr(i - 1)) / (TempAr(i) - TempAr(i - 1)) + tAr(i - 1)
                End If

                'Einen Zeitschritt vor ( = 1 Sekunde)
                t += 1

                i = 0
                Do While tAr(i) < t And i < Adim
                    i += 1
                Loop

                If tAr(i) - tAr(i - 1) = 0 Then
                    Return TempAr(i - 1)
                Else
                    Return (t - tAr(i - 1)) * (TempAr(i) - TempAr(i - 1)) / (tAr(i) - tAr(i - 1)) + TempAr(i - 1)
                End If

            End Function

        End Class

    End Class

    ''' <summary>
    ''' SCR Modell
    ''' </summary>
    ''' <remarks></remarks>
    Class cScrMod 'SCR Modell

        Implements KonvInterf

        'Klasse initialisiert als Unterelement von TempMod

        Dim DatKonvOut As New cFile_V3    'PHEM SCR model result file
        Dim PathKonvOut As String = ""

        Dim t_SCR As Single
        'c     Prefix "c" bedeutet: Zykluswert verwendet für Kennlinien-Korrektur
        Dim ct_up As Single, cNOxraw As Single, cNOx60s As Single, cSV As Single
        'c     Index "cc" bedeutet: Wert aus Kennlinie (-> "c" - "cc" ist die Differenz, mit der Korrigiert wird)
        Dim denox_cc As Single, t_up_cc As Single, NOxraw_cc As Single, NOx60s_cc As Single, SV_cc As Single, deNOxmin As Single
        Dim deNOx_cor As Single '! deNOx-Wert nach Korrektur as single, vor End-Filterung as single
        Dim deNOx As Single '! Endwert zur Berechnung von NOx-tailpipe as single
        'Filename sekündliches Ausgabefile spezifizieren

        Dim pt_SCR(izykt) As Single
        Dim pdeNOx(izykt) As Single
        Dim pt_up(izykt) As Single
        Dim pNOxraw(izykt) As Single
        Dim pNOx60s(izykt) As Single
        Dim pSV(izykt) As Single
        Dim Sharetdown As Single

        Dim DeNOxMax As Single
        Dim tminCor As Single
        Dim tmaxCor As Single
        Dim Cor_tup As Single
        Dim Cor_NOxraw As Single
        Dim Cor_NOx60s As Single
        Dim Cor_SV As Single
        Dim specCatVol As Single
        Dim iSCRAnz As Integer


        Dim KonvRate(8) As Single
        Dim iSchad As Long
        Dim id As Int16

        Private myEXS As cEXS

        Public Property DummyKonvRate As Dictionary(Of tMapComp, List(Of Single)) Implements KonvInterf.KonvRate

        Public Sub New(ByVal i As Int16, ByRef EXSref As cEXS)
            id = i
            myEXS = EXSref

            PathKonvOut = MODdata.ModOutpName & "_Mod" & id & ".den"

        End Sub


        Public Function Read(ByVal Name As String) As Boolean Implements KonvInterf.Read
            Dim DatKonvIn As New cFile_V3    'PHEM inputfile with definition of exhaust system structure
            Dim iz As Long
            Dim line() As String
            Dim MsgSrc As String

            MsgSrc = "EmCalc/EXS/SCR/Read"

            If Not DatKonvIn.OpenRead(Name) Then
                WorkerMsg(tMsgID.Err, "Cannot read " & Name & "!", MsgSrc)
                DatKonvIn = Nothing
                Return False
            End If

            'Abbruch wenn kein NOx gegeben
            If Not MAP.EmDefRef.ContainsKey(tMapComp.NOx) Then
                WorkerMsg(tMsgID.Err, "'NOx' not defined in emission map!", MsgSrc)
                Return False
            End If


            Sharetdown = CSng(DatKonvIn.ReadLine(0))
            DeNOxMax = CSng(DatKonvIn.ReadLine(0))
            specCatVol = CSng(DatKonvIn.ReadLine(0))
            tminCor = CSng(DatKonvIn.ReadLine(0))
            tmaxCor = CSng(DatKonvIn.ReadLine(0))
            Cor_tup = CSng(DatKonvIn.ReadLine(0))
            Cor_NOxraw = CSng(DatKonvIn.ReadLine(0))
            Cor_NOx60s = CSng(DatKonvIn.ReadLine(0))
            Cor_SV = CSng(DatKonvIn.ReadLine(0))
            DatKonvIn.ReadLine()
            DatKonvIn.ReadLine()
            DatKonvIn.ReadLine()

            iz = 0
            't-SCR (°C), deNOx(1-NOx-Auspuff/NOx-Roh), -t-upstream(°C), NOx-Roh (g/h)/kW_Nennleistg, Summe NOx ueber 60Sek vorher g/h)/kW_Nennleistg, Raumgeschwindigkeit (1/h)
            Do While Not DatKonvIn.EndOfFile
                iz = iz + 1
                line = DatKonvIn.ReadLine
                pt_SCR(iz) = CSng(line(0))
                pdeNOx(iz) = CSng(line(1))
                pt_up(iz) = CSng(line(2))
                pNOxraw(iz) = CSng(line(3))
                pNOx60s(iz) = CSng(line(4))
                pSV(iz) = CSng(line(5))
            Loop
            iSCRAnz = iz
            line = Nothing
            DatKonvIn.Close()
            DatKonvIn = Nothing

            Return True
        End Function


        Public Sub Konv(ByVal jz As Integer) Implements KonvInterf.Konv

            ' -----------------------------------------------------------------------
            ' Programm zur Simulation SCR-Flottendurchschnitt
            ' Anmerkung: deNOx-Werte kleiner als Null sind möglich:
            '            dies entspricht höherem Roh-Nox-Niveau als im Basiskennfeld
            ' -----------------------------------------------------------------------
            '
            Dim is0 As Long
            Dim such As Single
            Dim izpl As Int32
            Dim eNOx As System.Collections.Generic.List(Of Single)

            eNOx = MODdata.Em.EmDefComp(tMapComp.NOx).FinalVals
            '   
            '     -----------------------------------------------------------------
            '     1.) Berechnung der sekündlichen Werte für Eingangsgrößen SCR-Modell
            '
            '     a.) t_SCR: zusammengewichten der von t_upstream und t_downstream
            '     SCR-Model-intern werden dabei Temperaturen zwischen 50°C und 500°C begrenzt
            '     Temperaturmodelwerte (zB bei Kaltstart) werden nicht überschrieben

            t_SCR = ((1 - Sharetdown) * myEXS.TempMod(id - 1).t_tc(jz)) + (Sharetdown * myEXS.TempMod(id).t_tc(jz))
            t_SCR = Math.Max(50.001, t_SCR)
            t_SCR = Math.Min(499.999, t_SCR)
            '
            '     b.) t_up, NOxraw, SV: 20s gleitender Mittelwert in die Vergangenheit
            '         Formel gilt auch für die ersten 20 Sekunden
            '
            ct_up = 0
            cNOxraw = 0
            cSV = 0
            For is0 = Math.Max(0, jz - 19) To jz
                ct_up = ct_up + myEXS.TempMod(id - 1).t_tc(is0)
                cNOxraw = cNOxraw + (myEXS.TempMod(id - 1).eEmKomp(2, is0) / myEXS.p_rated)
                cSV = cSV + ((((myEXS.mpexh(is0) * 3600) / myEXS.p_rated) / 1.29) / (specCatVol * 0.001))
            Next is0
            ct_up = ct_up / (jz - Math.Max(0, jz - 19) + 1)
            cNOxraw = cNOxraw / (jz - Math.Max(0, jz - 19) + 1)
            cSV = cSV / (jz - Math.Max(0, jz - 19) + 1)
            '
            '     c.) NOx60s: Summe über letzten 60s der spezifischen NOx-Rohemissionen
            '         Formel gilt auch für die ersten 60 Sekunden

            cNOx60s = 0
            For is0 = Math.Max(0, jz - 59) To jz
                cNOx60s = cNOx60s + (eNOx(is0) / myEXS.p_rated)
            Next is0
            '        Für Sekunde 1 bis 59 muss summenwert hochgerechnet werden
            cNOx60s = cNOx60s / (Math.Min(60, jz + 1) / 60.0)

            '     -----------------------------------------------------------------
            '
            '
            '
            '     Berechnung deNOxmin aus Kennlinien-Wert bei 50°C
            For is0 = 1 To iSCRAnz
                myEXS.Xis(is0) = pt_SCR(is0)
                myEXS.Yis(is0) = pdeNOx(is0)
            Next is0
            such = 50.001
            izpl = iSCRAnz
            Call myEXS.Intlin(such, izpl)
            deNOxmin = such
            '
            '
            '     -----------------------------------------------------------------
            '     2.) Berechnung deNOx
            '
            '        a.) deNOx aus Kennlinie:
            For is0 = 1 To iSCRAnz
                myEXS.Xis(is0) = pt_SCR(is0)
                myEXS.Yis(is0) = pdeNOx(is0)
            Next is0
            such = t_SCR
            izpl = iSCRAnz
            Call myEXS.Intlin(such, izpl)
            denox_cc = such

            'c        b.) Falls Korrekturkriterien erfüllt sind: deNOx-Korrektur gegenüber Kennlinie
            If ((t_SCR > tminCor) And (t_SCR < tmaxCor)) Then
                'c
                'c           t_up aus Kennlinie:
                For is0 = 1 To iSCRAnz
                    myEXS.Xis(is0) = pt_SCR(is0)
                    myEXS.Yis(is0) = pt_up(is0)
                Next is0
                such = t_SCR
                izpl = iSCRAnz
                Call myEXS.Intlin(such, izpl)
                t_up_cc = such
                'c
                'c           NOx_raw aus Kennlinie:
                For is0 = 1 To iSCRAnz
                    myEXS.Xis(is0) = pt_SCR(is0)
                    myEXS.Yis(is0) = pNOxraw(is0)
                Next is0
                such = t_SCR
                izpl = iSCRAnz
                Call myEXS.Intlin(such, izpl)
                NOxraw_cc = such
                'c
                'c           Summe NOxraw in den letzten 60 Sekunden aus Kennlinie:
                For is0 = 1 To iSCRAnz
                    myEXS.Xis(is0) = pt_SCR(is0)
                    myEXS.Yis(is0) = pNOx60s(is0)
                Next is0
                such = t_SCR
                izpl = iSCRAnz
                Call myEXS.Intlin(such, izpl)
                NOx60s_cc = such
                'c
                'c           Raumgeschwindigkeit aus Kennlinie:
                For is0 = 1 To iSCRAnz
                    myEXS.Xis(is0) = pt_SCR(is0)
                    myEXS.Yis(is0) = pSV(is0)
                Next is0
                such = t_SCR
                izpl = iSCRAnz
                Call myEXS.Intlin(such, izpl)
                SV_cc = such
                'c
                deNOx_cor = denox_cc + Cor_tup * (ct_up - t_up_cc) + Cor_NOxraw * (cNOxraw - NOxraw_cc) + Cor_NOx60s * (cNOx60s - NOx60s_cc) + Cor_SV * (cSV - SV_cc)
                'c
                deNOx = Math.Max(Math.Min(deNOx_cor, DeNOxMax), deNOxmin)
                'c
            Else
                deNOx = Math.Max(Math.Min(denox_cc, DeNOxMax), deNOxmin)
            End If

            'Schreiben der Ergebnisse auf die standardisierten Variablen eEmKomp (iSchad, jz) und Qp_reak(jz)
            For iSchad = 2 To 8
                KonvRate(iSchad) = 0
            Next iSchad
            KonvRate(2) = deNOx
            KonvRate(7) = deNOx
            For iSchad = 2 To 8
                myEXS.TempMod(id).eEmKomp(iSchad, jz) = myEXS.TempMod(id - 1).eEmKomp(iSchad, jz) * (1 - KonvRate(iSchad))
            Next iSchad
            myEXS.TempMod(id).Qp_reak(jz) = 0
        End Sub


        Public Sub Header() Implements KonvInterf.Header
            DatKonvOut.OpenWrite(PathKonvOut)
            DatKonvOut.WriteLine("result-file VECTO SCR Model")
            DatKonvOut.WriteLine("VECTO " & VECTOvers)
            DatKonvOut.WriteLine("time,NOx_up,NOx_down,t_SCR,deNOx,deNOx_cc,t_tc_down,t_tc_up,20s t_tc_up,t_up_cc,NOx_raw,NOx_raw_cc,NOx60s,NOx60s_cc,SV,SV_cc ")
            DatKonvOut.WriteLine("[s],[g/h],[g/h],[°C],[1],[1],[°C],[°C],[°C],[°C],[g/(h*kW_rated)],[g/(h*kW_rated)],[g/(h*kW_rated)],[g/(h*kW_rated)],[h-1],[h-1]")
        End Sub


        Public Sub Write(ByVal jz As Integer) Implements KonvInterf.Write
            DatKonvOut.WriteLine(jz + 1 & "," & myEXS.TempMod(id - 1).eEmKomp(2, jz) & "," & myEXS.TempMod(id).eEmKomp(2, jz) & "," & t_SCR & "," & deNOx & "," & denox_cc & "," & myEXS.TempMod(id).t_tc(jz) & "," & myEXS.TempMod(id - 1).t_tc(jz) & "," & ct_up & "," & t_up_cc & "," & cNOxraw & "," & NOxraw_cc & "," & cNOx60s & "," & NOx60s_cc & "," & cSV & "," & SV_cc)
        End Sub


        Public Sub Close() Implements KonvInterf.Close
            DatKonvOut.Close()
        End Sub


    End Class


    ''' <summary>
    ''' SCR Modell
    ''' </summary>
    ''' <remarks></remarks>
    Class cScrMod_10sMW 'SCR Modell

        Implements KonvInterf

        'Klasse initialisiert als Unterelement von TempMod

        Dim DatKonvOut As New cFile_V3    'PHEM SCR model result file
        Dim PathKonvOut As String = ""

        Dim t_SCR As Single
        'c     Prefix "c" bedeutet: Zykluswert verwendet für Kennlinien-Korrektur
        Dim ct_up As Single, cNOxraw As Single, cNOx30s As Single, cSV As Single
        'c     Index "cc" bedeutet: Wert aus Kennlinie (-> "c" - "cc" ist die Differenz, mit der Korrigiert wird)
        Dim denox_cc As Single, t_up_cc As Single, NOxraw_cc As Single, NOx30s_cc As Single, SV_cc As Single, deNOxmin As Single
        Dim deNOx_cor As Single '! deNOx-Wert nach Korrektur as single, vor End-Filterung as single
        Dim deNOx As Single '! Endwert zur Berechnung von NOx-tailpipe as single
        'Filename sekündliches Ausgabefile spezifizieren

        Dim pt_SCR(izykt) As Single
        Dim pdeNOx(izykt) As Single
        Dim pt_up(izykt) As Single
        Dim pNOxraw(izykt) As Single
        Dim pNOx60s(izykt) As Single
        Dim pSV(izykt) As Single
        Dim Sharetdown As Single

        Dim DeNOxMax As Single
        Dim tminCor As Single
        Dim tmaxCor As Single
        Dim Cor_tup As Single
        Dim Cor_NOxraw As Single
        Dim Cor_NOx30s As Single
        Dim Cor_SV As Single
        Dim specCatVol As Single
        Dim iSCRAnz As Integer


        Dim KonvRate(8) As Single
        Dim iSchad As Long
        Dim id As Int16

        Private myEXS As cEXS

        Public Property DummyKonvRate As Dictionary(Of tMapComp, List(Of Single)) Implements KonvInterf.KonvRate



        Public Sub New(ByVal i As Int16, ByRef EXSref As cEXS)
            id = i
            myEXS = EXSref

            PathKonvOut = MODdata.ModOutpName & "_Mod" & id & ".den"

        End Sub


        Public Function Read(ByVal Name As String) As Boolean Implements KonvInterf.Read
            Dim DatKonvIn As New cFile_V3    'PHEM inputfile with definition of exhaust system structure
            Dim iz As Long
            Dim line() As String
            Dim MsgSrc As String

            MsgSrc = "EmCalc/EXS/SCR_10s/Read"

            If Not DatKonvIn.OpenRead(Name, , , True) Then
                WorkerMsg(tMsgID.Err, "Cannot read " & Name & "!", MsgSrc)
                DatKonvIn = Nothing
                Return False
            End If

            'Abbruch wenn kein NOx gegeben
            If Not MAP.EmDefRef.ContainsKey(tMapComp.NOx) Then
                WorkerMsg(tMsgID.Err, "'NOx' not defined in emission map!", MsgSrc)
                Return False
            End If

            Sharetdown = CSng(DatKonvIn.ReadLine(0))
            DeNOxMax = CSng(DatKonvIn.ReadLine(0))
            specCatVol = CSng(DatKonvIn.ReadLine(0))
            tminCor = CSng(DatKonvIn.ReadLine(0))
            tmaxCor = CSng(DatKonvIn.ReadLine(0))
            Cor_tup = CSng(DatKonvIn.ReadLine(0))
            Cor_NOxraw = CSng(DatKonvIn.ReadLine(0))
            Cor_NOx30s = CSng(DatKonvIn.ReadLine(0))
            Cor_SV = CSng(DatKonvIn.ReadLine(0))
            DatKonvIn.ReadLine()
            DatKonvIn.ReadLine()
            DatKonvIn.ReadLine()

            iz = 0
            't-SCR (°C), deNOx(1-NOx-Auspuff/NOx-Roh), -t-upstream(°C), NOx-Roh (g/h)/kW_Nennleistg, Summe NOx ueber 60Sek vorher g/h)/kW_Nennleistg, Raumgeschwindigkeit (1/h)
            Do While Not DatKonvIn.EndOfFile
                iz = iz + 1
                line = DatKonvIn.ReadLine
                pt_SCR(iz) = CSng(line(0))
                pdeNOx(iz) = CSng(line(1))
                pt_up(iz) = CSng(line(2))
                pNOxraw(iz) = CSng(line(3))
                pNOx60s(iz) = CSng(line(4))
                pSV(iz) = CSng(line(5))
            Loop
            iSCRAnz = iz
            line = Nothing
            DatKonvIn.Close()
            DatKonvIn = Nothing

            Return True

        End Function


        Public Sub Konv(ByVal jz As Integer) Implements KonvInterf.Konv

            ' -----------------------------------------------------------------------
            ' Programm zur Simulation SCR-Flottendurchschnitt
            ' Anmerkung: deNOx-Werte kleiner als Null sind möglich:
            '            dies entspricht höherem Roh-Nox-Niveau als im Basiskennfeld
            ' -----------------------------------------------------------------------
            '
            Dim is0 As Long
            Dim such As Single
            Dim izpl As Int32
            Dim eNOx As System.Collections.Generic.List(Of Single)

            eNOx = MODdata.Em.EmDefComp(tMapComp.NOx).FinalVals

            '   
            '     -----------------------------------------------------------------
            '     1.) Berechnung der sekündlichen Werte für Eingangsgrößen SCR-Modell
            '
            '     a.) t_SCR: zusammengewichten der von t_upstream und t_downstream
            '     SCR-Model-intern werden dabei Temperaturen zwischen 50°C und 500°C begrenzt
            '     Temperaturmodelwerte (zB bei Kaltstart) werden nicht überschrieben

            t_SCR = ((1 - Sharetdown) * myEXS.TempMod(id - 1).t_tc(jz)) + (Sharetdown * myEXS.TempMod(id).t_tc(jz))
            t_SCR = Math.Max(50.001, t_SCR)
            t_SCR = Math.Min(499.999, t_SCR)
            '
            '     b.) t_up, NOxraw, SV: 20s gleitender Mittelwert in die Vergangenheit
            '         Formel gilt auch für die ersten 20 Sekunden
            '
            ct_up = 0
            cNOxraw = 0
            cSV = 0
            For is0 = Math.Max(0, jz - 9) To jz
                If Not MODdata.EngState(is0) = tEngState.Stopped Then
                    ct_up = ct_up + myEXS.TempMod(id - 1).t_tc(is0)
                    cNOxraw = cNOxraw + (myEXS.TempMod(id - 1).eEmKomp(2, is0) / myEXS.p_rated)
                    cSV = cSV + ((((myEXS.mpexh(is0) * 3600) / myEXS.p_rated) / 1.29) / (specCatVol * 0.001))
                End If
            Next is0
            ct_up = ct_up / (jz - Math.Max(1, jz - 9) + 1)
            cNOxraw = cNOxraw / (jz - Math.Max(1, jz - 9) + 1)
            cSV = cSV / (jz - Math.Max(1, jz - 9) + 1)
            '
            '     c.) NOx60s: Summe über letzten 60s der spezifischen NOx-Rohemissionen
            '         Formel gilt auch für die ersten 60 Sekunden

            cNOx30s = 0
            For is0 = Math.Max(0, jz - 29) To jz
                If Not MODdata.EngState(is0) = tEngState.Stopped Then
                    cNOx30s = cNOx30s + (eNOx(is0) / myEXS.p_rated)
                End If
            Next is0
            '        Für Sekunde 1 bis 59 muss summenwert hochgerechnet werden
            cNOx30s = cNOx30s / (Math.Min(30, jz + 1) / 30.0)

            '     -----------------------------------------------------------------
            '
            '
            '
            '     Berechnung deNOxmin aus Kennlinien-Wert bei 50°C
            For is0 = 1 To iSCRAnz
                myEXS.Xis(is0) = pt_SCR(is0)
                myEXS.Yis(is0) = pdeNOx(is0)
            Next is0
            such = 50.001
            izpl = iSCRAnz
            Call myEXS.Intlin(such, izpl)
            deNOxmin = such
            '
            '
            '     -----------------------------------------------------------------
            '     2.) Berechnung deNOx
            '
            '        a.) deNOx aus Kennlinie:
            For is0 = 1 To iSCRAnz
                myEXS.Xis(is0) = pt_SCR(is0)
                myEXS.Yis(is0) = pdeNOx(is0)
            Next is0
            such = t_SCR
            izpl = iSCRAnz
            Call myEXS.Intlin(such, izpl)
            denox_cc = such

            'c        b.) Falls Korrekturkriterien erfüllt sind: deNOx-Korrektur gegenüber Kennlinie
            If ((t_SCR > tminCor) And (t_SCR < tmaxCor)) Then
                'c
                'c           t_up aus Kennlinie:
                For is0 = 1 To iSCRAnz
                    myEXS.Xis(is0) = pt_SCR(is0)
                    myEXS.Yis(is0) = pt_up(is0)
                Next is0
                such = t_SCR
                izpl = iSCRAnz
                Call myEXS.Intlin(such, izpl)
                t_up_cc = such
                'c
                'c           NOx_raw aus Kennlinie:
                For is0 = 1 To iSCRAnz
                    myEXS.Xis(is0) = pt_SCR(is0)
                    myEXS.Yis(is0) = pNOxraw(is0)
                Next is0
                such = t_SCR
                izpl = iSCRAnz
                Call myEXS.Intlin(such, izpl)
                NOxraw_cc = such
                'c
                'c           Summe NOxraw in den letzten 60 Sekunden aus Kennlinie:
                For is0 = 1 To iSCRAnz
                    myEXS.Xis(is0) = pt_SCR(is0)
                    myEXS.Yis(is0) = pNOx60s(is0)
                Next is0
                such = t_SCR
                izpl = iSCRAnz
                Call myEXS.Intlin(such, izpl)
                NOx30s_cc = such
                'c
                'c           Raumgeschwindigkeit aus Kennlinie:
                For is0 = 1 To iSCRAnz
                    myEXS.Xis(is0) = pt_SCR(is0)
                    myEXS.Yis(is0) = pSV(is0)
                Next is0
                such = t_SCR
                izpl = iSCRAnz
                Call myEXS.Intlin(such, izpl)
                SV_cc = such
                'c
                deNOx_cor = denox_cc + Cor_tup * (ct_up - t_up_cc) + Cor_NOxraw * (cNOxraw - NOxraw_cc) + Cor_NOx30s * (cNOx30s - NOx30s_cc) + Cor_SV * (cSV - SV_cc)
                'c
                deNOx = Math.Max(Math.Min(deNOx_cor, DeNOxMax), deNOxmin)
                'c
            Else
                deNOx = Math.Max(Math.Min(denox_cc, DeNOxMax), deNOxmin)
            End If

            'Schreiben der Ergebnisse auf die standardisierten Variablen eEmKomp (iSchad, jz) und Qp_reak(jz)
            For iSchad = 2 To 8
                KonvRate(iSchad) = 0
            Next iSchad
            KonvRate(2) = deNOx
            KonvRate(7) = deNOx
            For iSchad = 2 To 8
                myEXS.TempMod(id).eEmKomp(iSchad, jz) = myEXS.TempMod(id - 1).eEmKomp(iSchad, jz) * (1 - KonvRate(iSchad))
            Next iSchad
            myEXS.TempMod(id).Qp_reak(jz) = 0
        End Sub

        Public Sub Header() Implements KonvInterf.Header
            DatKonvOut.OpenWrite(PathKonvOut)
            DatKonvOut.WriteLine("result-file VECTO SCR Model 10s HERO")
            DatKonvOut.WriteLine("VECTO " & VECTOvers)
            DatKonvOut.WriteLine("time,NOx_up,NOx_down,t_SCR,deNOx,deNOx_cc,t_tc_down,t_tc_up,10s t_tc_up,t_up_cc,NOx_raw,NOx_raw_cc,NOx30s,NOx30s_cc,SV,SV_cc ")
            DatKonvOut.WriteLine("[s],[g/h],[g/h],[°C],[1],[1],[°C],[°C],[°C],[°C],[g/(h*kW_rated)],[g/(h*kW_rated)],[g/(h*kW_rated)],[g/(h*kW_rated)],[h-1],[h-1]")
        End Sub

        Public Sub Write(ByVal jz As Integer) Implements KonvInterf.Write
            DatKonvOut.WriteLine(jz & "," & myEXS.TempMod(id - 1).eEmKomp(2, jz) & "," & myEXS.TempMod(id).eEmKomp(2, jz) & "," & t_SCR & "," & deNOx & "," & denox_cc & "," & myEXS.TempMod(id).t_tc(jz) & "," & myEXS.TempMod(id - 1).t_tc(jz) & "," & ct_up & "," & t_up_cc & "," & cNOxraw & "," & NOxraw_cc & "," & cNOx30s & "," & NOx30s_cc & "," & cSV & "," & SV_cc)
        End Sub

        Public Sub Close() Implements KonvInterf.Close
            DatKonvOut.Close()
        End Sub

    End Class


    ''' <summary>
    ''' KAT-Modell
    ''' </summary>
    ''' <remarks></remarks>
    Class c3WayCatMod 'DOC Modell

        Implements KonvInterf

        'Klasse initialisiert als Unterelement von TempMod
        Dim iSchad As Long
        Dim id As Int16

        Private myEXS As cEXS

        'Kennfelddaten
        'Private Massflow As List(Of Single)
        'Private Temp_KAT As List(Of Single)
        'Private Massflow_Norm As List(Of Single)
        'Private Temp_KAT_Norm As List(Of Single)
        'Private Massflow_Min As Single
        'Private Massflow_Max As Single
        'Private Temp_KAT_Min As Single
        'Private Temp_KAT_Max As Single
        'Private ListDim As Integer

        Private KonvRateMap As Dictionary(Of tMapComp, cDelaunayMap)
        'Public KonvRate As Dictionary(Of tMapComp, List(Of Single))

        Dim DatKonvOut As New cFile_V3    'PHEM 3waycat result file
        Dim PathKonvOut As String = ""

        Public Property KonvRate As Dictionary(Of tMapComp, List(Of Single)) Implements KonvInterf.KonvRate


        ''' <summary>
        ''' Erstellen eines neuen KAT-Moduls
        ''' </summary>
        ''' <param name="i">ID</param>
        ''' <param name="EXSref">EXS-Klasse</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal i As Int16, ByRef EXSref As cEXS)
            id = i
            myEXS = EXSref

            PathKonvOut = MODdata.ModOutpName & "_Mod" & id & ".kat"

        End Sub


        ''' <summary>
        ''' Interpolationsfunktion
        ''' </summary>
        ''' <param name="x">Massenstrom</param>
        ''' <param name="y">Temperatur vor KAT</param>
        ''' <param name="MapID">MapID der entsprechenden Abgaskomponente</param>
        ''' <returns>interpolierten Wert für x und y aus Kennfeld</returns>
        ''' <remarks>Aus Massenstrom-Temperatur Kennfeld wird Konvertierungsrate für entsprechende Abgaskomponente berechnet</remarks>
        Private Function Intpol(ByVal x As Double, ByVal y As Double, ByVal MapID As tMapComp) As Double
            Try
                Return KonvRateMap(MapID).Intpol(x, y)
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "Extrapolation nicht möglich! Massenstrom=" & x & ", Temp_Kat=" & y, "3WayCatMod.Intpol")
                Return -1
            End Try
        End Function


        ''' <summary>
        ''' Einlesen der Kennfelder für Konvertierungsraten
        ''' </summary>
        ''' <param name="Name">Dateiname</param>
        ''' <remarks></remarks>
        Public Function Read(ByVal Name As String) As Boolean Implements KonvInterf.Read
            Dim DatKonv As New cFile_V3
            Dim line As String()
            Dim s As Integer
            Dim s1 As Integer
            Dim MapComp As tMapComp
            Dim Spalten As Dictionary(Of tMapComp, Integer)
            Dim Sp0 As KeyValuePair(Of tMapComp, Integer)
            Dim KonvRate0 As KeyValuePair(Of tMapComp, cDelaunayMap)
            Dim Massflow As Double
            Dim Temp_KAT As Double
            Dim MsgSrc As String

            MsgSrc = "EmCalc/EXS/c3WayCatMod/Read"

            If Not DatKonv.OpenRead(Name) Then
                WorkerMsg(tMsgID.Err, "Cannot read " & Name & "!", MsgSrc)
                DatKonv = Nothing
                Return False
            End If

            Spalten = New Dictionary(Of tMapComp, Integer)

            KonvRateMap = New Dictionary(Of tMapComp, cDelaunayMap)

            KonvRateMap.Add(tMapComp.NOx, New cDelaunayMap)
            KonvRateMap.Add(tMapComp.HC, New cDelaunayMap)
            KonvRateMap.Add(tMapComp.CO, New cDelaunayMap)
            KonvRateMap.Add(tMapComp.NO, New cDelaunayMap)

            'Header
            line = DatKonv.ReadLine
            line = DatKonv.ReadLine
            s1 = UBound(line)
            For s = 2 To s1

                MapComp = fMapComp(line(s))

                If Not KonvRateMap.ContainsKey(MapComp) Then
                    WorkerMsg(tMsgID.Err, "Komponente nicht vorgesehen!", MsgSrc)
                    Return False
                End If

                If Spalten.ContainsKey(MapComp) Then
                    WorkerMsg(tMsgID.Err, "Komponente kommt doppelt vor!", MsgSrc)
                    Return False
                End If

                Spalten.Add(MapComp, s)

            Next

            'Units (wird nicht ausgewertet)
            line = DatKonv.ReadLine

            'Werte
            Do While Not DatKonv.EndOfFile

                line = DatKonv.ReadLine

                Massflow = CDbl(line(0))
                Temp_KAT = CDbl(line(1))

                For Each Sp0 In Spalten
                    KonvRateMap(Sp0.Key).AddPoints(Massflow, Temp_KAT, CDbl(line(Sp0.Value)))
                Next

                'KonvRaten Null setzen wenn Komponente nicht gegeben
                For Each KonvRate0 In KonvRateMap
                    If Not Spalten.ContainsKey(KonvRate0.Key) Then
                        KonvRate0.Value.AddPoints(Massflow, Temp_KAT, 0)
                    End If
                Next

            Loop

            DatKonv.Close()

            'Triangulieren
            For Each KonvRate0 In KonvRateMap
                If Not KonvRate0.Value.Triangulate() Then
                    WorkerMsg(tMsgID.Err, "Triangulation-ERROR", MsgSrc)
                    Return False
                End If
            Next

            'Dic. für modale Konvrate definieren
            KonvRate = New Dictionary(Of tMapComp, List(Of Single))
            KonvRate.Add(tMapComp.NOx, New List(Of Single))
            KonvRate.Add(tMapComp.HC, New List(Of Single))
            KonvRate.Add(tMapComp.CO, New List(Of Single))
            KonvRate.Add(tMapComp.NO, New List(Of Single))

            Return True

        End Function


        ''' <summary>
        ''' Berechnung der Konvertierungsrate aus Kennfeld
        ''' </summary>
        ''' <param name="jz">Zeit</param>
        ''' <remarks>Für die Berechnung wird die Temperatur des Thermoelements am Kateingang (entspricht Modulnummer i-1) verwendet!!!</remarks>
        Public Sub Konv(ByVal jz As Integer) Implements KonvInterf.Konv
            'Konvertierungsrate aus Kennfeld berechnen
            Dim massflow As Single
            Dim temp As Single

            massflow = MODdata.Em.EmDefComp(tMapComp.MassFlow).FinalVals(jz)

            temp = myEXS.TempMod(id - 1).t_tc(jz)

            KonvRate(tMapComp.NOx).Add(Intpol(massflow, temp, tMapComp.NOx))
            KonvRate(tMapComp.HC).Add(Intpol(massflow, temp, tMapComp.HC))
            KonvRate(tMapComp.CO).Add(Intpol(massflow, temp, tMapComp.CO))
            KonvRate(tMapComp.NO).Add(Intpol(massflow, temp, tMapComp.NO))

        End Sub

        ''' <summary>
        ''' Header für Ausgabedatei
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Header() Implements KonvInterf.Header
            DatKonvOut.OpenWrite(PathKonvOut)
            DatKonvOut.WriteLine("result-file VECTO 3waycat Model")
            DatKonvOut.WriteLine("VECTO " & VECTOvers)
            DatKonvOut.WriteLine("time,MassFlow,NOx_up,NOx_down,HC_up,HCdown,CO_up,CO_down,NO_up,NO_down,deNOx,deHC,deCO,deNO,t_gas,t_m,t_tc")
            DatKonvOut.WriteLine("[s],[kg/s]N,[g/h],[g/h],[g/h],[g/h],[g/h],[g/h],[g/h],[g/h],[1],[1],[1],[1],[°C],[°C],[°C]")
        End Sub

        ''' <summary>
        ''' Daten für Ausgabedatei
        ''' </summary>
        ''' <param name="jz">Zeit</param>
        ''' <remarks></remarks>
        Public Sub Write(ByVal jz As Integer) Implements KonvInterf.Write
            DatKonvOut.WriteLine(jz & "," & MODdata.Em.EmDefComp(tMapComp.MassFlow).FinalVals(jz) & "," _
                                & myEXS.TempMod(id - 1).eEmKomp(2, jz) & "," & myEXS.TempMod(id).eEmKomp(2, jz) & "," _
                                & myEXS.TempMod(id - 1).eEmKomp(3, jz) & "," & myEXS.TempMod(id).eEmKomp(3, jz) & "," _
                                & myEXS.TempMod(id - 1).eEmKomp(4, jz) & "," & myEXS.TempMod(id).eEmKomp(4, jz) & "," _
                                & myEXS.TempMod(id - 1).eEmKomp(7, jz) & "," & myEXS.TempMod(id).eEmKomp(7, jz) & "," _
                                & myEXS.TempMod(id).KonvMod.KonvRate(tMapComp.NOx)(jz) & "," & myEXS.TempMod(id).KonvMod.KonvRate(tMapComp.HC)(jz) & "," _
                                & myEXS.TempMod(id).KonvMod.KonvRate(tMapComp.CO)(jz) & "," & myEXS.TempMod(id).KonvMod.KonvRate(tMapComp.NO)(jz) & "," _
                                & myEXS.TempMod(id).t_gas(jz) & "," & myEXS.TempMod(id).t_m(jz) & "," & myEXS.TempMod(id).t_tc(jz))
        End Sub

        Public Sub Close() Implements KonvInterf.Close
            DatKonvOut.Close()
        End Sub


    End Class

    ''' <summary>
    ''' Interface zur Konverter-Klasse cScrMod, cDocMod , usw...
    ''' </summary>
    ''' <remarks></remarks>
    Interface KonvInterf
        'Sub NewI(ByVal i As Int16)
        Function Read(ByVal s As String) As Boolean
        Sub Konv(ByVal t As Integer)
        Sub Header()
        Sub Write(ByVal t As Integer)
        Sub Close()
        Property KonvRate As Dictionary(Of tMapComp, List(Of Single))
    End Interface


    Sub Intlin(ByRef such As Single, ByVal izpl As Long)
        ''C
        'C     Unterprogramm zu PHEM zur linearen INterpolation aus einem Polygonzug (z.B. in Vissimzs.for aufgerufen)
        'C     uebergeben wid "such" als X-Wert, der dann als berechneter Y-Wert wieder zurueck gegeben wird
        'C     Zu Belegen sind vorher:
        'C     Xis(j) und Yis(j)
        'c     Zu uebergeben der gesuchte Wert (such) und die Anzahl an vorhandenen Polyginpunkten (izpl)
        'c
        'C
        ' INCLUDE "com.inc"<<<<<<<<<<<<<<<<<<<<<<<<<<
        Dim i1min As Long, i2min As Long, igel As Int32
        Dim abstad(1000) As Single
        Dim aminabst As Single
        Dim ji As Int32
        Dim x As Single
        'C
        'c
        'C    Suche der naechstgelegenen Drehzahlpunkte aus eingegebener Vollastkurve:
        'c     Abstand zu Eingabepunkten und Suche des Punktes mit geringstem Abstand:
        aminabst = Math.Abs(such - Xis(1)) + 1
        For ji = 1 To izpl
            x = such - Xis(ji)
            abstad(ji) = Math.Abs(x)
            If (abstad(ji) < aminabst) Then
                aminabst = abstad(ji)
                i1min = ji
            End If
        Next ji
        'c
        'C      Festlegung des zweiten INterpolationspunktes (nur interpolieren, nicht extrapolieren)
        'C
        x = such - Xis(i1min)
        If (x >= 0) Then
            i2min = i1min + 1
            If (i2min > izpl) Then
                '!Extrapolation nach oben
                i2min = izpl - 1
            End If
        Else
            i2min = i1min - 1
            If (i2min < 1) Then
                '!Extrapolation nach unten
                i2min = 2
            End If
        End If
        'c
        'c      Sortieren der 2 Werte nach aufsteigendem n:
        'c
        If (Xis(i2min) < Xis(i1min)) Then
            igel = i2min
            i2min = i1min
            i1min = igel
        End If
        'c
        'c     Interpolation der zugehoerigen Maximalleistung (P/Pnenn)
        'c
        If ((Xis(i2min) - Xis(i1min)) = 0) Then
            Xis(i2min) = Xis(i2min) + 0.000001
        End If
        such = Yis(i1min) + (such - Xis(i1min)) * (Yis(i2min) - Yis(i1min)) / (Xis(i2min) - Xis(i1min))
        'C
        Exit Sub ' RETURN<<<<<<<<<<<<<<<<<<<<<<<<<<
    End Sub


End Class

