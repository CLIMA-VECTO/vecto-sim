Imports System.Collections.Generic

Module M_Lese

    Public Function LESE() As Boolean
        Dim AuxEntry As cVEH.cAuxEntry
        Dim AuxKV As KeyValuePair(Of String, cVEH.cAuxEntry)
        Dim i As Integer
        Dim j As Integer
        Dim sb As cSubPath
        Dim fldgear As Dictionary(Of Integer, String)
        Dim fldgFromTo As String()
        Dim str As String

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
            WorkerMsg(tMsgID.Err, "File read error! (" & GenFile & ")", MsgSrc, GenFile)
            Return False
        End Try

        If GEN.NoJSON Then WorkerMsg(tMsgID.Warn, "VECTO file format is outdated! CLICK HERE to convert to current format!", MsgSrc, "<GUI>" & GenFile)


        'VECTO: Defaultwerte für Parameter die nicht mehr in der .GEN/.VECTO sind werden beim Einlesen über SetDefault belegt. |@@| VECTO: Default values for the parameters are no longer in GEN/.VECTO but are allocated when Read about SetDefault.


        CycleFiles.Clear()
        For Each sb In GEN.CycleFiles
            CycleFiles.Add(sb.FullPath)
        Next

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
                WorkerMsg(tMsgID.Err, "File read error! (" & GEN.PathVEH & ")", MsgSrc, GEN.PathVEH)
                Return False
            End Try

        End If

        If VEH.NoJSON Then WorkerMsg(tMsgID.Warn, "Vehicle file format is outdated! CLICK HERE to convert to current format!", MsgSrc, "<GUI>" & GEN.PathVEH)


        '-----------------------------    ~ENG~    -----------------------------
        ENG = New cENG
        ENG.FilePath = GEN.PathENG
        Try
            If Not ENG.ReadFile Then Return False
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "File read error! (" & GEN.PathENG & ")", MsgSrc, GEN.PathENG)
            Return False
        End Try

        If ENG.NoJSON Then WorkerMsg(tMsgID.Warn, "Engine file format is outdated! CLICK HERE to convert to current format!", MsgSrc, "<GUI>" & GEN.PathENG)


        '-----------------------------    ~GBX~    -----------------------------
        GBX = New cGBX

        If Not GEN.EngOnly Then
            GBX.FilePath = GEN.PathGBX
            Try
                If Not GBX.ReadFile Then Return False
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & GEN.PathGBX & ")", MsgSrc, GEN.PathGBX)
                Return False
            End Try
        End If

        If GBX.NoJSON Then WorkerMsg(tMsgID.Warn, "Gearbox file format is outdated! CLICK HERE to convert to current format!", MsgSrc, "<GUI>" & GEN.PathGBX)


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

        'ENG => VEH
        VEH.nLeerl = ENG.nleerl
        VEH.I_mot = ENG.I_mot

        'GBX => VEH
        If GEN.EngOnly Then
            VEH.ganganz = 0
        Else
            VEH.ganganz = GBX.GetrI.Count - 1
        End If

        For i = 0 To GBX.GetrI.Count - 1
            VEH.siGetrI.Add(GBX.GetrI(i))
            VEH.GetrMap.Add(New cSubPath)
            If IsNumeric(GBX.GetrMap(i, True)) Then
                VEH.GetrEffDef.Add(True)
                VEH.GetrEff.Add(CSng(GBX.GetrMap(i, True)))
            Else
                VEH.GetrEffDef.Add(False)
                VEH.GetrEff.Add(0)
                VEH.GetrMap(i).Init(fPATH(GEN.PathGBX), GBX.GetrMap(i, True))
            End If
        Next
        VEH.I_Getriebe = GBX.I_Getriebe
        VEH.TracIntrSi = GBX.TracIntrSi

        '-----------------------------    ~FLD~    -----------------------------
        '   FLD muss jetzt vor MAP/MEP eingelesen werden falls dort <DRAG> Einträge sind! |@@| if there are <DRAG> entries, then read FLD before MAP/MEP!

        FLD = New List(Of cFLD)

        If ENG.FLDgears.Count = 0 Then
            WorkerMsg(tMsgID.Err, "No .vfld file defined in Engine file!", MsgSrc, "<GUI>" & GEN.PathENG)
            Return False
        End If

        fldgear = New Dictionary(Of Integer, String)

        Try
            j = -1
            For Each str In ENG.FLDgears

                j += 1
                If str.Contains("-") Then
                    fldgFromTo = str.Replace(" ", "").Split("-")
                Else
                    fldgFromTo = New String() {str, str}
                End If

                For i = CInt(fldgFromTo(0)) To CInt(fldgFromTo(1))

                    If i > VEH.ganganz Then Exit For

                    If i < 0 Or i > 99 Then
                        WorkerMsg(tMsgID.Err, "Cannot assign .vfld file to gear " & i & "!", MsgSrc, "<GUI>" & GEN.PathENG)
                        Return False
                    End If

                    If fldgear.ContainsKey(i) Then
                        WorkerMsg(tMsgID.Err, "Multiple .vfld files are assigned to gear " & i & "!", MsgSrc, "<GUI>" & GEN.PathENG)
                        Return False
                    End If

                    fldgear.Add(i, ENG.PathFLD(j))

                Next

            Next
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to process engine file '" & GEN.PathENG & "'!", MsgSrc)
            Return False
        End Try

       
        For i = 0 To VEH.ganganz

            If Not fldgear.ContainsKey(i) Then
                WorkerMsg(tMsgID.Err, "No .vfld file assigned to gear " & i & "!", MsgSrc, "<GUI>" & GEN.PathENG)
                Return False
            End If

            FLD.Add(New cFLD)
            FLD(i).FilePath = fldgear(i)

            Try
                If Not FLD(i).ReadFile Then Return False 'Fehlermeldung hier nicht notwendig weil schon von in ReadFile
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & fldgear(i) & ")", MsgSrc, fldgear(i))
                Return False
            End Try

        Next

        VEH.nRatedInit()

     

        '-----------------------------    ~MAP~    -----------------------------
        '    Map: Columns 1 and 2 are the x-and y-coordinates (Pe, n)
        '    the rest are Measurement-values
        '    Emissions and Consumption in (g/(h*kW_NominalPower) at HDV(SNF)
        '    Emissions (g/h) and consumption in (g/(h*kW_NominalPower) in cars(PKW) and LCV(LNF)
     
        'Kennfeld read
        MAP = New cMAP
        MAP.FilePath = ENG.PathMAP

        Try
            If Not MAP.ReadFile Then Return False 'Fehlermeldung hier nicht notwendig weil schon von in ReadFile
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "File read error! (" & ENG.PathMAP & ")", MsgSrc, ENG.PathMAP)
            Return False
        End Try

        'Normalize
        MAP.Norm()


        Return True

    End Function

End Module
