using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class TaskSplitterTests
{
    [TestMethod]
    public void CanSplitTaskInto3Tasks()
    {
        var file = File.Open("./testdata/TaskSplitting/autolog1.zip",FileMode.Open);
        var isoxml = ISOXML.LoadFromArchive(file);
        var task1 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "Hallo"
        };
        var task2 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "Welt"
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(task1);
        isoxml.Data.Task.Add(task1);
        isoxml.IdTable.AddObjectAndAssignIdIfNone(task2);
        isoxml.Data.Task.Add(task2);
        var splitpoints = new Dictionary<ISOTask, List<DateTime>>()
        {
            {task1,[
            new DateTime(2025,06,22,10,00,00),
            new DateTime(2025,06,22,12,00,00),
            new DateTime(2025,06,22,14,00,00),
            new DateTime(2025,06,22,16,00,00),
            ] },
            {task2,[
            new DateTime(2025,06,22,11,00,00),
            new DateTime(2025,06,22,13,00,00),
            new DateTime(2025,06,22,15,00,00),
            new DateTime(2025,06,22,17,00,00)
            ] }
        };
        var task = isoxml.Data.Task.First();
        isoxml.SplitTaskAtTimeStamps(task, splitpoints);
        if (isoxml.Data.Task.Contains(task))
        {
            isoxml.Data.Task.Remove(task);
        }
        isoxml.SetFolderPath("C:\\src\\dev4Agriculture\\isodotnet2\\isoxml-dotnet\\Dev4Agriculture.ISO11783.ISOXML.Test\\testdata\\TaskSplitting");
        isoxml.Save();
        Assert.AreEqual(isoxml.Data.Task.Count, 2);
        foreach(var tsk in isoxml.Data.Task)
        {
            Assert.AreNotEqual(tsk.Time.Count, 0);
            Assert.AreNotEqual(tsk.DeviceAllocation.Count, 0);
        }

        // Find the tasks by their designator
        var halloTask = isoxml.Data.Task.FirstOrDefault(t => t.TaskDesignator == "Hallo");
        var weltTask = isoxml.Data.Task.FirstOrDefault(t => t.TaskDesignator == "Welt");

        Assert.IsNotNull(halloTask, "Hallo task should exist");
        Assert.IsNotNull(weltTask, "Welt task should exist");

        // Verify that Task1 (Hallo) includes TLG00005, TLG00007, TLG00009, TLG00011
        var halloTLGNames = halloTask.TimeLogs.Select(tlg => tlg.Name).ToList();
        Console.WriteLine($"Hallo task TimeLogs: {string.Join(", ", halloTLGNames)}");

        Assert.IsTrue(halloTLGNames.Contains("TLG00005"), "Hallo task should contain TLG00005");
        Assert.IsTrue(halloTLGNames.Contains("TLG00007"), "Hallo task should contain TLG00007");
        Assert.IsTrue(halloTLGNames.Contains("TLG00009"), "Hallo task should contain TLG00009");
        Assert.IsTrue(halloTLGNames.Contains("TLG00011"), "Hallo task should contain TLG00011");

        // Verify that Task2 (Welt) includes TLG00006, TLG00008, TLG00010
        var weltTLGNames = weltTask.TimeLogs.Select(tlg => tlg.Name).ToList();
        Console.WriteLine($"Welt task TimeLogs: {string.Join(", ", weltTLGNames)}");

        Assert.IsTrue(weltTLGNames.Contains("TLG00006"), "Welt task should contain TLG00006");
        Assert.IsTrue(weltTLGNames.Contains("TLG00008"), "Welt task should contain TLG00008");
        Assert.IsTrue(weltTLGNames.Contains("TLG00010"), "Welt task should contain TLG00010");

        // Additional verification: check the total count
        Assert.AreEqual(4, halloTask.TimeLogs.Count, "Hallo task should have exactly 4 TimeLogs");
        Assert.AreEqual(3, weltTask.TimeLogs.Count, "Welt task should have exactly 3 TimeLogs");
    }

    [TestMethod]
    public void CanGenerateDeviceAllocationsFromTimeLogs()
    {
        // Load test data using the same approach as the existing test
        var file = File.Open("./testdata/TaskSplitting/autolog1.zip", FileMode.Open);
        var isoxml = ISOXML.LoadFromArchive(file);

        // Get the first task that has TimeLogs
        var task = isoxml.Data.Task.FirstOrDefault(t => t.TimeLogs.Count > 0);
        Assert.IsNotNull(task, "No task with TimeLogs found in test data");



                // Test the GenerateDeviceAllocationsFromTimeLogs method
        var deviceAllocations = task.GenerateDeviceAllocationsFromTimeLogs(isoxml.Data.Device.ToList());

        // Verify that DeviceAllocations were generated
        Assert.IsNotNull(deviceAllocations, "DeviceAllocations list should not be null");
        Assert.IsTrue(deviceAllocations.Count > 0, "Should generate at least one DeviceAllocation");

        // Check that the DeviceAllocations have the expected structure
        foreach (var allocation in deviceAllocations)
        {
            Assert.IsNotNull(allocation.DeviceIdRef, "DeviceIdRef should not be null");
            Assert.IsNotNull(allocation.ClientNAMEValue, "ClientNAMEValue should not be null");
            Assert.IsNotNull(allocation.AllocationStamp, "AllocationStamp should not be null");
            Assert.IsTrue(allocation.AllocationStamp.Start != DateTime.MinValue, "Start time should be set");
            Assert.IsTrue(allocation.AllocationStamp.Stop != DateTime.MinValue, "Stop time should be set");
        }

        // Verify that the device with DeviceId "DVC-1" is included
        var dvc1Allocation = deviceAllocations.FirstOrDefault(da => da.DeviceIdRef == "DVC-1");
        Assert.IsNotNull(dvc1Allocation, "Should have DeviceAllocation for DVC-1");

        // Verify the ClientNAME matches the expected value
        var clientNameHex = BitConverter.ToString(dvc1Allocation.ClientNAMEValue).Replace("-", "");
        Assert.AreEqual("A01284000DE1177D", clientNameHex, "ClientNAME should match expected value");

        // Verify that only devices with matching DeviceElements are included
        var deviceElementIds = new HashSet<string>();
        foreach (var tlg in task.TimeLogs)
        {
            foreach (var ddiEntry in tlg.Header.Ddis)
            {
                // The DDI entries use negative numbers (-1, -2) which map to device element IDs "DET-1", "DET-2"
                var deviceElementId = $"DET{ddiEntry.DeviceElement}";
                deviceElementIds.Add(deviceElementId);
            }
        }

        Assert.IsTrue(deviceElementIds.Contains("DET-1"), "Should have DET-1 in device element IDs");
        Assert.IsTrue(deviceElementIds.Contains("DET-2"), "Should have DET-2 in device element IDs");

        // Store the original count of DeviceAllocations
        var originalCount = task.DeviceAllocation.Count;

        // Now actually add the DeviceAllocations to the Task
        foreach (var allocation in deviceAllocations)
        {
            task.DeviceAllocation.Add(allocation);
        }

        // Verify that the Task now contains the original plus the new DeviceAllocations
        Assert.AreEqual(originalCount + deviceAllocations.Count, task.DeviceAllocation.Count, "Task should contain original plus all generated DeviceAllocations");

        // Verify that the DeviceAllocations in the Task have the correct properties
        foreach (var allocation in task.DeviceAllocation)
        {
            var device = isoxml.Data.Device.FirstOrDefault(d => d.DeviceId == allocation.DeviceIdRef);
            Assert.IsNotNull(device, $"Device {allocation.DeviceIdRef} should exist");

            var hasMatchingDeviceElement = device.DeviceElement.Any(de => deviceElementIds.Contains(de.DeviceElementId));
            Assert.IsTrue(hasMatchingDeviceElement, $"Device {allocation.DeviceIdRef} should have a matching DeviceElement");
        }
    }


    [TestMethod]
    public void CanSplitExampleTaskset()
    {
        var path = "C:\\home\\OneDrive - dev4Agriculture\\dev4Ag\\07_CUSTOMERS\\059_HSC\\03_Projects\\02_ISOXML_Analysis\\02_Development\\2025-08-29-SplitTasks";
        // Load test data using the existing autolog1.zip file
        var file = File.Open(Path.Combine(path, "UnSplit.zip"), FileMode.Open);
        var isoxml = ISOXML.LoadFromArchive(file);

        // Create tasks with the given names (handling duplicates)
        // TSK12 appears multiple times, so we'll use consistent IDs
        var taskTSK12 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "TSK12 (Road)"
        };
        var taskTSK11 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "TSK11 (Field Vriescheloo)"
        };
        var taskTSK18 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "TSK18 (Field Gruenweiss)"
        };
        var taskTSK17 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "TSK17 (Field AmParkplatz)"
        };
        var taskTSK19 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "TSK19 (Field NorthMost)"
        };
        var taskTSK13 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "TSK13 (Field D an Flusskurve)"
        };
        var taskTSK14 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "TSK14 (Field LongTriangle)"
        };
        var taskTSK15 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "TSK15 (Field Aa-West)"
        };
        var taskTSK16 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "TSK16 (Field AmBurgGraben)"
        };

        var taskTSK20 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "TSK17 (Field RiverSouth)"
        };

        // Add tasks to ISOXML and assign IDs
        isoxml.IdTable.AddObjectAndAssignIdIfNone(taskTSK12);
        isoxml.IdTable.AddObjectAndAssignIdIfNone(taskTSK11);
        isoxml.IdTable.AddObjectAndAssignIdIfNone(taskTSK18);
        isoxml.IdTable.AddObjectAndAssignIdIfNone(taskTSK17);
        isoxml.IdTable.AddObjectAndAssignIdIfNone(taskTSK19);
        isoxml.IdTable.AddObjectAndAssignIdIfNone(taskTSK13);
        isoxml.IdTable.AddObjectAndAssignIdIfNone(taskTSK14);
        isoxml.IdTable.AddObjectAndAssignIdIfNone(taskTSK15);
        isoxml.IdTable.AddObjectAndAssignIdIfNone(taskTSK16);
        isoxml.IdTable.AddObjectAndAssignIdIfNone(taskTSK20);

        // Create the splitCombos with the given timestamps
        var splitCombos = new Dictionary<ISOTask, List<DateTime>>()
        {
            { taskTSK12, new List<DateTime>
                {
                    new DateTime(2025, 8, 20, 13, 18, 44,DateTimeKind.Utc),  // 2025-08-20T15:18:44
                    new DateTime(2025, 8, 20, 17, 53, 34,DateTimeKind.Utc),  // 2025-08-20T19:53:34
                    new DateTime(2025, 8, 20, 7, 44, 32, DateTimeKind.Utc),   // 2025-08-20T09:44:32
                    new DateTime(2025, 8, 20, 8, 47, 37,DateTimeKind.Utc),  // 2025-08-20T10:47:37
                    new DateTime(2025, 8, 20, 6, 42, 49, DateTimeKind.Utc)    // 2025-08-20T08:42:49
                }
            },
            { taskTSK11, new List<DateTime>
                {
                    new DateTime(2025, 8, 20, 12, 9, 13,DateTimeKind.Utc)   // 2025-08-20T14:09:13
                }
            },
            { taskTSK18, new List<DateTime>
                {
                    new DateTime(2025, 8, 20, 15, 14, 28,DateTimeKind.Utc), // 2025-08-20T17:14:28
                    new DateTime(2025, 8, 20, 17, 28, 55,DateTimeKind.Utc)  // 2025-08-20T19:28:55
                }
            },
            { taskTSK17, new List<DateTime>
                {
                    new DateTime(2025, 8, 20, 14, 6, 32,DateTimeKind.Utc)   // 2025-08-20T16:06:32
                }
            },
            { taskTSK19, new List<DateTime>
                {
                    new DateTime(2025, 8, 20, 16, 18, 10,DateTimeKind.Utc)  // 2025-08-20T18:18:10
                }
            },
            { taskTSK13, new List<DateTime>
                {
                    new DateTime(2025, 8, 20, 11, 0, 4, DateTimeKind.Utc)    // 2025-08-20T13:00:04
                }
            },
            { taskTSK14, new List<DateTime>
                {
                    new DateTime(2025, 8, 20, 7, 7, 26,DateTimeKind.Utc)    // 2025-08-20T09:07:26
                }
            },
            { taskTSK15, new List<DateTime>
                {
                    new DateTime(2025, 8, 20, 7, 45, 3, DateTimeKind.Utc)    // 2025-08-20T09:45:03
                }
            },
            { taskTSK16, new List<DateTime>
                {
                    new DateTime(2025, 8, 20, 8, 6, 33, DateTimeKind.Utc)   // 2025-08-20T10:06:33
                }
            },
            { taskTSK20, new List<DateTime>
                {
                    new DateTime(2025, 8, 20, 8, 49, 55,DateTimeKind.Utc),  // 2025-08-20T10:49:55
                }
            }
        };

        // Split the task set at the specified timestamps
        var splitted = isoxml.RedistributeTaskSetTimeLogs(splitCombos);


        // Save the archive to "FrankSplit.zip" in the same folder
        var frankSplitPath = Path.Combine(path, "FrankSplit.zip");
        isoxml.SaveToArchive(frankSplitPath);

        // Verify that the splitting was successful
        Assert.IsNotNull(splitted, "Split operation should return a result");
        Assert.IsTrue(splitted.Count > 0, "Should have created split tasks");

        // Verify that the FrankSplit.zip file was created
        Assert.IsTrue(File.Exists(frankSplitPath), "FrankSplit.zip should be created");

        Console.WriteLine($"Successfully created {splitted.Count} split tasks");
        Console.WriteLine($"Archive saved to: {frankSplitPath}");

        // Clean up the file handle
        file.Close();
    }
}
