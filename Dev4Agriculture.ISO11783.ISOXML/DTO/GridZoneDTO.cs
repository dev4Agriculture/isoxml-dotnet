using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.DTO
{
    public class GridZoneLayerDTO
    {
        public ushort DDI { get; set; }
        public int? DeviceElement { get; set; }
        public Dictionary<byte, int> GridType1Values { get; private set; } = new Dictionary<byte, int>();
    }

    public class GridZoneDTO
    {
        public GridZoneLayerDTO[] Layer { get; set; } = new GridZoneLayerDTO[0];
        public ISOGridType Type;

        public GridZoneDTO(ISOGridType type, int layers)
        {
            Layer = Enumerable.Range(0, layers)
                     .Select(_ => new GridZoneLayerDTO())
                     .ToArray();
            Type = type;
        }

    }
}
