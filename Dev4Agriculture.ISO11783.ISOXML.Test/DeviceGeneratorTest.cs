using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.Emulator;
using Dev4Agriculture.ISO11783.ISOXML.Emulator.Generators;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;
[TestClass]
public class DeviceGeneratorTest
{

    [TestMethod]
    public void CanGenerateDeviceWithNumericSerialNo()
    {
        var path = "./out/TCEmulator/DeviceGenerator/Base";
        var tcEmulator = TaskControllerEmulator.Generate(path, "dev4Agriculture", ISO11783TaskDataFileVersionMajor.Version4, ISO11783TaskDataFileVersionMinor.Item2, "0.1.1");
        var generator = new DeviceGenerator(tcEmulator.GetTaskDataSet(), "My Chopper", "0.1.0", new byte[] { 1, 2, 3, 4, 5, 6, 7 }, DeviceClass.Harvesters, 921, 12345);
        var dvc = generator.GetDevice();
        Assert.IsNotNull(dvc);
        Assert.AreEqual(dvc.DeviceDesignator, "My Chopper");
        Assert.AreEqual(dvc.DeviceSoftwareVersion, "0.1.0");
        Assert.AreEqual(dvc.DeviceSerialNumber, "12345");
        var clientName = dvc.ClientNameParsed;
        Assert.IsNotNull(clientName);
        Assert.AreEqual(clientName.DeviceClass, DeviceClass.Harvesters);
        Assert.AreEqual(clientName.SerialNo, 12345);

    }
    [TestMethod]
    public void CanGenerateDeviceWithStringSerialNo()
    {
        var path = "./out/TCEmulator/DeviceGenerator/Base";
        var tcEmulator = TaskControllerEmulator.Generate(path, "dev4Agriculture", ISO11783TaskDataFileVersionMajor.Version4, ISO11783TaskDataFileVersionMinor.Item2, "0.1.1");
        var generator = new DeviceGenerator(tcEmulator.GetTaskDataSet(), "My Chopper", "0.1.0", new byte[] { 1, 2, 3, 4, 5, 6, 7 }, DeviceClass.Harvesters, 921, "AB123");
        var dvc = generator.GetDevice();
        Assert.IsNotNull(dvc);
        Assert.AreEqual(dvc.DeviceDesignator, "My Chopper");
        Assert.AreEqual(dvc.DeviceSoftwareVersion, "0.1.0");
        Assert.AreEqual(dvc.DeviceSerialNumber, "AB123");
        var clientName = dvc.ClientNameParsed;
        Assert.IsNotNull(clientName);
        Assert.AreEqual(clientName.DeviceClass, DeviceClass.Harvesters);
        Assert.AreEqual(clientName.SerialNo, 1011123);

    }

    [TestMethod]
    public void CanGenerateDeviceWithStringAndLongSerialNo()
    {
        var path = "./out/TCEmulator/DeviceGenerator/Base";
        var tcEmulator = TaskControllerEmulator.Generate(path, "dev4Agriculture", ISO11783TaskDataFileVersionMajor.Version4, ISO11783TaskDataFileVersionMinor.Item2, "0.1.1");
        var generator = new DeviceGenerator(tcEmulator.GetTaskDataSet(), "My Chopper", "0.1.0", new byte[] { 1, 2, 3, 4, 5, 6, 7 }, DeviceClass.Harvesters, 921, "AB123456", 1654321);
        var dvc = generator.GetDevice();
        Assert.IsNotNull(dvc);
        Assert.AreEqual(dvc.DeviceDesignator, "My Chopper");
        Assert.AreEqual(dvc.DeviceSoftwareVersion, "0.1.0");
        Assert.AreEqual(dvc.DeviceSerialNumber, "AB123456");
        var clientName = dvc.ClientNameParsed;
        Assert.IsNotNull(clientName);
        Assert.AreEqual(clientName.DeviceClass, DeviceClass.Harvesters);
        Assert.AreEqual(clientName.SerialNo, 1654321);

    }


    [TestMethod]
    public void AddDeviceValuePresentationWithDPD()
    {
        var path = "./out/TCEmulator/DeviceGenerator/Base";
        var tcEmulator = TaskControllerEmulator.Generate(path, "dev4Agriculture", ISO11783TaskDataFileVersionMajor.Version4, ISO11783TaskDataFileVersionMinor.Item2, "0.1.1");
        var generator = new DeviceGenerator(tcEmulator.GetTaskDataSet(), "My Chopper", "0.1.0", new byte[] { 1, 2, 3, 4, 5, 6, 7 }, DeviceClass.Harvesters, 921, "AB123456", 1654321);
        var dvp_area = new ISODeviceValuePresentation()
        {
            DeviceValuePresentationObjectId = generator.NextDeviceValuePresentationId(),
            UnitDesignator = "ha",
            NumberOfDecimals = 1,
            Offset = 0,
            Scale = (decimal)0.001
        };
        generator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.TotalArea),
            DeviceProcessDataDesignator = "Flaeche",
            DeviceProcessDataObjectId = generator.NextDeviceProcessDataId(),
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.Setable,
            DeviceProcessDataTriggerMethods = (byte)ISODeviceProcessDataTriggerMethodType.Total,
        }, null, dvp_area);
        var dvp_temperatur = new ISODeviceValuePresentation()
        {
            DeviceValuePresentationObjectId = generator.NextDeviceValuePresentationId(),
            UnitDesignator = "°C",
            NumberOfDecimals = 1,
            Offset = 18300,
            Scale = (decimal)100
        };
        generator.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.ActualCoolingFluidTemperature),
            DeviceProcessDataDesignator = "MotorTemperatur",
            DeviceProcessDataObjectId = generator.NextDeviceProcessDataId(),
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet,
            DeviceProcessDataTriggerMethods = (byte)ISODeviceProcessDataTriggerMethodType.OnChange,
        }, null, dvp_temperatur);


        var dvc = generator.GetDevice();
        Assert.IsNotNull(dvc);
        Assert.AreEqual(2, dvc.DeviceValuePresentation.Count);
        Assert.AreEqual(2, dvc.DeviceProcessData.Count);
        Assert.AreEqual(1, dvc.DeviceElement.Count);
        Assert.AreEqual(0, dvc.DeviceProperty.Count);
        Assert.AreEqual("DET-1", dvc.DeviceElement[0].DeviceElementId);
        Assert.AreEqual("DVC-1", dvc.DeviceId);
    }
}
