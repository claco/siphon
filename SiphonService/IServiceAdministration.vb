Imports System.ServiceModel

<ServiceContract()> _
Public Interface ISiphonServiceAdministration

    <OperationContract()> _
    Function Process(ByVal monitor As String) As Boolean

End Interface
