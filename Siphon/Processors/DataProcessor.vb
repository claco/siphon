﻿Imports log4net

Namespace Processors
    ''' <summary>
    ''' Base class for data procesors.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class DataProcessor
        Implements IDataProcessor

        Private Shared ReadOnly Log As ILog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
        Private _disposed As Boolean

        ''' <summary>
        ''' Creates a new DataProcessor instance.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub New()

        End Sub

        ''' <summary>
        ''' Processes new data found by the monitor.
        ''' </summary>
        ''' <param name="data">Object. New data to be processed.</param>
        ''' <remarks>Returns True if the data was processed successfully. Returns False otherwise.</remarks>
        Public MustOverride Function Process(ByVal data As Object) As Boolean Implements IDataProcessor.Process

        ''' <summary>
        ''' Disposes the current schedule instance.
        ''' </summary>
        ''' <param name="disposing">Boolean. True if we're disposing.</param>
        ''' <remarks></remarks>
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not _disposed Then
                If disposing Then

                End If
            End If

            _disposed = True
        End Sub

        ''' <summary>
        ''' Disposes the current schedule instance.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace