''' <summary>
''' Interface that defines a data processor instance.
''' </summary>
''' <remarks></remarks>
Public Interface IDataProcessor
    Inherits IDisposable

    ''' <summary>
    ''' Initializes the processor with the specified config settings.
    ''' </summary>
    ''' <param name="config">ProcessorElement. The processor configuraton element contianing related settings.</param>
    ''' <remarks></remarks>
    Sub Initialize(ByVal config As ProcessorElement)

    ''' <summary>
    ''' Processes new data found by the monitor.
    ''' </summary>
    ''' <param name="data">IDataItem. New data to be processed.</param>
    ''' <remarks>Returns True if the data was processed successfully. Returns False otherwise.</remarks>
    Function Process(ByVal data As IDataItem) As Boolean
End Interface
