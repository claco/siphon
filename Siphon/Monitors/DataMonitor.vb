Imports System.Collections.ObjectModel
Imports System.Threading
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
    Private _name As String = String.Empty
    Private _processing As Boolean = False
    Private _processor As IDataProcessor = Nothing
    Private _schedule As IMonitorSchedule = Nothing
    Private _eventWaitHandle As EventWaitHandle = New ManualResetEvent(False)
    Private _timer As Threading.Timer = Nothing

    ''' <summary>
    ''' Creates a new DataMonitor instance.
    ''' </summary>
    ''' <param name="name">String. The friendly name for the monitor.</param>
    ''' <param name="processor">IDataProcessor. The processor to use to handle newly found data.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to detemrin when to look for new data.</param>
    ''' <remarks></remarks>
    Protected Sub New(ByVal name As String, ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        Me.Name = name.Trim
        Me.Schedule = schedule
        Me.Processor = processor
    End Sub

    ''' <summary>
    ''' Gets/sets the friendly name of the monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overridable Property Name() As String Implements IDataMonitor.Name
        Get
            Return _name
        End Get
        Set(ByVal value As String)
            _name = value.Trim
        End Set
    End Property

    ''' <summary>
    ''' Gets flag indicating if the current monitor is processing new data.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Boolean. True if the monitor is processing new data. False otherwise.</returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property Processing() As Boolean
        Get
            Return _processing
        End Get
    End Property

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
    Public MustOverride Function Scan() As Collection(Of Object) Implements IDataMonitor.Scan

    ''' <summary>
    ''' Starts monitoring for new data.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Start() Implements IDataMonitor.Start
        Log.InfoFormat("Starting Monitor {0}", Me.Name)

        Me.Timer.Change(Me.GetNextInterval, Timeout.Infinite)
    End Sub

    ''' <summary>
    ''' Pauses data monitoring, usually while processing files.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Pause() Implements IDataMonitor.Pause
        Log.InfoFormat("Pausing Monitor {0}", Me.Name)

        Me.Timer.Change(Timeout.Infinite, Timeout.Infinite)
    End Sub

    ''' <summary>
    ''' Resumes data monitors, usually after processing new data.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub [Resume]() Implements IDataMonitor.Resume
        Log.InfoFormat("Resuming Monitor {0}", Me.Name)

        Me.Timer.Change(Me.GetNextInterval, Timeout.Infinite)
    End Sub

    ''' <summary>
    ''' Stops monitoring for new data.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub [Stop]() Implements IDataMonitor.Stop
        Log.InfoFormat("Stopping Monitor {0}", Me.Name)

        If Me.Processing Then
            Log.Debug("Waiting for processot to finish")
            Me.EventWaitHandle.WaitOne()
            Log.Debug("Done waiting for processor to finish")
        End If
    End Sub

    ''' <summary>
    ''' Returns the internal event wait handle used for timer thread sync.
    ''' </summary>
    ''' <value></value>
    ''' <returns>EventWaitHandle</returns>
    ''' <remarks></remarks>
    Protected Overridable ReadOnly Property EventWaitHandle() As EventWaitHandle
        Get
            Return _eventWaitHandle
        End Get
    End Property

    ''' <summary>
    ''' Returns the internal timer used for the current monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Timer</returns>
    ''' <remarks></remarks>
    Protected Overridable ReadOnly Property Timer() As Threading.Timer
        Get
            If _timer Is Nothing Then
                _timer = New Threading.Timer(New TimerCallback(AddressOf Me.OnTimerElapsed), Me, Timeout.Infinite, Timeout.Infinite)
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

    ''' <summary>
    ''' Gets the next event from the schedule and returns the next interval in milliseconds to be sent to the timer.
    ''' </summary>
    ''' <returns>Integer. The number of milliseconds to wait until scanning for new data.</returns>
    ''' <remarks></remarks>
    Protected Overridable Function GetNextInterval() As Integer
        Dim start As DateTime = DateTime.Now
        Dim nextEvent As DateTime = Me.Schedule.NextEvent(start)
        Dim interval As Integer = (nextEvent - start).TotalMilliseconds

        Log.DebugFormat("Monitor Start Time {0}", start)
        Log.DebugFormat("Next Monitor Scan {0}", nextEvent)
        Log.DebugFormat("Timer Interval {0}", interval)

        Return interval
    End Function

    ''' <summary>
    ''' Subroutine called when the timer time has elapsed.
    ''' </summary>
    ''' <param name="state"></param>
    ''' <remarks></remarks>
    Protected Overridable Sub OnTimerElapsed(ByVal state As Object)
        Log.Debug("Timer Elapsed")
        Me.Pause()

        Try
            Dim items As Collection(Of Object) = Me.Scan
            Log.DebugFormat("Scan returned {0} items", items.Count)

            If items.Count > 0 Then
                Log.DebugFormat("Pre Process Processing {0}", _processing)

                _processing = True

                For Each item As Object In items
                    Me.Processor.Process(item)
                Next

                _processing = False

                Log.DebugFormat("Post Process Processing {0}", _processing)
            End If
        Catch ex As Exception
            Log.Error("Exception", ex)
        Finally
            _eventWaitHandle.Set()
        End Try

        Me.Resume()
    End Sub
End Class
