using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

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

        var north = (decimal)52.22;
        var south = (decimal)52.23;
        var west = (decimal)7.22;
        var east = (decimal)7.24;
        var west2 = (decimal)7.225;
        var east2 = (decimal)7.235;

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
                    grid.SetValue(gridColumn, gridRow, 5 + gridRow * 100);
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
            ProcessDataDDI = Utils.FormatDDI(6),
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
            ProcessDataDDI = Utils.FormatDDI(6),
            ProcessDataValue = 0,
            ValuePresentationIdRef = vpn.ValuePresentationId
        });

        isoGrid.TreatmentZoneCode = 2;


        var task = new ISOTask()
        {
            TaskDesignator = "Spraying",
            //Add TreatmentZones

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


    /// <summary>
    /// We want to find out which manufacturer build a specific machine.
    /// </summary>
    public static void ReadManufacturer()
    {
        var deviceDescription = "<DVC A=\"DVC-1\" B=\"Grosspackenpresse\" C=\"200812871\" D=\"A00E84000DE03CE9\" E=\"15593\" F=\"37303031303442\" G=\"FF000000406564\">\r\n    <DET A=\"DET-1\" B=\"1\" C=\"1\" D=\"Dev1\" E=\"0\" F=\"0\">\r\n      <DOR A=\"2\"/>\r\n      <DOR A=\"3\"/>\r\n      <DOR A=\"7\"/>\r\n      <DOR A=\"8\"/>\r\n      <DOR A=\"6\"/>\r\n      <DOR A=\"5\"/>\r\n      <DOR A=\"11\"/>\r\n      <DOR A=\"10\"/>\r\n      <DOR A=\"12\"/>\r\n      <DOR A=\"9\"/>\r\n      <DOR A=\"13\"/>\r\n      <DOR A=\"14\"/>\r\n      <DOR A=\"15\"/>\r\n      <DOR A=\"16\"/>\r\n    </DET>\r\n    <DPD A=\"2\" B=\"006B\" C=\"1\" D=\"9\" E=\"Ballenhoehe\" F=\"17\"/>\r\n    <DPD A=\"3\" B=\"0066\" C=\"1\" D=\"9\" E=\"Ballenbreite\" F=\"17\"/>\r\n    <DPD A=\"5\" B=\"0070\" C=\"1\" D=\"9\" E=\"Ballenlaenge Aktuell\" F=\"17\"/>\r\n    <DPD A=\"6\" B=\"0071\" C=\"1\" D=\"9\" E=\"Ballenlaenge Default\" F=\"17\"/>\r\n    <DPD A=\"7\" B=\"0072\" C=\"1\" D=\"9\" E=\"Ballenlaenge Minimal\" F=\"17\"/>\r\n    <DPD A=\"8\" B=\"0073\" C=\"1\" D=\"9\" E=\"Ballenlaenge Maximal\" F=\"17\"/>\r\n    <DPD A=\"9\" B=\"005B\" C=\"3\" D=\"25\" E=\"Anzahl Ballen\" F=\"21\"/>\r\n    <DPD A=\"10\" B=\"005A\" C=\"3\" D=\"25\" E=\"Summe Gewicht\" F=\"20\"/>\r\n    <DPD A=\"11\" B=\"004B\" C=\"1\" D=\"9\" E=\"Aktuelles Gewicht\" F=\"19\"/>\r\n    <DPD A=\"12\" B=\"0063\" C=\"1\" D=\"9\" E=\"Aktuelle Feuchte\" F=\"22\"/>\r\n    <DPD A=\"13\" B=\"0077\" C=\"3\" D=\"25\" E=\"Zeit Arbeit\" F=\"18\"/>\r\n    <DPD A=\"14\" B=\"0078\" C=\"3\" D=\"25\" E=\"Zeit nicht Arbeit\" F=\"18\"/>\r\n    <DPD A=\"15\" B=\"008D\" C=\"1\" D=\"9\" E=\"Arbeitsstellung\" F=\"23\"/>\r\n    <DPD A=\"16\" B=\"DFFF\" C=\"0\" D=\"31\" E=\"Default\"/>\r\n    <DVP A=\"17\" B=\"0\" C=\"0.1000000015\" D=\"0\" E=\"cm\"/>\r\n    <DVP A=\"18\" B=\"0\" C=\"0.0166669991\" D=\"0\" E=\"min\"/>\r\n    <DVP A=\"19\" B=\"0\" C=\"0.001\" D=\"0\" E=\"kg\"/>\r\n    <DVP A=\"20\" B=\"0\" C=\"1\" D=\"0\" E=\"kg\"/>\r\n    <DVP A=\"21\" B=\"0\" C=\"1\" D=\"0\" E=\"Ballen\"/>\r\n    <DVP A=\"22\" B=\"0\" C=\"0.1000000015\" D=\"1\" E=\"%\"/>\r\n    <DVP A=\"23\" B=\"0\" C=\"1\" D=\"0\"/>\r\n  </DVC>";
        var isoxml = ISOXML.ParseFromXMLString(deviceDescription);
        var device = isoxml.Data.Device[0];
        var clientNameBytes = device.ClientNAME;
        var clientName = new ClientName(clientNameBytes);
        Console.WriteLine("Manufacturer of The TaskSet: See https://www.isobus.net/isobus/manufacturerCode/" + clientName.ManufacturerCode);
    }
    public static void Main()
    {
        Console.WriteLine("Welcome to the Example code of the ISOXML.net Library \n " +
            "Created 2022 by dev4Agriculture \n" +
            "Enter the path were data shall be stored");
        var path = Console.ReadLine();
        if (path is string and not "")
        {
            CreateGrid(Path.Combine(path, "grid"));
            CreateTaskDataWithCodingData(Path.Combine(path, "CodingDataTaskData"));
        }

        ReadManufacturer();


    }

}
