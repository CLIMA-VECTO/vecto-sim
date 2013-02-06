Imports System.Collections.Generic

Public Class cVh

    'From DRI file
    Private lV As List(Of Single)       'Ist-Geschw. in Zwischensekunden.
    Private lV0ogl As List(Of Single)   'Original DRI-Geschwindigkeit. Wird nicht geändert.
    Private lV0 As List(Of Single)      'DRI-Geschwindigkeit. Bei Geschw.-Reduktion in Zeitschritt t wird LV0(t+1) reduziert.
    Private lGrad As List(Of Single)
    Private lGears As List(Of Short)
    Private lPadd As List(Of Single)
    Private lVairVres As List(Of Single)
    Private lVairBeta As List(Of Single)

    'Calculated
    Private la As List(Of Single)

    'WegKor |@@| Route(Weg)Correct
    Private WegIst As Single
    Private Weg As List(Of Single)
    Private WegX As Integer


    Public Sub Init()
        lV = New List(Of Single)
        lV0ogl = New List(Of Single)
        lV0 = New List(Of Single)
        lGrad = New List(Of Single)
        lGears = New List(Of Short)
        lPadd = New List(Of Single)
        la = New List(Of Single)
        Weg = New List(Of Single)
        lVairVres = New List(Of Single)
        lVairBeta = New List(Of Single)
        WegX = 0
        WegIst = 0
    End Sub

    Public Sub CleanUp()
        lV = Nothing
        lV0ogl = Nothing
        lV0 = Nothing
        lGrad = Nothing
        lGears = Nothing
        lPadd = Nothing
        la = Nothing
        Weg = Nothing
        lVairVres = Nothing
        lVairBeta = Nothing
    End Sub

    Public Sub VehCylceInit()

        Dim s As Integer
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
                    lV0ogl.Add(CSng(DRI.VoglS(s)) / 3.6)
                Next
            Else
                lV0ogl.Add(DRI.Values(tDriComp.V)(MODdata.tDim + 1))
            End If

            'Strecke (aus Zwischensekunden sonst passiert Fehler) |@@| Segment (from Intermediate-seconds, otherwise Error)
            Weg.Add(lV(0))
            For s = 1 To MODdata.tDim
                Weg.Add(Weg(s - 1) + lV(s))
            Next

        End If

        'Slope
        If DRI.GradVorg Then
            L = DRI.Values(tDriComp.Grad)
            For s = 0 To MODdata.tDim
                lGrad.Add((L(s + 1) + L(s)) / 2)
            Next
        Else
            For s = 0 To MODdata.tDim
                lGrad.Add(0)
            Next
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

            'Strecke  |@@| Segment
            Weg.Add(lV(0))
            For s = 1 To MODdata.tDim
                Weg.Add(Weg(s - 1) + lV(s))
            Next

        End If

        'Slope
        If DRI.GradVorg Then
            L = DRI.Values(tDriComp.Grad)
            For s = 0 To MODdata.tDim
                lGrad.Add(L(s))
            Next
        Else
            For s = 0 To MODdata.tDim
                lGrad.Add(0)
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

    Public Sub DistCorrection(ByVal t As Integer, ByVal VehState As tVehState)

        'TODO: If veh faster than cycle ...

        Dim v As Single
        Dim a As Single

        If t + 1 > MODdata.tDim Then Exit Sub

        v = lV(t)
        a = la(t)

        WegIst += v

        If WegX < MODdata.tDimOgl Then



            'Falls Zeitschritt wiederholen näher an Wegvorgabe als aktueller Weg => Zeitschritt wiederholen |@@| If the repeating Time-step is closer to the Specified-route than the Actual-route => Repeat Time-step
            If (VehState = tVehState.Acc Or VehState = tVehState.Cruise) AndAlso (Math.Abs(WegIst + Vsoll(t) - Weg(WegX)) < Math.Abs(WegIst - Weg(WegX))) And v > 0.1 Then

                Duplicate(t + 1)
                MODdata.tDim += 1

                'Falls nächsten Zeitschritt löschen näher an Wegvorgabe als aktueller Weg => Nächsten Zeitschritt löschen |@@| If the next Time-step to Delete closer to specified Route than the Actual-route => Delete Next Time-step
                'ElseIf WegX < MODdata.tDimOgl - 1 AndAlso t < MODdata.tDim - 1 AndAlso WegIst > Weg(WegX + 1) AndAlso Math.Abs(WegIst - Weg(WegX + 1)) <= Math.Abs(WegIst - Weg(WegX)) Then

                '    Do
                '        Cut(t + 1)
                '        MODdata.tDim -= 1
                '        WegX += 1
                '    Loop While WegX < MODdata.tDimOgl - 1 AndAlso t < MODdata.tDim - 1 AndAlso WegIst > Weg(WegX + 1) AndAlso Math.Abs(WegIst - Weg(WegX + 1)) <= Math.Abs(WegIst - Weg(WegX))
                '    WegX += 1

            Else

                'No correction
                WegX += 1

            End If
        End If

    End Sub

    Private Sub Duplicate(ByVal t As Integer)

        lV0.Insert(t + 1, lV0ogl(t + 1))
        lV0ogl.Insert(t + 1, lV0ogl(t + 1))
        lV.Insert(t, (lV0(t + 1) + lV0(t)) / 2)
        la.Insert(t, lV0(t + 1) - lV0(t))

        If t < MODdata.tDim Then
            lV(t + 1) = (lV0(t + 2) + lV0(t + 1)) / 2
            la(t + 1) = lV0(t + 2) - lV0(t + 1)
        End If

        lGrad.Insert(t, lGrad(t))
        lGears.Insert(t, lGears(t))
        lPadd.Insert(t, lPadd(t))

        If DRI.VairVorg Then
            lVairVres.Insert(t, lVairVres(t))
            lVairBeta.Insert(t, lVairBeta(t))
        End If

        MODdata.Duplicate(t)

        If PHEMmode = tPHEMmode.ModeADVANCE Then
            ADV.aWorldX.Insert(t, ADV.aWorldX(t))
            ADV.aWorldY.Insert(t, ADV.aWorldY(t))
            ADV.aStrId.Insert(t, ADV.aStrId(t))
        End If


    End Sub

    Private Sub Cut(ByVal t As Integer)

        lV0.RemoveAt(t + 1)
        lV0ogl.RemoveAt(t + 1)
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

        lGrad.RemoveAt(t)
        lGears.RemoveAt(t)
        lPadd.RemoveAt(t)

        If DRI.VairVorg Then
            lVairVres.RemoveAt(t)
            lVairBeta.RemoveAt(t)
        End If

        MODdata.Cut(t)

        If PHEMmode = tPHEMmode.ModeADVANCE Then
            ADV.aWorldX.RemoveAt(t)
            ADV.aWorldY.RemoveAt(t)
            ADV.aStrId.RemoveAt(t)
        End If

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

    Public ReadOnly Property Grad(ByVal t As Integer) As Single
        Get
            Return lGrad(t)
        End Get
    End Property

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

End Class
