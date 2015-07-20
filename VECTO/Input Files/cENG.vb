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

''' <summary>
''' Engine input file
''' </summary>
''' <remarks></remarks>
Public Class cENG

    ''' <summary>
    ''' Current format version
    ''' </summary>
    ''' <remarks></remarks>
	Private Const FormatVersion As Short = 3

    ''' <summary>
    ''' Format version of input file. Defined in ReadFile.
    ''' </summary>
    ''' <remarks></remarks>
    Private FileVersion As Short

    ''' <summary>
    ''' Engine description (model, type, etc.). Saved in input file.
    ''' </summary>
    ''' <remarks></remarks>
    Public ModelName As String

    ''' <summary>
    ''' Engine displacement [ccm]. Saved in input file.
    ''' </summary>
    ''' <remarks></remarks>
    Public Displ As Single

    ''' <summary>
    ''' Idling speed [1/min]. Saved in input file.
    ''' </summary>
    ''' <remarks></remarks>
    Public Nidle As Single

    ''' <summary>
    ''' Rotational inertia including flywheel [kgm²]. Saved in input file. Overwritten by generic value in Declaration mode.
    ''' </summary>
    ''' <remarks></remarks>
    Public I_mot As Single

    ''' <summary>
    ''' List of full load/motoring curve files (.vfld)
    ''' </summary>
    ''' <remarks></remarks>
	Public fFLD As cSubPath
	Public FLD As cFLD

    ''' <summary>
    ''' Path to fuel consumption map
    ''' </summary>
    ''' <remarks></remarks>
    Private fMAP As cSubPath

	''' <summary>
	''' Directory of engine file. Defined in FilePath property (Set)
	''' </summary>
	''' <remarks></remarks>
    Private MyPath As String

    ''' <summary>
    ''' Full file path. Needs to be defined via FilePath property before calling ReadFile or SaveFile.
    ''' </summary>
    ''' <remarks></remarks>
    Private sFilePath As String

    ''' <summary>
    ''' List of sub input files (e.g. FC map). Can be accessed by FileList property. Generated by CreateFileList.
    ''' </summary>
    ''' <remarks></remarks>
    Private MyFileList As List(Of String)

    ''' <summary>
    ''' WHTC Urban test results. Saved in input file. 
    ''' </summary>
    ''' <remarks></remarks>
    Public WHTCurban As Single

    ''' <summary>
    ''' WHTC Rural test results. Saved in input file. 
    ''' </summary>
    ''' <remarks></remarks>
    Public WHTCrural As Single

    ''' <summary>
    ''' WHTC Motorway test results. Saved in input file. 
    ''' </summary>
    ''' <remarks></remarks>
    Public WHTCmw As Single

    ''' <summary>
    ''' Rated engine speed [1/min]. Engine speed at max. power. Defined in Init.
    ''' </summary>
    ''' <remarks></remarks>
    Public Nrated As Single

    ''' <summary>
    ''' Maximum engine power [kW]. Power at rated engine speed.
    ''' </summary>
    ''' <remarks></remarks>
    Public Pmax As Single

    Public SavedInDeclMode As Boolean


    ''' <summary>
    ''' Generates list of all sub input files (e.g. FC map). Sets MyFileList.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <remarks></remarks>
    Public Function CreateFileList() As Boolean

        MyFileList = New List(Of String)

		MyFileList.Add(PathFLD)

        MyFileList.Add(PathMAP)

        'Not used!!! MyFileList.Add(PathWHTC)

        Return True

    End Function

    ''' <summary>
    ''' New instance. Initialise
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        MyPath = ""
        sFilePath = ""
		fMAP = New cSubPath
		fFLD = New cSubPath
        SetDefault()
    End Sub

    ''' <summary>
    ''' Set default values
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SetDefault()
        ModelName = "Undefined"
        Displ = 0
        Nidle = 0
        I_mot = 0
        Nrated = 0
        Pmax = 0

		fMAP.Clear()
		fFLD.Clear()

        WHTCurban = 0
        WHTCrural = 0
        WHTCmw = 0

        SavedInDeclMode = False

    End Sub

    ''' <summary>
    ''' Save file. <see cref="P:VECTO.cENG.FilePath" /> must be set before calling.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <remarks></remarks>
    Public Function SaveFile() As Boolean
        Dim JSON As New cJSON
        Dim dic As Dictionary(Of String, Object)

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

        dic.Add("Displacement", Displ)
        dic.Add("IdlingSpeed", Nidle)
        dic.Add("Inertia", I_mot)

		dic.Add("FullLoadCurve", fFLD.PathOrDummy)

        dic.Add("FuelMap", fMAP.PathOrDummy)

        dic.Add("WHTC-Urban", WHTCurban)
        dic.Add("WHTC-Rural", WHTCrural)
        dic.Add("WHTC-Motorway", WHTCmw)


        JSON.Content.Add("Body", dic)


        Return JSON.WriteFile(sFilePath)


    End Function

    ''' <summary>
    ''' Read file. <see cref="P:VECTO.cENG.FilePath" /> must be set before calling.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <remarks></remarks>
    Public Function ReadFile(Optional ByVal ShowMsg As Boolean = True) As Boolean
        Dim MsgSrc As String
        Dim JSON As New cJSON

        MsgSrc = "ENG/ReadFile"

        SetDefault()


        If Not JSON.ReadFile(sFilePath) Then Return False

        Try

            FileVersion = JSON.Content("Header")("FileVersion")

            If FileVersion > 1 Then
                SavedInDeclMode = JSON.Content("Body")("SavedInDeclMode")
            Else
                SavedInDeclMode = Cfg.DeclMode
            End If

            ModelName = JSON.Content("Body")("ModelName")

            Displ = JSON.Content("Body")("Displacement")
            Nidle = JSON.Content("Body")("IdlingSpeed")
            I_mot = JSON.Content("Body")("Inertia")

			If FileVersion < 3 Then
				fFLD.Init(MyPath, JSON.Content("Body")("FullLoadCurves")(0)("Path"))
			Else
				fFLD.Init(MyPath, JSON.Content("Body")("FullLoadCurve"))
			End If

            fMAP.Init(MyPath, JSON.Content("Body")("FuelMap"))

			If FileVersion > 2 AndAlso Not JSON.Content("Body")("WHTC-Urban") Is Nothing Then
				WHTCurban = CSng(JSON.Content("Body")("WHTC-Urban"))
				WHTCrural = CSng(JSON.Content("Body")("WHTC-Rural"))
				WHTCmw = CSng(JSON.Content("Body")("WHTC-Motorway"))
			End If

        Catch ex As Exception
            If ShowMsg Then WorkerMsg(tMsgID.Err, "Failed to read VECTO file! " & ex.Message, MsgSrc)
            Return False
        End Try

        Return True

    End Function

    ''' <summary>
    ''' Initialise for calculation. File must be read already.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <remarks></remarks>
    Public Function Init() As Boolean

        Dim MsgSrc As String
        MsgSrc = "ENG/Init"


		'read .vfld file
		FLD = New cFLD
		FLD.FilePath = PathFLD
		If Not FLD.ReadFile(False) Then
			WorkerMsg(tMsgID.Err, "File read error! (" & PathFLD & ")", MsgSrc, PathFLD)
			Return False
		End If

        'Read FC map
        MAP = New cMAP
        MAP.FilePath = PathMAP

        Try
            If Not MAP.ReadFile Then Return False 'Fehlermeldung hier nicht notwendig weil schon von in ReadFile
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "File read error! (" & PathMAP & ")", MsgSrc, PathMAP)
            Return False
        End Try

        'Triangulate FC map.
        If Not MAP.Triangulate() Then
            WorkerMsg(tMsgID.Err, "Failed to triangulate FC map! (" & PathMAP & ")", MsgSrc, PathMAP)
            Return False
        End If


		Nrated = FLD.fnUrated

		Pmax = FLD.Pfull(FLD.fnUrated)

        Return True

    End Function

    ''' <summary>
    ''' Set generic values for Declaration Mode  
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <remarks></remarks>
    Public Function DeclInit() As Boolean

        I_mot = Declaration.EngInertia(Displ)

		FLD.DeclInit()

        Return True
    End Function

    ''' <summary>
    ''' Returns list of sub input files after calling CreateFileList.
    ''' </summary>
    ''' <value></value>
    ''' <returns>list of sub input files</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property FileList As List(Of String)
        Get
            Return MyFileList
        End Get
    End Property

    ''' <summary>
    ''' Get or set Filepath before calling <see cref="M:VECTO.cENG.ReadFile" /> or <see cref="M:VECTO.cENG.SaveFile" />
    ''' </summary>
    ''' <value></value>
    ''' <returns>Full filepath</returns>
    ''' <remarks></remarks>
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


	Public Property PathFLD(Optional ByVal Original As Boolean = False) As String
		Get
			If Original Then
				Return fFLD.OriginalPath
			Else
				Return fFLD.FullPath
			End If
		End Get
		Set(ByVal value As String)
			fFLD.Init(MyPath, value)
		End Set
	End Property

    ''' <summary>
    ''' Get or set file path (cSubPath) to FC map (.vmap)
    ''' </summary>
    ''' <param name="Original">True= (relative) file path as saved in file; False= full file path</param>
    ''' <value></value>
    ''' <returns>Relative or absolute file path to FC map</returns>
    ''' <remarks></remarks>
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
