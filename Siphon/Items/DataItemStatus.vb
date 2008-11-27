''' <summary>
''' The status of the data item.
''' </summary>
''' <remarks></remarks>
<Flags()> _
Public Enum DataItemStatus
    ''' <summary>
    ''' The item is nw and has no yet been processed.
    ''' </summary>
    ''' <remarks></remarks>
    [New]

    ''' <summary>
    ''' The item was processed successfully.
    ''' </summary>
    ''' <remarks></remarks>
    CompletedProcessing

    ''' <summary>
    ''' The item could not be processed.
    ''' </summary>
    ''' <remarks></remarks>
    FailedProcessing
End Enum
