Imports System.Configuration

''' <summary>
''' A collection of monitor configuration elements
''' </summary>
''' <remarks></remarks>
<ConfigurationCollection(GetType(TimeElement), AddItemName:="time", CollectionType:=ConfigurationElementCollectionType.AddRemoveClearMap)> _
Public Class TimeElementCollection
    Inherits ConfigurationElementCollection

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
End Class
