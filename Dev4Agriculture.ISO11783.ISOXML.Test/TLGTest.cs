using System;
using System.Globalization;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test
{

    [TestClass]
    public class TLGTest
    {
        [TestMethod]
        public void CanLoadValidTimeLogs()
        {
            var isoxml = ISOXML.Load("./testdata/TimeLogs/ValidTimeLogs");

            Assert.AreEqual(isoxml.TimeLogs.Count, 21);
            Assert.AreEqual(isoxml.Messages.Count, 0);
        }


        [TestMethod]
        public void FindsErrorsInInvalidTimeLogs()
        {
            //In this folder, TLG00003 has an Invalid XML Structure
            var isoxml = ISOXML.Load("./testdata/TimeLogs/BrokenTimeLogs");

            Assert.AreEqual(isoxml.CountValidTimeLogs(), 20);
            Assert.AreEqual(isoxml.Messages.Count, 1);
        }

        [TestMethod]
        public void FindsErrorWithMissingTimeLogs()
        {
            //In this Folder, TLG00003.BIN, TLG00004.BIN, TLG00005.BIN, TLG00006.BIN are missing
            var isoxml = ISOXML.Load("./testdata/TimeLogs/MissingTimeLogs");

            Assert.AreEqual(isoxml.CountValidTimeLogs(), 17);
            Assert.IsTrue(isoxml.TimeLogs.TryGetValue("TLG00003", out var tlg));
            Assert.AreEqual(tlg.Loaded, TLGStatus.ERROR);
            Assert.IsTrue(isoxml.TimeLogs.TryGetValue("TLG00004", out tlg));
            Assert.AreEqual(tlg.Loaded, TLGStatus.ERROR);
            Assert.IsTrue(isoxml.TimeLogs.TryGetValue("TLG00005", out tlg));
            Assert.AreEqual(tlg.Loaded, TLGStatus.ERROR);
            Assert.IsTrue(isoxml.TimeLogs.TryGetValue("TLG00006", out tlg));
            Assert.AreEqual(tlg.Loaded, TLGStatus.ERROR);
            Assert.IsTrue(isoxml.TimeLogs.TryGetValue("TLG00015", out tlg));
            Assert.AreEqual(tlg.Loaded, TLGStatus.LOADED);
            Assert.IsTrue(isoxml.TimeLogs.TryGetValue("TLG00016", out tlg));
            Assert.AreEqual(tlg.Loaded, TLGStatus.LOADED);
            Assert.IsTrue(isoxml.TimeLogs.TryGetValue("TLG00017", out tlg));
            Assert.AreEqual(tlg.Loaded, TLGStatus.LOADED);
            Assert.IsTrue(isoxml.TimeLogs.TryGetValue("TLG00018", out tlg));
            Assert.AreEqual(tlg.Loaded, TLGStatus.LOADED);
            Assert.AreEqual(isoxml.Messages.Count, 4);

            Assert.IsTrue(isoxml.TimeLogs.TryGetValue(isoxml.Data.Task[0].TimeLog[0].Filename, out tlg));
            Assert.AreEqual(tlg.Entries.Count, 4716);
            Assert.AreEqual(tlg.Header.MaximumNumberOfEntries, 26);
            var dateString = DateUtilities.GetDateFromDaysSince1980(tlg.Entries[1].Date);
            Console.WriteLine("Date is " + dateString);
            var date = DateTime.ParseExact(dateString, DateUtilities.DATE_FORMAT, CultureInfo.InvariantCulture);
            Assert.AreEqual(date.Year, 2020);
            Assert.AreEqual(date.Day, 2);
            Assert.AreEqual(date.Month, 1);
        }


        [TestMethod]
        public void CanReadTimeLogContents()
        {
            var isoxml = ISOXML.Load("./testdata/TimeLogs/ValidTimeLogs");


            Assert.IsTrue(isoxml.TimeLogs.TryGetValue("TLG00001", out var tlg));
            Assert.IsTrue(tlg.Header.HasDDI(141));
            Assert.IsFalse(tlg.Header.HasDDI(1000));
            Assert.IsTrue(isoxml.TimeLogs.TryGetValue("TLG00001", out var timeLog));
            var extract = ISOTLGExtract.FromTimeLog(timeLog, 141);
            Assert.AreEqual(extract.Ddi, 141);
            Assert.AreEqual(extract.Data.Count, 4716);

            extract = ISOTLGExtract.FromTimeLog(timeLog, 148 /*Total FuelConsumption*/);
            Assert.AreEqual(extract.Ddi, 148);
            Assert.AreEqual(extract.Data.Count, 560);

        }


        [TestMethod]
        public void CanSummarizeData()
        {
            var isoxml = ISOXML.Load("./testdata/TimeLogs/ValidTimeLogs");

            //Testing Totals
            Assert.IsTrue(isoxml.TimeLogs["TLG00002"].TryGetTotalValue(90, 0, out var totalYield, TLGTotalAlgorithmType.NO_RESETS));
            Assert.AreEqual(totalYield, 242461);
        }
    }
}
