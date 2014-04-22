Imports System.Collections.Generic

Public Class cConfig

    Public FilePath As String

    Private Const FormatVersion As Short = 1
    Private FileVersion As Short

    Public GnUfromCycle As Boolean
    Public LastMode As Int16
    Public ModOut As Boolean
    Public BATCHoutpath As String   'Ausgabepfad für BATCH-Modus:   <WORKDIR>, <GENPATH> oder Pfad
    Public BATCHoutSubD As Boolean
    Public DistCorr As Boolean
    Public LogSize As Single
    Public AirDensity As Single
    Public OpenCmd As String
    Public OpenCmdName As String

    Public FuelDens As Single
    Public CO2perFC As Single

    Public FirstRun As Boolean

    Public DeclMode As Boolean

    Public Sub New()
        SetDefault()
    End Sub

    Public Sub DeclInit()
        AirDensity = cDeclaration.AirDensity
        FuelDens = cDeclaration.FuelDens
        CO2perFC = cDeclaration.CO2perFC
        DistCorr = True
        GnUfromCycle = False
    End Sub

    Public Sub SetDefault()
        GnUfromCycle = True
        LastMode = 0
        ModOut = True
        BATCHoutpath = sKey.JobPath
        BATCHoutSubD = False
        DistCorr = True
        LogSize = 2
        AirDensity = 1.2
        OpenCmd = "notepad"
        OpenCmdName = "Notepad"

        FuelDens = 0.835
        CO2perFC = 3.153

        FirstRun = True

        DeclMode = True

    End Sub

    Public Sub ConfigLOAD()

        Dim JSON As New cJSON

        SetDefault()

        If Not IO.File.Exists(FilePath) Then Exit Sub


        If Not JSON.ReadFile(FilePath) Then GUImsg(tMsgID.Err, "Failed to load settings! Using default settings.")

        Try

            FileVersion = JSON.Content("Header")("FileVersion")

            LastMode = JSON.Content("Body")("LastMode")
            ModOut = JSON.Content("Body")("ModOut")
            DistCorr = JSON.Content("Body")("DistCorrection")
            GnUfromCycle = JSON.Content("Body")("UseGnUfromCycle")
            LogSize = JSON.Content("Body")("LogSize")
            BATCHoutpath = JSON.Content("Body")("BATCHoutpath")
            BATCHoutSubD = JSON.Content("Body")("BATCHoutSubD")
            AirDensity = JSON.Content("Body")("AirDensity")
            FuelDens = JSON.Content("Body")("FuelDensity")
            CO2perFC = JSON.Content("Body")("CO2perFC")
            OpenCmd = JSON.Content("Body")("OpenCmd")
            OpenCmdName = JSON.Content("Body")("OpenCmdName")
            FirstRun = JSON.Content("Body")("FirstRun")
            DeclMode = JSON.Content("Body")("DeclMode")



        Catch ex As Exception

            GUImsg(tMsgID.Err, "Error while loading settings!")

        End Try


    End Sub

    Public Sub ConfigSAVE()
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

        dic.Add("LastMode", F_MAINForm.CBoxMODE.SelectedIndex)
        dic.Add("ModOut", ModOut)
        dic.Add("DistCorrection", DistCorr)
        dic.Add("UseGnUfromCycle", GnUfromCycle)
        dic.Add("LogSize", LogSize)
        dic.Add("BATCHoutpath", BATCHoutpath)
        dic.Add("BATCHoutSubD", BATCHoutSubD)
        dic.Add("AirDensity", AirDensity)
        dic.Add("FuelDensity", FuelDens)
        dic.Add("CO2perFC", CO2perFC)
        dic.Add("OpenCmd", OpenCmd)
        dic.Add("OpenCmdName", OpenCmdName)
        dic.Add("FirstRun", FirstRun)
        dic.Add("DeclMode", DeclMode)

        JSON.Content.Add("Body", dic)

        JSON.WriteFile(FilePath)

    End Sub


End Class

