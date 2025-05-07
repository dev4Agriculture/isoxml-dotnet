using System;
using System.Collections.Generic;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions
{
    public class SumTotalDDIFunctions : IDDITotalsFunctions
    {
        public ushort DDI;
        public long StartValue;
        public bool IsInitialized;

        public SumTotalDDIFunctions()
        {
        }

        public long EnqueueDataLogValue(long currentValue, ISOTime currentTimeEntry, int det, List<ISODevice> devices)
        {
            if (!IsInitialized)
            {
                IsInitialized = true;
                StartValue = currentValue;
            }
            else
            {
                StartValue += currentValue;
            }
            return StartValue;
        }

        public long SingulateDataLogValue(long currentValue, long previousValue, ISOTime currentTime, ISOTime previousTime, List<ISODevice> devices)
        {
            return currentValue - previousValue;
        }

        public long SingulateTimeLogValue(long currentValue, DateTime currentTime, List<LatestTLGEntry> latestTLGEntries)
        {
            if (!IsInitialized)
            {
                IsInitialized = true;
                StartValue = currentValue;
                return currentValue;
            }
            return currentValue - StartValue;
        }

        public void StartSingulateTimeLogValue(List<TLGDataLogDDI> ddis,List<ISODevice> devices)
        {
            IsInitialized = false;
        }
    }
}
