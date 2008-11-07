Imports System.Messaging
Imports log4net
Imports ChrisLaco.Siphon.Configuration

Namespace Processors
    Public Class MockQueueMessageProcessor
        Inherits MockProcessor

        Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

        Public Overrides Function Process(ByVal data As Object) As Boolean
            Dim message As Message = data

            MyBase.Process(message.Body)
        End Function

        Public Overrides Sub Initialize(ByVal config As ProcessorElement)

        End Sub
    End Class
End Namespace