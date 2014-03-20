Imports System.Collections.Generic

Public Class cMAP

    'Private Const FormatVersion As Integer = 1
    'Private FileVersion As Integer

    Public lFC As List(Of Single)

    Public LPe As List(Of Single)
    Public LnU As List(Of Single)

    Private sFilePath As String
    Private iMapDim As Integer

    Private MapIntp As cMapInterpol

    Private FuelMap As cDelaunayMap

    Private Sub ResetMe()
        MapIntp = Nothing
        lFC = Nothing
        LPe = Nothing
        LnU = Nothing
        iMapDim = -1
        FuelMap = New cDelaunayMap
    End Sub

    Public Function ReadFile(Optional ByVal MsgOutput As Boolean = True) As Boolean
        Dim file As cFile_V3
        Dim line As String()
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
        lFC = New System.Collections.Generic.List(Of Single)
        LPe = New System.Collections.Generic.List(Of Single)
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
                LPe.Add(nMtoPe(nU, CDbl(line(1))))

                'FC
                'Check sign
                If CSng(line(2)) < 0 Then
                    file.Close()
                    WorkerMsg(tMsgID.Err, "FC < 0 in map at " & nU & " [1/min], " & line(1) & " [Nm]", MsgSrc)
                    Return False
                End If

                lFC.Add(CSng(line(2)))


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

        Return True


        'ERROR-label for clean Abort
lbEr:
        file.Close()
        file = Nothing

        Return False

    End Function

    Public Sub Norm()
        Dim i As Integer

        Dim MsgSrc As String

        MsgSrc = "MAP/Norm"

        'FC Delauney
        For i = 0 To iMapDim
            FuelMap.AddPoints(LnU(i), LPe(i), lFC(i))
        Next

        FuelMap.Triangulate()

    End Sub


    Public Function fFCdelaunay_Intp(ByVal nU As Single, ByVal Pe As Single) As Single
        Try
            Return FuelMap.Intpol(nU, Pe)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot extrapolate FC map! n= " & nU.ToString("0") & " [1/min], Me= " & nPeToM(nU, Pe).ToString("0.0") & " [Nm]", "MAP/FC_Intp")
            Return -10000
        End Try
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

#End Region




End Class








