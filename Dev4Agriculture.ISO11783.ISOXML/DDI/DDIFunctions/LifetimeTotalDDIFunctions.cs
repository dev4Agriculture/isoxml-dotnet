using System;
using System.Collections.Generic;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using static Dev4Agriculture.ISO11783.ISOXML.DDI.DDIAlgorithms;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions
{
    public class LifetimeTotalDDIFunctions : IDDITotalsFunctions
    {
        public long EnqueueValueAsDataLogValueInTime(long currentValue, ISOTime currentTimeEntry, int det, List<ISODevice> devices)
        {
            return currentValue;
        }

        public long SingulateValueInISOTime(long currentValue, long previousValue, ISOTime currentTime, ISOTime previousTime, List<ISODevice> devices)
        {
            return currentValue;
        }

        public long SingulateValueInTimeLog(long currentValue, DateTime currentTime, List<LatestTLGEntry> latestTLGEntries)
        {
            return currentValue;
        }

        public void StartSingulateValueInTimeLog(List<TLGDataLogDDI> ddis, List<ISODevice> devices)
        {
            //Nothing to do here;
        }

        public TotalDDIAlgorithmEnum GetTotalType()
        {
            return TotalDDIAlgorithmEnum.Lifetime;
        }

        public void UpdateTimeLogEnqueuerWithHeaderLine(List<TLGDataLogDDI> ddis)
        {
            //Nothing to do here
        }

        public void UpdateTimeLogEnqueuerWithDataLine(TLGDataLogLine line)
        {
            //Nothing to do here
        }

        public int EnqueueUpdatedValueInTimeLog(int value)
        {
            return value; //Nothing To Do here
        }

        public void UpdateTimeLogEnqueuerWithHeaderLine(List<TLGDataLogDDI> ddis, List<ISODevice> devices)
        {
            //Nothing to do here
        }
    }
}
