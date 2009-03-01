Imports System.Data
Imports System.Data.Common

''' <summary>
''' Interface that defines a database monitoring instance.
''' </summary>
''' <remarks></remarks>
Public Interface IDatabaseMonitor
    Inherits IDataMonitor

    ''' <summary>
    ''' Gets/sets the name of the configuration connection string to use to connect to the database.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Property ConnectionStringName() As String

    ''' <summary>
    ''' Gets/sets the command to run to select new data records.
    ''' </summary>
    ''' <value></value>
    ''' <returns>IDbCommand</returns>
    ''' <remarks></remarks>
    Property SelectCommand() As IDbCommand

    ''' <summary>
    ''' Gets/sets the format string used to create a friendly name representation of a record.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Property NameFormat() As String

    ''' <summary>
    ''' Gets/sets the format string used to create a string representation of a record.
    ''' </summary>
    ''' <value></value>
    ''' <returns>String</returns>
    ''' <remarks></remarks>
    Property RecordFormat() As String
End Interface
