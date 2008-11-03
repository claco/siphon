Imports System.Configuration
Imports NUnit.Framework
Imports ChrisLaco.Siphon.Configuration

<TestFixture(Description:="Test configuration classes")> _
Public Class ConfigurationTests

    <Test(Description:="Test configuration classes")> _
    Public Sub Configuration()
        Dim exePath As String = System.IO.Path.Combine(Environment.CurrentDirectory, "Configuration\test.exe")
        Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(exePath)
        Dim section As ChrisLaco.Siphon.Configuration.ConfigurationSection = config.GetSection("siphon")

        Assert.AreEqual(2, section.Monitors.Count, "Have monitor count")

        Assert.AreEqual("IntervalMonitor", section.Monitors(0).Name)
        Assert.AreEqual("ChrisLaco.Siphon.Monitors.DirectoryMonitor, siphon", section.Monitors(0).Type)
        Assert.AreEqual("ChrisLaco.Siphon.Schedules.IntervalSchedule, siphon", section.Monitors(0).Schedule.Type)
        Assert.AreEqual(New TimeSpan(1, 2, 3, 4), section.Monitors(0).Schedule.Interval.Value)

        Assert.AreEqual("DailyMonitor", section.Monitors(1).Name)
        Assert.AreEqual("ChrisLaco.Siphon.Monitors.DirectoryMonitor, siphon", section.Monitors(1).Type)
        Assert.AreEqual("ChrisLaco.Siphon.Schedules.DailySchedule, siphon", section.Monitors(1).Schedule.Type)
        Assert.AreEqual(3, section.Monitors(1).Schedule.Daily.Count)
        Assert.AreEqual(New TimeSpan(1, 23, 0), section.Monitors(1).Schedule.Daily(0).Value)
        Assert.AreEqual(New TimeSpan(12, 23, 0), section.Monitors(1).Schedule.Daily(1).Value)
        Assert.AreEqual(New TimeSpan(2, 34, 56), section.Monitors(1).Schedule.Daily(2).Value)
    End Sub

End Class

'<monitors>
'	<monitor name="IntervalMonitor" type="ChrisLaco.Siphon.Monitors.DirectoryMonitor, siphon">
'		<schedule type="ChrisLaco.Siphon.Schedules.IntervalSchedule, siphon">
'			<interval value="1.2:3:4" />
'		</schedule>
'	</monitor>
'	<monitor name="DailyMonitor" type="ChrisLaco.Siphon.Monitors.DirectoryMonitor, siphon">
'		<schedule type="ChrisLaco.Siphon.Schedules.DailySchedule, siphon">
'			<daily>
'				<time value="1:23" />
'				<time value="12:23" />
'				<time value="2:34:56" />
'			</daily>
'		</schedule>
'	</monitor>
'</monitors>