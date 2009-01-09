''' <summary>
''' Class to reference and read IDataRecords for processing.
''' </summary>
''' <remarks></remarks>
Public Class RecordDataItem
    Inherits DataItem(Of IDataRecord)

    ''' <summary>
    ''' Creates a new RecordDataItem instance.
    ''' </summary>
    ''' <param name="record">IDataRecord. The IDataRecord being processed.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal record As IDataRecord)
        MyBase.New(record.ToString, record)
    End Sub

    ''' <summary>
    ''' Gets the contents of the IDataRecord for processing.
    ''' </summary>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overrides Function GetString() As String
        Return Me.Data.ToString
    End Function
End Class
