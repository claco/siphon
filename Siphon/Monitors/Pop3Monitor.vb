Imports System.Collections.ObjectModel
Imports System.IO
Imports LumiSoft.Net.POP3.Client
Imports log4net

Public Class Pop3Monitor
    Inherits RemoteDirectoryMonitor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private Const SCHEME_POP As String = "pop"
    Private Const SCHEME_POPS As String = "pops"
    Private Const PORT_POP As Integer = 110
    Private Const PORT_POPS As Integer = 995

    ''' <summary>
    ''' Protected constructor for reflection.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub New()

    End Sub

    ''' <summary>
    ''' Creates a new pop3 mailbox monitor.
    ''' </summary>
    ''' <param name="name">String. The friendly name for the monitor.</param>
    ''' <param name="path">String. The full path to the pop3 mailbox to be monitored.</param>
    ''' <param name="schedule">IMonitorSchedule. The schedule used to monitor the mailbox.</param>
    ''' <param name="processor">IDataProcessor. The data processor to use to process new files.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal name As String, ByVal path As String, ByVal schedule As IMonitorSchedule, ByVal processor As IDataProcessor)
        MyBase.New(name, path, schedule, processor)
    End Sub

    Public Overrides Sub Delete(ByVal item As IDataItem)
    End Sub

    Public Overrides Sub Move(ByVal item As IDataItem)

    End Sub

    Public Overrides Sub Rename(ByVal item As IDataItem)

    End Sub

    ''' <summary>
    ''' Scans the specified Pop3 mailbox for new email messages to process.
    ''' </summary>
    ''' <returns>Collection(Of MailDataItem)</returns>
    ''' <remarks></remarks>
    Public Overrides Function Scan() As Collection(Of IDataItem)
        Dim items As New Collection(Of IDataItem)

        Using client As New POP3_Client
            client.Connect(Me.Uri.Host, Me.Uri.Port, IIf(Me.Uri.Scheme = SCHEME_POPS, True, False))
            If client.IsConnected Then
                client.Authenticate(Me.Credentials.UserName, Me.Credentials.Password, True)

                If client.IsAuthenticated Then
                    Dim messages As POP3_ClientMessageCollection = client.Messages
                    Log.DebugFormat("Found {0} messages", messages.Count)

                    For Each item As POP3_ClientMessage In messages
                        If Not item.IsMarkedForDeletion Then
                            items.Add(New MailDataItem(Me.Uri, item.UID))
                        End If
                    Next
                End If
            End If
        End Using

        Return items
    End Function

    ''' <summary>
    ''' Prepares the data before if it processed.
    ''' </summary>
    ''' <param name="item">MailDataItem. The mail message to prepare.</param>
    ''' <remarks></remarks>
    Public Overrides Sub Prepare(ByVal item As IDataItem)
        Dim mailItem As MailDataItem = item
        Dim tempFile As String = IO.Path.Combine(Me.DownloadPath, String.Format("{0}.eml", mailItem.UID))

        Log.DebugFormat("Downloading {0} to {1}", item.Name, tempFile)

        Using client As New POP3_Client
            client.Connect(Me.Uri.Host, Me.Uri.Port, IIf(Me.Uri.Scheme = SCHEME_POPS, True, False))
            If client.IsConnected Then
                client.Authenticate(Me.Credentials.UserName, Me.Credentials.Password, True)

                If client.IsAuthenticated Then
                    Using stream As FileStream = File.Create(tempFile)
                        Dim message As POP3_ClientMessage = client.Messages(mailItem.UID)

                        message.MessageToStream(stream)
                        mailItem.LocalFile = New FileInfo(tempFile)
                    End Using
                End If
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Scrubs pop/pops uris to set default ports and folders
    ''' </summary>
    ''' <param name="value">Uri. The Uri, CompleteUri or FailureUri to prepare.</param>
    ''' <returns></returns>
    ''' <remarks>If no port is specified, it will be set to 143/993 for pop/pops. If no path is specified, /INDEX will be set as the default.</remarks>
    Protected Overrides Function PrepareUri(ByVal value As Uri) As Uri
        Dim builder As New UriBuilder(MyBase.PrepareUri(value))

        If builder.Port < 1 Then
            Select Case value.Scheme
                Case SCHEME_POPS
                    Log.DebugFormat("Adding default port {0}/{1}", value.Scheme, PORT_POPS)

                    builder.Port = PORT_POPS
                Case Else
                    Log.DebugFormat("Adding default port {0}/{1}", value.Scheme, PORT_POP)

                    builder.Port = PORT_POP
            End Select
        End If

        Return builder.Uri
    End Function

    ''' <summary>
    ''' Returns value indicating if the given uri scheme is supported or not.
    ''' </summary>
    ''' <param name="uri">Uri. The uri to validate.</param>
    ''' <returns>Boolean. True of the uri scheme is supported. False otherwise.</returns>
    ''' <remarks>Currently, only pop/pops is supported.</remarks>
    Protected Overrides Function IsSchemeSupported(ByVal uri As Uri) As Boolean
        Select Case uri.Scheme
            Case SCHEME_POP, SCHEME_POPS
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>
    ''' Validates the current monitors configuration for errors before processing/starting.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overrides Sub Validate()
        Log.Debug("Validating monitor configuration")

        MyBase.Validate()

        If Me.Credentials Is Nothing Then
            Throw New ApplicationException("Credentials required for this monitor")
        ElseIf (Me.ProcessCompleteActions And DataActions.Rename) <> DataActions.None Or (Me.ProcessFailureActions And DataActions.Rename) <> DataActions.None Then
            Throw New NotImplementedException("Rename is not supported for this monitor.")
        End If
    End Sub
End Class
