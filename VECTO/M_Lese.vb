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
        'Read GEN
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

        'VECTO: Defaultwerte für Parameter die nicht mehr in der .GEN/.VECTO sind werden beim Einlesen über SetDefault belegt. |@@| VECTO: Default values for the parameters are no longer in GEN/.VECTO but are allocated when Read about SetDefault.


        CycleFiles.Clear()
        For Each sb In GEN.CycleFiles
            CycleFiles.Add(sb.FullPath)
        Next

        SOCnJa = GEN.SOCnJa And GEN.VehMode = tVehMode.HEV
        SOCstart = GEN.SOCstart

        'Error message in init()
        If Not GEN.Init Then Return False


        '-----------------------------    ~VEH~    -----------------------------
        VEH = New cVEH

        'Read vehicle specifications
        If Not GEN.EngOnly Then
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
        End If

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

        If Not GEN.EngOnly Then
            GBX.FilePath = GEN.PathGBX
            Try
                If Not GBX.ReadFile Then Return False
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & GEN.PathGBX & ")", MsgSrc)
                Return False
            End Try
        End If

        '-----------------------------    VECTO    -----------------------------
        'GEN => VEH
        VEH.AuxDef = GEN.AuxDef
        If VEH.AuxDef Then
            For Each AuxKV In GEN.AuxPaths
                AuxEntry = New cVEH.cAuxEntry
                AuxEntry.Type = AuxKV.Value.Type
                AuxEntry.Path.Init(fPATH(GEN.FilePath), AuxKV.Value.Path.OriginalPath)
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
                VEH.GetrMap(i).Init(fPATH(GEN.PathGBX), GBX.GetrMap(i, True))
            End If
            If VEH.Igetr(i) > 0.0001 Then VEH.ganganz = i
        Next
        VEH.I_Getriebe = GBX.I_Getriebe
        VEH.TracIntrSi = GBX.TracIntrSi

        '-----------------------------    ~FLD~    -----------------------------
        '   FLD muss jetzt vor MAP/MEP eingelesen werden falls dort <DRAG> Einträge sind! |@@| if there are <DRAG> entries, then read FLD before MAP/MEP!
        FLD = New cFLD
        FLD.FilePath = ENG.PathFLD

        Try
            If Not FLD.ReadFile Then Return False 'Fehlermeldung hier nicht notwendig weil schon von in ReadFile
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "File read error! (" & ENG.PathFLD & ")", MsgSrc)
            Return False
        End Try

        'Normalize
        FLD.Norm()

        '-----------------------------    ~MAP~    -----------------------------
        '    Map: Columns 1 and 2 are the x-and y-coordinates (Pe, n)
        '    the rest are Measurement-values
        '    Emissions and Consumption in (g/(h*kW_NominalPower) at HDV(SNF)
        '    Emissions (g/h) and consumption in (g/(h*kW_NominalPower) in cars(PKW) and LCV(LNF)
        If Not GEN.CreateMap Then

            'Kennfeld read
            MAP = New cMAP(GEN.PKWja)
            MAP.FilePath = ENG.PathMAP

            Try
                If Not MAP.ReadFile Then Return False 'Fehlermeldung hier nicht notwendig weil schon von in ReadFile
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & ENG.PathMAP & ")", MsgSrc)
                Return False
            End Try

            'Normalize
            MAP.Norm()

        End If


        '-----------------------------    ~DRI~    -----------------------------
        '    Reading the Vehicle Driving-cycle (Not in ADVANCE).
        '       LUZ: 04.02.2011: From now outside of READING because of new BATCH structure

        '-----------------------------    ~TRS~    -----------------------------
        '    Dynamik-Korrekturparameter, falls dynamokkorrektur ausgewählt: |@@| Dynamic correction parameter, if exclusively Dynamic-correction(dynamokkorrektur):
        '    Parameter aus multipler Regressionsanalyse, Differenz zu stationär in |@@| Parameters of multiple regression analysis, Difference with stationary
        '      HDV(SNF): (g/h) / kW_Nominal-power for individual parameters
        '      Cars(PKW) (g/h) for emissions (g/h)/kW for consumption
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
        'Reading data for hybrid simulation:
        If (GEN.ModeHorEV) Then

            'TODO: Init EV/HEV here!

        End If
        '-----------------------------------------------------------------------


        Return True

    End Function

End Module
