Imports System.Collections.ObjectModel
Imports System.Configuration
Imports System.IO
Imports System.Net
Imports LumiSoft.Net.FTP
Imports LumiSoft.Net.FTP.Client
Imports log4net

Public Class FtpDirectoryMonitor
    Inherits RemoteDirectoryMonitor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private Const SCHEME_FTP As String = "ftp"
    Private Const SCHEME_FTPS As String = "ftps"
    Private Const PORT_FTP As Integer = 21
    Private Const PORT_FTPS As Integer = 990
    Private Const SETTING_PASSIVE As String = "Passive"

    Private _client As New FTP_Client
    Private _passive As Boolean = True

    ''' <summary>
    ''' Protected constructor for reflection.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new directory monitor.
    ''' </summary>
    ''' <param name="name">String. The friendly name for the monitor.</param>
    ''' <param name="path">String. The full path to the directory to be monitored.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to monitor the directory.</param>
    ''' <param name="processor">IDataProcessor. The data processor to use to process new files.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, ByVal path As String, ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        MyBase.New(name, path, schedule, processor)
    End Sub

    ''' <summary>
    ''' Initializes the monitor using the supplied monitor configuration settings.
    ''' </summary>
    ''' <param name="config">MonitorElement. The configuration for the current monitor.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Initialize(ByVal config As MonitorElement)
        MyBase.Initialize(config)

        Dim settings As NameValueConfigurationCollection = config.Settings
        If settings.AllKeys.Contains(SETTING_PASSIVE) Then
            Me.Passive = settings(SETTING_PASSIVE).Value
        End If
    End Sub

    ''' <summary>
    ''' Gets the ftp client instance for this monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>FTP_Client</returns>
    ''' <remarks></remarks>
    Protected ReadOnly Property Client() As FTP_Client
        Get
            Return _client
        End Get
    End Property

    ''' <summary>
    ''' Gets/sets the credentials to use to connect to the ftp server.
    ''' </summary>
    ''' <value></value>
    ''' <returns>NetworkCredential</returns>
    ''' <remarks>If no credentials are set in code or in config, the user "anonymous" will be sent.</remarks>
    Public Overrides Property Credentials() As System.Net.NetworkCredential
        Get
            If MyBase.Credentials Is Nothing Then
                Me.Credentials = New NetworkCredential("anonymous", "anonymous")
            End If

            Return MyBase.Credentials
        End Get
        Set(ByVal value As System.Net.NetworkCredential)
            MyBase.Credentials = value
        End Set
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
            Client.Connect(Me.Uri.Host, Me.Uri.Port, IIf(Me.Uri.Scheme = SCHEME_FTPS, True, False))
            If Client.IsConnected Then
                Client.Authenticate(Me.Credentials.UserName, Me.Credentials.Password)

                If Client.IsAuthenticated Then
                    Client.TransferMode = IIf(Me.Passive, FTP_TransferMode.Passive, FTP_TransferMode.Active)

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
            Log.Error(String.Format("Error disconnecting from {0}, Me.Uri") ex)
        Finally
            Me.IsConnected = False
        End Try
    End Sub

    ''' <summary>
    ''' Create any missing folders during start.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub CreateFolders()
        MyBase.CreateFolders()

        If Me.CreateMissingFolders And (Not String.IsNullOrEmpty(Me.Uri.AbsolutePath.TrimStart("/")) Or Me.CompleteUri IsNot Nothing Or Me.FailureUri IsNot Nothing) Then
            Try
                If Me.Connect Then
                    If Not String.IsNullOrEmpty(Me.Uri.AbsolutePath.TrimStart("/")) Then
                        Log.DebugFormat("Creating directory {0}", Me.Uri)

                        Try
                            Me.Client.CreateDirectory(Me.Uri.AbsolutePath)
                        Catch ex As Exception
                            Log.Error(String.Format("Error creating {0}", Me.Uri), ex)
                        End Try
                    End If

                    If Me.CompleteUri IsNot Nothing Then
                        Log.DebugFormat("Creating directory {0}", Me.CompleteUri)

                        Try
                            Me.Client.CreateDirectory(Me.CompleteUri.AbsolutePath)
                        Catch ex As Exception
                            Log.Error(String.Format("Error creating {0}", Me.CompleteUri), ex)
                        End Try
                    End If

                    If Me.FailureUri IsNot Nothing Then
                        Log.DebugFormat("Creating directory {0}", Me.FailureUri)

                        Try
                            Me.Client.CreateDirectory(Me.FailureUri.AbsolutePath)
                        Catch ex As Exception
                            Log.Error(String.Format("Error creating {0}", Me.FailureUri), ex)
                        End Try
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
    ''' Scans the specified directory for new files mathcing the specified filter.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Function Scan() As Collection(Of IDataItem)
        Log.DebugFormat("Scanning {0} for {1}", Me.Uri, Me.Filter)

        Dim items As New System.Collections.ObjectModel.Collection(Of IDataItem)

        Try
            If Me.Connect Then
                Dim uri As New Uri(IO.Path.Combine(Me.Uri.AbsoluteUri, Me.Filter))
                Dim list() As FTP_ListItem = Client.GetList(uri.AbsolutePath)

                For Each item As FTP_ListItem In list
                    If item.IsFile Then
                        Dim fileUri As Uri = New Uri(IO.Path.Combine(Me.Uri.AbsoluteUri, item.Name))

                        Log.DebugFormat("Found remote file {0}", fileUri.AbsoluteUri)

                        items.Add(New UriDataItem(fileUri))
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
    ''' Downloads the remote file to the hard drive for processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to prepare.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Prepare(ByVal item As IDataItem)
        Dim uriItem As UriDataItem = item
        Dim remoteFile As New FileInfo(uriItem.Data.LocalPath)
        Dim tempFile As String = IO.Path.Combine(Me.DownloadPath, remoteFile.Name)

        Log.DebugFormat("Downloading {0} to {1}", item.Name, tempFile)

        Try
            If Me.Connect Then
                Client.GetFile(uriItem.Data.AbsolutePath, tempFile)

                uriItem.LocalFile = New FileInfo(tempFile)
                uriItem.Name += " (" & tempFile & ")"
            Else
                Log.ErrorFormat("Could not connect to {0}", Me.Uri)
            End If
        Catch ex As Exception
            Log.Error(String.Format("Error downloading {0}", uriItem.Data), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Deletes the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to delete.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Delete(ByVal item As IDataItem)
        Dim uriItem As UriDataItem = item

        Log.DebugFormat("Deleting {0}", uriItem.Data)

        Try
            If Me.Connect Then
                Client.DeleteFile(uriItem.Data.AbsolutePath)
            Else
                Log.ErrorFormat("Could not connect to {0}", Me.Uri)
            End If
        Catch ex As Exception
            Log.Error(String.Format("Error deleting {0}", uriItem.Data), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Moves the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to move.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Move(ByVal item As IDataItem)
        Dim uriItem As UriDataItem = item
        Dim remoteFile As New FileInfo(uriItem.Data.LocalPath)
        Dim newFile As Uri

        If uriItem.Status = DataItemStatus.CompletedProcessing Then
            newFile = New Uri(IO.Path.Combine(Me.CompleteUri.AbsoluteUri, remoteFile.Name))
        ElseIf uriItem.Status = DataItemStatus.FailedProcessing Then
            newFile = New Uri(IO.Path.Combine(Me.FailureUri.AbsoluteUri, remoteFile.Name))
        Else
            Throw New NotImplementedException("Unknown Item Status")
        End If

        Log.DebugFormat("Moving {0} to {1}", uriItem.Data, newFile)

        Try
            If Me.Connect Then
                Client.Rename(uriItem.Data.AbsolutePath, newFile.AbsolutePath)
            Else
                Log.ErrorFormat("Could not connect to {0}", Me.Uri)
            End If
        Catch ex As Exception
            Log.Error(String.Format("Error moving {0}", uriItem.Data), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Renames the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to renamed.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Rename(ByVal item As IDataItem)
        Dim uriItem As UriDataItem = item
        Dim remoteFile As New FileInfo(uriItem.Data.LocalPath)
        Dim newFile As String = Me.GetNewFileName(remoteFile.Name)
        Dim newUri As New UriBuilder(uriItem.Data)
        newUri.Path = newUri.Path.Replace(remoteFile.Name, newFile)

        Log.DebugFormat("Renaming {0} to {1}", uriItem.Data, newUri.Uri)

        Try
            If Me.Connect Then
                Client.Rename(uriItem.Data.AbsolutePath, newUri.Uri.AbsolutePath)

                uriItem.Data = newUri.Uri
            Else
                Log.ErrorFormat("Could not connect to {0}", Me.Uri)
            End If
        Catch ex As Exception
            Log.Error(String.Format("Error renaming {0}", uriItem.Data), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Updates the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The data item to update.</param>
    ''' <remarks>This method always returns NotSupportedException</remarks>
    Public Overrides Sub Update(ByVal item As IDataItem)
        Throw New NotSupportedException
    End Sub

    ''' <summary>
    ''' Returns value indicating if the given uri scheme is supported or not.
    ''' </summary>
    ''' <param name="uri">Uri. The uri to validate.</param>
    ''' <returns>Boolean. True of the uri scheme is supported. False otherwise.</returns>
    ''' <remarks>Currently, only ftp is supported.</remarks>
    Protected Overrides Function IsSchemeSupported(ByVal uri As Uri) As Boolean
        Select Case uri.Scheme
            Case uri.UriSchemeFtp
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>
    ''' Gets/sets whether ftp should use passive or active mode.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Boolean. True to use Passive mode. False to use Active mode.</returns>
    ''' <remarks></remarks>
    Public Property Passive()
        Get
            Return _passive
        End Get
        Set(ByVal value)
            _passive = value
        End Set
    End Property

    ''' <summary>
    ''' Disposes the current FtpDataMonitor and the client if it is still connected.
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
