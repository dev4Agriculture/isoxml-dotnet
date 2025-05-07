using System;
using System.Collections.Generic;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions
{
    public class WeightedAverageDDIFunctions : IDDITotalsFunctions
    {
        public List<ushort> WeightDDIs;
        public ushort AverageDDI;
        public int DeviceElementId;
        public ushort RelevantWeightDDI;
        public long StartValue;
        public long StartWeightValue;
        public long LastWeightValue;
        public bool IsInitialized;
        public bool IsWeightInitialized;

        public WeightedAverageDDIFunctions(ushort ddi, int deviceElementId, List<ushort> weightDDIs)
        {
            AverageDDI = ddi;
            WeightDDIs = weightDDIs;
            DeviceElementId = deviceElementId;
        }


        private bool FindWeightDDI(List<ISODevice> devices)
        {
            var device = devices.FirstOrDefault(dvc => dvc.DeviceElement.Any(det => IdList.ToIntId(det.DeviceElementId) == DeviceElementId));
            foreach (var wDDI in WeightDDIs)
            {
                var relevantDPD = device.DeviceProcessData.FirstOrDefault(dpd => DDIUtils.ConvertDDI(dpd.DeviceProcessDataDDI) == wDDI);
                if (relevantDPD != null)
                {
                    RelevantWeightDDI = wDDI;
                    return true;
                }
            }
            return false;
        }

        public long EnqueueDataLogValue(long currentValue, ISOTime currentTimeEntry, int det, List<ISODevice> devices)
        {
            if (!IsInitialized)
            {
                StartValue = currentValue;
            }

            if (!IsWeightInitialized)
            {
                IsWeightInitialized = true;
                if (!FindWeightDDI(devices))
                {
                    //What would be the default here?
                }
            }

            if (currentTimeEntry.TryGetDDIValue(RelevantWeightDDI, det, out var weightValue))
            {
                LastWeightValue = weightValue;
                IsWeightInitialized = true;
            }

            if (IsWeightInitialized && IsInitialized)
            {
                currentValue = (StartValue * StartWeightValue + currentValue * LastWeightValue) / (StartWeightValue + LastWeightValue);
                StartValue = currentValue;
                StartWeightValue += LastWeightValue;
            }

            return currentValue;
        }

        public void InitSingulation(List<ISODevice> devices)
        {
            if (!FindWeightDDI(devices))
            {
                //Nothing to do here.maybe a log
            }
        }

        public long SingulateDataLogValue(long currentValue, long previousValue, ISOTime currentTime, ISOTime previousTime, List<ISODevice> devices)
        {
            if (previousTime.TryGetDDIValue(RelevantWeightDDI, DeviceElementId, out var startWeightValue) &&
                currentTime.TryGetDDIValue(RelevantWeightDDI, DeviceElementId, out var lastWeightValue)
                )
            {
                return (long)MathUtils.CalculateCleanedContinousWeightedAverage(StartValue, StartWeightValue, currentValue, LastWeightValue);
            }
            return currentValue;
        }

        public long SingulateTimeLogValue(long currentValue, DateTime currentTime, List<LatestTLGEntry> latestTLGEntries)
        {
            if (!IsInitialized)
            {
                StartValue = currentValue;
                IsInitialized = true;
            }
            var entry = latestTLGEntries.FirstOrDefault(dlv => dlv.DDI == RelevantWeightDDI && dlv.DeviceElement == DeviceElementId);
            if (entry != null && entry.Value != null)
            {
                if (!IsWeightInitialized)
                {
                    IsWeightInitialized = true;
                    StartWeightValue = entry.Value ?? 0;
                }
                LastWeightValue = entry.Value ?? 0;
            }

            if (IsInitialized && IsWeightInitialized)
            {
                return (long)MathUtils.CalculateCleanedContinousWeightedAverage(StartValue, StartWeightValue, currentValue, LastWeightValue);
            }
            else
            {
                return currentValue;
            }
        }

        public void StartSingulateTimeLogValue(List<TLGDataLogDDI> ddis, List<ISODevice> devices)
        {
            foreach (var wDDI in WeightDDIs)
            {
                if (ddis.Any(entry => entry.Ddi == wDDI && entry.DeviceElement == DeviceElementId))
                {
                    RelevantWeightDDI = wDDI;
                }
            }
        }
    }
}
