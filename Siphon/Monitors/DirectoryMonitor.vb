Imports System.IO
Imports log4net

''' <summary>
''' Monitors a directory for new files.
''' </summary>
''' <remarks></remarks>
Public Class DirectoryMonitor
    Inherits DataMonitor
    Implements IDirectoryMonitor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private Const DEFAULT_FILTER As String = "*"
    Private _filter As String = DEFAULT_FILTER
    Private _path As String = String.Empty

    ''' <summary>
    ''' Creates a new directory monitor.
    ''' </summary>
    ''' <param name="path">String. The full path to the directory to be monitored.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to monitor the directory.</param>
    ''' <param name="processor">IDataProcessor. The data processor to use to process new files.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal path As String, ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        MyBase.New(schedule, processor)
        Me.Path = path
    End Sub

    ''' <summary>
    ''' Scans the specified directory for new files mathcing the specified filter.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub Scan()
        Log.DebugFormat("Scanning {0} for {1}", Me.Path, Me.Filter)

        Dim files() As String = Directory.GetFiles(Me.Path, Me.Filter, SearchOption.TopDirectoryOnly)
        Log.DebugFormat("Found {0} files in {1}", files.Length, Me.Path)

        For Each file As String In files
            Log.DebugFormat("Processing {0}", file)
            Me.Processor.Process(file)
        Next
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
    ''' <remarks></remarks>
    Public Overridable Property Path() As String Implements IDirectoryMonitor.Path
        Get
            Return _path
        End Get
        Set(ByVal value As String)
            _path = value.Trim
        End Set
    End Property
End Class
