Imports System.Collections.ObjectModel
Imports System.IO
Imports log4net
Imports ChrisLaco.Siphon.Monitors
Imports ChrisLaco.Siphon.Schedules
Imports ChrisLaco.Siphon.Processors

Namespace Monitors
    Public Class LocalDirectoryMonitor
        Inherits DirectoryMonitor

        Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

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
        ''' Create any missing folders during start.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub CreateFolders()
            If Me.CreateMissingFolders Then
                If Not Directory.Exists(Me.Path) Then
                    Log.DebugFormat("Creating directory {0}", Me.Path)

                    Directory.CreateDirectory(Me.Path)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Scans the specified directory for new files matching the specified filter.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Function Scan() As Collection(Of IDataItem)
            Log.DebugFormat("Scanning {0} for {1}", Me.Path, Me.Filter)

            Dim files() As String = Directory.GetFiles(Me.Path, Me.Filter, SearchOption.TopDirectoryOnly)
            Dim items As New Collection(Of IDataItem)
            Log.DebugFormat("Found {0} files in {1}", files.Length, Me.Path)

            For Each File As String In files
                items.Add(New FileDataItem(File))
            Next

            Return items
        End Function
    End Class
End Namespace