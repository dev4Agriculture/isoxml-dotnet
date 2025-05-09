using System;
using System.Collections.Generic;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIRegistry
{
    public class DDIRegisterSumTotalEntry : DDIRegisterEntry
    {
        public override IDDITotalsFunctions GetInstance(ushort ddi, int deviceElementId, ISODevice device)
        {
            return new SumTotalDDIFunctions(ddi, deviceElementId, device);
        }
    }
}
