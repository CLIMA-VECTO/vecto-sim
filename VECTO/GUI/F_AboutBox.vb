''' <summary>
''' About Dialog. Shows Licence and contact/support information
''' </summary>
''' <remarks></remarks>
Public Class F_AboutBox


    'Initialize
    Private Sub F10_AboutBox_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Text = "VECTO " & VECTOvers '& "/ GUI " & GUIvers
        Me.LabelLic.Text = Lic.LicString
        Me.LabelLicDate.Text = "Expiring date (y/m/d):   " & Lic.ExpTime
    End Sub

    'e-mail links----------------------------------------------------------------
    Private Sub LinkLUZ_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLUZ.LinkClicked
        System.Diagnostics.Process.Start("mailto:luz@ivt.tugraz.at")
    End Sub

    Private Sub LinkREX_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkREX.LinkClicked
        System.Diagnostics.Process.Start("mailto:rexeis@ivt.tugraz.at")
    End Sub

    Private Sub LinkHAUS_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkHAUS.LinkClicked
        System.Diagnostics.Process.Start("mailto:hausberger@ivt.tugraz.at")
    End Sub

    Private Sub LinkJRC1_LinkClicked(sender As Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkJRC1.LinkClicked
        System.Diagnostics.Process.Start("mailto:georgios.fontaras@ftco.eu")
    End Sub

    Private Sub LinkJRC2_LinkClicked(sender As Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkJRC2.LinkClicked
        System.Diagnostics.Process.Start("mailto:panagiota.dilara@jrc.ec.europa.eu")
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As System.Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        System.Diagnostics.Process.Start("mailto:konstantinos.anagnostopoulos@ext.jrc.ec.europa.eu")
    End Sub

    '----------------------------------------------------------------------------

    'Picture Links------------------------------------------------------------------
    Private Sub PictureBoxFVT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBoxFVT.Click
        System.Diagnostics.Process.Start("http://www.ivt.tugraz.at/")
    End Sub
    Private Sub PictureBoxTUG_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBoxTUG.Click
        System.Diagnostics.Process.Start("http://www.tugraz.at/")
    End Sub
    '----------------------------------------------------------------------------
    Private Sub PictureBoxJRC_Click(sender As System.Object, e As System.EventArgs) Handles PictureBoxJRC.Click
        System.Diagnostics.Process.Start("http://ec.europa.eu/dgs/jrc/index.cfm")
    End Sub


End Class
