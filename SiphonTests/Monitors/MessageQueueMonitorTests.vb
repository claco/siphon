Imports System.Configuration
Imports System.IO
Imports System.Messaging
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Message Queue Monitor Tests")> _
    Public Class MessageQueueMonitorTests
    Inherits TestBase

    <Test(Description:="Test successful message queue monitor")> _
    Public Sub MessageQueueMonitor()
        Dim queue As MessageQueue = MessageQueue.Create(".\Private$\SiphonTestQueue")

        Try
            queue.Send("SUCCESS")

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor = New MockProcessor
                    Using monitor As MessageQueueMonitor = New MessageQueueMonitor("LocalMonitor", queue, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                        monitor.Start()
                        Threading.Thread.Sleep(5000)
                        monitor.Stop()

                        Assert.AreEqual(1, processor.Count, "Has processed 1 queue message")
                        Assert.IsTrue(Me.ProcessComplete)
                        Assert.IsFalse(Me.ProcessFailure)
                        Assert.AreEqual(1, monitor.Queue.GetAllMessages.Count)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        Finally
            MessageQueue.Delete(queue.Path)
        End Try
    End Sub

    <Test(Description:="Test message queue monitor create missing queues")> _
    Public Sub MessageQueueMonitorCreateQueues()
        Dim queue As String = ".\Private$\SiphonTestQueue"
        Dim completeQueue As String = ".\Private$\SiphonTestQueueComplete"
        Dim failureQueue As String = ".\Private$\SiphonTestQueueFailure"

        Try
            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor = New MockProcessor
                    Using monitor As MessageQueueMonitor = New MessageQueueMonitor("LocalMonitor", queue, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                        monitor.CompleteQueue = New MessageQueue(completeQueue)
                        monitor.FailureQueue = New MessageQueue(failureQueue)

                        Assert.IsFalse(MessageQueue.Exists(queue), "Monitor queue doesn't exist")
                        Assert.IsFalse(MessageQueue.Exists(completeQueue), "Monitor queue doesn't exist")
                        Assert.IsFalse(MessageQueue.Exists(failureQueue), "Monitor queue doesn't exist")

                        monitor.CreateMissingQueues = True
                        monitor.Start()

                        Assert.IsTrue(MessageQueue.Exists(queue), "Monitor queue exista")
                        Assert.IsTrue(MessageQueue.Exists(completeQueue), "Monitor queue exista")
                        Assert.IsTrue(MessageQueue.Exists(failureQueue), "Monitor queue exista")

                        monitor.Queue.Send("SUCCESS")

                        Threading.Thread.Sleep(5000)
                        monitor.Stop()

                        Assert.AreEqual(1, processor.Count, "Has processed 1 queue message")
                        Assert.IsTrue(Me.ProcessComplete)
                        Assert.IsFalse(Me.ProcessFailure)
                        Assert.AreEqual(1, monitor.Queue.GetAllMessages.Count)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        Finally
            MessageQueue.Delete(queue)
            MessageQueue.Delete(completeQueue)
            MessageQueue.Delete(failureQueue)
        End Try
    End Sub

    <Test(Description:="Test processor failure message queue monitor")> _
    Public Sub MessageQueueMonitorProcessorFailure()
        Dim queue As MessageQueue = MessageQueue.Create(".\Private$\SiphonTestQueue")

        Try
            queue.Send("FAILURE")

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor = New MockProcessor
                    Using monitor As MessageQueueMonitor = New MessageQueueMonitor("LocalMonitor", queue, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                        monitor.Start()
                        Threading.Thread.Sleep(5000)
                        monitor.Stop()

                        Assert.AreEqual(1, processor.Count, "Has processed 1 queue message")
                        Assert.IsFalse(Me.ProcessComplete)
                        Assert.IsTrue(Me.ProcessFailure)
                        Assert.AreEqual(1, monitor.Queue.GetAllMessages.Count)
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
        Dim queue As MessageQueue = MessageQueue.Create(".\Private$\SiphonTestQueue")

        Try
            queue.Send("EXCEPTION")

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor = New MockProcessor
                    Using monitor As MessageQueueMonitor = New MessageQueueMonitor("LocalMonitor", queue, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                        monitor.Start()
                        Threading.Thread.Sleep(5000)
                        monitor.Stop()

                        Assert.AreEqual(1, processor.Count, "Has processed 1 queue message")
                        Assert.IsFalse(Me.ProcessComplete)
                        Assert.IsTrue(Me.ProcessFailure)
                        Assert.AreEqual(1, monitor.Queue.GetAllMessages.Count)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        Finally
            MessageQueue.Delete(queue.Path)
        End Try
    End Sub

    <Test(Description:="Test successful message queue monitor deletes message")> _
    Public Sub MessageQueueMonitorProcessorCompleteDeleteMessage()
        Dim queue As MessageQueue = MessageQueue.Create(".\Private$\SiphonTestQueue")

        Try
            queue.Send("SUCCESS")

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor = New MockProcessor
                    Using monitor As MessageQueueMonitor = New MessageQueueMonitor("LocalMonitor", queue, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                        monitor.ProcessCompleteActions = DataActions.Delete
                        monitor.Start()
                        Threading.Thread.Sleep(5000)
                        monitor.Stop()

                        Assert.AreEqual(1, processor.Count, "Has processed 1 queue message")
                        Assert.IsTrue(Me.ProcessComplete)
                        Assert.IsFalse(Me.ProcessFailure)
                        Assert.AreEqual(0, monitor.Queue.GetAllMessages.Count)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        Finally
            MessageQueue.Delete(queue.Path)
        End Try
    End Sub

    <Test(Description:="Test successful message queue monitor moves message")> _
    Public Sub MessageQueueMonitorProcessorCompleteMoveMessage()
        Dim queue As MessageQueue = MessageQueue.Create(".\Private$\SiphonTestQueue")
        Dim completeQueue As MessageQueue = MessageQueue.Create(".\Private$\SiphonTestQueueComplete")

        Try
            queue.Send("SUCCESS")

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor = New MockProcessor
                    Using monitor As MessageQueueMonitor = New MessageQueueMonitor("LocalMonitor", queue, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                        monitor.ProcessCompleteActions = DataActions.Move
                        monitor.CompleteQueue = completeQueue

                        monitor.Start()
                        Threading.Thread.Sleep(5000)
                        monitor.Stop()

                        Assert.AreEqual(1, processor.Count, "Has processed 1 queue message")
                        Assert.IsTrue(Me.ProcessComplete)
                        Assert.IsFalse(Me.ProcessFailure)
                        Assert.AreEqual(0, monitor.Queue.GetAllMessages.Count)
                        Assert.AreEqual(1, monitor.CompleteQueue.GetAllMessages.Count)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        Finally
            MessageQueue.Delete(queue.Path)
            MessageQueue.Delete(completeQueue.Path)
        End Try
    End Sub

    <Test(Description:="Test failed message queue monitor deletes message")> _
    Public Sub MessageQueueMonitorProcessorFailureDeleteMessage()
        Dim queue As MessageQueue = MessageQueue.Create(".\Private$\SiphonTestQueue")

        Try
            queue.Send("FAILURE")

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor = New MockProcessor
                    Using monitor As MessageQueueMonitor = New MessageQueueMonitor("LocalMonitor", queue, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                        monitor.ProcessFailureActions = DataActions.Delete
                        monitor.Start()
                        Threading.Thread.Sleep(5000)
                        monitor.Stop()

                        Assert.AreEqual(1, processor.Count, "Has processed 1 queue message")
                        Assert.IsFalse(Me.ProcessComplete)
                        Assert.IsTrue(Me.ProcessFailure)
                        Assert.AreEqual(0, monitor.Queue.GetAllMessages.Count)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        Finally
            MessageQueue.Delete(queue.Path)
        End Try
    End Sub

    <Test(Description:="Test failure message queue monitor moves message")> _
    Public Sub MessageQueueMonitorProcessorFailureMoveMessage()
        Dim queue As MessageQueue = MessageQueue.Create(".\Private$\SiphonTestQueue")
        Dim failureQueue As MessageQueue = MessageQueue.Create(".\Private$\SiphonTestQueueFailure")

        Try
            queue.Send("FAILURE")

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor = New MockProcessor
                    Using monitor As MessageQueueMonitor = New MessageQueueMonitor("LocalMonitor", queue, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                        monitor.ProcessFailureActions = DataActions.Move
                        monitor.FailureQueue = failureQueue

                        monitor.Start()
                        Threading.Thread.Sleep(5000)
                        monitor.Stop()

                        Assert.AreEqual(1, processor.Count, "Has processed 1 queue message")
                        Assert.IsFalse(Me.ProcessComplete)
                        Assert.IsTrue(Me.ProcessFailure)
                        Assert.AreEqual(0, monitor.Queue.GetAllMessages.Count)
                        Assert.AreEqual(1, monitor.FailureQueue.GetAllMessages.Count)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Throw
        Finally
            MessageQueue.Delete(queue.Path)
            MessageQueue.Delete(failureQueue.Path)
        End Try
    End Sub

    <Test(Description:="Create monitor from configuration")> _
    Public Sub CreateFromConfiguration()
        Dim monitorElement As New MonitorElement("TestQueueMonitor", "ChrisLaco.Siphon.MessageQueueMonitor, Siphon")
        Dim processorElement As New ProcessorElement("ChrisLaco.Tests.Siphon.MockProcessor, SiphonTests")
        Dim scheduleElement As New ScheduleElement("ChrisLaco.Siphon.DailySchedule, Siphon")
        scheduleElement.Daily.Add(DateTime.Now.AddSeconds(3).TimeOfDay)
        monitorElement.Schedule = scheduleElement
        monitorElement.Processor = processorElement
        monitorElement.Settings.Add(New NameValueConfigurationElement("Queue", ".\Private$\SiphonTestQueue"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("CompleteQueue", ".\Private$\SiphonTestQueueComplete"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("FailureQueue", ".\Private$\SiphonTestQueueFailure"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("CreateMissingQueues", "true"))

        Using monitor As MessageQueueMonitor = monitorElement.CreateInstance
            Assert.IsInstanceOfType(GetType(MessageQueueMonitor), monitor)
            Assert.IsInstanceOfType(GetType(MessageQueue), monitor.Queue)
            Assert.IsInstanceOfType(GetType(MessageQueue), monitor.CompleteQueue)
            Assert.IsInstanceOfType(GetType(MessageQueue), monitor.FailureQueue)
            Assert.AreEqual(".\Private$\SiphonTestQueue", monitor.Queue.Path)
            Assert.AreEqual(".\Private$\SiphonTestQueueComplete", monitor.CompleteQueue.Path)
            Assert.AreEqual(".\Private$\SiphonTestQueueFailure", monitor.FailureQueue.Path)
            Assert.IsTrue(monitor.CreateMissingQueues)
        End Using
    End Sub

    <Test(Description:="Start throws exception for Rename action")> _
    <ExpectedException(GetType(NotImplementedException))> _
    Public Sub StartCompleteRenameNotImplementedException()
        Using monitor As New MessageQueueMonitor("TestMonitor", ".\Private$\SiphonTestQueueFailure", New IntervalSchedule, New MockProcessor)
            monitor.ProcessCompleteActions = DataActions.Rename
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for Rename action")> _
    <ExpectedException(GetType(NotImplementedException))> _
    Public Sub StartFailureRenameNotImplementedException()
        Using monitor As New MessageQueueMonitor("TestMonitor", ".\Private$\SiphonTestQueueFailure", New IntervalSchedule, New MockProcessor)
            monitor.ProcessFailureActions = DataActions.Rename
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Queue")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub ConstructorFailureNullQueueException()
        Using monitor As MessageQueueMonitor = New MessageQueueMonitor("Name", "", New IntervalSchedule, New MockProcessor)
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Queue")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullQueueException()
        Using monitor As New MessageQueueMonitor("TestMonitor", ".\Private$\SiphonTestQueue", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As MessageQueueMonitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.MessageQueueMonitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.Name = "Test"
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Name")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullNameException()
        Using monitor As New MessageQueueMonitor("TestMonitor", ".\Private$\SiphonTestQueue", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As MessageQueueMonitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.MessageQueueMonitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.Queue = New MessageQueue(monitor.Queue.Path)
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Queue set throws exception for null Queue")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub SetNullQueueException()
        Using monitor As New MessageQueueMonitor("TestMonitor", ".\Private$\SiphonTestQueueFailure", New IntervalSchedule, New MockProcessor)
            monitor.Queue = Nothing
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Queue")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullCompleteQueueException()
        Using monitor As New MessageQueueMonitor("TestMonitor", ".\Private$\SiphonTestQueueFailure", New IntervalSchedule, New MockProcessor)
            monitor.ProcessCompleteActions = DataActions.Move
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Queue")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullFailureQueueException()
        Using monitor As New MessageQueueMonitor("TestMonitor", ".\Private$\SiphonTestQueueFailure", New IntervalSchedule, New MockProcessor)
            monitor.ProcessFailureActions = DataActions.Move
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Rename throws exception")> _
    <ExpectedException(GetType(NotImplementedException))> _
    Public Sub RenameNotImplementedException()
        Using monitor As New MessageQueueMonitor("TestMonitor", ".\Private$\SiphonTestQueueFailure", New IntervalSchedule, New MockProcessor)
            monitor.Rename(New DataItem(Of String)("Test", "Data"))
        End Using
    End Sub

End Class

