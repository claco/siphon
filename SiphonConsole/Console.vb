Imports System.Collections.ObjectModel
Imports System.Configuration
Imports log4net

''' <summary>
''' Class containing windows console app code to run Siphon monitors manually.
''' </summary>
''' <remarks></remarks>
Public Class SiphonConsole
    Implements IDisposable

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Private _disposed As Boolean = False
    Private _configuration As SiphonConfigurationSection
    Private _monitors As Collection(Of IDataMonitor)

    Private COMMAND_ARG_HELP As String = "help"
    Private COMMAND_ARG_QUESTION As String = "?"
    Private COMMAND_ARG_PROCESS As String = "process"

    ''' <summary>
    ''' Gets the Siphon configuration section.
    ''' </summary>
    ''' <value></value>
    ''' <returns>SiphonConfigurationSection</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Configuration() As SiphonConfigurationSection
        Get
            If _configuration Is Nothing Then
                Log.Debug("Loading configuration")

                _configuration = SiphonConfigurationSection.GetSection
            End If

            Return _configuration
        End Get
    End Property

    ''' <summary>
    ''' Gets the collection ot loaded monitors.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Collection(Of IDataMonitor)</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Monitors() As Collection(Of IDataMonitor)
        Get
            If _monitors Is Nothing Then
                Try
                    Log.Debug("Creating monitors")
                    _monitors = New Collection(Of IDataMonitor)

                    For Each monitor As MonitorElement In Me.Configuration.Monitors
                        Log.DebugFormat("Creating monitor {0}", monitor.Name)

                        _monitors.Add(monitor.CreateInstance)
                    Next
                Catch ex As Exception
                    Log.FatalFormat("Error loading monitors: {0}", ex)
                End Try
            End If

            Return _monitors
        End Get
    End Property

    ''' <summary>
    ''' Class function called when console app starts.
    ''' </summary>
    ''' <param name="args">Array of string arguments passed to the console app.</param>
    ''' <remarks></remarks>
    Public Sub Run(ByVal args() As String)
        If args.Length > 0 Then
            Dim stack As New Stack(Of String)(args.Reverse)
            Dim first As String = stack.Pop.Trim.ToLower.Replace("-", "").Replace("/", "")

            Select Case first
                Case COMMAND_ARG_HELP, COMMAND_ARG_QUESTION
                    Help.Run(Me, stack.ToArray)
                Case COMMAND_ARG_PROCESS
                    Process.Run(Me, stack.ToArray)
                Case Else
                    Help.Run(Me, stack.ToArray)
            End Select
        Else
            Environment.ExitCode = 1

            Log.Error("No arguments were supplied")
        End If
    End Sub

    ''' <summary>
    ''' Main function called when console app starts.
    ''' </summary>
    ''' <param name="args">Array of string arguments passed to the console app.</param>
    ''' <remarks></remarks>
    Shared Sub Main(ByVal args() As String)
        Dim console As New SiphonConsole()

        console.Run(args)
    End Sub

    ''' <summary>
    ''' Disposes the current monitor instance.
    ''' </summary>
    ''' <param name="disposing">Boolean. Flag for if we are disposing or in GC.</param>
    ''' <remarks></remarks>
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not _disposed Then
            _disposed = True

            If disposing Then

            End If
        End If
    End Sub

    ''' <summary>
    ''' Disposes the current monitor instance.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

End Class
