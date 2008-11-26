''' <summary>
''' Interface representing a single data item being processed.
''' </summary>
''' <remarks></remarks>
Public Interface IDataItem

    ''' <summary>
    ''' Gets the contents of the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    ReadOnly Property GetString() As String

    ''' <summary>
    ''' Gets the friendly name of the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    ReadOnly Property Name() As String
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
    Overloads ReadOnly Property Item() As T


End Interface