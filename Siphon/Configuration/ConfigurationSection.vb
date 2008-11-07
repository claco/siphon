Imports System.Configuration
Imports System.Reflection

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
        Public Overridable ReadOnly Property Monitors() As MonitorElementCollection
            Get
                Return Me.Item("monitors")
            End Get
        End Property

        ''' <summary>
        ''' Returns a instance of the specified type from the given assembly.
        ''' </summary>
        ''' <param name="type">String. The type string, including the assembly name/path to create.</param>
        ''' <returns>Object</returns>
        ''' <remarks></remarks>
        Friend Shared Function CreateInstance(ByVal type As String) As Object
            Dim separators() As String = {","}
            Dim types() As String = type.Split(separators, 1)
            Dim assembly As Assembly = assembly.Load(types(1).Trim)

            Return assembly.CreateInstance(types(0).Trim, False, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
        End Function

    End Class
End Namespace