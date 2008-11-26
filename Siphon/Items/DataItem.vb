''' <summary>
''' Class containing a data item to be processed.
''' </summary>
''' <remarks></remarks>
Public Class DataItem(Of T)
    Implements IDataItem(Of T)

    Private _item As T = Nothing
    Private _name As String = String.Empty

    ''' <summary>
    ''' Creates a new DataItem instance.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new DataItem instance.
    ''' </summary>
    ''' <param name="name">String. The friendly name of the item being processed.</param>
    ''' <param name="item">Object. The item being processed.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, ByVal item As T)
        _name = name
        _item = item
    End Sub

    ''' <summary>
    ''' Gets the contents of the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Object</returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property GetString() As String Implements IDataItem.GetString
        Get
            Return _item.ToString
        End Get
    End Property

    ''' <summary>
    ''' Gets the the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>T</returns>
    ''' <remarks></remarks>
    Public Overloads ReadOnly Property Item() As T Implements IDataItem(Of T).Item
        Get
            Return _item
        End Get
    End Property


    ''' <summary>
    ''' Gets the friendly name of the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property Name() As String Implements IDataItem.Name
        Get
            Return _name
        End Get
    End Property

End Class
