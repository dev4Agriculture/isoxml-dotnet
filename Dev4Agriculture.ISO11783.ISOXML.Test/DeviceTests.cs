using System;
using System.IO;
using System.Linq;
using de.dev4Agriculture.ISOXML.DDI;
//using System.IO;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class DeviceTests
{
    [TestMethod]
    public void ParseDevice()
    {
        var path = "./testdata/devices/Device_description.xml";
        var result = ISOXML.Load(path);

        result.Messages.ForEach(msg =>
        {
            Console.WriteLine(msg.Title);
        });
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(result.Data.Device.Count, 1);
        Assert.AreEqual(result.Data.Device[0].DeviceElement.Count, 15);
        Assert.AreEqual(0, result.Messages.Count);

    }

    [TestMethod]
    public void DeviceWithBrokenScaleUnitShallCreateWarning()
    {
        var path = "./testdata/devices/Device_Description_Unit.xml";
        var result = ISOXML.Load(path);
        result.Messages.ForEach(msg => Console.WriteLine(msg.Title));
        Assert.AreEqual(1, result.Messages.Count);
    }

    [TestMethod]
    public void CommentsShallNotThrowErrors()
    {
        var path = "./testdata/Structure/FileWithComment.XML";
        var result = ISOXML.Load(path);
        result.Messages.ForEach(msg => Console.WriteLine(msg.Title));
        Assert.AreEqual(1, result.Messages.Count);
        Assert.AreEqual(ResultMessageType.Warning, result.Messages[0].Type);
    }

    [TestMethod]
    public void CanSetDeviceNameFromClientName()
    {
        var path_out = "./out/dvc/clientName";
        var result = ISOXML.Create(path_out);
        result.Data.Device.Add(new TaskFile.ISODevice());
        result.IdTable.AddObjectAndAssignIdIfNone(result.Data.Device[0]);
        var clientName = new ClientName()
        {
            ManufacturerCode = 339,
            DeviceClass = DeviceClass.SecondarySoilTillage
        };
        result.Data.Device[0].ClientNAME = clientName.ToArray();
        result.SetFolderPath(path_out);
        result.Save();
        result = ISOXML.Load(path_out);
        var clientName_Compare = new ClientName(result.Data.Device[0].ClientNAME);
        Assert.AreEqual(clientName_Compare.ManufacturerCode, 339);
        Assert.AreEqual(clientName_Compare.DeviceClass, DeviceClass.SecondarySoilTillage);

    }


    [TestMethod]
    public void CanLoadJustDeviceDescripton()
    {
        var path = "./testdata/devices/DeviceOnly.xml";
        var text = File.ReadAllText(path);
        var result = ISOXML.ParseFromXMLString(text);
        Assert.AreEqual(result.Messages.Count, 0);
        Assert.AreEqual(result.Data.Device.Count, 1);
        var dvc = result.Data.Device[0];
        Assert.AreEqual(dvc.DeviceElement.Count, 15);
    }




    [TestMethod]
    public void CanListTotals()
    {
        var path = "./testdata/devices/Device_Description_Unit.xml";
        var result = ISOXML.Load(path);
        Assert.AreEqual(6, result.Data.Device[0].GetAllTotalsProcessData().Count());
    }

    [TestMethod]
    public void CanCheckTriggerMethods()
    {
        var path = "./testdata/devices/DeviceOnly.xml";
        var text = File.ReadAllText(path);
        var result = ISOXML.ParseFromXMLString(text);
        Assert.IsTrue(result.Data.Device[0].DeviceProcessData.First(dpd => DDIUtils.ConvertDDI(dpd.DeviceProcessDataDDI) == (ushort)DDIList.ActualWorkState).IsOnChange());
        Assert.IsTrue(result.Data.Device[0].DeviceProcessData.First(dpd => DDIUtils.ConvertDDI(dpd.DeviceProcessDataDDI) == (ushort)DDIList.SetpointCountPerAreaApplicationRate).IsOnTime());
        Assert.IsTrue(result.Data.Device[0].DeviceProcessData.First(dpd => DDIUtils.ConvertDDI(dpd.DeviceProcessDataDDI) == (ushort)DDIList.TotalArea).IsTotal());
    }

}

