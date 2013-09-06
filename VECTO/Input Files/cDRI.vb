Imports System.Collections.Generic

Public Class cDRI

    'Private Const FormatVersion As Integer = 1
    'Private FileVersion As Integer
    Public tDim As Integer

    Public Values As Dictionary(Of tDriComp, List(Of Double))
    Public t0 As Integer

    Private sFilePath As String

    Public Tvorg As Boolean
    Public Vvorg As Boolean
    Public Pvorg As Boolean
    Public PaddVorg As Boolean
    Public Nvorg As Boolean
    Public Gvorg As Boolean
    Public GradVorg As Boolean

    Public PeNormed As Boolean
    Public nNormed As Boolean
    Public PaddNormed As Boolean

    Private bEmCompDef As Boolean
    Public EmComponents As Dictionary(Of String, cEmComp)
    Public EmDefRef As Dictionary(Of tMapComp, cEmComp)

    'Defaults(Vorgabe) for EXS
    Private bExsCompDef As Boolean
    Public ExsComponents As Dictionary(Of tExsComp, Dictionary(Of Short, List(Of Single)))

    'Parameters for KF-creation
    Public MapUnitsNormed As Dictionary(Of String, Boolean)
    Public MapPfak As Dictionary(Of String, tIntpPeCorMode)
    Private bCreateMapParDef As Boolean

    'Defaults(Vorgabe) for AUX
    Private bAuxDef As Boolean
    Public AuxComponents As Dictionary(Of String, List(Of Single))

    Public VairVorg As Boolean

    Public Scycle As Boolean
    Public VoglS As List(Of Double)

    Public Sub New()
        EmComponents = New Dictionary(Of String, cEmComp)
        EmDefRef = New Dictionary(Of tMapComp, cEmComp)
    End Sub

    Private Sub ResetMe()
        Values = Nothing
        PaddVorg = False
        Tvorg = False
        Vvorg = False
        GradVorg = False
        Nvorg = False
        Gvorg = False
        Pvorg = False
        PeNormed = False
        nNormed = False
        PaddNormed = False
        tDim = -1
        t0 = 1  'Ist Standardwert falls Converter nicht verwendet wird
        EmComponents.Clear()
        bEmCompDef = False
        EmDefRef.Clear()
        MapUnitsNormed = Nothing
        MapPfak = Nothing
        bExsCompDef = False
        ExsComponents = Nothing
        bCreateMapParDef = False
        bAuxDef = False
        AuxComponents = Nothing
        VairVorg = False
        Scycle = False
    End Sub

    Public Sub ADVinit()
        ResetMe()
        Tvorg = True
        Vvorg = True
        GradVorg = True
        Values = New Dictionary(Of tDriComp, List(Of Double))
        'Values.Add(tDriComp.t, New List (Of Single))             '<= Needed only if ADVANCE > 1 Hz supported
        Values.Add(tDriComp.V, New List(Of Double))
        Values.Add(tDriComp.Grad, New List(Of Double))
    End Sub

    Public Function ReadFile() As Boolean
        Dim file As cFile_V3
        Dim line As String()
        Dim s1 As Integer
        Dim s As Integer
        Dim txt As String
        Dim Comp As tDriComp
        Dim ExsComp As tExsComp
        Dim AuxComp As tAuxComp
        Dim AuxID As String
        Dim MapComp As tMapComp
        Dim Em0 As cEmComp
        Dim ModNr As Short
        Dim Svorg As Boolean = False

        Dim DRIcheck As Dictionary(Of tDriComp, Boolean)
        Dim Spalten As Dictionary(Of tDriComp, Integer)
        Dim sKV As KeyValuePair(Of tDriComp, Integer)

        Dim ExsSpalten As Dictionary(Of Integer, List(Of Single)) = Nothing
        Dim ExsKV As KeyValuePair(Of Integer, List(Of Single))

        Dim AuxSpalten As Dictionary(Of String, Integer) = Nothing
        Dim Mvorg As Boolean = False


        Dim MsgSrc As String



        MsgSrc = "Main/ReadInp/DRI"

        bCreateMapParDef = False

        'Reset
        ResetMe()

        'Abort if there's no file
        If sFilePath = "" OrElse Not IO.File.Exists(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Cycle file not found (" & sFilePath & ") !", MsgSrc)
            Return False
        End If

        'EmComp Init
        '...now in New()

        'Open file
        file = New cFile_V3
        If Not file.OpenRead(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Failed to open file (" & sFilePath & ") !", MsgSrc)
            file = Nothing
            Return False
        End If

        DRIcheck = New Dictionary(Of tDriComp, Boolean)
        DRIcheck.Add(tDriComp.t, False)
        DRIcheck.Add(tDriComp.V, False)
        DRIcheck.Add(tDriComp.Grad, False)
        DRIcheck.Add(tDriComp.nn, False)
        DRIcheck.Add(tDriComp.Gears, False)
        DRIcheck.Add(tDriComp.Padd, False)
        DRIcheck.Add(tDriComp.Pe, False)
        DRIcheck.Add(tDriComp.VairVres, False)
        DRIcheck.Add(tDriComp.VairBeta, False)
        DRIcheck.Add(tDriComp.s, False)
        DRIcheck.Add(tDriComp.StopTime, False)
        DRIcheck.Add(tDriComp.Torque, False)

        ''***
        ''*** First line: Version
        'line = file.ReadLine
        'txt = Trim(UCase(line(0)))
        'If Microsoft.VisualBasic.Left(txt, 1) = "V" Then
        '    ' "V" entfernen => Zahl bleibt übrig
        '    txt = txt.Replace("V", "")
        '    If Not IsNumeric(txt) Then
        '        'If invalid Version: Abort
        '        WorkerMsg(tMsgID.Err, "File Version invalid!", MsgSrc)
        '        GoTo lbEr
        '    Else
        '        'Version specified
        '        FileVersion = CInt(txt)
        '    End If
        'Else
        '    'If no version information: Old Format
        '    file.Close()
        '    Return ReadOldFormat()
        'End If

        ''Version Check: Abort if input file format is newer than PHEM-version
        'If FileVersion > FormatVersion Then
        '    WorkerMsg(tMsgID.Err, "File Version not supported!", MsgSrc)
        '    GoTo lbEr
        'End If

        ''Column 2: added option "+" = parameter for KF-creation
        'If UBound(line) > 0 Then
        '    If Trim(line(1)) = "+" Then
        '        bCreateMapParDef = True
        '        WorkerMsg(tMsgID.Normal, "MAP-Creation settings found.", MsgSrc)
        '    End If
        'End If

        If file.EndOfFile Then
            WorkerMsg(tMsgID.Err, "Driving cycle invalid!", MsgSrc)
            Return False
        End If

        Spalten = New Dictionary(Of tDriComp, Integer)
        Values = New Dictionary(Of tDriComp, List(Of Double))

        '***
        '*** Second row: Name/Identification of the Components
        line = file.ReadLine

        'Check Number of Columns/Components
        s1 = UBound(line)

        For s = 0 To s1

            Comp = fDriComp(line(s))

            'Falls DRIcomp = Undefined dann wirds als EXS-Comp oder als Emission für KF-Erstellung / Eng-Analysis verwendet |@@| If used DRIcomp = Undefined it will get as EXS-Comp or Emission for KF-Creation / Eng-Analysis
            If Comp = tDriComp.Undefined Then

                ExsComp = fExsComp(line(s))

                If ExsComp = tExsComp.Undefined Then

                    AuxComp = fAuxComp(line(s))

                    If AuxComp = tAuxComp.Undefined Then

                        MapComp = fMapComp(line(s))

                        txt = UCase(Trim(line(s)))

                        If EmComponents.ContainsKey(txt) Then
                            WorkerMsg(tMsgID.Err, "Multiple definitions of '" & line(s) & "'! Column " & s + 1, MsgSrc)
                            GoTo lbEr
                        End If

                        Em0 = New cEmComp
                        Em0.Col = s
                        Em0.Name = line(s)  'wird bei Def-Komp noch geändert
                        Em0.IDstring = txt
                        Em0.MapCompID = MapComp

                        If MapComp <> tMapComp.Undefined Then
                            Em0.Name = fMapCompName(MapComp)
                            EmDefRef.Add(MapComp, Em0)
                        End If

                        EmComponents.Add(Em0.IDstring, Em0)

                        bEmCompDef = True

                        'ERROR when component in angle brackets is unknown
                        If MapComp = tMapComp.Undefined And Em0.IDstring.Length > 1 Then
                            If Left(Em0.IDstring, 1) = "<" And Right(Em0.IDstring, 1) = ">" Then
                                WorkerMsg(tMsgID.Err, "'" & Em0.Name & "' is no valid Default Map, Cycle or EXS Component!", MsgSrc)
                            End If
                        End If

                    Else

                        txt = fCompSubStr(line(s))

                        If Not bAuxDef Then
                            AuxComponents = New Dictionary(Of String, List(Of Single))
                            AuxSpalten = New Dictionary(Of String, Integer)
                        End If

                        If AuxComponents.ContainsKey(txt) Then
                            WorkerMsg(tMsgID.Err, "Multiple definitions of auxiliary '" & txt & "' in driving cycle! Column " & s + 1, MsgSrc)
                            GoTo lbEr
                        End If

                        AuxComponents.Add(txt, New List(Of Single))
                        AuxSpalten.Add(txt, s)

                        bAuxDef = True

                    End If

                Else

                    'if first EXS-column, then create Dictionary
                    If Not bExsCompDef Then
                        ExsSpalten = New Dictionary(Of Integer, List(Of Single))
                        ExsComponents = New Dictionary(Of tExsComp, Dictionary(Of Short, List(Of Single)))
                    End If

                    'If EXS-Component not yet in Dictionary, create
                    If Not ExsComponents.ContainsKey(ExsComp) Then ExsComponents.Add(ExsComp, New Dictionary(Of Short, List(Of Single)))

                    txt = fCompSubStr(line(s))

                    If Not IsNumeric(txt) Then
                        WorkerMsg(tMsgID.Err, "Component ID String '" & line(s) & "' is invalid! Column " & s + 1, MsgSrc)
                        GoTo lbEr
                    Else
                        ModNr = CShort(txt)
                    End If

                    'Check whether ExsComp/Module-combination already exists => ERROR
                    If ExsComponents(ExsComp).ContainsKey(ModNr) Then
                        WorkerMsg(tMsgID.Err, "Component '" & line(s) & "' already defined! Column " & s + 1, MsgSrc)
                        GoTo lbEr
                    End If

                    ExsComponents(ExsComp).Add(ModNr, New List(Of Single))
                    ExsSpalten.Add(s, ExsComponents(ExsComp)(ModNr))

                    bExsCompDef = True

                End If

            Else

                If DRIcheck(Comp) Then
                    WorkerMsg(tMsgID.Err, "Component '" & line(s) & "' already defined! Column " & s + 1, MsgSrc)
                    GoTo lbEr
                End If

                DRIcheck(Comp) = True
                Spalten.Add(Comp, s)
                Values.Add(Comp, New List(Of Double))

            End If

        Next

        'Set Gvorg/Nvorg:
        Tvorg = DRIcheck(tDriComp.t)
        Vvorg = DRIcheck(tDriComp.V)
        Svorg = DRIcheck(tDriComp.s)
        Gvorg = DRIcheck(tDriComp.Gears)
        Nvorg = DRIcheck(tDriComp.nn)
        Pvorg = DRIcheck(tDriComp.Pe)
        PaddVorg = DRIcheck(tDriComp.Padd)
        GradVorg = DRIcheck(tDriComp.Grad)
        VairVorg = DRIcheck(tDriComp.VairVres) And DRIcheck(tDriComp.VairBeta)
        Mvorg = DRIcheck(tDriComp.Torque)

        If Mvorg And Pvorg Then
            WorkerMsg(tMsgID.Warn, "Engine torque and power defined in cycle! Torque will be ignored!", MsgSrc)
            Mvorg = False
        End If

        '***
        '*** Third row: Units/Normalization
        'VECTO: nothing read. Fixed Units (line = file.ReadLine)

        'Normalization-compatible DRI-components
        If DRIcheck(tDriComp.Pe) Then
            'PeNormed = (UCase(Trim(line(Spalten(tDriComp.Pe)))) = sKey.Normed)
            PeNormed = False
        End If

        If DRIcheck(tDriComp.nn) Then
            'nNormed = (UCase(Trim(line(Spalten(tDriComp.nn)))) = sKey.Normed)
            nNormed = False
        End If

        If DRIcheck(tDriComp.Padd) Then
            'PaddNormed = (UCase(Trim(line(Spalten(tDriComp.Padd)))) = sKey.Normed)
            PaddNormed = False
        End If

        'VECTO MAP-components: Always [g/h]!
        For Each Em0 In EmComponents.Values

            ''Store Unit in String for further checks
            'txt = Trim(line(Em0.Col))

            ''Remove brackets
            'txt = txt.Replace("[", "")
            'txt = txt.Replace("]", "")

            ''Set Scaling and Unit
            'If txt.Contains("/") Then


            '    Select Case UCase(Right(txt, txt.Length - txt.IndexOf("/") - 1))
            '        Case "KWH", "H" & sKey.Normed
            '            WorkerMsg(tMsgID.Warn, "Unit of component " & line(s) & " is not valid! Check Output!", MsgSrc)
            '            Em0.NormID = tEmNorm.x
            '            Em0.Unit = "Unit-ERROR!"

            '        Case "H"
            '            Em0.NormID = tEmNorm.x_h
            '            Em0.Unit = "[" & Left(txt, txt.IndexOf("/")) & "/h]"

            '        Case Else
            '            Em0.NormID = tEmNorm.x
            '            Em0.Unit = "[" & txt & "]"

            '    End Select

            'Else
            '    Em0.NormID = tEmNorm.x
            '    Em0.Unit = "[" & txt & "]"
            'End If

            Em0.NormID = tEmNorm.x_h
            Em0.Unit = "[g/h]"

        Next

        '***
        '*** Line 4, 5: (optional when "+"): Settings for KF-creation

        'If "+" enabled
        If bCreateMapParDef Then

            'Creating instances
            MapUnitsNormed = New Dictionary(Of String, Boolean)
            MapPfak = New Dictionary(Of String, tIntpPeCorMode)

            '1. Option "Map normalized by Pnom"
            line = file.ReadLine
            For Each Em0 In EmComponents.Values
                MapUnitsNormed.Add(Em0.IDstring, CBool(line(Em0.Col)))
            Next

            '2. Option "PfAK apply"
            line = file.ReadLine
            For Each Em0 In EmComponents.Values

                Select Case CShort(line(Em0.Col))
                    Case 0
                        MapPfak.Add(Em0.IDstring, tIntpPeCorMode.PeCorOff)
                    Case 1
                        MapPfak.Add(Em0.IDstring, tIntpPeCorMode.PeCorNull)
                    Case 2
                        MapPfak.Add(Em0.IDstring, tIntpPeCorMode.PeCorEmDrag)
                    Case Else
                        WorkerMsg(tMsgID.Err, "Power Correction Mode Nr. " & line(Em0.Col) & " is invalid!", MsgSrc)
                        GoTo lbEr
                End Select

            Next

        End If

        '***
        '*** Ab 4.Zeile bzw. Ab 6.Zeile: Werte (derzeit keine unterschiedlichen Einheiten/Normierungen unterstützt) |@@| From 4th line or From 6th line: values (no different units/normalizations support)
        Try
            Do While Not file.EndOfFile
                tDim += 1       'wird in ResetMe zurück gesetzt
                line = file.ReadLine

                For Each sKV In Spalten

                    If sKV.Key = tDriComp.Pe Or sKV.Key = tDriComp.Torque Then
                        If Trim(line(sKV.Value)) = sKey.MAP.Drag Then line(sKV.Value) = -999999
                    End If

                    Values(sKV.Key).Add(CDbl(line(sKV.Value)))
                Next

                For Each Em0 In EmComponents.Values
                    Em0.RawVals.Add(CSng(line(Em0.Col)))
                Next

                If bExsCompDef Then
                    For Each ExsKV In ExsSpalten
                        ExsKV.Value.Add(CSng(line(ExsKV.Key)))
                    Next
                End If

                If bAuxDef Then
                    For Each AuxID In AuxSpalten.Keys
                        AuxComponents(AuxID).Add(CSng(line(AuxSpalten(AuxID))))
                    Next
                End If

            Loop
        Catch ex As Exception

            WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & tDim + 1 & " (" & sFilePath & ")", MsgSrc, sFilePath)
            GoTo lbEr

        End Try

        file.Close()

        Scycle = (Svorg And Not Tvorg)

        If Vvorg Then
            For s = 0 To tDim
                If Values(tDriComp.V)(s) < 0.09 Then Values(tDriComp.V)(s) = 0
            Next
        End If

        If Mvorg And Nvorg Then
            Values.Add(tDriComp.Pe, New List(Of Double))
            Pvorg = True
            For s = 0 To tDim
                Values(tDriComp.Pe).Add(nMtoPe(Values(tDriComp.nn)(s), Values(tDriComp.Torque)(s)))
            Next
        End If

        Return True

lbEr:
        file.Close()

        Return False

    End Function

    Private Function ReadOldFormat() As Boolean

        Dim File As cFile_V3
        Dim line As String()
        Dim s1 As Integer
        Dim t As Integer
        Dim GNok As Boolean
        Dim STGok As Boolean
        Dim GNmax As Single
        Dim GNlist As List(Of Double)
        Dim MsgSrc As String

        MsgSrc = "Main/ReadInp/DRI"

        'Open file
        File = New cFile_V3
        If Not File.OpenRead(sFilePath, ",", True, True) Then
            File = Nothing
            Return False
        End If

        Values = New Dictionary(Of tDriComp, List(Of Double))
        GNlist = New List(Of Double)

        Do While Not File.EndOfFile
            tDim += 1       'wird in ResetMe zurück gesetzt
            line = File.ReadLine

            If tDim = 0 Then

                s1 = UBound(line)

                GNok = (s1 > 2)
                PaddVorg = (s1 > 3)

                If s1 > 1 Then
                    STGok = True
                    GradVorg = True
                    Values.Add(tDriComp.Grad, New List(Of Double))
                Else
                    If s1 < 1 Then
                        'TODO...
                        Return False
                    End If
                    STGok = False
                End If

                Tvorg = True
                Values.Add(tDriComp.t, New List(Of Double))

                Vvorg = True
                Values.Add(tDriComp.V, New List(Of Double))

                If PaddVorg Then
                    PaddNormed = True
                    Values.Add(tDriComp.Padd, New List(Of Double))
                End If

            End If

            Try
                Values(tDriComp.t).Add(CDbl(line(0)))
                Values(tDriComp.V).Add(CDbl(line(1)))
                If STGok Then Values(tDriComp.Grad).Add(CDbl(line(2)))
                If GNok Then GNlist.Add(CDbl(line(3)))
                If PaddVorg Then Values(tDriComp.Padd).Add(CDbl(line(4)))
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & tDim + 1 & " (" & sFilePath & ")", MsgSrc, sFilePath)
                Return False
            End Try


        Loop

        File.Close()

        'ResetMe resets Nvorg / Gvorg

        If GNok Then

            GNmax = GNlist(0)
            For t = 1 To tDim
                If GNlist(t) > GNmax Then GNmax = GNlist(t)
            Next

            If GNmax > 50 Then
                Nvorg = True
                nNormed = False
                Values.Add(tDriComp.nn, GNlist)

            ElseIf GNmax > 0 Then
                Gvorg = True
                Values.Add(tDriComp.Gears, GNlist)

            End If

        End If

        File = Nothing
        GNlist = Nothing

        Return True

    End Function

    Public Function ExsCompDef() As Boolean
        Return bExsCompDef
    End Function

    Public Function ExsCompDef(ByVal ExsComp As tExsComp, Optional ByVal ModNr As Short = -1) As Boolean

        If bExsCompDef Then
            If ExsComponents.ContainsKey(ExsComp) Then
                If ModNr = -1 Then
                    Return True
                Else
                    Return ExsComponents(ExsComp).ContainsKey(ModNr)
                End If
            Else
                Return False
            End If
        Else
            Return False
        End If

    End Function

    Public Sub DeNorm()
        Dim s As Integer
        Dim L As List(Of Double)

        'Convert Speed to m/s
        If Vvorg Then
            For s = 0 To tDim
                Values(tDriComp.V)(s) /= 3.6
                If Values(tDriComp.V)(s) < 0 Then Values(tDriComp.V)(s) = 0
            Next
        End If

        'Normalize, if necessary
        If Nvorg Then
            If Not nNormed Then
                L = Values(tDriComp.nn)
                For s = 0 To tDim
                    L(s) = (L(s) - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)
                Next
            End If
        End If

        'Padd unnormalised, if neccesary
        If PaddVorg Then
            If PaddNormed Then
                L = Values(tDriComp.Padd)
                For s = 0 To tDim
                    L(s) = L(s) * VEH.Pnenn
                Next
            End If
        End If

        'Pe normalize, if necessary
        If Pvorg Then
            If Not PeNormed Then
                L = Values(tDriComp.Pe)
                For s = 0 To tDim
                    L(s) = L(s) / VEH.Pnenn
                Next
            End If
        End If

        L = Nothing

        '!!!!!!!! Emissions are only accepted in x/h or x (see ReadFile)!!!!!!!!

    End Sub

    Public Function ConvStoT() As Boolean
        Dim i As Integer
        Dim j As Integer
        Dim ds As Double
        Dim vm As Double
        Dim a As Double
        Dim am As Double
        Dim dv As Double
        Dim s As Double
        Dim t As Double
        Dim dt As Double
        Dim vstep As Double
        Dim Dist As List(Of Double)
        Dim Speed As List(Of Double)
        Dim Grad As List(Of Double)
        Dim StopTime As List(Of Double)
        Dim Time As New List(Of Double)
        Dim tValues As New Dictionary(Of tDriComp, List(Of Double))
        Dim tDist As New List(Of Double)
        Dim hzTime As New List(Of Double)
        Dim hzDist As New List(Of Double)
        Dim hzValues As New Dictionary(Of tDriComp, List(Of Double))
        Dim ValKV As KeyValuePair(Of tDriComp, List(Of Double))
        Dim tmax As Integer

        Dim tExsValues As Dictionary(Of tExsComp, Dictionary(Of Short, List(Of Single))) = Nothing
        Dim hzExsValues As Dictionary(Of tExsComp, Dictionary(Of Short, List(Of Single))) = Nothing
        Dim ExsKV As KeyValuePair(Of tExsComp, Dictionary(Of Short, List(Of Single)))
        Dim ExsKVsub As KeyValuePair(Of Short, List(Of Single))

        Dim tAuxValues As Dictionary(Of String, List(Of Single)) = Nothing
        Dim hzAuxValues As Dictionary(Of String, List(Of Single)) = Nothing
        Dim AuxKV As KeyValuePair(Of String, List(Of Single))

        Dim SpeedOgl As New List(Of Double)
        Dim tSpeedOgl As New List(Of Double)
        Dim hzSpeedOgl As New List(Of Double)

        Dim StopTimeError As Boolean = False

        Dim MsgSrc As String

        MsgSrc = "Main/DRI/ConvStoT"

        If Not GEN.DesMaxJa Then
            WorkerMsg(tMsgID.Err, "No a(v) data defined!", MsgSrc)
            Return False
        End If

        If Not Values.ContainsKey(tDriComp.StopTime) Then
            WorkerMsg(tMsgID.Err, "Stop time not defined in cycle (" & sKey.DRI.StopTime & ")!", MsgSrc)
            Return False
        End If

        If Not Values.ContainsKey(tDriComp.V) Then
            WorkerMsg(tMsgID.Err, "Vehicle speed not defined in cycle (" & sKey.DRI.V & ")!", MsgSrc)
            Return False
        End If

        Dist = Values(tDriComp.s)
        Speed = New List(Of Double)
        For i = 0 To tDim
            Speed.Add(Values(tDriComp.V)(i) / 3.6)
            SpeedOgl.Add(Values(tDriComp.V)(i) / 3.6)
        Next

        StopTime = Values(tDriComp.StopTime)
        vstep = 0.001

        If Values.ContainsKey(tDriComp.Grad) Then
            Grad = Values(tDriComp.Grad)
        Else
            Grad = New List(Of Double)
            For i = 0 To tDim
                Grad.Add(0)
            Next
            Values.Add(tDriComp.Grad, Grad)
        End If

        For Each ValKV In Values
            If ValKV.Key <> tDriComp.s And ValKV.Key <> tDriComp.StopTime Then
                tValues.Add(ValKV.Key, New List(Of Double))
                hzValues.Add(ValKV.Key, New List(Of Double))
            End If
        Next

        If bExsCompDef Then
            tExsValues = New Dictionary(Of tExsComp, Dictionary(Of Short, List(Of Single)))
            hzExsValues = New Dictionary(Of tExsComp, Dictionary(Of Short, List(Of Single)))
            For Each ExsKV In ExsComponents
                tExsValues.Add(ExsKV.Key, New Dictionary(Of Short, List(Of Single)))
                hzExsValues.Add(ExsKV.Key, New Dictionary(Of Short, List(Of Single)))
                For Each ExsKVsub In ExsKV.Value
                    tExsValues(ExsKV.Key).Add(ExsKVsub.Key, New List(Of Single))
                    hzExsValues(ExsKV.Key).Add(ExsKVsub.Key, New List(Of Single))
                Next
            Next
        End If

        If bAuxDef Then
            tAuxValues = New Dictionary(Of String, List(Of Single))
            hzAuxValues = New Dictionary(Of String, List(Of Single))
            For Each AuxKV In AuxComponents
                tAuxValues.Add(AuxKV.Key, New List(Of Single))
                hzAuxValues.Add(AuxKV.Key, New List(Of Single))
            Next
        End If

        '*********************************** Deceleration(Verzögerung) limit ********************************
        For i = tDim To 1 Step -1

            ds = Dist(i) - Dist(i - 1)

            vm = (Speed(i) + Speed(i - 1)) / 2
            dv = Speed(i) - Speed(i - 1)

            a = vm * dv / ds

            am = GEN.aDesMin(vm)

            Do While a < am

                Speed(i - 1) -= vstep

                vm = (Speed(i) + Speed(i - 1)) / 2
                dv = Speed(i) - Speed(i - 1)

                a = vm * dv / ds

                am = GEN.aDesMin(vm)

            Loop

        Next


        '*********************************** Create Time-sequence '***********************************
        t = 0
        s = Dist(0)

        Time.Add(t)
        For Each ValKV In tValues
            If ValKV.Key <> tDriComp.V Then tValues(ValKV.Key).Add(Values(ValKV.Key)(0))
        Next
        tValues(tDriComp.V).Add(Speed(0) * 3.6)
        tSpeedOgl.Add(SpeedOgl(0) * 3.6)
        tDist.Add(s)
        If bExsCompDef Then
            For Each ExsKV In ExsComponents
                For Each ExsKVsub In ExsKV.Value
                    tExsValues(ExsKV.Key)(ExsKVsub.Key).Add(ExsKVsub.Value(0))
                Next
            Next
        End If
        If bAuxDef Then
            For Each AuxKV In AuxComponents
                tAuxValues(AuxKV.Key).Add(AuxKV.Value(0))
            Next
        End If


        If Speed(0) < 0.0001 Then

            If StopTime(0) = 0 Then
                WorkerMsg(tMsgID.Err, "Stop time = 0 at cylce start!", MsgSrc)
                StopTimeError = True
            End If

            t += StopTime(0)
            Time.Add(t)
            For Each ValKV In tValues
                If ValKV.Key <> tDriComp.V Then tValues(ValKV.Key).Add(Values(ValKV.Key)(0))
            Next
            tValues(tDriComp.V).Add(Speed(0) * 3.6)
            tSpeedOgl.Add(SpeedOgl(0) * 3.6)
            tDist.Add(s)
            If bExsCompDef Then
                For Each ExsKV In ExsComponents
                    For Each ExsKVsub In ExsKV.Value
                        tExsValues(ExsKV.Key)(ExsKVsub.Key).Add(ExsKVsub.Value(0))
                    Next
                Next
            End If
            If bAuxDef Then
                For Each AuxKV In AuxComponents
                    tAuxValues(AuxKV.Key).Add(AuxKV.Value(0))
                Next
            End If
        End If

        For i = 0 To tDim - 1

            vm = (Speed(i) + Speed(i + 1)) / 2

            If vm = 0 Then
                WorkerMsg(tMsgID.Err, "Speed can't be zero while distance changes! (line " & i.ToString & ")", MsgSrc)
                Return False
            End If

            ds = Dist(i + 1) - Dist(i)
            dt = ds / vm

            t += dt
            s += ds

            Time.Add(t)
            For Each ValKV In tValues
                If ValKV.Key <> tDriComp.V Then tValues(ValKV.Key).Add(Values(ValKV.Key)(i + 1))
            Next
            tValues(tDriComp.V).Add(Speed(i + 1) * 3.6)
            tSpeedOgl.Add(SpeedOgl(i + 1) * 3.6)
            tDist.Add(s)
            If bExsCompDef Then
                For Each ExsKV In ExsComponents
                    For Each ExsKVsub In ExsKV.Value
                        tExsValues(ExsKV.Key)(ExsKVsub.Key).Add(ExsKVsub.Value(i + 1))
                    Next
                Next
            End If
            If bAuxDef Then
                For Each AuxKV In AuxComponents
                    tAuxValues(AuxKV.Key).Add(AuxKV.Value(i + 1))
                Next
            End If

            If Speed(i + 1) < 0.0001 Then

                If StopTime(i + 1) = 0 Then
                    WorkerMsg(tMsgID.Err, "Stop time = 0 at distance= " & s & "[m]!", MsgSrc)
                    StopTimeError = True
                End If

                t += StopTime(i + 1)

                Time.Add(t)
                For Each ValKV In tValues
                    If ValKV.Key <> tDriComp.V Then tValues(ValKV.Key).Add(Values(ValKV.Key)(i + 1))
                Next
                tValues(tDriComp.V).Add(Speed(i + 1) * 3.6)
                tSpeedOgl.Add(SpeedOgl(i + 1) * 3.6)
                tDist.Add(s)
                If bExsCompDef Then
                    For Each ExsKV In ExsComponents
                        For Each ExsKVsub In ExsKV.Value
                            tExsValues(ExsKV.Key)(ExsKVsub.Key).Add(ExsKVsub.Value(i + 1))
                        Next
                    Next
                End If
                If bAuxDef Then
                    For Each AuxKV In AuxComponents
                        tAuxValues(AuxKV.Key).Add(AuxKV.Value(i + 1))
                    Next
                End If

            End If

        Next


        If StopTimeError Then Return False

        '*********************************** Convert to 1Hz '***********************************
        i = 0
        j = -1
        tDim = Time.Count - 1
        s = 0

        tmax = Math.Floor(Time(tDim))



        Do
            j += 1

            hzTime.Add(j)

            Do While Time(i) <= hzTime(j)
                i += 1
            Loop

            If Time(i) - Time(i - 1) = 0 Then
                hzDist.Add(tDist(i - 1))
            Else
                hzDist.Add((hzTime(j) - Time(i - 1)) * (tDist(i) - tDist(i - 1)) / (Time(i) - Time(i - 1)) + tDist(i - 1))
            End If

            If tDist(i) - tDist(i - 1) = 0 Then
                For Each ValKV In tValues
                    If ValKV.Key = tDriComp.V Then
                        hzValues(ValKV.Key).Add(0)
                    Else
                        hzValues(ValKV.Key).Add(tValues(ValKV.Key)(i - 1))
                    End If
                Next
                hzSpeedOgl.Add(0)

                If bExsCompDef Then
                    For Each ExsKV In ExsComponents
                        For Each ExsKVsub In ExsKV.Value
                            'WRONG!! =>   hzExsValues(ExsKV.Key)(ExsKVsub.Key).Add(ExsKVsub.Value(i - 1))
                            hzExsValues(ExsKV.Key)(ExsKVsub.Key).Add(tExsValues(ExsKV.Key)(ExsKVsub.Key)(i - 1))
                        Next
                    Next
                End If
                If bAuxDef Then
                    For Each AuxKV In AuxComponents
                        'WRONG!! => hzAuxValues(AuxKV.Key).Add(AuxKV.Value(i - 1))
                        hzAuxValues(AuxKV.Key).Add(tAuxValues(AuxKV.Key)(i - 1))
                    Next
                End If

            Else

                For Each ValKV In tValues
                    hzValues(ValKV.Key).Add((hzDist(j) - tDist(i - 1)) * (tValues(ValKV.Key)(i) - tValues(ValKV.Key)(i - 1)) / (tDist(i) - tDist(i - 1)) + tValues(ValKV.Key)(i - 1))
                Next
                hzSpeedOgl.Add((hzDist(j) - tDist(i - 1)) * (tSpeedOgl(i) - tSpeedOgl(i - 1)) / (tDist(i) - tDist(i - 1)) + tSpeedOgl(i - 1))

                If bExsCompDef Then
                    For Each ExsKV In ExsComponents
                        For Each ExsKVsub In ExsKV.Value
                            hzExsValues(ExsKV.Key)(ExsKVsub.Key).Add((hzDist(j) - tDist(i - 1)) * (tExsValues(ExsKV.Key)(ExsKVsub.Key)(i) - tExsValues(ExsKV.Key)(ExsKVsub.Key)(i - 1)) / (tDist(i) - tDist(i - 1)) + tExsValues(ExsKV.Key)(ExsKVsub.Key)(i - 1))
                        Next
                    Next
                End If
                If bAuxDef Then
                    For Each AuxKV In AuxComponents
                        hzAuxValues(AuxKV.Key).Add((hzDist(j) - tDist(i - 1)) * (tAuxValues(AuxKV.Key)(i) - tAuxValues(AuxKV.Key)(i - 1)) / (tDist(i) - tDist(i - 1)) + tAuxValues(AuxKV.Key)(i - 1))
                    Next
                End If

            End If

        Loop Until i = tDim + 1 Or j + 1 > tmax

        Values = hzValues
        VoglS = hzSpeedOgl
        MODdata.Vh.Weg = hzDist
        If bExsCompDef Then ExsComponents = hzExsValues
        If bAuxDef Then AuxComponents = hzAuxValues
        tDim = Values(tDriComp.V).Count - 1

        Return True

    End Function

    Public Function ConvTo1Hz() As Boolean

        Dim tMin As Double
        Dim tMax As Double
        Dim tMid As Integer
        Dim Anz As Integer
        Dim z As Integer
        Dim Time As Double
        Dim t1 As Double
        Dim Finish As Boolean
        Dim NewValues As Dictionary(Of tDriComp, List(Of Double))
        Dim KV As KeyValuePair(Of tDriComp, List(Of Double))
        Dim KVd As KeyValuePair(Of tDriComp, Double)
        Dim fTime As List(Of Double)
        Dim Summe As Dictionary(Of tDriComp, Double)

        Dim NewMapValues As Dictionary(Of String, List(Of Double))
        Dim EmKV As KeyValuePair(Of String, cEmComp)
        Dim MapSumme As Dictionary(Of String, Double)

        Dim NewExsValues As Dictionary(Of tExsComp, Dictionary(Of Short, List(Of Single))) = Nothing
        Dim ExsKV As KeyValuePair(Of tExsComp, Dictionary(Of Short, List(Of Single)))
        Dim ExsKVsub As KeyValuePair(Of Short, List(Of Single))
        Dim ExsSumme As Dictionary(Of tExsComp, Dictionary(Of Short, Single)) = Nothing

        Dim NewAuxValues As Dictionary(Of String, List(Of Single)) = Nothing
        Dim AuxKV As KeyValuePair(Of String, List(Of Single))
        Dim AuxSumme As Dictionary(Of String, Single) = Nothing

        Dim MsgSrc As String

        MsgSrc = "DRI/Convert"

        fTime = Values(tDriComp.t)

        'Check whether Time is not reversed
        For z = 1 To tDim
            If fTime(z) < fTime(z - 1) Then
                WorkerMsg(tMsgID.Err, "Time step invalid! t(" & z - 1 & ") = " & fTime(z - 1) & "[s], t(" & z & ") = " & fTime(z) & "[s]", MsgSrc)
                Return False
            End If
        Next

        'Define Time-range
        t0 = CInt(Math.Round(fTime(0), 0, MidpointRounding.AwayFromZero))
        t1 = fTime(tDim)

        'Create Output, Total and Num-of-Dictionaries
        NewValues = New Dictionary(Of tDriComp, List(Of Double))
        Summe = New Dictionary(Of tDriComp, Double)

        For Each KV In Values
            NewValues.Add(KV.Key, New List(Of Double))
            If KV.Key <> tDriComp.t Then Summe.Add(KV.Key, 0)
        Next

        NewMapValues = New Dictionary(Of String, List(Of Double))
        MapSumme = New Dictionary(Of String, Double)

        For Each EmKV In EmComponents
            NewMapValues.Add(EmKV.Key, New List(Of Double))
            MapSumme.Add(EmKV.Key, 0)
        Next

        If bExsCompDef Then
            NewExsValues = New Dictionary(Of tExsComp, Dictionary(Of Short, List(Of Single)))
            ExsSumme = New Dictionary(Of tExsComp, Dictionary(Of Short, Single))
            For Each ExsKV In ExsComponents
                NewExsValues.Add(ExsKV.Key, New Dictionary(Of Short, List(Of Single)))
                ExsSumme.Add(ExsKV.Key, New Dictionary(Of Short, Single))
                For Each ExsKVsub In ExsKV.Value
                    NewExsValues(ExsKV.Key).Add(ExsKVsub.Key, New List(Of Single))
                    ExsSumme(ExsKV.Key).Add(ExsKVsub.Key, 0)
                Next
            Next
        End If

        If bAuxDef Then
            NewAuxValues = New Dictionary(Of String, List(Of Single))
            AuxSumme = New Dictionary(Of String, Single)
            For Each AuxKV In AuxComponents
                NewAuxValues.Add(AuxKV.Key, New List(Of Single))
                AuxSumme.Add(AuxKV.Key, 0)
            Next
        End If

        'Start-values
        tMin = fTime(0)
        tMid = CInt(tMin)
        tMax = tMid + 0.5

        If fTime(0) >= tMax Then
            tMid = tMid + 1
            tMin = tMid - 0.5
            tMax = tMid + 0.5
            t0 = tMid
        End If

        Anz = 0
        Finish = False

        For z = 0 To tDim

            'Next Time-step
            Time = fTime(z)

lb10:

            'If Time-step > tMax:
            If Time >= tMax Or z = tDim Then

                'Conclude Second
                NewValues(tDriComp.t).Add(tMid)

                'If no values ​​in Sum: Interpolate
                If Anz = 0 Then

                    For Each KVd In Summe
                        NewValues(KVd.Key).Add((tMid - fTime(z - 1)) * (Values(KVd.Key)(z) - Values(KVd.Key)(z - 1)) / (fTime(z) - fTime(z - 1)) + Values(KVd.Key)(z - 1))
                    Next

                    For Each EmKV In EmComponents
                        NewMapValues(EmKV.Key).Add((tMid - fTime(z - 1)) * (EmKV.Value.RawVals(z) - EmKV.Value.RawVals(z - 1)) / (fTime(z) - fTime(z - 1)) + EmKV.Value.RawVals(z - 1))
                    Next

                    If bExsCompDef Then
                        For Each ExsKV In ExsComponents
                            For Each ExsKVsub In ExsKV.Value
                                NewExsValues(ExsKV.Key)(ExsKVsub.Key).Add((tMid - fTime(z - 1)) * (ExsKVsub.Value(z) - ExsKVsub.Value(z - 1)) / (fTime(z) - fTime(z - 1)) + ExsKVsub.Value(z - 1))
                            Next
                        Next
                    End If

                    If bAuxDef Then
                        For Each AuxKV In AuxComponents
                            NewAuxValues(AuxKV.Key).Add((tMid - fTime(z - 1)) * (AuxKV.Value(z) - AuxKV.Value(z - 1)) / (fTime(z) - fTime(z - 1)) + AuxKV.Value(z - 1))
                        Next
                    End If

                Else

                    If Time = tMax Then

                        For Each KVd In Summe
                            NewValues(KVd.Key).Add((Summe(KVd.Key) + Values(KVd.Key)(z)) / (Anz + 1))
                        Next

                        For Each EmKV In EmComponents
                            NewMapValues(EmKV.Key).Add((MapSumme(EmKV.Key) + EmKV.Value.RawVals(z)) / (Anz + 1))
                        Next

                        If bExsCompDef Then
                            For Each ExsKV In ExsComponents
                                For Each ExsKVsub In ExsKV.Value
                                    NewExsValues(ExsKV.Key)(ExsKVsub.Key).Add((ExsSumme(ExsKV.Key)(ExsKVsub.Key) + ExsKVsub.Value(z)) / (Anz + 1))
                                Next
                            Next
                        End If

                        If bAuxDef Then
                            For Each AuxKV In AuxComponents
                                NewAuxValues(AuxKV.Key).Add((AuxSumme(AuxKV.Key) + AuxKV.Value(z)) / (Anz + 1))
                            Next
                        End If

                    Else
                        'If only one Value: Inter- /Extrapolate
                        If Anz = 1 Then

                            If z < 2 OrElse fTime(z - 1) < tMid Then

                                For Each KVd In Summe
                                    NewValues(KVd.Key).Add((tMid - fTime(z - 1)) * (Values(KVd.Key)(z) - Values(KVd.Key)(z - 1)) / (fTime(z) - fTime(z - 1)) + Values(KVd.Key)(z - 1))
                                Next

                                For Each EmKV In EmComponents
                                    NewMapValues(EmKV.Key).Add((tMid - fTime(z - 1)) * (EmKV.Value.RawVals(z) - EmKV.Value.RawVals(z - 1)) / (fTime(z) - fTime(z - 1)) + EmKV.Value.RawVals(z - 1))
                                Next

                                If bExsCompDef Then
                                    For Each ExsKV In ExsComponents
                                        For Each ExsKVsub In ExsKV.Value
                                            NewExsValues(ExsKV.Key)(ExsKVsub.Key).Add((tMid - fTime(z - 1)) * (ExsKVsub.Value(z) - ExsKVsub.Value(z - 1)) / (fTime(z) - fTime(z - 1)) + ExsKVsub.Value(z - 1))
                                        Next
                                    Next
                                End If

                                If bAuxDef Then
                                    For Each AuxKV In AuxComponents
                                        NewAuxValues(AuxKV.Key).Add((tMid - fTime(z - 1)) * (AuxKV.Value(z) - AuxKV.Value(z - 1)) / (fTime(z) - fTime(z - 1)) + AuxKV.Value(z - 1))
                                    Next
                                End If

                            Else

                                For Each KVd In Summe
                                    NewValues(KVd.Key).Add((tMid - fTime(z - 2)) * (Values(KVd.Key)(z - 1) - Values(KVd.Key)(z - 2)) / (fTime(z - 1) - fTime(z - 2)) + Values(KVd.Key)(z - 2))
                                Next

                                For Each EmKV In EmComponents
                                    NewMapValues(EmKV.Key).Add((tMid - fTime(z - 2)) * (EmKV.Value.RawVals(z - 1) - EmKV.Value.RawVals(z - 2)) / (fTime(z - 1) - fTime(z - 2)) + EmKV.Value.RawVals(z - 2))
                                Next

                                If bExsCompDef Then
                                    For Each ExsKV In ExsComponents
                                        For Each ExsKVsub In ExsKV.Value
                                            NewExsValues(ExsKV.Key)(ExsKVsub.Key).Add((tMid - fTime(z - 2)) * (ExsKVsub.Value(z - 1) - ExsKVsub.Value(z - 2)) / (fTime(z - 1) - fTime(z - 2)) + ExsKVsub.Value(z - 2))
                                        Next
                                    Next
                                End If

                                If bAuxDef Then
                                    For Each AuxKV In AuxComponents
                                        NewAuxValues(AuxKV.Key).Add((tMid - fTime(z - 2)) * (AuxKV.Value(z - 1) - AuxKV.Value(z - 2)) / (fTime(z - 1) - fTime(z - 2)) + AuxKV.Value(z - 2))
                                    Next
                                End If

                            End If

                        Else

                            For Each KVd In Summe
                                NewValues(KVd.Key).Add(Summe(KVd.Key) / Anz)
                            Next

                            For Each EmKV In EmComponents
                                NewMapValues(EmKV.Key).Add(MapSumme(EmKV.Key) / Anz)
                            Next

                            If bExsCompDef Then
                                For Each ExsKV In ExsComponents
                                    For Each ExsKVsub In ExsKV.Value
                                        NewExsValues(ExsKV.Key)(ExsKVsub.Key).Add(ExsSumme(ExsKV.Key)(ExsKVsub.Key) / Anz)
                                    Next
                                Next
                            End If

                            If bAuxDef Then
                                For Each AuxKV In AuxComponents
                                    NewAuxValues(AuxKV.Key).Add(AuxSumme(AuxKV.Key) / Anz)
                                Next
                            End If

                        End If
                    End If
                End If

                If Not Finish Then

                    'Set New Area(Bereich)
                    tMid = tMid + 1
                    tMin = tMid - 0.5
                    tMax = tMid + 0.5

                    'Check whether last second
                    If tMax > t1 Then
                        tMax = t1
                        Finish = True
                    End If

                    'New Sum /Num no start
                    For Each KV In Values
                        If KV.Key <> tDriComp.t Then Summe(KV.Key) = 0
                    Next

                    For Each EmKV In EmComponents
                        MapSumme(EmKV.Key) = 0
                    Next

                    If bExsCompDef Then
                        For Each ExsKV In ExsComponents
                            For Each ExsKVsub In ExsKV.Value
                                ExsSumme(ExsKV.Key)(ExsKVsub.Key) = 0
                            Next
                        Next
                    End If

                    If bAuxDef Then
                        For Each AuxKV In AuxComponents
                            AuxSumme(AuxKV.Key) = 0
                        Next
                    End If

                    Anz = 0

                    GoTo lb10

                End If

            End If

            For Each KV In Values
                If KV.Key <> tDriComp.t Then Summe(KV.Key) += Values(KV.Key)(z)
            Next

            For Each EmKV In EmComponents
                MapSumme(EmKV.Key) += EmKV.Value.RawVals(z)
            Next

            If bExsCompDef Then
                For Each ExsKV In ExsComponents
                    For Each ExsKVsub In ExsKV.Value
                        ExsSumme(ExsKV.Key)(ExsKVsub.Key) += ExsKVsub.Value(z)
                    Next
                Next
            End If

            If bAuxDef Then
                For Each AuxKV In AuxComponents
                    AuxSumme(AuxKV.Key) += AuxKV.Value(z)
                Next
            End If

            Anz = Anz + 1

        Next

        'Accept New fields
        Values = NewValues
        tDim = Values(tDriComp.t).Count - 1

        For Each EmKV In EmComponents
            EmKV.Value.RawVals.Clear()
            For z = 0 To tDim
                EmKV.Value.RawVals.Add(NewMapValues(EmKV.Key)(z))
            Next
        Next

        Return True

    End Function

    Public Sub FirstZero()
        Dim EmKV As KeyValuePair(Of String, cEmComp)
        Dim ExsKV As KeyValuePair(Of tExsComp, Dictionary(Of Short, List(Of Single)))
        Dim AuxKV As KeyValuePair(Of String, List(Of Single))
        Dim ValKV As KeyValuePair(Of tDriComp, List(Of Double))
        Dim ExsKVsub As KeyValuePair(Of Short, List(Of Single))

        tDim += 1

        For Each ValKV In Values
            ValKV.Value.Insert(0, ValKV.Value(0))
        Next

        If Scycle Then VoglS.Insert(0, VoglS(0))

        If bExsCompDef Then
            For Each ExsKV In ExsComponents
                For Each ExsKVsub In ExsKV.Value
                    ExsKVsub.Value.Insert(0, ExsKVsub.Value(0))
                Next
            Next
        End If

        If bAuxDef Then
            For Each AuxKV In AuxComponents
                AuxKV.Value.Insert(0, AuxKV.Value(0))
            Next
        End If

        If bEmCompDef Then
            For Each EmKV In EmComponents
                EmKV.Value.RawVals.Insert(0, EmKV.Value.RawVals(0))
            Next
        End If


    End Sub

    Public Property FilePath() As String
        Get
            Return sFilePath
        End Get
        Set(ByVal value As String)
            sFilePath = value
        End Set
    End Property

    Public ReadOnly Property EmCompDef As Boolean
        Get
            Return bEmCompDef
        End Get
    End Property

    Public ReadOnly Property CreateMapParDef As Boolean
        Get
            Return bCreateMapParDef
        End Get
    End Property

    Public ReadOnly Property AuxDef As Boolean
        Get
            Return bAuxDef
        End Get
    End Property


   
End Class

