Imports System.Messaging

''' <summary>
''' Class to reference and read queue messages for processing.
''' </summary>
''' <remarks></remarks>
Public Class QueueMessageDataItem
    Inherits DataItem

    ''' <summary>
    ''' Creates a new QueueMessageDataItem instance.
    ''' </summary>
    ''' <param name="message">Message. The queue message being processed.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal message As Message)
        MyBase.New(message, String.Empty, message.Label)
    End Sub

    ''' <summary>
    ''' Gets the contents of the queue message for processing.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Object</returns>
    ''' <remarks></remarks>
    Public Overrides ReadOnly Property Contents() As Object
        Get
            Return DirectCast(Me.Item, Message).Body
        End Get
    End Property
End Class
