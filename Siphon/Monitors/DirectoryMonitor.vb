﻿Imports log4net
Imports ChrisLaco.Siphon.Monitors
Imports ChrisLaco.Siphon.Schedules
Imports ChrisLaco.Siphon.Processors

Namespace Monitors
    ''' <summary>
    ''' Monitors a directory for new files.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class DirectoryMonitor
        Inherits DataMonitor
        Implements IDirectoryMonitor

        Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
        Private Const DEFAULT_FILTER As String = "*"
        Private _createMissingFolders As Boolean
        Private _filter As String = DEFAULT_FILTER
        Private _path As String = String.Empty

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
    End Class
End Namespace