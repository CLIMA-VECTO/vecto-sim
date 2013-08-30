Imports System.Collections.Generic

Public Class cGBX

    Private MyPath As String
    Private sFilePath As String

    Public ModelName As String
    Public I_Getriebe As Single
    Public TracIntrSi As Single

    Public GetrI As List(Of Single)
    Public GetrMaps As List(Of cSubPath)
    Public IsTCgear As List(Of Boolean)

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
    Public gs_ShiftInside As Boolean

    Public gs_Type As tGearbox

    'Torque Converter Input
    Public TCon As Boolean
    Public TCrefrpm As Single
    Private TC_file As New cSubPath


    Private TCnu As New List(Of Single)
    Private TCmu As New List(Of Single)
    Private TCtorque As New List(Of Single)
    Private TCdim As Integer


    'Torque Converter Iteration Results
    Public TCMin As Single
    Public TCnUin As Single
    Public TC_PeBrake As Single
    Public TCMout As Single
    Public TCnout As Single
    Public TCmustReduce As Boolean


    Public Sub New()
        MyPath = ""
        sFilePath = ""
        SetDefault()
    End Sub

    Private Sub SetDefault()

        ModelName = ""
        I_Getriebe = 0
        TracIntrSi = 0

        GetrI = New List(Of Single)
        GetrMaps = New List(Of cSubPath)
        IsTCgear = New List(Of Boolean)

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
        gs_ShiftInside = False

        gs_Type = tGearbox.Manual

        TCon = False
        TCrefrpm = 0
        TC_file.Clear()

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

        file.WriteLine("c Gears (0=axle)")
        file.WriteLine("c Ratio [-], Loss Map or Efficiency, Is TC gear")
        For i = 0 To GetrI.Count - 1
            file.WriteLine("c Gear " & i)
            file.WriteLine(CStr(GetrI(i)), GetrMaps(i).PathOrDummy, CStr(Math.Abs(CInt(IsTCgear(i)))))
        Next
        file.WriteLine(sKey.Break)

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
        file.WriteLine("c Shift inside polygons")
        file.WriteLine(CStr(Math.Abs(CInt(gs_ShiftInside))))

        file.WriteLine("c Gearbox Type")
        file.WriteLine(CStr(CType(gs_Type, Integer)))

        file.WriteLine("c TC On/Off")
        file.WriteLine(CStr(Math.Abs(CInt(TCon))))
        file.WriteLine("c TC characteristics file")
        file.WriteLine(TC_file.PathOrDummy)
        file.WriteLine("c TC reference rpm")
        file.WriteLine(CStr(TCrefrpm))


        file.Close()
        file = Nothing

        Return True

    End Function

    Public Function ReadFile() As Boolean
        Dim line() As String
        Dim file As cFile_V3
        Dim i As Integer
        Dim MsgSrc As String
        Dim OldFile As Boolean = False

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


            i = -1
            Do While Not file.EndOfFile

                line = file.ReadLine
                i += 1

                If line(0) = sKey.Break Or (OldFile And i = 16) Then Exit Do

                If i = 0 AndAlso UBound(line) < 2 Then OldFile = True

                If CSng(line(0)) = 0 Then Continue Do

                GetrI.Add(CSng(line(0)))
                GetrMaps.Add(New cSubPath)
                GetrMaps(i).Init(MyPath, line(1))
                If OldFile Then
                    IsTCgear.Add(False)
                Else
                    IsTCgear.Add(CBool(CInt(line(2))))
                End If

            Loop

            iganganz = GetrI.Count - 1


            'Allow file end here to keep compatibility to older versions
            If Not file.EndOfFile Then
                gs_file.Init(MyPath, file.ReadLine(0))
                gs_TorqueResv = CSng(file.ReadLine(0))
                gs_SkipGears = CBool(CInt(file.ReadLine(0)))
                gs_ShiftTime = CInt(file.ReadLine(0))
                gs_TorqueResvStart = CSng(file.ReadLine(0))
                gs_StartSpeed = CSng(file.ReadLine(0))
                gs_StartAcc = CSng(file.ReadLine(0))
                gs_ShiftInside = CBool(CInt(file.ReadLine(0)))
            End If

            If Not file.EndOfFile Then
                gs_Type = CType(CInt(file.ReadLine(0)), tGearbox)
                TCon = CBool(CInt(file.ReadLine(0)))
                TC_file.Init(MyPath, file.ReadLine(0))
                TCrefrpm = CSng(file.ReadLine(0))
            Else
                gs_Type = tGearbox.Custom
            End If

            If OldFile And TCon Then IsTCgear(1) = True

        Catch ex As Exception
            WorkerMsg(tMsgID.Err, ex.Message, MsgSrc)
            file.Close()
            Return False
        End Try


        file.Close()
        Return True


    End Function

    Public Function TCinit() As Boolean
        Dim file As New cFile_V3
        Dim MsgSrc As String
        Dim line() As String

        MsgSrc = "GBX/TCinit"

        If Not file.OpenRead(TC_file.FullPath) Then
            WorkerMsg(tMsgID.Err, "Torque Converter file not found! (" & TC_file.FullPath & ")", MsgSrc)
            Return False
        End If

        If TCrefrpm <= 0 Then
            WorkerMsg(tMsgID.Err, "Torque converter reference torque invalid! (" & TCrefrpm & ")", MsgSrc)
            Return False
        End If

        TCnu.Clear()
        TCmu.Clear()
        TCtorque.Clear()
        TCdim = -1

        Try
            Do While Not file.EndOfFile
                line = file.ReadLine
                TCnu.Add(CSng(line(0)))
                TCmu.Add(CSng(line(1)))
                TCtorque.Add(CSng(line(2)))
                TCdim += 1
            Loop
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Error while reading Torque Converter file! (" & ex.Message & ")", MsgSrc)
            Return False
        End Try

        file.Close()

        'Check if more then one point
        If TCdim < 1 Then
            WorkerMsg(tMsgID.Err, "More points in Torque Converter file needed!", MsgSrc)
            Return False
        End If

        Return True

    End Function

    Public Function TCiteration(ByVal nUout As Single, ByVal PeOut As Single, ByVal t As Integer) As Boolean
        Dim nUin As Single
        Dim Mout As Single
        Dim VZ As Integer
        Dim nUstep As Single
        Dim lastErr As Single

        Dim nu As Single
        Dim mu As Single

        Dim MoutCalc As Single

        Dim MsgSrc As String

        MsgSrc = "GBX/TCiteration/t= " & t

        TC_PeBrake = 0
        TCmustReduce = False

        'Dim nUmin As Single
        'Dim nUmax As Single

        'Power to torque
        Mout = nPeToM(nUout, PeOut)

        'rpm-limits
        'nUmin = nnormTonU(GBX.fGSnnDown(Mout))
        'nUmax = nnormTonU(GBX.fGSnnUp(Mout))

        'Start values: Estimate torque converter state
        'nUin = Math.Min(VEH.nNenn, nUout * 2)
        If t = 0 Then
            nUin = nUout
        Else
            nUin = MODdata.nU(t - 1)
        End If

        'If nUin > nUmax Then nUin = nUmax
        'If nUin < nUmin Then nUin = nUmin

        nu = nUout / nUin

        If nu > TCnu(TCdim) Then
            nu = TCnu(TCdim)
            nUin = nUout / nu
        ElseIf nu < TCnu(0) Then
            nu = TCnu(0)
            nUin = nUout / nu
        End If

        mu = fTCmu(nu)
        MoutCalc = fTCtorque(nu, nUin) * mu

        nUstep = DEV.TCnUstep

        If MoutCalc > Mout Then
            VZ = -1
        Else
            VZ = 1
        End If

        lastErr = 99999

        'Iteration
        Do While Math.Abs(1 - MoutCalc / Mout) > DEV.TCiterPrec And nUstep > DEV.TCnUstepMin
lb10:
            nUin += VZ * nUstep
            nu = nUout / nUin

            If nu > TCnu(TCdim) Or nu < TCnu(0) Then
                nUin -= VZ * nUstep
                nUstep /= 2
                VZ *= -1
                GoTo lb10
            End If


            mu = fTCmu(nu)
            MoutCalc = fTCtorque(nu, nUin) * mu
            If Math.Abs(1 - MoutCalc / Mout) > lastErr Then
                nUstep /= 2
                VZ *= -1
            End If
            lastErr = Math.Abs(1 - MoutCalc / Mout)
        Loop


        If nUin < VEH.nLeerl Then

            MODdata.ModErrors.TCextrapol = ""

            nUin = VEH.nLeerl
            nu = nUout / nUin

            If nu > TCnu(TCdim) Then
                nu = TCnu(TCdim)
                nUin = nUout / nu
            ElseIf nu < TCnu(0) Then

                WorkerMsg(tMsgID.Err, "Torque converter creeping not possible with current TC characteristics!", MsgSrc)
                Return False

                'nu = TCnu(0)
                'nUin = nUout / nu
            End If

            mu = fTCmu(nu)
            MoutCalc = fTCtorque(nu, nUin) * mu

        End If


        If Math.Abs(1 - MoutCalc / Mout) > DEV.TCiterPrec Then

            If MoutCalc < Mout Then
              

                If MoutCalc > 0 Then TCmustReduce = True

            Else

                TC_PeBrake = nMtoPe(nUout, Mout - MoutCalc)


            End If

        End If


        TCMin = MoutCalc / mu
        TCnUin = nUin

        TCMout = MoutCalc
        TCnout = nUout

        Return True

    End Function

    Private Function fTCmu(ByVal nu As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If TCnu(0) >= nu Then
            If TCnu(0) > nu Then MODdata.ModErrors.TCextrapol = "nu= " & nu & " [n_out/n_in]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While TCnu(i) < nu And i < TCdim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If TCnu(i) < nu Then
            MODdata.ModErrors.TCextrapol = "nu= " & nu & " [n_out/n_in]"
        End If

lbInt:
        'Interpolation
        Return (nu - TCnu(i - 1)) * (TCmu(i) - TCmu(i - 1)) / (TCnu(i) - TCnu(i - 1)) + TCmu(i - 1)

    End Function

    Private Function fTCtorque(ByVal nu As Single, ByVal nUin As Single) As Single
        Dim i As Int32
        Dim M0 As Single

        'Extrapolation for x < x(1)
        If TCnu(0) >= nu Then
            If TCnu(0) > nu Then MODdata.ModErrors.TCextrapol = "nu= " & nu & " [n_out/n_in]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While TCnu(i) < nu And i < TCdim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If TCnu(i) < nu Then
            MODdata.ModErrors.TCextrapol = "nu= " & nu & " [n_out/n_in]"
        End If

lbInt:
        'Interpolation
        M0 = (nu - TCnu(i - 1)) * (TCtorque(i) - TCtorque(i - 1)) / (TCnu(i) - TCnu(i - 1)) + TCtorque(i - 1)

        Return M0 * (nUin / TCrefrpm) ^ 2

    End Function

    Public Function GSinit() As Boolean
        Dim file As cFile_V3
        Dim line As String()
        Dim i As Integer
        Dim gserror As Boolean

        Dim MsgSrc As String

        MsgSrc = "GBX/GSinit"

        'Check if settings ok
        If gs_Type <> tGearbox.Custom Then

            gserror = False

            Select Case gs_Type
                Case tGearbox.Manual
                    If gs_ShiftInside Then
                        WorkerMsg(tMsgID.Err, "Option 'Shift-Up inside polygons' is not available for Manual Transmissions!", MsgSrc)
                        gserror = True
                    End If
                    If TCon Then
                        WorkerMsg(tMsgID.Err, "Torque Converter is not available for Manual Transmissions!", MsgSrc)
                        gserror = True
                    End If

                Case tGearbox.SemiAutomatic
                    If TCon Then
                        WorkerMsg(tMsgID.Err, "Torque Converter is not available for Automated Manual Transmissions!", MsgSrc)
                        gserror = True
                    End If

                Case tGearbox.Automatic
                    If gs_ShiftInside Then
                        WorkerMsg(tMsgID.Err, "Option 'Shift-Up inside polygons' is not available for Automatic Transmissions!", MsgSrc)
                        gserror = True
                    End If
                    If gs_SkipGears Then
                        WorkerMsg(tMsgID.Err, "Option 'Skip gears' is not available for Automatic Transmissions!", MsgSrc)
                        gserror = True
                    End If
                    If Not TCon Then
                        WorkerMsg(tMsgID.Err, "Torque Converter must be activated for Automatic Transmissions!", MsgSrc)
                        gserror = True
                    End If

            End Select

            If gserror Then Return False

        End If

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

    Public Property TCfile(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return TC_file.OriginalPath
            Else
                Return TC_file.FullPath
            End If
        End Get
        Set(value As String)
            TC_file.Init(MyPath, value)
        End Set
    End Property


End Class
