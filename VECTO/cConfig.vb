Public Class cConfig

    'Private Const FormatVersion As Short = 1
    'Private FileVersion As Short

    Public GnVorgab As Boolean
    Private sWorkDPath As String
    Private WorkDirHome As Boolean  'Nicht direkt in Configdatei
    Public LastMode As Int16
    Public TEMpath As String
    Public LastTEM As String
    Public TEMexl As Boolean
    Public EAAvInt As Short
    Public ModOut As Boolean
    Public BATCHoutpath As String   'Ausgabepfad für BATCH-Modus:   <WORKDIR>, <GENPATH> oder Pfad
    Public BATCHoutSubD As Boolean
    Public WegKorJa As Boolean
    Public LogSize As Single
    Public FZPsort As Boolean
    Public FZPsortExp As Boolean
    Public AirDensity As Single
    Public FinalEmOnly As Boolean
    Public FCcorrection As Boolean
    Public nnormEngStop As Single
    Public OpenCmd As String
    Public OpenCmdName As String

    Public FuelDens As Single
    Public CO2perFC As Single

    Public FirstRun As Boolean

    Public Sub New()
        SetDefault()
    End Sub

    Public Function ConfigLOAD() As Boolean
        Dim c As New cFile_V3
        Dim line As String()

        SetDefault()

        If Not IO.File.Exists(MyConfPath & "settings.txt") Then
            If Not Cfg.FirstRun Then GUImsg(tMsgID.Err, "Config-file not found! Using default settings.")
            Return False
        End If

        c.OpenRead(MyConfPath & "settings.txt", ";")

        '***
        '*** First line: Version
        'txt = Trim(UCase(c.ReadLine(0)))
        'If Microsoft.VisualBasic.Left(txt, 1) = "V" Then
        '    ' "Remove V'' => It remains the number
        '    txt = txt.Replace("V", "")
        '    If Not IsNumeric(txt) Then
        '        'If invalid version: Abort
        '        GoTo lbEr
        '    Else
        '        'Version settled
        '        FileVersion = CInt(txt)
        '    End If
        'Else
        '    c.Close()
        '    Return ReadOldFormat()
        'End If

        'If FileVersion > FormatVersion Then
        '    GUImsg(tMsgID.Err, "Config-file Version " & FileVersion & " incompatible with application version! Using default settings.")
        '    Return False
        'End If

        sWorkDPath = Trim(c.ReadLine(0))
        If UCase(sWorkDPath) = sKey.HomePath Then
            WorkDirHome = True
            sWorkDPath = MyAppPath
        End If

        LastMode = CShort(c.ReadLine(0))

        nnormEngStop = CSng(c.ReadLine(0))

        TEMpath = c.ReadLine(0)

        LastTEM = c.ReadLine(0)

        TEMexl = CBool(c.ReadLine(0))

        EAAvInt = CShort(c.ReadLine(0))

        ModOut = CBool(c.ReadLine(0))

        WegKorJa = CBool(c.ReadLine(0))

        GnVorgab = CBool(c.ReadLine(0))

        LogSize = CSng(c.ReadLine(0))

        FZPsort = CBool(c.ReadLine(0))

        FZPsortExp = CBool(c.ReadLine(0))

        BATCHoutpath = Trim(c.ReadLine(0))

        BATCHoutSubD = CBool(c.ReadLine(0))

        AirDensity = CSng(c.ReadLine(0))

        FinalEmOnly = CBool(c.ReadLine(0))

        FCcorrection = CBool(c.ReadLine(0))

        line = c.ReadLine
        OpenCmd = line(0)
        If UBound(line) > 0 Then OpenCmdName = line(1)


        FuelDens = CSng(c.ReadLine(0))
        CO2perFC = CSng(c.ReadLine(0))

        If c.EndOfFile Then GoTo lbDone

        FirstRun = CBool(c.ReadLine(0))


lbDone:

        c.Close()

        Return True


lbEr:
        c.Close()
        Return False

    End Function

    Private Function ReadOldFormat() As Boolean
        Dim c As New cFile_V3
        Dim line As String = ""
        Dim x As Short

        'Config.txt
        'Line      Variable        Type         Description
        '(01)       WorkDPath       String      WorkDir
        '(02)       LastMode        Short      Last used mode (equivalent CBoxMODE.SelectedIndex)
        '(03)       IntpV2          Boolean     New interpolator to use
        '(04)       nnormEngStop    Single      Stop engine if under this Revolutions
        '(05)       TEMpath         String      TEM_Data path
        '(06)       LastTEM         String      Last TEM file -not in Options Form!!!!
        '(07)       TEMexl          Boolean     Open TEM in Excel  -not in Options Form!!!
        '(08)       EAAvInt         Short       Analysis intervals in seconds. If 0: Ask for Value
        '(09)       ModOut          Boolean     Dump Modal
        '(10)       WegKorJa        Boolean     Wegkorrektur damit bei Geschw. Reduktion Zyklus nicht kürzer wird |@@| 10)       WegKorJa        Boolean     Path-correction in so speed. Reduction cycle is not shorter
        '(11)       GnVorgab        Boolean     Gear-per- Revolutions
        '(12)       LogSize         Int16       Maximum Log-size [MiB]
        '(13)       FZPsort         Boolean     FZP sortieren (früher Standard da VISSIM die .fzp nach Sekunden sortiert ausgibt) |@@| 13)       FZPsort         Boolean     FZP sort (formerly standard since the VISSIM. Fzp sorted according seconds) outputs
        '(14)       FZPsortExp      Boolean     Export FZP Sorted
        '(15)       BATCHoutpath    Boolean     Output path for BATCH mode:   <WORKDIR>, <GENPATH> or path
        '(16)       BATCHoutSubD    Boolean     Dump-BATCH in Subfolders (per .gen File)
        '(17)       AirDensity      Single      Air-density
        '(18)       FinalEmOnly     Boolean     Dump only Final Emission
        '(19)       FCcorrection    Boolean     FC-Correction in BATCH-mode

        If Not IO.File.Exists(MyConfPath & "settings.txt") Then
            GUImsg(tMsgID.Err, "Config-file not found! Using default settings.")
            Return False
        End If

        c.OpenRead(MyConfPath & "settings.txt", "+hugo+")

        For x = 1 To 19
            If c.EndOfFile Then GoTo lbEnd
            line = c.ReadLine(0)
            Select Case x
                Case 1
                    sWorkDPath = Trim(line)
                    If UCase(sWorkDPath) = sKey.HomePath Then
                        sWorkDPath = MyAppPath
                        WorkDirHome = True
                    End If
                Case 2
                    LastMode = line 'Früher GenLpath = line
                Case 3
                    'Previously: IntpV2 = CBool(line)
                Case 4
                    nnormEngStop = CSng(line)
                Case 5
                    TEMpath = line
                Case 6
                    LastTEM = line
                Case 7
                    TEMexl = CBool(line)
                Case 8
                    EAAvInt = Val(line)
                Case 9
                    ModOut = CBool(line)
                Case 10
                    WegKorJa = CBool(line)
                Case 11
                    GnVorgab = CBool(line)
                Case 12
                    If IsNumeric(line) Then LogSize = line
                Case 13
                    FZPsort = CBool(line)
                Case 14
                    FZPsortExp = CBool(line)
                Case 15
                    BATCHoutpath = Trim(line)
                Case 16
                    BATCHoutSubD = CBool(line)
                Case 17
                    AirDensity = CSng(line)
                Case 18
                    FinalEmOnly = CBool(line)
                Case 19
                    FCcorrection = CBool(line)
            End Select
        Next
        '------------------------------------------------
        GoTo lbDone
lbEnd:
        GUImsg(tMsgID.Warn, "Missing parameters in Configuration File! Using default settings.")
lbDone:
        c.Close()
        c = Nothing

        Return True

    End Function

    Public Sub SetDefault()
        GnVorgab = True
        sWorkDPath = "c:\"
        LastMode = 0
        TEMpath = "<default>"
        LastTEM = "New File.tem"
        TEMexl = False
        EAAvInt = 20
        ModOut = True
        BATCHoutpath = sKey.GenPath
        BATCHoutSubD = False
        WegKorJa = True
        LogSize = 2
        FZPsort = True
        FZPsortExp = False
        AirDensity = 1.2
        FinalEmOnly = True
        FCcorrection = False
        nnormEngStop = -0.05
        OpenCmd = "notepad"
        OpenCmdName = "Notepad"


        FuelDens = 0.835
        CO2perFC = 3.153

        WorkDirHome = False

        FirstRun = True

    End Sub

    Public Sub ConfigSAVE()
        Dim c As New cFile_V3
        c.OpenWrite(MyConfPath & "settings.txt", ";")

        'Version
        'c.WriteLine("V" & FormatVersion)

        c.WriteLine("c Working Directory Path")
        If WorkDirHome And UCase(Trim(sWorkDPath)) = UCase(Trim(MyAppPath)) Then
            c.WriteLine(sKey.HomePath)
        Else
            c.WriteLine(sWorkDPath)
            WorkDirHome = False
        End If
        c.WriteLine("c LastMode 0/1/2")
        c.WriteLine(F_MAINForm.CBoxMODE.SelectedIndex)
        c.WriteLine("c nnorm engine stop [-]")
        c.WriteLine(nnormEngStop)
        c.WriteLine("c TEM_Data Path for *.tem file creation")
        c.WriteLine(TEMpath)
        c.WriteLine("c Last TEM File")
        c.WriteLine(LastTEM)
        c.WriteLine("c Open TEM 1/0")
        c.WriteLine(Math.Abs(CInt(TEMexl)))
        c.WriteLine("c Engine Analysis: Analyse intervals of seconds")
        c.WriteLine(EAAvInt)
        c.WriteLine("c Modal output 1/0")
        c.WriteLine(Math.Abs(CInt(ModOut)))
        c.WriteLine("c Cycle Distance Correction 1/0")
        c.WriteLine(Math.Abs(CInt(WegKorJa)))
        c.WriteLine("c Use gears/rpm's form driving cycle 1/0")
        c.WriteLine(Math.Abs(CInt(GnVorgab)))
        c.WriteLine("c Log file size limit [MB]")
        c.WriteLine(LogSize)
        c.WriteLine("c ADVANCE Sort .fzp file 1/0")
        c.WriteLine(Math.Abs(CInt(FZPsort)))
        c.WriteLine("c ADVANCE Export sorted .fzp file 1/0")
        c.WriteLine(Math.Abs(CInt(FZPsortExp)))
        c.WriteLine("c BATCH Output Path")
        c.WriteLine(BATCHoutpath)
        c.WriteLine("c BATCH Sub Dir Output 1/0")
        c.WriteLine(Math.Abs(CInt(BATCHoutSubD)))
        c.WriteLine("c Air Density [kg/m3]")
        c.WriteLine(CStr(AirDensity))
        c.WriteLine("c Emissions Output: Tailpipe Only 1/0")
        c.WriteLine(Math.Abs(CInt(FinalEmOnly)))
        c.WriteLine("c HDV FC Correction 1/0")
        c.WriteLine(Math.Abs(CInt(FCcorrection)))
        c.WriteLine("c File Open CMD")
        c.WriteLine(OpenCmd, OpenCmdName)

        c.WriteLine("c Fuel Density [kg/l]")
        c.WriteLine(FuelDens.ToString)
        c.WriteLine("c CO2 per FC [kgCO2/kgFC]")
        c.WriteLine(CO2perFC.ToString)

        c.WriteLine("c First Run (Show Quick Start Guide Prompt)")
        c.WriteLine(Math.Abs(CInt(FirstRun)))


        c.Close()
        c = Nothing
    End Sub

    Public Sub SetWorkDir(ByVal Path As String)

        Path = Trim(Path)

        If Path = "" Then Exit Sub

        If Right(Path, 1) <> "\" Then Path &= "\"

        If UCase(Path) <> UCase(sWorkDPath) Then

            If UCase(Path) = sKey.HomePath Then
                WorkDirHome = True
                sWorkDPath = MyAppPath
            Else
                WorkDirHome = False
                sWorkDPath = Path
            End If

        End If

    End Sub

    Public ReadOnly Property WorkDPath As String
        Get
            Return sWorkDPath
        End Get
    End Property

End Class

