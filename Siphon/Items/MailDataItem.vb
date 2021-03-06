﻿Imports System.Text.RegularExpressions
Imports System.Text.Encoding
Imports System.IO
Imports log4net
Imports LumiSoft.Net.MIME

''' <summary>
''' Class to reference remote uri-based mail (imap/pop3) maessages.
''' </summary>
''' <remarks></remarks>
Public Class MailDataItem
    Inherits UriDataItem

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    ''' <summary>
    ''' Creates a new MailDataItem instance.
    ''' </summary>
    ''' <param name="path">String. The full uri to the location of the message being processed.</param>
    ''' <param name="uid">String. The uid of the message being processed.</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal path As Uri, ByVal uid As String)
        MyBase.New(New Uri(String.Format("{0}/;UID={1}", path, uid)))
    End Sub

    ''' <summary>
    ''' Gets the contents of the item being processed.
    ''' </summary>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overrides Function GetString() As String
        Try
            Using stream As FileStream = Me.LocalFile.OpenRead
                Dim bytes(stream.Length - 1) As Byte

                stream.Read(bytes, 0, stream.Length)

                Dim message As Mime = Mime.Parse(bytes)

                If Not String.IsNullOrEmpty(message.BodyText) Then
                    GetString = message.BodyText.Trim
                ElseIf Not String.IsNullOrEmpty(message.BodyHtml) Then
                    GetString = message.BodyHtml.Trim
                Else
                    GetString = UTF8.GetString(bytes).Trim
                End If
            End Using
        Catch
            Using reader As StreamReader = Me.LocalFile.OpenText
                GetString = reader.ReadToEnd.Trim
            End Using
        End Try
    End Function

    ''' <summary>
    ''' Gets the message UID from the items current uri.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property UID() As String
        Get
            Dim match As Match = Regex.Match(Me.Data.AbsoluteUri, ";UID=(?<uid>.*);?\/?", RegexOptions.IgnoreCase)

            Return match.Groups("uid").Value
        End Get
    End Property
End Class
