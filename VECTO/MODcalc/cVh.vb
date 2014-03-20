Imports System.Collections.Generic

Public Class cVh

    'From DRI file
    Private lV As List(Of Single)       'Ist-Geschw. in Zwischensekunden.
    Private lV0ogl As List(Of Single)   'Original DRI-Geschwindigkeit. Wird nicht geändert.
    Private lV0 As List(Of Single)      'DRI-Geschwindigkeit. Bei Geschw.-Reduktion in Zeitschritt t wird LV0(t+1) reduziert.
    Private lGears As List(Of Short)
    Private lPadd As List(Of Single)
    Private lVairVres As List(Of Single)
    Private lVairBeta As List(Of Single)
    Public EcoRoll As List(Of Boolean)

    'Calculated
    Private la As List(Of Single)

    'WegKor |@@| Route(Weg)Correct
    Private dWegIst As Double
    Public Weg As List(Of Double)
    Private WegX As Integer
    Private WegV As List(Of Single)

    Public NoDistCorr As List(Of Boolean)

    Private lAlt0 As List(Of Double)
    Private ls0 As List(Of Single)
    Private iAlt As Integer
    Private iAltDim As Integer

    Public Sub Init()
        lV = New List(Of Single)
        lV0ogl = New List(Of Single)
        lV0 = New List(Of Single)
        lAlt0 = New List(Of Double)
        ls0 = New List(Of Single)
        lGears = New List(Of Short)
        lPadd = New List(Of Single)
        la = New List(Of Single)
        Weg = New List(Of Double)
        lVairVres = New List(Of Single)
        lVairBeta = New List(Of Single)
        EcoRoll = New List(Of Boolean)
        NoDistCorr = New List(Of Boolean)
        iAlt = 1
        iAltDim = 0
    End Sub

    Public Sub CleanUp()
        lV = Nothing
        lV0ogl = Nothing
        lV0 = Nothing
        lAlt0 = Nothing
        ls0 = Nothing
        lGears = Nothing
        lPadd = Nothing
        la = Nothing
        Weg = Nothing
        lVairVres = Nothing
        lVairBeta = Nothing
        EcoRoll = Nothing
        NoDistCorr = Nothing
    End Sub

    Public Sub VehCylceInit()

        Dim s As Integer
        Dim sl As Integer
        Dim L As List(Of Double)
        Dim Val As Single

        'Speed
        If DRI.Vvorg Then

            L = DRI.Values(tDriComp.V)
            For s = 0 To MODdata.tDim
                lV0.Add(L(s))
                If Not DRI.Scycle Then lV0ogl.Add(L(s))
                lV.Add((L(s + 1) + L(s)) / 2)
                If lV(s) < 0.001 Then lV(s) = 0 '<= aus Leistg
            Next

            'Original-speed is longer by 1
            lV0.Add(DRI.Values(tDriComp.V)(MODdata.tDim + 1))
            If DRI.Scycle Then
                For s = 0 To MODdata.tDim + 1
                    lV0ogl.Add(CSng(DRI.VoglS(s)))
                Next
            Else
                lV0ogl.Add(DRI.Values(tDriComp.V)(MODdata.tDim + 1))
            End If

            'Strecke (aus Zwischensekunden sonst passiert Fehler) |@@| Segment (from Intermediate-seconds, otherwise Error)
            If Not DRI.Scycle Then
                Weg.Add(lV(0))
                For s = 1 To MODdata.tDim
                    Weg.Add(Weg(s - 1) + lV(s))
                Next
            End If

        End If

        'Altitude / distance
        If Not DRI.Scycle Then
            L = DRI.Values(tDriComp.Alt)
            lAlt0.Add(0)
            ls0.Add(lV0(0))
            sl = 0
            For s = 1 To MODdata.tDim + 1
                If lV0(s) > 0 Then
                    sl += 1
                    ls0.Add(ls0(sl - 1) + lV0(s))
                    lAlt0.Add(L(s))
                End If
            Next
            iAltDim = ls0.Count - 1
        End If



        'Gear - but not Averaged, rather Gang(t) = DRI.Gear(t)
        If DRI.Gvorg Then
            L = DRI.Values(tDriComp.Gears)
            For s = 0 To MODdata.tDim
                'lGears.Add(Math.Round((DRI.Values(tDriComp.Gears)(s + 1) + DRI.Values(tDriComp.Gears)(s)) / 2, 0, MidpointRounding.AwayFromZero))
                lGears.Add(L(s))
            Next
        Else
            For s = 0 To MODdata.tDim
                lGears.Add(-1)
            Next
        End If

        'Padd
        If DRI.PaddVorg Then
            L = DRI.Values(tDriComp.Padd)
            For s = 0 To MODdata.tDim
                lPadd.Add((L(s + 1) + L(s)) / 2)
            Next
        Else
            For s = 0 To MODdata.tDim
                lPadd.Add(0)
            Next
        End If

        'Calculate Acceleration
        If DRI.Vvorg Then
            L = DRI.Values(tDriComp.V)
            For s = 0 To MODdata.tDim
                la.Add(L(s + 1) - L(s))
            Next
        End If

        'Vair specifications: Not in Intermediate-seconds!
        If DRI.VairVorg Then

            L = DRI.Values(tDriComp.VairVres)
            For s = 0 To MODdata.tDim
                lVairVres.Add(L(s) / 3.6)
            Next

            L = DRI.Values(tDriComp.VairBeta)
            For s = 0 To MODdata.tDim
                Val = Math.Abs(L(s))
                If Val > 180 Then Val -= 360
                lVairBeta.Add(Val)
            Next

        End If

        For s = 0 To MODdata.tDim
            EcoRoll.Add(False)
            NoDistCorr.Add(False)
        Next


    End Sub

    Public Sub EngCylceInit()

        Dim s As Integer
        Dim L As List(Of Double)

        'Speed
        If DRI.Vvorg Then

            L = DRI.Values(tDriComp.V)

            For s = 0 To MODdata.tDim
                lV0.Add(L(s))
                lV0ogl.Add(L(s))
                lV.Add(L(s))
                If lV(s) < 0.001 Then lV(s) = 0 '<= aus Leistg
            Next

            'Distance
            Weg.Add(lV(0))
            For s = 1 To MODdata.tDim
                Weg.Add(Weg(s - 1) + lV(s))
            Next

        End If

        'Gear - not Averaged, rather Gear(t) = DRI.Gear(t)
        If DRI.Gvorg Then
            L = DRI.Values(tDriComp.Gears)
            For s = 0 To MODdata.tDim
                'lGears.Add(Math.Round((DRI.Values(tDriComp.Gears)(s + 1) + DRI.Values(tDriComp.Gears)(s)) / 2, 0, MidpointRounding.AwayFromZero))
                lGears.Add(L(s))
            Next
        Else
            For s = 0 To MODdata.tDim
                lGears.Add(-1)
            Next
        End If

        'Padd
        If DRI.PaddVorg Then
            L = DRI.Values(tDriComp.Padd)
            For s = 0 To MODdata.tDim
                lPadd.Add(L(s))
            Next
        Else
            For s = 0 To MODdata.tDim
                lPadd.Add(0)
            Next
        End If

        'Calculate Acceleration
        If DRI.Vvorg Then
            la.Add(DRI.Values(tDriComp.V)(1) - DRI.Values(tDriComp.V)(0))
            For s = 1 To MODdata.tDim - 1
                la.Add((DRI.Values(tDriComp.V)(s + 1) - DRI.Values(tDriComp.V)(s - 1)) / 2)
            Next
            la.Add(DRI.Values(tDriComp.V)(MODdata.tDim) - DRI.Values(tDriComp.V)(MODdata.tDim - 1))
        End If

    End Sub

    Public Sub SetSpeed0(ByVal t As Integer, ByVal v0 As Single)
        lV0(t + 1) = v0
        lV(t) = (lV0(t + 1) + lV0(t)) / 2
        la(t) = lV0(t + 1) - lV0(t)
        If t < MODdata.tDim Then
            lV(t + 1) = (lV0(t + 2) + lV0(t + 1)) / 2
            la(t + 1) = lV0(t + 2) - lV0(t + 1)
        End If
    End Sub

    Public Sub SetSpeed(ByVal t As Integer, ByVal v As Single)

        If 2 * v - lV0(t) < 0 Then v = lV0(t) / 2

        lV0(t + 1) = 2 * v - lV0(t)
        lV(t) = v
        la(t) = lV0(t + 1) - lV0(t)
        If t < MODdata.tDim Then
            lV(t + 1) = (lV0(t + 2) + lV0(t + 1)) / 2
            la(t + 1) = lV0(t + 2) - lV0(t + 1)
        End If
    End Sub

    Public Sub ReduceSpeed(ByVal t As Integer, ByVal p As Single)
        lV0(t + 1) *= p
        lV(t) = (lV0(t + 1) + lV0(t)) / 2
        la(t) = lV0(t + 1) - lV0(t)
        If t < MODdata.tDim Then
            lV(t + 1) = (lV0(t + 2) + lV0(t + 1)) / 2
            la(t + 1) = lV0(t + 2) - lV0(t + 1)
        End If
    End Sub


    Public Sub SetMaxAcc(ByVal t As Integer)
        Dim a As Single
        Dim v As Single
        Dim v0plus As Single
        Dim a0 As Single

        v0plus = lV0(t + 1)

        v = (v0plus + lV0(t)) / 2
        a0 = VEC.aDesMax(v)

        v0plus = lV0(t) + a
        v = (v0plus + lV0(t)) / 2
        a = VEC.aDesMax(v)

        Do While Math.Abs(a - a0) > 0.0001

            a0 = a

            v0plus = lV0(t) + a
            v = (v0plus + lV0(t)) / 2
            a = VEC.aDesMax(v)

        Loop

        la(t) = a
        lV0(t + 1) = lV0(t) + a
        lV(t) = (lV0(t + 1) + lV0(t)) / 2
        If t < MODdata.tDim Then
            lV(t + 1) = (lV0(t + 2) + lV0(t + 1)) / 2
            la(t + 1) = lV0(t + 2) - lV0(t + 1)
        End If
    End Sub


    Public Sub SetMinAccBackw(ByVal t As Integer)
        Dim a As Single
        Dim v As Single
        Dim v0 As Single
        Dim a0 As Single

        v0 = lV0(t)

        v = (lV0(t + 1) + v0) / 2
        a0 = VEC.aDesMin(v)

        v0 = lV0(t + 1) - a
        v = (lV0(t + 1) + v0) / 2
        a = VEC.aDesMin(v)

        Do While Math.Abs(a - a0) > 0.0001

            a0 = a

            v0 = lV0(t + 1) - a
            v = (lV0(t + 1) + v0) / 2
            a = VEC.aDesMin(v)

        Loop

        la(t) = a
        lV0(t) = lV0(t + 1) - a
        lV(t) = (lV0(t + 1) + lV0(t)) / 2
        If t > 0 Then
            lV(t - 1) = (lV0(t) + lV0(t - 1)) / 2
            la(t - 1) = lV0(t) - lV0(t - 1)
        End If
    End Sub

    Public Sub DistCorrInit()
        Dim i As Int16

        WegX = 0
        dWegIst = 0

        WegV = New List(Of Single)

        For i = 0 To MODdata.tDim + 1
            WegV.Add(lV0(i))
        Next

    End Sub

    Public Function DistCorrection(ByVal t As Integer, ByVal VehState As tVehState) As Boolean
        Dim v As Single

        v = lV(t)
        dWegIst += v

        If Not Cfg.WegKorJa Then Return False

        If t + 1 > MODdata.tDim Then Return False


        If WegX + 2 < MODdata.tDimOgl Then

            'If repeating of current time-step is closer to the target distance => Repeat time-step
            If Not NoDistCorr(t) AndAlso (Math.Abs(dWegIst + Vsoll(t) - Weg(WegX)) < Math.Abs(dWegIst - Weg(WegX))) And v > 1 Then

                Duplicate(t + 1)
                MODdata.tDim += 1
                'Debug.Print("Duplicate," & t & "," & WegIst & "," & Weg(WegX))
                NoDistCorr(t + 1) = True
                Return True

                'If deleting the next time-step is closer to target distance => Delete Next Time-step
            ElseIf Not NoDistCorr(t) AndAlso WegX < MODdata.tDimOgl - 1 AndAlso t < MODdata.tDim - 1 AndAlso Math.Abs(dWegIst - Weg(WegX + 1)) <= Math.Abs(dWegIst - Weg(WegX)) AndAlso v > 1 Then

                Cut(t + 1)
                MODdata.tDim -= 1
                'Debug.Print("Cut," & t & "," & WegIst & "," & Weg(WegX))
                NoDistCorr(t + 1) = True
                WegX += 2
                Return True

            Else

                'No correction
                WegX += 1
                Return False

            End If

        End If

        Return False

    End Function

    Private Sub Duplicate(ByVal t As Integer)

        lV0.Insert(t + 1, (WegV(t + 1) + WegV(t)) / 2)
        WegV.Insert(t + 1, (WegV(t + 1) + WegV(t)) / 2)

        'If t + 1 < MODdata.tDim Then
        '    lV0.Insert(t + 1, (WegV(t + 1) + WegV(t + 2)) / 2)
        '    WegV.Insert(t + 1, (WegV(t + 1) + WegV(t + 2)) / 2)
        'Else
        '    lV0.Insert(t + 1, WegV(t + 1))
        '    WegV.Insert(t + 1, WegV(t + 1))
        'End If

        lV0ogl.Insert(t + 1, lV0ogl(t + 1))
        lV.Insert(t, (lV0(t + 1) + lV0(t)) / 2)
        la.Insert(t, lV0(t + 1) - lV0(t))

        If t < MODdata.tDim Then
            lV(t + 1) = (lV0(t + 2) + lV0(t + 1)) / 2
            la(t + 1) = lV0(t + 2) - lV0(t + 1)
        End If

        lGears.Insert(t, lGears(t))
        lPadd.Insert(t, lPadd(t))
        EcoRoll.Insert(t, EcoRoll(t))
        NoDistCorr.Insert(t, NoDistCorr(t))

        If DRI.VairVorg Then
            lVairVres.Insert(t, lVairVres(t))
            lVairBeta.Insert(t, lVairBeta(t))
        End If

        MODdata.Px.Positions.Insert(t, MODdata.Px.Positions(t))

        MODdata.Duplicate(t)


    End Sub

    Public Sub DuplicatePreRun(ByVal t As Integer)

        lV0.Insert(t, lV0(t))

        lV0ogl.Insert(t, lV0ogl(t))
        lV.Insert(t, (lV0(t + 1) + lV0(t)) / 2)
        la.Insert(t, lV0(t + 1) - lV0(t))

        If t > 0 Then
            lV(t - 1) = (lV0(t) + lV0(t - 1)) / 2
            la(t - 1) = lV0(t) - lV0(t - 1)
        End If

        lGears.Insert(t, lGears(t))
        lPadd.Insert(t, lPadd(t))
        EcoRoll.Insert(t, EcoRoll(t))
        NoDistCorr.Insert(t, NoDistCorr(t))

        If DRI.VairVorg Then
            lVairVres.Insert(t, lVairVres(t))
            lVairBeta.Insert(t, lVairBeta(t))
        End If

        MODdata.Duplicate(t)

    End Sub

    Private Sub Cut(ByVal t As Integer)

        lV0.RemoveAt(t + 1)
        lV0ogl.RemoveAt(t + 1)
        WegV.RemoveAt(t + 1)
        lV.RemoveAt(t)
        la.RemoveAt(t)

        If t < MODdata.tDim Then
            lV(t) = (lV0(t + 1) + lV0(t)) / 2
            la(t) = lV0(t + 1) - lV0(t)
        End If

        If t < MODdata.tDim - 1 Then
            lV(t + 1) = (lV0(t + 2) + lV0(t + 1)) / 2
            la(t + 1) = lV0(t + 2) - lV0(t + 1)
        End If

        lGears.RemoveAt(t)
        lPadd.RemoveAt(t)
        EcoRoll.RemoveAt(t)
        NoDistCorr.RemoveAt(t)

        If DRI.VairVorg Then
            lVairVres.RemoveAt(t)
            lVairBeta.RemoveAt(t)
        End If

        MODdata.Px.Positions.RemoveAt(t)


        MODdata.Cut(t)

    End Sub

    Public ReadOnly Property Vsoll(ByVal t As Integer) As Single
        Get
            Return (lV0ogl(t + 1) + lV0ogl(t)) / 2
        End Get
    End Property

    Public ReadOnly Property V(ByVal t As Integer) As Single
        Get
            Return lV(t)
        End Get
    End Property

    Public ReadOnly Property V0(ByVal t As Integer) As Single
        Get
            Return lV0(t)
        End Get
    End Property

    Public ReadOnly Property GearVorg(ByVal t As Integer) As Short
        Get
            Return lGears(t)
        End Get
    End Property


    Public Sub SetAlt()
        Dim Ls As List(Of Double)
        Dim Lalt As List(Of Double)
        Dim sl As Integer
        Dim s As Integer

        'Altitude / distance
        Ls = DRI.Values(tDriComp.s)
        Lalt = DRI.Values(tDriComp.Alt)

        lAlt0.Add(Lalt(0))
        ls0.Add(Ls(0))

        sl = 0
        For s = 1 To DRI.tDim
            If Ls(s) > ls0(sl) Then
                sl += 1
                ls0.Add(Ls(s))
                lAlt0.Add(Lalt(s))
            End If
        Next
        iAltDim = ls0.Count - 1

    End Sub

    Public Function fGrad(ByVal s As Double) As Single
        Dim i As Int32
        Dim dh As Single
        Dim ds As Single

        If ls0(0) >= s Then
            i = 1
            GoTo lbInt
        End If

        i = iAlt

        If ls0(i - 1) > s Then

            Do While ls0(i - 1) > s And i > 1
                i -= 1
            Loop

        Else

            Do While ls0(i) < s And i < iAltDim
                i += 1
            Loop

        End If


lbInt:
        iAlt = i

        ds = ls0(i) - ls0(i - 1)
        dh = lAlt0(i) - lAlt0(i - 1)
        Return (dh / ds) * 100

    End Function

    Public Function AltIntp(ByVal s As Single, Optional ByVal OverwiAlt As Boolean = False) As Single
        Dim i As Int32

        If ls0(0) >= s Then
            i = 1
            GoTo lbInt
        End If

        i = iAlt

        If ls0(i - 1) > s Then

            Do While ls0(i - 1) > s And i > 1
                i -= 1
            Loop

        Else

            Do While ls0(i) < s And i < iAltDim
                i += 1
            Loop

        End If


lbInt:

        If OverwiAlt Then iAlt = i

        Return (s - ls0(i - 1)) * (lAlt0(i) - lAlt0(i - 1)) / (ls0(i) - ls0(i - 1)) + lAlt0(i - 1)



    End Function
        

    Public ReadOnly Property Padd(ByVal t As Integer) As Single
        Get
            Return lPadd(t)
        End Get
    End Property

    Public ReadOnly Property a(ByVal t As Integer) As Single
        Get
            Return la(t)
        End Get
    End Property

    Public ReadOnly Property VairVres(ByVal t As Integer) As Single
        Get
            Return lVairVres(t)
        End Get
    End Property

    Public ReadOnly Property VairBeta(ByVal t As Integer) As Single
        Get
            Return lVairBeta(t)
        End Get
    End Property

    Public ReadOnly Property WegIst As Double
        Get
            Return dWegIst
        End Get
    End Property

    Public ReadOnly Property WegSoll As Double
        Get
            Return Weg(WegX)
        End Get
    End Property


End Class
