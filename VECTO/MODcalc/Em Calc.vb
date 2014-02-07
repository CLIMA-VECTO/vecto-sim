Imports System.Collections.Generic

Public Class cEm

    Public EmComp As Dictionary(Of String, cEmComp)
    Public EmDefComp As Dictionary(Of tMapComp, cEmComp)


    Public Sub Init()
        EmComp = New Dictionary(Of String, cEmComp)
        EmDefComp = New Dictionary(Of tMapComp, cEmComp)
    End Sub

    Public Sub CleanUp()
        EmComp = Nothing
        EmDefComp = Nothing
    End Sub

    Public Function Raw_Calc() As Boolean
        Dim v As Single
        Dim i As Integer
        Dim KV As KeyValuePair(Of String, cEmComp)
        Dim Em0 As cEmComp
        Dim Result As Boolean
        Dim MsgSrc As String

        MsgSrc = ""

        Result = True

        For Each KV In MAP.EmComponents
            Em0 = New cEmComp
            Em0.Name = KV.Value.Name
            Em0.IDstring = KV.Value.IDstring
            Em0.MapCompID = KV.Value.MapCompID
            Em0.Unit = KV.Value.Unit
            Em0.NormID = KV.Value.NormID
            Em0.WriteOutput = KV.Value.WriteOutput
            EmComp.Add(KV.Key, Em0)
        Next

        For i = 0 To MODdata.tDim

            For Each KV In MAP.EmComponents

                Select Case MODdata.EngState(i)

                    Case tEngState.Stopped

                        EmComp(KV.Key).RawVals.Add(0)        '<= Soll das so bleiben?

                    Case Else   '<= Idle / Drag / FullLoad-Unterscheidung...?


                        'Delaunay
                        v = MAP.fFCdelaunay_Intp(MODdata.nU(i), MODdata.Pe(i))

                        If v < 0 And v > -999 Then
                            If v < -0.1 Then
                                WorkerMsg(tMsgID.Err, "FC= " & v & " at " & MODdata.nU(i) & " [1/min], " & MODdata.Pe(i) & " [kW]!", "MAP/FC_Intp")
                                Result = False
                            Else
                                v = 0
                            End If
                        End If

                        If Result Then
                            If v < -999 Then Result = False
                        End If
                        EmComp(KV.Key).RawVals.Add(v)


                End Select

            Next
        Next

        For Each KV In EmComp
            If KV.Value.MapCompID <> tMapComp.Undefined Then
                EmDefComp.Add(KV.Value.MapCompID, KV.Value)
            End If
        Next

        KV = Nothing

        Return Result

    End Function

    Public Sub SumCalc()
        Dim KV As KeyValuePair(Of String, cEmComp)
        For Each KV In EmComp
            KV.Value.SumCalc()
        Next
    End Sub


End Class
