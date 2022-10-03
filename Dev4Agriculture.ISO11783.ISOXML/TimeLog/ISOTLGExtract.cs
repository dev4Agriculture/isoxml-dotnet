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
        public static readonly uint TLG_VALUE_FOR_NO_VALUE = 0xFFFFFFFF;

        public DateTime TimeStamp { get; private set; }
        public TLGGPSInfo GPS { get; private set; }
        public uint DDIValue { get; private set; }
        public bool HasValue { get; private set; }

        private ISOTLGExtractPoint(DateTime timeStamp, TLGGPSInfo gps, uint ddiValue, bool hasValue)
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
                        has == true ? value : TLG_VALUE_FOR_NO_VALUE,
                        has
                );
        }
    }

    public class ISOTLGExtract
    {
        public readonly int Ddi;
        public readonly int Det;
        public readonly string Name;
        public List<ISOTLGExtractPoint> Data {  get; private set; }

        public ISOTLGExtract(int ddi, int det, string name, List<ISOTLGExtractPoint> data)
        {
            Ddi = ddi;
            Det = det;
            Name = name;
            Data = data;
        }

        public static ISOTLGExtract FromTimeLog(ISOTLG timeLog, ushort ddi, ushort det = 0, string name = "")
        {

            var entries = new List<ISOTLGExtractPoint>();
            if (timeLog.Header.TryGetDDIIndex(ddi, det, out uint index))
            {
                foreach (var entry in timeLog.Entries)
                {
                    if (entry.Has(index))
                    {
                        entries.Add(ISOTLGExtractPoint.FromTLGDataLogLine(entry, index));
                    }
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
