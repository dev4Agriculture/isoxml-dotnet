using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class StreamInputTests
{
    [TestMethod]
    public void CanLoadValidZipStreamWithGrids()
    {
        var filePath = "./testdata/LoadFromStream/Zipped_Task.zip";
        ISOXML result = null;
        using (var stream = File.OpenRead(filePath))
        {
            result = ISOXML.Load(stream);
        }

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
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
            result = ISOXML.Load(stream);
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
        ISOXML result = null;
        using (var stream = File.OpenRead(filePath))
        {
            Assert.ThrowsException<InvalidDataException>(() => ISOXML.Load(stream));
        }
    }
}
