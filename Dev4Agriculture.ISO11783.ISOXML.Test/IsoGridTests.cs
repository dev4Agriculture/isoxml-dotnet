using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev4Agriculture.ISO11783.ISOXML;
namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class IsoGridTests
{

    [TestMethod]
    public void CanLoadValidGridType1()
    {
        var path = "./testdata/Grid/Type1";
        var result = ISOXML.Load(path);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Grids.Count);
        Assert.AreEqual(1, result.Grids["GRD00001"].Layers);
    }

    [TestMethod]
    public void CanLoadValidGridType2()
    {
        var path = "./testdata/Grid/Type2";
        var result = ISOXML.Load(path);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Grids.Count);
        Assert.AreEqual(3, result.Grids["GRD00001"].Layers);

    }

    [TestMethod]
    public void CanRecognizeInvalidGridType1()
    {
        var path = "./testdata/Grid/Type1_Invalid";
        var result = ISOXML.Load(path);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Messages.Count);

    }

    [TestMethod]
    public void CanRecognizeInvalidGridType2()
    {
        var path = "./testdata/Grid/Type2_Invalid";
        var result = ISOXML.Load(path);
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Messages.Count);

    }

    [TestMethod]
    public void CanWriteValidGridType2()
    {
        var path = "./testdata/Grid/Type2";
        var outPath = "./out/grid_type2";
        var isoxml = ISOXML.Load(path);
        var taskData = isoxml.Data;
        var task = taskData.Task[0];
        var gridFileName = task.Grid[0].Filename;
        var success = isoxml.Grids.TryGetValue(gridFileName, out var grid);
        Assert.IsNotNull(grid);
        isoxml.SetFolderPath(outPath);
        for (uint l = 0; l < grid.Layers; l++)
        {
            for (uint y = 0; y < grid.Height; y++)
            {
                for (uint x = 0; x < grid.Width; x++)
                {
                    grid.SetValue(x, y, (int)y, l);
                }
            }
        }
        isoxml.Save();

        Assert.IsTrue(File.Exists(Path.Combine(outPath, gridFileName + ".bin")));
        var data = File.ReadAllBytes(Path.Combine(outPath, gridFileName + ".bin"));
        Assert.AreEqual(data.Length, (int)(grid.Width * grid.Height * grid.Layers * sizeof(uint)));

        var isoxml2 = ISOXML.Load(outPath);
        var gridLoaded = isoxml2.Grids.First().Value;
        Assert.AreEqual(gridLoaded.Width, grid.Width);
        Assert.AreEqual(gridLoaded.Height, grid.Height);
        Assert.AreEqual(gridLoaded.Layers, grid.Layers);
        for (uint l = 0; l < grid.Layers; l++)
        {
            for (uint y = 0; y < grid.Height; y++)
            {
                for (uint x = 0; x < grid.Width; x++)
                {
                    Assert.AreEqual(grid.GetValue(x, y, l), gridLoaded.GetValue(x, y, l));
                }
            }
        }

    }

    [TestMethod]
    public void CanWriteValidGridType1()
    {
        var path = "./testdata/Grid/Type1";
        var outPath = "./out/grid_type1";
        var result = ISOXML.Load(path);
        var taskData = result.Data;
        var task = taskData.Task[0];
        var gridFileName = task.Grid[0].Filename;
        var success = result.Grids.TryGetValue(gridFileName, out var grid);
        Assert.IsNotNull(grid);
        result.SetFolderPath(outPath);
        for (uint y = 0; y < grid.Height; y++)
        {
            for (uint x = 0; x < grid.Width; x++)
            {
                grid.SetValue(y, x, (int)y);
            }
        }
        result.Save();

        Assert.IsTrue(File.Exists(Path.Combine(outPath, gridFileName + ".bin")));
        var data = File.ReadAllBytes(Path.Combine(outPath, gridFileName + ".bin"));
        Assert.AreEqual(data.Length, (byte)(grid.Width * grid.Height));

    }

    [TestMethod]
    public void CanReadGridValue()
    {
        var isoxml = ISOXML.Load("./testdata/Grid/Type2_ReadValue");
        var grid = isoxml.Data.Task[0].Grid[0];
        var longitude = (decimal)7.2251319728;
        var latitude = (decimal)52.2240239571;
        var value = grid.GetSetpointValue(longitude, latitude);
        Assert.AreEqual(value, 1005);
        //Test Outside of Area
        longitude = (decimal)8.22513197280;
        latitude = (decimal)52.2240239571;
        value = grid.GetSetpointValue(longitude, latitude);
        Assert.AreEqual(value, Constants.TLG_VALUE_FOR_NO_VALUE);
    }

    [TestMethod]
    public void WillAdjustGridPositionOnSave()
    {
        var outPath = "./testdata/Grid/Type2_ReadValue";
        //Loading a random task is faster than creating one ;) 
        var isoxml = ISOXML.Load(outPath);
        isoxml.Data.Task[0].Grid.FirstOrDefault().GridMinimumNorthPosition = 10.01234567890123m;

        isoxml.SetFolderPath("./GridTest");
        isoxml.Save();
        Assert.AreEqual(isoxml.Data.Task[0].Grid.FirstOrDefault().GridMinimumNorthPosition, 10.012345679m);
    }


    [TestMethod]
    public void CanExportCSVFiles()
    {
        var inPath = "./testdata/Grid/Type2_ReadValue";
        var outPath = "./GridTest";
        //Loading a random task is faster than creating one ;) 
        var isoxml = ISOXML.Load(inPath);
        isoxml.SetFolderPath(outPath);
        foreach (var grid in isoxml.Grids)
        {
            grid.Value.SaveCSV(outPath);
        }
        Assert.IsTrue(File.Exists(Path.Combine(outPath, "GRD00001.CSV")));
    }
}
