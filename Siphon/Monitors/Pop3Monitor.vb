Imports System.Collections.ObjectModel
Imports System.IO
Imports LumiSoft.Net.POP3.Client
Imports log4net

Public Class Pop3Monitor
    Inherits RemoteDirectoryMonitor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private Const SCHEME_POP As String = "pop"
    Private Const SCHEME_POPS As String = "pops"
    Private Const PORT_POP As Integer = 110
    Private Const PORT_POPS As Integer = 995

    Private _client As New POP3_Client

    ''' <summary>
    ''' Protected constructor for reflection.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new pop3 mailbox monitor.
    ''' </summary>
    ''' <param name="name">String. The friendly name for the monitor.</param>
    ''' <param name="path">String. The full path to the pop3 mailbox to be monitored.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to monitor the mailbox.</param>
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
    Protected ReadOnly Property Client() As POP3_Client
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
            Client.Connect(Me.Uri.Host, Me.Uri.Port, IIf(Me.Uri.Scheme = SCHEME_POPS, True, False))
            If Client.IsConnected Then
                Client.Authenticate(Me.Credentials.UserName, Me.Credentials.Password, True)

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
            Log.Error(String.Format("Error connecting to {0}", Me.Uri), ex)
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
            Log.Error(String.Format("Error disconnecting from {0}", Me.Uri), ex)
        Finally
            Me.IsConnected = False
        End Try
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
                Dim message As POP3_ClientMessage = Client.Messages(mailItem.UID)
                message.MarkForDeletion()
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
    ''' <remarks>This method always returns NotSupportedException</remarks>
    Public Overrides Sub Move(ByVal item As IDataItem)
        Throw New NotSupportedException
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
    ''' Scans the specified Pop3 mailbox for new email messages to process.
    ''' </summary>
    ''' <returns>Collection(Of MailDataItem)</returns>
    ''' <remarks></remarks>
    Public Overrides Function Scan() As Collection(Of IDataItem)
        Log.DebugFormat("Scanning {0} for {1}", Me.Uri, Me.Filter)

        Dim items As New Collection(Of IDataItem)

        Try
            If Me.Connect Then
                Dim messages As POP3_ClientMessageCollection = Client.Messages
                Log.DebugFormat("Found {0} messages", messages.Count)

                For Each item As POP3_ClientMessage In messages
                    If Not item.IsMarkedForDeletion Then
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
                Using stream As FileStream = File.Create(tempFile)
                    Dim message As POP3_ClientMessage = Client.Messages(mailItem.UID)

                    message.MessageToStream(stream)
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
    ''' Scrubs pop/pops uris to set default ports and folders
    ''' </summary>
    ''' <param name="value">Uri. The Uri, CompleteUri or FailureUri to prepare.</param>
    ''' <returns></returns>
    ''' <remarks>If no port is specified, it will be set to 143/993 for pop/pops. If no path is specified, /INDEX will be set as the default.</remarks>
    Protected Overrides Function PrepareUri(ByVal value As Uri) As Uri
        Dim builder As New UriBuilder(MyBase.PrepareUri(value))

        If builder.Port < 1 Then
            Select Case value.Scheme
                Case SCHEME_POPS
                    Log.DebugFormat("Adding default port {0}/{1}", value.Scheme, PORT_POPS)

                    builder.Port = PORT_POPS
                Case Else
                    Log.DebugFormat("Adding default port {0}/{1}", value.Scheme, PORT_POP)

                    builder.Port = PORT_POP
            End Select
        End If

        Return builder.Uri
    End Function

    ''' <summary>
    ''' Returns value indicating if the given uri scheme is supported or not.
    ''' </summary>
    ''' <param name="uri">Uri. The uri to validate.</param>
    ''' <returns>Boolean. True of the uri scheme is supported. False otherwise.</returns>
    ''' <remarks>Currently, only pop/pops is supported.</remarks>
    Protected Overrides Function IsSchemeSupported(ByVal uri As Uri) As Boolean
        Select Case uri.Scheme
            Case SCHEME_POP, SCHEME_POPS
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
