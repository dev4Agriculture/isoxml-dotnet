using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.Analysis
{

    public enum DeviceValueType
    {
        None = 0,
        ProcessData = 1,
        Property = 2
    }

    public class TaskDDIEntry
    {
        public string DeviceElement;
        public DeviceValueType Type;
    }


    public class ISODeviceAnalysis
    {
        private ISOXML _isoxml;
        private ISODeviceAnalysis(ISOXML isoxml)
        {
            _isoxml = isoxml;
        }

        public List<TaskDDIEntry> FindDeviceElementsForDDI(ISOTask isoTask, uint ddi)
        {
            var processData = isoTask.TimeLogs.ToList().SelectMany(entry => entry.Header.Ddis)
                        .Where(dlv => dlv.Ddi == ddi)  //Find DataLogValues with DDI; those list the DET as well
                        .Select(dlvEntry => new TaskDDIEntry()
                        {
                            DeviceElement = "DET" + dlvEntry.DeviceElement,
                            Type = DeviceValueType.ProcessData
                        }).ToList();


            //Find all assigned machines and in such, search for the properties.
            var properties = isoTask.DeviceAllocation.ToList()
                //First Filter all DANs for those that are not planned)
                .Where(deviceAllocation => deviceAllocation.AllocationStamp != null && deviceAllocation.AllocationStamp.Type != ISOType.Planned)
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
                    .Select(dpt => (Utils.ConvertDDI(dpt.DevicePropertyDDI) == ddi) ? dpt.DeviceValuePresentationObjectId : null)
                    //which is then linked in the DeviceObjectRelation(DOR) of a DET
                    .SelectMany(dpt =>
                            device.DeviceElement
                           .Select(det => {
                               //Now, if this DOR links our DPT from the machine in the Task, we found an entry to add
                               if (det.DeviceObjectReference.Any(dor => dor.DeviceObjectId == dpt)) {
                                   return new TaskDDIEntry()
                                   {
                                       DeviceElement = "DET" + det.DeviceElementId,
                                       Type = DeviceValueType.Property
                                   };
                               };
                               return null;
                           }).ToList()
                           ).ToList()
                    ).ToList();

            processData.AddRange(properties );
            return processData;
        }


    }
}
