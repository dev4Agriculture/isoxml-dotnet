using System;
using System.Collections.Generic;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{

    public class TLGGPSInfo
    {
        public readonly double PosNorth;
        public readonly double PosEast;
        public readonly int PosUp;
        public readonly byte PosStatus;
        public readonly ushort Pdop;
        public readonly ushort Hdop;
        public readonly byte NumberOfSatellites;
        public readonly uint GpsUTCTime;
        public readonly ushort GpsUTCDate;

        public TLGGPSInfo(int posNorth, int posEast, int posUp, byte posStatus, ushort pdop, ushort hdop, byte numberOfSatellites, uint gpsUTCTime, ushort gpsUTCDate)
        {
            PosNorth = posNorth / ISOTLG.TLG_GPS_FACTOR;
            PosEast = posEast / ISOTLG.TLG_GPS_FACTOR;
            PosUp = posUp;
            PosStatus = posStatus;
            Pdop = pdop;
            Hdop = hdop;
            NumberOfSatellites = numberOfSatellites;
            GpsUTCTime = gpsUTCTime;
            GpsUTCDate = gpsUTCDate;
        }

        public static TLGGPSInfo FromTLGDataLogLine(TLGDataLogLine line)
        {
            return new TLGGPSInfo(
                line.PosNorth,
                line.PosEast,
                line.PosUp,
                line.PosStatus,
                line.Pdop,
                line.Hdop,
                line.NumberOfSatellites,
                line.GpsUTCTime,
                line.GpsUTCDate);
        }
    }

    public class ISOTLGExtractPoint
    {
        public DateTime TimeStamp { get; private set; }
        public TLGGPSInfo GPS { get; private set; }
        public int DDIValue { get; private set; }
        public bool HasValue { get; private set; }

        private ISOTLGExtractPoint(DateTime timeStamp, TLGGPSInfo gps, int ddiValue, bool hasValue)
        {
            TimeStamp = timeStamp;
            GPS = gps;
            DDIValue = ddiValue;
            HasValue = hasValue;
        }

        public static ISOTLGExtractPoint FromTLGDataLogLine(TLGDataLogLine line, uint index)
        {
            var has = line.TryGetValue(index, out var value);
            return new ISOTLGExtractPoint(
                        DateUtilities.GetDateTimeFromTimeLogInfos(line.Date, line.Time),
                        TLGGPSInfo.FromTLGDataLogLine(line),
                        has ? value : (int)Constants.TLG_VALUE_FOR_NO_VALUE,
                        has
                );
        }

        internal static ISOTLGExtractPoint FromTLGDataLogLineWithGivenValue(TLGDataLogLine line, int lastValue)
        {
            return new ISOTLGExtractPoint(
                        DateUtilities.GetDateTimeFromTimeLogInfos(line.Date, line.Time),
                        TLGGPSInfo.FromTLGDataLogLine(line),
                        lastValue,
                        true
                );
        }
    }

    public class ISOTLGExtract
    {
        public readonly ushort Ddi;
        public readonly int Det;
        public readonly string Name;
        public List<ISOTLGExtractPoint> Data { get; private set; }

        public ISOTLGExtract(ushort ddi, int det, string name, List<ISOTLGExtractPoint> data)
        {
            Ddi = ddi;
            Det = det;
            Name = name;
            Data = data;
        }

        /// <summary>
        /// Extracts a TimeLogExtract that includes Positions, Times and Values for one specific DDI.
        /// If filling is enabled, those entries without a value are added with the latest known value
        /// </summary>
        /// <param name="timeLog"> The TimeLog to extract from</param>
        /// <param name="ddi"> The DDI to search; see isobus.net</param>
        /// <param name="det"> The DeviceElementIndex. E.g. if the ID is "DET-1", det is -1. For "DET1", it is 1</param>
        /// <param name="name"> An optional name to describe the values</param>
        /// <param name="fillLines"> Default false: If set to true, those point+time-Entries that don't include the value are filled with the latest known value</param>
        /// <param name="lastValue"> Default set to "NO VALUE": The last known Value. This should *only* be used if the call for this function
        /// is part of loop for multiple TimeLogs. In case there are no values recorded for this TimeLog, this ensures that there are Entries created anyway</param>
        /// <returns></returns>
        public static ISOTLGExtract FromTimeLog(ISOTLG timeLog, ushort ddi, int det = 0, string name = "", bool fillLines = false, int lastValue = Constants.TLG_VALUE_FOR_NO_VALUE)
        {

            var entries = new List<ISOTLGExtractPoint>();
            if (timeLog.Header.TryGetDDIIndex(ddi, det, out var index))
            {
                //Find the first existing value for the rare case that no value was there at the beginning
                if (fillLines && lastValue == Constants.TLG_VALUE_FOR_NO_VALUE)
                {
                    foreach (var entry in timeLog.Entries)
                    {
                        if (entry.Has(index))
                        {
                            lastValue = entry.Get(index);
                            break;
                        }
                    }
                }
                foreach (var entry in timeLog.Entries)
                {
                    if (entry.Has(index))
                    {
                        var point = ISOTLGExtractPoint.FromTLGDataLogLine(entry, index);
                        lastValue = point.DDIValue;
                        entries.Add(point);
                    }
                    else if (fillLines && lastValue != Constants.TLG_VALUE_FOR_NO_VALUE)
                    {
                        var point = ISOTLGExtractPoint.FromTLGDataLogLineWithGivenValue(entry, lastValue);
                        entries.Add(point);
                    }
                }
            }
            else if (fillLines && lastValue != 0)
            {
                foreach (var entry in timeLog.Entries)
                {
                    entries.Add(ISOTLGExtractPoint.FromTLGDataLogLineWithGivenValue(entry, lastValue));
                }
            }

            return new ISOTLGExtract(
                ddi,
                det,
                name,
                entries
            );
        }
    }
}
