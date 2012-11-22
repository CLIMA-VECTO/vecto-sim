Module M_FCCor

    'C
    'C
    'C     Unterprogramm zur Korrektur der verbrauchswerte nach Motorgroesse (fuer HBEFA-Berechnungen, da dort
    'C     alle LKW-Groessen mit gleichem *.mep gerechnet werden
    'C
    'C
    Sub FcCorr()
        'C
        ' include "com.inc"<<<<<<<<<<<<<<<<<<<<<<<<<<
        'c
        Dim corr As Single, fc_pnen As Single, fc_ave As Single
        Dim t As Int32
        Dim t1 As Integer
        Dim Pnenn As Single
        Dim L As System.Collections.Generic.List(Of Single)
        Dim MsgSrc As String

        MsgSrc = "EmCalc/FcCorr"

        If Not MAP.EmDefRef.ContainsKey(tMapComp.FC) Then
            WorkerMsg(tMsgID.Err, "FC component not found! Aborting FC Correction!", MsgSrc)
            Exit Sub
        End If

        Pnenn = VEH.Pnenn
        t1 = MODdata.tDim
        L = MODdata.Em.EmDefComp(tMapComp.FC).FinalVals

        'C
        'c  Korrektur des Kraftstoffverbrauches
        'c  nur für Handbuchrechnungen und LKW
        If (GEN.eklasse = 0) Then
            'c        für Euro0 und früher werden größenabhängig 3 verschiedene KF verwendet
            'c        daher hier keine Größenkorrektur
            corr = 1
        ElseIf (GEN.eklasse = 1) Then
            'c         Korrekturfunktion für Euro1 und Euro 2 1:1 von Stand ARTEMIS übernommen
            fc_pnen = 230.55 - 0.0798 * Pnenn
            fc_pnen = Math.Max(fc_pnen, 206.2)
            fc_ave = 213.9
            corr = (fc_pnen / fc_ave)
        ElseIf (GEN.eklasse = 2) Then
            fc_pnen = 222.56 - 0.0575 * Pnenn
            fc_pnen = Math.Max(fc_pnen, 202.15)
            fc_ave = 206.8
            corr = (fc_pnen / fc_ave)
        ElseIf (GEN.eklasse = 3) Then
            'c        Korrekturfunktion Euro 3 gg. ARTEMIS geringfügig adaptiert (siehe FcCorr_Eu3ff.xls)
            fc_pnen = 239.4 - (0.08465 * (Math.Min(Pnenn, 345)))
            fc_ave = 239.4 - (0.08465 * 273.5)
            corr = (fc_pnen / fc_ave)
        ElseIf ((GEN.eklasse = 4) Or (GEN.eklasse = 5)) Then
            'c         Korrekturfunktion für Euro 4 ff analog zu Euro3
            'c         lediglich adaptiert: Durchschnittsnennleistung der ins mep verwursteten Motoren
            fc_pnen = 239.4 - (0.08465 * (Math.Min(Pnenn, 345)))
            fc_ave = 239.4 - (0.08465 * 287.1)
            corr = (fc_pnen / fc_ave)
        Else
            'c         Euro6ff
            fc_pnen = 239.4 - (0.08465 * (Math.Min(Pnenn, 345)))
            fc_ave = 239.4 - (0.08465 * 279.1)
            corr = (fc_pnen / fc_ave)
        End If
        'c
        For t = 0 To t1
            L(t) = L(t) * corr
        Next
        'C
        Exit Sub ' return<<<<<<<<<<<<<<<<<<<<<<<<<<
    End Sub


End Module
