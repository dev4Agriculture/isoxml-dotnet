using System;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOPoint
    {
        public void FixDigits()
        {
            PointEast = decimal.Round(PointEast, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
            PointNorth = decimal.Round(PointNorth, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
        }
    }
}
