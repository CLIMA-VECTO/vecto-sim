Public Class cENG

    Public ModelName As String
    Public Pnenn As Single
    Public Displ As Single
    Public nnenn As Single
    Public nleerl As Single
    Public I_mot As Single

    Private fFLD As cSubPath
    Private fMAP As cSubPath
    Private fWHTC As cSubPath

    Private MyPath As String
    Private sFilePath As String

    Public Sub New()
        MyPath = ""
        sFilePath = ""
        fFLD = New cSubPath
        fMAP = New cSubPath
        fWHTC = New cSubPath
        SetDefault()
    End Sub

    Private Sub SetDefault()
        ModelName = "Undefined"
        Pnenn = 0
        Displ = 0
        nnenn = 0
        nleerl = 0
        I_mot = 0
        fFLD.Clear()
        fMAP.Clear()
        fWHTC.Clear()
    End Sub

    Public Function SaveFile() As Boolean
        Dim file As cFile_V3

        If sFilePath = "" Then Return False

        file = New cFile_V3

        If Not file.OpenWrite(sFilePath) Then Return False

        file.WriteLine("c VECTO Engine Input File")
        file.WriteLine("c VECTO " & VECTOvers)
        file.WriteLine("c " & Now.ToString)

        If Trim(ModelName) = "" Then ModelName = "Undefined"

        file.WriteLine("c Make & Model")
        file.WriteLine(ModelName.Replace(",", "\c\"))
        file.WriteLine("c Rated power [kW]")
        file.WriteLine(Pnenn.ToString)
        file.WriteLine("c Displacement [ccm]")
        file.WriteLine(Displ.ToString)
        file.WriteLine("c Rated speed [rpm]")
        file.WriteLine(nnenn.ToString)
        file.WriteLine("c Idling speed [rpm]")
        file.WriteLine(nleerl.ToString)
        file.WriteLine("c Inertia [kgm2]")
        file.WriteLine(I_mot.ToString)
        file.WriteLine("c Full load curve")
        file.WriteLine(fFLD.PathOrDummy)
        file.WriteLine("c Fuel map")
        file.WriteLine(fMAP.PathOrDummy)
        file.WriteLine("c WHTC test results")
        file.WriteLine(fWHTC.PathOrDummy)

        file.Close()

        Return True

    End Function

    Public Function ReadFile() As Boolean
        Dim MsgSrc As String
        Dim file As cFile_V3

        MsgSrc = "ENG/ReadFile"

        SetDefault()

        If sFilePath = "" Or Not IO.File.Exists(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Engine file not found (" & sFilePath & ") !", MsgSrc)
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
            Pnenn = CSng(file.ReadLine(0))
            Displ = CSng(file.ReadLine(0))
            nnenn = CSng(file.ReadLine(0))
            nleerl = CSng(file.ReadLine(0))
            I_mot = CSng(file.ReadLine(0))
            fFLD.Init(MyPath, file.ReadLine(0))
            fMAP.Init(MyPath, file.ReadLine(0))
            fWHTC.Init(MyPath, file.ReadLine(0))
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

    Public Property PathFLD(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return fFLD.OriginalPath
            Else
                Return fFLD.FullPath
            End If
        End Get
        Set(ByVal value As String)
            fFLD.Init(MyPath, value)
        End Set
    End Property

    Public Property PathMAP(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return fMAP.OriginalPath
            Else
                Return fMAP.FullPath
            End If
        End Get
        Set(ByVal value As String)
            fMAP.Init(MyPath, value)
        End Set
    End Property

    Public Property PathWHTC(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return fWHTC.OriginalPath
            Else
                Return fWHTC.FullPath
            End If
        End Get
        Set(ByVal value As String)
            fWHTC.Init(MyPath, value)
        End Set
    End Property


End Class
