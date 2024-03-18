using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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
        Assert.AreEqual(isoxml.Messages[12].Details[0].Value, "TLG00026.xml");
    }


    [TestMethod]
    public void TestMessageTitleWorks()
    {
        var isoxml = ISOXML.Load("./testdata/ResultMessages/BrokenBinFile");
        Assert.AreEqual(isoxml.Messages.Count, 13);
        Assert.AreEqual(isoxml.Messages[1].Title, "FileNotFound");

    }

    [TestMethod]
    public void TestMessageSerializationWorks() {
        var isoxml = ISOXML.Load("./testdata/ResultMessages/BrokenBinFile");
        string result = JsonConvert.SerializeObject(isoxml.Messages);
        Assert.IsTrue(Regex.Matches(result,"Code").Count == 13);
    }

}
