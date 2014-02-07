Imports System.Collections.Generic

Public Class cENG

    Private Const FormatVersion As String = "1.0"
    Private FileVersion As String

    Public ModelName As String
    Public Displ As Single
    Public nleerl As Single
    Public I_mot As Single

    Public fFLD As List(Of cSubPath)
    Private fMAP As cSubPath
    Private fWHTC As cSubPath
    Public FLDgears As List(Of String)

    Private MyPath As String
    Private sFilePath As String

    Public NoJSON As Boolean

    Private MyFileList As List(Of String)


    Public Function CreateFileList() As Boolean
        Dim sb As cSubPath

        If Not Me.ReadFile Then Return False

        MyFileList = New List(Of String)

        For Each sb In Me.fFLD
            MyFileList.Add(sb.FullPath)
        Next

        MyFileList.Add(PathMAP)

        'Not used!!! MyFileList.Add(PathWHTC)

        Return True

    End Function

    Public Sub New()
        MyPath = ""
        sFilePath = ""
        fMAP = New cSubPath
        fWHTC = New cSubPath
        SetDefault()
    End Sub

    Private Sub SetDefault()
        ModelName = "Undefined"
        Displ = 0
        nleerl = 0
        I_mot = 0

        fFLD = New List(Of cSubPath)
        FLDgears = New List(Of String)

        fMAP.Clear()
        fWHTC.Clear()
    End Sub

    Private Function SaveFileOld() As Boolean
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
        file.WriteLine("c NOT USED")
        file.WriteLine("0")
        file.WriteLine("c Displacement [ccm]")
        file.WriteLine(Displ.ToString)
        file.WriteLine("c NOT USED")
        file.WriteLine("0")
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

    Private Function ReadFileOld() As Boolean
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
            file.ReadLine()  'NOT USED (Pnenn)
            Displ = CSng(file.ReadLine(0))
            file.ReadLine()  'NOT USED (nnenn)
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

    Public Function SaveFile() As Boolean
        Dim i As Integer
        Dim JSON As New cJSON
        Dim dic As Dictionary(Of String, Object)
        Dim dic0 As Dictionary(Of String, Object)
        Dim ls As List(Of Object)

        If Not Cfg.JSON Then Return SaveFileOld()

        'Header
        dic = New Dictionary(Of String, Object)
        dic.Add("CreatedBy", Lic.LicString & " (" & Lic.GUID & ")")
        dic.Add("Date", Now.ToString)
        dic.Add("AppVersion", VECTOvers)
        dic.Add("FileVersion", FormatVersion)
        JSON.Content.Add("Header", dic)

        'Body
        dic = New Dictionary(Of String, Object)

        dic.Add("ModelName", ModelName)

        dic.Add("Displacement", Displ)
        dic.Add("IdlingSpeed", nleerl)
        dic.Add("Inertia", I_mot)

        ls = New List(Of Object)
        For i = 0 To fFLD.Count - 1
            dic0 = New Dictionary(Of String, Object)
            dic0.Add("Path", fFLD(i).PathOrDummy)
            dic0.Add("Gears", FLDgears(i))
            ls.Add(dic0)
        Next
        dic.Add("FullLoadCurves", ls)

        dic.Add("FuelMap", fMAP.PathOrDummy)
        dic.Add("WHTCresults", fWHTC.PathOrDummy)

        JSON.Content.Add("Body", dic)
   
        Return JSON.WriteFile(sFilePath)


    End Function

    Public Function ReadFile() As Boolean
        Dim MsgSrc As String
        Dim i As Integer
        Dim JSON As New cJSON
        Dim dic As Object

        MsgSrc = "ENG/ReadFile"

        'Flag for "File is not JSON" Warnings        
        NoJSON = False

        SetDefault()

        If Cfg.JSON Then
            If Not JSON.ReadFile(sFilePath) Then
                NoJSON = True
                Try
                    Return ReadFileOld()
                Catch ex As Exception
                    Return False
                End Try
            End If
        Else
            Try
                Return ReadFileOld()
            Catch ex As Exception
                Return False
            End Try
        End If

        Try

            FileVersion = JSON.Content("Header")("FileVersion")

            ModelName = JSON.Content("Body")("ModelName")

            Displ = JSON.Content("Body")("Displacement")
            nleerl = JSON.Content("Body")("IdlingSpeed")
            I_mot = JSON.Content("Body")("Inertia")

            i = -1
            For Each dic In JSON.Content("Body")("FullLoadCurves")
                i += 1
                fFLD.Add(New cSubPath)
                fFLD(i).Init(MyPath, dic("Path"))
                FLDgears.Add(dic("Gears"))
            Next

            fMAP.Init(MyPath, JSON.Content("Body")("FuelMap"))
            fWHTC.Init(MyPath, JSON.Content("Body")("WHTCresults"))

        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to read VECTO file! " & ex.Message, MsgSrc)
            Return False
        End Try

        Return True

    End Function



    Public ReadOnly Property FileList As List(Of String)
        Get
            Return MyFileList
        End Get
    End Property

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
