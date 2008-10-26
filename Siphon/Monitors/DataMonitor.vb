Imports System.Timers

''' <summary>
''' Base class for all DataMonitor based classes.
''' </summary>
''' <remarks></remarks>
Public MustInherit Class DataMonitor
    Implements IDataMonitor

    Private _disposed As Boolean = False
    Private _processor As IDataProcessor = Nothing
    Private _schedule As IMonitorSchedule = Nothing
    Private _timer As System.Timers.Timer

    Public Sub New(ByVal processor As IDataProcessor, ByVal schedule As IMonitorSchedule)
        Me.Processor = processor
        Me.Schedule = schedule
    End Sub

    Public Overridable Property Processor() As IDataProcessor Implements IDataMonitor.Processor
        Get
            Return _processor
        End Get
        Set(ByVal value As IDataProcessor)
            _processor = value
        End Set
    End Property

    Public Overridable Sub Start() Implements IDataMonitor.Start
        Timer.Start()
    End Sub

    Public Overridable Sub [Stop]() Implements IDataMonitor.Stop
        Timer.Stop()
    End Sub

    ''' <summary>
    ''' Returns the internal timer used for the current monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Timer</returns>
    ''' <remarks></remarks>
    Protected Overridable ReadOnly Property Timer() As Timer
        Get
            If _timer Is Nothing Then
                _timer = New Timer
                _timer.AutoReset = False
            End If

            Return _timer
        End Get
    End Property

    ''' <summary>
    ''' Gets/sets the schedule for the current monitor instance.
    ''' </summary>
    ''' <value></value>
    ''' <returns>IMonitorSchedule</returns>
    ''' <remarks></remarks>
    Public Overridable Property Schedule() As IMonitorSchedule Implements IDataMonitor.Schedule
        Get
            Return _schedule
        End Get
        Set(ByVal value As IMonitorSchedule)
            _schedule = value
        End Set
    End Property

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not _disposed Then
            If disposing Then

            End If
        End If

        _disposed = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

End Class
