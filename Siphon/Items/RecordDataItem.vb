''' <summary>
''' Class to reference and read IDataRecords for processing.
''' </summary>
''' <remarks></remarks>
Public Class RecordDataItem
    Inherits DataItem(Of DataRow)

    Private _nameFormat As String = String.Empty
    Private _primaryKeyFormat As String = String.Empty
    Private _recordFormat As String = String.Empty

    ''' <summary>
    ''' Creates a new RecordDataItem instance.
    ''' </summary>
    ''' <param name="record">DataRow. The database record/row being processed.</param>
    ''' <param name="nameFormat">String. The format to use to create a friendly name for the given record.</param>
    ''' <param name="recordFormat">String. The format to use to create a string representation of the given record.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal record As DataRow, Optional ByVal nameFormat As String = "", Optional ByVal recordFormat As String = "")
        Me.Data = record
        Me.NameFormat = nameFormat
        Me.RecordFormat = recordFormat
        Me.Name = Me.FormatRecord(Me.Data, Me.NameFormat)
    End Sub

    ''' <summary>
    ''' Gets/sets the format string used to create a string representation of a record.
    ''' </summary>
    ''' <value>String</value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Property RecordFormat() As String
        Get
            Return _recordFormat
        End Get
        Set(ByVal value As String)
            _recordFormat = value.Trim
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the format string used to create a friendly name representation of a record.
    ''' </summary>
    ''' <value>String</value>
    ''' <returns>String</returns>
    ''' <remarks>If no name format is specified, the primary key fields will be used.</remarks>
    Public Property NameFormat() As String
        Get
            If String.IsNullOrEmpty(_nameFormat) Then
                If String.IsNullOrEmpty(_primaryKeyFormat) Then
                    For Each column As DataColumn In Me.Data.Table.PrimaryKey
                        _primaryKeyFormat += String.Format("%{0}% ", column.ColumnName)
                    Next
                    _primaryKeyFormat = String.Concat("Record ", _primaryKeyFormat.Trim)
                End If
                Return _primaryKeyFormat
            End If

            Return _nameFormat
        End Get
        Set(ByVal value As String)
            _nameFormat = value.Trim
        End Set
    End Property

    ''' <summary>
    ''' Gets the contents of the data record for processing.
    ''' </summary>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overrides Function GetString() As String
        If String.IsNullOrEmpty(Me.RecordFormat) Then
            Return String.Empty
        Else
            Return FormatRecord(Me.Data, Me.RecordFormat)
        End If
    End Function

    ''' <summary>
    ''' Formats a record into a string representing the record in human readable form.
    ''' </summary>
    ''' <param name="record">DataRow. The record to be formatted.</param>
    ''' <param name="format">String. The format string to use to represent the record.</param>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Protected Overridable Function FormatRecord(ByVal record As DataRow, ByVal format As String) As String
        Dim value = format
        Dim table As DataTable = record.Table

        If Not String.IsNullOrEmpty(format) Then
            For Each column As DataColumn In table.Columns
                value = value.Replace(String.Format("%{0}%", column.ColumnName), record.Item(column.Ordinal).ToString)
            Next
        End If

        Return value
    End Function

End Class
