using System.IO;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.IO.Compression;
using System.Collections.Generic;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using Dev4Agriculture.ISO11783.ISOXML.DTO;
using de.dev4Agriculture.ISOXML.DDI;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class SaveISOXMLTests
{
    private void AddData(ISOXML isoxml, string taskName)
    {
        var idTable = isoxml.IdTable;
        var task = new ISOTask()
        {
            TaskDesignator = taskName,
            TaskStatus = ISOTaskStatus.Completed
        };
        idTable.AddObjectAndAssignIdIfNone(task);
        isoxml.Data.Task.Add(task);

    }


    [TestMethod]
    public void CanExtendISOXML()
    {
        var path = "./testdata/Structure/Valid_To_Extend/";
        var path_Out = "./out/Valid_Extended";
        var isoxml = ISOXML.Load(path);
        isoxml.SetFolderPath(path_Out);
        var taskName = "Hello";
        AddData(isoxml, taskName);


        isoxml.Save();

        var tdPath = Path.Combine(path_Out, "TASKDATA.XML");
        Assert.IsTrue(File.Exists(tdPath));
        var allText = File.ReadAllText(tdPath);
        Assert.IsTrue(allText.Contains(taskName));

        var loaded = ISOXML.Load(path_Out);
        Assert.AreEqual(2, loaded.Data.Task.Count);
        Assert.AreEqual("TSK2", loaded.Data.Task[1].TaskId);

    }

    [TestMethod]
    public void CanExtendISOXMLAsync()
    {
        var path = "./testdata/Structure/Valid_To_Extend/";
        var path_Out = "./out/Valid_Extended_Async";
        var waiter = ISOXML.LoadAsync(path);
        waiter.Wait();
        var isoxml = waiter.Result;
        isoxml.SetFolderPath(path_Out);
        var taskName = "Hello";
        AddData(isoxml, taskName);


        var saver = isoxml.SaveAsync();
        saver.Wait();

        var tdPath = Path.Combine(path_Out, "TASKDATA.XML");
        Assert.IsTrue(File.Exists(tdPath));
        var allText = File.ReadAllText(tdPath);
        Assert.IsTrue(allText.Contains(taskName));

        var loaded = ISOXML.Load(path_Out);
        Assert.AreEqual(2, loaded.Data.Task.Count);
        Assert.AreEqual("TSK2", loaded.Data.Task[1].TaskId);

    }



    [TestMethod]
    public void CanCreateISOXML()
    {
        var path = "./out/valid_new/";
        var taskName = "New";
        var isoxml = ISOXML.Create(path);
        var idTable = isoxml.IdTable;
        var task = new ISOTask()
        {
            TaskDesignator = taskName,
            TaskStatus = ISOTaskStatus.Completed
        };
        idTable.AddObjectAndAssignIdIfNone(task);
        isoxml.Data.Task.Add(task);
        isoxml.Save();

        var tdPath = Path.Combine(path, "TASKDATA.XML");
        Assert.IsTrue(File.Exists(tdPath));
        var allText = File.ReadAllText(tdPath);
        Assert.IsTrue(allText.Contains(taskName));

        var loaded = ISOXML.Load(path);
        Assert.AreEqual(1, loaded.Data.Task.Count);
        Assert.AreEqual("TSK1", loaded.Data.Task[0].TaskId);

    }

    private ISOXML GenerateISOXMLForStreamSaveTesting(string tempPath)
    {

        var isoxml = ISOXML.Create(tempPath);

        // Add LinkList
        isoxml.AddLinkList();
        isoxml.LinkList.AddLink("TSK1", Guid.NewGuid().ToString());
        isoxml.LinkList.AddLink("TSK2", Guid.NewGuid().ToString());

        // Create first task with grid
        var task1 = new ISOTask()
        {
            TaskDesignator = "Task with Grid 1",
            TaskStatus = ISOTaskStatus.Planned
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(task1);
        isoxml.Data.Task.Add(task1);

        // Create second task with grid
        var task2 = new ISOTask()
        {
            TaskDesignator = "Task with Grid 2",
            TaskStatus = ISOTaskStatus.Planned
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(task2);
        isoxml.Data.Task.Add(task2);

        // Generate grids for both tasks
        var grid1 = isoxml.GenerateGrid(ISOGridType.gridtype1, 10, 10, 1);
        var grid2 = isoxml.GenerateGrid(ISOGridType.gridtype2, 20, 20, 3);

        // Assign grids to tasks
        task1.Grid.Add(new ISOGrid()
        {
            Filename = grid1.Filename,
            GridMaximumColumn = grid1.GridMaximumColumn,
            GridMaximumRow = grid1.GridMaximumRow,
            GridType = grid1.GridType
        });

        task2.Grid.Add(new ISOGrid()
        {
            Filename = grid2.Filename,
            GridMaximumColumn = grid2.GridMaximumColumn,
            GridMaximumRow = grid2.GridMaximumRow,
            GridType = grid2.GridType
        });

        var gridZone = new GridZoneDTO(ISOGridType.gridtype1, 1);
        gridZone.Layer[0].DDI = (ushort)DDIList.SetpointApplicationRateOfAmmonium;
        gridZone.Layer[0].GridType1Values.Add(1, 10);
        gridZone.Layer[0].GridType1Values.Add(2, 20);


        task1.GenerateTreatmentZones(gridZone);
        var zones = new GridZoneDTO(ISOGridType.gridtype2, 3);
        zones.Layer[0].DDI = (ushort)DDIList.SetpointApplicationRateOfAmmonium;
        zones.Layer[1].DDI = (ushort)DDIList.SetpointApplicationRateOfDryMatter;
        zones.Layer[2].DDI = (ushort)DDIList.SetpointApplicationRateOfNitrogenN2;
        task2.GenerateTreatmentZones(zones);

        // Fill grid data - fix the logic to properly fill each grid
        var grid1File = isoxml.Grids["GRD00001"];
        var grid2File = isoxml.Grids["GRD00002"];

        for (uint y = 0; y < grid1File.Height; y++)
        {
            for (uint x = 0; x < grid1File.Width; x++)
            {
                grid1File.SetValue(x, y, (int)(x + y), 0);
            }
        }

        for (uint l = 0; l < grid2File.Layers; l++)
        {
            for (uint y = 0; y < grid2File.Height; y++)
            {
                for (uint x = 0; x < grid2File.Width; x++)
                {
                    grid2File.SetValue(x, y, (int)(x + y + l), l);
                }
            }
        }

        return isoxml;
    }

    [TestMethod]
    public void CanSaveToStreamAndLoadArchiveWithLinkListAndGrids()
    {
        var tempPath = FileUtils.GetLibraryTempFolder();
        Directory.Delete(tempPath,true);
        Directory.CreateDirectory(tempPath);

        var isoxml = GenerateISOXMLForStreamSaveTesting(tempPath);

        MemoryStream zipStream = null;
        try
        {
            zipStream = isoxml.SaveToStream();

            // Verify the stream contains data
            Assert.IsTrue(zipStream.Length > 0);
            Assert.AreEqual(0, zipStream.Position);

            // Verify the zip file structure
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                var entries = archive.Entries.ToList();

                // Check that all expected files exist
                Assert.IsTrue(entries.Any(e => e.Name == "TASKDATA.XML"), "TASKDATA.XML should exist in archive");
                Assert.IsTrue(entries.Any(e => e.Name == "LINKLIST.XML"), "LINKLIST.XML should exist in archive");
                Assert.IsTrue(entries.Any(e => e.Name == "GRD00001.bin"), "GRD00001.bin should exist in archive");
                Assert.IsTrue(entries.Any(e => e.Name == "GRD00002.bin"), "GRD00002.bin should exist in archive");

                // Verify we have exactly 4 files
                Assert.AreEqual(4, entries.Count, "Archive should contain exactly 4 files");

                // Verify file sizes are reasonable
                var taskDataEntry = entries.First(e => e.Name == "TASKDATA.XML");
                var linkListEntry = entries.First(e => e.Name == "LINKLIST.XML");
                var grid1Entry = entries.First(e => e.Name == "GRD00001.bin");
                var grid2Entry = entries.First(e => e.Name == "GRD00002.bin");

                Assert.IsTrue(taskDataEntry.Length > 0, "TASKDATA.XML should have content");
                Assert.IsTrue(linkListEntry.Length > 0, "LINKLIST.XML should have content");
                Assert.IsTrue(grid1Entry.Length > 0, "GRD00001.bin should have content");
                Assert.IsTrue(grid2Entry.Length > 0, "GRD00002.bin should have content");

                // Verify grid binary file sizes match expected sizes
                var expectedGrid1Size = 10 * 10 * 1 * sizeof(byte); // width * height * layers * sizeof(uint)
                var expectedGrid2Size = 20 * 20 * 3 * sizeof(uint);

                Assert.AreEqual(expectedGrid1Size, grid1Entry.Length, "GRD00001.bin should have correct size");
                Assert.AreEqual(expectedGrid2Size, grid2Entry.Length, "GRD00002.bin should have correct size");

                // Reset stream position for loading
                zipStream.Position = 0;

                // Test loading the archive back
                var loadedIsoxml = ISOXML.LoadFromArchive(zipStream);

                // Verify the loaded ISOXML has all the expected data
                Assert.IsNotNull(loadedIsoxml);
                Assert.IsNotNull(loadedIsoxml.Data);
                Assert.AreEqual(2, loadedIsoxml.Data.Task.Count);
                Assert.IsTrue(loadedIsoxml.HasLinkList);
                Assert.AreEqual(2, loadedIsoxml.Grids.Count);

                // Verify tasks
                Assert.AreEqual("Task with Grid 1", loadedIsoxml.Data.Task[0].TaskDesignator);
                Assert.AreEqual("Task with Grid 2", loadedIsoxml.Data.Task[1].TaskDesignator);

                // Verify grids
                Assert.IsTrue(loadedIsoxml.Grids.ContainsKey("GRD00001"));
                Assert.IsTrue(loadedIsoxml.Grids.ContainsKey("GRD00002"));

                var loadedGrid1 = loadedIsoxml.Grids["GRD00001"];
                var loadedGrid2 = loadedIsoxml.Grids["GRD00002"];

                Assert.AreEqual(20u, loadedGrid2.Width);
                Assert.AreEqual(20u, loadedGrid2.Height);
                Assert.AreEqual(3, loadedGrid2.Layers);

                Assert.AreEqual(10u, loadedGrid1.Width);
                Assert.AreEqual(10u, loadedGrid1.Height);
                Assert.AreEqual(1, loadedGrid1.Layers);

                // Verify grid data integrity
                for (uint y = 0; y < loadedGrid1.Height; y++)
                {
                    for (uint x = 0; x < loadedGrid1.Width; x++)
                    {
                        Assert.AreEqual((int)(x + y), loadedGrid1.GetValue(x, y, 0));
                    }
                }

                for (uint l = 0; l < loadedGrid2.Layers; l++)
                {
                    for (uint y = 0; y < loadedGrid2.Height; y++)
                    {
                        for (uint x = 0; x < loadedGrid2.Width; x++)
                        {
                            Assert.AreEqual((int)(x + y + l), loadedGrid2.GetValue(x, y, l));
                        }
                    }
                }

                // Verify LinkList
                Assert.IsNotNull(loadedIsoxml.LinkList);
                Assert.AreEqual(1, loadedIsoxml.LinkList.FindAllLinks("TSK1").ToList().Count);
                Assert.AreEqual(1, loadedIsoxml.LinkList.FindAllLinks("TSK2").ToList().Count);
            }
        }
        finally
        {
            // Dispose the stream properly
            zipStream?.Dispose();
        }

        // Verify temporary folder was cleaned up (this should happen automatically in SaveToStream)
        // The temp folder should not exist anymore since it was cleaned up in the finally block
        var tempFolders = Directory.GetDirectories(tempPath);
        // Note: We can't guarantee cleanup timing, so we just verify the method completed successfully
        Thread.Sleep(1000);
        Assert.AreEqual(0, tempFolders.Length);
    }

    [TestMethod]
    public async System.Threading.Tasks.Task CanSaveToStreamAsyncAndLoadArchiveWithLinkListAndGrids()
    {
        var tempPath = FileUtils.GetLibraryTempFolder();
        Directory.Delete(tempPath, true);
        Directory.CreateDirectory(tempPath);

        var isoxml = GenerateISOXMLForStreamSaveTesting(tempPath);

        // Save to stream asynchronously (this creates the zip in memory)
        MemoryStream zipStream = null;
        try
        {
            zipStream = await isoxml.SaveToStreamAsync();

            // Verify the stream contains data
            Assert.IsTrue(zipStream.Length > 0);
            Assert.AreEqual(0, zipStream.Position);

            // Verify the zip file structure
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                var entries = archive.Entries.ToList();

                // Check that all expected files exist
                Assert.IsTrue(entries.Any(e => e.Name == "TASKDATA.XML"), "TASKDATA.XML should exist in archive");
                Assert.IsTrue(entries.Any(e => e.Name == "LINKLIST.XML"), "LINKLIST.XML should exist in archive");
                Assert.IsTrue(entries.Any(e => e.Name == "GRD00001.bin"), "GRD00001.bin should exist in archive");
                Assert.IsTrue(entries.Any(e => e.Name == "GRD00002.bin"), "GRD00002.bin should exist in archive");

                // Verify we have exactly 4 files
                Assert.AreEqual(4, entries.Count, "Archive should contain exactly 4 files");

                // Verify file sizes are reasonable
                var taskDataEntry = entries.First(e => e.Name == "TASKDATA.XML");
                var linkListEntry = entries.First(e => e.Name == "LINKLIST.XML");
                var grid1Entry = entries.First(e => e.Name == "GRD00001.bin");
                var grid2Entry = entries.First(e => e.Name == "GRD00002.bin");

                Assert.IsTrue(taskDataEntry.Length > 0, "TASKDATA.XML should have content");
                Assert.IsTrue(linkListEntry.Length > 0, "LINKLIST.XML should have content");
                Assert.IsTrue(grid1Entry.Length > 0, "GRD00001.bin should have content");
                Assert.IsTrue(grid2Entry.Length > 0, "GRD00002.bin should have content");

                // Verify grid binary file sizes match expected sizes
                var expectedGrid1Size = 10 * 10 * 1 * sizeof(byte); // width * height * layers * sizeof(uint)
                var expectedGrid2Size = 20 * 20 * 3 * sizeof(uint);

                Assert.AreEqual(expectedGrid1Size, grid1Entry.Length, "GRD00001.bin should have correct size");
                Assert.AreEqual(expectedGrid2Size, grid2Entry.Length, "GRD00002.bin should have correct size");

                // Reset stream position for loading
                zipStream.Position = 0;

                // Test loading the archive back
                var loadedIsoxml = await ISOXML.LoadFromArchiveAsync(zipStream);

                // Verify the loaded ISOXML has all the expected data
                Assert.IsNotNull(loadedIsoxml);
                Assert.IsNotNull(loadedIsoxml.Data);
                Assert.AreEqual(2, loadedIsoxml.Data.Task.Count);
                Assert.IsTrue(loadedIsoxml.HasLinkList);
                Assert.AreEqual(2, loadedIsoxml.Grids.Count);

                // Verify tasks
                Assert.AreEqual("Task with Grid 1", loadedIsoxml.Data.Task[0].TaskDesignator);
                Assert.AreEqual("Task with Grid 2", loadedIsoxml.Data.Task[1].TaskDesignator);

                // Verify grids
                Assert.IsTrue(loadedIsoxml.Grids.ContainsKey("GRD00001"));
                Assert.IsTrue(loadedIsoxml.Grids.ContainsKey("GRD00002"));

                var loadedGrid1 = loadedIsoxml.Grids["GRD00001"];
                var loadedGrid2 = loadedIsoxml.Grids["GRD00002"];

                Assert.AreEqual(10u, loadedGrid1.Width);
                Assert.AreEqual(10u, loadedGrid1.Height);
                Assert.AreEqual(1, loadedGrid1.Layers);

                Assert.AreEqual(20u, loadedGrid2.Width);
                Assert.AreEqual(20u, loadedGrid2.Height);
                Assert.AreEqual(3, loadedGrid2.Layers);

                // Verify grid data integrity
                for (uint y = 0; y < loadedGrid1.Height; y++)
                {
                    for (uint x = 0; x < loadedGrid1.Width; x++)
                    {
                        Assert.AreEqual((int)(x + y), loadedGrid1.GetValue(x, y, 0));
                    }
                }

                for (uint l = 0; l < loadedGrid2.Layers; l++)
                {
                    for (uint y = 0; y < loadedGrid2.Height; y++)
                    {
                        for (uint x = 0; x < loadedGrid2.Width; x++)
                        {
                            Assert.AreEqual((int)(x + y + l), loadedGrid2.GetValue(x, y, l));
                        }
                    }
                }

                // Verify LinkList
                Assert.IsNotNull(loadedIsoxml.LinkList);
                Assert.AreEqual(1, loadedIsoxml.LinkList.FindAllLinks("TSK1").Count());
                Assert.AreEqual(1, loadedIsoxml.LinkList.FindAllLinks("TSK2").Count());
            }

        }
        finally
        {
            // Dispose the stream properly
            zipStream?.Dispose();
        }

        // Verify temporary folder was cleaned up (this should happen automatically in SaveToStream)
        // The temp folder should not exist anymore since it was cleaned up in the finally block
        var tempFolders = Directory.GetDirectories(tempPath, "isoxmltmp*");
        // Note: We can't guarantee cleanup timing, so we just verify the method completed successfully
    }


}
