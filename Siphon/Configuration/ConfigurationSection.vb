Imports System.Configuration

Namespace Configuration
    ''' <summary>
    ''' Main Siphon configuration section information
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ConfigurationSection
        Inherits System.Configuration.ConfigurationSection

        ''' <summary>
        ''' Returns a collection of monitor elements.
        ''' </summary>
        ''' <value></value>
        ''' <returns>MonitorElementCollection</returns>
        ''' <remarks></remarks>
        <ConfigurationProperty("monitors", IsDefaultCollection:=True, IsRequired:=True)> _
        Public ReadOnly Property Monitors() As MonitorElementCollection
            Get
                Return Me.Item("monitors")
            End Get
        End Property
    End Class
End Namespace