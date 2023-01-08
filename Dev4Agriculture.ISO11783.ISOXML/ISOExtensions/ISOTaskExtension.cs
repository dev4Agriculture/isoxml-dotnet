using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
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
        public List<ISOTLGExtract> GetTaskExtract(ushort ddi, short det, string name = "", bool fillLines = false)
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
        public ISOTLGExtract GetMergedTaskExtract(ushort ddi, short det, string name = "", bool fillLines = false)
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
        /// Adds the Element <DLT A="DFFF" B="31"/>
        /// This tells the TaskController to track all data received from the DefaultSet of any device during task operation
        /// </summary>
        public void AddDefaultDataLogTrigger()
        {
            DataLogTrigger.Add(new ISODataLogTrigger()
            {
                DataLogDDI = Utils.FormatDDI(0xDFFF),
                DataLogMethod = (byte)(TriggerMethods.OnTime
                | TriggerMethods.OnDistance
                | TriggerMethods.ThresholdLimits
                | TriggerMethods.OnChange
                | TriggerMethods.Total)
            });
        }


        public void AddDefaultGridValues(uint ddi, uint value, string det = "")
        {
            var pdv = new ISOProcessDataVariable()
            {
                ProcessDataDDI = Utils.FormatDDI(ddi),
                DeviceElementIdRef = det ?? null,
                ProcessDataValue = value

            };

            var tzn = new ISOTreatmentZone()
            {
                TreatmentZoneCode = 0,
                TreatmentZoneDesignator = "Default"
            };

            tzn.ProcessDataVariable.Add(pdv);

            DefaultTreatmentZoneCodeValue = 0;
            PositionLostTreatmentZoneCode = 0;
            OutOfFieldTreatmentZoneCode = 0;

        }

        public ISOGridFile CreateGrid(ISOGrid grid, ISOGridLayer[] gridLayers)
        {
            Grid.Add(grid);
            var gridFile = ISOGridFile.Create(grid, (byte)gridLayers.Length);
            var tzn = new ISOTreatmentZone()
            {
                TreatmentZoneCode = grid.TreatmentZoneCodeValue,
                TreatmentZoneDesignator = "Data"
            };
            foreach(var layer in gridLayers)
            {
                var pdv = new ISOProcessDataVariable()
                {
                    DeviceElementIdRef = layer.Det,
                    ProcessDataDDI = Utils.FormatDDI(layer.Ddi)
                };
                tzn.ProcessDataVariable.Add(pdv);
            }
            TreatmentZone.Add(tzn);
            return gridFile;
        }

        //=====================================================    Analysis    ==================================================================================//


        /// <summary>
        /// Try to get the maximum value for a specific combination of DDI and  deviceElement from a Task
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="deviceElement"></param>
        /// <param name="maximum"> The variable that shall receive the result</param>
        /// <returns> True if a maximum value could be identified</returns>
        public bool TryGetMaximum(int ddi, int deviceElement, out int maximum)
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
        /// Try to get the minimum value for a specific combination of DDI and  deviceElement from a Task
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="deviceElement"></param>
        /// <param name="minimum"> The variable that shall receive the result</param>
        /// <returns> True if a minimum value could be identified</returns>
        public bool TryGetMinimum(int ddi, int deviceElement, out int minimum)
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
        /// Try to get the first available value for a specific combination of DDI and  deviceElement from a Task
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="deviceElement"></param>
        /// <param name="first"> The variable that shall receive the result</param>
        /// <returns> True if a first value could be identified, False if this value doesn't exist in the Task</returns>
        public bool TryGetFirstValue(int ddi, int deviceElement, out int firstValue)
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
        /// Try to get the last available value for a specific combination of DDI and  deviceElement from a Task
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="deviceElement"></param>
        /// <param name="last"> The variable that shall receive the result</param>
        /// <returns> True if a last value could be identified, False if this value doesn't exist in the Task</returns>
        public bool TryGetLastValue(int ddi, int deviceElement, out int lastValue)
        {
            for (var index = TimeLogs.Count - 1; index >= 0; index--)
            {
                if (TimeLogs[index].TryGetLastValue(ddi, deviceElement, out lastValue))
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
        /// <returns></returns>
        public bool TryGetTotalValue(int ddi, int deviceElement, out int totalValue, TLGTotalAlgorithmType totalAlgorithm)
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


            return found;

        }


    }
}
