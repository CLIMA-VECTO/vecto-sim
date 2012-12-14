Public Class cFile_V3

    Private TxtFldParser As Microsoft.VisualBasic.FileIO.TextFieldParser
    Private StrWrter As System.IO.StreamWriter
    Private Mode As FileMode
    Private Path As String
    Private Sepp As String
    Private SkipCom As Boolean
    Private StopE As Boolean
    Private FileOpen As Boolean
    Private PreLine As String()
    Private FileEnd As Boolean

    Public Sub New()
        Me.Reset()
    End Sub

    Private Sub Reset()
        FileOpen = False
        Mode = FileMode.Undefined
        PreLine = Nothing
        FileEnd = False
    End Sub

    Public Function OpenRead(ByVal FileName As String, Optional ByVal Separator As String = ",", Optional ByVal SkipComment As Boolean = True, Optional ByVal StopAtE As Boolean = False) As Boolean
        Me.Reset()
        StopE = StopAtE
        Path = FileName
        Sepp = Separator
        SkipCom = SkipComment
        If Not (Mode = FileMode.Undefined) Then Return False
        If Not IO.File.Exists(Path) Then Return False
        Mode = FileMode.Read
        Try
            TxtFldParser = New Microsoft.VisualBasic.FileIO.TextFieldParser(Path, System.Text.Encoding.Default)
            FileOpen = True
        Catch ex As Exception
            Return False
        End Try
        TxtFldParser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited
        TxtFldParser.Delimiters = New String() {Sepp}

        'If TxtFldParser.EndOfData Then Return False

        Me.ReadLine()
        Return True
    End Function

    Public Function ReadLine() As String()
        Dim line As String()
        Dim line0 As String

        line = PreLine

lb10:
        If TxtFldParser.EndOfData Then

            FileEnd = True

        Else

            PreLine = TxtFldParser.ReadFields
            line0 = UCase(Trim(PreLine(0)))

            If SkipCom Then
                If Left(line0, 1) = "C" And Mid(line0, 2, 1) <> ":" Then
                    If Len(line0) = 1 Or Mid(line0, 2, 1) = " " Or InStr(line0, ".") < 1 Then GoTo lb10
                End If
            End If

            If StopE Then FileEnd = (line0 = "E")

        End If

        Return line

    End Function

    Public Sub Close()
        Select Case Mode
            Case FileMode.Read
                If FileOpen Then TxtFldParser.Close()
                TxtFldParser = Nothing
            Case FileMode.Write
                If FileOpen Then StrWrter.Close()
                StrWrter = Nothing
        End Select
        Me.Reset()
    End Sub

    Public ReadOnly Property EndOfFile() As Boolean
        Get
            Return FileEnd
        End Get
    End Property

    Public Function OpenWrite(ByVal FileName As String, Optional ByVal Separator As String = ",", Optional ByVal AutoFlush As Boolean = False, Optional ByVal Append As Boolean = False) As Boolean
        Me.Reset()
        Path = FileName
        Sepp = Separator
        If Not (Mode = FileMode.Undefined) Then Return False
        Mode = FileMode.Write
        Try
            StrWrter = My.Computer.FileSystem.OpenTextFileWriter(Path, Append, FileFormat)
            FileOpen = True
        Catch ex As Exception
            Return False
        End Try
        StrWrter.AutoFlush = AutoFlush
        Return True
    End Function

    Public Sub WriteLine(ByVal ParamArray x() As Object)
        Dim St As String
        Dim StB As New System.Text.StringBuilder
        Dim Skip As Boolean
        Skip = True
        For Each St In x
            If Skip Then
                StB.Append(St)
                Skip = False
            Else
                StB.Append(Sepp & St)
            End If
        Next
        StrWrter.WriteLine(StB.ToString)
        StB = Nothing
    End Sub
    Public Sub WriteLine(ByVal x As String)
        StrWrter.WriteLine(x)
    End Sub

    Private Enum FileMode
        Undefined
        Read
        Write
    End Enum

End Class
