Imports System.IO
Imports System.Messaging
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Monitor Tests")> _
    Public Class MonitorTests
    Inherits TestBase

#Region "Local Directory Monitor Tests"

    <Test(Description:="Path throws exception when empty")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub DirectoryMonitorPathException()
        Dim monitor As New LocalDirectoryMonitor("NewMonitor", String.Empty, New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Name throws exception when empty")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub DirectoryMonitorNameException()
        Dim monitor As New LocalDirectoryMonitor(String.Empty, "C:\temp", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Test successful directory monitor")> _
    Public Sub DirectoryMonitor()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    monitor.Filter = String.Empty
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor failure")> _
    Public Sub DirectoryMonitorProcessorFailure()
        CreateFailureFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor exception")> _
    Public Sub DirectoryMonitorProcessorException()
        CreateExceptionFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor create missing root directory")> _
    Public Sub DirectoryMonitorCreateDirectory()
        Dim tempdir As String = Path.Combine(Path.GetTempPath, Path.GetRandomFileName)

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(1).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", tempdir, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    Assert.IsFalse(Directory.Exists(tempdir), "Monitor path doesn't exist")

                    monitor.CreateMissingFolders = True
                    monitor.Start()
                    monitor.Stop()

                    Assert.IsTrue(Directory.Exists(tempdir), "Monitor path exista")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with filter")> _
    Public Sub DirectoryMonitorWithFilter()
        CreateSuccessFile()
        CreateSuccessFile("test.csv")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    monitor.Filter = "*.csv"

                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 files")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub


    <Test(Description:="Test directory monitor with slow processor on stop")> _
    Public Sub DirectoryMonitorStillProcessing()
        CreateSuccessFile()

        Using schedule = New IntervalSchedule(2)
            Using processor = New MockProcessor
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    processor.Delay = 10
                    monitor.Start()
                    Threading.Thread.Sleep(3000)

                    Assert.IsTrue(monitor.Processing, "Processing is true when a worker processor is still running")
                    Dim pre As DateTime = DateTime.Now
                    monitor.Stop()
                    Dim post As DateTime = DateTime.Now

                    Assert.AreEqual(1, processor.Count, "Has processed 1 files")
                    Assert.GreaterOrEqual((post - pre).TotalSeconds, 5, "Waited for still running process to finish")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub
#End Region

#Region "Message Queue Monitor Tests"

    <Test(Description:="Test successful message queue monitor")> _
    Public Sub MessageQueueMonitor()
        Dim queue As MessageQueue = MessageQueue.Create(".\Private$\MyNewQueue")

        Try
            queue.Send("SUCCESS")

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor = New MockProcessor
                    Using monitor As MessageQueueMonitor = New MessageQueueMonitor("LocalMonitor", queue, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                        monitor.Start()
                        Threading.Thread.Sleep(3000)
                        monitor.Stop()

                        Assert.AreEqual(1, processor.Count, "Has processed 1 queue message")
                        Assert.IsTrue(Me.ProcessComplete)
                        Assert.IsFalse(Me.ProcessFailure)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        Finally
            MessageQueue.Delete(queue.Path)
        End Try
    End Sub

    <Test(Description:="Test message queue monitor create missing root queue")> _
    Public Sub MessageQueueMonitorCreateQueue()
        Dim queue As String = ".\Private$\MyNewQueue"

        Try
            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor = New MockProcessor
                    Using monitor As MessageQueueMonitor = New MessageQueueMonitor("LocalMonitor", queue, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                        Assert.IsFalse(MessageQueue.Exists(queue), "Monitor queue doesn't exist")

                        monitor.CreateMissingQueues = True
                        monitor.Start()

                        Assert.IsTrue(MessageQueue.Exists(queue), "Monitor queue exista")

                        monitor.Queue.Send("SUCCESS")

                        Threading.Thread.Sleep(3000)
                        monitor.Stop()

                        Assert.AreEqual(1, processor.Count, "Has processed 1 queue message")
                        Assert.IsTrue(Me.ProcessComplete)
                        Assert.IsFalse(Me.ProcessFailure)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        Finally
            MessageQueue.Delete(queue)
        End Try
    End Sub

    <Test(Description:="Test processor failure message queue monitor")> _
    Public Sub MessageQueueMonitorProcessorFailure()
        Dim queue As MessageQueue = MessageQueue.Create(".\Private$\MyNewQueue")

        Try
            queue.Send("FAILURE")

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor = New MockProcessor
                    Using monitor As MessageQueueMonitor = New MessageQueueMonitor("LocalMonitor", queue, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                        monitor.Start()
                        Threading.Thread.Sleep(3000)
                        monitor.Stop()

                        Assert.AreEqual(1, processor.Count, "Has processed 1 queue message")
                        Assert.IsFalse(Me.ProcessComplete)
                        Assert.IsTrue(Me.ProcessFailure)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        Finally
            MessageQueue.Delete(queue.Path)
        End Try
    End Sub

    <Test(Description:="Test processor exception message queue monitor")> _
    Public Sub MessageQueueMonitorProcessorException()
        Dim queue As MessageQueue = MessageQueue.Create(".\Private$\MyNewQueue")

        Try
            queue.Send("EXCEPTION")

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor = New MockProcessor
                    Using monitor As MessageQueueMonitor = New MessageQueueMonitor("LocalMonitor", queue, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                        monitor.Start()
                        Threading.Thread.Sleep(3000)
                        monitor.Stop()

                        Assert.AreEqual(1, processor.Count, "Has processed 1 queue message")
                        Assert.IsFalse(Me.ProcessComplete)
                        Assert.IsTrue(Me.ProcessFailure)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        Finally
            MessageQueue.Delete(queue.Path)
        End Try
    End Sub
#End Region

End Class
