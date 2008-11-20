Namespace Monitors

    ''' <summary>
    ''' Class containing the monitor data being processed.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ProcessEventArgs
        Inherits EventArgs

        Private _data As IDataItem = Nothing

        ''' <summary>
        ''' Creates a new ProcessEventArgs class.
        ''' </summary>
        ''' <param name="data">IDataItem. The data item being processed.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal data As IDataItem)
            _data = data
        End Sub

        ''' <summary>
        ''' Gets the data item being processed.
        ''' </summary>
        ''' <value></value>
        ''' <returns>IDataItem</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Data() As IDataItem
            Get
                Return _data
            End Get
        End Property
    End Class
End Namespace