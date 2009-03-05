Imports System.Collections.ObjectModel
Imports System.Configuration
Imports System.Net
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

    Private Const SETTING_USERNAME As String = "Username"
    Private Const SETTING_PASSWORD As String = "Password"
    Private Const SETTING_DOMAIN As String = "Domain"

    Private _credentials As NetworkCredential = Nothing
    Private _disposed As Boolean
    Private _name As String = String.Empty
    Private _processing As Boolean
    Private _processFailureActions As DataActions = DataActions.None
    Private _processCompleteActions As DataActions = DataActions.None
    Private _processor As IDataProcessor = Nothing
    Private _schedule As IMonitorSchedule = Nothing
    Private _eventWaitHandle As EventWaitHandle = New ManualResetEvent(False)
    Private _timer As Threading.Timer = Nothing

    ''' <summary>
    ''' Protected constructor for reflection.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub New()

    End Sub

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
    ''' Gets/sets the credentials to use for the data source.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Property Credentials() As NetworkCredential Implements IDataMonitor.Credentials
        Get
            Return _credentials
        End Get
        Set(ByVal value As NetworkCredential)
            _credentials = value
        End Set
    End Property

    ''' <summary>
    ''' Initializes the monitor using the supplied monitor configuration settings.
    ''' </summary>
    ''' <param name="config">MonitorElement. The configuraiton for the current monitor.</param>
    ''' <remarks></remarks>
    Public Overridable Sub Initialize(ByVal config As MonitorElement) Implements IDataMonitor.Initialize
        Dim userName As String = String.Empty
        Dim password As String = String.Empty
        Dim domain As String = String.Empty

        Dim settings As NameValueConfigurationCollection = config.Settings
        If settings.AllKeys.Contains(SETTING_USERNAME) Then
            userName = settings(SETTING_USERNAME).Value

            If settings.AllKeys.Contains(SETTING_PASSWORD) Then
                password = settings(SETTING_PASSWORD).Value
            End If
            If settings.AllKeys.Contains(SETTING_DOMAIN) Then
                domain = settings(SETTING_DOMAIN).Value
            End If

            Me.Credentials = New NetworkCredential(userName, password, domain)
        End If

        Me.Schedule = config.Schedule.CreateInstance
        Me.Processor = config.Processor.CreateInstance
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
            If String.IsNullOrEmpty(value) Then
                Throw New ArgumentException("Name can not be null or empty")
            Else
                _name = value.Trim
            End If
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
    Public MustOverride Function Scan() As Collection(Of IDataItem) Implements IDataMonitor.Scan

    ''' <summary>
    ''' Starts monitoring for new data.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Start() Implements IDataMonitor.Start
        Log.InfoFormat("Starting Monitor {0}", Me.Name)

        Me.Validate()
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

        Me.Validate()

        Try
            Me.Timer.Change(Me.GetNextInterval, Timeout.Infinite)
        Catch ex As ObjectDisposedException
            REM If processing is going after main thread is done
        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Stops monitoring for new data.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub [Stop]() Implements IDataMonitor.Stop
        Log.InfoFormat("Stopping Monitor {0}", Me.Name)

        If Me.Processing Then
            Log.Debug("Waiting for processor to finish")
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
                _timer = New Threading.Timer(New TimerCallback(AddressOf Me.OnScheduledEvent), Me, Timeout.Infinite, Timeout.Infinite)
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
            _disposed = True

            If disposing Then
                Me.Stop()
                Me.Timer.Dispose()
            End If
        End If
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
        Log.DebugFormat("Next Monitor Event {0}", nextEvent)
        Log.DebugFormat("Timer Interval {0}", interval)

        Return interval
    End Function

    ''' <summary>
    ''' Scans a for new data and sends that data to the processor.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Process() Implements IDataMonitor.Process
        Try
            Me.Validate()

            Dim items As Collection(Of IDataItem) = Me.Scan
            Log.DebugFormat("Scan returned {0} items", items.Count)

            If items.Count > 0 Then
                Log.DebugFormat("Pre Process Processing {0}", _processing)

                _processing = True

                For Each item As IDataItem In items
                    Dim prepared As Boolean = False
                    Dim processed As Boolean = False

                    Try
                        Me.Prepare(item)
                        prepared = True
                    Catch ex As Exception
                        Log.Error(String.Format("Error preparing {0}", item.Name), ex)
                    End Try

                    If Not prepared Then
                        Log.DebugFormat("Skipping {0}", item.Name)
                    Else
                        Try
                            processed = Me.Processor.Process(item)
                        Catch ex As Exception
                            Log.Debug(String.Format("Processing {0} failed", item.Name), ex)
                        End Try
                        Log.DebugFormat("Processed: {0}", processed)

                        If processed Then
                            item.Status = DataItemStatus.CompletedProcessing
                            Me.OnProcessComplete(New ProcessEventArgs(item))
                        Else
                            item.Status = DataItemStatus.FailedProcessing
                            Me.OnProcessFailure(New ProcessEventArgs(item))
                        End If
                    End If
                Next

                _processing = False

                Log.DebugFormat("Post Process Processing {0}", _processing)
            End If
        Catch ex As Exception
            Log.Error("Exception", ex)
        Finally
            _eventWaitHandle.Set()
        End Try
    End Sub

    ''' <summary>
    ''' Subroutine called when the timer time has elapsed, reaching the scheduled event.
    ''' </summary>
    ''' <param name="state"></param>
    ''' <remarks></remarks>
    Protected Overridable Sub OnScheduledEvent(ByVal state As Object)
        If String.IsNullOrEmpty(Thread.CurrentThread.Name) Then
            Thread.CurrentThread.Name = String.Format("{0}TimerThread", state.Name)
        End If

        Log.Debug("Timer Elapsed")
        Me.Pause()
        Me.Process()
        Me.Resume()
    End Sub

    ''' <summary>
    ''' Gets/sets the actions to perform when data processing fails.
    ''' </summary>
    ''' <value></value>
    ''' <returns>DataActions</returns>
    ''' <remarks></remarks>
    Public Overridable Property ProcessFailureActions() As DataActions Implements IDataMonitor.ProcessFailureActions
        Get
            Return _processFailureActions
        End Get
        Set(ByVal value As DataActions)
            _processFailureActions = value
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the actions to perform when data processing succeeds.
    ''' </summary>
    ''' <value></value>
    ''' <returns>DataActions</returns>
    ''' <remarks></remarks>
    Public Overridable Property ProcessCompleteActions() As DataActions Implements IDataMonitor.ProcessCompleteActions
        Get
            Return _processCompleteActions
        End Get
        Set(ByVal value As DataActions)
            _processCompleteActions = value
        End Set
    End Property

    ''' <summary>
    ''' Deletes the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to delete.</param>
    ''' <remarks></remarks>
    Public MustOverride Sub Delete(ByVal item As IDataItem) Implements IDataMonitor.Delete

    ''' <summary>
    ''' Moves the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to move.</param>
    ''' <remarks></remarks>
    Public MustOverride Sub Move(ByVal item As IDataItem) Implements IDataMonitor.Move

    ''' <summary>
    ''' Renames the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to renamed.</param>
    ''' <remarks></remarks>
    Public MustOverride Sub Rename(ByVal item As IDataItem) Implements IDataMonitor.Rename

    ''' <summary>
    ''' Updates the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to updated.</param>
    ''' <remarks></remarks>
    Public MustOverride Sub Update(ByVal item As IDataItem) Implements IDataMonitor.Update

    ''' <summary>
    ''' Prepares the data before if it processed.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to prepare.</param>
    ''' <remarks></remarks>
    Public Overridable Sub Prepare(ByVal item As IDataItem) Implements IDataMonitor.Prepare

    End Sub

    ''' <summary>
    ''' Validates the current monitors configuration for errors before processing/starting.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overridable Sub Validate()
        Log.Debug("Validating monitor configuration")

        If String.IsNullOrEmpty(Me.Name) Then
            Throw New ApplicationException("Name can not be null or empty")
        End If
    End Sub

#Region "Events"

    ''' <summary>
    ''' Performs process failure actions when processing fails.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overridable Sub OnProcessFailure(ByVal e As ProcessEventArgs)
        Log.Debug("On Process Failure")

        If (Me.ProcessFailureActions And DataActions.Delete) <> DataActions.None Then
            Log.InfoFormat("Deleting {0}", e.Item.Name)

            Try
                Me.Delete(e.Item)
            Catch ex As Exception
                Log.Error(ex)
            End Try
        Else
            If (Me.ProcessFailureActions And DataActions.Update) <> DataActions.None Then
                Log.InfoFormat("Updating {0}", e.Item.Name)

                Try
                    Me.Update(e.Item)
                Catch ex As Exception
                    Log.Error(ex)
                End Try
            End If
            If (Me.ProcessFailureActions And DataActions.Rename) <> DataActions.None Then
                Log.InfoFormat("Renaming {0}", e.Item.Name)

                Try
                    Me.Rename(e.Item)
                Catch ex As Exception
                    Log.Error(ex)
                End Try
            End If
            If (Me.ProcessFailureActions And DataActions.Move) <> DataActions.None Then
                Log.InfoFormat("Moving {0}", e.Item.Name)

                Try
                    Me.Move(e.Item)
                Catch ex As Exception
                    Log.Error(ex)
                End Try
            End If
        End If

        RaiseEvent ProcessFailure(Me, e)
    End Sub

    ''' <summary>
    ''' Performs process complete actions when processing succeeds.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overridable Sub OnProcessComplete(ByVal e As ProcessEventArgs)
        Log.Debug("On Process Complete")

        If (Me.ProcessCompleteActions And DataActions.Delete) <> DataActions.None Then
            Log.InfoFormat("Deleting {0}", e.Item.Name)

            Try
                Me.Delete(e.Item)
            Catch ex As Exception
                Log.Error(ex)
            End Try
        Else
            If (Me.ProcessCompleteActions And DataActions.Update) <> DataActions.None Then
                Log.InfoFormat("Updating {0}", e.Item.Name)

                Try
                    Me.Update(e.Item)
                Catch ex As Exception
                    Log.Error(ex)
                End Try
            End If
            If (Me.ProcessCompleteActions And DataActions.Rename) <> DataActions.None Then
                Log.InfoFormat("Renaming {0}", e.Item.Name)

                Try
                    Me.Rename(e.Item)
                Catch ex As Exception
                    Log.Error(ex)
                End Try
            End If
            If (Me.ProcessCompleteActions And DataActions.Move) <> DataActions.None Then
                Log.InfoFormat("Moving {0}", e.Item.Name)

                Try
                    Me.Move(e.Item)
                Catch ex As Exception
                    Log.Error(ex)
                End Try
            End If
        End If

        RaiseEvent ProcessComplete(Me, e)
    End Sub

    ''' <summary>
    ''' Event fires when data processing fails.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event ProcessFailure(ByVal sender As Object, ByVal e As ProcessEventArgs) Implements IDataMonitor.ProcessFailure

    ''' <summary>
    ''' Event fires when data processing completes successfully.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event ProcessComplete(ByVal sender As Object, ByVal e As ProcessEventArgs) Implements IDataMonitor.ProcessComplete

#End Region

End Class
