using System;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOPosition
    {
        public decimal Latitude
        {
            get => PositionNorth / (decimal)ISOTLG.TLG_GPS_FACTOR;
            set => PositionNorth = (int)(value * (decimal)ISOTLG.TLG_GPS_FACTOR);
        }

        public decimal Longitude
        {
            get => PositionEast / (decimal)ISOTLG.TLG_GPS_FACTOR;
            set => PositionEast = (int)(value * (decimal)ISOTLG.TLG_GPS_FACTOR);
        }

        public void FixDigits()
        {
            PositionEast = decimal.Round(PositionEast, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
            PositionNorth = decimal.Round(PositionNorth, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
        }
    }
}
