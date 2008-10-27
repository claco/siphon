Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Schedule Tests")> _
Public Class ScheduleTests

    <TestFixtureSetUp()> _
    Public Sub TestFixtureSetupUp()
        log4net.Config.XmlConfigurator.Configure()
    End Sub

#Region "Interval Schedule Tests"

    <Test(Description:="Test schedule default")> _
    Public Sub IntervalScheduleDefault()
        Using schedule As IMonitorSchedule = New IntervalSchedule
            Dim dt As DateTime = DateTime.Now
            Dim ndt As DateTime = schedule.NextEvent(dt)
            Dim ts As TimeSpan = (ndt - dt)

            Assert.Greater(ndt, dt, "Next event is greater than now")
            Assert.AreEqual(1, ts.Minutes, "Default span of 1 minute")
        End Using
    End Sub

    <Test(Description:="Test schedule with interval")> _
    Public Sub IntervalSchedule()
        Using schedule As IMonitorSchedule = New IntervalSchedule(New TimeSpan(1, 2, 3))
            Dim dt As DateTime = DateTime.Now
            Dim ndt As DateTime = schedule.NextEvent(dt)
            Dim ts As TimeSpan = (ndt - dt)

            Assert.Greater(ndt, dt, "Next event is greater than now")
            Assert.AreEqual(1, ts.Hours, "1 hours in interval")
            Assert.AreEqual(2, ts.Minutes, "2 minutes in interval")
            Assert.AreEqual(3, ts.Seconds, "3 seconds in interval")
        End Using
    End Sub

#End Region

#Region "Daily Schedule Tests"

    <Test(Description:="Test daily schedule")> _
    Public Sub DailySchedule()
        Using schedule As IMonitorSchedule = New DailySchedule(New TimeSpan(4, 0, 0), New TimeSpan(2, 5, 0, 0), New TimeSpan(12, 30, 25), New TimeSpan(14, 0, 0))
            Dim start As DateTime
            Dim ndt As DateTime

            start = DateTime.Parse("1/1/2001 1:00 AM")
            ndt = schedule.NextEvent(start)
            Assert.AreEqual(4, ndt.Hour, "Got hour 4")
            Assert.AreEqual(start.Date, ndt.Date, "Got same date")

            start = DateTime.Parse("1/1/2001 4:00 AM")
            ndt = schedule.NextEvent(start)
            Assert.AreEqual(4, ndt.Hour, "Got hour 4")
            Assert.AreEqual(start.Date, ndt.Date, "Got same date")

            start = DateTime.Parse("1/1/2001 4:10 AM")
            ndt = schedule.NextEvent(start)
            Assert.AreEqual(12, ndt.Hour, "Got hour 12")
            Assert.AreEqual(start.Date, ndt.Date, "Got same date")

            start = DateTime.Parse("1/1/2001 12:30 PM")
            ndt = schedule.NextEvent(start)
            Assert.AreEqual(12, ndt.Hour, "Got hour 12")
            Assert.AreEqual(start.Date, ndt.Date, "Got same date")

            start = DateTime.Parse("1/1/2001 12:30:45 PM")
            ndt = schedule.NextEvent(start)
            Assert.AreEqual(14, ndt.Hour, "Got hour 14")
            Assert.AreEqual(start.Date, ndt.Date, "Got same date")

            start = DateTime.Parse("1/1/2001 15:00 PM")
            ndt = schedule.NextEvent(start)
            Assert.AreEqual(4, ndt.Hour, "Got hour 4")
            Assert.AreEqual(start.AddDays(1).Date, ndt.Date, "Got next day")
        End Using
    End Sub

#End Region

End Class
