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

Public Class cVEH

    'V2 MassMax is now saved in [t] instead of [kg]
    Private Const FormatVersion As Short = 5
    Private FileVersion As Short

    Private sFilePath As String
    Private MyPath As String

    Public Mass As Single
    Public Loading As Single
    Private siFr0 As Single

    Public Cd0 As Single
    Public Aquers As Single

    Public Cd02 As Single
    Public Aquers2 As Single

    Private Cd0Act As Single
    Private AquersAct As Single

    Public CdMode As tCdMode
    Public CdFile As cSubPath
    Private CdX As List(Of Single)
    Private CdY As List(Of Single)
    Private CdDim As Integer

    Public RtType As tRtType '0=None, 1=Primary, 2=Secondary
    Public RtRatio As Single = 0
    Public RtFile As cSubPath
    Private RtDim As Integer
    Private RtnU As List(Of Single)
    Private RtM As List(Of Single)

    Public rdyn As Single
    Public Axles As List(Of cAxle)
    Public Rim As String
    Private m_red0 As Single

    Public VehCat As tVehCat
    Public MassExtra As Single
    Public MassMax As Single
    Public AxleConf As tAxleConf

    Private MyFileList As List(Of String)

    Public SavedInDeclMode As Boolean


    Public Class cAxle
        Public RRC As Single
        Public Share As Single
        Public TwinTire As Boolean
        Public FzISO As Single
        Public Wheels As String
        Public Inertia As Single
    End Class



    Public Function CreateFileList() As Boolean

        MyFileList = New List(Of String)

        '.vcdv  / .vcdb
        If Me.CdMode <> tCdMode.ConstCd0 Then MyFileList.Add(Me.CdFile.FullPath)

        'Retarder
        If Me.RtType <> tRtType.None Then MyFileList.Add(Me.RtFile.FullPath)

        Return True

    End Function



    Public Sub New()
        MyPath = ""
        sFilePath = ""
        CdFile = New cSubPath
        CdX = New List(Of Single)
        CdY = New List(Of Single)
        RtFile = New cSubPath
        RtnU = New List(Of Single)
        RtM = New List(Of Single)
        Axles = New List(Of cAxle)
        SetDefault()
    End Sub

    Private Sub SetDefault()
        Mass = 0
        MassExtra = 0
        Loading = 0
        Cd0 = 0
        Aquers = 0
        Cd0Act = Cd0
        AquersAct = Aquers
        Cd02 = 0
        Aquers2 = 0
        CdFile.Clear()
        CdMode = tCdMode.ConstCd0
        CdX.Clear()
        CdY.Clear()
        CdDim = -1

        siFr0 = 0
        rdyn = 0
        Rim = ""

        RtType = tRtType.None
        RtRatio = 0
        RtnU.Clear()
        RtM.Clear()
        RtFile.Clear()
        Axles.Clear()
        VehCat = tVehCat.Undef
        MassMax = 0
        AxleConf = tAxleConf.Undef

        SavedInDeclMode = False

    End Sub

    Public Function Validate() As Boolean
        Dim MsgSrc As String
        Dim Check As Boolean = True

        MsgSrc = "VEH/Validate"

        If rdyn < 100 Then
            WorkerMsg(tMsgID.Err, "Parameter 'Dynamic Tire Radius' is invalid (" & rdyn & "mm).", MsgSrc, sFilePath)
            Check = False
        End If

        Return Check

    End Function

    Public Function ReadFile(Optional ByVal ShowMsg As Boolean = True) As Boolean
        Dim Itemp As Single
        Dim a0 As cAxle
        Dim JSON As New cJSON
        Dim dic As Object

        Dim MsgSrc As String


        MsgSrc = "VEH/ReadFile"

        SetDefault()

        If Not JSON.ReadFile(sFilePath) Then Return False

        Try

            FileVersion = JSON.Content("Header")("FileVersion")

            If FileVersion > 4 Then
                SavedInDeclMode = JSON.Content("Body")("SavedInDeclMode")
            Else
                SavedInDeclMode = Cfg.DeclMode
            End If


            Mass = JSON.Content("Body")("CurbWeight")
            MassExtra = JSON.Content("Body")("CurbWeightExtra")
            Loading = JSON.Content("Body")("Loading")
            MassMax = JSON.Content("Body")("MassMax")
            If FileVersion < 2 Then MassMax /= 1000

            Cd0 = JSON.Content("Body")("Cd")
            Aquers = JSON.Content("Body")("CrossSecArea")

            Cd02 = Cd0
            Aquers2 = Aquers

            If FileVersion < 4 Then
                If Not JSON.Content("Body")("CdRigid") Is Nothing Then Cd02 = JSON.Content("Body")("CdRigid")
                If Not JSON.Content("Body")("CrossSecAreaRigid") Is Nothing Then Aquers2 = JSON.Content("Body")("CrossSecAreaRigid")
            Else
                If Not JSON.Content("Body")("Cd2") Is Nothing Then Cd02 = JSON.Content("Body")("Cd2")
                If Not JSON.Content("Body")("CrossSecArea2") Is Nothing Then Aquers2 = JSON.Content("Body")("CrossSecArea2")
            End If

            Cd0Act = Cd0
            AquersAct = Aquers

            If FileVersion < 3 Then
                Itemp = JSON.Content("Body")("WheelsInertia")
                rdyn = 1000 * JSON.Content("Body")("WheelsDiaEff") / 2
                Rim = "-"
            Else
                Rim = JSON.Content("Body")("Rim")
                rdyn = JSON.Content("Body")("rdyn")
            End If

            CdMode = CdModeConv(JSON.Content("Body")("CdCorrMode").ToString)
            If Not JSON.Content("Body")("CdCorrFile") Is Nothing Then CdFile.Init(MyPath, JSON.Content("Body")("CdCorrFile"))

            If JSON.Content("Body")("Retarder") Is Nothing Then
                RtType = tRtType.None
            Else
                RtType = RtTypeConv(JSON.Content("Body")("Retarder")("Type").ToString)
                If Not JSON.Content("Body")("Retarder")("Ratio") Is Nothing Then RtRatio = JSON.Content("Body")("Retarder")("Ratio")
                If Not JSON.Content("Body")("Retarder")("File") Is Nothing Then RtFile.Init(MyPath, JSON.Content("Body")("Retarder")("File"))
            End If

            VehCat = ConvVehCat(JSON.Content("Body")("VehCat").ToString)
            AxleConf = ConvAxleConf(JSON.Content("Body")("AxleConfig")("Type").ToString)

            For Each dic In JSON.Content("Body")("AxleConfig")("Axles")

                a0 = New cAxle

                If FileVersion < 3 Then
                    a0.Wheels = "-"
                Else
                    a0.Inertia = CSng(dic("Inertia"))
                    a0.Wheels = CStr(dic("Wheels"))
                End If

                a0.Share = CSng(dic("AxleWeightShare"))
                a0.TwinTire = CBool(dic("TwinTyres"))
                a0.RRC = CSng(dic("RRCISO"))
                a0.FzISO = CSng(dic("FzISO"))

                Axles.Add(a0)

            Next

            If FileVersion < 3 Then
                For Each a0 In Axles
                    If a0.TwinTire Then
                        a0.Inertia = Itemp / (4 * Axles.Count)
                    Else
                        a0.Inertia = Itemp / (2 * Axles.Count)
                    End If
                Next
            End If



        Catch ex As Exception
            If ShowMsg Then WorkerMsg(tMsgID.Err, "Failed to read Vehicle file! " & ex.Message, MsgSrc)
            Return False
        End Try

        Return True



    End Function

    Public Function SaveFile() As Boolean
        Dim dic As Dictionary(Of String, Object)
        Dim dic0 As Dictionary(Of String, Object)
        Dim ls As List(Of Dictionary(Of String, Object))
        Dim a0 As cAxle
        Dim JSON As New cJSON


        'Header
        dic = New Dictionary(Of String, Object)
        dic.Add("CreatedBy", Lic.LicString & " (" & Lic.GUID & ")")
        dic.Add("Date", Now.ToString)
        dic.Add("AppVersion", VECTOvers)
        dic.Add("FileVersion", FormatVersion)
        JSON.Content.Add("Header", dic)

        'Body
        dic = New Dictionary(Of String, Object)

        dic.Add("SavedInDeclMode", Cfg.DeclMode)
        SavedInDeclMode = Cfg.DeclMode

        dic.Add("VehCat", ConvVehCat(VehCat, False))

        dic.Add("CurbWeight", Mass)
        dic.Add("CurbWeightExtra", MassExtra)
        dic.Add("Loading", Loading)
        dic.Add("MassMax", MassMax)

        dic.Add("Cd", Cd0)
        dic.Add("CrossSecArea", Aquers)

        If Cd02 > 0 And Aquers2 > 0 Then
            dic.Add("Cd2", Cd02)
            dic.Add("CrossSecArea2", Aquers2)
        End If

        dic.Add("rdyn", rdyn)
        dic.Add("Rim", Rim)


        dic.Add("CdCorrMode", CdModeConv(CdMode))
        dic.Add("CdCorrFile", CdFile.PathOrDummy)

        dic0 = New Dictionary(Of String, Object)
        dic0.Add("Type", RtTypeConv(RtType))
        dic0.Add("Ratio", RtRatio)
        dic0.Add("File", RtFile.PathOrDummy)
        dic.Add("Retarder", dic0)

        ls = New List(Of Dictionary(Of String, Object))
        For Each a0 In Axles
            dic0 = New Dictionary(Of String, Object)

            dic0.Add("Inertia", a0.Inertia)
            dic0.Add("Wheels", a0.Wheels)
            dic0.Add("AxleWeightShare", a0.Share)
            dic0.Add("TwinTyres", a0.TwinTire)
            dic0.Add("RRCISO", a0.RRC)
            dic0.Add("FzISO", a0.FzISO)
            ls.Add(dic0)
        Next

        dic0 = New Dictionary(Of String, Object)
        dic0.Add("Type", ConvAxleConf(AxleConf))
        dic0.Add("Axles", ls)
        dic.Add("AxleConfig", dic0)

        JSON.Content.Add("Body", dic)

        Return JSON.WriteFile(sFilePath)


    End Function


	Public Function DeclInitCycle() As Boolean
		Dim al As List(Of Single)
		Dim i As Integer
		Dim a As Single
		Dim a0 As cAxle
		Dim MissionID As tMission
		Dim MsgSrc As String

		MsgSrc = "VEH/DeclInit"

		MissionID = Declaration.CurrentMission.MissionID

		MassExtra = Declaration.SegRef.GetBodyTrWeight(MissionID)


		al = Declaration.SegRef.AxleShares(MissionID)

		If al.Count > Axles.Count Then
			WorkerMsg(tMsgID.Err, "Invalid number of axles! Defined: " & Axles.Count & ", required: " & al.Count, MsgSrc)
			Return False
		End If

		i = -1
		For Each a In al
			i += 1
			Axles(i).Share = a / 100
		Next

		'Remove non-Truck axles
		Do While Axles.Count > al.Count
			Axles.RemoveAt(Axles.Count - 1)
		Loop


		'(Semi-) Trailer
		If Not Declaration.SegRef.TrailerOnlyInLongHaul OrElse MissionID = tMission.LongHaul Then
			al = Declaration.SegRef.AxleSharesTr(MissionID)
			For Each a In al

				a0 = New cAxle

				a0.Inertia = 0	 'Defined later
				a0.Wheels = "385/65 R 22.5"

				a0.Share = a / 100
				a0.TwinTire = False
				a0.RRC = cDeclaration.RRCTr
				a0.FzISO = cDeclaration.FzISOTr

				Axles.Add(a0)

			Next
		End If

		'Wheels Inertias
		For Each a0 In Axles
			a0.Inertia = Declaration.WheelsInertia(a0.Wheels)

			If a0.Inertia < 0 Then
				WorkerMsg(tMsgID.Err, "Selected wheels (" & a0.Wheels & ") are not supported!", MsgSrc)
				Return False
			End If

		Next

		CdMode = tCdMode.CdOfVdecl
		If Not Declaration.SegRef.VCDVparam.ContainsKey(MissionID) Then
			WorkerMsg(tMsgID.Err, "No Cross Wind Correction parameters defined for current vehicle & mission profile!", MsgSrc)
			Return False
		End If

		If Declaration.SegRef.TrailerOnlyInLongHaul Then

			If MissionID = tMission.LongHaul Then
				Cd0Act = Cd0
				AquersAct = Aquers
			Else
				Cd0Act = Cd02
				AquersAct = Aquers2
			End If

		Else

			Cd0Act = Cd0
			AquersAct = Aquers

		End If

		If Axles.Count < 2 Then
			rdyn = -1
		Else
			rdyn = Declaration.rdyn(Axles(1).Wheels, Rim)
		End If

		If rdyn < 0 Then
			WorkerMsg(tMsgID.Err, "Failed to calculate dynamic tire radius! Check wheels/rims", MsgSrc)
			Return False
		End If

		Return True

	End Function

    Public Function DeclInitLoad(ByVal LoadingID As tLoading) As Boolean
        Dim lmax As Single
        Dim MissionID As tMission
        Dim MsgSrc As String

        MsgSrc = "VEH/DeclInit"

        MissionID = Declaration.CurrentMission.MissionID


        lmax = MassMax * 1000 - Mass - MassExtra

        Select Case LoadingID
            Case tLoading.FullLoaded
                Loading = lmax

            Case tLoading.RefLoaded
                Loading = Declaration.SegRef.GetLoading(MissionID, MassMax)
                If Loading < 0 Then
                    WorkerMsg(tMsgID.Err, "Invalid loading in segement table!", MsgSrc)
                    Return False
                End If

                If Loading > lmax Then
                    WorkerMsg(tMsgID.Warn, "Reference loading > Max. loading! Using max. loading.", MsgSrc)
                    Loading = lmax
                End If

            Case tLoading.EmptyLoaded
                Loading = 0

            Case Else ' tLoading.EmptyLoaded
                WorkerMsg(tMsgID.Err, "tLoading.UserDefLoaded not allowed!", MsgSrc)
                Return False

        End Select

        Return True

    End Function


    Public Function VehmodeInit() As Boolean

        Dim MsgSrc As String
        Dim a0 As cAxle
        Dim ShareSum As Double
        Dim RRC As Double
        Dim nrwheels As Single

        MsgSrc = "VEH/Init"

        'Cd-Init
        If Not CdInit() Then Return False

        'Transmission Loss Maps
        If Not GBX.TrLossMapInit Then
            WorkerMsg(tMsgID.Err, "Failed to initialize Transmission Loss Maps!", MsgSrc)
            Return False
        End If

        'Retarder
        If Not RtInit() Then Return False

        'Fr0
        If Axles.Count < 2 Then
            WorkerMsg(tMsgID.Err, "At least 2 axle configurations are required!", MsgSrc, "<GUI>" & sFilePath)
            Return False
        End If

        'Check if sum=100%
        ShareSum = 0
        For Each a0 In Axles
            ShareSum += a0.Share
        Next

        If Math.Abs(ShareSum - 1) > 0.0001 Then
            WorkerMsg(tMsgID.Err, "Sum of relative axle shares is not 100%!", MsgSrc, "<GUI>" & sFilePath)
            Return False
        End If

        If rdyn <= 0 Then
            WorkerMsg(tMsgID.Err, "rdyn is invalid!", MsgSrc, "<GUI>" & sFilePath)
            Return False
        End If

        RRC = 0
        m_red0 = 0
        For Each a0 In Axles

            If a0.RRC < -0.000001 Then
                WorkerMsg(tMsgID.Err, "Invalid RRC value! (" & a0.RRC & ")", MsgSrc, "<GUI>" & sFilePath)
                Return False
            End If

            If a0.FzISO < 0.00001 Then
                WorkerMsg(tMsgID.Err, "Invalid FzISO value! (" & a0.FzISO & ")", MsgSrc, "<GUI>" & sFilePath)
                Return False
            End If

            If a0.TwinTire Then
                nrwheels = 4
            Else
                nrwheels = 2
            End If

            RRC += a0.Share * (a0.RRC * ((Loading + Mass + MassExtra) * a0.Share * 9.81 / (a0.FzISO * nrwheels)) ^ (0.9 - 1))     'Beta=0.9

            m_red0 += nrwheels * a0.Inertia / ((rdyn / 1000) ^ 2)

        Next

        siFr0 = RRC

        Return True

    End Function

 




#Region "Cd Funktionen"

    Private Function CdInit() As Boolean
        Dim file As cFile_V3
        Dim MsgSrc As String
        Dim line As String()

        MsgSrc = "VEH/CdInit"

        'Warn If Vair specified in DRI but CdType != CdOfBeta
        If DRI.VairVorg Xor CdMode = tCdMode.CdOfBeta Then

            If DRI.VairVorg Then
                WorkerMsg(tMsgID.Warn, "Vair input in driving cycle will be irgnored! (Side wind correction disabled in .veh file)", MsgSrc)
            Else
                WorkerMsg(tMsgID.Err, "No Vair input in driving cycle defined! Vres and Beta is required!", MsgSrc)
                Return False
            End If

        End If

        'If Cd-value is constant then do nothing
		If CdMode = tCdMode.ConstCd0 Then Return True

		'Declaration Mode
		If CdMode = tCdMode.CdOfVdecl Then Return CdofVdeclInit()


		'Read Inputfile
		file = New cFile_V3

		If Not file.OpenRead(CdFile.FullPath) Then
			WorkerMsg(tMsgID.Err, "Failed to read Cd input file! (" & CdFile.FullPath & ")", MsgSrc)
			Return False
		End If

		'Skip Header
		file.ReadLine()

		CdX.Clear()
		CdY.Clear()
		CdDim = -1
		Do While Not file.EndOfFile

			CdDim += 1
			line = file.ReadLine

			Try
				CdX.Add(CSng(line(0)))
				CdY.Add(CSng(line(1)))
			Catch ex As Exception
				WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & CdDim + 1 & " (" & CdFile.FullPath & ")", MsgSrc, CdFile.FullPath)
				file.Close()
				Return False
			End Try

		Loop

		file.Close()

		If CdDim < 1 Then
			WorkerMsg(tMsgID.Err, "Cd input file invalid! Two or more lines required! (" & CdFile.FullPath & ")", MsgSrc, CdFile.FullPath)
			Return False
		End If

		Return True

	End Function

	Private Function CdofVdeclInit() As Boolean
		Dim lBeta As New List(Of Single)
		Dim lDeltaCdA As New List(Of Single)
		Dim i As Integer
		Dim j As Integer
		Dim k As Integer
		Dim vveh As Single
		Dim alpha As Single
		Dim beta As Single
		Dim CdA0 As Single
		Dim CdA As Single
		Dim CdAsum As Single
		Dim Vwind As Single
		Dim VwindX As Single
		Dim VwindY As Single
		Dim vair As Single
		Dim vairX As Single
		Dim vairY As Single
		Dim a As List(Of Single)
		Dim iDim As Integer
		Dim DeltaCdA As Single
		Dim share As Single

		CdA0 = Cd0Act * AquersAct
		Vwind = cDeclaration.Vwind * 3.6

		Try
			If Cfg.DeclMode Then
				a = Declaration.SegRef.VCDVparam(Declaration.CurrentMission.MissionID)
			Else
				a = Declaration.VCDVparamPerCat(VEH.VehCat)
			End If
		Catch ex As Exception
			Return False
		End Try

		For i = 0 To 12
			beta = CSng(i)
			lBeta.Add(beta)
			lDeltaCdA.Add(a(0) * beta + a(1) * beta ^ 2 + a(2) * beta ^ 3)
		Next

		iDim = lBeta.Count - 1

		CdX.Clear()
		CdY.Clear()
		CdDim = -1

		CdX.Add(0)
		CdY.Add(0)
		For i = 60 To 100 Step 5
			vveh = CSng(i)

			CdAsum = 0
			For j = 0 To 180 Step 10
				alpha = CSng(j)
				VwindX = Vwind * Math.Cos(alpha * Math.PI / 180)
				VwindY = Vwind * Math.Sin(alpha * Math.PI / 180)
				vairX = vveh + VwindX
				vairY = VwindY
				vair = Math.Sqrt(vairX ^ 2 + vairY ^ 2)
				beta = Math.Atan(vairY / vairX) * 180 / Math.PI

				If lBeta(0) >= beta Then
					k = 1
				Else
					k = 0
					Do While lBeta(k) < beta And k < iDim
						k += 1
					Loop
				End If
				DeltaCdA = (beta - lBeta(k - 1)) * (lDeltaCdA(k) - lDeltaCdA(k - 1)) / (lBeta(k) - lBeta(k - 1)) + lDeltaCdA(k - 1)

				CdA = CdA0 + DeltaCdA

				If j = 0 OrElse j = 180 Then
					share = 5 / 180
				Else
					share = 10 / 180
				End If

				CdAsum += share * CdA * (vair ^ 2 / vveh ^ 2)

			Next

			CdX.Add(vveh)
			CdY.Add(CdAsum)

		Next

		CdY(0) = CdY(1)
		CdDim = CdX.Count - 1

		Return True


	End Function

    Public Function Cd(ByVal x As Single) As Single
        Return CdIntpol(x) * Cd0Act
	End Function

	Public Function CdA(ByVal vveh As Single) As Single
		Return CdIntpol(vveh)
	End Function

    Public Function Cd() As Single
        Return Cd0Act
    End Function

    Public Function CrossSecArea() As Single
        Return AquersAct
    End Function

    Private Function CdIntpol(ByVal x As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If CdX(0) >= x Then
            If CdX(0) > x Then
                If CdMode = tCdMode.CdOfBeta Then
                    MODdata.ModErrors.CdExtrapol = "β= " & x
                Else
                    MODdata.ModErrors.CdExtrapol = "v= " & x
                End If
            End If
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While CdX(i) < x And i < CdDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If CdX(i) < x Then
            If CdMode = tCdMode.CdOfBeta Then
                MODdata.ModErrors.CdExtrapol = "β= " & x
            Else
                MODdata.ModErrors.CdExtrapol = "v= " & x
            End If
        End If

lbInt:
        'Interpolation
        Return (x - CdX(i - 1)) * (CdY(i) - CdY(i - 1)) / (CdX(i) - CdX(i - 1)) + CdY(i - 1)

    End Function

#End Region

#Region "Retarder"

    Private Function RtInit() As Boolean
        Dim file As cFile_V3
        Dim MsgSrc As String
        Dim line As String()

        MsgSrc = "VEH/RtInit"

        If RtType = tRtType.None Then Return True

        'Read Inputfile
        file = New cFile_V3
        If Not file.OpenRead(RtFile.FullPath) Then
            WorkerMsg(tMsgID.Err, "Failed to read Retarder input file! (" & RtFile.FullPath & ")", MsgSrc)
            Return False
        End If

        'Skip Header
        file.ReadLine()

        RtDim = -1
        Do While Not file.EndOfFile

            RtDim += 1
            line = file.ReadLine

            Try
                RtnU.Add(CSng(line(0)))
                RtM.Add(CSng(line(1)))
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & RtDim + 1 & " (" & RtFile.FullPath & ")", MsgSrc, RtFile.FullPath)
                file.Close()
                Return False
            End Try

        Loop

        file.Close()

        If RtDim < 1 Then
            WorkerMsg(tMsgID.Err, "Retarder input file invalid! Two or more lines required! (" & RtFile.FullPath & ")", MsgSrc, RtFile.FullPath)
            Return False
        End If

        Return True

    End Function


    Public Function RtPeLoss(ByVal v As Single, ByVal Gear As Integer) As Single
        Dim M As Single
        Dim nU As Single

        Select Case RtType

            Case tRtType.Primary
                nU = (60 * v) / (2 * VEH.rdyn * Math.PI / 1000) * GBX.Igetr(0) * GBX.Igetr(Gear) * RtRatio

            Case tRtType.Secondary
                nU = (60 * v) / (2 * VEH.rdyn * Math.PI / 1000) * GBX.Igetr(0) * RtRatio

            Case Else 'tRtType.None
                Return 0

        End Select

        M = RtIntpol(nU)

        Return M * nU * 2 * Math.PI / 60 / 1000

    End Function

    Private Function RtIntpol(ByVal nU As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If RtnU(0) >= nU Then
            If RtnU(0) > nU Then MODdata.ModErrors.RtExtrapol = "n= " & nU & " [1/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While RtnU(i) < nU And i < RtDim
            i += 1
        Loop

        'Extrapolation for x> x(imax)
        If RtnU(i) < nU Then MODdata.ModErrors.RtExtrapol = "n= " & nU & " [1/min]"

lbInt:
        'Interpolation
        Return (nU - RtnU(i - 1)) * (RtM(i) - RtM(i - 1)) / (RtnU(i) - RtnU(i - 1)) + RtM(i - 1)

    End Function

#End Region

#Region "Properties"

    Public ReadOnly Property FileList As List(Of String)
        Get
            Return MyFileList
        End Get
    End Property

    Public ReadOnly Property m_red As Single
        Get
            Return m_red0
        End Get
    End Property

    Public Property Fr0 As Single
        Get
            Return siFr0
        End Get
        Set(ByVal value As Single)
            siFr0 = value
        End Set
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










#End Region


End Class

