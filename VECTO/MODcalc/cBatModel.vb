' Copyright 2014 European Union.
' Licensed under the EUPL (the 'Licence');
'
' * You may not use this work except in compliance with the Licence.
' * You may obtain a copy of the Licence at: http://ec.europa.eu/idabc/eupl
' * Unless required by applicable law or agreed to in writing,
'   software distributed under the Licence is distributed on an "AS IS" basis,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'
' See the LICENSE.txt for the specific language governing permissions and limitations.
Public Class cBatModel

    Private sFilePath As String

    Private a0 As Single, a1 As Single, a2 As Single, a3 As Single, a4 As Single, a5 As Single, a6 As Single
    Private b0 As Single, b1 As Single, b2 As Single, b3 As Single, b4 As Single, b5 As Single, b6 As Single
    Private c0 As Single, c1 As Single, c2 As Single, c3 As Single, c4 As Single
    Private d0 As Single, d1 As Single, d2 As Single
    Private NCELLS As Single
    Private RI_20 As Single
    Private T_AMB As Single
    Private T_MAX As Single
    Private CP As Single
    Private MB As Single


    Public SOC_Kap As Single
    Public SOC_MAX As Single
    Public SOC_MIN As Single

    'Current data (ie the current (last calculated) Time-step)

    Public TempBat As Single
    Public Ubat As Single           'Spannung   [V]     (Array)
    Public Ibat As Single           'Strom      [A]     (Array)
    Public SOC As Single
    Public PbatV As Single       'Verluste in Batterie   [kW]    (Array)


    Private LastTempBat As Single
    Private LastSOC As Single
    Private PmaxEntl As Single              'maximal zulässige Leistung zum Assistieren (Batterie entladen) [kW]    Vorzeichen negativ (Renhart Standard)
    Private PmaxLad As Single               'maximal zulässige Leistung zum Generieren (Batterie laden)     [kW]    Vorzeichen positiv (Renhart Standard)

    Private MyEV As cPower


    Public Sub New(ByRef MycPower As cPower)
        MyEV = MycPower
    End Sub

    Public Sub CleanUp()
        MyEV = Nothing
    End Sub

    Public Function PiBat(ByVal PiEM As Single) As Single
        'TODO...

        Return PiEM / 0.9



    End Function

    'Maximum allowable Power for driving (Battery discharged) [kW]    positive sign (PHEM Standard)
    Public ReadOnly Property PmaxAntr As Single
        Get
            Return -PmaxEntl
        End Get
    End Property

    'Maximum allowable power for Generating/Rekuperiren (Battery-charging) [kW]    negative sign (PHEM Standard)
    Public ReadOnly Property PmaxLaden As Single
        Get
            Return -PmaxLad
        End Get
    End Property



    '--------------------------------------------------------------------------------------------
    '--------------------------------- ~Renhart Battery model ~ ---------------------------------
    '--------------------------------------------------------------------------------------------
    'Method for initializaztion - it is called once
    Public Function Bat_Init() As Boolean

        Dim BFile As New cFile_V3
        Dim FunkTemp As Single  ' Temp-Funktion für Ri(Temp)
        Dim U0_E As Single      ' Ubatt(SOC(T), Ibatt=0), Entladekurve
        Dim U0_L As Single      ' Ubatt(SOC(T), Ibatt=0), Ladekurve

        'Abort if there's no file
        If sFilePath = "" Then Return False
        If Not IO.File.Exists(sFilePath) Then Return False


        If Not BFile.OpenRead(sFilePath) Then
            Return False
        End If

        'Read the Parameters:
        a0 = BFile.ReadLine(0)
        a1 = BFile.ReadLine(0)
        a2 = BFile.ReadLine(0)
        a3 = BFile.ReadLine(0)
        a4 = BFile.ReadLine(0)
        a5 = BFile.ReadLine(0)
        a6 = BFile.ReadLine(0)
        b0 = BFile.ReadLine(0)
        b1 = BFile.ReadLine(0)
        b2 = BFile.ReadLine(0)
        b3 = BFile.ReadLine(0)
        b4 = BFile.ReadLine(0)
        b5 = BFile.ReadLine(0)
        b6 = BFile.ReadLine(0)
        c0 = BFile.ReadLine(0)
        c1 = BFile.ReadLine(0)
        c2 = BFile.ReadLine(0)
        c3 = BFile.ReadLine(0)
        c4 = BFile.ReadLine(0)
        d0 = BFile.ReadLine(0)
        d1 = BFile.ReadLine(0)
        d2 = BFile.ReadLine(0)
        SOC_MIN = BFile.ReadLine(0)     '0..1       SOC_MIN = SOCzulu
        SOC_MAX = BFile.ReadLine(0)     '0..1       SOC_MAX = SOCzulo
        SOC_Kap = BFile.ReadLine(0)     '[Ah]
        NCELLS = BFile.ReadLine(0)      'Anzahl der NiMH-Zellen
        RI_20 = BFile.ReadLine(0)       'RI_20 = RiOhm (abzuklären)
        T_AMB = BFile.ReadLine(0)
        T_MAX = BFile.ReadLine(0)
        CP = BFile.ReadLine(0)
        MB = BFile.ReadLine(0)

        BFile.Close()
        BFile = Nothing

        TempBat = T_AMB
        PbatV = 0.0
        SOC = GEN.SOCstart
        LastSOC = MyEV.SOCstart
        ' ----------------------------------------------------------------------------------------
        ' Calculation of the Battery-voltage at TempBat and SOC(0), Discharge curve
        ' ----------------------------------------------------------------------------------------
        FunkTemp = Ftemp(TempBat)
        U0_E = Uent(SOC) - (Uent(1) - Uent(0)) * 1.1 * (1 - FunkTemp)
        Ubat = U0_E
        ' ----------------------------------------------------------------------------------------
        ' Calculation of the Battery-voltage at TempBat and SOC(0), Charging-curve
        ' ----------------------------------------------------------------------------------------
        U0_L = Ulad(SOC) - (Uent(1) - Uent(0)) * 1.1 * (1 - FunkTemp)
        ' ----------------------------------------------------------------------------------------


        Return True

    End Function

    'Method of calculating the allowable power - Invoked second by second
    Public Sub Bat_Pzul(ByVal t As Integer)
        Dim FunkTemp As Single  ' Temp-Funktion für Ri(Temp)
        Dim U0_E As Single      ' Ubatt(SOC(T), Ibatt=0), Entladekurve
        Dim U0_L As Single      ' Ubatt(SOC(T), Ibatt=0), Ladekurve

        If t < 1 Then
            LastTempBat = T_AMB
            LastSOC = MyEV.SOCstart
        Else
            LastTempBat = MyEV.TempBat(t - 1)
            LastSOC = MyEV.SOC(t - 1)
        End If

        FunkTemp = Ftemp(LastTempBat)
        U0_E = Uent(LastSOC) - (Uent(1) - Uent(0.0)) * 1.1 * (1 - FunkTemp)
        PmaxEntl = -U0_E ^ 2 / (4.0 * RiTemp(LastTempBat) * 1000.0)  ' in kW
        U0_L = Ulad(LastSOC) - (Uent(1) - Uent(0.0)) * 1.1 * (1 - FunkTemp)
        PmaxLad = U0_L ^ 2 / (4.0 * RiTemp(LastTempBat) * 1000.0)  ' in kW

        'PmaxEntl = -SOC_Kap * 3600 * (SOC(jz - 1) - SOC_MIN)
        'PmaxLad = SOC_Kap * 3600 * (SOC_MAX - SOC(jz - 1))


    End Sub

    'Method of calculating the Batterie-losses and SOC for the given  Power - Invoked second by second
    Public Sub Bat_Calc(ByVal Perf As Single, ByVal t As Integer)
        Dim epsilon As Single = 0.001 ' Abfrageschranke

        If t < 1 Then
            LastTempBat = T_AMB
            LastSOC = MyEV.SOCstart
        Else
            LastTempBat = MyEV.TempBat(t - 1)
            LastSOC = MyEV.SOC(t - 1)
        End If

        Perf *= -1                  'Vozeichenwechsel PHEM-Standard => Renhart-Standard

        Select Case Perf
            Case Is < -epsilon      ' Assistieren (Entladen) der Batterie
                ' MsgBox("negativ" + Str(Perf))
                Bat_Ent(Perf)
            Case Is > epsilon       ' Generieren (Laden) der Batterie
                'MsgBox("positiv" + Str(Perf))
                Bat_Lad(Perf)
            Case Else               ' Leistung ist zu klein, alles bleibt gleich
                'MsgBox(Str(jz) + ": Betrag von " + Str(Perf) + " ist kleiner gleich " + Str(epsilon))
                Bat_Nix()
        End Select


        'Input:
        '   Perf ... required Power  Condition: PgMAX < Perf < PaMAX [kW]
        '   all Paramers were determined/read in Bat_Init
        '   jz ...Current time-step
        '   All arrays from Time-step 1 to jz-1

        'Output:
        '   SOC(jz)
        '   Ubat(jz)
        '   Ibat(jz)
        '   PbatV(jz)

        '-------------TEST-------------
        'PbatV(jz) = 0.2 * Math.Abs(Perf)
        'SOC(jz) = SOC(jz - 1) + Perf / 5000
        'Ubat(jz) = (SOC(jz) - SOC_MIN) / (SOC_MAX - SOC_MIN) * 80 + 120
        'Ibat(jz) = Perf / Ubat(jz)
        '-------------TEST-------------




    End Sub

    'Returns PeBat for the given  PiBat (sign from PHEM)
    Public Function fPeBat(ByVal PiBat As Single) As Single
        If PiBat < 0 Then
            Return -fPbatLad(-PiBat)
        Else
            Return -fPbatEnt(-PiBat)
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



    '--------------------------------------------------------------------------------------------
    '----------------------------------------- PRIVATE ------------------------------------------
    '--------------------------------------------------------------------------------------------

    'Battery discharged
    Private Sub Bat_Ent(ByVal Perf As Single)
        Dim FunkTemp As Single  ' Temp-Funktion für Ri(Temp)
        Dim U0_E As Single      ' Ubatt(SOC(T), Ibatt=0), Entladekurve
        Dim Ri_T As Single      ' Ri bei Temperatur T, lokal

        'Temperature Function
        FunkTemp = Ftemp(LastTempBat)

        'Determine Ri depending on temperature
        Ri_T = RiTemp(LastTempBat)

        'Voltage determined from SOC and Voltage-curve
        U0_E = Uent(LastSOC) - (Uent(1) - Uent(0)) * 1.1 * (1 - FunkTemp)

        'Current calculation
        Ibat = -U0_E / 2 / Ri_T + Math.Sqrt((U0_E / 2 / Ri_T) ^ 2 + Perf * 1000 / Ri_T)

        'Battery-losses
        PbatV = Ibat ^ 2 * Ri_T / 1000.0

        'Battery-temperature
        TempBat = LastTempBat + PbatV * 1000.0 * 1.0 / CP / MB
        If TempBat >= T_MAX Then
            TempBat = T_MAX
        End If

        'SOC calculation
        SOC = LastSOC + Ibat * 1.0 / (SOC_Kap * 3600)

        'Adjustment for the current time-step
        FunkTemp = Ftemp(TempBat)
        U0_E = Uent(SOC) - (Uent(1) - Uent(0)) * 1.1 * (1 - FunkTemp)
        Ri_T = RiTemp(TempBat)
        Ubat = U0_E + Ibat * Ri_T
    End Sub

    'Charging Battery
    Private Sub Bat_Lad(ByVal Perf As Single)
        Dim FunkTemp As Single  ' Temp-Funktion für Ri(Temp)
        Dim U0_L As Single      ' Ubatt(SOC(T), Ibatt=0), Entladekurve
        Dim Ri_T As Single      ' Ri bei Temperatur T, lokal


        'Temperature-Function
        FunkTemp = Ftemp(LastTempBat)

        'Determine Ri depending on temperature
        Ri_T = RiTemp(LastTempBat)

        'Voltage determined from SOC and Voltage-curve
        U0_L = Ulad(LastSOC) - (Ulad(1) - Ulad(0)) * 1.1 * (1 - FunkTemp)

        'Current calculation
        Ibat = +U0_L / 2 / Ri_T - Math.Sqrt((U0_L / 2 / Ri_T) ^ 2 - Perf * 1000 / Ri_T)

        'Battery-losses
        PbatV = Ibat ^ 2 * Ri_T / 1000.0

        'Battery-temperature
        TempBat = LastTempBat + PbatV * 1000.0 * 1.0 / CP / MB
        If TempBat >= T_MAX Then
            TempBat = T_MAX
        End If

        'SOC  calculation
        SOC = LastSOC + Ibat * 1.0 / (SOC_Kap * 3600)

        'Adjustment for the current time-step
        FunkTemp = Ftemp(TempBat)
        U0_L = Ulad(SOC) - (Ulad(1) - Ulad(0)) * 1.1 * (1 - FunkTemp)
        Ri_T = RiTemp(TempBat)
        Ubat = U0_L + Ibat * Ri_T
    End Sub

    'Battery do nothing
    Private Sub Bat_Nix()
        'Dim SocProz As Single   ' lokal, SOC in Prozent
        'Dim FunkTemp As Single  ' Temp-Funktion für Ri(Temp)
        'Dim U0_L As Single      ' Ubatt(SOC(T), Ibatt=0), Entladekurve
        'Dim Ri_T As Single      ' Ri bei Temperatur T, lokal

        Ibat = 0.0
        PbatV = 0.0
        TempBat = LastTempBat
        SOC = LastSOC
        ' ALT: Ubat(jz) = Ubat(jz - 1)
    End Sub


    'Returns PeBat when invoked(Laden) with PEmot (sign from(nach)Renhart)
    Private Function fPbatLad(ByVal PEmot As Single) As Single
        Dim FunkTemp As Single
        Dim U0_L As Single
        Dim Ri_T As Single
        Dim Ibat0 As Single
        Dim PbatV0 As Single

        Ri_T = RiTemp(LastTempBat)
        FunkTemp = Ftemp(LastTempBat)
        U0_L = Ulad(LastSOC) - (Ulad(1) - Ulad(0)) * 1.1 * (1 - FunkTemp)

        If (PEmot * 1000 / Ri_T) > (U0_L / 2 / Ri_T) ^ 2 Then
            Return 0
        Else
            Ibat0 = U0_L / 2 / Ri_T - Math.Sqrt((U0_L / 2 / Ri_T) ^ 2 - PEmot * 1000 / Ri_T)
            PbatV0 = Ibat0 ^ 2 * Ri_T / 1000.0
            Return PEmot + PbatV0
        End If

    End Function

    'Return PeBat when Unloaded(Entladen) with PEmot (sign  from(nach) Renhart)
    Private Function fPbatEnt(ByVal PEmot As Single) As Single
        Dim FunkTemp As Single
        Dim U0_E As Single
        Dim Ri_T As Single
        Dim Ibat0 As Single
        Dim PbatV0 As Single

        Ri_T = RiTemp(LastTempBat)
        FunkTemp = Ftemp(LastTempBat)
        U0_E = Uent(LastSOC) - (Uent(1) - Uent(0)) * 1.1 * (1 - FunkTemp)

        Ibat0 = -U0_E / 2 / Ri_T + Math.Sqrt((U0_E / 2 / Ri_T) ^ 2 + PEmot * 1000 / Ri_T)
        PbatV0 = Ibat0 ^ 2 * Ri_T / 1000.0

        Return PEmot - PbatV0

    End Function

    Private Function Uent(ByVal Socf As Single) As Single
        Uent = (a0 + a1 * Socf + a2 * Socf ^ 2 + a3 * Socf ^ 3 + a4 * Socf ^ 4 + a5 * Socf ^ 5 + a6 * Socf ^ 6) * NCELLS
    End Function
    Private Function Ulad(ByVal Socf As Single) As Single
        Ulad = (b0 + b1 * Socf + b2 * Socf ^ 2 + b3 * Socf ^ 3 + b4 * Socf ^ 4 + b5 * Socf ^ 5 + b6 * Socf ^ 6) * NCELLS
    End Function
    Private Function Ftemp(ByVal Tempf As Single) As Single
        Ftemp = c0 + c1 * Tempf + c2 * Tempf ^ 2 + c3 * Tempf ^ 3 + c4 * Tempf ^ 4
    End Function
    Private Function RiTemp(ByVal Ri_T As Single) As Single
        RiTemp = RI_20 * (2.0 - (d0 + d1 * Ri_T + d2 * Ri_T ^ 2))
    End Function







End Class
