Imports System.Xml.Linq
' Copyright 2014 European Union.
' Licensed under the EUPL (the 'Licence');
'
' * You may not use this work except in compliance with the Licence.
' * You may obtain a copy of the Licence at: http://ec.europa.eu/idabc/eupl
' * Unless required by applicable law or agreed to in writing,
'   software distributed under the Licence is distributed on an "AS IS" basis,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'
' See the LICENSE.txt for the specific language governing permissions and limitations.

''' <summary>
''' Engine Editor. Open and save .VENG files.
''' </summary>
''' <remarks></remarks>
Public Class F_ENG

    Private EngFile As String = ""
    Public AutoSendTo As Boolean = False
    Public JobDir As String = ""
    Private Changed As Boolean = False


    'Before closing Editor: Check if file was changed and ask to save.
    Private Sub F_ENG_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.ApplicationExitCall And e.CloseReason <> CloseReason.WindowsShutDown Then
            e.Cancel = ChangeCheckCancel()
        End If
    End Sub

    'Initialise.
    Private Sub F_ENG_Load(sender As Object, e As System.EventArgs) Handles Me.Load

		Me.PnInertia.Enabled = Not Cfg.DeclMode
        Me.GrWHTC.Enabled = Cfg.DeclMode


        Changed = False
        newENG()
    End Sub

    'Set generic values for Declaration mode.
    Private Sub DeclInit()

        If Not Cfg.DeclMode Then Exit Sub

        Me.TbInertia.Text = CStr(Declaration.EngInertia(fTextboxToNumString(Me.TbDispl.Text)))
    End Sub


#Region "Toolbar"

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

        If Not F_VECTO.Visible Then
            JobDir = ""
            F_VECTO.Show()
            F_VECTO.VECTOnew()
        Else
            F_VECTO.WindowState = FormWindowState.Normal
        End If

        F_VECTO.TbENG.Text = fFileWoDir(EngFile, JobDir)

    End Sub

    Private Sub ToolStripButton1_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButton1.Click
        If IO.File.Exists(MyAppPath & "User Manual\GUI\GUI_Calls\ENG.html") Then
            System.Diagnostics.Process.Start(MyAppPath & "User Manual\GUI\GUI_Calls\ENG.html")
        Else
            MsgBox("User Manual not found!", MsgBoxStyle.Critical)
        End If
    End Sub

#End Region

    'Create new empty Engine file.
    Private Sub newENG()

        If ChangeCheckCancel() Then Exit Sub

        Me.TbName.Text = ""
        Me.TbDispl.Text = ""
        Me.TbInertia.Text = ""
        Me.TbNleerl.Text = ""
		Me.TbMAP.Text = ""
		Me.TbFLD.Text = ""
        Me.TbWHTCurban.Text = ""
        Me.TbWHTCrural.Text = ""
        Me.TbWHTCmw.Text = ""


        DeclInit()


        EngFile = ""
        Me.Text = "ENG Editor"
        Me.LbStatus.Text = ""

        Changed = False

        UpdatePic()

    End Sub

    'Open VENG file
    Public Sub openENG(ByVal file As String)
        Dim ENG0 As cENG

        If ChangeCheckCancel() Then Exit Sub

        ENG0 = New cENG

        ENG0.FilePath = file

        If Not ENG0.ReadFile Then
            MsgBox("Cannot read " & file & "!")
            Exit Sub
        End If

        If Cfg.DeclMode <> ENG0.SavedInDeclMode Then
            Select Case WrongMode()
                Case 1
                    Me.Close()
                    F_MAINForm.RbDecl.Checked = Not F_MAINForm.RbDecl.Checked
                    F_MAINForm.OpenVectoFile(file)
                Case -1
                    Exit Sub
                Case Else '0
                    'Continue...
            End Select
        End If

        Me.TbName.Text = ENG0.ModelName
        Me.TbDispl.Text = ENG0.Displ.ToString
        Me.TbInertia.Text = ENG0.I_mot.ToString
        Me.TbNleerl.Text = ENG0.Nidle.ToString

		Me.TbMAP.Text = ENG0.PathMAP(True)
		Me.TbFLD.Text = ENG0.PathFLD(True)
        Me.TbWHTCurban.Text = ENG0.WHTCurban
        Me.TbWHTCrural.Text = ENG0.WHTCrural
        Me.TbWHTCmw.Text = ENG0.WHTCmw


        DeclInit()


        fbENG.UpdateHistory(file)
        Me.Text = fFILE(file, True)
        Me.LbStatus.Text = ""
        EngFile = file
        Me.Activate()

        Changed = False
        UpdatePic()

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

    'Save VENG file to given filepath. Called by SaveOrSaveAs. 
    Private Function saveENG(ByVal file As String) As Boolean
        Dim ENG0 As cENG

        ENG0 = New cENG
        ENG0.FilePath = file

        ENG0.ModelName = Me.TbName.Text
        If Trim(ENG0.ModelName) = "" Then ENG0.ModelName = "Undefined"
        ENG0.Displ = CSng(fTextboxToNumString(Me.TbDispl.Text))
        ENG0.I_mot = CSng(fTextboxToNumString(Me.TbInertia.Text))
        ENG0.Nidle = CSng(fTextboxToNumString(Me.TbNleerl.Text))

		ENG0.PathFLD = Me.TbFLD.Text
		ENG0.PathMAP = Me.TbMAP.Text


        ENG0.WHTCurban = CSng(fTextboxToNumString(Me.TbWHTCurban.Text))
        ENG0.WHTCrural = CSng(fTextboxToNumString(Me.TbWHTCrural.Text))
        ENG0.WHTCmw = CSng(fTextboxToNumString(Me.TbWHTCmw.Text))


        If Not ENG0.SaveFile Then
            MsgBox("Cannot safe to " & file, MsgBoxStyle.Critical)
            Return False
        End If

        If AutoSendTo Then
            If F_VECTO.Visible Then
                If UCase(fFileRepl(F_VECTO.TbENG.Text, JobDir)) <> UCase(file) Then F_VECTO.TbENG.Text = fFileWoDir(file, JobDir)
                F_VECTO.UpdatePic()
            End If
        End If

        fbENG.UpdateHistory(file)
        Me.Text = fFILE(file, True)
        Me.LbStatus.Text = ""

        Changed = False

        Return True

    End Function


#Region "Track changes"

    'Flags current file as modified.
    Private Sub Change()
        If Not Changed Then
            Me.LbStatus.Text = "Unsaved changes in current file"
            Changed = True
        End If
    End Sub

    ' "Save changes ?" .... Returns True if User aborts
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

    Private Sub TbPnenn_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

    Private Sub TbDispl_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbDispl.TextChanged
        Change()
        DeclInit()
    End Sub

    Private Sub TbInertia_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbInertia.TextChanged
        Change()
    End Sub

    Private Sub TbNleerl_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbNleerl.TextChanged
        UpdatePic()
        Change()
    End Sub

    Private Sub TbNnenn_TextChanged(sender As System.Object, e As System.EventArgs)
        Change()
    End Sub

	Private Sub TbMAP_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbMAP.TextChanged, TbFLD.TextChanged
		UpdatePic()
		Change()
	End Sub

    Private Sub TbWHTCurban_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbWHTCurban.TextChanged
        Change()
    End Sub

    Private Sub TbWHTCrural_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbWHTCrural.TextChanged
        Change()
    End Sub

    Private Sub TbWHTCmw_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbWHTCmw.TextChanged
        Change()
    End Sub

	Private Sub LvFLDs_SelectedIndexChanged(sender As System.Object, e As System.EventArgs)
		UpdatePic()
	End Sub



#End Region

	'Browse for VMAP file
	Private Sub BtMAP_Click(sender As System.Object, e As System.EventArgs) Handles BtMAP.Click
		If fbMAP.OpenDialog(fFileRepl(Me.TbMAP.Text, fPATH(EngFile))) Then Me.TbMAP.Text = fFileWoDir(fbMAP.Files(0), fPATH(EngFile))
	End Sub


    'Open VMAP file
	Private Sub BtMAPopen_Click(sender As System.Object, e As System.EventArgs) Handles BtMAPopen.Click
		Dim fldfile As String

		fldfile = fFileRepl(Me.TbFLD.Text, fPATH(EngFile))

		If fldfile <> sKey.NoFile AndAlso IO.File.Exists(fldfile) Then
			OpenFiles(fFileRepl(Me.TbMAP.Text, fPATH(EngFile)), fldfile)
		Else
			OpenFiles(fFileRepl(Me.TbMAP.Text, fPATH(EngFile)))
		End If

	End Sub


    'Save and close
    Private Sub ButOK_Click(sender As System.Object, e As System.EventArgs) Handles ButOK.Click
        If SaveOrSaveAs(False) Then Me.Close()
    End Sub

    'Close without saving (see FormClosing Event)
    Private Sub ButCancel_Click(sender As System.Object, e As System.EventArgs) Handles ButCancel.Click
        Me.Close()
    End Sub

    Private Sub UpdatePic()

        Dim fldOK As Boolean = False
        Dim mapOK As Boolean = False
        Dim FLD0 As New cFLD
        Dim MAP0 As New cMAP
        Dim MyChart As System.Windows.Forms.DataVisualization.Charting.Chart
        Dim s As System.Windows.Forms.DataVisualization.Charting.Series
        Dim a As System.Windows.Forms.DataVisualization.Charting.ChartArea
        Dim img As Image

        Me.PicBox.Image = Nothing

        Try

            'Read Files
			FLD0.FilePath = fFileRepl(Me.TbFLD.Text, fPATH(EngFile))
			fldOK = FLD0.ReadFile(False, False)

            MAP0.FilePath = fFileRepl(Me.TbMAP.Text, fPATH(EngFile))
			mapOK = MAP0.ReadFile(False)

        Catch ex As Exception

        End Try

        If Not fldOK And Not mapOK Then Exit Sub


        'Create plot
        MyChart = New System.Windows.Forms.DataVisualization.Charting.Chart
        MyChart.Width = Me.PicBox.Width
        MyChart.Height = Me.PicBox.Height

        a = New System.Windows.Forms.DataVisualization.Charting.ChartArea

        If fldOK Then

            s = New System.Windows.Forms.DataVisualization.Charting.Series
            s.Points.DataBindXY(FLD0.LnU, FLD0.LTq)
            s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
            s.BorderWidth = 2
            s.Color = Color.DarkBlue
            s.Name = "Full load (" & fFILE(FLD0.FilePath, True) & ")"
            MyChart.Series.Add(s)

            s = New System.Windows.Forms.DataVisualization.Charting.Series
            s.Points.DataBindXY(FLD0.LnU, FLD0.LTqDrag)
            s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
            s.BorderWidth = 2
            s.Color = Color.Blue
            s.Name = "Motoring (" & fFILE(FLD0.FilePath, True) & ")"
			MyChart.Series.Add(s)

        End If

        If mapOK Then
            s = New System.Windows.Forms.DataVisualization.Charting.Series
            s.Points.DataBindXY(MAP0.nU, MAP0.Tq)
            s.ChartType = DataVisualization.Charting.SeriesChartType.Point
            s.MarkerSize = 3
            s.Color = Color.Red
            s.Name = "Map"
            MyChart.Series.Add(s)
        End If

        a.Name = "main"

        a.AxisX.Title = "engine speed [1/min]"
        a.AxisX.TitleFont = New Font("Helvetica", 10)
        a.AxisX.LabelStyle.Font = New Font("Helvetica", 8)
        a.AxisX.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None
        a.AxisX.MajorGrid.LineDashStyle = DataVisualization.Charting.ChartDashStyle.Dot

        a.AxisY.Title = "engine torque [Nm]"
        a.AxisY.TitleFont = New Font("Helvetica", 10)
        a.AxisY.LabelStyle.Font = New Font("Helvetica", 8)
        a.AxisY.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None
        a.AxisY.MajorGrid.LineDashStyle = DataVisualization.Charting.ChartDashStyle.Dot

        a.AxisX.Minimum = 300
        a.BorderDashStyle = DataVisualization.Charting.ChartDashStyle.Solid
        a.BorderWidth = 1

        a.BackColor = Color.GhostWhite

        MyChart.ChartAreas.Add(a)

        MyChart.Update()

        img = New Bitmap(MyChart.Width, MyChart.Height, Imaging.PixelFormat.Format32bppArgb)
        MyChart.DrawToBitmap(img, New Rectangle(0, 0, Me.PicBox.Width, Me.PicBox.Height))


        Me.PicBox.Image = img


    End Sub



#Region "Open File Context Menu"

    Private CmFiles As String()

    Private Sub OpenFiles(ParamArray files() As String)

        If files.Length = 0 Then Exit Sub

        CmFiles = files

        OpenWithToolStripMenuItem.Text = "Open with " & Cfg.OpenCmdName

        CmOpenFile.Show(Cursor.Position)

    End Sub

    Private Sub OpenWithToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles OpenWithToolStripMenuItem.Click
        If Not FileOpenAlt(CmFiles(0)) Then MsgBox("Failed to open file!")
    End Sub

    Private Sub ShowInFolderToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ShowInFolderToolStripMenuItem.Click
        If IO.File.Exists(CmFiles(0)) Then
            Try
                System.Diagnostics.Process.Start("explorer", "/select,""" & CmFiles(0) & "")
            Catch ex As Exception
                MsgBox("Failed to open file!")
            End Try
        Else
            MsgBox("File not found!")
        End If
    End Sub

#End Region



	Private Sub BtFLD_Click(sender As Object, e As EventArgs) Handles BtFLD.Click
		If fbFLD.OpenDialog(fFileRepl(Me.TbFLD.Text, fPATH(EngFile))) Then Me.TbFLD.Text = fFileWoDir(fbFLD.Files(0), fPATH(EngFile))

	End Sub

	Private Sub BtFLDopen_Click(sender As Object, e As EventArgs) Handles BtFLDopen.Click
		Dim fldfile As String

		fldfile = fFileRepl(Me.TbFLD.Text, fPATH(EngFile))

		If fldfile <> sKey.NoFile AndAlso IO.File.Exists(fldfile) Then
			OpenFiles(fldfile)
		End If

	End Sub

	Private Sub BtWHTCimport_Click(sender As Object, e As EventArgs) Handles BtWHTCimport.Click
		Dim xml As XDocument

		Dim dlog As New cFileBrowser("XML", False, True)
		dlog.Extensions = New String() {"xml"}

		If Not dlog.OpenDialog("") Then Exit Sub

		Try
			xml = XDocument.Load(dlog.Files(0))

			Me.TbWHTCurban.Text = xml.<VECTO-Engine-TransferFile>.<WHTCCorrectionFactors>.<Urban>.Value
			Me.TbWHTCrural.Text = xml.<VECTO-Engine-TransferFile>.<WHTCCorrectionFactors>.<Rural>.Value
			Me.TbWHTCmw.Text = xml.<VECTO-Engine-TransferFile>.<WHTCCorrectionFactors>.<Motorway>.Value

		Catch ex As Exception
			MsgBox("Failed to load file! " & ex.Message, MsgBoxStyle.Critical)
		End Try

	End Sub
End Class
