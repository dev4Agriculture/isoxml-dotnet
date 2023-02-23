using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        var Analysis = new ISODeviceAnalysis(isoxml);
        var task = isoxml.Data.Task[0];
        var possibleDeviceElements = Analysis.FindDeviceElementsForDDI(task, 119);
        Assert.AreEqual(possibleDeviceElements.Count, 1);
        Assert.AreEqual(possibleDeviceElements[0].Type, DDIValueType.ProcessData);

        possibleDeviceElements = Analysis.FindDeviceElementsForDDI(task, 179 /*ActualCulturalPractice*/);
        Assert.AreEqual(possibleDeviceElements.Count, 1);
        Assert.AreEqual(possibleDeviceElements[0].DeviceElement, "DET-2");

    }


}
