''' <summary>
''' Interface representing a single data item.
''' </summary>
''' <remarks></remarks>
Public Interface IDataItem

    ''' <summary>
    ''' Gets the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Object</returns>
    ''' <remarks></remarks>
    ReadOnly Property Item() As Object

    ''' <summary>
    ''' Gets the contents of the data item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Object</returns>
    ''' <remarks></remarks>
    ReadOnly Property Contents() As Object

    ''' <summary>
    ''' Gets the name od the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    ReadOnly Property Name() As String

End Interface
