Imports System.Collections.ObjectModel
Imports System.IO
Imports LumiSoft.Net.IMAP
Imports LumiSoft.Net.IMAP.Client
Imports log4net

Public Class ImapMonitor
    Inherits RemoteDirectoryMonitor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private Const SCHEME_IMAP As String = "imap"
    Private Const SCHEME_IMAPS As String = "imaps"
    Private Const PORT_IMAP As Integer = 143
    Private Const PORT_IMAPS As Integer = 993
    Private Const DEFAULT_FOLDER As String = "INBOX"

    ''' <summary>
    ''' Protected constructor for reflection.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new imap folder monitor.
    ''' </summary>
    ''' <param name="name">String. The friendly name for the monitor.</param>
    ''' <param name="path">String. The full path to the imap folder to be monitored.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to monitor the directory.</param>
    ''' <param name="processor">IDataProcessor. The data processor to use to process new files.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, ByVal path As String, ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        MyBase.New(name, path, schedule, processor)
    End Sub

    ''' <summary>
    ''' Creates missing folders before starting the timer.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub CreateFolders()
        MyBase.CreateFolders()

        If Me.CreateMissingFolders Then

            Using client As New IMAP_Client
                client.Connect(Me.Uri.Host, Me.Uri.Port, IIf(Me.Uri.Scheme = SCHEME_IMAPS, True, False))
                If client.IsConnected Then
                    client.Authenticate(Me.Credentials.UserName, Me.Credentials.Password)

                    If client.IsAuthenticated Then
                        Dim folders() As String = client.GetFolders()

                        For Each folderName As String In folders
                            Log.DebugFormat("Folder {0} Exists", folderName)
                        Next

                        Dim folder As String = Me.GetFolderName(Me.Uri, client.GetFolderSeparator)
                        If Not folders.Contains(folder) Then
                            Log.DebugFormat("Creating folder {0}", Me.Uri)

                            client.CreateFolder(folder)
                        End If

                        If Me.CompleteUri IsNot Nothing Then
                            Dim completeFolder As String = Me.GetFolderName(Me.CompleteUri, client.GetFolderSeparator)

                            If Not folders.Contains(completeFolder) Then
                                Log.DebugFormat("Creating folder {0}", Me.CompleteUri)

                                client.CreateFolder(completeFolder)
                            End If
                        End If

                        If Me.FailureUri IsNot Nothing Then
                            Dim failureFolder As String = Me.GetFolderName(Me.FailureUri, client.GetFolderSeparator)

                            If Not folders.Contains(failureFolder) Then
                                Log.DebugFormat("Creating folder {0}", Me.FailureUri)

                                client.CreateFolder(failureFolder)
                            End If
                        End If
                    End If
                End If
            End Using
        End If
    End Sub

    ''' <summary>
    ''' Deletes the data item after processing.
    ''' </summary>
    ''' <param name="item">ImapDataItem. The imap message to delete.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Delete(ByVal item As IDataItem)
        Dim imapItem As ImapDataItem = item

        Log.DebugFormat("Deleting {0}", imapItem.Data)

        Using client As New IMAP_Client
            client.Connect(Me.Uri.Host, Me.Uri.Port, IIf(Me.Uri.Scheme = SCHEME_IMAPS, True, False))
            If client.IsConnected Then
                client.Authenticate(Me.Credentials.UserName, Me.Credentials.Password)

                If client.IsAuthenticated Then
                    Dim folder As String = Me.GetFolderName(Me.Uri, client.GetFolderSeparator)

                    client.SelectFolder(folder)

                    Dim seq As New IMAP_SequenceSet
                    seq.Parse(imapItem.UID)

                    client.DeleteMessages(seq, True)
                End If
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Moves the data item after processing.
    ''' </summary>
    ''' <param name="item">ImapDataItem. The imap message to move.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Move(ByVal item As IDataItem)
        Dim imapItem As ImapDataItem = item
 
        Using client As New IMAP_Client
            client.Connect(Me.Uri.Host, Me.Uri.Port, IIf(Me.Uri.Scheme = SCHEME_IMAPS, True, False))
            If client.IsConnected Then
                client.Authenticate(Me.Credentials.UserName, Me.Credentials.Password)

                If client.IsAuthenticated Then
                    Dim folder As String = Me.GetFolderName(Me.Uri, client.GetFolderSeparator)

                    client.SelectFolder(folder)

                    Dim seq As New IMAP_SequenceSet
                    seq.Parse(imapItem.UID)

                    Dim destination As String = String.Empty
                    If imapItem.Status = DataItemStatus.CompletedProcessing Then
                        Log.DebugFormat("Moving {0} to {1}", imapItem.Data, Me.CompleteUri)

                        destination = Me.GetFolderName(Me.CompleteUri, client.GetFolderSeparator)
                    ElseIf imapItem.Status = DataItemStatus.FailedProcessing Then
                        Log.DebugFormat("Moving {0} to {1}", imapItem.Data, Me.FailureUri)

                        destination = Me.GetFolderName(Me.FailureUri, client.GetFolderSeparator)
                    End If

                    client.MoveMessages(seq, destination, True)
                End If
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Deletes the data item after processing.
    ''' </summary>
    ''' <param name="item">ImapDataItem. The imap message to delete.</param>
    ''' <remarks>This method always returns NotImplementedException</remarks>
    Public Overrides Sub Rename(ByVal item As IDataItem)
        Throw New NotImplementedException
    End Sub

    ''' <summary>
    ''' Prepares the data before if it processed.
    ''' </summary>
    ''' <param name="item">ImapDataItem. The imap message to prepare.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Prepare(ByVal item As IDataItem)
        Dim imapItem As ImapDataItem = item
        Dim tempFile As String = IO.Path.Combine(Me.DownloadPath, String.Format("{0}.eml", imapItem.UID))

        Log.DebugFormat("Downloading {0} to {1}", item.Name, tempFile)

        Using client As New IMAP_Client
            client.Connect(Me.Uri.Host, Me.Uri.Port, IIf(Me.Uri.Scheme = SCHEME_IMAPS, True, False))
            If client.IsConnected Then
                client.Authenticate(Me.Credentials.UserName, Me.Credentials.Password)

                If client.IsAuthenticated Then
                    Dim folder As String = Me.GetFolderName(Me.Uri, client.GetFolderSeparator)

                    client.SelectFolder(folder)
                    Dim m As New LumiSoft.Net.IMAP.IMAP_BODY
                    Using stream As FileStream = File.Create(tempFile)
                        client.FetchMessage(imapItem.UID, stream)
                        imapItem.LocalFile = New FileInfo(tempFile)
                    End Using
                End If
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Scans the specified IMAP folder for new email messages to process.
    ''' </summary>
    ''' <returns>Collection(Of ImapDataItem)</returns>
    ''' <remarks></remarks>
    Public Overrides Function Scan() As Collection(Of IDataItem)
        Dim items As New Collection(Of IDataItem)

        Using client As New IMAP_Client
            client.Connect(Me.Uri.Host, Me.Uri.Port, IIf(Me.Uri.Scheme = SCHEME_IMAPS, True, False))
            If client.IsConnected Then
                client.Authenticate(Me.Credentials.UserName, Me.Credentials.Password)

                If client.IsAuthenticated Then
                    Dim folder As String = Me.GetFolderName(Me.Uri, client.GetFolderSeparator)

                    Log.DebugFormat("Selecting Folder: {0}", folder)
                    client.SelectFolder(folder)

                    Dim seq As New IMAP_SequenceSet
                    seq.Parse("1:*")

                    Dim messages() As IMAP_FetchItem = client.FetchMessages(seq, IMAP_FetchItem_Flags.UID Or IMAP_FetchItem_Flags.MessageFlags, False, True)
                    Log.DebugFormat("Found {0} messages", messages.Length)

                    For Each item As IMAP_FetchItem In messages
                        Log.DebugFormat("Message Flags: {0}", item.MessageFlags)

                        If (item.MessageFlags And IMAP_MessageFlags.Deleted) <> IMAP_MessageFlags.Deleted Then
                            items.Add(New ImapDataItem(New Uri(String.Format("{0}/;UID={1}", Me.Uri, item.UID))))
                        End If
                    Next
                End If
            End If
        End Using

        Return items
    End Function

    ''' <summary>
    ''' Scrubs imap/imaps uris to set default ports and folders
    ''' </summary>
    ''' <param name="value">Uri. The Uri, CompleteUri or FailureUri to prepare.</param>
    ''' <returns></returns>
    ''' <remarks>If no port is specified, it will be set to 143/993 for imap/imaps. If no path is specified, /INDEX will be set as the default.</remarks>
    Protected Overrides Function PrepareUri(ByVal value As Uri) As Uri
        Dim builder As New UriBuilder(MyBase.PrepareUri(value))

        If builder.Port < 1 Then
            Select Case value.Scheme
                Case SCHEME_IMAPS
                    Log.DebugFormat("Adding default port {0}/{1}", value.Scheme, PORT_IMAPS)

                    builder.Port = PORT_IMAPS
                Case Else
                    Log.DebugFormat("Adding default port {0}/{1}", value.Scheme, PORT_IMAP)

                    builder.Port = PORT_IMAP
            End Select
        End If

        If builder.Path = "/" Then
            Log.DebugFormat("Adding default folder {0}", DEFAULT_FOLDER)

            builder.Path += DEFAULT_FOLDER
        End If

        Return builder.Uri
    End Function

    ''' <summary>
    ''' Gets the imap folder name from the uri and converts it to the specified directory separator.
    ''' </summary>
    ''' <param name="uri">Uri. The uri containing the folder name.</param>
    ''' <param name="folderSeparator">String. The folder separator used on the imap server.</param>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Protected Overridable Function GetFolderName(ByVal uri As Uri, ByVal folderSeparator As String) As String
        Dim folder As String = uri.AbsolutePath.Substring(1)
        Dim name As String = folder.Replace("/", folderSeparator)

        Log.DebugFormat("Devined folder {0} from {1}", name, uri.AbsoluteUri)

        Return name
    End Function

    ''' <summary>
    ''' Returns value indicating if the given uri scheme is supported or not.
    ''' </summary>
    ''' <param name="uri">Uri. The uri to validate.</param>
    ''' <returns>Boolean. True of the uri scheme is supported. False otherwise.</returns>
    ''' <remarks>Currently, only imap/imaps is supported.</remarks>
    Protected Overrides Function IsSchemeSupported(ByVal uri As Uri) As Boolean
        Select Case uri.Scheme
            Case SCHEME_IMAP, SCHEME_IMAPS
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>
    ''' Validates the current monitors configuration for errors before processing/starting.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overrides Sub Validate()
        Log.Debug("Validating monitor configuration")

        MyBase.Validate()

        If Me.Credentials Is Nothing Then
            Throw New ApplicationException("Credentials required for this monitor")
        ElseIf (Me.ProcessCompleteActions And DataActions.Rename) <> DataActions.None Or (Me.ProcessFailureActions And DataActions.Rename) <> DataActions.None Then
            Throw New NotImplementedException("Rename is not supported for this monitor.")
        End If
    End Sub
End Class
