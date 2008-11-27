''' <summary>
''' Class containing a data item to be processed.
''' </summary>
''' <remarks></remarks>
Public Class DataItem(Of T)
    Implements IDataItem(Of T)

    Private _data As T = Nothing
    Private _name As String = String.Empty
    Private _status As DataItemStatus = DataItemStatus.New

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
    ''' <param name="data">Object. The data being processed.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, ByVal data As T)
        _name = name
        _data = data
    End Sub

    ''' <summary>
    ''' Gets the contents of the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Object</returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property GetString() As String Implements IDataItem.GetString
        Get
            Return _data.ToString
        End Get
    End Property

    ''' <summary>
    ''' Gets the the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>T</returns>
    ''' <remarks></remarks>
    Public Overloads ReadOnly Property Data() As T Implements IDataItem(Of T).Data
        Get
            Return _data
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

    ''' <summary>
    ''' Gets/sets the status of the data item.
    ''' </summary>
    ''' <value></value>
    ''' <returns>DataItemStatus</returns>
    ''' <remarks></remarks>
    Public Property Status() As DataItemStatus Implements IDataItem.Status
        Get
            Return _status
        End Get
        Set(ByVal value As DataItemStatus)
            _status = value
        End Set
    End Property

    ''' <summary>
    ''' Sets data from ithin inherited classes.
    ''' </summary>
    ''' <param name="data">T. The data to be set.</param>
    ''' <remarks></remarks>
    Protected Sub SetData(ByVal data As T)
        _data = data
    End Sub
End Class
