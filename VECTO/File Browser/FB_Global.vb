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
''' Global File Brower properties and cFilebrowser instances.
''' </summary>
''' <remarks></remarks>
Module FB_Global

    Public FB_FolderHistory(19) As String
    Public FB_Drives() As String
    Public FB_Init As Boolean
    Public FB_FilHisDir As String
    '-----------------------------
    Public fbFolder As cFileBrowser
    Public fbVECTO As cFileBrowser
    Public fbFileLists As cFileBrowser
    Public fbVEH As cFileBrowser
    Public fbDRI As cFileBrowser
    Public fbMAP As cFileBrowser
    Public fbFLD As cFileBrowser

    Public fbENG As cFileBrowser
    Public fbGBX As cFileBrowser
    Public fbACC As cFileBrowser
    Public fbAUX As cFileBrowser

    Public fbGBS As cFileBrowser
    Public fbTLM As cFileBrowser
    Public fbRLM As cFileBrowser
    Public fbTCC As cFileBrowser
    Public fbCDx As cFileBrowser

    Public fbVMOD As cFileBrowser


End Module

