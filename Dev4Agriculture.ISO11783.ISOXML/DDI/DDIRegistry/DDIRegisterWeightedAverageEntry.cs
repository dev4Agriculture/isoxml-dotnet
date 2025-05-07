using System.Collections.Generic;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIRegistry
{
    public class DDIRegisterWeightedAverageEntry : DDIRegisterEntry
    {
        public List<ushort> WeightDDIs;

        public DDIRegisterWeightedAverageEntry(List<ushort> weightDDIs)
        {
            WeightDDIs = weightDDIs;
        }

        public override IDDITotalsFunctions GetInstance(int deviceElementId)
        {
            return new WeightedAverageDDIFunctions(DDI, deviceElementId, WeightDDIs);
        }
    }
}
