Module GUI_Subs


#Region "GUI Steuerung durch BG-Worker"

    'Status Message => Msg-Listview
    Public Sub WorkerMsg(ByVal ID As tMsgID, ByVal Msg As String, ByVal Source As String, Optional ByVal Link As String = "")
        Dim WorkProg As New cWorkProg(tWorkMsgType.StatusListBox)
        WorkProg.ID = ID
        Select Case ID
            Case tMsgID.Err
                MSGerror += 1
            Case tMsgID.Warn
                MSGwarn += 1        
        End Select
        WorkProg.Msg = Msg
        WorkProg.Source = Source
        WorkProg.Link = Link
        Try
            PHEMworker.ReportProgress(0, WorkProg)
        Catch ex As Exception
            GUImsg(ID, Msg)
        End Try
    End Sub

    'Status => Statusbar
    Public Sub WorkerStatus(ByVal Msg As String)
        Dim WorkProg As New cWorkProg(tWorkMsgType.StatusBar)
        WorkProg.Msg = Msg
        PHEMworker.ReportProgress(0, WorkProg)
    End Sub

    'Job-Status => Jobliste Status-Spalte
    Public Sub WorkerJobStatus(ByVal JobIndex As Int16, ByVal Msg As String, ByVal Status As tJobStatus)
        Dim WorkProg As cWorkProg
        WorkProg = New cWorkProg(tWorkMsgType.JobStatus)
        WorkProg.FileIndex = JobIndex
        WorkProg.Msg = Msg
        WorkProg.Status = Status
        PHEMworker.ReportProgress(0, WorkProg)
    End Sub

    'Zyklus-Status => Zyklusliste Status-Spalte
    Public Sub WorkerCycleStatus(ByVal CycleIndex As Int16, ByVal Msg As String)
        Dim WorkProg As cWorkProg
        WorkProg = New cWorkProg(tWorkMsgType.CycleStatus)
        WorkProg.FileIndex = CycleIndex
        WorkProg.Msg = Msg
        PHEMworker.ReportProgress(0, WorkProg)
    End Sub

    'Worker Progress => Progbar (ProgBarSec Update wenn ProgSec > -1; ProgBarSec-Reset bei ProgSec = 0)
    Public Sub WorkerProg(ByVal Prog As Int16, Optional ByVal ProgSec As Integer = -1)
        Dim WorkProg As New cWorkProg(tWorkMsgType.ProgBars)
        WorkProg.ProgSec = ProgSec
        PHEMworker.ReportProgress(Prog, WorkProg)
    End Sub

    'Progbar auf Continuous setzen
    Public Sub WorkerProgInit()
        Dim WorkProg As New cWorkProg(tWorkMsgType.InitProgBar)
        PHEMworker.ReportProgress(0, WorkProg)
    End Sub

    'Abbruch
    Public Sub WorkerAbort()
        Dim WorkProg As New cWorkProg(tWorkMsgType.Abort)
        PHEMworker.ReportProgress(0, WorkProg)
    End Sub



#End Region

#Region "GUI Steuerung durch GUI direkt - NICHT über BG-Worker"

    'Status Message direkt an GUI - darf nicht durch den Backgroundworker geschehen!
    Public Sub GUImsg(ByVal ID As tMsgID, ByVal Msg As String)
        F_MAINForm.MSGtoForm(ID, Msg, "", "")
    End Sub

    'Statusbar  - Aufruf durch WorkerMSG oder direkt über Form, NIEMALS über Worker
    Public Sub Status(ByVal txt As String)
        F_MAINForm.ToolStripLbStatus.Text = txt
    End Sub

    'Status Form zurück setzen - Aufruf NUR durch Events, NIEMALS über Worker
    Public Sub ClearMSG()
        F_MAINForm.LvMsg.Items.Clear()
    End Sub

#End Region

    'Klasse zum Übergeben von Nachrichten vom Backgroundworker
    Public Class cWorkProg
        Private MyTarget As tWorkMsgType
        Private MyID As tMsgID
        Private MyMsg As String
        Private MyFileIndex As Int16
        Private MySource As String
        Private MyStatus As tJobStatus
        Public ProgSec As Integer
        Public Link As String

        Public Sub New(ByVal MsgTarget As tWorkMsgType)
            MyTarget = MsgTarget
            MySource = ""
            MyStatus = tJobStatus.Undef
            ProgSec = -1
            Link = ""
        End Sub

        Public Property Status As tJobStatus
            Get
                Return MyStatus
            End Get
            Set(value As tJobStatus)
                MyStatus = value
            End Set
        End Property


        Public Property Source As String
            Get
                Return MySource
            End Get
            Set(value As String)
                MySource = value
            End Set
        End Property

        Public Property ID() As tMsgID
            Get
                Return MyID
            End Get
            Set(ByVal value As tMsgID)
                MyID = value
            End Set
        End Property

        Public Property Msg() As String
            Get
                Return MyMsg
            End Get
            Set(ByVal value As String)
                MyMsg = value
            End Set
        End Property

        Public ReadOnly Property Target() As tWorkMsgType
            Get
                Return MyTarget
            End Get
        End Property

        Public Property FileIndex() As Int16
            Get
                Return MyFileIndex
            End Get
            Set(ByVal value As Int16)
                MyFileIndex = value
            End Set
        End Property


    End Class

    Public Class cProgBarCtrl
        Public ProgOverallStartInt As Integer = -1
        Public PgroOverallEndInt As Integer = -1
        Public ProgJobInt As Integer = -1
    End Class

    '-----------------------------------------------------------------------

    'Falls String nicht Zahl dann Null
    Public Function fTextboxToNumString(ByVal txt As String) As String
        If Not IsNumeric(txt) Then
            'GUImsg(tMsgID.Err, "'" & txt & "' is no numeric expression!")
            Return "0"
        Else
            Return txt
        End If
    End Function

    Public Function fStringOrDummySet(ByVal txt As String) As String
        If txt = "" Then
            Return sKey.EmptyString
        Else
            Return txt
        End If
    End Function

    Public Function fStringOrDummyGet(ByVal txt As String) As String
        If Trim(UCase(txt)) = sKey.EmptyString Then
            Return ""
        Else
            Return txt
        End If
    End Function

    'Datei in Excel öffnen
    Public Function FileOpen(ByVal file As String) As Boolean
        Dim PSI As New ProcessStartInfo
        PSI.FileName = ChrW(34) & Cfg.OpenCmd & ChrW(34)
        PSI.Arguments = ChrW(34) & file & ChrW(34)
        Try
            Process.Start(PSI)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

#Region "Dateipfad Funktionen"
    'WorkDir oder MainDir einfügen falls kein Pfad angegeben. Spezial-Ordner einfügen
    Public Function fFileRepl(ByVal file As String, Optional ByVal MainDir As String = "") As String

        Dim ReplPath As String

        'Pfad trimmen
        file = Trim(file)

        'Falls leere Datei => Abbruch
        If file = "" Then Return ""

        'sKeys ersetzen
        file = Microsoft.VisualBasic.Strings.Replace(file, sKey.DefVehPath & "\", MyAppPath & "Default Vehicles\", 1, -1, CompareMethod.Text)
        file = Microsoft.VisualBasic.Strings.Replace(file, sKey.WorkDir & "\", Cfg.WorkDPath, 1, -1, CompareMethod.Text)
        file = Microsoft.VisualBasic.Strings.Replace(file, sKey.HomePath & "\", MyAppPath, 1, -1, CompareMethod.Text)

        'Replace - Ordner bestimmen
        If MainDir = "" Then
            ReplPath = Cfg.WorkDPath
        Else
            ReplPath = MainDir
        End If

        ' "..\" => Eine Ordner-Ebene hoch
        Do While ReplPath.Length > 0 AndAlso Left(file, 3) = "..\"
            ReplPath = fPathUp(ReplPath)
            file = file.Substring(3)
        Loop


        'Pfad ergänzen falls nicht vorhanden
        If fPATH(file) = "" Then

            Return ReplPath & file

        Else
            Return file
        End If

    End Function

    'Pfad eine Ebene nach oben      "C:\temp\ordner1\"  >>  "C:\temp\"
    Private Function fPathUp(ByVal Pfad As String) As String
        Dim x As Int16

        Pfad = Pfad.Substring(0, Pfad.Length - 1)

        x = Pfad.LastIndexOf("\")

        If x = -1 Then Return ""

        Return Pfad.Substring(0, x + 1)

    End Function

    'Dateiname ohne Pfad    "C:\temp\TEST.txt"  >>  "TEST.txt" oder "TEST"
    Public Function fFILE(ByVal Pfad As String, ByVal MitEndung As Boolean) As String
        Dim x As Int16
        x = Pfad.LastIndexOf("\") + 1
        Pfad = Microsoft.VisualBasic.Right(Pfad, Microsoft.VisualBasic.Len(Pfad) - x)
        If Not MitEndung Then
            x = Pfad.LastIndexOf(".")
            If x > 0 Then Pfad = Microsoft.VisualBasic.Left(Pfad, x)
        End If
        Return Pfad
    End Function

    'Dateiname ohne Extension   "C:\temp\TEST.txt" >> "C:\temp\TEST"
    Public Function fFileWoExt(ByVal Path As String) As String
        Return fPATH(Path) & fFILE(Path, False)
    End Function

    'Dateiname ohne Pfad falls Pfad = WorkDir oder MainDir
    Public Function fFileWoDir(ByVal file As String, Optional ByVal MainDir As String = "") As String
        Dim path As String


        If MainDir = "" Then
            path = Cfg.WorkDPath
        Else
            path = MainDir
        End If

        If UCase(fPATH(file)) = UCase(path) Then file = fFILE(file, True)

        Return file

    End Function

    'Pfad allein        "C:\temp\TEST.txt"  >>  "C:\temp\"
    '                   "TEST.txt"          >>  ""
    Public Function fPATH(ByVal Pfad As String) As String
        Dim x As Int16
        If Pfad Is Nothing OrElse Pfad.Length < 3 OrElse Pfad.Substring(1, 2) <> ":\" Then Return ""
        x = Pfad.LastIndexOf("\")
        Return Microsoft.VisualBasic.Left(Pfad, x + 1)
    End Function

    'Endung allein      "C:\temp\TEST.txt" >> ".txt"
    Public Function fEXT(ByVal Pfad As String) As String
        Dim x As Int16
        x = Pfad.LastIndexOf(".")
        If x = -1 Then
            Return ""
        Else
            Return Microsoft.VisualBasic.Right(Pfad, Microsoft.VisualBasic.Len(Pfad) - x)
        End If
    End Function


#End Region



End Module
