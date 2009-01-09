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
    Private _providerFactory As DbProviderFactory = Nothing
    Private _filter As String = String.Empty
    Private _selectCommand As IDbCommand = Nothing
    Private Const SETTING_CONNECTION_STRING_NAME As String = "ConnectionStringName"
    Private Const SETTING_FILTER As String = "Filter"
    Private Const SETTING_SELECT_COMMAND As String = "SelectCommand"

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
                _connection.ConnectionString = connectionString.ConnectionString
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
                _providerFactory = DbProviderFactories.GetFactory(connectionString.ProviderName)
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
            _connectionStringName = value.Trim

            If _connection IsNot Nothing Then
                _connection.Dispose()
                _connection = Nothing
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
        If settings.AllKeys.Contains(SETTING_FILTER) Then
            Me.Filter = settings(SETTING_FILTER).Value
        End If
        If settings.AllKeys.Contains(SETTING_SELECT_COMMAND) Then
            Me.SelectCommand = Me.GetCommand(settings(SETTING_SELECT_COMMAND).Value)
        End If
    End Sub

    ''' <summary>
    ''' Gets/sets the filter to apply to the records being monitored.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overridable Property Filter() As String Implements IDatabaseMonitor.Filter
        Get
            Return _filter
        End Get
        Set(ByVal value As String)
            _filter = value.Trim
        End Set
    End Property

    ''' <summary>
    ''' Deletes the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to delete.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Delete(ByVal item As IDataItem)

    End Sub

    ''' <summary>
    ''' Moves the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to move.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Move(ByVal item As IDataItem)

    End Sub

    ''' <summary>
    ''' Renames the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to renamed.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Rename(ByVal item As IDataItem)

    End Sub

    ''' <summary>
    ''' Scans for new data and sends new data to the current processor.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Function Scan() As Collection(Of IDataItem)

    End Function

    ''' <summary>
    ''' Gets/sets the command to run to select new data records.
    ''' </summary>
    ''' <value></value>
    ''' <returns>IDbCommand</returns>
    ''' <remarks></remarks>
    Public Property SelectCommand() As IDbCommand Implements IDatabaseMonitor.SelectCommand
        Get
            Return _selectCommand
        End Get
        Set(ByVal value As System.Data.IDbCommand)
            _selectCommand = value
        End Set
    End Property

    ''' <summary>
    ''' Converts the SelectCommand configuration parameter into the appropriate select command.
    ''' </summary>
    ''' <param name="commandText">String. The select command string from app.config</param>
    ''' <returns>IDbCommand</returns>
    ''' <remarks></remarks>
    Protected Overridable Function GetCommand(ByVal commandText As String) As IDbCommand

    End Function

End Class
