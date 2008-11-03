Imports log4net
Imports ChrisLaco.Siphon.Processors

Public MustInherit Class MockProcessor
    Inherits DataProcessor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Public Count As Integer
    Public Delay As Integer

    Public Sub Reset()
        Me.Count = 0
    End Sub

    Public Overrides Function Process(ByVal data As Object) As Boolean
        Log.DebugFormat("Process {0}", data.ToString)
        Me.Count += 1

        Log.DebugFormat("Process Start Delay {0}", Me.Delay)
        Threading.Thread.Sleep(Me.Delay * 1000)
        Log.DebugFormat("Process Finished Delay {0}", Me.Delay)

        Select Case data.ToString.ToUpper
            Case "SUCCESS"
                Return True
            Case "FAILURE"
                Return False
            Case "EXCEPTION"
                Throw New Exception
        End Select
    End Function
End Class
