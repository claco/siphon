Imports System.Configuration
Imports System.Data.Common
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

    <Test(Description:="Test successful database monitor")> _
    Public Sub DatabaseMonitorWithCommand()
        CreateSuccessRecord()

        Dim connectionString As ConnectionStringSettings = ConfigurationManager.ConnectionStrings("SiphonTests")
        Dim factory As DbProviderFactory = DbProviderFactories.GetFactory(connectionString.ProviderName)
        Dim command As DbCommand = factory.CreateCommand
        command.CommandText = "Select * From DatabaseMonitor"

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(3).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTests", command, schedule, processor)
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

    <Test(Description:="Test successful database monitor process complete updates record")> _
    Public Sub DatabaseMonitorProcessorCompleteUpdateRecord()
        CreateSuccessRecord()

        Dim connectionString As ConnectionStringSettings = ConfigurationManager.ConnectionStrings("SiphonTests")
        Dim factory As DbProviderFactory = DbProviderFactories.GetFactory(connectionString.ProviderName)
        Dim command As DbCommand = factory.CreateCommand
        command.CommandText = "Update DatabaseMonitor Set Status=1"

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.RecordFormat = "%Name%"
                    monitor.ProcessCompleteActions = DataActions.Update
                    monitor.UpdatedCommand = command
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 record")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                    Assert.AreEqual(1, Me.GetRecords.Rows.Count)
                    Assert.AreEqual(1, Me.GetRecords.Rows(0).Item("Status"))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test database monitor with processor failure")> _
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

    <Test(Description:="Test successful database monitor process failure updates record")> _
    Public Sub DatabaseMonitorProcessorFailureUpdateRecord()
        CreateFailureRecord()

        Dim connectionString As ConnectionStringSettings = ConfigurationManager.ConnectionStrings("SiphonTests")
        Dim factory As DbProviderFactory = DbProviderFactories.GetFactory(connectionString.ProviderName)
        Dim command As DbCommand = factory.CreateCommand
        command.CommandText = "Update DatabaseMonitor Set Status=1"

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.RecordFormat = "%Name%"
                    monitor.ProcessFailureActions = DataActions.Update
                    monitor.UpdatedCommand = command
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 record")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                    Assert.AreEqual(1, Me.GetRecords.Rows.Count)
                    Assert.AreEqual(1, Me.GetRecords.Rows(0).Item("Status"))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test database monitor with processor failure deletes record")> _
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

    <Test(Description:="Test database monitor with processor exception")> _
    Public Sub DatabaseMonitorProcessorException()
        CreateExceptionRecord()

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

    <Test(Description:="Test database monitor with filter")> _
    Public Sub DatabaseMonitorWithFilter()
        CreateSuccessRecord()
        CreateSuccessRecord("SUCCESSED2")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor Where Name = 'SUCCESS'", schedule, processor)
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
                    Assert.AreEqual(1, Me.GetRecords.Rows.Count)
                    Assert.AreEqual("SUCCESSED2", Me.GetRecords.Rows(0).Item("Name"))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test database monitor with slow processor on stop")> _
     Public Sub DatabaseMonitorStillProcessing()
        CreateSuccessRecord()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As DatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor Where Name = 'SUCCESS'", schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    monitor.RecordFormat = "%Name%"
                    processor.Delay = 10
                    monitor.Start()
                    Threading.Thread.Sleep(5000)

                    Assert.IsTrue(monitor.IsProcessing, "Processing is true when a worker processor is still running")
                    Dim pre As DateTime = DateTime.Now
                    monitor.Stop()
                    Dim post As DateTime = DateTime.Now

                    Assert.AreEqual(1, processor.Count, "Has processed 1 record")
                    Assert.GreaterOrEqual((post - pre).TotalSeconds, 5, "Waited for still running process to finish")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub NameEmptyArgumentException()
        Dim monitor As New DatabaseMonitor("", "SiphonTests", "Select * From DatabaseMonitor Where Name = 'SUCCESS'", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub RecordFormatEmptyArgumentException()
        Dim monitor As New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor Where Name = 'SUCCESS'", New IntervalSchedule, New MockProcessor)

        monitor.RecordFormat = "Foo"
        monitor.RecordFormat = ""
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub ConnectionStringNameEmptyArgumentException()
        Dim monitor As New DatabaseMonitor("DatabaseMonitor", "", "Select * From DatabaseMonitor Where Name = 'SUCCESS'", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub SelectCommandEmptyArgumentException()
        Dim monitor As New DatabaseMonitor("DatabaseMonitor", "SiphoneTests", "", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub SelectCommandNothingArgumentException()
        Dim command As IDbCommand = Nothing
        Dim monitor As New DatabaseMonitor("DatabaseMonitor", "SiphoneTests", command, New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub ConnectionStringNameNotFoundArgumentException()
        Dim monitor As New DatabaseMonitor("DatabaseMonitor", "SiphonTestsNotFound", "Select * From DatabaseMonitor Where Name = 'SUCCESS'", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Start throws exception for null Name")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullNameException()
        Using monitor As New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As DatabaseMonitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.DatabaseMonitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.ConnectionStringName = monitor.ConnectionStringName
            newMonitor.SelectCommand = monitor.SelectCommand
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Name")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullRecordFormatException()
        Using monitor As New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As DatabaseMonitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.DatabaseMonitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.ConnectionStringName = monitor.ConnectionStringName
            newMonitor.SelectCommand = monitor.SelectCommand
            newMonitor.Name = "DatabaseMonitor"
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for update action without command")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureUpdateCompleteActionException()
        Using monitor As New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As DatabaseMonitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.DatabaseMonitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.ConnectionStringName = monitor.ConnectionStringName
            newMonitor.RecordFormat = "%Name%"
            newMonitor.SelectCommand = monitor.SelectCommand
            newMonitor.Name = "DatabaseMonitor"
            newMonitor.ProcessCompleteActions = DataActions.Update
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for update action without command")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureUpdateFailureActionException()
        Using monitor As New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As DatabaseMonitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.DatabaseMonitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.ConnectionStringName = monitor.ConnectionStringName
            newMonitor.RecordFormat = "%Name%"
            newMonitor.SelectCommand = monitor.SelectCommand
            newMonitor.Name = "DatabaseMonitor"
            newMonitor.ProcessFailureActions = DataActions.Update
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Create monitor from configuration")> _
    Public Sub CreateFromConfiguration()
        Dim monitorElement As New MonitorElement("TestDatabaseMonitor", "ChrisLaco.Siphon.DatabaseMonitor, Siphon")
        Dim processorElement As New ProcessorElement("ChrisLaco.Tests.Siphon.MockProcessor, SiphonTests")
        Dim scheduleElement As New ScheduleElement("ChrisLaco.Siphon.DailySchedule, Siphon")
        scheduleElement.Daily.Add(DateTime.Now.AddSeconds(3).TimeOfDay)
        monitorElement.Schedule = scheduleElement
        monitorElement.Processor = processorElement
        monitorElement.Settings.Add(New NameValueConfigurationElement("ConnectionStringName", "SiphonTests"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("SelectCommand", "Select * From DatabaseMonitor"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("UpdateCommand", "Update DatabaseMonitor Set Status=1"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("NameFormat", "Record Id %Id%"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("RecordFormat", "<Name>%Name%</Name>"))

        Using monitor As DatabaseMonitor = monitorElement.CreateInstance
            Assert.IsInstanceOfType(GetType(DatabaseMonitor), monitor)
            Assert.AreEqual("SiphonTests", monitor.ConnectionStringName)
            Assert.AreEqual("Select * From DatabaseMonitor", monitor.SelectCommand.CommandText)
            Assert.AreEqual("Update DatabaseMonitor Set Status=1", monitor.UpdateCommand.CommandText)
            Assert.AreEqual("Record Id %Id%", monitor.NameFormat)
            Assert.AreEqual("<Name>%Name%</Name>", monitor.RecordFormat)
        End Using
    End Sub

    <Test(Description:="Test successful database monitor")> _
    Public Sub ResetConnection()
        CreateSuccessRecord()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(3).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As DatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTestConnectionStringName", "Select * From DatabaseMonitor", schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.ConnectionStringName = "SiphonTests"
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

    <Test(Description:="Test successful database monitor")> _
    Public Sub DatabaseMonitorMoveExceptionComplete()
        CreateSuccessRecord()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(3).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.ProcessCompleteActions = DataActions.Move
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

    <Test(Description:="Test successful database monitor")> _
    Public Sub DatabaseMonitorMoveExceptionFailure()
        CreateFailureRecord()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(3).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.ProcessFailureActions = DataActions.Move
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

    <Test(Description:="Test successful database monitor")> _
    Public Sub DatabaseMonitorRenameExceptionFailure()
        CreateFailureRecord()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(3).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDatabaseMonitor = New DatabaseMonitor("DatabaseMonitor", "SiphonTests", "Select * From DatabaseMonitor", schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.ProcessFailureActions = DataActions.Rename
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
End Class
