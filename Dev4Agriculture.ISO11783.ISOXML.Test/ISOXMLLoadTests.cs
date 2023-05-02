using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
}
