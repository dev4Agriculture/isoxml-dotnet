using System;
using System.Collections.Generic;
using System.Linq;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{

    /// <summary>
    /// ISOTLG is the class to read and write TimeLog Files (TLG....bin/.xml)
    /// This partial class hosts the 
    /// </summary>
    public partial class ISOTLG
    {
        public const double TLG_GPS_FACTOR = 10000000.0;
        private DDIAvailabilityStatus _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;


        public bool TryGetMaximum(ushort ddi, int deviceElement, out int maximum)
        {
            if (!Header.TryGetDDIIndex(ddi, deviceElement, out var index))
            {
                _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;
                maximum = 0;
                return false;
            }

            _ddiAvailabilityStatus = DDIAvailabilityStatus.NO_VALUE;
            maximum = 0;
            foreach (var entry in Entries)
            {
                if (entry.TryGetValue(index, out var entryValue))
                {
                    maximum = maximum > entryValue ? maximum : entryValue;
                    _ddiAvailabilityStatus = DDIAvailabilityStatus.HAS_VALUE;
                }
            }
            return true;
        }

        public bool TryGetMinimum(ushort ddi, int deviceElement, out int minimum)
        {
            if (!Header.TryGetDDIIndex(ddi, deviceElement, out var index))
            {
                _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;
                minimum = 0;
                return false;
            }

            _ddiAvailabilityStatus = DDIAvailabilityStatus.NO_VALUE;
            minimum = int.MaxValue;
            foreach (var entry in Entries)
            {
                if (entry.TryGetValue(index, out var entryValue))
                {
                    minimum = minimum < entryValue ? minimum : entryValue;
                    _ddiAvailabilityStatus = DDIAvailabilityStatus.HAS_VALUE;
                }
            }
            return true;
        }




        public bool TryGetFirstValue(ushort ddi, int deviceElement, out int firstValue)
        {
            if (!Header.TryGetDDIIndex(ddi, deviceElement, out var index))
            {
                _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;
                firstValue = 0;
                return false;
            }

            _ddiAvailabilityStatus = DDIAvailabilityStatus.NO_VALUE;
            foreach (var entry in Entries)
            {
                if (entry.TryGetValue(index, out var entryValue))
                {
                    _ddiAvailabilityStatus = DDIAvailabilityStatus.HAS_VALUE;
                    firstValue = entryValue;
                    return true;
                }
            }
            firstValue = 0;
            return false;
        }


        public bool TryGetLastValue(ushort ddi, int deviceElement, out int lastValue)
        {
            if (!Header.TryGetDDIIndex(ddi, deviceElement, out var index))
            {
                _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;
                lastValue = 0;
                return false;
            }

            _ddiAvailabilityStatus = DDIAvailabilityStatus.NO_VALUE;
            var v = Entries.Count - 1;
            for (var entryIndex = v; entryIndex > 0; entryIndex--)
            {
                if (Entries[entryIndex].TryGetValue(index, out var entryValue))
                {
                    _ddiAvailabilityStatus = DDIAvailabilityStatus.HAS_VALUE;
                    lastValue = entryValue;
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
        public bool TryGetTotalValue(ushort ddi, int deviceElement, out int totalValue, TLGTotalAlgorithmType totalAlgorithm)
        {
            if (!Header.TryGetDDIIndex(ddi, deviceElement, out var index))
            {
                _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;
                totalValue = 0;
                return false;
            }

            _ddiAvailabilityStatus = DDIAvailabilityStatus.NO_VALUE;

            if (totalAlgorithm == TLGTotalAlgorithmType.LIFETIME)
            {
                return TryGetLastValue(ddi, deviceElement, out totalValue);
            }
            else if (totalAlgorithm == TLGTotalAlgorithmType.NO_RESETS)
            {
                if (TryGetFirstValue(ddi, deviceElement, out var first))
                {
                    if (TryGetLastValue(ddi, deviceElement, out var last))
                    {
                        _ddiAvailabilityStatus = DDIAvailabilityStatus.HAS_VALUE;
                        totalValue = last - first;
                        return true;
                    }
                }
                totalValue = 0;
                return false;
            }
            else
            {
                var lastValue = 0;
                var isStarted = false;
                totalValue = 0;
                foreach (var entry in Entries)
                {
                    if (entry.TryGetValue(index, out var curValue))
                    {
                        _ddiAvailabilityStatus = DDIAvailabilityStatus.HAS_VALUE;
                        if (!isStarted)
                        {
                            isStarted = true;
                            lastValue = curValue;
                        }
                        if (curValue >= lastValue)
                        {
                            totalValue += curValue - lastValue;
                            lastValue = curValue;
                        }
                        else
                        {
                            lastValue = curValue;
                        }
                    }
                }
                return true;

            }
        }


        /// <summary>
        /// Returns the Availability-Status for a value based on the result from the latest call for TryGetMaximum,-Minimum, etc.
        /// This function does NOT actually check the availability status
        /// </summary>
        /// <returns></returns>
        public DDIAvailabilityStatus GetLatestDDIAvailabilityStatus()
        {
            return _ddiAvailabilityStatus;
        }

        /// <summary>
        /// Checks if values for a specific combination of DDI and DeviceElement are available
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="deviceElement"></param>
        /// <returns></returns>
        public DDIAvailabilityStatus GetDDIAvailabilityStatus(ushort ddi, int deviceElement)
        {
            if (!Header.TryGetDDIIndex(ddi, deviceElement, out var index))
            {
                return DDIAvailabilityStatus.NOT_IN_HEADER;
            }

            foreach (var entry in Entries)
            {
                if (entry.TryGetValue(index, out var entryValue))
                {
                    return DDIAvailabilityStatus.HAS_VALUE;
                }
            }
            return DDIAvailabilityStatus.NO_VALUE;
        }


        public List<ISODataLogValue> GetAllTotals(TLGTotalAlgorithmType totalAlgorithmType)
        {
            var list = new List<ISODataLogValue>();
            foreach (var entry in Header.Ddis)
            {
                var dlv = new ISODataLogValue()
                {
                    ProcessDataDDI = Utils.FormatDDI(entry.Ddi),
                    DeviceElementIdRef = IdList.BuildID("DET", entry.DeviceElement),
                };
                list.Add(dlv);
            }

            foreach (var entry in Header.Pgns)
            {
                var dlv = new ISODataLogValue()
                {
                    ProcessDataDDI = Utils.FormatDDI(DDIList.PGNBasedData),
                    DeviceElementIdRef = "",
                    DataLogPGN = entry.DataLogPGN,
                    DataLogPGNStartBit = entry.StartBit,
                    DataLogPGNStopBit = entry.StopBit
                };
                list.Add(dlv);
            }

            foreach (var entry in list)
            {
                if (TryGetTotalValue(
                        Utils.ConvertDDI(entry.ProcessDataDDI),
                        IdList.ToIntId(entry.DeviceElementIdRef),
                        out var total, totalAlgorithmType)
                    )
                {
                    entry.ProcessDataValue = total;
                }
            }

            return list;
        }


        public ISOTime GetTimeElement()
        {
            var min = DateTime.MaxValue;
            var max = DateTime.MinValue;
            foreach (var entry in Entries)
            {
                var date = DateUtilities.GetDateTimeFromTimeLogInfos(entry.Date, entry.Time);
                if (min.CompareTo(date) > 0)
                {
                    min = date;
                }

                if (max.CompareTo(date) < 0)
                {
                    max = date;
                }
            }

            var dataLogValues = this.GetAllTotals(TLGTotalAlgorithmType.NO_RESETS);

            var isoTime = new ISOTime()
            {
                Start = min,
                Stop = max,
                Type = ISOType2.Effective,
            };
            isoTime.DataLogValue.ToList().AddRange(dataLogValues);

            return isoTime;
        }
    }

}
