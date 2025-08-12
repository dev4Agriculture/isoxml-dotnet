using System;
using System.Collections.Generic;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using static Dev4Agriculture.ISO11783.ISOXML.DDI.DDIAlgorithms;

namespace Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions
{
    public class WeightedAverageDDIFunctions : IDDITotalsFunctions
    {
        public ushort AverageDDI;
        public int DeviceElementId;
        public ISODevice Device;
        public bool IsInitialized;
        public double StartValue;
        public double LastValue;
        public double BaseValue;

        public bool IsWeightInitialized;
        public long StartWeightValue;
        public long CurrentWeightValue;
        public long BaseWeightValue;

        public List<ushort> WeightDDIs;
        public ushort RelevantWeightDDI;
        public byte RelevantWeightIndex;

        private int TaskTotalLatestWeight;


        public WeightedAverageDDIFunctions(ushort ddi, int deviceElementId, ISODevice device, List<ushort> weightDDIs)
        {
            AverageDDI = ddi;
            WeightDDIs = weightDDIs;
            DeviceElementId = deviceElementId;
            Device = device;
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

        /// <summary>
        /// Updates the AverageValue based on the Weight and new Weight values. If no new Weight value is found, the currentValue persists and becomes the new value
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="currentTimeEntry"></param>
        /// <param name="det"></param>
        /// <param name="devices"></param>
        /// <returns></returns>
        public long EnqueueValueAsDataLogValueInTime(long currentValue, ISOTime currentTimeEntry, int det, List<ISODevice> devices)
        {
            if (currentValue == 0)
            {
                return (long)StartValue;
            }

            if (!IsInitialized)
            {
                StartValue = currentValue;
                IsInitialized = true;
            }

            if (!IsWeightInitialized)
            {
                if (!FindWeightDDI(devices))
                {
                    return currentValue;
                }
                IsWeightInitialized = true;
            }

            if (currentTimeEntry.TryGetDDIValue(RelevantWeightDDI, det, out var weightValue))
            {
                CurrentWeightValue = weightValue;
                IsWeightInitialized = true;
            }
            if(CurrentWeightValue == 0)
            {
                return (long)StartValue;
            }
            if (IsWeightInitialized && IsInitialized)
            {
                StartValue = (StartValue * StartWeightValue + currentValue * CurrentWeightValue) / (StartWeightValue + CurrentWeightValue);
                currentValue = (long) StartValue;
                StartWeightValue += CurrentWeightValue;
            }

            return currentValue;
        }

        public long SingulateValueInISOTime(long currentValue, long previousValue, ISOTime currentTime, ISOTime previousTime, List<ISODevice> devices)
        {
            if(RelevantWeightDDI == 0)
            {
                FindWeightDDI(devices);
            }

            if (previousTime.TryGetDDIValue(RelevantWeightDDI, DeviceElementId, out var startWeightValue) &&
                currentTime.TryGetDDIValue(RelevantWeightDDI, DeviceElementId, out var lastWeightValue)
                )
            {
                StartValue = previousValue;
                StartWeightValue = startWeightValue;
                CurrentWeightValue = lastWeightValue;
                return (long)MathUtils.CalculateCleanedContinousWeightedAverage(StartValue, StartWeightValue, currentValue, CurrentWeightValue);
            }
            return currentValue;
        }

        public long SingulateValueInTimeLog(long currentValue, DateTime currentTime, List<LatestTLGEntry> latestTLGEntries)
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
                CurrentWeightValue = entry.Value ?? 0;
            }

            if (IsInitialized && IsWeightInitialized)
            {
                return (long)MathUtils.CalculateCleanedContinousWeightedAverage(StartValue, StartWeightValue, currentValue, CurrentWeightValue);
            }
            else
            {
                return currentValue;
            }
        }

        public void StartSingulateValueInTimeLog(List<TLGDataLogDDI> ddis, List<ISODevice> devices)
        {
            foreach (var wDDI in WeightDDIs)
            {
                if (ddis.Any(entry => entry.Ddi == wDDI && entry.DeviceElement == DeviceElementId))
                {
                    RelevantWeightDDI = wDDI;
                }
            }
        }

        public TotalDDIAlgorithmEnum GetTotalType()
        {
            return TotalDDIAlgorithmEnum.Average;
        }

        public void UpdateTimeLogEnqueuerWithHeaderLine(List<TLGDataLogDDI> ddis, List<ISODevice> devices)
        {
            if (RelevantWeightDDI == 0)
            {
                if (!FindWeightDDI(devices))
                {
                    //TODO: What if we don't have a Weight DDI available?
                }
            }

            var ddiEntry = ddis.FirstOrDefault(entry => entry.Ddi == RelevantWeightDDI && entry.DeviceElement == DeviceElementId);
            if (ddiEntry != null)
            {
                RelevantWeightIndex = ddiEntry.Index;
                BaseWeightValue = CurrentWeightValue;//The LastWeightValue is the leftOver from the previous TimeLog if any existed
                BaseValue = LastValue;
                IsInitialized = false;
                IsWeightInitialized = false;
            }


        }

        public void UpdateTimeLogEnqueuerWithDataLine(TLGDataLogLine line)
        {
            if (line.Entries.Length >= RelevantWeightIndex && line.Entries[RelevantWeightIndex].IsSet)
            {
                CurrentWeightValue = line.Entries[RelevantWeightIndex].Value;
                if (!IsWeightInitialized)
                {
                    StartWeightValue = CurrentWeightValue;
                    IsWeightInitialized = true;
                }
            }
        }

        public int EnqueueUpdatedValueInTimeLog(int value)
        {
            if (!IsInitialized)
            {
                StartValue = value;
                IsInitialized = true;
            }
            if (StartWeightValue != CurrentWeightValue)
            {
                var weightWithInTLG = CurrentWeightValue - StartWeightValue;
                var averageWithinTLG = (CurrentWeightValue * value - StartWeightValue * StartValue) / (CurrentWeightValue - StartWeightValue);
                var cleanedAverage = (BaseValue * BaseWeightValue + averageWithinTLG * weightWithInTLG)/(BaseWeightValue + averageWithinTLG);
                LastValue = value;
                return (int)cleanedAverage;
            }
            else
            {
                LastValue = value;
                return value;
            } 
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="iSOTLG"></param>
        /// <param name="totalValue"></param>
        /// <returns></returns>
        public bool GetCleanedTotalForTimeLog(ISOTLG iSOTLG, out int totalValue)
        {

            if (
                iSOTLG.TryGetLastValue(AverageDDI, DeviceElementId, out var intEndValue) &&
                iSOTLG.TryGetFirstValue(AverageDDI, DeviceElementId, out var intStartValue) &&
                iSOTLG.TryGetFirstValue(RelevantWeightDDI, DeviceElementId, out var intStartWeightValue) &&
                iSOTLG.TryGetLastValue(RelevantWeightDDI, DeviceElementId, out var intEndWeightValue)
                )
            {
                totalValue = (int)MathUtils.CalculateCleanedContinousWeightedAverage(intStartValue, intStartWeightValue, intEndValue, intEndWeightValue);
                TaskTotalLatestWeight = intEndWeightValue - intStartWeightValue;
                return true;
            }
            totalValue = 0;
            return false;
        }

        /// <summary>
        ///   Get a Total for the full Task. here it's an Average Value
        /// </summary>
        /// <param name="task"></param>
        /// <param name="totalValue"></param>
        /// <returns></returns>
        public bool GetCleanedTotalForTask(ISOTask task, out int totalValue)
        {
            var avgSum = 0;
            var valueSum = 0;
            foreach (var tlg in task.TimeLogs)
            {
                if(GetCleanedTotalForTimeLog(tlg, out var curAverage))
                {
                    avgSum += curAverage * TaskTotalLatestWeight;
                    valueSum += curAverage * TaskTotalLatestWeight;
                }
            }

            if (valueSum > 0)
            {
                totalValue = avgSum / valueSum;
                return true;
            }
            totalValue = 0;
            return false;
        }
    }
}
