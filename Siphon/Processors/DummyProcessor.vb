''' <summary>
''' Dummy processor that performs no actions and always returns true.
''' </summary>
''' <remarks></remarks>
Public Class DummyProcessor
    Inherits DataProcessor

    ''' <summary>
    ''' Initializes the schedule with the specified configuration.
    ''' </summary>
    ''' <param name="config">ProcessorElement. The processor configuration containing processor settings.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Initialize(ByVal config As ProcessorElement)

    End Sub

    ''' <summary>
    ''' Processes new data found by the monitor.
    ''' </summary>
    ''' <param name="data">Object. New data to be processed.</param>
    ''' <remarks>This processor performs no actions and always returns True.</remarks>
    Public Overrides Function Process(ByVal data As IDataItem) As Boolean
        Return True
    End Function
End Class
