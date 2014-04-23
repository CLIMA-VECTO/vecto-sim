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

''' <summary>
''' Auxiliary
''' </summary>
''' <remarks></remarks>
Public Class cAux

    ''' <summary>
    ''' Input file path (.vaux)
    ''' </summary>
    ''' <remarks></remarks>
    Public Filepath As String

    ''' <summary>
    ''' Transmission ratio to engine speed [-]
    ''' </summary>
    ''' <remarks></remarks>
    Public TransRatio As Single

    ''' <summary>
    ''' Efficiency to engine [-]
    ''' </summary>
    ''' <remarks></remarks>
    Public EffToEng As Single

    ''' <summary>
    ''' Efficiency auxiliary to supply [-]
    ''' </summary>
    ''' <remarks></remarks>
    Public EffToSply As Single

    ''' <summary>
    ''' Efficiency map
    ''' </summary>
    ''' <remarks>x= Auxiliary speed [rpm]; y= Supply power [kW]; z= Mechanical power [kW]. Note that the columns in the input file are different!</remarks>
    Private EffMap As cDelaunayMap

    ''' <summary>
    ''' New instance. Creates new efficiency map
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        EffMap = New cDelaunayMap
    End Sub

    ''' <summary>
    ''' Read input file (.vaux)
    ''' </summary>
    ''' <returns>True if successful, else False.</returns>
    ''' <remarks></remarks>
    Public Function Readfile() As Boolean
        Dim MsgSrc As String
        Dim file As cFile_V3
        Dim line As String()

        MsgSrc = "Main/ReadInp/Aux"

        'Open file - Abort if file not accessible
        file = New cFile_V3
        If Not file.OpenRead(Filepath) Then
            file = Nothing
            WorkerMsg(tMsgID.Err, "Failed to open file (" & Filepath & ") !", MsgSrc)
            Return False
        End If

        'Map reset
        EffMap = New cDelaunayMap

        'Abort if file is empty
        If file.EndOfFile Then GoTo lbFileEndErr

        'Read file
        Try

            'Transmission ration to engine rpm [-]
            If file.EndOfFile Then GoTo lbFileEndErr
            line = file.ReadLine
            TransRatio = CSng(line(0))

            'Efficiency to engine [-]
            If file.EndOfFile Then GoTo lbFileEndErr
            line = file.ReadLine
            EffToEng = CSng(line(0))

            'Efficiency auxiliary to supply [-]
            If file.EndOfFile Then GoTo lbFileEndErr
            line = file.ReadLine
            EffToSply = CSng(line(0))

            'Efficiency Map
            If file.EndOfFile Then GoTo lbFileEndErr

            'Column 1 = Auxiliary speed [rpm] => X-axis
            'Column 2 = Mechanical power [kW] => Z-Axis (!)
            'Column 3 = Output power [kW] => Y-Axis (!)

            Do While Not file.EndOfFile
                line = file.ReadLine
                EffMap.AddPoints(CDbl(line(0)), CDbl(line(2)), CDbl(line(1)))
            Loop

            'Triangulate map
            If Not EffMap.Triangulate Then
                WorkerMsg(tMsgID.Err, "Aux Map is invalid! (Triangulation Error)", MsgSrc)
                GoTo lbErr
            End If

        Catch ex As Exception

            WorkerMsg(tMsgID.Err, "Error while reading aux file! (" & ex.Message & ")", MsgSrc)
            GoTo lbErr

        End Try

        file.Close()

        Return True


lbFileEndErr:

        WorkerMsg(tMsgID.Err, "Unexpected end of file (aux)!", MsgSrc)


lbErr:
        file.Close()
        Return False



    End Function

    ''' <summary>
    ''' Returns power demand for given engine speed and supply power
    ''' </summary>
    ''' <param name="nU">Engine speed [1/min]</param>
    ''' <param name="Psupply">Supply power [kW] from driving cycle</param>
    ''' <returns>Power demand [kW]</returns>
    ''' <remarks></remarks>
    Public Function Paux(ByVal nU As Single, ByVal Psupply As Single) As Single

        Dim nUaux As Single
        Dim PsplyAux As Single
        Dim PauxEff As Single

        nUaux = nU * TransRatio
        PsplyAux = Psupply / EffToSply

        Try
            PauxEff = EffMap.Intpol(nUaux, PsplyAux)
        Catch ex As Exception
            MODdata.ModErrors.AuxMapExtr = fFILE(Filepath, False) & ", U= " & nUaux & " [1/min], PsupplyAux= " & PsplyAux & " [kW]"
            Return 0
        End Try

        Return PauxEff / EffToEng

    End Function





End Class
