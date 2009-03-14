Imports System.Collections.ObjectModel
Imports System.Net

''' <summary>
''' Interface that defines a data monitoring instance.
''' </summary>
''' <remarks></remarks>
Public Interface IDataMonitor
    Inherits IDisposable

    ''' <summary>
    ''' Initializes the monitor using the supplied monitor configuration settings.
    ''' </summary>
    ''' <param name="config">MonitorElement. The configuraiton for the current monitor.</param>
    ''' <remarks></remarks>
    Sub Initialize(ByVal config As MonitorElement)

    ''' <summary>
    ''' Gets/sets the friendly name of the monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Property Name() As String

    ''' <summary>
    ''' Gets/sets the credentials to use for the data source.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Property Credentials() As NetworkCredential

    ''' <summary>
    ''' Starts the data monitoring instance.
    ''' </summary>
    ''' <remarks></remarks>
    Sub Start()

    ''' <summary>
    ''' Stops the data monitoring instance.
    ''' </summary>
    ''' <remarks></remarks>
    Sub [Stop]()

    ''' <summary>
    ''' Pauses data monitoring, usually while processing files.
    ''' </summary>
    ''' <remarks></remarks>
    Sub Pause()

    ''' <summary>
    ''' Resumes data monitors, usually after processing new data.
    ''' </summary>
    ''' <remarks></remarks>
    Sub [Resume]()

    ''' <summary>
    ''' Scans a for new data and sends that data to the processor.
    ''' </summary>
    ''' <remarks></remarks>
    Sub Process()

    ''' <summary>
    ''' Gets flag indicating if the current monitor is processing new data.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Boolean. True if the monitor is processing new data. False otherwise.</returns>
    ''' <remarks></remarks>
    ReadOnly Property IsProcessing() As Boolean

    ''' <summary>
    ''' Gets flag indicating if the current monitor is running/has been started.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Boolean. True if the monitor is running. False otherwise.</returns>
    ''' <remarks></remarks>
    ReadOnly Property IsRunning() As Boolean

    ''' <summary>
    ''' Gets/sets the data processor to use when new data is found.
    ''' </summary>
    ''' <value></value>
    ''' <returns>IDataProcessor</returns>
    ''' <remarks></remarks>
    Property Processor() As IDataProcessor

    ''' <summary>
    ''' Gets/sets the data monitor schedule to be used.
    ''' </summary>
    ''' <value></value>
    ''' <returns>IMonitorSchedule</returns>
    ''' <remarks></remarks>
    Property Schedule() As IMonitorSchedule

    ''' <summary>
    ''' Scans for and returns a collection of new data.
    ''' </summary>
    ''' <remarks></remarks>
    Function Scan() As Collection(Of IDataItem)

    ''' <summary>
    ''' Gets/sets the actions to perform when data processing fails.
    ''' </summary>
    ''' <value></value>
    ''' <returns>DataActions</returns>
    ''' <remarks></remarks>
    Property ProcessFailureActions() As DataActions

    ''' <summary>
    ''' Gets/sets the actions to perform when data processing succeeds.
    ''' </summary>
    ''' <value></value>
    ''' <returns>DataActions</returns>
    ''' <remarks></remarks>
    Property ProcessCompleteActions() As DataActions

    ''' <summary>
    ''' Deletes the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to delete.</param>
    ''' <remarks></remarks>
    Sub Delete(ByVal item As IDataItem)

    ''' <summary>
    ''' Moves the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to move.</param>
    ''' <remarks></remarks>
    Sub Move(ByVal item As IDataItem)

    ''' <summary>
    ''' Renames the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to renamed.</param>
    ''' <remarks></remarks>
    Sub Rename(ByVal item As IDataItem)

    ''' <summary>
    ''' Updates the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to be updated.</param>
    ''' <remarks></remarks>
    Sub Update(ByVal item As IDataItem)

    ''' <summary>
    ''' Prepares the data before if it processed.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to prepare.</param>
    ''' <remarks></remarks>
    Sub Prepare(ByVal item As IDataItem)

    ''' <summary>
    ''' Event fires when data processing fails.
    ''' </summary>
    ''' <remarks></remarks>
    Event ProcessFailure(ByVal sender As Object, ByVal e As ProcessEventArgs)

    ''' <summary>
    ''' Event fires when data processing completes successfully.
    ''' </summary>
    ''' <remarks></remarks>
    Event ProcessComplete(ByVal sender As Object, ByVal e As ProcessEventArgs)

    ''' <summary>
    ''' Connects to the data source being monitored.
    ''' </summary>
    ''' <returns>Boolean</returns>
    ''' <remarks>Returns True if the connection was established. Returns False is the connection failed.</remarks>
    Function Connect() As Boolean

    ''' <summary>
    ''' Gets value indicating if the monitor is connected to the data source being monitored.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Boolean</returns>
    ''' <remarks>True if connected. False otherwise</remarks>
    Property IsConnected() As Boolean

    ''' <summary>
    ''' Disconnects from the data source being monitored.
    ''' </summary>
    ''' <remarks></remarks>
    Sub Disconnect()

End Interface
