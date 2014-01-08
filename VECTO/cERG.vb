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
Imports System.Collections.Generic

Class cERG

    Private Const FormatVersion As String = "1.0"

    Private ERGpath As String
    Private Ferg As System.IO.StreamWriter
    Private HeadInitialized As Boolean

    Private ErgEntries As Dictionary(Of String, cErgEntry)
    Private ErgEntryList As List(Of String)     'Wird benötigt weil Dictionary nicht sortiert ist

    Private ergJSON As cJSON
    Private ResList As List(Of Dictionary(Of String, Object))


    Public Sub New()
        HeadInitialized = False
        ERGpath = ""
    End Sub

    Public Function ErgHead() As String
        Dim s As New System.Text.StringBuilder
        Dim key As String
        Dim First As Boolean

        First = True
        For Each key In ErgEntryList
            If Not First Then s.Append(",")
            s.Append(ErgEntries(key).Head)
            First = False
        Next

        Return s.ToString

    End Function

    Public Function ErgUnits() As String
        Dim s As New System.Text.StringBuilder
        Dim First As Boolean
        Dim key As String

        First = True
        For Each key In ErgEntryList
            If Not First Then s.Append(",")
            s.Append(ErgEntries(key).Unit)
            First = False
        Next

        Return s.ToString

    End Function

    Public Function ErgLine() As String
        Dim ErgEntry As cErgEntry
        Dim s As New System.Text.StringBuilder
        Dim t1 As Integer
        Dim Vquer As Single
        Dim sum As Double
        Dim t As Integer
        Dim c As Integer
        Dim EBatPlus As Single
        Dim EBatMinus As Single
        Dim Em0 As cEmComp
        Dim key As String
        Dim First As Boolean

        For Each ErgEntry In ErgEntries.Values
            ErgEntry.ValueString = Nothing
        Next

        t1 = MODdata.tDim

        'Vehicle type-independent
        ErgEntries("\\T").ValueString = (t1 + 1)
        ErgEntries("\\Prated").ValueString = VEH.Pnenn

        'Length, Speed, Slope
        If Not GEN.VehMode = tVehMode.EngineOnly Then

            'Average-Speed. calculation
            sum = 0
            For t = 0 To t1
                sum += MODdata.Vh.V(t)
            Next
            Vquer = 3.6 * sum / (t1 + 1)

            ErgEntries("\\S").ValueString = (Vquer * (t1 + 1) / 3600)
            ErgEntries("\\V").ValueString = Vquer

            'altitude change
            ErgEntries("\\G").ValueString = MODdata.Vh.AltIntp(Vquer * (t1 + 1) / 3.6) - MODdata.Vh.AltIntp(0)

            'Auxiliary energy consumption
            If GEN.AuxDef Then
                For Each key In GEN.AuxPaths.Keys
                    sum = 0
                    For t = 0 To t1
                        sum += MODdata.Paux(key)(t)
                    Next
                    ErgEntries("\\Eaux_" & UCase(key)).ValueString = sum / 3600
                Next
            End If


        End If


        'EV / Hybrid
        If GEN.VehMode = tVehMode.EV Or GEN.VehMode = tVehMode.HEV Then

            'Positive effective EM-Power
            sum = 0
            c = 0
            For t = 0 To t1
                If MODdata.Px.PeEMot(t) > 0 Then
                    sum += MODdata.Px.PeEMot(t)
                    c += 1
                End If
            Next
            If c > 0 Then ErgEntries("\\PeEM+").ValueString = (sum / c)

            'Positive effective Battery-Power = internal EM-Power
            sum = 0
            c = 0
            For t = 0 To t1
                If MODdata.Px.PeBat(t) > 0 Then
                    sum += MODdata.Px.PeBat(t)
                    c += 1
                End If
            Next
            If c > 0 Then ErgEntries("\\PeBat+").ValueString = (sum / c)

            'Positive internal Battery-Power
            sum = 0
            c = 0
            For t = 0 To t1
                If MODdata.Px.PiBat(t) > 0 Then
                    sum += MODdata.Px.PiBat(t)
                    c += 1
                End If
            Next
            If c > 0 Then ErgEntries("\\PiBat+").ValueString = (sum / c)

            'Calculate Energy consumed
            EBatPlus = sum / 3600

            'Negative effective EM-Power
            sum = 0
            c = 0
            For t = 0 To t1
                If MODdata.Px.PeEMot(t) < 0 Then
                    sum += MODdata.Px.PeEMot(t)
                    c += 1
                End If
            Next
            If c > 0 Then ErgEntries("\\PeEM-").ValueString = (sum / c)

            'Negative effective Battery-Power = internal EM-Power
            sum = 0
            c = 0
            For t = 0 To t1
                If MODdata.Px.PeBat(t) < 0 Then
                    sum += MODdata.Px.PeBat(t)
                    c += 1
                End If
            Next
            If c > 0 Then ErgEntries("\\PeBat-").ValueString = (sum / c)

            'Negative internal Battery-Power
            sum = 0
            c = 0
            For t = 0 To t1
                If MODdata.Px.PiBat(t) < 0 Then
                    sum += MODdata.Px.PiBat(t)
                    c += 1
                End If
            Next
            If c > 0 Then ErgEntries("\\PiBat-").ValueString = (sum / c)

            'Charged-energy calculation
            EBatMinus = sum / 3600

            'Battery in/out Energy
            ErgEntries("\\EiBat+").ValueString = EBatPlus
            ErgEntries("\\EiBat-").ValueString = EBatMinus

            'EtaEM
            sum = 0
            c = 0
            For t = 0 To t1
                If MODdata.Px.PeEMot(t) > 0 Then
                    sum += (MODdata.Px.PeEMot(t)) / MODdata.Px.PiEMot(t)
                    c += 1
                ElseIf MODdata.Px.PeEMot(t) < 0 Then
                    sum += MODdata.Px.PiEMot(t) / (MODdata.Px.PeEMot(t))
                    c += 1
                End If
            Next
            If c > 0 Then ErgEntries("\\EtaEM").ValueString = (sum / c)

            'EtaBat
            sum = 0
            c = 0
            For t = 0 To t1
                If MODdata.Px.PeBat(t) > 0 Then
                    sum += MODdata.Px.PeBat(t) / MODdata.Px.PiBat(t)
                    c += 1
                ElseIf MODdata.Px.PeBat(t) < 0 Then
                    sum += MODdata.Px.PiBat(t) / MODdata.Px.PeBat(t)
                    c += 1
                End If
            Next
            If c > 0 Then ErgEntries("\\EtaBat").ValueString = (sum / c)

            'Delta SOC
            ErgEntries("\\∆SOC").ValueString = (MODdata.Px.SOC(t1) - MODdata.Px.SOC(0))

            'Only EV:
            If GEN.VehMode = tVehMode.EV Then

                'Energy-consumption
                ErgEntries("\\EC").ValueString = ((EBatPlus + EBatMinus) / (Vquer * (t1 + 1) / 3600))

            End If

        End If

        'Conventional means everything with ICE (not EV)
        If GEN.VehMode <> tVehMode.EV Then

            'Emissions
            For Each Em0 In MODdata.Em.EmComp.Values

                'Dump x/h if ADVANCE mode -or- EngineOnly -or- Units not in x/h and therefore Conversion into x/km is not possible
                If Em0.WriteOutput Then

                    If FCerror Then
                        If Em0.NormID = tEmNorm.x Or GEN.VehMode = tVehMode.EngineOnly Then
                            ErgEntries(Em0.IDstring).ValueString = "ERROR"
                        Else
                            ErgEntries(Em0.IDstring & "_km").ValueString = "ERROR"
                        End If
                    Else
                        If Em0.NormID = tEmNorm.x Or GEN.VehMode = tVehMode.EngineOnly Then
                            ErgEntries(Em0.IDstring).ValueString = Em0.FinalAvg
                        Else
                            ErgEntries(Em0.IDstring & "_km").ValueString = (Em0.FinalAvg / Vquer)
                        End If
                    End If

                End If

            Next

            'Power, Revolutions
            'sum = 0
            'For t = 0 To t1
            '    sum += MODdata.Pe(t)
            'Next
            'ErgEntries("\\Pe_norm").ValueString = (sum / (t1 + 1))

            'sum = 0
            'For t = 0 To t1
            '    sum += MODdata.nn(t)
            'Next
            'ErgEntries("\\n_norm").ValueString = (sum / (t1 + 1))

            'Ppos
            sum = 0
            For t = 0 To t1
                sum += Math.Max(0, MODdata.Pe(t))
            Next
            ErgEntries("\\Ppos").ValueString = (sum / (t1 + 1))

            'Pneg
            sum = 0
            For t = 0 To t1
                sum += Math.Min(0, MODdata.Pe(t))
            Next
            ErgEntries("\\Pneg").ValueString = (sum / (t1 + 1))

        End If

        'Nur Gesamtfahrzeug (nicht EngOnly) |@@| Only Entire-vehicle (not EngOnly)
        If Not GEN.VehMode = tVehMode.EngineOnly Then

            'Pbrake-norm
            sum = 0
            For t = 0 To t1
                sum += MODdata.Pbrake(t) / VEH.Pnenn
            Next
            ErgEntries("\\Pbrake").ValueString = (sum / (t1 + 1))

            'Eair
            sum = 0
            For t = 0 To t1
                sum += MODdata.Pluft(t)
            Next
            ErgEntries("\\Eair").ValueString = (-sum / 3600)

            'Eroll
            sum = 0
            For t = 0 To t1
                sum += MODdata.Proll(t)
            Next
            ErgEntries("\\Eroll").ValueString = (-sum / 3600)

            'Egrad
            sum = 0
            For t = 0 To t1
                sum += MODdata.Pstg(t)
            Next
            ErgEntries("\\Egrad").ValueString = (-sum / 3600)

            'Eacc
            sum = 0
            For t = 0 To t1
                sum += MODdata.Pa(t) + MODdata.PaGB(t) + MODdata.PaEng(t)
            Next
            ErgEntries("\\Eacc").ValueString = (-sum / 3600)

            'Eaux
            sum = 0
            For t = 0 To t1
                sum += MODdata.PauxSum(t)
            Next
            ErgEntries("\\Eaux").ValueString = (-sum / 3600)

            'Ebrake
            sum = 0
            For t = 0 To t1
                sum += MODdata.Pbrake(t)
            Next
            ErgEntries("\\Ebrake").ValueString = (sum / 3600)

            'Etransm
            sum = 0
            For t = 0 To t1
                sum += MODdata.PlossDiff(t) + MODdata.PlossGB(t)
            Next
            ErgEntries("\\Etransm").ValueString = (-sum / 3600)

            'Masse, Loading
            ErgEntries("\\Mass").ValueString = (VEH.Mass + VEH.MassExtra)
            ErgEntries("\\Loading").ValueString = VEH.Loading

            'CylceKin
            For Each ErgEntry In MODdata.CylceKin.ErgEntries
                ErgEntries("\\" & ErgEntry.Head).ValueString = MODdata.CylceKin.GetValueString(ErgEntry.Head)
            Next

            If GEN.VehMode <> tVehMode.EV Then
                'EposICE
                sum = 0
                For t = 0 To t1
                    sum += Math.Max(0, MODdata.Pe(t))
                Next
                ErgEntries("\\EposICE").ValueString = (VEH.Pnenn * sum / 3600)

                'EnegICE
                sum = 0
                For t = 0 To t1
                    sum += Math.Min(0, MODdata.Pe(t))
                Next
                ErgEntries("\\EnegICE").ValueString = (VEH.Pnenn * sum / 3600)
            End If

        End If

        'Create Output-string:
        First = True

        For Each key In ErgEntryList
            If Not First Then s.Append(",")
            s.Append(ErgEntries(key).ValueString)
            First = False
        Next

        Return s.ToString

    End Function

    Private Function HeadInit() As Boolean
        Dim MsgSrc As String

        MsgSrc = "SUMALL/Output"

        'Open file
        Try
            Ferg = My.Computer.FileSystem.OpenTextFileWriter(ERGpath, True, FileFormat)
            Ferg.AutoFlush = True
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot access .vsum file (" & ERGpath & ")", MsgSrc)
            Return False
        End Try

        '*** Header / Units
        Ferg.WriteLine("Job,Input File,Cycle," & ErgHead())
        Ferg.WriteLine("[-],[-],[-]," & ErgUnits())

        'Close file (will open after each job)
        Ferg.Close()

        HeadInitialized = True

        Return True

    End Function

    Public Function AusgERG(ByVal NrOfRunStr As String, ByVal GenFilename As String, ByVal CycleFilename As String, ByVal AbortedByError As Boolean) As Boolean
        Dim str As String
        Dim MsgSrc As String
        Dim dic As Dictionary(Of String, Object)
        Dim dic0 As Dictionary(Of String, Object)
        Dim dic1 As Dictionary(Of String, Object)
        Dim key As String

        MsgSrc = "SUMALL/Output"

        If Not HeadInitialized Then
            If Not HeadInit() Then Return False
        End If

        'JSON
        dic = New Dictionary(Of String, Object)

        'Open file
        Try
            Ferg = My.Computer.FileSystem.OpenTextFileWriter(ERGpath, True, FileFormat)
            Ferg.AutoFlush = True
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot access .vsum file (" & ERGpath & ")", MsgSrc)
            Return False
        End Try

        str = NrOfRunStr & "," & GenFilename & "," & CycleFilename & ","
        dic.Add("Job", GenFilename)
        dic.Add("Cycle", CycleFilename)

        If AbortedByError Then
            Ferg.WriteLine(str & "Aborted due to Error!")
            dic.Add("AbortedByError", True)
        Else
            Ferg.WriteLine(str & ErgLine())
            dic.Add("AbortedByError", False)

            dic1 = New Dictionary(Of String, Object)
            For Each key In ErgEntryList
                dic0 = New Dictionary(Of String, Object)
                dic0.Add("Value", ErgEntries(key).ValueString)
                dic0.Add("Unit", ErgEntries(key).Unit)
                dic1.Add(ErgEntries(key).Head, dic0)
            Next
            dic.Add("Results", dic1)

        End If

        ResList.Add(dic)


        'Close file
        Ferg.Close()
        Ferg = Nothing

        Return True

    End Function

    Public Function WriteJSON() As Boolean

        ergJSON.Content("Body").add("Results", ResList)

        Try
            Return ergJSON.WriteFile(ERGpath & ".json")
        Catch ex As Exception
            Return False
        End Try

    End Function

    Private Sub AddToErg(ByVal IDstring As String, ByVal Head As String, ByVal Unit As String)
        If Not ErgEntries.ContainsKey(IDstring) Then
            ErgEntries.Add(IDstring, New cErgEntry(Head, Unit))
            ErgEntryList.Add(IDstring)
        End If
    End Sub

    Private Sub AddToErg(ByVal IDstring As String, ByVal Head As String, ByVal Unit As String, ByVal PerKm As Boolean)
        If PerKm Then
            If Not ErgEntries.ContainsKey(IDstring & "_km") Then
                ErgEntries.Add(IDstring & "_km", New cErgEntry(Head, Unit))
                ErgEntryList.Add(IDstring & "_km")
            End If
        Else
            If Not ErgEntries.ContainsKey(IDstring) Then
                ErgEntries.Add(IDstring, New cErgEntry(Head, Unit))
                ErgEntryList.Add(IDstring)
            End If
        End If

    End Sub

    Public Function Init(ByVal GenFile As String) As Boolean
        Dim GENs As New List(Of String)
        Dim str As String
        Dim str1 As String
        Dim iGEN As Integer
        Dim file As New cFile_V3
        Dim GEN0 As cGEN
        Dim MAP0 As cMAP
        Dim ENG0 As cENG
        Dim Em0 As cEmComp
        Dim HEVorEVdone As Boolean
        Dim EVdone As Boolean
        Dim EngOnly As Boolean
        Dim NonEngOnly As Boolean
        Dim ErgEntry As cErgEntry
        Dim CylceKin As cCycleKin
        Dim i1 As Integer
        Dim i2 As Integer
        Dim iDim As Integer
        Dim DRI0 As cDRI
        Dim dic As Dictionary(Of String, Object)


        Dim MsgSrc As String

        MsgSrc = "SUMALL/Init"

        'Check if file exists
        If Not IO.File.Exists(GenFile) Then
            WorkerMsg(tMsgID.Err, "Job file not found! (" & GenFile & ")", MsgSrc)
            Return False
        End If

        'Define Output-path
        If (PHEMmode = tPHEMmode.ModeBATCH) Then
            Select Case UCase(Cfg.BATCHoutpath)
                Case sKey.WorkDir
                    ERGpath = Cfg.WorkDPath & fFILE(GenFile, False) & "_BATCH.vsum"
                Case sKey.GenPath
                    ERGpath = fFileWoExt(GenFile) & "_BATCH.vsum"
                Case Else
                    ERGpath = Cfg.BATCHoutpath & fFILE(GenFile, False) & "_BATCH.vsum"
            End Select
        Else
            ERGpath = fFileWoExt(GenFile) & ".vsum"
        End If

        'Open file
        Try
            'Open file
            Ferg = My.Computer.FileSystem.OpenTextFileWriter(ERGpath, False, FileFormat)
            Ferg.AutoFlush = True
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot write to .vsum file (" & ERGpath & ")", MsgSrc)
            Return False
        End Try

        'JSON
        ergJSON = New cJSON

        dic = New Dictionary(Of String, Object)
        dic.Add("CreatedBy", Lic.LicString & " (" & Lic.GUID & ")")
        dic.Add("Date", Now.ToString)
        dic.Add("AppVersion", VECTOvers)
        dic.Add("FileVersion", FormatVersion)
        ergJSON.Content.Add("Header", dic)
        ergJSON.Content.Add("Body", New Dictionary(Of String, Object))
        dic = New Dictionary(Of String, Object)
        dic.Add("Air Density [kg/m3]", Cfg.AirDensity)
        dic.Add("Distance Correction", Cfg.WegKorJa)
        ergJSON.Content("Body").add("Settings", dic)

        ResList = New List(Of Dictionary(Of String, Object))

        'Info
        Ferg.WriteLine("VECTO results")
        Ferg.WriteLine("VECTO " & VECTOvers)
        Ferg.WriteLine(Now.ToString)
        Ferg.WriteLine("air density [kg/m3]: " & Cfg.AirDensity)
        If Cfg.WegKorJa Then
            Ferg.WriteLine("Distance Correction ON")
        Else
            Ferg.WriteLine("Distance Correction OFF")
        End If

        'Close file (will open after each job)
        Ferg.Close()

        'Add file to signing list
        Lic.FileSigning.AddFile(ERGpath)
        If Cfg.JSON Then Lic.FileSigning.AddFile(ERGpath & ".json")


        ErgEntries = New Dictionary(Of String, cErgEntry)
        ErgEntryList = New List(Of String)


        '********************** GEN-Liste raussuchen. Bei ADVANCE aus Flotte sonst aus Jobliste '********************** |@@| Select GEN-list for ADVANCE either from Fleet or from Job-list '**********************
        For Each str In JobFileList
            GENs.Add(fFileRepl(str))
        Next
        iGEN = GENs.Count - 1


        '********************** Create Erg-entries '**********************
        EVdone = False
        HEVorEVdone = False
        EngOnly = False
        NonEngOnly = False

        'Vehicle type-independent
        AddToErg("\\T", "time", "[s]")
        AddToErg("\\Prated", "Prated", "[kW]")

        'For each GEN-file check Mode and Map
        For Each str In GENs

            GEN0 = New cGEN

            GEN0.FilePath = str

            Try
                If Not GEN0.ReadFile Then
                    WorkerMsg(tMsgID.Err, "Can't read .gen file '" & str & "' !", MsgSrc)
                    Return False
                End If
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & str & ")", MsgSrc)
                Return False
            End Try

            If GEN0.VehMode = tVehMode.EngineOnly Then

                If Not EngOnly Then

                    'nothing...

                    EngOnly = True

                End If

            Else

                If Not NonEngOnly Then

                    AddToErg("\\S", "distance", "[km]")
                    AddToErg("\\V", "speed", "[km/h]")
                    AddToErg("\\G", "∆altitude", "[m]")

                    NonEngOnly = True

                End If

                'Auxiliary energy consumption
                If GEN0.AuxDef Then
                    For Each str1 In GEN0.AuxPaths.Keys
                        AddToErg("\\Eaux_" & UCase(str1), "Eaux_" & str1, "[kWh]")
                    Next
                End If

            End If


            'Electric-Vehicle / Hybrid
            If GEN0.VehMode = tVehMode.EV Or GEN0.VehMode = tVehMode.HEV Then

                'EV & HEV
                If Not HEVorEVdone Then

                    AddToErg("\\PeEM+", "PeEM+", "[kW]")
                    AddToErg("\\PeBat+", "PeBat+", "[kW]")
                    AddToErg("\\PiBat+", "PiBat+", "[kW]")
                    AddToErg("\\PeEM-", "PeEM-", "[kW]")
                    AddToErg("\\PeBat-", "PeBat-", "[kW]")
                    AddToErg("\\PiBat-", "PiBat-", "[kW]")
                    AddToErg("\\EiBat+", "EiBat+", "[kWh]")
                    AddToErg("\\EiBat-", "EiBat-", "[kWh]")
                    AddToErg("\\EtaEM", "EtaEM", "[%]")
                    AddToErg("\\EtaBat", "EtaBat", "[%]")
                    AddToErg("\\∆SOC", "∆SOC", "[%]")

                    HEVorEVdone = True

                End If

                'Only EV:
                If GEN0.VehMode = tVehMode.EV And Not EVdone Then

                    AddToErg("\\EC", "EC", "[kWh/km]")

                    EVdone = True

                End If


            End If


            'Conventional / Hybrid (Everything except EV)
            If GEN0.VehMode <> tVehMode.EV Then

                'Conventional vehicles ...
                'AddToErg("\\n_norm", "n_norm", "[-]")
                'AddToErg("\\Pe_norm", "Pe_norm", "[-]")
                AddToErg("\\Ppos", "Ppos", "[-]")
                AddToErg("\\Pneg", "Pneg", "[-]")

                If GEN0.CreateMap Then

                    'From the measured data
                    DRI0 = New cDRI
                    DRI0.FilePath = GEN0.CycleFiles(0).FullPath

                    Try
                        If Not DRI0.ReadFile Then Return False
                    Catch ex As Exception
                        WorkerMsg(tMsgID.Err, "File read error! (" & GEN0.CycleFiles(0).FullPath & ")", MsgSrc)
                        Return False
                    End Try

                    For Each Em0 In DRI0.EmComponents.Values

                        If Em0.WriteOutput Then

                            'Dump x/h if in ADVANCE mode -or- EngineOnly -or- Units not in x/h and therefore Conversion into  x/km is not possible
                            If Em0.NormID = tEmNorm.x Or GEN0.VehMode = tVehMode.EngineOnly Then
                                AddToErg(Em0.IDstring, Em0.Name, Em0.Unit, False)
                            Else
                                AddToErg(Em0.IDstring, Em0.Name, "[" & Em0.RawUnit & "/km]", True)
                            End If

                        End If

                    Next

                    AddToErg(sKey.MAP.Extrapol, fMapCompName(tMapComp.Extrapol), "[-]")

                Else

                    'From the Engine-Map
                    ENG0 = New cENG
                    ENG0.FilePath = GEN0.PathENG

                    Try
                        If Not ENG0.ReadFile Then Return False
                    Catch ex As Exception
                        WorkerMsg(tMsgID.Err, "File read error! (" & GEN0.PathENG & ")", MsgSrc)
                        Return False
                    End Try

                    MAP0 = New cMAP(GEN0.PKWja)
                    MAP0.FilePath = ENG0.PathMAP

                    Try
                        If Not MAP0.ReadFile(False) Then Return False 'Fehlermeldungen werden auch bei "MsgOutput = False" ausgegeben
                    Catch ex As Exception
                        WorkerMsg(tMsgID.Err, "File read error! (" & ENG0.PathMAP & ")", MsgSrc)
                        Return False
                    End Try


                    For Each str1 In MAP0.EmList

                        Em0 = MAP0.EmComponents(str1)

                        If Em0.WriteOutput Then

                            'Dump x/h if ADVANCE mode -or- EngineOnly -or- Units not in x/h and therefore Conversion into x/km is not possible
                            If Em0.NormID = tEmNorm.x Or GEN0.VehMode = tVehMode.EngineOnly Then
                                AddToErg(Em0.IDstring, Em0.Name, Em0.Unit, False)
                            Else
                                AddToErg(Em0.IDstring, Em0.Name, "[" & Em0.RawUnit & "/km]", True)
                            End If

                        End If

                    Next

                End If

            End If

        Next


        If EngOnly Then

            'currently nothing

        End If

        If NonEngOnly Then

            'Vehicle-related fields
            AddToErg("\\Pbrake", "Pbrake", "[-]")
            AddToErg("\\EposICE", "EposICE", "[kWh]")
            AddToErg("\\EnegICE", "EnegICE", "[kWh]")
            AddToErg("\\Eair", "Eair", "[kWh]")
            AddToErg("\\Eroll", "Eroll", "[kWh]")
            AddToErg("\\Egrad", "Egrad", "[kWh]")
            AddToErg("\\Eacc", "Eacc", "[kWh]")
            AddToErg("\\Eaux", "Eaux", "[kWh]")
            AddToErg("\\Ebrake", "Ebrake", "[kWh]")
            AddToErg("\\Etransm", "Etransm", "[kWh]")
            AddToErg("\\Mass", "Mass", "[kg]")
            AddToErg("\\Loading", "Loading", "[kg]")

            'CylceKin
            CylceKin = New cCycleKin
            For Each ErgEntry In CylceKin.ErgEntries
                AddToErg("\\" & ErgEntry.Head, ErgEntry.Head, ErgEntry.Unit)
            Next

        End If

        'ErgListe sortieren damit g/km und g/h nebeneinander liegen |@@| Sort ErgListe so that g/km and g/h are side-by-side
        iDim = ErgEntryList.Count - 1

        For i1 = 0 To iDim - 1
            str = ErgEntries(ErgEntryList(i1)).Head
            For i2 = i1 + 1 To iDim
                If ErgEntries(ErgEntryList(i2)).Head = str Then
                    ErgEntryList.Insert(i1 + 1, ErgEntryList(i2))
                    ErgEntryList.RemoveAt(i2 + 1)
                End If
            Next
        Next

        'Sort Aux
        For i1 = 0 To iDim - 1
            str = ErgEntries(ErgEntryList(i1)).Head
            If str.Length > 4 AndAlso Left(str, 4) = "Eaux" Then
                For i2 = i1 + 1 To iDim
                    If ErgEntries(ErgEntryList(i2)).Head.Length > 4 AndAlso Left(ErgEntries(ErgEntryList(i2)).Head, 4) = "Eaux" Then
                        ErgEntryList.Insert(i1 + 1, ErgEntryList(i2))
                        ErgEntryList.RemoveAt(i2 + 1)
                    End If
                Next
            End If
        Next

        Return True

    End Function

    Public ReadOnly Property ErgFile As String
        Get
            Return ERGpath
        End Get
    End Property

End Class

Public Class cErgEntry
    Public Head As String
    Public Unit As String
    Public MyVal As Object

    Public Sub New(ByVal HeadStr As String, ByVal UnitStr As String)
        Head = HeadStr
        Unit = UnitStr
        MyVal = Nothing
    End Sub

    Public Property ValueString As Object
        Get
            If MyVal Is Nothing Then
                Return "-"
            Else
                Return MyVal
            End If
        End Get
        Set(value As Object)
            MyVal = value
        End Set
    End Property



End Class
