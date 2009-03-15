Imports System.Configuration
Imports System.Reflection
Imports log4net

''' <summary>
''' Main Siphon configuration section information
''' </summary>
''' <remarks></remarks>
Public Class SiphonConfigurationSection
    Inherits System.Configuration.ConfigurationSection

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Private Const SECTION_NAME As String = "siphon"

    ''' <summary>
    ''' Returnz the Siphon configuration section from the current config.
    ''' </summary>
    ''' <returns>ConfigurationSection</returns>
    ''' <remarks></remarks>
    Public Shared Function GetSection() As SiphonConfigurationSection
        Return SiphonConfigurationSection.GetSection(SECTION_NAME)
    End Function

    ''' <summary>
    ''' Returns the Siphon configuration section from the current config using the given name.
    ''' </summary>
    ''' <param name="name">String. The section tag name.</param>
    ''' <returns>ConfigurationSection</returns>
    ''' <remarks></remarks>
    Public Shared Function GetSection(ByVal name As String) As SiphonConfigurationSection
        Dim section As SiphonConfigurationSection = ConfigurationManager.GetSection(name)
        If section Is Nothing Then
            section = New SiphonConfigurationSection
        End If

        Return section
    End Function

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
    ''' Gets/sets if remote administration should be enabled.
    ''' </summary>ss
    ''' <value></value>
    ''' <returns>Boolean</returns>
    ''' <remarks>If True, the remote administration will be started. Otherwise it will be disabled.</remarks>
    <ConfigurationProperty("enableRemoteAdministration", DefaultValue:=False)> _
    Public Overridable Property EnableRemoteAdministration() As Boolean
        Get
            Return Me.Item("enableRemoteAdministration")
        End Get
        Set(ByVal value As Boolean)
            Me.Item("enableRemoteAdministration") = value
        End Set
    End Property

    ''' <summary>
    ''' Returns a instance of the specified type from the given assembly.
    ''' </summary>
    ''' <param name="type">String. The type string, including the assembly name/path to create.</param>
    ''' <returns>Object</returns>
    ''' <remarks></remarks>
    Friend Shared Function CreateInstance(ByVal type As String) As Object
        Log.DebugFormat("Loading type {0}", type)

        Dim separators() As String = {","}
        Dim types() As String = type.Split(separators, 1)
        Dim assembly As Assembly = assembly.Load(types(1).Trim)

        Return assembly.CreateInstance(types(0).Trim, False, BindingFlags.CreateInstance Or BindingFlags.Instance Or BindingFlags.FlattenHierarchy Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
    End Function
End Class
