Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Schedule Tests")> _
Public Class ScheduleTests

    <TestFixtureSetUp()> _
    Public Sub TestFixtureSetupUp()
        log4net.Config.XmlConfigurator.Configure()
    End Sub

#Region "Exclusions Tests"

    <Test(Description:="Has collection of exclusions")> _
    Public Sub HasCollectionOfExclusions()
        Using schedule As IMonitorSchedule = New IntervalSchedule
            Assert.IsInstanceOfType(GetType(ICollection(Of ScheduleExclusion)), schedule.Exclusions)
            Assert.AreEqual(0, schedule.Exclusions.Count)
        End Using
    End Sub

    <Test(Description:="Can add eclusions")> _
    Public Sub CanAddExclusions()
        Using schedule As IMonitorSchedule = New IntervalSchedule
            schedule.Exclusions.Add(New ScheduleExclusion(DateTime.Now, DateTime.Now))

            Assert.AreEqual(1, schedule.Exclusions.Count)
        End Using
    End Sub

#End Region

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

    <Test(Description:="Test schedule with interval with DateTime exclusions")> _
    Public Sub IntervalScheduleWithDateTimeExclusions()
        Using schedule As IMonitorSchedule = New IntervalSchedule(New TimeSpan(1, 2, 3))
            Dim start As DateTime = DateTime.Now
            Dim from As DateTime = start.AddHours(1)
            Dim [to] As DateTime = start.AddHours(2)

            schedule.Exclusions.Add(New ScheduleExclusion(from, [to]))

            Dim nextAvailable As DateTime = schedule.NextEvent(start)

            Assert.AreEqual([to].AddSeconds(1), nextAvailable)
        End Using
    End Sub

    <Test(Description:="Test schedule with interval with TimeSpan exclusions")> _
    Public Sub IntervalScheduleWithTimeSpanExclusions()
        Using schedule As IMonitorSchedule = New IntervalSchedule(New TimeSpan(1, 2, 3))
            Dim start As DateTime = DateTime.Parse("10/10/2010 1:00AM")
            Dim from As TimeSpan = TimeSpan.Parse("2:00")
            Dim [to] As TimeSpan = TimeSpan.Parse("3:00")

            schedule.Exclusions.Add(New ScheduleExclusion(from, [to]))

            Dim nextAvailable As DateTime = schedule.NextEvent(start)

            Assert.AreEqual(DateTime.Parse("10/10/2010 3:00:01AM"), nextAvailable)
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
            Assert.AreEqual(12, ndt.Hour, "Got hour 12")
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

    <Test(Description:="Test schedule with interval with DateTime exclusions")> _
    Public Sub DailyScheduleWithDateTimeExclusions()
        Using schedule As IMonitorSchedule = New DailySchedule(New TimeSpan(1, 2, 3))
            Dim start As DateTime = DateTime.Parse("10/10/2010 12:00AM")
            Dim from As DateTime = start.AddHours(1)
            Dim [to] As DateTime = start.AddHours(2)

            schedule.Exclusions.Add(New ScheduleExclusion(from, [to]))

            Dim nextAvailable As DateTime = schedule.NextEvent(start)

            Assert.AreEqual([to].AddSeconds(1), nextAvailable)
        End Using
    End Sub

    <Test(Description:="Test schedule with interval with TimeSpan exclusions")> _
    Public Sub DailyScheduleWithTimeSpanExclusions()
        Using schedule As IMonitorSchedule = New DailySchedule(New TimeSpan(1, 2, 3))
            Dim start As DateTime = DateTime.Parse("10/10/2010 1:00AM")
            Dim from As TimeSpan = TimeSpan.Parse("1:00")
            Dim [to] As TimeSpan = TimeSpan.Parse("3:00")

            schedule.Exclusions.Add(New ScheduleExclusion(from, [to]))

            Dim nextAvailable As DateTime = schedule.NextEvent(start)

            Assert.AreEqual(DateTime.Parse("10/10/2010 3:00:01AM"), nextAvailable)
        End Using
    End Sub

#End Region

End Class
