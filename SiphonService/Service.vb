Imports log4net

''' <summary>
''' Class containing windows service code to run Siphon monitors.
''' </summary>
''' <remarks></remarks>
Public Class SiphonService

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    ''' <summary>
    ''' Code run when the service starts.
    ''' </summary>
    ''' <param name="args"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub OnStart(ByVal args() As String)

    End Sub

    ''' <summary>
    ''' Code run when the service stops.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overrides Sub OnStop()

    End Sub

End Class
