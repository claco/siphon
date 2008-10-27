Imports System.Timers
Imports log4net

''' <summary>
''' Base class for all DataMonitor based classes.
''' </summary>
''' <remarks></remarks>
Public MustInherit Class DataMonitor
    Implements IDataMonitor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private _disposed As Boolean = False
    Private _processor As IDataProcessor = Nothing
    Private _schedule As IMonitorSchedule = Nothing
    Private WithEvents _timer As System.Timers.Timer

    ''' <summary>
    ''' Creates a new DataMonitor instance.
    ''' </summary>
    ''' <param name="processor">IDataProcessor. The processor to use to handle newly found data.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to detemrin when to look for new data.</param>
    ''' <remarks></remarks>
    Protected Sub New(ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        Me.Schedule = schedule
        Me.Processor = processor
    End Sub

    ''' <summary>
    ''' Gets/sets the processor to handle newly found data.
    ''' </summary>
    ''' <value></value>
    ''' <returns>IDataProcessor</returns>
    ''' <remarks></remarks>
    Public Overridable Property Processor() As IDataProcessor Implements IDataMonitor.Processor
        Get
            Return _processor
        End Get
        Set(ByVal value As IDataProcessor)
            _processor = value
        End Set
    End Property

    ''' <summary>
    ''' Scans for new data and sends new data to the current processor.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustOverride Sub Scan() Implements IDataMonitor.Scan

    ''' <summary>
    ''' Starts monitoring for new data.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Start() Implements IDataMonitor.Start
        Log.InfoFormat("Starting Monitor")

        Dim start As DateTime = DateTime.Now
        Dim nextEvent As DateTime = Me.Schedule.NextEvent(start)

        Log.InfoFormat("Start: {0}", start)
        Log.InfoFormat("Next data check at {0}", nextEvent)

        Timer.Interval = (nextEvent - start).TotalMilliseconds
        Timer.Start()
    End Sub

    ''' <summary>
    ''' Stops monitoring for new data.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub [Stop]() Implements IDataMonitor.Stop
        Log.InfoFormat("Stopping Monitor")
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

    ''' <summary>
    ''' Disposes the current monitor instance.
    ''' </summary>
    ''' <param name="disposing">Boolean. Flag for if we are disposing or in GC.</param>
    ''' <remarks></remarks>
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not _disposed Then
            If disposing Then
                Me.Stop()
            End If
        End If

        _disposed = True
    End Sub

    ''' <summary>
    ''' Disposes the current monitor instance.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Private Sub _timer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles _timer.Elapsed
        Log.Debug("Timer Elapsed")

        Me.Stop()
        Me.Scan()
        Me.Start()
    End Sub
End Class
