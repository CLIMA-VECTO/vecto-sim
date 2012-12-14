Public Class cTRS

    Private Const FormatVersion As Integer = 1
    Private FileVersion As Integer

    Private sFilePath As String

    Private Sub ResetMe()
        FileVersion = 0

    End Sub

    Public Function ReadFile() As Boolean
        Dim file As cFile_V3
        Dim line As String()
        Dim txt As String
        Dim s As Integer
        Dim s1 As Integer
        Dim sTC As System.Collections.Generic.Dictionary(Of tMapComp, Integer)
        Dim KV As System.Collections.Generic.KeyValuePair(Of tMapComp, Integer)
        Dim MapC As tMapComp
        Dim TCvals As System.Collections.Generic.Dictionary(Of tMapComp, Double)
        Dim MsgSrc As String
        Dim l As Integer

        MsgSrc = "Main/ReadInp/TRS"


        'Reset
        ResetMe()

        'Stop if there's no file
        If sFilePath = "" OrElse Not IO.File.Exists(sFilePath) Then
            WorkerMsg(tMsgID.Err, "TRS file not found! (" & sFilePath & ")", MsgSrc)
            Return False
        End If

        'Open file
        file = New cFile_V3
        If Not file.OpenRead(sFilePath) Then
            file = Nothing
            WorkerMsg(tMsgID.Err, "Failed to open file (" & sFilePath & ") !", MsgSrc)
            Return False
        End If

        '***
        '*** First Line: Version
        line = file.ReadLine
        txt = Trim(UCase(line(0)))
        If Microsoft.VisualBasic.Left(txt, 1) = "V" Then
            ' "Remove "V" => Number remains
            txt = txt.Replace("V", "")
            If Not IsNumeric(txt) Then
                'If invalid version: Abort
                WorkerMsg(tMsgID.Err, "File Version invalid!", MsgSrc)
                GoTo lbEr
            Else
                'Version set
                FileVersion = CInt(txt)
            End If
        Else
            'If no version information: Old format
            file.Close()
            Return ReadOldFormat()
        End If

        'Version Check: Abort if Input-file-format is newer than PHEM-version
        If FileVersion > FormatVersion Then
            WorkerMsg(tMsgID.Err, "File Version not supported!", MsgSrc)
            GoTo lbEr
        End If



        sTC = New System.Collections.Generic.Dictionary(Of tMapComp, Integer)


        '***
        '*** Second Line: Check which TC-factors exist in any column (from column 1!)
        line = file.ReadLine
        s1 = UBound(line)

        'Abort if less than 2 Columns:
        If s1 < 1 Then
            WorkerMsg(tMsgID.Err, "Format invalid!", MsgSrc)
            GoTo lbEr
        End If

        For s = 1 To s1
            MapC = fMapComp(line(s))
            'Abort if unknown TC-factor
            If MapC = tMapComp.Undefined Then
                WorkerMsg(tMsgID.Err, "Component '" & line(s) & "' is invalid!", MsgSrc)
                GoTo lbEr
            End If
            'Add to Dict
            sTC.Add(MapC, s)
        Next

        '***
        '*** From Line 3: TC-factors for each Em-component
        '   l is for Error-output
        l = 0
        Try

            Do While Not file.EndOfFile

                line = file.ReadLine
                l += 1

                TCvals = New System.Collections.Generic.Dictionary(Of tMapComp, Double)
                For Each KV In sTC
                    TCvals.Add(KV.Key, CDbl(line(KV.Value)))
                Next

                If Not MAP.InitTCcomp(line(0), TCvals) Then
                    WorkerMsg(tMsgID.Err, "Component '" & line(0) & "' not defined in emission map!", MsgSrc)
                    GoTo lbEr
                End If

                TCvals = Nothing

            Loop

        Catch ex As Exception

            WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & l & " (" & sFilePath & ")", MsgSrc)
            GoTo lbEr

        End Try


        file.Close()

        KV = Nothing
        sTC = Nothing
        file = Nothing
        TCvals = Nothing

        Return True

lbEr:
        file.Close()
        KV = Nothing
        sTC = Nothing
        TCvals = Nothing
        file = Nothing


        Return False

    End Function

    Private Function ReadOldFormat() As Boolean
        Dim File As cFile_V3
        Dim line As String() = New String() {""}
        Dim z As Integer
        Dim First As Boolean
        Dim TCvals As System.Collections.Generic.Dictionary(Of tMapComp, Double)
        Dim Em0 As cEmComp
        Dim x As Short

        'Open file
        File = New cFile_V3
        If Not File.OpenRead(sFilePath) Then
            File = Nothing
            Return False
        End If

        First = True

        For x = 0 To 9
            Select Case x
                Case 0
                    Em0 = MAP.EmDefRef(tMapComp.FC)
                Case 1
                    Em0 = MAP.EmDefRef(tMapComp.NOx)
                Case 2
                    Em0 = MAP.EmDefRef(tMapComp.CO)
                Case 3
                    Em0 = MAP.EmDefRef(tMapComp.HC)
                Case 4
                    Em0 = MAP.EmDefRef(tMapComp.PM)
                Case 5
                    Em0 = MAP.EmDefRef(tMapComp.PN)
                Case 6
                    Em0 = MAP.EmDefRef(tMapComp.NO)
                Case 7
                    Em0 = MAP.EmComponents("E3")
                Case 8
                    Em0 = MAP.EmDefRef(tMapComp.Lambda)
                Case Else '9
                    Em0 = MAP.EmDefRef(tMapComp.ExhTemp)
            End Select

            If First Then
                For z = 0 To GEN.eklasse
                    line = File.ReadLine
                Next
                First = False
            Else
                For z = 1 To 10
                    line = File.ReadLine
                Next
            End If

            'Abort if less than 11 Columns:
            If UBound(line) < 10 Then GoTo lbEr

            TCvals = New System.Collections.Generic.Dictionary(Of tMapComp, Double)
            TCvals.Add(tMapComp.TCAmpl3s, CDbl(line(0)))
            TCvals.Add(tMapComp.TCP40sABS, CDbl(line(1)))
            TCvals.Add(tMapComp.TCLW3p3s, CDbl(line(2)))
            TCvals.Add(tMapComp.TCPneg3s, CDbl(line(3)))
            TCvals.Add(tMapComp.TCPpos3s, CDbl(line(4)))
            TCvals.Add(tMapComp.TCabsdn2s, CDbl(line(5)))
            TCvals.Add(tMapComp.TCP10sn10s3, CDbl(line(6)))
            TCvals.Add(tMapComp.TCdP2s, CDbl(line(7)))
            TCvals.Add(tMapComp.TCdynV, CDbl(line(8)))
            TCvals.Add(tMapComp.TCdynAV, CDbl(line(9)))
            TCvals.Add(tMapComp.TCdynDAV, CDbl(line(10)))

            If Not MAP.InitTCcomp(Em0.Name, TCvals) Then GoTo lbEr

            TCvals = Nothing


        Next




        File.Close()
        TCvals = Nothing
        File = Nothing

        Return True

lbEr:
        File.Close()
        TCvals = Nothing
        File = Nothing


        Return False


    End Function



    Public Property FilePath() As String
        Get
            Return sFilePath
        End Get
        Set(ByVal value As String)
            sFilePath = value
        End Set
    End Property


End Class
