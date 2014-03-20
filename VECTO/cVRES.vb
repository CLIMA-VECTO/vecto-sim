Imports System.Collections.Generic
Imports iTextSharp.text.pdf
Imports System.IO





Public Class cVRES

    Public FilePath As String

    Public SingleResults As New List(Of cVRESsingle)

    Public Sub New()
        ResetMe()
    End Sub

    Public Sub ResetMe()
        SingleResults.Clear()
    End Sub

    Public Function SetVals(ByVal VREStype As tLoading) As Boolean
        Dim VRESsingleRef As cVRESsingle
        Dim t1 As Integer
        Dim t As Integer
        Dim MsgSrc As String
        Dim Vquer As Single
        Dim sum As Double

        MsgSrc = "SUM/SetVals"

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

        VRESsingleRef = New cVRESsingle

        VRESsingleRef.DescStr = ConvLoading(VREStype)

        VRESsingleRef.Loading = VEH.Loading / 1000
        VRESsingleRef.FCl100km = (100 * MODdata.FCavg / Vquer) / (Cfg.FuelDens * 1000)
        If VRESsingleRef.Loading > 0 Then VRESsingleRef.FCl100tkm = VRESsingleRef.FCl100km / VRESsingleRef.Loading
        VRESsingleRef.CO2gkm = Cfg.CO2perFC * (MODdata.FCavg / Vquer)
        If VRESsingleRef.Loading > 0 Then VRESsingleRef.CO2gtkm = VRESsingleRef.CO2gkm / VRESsingleRef.Loading
        VRESsingleRef.AvgSpeed = Vquer

        VRESsingleRef.FCerror = MODdata.FCerror

        SingleResults.Add(VRESsingleRef)

        Return True

    End Function


    Public Function Output() As Boolean
        Dim file As New cFile_V3
        Dim MsgSrc As String
        Dim x As Integer
        Dim VRESsingleRef As cVRESsingle
        Dim pdfTemplate As String
        Dim pdfout As String
        Dim pdfReader As PdfReader
        Dim pdfStamper As PdfStamper


        MsgSrc = "SUM/Output"

        If Not file.OpenWrite(FilePath & ".vres", vbTab) Then
            WorkerMsg(tMsgID.Err, "Can't write to " & FilePath & ".vres", MsgSrc)
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

        If Declaration.Active Then
            file.WriteLine(vbTab & "Mission: " & vbTab & Declaration.CurrentMission.NameStr)
            file.WriteLine(vbTab & "HDV Class: " & vbTab & Declaration.SegRef.HDVclass)
        Else
            file.WriteLine(vbTab & "Cycle: " & vbTab & fFILE(CurrentCycleFile, False))
        End If

        file.WriteLine(" ")
        file.WriteLine("Results:")
        file.WriteLine(" ")
        file.WriteLine(vbTab & "Efficiency Index (FCrel): " & vbTab & "N/A")
        file.WriteLine(" ")
        file.WriteLine("Single Results:")

        For Each VRESsingleRef In SingleResults
            file.WriteLine(vbTab & VRESsingleRef.DescStr)
            file.WriteLine(vbTab & vbTab & "Loading: " & vbTab & VRESsingleRef.Loading & vbTab & "[t]")
            file.WriteLine(vbTab & vbTab & "Average Speed: " & vbTab & VRESsingleRef.AvgSpeed.ToString & vbTab & "[km/h]")
            file.WriteLine(vbTab & vbTab & "Fuel Consumption" & vbTab & vbTab & "CO2 Emissions")

            If VRESsingleRef.FCerror Then
                file.WriteLine(vbTab & vbTab & "ERROR")
            Else
                file.WriteLine(vbTab & vbTab & VRESsingleRef.FCl100km.ToString("#.0") & vbTab & "[l/100km]" & vbTab & VRESsingleRef.CO2gkm.ToString("#.0") & vbTab & "[g/km]")
                If VRESsingleRef.Loading = 0 Then
                    file.WriteLine(vbTab & vbTab & "-" & vbTab & "[l/100tkm]" & vbTab & "-" & vbTab & "[g/tkm]")
                Else
                    file.WriteLine(vbTab & vbTab & VRESsingleRef.FCl100tkm.ToString("#.0") & vbTab & "[l/100tkm]" & vbTab & VRESsingleRef.CO2gtkm.ToString("#.0") & vbTab & "[g/tkm]")
                End If
            End If


        Next

        file.Close()

        'Add file to signing list                                                           
        Lic.FileSigning.AddFile(FilePath & ".vres")

        'pdf Output
        If Declaration.Active Then

            pdfTemplate = MyDeclPath & "reptemp.pdf"
            pdfout = FilePath & ".pdf"

            Try

                pdfReader = New PdfReader(pdfTemplate)
                pdfStamper = New PdfStamper(pdfReader, New FileStream(pdfout, FileMode.Create))

                Dim pdfFormFields As AcroFields = pdfStamper.AcroFields


                pdfFormFields.SetField("version", VECTOvers)
                pdfFormFields.SetField("date", Now.ToString)
                pdfFormFields.SetField("mission", Declaration.CurrentMission.NameStr)
                pdfFormFields.SetField("HDVclass", Declaration.SegRef.HDVclass)

                VRESsingleRef = SingleResults(1)
                pdfFormFields.SetField("load1", VRESsingleRef.Loading.ToString & " [t]")
                pdfFormFields.SetField("speed1", VRESsingleRef.AvgSpeed.ToString("#.0") & " [km/h]")
                pdfFormFields.SetField("FC11", VRESsingleRef.FCl100km.ToString("#.0") & " [l/100km]")
                pdfFormFields.SetField("FC12", VRESsingleRef.FCl100tkm.ToString("#.0") & " [l/100tkm]")

                pdfFormFields.SetField("CO211", VRESsingleRef.CO2gkm.ToString("#.0") & " [g/km]")
                pdfFormFields.SetField("CO212", VRESsingleRef.CO2gtkm.ToString("#.0") & " [g/tkm]")

                VRESsingleRef = SingleResults(0)
                pdfFormFields.SetField("load2", VRESsingleRef.Loading.ToString & " [t]")
                pdfFormFields.SetField("speed2", VRESsingleRef.AvgSpeed.ToString("#.0") & " [km/h]")
                pdfFormFields.SetField("FC21", VRESsingleRef.FCl100km.ToString("#.0") & " [l/100km]")
                pdfFormFields.SetField("FC22", "- [l/100tkm]")

                pdfFormFields.SetField("CO221", VRESsingleRef.CO2gkm.ToString("#.0") & " [g/km]")
                pdfFormFields.SetField("CO222", "- [g/tkm]")

                VRESsingleRef = SingleResults(2)
                pdfFormFields.SetField("load3", VRESsingleRef.Loading.ToString & " [t]")
                pdfFormFields.SetField("speed3", VRESsingleRef.AvgSpeed.ToString("#.0") & " [km/h]")
                pdfFormFields.SetField("FC31", VRESsingleRef.FCl100km.ToString("#.0") & " [l/100km]")
                pdfFormFields.SetField("FC32", VRESsingleRef.FCl100tkm.ToString("#.0") & " [l/100tkm]")

                pdfFormFields.SetField("CO231", VRESsingleRef.CO2gkm.ToString("#.0") & " [g/km]")
                pdfFormFields.SetField("CO232", VRESsingleRef.CO2gtkm.ToString("#.0") & " [g/tkm]")


                ' flatten the form to remove editting options, set it to false
                ' to leave the form open to subsequent manual edits
                pdfStamper.FormFlattening = True

                ' close the pdf
                pdfStamper.Close()

            Catch ex As Exception

                WorkerMsg(tMsgID.Err, "Failed to write pdf file (" & pdfout & ")!", MsgSrc)

            End Try






        End If

  























        Return True



    End Function

End Class

Public Class cVRESsingle
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
