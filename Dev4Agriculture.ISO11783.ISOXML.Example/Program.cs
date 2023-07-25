using System.IO;
using Dev4Agriculture.ISO11783.ISOXML.Analysis;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.Examples;


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
            "6: Check ISOXML");
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
        }

        return;
    }

}
