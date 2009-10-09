Imports System.Configuration

''' <summary>
''' A collection of monitor configuration elements
''' </summary>
''' <remarks></remarks>
<ConfigurationCollection(GetType(MonitorElement), AddItemName:="monitor", CollectionType:=ConfigurationElementCollectionType.AddRemoveClearMap)> _
Public Class MonitorElementCollection
    Inherits ConfigurationElementCollection

    ''' <summary>
    ''' Adds a new monitor element.
    ''' </summary>
    ''' <param name="monitor">MonitorElement. The new mlonitor being added.</param>
    ''' <remarks></remarks>
    Public Sub Add(ByVal monitor As MonitorElement)
        Me.BaseAdd(monitor)
    End Sub

    ''' <summary>
    ''' Gets a monitor element by index.
    ''' </summary>
    ''' <param name="i">Integer. The position of the monitor element to return.</param>
    ''' <value></value>
    ''' <returns>MonitorElement</returns>
    ''' <remarks></remarks>
    Default Public Overridable Overloads ReadOnly Property Item(ByVal i As Integer) As MonitorElement
        Get
            Return DirectCast(Me.BaseGet(i), MonitorElement)
        End Get
    End Property

    ''' <summary>
    ''' Gets a monitor element by name.
    ''' </summary>
    ''' <param name="name">String. The name of the monitor element to return.</param>
    ''' <value></value>
    ''' <returns>MonitorElement</returns>
    ''' <remarks></remarks>
    Default Public Overridable Overloads ReadOnly Property Item(ByVal name As String) As MonitorElement
        Get
            Return DirectCast(Me.BaseGet(name), MonitorElement)
        End Get
    End Property

    ''' <summary>
    ''' Creats a new monitor element.
    ''' </summary>
    ''' <returns>MonitorElement</returns>
    ''' <remarks></remarks>
    Protected Overloads Overrides Function CreateNewElement() As System.Configuration.ConfigurationElement
        Return New MonitorElement
    End Function

    ''' <summary>
    ''' Gets the key for the specified element.
    ''' </summary>
    ''' <param name="element">configurationElement. The element to get the key for.</param>
    ''' <returns>Object</returns>
    ''' <remarks></remarks>
    Protected Overrides Function GetElementKey(ByVal element As System.Configuration.ConfigurationElement) As Object
        Return DirectCast(element, MonitorElement).Name
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
