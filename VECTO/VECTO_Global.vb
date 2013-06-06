Imports System.Collections.Generic

Module VECTO_Global

    Public Const VECTOvers As String = "1.3.1.1"
    Public MyAppPath As String
    Public MyConfPath As String
    Public MyDeclPath As String

    'BackgroundWorker
    Public PHEMworker As System.ComponentModel.BackgroundWorker

    'Log/Msg
    Public MSGerror As Integer
    Public MSGwarn As Integer

    'Config-------------------------------------------------------
    Public Cfg As cConfig

    'TODO: Get rid of it! SOC-iteration belongs either in the Power-loop or Em-calculation for LinReg
    Public SOCstart As Single
    Public SOC(izykt) As Single
    Public Const izykt As Integer = 40000

    Public sKey As csKey

    'File format
    Public FileFormat As System.Text.Encoding = System.Text.Encoding.UTF8

    Public LOGfile As System.IO.StreamWriter

    Public GEN As cGEN
    Public VEH As cVEH
    Public MAP As cMAP
    Public DRI As cDRI
    Public FLD As cFLD
    Public MODdata As cMOD
    Public TRS As cTRS
    Public EXS As cEXS
    Public ADV As cADVANCE_V3
    Public Lic As vectolic.cLicense
    Public ERG As cERG
    Public DEV As cDEV

    Public ENG As cENG
    Public GBX As cGBX
    Public VSUM As cVSUM

    Public VEC As cVECTO

    Public ProgBarCtrl As cProgBarCtrl

    Public SetCulture As Boolean       'Damit der Backgroundworker das richtige Format verwendet

    Public Function nMtoPe(ByVal nU As Double, ByVal M As Double) As Double
        Return ((nU * 2 * Math.PI / 60) * M / 1000)
    End Function

    Public Function nnormTonU(ByVal nnorm As Single) As Single
        Return nnorm * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl
    End Function

    Public Function PnormToM(ByVal nnorm As Single, ByVal Pnorm As Double) As Single
        Dim nU As Single
        nU = nnorm * (VEH.nNenn - VEH.nLeerl) + VEH.nLeerl
        Return Pnorm * VEH.Pnenn * 1000 / (nU * 2 * Math.PI / 60)
    End Function

    Public Function nPeToM(ByVal nU As Single, ByVal Pe As Double) As Single
        Return Pe * 1000 / (nU * 2 * Math.PI / 60)
    End Function


#Region "sKey > Typ Umwandlung"

    Public Function fDriComp(ByVal sK As String) As tDriComp
        sK = Trim(UCase(sK))
        Select Case sK
            Case sKey.DRI.t
                Return tDriComp.t
            Case sKey.DRI.V
                Return tDriComp.V
            Case sKey.DRI.Grad
                Return tDriComp.Grad
            Case sKey.DRI.n
                Return tDriComp.nn
            Case sKey.DRI.Gears
                Return tDriComp.Gears
            Case sKey.DRI.Padd
                Return tDriComp.Padd
            Case sKey.DRI.Pe
                Return tDriComp.Pe
            Case sKey.DRI.VairVres
                Return tDriComp.VairVres
            Case sKey.DRI.VairBeta
                Return tDriComp.VairBeta
            Case sKey.DRI.s
                Return tDriComp.s
            Case sKey.DRI.StopTime
                Return tDriComp.StopTime
            Case sKey.DRI.Torque
                Return tDriComp.Torque
            Case Else
                Return tDriComp.Undefined

        End Select
    End Function

    Public Function fExsComp(ByVal sK As String) As tExsComp
        Dim x As Integer
        sK = Trim(UCase(sK))

        x = sK.IndexOf("_")

        If x = -1 Then Return tExsComp.Undefined

        sK = Left(sK, x + 1)

        Select Case sK
            Case sKey.EXS.Tgas
                Return tExsComp.Tgas
            Case Else
                Return tExsComp.Undefined
        End Select

    End Function

    Public Function fAuxComp(ByVal sK As String) As tAuxComp
        Dim x As Integer
        sK = Trim(UCase(sK))

        x = sK.IndexOf("_")

        If x = -1 Then Return tAuxComp.Undefined

        sK = Left(sK, x + 1)

        Select Case sK
            Case sKey.PauxSply
                Return tAuxComp.Psupply
            Case Else
                Return tAuxComp.Undefined
        End Select

    End Function

    Public Function fMapComp(ByVal sK As String) As tMapComp
        sK = Trim(UCase(sK))
        Select Case sK
            Case sKey.MAP.FC
                Return tMapComp.FC
            Case sKey.MAP.NOx
                Return tMapComp.NOx
            Case sKey.MAP.HC
                Return tMapComp.HC
            Case sKey.MAP.CO
                Return tMapComp.CO
            Case sKey.MAP.PM
                Return tMapComp.PM
            Case sKey.MAP.PN
                Return tMapComp.PN
            Case sKey.MAP.NO
                Return tMapComp.NO
            Case sKey.MAP.MassF
                Return tMapComp.MassFlow
            Case sKey.MAP.Lambda
                Return tMapComp.Lambda
            Case sKey.MAP.Temp
                Return tMapComp.ExhTemp
            Case sKey.MAP.Qp_coolant
                Return tMapComp.Qp_coolant
            Case sKey.MAP.dP2s
                Return tMapComp.TCdP2s
            Case sKey.MAP.Pneg3s
                Return tMapComp.TCPneg3s
            Case sKey.MAP.Ppos3s
                Return tMapComp.TCPpos3s
            Case sKey.MAP.Ampl3s
                Return tMapComp.TCAmpl3s
            Case sKey.MAP.LW3p3s
                Return tMapComp.TCLW3p3s
            Case sKey.MAP.P40sABS
                Return tMapComp.TCP40sABS
            Case sKey.MAP.absdn2s
                Return tMapComp.TCabsdn2s
            Case sKey.MAP.P10sn10s3
                Return tMapComp.TCP10sn10s3
            Case sKey.MAP.dynV
                Return tMapComp.TCdynV
            Case sKey.MAP.dynAV
                Return tMapComp.TCdynAV
            Case sKey.MAP.dynDAV
                Return tMapComp.TCdynDAV
            Case sKey.MAP.Extrapol
                Return tMapComp.Extrapol
            Case sKey.MAP.Eta
                Return tMapComp.Eta
            Case Else
                Return tMapComp.Undefined
        End Select
    End Function

    Public Function fFldComp(ByVal sK As String) As tFldComp
        sK = Trim(UCase(sK))
        Select Case sK
            Case sKey.FLD.PeTarget
                Return tFldComp.PeTarget
            Case sKey.FLD.PT1
                Return tFldComp.PT1
            Case Else
                Return tMapComp.Undefined
        End Select
    End Function

    Public Function fMapCompIsTC(ByVal ID As tMapComp) As Boolean
        Select Case ID
            Case tMapComp.TCPpos3s, tMapComp.TCPneg3s, tMapComp.TCAmpl3s, tMapComp.TCdP2s, tMapComp.TCP10sn10s3, tMapComp.TCP40sABS, tMapComp.TCabsdn2s, tMapComp.TCLW3p3s, tMapComp.TCdynV, tMapComp.TCdynAV, tMapComp.TCdynDAV
                Return True
            Case Else
                Return False
        End Select
    End Function


    Public Function fCompSubStr(ByVal sK As String) As String
        Dim x As Integer

        sK = Trim(UCase(sK))

        x = sK.IndexOf("_")

        If x = -1 Then Return ""

        sK = Right(sK, Len(sK) - x - 1)

        x = CShort(sK.IndexOf(">"))

        If x = -1 Then Return ""

        sK = Left(sK, x)

        Return sK

    End Function


#End Region

#Region "Typ > Name Umwandlung"

    Public Function fMapCompName(ByVal ID As tMapComp) As String
        Select Case ID
            Case tMapComp.CO
                Return "CO"
            Case tMapComp.Eta
                Return "Eta"
            Case tMapComp.ExhTemp
                Return "ExhTemp"
            Case tMapComp.Extrapol
                Return "Extrapolation"
            Case tMapComp.FC
                Return "FC"
            Case tMapComp.HC
                Return "HC"
            Case tMapComp.Lambda
                Return "Lambda"
            Case tMapComp.MassFlow
                Return "MassFlow"
            Case tMapComp.NO
                Return "NO"
            Case tMapComp.NOx
                Return "NOx"
            Case tMapComp.PM
                Return "PM"
            Case tMapComp.PN
                Return "PN"
            Case tMapComp.TCPpos3s
                Return "dyn_Ppos3s"
            Case tMapComp.TCPneg3s
                Return "dyn_Pneg3s"
            Case tMapComp.TCAmpl3s
                Return "Ampl3s"
            Case tMapComp.TCdP2s
                Return "dP_2s"
            Case tMapComp.TCP10sn10s3
                Return "P10s_n10s3"
            Case tMapComp.TCP40sABS
                Return "P40sABS"
            Case tMapComp.TCabsdn2s
                Return "abs_dn2s"
            Case tMapComp.TCLW3p3s
                Return "LW3p3s"
            Case tMapComp.TCdynV
                Return "dynV"
            Case tMapComp.TCdynAV
                Return "dynAV"
            Case tMapComp.TCdynDAV
                Return "dynDAV"
            Case tMapComp.Qp_coolant
                Return "Qp_coolant"
            Case Else
                Return "fMapCompName() ERROR"
        End Select
    End Function

    Public Function fPwCorName(ByVal PCmode As tIntpPeCorMode) As String
        Select Case PCmode
            Case tIntpPeCorMode.PeCorEmDrag
                Return "PeCorEmDrag"
            Case tIntpPeCorMode.PeCorNull
                Return "PeCorNull"
            Case tIntpPeCorMode.PeCorNullPmin
                Return "PeCorNullPmin"
            Case Else 'tIntpPeCorMode.PeCorOff
                Return "Off"
        End Select
    End Function

    Public Function fAxleConfName(ByVal AxleConf As tAxleConf) As String
        Select Case AxleConf
            Case tAxleConf.a4x2
                Return "4x2"
            Case tAxleConf.a4x4
                Return "4x4"
            Case tAxleConf.a6x2
                Return "6x2"
            Case tAxleConf.a6x4
                Return "6x4"
            Case tAxleConf.a6x6
                Return "6x6"
            Case tAxleConf.a8x2
                Return "8x2"
            Case tAxleConf.a8x4
                Return "8x4"
            Case tAxleConf.a8x6
                Return "8x6"
            Case Else ' tAxleConf.a8x8
                Return "8x8"
        End Select
    End Function

#End Region


    Public Sub StartLogfile()

        'Log start
        LOGfile = My.Computer.FileSystem.OpenTextFileWriter(MyAppPath & "LOG.txt", True, FileFormat)
        LOGfile.AutoFlush = True

        LOGfile.WriteLine("------------------------------------------------------------------------------------------")
        LOGfile.WriteLine("Starting Session " & Now)
        LOGfile.WriteLine("VECTO " & VECTOvers)

    End Sub

End Module


Public Class csKey
    Public MAP As csKeyMAP
    Public DRI As csKeyDRI
    Public EXS As csKeyEXS
    Public HEV As csKeyHEV
    Public FLD As csKeyFLD

    Public WorkDir As String = "<WORKDIR>"
    Public HomePath As String = "<HOME>"
    Public GenPath As String = "<GENPATH>"
    Public DefVehPath As String = "<VEHDIR>"
    Public NoFile As String = "<NOFILE>"
    Public EmptyString As String = "<EMPTYSTRING>"
    Public Break As String = "<//>"

    Public Normed As String = "NORM"

    Public PauxSply As String = "<AUX_"

    Public Sub New()
        MAP = New csKeyMAP
        DRI = New csKeyDRI
        EXS = New csKeyEXS
        HEV = New csKeyHEV
        FLD = New csKeyFLD
    End Sub

    Public Class csKeyMAP
        Public FC As String = "<FC>"
        Public NOx As String = "<NOX>"
        Public HC As String = "<HC>"
        Public CO As String = "<CO>"
        Public PM As String = "<PM>"
        Public PN As String = "<PN>"
        Public NO As String = "<NO>"
        Public MassF As String = "<MASSFLOW>"
        Public Lambda As String = "<LAMBDA>"
        Public Temp As String = "<TEMP>"
        Public Qp_coolant As String = "<QP_COOLANT>"
        Public dP2s As String = "<DP2S>"
        Public Pneg3s As String = "<PNEG3S>"
        Public Ppos3s As String = "<PPOS3S>"
        Public Ampl3s As String = "<AMPL3S>"
        Public LW3p3s As String = "<LW3P3S>"
        Public P40sABS As String = "<P40SABS>"
        Public absdn2s As String = "<ABSDN2S>"
        Public P10sn10s3 As String = "<P10SN10S3>"
        Public dynV As String = "<DYNV>"
        Public dynAV As String = "<DYNAV>"
        Public dynDAV As String = "<DYNDAV>"
        Public Extrapol As String = "<E>"
        Public Drag As String = "<DRAG>"
        Public Eta As String = "<ETA>"
    End Class

    Public Class csKeyDRI
        Public t As String = "<T>"
        Public V As String = "<V>"
        Public Grad As String = "<GRAD>"
        Public Gears As String = "<GEAR>"
        Public n As String = "<N>"
        Public Pe As String = "<PE>"
        Public Padd As String = "<PADD>"
        Public VairVres As String = "<VAIR_RES>"
        Public VairBeta As String = "<VAIR_BETA>"
        Public s As String = "<S>"
        Public StopTime As String = "<STOP>"
        Public Torque As String = "<ME>"
    End Class

    Public Class csKeyEXS
        Public Tgas As String = "<TGAS_"
    End Class

    Public Class csKeyHEV
        Public EtaMap As String = "<MAP>"
    End Class

    Public Class csKeyFLD
        Public PT1 As String = "<PT1>"
        Public PeTarget As String = "<PETARGET>"
    End Class



End Class



