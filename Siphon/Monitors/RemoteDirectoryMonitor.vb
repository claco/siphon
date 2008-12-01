Imports System.Configuration
Imports System.Text.RegularExpressions
Imports log4net

''' <summary>
''' Monitors a remote directory for new files.
''' </summary>
''' <remarks></remarks>
Public MustInherit Class RemoteDirectoryMonitor
    Inherits DirectoryMonitor
    Implements IRemoteDirectoryMonitor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Private _downloadUri As Uri = Nothing

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
        MyBase.New(name, path, schedule, processor)
    End Sub

    ''' <summary>
    ''' Gets/sets the path where files that download successfully should be placed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Property DownloadPath() As String Implements IRemoteDirectoryMonitor.DownloadPath
        Get
            Dim uri As Uri = Me.DownloadUri
            If uri Is Nothing Then
                Return Nothing
            Else
                Return uri.LocalPath
            End If
        End Get
        Set(ByVal value As String)
            If Regex.IsMatch(value, Uri.SchemeDelimiter) Then
                Dim uri As New Uri(value)

                Log.DebugFormat("DownloadPath: {0} converted to {1}", value, uri.ToString)

                Me.DownloadUri = uri
            Else
                Dim builder As New UriBuilder(Me.Uri)
                builder.Path = IO.Path.Combine(builder.Path, value)

                Log.DebugFormat("DownloadPath: {0} converted to {1}", value, builder.Uri.ToString)

                Me.DownloadUri = builder.Uri
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the uri where files that download successfully should placed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Uri</returns>
    ''' <remarks></remarks>
    Public Property DownloadUri() As System.Uri Implements IRemoteDirectoryMonitor.DownloadUri
        Get
            Return _downloadUri
        End Get
        Set(ByVal value As Uri)
            If value.Scheme = Uri.UriSchemeFile Then
                _downloadUri = Me.VerifyDriveLetter(value)
            Else
                Throw New UriFormatException("Scheme not supported")
            End If
        End Set
    End Property
End Class
