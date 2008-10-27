Imports log4net
Imports ChrisLaco.Siphon

Public Class MockProcessor
    Inherits DataProcessor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Public Overrides Function Process(ByVal data As Object) As Boolean
        Log.Debug("MockProcessor.Process")
    End Function
End Class
