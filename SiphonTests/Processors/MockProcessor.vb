Imports log4net
Imports ChrisLaco.Siphon

Public Class MockProcessor
    Inherits DataProcessor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    Public Count As Integer
    Public Delay As Integer

    Public Sub Reset()
        Me.Count = 0
    End Sub

    Public Overrides Function Process(ByVal data As IDataItem) As Boolean
        Log.DebugFormat("Process {0}", data.Name)
        Me.Count += 1

        Log.DebugFormat("Process Start Delay {0}", Me.Delay)
        Threading.Thread.Sleep(Me.Delay * 1000)
        Log.DebugFormat("Process Finished Delay {0}", Me.Delay)

        Select Case data.GetString.ToUpper
            Case "SUCCESS"
                Return True
            Case "FAILURE"
                Return False
            Case "EXCEPTION"
                Throw New Exception("Manually invoked exception")
            Case Else
                Throw New Exception(String.Format("Unknown contents: {0}", data.GetString))
        End Select
    End Function

    Public Overrides Sub Initialize(ByVal config As ProcessorElement)

    End Sub
End Class
