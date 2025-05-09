using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using static Dev4Agriculture.ISO11783.ISOXML.DDI.DDIAlgorithms;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions
{
    public class LifetimeTotalDDIFunctions : IDDITotalsFunctions
    {
        private ushort _ddi;
        private int _deviceElement;

        public LifetimeTotalDDIFunctions(ushort ddi, int deviceElement, ISODevice device)
        {
            _ddi = ddi;
            _deviceElement = deviceElement;
        }

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


        public bool GetCleanedTotalForTimeLog(ISOTLG iSOTLG, out int totalValue)
        {
            return iSOTLG.TryGetLastValue(_ddi, _deviceElement, out totalValue);
        }

        public bool GetCleanedTotalForTask(ISOTask task, out int totalValue)
        {
            var tlg = task.TimeLogs.OrderByDescending(entry => entry.Name).ToList().LastOrDefault();
            if (GetCleanedTotalForTimeLog(tlg, out totalValue))
            {
                return true;
            }
            return false;
        }
    }
}
