Namespace Monitors

    ''' <summary>
    ''' Actions to take when data processing has completed.
    ''' </summary>
    ''' <remarks></remarks>
    <Flags()> _
    Public Enum DataActions
        ''' <summary>
        ''' None action will be taken.
        ''' </summary>
        ''' <remarks></remarks>
        None = 0

        ''' <summary>
        ''' The data should be deleted.
        ''' </summary>
        ''' <remarks></remarks>
        Delete = 1

        ''' <summary>
        ''' The data should be moved.
        ''' </summary>
        ''' <remarks></remarks>
        Move = 2

        ''' <summary>
        ''' The data should be renamed
        ''' </summary>
        ''' <remarks></remarks>
        Rename = 4
    End Enum
End Namespace