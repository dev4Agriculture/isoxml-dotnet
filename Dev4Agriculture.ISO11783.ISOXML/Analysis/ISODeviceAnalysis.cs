using System;
using System.Collections.Generic;
using System.Linq;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;

namespace Dev4Agriculture.ISO11783.ISOXML.Analysis
{

    public enum DDIValueType
    {
        None = 0,
        ProcessData = 1,
        Property = 2
    }

    public class TaskDDIEntry
    {
        public string DeviceElementId;
        public DDIValueType Type;
        public ushort DDI;
        public int DeviceElementNo()
        {
            return IdList.ToIntId(DeviceElementId);
        }
    }

    public class CulturalPracticeInfo
    {
        public CulturalPracticesType CulturalPractice { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime StopDateTime { get; set; }
        public double DurationInSeconds { get; set; }
        public string DeviceElementId { get; set; }
        public string DeviceId { get; set; }
        /// <summary>
        /// If true source is property, otherwise DeviceClass
        /// </summary>
        public bool IsPropertySource { get; set; }
    }

    internal class WorkingTimeInfo
    {
        public TaskDDIEntry DdiEntry { get; set; }
        public double Duration { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// This class provides several functions to analyse tasks and devices with device specific information
    /// </summary>
    public class ISODeviceAnalysis
    {
        private ISOXML _isoxml;
        public ISODeviceAnalysis(ISOXML isoxml)
        {
            _isoxml = isoxml;
        }

        public ISODevice GetDevice(string deviceElementId)
        {
            return _isoxml.Data.Device.First(dvc => dvc.DeviceElement.Any(det => det.DeviceElementId == deviceElementId));
        }

        public ISODeviceElement GetDeviceElement(string deviceElementId)
        {
            return _isoxml.Data.Device.SelectMany(dvc => dvc.DeviceElement).FirstOrDefault(det => det.DeviceElementId == deviceElementId);
        }


        /// <summary>
        /// Find the full DeviceProcessData-Element; e.g. to read the Triggermethods
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public ISODeviceProcessData GetDeviceProcessData(TaskDDIEntry entry, ISODevice device = null)
        {
            if (entry.Type != DDIValueType.ProcessData)
            {
                return null;
            }
            device ??= GetDevice(entry.DeviceElementId);
            var dorList = device.DeviceElement.Where(det => det.DeviceElementId == entry.DeviceElementId)
                .SelectMany(det => det.DeviceObjectReference).Select(dor => dor.DeviceObjectId).ToList();
            return
                device.DeviceProcessData.First(dpd =>
                    Utils.ConvertDDI(dpd.DeviceProcessDataDDI) == entry.DDI &&
                    dorList.Contains(dpd.DeviceProcessDataObjectId)
                );
        }

        /// <summary>
        /// Get the full DeviceProperty to read e.g. the Value
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public ISODeviceProperty GetDeviceProperty(TaskDDIEntry entry, ISODevice device = null)
        {
            if (entry.Type != DDIValueType.Property)
            {
                return null;
            }

            device ??= GetDevice(entry.DeviceElementId);
            var dorList = device.DeviceElement.Where(det => det.DeviceElementId == entry.DeviceElementId)
                .SelectMany(det => det.DeviceObjectReference).Select(dor => dor.DeviceObjectId).ToList();

            return device.DeviceProperty.First(dpd =>
                Utils.ConvertDDI(dpd.DevicePropertyDDI) == entry.DDI &&
                dorList.Contains(dpd.DevicePropertyObjectId)
            );
        }


        /// <summary>
        /// Get the Name of a DDI as shown in the DeviceDescription
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public string GetDeviceDataDesignator(TaskDDIEntry entry)
        {
            switch (entry.Type)
            {
                case DDIValueType.ProcessData:
                    return GetDeviceProcessData(entry).DeviceProcessDataDesignator;


                case DDIValueType.Property:
                    return GetDeviceProperty(entry).DevicePropertyDesignator;

                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the DeviceValuePresentation to convert values more conveniently
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public ISODeviceValuePresentation GetDeviceValuePresentation(TaskDDIEntry entry)
        {
            var device = GetDevice(entry.DeviceElementId);
            switch (entry.Type)
            {
                case DDIValueType.ProcessData:
                    var dpd = GetDeviceProcessData(entry);
                    if (dpd.DeviceValuePresentationObjectIdValueSpecified)
                    {
                        return device.DeviceValuePresentation.FirstOrDefault(dvp => dvp.DeviceValuePresentationObjectId == dpd.DeviceValuePresentationObjectId);
                    }
                    break;


                case DDIValueType.Property:
                    var dpt = GetDeviceProperty(entry);
                    if (dpt.DeviceValuePresentationObjectIdValueSpecified)
                    {
                        return device.DeviceValuePresentation.FirstOrDefault(dvp => dvp.DeviceValuePresentationObjectId == dpt.DeviceValuePresentationObjectId);
                    }
                    break;

                default:
                    break;
            }

            return new ISODeviceValuePresentation()
            {
                Scale = 1,
                Offset = 0,
                UnitDesignator = ""
            };
        }

        /// <summary>
        /// One of the most common issues used to be to find the Correct DeviceElement that was mentioned in a Task
        /// This function returns a list of possible DETs. If it's more than 1 DET, a filter for Device- or DeviceElementType helps to find the wanted one
        /// </summary>
        /// <param name="isoTask"></param>
        /// <param name="ddi"></param>
        /// <returns></returns>
        public List<TaskDDIEntry> FindDeviceElementsForDDI(ISOTask isoTask, ushort ddi)
        {
            var processData = isoTask.TimeLogs.ToList().SelectMany(entry => entry.Header.Ddis)
                        .Where(dlv => dlv.Ddi == ddi)  //Find DataLogValues with DDI; those list the DET as well
                        .Select(dlvEntry => new TaskDDIEntry()
                        {
                            DeviceElementId = "DET" + dlvEntry.DeviceElement,
                            Type = DDIValueType.ProcessData,
                            DDI = ddi
                        }).ToList();


            //Find all assigned machines and in such, search for the properties.
            var properties = isoTask.DeviceAllocation.ToList()
                //First Filter all DANs for those that are not planned. Only filter if an AllocationStamp is available anyway)
                .Where(deviceAllocation => deviceAllocation.AllocationStamp == null || deviceAllocation.AllocationStamp.Type != ISOType.Planned)
                //Select all DeviceAllocations (DANs) and read the DET referencess
                .Select(deviceAllocation => deviceAllocation.DeviceIdRef).ToList()
                //So, here we have a list of DeviceElements (DETs) by their Reference ("DVC-1", "DVC5")
                //Let's deduplicate
                .Distinct()
                //Next, find a list of all corresponding machines for those DVCs
                .Select(deviceRef => _isoxml.Data.Device.FirstOrDefault(entry => entry.DeviceId == deviceRef)).ToList()
                //Now, go through the list of DeviceProperties and check, if our DDI is available there
                .SelectMany(device => device.DeviceProperty
                    //Find the ObjectID for our DPT
                    .Where(dpt => Utils.ConvertDDI(dpt.DevicePropertyDDI) == ddi)
                    .Select(dpt => dpt.DevicePropertyObjectId)
                    //which is then linked in the DeviceObjectRelation(DOR) of a DET
                    .SelectMany(dptObjectId =>
                            device.DeviceElement
                            //Now, if this DOR links our DPT from the machine in the Task, we found an entry to add
                            .Where( det => det.DeviceObjectReference.Any(dor => dor.DeviceObjectId == dptObjectId))
                            .Select(det => new TaskDDIEntry()
                            {
                                DeviceElementId = det.DeviceElementId,
                                Type = DDIValueType.Property,
                                DDI = ddi
                            }).ToList()
                           ).ToList()
                    ).ToList();
            processData.AddRange(properties);
            return processData;
        }

        public CulturalPracticeInfo GetTaskCulturalPractices(ISOTask isoTask)
        {
            var result = new CulturalPracticeInfo();
            //all ddi values must me in constants
            var elements = FindDeviceElementsForDDI(isoTask, (ushort)DDIList.ActualCulturalPractice);
            var elementWorkTime = new List<WorkingTimeInfo>();

            foreach (var element in elements)
            {
                var logs = isoTask.GetTaskExtract((ushort)DDIList.ActualWorkState, element.DeviceElementNo());
                var info = new WorkingTimeInfo
                {
                    Duration = 0.0,
                    StartDate = null,
                    EndDate = null,
                };
                DateTime? startTimestamp = null;
                foreach (var log in logs)
                {
                    foreach (var logData in log.Data)
                    {
                        if (logData.HasValue)
                        {
                            if (startTimestamp == null && logData.DDIValue == 1)
                            {
                                info.StartDate ??= logData.TimeStamp;
                                startTimestamp = logData.TimeStamp;
                            }
                            else if(startTimestamp != null && logData.DDIValue == 0)
                            {
                                info.Duration += (logData.TimeStamp - startTimestamp.Value).TotalSeconds;
                                startTimestamp = null;
                                info.EndDate = logData.TimeStamp;
                            }
                        }
                    }
                }
                elementWorkTime.Add(info);
            }

            var workingElement = elementWorkTime.OrderByDescending(s => s.Duration).FirstOrDefault();
            result.DeviceElementId = workingElement.DdiEntry.DeviceElementId;
            result.DurationInSeconds = workingElement.Duration;
            result.StartDateTime = workingElement.StartDate.GetValueOrDefault();
            result.StopDateTime = workingElement.EndDate.GetValueOrDefault();

            var device = _isoxml.Data.Device.FirstOrDefault(s => s.DeviceElement.Any(s => s.DeviceElementId == workingElement.DdiEntry.DeviceElementId));
            result.DeviceId = device.DeviceId;
            if (workingElement.DdiEntry.Type == DDIValueType.Property)
            {
                var properties = device.DeviceProperty.Where(s => s.DevicePropertyDDI == Utils.FormatDDI(179));
                var elementDevice = device.DeviceElement.FirstOrDefault(s => s.DeviceElementId == workingElement.DdiEntry.DeviceElementId);
                var property = properties.FirstOrDefault(s => elementDevice.DeviceObjectReference.Any(dor => dor.DeviceObjectId == s.DevicePropertyObjectId));
                result.CulturalPractice = (CulturalPracticesType)property.DevicePropertyValue;
                result.IsPropertySource = true;
            }
            else if (isoTask.TryGetLastValue((ushort)DDIList.ActualCulturalPractice, workingElement.DdiEntry.DeviceElementNo(), out var ddiValue, false))
            {
                result.CulturalPractice = (CulturalPracticesType)ddiValue;
            }
            else
            {
                var client = new ClientName(device.ClientNAME);
                result.CulturalPractice = MapDeviceClassToPracticeType(client.DeviceClass);
            }
            return result;
        }

        private CulturalPracticesType MapDeviceClassToPracticeType(DeviceClass className) => className switch
        {
            DeviceClass.NonSpecificSystem => CulturalPracticesType.Unknown,
            DeviceClass.Tractor => CulturalPracticesType.Unknown,
            DeviceClass.PrimarySoilTillage => CulturalPracticesType.Tillage,
            DeviceClass.SecondarySoilTillage => CulturalPracticesType.Tillage,
            DeviceClass.PlantersSeeders => CulturalPracticesType.SowingAndPlanting,
            DeviceClass.Fertilizer => CulturalPracticesType.Fertilizing,
            DeviceClass.Sprayers => CulturalPracticesType.CropProtection,
            DeviceClass.Harvesters => CulturalPracticesType.Harvesting,
            DeviceClass.RootHarvester => CulturalPracticesType.Harvesting,
            DeviceClass.ForageHarvester => CulturalPracticesType.ForageHarvesting,
            DeviceClass.Irrigation => CulturalPracticesType.Irrigation,
            DeviceClass.TransportTrailers => CulturalPracticesType.Transport,
            DeviceClass.FarmyardWork => CulturalPracticesType.Unknown,
            DeviceClass.PoweredAuxilaryUnits => CulturalPracticesType.Unknown,
            DeviceClass.SpecialCrops => CulturalPracticesType.Unknown,
            DeviceClass.MunicipalWork => CulturalPracticesType.Unknown,
            DeviceClass.UnDefined16 => CulturalPracticesType.Unknown,
            DeviceClass.SensorSystem => CulturalPracticesType.Unknown,
            DeviceClass.ReservedForFutureAssignment => CulturalPracticesType.Unknown,
            DeviceClass.TimberHarvesters => CulturalPracticesType.Harvesting,
            DeviceClass.Forwarders => CulturalPracticesType.Transport,
            DeviceClass.TimberLoaders => CulturalPracticesType.Transport,
            DeviceClass.TimberProcessingMachines => CulturalPracticesType.Unknown,
            DeviceClass.Mulchers => CulturalPracticesType.Mulching,
            DeviceClass.UtilityVehicles => CulturalPracticesType.Unknown,
            DeviceClass.FeederMixer => CulturalPracticesType.Unknown,
            DeviceClass.SlurryApplicators => CulturalPracticesType.SlurryManureApplication,
            DeviceClass.Reserved => CulturalPracticesType.Unknown,
            _ => CulturalPracticesType.Unknown
        };
    }
}
