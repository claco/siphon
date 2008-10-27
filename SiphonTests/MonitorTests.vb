Imports System.IO
Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="Monitor Tests")> _
Public Class MonitorTests

    <TestFixtureSetUp()> _
    Public Sub TestFixtureSetupUp()
        log4net.Config.XmlConfigurator.Configure()
    End Sub

#Region "Directory Monitor Tests"

    <Test()> _
    Public Sub DirectoryMonitor()
        Dim temp As DirectoryInfo = Directory.CreateDirectory(Path.Combine(Path.GetTempPath, Path.GetRandomFileName))

        Using monitor As IDataMonitor = New DirectoryMonitor(temp.FullName, New IntervalSchedule(5), New MockProcessor)
            monitor.Start()
            Threading.Thread.Sleep(20000)
            monitor.Stop()
        End Using

        temp.Delete()
    End Sub

#End Region

End Class
