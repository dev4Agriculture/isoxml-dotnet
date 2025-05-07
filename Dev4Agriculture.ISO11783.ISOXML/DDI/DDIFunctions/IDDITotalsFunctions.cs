using System;
using System.Collections.Generic;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions
{
    public class LatestTLGEntry
    {
        public ushort DDI = 0;
        public int DeviceElement = 0;
        public DateTime TimeStamp;
        public long? Value;
    }


    public interface IDDITotalsFunctions
    {
        void StartSingulateTimeLogValue(List<TLGDataLogDDI> ddis, List<ISODevice> devices);
        long SingulateTimeLogValue(long currentValue, DateTime currentTime, List<LatestTLGEntry> latestTLGEntries);
        long SingulateDataLogValue(long currentValue, long previousValue, ISOTime currentTime, ISOTime previousTime, List<ISODevice> devices);
        long EnqueueDataLogValue(long currentValue, ISOTime currentTimeEntry, int det, List<ISODevice> devices);
    }
}
