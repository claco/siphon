Imports System.Configuration
Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Data.Common
Imports log4net

''' <summary>
''' Monitors a database for new records.
''' </summary>
''' <remarks></remarks>
Public Class DatabaseMonitor
    Inherits DataMonitor
    Implements IDatabaseMonitor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private _connection As IDbConnection = Nothing
    Private _connectionStringName As String = String.Empty
    Private _recordFormat As String = String.Empty
    Private _nameFormat As String = String.Empty
    Private _providerFactory As DbProviderFactory = Nothing
    Private _selectCommand As IDbCommand = Nothing
    Private _updateCommand As IDbCommand = Nothing
    Private Const SETTING_CONNECTION_STRING_NAME As String = "ConnectionStringName"
    Private Const SETTING_FILTER As String = "Filter"
    Private Const SETTING_NAME_FORMAT As String = "NameFormat"
    Private Const SETTING_RECORD_FORMAT As String = "RecordFormat"
    Private Const SETTING_SELECT_COMMAND As String = "SelectCommand"
    Private Const SETTING_UPDATE_COMMAND As String = "UpdateCommand"

    ''' <summary>
    ''' Protected constructor for reflection.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new database monitor.
    ''' </summary>
    ''' <param name="name">String. The friendly name for the monitor.</param>
    ''' <param name="connectionStringName">String. The name of the connection string from config to use.</param>
    ''' <param name="selectCommand">String. The select command to run to find new records.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to monitor the directory.</param>
    ''' <param name="processor">IDataProcessor. The data processor to use to process new files.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, ByVal connectionStringName As String, ByVal selectCommand As String, ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        MyBase.New(name, schedule, processor)
        Me.ConnectionStringName = connectionStringName
        Me.SelectCommand = Me.GetCommand(selectCommand)
    End Sub

    ''' <summary>
    ''' Creates a new database monitor.
    ''' </summary>
    ''' <param name="name">String. The friendly name for the monitor.</param>
    ''' <param name="connectionStringName">String. The name of the connection string from config to use.</param>
    ''' <param name="selectCommand">IDbCommand. The select command to run to find new records.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to monitor the directory.</param>
    ''' <param name="processor">IDataProcessor. The data processor to use to process new files.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, ByVal connectionStringName As String, ByVal selectCommand As IDbCommand, ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        MyBase.New(name, schedule, processor)
        Me.ConnectionStringName = connectionStringName
        Me.SelectCommand = selectCommand
    End Sub

    ''' <summary>
    ''' Gets the connection from the current provider.
    ''' </summary>
    ''' <value></value>
    ''' <returns>IDbConnection</returns>
    ''' <remarks></remarks>
    Protected Overridable ReadOnly Property Connection() As IDbConnection
        Get
            If _connection Is Nothing Then
                Dim connectionString As ConnectionStringSettings = ConfigurationManager.ConnectionStrings.Item(Me.ConnectionStringName)
                _connection = Me.ProviderFactory.CreateConnection
                _connection.ConnectionString = connectionString.ConnectionString()
            End If

            Return _connection
        End Get
    End Property

    ''' <summary>
    ''' Gets the provider factory for the current providers connection.
    ''' </summary>
    ''' <value></value>
    ''' <returns>DbProviderFactory</returns>
    ''' <remarks></remarks>
    Protected Overridable ReadOnly Property ProviderFactory() As DbProviderFactory
        Get
            If _providerFactory Is Nothing Then
                Dim connectionString As ConnectionStringSettings = ConfigurationManager.ConnectionStrings.Item(Me.ConnectionStringName)

                If connectionString Is Nothing Then
                    Throw New ArgumentException(String.Format("The connection string {0} could not be found", Me.ConnectionStringName))
                Else
                    _providerFactory = DbProviderFactories.GetFactory(connectionString.ProviderName)
                End If
            End If

            Return _providerFactory
        End Get
    End Property

    ''' <summary>
    ''' Gets/sets the name of the connection string to use to connect to the database.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overridable Property ConnectionStringName() As String Implements IDatabaseMonitor.ConnectionStringName
        Get
            Return _connectionStringName
        End Get
        Set(ByVal value As String)
            If String.IsNullOrEmpty(value) Then
                Throw New ArgumentException("ConnectionStringName can not be null or empty")
            Else
                _connectionStringName = value.Trim

                If _connection IsNot Nothing Then
                    _connection.Dispose()
                    _connection = Nothing
                End If
                _providerFactory = Nothing
                If _selectCommand IsNot Nothing Then
                    Dim commandText As String = _selectCommand.CommandText
                    _selectCommand.Dispose()
                    _selectCommand = Nothing
                    Me.SelectCommand = GetCommand(commandText)
                End If
            End If
        End Set
    End Property

    ''' <summary>
    ''' Initializes the monitor using the supplied monitor configuration settings.
    ''' </summary>
    ''' <param name="config">MonitorElement. The configuration for the current monitor.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Initialize(ByVal config As MonitorElement)
        MyBase.Initialize(config)

        Dim settings As NameValueConfigurationCollection = config.Settings
        If settings.AllKeys.Contains(SETTING_CONNECTION_STRING_NAME) Then
            Me.ConnectionStringName = settings(SETTING_CONNECTION_STRING_NAME).Value
        End If
        If settings.AllKeys.Contains(SETTING_SELECT_COMMAND) Then
            Me.SelectCommand = Me.GetCommand(settings(SETTING_SELECT_COMMAND).Value)
        End If
        If settings.AllKeys.Contains(SETTING_UPDATE_COMMAND) Then
            Me.UpdateCommand = Me.GetCommand(settings(SETTING_UPDATE_COMMAND).Value)
        End If
        If settings.AllKeys.Contains(SETTING_NAME_FORMAT) Then
            Me.NameFormat = settings(SETTING_NAME_FORMAT).Value
        End If
        If settings.AllKeys.Contains(SETTING_RECORD_FORMAT) Then
            Me.RecordFormat = settings(SETTING_RECORD_FORMAT).Value
        End If
    End Sub

    ''' <summary>
    ''' Deletes the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to delete.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Delete(ByVal item As IDataItem)
        Dim recordItem As RecordDataItem = item

        Log.DebugFormat("Deleting {0}", recordItem.Name)

        Try
            Using builder As DbCommandBuilder = ProviderFactory.CreateCommandBuilder
                builder.DataAdapter = Me.ProviderFactory.CreateDataAdapter
                builder.DataAdapter.SelectCommand = Me.SelectCommand
                builder.DataAdapter.SelectCommand.Connection = Me.Connection

                recordItem.Data.Delete()
                Dim rows() As DataRow = {recordItem.Data}
                builder.DataAdapter.Update(rows)
            End Using
        Catch ex As Exception
            Log.Error(ex)
        Finally
            Me.Connection.Close()
            Me.SelectCommand.Connection = Nothing
        End Try
    End Sub

    ''' <summary>
    ''' Moves the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to move.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Move(ByVal item As IDataItem)
        Throw New NotImplementedException
    End Sub

    ''' <summary>
    ''' Gets/sets the format string to use when creating a name representation of a record.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overridable Property NameFormat() As String Implements IDatabaseMonitor.NameFormat
        Get
            Return _nameFormat
        End Get
        Set(ByVal value As String)
            _nameFormat = value.Trim
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the format string to use when creating a string representation of a record.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overridable Property RecordFormat() As String Implements IDatabaseMonitor.RecordFormat
        Get
            Return _recordFormat
        End Get
        Set(ByVal value As String)
            If String.IsNullOrEmpty(value) Then
                Throw New ArgumentException("RecordFormat can not be null or empty")
            Else
                _recordFormat = value.Trim
            End If

        End Set
    End Property

    ''' <summary>
    ''' Renames the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to renamed.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Rename(ByVal item As IDataItem)
        Throw New NotImplementedException
    End Sub

    ''' <summary>
    ''' Update the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to update.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Update(ByVal item As IDataItem)
        If Me.UpdateCommand IsNot Nothing Then
            Dim recordItem As RecordDataItem = item

            Log.DebugFormat("Updating {0}", recordItem.Name)

            Try
                Me.UpdateCommand.Connection = Me.Connection
                Me.Connection.Open()
                Me.UpdateCommand.ExecuteNonQuery()
                Me.Connection.Close()
            Catch ex As Exception
                Log.Error(ex)
            Finally
                Me.Connection.Close()
                Me.UpdateCommand.Connection = Nothing
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Scans for new data and sends new data to the current processor.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Function Scan() As Collection(Of IDataItem)
        Log.DebugFormat("Scanning {0} for {1}", Me.ConnectionStringName, Me.SelectCommand.CommandText)

        Dim items As New Collection(Of IDataItem)

        Try
            Using dataSet As New DataSet
                Using adapter As DbDataAdapter = Me.ProviderFactory.CreateDataAdapter
                    Me.SelectCommand.Connection = Me.Connection

                    adapter.SelectCommand = Me.SelectCommand
                    adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey
                    adapter.Fill(dataSet)
                End Using

                Log.DebugFormat("Found {0} records", dataSet.Tables(0).Rows.Count)
                For Each row As DataRow In dataSet.Tables(0).Rows
                    items.Add(New RecordDataItem(row, Me.NameFormat, Me.RecordFormat))
                Next
            End Using
        Catch ex As Exception
            Log.Error(ex)
        Finally
            Me.Connection.Close()
            Me.SelectCommand.Connection = Nothing
        End Try

        Return items
    End Function

    ''' <summary>
    ''' Gets/sets the command to run to select new data records.
    ''' </summary>
    ''' <value></value>
    ''' <returns>IDbCommand</returns>
    ''' <remarks></remarks>
    Public Overridable Property SelectCommand() As IDbCommand Implements IDatabaseMonitor.SelectCommand
        Get
            Return _selectCommand
        End Get
        Set(ByVal value As System.Data.IDbCommand)
            If value Is Nothing Then
                Throw New ArgumentException("The select command can not be null or empty")
            Else
                _selectCommand = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the command to run to update processed data records.
    ''' </summary>
    ''' <value></value>
    ''' <returns>IDbCommand</returns>
    ''' <remarks></remarks>
    Public Overridable Property UpdateCommand() As IDbCommand Implements IDatabaseMonitor.UpdatedCommand
        Get
            Return _updateCommand
        End Get
        Set(ByVal value As System.Data.IDbCommand)
            _updateCommand = value
        End Set
    End Property

    ''' <summary>
    ''' Converts the SelectCommand configuration parameter into the appropriate select command.
    ''' </summary>
    ''' <param name="commandText">String. The select command string from app.config</param>
    ''' <returns>IDbCommand</returns>
    ''' <remarks></remarks>
    Protected Overridable Function GetCommand(ByVal commandText As String) As IDbCommand
        If String.IsNullOrEmpty(commandText) Then
            Throw New ArgumentException("The select command can not be null or empty")
        Else
            Dim command As IDbCommand = Me.ProviderFactory.CreateCommand
            command.CommandText = commandText
            command.CommandType = CommandType.Text

            Return command
        End If
    End Function

    ''' <summary>
    ''' Validates the current monitors configuration for errors before processing/starting.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overrides Sub Validate()
        Log.Debug("Validating monitor configuration")

        MyBase.Validate()

        If String.IsNullOrEmpty(Me.RecordFormat) Then
            Throw New ApplicationException("No record format is defined")
        ElseIf Me.UpdateCommand Is Nothing And (Me.ProcessCompleteActions And DataActions.Update) <> DataActions.None Then
            Throw New ApplicationException("UpdateCommand must be defined to use the Update action on process completion")
        ElseIf Me.UpdateCommand Is Nothing And (Me.ProcessFailureActions And DataActions.Update) <> DataActions.None Then
            Throw New ApplicationException("UpdateCommand must be defined to use the Update action on process failure")
        End If
    End Sub
End Class
