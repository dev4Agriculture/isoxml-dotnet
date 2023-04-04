using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class LocalizationLabelTest
{
    [TestMethod]
    public void TestCanReCreateLocalizationLabel()
    {
        var label = "FF000000400000";
        Assert.AreEqual(label, new LocalizationLabel(label).ToString());
        label = "FFAD0D00404545";
        Assert.AreEqual(label, new LocalizationLabel(label).ToString());
        label = "FF000000406465";
        Assert.AreEqual(label, new LocalizationLabel(label).ToString());
        label = "FF000000406565";
        Assert.AreEqual(label, new LocalizationLabel(label).ToString());
        label = "FF0DD00D400000";
        Assert.AreEqual(label, new LocalizationLabel(label).ToString());
        label = "FF0000DA400000";
        Assert.AreEqual(label, new LocalizationLabel(label).ToString());
        label = "FF00080B400000";
        Assert.AreEqual(label, new LocalizationLabel(label).ToString());
        label = "FF000000400800";
        Assert.AreEqual(label, new LocalizationLabel(label).ToString());
        label = "FF000000409000";
        Assert.AreEqual(label, new LocalizationLabel(label).ToString());
        label = "FF000000800100";
        Assert.AreEqual(label, new LocalizationLabel(label).ToString());
        label = "FF000000200000";
        Assert.AreEqual(label, new LocalizationLabel(label).ToString());
    }


    [TestMethod]
    public void InvalidDataThrowsException()
    {
        Assert.ThrowsException<LocalizationLabelInvalidException>(() => new LocalizationLabel(new byte[] { 1, 2, 3 }));
    }

}

