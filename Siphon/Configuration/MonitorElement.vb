Imports System.Configuration
Imports ChrisLaco.Siphon.Monitors

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
        Public Overridable ReadOnly Property Name() As String
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
        Public Overridable ReadOnly Property Type() As String
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
        Public Overridable ReadOnly Property Schedule() As ScheduleElement
            Get
                Return Me.Item("schedule")
            End Get
        End Property

        ''' <summary>
        ''' Gets the processor to use for the current monitor.
        ''' </summary>
        ''' <value></value>
        ''' <returns>ProcessorElement</returns>
        ''' <remarks></remarks>
        <ConfigurationProperty("processor", IsRequired:=True)> _
        Public Overridable ReadOnly Property Processor() As ProcessorElement
            Get
                Return Me.Item("processor")
            End Get
        End Property

        ''' <summary>
        ''' Gets the settings collection for the current monitor.
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
        ''' Creates an instance of the currently configured monitor.
        ''' </summary>
        ''' <returns>IDataMonitor</returns>
        ''' <remarks></remarks>
        Public Overridable Function CreateInstance() As IDataMonitor
            Dim monitor As IDataMonitor = SiphonConfigurationSection.CreateInstance(Me.Type)
            monitor.Initialize(Me)

            Return monitor
        End Function
    End Class
End Namespace
