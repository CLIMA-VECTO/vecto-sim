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

Class cVSUM

    Private Const FormatVersion As Short = 1

    Private VSUMpath As String
    Private Fvsum As System.IO.StreamWriter
    Private HeadInitialized As Boolean

    Private VSUMentries As Dictionary(Of String, cVSUMentry)
    Private VSUMentryList As List(Of String)     'Wird benötigt weil Dictionary nicht sortiert ist

    Private vsumJSON As cJSON
    Private ResList As List(Of Dictionary(Of String, Object))


    Public Sub New()
        HeadInitialized = False
        VSUMpath = ""
    End Sub

    Public Function VSUMhead() As String
        Dim s As New System.Text.StringBuilder
        Dim key As String
        Dim First As Boolean

        First = True
        For Each key In VSUMentryList
            If Not First Then s.Append(",")
            s.Append(VSUMentries(key).Head)
            First = False
        Next

        Return s.ToString

    End Function

    Public Function VSUMunits() As String
        Dim s As New System.Text.StringBuilder
        Dim First As Boolean
        Dim key As String

        First = True
        For Each key In VSUMentryList
            If Not First Then s.Append(",")
            s.Append(VSUMentries(key).Unit)
            First = False
        Next

        Return s.ToString

    End Function

    Public Function VSUMline() As String
        Dim VSUMentry As cVSUMentry
        Dim s As New System.Text.StringBuilder
        Dim t1 As Integer
        Dim Vquer As Single
        Dim sum As Double
        Dim t As Integer
        Dim key As String
        Dim First As Boolean

        For Each VSUMentry In VSUMentries.Values
            VSUMentry.ValueString = Nothing
        Next

        t1 = MODdata.tDim

        'Vehicle type-independent
        VSUMentries("\\T").ValueString = (t1 + 1)

        'Length, Speed, Slope
        If Not VEC.EngOnly Then

            'Average-Speed. calculation
            sum = 0
            For t = 0 To t1
                sum += MODdata.Vh.V(t)
            Next
            Vquer = 3.6 * sum / (t1 + 1)

            VSUMentries("\\S").ValueString = (Vquer * (t1 + 1) / 3600)
            VSUMentries("\\V").ValueString = Vquer

            'altitude change
            VSUMentries("\\G").ValueString = MODdata.Vh.AltIntp(Vquer * (t1 + 1) / 3.6, False) - MODdata.Vh.AltIntp(0, False)

            'Auxiliary energy consumption
            If VEC.AuxDef Then
                For Each key In VEC.AuxPaths.Keys
                    sum = 0
                    For t = 0 To t1
                        sum += MODdata.Paux(key)(t)
                    Next
                    VSUMentries("\\Eaux_" & UCase(key)).ValueString = sum / 3600
                Next
            End If


        End If

        'FC
        If MODdata.FCerror Then

            If VEC.EngOnly Then
                VSUMentries("FC_h").ValueString = "ERROR"
            Else
                VSUMentries("FC_km").ValueString = "ERROR"
            End If

            If MODdata.FCAUXcSet Then
                If VEC.EngOnly Then
                    VSUMentries("FC-AUXc_h").ValueString = "ERROR"
                Else
                    VSUMentries("FC-AUXc_km").ValueString = "ERROR"
                End If
            End If

            If Cfg.DeclMode Then
                If VEC.EngOnly Then
                    VSUMentries("FC-WHTCc_h").ValueString = "ERROR"
                Else
                    VSUMentries("FC-WHTCc_km").ValueString = "ERROR"
                End If
            End If

        Else

            If VEC.EngOnly Then
                VSUMentries("FC_h").ValueString = MODdata.FCavg
            Else
                VSUMentries("FC_km").ValueString = (MODdata.FCavg / Vquer)
            End If

            If MODdata.FCAUXcSet Then
                If VEC.EngOnly Then
                    VSUMentries("FC-AUXc_h").ValueString = MODdata.FCavgAUXc
                Else
                    VSUMentries("FC-AUXc_km").ValueString = (MODdata.FCavgAUXc / Vquer)
                End If
            End If

            If Cfg.DeclMode Then
                If VEC.EngOnly Then
                    VSUMentries("FC-WHTCc_h").ValueString = MODdata.FCavgWHTCc
                Else
                    VSUMentries("FC-WHTCc_km").ValueString = (MODdata.FCavgWHTCc / Vquer)
                End If
            End If

        End If

        'Power, Revolutions

        'Ppos
        sum = 0
        For t = 0 To t1
            sum += Math.Max(0, MODdata.Pe(t))
        Next
        VSUMentries("\\Ppos").ValueString = (sum / (t1 + 1))

        'Pneg
        sum = 0
        For t = 0 To t1
            sum += Math.Min(0, MODdata.Pe(t))
        Next
        VSUMentries("\\Pneg").ValueString = (sum / (t1 + 1))



        'Only Entire-vehicle (not EngOnly)
        If Not VEC.EngOnly Then

            'Pbrake-norm
            sum = 0
            For t = 0 To t1
                sum += MODdata.Pbrake(t)
            Next
            VSUMentries("\\Pbrake").ValueString = (sum / (t1 + 1))

            'Eair
            sum = 0
            For t = 0 To t1
                sum += MODdata.Pluft(t)
            Next
            VSUMentries("\\Eair").ValueString = (-sum / 3600)

            'Eroll
            sum = 0
            For t = 0 To t1
                sum += MODdata.Proll(t)
            Next
            VSUMentries("\\Eroll").ValueString = (-sum / 3600)

            'Egrad
            sum = 0
            For t = 0 To t1
                sum += MODdata.Pstg(t)
            Next
            VSUMentries("\\Egrad").ValueString = (-sum / 3600)

            'Eacc
            sum = 0
            For t = 0 To t1
                sum += MODdata.Pa(t) + MODdata.PaGB(t) + MODdata.PaEng(t)
            Next
            VSUMentries("\\Eacc").ValueString = (-sum / 3600)

            'Eaux
            sum = 0
            For t = 0 To t1
                sum += MODdata.PauxSum(t)
            Next
            VSUMentries("\\Eaux").ValueString = (-sum / 3600)

            'Ebrake
            sum = 0
            For t = 0 To t1
                sum += MODdata.Pbrake(t)
            Next
            VSUMentries("\\Ebrake").ValueString = (sum / 3600)

            'Etransm
            sum = 0
            For t = 0 To t1
                sum += MODdata.PlossDiff(t) + MODdata.PlossGB(t)
            Next
            VSUMentries("\\Etransm").ValueString = (-sum / 3600)

            'Retarder
            sum = 0
            For t = 0 To t1
                sum += MODdata.PlossRt(t)
            Next
            VSUMentries("\\Eretarder").ValueString = (-sum / 3600)

            'Masse, Loading
            VSUMentries("\\Mass").ValueString = (VEH.Mass + VEH.MassExtra)
            VSUMentries("\\Loading").ValueString = VEH.Loading

            'CylceKin
            For Each VSUMentry In MODdata.CylceKin.VSUMentries
                VSUMentries("\\" & VSUMentry.Head).ValueString = MODdata.CylceKin.GetValueString(VSUMentry.Head)
            Next

            'EposICE
            sum = 0
            For t = 0 To t1
                sum += Math.Max(0, MODdata.Pe(t))
            Next
            VSUMentries("\\EposICE").ValueString = (sum / 3600)

            'EnegICE
            sum = 0
            For t = 0 To t1
                sum += Math.Min(0, MODdata.Pe(t))
            Next
            VSUMentries("\\EnegICE").ValueString = (sum / 3600)

        End If

        'Create Output-string:
        First = True

        For Each key In VSUMentryList
            If Not First Then s.Append(",")
            s.Append(VSUMentries(key).ValueString)
            First = False
        Next

        Return s.ToString

    End Function

    Private Function HeadInit() As Boolean
        Dim MsgSrc As String

        MsgSrc = "SUMALL/Output"

        'Open file
        Try
            Fvsum = My.Computer.FileSystem.OpenTextFileWriter(VSUMpath, True, FileFormat)
            Fvsum.AutoFlush = True
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot access .vsum file (" & VSUMpath & ")", MsgSrc)
            Return False
        End Try

        '*** Header / Units
        Fvsum.WriteLine("Job,Input File,Cycle," & VSUMhead())
        Fvsum.WriteLine("[-],[-],[-]," & VSUMunits())

        'Close file (will open after each job)
        Fvsum.Close()

        HeadInitialized = True

        Return True

    End Function

    Public Function AusgVSUM(ByVal NrOfRunStr As String, ByVal JobFilename As String, ByVal CycleFilename As String, ByVal AbortedByError As Boolean) As Boolean
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
            Fvsum = My.Computer.FileSystem.OpenTextFileWriter(VSUMpath, True, FileFormat)
            Fvsum.AutoFlush = True
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot access .vsum file (" & VSUMpath & ")", MsgSrc)
            Return False
        End Try

        str = NrOfRunStr & "," & JobFilename & "," & CycleFilename & ","
        dic.Add("Job", JobFilename)
        dic.Add("Cycle", CycleFilename)

        If AbortedByError Then
            Fvsum.WriteLine(str & "Aborted due to Error!")
            dic.Add("AbortedByError", True)
        Else
            Fvsum.WriteLine(str & VSUMline())
            dic.Add("AbortedByError", False)

            dic1 = New Dictionary(Of String, Object)
            For Each key In VSUMentryList
                dic0 = New Dictionary(Of String, Object)
                dic0.Add("Value", VSUMentries(key).ValueString)
                dic0.Add("Unit", VSUMentries(key).Unit)
                dic1.Add(VSUMentries(key).Head, dic0)
            Next
            dic.Add("Results", dic1)

        End If

        ResList.Add(dic)


        'Close file
        Fvsum.Close()
        Fvsum = Nothing

        Return True

    End Function

    Public Function WriteJSON() As Boolean

        vsumJSON.Content("Body").add("Results", ResList)

        Try
            Return vsumJSON.WriteFile(VSUMpath & ".json")
        Catch ex As Exception
            Return False
        End Try

    End Function

    Private Sub AddToVSUM(ByVal IDstring As String, ByVal Head As String, ByVal Unit As String)
        If Not VSUMentries.ContainsKey(IDstring) Then
            VSUMentries.Add(IDstring, New cVSUMentry(Head, Unit))
            VSUMentryList.Add(IDstring)
        End If
    End Sub

    Public Function Init(ByVal JobFile As String) As Boolean
        Dim JobFiles As New List(Of String)
        Dim str As String
        Dim str1 As String
        Dim file As New cFile_V3
        Dim VEC0 As cVECTO
        Dim MAP0 As cMAP
        Dim ENG0 As cENG
        Dim HEVorEVdone As Boolean
        Dim EVdone As Boolean
        Dim EngOnly As Boolean
        Dim NonEngOnly As Boolean
        Dim VSUMentry As cVSUMentry
        Dim CylceKin As cCycleKin
        Dim i1 As Integer
        Dim i2 As Integer
        Dim iDim As Integer
        Dim dic As Dictionary(Of String, Object)


        Dim MsgSrc As String

        MsgSrc = "SUMALL/Init"

        'Check if file exists
        If Not IO.File.Exists(JobFile) Then
            WorkerMsg(tMsgID.Err, "Job file not found! (" & JobFile & ")", MsgSrc)
            Return False
        End If

        'Define Output-path
        If (CalcMode = tCalcMode.ModeBATCH) Then
            Select Case UCase(Cfg.BATCHoutpath)
                Case sKey.JobPath
                    VSUMpath = fFileWoExt(JobFile) & "_BATCH.vsum"
                Case Else
                    VSUMpath = Cfg.BATCHoutpath & fFILE(JobFile, False) & "_BATCH.vsum"
            End Select
        Else
            VSUMpath = fFileWoExt(JobFile) & ".vsum"
        End If

        'Open file
        Try
            'Open file
            Fvsum = My.Computer.FileSystem.OpenTextFileWriter(VSUMpath, False, FileFormat)
            Fvsum.AutoFlush = True
        Catch ex As Exception
            WorkerMsg(tMsgID.Err, "Cannot write to .vsum file (" & VSUMpath & ")", MsgSrc)
            Return False
        End Try

        'JSON
        vsumJSON = New cJSON

        dic = New Dictionary(Of String, Object)
        dic.Add("CreatedBy", Lic.LicString & " (" & Lic.GUID & ")")
        dic.Add("Date", Now.ToString)
        dic.Add("AppVersion", VECTOvers)
        dic.Add("FileVersion", FormatVersion)
        vsumJSON.Content.Add("Header", dic)
        vsumJSON.Content.Add("Body", New Dictionary(Of String, Object))
        dic = New Dictionary(Of String, Object)
        dic.Add("Air Density [kg/m3]", Cfg.AirDensity)
        dic.Add("Distance Correction", Cfg.DistCorr)
        vsumJSON.Content("Body").add("Settings", dic)

        ResList = New List(Of Dictionary(Of String, Object))

        'Info
        Fvsum.WriteLine("VECTO results")
        Fvsum.WriteLine("VECTO " & VECTOvers)
        Fvsum.WriteLine(Now.ToString)
        Fvsum.WriteLine("air density [kg/m3]: " & Cfg.AirDensity)
        If Cfg.DistCorr Then
            Fvsum.WriteLine("Distance Correction ON")
        Else
            Fvsum.WriteLine("Distance Correction OFF")
        End If

        'Close file (will open after each job)
        Fvsum.Close()

        'Add file to signing list
        Lic.FileSigning.AddFile(VSUMpath)
        Lic.FileSigning.AddFile(VSUMpath & ".json")


        VSUMentries = New Dictionary(Of String, cVSUMentry)
        VSUMentryList = New List(Of String)


        For Each str In JobFileList
            JobFiles.Add(fFileRepl(str))
        Next


        '********************** Create VSUM-Entries '**********************
        EVdone = False
        HEVorEVdone = False
        EngOnly = False
        NonEngOnly = False

        'Vehicle type-independent
        AddToVSUM("\\T", "time", "[s]")

        For Each str In JobFiles

            VEC0 = New cVECTO

            VEC0.FilePath = str

            Try
                If Not VEC0.ReadFile Then
                    WorkerMsg(tMsgID.Err, "Can't read .vecto file '" & str & "' !", MsgSrc)
                    Return False
                End If
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & str & ")", MsgSrc)
                Return False
            End Try

            If VEC0.EngOnly Then

                If Not EngOnly Then

                    'nothing...

                    EngOnly = True

                End If

            Else

                If Not NonEngOnly Then

                    AddToVSUM("\\S", "distance", "[km]")
                    AddToVSUM("\\V", "speed", "[km/h]")
                    AddToVSUM("\\G", "∆altitude", "[m]")

                    NonEngOnly = True

                End If

                'Auxiliary energy consumption
                If VEC0.AuxDef Then
                    For Each str1 In VEC0.AuxPaths.Keys
                        AddToVSUM("\\Eaux_" & UCase(str1), "Eaux_" & str1, "[kWh]")
                    Next
                End If

            End If

            'Conventional vehicles ...
            AddToVSUM("\\Ppos", "Ppos", "[kW]")
            AddToVSUM("\\Pneg", "Pneg", "[kW]")

            'From the Engine-Map
            ENG0 = New cENG
            ENG0.FilePath = VEC0.PathENG

            Try
                If Not ENG0.ReadFile Then Return False
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & VEC0.PathENG & ")", MsgSrc)
                Return False
            End Try

            MAP0 = New cMAP
            MAP0.FilePath = ENG0.PathMAP

            Try
                If Not MAP0.ReadFile(False) Then Return False 'Fehlermeldungen werden auch bei "MsgOutput = False" ausgegeben
            Catch ex As Exception
                WorkerMsg(tMsgID.Err, "File read error! (" & ENG0.PathMAP & ")", MsgSrc)
                Return False
            End Try

            If VEC0.EngOnly Then
                AddToVSUM("FC_h", "FC", "[g/h]")
                AddToVSUM("FC-AUXc_h", "FC-AUXc", "[g/h]")
                AddToVSUM("FC-WHTCc_h", "FC-WHTCc", "[g/h]")
            Else
                AddToVSUM("FC_km", "FC", "[g/km]")
                AddToVSUM("FC-AUXc_km", "FC-AUXc", "[g/km]")
                AddToVSUM("FC-WHTCc_km", "FC-WHTCc", "[g/km]")
            End If

        Next


        If EngOnly Then

            'currently nothing

        End If

        If NonEngOnly Then

            'Vehicle-related fields
            AddToVSUM("\\Pbrake", "Pbrake", "[kW]")
            AddToVSUM("\\EposICE", "EposICE", "[kWh]")
            AddToVSUM("\\EnegICE", "EnegICE", "[kWh]")
            AddToVSUM("\\Eair", "Eair", "[kWh]")
            AddToVSUM("\\Eroll", "Eroll", "[kWh]")
            AddToVSUM("\\Egrad", "Egrad", "[kWh]")
            AddToVSUM("\\Eacc", "Eacc", "[kWh]")
            AddToVSUM("\\Eaux", "Eaux", "[kWh]")
            AddToVSUM("\\Ebrake", "Ebrake", "[kWh]")
            AddToVSUM("\\Etransm", "Etransm", "[kWh]")
            AddToVSUM("\\Eretarder", "Eretarder", "[kWh]")
            AddToVSUM("\\Mass", "Mass", "[kg]")
            AddToVSUM("\\Loading", "Loading", "[kg]")

            'CylceKin
            CylceKin = New cCycleKin
            For Each VSUMentry In CylceKin.VSUMentries
                AddToVSUM("\\" & VSUMentry.Head, VSUMentry.Head, VSUMentry.Unit)
            Next

        End If

        'Sort
        iDim = VSUMentryList.Count - 1

        For i1 = 0 To iDim - 1
            str = VSUMentries(VSUMentryList(i1)).Head
            For i2 = i1 + 1 To iDim
                If VSUMentries(VSUMentryList(i2)).Head = str Then
                    VSUMentryList.Insert(i1 + 1, VSUMentryList(i2))
                    VSUMentryList.RemoveAt(i2 + 1)
                End If
            Next
        Next

        'Sort Aux
        For i1 = 0 To iDim - 1
            str = VSUMentries(VSUMentryList(i1)).Head
            If str.Length > 4 AndAlso Left(str, 4) = "Eaux" Then
                For i2 = i1 + 1 To iDim
                    If VSUMentries(VSUMentryList(i2)).Head.Length > 4 AndAlso Left(VSUMentries(VSUMentryList(i2)).Head, 4) = "Eaux" Then
                        VSUMentryList.Insert(i1 + 1, VSUMentryList(i2))
                        VSUMentryList.RemoveAt(i2 + 1)
                    End If
                Next
            End If
        Next

        Return True

    End Function

    Public ReadOnly Property VSUMfile As String
        Get
            Return VSUMpath
        End Get
    End Property

End Class

Public Class cVSUMentry
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
