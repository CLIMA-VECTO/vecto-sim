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

Public Class cHEVctrl

    Public STEfile As String

    Private lSOC As List(Of Single)
    Private lSTE As List(Of Single)
    Private strDim As Integer

    Public vEVu As Single  '[m/s]
    Public vEVo As Single  '[m/s]
    Public SOC_OW As Single
    Public SOC_UW As Single

    Public Sub New()
        lSOC = New List(Of Single)
        lSTE = New List(Of Single)
    End Sub

    Public Sub CleanUp()
        lSOC = Nothing
        lSTE = Nothing
    End Sub

    Public Function readSTE() As Boolean
        Dim file As cFile_V3
        Dim line As String()

        file = New cFile_V3

        If Not file.OpenRead(STEfile) Then
            Return False
        End If

        lSOC.Clear()
        lSTE.Clear()
        strDim = -1

        vEVu = file.ReadLine(0) / 3.6
        vEVo = file.ReadLine(0) / 3.6
        SOC_UW = file.ReadLine(0)
        SOC_OW = file.ReadLine(0)

        Do While Not file.EndOfFile
            strDim += 1
            line = file.ReadLine
            lSOC.Add(CSng(line(0)))
            lSTE.Add(CSng(line(1)))
        Loop

        file.Close()

        Return True

    End Function

    Public Function STEintp(ByVal SOC As Single) As Single
        Dim i As Int32

        'Extrapolation für x < x(1)
        If lSOC(0) >= SOC Then
            ' ....Int1D_ERROR = True
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While lSOC(i) < SOC And i < strDim
            i += 1
        Loop

        'Extrapolation für x > x(imax)
        If lSOC(i) < SOC Then
            ' ....Int1D_ERROR = True
        End If

lbInt:
        'Interpolation
        Return ((SOC - lSOC(i - 1)) * (lSTE(i) - lSTE(i - 1)) / (lSOC(i) - lSOC(i - 1)) + lSTE(i - 1))

    End Function




End Class
