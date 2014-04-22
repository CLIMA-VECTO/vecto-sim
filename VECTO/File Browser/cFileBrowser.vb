Imports System.Collections



''' <summary>
''' File Browser for Open/Save File dialogs and Folder Browser. Features File History and Favorite Folders.
''' </summary>
''' <remarks>
''' Usage:
'''  1. Create new instance, preferably in FB_Global, e.g. fbTXT = New cFileBrowser("txt")
'''  2. Define extensions, e.g.  fbTXT.Extensions = New String() {"txt","log"}
'''  3. Use OpenDialog, SaveDialog, etc.
'''  4. Call Close method when closing application to write file history, e.g. fbTXT.Close 
''' File history is unique for each ID. Folder history is global.
''' </remarks>
Public Class cFileBrowser

    Private Initialized As Boolean
    Private MyID As String
    Private MyExt As String()
    Private Dlog As FB_Dialog
    Private NoExt As Boolean
    Private bFolderBrowser As Boolean
    Private bLightMode As Boolean

    ''' <summary>
    ''' New cFileBrowser instance
    ''' </summary>
    ''' <param name="ID">Needed to save the file history when not using LightMode.</param>
    ''' <param name="FolderBrowser">Browse folders instead of files.</param>
    ''' <param name="LightMode">If enabled file history is not saved.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal ID As String, Optional ByVal FolderBrowser As Boolean = False, Optional ByVal LightMode As Boolean = False)
        Initialized = False
        MyID = ID
        NoExt = True
        bFolderBrowser = FolderBrowser
        bLightMode = LightMode
    End Sub

    ''' <summary>
    ''' Opens dialog for OPENING files. Selected file must exist. Returns False if cancelled by user, else True.
    ''' </summary>
    ''' <param name="path">Initial selected file. If empty the last selected file is used. If file without directoy the last directory will be used.</param>
    ''' <param name="MultiFile">Allow selecting multiple files.</param>
    ''' <param name="Ext">Set extension. If not defined the first predefined extension is used.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function OpenDialog(ByVal path As String, Optional ByVal MultiFile As Boolean = False, Optional ByVal Ext As String = "") As Boolean
        Return CustomDialog(path, True, False, tFbExtMode.MultiExt, MultiFile, Ext, "Open")
    End Function

    ''' <summary>
    ''' Opens dialog for SAVING files. If file already exists user will be asked to overwrite. Returns False if cancelled by user, else True.
    ''' </summary>
    ''' <param name="path">Initial selected file. If empty the last selected file is used. If file without directoy the last directory will be used.</param>
    ''' <param name="ForceExt">Force predefined file extension.</param>
    ''' <param name="Ext">Set extension. If not defined the first predefined extension is used.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function SaveDialog(ByVal path As String, Optional ByVal ForceExt As Boolean = True, Optional ByVal Ext As String = "") As Boolean
        Dim x As tFbExtMode
        If ForceExt Then
            x = tFbExtMode.ForceExt
        Else
            x = tFbExtMode.SingleExt
        End If
        Return CustomDialog(path, False, True, x, False, Ext, "Save As")
    End Function

    ''' <summary>
    ''' Custom open/save dialog. Returns False if cancelled by user, else True.
    ''' </summary>
    ''' <param name="path">Initial selected file. If empty the last selected file is used. If file without directoy the last directory will be used.</param>
    ''' <param name="FileMustExist">Selected file must exist.</param>
    ''' <param name="OverwriteCheck">If file already exists user will be asked to overwrite.</param>
    ''' <param name="ExtMode">ForceExt= First predefined extension (or Ext parameter) will be forced (Default for SaveDialog), MultiExt= All files with predefined extensions are shown (Default for OpenDialog), SingleExt= All files with the first predefined extension will be shown.</param>
    ''' <param name="MultiFile">Allow to select multiple files.</param>       
    ''' <param name="Ext">Set extension. If not defined the first predefined extension is used.</param>            
    ''' <param name="Title">Dialog title.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CustomDialog(ByVal path As String, ByVal FileMustExist As Boolean, ByVal OverwriteCheck As Boolean, ByVal ExtMode As tFbExtMode, ByVal MultiFile As Boolean, ByVal Ext As String, Optional Title As String = "File Browser") As Boolean
        If Not Initialized Then Init()
        Return Dlog.Browse(path, FileMustExist, OverwriteCheck, ExtMode, MultiFile, Ext, Title)
    End Function

    'Manually update File History
    ''' <summary>
    ''' Add file to file history.
    ''' </summary>
    ''' <param name="Path">File to be added to file history.</param>
    ''' <remarks></remarks>
    Public Sub UpdateHistory(ByVal Path As String)
        If Not Initialized Then Init()
        Dlog.UpdateHistory(Path)
    End Sub

    ''' <summary>
    ''' Save file history (if not LightMode) and global folder history.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Close()
        If Initialized Then
            Dlog.SaveAndClose()
            Initialized = False
        End If
        Dlog = Nothing
    End Sub

    Private Sub Init()
        Dlog = New FB_Dialog(bLightMode)
        Dlog.ID = MyID
        If Not NoExt Then Dlog.Extensions = MyExt
        If bFolderBrowser Then Dlog.SetFolderBrowser()
        Initialized = True
    End Sub

    ''' <summary>
    ''' Predefined file extensions. Must be set before Open/Save dialog is used for the first time.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Extensions() As String()
        Get
            Return MyExt
        End Get
        Set(ByVal value As String())
            MyExt = value
            NoExt = False
        End Set
    End Property

    ''' <summary>
    ''' Selected file(s) oder folder (if FolderBrowser)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Files() As String()
        Get
            If Initialized Then
                Return Dlog.Files
            Else
                Return New String() {""}
            End If
        End Get
    End Property

End Class


