using System;
using System.Collections.Generic;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIRegistry
{
    public class DDIRegisterSumTotalEntry : DDIRegisterEntry
    {
        public override IDDITotalsFunctions GetInstance(int deviceElementId)
        {
            return new SumTotalDDIFunctions();
        }
    }
}
