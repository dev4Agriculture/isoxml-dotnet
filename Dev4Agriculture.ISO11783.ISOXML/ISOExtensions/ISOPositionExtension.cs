using System;
using System.Xml.Serialization;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOPosition
    {
        [XmlIgnore]
        public decimal Latitude//This Value should not really be needed, but as we have the same in TimeLogs, we added it here for better usability of the library.
        {
            get => PositionNorth;
            set => PositionNorth = value;
        }

        [XmlIgnore]
        public decimal Longitude//This Value should not really be needed, but as we have the same in TimeLogs, we added it here for better usability of the library.
        {
            get => PositionEast;
            set => PositionEast = value;
        }

        [XmlIgnore]
        public DateTime DateTime
        {
            get => DateUtilities.GetDateTimeFromTimeLogInfos(GpsUtcDateValue, (uint)GpsUtcTimeValue);
            set
            {
                GpsUtcDate = DateUtilities.GetDaysSince1980(value);
                GpsUtcTime = DateUtilities.GetMilliSecondsInDay(value);
            }
        }

        internal static ISOPosition FromTimeLogLine(TLGDataLogLine latestPosition, TLGDataLogHeader header)
        {
            var gps = header.GpsOptions;
            return new ISOPosition()
            {
                PositionEast = (decimal)latestPosition.Longitude,
                PositionNorth = (decimal)latestPosition.Latitude,
                PositionUp = gps.PosUp ? (long?)latestPosition.PosUp : null,
                GpsUtcDate = gps.GpsUTCDate ? (ushort?)latestPosition.Date : null,
                GpsUtcTime = gps.GpsUTCTime ? (uint?)latestPosition.Time : null,
                PDOP = gps.Pdop ? (long?)latestPosition.Pdop : null,
                HDOP = gps.Hdop ? (long?)latestPosition.Hdop : null,
                NumberOfSatellites = gps.NumberOfSatellites ? (byte?)latestPosition.NumberOfSatellites : null
            };

        }

        public void FixDigits()
        {
            PositionEast = decimal.Round(PositionEast, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
            PositionNorth = decimal.Round(PositionNorth, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
        }
    }
}
