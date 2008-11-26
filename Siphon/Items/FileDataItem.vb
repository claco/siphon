Imports System.IO

''' <summary>
''' Class to reference and read files for processing.
''' </summary>
''' <remarks></remarks>
Public Class FileDataItem
    Inherits DataItem(Of FileInfo)

    ''' <summary>
    ''' Creates a new FileDataItem instance.
    ''' </summary>
    ''' <param name="path">String. The full path including file name to the file being processed.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal path As String)
        MyBase.New(path, New FileInfo(path))
    End Sub

    ''' <summary>
    ''' Gets the contents of the file for processing.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Object</returns>
    ''' <remarks></remarks>
    Public Overrides ReadOnly Property GetString() As String
        Get
            Using reader As StreamReader = DirectCast(Me.Item, FileInfo).OpenText
                GetString = reader.ReadToEnd
            End Using
        End Get
    End Property
End Class
