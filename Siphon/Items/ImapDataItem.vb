Imports System.Text.RegularExpressions

''' <summary>
''' Class to reference remote uri-based imap maessages.
''' </summary>
''' <remarks></remarks>
Public Class ImapDataItem
    Inherits UriDataItem

    ''' <summary>
    ''' Creates a new ImapDataItem instance.
    ''' </summary>
    ''' <param name="path">String. The full uri including UID to the message being processed.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal path As Uri)
        MyBase.New(path)
    End Sub

    ''' <summary>
    ''' Gets the message UID from the items current uri.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property UID() As String
        Get
            Dim match As Match = Regex.Match(Me.Data.AbsoluteUri, ";UID=(?<uid>.*);?\/?", RegexOptions.IgnoreCase)

            Return match.Groups("uid").Value
        End Get
    End Property
End Class
