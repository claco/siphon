Imports System.Collections.ObjectModel

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
    ''' <param name="data">IDataItem. The item to delete.</param>
    ''' <remarks></remarks>
    Sub Delete(ByVal data As IDataItem)

    ''' <summary>
    ''' Renames the data item after processing.
    ''' </summary>
    ''' <param name="data">IDataItem. The item to renamed.</param>
    ''' <remarks></remarks>
    Sub Rename(ByVal data As IDataItem)

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
End Interface
