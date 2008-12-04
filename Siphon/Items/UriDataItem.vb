Imports System.IO

''' <summary>
''' Class to reference remote uri-based files and read local file copies for processing.
''' </summary>
''' <remarks></remarks>
Public Class UriDataItem
    Inherits DataItem(Of Uri)

    Private _localFile As FileInfo = Nothing

    ''' <summary>
    ''' Creates a new UriDataItem instance.
    ''' </summary>
    ''' <param name="path">String. The full uri including file name to the file being processed.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal path As Uri)
        MyBase.New(path.AbsoluteUri, path)
    End Sub

    Public Property LocalFile() As FileInfo
        Get
            Return _localFile
        End Get
        Set(ByVal value As FileInfo)
            _localFile = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the contents of the file for processing.
    ''' </summary>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overrides Function GetString() As String
        Using reader As StreamReader = Me.LocalFile.OpenText
            GetString = reader.ReadToEnd
        End Using
    End Function
End Class
