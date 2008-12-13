Imports System.Configuration
Imports System.IO
Imports System.Net
Imports NUnit.Framework
Imports log4net
Imports LumiSoft.Net.IMAP.Server
Imports ChrisLaco.Siphon

Public Class FtpTestBase
    Inherits TestBase

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Protected Overrides ReadOnly Property Credentials() As System.Net.NetworkCredential
        Get
            Dim userName As String = ConfigurationManager.AppSettings("FtpUserName")
            Dim password As String = ConfigurationManager.AppSettings("FtpPassword")

            Return New NetworkCredential(userName, password)
        End Get
    End Property

    Protected Overrides ReadOnly Property Uri() As Uri
        Get
            Return New Uri(ConfigurationManager.AppSettings("FtpUri"))
        End Get
    End Property
End Class
