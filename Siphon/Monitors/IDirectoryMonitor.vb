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

    ''' <summary>
    ''' Gets/sets the full path as a uri of the directory to monitor.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Uri</returns>
    ''' <remarks></remarks>
    Property Uri() As Uri

    ''' <summary>
    ''' Gets/sets the path where files that fail processing should be moved to if the DataActions.Move action is set. 
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Property FailurePath() As String

    ''' <summary>
    ''' Gets/sets the uri where files that fail processing should be moved to if the DataActions.Move action is set. 
    ''' </summary>
    ''' <value></value>
    ''' <returns>Uri</returns>
    ''' <remarks></remarks>
    Property FailureUri() As Uri

    ''' <summary>
    ''' Gets/sets the path where files that process successfully should be moved to if the DataActions.Move aciton is set.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Property CompletePath() As String

    ''' <summary>
    ''' Gets/sets the uri where files that process successfully should be moved to if the DataActions.Move aciton is set.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Uri</returns>
    ''' <remarks></remarks>
    Property CompleteUri() As Uri
End Interface
