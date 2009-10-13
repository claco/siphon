Imports System.Configuration

''' <summary>
''' A collection of schedule exclusion configuration elements
''' </summary>
''' <remarks></remarks>
<ConfigurationCollection(GetType(ExclusionElement), AddItemName:="exclude", CollectionType:=ConfigurationElementCollectionType.AddRemoveClearMap)> _
Public Class ExclusionElementCollection
    Inherits ConfigurationElementCollection

    ''' <summary>
    ''' Adds a new schedule exclusion element.
    ''' </summary>
    ''' <param name="from">TimeSpan. The start of the schedule exclusion.</param>
    ''' <param name="to">TimeSpan. The end of the schedule exclusion.</param>
    ''' <remarks></remarks>
    Public Sub Add(ByVal from As TimeSpan, ByVal [to] As TimeSpan)
        Me.BaseAdd(New ExclusionElement(from, [to]))
    End Sub

    ''' <summary>
    ''' Adds a new schedule exclusion element.
    ''' </summary>
    ''' <param name="from">DateTime. The start of the schedule exclusion.</param>
    ''' <param name="to">DateTime. The end of the schedule exclusion.</param>
    ''' <remarks></remarks>
    Public Sub Add(ByVal from As DateTime, ByVal [to] As DateTime)
        Me.BaseAdd(New ExclusionElement(from, [to]))
    End Sub

    ''' <summary>
    ''' Adds a new schedule exclusion element.
    ''' </summary>
    ''' <param name="from">String. The start of the schedule exclusion.</param>
    ''' <param name="to">String. The end of the schedule exclusion.</param>
    ''' <remarks></remarks>
    Public Sub Add(ByVal from As String, ByVal [to] As String)
        Me.BaseAdd(New ExclusionElement(from, [to]))
    End Sub

    ''' <summary>
    ''' Adds a new schedule exclusion element.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Add(ByVal exception As ExclusionElement)
        Me.BaseAdd(exception)
    End Sub

    ''' <summary>
    ''' Returns an exclusion element by index.
    ''' </summary>
    ''' <param name="i">Integer. The position of the exclusion element to return.</param>
    ''' <value></value>
    ''' <returns>ExclusionElement</returns>
    ''' <remarks></remarks>
    Default Public Overridable Overloads ReadOnly Property Item(ByVal i As Integer) As ExclusionElement
        Get
            Return DirectCast(Me.BaseGet(i), ExclusionElement)
        End Get
    End Property

    ''' <summary>
    ''' Creates a new exclusion element.
    ''' </summary>
    ''' <returns>ExclusionElement</returns>
    ''' <remarks></remarks>
    Protected Overloads Overrides Function CreateNewElement() As System.Configuration.ConfigurationElement
        Return New ExclusionElement
    End Function

    ''' <summary>
    ''' Gets the key for a schedule element.
    ''' </summary>
    ''' <param name="element">ExclusionElement. The exclusion element to get the key for.</param>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Protected Overrides Function GetElementKey(ByVal element As System.Configuration.ConfigurationElement) As Object
        Dim exception As ExclusionElement = element

        Return String.Format("{0}-{1}", exception.From, exception.To)
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
