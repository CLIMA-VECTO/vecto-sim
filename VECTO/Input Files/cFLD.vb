Imports System.Collections.Generic

Public Class cFLD

    'Private Const FormatVersion As Integer = 1
    'Private FileVersion As Integer

    Private sFilePath As String

    Private LPfull As List(Of Single)
    Private LPdrag As List(Of Single)
    Private LnU As List(Of Single)
    Private LPT1 As List(Of Single)

    Private LPtarget As List(Of Single)

    Private iDim As Integer

    Private Sub ResetMe()
        LPfull = Nothing
        LPdrag = Nothing
        LnU = Nothing
        LPT1 = Nothing
        iDim = -1
    End Sub

    Public Function ReadFile() As Boolean
        Dim file As cFile_V3
        Dim line As String()
        Dim s1 As Integer
        Dim sPT1 As Integer
        Dim PT1 As Single
        Dim nU As Double
        Dim MsgSrc As String

        MsgSrc = "Main/ReadInp/FLD"

        sPT1 = -1
        PT1 = 0       '=> Defaultwert falls nicht in FLD vorgegeben

        'Reset
        ResetMe()

        'Stop if there's no file
        If sFilePath = "" OrElse Not IO.File.Exists(sFilePath) Then
            WorkerMsg(tMsgID.Err, "FLD file '" & sFilePath & "' not found!", MsgSrc)
            Return False
        End If

        'Open file
        file = New cFile_V3
        If Not file.OpenRead(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Failed to open file (" & sFilePath & ") !", MsgSrc)
            file = Nothing
            Return False
        End If

        'Initialize Lists
        LPfull = New System.Collections.Generic.List(Of Single)
        LPdrag = New System.Collections.Generic.List(Of Single)
        LnU = New System.Collections.Generic.List(Of Single)
        LPT1 = New System.Collections.Generic.List(Of Single)


        'VECTO: No Header/Unit column. Always PT1!
        s1 = 3
        sPT1 = 3

        'From Line 4: Values
        Try

            Do While Not file.EndOfFile

                'Read Line
                line = file.ReadLine

                'VECTO: M => Pe
                nU = CDbl(line(0))

                LnU.Add(nU)
                LPfull.Add(nMtoPe(nU, CDbl(line(1))))
                LPdrag.Add(nMtoPe(nU, CDbl(line(2))))

                'If PT1 not given, use default value (see above)
                If sPT1 > -1 Then

                    PT1 = CSng(line(sPT1))

                    If PT1 < 0 Then
                        WorkerMsg(tMsgID.Err, "PT1 value invalid! line " & iDim + 1, MsgSrc)
                        PT1 = 0
                    End If

                End If

                LPT1.Add(PT1)

                'Line-counter up (was reset in ResetMe)
                iDim += 1

            Loop

        Catch ex As Exception

            WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & iDim + 1 & " (" & sFilePath & ")", MsgSrc, sFilePath)
            GoTo lbEr

        End Try


        'Close file
        file.Close()

        Return True


        'ERROR-label for clean Abort
lbEr:
        file.Close()
        file = Nothing

        Return False

    End Function

    Public Function Pdrag(ByVal nU As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If LnU(0) >= nU Then
            If LnU(0) > nU Then MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While LnU(i) < nU And i < iDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If LnU(i) < nU Then
            MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
        End If

lbInt:
        'Interpolation
        Return (nU - LnU(i - 1)) * (LPdrag(i) - LPdrag(i - 1)) / (LnU(i) - LnU(i - 1)) + LPdrag(i - 1)

    End Function

    Public Function Pfull(ByVal nU As Single, ByVal LastPe As Single) As Single
        Dim i As Int32
        Dim PfullStat As Single
        Dim PT1 As Single

        'Extrapolation for x < x(1)
        If LnU(0) >= nU Then
            If LnU(0) > nU Then MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While LnU(i) < nU And i < iDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If LnU(i) < nU Then
            MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
        End If

lbInt:
        'Interpolation
        PfullStat = (nU - LnU(i - 1)) * (LPfull(i) - LPfull(i - 1)) / (LnU(i) - LnU(i - 1)) + LPfull(i - 1)
        PT1 = (nU - LnU(i - 1)) * (LPT1(i) - LPT1(i - 1)) / (LnU(i) - LnU(i - 1)) + LPT1(i - 1)

        'Dynamic Full-load
        Return Math.Min((1 / (PT1 + 1)) * (PfullStat + PT1 * LastPe), PfullStat)

    End Function

    Public Function Pfull(ByVal nU As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If LnU(0) >= nU Then
            If LnU(0) > nU Then MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While LnU(i) < nU And i < iDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If LnU(i) < nU Then
            MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
        End If

lbInt:
        'Interpolation
        Return (nU - LnU(i - 1)) * (LPfull(i) - LPfull(i - 1)) / (LnU(i) - LnU(i - 1)) + LPfull(i - 1)
    End Function

    Public Function nRated() As Single
        Dim i As Int16
        Dim PeMax As Single
        Dim nU As Single

        PeMax = LPfull(0)
        nU = LnU(0)
        For i = 1 To iDim
            If LPfull(i) >= PeMax Then
                PeMax = LPfull(i)
                nU = LnU(i)
            End If
        Next

        Return nU

    End Function

    Public Property FilePath() As String
        Get
            Return sFilePath
        End Get
        Set(ByVal value As String)
            sFilePath = value
        End Set
    End Property


End Class
