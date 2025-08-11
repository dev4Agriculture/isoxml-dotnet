using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{
    public class ISOTimeListEnqueuer
    {
        public static long MergeLifetimeValues(long previousValue, long currentValue)
        {
            return currentValue;
        }

        public static long MergeContinouseTotalValues(long previousValue, long currentValue)
        {
            return previousValue + currentValue;
        }

        public static long MergeContinousAverageValues(long previousValue, long currentValue, ISOTime previousTime, ISOTime currentTime, List<ushort> weightDDIList, int deviceElementId)
        {
            long weightDDIValue = 0;
            long previousWeightDDIValue = 0;
            foreach (var weightDDI in weightDDIList)
            {
                if (previousTime.TryGetDataLogValue(weightDDI, deviceElementId, out var lastDlvEntry))
                {
                    weightDDIValue = lastDlvEntry.ProcessDataValue;
                    if (previousTime.TryGetDataLogValue(weightDDI, deviceElementId, out var currentDlvEntry))
                    {
                        previousWeightDDIValue = currentDlvEntry.ProcessDataValue;
                    }
                    break;
                }
            }
            if (weightDDIValue != 0 && previousWeightDDIValue != 0)
            {
                return (long)MathUtils.CalculateCleanedContinousWeightedAverage(previousValue, previousWeightDDIValue, currentValue, weightDDIValue);
            }
            return currentValue;

        }


        public static List<ISOTime> EnqueueTimeElements(List<ISOTime> times, List<ISODevice> devices)
        {
            var completeDLVList = new List<ISODataLogValue>();
            times = times.OrderBy(entry => entry.Start).ToList();
            var dataLogValues = new Dictionary<string, IDDITotalsFunctions>();
            foreach (var currentTim in times)
            {
                if (currentTim.Type != ISOType2.Effective)
                {
                    continue;
                }
                foreach (var dlv in currentTim.DataLogValue)
                {
                    var ddi = DDIUtils.ConvertDDI(dlv.ProcessDataDDI);
                    var device = devices.FirstOrDefault(dvc => dvc.DeviceElement.Any(det => det.DeviceElementId == dlv.DeviceElementIdRef));
                    var deviceElement = IdList.ToIntId(dlv.DeviceElementIdRef);
                    var key = $"{ddi}_{deviceElement}";
                    if (device == null)
                    {
                        continue;
                    }
                    if (!dataLogValues.TryGetValue(key, out var dlvHandler))
                    {
                        dlvHandler = DDIAlgorithms.FindTotalDDIHandler(ddi, deviceElement, device);

                        dataLogValues.Add(key, dlvHandler);
                    }
                    if (dlvHandler != null)
                    {
                        dlv.ProcessDataValue = dlvHandler.EnqueueValueAsDataLogValueInTime(dlv.ProcessDataValue, currentTim, deviceElement, devices);
                    }
                }
                //Fill up all DLVs that were in the previous but not in the current TIM
                foreach (var dlv in completeDLVList)
                {
                    if (!currentTim.TryGetDataLogValue(DDIUtils.ConvertDDI(dlv.ProcessDataDDI), IdList.ToIntId(dlv.DeviceElementIdRef), out _))
                    {
                        currentTim.DataLogValue.Add(dlv);
                    }
                }
                completeDLVList = currentTim.DataLogValue.ToList();
            }
            return times;
        }
    }
}
