Imports System.Configuration

    ''' <summary>
    ''' An interval of time to wait.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class IntervalElement
        Inherits ConfigurationElement

    ''' <summary>
    ''' Creates a new interval element.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new interval element.
    ''' </summary>
    ''' <param name="value">TimeSpan. The value of the new element.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal value As TimeSpan)
        Me.Value = value
    End Sub

    ''' <summary>
    ''' Gets/set the value of the interval.
    ''' </summary>
    ''' <value></value>
    ''' <returns>TimeSpan</returns>
    ''' <remarks></remarks>
    <ConfigurationProperty("value", DefaultValue:="00:01:00", IsRequired:=True, IsKey:=True)> _
    <TimeSpanValidator(MinValueString:="0")> _
    Public Overridable Property Value() As TimeSpan
        Get
            Return Me.Item("value")
        End Get
        Set(ByVal value As TimeSpan)
            Me.Item("value") = value
        End Set
    End Property

    ''' <summary>
    ''' Determins if the config is read only.
    ''' </summary>
    ''' <returns>Boolean. Always returns False.</returns>
    ''' <remarks></remarks>
    Public Overrides Function IsReadOnly() As Boolean
        Return False
    End Function
End Class
