Imports System.Configuration

Namespace Configuration
    ''' <summary>
    ''' The configuration for each monitor.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class MonitorElement
        Inherits ConfigurationElement

        ''' <summary>
        ''' Gets the name of the current monitor.
        ''' </summary>
        ''' <value></value>
        ''' <returns>String</returns>
        ''' <remarks></remarks>
        <ConfigurationProperty("name", IsKey:=True, IsRequired:=True)> _
        Public ReadOnly Property Name() As String
            Get
                Return Me.Item("name")
            End Get
        End Property

        ''' <summary>
        ''' Gets the type of the monitor to load.
        ''' </summary>
        ''' <value></value>
        ''' <returns>String</returns>
        ''' <remarks></remarks>
        <ConfigurationProperty("type", IsRequired:=True)> _
        Public ReadOnly Property Type() As String
            Get
                Return Me.Item("type")
            End Get
        End Property

        ''' <summary>
        ''' Gets the schedule to use for the current monitor.
        ''' </summary>
        ''' <value></value>
        ''' <returns>ScheduleElement</returns>
        ''' <remarks></remarks>
        <ConfigurationProperty("schedule", IsRequired:=True)> _
        Public ReadOnly Property Schedule() As ScheduleElement
            Get
                Return Me.Item("schedule")
            End Get
        End Property
    End Class
End Namespace
