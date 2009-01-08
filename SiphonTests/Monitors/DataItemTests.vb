Imports NUnit.Framework
Imports ChrisLaco.Siphon

<TestFixture(Description:="DataItem Tests")> _
Public Class DataItemsTests

    <Test(Description:="Generic String Data Item")> _
    Public Sub StringDataItem()
        Dim item As IDataItem(Of String) = New DataItem(Of String)("Name", "Data")

        Assert.AreEqual("Name", item.Name)
        Assert.AreEqual("Data", item.GetString)
        Assert.AreEqual("Data", item.Data)
        Assert.AreEqual(DataItemStatus.New, item.Status)

        item.Status = DataItemStatus.CompletedProcessing
        Assert.AreEqual(DataItemStatus.CompletedProcessing, item.Status)
    End Sub

End Class
