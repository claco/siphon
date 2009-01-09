Imports System.Reflection
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Database Monitor Tests")> _
    Public Class DatabaseMonitorTests
    Inherits TestBase

    <Test(Description:="Test IDbConnection from Factory from named config connection string")> _
    Public Sub Connection()
        Using monitor As IDatabaseMonitor = New DatabaseMonitor
            monitor.ConnectionStringName = "TestConnectionStringName"

            Dim prop As PropertyInfo = monitor.GetType.GetProperty("Connection", BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)
            Dim connection As IDbConnection = prop.GetValue(monitor, Nothing, Nothing, Nothing, Nothing)

            Assert.IsInstanceOfType(GetType(IDbConnection), connection)
            Assert.AreEqual("Data Source=.;Initial Catalog=;Integrated Security=True", connection.ConnectionString)
        End Using
    End Sub
End Class
