Imports System.Messaging

''' <summary>
''' Class to reference and read queue messages for processing.
''' </summary>
''' <remarks></remarks>
Public Class QueueMessageDataItem
    Inherits DataItem(Of Message)

    ''' <summary>
    ''' Creates a new QueueMessageDataItem instance.
    ''' </summary>
    ''' <param name="message">Message. The queue message being processed.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal message As Message)
        MyBase.New(message.Label, message)
    End Sub

    ''' <summary>
    ''' Gets the contents of the queue message for processing.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Object</returns>
    ''' <remarks></remarks>
    Public Overrides ReadOnly Property GetString() As String
        Get
            Return Me.Item.Body
        End Get
    End Property
End Class
