' Copyright 2014 European Union.
' Licensed under the EUPL (the 'Licence');
'
' * You may not use this work except in compliance with the Licence.
' * You may obtain a copy of the Licence at: http://ec.europa.eu/idabc/eupl
' * Unless required by applicable law or agreed to in writing,
'   software distributed under the Licence is distributed on an "AS IS" basis,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'
' See the LICENSE.txt for the specific language governing permissions and limitations.
Imports System.Collections.Generic

Public Class cGBX

    Private Const FormatVersion As Short = 2
    Private FileVersion As Short

    Private MyPath As String
    Private sFilePath As String

    Public ModelName As String
    Public I_Getriebe As Single
    Public TracIntrSi As Single

    Public Igetr As List(Of Single)
    Public GetrMaps As List(Of cSubPath)
    Public IsTCgear As List(Of Boolean)

    Private MyGBmaps As List(Of cDelaunayMap)
    Private GetrEffDef As List(Of Boolean)
    Private GetrEff As List(Of Single)

    'Gear shift polygons
    Public gs_files As List(Of cSubPath)
    Public Shiftpolygons As List(Of cShiftPolygon)


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

        'Transm. Loss Maps
        For i = 0 To GearCount() - 1
            If Not IsNumeric(Me.GetrMap(i, True)) Then
                If Not MyFileList.Contains(Me.GetrMap(i)) Then MyFileList.Add(Me.GetrMap(i))
            End If

            '.vgbs
            If Not Cfg.DeclMode Then
                If i > 0 AndAlso Not MyFileList.Contains(Me.gs_files(i).FullPath) Then MyFileList.Add(Me.gs_files(i).FullPath)
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

        Igetr = New List(Of Single)
        IsTCgear = New List(Of Boolean)
        GetrMaps = New List(Of cSubPath)
        gs_files = New List(Of cSubPath)

        GetrEffDef = New List(Of Boolean)
        GetrEff = New List(Of Single)

        MyGBmaps = Nothing

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

    Private Function ReadFileOld(ByVal ShowMsg As Boolean) As Boolean
        Dim line() As String
        Dim file As cFile_V3
        Dim i As Integer
        Dim MsgSrc As String
        Dim OldFile As Boolean = False

        MsgSrc = "GBX/ReadFile"

        SetDefault()

        If sFilePath = "" Or Not IO.File.Exists(sFilePath) Then
            If ShowMsg Then WorkerMsg(tMsgID.Err, "Gearbox file not found (" & sFilePath & ") !", MsgSrc)
            Return False
        End If

        file = New cFile_V3

        If Not file.OpenRead(sFilePath) Then
            If ShowMsg Then WorkerMsg(tMsgID.Err, "Failed to open file (" & sFilePath & ") !", MsgSrc)
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

                Igetr.Add(CSng(line(0)))
                GetrMaps.Add(New cSubPath)
                GetrMaps(i).Init(MyPath, line(1))
                If OldFile Then
                    IsTCgear.Add(False)
                Else
                    IsTCgear.Add(CBool(CInt(line(2))))
                End If

            Loop

            line = file.ReadLine
            For i = 0 To Igetr.Count - 1
                gs_files.Add(New cSubPath)
                gs_files(i).Init(MyPath, line(0))
            Next

            gs_TorqueResv = CSng(file.ReadLine(0))
            gs_SkipGears = CBool(CInt(file.ReadLine(0)))
            gs_ShiftTime = CInt(file.ReadLine(0))
            gs_TorqueResvStart = CSng(file.ReadLine(0))
            gs_StartSpeed = CSng(file.ReadLine(0))
            gs_StartAcc = CSng(file.ReadLine(0))
            gs_ShiftInside = CBool(CInt(file.ReadLine(0)))

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
            If ShowMsg Then WorkerMsg(tMsgID.Err, ex.Message, MsgSrc)
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
        For i = 0 To Igetr.Count - 1
            dic0 = New Dictionary(Of String, Object)
            dic0.Add("Ratio", Igetr(i))
            If IsNumeric(Me.GetrMap(i, True)) Then
                dic0.Add("Efficiency", GetrMaps(i).PathOrDummy)
            Else
                dic0.Add("LossMap", GetrMaps(i).PathOrDummy)
            End If
            If i > 0 Then
                dic0.Add("TCactive", IsTCgear(i))
                dic0.Add("ShiftPolygon", gs_files(i).PathOrDummy)
            End If
           
            ls.Add(dic0)
        Next
        dic.Add("Gears", ls)

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

    Public Function ReadFile(Optional ByVal ShowMsg As Boolean = True) As Boolean
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
                Return ReadFileOld(ShowMsg)
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

                Igetr.Add(dic("Ratio"))
                GetrMaps.Add(New cSubPath)

                If dic("Efficiency") Is Nothing Then
                    GetrMaps(i).Init(MyPath, dic("LossMap"))
                Else
                    GetrMaps(i).Init(MyPath, dic("Efficiency"))
                End If


                gs_files.Add(New cSubPath)

                If i = 0 Then
                    IsTCgear.Add(False)
                    gs_files(i).Init(MyPath, sKey.NoFile)
                Else
                    IsTCgear.Add(dic("TCactive"))
                    If FileVersion < 2 Then
                        gs_files(i).Init(MyPath, JSON.Content("Body")("ShiftPolygons"))
                    Else
                        gs_files(i).Init(MyPath, dic("ShiftPolygon"))
                    End If
                End If

            Next

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
            If ShowMsg Then WorkerMsg(tMsgID.Err, "Failed to read VECTO file! " & ex.Message, MsgSrc)
            Return False
        End Try

        Return True


    End Function

    Public Function DeclInit() As Boolean
        Dim MsgSrc As String
        Dim i As Int16

        MsgSrc = "GBX/DeclInit"

        If gs_Type = tGearbox.Custom Or gs_Type = tGearbox.Automatic Then
            WorkerMsg(tMsgID.Err, "Invalid gearbox type for Declaration Mode!", MsgSrc)
            Return False
        End If

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

        For i = 1 To GearCount()
            Shiftpolygons(i).SetGenericShiftPoly()
        Next


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

    Public Function TCiteration(ByVal Gear As Integer, ByVal nUout As Single, ByVal PeOut As Single, ByVal t As Integer) As Boolean
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
            nUup = GBX.Shiftpolygons(Gear).fGSnUup(Min)
            nUdown = GBX.Shiftpolygons(Gear).fGSnUdown(Min)

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
        Dim i As Integer

        'Set Gearbox Type-specific settings
        If gs_Type <> tGearbox.Custom Then

            gs_ShiftInside = Declaration.ShiftInside(gs_Type)
            TCon = (gs_Type = tGearbox.Automatic)
            gs_SkipGears = Declaration.SkipGears(gs_Type)

        End If

        Shiftpolygons = New List(Of cShiftPolygon)
        For i = 0 To Igetr.Count - 1
            Shiftpolygons.Add(New cShiftPolygon(gs_files(i).FullPath, i))
            If Not Cfg.DeclMode And i > 0 Then
                'Error-notification within ReadFile()
                If Not Shiftpolygons(i).ReadFile() Then Return False
            End If
        Next

        Return True

    End Function

    Public Function GearCount() As Integer
        Return Me.Igetr.Count - 1
    End Function

#Region "Transmission Loss Maps"

    Public Function TrLossMapInit() As Boolean
        Dim i As Short
        Dim GBmap0 As cDelaunayMap
        'Dim n_norm As Double
        'Dim Pe_norm As Double
        Dim file As cFile_V3
        Dim path As String
        Dim line As String()
        Dim l As Integer
        Dim nU As Double
        Dim M_in As Double
        Dim M_loss As Double
        Dim M_out As Double

        Dim dnU As Single
        Dim dM As Single
        Dim P_In As Single
        Dim P_Loss As Single
        Dim EffSum As Single
        Dim Anz As Integer
        Dim EffDiffSum As Single = 0
        Dim AnzDiff As Integer = 0

        Dim MinG As Single
        Dim plossG As Single

        Dim MsgSrc As String

        MyGBmaps = New List(Of cDelaunayMap)
        file = New cFile_V3

        For i = 0 To GBX.GearCount

            MsgSrc = "VEH/TrLossMapInit/G" & i

            If IsNumeric(GetrMap(i, True)) Then
                GetrEffDef.Add(True)
                GetrEff.Add(CSng(GBX.GetrMap(i, True)))
            Else
                GetrEffDef.Add(False)
                GetrEff.Add(0)
            End If

            If GetrEffDef(i) Then

                If GetrEff(i) > 1 Or GetrEff(i) <= 0 Then
                    WorkerMsg(tMsgID.Err, "Gearboy efficiency '" & GetrEff(i) & "' invalid!", MsgSrc)
                    Return False
                End If

                MyGBmaps.Add(Nothing)

            Else

                path = GetrMaps(i).FullPath

                If Not file.OpenRead(path) Then
                    WorkerMsg(tMsgID.Err, "Cannot read file '" & path & "'!", MsgSrc)
                    MyGBmaps = Nothing
                    Return False
                End If

                GBmap0 = New cDelaunayMap
                GBmap0.DualMode = True

                l = 0   'Nur für Fehler-Ausgabe
                Do While Not file.EndOfFile
                    l += 1
                    line = file.ReadLine
                    Try

                        nU = CDbl(line(0))
                        M_in = CDbl(line(1))
                        M_loss = CDbl(line(2))

                        M_out = M_in - M_loss

                        'old version: Power instead of torque: GBmap0.AddPoints(nU, nMtoPe(nU, M_out), nMtoPe(nU, M_in))
                        GBmap0.AddPoints(nU, M_out, M_in)
                    Catch ex As Exception
                        WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & l & " (" & path & ")", MsgSrc, path)
                        file.Close()
                        MyGBmaps = Nothing
                        Return False
                    End Try
                Loop

                file.Close()

                If Not GBmap0.Triangulate Then
                    WorkerMsg(tMsgID.Err, "Map triangulation failed! File: " & path, MsgSrc, path)
                    MyGBmaps = Nothing
                    Return False
                End If

                MyGBmaps.Add(GBmap0)

                'Calculate average efficiency for fast approx. calculation
                If i > 0 Then

                    If GBX.IsTCgear(i) Then

                        GetrEff(i) = -1

                    Else

                        EffSum = 0
                        Anz = 0

                        dnU = (2 / 3) * (ENG.Nrated - ENG.Nidle) / 10
                        nU = ENG.Nidle + dnU

                        Do While nU <= ENG.Nrated

                            dM = nPeToM(nU, (2 / 3) * FLD(i).Pfull(nU) / 10)
                            M_in = nPeToM(nU, (1 / 3) * FLD(i).Pfull(nU))

                            Do While M_in <= nPeToM(nU, FLD(i).Pfull(nU))

                                P_In = nMtoPe(nU, M_in)

                                P_Loss = IntpolPeLossFwd(i, nU, P_In, False)

                                EffSum += (P_In - P_Loss) / P_In
                                Anz += 1


                                plossG = P_Loss
                                MinG = M_in


                                'Axle
                                P_In -= P_Loss
                                P_Loss = IntpolPeLossFwd(0, nU / GBX.Igetr(i), P_In, False)
                                EffDiffSum += (P_In - P_Loss) / P_In
                                AnzDiff += 1

                                If MODdata.ModErrors.TrLossMapExtr <> "" Then
                                    WorkerMsg(tMsgID.Err, "Transmission loss map does not cover full engine operating range!", MsgSrc)
                                    WorkerMsg(tMsgID.Err, MODdata.ModErrors.TrLossMapExtr, MsgSrc)
                                    WorkerMsg(tMsgID.Err, "nU_In(GB)=" & nU & " [1/min]", MsgSrc)
                                    WorkerMsg(tMsgID.Err, "M_In(GB)=" & MinG & " [Nm]", MsgSrc)
                                    WorkerMsg(tMsgID.Err, "P_Loss(GB)=" & plossG & " [kW]", MsgSrc)
                                    WorkerMsg(tMsgID.Err, "nU_In(axle)=" & CStr(nU / Igetr(i)) & " [1/min]", MsgSrc)
                                    WorkerMsg(tMsgID.Err, "M_In(axle)=" & CStr(nPeToM(nU / Igetr(i), P_In)) & " [Nm]", MsgSrc)
                                    WorkerMsg(tMsgID.Err, "P_Loss(axle)=" & P_Loss & " [kW]", MsgSrc)
                                    Return False
                                End If

                                M_in += dM
                            Loop


                            nU += dnU
                        Loop

                        If Anz = 0 Then
                            WorkerMsg(tMsgID.Err, "Failed to calculate approx. transmission losses!", MsgSrc)
                            Return False
                        End If

                        GetrEff(i) = EffSum / Anz

                    End If

                End If


            End If

        Next

        If Not GetrEffDef(0) Then
            GetrEff(0) = EffDiffSum / AnzDiff
        End If


        Return True

    End Function

    Public Function IntpolPeLoss(ByVal Gear As Integer, ByVal nU As Double, ByVal PeOut As Double, ByVal Approx As Boolean) As Double

        Dim PeIn As Double
        Dim WG As Double
        Dim GBmap As cDelaunayMap
        Dim i As Integer
        Dim Ab As Double
        Dim AbMin As Double
        Dim iMin As Integer
        Dim PeOutX As Double
        Dim GrTxt As String
        Dim Ploss As Single

        Dim MsgSrc As String

        MsgSrc = "VEH/TrLossMapInterpol/G" & Gear

        If Gear = 0 Then
            GrTxt = "A"
        Else
            GrTxt = Gear.ToString
        End If

        If GetrEffDef(Gear) Or (Approx AndAlso GetrEff(Gear) > 0) Then

            If PeOut > 0 Then
                PeIn = PeOut / GetrEff(Gear)
            Else
                PeIn = PeOut * GetrEff(Gear)
            End If
            Ploss = PeIn - PeOut

        Else

            GBmap = MyGBmaps(Gear)

            Try
                'Interpolate with Original Values
                PeIn = nMtoPe(nU, GBmap.Intpol(nU, nPeToM(nU, PeOut)))
                Ploss = PeIn - PeOut

            Catch ex As Exception

                'If error: try extrapolation

                'Search for the nearest Map point
                AbMin = ((GBmap.ptList(0).X - nU) ^ 2 + (GBmap.ptList(0).Y - nPeToM(nU, PeOut)) ^ 2) ^ 0.5
                iMin = 0
                For i = 1 To GBmap.ptDim
                    Ab = ((GBmap.ptList(i).X - nU) ^ 2 + (GBmap.ptList(i).Y - nPeToM(nU, PeOut)) ^ 2) ^ 0.5
                    If Ab < AbMin Then
                        AbMin = Ab
                        iMin = i
                    End If
                Next

                PeOutX = nMtoPe(nU, GBmap.ptList(iMin).Y)
                PeIn = nMtoPe(nU, GBmap.ptList(iMin).Z)

                'Efficiency
                If PeOutX > 0 Then
                    If PeIn > 0 Then

                        WG = PeOutX / PeIn
                        PeIn = PeOut / WG
                        Ploss = PeIn - PeOut

                    Else

                        'Drag => Drive: ERROR!
                        WorkerMsg(tMsgID.Err, "Transmission Loss Map invalid! Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" & PeIn.ToString("0.0") & " [kW], PeOut=" & PeOutX.ToString("0.0") & " [kW]", MsgSrc)
                        WorkerAbort()
                        Return 0

                    End If

                ElseIf PeOutX < 0 Then

                    If PeIn > 0 Then

                        WG = (PeIn - (PeIn - PeOutX)) / PeIn
                        PeIn = PeOut / WG
                        Ploss = PeIn - PeOut

                    ElseIf PeIn < 0 Then

                        WG = PeIn / PeOutX
                        PeIn = PeOut * WG
                        Ploss = PeIn - PeOut

                    Else

                        Ploss = Math.Abs(PeOut)

                    End If


                Else

                    If PeIn > 0 Then

                        Ploss = PeIn

                    ElseIf PeIn < 0 Then

                        'Drag => Zero: ERROR!
                        WorkerMsg(tMsgID.Err, "Transmission Loss Map invalid! Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" & PeIn.ToString("0.0") & " [kW], PeOut=" & PeOutX.ToString("0.0") & " [kW]", MsgSrc)
                        WorkerAbort()
                        Return 0
                    Else

                        Ploss = Math.Abs(PeOut)

                    End If

                End If

                MODdata.ModErrors.TrLossMapExtr = "Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], MeOut=" & nPeToM(nU, PeOut).ToString("0.00") & " [Nm]"

            End Try

        End If

        Return Math.Max(Ploss, 0)


    End Function

    Public Function IntpolPeLossFwd(ByVal Gear As Integer, ByVal nU As Double, ByVal PeIn As Double, ByVal Approx As Boolean) As Double

        Dim PeOut As Double
        Dim WG As Double
        Dim GBmap As cDelaunayMap
        Dim i As Integer
        Dim Ab As Double
        Dim AbMin As Double
        Dim iMin As Integer
        Dim PeInX As Double
        Dim GrTxt As String

        Dim MsgSrc As String

        MsgSrc = "VEH/TrLossMapInterpolFwd/G" & Gear

        If Gear = 0 Then
            GrTxt = "A"
        Else
            GrTxt = Gear.ToString
        End If

        If GetrEffDef(Gear) Or (Approx AndAlso GetrEff(Gear) > 0) Then

            If PeIn > 0 Then
                PeOut = PeIn * GetrEff(Gear)
            Else
                PeOut = PeIn / GetrEff(Gear)
            End If

        Else

            GBmap = MyGBmaps(Gear)

            Try
                'Interpolate with original values
                PeOut = nMtoPe(nU, GBmap.IntpolXZ(nU, nPeToM(nU, PeIn)))

            Catch ex As Exception

                'If error: try extrapolation

                'Search for the nearest Map-point
                AbMin = ((GBmap.ptList(0).X - nU) ^ 2 + (GBmap.ptList(0).Z - nPeToM(nU, PeIn)) ^ 2) ^ 0.5
                iMin = 0
                For i = 1 To GBmap.ptDim
                    Ab = ((GBmap.ptList(i).X - nU) ^ 2 + (GBmap.ptList(i).Z - nPeToM(nU, PeIn)) ^ 2) ^ 0.5
                    If Ab < AbMin Then
                        AbMin = Ab
                        iMin = i
                    End If
                Next

                PeInX = nMtoPe(nU, GBmap.ptList(iMin).Z)
                PeOut = nMtoPe(nU, GBmap.ptList(iMin).Y)

                'Efficiency
                If PeOut > 0 Then
                    If PeInX > 0 Then

                        'Drivetrain => Drivetrain
                        WG = PeOut / PeInX

                    Else

                        'Drag => Drivetrain: ERROR!
                        WorkerMsg(tMsgID.Err, "Transmission Loss Map invalid! Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" & PeInX.ToString("0.00") & " [kW], PeOut=" & PeOut.ToString("0.00") & " [kW] (fwd)", MsgSrc)
                        WorkerAbort()
                        Return 0

                    End If

                Else
                    If PeInX > 0 Then

                        WorkerMsg(tMsgID.Warn, "Change of sign in Transmission Loss Map! Set efficiency to 10%. Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" & PeInX.ToString("0.00") & " [kW], PeOut=" & PeOut.ToString("0.00") & " [kW] (fwd)", MsgSrc)
                        'WorkerAbort()
                        WG = 0.1

                    Else

                        'Drag => Drag
                        WG = PeInX / PeOut


                    End If
                End If

                'Calculate efficiency with PeIn for original PeOut
                PeOut = PeIn * WG

                MODdata.ModErrors.TrLossMapExtr = "Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], MeIn=" & nPeToM(nU, PeIn).ToString("0.00") & " [Nm] (fwd)"

            End Try

        End If

        Return Math.Max(PeIn - PeOut, 0)

    End Function



#End Region

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
            If sFilePath = "" Then
                MyPath = ""
            Else
                MyPath = IO.Path.GetDirectoryName(sFilePath) & "\"
            End If
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

    Public Property gsFile(ByVal x As Short, Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return gs_files(x).OriginalPath
            Else
                Return gs_files(x).FullPath
            End If
        End Get
        Set(value As String)
            gs_files(x).Init(MyPath, value)
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


    Public Class cShiftPolygon

        Private Filepath As String
        Public MyGear As Integer

        Public gs_Mup As New List(Of Single)
        Public gs_Mdown As New List(Of Single)
        Public gs_nUup As New List(Of Single)
        Public gs_nUdown As New List(Of Single)
        Private gs_Dup As Integer = -1
        Private gs_Ddown As Integer = -1

        Public Sub New(ByVal Path As String, ByVal Gear As Integer)
            Filepath = Path
            MyGear = Gear
        End Sub

        Public Function ReadFile() As Boolean
            Dim file As cFile_V3
            Dim line As String()

            Dim MsgSrc As String

            MsgSrc = "GBX/GSinit/ShiftPolygon.Init"

            'Check if file exists
            If Not IO.File.Exists(Filepath) Then
                WorkerMsg(tMsgID.Err, "Gear Shift Polygon File not found! '" & Filepath & "'", MsgSrc)
                Return False
            End If

            'Init file instance
            file = New cFile_V3

            'Open file
            If Not file.OpenRead(Filepath) Then
                WorkerMsg(tMsgID.Err, "Failed to load Gear Shift Polygon File! '" & Filepath & "'", MsgSrc)
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


        Public Sub SetGenericShiftPoly(Optional ByRef fld0 As cFLD = Nothing, Optional ByVal nidle As Single = -1)
            Dim Tmax As Single

            'Clear lists
            gs_Mup.Clear()
            gs_Mdown.Clear()
            gs_nUdown.Clear()
            gs_nUup.Clear()

            If fld0 Is Nothing Then fld0 = FLD(MyGear)
            If nidle < 0 Then nidle = ENG.Nidle

            Tmax = fld0.Tmax

            gs_nUdown.Add(nidle)
            gs_nUdown.Add(nidle)
            gs_nUdown.Add((fld0.Npref + fld0.Nlo) / 2)

            gs_Mdown.Add(0)
            gs_Mdown.Add(Tmax * nidle / (fld0.Npref + fld0.Nlo - nidle))
            gs_Mdown.Add(Tmax)

            gs_nUup.Add(fld0.Npref)
            gs_nUup.Add(fld0.Npref)
            gs_nUup.Add(fld0.N95h)

            gs_Mup.Add(0)
            gs_Mup.Add(Tmax * (fld0.Npref - nidle) / (fld0.N95h - nidle))
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


    
    End Class



End Class
