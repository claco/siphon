Imports System.IO
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Monitor Tests")> _
Public Class MonitorTests

    <TestFixtureSetUp()> _
    Public Sub TestFixtureSetupUp()
        log4net.Config.XmlConfigurator.Configure()
    End Sub

#Region "Local Directory Monitor Tests"

    <Test(Description:="Test successful directory monitor")> _
    Public Sub DirectoryMonitor()
        Dim tempdir As DirectoryInfo = Directory.CreateDirectory(Path.Combine(Path.GetTempPath, Path.GetRandomFileName))
        File.Create(Path.Combine(tempdir.FullName, "SUCCESS"))

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDataMonitor = New LocalDirectoryMonitor("LocalMonitor", tempdir.FullName, schedule, processor)
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor failure")> _
    Public Sub DirectoryMonitorProcessorFailure()
        Dim tempdir As DirectoryInfo = Directory.CreateDirectory(Path.Combine(Path.GetTempPath, Path.GetRandomFileName))
        File.Create(Path.Combine(tempdir.FullName, "FAILURE"))

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDataMonitor = New LocalDirectoryMonitor("LocalMonitor", tempdir.FullName, schedule, processor)
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with processor exception")> _
    Public Sub DirectoryMonitorProcessorException()
        Dim tempdir As DirectoryInfo = Directory.CreateDirectory(Path.Combine(Path.GetTempPath, Path.GetRandomFileName))
        File.Create(Path.Combine(tempdir.FullName, "EXCEPTION"))

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDataMonitor = New LocalDirectoryMonitor("LocalMonitor", tempdir.FullName, schedule, processor)
                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 file")
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor create missing root directory")> _
    Public Sub DirectoryMonitorCreateDirectory()
        Dim tempdir As String = Path.Combine(Path.GetTempPath, Path.GetRandomFileName)

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(1).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDataMonitor = New LocalDirectoryMonitor("LocalMonitor", tempdir, schedule, processor)
                    Assert.IsFalse(Directory.Exists(tempdir), "Monitor path doesn't exist")

                    DirectCast(monitor, LocalDirectoryMonitor).CreateMissingFolders = True
                    monitor.Start()
                    monitor.Stop()

                    Assert.IsTrue(Directory.Exists(tempdir), "Monitor path exista")
                End Using
            End Using
        End Using
    End Sub

    <Test(Description:="Test directory monitor with filter")> _
    Public Sub DirectoryMonitorWithFilter()
        Dim tempdir As DirectoryInfo = Directory.CreateDirectory(Path.Combine(Path.GetTempPath, Path.GetRandomFileName))
        File.Create(Path.Combine(tempdir.FullName, "SUCCESS"))
        File.Create(Path.Combine(tempdir.FullName, "test.csv"))

        Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
            Using processor = New MockProcessor
                Using monitor As IDataMonitor = New LocalDirectoryMonitor("LocalMonitor", tempdir.FullName, schedule, processor)
                    DirectCast(monitor, LocalDirectoryMonitor).Filter = "*.csv"

                    monitor.Start()
                    Threading.Thread.Sleep(3000)
                    monitor.Stop()

                    Assert.AreEqual(1, processor.Count, "Has processed 1 files")
                End Using
            End Using
        End Using
    End Sub


    <Test(Description:="Test directory monitor with slow processor on stop")> _
    Public Sub DirectoryMonitorStillProcessing()
        Dim tempdir As DirectoryInfo = Directory.CreateDirectory(Path.Combine(Path.GetTempPath, Path.GetRandomFileName))
        File.Create(Path.Combine(tempdir.FullName, "SUCCESS"))

        'Using schedule = New DailySchedule(DateTime.Now.AddSeconds(2).TimeOfDay)
        Using schedule = New IntervalSchedule(2)
            Using processor = New MockProcessor
                Using monitor As LocalDirectoryMonitor = New LocalDirectoryMonitor("LocalMonitor", tempdir.FullName, schedule, processor)
                    processor.DelayProcess = 10
                    monitor.Start()
                    Threading.Thread.Sleep(3000)

                    Assert.IsTrue(monitor.Processing, "Processing is true when a worker processor is still running")
                    Dim pre As DateTime = DateTime.Now
                    monitor.Stop()
                    Dim post As DateTime = DateTime.Now

                    Assert.AreEqual(1, processor.Count, "Has processed 1 files")
                    Assert.GreaterOrEqual((post - pre).TotalSeconds, 5, "Waited for still running process to finish")
                End Using
            End Using
        End Using
    End Sub
#End Region

End Class
