Public Class F_ENG

    Private EngFile As String = ""
    Public AutoSendTo As Boolean = False
    Public GenDir As String = ""
    Private Changed As Boolean = False

    Private Sub F_ENG_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.ApplicationExitCall And e.CloseReason <> CloseReason.WindowsShutDown Then
            e.Cancel = ChangeCheckCancel()
        End If
    End Sub

    Private Sub F_ENG_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Changed = False
        newENG()
    End Sub

#Region "ToolStrip"

    Private Sub ToolStripBtNew_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtNew.Click
        newENG()
    End Sub

    Private Sub ToolStripBtOpen_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtOpen.Click
        If fbENG.OpenDialog(EngFile) Then openENG(fbENG.Files(0))
    End Sub

    Private Sub ToolStripBtSave_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSave.Click
        SaveOrSaveAs(False)
    End Sub

    Private Sub ToolStripBtSaveAs_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSaveAs.Click
        SaveOrSaveAs(True)
    End Sub

    Private Sub ToolStripBtSendTo_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSendTo.Click

        If ChangeCheckCancel() Then Exit Sub

        If EngFile = "" Then
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

        F_GEN.TbENG.Text = fFileWoDir(EngFile, GenDir)

    End Sub

    Private Sub ToolStripButton1_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButton1.Click
        If IO.File.Exists(MyAppPath & "User Manual\GUI\ENG-Editor.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\GUI\ENG-Editor.html")
        Else
            MsgBox("User Manual not found!", MsgBoxStyle.Critical)
        End If
    End Sub

#End Region

    Private Sub newENG()

        If ChangeCheckCancel() Then Exit Sub

        Me.TbName.Text = ""
        Me.TbPnenn.Text = ""
        Me.TbDispl.Text = ""
        Me.TbInertia.Text = ""
        Me.TbNleerl.Text = ""
        Me.TbNnenn.Text = ""
        Me.TbFLD.Text = ""
        Me.TbMAP.Text = ""
        Me.TbWHTC.Text = ""

        EngFile = ""
        Me.Text = "ENG Editor"
        Me.LbStatus.Text = ""

        Changed = False

    End Sub

    Public Sub openENG(ByVal file As String)
        Dim ENG0 As cENG

        If ChangeCheckCancel() Then Exit Sub

        ENG0 = New cENG

        ENG0.FilePath = file

        If Not ENG0.ReadFile Then
            MsgBox("Cannot read " & file & "!")
            Exit Sub
        End If

        Me.TbName.Text = ENG0.ModelName
        Me.TbPnenn.Text = ENG0.Pnenn.ToString
        Me.TbDispl.Text = ENG0.Displ.ToString
        Me.TbInertia.Text = ENG0.I_mot.ToString
        Me.TbNleerl.Text = ENG0.nleerl.ToString
        Me.TbNnenn.Text = ENG0.nnenn.ToString
        Me.TbFLD.Text = ENG0.PathFLD(True)
        Me.TbMAP.Text = ENG0.PathMAP(True)
        Me.TbWHTC.Text = ENG0.PathWHTC(True)

        fbENG.UpdateHistory(file)
        Me.Text = fFILE(file, True)
        Me.LbStatus.Text = ""
        EngFile = file
        Me.Activate()

        Changed = False

    End Sub

    'Save or Save As function = true if file is saved
    Private Function SaveOrSaveAs(ByVal SaveAs As Boolean) As Boolean
        If EngFile = "" Or SaveAs Then
            If fbENG.SaveDialog(EngFile) Then
                EngFile = fbENG.Files(0)
            Else
                Return False
            End If
        End If
        Return saveENG(EngFile)
    End Function

    'Save ENG
    Private Function saveENG(ByVal file As String) As Boolean
        Dim ENG0 As cENG

        ENG0 = New cENG
        ENG0.FilePath = file

        ENG0.ModelName = Me.TbName.Text
        If Trim(ENG0.ModelName) = "" Then ENG0.ModelName = "Undefined"
        ENG0.Pnenn = CSng(fTextboxToNumString(Me.TbPnenn.Text))
        ENG0.Displ = CSng(fTextboxToNumString(Me.TbDispl.Text))
        ENG0.I_mot = CSng(fTextboxToNumString(Me.TbInertia.Text))
        ENG0.nleerl = CSng(fTextboxToNumString(Me.TbNleerl.Text))
        ENG0.nnenn = CSng(fTextboxToNumString(Me.TbNnenn.Text))

        ENG0.PathFLD = Me.TbFLD.Text
        ENG0.PathMAP = Me.TbMAP.Text
        ENG0.PathWHTC = Me.TbWHTC.Text

        If Not ENG0.SaveFile Then
            MsgBox("Cannot safe to " & file, MsgBoxStyle.Critical)
            Return False
        End If

        If Not GenDir = "" Or AutoSendTo Then
            If F_GEN.Visible And UCase(fFileRepl(F_GEN.TbENG.Text, GenDir)) <> UCase(file) Then F_GEN.TbENG.Text = fFileWoDir(file, GenDir)
        End If

        fbENG.UpdateHistory(file)
        Me.Text = fFILE(file, True)
        Me.LbStatus.Text = ""

        Changed = False

        Return True

    End Function


#Region "Change Events"

    'Change Status ändern |@@| Change Status change
    Private Sub Change()
        If Not Changed Then
            Me.LbStatus.Text = "Unsaved changes in current file"
            Changed = True
        End If
    End Sub

    ' "Save changes ?" ...liefert True wenn User Vorgang abbricht |@@| Save changes? "... Return True if User aborts
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

    Private Sub TbPnenn_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbPnenn.TextChanged
        Change()
    End Sub

    Private Sub TbDispl_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbDispl.TextChanged
        Change()
    End Sub

    Private Sub TbInertia_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbInertia.TextChanged
        Change()
    End Sub

    Private Sub TbNleerl_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbNleerl.TextChanged
        Change()
    End Sub

    Private Sub TbNnenn_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbNnenn.TextChanged
        Change()
    End Sub

    Private Sub TbFLD_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbFLD.TextChanged
        Change()
    End Sub

    Private Sub TbMAP_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbMAP.TextChanged
        Change()
    End Sub

    Private Sub TbWHTC_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbWHTC.TextChanged
        Change()
    End Sub

#End Region




#Region "Browse Buttons"

    Private Sub BtFLD_Click(sender As System.Object, e As System.EventArgs) Handles BtFLD.Click
        If fbFLD.OpenDialog(fFileRepl(Me.TbFLD.Text, fPATH(EngFile))) Then Me.TbFLD.Text = fFileWoDir(fbFLD.Files(0), fPATH(EngFile))
    End Sub

    Private Sub BtMAP_Click(sender As System.Object, e As System.EventArgs) Handles BtMAP.Click
        If fbMAP.OpenDialog(fFileRepl(Me.TbMAP.Text, fPATH(EngFile))) Then Me.TbMAP.Text = fFileWoDir(fbMAP.Files(0), fPATH(EngFile))
    End Sub

    Private Sub BtWHTC_Click(sender As System.Object, e As System.EventArgs) Handles BtWHTC.Click
        If fbWHTC.OpenDialog(fFileRepl(Me.TbWHTC.Text, fPATH(EngFile))) Then Me.TbWHTC.Text = fFileWoDir(fbWHTC.Files(0), fPATH(EngFile))
    End Sub

#End Region


    Private Sub ButOK_Click(sender As System.Object, e As System.EventArgs) Handles ButOK.Click
        If SaveOrSaveAs(False) Then Me.Close()
    End Sub

    Private Sub ButCancel_Click(sender As System.Object, e As System.EventArgs) Handles ButCancel.Click
        Me.Close()
    End Sub

    
End Class
