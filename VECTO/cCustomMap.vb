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

        'Search Min/Max
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

        'Normalize
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

            'When sign of x and y is not equal to the sign of xA(i) and yA(i) respectively, then skip Row i
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
