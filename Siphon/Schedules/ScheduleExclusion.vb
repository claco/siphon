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
    ''' Creates a new ScheduleExclusion using strings containing DateTime or TimeSpan formats.
    ''' </summary>
    ''' <param name="from">String. The start of the schedules exclusion.</param>
    ''' <param name="to">String. The end of the schedules exclusion.</param>
    ''' <remarks>This is used to load an exclusion dynamically from strings read from config.</remarks>
    Public Sub New(ByVal from As String, ByVal [to] As String)
        Dim ts As TimeSpan
        Dim dt As DateTime

        If TimeSpan.TryParse(from, ts) AndAlso TimeSpan.TryParse([to], ts) Then
            _from = Date.MinValue + TimeSpan.Parse(from)
            _to = Date.MinValue + TimeSpan.Parse([to])
        ElseIf DateTime.TryParse(from, dt) AndAlso DateTime.TryParse([to], dt) Then
            _from = from
            _to = [to]
        Else
            Throw New FormatException("From/To must both be TimeSpan or Datetime parsable strings")
        End If
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
