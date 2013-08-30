Imports System.Collections.Generic

Public Class cENG

    Public ModelName As String
    Public Pnenn As Single
    Public Displ As Single
    Public nnenn As Single
    Public nleerl As Single
    Public I_mot As Single

    Public fFLD As List(Of cSubPath)
    Private fMAP As cSubPath
    Private fWHTC As cSubPath
    Public FLDgears As List(Of String)

    Private MyPath As String
    Private sFilePath As String

    Public Sub New()
        MyPath = ""
        sFilePath = ""
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

        fFLD = New List(Of cSubPath)
        FLDgears = New List(Of String)

        fMAP.Clear()
        fWHTC.Clear()
    End Sub

    Public Function SaveFile() As Boolean
        Dim file As cFile_V3
        Dim i As Integer

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

        file.WriteLine("c Full load curves")
        For i = 0 To fFLD.Count - 1
            file.WriteLine(fFLD(i).PathOrDummy, FLDgears(i))
        Next

        file.WriteLine(sKey.Break)

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
        Dim line() As String
        Dim OldFile As Boolean = False
        Dim i As Integer

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


            i = -1
            Do While Not file.EndOfFile

                line = file.ReadLine
                i += 1

                If line(0) = sKey.Break Then Exit Do

                If i = 0 AndAlso UBound(line) < 1 Then OldFile = True

                fFLD.Add(New cSubPath)

                fFLD(i).Init(MyPath, line(0))

                If OldFile Then
                    FLDgears.Add("0 - 99")
                    Exit Do
                Else
                    FLDgears.Add(line(1))
                End If

            Loop

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

    Public Property PathFLD(ByVal x As Short, Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return fFLD(x).OriginalPath
            Else
                Return fFLD(x).FullPath
            End If
        End Get
        Set(ByVal value As String)
            fFLD(x).Init(MyPath, value)
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
