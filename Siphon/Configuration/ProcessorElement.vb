Imports System.Configuration
Imports System.ComponentModel
Imports System.Reflection
Imports ChrisLaco.Siphon.Processors

Namespace Configuration
    ''' <summary>
    ''' The configuraton for a processor.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ProcessorElement
        Inherits ConfigurationElement

        ''' <summary>
        ''' Gets the type of the processor to load.
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
            Dim processor As IDataProcessor = ConfigurationSection.CreateInstance(Me.Type)
            processor.Initialize(Me)

            Return processor
        End Function
    End Class
End Namespace
