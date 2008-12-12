Imports System.Configuration
Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports NUnit.Framework
Imports ChrisLaco.Siphon
Imports LumiSoft.Net.IMAP.Server

<TestFixture(Description:="Imap Monitor Tests")> _
Public Class ImapMonitorTests
    Inherits TestBase

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

    Protected Overrides Sub CreateTestDirectory()
        TestDirectory = System.IO.Directory.CreateDirectory(Path.Combine(Path.Combine(Path.GetTempPath, Path.GetRandomFileName), "INBOX"))
    End Sub

    Protected Overridable ReadOnly Property ImapUri() As Uri
        Get
            Return New Uri(ConfigurationManager.AppSettings("ImapUri"))
        End Get
    End Property

    Protected Overridable ReadOnly Property ImapUser() As String
        Get
            Return ConfigurationManager.AppSettings("ImapUser")
        End Get
    End Property

    Protected Overridable ReadOnly Property ImapPass() As String
        Get
            Return ConfigurationManager.AppSettings("ImapPass")
        End Get
    End Property

    Protected Overridable ReadOnly Property Server() As IMAP_Server
        Get
            If _server Is Nothing Then
                _server = New IMAP_Server
                AddHandler _server.AuthUser, AddressOf Authenticate
                AddHandler _server.GetFolders, AddressOf GetFolders
                AddHandler _server.GetMessagesInfo, AddressOf GetMessagesInfo
                AddHandler _server.GetMessageItems, AddressOf GetMessageItems
            End If

            Return _server
        End Get
    End Property

    Protected Overridable Sub Authenticate(ByVal sender As Object, ByVal e As AuthUser_EventArgs)
        e.Validated = True
    End Sub

    Protected Overridable Sub GetFolders(ByVal sender As Object, ByVal e As IMAP_Folders)
        e.Add("Foo", True)
    End Sub

    Protected Overridable Sub GetMessagesInfo(ByVal sender As Object, ByVal e As IMAP_eArgs_GetMessagesInfo)
        For Each file As FileInfo In TestDirectory.GetFiles
            e.FolderInfo.Messages.Add(file.Name, 1, file.LastWriteTime, file.Length, LumiSoft.Net.IMAP.IMAP_MessageFlags.None)
        Next
    End Sub

    Protected Overridable Sub GetMessageItems()

    End Sub

    <Test(Description:="Path throws exception when empty")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub ImapMonitorPathException()
        Dim monitor As New ImapMonitor("NewMonitor", String.Empty, New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Name throws exception when empty")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub ImapMonitorNameException()
        Dim monitor As New ImapMonitor(String.Empty, "imap://foo.com", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Test successful directory monitor")> _
    Public Sub ImapMonitor()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitor", ImapUri.AbsoluteUri, schedule, processor)
                    monitor.Path = "imaps://mail.chrislaco.com"
                    monitor.Credentials = New NetworkCredential("claco@chrislaco.com", "aH57T29Re3#")

                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    'monitor.Credentials = New NetworkCredential(ImapUser, ImapPass)
                    monitor.Filter = String.Empty
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="DirectoryMonitor path tests")> _
    Public Sub ImapDirectoryPaths()
        Using monitor As New ImapMonitor("ImapMonitor", "imap://foo.com/", New IntervalSchedule, New MockProcessor)
            REM path gets uri
            monitor.Path = "imap://foo.com/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual("imap://foo.com:143/bar", monitor.Uri.ToString)

            REM imaps path gets uri
            monitor.Path = "imaps://foo.com/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual("imaps://foo.com:993/bar", monitor.Uri.ToString)

            REM uri gets uri
            monitor.Uri = New Uri("imap://foo.com/baz")
            Assert.AreEqual("/baz", monitor.Path)
            Assert.AreEqual("imap://foo.com:143/baz", monitor.Uri.ToString)

            REM blank defaults
            Assert.IsNull(monitor.CompleteUri)
            Assert.IsNull(monitor.FailureUri)
            Assert.IsTrue(String.IsNullOrEmpty(monitor.CompletePath))
            Assert.IsTrue(String.IsNullOrEmpty(monitor.FailurePath))

            REM Relative paths
            monitor.Path = "imap://foo.com/bar"
            monitor.CompletePath = "Processed"
            Assert.AreEqual("/bar/Processed", monitor.CompletePath)
            Assert.AreEqual("imap://foo.com:143/bar/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "Failed"
            Assert.AreEqual("/bar/Failed", monitor.FailurePath)
            Assert.AreEqual("imap://foo.com:143/bar/Failed", monitor.FailureUri.ToString)

            REM Parent Paths
            monitor.Path = "imap://foo.com/quix"
            monitor.CompletePath = "../Processed"
            Assert.AreEqual("/Processed", monitor.CompletePath)
            Assert.AreEqual("imap://foo.com:143/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "../Failed"
            Assert.AreEqual("/Failed", monitor.FailurePath)
            Assert.AreEqual("imap://foo.com:143/Failed", monitor.FailureUri.ToString)
            monitor.CompletePath = "..\Processed"
            Assert.AreEqual("/Processed", monitor.CompletePath)
            Assert.AreEqual("imap://foo.com:143/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "..\Failed"
            Assert.AreEqual("/Failed", monitor.FailurePath)
            Assert.AreEqual("imap://foo.com:143/Failed", monitor.FailureUri.ToString)

            REM Absolute Paths: Drive letter retained from main Path/Uri
            monitor.Path = "imap://foo.com/bar/baz"
            monitor.CompletePath = "/Processed"
            Assert.AreEqual("/Processed", monitor.CompletePath)
            Assert.AreEqual("imap://foo.com:143/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "/Failed"
            Assert.AreEqual("/Failed", monitor.FailurePath)
            Assert.AreEqual("imap://foo.com:143/Failed", monitor.FailureUri.ToString)

            REM complete/failure uri paths
            monitor.CompleteUri = New Uri("imap://foo.com/foo")
            Assert.AreEqual("/foo", monitor.CompletePath)
            Assert.AreEqual("imap://foo.com:143/foo", monitor.CompleteUri.ToString)
            monitor.FailureUri = New Uri("imap://foo.com/foo")
            Assert.AreEqual("/foo", monitor.FailurePath)
            Assert.AreEqual("imap://foo.com:143/foo", monitor.FailureUri.ToString)

            REM complete/failure uri to paths
            monitor.CompletePath = New Uri("imap://foo.com/foo").ToString
            Assert.AreEqual("/foo", monitor.CompletePath)
            Assert.AreEqual("imap://foo.com:143/foo", monitor.CompleteUri.ToString)
            monitor.FailurePath = New Uri("imap://foo.com/foo").ToString
            Assert.AreEqual("/foo", monitor.FailurePath)
            Assert.AreEqual("imap://foo.com:143/foo", monitor.FailureUri.ToString)

            REM Uri with user credentials
            monitor.Path = "imap://user:pass@foo.com/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual("imap://foo.com:143/bar", monitor.Uri.ToString)
            Assert.AreEqual("user", monitor.Credentials.UserName)
            Assert.AreEqual("pass", monitor.Credentials.Password)

            REM Uri with user credentials and port
            monitor.Path = "imap://user2:pass2@foo.com:993/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual(993, monitor.Uri.Port)
            Assert.AreEqual("imap://foo.com:993/bar", monitor.Uri.ToString)
            Assert.AreEqual("user2", monitor.Credentials.UserName)
            Assert.AreEqual("pass2", monitor.Credentials.Password)

            REM no path gets INBOX
            monitor.Path = "imap://foo.com/"
            Assert.AreEqual("/INBOX", monitor.Path)
            Assert.AreEqual("imap://foo.com:143/INBOX", monitor.Uri.ToString)
        End Using
    End Sub

End Class
