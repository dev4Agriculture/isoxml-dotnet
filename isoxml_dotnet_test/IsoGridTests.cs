using Dev4ag;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4ag
{
    [TestClass]
    public class IsoGridTests
    {

        [TestMethod]
        public void CanLoadValidGridType1()
        {
            string path = "./testdata/Grid/Type1";
            var result = ISOXML.Load(path);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(1,result.grids.Count);
        }

        [TestMethod]
        public void CanLoadValidGridType2()
        {
            string path = "./testdata/Grid/Type2";
            var result = ISOXML.Load(path);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(1, result.grids.Count);

        }

        [TestMethod]
        public void CanRecognizeInvalidGridType1()
        {
            string path = "./testdata/Grid/Type1_Invalid";
            var result = ISOXML.Load(path);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(1, result.messages.Count);

        }

        [TestMethod]
        public void CanRecognizeInvalidGridType2()
        {
            string path = "./testdata/Grid/Type2_Invalid";
            var result = ISOXML.Load(path);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.data);
            Assert.AreEqual(1, result.messages.Count);

        }

        [TestMethod]
        public void CanWriteValidGridType2()
        {

        }

    }
}
