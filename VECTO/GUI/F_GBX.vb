Public Class F_GBX

    Private GbxFile As String = ""
    Public AutoSendTo As Boolean = False
    Public GenDir As String = ""
    Private GearDia As F_VEH_GearDlog

    Private Changed As Boolean = False

    Private Sub F_GBX_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.ApplicationExitCall And e.CloseReason <> CloseReason.WindowsShutDown Then
            e.Cancel = ChangeCheckCancel()
        End If
    End Sub

    Private Sub F_GBX_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Dim lvi As ListViewItem
        Dim i As Short

        GearDia = New F_VEH_GearDlog

        lvi = New ListViewItem("A")
        lvi.SubItems.Add("")
        lvi.SubItems.Add("")
        Me.LvGears.Items.Add(lvi)

        For i = 1 To 16
            lvi = New ListViewItem(i.ToString("00"))
            lvi.SubItems.Add("")
            lvi.SubItems.Add("")
            Me.LvGears.Items.Add(lvi)
        Next

        Changed = False
        newGBX()

    End Sub

#Region "ToolStrip"

    Private Sub ToolStripBtNew_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtNew.Click
        newGBX()
    End Sub

    Private Sub ToolStripBtOpen_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtOpen.Click
        If fbGBX.OpenDialog(GbxFile) Then openGBX(fbGBX.Files(0))
    End Sub

    Private Sub ToolStripBtSave_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSave.Click
        SaveOrSaveAs(False)
    End Sub

    Private Sub ToolStripBtSaveAs_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSaveAs.Click
        SaveOrSaveAs(True)
    End Sub

    Private Sub ToolStripBtSendTo_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSendTo.Click

        If ChangeCheckCancel() Then Exit Sub

        If GbxFile = "" Then
            If MsgBox("Save file now?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
                If Not SaveOrSaveAs(True) Then Exit Sub
            Else
                Exit Sub
            End If
        End If

        If Not F_GEN.Visible Then
            GenDir = ""
            F_GEN.Show()
            F_GEN.GENnew()
        Else
            F_GEN.WindowState = FormWindowState.Normal
        End If

        F_GEN.TbGBX.Text = fFileWoDir(GbxFile, GenDir)

    End Sub

    'Help
    Private Sub ToolStripButton1_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButton1.Click
        If IO.File.Exists(MyAppPath & "User Manual\GUI\GBX-Editor.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\GUI\GBX-Editor.html")
        Else
            MsgBox("User Manual not found!", MsgBoxStyle.Critical)
        End If
    End Sub

#End Region

    Private Sub newGBX()
        Dim lvi As ListViewItem

        If ChangeCheckCancel() Then Exit Sub

        Me.TbName.Text = ""
        Me.TbTracInt.Text = ""
        Me.TBI_getr.Text = ""

        For Each lvi In LvGears.Items
            lvi.SubItems(1).Text = "0"
            lvi.SubItems(2).Text = ""
            lvi.ForeColor = Color.Gray
        Next

        GbxFile = ""
        Me.Text = "GBX Editor"
        Me.LbStatus.Text = ""

        Changed = False

    End Sub

    Public Sub openGBX(ByVal file As String)
        Dim GBX0 As cGBX
        Dim i As Integer

        If ChangeCheckCancel() Then Exit Sub

        GBX0 = New cGBX

        GBX0.FilePath = file

        If Not GBX0.ReadFile Then
            MsgBox("Cannot read " & file & "!")
            Exit Sub
        End If

        Me.TbName.Text = GBX0.ModelName
        Me.TbTracInt.Text = GBX0.TracIntrSi.ToString
        Me.TBI_getr.Text = GBX0.I_Getriebe.ToString

        For i = 0 To 16
            Me.LvGears.Items(i).SubItems(1).Text = GBX0.GetrI(i)
            Me.LvGears.Items(i).SubItems(2).Text = GBX0.GetrMap(i, True)
            If GBX0.GetrI(i) = 0 Then
                Me.LvGears.Items(i).ForeColor = Color.Gray
            Else
                Me.LvGears.Items(i).ForeColor = Color.Black
            End If
        Next

        fbGBX.UpdateHistory(file)
        Me.Text = fFILE(file, True)
        Me.LbStatus.Text = ""
        GbxFile = file
        Me.Activate()

        Changed = False

    End Sub

    'Speichern oder Speichern als Function = true wenn Datei gespeichert
    Private Function SaveOrSaveAs(ByVal SaveAs As Boolean) As Boolean
        If GbxFile = "" Or SaveAs Then
            If fbGBX.SaveDialog(GbxFile) Then
                GbxFile = fbGBX.Files(0)
            Else
                Return False
            End If
        End If
        Return saveGBX(GbxFile)
    End Function

    Private Function saveGBX(ByVal file As String) As Boolean
        Dim GBX0 As cGBX
        Dim i As Int16

        GBX0 = New cGBX
        GBX0.FilePath = file

        GBX0.ModelName = Me.TbName.Text
        If Trim(GBX0.ModelName) = "" Then GBX0.ModelName = "Undefined"

        GBX0.TracIntrSi = fTextboxToNumString(Me.TbTracInt.Text)
        GBX0.I_Getriebe = fTextboxToNumString(Me.TBI_getr.Text)

        For i = 0 To 16
            GBX0.GetrI(i) = CSng(Me.LvGears.Items(i).SubItems(1).Text)
            GBX0.GetrMap(i) = Me.LvGears.Items(i).SubItems(2).Text
        Next


        If Not GBX0.SaveFile Then
            MsgBox("Cannot safe to " & file, MsgBoxStyle.Critical)
            Return False
        End If

        If Not GenDir = "" Or AutoSendTo Then
            If F_GEN.Visible And UCase(fFileRepl(F_GEN.TbGBX.Text, GenDir)) <> UCase(file) Then F_GEN.TbGBX.Text = fFileWoDir(file, GenDir)
        End If

        fbGBX.UpdateHistory(file)
        Me.Text = fFILE(file, True)
        Me.LbStatus.Text = ""

        Changed = False

        Return True

    End Function

#Region "Change Events"

    'Change Status ändern
    Private Sub Change()
        If Not Changed Then
            Me.LbStatus.Text = "Unsaved changes in current file"
            Changed = True
        End If
    End Sub

    ' "Save changes ?" ...liefert True wenn User Vorgang abbricht
    Private Function ChangeCheckCancel() As Boolean

        If Changed Then
            Select Case MsgBox("Save changes ?", MsgBoxStyle.YesNoCancel)
                Case MsgBoxResult.Yes
                    Return Not SaveOrSaveAs(False)
                Case MsgBoxResult.Cancel
                    Return True
                Case Else 'MsgBoxResult.No
                    Changed = False
                    Return False
            End Select

        Else

            Return False

        End If

    End Function

    Private Sub TbName_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbName.TextChanged
        Change()
    End Sub

    Private Sub TBI_getr_TextChanged(sender As System.Object, e As System.EventArgs) Handles TBI_getr.TextChanged
        Change()
    End Sub

    Private Sub TbTracInt_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbTracInt.TextChanged
        Change()
    End Sub

#End Region

    Private Sub ButOK_Click(sender As System.Object, e As System.EventArgs) Handles ButOK.Click
        If SaveOrSaveAs(False) Then Me.Close()
    End Sub

    Private Sub ButCancel_Click(sender As System.Object, e As System.EventArgs) Handles ButCancel.Click
        Me.Close()
    End Sub


#Region "Gears"

    'Gear-DoubleClick
    Private Sub LvGears_MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles LvGears.MouseDoubleClick
        EditGear()
    End Sub

    'Gear-KeyDown
    Private Sub LvGears_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles LvGears.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete, Keys.Back
                ClearGear()
            Case Keys.Enter
                EditGear()
        End Select
    End Sub

    'Clear Gear Button
    Private Sub BtClearGear_Click(sender As System.Object, e As System.EventArgs) Handles BtClearGear.Click
        ClearGear()
    End Sub

    'Edit Gear
    Private Sub EditGear()

        Do
            GearDia.GbxPath = fPATH(GbxFile)
            GearDia.TbGear.Text = Me.LvGears.SelectedItems(0).SubItems(0).Text
            GearDia.TbRatio.Text = Me.LvGears.SelectedItems(0).SubItems(1).Text
            GearDia.TbMapPath.Text = Me.LvGears.SelectedItems(0).SubItems(2).Text

            GearDia.BtNext.Enabled = (Me.LvGears.SelectedIndices(0) < Me.LvGears.Items.Count - 1)

            If GearDia.ShowDialog = Windows.Forms.DialogResult.OK Then
                Me.LvGears.SelectedItems(0).SubItems(1).Text = GearDia.TbRatio.Text
                Me.LvGears.SelectedItems(0).SubItems(2).Text = GearDia.TbMapPath.Text
                If GearDia.TbRatio.Text = "0" Then
                    Me.LvGears.SelectedItems(0).ForeColor = Color.Gray
                Else
                    Me.LvGears.SelectedItems(0).ForeColor = Color.Black
                End If

                Change()

            End If

            If GearDia.NextGear Then Me.LvGears.Items(Me.LvGears.SelectedIndices(0) + 1).Selected = True

        Loop Until Not GearDia.NextGear

    End Sub

    'Clear Gear
    Private Sub ClearGear()
        Dim lv0 As ListViewItem

        If Me.LvGears.SelectedItems.Count = 0 Then Exit Sub

        lv0 = Me.LvGears.SelectedItems(0)

        lv0.SubItems(1).Text = "0"
        lv0.SubItems(2).Text = ""
        lv0.ForeColor = Color.Gray

        If lv0.Index < Me.LvGears.Items.Count - 1 Then
            Me.LvGears.Items(lv0.Index + 1).Selected = True
            Me.LvGears.Items(lv0.Index + 1).EnsureVisible()
        End If

        Me.LvGears.Focus()

        Change()

    End Sub


#End Region

  
End Class