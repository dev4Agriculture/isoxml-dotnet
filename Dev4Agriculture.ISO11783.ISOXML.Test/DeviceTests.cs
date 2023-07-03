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

    [TestMethod]
    public void CanReadMultipleDevices()
    {
        var textInput = "<ISO11783_TaskData VersionMajor=\"4\" VersionMinor=\"1\" ManagementSoftwareManufacturer=\"\" ManagementSoftwareVersion=\"\" TaskControllerManufacturer=\"KRONE Smart Connect\" TaskControllerVersion=\"1.104.49.0\" DataTransferOrigin=\"2\"><DVC A=\"DVC-1\" B=\"AXION__A4001234\" C=\"01.00.00\" D=\"A00086000D017655\" F=\"00001B001A0001\" G=\"FF0000004F6E65\">  <DET A=\"DET-1\" B=\"1\" C=\"1\" D=\"Base\" E=\"0\" F=\"0\">    <DOR A=\"11\" />    <DOR A=\"12\" />    <DOR A=\"23\" />    <DOR A=\"35\" />  </DET>  <DET A=\"DET-2\" B=\"2\" C=\"2\" D=\"Engine\" E=\"1\" F=\"1\">    <DOR A=\"14\" />    <DOR A=\"28\" />  </DET>  <DET A=\"DET-3\" B=\"3\" C=\"2\" D=\"Powertrain\" E=\"2\" F=\"1\">    <DOR A=\"30\" />    <DOR A=\"31\" />  </DET>  <DET A=\"DET-4\" B=\"4\" C=\"2\" D=\"Performance monitor\" E=\"3\" F=\"1\">    <DOR A=\"13\" />    <DOR A=\"18\" />    <DOR A=\"19\" />  </DET>  <DET A=\"DET-5\" B=\"5\" C=\"2\" D=\"Power take offs\" E=\"4\" F=\"1\">    <DOR A=\"26\" />    <DOR A=\"27\" />  </DET>  <DET A=\"DET-6\" B=\"6\" C=\"7\" D=\"GPS\" E=\"5\" F=\"1\">    <DOR A=\"1000\" />    <DOR A=\"1001\" />    <DOR A=\"1002\" />  </DET>  <DET A=\"DET-7\" B=\"7\" C=\"6\" D=\"Rear Connector\" E=\"6\" F=\"1\">    <DOR A=\"2000\" />    <DOR A=\"2001\" />    <DOR A=\"2003\" />  </DET>  <DET A=\"DET-8\" B=\"8\" C=\"6\" D=\"Front Connector\" E=\"7\" F=\"1\">    <DOR A=\"2004\" />    <DOR A=\"2005\" />    <DOR A=\"2007\" />  </DET>  <DPD A=\"11\" B=\"0077\" C=\"2\" D=\"17\" E=\"Total operative time\" F=\"101\" />  <DPD A=\"12\" B=\"0078\" C=\"2\" D=\"17\" E=\"Total ineffective time\" F=\"101\" />  <DPD A=\"13\" B=\"008D\" C=\"1\" D=\"9\" E=\"Working status\" F=\"112\" />  <DPD A=\"14\" B=\"0094\" C=\"2\" D=\"17\" E=\"Fuel consumption\" F=\"102\" />  <DPD A=\"18\" B=\"013C\" C=\"2\" D=\"17\" E=\"Effective fuel consumption\" F=\"102\" />  <DPD A=\"19\" B=\"013D\" C=\"2\" D=\"17\" E=\"Ineffective fuel consumption\" F=\"102\" />  <DPD A=\"23\" B=\"00C0\" C=\"1\" D=\"1\" E=\"Exterior temperature\" F=\"113\" />  <DPD A=\"26\" B=\"014F\" C=\"2\" D=\"17\" E=\"Front power take off\" F=\"101\" />  <DPD A=\"27\" B=\"0150\" C=\"2\" D=\"17\" E=\"Rear power take off\" F=\"101\" />  <DPD A=\"28\" B=\"018A\" C=\"1\" D=\"1\" E=\"Current fuel level\" F=\"106\" />  <DPD A=\"30\" B=\"0075\" C=\"2\" D=\"17\" E=\"Effective distance\" F=\"107\" />  <DPD A=\"31\" B=\"0076\" C=\"2\" D=\"17\" E=\"Ineffective distance\" F=\"107\" />  <DPD A=\"35\" B=\"DFFF\" C=\"0\" D=\"31\" E=\"Process data\" />  <DPD A=\"1000\" B=\"0086\" C=\"0\" D=\"0\" />  <DPD A=\"1001\" B=\"0087\" C=\"0\" D=\"0\" />  <DPD A=\"1002\" B=\"0088\" C=\"0\" D=\"0\" />  <DPD A=\"2000\" B=\"0086\" C=\"0\" D=\"0\" />  <DPD A=\"2001\" B=\"0087\" C=\"0\" D=\"0\" />  <DPD A=\"2003\" B=\"009D\" C=\"0\" D=\"0\" />  <DPD A=\"2004\" B=\"0086\" C=\"0\" D=\"0\" />  <DPD A=\"2005\" B=\"0087\" C=\"0\" D=\"0\" />  <DPD A=\"2007\" B=\"009D\" C=\"0\" D=\"0\" />  <DVP A=\"100\" B=\"0\" C=\"0.0001\" D=\"2\" E=\"ha\" />  <DVP A=\"101\" B=\"0\" C=\"0.000277778\" D=\"2\" E=\"h\" />  <DVP A=\"102\" B=\"0\" C=\"0.001\" D=\"1\" E=\"l\" />  <DVP A=\"103\" B=\"0\" C=\"0.0036\" D=\"2\" E=\"l/h\" />  <DVP A=\"104\" B=\"0\" C=\"0.01\" D=\"2\" E=\"l/ha\" />  <DVP A=\"105\" B=\"0\" C=\"0.360000014\" D=\"2\" E=\"ha/h\" />  <DVP A=\"106\" B=\"0\" C=\"0.001\" D=\"2\" E=\"l\" />  <DVP A=\"107\" B=\"0\" C=\"0.000001\" D=\"2\" E=\"km\" />  <DVP A=\"108\" B=\"0\" C=\"0.0036\" D=\"2\" E=\"km/h\" />  <DVP A=\"109\" B=\"0\" C=\"0.001\" D=\"2\" E=\"m\" />  <DVP A=\"110\" B=\"0\" C=\"1.0\" D=\"2\" E=\"rpm\" />  <DVP A=\"111\" B=\"0\" C=\"1.0\" D=\"0\" E=\"%\" />  <DVP A=\"112\" B=\"0\" C=\"1.0\" D=\"0\" />  <DVP A=\"113\" B=\"-273150\" C=\"0.001\" D=\"0\" E=\"°C\" /></DVC><DVC A=\"DVC-3\" B=\"ME_Slurry_Tanker\" C=\"MEST_MIDI3 V03.07.00.06.1\" D=\"A03280000C40FEEB\" E=\"A26056\" F=\"34453834335453\" G=\"FF0000005F6E65\">  <DET A=\"DET-6\" B=\"1\" C=\"1\" D=\"Implement\" E=\"0\" F=\"0\">    <DOR A=\"105\" />    <DOR A=\"106\" />    <DOR A=\"133\" />    <DOR A=\"169\" />    <DOR A=\"170\" />    <DOR A=\"171\" />    <DOR A=\"172\" />  </DET>  <DET A=\"DET-7\" B=\"2\" C=\"6\" D=\"Connector 1\" E=\"1\" F=\"1\">    <DOR A=\"308\" />    <DOR A=\"302\" />    <DOR A=\"303\" />    <DOR A=\"304\" />  </DET>  <DET A=\"DET-8\" B=\"3\" C=\"2\" D=\"Boom\" E=\"2\" F=\"1\">    <DOR A=\"100\" />    <DOR A=\"101\" />    <DOR A=\"102\" />    <DOR A=\"103\" />    <DOR A=\"104\" />    <DOR A=\"108\" />    <DOR A=\"110\" />    <DOR A=\"112\" />    <DOR A=\"113\" />    <DOR A=\"114\" />    <DOR A=\"115\" />    <DOR A=\"117\" />    <DOR A=\"133\" />    <DOR A=\"134\" />    <DOR A=\"151\" />    <DOR A=\"297\" />    <DOR A=\"298\" />    <DOR A=\"299\" />    <DOR A=\"301\" />  </DET>  <DET A=\"DET-9\" B=\"4\" C=\"3\" D=\"Tank\" E=\"3\" F=\"3\">    <DOR A=\"109\" />    <DOR A=\"300\" />  </DET>  <DET A=\"DET-10\" B=\"11\" C=\"4\" D=\"Section 1\" E=\"10\" F=\"3\">    <DOR A=\"310\" />    <DOR A=\"311\" />    <DOR A=\"312\" />    <DOR A=\"313\" />  </DET>  <DET A=\"DET-11\" B=\"12\" C=\"4\" D=\"Section 2\" E=\"11\" F=\"3\">    <DOR A=\"320\" />    <DOR A=\"321\" />    <DOR A=\"322\" />    <DOR A=\"323\" />  </DET>  <DET A=\"DET-12\" B=\"13\" C=\"4\" D=\"Section 3\" E=\"12\" F=\"3\">    <DOR A=\"330\" />    <DOR A=\"331\" />    <DOR A=\"332\" />    <DOR A=\"333\" />  </DET>  <DET A=\"DET-13\" B=\"14\" C=\"4\" D=\"Section 4\" E=\"13\" F=\"3\">    <DOR A=\"340\" />    <DOR A=\"341\" />    <DOR A=\"342\" />    <DOR A=\"343\" />  </DET>  <DET A=\"DET-14\" B=\"15\" C=\"4\" D=\"Section 5\" E=\"14\" F=\"3\">    <DOR A=\"350\" />    <DOR A=\"351\" />    <DOR A=\"352\" />    <DOR A=\"353\" />  </DET>  <DET A=\"DET-15\" B=\"16\" C=\"4\" D=\"Section 6\" E=\"15\" F=\"3\">    <DOR A=\"360\" />    <DOR A=\"361\" />    <DOR A=\"362\" />    <DOR A=\"363\" />  </DET>  <DET A=\"DET-16\" B=\"17\" C=\"4\" D=\"Section 7\" E=\"16\" F=\"3\">    <DOR A=\"370\" />    <DOR A=\"371\" />    <DOR A=\"372\" />    <DOR A=\"373\" />  </DET>  <DET A=\"DET-17\" B=\"18\" C=\"4\" D=\"Section 8\" E=\"17\" F=\"3\">    <DOR A=\"380\" />    <DOR A=\"381\" />    <DOR A=\"382\" />    <DOR A=\"383\" />  </DET>  <DET A=\"DET-18\" B=\"19\" C=\"4\" D=\"Section 9\" E=\"18\" F=\"3\">    <DOR A=\"390\" />    <DOR A=\"391\" />    <DOR A=\"392\" />    <DOR A=\"393\" />  </DET>  <DET A=\"DET-19\" B=\"20\" C=\"4\" D=\"Section 10\" E=\"19\" F=\"3\">    <DOR A=\"400\" />    <DOR A=\"401\" />    <DOR A=\"402\" />    <DOR A=\"403\" />  </DET>  <DET A=\"DET-20\" B=\"21\" C=\"4\" D=\"Section 11\" E=\"20\" F=\"3\">    <DOR A=\"410\" />    <DOR A=\"411\" />    <DOR A=\"412\" />    <DOR A=\"413\" />  </DET>  <DET A=\"DET-21\" B=\"22\" C=\"4\" D=\"Section 12\" E=\"21\" F=\"3\">    <DOR A=\"420\" />    <DOR A=\"421\" />    <DOR A=\"422\" />    <DOR A=\"423\" />  </DET>  <DPT A=\"297\" B=\"0086\" C=\"0\" E=\"205\" />  <DPT A=\"298\" B=\"0087\" C=\"0\" E=\"205\" />  <DPT A=\"299\" B=\"0088\" C=\"0\" E=\"205\" />  <DPT A=\"300\" B=\"0049\" C=\"18927000\" D=\"Tank max. volume\" E=\"207\" />  <DPT A=\"301\" B=\"0046\" C=\"18000\" D=\"Max working width\" E=\"205\" />  <DPT A=\"302\" B=\"0086\" C=\"8000\" D=\"Connector X Offset\" E=\"205\" />  <DPT A=\"303\" B=\"0087\" C=\"0\" D=\"Connector Y Offset\" E=\"205\" />  <DPT A=\"304\" B=\"0088\" C=\"0\" D=\"Connector Z Offset\" E=\"205\" />  <DPT A=\"305\" B=\"0086\" C=\"0\" E=\"205\" />  <DPT A=\"306\" B=\"0087\" C=\"0\" E=\"205\" />  <DPT A=\"307\" B=\"0088\" C=\"0\" E=\"205\" />  <DPT A=\"308\" B=\"009D\" C=\"8\" />  <DPT A=\"309\" B=\"009D\" C=\"8\" />  <DPT A=\"310\" B=\"0046\" C=\"1500\" D=\"Section width 1\" E=\"205\" />  <DPT A=\"311\" B=\"0086\" C=\"0\" D=\"X Offset Section 1\" E=\"205\" />  <DPT A=\"312\" B=\"0087\" C=\"-8250\" D=\"Y Offset Section 1\" E=\"205\" />  <DPT A=\"313\" B=\"0088\" C=\"0\" D=\"Z Offset Section 1\" E=\"205\" />  <DPT A=\"320\" B=\"0046\" C=\"1500\" D=\"Section width 2\" E=\"205\" />  <DPT A=\"321\" B=\"0086\" C=\"0\" D=\"X Offset Section 2\" E=\"205\" />  <DPT A=\"322\" B=\"0087\" C=\"-6750\" D=\"Y Offset Section 2\" E=\"205\" />  <DPT A=\"323\" B=\"0088\" C=\"0\" D=\"Z Offset Section 2\" E=\"205\" />  <DPT A=\"330\" B=\"0046\" C=\"1500\" D=\"Section width 3\" E=\"205\" />  <DPT A=\"331\" B=\"0086\" C=\"0\" D=\"X Offset Section 3\" E=\"205\" />  <DPT A=\"332\" B=\"0087\" C=\"-5250\" D=\"Y Offset Section 3\" E=\"205\" />  <DPT A=\"333\" B=\"0088\" C=\"0\" D=\"Z Offset Section 3\" E=\"205\" />  <DPT A=\"340\" B=\"0046\" C=\"1500\" D=\"Section width 4\" E=\"205\" />  <DPT A=\"341\" B=\"0086\" C=\"0\" D=\"X Offset Section 4\" E=\"205\" />  <DPT A=\"342\" B=\"0087\" C=\"-3750\" D=\"Y Offset Section 4\" E=\"205\" />  <DPT A=\"343\" B=\"0088\" C=\"0\" D=\"Z Offset Section 4\" E=\"205\" />  <DPT A=\"350\" B=\"0046\" C=\"1500\" D=\"Section width 5\" E=\"205\" />  <DPT A=\"351\" B=\"0086\" C=\"0\" D=\"X Offset Section 5\" E=\"205\" />  <DPT A=\"352\" B=\"0087\" C=\"-2250\" D=\"Y Offset Section 5\" E=\"205\" />  <DPT A=\"353\" B=\"0088\" C=\"0\" D=\"Z Offset Section 5\" E=\"205\" />  <DPT A=\"360\" B=\"0046\" C=\"1500\" D=\"Section width 6\" E=\"205\" />  <DPT A=\"361\" B=\"0086\" C=\"0\" D=\"X Offset Section 6\" E=\"205\" />  <DPT A=\"362\" B=\"0087\" C=\"-750\" D=\"Y Offset Section 6\" E=\"205\" />  <DPT A=\"363\" B=\"0088\" C=\"0\" D=\"Z Offset Section 6\" E=\"205\" />  <DPT A=\"370\" B=\"0046\" C=\"1500\" D=\"Section width 7\" E=\"205\" />  <DPT A=\"371\" B=\"0086\" C=\"0\" D=\"X Offset Section 7\" E=\"205\" />  <DPT A=\"372\" B=\"0087\" C=\"750\" D=\"Y Offset Section 7\" E=\"205\" />  <DPT A=\"373\" B=\"0088\" C=\"0\" D=\"Z Offset Section 7\" E=\"205\" />  <DPT A=\"380\" B=\"0046\" C=\"1500\" D=\"Section width 8\" E=\"205\" />  <DPT A=\"381\" B=\"0086\" C=\"0\" D=\"X Offset Section 8\" E=\"205\" />  <DPT A=\"382\" B=\"0087\" C=\"2250\" D=\"Y Offset Section 8\" E=\"205\" />  <DPT A=\"383\" B=\"0088\" C=\"0\" D=\"Z Offset Section 8\" E=\"205\" />  <DPT A=\"390\" B=\"0046\" C=\"1500\" D=\"Section width 9\" E=\"205\" />  <DPT A=\"391\" B=\"0086\" C=\"0\" D=\"X Offset Section 9\" E=\"205\" />  <DPT A=\"392\" B=\"0087\" C=\"3750\" D=\"Y Offset Section 9\" E=\"205\" />  <DPT A=\"393\" B=\"0088\" C=\"0\" D=\"Z Offset Section 9\" E=\"205\" />  <DPT A=\"400\" B=\"0046\" C=\"1500\" D=\"Section width 10\" E=\"205\" />  <DPT A=\"401\" B=\"0086\" C=\"0\" D=\"X Offset Section 10\" E=\"205\" />  <DPT A=\"402\" B=\"0087\" C=\"5250\" D=\"Y Offset Section 10\" E=\"205\" />  <DPT A=\"403\" B=\"0088\" C=\"0\" D=\"Z Offset Section 10\" E=\"205\" />  <DPT A=\"410\" B=\"0046\" C=\"1500\" D=\"Section width 11\" E=\"205\" />  <DPT A=\"411\" B=\"0086\" C=\"0\" D=\"X Offset Section 11\" E=\"205\" />  <DPT A=\"412\" B=\"0087\" C=\"6750\" D=\"Y Offset Section 11\" E=\"205\" />  <DPT A=\"413\" B=\"0088\" C=\"0\" D=\"Z Offset Section 11\" E=\"205\" />  <DPT A=\"420\" B=\"0046\" C=\"1500\" D=\"Section width 12\" E=\"205\" />  <DPT A=\"421\" B=\"0086\" C=\"0\" D=\"X Offset Section 12\" E=\"205\" />  <DPT A=\"422\" B=\"0087\" C=\"8250\" D=\"Y Offset Section 12\" E=\"205\" />  <DPT A=\"423\" B=\"0088\" C=\"0\" D=\"Z Offset Section 12\" E=\"205\" />  <DPD A=\"100\" B=\"0001\" C=\"2\" D=\"1\" E=\"Application rate\" F=\"200\" />  <DPD A=\"101\" B=\"0002\" C=\"1\" D=\"1\" E=\"Current rate\" F=\"200\" />  <DPD A=\"102\" B=\"0050\" C=\"3\" D=\"17\" E=\"Volume counter\" F=\"201\" />  <DPD A=\"103\" B=\"0074\" C=\"3\" D=\"17\" E=\"Surface counter\" F=\"202\" />  <DPD A=\"104\" B=\"0075\" C=\"3\" D=\"17\" E=\"Distance counter\" F=\"203\" />  <DPD A=\"105\" B=\"0077\" C=\"3\" D=\"17\" E=\"Time counter\" F=\"204\" />  <DPD A=\"106\" B=\"DFFF\" C=\"1\" D=\"31\" E=\"Default DDI\" F=\"65534\" />  <DPD A=\"107\" B=\"0134\" C=\"1\" D=\"1\" F=\"206\" />  <DPD A=\"108\" B=\"0025\" C=\"1\" D=\"1\" E=\"Current flow\" F=\"208\" />  <DPD A=\"109\" B=\"0048\" C=\"1\" D=\"1\" E=\"Current tank volume\" F=\"209\" />  <DPD A=\"110\" B=\"008C\" C=\"2\" D=\"0\" E=\"Application rate deviation\" F=\"206\" />  <DPD A=\"111\" B=\"0121\" C=\"2\" D=\"0\" E=\"State\" F=\"65534\" />  <DPD A=\"112\" B=\"00A0\" C=\"2\" D=\"9\" E=\"Autom. Section switching\" F=\"65534\" />  <DPD A=\"113\" B=\"0043\" C=\"1\" D=\"1\" E=\"Current working width\" F=\"205\" />  <DPD A=\"114\" B=\"00CD\" C=\"1\" D=\"9\" E=\"Turn On Delay\" F=\"213\" />  <DPD A=\"115\" B=\"00CE\" C=\"1\" D=\"9\" E=\"Turn Off Delay\" F=\"213\" />  <DPD A=\"117\" B=\"00A1\" C=\"0\" D=\"9\" E=\"Actual State .1-.16\" F=\"65534\" />  <DPD A=\"118\" B=\"00A2\" C=\"0\" D=\"9\" E=\"Actual State .17-.32\" F=\"65534\" />  <DPD A=\"133\" B=\"008D\" C=\"0\" D=\"9\" E=\"Actual state\" F=\"65534\" />  <DPD A=\"134\" B=\"0122\" C=\"2\" D=\"0\" E=\"Setpoint State .1-.16\" F=\"65534\" />  <DPD A=\"135\" B=\"0123\" C=\"2\" D=\"0\" E=\"Setpoint State .17-.32\" F=\"65534\" />  <DPD A=\"151\" B=\"009E\" C=\"2\" D=\"9\" F=\"65534\" />  <DPD A=\"152\" B=\"01B0\" C=\"2\" D=\"1\" E=\"Setpoint Rate of Nitrogen\" F=\"214\" />  <DPD A=\"153\" B=\"01B1\" C=\"1\" D=\"1\" E=\"Actual Rate of Nitrogen\" F=\"214\" />  <DPD A=\"154\" B=\"01B4\" C=\"2\" D=\"1\" E=\"Setpoint Rate of Ammonium\" F=\"214\" />  <DPD A=\"155\" B=\"01B5\" C=\"1\" D=\"1\" E=\"Actual Rate of Ammonium\" F=\"214\" />  <DPD A=\"156\" B=\"01B8\" C=\"2\" D=\"1\" E=\"Setpoint Rate of Phosphor\" F=\"214\" />  <DPD A=\"157\" B=\"01B9\" C=\"1\" D=\"1\" E=\"Actual Rate of Phosphor\" F=\"214\" />  <DPD A=\"158\" B=\"01BC\" C=\"2\" D=\"1\" E=\"Setpoint Rate of Potassium\" F=\"214\" />  <DPD A=\"159\" B=\"01BD\" C=\"1\" D=\"1\" E=\"Actual Rate of Potassium\" F=\"214\" />  <DPD A=\"160\" B=\"01C0\" C=\"2\" D=\"1\" E=\"Setpoint Rate of Dry Matter\" F=\"214\" />  <DPD A=\"161\" B=\"01C1\" C=\"1\" D=\"1\" E=\"Actual Rate of Dry Matter\" F=\"214\" />  <DPD A=\"162\" B=\"0161\" C=\"2\" D=\"17\" E=\"Nitrogen counter\" F=\"215\" />  <DPD A=\"163\" B=\"0162\" C=\"2\" D=\"17\" E=\"Ammonium counter\" F=\"215\" />  <DPD A=\"164\" B=\"0163\" C=\"2\" D=\"17\" E=\"Phosphor counter\" F=\"215\" />  <DPD A=\"165\" B=\"0164\" C=\"2\" D=\"17\" E=\"Potassium counter\" F=\"215\" />  <DPD A=\"166\" B=\"0165\" C=\"2\" D=\"17\" E=\"Dry Matter counter\" F=\"216\" />  <DPD A=\"167\" B=\"0006\" C=\"2\" D=\"1\" E=\"Application rate\" F=\"214\" />  <DPD A=\"168\" B=\"0007\" C=\"1\" D=\"1\" E=\"Current rate\" F=\"214\" />  <DPD A=\"169\" B=\"010F\" C=\"0\" D=\"17\" E=\"Total surface\" F=\"202\" />  <DPD A=\"170\" B=\"0110\" C=\"0\" D=\"17\" E=\"Total distance\" F=\"203\" />  <DPD A=\"171\" B=\"0112\" C=\"0\" D=\"17\" E=\"Lifetime\" F=\"204\" />  <DPD A=\"172\" B=\"0145\" C=\"0\" D=\"17\" E=\"Total volume\" F=\"201\" />  <DVP A=\"200\" B=\"0\" C=\"0.00000010000000\" D=\"2\" E=\"m3/ha\" />  <DVP A=\"201\" B=\"0\" C=\"0.00100000004750\" D=\"2\" E=\"m3\" />  <DVP A=\"202\" B=\"0\" C=\"0.00009999999747\" D=\"2\" E=\"ha\" />  </DVC></ISO11783_TaskData>";

        var isoxml = ISOXML.ParseFromXMLString(textInput);

        Assert.AreEqual(isoxml.Messages.Count, 3);
    }

}

