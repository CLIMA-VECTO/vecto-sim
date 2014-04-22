Imports System.Windows.Forms

''' <summary>
''' Create/Verify signature files (.vsig).
''' </summary>
''' <remarks></remarks>
Public Class F_FileSign

    'Create signature file
    Private Sub BtSign_Click(sender As System.Object, e As System.EventArgs) Handles BtSign.Click
        Dim lv0 As ListViewItem
        Dim MainDir As String

        If Me.lvFiles.Items.Count = 0 Then
            MsgBox("No files selected!", MsgBoxStyle.Critical)
            Exit Sub
        End If

        If Trim(Me.TbSigFile.Text) = "" Then
            MsgBox("No signature file path defined!", MsgBoxStyle.Critical)
            Exit Sub
        End If

        If IO.File.Exists(Me.TbSigFile.Text) Then
            If MsgBox("Overwrite existing signature file?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then Exit Sub
        End If

        ClearForm(False)


        MainDir = fPATH(Me.TbSigFile.Text)


        Lic.FileSigning.NewFile()
        Lic.FileSigning.Mode = vectolic.cFileSigning.tMode.Manual


        For Each lv0 In Me.lvFiles.Items
            Lic.FileSigning.AddFile(fFileRepl(lv0.SubItems(0).Text, MainDir))
            lv0.SubItems(1).Text = ""
            lv0.ForeColor = Color.Black
        Next

        If Lic.FileSigning.WriteSigFile(Me.TbSigFile.Text, LicSigAppCode) Then
            Me.LbStatus.Text = "Signature file created successfully"
            Me.LbStatus.ForeColor = Color.DarkGreen
        Else
            Me.LbStatus.Text = "Fail to create signature file! " & Lic.FileSigning.ErrorMsg
            Me.LbStatus.ForeColor = Color.Red
        End If

        Me.TbLicStr.Text = Lic.FileSigning.CreatorLicStr
        Me.TbPubKey.Text = Lic.FileSigning.PubKey
        Me.LbMode.Text = Lic.FileSigning.ModeConv(Lic.FileSigning.Mode)
        Me.LbDateStr.Text = Lic.FileSigning.DateStr

        If Lic.FileSigning.FilesOK.Count > 0 Then
            For Each lv0 In Me.lvFiles.Items
                lv0.SubItems(1).Text = Lic.FileSigning.FilesMsg(lv0.Index)
                If Lic.FileSigning.FilesOK(lv0.Index) Then
                    lv0.ForeColor = Color.DarkGreen
                Else
                    lv0.ForeColor = Color.Red
                    Exit For
                End If
            Next
        End If


    End Sub

    'Verify existing signature file
    Public Sub VerifySigFile()
        Dim lv0 As ListViewItem
        Dim i As Integer

        If Not IO.File.Exists(Me.TbSigFile.Text) Then
            MsgBox("Signature file not found!", MsgBoxStyle.Critical)
            Exit Sub
        End If

        ClearForm(True)

        If Lic.FileSigning.ReadSigFile(Me.TbSigFile.Text, LicSigAppCode) Then
            Me.LbStatus.Text = "File signature verified"
            Me.LbStatus.ForeColor = Color.DarkGreen
        Else
            Me.LbStatus.Text = "ERROR! " & Lic.FileSigning.ErrorMsg
            Me.LbStatus.ForeColor = Color.Red
        End If

        Me.TbLicStr.Text = Lic.FileSigning.CreatorLicStr
        Me.TbPubKey.Text = Lic.FileSigning.PubKey
        Me.LbMode.Text = Lic.FileSigning.ModeConv(Lic.FileSigning.Mode)
        If Lic.FileSigning.Mode = vectolic.cFileSigning.tMode.Auto Then
            Me.LbMode.ForeColor = Color.DarkGreen
        Else
            Me.LbMode.ForeColor = Color.Red
        End If
        Me.LbDateStr.Text = Lic.FileSigning.DateStr

        For i = 0 To Lic.FileSigning.FilesOK.Count - 1
            lv0 = New ListViewItem(Lic.FileSigning.Files(i))
            lv0.SubItems.Add(Lic.FileSigning.FilesMsg(i))
            If Lic.FileSigning.FilesOK(i) Then
                lv0.ForeColor = Color.DarkGreen
            Else
                lv0.ForeColor = Color.Red
            End If
            Me.lvFiles.Items.Add(lv0)
        Next

    End Sub

    'Clear form
    Private Sub ClearForm(ByVal ClearFileList As Boolean)
        If ClearFileList Then lvFiles.Items.Clear()
        Me.TbLicStr.Text = ""
        Me.TbPubKey.Text = ""
        Me.LbMode.Text = ""
        Me.LbDateStr.Text = ""
        Me.LbStatus.Text = ""
        Me.LbMode.ForeColor = Control.DefaultForeColor
        Me.LbMode.BackColor = Control.DefaultBackColor
        Me.LbStatus.ForeColor = Control.DefaultForeColor
        Me.LbStatus.BackColor = Control.DefaultBackColor
    End Sub


#Region "GUI Controls"

    Private Sub BtBrowse_Click(sender As System.Object, e As System.EventArgs) Handles BtBrowse.Click
        Dim fb As New cFileBrowser("sig", False, True)
        fb.Extensions = New String() {"vsig"}

        If fb.CustomDialog(Me.TbSigFile.Text, False, False, tFbExtMode.ForceExt, False, "vsig") Then
            Me.TbSigFile.Text = fb.Files(0)
        End If

        If IO.File.Exists(Me.TbSigFile.Text) Then
            VerifySigFile()
        End If

    End Sub

    Private Sub BtAddFLD_Click(sender As System.Object, e As System.EventArgs) Handles BtAddFLD.Click
        AddFile()
    End Sub

    Private Sub BtRemFLD_Click(sender As System.Object, e As System.EventArgs) Handles BtRemFLD.Click
        RemoveFile()
    End Sub

    Private Sub BtClose_Click(sender As System.Object, e As System.EventArgs) Handles BtClose.Click
        Me.Close()
    End Sub

    Private Sub BtClearList_Click(sender As System.Object, e As System.EventArgs) Handles BtClearList.Click
        Me.lvFiles.Items.Clear()
    End Sub

    Private Sub BtReload_Click(sender As System.Object, e As System.EventArgs) Handles BtReload.Click
        VerifySigFile()
    End Sub

    Private Sub lvFiles_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles lvFiles.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete, Keys.Back
                RemoveFile()
        End Select
    End Sub

#End Region

    'Add File
    Private Sub AddFile()
        Dim lvi As ListViewItem
        Dim fb As New cFileBrowser("xxx", False, True)
        Dim str As String

        If fb.OpenDialog("", True) Then

            For Each str In fb.Files

                lvi = New ListViewItem(str)
                lvi.SubItems.Add("")
                lvi.ForeColor = Color.Black

                Me.lvFiles.Items.Add(lvi)
                lvi.EnsureVisible()

                Me.lvFiles.Focus()

            Next

        End If

    End Sub

    'Remove File
    Private Sub RemoveFile()
        Dim i0 As Int16

        If Me.lvFiles.Items.Count = 0 Then Exit Sub

        If Me.lvFiles.SelectedItems.Count = 0 Then Me.lvFiles.Items(Me.lvFiles.Items.Count - 1).Selected = True

        i0 = Me.lvFiles.SelectedItems(0).Index

        Me.lvFiles.SelectedItems(0).Remove()

        If i0 < Me.lvFiles.Items.Count Then
            Me.lvFiles.Items(i0).Selected = True
            Me.lvFiles.Items(i0).EnsureVisible()
        End If

        Me.lvFiles.Focus()

    End Sub


End Class
