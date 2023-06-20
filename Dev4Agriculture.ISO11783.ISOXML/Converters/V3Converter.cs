using System;
using System.Collections.Generic;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.Serializer;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.Converters
{
    internal static class V3Converter
    {

        private static void MakeAllocationStampV3(ISOAllocationStamp stamp)
        {
            stamp.Start = new DateTime(stamp.Start.Ticks, DateTimeKind.Unspecified);
            if (stamp.StopValueSpecified)
            {
                stamp.Stop = new DateTime(stamp.StopValue.Ticks, DateTimeKind.Unspecified);
            }
        }

        private static ISOTask MakeTaskV3(ISOTask task)
        {
            switch (task.TaskStatus)
            {
                //p.141
                case ISOTaskStatus.Template:
                case ISOTaskStatus.Canceled:
                    task.TaskStatus = ISOTaskStatus.Planned;
                    break;
                default:
                    break;
            }

            if (task.GuidanceAllocationSpecified)
            {
                //p.142
                task.GuidanceAllocation.Clear();
            }

            //p.75
            if (task.ControlAssignmentSpecified)
            {
                //p.142
                task.ControlAssignment.Clear();
            }

            if (task.TreatmentZoneSpecified)
            {
                foreach (var trZone in task.TreatmentZone)
                {
                    if (trZone.ProcessDataVariable.Count > 1)
                    {
                        //p.148
                        var firstDataVar = trZone.ProcessDataVariable.First();
                        trZone.ProcessDataVariable.Clear();
                        trZone.ProcessDataVariable.Add(firstDataVar);
                    }

                    //p.129
                    trZone.ProcessDataVariable.First().ActualCulturalPracticeValue = null;
                    trZone.ProcessDataVariable.First().ElementTypeInstanceValue = null;

                    if (trZone.PolygonTreatmentZoneonly.Count > 1)
                    {
                        var firstPoly = trZone.PolygonTreatmentZoneonly.First();
                        trZone.PolygonTreatmentZoneonly.Clear();
                        trZone.PolygonTreatmentZoneonly.Add(firstPoly);
                    }
                }
            }

            foreach (var pAlloc in task.ProductAllocation)
            {
                if (pAlloc.TransferMode.HasValue && pAlloc.TransferMode == ISOTransferMode.Remainder)
                {
                    //p.136
                    pAlloc.TransferMode = ISOTransferMode.Emptying;
                }
                MakeAllocationStampV3(pAlloc.AllocationStamp);
            }

            foreach (var item in task.WorkerAllocation)
            {
                MakeAllocationStampV3(item.AllocationStamp);
            }

            foreach (var time in task.Time)
            {
                if (time.Type == ISOType2.PoweredDown)
                {
                    //p.146
                    time.Type = ISOType2.Clearing;
                }

                time.Start = new DateTime(time.Start.Ticks, DateTimeKind.Unspecified);
                if (time.StopValueSpecified)
                {
                    time.Stop = new DateTime(time.StopValue.Ticks, DateTimeKind.Unspecified);
                }
            }

            return task;
        }

        public static ISOPoint MakePointV3(ISOPoint point)
        {
            //p.124
            point.PointId = null;
            point.PointHorizontalAccuracy = null;
            point.PointVerticalAccuracy = null;
            point.Filename = null;
            point.Filelength = null;

        }


        public static ISOLineString MakeLineStringV3(ISOLineString lineString)
        {

            lineString.LineStringId = null; //p.118
            if (lineString.LineStringType == ISOLineStringType.Obstacle)
            {
                lineString.LineStringType = ISOLineStringType.Flag;//p.117
                var pointsToDelete = new List<ISOPoint>();
                foreach (var point in lineString.Point)
                {
                    if (point.PointType > ISOPointType.other)
                    {
                        pointsToDelete.Add(point);
                        continue;
                    }
                    MakePointV3(point);
                }

                foreach (var item in pointsToDelete)
                {
                    lineString.Point.Remove(item);
                }
            }
            return lineString;
        }

        public static ISOPartfield MakePartFieldV3(ISOPartfield partfield)
        {
            if (partfield.GuidanceGroupSpecified)
            {
                partfield.GuidanceGroup.Clear();
            }

            foreach (var line in partfield.LineString)
            {
                MakeLineStringV3(line);
            }

            foreach (var polygon in partfield.PolygonnonTreatmentZoneonly)
            {
                //p.125
                if (polygon.PolygonType > ISOPolygonType.Other)
                {
                    polygon.PolygonType = ISOPolygonType.Other;
                }
                polygon.PolygonId = null;

                foreach(var lsg in polygon.LineString)
                {
                    MakeLineStringV3(lsg);
                }
            }


            return partfield;
        }


        private static ISODevice MakeDeviceV3(ISODevice device)
        {
            if (device.DeviceStructureLabel.Length > 7)
            {
                device.DeviceStructureLabel = device.DeviceStructureLabel.Take(7).ToArray();
            }

            foreach (var item in device.DeviceProcessData)
            {
                var propAsByteArray = BitConverter.GetBytes(item.DeviceProcessDataProperty);
                if (propAsByteArray.Length >= 3)
                {
                    var thirdbit = propAsByteArray.ElementAt(3);
                    thirdbit = 0;
                    item.DeviceProcessDataProperty = (byte)BitConverter.ToInt16(propAsByteArray); // p.99
                }
            }


            return device;
        }

        public static ISO11783TaskDataFile ConvertToV3(ISO11783TaskDataFile taskData)
        {
            var isoxmlSerializer = new IsoxmlSerializer();
            var clonedData = isoxmlSerializer.DeepClone(taskData);

            foreach (var task in clonedData.Task)
            {
                MakeTaskV3(task);
            }

            foreach (var partfield in clonedData.Partfield)
            {
                MakePartFieldV3(partfield);
            }

            foreach (var product in clonedData.Product)
            {
                if (product.ProductRelationSpecified)
                {
                    //clear ProductRelation
                    product.ProductRelation.Clear();
                }
                //p.133
                product.ProductType = null;
                product.MixtureRecipeQuantity = null;
                product.DensityMassPerVolume = null;
                product.DensityMassPerCount = null;
                product.DensityVolumePerCount = null;
            }

            if (clonedData.AttachedFileSpecified)
            {
                clonedData.AttachedFile.Clear();
            }

            if (clonedData.BaseStationSpecified)
            {
                clonedData.BaseStation.Clear();
            }

            if (clonedData.CropTypeSpecified)
            {
                foreach (var crop in clonedData.CropType)
                {
                    crop.ProductGroupIdRef = null; //p.87
                    if (crop.CropVarietySpecified)
                    {
                        foreach (var item in crop.CropVariety)
                        {
                            item.ProductIdRef = null; //p.88
                        }
                    }
                }
            }

            if (clonedData.DeviceSpecified)
            {
                foreach (var device in clonedData.Device)
                {
                    MakeDeviceV3(device);
                }
            }

            clonedData.lang = null; //p.115
            if (clonedData.TaskControllerCapabilitiesSpecified)
            {
                clonedData.TaskControllerCapabilities.Clear();//p.116, p.143
            }

            if (clonedData.ProductGroupSpecified)
            {
                foreach (var productGroup in clonedData.ProductGroup)
                {
                    //p.139
                    productGroup.ProductGroupType = null;
                }
            }

            return clonedData;
        }

    }
}
