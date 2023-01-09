using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test
{
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
            Assert.IsTrue(isoxml.Data.Task[0].TryGetTotalValue(0x78, 0, out var totalInEffectiveTime, TLGTotalAlgorithmType.NO_RESETS));
            Assert.AreEqual(totalInEffectiveTime, 4531 /*Close to 75.5 minutes*/);

            //Testing Task Maximum
            Assert.IsTrue(isoxml.Data.Task[0].TryGetMaximum(0x43, 0, out var maximum));
            Assert.AreEqual(maximum, 12000);
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
    }
}

