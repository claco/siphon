Imports System.Collections.ObjectModel
Imports ChrisLaco.Siphon.Configuration
Imports ChrisLaco.Siphon.Monitors
Imports ChrisLaco.Siphon.Schedules
Imports ChrisLaco.Siphon.Processors

Namespace Monitors
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
        Function Scan() As Collection(Of Object)
    End Interface
End Namespace