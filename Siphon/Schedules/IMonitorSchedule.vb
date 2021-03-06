﻿''' <summary>
''' Interface that defines a monitoring schedule.
''' </summary>
''' <remarks></remarks>
Public Interface IMonitorSchedule
    Inherits IDisposable

    ''' <summary>
    ''' Initializes the schedule with the specified config settings.
    ''' </summary>
    ''' <param name="config">ScheduleElement. The schedule configuraton element contianing related settings.</param>
    ''' <remarks></remarks>
    Sub Initialize(ByVal config As ScheduleElement)

    ''' <summary>
    ''' Gets the next schedule event after the given start date/time.
    ''' </summary>
    ''' <param name="start">DateTime. The schedules start date/time.</param>
    ''' <value></value>
    ''' <returns>DateTime</returns>
    ''' <remarks></remarks>
    ReadOnly Property NextEvent(ByVal start As DateTime) As DateTime

    ''' <summary>
    ''' Returns a collection of ScheduleExclusions for the current schedule.
    ''' </summary>
    ''' <value></value>
    ''' <returns>ICollection(Of ScheduleExclusion)</returns>
    ''' <remarks></remarks>
    ReadOnly Property Exclusions() As ICollection(Of ScheduleExclusion)
End Interface
