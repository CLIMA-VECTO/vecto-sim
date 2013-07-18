Imports System.Collections.Generic

Public Class cVSUM

    Public FilePath As String

    Public VehConfig As String
    Public EffIdx As Single

    Public SingleResults As New List(Of cVSUMsingle)



    Public Sub New()
        ResetMe()
    End Sub

    Public Sub ResetMe()
        VehConfig = ""
        EffIdx = 0
        SingleResults.Clear()
    End Sub

    Public Function SetVals(ByVal VSUMtype As tVSUM) As Boolean
        Dim Em0 As cEmComp
        Dim VSUMsingleRef As cVSUMsingle
        Dim t1 As Integer
        Dim t As Integer
        Dim MsgSrc As String
        Dim Vquer As Single
        Dim sum As Double

        MsgSrc = "SUM/SetVals"

        VehConfig = fAxleConfName(VEH.AxcleConf)
        EffIdx = 0

        If Not MODdata.Em.EmDefComp.ContainsKey(tMapComp.FC) Then
            WorkerMsg(tMsgID.Err, sKey.MAP.FC & " not found!", MsgSrc)
            Return False
        End If

        Em0 = MODdata.Em.EmDefComp(tMapComp.FC)
        t1 = MODdata.tDim

        'Average Speed calculation
        sum = 0
        For t = 0 To t1
            sum += MODdata.Vh.V(t)
        Next
        Vquer = 3.6 * sum / (t1 + 1)

        If Vquer = 0 Then
            WorkerMsg(tMsgID.Err, "Average Speed = 0 ?!", MsgSrc)
            Return False
        End If

        VSUMsingleRef = New cVSUMsingle

        Select Case VSUMtype
            Case tVSUM.EmptyLoaded
                VSUMsingleRef.DescStr = "Empty Loading"

            Case tVSUM.FullLoaded
                VSUMsingleRef.DescStr = "Full Loading"

            Case tVSUM.RefLoaded
                VSUMsingleRef.DescStr = "Reference Loading"

            Case Else ' tVSUM.UserDefLoaded
                VSUMsingleRef.DescStr = "User-defined Loading"
        End Select

        VSUMsingleRef.Loading = VEH.Loading / 1000
        VSUMsingleRef.FCl100km = (100 * Em0.FinalAvg / Vquer) / (Cfg.FuelDens * 1000)
        If VSUMsingleRef.Loading > 0 Then VSUMsingleRef.FCl100tkm = VSUMsingleRef.FCl100km / VSUMsingleRef.Loading
        VSUMsingleRef.CO2gkm = Cfg.CO2perFC * (Em0.FinalAvg / Vquer)
        If VSUMsingleRef.Loading > 0 Then VSUMsingleRef.CO2gtkm = VSUMsingleRef.CO2gkm / VSUMsingleRef.Loading
        VSUMsingleRef.AvgSpeed = Vquer

        VSUMsingleRef.FCerror = FCerror

        SingleResults.Add(VSUMsingleRef)

        Return True

    End Function


    Public Function Output() As Boolean
        Dim file As New cFile_V3
        Dim MsgSrc As String
        Dim x As Integer
        Dim VSUMsingleRef As cVSUMsingle


        MsgSrc = "SUM/Output"

        If Not file.OpenWrite(FilePath, vbTab) Then
            WorkerMsg(tMsgID.Err, "Can't write to " & FilePath, MsgSrc)
            Return False
        End If

        file.WriteLine(" _    ________________________ ")
        file.WriteLine("| |  /   ____  ______  __  __ \")
        file.WriteLine("| | / / __/ / /     / / / / / /")
        file.WriteLine("| |/ / /___/ /___  / / / /_/ / ")
        file.WriteLine("|___/_____/\____/ /_/  \____/  ")

        x = CInt((31 - 4 - Len(VECTOvers)) / 2)
        file.WriteLine(Space(x) & "~ " & VECTOvers & " ~" & Space(x))
        file.WriteLine(" ")
        file.WriteLine("Date:" & vbTab & Now.ToString)
        file.WriteLine(" ")
        file.WriteLine("Specifications")
        'TODO: Mission without Cycle-name
        file.WriteLine(vbTab & "Mission: " & vbTab & fFILE(CurrentCycleFile, False))
        file.WriteLine(vbTab & "Vehicle Class: " & vbTab & VehConfig)
        'TODO: Test Setup
        file.WriteLine(vbTab & "Vehicle Setup: " & vbTab & "")
        file.WriteLine(" ")
        file.WriteLine("Results from CO2-Simulator:")
        file.WriteLine(" ")
        file.WriteLine(vbTab & "Efficiency Index (FCrel): " & vbTab & EffIdx.ToString)
        file.WriteLine(" ")
        file.WriteLine("Single Results:")

        For Each VSUMsingleRef In SingleResults
            file.WriteLine(vbTab & VSUMsingleRef.DescStr)
            file.WriteLine(vbTab & vbTab & "Loading: " & vbTab & VSUMsingleRef.Loading & vbTab & "[t]")
            file.WriteLine(vbTab & vbTab & "Average Speed: " & vbTab & VSUMsingleRef.AvgSpeed.ToString & vbTab & "[km/h]")
            file.WriteLine(vbTab & vbTab & "Fuel Consumption" & vbTab & vbTab & "CO2 Emissions")

            If VSUMsingleRef.FCerror Then
                file.WriteLine(vbTab & vbTab & "ERROR")
            Else
                file.WriteLine(vbTab & vbTab & VSUMsingleRef.FCl100km.ToString("#.0") & vbTab & "[l/100km]" & vbTab & VSUMsingleRef.CO2gkm.ToString("#.0") & vbTab & "[g/km]")
                If VSUMsingleRef.Loading = 0 Then
                    file.WriteLine(vbTab & vbTab & "-" & vbTab & "[l/100tkm]" & vbTab & "-" & vbTab & "[g/tkm]")
                Else
                    file.WriteLine(vbTab & vbTab & VSUMsingleRef.FCl100tkm.ToString("#.0") & vbTab & "[l/100tkm]" & vbTab & VSUMsingleRef.CO2gtkm.ToString("#.0") & vbTab & "[g/tkm]")
                End If
            End If


        Next


        file.Close()

        Return True

    End Function

End Class

Public Class cVSUMsingle
    Public Loading As Single
    Public AvgSpeed As Single
    Public FCl100km As Single
    Public FCl100tkm As Single
    Public CO2gkm As Single
    Public CO2gtkm As Single
    Public DescStr As String
    Public FCerror As Boolean

    Public Sub ResetMe()
        FCl100km = 0
        FCl100tkm = 0
        CO2gkm = 0
        CO2gtkm = 0
        DescStr = ""
    End Sub


End Class
