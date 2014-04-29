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

Imports System.Collections.Generic

Public Class F_Graph

    Private Filepath As String
    Private Channels As List(Of cChannel)
    Private DistList As List(Of Single)
    Private TimeList As List(Of Single)

    Private xMin As Single
    Private xMax As Single

    Private xMax0 As Single


    Public Sub New()

        ' Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent()

        ' Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Channels = New List(Of cChannel)

        Clear()

        Me.CbXaxis.SelectedIndex = 0


    End Sub

    Private Sub ToolStripBtOpen_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripBtOpen.Click
       
        If fbVMOD.OpenDialog(Filepath) Then

            LoadNewFile(fbVMOD.Files(0))

        End If

    End Sub


    Private Sub ToolStripButton2_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButton2.Click
        LoadFile()
    End Sub

    Public Sub LoadNewFile(ByVal Path As String)
        Dim lv0 As ListViewItem
        Dim i As Integer

        Clear()

        Filepath = Path

        LoadFile()

        For i = 2 To 3
            lv0 = New ListViewItem
            lv0.Text = Channels(i).Name
            lv0.SubItems.Add(Channels(i).Unit)
            lv0.SubItems.Add("Left")
            lv0.Tag = i
            lv0.Checked = True
            Me.ListView1.Items.Add(lv0)
        Next

    End Sub

    Private Sub LoadFile()
        Dim file As cFile_V3
        Dim i As Integer
        Dim sDim As Integer
        Dim line As String()
        Dim c0 As cChannel
        Dim l0 As List(Of String)


        file = New cFile_V3

        If file.OpenRead(Filepath) Then

            Try

                Channels.Clear()

                'For i = 1 To 4
                '    file.ReadLine()
                'Next

                'Header
                line = file.ReadLine

                sDim = UBound(line)

                For i = 0 To sDim
                    c0 = New cChannel
                    c0.Name = line(i)
                    c0.Values = New List(Of String)
                    Channels.Add(c0)
                Next

                'Units
                line = file.ReadLine

                For i = 0 To sDim
                    Channels(i).Unit = line(i)
                Next

                'Values
                Do While Not file.EndOfFile
                    line = file.ReadLine
                    For i = 0 To sDim
                        Channels(i).Values.Add(line(i))
                    Next
                Loop

                file.Close()

                l0 = Channels(0).Values
                TimeList = New List(Of Single)
                For i = 0 To l0.Count - 1
                    TimeList.Add(CSng(l0(i)))
                Next

                l0 = Channels(1).Values
                DistList = New List(Of Single)
                For i = 0 To l0.Count - 1
                    DistList.Add(CSng(l0(i)))
                Next

                SetxMax0()

                Me.TbXmin.Text = 0
                Me.TbXmax.Text = xMax0

                Me.Text = fFILE(Filepath, True)

            Catch ex As Exception
                file.Close()
                Exit Sub
            End Try

        End If

        UpdateGraph()

    End Sub

    Private Sub SetxMax0()

        If Channels.Count = 0 Then Exit Sub

        If Me.CbXaxis.SelectedIndex = 0 Then
            xMax0 = DistList(DistList.Count - 1)
        Else
            xMax0 = TimeList(TimeList.Count - 1)
        End If

    End Sub

    Private Sub UpdateGraph()
        Dim lv0 As ListViewItem
        Dim MyChart As System.Windows.Forms.DataVisualization.Charting.Chart
        Dim s As System.Windows.Forms.DataVisualization.Charting.Series
        Dim a As System.Windows.Forms.DataVisualization.Charting.ChartArea
        Dim OverDist As Boolean
        Dim leftaxis As New List(Of String)
        Dim rightaxis As New List(Of String)
        Dim IsLeft As Boolean
        Dim txt As String
        Dim i As Integer
        Dim img As Image

        If Me.ListView1.CheckedItems.Count = 0 Then
            Me.PictureBox1.Image = Nothing
            Exit Sub
        End If

        OverDist = (Me.CbXaxis.SelectedIndex = 0)

        SetxMax0()


        MyChart = New System.Windows.Forms.DataVisualization.Charting.Chart
        MyChart.Width = Me.PictureBox1.Width
        MyChart.Height = Me.PictureBox1.Height

        a = New System.Windows.Forms.DataVisualization.Charting.ChartArea


        For Each lv0 In Me.ListView1.CheckedItems

            IsLeft = (lv0.SubItems(2).Text = "Left")

            s = New System.Windows.Forms.DataVisualization.Charting.Series

            If OverDist Then
                s.Points.DataBindXY(DistList, Channels(lv0.Tag).Values)
            Else
                s.Points.DataBindXY(TimeList, Channels(lv0.Tag).Values)
            End If

            s.ChartType = DataVisualization.Charting.SeriesChartType.FastLine
            s.Name = lv0.Text
            s.BorderWidth = 2

            If IsLeft Then
                If Not leftaxis.Contains(lv0.SubItems(1).Text) Then leftaxis.Add(lv0.SubItems(1).Text)
            Else
                If Not rightaxis.Contains(lv0.SubItems(1).Text) Then rightaxis.Add(lv0.SubItems(1).Text)
                s.YAxisType = DataVisualization.Charting.AxisType.Secondary
            End If

            MyChart.Series.Add(s)

        Next


        a.Name = "main"

        If OverDist Then
            a.AxisX.Title = "distance [km]"
        Else
            a.AxisX.Title = "time [s]"
        End If
        a.AxisX.TitleFont = New Font("Helvetica", 10)
        a.AxisX.LabelStyle.Font = New Font("Helvetica", 8)
        a.AxisX.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None
        a.AxisX.MajorGrid.LineDashStyle = DataVisualization.Charting.ChartDashStyle.Dot

        If xMax > xMin Then
            a.AxisX.Minimum = xMin
            a.AxisX.Maximum = xMax
            a.AxisX.Interval = xAutoInt()
        Else
            a.AxisX.Minimum = 0
            a.AxisX.Maximum = xMax0
        End If



        If leftaxis.Count > 0 Then

            txt = leftaxis(0)
            For i = 1 To leftaxis.Count - 1
                txt &= ", " & leftaxis(i)
            Next

            a.AxisY.Title = txt
            a.AxisY.TitleFont = New Font("Helvetica", 10)
            a.AxisY.LabelStyle.Font = New Font("Helvetica", 8)
            a.AxisY.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None

        End If

        If rightaxis.Count > 0 Then

            txt = rightaxis(0)
            For i = 1 To rightaxis.Count - 1
                txt &= ", " & rightaxis(i)
            Next

            a.AxisY2.Title = txt
            a.AxisY2.TitleFont = New Font("Helvetica", 10)
            a.AxisY2.LabelStyle.Font = New Font("Helvetica", 8)
            a.AxisY2.LabelAutoFitStyle = DataVisualization.Charting.LabelAutoFitStyles.None
            a.AxisY2.MinorGrid.Enabled = False
            a.AxisY2.MajorGrid.Enabled = False

        End If

        a.BackColor = Color.GhostWhite

        a.BorderDashStyle = DataVisualization.Charting.ChartDashStyle.Solid
        a.BorderWidth = 1

        MyChart.ChartAreas.Add(a)

        With MyChart.ChartAreas(0)
            .Position.X = 0
            .Position.Y = 0
            .Position.Width = 85
            .Position.Height = 100
        End With

        MyChart.Legends.Add("main")
        MyChart.Legends(0).Font = New Font("Helvetica", 8)
        MyChart.Legends(0).BorderColor = Color.Black
        MyChart.Legends(0).BorderWidth = 1
        MyChart.Legends(0).Position.X = 86
        MyChart.Legends(0).Position.Y = 3
        MyChart.Legends(0).Position.Width = 13
        MyChart.Legends(0).Position.Height = 40

        MyChart.Update()

        img = New Bitmap(MyChart.Width, MyChart.Height, Imaging.PixelFormat.Format32bppArgb)
        MyChart.DrawToBitmap(img, New Rectangle(0, 0, Me.PictureBox1.Width, Me.PictureBox1.Height))

        Me.PictureBox1.Image = img

    End Sub

    Private Function xAutoInt() As Single
        Dim Interv0 As Single
        Dim Grx As Long
        Dim xyd(3) As Single
        Dim xydmin As Single
        Dim xyamin As Single
        Dim xya(3) As Single
        Dim i As Int16

        Interv0 = (xMax - xMin) / 10

        Grx = 20
        Do While 10 ^ Grx > Interv0
            Grx = Grx - 1
        Loop

        xyd(0) = 1 * 10 ^ Grx
        xyd(1) = 2.5 * 10 ^ Grx
        xyd(2) = 5 * 10 ^ Grx
        xyd(3) = 10 * 10 ^ Grx
        For i = 0 To 3
            xya(i) = Math.Abs(Interv0 - xyd(i))
        Next

        xyamin = xya(0)
        xydmin = xyd(0)
        For i = 1 To 3
            If xya(i) < xyamin Then
                xyamin = xya(i)
                xydmin = xyd(i)
            End If
        Next

        'Intervall speichern
        Return xydmin

    End Function

    Private Sub Clear()

        Filepath = ""

        Me.ListView1.Items.Clear()

        Me.TbXmin.Text = ""
        Me.TbXmax.Text = ""

        Me.PictureBox1.Image = Nothing
    End Sub

    Private Class cChannel
        Public Name As String
        Public Unit As String
        Public Values As List(Of String)

    End Class

    Private Sub BtAddCh_Click(sender As System.Object, e As System.EventArgs) Handles BtAddCh.Click
        Dim dlog As New F_Graph_ChEdit
        Dim i As Integer
        Dim lv0 As ListViewItem

        If Channels.Count = 0 Then Exit Sub

        For i = 0 To Channels.Count - 1
            dlog.ComboBox1.Items.Add(Channels(i).Name & "  " & Channels(i).Unit)
        Next

        dlog.RbLeft.Checked = True

        dlog.ComboBox1.SelectedIndex = 0

        If dlog.ShowDialog = Windows.Forms.DialogResult.OK Then
            lv0 = New ListViewItem
            i = dlog.ComboBox1.SelectedIndex
            lv0.Text = Channels(i).Name
            lv0.SubItems.Add(Channels(i).Unit)
            lv0.Tag = i
            lv0.Checked = True
            If dlog.RbLeft.Checked Then
                lv0.SubItems.Add("Left")
            Else
                lv0.SubItems.Add("Right")
            End If

            Me.ListView1.Items.Add(lv0)

            UpdateGraph()

        End If

    End Sub

    Private Sub EditChannel()
        Dim dlog As New F_Graph_ChEdit
        Dim i As Integer
        Dim lv0 As ListViewItem

        If Me.ListView1.SelectedItems.Count = 0 Or Channels.Count = 0 Then Exit Sub

        lv0 = Me.ListView1.SelectedItems(0)

        For i = 0 To Channels.Count - 1
            dlog.ComboBox1.Items.Add(Channels(i).Name & "  " & Channels(i).Unit)
        Next

        If lv0.SubItems(2).Text = "Left" Then
            dlog.RbLeft.Checked = True
        Else
            dlog.RbRight.Checked = True
        End If

        dlog.ComboBox1.SelectedIndex = lv0.Tag

        If dlog.ShowDialog = Windows.Forms.DialogResult.OK Then
            i = dlog.ComboBox1.SelectedIndex
            lv0.Text = Channels(i).Name
            lv0.SubItems(1).Text = Channels(i).Unit
            lv0.Tag = i
            lv0.Checked = True
            If dlog.RbLeft.Checked Then
                lv0.SubItems(2).Text = "Left"
            Else
                lv0.SubItems(2).Text = "Right"
            End If

            UpdateGraph()

        End If

    End Sub

    Private Sub RemoveChannel()
        Dim i0 As Int16

        If Me.ListView1.Items.Count = 0 Then Exit Sub

        If Me.ListView1.SelectedItems.Count = 0 Then Me.ListView1.Items(Me.ListView1.Items.Count - 1).Selected = True

        i0 = Me.ListView1.SelectedItems(0).Index

        Me.ListView1.SelectedItems(0).Remove()

        If i0 < Me.ListView1.Items.Count Then
            Me.ListView1.Items(i0).Selected = True
            Me.ListView1.Items(i0).EnsureVisible()
        End If

        UpdateGraph()

    End Sub

    Private Sub ListView1_DoubleClick(sender As Object, e As System.EventArgs) Handles ListView1.DoubleClick
        If Me.ListView1.SelectedItems.Count > 0 Then
            Me.ListView1.SelectedItems(0).Checked = Not Me.ListView1.SelectedItems(0).Checked
            EditChannel()
        End If
    End Sub

    Private Sub BtRemCh_Click(sender As System.Object, e As System.EventArgs) Handles BtRemCh.Click
        RemoveChannel()
    End Sub

    Private Sub ListView1_ItemChecked(sender As Object, e As System.Windows.Forms.ItemCheckedEventArgs) Handles ListView1.ItemChecked
        UpdateGraph()
    End Sub

    Private Sub ListView1_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles ListView1.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete, Keys.Back
                RemoveChannel()
            Case Keys.Enter
                EditChannel()
        End Select
    End Sub

    Private Sub CbXaxis_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles CbXaxis.SelectedIndexChanged
        SetxMax0()
        Me.TbXmin.Text = 0
        Me.TbXmax.Text = xMax0
        UpdateGraph()
    End Sub

    Private Sub BtReset_Click(sender As System.Object, e As System.EventArgs) Handles BtReset.Click
        xMin = 0
        xMax = xMax0
        Me.TbXmin.Text = 0
        Me.TbXmax.Text = xMax0
    End Sub

    Private Sub TbXmin_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbXmin.TextChanged
        If IsNumeric(Me.TbXmin.Text) Then xMin = Me.TbXmin.Text
        UpdateGraph()
    End Sub

    Private Sub TbXmax_TextChanged(sender As System.Object, e As System.EventArgs) Handles TbXmax.TextChanged
        If IsNumeric(Me.TbXmax.Text) Then xMax = Me.TbXmax.Text
        UpdateGraph()
    End Sub

    Private Sub ToolStripButton3_Click(sender As System.Object, e As System.EventArgs) Handles ToolStripButton3.Click
        Dim FGraph As New F_Graph
        FGraph.Show()
    End Sub

    Private Sub F_Graph_SizeChanged(sender As Object, e As System.EventArgs) Handles Me.SizeChanged
        UpdateGraph()
    End Sub

    Private Sub BtZoomIn_Click(sender As System.Object, e As System.EventArgs) Handles BtZoomIn.Click
        Dim d As Single

        d = (xMax - xMin) / 10

        xMin += 2 * 0.5 * d
        xMax -= 2 * (1 - 0.5) * d

        If xMin > 1000 Then
            xMin = Math.Round(xMin / 100, 0) * 100
        Else
            xMin = Math.Round(xMin, 0)
        End If

        Me.TbXmin.Text = xMin
        Me.TbXmax.Text = xMax
      
    End Sub

    Private Sub BtZoomOut_Click(sender As System.Object, e As System.EventArgs) Handles BtZoomOut.Click
        Dim d As Single

        d = (xMax - xMin) / 10

        xMin -= 2 * 0.5 * d
        xMax += 2 * (1 - 0.5) * d

        If xMin > 1000 Then
            xMin = Math.Round(xMin / 100, 0) * 100
        Else
            xMin = Math.Round(xMin, 0)
        End If

        Me.TbXmin.Text = xMin
        Me.TbXmax.Text = xMax

    End Sub

    Private Sub BtMoveL_Click(sender As System.Object, e As System.EventArgs) Handles BtMoveL.Click
        Dim d As Single

        If xMin <= 0 Then Exit Sub

        d = (xMax - xMin) / 3
        xMin -= d
        xMax -= d

        If xMin > 1000 Then
            xMin = Math.Round(xMin / 100, 0) * 100
        Else
            xMin = Math.Round(xMin, 0)
        End If

        Me.TbXmin.Text = xMin
        Me.TbXmax.Text = xMax


    End Sub

    Private Sub BtMoveR_Click(sender As System.Object, e As System.EventArgs) Handles BtMoveR.Click
        Dim d As Single

        If xMax >= xMax0 Then Exit Sub

        d = (xMax - xMin) / 3
        xMin += d
        xMax += d

        If xMin > 1000 Then
            xMin = Math.Round(xMin / 100, 0) * 100
        Else
            xMin = Math.Round(xMin, 0)
        End If

        Me.TbXmin.Text = xMin
        Me.TbXmax.Text = xMax

    End Sub
End Class