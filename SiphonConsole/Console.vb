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
                Log.Debug("Creating monitors")

                _monitors = New Collection(Of IDataMonitor)

                For Each monitor As MonitorElement In Me.Configuration.Monitors
                    Log.InfoFormat("Creating monitor {0}", monitor.Name)

                    _monitors.Add(monitor.CreateInstance)
                Next
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
            Dim first As String = args(0).Trim.ToLower

            If first = "all" Or first = "*" Then
                Log.Info("Running all monitors")

                For Each monitor As IDataMonitor In Me.Monitors
                    monitor.Process()
                Next
            Else
                For Each arg As String In args
                    Dim name As String = arg.Trim
                    Dim query = From monitor In Me.Monitors Where monitor.Name = name

                    If query.Count = 0 Then
                        Log.ErrorFormat("Could not find monitor {0}", name)
                    Else
                        Log.InfoFormat("Running monitor {0}", query.First.Name)

                        query.First.Process()
                    End If
                Next
            End If

            Environment.ExitCode = 0
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
