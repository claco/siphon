Imports System.Configuration

Namespace Configuration
    ''' <summary>
    ''' An interval of time to wait.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class IntervalElement
        Inherits ConfigurationElement

        ''' <summary>
        ''' Gets the value of the interval.
        ''' </summary>
        ''' <value></value>
        ''' <returns>TimeSpan</returns>
        ''' <remarks></remarks>
        <ConfigurationProperty("value", DefaultValue:="00:01:00", IsRequired:=True, IsKey:=True)> _
        <TimeSpanValidator(MinValueString:="0")> _
        Public Overridable ReadOnly Property Value() As TimeSpan
            Get
                Return Me.Item("value")
            End Get
        End Property
    End Class
End Namespace

