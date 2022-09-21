﻿using System;
using System.Collections.Generic;
using System.Text;
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
            Assert.AreEqual(totalInEffectiveTime, (uint)4531 /*Close to 75.5 minutes*/);

            //Testing Task Maximum
            Assert.IsTrue(isoxml.Data.Task[0].TryGetMaximum(0x43, 0, out uint maximum));
            Assert.AreEqual(maximum, (uint)12000);
        }

    }
}