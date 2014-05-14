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
    Private FLDdia As F_FLD


    'Before closing Editor: Check if file was changed and ask to save.
    Private Sub F_ENG_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason <> CloseReason.ApplicationExitCall And e.CloseReason <> CloseReason.WindowsShutDown Then
            e.Cancel = ChangeCheckCancel()
        End If
    End Sub

    'Initialise.
    Private Sub F_ENG_Load(sender As Object, e As System.EventArgs) Handles Me.Load

        FLDdia = New F_FLD

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
        Me.LvFLDs.Items.Clear()
        Me.TbMAP.Text = ""
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
        Dim i As Integer
        Dim lv0 As ListViewItem

        If ChangeCheckCancel() Then Exit Sub

        ENG0 = New cENG

        ENG0.FilePath = file

        If Not ENG0.ReadFile Then
            MsgBox("Cannot read " & file & "!")
            Exit Sub
        End If

        Me.TbName.Text = ENG0.ModelName
        Me.TbDispl.Text = ENG0.Displ.ToString
        Me.TbInertia.Text = ENG0.I_mot.ToString
        Me.TbNleerl.Text = ENG0.Nidle.ToString

        Me.LvFLDs.Items.Clear()
        For i = 0 To ENG0.fFLD.Count - 1
            lv0 = New ListViewItem(ENG0.PathFLD(i, True))
            lv0.SubItems.Add(ENG0.FLDgears(i))
            Me.LvFLDs.Items.Add(lv0)
        Next

        Me.TbMAP.Text = ENG0.PathMAP(True)
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
        Dim i As Int16

        ENG0 = New cENG
        ENG0.FilePath = file

        ENG0.ModelName = Me.TbName.Text
        If Trim(ENG0.ModelName) = "" Then ENG0.ModelName = "Undefined"
        ENG0.Displ = CSng(fTextboxToNumString(Me.TbDispl.Text))
        ENG0.I_mot = CSng(fTextboxToNumString(Me.TbInertia.Text))
        ENG0.Nidle = CSng(fTextboxToNumString(Me.TbNleerl.Text))

        For i = 0 To Me.LvFLDs.Items.Count - 1
            ENG0.fFLD.Add(New cSubPath)
            ENG0.PathFLD(i) = Me.LvFLDs.Items(i).SubItems(0).Text
            ENG0.FLDgears.Add(Me.LvFLDs.Items(i).SubItems(1).Text)
        Next

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

    Private Sub TbMAP_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbMAP.TextChanged
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

    Private Sub LvFLDs_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles LvFLDs.SelectedIndexChanged
        UpdatePic()
    End Sub



#End Region

#Region "Add/Remove/Edit VFLD entries"

    Private Sub LvFLDs_DoubleClick(sender As Object, e As System.EventArgs) Handles LvFLDs.DoubleClick
        EditFLD()
    End Sub

    Private Sub LvFLDs_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles LvFLDs.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete, Keys.Back
                RemoveFLD(False)
            Case Keys.Enter
                EditFLD()
        End Select
    End Sub

    Private Sub BtAddFLD_Click(sender As System.Object, e As System.EventArgs) Handles BtAddFLD.Click
        AddFLD()
        Me.LvFLDs.Items(Me.LvFLDs.Items.Count - 1).Selected = True
        EditFLD()
    End Sub

    Private Sub BtRemFLD_Click(sender As System.Object, e As System.EventArgs) Handles BtRemFLD.Click
        RemoveFLD(False)
    End Sub

    Private Sub EditFLD()
        Dim nums As String()

        FLDdia.EngFile = EngFile
        FLDdia.TbFLD.Text = Me.LvFLDs.SelectedItems(0).SubItems(0).Text

        If Me.LvFLDs.SelectedItems(0).SubItems(1).Text.Contains("-") Then
            Try
                nums = Me.LvFLDs.SelectedItems(0).SubItems(1).Text.Replace(" ", "").Split("-")
                FLDdia.NumGearFrom.Value = Math.Max(CInt(nums(0)), 0)
                FLDdia.NumGearTo.Value = Math.Min(CInt(nums(1)), 99)
            Catch ex As Exception
                FLDdia.NumGearFrom.Value = 0
                FLDdia.NumGearTo.Value = 99
                MsgBox(Me.LvFLDs.SelectedItems(0).SubItems(1).Text & " is no valid range!")
            End Try
        Else
            FLDdia.NumGearFrom.Value = Math.Max(CInt(Me.LvFLDs.SelectedItems(0).SubItems(1).Text), 0)
            FLDdia.NumGearTo.Value = Math.Min(CInt(Me.LvFLDs.SelectedItems(0).SubItems(1).Text), 99)
        End If

        If FLDdia.ShowDialog = Windows.Forms.DialogResult.OK Then

            Me.LvFLDs.SelectedItems(0).SubItems(0).Text = FLDdia.TbFLD.Text

            If FLDdia.NumGearFrom.Value = FLDdia.NumGearTo.Value Then
                Me.LvFLDs.SelectedItems(0).SubItems(1).Text = FLDdia.NumGearFrom.Value
            Else
                Me.LvFLDs.SelectedItems(0).SubItems(1).Text = FLDdia.NumGearFrom.Value & "-" & FLDdia.NumGearTo.Value
            End If

            UpdatePic()

            Change()

        Else

            If Me.LvFLDs.SelectedItems(0).SubItems(0).Text = "" Then RemoveFLD(True)

        End If


    End Sub

    Private Sub AddFLD()
        Dim lvi As ListViewItem

        lvi = New ListViewItem("")
        lvi.SubItems.Add("0 - 99")
        Me.LvFLDs.Items.Add(lvi)

        lvi.EnsureVisible()

        Me.LvFLDs.Focus()

        'Change() => NO! Already in EditFLD

    End Sub

    Private Sub RemoveFLD(ByVal NoChange As Boolean)
        Dim i0 As Int16

        If Me.LvFLDs.Items.Count = 0 Then Exit Sub

        If Me.LvFLDs.SelectedItems.Count = 0 Then Me.LvFLDs.Items(Me.LvFLDs.Items.Count - 1).Selected = True

        i0 = Me.LvFLDs.SelectedItems(0).Index

        Me.LvFLDs.SelectedItems(0).Remove()

        If i0 < Me.LvFLDs.Items.Count Then
            Me.LvFLDs.Items(i0).Selected = True
            Me.LvFLDs.Items(i0).EnsureVisible()
        End If

        Me.LvFLDs.Focus()
        UpdatePic()


        If Not NoChange Then Change()

    End Sub

#End Region


    'Browse for VMAP file
    Private Sub BtMAP_Click(sender As System.Object, e As System.EventArgs) Handles BtMAP.Click
        If fbMAP.OpenDialog(fFileRepl(Me.TbMAP.Text, fPATH(EngFile))) Then Me.TbMAP.Text = fFileWoDir(fbMAP.Files(0), fPATH(EngFile))
    End Sub


    'Open VMAP file
    Private Sub BtMAPopen_Click(sender As System.Object, e As System.EventArgs) Handles BtMAPopen.Click
        Dim fldfile As String

        If Me.LvFLDs.Items.Count = 1 Then
            fldfile = fFileRepl(Me.LvFLDs.Items(0).Text, fPATH(EngFile))
        Else
            fldfile = sKey.NoFile
        End If


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
        Dim fp As String
        Dim FLD0 As New cFLD
        Dim MAP0 As New cMAP
        Dim Shiftpoly As cGBX.cShiftPolygon
        Dim MyChart As System.Windows.Forms.DataVisualization.Charting.Chart
        Dim s As System.Windows.Forms.DataVisualization.Charting.Series
        Dim a As System.Windows.Forms.DataVisualization.Charting.ChartArea
        Dim img As Image

        Me.PicBox.Image = Nothing

        Try

            'Read Files
            If Me.LvFLDs.Items.Count > 0 Then
                If Me.LvFLDs.SelectedItems.Count > 0 Then
                    fp = fFileRepl(Me.LvFLDs.SelectedItems(0).Text, fPATH(EngFile))
                Else
                    fp = fFileRepl(Me.LvFLDs.Items(0).Text, fPATH(EngFile))
                End If
                FLD0.FilePath = fp
                fldOK = FLD0.ReadFile(False)
            End If

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

            If IsNumeric(Me.TbNleerl.Text) AndAlso Me.TbNleerl.Text > 0 Then

                FLD0.Init(CSng(Me.TbNleerl.Text))

                Shiftpoly = New cGBX.cShiftPolygon("", 0)
                Shiftpoly.SetGenericShiftPoly(FLD0, Me.TbNleerl.Text)

                s = New System.Windows.Forms.DataVisualization.Charting.Series
                s.Points.DataBindXY(Shiftpoly.gs_nUup, Shiftpoly.gs_Mup)
                s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
                s.BorderWidth = 2
                s.Color = Color.DarkRed
                s.Name = "Upshift curve"
                MyChart.Series.Add(s)

                s = New System.Windows.Forms.DataVisualization.Charting.Series
                s.Points.DataBindXY(Shiftpoly.gs_nUdown, Shiftpoly.gs_Mdown)
                s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
                s.BorderWidth = 2
                s.Color = Color.DarkRed
                s.Name = "Downshift curve"
                MyChart.Series.Add(s)

            End If

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



End Class
