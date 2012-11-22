Imports System.Collections.Generic

Public Class cMAP

    'Private Const FormatVersion As Integer = 1
    'Private FileVersion As Integer

    Private bPKWja As Boolean

    Public TransMap As Boolean

    Public EmComponents As Dictionary(Of String, cEmComp)
    Public EmDefRef As Dictionary(Of tMapComp, cEmComp)
    Private MyEmList As List(Of String)

    Public LPe As List(Of Single)
    Public Lnn As List(Of Single)

    Private sFilePath As String
    Private iMapDim As Integer
    Private Ln As System.Collections.Generic.List(Of Single)
    Private PeNormed As Boolean
    Private nNormed As Boolean

    Private MapIntp As cMapInterpol

    Private FuelMap As cDelaunayMap

    Public Sub New(ByVal PKWja As Boolean)
        bPKWja = PKWja
    End Sub

    Private Sub ResetMe()
        'FileVersion = 0
        TransMap = False
        MapIntp = Nothing
        EmComponents = Nothing
        MyEmList = Nothing
        LPe = Nothing
        Lnn = Nothing
        Ln = Nothing
        EmDefRef = Nothing
        iMapDim = -1
        PeNormed = False
        nNormed = False
        FuelMap = New cDelaunayMap
    End Sub

    Private Function fDefIntpV2(ByVal DefEm As tMapComp) As Boolean

        Select Case DefEm
            Case tMapComp.ExhTemp, tMapComp.MassFlow, tMapComp.Lambda
                Return True
            Case Else
                Return False
        End Select

    End Function

    Private Function fDefPeCorMode(ByVal DefEm As tMapComp) As tIntpPeCorMode

        Select Case DefEm

            Case tMapComp.FC
                Return tIntpPeCorMode.PeCorNull

            Case tMapComp.NOx, tMapComp.CO, tMapComp.HC, tMapComp.PM, tMapComp.PN, tMapComp.NO

                If bPKWja Then
                    Return tIntpPeCorMode.PeCorOff              'Früher: tMapIntpol.ShepNoCor
                Else
                    Return tIntpPeCorMode.PeCorNullPmin         'Früher: tMapIntpol.ShepPwCorMin
                End If

            Case tMapComp.ExhTemp, tMapComp.Lambda, tMapComp.MassFlow
                Return tIntpPeCorMode.PeCorEmDrag

            Case Else   'DynKor-Parameter, Eta 
                Return tIntpPeCorMode.PeCorOff

        End Select

    End Function

    Private Function fDefPeCorModeOld(ByVal DefEm As tMapComp) As tIntpPeCorMode

        Select Case DefEm

            Case tMapComp.FC
                Return tIntpPeCorMode.PeCorNull

            Case tMapComp.NOx, tMapComp.CO, tMapComp.HC, tMapComp.PM, tMapComp.PN, tMapComp.NO

                If bPKWja Then
                    Return tIntpPeCorMode.PeCorOff              'Früher: tMapIntpol.ShepNoCor
                Else
                    Return tIntpPeCorMode.PeCorNullPmin         'Früher: tMapIntpol.ShepPwCorMin
                End If

            Case Else   'MassFlow, ExhTemp und DynKor-Parameter
                Return tIntpPeCorMode.PeCorOff

        End Select

    End Function

    Private Function fDefEmNormID(ByVal DefEm As tMapComp) As tEmNorm
        Select Case DefEm
            Case tMapComp.FC
                Return tEmNorm.x_hPnenn
            Case tMapComp.NOx, tMapComp.CO, tMapComp.HC, tMapComp.PM, tMapComp.PN, tMapComp.NO
                If bPKWja Then
                    Return tEmNorm.x_h
                Else
                    Return tEmNorm.x_hPnenn
                End If
            Case Else
                Return tEmNorm.x
        End Select
    End Function

    Public Function ReadFile(Optional ByVal MsgOutput As Boolean = True) As Boolean
        Dim file As cFile_V3
        Dim line As String()
        Dim s1 As Integer
        Dim s As Integer
        'Dim txt As String
        Dim Em0 As cEmComp
        Dim EmKV As System.Collections.Generic.KeyValuePair(Of String, cEmComp)
        Dim SwitchOn As Boolean
        Dim nU As Double
        Dim MsgSrc As String


        MsgSrc = "Main/ReadInp/MAP"

        'Reset
        ResetMe()

        'Abbruch wenn's Datei nicht gibt
        If sFilePath = "" OrElse Not IO.File.Exists(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Map file not found! (" & sFilePath & ")", MsgSrc)
            Return False
        End If

        'Datei öffnen
        file = New cFile_V3
        If Not file.OpenRead(sFilePath) Then
            file = Nothing
            WorkerMsg(tMsgID.Err, "Failed to open file (" & sFilePath & ") !", MsgSrc)
            Return False
        End If

        'Listen initialisieren (vor Version-Check damit ReadOldFormat funktioniert)
        MyEmList = New List(Of String)
        EmComponents = New System.Collections.Generic.Dictionary(Of String, cEmComp)
        EmDefRef = New System.Collections.Generic.Dictionary(Of tMapComp, cEmComp)
        LPe = New System.Collections.Generic.List(Of Single)
        Lnn = New System.Collections.Generic.List(Of Single)
        Ln = New System.Collections.Generic.List(Of Single)
        SwitchOn = False

        'Check ob MEP oder MAP
        '...wird jetzt weiter unten gecheckt beim Einlesen.

        ''***
        ''*** Erste Zeile: Version
        'line = file.ReadLine
        'txt = Trim(UCase(line(0)))
        'If Microsoft.VisualBasic.Left(txt, 1) = "V" Then
        '    ' "V" entfernen => Zahl bleibt übrig
        '    txt = txt.Replace("V", "")
        '    If Not IsNumeric(txt) Then
        '        WorkerMsg(tMsgID.Err, "File Version invalid!", MsgSrc)
        '        GoTo lbEr
        '    Else
        '        'Version festgelegt
        '        FileVersion = CInt(txt)
        '    End If
        'Else
        '    file.Close()
        '    Return ReadOldFormat()
        'End If

        ''Version-Check: Abbruch falls Inputdateiformat neuer ist als PHEM-Version
        'If FileVersion > FormatVersion Then
        '    WorkerMsg(tMsgID.Err, "File Version not supported!", MsgSrc)
        '    GoTo lbEr
        'End If

        ''Spalte 2: Option "+" = Parameter für KF-Erstellung gegeben
        'If UBound(line) > 0 Then
        '    If Trim(line(1)) = "+" Then
        '        SwitchOn = True
        '        If MsgOutput Then WorkerMsg(tMsgID.Normal, "Advanced settings found.", MsgSrc)
        '    End If
        'End If

        '***
        '*** Zweite Zeile: Namen/Identifizierung der Komponenten (Nur Em. Leistung, Drehzahl ist fix!)
        'line = file.ReadLine

        ''Spaltenanzahl checken
        's1 = UBound(line)
        s1 = 2

        ''Abbruch falls weniger als 3 Spalten
        'If s1 < 2 Then GoTo lbEr

        ' ''Check ob Leistung/Drehzahl vertauscht
        ''If UCase(line(0)).Contains("PE") Then
        ''    If MsgOutput Then WorkerMsg(tMsgID.Warn, "Expected Emission Map format: 1st column = Engine Speed, 2nd column = Engine Power (Header Check failed)", MsgSrc)
        ''End If

        ''Em-Komponenten initialisieren
        'For s = 3 To s1

        '    Em0 = New cEmComp
        '    Em0.Col = s
        '    Em0.Name = line(s)  'wird bei Default-Komponenten noch geändert
        '    Em0.IDstring = Trim(UCase(line(s)))
        '    Em0.MapCompID = fMapComp(Em0.Name)
        '    Em0.NormID = tEmNorm.x  'wird ggf. weiter unten korrigiert!
        '    'Default-Interpolator definiert in Em0 = New cEmComp
        '    'Default Pe-Correction definiert in Em0 = New cEmComp

        '    If EmComponents.ContainsKey(UCase(Em0.Name)) Then

        '        'Abbruch falls schon definiert
        '        WorkerMsg(tMsgID.Err, "Component '" & Em0.Name & "' already defined! Col. " & s + 1, MsgSrc)
        '        GoTo lbEr

        '    Else

        '        'Dictionary füllen....
        '        If Em0.MapCompID = tMapComp.Undefined Then

        '            'ERROR wenn Komponente in spitzen Klammern aber nicht bekannt
        '            If Em0.IDstring.Length > 1 Then
        '                If Left(Em0.IDstring, 1) = "<" And Right(Em0.IDstring, 1) = ">" Then
        '                    If MsgOutput Then WorkerMsg(tMsgID.Err, "'" & Em0.Name & "' is no valid Default Map Component!", MsgSrc)
        '                End If
        '            End If

        '            'Custom Em-Komponenten Dictionary:
        '            EmComponents.Add(Em0.IDstring, Em0)
        '            MyEmList.Add(Em0.IDstring)

        '        Else

        '            '*** Default Em-Komponenten ***

        '            'Default-Interpolator
        '            Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)

        '            'Default Pe-Correction
        '            Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)

        '            'Default-Name
        '            Em0.Name = fMapCompName(Em0.MapCompID)

        '            'TC-Komponenten werden nicht ausgegeben
        '            If fMapCompIsTC(Em0.MapCompID) Then
        '                TransMap = True
        '                Em0.WriteOutput = False
        '            End If

        '            'Custom Em-Komponenten Dictionary:
        '            EmComponents.Add(Em0.IDstring, Em0)
        '            MyEmList.Add(Em0.IDstring)

        '            'Eintrag in Referenz-Dictionary
        '            EmDefRef.Add(Em0.MapCompID, Em0)

        '        End If
        '    End If
        'Next

        'VECTO: Spalte 3 immer Verbrauch
        s = 2
        Em0 = New cEmComp
        Em0.Col = s
        Em0.Name = "FC"  'wird bei Default-Komponenten noch geändert
        Em0.Unit = "[g/h]"
        Em0.IDstring = "<FC>"
        Em0.MapCompID = tMapComp.FC
        Em0.NormID = tEmNorm.x_h
        If EmComponents.ContainsKey(UCase(Em0.Name)) Then
            'Abbruch falls schon definiert
            WorkerMsg(tMsgID.Err, "Component '" & Em0.Name & "' already defined! Col. " & s + 1, MsgSrc)
            GoTo lbEr
        Else
            Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)
            Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
            Em0.Name = fMapCompName(Em0.MapCompID)
            EmComponents.Add(Em0.IDstring, Em0)
            MyEmList.Add(Em0.IDstring)
            EmDefRef.Add(Em0.MapCompID, Em0)
        End If

        '***
        '*** Dritte Zeile: Normierung/Einheit
        'line = file.ReadLine

        ''Abbruch falls weniger Spalten als in zweiter Zeile
        'If UBound(line) < s1 Then GoTo lbEr

        ''Normierung/Einheit einlesen
        'For Each EmKV In EmComponents

        '    'EM-Komp Referenz
        '    Em0 = EmKV.Value

        '    'Unit in String für weitere Checks speichern
        '    txt = Trim(line(Em0.Col))

        '    'Klammern entfernen
        '    txt = txt.Replace("[", "")
        '    txt = txt.Replace("]", "")

        '    'Normierung und Unit festlegen
        '    If txt.Contains("/") Then
        '        Select Case UCase(Right(txt, txt.Length - txt.IndexOf("/") - 1))

        '            Case "H"
        '                Em0.NormID = tEmNorm.x_h
        '                Em0.Unit = "[" & Left(txt, txt.IndexOf("/")) & "/h]"

        '            Case "KWH"
        '                Em0.NormID = tEmNorm.x_kWh
        '                Em0.Unit = "[" & Left(txt, txt.IndexOf("/")) & "/h]"

        '            Case "H" & sKey.Normed
        '                Em0.NormID = tEmNorm.x_hPnenn
        '                Em0.Unit = "[" & Left(txt, txt.IndexOf("/")) & "/h]"

        '            Case Else
        '                Em0.NormID = tEmNorm.x
        '                Em0.Unit = "[" & txt & "]"

        '        End Select
        '    Else
        '        Em0.NormID = tEmNorm.x
        '        Em0.Unit = "[" & txt & "]"
        '    End If

        'Next
        'nNormed = Trim(UCase(line(0))) = sKey.Normed
        'PeNormed = Trim(UCase(line(1))) = sKey.Normed
        nNormed = False
        PeNormed = False

        ''Check ob n/Pe Einheiten OK:
        'If Not nNormed Then
        '    Select Case Trim(UCase(line(0)))
        '        Case "[U/MIN]", "RPM", "[1/MIN]", "[MIN^-1]"
        '            'Alles okay
        '        Case Else
        '            If MsgOutput Then WorkerMsg(tMsgID.Err, "Engine Speed Unit '" & line(0) & "' unknown! '[U/min]' expected.", MsgSrc)
        '    End Select
        'Else
        '    If MsgOutput Then WorkerMsg(tMsgID.Err, "Engine Speed Unit '" & line(0) & "' unknown! '[U/min]' expected.", MsgSrc)
        'End If

        'If Not PeNormed Then
        '    If Trim(UCase(line(1))) <> "[NM]" Then
        '        If MsgOutput Then WorkerMsg(tMsgID.Err, "Engine Torque Unit '" & line(1) & "' unknown! '[Nm]' expected.", MsgSrc)
        '    End If
        'Else
        '    If MsgOutput Then WorkerMsg(tMsgID.Err, "Engine Torque Unit '" & line(1) & "' unknown! '[Nm]' expected.", MsgSrc)
        'End If


        '***
        '*** Zeile 4,5: (optional, wenn oben "+"): Einstellungen für Pe-Cor (altes Pfak)
        '   Falls nicht "+" werden Default Interpolatoren verwendet (s.o.)
        If SwitchOn Then

            'Zeile 4 einlesen
            line = file.ReadLine

            'Schleife über Em-Komponenten
            For Each EmKV In EmComponents

                If UBound(line) < EmKV.Value.Col Then
                    WorkerMsg(tMsgID.Err, "Power Correction settings line invalid! (UBound(line) < Em.Col)", MsgSrc)
                    GoTo lbEr
                ElseIf Not IsNumeric(line(EmKV.Value.Col)) Then
                    WorkerMsg(tMsgID.Err, "Power Correction settings for '" & EmKV.Value.Name & "' (Col. " & EmKV.Value.Col + 1 & ") invalid!", MsgSrc)
                    GoTo lbEr
                End If

                Select Case line(EmKV.Value.Col)
                    Case 0
                        EmKV.Value.PeCorMode = tIntpPeCorMode.PeCorOff
                    Case 1
                        EmKV.Value.PeCorMode = tIntpPeCorMode.PeCorNull
                    Case 2
                        EmKV.Value.PeCorMode = tIntpPeCorMode.PeCorEmDrag
                    Case Else
                        WorkerMsg(tMsgID.Err, "Power Correction Mode Nr. " & line(EmKV.Value.Col) & " is invalid!", MsgSrc)
                        GoTo lbEr
                End Select

            Next


            'Zeile 5 einlesen
            line = file.ReadLine

            'Schleife über Em-Komponenten
            For Each EmKV In EmComponents

                If UBound(line) < EmKV.Value.Col Then
                    WorkerMsg(tMsgID.Err, "Interpolator settings line invalid! (UBound(line) < Em.Col)", MsgSrc)
                    GoTo lbEr
                ElseIf Not IsNumeric(line(EmKV.Value.Col)) Then
                    WorkerMsg(tMsgID.Err, "Interpolator settings for '" & EmKV.Value.Name & "' (Col. " & EmKV.Value.Col + 1 & ") invalid!", MsgSrc)
                    GoTo lbEr
                End If

                Select Case line(EmKV.Value.Col)
                    Case 1
                        EmKV.Value.IntpolV2 = False
                    Case 2
                        EmKV.Value.IntpolV2 = True
                    Case Else
                        WorkerMsg(tMsgID.Err, "Interpolator Mode Nr. " & line(EmKV.Value.Col) & " is invalid!", MsgSrc)
                        GoTo lbEr
                End Select

            Next

        End If

        'Ab Zeile 4 (bzw. 6): Werte
        Try
            Do While Not file.EndOfFile

                'Zeile einlesen
                line = file.ReadLine

                'Zeilen-Zähler hoch (wurde in ResetMe zurück gesetzt)
                iMapDim += 1

                'Drehzahl
                nU = CDbl(line(0))

                Lnn.Add(nU)

                'Leistung
                'If Trim(UCase(line(1))) = sKey.MAP.Drag Then
                '    If PeNormed Then
                '        LPe.Add(FLD.Pdrag(Lnn(iMapDim)) / VEH.Pnenn)
                '    Else
                '        LPe.Add(FLD.Pdrag(Lnn(iMapDim)))
                '    End If
                'Else
                LPe.Add(nMtoPe(nU, CDbl(line(1))))
                'End If

                'Emissionen
                For Each EmKV In EmComponents
                    EmKV.Value.RawVals.Add(CSng(line(EmKV.Value.Col)))
                Next
            Loop
        Catch ex As Exception

            WorkerMsg(tMsgID.Err, "Error during file input! Line number " & iMapDim + 1 & " (" & sFilePath & ")", MsgSrc)
            GoTo lbEr

        End Try

        'Shep-Init
        MapIntp = New cMapInterpol(Me)

        'Datei schließen
        file.Close()

        file = Nothing
        EmKV = Nothing
        Em0 = Nothing

        Return True


        'ERROR-Label für sauberen Abbruch
lbEr:
        file.Close()
        file = Nothing
        EmKV = Nothing
        Em0 = Nothing

        Return False

    End Function

    Private Function ReadOldFormat() As Boolean
        Dim File As cFile_V3
        Dim line As String()
        Dim Em0 As cEmComp
        Dim EmKV As System.Collections.Generic.KeyValuePair(Of String, cEmComp)
        Dim s1 As Integer
        Dim s As Integer

        'Datei öffnen
        File = New cFile_V3
        If Not File.OpenRead(sFilePath, ",", True, True) Then
            File = Nothing
            Return False
        End If

        'Alte Kennfelder haben immer TC-Faktoren (sind halt evtl. Null)
        TransMap = True

        Em0 = New cEmComp
        Em0.Col = 2
        Em0.NormID = tEmNorm.x_hPnenn
        Em0.MapCompID = tMapComp.FC
        Em0.Unit = "[g/h]"
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.FC
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.Col = 3
        Em0.MapCompID = tMapComp.NOx
        Em0.NormID = fDefEmNormID(Em0.MapCompID)
        Em0.Unit = "[g/h]"
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.NOx
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.Col = 4
        Em0.MapCompID = tMapComp.CO
        Em0.NormID = fDefEmNormID(Em0.MapCompID)
        Em0.Unit = "[g/h]"
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.CO
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.Col = 5
        Em0.MapCompID = tMapComp.HC
        Em0.NormID = fDefEmNormID(Em0.MapCompID)
        Em0.Unit = "[g/h]"
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.HC
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.Col = 6
        Em0.MapCompID = tMapComp.PM
        Em0.NormID = fDefEmNormID(Em0.MapCompID)
        Em0.Unit = "[g/h]"
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.PM
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.Col = 7
        Em0.MapCompID = tMapComp.PN
        Em0.NormID = fDefEmNormID(Em0.MapCompID)
        Em0.Unit = "[#/h]"
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.PN
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.Col = 8
        Em0.MapCompID = tMapComp.NO
        Em0.NormID = fDefEmNormID(Em0.MapCompID)
        Em0.Unit = "[g/h]"
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.NO
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.Name = "E3"
        Em0.IDstring = "E3"
        Em0.Col = 9
        Em0.NormID = tEmNorm.x_h
        Em0.MapCompID = tMapComp.Undefined
        Em0.Unit = "[?/h]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.Col = 10
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.Lambda
        Em0.Unit = "[-]"
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.Lambda
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.Col = 11
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.ExhTemp
        Em0.Unit = "[°C]"
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.Temp
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.WriteOutput = False
        Em0.Name = fMapCompName(tMapComp.TCdP2s)
        Em0.IDstring = sKey.MAP.dP2s
        Em0.Col = 12
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCdP2s
        Em0.Unit = "[-]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.WriteOutput = False
        Em0.Name = fMapCompName(tMapComp.TCPneg3s)
        Em0.IDstring = sKey.MAP.Pneg3s
        Em0.Col = 13
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCPneg3s
        Em0.Unit = "[-]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.WriteOutput = False
        Em0.Name = fMapCompName(tMapComp.TCPpos3s)
        Em0.IDstring = sKey.MAP.Ppos3s
        Em0.Col = 14
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCPpos3s
        Em0.Unit = "[-]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.WriteOutput = False
        Em0.Name = fMapCompName(tMapComp.TCAmpl3s)
        Em0.IDstring = sKey.MAP.Ampl3s
        Em0.Col = 15
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCAmpl3s
        Em0.Unit = "[-]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.WriteOutput = False
        Em0.Name = fMapCompName(tMapComp.TCLW3p3s)
        Em0.IDstring = sKey.MAP.LW3p3s
        Em0.Col = 16
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCLW3p3s
        Em0.Unit = "[-]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.WriteOutput = False
        Em0.Name = fMapCompName(tMapComp.TCP40sABS)
        Em0.IDstring = sKey.MAP.P40sABS
        Em0.Col = 17
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCP40sABS
        Em0.Unit = "[-]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.WriteOutput = False
        Em0.Name = fMapCompName(tMapComp.TCabsdn2s)
        Em0.IDstring = sKey.MAP.absdn2s
        Em0.Col = 18
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCabsdn2s
        Em0.Unit = "[-]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.WriteOutput = False
        Em0.Name = fMapCompName(tMapComp.TCP10sn10s3)
        Em0.IDstring = sKey.MAP.P10sn10s3
        Em0.Col = 19
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCP10sn10s3
        Em0.Unit = "[-]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.WriteOutput = False
        Em0.Name = fMapCompName(tMapComp.TCdynV)
        Em0.IDstring = sKey.MAP.dynV
        Em0.Col = 20
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCdynV
        Em0.Unit = "[-]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.WriteOutput = False
        Em0.Name = fMapCompName(tMapComp.TCdynAV)
        Em0.IDstring = sKey.MAP.dynAV
        Em0.Col = 21
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCdynAV
        Em0.Unit = "[-]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.WriteOutput = False
        Em0.Name = fMapCompName(tMapComp.TCdynDAV)
        Em0.IDstring = sKey.MAP.dynDAV
        Em0.Col = 22
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCdynDAV
        Em0.Unit = "[-]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        Em0 = New cEmComp
        Em0.Col = 23
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.Extrapol
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.Extrapol
        Em0.Unit = "[-]"
        Em0.PeCorMode = fDefPeCorModeOld(Em0.MapCompID)
        EmComponents.Add(Em0.IDstring, Em0)
        MyEmList.Add(Em0.IDstring)

        For Each EmKV In EmComponents
            If EmKV.Value.MapCompID <> tMapComp.Undefined Then EmDefRef.Add(EmKV.Value.MapCompID, EmKV.Value)
        Next

        nNormed = True
        PeNormed = True

        'Werte
        Do While Not File.EndOfFile
            'Zeile einlesen
            line = File.ReadLine

            'Zeilen-Zähler hoch (wurde in ResetMe zurück gesetzt)
            iMapDim += 1

            'Drehzahl
            Lnn.Add(CSng(line(1)))

            'Leistung
            LPe.Add(CSng(line(0)))

            If iMapDim = 0 Then s1 = UBound(line)

            'Emissionen
            For Each EmKV In EmComponents
                s = EmKV.Value.Col
                If s > s1 Then
                    EmKV.Value.RawVals.Add(0)
                Else
                    EmKV.Value.RawVals.Add(CSng(line(s)))
                End If
            Next
        Loop

        'Shep-Init
        MapIntp = New cMapInterpol(Me)

        'Datei schließen
        File.Close()

        Return True

    End Function

    Public Function InitTCcomp(ByVal sK As String, ByVal TCvalues As System.Collections.Generic.Dictionary(Of tMapComp, Double)) As Boolean
        Dim KV As System.Collections.Generic.KeyValuePair(Of tMapComp, Double)
        Dim StringID As String
        Dim ID As tMapComp

        ID = fMapComp(sK)
        StringID = Trim(UCase(sK))

        'Abbruch falls Em-Komponente nicht in MAP
        If ID = tMapComp.Undefined Then
            If Not EmComponents.ContainsKey(StringID) Then Return False
        Else
            If Not EmDefRef.ContainsKey(ID) Then Return False
        End If

        'Abbruch falls TC-Faktoren für die Komponente schon definiert
        If EmComponents(StringID).TCdef Then Return False

        EmComponents(StringID).InitTC()

        For Each KV In TCvalues
            EmComponents(StringID).TCfactors(KV.Key) = KV.Value
        Next

        Return True

    End Function

    Public Sub Norm()
        Dim i As Integer
        Dim j As Integer
        Dim MapCheck As Boolean
        Dim Em0 As cEmComp

        Dim Pnenn As Single
        Dim nleerl As Single
        Dim nnenn As Single

        Dim MsgSrc As String

        MsgSrc = "MAP/Norm"

        Pnenn = VEH.Pnenn
        nleerl = VEH.nLeerl
        nnenn = VEH.nNenn

        'Drehzahl normieren
        If Not nNormed Then
            For i = 0 To iMapDim
                Lnn(i) = (Lnn(i) - nleerl) / (nnenn - nleerl)
            Next
        End If

        ' "anders" normierte Drehzahl berechnen
        For i = 0 To iMapDim
            Ln.Add((Lnn(i) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl) / VEH.nNenn)
        Next

        'Leistung normieren
        If Not PeNormed Then
            For i = 0 To iMapDim
                LPe(i) /= Pnenn
            Next
        End If

        'Emissionen entnormieren
        '   ACHTUNG: Selbst wenn x_kWh bzw. x_hPnenn in x_h umgewandelt werden muss Normed gleich bleiben weil sonst die DynKor nicht stimmt!
        For Each Em0 In EmComponents.Values
            Select Case Em0.NormID
                Case tEmNorm.x_hPnenn
                    For i = 0 To iMapDim
                        Em0.RawVals(i) *= Pnenn
                    Next
                Case tEmNorm.x_kWh
                    For i = 0 To iMapDim
                        Em0.RawVals(i) *= LPe(i) * Pnenn
                    Next
                Case Else
                    'Werte sind bereits absolut angegeben
                    'Unterscheidung in [x] und [x/h] derzeit nicht verwendet/unterstützt
            End Select
        Next

        'Check ob Drehzahl/Leistung vertauscht
        For i = 0 To iMapDim

            If Lnn(i) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl < 0 Then

                MapCheck = False

                For j = 0 To iMapDim

                    If LPe(i) < -0.1 Then

                        MapCheck = True

                    ElseIf LPe(i) > 1.1 Then

                        MapCheck = False
                        Exit For

                    End If

                Next

                If Not MapCheck Then WorkerMsg(tMsgID.Warn, "Expected Emission Map format: 1st column = Engine Speed, 2nd column = Engine Power (rpm values < 0)", MsgSrc)

                Exit For

            End If

        Next

        'FC Delauney
        Em0 = EmDefRef(tMapComp.FC)
        For i = 0 To iMapDim
            FuelMap.AddPoints(Lnn(i), LPe(i), Em0.RawVals(i))
        Next

        FuelMap.Triangulate()

    End Sub

    Public Function GetSingleValue(ByRef EmComp As tMapComp, ByVal nnorm As Single, ByVal Pnorm As Single) As Single
        MapIntp.ShepInit(nnorm, Pnorm)
        Return MapIntp.fIntShepDef(EmDefRef(EmComp))
    End Function

    Public Sub Intp_Init(ByVal Pnorm As Single, ByVal nnorm As Single)
        MapIntp.ShepInit(Pnorm, nnorm)
    End Sub

    Public Function fShep_Intp(ByRef EmComp As cEmComp) As Single
        Return MapIntp.fIntShepDef(EmComp)
    End Function

    Public Function fFCdelaunay_Intp(ByVal nnorm As Single, ByVal Pnorm As Single) As Single
        Try
            Return FuelMap.Intpol(nnorm, Pnorm)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot extrapolate FC map! n= " & nnormTonU(nnorm).ToString("0") & " [U/min], Me= " & PnormToM(nnorm, Pnorm).ToString("0.0") & " [Nm]", "MAP/FC_Intp")
            Return -100
        End Try
    End Function

    'Kennfeld-Erstellung
    Public Function CreateMAP() As Boolean

        Dim file As cFile_V3
        Dim str As System.Text.StringBuilder
        Dim Em0 As cEmComp
        Dim EmExtra As cEmComp
        Dim Extra As Boolean
        Dim EmKV As KeyValuePair(Of String, cEmComp)
        Dim s As Integer
        Dim i As Integer
        Dim j As Integer
        Dim EmMW As Dictionary(Of String, List(Of Single))
        Dim EmMW0 As KeyValuePair(Of String, List(Of Single))
        Dim TCcomponents As System.Collections.Generic.Dictionary(Of tMapComp, cEmComp)
        Dim nn As List(Of Single)
        Dim pe As List(Of Single)
        Dim px As Single
        Dim nnx As Single
        Dim dp As Single
        Dim dp0 As Single
        Dim dnn As Single
        Dim dnn0 As Single
        Dim pf As Single
        Dim pm As Single
        Dim EmCheck As List(Of Boolean)
        Dim KFanz As List(Of Integer)
        Dim KFradproz As List(Of Single)
        Dim KFmin As Dictionary(Of String, List(Of Single))
        Dim KFmax As Dictionary(Of String, List(Of Single))
        Dim ValMin As Dictionary(Of String, Single)
        Dim ValMax As Dictionary(Of String, Single)
        Dim anz As Integer
        Dim sum As Single
        Dim EmSum As Dictionary(Of String, Double)
        Dim EmDrag As Single
        Dim Pfak As List(Of Single)
        Dim IntpPe As Single
        Dim NrUsed As List(Of Integer)
        Dim Used As List(Of Boolean)

        Dim IsDrag As List(Of Boolean)
        Dim nnAb As Single
        Dim MsgSrc As String

        Dim KeyListFull As List(Of String)
        Dim KeyListLog As List(Of String)
        Dim Key As String

        MsgSrc = "MAP/CreateMAP"

        file = New cFile_V3

        If Not file.OpenWrite(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Cannot access map file (" & sFilePath & ")", MsgSrc)
            Return False
        End If

        'Reset
        ResetMe()

        If GEN.nschrit < 1 Or GEN.Pschrit < 1 Or GEN.iMsek < 1 Then
            WorkerMsg(tMsgID.Err, "Invalid 'Create Map' settings in input file!", MsgSrc)
            Return False
        End If

        '******************************************************************
        '***************** Initialisiere Em-Komponenten *******************
        MyEmList = New List(Of String)
        EmComponents = New Dictionary(Of String, cEmComp)
        TCcomponents = New Dictionary(Of tMapComp, cEmComp)
        EmDefRef = New Dictionary(Of tMapComp, cEmComp)
        EmMW = New Dictionary(Of String, List(Of Single))
        nn = New List(Of Single)
        pe = New List(Of Single)
        LPe = New List(Of Single)
        Lnn = New List(Of Single)
        Ln = New System.Collections.Generic.List(Of Single)
        EmCheck = New List(Of Boolean)
        KFanz = New List(Of Integer)
        KFmin = New Dictionary(Of String, List(Of Single))
        KFmax = New Dictionary(Of String, List(Of Single))
        ValMin = New Dictionary(Of String, Single)
        ValMax = New Dictionary(Of String, Single)
        KFradproz = New List(Of Single)
        EmSum = New Dictionary(Of String, Double)
        NrUsed = New List(Of Integer)
        Used = New List(Of Boolean)
        Pfak = New List(Of Single)
        IsDrag = New List(Of Boolean)
        KeyListFull = New List(Of String)
        KeyListLog = New List(Of String)

        TransMap = True
        'FileVersion = FormatVersion
        nNormed = True
        PeNormed = True

        s = 1   '0 = n, 1 = pe
        For Each EmKV In DRI.EmComponents
            s += 1
            Em0 = New cEmComp
            Em0.Col = s
            Em0.Name = EmKV.Value.Name  'das ist schon der "schöne" Name! D.h. "NOx" und nicht "<NOX>"
            Em0.Unit = EmKV.Value.Unit
            Em0.MapCompID = EmKV.Value.MapCompID
            Em0.NormID = EmKV.Value.NormID
            Em0.IDstring = EmKV.Value.IDstring

            'PeCorMode: Falls nicht in MES/NPI vorgegeben gilt bei Nicht-Default-Em was in cEmComp.New() eingestellt ist
            If DRI.CreateMapParDef Then
                Em0.PeCorMode = DRI.MapPfak(EmKV.Key)
            ElseIf Em0.MapCompID <> tMapComp.Undefined Then
                Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
            End If

            If Em0.MapCompID <> tMapComp.Undefined Then

                If fMapCompIsTC(Em0.MapCompID) Then

                    'Falls TC angegeben dann Abbruch
                    WorkerMsg(tMsgID.Err, "Component '" & Em0.Name & "' is invalid (Trans.Corr. Parameter)!", MsgSrc)
                    Return False

                Else

                    'Interpolator auswählen
                    Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)

                    'Eintrag in Referenz-Dictionary (Es wird nicht überprüft ob Em-Comp. doppelt vorkommt weil das schon in DRI.ReadFile passiert)
                    EmDefRef.Add(Em0.MapCompID, Em0)

                End If

            End If

            EmComponents.Add(EmKV.Key, Em0)
            MyEmList.Add(EmKV.Key)

            'Infos ausgeben
            WorkerMsg(tMsgID.Normal, "   '" & Em0.Name & "': Unit = " & Em0.Unit & ", PeCorMode = " & fPwCorName(Em0.PeCorMode), MsgSrc)

        Next

        'Dynamikparameter und Extrapol kommen noch dazu
        Em0 = New cEmComp
        Em0.Col = s + 1
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCdP2s
        Em0.Unit = "[-]"
        Em0.WriteOutput = False
        Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)
        Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.dP2s
        TCcomponents.Add(Em0.MapCompID, Em0)

        Em0 = New cEmComp
        Em0.Col = s + 2
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCPneg3s
        Em0.Unit = "[-]"
        Em0.WriteOutput = False
        Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)
        Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.Pneg3s
        TCcomponents.Add(Em0.MapCompID, Em0)

        Em0 = New cEmComp
        Em0.Col = s + 3
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCPpos3s
        Em0.Unit = "[-]"
        Em0.WriteOutput = False
        Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)
        Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.Ppos3s
        TCcomponents.Add(Em0.MapCompID, Em0)

        Em0 = New cEmComp
        Em0.Col = s + 4
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCAmpl3s
        Em0.Unit = "[-]"
        Em0.WriteOutput = False
        Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)
        Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.Ampl3s
        TCcomponents.Add(Em0.MapCompID, Em0)

        Em0 = New cEmComp
        Em0.Col = s + 5
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCLW3p3s
        Em0.Unit = "[-]"
        Em0.WriteOutput = False
        Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)
        Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.LW3p3s
        TCcomponents.Add(Em0.MapCompID, Em0)

        Em0 = New cEmComp
        Em0.Col = s + 6
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCP40sABS
        Em0.Unit = "[-]"
        Em0.WriteOutput = False
        Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)
        Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.P40sABS
        TCcomponents.Add(Em0.MapCompID, Em0)

        Em0 = New cEmComp
        Em0.Col = s + 7
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCabsdn2s
        Em0.Unit = "[-]"
        Em0.WriteOutput = False
        Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)
        Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.absdn2s
        TCcomponents.Add(Em0.MapCompID, Em0)

        Em0 = New cEmComp
        Em0.Col = s + 8
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCP10sn10s3
        Em0.Unit = "[-]"
        Em0.WriteOutput = False
        Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)
        Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.P10sn10s3
        TCcomponents.Add(Em0.MapCompID, Em0)

        Em0 = New cEmComp
        Em0.Col = s + 8
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCdynV
        Em0.Unit = "[-]"
        Em0.WriteOutput = False
        Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)
        Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.dynV
        TCcomponents.Add(Em0.MapCompID, Em0)

        Em0 = New cEmComp
        Em0.Col = s + 9
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCdynAV
        Em0.Unit = "[-]"
        Em0.WriteOutput = False
        Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)
        Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.dynAV
        TCcomponents.Add(Em0.MapCompID, Em0)

        Em0 = New cEmComp
        Em0.Col = s + 10
        Em0.NormID = tEmNorm.x
        Em0.MapCompID = tMapComp.TCdynDAV
        Em0.Unit = "[-]"
        Em0.WriteOutput = False
        Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)
        Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
        Em0.Name = fMapCompName(Em0.MapCompID)
        Em0.IDstring = sKey.MAP.dynDAV
        TCcomponents.Add(Em0.MapCompID, Em0)

        EmExtra = New cEmComp
        EmExtra.Col = s + 11
        EmExtra.NormID = tEmNorm.x
        EmExtra.MapCompID = tMapComp.Extrapol
        EmExtra.Unit = "[-]"
        EmExtra.IntpolV2 = fDefIntpV2(tMapComp.Extrapol)
        EmExtra.PeCorMode = fDefPeCorMode(tMapComp.Extrapol)
        EmExtra.Name = fMapCompName(tMapComp.Extrapol)
        EmExtra.IDstring = sKey.MAP.Extrapol


        '******************************************************************
        '************* Mittelwert-Dictionary initialisieren ***************
        For Each EmKV In EmComponents

            EmMW.Add(EmKV.Key, New List(Of Single))
            EmSum.Add(EmKV.Key, 0)

            KFmin.Add(EmKV.Key, New List(Of Single))
            KFmax.Add(EmKV.Key, New List(Of Single))

            ValMin.Add(EmKV.Key, -1)
            ValMax.Add(EmKV.Key, -1)

            KeyListLog.Add(EmKV.Key)

        Next

        For Each Em0 In TCcomponents.Values

            EmMW.Add(Em0.Name, New List(Of Single))
            EmSum.Add(Em0.Name, 0)

        Next

        '******************************************************************
        '********************* Mittelwerte berechnen **********************

        'Haupt-Schleife
        If GEN.iMsek > 1 Then

            'Listen erstellen
            For i = 0 To MODdata.tDim

                pe.Add(0)
                nn.Add(0)

                For Each EmKV In EmComponents
                    EmMW(EmKV.Key).Add(0)
                Next
                For Each Em0 In TCcomponents.Values
                    EmMW(Em0.Name).Add(0)
                Next

            Next

            'Schleife über Messwerte
            For i = 0 To MODdata.tDim

                If i + 1 < GEN.iMsek Then

                    'Bereich unter iMsek mit Messwert belegen
                    pe(i) = MODdata.Pe(i)
                    nn(i) = MODdata.nn(i)

                    For Each EmKV In EmComponents
                        EmMW(EmKV.Key)(i) = DRI.EmComponents(EmKV.Key).RawVals(i)
                    Next

                    For Each Em0 In TCcomponents.Values
                        EmMW(Em0.Name)(i) = MODdata.TC.TCcomponents(Em0.MapCompID)(i)
                    Next

                Else

                    'Bereich über iMsek mit Mittelwerten der Messwerte belegen
                    For j = 0 To (GEN.iMsek - 1)

                        pe(i) += MODdata.Pe(i - j) / GEN.iMsek
                        nn(i) += MODdata.nn(i - j) / GEN.iMsek

                        For Each EmKV In EmComponents
                            EmMW(EmKV.Key)(i) += DRI.EmComponents(EmKV.Key).RawVals(i - j) / GEN.iMsek
                        Next

                        For Each Em0 In TCcomponents.Values
                            EmMW(Em0.Name)(i) += MODdata.TC.TCcomponents(Em0.MapCompID)(i - j) / GEN.iMsek
                        Next

                    Next
                End If

            Next

        Else

            'Keine Mittelwertbildung
            For i = 0 To MODdata.tDim

                pe.Add(MODdata.Pe(i))
                nn.Add(MODdata.nn(i))

                For Each EmKV In EmComponents
                    EmMW(EmKV.Key).Add(DRI.EmComponents(EmKV.Key).RawVals(i))
                Next
                For Each Em0 In TCcomponents.Values
                    EmMW(Em0.Name).Add(MODdata.TC.TCcomponents(Em0.MapCompID)(i))
                Next
            Next

        End If

        'EmCheck: False = Wert noch nicht verwendet, True = Wert wurde bereits in Kennfeld verwurschtet
        For j = 0 To MODdata.tDim
            EmCheck.Add(False)
        Next

        '******************************************************************
        '*************************** Rasterung ****************************

        dnn = 1.2 / (GEN.nschrit)
        dp = 1.4 / (GEN.Pschrit)

        nnx = 0 - dnn    'wird beim ersten Durchgang durch dn erhöht => Startwert = 0

        iMapDim = -1
        Do While nnx + dnn <= 1.20001     'nn-Schleife

            nnx += dnn

            pf = FLD.Pfull(nnx) / VEH.Pnenn
            pm = FLD.Pdrag(nnx) / VEH.Pnenn

            If GEN.KFcutDrag Then

                px = -0.4 - 5 * dp
                Do Until px + dp > pm
                    px += dp
                Loop
                px -= dp        'wird beim ersten Durchgang durch dp erhöht!

            Else

                px = -0.4 - dp  'wird beim ersten Durchgang durch dp erhöht => Startwert = -0.4

            End If


            If GEN.KFcutFull Then

                Do While px <= pf + 0.0001   'pe-Schleife bis eine Reihe über Volllast

                    px += dp

                    If Not (Math.Abs(px) < 0.00001 And nnx < 0.00001) Then
                        LPe.Add(Math.Round(px, 6))
                        Lnn.Add(Math.Round(nnx, 6))
                        IsDrag.Add(False)
                        iMapDim += 1
                    End If

                Loop

            Else

                Do While px <= 1   'pe-Schleife bis 1

                    px += dp

                    If Not (Math.Abs(px) < 0.00001 And nnx < 0.00001) Then
                        LPe.Add(px)
                        Lnn.Add(nnx)
                        IsDrag.Add(False)
                        iMapDim += 1
                    End If

                Loop

            End If


        Loop

        'Schlepp hinzufügen mit halber nn-Schrittweite. Wird für Pfak benötigt. Falls GEN.KFinsertDrag deaktiviert wird sie später wieder gelöscht.
        nnx = 0 - dnn / 2
        Do While nnx + dnn / 2 <= 1.20001
            nnx += dnn / 2
            LPe.Add(FLD.Pdrag(nnx) / VEH.Pnenn)
            Lnn.Add(nnx)
            IsDrag.Add(True)
            iMapDim += 1
        Loop

        'Leerlaufpunkt hinzufügen
        LPe.Add(0)
        Lnn.Add(0)
        IsDrag.Add(False)
        iMapDim += 1

        '******************************************************************
        '**************** NrUsed Erstellung / Null setzen *****************
        For j = 0 To MODdata.tDim
            NrUsed.Add(0)
            Used.Add(False)
        Next

        '******************************************************************
        '**************** Messwerte in Raster reinrechnen *****************

        'Basis Schrittweite zwischenspeichern
        dnn0 = dnn
        dp0 = dp

        'Schleife über Rasterpunkte (i)
        For i = 0 To iMapDim

            'Summen/Anzahl/Flags zurücksetzen
            Extra = False
            dnn = dnn0
            dp = dp0

            'Schlepp-Leistung
            pm = FLD.Pdrag(Lnn(i)) / VEH.Pnenn

            'Schleife bis genug Werte im Radius gefunden
            Do

                'Zähler/Summen zurück setzen
                anz = 0
                For Each EmMW0 In EmMW
                    EmSum(EmMW0.Key) = 0
                Next
                IntpPe = 0

                'Schleife über Messwerte    (j)
                For j = 0 To MODdata.tDim

                    'Falls innerhalb von Radius...
                    If Math.Abs(pe(j) - LPe(i)) <= dp / 2 And Math.Abs(nn(j) - Lnn(i)) <= dnn / 2 Then

                        'Anz + 1
                        anz += 1

                        'Schleife über alle Em-Komp.
                        For Each EmMW0 In EmMW

                            'Summe +
                            EmSum(EmMW0.Key) += EmMW0.Value(j)

                            'Min/Max belegen (für Log-Ausgabe)
                            If anz = 1 Then
                                ValMin(EmMW0.Key) = EmMW0.Value(j)
                                ValMax(EmMW0.Key) = EmMW0.Value(j)
                            Else
                                ValMin(EmMW0.Key) = Math.Min(EmMW0.Value(j), ValMin(EmMW0.Key))
                                ValMax(EmMW0.Key) = Math.Max(EmMW0.Value(j), ValMax(EmMW0.Key))
                            End If

                        Next

                        'Interpolierte Leistung aufsummieren (wird dann durch Anz dividiert)
                        IntpPe += pe(j)

                        'Zählen wieviel Messwerte für Rasterpunkt verwendet werden (Log-Ausgabe)
                        Used(j) = True

                    Else

                        Used(j) = False

                    End If

                Next

                'Falls keine Messwerte im Radius (Anz=0) dann Radius vergrößern und Extra-Flag setzen
                If anz < 2 Then
                    Extra = True
                    dp *= 1.1
                    dnn *= 1.1
                End If

            Loop Until anz > 1

            'NrUsed belegen
            For j = 0 To MODdata.tDim
                If Used(j) Then NrUsed(j) += 1
            Next

            'Interpolierte Leistung = Summe / Anz
            IntpPe /= anz

            'Pfak berechnen:
            '   Falls oberhalb Pschlepp dann Pfak laut Formel oder 1 falls Abstand zw. Pe-Interpol und Pschlepp zu gering
            '   Unterhalb von Pschlepp Pfak=0 => Em-Wert=Null
            If LPe(i) > pm Then
                If (IntpPe - pm) > 0.05 Then
                    Pfak.Add(Math.Abs((LPe(i) - pm) / (IntpPe - pm)))
                Else
                    Pfak.Add(1)
                End If
            Else
                Pfak.Add(0)
            End If

            'Extrapol-Flag in Extrapol-Spalte (1/0) übernehmen
            If Extra Then
                EmExtra.RawVals.Add(1)
            Else
                EmExtra.RawVals.Add(0)
            End If

            'Für Log-Ausgabe
            KFanz.Add(anz)
            KFradproz.Add(dnn / dnn0)

            'Schleife über Em-Comp (innerhalb Rasterpunkt Schleife)
            For Each EmKV In EmComponents

                'Falls Option 'Schlepp-Em aus .FLD' belegen und Rasterpunkt-Leistung <= Schleppleistung
                If Not GEN.KFDragIntp And LPe(i) <= pm + 0.0001 Then

                    'Falls Schlepp-Em in .FLD vorhanden dann nehmen sonst mit Null belegen
                    If FLD.EmDef(EmKV.Key) Then
                        EmKV.Value.RawVals.Add(FLD.EmDrag(EmKV.Key, Lnn(i)))
                    Else
                        EmKV.Value.RawVals.Add(0)
                    End If

                Else    'Option 'Schlepp-Em aus Messwerten' bzw. Punkte über Schleppleistung

                    'Em-Belegung ohne Pfak (=> Pfak wird später gemacht)
                    EmKV.Value.RawVals.Add(CSng(EmSum(EmKV.Key) / anz))

                End If

                'Für Log-Ausgabe
                KFmin(EmKV.Key).Add(ValMin(EmKV.Key))
                KFmax(EmKV.Key).Add(ValMax(EmKV.Key))

            Next

            'TC-Faktoren ohne Pfak übernehmen
            For Each Em0 In TCcomponents.Values
                Em0.RawVals.Add(CSng(EmSum(Em0.Name) / anz))
            Next

        Next

        '******************************************************************
        '*****************************  Pfak ******************************

        '!!! WICHTIG !!!
        'Schleife geht über alle Rasterpunkt (auch über die Pe <= PeSchlepp bzw. PeIntpol nahe an Pschlepp).
        '   Das ist OK weil Pfak dort sowieso mit Eins beleget.

        'Schleife über Em-Comp
        For Each EmKV In EmComponents

            'Falls keine Create Map Einstellungen (in .NPI/.MES) oder Pfak explizit aktiviert => Pfak verwenden
            If EmKV.Value.PeCorMode <> tIntpPeCorMode.PeCorOff Then

                'Schleife über Rasterpunkte (i)
                For i = 0 To iMapDim

                    If EmKV.Value.PeCorMode = tIntpPeCorMode.PeCorNull Then

                        'Altes Pfak mit Extrapolation von Null weg
                        EmDrag = 0

                    Else    'PeCorMode = tIntpPeCorMode.PeCorEmDrag

                        'Schlepp-Emission raus suchen
                        '   Schlepp-Em aus nächstgelegenen Rasterpunkt nehmen. Das geht weil Schleppkurve 
                        '   immer ins Map kommt (auch wenn sie dann später raus gelöscht wird) !!
                        '   Option 'Schlepp-Em aus .FLD' spielt keine Rolle weil das nur die Belegungs-Methode der Schleppkurve betrifft (s.o.) 
                        EmDrag = 0
                        nnAb = dnn0
                        For j = 0 To iMapDim
                            If IsDrag(j) Then
                                If Math.Abs(Lnn(i) - Lnn(j)) < nnAb Then
                                    nnAb = Math.Abs(Lnn(i) - Lnn(j))
                                    EmDrag = EmKV.Value.RawVals(j)
                                End If
                            End If
                        Next

                    End If

                    'Pfak anwenden
                    EmKV.Value.RawVals(i) = CSng(Pfak(i) * (EmKV.Value.RawVals(i) - EmDrag) + EmDrag)

                Next

            End If

        Next

        '******************************************************************
        '******************* Normieren (Wert und Unit) ********************
        For Each EmKV In EmComponents

            'Falls Vorgabe in MES/NPI-Datei dann verwenden
            If DRI.CreateMapParDef Then

                If DRI.MapUnitsNormed(EmKV.Key) Then

                    'Werte normieren
                    For i = 0 To iMapDim
                        EmKV.Value.RawVals(i) /= VEH.Pnenn
                    Next

                    EmKV.Value.NormID = tEmNorm.x_hPnenn

                End If

            Else

                'Sonst Standard-Normierung verwenden
                If EmKV.Value.MapCompID <> tMapComp.Undefined Then

                    If fDefEmNormID(EmKV.Value.MapCompID) = tEmNorm.x_hPnenn Then

                        'Werte normieren
                        For i = 0 To iMapDim
                            EmKV.Value.RawVals(i) /= VEH.Pnenn
                        Next

                        EmKV.Value.NormID = tEmNorm.x_hPnenn

                    End If

                End If

            End If

        Next

        '******************************************************************
        '****************** EmComponents zusammenfassen *******************
        For Each Em0 In TCcomponents.Values
            EmComponents.Add(Em0.Name, Em0)
            MyEmList.Add(Em0.Name)
        Next

        EmComponents.Add(EmExtra.Name, EmExtra)
        MyEmList.Add(EmExtra.Name)

        For Each EmKV In EmComponents
            KeyListFull.Add(EmKV.Key)
        Next


        '******************************************************************
        '*********** Schleppkurve wieder raus nehmen (optional) ***********
        If Not GEN.KFinsertDrag Then

            'Schleife über Rasterpunkte (i). Keine For-Schleife weil iMapDim reduziert wird
            i = -1
            Do While i < iMapDim
                i += 1
                If IsDrag(i) Then
                    Lnn.RemoveAt(i)
                    LPe.RemoveAt(i)
                    IsDrag.RemoveAt(i)

                    For Each EmKV In EmComponents
                        EmKV.Value.RawVals.RemoveAt(i)
                    Next

                    iMapDim -= 1
                    i -= 1
                End If
            Loop

        End If


        '******************************************************************
        '************************** Ausgabe Map '**************************

        str = New System.Text.StringBuilder

        file.WriteLine("c VECTO Engine Map")
        file.WriteLine("c VECTO " & VECTOvers)
        file.WriteLine("c " & Now.ToString)
        file.WriteLine("c Input File: " & JobFile)
        file.WriteLine("c Measurement Data: " & CurrentCycleFile)
        file.WriteLine("c Rated engine power: " & VEH.Pnenn)
        file.WriteLine("c Rated engine speed: " & VEH.nNenn)
        file.WriteLine("c Idle engine speed: " & VEH.nLeerl)

        'file.WriteLine("V" & FormatVersion)

        'Header
        str.Append("n_norm,Pe_norm")

        For Each Key In KeyListFull
            'ACHTUNG: Nicht Name sondern sKey !!!
            str.Append("," & EmComponents(Key).IDstring)
        Next

        file.WriteLine(str.ToString)

        str.Length = 0

        'Unit
        str.Append(sKey.Normed & "," & sKey.Normed)
        For Each Key In KeyListFull
            If EmComponents(Key).NormID = tEmNorm.x_hPnenn Then
                str.Append("," & EmComponents(Key).Unit & sKey.Normed)
            Else
                str.Append("," & EmComponents(Key).Unit)
            End If
        Next
        file.WriteLine(str.ToString)

        'Werte
        For i = 0 To iMapDim
            str.Length = 0
            str.Append(CStr(Lnn(i)))
            str.Append("," & CStr(LPe(i)))
            For Each Key In KeyListFull
                str.Append("," & EmComponents(Key).RawVals(i))
            Next
            file.WriteLine(str.ToString)
        Next

        file.Close()

        '******************************************************************
        '********************** Ausgabe Zusatzinfo '***********************

        If Not file.OpenWrite(fFileWoExt(sFilePath) & ".log") Then
            WorkerMsg(tMsgID.Err, "Cannot access file (" & fFileWoExt(sFilePath) & ".log" & ")!", MsgSrc)
            Return False
        End If

        file.WriteLine("c VECTO Engine Map Creation Detailed Information")
        file.WriteLine("c VECTO " & VECTOvers)
        file.WriteLine("c " & Now.ToString)
        file.WriteLine("c Input File: " & JobFile)
        file.WriteLine("c Measurement Data: " & CurrentCycleFile)
        file.WriteLine("c Output Emission Map:" & sFilePath)
        file.WriteLine(" ")

        sum = 0
        For i = 0 To iMapDim
            sum += EmExtra.RawVals(i)       'EmComponents(sKey.MAP.Extr)
        Next
        file.WriteLine("c Nr. of extrapolated points: " & sum)

        file.WriteLine(" ")
        file.WriteLine("Load points of created Emission Map")
        file.WriteLine("NrIntPts" & vbTab & "...Number of load points (from Measurement Data) interpolated for the Emission Map load point.")
        file.WriteLine("RngExtRat" & vbTab & "...Extension Ratio of Interpolation Range. RngExtRat > 1 means Extrapolation.")
        file.WriteLine("Em Min/Max" & vbTab & "...Extremum values of the interpolated load points (from Measurement Data) for the Emission Map load point (if NrIntPts > 1). Units not normalized.")
        file.WriteLine("PeCor" & vbTab & "...Engine Power Correction Factor. PeCor = (Emission Map engine power - Drag) / (Interpolated engine power - Drag). PeCor = 1 for pe < 0.")
        file.WriteLine(" ")

        'Header (MAP)
        str.Length = 0
        str.Append("n,pe,PeCor,NrIntPts,RngExtRat")
        For Each Key In KeyListLog
            If Not KFmin.ContainsKey(Key) Then Exit For
            str.Append("," & EmComponents(Key).Name & " Min")
            str.Append("," & EmComponents(Key).Name & " Max")
        Next
        file.WriteLine(str.ToString)

        'Werte
        str.Length = 0
        For i = 0 To iMapDim

            str.Length = 0

            str.Append(CStr(Lnn(i)))
            str.Append("," & CStr(LPe(i)))
            str.Append("," & CStr(Pfak(i)))
            str.Append("," & KFanz(i))
            str.Append("," & KFradproz(i))

            For Each Key In KeyListLog
                str.Append("," & KFmin(Key)(i))
                str.Append("," & KFmax(Key)(i))
            Next

            file.WriteLine(str.ToString)

        Next

        file.WriteLine(" ")
        file.WriteLine("Load points of Measurement Data.")
        file.WriteLine("Averaging interval [s]: " & GEN.iMsek)
        file.WriteLine("NrUsed" & vbTab & "...Number of times used for interpolation.")
        file.WriteLine(" ")

        'Header (MES/NPI)
        str.Length = 0

        str.Append("n,pe,NrUsed")

        For Each Key In KeyListLog
            str.Append("," & EmComponents(Key).Name)
        Next

        file.WriteLine(str.ToString)

        For j = 0 To MODdata.tDim

            str.Length = 0

            str.Append(nn(j) & "," & pe(j) & "," & NrUsed(j))

            For Each Key In KeyListLog
                str.Append("," & EmMW(Key)(j))
            Next

            file.WriteLine(str.ToString)
        Next

        file.Close()

        'Shep-Init
        MapIntp = New cMapInterpol(Me)

        Return True

    End Function

    'Default Shepard wie in intpshep()
    Private Class cMapInterpol

        Private iMapDim As Integer
        Private Pe0 As Single
        Private n0 As Single
        Private myMap As cMAP
        Private PeDrag As Single

        'Interpolator V1
        Private abOK As Boolean()                                   'Array für alle Punkte (iMapDim)
        Private ab As Double()                                      'Array für ausgewählte Punkte (inim)
        Private wisum As Double
        Private PeIntp As Single

        'Interpolator V2
        Private abOKV2 As Boolean()                                   'Array für alle Punkte (iMapDim)
        Private abV2 As Double()                                      'Array für ausgewählte Punkte (inim)
        Private wisumV2 As Double
        Private PeIntpV2 As Single

        Public Sub New(ByRef MapClass As cMAP)
            myMap = MapClass
            iMapDim = MapClass.iMapDim
        End Sub

        Public Sub CleanUp()
            myMap = Nothing
        End Sub

        Public Sub ShepInit(ByVal nnorm As Single, ByVal Pnorm As Single)
            Dim ab0 As Double()
            Dim i As Integer
            Dim i0 As Integer
            Dim sumo As Double
            Dim iminMin As Integer
            Dim Radius As Double
            Dim inim As Integer

            '*****************************************************************************
            '*****************************************************************************
            '********************************  V1 & V2  **********************************
            '*****************************************************************************
            '*****************************************************************************

            Pe0 = Pnorm
            n0 = (nnorm * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl) / VEH.nNenn
            PeDrag = FLD.Pdrag(nnorm) / VEH.Pnenn       'Schleppkurve für die geg. Drehzahl

            ReDim ab0(iMapDim)


            '*****************************************************************************
            '*****************************************************************************
            '***********************************  V1  ************************************
            '*****************************************************************************
            '*****************************************************************************

            ReDim abOK(iMapDim)

            'Abstand berechnen und korrigieren.
            For i = 0 To iMapDim
                If (Pnorm < 0.05 And Pnorm >= 0.0) Then

                    'Um Nullleistung werden Kennfeldpunkte um Pe=0 hoeher gewichtet und Drehzahlabstand geringer,
                    ' da Interpolation aus Punkten mit hoeherer last dort schlecht passt:
                    ab0(i) = (Pnorm - myMap.LPe(i)) ^ 2 + (n0 - myMap.Ln(i)) ^ 2 * 888.9 * (Math.Abs(Pnorm) ^ 3 + 0.001)
                    ab0(i) = ab0(i) * (Math.Abs(Pnorm) + Math.Abs(myMap.LPe(i)) + 0.005) * 9.52

                Else

                    'Quadrat des Abstandes:             
                    ab0(i) = (Pnorm - myMap.LPe(i)) ^ 2 + (n0 - myMap.Ln(i)) ^ 2

                    'Falls Vorzeichen von Pe unterschiedlich (Last/Schlepp-Trennung) wird Abstand vergroessert um Schlepp bei Schlepp mehr zu gewichten:
                    If Pnorm * myMap.LPe(i) < 0 Then ab0(i) *= 50

                End If

            Next

            'Punkte innerhalb Radius zählen und ggf. Radius erhöhen
            Radius = 0.01   '<= Startwert ist Radius * 2 weil in Do-Schleife gleich verdoppelt wird!   
            iminMin = 2

            Do
                Radius *= 2
                inim = 0
                For i = 0 To iMapDim
                    If ab0(i) <= Radius Then
                        inim += 1
                        abOK(i) = True
                    Else
                        abOK(i) = False
                    End If
                Next
            Loop Until inim >= iminMin

            'Abstand-Array erstellen und Leistung interpolieren für Pe-Korrektur
            ReDim ab(inim - 1)
            i0 = -1
            sumo = 0
            For i = 0 To iMapDim
                If abOK(i) Then
                    i0 += 1
                    ab(i0) = Math.Max(ab0(i), 0.00000001)
                    sumo += myMap.LPe(i) / ab(i0)
                End If
            Next

            'Berechnung von wisum
            wisum = 0
            For i = 0 To inim - 1
                wisum = wisum + 1.0 / ab(i)
            Next

            'Interpolierte Leistung berechnen
            PeIntp = sumo / wisum


            '*****************************************************************************
            '*****************************************************************************
            '***********************************  V2  ************************************
            '*****************************************************************************
            '*****************************************************************************

            ReDim abOKV2(iMapDim)

            'Abstand berechnen und korrigieren.
            For i = 0 To iMapDim

                'Quadrat des Abstandes:                 
                ab0(i) = (Pnorm - myMap.LPe(i)) ^ 2 + (nnorm - myMap.Lnn(i)) ^ 2

            Next

            'Punkte innerhalb Radius zählen und ggf. Radius erhöhen
            Radius = 0.0001   '<= Startwert ist Radius * 2 weil in Do-Schleife gleich verdoppelt wird!       '0.0001
            iminMin = 3

            Do
                Radius *= 2
                inim = 0
                For i = 0 To iMapDim
                    If ab0(i) <= Radius Then
                        inim += 1
                        abOKV2(i) = True
                    Else
                        abOKV2(i) = False
                    End If
                Next
            Loop Until inim >= iminMin

            'Abstand-Array erstellen und Leistung interpolieren für Pe-Korrektur
            ReDim abV2(inim - 1)
            i0 = -1
            sumo = 0
            For i = 0 To iMapDim
                If abOKV2(i) Then
                    i0 += 1
                    abV2(i0) = Math.Max(ab0(i), 0.00000001)
                    sumo += myMap.LPe(i) / abV2(i0)
                End If
            Next

            'Berechnung von wisumV2
            wisumV2 = 0
            For i = 0 To inim - 1
                wisumV2 = wisumV2 + 1.0 / abV2(i)
            Next

            'Interpolierte Leistung berechnen
            PeIntpV2 = sumo / wisumV2

        End Sub

        Public Function fIntShepDef(ByRef EmComp As cEmComp) As Single

            Dim i As Integer
            Dim i0 As Integer
            Dim sumo As Double
            Dim EmDrag As Single
            Dim wisum0 As Double
            Dim PeIntp0 As Single

            i0 = -1
            sumo = 0

            If EmComp.IntpolV2 Then

                For i = 0 To iMapDim
                    If abOKV2(i) Then
                        i0 += 1
                        sumo += EmComp.RawVals(i) / abV2(i0)
                    End If
                Next

                wisum0 = wisumV2
                PeIntp0 = PeIntpV2

            Else

                For i = 0 To iMapDim
                    If abOK(i) Then
                        i0 += 1
                        sumo += EmComp.RawVals(i) / ab(i0)
                    End If
                Next

                wisum0 = wisum
                PeIntp0 = PeIntp

            End If

            Select Case EmComp.PeCorMode

                Case tIntpPeCorMode.PeCorOff
                    Return sumo / wisum0

                Case tIntpPeCorMode.PeCorNull

                    If (PeIntp0 - PeDrag) > 0.05 Then
                        Return Math.Abs((Pe0 - PeDrag) / (PeIntp0 - PeDrag)) * sumo / wisum0
                    Else
                        Return sumo / wisum0
                    End If

                Case tIntpPeCorMode.PeCorNullPmin

                    If (PeIntp0 - PeDrag) > 0.05 And Math.Abs(Pe0) > 0.01 Then
                        Return Math.Abs((Pe0 - PeDrag) / (PeIntp0 - PeDrag)) * sumo / wisum0
                    Else
                        Return sumo / wisum0
                    End If

                Case Else  'tIntpPeCorMode.PeCorEmDrag

                    If PeIntp0 - PeDrag > 0.05 Then
                        EmDrag = fEmDrag(EmComp)
                        Return (Math.Abs(Pe0 - PeDrag) / (PeIntp0 - PeDrag)) * (sumo / wisum0 - EmDrag) + EmDrag
                    Else
                        Return sumo / wisum0
                    End If

            End Select

        End Function

        'Berechnet Emission an Schleppkurve
        Private Function fEmDrag(ByRef EmComp As cEmComp) As Single
            Dim ab0 As Double()
            Dim abOK_loc As Boolean()
            Dim ab_loc As Double()
            Dim i As Integer
            Dim i0 As Integer
            Dim sumo As Double
            Dim iminMin As Integer
            Dim Pnorm As Single
            Dim Radius As Double
            Dim wisum_loc As Double

            ReDim ab0(iMapDim)
            ReDim abOK_loc(iMapDim)
            Dim inim As Integer

            'Es wird an Schleppkurve gesucht
            Pnorm = PeDrag
            'n0 ist schon in Init definiert worden

            'Abstand berechnen und korrigieren.
            For i = 0 To iMapDim

                'Quadrat des Abstandes:
                ab0(i) = (Pnorm - myMap.LPe(i)) ^ 2 + (n0 - myMap.Ln(i)) ^ 2

                'Falls Vorzeichen von Pe unterschiedlich (Last/Schlepp-Trennung) wird Abstand vergroessert um Schlepp bei Schlepp mehr zu gewichten:
                If Not EmComp.IntpolV2 Then
                    If Pnorm * myMap.LPe(i) < 0 Then ab0(i) *= 50
                End If

            Next

            'Punkte innerhalb Radius zählen und ggf. Radius erhöhen
            Radius = 0.0001   '<= Startwert ist Radius * 2 weil in Do-Schleife gleich verdoppelt wird!       '0.0001
            iminMin = 3

            Do
                Radius *= 2
                inim = 0
                For i = 0 To iMapDim
                    If ab0(i) <= Radius Then
                        inim += 1
                        abOK_loc(i) = True
                    Else
                        abOK_loc(i) = False
                    End If
                Next
            Loop Until inim >= iminMin

            'Abstand-Array erstellen
            ReDim ab_loc(inim - 1)
            i0 = -1
            For i = 0 To iMapDim
                If abOK_loc(i) Then
                    i0 += 1
                    ab_loc(i0) = Math.Max(ab0(i), 0.00000001)
                End If
            Next

            'Berechnung von wisum
            wisum_loc = 0
            For i = 0 To inim - 1
                wisum_loc = wisum_loc + 1.0 / ab_loc(i)
            Next

            'Emission berechnen
            sumo = 0
            i0 = -1
            For i = 0 To iMapDim
                If abOK_loc(i) Then
                    i0 += 1
                    sumo += EmComp.RawVals(i) / ab_loc(i0)
                End If
            Next

            Return sumo / wisum_loc

        End Function

    End Class




#Region "Properties"

    Public Property FilePath() As String
        Get
            Return sFilePath
        End Get
        Set(ByVal value As String)
            sFilePath = value
        End Set
    End Property

    Public ReadOnly Property EmList As List(Of String)
        Get
            Return MyEmList
        End Get
    End Property


#End Region




End Class








