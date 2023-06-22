using System;
using System.Xml.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOGrid
    {
        [XmlIgnore]
        public ISOGridFile GridFile { private get; set; }

        /// <summary>
        /// Get a Setpoint value for a specific position from the grid
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public int GetSetpointValue(decimal longitude, decimal latitude, uint layer = 0)
        {
            var maxNorth = GridMinimumNorthPosition + (decimal)(GridMaximumRow * GridCellNorthSize);
            var maxEast = GridMinimumEastPosition + (decimal)(GridMaximumColumn * GridCellEastSize);
            if (latitude < GridMinimumNorthPosition || latitude > maxNorth ||
                longitude < GridMinimumEastPosition || longitude > maxEast)
            {
                return Constants.TLG_VALUE_FOR_NO_VALUE;
            }
            else
            {
                try
                {
                    var row = Convert.ToUInt32((double)(latitude - GridMinimumNorthPosition) / GridCellNorthSize);
                    var column = Convert.ToUInt32((double)(longitude - GridMinimumEastPosition) / GridCellEastSize);
                    return GridFile.GetValue(column, row, layer);
                }
                catch
                {
                    return Constants.TLG_VALUE_FOR_NO_VALUE;
                }
            }
        }

        internal void FixPositionDigits()
        {
            GridMinimumEastPosition = decimal.Round(GridMinimumEastPosition, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
            GridMinimumNorthPosition = decimal.Round(GridMinimumNorthPosition, Constants.NUMBER_OF_DIGITS_FOR_POSITIONS, MidpointRounding.ToEven);
        }
    }
}
