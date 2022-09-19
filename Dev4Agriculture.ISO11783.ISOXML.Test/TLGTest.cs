using System;
using System.Collections.Generic;
using System.Text;
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
            var isoxml = ISOXML.Load("./testdata/TimeLogs/ValidTimeLogs/");

            Assert.AreEqual(isoxml.TimeLogs.Count, 21);
            Assert.AreEqual(isoxml.Messages.Count, 0);
        }


        [TestMethod]
        public void FindsErrorsInInvalidTimeLogs()
        {
            //In this folder, TLG00003 has an Invalid XML Structure
            var isoxml = ISOXML.Load("./testdata/TimeLogs/BrokenTimeLogs/");

            Assert.AreEqual(isoxml.CountValidTimeLogs(), 20);
            Assert.AreEqual(isoxml.Messages.Count, 0);
        }

        [TestMethod]
        public void FindsErrorWithMissingTimeLogs()
        {
            //In this Folder, TLG00003.BIN, TLG00004.BIN, TLG00005.BIN, TLG00006.BIN are missing
            var isoxml = ISOXML.Load("./testdata/TimeLogs/MissingTimeLogs/");

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
        }


        [TestMethod]
        public void CanReadTimeLogContents()
        {
            var isoxml = ISOXML.Load("./testdata/TimeLogs/ValidTimeLogs/");


            Assert.IsTrue(isoxml.TimeLogs.TryGetValue("TLG00001", out var tlg));
            //Assert.IsTrue(tlg.Header.Ddis.Contains()
        }
    }
}
