Imports log4net

''' <summary>
''' Class for schedules based on time intervals.
''' </summary>
''' <remarks></remarks>
Public Class IntervalSchedule
    Inherits MonitorSchedule

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private _interval As New TimeSpan(0, 1, 0)

    ''' <summary>
    ''' Creates a new interval schedule instance.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new interval schedule instance.
    ''' </summary>
    ''' <param name="seconds">Integer. The number if seconds for this interval.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal seconds As Integer)
        Me.Interval = New TimeSpan(0, 0, seconds)
    End Sub

    ''' <summary>
    ''' Creates a new interval schedule instance.
    ''' </summary>
    ''' <param name="interval">TimeSpan. The interval to wait between schedule events.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal interval As TimeSpan)
        Me.Interval = interval
    End Sub

    ''' <summary>
    ''' Gets/sets the interval to wait between schedule events.
    ''' </summary>
    ''' <value></value>
    ''' <returns>TimeSpan</returns>
    ''' <remarks></remarks>
    Public Overridable Property Interval() As TimeSpan
        Get
            Return _interval
        End Get
        Set(ByVal value As TimeSpan)
            _interval = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the next schedule event after the given start date/time.
    ''' </summary>
    ''' <param name="start">DateTime. The schedules start date/time.</param>
    ''' <value></value>
    ''' <returns>DateTime</returns>
    ''' <remarks></remarks>
    Public Overrides ReadOnly Property NextEvent(ByVal start As DateTime) As DateTime
        Get
            Return start.Add(Me.Interval)
        End Get
    End Property
End Class
