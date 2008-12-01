''' <summary>
''' Interface representing a single data item being processed.
''' </summary>
''' <remarks></remarks>
Public Interface IDataItem

    ''' <summary>
    ''' Gets the contents of the item being processed.
    ''' </summary>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Function GetString() As String

    ''' <summary>
    ''' Gets/sets the friendly name of the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Property Name() As String

    ''' <summary>
    ''' Gets/sets the status of the item.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Property Status() As DataItemStatus
End Interface

''' <summary>
''' Generics Interface representing a single data item being processed.
''' </summary>
''' <typeparam name="T"></typeparam>
''' <remarks></remarks>
Public Interface IDataItem(Of T)
    Inherits IDataItem

    ''' <summary>
    ''' Gets the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>T</returns>
    ''' <remarks></remarks>
    Property Data() As T
End Interface