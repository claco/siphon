Imports System.Collections.ObjectModel
Imports System.IO
Imports log4net

Public Class FtpDirectoryMonitor
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

    End Sub

    ''' <summary>
    ''' Scans the specified directory for new files mathcing the specified filter.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Function Scan() As Collection(Of IDataItem)
        Log.DebugFormat("Scanning {0} for {1}", Me.Path, Me.Filter)

        Return New System.Collections.ObjectModel.Collection(Of IDataItem)
    End Function

    ''' <summary>
    ''' Deletes the data item after processing.
    ''' </summary>
    ''' <param name="data">IDataItem. The item to delete.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Delete(ByVal data As IDataItem)
        Throw New NotImplementedException
    End Sub

    ''' <summary>
    ''' Renames the data item after processing.
    ''' </summary>
    ''' <param name="data">IDataItem. The item to renamed.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Rename(ByVal data As IDataItem)
        Throw New NotImplementedException
    End Sub
End Class
