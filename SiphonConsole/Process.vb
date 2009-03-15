Imports System.Collections.ObjectModel
Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports System.ServiceModel
Imports log4net

''' <summary>
''' Class containing the processing commands.
''' </summary>
''' <remarks></remarks>
Module Process

    Private ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Private Const COMMAND_MONITOR_ALL As String = "all"
    Private Const COMMAND_MONITOR_STAR As String = "*"

    ''' <summary>
    ''' Runs a local or remote monitors process command by name.
    ''' </summary>
    ''' <param name="app">SiphonConsole. The main console instance.</param>
    ''' <param name="args">String. The arguments passed to the process command.</param>
    ''' <remarks></remarks>
    Public Sub Run(ByVal app As SiphonConsole, ByVal args() As String)
        If args.Count > 1 Then
            Dim endpoint As String = args(0).Trim
            Dim monitor As String = args(1).Trim

            If Regex.IsMatch(endpoint, "\A(http|\\\\)") Then
                Try
                    Using client As New SiphonServiceAdministration.SiphonServiceAdministrationClient
                        client.Endpoint.Address = New EndpointAddress(endpoint)
                        client.Process(monitor)
                    End Using
                Catch ex As Exception
                    Log.Error(ex.Message)
                    Log.Debug(ex.StackTrace)

                    Environment.ExitCode = 1
                End Try
            Else
                Log.InfoFormat("Running monitor {0} on endpoint {1}", monitor, endpoint)

                Try
                    Using client As New SiphonServiceAdministration.SiphonServiceAdministrationClient(endpoint)
                        client.Process(monitor)
                    End Using
                Catch ex As Exception
                    Log.Error(ex.Message)
                    Log.Debug(ex.StackTrace)

                    Environment.ExitCode = 1
                End Try
            End If
        ElseIf args.Count = 1 Then
            Dim name As String = args(0).Trim

            If name.ToLower = COMMAND_MONITOR_ALL Or name.ToLower = COMMAND_MONITOR_STAR Then
                If app.Monitors.Count > 0 Then
                    For Each monitor As IDataMonitor In app.Monitors
                        Log.InfoFormat("Running monitor {0}", monitor.Name)
                        monitor.Process()
                    Next
                Else
                    Log.Error("No monitors are configured")
                    Environment.ExitCode = 1
                End If
            Else
                If app.Monitors.Count > 0 Then
                    Dim query As IEnumerable(Of IDataMonitor) = From monitor In app.Monitors Where monitor.Name.Trim.ToLower = name.ToLower

                    If query.Count = 0 Then
                        Log.ErrorFormat("Could not find monitor {0}", name)
                        Environment.ExitCode = 1
                    Else
                        Log.InfoFormat("Running monitor {0}", query.First.Name)

                        query.First.Process()
                    End If
                Else
                    Log.Error("No monitors are configured")
                    Environment.ExitCode = 1
                End If
            End If
        Else
            Environment.ExitCode = 1
            Log.Error("No arguments were supplied")
        End If
    End Sub

End Module
