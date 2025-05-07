using System;
using System.Collections.Generic;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions
{
    public class LifetimeTotalDDIFunctions : IDDITotalsFunctions
    {
        public long EnqueueDataLogValue(long currentValue, ISOTime currentTimeEntry, int det, List<ISODevice> devices)
        {
            return currentValue;
        }

        public long SingulateDataLogValue(long currentValue, long previousValue, ISOTime currentTime, ISOTime previousTime, List<ISODevice> devices)
        {
            return currentValue;
        }

        public long SingulateTimeLogValue(long currentValue, DateTime currentTime, List<LatestTLGEntry> latestTLGEntries)
        {
            return currentValue;
        }

        public void StartSingulateTimeLogValue(List<TLGDataLogDDI> ddis, List<ISODevice> devices)
        {
            //Nothing to do here;
        }
    }
}
