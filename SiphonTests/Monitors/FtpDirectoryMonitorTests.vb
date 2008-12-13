Imports System.Configuration
Imports System.IO
Imports System.Messaging
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="FTP Directory Monitor Tests")> _
    Public Class FtpDirectoryMonitorTests
    Inherits FtpTestBase

    Protected Overrides Sub CreateTestDirectory()
        TestDirectory = System.IO.Directory.CreateDirectory(Path.Combine(FtpDirectory.FullName, Path.GetRandomFileName))
    End Sub

    Protected Overridable ReadOnly Property FtpDirectory() As DirectoryInfo
        Get
            Return New DirectoryInfo(ConfigurationManager.AppSettings("FtpDirectory"))
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
    Public Sub DirectoryMonitor()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.Credentials = Me.Credentials
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

    <Test(Description:="Test successful directory monitor process complete deletes file")> _
    Public Sub DirectoryMonitorProcessorCompleteDeleteFile()
        CreateSuccessFile("SUCCESS")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                    monitor.ProcessCompleteActions = DataActions.Delete
                    monitor.Credentials = New NetworkCredential("anonymous", "claco@chrislaco.com")
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                    Assert.IsFalse(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test successful directory monitor process complete renames file")> _
    Public Sub DirectoryMonitorProcessorCompleteRenameFile()
        CreateSuccessFile("SUCCESS")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Dim builder As New UriBuilder(Uri)
                builder.UserName = Me.Credentials.UserName
                builder.Password = Me.Credentials.Password

                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(builder.Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                    monitor.ProcessCompleteActions = DataActions.Rename
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                    Assert.IsFalse(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                    Assert.AreEqual(1, TestDirectory.GetFiles.Count)

                    Dim files() As FileInfo = TestDirectory.GetFiles
                    Assert.IsTrue(Regex.IsMatch(files(0).Name, "SUCCESS\.{?([0-9a-fA-F]){8}(-([0-9a-fA-F]){4}){3}-([0-9a-fA-F]){12}}?", RegexOptions.IgnoreCase))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test successful directory monitor process complete moves file")> _
    Public Sub DirectoryMonitorProcessorCompleteMoveFile()
        CreateSuccessFile("SUCCESS")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                    monitor.CompletePath = "Processed"
                    monitor.ProcessCompleteActions = DataActions.Move
                    monitor.CreateMissingFolders = True
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
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

    <Test(Description:="Test successful directory monitor process complete moves file")> _
    Public Sub DirectoryMonitorProcessorCompleteMoveRenameFile()
        CreateSuccessFile("SUCCESS")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                    monitor.CompletePath = "Processed"
                    monitor.ProcessCompleteActions = DataActions.Move Or DataActions.Rename
                    monitor.CreateMissingFolders = True
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                    Assert.IsFalse(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                    Assert.AreEqual(0, TestDirectory.GetFiles.Count)
                    Assert.AreEqual(1, Directory.GetFiles(Path.Combine(TestDirectory.FullName, "Processed")).Length)

                    Dim files() As String = Directory.GetFiles(Path.Combine(TestDirectory.FullName, "Processed"))
                    Dim file1 As New FileInfo(files(0))
                    Assert.IsTrue(Regex.IsMatch(file1.Name, "SUCCESS\.{?([0-9a-fA-F]){8}(-([0-9a-fA-F]){4}){3}-([0-9a-fA-F]){12}}?", RegexOptions.IgnoreCase))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor failure")> _
    Public Sub DirectoryMonitorProcessorFailure()
        CreateFailureFile("FAILURE")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor failure deletes file")> _
    Public Sub DirectoryMonitorProcessorFailureDeleteFile()
        CreateFailureFile("FAILURE")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                    monitor.ProcessFailureActions = DataActions.Delete
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                    Assert.IsFalse(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor failure renames file")> _
    Public Sub DirectoryMonitorProcessorFailureRenameFile()
        CreateFailureFile("FAILURE")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                    monitor.ProcessFailureActions = DataActions.Rename
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                    Assert.IsFalse(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                    Assert.AreEqual(1, TestDirectory.GetFiles.Count)

                    Dim files() As FileInfo = TestDirectory.GetFiles
                    Assert.IsTrue(Regex.IsMatch(files(0).Name, "FAILURE\.{?([0-9a-fA-F]){8}(-([0-9a-fA-F]){4}){3}-([0-9a-fA-F]){12}}?", RegexOptions.IgnoreCase))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test successful directory monitor process failure moves file")> _
    Public Sub DirectoryMonitorProcessorFailureMoveFile()
        CreateFailureFile("FAILURE")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                    monitor.FailurePath = "Failed"
                    monitor.ProcessFailureActions = DataActions.Move
                    monitor.CreateMissingFolders = True
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
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

    <Test(Description:="Test successful directory monitor process failure moves file")> _
    Public Sub DirectoryMonitorProcessorFailureMoveRenameFile()
        CreateFailureFile("FAILURE")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                    monitor.FailurePath = "Failed"
                    monitor.ProcessFailureActions = DataActions.Move Or DataActions.Rename
                    monitor.CreateMissingFolders = True
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                    Assert.IsFalse(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                    Assert.AreEqual(0, TestDirectory.GetFiles.Count)
                    Assert.AreEqual(1, Directory.GetFiles(Path.Combine(TestDirectory.FullName, "Failed")).Length)

                    Dim files() As String = Directory.GetFiles(Path.Combine(TestDirectory.FullName, "Failed"))
                    Dim file1 As New FileInfo(files(0))
                    Assert.IsTrue(Regex.IsMatch(file1.Name, "FAILURE\.{?([0-9a-fA-F]){8}(-([0-9a-fA-F]){4}){3}-([0-9a-fA-F]){12}}?", RegexOptions.IgnoreCase))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor exception")> _
    Public Sub DirectoryMonitorProcessorException()
        CreateExceptionFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                    Assert.IsFalse(Me.ProcessComplete)
                    Assert.IsTrue(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor create missing directories")> _
    Public Sub DirectoryMonitorCreateDirectoriesNested()
        Dim newdir As String = Path.GetRandomFileName
        Dim tempdir As String = Path.Combine(TestDirectory.FullName, newdir)

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(1).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), newdir), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.CreateMissingFolders = True
                    monitor.CompletePath = "Processed"
                    monitor.FailurePath = "Failed"

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

    <Test(Description:="Test directory monitor create missing directories")> _
    Public Sub DirectoryMonitorCreateDirectories()
        Dim newdir As String = Path.GetRandomFileName
        Dim tempdir As String = Path.Combine(TestDirectory.FullName, newdir)

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(1).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), newdir), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.CreateMissingFolders = True
                    monitor.CompletePath = "../Processed"
                    monitor.FailurePath = "../Failed"

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
    Public Sub DirectoryMonitorWithFilter()
        CreateSuccessFile()
        CreateSuccessFile("test.csv")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    monitor.Filter = "*.csv"

                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 files")
                    Assert.IsTrue(Me.ProcessComplete)
                    Assert.IsFalse(Me.ProcessFailure)
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with slow processor on stop")> _
    Public Sub DirectoryMonitorStillProcessing()
        CreateSuccessFile()

        Using schedule = New IntervalSchedule(2)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    processor.Delay = 10
                    monitor.Start()
                    Threading.Thread.Sleep(3000)

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
    Public Sub FtpDirectoryPaths()
        Using monitor As New FtpDirectoryMonitor("FtpMonitor", Uri.AbsoluteUri, New IntervalSchedule, New MockProcessor)
            Assert.AreEqual(Uri.AbsolutePath, monitor.Path)
            Assert.AreEqual(Uri.AbsoluteUri, monitor.Uri.ToString)

            REM path gets uri
            monitor.Path = "ftp://foo.com/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual("ftp://foo.com/bar", monitor.Uri.ToString)

            REM uri gets uri
            monitor.Uri = New Uri("ftp://foo.com/baz")
            Assert.AreEqual("/baz", monitor.Path)
            Assert.AreEqual("ftp://foo.com/baz", monitor.Uri.ToString)

            REM blank defaults
            Assert.IsNull(monitor.CompleteUri)
            Assert.IsNull(monitor.FailureUri)
            Assert.IsTrue(String.IsNullOrEmpty(monitor.CompletePath))
            Assert.IsTrue(String.IsNullOrEmpty(monitor.FailurePath))

            REM Relative paths
            monitor.Path = "ftp://foo.com/bar"
            monitor.CompletePath = "Processed"
            Assert.AreEqual("/bar/Processed", monitor.CompletePath)
            Assert.AreEqual("ftp://foo.com/bar/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "Failed"
            Assert.AreEqual("/bar/Failed", monitor.FailurePath)
            Assert.AreEqual("ftp://foo.com/bar/Failed", monitor.FailureUri.ToString)

            REM Parent Paths
            monitor.Path = "ftp://foo.com/quix"
            monitor.CompletePath = "../Processed"
            Assert.AreEqual("/Processed", monitor.CompletePath)
            Assert.AreEqual("ftp://foo.com/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "../Failed"
            Assert.AreEqual("/Failed", monitor.FailurePath)
            Assert.AreEqual("ftp://foo.com/Failed", monitor.FailureUri.ToString)
            monitor.CompletePath = "..\Processed"
            Assert.AreEqual("/Processed", monitor.CompletePath)
            Assert.AreEqual("ftp://foo.com/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "..\Failed"
            Assert.AreEqual("/Failed", monitor.FailurePath)
            Assert.AreEqual("ftp://foo.com/Failed", monitor.FailureUri.ToString)

            REM Absolute Paths: Drive letter retained from main Path/Uri
            monitor.Path = "ftp://foo.com/bar/baz"
            monitor.CompletePath = "/Processed"
            Assert.AreEqual("/Processed", monitor.CompletePath)
            Assert.AreEqual("ftp://foo.com/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "/Failed"
            Assert.AreEqual("/Failed", monitor.FailurePath)
            Assert.AreEqual("ftp://foo.com/Failed", monitor.FailureUri.ToString)

            REM complete/failure uri paths
            monitor.CompleteUri = New Uri("ftp://foo.com/foo")
            Assert.AreEqual("/foo", monitor.CompletePath)
            Assert.AreEqual("ftp://foo.com/foo", monitor.CompleteUri.ToString)
            monitor.FailureUri = New Uri("ftp://foo.com/foo")
            Assert.AreEqual("/foo", monitor.FailurePath)
            Assert.AreEqual("ftp://foo.com/foo", monitor.FailureUri.ToString)

            REM complete/failure uri to paths
            monitor.CompletePath = New Uri("ftp://foo.com/foo").ToString
            Assert.AreEqual("/foo", monitor.CompletePath)
            Assert.AreEqual("ftp://foo.com/foo", monitor.CompleteUri.ToString)
            monitor.FailurePath = New Uri("ftp://foo.com/foo").ToString
            Assert.AreEqual("/foo", monitor.FailurePath)
            Assert.AreEqual("ftp://foo.com/foo", monitor.FailureUri.ToString)

            REM Uri with user credentials
            monitor.Path = "ftp://user:pass@foo.com/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual("ftp://foo.com/bar", monitor.Uri.ToString)
            Assert.AreEqual("user", monitor.Credentials.UserName)
            Assert.AreEqual("pass", monitor.Credentials.Password)

            REM Uri with user credentials and port
            monitor.Path = "ftp://user2:pass2@foo.com:993/bar"
            Assert.AreEqual("/bar", monitor.Path)
            Assert.AreEqual(993, monitor.Uri.Port)
            Assert.AreEqual("ftp://foo.com:993/bar", monitor.Uri.ToString)
            Assert.AreEqual("user2", monitor.Credentials.UserName)
            Assert.AreEqual("pass2", monitor.Credentials.Password)
        End Using
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub UriUnsupportedException()
        Dim monitor As New FtpDirectoryMonitor("TestName", "file:///C:/bar", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub FailureUriUnsupportedException()
        Dim monitor As New FtpDirectoryMonitor("TestName", "ftp://foo.com/bar", New IntervalSchedule, New MockProcessor)

        monitor.FailureUri = New Uri("file:///C:/foo")
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub CompleteUriUnsupportedException()
        Dim monitor As New FtpDirectoryMonitor("TestName", "ftp://foo.com/bar", New IntervalSchedule, New MockProcessor)
        Dim uri = New Uri("file:///C:/foo")

        monitor.CompleteUri = uri
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub DownloadUriUnsupportedException()
        Dim monitor As New FtpDirectoryMonitor("TestName", "ftp://foo.com/bar", New IntervalSchedule, New MockProcessor)
        Dim uri = New Uri("ftp://foo.com/bar")

        monitor.DownloadUri = uri
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub PathEmptyArgumentException()
        Dim monitor As New FtpDirectoryMonitor("TestName", "", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub NameEmptyArgumentException()
        Dim monitor As New FtpDirectoryMonitor("", "file:///C:/boo", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Path returns empty with no uri")> _
    Public Sub NoUriReturnsEmpty()
        Dim monitor As FtpDirectoryMonitor = GetType(FtpDirectoryMonitor).Assembly.CreateInstance(GetType(FtpDirectoryMonitor).FullName, False, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)

        Assert.IsTrue(String.IsNullOrEmpty(monitor.Path))
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullUriException()
        Using monitor As New FtpDirectoryMonitor("TestMonitor", "ftp://foo.com/bar", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As FtpDirectoryMonitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.FtpDirectoryMonitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.Name = "Test"
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullNameException()
        Using monitor As New FtpDirectoryMonitor("TestMonitor", "ftp://foo.com/bar", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As FtpDirectoryMonitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.FtpDirectoryMonitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.Path = "ftp://foo.com/bar"
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullCompleteUriException()
        Using monitor As New FtpDirectoryMonitor("TestMonitor", "ftp://foo.com/bar", New IntervalSchedule, New MockProcessor)
            monitor.ProcessCompleteActions = DataActions.Move
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullFailureUriException()
        Using monitor As New FtpDirectoryMonitor("TestMonitor", "ftp://foo.com/bar", New IntervalSchedule, New MockProcessor)
            monitor.ProcessFailureActions = DataActions.Move
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Test successful directory monitor remote downloads to download path")> _
    Public Sub DirectoryMonitorDownloadPath()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.DownloadPath = Path.Combine(TestDirectory.FullName, "Downloads")
                    monitor.CreateMissingFolders = True
                    monitor.Filter = String.Empty
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
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
    Public Sub DirectoryMonitorDownloadUri()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As FtpDirectoryMonitor = New FtpDirectoryMonitor("FtpMonitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.DownloadPath = New Uri(Path.Combine(TestDirectory.FullName, "Downloads")).AbsoluteUri
                    monitor.CreateMissingFolders = True
                    monitor.Filter = String.Empty
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
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
        Dim monitorElement As New MonitorElement("TestQueueMonitor", "ChrisLaco.Siphon.FtpDirectoryMonitor, Siphon")
        Dim processorElement As New ProcessorElement("ChrisLaco.Tests.Siphon.MockProcessor, SiphonTests")
        Dim scheduleElement As New ScheduleElement("ChrisLaco.Siphon.DailySchedule, Siphon")
        scheduleElement.Daily.Add(DateTime.Now.AddSeconds(3).TimeOfDay)
        monitorElement.Schedule = scheduleElement
        monitorElement.Processor = processorElement
        monitorElement.Settings.Add(New NameValueConfigurationElement("Path", "ftp://foo.com/bar"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("CompletePath", "Processed"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("FailurePath", "Failed"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("CreateMissingFolders", "true"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("Username", "FtpUser"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("Password", "FtpPass"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("Domain", "FtpDomain"))

        Using monitor As FtpDirectoryMonitor = monitorElement.CreateInstance
            Assert.IsInstanceOfType(GetType(FtpDirectoryMonitor), monitor)
            Assert.IsInstanceOfType(GetType(Uri), monitor.Uri)
            Assert.IsInstanceOfType(GetType(Uri), monitor.CompleteUri)
            Assert.IsInstanceOfType(GetType(Uri), monitor.FailureUri)
            Assert.IsInstanceOfType(GetType(NetworkCredential), monitor.Credentials)
            Assert.AreEqual("ftp://foo.com/bar", monitor.Uri.ToString)
            Assert.AreEqual("ftp://foo.com/bar/Processed", monitor.CompleteUri.ToString)
            Assert.AreEqual("ftp://foo.com/bar/Failed", monitor.FailureUri.ToString)
            Assert.AreEqual("FtpUser", monitor.Credentials.UserName)
            Assert.AreEqual("FtpPass", monitor.Credentials.Password)
            Assert.AreEqual("FtpDomain", monitor.Credentials.Domain)
            Assert.IsTrue(monitor.CreateMissingFolders)
        End Using
    End Sub

    <Test(Description:="Set credentials when they're passed in the Uri")> _
    Public Sub CredentialsFromUri()
        Using monitor As New FtpDirectoryMonitor("FtpMonitor", "ftp://claco:mypass@foo.com/bar", New IntervalSchedule, New MockProcessor)
            Assert.AreEqual("ftp://foo.com/bar", monitor.Uri.ToString)
            Assert.AreEqual("claco", monitor.Credentials.UserName)
            Assert.AreEqual("mypass", monitor.Credentials.Password)
        End Using
    End Sub

    <Test(Description:="Set credentials when they're passed in the Uri")> _
    Public Sub PartialCredentialsFromUri()
        Using monitor As New FtpDirectoryMonitor("FtpMonitor", "ftp://claco@foo.com/bar", New IntervalSchedule, New MockProcessor)
            Assert.AreEqual("ftp://foo.com/bar", monitor.Uri.ToString)
            Assert.AreEqual("claco", monitor.Credentials.UserName)
            Assert.AreEqual(String.Empty, monitor.Credentials.Password)
        End Using
    End Sub

End Class
