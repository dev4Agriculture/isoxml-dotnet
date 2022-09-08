using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.Examples;


public static class Program
{

    public static void Main()
    {
        CreateSquareGrid();
        CreateTriangleGrid();

    }

    public static void CreateTriangleGrid()
    {
        uint[,] rawTestData = new uint[,] {
            {1,1,0,0,0,0,0,0,0,0,0,0,1},//Recognize that there is a 1 at the end of this row!
            {1,1,1,1,0,0,0,0,0,0,0,0,0},
            {1,1,1,1,1,1,1,0,0,0,0,0,0},
            {1,1,1,1,1,1,1,1,1,1,0,0,0},
            {1,1,1,1,1,1,1,1,1,1,1,1,0},
            {1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,0},
            {1,1,1,1,1,1,1,1,1,1,0,0,0},
            {1,1,1,1,1,1,1,0,0,0,0,0,0},
            {1,1,1,1,0,0,0,0,0,0,0,0,0},
            {1,1,0,0,0,0,0,0,0,0,0,0,0},
        };


        var isoxml = ISOXML.Create("C:/out/triangle");

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

        var north = (decimal)52.23;
        var south = (decimal)52.22;
        var west = (decimal)7.22;
        var east = (decimal)7.24;

        lineString.Point.Add(new ISOPoint()
        {
            PointNorth = north,
            PointEast = west,
            PointType = ISOPointType.other
        });


        lineString.Point.Add(new ISOPoint()
        {
            PointNorth = south,
            PointEast = west,
            PointType = ISOPointType.other
        });


        lineString.Point.Add(new ISOPoint()
        {
            PointNorth = (north+south)/2,
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

        var isoGrid = isoxml.GenerateGrid(ISOGridType.gridtype2, 13, 11, 1);
        var grid = isoxml.GetGridFile(isoGrid);
        isoGrid.TreatmentZoneCode = 2;
        isoGrid.GridMinimumNorthPosition = south;
        isoGrid.GridMinimumEastPosition = west;
        isoGrid.GridCellEastSize = (double)((east - west) / isoGrid.GridMaximumColumn);
        isoGrid.GridCellNorthSize = (double)((north - south) / isoGrid.GridMaximumRow);

        for (uint gridColumn = 0; gridColumn < isoGrid.GridMaximumColumn; gridColumn++)
        {
            for (uint gridRow = 0; gridRow < isoGrid.GridMaximumRow; gridRow++)
            {
                grid.SetValue(gridColumn, gridRow, rawTestData[gridRow, gridColumn] *100 * (gridColumn+1));
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
        var isoxml2 = ISOXML.Load("C:/out/triangle");

    }

    public static void CreateSquareGrid() { 
        var isoxml = ISOXML.Create("C:/out/square");

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

        var north = (decimal)52.23;
        var south = (decimal)52.22;
        var west = (decimal)7.22;
        var east = (decimal)7.24;

        lineString.Point.Add(new ISOPoint()
        {
            PointNorth = north,
            PointEast = west,
            PointType = ISOPointType.other
        });


        lineString.Point.Add(new ISOPoint()
        {
            PointNorth = south,
            PointEast = west,
            PointType = ISOPointType.other
        });

        lineString.Point.Add(new ISOPoint()
        {
            PointNorth = south,
            PointEast = east,
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

        var isoGrid = isoxml.GenerateGrid(ISOGridType.gridtype2, 10, 13, 1);
        var grid = isoxml.GetGridFile(isoGrid);
        isoGrid.TreatmentZoneCode = 2;
        isoGrid.GridMinimumNorthPosition = south;
        isoGrid.GridMinimumEastPosition = west;
        isoGrid.GridCellEastSize = (double)((east - west) / isoGrid.GridMaximumColumn);
        isoGrid.GridCellNorthSize = (double)((north - south) / isoGrid.GridMaximumRow);

        for (uint gridColumn = 0; gridColumn < isoGrid.GridMaximumColumn; gridColumn++)
        {
            for (uint gridRow = 0; gridRow < isoGrid.GridMaximumRow; gridRow++)
            {
                grid.SetValue(gridColumn, gridRow, 5 + gridColumn * 100);
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
        var isoxml2 = ISOXML.Load("C:/out/square" +
            "");

    }

}
