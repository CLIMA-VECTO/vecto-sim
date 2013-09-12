Imports System.Collections.Generic
Imports Newtonsoft.Json


'uses JSON.NET http://json.codeplex.com/

Public Class cJSON
    Public Content As Dictionary(Of String, Object)
    Public ErrorMsg As String

    Public Sub New()
        Content = New Dictionary(Of String, Object)
    End Sub


    Public Function ReadFile(ByVal path As String) As Boolean
        Dim file As Microsoft.VisualBasic.FileIO.TextFieldParser
        Dim str As String


        Content.Clear()

        'check if file exists
        If Not IO.File.Exists(path) Then
            ErrorMsg = "file not found"
            Return False
        End If

        'open file
        Try
            file = New Microsoft.VisualBasic.FileIO.TextFieldParser(path)
        Catch ex As Exception
            ErrorMsg = ex.Message
            Return False
        End Try

        'Check if file is empty
        If file.EndOfData Then
            file.Close()
            ErrorMsg = "file is empty"
            Return False
        End If

        'read file
        str = file.ReadToEnd

        'close file
        file.Close()

        'parse JSON to Dictionary
        Try
            'JSONobj = JsonConvert.DeserializeObject(str)
            Content = JsonConvert.DeserializeObject(str, Content.GetType)
        Catch ex As Exception
            ErrorMsg = ex.Message
            Return False
        End Try

        Return True

    End Function

    Public Function WriteFile(ByVal path As String) As Boolean
        Dim file As System.IO.StreamWriter
        Dim str As String
        Dim First As Boolean = True

        If Content.Count = 0 Then Return False

        Try
            str = Newtonsoft.Json.JsonConvert.SerializeObject(Content, Formatting.Indented)
            file = My.Computer.FileSystem.OpenTextFileWriter(path, False)
        Catch ex As Exception
            Return False
        End Try

        file.Write(str)

        file.Close()

        Return True

    End Function


#Region "old self-made parser"
    Private fullfile As String

    Private Function ReadFileXXX(ByVal path As String) As Boolean
        Dim file As Microsoft.VisualBasic.FileIO.TextFieldParser

        Content.Clear()

        'check if file exists
        If Not IO.File.Exists(path) Then Return False

        'open file
        Try
            file = New Microsoft.VisualBasic.FileIO.TextFieldParser(path)
        Catch ex As Exception
            Return False
        End Try

        'Check if file is empty
        If file.EndOfData Then
            file.Close()
            Return False
        End If

        'read file
        fullfile = file.ReadToEnd

        'close file
        file.Close()

        'trim spaces
        fullfile = fullfile.Trim

        'remove line breaks
        fullfile = fullfile.Replace(vbCrLf, "")

        If Left(fullfile, 1) <> "{" Or Right(fullfile, 1) <> "}" Then Return False

        'parse JSON to Dictionary
        Try
            Content = GetObject()
        Catch ex As Exception
            Return False
        End Try


        Return True

    End Function




    Private Function WriteFileXXX(ByVal path As String) As Boolean
        Dim file As System.IO.StreamWriter
        Dim kv As KeyValuePair(Of String, Object)
        Dim str As New System.Text.StringBuilder
        Dim First As Boolean = True

        If Content.Count = 0 Then Return False

        Try
            str.AppendLine("{")
            For Each kv In Content
                If First Then
                    First = False
                Else
                    str.AppendLine(",")
                End If
                str.Append(GetKeyValString(1, kv))
            Next
            str.AppendLine()
            str.AppendLine("}")
        Catch ex As Exception
            Return False
        End Try

        Try
            file = My.Computer.FileSystem.OpenTextFileWriter(path, False)
        Catch ex As Exception
            Return False
        End Try

        file.Write(str.ToString)

        file.Close()

        Return True

    End Function

    Private Function GetKeyValString(ByVal TabLvl As Integer, ByRef kv As KeyValuePair(Of String, Object)) As String
        Dim str As New System.Text.StringBuilder
        Dim obj As Object
        Dim kv0 As KeyValuePair(Of String, Object)
        Dim First As Boolean

        str.Append(Tabs(TabLvl) & ChrW(34) & kv.Key & ChrW(34) & ": ")

        Select Case kv.Value.GetType

            Case GetType(Dictionary(Of String, Object))

                str.AppendLine("{")

                First = True
                For Each kv0 In kv.Value
                    If First Then
                        First = False
                    Else
                        str.AppendLine(",")
                    End If
                    str.Append(GetKeyValString(TabLvl + 1, kv0))
                Next

                str.AppendLine()
                str.Append(Tabs(TabLvl) & "}")

            Case GetType(List(Of Object))

                str.AppendLine("[")

                First = True
                For Each obj In kv.Value
                    If First Then
                        First = False
                    Else
                        str.AppendLine(",")
                    End If
                    str.Append(Tabs(TabLvl + 1) & GetObjString(TabLvl + 1, obj))
                Next

                str.AppendLine()
                str.Append(Tabs(TabLvl) & "]")

            Case Else

                str.Append(GetObjString(TabLvl + 1, kv.Value))

        End Select

        Return str.ToString

    End Function

    Private Function GetObjString(ByVal TabLvl As Integer, ByRef obj As Object) As String
        Dim kv0 As KeyValuePair(Of String, Object)
        Dim First As Boolean
        Dim str As System.Text.StringBuilder

        If obj Is Nothing Then
            Return "null"
        Else
            Select Case obj.GetType

                Case GetType(Dictionary(Of String, Object))

                    str = New System.Text.StringBuilder
                    str.AppendLine("{")

                    First = True
                    For Each kv0 In obj
                        If First Then
                            First = False
                        Else
                            str.AppendLine(",")
                        End If
                        str.Append(GetKeyValString(TabLvl + 1, kv0))
                    Next

                    str.AppendLine()
                    str.Append(Tabs(TabLvl) & "}")

                    Return str.ToString

                Case GetType(String)

                    Return ChrW(34) & CStr(obj) & ChrW(34)

                Case GetType(Boolean)

                    If CBool(obj) Then
                        Return "true"
                    Else
                        Return "false"
                    End If

                Case Else

                    Return CDbl(obj).ToString

            End Select
        End If


    End Function

    Private Function Tabs(ByVal l As Integer) As String
        Dim i As Integer
        Dim str As String

        str = ""
        For i = 1 To l
            str &= vbTab
        Next

        Return str
    End Function

    Private Function GetObject() As Dictionary(Of String, Object)
        Dim MyDic As Dictionary(Of String, Object)
        Dim key As String
        Dim obj As Object
        Dim i As Integer
        Dim i2 As Integer
        Dim Valstr As String
        Dim ValList As List(Of Object) = Nothing
        Dim ArrayMode As Boolean = False

        'remove {
        fullfile = (Right(fullfile, Len(fullfile) - 1)).Trim

        'new list of key/value pairs
        MyDic = New Dictionary(Of String, Object)


        'loop through key/value pairs
lb10:
        If Left(fullfile, 1) <> ChrW(34) Then
            Throw New Exception
            Return Nothing
        End If

        'get key
        i = fullfile.IndexOf(ChrW(34), 1)
        key = Mid(fullfile, 2, i - 1)
        fullfile = (Right(fullfile, Len(fullfile) - i - 1)).Trim
        fullfile = (Right(fullfile, Len(fullfile) - 1)).Trim

        If key = "" Then
            Throw New Exception
            Return Nothing
        End If

        'get value (object, number, boolean, array)
        If Left(fullfile, 1) = "[" Then
            ArrayMode = True
            fullfile = (Right(fullfile, Len(fullfile) - 1)).Trim
            ValList = New List(Of Object)
        End If

lb20:
        If Left(fullfile, 1) = "{" Then
            obj = GetObject()
        Else
            If Left(fullfile, 1) = ChrW(34) Then
                'string
                i = fullfile.IndexOf(ChrW(34), 1)
                obj = Mid(fullfile, 2, i - 1)
                fullfile = (Right(fullfile, Len(fullfile) - i - 1)).Trim
            Else
                'number/boolean
                i = fullfile.IndexOf(",", 1)
                i2 = fullfile.IndexOf("}", 1)

                If i = -1 Then
                    If i2 = -1 Then
                        Valstr = Right(fullfile, Len(fullfile) - 1)
                        fullfile = ""
                    Else
                        Valstr = Mid(fullfile, 1, i2)
                        fullfile = (Right(fullfile, Len(fullfile) - i2)).Trim
                    End If
                Else
                    If i2 = -1 Or i < i2 Then
                        Valstr = Mid(fullfile, 1, i)
                        fullfile = (Right(fullfile, Len(fullfile) - i)).Trim
                    Else
                        Valstr = Mid(fullfile, 1, i2)
                        fullfile = (Right(fullfile, Len(fullfile) - i2)).Trim
                    End If
                End If

                If IsNumeric(Valstr) Then
                    obj = CDbl(Valstr)
                ElseIf (UCase(Valstr)).Trim = "FALSE" Then
                    obj = False
                ElseIf (UCase(Valstr)).Trim = "TRUE" Then
                    obj = True
                ElseIf (UCase(Valstr)).Trim = "NULL" Then
                    obj = Nothing
                Else
                    Throw New Exception
                    Return Nothing
                End If

            End If
        End If

        If ArrayMode Then
            ValList.Add(obj)
            If Left(fullfile, 1) = "]" Then
                ArrayMode = False
                fullfile = (Right(fullfile, Len(fullfile) - 1)).Trim
                MyDic.Add(key, ValList)
            End If
        Else
            MyDic.Add(key, obj)
        End If

        If Left(fullfile, 1) = "," Then
            fullfile = (Right(fullfile, Len(fullfile) - 1)).Trim
            If ArrayMode Then
                GoTo lb20
            Else
                GoTo lb10
            End If
        End If

        If Left(fullfile, 1) = "}" Then
            fullfile = (Right(fullfile, Len(fullfile) - 1)).Trim
        End If

        Return MyDic


    End Function


#End Region



End Class
