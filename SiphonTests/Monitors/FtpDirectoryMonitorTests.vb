Imports System.Configuration
Imports System.IO
Imports System.Messaging
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="FTP Directory Monitor Tests")> _
    Public Class FtpDirectoryMonitorTests
    Inherits TestBase

    Protected Overrides Sub CreateTestDirectory()
        TestDirectory = System.IO.Directory.CreateDirectory(Path.Combine(FtpDirectory.FullName, Path.GetRandomFileName))
    End Sub

    Protected Overridable ReadOnly Property FtpDirectory() As DirectoryInfo
        Get
            Return New DirectoryInfo(ConfigurationManager.AppSettings("FtpDirectory"))
        End Get
    End Property

    Protected Overridable ReadOnly Property FtpUri() As Uri
        Get
            Return New Uri(ConfigurationManager.AppSettings("FtpUri"))
        End Get
    End Property

    Protected Overridable ReadOnly Property FtpUser() As String
        Get
            Return ConfigurationManager.AppSettings("FtpUser")
        End Get
    End Property

    Protected Overridable ReadOnly Property FtpPass() As String
        Get
            Return ConfigurationManager.AppSettings("FtpPass")
        End Get
    End Property

    <Test(Description:="Path throws exception when empty")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub DirectoryMonitorPathException()
        Dim monitor As New FtpDirectoryMonitor("NewMonitor", String.Empty, New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Name throws exception when empty")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub DirectoryMonitorNameException()
        Dim monitor As New FtpDirectoryMonitor(String.Empty, "C:\temp", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Test successful directory monitor")> _
    <Explicit()> _
    Public Sub DirectoryMonitor()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(FtpUri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    monitor.Filter = String.Empty
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub
End Class
