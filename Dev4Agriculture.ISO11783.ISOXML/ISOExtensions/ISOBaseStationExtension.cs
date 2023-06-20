using System;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOBaseStation
    {
 
        internal void FixPositionDigits()
        {
            BaseStationEast = decimal.Round(BaseStationEast, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
            BaseStationNorth = decimal.Round(BaseStationNorth, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
        }
    }
}
