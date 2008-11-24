Imports System.IO

''' <summary>
''' Class to reference and read files for processing.
''' </summary>
''' <remarks></remarks>
Public Class FileDataItem
    Inherits DataItem

    ''' <summary>
    ''' Creates a new FileDataItem instance.
    ''' </summary>
    ''' <param name="path">String. The full path including file name to the file being processed.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal path As String)
        MyBase.New(New FileInfo(path), String.Empty, path)
    End Sub

    ''' <summary>
    ''' Gets the contents of the file for processing.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Object</returns>
    ''' <remarks></remarks>
    Public Overrides ReadOnly Property Contents() As Object
        Get
            Using reader As StreamReader = DirectCast(Me.Item, FileInfo).OpenText
                Contents = reader.ReadToEnd
            End Using
        End Get
    End Property

    ''' <summary>
    ''' Gets the file info for the current item being processed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>FileInfo</returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property File() As FileInfo
        Get
            Return Me.Item
        End Get
    End Property
End Class
