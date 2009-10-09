Imports System.Configuration

''' <summary>
''' A collection of schedule exception configuration elements
''' </summary>
''' <remarks></remarks>
<ConfigurationCollection(GetType(ExceptionElement), AddItemName:="except", CollectionType:=ConfigurationElementCollectionType.AddRemoveClearMap)> _
Public Class ExceptionElementCollection
    Inherits ConfigurationElementCollection

    ''' <summary>
    ''' Adds a new schedule exception element.
    ''' </summary>
    ''' <param name="from">TimeSpan. The start of the schedule exception.</param>
    ''' <param name="to">TimeSpan. The end of the schedule exception.</param>
    ''' <remarks></remarks>
    Public Sub Add(ByVal from As TimeSpan, ByVal [to] As TimeSpan)
        Me.BaseAdd(New ExceptionElement(from, [to]))
    End Sub

    ''' <summary>
    ''' Adds a new schedule exception element.
    ''' </summary>
    ''' <param name="from">DateTime. The start of the schedule exception.</param>
    ''' <param name="to">DateTime. The end of the schedule exception.</param>
    ''' <remarks></remarks>
    Public Sub Add(ByVal from As DateTime, ByVal [to] As DateTime)
        Me.BaseAdd(New ExceptionElement(from, [to]))
    End Sub

    ''' <summary>
    ''' Adds a new schedule exception element.
    ''' </summary>
    ''' <param name="from">String. The start of the schedule exception.</param>
    ''' <param name="to">String. The end of the schedule exception.</param>
    ''' <remarks></remarks>
    Public Sub Add(ByVal from As String, ByVal [to] As String)
        Me.BaseAdd(New ExceptionElement(from, [to]))
    End Sub

    ''' <summary>
    ''' Adds a new schedule exception element.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Add(ByVal exception As ExceptionElement)
        Me.BaseAdd(exception)
    End Sub

    ''' <summary>
    ''' Returns an exception element by index.
    ''' </summary>
    ''' <param name="i">Integer. The position of the exception element to return.</param>
    ''' <value></value>
    ''' <returns>ExceptionElement</returns>
    ''' <remarks></remarks>
    Default Public Overridable Overloads ReadOnly Property Item(ByVal i As Integer) As ExceptionElement
        Get
            Return DirectCast(Me.BaseGet(i), ExceptionElement)
        End Get
    End Property

    ''' <summary>
    ''' Creates a new exception element.
    ''' </summary>
    ''' <returns>ExceptionElement</returns>
    ''' <remarks></remarks>
    Protected Overloads Overrides Function CreateNewElement() As System.Configuration.ConfigurationElement
        Return New ExceptionElement
    End Function

    ''' <summary>
    ''' Gets the key for a schedule element.
    ''' </summary>
    ''' <param name="element">ExceptionElement. The exception element to get the key for.</param>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Protected Overrides Function GetElementKey(ByVal element As System.Configuration.ConfigurationElement) As Object
        Dim exception As ExceptionElement = element

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
