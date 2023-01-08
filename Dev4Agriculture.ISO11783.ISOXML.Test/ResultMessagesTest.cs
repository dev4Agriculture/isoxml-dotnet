using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;
[TestClass]
public class ResultMessagesTest
{
    [TestMethod]
    public void TestMessageDetailsCanBeBuilt()
    {
        var isoxml = ISOXML.Load("./testdata/ResultMessages/BrokenBinFile");
        Assert.AreEqual(isoxml.Messages.Count, 13);
        Assert.AreEqual(isoxml.Messages[0].Details.Length, 2);
        Assert.AreEqual(isoxml.Messages[3].Code, Messaging.ResultMessageCode.FileNotFound);
        Assert.AreEqual(isoxml.Messages[12].Details[0].Value, "TLG00026.XML");
    }


    [TestMethod]
    public void TestMessageTitleWorks()
    {
        var isoxml = ISOXML.Load("./testdata/ResultMessages/BrokenBinFile");
        Assert.AreEqual(isoxml.Messages.Count, 13);
        Assert.AreEqual(isoxml.Messages[1].Title, "FileNotFound");

    }

}
