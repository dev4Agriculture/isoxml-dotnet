using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class StreamInputTests
{
    [TestMethod]
    public void CanLoadValidZipStreamWithGrids()
    {
        var filePath = "./testdata/LoadFromStream/ZippedTask.zip";
        ISOXML result = null;
        using (var stream = File.OpenRead(filePath))
        {
            result = ISOXML.LoadFromArchive(stream);
        }

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Grids.Count);
        Assert.AreEqual(1, result.Grids["GRD00001"].Layers);
    }

    [TestMethod]
    public void CanLoadValidZipFromSubfolderStreamWithGrids()
    {
        var filePath = "./testdata/LoadFromStream/ZippedTaskWithSubFolder.zip";
        ISOXML result = null;
        using (var stream = File.OpenRead(filePath))
        {
            result = ISOXML.LoadFromArchive(stream);
        }

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Grids.Count);
        Assert.AreEqual(1, result.Grids["GRD00001"].Layers);
    }

    [TestMethod]
    public void LoadFromZipWithMultipleExternalFilesInSubfolder()
    {
        var filePath = "./testdata/LoadFromStream/MultipleExternalsSubFolder.zip";
        ISOXML result = null;
        using (var stream = File.OpenRead(filePath))
        {
            result = ISOXML.LoadFromArchive(stream);
        }

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Data.Farm.Count);
        Assert.AreEqual(3, result.Data.Customer.Count);
        Assert.AreEqual(3, result.Data.Task.Count);
        Assert.AreEqual(0, result.Messages.Count);
    }

    [TestMethod]
    public void LoadFromZipWithMultipleExternalFiles()
    {
        var filePath = "./testdata/LoadFromStream/MultipleExternals.zip";
        ISOXML result = null;
        using (var stream = File.OpenRead(filePath))
        {
            result = ISOXML.LoadFromArchive(stream);
        }

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Data.Farm.Count);
        Assert.AreEqual(3, result.Data.Customer.Count);
        Assert.AreEqual(3, result.Data.Task.Count);
        Assert.AreEqual(0, result.Messages.Count);
    }

    [TestMethod]
    public void LoadZipFileWithMultiplyTaskdatXML()
    {
        var filePath = "./testdata/LoadFromStream/MultiplyTaskdataXML.zip";
        ISOXML result = null;
        using (var stream = File.OpenRead(filePath))
        {
            result = ISOXML.LoadFromArchive(stream);
        }

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Messages.Count);
        Assert.AreEqual(1, result.Grids.Count);
        Assert.AreEqual(1, result.Grids["GRD00001"].Layers);
    }

    [TestMethod]
    public void CanLoadValidZipStreamValidLinkList()
    {
        var filePath = "./testdata/LoadFromStream/ValidLinkList.zip";
        ISOXML result = null;
        using (var stream = File.OpenRead(filePath))
        {
            result = ISOXML.LoadFromArchive(stream);
        }

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(0, result.Messages.Count);
        Assert.IsTrue(result.HasLinkList);
    }
    
    [TestMethod]
    public async Task CanLoadValidZipStreamValidLinkListAsync()
    {
        var filePath = "./testdata/LoadFromStream/ValidLinkList.zip";
        ISOXML result = null;
        using (var stream = File.OpenRead(filePath))
        {
            result = await ISOXML.LoadFromArchiveAsync(stream);
        }

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(0, result.Messages.Count);
        Assert.IsTrue(result.HasLinkList);
    }

    [TestMethod]
    public void ThrowExceptionIfNoTaskDataFile()
    {
        var filePath = "./testdata/LoadFromStream/InvalidArchive.zip";
        using (var stream = File.OpenRead(filePath))
        {
            Assert.ThrowsException<InvalidDataException>(() => ISOXML.LoadFromArchive(stream));
        }
        using (var stream = File.OpenRead(filePath))
        {
            Assert.ThrowsExceptionAsync<InvalidDataException>(async () => await ISOXML.LoadFromArchiveAsync(stream));
        }
    }
}
