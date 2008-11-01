Imports System.IO
Imports log4net

Public Class MockFileProcessor
    Inherits MockProcessor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Public Overrides Function Process(ByVal data As Object) As Boolean
        Dim info As FileInfo = New FileInfo(data.ToString)

        MyBase.Process(info.Name)
    End Function
End Class
