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

        public void FixDigits()
        {
            PositionEast = decimal.Round(PositionEast, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
            PositionNorth = decimal.Round(PositionNorth, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
        }
    }
}
