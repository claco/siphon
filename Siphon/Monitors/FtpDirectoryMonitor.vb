Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Net
Imports log4net

Public Class FtpDirectoryMonitor
    Inherits RemoteDirectoryMonitor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    ''' <summary>
    ''' Protected constructor for reflection.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new directory monitor.
    ''' </summary>
    ''' <param name="name">String. The friendly name for the monitor.</param>
    ''' <param name="path">String. The full path to the directory to be monitored.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to monitor the directory.</param>
    ''' <param name="processor">IDataProcessor. The data processor to use to process new files.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, ByVal path As String, ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        MyBase.New(name, path, schedule, processor)
    End Sub

    ''' <summary>
    ''' Create any missing folders during start.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub CreateFolders()
        MyBase.CreateFolders()

        If Me.CreateMissingFolders Then
            Log.DebugFormat("Creating directory {0}", Me.Uri)

            Try
                Dim request As FtpWebRequest = FtpWebRequest.Create(Me.Uri)
                request.Method = WebRequestMethods.Ftp.MakeDirectory
                request.UsePassive = True
                request.KeepAlive = False
                If Me.Credentials IsNot Nothing Then
                    request.Credentials = Me.Credentials
                End If

                Dim response As FtpWebResponse = request.GetResponse
            Catch ex As Exception
                Log.Error(String.Format("Error creating {0}", Me.Uri), ex)
            End Try

            If Me.CompleteUri IsNot Nothing Then
                Log.DebugFormat("Creating directory {0}", Me.CompleteUri)

                Try
                    Dim request As FtpWebRequest = FtpWebRequest.Create(Me.CompleteUri)
                    request.Method = WebRequestMethods.Ftp.MakeDirectory
                    request.UsePassive = True
                    request.KeepAlive = False
                    If Me.Credentials IsNot Nothing Then
                        request.Credentials = Me.Credentials
                    End If

                    Dim response As FtpWebResponse = request.GetResponse
                Catch ex As Exception
                    Log.Error(String.Format("Error creating {0}", Me.CompleteUri), ex)
                End Try
            End If

            If Me.FailureUri IsNot Nothing Then
                Log.DebugFormat("Creating directory {0}", Me.FailureUri)

                Try
                    Dim request As FtpWebRequest = FtpWebRequest.Create(Me.FailureUri)
                    request.Method = WebRequestMethods.Ftp.MakeDirectory
                    request.UsePassive = True
                    request.KeepAlive = False
                    If Me.Credentials IsNot Nothing Then
                        request.Credentials = Me.Credentials
                    End If

                    Dim response As FtpWebResponse = request.GetResponse
                Catch ex As Exception
                    Log.Error(String.Format("Error creating {0}", Me.FailureUri), ex)
                End Try
            End If
        End If
    End Sub

    ''' <summary>
    ''' Scans the specified directory for new files mathcing the specified filter.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Function Scan() As Collection(Of IDataItem)
        Log.DebugFormat("Scanning {0} for {1}", Me.Uri, Me.Filter)

        Dim items As New System.Collections.ObjectModel.Collection(Of IDataItem)

        Try
            Dim uri As New Uri(IO.Path.Combine(Me.Uri.AbsoluteUri, Me.Filter))
            Dim request As FtpWebRequest = FtpWebRequest.Create(Uri)
            request.Method = WebRequestMethods.Ftp.ListDirectory
            request.UsePassive = True
            request.KeepAlive = False
            If Me.Credentials IsNot Nothing Then
                request.Credentials = Me.Credentials
            End If

            Dim response As FtpWebResponse = request.GetResponse
            Using reader As New StreamReader(response.GetResponseStream)
                Dim fileName As String = reader.ReadLine.Trim
                Dim fileUri As Uri = New Uri(IO.Path.Combine(Me.Uri.AbsoluteUri, fileName))

                Log.DebugFormat("Found remote file {0}", fileUri.AbsoluteUri)

                items.Add(New UriDataItem(fileUri))
            End Using
            response.Close()
        Catch ex As Exception
            Log.Error(String.Format("Error scanning {0}", Me.Uri), ex)
        End Try

        Return items
    End Function

    ''' <summary>
    ''' Downloads the remote file to the hard drive for processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to prepare.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Prepare(ByVal item As IDataItem)
        Dim uriItem As UriDataItem = item
        Dim remoteFile As New FileInfo(uriItem.Data.LocalPath)
        Dim tempFile As String = IO.Path.Combine(Me.DownloadPath, remoteFile.Name)

        Log.DebugFormat("Downloading {0} to {1}", item.Name, tempFile)

        Try
            Dim request As FtpWebRequest = FtpWebRequest.Create(uriItem.Data)
            request.Method = WebRequestMethods.Ftp.DownloadFile
            request.UsePassive = True
            request.KeepAlive = False
            request.UseBinary = True
            If Me.Credentials IsNot Nothing Then
                request.Credentials = Me.Credentials
            End If

            Dim response As FtpWebResponse = request.GetResponse
            Using stream As IO.Stream = response.GetResponseStream
                Using fs As New IO.FileStream(tempFile, IO.FileMode.Create)
                    Dim buffer(2047) As Byte
                    Dim read As Integer = 0
                    Do
                        read = stream.Read(buffer, 0, buffer.Length)
                        fs.Write(buffer, 0, read)
                    Loop Until read = 0

                    stream.Close()
                    fs.Flush()
                    fs.Close()
                End Using
                stream.Close()
            End Using
            response.Close()

            uriItem.LocalFile = New FileInfo(tempFile)
            uriItem.Name += " (" & tempFile & ")"
        Catch ex As Exception
            Log.Error(String.Format("Error downloading {0}", uriItem.Data), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Deletes the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to delete.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Delete(ByVal item As IDataItem)
        Dim uriItem As UriDataItem = item

        Log.DebugFormat("Deleting {0}", uriItem.Data)

        Try
            Dim request As FtpWebRequest = FtpWebRequest.Create(uriItem.Data)
            request.Method = WebRequestMethods.Ftp.DeleteFile
            request.UsePassive = True
            request.KeepAlive = False
            If Me.Credentials IsNot Nothing Then
                request.Credentials = Me.Credentials
            End If

            Dim response As FtpWebResponse = request.GetResponse
        Catch ex As Exception
            Log.Error(String.Format("Error deleting {0}", uriItem.Data), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Moves the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to move.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Move(ByVal item As IDataItem)
        Dim uriItem As UriDataItem = item
        Dim remoteFile As New FileInfo(uriItem.Data.LocalPath)
        Dim newFile As Uri

        If uriItem.Status = DataItemStatus.CompletedProcessing Then
            newFile = New Uri(IO.Path.Combine(Me.CompleteUri.AbsoluteUri, remoteFile.Name))
        ElseIf uriItem.Status = DataItemStatus.FailedProcessing Then
            newFile = New Uri(IO.Path.Combine(Me.FailureUri.AbsoluteUri, remoteFile.Name))
        Else
            Throw New NotImplementedException("Unknown Item Status")
        End If

        Log.DebugFormat("Moving {0} to {1}", uriItem.Data, newFile)

        Try
            Dim request As FtpWebRequest = FtpWebRequest.Create(uriItem.Data)
            request.Method = WebRequestMethods.Ftp.Rename
            request.UsePassive = True
            request.KeepAlive = False
            request.RenameTo = newFile.AbsolutePath
            If Me.Credentials IsNot Nothing Then
                request.Credentials = Me.Credentials
            End If

            Dim response As FtpWebResponse = request.GetResponse
        Catch ex As Exception
            Log.Error(String.Format("Error moving {0}", uriItem.Data), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Renames the data item after processing.
    ''' </summary>
    ''' <param name="item">IDataItem. The item to renamed.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Rename(ByVal item As IDataItem)
        Dim uriItem As UriDataItem = item
        Dim remoteFile As New FileInfo(uriItem.Data.LocalPath)
        Dim newFile As String = Me.GetNewFileName(remoteFile.Name)
        Dim newUri As New UriBuilder(uriItem.Data)
        newUri.Path = newUri.Path.Replace(remoteFile.Name, newFile)

        Log.DebugFormat("Renaming {0} to {1}", uriItem.Data, newUri.Uri)

        Try
            Dim request As FtpWebRequest = FtpWebRequest.Create(uriItem.Data)
            request.Method = WebRequestMethods.Ftp.Rename
            request.UsePassive = True
            request.KeepAlive = False
            request.RenameTo = newFile
            If Me.Credentials IsNot Nothing Then
                request.Credentials = Me.Credentials
            End If

            Dim response As FtpWebResponse = request.GetResponse

            uriItem.Data = newUri.Uri
        Catch ex As Exception
            Log.Error(String.Format("Error renaming {0}", uriItem.Data), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Returns value indicating if the given uri scheme is supported or not.
    ''' </summary>
    ''' <param name="uri">Uri. The uri to validate.</param>
    ''' <returns>Boolean. True of the uri scheme is supported. False otherwise.</returns>
    ''' <remarks>Currently, only ftp is supported.</remarks>
    Protected Overrides Function IsSchemeSupported(ByVal uri As Uri) As Boolean
        Select Case uri.Scheme
            Case uri.UriSchemeFtp
                Return True
            Case Else
                Return False
        End Select
    End Function
End Class
