Imports System.Collections.Generic

Public Class cCycleKin

    Private Const aAccThres As Single = 0.125   '[m/s2]
    Private Const aDecThres As Single = -0.125  '[m/s2]
    Private Const vStopThres As Single = 0.1    '[m/s]

    'Fahrzustände in Sekunden
    Private tStop0 As Integer
    Private tAcc0 As Integer
    Private tDec0 As Integer
    Private tCruise0 As Integer

    'Fahrzustands-Anteile
    Private pStop0 As Single
    Private pAcc0 As Single
    Private pDec0 As Single
    Private pCruise0 As Single

    'Beschl.-Parameter
    Private aAvg0 As Single
    Private aPos0 As Single
    Private aNeg0 As Single
    Private AccNoise0 As Single

    Private MyErgEntries As List(Of cErgEntry)

    Public Sub New()
        MyErgEntries = New List(Of cErgEntry)
        MyErgEntries.Add(New cErgEntry("a", "[m/s^2]"))
        MyErgEntries.Add(New cErgEntry("a_pos", "[m/s^2]"))
        MyErgEntries.Add(New cErgEntry("a_neg", "[m/s^2]"))
        MyErgEntries.Add(New cErgEntry("Acc.Noise", "[m/s^2]"))
        MyErgEntries.Add(New cErgEntry("pAcc", "[%]"))
        MyErgEntries.Add(New cErgEntry("pDec", "[%]"))
        MyErgEntries.Add(New cErgEntry("pCruise", "[%]"))
        MyErgEntries.Add(New cErgEntry("pStop", "[%]"))
    End Sub

    Public Function ValLine() As String
        Dim s As System.Text.StringBuilder
        Dim Sepp As String = ","

        s = New System.Text.StringBuilder

        s.Append(aAvg0)
        s.Append(Sepp & aPos0)
        s.Append(Sepp & aNeg0)
        s.Append(Sepp & AccNoise0)
        s.Append(Sepp & pAcc0)
        s.Append(Sepp & pDec0)
        s.Append(Sepp & pCruise0)
        s.Append(Sepp & pStop0)

        Return s.ToString

    End Function


    Public Sub Calc()
        Dim t As Integer
        Dim t1 As Integer
        Dim a3save() As Single

        t1 = MODdata.tDim

        aAvg0 = 0
        aPos0 = 0
        aNeg0 = 0
        AccNoise0 = 0
        tStop0 = 0
        tAcc0 = 0
        tDec0 = 0
        tCruise0 = 0
        pStop0 = 0
        pAcc0 = 0
        pDec0 = 0
        pCruise0 = 0
        ReDim a3save(t1)

        '3s-Beschl
        a3save(0) = MODdata.Vh.a(0)
        For t = 1 To t1 - 1
            a3save(t) = (MODdata.Vh.a(t - 1) + MODdata.Vh.a(t) + MODdata.Vh.a(t + 1)) * (1.0 / 3.0)
        Next
        a3save(t1) = MODdata.Vh.a(t1)

        't_apos, t_aneg, t_cruise, t_stop
        For t = 0 To t1
            'Fahranteile Stop/Acc/Dec/Cruise
            If MODdata.Vh.V(t) < vStopThres Then
                tStop0 += 1
            Else
                Select Case a3save(t)
                    Case Is > aAccThres
                        tAcc0 += 1
                        aPos0 += a3save(t)
                    Case Is < aDecThres
                        tDec0 += 1
                        aNeg0 += a3save(t)
                    Case Else
                        tCruise0 += 1
                End Select
            End If
            'Durchschnitts-Beschl
            aAvg0 += MODdata.Vh.a(t)
        Next

        'a
        aAvg0 /= (t1 + 1)

        'a-pos
        If tAcc0 > 0 Then aPos0 /= tAcc0

        'a-neg
        If tDec0 > 0 Then aNeg0 /= tDec0

        'Acc.Noise
        For t = 0 To t1
            AccNoise0 += (MODdata.Vh.a(t) - aAvg0) ^ 2
        Next
        AccNoise0 = (AccNoise0 / (t1 + 1)) ^ 0.5

        pStop0 = tStop0 / (t1 + 1)
        pAcc0 = tAcc0 / (t1 + 1)
        pDec0 = tDec0 / (t1 + 1)
        pCruise0 = tCruise0 / (t1 + 1)

    End Sub

    Public ReadOnly Property ErgEntries As List(Of cErgEntry)
        Get
            Return MyErgEntries
        End Get
    End Property

    Public ReadOnly Property GetValueString(ByVal key As String) As String
        Get
            Select Case key
                Case "a"
                    Return aAvg0.ToString
                Case "a_pos"
                    Return aPos0.ToString
                Case "a_neg"
                    Return aNeg0.ToString
                Case "Acc.Noise"
                    Return AccNoise0.ToString
                Case "pAcc"
                    Return pAcc0.ToString
                Case "pDec"
                    Return pDec0.ToString
                Case "pCruise"
                    Return pCruise0.ToString
                Case "pStop"
                    Return pStop0.ToString
                Case Else
                    Return "ERROR - Unknown Key '" & key & "'"
            End Select
        End Get
    End Property



End Class




