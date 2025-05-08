using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.Emulator;
using Dev4Agriculture.ISO11783.ISOXML.Emulator.Generators;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class EnqueuerSingulatorTests
{
    private ISODevice GenerateHarvester(ISOXML isoxml, int serialNumber)
    {
        var generator = new DeviceGenerator(isoxml, "Harvester", "V1", new byte[] { 1, 2, 3, 4, 5, 6, 7 }, DeviceClass.Harvesters, 111, serialNumber);
        generator.AddDeviceProperty(new ISODeviceProperty()
        {
            DevicePropertyDDI = DDIUtils.FormatDDI(DDIList.DeviceElementOffsetX),
            DevicePropertyDesignator = "OffsetX",
            DevicePropertyValue = 200
        });

        generator.AddDeviceProperty(new ISODeviceProperty()
        {
            DevicePropertyDDI = DDIUtils.FormatDDI(DDIList.DeviceElementOffsetY),
            DevicePropertyDesignator = "OffsetY",
            DevicePropertyValue = 200
        });

        generator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.EffectiveTotalTime),
            DeviceProcessDataDesignator = "Effective Total Time",
            DeviceProcessDataProperty = (byte)(ISODeviceProcessDataPropertyType.Setable | ISODeviceProcessDataPropertyType.BelongsToDefaultSet),
            DeviceProcessDataTriggerMethods = (byte)(ISODeviceProcessDataTriggerMethodType.Total | ISODeviceProcessDataTriggerMethodType.OnTime)
        });

        generator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.IneffectiveTotalTime),
            DeviceProcessDataDesignator = "InEffective Total Time",
            DeviceProcessDataProperty = (byte)(ISODeviceProcessDataPropertyType.Setable | ISODeviceProcessDataPropertyType.BelongsToDefaultSet),
            DeviceProcessDataTriggerMethods = (byte)(ISODeviceProcessDataTriggerMethodType.Total | ISODeviceProcessDataTriggerMethodType.OnTime)
        });

        generator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.TotalArea),
            DeviceProcessDataDesignator = "TotalArea",
            DeviceProcessDataProperty = (byte)(ISODeviceProcessDataPropertyType.Setable | ISODeviceProcessDataPropertyType.BelongsToDefaultSet),
            DeviceProcessDataTriggerMethods = (byte)(ISODeviceProcessDataTriggerMethodType.Total | ISODeviceProcessDataTriggerMethodType.OnTime)
        });


        generator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.YieldTotalMass),
            DeviceProcessDataDesignator = "Total Mass",
            DeviceProcessDataProperty = (byte)(ISODeviceProcessDataPropertyType.Setable | ISODeviceProcessDataPropertyType.BelongsToDefaultSet),
            DeviceProcessDataTriggerMethods = (byte)(ISODeviceProcessDataTriggerMethodType.Total | ISODeviceProcessDataTriggerMethodType.OnTime)
        });

        generator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.LifetimeTotalArea),
            DeviceProcessDataDesignator = "Lifetime Total Area",
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet,
            DeviceProcessDataTriggerMethods = (byte)(ISODeviceProcessDataTriggerMethodType.Total | ISODeviceProcessDataTriggerMethodType.OnTime)
        });


        generator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.AverageYieldMassPerArea),
            DeviceProcessDataDesignator = "Average Mass per Area",
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet,
            DeviceProcessDataTriggerMethods = (byte)(ISODeviceProcessDataTriggerMethodType.Total | ISODeviceProcessDataTriggerMethodType.OnTime)
        });


        return generator.GetDevice();

    }


    [TestMethod]
    public void CanCreateSingulateAndEnqueueTimesAndTimeLogs()
    {
        var isoxml = ISOXML.Create("C:/data/isoxml_test");
        var task1 = new ISOTask()
        {
            TaskDesignator = "Harvest"
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(task1);
        isoxml.Data.Task.Add(task1);

        var task2 = new ISOTask()
        {
            TaskDesignator = "Second"
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(task2);
        isoxml.Data.Task.Add(task2);


        


        var emulator = new TaskControllerEmulator(isoxml);
        emulator.ConnectDevice(GenerateHarvester(isoxml, 12345));

        var startTime = new DateTime(2024, 09, 03, 10, 5, 2);


        var effectiveTotalTime = 0;
        var inEffectiveTotalTime = 0;
        var totalArea = 0;
        var lifetimeTotalArea = 600000;
        var AverageMassPerArea = 0;
        var totalMass = 0;

        var curWorkstate = 0;
        var minYieldPerSecond = 300;
        var minAreaPerSecond = 30;
        var curYieldPerSecond = 0;
        var curAreaPerSecond = 0;

        var stepMax = 400;
        var breakTime = 200;

        for (var taskCount = 0; taskCount < 4; taskCount++)
        {
            emulator.StartTask(startTime.AddSeconds(stepMax*taskCount), task1);
            for (var index = 0; index < stepMax; index++)
            {
                emulator.AddTimeAndPosition(startTime.AddSeconds((stepMax + breakTime) * taskCount + index), new ISOPosition()
                {
                    PositionEast = (decimal)(7.3 + taskCount * 0.001),
                    PositionNorth = taskCount % 2 == 0 ? (decimal)(52.3 + index * 0.0001) : (decimal)(52.3 + (stepMax - index) * 0.0001)
                });

                if (index % 80 == 0)
                {
                    curWorkstate = curWorkstate == 0 ? 1 : 0;
                    curYieldPerSecond = minYieldPerSecond * curWorkstate;
                    curAreaPerSecond = minAreaPerSecond * curWorkstate;
                }

                if (index % 20 == 0)
                {
                    curYieldPerSecond += 20;
                    curAreaPerSecond += 2;
                }
                if (index % 10 == 0)
                {
                    curYieldPerSecond -= 10;
                    curAreaPerSecond -= 3;
                }


                if (curWorkstate == 1)
                {
                    emulator.UpdateRawMachineValue(DDIList.TotalArea, totalArea);
                    emulator.UpdateRawMachineValue(DDIList.YieldTotalMass, totalMass);
                    emulator.UpdateRawMachineValue(DDIList.EffectiveTotalTime, effectiveTotalTime);
                    if(totalArea > 0)
                    {
                        emulator.UpdateRawMachineValue(DDIList.AverageYieldMassPerArea, AverageMassPerArea);
                    }
                }
                else
                {
                    emulator.UpdateRawMachineValue(DDIList.IneffectiveTotalTime, inEffectiveTotalTime);
                }

                if (totalArea > 0)
                {
                    AverageMassPerArea = totalMass / totalArea * 10;
                }
                effectiveTotalTime += curWorkstate;
                inEffectiveTotalTime += 1 - curWorkstate;
                totalArea += curAreaPerSecond * curWorkstate;
                totalMass += curYieldPerSecond * curWorkstate;
                lifetimeTotalArea += curAreaPerSecond * curWorkstate;


            }
            emulator.PauseTask();
        }

        emulator.FinishTask();


        var firstEmulatedTaskData = emulator.ExportISOXML(DateTime.Now, true);
        firstEmulatedTaskData.Save();
        Assert.AreEqual(3,firstEmulatedTaskData.Data.Task.Count);//2 manually added, 1 AutoLogTask
        Assert.AreEqual(true, task1.TryGetTotalValue((ushort)DDIList.IneffectiveTotalTime, -1, out var ineffTime, TLGTotalAlgorithmType.NO_RESETS));
        Assert.AreEqual(inEffectiveTotalTime - 1, ineffTime);
        Assert.AreEqual(true, task1.TryGetTotalValue((ushort)DDIList.AverageYieldMassPerArea, -1, out var avgYieldMassPerArea, TLGTotalAlgorithmType.NO_RESETS));
        Assert.AreEqual(AverageMassPerArea, avgYieldMassPerArea);

    }
}
