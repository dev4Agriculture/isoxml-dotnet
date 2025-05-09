using System;
using System.Linq;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.Emulator;
using Dev4Agriculture.ISO11783.ISOXML.Emulator.Generators;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class TaskControllerEmulatorTests
{
    private ISOXML _isoxml;

    private const double Mm3persTolperh = 0.0036;
    private const double SecondsPerHour = 3600;

    private TaskControllerEmulator PrepareEmulator(bool autolog)
    {
        var emulator = TaskControllerEmulator.Generate("", "Test", ISO11783TaskDataFileVersionMajor.Version4, ISO11783TaskDataFileVersionMinor.Item1, "1.1", autolog);
        _isoxml = emulator.GetTaskDataSet();

        return emulator;
    }

    private ISODevice GetChopperDevice()
    {
        var deviceGenerator = new DeviceGenerator(_isoxml, "Chopper", "1.0", new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 }, DeviceClass.Harvesters, 99, 1234);
        deviceGenerator.SetLocalization(
            "en", UnitSystem_US.METRIC
            );
        var dvpArea = new ISODeviceValuePresentation()
        {
            UnitDesignator = "ha",
            NumberOfDecimals = 1,
            Scale = (decimal)0.001
        };

        var dvpDistance = new ISODeviceValuePresentation()
        {
            UnitDesignator = "km",
            NumberOfDecimals = 1,
            Scale = (decimal)0.01
        };


        var dvpVolume = new ISODeviceValuePresentation()
        {
            UnitDesignator = "l",
            NumberOfDecimals = 1,
            Scale = (decimal)0.0001
        };


        var dvpConsumption = new ISODeviceValuePresentation()
        {
            UnitDesignator = "l/h",
            NumberOfDecimals = 1,
            Scale = (decimal)Mm3persTolperh
        };



        deviceGenerator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.ActualWorkState),
            DeviceProcessDataDesignator = "Workstate",
            DeviceProcessDataTriggerMethods = (byte)TriggerMethods.OnChange,
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet
        });

        deviceGenerator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.InstantaneousFuelConsumptionPerTime),
            DeviceProcessDataDesignator = "Fuel Consumption per Time",
            DeviceProcessDataTriggerMethods = (byte)TriggerMethods.OnTime | (byte)TriggerMethods.OnChange,
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet
        },
        valuePresentation: dvpConsumption
        );


        deviceGenerator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.EffectiveTotalDieselExhaustFluidConsumption),
            DeviceProcessDataDesignator = "AdBlue Consumption",
            DeviceProcessDataTriggerMethods = (byte)TriggerMethods.OnTime | (byte)TriggerMethods.Total,
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet | (byte)ISODeviceProcessDataPropertyType.Setable
        },
        valuePresentation: dvpVolume
        );


        deviceGenerator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.TotalFuelConsumption),
            DeviceProcessDataDesignator = "Total Fuel Consumption",
            DeviceProcessDataTriggerMethods = (byte)TriggerMethods.OnChange | (byte)TriggerMethods.Total,
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet | (byte)ISODeviceProcessDataPropertyType.Setable
        },
        valuePresentation: dvpVolume
        );



        deviceGenerator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.TotalArea),
            DeviceProcessDataDesignator = "Total Area",
            DeviceProcessDataTriggerMethods = (byte)TriggerMethods.OnChange | (byte)TriggerMethods.Total,
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet | (byte)ISODeviceProcessDataPropertyType.Setable
        },
        valuePresentation: dvpArea
        );


        return deviceGenerator.GetDevice();
    }



    [TestMethod]
    public void AddingPositionsAddsTimeElement()
    {
        var emulator = PrepareEmulator(false);

        var chopper = GetChopperDevice();


        emulator.ConnectDevice(chopper);

        emulator.StartTask(DateTime.Now, "Task");
        for (var a = 1; a < 10; a++)
        {
            emulator.AddTimeAndPosition(DateTime.Now, new ISOPosition()
            {
                PositionNorth = (decimal)(42.003 + 0.001 * a),
                PositionEast = (decimal)7.3,
                PositionStatus = ISOPositionStatus.GNSSfix
            });
        }
        emulator.FinishTask();

        var isoxml = emulator.ExportISOXML(DateTime.Now.AddSeconds(10));



        isoxml.SetFolderPath("./out/TCEmulator/simpleTask");
        isoxml.Save();

        Assert.AreEqual(isoxml.Data.Task.Count, 1);
        Assert.AreEqual(isoxml.Data.Device.Count, 1);
        var task = isoxml.Data.Task[0];
        Assert.AreEqual(task.Time.Count, 1);
        Assert.AreEqual(task.DeviceAllocation.Count, 0);

    }

    [TestMethod]
    public void AddingMachineDataAddsDeviceAllocation()
    {
        var emulator = PrepareEmulator(false);

        var chopper = GetChopperDevice();


        emulator.ConnectDevice(chopper);
        emulator.StartTask(DateTime.Now, "Task1");
        for (var a = 1; a < 10; a++)
        {
            emulator.AddTimeAndPosition(DateTime.Now, new ISOPosition()
            {
                PositionNorth = (decimal)(42.003 + 0.001 * a),
                PositionEast = (decimal)7.3,
                PositionStatus = ISOPositionStatus.GNSSfix
            });
            emulator.AddRawValueToMachineValue(DDIList.ActualWorkState, a % 2);
        }
        emulator.FinishTask();

        var isoxml = emulator.GetTaskDataSet();

        isoxml.SetFolderPath("./out/TCEmulator/simpleTaskWithRawMachineData");
        isoxml.Save();


        Assert.AreEqual(isoxml.Data.Task.Count, 1);
        Assert.AreEqual(isoxml.Data.Device.Count, 1);
        var task = isoxml.Data.Task[0];
        Assert.AreEqual(task.Time.Count, 1);
        Assert.AreEqual(task.DeviceAllocation.Count, 1);

    }

    [TestMethod]
    public void AddingFormatedMachineDataIsConvertedCorrectly()
    {
        var emulator = PrepareEmulator(false);

        var chopper = GetChopperDevice();


        emulator.ConnectDevice(chopper);
        emulator.StartTask(DateTime.Now, "TaskWithMachines");
        for (var a = 1; a < 10; a++)
        {
            emulator.AddTimeAndPosition(DateTime.Now, new ISOPosition()
            {
                PositionNorth = (decimal)(42.003 + 0.001 * a),
                PositionEast = (decimal)7.3,
                PositionStatus = ISOPositionStatus.GNSSfix
            });
            emulator.UpdateMachineValue(DDIList.InstantaneousFuelConsumptionPerTime, a);
            if (a % 2 == 0)
            {
                emulator.AddRawValueToMachineValue(DDIList.TotalFuelConsumption, 2);

            }
        }
        emulator.FinishTask();

        var isoxml = emulator.ExportISOXML(DateTime.Now.AddSeconds(400));

        isoxml.SetFolderPath("./out/TCEmulator/simpleTask");
        isoxml.Save();

        Assert.AreEqual(isoxml.Data.Task.Count, 1);
        Assert.AreEqual(isoxml.Data.Device.Count, 1);
        var task = isoxml.Data.Task[0];
        Assert.AreEqual(task.Time.Count, 1);
        Assert.AreEqual(task.DeviceAllocation.Count, 1);
        Assert.AreEqual(task.TimeLogs.Count, 1);
        var timeLog = task.TimeLogs[0];
        var currentFuelIndex = timeLog.Header.GetDDIIndex((ushort)DDIList.InstantaneousFuelConsumptionPerTime, -1);
        var totalFuelIndex = timeLog.Header.GetDDIIndex((ushort)DDIList.TotalFuelConsumption, -1);

        for (var a = 1; a <= task.TimeLogs.Count; a++)
        {
            Assert.AreEqual(timeLog.Entries[a - 1].NumberOfEntries, a % 2 == 0 ? 2 : 1);
            Assert.AreEqual(timeLog.Entries[a - 1].Entries[currentFuelIndex].Value, (int)Math.Round(a / Mm3persTolperh));
            if (a % 2 == 0)
            {
                Assert.AreEqual(timeLog.Entries[a - 1].Entries[totalFuelIndex].Value, a);
            }
            else
            {
                Assert.AreEqual(timeLog.Entries[a - 1].Entries[totalFuelIndex].IsSet, false);
            }
        }
    }


    [TestMethod]
    public void CanStartATaskMultipleTimes()
    {
        var emulator = PrepareEmulator(true);

        var chopper = GetChopperDevice();


        emulator.ConnectDevice(chopper);
        emulator.StartTask(DateTime.Now, "First Task");
        for (var a = 1; a < 400; a++)
        {
            emulator.AddTimeAndPosition(DateTime.Now.AddSeconds(a), new ISOPosition()
            {
                PositionNorth = (decimal)(42.003 + 0.001 * a),
                PositionEast = (decimal)7.3,
                PositionStatus = ISOPositionStatus.GNSSfix
            });
            emulator.UpdateMachineValue(DDIList.InstantaneousFuelConsumptionPerTime, a % 100);
            if (a % 2 == 0)
            {
                emulator.AddValueToMachineValue(DDIList.TotalFuelConsumption, a % 100 * 2 / SecondsPerHour);

            }
            if (a % 100 == 0)
            {
                emulator.StartTask(DateTime.Now.AddSeconds(a), "Task " + a);
            }
            if (a % 90 == 0)
            {
                emulator.PauseTask();
            }
        }
        emulator.FinishTask();

        var isoxml = emulator.ExportISOXML(DateTime.Now.AddSeconds(400));

        isoxml.SetFolderPath("./out/TCEmulator/MultiTask");
        isoxml.Save();

        var loadedISOXML = ISOXML.Load("./out/TCEmulator/MultiTask");

        Assert.AreEqual(loadedISOXML.Data.Task.Count, 5); //4 Tasks and one AutoLog Task
        Assert.AreEqual(loadedISOXML.Data.Device.Count, 1);

        //Check AutoLogTask
        var task = loadedISOXML.Data.Task[0];
        Assert.AreEqual(task.TaskDesignator, "AUTOLOG");
        Assert.AreEqual(task.Time.Count, 4);
        Assert.AreEqual(task.DeviceAllocation.Count, 4);
        Assert.AreEqual(task.TimeLogs.Count, 4);

        Assert.AreEqual(task.TryGetTotalValue((ushort)DDIList.TotalFuelConsumption, -1, out var totalValue, loadedISOXML.Data.Device.ToList()), true);
        Assert.AreEqual(totalValue, 21664);
    }

}
