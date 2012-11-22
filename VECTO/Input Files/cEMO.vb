Imports System.Collections.Generic

Public Class cEMO

    Private sFilePath As String

    Public Pnenn As Single
    Public I_mot As Single

    Private lnU As List(Of Single)
    Private lAntr As List(Of Single)
    Private lGen As List(Of Single)
    Private fldDIM As Integer

    Private EtaKF As cCustomMap

    'TODO...
    Private ULok As Boolean = False
    Private ULfactor As Single = 1.1

    Public Sub CleanUp()
        lnU = Nothing
        lAntr = Nothing
        lGen = Nothing
        EtaKF = Nothing
    End Sub

    Public Function ReadFile() As Boolean
        Dim file As cFile_V3
        Dim line As String()
        Dim FLDdone As Boolean

        lnU = New List(Of Single)
        lAntr = New List(Of Single)
        lGen = New List(Of Single)
        fldDIM = -1

        EtaKF = New cCustomMap

        'Abbruch wenn's Datei nicht gibt
        If sFilePath = "" Then Return False
        If Not IO.File.Exists(sFilePath) Then Return False

        'Datei öffnen
        file = New cFile_V3
        If Not file.OpenRead(sFilePath) Then
            file = Nothing
            Return False
        End If

        'Kennfeld-Konfig
        EtaKF.Init()

        'FLD und MAP einlesen
        FLDdone = False
        Do While Not file.EndOfFile
            line = file.ReadLine

            If FLDdone Then
                EtaKF.Add(line)
            Else
                If Trim(UCase(line(0))) = sKey.HEV.EtaMap Then
                    FLDdone = True
                Else
                    lnU.Add(line(0))
                    lAntr.Add(line(1))
                    lGen.Add(line(2))
                    fldDIM += 1
                End If
            End If

        Loop

        file.Close()

        'Kennfeld normieren
        EtaKF.Norm()

        Return True

    End Function

    'Übergibt aktuell mögliche Antriebsleistung für geg. Drehzahl
    Public Function PeMax(ByVal nU As Single) As Single
        Dim i As Int32

        'Extrapolation für x < x(1)
        If lnU(0) >= nU Then
            If lnU(0) > nU Then MODdata.ModErrors.FLDextrapol = "EMO: nU= " & nU
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While lnU(i) < nU And i < fldDIM
            i += 1
        Loop

        'Extrapolation für x > x(imax)
        If lnU(i) < nU Then
            MODdata.ModErrors.FLDextrapol = "EMO: nU= " & nU
        End If

lbInt:
        'Interpolation
        If ULok Then
            Return ULfactor * ((nU - lnU(i - 1)) * (lAntr(i) - lAntr(i - 1)) / (lnU(i) - lnU(i - 1)) + lAntr(i - 1))
        Else
            Return (nU - lnU(i - 1)) * (lAntr(i) - lAntr(i - 1)) / (lnU(i) - lnU(i - 1)) + lAntr(i - 1)
        End If

    End Function

    'Übergibt aktuell mögliche Generatorleistung für geg. Drehzahl
    Public Function PeMin(ByVal nU As Single) As Single
        Dim i As Int32

        'Extrapolation für x < x(1)
        If lnU(0) >= nU Then
            If lnU(0) > nU Then MODdata.ModErrors.FLDextrapol = "EMO: nU= " & nU
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While lnU(i) < nU And i < fldDIM
            i += 1
        Loop

        'Extrapolation für x > x(imax)
        If lnU(i) < nU Then
            MODdata.ModErrors.FLDextrapol = "EMO: nU= " & nU
        End If

lbInt:
        'Interpolation
        If ULok Then
            Return ULfactor * ((nU - lnU(i - 1)) * (lGen(i) - lGen(i - 1)) / (lnU(i) - lnU(i - 1)) + lGen(i - 1))
        Else
            Return (nU - lnU(i - 1)) * (lGen(i) - lGen(i - 1)) / (lnU(i) - lnU(i - 1)) + lGen(i - 1)
        End If

    End Function

    Public Function PiEM(ByVal nU As Single, ByVal PeEM As Single) As Single
        Dim Eta As Single

        If PeEM = 0 Then Return 0

        Eta = EtaKF.Intp(nU, PeEM)

        If PeEM > 0 Then
            Return PeEM / Eta
        Else
            Return PeEM * Eta
        End If

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
