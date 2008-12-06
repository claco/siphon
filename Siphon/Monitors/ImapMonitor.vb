Imports LumiSoft.Net.IMAP.Client
Imports log4net

Public Class ImapMonitor
    Inherits RemoteDirectoryMonitor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    ''' <summary>
    ''' Protected constructor for reflection.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new imap folder monitor.
    ''' </summary>
    ''' <param name="name">String. The friendly name for the monitor.</param>
    ''' <param name="path">String. The full path to the imap folder to be monitored.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to monitor the directory.</param>
    ''' <param name="processor">IDataProcessor. The data processor to use to process new files.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, ByVal path As String, ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        MyBase.New(name, path, schedule, processor)
    End Sub

    Public Overrides Sub CreateFolders()

    End Sub

    Public Overrides Sub Delete(ByVal item As IDataItem)

    End Sub

    Public Overrides Sub Move(ByVal item As IDataItem)

    End Sub

    Public Overrides Sub Rename(ByVal item As IDataItem)

    End Sub

    Public Overrides Function Scan() As System.Collections.ObjectModel.Collection(Of IDataItem)
        Using client As New IMAP_Client
            client.Connect(Me.Uri.Host, Me.Uri.Port, True)

            If client.IsConnected Then
                client.Authenticate(Me.Credentials.UserName, Me.Credentials.Password)

                If client.IsAuthenticated Then
                    Debug.WriteLine(Me.Uri.PathAndQuery)
                    client.SelectFolder(Me.Uri.PathAndQuery)
                    Debug.WriteLine(client.GetUnseenMessagesCount)
                End If
            End If
        End Using
    End Function

    ''' <summary>
    ''' Returns value indicating if the given uri scheme is supported or not.
    ''' </summary>
    ''' <param name="uri">Uri. The uri to validate.</param>
    ''' <returns>Boolean. True of the uri scheme is supported. False otherwise.</returns>
    ''' <remarks>Currently, only imap is supported.</remarks>
    Protected Overrides Function IsSchemeSupported(ByVal uri As Uri) As Boolean
        Select Case uri.Scheme
            Case "imap"
                Return True
            Case Else
                Return False
        End Select
    End Function
End Class
