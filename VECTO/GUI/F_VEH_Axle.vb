Imports System.Windows.Forms

''' <summary>
''' Axle Config Editor (Vehicle Editor sub-dialog)
''' </summary>
''' <remarks></remarks>
Public Class F_VEH_Axle

    Public Sub New()
        Dim w As String

        ' Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent()

        ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Me.CbWheels.Items.Add("-")
        For Each w In Declaration.WheelsList
            Me.CbWheels.Items.Add(w)
        Next



    End Sub

    Public Sub Clear()
        Me.CbTwinT.Checked = False
        Me.TbAxleShare.Text = ""
        Me.TbI_wheels.Text = ""
        Me.TbRRC.Text = ""
        Me.TbFzISO.Text = ""
        Me.CbWheels.SelectedIndex = 0
    End Sub

    'Initialise
    Private Sub F_VEH_Axle_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Me.PnAxle.Enabled = Not Cfg.DeclMode
    End Sub

    'Save and close
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click

        If Not Cfg.DeclMode Then
            If Not IsNumeric(Me.TbAxleShare.Text) OrElse Trim(Me.TbAxleShare.Text) = "" Then
                MsgBox("Weight input is not valid!")
                Exit Sub
            End If
        End If

        If Not IsNumeric(Me.TbRRC.Text) OrElse Trim(Me.TbRRC.Text) = "" Then
            MsgBox("RRC input is not valid!")
            Exit Sub
        End If

        If Not IsNumeric(Me.TbFzISO.Text) OrElse Trim(Me.TbFzISO.Text) = "" Then
            MsgBox("Fz ISO input is not valid!")
            Exit Sub
        End If

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub CbWheels_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles CbWheels.SelectedIndexChanged
        Dim inertia As Single
        If Cfg.DeclMode Then
            inertia = Declaration.WheelsInertia(Me.CbWheels.Text)
            If inertia < 0 Then
                Me.TbI_wheels.Text = "-"
            Else
                Me.TbI_wheels.Text = inertia
            End If
        End If
    End Sub

    'Cancel
    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub



    
End Class
