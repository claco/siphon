''' <summary>
''' Class used to exclude specific times from the normal schedule.
''' </summary>
''' <remarks></remarks>
Public Class ScheduleExclusion

    Private _from As DateTime
    Private _to As DateTime

    ''' <summary>
    ''' Creates a new ScheduleExclusion using specific start/stop times.
    ''' </summary>
    ''' <param name="from">DateTime. The start of the schedules exclusion.</param>
    ''' <param name="to">DateTime. The end of the schedules exclusion.</param>
    ''' <remarks>This is normally used to exclude a specific date range: 12/30/2009 1:00PM to 1:2/2009 3:00PM</remarks>
    Public Sub New(ByVal from As DateTime, ByVal [to] As DateTime)
        _from = from
        _to = [to]
    End Sub

    ''' <summary>
    ''' Creates a new ScheduleExclusion using relative times.
    ''' </summary>
    ''' <param name="from">TimeSpan. The start of the schedules exclusion.</param>
    ''' <param name="to">TimeSpan. The end of the schedules exclusion.</param>
    ''' <remarks>This is normally used to exclude a specific time of day, regardless of the date: 1:00PM to 3:00PM</remarks>
    Public Sub New(ByVal from As TimeSpan, ByVal [to] As TimeSpan)
        _from = Date.MinValue + from
        _to = Date.MinValue + [to]
    End Sub

    ''' <summary>
    ''' Returns the next available DateTime, checking if the start is within the current exclusion.
    ''' </summary>
    ''' <param name="start">DateTime. The proposed next date time.</param>
    ''' <returns>DateTime</returns>
    ''' <remarks>If the start is within the exclusions range, the next available DateTime will be returned. If start is not within the range, it will be returned as is.</remarks>
    Public Function NextAvailable(ByVal start As DateTime) As DateTime
        If _from.Date = Date.MinValue AndAlso _to.Date = Date.MinValue Then
            If start.TimeOfDay >= _from.TimeOfDay And start.TimeOfDay <= _to.TimeOfDay Then
                Return DateTime.Parse(start.Date.ToShortDateString).AddTicks(_to.Ticks).AddSeconds(1)
            Else
                Return start
            End If
        Else
            If start >= _from And start <= _to Then
                Return _to.AddSeconds(1)
            Else
                Return start
            End If
        End If
    End Function
End Class
