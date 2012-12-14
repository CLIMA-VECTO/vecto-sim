Public Class F_ADV

    Private Const IFanz As Int16 = 4
    Dim Advfile As String
    Dim Changed As Boolean = False

    'Initialisieren
    Private Sub F05_ADV_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Changed = False
    End Sub

    'Schließen
    Private Sub F05_ADV_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.ApplicationExitCall And e.CloseReason <> CloseReason.WindowsShutDown Then
            e.Cancel = ChangeCheckCancel()
        End If
    End Sub

    'Inputfile- Steuerelemente------------------------------------------
  
    'Browse Button-Click Events
    Private Sub ButtonFZP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonFZP.Click
        If fbFZP.OpenDialog(fFileRepl(Me.TextBoxFZP.Text, fPATH(Advfile))) Then Me.TextBoxFZP.Text = fbFZP.Files(0)
    End Sub

    Private Sub ButtonFLT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonFLT.Click
        If fbFLT.OpenDialog(fFileRepl(Me.TextBoxFLT.Text, fPATH(Advfile))) Then Me.TextBoxFLT.Text = fbFLT.Files(0)
    End Sub

    Private Sub ButtonTEM_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonTEM.Click
        If fbTEM.OpenDialog(fFileRepl(Me.TextBoxTEM.Text, fPATH(Advfile))) Then Me.TextBoxTEM.Text = fbTEM.Files(0)
    End Sub

    Private Sub ButSTRadd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButSTRadd.Click
        Dim file As String
        If Me.LbSTR.Items.Count > 0 Then
            file = Me.LbSTR.Items(Me.LbSTR.Items.Count - 1).ToString
        Else
            file = ""
        End If
        If fbSTR.OpenDialog(file, True) Then
            For Each file In fbSTR.Files
                Me.LbSTR.Items.Add(file)
            Next
            Change()
        End If
    End Sub

    Private Sub ButSTRrem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButSTRrem.Click
        If Me.LbSTR.SelectedItems.Count > 0 Then Change()
        Do While Me.LbSTR.SelectedItems.Count > 0
            Me.LbSTR.Items.Remove(Me.LbSTR.SelectedItems(0))
        Loop
    End Sub

#Region "Toolbar"

    'New
    Private Sub ToolStripBtNew_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtNew.Click
        Call ADVnew()
    End Sub

    'Open
    Private Sub ToolStripBtOpen_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtOpen.Click
        If fbADV.OpenDialog(Advfile) Then ADVload2Form(fbADV.Files(0))
    End Sub

    'Save
    Private Sub ToolStripBtSave_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSave.Click
        Save()
    End Sub

    'Save As
    Private Sub ToolStripBtSaveAs_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSaveAs.Click
        If fbADV.SaveDialog(Advfile) Then ADVsave(fbADV.Files(0))
    End Sub

    'Send to ADV List
    Private Sub ToolStripBtSendTo_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtSendTo.Click
        If ChangeCheckCancel() Then Exit Sub
        If Advfile = "" Then
            MsgBox("File not found!" & ChrW(10) & ChrW(10) & "Save file and try again.")
        Else
            F_MAINForm.AddToListViewGEN(Advfile)
            Me.ToolStripStatusLabelADV.Text = Advfile & " sent to ADV List."
        End If
    End Sub

#End Region

    'ADV speichern
    Private Function Save() As Boolean
        If Advfile = "" Then
            If fbADV.SaveDialog("") Then
                Advfile = fbADV.Files(0)
            Else
                Return False
            End If
        End If
        Return ADVsave(Advfile)
    End Function



    '-------------------------------------------------------------------


    'Neue leere ADV
    Public Sub ADVnew()
        If ChangeCheckCancel() Then Exit Sub
        Me.TBseed.Text = "1"
        Me.TextBoxFZP.Text = ""
        Me.TextBoxFLT.Text = ""
        Me.TextBoxTEM.Text = ""
        Me.TbFilter.Text = "85"
        Me.LbSTR.Items.Clear()
        Me.ToolStripStatusLabelADV.Text = ""
        Me.CheckBoxMISKAM.Checked = False
        Me.CheckBoxSTRfilter.Checked = False
        Changed = False
        Advfile = ""
        Me.Text = "ADV Editor"
        Me.ToolStripStatusLabelADV.Text = "New file"
    End Sub

    'ADV in Form laden
    Public Sub ADVload2Form(ByVal file As String)
        Dim ADV As cADV
        Dim i As Integer

        If Not IO.File.Exists(file) Then
            MsgBox("File not found!")
            Exit Sub
        End If

        If ChangeCheckCancel() Then Exit Sub

        ADVnew()

        ADV = New cADV

        ADV.FilePath = file

        If Not ADV.ReadFile() Then
            MsgBox("Cannot read file!")
            Exit Sub
        End If

        'Zeile 1: FZP-Datei
        Me.TextBoxFZP.Text = ADV.FZPpath(True)

        'Zeile 2: FLT-Datei
        Me.TextBoxFLT.Text = ADV.FLTpath(True)

        'Zeile 3: TEM-Datei
        Me.TextBoxTEM.Text = ADV.TEMpath(True)

        'Zeile 4: RndSeed
        Me.TBseed.Text = ADV.RndSeed

        'Zeile 5: MISKAMout True/False
        Me.CheckBoxMISKAM.Checked = ADV.SD3out

        'Zeile 6: STRfilter True/False
        Me.CheckBoxSTRfilter.Checked = ADV.STRfilter

        'Zeile 7: Distance filter für SUM.STR
        Me.TbFilter.Text = ADV.STRSUMdistflt

        'Zeile 8+: STR Dateien
        For i = 1 To ADV.STRcount
            Me.LbSTR.Items.Add(ADV.STRpaths(i - 1, True))
        Next

        Advfile = file
        Me.Text = IO.Path.GetFileName(file) & " - ADV Editor"
        Changed = False
        Me.ToolStripStatusLabelADV.Text = "" 'file
    End Sub

    'ADV aus Form speichern
    Private Function ADVsave(ByVal file As String) As Boolean
        Dim ADV As cADV
        Dim s As String

        ADV = New cADV

        ADV.FilePath = file

        ADV.FZPpath = Me.TextBoxFZP.Text

        ADV.FLTpath = Me.TextBoxFLT.Text

        ADV.TEMpath = Me.TextBoxTEM.Text

        ADV.RndSeed = Me.TBseed.Text

        ADV.SD3out = Me.CheckBoxMISKAM.Checked

        ADV.STRfilter = Me.CheckBoxSTRfilter.Checked

        ADV.STRSUMdistflt = Me.TbFilter.Text

        'ADV.STRpathsClear()    '<= Nicht notwendig da neues lokales cADV-Objekt
        For Each s In Me.LbSTR.Items
            ADV.STRpathsAdd(s)
        Next

        If Not ADV.SaveFile Then
            MsgBox("Cannot safe to " & file, MsgBoxStyle.Critical)
            Return False
        End If

        Advfile = file

        Me.Text = fFILE(file, True) & " - ADV Editor"
        Advfile = file

        Me.ToolStripStatusLabelADV.Text = file & " saved."

        F_MAINForm.AddToListViewGEN(Advfile)

        Changed = False

        Return True

    End Function

    'TEMformCreator
    Sub TEMfromCreator(ByVal txt As String)
        Me.TextBoxTEM.Text = txt
    End Sub

    'Formular Änderungen (Kontrolle ob GEN gespeichert)---------------------------------
    'Event Handler für Formänderungen

    'Change Status ändern
    Private Sub Change()
        Me.ToolStripStatusLabelADV.Text = "Unsaved changes in current file"
        Changed = True
    End Sub

    ' "Save changes ?" ...liefert True wenn User Vorgang abbricht
    Private Function ChangeCheckCancel() As Boolean

        If Changed Then

            Select Case MsgBox("Save changes ?", MsgBoxStyle.YesNoCancel)
                Case MsgBoxResult.Yes
                    Return Not Save()
                Case MsgBoxResult.Cancel
                    Return True
                Case Else ' MsgBoxResult.No
                    Changed = False
                    Return False
            End Select

        Else

            Return False

        End If

    End Function
    '-----------------------------------------------------------------------------------

   

#Region "Control-Enables und Change!"

    Private Sub CBMISKAM_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxMISKAM.CheckedChanged
        If Me.CheckBoxMISKAM.Checked Then
            Me.CheckBoxSTRfilter.Checked = True
            Me.CheckBoxSTRfilter.Enabled = False
        Else
            Me.CheckBoxSTRfilter.Enabled = True
        End If
        Change()
    End Sub

    Private Sub CheckBoxSTRfilter_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxSTRfilter.CheckedChanged
        Me.LbSTR.Enabled = Me.CheckBoxSTRfilter.Checked
        Me.ButSTRadd.Enabled = Me.CheckBoxSTRfilter.Checked
        Me.ButSTRrem.Enabled = Me.CheckBoxSTRfilter.Checked
        Me.TbFilter.Enabled = Me.CheckBoxSTRfilter.Checked
        Me.LbFilter0.Enabled = Me.CheckBoxSTRfilter.Checked
        Me.LbFilter1.Enabled = Me.CheckBoxSTRfilter.Checked
        Change()
    End Sub

    Private Sub TextBoxFZP_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxFZP.TextChanged
        Change()
    End Sub

    Private Sub TextBoxFLT_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxFLT.TextChanged
        Change()
    End Sub

    Private Sub TextBoxTEM_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxTEM.TextChanged
        Change()
    End Sub

    Private Sub TbFilter_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TbFilter.TextChanged
        Change()
    End Sub

#End Region



    'OK (Save & Close)
    Private Sub ButSaveClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButOK.Click
        If Not Save() Then Exit Sub
        Me.Close()
    End Sub

    'Cancel
    Private Sub ButCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButCancel.Click
        Me.Close()
    End Sub


End Class
