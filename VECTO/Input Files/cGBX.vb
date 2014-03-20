Imports System.Collections.Generic

Public Class cGBX

    Private Const FormatVersion As String = "1.0"
    Private FileVersion As String

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
    Private gs_Mup As New List(Of Single)
    Private gs_Mdown As New List(Of Single)
    Private gs_nUup As New List(Of Single)
    Private gs_nUdown As New List(Of Single)
    Private gs_Dup As Integer
    Private gs_Ddown As Integer
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

    Public NoJSON As Boolean

    Private MyFileList As List(Of String)


    Public Function CreateFileList() As Boolean
        Dim i As Integer

        MyFileList = New List(Of String)

        '.vgbs
        MyFileList.Add(Me.gs_file.FullPath)

        'Transm. Loss Maps
        For i = 0 To GetrMaps.Count - 1
            If Not IsNumeric(Me.GetrMap(i, True)) Then
                MyFileList.Add(Me.GetrMap(i))
            End If
        Next

        'Torque Converter
        If Me.TCon Then MyFileList.Add(TCfile)


        Return True
    End Function


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
        gs_Mup.Clear()
        gs_Mdown.Clear()
        gs_nUdown.Clear()
        gs_nUup.Clear()
        gs_file.Clear()
        gs_Dup = -1
        gs_Ddown = -1
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

    Private Function ReadFileOld() As Boolean
        Dim line() As String
        Dim file As cFile_V3
        Dim i As Integer
        Dim MsgSrc As String
        Dim OldFile As Boolean = False

        MsgSrc = "GBX/ReadFile"

        SetDefault()

        If sFilePath = "" Or Not IO.File.Exists(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Gearbox file not found (" & sFilePath & ") !", MsgSrc)
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

        dic.Add("Inertia", I_Getriebe)
        dic.Add("TracInt", TracIntrSi)

        ls = New List(Of Object)
        For i = 0 To GetrI.Count - 1
            dic0 = New Dictionary(Of String, Object)
            dic0.Add("Ratio", GetrI(i))
            If IsNumeric(Me.GetrMap(i, True)) Then
                dic0.Add("Efficiency", GetrMaps(i).PathOrDummy)
            Else
                dic0.Add("LossMap", GetrMaps(i).PathOrDummy)
            End If
            dic0.Add("TCactive", IsTCgear(i))
            ls.Add(dic0)
        Next
        dic.Add("Gears", ls)

        dic.Add("ShiftPolygons", gs_file.PathOrDummy)
        dic.Add("TqReserve", gs_TorqueResv)
        dic.Add("SkipGears", gs_SkipGears)
        dic.Add("ShiftTime", gs_ShiftTime)
        dic.Add("EaryShiftUp", gs_ShiftInside)

        dic.Add("StartTqReserve", gs_TorqueResvStart)
        dic.Add("StartSpeed", gs_StartSpeed)
        dic.Add("StartAcc", gs_StartAcc)

        dic.Add("GearboxType", GearboxConv(gs_Type))

        dic0 = New Dictionary(Of String, Object)
        dic0.Add("Enabled", TCon)
        dic0.Add("File", TC_file.PathOrDummy)
        dic0.Add("RefRPM", TCrefrpm)
        dic.Add("TorqueConverter", dic0)

        JSON.Content.Add("Body", dic)

        Return JSON.WriteFile(sFilePath)

    End Function

    Public Function ReadFile() As Boolean
        Dim i As Integer
        Dim MsgSrc As String
        Dim JSON As New cJSON
        Dim dic As Object

        MsgSrc = "GBX/ReadFile"

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
            I_Getriebe = JSON.Content("Body")("Inertia")
            TracIntrSi = JSON.Content("Body")("TracInt")

            i = -1
            For Each dic In JSON.Content("Body")("Gears")
                i += 1

                GetrI.Add(dic("Ratio"))
                GetrMaps.Add(New cSubPath)

                If dic("Efficiency") Is Nothing Then
                    GetrMaps(i).Init(MyPath, dic("LossMap"))
                Else
                    GetrMaps(i).Init(MyPath, dic("Efficiency"))
                End If

                IsTCgear.Add(dic("TCactive"))

            Next

            iganganz = GetrI.Count - 1

            gs_file.Init(MyPath, JSON.Content("Body")("ShiftPolygons"))
            gs_TorqueResv = JSON.Content("Body")("TqReserve")
            gs_SkipGears = JSON.Content("Body")("SkipGears")
            gs_ShiftTime = JSON.Content("Body")("ShiftTime")
            gs_TorqueResvStart = JSON.Content("Body")("StartTqReserve")
            gs_StartSpeed = JSON.Content("Body")("StartSpeed")
            gs_StartAcc = JSON.Content("Body")("StartAcc")
            gs_ShiftInside = JSON.Content("Body")("EaryShiftUp")

            gs_Type = GearboxConv(JSON.Content("Body")("GearboxType").ToString)

            If JSON.Content("Body")("TorqueConverter") Is Nothing Then
                TCon = False
            Else
                TCon = JSON.Content("Body")("TorqueConverter")("Enabled")
                TC_file.Init(MyPath, JSON.Content("Body")("TorqueConverter")("File"))
                TCrefrpm = JSON.Content("Body")("TorqueConverter")("RefRPM")
            End If

        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to read VECTO file! " & ex.Message, MsgSrc)
            Return False
        End Try

        Return True


    End Function

    Public Function DeclInit() As Boolean

        If gs_Type = tGearbox.Custom Or gs_Type = tGearbox.Automatic Then Return False

        I_Getriebe = cDeclaration.GbInertia
        TracIntrSi = Declaration.TracInt(gs_Type)
        gs_SkipGears = Declaration.SkipGears(gs_Type)
        gs_ShiftTime = Declaration.ShiftTime(gs_Type)
        gs_ShiftInside = Declaration.ShiftInside(gs_Type)
        gs_TorqueResv = cDeclaration.TqResv
        gs_TorqueResvStart = cDeclaration.TqResvStart
        gs_StartSpeed = cDeclaration.StartSpeed
        gs_StartAcc = cDeclaration.StartAcc

        TCon = (gs_Type = tGearbox.Automatic)

        SetGenericShiftPoly()

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
        Dim Min As Single
        Dim VZ As Integer
        Dim nUstep As Single
        Dim lastErr As Single

        Dim nu As Single
        Dim mu As Single

        Dim MoutCalc As Single

        Dim nUup As Single
        Dim nUdown As Single

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
            nUin += VZ * nUstep
            nu = nUout / nUin

            If nu > TCnu(TCdim) Or nu < TCnu(0) Then
                nUin -= VZ * nUstep
                nUstep /= 2
                VZ *= -1
                Continue Do
            End If

            mu = fTCmu(nu)
            Min = Mout / mu

            'Up/Downshift rpms
            nUup = GBX.fGSnUup(Min)
            nUdown = GBX.fGSnUdown(Min)

            'If nUin > 1.05 * nUup - 0.0001 Then
            If nUin > ENG.Nrated - 0.0001 Then
                'nUin = 1.05 * nUup
                nUin = ENG.Nrated
                nUstep /= 2
                VZ *= -1
                Continue Do
            ElseIf nUin < 0.95 * nUdown + 0.0001 Then
                nUin = 0.95 * nUdown
                nUstep /= 2
                VZ *= -1
                Continue Do
            End If


            MoutCalc = fTCtorque(nu, nUin) * mu
            If Math.Abs(1 - MoutCalc / Mout) > lastErr Then
                nUstep /= 2
                VZ *= -1
            End If
            lastErr = Math.Abs(1 - MoutCalc / Mout)
        Loop

        'Calc nu again because nUin might have changed
        nu = nUout / nUin

        If nUin < ENG.Nidle Then

            MODdata.ModErrors.TCextrapol = ""

            nUin = ENG.Nidle
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
        gs_Mup.Clear()
        gs_Mdown.Clear()
        gs_nUdown.Clear()
        gs_nUup.Clear()
        gs_Dup = -1

        'Read file
        Try
            Do While Not file.EndOfFile
                line = file.ReadLine
                gs_Dup += 1
                gs_Mup.Add(CSng(line(0)))
                gs_Mdown.Add(CSng(line(0)))
                gs_nUdown.Add(CSng(line(1)))
                gs_nUup.Add(CSng(line(2)))
            Loop
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Error while reading Gear Shift Polygon File! (" & ex.Message & ")", MsgSrc)
            Return False
        End Try

        'Check if more then one point
        If gs_Dup < 1 Then
            WorkerMsg(tMsgID.Err, "More points in Gear Shift Polygon File needed!", MsgSrc)
            Return False
        End If

        gs_Ddown = gs_Dup

        Return True

    End Function

    Public Sub SetGenericShiftPoly()

        Dim Tmax As Single

        'Clear lists
        gs_Mup.Clear()
        gs_Mdown.Clear()
        gs_nUdown.Clear()
        gs_nUup.Clear()

        Tmax = FLD(FLD.Count - 1).Tmax

        gs_nUdown.Add(ENG.Nidle)
        gs_nUdown.Add(ENG.Nidle)
        gs_nUdown.Add((ENG.Npref + ENG.Nlo) / 2)

        gs_Mdown.Add(0)
        gs_Mdown.Add(Tmax * ENG.Nidle / (ENG.Npref + ENG.Nlo - ENG.Nidle))
        gs_Mdown.Add(Tmax)

        gs_nUup.Add(ENG.Npref)
        gs_nUup.Add(ENG.Npref)
        gs_nUup.Add(ENG.N95h)

        gs_Mup.Add(0)
        gs_Mup.Add(Tmax * (ENG.Npref - ENG.Nidle) / (ENG.N95h - ENG.Nidle))
        gs_Mup.Add(Tmax)

        gs_Ddown = 2
        gs_Dup = 2

    End Sub

    Public Function fGSnUdown(ByVal Md As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If gs_Mdown(0) >= Md Then
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While gs_Mdown(i) < Md And i < gs_Ddown
            i += 1
        Loop


lbInt:
        'Interpolation
        Return (Md - gs_Mdown(i - 1)) * (gs_nUdown(i) - gs_nUdown(i - 1)) / (gs_Mdown(i) - gs_Mdown(i - 1)) + gs_nUdown(i - 1)

    End Function

    Public Function fGSnUup(ByVal Md As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If gs_Mup(0) >= Md Then
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While gs_Mup(i) < Md And i < gs_Dup
            i += 1
        Loop


lbInt:
        'Interpolation
        Return (Md - gs_Mup(i - 1)) * (gs_nUup(i) - gs_nUup(i - 1)) / (gs_Mup(i) - gs_Mup(i - 1)) + gs_nUup(i - 1)

    End Function

    Public Function GearCount() As Integer
        Return GBX.GetrI.Count - 1
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

    Public Function GearboxConv(ByVal Gearbox As tGearbox) As String
        Select Case Gearbox
            Case tGearbox.Manual
                Return "MT"
            Case tGearbox.Automatic
                Return "AT"
            Case tGearbox.SemiAutomatic
                Return "AMT"
            Case Else 'tGearbox.Custom
                Return "Custom"
        End Select
    End Function

    Public Function GearboxConv(ByVal Gearbox As String) As tGearbox
        Select Case UCase(Trim(Gearbox))
            Case "MT"
                Return tGearbox.Manual
            Case "AT"
                Return tGearbox.Automatic
            Case "AMT"
                Return tGearbox.SemiAutomatic
            Case Else  '"Custom"
                Return tGearbox.Custom
        End Select
    End Function


End Class
