Imports log4net
Imports ChrisLaco.Siphon.Configuration

Namespace Schedules
    ''' <summary>
    ''' Base class for monitoring schedules.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class MonitorSchedule
        Implements IMonitorSchedule

        Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
        Private _disposed As Boolean

        ''' <summary>
        ''' Creates a new monitor schedule instance.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub New()

        End Sub

        ''' <summary>
        ''' Initializes the schedule with the specified configuration.
        ''' </summary>
        ''' <param name="config">ScheduleElement. The schedule configuration containing schedule settings.</param>
        ''' <remarks></remarks>
        Public MustOverride Sub Initialize(ByVal config As ScheduleElement) Implements IMonitorSchedule.Initialize

        ''' <summary>
        ''' Gets the next schedule event after the given start date/time.
        ''' </summary>
        ''' <param name="start">DateTime. The schedules start date/time.</param>
        ''' <value></value>
        ''' <returns>DateTime</returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property NextEvent(ByVal start As DateTime) As DateTime Implements IMonitorSchedule.NextEvent

        ''' <summary>
        ''' Disposes the current schedule instance.
        ''' </summary>
        ''' <param name="disposing">Boolean. True if we're disposing.</param>
        ''' <remarks></remarks>
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not _disposed Then
                If disposing Then

                End If
            End If

            _disposed = True
        End Sub

        ''' <summary>
        ''' Disposes the current schedule instance.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
