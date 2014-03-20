Imports System.Collections.Generic

Module VECTO_Global

    Public Const VECTOvers As String = "1.4.RC8+"
    Public Const LicSigAppCode As String = "VECTO-Release-0093C61E0A2E4BFA9A7ED7E729C56AE4"
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

    Public Const izykt As Integer = 40000

    Public sKey As csKey

    'File format
    Public FileFormat As System.Text.Encoding = System.Text.Encoding.UTF8

    Public LOGfile As System.IO.StreamWriter

    Public VEC As cVECTO
    Public VEH As cVEH
    Public MAP As cMAP
    Public DRI As cDRI
    Public FLD As List(Of cFLD)
    Public MODdata As cMOD
    Public Lic As vectolic.cLicense
    Public VSUM As cVSUM
    Public DEV As cDEV

    Public ENG As cENG
    Public GBX As cGBX
    Public VRES As cVRES

    Public Declaration As cDeclaration

    Public ProgBarCtrl As cProgBarCtrl

    Public SetCulture As Boolean       'Damit der Backgroundworker das richtige Format verwendet

    Public Function nMtoPe(ByVal nU As Double, ByVal M As Double) As Double
        Return ((nU * 2 * Math.PI / 60) * M / 1000)
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
            Case sKey.DRI.nU
                Return tDriComp.nU
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
            Case sKey.DRI.Alt
                Return tDriComp.Alt
            Case Else
                Return tDriComp.Undefined

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

#Region "Typ > Name Conversion"

    Public Function ConvLoading(ByVal load As tLoading) As String
        Select Case load
            Case tLoading.FullLoaded
                Return "Full Loading"

            Case tLoading.RefLoaded
                Return "Reference Loading"

            Case tLoading.EmptyLoaded
                Return "Empty Loading"

            Case Else ' tLoading.UserDefLoaded
                Return "User-defined Loading"

        End Select
    End Function



    Public Function ConvVehCat(ByVal VehCat As tVehCat) As String
        Select Case VehCat
            Case tVehCat.Citybus
                Return "Citybus"
            Case tVehCat.Coach
                Return "Coach"
            Case tVehCat.InterurbanBus
                Return "InterurbanBus"
            Case tVehCat.RigidTruck
                Return "RigidTruck"
            Case tVehCat.Tractor
                Return "Tractor"
            Case Else ' tVehCat.Undef
                Return "not defined"
        End Select
    End Function

    Public Function ConvVehCat(ByVal VehCat As String) As tVehCat
        Select Case UCase(Trim(VehCat))
            Case "CITYBUS"
                Return tVehCat.Citybus
            Case "COACH"
                Return tVehCat.Coach
            Case "INTERURBANBUS"
                Return tVehCat.InterurbanBus
            Case "RIGIDTRUCK"
                Return tVehCat.RigidTruck
            Case "TRACTOR"
                Return tVehCat.Tractor
            Case Else
                Return tVehCat.Undef
        End Select
    End Function

    Public Function ConvAxleConf(ByVal AxleConf As tAxleConf) As String
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
            Case Else  'tAxleConf.a8x8
                Return "8x8"
        End Select
    End Function

    Public Function ConvAxleConf(ByVal AxleConf As String) As tAxleConf
        Select Case UCase(Trim(AxleConf))
            Case "4X2"
                Return tAxleConf.a4x2
            Case "4X4"
                Return tAxleConf.a4x4
            Case "6X2"
                Return tAxleConf.a6x2
            Case "6X4"
                Return tAxleConf.a6x4
            Case "6X6"
                Return tAxleConf.a6x6
            Case "8X2"
                Return tAxleConf.a8x2
            Case "8X4"
                Return tAxleConf.a8x4
            Case "8X6"
                Return tAxleConf.a8x6
            Case Else '"8X8"
                Return tAxleConf.a8x8
        End Select
    End Function

    Public Function ConvMission(ByVal Mission As tMission) As String
        Select Case Mission
            Case tMission.LongHaul
                Return "LongHaul"
            Case tMission.RegionalDelivery
                Return "RegionalDelivery"
            Case tMission.UrbanDelivery
                Return "UrbanDelivery"
            Case tMission.MunicipalUtility
                Return "MunicipalUtility"
            Case tMission.Construction
                Return "Construction"
            Case tMission.HeavyUrban
                Return "HeavyUrban"
            Case tMission.Urban
                Return "Urban"
            Case tMission.Suburban
                Return "Suburban"
            Case tMission.Interurban
                Return "Interurban"
            Case tMission.Coach
                Return "Coach"
            Case Else
                Return "not defined"
        End Select
    End Function

    Public Function ConvMission(ByVal Mission As String) As tMission
        Select Case Mission
            Case "LongHaul"
                Return tMission.LongHaul
            Case "RegionalDelivery"
                Return tMission.RegionalDelivery
            Case "UrbanDelivery"
                Return tMission.UrbanDelivery
            Case "MunicipalUtility"
                Return tMission.MunicipalUtility
            Case "Construction"
                Return tMission.Construction
            Case "HeavyUrban"
                Return tMission.HeavyUrban
            Case "Urban"
                Return tMission.Urban
            Case "Suburban"
                Return tMission.Suburban
            Case "Interurban"
                Return tMission.Interurban
            Case "Coach"
                Return tMission.Coach
            Case Else
                Return tMission.Undef
        End Select
    End Function



    Public Function CdModeConv(ByVal CdMode As tCdMode) As String
        Select Case CdMode
            Case tCdMode.CdOfBeta
                Return "CdOfBeta"
            Case tCdMode.CdOfV
                Return "CdOfV"
            Case Else  'tCdMode.ConstCd0
                Return "Off"
        End Select
    End Function

    Public Function CdModeConv(ByVal CdMode As String) As tCdMode
        Select Case UCase(Trim(CdMode))
            Case "CDOFBETA"
                Return tCdMode.CdOfBeta
            Case "CDOFV"
                Return tCdMode.CdOfV
            Case Else  '"OFF"
                Return tCdMode.ConstCd0
        End Select
    End Function



    Public Function RtTypeConv(ByVal RtType As tRtType) As String
        Select Case RtType
            Case tRtType.Primary
                Return "Primary"
            Case tRtType.Secondary
                Return "Secondary"
            Case Else 'tRtType.None
                Return "None"
        End Select
    End Function

    Public Function RtTypeConv(ByVal RtType As String) As tRtType
        Select Case UCase(Trim(RtType))
            Case "PRIMARY"
                Return tRtType.Primary
            Case "SECONDARY"
                Return tRtType.Secondary
            Case Else  '"NONE"
                Return tRtType.None
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
    Public DRI As csKeyDRI

    Public WorkDir As String = "<WORKDIR>"
    Public HomePath As String = "<HOME>"
    Public GenPath As String = "<GENPATH>"
    Public DefVehPath As String = "<VEHDIR>"
    Public NoFile As String = "<NOFILE>"
    Public EmptyString As String = "<EMPTYSTRING>"
    Public Break As String = "<//>"

    Public Normed As String = "NORM"

    Public PauxSply As String = "<AUX_"

    Public EngDrag As String = "<DRAG>"

    Public Sub New()
        DRI = New csKeyDRI
    End Sub

    Public Class csKeyDRI
        Public t As String = "<T>"
        Public V As String = "<V>"
        Public Grad As String = "<GRAD>"
        Public Alt As String = "<ALT>"
        Public Gears As String = "<GEAR>"
        Public nU As String = "<N>"
        Public Pe As String = "<PE>"
        Public Padd As String = "<PADD>"
        Public VairVres As String = "<VAIR_RES>"
        Public VairBeta As String = "<VAIR_BETA>"
        Public s As String = "<S>"
        Public StopTime As String = "<STOP>"
        Public Torque As String = "<ME>"
    End Class


End Class



