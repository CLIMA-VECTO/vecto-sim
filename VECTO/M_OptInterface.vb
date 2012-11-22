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

    'Korrigierte Emissionen (wird von SOC-Iterations-Modul bestimmt)
    Public OptEMkor() As Single

    '*** Opt_Interface Ein/Ausschalten
    Public Sub OptOnOff()
        If bOptOn Then
            bOptOn = False
            OptClose()
            OptStatus("OFF")
        Else
            Try
                'Initialisierung
                OptInit()
            Catch ex As Exception
                GUImsg(tMsgID.Err, ex.Message.ToString)
            End Try
        End If
    End Sub

    '*** Initialisierung
    Private Sub OptInit()


        'Zähler null setzen
        OptZaehl = 0

        'Korr. Em. löschen 
        ReDim OptEMkor(10)

        OptERstat = False

        'TODO: I/O-Verzeichnis festlegen
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

        'Timer initialisieren/starten
        F_MAINForm.ComMsgTimer.Start()
        GUImsg(tMsgID.Normal, "Warte auf START-Signal...")


    End Sub

    '*** Parameter einlesen
    Public Sub OptInput()
        Dim file As New cFile_V3

        GUImsg(tMsgID.Normal, "Lese Opt_In.txt")

        '**** Einlesen der Input-Datei mit Opt-Parameter
        file.OpenRead(OptDir & "Opt_In.txt")

        '...

        file.Close()
        file = Nothing

    End Sub

    '*** Parameter ausgeben
    Private Sub OptOutput()

        Dim StreamF As System.IO.StreamWriter
        Dim Zielf As Single
    
        '**** Ausgabe der Output-Datei mit Zielfunktion
        GUImsg(tMsgID.Normal, "Schreibe Opt_Out.txt")

        Try
            StreamF = My.Computer.FileSystem.OpenTextFileWriter(OptDir & "Opt_Out.txt", False, System.Text.Encoding.ASCII)
        Catch ex As Exception
            GUImsg(tMsgID.Err, "Kann nicht zugreifen auf " & OptDir & "Opt_Out.txt")
            Exit Sub
        End Try

        'Ausgabe StatusString
        If OptERstat Then
            StreamF.WriteLine("ER")
            OptERstat = False
            'F_DEV.ButOptAbbr.Enabled = True
        Else
            StreamF.WriteLine("OK")
        End If

        'Berechnung der Zielfunktion
        Zielf = OptEMkor(1) / 40 + (OptEMkor(2) / 0.2) ^ 4

        'Ausgabe der Zielfunktion
        StreamF.WriteLine(Zielf)

        StreamF.Close()
        StreamF.Dispose()

    End Sub

    '*** Opt Deaktivieren
    Private Sub OptClose()
        F_MAINForm.ComMsgTimer.Stop()
        ChannelServices.UnregisterChannel(ipcCh)
        OptERstat = False
        'F_DEV.ButOptAbbr.Enabled = True
        GUImsg(tMsgID.Normal, "OptInterface deaktiviert")
    End Sub

    '*** Status-Meldung (darf nicht von BGWorker aufgerufen werden)
    Private Sub OptStatus(ByVal txt As String)
        'F_DEV.LbOptStatus.Text = txt
    End Sub

    '*** Starte PHEM - wird von F_MAINForm.ComMsgTimer aufgerufen wenn Start-Signal erhalten
    Public Sub OptSTART()
        GUImsg(tMsgID.Normal, "START-Signal erhalten.")
        'PHEM starten
        If bOptStartPHEM Then OptStartRun()
    End Sub
    Public Sub OptStartRun()
        'Timer anhalten
        F_MAINForm.ComMsgTimer.Stop()
        'Zähler + 1
        OptZaehl += 1
        'PHEM starten
        GUImsg(tMsgID.Normal, "Starte PHEM. Durchgang " & OptZaehl)
        F_MAINForm.PHEM_Launcher()
    End Sub

    '*** PHEM fertig - wird von BackgroundWorker1_RunWorkerCompleted aufgerufen wenn PHEM beendet
    Public Sub OptEND()

        'Ausgabe der Zielfunktion
        OptOutput()

        'PHEM_Launcher beenden
        If bOptKillLauncher Then KillLauncher()

        'Timer wieder starten
        F_MAINForm.ComMsgTimer.Start()

        GUImsg(tMsgID.Normal, "Warte auf START-Signal...")

    End Sub

    '*** Beendet PHEM_Launcher
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




