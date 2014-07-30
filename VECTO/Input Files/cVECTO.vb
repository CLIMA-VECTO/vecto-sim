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

Public Class cVECTO

    Private Const FormatVersion As Short = 2
    Private FileVersion As Short

    Private sFilePath As String

    Private MyPath As String

    'Input parameters
    Private stPathVEH As cSubPath
    Private stPathENG As cSubPath
    Private stPathGBX As cSubPath

    Private boStartStop As Boolean
    Private siStStV As Single
    Private siStStT As Single
    Public StStDelay As Integer

    Private stDesMaxFile As cSubPath
    Private laDesV As List(Of Single)
    Private laDesMax As List(Of Single)
    Private laDesMin As List(Of Single)
    Private DesMaxDim As Integer

    Public AuxPaths As Dictionary(Of String, cAuxEntry)
    Public AuxRefs As Dictionary(Of String, cAux)          'Alle Nebenverbraucher die in der Veh-Datei UND im Zyklus definiert sind
    Public AuxDef As Boolean                               'True wenn ein oder mehrere Nebenverbraucher definiert sind
    Public EStechs As List(Of String)

    Public CycleFiles As List(Of cSubPath)

    Public EngOnly As Boolean

    Public a_lookahead As Single
    Public vMin As Single
    Public vMinLA As Single
    Public LookAheadOn As Boolean
    Public OverSpeedOn As Boolean
    Public OverSpeed As Single
    Public UnderSpeed As Single
    Public EcoRollOn As Boolean

    Private MyFileList As List(Of String)

    Public SavedInDeclMode As Boolean


    Public Class cAuxEntry
        Public Type As String
        Public Path As cSubPath
        Public TechStr As String = ""

        Public Sub New()
            Path = New cSubPath
        End Sub

    End Class

    Public Function CreateFileList() As Boolean
        Dim Aux0 As cAuxEntry
        Dim sb As cSubPath
        Dim str As String

        MyFileList = New List(Of String)

        '.vecto
        MyFileList.Add(Me.sFilePath)

        'Veh
        If Not Me.EngOnly Then
            MyFileList.Add(Me.PathVEH)

            If Not VEH.CreateFileList Then Return False
            For Each str In VEH.FileList
                MyFileList.Add(str)
            Next
        End If

        'Eng
        MyFileList.Add(Me.PathENG)

        If Not ENG.CreateFileList Then Return False
        For Each str In ENG.FileList
            MyFileList.Add(str)
        Next

        If Not Me.EngOnly Then

            'Gbx
            MyFileList.Add(Me.PathGBX)

            If Not GBX.CreateFileList Then Return False
            For Each str In GBX.FileList
                MyFileList.Add(str)
            Next

            'Aux
            If AuxDef And Not Cfg.DeclMode Then
                For Each Aux0 In Me.AuxPaths.Values
                    MyFileList.Add(Aux0.Path.FullPath)
                Next
            End If

            '.vacc
            MyFileList.Add(Me.stDesMaxFile.FullPath)

        End If

        'Cycles
        For Each sb In Me.CycleFiles
            MyFileList.Add(sb.FullPath)
        Next


        Return True

    End Function

    Public Sub New()

        MyPath = ""
        sFilePath = ""

        stPathVEH = New cSubPath
        stPathENG = New cSubPath
        stPathGBX = New cSubPath

        stDesMaxFile = New cSubPath

        laDesV = New List(Of Single)
        laDesMax = New List(Of Single)
        laDesMin = New List(Of Single)

        AuxPaths = New Dictionary(Of String, cAuxEntry)
        AuxRefs = New Dictionary(Of String, cAux)
        AuxDef = False
        EStechs = New List(Of String)

        CycleFiles = New List(Of cSubPath)

    End Sub

    Public Function SaveFile() As Boolean
        Dim AuxEntryKV As KeyValuePair(Of String, cAuxEntry)
        'Dim s As String
        Dim sb As cSubPath
        Dim JSON As New cJSON
        Dim ls As List(Of Object)
        Dim dic As Dictionary(Of String, Object)
        Dim dic0 As Dictionary(Of String, Object)

        'Header
        dic = New Dictionary(Of String, Object)
        dic.Add("CreatedBy", Lic.LicString & " (" & Lic.GUID & ")")
        dic.Add("Date", Now.ToString)
        dic.Add("AppVersion", VECTOvers)
        dic.Add("FileVersion", FormatVersion)
        JSON.Content.Add("Header", dic)

        'Body
        dic0 = New Dictionary(Of String, Object)

        dic0.Add("SavedInDeclMode", Cfg.DeclMode)
        SavedInDeclMode = Cfg.DeclMode

        'Main Files
        dic0.Add("VehicleFile", stPathVEH.PathOrDummy)
        dic0.Add("EngineFile", stPathENG.PathOrDummy)
        dic0.Add("GearboxFile", stPathGBX.PathOrDummy)

        'Cycles
        If CycleFiles.Count > 0 Then
            ls = New List(Of Object)
            For Each sb In CycleFiles
                ls.Add(sb.PathOrDummy)
            Next
            dic0.Add("Cycles", ls)
        End If

        'Aux
        If AuxPaths.Count > 0 Then
            ls = New List(Of Object)
            For Each AuxEntryKV In AuxPaths
                dic = New Dictionary(Of String, Object)
                dic.Add("ID", Trim(UCase(AuxEntryKV.Key)))
                dic.Add("Type", AuxEntryKV.Value.Type)
                dic.Add("Path", AuxEntryKV.Value.Path.PathOrDummy)
                dic.Add("Technology", AuxEntryKV.Value.TechStr)

                If AuxEntryKV.Key = sKey.AUX.ElecSys Then
                    dic.Add("TechList", EStechs)
                End If

                ls.Add(dic)
            Next
            dic0.Add("Aux", ls)
        End If

        'VACC
        dic0.Add("VACC", stDesMaxFile.PathOrDummy)

        'EngineOnlyMode
        dic0.Add("EngineOnlyMode", EngOnly)

        'Start Stop
        dic = New Dictionary(Of String, Object)
        dic.Add("Enabled", boStartStop)
        dic.Add("MaxSpeed", siStStV)
        dic.Add("MinTime", siStStT)
        dic.Add("Delay", StStDelay)
        dic0.Add("StartStop", dic)

        'LAC
        dic = New Dictionary(Of String, Object)
        dic.Add("Enabled", LookAheadOn)
        dic.Add("Dec", a_lookahead)
        dic.Add("MinSpeed", vMinLA)
        dic0.Add("LAC", dic)

        'Overspeed / EcoRoll
        dic = New Dictionary(Of String, Object)
        If EcoRollOn Then
            dic.Add("Mode", "EcoRoll")
        ElseIf OverSpeedOn Then
            dic.Add("Mode", "OverSpeed")
        Else
            dic.Add("Mode", "Off")
        End If
        dic.Add("MinSpeed", vMin)
        dic.Add("OverSpeed", OverSpeed)
        dic.Add("UnderSpeed", UnderSpeed)
        dic0.Add("OverSpeedEcoRoll", dic)


        JSON.Content.Add("Body", dic0)

        Return JSON.WriteFile(sFilePath)

    End Function

    Public Function ReadFile() As Boolean
        Dim AuxEntry As cAuxEntry
        Dim AuxID As String
        Dim MsgSrc As String
        Dim SubPath As cSubPath
        Dim JSON As New cJSON
        Dim str As String
        Dim dic As Object

        MsgSrc = "Main/ReadInp/GEN"

        SetDefault()

        If Not JSON.ReadFile(sFilePath) Then Return False

        Try

            FileVersion = JSON.Content("Header")("FileVersion")

            If FileVersion > 1 Then
                SavedInDeclMode = JSON.Content("Body")("SavedInDeclMode")
            Else
                SavedInDeclMode = Cfg.DeclMode
            End If

            If Not JSON.Content("Body")("VehicleFile") Is Nothing Then stPathVEH.Init(MyPath, JSON.Content("Body")("VehicleFile"))

            stPathENG.Init(MyPath, JSON.Content("Body")("EngineFile"))

            If Not JSON.Content("Body")("GearboxFile") Is Nothing Then stPathGBX.Init(MyPath, JSON.Content("Body")("GearboxFile"))

            If Not JSON.Content("Body")("Cycles") Is Nothing Then
                For Each str In JSON.Content("Body")("Cycles")
                    SubPath = New cSubPath
                    SubPath.Init(MyPath, str)
                    CycleFiles.Add(SubPath)
                Next
            End If

            If Not JSON.Content("Body")("Aux") Is Nothing Then
                For Each dic In JSON.Content("Body")("Aux")

                    AuxID = UCase(Trim(dic("ID").ToString))

                    If AuxPaths.ContainsKey(AuxID) Then
                        WorkerMsg(tMsgID.Err, "Multiple definitions of the same auxiliary type (" & AuxID & ")!", MsgSrc)
                        Return False
                    End If

                    AuxEntry = New cAuxEntry

                    AuxEntry.Type = dic("Type")
                    AuxEntry.Path.Init(MyPath, dic("Path"))

                    If Not dic("Technology") Is Nothing Then AuxEntry.TechStr = dic("Technology")

                    AuxPaths.Add(AuxID, AuxEntry)

                    AuxDef = True

                    If AuxID = sKey.AUX.ElecSys Then
                        If Not dic("TechList") Is Nothing Then
                            For Each str In dic("TechList")
                                EStechs.Add(str)
                            Next
                        End If
                    End If

                Next
            End If

            If Not JSON.Content("Body")("VACC") Is Nothing Then stDesMaxFile.Init(MyPath, JSON.Content("Body")("VACC"))

            EngOnly = JSON.Content("Body")("EngineOnlyMode")

            If Not JSON.Content("Body")("StartStop") Is Nothing Then
                dic = JSON.Content("Body")("StartStop")
                boStartStop = dic("Enabled")
                siStStV = dic("MaxSpeed")
                siStStT = dic("MinTime")
                StStDelay = dic("Delay")
            Else
                boStartStop = False
            End If

            If Not JSON.Content("Body")("LAC") Is Nothing Then
                dic = JSON.Content("Body")("LAC")
                LookAheadOn = dic("Enabled")
                a_lookahead = dic("Dec")
                vMinLA = dic("MinSpeed")
            Else
                LookAheadOn = False
            End If

            If Not JSON.Content("Body")("OverSpeedEcoRoll") Is Nothing Then

                dic = JSON.Content("Body")("OverSpeedEcoRoll")

                Select Case UCase(dic("Mode").ToString).Trim
                    Case "ECOROLL"
                        OverSpeedOn = False
                        EcoRollOn = True

                    Case "OVERSPEED"
                        OverSpeedOn = True
                        EcoRollOn = False

                    Case "OFF"
                        OverSpeedOn = False
                        EcoRollOn = False

                    Case Else
                        WorkerMsg(tMsgID.Err, "Value '" & dic("Mode") & "' is not valid for OverSpeedEcoRoll/Mode!", MsgSrc)
                        Return False
                End Select

                vMin = dic("MinSpeed")
                OverSpeed = dic("OverSpeed")
                If Not dic("UnderSpeed") Is Nothing Then UnderSpeed = dic("UnderSpeed")

            Else
                OverSpeedOn = False
                EcoRollOn = False
            End If

        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Failed to read VECTO file! " & ex.Message, MsgSrc)
            Return False
        End Try


        Return True


    End Function

    Private Sub SetDefault()
        boStartStop = False
        siStStV = 5
        siStStT = 5
        StStDelay = 0
        FileVersion = 0

        stPathVEH.Clear()
        stPathENG.Clear()
        CycleFiles.Clear()
        stPathGBX.Clear()

        stDesMaxFile.Clear()
        laDesV.Clear()
        laDesMax.Clear()
        laDesMin.Clear()
        DesMaxDim = -1

        AuxPaths.Clear()
        AuxRefs.Clear()
        AuxDef = False
        EStechs.Clear()

        EngOnly = False

        a_lookahead = 0
        vMin = 0
        LookAheadOn = True
        OverSpeedOn = False
        EcoRollOn = False
        OverSpeed = 0
        UnderSpeed = 0
        vMinLA = 0

        SavedInDeclMode = False

    End Sub

    Public Function DeclInit() As Boolean

        Dim cl As List(Of String)
        Dim s As String
        Dim SubPath As cSubPath
        Dim MsgSrc As String

        MsgSrc = "VECTO/DeclInit"

        EngOnly = False

        CycleFiles.Clear()

        cl = Declaration.SegRef.GetCycles

        For Each s In cl
            SubPath = New cSubPath
            SubPath.Init(MyPath, s)
            CycleFiles.Add(SubPath)
        Next

        stDesMaxFile.Init(MyPath, Declaration.SegRef.VACCfile)

        siStStV = cDeclaration.SSspeed
        siStStT = cDeclaration.SStime
        StStDelay = cDeclaration.SSdelay

        If Not EcoRollOn Then OverSpeedOn = True

        OverSpeed = cDeclaration.Overspeed
        UnderSpeed = cDeclaration.Underspeed
        vMin = cDeclaration.ECvmin

        LookAheadOn = True
        a_lookahead = cDeclaration.LACa
        vMinLA = cDeclaration.LACvmin

        'No need to check Aux (AuxDef). Will be checked in cDeclaration.CalcInitLoad

        Return True

    End Function

    'This Sub reads those Input-files that do not have their own class, etc.
    Public Function Init() As Boolean
        Dim file As cFile_V3
        Dim line As String()

        Dim MsgSrc As String

        MsgSrc = "VECTO/Init"

        If Not EngOnly Then

            file = New cFile_V3

            If Not file.OpenRead(stDesMaxFile.FullPath) Then
                WorkerMsg(tMsgID.Err, "Can't read .vacc file (" & stDesMaxFile.FullPath & ")", MsgSrc)
                Return False
            End If

            'Skip Header
            file.ReadLine()

            laDesV.Clear()
            laDesMax.Clear()
            laDesMin.Clear()
            DesMaxDim = -1
            Try

                Do While Not file.EndOfFile

                    DesMaxDim += 1

                    line = file.ReadLine

                    laDesV.Add(CSng(line(0)) / 3.6)   'km/h => m/s !!!!
                    laDesMax.Add(CSng(line(1)))
                    laDesMin.Add(CSng(line(2)))

                Loop

            Catch ex As Exception

                file.Close()
                WorkerMsg(tMsgID.Err, "Error in .vacc file. " & ex.Message & " (" & stDesMaxFile.FullPath & ")", MsgSrc, stDesMaxFile.FullPath)
                Return False

            End Try

            file.Close()

        End If

        Return True

    End Function

#Region "Aux"

    Public Function AuxInit() As Boolean

        Dim Aux0 As cAux
        Dim AuxPathKV As KeyValuePair(Of String, cAuxEntry)
        Dim DRIauxcheck As New Dictionary(Of String, Boolean)
        Dim AuxID As String

        Dim MsgSrc As String

        MsgSrc = "VEH/AuxInit"

        AuxRefs = New Dictionary(Of String, cAux)

        If Cfg.DeclMode Then

            For Each AuxPathKV In AuxPaths
                AuxRefs.Add(AuxPathKV.Key, Nothing)
            Next

            Return True

        End If


        If DRI.AuxDef Xor AuxDef Then

            If AuxDef Then
                WorkerMsg(tMsgID.Err, "No auxiliary input defined in driving cycle!", MsgSrc)
                Return False
            Else
                WorkerMsg(tMsgID.Warn, "No auxiliary defined in vehicle file! Psupply input will be ignored!", MsgSrc)
                Return True
            End If

        End If

        If Not (DRI.AuxDef Or AuxDef) Then Return True


        For Each AuxID In DRI.AuxComponents.Keys
            DRIauxcheck.Add(AuxID, False)
        Next

        For Each AuxPathKV In AuxPaths

            MsgSrc = "VEH/AuxInit/" & AuxPathKV.Key

            If Not DRI.AuxComponents.ContainsKey(AuxPathKV.Key) Then
                WorkerMsg(tMsgID.Err, "No Psupply input defined in driving cycle for auxiliary '" & AuxPathKV.Key & "'!", MsgSrc)
                Return False
            End If

            Aux0 = New cAux
            Aux0.Filepath = AuxPathKV.Value.Path.FullPath

            If Not Aux0.Readfile Then
                'Notificationin ReadFile()
                Return False
            End If

            AuxRefs.Add(AuxPathKV.Key, Aux0)

            DRIauxcheck(AuxPathKV.Key) = True

        Next

        MsgSrc = "VEH/AuxInit"

        For Each AuxID In DRI.AuxComponents.Keys
            If Not DRIauxcheck(AuxID) Then WorkerMsg(tMsgID.Warn, "Auxiliary '" & AuxID & "' not found! Psupply input will be ignored!", MsgSrc)
        Next

        Return True

    End Function

    Public Function Paux(ByVal AuxID As String, ByVal t As Integer, ByVal nU As Single) As Single
        Dim Psupply As Single
        Dim Px As Single
        Dim Aux0 As cAux

        Dim MsgSrc As String

        MsgSrc = "VEH/Paux"

        If Cfg.DeclMode Then Return Declaration.AuxPower(AuxID)

        If AuxDef Then

            Aux0 = AuxRefs(AuxID)

            Psupply = DRI.AuxComponents(AuxID)(t)

            If Psupply < 0 Then GoTo lbAuxError

            Px = Aux0.Paux(nU, Psupply)

            If Px < 0 Then GoTo lbAuxError

            Return Px

        Else

            Return 0

        End If


lbAuxError:
        MODdata.ModErrors.AuxNegative = AuxID
        Return 0


    End Function

    Public Function PauxSum(ByVal t As Integer, ByVal nU As Single) As Single
        Dim sum As Single
        Dim AuxID As String

        Dim MsgSrc As String

        MsgSrc = "VEH/Paux"

        If AuxDef Then

            sum = 0

            For Each AuxID In AuxRefs.Keys

                sum += Paux(AuxID, t, nU)

            Next

            Return sum

        Else

            Return 0

        End If

    End Function

#End Region


#Region "Properties"

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


    Public Property PathVEH(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stPathVEH.OriginalPath
            Else
                Return stPathVEH.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stPathVEH.Init(MyPath, value)
        End Set
    End Property

    Public Property PathENG(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stPathENG.OriginalPath
            Else
                Return stPathENG.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stPathENG.Init(MyPath, value)
        End Set
    End Property

    Public Property PathGBX(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stPathGBX.OriginalPath
            Else
                Return stPathGBX.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stPathGBX.Init(MyPath, value)
        End Set
    End Property


    Public Property StartStop() As Boolean
        Get
            Return boStartStop
        End Get
        Set(ByVal value As Boolean)
            boStartStop = value
        End Set
    End Property

    Public Property StStV() As Single
        Get
            Return siStStV
        End Get
        Set(ByVal value As Single)
            siStStV = value
        End Set
    End Property

    Public Property StStT() As Single
        Get
            Return siStStT
        End Get
        Set(ByVal value As Single)
            siStStT = value
        End Set
    End Property

    Public Property DesMaxFile(Optional ByVal Original As Boolean = False) As String
        Get
            If Original Then
                Return stDesMaxFile.OriginalPath
            Else
                Return stDesMaxFile.FullPath
            End If
        End Get
        Set(ByVal value As String)
            stDesMaxFile.Init(MyPath, value)
        End Set
    End Property

#End Region

    Public Function aDesMax(ByVal v As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If laDesV(0) >= v Then
            If laDesV(0) > v Then MODdata.ModErrors.DesMaxExtr = "v= " & v * 3.6 & "[km/h]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While laDesV(i) < v And i < DesMaxDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If laDesV(i) < v Then
            MODdata.ModErrors.DesMaxExtr = "v= " & v * 3.6 & "[km/h]"
        End If

lbInt:
        'Interpolation
        Return (v - laDesV(i - 1)) * (laDesMax(i) - laDesMax(i - 1)) / (laDesV(i) - laDesV(i - 1)) + laDesMax(i - 1)

    End Function

    Public Function aDesMin(ByVal v As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If laDesV(0) >= v Then
            If laDesV(0) > v Then MODdata.ModErrors.DesMaxExtr = "v= " & v * 3.6 & "[km/h]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While laDesV(i) < v And i < DesMaxDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If laDesV(i) < v Then
            MODdata.ModErrors.DesMaxExtr = "v= " & v * 3.6 & "[km/h]"
        End If

lbInt:
        'Interpolation
        Return (v - laDesV(i - 1)) * (laDesMin(i) - laDesMin(i - 1)) / (laDesV(i) - laDesV(i - 1)) + laDesMin(i - 1)

    End Function


End Class

