Imports System.Configuration
Imports System.IO
Imports System.Net
Imports NUnit.Framework
Imports log4net
Imports LumiSoft.Net.POP3.Server
Imports ChrisLaco.Siphon

Public Class Pop3TestBase
    Inherits TestBase

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Private _server As POP3_Server

    <SetUp()> _
    Protected Overrides Sub SetUp()
        MyBase.SetUp()

        Server.StartServer()
    End Sub

    <TearDown()> _
    Protected Overrides Sub TearDown()
        MyBase.TearDown()

        Server.StopServer()
    End Sub

    Protected Overridable Sub Authenticate(ByVal sender As Object, ByVal e As AuthUser_EventArgs)
        e.Validated = True
    End Sub

    Protected Overrides Sub CreateTestDirectory()
        TestDirectory = System.IO.Directory.CreateDirectory(Path.Combine(Path.Combine(Path.GetTempPath, Path.GetRandomFileName), "INBOX"))
    End Sub

    Protected Overrides ReadOnly Property Credentials() As System.Net.NetworkCredential
        Get
            Dim userName As String = ConfigurationManager.AppSettings("Pop3UserName")
            Dim password As String = ConfigurationManager.AppSettings("Pop3Password")

            Return New NetworkCredential(userName, password)
        End Get
    End Property

    Protected Overridable Sub GetMessagesList(ByVal sender As Object, ByVal e As GetMessagesInfo_EventArgs)
        Dim files() As FileInfo = TestDirectory.GetFiles

        For i As Integer = 1 To files.Length
            Dim file As FileInfo = files(i - 1)

            e.Messages.Add(file.Name, i, file.Length)
        Next
    End Sub

    Protected Overridable Sub GetMessageStream(ByVal sender As Object, ByVal e As POP3_eArgs_GetMessageStream)
        Dim files() As FileInfo = TestDirectory.GetFiles

        e.MessageStream = files(e.MessageInfo.UID - 1).OpenRead
    End Sub

    Protected Overridable ReadOnly Property Server() As POP3_Server
        Get
            If _server Is Nothing Then
                _server = New POP3_Server
                AddHandler _server.AuthUser, AddressOf Authenticate
                AddHandler _server.GetMessgesList, AddressOf GetMessagesList
                AddHandler _server.GetMessageStream, AddressOf GetMessageStream
            End If

            Return _server
        End Get
    End Property

    Protected Overrides ReadOnly Property Uri() As System.Uri
        Get
            Return New Uri(ConfigurationManager.AppSettings("Pop3Uri"))
        End Get
    End Property
End Class
