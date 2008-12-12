Imports System.Collections.ObjectModel
Imports System.IO
Imports log4net

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
                Dim root As String = Me.Path

                Log.DebugFormat("Creating directory {0}", root)

                Directory.CreateDirectory(root)
            End If

            If Not String.IsNullOrEmpty(Me.CompletePath) Then
                Dim complete As String = Me.CompleteUri.LocalPath

                If Not Directory.Exists(complete) Then
                    Log.DebugFormat("Creating directory {0}", complete)

                    Directory.CreateDirectory(complete)
                End If
            End If

            If Not String.IsNullOrEmpty(Me.FailurePath) Then
                Dim failure As String = Me.FailureUri.LocalPath

                If Not Directory.Exists(failure) Then
                    Log.DebugFormat("Creating directory {0}", failure)

                    Directory.CreateDirectory(failure)
                End If
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

        For Each file As String In files
            items.Add(New FileDataItem(file))
        Next

        Return items
    End Function

    ''' <summary>
    ''' Deletes the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to delete.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Delete(ByVal item As IDataItem)
        Dim file As IDataItem(Of FileInfo) = item

        Log.DebugFormat("Deleting {0}", file.Data.FullName)

        file.Data.Delete()
    End Sub

    ''' <summary>
    ''' Moves the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to move.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Move(ByVal item As IDataItem)
        Dim file As IDataItem(Of FileInfo) = item
        Dim path As String = String.Empty

        If item.Status = DataItemStatus.CompletedProcessing Then
            path = Me.CompletePath
        ElseIf item.Status = DataItemStatus.FailedProcessing Then
            path = Me.FailurePath
        Else
            Throw New NotImplementedException("Unknown Item Status")
        End If

        Dim moved As String = IO.Path.Combine(path, file.Data.Name)
        file.Data.MoveTo(moved)
        file.Data = New FileInfo(moved)

        Log.DebugFormat("Moving {0} to {1}", file.Data.FullName, path)
    End Sub

    ''' <summary>
    ''' Renames the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to renamed.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Rename(ByVal item As IDataItem)
        Dim file As IDataItem(Of FileInfo) = item
        Dim original As String = file.Data.Name
        Dim name As String = Me.GetNewFileName(original)
        Dim renamed As String = IO.Path.Combine(file.Data.Directory.FullName, name)

        file.Data.MoveTo(renamed)
        file.Data = New FileInfo(renamed)

        Log.DebugFormat("Renaming {0} to {1}", original, name)
    End Sub

    ''' <summary>
    ''' Returns value indicating if the given uri scheme is supported or not.
    ''' </summary>
    ''' <param name="uri">Uri. The uri to validate.</param>
    ''' <returns>Boolean. True of the uri scheme is supported. False otherwise.</returns>
    ''' <remarks>Currently, only file is supported.</remarks>
    Protected Overrides Function IsSchemeSupported(ByVal uri As Uri) As Boolean
        Select Case uri.Scheme
            Case uri.UriSchemeFile
                Return True
            Case Else
                Return False
        End Select
    End Function
End Class
