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

    <Test(Description:="Test successful mailbox monitor update exception")> _
    Public Sub Pop3MonitorUpdateException()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As Pop3Monitor = New Pop3Monitor("Pop3Monitor", Uri.AbsoluteUri, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.ProcessCompleteActions = DataActions.Update
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

    <Test(Description:="Test successful mailbox monitor rename exception")> _
    Public Sub Pop3MonitorRenameException()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As Pop3Monitor = New Pop3Monitor("Pop3Monitor", Uri.AbsoluteUri, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.ProcessCompleteActions = DataActions.Rename
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

    <Test(Description:="Test successful mailbox monitor move exception")> _
    Public Sub Pop3MonitorMoveException()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As Pop3Monitor = New Pop3Monitor("Pop3Monitor", Uri.AbsoluteUri, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    monitor.CompletePath = "Foo"
                    monitor.ProcessCompleteActions = DataActions.Move
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
    Public Sub Pop3MonitorProcessorCompleteDeleteMessage()
        CreateSuccessFile("SUCCESS")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As Pop3Monitor = New Pop3Monitor("Pop3Monitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
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
                    Threading.Thread.Sleep(2000)
                    Assert.IsFalse(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor failure")> _
    Public Sub Pop3MonitorProcessorFailure()
        CreateFailureFile("FAILURE")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As Pop3Monitor = New Pop3Monitor("Pop3Monitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
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
    Public Sub Pop3MonitorProcessorFailureDeleteMessage()
        CreateFailureFile("FAILURE")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDirectoryMonitor = New Pop3Monitor("Pop3Monitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
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
                    Threading.Thread.Sleep(2000)
                    Assert.IsFalse(File.Exists(Path.Combine(TestDirectory.FullName, "FAILURE")))
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor exception")> _
    Public Sub Pop3MonitorProcessorException()
        CreateExceptionFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As Pop3Monitor = New Pop3Monitor("Pop3Monitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
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

    <Test(Description:="Test directory monitor with filter")> _
    <Ignore("TODO: Devise Filter strings")> _
    Public Sub Pop3MonitorWithFilter()
        CreateSuccessFile()
        CreateSuccessFile("test.csv")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As Pop3Monitor = New Pop3Monitor("Pop3Monitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
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
    Public Sub Pop3MonitorStillProcessing()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As Pop3Monitor = New Pop3Monitor("Pop3Monitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure
                    monitor.Credentials = Me.Credentials
                    processor.Delay = 10
                    monitor.Start()
                    Threading.Thread.Sleep(5000)

                    Assert.IsTrue(monitor.IsProcessing, "Processing is true when a worker processor is still running")
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

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub UriUnsupportedException()
        Dim monitor As New Pop3Monitor("TestName", "file:///C:/bar", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub FailureUriUnsupportedException()
        Dim monitor As New Pop3Monitor("TestName", "pop://foo.com/bar", New IntervalSchedule, New MockProcessor)

        monitor.FailureUri = New Uri("file:///C:/foo")
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub CompleteUriUnsupportedException()
        Dim monitor As New Pop3Monitor("TestName", "pop://foo.com/bar", New IntervalSchedule, New MockProcessor)
        Dim uri = New Uri("file:///C:/foo")

        monitor.CompleteUri = uri
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub DownloadUriUnsupportedException()
        Dim monitor As New Pop3Monitor("TestName", "pop://foo.com/bar", New IntervalSchedule, New MockProcessor)
        Dim uri = New Uri("ftp://foo.com/bar")

        monitor.DownloadUri = uri
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub PathEmptyArgumentException()
        Dim monitor As New Pop3Monitor("TestName", "", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub NameEmptyArgumentException()
        Dim monitor As New Pop3Monitor("", "pop://foo.com", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Path returns empty with no uri")> _
    Public Sub NoUriReturnsEmpty()
        Dim monitor As Pop3Monitor = GetType(Pop3Monitor).Assembly.CreateInstance(GetType(Pop3Monitor).FullName, False, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)

        Assert.IsTrue(String.IsNullOrEmpty(monitor.Path))
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullUriException()
        Using monitor As New Pop3Monitor("Pop3Monitor", "pop://foo.com/bar", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As Pop3Monitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.Pop3Monitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.Credentials = Me.Credentials
            newMonitor.Name = "Test"
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullNameException()
        Using monitor As New Pop3Monitor("Pop3Monitor", "pop://foo.com/bar", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As Pop3Monitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.Pop3Monitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.Credentials = Me.Credentials
            newMonitor.Path = "pop://foo.com/bar"
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullCompleteUriException()
        Using monitor As New Pop3Monitor("Pop3Monitor", "pop://foo.com/bar", New IntervalSchedule, New MockProcessor)
            monitor.Credentials = Me.Credentials
            monitor.ProcessCompleteActions = DataActions.Move
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullFailureUriException()
        Using monitor As New Pop3Monitor("Pop3Monitor", "pop://foo.com/bar", New IntervalSchedule, New MockProcessor)
            monitor.Credentials = Me.Credentials
            monitor.ProcessFailureActions = DataActions.Move
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Test successful directory monitor remote downloads to download path")> _
    Public Sub Pop3MonitorDownloadPath()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As Pop3Monitor = New Pop3Monitor("Pop3Monitor", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
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
    Public Sub Pop3MonitorDownloadUri()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As Pop3Monitor = New Pop3Monitor("Pop3MonitorDownloadUri", Path.Combine(Uri.AbsoluteUri, TestDirectory.Name), schedule, processor)
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
        Dim monitorElement As New MonitorElement("TestQueueMonitor", "ChrisLaco.Siphon.Pop3Monitor, Siphon")
        Dim processorElement As New ProcessorElement("ChrisLaco.Tests.Siphon.MockProcessor, SiphonTests")
        Dim scheduleElement As New ScheduleElement("ChrisLaco.Siphon.DailySchedule, Siphon")
        scheduleElement.Daily.Add(DateTime.Now.AddSeconds(3).TimeOfDay)
        monitorElement.Schedule = scheduleElement
        monitorElement.Processor = processorElement
        monitorElement.Settings.Add(New NameValueConfigurationElement("Path", "pop://foo.com/bar"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("CompletePath", "Processed"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("FailurePath", "Failed"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("DownloadPath", "C:\Downloads"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("CreateMissingFolders", "true"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("Username", "Pop3User"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("Password", "Pop3Pass"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("Domain", "Pop3Domain"))

        Using monitor As Pop3Monitor = monitorElement.CreateInstance
            Assert.IsInstanceOfType(GetType(Pop3Monitor), monitor)
            Assert.IsInstanceOfType(GetType(Uri), monitor.Uri)
            Assert.IsInstanceOfType(GetType(Uri), monitor.CompleteUri)
            Assert.IsInstanceOfType(GetType(Uri), monitor.FailureUri)
            Assert.IsInstanceOfType(GetType(Uri), monitor.DownloadUri)
            Assert.IsInstanceOfType(GetType(NetworkCredential), monitor.Credentials)
            Assert.AreEqual("pop://foo.com:110/bar", monitor.Uri.ToString)
            Assert.AreEqual("pop://foo.com:110/bar/Processed", monitor.CompleteUri.ToString)
            Assert.AreEqual("pop://foo.com:110/bar/Failed", monitor.FailureUri.ToString)
            Assert.AreEqual("file:///C:/Downloads", monitor.DownloadUri.ToString)
            Assert.AreEqual("Pop3User", monitor.Credentials.UserName)
            Assert.AreEqual("Pop3Pass", monitor.Credentials.Password)
            Assert.AreEqual("Pop3Domain", monitor.Credentials.Domain)
            Assert.IsTrue(monitor.CreateMissingFolders)
        End Using
    End Sub

    <Test(Description:="Set credentials when they're passed in the Uri")> _
    Public Sub CredentialsFromUri()
        Using monitor As New Pop3Monitor("Pop3Monitor", "pop://claco:mypass@foo.com/bar", New IntervalSchedule, New MockProcessor)
            Assert.AreEqual("pop://foo.com:110/bar", monitor.Uri.ToString)
            Assert.AreEqual("claco", monitor.Credentials.UserName)
            Assert.AreEqual("mypass", monitor.Credentials.Password)
        End Using
    End Sub

    <Test(Description:="Set credentials when they're passed in the Uri")> _
    Public Sub PartialCredentialsFromUri()
        Using monitor As New Pop3Monitor("Pop3Monitor", "pops://claco@foo.com/bar", New IntervalSchedule, New MockProcessor)
            Assert.AreEqual("pops://foo.com:995/bar", monitor.Uri.ToString)
            Assert.AreEqual("claco", monitor.Credentials.UserName)
            Assert.AreEqual(String.Empty, monitor.Credentials.Password)
        End Using
    End Sub
End Class
