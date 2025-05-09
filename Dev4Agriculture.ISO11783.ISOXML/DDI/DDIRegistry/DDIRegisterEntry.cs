using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIRegistry
{
    public abstract class DDIRegisterEntry
    {
        public ushort DDI;
        public ushort Manufacturer;
        public abstract IDDITotalsFunctions GetInstance(ushort ddi, int deviceElementId, ISODevice device);
    }
}
