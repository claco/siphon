Imports System.IO
Imports NUnit.Framework
Imports ChrisLaco.Siphon.Monitors
Imports ChrisLaco.Siphon.Schedules
Imports ChrisLaco.Siphon.Console
Imports ChrisLaco.Tests.Siphon.Processors

Namespace Console
    <TestFixture(Description:="Console Tests")> _
    Public Class ConsoleTests

        <TestFixtureSetUp()> _
        Public Sub TestFixtureSetupUp()
            log4net.Config.XmlConfigurator.Configure()
        End Sub

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
                Assert.AreEqual("ftp://foo.bar.baz/", DirectCast(monitor, FtpDirectoryMonitor).Path)
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
                Dim tempdir As DirectoryInfo = Directory.CreateDirectory(Path.Combine(Path.GetTempPath, Path.GetRandomFileName))
                File.Create(Path.Combine(tempdir.FullName, "SUCCESS"))

                Dim processor As New MockProcessor
                Dim monitor As New LocalDirectoryMonitor("TestLocalDirectoryMonitor", tempdir.FullName, New IntervalSchedule(2), processor)

                console.Monitors.Clear()
                console.Monitors.Add(monitor)

                Dim args() As String = {"all"}
                console.Run(args)

                Assert.AreEqual(1, processor.Count)
            End Using
        End Sub

        <Test(Description:="Monitor exception does not kill console")> _
        Public Sub ServiceMonitorException()
            Using console As New SiphonConsole
                Dim tempdir As DirectoryInfo = Directory.CreateDirectory(Path.Combine(Path.GetTempPath, Path.GetRandomFileName))
                File.Create(Path.Combine(tempdir.FullName, "EXCEPTION"))

                Dim processor As New MockProcessor
                Dim monitor As New LocalDirectoryMonitor("TestLocalDirectoryMonitor", tempdir.FullName, New IntervalSchedule(2), processor)

                console.Monitors.Clear()
                console.Monitors.Add(monitor)

                Dim args() As String = {"all"}
                console.Run(args)

                Assert.AreEqual(1, processor.Count)
            End Using
        End Sub
    End Class
End Namespace