Namespace Monitors
    ''' <summary>
    ''' Interface that defines a directory monitoring instance.
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IDirectoryMonitor
        Inherits IDataMonitor

        ''' <summary>
        ''' Gets/sets the file name filter to apply to the directorys contents
        ''' </summary>
        ''' <value></value>
        ''' <returns>String</returns>
        ''' <remarks>The default should be *, or all files.</remarks>
        Property Filter() As String

        ''' <summary>
        ''' Gets/sets the full path of the directory to monitor.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>The path should take either a local system path or a file uri.</remarks>
        Property Path() As String
    End Interface
End Namespace