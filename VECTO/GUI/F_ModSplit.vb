Imports System.Windows.Forms

Public Class F_ModSplit

    Private LastFile As String = ""
    Private sMODpath As String
    Private iVehList As Int32()
    Dim FBtemp As cFileBrowser

    Public Sub New()
        ' Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent()

        ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        FBtemp = New cFileBrowser("temp", False, True)
        FBtemp.Extensions = New String() {"mod"}
    End Sub

    'Load
    Private Sub F_ModSplit_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.TextBoxPath.Text = LastFile
    End Sub

    'Browse
    Private Sub ButBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButBrowse.Click
        If FBtemp.OpenDialog("", False, "mod") Then
            Me.TextBoxPath.Text = FBtemp.Files(0)
        Else
            Exit Sub
        End If
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click

        Dim VehStr As String
        Dim VehStrList As String()
        Dim x As Int16
        Dim SplitArray As System.Collections.Generic.List(Of Char)

        sMODpath = Me.TextBoxPath.Text
        VehStr = Me.RichTextBox1.Text

        If Not IO.File.Exists(sMODpath) Then
            MsgBox("File not found!")
            Exit Sub
        End If

        If VehStr = "" Then
            MsgBox("No Vehicle Numbers defined!")
            Exit Sub
        End If

        SplitArray = New System.Collections.Generic.List(Of Char)

        SplitArray.AddRange(vbNewLine.ToCharArray)
        SplitArray.Add(",")
        SplitArray.Add(";")

        '   Split
        VehStrList = VehStr.Split(SplitArray.ToArray, StringSplitOptions.RemoveEmptyEntries)

        '   Zu Int32 convertieren
        ReDim iVehList(UBound(VehStrList))
        For x = 0 To UBound(VehStrList)
            If IsNumeric(VehStrList(x)) Then
                iVehList(x) = CInt(VehStrList(x))
            Else
                iVehList(x) = 0
            End If
        Next
        '   Sort
        Array.Sort(iVehList)

        LastFile = sMODpath

        'Ende
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Public ReadOnly Property ModPath() As String
        Get
            Return sMODpath
        End Get
    End Property

    Public ReadOnly Property VehList() As Int32()
        Get
            Return iVehList
        End Get
    End Property

    Private Sub ButClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButClear.Click
        Me.RichTextBox1.Text = ""
    End Sub

End Class
