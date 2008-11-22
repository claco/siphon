''' <summary>
''' Class containing a data item to be processed.
''' </summary>
''' <remarks></remarks>
Public Class DataItem
    Implements IDataItem

    Private _item As Object = Nothing
    Private _contents As Object = Nothing
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
    ''' <param name="contents">Object. The contents of the item being processed.</param>
    ''' <param name="item">Object. The item being processed.</param>
    ''' <param name="name">String. The friendly name of the item being processed.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal item As Object, ByVal contents As Object, ByVal name As String)
        _contents = contents
        _item = item
        _name = name
    End Sub

    ''' <summary>
    ''' Gets the contents of the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Object</returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property Contents() As Object Implements IDataItem.Contents
        Get
            Return _contents
        End Get
    End Property

    ''' <summary>
    ''' Gets the the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Object</returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property Item() As Object Implements IDataItem.Item
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
