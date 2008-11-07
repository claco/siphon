﻿Imports System.Configuration
Imports NUnit.Framework
Imports ChrisLaco.Siphon.Configuration
Imports ChrisLaco.Siphon.Schedules
Imports ChrisLaco.Siphon.Processors
Imports ChrisLaco.Siphon.Monitors
Imports ChrisLaco.Tests.Siphon.Processors

Namespace Configuration

    <TestFixture(Description:="Test configuration classes")> _
    Public Class ConfigurationTests

        <Test(Description:="Test configuration classes")> _
        Public Sub Configuration()
            Dim exePath As String = System.IO.Path.Combine(Environment.CurrentDirectory, "Configuration\test.exe")
            Dim config As System.Configuration.Configuration = ConfigurationManager.OpenExeConfiguration(exePath)
            Dim section As ChrisLaco.Siphon.Configuration.ConfigurationSection = config.GetSection("siphon")

            Assert.AreEqual(2, section.Monitors.Count, "Have monitor count")

            REM Test settingss
            Assert.AreEqual("IntervalMonitor", section.Monitors(0).Name)
            Assert.AreEqual("ChrisLaco.Siphon.Monitors.LocalDirectoryMonitor, Siphon", section.Monitors(0).Type)
            Assert.AreEqual("ChrisLaco.Siphon.Schedules.IntervalSchedule, Siphon", section.Monitors(0).Schedule.Type)
            Assert.AreEqual("ChrisLaco.Tests.Siphon.Processors.MockFileProcessor, SiphonTests", section.Monitors(0).Processor.Type)
            Assert.AreEqual(New TimeSpan(1, 2, 3, 4), section.Monitors(0).Schedule.Interval.Value)

            Assert.AreEqual("DailyMonitor", section.Monitors(1).Name)
            Assert.AreEqual("ChrisLaco.Siphon.Monitors.FtpDirectoryMonitor, Siphon", section.Monitors(1).Type)
            Assert.AreEqual("ChrisLaco.Siphon.Schedules.DailySchedule, Siphon", section.Monitors(1).Schedule.Type)
            Assert.AreEqual("ChrisLaco.Tests.Siphon.Processors.MockQueueMessageProcessor, SiphonTests", section.Monitors(1).Processor.Type)
            Assert.AreEqual(3, section.Monitors(1).Schedule.Daily.Count)
            Assert.AreEqual(New TimeSpan(1, 23, 0), section.Monitors(1).Schedule.Daily(0).Value)
            Assert.AreEqual(New TimeSpan(12, 23, 0), section.Monitors(1).Schedule.Daily(1).Value)
            Assert.AreEqual(New TimeSpan(2, 34, 56), section.Monitors(1).Schedule.Daily(2).Value)

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
            Assert.IsInstanceOfType(GetType(MockFileProcessor), processor)

            processor = section.Monitors(1).Processor.CreateInstance
            Assert.IsInstanceOfType(GetType(MockQueueMessageProcessor), processor)

            REM Test monitor create instances
            Dim monitor As IDataMonitor = section.Monitors(0).CreateInstance
            Assert.IsInstanceOfType(GetType(LocalDirectoryMonitor), monitor)
            Assert.IsInstanceOfType(GetType(IntervalSchedule), monitor.Schedule)
            Assert.AreEqual(New TimeSpan(1, 2, 3, 4), DirectCast(monitor.Schedule, IntervalSchedule).Interval)
            Assert.IsInstanceOfType(GetType(MockFileProcessor), monitor.Processor)

            monitor = section.Monitors(1).CreateInstance
            Assert.IsInstanceOfType(GetType(FtpDirectoryMonitor), monitor)
            Assert.IsInstanceOfType(GetType(DailySchedule), monitor.Schedule)
            Assert.AreEqual(3, DirectCast(monitor.Schedule, DailySchedule).Times.Count)
            Assert.AreEqual(New TimeSpan(1, 23, 0), DirectCast(monitor.Schedule, DailySchedule).Times(0))
            Assert.AreEqual(New TimeSpan(12, 23, 0), DirectCast(monitor.Schedule, DailySchedule).Times(1))
            Assert.AreEqual(New TimeSpan(2, 34, 56), DirectCast(monitor.Schedule, DailySchedule).Times(2))
            Assert.IsInstanceOfType(GetType(MockQueueMessageProcessor), monitor.Processor)

        End Sub
    End Class

End Namespace

'<monitors>
'	<monitor name="IntervalMonitor" type="ChrisLaco.Siphon.Monitors.LocalDirectoryMonitor, Siphon">
'		<schedule type="ChrisLaco.Siphon.Schedules.IntervalSchedule, Siphon">
'			<interval value="1.2:3:4" />
'		</schedule>
'		<processor type="ChrisLaco.Tests.Siphon.Processors.MockFileProcessor, SiphonTests" />
'	</monitor>
'	<monitor name="DailyMonitor" type="ChrisLaco.Siphon.Monitors.FtpDirectoryMonitor, Siphon">
'		<schedule type="ChrisLaco.Siphon.Schedules.DailySchedule, Siphon">
'			<daily>
'				<time value="1:23" />
'				<time value="12:23" />
'				<time value="2:34:56" />
'			</daily>
'		</schedule>
'		<processor type="ChrisLaco.Tests.Siphon.Processors.MockQueueMessageProcessor, SiphonTests" />
'	</monitor>
'</monitors>