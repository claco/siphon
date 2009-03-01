Imports System.Configuration
Imports System.Data.Common
Imports log4net

Public Class DatabaseTestBase
    Inherits TestBase

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Protected Overrides Sub SetUp()
        MyBase.SetUp()

        DeleteRecords()
    End Sub

    Protected Overridable Sub DeleteRecords()
        Dim connectionString As ConnectionStringSettings = ConfigurationManager.ConnectionStrings("SiphonTests")
        Dim factory As DbProviderFactory = DbProviderFactories.GetFactory(connectionString.ProviderName)

        Using connection As IDbConnection = factory.CreateConnection
            connection.ConnectionString = connectionString.ConnectionString

            Using command As IDbCommand = factory.CreateCommand
                command.CommandText = "Delete From DatabaseMonitor"
                command.CommandType = CommandType.Text
                command.Connection = connection

                connection.Open()
                command.ExecuteNonQuery()
                connection.Close()
            End Using
        End Using
    End Sub

    Protected Overridable Function GetRecords() As DataTable
        Dim connectionString As ConnectionStringSettings = ConfigurationManager.ConnectionStrings("SiphonTests")
        Dim factory As DbProviderFactory = DbProviderFactories.GetFactory(connectionString.ProviderName)
        Dim adapter As DbDataAdapter = factory.CreateDataAdapter
        Dim table As New DataTable

        Using connection As IDbConnection = factory.CreateConnection
            connection.ConnectionString = connectionString.ConnectionString

            Using command As IDbCommand = factory.CreateCommand
                command.CommandText = "Select * From DatabaseMonitor"
                command.CommandType = CommandType.Text
                command.Connection = connection
                adapter.SelectCommand = command
                adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey
                connection.Open()
                adapter.Fill(table)
                connection.Close()
            End Using
        End Using

        Return table
    End Function

    Protected Overridable Sub CreateRecord(ByVal record As Dictionary(Of String, Object))
        Dim connectionString As ConnectionStringSettings = ConfigurationManager.ConnectionStrings("SiphonTests")
        Dim factory As DbProviderFactory = DbProviderFactories.GetFactory(connectionString.ProviderName)

        Using connection As IDbConnection = factory.CreateConnection
            connection.ConnectionString = connectionString.ConnectionString

            Using command As IDbCommand = factory.CreateCommand
                command.CommandText = String.Format("Insert Into DatabaseMonitor ({0}) Values (@{1});", String.Join(",", record.Keys.ToArray), String.Join(", @", record.Keys.ToArray))
                command.CommandType = CommandType.Text
                command.Connection = connection

                For Each column As String In record.Keys
                    Dim parameter As IDbDataParameter = factory.CreateParameter
                    parameter.ParameterName = String.Format("@{0}", column)
                    parameter.Value = record(column)
                    command.Parameters.Add(parameter)
                Next

                connection.Open()
                command.ExecuteNonQuery()
                connection.Close()
            End Using
        End Using
    End Sub

    Protected Overridable Sub CreateSuccessRecord(Optional ByVal name As String = "SUCCESS")
        Dim row As New Dictionary(Of String, Object)
        row("Name") = name

        CreateRecord(row)
    End Sub

    Protected Overridable Sub CreateFailureRecord(Optional ByVal name As String = "FAILURE")
        Dim row As New Dictionary(Of String, Object)
        row("Name") = name

        CreateRecord(row)
    End Sub

    Protected Overridable Sub CreateExceptionRecord(Optional ByVal name As String = "EXCEPTION")
        Dim row As New Dictionary(Of String, Object)
        row("Name") = name

        CreateRecord(row)
    End Sub
End Class
