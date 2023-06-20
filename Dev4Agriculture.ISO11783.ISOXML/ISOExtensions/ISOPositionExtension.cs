using System;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOPosition
    {

        public void FixDigits()
        {
            PositionEast = decimal.Round(PositionEast, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
            PositionNorth = decimal.Round(PositionNorth, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
        }
    }
}
