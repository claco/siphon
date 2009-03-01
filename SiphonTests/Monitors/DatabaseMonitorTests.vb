Imports System.Reflection
Imports System.Transactions
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Database Monitor Tests")> _
    Public Class DatabaseMonitorTests
    Inherits DatabaseTestBase

    <Test(Description:="Test IDbConnection from Factory from named config connection string")> _
    Public Sub Connection()
        Using monitor As IDatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTestConnectionStringName", "Select * From DatabaseMonitor", New IntervalSchedule, New MockProcessor)
            monitor.ConnectionStringName = "SiphonTestConnectionStringName"

            Dim prop As PropertyInfo = monitor.GetType.GetProperty("Connection", BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)
            Dim connection As IDbConnection = prop.GetValue(monitor, Nothing, Nothing, Nothing, Nothing)

            Assert.IsInstanceOfType(GetType(IDbConnection), connection)
            Assert.AreEqual("Data Source=.;Initial Catalog=;Integrated Security=True", connection.ConnectionString)
        End Using
    End Sub

    <Test(Description:="Test successful database monitor")> _
    Public Sub DatabaseMonitor()
        CreateSuccessRecord()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(3).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.RecordFormat = "%Name%"
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 record")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test successful database monitor process complete deletes record")> _
    Public Sub DatabaseMonitorProcessorCompleteDeleteRecord()
        CreateSuccessRecord()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.RecordFormat = "%Name%"
                    monitor.ProcessCompleteActions = DataActions.Delete
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 record")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                    Assert.AreEqual(0, Me.GetRecords.Rows.Count)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor failure")> _
    Public Sub DatabaseMonitorProcessorFailure()
        CreateFailureRecord()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.RecordFormat = "%Name%"
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 record")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor failure deletes record")> _
    Public Sub DatabaseMonitorProcessorFailureDeleteRecord()
        CreateFailureRecord()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.RecordFormat = "%Name%"
                    monitor.ProcessFailureActions = DataActions.Delete
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 record")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                    Threading.Thread.Sleep(2000)
                    Assert.AreEqual(0, Me.GetRecords.Rows.Count)
                End Using
            End Using
        End Using
    End Sub
End Class
