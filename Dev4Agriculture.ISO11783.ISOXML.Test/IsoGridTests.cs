using System.IO;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    public void CanCreateAndSaveGridType2()
    {
        var path_out = "./out/gridType1";
        var gridName = "GRD00001";
        uint rows = 20;
        uint columns = 10;
        var isoxml = ISOXML.Create(path_out);
        var task = new ISOTask()
        {
            TaskDesignator = "TestGrid",
            TaskStatus = ISOTaskStatus.Planned
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(task);


        var grid = task.CreateGrid(new ISOGrid()
        {
            Filename = gridName,
            GridCellEastSize = 0.0001,
            GridCellNorthSize = 0.0001,
            GridMaximumColumn = columns,
            GridMaximumRow = rows,
            GridType = ISOGridType.gridtype2,
            TreatmentZoneCode = 1
        },
        new[] {
            new ISOGridLayer(1, "DET-1")
        });

        isoxml.Grids.Add(gridName, grid);

        task.AddDefaultGridValues(0, 1);

        for (uint row = 0; row < rows; row++)
        {
            for (uint column = 0; column < columns; column++)
            {
                grid.SetValue(column, row, row);
            }
        }

        isoxml.Data.Task.Add(task);


        isoxml.Save();
        var filePath = Path.Combine(path_out, gridName + ".bin");
        Assert.IsTrue(File.Exists(filePath));
        var info = new FileInfo(filePath);
        Assert.AreEqual(info.Length, 20 * 10 * 4);
        var isoXML_Loaded = ISOXML.Load(path_out);
        Assert.AreEqual(isoXML_Loaded.Data.Task.Count, 1);
        Assert.AreEqual(isoXML_Loaded.Grids[ isoXML_Loaded.Data.Task[0].Grid[0].Filename].GetValue(8, 5, 0), (uint)5);

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
                    grid.SetValue(y, x, y, l);
                }
            }
        }
        isoxml.Save();

        Assert.IsTrue(File.Exists(Path.Combine(outPath, gridFileName + ".BIN")));
        var data = File.ReadAllBytes(Path.Combine(outPath, gridFileName + ".BIN"));
        Assert.AreEqual(data.Length, (int)(grid.Width * grid.Height * grid.Layers * sizeof(uint)));

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
                grid.SetValue(y, x, y);
            }
        }
        result.Save();

        Assert.IsTrue(File.Exists(Path.Combine(outPath, gridFileName + ".BIN")));
        var data = File.ReadAllBytes(Path.Combine(outPath, gridFileName + ".BIN"));
        Assert.AreEqual(data.Length, (byte)(grid.Width * grid.Height));

    }

}
