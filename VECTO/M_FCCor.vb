Module M_FCCor

    'C
    'C
    'C     Subroutine to correct the Consumption-values from Engine-size (HBEFA-calculations because
    'C     all HDV(LKW)-sizes are calculated from the same *. mep
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
        'c  Correction of the Fuel-consumption
        'c  only for manual calculations and HDV(LKW)
        If (GEN.eklasse = 0) Then
            'c        for Euro0 and earlier, 3 different KF used depending on the size
            'c        therefore no Size-correction here
            corr = 1
        ElseIf (GEN.eklasse = 1) Then
            'c         Correction-function for EUR1 and EUR 2 adopted 1:1 from ARTEMIS standard
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
            'c        Korrekturfunktion Euro 3 gg. ARTEMIS geringfügig adaptiert (siehe FcCorr_Eu3ff.xls) |@@| Correction-function for Euro 3 like  ARTEMIS slightly adapted (see FcCorr_Eu3ff.xls)
            fc_pnen = 239.4 - (0.08465 * (Math.Min(Pnenn, 345)))
            fc_ave = 239.4 - (0.08465 * 273.5)
            corr = (fc_pnen / fc_ave)
        ElseIf ((GEN.eklasse = 4) Or (GEN.eklasse = 5)) Then
            'c         Correction function for Euro 4 similar to Euro3
            'c         but lightly adapted: Average Nominal-Power of the engines into mep verwursteten
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
