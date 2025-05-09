﻿using System;
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
        public long StartValue;
        public long LastValue;
        public long BaseValue;

        public bool IsWeightInitialized;
        public long StartWeightValue;
        public long LastWeightValue;
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

        public long EnqueueValueAsDataLogValueInTime(long currentValue, ISOTime currentTimeEntry, int det, List<ISODevice> devices)
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
                LastWeightValue = lastWeightValue;
                return (long)MathUtils.CalculateCleanedContinousWeightedAverage(StartValue, StartWeightValue, currentValue, LastWeightValue);
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
                BaseWeightValue = LastWeightValue;//The LastWeightValue is the leftOver from the previous TimeLog if any existed
                BaseValue = LastValue;
                IsInitialized = false;
                IsWeightInitialized = false;
            }


        }

        public void UpdateTimeLogEnqueuerWithDataLine(TLGDataLogLine line)
        {
            if (line.Entries.Length >= RelevantWeightIndex && line.Entries[RelevantWeightIndex].IsSet)
            {
                LastWeightValue = line.Entries[RelevantWeightIndex].Value;
                if (!IsWeightInitialized)
                {
                    StartWeightValue = LastWeightValue;
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
            if (StartWeightValue != LastWeightValue)
            {
                var weightWithInTLG = LastWeightValue - StartWeightValue;
                var averageWithinTLG = (LastWeightValue * value - StartWeightValue * StartValue) / (LastWeightValue - StartWeightValue);
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
