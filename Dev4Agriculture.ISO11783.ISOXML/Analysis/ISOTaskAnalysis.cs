using System;
using System.Collections.Generic;
using System.Linq;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.Analysis
{
    public enum CulturalPracticeSourceType
    {
        Ddi179 = 0,
        ClientName = 1,
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

        /// <summary>
        /// Return CulturalPractice from device working element with biggest working time
        /// </summary>
        /// <param name="isoTask"></param>
        /// <returns>CulturalPracticeInfo enum value</returns>
        public CulturalPracticeInfo GetTaskCulturalPractice(ISOTask isoTask)
        {
            var result = new CulturalPracticeInfo();
            var deviceAnalysis = new ISODeviceAnalysis(_isoxml);
            var elements = deviceAnalysis.FindDeviceElementsForDDI(isoTask, (ushort)DDIList.ActualCulturalPractice);
            var elementWorkTimes = new List<WorkingTimeInfo>();

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
                elementWorkTimes.Add(info);
            }

            var workingElement = elementWorkTimes.OrderByDescending(s => s.Duration).FirstOrDefault();
            result.DeviceElementId = workingElement.DdiEntry.DeviceElementId;
            result.DurationInSeconds = workingElement.Duration;
            result.StartDateTime = workingElement.StartDate.GetValueOrDefault();
            result.StopDateTime = workingElement.EndDate.GetValueOrDefault();

            var device = _isoxml.Data.Device.FirstOrDefault(s => s.DeviceElement.Any(s => s.DeviceElementId == workingElement.DdiEntry.DeviceElementId));
            result.DeviceId = device.DeviceId;
            if (workingElement.DdiEntry.Type == DDIValueType.Property)
            {
                var properties = device.DeviceProperty.Where(s => s.DevicePropertyDDI == Utils.FormatDDI((ushort)DDIList.ActualCulturalPractice));
                var elementDevice = device.DeviceElement.FirstOrDefault(s => s.DeviceElementId == workingElement.DdiEntry.DeviceElementId);
                var property = properties.FirstOrDefault(s => elementDevice.DeviceObjectReference.Any(dor => dor.DeviceObjectId == s.DevicePropertyObjectId));
                result.CulturalPractice = (CulturalPracticesType)property.DevicePropertyValue;
                result.Source = CulturalPracticeSourceType.Ddi179;
            }
            else if (isoTask.TryGetLastValue((ushort)DDIList.ActualCulturalPractice, workingElement.DdiEntry.DeviceElementNo(), out var ddiValue, false))
            {
                result.CulturalPractice = (CulturalPracticesType)ddiValue;
                result.Source = CulturalPracticeSourceType.Ddi179;
            }
            else
            {
                var client = new ClientName(device.ClientNAME);
                result.CulturalPractice = Utils.MapDeviceClassToPracticeType(client.DeviceClass);
                result.Source = CulturalPracticeSourceType.ClientName;
            }
            return result;
        }
    }
}
