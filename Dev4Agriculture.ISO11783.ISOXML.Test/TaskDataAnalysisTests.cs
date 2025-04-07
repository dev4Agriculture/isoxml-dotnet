using System.Linq;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.Analysis;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class TaskDataAnalysisTests
{
    [TestMethod]
    public void CanCalculateTotals()
    {
        var isoxml = ISOXML.Load("./testdata/TimeLogs/TotalsTests");
        //Testing LifeTime Totals
        Assert.IsTrue(isoxml.Data.Task[0].TryGetTotalValue(0x114, 0, out var totalFuelLifeTime, TLGTotalAlgorithmType.LIFETIME));
        Assert.AreEqual(totalFuelLifeTime, 1294 / 0.5);


        //Testing Task Total
        var analysis = new ISODeviceAnalysis(isoxml);
        //We know it's all the same DET in this case, so we only call it once for all DDIs
        var detList = analysis.FindDeviceElementsForDDI(isoxml.Data.Task[0], 0x0078);
        var detId = IdList.ToIntId(detList[0].DeviceElementId);
        Assert.IsTrue(isoxml.Data.Task[0].TryGetTotalValue(0x78, detId, out var totalInEffectiveTime, TLGTotalAlgorithmType.NO_RESETS))
        ;
        Assert.AreEqual(totalInEffectiveTime, 4531 /*Close to 75.5 minutes*/);

        //Testing Task Maximum
        Assert.IsTrue(isoxml.Data.Task[0].TryGetMaximum(0x43, detId, out var maximum));
        Assert.AreEqual(maximum, 12000);


        Assert.IsTrue(isoxml.Data.Task[0].TryGetTotalValue(0xB7, detId, out var totalDryMass, TLGTotalAlgorithmType.NO_RESETS));
        Assert.AreEqual(totalDryMass, 2000);

        var timeElements = isoxml.Data.Task[0].GenerateTimeElementsFromTimeLogs(isoxml.Data.Device);
        Assert.AreEqual(timeElements.Count, 3);
        Assert.AreEqual(timeElements[2].DataLogValue.First(entry => DDIUtils.ConvertDDI(entry.ProcessDataDDI) == (ushort)DDIList.IneffectiveTotalTime).ProcessDataValue, 4531);
        Assert.AreEqual(timeElements[2].DataLogValue.First(entry => DDIUtils.ConvertDDI(entry.ProcessDataDDI) == (ushort)DDIList.LifetimeTotalArea).ProcessDataValue, 561780);
    }

    [TestMethod]
    public void CanReadTaskExtract()
    {
        var isoxml = ISOXML.Load("./testdata/TimeLogs/TotalsTests");
        //Testing LifeTime Totals
        var entries = isoxml.Data.Task[0].GetMergedTaskExtract(0x0043, -1);
        var filledEntries = isoxml.Data.Task[0].GetMergedTaskExtract(0x0043, -1, "", true);
        Assert.AreEqual(entries.Data.Count, 2);
        Assert.AreEqual(filledEntries.Data.Count, 4677);
    }

    [TestMethod]
    public void CanReadDataLogValues()
    {
        var isoxml = ISOXML.Load("./testdata/TimeLogs/TotalsTests");
        //Testing TIM-wise ValueReading.
        //This is found <DLV A="0094" B="1000" C="DET-1"/>
        Assert.IsTrue(isoxml.Data.Task[0].Time[0].TryGetDDIValue(148, -1, out var result));
        Assert.AreEqual(result, 1000);
        Assert.IsFalse(isoxml.Data.Task[0].Time[0].TryGetDDIValue(148, 1, out var result2));
        if(isoxml.Data.Task[0].TryGetBounds(out var bounds))
        {
            Assert.AreEqual(true, (double)bounds.MaxLat < 46.404);
            Assert.AreEqual(true, (double)bounds.MinLat > 46.1198);
            Assert.AreEqual(true, (double)bounds.MaxLong < 9.169);
            Assert.AreEqual(true, (double)bounds.MinLong > 8.94);
        }
    }
}

