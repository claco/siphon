Imports System.Configuration

''' <summary>
''' A time of day element.
''' </summary>
''' <remarks></remarks>
Public Class TimeElement
    Inherits ConfigurationElement

    ''' <summary>
    ''' Gets the value of the current time of day element.
    ''' </summary>
    ''' <value></value>
    ''' <returns>TimeSpan</returns>
    ''' <remarks></remarks>
    <ConfigurationProperty("value", DefaultValue:="00:00:00", IsRequired:=True, IsKey:=True)> _
    <TimeSpanValidator(MinValueString:="00:00:00", MaxValueString:="23:59:59")> _
    Public Overridable ReadOnly Property Value() As TimeSpan
        Get
            Return Me.Item("value")
        End Get
    End Property
End Class
