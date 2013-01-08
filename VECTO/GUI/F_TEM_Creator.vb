Public Class F_TEM_Creator

    'Initialization
    Private Sub F04_TEM_Creator_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.TextBoxTemp.Text = "20"
        Me.ComboBoxDrCyc.Items.Add("Average")
        Me.ComboBoxDrCyc.Items.Add("Residential Street")
        Me.ComboBoxDrCyc.Items.Add("Main Street")
        Me.ComboBoxDrCyc.SelectedIndex = 0
        Me.TextBoxPath.Text = Cfg.LastTEM
        Me.CheckBoxEXL.Checked = Cfg.TEMexl
    End Sub

    'Cancel
    Private Sub ButtonCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCancel.Click
        Me.Close()
    End Sub

    'OK
    Private Sub ButtonOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonOK.Click
        Dim f As String
        If Me.TextBoxPath.Text = "" Then
            MsgBox("No file specified!", MsgBoxStyle.Critical)
            Exit Sub
        End If
        f = Me.TextBoxPath.Text
        Cfg.TEMexl = Me.CheckBoxEXL.Checked
        If UCase(Microsoft.VisualBasic.Right(f, 4)) <> ".TEM" Then f = f & ".TEM"
        Cfg.LastTEM = f
        f = fFileRepl(f)
        If IO.File.Exists(f) Then
            If MsgBox("Overwrite existing file?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then Exit Sub
        End If
        Me.Close()
        If F_ADV.Visible Then
            F_ADV.WindowState = FormWindowState.Normal
            F_ADV.TEMfromCreator(f)
        End If
        If Cfg.TEMexl Then FileOpen(f)
    End Sub

    'Browse
    Private Sub ButtonBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonBrowse.Click
        If fbTEM.SaveDialog(Me.TextBoxPath.Text) Then
            Me.TextBoxPath.Text = fbTEM.Files(0)
        End If
    End Sub
End Class
