Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting.Channels.Ipc

Module M_OptInterface

    Public bOptOn As Boolean = False
    Public OptMsgTxt As String
    Public OptERstat As Boolean = False
    Public bOptKillLauncher As Boolean = True
    Public bOptStartPHEM As Boolean = True

    Private OptZaehl As Int16
    Private OptDir As String
    Private ipcCh As IpcChannel
    Private bRecInit As Boolean = False

    Public SlB_nU_opt As Single
    Public SlB_Pe_opt As Single

    'Corrected Emissions (determined by SOC-iteration-module)
    Public OptEMkor() As Single

    '*** Opt_Interface On/Off
    Public Sub OptOnOff()
        If bOptOn Then
            bOptOn = False
            OptClose()
            OptStatus("OFF")
        Else
            Try
                'Initialization
                OptInit()
            Catch ex As Exception
                GUImsg(tMsgID.Err, ex.Message.ToString)
            End Try
        End If
    End Sub

    '*** Initialization
    Private Sub OptInit()


        'Count to Zero
        OptZaehl = 0

        'Delete Corr. Em.
        ReDim OptEMkor(10)

        OptERstat = False

        'TODO: I/O-Verzeichnis festlegen |@@| TODO: Specify I/O-directory
        OptDir = "TODO"        '  Früher: F_DEV.TbOptInOut.Text
        If Right(OptDir, 1) <> "\" Then OptDir &= "\"

        'Communication - Input
        Try
            ipcCh = New IpcChannel("IPC_Server")
            ChannelServices.RegisterChannel(ipcCh, False)
            RemotingConfiguration.RegisterWellKnownServiceType(GetType(CommunicationService), "ServerReceiveObj", WellKnownObjectMode.Singleton)
        Catch ex As Exception
            Exit Sub
        End Try

        OptMsgTxt = ""
        bOptOn = True

        GUImsg(tMsgID.Normal, "OptInterface aktiviert")
        OptStatus("ON")


        'Dim file As New cFile
        'ReDim xA(Adim)
        'ReDim yA(Adim)
        'ReDim zA(Adim)
        'file.OpenRead(OptDir & "XYZ_Punkte.csv")
        'For x = 1 To Adim
        '    file.Read(xA(x))
        '    file.ReadTab(yA(x))
        '    file.ReadTab(zA(x))
        'Next
        'file.Close()
        'file = Nothing

        'Timer Initialization/Start
        F_MAINForm.ComMsgTimer.Start()
        GUImsg(tMsgID.Normal, "Warte auf START-Signal...")


    End Sub

    '*** Read Parameters
    Public Sub OptInput()
        Dim file As New cFile_V3

        GUImsg(tMsgID.Normal, "Lese Opt_In.txt")

        '**** Reading the Input-file with Opt-parameter
        file.OpenRead(OptDir & "Opt_In.txt")

        '...

        file.Close()
        file = Nothing

    End Sub

    '*** Dump parameters
    Private Sub OptOutput()

        Dim StreamF As System.IO.StreamWriter
        Dim Zielf As Single
    
        '**** Ausgabe der Output-Datei mit Zielfunktion |@@| Dump the output file along with the Objective-function(Zielfunktion)
        GUImsg(tMsgID.Normal, "Schreibe Opt_Out.txt")

        Try
            StreamF = My.Computer.FileSystem.OpenTextFileWriter(OptDir & "Opt_Out.txt", False, System.Text.Encoding.ASCII)
        Catch ex As Exception
            GUImsg(tMsgID.Err, "Kann nicht zugreifen auf " & OptDir & "Opt_Out.txt")
            Exit Sub
        End Try

        'Dump the StatusString
        If OptERstat Then
            StreamF.WriteLine("ER")
            OptERstat = False
            'F_DEV.ButOptAbbr.Enabled = True
        Else
            StreamF.WriteLine("OK")
        End If

        'Berechnung der Zielfunktion |@@| Calculation of the Objective-function(Zielfunktion)
        Zielf = OptEMkor(1) / 40 + (OptEMkor(2) / 0.2) ^ 4

        'Ausgabe der Zielfunktion |@@| Dump the Objective-function(Zielfunktion)
        StreamF.WriteLine(Zielf)

        StreamF.Close()
        StreamF.Dispose()

    End Sub

    '*** Opt Deactivation
    Private Sub OptClose()
        F_MAINForm.ComMsgTimer.Stop()
        ChannelServices.UnregisterChannel(ipcCh)
        OptERstat = False
        'F_DEV.ButOptAbbr.Enabled = True
        GUImsg(tMsgID.Normal, "OptInterface deaktiviert")
    End Sub

    '*** Status-notification (must not be called by BGWorker)
    Private Sub OptStatus(ByVal txt As String)
        'F_DEV.LbOptStatus.Text = txt
    End Sub

    '*** Start PHEM - Called from F_MAINForm.ComMsgTimer when the Start-signal is received
    Public Sub OptSTART()
        GUImsg(tMsgID.Normal, "START-Signal erhalten.")
        'PHEM start
        If bOptStartPHEM Then OptStartRun()
    End Sub
    Public Sub OptStartRun()
        'Stop the timer
        F_MAINForm.ComMsgTimer.Stop()
        'Count + 1
        OptZaehl += 1
        'PHEM start
        GUImsg(tMsgID.Normal, "Starte PHEM. Durchgang " & OptZaehl)
        F_MAINForm.PHEM_Launcher()
    End Sub

    '*** PHEM ready - called by BackgroundWorker1_RunWorkerCompleted when PHEM finished
    Public Sub OptEND()

        'Ausgabe der Zielfunktion |@@| Dump of the Objective-function(Zielfunktion)
        OptOutput()

        'Finish PHEM_Launcher
        If bOptKillLauncher Then KillLauncher()

        'Start the Timer again
        F_MAINForm.ComMsgTimer.Start()

        GUImsg(tMsgID.Normal, "Warte auf START-Signal...")

    End Sub

    '*** Finished PHEM_Launcher
    Public Sub KillLauncher()
        Dim pProcess() As Process = System.Diagnostics.Process.GetProcessesByName("PHEM_Launcher")
        If UBound(pProcess) > -1 Then
            pProcess(0).Kill()
            GUImsg(tMsgID.Normal, "PHEM_Launcher beendet.")
        Else
            GUImsg(tMsgID.Err, "Prozess PHEM_Launcher nicht gefunden!")
        End If
    End Sub

    '*** CommunicationService
    Public Class CommunicationService
        'Inherits MarshalByRefObject
        'Implements SharedInterfaces.ICommunicationService
        'Public Sub MsgReceive(ByVal txt As String) Implements SharedInterfaces.ICommunicationService.SendMsg
        '    OptMsgTxt = txt
        'End Sub
    End Class


    ' ''*** Message Output
    '' ''Public Sub OptSendMsg(ByVal txt As String)
    '' ''    Try
    '' ''        '      Dim ipcChS As New IpcChannel("ClientSender")
    '' ''        '     ChannelServices.RegisterChannel(ipcChS, False)
    '' ''        Dim ClientRemoteObj As SharedInterfaces.ICommunicationService = DirectCast(Activator.GetObject(GetType(SharedInterfaces.ICommunicationService), "ipc://IPC_Client/ClientReceiveObj"), SharedInterfaces.ICommunicationService)
    '' ''        ClientRemoteObj.SendMsg(txt)
    '' ''        '   ChannelServices.UnregisterChannel(ipcChS)
    '' ''    Catch ex As Exception
    '' ''        WorkerMSG(tMsgID.Err, "ClientRemoteObj.SendMsg failed!!")
    '' ''    End Try
    '' ''End Sub





End Module




