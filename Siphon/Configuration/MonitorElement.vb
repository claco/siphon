Imports System.Configuration

''' <summary>
''' The configuration for each monitor.
''' </summary>
''' <remarks></remarks>
Public Class MonitorElement
    Inherits ConfigurationElement

    ''' <summary>
    ''' Creates a new monitor element.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new monitor element.
    ''' </summary>
    ''' <param name="name">String. The name of the new monitor.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String)
        Me.Name = name
    End Sub

    ''' <summary>
    ''' Creates a new monitor element.
    ''' </summary>
    ''' <param name="name">String. The name of the new monitor.</param>
    ''' <param name="type">String. The type of the new monitor.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, ByVal type As String)
        Me.Name = name
        Me.Type = type
    End Sub

    ''' <summary>
    ''' Gets/sets the name of the current monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    <ConfigurationProperty("name", IsKey:=True, IsRequired:=True)> _
    Public Overridable Property Name() As String
        Get
            Return Me.Item("name")
        End Get
        Set(ByVal value As String)
            Me.Item("name") = value.Trim
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the type of the monitor to load.
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
    ''' Gets/sets the schedule to use for the current monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>ScheduleElement</returns>
    ''' <remarks></remarks>
    <ConfigurationProperty("schedule", IsRequired:=True)> _
    Public Overridable Property Schedule() As ScheduleElement
        Get
            Return Me.Item("schedule")
        End Get
        Set(ByVal value As ScheduleElement)
            Me.Item("schedule") = value
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the processor to use for the current monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>ProcessorElement</returns>
    ''' <remarks></remarks>
    <ConfigurationProperty("processor", IsRequired:=True)> _
    Public Overridable Property Processor() As ProcessorElement
        Get
            Return Me.Item("processor")
        End Get
        Set(ByVal value As ProcessorElement)
            Me.Item("processor") = value
        End Set
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

    ''' <summary>
    ''' Determins if the config is read only.
    ''' </summary>
    ''' <returns>Boolean. Always returns False.</returns>
    ''' <remarks></remarks>
    Public Overrides Function IsReadOnly() As Boolean
        Return False
    End Function
End Class
