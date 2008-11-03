Imports System.Configuration

Namespace Configuration
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
        Default Public Overloads ReadOnly Property Item(ByVal i As Integer) As TimeElement
            Get
                Return DirectCast(Me.BaseGet(i), TimeElement)
            End Get
        End Property

        ''' <summary>
        ''' Returns a monitor element by name.
        ''' </summary>
        ''' <param name="name">String. The name of the monitor element to return.</param>
        ''' <value></value>
        ''' <returns>MonitorElement</returns>
        ''' <remarks></remarks>
        Default Public Overloads ReadOnly Property Item(ByVal name As String) As TimeElement
            Get
                Return DirectCast(Me.BaseGet(name), TimeElement)
            End Get
        End Property

        Protected Overloads Overrides Function CreateNewElement() As System.Configuration.ConfigurationElement
            Return New TimeElement
        End Function

        Protected Overrides Function GetElementKey(ByVal element As System.Configuration.ConfigurationElement) As Object
            Return DirectCast(element, TimeElement).Value
        End Function
    End Class
End Namespace