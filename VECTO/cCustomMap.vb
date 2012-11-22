Imports System.Collections.Generic

Public Class cCustomMap

    Private lX As List(Of Single)
    Private lY As List(Of Single)
    Private lZ As List(Of List(Of Single))
    Private lDim As Integer
    Private zDim As Integer
    Private Xmin As Single
    Private Xmax As Single
    Private Ymin As Single
    Private Ymax As Single

    Private Sub Reset()
        lX = New List(Of Single)
        lY = New List(Of Single)
        lZ = New List(Of List(Of Single))
        lDim = -1
        zDim = -1
        Xmin = 0
        Xmax = 0
        Ymin = 0
        Ymax = 0
    End Sub

    Public Sub Init(Optional ByVal zAnz As Integer = 1)
        Dim i As Integer

        Reset()

        zDim = zAnz - 1

        For i = 0 To zDim
            lZ.Add(New List(Of Single))
        Next

    End Sub

    Public Sub Add(ByVal x As Single, ByVal y As Single, ByVal z As Single)
        lX.Add(x)
        lY.Add(y)
        lZ(0).Add(z)
        lDim += 1
    End Sub

    Public Sub Add(ByVal Ar() As Single)
        Dim i As Integer

        lX.Add(Ar(0))
        lY.Add(Ar(1))

        For i = 0 To zDim
            lZ(i).Add(Ar(i + 2))
        Next

        lDim += 1

    End Sub

    Public Sub Add(ByVal StrAr() As String)
        Dim i As Integer

        lX.Add(CSng(StrAr(0)))
        lY.Add(CSng(StrAr(1)))

        For i = 0 To zDim
            lZ(i).Add(CSng(StrAr(i + 2)))
        Next

        lDim += 1

    End Sub

    Public Sub Norm()
        Dim i As Integer
        Dim x As Single
        Dim y As Single

        Xmin = lX(0)
        Xmax = lX(0)
        Ymin = lY(0)
        Ymax = lY(0)

        'Min/Max suchen
        For i = 1 To lDim

            x = lX(i)
            y = lY(i)

            If x < Xmin Then
                Xmin = x
            ElseIf x > Xmax Then
                Xmax = x
            End If

            If y < Ymin Then
                Ymin = y
            ElseIf y > Ymax Then
                Ymax = y
            End If

        Next

        'Normieren
        For i = 1 To lDim
            lX(i) = (lX(i) - Xmin) / (Xmax - Xmin)
            lY(i) = (lY(i) - Ymin) / (Ymax - Ymin)
        Next

    End Sub

    Public Function Intp(ByVal x As Single, ByVal y As Single, Optional ByVal zi As Integer = 0) As Single

        Dim wisum As Double = 0
        Dim sumo As Double = 0
        Dim ab As Double = 0
        Dim i As Int32

        x = (x - Xmin) / (Xmax - Xmin)
        y = (y - Ymin) / (Ymax - Ymin)

        For i = 0 To lDim

            'Wenn Vorzeichen von x oder y nicht gleich Vorzeichen von xA(i) oder yA(i) dann wird Zeile i übersprungen
            If x * lX(i) < 0 Or y * lY(i) < 0 Then Continue For

            ab = (x - lX(i)) ^ 2 + (y - lY(i)) ^ 2

            If ab = 0 Then
                Return lZ(zi)(i)
            Else
                sumo = sumo + (lZ(zi)(i)) / ab
                wisum = wisum + 1 / ab
            End If

        Next

        If wisum = 0 Then
            Return 0
        Else
            Return sumo / wisum
        End If

    End Function


End Class
