Imports System.Configuration
Imports System.ComponentModel
Imports System.Reflection

''' <summary>
''' The configuraton for a processor.
''' </summary>
''' <remarks></remarks>
Public Class ProcessorElement
    Inherits ConfigurationElement

    ''' <summary>
    ''' Creates a new processor element.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new processor element.
    ''' </summary>
    ''' <param name="type">String. The type of the new monitor.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal type As String)
        Me.Type = type
    End Sub

    ''' <summary>
    ''' Gets/sets the type of the processor to load.
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
    ''' Gets the settings collection for the current processor.
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
    ''' Creates an instance of the currently configured processor.
    ''' </summary>
    ''' <returns>IDataProcessor</returns>
    ''' <remarks></remarks>
    Public Overridable Function CreateInstance() As IDataProcessor
        Dim processor As IDataProcessor = SiphonConfigurationSection.CreateInstance(Me.Type)
        processor.Initialize(Me)

        Return processor
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
