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
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overrides Function GetString() As String
        Using reader As StreamReader = DirectCast(Me.Data, FileInfo).OpenText
            GetString = reader.ReadToEnd
        End Using
    End Function

    ''' <summary>
    ''' Moves the current file and sets data to the new file location.
    ''' </summary>
    ''' <param name="path">String. The full path of the directory the file should be moved into.</param>
    ''' <remarks></remarks>
    Public Sub Move(ByVal path As String)
        Dim moved As String = IO.Path.Combine(path, Me.Data.Name)
        Me.Data.MoveTo(moved)
        Me.Data = New FileInfo(moved)
    End Sub

    ''' <summary>
    ''' Renames the current file and sets data to the new file info.
    ''' </summary>
    ''' <param name="name">String. The name that the file should be renamed to.</param>
    ''' <remarks></remarks>
    Public Sub Rename(ByVal name As String)
        Dim renamed As String = IO.Path.Combine(Me.Data.Directory.FullName, name)
        Me.Data.MoveTo(renamed)
        Me.Data = New FileInfo(renamed)
    End Sub
End Class
