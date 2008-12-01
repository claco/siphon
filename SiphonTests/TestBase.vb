Imports System.IO
Imports NUnit.Framework
Imports log4net
Imports ChrisLaco.Siphon

Public Class TestBase

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Private _processComplete As Boolean
    Private _processFailure As Boolean
    Private _processItem As IDataItem
    Private _testDirectory As DirectoryInfo

    <TestFixtureSetUp()> _
    Protected Overridable Sub TestFixtureSetupUp()
        log4net.Config.XmlConfigurator.Configure()
    End Sub

    <SetUp()> _
    Protected Overridable Sub SetUp()
        Me.ProcessComplete = False
        Me.ProcessFailure = False

        TestDirectory = Nothing
    End Sub

    <TearDown()> _
    Protected Sub TearDown()
        DeleteTestDirectory()
    End Sub

    Protected Property ProcessComplete() As Boolean
        Get
            Return _processComplete
        End Get
        Set(ByVal value As Boolean)
            _processComplete = value
        End Set
    End Property

    Protected Property ProcessFailure() As Boolean
        Get
            Return _processFailure
        End Get
        Set(ByVal value As Boolean)
            _processFailure = value
        End Set
    End Property

    Protected Property ProcessItem() As IDataItem
        Get
            Return _processItem
        End Get
        Set(ByVal value As IDataItem)
            _processItem = value
        End Set
    End Property

    Protected Sub Monitor_ProcessComplete(ByVal sender As Object, ByVal e As ProcessEventArgs)
        Log.Debug("ProcessComplete Event Caught")

        Me.ProcessItem = e.Item
        Me.ProcessComplete = True
        Me.ProcessFailure = False
    End Sub

    Protected Sub Monitor_ProcessFailure(ByVal sender As Object, ByVal e As ProcessEventArgs)
        Log.Debug("ProcessFailure Event Caught")

        Me.ProcessItem = e.Item
        Me.ProcessComplete = False
        Me.ProcessFailure = True
    End Sub

    Protected Overridable Sub CreateTestDirectory()
        TestDirectory = System.IO.Directory.CreateDirectory(Path.Combine(Path.GetTempPath, Path.GetRandomFileName))
    End Sub

    Protected Overridable Property TestDirectory() As DirectoryInfo
        Get
            If _testDirectory Is Nothing Then
                CreateTestDirectory()

                Log.DebugFormat("Created directory {0}", _testDirectory.FullName)
            End If

            Return _testDirectory
        End Get
        Set(ByVal value As DirectoryInfo)
            _testDirectory = value
        End Set
    End Property
 
    Protected Overridable Sub DeleteTestDirectory()
        If _testDirectory IsNot Nothing Then
            TestDirectory.Delete(True)
            TestDirectory = Nothing
        End If
    End Sub

    Protected Overridable Sub CreateFile(ByVal name As String, Optional ByVal content As String = "")
        Using file As FileStream = System.IO.File.Create(Path.Combine(TestDirectory.FullName, name))
            If Not String.IsNullOrEmpty(content) Then
                Dim bytes() As Byte = Text.Encoding.UTF8.GetBytes(content)
                file.Write(bytes, 0, bytes.Length)
            End If

            Log.DebugFormat("Created file {0}", file.Name)
        End Using
    End Sub

    Protected Overridable Sub CreateSuccessFile(Optional ByVal name As String = "SUCCESS")
        CreateFile(name, "SUCCESS")
    End Sub

    Protected Overridable Sub CreateFailureFile(Optional ByVal name As String = "FAILURE")
        CreateFile(name, "FAILURE")
    End Sub

    Protected Overridable Sub CreateExceptionFile(Optional ByVal name As String = "EXCEPTION")
        CreateFile(name, "EXCEPTION")
    End Sub
End Class
