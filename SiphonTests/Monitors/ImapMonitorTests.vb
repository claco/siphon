Imports System.Configuration
Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Imap Monitor Tests")> _
Public Class ImapMonitorTests
    Inherits ImapTestBase

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
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitor", Uri.AbsoluteUri, schedule, processor)
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

    <Test(Description:="Test successful directory monitor process complete deletes message")> _
    Public Sub ImapMonitorProcessorCompleteDeleteMessage()
        CreateSuccessFile("SUCCESS")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                    monitor.ProcessCompleteActions = DataActions.Delete
                    monitor.Credentials = Me.Credentials
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                    Assert.IsFalse(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test successful directory monitor process complete moves message")> _
    Public Sub ImapMonitorProcessorCompleteMoveMessage()
        CreateSuccessFile("SUCCESS")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(3).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                    monitor.CompletePath = "Processed"
                    monitor.ProcessCompleteActions = DataActions.Move
                    monitor.CreateMissingFolders = True
                    monitor.Credentials = Me.Credentials
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                    Assert.IsFalse(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                    Assert.IsTrue(File.Exists(Path.Combine(Path.Combine(TestDirectory.FullName, "Processed"), "SUCCESS")))
                    Assert.AreEqual(0, TestDirectory.GetFiles.Count)
                    Assert.AreEqual(1, Directory.GetFiles(Path.Combine(TestDirectory.FullName, "Processed")).Length)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor failure")> _
    Public Sub ImapMonitorProcessorFailure()
        CreateFailureFile("FAILURE")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                    monitor.Credentials = Me.Credentials
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor failure deletes message")> _
    Public Sub ImapMonitorProcessorFailureDeleteMessage()
        CreateFailureFile("FAILURE")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDirectoryMonitor = New ImapMonitor("ImapMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                    monitor.ProcessFailureActions = DataActions.Delete
                    monitor.Credentials = Me.Credentials
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                    Assert.IsFalse(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test successful directory monitor process failure moves message")> _
    Public Sub ImapMonitorProcessorFailureMoveMessage()
        CreateFailureFile("FAILURE")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(3).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                    monitor.FailurePath = "Failed"
                    monitor.ProcessFailureActions = DataActions.Move
                    monitor.CreateMissingFolders = True
                    monitor.Credentials = Me.Credentials
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                    Assert.IsFalse(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                    Assert.IsTrue(File.Exists(Path.Combine(Path.Combine(TestDirectory.FullName, "Failed"), "FAILURE")))
                    Assert.AreEqual(0, TestDirectory.GetFiles.Count)
                    Assert.AreEqual(1, Directory.GetFiles(Path.Combine(TestDirectory.FullName, "Failed")).Length)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor exception")> _
    Public Sub ImapMonitorProcessorException()
        CreateExceptionFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    monitor.Credentials = Me.Credentials
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor create missing folders")> _
    Public Sub ImapMonitorCreateDirectoriesNested()
        Dim newdir As String = Path.GetRandomFileName
        Dim tempdir As String = Path.Combine(TestDirectory.FullName, newdir)

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(1).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitor", Path.Combine(Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), newdir), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.CreateMissingFolders = True
                    monitor.CompletePath = "Processed"
                    monitor.FailurePath = "Failed"
                    monitor.Credentials = Me.Credentials

                    monitor.Start()
                    monitor.Stop()

                    Assert.IsTrue(Directory.Exists(tempdir), "Monitor path exista")
                    Assert.IsTrue(Directory.Exists(Path.Combine(tempdir, "Processed")), "Processed child path exists")
                    Assert.IsTrue(Directory.Exists(Path.Combine(tempdir, "Failed")), "Failed child path exists")
                    Assert.IsTrue(Directory.Exists(tempdir), "Monitor path exista")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor create missing folders")> _
    Public Sub ImapMonitorCreateDirectories()
        Dim newdir As String = Path.GetRandomFileName
        Dim tempdir As String = Path.Combine(TestDirectory.FullName, newdir)

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(1).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitor", Path.Combine(Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), newdir), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.CreateMissingFolders = True
                    monitor.CompletePath = "../Processed"
                    monitor.FailurePath = "../Failed"
                    monitor.Credentials = Me.Credentials

                    monitor.Start()
                    monitor.Stop()

                    Dim temp As New DirectoryInfo(tempdir)
                    Assert.IsTrue(Directory.Exists(tempdir), "Monitor path exista")
                    Assert.IsTrue(Directory.Exists(Path.Combine(temp.Parent.FullName, "Processed")), "Processed child path exists")
                    Assert.IsTrue(Directory.Exists(Path.Combine(temp.Parent.FullName, "Failed")), "Failed child path exists")
                    Assert.IsTrue(Directory.Exists(tempdir), "Monitor path exista")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with filter")> _
    <Ignore("TODO: Devise Filter strings")> _
    Public Sub ImapMonitorWithFilter()
        CreateSuccessFile()
        CreateSuccessFile("test.csv")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    monitor.Filter = "*.csv"
                    monitor.Credentials = Me.Credentials

                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 files")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with slow processor on stop")> _
    Public Sub ImapMonitorStillProcessing()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    monitor.Credentials = Me.Credentials
                    processor.Delay = 10
                    monitor.Start()
                    Threading.Thread.Sleep(5000)

                    Assert.IsTrue(monitor.Processing, "Processing is true when a worker processor is still running")
                    Dim pre As DateTime = DateTime.Now
                    monitor.Stop()
                    Dim post As DateTime = DateTime.Now

                    Assert.AreEqual(1, processor.Count, "Has processed 1 files")
                    Assert.GreaterOrEqual((post - pre).TotalSeconds, 5, "Waited for still running process to finish")
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

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub UriUnsupportedException()
        Dim monitor As New ImapMonitor("TestName", "file:///C:/bar", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub FailureUriUnsupportedException()
        Dim monitor As New ImapMonitor("TestName", "imap://foo.com/bar", New IntervalSchedule, New MockProcessor)

        monitor.FailureUri = New Uri("file:///C:/foo")
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub CompleteUriUnsupportedException()
        Dim monitor As New ImapMonitor("TestName", "imap://foo.com/bar", New IntervalSchedule, New MockProcessor)
        Dim uri = New Uri("file:///C:/foo")

        monitor.CompleteUri = uri
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub DownloadUriUnsupportedException()
        Dim monitor As New ImapMonitor("TestName", "imap://foo.com/bar", New IntervalSchedule, New MockProcessor)
        Dim uri = New Uri("ftp://foo.com/bar")

        monitor.DownloadUri = uri
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub PathEmptyArgumentException()
        Dim monitor As New ImapMonitor("TestName", "", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub NameEmptyArgumentException()
        Dim monitor As New ImapMonitor("", "imap://foo.com", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Path returns empty with no uri")> _
    Public Sub NoUriReturnsEmpty()
        Dim monitor As ImapMonitor = GetType(ImapMonitor).Assembly.CreateInstance(GetType(ImapMonitor).FullName, False, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)

        Assert.IsTrue(String.IsNullOrEmpty(monitor.Path))
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullUriException()
        Using monitor As New ImapMonitor("ImapMonitor", "imap://foo.com/bar", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As ImapMonitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.ImapMonitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.Credentials = Me.Credentials
            newMonitor.Name = "Test"
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullNameException()
        Using monitor As New ImapMonitor("ImapMonitor", "imap://foo.com/bar", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As ImapMonitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.ImapMonitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.Credentials = Me.Credentials
            newMonitor.Path = "imap://foo.com/bar"
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullCompleteUriException()
        Using monitor As New ImapMonitor("ImapMonitor", "imap://foo.com/bar", New IntervalSchedule, New MockProcessor)
            monitor.Credentials = Me.Credentials
            monitor.ProcessCompleteActions = DataActions.Move
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullFailureUriException()
        Using monitor As New ImapMonitor("ImapMonitor", "imap://foo.com/bar", New IntervalSchedule, New MockProcessor)
            monitor.Credentials = Me.Credentials
            monitor.ProcessFailureActions = DataActions.Move
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Test successful directory monitor remote downloads to download path")> _
    Public Sub ImapMonitorDownloadPath()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(3).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.DownloadPath = Path.Combine(TestDirectory.FullName, "Downloads")
                    monitor.CreateMissingFolders = True
                    monitor.Filter = String.Empty
                    monitor.Credentials = Me.Credentials
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                    Assert.IsTrue(Directory.Exists(Path.Combine(TestDirectory.FullName, "Downloads")))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test successful directory monitor remote downloads to download path")> _
    Public Sub ImapMonitorDownloadUri()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(3).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As ImapMonitor = New ImapMonitor("ImapMonitorDownloadUri", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.DownloadPath = New Uri(Path.Combine(TestDirectory.FullName, "Downloads")).AbsoluteUri
                    monitor.CreateMissingFolders = True
                    monitor.Filter = String.Empty
                    monitor.Credentials = Me.Credentials
                    monitor.Start()
                    Threading.Thread.Sleep(5000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                    Assert.IsTrue(Directory.Exists(Path.Combine(TestDirectory.FullName, "Downloads")))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Create monitor from configuration")> _
    Public Sub CreateFromConfiguration()
        Dim monitorElement As New MonitorElement("TestQueueMonitor", "ChrisLaco.Siphon.ImapMonitor, Siphon")
        Dim processorElement As New ProcessorElement("ChrisLaco.Tests.Siphon.MockProcessor, SiphonTests")
        Dim scheduleElement As New ScheduleElement("ChrisLaco.Siphon.DailySchedule, Siphon")
        scheduleElement.Daily.Add(DateTime.Now.AddSeconds(3).TimeOfDay)
        monitorElement.Schedule = scheduleElement
        monitorElement.Processor = processorElement
        monitorElement.Settings.Add(New NameValueConfigurationElement("Path", "imap://foo.com/bar"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("CompletePath", "Processed"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("FailurePath", "Failed"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("DownloadPath", "C:\Downloads"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("CreateMissingFolders", "true"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("Username", "ImapUser"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("Password", "ImapPass"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("Domain", "ImapDomain"))

        Using monitor As ImapMonitor = monitorElement.CreateInstance
            Assert.IsInstanceOfType(GetType(ImapMonitor), monitor)
            Assert.IsInstanceOfType(GetType(Uri), monitor.Uri)
            Assert.IsInstanceOfType(GetType(Uri), monitor.CompleteUri)
            Assert.IsInstanceOfType(GetType(Uri), monitor.FailureUri)
            Assert.IsInstanceOfType(GetType(Uri), monitor.DownloadUri)
            Assert.IsInstanceOfType(GetType(NetworkCredential), monitor.Credentials)
            Assert.AreEqual("imap://foo.com:143/bar", monitor.Uri.ToString)
            Assert.AreEqual("imap://foo.com:143/bar/Processed", monitor.CompleteUri.ToString)
            Assert.AreEqual("imap://foo.com:143/bar/Failed", monitor.FailureUri.ToString)
            Assert.AreEqual("file:///C:/Downloads", monitor.DownloadUri.ToString)
            Assert.AreEqual("ImapUser", monitor.Credentials.UserName)
            Assert.AreEqual("ImapPass", monitor.Credentials.Password)
            Assert.AreEqual("ImapDomain", monitor.Credentials.Domain)
            Assert.IsTrue(monitor.CreateMissingFolders)
        End Using
    End Sub

    <Test(Description:="Set credentials when they're passed in the Uri")> _
    Public Sub CredentialsFromUri()
        Using monitor As New ImapMonitor("ImapMonitor", "imap://claco:mypass@foo.com/bar", New IntervalSchedule, New MockProcessor)
            Assert.AreEqual("imap://foo.com:143/bar", monitor.Uri.ToString)
            Assert.AreEqual("claco", monitor.Credentials.UserName)
            Assert.AreEqual("mypass", monitor.Credentials.Password)
        End Using
    End Sub

    <Test(Description:="Set credentials when they're passed in the Uri")> _
    Public Sub PartialCredentialsFromUri()
        Using monitor As New ImapMonitor("ImapMonitor", "imaps://claco@foo.com/bar", New IntervalSchedule, New MockProcessor)
            Assert.AreEqual("imaps://foo.com:993/bar", monitor.Uri.ToString)
            Assert.AreEqual("claco", monitor.Credentials.UserName)
            Assert.AreEqual(String.Empty, monitor.Credentials.Password)
        End Using
    End Sub

End Class
