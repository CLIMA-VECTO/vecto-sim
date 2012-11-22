Imports System.Collections.Generic

Public Class cFLD

    'Private Const FormatVersion As Integer = 1
    'Private FileVersion As Integer

    Private sFilePath As String

    Private LPfull As List(Of Single)
    Private LPdrag As List(Of Single)
    Private Lnn As List(Of Single)
    Private LPT1 As List(Of Single)

    Private LPtarget As List(Of Single)
    Private PtargetDef As Boolean = False
    Private PtargetNormed As Boolean

    Private PfullNormed As Boolean
    Private PdragNormed As Boolean
    Private nNormed As Boolean
    Private iDim As Integer

    Private bEmDef As Boolean
    Private EmDragD As Dictionary(Of String, List(Of Single))
    Private EmDragNormed As Dictionary(Of String, Boolean)

    Private Sub ResetMe()
        LPfull = Nothing
        LPdrag = Nothing
        Lnn = Nothing
        LPT1 = Nothing
        iDim = -1
        PfullNormed = False
        PdragNormed = False
        PtargetDef = False
        PtargetNormed = False
        bEmDef = False
        EmDragD = Nothing
        EmDragNormed = Nothing
    End Sub

    Public Function ReadFile() As Boolean
        Dim file As cFile_V3
        Dim line As String()
        'Dim txt As String
        Dim s1 As Integer
        'Dim s As Integer
        'Dim ID As tFldComp
        Dim sTarget As Integer
        Dim sPT1 As Integer
        Dim PT1 As Single
        Dim sEmDrag As Dictionary(Of String, Integer)
        Dim EmKV As KeyValuePair(Of String, Integer)
        Dim nU As Double
        Dim MsgSrc As String

        MsgSrc = "Main/ReadInp/FLD"

        sTarget = -1
        sPT1 = -1
        PT1 = 0       '=> Defaultwert falls nicht in FLD vorgegeben

        'Reset
        ResetMe()

        'Abbruch wenn's Datei nicht gibt
        If sFilePath = "" OrElse Not IO.File.Exists(sFilePath) Then
            WorkerMsg(tMsgID.Err, "FLD file '" & sFilePath & "' not found!", MsgSrc)
            Return False
        End If

        'Datei öffnen
        file = New cFile_V3
        If Not file.OpenRead(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Failed to open file (" & sFilePath & ") !", MsgSrc)
            file = Nothing
            Return False
        End If

        'Listen initialisieren
        LPfull = New System.Collections.Generic.List(Of Single)
        LPdrag = New System.Collections.Generic.List(Of Single)
        Lnn = New System.Collections.Generic.List(Of Single)
        LPT1 = New System.Collections.Generic.List(Of Single)
        EmDragD = New Dictionary(Of String, List(Of Single))
        EmDragNormed = New Dictionary(Of String, Boolean)
        sEmDrag = New Dictionary(Of String, Integer)

        ''***
        ''*** Erste Zeile: Version
        'line = file.ReadLine
        'txt = Trim(UCase(line(0)))
        'If Microsoft.VisualBasic.Left(txt, 1) = "V" Then
        '    ' "V" entfernen => Zahl bleibt übrig
        '    txt = txt.Replace("V", "")
        '    If Not IsNumeric(txt) Then
        '        'Falls Version ungültig: Abbruch
        '        WorkerMsg(tMsgID.Err, "File Version invalid!", MsgSrc)
        '        GoTo lbEr
        '    Else
        '        'Version festgelegt
        '        FileVersion = CInt(txt)
        '    End If

        'Else
        '    'Falls keine Versionsangabe: Altes Format
        '    file.Close()
        '    Return ReadOldFormat()
        'End If

        ''Version-Check: Abbruch falls Inputdateiformat neuer ist als PHEM-Version
        'If FileVersion > FormatVersion Then
        '    WorkerMsg(tMsgID.Err, "File Version not supported!", MsgSrc)
        '    GoTo lbEr
        'End If



        '***
        '*** Zweite Zeile: Namen/Identifizierung der Komponenten (Schlepp-Emissionen f. KF-Erstellung)
        'line = file.ReadLine

        ''Spaltenanzahl checken
        's1 = UBound(line)

        ''Abbruch falls weniger als 3 Spalten
        'If s1 < 3 Then
        '    WorkerMsg(tMsgID.Err, "Format invalid!", MsgSrc)
        '    GoTo lbEr
        'End If

        'If s1 > 3 Then
        '    For s = 4 To s1

        '        txt = Trim(UCase(line(s)))
        '        ID = fFldComp(txt)

        '        Select Case ID

        '            Case tFldComp.PeTarget

        '                sTarget = s
        '                PtargetDef = True

        '                'Case tFldComp.PT1

        '                '    sPT1 = s

        '            Case Else 'tFldComp.Undefined

        '                bEmDef = True

        '                If EmDragD.ContainsKey(txt) Then
        '                    WorkerMsg(tMsgID.Err, "Em-Component '" & txt & "' already defined!", MsgSrc)
        '                    GoTo lbEr
        '                Else
        '                    EmDragD.Add(txt, New List(Of Single))
        '                    EmDragNormed.Add(txt, False)
        '                    sEmDrag.Add(txt, s)
        '                End If

        '        End Select

        '    Next
        'End If

        'VECTO: Keine Header/Unit-Spalte. Immer PT1!
        s1 = 3
        sPT1 = 3
        bEmDef = False

        '***
        '*** Dritte Zeile: Normierung/Einheit
        'line = file.ReadLine

        ''Abbruch falls weniger Spalten als in zweiter Zeile
        'If UBound(line) < s1 Then
        '    WorkerMsg(tMsgID.Err, "Format invalid!", MsgSrc)
        '    GoTo lbEr
        'End If

        'nNormed = (Trim(UCase(line(0))) = sKey.Normed)
        nNormed = False

        'If Not nNormed Then
        '    Select Case Trim(UCase(line(0)))
        '        Case "[U/MIN]", "RPM", "[1/MIN]", "[MIN^-1]"
        '            'Alles okay
        '        Case Else
        '            WorkerMsg(tMsgID.Err, "Engine Speed Unit '" & line(0) & "' unknown! '[U/min]' expected.", MsgSrc)
        '    End Select
        'Else
        '    WorkerMsg(tMsgID.Err, "Engine Speed Unit '" & line(0) & "' unknown! '[U/min]' expected.", MsgSrc)
        'End If

        'PfullNormed = (Trim(UCase(line(1))) = sKey.Normed)
        PfullNormed = False

        'If Not PfullNormed Then
        '    If Trim(UCase(line(1))) <> "[NM]" Then
        '        WorkerMsg(tMsgID.Err, "Engine Torque Unit '" & line(1) & "' unknown! '[Nm]' expected.", MsgSrc)
        '    End If
        'Else
        '    WorkerMsg(tMsgID.Err, "Engine Torque Unit '" & line(1) & "' unknown! '[Nm]' expected.", MsgSrc)
        'End If

        'PdragNormed = (Trim(UCase(line(2))) = sKey.Normed)
        PdragNormed = False

        'If Not PdragNormed Then
        '    If Trim(UCase(line(1))) <> "[NM]" Then
        '        WorkerMsg(tMsgID.Err, "Engine Torque Unit '" & line(2) & "' unknown! '[Nm]' expected.", MsgSrc)
        '    End If
        'Else
        '    WorkerMsg(tMsgID.Err, "Engine Torque Unit '" & line(2) & "' unknown! '[Nm]' expected.", MsgSrc)
        'End If

        'If PtargetDef Then
        '    LPtarget = New System.Collections.Generic.List(Of Single)
        '    PtargetNormed = (Trim(UCase(line(sTarget))) = sKey.Normed)

        '    If Not PtargetNormed Then
        '        If Trim(UCase(line(1))) <> "[KW]" Then
        '            WorkerMsg(tMsgID.Err, "Engine Power Unit '" & line(sTarget) & "' unknown! '[kW]' or '" & sKey.Normed & "' expected.", MsgSrc)
        '        End If
        '    End If

        'End If

        ''Zusatzkomponenten
        'If bEmDef Then
        '    For Each EmKV In sEmDrag

        '        txt = line(EmKV.Value)

        '        'Klammern entfernen
        '        txt = txt.Replace("[", "")
        '        txt = txt.Replace("]", "")

        '        'Normierung und Unit festlegen
        '        If txt.Contains("/") Then
        '            Select Case UCase(Right(txt, txt.Length - txt.IndexOf("/") - 1))
        '                Case "H"
        '                    EmDragNormed(EmKV.Key) = False
        '                Case "KWH"
        '                    WorkerMsg(tMsgID.Err, "Unit '" & line(EmKV.Value) & "' is not supported in this file!", MsgSrc)
        '                    GoTo lbEr
        '                Case "H" & sKey.Normed
        '                    EmDragNormed(EmKV.Key) = True
        '            End Select
        '        Else
        '            EmDragNormed(EmKV.Key) = False
        '        End If

        '    Next
        'End If


        'Ab Zeile 4: Werte
        Try

            Do While Not file.EndOfFile

                'Zeile einlesen
                line = file.ReadLine

                'VECTO: M => Pe
                nU = CDbl(line(0))

                Lnn.Add(nU)
                LPfull.Add(nMtoPe(nU, CDbl(line(1))))
                LPdrag.Add(nMtoPe(nU, CDbl(line(2))))
                If PtargetDef Then LPtarget.Add(CSng(line(sTarget)))

                'Falls PT1 nicht vorgegeben wird Defaultwert verwendet (siehe oben)
                If sPT1 > -1 Then

                    PT1 = CSng(line(sPT1))

                    If PT1 < 0 Then
                        WorkerMsg(tMsgID.Err, "PT1 value invalid! line " & iDim + 1, MsgSrc)
                        PT1 = 0
                    End If

                End If

                LPT1.Add(PT1)

                If bEmDef Then
                    For Each EmKV In sEmDrag
                        EmDragD(EmKV.Key).Add(line(sEmDrag(EmKV.Key)))
                    Next
                End If

                'Zeilen-Zähler hoch (wurde in ResetMe zurück gesetzt)
                iDim += 1

            Loop

        Catch ex As Exception

            WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & iDim + 1 & " (" & sFilePath & ")", MsgSrc)
            GoTo lbEr

        End Try


        'Datei schließen
        file.Close()

        Return True


        'ERROR-Label für sauberen Abbruch
lbEr:
        file.Close()
        file = Nothing

        Return False

    End Function

    Private Function ReadOldFormat() As Boolean
        Dim File As cFile_V3
        Dim line As String()

        'Datei öffnen
        File = New cFile_V3
        If Not File.OpenRead(sFilePath, ",", True, True) Then
            File = Nothing
            Return False
        End If

        nNormed = True
        PfullNormed = True
        PdragNormed = True

        Do While Not File.EndOfFile
            line = File.ReadLine

            Lnn.Add(CSng(line(0)))
            LPfull.Add(CSng(line(1)))
            LPdrag.Add(CSng(line(2)))

            LPT1.Add(0)

            'Zeilen-Zähler hoch (wurde in ResetMe zurück gesetzt)
            iDim += 1

        Loop

        'Datei schließen
        File.Close()

        Return True

    End Function

    Public Sub Norm()
        Dim i As Integer
        Dim Pnenn As Single
        Dim nleerl As Single
        Dim nnenn As Single
        Dim EmKV As KeyValuePair(Of String, Boolean)

        Pnenn = VEH.Pnenn
        nleerl = VEH.nLeerl
        nnenn = VEH.nNenn

        'Drehzahl normieren
        If Not nNormed Then
            For i = 0 To iDim
                Lnn(i) = (Lnn(i) - nleerl) / (nnenn - nleerl)
            Next
        End If

        'Leistung normieren
        If Not PfullNormed Then
            For i = 0 To iDim
                LPfull(i) /= Pnenn
            Next
        End If

        'Leistung normieren
        If Not PdragNormed Then
            For i = 0 To iDim
                LPdrag(i) /= Pnenn
            Next
        End If

        'Pe-Target normieren
        If PtargetDef AndAlso Not PtargetNormed Then
            For i = 0 To iDim
                LPtarget(i) /= Pnenn
            Next
        End If

        'Em ent-normieren
        If bEmDef Then
            For Each EmKV In EmDragNormed
                If EmKV.Value Then
                    For i = 0 To iDim
                        EmDragD(EmKV.Key)(i) *= Pnenn
                    Next
                End If
            Next
        End If

    End Sub

    Public Function Pdrag(ByVal nnorm As Single) As Single
        Dim i As Int32

        'Extrapolation für x < x(1)
        If Lnn(0) >= nnorm Then
            If Lnn(0) > nnorm Then MODdata.ModErrors.FLDextrapol = "n= " & nnormTonU(nnorm) & " [U/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While Lnn(i) < nnorm And i < iDim
            i += 1
        Loop

        'Extrapolation für x > x(imax)
        If Lnn(i) < nnorm Then
            MODdata.ModErrors.FLDextrapol = "n= " & nnormTonU(nnorm) & " [U/min]"
        End If

lbInt:
        'Interpolation
        Return VEH.Pnenn * ((nnorm - Lnn(i - 1)) * (LPdrag(i) - LPdrag(i - 1)) / (Lnn(i) - Lnn(i - 1)) + LPdrag(i - 1))

    End Function


    Public Function Pfull(ByVal nnorm As Single, ByVal LastPenorm As Single) As Single
        Dim i As Int32
        Dim PfullStat As Single
        Dim PT1 As Single

        'Extrapolation für x < x(1)
        If Lnn(0) >= nnorm Then
            If Lnn(0) > nnorm Then MODdata.ModErrors.FLDextrapol = "n= " & nnormTonU(nnorm) & " [U/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While Lnn(i) < nnorm And i < iDim
            i += 1
        Loop

        'Extrapolation für x > x(imax)
        If Lnn(i) < nnorm Then
            MODdata.ModErrors.FLDextrapol = "n= " & nnormTonU(nnorm) & " [U/min]"
        End If

lbInt:
        'Interpolation
        PfullStat = (nnorm - Lnn(i - 1)) * (LPfull(i) - LPfull(i - 1)) / (Lnn(i) - Lnn(i - 1)) + LPfull(i - 1)
        PT1 = (nnorm - Lnn(i - 1)) * (LPT1(i) - LPT1(i - 1)) / (Lnn(i) - Lnn(i - 1)) + LPT1(i - 1)

        'Dynamische Volllast
        Return Math.Min(VEH.Pnenn * (1 / (PT1 + 1)) * (PfullStat + PT1 * LastPenorm), PfullStat * VEH.Pnenn)

    End Function


    Public Function Pfull(ByVal nnorm As Single) As Single
        Dim i As Int32

        'Extrapolation für x < x(1)
        If Lnn(0) >= nnorm Then
            If Lnn(0) > nnorm Then MODdata.ModErrors.FLDextrapol = "n= " & nnormTonU(nnorm) & " [U/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While Lnn(i) < nnorm And i < iDim
            i += 1
        Loop

        'Extrapolation für x > x(imax)
        If Lnn(i) < nnorm Then
            MODdata.ModErrors.FLDextrapol = "n= " & nnormTonU(nnorm) & " [U/min]"
        End If

lbInt:
        'Interpolation
        Return VEH.Pnenn * ((nnorm - Lnn(i - 1)) * (LPfull(i) - LPfull(i - 1)) / (Lnn(i) - Lnn(i - 1)) + LPfull(i - 1))
    End Function

    Public Function Ptarget(ByVal nnorm As Single) As Single
        Dim i As Int32

        'Extrapolation für x < x(1)
        If Lnn(0) >= nnorm Then
            If Lnn(0) > nnorm Then MODdata.ModErrors.FLDextrapol = "n= " & nnormTonU(nnorm) & " [U/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While Lnn(i) < nnorm And i < iDim
            i += 1
        Loop

        'Extrapolation für x > x(imax)
        If Lnn(i) < nnorm Then
            MODdata.ModErrors.FLDextrapol = "n= " & nnormTonU(nnorm) & " [U/min]"
        End If

lbInt:
        'Interpolation
        Return VEH.Pnenn * ((nnorm - Lnn(i - 1)) * (LPtarget(i) - LPtarget(i - 1)) / (Lnn(i) - Lnn(i - 1)) + LPtarget(i - 1))
    End Function

    Public Function EmDrag(ByVal EmComp As String, ByVal nnorm As Single) As Single
        Dim i As Int32
        Dim EmL As List(Of Single)

        If Not EmDragD.ContainsKey(EmComp) Then Return -1

        EmL = EmDragD(EmComp)

        'Extrapolation für x < x(1)
        If Lnn(0) >= nnorm Then
            If Lnn(0) > nnorm Then MODdata.ModErrors.FLDextrapol = "n= " & nnormTonU(nnorm) & " [U/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While Lnn(i) < nnorm And i < iDim
            i += 1
        Loop

        'Extrapolation für x > x(imax)
        If Lnn(i) < nnorm Then
            MODdata.ModErrors.FLDextrapol = "n= " & nnormTonU(nnorm) & " [U/min]"
        End If

lbInt:
        'Interpolation
        Return ((nnorm - Lnn(i - 1)) * (EmL(i) - EmL(i - 1)) / (Lnn(i) - Lnn(i - 1)) + EmL(i - 1))
    End Function


    Public Property FilePath() As String
        Get
            Return sFilePath
        End Get
        Set(ByVal value As String)
            sFilePath = value
        End Set
    End Property

    Public ReadOnly Property EmDef(ByVal EmComp As String) As Boolean
        Get
            If Not bEmDef Then
                Return False
            Else
                Return EmDragD.ContainsKey(EmComp)
            End If
        End Get
    End Property

End Class
