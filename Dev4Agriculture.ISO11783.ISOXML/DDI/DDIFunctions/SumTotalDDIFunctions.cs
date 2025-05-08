using System;
using System.Collections.Generic;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using static Dev4Agriculture.ISO11783.ISOXML.DDI.DDIAlgorithms;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions
{
    public class SumTotalDDIFunctions : IDDITotalsFunctions
    {
        public ushort DDI;
        public long StartValue;
        public bool IsInitialized;
        public long LatestValue = 0;
        public long TLGBaseValue = 0;

        public SumTotalDDIFunctions()
        {
        }

        public long EnqueueValueAsDataLogValueInTime(long currentValue, ISOTime currentTimeEntry, int det, List<ISODevice> devices)
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

        public long SingulateValueInISOTime(long currentValue, long previousValue, ISOTime currentTime, ISOTime previousTime, List<ISODevice> devices)
        {
            return currentValue - previousValue;
        }

        public long SingulateValueInTimeLog(long currentValue, DateTime currentTime, List<LatestTLGEntry> latestTLGEntries)
        {
            if (!IsInitialized)
            {
                IsInitialized = true;
                StartValue = currentValue;
                return currentValue;
            }
            return currentValue - StartValue;
        }

        public void StartSingulateValueInTimeLog(List<TLGDataLogDDI> ddis,List<ISODevice> devices)
        {
            IsInitialized = false;
        }

        public TotalDDIAlgorithmEnum GetTotalType()
        {
            return TotalDDIAlgorithmEnum.Sum;
        }

        public void UpdateTimeLogEnqueuerWithHeaderLine(List<TLGDataLogDDI> ddis, List<ISODevice> devices)
        {
            StartValue = LatestValue;
        }

        public void UpdateTimeLogEnqueuerWithDataLine(TLGDataLogLine line)
        {
            //Nothing to do here
        }

        public int EnqueueUpdatedValueInTimeLog(int value)
        {
            if (!IsInitialized)
            {
                TLGBaseValue = value;
            }
            LatestValue = value + StartValue - TLGBaseValue;
            return (int)LatestValue;
        }

    }
}
