using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Dev4Agriculture.ISO11783.ISOXML.Test
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
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, result.Grids.Count);
        }

        [TestMethod]
        public void CanLoadValidGridType2()
        {
            string path = "./testdata/Grid/Type2";
            var result = ISOXML.Load(path);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, result.Grids.Count);

        }

        [TestMethod]
        public void CanRecognizeInvalidGridType1()
        {
            string path = "./testdata/Grid/Type1_Invalid";
            var result = ISOXML.Load(path);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, result.Messages.Count);

        }

        [TestMethod]
        public void CanRecognizeInvalidGridType2()
        {
            string path = "./testdata/Grid/Type2_Invalid";
            var result = ISOXML.Load(path);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, result.Messages.Count);

        }

        [TestMethod]
        public void CanWriteValidGridType2()
        {
            string path = "./testdata/Grid/Type2";
            string outPath = "./out/grid_type2";
            var isoxml = ISOXML.Load(path);
            var taskData = isoxml.Data;
            var task = taskData.Task[0];
            ISOGridFile grid = null;
            string gridFileName = task.Grid[0].Filename;
            var success = isoxml.Grids.TryGetValue(gridFileName, out grid);
            Assert.IsNotNull(grid);
            isoxml.SetFolderPath(outPath);
            for (uint l = 0; l < grid.Layers; l++)
            {
                for (uint y = 0; y < grid.Height; y++)
                {
                    for (uint x = 0; x < grid.Width; x++)
                    {
                        grid.SetValue(y, x, y, l);
                    }
                }
            }
            isoxml.Save();

            Assert.IsTrue(File.Exists(Path.Combine(outPath, gridFileName + ".BIN")));
            byte[] data = File.ReadAllBytes(Path.Combine(outPath, gridFileName + ".BIN"));
            Assert.AreEqual(data.Length, (Int32)(grid.Width * grid.Height * grid.Layers * sizeof(UInt32)));

        }

        [TestMethod]
        public void CanWriteValidGridType1()
        {
            string path = "./testdata/Grid/Type1";
            string outPath = "./out/grid_type1";
            var result = ISOXML.Load(path);
            var taskData = result.Data;
            var task = taskData.Task[0];
            ISOGridFile grid = null;
            string gridFileName = task.Grid[0].Filename;
            var success = result.Grids.TryGetValue(gridFileName, out grid);
            Assert.IsNotNull(grid);
            result.SetFolderPath(outPath);
            for (uint y = 0; y < grid.Height; y++)
            {
                for (uint x = 0; x < grid.Width; x++)
                {
                    grid.SetValue(y, x, y);
                }
            }
            result.Save();

            Assert.IsTrue(File.Exists(Path.Combine(outPath, gridFileName + ".BIN")));
            byte[] data = File.ReadAllBytes(Path.Combine(outPath, gridFileName + ".BIN"));
            Assert.AreEqual(data.Length, (Byte)(grid.Width * grid.Height));

        }

    }
}
