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

''' <summary>
''' Methods for GUI interaction
''' </summary>
''' <remarks></remarks>
Module GUI_Subs

    Public test As Integer = 0

#Region "GUI control via background worker"

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
            VECTOworker.ReportProgress(0, WorkProg)
        Catch ex As Exception
            GUImsg(ID, Msg)
        End Try
    End Sub

    'Status => Statusbar
    Public Sub WorkerStatus(ByVal Msg As String)
        Dim WorkProg As New cWorkProg(tWorkMsgType.StatusBar)
        WorkProg.Msg = Msg
        VECTOworker.ReportProgress(0, WorkProg)
    End Sub

    'Job status => Job listview
    Public Sub WorkerJobStatus(ByVal JobIndex As Int16, ByVal Msg As String, ByVal Status As tJobStatus)
        Dim WorkProg As cWorkProg
        WorkProg = New cWorkProg(tWorkMsgType.JobStatus)
        WorkProg.FileIndex = JobIndex
        WorkProg.Msg = Msg
        WorkProg.Status = Status
        VECTOworker.ReportProgress(0, WorkProg)
    End Sub

    'Cycle status => Cycle listview
    Public Sub WorkerCycleStatus(ByVal CycleIndex As Int16, ByVal Msg As String)
        Dim WorkProg As cWorkProg
        WorkProg = New cWorkProg(tWorkMsgType.CycleStatus)
        WorkProg.FileIndex = CycleIndex
        WorkProg.Msg = Msg
        VECTOworker.ReportProgress(0, WorkProg)
    End Sub

    'Worker Progress => Progbar
    Public Sub WorkerProgJobEnd(ByVal Prog As Int16)
        Dim WorkProg As New cWorkProg(tWorkMsgType.ProgBars)
        VECTOworker.ReportProgress(Prog, WorkProg)
    End Sub

    'Progbar set to Continuous
    Public Sub WorkerProgInit()
        Dim WorkProg As New cWorkProg(tWorkMsgType.InitProgBar)
        VECTOworker.ReportProgress(0, WorkProg)
    End Sub

    'Abort
    Public Sub WorkerAbort()
        Dim WorkProg As New cWorkProg(tWorkMsgType.Abort)
        VECTOworker.ReportProgress(0, WorkProg)
    End Sub



#End Region

#Region "Direct GUI control - Cannot be called by background worker!"

    'Status message
    Public Sub GUImsg(ByVal ID As tMsgID, ByVal Msg As String)
        F_MAINForm.MSGtoForm(ID, Msg, "", "")
    End Sub

    'Statusbar
    Public Sub Status(ByVal txt As String)
        F_MAINForm.ToolStripLbStatus.Text = txt
    End Sub

    'Status form reset
    Public Sub ClearMSG()
        F_MAINForm.LvMsg.Items.Clear()
    End Sub

#End Region

    'Class used to pass Messages from BackgroundWorker to GUI
    Public Class cWorkProg
        Private MyTarget As tWorkMsgType
        Private MyID As tMsgID
        Private MyMsg As String
        Private MyFileIndex As Int16
        Private MySource As String
        Private MyStatus As tJobStatus
        Public Link As String

        Public Sub New(ByVal MsgTarget As tWorkMsgType)
            MyTarget = MsgTarget
            MySource = ""
            MyStatus = tJobStatus.Undef
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

    'Progress bar control
    Public Class cProgBarCtrl
        Public ProgOverallStartInt As Integer = -1
        Public PgroOverallEndInt As Integer = -1
        Public ProgJobInt As Integer = -1
        Public ProgLock As Boolean = False
    End Class

#Region "Textbox text conversion for file open/save operations"

    'Text-to-number
    Public Function fTextboxToNumString(ByVal txt As String) As String
        If Not IsNumeric(txt) Then
            Return "0"
        Else
            Return txt
        End If
    End Function

    'Empty text to writable placeholder
    Public Function fStringOrDummySet(ByVal txt As String) As String
        If txt = "" Then
            Return sKey.EmptyString
        Else
            Return txt
        End If
    End Function

    'Placeholder to empty string
    Public Function fStringOrDummyGet(ByVal txt As String) As String
        If Trim(UCase(txt)) = sKey.EmptyString Then
            Return ""
        Else
            Return txt
        End If
    End Function

#End Region

    'Open File with software defined in Config
    Public Function FileOpenAlt(ByVal file As String) As Boolean
        Dim PSI As New ProcessStartInfo

        If Not IO.File.Exists(file) Then Return False

        PSI.FileName = Cfg.OpenCmd
        PSI.Arguments = ChrW(34) & file & ChrW(34)
        Try
            Process.Start(PSI)
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function WrongMode() As Integer

        If Cfg.DeclMode Then

            Select Case MsgBox("This file was created in Engineering Mode! Opening in Declaration Mode will overwrite some parameters with generic values." & vbCrLf & vbCrLf & _
             "Do you want to switch to Engineering Mode?" & vbCrLf & vbCrLf & _
             "[Yes] Switch mode and open file" & vbCrLf & _
             "[No] Open file without changing mode" & vbCrLf & _
             "[Cancel] Abort opening file" _
             , MsgBoxStyle.YesNoCancel, "Warning")
                Case MsgBoxResult.Yes
                    Return 1

                Case (MsgBoxResult.No)
                    Return 0

                Case Else
                    Return -1

            End Select

        Else

            Select Case MsgBox("This file was created in Declaration Mode! For use in Engineering Mode missing parameters must be defined." & vbCrLf & vbCrLf & _
                      "Do you want to switch to Declaration Mode?" & vbCrLf & vbCrLf & _
                      "[Yes] Switch mode and open file" & vbCrLf & _
                      "[No] Open file without changing mode" & vbCrLf & _
                      "[Cancel] Abort opening file" _
                      , MsgBoxStyle.YesNoCancel, "Warning")
                Case MsgBoxResult.Yes
                    Return 1

                Case (MsgBoxResult.No)
                    Return 0

                Case Else
                    Return -1

            End Select

        End If

    End Function

End Module
