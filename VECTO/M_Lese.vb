Imports System.Collections.Generic

Module M_Lese

    Public Function LESE() As Boolean
        Dim AuxEntry As cVEH.cAuxEntry
        Dim AuxKV As KeyValuePair(Of String, cVEH.cAuxEntry)
        Dim i As Integer
        Dim sb As cSubPath

        Dim MsgSrc As String

        MsgSrc = "Main/ReadInp"

        '-----------------------------    ~GEN~    -----------------------------
        'GEN einlesen
        If UCase(fEXT(GenFile)) <> ".VECTO" Then
            WorkerMsg(tMsgID.Err, "Only .VECTO files are supported in this mode", MsgSrc)
            Return False
        End If

        GEN = New cGEN
        GEN.FilePath = GenFile

        Try
            If Not GEN.ReadFile() Then
                WorkerMsg(tMsgID.Err, "Cannot read .gen file (" & GenFile & ")", MsgSrc)
                Return False
            End If
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "File read error! (" & GenFile & ")", MsgSrc)
            Return False
        End Try

        'VECTO: Defaultwerte für Parameter die nicht mehr in der .GEN/.VECTO sind werden beim Einlesen über SetDefault belegt.


        CycleFiles.Clear()
        For Each sb In GEN.CycleFiles
            CycleFiles.Add(sb.FullPath)
        Next

        SOCnJa = GEN.SOCnJa And GEN.VehMode = tVehMode.HEV
        SOCstart = GEN.SOCstart

        'Fehlermeldung in Init()
        If Not GEN.Init Then Return False


        '-----------------------------    ~VEH~    -----------------------------
        'Einlesen der KFZ-Spezifikationen aus 'KFZspez'
        VEH = New cVEH
        VEH.FilePath = GEN.PathVEH
        Try
            If Not VEH.ReadFile Then Return False
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "File read error! (" & GEN.PathVEH & ")", MsgSrc)
            Return False
        End Try

        VEH.hinauf = (VEH.nLeerl / VEH.nNenn) + VEH.hinauf * (1 - (VEH.nLeerl / VEH.nNenn))
        VEH.hinunter = (VEH.nLeerl / VEH.nNenn) + VEH.hinunter * (1 - (VEH.nLeerl / VEH.nNenn))
        VEH.lhinauf = (VEH.nLeerl / VEH.nNenn) + VEH.lhinauf * (1 - (VEH.nLeerl / VEH.nNenn))
        VEH.lhinunter = (VEH.nLeerl / VEH.nNenn) + VEH.lhinunter * (1 - (VEH.nLeerl / VEH.nNenn))


        '-----------------------------    ~ENG~    -----------------------------
        ENG = New cENG
        ENG.FilePath = GEN.PathENG
        Try
            If Not ENG.ReadFile Then Return False
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "File read error! (" & GEN.PathENG & ")", MsgSrc)
            Return False
        End Try


        '-----------------------------    ~GBX~    -----------------------------
        GBX = New cGBX
        GBX.FilePath = GEN.PathGBX
        Try
            If Not GBX.ReadFile Then Return False
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "File read error! (" & GEN.PathGBX & ")", MsgSrc)
            Return False
        End Try


        '-----------------------------    VECTO    -----------------------------
        'GEN => VEH
        VEH.AuxDef = GEN.AuxDef
        If VEH.AuxDef Then
            For Each AuxKV In GEN.AuxPaths
                AuxEntry = New cVEH.cAuxEntry
                AuxEntry.Type = AuxKV.Value.Type
                AuxEntry.Path.Init(fPATH(GEN.PathVEH), AuxKV.Value.Path.OriginalPath)
                VEH.AuxPaths.Add(UCase(Trim(AuxKV.Key)), AuxEntry)
            Next
        End If
        VEH.hinauf = GEN.hinauf
        VEH.hinunter = GEN.hinunter
        VEH.lhinauf = GEN.lhinauf
        VEH.lhinunter = GEN.lhinunter
        VEH.pspar = GEN.pspar
        VEH.pmodell = GEN.pmodell

        'ENG => VEH
        VEH.Pnenn = ENG.Pnenn
        VEH.nNenn = ENG.nnenn
        VEH.nLeerl = ENG.nleerl
        VEH.I_mot = ENG.I_mot

        'GBX => VEH
        VEH.ganganz = 0
        For i = 0 To 16
            VEH.Igetr(i) = GBX.GetrI(i)
            If IsNumeric(GBX.GetrMap(i, True)) Then
                VEH.GetrEffDef(i) = True
                VEH.GetrEff(i) = CSng(GBX.GetrMap(i, True))
            Else
                VEH.GetrMap(i) = GBX.GetrMap(i, True)
            End If
            If VEH.Igetr(i) > 0.0001 Then VEH.ganganz = i
        Next
        VEH.I_Getriebe = GBX.I_Getriebe
        VEH.TracIntrSi = GBX.TracIntrSi



        '-----------------------------    ~FLD~    -----------------------------
        '   FLD muss jetzt vor MAP/MEP eingelesen werden falls dort <DRAG> Einträge sind!
        FLD = New cFLD
        FLD.FilePath = ENG.PathFLD

        Try
            If Not FLD.ReadFile Then Return False 'Fehlermeldung hier nicht notwendig weil schon von in ReadFile
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "File read error! (" & ENG.PathFLD & ")", MsgSrc)
            Return False
        End Try

        'Normieren
        FLD.Norm()

        '-----------------------------    ~MAP~    -----------------------------
        '    Kennfeld: Spalten 1 und 2 sind die x- und y- Koordinaten (Pe,n), die
        '    uebrigen sind Messwerte
        '    Emissionen und Verbrauch in (g/(h*(kW_Nennleistung) bei SNF
        '    Emissionen in (g/h) und Verbrauch in (g/(h*(kW_Nennleistung) bei PKW und LNF
        If Not GEN.CreateMap Then

            'Kennfeld einlesen
            MAP = New cMAP(GEN.PKWja)
            MAP.FilePath = ENG.PathMAP

            Try
                If Not MAP.ReadFile Then Return False 'Fehlermeldung hier nicht notwendig weil schon von in ReadFile
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & ENG.PathMAP & ")", MsgSrc)
                Return False
            End Try

            'Normieren
            MAP.Norm()

        End If


        '-----------------------------    ~DRI~    -----------------------------
        '    Einlesen des KFZ-Fahrzyklus (Nicht in ADVANCE).
        '       LUZ: 04.02.2011: Ab jetzt ausserhalb von LESE wegen neuer BATCH-Struktur

        '-----------------------------    ~TRS~    -----------------------------
        '    Dynamik-Korrekturparameter, falls dynamokkorrektur ausgewählt:
        '    Parameter aus multipler Regressionsanalyse, Differenz zu stationär in
        '      SNF: (g/h)/kW_Nennleistung fuer einzelne Parameter
        '      PKW  (g/h) für Emissionen , (g/h)/kW fuer Verbrauch
        If GEN.dynkorja Then
            TRS = New cTRS
            TRS.FilePath = GEN.dynspez

            Try
                If Not TRS.ReadFile Then Return False
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & GEN.dynspez & ")", MsgSrc)
                Return False
            End Try

        End If

        '-----------------------------------------------------------------------
        'Einlesen der Daten fuer Hybridsimulation:
        If (GEN.ModeHorEV) Then

            'TODO: EV/HEV Init hierher!

        End If
        '-----------------------------------------------------------------------


        Return True

    End Function

End Module
