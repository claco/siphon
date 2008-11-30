Imports System.Collections.ObjectModel
Imports System.Configuration
Imports log4net

''' <summary>
''' Class containing windows service code to run Siphon monitors.
''' </summary>
''' <remarks></remarks>
Public Class SiphonService

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private _configuration As SiphonConfigurationSection
    Private _monitors As Collection(Of IDataMonitor)

    ''' <summary>
    ''' Gets the Siphon configuration section.
    ''' </summary>
    ''' <value></value>
    ''' <returns>ConfigurationSection</returns>
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
    ''' Code run when the service starts.
    ''' </summary>
    ''' <param name="args"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub OnStart(ByVal args() As String)
        Log.Info("Starting Siphon Service")

        For Each monitor As IDataMonitor In Me.Monitors
            Log.InfoFormat("Starting monitor {0}", monitor.Name)

            Try
                monitor.Start()
            Catch ex As Exception
                Log.Error(String.Format("Error starting monitor {0}", monitor.Name), ex)
            End Try
        Next
    End Sub

    ''' <summary>
    ''' Code run when the service is paused.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overrides Sub OnPause()
        Log.Info("Pausing Siphon Service")

        For Each monitor As IDataMonitor In Me.Monitors
            Log.InfoFormat("Pausing monitor {0}", monitor.Name)

            monitor.Pause()
        Next
    End Sub

    ''' <summary>
    ''' Code run when the service continues.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overrides Sub OnContinue()
        Log.Info("Resuming Siphon Service")

        For Each monitor As IDataMonitor In Me.Monitors
            Log.InfoFormat("Resuming monitor {0}", monitor.Name)

            Try
                monitor.Resume()
            Catch ex As Exception
                Log.Error(String.Format("Error resuming monitor {0}", monitor.Name), ex)
            End Try
        Next
    End Sub

    ''' <summary>
    ''' Code run when the service stops.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overrides Sub OnStop()
        Log.Info("Stopping Siphon Service")

        For Each monitor As IDataMonitor In Me.Monitors
            Log.InfoFormat("Stopping monitor {0}", monitor.Name)

            monitor.Stop()
        Next
    End Sub

End Class
