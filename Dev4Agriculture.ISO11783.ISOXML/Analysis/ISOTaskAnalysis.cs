using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.Analysis
{
    public enum CulturalPracticeSourceType
    {
        Ddi179 = 0,
        ClientName = 1,
        None = 2,
    }

    public class CulturalPracticeInfo
    {
        public CulturalPracticesType CulturalPractice { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime StopDateTime { get; set; }
        public double DurationInSeconds { get; set; }
        public string DeviceElementId { get; set; }
        public string DeviceId { get; set; }
        public CulturalPracticeSourceType Source { get; set; }
    }

    internal class WorkingTimeInfo
    {
        public TaskDDIEntry DdiEntry { get; set; }
        public double Duration { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ISOTaskAnalysis
    {
        private ISOXML _isoxml;

        public ISOTaskAnalysis(ISOXML isoxml)
        {
            _isoxml = isoxml;
        }


        private Dictionary<string,WorkingTimeInfo> GetDeviceActivityDurationsFromDeviceAllocation(ISOTask isoTask)
        {
            var dvcDurations = new Dictionary<string, WorkingTimeInfo>();
            foreach (var dan in isoTask.DeviceAllocation)
            {
                if (!dvcDurations.Keys.Contains(dan.DeviceIdRef))
                {
                    dvcDurations.Add(dan.DeviceIdRef, new WorkingTimeInfo());
                }

                if (dan.AllocationStamp != null)
                {
                    if (dvcDurations[dan.DeviceIdRef].StartDate == null || dvcDurations[dan.DeviceIdRef].StartDate > dan.AllocationStamp.Start)
                    {
                        dvcDurations[dan.DeviceIdRef].StartDate = dan.AllocationStamp.Start;
                    }

                    if (dvcDurations[dan.DeviceIdRef].EndDate == null || dvcDurations[dan.DeviceIdRef].EndDate > dan.AllocationStamp.Stop)
                    {
                        dvcDurations[dan.DeviceIdRef].EndDate = dan.AllocationStamp.Stop;
                    }
                    dvcDurations[dan.DeviceIdRef].Duration += dan.AllocationStamp.GetSeconds();

                }

            }
            return dvcDurations;
        }


        private WorkingTimeInfo GetDeviceElementActivityDurationInTask(ISOTask isoTask, TaskDDIEntry element)
        {
            var logs = isoTask.GetTaskExtract((ushort)DDIList.ActualWorkState, element.DeviceElementNo(), "WorkState", true);
            var info = new WorkingTimeInfo
            {
                Duration = 0.0,
                StartDate = null,
                EndDate = null,
                DdiEntry = element
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
                        else if (startTimestamp != null && logData.DDIValue == 0)
                        {
                            info.Duration += (logData.TimeStamp - startTimestamp.Value).TotalSeconds;
                            startTimestamp = null;
                            info.EndDate = logData.TimeStamp;
                        }
                    }
                }
                if (info.EndDate == null && info.StartDate != null)
                {
                    info.EndDate = log.Data.Last().TimeStamp;
                    info.Duration += (info.EndDate.Value - startTimestamp.Value).TotalSeconds;
                }
            }
            return info;
        }

        /// <summary>
        /// Return CulturalPractice from device working element with biggest working time
        /// </summary>
        /// <param name="isoTask"></param>
        /// <returns>CulturalPracticeInfo enum value</returns>
        public List<CulturalPracticeInfo> GetTaskCulturalPractices(ISOTask isoTask)
        {
            var result = new List<CulturalPracticeInfo>();
            var deviceAnalysis = new ISODeviceAnalysis(_isoxml);
            var elementWorkTimes = new List<WorkingTimeInfo>();
            //First find those devices in the task where we have an actual cultural practice.
            var acpElements = deviceAnalysis.FindDeviceElementsForDDI(isoTask, (ushort)DDIList.ActualCulturalPractice);
            foreach (var element in acpElements)
            {
                var info = GetDeviceElementActivityDurationInTask(isoTask, element);
                if (info.Duration > 0.0)
                {
                    elementWorkTimes.Add(info);
                }
            }



            //Additionally find those, that don't have the ACP but were active
            var dvcDurations = GetDeviceActivityDurationsFromDeviceAllocation(isoTask);
            if (dvcDurations.Any())
            {


                var irrelevantClasses = new DeviceClass[] {
                DeviceClass.UnDefined16,
                DeviceClass.Reserved,
                DeviceClass.NonSpecificSystem,
                DeviceClass.ReservedForFutureAssignment,
                DeviceClass.SensorSystem,
                DeviceClass.Tractor,
                DeviceClass.UtilityVehicles
                };

                //Find all devices used in the dvcDurations to be able to access the ClientName
                var devices = _isoxml.Data.Device.Where(entry => dvcDurations.Any(idRef => idRef.Key == entry.DeviceId));
                foreach (var dvc in devices)
                {
                    var client = new ClientName(dvc.ClientNAME);
                    if (Array.IndexOf(irrelevantClasses, client.DeviceClass) == -1)
                    {
                        result.Add(new CulturalPracticeInfo()
                        {
                            CulturalPractice = Utils.MapDeviceClassToPracticeType(client.DeviceClass),
                            Source = CulturalPracticeSourceType.ClientName,
                            DeviceId = dvc.DeviceId,
                            DurationInSeconds = dvcDurations[dvc.DeviceId].Duration,
                            StartDateTime = dvcDurations[dvc.DeviceId].StartDate ?? DateTime.MinValue,
                            StopDateTime = dvcDurations[dvc.DeviceId].EndDate ?? DateTime.MaxValue,
                        });
                    }
                }
            }

            foreach (var workingElement in elementWorkTimes)
            {
                var device = _isoxml.Data.Device.FirstOrDefault(s => s.DeviceElement.Any(s => s.DeviceElementId == workingElement.DdiEntry.DeviceElementId));
                var cpt = new CulturalPracticeInfo()
                {
                    DeviceElementId = workingElement.DdiEntry.DeviceElementId,
                    DurationInSeconds = workingElement.Duration,
                    StartDateTime = workingElement.StartDate.GetValueOrDefault(),
                    StopDateTime = workingElement.EndDate.GetValueOrDefault(),
                    DeviceId = device.DeviceId
                };

                if (workingElement.DdiEntry == null)
                {
                    var client = new ClientName(device.ClientNAME);
                    cpt.CulturalPractice = Utils.MapDeviceClassToPracticeType(client.DeviceClass);
                    cpt.Source = CulturalPracticeSourceType.ClientName;

                }
                else if (workingElement.DdiEntry.Type == DDIValueType.Property)
                {
                    var properties = device.DeviceProperty.Where(s => s.DevicePropertyDDI.SequenceEqual(Utils.FormatDDI((ushort)DDIList.ActualCulturalPractice)));
                    var elementDevice = device.DeviceElement.FirstOrDefault(s => s.DeviceElementId == workingElement.DdiEntry.DeviceElementId);
                    var property = properties.FirstOrDefault(s => elementDevice.DeviceObjectReference.Any(dor => dor.DeviceObjectId == s.DevicePropertyObjectId));
                    cpt.CulturalPractice = (CulturalPracticesType)property.DevicePropertyValue;
                    cpt.Source = CulturalPracticeSourceType.Ddi179;
                }
                else if (isoTask.TryGetLastValue((ushort)DDIList.ActualCulturalPractice, workingElement.DdiEntry.DeviceElementNo(), out var ddiValue, false))
                {
                    cpt.CulturalPractice = (CulturalPracticesType)ddiValue;
                    cpt.Source = CulturalPracticeSourceType.Ddi179;
                    result.Add(cpt);
                }
                result.Add(cpt);
            }
            return result;
        }

        public CulturalPracticeInfo GetTaskCulturalPractice(ISOTask isoTask)
        {
            return GetTaskCulturalPractices(isoTask).OrderByDescending(entry => entry.DurationInSeconds).FirstOrDefault() ?? new CulturalPracticeInfo()
            {
                DurationInSeconds = 0,
                Source = CulturalPracticeSourceType.None,
                CulturalPractice = CulturalPracticesType.Unknown
            }
            ;
        }
    }
}
