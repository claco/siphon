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
    Private _createMissingQueues As Boolean
    Private _queue As MessageQueue

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
        Me.Queue = New MessageQueue(path)
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
            _queue = value
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
        If Me.CreateMissingQueues Then
            Me.CreateQueues()
        End If

        MyBase.Start()
    End Sub
End Class