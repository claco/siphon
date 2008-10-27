''' <summary>
''' Interface that defines a data processor instance.
''' </summary>
''' <remarks></remarks>
Public Interface IDataProcessor
    Inherits IDisposable

    ''' <summary>
    ''' Processes new data found by the monitor.
    ''' </summary>
    ''' <param name="data">Object. New data to be processed.</param>
    ''' <remarks>Returns True if the data was processed successfully. Returns False otherwise.</remarks>
    Function Process(ByVal data As Object) As Boolean
End Interface
