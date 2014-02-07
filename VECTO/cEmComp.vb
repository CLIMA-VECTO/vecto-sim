Public Class cEmComp

    Public Name As String

    'IDstring:
    '   Def-Komponente: IDstring = sKey (upper case)
    '   Cstm-Komponente (tMapComp.Undefined): IDstring = Name (upper case)
    Public IDstring As String

    Public Unit As String
    Public NormID As tEmNorm
    Public MapCompID As tMapComp = tMapComp.Undefined
    Public Col As Integer
    Public RawVals As System.Collections.Generic.List(Of Single)
    Public FinalVals As System.Collections.Generic.List(Of Single)

    Public WriteOutput As Boolean

    'Totals/Average Values
    Public FinalAvg As Single       'Durchschnittswert [g/h]
    Private FinalSum As Single       'Summe [g]
    Private FinalAvgPos As Single    'Durchschnittswert ohne negative Werte [g/h]
    Private FinalSumPos As Single    'Summe ohne negative Werte [g]

    Public Sub New()
        RawVals = New System.Collections.Generic.List(Of Single)
        FinalVals = RawVals
        NormID = tEmNorm.x
        WriteOutput = True
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
