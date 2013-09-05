Public Class cEmComp

    Public Name As String

    'IDstring:
    '   Def-Komponente: IDstring = sKey (upper case)
    '   Cstm-Komponente (tMapComp.Undefined): IDstring = Name (upper case)
    Public IDstring As String

    Public Unit As String
    Public IntpolV2 As Boolean
    Public PeCorMode As tIntpPeCorMode
    Public NormID As tEmNorm
    Public MapCompID As tMapComp = tMapComp.Undefined
    Public Col As Integer
    Public RawVals As System.Collections.Generic.List(Of Single)    'RawVals darf man extern verändern aber nicht mit "New" neu definieren weil sonst stimmt FinalVals-Referenz nicht mehr
    Public TCVals As System.Collections.Generic.List(Of Single)     'Werte nach Dyn-Kor
    Public ATVals As System.Collections.Generic.List(Of Single)     'Werte nach EXS
    Public FinalVals As System.Collections.Generic.List(Of Single)  'Verweist immer auf das letzte Feld (Tailpipe) 

    'Transient Correction
    Private bTCdef As Boolean
    Public TCfactors As System.Collections.Generic.Dictionary(Of tMapComp, Double)

    'Component is dumped every one second
    Public WriteOutput As Boolean

    'Define ATVals (EXS)
    Private bATdef As Boolean

    'Totals/Average Values
    Public FinalAvg As Single       'Durchschnittswert [g/h]
    Private FinalSum As Single       'Summe [g]
    Private FinalAvgPos As Single    'Durchschnittswert ohne negative Werte [g/h]
    Private FinalSumPos As Single    'Summe ohne negative Werte [g]

    Public Sub New()
        RawVals = New System.Collections.Generic.List(Of Single)
        FinalVals = RawVals
        bTCdef = False
        bATdef = False
        IntpolV2 = False                       'WICHTIG: Beim Einlesen von Map ist das hier der Standard für Nicht-Default-Komponenten!
        PeCorMode = tIntpPeCorMode.PeCorOff    'WICHTIG: Beim Einlesen von Map ist das hier der Standard für Nicht-Default-Komponenten!
        NormID = tEmNorm.x
        WriteOutput = True
    End Sub

    Public Sub InitTC()
        bTCdef = True
        TCfactors = New System.Collections.Generic.Dictionary(Of tMapComp, Double)
        TCfactors.Add(tMapComp.TCdP2s, 0)
        TCfactors.Add(tMapComp.TCPneg3s, 0)
        TCfactors.Add(tMapComp.TCPpos3s, 0)
        TCfactors.Add(tMapComp.TCAmpl3s, 0)
        TCfactors.Add(tMapComp.TCLW3p3s, 0)
        TCfactors.Add(tMapComp.TCP40sABS, 0)
        TCfactors.Add(tMapComp.TCabsdn2s, 0)
        TCfactors.Add(tMapComp.TCP10sn10s3, 0)
        TCfactors.Add(tMapComp.TCdynV, 0)
        TCfactors.Add(tMapComp.TCdynAV, 0)
        TCfactors.Add(tMapComp.TCdynDAV, 0)
    End Sub

    Public Sub InitAT()
        bATdef = True
    End Sub

    Public ReadOnly Property RawUnit As String
        Get
            Dim txt As String

            txt = Me.Unit
            txt = txt.Replace("[", "")
            txt = txt.Replace("]", "")

            If NormID = tEmNorm.x OrElse Not txt.Contains("/") Then
                Return txt
            Else
                Return Left(txt, txt.IndexOf("/"))
            End If

        End Get
    End Property

    Public ReadOnly Property TCdef() As Boolean
        Get
            Return bTCdef
        End Get
    End Property

    Public ReadOnly Property ATdef() As Boolean
        Get
            Return bATdef
        End Get
    End Property

    Public Sub SumCalc()
        Dim x As Single
        Dim sum As Double
        Dim sumPos As Double

        sum = 0
        sumPos = 0
        For Each x In FinalVals
            sum += x
            sumPos += Math.Max(x, 0)
        Next

        'Averaged
        FinalAvg = CSng(sum / FinalVals.Count)
        FinalAvgPos = CSng(sumPos / FinalVals.Count)

        'Total (g/h converted into g)
        Select Case NormID
            Case tEmNorm.x
                FinalSum = sum
                FinalSumPos = sumPos
            Case Else 'tEmNorm.x_h, tEmNorm.x_hPnenn, tEmNorm.x_kWh
                FinalSum = sum / 3600
                FinalSumPos = sumPos / 3600
        End Select

    End Sub




End Class
