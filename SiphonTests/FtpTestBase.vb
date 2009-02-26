Imports System.Configuration
Imports System.IO
Imports System.Net
Imports NUnit.Framework
Imports log4net
Imports LumiSoft.Net.FTP.Server
Imports ChrisLaco.Siphon

Public Class FtpTestBase
    Inherits TestBase

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Private _server As FTP_Server

    <SetUp()> _
    Protected Overrides Sub SetUp()
        MyBase.SetUp()

        Server.StartServer()
    End Sub

    <TearDown()> _
    Protected Overrides Sub TearDown()
        MyBase.TearDown()

        Server.StopServer()
    End Sub

    Protected Overridable Sub Authenticate(ByVal sender As Object, ByVal e As AuthUser_EventArgs)
        e.Validated = True
    End Sub

    Protected Overrides ReadOnly Property Credentials() As System.Net.NetworkCredential
        Get
            Dim userName As String = ConfigurationManager.AppSettings("FtpUserName")
            Dim password As String = ConfigurationManager.AppSettings("FtpPassword")

            Return New NetworkCredential(userName, password)
        End Get
    End Property

    Protected Overridable Sub GetDirInfo(ByVal sender As Object, ByVal e As FileSysEntry_EventArgs)
        Dim parts() As String = e.Name.Split("/")
        Dim filter As String = parts(parts.Length - 2)
        Dim path As String = e.Name.TrimStart("/").Replace(filter & "/", "")
        Dim dir As New DirectoryInfo(IO.Path.Combine(TestDirectory.FullName, path))
        Dim files() As FileInfo = dir.GetFiles(filter)

        For Each file As FileInfo In files
            Dim row As DataRow = e.DirInfo.Tables(0).NewRow()
            row.Item("Name") = file.Name
            row.Item("Date") = file.LastWriteTime
            row.Item("Size") = file.Length
            row.Item("IsDirectory") = False
            e.DirInfo.Tables(0).Rows.Add(row)
        Next
    End Sub

    Protected Overridable Sub GetFile(ByVal sender As Object, ByVal e As FileSysEntry_EventArgs)
        Dim file As String = Path.Combine(TestDirectory.FullName, e.Name.TrimStart("/").TrimEnd("/"))

        e.FileStream = New FileStream(file, FileMode.Open)
    End Sub

    Protected Overridable Sub DeleteFile(ByVal sender As Object, ByVal e As FileSysEntry_EventArgs)
        Dim file As String = Path.Combine(TestDirectory.FullName, e.Name.TrimStart("/").TrimEnd("/"))

        IO.File.Delete(file)
    End Sub

    Protected Overridable Sub RenameFile(ByVal sender As Object, ByVal e As FileSysEntry_EventArgs)
        Dim file As String = Path.Combine(TestDirectory.FullName, e.Name.TrimStart("/").TrimEnd("/"))
        Dim newfile As String = Path.Combine(TestDirectory.FullName, e.NewName.TrimStart("/").TrimEnd("/"))

        IO.File.Move(file, newfile)
    End Sub

    Protected Overridable Sub CreateDir(ByVal sender As Object, ByVal e As FileSysEntry_EventArgs)
        Dim dir As String = Path.Combine(TestDirectory.FullName, e.Name.TrimStart("/").TrimEnd("/"))

        Directory.CreateDirectory(dir)
    End Sub

    Protected Overrides ReadOnly Property Uri() As Uri
        Get
            Return New Uri(ConfigurationManager.AppSettings("FtpUri"))
        End Get
    End Property

    Protected Overridable ReadOnly Property Server() As FTP_Server
        Get
            If _server Is Nothing Then
                _server = New FTP_Server
                _server.LogCommands = True
                _server.Enabled = True
                AddHandler _server.AuthUser, AddressOf Authenticate
                AddHandler _server.GetDirInfo, AddressOf GetDirInfo
                AddHandler _server.GetFile, AddressOf GetFile
                AddHandler _server.DeleteFile, AddressOf DeleteFile
                AddHandler _server.RenameDirFile, AddressOf RenameFile
                AddHandler _server.CreateDir, AddressOf CreateDir
            End If

            Return _server
        End Get
    End Property
End Class
