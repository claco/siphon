''' <summary>
''' Actions to take when data processing has failed or completed.
''' </summary>
''' <remarks></remarks>
<Flags()> _
Public Enum DataActions
    ''' <summary>
    ''' No action will be taken.
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
    ''' The data should be renamed.
    ''' </summary>
    ''' <remarks></remarks>
    Rename = 4

    ''' <summary>
    ''' The data should be updated.
    ''' </summary>
    ''' <remarks></remarks>
    Update = 8
End Enum
