Imports System.Windows.Forms

''' <summary>
''' Sub-dialog for File Browser. Entirely controlled by cFilebrowser class (via FB_Dialog).
''' </summary>
''' <remarks></remarks>
Public Class FB_FavDlog

    Private Const NoFavString As String = "<undefined>"
    Private Const EmptyText As String = " "

    Private Sub FB_FavDlog_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Dim x As Integer
        Dim txt As String
        For x = 10 To 19
            txt = FB_FolderHistory(x)
            If txt = EmptyText Then
                Me.ListBox1.Items.Add(NoFavString)
            Else
                Me.ListBox1.Items.Add(txt)
            End If
        Next
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub ListBox1_MouseDoubleClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles ListBox1.MouseDoubleClick
        Dim i As Integer
        Dim txt As String
        Dim fb As cFileBrowser

        i = Me.ListBox1.SelectedIndex

        txt = Me.ListBox1.Items(i).ToString

        If txt = NoFavString Then txt = ""

        fb = New cFileBrowser("DirBr", True, True)

        If fb.OpenDialog(txt) Then
            txt = fb.Files(0)
            Me.ListBox1.Items.Insert(i, txt)
            Me.ListBox1.Items.RemoveAt(i + 1)
        End If

    End Sub


End Class
