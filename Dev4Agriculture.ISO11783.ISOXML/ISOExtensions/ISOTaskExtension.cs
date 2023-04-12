using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;

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
            var lastValue = ISOTLGExtractPoint.TLG_VALUE_FOR_NO_VALUE;
            foreach (var tlg in TimeLogs)
            {
                var entry = ISOTLGExtract.FromTimeLog(tlg, ddi, det, name, fillLines, lastValue);
                if (fillLines && entry.Data.Count > 0)
                {
                    lastValue = entry.Data.Last().DDIValue;
                }
                extracts.Add(entry);
            }
            var merge = new List<ISOTLGExtractPoint>();
            extracts.ForEach(entry => merge.AddRange(entry.Data));
            var result = new ISOTLGExtract(ddi, det, name, merge);
            return result;
        }


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
            if (!DataLogTrigger.Any(entry => Utils.ConvertDDI(entry.DataLogDDI) == (ushort)DDIList.RequestDefaultProcessData))
            {
                DataLogTrigger.Add(new ISODataLogTrigger()
                {
                    DataLogDDI = Utils.FormatDDI(DDIList.RequestDefaultProcessData),
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
                if (tlg.TryGetMaximum(ddi, deviceElement, out var compare))
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
                if (Time.First(entry => entry.Start == endTime).TryGetDDIValue(ddi, deviceElement, out lastValue))
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
                    if (Time.First(entry => entry.Start == endTime).TryGetDDIValue(ddi, deviceElement, out totalValue))
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
            if (found == false)
            {
                if (shallCheckTimeElements)
                {
                    var endTime = Time.Max(entry => entry.Start);
                    if (Time.First(entry => entry.Start == endTime).TryGetDDIValue(ddi, deviceElement, out totalValue))
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

            foreach(var tlg in TimeLogs)
            {
                var tim = tlg.GenerateTimeElement(devices);
                if (lastTim != null)
                {
                    tim = ISOTime.CreateSummarizedTimeElement(lastTim, tim,devices);
                }
                list.Add(tim);
                lastTim = tim;
            }
            return list;

        }

    }
}
