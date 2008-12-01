''' <summary>
''' Interface that defines a remote directory monitoring instance.
''' </summary>
''' <remarks></remarks>
Public Interface IRemoteDirectoryMonitor
    Inherits IDirectoryMonitor

    ''' <summary>
    ''' Gets/sets the path where files that download successfully should be placed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Property DownloadPath() As String

    ''' <summary>
    ''' Gets/sets the uri where files that download successfully should placed.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Uri</returns>
    ''' <remarks></remarks>
    Property DownloadUri() As Uri
End Interface
