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

Public Class cVECTO
    Public Mode As tVECTOmode
    Public CurrentMission As cMission
    Public Missions As Dictionary(Of tMission, cMission)
    Public SegmentTable As cSegmentTable


    Public Sub New()
        Mode = tVECTOmode.Declaration
    End Sub

    Public Function Init() As Boolean

        Dim file As New cFile_V3
        Dim m0 As cMission
        Dim s0 As cSegmentTableEntry
        Dim line As String()
        Dim i As Integer

        'Initialize
        Missions = New Dictionary(Of tMission, cMission)
        SegmentTable = New cSegmentTable

        If Not IO.Directory.Exists(MyDeclPath) Then
            GUImsg(tMsgID.Err, "Failed to load Declaration Config!")
            Return False
        End If

        'Missions
        If Not file.OpenRead(MyDeclPath & "Missions.csv") Then
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Mission Definitions)!")
            Return False
        End If

        Try
            'Header
            line = file.ReadLine
            'Data
            Do While Not file.EndOfFile
                m0 = New cMission
                line = file.ReadLine
                m0.MissionID = CType(CInt(line(0)), tMission)
                m0.NameStr = line(1)
                m0.CyclePath = MyDeclPath & line(2)
                Missions.Add(m0.MissionID, m0)
            Loop
        Catch ex As Exception
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Mission Definitions)!")
            file.Close()
            Return False
        End Try

        file.Close()

        'Segment Table
        If Not file.OpenRead(MyDeclPath & "SegmentTable.csv") Then
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Segment Table)!")
            Return False
        End If

        Try
            'Header
            line = file.ReadLine
            'Data
            Do While Not file.EndOfFile
                line = file.ReadLine
                s0 = New cSegmentTableEntry
                s0.VehCat = CType(CInt(line(0)), tVehCat)
                s0.AxleConf = CType(CInt(line(1)), tAxleConf)
                s0.MaxGVW = CSng(line(2))
                s0.VehClass = line(3)
                For i = 4 To 13
                    If Trim(UCase(line((i)))) = "X" Or Trim(UCase(line((i)))) = "XX" Then
                        s0.MissionList.Add(CType(CInt(i - 4), tMission))
                    End If
                Next
                SegmentTable.SegTableEntries.Add(s0)
            Loop
        Catch ex As Exception
            file.Close()
            GUImsg(tMsgID.Err, "Failed to load Declaration Config (Segment Table)!")
            Return False
        End Try

        GUImsg(tMsgID.Normal, "Declaration Config loaded.")

        Return True

    End Function







End Class

Public Class cMission
    Public MissionID As tMission
    Public NameStr As String
    Public CyclePath As String
End Class

Public Class cSegmentTable
    Public SegTableEntries As New List(Of cSegmentTableEntry)

    Public Function ConfigIsValid(ByVal VehCat As tVehCat, ByVal AxleConf As tAxleConf, ByVal MaxGVW As Single) As Boolean
        Dim s0 As cSegmentTableEntry

        For Each s0 In SegTableEntries
            If s0.VehCat = VehCat And s0.AxleConf = AxleConf And s0.MaxGVW > MaxGVW Then
                Return True
            End If
        Next

        Return False

    End Function

    Public Function SetRef(ByRef SegTableEntryRef As cSegmentTableEntry, ByVal VehCat As tVehCat, ByVal AxleConf As tAxleConf, ByVal MaxGVW As Single) As Boolean
        Dim s0 As cSegmentTableEntry

        For Each s0 In SegTableEntries
            If s0.VehCat = VehCat And s0.AxleConf = AxleConf And s0.MaxGVW > MaxGVW Then
                SegTableEntryRef = s0
                Return True
            End If
        Next

        SegTableEntryRef = Nothing
        Return False

    End Function


End Class


Public Class cSegmentTableEntry
    Public VehCat As tVehCat
    Public AxleConf As tAxleConf
    Public MaxGVW As Single
    Public VehClass As String
    Public MissionList As New List(Of tMission)



End Class



