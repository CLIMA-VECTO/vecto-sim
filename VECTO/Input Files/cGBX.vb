Public Class cGBX

    Private MyPath As String
    Private sFilePath As String

    Public ModelName As String
    Public I_Getriebe As Single
    Public TracIntrSi As Single

    Public GetrI(16) As Single
    Private GetrMaps(16) As cSubPath

    Private iganganz As Short

    Public Sub New()
        Dim i As Short
        MyPath = ""
        sFilePath = ""
        For i = 0 To 16
            GetrI(i) = 0
            GetrMaps(i) = New cSubPath
        Next
        SetDefault()
    End Sub

    Private Sub SetDefault()
        Dim i As Integer
        ModelName = ""
        I_Getriebe = 0
        TracIntrSi = 0
        For i = 0 To 16
            GetrI(i) = 0
            GetrMaps(i).Clear()
        Next
        iganganz = 0
    End Sub

    Public Function SaveFile() As Boolean
        Dim file As cFile_V3
        Dim i As Integer
        file = New cFile_V3

        If sFilePath = "" Then Return False

        If Not file.OpenWrite(sFilePath) Then Return False

        file.WriteLine("c VECTO Gearbox Input File")
        file.WriteLine("c VECTO " & VECTOvers)
        file.WriteLine("c " & Now.ToString)

        If Trim(ModelName) = "" Then ModelName = "Undefined"

        file.WriteLine("c Make & Model")
        file.WriteLine(ModelName.Replace(",", "\c\"))
        file.WriteLine("c Gearbox rotational inertia [kgm2]")
        file.WriteLine(CStr(I_Getriebe))
        file.WriteLine("c Traction Interruption")
        file.WriteLine(CStr(TracIntrSi))

        file.WriteLine("c Ratio [-], Loss Map or Efficiency")
        For i = 0 To 16
            file.WriteLine("c Gear " & i)
            file.WriteLine(CStr(GetrI(i)), GetrMaps(i).PathOrDummy)
        Next

        file.Close()
        file = Nothing

        Return True

    End Function

    Public Function ReadFile() As Boolean
        Dim line() As String
        Dim file As cFile_V3
        Dim i As Integer
        Dim MsgSrc As String

        MsgSrc = "GBX/ReadFile"

        SetDefault()

        If sFilePath = "" Or Not IO.File.Exists(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Vehicle file not found (" & sFilePath & ") !", MsgSrc)
            Return False
        End If

        file = New cFile_V3

        If Not file.OpenRead(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Failed to open file (" & sFilePath & ") !", MsgSrc)
            file = Nothing
            Return False
        End If

        Try

            ModelName = file.ReadLine(0).Replace("\c\", ",")
            I_Getriebe = CSng(file.ReadLine(0))
            TracIntrSi = CSng(file.ReadLine(0))

            iganganz = 0
            For i = 0 To 16
                line = file.ReadLine
                GetrI(i) = CSng(line(0))
                GetrMaps(i).Init(MyPath, line(1))
                If GetrI(i) > 0.0001 Then iganganz = i
            Next

        Catch ex As Exception
            WorkerMsg(tMsgID.Err, ex.Message, MsgSrc)
            file.Close()
            Return False
        End Try


        file.Close()
        Return True


    End Function


    Public Property FilePath() As String
        Get
            Return sFilePath
        End Get
        Set(ByVal value As String)
            sFilePath = value
            MyPath = IO.Path.GetDirectoryName(sFilePath) & "\"
        End Set
    End Property

    Public Property GetrMap(ByVal x As Short, Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return GetrMaps(x).OriginalPath
            Else
                Return GetrMaps(x).FullPath
            End If
        End Get
        Set(ByVal value As String)
            GetrMaps(x).Init(MyPath, value)
        End Set
    End Property


End Class
