﻿''' <summary>
''' Interface that defines a monitoring schedule.
''' </summary>
''' <remarks></remarks>
Public Interface IMonitorSchedule
    Inherits IDisposable

    ''' <summary>
    ''' Gets the next schedule event after the given start date/time.
    ''' </summary>
    ''' <param name="start">DateTime. The schedules start date/time.</param>
    ''' <value></value>
    ''' <returns>DateTime</returns>
    ''' <remarks></remarks>
    ReadOnly Property NextEvent(ByVal start As DateTime) As DateTime
End Interface