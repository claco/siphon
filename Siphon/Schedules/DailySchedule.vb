Imports log4net

''' <summary>
''' Class for schedules based on times of day.
''' </summary>
''' <remarks></remarks>
Public Class DailySchedule
    Inherits MonitorSchedule

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private _times() As TimeSpan = {}

    ''' <summary>
    ''' Creates a new daily schedule instance.
    ''' </summary>
    ''' <param name="times">TimePan. An array of times to run the events each day.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal ParamArray times() As TimeSpan)
        Me.Times = times
    End Sub

    ''' <summary>
    ''' Gets/sets the times of day for the events.
    ''' </summary>
    ''' <value></value>
    ''' <returns>TimeSpan</returns>
    ''' <remarks></remarks>
    Public Overridable Property Times() As TimeSpan()
        Get
            Return _times
        End Get
        Set(ByVal value() As TimeSpan)
            _times = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the next schedule event after the given start date/time.
    ''' </summary>
    ''' <param name="start">DateTime. The schedules start date/time.</param>
    ''' <value></value>
    ''' <returns>DateTime</returns>
    ''' <remarks></remarks>
    Public Overloads Overrides ReadOnly Property NextEvent(ByVal start As DateTime) As DateTime
        Get
            Array.Sort(Me.Times)

            Dim max As New TimeSpan(0, 23, 59, 59, 999)

            For Each tp As TimeSpan In Me.Times
                If tp > max Then
                    Log.WarnFormat("Skipping TimeSpan greater than 24 hours {0}", tp)
                Else
                    If tp >= start.TimeOfDay Then
                        Return New DateTime(start.Year, start.Month, start.Day, tp.Hours, tp.Minutes, tp.Seconds)
                    End If
                End If
            Next

            Dim tomorow As DateTime = start.AddDays(1)
            Dim time As TimeSpan = Me.Times(0)

            Return New DateTime(tomorow.Year, tomorow.Month, tomorow.Day, time.Hours, time.Minutes, time.Seconds)
        End Get
    End Property
End Class
