Imports System.Collections.Generic

Public Class cMAP

    'Private Const FormatVersion As Integer = 1
    'Private FileVersion As Integer

    Public EmComponents As Dictionary(Of String, cEmComp)
    Public EmDefRef As Dictionary(Of tMapComp, cEmComp)
    Private MyEmList As List(Of String)

    Public LPe As List(Of Single)
    Public LnU As List(Of Single)

    Private sFilePath As String
    Private iMapDim As Integer

    Private MapIntp As cMapInterpol

    Private FuelMap As cDelaunayMap

    Private Sub ResetMe()
        MapIntp = Nothing
        EmComponents = Nothing
        MyEmList = Nothing
        LPe = Nothing
        LnU = Nothing
        EmDefRef = Nothing
        iMapDim = -1
        FuelMap = New cDelaunayMap
    End Sub

    Public Function ReadFile(Optional ByVal MsgOutput As Boolean = True) As Boolean
        Dim file As cFile_V3
        Dim line As String()
        Dim s1 As Integer
        Dim s As Integer
        'Dim txt As String
        Dim Em0 As cEmComp
        Dim EmKV As System.Collections.Generic.KeyValuePair(Of String, cEmComp)
        Dim nU As Double
        Dim MsgSrc As String


        MsgSrc = "Main/ReadInp/MAP"

        'Reset
        ResetMe()

        'Stop if there's no file
        If sFilePath = "" OrElse Not IO.File.Exists(sFilePath) Then
            WorkerMsg(tMsgID.Err, "Map file not found! (" & sFilePath & ")", MsgSrc)
            Return False
        End If

        'Open file
        file = New cFile_V3
        If Not file.OpenRead(sFilePath) Then
            file = Nothing
            WorkerMsg(tMsgID.Err, "Failed to open file (" & sFilePath & ") !", MsgSrc)
            Return False
        End If

        'Initi Lists (before version check so ReadOldFormat works)
        MyEmList = New List(Of String)
        EmComponents = New System.Collections.Generic.Dictionary(Of String, cEmComp)
        EmDefRef = New System.Collections.Generic.Dictionary(Of tMapComp, cEmComp)
        LPe = New System.Collections.Generic.List(Of Single)
        LnU = New System.Collections.Generic.List(Of Single)

        s1 = 2

        'Column 3: fuel consumption
        s = 2
        Em0 = New cEmComp
        Em0.Col = s
        Em0.Name = "FC"  'wird bei Default-Komponenten noch geändert
        Em0.Unit = "[g/h]"
        Em0.IDstring = "<FC>"
        Em0.MapCompID = tMapComp.FC
        Em0.NormID = tEmNorm.x_h
        If EmComponents.ContainsKey(UCase(Em0.Name)) Then
            'Abort if already defined
            WorkerMsg(tMsgID.Err, "Component '" & Em0.Name & "' already defined! Col. " & s + 1, MsgSrc)
            GoTo lbEr
        Else
            Em0.Name = fMapCompName(Em0.MapCompID)
            EmComponents.Add(Em0.IDstring, Em0)
            MyEmList.Add(Em0.IDstring)
            EmDefRef.Add(Em0.MapCompID, Em0)
        End If

        'From line 4 (or  6): Values
        Try
            Do While Not file.EndOfFile

                'Line read
                line = file.ReadLine

                'Line counter up (was reset in ResetMe)
                iMapDim += 1

                'Revolutions
                nU = CDbl(line(0))

                LnU.Add(nU)

                'Power
                LPe.Add(nMtoPe(nU, CDbl(line(1))))

                'FC
                For Each EmKV In EmComponents
                    EmKV.Value.RawVals.Add(CSng(line(EmKV.Value.Col)))
                Next
            Loop
        Catch ex As Exception

            WorkerMsg(tMsgID.Err, "Error during file input! Line number " & iMapDim + 1 & " (" & sFilePath & ")", MsgSrc, sFilePath)
            GoTo lbEr

        End Try

        'Shep-Init
        MapIntp = New cMapInterpol(Me)

        'Close file
        file.Close()

        file = Nothing
        EmKV = Nothing
        Em0 = Nothing

        Return True


        'ERROR-label for clean Abort
lbEr:
        file.Close()
        file = Nothing
        EmKV = Nothing
        Em0 = Nothing

        Return False

    End Function

    Public Sub Norm()
        Dim i As Integer
        Dim Em0 As cEmComp

        Dim nleerl As Single
        Dim nnenn As Single

        Dim MsgSrc As String

        MsgSrc = "MAP/Norm"

        nleerl = VEH.nLeerl
        nnenn = VEH.nNenn

        'FC Delauney
        Em0 = EmDefRef(tMapComp.FC)
        For i = 0 To iMapDim
            FuelMap.AddPoints(LnU(i), LPe(i), Em0.RawVals(i))
        Next

        FuelMap.Triangulate()

    End Sub


    Public Function fFCdelaunay_Intp(ByVal nU As Single, ByVal Pe As Single) As Single
        Try
            Return FuelMap.Intpol(nU, Pe)
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot extrapolate FC map! n= " & nU.ToString("0") & " [1/min], Me= " & nPeToM(nU, Pe).ToString("0.0") & " [Nm]", "MAP/FC_Intp")
            Return -10000
        End Try
    End Function

   'Default Shepard in intpshep ()
    Private Class cMapInterpol

        Private iMapDim As Integer
        Private Pe0 As Single
        Private n0 As Single
        Private myMap As cMAP
        Private PeDrag As Single

        'Interpolator V1
        Private abOK As Boolean()                                   'Array für alle Punkte (iMapDim)
        Private ab As Double()                                      'Array für ausgewählte Punkte (inim)
        Private wisum As Double
        Private PeIntp As Single

        'Interpolator V2
        Private abOKV2 As Boolean()                                   'Array für alle Punkte (iMapDim)
        Private abV2 As Double()                                      'Array für ausgewählte Punkte (inim)
        Private wisumV2 As Double
        Private PeIntpV2 As Single

        Public Sub New(ByRef MapClass As cMAP)
            myMap = MapClass
            iMapDim = MapClass.iMapDim
        End Sub

        Public Sub CleanUp()
            myMap = Nothing
        End Sub

    
    End Class




#Region "Properties"

    Public Property FilePath() As String
        Get
            Return sFilePath
        End Get
        Set(ByVal value As String)
            sFilePath = value
        End Set
    End Property

    Public ReadOnly Property EmList As List(Of String)
        Get
            Return MyEmList
        End Get
    End Property


#End Region




End Class








