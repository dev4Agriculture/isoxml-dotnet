using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.DTO;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

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
        /// <param name="totalAlgorithm">The Algorithm to use for this Total</param>
        /// <param name="shallCheckTimeElements">If true (Default!), the TIM-Elements is checked if no data was found in the TimeLogs</param>
        /// <returns>True if Value was found</returns>
        public bool TryGetTotalValue(ushort ddi, int deviceElement, out int totalValue, TLGTotalAlgorithmType totalAlgorithm, bool shallCheckTimeElements = true)
        {
            var found = false;
            if (totalAlgorithm == TLGTotalAlgorithmType.LIFETIME)
            {
                for (var index = TimeLogs.Count - 1; index >= 0; index--)
                {
                    if (TimeLogs[index].TryGetTotalValue(ddi, deviceElement, out totalValue, totalAlgorithm))
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
            }

            totalValue = 0;
            foreach (var tlg in TimeLogs)
            {
                if (tlg.TryGetTotalValue(ddi, deviceElement, out var additional, totalAlgorithm))
                {
                    totalValue += additional;
                    found = true;
                }
            }
            if (!found)
            {
                if (shallCheckTimeElements)
                {
                    var endTime = Time.Max(entry => entry.Start);
                    if (Time.FirstOrDefault(entry => entry.Start == endTime)?.TryGetDDIValue(ddi, deviceElement, out totalValue) ?? false)
                    {
                        return true;
                    }
                }

            }

            return found;

        }


        /// <summary>
        /// Create a list of TimeElements with DataLogValue-Elements for the given Task. Only used when the TimeLogs were created in code; normally the TIM-Element exists
        /// </summary>
        /// <param name="devices">The list of devices; used to differentiate between Totals and LifeTimetotals; based on the DeviceDescriptions</param>
        /// <returns>List of TIM-Elements with DataLogValues</returns>
        public List<ISOTime> GenerateTimeElementsFromTimeLogs(IEnumerable<ISODevice> devices)
        {
            var list = new List<ISOTime>();

            ISOTime lastTim = null;

            foreach (var tlg in TimeLogs)
            {
                var tim = tlg.GenerateTimeElement(devices);
                if (lastTim != null)
                {
                    tim = ISOTime.CreateSummarizedTimeElement(lastTim, tim, devices);
                }
                list.Add(tim);
                lastTim = tim;
            }
            return list;

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

    }
}
