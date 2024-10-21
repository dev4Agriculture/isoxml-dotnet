using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;
[TestClass]
public class ISOXMLLoadTests
{
    [TestMethod]
    public void LoadingInvalidISOXMLDoesNotCrash()
    {
        var isoxml = ISOXML.Load("./testdata/Structure/InvalidXML/");
        Assert.AreEqual(isoxml.Data, null);
    }

    [TestMethod]
    public void LoadingAZipWithMacOSFilesSucceedsAndCreatesAWarning()
    {
        var isoxml = ISOXML.Load("./testdata/Structure/MacOSExtensionFiles");
        Assert.AreEqual(isoxml.Messages.Count(entry => entry.Code == ResultMessageCode.FileNameEndingMultipleTimes), 3);
    }
}
