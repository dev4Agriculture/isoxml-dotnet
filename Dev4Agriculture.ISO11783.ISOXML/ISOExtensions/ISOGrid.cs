using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOGrid
    {
        [XmlIgnore]
        public ISOGridFile gridFile { private get; set; }

        public uint GetSetpointValue(decimal longitude, decimal latitude,uint layer = 0)
        {
            decimal maxNorth = this.GridMinimumNorthPosition + (decimal)(this.GridMaximumRow * this.GridCellNorthSize);
            decimal maxEast = this.GridMinimumEastPosition + (decimal)(this.GridMaximumColumn* this.GridCellEastSize);
            if( latitude < this.GridMinimumNorthPosition || latitude > maxNorth ||
                longitude < this.GridMinimumEastPosition || longitude > maxEast)
            {
                return (int)0xFFFFFFF;
            } else 
            {
                try
                {
                    uint column = Convert.ToUInt32((double)(latitude - this.GridMinimumNorthPosition) / this.GridCellNorthSize);
                    uint row = Convert.ToUInt32((double)(longitude - this.GridMinimumEastPosition) / this.GridCellEastSize);
                    return this.gridFile.GetValue(column, row, layer);
                } catch
                {
                    return (int)0xFFFFFFF;
                }
            }
        }

    }
}
