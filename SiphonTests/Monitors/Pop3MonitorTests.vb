Imports System.Configuration
Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Pop3 Monitor Tests")> _
Public Class Pop3MonitorTests
    Inherits Pop3TestBase

    <Test(Description:="Path throws exception when empty")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub Pop3MonitorPathException()
        Dim monitor As New Pop3Monitor("Pop3Monitor", String.Empty, New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Name throws exception when empty")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub Pop3MonitorNameException()
        Dim monitor As New Pop3Monitor(String.Empty, "pop://foo.com", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Test successful mailbox monitor")> _
    Public Sub Pop3Monitor()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As Pop3Monitor = New Pop3Monitor("Pop3Monitor", Uri.AbsoluteUri, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.Credentials = Me.Credentials
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
    Public Sub Pop3DirectoryPaths()
        Using monitor As New Pop3Monitor("Pop3Monitor", "pop://foo.com/", New IntervalSchedule, New MockProcessor)
            REM path gets uri
            monitor.Path = "pop://foo.com/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual("pop://foo.com:110/bar", monitor.Uri.ToString)

            REM imaps path gets uri
            monitor.Path = "pops://foo.com/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual("pops://foo.com:995/bar", monitor.Uri.ToString)

            REM uri gets uri
            monitor.Uri = New Uri("pop://foo.com/baz")
            Assert.AreEqual("/baz", monitor.Path)
            Assert.AreEqual("pop://foo.com:110/baz", monitor.Uri.ToString)

            REM blank defaults
            Assert.IsNull(monitor.CompleteUri)
            Assert.IsNull(monitor.FailureUri)
            Assert.IsTrue(String.IsNullOrEmpty(monitor.CompletePath))
            Assert.IsTrue(String.IsNullOrEmpty(monitor.FailurePath))

            REM Relative paths
            monitor.Path = "pop://foo.com/bar"
            monitor.CompletePath = "Processed"
            Assert.AreEqual("/bar/Processed", monitor.CompletePath)
            Assert.AreEqual("pop://foo.com:110/bar/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "Failed"
            Assert.AreEqual("/bar/Failed", monitor.FailurePath)
            Assert.AreEqual("pop://foo.com:110/bar/Failed", monitor.FailureUri.ToString)

            REM Parent Paths
            monitor.Path = "pop://foo.com/quix"
            monitor.CompletePath = "../Processed"
            Assert.AreEqual("/Processed", monitor.CompletePath)
            Assert.AreEqual("pop://foo.com:110/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "../Failed"
            Assert.AreEqual("/Failed", monitor.FailurePath)
            Assert.AreEqual("pop://foo.com:110/Failed", monitor.FailureUri.ToString)
            monitor.CompletePath = "..\Processed"
            Assert.AreEqual("/Processed", monitor.CompletePath)
            Assert.AreEqual("pop://foo.com:110/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "..\Failed"
            Assert.AreEqual("/Failed", monitor.FailurePath)
            Assert.AreEqual("pop://foo.com:110/Failed", monitor.FailureUri.ToString)

            REM Absolute Paths: Drive letter retained from main Path/Uri
            monitor.Path = "pop://foo.com/bar/baz"
            monitor.CompletePath = "/Processed"
            Assert.AreEqual("/Processed", monitor.CompletePath)
            Assert.AreEqual("pop://foo.com:110/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "/Failed"
            Assert.AreEqual("/Failed", monitor.FailurePath)
            Assert.AreEqual("pop://foo.com:110/Failed", monitor.FailureUri.ToString)

            REM complete/failure uri paths
            monitor.CompleteUri = New Uri("pop://foo.com/foo")
            Assert.AreEqual("/foo", monitor.CompletePath)
            Assert.AreEqual("pop://foo.com:110/foo", monitor.CompleteUri.ToString)
            monitor.FailureUri = New Uri("pop://foo.com/foo")
            Assert.AreEqual("/foo", monitor.FailurePath)
            Assert.AreEqual("pop://foo.com:110/foo", monitor.FailureUri.ToString)

            REM complete/failure uri to paths
            monitor.CompletePath = New Uri("pop://foo.com/foo").ToString
            Assert.AreEqual("/foo", monitor.CompletePath)
            Assert.AreEqual("pop://foo.com:110/foo", monitor.CompleteUri.ToString)
            monitor.FailurePath = New Uri("pop://foo.com/foo").ToString
            Assert.AreEqual("/foo", monitor.FailurePath)
            Assert.AreEqual("pop://foo.com:110/foo", monitor.FailureUri.ToString)

            REM Uri with user credentials
            monitor.Path = "pop://user:pass@foo.com/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual("pop://foo.com:110/bar", monitor.Uri.ToString)
            Assert.AreEqual("user", monitor.Credentials.UserName)
            Assert.AreEqual("pass", monitor.Credentials.Password)

            REM Uri with user credentials and port
            monitor.Path = "pop://user2:pass2@foo.com:110/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual(110, monitor.Uri.Port)
            Assert.AreEqual("pop://foo.com:110/bar", monitor.Uri.ToString)
            Assert.AreEqual("user2", monitor.Credentials.UserName)
            Assert.AreEqual("pass2", monitor.Credentials.Password)
        End Using
    End Sub
End Class
