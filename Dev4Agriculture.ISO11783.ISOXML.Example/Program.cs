using System.IO;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.Analysis;
using Dev4Agriculture.ISO11783.ISOXML.Emulator;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using Newtonsoft.Json;

namespace Dev4Agriculture.ISO11783.ISOXML.Examples;

public enum APIDeviceDescriptionType
{
    ISOXML,
    JSON
}

public record APIDeviceDescriptionInputModel
(
    APIDeviceDescriptionType DeviceDescriptionType,
    string Content
);



public static class Program
{
    /// <summary>
    /// An example on how to create a TaskData that includes a Grid and a Field Boundary
    /// </summary>
    public static void CreateGrid(string path)
    {
        var isoxml = ISOXML.Create(path);

        var customer = new ISOCustomer
        {
            CustomerLastName = "Miller"
        };
        var ctrId = isoxml.IdTable.AddObjectAndAssignIdIfNone(customer);
        isoxml.Data.Customer.Add(customer);


        var lineString = new ISOLineString
        {
            LineStringType = ISOLineStringType.PolygonExterior
        };

        var north = (decimal)52.223;
        var south = (decimal)52.225;
        var west =  (decimal)7.222;
        var east =  (decimal)7.226;
        var west2 = (decimal)7.223;
        var east2 = (decimal)7.225;

        lineString.Point.Add(new ISOPoint()
        {
            PointNorth = north,
            PointEast = west,
            PointType = ISOPointType.other
        });


        lineString.Point.Add(new ISOPoint()
        {
            PointNorth = south,
            PointEast = west2,
            PointType = ISOPointType.other
        });

        lineString.Point.Add(new ISOPoint()
        {
            PointNorth = south,
            PointEast = east2,
            PointType = ISOPointType.other
        });

        lineString.Point.Add(new ISOPoint()
        {
            PointNorth = north,
            PointEast = east,
            PointType = ISOPointType.other
        });

        lineString.Point.Add(lineString.Point.First());


        var polygon = new ISOPolygon
        {
            PolygonType = ISOPolygonType.PartfieldBoundary
        };
        polygon.LineString.Add(lineString);

        var partField = new ISOPartfield();
        var pfdId = isoxml.IdTable.AddObjectAndAssignIdIfNone(partField);
        partField.PartfieldDesignator = "My Field";
        partField.PolygonnonTreatmentZoneonly.Add(polygon);
        isoxml.Data.Partfield.Add(partField);

        var isoGrid = isoxml.GenerateGrid(ISOGridType.gridtype2, 20, 20, 1);
        var grid = isoxml.GetGridFile(isoGrid);
        isoGrid.TreatmentZoneCode = 2;
        isoGrid.GridMinimumNorthPosition = north;
        isoGrid.GridMinimumEastPosition = west;
        isoGrid.GridCellEastSize = (double)((east - west) / isoGrid.GridMaximumColumn);
        isoGrid.GridCellNorthSize = (double)((south - north) / isoGrid.GridMaximumRow);

        for (uint gridColumn = 0; gridColumn < isoGrid.GridMaximumColumn; gridColumn++)
        {
            for (uint gridRow = 0; gridRow < isoGrid.GridMaximumRow; gridRow++)
            {
                var latitude = north + (decimal)(isoGrid.GridCellNorthSize * (gridRow + 0.5));
                var longitude = west + (decimal)(isoGrid.GridCellEastSize * (gridColumn + 0.5));
                if (partField.IsInField(longitude, latitude))
                {
                    grid.SetValue(gridColumn, gridRow, 5 + (int)gridRow * 100);
                }
                else
                {
                    grid.SetValue(gridColumn, gridRow, 0);
                }
            }
        }


        var vpn = new ISOValuePresentation()
        {
            Scale = (decimal)0.1,
            NumberOfDecimals = 1,
            Offset = 0,
            UnitDesignator = "kg/ha",
            ValuePresentationId = "VPN1"
        };
        isoxml.Data.ValuePresentation.Add(vpn);


        var tzn1 = new ISOTreatmentZone()
        {
            TreatmentZoneCode = 1,
            TreatmentZoneDesignator = "OutOfField"
        };


        tzn1.ProcessDataVariable.Add(new ISOProcessDataVariable()
        {
            ProcessDataDDI = DDIUtils.FormatDDI(6),
            ProcessDataValue = 0,
            ValuePresentationIdRef = vpn.ValuePresentationId
        });

        var tznInner = new ISOTreatmentZone()
        {
            TreatmentZoneCode = 2,
            TreatmentZoneDesignator = "GridData"
        };

        tznInner.ProcessDataVariable.Add(new ISOProcessDataVariable()
        {
            ProcessDataDDI = DDIUtils.FormatDDI(6),
            ProcessDataValue = 0,
            ValuePresentationIdRef = vpn.ValuePresentationId
        });

        isoGrid.TreatmentZoneCode = 2;


        var task = new ISOTask()
        {
            TaskDesignator = "Spraying",
            CustomerIdRef = ctrId,
            PartfieldIdRef = pfdId,
            DefaultTreatmentZoneCode = 1,
            OutOfFieldTreatmentZoneCode = 1,
            PositionLostTreatmentZoneCode = 1
        };
        task.Grid.Add(isoGrid);
        task.TreatmentZone.Add(tzn1);
        task.TreatmentZone.Add(tznInner);

        isoxml.IdTable.AddObjectAndAssignIdIfNone(task);
        isoxml.Data.Task.Add(task);

        isoxml.Save();
        var isoxml2 = ISOXML.Load("C:/out");

    }


    /// <summary>
    /// The most simple way to create a task with a Customer
    /// </summary>
    /// <param name="path"></param>
    public static void CreateTaskDataWithCodingData(string path)
    {
        var isoxml = ISOXML.Create(path);

        var customer = new ISOCustomer()
        {
            CustomerFirstName = "Peter",
            CustomerLastName = "Parker"
        };

        isoxml.IdTable.AddObjectAndAssignIdIfNone(customer);

        var task = new ISOTask()
        {
            CustomerIdRef = customer.CustomerId,
            TaskStatus = ISOTaskStatus.Planned,
            TaskDesignator = "Harvesting"
        };

        isoxml.IdTable.AddObjectAndAssignIdIfNone(task);

        isoxml.Data.Customer.Add(customer);
        isoxml.Data.Task.Add(task);

        isoxml.Save();
    }
    private static String FixStringLength(String str)
    {
        return str.Substring(0, Math.Min(str.Length, 32)).PadRight(32, ' ');
    }


    public static void CompareFieldOverLaps(string path1, string path2)
    {
        var isoxml1 = ISOXML.LoadFromArchive(File.OpenRead(path1));
        var isoxml2 = ISOXML.LoadFromArchive(File.OpenRead(path2));

        var startLine = FixStringLength("    ")+"| ";
        foreach (var field in isoxml1.Data.Partfield)
        {
            startLine += FixStringLength(field.PartfieldDesignator) + "| ";
        }
        Console.WriteLine(startLine);

        foreach (var compareField in isoxml2.Data.Partfield)
        {
            var row = FixStringLength(compareField.PartfieldDesignator) + "| ";
            foreach (var field in isoxml1.Data.Partfield)
            {
                var res = compareField.TryGetOverlapWithPartfield(field); 
                if (res != null)
                {
                    row +=  FixStringLength((res.IntersectPercent * 100).ToString() + "%") + "| ";

                }
                else
                {
                    row +=  FixStringLength("Error") + "| ";
                }
            }
            Console.WriteLine(row);

        }
    }

    /// <summary>
    /// We want to find out which manufacturer build a specific machine.
    /// </summary>
    public static string ReadManufacturer(string deviceDescription)
    {
        var isoxml = ISOXML.ParseFromXMLString(deviceDescription);
        var device = isoxml.Data.Device[0];
        var clientNameBytes = device.ClientNAME;
        var clientName = new ClientName(clientNameBytes);
        return clientName.ManufacturerCode.ToString();
    }

    public static CulturalPracticesType GetCulturalPractice(string path)
    {
        ISOXML isoxml = null;
        if(path.EndsWith(".zip"))
        {
            var stream = File.OpenRead(path);
            isoxml = ISOXML.LoadFromArchive(stream);
        }
        else
        {
            isoxml = ISOXML.Load(path);
        }
        var analyzer = new ISOTaskAnalysis(isoxml);
        var acp = analyzer.FindTaskCulturalPractice(isoxml.Data.Task[0]);
        return acp.CulturalPractice;
    }


    public static void CheckISOXML(string? path3)
    {
        ISOXML isoxml = null;
        if (path3.EndsWith(".zip"))
        {
            var stream = File.OpenRead(path3);
            isoxml = ISOXML.LoadFromArchive(stream);
        }
        else
        {
            isoxml = ISOXML.Load(path3);
        }
        foreach (var item in isoxml.Messages)
        {
            Console.WriteLine(item.Description);
        }
    }


    private static void ConvertXML2JSON(string? path4)
    {
        var devices = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<ISO11783_TaskData VersionMajor=\"3\" VersionMinor=\"3\" ManagementSoftwareManufacturer=\"iXmap Services GmbH &amp; Co. KG\" ManagementSoftwareVersion=\"1.5.0.2\" TaskControllerManufacturer=\"SDF Guidance\" TaskControllerVersion=\"IM6447AE\" DataTransferOrigin=\"2\">\n<DVC A=\"DVC-1\" B=\"KRONE Haecksler\" C=\"150200029-17 \" D=\"A00E86000DE0287F\" E=\"0000862189\" F=\"38303030303131\" G=\"FF000000406564\">\n    <DET A=\"DET-1\" B=\"1\" C=\"1\" D=\"DeviceElement\" E=\"0\" F=\"0\">\n      <DOR A=\"19\"/>\n      <DOR A=\"20\"/>\n      <DOR A=\"21\"/>\n      <DOR A=\"22\"/>\n      <DOR A=\"23\"/>\n      <DOR A=\"24\"/>\n      <DOR A=\"25\"/>\n      <DOR A=\"26\"/>\n      <DOR A=\"27\"/>\n      <DOR A=\"28\"/>\n      <DOR A=\"29\"/>\n      <DOR A=\"30\"/>\n      <DOR A=\"31\"/>\n      <DOR A=\"32\"/>\n      <DOR A=\"33\"/>\n      <DOR A=\"34\"/>\n      <DOR A=\"35\"/>\n      <DOR A=\"36\"/>\n      <DOR A=\"37\"/>\n      <DOR A=\"38\"/>\n      <DOR A=\"39\"/>\n      <DOR A=\"40\"/>\n      <DOR A=\"41\"/>\n      <DOR A=\"42\"/>\n      <DOR A=\"43\"/>\n      <DOR A=\"44\"/>\n      <DOR A=\"45\"/>\n	  <DOR A=\"46\"/>\n    </DET>\n    <DPD A=\"19\" B=\"0043\" C=\"1\" D=\"8\" E=\"Aktuelle Arbeitsbreite\" F=\"3\"/>\n    <DPD A=\"20\" B=\"0054\" C=\"1\" D=\"1\" E=\"Ertrag pro Flaeche\" F=\"4\"/>\n    <DPD A=\"21\" B=\"0057\" C=\"1\" D=\"1\" E=\"Ertrag pro Zeit\" F=\"5\"/>\n    <DPD A=\"22\" B=\"005A\" C=\"3\" D=\"24\" E=\"Gesamtertrag\" F=\"6\"/>\n    <DPD A=\"23\" B=\"0063\" C=\"1\" D=\"1\" E=\"Feuchtegehalt\" F=\"7\"/>\n    <DPD A=\"24\" B=\"0074\" C=\"3\" D=\"24\" E=\"Gesamtflaeche\" F=\"8\"/>\n    <DPD A=\"25\" B=\"0075\" C=\"3\" D=\"24\" E=\"Aktive Fahrstrecke\" F=\"9\"/>\n    <DPD A=\"26\" B=\"0076\" C=\"3\" D=\"24\" E=\"Inaktive Fahrstrecke\" F=\"9\"/>\n    <DPD A=\"27\" B=\"0077\" C=\"3\" D=\"24\" E=\"Aktive Gesamtzeit\" F=\"10\"/>\n    <DPD A=\"28\" B=\"0078\" C=\"3\" D=\"24\" E=\"Inaktive Gesamtzeit\" F=\"10\"/>\n    <DPD A=\"29\" B=\"008D\" C=\"1\" D=\"1\" E=\"Arbeitsstatus\" F=\"2\"/>\n    <DPD A=\"30\" B=\"0094\" C=\"3\" D=\"24\" E=\"Gesamtkraftstoffverbrauch\" F=\"11\"/>\n    <DPD A=\"31\" B=\"0095\" C=\"1\" D=\"1\" E=\"Aktueller Kraftstoffverbrauch\" F=\"12\"/>\n    <DPD A=\"32\" B=\"00B1\" C=\"1\" D=\"1\" E=\"Aktuelle Schnittlaenge\" F=\"14\"/>\n    <DPD A=\"33\" B=\"0106\" C=\"3\" D=\"24\" E=\"Durchschnittsfeuchte\" F=\"7\"/>\n    <DPD A=\"34\" B=\"00FE\" C=\"1\" D=\"8\" E=\"Seriennummer\" F=\"2\"/>\n    <DPD A=\"35\" B=\"EA60\" C=\"1\" D=\"1\" E=\"Motoristdrehzahl\" F=\"15\"/>\n    <DPD A=\"36\" B=\"EA6A\" C=\"1\" D=\"1\" E=\"Motorauslastung\" F=\"16\"/>\n    <DPD A=\"37\" B=\"EA74\" C=\"1\" D=\"1\" E=\"Kraftstofffuellstand\" F=\"16\"/>\n    <DPD A=\"38\" B=\"EA7E\" C=\"1\" D=\"1\" E=\"Fahrgeschwindigkeit\" F=\"13\"/>\n    <DPD A=\"39\" B=\"EA88\" C=\"1\" D=\"1\" E=\"Corn Craecker Abstand\" F=\"17\"/>\n    <DPD A=\"40\" B=\"EA9C\" C=\"1\" D=\"1\" E=\"Drehzahl Vorsatz\" F=\"15\"/>\n    <DPD A=\"41\" B=\"EAA6\" C=\"1\" D=\"1\" E=\"Drehzahl Einzug\" F=\"15\"/>\n    <DPD A=\"42\" B=\"EAB0\" C=\"1\" D=\"1\" E=\"Drehzahl Haeckseltrommel\" F=\"15\"/>\n    <DPD A=\"43\" B=\"EACE\" C=\"3\" D=\"24\" E=\"Trommelstunden\" F=\"18\"/>\n    <DPD A=\"44\" B=\"EAD8\" C=\"3\" D=\"24\" E=\"Vorsatzstunden\" F=\"18\"/>\n    <DPD A=\"45\" B=\"EAE2\" C=\"3\" D=\"24\" E=\"Motorstunden\" F=\"18\"/>\n    <DPD A=\"46\" B=\"DFFF\" C=\"1\" D=\"31\" E=\"default logging\" F=\"2\"/>\n    <DVP A=\"2\" B=\"0\" C=\"1\" D=\"0\" E=\" \"/>\n    <DVP A=\"3\" B=\"0\" C=\"0.001\" D=\"3\" E=\"m\"/>\n    <DVP A=\"4\" B=\"0\" C=\"0.00001\" D=\"2\" E=\"t/ha\"/>\n    <DVP A=\"5\" B=\"0\" C=\"0.0000036\" D=\"2\" E=\"t/h\"/>\n    <DVP A=\"6\" B=\"0\" C=\"0.001\" D=\"2\" E=\"t\"/>\n    <DVP A=\"7\" B=\"0\" C=\"0.0001\" D=\"2\" E=\"Prozent\"/>\n    <DVP A=\"8\" B=\"0\" C=\"0.0001\" D=\"2\" E=\"ha\"/>\n    <DVP A=\"9\" B=\"0\" C=\"0.000001\" D=\"2\" E=\"km\"/>\n    <DVP A=\"10\" B=\"0\" C=\"0.0166666657\" D=\"2\" E=\"min\"/>\n    <DVP A=\"11\" B=\"0\" C=\"0.001\" D=\"2\" E=\"l\"/>\n    <DVP A=\"12\" B=\"0\" C=\"0.0035999999\" D=\"2\" E=\"l/h\"/>\n    <DVP A=\"13\" B=\"0\" C=\"0.1000000015\" D=\"1\" E=\"km/h\"/>\n    <DVP A=\"14\" B=\"0\" C=\"0.001\" D=\"3\" E=\"mm\"/>\n    <DVP A=\"15\" B=\"0\" C=\"1\" D=\"0\" E=\"UPM\"/>\n    <DVP A=\"16\" B=\"0\" C=\"1\" D=\"0\" E=\"Prozent\"/>\n    <DVP A=\"17\" B=\"0\" C=\"0.1000000015\" D=\"1\" E=\"mm\"/>\n    <DVP A=\"18\" B=\"0\" C=\"0.000277777\" D=\"2\" E=\"h\"/>\n  </DVC>\n  </ISO11783_TaskData>";
        var content = new APIDeviceDescriptionInputModel(APIDeviceDescriptionType.ISOXML, devices);
        File.WriteAllText(path4, JsonConvert.SerializeObject(content));


    }


    private static void CreateDeviceAndWriteSomeTimeLogs(string? path)
    {
        var emulator = TaskControllerEmulator.Generate(path, "Test", ISO11783TaskDataFileVersionMajor.Version4, ISO11783TaskDataFileVersionMinor.Item1, "1.1");
        emulator.SetLocalization("en", UnitSystem_US.METRIC);
        var machine = emulator.GenerateDevice("BeetHarvester", "1.0", new byte[] { 1, 0, 20, 30, 10, 10, 20, 10 }, DeviceClass.Harvesters, 999, 12345);
        //Units
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
            Scale = (decimal)0.02666
        };


        //WorkState
        machine.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.ActualWorkState),
            DeviceProcessDataDesignator = "Workstate",
            DeviceProcessDataTriggerMethods = (byte)TriggerMethods.OnChange,
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet
        });

        machine.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.InstantaneousFuelConsumptionPerTime),
            DeviceProcessDataDesignator = "Fuel Consumption",
            DeviceProcessDataTriggerMethods = (byte)TriggerMethods.OnTime | (byte)TriggerMethods.OnChange,
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet| (byte) ISODeviceProcessDataPropertyType.Setable 
        },
        valuePresentation: dvpConsumption
        );


        machine.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.EffectiveTotalDieselExhaustFluidConsumption),
            DeviceProcessDataDesignator = "AdBlue Consumption",
            DeviceProcessDataTriggerMethods = (byte)TriggerMethods.OnTime | (byte)TriggerMethods.OnChange,
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet | (byte)ISODeviceProcessDataPropertyType.Setable
        },
        valuePresentation: dvpVolume
        );


        machine.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.TotalFuelConsumption),
            DeviceProcessDataDesignator = "Total Fuel Consumption",
            DeviceProcessDataTriggerMethods = (byte)TriggerMethods.OnChange | (byte)TriggerMethods.Total,
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet | (byte)ISODeviceProcessDataPropertyType.Setable
        },
        valuePresentation: dvpVolume
        );



        machine.AddDeviceProcessData(new ISODeviceProcessData()
        {
            DeviceProcessDataDDI = DDIUtils.FormatDDI(DDIList.TotalArea),
            DeviceProcessDataDesignator = "Total Area",
            DeviceProcessDataTriggerMethods = (byte)TriggerMethods.OnChange | (byte)TriggerMethods.Total,
            DeviceProcessDataProperty = (byte)ISODeviceProcessDataPropertyType.BelongsToDefaultSet | (byte)ISODeviceProcessDataPropertyType.Setable
        },
        valuePresentation: dvpArea
        );

        var isoxml = emulator.GetTaskDataSet();

        var task = new ISOTask()
        {
            TaskDesignator = "Working 1",
        };
        var taskId = isoxml.IdTable.AddObjectAndAssignIdIfNone(task);

        task.AddDefaultDataLogTrigger();

        isoxml.Data.Task.Add(task);


        emulator.ConnectDevice(machine.GetDevice());

        emulator.StartTask(DateTime.Now, task);

        var datetime = DateTime.Now;

        for (var a = 0; a < 200; a++)
        {
            datetime = datetime.AddSeconds(1);
            emulator.AddTimeAndPosition(datetime, new ISOPosition()
            {
                PositionNorth = (decimal)52.2,
                PositionEast = (decimal)(7.1 + 0.001 * a),
                PositionStatus = ISOPositionStatus.GNSSfix

            });

            if (a % 2 == 0)
            {
                emulator.UpdateRawMachineValue(DDIList.ActualWorkState, (a > 5 ? 0 : 1));
                emulator.AddRawValueToMachineValue(DDIList.TotalFuelConsumption, 4900);

            }
            else
            {
                emulator.UpdateRawMachineValue(DDIList.InstantaneousFuelConsumptionPerTime, 400 + a);
                emulator.AddRawValueToMachineValue(DDIList.EffectiveTotalDieselExhaustFluidConsumption, 4900);
            }
            if (a % 4 == 0)
            {
                emulator.UpdateRawMachineValue(DDIList.TotalArea, a * 1000);
            }
        }
        datetime = datetime.AddSeconds(5);

        emulator.StartTask(datetime);


        for (var a = 0; a < 200; a++)
        {
            datetime = datetime.AddSeconds(1);
            emulator.AddTimeAndPosition(datetime, new ISOPosition()
            {
                PositionNorth = (decimal)52.2,
                PositionEast = (decimal)(7.11 + 0.001 * a),
                PositionStatus = ISOPositionStatus.GNSSfix

            });

            if (a % 2 == 0)
            {
                emulator.UpdateRawMachineValue(DDIList.ActualWorkState, (a > 5 ? 0 : 1));
                emulator.AddRawValueToMachineValue(DDIList.TotalFuelConsumption, 4900);

            }
            else
            {
                emulator.UpdateRawMachineValue(DDIList.InstantaneousFuelConsumptionPerTime, 20000);
                emulator.AddRawValueToMachineValue(DDIList.EffectiveTotalDieselExhaustFluidConsumption, 4900);
            }
        }

        datetime = datetime.AddSeconds(1);
        emulator.PauseTask();
        datetime = datetime.AddSeconds(10);

        emulator.StartTask(datetime, task);


        for (var a = 0; a < 100; a++)
        {
            datetime = datetime.AddSeconds(1);
            emulator.AddTimeAndPosition(datetime, new ISOPosition()
            {
                PositionNorth = (decimal)52.2,
                PositionEast = (decimal)(7.12 + 0.001 * a),
                PositionStatus = ISOPositionStatus.GNSSfix

            });

            if (a % 2 == 0)
            {
                emulator.UpdateRawMachineValue(DDIList.ActualWorkState, (a > 5 ? 0 : 1));
                emulator.AddRawValueToMachineValue(DDIList.TotalFuelConsumption, 4900);

            }
            else
            {
                emulator.UpdateRawMachineValue(DDIList.InstantaneousFuelConsumptionPerTime, 20000 + a);
                emulator.AddRawValueToMachineValue(DDIList.EffectiveTotalDieselExhaustFluidConsumption, 4900);
            }
        }
        emulator.FinishTask();

        isoxml.Save();
    }

    public static void Main()
    {
        Console.WriteLine("Welcome to the Example code of the ISOXML.net Library \n " +
            "Created 2022 by dev4Agriculture \n" +
            "Enter the path were data shall be stored");
        Console.WriteLine("Enter a letter to run a function: \n" +
            "1: Read Manufacturer from DeviceDescription\n" +
            "2: Get Cultural Practice for TaskSet first Task\n" +
            "3: Create Task with CodingData \n" +
            "4: Create an example grid \n" +
            "5: Create FieldSize Comparison for 2 ISOXML DataSets\n" +
            "6: Check ISOXML\n" +
            "7: Convert to JSON\n"+
            "8: Generate ISOXML Machine Data");
        var entry = Console.ReadLine();
        if (!int.TryParse(entry, out var nr))
        {
            Console.WriteLine("Error");
            return;
        }
        string path = "";
        switch (nr)
        {
            case 1:
                Console.WriteLine("Enter DeviceDescription");
                var dvcDescript = "";
                while (!dvcDescript.EndsWith("</DVC>"))
                {
                    dvcDescript += Console.ReadLine();
                }
                Console.WriteLine("Manufacturer of The TaskSet: See https://www.isobus.net/isobus/manufacturerCode/ : " + ReadManufacturer(dvcDescript));
                break;
            case 2:
                Console.WriteLine("Enter Path to get ActualCulturalPractice");
                path = Console.ReadLine();
                if (!string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Actual Cultural Practice: " + GetCulturalPractice(path).ToString());
                }

                break;
            case 3:
                Console.WriteLine("Please enter path");
                path = Console.ReadLine();
                if (path is string and not "")
                {
                    CreateTaskDataWithCodingData(Path.Combine(path, "CodingDataTaskData"));
                }
                break;
            case 4:
                Console.WriteLine("Please enter path");
                path = Console.ReadLine();
                if (path is string and not "")
                {
                    CreateGrid(Path.Combine(path, "grid"));
                }
                break;
            case 5:
                Console.WriteLine("Select first ISOXML");
                var path1 = Console.ReadLine();
                Console.WriteLine("Select second ISOXML");
                var path2 = Console.ReadLine();
                if( String.IsNullOrWhiteSpace(path1) || String.IsNullOrWhiteSpace(path2))
                {
                    Console.WriteLine("One TaskSet was missing");
                    return;
                }
                CompareFieldOverLaps(path1, path2);
                Console.ReadKey();
                break;
            case 6:
                Console.WriteLine("Select ISOXML to Check");
                var path3 = Console.ReadLine();
                CheckISOXML(path3);
                break;
            case 7:
                Console.WriteLine("Select ISOXML to convert");
                var path4 = Console.ReadLine();
                ConvertXML2JSON(path4);
                break;
            case 8:
                Console.WriteLine("Select ISOXML to convert");
                var path5 = Console.ReadLine();
                CreateDeviceAndWriteSomeTimeLogs(path5);
                break;
        }

        return;
    }


}
