Imports System.Windows.Forms
Imports System.Runtime.InteropServices

Public Class F_ShutDown

    Private iTime As Int16
    Private objExitWin As New cWrapExitWindows()

    Public Function ShutDown() As Boolean
        If Me.ShowDialog = Windows.Forms.DialogResult.Cancel Then
            Me.Timer1.Stop()
            Return False
        Else
            Me.Timer1.Stop()
            Return True
        End If
    End Function

    Private Sub F_ShutDown_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        iTime = 99
        Me.LbTime.Text = iTime + 1
        Me.Timer1.Start()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.Timer1.Stop()
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub Timer1_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Me.LbTime.Text = iTime
        If iTime = 0 Then
            Me.Timer1.Stop()
            Try
                objExitWin.ExitWindows(cWrapExitWindows.Action.Shutdown)
                Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Catch ex As Exception
                Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
            End Try
            Me.Close()
        End If
        iTime -= 1
    End Sub

    Private Class cWrapExitWindows

        Private Declare Function ExitWindowsEx Lib "user32.dll" (ByVal uFlags As Int32, ByVal dwReserved As Int32) As Boolean
        Private Declare Function GetCurrentProcess Lib "kernel32.dll" () As IntPtr
        Private Declare Sub OpenProcessToken Lib "advapi32.dll" (ByVal ProcessHandle As IntPtr, ByVal DesiredAccess As Int32, ByRef TokenHandle As IntPtr)
        Private Declare Sub LookupPrivilegeValue Lib "advapi32.dll" Alias "LookupPrivilegeValueA" (ByVal lpSystemName As String, ByVal lpName As String, ByRef lpLuid As Long)
        Private Declare Function AdjustTokenPrivileges Lib "advapi32.dll" (ByVal TokenHandle As IntPtr, ByVal DisableAllPrivileges As Boolean, ByRef NewState As LUID, ByVal BufferLength As Int32, ByVal PreviousState As IntPtr, ByVal ReturnLength As IntPtr) As Boolean

        <StructLayout(LayoutKind.Sequential, Pack:=1)> _
        Friend Structure LUID
            Public Count As Integer
            Public LUID As Long
            Public Attribute As Integer
        End Structure

        Public Enum Action
            LogOff = 0
            Shutdown = 1
            Restart = 2
            PowerOff = 8
        End Enum

        Public Sub ExitWindows(ByVal how As Action, Optional ByVal Forced As Boolean = True)
            Dim TokenPrivilege As LUID
            Dim hProcess As IntPtr = GetCurrentProcess()
            Dim hToken As IntPtr = IntPtr.Zero
            OpenProcessToken(hProcess, &H28, hToken)
            TokenPrivilege.Count = 1
            TokenPrivilege.LUID = 0
            TokenPrivilege.Attribute = 2
            LookupPrivilegeValue(Nothing, "SeShutdownPrivilege", TokenPrivilege.LUID)
            AdjustTokenPrivileges(hToken, False, TokenPrivilege, 0, IntPtr.Zero, IntPtr.Zero)
            If Forced Then
                ExitWindowsEx(how + 4, 0)
            Else
                ExitWindowsEx(how, 0)
            End If
        End Sub

    End Class


End Class



