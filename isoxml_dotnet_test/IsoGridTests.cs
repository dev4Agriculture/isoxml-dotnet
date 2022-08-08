using Dev4ag;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
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
            string path = "./testdata/Grid/Type2";
            string outPath = "./out/grid_type2";
            var result = ISOXML.Load(path);
            var taskData = result.data;
            var task = taskData.Task[0];
            IsoGrid grid = null;
            string gridFileName = task.Grid[0].Filename;
            var success = result.grids.TryGetValue(gridFileName, out grid);
            Assert.IsNotNull(grid);
            result.folderPath = outPath;
            for(uint l = 0; l< grid.layers; l++)
            {
                for(uint y=0; y< grid.height; y++)
                {
                    for(uint x=0; x< grid.width; x++)
                    {
                        grid.setValue(y, x, y, l);
                    }
                }
            }
            result.save();

            Assert.IsTrue(File.Exists(Path.Combine(outPath, gridFileName+".BIN")));
            byte[] data = File.ReadAllBytes(Path.Combine(outPath, gridFileName + ".BIN"));
            Assert.AreEqual(data.Length, (Int32)(grid.width * grid.height * grid.layers * sizeof(UInt32)));

        }

        [TestMethod]
        public void CanWriteValidGridType1()
        {
            string path = "./testdata/Grid/Type1";
            string outPath = "./out/grid_type1";
            var result = ISOXML.Load(path);
            var taskData = result.data;
            var task = taskData.Task[0];
            IsoGrid grid = null;
            string gridFileName = task.Grid[0].Filename;
            var success = result.grids.TryGetValue(gridFileName, out grid);
            Assert.IsNotNull(grid);
            result.folderPath = outPath;
            for (uint y = 0; y < grid.height; y++)
            {
                for (uint x = 0; x < grid.width; x++)
                {
                    grid.setValue(y, x, y);
                }
            }
            result.save();

            Assert.IsTrue(File.Exists(Path.Combine(outPath, gridFileName + ".BIN")));
            byte[] data = File.ReadAllBytes(Path.Combine(outPath, gridFileName + ".BIN"));
            Assert.AreEqual(data.Length, (Byte)(grid.width * grid.height));

        }

    }
}
