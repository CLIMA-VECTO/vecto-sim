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

Public Class cDEV
	Public Enabled As Boolean

	Private MyOptions As Dictionary(Of String, cDEVoption)
	Private iOptionsDim As Integer

	Public AdvFormat As Boolean
	Public TCiterPrec As Single
	Public TClimitOn As Boolean
	Public TClimit As Single
	Public TCshiftModeNew As Boolean
	Public TCaccmin As Single


	'**************************************************************************************************************
	'**************************************************************************************************************
	'********************************* Instructions for integrating new DEV-Options *********************************

	'1. Entry in "Sub New()"
	'
	'   I) Define new cDEVoption Object with "Conf0 = New cDEVoption(ConfigType, Description, SaveInConfg, Enabled)"
	'
	'       ConfigType      <tDEVconfType>  ...Type definition: Boolean, Integer, Single, String, menu selection (Integer) or Reference to Function
	'       Description     <String>        ...Description of the parameters
	'       SaveInConfg     <Boolean>       ...Whether you want to save settings for next VECTO-startup
	'       Enabled         <Boolean>       ...Whether settings in the DEV-tab can be changed
	'
	'   II) default value definition. Distinguish which ConfigType to use:
	'
	'       a) ConfigType = tBoolean:
	'
	'           Conf0.BoolVal = ...             <Boolean> 
	'
	'       b) ConfigType = tSingleVal:  
	'
	'           Conf0.SingleVal = ...           <Single>
	'
	'       c) ConfigType = tStringVal:    
	'
	'           Conf0.StringVal = ...           <String>
	'
	'       d) ConfigType = tIntVal:    
	'
	'           Conf0.IntVal = ...              <Integer>
	'
	'       e) ConfigType = tAction:
	'
	'           Conf0.ActionDelegate = New cDEVoption.dActionDelegate(AddressOf NameDerFunktion)
	'
	'           Where NameDerFunktion is a function to call returning a <String>: "Public Function NameDerFunktion() As String"
	'
	'       f) ConfigType = tContMenIndex:
	'
	'           Definition of Available selection options as <String>:
	'
	'               Conf0.AddMode ("select 1")
	'               Conf0.AddMode ("Option 2")
	'               and so forth.
	'
	'           Default value definition: First choice = 0
	'
	'               Conf0.ModeIndex = ...       <Integer>


	'**************************************************************************************************************
	'**************************************************************************************************************


	Public Sub New()

		Enabled = False
		MyOptions = New Dictionary(Of String, cDEVoption)

		'*****************************************************************************************
		'*****************************************************************************************
		'**************************** START: Parameters Configuration '****************************

		Dim Conf0 As cDEVoption

		'Conf0 = New cDEVoption(tDEVconfType.tBoolean, "Kennfelderstellung mit Median")
		'Conf0.BoolVal = False
		'MyOptions.Add("KF-Median", Conf0)

		'Conf0 = New cDEVoption(tDEVconfType.tAction, "Action Test")
		'Conf0.ActionDelegate = New cDEVoption.dActionDelegate(AddressOf Me.TestFunction)
		'MyOptions.Add("Action_Test", Conf0)

		'Conf0 = New cDEVoption(tDEVconfType.tIntVal, "Integer Test", True, False)
		'Conf0.IntVal = 666
		'MyOptions.Add("Integer_Test", Conf0)

		'Conf0 = New cDEVoption(tDEVconfType.tSingleVal, "Single Test")
		'Conf0.SingleVal = 1.2345
		'MyOptions.Add("Single_Test", Conf0)

		'Conf0 = New cDEVoption(tDEVconfType.tStringVal, "String Test", False)
		'Conf0.StringVal = "Hallo DU!"
		'MyOptions.Add("String_Test", Conf0)

		'Conf0 = New cDEVoption(tDEVconfType.tSelection, "Menu Test", False, False)
		'Conf0.AddMode("Mode 0")
		'Conf0.AddMode("Hugo")
		'Conf0.AddMode("Charlie")
		'Conf0.AddMode("Mode 3")
		'Conf0.ModeIndex = 3
		'MyOptions.Add("Menu_Test", Conf0)


		Conf0 = New cDEVoption(tDEVconfType.tSingleVal, "TC iteration: target precision for torque ratio")
		Conf0.SingleVal = 0.001
		MyOptions.Add("TCiterPrec", Conf0)

		Conf0 = New cDEVoption(tDEVconfType.tBoolean, "Advanced output files format")
		Conf0.BoolVal = False
		MyOptions.Add("AdvFormat", Conf0)

		Conf0 = New cDEVoption(tDEVconfType.tBoolean,
								"Limit engine speed in torque converter operation if Pe(acc_min) < Pe_max")
		Conf0.BoolVal = True
		MyOptions.Add("TClimitOn", Conf0)

		Conf0 = New cDEVoption(tDEVconfType.tSingleVal, "Max. engine speed for torque converter operation [1/min]")
		Conf0.SingleVal = 1600
		MyOptions.Add("TClimit", Conf0)

		Conf0 = New cDEVoption(tDEVconfType.tSelection,
								"Acceleration for AT Up-Shift power condition")
		Conf0.AddMode("acc_target")	'0
		Conf0.AddMode("acc_min")		'1
		Conf0.ModeIndex = 1
		MyOptions.Add("TCshiftMode", Conf0)


		Conf0 = New cDEVoption(tDEVconfType.tSingleVal, "Minimum acceleration (acc_min) for TC rpm limit and AT-Shift [m/s²]")
		Conf0.SingleVal = 0.025
		MyOptions.Add("TCaccMin", Conf0)


		'**************************** END: Parameters Configuration '*****************************
		'*****************************************************************************************
		'*****************************************************************************************

		iOptionsDim = MyOptions.Count - 1
	End Sub

	'Initialize the actual Config-Parameters from MyConfigs list
	Public Sub SetOptions()
		TCiterPrec = MyOptions("TCiterPrec").SingleVal
		AdvFormat = MyOptions("AdvFormat").BoolVal
		TClimitOn = MyOptions("TClimitOn").BoolVal
		TClimit = MyOptions("TClimit").SingleVal
		TCshiftModeNew = (MyOptions("TCshiftMode").ModeIndex = 1)
		TCaccmin = MyOptions("TCaccMin").SingleVal
	End Sub

	'Demo for Delegate Function
	Public Function TestFunction() As String
		Return "OK...?"
	End Function

	Public sub UpdateDevConfigs
		Dim lv0 As ListViewItem

		EnabledOptCheck()

		for each lv0 In F_MAINForm.LvDEVoptions.Items
			lv0.SubItems(3).Text = DEV.Options(lv0.Tag).ValText
			If DEV.Options(lv0.Tag).Enabled Then
				lv0.ForeColor = Color.Black
			Else
				lv0.ForeColor = Color.DarkGray
			End If
		Next
	End Sub

	Private sub EnabledOptCheck
		MyOptions("TClimit").EnabledOpt = MyOptions("TClimitOn").BoolVal
		MyOptions("TCaccMin").EnabledOpt = MyOptions("TClimitOn").BoolVal Or MyOptions("TCshiftMode").ModeIndex = 1
	End Sub


	Public Function DEVinfo() As String
		Dim s As New System.Text.StringBuilder
		Dim Conf0 As KeyValuePair(Of String, cDEVoption)

		For Each Conf0 In MyOptions

			If Conf0.Value.ConfigType <> tDEVconfType.tAction Then

				s.Append(Conf0.Key & " <" & Conf0.Value.TypeString & "> (" & Conf0.Value.Description & ")")

				If Conf0.Value.ConfigType = tDEVconfType.tSelection Then
					s.AppendLine("= " & Conf0.Value.ValToString & " (" & Conf0.Value.Mode & ")")
				Else
					s.AppendLine("= " & Conf0.Value.ValToString)
				End If

			End If

		Next

		Return s.ToString
	End Function


	Public Function LoadFromFile() As Boolean
		Dim file As cFile_V3
		Dim ConfigFromFile As Dictionary(Of String, String)
		Dim Key As String
		Dim Val As String
		Dim KV As KeyValuePair(Of String, String)

		file = New cFile_V3

		If Not file.OpenRead(MyConfPath & "DEVconfig.txt") Then Return False

		ConfigFromFile = New Dictionary(Of String, String)

		Do While Not file.EndOfFile

			Key = file.ReadLine(0)
			Val = file.ReadLine(0)

			If ConfigFromFile.ContainsKey(Key) Then
				GUImsg(tMsgID.Err, "Multiple definitions of DEV Config '" & Key & "' !")
				Return False
			End If

			ConfigFromFile.Add(Key, Val)

		Loop

		file.Close()

		For Each KV In ConfigFromFile
			If Not MyOptions.ContainsKey(KV.Key) Then
				GUImsg(tMsgID.Warn, "DEV Config '" & KV.Key & "' not defined!")
			Else
				If MyOptions(KV.Key).SaveInFile Then MyOptions(KV.Key).StringToVal(KV.Value)
			End If
		Next

		UpdateDevConfigs

		Return True
	End Function

	Public Function SaveToFile() As Boolean
		Dim file As cFile_V3
		Dim Conf0 As KeyValuePair(Of String, cDEVoption)

		file = New cFile_V3

		If Not file.OpenWrite(MyConfPath & "DEVconfig.txt") Then Return False

		For Each Conf0 In MyOptions

			If Conf0.Value.SaveInFile And Conf0.Value.ConfigType <> tDEVconfType.tAction Then

				If Conf0.Value.ConfigType = tDEVconfType.tSelection Then
					file.WriteLine("# " & Conf0.Value.Description & " <" & Conf0.Value.TypeString & "> (" & Conf0.Value.Mode & ")")
				Else
					file.WriteLine("# " & Conf0.Value.Description & " <" & Conf0.Value.TypeString & ">")
				End If

				file.WriteLine(Conf0.Key)
				file.WriteLine(fStringOrDummySet(Conf0.Value.ValToString))

			End If

		Next

		file.Close()

		Return True
	End Function


	Public ReadOnly Property Options As Dictionary(Of String, cDEVoption)
		Get
			Return MyOptions
		End Get
	End Property

	Public ReadOnly Property OptionsDim As Integer
		Get
			Return iOptionsDim
		End Get
	End Property

	Protected Overrides Sub Finalize()
		MyBase.Finalize()
	End Sub
End Class


Public Class cDEVoption
	Private MyConfType As tDEVconfType
	Private sTypeString As String
	Private MyDescr As String
	Private sModes As List(Of String)
	Private iModesDim As Integer
	Private bEnabled As Boolean
	Private bSaveInFile As Boolean

	Private sValText As String
	Private iIntVal As Integer
	Private sSingleVal As Single
	Private bBoolVal As Boolean
	Private sStringVal As String
	Private iModeIndex As Integer

	Private sValTextDef As String
	Private sValTextDefdef As Boolean

	Public EnabledOpt As Boolean

	Public Delegate Function dActionDelegate() As String

	Public ActionDelegate As dActionDelegate

	Public Sub New(ByVal ConfigType As tDEVconfType, ByVal Description As String, Optional SaveInConfg As Boolean = True,
					Optional ByVal Enabled As Boolean = True)
		MyConfType = ConfigType
		MyDescr = Description
		bEnabled = Enabled
		bSaveInFile = SaveInConfg
		EnabledOpt = True

		Select Case MyConfType
			Case tDEVconfType.tAction
				sTypeString = "Subroutine"
			Case tDEVconfType.tBoolean
				sTypeString = "Boolean"
			Case tDEVconfType.tSelection
				sTypeString = "Selection (Int)"
			Case tDEVconfType.tIntVal
				sTypeString = "Integer"
			Case tDEVconfType.tSingleVal
				sTypeString = "Single"
			Case tDEVconfType.tStringVal
				sTypeString = "String"
		End Select

		sModes = New List(Of String)
		iModesDim = - 1

		iIntVal = 0
		sSingleVal = 0.0F
		bBoolVal = False
		sStringVal = ""
		iModeIndex = - 1

		If MyConfType = tDEVconfType.tAction Then
			sValText = ""
		Else
			sValText = "<undefined>"
		End If

		sValTextDef = ""
		sValTextDefdef = False
	End Sub

	Private Sub DefValTextDef()
		sValTextDefdef = True
		sValTextDef = sValText
	End Sub

	Public Sub DoAction()
		sValText = ActionDelegate.Invoke()
	End Sub

	Public Sub AddMode(ByVal Txt As String)
		sModes.Add(Txt)
		iModesDim += 1
	End Sub

	Public Function StringToVal(ByVal StrExpr As String) As Boolean

		Try
			Select Case MyConfType
				Case tDEVconfType.tAction
					'??? Darf nicht sein |@@| May not be
					Return False

				Case tDEVconfType.tBoolean
					BoolVal = CBool(StrExpr)

				Case tDEVconfType.tSelection
					If CInt(StrExpr) > iModesDim Then
						Return False
					Else
						ModeIndex = CInt(StrExpr)
					End If
				Case tDEVconfType.tIntVal
					IntVal = CInt(StrExpr)

				Case tDEVconfType.tSingleVal
					SingleVal = CSng(StrExpr)

				Case tDEVconfType.tStringVal
					StringVal = fStringOrDummyGet(StrExpr)

			End Select

			Return True

		Catch ex As Exception

			Return False

		End Try
	End Function

	Public Function ValToString() As String

		Select Case MyConfType
			Case tDEVconfType.tBoolean
				Return CStr(Math.Abs(CDec(bBoolVal)))

			Case tDEVconfType.tSelection
				Return iModeIndex.ToString

			Case tDEVconfType.tIntVal
				Return IntVal.ToString

			Case tDEVconfType.tSingleVal
				Return SingleVal.ToString

			Case Else ' tDEVconfType.tStringVal (tDEVconfType.tAction ...darf nicht sein)
				Return StringVal

		End Select
	End Function

	Public ReadOnly Property ValText As String
		Get
			Return sValText
		End Get
	End Property

	Public ReadOnly Property ValTextDef As String
		Get
			Return sValTextDef
		End Get
	End Property

	Public Property IntVal As Integer
		Get
			Return iIntVal
		End Get
		Set(value As Integer)
			iIntVal = value
			sValText = iIntVal.ToString
			If Not sValTextDefdef Then DefValTextDef()
		End Set
	End Property

	Public Property SingleVal As Single
		Get
			Return sSingleVal
		End Get
		Set(value As Single)
			sSingleVal = value
			sValText = sSingleVal.ToString
			If Not sValTextDefdef Then DefValTextDef()
		End Set
	End Property

	Public Property BoolVal As Boolean
		Get
			Return bBoolVal
		End Get
		Set(value As Boolean)
			bBoolVal = value
			If bBoolVal Then
				sValText = "True"
			Else
				sValText = "False"
			End If
			If Not sValTextDefdef Then DefValTextDef()
		End Set
	End Property

	Public Property StringVal As String
		Get
			Return sStringVal
		End Get
		Set(value As String)
			sStringVal = value
			sValText = ChrW(34) & sStringVal & ChrW(34)
			If Not sValTextDefdef Then DefValTextDef()
		End Set
	End Property

	Public Property ModeIndex As Integer
		Get
			Return iModeIndex
		End Get
		Set(value As Integer)
			iModeIndex = value
			sValText = "(" & iModeIndex & ") " & sModes(iModeIndex)
			If Not sValTextDefdef Then DefValTextDef()
		End Set
	End Property

	Public ReadOnly Property Modes As List(Of String)
		Get
			Return sModes
		End Get
	End Property

	Public ReadOnly Property Mode As String
		Get
			If iModeIndex = - 1 Then
				Return "<undefined>"
			Else
				Return sModes(iModeIndex)
			End If
		End Get
	End Property

	Public ReadOnly Property ModesDim As Integer
		Get
			Return iModesDim
		End Get
	End Property

	Public ReadOnly Property Description As String
		Get
			Return MyDescr
		End Get
	End Property

	Public ReadOnly Property ConfigType As tDEVconfType
		Get
			Return MyConfType
		End Get
	End Property

	Public ReadOnly Property TypeString As String
		Get
			Return sTypeString
		End Get
	End Property

	Public ReadOnly Property Enabled As Boolean
		Get
			Return (bEnabled And EnabledOpt)
		End Get
	End Property

	Public ReadOnly Property SaveInFile As Boolean
		Get
			Return bSaveInFile
		End Get
	End Property
End Class

Public Enum tDEVconfType
	tBoolean
	tSingleVal
	tStringVal
	tIntVal
	tAction
	tSelection
End Enum


