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
            times = times.OrderBy(entry => entry.Start).ToList();
            Dictionary<string, IDDITotalsFunctions> DataLogValues = new Dictionary<string, IDDITotalsFunctions>();
            ISOTime previousTim = null;
            foreach( var currentTim in times)
            {
                if(currentTim.Type != ISOType2.Effective)
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
                    if (!DataLogValues.TryGetValue(key, out var dlvHandler))
                    {
                        if (DDIRegister.TryGetManufacturerSpecificDDI(ddi, device, out var ddiRegistry))
                        {
                            dlvHandler = ddiRegistry.GetInstance(deviceElement);
                        }
                        else if (device.IsLifetimeTotal(ddi))
                        {
                            dlvHandler = new LifetimeTotalDDIFunctions();
                        }
                        else if (DDIAlgorithms.AveragesDDIWeightedDdiMap.TryGetValue(ddi, out var dvi))
                        {
                            dlvHandler = new WeightedAverageDDIFunctions(ddi, deviceElement, dvi.ToList());
                        }
                        else
                        {
                            dlvHandler = new SumTotalDDIFunctions();
                        }
                        DataLogValues.Add(key, dlvHandler);
                    }
                    if (dlvHandler != null)
                    {
                        dlv.ProcessDataValue = dlvHandler.EnqueueDataLogValue(dlv.ProcessDataValue, currentTim, deviceElement, devices);
                    }
                }

            }
            return times;
        }
    }
}
