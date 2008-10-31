Imports System.IO
Imports log4net
Imports ChrisLaco.Siphon

Public Class MockProcessor
    Inherits DataProcessor

    Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)

    Private _count As Integer = 0
    Private _delayProcess As Integer = 0

    Public Overrides Function Process(ByVal data As Object) As Boolean
        Log.DebugFormat("MockProcessor.Process {0}", data.ToString)
        _count += 1

        Dim info As FileInfo = New FileInfo(data.ToString)

        Log.DebugFormat("MockProccessor Process Start Delay {0}", Me.DelayProcess)
        Threading.Thread.Sleep(Me.DelayProcess * 1000)
        Log.DebugFormat("MockProccessor Process Finished Delay {0}", Me.DelayProcess)

        Select Case info.Name.ToUpper
            Case "SUCCESS"
                Return True
            Case "FAILURE"
                Return False
            Case "EXCEPTION"
                Throw New Exception
        End Select
    End Function

    Public ReadOnly Property Count() As Integer
        Get
            Return _count
        End Get
    End Property

    Public Property DelayProcess() As Integer
        Get
            Return _delayProcess
        End Get
        Set(ByVal value As Integer)
            _delayProcess = value
        End Set
    End Property

    Public Sub Reset()
        _count = 0
    End Sub
End Class
