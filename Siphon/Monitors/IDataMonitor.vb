﻿Imports System.Collections.ObjectModel

''' <summary>
''' Interface that defines a data monitoring instance.
''' </summary>
''' <remarks></remarks>
Public Interface IDataMonitor
    Inherits IDisposable

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
    ''' Scans for new data and sends new data to the current processor.
    ''' </summary>
    ''' <remarks></remarks>
    Function Scan() As Collection(Of Object)
End Interface
