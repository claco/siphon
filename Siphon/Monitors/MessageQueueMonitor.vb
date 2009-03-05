Imports System.Configuration
Imports System.Collections.ObjectModel
Imports System.Messaging
Imports log4net

''' <summary>
''' Class for monitoring Microsoft Message Queues
''' </summary>
''' <remarks></remarks>
Public Class MessageQueueMonitor
    Inherits DataMonitor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private Const SETTING_QUEUE As String = "Queue"
    Private Const SETTING_COMPLETE_QUEUE As String = "CompleteQueue"
    Private Const SETTING_FAILURE_QUEUE As String = "FailureQueue"
    Private Const SETTINGS_CREATE_MISSING_QUEUES As String = "CreateMissingQueues"

    Private _createMissingQueues As Boolean
    Private _queue As MessageQueue
    Private _completeQueue As MessageQueue
    Private _failureQueue As MessageQueue

    ''' <summary>
    ''' Protected constructor for reflection.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new message queue monitor.
    ''' </summary>
    ''' <param name="name">String. The friendly name for the monitor.</param>
    ''' <param name="queue">MessageQueue. The queue to be monitored.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to monitor the directory.</param>
    ''' <param name="processor">IDataProcessor. The data processor to use to process new files.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, ByVal queue As MessageQueue, ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        MyBase.New(name, schedule, processor)
        Me.Queue = queue
    End Sub

    ''' <summary>
    ''' Creates a new message queue monitor.
    ''' </summary>
    ''' <param name="name">String. The friendly name for the monitor.</param>
    ''' <param name="path">String. The full path to the queue to be monitored.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to monitor the directory.</param>
    ''' <param name="processor">IDataProcessor. The data processor to use to process new files.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, ByVal path As String, ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        MyBase.New(name, schedule, processor)

        If String.IsNullOrEmpty(path) Then
            Throw New ArgumentException("Path can not be null or empty")
        Else
            Me.Queue = New MessageQueue(path)
        End If
    End Sub

    ''' <summary>
    ''' Gets/sets flag determining whether to create any missing queues when starting the monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Boolean</returns>
    ''' <remarks></remarks>
    Public Overridable Property CreateMissingQueues() As Boolean
        Get
            Return _createMissingQueues
        End Get
        Set(ByVal value As Boolean)
            _createMissingQueues = value
        End Set
    End Property

    ''' <summary>
    ''' Create any missing queues during start.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub CreateQueues()
        If Not MessageQueue.Exists(Me.Queue.Path) Then
            Log.DebugFormat("Creating queue {0}", Me.Queue.Path)

            Me.Queue = MessageQueue.Create(Me.Queue.Path)
        End If
        If Me.CompleteQueue IsNot Nothing Then
            If Not MessageQueue.Exists(Me.CompleteQueue.Path) Then
                Log.DebugFormat("Creating queue {0}", Me.CompleteQueue.Path)

                Me.CompleteQueue = MessageQueue.Create(Me.CompleteQueue.Path)
            End If
        End If
        If Me.FailureQueue IsNot Nothing Then
            If Not MessageQueue.Exists(Me.FailureQueue.Path) Then
                Log.DebugFormat("Creating queue {0}", Me.FailureQueue.Path)

                Me.FailureQueue = MessageQueue.Create(Me.FailureQueue.Path)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Initializes the monitor using the supplied monitor configuration settings.
    ''' </summary>
    ''' <param name="config">MonitorElement. The configuration for the current monitor.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Initialize(ByVal config As MonitorElement)
        MyBase.Initialize(config)

        Dim settings As NameValueConfigurationCollection = config.Settings
        If settings.AllKeys.Contains(SETTING_QUEUE) Then
            Me.Queue = New MessageQueue(settings(SETTING_QUEUE).Value)
        End If
        If settings.AllKeys.Contains(SETTING_COMPLETE_QUEUE) Then
            Me.CompleteQueue = New MessageQueue(settings(SETTING_COMPLETE_QUEUE).Value)
        End If
        If settings.AllKeys.Contains(SETTING_FAILURE_QUEUE) Then
            Me.FailureQueue = New MessageQueue(settings(SETTING_FAILURE_QUEUE).Value)
        End If
        If settings.AllKeys.Contains(SETTINGS_CREATE_MISSING_QUEUES) Then
            Me.CreateMissingQueues = settings(SETTINGS_CREATE_MISSING_QUEUES).Value
        End If
    End Sub

    ''' <summary>
    ''' Gets/sets the mesage queue to use for the current monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>MessageQueue</returns>
    ''' <remarks></remarks>
    Public Overridable Property Queue() As MessageQueue
        Get
            Return _queue
        End Get
        Set(ByVal value As MessageQueue)
            If value Is Nothing Then
                Throw New ArgumentException("Queue can not be null or empty")
            Else
                _queue = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the mesage queue to use for the completed items.
    ''' </summary>
    ''' <value></value>
    ''' <returns>MessageQueue</returns>
    ''' <remarks></remarks>
    Public Overridable Property CompleteQueue() As MessageQueue
        Get
            Return _completeQueue
        End Get
        Set(ByVal value As MessageQueue)
            _completeQueue = value
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the mesage queue to use for the failed items.
    ''' </summary>
    ''' <value></value>
    ''' <returns>MessageQueue</returns>
    ''' <remarks></remarks>
    Public Overridable Property FailureQueue() As MessageQueue
        Get
            Return _failureQueue
        End Get
        Set(ByVal value As MessageQueue)
            _failureQueue = value
        End Set
    End Property

    ''' <summary>
    ''' Scans the specified directory for new files matching the specified filter.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Function Scan() As System.Collections.ObjectModel.Collection(Of IDataItem)
        Log.DebugFormat("Scanning {0}", Me.Queue.Path)

        Dim messages() As Message = Me.Queue.GetAllMessages
        Dim items As New Collection(Of IDataItem)
        Log.DebugFormat("Found {0} messages in {1}", messages.Length, Me.Queue.Path)

        For Each message As Message In messages
            items.Add(New QueueMessageDataItem(message))
        Next

        Return items
    End Function

    ''' <summary>
    ''' Starts the queue monitor, creating any missing queues if configured.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub Start()
        Me.Validate()

        If Me.CreateMissingQueues Then
            Me.CreateQueues()
        End If

        MyBase.Start()
    End Sub

    ''' <summary>
    ''' Deletes the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to delete.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Delete(ByVal item As IDataItem)
        Dim message As IDataItem(Of Message) = item
        Me.Queue.ReceiveById(message.Data.Id)
    End Sub

    ''' <summary>
    ''' Moves the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to move.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Move(ByVal item As IDataItem)
        Dim message As IDataItem(Of Message) = item

        If item.Status = DataItemStatus.CompletedProcessing Then
            Log.DebugFormat("Moving {0} to {1}", message.Data.Label, Me.CompleteQueue.Path)

            Me.CompleteQueue.Send(Me.Queue.ReceiveById(message.Data.Id))
        ElseIf item.Status = DataItemStatus.FailedProcessing Then
            Log.DebugFormat("Moving {0} to {1}", message.Data.Label, Me.FailureQueue.Path)

            Me.FailureQueue.Send(Me.Queue.ReceiveById(message.Data.Id))
        Else
            Throw New NotImplementedException("Unknown Item Status")
        End If
    End Sub

    ''' <summary>
    ''' Renames the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to renamed.</param>
    ''' <remarks>Always throws a NotImplementedException</remarks>
    Public Overrides Sub Rename(ByVal item As IDataItem)
        Throw New NotImplementedException
    End Sub

    ''' <summary>
    ''' Updates the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to update.</param>
    ''' <remarks>Always throws a NotImplementedException</remarks>
    Public Overrides Sub Update(ByVal item As IDataItem)
        Throw New NotImplementedException
    End Sub

    ''' <summary>
    ''' Validates the current monitors configuration for errors before processing/starting.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overrides Sub Validate()
        Log.Debug("Validating monitor configuration")

        MyBase.Validate()

        If Me.Queue Is Nothing Then
            Throw New ApplicationException("No Queue is not defined")
        ElseIf (Me.ProcessCompleteActions And DataActions.Rename) <> DataActions.None Or (Me.ProcessFailureActions And DataActions.Rename) <> DataActions.None Then
            Throw New NotImplementedException("Rename is not supported for this monitor.")
        ElseIf Me.CompleteQueue Is Nothing And (Me.ProcessCompleteActions And DataActions.Move) <> DataActions.None Then
            Throw New ApplicationException("CompleteQueue must be defined to use the Move action on process completion.")
        ElseIf Me.FailureQueue Is Nothing And (Me.ProcessFailureActions And DataActions.Move) <> DataActions.None Then
            Throw New ApplicationException("FailureQueue must be defined to use the Move action on process failure.")
        End If
    End Sub
End Class