using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.DTO;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using System.Diagnostics;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{



    public partial class ISOTask
    {

        [XmlIgnore]
        public List<ISOTLG> TimeLogs = new List<ISOTLG>();

        internal void InitTimeLogList(Dictionary<string, ISOTLG> timeLogs)
        {
            foreach (var tlg in TimeLog)
            {
                if (timeLogs.TryGetValue(tlg.Filename, out var isoTLG))
                {
                    TimeLogs.Add(isoTLG);
                }
            }
        }

        /// <summary>
        /// A function to extract all Positions + Times + One Value for a specific DDI in a Specific DeviceElement; In a list of Lists; one per TimeLog
        /// </summary>
        /// <param name="ddi"> The DDI; see isobus.net</param>
        /// <param name="det"> The DeviceElement. E.g. "DET-1" would be -1; "DET1" would be 1</param>
        /// <param name="name">An optional designator</param>
        /// <param name="fillLines">An optional boolean. If true, all Positions and Times are used. In case a value is not present, the latest known value is used</param>
        /// <returns> A List of Points with Time and Value</returns>
        public List<ISOTLGExtract> GetTaskExtract(ushort ddi, int det, string name = "", bool fillLines = false)
        {
            var extracts = new List<ISOTLGExtract>();
            foreach (var tlg in TimeLogs)
            {
                extracts.Add(ISOTLGExtract.FromTimeLog(tlg, ddi, det, name, fillLines));
            }
            return extracts;
        }


        /// <summary>
        /// A function to extract all Positions + Times + One Value for a specific DDI in a Specific DeviceElement; Merged as one List
        /// </summary>
        /// <param name="ddi"> The DDI; see isobus.net</param>
        /// <param name="det"> The DeviceElement. E.g. "DET-1" would be -1; "DET1" would be 1</param>
        /// <param name="name">An optional designator</param>
        /// <param name="fillLines">An optional boolean. If true, all Positions and Times are used. In case a value is not present, the latest known value is used</param>
        /// <returns> A List of Points with Time and Value</returns>
        public ISOTLGExtract GetMergedTaskExtract(ushort ddi, int det, string name = "", bool fillLines = false)
        {
            var extracts = new List<ISOTLGExtract>();
            var lastValue = Constants.TLG_VALUE_FOR_NO_VALUE;
            foreach (var tlg in TimeLogs)
            {
                var entry = ISOTLGExtract.FromTimeLog(tlg, ddi, det, name, fillLines, lastValue);
                if (fillLines && entry.Data.Count > 0)
                {
                    lastValue = entry.Data.LastOrDefault()?.DDIValue ?? 0;
                }
                extracts.Add(entry);
            }
            var merge = new List<ISOTLGExtractPoint>();
            extracts.ForEach(entry => merge.AddRange(entry.Data));
            var result = new ISOTLGExtract(ddi, det, name, merge);
            return result;
        }


        /// <summary>
        /// Return the number of available TimeLogs
        /// </summary>
        /// <returns></returns>
        public int CountTimeLogs()
        {
            return TimeLogs.Count;
        }

        /// <summary>
        /// The DefaultDataLogTrigger forces Terminals to request the DefaultSet from machines.
        /// This leads to receiving as many data as the machine thinks is smart. and useful.
        /// </summary>
        public void AddDefaultDataLogTrigger()
        {
            if (!DataLogTrigger.Any(entry => DDIUtils.ConvertDDI(entry.DataLogDDI) == (ushort)DDIList.RequestDefaultProcessData))
            {
                DataLogTrigger.Add(new ISODataLogTrigger()
                {
                    DataLogDDI = DDIUtils.FormatDDI(DDIList.RequestDefaultProcessData),
                    DataLogMethod = (byte)(TriggerMethods.OnTime
                    | TriggerMethods.OnDistance
                    | TriggerMethods.ThresholdLimits
                    | TriggerMethods.OnChange
                    | TriggerMethods.Total)

                });
            }
        }



        /// <summary>
        /// Get the maximum available Value (Raw Value!) from a Task and a specific DeviceElement.
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="deviceElement"></param>
        /// <param name="maximum"> An OUT-Variable that receives the maximum value</param>
        /// <returns>True if any value could be found</returns>
        public bool TryGetMaximum(ushort ddi, int deviceElement, out int maximum)
        {
            maximum = int.MinValue;
            var found = false;
            foreach (var tlg in TimeLogs)
            {
                if (tlg.TryGetMaximum(ddi, deviceElement, out var compare))
                {
                    found = true;
                    if (maximum < compare)
                    {
                        maximum = compare;
                    }
                }
            }
            return found;
        }


        /// <summary>
        /// Read the Area on which TimeLog Points are loaded
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns>True if a Bound could be found</returns>
        public bool TryGetBounds(out AreaBounds bounds)
        {
            bounds = new AreaBounds();
            if (TimeLogs.Count == 0)
            {
                return false;
            }
            var found = false;
            foreach (var timeLog in TimeLogs)
            {
                if (timeLog.TryGetTLGBounds(out var tlgBounds))
                {
                    found = true;
                    bounds.Update(tlgBounds.MinLat, tlgBounds.MinLong);
                    bounds.Update(tlgBounds.MaxLat, tlgBounds.MaxLong);
                }
            }

            return found;
        }


        /// <summary>
        /// Get the minimum available Value (Raw Value!) from a Task and a specific DeviceElement.
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="deviceElement"></param>
        /// <param name="minimum"> An OUT-Variable that receives the minimum value</param>
        /// <returns>True if any value could be found</returns>
        public bool TryGetMinimum(ushort ddi, int deviceElement, out int minimum)
        {
            minimum = int.MaxValue;
            var found = false;
            foreach (var tlg in TimeLogs)
            {
                if (tlg.TryGetMinimum(ddi, deviceElement, out var compare))
                {
                    found = true;
                    if (minimum > compare)
                    {
                        minimum = compare;
                    }
                }
            }
            return found;
        }




        /// <summary>
        /// Get the first available Value (Raw Value!) from a Task and a specific DeviceElement.
        /// This is useful specifically for onChange values as some (older) TaskControllers do not write such value by default
        /// On start of a Task but only if it changes.
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="deviceElement"></param>
        /// <param name="firstValue"> An OUT-Variable that receives the result</param>
        /// <returns>True if any value could be found</returns>
        public bool TryGetFirstValue(ushort ddi, int deviceElement, out int firstValue)
        {
            foreach (var tlg in TimeLogs)
            {
                if (TryGetFirstValue(ddi, deviceElement, out firstValue))
                {
                    return true;
                }
            }
            firstValue = 0;
            return false;
        }


        /// <summary>
        /// Get the last available Value (Raw Value!) from a Task and a specific DeviceElement.
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="deviceElement"></param>
        /// <param name="firstValue"> An OUT-Variable that receives the result</param>
        /// <param name="shallCheckTimeElements">If true (Default!), the TIM-Elements is checked if no data was found in the TimeLogs</param>
        /// <returns>True if any value was found</returns>
        public bool TryGetLastValue(ushort ddi, int deviceElement, out int lastValue, bool shallCheckTimeElements = true)
        {
            for (var index = TimeLogs.Count - 1; index >= 0; index--)
            {
                if (TimeLogs[index].TryGetLastValue(ddi, deviceElement, out lastValue))
                {
                    return true;
                }
            }
            if (shallCheckTimeElements)
            {
                var endTime = Time.Max(entry => entry.Start);
                if (Time.FirstOrDefault(entry => entry.Start == endTime)?.TryGetDDIValue(ddi, deviceElement, out lastValue) ?? false)
                {
                    return true;
                }
            }
            lastValue = 0;

            return false;
        }




        /// <summary>
        /// Finds a total for DDI + DeviceElement based on the requested Algorithm.
        /// </summary>
        /// <param name="ddi">The Data Dictionary Identifier</param>
        /// <param name="deviceElement">The Data Dictionary Identifier</param>
        /// <param name="totalValue">The RETURNED Total Value</param>
        /// <param name="devices">All available Devices; possible relevant for some Totals Functions</param>
        /// <param name="shallCheckTimeElements">If true (Default!), the TIM-Elements is checked if no data was found in the TimeLogs</param>
        /// <returns>True if Value was found</returns>
        public bool TryGetTotalValue(ushort ddi, int deviceElement, out int totalValue, List<ISODevice> devices, bool shallCheckTimeElements = true)
        {
            if(deviceElement == 0)
            {
                foreach(var curDevice in devices)
                {
                    if (curDevice.IsDeviceProcessData(ddi))
                    {
                        if(curDevice.TryFindDeviceElementForDDI(ddi, out var deviceElements))
                        {
                            deviceElement = IdList.ToIntId(deviceElements.First().DeviceElementId);
                            break;
                        }
                    }
                }
            }

            if(deviceElement == 0)
            {
                totalValue = 0;
                return false;
            }

            var found = false;
            var timElements = Time.OrderBy(entry => entry.Start).ToList();
            var index = timElements.Count() - 1;
            while (!found && index > -1)
            {
                if (timElements[index].Type == ISOType2.Effective)
                {
                    var dlv = timElements[index].DataLogValue.FirstOrDefault(
                        dlvEntry => DDIUtils.ConvertDDI(dlvEntry.ProcessDataDDI) == ddi &&
                        IdList.ToIntId(dlvEntry.DeviceElementIdRef) == deviceElement);
                    if (dlv != null)
                    {
                        totalValue = (int)dlv.ProcessDataValue;
                        return true;
                    }
                }
                index--;
            }

            var device = devices.FirstOrDefault(dvc => dvc.DeviceElement.Any(det => IdList.ToIntId(det.DeviceElementId) == deviceElement));

            for (index = TimeLogs.Count - 1; index >= 0; index--)
            {
                if (TimeLogs[index].TryGetTotalValue(ddi, deviceElement, out totalValue, device))
                {
                    return true;
                }
            }

            if (shallCheckTimeElements)
            {
                var endTime = Time.Max(entry => entry.Start);
                if (Time.FirstOrDefault(entry => entry.Start == endTime)?.TryGetDDIValue(ddi, deviceElement, out totalValue) ?? false)
                {
                    return true;
                }
            }
            totalValue = 0;

            return found;

        }


        /// <summary>
        /// Create a list of TimeElements with DataLogValue-Elements for the given Task.
        /// ATTENTION: Only used when the TimeLogs were created in code; normally the TIM-Element exists
        /// </summary>
        /// <param name="devices">The list of devices; used to differentiate between Totals and LifeTimetotals; based on the DeviceDescriptions</param>
        /// <param name="assign">If true, the generated TIM-List replaces the TIM-List in the task. All Non-Effective TIM-Elements are kept</param>
        /// <returns>List of TIM-Elements with DataLogValues</returns>
        public List<ISOTime> GenerateTimeElementsFromTimeLogs(List<ISODevice> devices, bool assign = false)
        {
            if (assign)
            {
                var keptTims = Time
                    .Where(tim => tim.Type != ISOType2.Effective)
                    .ToList();

                Time.Clear();
                keptTims.ForEach(Time.Add);
            }

            var list = new List<ISOTime>();
            var singulator = new ISOTimeLogSingulator();
            for (var index = 0; index < TimeLogs.Count; index++)
            {
                var singulated = singulator.SingulateTimeLog(TimeLogs[index], devices);
                var tim = singulated.GenerateTimeElement(devices);
                list.Add(tim);
                if (assign)
                {
                    Time.Add(tim);
                }
            }

            ISOTimeListEnqueuer.EnqueueTimeElements(list, devices);
            var enqueuer = new ISOTimeLogEnqueuer();
            enqueuer.EnqueueTimeLogs(TimeLogs, devices);
            return list;
        }

        /// <summary>
        /// Create a list of DeviceAllocation elements for the given Task based on TimeLog DDI entries.
        /// Only creates DeviceAllocations for devices that have DeviceElements matching the DDI entries in TimeLog headers.
        /// </summary>
        /// <param name="devices">The list of devices to check for matching DeviceElements</param>
        /// <returns>List of DeviceAllocation elements with AllocationStamps</returns>
        public List<ISODeviceAllocation> GenerateDeviceAllocationsFromTimeLogs(List<ISODevice> devices, bool assign = true)
        {
            var deviceAllocations = new List<ISODeviceAllocation>();

            if (assign)
            {
                DeviceAllocation.Clear();
            }

            // Get all unique DeviceElementIds from TimeLog headers
            foreach (var tlg in TimeLogs)
            {
                if (tlg.Entries.Count == 0)
                {
                    continue;
                }

                var deviceElementIds = tlg.Header.Ddis.Select(entry => entry.DeviceElement).Distinct().ToList();

                // Find devices that have DeviceElements matching the DDI entries in TimeLog headers
                var usedDevices = devices.Where(device =>
                    device.DeviceElement.Any(det =>
                        deviceElementIds.Contains(IdList.ToIntId(det.DeviceElementId))
                    )
                ).Distinct().ToList();

                foreach(var dvc in usedDevices)
                {
                    var deviceAllocation = new ISODeviceAllocation()
                    {
                        DeviceIdRef = dvc.DeviceId,
                        ClientNAMEValue = dvc.ClientNAME,
                        AllocationStamp = new ISOAllocationStamp()
                        {
                            Start = tlg.GetStartTime(),
                            Stop = tlg.GetEndTime(),
                            Type = ISOType.Effective_Realized
                        }
                    };
                    deviceAllocations.Add(deviceAllocation);
                    if (assign)
                    {
                        DeviceAllocation.Add(deviceAllocation);
                    }
                }
            }
            return deviceAllocations;
        }

        /// <summary>
        /// Get the start time of the task from its TimeLogs
        /// </summary>
        /// <returns>Start time of the task</returns>
        private DateTime GetTaskStartTime()
        {
            if (TimeLogs.Count == 0)
                return DateTime.MinValue;

            return TimeLogs
                .Where(tlg => tlg.Entries.Count > 0)
                .Min(tlg => tlg.GetStartTime());
        }

        /// <summary>
        /// Get the end time of the task from its TimeLogs
        /// </summary>
        /// <returns>End time of the task</returns>
        private DateTime GetTaskEndTime()
        {
            if (TimeLogs.Count == 0)
                return DateTime.MinValue;

            return TimeLogs
                .Where(tlg => tlg.Entries.Count > 0)
                .Max(tlg => tlg.GetEndTime());
        }

        public List<ISOTask> SplitAtDateTimes(Dictionary<ISOTask, List<DateTime>> taskSplitTimeCombos, List<ISODevice> devices, int nextTLGNo = 0)
        {
            var splitPoints = new Dictionary<ISOTLG, List<int>>();
            var assignments = new List<TaskSplitEntry>();

            TimeLogs = TimeLogs
                .Where(tlg => tlg.Entries.Count > 0)
                .ToList();

            foreach (var tlg in TimeLogs)
            {
                splitPoints.Add(tlg, new List<int>());
            }

            foreach (var entry in taskSplitTimeCombos)
            {
                foreach (var time in entry.Value)
                {
                    foreach (var timeLog in TimeLogs)
                    {
                        if (timeLog.TryFindClosestIndex(time, out var index))
                        {
                            splitPoints[timeLog].Add(index);
                            assignments.Add(new TaskSplitEntry()
                            {
                                Timestamp = time,
                                Task = entry.Key,
                                Index = index,
                                TimeLog = timeLog
                            });
                            break;
                        }
                    }
                }
            }
            assignments = assignments.OrderBy(entry => entry.Timestamp).ToList();
            var times = assignments.Select(entry => entry.Timestamp).ToList();
            var splitted = SplitAtDateTimes(times, devices, nextTLGNo);
            if(splitted.Count == 0)
            {
                return new List<ISOTask>();
            }

            List<(string, DateTime, DateTime)> pairs = splitted
                .Where(tlg => tlg.Entries.Count > 0)
                .Select(entry => (entry.Name, entry.GetStartTime(), entry.GetEndTime()))
                .ToList();

            var assignmentIndex = 0;
            var resultEntryList = new List<TaskSplitEntry>();
            var splittedTLGIndex = 0;
            var loopShallEnd = false;
            while (splitted[splittedTLGIndex].GetEndTime() < assignments[assignmentIndex].Timestamp)
            {
                splittedTLGIndex++;
            }
            while (assignmentIndex < assignments.Count && splittedTLGIndex < splitted.Count)
            {
                while (splitted[splittedTLGIndex].GetStartTime() > assignments[assignmentIndex].Timestamp)
                {
                    assignmentIndex++;
                    if (assignmentIndex >= assignments.Count)
                    {
                        loopShallEnd = true;
                        break;
                    }
                }
                if (loopShallEnd)
                {
                    break;
                }
                var toAdd = new TaskSplitEntry()
                {
                    Timestamp = splitted[splittedTLGIndex].GetStartTime(),
                    TimeLog = splitted[splittedTLGIndex],
                    Index = 0,
                    Task = assignments[assignmentIndex].Task
                };
                resultEntryList.Add(toAdd);
                splittedTLGIndex++;
            }

            var taskGroups = resultEntryList.GroupBy(entry => entry.Task);
            var taskList = new List<ISOTask>();
            foreach (var groupEntry in taskGroups)
            {
                var task = groupEntry.First().Task;
                var tlgList = groupEntry.OrderBy(entry => entry.Timestamp).Select(entry => entry.TimeLog).ToList();
                task.ReplaceTimeLogs(tlgList, true, devices);
                taskList.Add(task);
            }


            foreach (var task in taskList)
            {
                // Generate DeviceAllocations and TimeElements after replacing TimeLogs
                task.GenerateDeviceAllocationsFromTimeLogs(devices, true);
                task.GenerateTimeElementsFromTimeLogs(devices, true);
            }

            return taskList;
        }

        public List<ISOTLG> SplitAtDateTimes(List<DateTime> splitTimes, List<ISODevice> devices, int nextTLGNo = 0)
        {
            var splitPoints = new Dictionary<ISOTLG, List<int>>();
            var validTimeLogs = TimeLogs
                .Where(tlg => tlg.Entries.Count > 0)
                .ToList();

            foreach (var tlg in validTimeLogs)
            {
                splitPoints.Add(tlg, new List<int>());
            }

            splitTimes.Sort();
            foreach (var time in splitTimes)
            {
                foreach (var timeLog in validTimeLogs)
                {
                    if (timeLog.TryFindClosestIndex(time, out var index))
                    {
                        splitPoints[timeLog].Add(index);
                        break;
                    }
                }
            }

            var generatedTLGs = new List<ISOTLG>();
            foreach (var entry in splitPoints)
            {
                if (entry.Value.Count > 0)
                {
                    var tlgsToAdd = entry.Key.SplitTimeLog(devices, entry.Value, nextTLGNo);
                    nextTLGNo += tlgsToAdd.Count;
                    generatedTLGs.AddRange(tlgsToAdd);
                } else
                {
                    generatedTLGs.Add(entry.Key);
                }
            }
            return generatedTLGs;
        }


        public void SplitTaskAtDateTimes(List<DateTime> splitTimes, List<ISODevice> devices, int nextTLGNo = 0)
        {
            var generatedTLGs = SplitAtDateTimes(splitTimes, devices, nextTLGNo);
            ReplaceTimeLogs(generatedTLGs);

            // Generate DeviceAllocations and TimeElements after replacing TimeLogs
            GenerateDeviceAllocationsFromTimeLogs(devices, true);
            GenerateTimeElementsFromTimeLogs(devices, true);
        }


        /// <summary>
        /// Trys to add a TimeLog to a Task; returns 0 if it already exists
        /// </summary>
        /// <param name="timeLog"></param>
        /// <returns></returns>
        public bool TryAddTimeLog(ISOTLG timeLog)
        {
            if (TimeLogs.Any(entry => entry.Name == timeLog.Name))
            {
                return false;
            }

            if (!TimeLog.Any(entry => entry.Filename == timeLog.Name))
            {
                TimeLog.Add(new ISOTimeLog()
                {
                    Filename = timeLog.Name,
                    TimeLogType = ISOTimeLogType.Binarytimelogfiletype1
                });
            }

            TimeLogs.Add(timeLog);

            return true;
        }

        /// <summary>
        /// Try to remove a TimeLog from a Task; especially usefull for Task Splitting
        /// </summary>
        /// <param name="timeLog"></param>
        /// <returns></returns>
        public bool TryRemoveTimeLog(ISOTLG timeLog)
        {
            if (TimeLog.Any(entry => entry.Filename == timeLog.Name))
            {
                TimeLog.Remove(TimeLog.First(entry => entry.Filename == timeLog.Name));

                if (TimeLogs.Any(entry => entry.Name == timeLog.Name))
                {
                    TimeLogs.Remove(TimeLogs.First(entry => entry.Name == timeLog.Name));
                }
                return true;
            }
            return false;
        }



        /// <summary>
        /// Replace the current TimeLogs within a Task with new TimeLogs and - potentially - update all Effective TIM-Elements
        /// </summary>
        /// <param name="timeLogs"></param>
        /// <param name="updateTimeElements"></param>
        /// <param name="devices"></param>
        public void ReplaceTimeLogs(List<ISOTLG> timeLogs, bool updateTimeElements = true, List<ISODevice> devices = null)
        {
            TimeLog.Clear();
            TimeLogs.Clear();
            foreach (var timeLog in timeLogs)
            {
                TryAddTimeLog(timeLog);
            }

            if (updateTimeElements && devices != null)
            {
                GenerateTimeElementsFromTimeLogs(devices, true);
                GenerateDeviceAllocationsFromTimeLogs(devices, true);
            }

        }


        public List<ISOTreatmentZone> GenerateTreatmentZones(GridZoneDTO zone, bool assign = true)
        {
            switch (zone.Type)
            {
                case ISOGridType.gridtype1:
                    var tznList = new List<ISOTreatmentZone>();
                    var listOfValidIndizes = zone.Layer.SelectMany(layer => layer.GridType1Values.Keys).Distinct();
                    foreach (var index in listOfValidIndizes)
                    {
                        var treatmentZone = new ISOTreatmentZone()
                        {
                            TreatmentZoneCode = index
                        };
                        foreach (var layer in zone.Layer)
                        {
                            treatmentZone.ProcessDataVariable.Add(new ISOProcessDataVariable()
                            {
                                ProcessDataValue = layer.GridType1Values[index],
                                ProcessDataDDI = DDIUtils.FormatDDI(layer.DDI),
                                DeviceElementIdRef = layer.DeviceElement != null ? IdList.BuildID("DET", layer.DeviceElement ?? 0) : null
                            });
                        }
                        tznList.Add(treatmentZone);
                        if (assign)
                        {
                            TreatmentZone.Add(treatmentZone);
                        }
                    }

                    return tznList;
                case ISOGridType.gridtype2:
                    var tzn = new ISOTreatmentZone()
                    {
                        TreatmentZoneCode = 0
                    };
                    foreach (var layer in zone.Layer)
                    {
                        tzn.ProcessDataVariable.Add(new ISOProcessDataVariable()
                        {
                            ProcessDataValue = 0,
                            ProcessDataDDI = DDIUtils.FormatDDI(layer.DDI),
                            DeviceElementIdRef = layer.DeviceElement != null ? IdList.BuildID("DET", layer.DeviceElement ?? 0) : null
                        });
                    }
                    if (assign)
                    {
                        TreatmentZone.Add(tzn);
                    }
                    return new List<ISOTreatmentZone>() { tzn };
                default:
                    return new List<ISOTreatmentZone>();
            }
        }

        /// <summary>
        /// TimeStamps tend to have a lot of Milliseconds. We intend to lower the maximum digits of Milliseconds to 3
        /// </summary>
        internal void CleanTimeStamps()
        {
            foreach (var tim in Time)
            {
                tim.Start = (DateTime)DateUtilities.TrimDateTimeToThreeDigitsOfMillisecondsMax(tim.Start);
                tim.Stop = DateUtilities.TrimDateTimeToThreeDigitsOfMillisecondsMax(tim.Stop);
            }
            foreach (var dan in DeviceAllocation)
            {
                if (dan.AllocationStamp != null)
                {
                    dan.AllocationStamp.Start = (DateTime)DateUtilities.TrimDateTimeToThreeDigitsOfMillisecondsMax(dan.AllocationStamp.Start);
                    dan.AllocationStamp.Stop = DateUtilities.TrimDateTimeToThreeDigitsOfMillisecondsMax(dan.AllocationStamp.Stop);
                }
            }

            foreach (var can in CommentAllocation)
            {
                if (can.AllocationStamp != null)
                {
                    can.AllocationStamp.Start = (DateTime)DateUtilities.TrimDateTimeToThreeDigitsOfMillisecondsMax(can.AllocationStamp.Start);
                    can.AllocationStamp.Stop = DateUtilities.TrimDateTimeToThreeDigitsOfMillisecondsMax(can.AllocationStamp.Stop);
                }
            }

            foreach (var gan in GuidanceAllocation)
            {
                if (gan.AllocationStamp != null)
                {
                    foreach (var asp in gan.AllocationStamp)
                    {
                        asp.Start = (DateTime)DateUtilities.TrimDateTimeToThreeDigitsOfMillisecondsMax(asp.Start);
                        asp.Stop = DateUtilities.TrimDateTimeToThreeDigitsOfMillisecondsMax(asp.Stop);
                    }
                }
            }

            foreach (var pan in ProductAllocation)
            {
                if (pan.AllocationStamp != null)
                {
                    pan.AllocationStamp.Start = (DateTime)DateUtilities.TrimDateTimeToThreeDigitsOfMillisecondsMax(pan.AllocationStamp.Start);
                    pan.AllocationStamp.Stop = DateUtilities.TrimDateTimeToThreeDigitsOfMillisecondsMax(pan.AllocationStamp.Stop);
                }
            }
        }

        internal DateTime GetStartTime()
        {
            if (TimeLogs.Count == 0)
            {
                return DateTime.MaxValue;
            }

            return TimeLogs
                .Where(tlg => tlg.Entries.Count > 0)
                .Min(tlg => tlg.GetStartTime());
        }

        internal bool IsInActiveWorkTime(DateTime time)
        {
            foreach (var tlg in TimeLogs)
            {
                if (tlg.Entries.Count > 0 && time >= tlg.GetStartTime() && time <= tlg.GetEndTime())
                {
                    return true;
                }
            }

            return false;
        }

        internal bool HasTimeLog(string key)
        {
            return TimeLogs.Any(entry => entry.Name == key);
        }
    }
}
