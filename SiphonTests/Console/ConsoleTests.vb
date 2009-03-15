Imports System.IO
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Console Tests")> _
Public Class ConsoleTests
    Inherits TestBase

    <Test(Description:="Console loads configuration")> _
    Public Sub ConsoleConfiguration()
        Using console As New SiphonConsole
            Assert.AreEqual(2, console.Monitors.Count)

            Dim monitor As IDataMonitor = console.Monitors(0)
            Assert.IsInstanceOfType(GetType(LocalDirectoryMonitor), monitor)
            Assert.IsInstanceOfType(GetType(IntervalSchedule), monitor.Schedule)
            Assert.AreEqual("C:\Temp", DirectCast(monitor, LocalDirectoryMonitor).Path)
            Assert.AreEqual("*.tmp", DirectCast(monitor, LocalDirectoryMonitor).Filter)
            Assert.AreEqual(New TimeSpan(1, 2, 3, 4), DirectCast(monitor.Schedule, IntervalSchedule).Interval)
            Assert.IsInstanceOfType(GetType(MockProcessor), monitor.Processor)

            monitor = console.Monitors(1)
            Assert.IsInstanceOfType(GetType(FtpDirectoryMonitor), monitor)
            Assert.IsInstanceOfType(GetType(DailySchedule), monitor.Schedule)
            Assert.AreEqual("/", DirectCast(monitor, FtpDirectoryMonitor).Path)
            Assert.AreEqual("ftp://foo.bar.baz/", DirectCast(monitor, FtpDirectoryMonitor).Uri.ToString)
            Assert.AreEqual(3, DirectCast(monitor.Schedule, DailySchedule).Times.Count)
            Assert.AreEqual(New TimeSpan(1, 23, 0), DirectCast(monitor.Schedule, DailySchedule).Times(0))
            Assert.AreEqual(New TimeSpan(12, 23, 0), DirectCast(monitor.Schedule, DailySchedule).Times(1))
            Assert.AreEqual(New TimeSpan(2, 34, 56), DirectCast(monitor.Schedule, DailySchedule).Times(2))
            Assert.IsInstanceOfType(GetType(MockProcessor), monitor.Processor)
        End Using
    End Sub

    <Test(Description:="Run actual monitor")> _
    Public Sub Console()
        Using console As New SiphonConsole
            CreateSuccessFile()

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor As New MockProcessor
                    Dim monitor As New LocalDirectoryMonitor("TestLocalDirectoryMonitor", TestDirectory.FullName, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    console.Monitors.Clear()
                    console.Monitors.Add(monitor)

                    Dim args() As String = {"process", "all"}
                    console.Run(args)

                    Assert.AreEqual(1, processor.Count)
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                    Assert.AreEqual(0, Environment.ExitCode)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Run actual monitor by name")> _
    Public Sub ConsoleByName()
        Using console As New SiphonConsole
            CreateSuccessFile()

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor As New MockProcessor
                    Dim monitor As New LocalDirectoryMonitor("TestLocalDirectoryMonitor", TestDirectory.FullName, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    console.Monitors.Clear()
                    console.Monitors.Add(monitor)

                    Dim args() As String = {"process", "TestLocalDirectoryMonitor"}
                    console.Run(args)

                    Assert.AreEqual(1, processor.Count)
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                    Assert.AreEqual(0, Environment.ExitCode)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Run no monitor by name")> _
    Public Sub ConsoleByNameNotFound()
        Using console As New SiphonConsole
            CreateSuccessFile()

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor As New MockProcessor
                    Dim monitor As New LocalDirectoryMonitor("TestLocalDirectoryMonitor", TestDirectory.FullName, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    console.Monitors.Clear()
                    console.Monitors.Add(monitor)

                    Dim args() As String = {"process", "Mispelled"}
                    console.Run(args)

                    Assert.AreEqual(0, processor.Count)
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                    Assert.AreEqual(1, Environment.ExitCode)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Exit code no args")> _
    Public Sub ConsoleNoMonitors()
        Using console As New SiphonConsole
            Dim args() As String = {}
            console.Run(args)

            Assert.AreEqual(1, Environment.ExitCode)
        End Using
    End Sub

    <Test(Description:="Monitor exception does not kill console")> _
    Public Sub ServiceMonitorException()
        Using console As New SiphonConsole
            CreateExceptionFile()

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor As New MockProcessor
                    Using monitor As New LocalDirectoryMonitor("TestLocalDirectoryMonitor", TestDirectory.FullName, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                        console.Monitors.Clear()
                        console.Monitors.Add(monitor)

                        Dim args() As String = {"process", "all"}
                        console.Run(args)

                        Assert.AreEqual(1, processor.Count)
                        Assert.IsFalse(Me.ProcessComplete)
                        Assert.IsTrue(Me.ProcessFailure)
                    End Using
                End Using
            End Using
        End Using
    End Sub
End Class
