Imports System.Collections.Generic

Public Class cENG

    Private Const FormatVersion As String = "1.0"
    Private FileVersion As String

    Public ModelName As String
    Public Displ As Single
    Public Nidle As Single
    Public I_mot As Single

    Public fFLD As List(Of cSubPath)
    Private fMAP As cSubPath
    Public FLDgears As List(Of String)

    Private MyPath As String
    Private sFilePath As String

    Public NoJSON As Boolean

    Private MyFileList As List(Of String)
    Public WHTCurban As Single
    Public WHTCrural As Single
    Public WHTCmw As Single

    Public Nrated As Single
    Public Nlo As Single
    Public Npref As Single
    Public N95h As Single
    Public Nhi As Single


    Public Function CreateFileList() As Boolean
        Dim sb As cSubPath

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
        SetDefault()
    End Sub

    Private Sub SetDefault()
        ModelName = "Undefined"
        Displ = 0
        Nidle = 0
        I_mot = 0
        Nrated = 0

        fFLD = New List(Of cSubPath)
        FLDgears = New List(Of String)

        fMAP.Clear()

        WHTCurban = 0
        WHTCrural = 0
        WHTCmw = 0


    End Sub

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
            Nidle = CSng(file.ReadLine(0))
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
        dic.Add("IdlingSpeed", Nidle)
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

        dic.Add("WHTC-Urban", WHTCurban)
        dic.Add("WHTC-Rural", WHTCrural)
        dic.Add("WHTC-Motorway", WHTCmw)


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


        If Not JSON.ReadFile(sFilePath) Then
            NoJSON = True
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
            Nidle = JSON.Content("Body")("IdlingSpeed")
            I_mot = JSON.Content("Body")("Inertia")

            i = -1
            For Each dic In JSON.Content("Body")("FullLoadCurves")
                i += 1
                fFLD.Add(New cSubPath)
                fFLD(i).Init(MyPath, dic("Path"))
                FLDgears.Add(dic("Gears"))
            Next

            fMAP.Init(MyPath, JSON.Content("Body")("FuelMap"))

            If Not JSON.Content("Body")("WHTC-Urban") Is Nothing Then
                WHTCurban = CSng(JSON.Content("Body")("WHTC-Urban"))
                WHTCrural = CSng(JSON.Content("Body")("WHTC-Rural"))
                WHTCmw = CSng(JSON.Content("Body")("WHTC-Motorway"))
            End If

        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to read VECTO file! " & ex.Message, MsgSrc)
            Return False
        End Try

        Return True

    End Function

    Public Function Init() As Boolean
        Dim Pmax As Single
        Dim fl As cFLD
        Dim fldgear As Dictionary(Of Integer, String)
        Dim fldgFromTo As String()
        Dim str As String
        Dim i As Integer
        Dim j As Integer
        Dim nr As Single

        Dim MsgSrc As String
        MsgSrc = "ENG/Init"

        'Read FLDs and MAP
        FLD = New List(Of cFLD)

        If FLDgears.Count = 0 Then
            WorkerMsg(tMsgID.Err, "No .vfld file defined in Engine file!", MsgSrc, "<GUI>" & sFilePath)
            Return False
        End If

        fldgear = New Dictionary(Of Integer, String)

        Try
            j = -1
            For Each str In FLDgears

                j += 1
                If str.Contains("-") Then
                    fldgFromTo = str.Replace(" ", "").Split("-")
                Else
                    fldgFromTo = New String() {str, str}
                End If

                For i = CInt(fldgFromTo(0)) To CInt(fldgFromTo(1))

                    If i > GBX.GearCount Then Exit For

                    If i < 0 Or i > 99 Then
                        WorkerMsg(tMsgID.Err, "Cannot assign .vfld file to gear " & i & "!", MsgSrc, "<GUI>" & sFilePath)
                        Return False
                    End If

                    If fldgear.ContainsKey(i) Then
                        WorkerMsg(tMsgID.Err, "Multiple .vfld files are assigned to gear " & i & "!", MsgSrc, "<GUI>" & sFilePath)
                        Return False
                    End If

                    fldgear.Add(i, PathFLD(j))

                Next

            Next
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to process engine file '" & sFilePath & "'!", MsgSrc)
            Return False
        End Try


        For i = 0 To GBX.GearCount

            If Not fldgear.ContainsKey(i) Then
                WorkerMsg(tMsgID.Err, "No .vfld file assigned to gear " & i & "!", MsgSrc, "<GUI>" & sFilePath)
                Return False
            End If

            FLD.Add(New cFLD)
            FLD(i).FilePath = fldgear(i)

            Try
                If Not FLD(i).ReadFile Then Return False 'Fehlermeldung hier nicht notwendig weil schon von in ReadFile
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & fldgear(i) & ")", MsgSrc, fldgear(i))
                Return False
            End Try

        Next

        'Kennfeld read
        MAP = New cMAP
        MAP.FilePath = PathMAP

        Try
            If Not MAP.ReadFile Then Return False 'Fehlermeldung hier nicht notwendig weil schon von in ReadFile
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "File read error! (" & PathMAP & ")", MsgSrc, PathMAP)
            Return False
        End Try

        'Normalize
        MAP.Norm()

        Nrated = 0
        For Each fl In FLD
            nr = fl.fnUrated
            If Nrated < nr Then Nrated = nr
        Next

        'Special rpms for Shift Model
        fl = FLD(FLD.Count - 1)

        Pmax = fl.Pfull(fl.fnUrated)

        Nlo = fl.fnUofPfull(0.55 * Pmax, True)

        N95h = fl.fnUofPfull(0.95 * Pmax, False)

        Npref = fl.Npref

        Nhi = fl.fnUofPfull(0.7 * Pmax, False)


        Return True

    End Function

    Public Sub DeclInit()

        I_mot = Declaration.GetEngInertia(Displ)

    End Sub

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

End Class
