Imports System.Collections.ObjectModel
Imports System.Collections.Generic
Imports System.ServiceModel
Imports log4net

''' <summary>
''' The SiphonService remote administration service instance.
''' </summary>
''' <remarks></remarks>
<ServiceBehavior(InstanceContextMode:=InstanceContextMode.Single)> _
Public Class SiphonServiceAdministration
    Implements ISiphonServiceAdministration

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Private _service As SiphonService = Nothing

    Public Sub New(ByVal service As SiphonService)
        _service = service
    End Sub

    ''' <summary>
    ''' Gets the SiphonService instance this class belongs to.
    ''' </summary>
    ''' <value></value>
    ''' <returns>SiphonService</returns>
    ''' <remarks></remarks>
    Protected ReadOnly Property Service() As SiphonService
        Get
            Return _service
        End Get
    End Property

    ''' <summary>
    ''' Runs the Process() method of the named monitor instance.
    ''' </summary>
    ''' <param name="name">String. The name of the monitor to process.</param>
    ''' <remarks></remarks>
    Public Sub Process(ByVal name As String) Implements ISiphonServiceAdministration.Process
        Dim context As OperationContext = OperationContext.Current
        Dim properties As Channels.MessageProperties = context.IncomingMessageProperties
        Dim endpoint As Channels.RemoteEndpointMessageProperty = properties(Channels.RemoteEndpointMessageProperty.Name)

        If Me.Service.Monitors.Count > 0 Then
            Try
                Dim query As IEnumerable(Of IDataMonitor) = From monitor In Me.Service.Monitors Where monitor.Name.Trim.ToLower = name.ToLower

                If query.Count = 0 Then
                    Log.ErrorFormat("[{0}] Could not find monitor {1}", endpoint.Address, name)

                    Throw New FaultException(String.Format("Could not find monitor {0}", name))
                Else
                    Log.InfoFormat("[{0}] Running monitor {1}", endpoint.Address, query.First.Name)

                    query.First.Process()
                End If
            Catch ex As Exception
                If TypeOf ex Is FaultException Then
                    Throw
                Else
                    Log.Error(ex)
                    Throw New FaultException(ex.Message)
                End If
            End Try
        Else
            Log.ErrorFormat("[{0}] No monitors are configured", endpoint.Address)

            Throw New FaultException("No monitors are configured")
        End If
    End Sub
End Class
