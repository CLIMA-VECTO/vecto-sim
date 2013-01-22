Imports System.Collections.Generic

Public Class cGBX

    Private MyPath As String
    Private sFilePath As String

    Public ModelName As String
    Public I_Getriebe As Single
    Public TracIntrSi As Single

    Public GetrI(16) As Single
    Private GetrMaps(16) As cSubPath

    Private iganganz As Short

    'Gear shift polygons
    Private gs_file As New cSubPath
    Private gs_M As New List(Of Single)
    Private gs_nnUp As New List(Of Single)
    Private gs_nnDown As New List(Of Single)
    Private gs_Dim As Integer
    Public gs_TorqueResv As Single
    Public gs_SkipGears As Boolean
    Public gs_ShiftTime As Integer
    Public gs_TorqueResvStart As Single
    Public gs_StartSpeed As Single
    Public gs_StartAcc As Single
    'Public gs_Type As tGearbox 'not used yet


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
        gs_M.Clear()
        gs_nnDown.Clear()
        gs_nnUp.Clear()
        gs_file.Clear()
        gs_Dim = -1
        gs_TorqueResv = 0
        gs_SkipGears = False
        gs_ShiftTime = 0
        gs_TorqueResvStart = 0
        gs_StartSpeed = 0
        gs_StartAcc = 0

        'gs_Type = tGearbox.Manual
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

        file.WriteLine("c Gear shift polygons file")
        file.WriteLine(gs_file.PathOrDummy)
        file.WriteLine("c Torque Reserve [%]")
        file.WriteLine(CStr(gs_TorqueResv))
        file.WriteLine("c Skip gears")
        file.WriteLine(CStr(Math.Abs(CInt(gs_SkipGears))))
        file.WriteLine("c Minimum time between two gear shifts [s]")
        file.WriteLine(CStr(gs_ShiftTime))
        file.WriteLine("c Start Torque Reserve [%]")
        file.WriteLine(CStr(gs_TorqueResvStart))
        file.WriteLine("c Vehicle speed with clutch fully closed [m/s]")
        file.WriteLine(CStr(gs_StartSpeed))
        file.WriteLine("c Acceleration during start [m/s2]")
        file.WriteLine(CStr(gs_StartAcc))
        'file.WriteLine("c Gearbox Type")
        'file.WriteLine(CStr(CType(gs_Type, Integer)))


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

            'Allow file end here to keep compatibility to older versions
            If Not file.EndOfFile Then
                gs_file.Init(MyPath, file.ReadLine(0))
                gs_TorqueResv = CSng(file.ReadLine(0))
                gs_SkipGears = CBool(CInt(file.ReadLine(0)))
                gs_ShiftTime = CInt(file.ReadLine(0))
                gs_TorqueResvStart = CSng(file.ReadLine(0))
                gs_StartSpeed = CSng(file.ReadLine(0))
                gs_StartAcc = CSng(file.ReadLine(0))
                'gs_Type = CType(CInt(file.ReadLine(0)), tGearbox)
            End If

        Catch ex As Exception
            WorkerMsg(tMsgID.Err, ex.Message, MsgSrc)
            file.Close()
            Return False
        End Try


        file.Close()
        Return True


    End Function

    Public Function GSinit() As Boolean
        Dim file As cFile_V3
        Dim line As String()
        Dim i As Integer

        Dim MsgSrc As String

        MsgSrc = "GBX/GSinit"

        'Abort if Polygon Model is not activated
        If Not DEV.UseGearShiftPolygon Then Return True

        'Check if file exists
        If Not IO.File.Exists(gs_file.FullPath) Then
            WorkerMsg(tMsgID.Err, "Gear Shift Polygon File not found! '" & gs_file.FullPath & "'", MsgSrc)
            Return False
        End If

        'Init file instance
        file = New cFile_V3

        'Open file
        If Not file.OpenRead(gs_file.FullPath) Then
            WorkerMsg(tMsgID.Err, "Failed to load Gear Shift Polygon File! '" & gs_file.FullPath & "'", MsgSrc)
            Return False
        End If

        'Clear lists
        gs_M.Clear()
        gs_nnDown.Clear()
        gs_nnUp.Clear()
        gs_Dim = -1

        'Read file
        Try
            Do While Not file.EndOfFile
                line = file.ReadLine
                gs_Dim += 1
                gs_M.Add(CSng(line(0)))
                gs_nnDown.Add(CSng(line(1)))
                gs_nnUp.Add(CSng(line(2)))
            Loop
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Error while reading Gear Shift Polygon File! (" & ex.Message & ")", MsgSrc)
            Return False
        End Try

        'Check if more then one point
        If gs_Dim < 1 Then
            WorkerMsg(tMsgID.Err, "More points in Gear Shift Polygon File needed!", MsgSrc)
            Return False
        End If
       
        'Normalize rpm
        For i = 0 To gs_Dim
            gs_nnDown(i) = (gs_nnDown(i) - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)
            gs_nnUp(i) = (gs_nnUp(i) - VEH.nLeerl) / (VEH.nNenn - VEH.nLeerl)
        Next

        Return True

    End Function

    Public Function fGSnnDown(ByVal Md As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If gs_M(0) >= Md Then
            If gs_M(0) > Md Then MODdata.ModErrors.GSextrapol = "Md= " & Md & " [Nm]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While gs_M(i) < Md And i < gs_Dim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If gs_M(i) < Md Then
            MODdata.ModErrors.GSextrapol = "Md= " & Md & " [Nm]"
        End If

lbInt:
        'Interpolation
        Return (Md - gs_M(i - 1)) * (gs_nnDown(i) - gs_nnDown(i - 1)) / (gs_M(i) - gs_M(i - 1)) + gs_nnDown(i - 1)

    End Function

    Public Function fGSnnUp(ByVal Md As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If gs_M(0) >= Md Then
            If gs_M(0) > Md Then MODdata.ModErrors.GSextrapol = "Md= " & Md & " [Nm]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While gs_M(i) < Md And i < gs_Dim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If gs_M(i) < Md Then
            MODdata.ModErrors.GSextrapol = "Md= " & Md & " [Nm]"
        End If

lbInt:
        'Interpolation
        Return (Md - gs_M(i - 1)) * (gs_nnUp(i) - gs_nnUp(i - 1)) / (gs_M(i) - gs_M(i - 1)) + gs_nnUp(i - 1)

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

    Public Property gsFile(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return gs_file.OriginalPath
            Else
                Return gs_file.FullPath
            End If
        End Get
        Set(value As String)
            gs_file.Init(MyPath, value)
        End Set
    End Property


End Class
