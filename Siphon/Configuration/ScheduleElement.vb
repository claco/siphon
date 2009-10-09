Imports System.Configuration
Imports System.ComponentModel
Imports System.Reflection

''' <summary>
''' The configuraton for a schedule.
''' </summary>
''' <remarks></remarks>
Public Class ScheduleElement
    Inherits ConfigurationElement

    ''' <summary>
    ''' Creates a new schedule element.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new schedule element.
    ''' </summary>
    ''' <param name="type">String. The type of the new schedule.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal type As String)
        Me.Type = type
    End Sub

    ''' <summary>
    ''' Gets/sets the type of the schedule to load.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    <ConfigurationProperty("type", IsRequired:=True)> _
    Public Overridable Property Type() As String
        Get
            Return Me.Item("type")
        End Get
        Set(ByVal value As String)
            Me.Item("type") = value.Trim
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the interval for use by a schedule.
    ''' </summary>
    ''' <value></value>
    ''' <returns>TimeSpan</returns>
    ''' <remarks></remarks>
    <ConfigurationProperty("interval")> _
    Public Overridable Property Interval() As IntervalElement
        Get
            Return Me.Item("interval")
        End Get
        Set(ByVal value As IntervalElement)
            Me.Item("interval") = value
        End Set
    End Property

    ''' <summary>
    ''' Gets a collection of time elements for use by a schedule.
    ''' </summary>
    ''' <value></value>
    ''' <returns>TimeElementCollection</returns>
    ''' <remarks></remarks>
    <ConfigurationProperty("daily")> _
    Public Overridable ReadOnly Property Daily() As TimeElementCollection
        Get
            Return Me.Item("daily")
        End Get
    End Property

    ''' <summary>
    ''' Gets a collection of schedule exceptions for use by a schedule.
    ''' </summary>
    ''' <value></value>
    ''' <returns>ExceptionElementCollection</returns>
    ''' <remarks></remarks>
    <ConfigurationProperty("exceptions")> _
    Public Overridable ReadOnly Property Exceptions() As ExceptionElementCollection
        Get
            Return Me.Item("exceptions")
        End Get
    End Property

    ''' <summary>
    ''' Gets the settings collection for the current schedule.
    ''' </summary>
    ''' <value></value>
    ''' <returns>NameValueConfigurationCollection</returns>
    ''' <remarks></remarks>
    <ConfigurationProperty("settings")> _
    Public Overridable ReadOnly Property Settings() As NameValueConfigurationCollection
        Get
            Return Me.Item("settings")
        End Get
    End Property

    ''' <summary>
    ''' Creates an instance of the currently configured schedule.
    ''' </summary>
    ''' <returns>IMonitorSchedule</returns>
    ''' <remarks></remarks>
    Public Overridable Function CreateInstance() As IMonitorSchedule
        Dim schedule As IMonitorSchedule = SiphonConfigurationSection.CreateInstance(Me.Type)
        schedule.Initialize(Me)

        Return schedule
    End Function

    ''' <summary>
    ''' Determins if the config is read only.
    ''' </summary>
    ''' <returns>Boolean. Always returns False.</returns>
    ''' <remarks></remarks>
    Public Overrides Function IsReadOnly() As Boolean
        Return False
    End Function
End Class
