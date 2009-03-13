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

    Private _client As New IMAP_Client

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
    ''' Gets the imap client instance for this monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>IMAP_Client</returns>
    ''' <remarks></remarks>
    Protected ReadOnly Property Client() As IMAP_Client
        Get
            Return _client
        End Get
    End Property

    ''' <summary>
    ''' Connects to the data source being monitored.
    ''' </summary>
    ''' <returns>Boolean</returns>
    ''' <remarks>Returns True if the connection was established. Returns False is the connection failed.</remarks>
    Public Overrides Function Connect() As Boolean
        If Me.IsConnected AndAlso Client.IsConnected Then
            Return True
        End If
        Log.DebugFormat("Connecting to {0}", Me.Uri)

        Try
            Client.Connect(Me.Uri.Host, Me.Uri.Port, IIf(Me.Uri.Scheme = SCHEME_IMAPS, True, False))
            If Client.IsConnected Then
                Client.Authenticate(Me.Credentials.UserName, Me.Credentials.Password)

                If Client.IsAuthenticated Then
                    Me.IsConnected = True
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        Catch ex As Exception
            Log.Error(String.Format("Error connecting to {0}", Me.Uri) ex)
        End Try
    End Function

    ''' <summary>
    ''' Disconnects from the data source being monitored.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub Disconnect()
        Log.DebugFormat("Disconnecting from {0}", Me.Uri)

        Try
            Client.Disconnect()
        Catch ex As Exception
            Log.Error(String.Format("Error disconnecting from {0}") ex)
        Finally
            Me.IsConnected = False
        End Try
    End Sub

    ''' <summary>
    ''' Creates missing folders before starting the timer.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub CreateFolders()
        MyBase.CreateFolders()

        If Me.CreateMissingFolders Then
            Try
                If Me.Connect Then
                    Dim folders() As String = Client.GetFolders()

                    For Each folderName As String In folders
                        Log.DebugFormat("Folder {0} Exists", folderName)
                    Next

                    Dim folder As String = Me.GetFolderName(Me.Uri, Client.GetFolderSeparator)
                    If Not folders.Contains(folder) Then
                        Log.DebugFormat("Creating folder {0}", Me.Uri)

                        Try
                            Client.CreateFolder(folder)
                        Catch ex As Exception
                            Log.Error(String.Format("Error creating {0}", Me.Uri), ex)
                        End Try
                    End If

                    If Me.CompleteUri IsNot Nothing Then
                        Dim completeFolder As String = Me.GetFolderName(Me.CompleteUri, Client.GetFolderSeparator)

                        If Not folders.Contains(completeFolder) Then
                            Log.DebugFormat("Creating folder {0}", Me.CompleteUri)

                            Try
                                Client.CreateFolder(completeFolder)
                            Catch ex As Exception
                                Log.Error(String.Format("Error creating {0}", Me.CompleteUri), ex)
                            End Try
                        End If
                    End If

                    If Me.FailureUri IsNot Nothing Then
                        Dim failureFolder As String = Me.GetFolderName(Me.FailureUri, Client.GetFolderSeparator)

                        If Not folders.Contains(failureFolder) Then
                            Log.DebugFormat("Creating folder {0}", Me.FailureUri)

                            Try
                                Client.CreateFolder(failureFolder)
                            Catch ex As Exception
                                Log.Error(String.Format("Error creating {0}", Me.FailureUri), ex)
                            End Try
                        End If
                    End If

                    Me.Disconnect()
                Else
                    Log.ErrorFormat("Could not connect to {0}", Me.Uri)
                End If
            Catch ex As Exception
                Log.Error("Error creating folders", ex)
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Deletes the data item after processing.
    ''' </summary>
    ''' <param name="item">MailDataItem. The mail message to delete.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Delete(ByVal item As IDataItem)
        Dim mailItem As MailDataItem = item

        Log.DebugFormat("Deleting {0}", mailItem.Data)

        Try
            If Me.Connect Then
                Dim folder As String = Me.GetFolderName(Me.Uri, Client.GetFolderSeparator)

                Client.SelectFolder(folder)

                Dim seq As New IMAP_SequenceSet
                seq.Parse(mailItem.UID)

                Client.DeleteMessages(seq, True)
            Else
                Log.ErrorFormat("Could not connect to {0}", Me.Uri)
            End If
        Catch ex As Exception
            Log.Error(String.Format("Error deleting {0}", mailItem.Data), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Moves the data item after processing.
    ''' </summary>
    ''' <param name="item">MailDataItem. The mail message to move.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Move(ByVal item As IDataItem)
        Dim mailItem As MailDataItem = item

        Try
            If Me.Connect Then
                Dim folder As String = Me.GetFolderName(Me.Uri, Client.GetFolderSeparator)

                Client.SelectFolder(folder)

                Dim seq As New IMAP_SequenceSet
                seq.Parse(mailItem.UID)

                Dim destination As String = String.Empty
                If mailItem.Status = DataItemStatus.CompletedProcessing Then
                    Log.DebugFormat("Moving {0} to {1}", mailItem.Data, Me.CompleteUri)

                    destination = Me.GetFolderName(Me.CompleteUri, Client.GetFolderSeparator)
                ElseIf mailItem.Status = DataItemStatus.FailedProcessing Then
                    Log.DebugFormat("Moving {0} to {1}", mailItem.Data, Me.FailureUri)

                    destination = Me.GetFolderName(Me.FailureUri, Client.GetFolderSeparator)
                End If

                Client.MoveMessages(seq, destination, True)
            Else
                Log.ErrorFormat("Could not connect to {0}", Me.Uri)
            End If
        Catch ex As Exception
            Log.Error(String.Format("Error moving {0}", mailItem.Data), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Renames the data item after processing.
    ''' </summary>
    ''' <param name="item">MailDataItem. The mail message to rename.</param>
    ''' <remarks>This method always returns NotSupportedException</remarks>
    Public Overrides Sub Rename(ByVal item As IDataItem)
        Throw New NotSupportedException
    End Sub

    ''' <summary>
    ''' Updates the data item after processing.
    ''' </summary>
    ''' <param name="item">MailDataItem. The mail message to update.</param>
    ''' <remarks>This method always returns NotSupportedException</remarks>
    Public Overrides Sub Update(ByVal item As IDataItem)
        Throw New NotSupportedException
    End Sub

    ''' <summary>
    ''' Prepares the data before if it processed.
    ''' </summary>
    ''' <param name="item">MailDataItem. The mail message to prepare.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Prepare(ByVal item As IDataItem)
        Dim mailItem As MailDataItem = item
        Dim tempFile As String = IO.Path.Combine(Me.DownloadPath, String.Format("{0}.eml", mailItem.UID))

        Log.DebugFormat("Downloading {0} to {1}", item.Name, tempFile)

        Try
            If Me.Connect Then
                Dim folder As String = Me.GetFolderName(Me.Uri, Client.GetFolderSeparator)

                Client.SelectFolder(folder)
                Dim m As New LumiSoft.Net.IMAP.IMAP_BODY
                Using stream As FileStream = File.Create(tempFile)
                    Client.FetchMessage(mailItem.UID, stream)
                    mailItem.LocalFile = New FileInfo(tempFile)
                End Using
            Else
                Log.ErrorFormat("Could not connect to {0}", Me.Uri)
            End If
        Catch ex As Exception
            Log.Error(String.Format("Error downloading {0}", item.Name), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Scans the specified IMAP folder for new email messages to process.
    ''' </summary>
    ''' <returns>Collection(Of ImapDataItem)</returns>
    ''' <remarks></remarks>
    Public Overrides Function Scan() As Collection(Of IDataItem)
        Log.DebugFormat("Scanning {0} for {1}", Me.Uri, Me.Filter)

        Dim items As New Collection(Of IDataItem)

        Try
            If Me.Connect Then
                Dim folder As String = Me.GetFolderName(Me.Uri, Client.GetFolderSeparator)

                Log.DebugFormat("Selecting Folder: {0}", folder)
                Client.SelectFolder(folder)

                Dim seq As New IMAP_SequenceSet
                seq.Parse("1:*")

                Dim messages() As IMAP_FetchItem = Client.FetchMessages(seq, IMAP_FetchItem_Flags.UID Or IMAP_FetchItem_Flags.MessageFlags, False, True)
                Log.DebugFormat("Found {0} messages", messages.Length)

                For Each item As IMAP_FetchItem In messages
                    Log.DebugFormat("Message Flags: {0}", item.MessageFlags)

                    If (item.MessageFlags And IMAP_MessageFlags.Deleted) <> IMAP_MessageFlags.Deleted Then
                        items.Add(New MailDataItem(Me.Uri, item.UID))
                    End If
                Next
            Else
                Log.ErrorFormat("Could not connect to {0}", Me.Uri)
            End If
        Catch ex As Exception
            Log.Error(String.Format("Error scanning {0}", Me.Uri), ex)
        End Try

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
        End If
    End Sub

    ''' <summary>
    ''' Disposes the current DataMonitor and the client if it is still connected.
    ''' </summary>
    ''' <param name="disposing">Boolean. True if we're disposing. False if we're in the GC.</param>
    ''' <remarks></remarks>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        MyBase.Dispose(disposing)

        If disposing Then
            If Client.IsConnected Then
                Client.Disconnect()
            End If
            Client.Dispose()
        End If
    End Sub
End Class
