Imports System.ServiceModel

''' <summary>
''' The SiphonService remote administration service contract.
''' </summary>
''' <remarks></remarks>
<ServiceContract()> _
Public Interface ISiphonServiceAdministration

    ''' <summary>
    ''' Runs the Process() method of the named monitor instance.
    ''' </summary>
    ''' <param name="name">String. The name of the monitor to process.</param>
    ''' <remarks></remarks>
    <OperationContract()> _
    Sub Process(ByVal name As String)

End Interface
