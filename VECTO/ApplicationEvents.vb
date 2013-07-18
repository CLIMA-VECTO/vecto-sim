Imports System.Collections.Generic

Namespace My

    ' The following events are available for MyApplication:
    ' 
    ' Startup: Raised when the application starts even before the creation of the Startup-forms.
    ' Shutdown: Raised after closing all the application forms. This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application, and one is already active.
    ' NetworkAvailabilityChanged: Occurs when connecting or disconnecting to the network.
    Partial Friend Class MyApplication

        'Initialization
        Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup

            Dim logfDetail As IO.FileInfo
            Dim BackUpError As Boolean
            Dim s As String
            Dim i As Int16
            Dim file As cFile_V3

            'Paths
            MyAppPath = My.Application.Info.DirectoryPath & "\"
            MyConfPath = MyAppPath & "config\"
            MyDeclPath = MyAppPath & "Declaration\"
            FB_FilHisDir = MyAppPath & "FileHistory\"

            StartLogfile()

            'If folder does not exist: Create!
            If Not IO.Directory.Exists(MyConfPath) Then
                Try
                    IO.Directory.CreateDirectory(MyConfPath)
                Catch ex As Exception
                    MsgBox("Failed to create directory '" & MyConfPath & "'!", MsgBoxStyle.Critical)
                    LOGfile.WriteLine("Failed to create directory '" & MyConfPath & "'!")
                    e.Cancel = True
                End Try
                IO.File.Create(MyConfPath & "joblist.txt")
                IO.File.Create(MyConfPath & "cyclelist.txt")
                'IO.File.Create(MyConfPath & "ADVlist.txt")
            End If
            If Not IO.Directory.Exists(FB_FilHisDir) Then
                Try
                    IO.Directory.CreateDirectory(FB_FilHisDir)

                    'Preconfigure Directories.txt
                    Try
                        s = IO.Directory.GetParent(My.Application.Info.DirectoryPath).ToString & "\"
                    Catch ex As Exception
                        s = MyAppPath
                    End Try
                    Try

                        file = New cFile_V3
                        file.OpenWrite(FB_FilHisDir & "Directories.txt")
                        file.WriteLine(s)
                        For i = 2 To 20
                            file.WriteLine(" ")
                        Next
                        file.Close()
                    Catch ex As Exception

                    End Try

                Catch ex As Exception
                    MsgBox("Failed to create directory '" & FB_FilHisDir & "'!", MsgBoxStyle.Critical)
                    LOGfile.WriteLine("Failed to create directory '" & FB_FilHisDir & "'!")
                    e.Cancel = True
                End Try
            End If

            'Separator!
            SetCulture = False
            If System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator <> "." Then
                SetCulture = True
                Try
                    System.Threading.Thread.CurrentThread.CurrentCulture = New System.Globalization.CultureInfo("en-US")
                    System.Threading.Thread.CurrentThread.CurrentUICulture = New System.Globalization.CultureInfo("en-US")
                    'MSGtoForm(8, "Set CurrentCulture to 'en-US'", True)
                Catch ex As Exception
                    GUImsg(tMsgID.Err, "Failed to set Application Regional Settings to 'en-US'! Check system decimal- and group- separators!")
                End Try
            End If

            'Initialize Classes
            sKey = New csKey
            JobFileList = New List(Of String)
            JobCycleList = New List(Of String)
            DEV = New cDEV
            VEC = New cVECTO

            Cfg = New cConfig   'ACHTUNG: cConfig.New löst cConfig.SetDefault aus welches sKey benötigt dehalb muss sKey schon vorher initialisiert werden!!

            ProgBarCtrl = New cProgBarCtrl

            'Config
            Cfg.ConfigLOAD()


            'Start Log
            If IO.File.Exists(MyAppPath & "LOG.txt") Then

                'File size check
                logfDetail = My.Computer.FileSystem.GetFileInfo(MyAppPath & "LOG.txt")

                'If Log too large: Delete
                If logfDetail.Length / (2 ^ 20) > Cfg.LogSize Then

                    LOGfile.WriteLine("Starting new logfile")
                    LOGfile.Close()

                    BackUpError = False

                    Try
                        If IO.File.Exists(MyAppPath & "LOG_backup.txt") Then IO.File.Delete(MyAppPath & "LOG_backup.txt")
                        IO.File.Move(MyAppPath & "LOG.txt", MyAppPath & "LOG_backup.txt")
                    Catch ex As Exception
                        BackUpError = True
                    End Try

                    StartLogfile()

                    If BackUpError Then
                        MsgBox("Failed to backup logfile! (" & MyAppPath & "LOG_backup.txt)")
                    Else
                        LOGfile.WriteLine("Logfile restarted. Old log saved to LOG_backup.txt")
                    End If

                End If

            End If

            'License initialization
            Lic = New vectolic.cLicense
            Lic.AppVersion = VECTOvers
            Lic.FilePath = MyAppPath & "license.dat"

        End Sub

        Private Sub MyApplication_UnhandledException(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            e.ExitApplication = True
            MsgBox("ERROR!" & ChrW(10) & ChrW(10) & e.Exception.Message.ToString, MsgBoxStyle.Critical, "Unexpected Error")
            LOGfile.WriteLine(">>>Unexpected Error:" & e.Exception.ToString())
        End Sub

    End Class

End Namespace

