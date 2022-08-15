using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML
{


    class Program
    {

        public static void Main()
        {
            var isoxml = ISOXML.Create("C:/out");

            var customer = new ISOCustomer();
            customer.CustomerLastName = "Miller";
            var ctrId = isoxml.IdTable.AddObjectAndAssignIdIfNone(customer);
            isoxml.Data.Customer.Add(customer);

            ISOLineString lineString = new ISOLineString();
            lineString.LineStringType = ISOLineStringType.PolygonExterior;

            Decimal north = (Decimal)52.2;
            Decimal south = (Decimal)52.3;
            Decimal west = (Decimal)7.2;
            Decimal east = (Decimal)7.4;

            lineString.Point.Add(new ISOPoint()
            {
                PointNorth = (Decimal)north,
                PointEast = (Decimal)west,
                PointType = ISOPointType.other
            });


            lineString.Point.Add(new ISOPoint()
            {
                PointNorth = (Decimal)south,
                PointEast = (Decimal)west,
                PointType = ISOPointType.other
            });

            lineString.Point.Add(new ISOPoint()
            {
                PointNorth = (Decimal)south,
                PointEast = (Decimal)east,
                PointType = ISOPointType.other
            });

            lineString.Point.Add(new ISOPoint()
            {
                PointNorth = (Decimal)north,
                PointEast = (Decimal)east,
                PointType = ISOPointType.other
            });

            lineString.Point.Add(lineString.Point.First());


            var polygon = new ISOPolygon();
            polygon.PolygonType = ISOPolygonType.PartfieldBoundary;
            polygon.LineString.Add(lineString);

            var partField = new ISOPartfield();
            var pfdId = isoxml.IdTable.AddObjectAndAssignIdIfNone(partField);
            partField.PartfieldDesignator = "My Field";
            partField.PolygonnonTreatmentZoneonly.Add(polygon);
            isoxml.Data.Partfield.Add(partField);

            //TODO Create Grid
            var isoGrid = isoxml.GenerateGrid(ISOGridType.gridtype2, 20, 20, 1);
            var grid = isoxml.GetGridFile(isoGrid);
            isoGrid.TreatmentZoneCode = 2;
            isoGrid.GridMinimumNorthPosition = north;
            isoGrid.GridMinimumEastPosition = east;
            isoGrid.GridCellEastSize = (double)((east - west) / isoGrid.GridMaximumColumn);
            isoGrid.GridCellNorthSize = (double)((south-north) / isoGrid.GridMaximumRow);

            for (uint gridColumn = 0; gridColumn < isoGrid.GridMaximumColumn; gridColumn++)
            {
                for (uint gridRow = 0; gridRow < isoGrid.GridMaximumRow; gridRow++)
                {
                    grid.SetValue(gridRow, gridColumn, gridRow * 100);
                }
            }


            var vpn = new ISOValuePresentation()
            {
                Scale = (Decimal)0.1,
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
            }) ;

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
                DefaultTreatmentZoneCode = tzn1.TreatmentZoneCode,
                OutOfFieldTreatmentZoneCode = tzn1.TreatmentZoneCode,
                PositionLostTreatmentZoneCode = tzn1.TreatmentZoneCode
            };
            task.Grid.Add(isoGrid);
            task.TreatmentZone.Add(tzn1);
            task.TreatmentZone.Add(tznInner);   

            //TODO Add Stuff
            
            isoxml.IdTable.AddObjectAndAssignIdIfNone(task);
            isoxml.Data.Task.Add(task);

            isoxml.Save();
            var isoxml2 = ISOXML.Load("C:/out");

        }

    }
}
