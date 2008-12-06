Imports System.Configuration
Imports System.IO
Imports System.Messaging
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Imap Monitor Tests")> _
Public Class ImapMonitorTests
    Inherits TestBase

    '<Test()> _
    'Public Sub Tinker()
    '    Using monitor As New ImapMonitor
    '        monitor.Uri = New Uri("imap://mail.chrislaco.com:993/INBOX")
    '        monitor.Credentials = New Net.NetworkCredential("claco@chrislaco.com", "aH57T29Re3#")
    '        monitor.Scan()
    '    End Using
    'End Sub

    <Test(Description:="DirectoryMonitor path tests")> _
    Public Sub ImapDirectoryPaths()
        Using monitor As New ImapMonitor("ImapMonitor", "imap://foo.com/", New IntervalSchedule, New MockProcessor)
            'Assert.AreEqual(FtpUri.AbsolutePath, monitor.Path)
            'Assert.AreEqual(FtpUri.AbsoluteUri, monitor.Uri.ToString)

            REM path gets uri
            monitor.Path = "imap://foo.com/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual("imap://foo.com/bar", monitor.Uri.ToString)

            REM uri gets uri
            monitor.Uri = New Uri("imap://foo.com/baz")
            Assert.AreEqual("/baz", monitor.Path)
            Assert.AreEqual("imap://foo.com/baz", monitor.Uri.ToString)

            REM blank defaults
            Assert.IsNull(monitor.CompleteUri)
            Assert.IsNull(monitor.FailureUri)
            Assert.IsTrue(String.IsNullOrEmpty(monitor.CompletePath))
            Assert.IsTrue(String.IsNullOrEmpty(monitor.FailurePath))

            REM Relative paths
            monitor.Path = "imap://foo.com/bar"
            monitor.CompletePath = "Processed"
            Assert.AreEqual("/bar/Processed", monitor.CompletePath)
            Assert.AreEqual("imap://foo.com/bar/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "Failed"
            Assert.AreEqual("/bar/Failed", monitor.FailurePath)
            Assert.AreEqual("imap://foo.com/bar/Failed", monitor.FailureUri.ToString)

            REM Parent Paths
            monitor.Path = "imap://foo.com/quix"
            monitor.CompletePath = "../Processed"
            Assert.AreEqual("/Processed", monitor.CompletePath)
            Assert.AreEqual("imap://foo.com/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "../Failed"
            Assert.AreEqual("/Failed", monitor.FailurePath)
            Assert.AreEqual("imap://foo.com/Failed", monitor.FailureUri.ToString)
            monitor.CompletePath = "..\Processed"
            Assert.AreEqual("/Processed", monitor.CompletePath)
            Assert.AreEqual("imap://foo.com/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "..\Failed"
            Assert.AreEqual("/Failed", monitor.FailurePath)
            Assert.AreEqual("imap://foo.com/Failed", monitor.FailureUri.ToString)

            REM Absolute Paths: Drive letter retained from main Path/Uri
            monitor.Path = "imap://foo.com/bar/baz"
            monitor.CompletePath = "/Processed"
            Assert.AreEqual("/Processed", monitor.CompletePath)
            Assert.AreEqual("imap://foo.com/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "/Failed"
            Assert.AreEqual("/Failed", monitor.FailurePath)
            Assert.AreEqual("imap://foo.com/Failed", monitor.FailureUri.ToString)

            REM complete/failure uri paths
            monitor.CompleteUri = New Uri("imap://foo.com/foo")
            Assert.AreEqual("/foo", monitor.CompletePath)
            Assert.AreEqual("imap://foo.com/foo", monitor.CompleteUri.ToString)
            monitor.FailureUri = New Uri("imap://foo.com/foo")
            Assert.AreEqual("/foo", monitor.FailurePath)
            Assert.AreEqual("imap://foo.com/foo", monitor.FailureUri.ToString)

            REM complete/failure uri to paths
            monitor.CompletePath = New Uri("imap://foo.com/foo").ToString
            Assert.AreEqual("/foo", monitor.CompletePath)
            Assert.AreEqual("imap://foo.com/foo", monitor.CompleteUri.ToString)
            monitor.FailurePath = New Uri("imap://foo.com/foo").ToString
            Assert.AreEqual("/foo", monitor.FailurePath)
            Assert.AreEqual("imap://foo.com/foo", monitor.FailureUri.ToString)

            REM Uri with user credentials
            monitor.Path = "imap://user:pass@foo.com/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual("imap://foo.com/bar", monitor.Uri.ToString)
            Assert.AreEqual("user", monitor.Credentials.UserName)
            Assert.AreEqual("pass", monitor.Credentials.Password)

            REM Uri with user credentials and port
            monitor.Path = "imap://user2:pass2@foo.com:993/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual(993, monitor.Uri.Port)
            Assert.AreEqual("imap://foo.com:993/bar", monitor.Uri.ToString)
            Assert.AreEqual("user2", monitor.Credentials.UserName)
            Assert.AreEqual("pass2", monitor.Credentials.Password)
        End Using
    End Sub

End Class
