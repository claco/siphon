''' <summary>
''' Class containing the monitor data being processed.
''' </summary>
''' <remarks></remarks>
Public Class ProcessEventArgs
    Inherits EventArgs

    Private _item As IDataItem = Nothing

    ''' <summary>
    ''' Creates a new ProcessEventArgs class.
    ''' </summary>
    ''' <param name="item">IDataItem. The data item being processed.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal item As IDataItem)
        _item = item
    End Sub

    ''' <summary>
    ''' Gets the data item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>IDataItem</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Item() As IDataItem
        Get
            Return _item
        End Get
    End Property
End Class
