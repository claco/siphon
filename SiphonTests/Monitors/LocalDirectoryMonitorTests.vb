Imports System.Configuration
Imports System.IO
Imports System.Messaging
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Local Directory Monitor Tests")> _
    Public Class LocalDirectoryMonitorTests
    Inherits TestBase

    <Test(Description:="Path throws exception when empty")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub DirectoryMonitorPathException()
        Dim monitor As New LocalDirectoryMonitor("NewMonitor", String.Empty, New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Name throws exception when empty")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub DirectoryMonitorNameException()
        Dim monitor As New LocalDirectoryMonitor(String.Empty, "C:\temp", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Test successful directory monitor")> _
    Public Sub DirectoryMonitor()
        CreateSuccessFile()

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
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

    <Test(Description:="Test successful directory monitor process complete deletes file")> _
    Public Sub DirectoryMonitorProcessorCompleteDeleteFile()
        CreateSuccessFile("SUCCESS")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
                    AddHandler monitor.ProcessComplete, AddressOf Monitor_ProcessComplete
                    AddHandler monitor.ProcessFailure, AddressOf Monitor_ProcessFailure

                    Assert.IsTrue(File.Exists(Path.Combine(TestDirectory.FullName, "SUCCESS")))
                    monitor.ProcessCompleteActions = DataActions.Delete
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
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
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
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
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
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
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
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
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
                Using monitor As IDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
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
                Using monitor As IDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
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
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
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
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
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
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
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
        Dim tempdir As String = Path.Combine(Path.GetTempPath, Path.GetRandomFileName)

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(1).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", tempdir, schedule, processor)
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
        Dim tempdir As String = Path.Combine(Path.Combine(Path.GetTempPath, Path.GetRandomFileName), "New")

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(1).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", tempdir, schedule, processor)
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
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
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
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", TestDirectory.FullName, schedule, processor)
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
    Public Sub LocalDirectoryPaths()
        Using monitor As New LocalDirectoryMonitor("LocalDirectoryMonitor", "C:\", New IntervalSchedule, New MockProcessor)
            Assert.AreEqual("C:\", monitor.Path)
            Assert.AreEqual("file:///C:/", monitor.Uri.ToString)

            REM path gets uri
            monitor.Path = "file:///D:/"
            Assert.AreEqual("D:\", monitor.Path)
            Assert.AreEqual("file:///D:/", monitor.Uri.ToString)

            REM uri gets uri
            monitor.Uri = New Uri("file:///C:/")
            Assert.AreEqual("C:\", monitor.Path)
            Assert.AreEqual("file:///C:/", monitor.Uri.ToString)

            REM blank defaults
            Assert.IsNull(monitor.CompleteUri)
            Assert.IsNull(monitor.FailureUri)
            Assert.IsTrue(String.IsNullOrEmpty(monitor.CompletePath))
            Assert.IsTrue(String.IsNullOrEmpty(monitor.FailurePath))

            REM Relative paths
            monitor.Path = "C:\"
            monitor.CompletePath = "Processed"
            Assert.AreEqual("C:\Processed", monitor.CompletePath)
            Assert.AreEqual("file:///C:/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "Failed"
            Assert.AreEqual("C:\Failed", monitor.FailurePath)
            Assert.AreEqual("file:///C:/Failed", monitor.FailureUri.ToString)

            REM Drive paths
            monitor.CompletePath = "D:\Processed"
            Assert.AreEqual("D:\Processed", monitor.CompletePath)
            Assert.AreEqual("file:///D:/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "D:\Failed"
            Assert.AreEqual("D:\Failed", monitor.FailurePath)
            Assert.AreEqual("file:///D:/Failed", monitor.FailureUri.ToString)

            REM Parent Paths
            monitor.Path = "C:\Parent"
            monitor.CompletePath = "../Processed"
            Assert.AreEqual("C:\Processed", monitor.CompletePath)
            Assert.AreEqual("file:///C:/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "../Failed"
            Assert.AreEqual("C:\Failed", monitor.FailurePath)
            Assert.AreEqual("file:///C:/Failed", monitor.FailureUri.ToString)
            monitor.CompletePath = "..\Processed"
            Assert.AreEqual("C:\Processed", monitor.CompletePath)
            Assert.AreEqual("file:///C:/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "..\Failed"
            Assert.AreEqual("C:\Failed", monitor.FailurePath)
            Assert.AreEqual("file:///C:/Failed", monitor.FailureUri.ToString)

            REM Absolute Paths: Drive letter retained from main Path/Uri
            monitor.Path = "C:\Top"
            monitor.CompletePath = "/Processed"
            Assert.AreEqual("C:\Processed", monitor.CompletePath)
            Assert.AreEqual("file:///C:/Processed", monitor.CompleteUri.ToString)
            monitor.FailurePath = "/Failed"
            Assert.AreEqual("C:\Failed", monitor.FailurePath)
            Assert.AreEqual("file:///C:/Failed", monitor.FailureUri.ToString)

            REM complete/failure uri paths
            monitor.CompleteUri = New Uri("file:///D:/Foo")
            Assert.AreEqual("D:\Foo", monitor.CompletePath)
            Assert.AreEqual("file:///D:/Foo", monitor.CompleteUri.ToString)
            monitor.FailureUri = New Uri("file:///D:/Foo")
            Assert.AreEqual("D:\Foo", monitor.FailurePath)
            Assert.AreEqual("file:///D:/Foo", monitor.FailureUri.ToString)

            REM complete/failure uri to paths
            monitor.CompletePath = New Uri("file:///D:/Foo").ToString
            Assert.AreEqual("D:\Foo", monitor.CompletePath)
            Assert.AreEqual("file:///D:/Foo", monitor.CompleteUri.ToString)
            monitor.FailurePath = New Uri("file:///D:/Foo").ToString
            Assert.AreEqual("D:\Foo", monitor.FailurePath)
            Assert.AreEqual("file:///D:/Foo", monitor.FailureUri.ToString)

            REM rooted uri
            monitor.CompleteUri = New Uri("file:///Foo")
            Assert.AreEqual("C:\Foo", monitor.CompletePath)
            Assert.AreEqual("file:///C:/Foo", monitor.CompleteUri.ToString)
            monitor.FailureUri = New Uri("file:///Foo")
            Assert.AreEqual("C:\Foo", monitor.FailurePath)
            Assert.AreEqual("file:///C:/Foo", monitor.FailureUri.ToString)

        End Using
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub UriUnsupportedException()
        Dim monitor As New LocalDirectoryMonitor("TestName", "gopher://foo.com/bar", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub FailureUriUnsupportedException()
        Dim monitor As New LocalDirectoryMonitor("TestName", "C:\", New IntervalSchedule, New MockProcessor)

        monitor.FailureUri = New Uri("gopher://foo.bar/")
    End Sub

    <Test(Description:="Unsupported Uri Scheme Exception")> _
    <ExpectedException(GetType(UriFormatException))> _
    Public Sub CompleteUriUnsupportedException()
        Dim monitor As New LocalDirectoryMonitor("TestName", "C:\", New IntervalSchedule, New MockProcessor)
        Dim uri = New Uri("gopher://foo.bar/")

        monitor.CompleteUri = uri
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub PathEmptyArgumentException()
        Dim monitor As New LocalDirectoryMonitor("TestName", "", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Argument Exception")> _
    <ExpectedException(GetType(ArgumentException))> _
    Public Sub NameEmptyArgumentException()
        Dim monitor As New LocalDirectoryMonitor("", "C:\", New IntervalSchedule, New MockProcessor)
    End Sub

    <Test(Description:="Path returns empty with no uri")> _
    Public Sub NoUriReturnsEmpty()
        Dim monitor As LocalDirectoryMonitor = GetType(LocalDirectoryMonitor).Assembly.CreateInstance(GetType(LocalDirectoryMonitor).FullName, False, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)

        Assert.IsTrue(String.IsNullOrEmpty(monitor.Path))
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullUriException()
        Using monitor As New LocalDirectoryMonitor("TestMonitor", "C:\", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As LocalDirectoryMonitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.LocalDirectoryMonitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.Name = "Test"
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullNameException()
        Using monitor As New LocalDirectoryMonitor("TestMonitor", "C:\", New IntervalSchedule, New MockProcessor)
            Dim newMonitor As LocalDirectoryMonitor = monitor.GetType.Assembly.CreateInstance("ChrisLaco.Siphon.LocalDirectoryMonitor", True, BindingFlags.CreateInstance Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic, Nothing, Nothing, Nothing, Nothing)
            newMonitor.Path = "C:\"
            newMonitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullCompleteUriException()
        Using monitor As New LocalDirectoryMonitor("TestMonitor", "C:\", New IntervalSchedule, New MockProcessor)
            monitor.ProcessCompleteActions = DataActions.Move
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Start throws exception for null Uri/Path")> _
    <ExpectedException(GetType(ApplicationException))> _
    Public Sub StartFailureNullFailureUriException()
        Using monitor As New LocalDirectoryMonitor("TestMonitor", "C:\", New IntervalSchedule, New MockProcessor)
            monitor.ProcessFailureActions = DataActions.Move
            monitor.Start()
        End Using
    End Sub

    <Test(Description:="Create monitor from configuration")> _
    Public Sub CreateFromConfiguration()
        Dim monitorElement As New MonitorElement("TestQueueMonitor", "ChrisLaco.Siphon.LocalDirectoryMonitor, Siphon")
        Dim processorElement As New ProcessorElement("ChrisLaco.Tests.Siphon.MockProcessor, SiphonTests")
        Dim scheduleElement As New ScheduleElement("ChrisLaco.Siphon.DailySchedule, Siphon")
        scheduleElement.Daily.Add(DateTime.Now.AddSeconds(3).TimeOfDay)
        monitorElement.Schedule = scheduleElement
        monitorElement.Processor = processorElement
        monitorElement.Settings.Add(New NameValueConfigurationElement("Path", "C:\temp"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("CompletePath", "Processed"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("FailurePath", "Failed"))
        monitorElement.Settings.Add(New NameValueConfigurationElement("CreateMissingFolders", "true"))

        Using monitor As LocalDirectoryMonitor = monitorElement.CreateInstance
            Assert.IsInstanceOfType(GetType(LocalDirectoryMonitor), monitor)
            Assert.IsInstanceOfType(GetType(Uri), monitor.Uri)
            Assert.IsInstanceOfType(GetType(Uri), monitor.CompleteUri)
            Assert.IsInstanceOfType(GetType(Uri), monitor.FailureUri)
            Assert.AreEqual("C:\temp", monitor.Path)
            Assert.AreEqual("C:\temp\Processed", monitor.CompletePath)
            Assert.AreEqual("C:\temp\Failed", monitor.FailurePath)
            Assert.IsTrue(monitor.CreateMissingFolders)
        End Using
    End Sub
End Class
