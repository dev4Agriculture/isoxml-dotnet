using Dev4Agriculture.ISO11783.ISOXML.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;
[TestClass]
public class AnalysisDeviceTest
{
    [TestMethod]
    public void CanFindDPDs()
    {
        var isoxml = ISOXML.Load("testdata/TimeLogs/ValidTimeLogs");

        var analysis = new ISODeviceAnalysis(isoxml);
        var task = isoxml.Data.Task[0];
        var possibleDeviceElements = analysis.FindDeviceElementsForDDI(task, 119);
        Assert.AreEqual(possibleDeviceElements.Count, 1);
        Assert.AreEqual(possibleDeviceElements[0].Type, DDIValueType.ProcessData);
        Assert.AreEqual(analysis.GetDeviceValuePresentation(possibleDeviceElements[0]).UnitDesignator, "min");

        possibleDeviceElements = analysis.FindDeviceElementsForDDI(task, 179 /*ActualCulturalPractice*/);
        Assert.AreEqual(possibleDeviceElements.Count, 1);
        Assert.AreEqual(possibleDeviceElements[0].DeviceElementId, "DET-123456789");
        Assert.AreEqual(possibleDeviceElements[0].DeviceElementNo(), -123456789);
        Assert.AreEqual(analysis.GetDeviceDataDesignator(possibleDeviceElements[0]), "BearbeitungsArt");

    }


}
