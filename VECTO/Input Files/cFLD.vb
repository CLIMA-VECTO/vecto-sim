Imports System.Collections.Generic

''' <summary>
''' Full load/motoring curve input file
''' </summary>
''' <remarks></remarks>
Public Class cFLD

    ''' <summary>
    ''' Full file path. Needs to be defined via FilePath property before calling ReadFile or SaveFile.
    ''' </summary>
    ''' <remarks></remarks>
    Private sFilePath As String

    ''' <summary>
    ''' List of full load torque values [Nm]
    ''' </summary>
    ''' <remarks></remarks>
    Public LTq As List(Of Single)

    ''' <summary>
    ''' List of motoring torque values [Nm]
    ''' </summary>
    ''' <remarks></remarks>
    Public LTqDrag As List(Of Single)

    ''' <summary>
    ''' List of engine speed values [1/min]
    ''' </summary>
    ''' <remarks></remarks>
    Public LnU As List(Of Single)

    ''' <summary>
    ''' List of PT1 values [s]
    ''' </summary>
    ''' <remarks></remarks>
    Private LPT1 As List(Of Single)

    ''' <summary>
    ''' Last index of lists (items count - 1)
    ''' </summary>
    ''' <remarks></remarks>
    Private iDim As Integer

    ''' <summary>
    ''' Nlo [1/min]. Lowest enging speed with 55% of max. power. Defined in Init.
    ''' </summary>
    ''' <remarks></remarks>
    Public Nlo As Single

    ''' <summary>
    ''' Nhi [1/min]. Highest engine speed with 70% of max. power. Defined in Init.
    ''' </summary>
    ''' <remarks></remarks>
    Public Nhi As Single

    ''' <summary>
    ''' Npref [1/min]. Speed at 51% torque/speed-integral between idling and N95h. Defined in Init.
    ''' </summary>
    ''' <remarks></remarks>
    Public Npref As Single

    ''' <summary>
    ''' N95h [1/min]. Highest engine speed with 95% of max. power. Defined in Init.
    ''' </summary>
    ''' <remarks></remarks>
    Public N95h As Single

    ''' <summary>
    ''' Read file. FilePath must be set before calling. 
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' <remarks></remarks>   
    Public Function ReadFile(Optional ByVal ShowMsg As Boolean = True) As Boolean
        Dim file As cFile_V3
        Dim line As String()
        Dim PT1set As Boolean
        Dim FirstLine As Boolean
        Dim nU As Double
        Dim MsgSrc As String

        MsgSrc = "Main/ReadInp/FLD"

        'Reset
        LTq = Nothing
        LTqDrag = Nothing
        LnU = Nothing
        LPT1 = Nothing
        iDim = -1

        'Stop if there's no file
        If sFilePath = "" OrElse Not IO.File.Exists(sFilePath) Then
            If ShowMsg Then WorkerMsg(tMsgID.Err, "FLD file '" & sFilePath & "' not found!", MsgSrc)
            Return False
        End If

        'Open file
        file = New cFile_V3
        If Not file.OpenRead(sFilePath) Then
            If ShowMsg Then WorkerMsg(tMsgID.Err, "Failed to open file (" & sFilePath & ") !", MsgSrc)
            file = Nothing
            Return False
        End If

        'Initialize Lists
        LTq = New System.Collections.Generic.List(Of Single)
        LTqDrag = New System.Collections.Generic.List(Of Single)
        LnU = New System.Collections.Generic.List(Of Single)
        LPT1 = New System.Collections.Generic.List(Of Single)

        FirstLine = True
        Try

            Do While Not file.EndOfFile

                'Read Line
                line = file.ReadLine

                'VECTO: M => Pe
                nU = CDbl(line(0))

                LnU.Add(nU)
                LTq.Add(CDbl(line(1)))
                LTqDrag.Add(CDbl(line(2)))

                If FirstLine Then
                    PT1set = (UBound(line) > 2)
                    FirstLine = False
                End If

                'If PT1 not defined, use default value (0)
                If PT1set Then
                    LPT1.Add(CSng(line(3)))
                Else
                    LPT1.Add(0)
                End If

                'Line-counter up (was reset in ResetMe)
                iDim += 1


            Loop

        Catch ex As Exception

            If ShowMsg Then WorkerMsg(tMsgID.Err, "Error during file read! Line number: " & iDim + 1 & " (" & sFilePath & ")", MsgSrc, sFilePath)
            GoTo lbEr

        End Try


        'Close file
        file.Close()

        Return True


        'ERROR-label for clean Abort
lbEr:
        file.Close()
        file = Nothing

        Return False

    End Function

    ''' <summary>
    ''' Returns motoring power [kW] for given engine speed
    ''' </summary>
    ''' <param name="nU">engine speed [1/min]</param>
    ''' <returns>motoring power [kW]</returns>
    ''' <remarks></remarks>
    Public Function Pdrag(ByVal nU As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If LnU(0) >= nU Then
            If LnU(0) > nU Then MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While LnU(i) < nU And i < iDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If LnU(i) < nU Then
            MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
        End If

lbInt:
        'Interpolation
        Return nMtoPe(nU, (nU - LnU(i - 1)) * (LTqDrag(i) - LTqDrag(i - 1)) / (LnU(i) - LnU(i - 1)) + LTqDrag(i - 1))

    End Function

    ''' <summary>
    ''' Returns full load power [kW] at given engine speed considering transient torque build-up via PT1.
    ''' </summary>
    ''' <param name="nU">engine speed [1/min]</param>
    ''' <param name="LastPe">engine power at previous time step</param>
    ''' <returns>full load power [kW]</returns>
    ''' <remarks></remarks>
    Public Function Pfull(ByVal nU As Single, ByVal LastPe As Single) As Single
        Dim i As Int32
        Dim PfullStat As Single
        Dim PT1 As Single

        'Extrapolation for x < x(1)
        If LnU(0) >= nU Then
            If LnU(0) > nU Then MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While LnU(i) < nU And i < iDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If LnU(i) < nU Then
            MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
        End If

lbInt:
        'Interpolation
        PfullStat = nMtoPe(nU, (nU - LnU(i - 1)) * (LTq(i) - LTq(i - 1)) / (LnU(i) - LnU(i - 1)) + LTq(i - 1))
        PT1 = (nU - LnU(i - 1)) * (LPT1(i) - LPT1(i - 1)) / (LnU(i) - LnU(i - 1)) + LPT1(i - 1)

        'Dynamic Full-load
        Return Math.Min((1 / (PT1 + 1)) * (PfullStat + PT1 * LastPe), PfullStat)

    End Function

    ''' <summary>
    ''' Returns stationary full load power [kW] at given engine speed.
    ''' </summary>
    ''' <param name="nU">engine speed [1/min]</param>
    ''' <returns>stationary full load power [kW]</returns>
    ''' <remarks></remarks>
    Public Function Pfull(ByVal nU As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If LnU(0) >= nU Then
            If LnU(0) > nU Then MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While LnU(i) < nU And i < iDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If LnU(i) < nU Then
            MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
        End If

lbInt:
        'Interpolation
        Return nMtoPe(nU, (nU - LnU(i - 1)) * (LTq(i) - LTq(i - 1)) / (LnU(i) - LnU(i - 1)) + LTq(i - 1))
    End Function

    ''' <summary>
    ''' Returns stationary full load torque [Nm] at given engine speed.
    ''' </summary>
    ''' <param name="nU">engine speed [1/min]</param>
    ''' <returns>stationary full load torque [Nm]</returns>
    ''' <remarks></remarks>
    Public Function Tq(ByVal nU As Single) As Single
        Dim i As Int32

        'Extrapolation for x < x(1)
        If LnU(0) >= nU Then
            If LnU(0) > nU Then MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
            i = 1
            GoTo lbInt
        End If

        i = 0
        Do While LnU(i) < nU And i < iDim
            i += 1
        Loop

        'Extrapolation for x > x(imax)
        If LnU(i) < nU Then
            MODdata.ModErrors.FLDextrapol = "n= " & nU & " [1/min]"
        End If

lbInt:
        'Interpolation
        Return (nU - LnU(i - 1)) * (LTq(i) - LTq(i - 1)) / (LnU(i) - LnU(i - 1)) + LTq(i - 1)
    End Function

    ''' <summary>
    ''' Calculates and returns Npref [1/min]. Speed at 51% torque/speed-integral between idling and N95h. Defined in Init.
    ''' </summary>
    ''' <returns>Npref [1/min]</returns>
    ''' <remarks></remarks>
    Public Function fNpref(ByVal Nidle As Single) As Single
        Dim i As Integer
        Dim Amax As Single
        Dim N95h As Single
        Dim n As Single
        Dim T0 As Single
        Dim dn As Single
        Dim A As Single
        Dim k As Single


        dn = 0.001

        N95h = fnUofPfull(0.95 * Pfull(fnUrated), False)

        Amax = Area(Nidle, N95h)

        For i = 0 To iDim - 1

            If Area(Nidle, LnU(i + 1)) > 0.51 * Amax Then

                n = LnU(i)
                T0 = LTq(i)
                A = Area(Nidle, n)

                k = (LTq(i + 1) - LTq(i)) / (LnU(i + 1) - LnU(i))

                Do While A < 0.51 * Amax
                    n += dn
                    A += dn * (2 * T0 + k * dn) / 2
                Loop

                Exit For

            End If

        Next

        Return n

    End Function

    ''' <summary>
    ''' Calculates torque/speed-integral between two engine speed limits. Used for Npref.
    ''' </summary>
    ''' <param name="nFrom">lower engine speed limit [1/min]</param>
    ''' <param name="nTo">upper engine speed limit [1/min]</param>
    ''' <returns>torque/speed-integral between nFrom and nTo [Nm/min]</returns>
    ''' <remarks></remarks>
    Private Function Area(ByVal nFrom As Single, ByVal nTo As Single) As Single
        Dim A As Single
        Dim i As Integer


        A = 0
        For i = 1 To iDim

            If LnU(i - 1) >= nTo Then Exit For

            If LnU(i - 1) >= nFrom Then


                If LnU(i) <= nTo Then

                    'Add full segment
                    A += (LnU(i) - LnU(i - 1)) * (LTq(i) + LTq(i - 1)) / 2

                Else

                    'Add segment till nTo
                    A += (nTo - LnU(i - 1)) * (Tq(nTo) + LTq(i - 1)) / 2

                End If

            Else

                If LnU(i) > nFrom Then

                    'Add segment starting from nFrom
                    A += (LnU(i) - nFrom) * (LTq(i) + Tq(nFrom)) / 2

                End If

            End If

        Next

        Return A


    End Function

    ''' <summary>
    ''' Calculates and returns engine speed at maximum power [1/min]. 
    ''' </summary>
    ''' <returns>engine speed at maximum power [1/min]</returns>
    ''' <remarks></remarks>
    Public Function fnUrated() As Single
        Dim PeMax As Single
        Dim nU As Single
        Dim nUmax As Single
        Dim nUrated As Single
        Dim dnU As Single
        Dim P As Single

        dnU = 1
        PeMax = 0
        nU = LnU(0)
        nUmax = LnU(iDim)
        nUrated = nU

        Do
            P = nMtoPe(nU, Tq(nU))
            If P > PeMax Then
                PeMax = P
                nUrated = nU
            End If
            nU += dnU
        Loop Until nU > nUmax

        Return nUrated

    End Function

    ''' <summary>
    ''' Calculates and returns lowest or highest engine speed at given full load power [1/min]. 
    ''' </summary>
    ''' <param name="PeTarget">full load power [kW]</param>
    ''' <param name="FromLeft">True= lowest engine speed; False= highest engine speed</param>
    ''' <returns>lowest or highest engine speed at given full load power [1/min]</returns>
    ''' <remarks></remarks>
    Public Function fnUofPfull(ByVal PeTarget As Single, ByVal FromLeft As Boolean) As Single
        Dim Pe As Single
        Dim LastPe As Single
        Dim nU As Single
        Dim nUmin As Single
        Dim nUmax As Single
        Dim nUtarget As Single
        Dim dnU As Single

        dnU = 1
        nUmin = LnU(0)
        nUmax = LnU(iDim)

        If FromLeft Then

            nU = nUmin
            LastPe = nMtoPe(nU, Tq(nU))
            nUtarget = nU

            Do
                Pe = nMtoPe(nU, Tq(nU))

                If Pe > PeTarget Then
                    If Math.Abs(LastPe - PeTarget) < Math.Abs(Pe - PeTarget) Then
                        Return nU - dnU
                    Else
                        Return nU
                    End If
                End If

                LastPe = Pe
                nU += dnU
            Loop Until nU > nUmax

        Else

            nU = nUmax
            LastPe = nMtoPe(nU, Tq(nU))
            nUtarget = nU

            Do
                Pe = nMtoPe(nU, Tq(nU))

                If Pe > PeTarget Then
                    If Math.Abs(LastPe - PeTarget) < Math.Abs(Pe - PeTarget) Then
                        Return nU + dnU
                    Else
                        Return nU
                    End If
                End If

                LastPe = Pe
                nU -= dnU
            Loop Until nU < nUmin

        End If

        Return nUtarget

    End Function

    ''' <summary>
    ''' Calculates and returns maximum torque [Nm]. 
    ''' </summary>
    ''' <returns>maximum torque [Nm]</returns>
    ''' <remarks></remarks>
    Public Function Tmax() As Single
        Dim i As Int16
        Dim Tm As Single

        Tm = LTq(0)
        For i = 1 To iDim
            If LTq(i) > Tm Then Tm = LTq(i)
        Next

        Return Tm

    End Function

    Public Sub Init(ByVal Nidle As Single)
        Dim Pmax As Single

        Pmax = Pfull(fnUrated)

        Nlo = fnUofPfull(0.55 * Pmax, True)

        N95h = fnUofPfull(0.95 * Pmax, False)

        Npref = fNpref(Nidle)

        Nhi = fnUofPfull(0.7 * Pmax, False)

    End Sub

    Public Sub DeclInit()
        Dim i As Integer

        For i = 0 To iDim
            LPT1(i) = Declaration.PT1(LnU(i))
        Next

    End Sub

    ''' <summary>
    ''' Get or set Filepath before calling ReadFile
    ''' </summary>
    ''' <value></value>
    ''' <returns>Full filepath</returns>
    ''' <remarks></remarks>
    Public Property FilePath() As String
        Get
            Return sFilePath
        End Get
        Set(ByVal value As String)
            sFilePath = value
        End Set
    End Property




End Class
