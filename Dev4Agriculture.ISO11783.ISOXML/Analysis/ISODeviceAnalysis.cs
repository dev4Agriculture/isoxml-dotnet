using System.Collections.Generic;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

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

        public ISODevice GetDeviceFromDeviceElement(string deviceElementId)
        {
            return _isoxml.Data.Device.FirstOrDefault(dvc => dvc.DeviceElement.Any(det => det.DeviceElementId == deviceElementId));
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
        public ISODeviceProcessData FindDeviceProcessData(TaskDDIEntry entry, ISODevice device = null)
        {
            if (entry.Type != DDIValueType.ProcessData)
            {
                return null;
            }
            device ??= GetDeviceFromDeviceElement(entry.DeviceElementId);
            var dorList = device.DeviceElement.Where(det => det.DeviceElementId == entry.DeviceElementId)
                .SelectMany(det => det.DeviceObjectReference).Select(dor => dor.DeviceObjectId).ToList();
            return
                device.DeviceProcessData.FirstOrDefault(dpd =>
                    DDIUtils.ConvertDDI(dpd.DeviceProcessDataDDI) == entry.DDI &&
                    dorList.Contains(dpd.DeviceProcessDataObjectId)
                );
        }

        /// <summary>
        /// Get the full DeviceProperty to read e.g. the Value
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public ISODeviceProperty FindDeviceProperty(TaskDDIEntry entry, ISODevice device = null)
        {
            if (entry.Type != DDIValueType.Property)
            {
                return null;
            }

            device ??= GetDeviceFromDeviceElement(entry.DeviceElementId);
            var dorList = device.DeviceElement.Where(det => det.DeviceElementId == entry.DeviceElementId)
                .SelectMany(det => det.DeviceObjectReference).Select(dor => dor.DeviceObjectId).ToList();

            return device.DeviceProperty.FirstOrDefault(dpd =>
                DDIUtils.ConvertDDI(dpd.DevicePropertyDDI) == entry.DDI &&
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
                    return FindDeviceProcessData(entry)?.DeviceProcessDataDesignator ?? "";


                case DDIValueType.Property:
                    return FindDeviceProperty(entry)?.DevicePropertyDesignator ?? "";

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
            var device = GetDeviceFromDeviceElement(entry.DeviceElementId);
            switch (entry.Type)
            {
                case DDIValueType.ProcessData:
                    var dpd = FindDeviceProcessData(entry);
                    if (dpd != null && dpd.DeviceValuePresentationObjectIdValueSpecified)
                    {
                        return device.DeviceValuePresentation.FirstOrDefault(dvp => dvp.DeviceValuePresentationObjectId == dpd.DeviceValuePresentationObjectId);
                    }
                    break;


                case DDIValueType.Property:
                    var dpt = FindDeviceProperty(entry);
                    if (dpt != null && dpt.DeviceValuePresentationObjectIdValueSpecified)
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
                    .Where(dpt => DDIUtils.ConvertDDI(dpt.DevicePropertyDDI) == ddi)
                    .Select(dpt => dpt.DevicePropertyObjectId)
                    //which is then linked in the DeviceObjectRelation(DOR) of a DET
                    .SelectMany(dptObjectId =>
                            device.DeviceElement
                            //Now, if this DOR links our DPT from the machine in the Task, we found an entry to add
                            .Where(det => det.DeviceObjectReference.Any(dor => dor.DeviceObjectId == dptObjectId))
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

    }
}
