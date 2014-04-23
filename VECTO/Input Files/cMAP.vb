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

    Private LnU As List(Of Single)
    Private LTq As List(Of Single)
    Private lFC As List(Of Single)

    Private sFilePath As String
    Private iMapDim As Integer

    Private FuelMap As cDelaunayMap

    Private Sub ResetMe()
        lFC = Nothing
        LTq = Nothing
        LnU = Nothing
        iMapDim = -1
        FuelMap = New cDelaunayMap
    End Sub

    Public Function ReadFile(Optional ByVal ShowMsg As Boolean = True) As Boolean
        Dim file As cFile_V3
        Dim line As String()
        Dim nU As Double
        Dim MsgSrc As String


        MsgSrc = "Main/ReadInp/MAP"

        'Reset
        ResetMe()

        'Stop if there's no file
        If sFilePath = "" OrElse Not IO.File.Exists(sFilePath) Then
            If ShowMsg Then WorkerMsg(tMsgID.Err, "Map file not found! (" & sFilePath & ")", MsgSrc)
            Return False
        End If

        'Open file
        file = New cFile_V3
        If Not file.OpenRead(sFilePath) Then
            file = Nothing
            If ShowMsg Then WorkerMsg(tMsgID.Err, "Failed to open file (" & sFilePath & ") !", MsgSrc)
            Return False
        End If

        'Initi Lists (before version check so ReadOldFormat works)
        lFC = New System.Collections.Generic.List(Of Single)
        LTq = New System.Collections.Generic.List(Of Single)
        LnU = New System.Collections.Generic.List(Of Single)

        Try
            Do While Not file.EndOfFile

                'Line read
                line = file.ReadLine

                'Line counter up (was reset in ResetMe)
                iMapDim += 1

                'Revolutions
                nU = CDbl(line(0))

                LnU.Add(nU)

                'Power
                LTq.Add(line(1))

                'FC
                'Check sign
                If CSng(line(2)) < 0 Then
                    file.Close()
                    If ShowMsg Then WorkerMsg(tMsgID.Err, "FC < 0 in map at " & nU & " [1/min], " & line(1) & " [Nm]", MsgSrc)
                    Return False
                End If

                lFC.Add(CSng(line(2)))


            Loop
        Catch ex As Exception

            If ShowMsg Then WorkerMsg(tMsgID.Err, "Error during file input! Line number " & iMapDim + 1 & " (" & sFilePath & ")", MsgSrc, sFilePath)
            GoTo lbEr

        End Try

        'Close file
        file.Close()

        file = Nothing

        Return True


        'ERROR-label for clean Abort
lbEr:
        file.Close()
        file = Nothing

        Return False

    End Function

    Public Function Triangulate() As Boolean
        Dim i As Integer

        Dim MsgSrc As String

        MsgSrc = "MAP/Norm"

        'FC Delauney
        For i = 0 To iMapDim
            FuelMap.AddPoints(LnU(i), LTq(i), lFC(i))
        Next

        Return FuelMap.Triangulate()

    End Function


    Public Function fFCdelaunay_Intp(ByVal nU As Single, ByVal Tq As Single) As Single
        Try
            Return FuelMap.Intpol(nU, Tq)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot extrapolate FC map! n= " & nU.ToString("0.0") & " [1/min], Me= " & Tq.ToString("0.0") & " [Nm]", "MAP/FC_Intp")
            Return -10000
        End Try
    End Function

#Region "Properties"

    Public Property FilePath() As String
        Get
            Return sFilePath
        End Get
        Set(ByVal value As String)
            sFilePath = value
        End Set
    End Property

    Public ReadOnly Property MapDim As Integer
        Get
            Return iMapDim
        End Get
    End Property

    Public ReadOnly Property Tq As List(Of Single)
        Get
            Return LTq
        End Get
    End Property

    Public ReadOnly Property FC As List(Of Single)
        Get
            Return lFC
        End Get
    End Property

    Public ReadOnly Property nU As List(Of Single)
        Get
            Return LnU
        End Get
    End Property

#End Region




End Class








