Imports System.Configuration
Imports System.Text.RegularExpressions
Imports log4net

''' <summary>
''' Monitors a directory for new files.
''' </summary>
''' <remarks></remarks>
Public MustInherit Class DirectoryMonitor
    Inherits DataMonitor
    Implements IDirectoryMonitor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private Const DEFAULT_FILTER As String = "*"
    Private Const CONFIG_SETTING_PATH As String = "Path"
    Private Const CONFIG_SETTING_FILTER As String = "Filter"

    Private _createMissingFolders As Boolean
    Private _filter As String = DEFAULT_FILTER
    Private _uri As Uri = Nothing
    Private _completeUri As Uri = Nothing
    Private _failureUri As Uri = Nothing

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
    Protected Sub New(ByVal name As String, ByVal path As String, ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        MyBase.New(name, schedule, processor)
        Me.Path = path
    End Sub

    ''' <summary>
    ''' Creates missing folders before starting the timer.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustOverride Sub CreateFolders()

    ''' <summary>
    ''' Gets/sets flag determining whether to create any missing folders when starting the monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Boolean</returns>
    ''' <remarks></remarks>
    Public Overridable Property CreateMissingFolders() As Boolean
        Get
            Return _createMissingFolders
        End Get
        Set(ByVal value As Boolean)
            _createMissingFolders = value
        End Set
    End Property

    ''' <summary>
    ''' Initializes the monitor using the supplied monitor configuration settings.
    ''' </summary>
    ''' <param name="config">MonitorElement. The configuraiton for the current monitor.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Initialize(ByVal config As MonitorElement)
        MyBase.Initialize(config)

        Dim path As NameValueConfigurationElement = config.Settings(CONFIG_SETTING_PATH)
        If path Is Nothing OrElse String.IsNullOrEmpty(path.Value) Then
            Throw New ConfigurationErrorsException(String.Format("The setting {0} is required", CONFIG_SETTING_PATH))
        Else
            Me.Path = path.Value
        End If

        Dim filter As NameValueConfigurationElement = config.Settings(CONFIG_SETTING_FILTER)
        If Not filter Is Nothing Then
            Me.Filter = filter.Value
        End If
    End Sub

    ''' <summary>
    ''' Gets/sets the file filter to apply to the directory.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overridable Property Filter() As String Implements IDirectoryMonitor.Filter
        Get
            Return _filter
        End Get
        Set(ByVal value As String)
            If String.IsNullOrEmpty(value) Then
                _filter = DEFAULT_FILTER
            Else
                _filter = value.Trim
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the full path to the directory to monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks>The path can be a local file system path, or a file uri path.</remarks>
    Public Overridable Property Path() As String Implements IDirectoryMonitor.Path
        Get
            If Me.Uri Is Nothing Then
                Return String.Empty
            Else
                Return Me.Uri.LocalPath
            End If
        End Get
        Set(ByVal value As String)
            If String.IsNullOrEmpty(value) Then
                Throw New ArgumentException("Path can not be null or empty")
            Else
                Dim parsedUri As Uri = Nothing

                If Uri.TryCreate(value, UriKind.RelativeOrAbsolute, parsedUri) Then
                    Log.DebugFormat("Path: {0} converted to {1}", value, parsedUri.AbsoluteUri)

                    Me.Uri = parsedUri
                Else
                    Me.Uri = New Uri(value)
                End If
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the full path as a uri of the directory to monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Uri</returns>
    ''' <remarks></remarks>
    Public Property Uri() As Uri Implements IDirectoryMonitor.Uri
        Get
            Return _uri

        End Get
        Set(ByVal value As Uri)
            If IsSchemeSupported(value) Then
                _uri = value
            Else
                Throw New UriFormatException("Scheme not supported")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Starts the directory monitor, creating any missing directories if configured.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub Start()
        If Me.CreateMissingFolders Then
            Me.CreateFolders()
        End If

        MyBase.Start()
    End Sub

    ''' <summary>
    ''' Gets/sets the path where files that fail processing should be moved to if the DataActions.Move action is set. 
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Property CompletePath() As String Implements IDirectoryMonitor.CompletePath
        Get
            Dim uri As Uri = Me.CompleteUri
            If uri Is Nothing Then
                Return Nothing
            Else
                Return uri.LocalPath
            End If
        End Get
        Set(ByVal value As String)
            If Regex.IsMatch(value, Uri.SchemeDelimiter) Then
                Dim uri As New Uri(value)

                Log.DebugFormat("CompletePath: {0} converted to {1}", value, uri.ToString)

                Me.CompleteUri = uri
            Else
                Dim builder As New UriBuilder(Me.Uri)
                builder.Path = IO.Path.Combine(builder.Path, value)

                Log.DebugFormat("CompletePath: {0} converted to {1}", value, builder.Uri.ToString)

                Me.CompleteUri = builder.Uri
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets the uri where files that process successfully should be moved to if the DataActions.Move aciton is set.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Uri</returns>
    ''' <remarks></remarks>
    Public Property CompleteUri() As Uri Implements IDirectoryMonitor.CompleteUri
        Get
            Return _completeUri
        End Get
        Set(ByVal value As Uri)
            If IsSchemeSupported(value) Then
                _completeUri = Me.VerifyDriveLEtter(value)
            Else
                Throw New UriFormatException("Scheme not supported")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the path where files that process successfully should be moved to if the DataActions.Move aciton is set.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FailurePath() As String Implements IDirectoryMonitor.FailurePath
        Get
            Dim uri As Uri = Me.FailureUri
            If uri Is Nothing Then
                Return Nothing
            Else
                Return uri.LocalPath
            End If
        End Get
        Set(ByVal value As String)
            If Regex.IsMatch(value, Uri.SchemeDelimiter) Then
                Dim uri As New Uri(value)

                Log.DebugFormat("FailurePath: {0} converted to {1}", value, uri.ToString)

                Me.CompleteUri = uri
            Else
                Dim builder As New UriBuilder(Me.Uri)
                builder.Path = IO.Path.Combine(builder.Path, value)

                Log.DebugFormat("FailurePath: {0} converted to {1}", value, builder.Uri.ToString)

                Me.FailureUri = builder.Uri
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the uri where files that fail processing should be moved to if the DataActions.Move action is set. 
    ''' </summary>
    ''' <value></value>
    ''' <returns>Uri</returns>
    ''' <remarks></remarks>
    Public Property FailureUri() As Uri Implements IDirectoryMonitor.FailureUri
        Get
            Return _failureUri
        End Get
        Set(ByVal value As Uri)
            If IsSchemeSupported(value) Then
                _failureUri = Me.VerifyDriveLEtter(value)
            Else
                Throw New UriFormatException("Scheme not supported")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Returns value indicating if the given uri scheme is supported or not.
    ''' </summary>
    ''' <param name="uri">Uri. The uri to validate.</param>
    ''' <returns>Boolean. True of the uri scheme is supported. False otherwise.</returns>
    ''' <remarks>Currently, only the file, ftp, http and https schemes are supported.</remarks>
    Protected Function IsSchemeSupported(ByVal uri As Uri) As Boolean
        Select Case uri.Scheme
            Case uri.UriSchemeFile, uri.UriSchemeFtp, uri.UriSchemeHttp, uri.UriSchemeHttps
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>
    ''' Verifies that a file uri is rooted to a drive and assigns it to the main Path/Uris drive if it isn't.
    ''' </summary>
    ''' <param name="value">Uri. The file uri being verified.</param>
    ''' <returns>Uri</returns>
    ''' <remarks></remarks>
    Private Function VerifyDriveLEtter(ByVal value As Uri) As Uri
        If value.Scheme = uri.UriSchemeFile Then
            If Not Regex.IsMatch(value.Segments(1), "(\:|\$)") Then
                REM this will fix absolute file uris without drive letters
                REM assume drive from main Path/Uri
                Dim builder As New UriBuilder(value)
                builder.Path = IO.Path.Combine(Me.Uri.Segments(1), Regex.Replace(value.LocalPath, "\A(\\|\/)", ""))

                Log.DebugFormat("CompleteUri: Rooting file uri from {0} to {1}", value.ToString, builder.Uri.ToString)
                value = builder.Uri
            End If
        End If

        Return value
    End Function

End Class