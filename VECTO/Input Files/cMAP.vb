' Copyright 2014 European Union.
' Licensed under the EUPL (the 'Licence');
'
' * You may not use this work except in compliance with the Licence.
' * You may obtain a copy of the Licence at: http://ec.europa.eu/idabc/eupl
' * Unless required by applicable law or agreed to in writing,
'   software distributed under the Licence is distributed on an "AS IS" basis,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'
' See the LICENSE.txt for the specific language governing permissions and limitations.
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

        'Stop if there's no file
        If sFilePath = "" OrElse Not IO.File.Exists(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Map file not found! (" & sFilePath & ")", MsgSrc)
            Return False
        End If

        'Open file
        file = New cFile_V3
        If Not file.OpenRead(sFilePath) Then
            file = Nothing
            WorkerMsg(tMsgID.Err, "Failed to open file (" & sFilePath & ") !", MsgSrc)
            Return False
        End If

        'Initi Lists (before version check so ReadOldFormat works)
        MyEmList = New List(Of String)
        EmComponents = New System.Collections.Generic.Dictionary(Of String, cEmComp)
        EmDefRef = New System.Collections.Generic.Dictionary(Of tMapComp, cEmComp)
        LPe = New System.Collections.Generic.List(Of Single)
        Lnn = New System.Collections.Generic.List(Of Single)
        Ln = New System.Collections.Generic.List(Of Single)
        SwitchOn = False

        'Now checking whether MIP or MAP
        '...is Read.

        ''***
        ''*** First line: Version
        'line = file.ReadLine
        'txt = Trim(UCase(line(0)))
        'If Microsoft.VisualBasic.Left(txt, 1) = "V" Then
        '    ' "V" entfernen => Zahl bleibt übrig
        '    txt = txt.Replace("V", "")
        '    If Not IsNumeric(txt) Then
        '        WorkerMsg(tMsgID.Err, "File Version invalid!", MsgSrc)
        '        GoTo lbEr
        '    Else
        '        'Specify Version
        '        FileVersion = CInt(txt)
        '    End If
        'Else
        '    file.Close()
        '    Return ReadOldFormat()
        'End If

        ''Version Check: abort if input file format is newer than version PHEM
        'If FileVersion > FormatVersion Then
        '    WorkerMsg(tMsgID.Err, "File Version not supported!", MsgSrc)
        '    GoTo lbEr
        'End If

        ''Column 2: Option "+" = parameter for KF creation
        'If UBound(line) > 0 Then
        '    If Trim(line(1)) = "+" Then
        '        SwitchOn = True
        '        If MsgOutput Then WorkerMsg(tMsgID.Normal, "Advanced settings found.", MsgSrc)
        '    End If
        'End If

        '***
        '*** Second Line: Name/Identification of Components (Only Em. Power, Revolutions is fixed!)
        'line = file.ReadLine

        ''Column-count check
        's1 = UBound(line)
        s1 = 2

        ''Abort if less than 3 columns
        'If s1 < 2 Then GoTo lbEr

        ' ''Check whether Power/Revolutions swapped
        ''If UCase(line(0)).Contains("PE") Then
        ''    If MsgOutput Then WorkerMsg(tMsgID.Warn, "Expected Emission Map format: 1st column = Engine Speed, 2nd column = Engine Power (Header Check failed)", MsgSrc)
        ''End If

        ''Em-components initialize
        'For s = 3 To s1

        '    Em0 = New cEmComp
        '    Em0.Col = s
        '    Em0.Name = line(s)  'wird bei Default-Komponenten noch geändert
        '    Em0.IDstring = Trim(UCase(line(s)))
        '    Em0.MapCompID = fMapComp(Em0.Name)
        '    Em0.NormID = tEmNorm.x  'wird ggf. weiter unten korrigiert!
        '    'Default interpolator defined in Em0 = New cEmComp
        '    'Default Correction Pe defined in Em0 = New cEmComp

        '    If EmComponents.ContainsKey(UCase(Em0.Name)) Then

        '        'Abbruch falls schon definiert
        '        WorkerMsg(tMsgID.Err, "Component '" & Em0.Name & "' already defined! Col. " & s + 1, MsgSrc)
        '        GoTo lbEr

        '    Else

        '        'Dictionary .... fill
        '        If Em0.MapCompID = tMapComp.Undefined Then

        '            'ERROR when Component in angle brackets but unknown
        '            If Em0.IDstring.Length > 1 Then
        '                If Left(Em0.IDstring, 1) = "<" And Right(Em0.IDstring, 1) = ">" Then
        '                    If MsgOutput Then WorkerMsg(tMsgID.Err, "'" & Em0.Name & "' is no valid Default Map Component!", MsgSrc)
        '                End If
        '            End If

        '            'Custom Em-Components Dictionary:
        '            EmComponents.Add(Em0.IDstring, Em0)
        '            MyEmList.Add(Em0.IDstring)

        '        Else

        '            '*** Default Em components ***

        '            'Default-Interpolator
        '            Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)

        '            'Default Pe-Correction
        '            Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)

        '            'Default-Name
        '            Em0.Name = fMapCompName(Em0.MapCompID)

        '            'TC-components are not dumped
        '            If fMapCompIsTC(Em0.MapCompID) Then
        '                TransMap = True
        '                Em0.WriteOutput = False
        '            End If

        '            'Custom Em-Components Dictionary:
        '            EmComponents.Add(Em0.IDstring, Em0)
        '            MyEmList.Add(Em0.IDstring)

        '            'Entry in Reference-dictionary
        '            EmDefRef.Add(Em0.MapCompID, Em0)

        '        End If
        '    End If
        'Next

        'VECTO: Column 3 alwaysd consumption(Verbrauch)
        s = 2
        Em0 = New cEmComp
        Em0.Col = s
        Em0.Name = "FC"  'wird bei Default-Komponenten noch geändert
        Em0.Unit = "[g/h]"
        Em0.IDstring = "<FC>"
        Em0.MapCompID = tMapComp.FC
        Em0.NormID = tEmNorm.x_h
        If EmComponents.ContainsKey(UCase(Em0.Name)) Then
            'Abort if already defined
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
        '*** Read Normalized/Measured
        'line = file.ReadLine

        ''Abort when fewer columns than in the second Line
        'If UBound(line) < s1 Then GoTo lbEr

        ''Read Normalized/Measured
        'For Each EmKV In EmComponents

        '    'EM-component reference
        '    Em0 = EmKV.Value

        '    'Store Unit in String for further checks
        '    txt = Trim(line(Em0.Col))

        '    'Remove brackets
        '    txt = txt.Replace("[", "")
        '    txt = txt.Replace("]", "")

        '    'Normalize and set Unit
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

        ''Check whether n/Pe measured(Einheiten) OK:
        'If Not nNormed Then
        '    Select Case Trim(UCase(line(0)))
        '        Case "[U/MIN]", "RPM", "[1/MIN]", "[MIN^-1]"
        '            'Everything is okay
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
        '*** Line 4.5: (optional when "+"): Settings for Pe-Cor (old PfAK)
        '   If not "+", use default Interpolators (see above)
        If SwitchOn Then

            'Line 4 Reading
            line = file.ReadLine

            'Loop over Em-components
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


            'Line 5 Reading
            line = file.ReadLine

            'Loop over Em-components
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

        'From line 4 (or  6): Values
        Try
            Do While Not file.EndOfFile

                'Line read
                line = file.ReadLine

                'Line counter up (was reset in ResetMe)
                iMapDim += 1

                'Revolutions
                nU = CDbl(line(0))

                Lnn.Add(nU)

                'Power
                'If Trim(UCase(line(1))) = sKey.MAP.Drag Then
                '    If PeNormed Then
                '        LPe.Add(FLD.Pdrag(Lnn(iMapDim)) / VEH.Pnenn)
                '    Else
                '        LPe.Add(FLD.Pdrag(Lnn(iMapDim)))
                '    End If
                'Else
                LPe.Add(nMtoPe(nU, CDbl(line(1))))
                'End If

                'Emissions
                For Each EmKV In EmComponents
                    EmKV.Value.RawVals.Add(CSng(line(EmKV.Value.Col)))
                Next
            Loop
        Catch ex As Exception

            WorkerMsg(tMsgID.Err, "Error during file input! Line number " & iMapDim + 1 & " (" & sFilePath & ")", MsgSrc, sFilePath)
            GoTo lbEr

        End Try

        'Shep-Init
        MapIntp = New cMapInterpol(Me)

        'Close file
        file.Close()

        file = Nothing
        EmKV = Nothing
        Em0 = Nothing

        Return True


        'ERROR-label for clean Abort
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

        'Open file
        File = New cFile_V3
        If Not File.OpenRead(sFilePath, ",", True, True) Then
            File = Nothing
            Return False
        End If

        'Old maps have always TC-factors are (possibly  null)
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

        'Values
        Do While Not File.EndOfFile
            'Line read
            line = File.ReadLine

            'Line counter up (was reset in ResetMe)
            iMapDim += 1

            'Revolutions
            Lnn.Add(CSng(line(1)))

            'Power
            LPe.Add(CSng(line(0)))

            If iMapDim = 0 Then s1 = UBound(line)

            'Emissions
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

        'Close file
        File.Close()

        Return True

    End Function

    Public Function InitTCcomp(ByVal sK As String, ByVal TCvalues As System.Collections.Generic.Dictionary(Of tMapComp, Double)) As Boolean
        Dim KV As System.Collections.Generic.KeyValuePair(Of tMapComp, Double)
        Dim StringID As String
        Dim ID As tMapComp

        ID = fMapComp(sK)
        StringID = Trim(UCase(sK))

        'Abort when Em-component not in MAP
        If ID = tMapComp.Undefined Then
            If Not EmComponents.ContainsKey(StringID) Then Return False
        Else
            If Not EmDefRef.ContainsKey(ID) Then Return False
        End If

        'Abort if TC-factors for the component already defined
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

        'Speed Normalized
        If Not nNormed Then
            For i = 0 To iMapDim
                Lnn(i) = (Lnn(i) - nleerl) / (nnenn - nleerl)
            Next
        End If

        ' "otherwise calculate normalized Revolutions
        For i = 0 To iMapDim
            Ln.Add((Lnn(i) * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl) / VEH.nNenn)
        Next

        'Normalized Power
        If Not PeNormed Then
            For i = 0 To iMapDim
                LPe(i) /= Pnenn
            Next
        End If

        'Emissions unnormalised
        '   CAUTION: Even if x_kWh and x_hPnenn are to be transformed into x_h, the Normed must remain the same because otherwise the DynKor will not be right!
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
                    'Values are already specified in absolute
                    'Distinction between [x] and [x/h] currently not used/supported
            End Select
        Next

        'Check whether Revolutions/Power reversed
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
            WorkerMsg(tMsgID.Err, "Cannot extrapolate FC map! n= " & nnormTonU(nnorm).ToString("0") & " [1/min], Me= " & PnormToM(nnorm, Pnorm).ToString("0.0") & " [Nm]", "MAP/FC_Intp")
            Return -10000
        End Try
    End Function

    'Map creation
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
        '***************** Initialize Em-components *******************
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

            'PeCorMode: Falls nicht in MES/NPI vorgegeben gilt bei Nicht-Default-Em was in cEmComp.New() eingestellt ist |@@| PeCorMode: Unless specified in MES/NPI with non-default Em what is in cEmComp.New()
            If DRI.CreateMapParDef Then
                Em0.PeCorMode = DRI.MapPfak(EmKV.Key)
            ElseIf Em0.MapCompID <> tMapComp.Undefined Then
                Em0.PeCorMode = fDefPeCorMode(Em0.MapCompID)
            End If

            If Em0.MapCompID <> tMapComp.Undefined Then

                If fMapCompIsTC(Em0.MapCompID) Then

                    'If TC specified, then Abort
                    WorkerMsg(tMsgID.Err, "Component '" & Em0.Name & "' is invalid (Trans.Corr. Parameter)!", MsgSrc)
                    Return False

                Else

                    'Select interpolator
                    Em0.IntpolV2 = fDefIntpV2(Em0.MapCompID)

                    'Entry in Reference Dictionary (It does not check whether Em-Comp occurs twice since it has been caught in DRI.ReadFile)
                    EmDefRef.Add(Em0.MapCompID, Em0)

                End If

            End If

            EmComponents.Add(EmKV.Key, Em0)
            MyEmList.Add(EmKV.Key)

            'Dump Infos
            WorkerMsg(tMsgID.Normal, "   '" & Em0.Name & "': Unit = " & Em0.Unit & ", PeCorMode = " & fPwCorName(Em0.PeCorMode), MsgSrc)

        Next

        'Dynamic parameters and Extrapol to be added
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
        '************* Initialize Mean-value Dictionary ***************
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
        '********************* Calculate Mean-values **********************

        'Main-loop
        If GEN.iMsek > 1 Then

            'Create Lists
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

            'Loop over Measurement-values
            For i = 0 To MODdata.tDim

                If i + 1 < GEN.iMsek Then

                    'Fill the Area below iMsek with Measurement-values
                    pe(i) = MODdata.Pe(i)
                    nn(i) = MODdata.nn(i)

                    For Each EmKV In EmComponents
                        EmMW(EmKV.Key)(i) = DRI.EmComponents(EmKV.Key).RawVals(i)
                    Next

                    For Each Em0 In TCcomponents.Values
                        EmMW(Em0.Name)(i) = MODdata.TC.TCcomponents(Em0.MapCompID)(i)
                    Next

                Else

                    'Fill the Area above iMsek with the Mean-values of the Measurement-values
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

            'No averaging
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

        'EmCheck: False = Wert noch nicht verwendet, True = Wert wurde bereits in Kennfeld verwurschtet |@@| EmCheck: False = value is not used, True = Value is already(verwurschtet) in the Map
        For j = 0 To MODdata.tDim
            EmCheck.Add(False)
        Next

        '******************************************************************
        '*************************** Rasterung **************************** |@@| Grd-ing(Rasterung) ****************************

        dnn = 1.2 / (GEN.nschrit)
        dp = 1.4 / (GEN.Pschrit)

        nnx = 0 - dnn    'wird beim ersten Durchgang durch dn erhöht => Startwert = 0

        iMapDim = -1
        Do While nnx + dnn <= 1.20001     'nn-Schleife

            nnx += dnn

            pf = FLD(0).Pfull(nnx) / VEH.Pnenn
            pm = FLD(0).Pdrag(nnx) / VEH.Pnenn

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

        'Add Drag at half nn-increments. Needed for PfAK. If GEN.KFinsertDrag is disabled, it will be deleted later.
        nnx = 0 - dnn / 2
        Do While nnx + dnn / 2 <= 1.20001
            nnx += dnn / 2
            LPe.Add(FLD(0).Pdrag(nnx) / VEH.Pnenn)
            Lnn.Add(nnx)
            IsDrag.Add(True)
            iMapDim += 1
        Loop

        'Add Idle-point
        LPe.Add(0)
        Lnn.Add(0)
        IsDrag.Add(False)
        iMapDim += 1

        '******************************************************************
        '**************** Create NrUsed / Set to zero set *****************
        For j = 0 To MODdata.tDim
            NrUsed.Add(0)
            Used.Add(False)
        Next

        '******************************************************************
        '**************** Expect pure Measurement-values in Grid *****************

        'Basis Schrittweite zwischenspeichern |@@| Basis for step-size buffering
        dnn0 = dnn
        dp0 = dp

        'Loop over Grid-points(i)
        For i = 0 To iMapDim

            'Return Totals/Numbers/Flags
            Extra = False
            dnn = dnn0
            dp = dp0

            'Drag-Power
            pm = FLD(0).Pdrag(Lnn(i)) / VEH.Pnenn

            'Loop until enough Values found in Radius
            Do

                'Reset Counter/Totals
                anz = 0
                For Each EmMW0 In EmMW
                    EmSum(EmMW0.Key) = 0
                Next
                IntpPe = 0

                'Loop over Measured-values   ​​(j)
                For j = 0 To MODdata.tDim

                    'If within Radius ...
                    If Math.Abs(pe(j) - LPe(i)) <= dp / 2 And Math.Abs(nn(j) - Lnn(i)) <= dnn / 2 Then

                        'Num + 1
                        anz += 1

                        'Loop over all Em-comp.
                        For Each EmMW0 In EmMW

                            'Total +
                            EmSum(EmMW0.Key) += EmMW0.Value(j)

                            'Calculate Min/Max (for Log-output)
                            If anz = 1 Then
                                ValMin(EmMW0.Key) = EmMW0.Value(j)
                                ValMax(EmMW0.Key) = EmMW0.Value(j)
                            Else
                                ValMin(EmMW0.Key) = Math.Min(EmMW0.Value(j), ValMin(EmMW0.Key))
                                ValMax(EmMW0.Key) = Math.Max(EmMW0.Value(j), ValMax(EmMW0.Key))
                            End If

                        Next

                        'Interpolierte Leistung aufsummieren (wird dann durch Anz dividiert) |@@| Sum-up Interpolated Power (then divided by Num)
                        IntpPe += pe(j)

                        'Count how many Measurement-values exist for the Grid-points (Log-output)
                        Used(j) = True

                    Else

                        Used(j) = False

                    End If

                Next

                'If none Measured-value in Radius (Num = 0), then enlarge Radius and set Extra-flag
                If anz < 2 Then
                    Extra = True
                    dp *= 1.1
                    dnn *= 1.1
                End If

            Loop Until anz > 1

            'Allocate NrUsed
            For j = 0 To MODdata.tDim
                If Used(j) Then NrUsed(j) += 1
            Next

            'Interpolated-Power = Sum / number
            IntpPe /= anz

            'Calculate PfAK:
            '   If above Drag then PfAK according to Formula, or 1 when the difference between  Pe-Interpol and Drag-power is too low
            '   If below Drag, Pfak=0 => Em-value = Zero
            If LPe(i) > pm Then
                If (IntpPe - pm) > 0.05 Then
                    Pfak.Add(Math.Abs((LPe(i) - pm) / (IntpPe - pm)))
                Else
                    Pfak.Add(1)
                End If
            Else
                Pfak.Add(0)
            End If

            'Get the Extrapol flag from the Extrapol-column (1/0)
            If Extra Then
                EmExtra.RawVals.Add(1)
            Else
                EmExtra.RawVals.Add(0)
            End If

            'For Log-output
            KFanz.Add(anz)
            KFradproz.Add(dnn / dnn0)

            'Loop through Em-Comp (within Grid-points-loop)
            For Each EmKV In EmComponents

                'Falls Option 'Schlepp-Em aus .FLD' belegen und Rasterpunkt-Leistung <= Schleppleistung |@@| If Option 'Drag-Em' from .FLD 'and Power-gridpoints <= Drag-power
                If Not GEN.KFDragIntp And LPe(i) <= pm + 0.0001 Then

                    'If Drag-Em exists in .FLD, then use it otherwise alocate with zero
                    If FLD(0).EmDef(EmKV.Key) Then
                        EmKV.Value.RawVals.Add(FLD(0).EmDrag(EmKV.Key, Lnn(i)))
                    Else
                        EmKV.Value.RawVals.Add(0)
                    End If

                Else    'Option 'Schlepp-Em aus Messwerten' bzw. Punkte über Schleppleistung

                    'Em-allocation without PfAK (=> PfAK is crafted later)
                    EmKV.Value.RawVals.Add(CSng(EmSum(EmKV.Key) / anz))

                End If

                'For Log-output
                KFmin(EmKV.Key).Add(ValMin(EmKV.Key))
                KFmax(EmKV.Key).Add(ValMax(EmKV.Key))

            Next

            'Assume TC-factors without Pfak
            For Each Em0 In TCcomponents.Values
                Em0.RawVals.Add(CSng(EmSum(Em0.Name) / anz))
            Next

        Next

        '******************************************************************
        '*****************************  Pfak ******************************

        '!!! IMPORTANT !!!
        'Loop passes over all Grid-points (also for Pe <= PeDrag and  respectively for PeIntpol near Pdrag).
        '   That's OK because PfAK is in anyway allocated with 1s.

        'Loop through Em-Comp
        For Each EmKV In EmComponents

            'Falls keine Create Map Einstellungen (in .NPI/.MES) oder Pfak explizit aktiviert => Pfak verwenden |@@| If no Create Map is set (in .NPI/.MES) or PfAK activated explicitly => Use PfAK
            If EmKV.Value.PeCorMode <> tIntpPeCorMode.PeCorOff Then

                'Loop over Grid-points (i)
                For i = 0 To iMapDim

                    If EmKV.Value.PeCorMode = tIntpPeCorMode.PeCorNull Then

                        'Altes Pfak mit Extrapolation von Null weg |@@| Old PfAK with Extrapolation from Zero route?
                        EmDrag = 0

                    Else    'PeCorMode = tIntpPeCorMode.PeCorEmDrag

                        'Schlepp-Emission raus suchen |@@| Pick Drag-Emission
                        '   Schlepp-Em aus nächstgelegenen Rasterpunkt nehmen. Das geht weil Schleppkurve  |@@| Take the Drag-Em from the nearest Grid-point. This is because the Drag-curve
                        '   immer ins Map kommt (auch wenn sie dann später raus gelöscht wird) !! |@@| always comes into the Map (even if it is later deleted) !!
                        '   Option 'Schlepp-Em aus .FLD' spielt keine Rolle weil das nur die Belegungs-Methode der Schleppkurve betrifft (s.o.)  |@@| Option 'Drag-Em(Schlepp-Em)' from .FLD plays no role because it affects only the Allocation-method of the Drag-curve (see above)
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

                    'Apply PfAK
                    EmKV.Value.RawVals(i) = CSng(Pfak(i) * (EmKV.Value.RawVals(i) - EmDrag) + EmDrag)

                Next

            End If

        Next

        '******************************************************************
        '******************* Normalize (Value and Unit) ********************
        For Each EmKV In EmComponents

            'Use them If specified in MES/NPI-files
            If DRI.CreateMapParDef Then

                If DRI.MapUnitsNormed(EmKV.Key) Then

                    'Values normalized
                    For i = 0 To iMapDim
                        EmKV.Value.RawVals(i) /= VEH.Pnenn
                    Next

                    EmKV.Value.NormID = tEmNorm.x_hPnenn

                End If

            Else

                'Otherwise, use a standard normalization
                If EmKV.Value.MapCompID <> tMapComp.Undefined Then

                    If fDefEmNormID(EmKV.Value.MapCompID) = tEmNorm.x_hPnenn Then

                        'Normalized Values
                        For i = 0 To iMapDim
                            EmKV.Value.RawVals(i) /= VEH.Pnenn
                        Next

                        EmKV.Value.NormID = tEmNorm.x_hPnenn

                    End If

                End If

            End If

        Next

        '******************************************************************
        '****************** EmComponents zusammenfassen ******************* |@@| Summarized EmComponents *******************
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
        '*********** Schleppkurve wieder raus nehmen (optional) *********** |@@| Get Load-curve again (optional) ***********
        If Not GEN.KFinsertDrag Then

            'Schleife über Rasterpunkte (i). Keine For-Schleife weil iMapDim reduziert wird |@@| Loop over Grid-points(i). No For-loop because iMapDim is reduced
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
        '************************** Dump Map '**************************

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
            'CAUTION: Not Name but sKey !!!
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

        'Values
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
        '********************** Dump Extended-Info '***********************

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

        'Values
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

    'Default Shepard in intpshep ()
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
            PeDrag = FLD(0).Pdrag(nnorm) / VEH.Pnenn       'Schleppkurve für die geg. Drehzahl

            ReDim ab0(iMapDim)


            '*****************************************************************************
            '*****************************************************************************
            '***********************************  V1  ************************************
            '*****************************************************************************
            '*****************************************************************************

            ReDim abOK(iMapDim)

            'Calculate Distance and Correction.
            For i = 0 To iMapDim
                If (Pnorm < 0.05 And Pnorm >= 0.0) Then

                    'Um Nullleistung werden Kennfeldpunkte um Pe=0 hoeher gewichtet und Drehzahlabstand geringer, |@@| ????The Map-points with zero-Power (Pe=0) will be weighted higher and Revolution-distances lower,
                    ' da Interpolation aus Punkten mit hoeherer last dort schlecht passt: |@@| because interpolation of Points with higher Load is fitted badly:
                    ab0(i) = (Pnorm - myMap.LPe(i)) ^ 2 + (n0 - myMap.Ln(i)) ^ 2 * 888.9 * (Math.Abs(Pnorm) ^ 3 + 0.001)
                    ab0(i) = ab0(i) * (Math.Abs(Pnorm) + Math.Abs(myMap.LPe(i)) + 0.005) * 9.52

                Else

                    'Square of the distance:
                    ab0(i) = (Pnorm - myMap.LPe(i)) ^ 2 + (n0 - myMap.Ln(i)) ^ 2

                    'Falls Vorzeichen von Pe unterschiedlich (Last/Schlepp-Trennung) wird Abstand vergroessert um Schlepp bei Schlepp mehr zu gewichten: |@@| ????If the Sign of Pe unequal (Load /Drag separation), then Distance increases more weight to Drag by Drag:
                    If Pnorm * myMap.LPe(i) < 0 Then ab0(i) *= 50

                End If

            Next

            'Punkte innerhalb Radius zählen und ggf. Radius erhöhen |@@| Points are within radius and possibly within  a bigger Radius
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

            'Abstand-Array erstellen und Leistung interpolieren für Pe-Korrektur |@@| Distance array and create Power interpolate for Pe-correction
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

            'Calculation of Wisum
            wisum = 0
            For i = 0 To inim - 1
                wisum = wisum + 1.0 / ab(i)
            Next

            'Calcluate Interpolated Power
            PeIntp = sumo / wisum


            '*****************************************************************************
            '*****************************************************************************
            '***********************************  V2  ************************************
            '*****************************************************************************
            '*****************************************************************************

            ReDim abOKV2(iMapDim)

            'Calculate Distance and Correction.
            For i = 0 To iMapDim

                'Square of the distance:
                ab0(i) = (Pnorm - myMap.LPe(i)) ^ 2 + (nnorm - myMap.Lnn(i)) ^ 2

            Next

            'Punkte innerhalb Radius zählen und ggf. Radius erhöhen |@@| Points are within radius and possibly within  a bigger Radius
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

            'Abstand-Array erstellen und Leistung interpolieren für Pe-Korrektur |@@| Distance array and create Power interpolate for Pe-correction
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

            'Calculation of wisumV2
            wisumV2 = 0
            For i = 0 To inim - 1
                wisumV2 = wisumV2 + 1.0 / abV2(i)
            Next

            'Calculate Interpolated Power
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

        'Berechnet Emission an Schleppkurve |@@| Calculated Emission on Drag-curve
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

            'Es wird an Schleppkurve gesucht |@@| Search on Drag-curve
            Pnorm = PeDrag
            'n0 has already been defined in Init

            'Calculate Distance and Correction.
            For i = 0 To iMapDim

                'Square of the Distances:
                ab0(i) = (Pnorm - myMap.LPe(i)) ^ 2 + (n0 - myMap.Ln(i)) ^ 2

                'Falls Vorzeichen von Pe unterschiedlich (Last/Schlepp-Trennung) wird Abstand vergroessert um Schlepp bei Schlepp mehr zu gewichten: |@@| ????If the Sign of Pe unequal (Load /Drag separation), then Distance increases more weight to Drag by Drag:
                If Not EmComp.IntpolV2 Then
                    If Pnorm * myMap.LPe(i) < 0 Then ab0(i) *= 50
                End If

            Next

            'Punkte innerhalb Radius zählen und ggf. Radius erhöhen |@@| Points are within radius and possibly within a  bigger Radius
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

            'Create Distances-array
            ReDim ab_loc(inim - 1)
            i0 = -1
            For i = 0 To iMapDim
                If abOK_loc(i) Then
                    i0 += 1
                    ab_loc(i0) = Math.Max(ab0(i), 0.00000001)
                End If
            Next

            'Calculation of wisum
            wisum_loc = 0
            For i = 0 To inim - 1
                wisum_loc = wisum_loc + 1.0 / ab_loc(i)
            Next

            'Calculate emission
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








