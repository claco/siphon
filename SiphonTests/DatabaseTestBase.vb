Imports System.Configuration
Imports System.Data.Common
Imports log4net

Public Class DatabaseTestBase
    Inherits TestBase

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Protected Overridable Sub CreateRecord(ByVal record As DataRow)
        Dim connectionString As ConnectionStringSettings = ConfigurationManager.ConnectionStrings("SiphonTests")
        Dim factory As DbProviderFactory = DbProviderFactories.GetFactory(connectionString.ProviderName)
        Dim adapter As DbDataAdapter = factory.CreateDataAdapter

        adapter.TableMappings.Add("DatabaseMonitor", "DatabaseMonitor")

        Dim table As New DataTable("DatabaseMonitor")
        table.Rows.Add(record)
        adapter.Update(table)
    End Sub

    Protected Overridable Sub CreateSuccessRecord(Optional ByVal name As String = "SUCCESS")
        Dim table As New DataTable("DatabaseMonitor")
        Dim row As DataRow = table.NewRow

        row.Item("Name") = name

        CreateRecord(row)
    End Sub

    Protected Overridable Sub CreateFailureRecord(Optional ByVal name As String = "FAILURE")
        Dim table As New DataTable
        Dim row As DataRow = table.NewRow

        row.Item("Name") = name

        CreateRecord(row)
    End Sub

    Protected Overridable Sub CreateExceptionRecord(Optional ByVal name As String = "EXCEPTION")
        Dim table As New DataTable
        Dim row As DataRow = table.NewRow

        row.Item("Name") = name

        CreateRecord(row)
    End Sub
End Class
