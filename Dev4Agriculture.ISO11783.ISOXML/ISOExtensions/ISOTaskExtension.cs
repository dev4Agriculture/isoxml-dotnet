using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOTask
    {

        [XmlIgnore]
        public List<ISOTLG> TimeLogs = new List<ISOTLG>();

        [XmlIgnore]
        private DDIAvailabilityStatus _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;

        internal void initTimeLogList(Dictionary<string, ISOTLG> timeLogs)
        {
            foreach (var tlg in TimeLog)
            {
                if (timeLogs.TryGetValue(tlg.Filename, out var isoTLG))
                {
                    TimeLogs.Add(isoTLG);
                }
            }
        }

        public List<ISOTLGExtract> GetTaskExtract(ushort ddi, ushort det)
        {
            var extracts = new List<ISOTLGExtract>();
            foreach (var tlg in TimeLogs)
            {
                extracts.Add(ISOTLGExtract.FromTimeLog(tlg, ddi, det));
            }
            return extracts;
        }


        public int CountTimeLogs()
        {
            return TimeLogs.Count;
        }

        public void AddDefaultDataLogTrigger()
        {
            DataLogTrigger.Add(new ISODataLogTrigger()
            {
                DataLogDDI = BitConverter.GetBytes((ushort)0xDFFF),
                DataLogMethod = (byte)(TriggerMethods.OnTime
                | TriggerMethods.OnDistance
                | TriggerMethods.ThresholdLimits
                | TriggerMethods.OnChange
                | TriggerMethods.Total)

            });
        }




        public bool TryGetMaximum(int ddi, int deviceElement, out uint maximum)
        {
            maximum = 0;
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

        public bool TryGetMinimum(int ddi, int deviceElement, out uint minimum)
        {
            minimum = uint.MaxValue;
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


        public bool TryGetLastValue(int ddi, int deviceElement, out uint lastValue)
        {
            for (int index = TimeLogs.Count - 1; index >= 0; index--)
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
        public bool TryGetTotalValue(int ddi, int deviceElement, out uint totalValue, TLGTotalAlgorithmType totalAlgorithm)
        {
            bool found = false;
            if (totalAlgorithm == TLGTotalAlgorithmType.LIFETIME)
            {
                for (int index = TimeLogs.Count - 1; index >= 0; index--)
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
