using System;
using System.Xml.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOGrid
    {
        [XmlIgnore]
        public ISOGridFile GridFile { private get; set; }

        public uint GetSetpointValue(decimal longitude, decimal latitude, uint layer = 0)
        {
            var maxNorth = GridMinimumNorthPosition + (decimal)(GridMaximumRow * GridCellNorthSize);
            var maxEast = GridMinimumEastPosition + (decimal)(GridMaximumColumn * GridCellEastSize);
            if (latitude < GridMinimumNorthPosition || latitude > maxNorth ||
                longitude < GridMinimumEastPosition || longitude > maxEast)
            {
                return 0xFFFFFFF;
            }
            else
            {
                try
                {
                    var column = Convert.ToUInt32((double)(latitude - GridMinimumNorthPosition) / GridCellNorthSize);
                    var row = Convert.ToUInt32((double)(longitude - GridMinimumEastPosition) / GridCellEastSize);
                    return GridFile.GetValue(column, row, layer);
                }
                catch
                {
                    return 0xFFFFFFF;
                }
            }
        }

    }
}
