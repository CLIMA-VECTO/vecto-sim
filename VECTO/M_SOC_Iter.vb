Imports System.Collections.Generic

Module M_SOC_Iter

    '-----------SOC Neutral Iteration------------
    Public SOCnJa As Boolean
    Public SOCfirst As Boolean
    Private SOCnAbbr As Boolean
    Private SOCnDdelta As Single = 0.001 '<= in Optionen...?
    Private SOChistory As List(Of Double)
    Private SOCdeltaHis As List(Of Double)
    Private SOCnLog As System.IO.StreamWriter
    Private SOCcount As Integer
    Private EmHist As cEmHist
    Private SOCok As Boolean
    '--------------------------------------------

    Private Const MsgSrcSOC As String = "Main/SOCIteration"

    'TODO: Dummies
    Private Vquer As Single
    Private gesVerbr As Single
    Private gesNOx As Single
    Private gesHC As Single
    Private gesCO As Single
    Private gesPM As Single
    Private gesrawNOx As Single
    Private gesEk(5) As Single

    '--------------------------------------------------------------------------------------------
    '-------------------------------- ~SOC - Neutral Iteration~ ---------------------------------
    '--------------------------------------------------------------------------------------------
    Private Sub SOCnLogLine()
        SOCnLog.WriteLine(SOCstart & "," & SOC(MODdata.tDim) & "," & SOC(MODdata.tDim) - SOCstart)
    End Sub

    Public Function SOCnStatus() As Boolean

        ''Dim SOCnMiCheck As Boolean
        ''Dim SOCnMaCheck As Boolean
        Dim x As Int16
        Dim smin As Single
        Dim SOCvzX As Short
      

        If SOCfirst Then
            If Not SOCnInit() Then Return True
        End If

        If Math.Abs(SOCstart - SOC(MODdata.tDim)) < SOCnDdelta Then
            SOCok = True
            If SOCfirst Then
                WorkerMsg(tMsgID.Normal, "No SOC-Start Iteration needed. Delta = " & SOC(MODdata.tDim) - SOCstart, MsgSrcSOC)
            Else
                HistAdd()
                SOChistory.Add(SOCstart)
                SOCdeltaHis.Add(SOC(MODdata.tDim) - SOCstart)
                SOCcount += 1
                WorkerMsg(tMsgID.Normal, "SOC-Start Iteration complete. Delta = " & SOC(MODdata.tDim) - SOCstart & ", SOC-Start = " & SOCstart, MsgSrcSOC)
            End If
            SOCnLogLine()
            If SOCfirst Then
                SOCnLog.WriteLine("No Iteration needed.")
            Else
                SOCnLog.WriteLine("Iteration completed successfully.")
            End If
            HistRegLog()
            SOCnFinalize()
            Return True
        End If

        SOCcount += 1

        If SOCfirst Then
            SOCfirst = False
            SOChistory.Add(SOCstart)
            SOCdeltaHis.Add(SOC(MODdata.tDim) - SOCstart)
            SOCnLogLine()
            WorkerMsg(tMsgID.Normal, "Starting SOC-Start Iteration. Delta = " & SOC(MODdata.tDim) - SOCstart & ", SOC-End = " & SOC(MODdata.tDim), MsgSrcSOC)
            SOCstart = SOC(MODdata.tDim)
            HistAdd()
            'Return
            Return False
        End If

        SOCnLogLine()

        If SOCnAbbr Then
            SOCnAbort()
            Return True
        End If

        HistAdd()
        SOChistory.Add(SOCstart)
        SOCdeltaHis.Add(SOC(MODdata.tDim) - SOCstart)

        'Check whether the Sign of Delta-SOC changes
        SOCvzX = 0

        For x = SOCcount - 1 To 1 Step -1
            If SOCdeltaHis(x) * SOCdeltaHis(x - 1) < 0 Then
                'Sign changes ...
                SOCvzX += 1
                If SOCvzX = 4 Then
                    '...Limit reached => Abort
                    SOCnAbbr = True
                    Exit For
                End If
            End If
        Next

        If SOCnAbbr Then
            SOCnLog.WriteLine("Iteration failed.")
            HistRegLog()
            SOCnLog.WriteLine(" ")
            smin = Math.Abs(SOCdeltaHis(SOCcount - 1))
            SOCstart = SOChistory(SOCcount - 1)
            For x = SOCcount - 2 To 0 Step -1
                If Math.Abs(SOCdeltaHis(x)) < smin Then
                    SOCstart = SOChistory(x)
                    smin = Math.Abs(SOCdeltaHis(x))
                    SOCnAbbr = False
                End If
            Next
            If SOCnAbbr Then
                'If the last iteration was the best (SOCnAbbr = True): Exit
                WorkerMsg(2, "SOC-Start Iteration failed. Last Iteration produced best result: SOC-Start = " & SOCstart, MsgSrcSOC)
                SOCnAbort()
                Return True
            Else
                'If another iteration was better (SOCnAbbr = False): Repeat
                SOCnAbbr = True
                SOCnLog.WriteLine("Repeating Iteration with best result.")
                SOCnLog.WriteLine("SOC-Start,SOC-End,dSOC")
                WorkerMsg(2, "SOC-Start Iteration failed. Repeating Iteration with best result: SOC-Start = " & SOCstart, MsgSrcSOC)
                Return False
            End If
        End If

        WorkerMsg(tMsgID.Normal, "Continuing SOC-Start Iteration. Delta = " & SOC(MODdata.tDim) - SOCstart & ", SOC-End = " & SOC(MODdata.tDim), MsgSrcSOC)

        SOCstart = SOC(MODdata.tDim)

        Return False

    End Function

    Private Sub HistAdd()
        EmHist.FC.Add(gesVerbr / Vquer)
        EmHist.NOx.Add(gesNOx / Vquer)
        EmHist.HC.Add(gesHC / Vquer)
        EmHist.CO.Add(gesCO / Vquer)
        EmHist.PM.Add(gesPM / Vquer)
        EmHist.E1_PN.Add(gesEk(1) / Vquer)
        EmHist.E2_NO.Add(gesEk(2) / Vquer)
        EmHist.E3.Add(gesEk(3) / Vquer)
        EmHist.E4.Add(gesEk(4) / Vquer)
        EmHist.E5.Add(gesEk(5) / Vquer)
    End Sub

    Private Sub HistRegLog()
        Dim str As System.Text.StringBuilder
        Dim i As Integer
        Dim Check As Boolean
        Dim VorzPlus As Boolean
        Dim UseEmOgl As Boolean

        'Check whether LinReg possible: Mind. 2 calculations; Mind. dSOC-1 sign-changes
        Check = SOCcount > 1
        If Check Then
            VorzPlus = (SOCdeltaHis(0) > 0)
            For i = 1 To SOCcount - 1
                If (Not VorzPlus) = (SOCdeltaHis(i) > 0) Then GoTo lb10
            Next
            Check = False
lb10:
        End If

        If Check Then

            EmHist.RegrAll()

            str = New System.Text.StringBuilder

            SOCnLog.WriteLine(" ")
            SOCnLog.WriteLine("*************** Linear Regression ***************")
            SOCnLog.WriteLine("  Em = Em0 + dSOC * b")
            SOCnLog.WriteLine("  R2 = Coefficient of Determination (Pearson's)")
            SOCnLog.WriteLine("  SE = Standard Error")
            SOCnLog.WriteLine(" ")
            SOCnLog.WriteLine("Property,FC,NOx,HC,CO,PM,E1/PN,E2/NO,E3,E4,E5")

            str.Append("Em0,")
            str.Append(EmHist.FC.a & ",")
            str.Append(EmHist.NOx.a & ",")
            str.Append(EmHist.HC.a & ",")
            str.Append(EmHist.CO.a & ",")
            str.Append(EmHist.PM.a & ",")
            str.Append(EmHist.E1_PN.a & ",")
            str.Append(EmHist.E2_NO.a & ",")
            str.Append(EmHist.E3.a & ",")
            str.Append(EmHist.E4.a & ",")
            str.Append(EmHist.E5.a)
            SOCnLog.WriteLine(str.ToString)

            str.Length = 0
            str.Append("b,")
            str.Append(EmHist.FC.b & ",")
            str.Append(EmHist.NOx.b & ",")
            str.Append(EmHist.HC.b & ",")
            str.Append(EmHist.CO.b & ",")
            str.Append(EmHist.PM.b & ",")
            str.Append(EmHist.E1_PN.b & ",")
            str.Append(EmHist.E2_NO.b & ",")
            str.Append(EmHist.E3.b & ",")
            str.Append(EmHist.E4.b & ",")
            str.Append(EmHist.E5.b)
            SOCnLog.WriteLine(str.ToString)

            str.Length = 0
            str.Append("R2,")
            str.Append(EmHist.FC.R2 & ",")
            str.Append(EmHist.NOx.R2 & ",")
            str.Append(EmHist.HC.R2 & ",")
            str.Append(EmHist.CO.R2 & ",")
            str.Append(EmHist.PM.R2 & ",")
            str.Append(EmHist.E1_PN.R2 & ",")
            str.Append(EmHist.E2_NO.R2 & ",")
            str.Append(EmHist.E3.R2 & ",")
            str.Append(EmHist.E4.R2 & ",")
            str.Append(EmHist.E5.R2)
            SOCnLog.WriteLine(str.ToString)

            str.Length = 0
            str.Append("SE,")
            str.Append(EmHist.FC.SE & ",")
            str.Append(EmHist.NOx.SE & ",")
            str.Append(EmHist.HC.SE & ",")
            str.Append(EmHist.CO.SE & ",")
            str.Append(EmHist.PM.SE & ",")
            str.Append(EmHist.E1_PN.SE & ",")
            str.Append(EmHist.E2_NO.SE & ",")
            str.Append(EmHist.E3.SE & ",")
            str.Append(EmHist.E4.SE & ",")
            str.Append(EmHist.E5.SE)
            SOCnLog.WriteLine(str.ToString)

            SOCnLog.WriteLine(" ")
            SOCnLog.WriteLine("Regression Source")
            SOCnLog.WriteLine("dSOC,FC,NOx,HC,CO,PM,E1/PN,E2/NO,E3,E4,E5")
            For i = 0 To SOCcount - 1
                str.Length = 0
                str.Append(SOCdeltaHis(i) & ",")
                str.Append(EmHist.FC.EmHist(i) & ",")
                str.Append(EmHist.NOx.EmHist(i) & ",")
                str.Append(EmHist.HC.EmHist(i) & ",")
                str.Append(EmHist.CO.EmHist(i) & ",")
                str.Append(EmHist.PM.EmHist(i) & ",")
                str.Append(EmHist.E1_PN.EmHist(i) & ",")
                str.Append(EmHist.E2_NO.EmHist(i) & ",")
                str.Append(EmHist.E3.EmHist(i) & ",")
                str.Append(EmHist.E4.EmHist(i) & ",")
                str.Append(EmHist.E5.EmHist(i))
                SOCnLog.WriteLine(str.ToString)
            Next

            SOCnLog.WriteLine("*************************************************")

            str = Nothing

            'Uncorrected Em use if SOC-iteration OK
            UseEmOgl = SOCok

        Else

            'Uncorrected Em used
            UseEmOgl = True

            SOCnLog.WriteLine(" ")
            SOCnLog.WriteLine("Linear Regression not possible.")

        End If

        'Corrected Emissions for Optimizer
        If bOptOn Then
            'If SOC-iteration was successful (or Lin.Reg not possible) then use Emissions from the last (uncorrected) calculation
            If UseEmOgl Then
                OptEMkor(1) = gesVerbr / Vquer
                OptEMkor(2) = gesNOx / Vquer
                OptEMkor(3) = gesHC / Vquer
                OptEMkor(4) = gesCO / Vquer
                OptEMkor(5) = gesPM / Vquer
                OptEMkor(6) = gesEk(1) / Vquer
                OptEMkor(7) = gesEk(2) / Vquer
                OptEMkor(8) = gesEk(3) / Vquer
                OptEMkor(9) = gesEk(4) / Vquer
                OptEMkor(10) = gesEk(5) / Vquer

            Else

                OptEMkor(1) = EmHist.FC.a
                OptEMkor(2) = EmHist.NOx.a
                OptEMkor(3) = EmHist.HC.a
                OptEMkor(4) = EmHist.CO.a
                OptEMkor(5) = EmHist.PM.a
                OptEMkor(6) = EmHist.E1_PN.a
                OptEMkor(7) = EmHist.E2_NO.a
                OptEMkor(8) = EmHist.E3.a
                OptEMkor(9) = EmHist.E4.a
                OptEMkor(10) = EmHist.E5.a

            End If

        End If


    End Sub

    Private Sub SOCnAbort()
        SOCnFinalize()
        WorkerMsg(tMsgID.Err, "SOC-Start Iteration failed.", MsgSrcSOC)     ' Delta = " & SOC(Tzykl - 1) - SOCstart)
    End Sub

    Private Function SOCnInit() As Boolean
        Dim f As String
        EmHist = New cEmHist
        SOCcount = 0
        SOCok = False
        SOChistory = New List(Of Double)()
        SOCdeltaHis = New List(Of Double)()
        SOCnAbbr = False
        f = MODdata.ModOutpName & "_SOCnLog.txt"
        If SOCnLog IsNot Nothing Then SOCnLog.Close()
        Try
            SOCnLog = My.Computer.FileSystem.OpenTextFileWriter(f, False, FileFormat)
            SOCnLog.AutoFlush = True
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Can't write to " & f, MsgSrcSOC)
            Return False
        End Try
        SOCnLog.WriteLine("VECTO " & VECTOvers & "  " & Now.ToString)
        SOCnLog.WriteLine("Logfile for SOC-Start Iteration")
        SOCnLog.WriteLine("Jobfile: " & JobFile)
        SOCnLog.WriteLine("Driving Cycle: " & CurrentCycleFile)
        SOCnLog.WriteLine(" ")
        SOCnLog.WriteLine("SOC-Start,SOC-End,dSOC")

        Return True

    End Function

    Private Sub SOCnFinalize()
        SOCnLog.Close()
        SOCnLog = Nothing
        EmHist = Nothing
        SOChistory = Nothing
        SOCdeltaHis = Nothing
    End Sub

    Private Class cEmHist

        Public FC As EmComp
        Public NOx As EmComp
        Public HC As EmComp
        Public CO As EmComp
        Public PM As EmComp
        Public E1_PN As EmComp
        Public E2_NO As EmComp
        Public E3 As EmComp
        Public E4 As EmComp
        Public E5 As EmComp


        Public Sub New()
            FC = New EmComp
            NOx = New EmComp
            HC = New EmComp
            CO = New EmComp
            PM = New EmComp
            E1_PN = New EmComp
            E2_NO = New EmComp
            E3 = New EmComp
            E4 = New EmComp
            E5 = New EmComp
        End Sub

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
            FC = Nothing
            NOx = Nothing
            HC = Nothing
            CO = Nothing
            PM = Nothing
            E1_PN = Nothing
            E2_NO = Nothing
            E3 = Nothing
            E4 = Nothing
            E5 = Nothing
        End Sub

        Public Sub RegrAll()
            FC.Regress()
            NOx.Regress()
            HC.Regress()
            CO.Regress()
            PM.Regress()
            E1_PN.Regress()
            E2_NO.Regress()
            E3.Regress()
            E4.Regress()
            E5.Regress()
        End Sub

        Public Class EmComp
            Private Hist As System.Collections.Generic.List(Of Single)
            Private Hdim As Integer

            Public R2 As Single
            Public a As Double
            Public b As Double
            Public SE As Double

            Public Sub New()
                Hdim = -1
                a = 0
                b = 0
                R2 = 0
                Hist = New System.Collections.Generic.List(Of Single)
            End Sub

            Protected Overrides Sub Finalize()
                MyBase.Finalize()
                Hist = Nothing
            End Sub

            Public Sub Add(ByVal Val As Single)
                Hist.Add(Val)
                Hdim += 1
            End Sub

            Public Sub Regress()
                Dim y(SOCcount - 1) As Double
                Dim info As cRegression.RegressionProcessInfo
                Dim reg As cRegression
                Dim i As Integer

                For i = 0 To SOCcount - 1
                    y(i) = CDbl(Hist(i))
                Next

                reg = New cRegression

                info = reg.Regress(SOCdeltaHis.ToArray, y)
                R2 = info.PearsonsR ^ 2
                a = info.a
                b = info.b
                SE = info.StandardError

            End Sub

            Public ReadOnly Property EmHist(ByVal i As Integer) As Single
                Get
                    Return Hist(i)
                End Get
            End Property

        End Class




    End Class



End Module
