Imports System.Messaging
Imports log4net

Public Class MockQueueMessageProcessor
    Inherits MockProcessor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Public Overrides Function Process(ByVal data As Object) As Boolean
        Dim message As Message = data

        MyBase.Process(message.Body)
    End Function
End Class
