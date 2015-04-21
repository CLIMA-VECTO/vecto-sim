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
	Private Const FormatVersion As Short = 4
	Private FileVersion As Short

	Private MyPath As String
	Private sFilePath As String

	Public ModelName As String
	Public GbxInertia As Single
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
	Public TCinertia As Single


	Private TCnu As New List(Of Single)
	Private TCmu As New List(Of Single)
	Private TCtorque As New List(Of Single)
	Private TCdim As Integer


	'Torque Converter Iteration Results
	Public TCMin As Single
	Public TCnUin As Single
	Public TC_PeBrake As Single
	Public TCMout As Single
	Public TCnUout As Single
	Public TCReduce As Boolean
	Public TCNeutral As Boolean
	Public TC_mu As Single
	Public TC_nu As Single
	Private TCnuMax As Single


	Private MyFileList As List(Of String)
	Public SavedInDeclMode As Boolean


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
		GbxInertia = 0
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

		TCinertia = 0

		SavedInDeclMode = False
	End Sub

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

		dic.Add("SavedInDeclMode", Cfg.DeclMode)
		SavedInDeclMode = Cfg.DeclMode

		dic.Add("ModelName", ModelName)

		dic.Add("Inertia", GbxInertia)
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
		dic0.Add("Inertia", TCinertia)
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

		SetDefault()

		If Not JSON.ReadFile(sFilePath) Then Return False

		Try

			FileVersion = JSON.Content("Header")("FileVersion")

			If FileVersion > 3 Then
				SavedInDeclMode = JSON.Content("Body")("SavedInDeclMode")
			Else
				SavedInDeclMode = Cfg.DeclMode
			End If

			ModelName = JSON.Content("Body")("ModelName")
			GbxInertia = JSON.Content("Body")("Inertia")
			TracIntrSi = JSON.Content("Body")("TracInt")

			i = - 1
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
				If FileVersion > 2 Then TCinertia = JSON.Content("Body")("TorqueConverter")("Inertia")
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

		GbxInertia = cDeclaration.GbInertia
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

		'Skip Header
		file.ReadLine()

		If TCrefrpm <= 0 Then
			WorkerMsg(tMsgID.Err, "Torque converter reference torque invalid! (" & TCrefrpm & ")", MsgSrc)
			Return False
		End If

		TCnu.Clear()
		TCmu.Clear()
		TCtorque.Clear()
		TCdim = - 1

		Try
			Do While Not file.EndOfFile
				line = file.ReadLine
				If CSng(line(0)) < 1 Then
					TCnu.Add(CSng(line(0)))
					TCmu.Add(CSng(line(1)))
					TCtorque.Add(CSng(line(2)))
					TCdim += 1
				End If
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

		TCnuMax = TCnu(TCdim)


		'Add default values for nu>1
		If Not file.OpenRead(MyDeclPath & "DefaultTC.vtcc") Then
			WorkerMsg(tMsgID.Err, "Default Torque Converter file not found!", MsgSrc)
			Return False
		End If

		'Skip Header
		file.ReadLine()

		Try
			Do While Not file.EndOfFile
				line = file.ReadLine
				TCnu.Add(CSng(line(0)))
				TCmu.Add(CSng(line(1)))
				TCtorque.Add(CSng(line(2)))
				TCdim += 1
			Loop
		Catch ex As Exception
			WorkerMsg(tMsgID.Err, "Error while reading Default Torque Converter file! (" & ex.Message & ")", MsgSrc)
			Return False
		End Try

		file.Close()

		Return True
	End Function

	Public Function TCiteration(ByVal Gear As Integer, ByVal nUout As Single, ByVal PeOut As Single, ByVal t As Integer,
								Optional ByVal LastnU As Single? = Nothing, Optional ByVal LastPe As Single? = Nothing) As Boolean

		Dim i As Integer
		Dim iDim As Integer
		Dim nUin As Single
		Dim Mout As Single
		Dim Min As Single
		Dim MinMax As Single
		Dim MinCalc As Single
		Dim nuStep As Single
		Dim nuMin As Single
		Dim nuMax As Single

		Dim nu As Single
		Dim mu As Single

		Dim MoutCalc As Single

		Dim Paux As Single
		Dim PaMot As Single
		Dim Pfull As Single
		Dim PinMax As Single

		Dim nuList As New List(Of Single)
		Dim McalcRatio As New List(Of Single)

		Dim McalcRatMax As Single
		Dim ErrMin As Single
		Dim iMin As Integer

		Dim Brake As Boolean
		Dim FirstDone As Boolean

		Dim rpmLimit As Single

		Dim MsgSrc As String

		MsgSrc = "GBX/TCiteration/t= " & t + 1

		TC_PeBrake = 0
		TCReduce = False
		nuStep = 0.001
		Brake = False
		TCNeutral = False

		'TC rpm limit
		If DEV.TClimitOn Then
			If MODdata.Vh.a(t) >= DEV.TCaccmin Then
				rpmLimit = DEV.TClimit
			Else
				rpmLimit = ENG.Nrated
			End If
		Else
			rpmLimit = ENG.Nrated
		End If

		'Power to torque
		Mout = nPeToM(nUout, PeOut)


		'Set nu boundaries
		If Mout < 0 Then

			'Speed too low in motoring(check if nu=1 allows enough engine speed)
			If nUout < ENG.Nidle Then
				TCNeutral = True
				Return True
			End If

			nuMin = 1
			nuMax = Math.Min(TCnu(TCdim), nUout/ENG.Nidle)

		Else
			nuMin = Math.Max(nUout / rpmLimit, TCnu(0))
			nuMax = Math.Min(TCnuMax, nUout/ENG.Nidle)
		End If

		If nuMax <= nuMin Then
			TCReduce = True
			Return True
		End If

		'Reduce step size if nu-range is too low
		Do While (nuMax - nuMin)/nuStep < 10 And nuStep > 0.0001
			nuStep *= 0.1
		Loop

		FirstDone = False
		nu = nuMin - nuStep
		iDim = - 1
		Do While nu + nuStep <= nuMax

			'nu
			nu += nuStep

			'Abort if nu<=0
			If nu <= 0 Then Continue Do

			'mu
			mu = fTCmu(nu)

			'Abort if mu<=0
			If mu <= 0 Then Continue Do

			'nIn
			nUin = nUout/nu

			'MinMax
			Paux = MODdata.Px.fPaux(t, Math.Max(nUin, ENG.Nidle))
			If LastnU Is Nothing Then
				PaMot = 0
			Else
				PaMot = MODdata.Px.fPaMot(nUin, LastnU)
			End If
			If LastPe Is Nothing Then
				Pfull = FLD(Gear).Pfull(nUin)
			Else
				Pfull = FLD(Gear).Pfull(nUin, LastPe)
			End If
			PinMax = 0.999*(Pfull - Paux - PaMot)
			MinMax = nPeToM(nUin, PinMax)

			'Min
			Min = Mout/mu

			'Check if Min is too high
			If Min > MinMax Then Continue Do

			'Calculated input and output torque for given mu
			MinCalc = fTCtorque(nu, nUin)
			MoutCalc = MinCalc*mu

			'Add to lists
			nuList.Add(nu)
			McalcRatio.Add(MoutCalc/Mout)
			iDim += 1

			'Calc smallest error for each mu value
			If FirstDone Then
				If Math.Abs(1 - McalcRatio(i)) < ErrMin Then
					ErrMin = Math.Abs(1 - McalcRatio(i))
					iMin = i
				End If
				If McalcRatio(i) > McalcRatMax Then McalcRatMax = McalcRatio(i)
			Else
				FirstDone = True
				ErrMin = Math.Abs(1 - McalcRatio(i))
				iMin = i
				McalcRatMax = McalcRatio(i)
			End If

			'Abort if error is small enough
			If ErrMin <= DEV.TCiterPrec Then Exit Do

		Loop

		If iDim = - 1 Then
			TCReduce = True
			Return True
		End If


		If ErrMin > DEV.TCiterPrec Then

			If McalcRatMax >= 1 Then

				'Creeping...
				FirstDone = False
				For i = 0 To iDim
					If McalcRatio(i) > 1 Then
						If FirstDone Then
							If Math.Abs(1 - McalcRatio(i)) < ErrMin Then
								ErrMin = Math.Abs(1 - McalcRatio(i))
								iMin = i
							End If
						Else
							FirstDone = True
							ErrMin = Math.Abs(1 - McalcRatio(i))
							iMin = i
						End If
					End If
				Next

				Brake = True

			Else

				If MoutCalc > 0 Then
					TCReduce = True
					Return True
				End If


			End If

		End If

		nu = nuList(iMin)
		mu = fTCmu(nu)
		TCnUin = nUout/nu
		TCMout = fTCtorque(nu, TCnUin)*mu
		TCMin = TCMout/mu
		TCnUout = nUout

		TC_mu = mu
		TC_nu = nu

		If Brake Then TC_PeBrake = nMtoPe(TCnUout, Mout - TCMout)


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
		Return (nu - TCnu(i - 1))*(TCmu(i) - TCmu(i - 1))/(TCnu(i) - TCnu(i - 1)) + TCmu(i - 1)
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
		M0 = (nu - TCnu(i - 1))*(TCtorque(i) - TCtorque(i - 1))/(TCnu(i) - TCnu(i - 1)) + TCtorque(i - 1)

		Return M0*(nUin/TCrefrpm)^2
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

				'Skip header
				file.ReadLine()

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

						GetrEff(i) = - 1

					Else

						EffSum = 0
						Anz = 0

						dnU = (2/3)*(ENG.Nrated - ENG.Nidle)/10
						nU = ENG.Nidle + dnU

						Do While nU <= ENG.Nrated

							dM = nPeToM(nU, (2/3)*FLD(i).Pfull(nU)/10)
							M_in = nPeToM(nU, (1/3)*FLD(i).Pfull(nU))

							Do While M_in <= nPeToM(nU, FLD(i).Pfull(nU))

								P_In = nMtoPe(nU, M_in)

								P_Loss = IntpolPeLossFwd(i, nU, P_In, False)

								EffSum += (P_In - P_Loss)/P_In
								Anz += 1


								plossG = P_Loss
								MinG = M_in


								'Axle
								P_In -= P_Loss
								P_Loss = IntpolPeLossFwd(0, nU/GBX.Igetr(i), P_In, False)
								EffDiffSum += (P_In - P_Loss)/P_In
								AnzDiff += 1

								If MODdata.ModErrors.TrLossMapExtr <> "" Then
									WorkerMsg(tMsgID.Err, "Transmission loss map does not cover full engine operating range!", MsgSrc)
									WorkerMsg(tMsgID.Err, MODdata.ModErrors.TrLossMapExtr, MsgSrc)
									WorkerMsg(tMsgID.Err, "nU_In(GB)=" & nU & " [1/min]", MsgSrc)
									WorkerMsg(tMsgID.Err, "M_In(GB)=" & MinG & " [Nm]", MsgSrc)
									WorkerMsg(tMsgID.Err, "P_Loss(GB)=" & plossG & " [kW]", MsgSrc)
									WorkerMsg(tMsgID.Err, "nU_In(axle)=" & CStr(nU/Igetr(i)) & " [1/min]", MsgSrc)
									WorkerMsg(tMsgID.Err, "M_In(axle)=" & CStr(nPeToM(nU/Igetr(i), P_In)) & " [Nm]", MsgSrc)
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

						GetrEff(i) = EffSum/Anz

					End If

				End If


			End If

		Next

		If Not GetrEffDef(0) Then
			GetrEff(0) = EffDiffSum/AnzDiff
		End If


		Return True
	End Function

	Public Function IntpolPeLoss(ByVal Gear As Integer, ByVal nU As Double, ByVal PeOut As Double, ByVal Approx As Boolean) _
		As Double

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
				PeIn = PeOut/GetrEff(Gear)
			Else
				PeIn = PeOut*GetrEff(Gear)
			End If
			Ploss = PeIn - PeOut

		Else

			GBmap = MyGBmaps(Gear)


			'Interpolate with Original Values
			PeIn = nMtoPe(nU, GBmap.Intpol(nU, nPeToM(nU, PeOut)))

			If GBmap.ExtrapolError Then

				'If error: try extrapolation

				'Search for the nearest Map point
				AbMin = ((GBmap.ptList(0).X - nU)^2 + (GBmap.ptList(0).Y - nPeToM(nU, PeOut))^2)^0.5
				iMin = 0
				For i = 1 To GBmap.ptDim
					Ab = ((GBmap.ptList(i).X - nU)^2 + (GBmap.ptList(i).Y - nPeToM(nU, PeOut))^2)^0.5
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

						WG = PeOutX/PeIn
						PeIn = PeOut/WG
						Ploss = PeIn - PeOut

					Else

						'Drag => Drive: ERROR!
						WorkerMsg(tMsgID.Err,
								"Transmission Loss Map invalid! Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" &
								PeIn.ToString("0.0") & " [kW], PeOut=" & PeOutX.ToString("0.0") & " [kW]", MsgSrc)
						WorkerAbort()
						Return 0

					End If

				ElseIf PeOutX < 0 Then

					If PeIn > 0 Then

						WG = (PeIn - (PeIn - PeOutX))/PeIn
						PeIn = PeOut/WG
						Ploss = PeIn - PeOut

					ElseIf PeIn < 0 Then

						WG = PeIn/PeOutX
						PeIn = PeOut*WG
						Ploss = PeIn - PeOut

					Else

						Ploss = Math.Abs(PeOut)

					End If


				Else

					If PeIn > 0 Then

						Ploss = PeIn

					ElseIf PeIn < 0 Then

						'Drag => Zero: ERROR!
						WorkerMsg(tMsgID.Err,
								"Transmission Loss Map invalid! Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" &
								PeIn.ToString("0.0") & " [kW], PeOut=" & PeOutX.ToString("0.0") & " [kW]", MsgSrc)
						WorkerAbort()
						Return 0
					Else

						Ploss = Math.Abs(PeOut)

					End If

				End If

				MODdata.ModErrors.TrLossMapExtr = "Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], MeOut=" &
												nPeToM(nU, PeOut).ToString("0.00") & " [Nm]"

			Else

				Ploss = PeIn - PeOut


			End If

		End If

		Return Math.Max(Ploss, 0)
	End Function

	Public Function IntpolPeLossFwd(ByVal Gear As Integer, ByVal nU As Double, ByVal PeIn As Double,
									ByVal Approx As Boolean) As Double

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
				PeOut = PeIn*GetrEff(Gear)
			Else
				PeOut = PeIn/GetrEff(Gear)
			End If

		Else

			GBmap = MyGBmaps(Gear)


			'Interpolate with original values
			PeOut = nMtoPe(nU, GBmap.IntpolXZ(nU, nPeToM(nU, PeIn)))

			If GBmap.ExtrapolError Then

				'If error: try extrapolation

				'Search for the nearest Map-point
				AbMin = ((GBmap.ptList(0).X - nU)^2 + (GBmap.ptList(0).Z - nPeToM(nU, PeIn))^2)^0.5
				iMin = 0
				For i = 1 To GBmap.ptDim
					Ab = ((GBmap.ptList(i).X - nU)^2 + (GBmap.ptList(i).Z - nPeToM(nU, PeIn))^2)^0.5
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
						WG = PeOut/PeInX

					Else

						'Drag => Drivetrain: ERROR!
						WorkerMsg(tMsgID.Err,
								"Transmission Loss Map invalid! Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], PeIn=" &
								PeInX.ToString("0.00") & " [kW], PeOut=" & PeOut.ToString("0.00") & " [kW] (fwd)", MsgSrc)
						WorkerAbort()
						Return 0

					End If

				Else
					If PeInX > 0 Then

						WorkerMsg(tMsgID.Warn,
								"Change of sign in Transmission Loss Map! Set efficiency to 10%. Gear= " & GrTxt & ", nU= " &
								nU.ToString("0.00") & " [1/min], PeIn=" & PeInX.ToString("0.00") & " [kW], PeOut=" & PeOut.ToString("0.00") &
								" [kW] (fwd)", MsgSrc)
						'WorkerAbort()
						WG = 0.1

					Else

						'Drag => Drag
						WG = PeInX/PeOut


					End If
				End If

				'Calculate efficiency with PeIn for original PeOut
				PeOut = PeIn*WG

				MODdata.ModErrors.TrLossMapExtr = "Gear= " & GrTxt & ", nU= " & nU.ToString("0.00") & " [1/min], MeIn=" &
												nPeToM(nU, PeIn).ToString("0.00") & " [Nm] (fwd)"

			End If

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

		Public gs_TqUp As New List(Of Single)
		Public gs_TqDown As New List(Of Single)
		Public gs_nUup As New List(Of Single)
		Public gs_nUdown As New List(Of Single)
		Private gs_Dup As Integer = - 1
		Private gs_Ddown As Integer = - 1

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

			'Skip Header
			file.ReadLine()

			'Clear lists
			gs_TqUp.Clear()
			gs_TqDown.Clear()
			gs_nUdown.Clear()
			gs_nUup.Clear()
			gs_Dup = - 1

			'Read file
			Try
				Do While Not file.EndOfFile
					line = file.ReadLine
					gs_Dup += 1
					gs_TqUp.Add(CSng(line(0)))
					gs_TqDown.Add(CSng(line(0)))
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


		Public Sub SetGenericShiftPoly(Optional ByRef fld0 As cFLD = Nothing, Optional ByVal nidle As Single = - 1)
			Dim Tmax As Single

			'Clear lists
			gs_TqUp.Clear()
			gs_TqDown.Clear()
			gs_nUdown.Clear()
			gs_nUup.Clear()

			If fld0 Is Nothing Then fld0 = FLD(MyGear)
			If nidle < 0 Then nidle = ENG.Nidle

			Tmax = fld0.Tmax

			gs_nUdown.Add(nidle)
			gs_nUdown.Add(nidle)
			gs_nUdown.Add((fld0.Npref + fld0.Nlo)/2)

			gs_TqDown.Add(0)
			gs_TqDown.Add(Tmax*nidle/(fld0.Npref + fld0.Nlo - nidle))
			gs_TqDown.Add(Tmax)

			gs_nUup.Add(fld0.Npref)
			gs_nUup.Add(fld0.Npref)
			gs_nUup.Add(fld0.N95h)

			gs_TqUp.Add(0)
			gs_TqUp.Add(Tmax*(fld0.Npref - nidle)/(fld0.N95h - nidle))
			gs_TqUp.Add(Tmax)

			gs_Ddown = 2
			gs_Dup = 2
		End Sub

		Public Function fGSnUdown(ByVal Tq As Single) As Single
			Dim i As Int32

			'Extrapolation for x < x(1)
			If gs_TqDown(0) >= Tq Then
				i = 1
				GoTo lbInt
			End If

			i = 0
			Do While gs_TqDown(i) < Tq And i < gs_Ddown
				i += 1
			Loop


			lbInt:
			'Interpolation
			Return (Tq - gs_TqDown(i - 1))*(gs_nUdown(i) - gs_nUdown(i - 1))/(gs_TqDown(i) - gs_TqDown(i - 1)) + gs_nUdown(i - 1)
		End Function

		Public Function fGSnUup(ByVal Tq As Single) As Single
			Dim i As Int32

			'Extrapolation for x < x(1)
			If gs_TqUp(0) >= Tq Then
				i = 1
				GoTo lbInt
			End If

			i = 0
			Do While gs_TqUp(i) < Tq And i < gs_Dup
				i += 1
			Loop


			lbInt:
			'Interpolation
			Return (Tq - gs_TqUp(i - 1))*(gs_nUup(i) - gs_nUup(i - 1))/(gs_TqUp(i) - gs_TqUp(i - 1)) + gs_nUup(i - 1)
		End Function
	End Class
End Class
