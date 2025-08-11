using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIRegistry;
using Dev4Agriculture.ISO11783.ISOXML.Emulator;
using Dev4Agriculture.ISO11783.ISOXML.Emulator.Generators;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class EnqueuerSingulatorTests
{
    private const ushort ProprietaryAverageDDI = 0xEAAA;
    private const ushort ProprietaryWeightDDI = 0xEAAB;
    private const ushort Manufacturer = 47;


    private ISODevice GenerateHarvester(ISOXML isoxml, int serialNumber)
    {
        var generator = new DeviceGenerator(isoxml, "Harvester", "V1", new byte[] { 1, 2, 3, 4, 5, 6, 7 }, DeviceClass.Harvesters, Manufacturer, serialNumber);
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


        generator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(ProprietaryAverageDDI),
            DeviceProcessDataDesignator = "Average Fun per Time",
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet,
            DeviceProcessDataTriggerMethods = (byte)(ISODeviceProcessDataTriggerMethodType.Total | ISODeviceProcessDataTriggerMethodType.OnTime)
        });

        generator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(ProprietaryWeightDDI),
            DeviceProcessDataDesignator = "Fun count",
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet,
            DeviceProcessDataTriggerMethods = (byte)(ISODeviceProcessDataTriggerMethodType.Total | ISODeviceProcessDataTriggerMethodType.OnTime)
        });

        return generator.GetDevice();

    }


    [TestMethod]
    public void CanCreateSingulateAndEnqueueTimesAndTimeLogs()
    {
        DDIRegister.Clear();
        DDIRegister.RegisterProprietaryDDI(ProprietaryAverageDDI, Manufacturer, new DDIRegisterWeightedAverageEntry(new() { ProprietaryWeightDDI }));
        DDIRegister.RegisterProprietaryDDI(ProprietaryWeightDDI, Manufacturer, new DDIRegisterSumTotalEntry());
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
        var FunSum = 0;
        var FunCount = 0;
        var totalMass = 0;

        var curWorkstate = 0;
        var minYieldPerSecond = 300;
        var minAreaPerSecond = 30;
        var curYieldPerSecond = 0;
        var curAreaPerSecond = 0;

        var stepMax = 400;
        var breakTime = 200;

        for (var taskStepCount = 0; taskStepCount < 4; taskStepCount++)
        {
            emulator.StartTask(startTime.AddSeconds(stepMax*taskStepCount), task1);
            curWorkstate = 0;
            for (var index = 0; index < stepMax; index++)
            {
                emulator.AddTimeAndPosition(startTime.AddSeconds((stepMax + breakTime) * taskStepCount + index), new ISOPosition()
                {
                    PositionEast = (decimal)(7.3 + taskStepCount * 0.001),
                    PositionNorth = taskStepCount % 2 == 0 ? (decimal)(52.3 + index * 0.0001) : (decimal)(52.3 + (stepMax - index) * 0.0001)
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

                FunCount++;
                FunSum += taskStepCount*10;


                if (totalArea > 0)
                {
                    AverageMassPerArea = totalMass / totalArea * 10;
                }
                effectiveTotalTime += curWorkstate;
                inEffectiveTotalTime += 1 - curWorkstate;
                totalArea += curAreaPerSecond * curWorkstate;
                totalMass += curYieldPerSecond * curWorkstate;
                lifetimeTotalArea += curAreaPerSecond * curWorkstate;


                emulator.UpdateRawMachineValue(ProprietaryAverageDDI, FunSum / FunCount);
                emulator.UpdateRawMachineValue(ProprietaryWeightDDI, FunCount);

                if (curWorkstate == 1)
                {
                    emulator.UpdateRawMachineValue(DDIList.TotalArea, totalArea);
                    emulator.UpdateRawMachineValue(DDIList.YieldTotalMass, totalMass);
                    emulator.UpdateRawMachineValue(DDIList.EffectiveTotalTime, effectiveTotalTime);
                    if (totalArea > 0)
                    {
                        emulator.UpdateRawMachineValue(DDIList.AverageYieldMassPerArea, AverageMassPerArea);
                    }
                }
                else
                {
                    emulator.UpdateRawMachineValue(DDIList.IneffectiveTotalTime, inEffectiveTotalTime);
                }

            }
            emulator.PauseTask();
        }

        emulator.FinishTask();


        var firstEmulatedTaskData = emulator.ExportISOXML(DateTime.Now, true);
        firstEmulatedTaskData.Save();
        Assert.AreEqual(3,firstEmulatedTaskData.Data.Task.Count);//2 manually added, 1 AutoLogTask
        Assert.AreEqual(true, task1.TryGetTotalValue((ushort)DDIList.IneffectiveTotalTime, -1, out var ineffTime, firstEmulatedTaskData.Data.Device.ToList()));
        Assert.AreEqual(inEffectiveTotalTime, ineffTime);
        Assert.AreEqual(true, task1.TryGetTotalValue((ushort)DDIList.AverageYieldMassPerArea, -1, out var avgYieldMassPerArea, firstEmulatedTaskData.Data.Device.ToList()));
        Assert.AreEqual(AverageMassPerArea, avgYieldMassPerArea);

        var timElements = firstEmulatedTaskData.Data.Task.First().Time.Where(entry => entry.Type == ISOType2.Effective).ToList();
        Assert.AreEqual(4, timElements.Count);
        var timIndex = 0;
        foreach (var entry in timElements)
        {
            Assert.AreEqual(7, entry.DataLogValue.Count);
            var effTime = entry.DataLogValue.FirstOrDefault(entry => DDIUtils.ConvertDDI(entry.ProcessDataDDI) == (ushort)DDIList.EffectiveTotalTime);
            Assert.AreEqual(240 * (timIndex+1), effTime.ProcessDataValue);
            timIndex++;
        }

        var singulator = new ISOTimeListSingulator();

        var singleTimElements = singulator.SingulateTimeElements(timElements, firstEmulatedTaskData.Data.Device.ToList());
        Assert.AreEqual(4, singleTimElements.Count);
        timIndex = 0;
        foreach (var entry in singleTimElements)
        {
            Assert.AreEqual(7, entry.DataLogValue.Count);
            var funValue = entry.DataLogValue.FirstOrDefault(entry => DDIUtils.ConvertDDI(entry.ProcessDataDDI) == ProprietaryAverageDDI);
            Assert.AreEqual(timIndex * 10, funValue.ProcessDataValue);
            var effTime = entry.DataLogValue.FirstOrDefault(entry => DDIUtils.ConvertDDI(entry.ProcessDataDDI) == (ushort)DDIList.EffectiveTotalTime);
            Assert.AreEqual(240, effTime.ProcessDataValue);
            timIndex++;
        }


        var connected = ISOTimeListEnqueuer.EnqueueTimeElements(singleTimElements, firstEmulatedTaskData.Data.Device.ToList());
        Assert.AreEqual(4, connected.Count);
        foreach (var tim in timElements)
        {
            var timCompare = connected.FirstOrDefault(entry => entry.Start == tim.Start);
            Assert.IsNotNull(timCompare);
            Assert.AreEqual(7, tim.DataLogValue.Count);
            foreach( var dlv in tim.DataLogValue)
            {
                var dlvCompare = timCompare.DataLogValue.FirstOrDefault(compareDLV => compareDLV.ProcessDataDDI == dlv.ProcessDataDDI && dlv.DeviceElementIdRef == compareDLV.DeviceElementIdRef);
                Assert.IsNotNull(dlvCompare);
                Assert.AreEqual(dlvCompare.ProcessDataValue, dlv.ProcessDataValue);
            }
        }
    }


    [TestMethod]
    public void AddingTimsWithPartialDLVListLeadsToFullDLVList()
    {
        //Generate Device just for completness to call the Enqueue-Function with it
        var device = new ISODevice()
        {
            DeviceDesignator = "Test",
            DeviceId = "DVC-1"
        };
        var deviceElement = new ISODeviceElement()
        {
            DeviceElementDesignator = "Test",
            DeviceElementId = "DET-1"
        };

        device.DeviceElement.Add(deviceElement);
        for (var a = 0; a < 10; a++)
        {
            device.DeviceProcessData.Add(
                new ISODeviceProcessData()
                {
                    DeviceProcessDataDDI = DDIUtils.FormatDDI((ushort)(a + 1)),
                    DeviceProcessDataDesignator = $"{a + 1}",
                    DeviceProcessDataObjectId = (ushort)(a + 1),
                    DeviceProcessDataProperty = (int)ISODeviceProcessDataPropertyType.Setable,
                    DeviceProcessDataTriggerMethods = (int)ISODeviceProcessDataTriggerMethodType.Total,
                });
            deviceElement.DeviceObjectReference.Add(new ISODeviceObjectReference()
            {
                DeviceObjectId = (ushort)(a + 1)
            });
        }
        var deviceList = new List<ISODevice>() { device };

        //Create 10 TIM-Elements; the first having 10 DLVs, the second only 9 DLVs, ....
        var tims = new List<ISOTime>();
        for (var a = 0; a < 10; a++)
        {
            var tim = new ISOTime()
            {
                Start = DateTime.Now.AddDays(-1 * a - 1),
                Stop = DateTime.Now.AddDays(-1 * a),
                Type = ISOType2.Effective
            };
            for (var b = 10; b > a; b--)
            {
                var dlv = new ISODataLogValue()
                {
                    ProcessDataDDI = DDIUtils.FormatDDI((ushort)b),
                    ProcessDataValue = 1,
                    DeviceElementIdRef = "DET-1"
                };
                tim.DataLogValue.Add(dlv);
            }
            tims.Add(tim);
        }
        Assert.AreEqual(tims.Last().DataLogValue.Count, 1);
        Assert.AreEqual(tims.First().DataLogValue.Count, 10);
        Assert.IsTrue(tims.First().DataLogValue.All(entry => entry.ProcessDataValue == 1));
        var resultList = ISOTimeListEnqueuer.EnqueueTimeElements(tims, deviceList);
        var last = resultList.Last();
        Assert.AreEqual(last.DataLogValue.Count, 10);
        for (var a = 1; a < 11; a++)
        {
            Assert.IsTrue(last.TryGetDDIValue((ushort)a, -1, out var lastValue));
            Assert.AreEqual(lastValue, a);
        }
    }
}
