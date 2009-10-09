Imports System.Configuration
Imports System.ComponentModel
Imports System.Reflection

''' <summary>
''' The configuraton for a schedule exception element.
''' </summary>
''' <remarks></remarks>
Public Class ExceptionElement
    Inherits ConfigurationElement

    ''' <summary>
    ''' Creates a new schedule exception element.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new schedule exception element.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New(ByVal from As TimeSpan, ByVal [to] As TimeSpan)
        Me.From = from.ToString
        Me.To = [to].ToString
    End Sub

    ''' <summary>
    ''' Creates a new schedule exception element.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New(ByVal from As DateTime, ByVal [to] As DateTime)
        Me.From = from.ToString
        Me.To = [to].ToString
    End Sub

    ''' <summary>
    ''' Creates a new schedule exception element.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New(ByVal from As String, ByVal [to] As String)
        Me.From = from
        Me.To = [to]
    End Sub

    ''' <summary>
    ''' Gets/sets the start time of the schedule interval exception.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    <ConfigurationProperty("from")> _
    Public Overridable Property From() As String
        Get
            Return Me.Item("from")
        End Get
        Set(ByVal value As String)
            Me.Item("from") = value
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the end time of the schedule interval exception.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    <ConfigurationProperty("to")> _
    Public Overridable Property [To]() As String
        Get
            Return Me.Item("to")
        End Get
        Set(ByVal value As String)
            Me.Item("to") = value
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
