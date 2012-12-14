Imports System.Collections.Generic

Public Class cADV

    Private sFilePath As String

    Private MyPath As String

    Private fFZPpath As cSubPath
    Private fFLTpath As cSubPath
    Private fTEMpath As cSubPath
    Private fSTRpaths As List(Of cSubPath)
    Public SD3out As Boolean
    Public STRfilter As Boolean
    Public STRSUMdistflt As Single
    Public RndSeed As Short


    Public Sub New()

        sFilePath = ""
        MyPath = ""

        fFZPpath = New cSubPath
        fFLTpath = New cSubPath
        fTEMpath = New cSubPath
        fSTRpaths = New List(Of cSubPath)

    End Sub

    Public Function ReadFile() As Boolean
        Dim file As cFile_V3
        Dim SubPath As cSubPath

        If sFilePath = "" Then Return False
        If Not IO.File.Exists(sFilePath) Then Return False

        file = New cFile_V3

        SetDefault()

        If Not file.OpenRead(sFilePath) Then
            file = Nothing
            Return False
        End If


        fFZPpath.Init(MyPath, file.ReadLine(0))

        fFLTpath.Init(MyPath, file.ReadLine(0))

        fTEMpath.Init(MyPath, file.ReadLine(0))

        RndSeed = CShort(file.ReadLine(0))

        SD3out = CBool(file.ReadLine(0))

        STRfilter = CBool(file.ReadLine(0))

        STRSUMdistflt = CSng(file.ReadLine(0))

        Do While Not file.EndOfFile
            SubPath = New cSubPath
            SubPath.Init(MyPath, file.ReadLine(0))
            fSTRpaths.Add(SubPath)
        Loop


lbClose:

        file.Close()
        file = Nothing

        Return True

    End Function

    Public Function SaveFile() As Boolean
        Dim fADV As New cFile_V3
        Dim SubPath As cSubPath

        If Not fADV.OpenWrite(sFilePath) Then Return False

        'Line 1: FZP file
        fADV.WriteLine(fFZPpath.PathOrDummy)

        'Line 2: FLT file
        fADV.WriteLine(fFLTpath.PathOrDummy)

        'Line 3: TEM file
        fADV.WriteLine(fTEMpath.PathOrDummy)

        'Line 4: RndSeed
        fADV.WriteLine(RndSeed.ToString)

        'Line 5: MISKAMout True/False|
        fADV.WriteLine(Math.Abs(CInt(SD3out)))

        'Line 6: STRfilter True/False
        fADV.WriteLine(Math.Abs(CInt(STRfilter)))

        'Line 7: Distance filters for SUM.STR
        fADV.WriteLine(STRSUMdistflt.ToString)

        'Line 8 +: STR files
        For Each SubPath In fSTRpaths
            fADV.WriteLine(SubPath.PathOrDummy)
        Next

        fADV.Close()
        fADV = Nothing

        Return True

    End Function

    Private Sub SetDefault()

        SD3out = False
        STRfilter = False
        STRSUMdistflt = 85
        RndSeed = 0

        STRpathsClear()

    End Sub

    Public Property FilePath() As String
        Get
            Return sFilePath
        End Get
        Set(ByVal value As String)
            sFilePath = value
            MyPath = IO.Path.GetDirectoryName(sFilePath) & "\"
        End Set
    End Property

    Public Property FZPpath(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return fFZPpath.OriginalPath
            Else
                Return fFZPpath.FullPath
            End If
        End Get
        Set(ByVal value As String)
            fFZPpath.Init(MyPath, value)
        End Set
    End Property

    Public Property FLTpath(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return fFLTpath.OriginalPath
            Else
                Return fFLTpath.FullPath
            End If
        End Get
        Set(ByVal value As String)
            fFLTpath.Init(MyPath, value)
        End Set
    End Property

    Public Property TEMpath(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return fTEMpath.OriginalPath
            Else
                Return fTEMpath.FullPath
            End If
        End Get
        Set(ByVal value As String)
            fTEMpath.Init(MyPath, value)
        End Set
    End Property

#Region "STR-Dateien"
    Public ReadOnly Property STRpaths(ByVal index As Short, Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return fSTRpaths(index).OriginalPath
            Else
                Return fSTRpaths(index).FullPath
            End If
        End Get
    End Property

    Public Sub STRpathsAdd(ByVal path As String)
        Dim SubPath As cSubPath

        SubPath = New cSubPath

        SubPath.Init(MyPath, path)

        fSTRpaths.Add(SubPath)

    End Sub

    Public Sub STRpathsClear()
        fSTRpaths.Clear()
    End Sub

    Public ReadOnly Property STRcount As Integer
        Get
            Return fSTRpaths.Count
        End Get
    End Property

#End Region






End Class
