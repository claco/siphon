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
        Me.Name = name
        Me.Data = data
    End Sub

    ''' <summary>
    ''' Gets the contents of the item being processed.
    ''' </summary>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overridable Function GetString() As String Implements IDataItem.GetString
        Return Me.Data.ToString
    End Function

    ''' <summary>
    ''' Gets/sets the the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>T</returns>
    ''' <remarks></remarks>
    Public Overloads Property Data() As T Implements IDataItem(Of T).Data
        Get
            Return _data
        End Get
        Set(ByVal value As T)
            _data = value
        End Set
    End Property


    ''' <summary>
    ''' Gets the friendly name of the item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overridable Property Name() As String Implements IDataItem.Name
        Get
            Return _name
        End Get
        Set(ByVal value As String)
            _name = value.Trim
        End Set
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
End Class
