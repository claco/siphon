Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Schedule Exclusion Tests")> _
Public Class ExclusionTests

    <TestFixtureSetUp()> _
    Public Sub TestFixtureSetupUp()
        log4net.Config.XmlConfigurator.Configure()
    End Sub

#Region "DateTime Tests"

    <Test(Description:="Can create an exclusion with DateTime")> _
    Public Sub CanCreateUsingDateTime()
        Dim exclusion As New ScheduleExclusion(DateTime.Now, DateTime.Now.AddHours(1))

        Assert.IsInstanceOfType(GetType(ScheduleExclusion), exclusion)
    End Sub

    <Test(Description:="Returns first available DateTime after excluded DateTime range")> _
    Public Sub ReturnsFirstAvailableDateTimeAfterDateTimeRange()
        Dim from As DateTime = DateTime.Now
        Dim [to] As DateTime = from.AddHours(1)
        Dim nextAvailable As DateTime = [to].AddSeconds(1)

        Dim exclusion As New ScheduleExclusion(from, [to])

        Assert.AreEqual(nextAvailable, exclusion.NextAvailable(from.AddMinutes(2)))
    End Sub

    <Test(Description:="Returns start when out of excluded DateTime range")> _
    Public Sub ReturnsStartWhenOutOfDateTimeRange()
        Dim from As DateTime = DateTime.Now
        Dim [to] As DateTime = from.AddHours(1)
        Dim start As DateTime = [to].AddMinutes(10)

        Dim exclusion As New ScheduleExclusion(from, [to])

        Assert.AreEqual(start, exclusion.NextAvailable(start))
    End Sub

    <Test(Description:="Returns first available DateTime after excluded DateTime range when start is to")> _
    Public Sub ReturnsFirstAvailableDateTimeWhenDateTimeStartIsTo()
        Dim from As DateTime = DateTime.Now
        Dim [to] As DateTime = from.AddHours(1)
        Dim nextAvailable As DateTime = [to].AddSeconds(1)

        Dim exclusion As New ScheduleExclusion(from, [to])

        Assert.AreEqual(nextAvailable, exclusion.NextAvailable([to]))
    End Sub

    <Test(Description:="Returns first available DateTime after excluded DateTime range when start is from")> _
    Public Sub ReturnsFirstAvailableDateTimeWhenStartIsFrom()
        Dim from As DateTime = DateTime.Now
        Dim [to] As DateTime = from.AddHours(1)
        Dim nextAvailable As DateTime = [to].AddSeconds(1)

        Dim exclusion As New ScheduleExclusion(from, [to])

        Assert.AreEqual(nextAvailable, exclusion.NextAvailable(from))
    End Sub

#End Region

#Region "TimeSpan Tests"

    <Test(Description:="Can create an exclusion with TimeSpan")> _
    Public Sub CanCreateUsingTimeSpan()
        Dim exclusion As New ScheduleExclusion(TimeSpan.Parse("1:20:00"), TimeSpan.Parse("2:30:00"))

        Assert.IsInstanceOfType(GetType(ScheduleExclusion), exclusion)
    End Sub

    <Test(Description:="Returns first available DateTime after excluded TimeSpan range")> _
    Public Sub ReturnsFirstAvailableDateTimeAfterTimeSpanRange()
        Dim from As TimeSpan = TimeSpan.Parse("1:20")
        Dim [to] As TimeSpan = TimeSpan.Parse("2:30")
        Dim start As DateTime = DateTime.Parse("10/10/2010 1:30AM")
        Dim nextAvailable As DateTime = DateTime.Parse("10/10/2010 2:30:01AM")

        Dim exclusion As New ScheduleExclusion(from, [to])

        Assert.AreEqual(nextAvailable, exclusion.NextAvailable(start))
    End Sub

    <Test(Description:="Returns start when out of excluded TimeStamp range")> _
    Public Sub ReturnsStartWhenOutOfTimeStampRange()
        Dim from As TimeSpan = TimeSpan.Parse("1:20")
        Dim [to] As TimeSpan = TimeSpan.Parse("2:30")
        Dim start As DateTime = DateTime.Parse("10/10/2010 2:34AM")

        Dim exclusion As New ScheduleExclusion(from, [to])

        Assert.AreEqual(start, exclusion.NextAvailable(start))
    End Sub

    <Test(Description:="Returns first available DateTime after excluded TimeSpan range when start is to")> _
    Public Sub ReturnsFirstAvailableDateTimeWhenTimeSpanStartIsTo()
        Dim from As TimeSpan = TimeSpan.Parse("1:20")
        Dim [to] As TimeSpan = TimeSpan.Parse("2:30")
        Dim start As DateTime = DateTime.Parse("10/10/2010 2:30:00AM")
        Dim nextAvailable As DateTime = DateTime.Parse("10/10/2010 2:30:01AM")

        Dim exclusion As New ScheduleExclusion(from, [to])

        Assert.AreEqual(nextAvailable, exclusion.NextAvailable(start))
    End Sub

    <Test(Description:="Returns first available DateTime after excluded TimeSpan range when start is from")> _
    Public Sub ReturnsFirstAvailableDateTimeWhenTimeSpanStartIsFrom()
        Dim from As TimeSpan = TimeSpan.Parse("1:20")
        Dim [to] As TimeSpan = TimeSpan.Parse("2:30")
        Dim start As DateTime = DateTime.Parse("10/10/2010 1:20AM")
        Dim nextAvailable As DateTime = DateTime.Parse("10/10/2010 2:30:01AM")

        Dim exclusion As New ScheduleExclusion(from, [to])

        Assert.AreEqual(nextAvailable, exclusion.NextAvailable(start))
    End Sub
#End Region

End Class
