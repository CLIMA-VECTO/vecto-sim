

Public Class cSubPath

    Private sFullPath As String
    Private sOglPath As String
    Private bDefined As Boolean

    Public Sub New()
        bDefined = False
    End Sub

    Public Sub Init(ByVal ParentDir As String, ByVal Path As String)
        If fFileOrNot(Path) = "" Then
            bDefined = False
        Else
            bDefined = True
            sOglPath = Path
            sFullPath = fFileRepl(Path, ParentDir)
        End If
    End Sub

    Private Function fFileOrNot(ByVal f As String) As String
        If Trim(UCase(f)) = sKey.NoFile Then
            Return ""
        Else
            Return f
        End If
    End Function


    Public Sub Clear()
        bDefined = False
    End Sub

    Public ReadOnly Property FullPath() As String
        Get
            If bDefined Then
                Return sFullPath
            Else
                Return ""
            End If
        End Get
    End Property

    Public ReadOnly Property OriginalPath() As String
        Get
            If bDefined Then
                Return sOglPath
            Else
                Return ""
            End If
        End Get
    End Property

    Public ReadOnly Property PathOrDummy() As String
        Get
            If bDefined Then
                Return sOglPath
            Else
                Return sKey.NoFile
            End If
        End Get
    End Property





End Class
