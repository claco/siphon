Imports System.IO
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Service Tests")> _
Public Class ServiceTests
    Inherits TestBase

    <Test(Description:="Service loads configuration")> _
    Public Sub ServiceConfiguration()
        Using service As New SiphonService
            Assert.AreEqual(2, service.Monitors.Count)

            Dim monitor As IDataMonitor = service.Monitors(0)
            Assert.IsInstanceOfType(GetType(LocalDirectoryMonitor), monitor)
            Assert.IsInstanceOfType(GetType(IntervalSchedule), monitor.Schedule)
            Assert.AreEqual("C:\Temp", DirectCast(monitor, LocalDirectoryMonitor).Path)
            Assert.AreEqual("*.tmp", DirectCast(monitor, LocalDirectoryMonitor).Filter)
            Assert.AreEqual(New TimeSpan(1, 2, 3, 4), DirectCast(monitor.Schedule, IntervalSchedule).Interval)
            Assert.IsInstanceOfType(GetType(MockProcessor), monitor.Processor)

            monitor = service.Monitors(1)
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
    Public Sub Service()
        Using service As New SiphonService
            CreateSuccessFile()

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor As New MockProcessor
                    Using monitor As New LocalDirectoryMonitor("TestLocalDirectoryMonitor", TestDirectory.FullName, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                        service.Monitors.Clear()
                        service.Monitors.Add(monitor)

                        Dim args() As String = {}
                        Dim parameters() As Object = {args}
                        service.GetType.GetMethod("OnStart", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).Invoke(service, parameters)
                        Threading.Thread.Sleep(3000)
                        service.GetType.GetMethod("OnStop", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).Invoke(service, Nothing)

                        Assert.AreEqual(1, processor.Count)
                        Assert.IsTrue(Me.ProcessComplete)
                        Assert.IsFalse(Me.ProcessFailure)
                    End Using
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Pause/Continue actual monitor")> _
    Public Sub ServicePauseContinue()
        Using service As New SiphonService
            CreateSuccessFile()

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(7).TimeOfDay)
                Using processor As New MockProcessor
                    Using monitor As New LocalDirectoryMonitor("TestLocalDirectoryMonitor", TestDirectory.FullName, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                        service.Monitors.Clear()
                        service.Monitors.Add(monitor)

                        Dim args() As String = {}
                        Dim parameters() As Object = {args}
                        service.GetType.GetMethod("OnStart", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).Invoke(service, parameters)
                        service.GetType.GetMethod("OnPause", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).Invoke(service, Nothing)
                        Threading.Thread.Sleep(5000)
                        Assert.AreEqual(0, processor.Count)
                        service.GetType.GetMethod("OnContinue", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).Invoke(service, Nothing)
                        Threading.Thread.Sleep(4000)
                        service.GetType.GetMethod("OnStop", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).Invoke(service, Nothing)

                        Assert.AreEqual(1, processor.Count)
                        Assert.IsTrue(Me.ProcessComplete)
                        Assert.IsFalse(Me.ProcessFailure)
                    End Using
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Monitor exception does not kill service")> _
    Public Sub ServiceMonitorException()
        Using service As New SiphonService
            CreateExceptionFile()

            Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
                Using processor As New MockProcessor
                    Using monitor As New LocalDirectoryMonitor("TestLocalDirectoryMonitor", TestDirectory.FullName, schedule, processor)
                        AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                        AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                        service.Monitors.Clear()
                        service.Monitors.Add(monitor)

                        Dim args() As String = {}
                        Dim parameters() As Object = {args}
                        service.GetType.GetMethod("OnStart", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).Invoke(service, parameters)
                        Threading.Thread.Sleep(4000)
                        service.GetType.GetMethod("OnStop", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).Invoke(service, Nothing)

                        Assert.AreEqual(1, processor.Count)
                        Assert.IsFalse(Me.ProcessComplete)
                        Assert.IsTrue(Me.ProcessFailure)
                    End Using
                End Using
            End Using
        End Using
    End Sub
End Class
