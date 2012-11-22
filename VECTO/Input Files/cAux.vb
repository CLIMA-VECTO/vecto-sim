Public Class cAux

    Public Filepath As String

    Public TransRatio As Single
    Public EffToEng As Single
    Public EffToSply As Single
    Private EffMap As cDelaunayMap

    Public Sub New()
        EffMap = New cDelaunayMap
    End Sub

    Public Function Readfile() As Boolean
        Dim MsgSrc As String
        Dim file As cFile_V3
        Dim line As String()

        MsgSrc = "Main/ReadInp/Aux"

        'Abbruch wenn's Datei nicht gibt
        If Filepath = "" OrElse Not IO.File.Exists(Filepath) Then
            WorkerMsg(tMsgID.Err, "Aux file not found (" & Filepath & ") !", MsgSrc)
            Return False
        End If

        'Datei öffnen
        file = New cFile_V3
        If Not file.OpenRead(Filepath) Then
            file = Nothing
            WorkerMsg(tMsgID.Err, "Failed to open file (" & Filepath & ") !", MsgSrc)
            Return False
        End If

        'Kennfeld zurück setzen
        EffMap = New cDelaunayMap

        If file.EndOfFile Then GoTo lbFileEndErr

        Try

            'Transmission ration to engine rpm [-]
            If file.EndOfFile Then GoTo lbFileEndErr
            line = file.ReadLine
            TransRatio = CSng(line(0))

            'Efficiency to engine [-]
            If file.EndOfFile Then GoTo lbFileEndErr
            line = file.ReadLine
            EffToEng = CSng(line(0))

            'Efficiency auxiliary to supply [-]
            If file.EndOfFile Then GoTo lbFileEndErr
            line = file.ReadLine
            EffToSply = CSng(line(0))

            'Efficiency Map
            If file.EndOfFile Then GoTo lbFileEndErr

            'Spalte 1 = Auxiliary speed [rpm]   => X-Achse
            'Spalte 2 = Mechanical power [kW]   => Z-Achse (!)
            'Spalte 3 = Output power [kW]       => Y-Achse (!)

            Do While Not file.EndOfFile
                line = file.ReadLine
                EffMap.AddPoints(CDbl(line(0)), CDbl(line(2)), CDbl(line(1)))
            Loop

            If Not EffMap.Triangulate Then
                WorkerMsg(tMsgID.Err, "Aux Map is invalid! (Triangulation Error)", MsgSrc)
                GoTo lbErr
            End If

        Catch ex As Exception

            WorkerMsg(tMsgID.Err, "Error while reading aux file! (" & ex.Message & ")", MsgSrc)
            GoTo lbErr

        End Try

        file.Close()

        Return True


lbFileEndErr:

        WorkerMsg(tMsgID.Err, "Unexpected end of file (aux)!", MsgSrc)


lbErr:
        file.Close()
        Return False



    End Function


    Public Function Paux(ByVal nU As Single, ByVal Psupply As Single) As Single

        Dim nUaux As Single
        Dim PsplyAux As Single
        Dim PauxEff As Single

        nUaux = nU * TransRatio
        PsplyAux = Psupply / EffToSply

        Try
            PauxEff = EffMap.Intpol(nUaux, PsplyAux)
        Catch ex As Exception
            MODdata.ModErrors.AuxMapExtr = fFILE(Filepath, False) & ", U= " & nUaux & ", PsupplyAux= " & PsplyAux
            Return 0
        End Try

        Return PauxEff / EffToEng

    End Function





End Class
