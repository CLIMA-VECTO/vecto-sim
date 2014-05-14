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

''' <summary>
''' Driving cycle input file
''' </summary>
''' <remarks></remarks>
Public Class cDRI

    ''' <summary>
    ''' Last index of driving cycle columns
    ''' </summary>
    ''' <remarks></remarks>
    Public tDim As Integer

    ''' <summary>
    ''' Dictionary holding all driving cycle columns. Key= Parameter-ID (enum), Value= parameter value per time step
    ''' </summary>
    ''' <remarks></remarks>
    Public Values As Dictionary(Of tDriComp, List(Of Double))

    ''' <summary>
    ''' First time stamp in driving cycle
    ''' </summary>
    ''' <remarks></remarks>
    Public t0 As Integer

    ''' <summary>
    ''' Full filepath. Needs to be defined before calling ReadFile. 
    ''' </summary>
    ''' <remarks></remarks>
    Public FilePath As String

    ''' <summary>
    ''' True= Cycle includes time stamps. Defined in ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Public Tvorg As Boolean

    ''' <summary>
    ''' True= Cycle includes time vehicle speed. Defined in ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Public Vvorg As Boolean

    ''' <summary>
    ''' True= Cycle includes engine power. Defined in ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Public Pvorg As Boolean

    ''' <summary>
    ''' True= Cycle includes additional auxiliary power demand (not to be confused with auxiliary supply power). Defined in ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Public PaddVorg As Boolean

    ''' <summary>
    ''' True= Cycle includes engine speed. Defined in ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Public Nvorg As Boolean

    ''' <summary>
    ''' True= Cycle includes gear input. Defined in ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Public Gvorg As Boolean

    ''' <summary>
    ''' True= Cycle includes slope. Defined in ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Public GradVorg As Boolean

    ''' <summary>
    ''' True= Cycle includes auxiliary supply power for at least one auxiliary. Defined in ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Public AuxDef As Boolean

    ''' <summary>
    ''' Auxiliary supply power input. Key= Aux-ID, Value= Supply power [kW] per time step
    ''' </summary>
    ''' <remarks></remarks>
    Public AuxComponents As Dictionary(Of String, List(Of Single))

    ''' <summary>
    ''' True= Cycle includes VairRes and VairBeta for side wind correction. Defined in ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Public VairVorg As Boolean

    ''' <summary>
    ''' True= Cycle includes distance. Defined in ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Public Scycle As Boolean

    ''' <summary>
    ''' True= Cycle includes slope. Defined in ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Public VoglS As List(Of Double)

    ''' <summary>
    ''' Reset all fields, etc. berfore loading new file. Called by ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ResetMe()
        Values = Nothing
        PaddVorg = False
        Tvorg = False
        Vvorg = False
        GradVorg = False
        Nvorg = False
        Gvorg = False
        Pvorg = False
        tDim = -1
        t0 = 1  'Default if no time steps are defined in driving cycle
        AuxDef = False
        AuxComponents = Nothing
        VairVorg = False
        Scycle = False
    End Sub

    ''' <summary>
    ''' Read driving cycle. FilePath must be defined before calling.
    ''' </summary>
    ''' <returns>True= File loaded successfully.</returns>
    ''' <remarks></remarks>
    Public Function ReadFile() As Boolean
        Dim file As cFile_V3
        Dim line As String()
        Dim s1 As Integer
        Dim s As Integer
        Dim txt As String
        Dim Comp As tDriComp
        Dim AuxComp As tAuxComp
        Dim AuxID As String
        Dim Svorg As Boolean = False

        Dim DRIcheck As Dictionary(Of tDriComp, Boolean)
        Dim Spalten As Dictionary(Of tDriComp, Integer)
        Dim sKV As KeyValuePair(Of tDriComp, Integer)

        Dim AuxSpalten As Dictionary(Of String, Integer) = Nothing
        Dim Mvorg As Boolean = False


        Dim MsgSrc As String


        MsgSrc = "Main/ReadInp/DRI"


        'Reset
        ResetMe()

        'Abort if there's no file
        If FilePath = "" OrElse Not IO.File.Exists(FilePath) Then
            WorkerMsg(tMsgID.Err, "Cycle file not found (" & FilePath & ") !", MsgSrc)
            Return False
        End If

        'EmComp Init
        '...now in New()

        'Open file
        file = New cFile_V3
        If Not file.OpenRead(FilePath) Then
            WorkerMsg(tMsgID.Err, "Failed to open file (" & FilePath & ") !", MsgSrc)
            file = Nothing
            Return False
        End If

        DRIcheck = New Dictionary(Of tDriComp, Boolean)
        DRIcheck.Add(tDriComp.t, False)
        DRIcheck.Add(tDriComp.V, False)
        DRIcheck.Add(tDriComp.Grad, False)
        DRIcheck.Add(tDriComp.nU, False)
        DRIcheck.Add(tDriComp.Gears, False)
        DRIcheck.Add(tDriComp.Padd, False)
        DRIcheck.Add(tDriComp.Pe, False)
        DRIcheck.Add(tDriComp.VairVres, False)
        DRIcheck.Add(tDriComp.VairBeta, False)
        DRIcheck.Add(tDriComp.s, False)
        DRIcheck.Add(tDriComp.StopTime, False)
        DRIcheck.Add(tDriComp.Torque, False)

        If file.EndOfFile Then
            WorkerMsg(tMsgID.Err, "Driving cycle invalid!", MsgSrc)
            Return False
        End If

        Spalten = New Dictionary(Of tDriComp, Integer)
        Values = New Dictionary(Of tDriComp, List(Of Double))

        '***
        '*** First row: Name/Identification of the Components
        line = file.ReadLine

        'Check Number of Columns/Components
        s1 = UBound(line)

        For s = 0 To s1

            Comp = fDriComp(line(s))

            'Falls DRIcomp = Undefined dann wirds als EXS-Comp oder als Emission für KF-Erstellung / Eng-Analysis verwendet |@@| If used DRIcomp = Undefined it will get as EXS-Comp or Emission for KF-Creation / Eng-Analysis
            If Comp = tDriComp.Undefined Then

                AuxComp = fAuxComp(line(s))

                If AuxComp = tAuxComp.Undefined Then

                    'ERROR when component is unknown
                    WorkerMsg(tMsgID.Err, "'" & line(s) & "' is no valid cycle input parameter!", MsgSrc)
                    GoTo lbEr

                Else

                    txt = fCompSubStr(line(s))

                    If Not AuxDef Then
                        AuxComponents = New Dictionary(Of String, List(Of Single))
                        AuxSpalten = New Dictionary(Of String, Integer)
                    End If

                    If AuxComponents.ContainsKey(txt) Then
                        WorkerMsg(tMsgID.Err, "Multiple definitions of auxiliary '" & txt & "' in driving cycle! Column " & s + 1, MsgSrc)
                        GoTo lbEr
                    End If

                    AuxComponents.Add(txt, New List(Of Single))
                    AuxSpalten.Add(txt, s)

                    AuxDef = True

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
        Nvorg = DRIcheck(tDriComp.nU)
        Pvorg = DRIcheck(tDriComp.Pe)
        PaddVorg = DRIcheck(tDriComp.Padd)
        GradVorg = DRIcheck(tDriComp.Grad)
        VairVorg = DRIcheck(tDriComp.VairVres) And DRIcheck(tDriComp.VairBeta)
        Mvorg = DRIcheck(tDriComp.Torque)

        If Mvorg And Pvorg Then
            WorkerMsg(tMsgID.Warn, "Engine torque and power defined in cycle! Torque will be ignored!", MsgSrc)
            Mvorg = False
        End If


        Try
            Do While Not file.EndOfFile
                tDim += 1       'wird in ResetMe zurück gesetzt
                line = file.ReadLine

                For Each sKV In Spalten

                    If sKV.Key = tDriComp.Pe Or sKV.Key = tDriComp.Torque Then
                        If Trim(line(sKV.Value)) = sKey.EngDrag Then line(sKV.Value) = -999999
                    End If

                    Values(sKV.Key).Add(CDbl(line(sKV.Value)))
                Next

                If AuxDef Then
                    For Each AuxID In AuxSpalten.Keys
                        AuxComponents(AuxID).Add(CSng(line(AuxSpalten(AuxID))))
                    Next
                End If

            Loop
        Catch ex As Exception

            WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & tDim + 1 & " (" & FilePath & ")", MsgSrc, FilePath)
            GoTo lbEr

        End Try

        file.Close()

        Scycle = (Svorg And Not Tvorg)

        If Vvorg Then
            For s = 0 To tDim
                Values(tDriComp.V)(s) /= 3.6
                If Values(tDriComp.V)(s) < 0.025 Then Values(tDriComp.V)(s) = 0
            Next
        End If

        If Mvorg And Nvorg Then
            Values.Add(tDriComp.Pe, New List(Of Double))
            Pvorg = True
            For s = 0 To tDim
                Values(tDriComp.Pe).Add(nMtoPe(Values(tDriComp.nU)(s), Values(tDriComp.Torque)(s)))
            Next
        End If

        Return True

lbEr:
        file.Close()

        Return False

    End Function

    ''' <summary>
    ''' Calculates altitude for each time step if driving cycle includes slope.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GradToAlt()
        Dim i As Integer
        Dim v0 As New List(Of Double)
        Dim vg As List(Of Double)
        Dim vs As List(Of Double)
        Dim vv As List(Of Double)
        Dim vt As List(Of Double)

        'Skip if altitude is defined already
        If Values.ContainsKey(tDriComp.Alt) Then Exit Sub

        If GradVorg And Vvorg Then

            vg = Values(tDriComp.Grad)

            v0.Add(0)

            If Scycle Then

                vs = Values(tDriComp.s)

                For i = 1 To tDim
                    v0.Add(v0(i - 1) + ((vg(i) + vg(i - 1)) / 200) * (vs(i) - vs(i - 1)))
                Next


            Else

                vv = Values(tDriComp.V)

                If Tvorg Then

                    vt = Values(tDriComp.t)

                    For i = 1 To tDim
                        v0.Add(v0(i - 1) + ((vg(i) + vg(i - 1)) / 200) * vv(i) * (vt(i) - vt(i - 1)))
                    Next

                Else

                    For i = 1 To tDim
                        v0.Add(v0(i - 1) + ((vg(i) + vg(i - 1)) / 200) * vv(i) * 1)
                    Next

                End If
            End If



        Else

            For i = 0 To tDim
                v0.Add(0)
            Next

        End If

        Values.Add(tDriComp.Alt, v0)
        Values.Remove(tDriComp.Grad)

    End Sub

    ''' <summary>
    ''' Convert distance-based cycle to time-based cycle.
    ''' </summary>
    ''' <returns>True= Convertion successful.</returns>
    ''' <remarks></remarks>
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
        Dim Alt As List(Of Double)
        Dim StopTime As List(Of Double)
        Dim Time As New List(Of Double)
        Dim tValues As New Dictionary(Of tDriComp, List(Of Double))
        Dim tDist As New List(Of Double)
        Dim hzTime As New List(Of Double)
        Dim hzDist As New List(Of Double)
        Dim hzValues As New Dictionary(Of tDriComp, List(Of Double))
        Dim ValKV As KeyValuePair(Of tDriComp, List(Of Double))
        Dim tmax As Integer

        Dim tAuxValues As Dictionary(Of String, List(Of Single)) = Nothing
        Dim hzAuxValues As Dictionary(Of String, List(Of Single)) = Nothing
        Dim AuxKV As KeyValuePair(Of String, List(Of Single))

        Dim SpeedOgl As New List(Of Double)
        Dim tSpeedOgl As New List(Of Double)
        Dim hzSpeedOgl As New List(Of Double)

        Dim StopTimeError As Boolean = False

        Dim MsgSrc As String

        MsgSrc = "Main/DRI/ConvStoT"

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
            Speed.Add(Values(tDriComp.V)(i))
            SpeedOgl.Add(Values(tDriComp.V)(i))
        Next

        StopTime = Values(tDriComp.StopTime)
        vstep = 0.001

        If Values.ContainsKey(tDriComp.Alt) Then
            Alt = Values(tDriComp.Alt)
        Else
            Alt = New List(Of Double)
            For i = 0 To tDim
                Alt.Add(0)
            Next
            Values.Add(tDriComp.Alt, Alt)
        End If

        For Each ValKV In Values
            If ValKV.Key <> tDriComp.s And ValKV.Key <> tDriComp.StopTime Then
                tValues.Add(ValKV.Key, New List(Of Double))
                hzValues.Add(ValKV.Key, New List(Of Double))
            End If
        Next

        If AuxDef Then
            tAuxValues = New Dictionary(Of String, List(Of Single))
            hzAuxValues = New Dictionary(Of String, List(Of Single))
            For Each AuxKV In AuxComponents
                tAuxValues.Add(AuxKV.Key, New List(Of Single))
                hzAuxValues.Add(AuxKV.Key, New List(Of Single))
            Next
        End If

        'Check if smallest distance step is smaller or equal 1m
        For i = 1 To tDim
            If Dist(i) - Dist(i - 1) > 1 Then
                WorkerMsg(tMsgID.Err, "Distance-based cycles must not include larger distance-steps than 1[m]!", MsgSrc)
                Return False
            End If
        Next

        '*********************************** Deceleration(Verzögerung) limit ********************************
        For i = tDim To 1 Step -1

            ds = Dist(i) - Dist(i - 1)

            vm = (Speed(i) + Speed(i - 1)) / 2
            dv = Speed(i) - Speed(i - 1)

            a = vm * dv / ds

            am = VEC.aDesMin(vm)

            Do While a < am

                Speed(i - 1) -= vstep

                vm = (Speed(i) + Speed(i - 1)) / 2
                dv = Speed(i) - Speed(i - 1)

                a = vm * dv / ds

                am = VEC.aDesMin(vm)

            Loop

        Next


        '*********************************** Create Time-sequence '***********************************
        t = 0
        s = Dist(0)

        Time.Add(t)
        For Each ValKV In tValues
            If ValKV.Key <> tDriComp.V Then tValues(ValKV.Key).Add(Values(ValKV.Key)(0))
        Next
        tValues(tDriComp.V).Add(Speed(0))
        tSpeedOgl.Add(SpeedOgl(0))
        tDist.Add(s)
        If AuxDef Then
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
            tValues(tDriComp.V).Add(Speed(0))
            tSpeedOgl.Add(SpeedOgl(0))
            tDist.Add(s)
            If AuxDef Then
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
            tValues(tDriComp.V).Add(Speed(i + 1))
            tSpeedOgl.Add(SpeedOgl(i + 1))
            tDist.Add(s)
            If AuxDef Then
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
                tValues(tDriComp.V).Add(Speed(i + 1))
                tSpeedOgl.Add(SpeedOgl(i + 1))
                tDist.Add(s)

                If AuxDef Then
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

                If AuxDef Then
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

                If AuxDef Then
                    For Each AuxKV In AuxComponents
                        hzAuxValues(AuxKV.Key).Add((hzDist(j) - tDist(i - 1)) * (tAuxValues(AuxKV.Key)(i) - tAuxValues(AuxKV.Key)(i - 1)) / (tDist(i) - tDist(i - 1)) + tAuxValues(AuxKV.Key)(i - 1))
                    Next
                End If

            End If

        Loop Until i = tDim + 1 Or j + 1 > tmax

        Values = hzValues
        VoglS = hzSpeedOgl
        MODdata.Vh.Weg = hzDist
        If AuxDef Then AuxComponents = hzAuxValues
        tDim = Values(tDriComp.V).Count - 1

        Return True

    End Function

    ''' <summary>
    ''' Convert cycle to 1Hz.
    ''' </summary>
    ''' <returns>True= Convertion successful.</returns>
    ''' <remarks></remarks>
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

        If AuxDef Then
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

                    If AuxDef Then
                        For Each AuxKV In AuxComponents
                            NewAuxValues(AuxKV.Key).Add((tMid - fTime(z - 1)) * (AuxKV.Value(z) - AuxKV.Value(z - 1)) / (fTime(z) - fTime(z - 1)) + AuxKV.Value(z - 1))
                        Next
                    End If

                Else

                    If Time = tMax Then

                        For Each KVd In Summe
                            NewValues(KVd.Key).Add((Summe(KVd.Key) + Values(KVd.Key)(z)) / (Anz + 1))
                        Next

                        If AuxDef Then
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

                                If AuxDef Then
                                    For Each AuxKV In AuxComponents
                                        NewAuxValues(AuxKV.Key).Add((tMid - fTime(z - 1)) * (AuxKV.Value(z) - AuxKV.Value(z - 1)) / (fTime(z) - fTime(z - 1)) + AuxKV.Value(z - 1))
                                    Next
                                End If

                            Else

                                For Each KVd In Summe
                                    NewValues(KVd.Key).Add((tMid - fTime(z - 2)) * (Values(KVd.Key)(z - 1) - Values(KVd.Key)(z - 2)) / (fTime(z - 1) - fTime(z - 2)) + Values(KVd.Key)(z - 2))
                                Next

                                If AuxDef Then
                                    For Each AuxKV In AuxComponents
                                        NewAuxValues(AuxKV.Key).Add((tMid - fTime(z - 2)) * (AuxKV.Value(z - 1) - AuxKV.Value(z - 2)) / (fTime(z - 1) - fTime(z - 2)) + AuxKV.Value(z - 2))
                                    Next
                                End If

                            End If

                        Else

                            For Each KVd In Summe
                                NewValues(KVd.Key).Add(Summe(KVd.Key) / Anz)
                            Next

                            If AuxDef Then
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

                    If AuxDef Then
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

            If AuxDef Then
                For Each AuxKV In AuxComponents
                    AuxSumme(AuxKV.Key) += AuxKV.Value(z)
                Next
            End If

            Anz = Anz + 1

        Next

        'Accept New fields
        Values = NewValues
        tDim = Values(tDriComp.t).Count - 1

        Return True

    End Function

    ''' <summary>
    ''' Duplicates first time step. Needed for distance-based cycles to ensure vehicle stop at cycle start.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub FirstZero()
        Dim AuxKV As KeyValuePair(Of String, List(Of Single))
        Dim ValKV As KeyValuePair(Of tDriComp, List(Of Double))

        tDim += 1

        For Each ValKV In Values
            ValKV.Value.Insert(0, ValKV.Value(0))
        Next

        If Scycle Then VoglS.Insert(0, VoglS(0))

        If AuxDef Then
            For Each AuxKV In AuxComponents
                AuxKV.Value.Insert(0, AuxKV.Value(0))
            Next
        End If

    End Sub

End Class

