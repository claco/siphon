Imports System.Configuration

''' <summary>
''' A collection of monitor configuration elements
''' </summary>
''' <remarks></remarks>
<ConfigurationCollection(GetType(TimeElement), AddItemName:="time", CollectionType:=ConfigurationElementCollectionType.AddRemoveClearMap)> _
Public Class TimeElementCollection
    Inherits ConfigurationElementCollection

    ''' <summary>
    ''' Adds a new time element.
    ''' </summary>
    ''' <param name="time">TimeSpan. The new value for the time element.</param>
    ''' <remarks></remarks>
    Public Sub Add(ByVal time As TimeSpan)
        Me.BaseAdd(New TimeElement(time))
    End Sub
    ''' <summary>
    ''' Adds a new time element.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Add(ByVal time As TimeElement)
        Me.BaseAdd(time)
    End Sub

    ''' <summary>
    ''' Returns a monitor element by index.
    ''' </summary>
    ''' <param name="i">Integer. The position of the minitor element to return.</param>
    ''' <value></value>
    ''' <returns>MonitorElement</returns>
    ''' <remarks></remarks>
    Default Public Overridable Overloads ReadOnly Property Item(ByVal i As Integer) As TimeElement
        Get
            Return DirectCast(Me.BaseGet(i), TimeElement)
        End Get
    End Property

    ''' <summary>
    ''' Creates a new time element.
    ''' </summary>
    ''' <returns>TimeElement</returns>
    ''' <remarks></remarks>
    Protected Overloads Overrides Function CreateNewElement() As System.Configuration.ConfigurationElement
        Return New TimeElement
    End Function

    ''' <summary>
    ''' Gets he key for a time element.
    ''' </summary>
    ''' <param name="element">TimeElement. The time element to get the key for.</param>
    ''' <returns>TimeSpan</returns>
    ''' <remarks></remarks>
    Protected Overrides Function GetElementKey(ByVal element As System.Configuration.ConfigurationElement) As Object
        Return DirectCast(element, TimeElement).Value
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
