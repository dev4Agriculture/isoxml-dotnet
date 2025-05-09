using System;
using System.Collections.Generic;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using static Dev4Agriculture.ISO11783.ISOXML.DDI.DDIAlgorithms;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions
{
    public class SumTotalDDIFunctions : IDDITotalsFunctions
    {
        public ushort DDI;
        public int DeviceElement;
        public ISODevice Device;
        public long StartValue;
        public bool IsInitialized;
        public long LatestValue = 0;
        public long TLGBaseValue = 0;

        public SumTotalDDIFunctions(ushort ddi, int deviceElement, ISODevice device)
        {
            DDI = ddi;
            DeviceElement = deviceElement;
            Device = device;
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


        public bool GetCleanedTotalForTimeLog(ISOTLG iSOTLG, out int totalValue)
        {
            if(iSOTLG.TryGetLastValue(DDI, DeviceElement, out var last) &&
                iSOTLG.TryGetFirstValue(DDI, DeviceElement, out var first))
            {
                totalValue = last - first;
                return true;
            }
            totalValue = 0;
            return false;
        }


        public bool GetCleanedTotalForTask(ISOTask task, out int totalValue)
        {
            totalValue = 0;
            foreach( var tlg in task.TimeLogs)
            {
                if( GetCleanedTotalForTimeLog(tlg, out var nextValue))
                {
                    totalValue += nextValue;
                }
            }
            return true;
        }


    }
}
