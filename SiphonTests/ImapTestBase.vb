Imports System.Configuration
Imports System.IO
Imports System.Net
Imports NUnit.Framework
Imports log4net
Imports LumiSoft.Net.IMAP.Server
Imports ChrisLaco.Siphon

Public Class ImapTestBase
    Inherits TestBase

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Private _server As IMAP_Server

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

    Protected Overridable Sub CopyMessage(ByVal sender As Object, ByVal e As Message_EventArgs)
        Dim files() As FileInfo = TestDirectory.GetFiles
        Dim name As String = files(e.Message.UID - 1).Name
        Dim destination As String = Path.Combine(TestDirectory.Parent.FullName, e.CopyLocation)

        files(e.Message.UID - 1).CopyTo(Path.Combine(destination, name))
    End Sub

    Protected Overrides Sub CreateTestDirectory()
        TestDirectory = System.IO.Directory.CreateDirectory(Path.Combine(Path.Combine(Path.GetTempPath, Path.GetRandomFileName), "INBOX"))
    End Sub

    Protected Overridable Sub CreateFolder(ByVal sender As Object, ByVal e As Mailbox_EventArgs)
        TestDirectory.Parent.CreateSubdirectory(e.Folder)
    End Sub

    Protected Overrides ReadOnly Property Credentials() As System.Net.NetworkCredential
        Get
            Dim userName As String = ConfigurationManager.AppSettings("ImapUserName")
            Dim password As String = ConfigurationManager.AppSettings("ImapPassword")

            Return New NetworkCredential(userName, password)
        End Get
    End Property

    Protected Overridable Sub DeleteMessage(ByVal sender As Object, ByVal e As Message_EventArgs)
        Dim files() As FileInfo = TestDirectory.GetFiles

        files(e.Message.UID - 1).Delete()
    End Sub

    Protected Overridable Sub GetFolders(ByVal sender As Object, ByVal e As IMAP_Folders)
        For Each directory As DirectoryInfo In TestDirectory.Parent.GetDirectories("*", SearchOption.AllDirectories)
            e.Add(directory.FullName.Replace(TestDirectory.Parent.FullName.ToString, "").Substring(1), True)
        Next
    End Sub

    Protected Overridable Sub GetMessagesInfo(ByVal sender As Object, ByVal e As IMAP_eArgs_GetMessagesInfo)
        Dim files() As FileInfo = TestDirectory.GetFiles

        For i As Integer = 1 To files.Length
            Dim file As FileInfo = files(i - 1)

            e.FolderInfo.Messages.Add(file.Name, i, file.LastWriteTimeUtc, file.Length, LumiSoft.Net.IMAP.IMAP_MessageFlags.None)
        Next
    End Sub

    Protected Overridable Sub GetMessageItems(ByVal sender As Object, ByVal e As IMAP_eArgs_MessageItems)
        Dim files() As FileInfo = TestDirectory.GetFiles

        e.MessageStream = files(e.MessageInfo.UID - 1).OpenRead
    End Sub

    Protected Overridable ReadOnly Property Server() As IMAP_Server
        Get
            If _server Is Nothing Then
                _server = New IMAP_Server
                AddHandler _server.AuthUser, AddressOf Authenticate
                AddHandler _server.GetFolders, AddressOf GetFolders
                AddHandler _server.GetMessagesInfo, AddressOf GetMessagesInfo
                AddHandler _server.GetMessageItems, AddressOf GetMessageItems
                AddHandler _server.DeleteMessage, AddressOf DeleteMessage
                AddHandler _server.CreateFolder, AddressOf CreateFolder
                AddHandler _server.CopyMessage, AddressOf CopyMessage
            End If

            Return _server
        End Get
    End Property

    Protected Overrides ReadOnly Property Uri() As System.Uri
        Get
            Return New Uri(ConfigurationManager.AppSettings("ImapUri"))
        End Get
    End Property
End Class
