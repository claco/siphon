Imports System.Configuration
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Test configuration classes")> _
Public Class ConfigurationTests
    Inherits TestBase

    <Test(Description:="Test configuration classes")> _
    Public Sub Configuration()
        Dim exePath As String = System.IO.Path.Combine(Environment.CurrentDirectory, "SiphonTests.dll")
        Dim config As System.Configuration.Configuration = ConfigurationManager.OpenExeConfiguration(exePath)
        Dim section As SiphonConfigurationSection = config.GetSection("siphon")

        Assert.AreEqual(2, section.Monitors.Count, "Have monitor count")

        REM Test settingss
        Assert.AreEqual("IntervalMonitor", section.Monitors(0).Name)
        Assert.AreEqual("ChrisLaco.Siphon.LocalDirectoryMonitor, Siphon", section.Monitors(0).Type)
        Assert.AreEqual(4, section.Monitors(0).Settings.Count)
        Assert.AreEqual("C:\Temp", section.Monitors(0).Settings("Path").Value)
        Assert.AreEqual("*.tmp", section.Monitors(0).Settings("Filter").Value)
        Assert.AreEqual("Rename, Move", section.Monitors(0).Settings("ProcessCompleteActions").Value)
        Assert.AreEqual("Delete", section.Monitors(0).Settings("ProcessFailureActions").Value)
        Assert.AreEqual("ChrisLaco.Siphon.IntervalSchedule, Siphon", section.Monitors(0).Schedule.Type)
        Assert.AreEqual("ChrisLaco.Tests.Siphon.MockProcessor, SiphonTests", section.Monitors(0).Processor.Type)
        Assert.AreEqual(New TimeSpan(1, 2, 3, 4), section.Monitors(0).Schedule.Interval.Value)
        Assert.AreEqual(2, section.Monitors(0).Schedule.Exclusions.Count)
        Assert.AreEqual("1/1/2007 12:31PM", section.Monitors(0).Schedule.Exclusions(0).From)
        Assert.AreEqual("1/3/2007 2:31PM", section.Monitors(0).Schedule.Exclusions(0).To)
        Assert.AreEqual("1:2:3", section.Monitors(0).Schedule.Exclusions(1).From)
        Assert.AreEqual("2:3:4", section.Monitors(0).Schedule.Exclusions(1).To)

        Assert.AreEqual("DailyMonitor", section.Monitors(1).Name)
        Assert.AreEqual("ChrisLaco.Siphon.FtpDirectoryMonitor, Siphon", section.Monitors(1).Type)
        Assert.AreEqual(1, section.Monitors(1).Settings.Count)
        Assert.AreEqual("ftp://foo.bar.baz/", section.Monitors(1).Settings("Path").Value)
        Assert.AreEqual("ChrisLaco.Siphon.DailySchedule, Siphon", section.Monitors(1).Schedule.Type)
        Assert.AreEqual("ChrisLaco.Tests.Siphon.MockProcessor, SiphonTests", section.Monitors(1).Processor.Type)
        Assert.AreEqual(3, section.Monitors(1).Schedule.Daily.Count)
        Assert.AreEqual(New TimeSpan(1, 23, 0), section.Monitors(1).Schedule.Daily(0).Value)
        Assert.AreEqual(New TimeSpan(12, 23, 0), section.Monitors(1).Schedule.Daily(1).Value)
        Assert.AreEqual(New TimeSpan(2, 34, 56), section.Monitors(1).Schedule.Daily(2).Value)
        Assert.AreEqual(0, section.Monitors(1).Schedule.Exclusions.Count)

        REM Test schedule create instances
        Dim schedule As IMonitorSchedule = section.Monitors(0).Schedule.CreateInstance
        Assert.IsInstanceOfType(GetType(IntervalSchedule), schedule)
        Assert.AreEqual(New TimeSpan(1, 2, 3, 4), DirectCast(schedule, IntervalSchedule).Interval)

        schedule = section.Monitors(1).Schedule.CreateInstance
        Assert.IsInstanceOfType(GetType(DailySchedule), schedule)
        Assert.AreEqual(3, DirectCast(schedule, DailySchedule).Times.Count)
        Assert.AreEqual(New TimeSpan(1, 23, 0), DirectCast(schedule, DailySchedule).Times(0))
        Assert.AreEqual(New TimeSpan(12, 23, 0), DirectCast(schedule, DailySchedule).Times(1))
        Assert.AreEqual(New TimeSpan(2, 34, 56), DirectCast(schedule, DailySchedule).Times(2))

        REM Test processor create instancess
        Dim processor As IDataProcessor = section.Monitors(0).Processor.CreateInstance
        Assert.IsInstanceOfType(GetType(MockProcessor), processor)

        processor = section.Monitors(1).Processor.CreateInstance
        Assert.IsInstanceOfType(GetType(MockProcessor), processor)

        REM Test monitor create instances
        Dim monitor As IDataMonitor = section.Monitors("IntervalMonitor").CreateInstance
        Assert.IsInstanceOfType(GetType(LocalDirectoryMonitor), monitor)
        Assert.IsInstanceOfType(GetType(IntervalSchedule), monitor.Schedule)
        Assert.AreEqual("C:\Temp", DirectCast(monitor, LocalDirectoryMonitor).Path)
        Assert.AreEqual("*.tmp", DirectCast(monitor, LocalDirectoryMonitor).Filter)
        Assert.AreEqual(New TimeSpan(1, 2, 3, 4), DirectCast(monitor.Schedule, IntervalSchedule).Interval)
        Assert.IsInstanceOfType(GetType(MockProcessor), monitor.Processor)
        Assert.AreEqual(DataActions.Rename Or DataActions.Move, monitor.ProcessCompleteActions)
        Assert.AreEqual(DataActions.Delete, monitor.ProcessFailureActions)
        Assert.AreEqual(2, monitor.Schedule.Exclusions.Count)

        monitor = section.Monitors(1).CreateInstance
        Assert.IsInstanceOfType(GetType(FtpDirectoryMonitor), monitor)
        Assert.IsInstanceOfType(GetType(DailySchedule), monitor.Schedule)
        Assert.AreEqual("/", DirectCast(monitor, FtpDirectoryMonitor).Path)
        Assert.AreEqual("ftp://foo.bar.baz/", DirectCast(monitor, FtpDirectoryMonitor).Uri.ToString)
        Assert.AreEqual(3, DirectCast(monitor.Schedule, DailySchedule).Times.Count)
        Assert.AreEqual(New TimeSpan(1, 23, 0), DirectCast(monitor.Schedule, DailySchedule).Times(0))
        Assert.AreEqual(New TimeSpan(12, 23, 0), DirectCast(monitor.Schedule, DailySchedule).Times(1))
        Assert.AreEqual(New TimeSpan(2, 34, 56), DirectCast(monitor.Schedule, DailySchedule).Times(2))
        Assert.IsInstanceOfType(GetType(MockProcessor), monitor.Processor)
    End Sub

    <Test(Description:="Config Exception when path is empty")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub PathConfigurationException()
        Dim monitor As New MonitorElement("Test")
        monitor.Settings.Add(New NameValueConfigurationElement("Path", String.Empty))

        Dim schedule As New ScheduleElement
        schedule.Type = "ChrisLaco.Siphon.IntervalSchedule, Siphon"
        schedule.Interval = New IntervalElement(New TimeSpan(100))
        monitor.Schedule = schedule

        Dim processor As New ProcessorElement
        processor.Type = "ChrisLaco.Tests.Siphon.MockProcessor, SiphonTests"
        monitor.Processor = processor

        Dim directory As New LocalDirectoryMonitor("Test", "C:\", New IntervalSchedule, New MockProcessor)
        directory.Initialize(monitor)
    End Sub

    <Test(Description:="Config empty when given bogus section")> _
    Public Sub BadSectionEmptyConfig()
        Dim section As SiphonConfigurationSection = SiphonConfigurationSection.GetSection("crap")

        Assert.IsInstanceOfType(GetType(SiphonConfigurationSection), section)
        Assert.AreEqual(0, section.Monitors.Count)
    End Sub

    <Test(Description:="Generate config file from config classes")> _
    Public Sub GenerateConfig()
        IO.File.Create(IO.Path.Combine(TestDirectory.FullName, "test.exe")).Dispose()

        Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(TestDirectory.FullName & "/test.exe")
        Dim section As New SiphonConfigurationSection
        Dim monitor As New MonitorElement("TestMonitor", "LocalDirectoryMonitor, Siphon")
        monitor.Settings.Add(New NameValueConfigurationElement("Path", "C:\"))
        monitor.Processor = New ProcessorElement("MockProcessor, SiphonTests")
        monitor.Processor.Settings.Add(New NameValueConfigurationElement("Foo", "Bar"))
        monitor.Schedule.Type = "IntervalSchedule, Siphon"
        monitor.Schedule.Daily.Add(New TimeElement(New TimeSpan(100)))
        monitor.Schedule.Daily.Add(New TimeSpan(200))
        monitor.Schedule.Exclusions.Add("1:2:3", "4:5:6")
        section.Monitors.Add(monitor)

        config.Sections.Add("siphon", section)
        config.Save()

        Assert.AreEqual(IO.File.ReadAllText("generated.config"), IO.File.ReadAllText(TestDirectory.FullName & "/test.exe.config"))
    End Sub
End Class
