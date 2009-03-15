''' <summary>
''' Module containing all of the help related commands.
''' </summary>
''' <remarks></remarks>
Module Help

    ''' <summary>
    ''' Displays the available help commands.
    ''' </summary>
    ''' <param name="app">SiphonConsole. The main console instance.</param>
    ''' <param name="args">String. The arguments passed to the help command.</param>
    ''' <remarks></remarks>
    Public Sub Run(ByVal app As SiphonConsole, ByVal args() As String)
        Environment.ExitCode = 1
        Console.WriteLine(" help                            Displays the help (this screen)")
        Console.WriteLine(" process <monitor>               Runs the specified monitor found in app.config")
        Console.WriteLine(" process <endpoint> <monitor>    Runs the specified monitor found at the endpoint name in app.config")
        Console.WriteLine(" process <adminurl> <monitor>    Runs the specified monitor found at the specified url")
    End Sub

End Module
