using System;
using System.Collections.Generic;
using System.Linq;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.Converters;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

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
        public string DeviceIdRef { get; set; }
        public double Duration { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ISOTaskAnalysis
    {
        private readonly ISOXML _isoxml;
        private readonly ISODeviceAnalysis _isoDeviceAnalysis;

        public ISOTaskAnalysis(ISOXML isoxml)
        {
            _isoxml = isoxml;
            _isoDeviceAnalysis = new ISODeviceAnalysis(isoxml);
        }


        private WorkingTimeInfo GetDeviceElementActivityDurationInTask(ISOTask isoTask, TaskDDIEntry element)
        {
            var logs = isoTask.GetTaskExtract((ushort)DDIList.ActualWorkState, element.DeviceElementNo(), "WorkState", true);
            var device = _isoDeviceAnalysis.GetDeviceFromDeviceElement(element.DeviceElementId);
            var info = new WorkingTimeInfo
            {
                Duration = 0.0,
                StartDate = null,
                EndDate = null,
                DdiEntry = element,
                DeviceIdRef = device != null ? device.DeviceId : ""
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
        /// Return all CulturalPractices from a given Task. Useful if you want to filter for multiple CulturalPractices
        /// </summary>
        /// <param name="isoTask"></param>
        /// <param name="includeUnknownPractices">If activated, all those data for an unknown cultural practice and those with a duration of 0 are removed</param>
        /// <returns>CulturalPracticeInfo enum value</returns>
        public List<CulturalPracticeInfo> FindTaskCulturalPracticesList(ISOTask isoTask, bool includeUnknownPractices = true)
        {
            var result = new List<CulturalPracticeInfo>();
            var deviceAnalysis = new ISODeviceAnalysis(_isoxml);
            var elementWorkTimes = new List<WorkingTimeInfo>();


            //First find those devices in the task where we have an actual cultural practice.
            var acpElements = deviceAnalysis.FindDeviceElementsForDDI(isoTask, (ushort)DDIList.ActualCulturalPractice);
            foreach (var element in acpElements)
            {
                var workingInfo = GetDeviceElementActivityDurationInTask(isoTask, element);
                if (workingInfo.Duration > 0.0)
                {
                    elementWorkTimes.Add(workingInfo);
                }
            }


            //Now check if there are devices left that are used but don't have ActualCulturalPractice in it
            var irrelevantClasses = new DeviceClass[] {
            DeviceClass.UnDefined16,
            DeviceClass.Reserved,
            DeviceClass.NonSpecificSystem,
            DeviceClass.ReservedForFutureAssignment,
            DeviceClass.SensorSystem,
            DeviceClass.Tractor,
            DeviceClass.UtilityVehicles
            };
            var detsWithWorkState = deviceAnalysis.FindDeviceElementsForDDI(isoTask, (ushort)DDIList.ActualWorkState);
            foreach (var detEntry in detsWithWorkState)
            {
                if (!elementWorkTimes.Any(entry => entry.DdiEntry != null && entry.DdiEntry.DeviceElementId == detEntry.DeviceElementId))
                {
                    ISODevice dvc = deviceAnalysis.GetDeviceFromDeviceElement(detEntry.DeviceElementId);
                    if (dvc != null)
                    {
                        var client = new ClientName(dvc.ClientNAME);
                        if (includeUnknownPractices || Array.IndexOf(irrelevantClasses, client.DeviceClass) == -1)
                        {
                            var workingInfo = GetDeviceElementActivityDurationInTask(isoTask, detEntry);
                            if (workingInfo.Duration > 0.0)
                            {
                                elementWorkTimes.Add(workingInfo);
                            }
                        }
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

                if (workingElement.DdiEntry == null || workingElement.DdiEntry.DDI != (ushort)DDIList.ActualCulturalPractice)
                {
                    var client = new ClientName(device.ClientNAME);
                    cpt.CulturalPractice = DeviceClassConversion.MapDeviceClassToPracticeType(client.DeviceClass);
                    cpt.Source = CulturalPracticeSourceType.ClientName;

                }
                else if (workingElement.DdiEntry.Type == DDIValueType.Property)
                {
                    var properties = device.DeviceProperty.Where(s => s.DevicePropertyDDI.SequenceEqual(DDIUtils.FormatDDI((ushort)DDIList.ActualCulturalPractice)));
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
                if (includeUnknownPractices || cpt.CulturalPractice != CulturalPracticesType.Unknown)
                {
                    result.Add(cpt);
                }
            }
            return result;
        }


        /// <summary>
        /// Return the most significant Cultural Practice for a given Task.
        /// Prefer known CulturalPractices over unknowns
        /// </summary>
        /// <param name="isoTask"></param>
        /// <returns>An actual Cultural Practice and the corresponding duration</returns>
        public CulturalPracticeInfo FindTaskCulturalPractice(ISOTask isoTask)
        {
            var culturalPractices = FindTaskCulturalPracticesList(isoTask).OrderByDescending(entry => entry.DurationInSeconds);
            return culturalPractices
                //Prefer returning a known cultural practise
                .FirstOrDefault(entry => entry.CulturalPractice != CulturalPracticesType.Unknown) ??
                //If none exists, try at least to return the longest time
                culturalPractices.FirstOrDefault() ??
                //Otherwise, at least don't return a nullpointer
                new CulturalPracticeInfo()
                {
                    DurationInSeconds = 0,
                    Source = CulturalPracticeSourceType.None,
                    CulturalPractice = CulturalPracticesType.Unknown
                }
            ;
        }
    }
}
