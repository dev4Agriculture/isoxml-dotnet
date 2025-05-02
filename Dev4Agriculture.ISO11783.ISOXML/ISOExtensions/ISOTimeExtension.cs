using System;
using System.Collections.Generic;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOTime
    {

        public ulong GetSeconds()
        {
            if (Stop != null)
            {
                var duration = Stop - Start;
                return (ulong)duration?.TotalSeconds;
            }
            else if (Duration != null)
            {
                return Duration ?? 0;
            }
            else
            {
                return 0;
            }
        }


        public bool TryGetSeconds(out ulong seconds)
        {
            if (Stop != null)
            {
                var duration = Stop - Start;
                seconds = (ulong)duration?.TotalSeconds;
                return true;
            }
            else if (Duration != null)
            {
                seconds = Duration ?? 0;
                return true;
            }
            else
            {
                seconds = 0;
                return false;
            }
        }


        public DateTime? GetStopTime()
        {
            if (Stop != null)
            {
                return Stop;
            }
            else
            {
                return Start.AddSeconds(Duration ?? 0);
            }
        }

        public bool TryGetStopTime(out DateTime stop)
        {
            stop = (DateTime)(GetStopTime() ?? null);
            return stop != null;
        }


        internal static ISOTime CreateSummarizedTimeElement(ISOTime lastTim, ISOTime tim, IEnumerable<ISODevice> devices)
        {
            foreach (var dlv in tim.DataLogValue)
            {
                var ddi = DDIUtils.ConvertDDI(dlv.ProcessDataDDI);
                var compare = lastTim.DataLogValue.FirstOrDefault(entry =>
                                                            DDIUtils.ConvertDDI(entry.ProcessDataDDI) == ddi &&
                                                            entry.DeviceElementIdRef == dlv.DeviceElementIdRef
                                                        );
                if (compare != null)
                {
                    var device = devices.FirstOrDefault(dvc => dvc.DeviceElement.Any(det => det.DeviceElementId == dlv.DeviceElementIdRef));
                    if (device == null)
                    {
                        continue;
                    }

                    if (DDIRegister.TryGetManufacturerSpecificGroupedTotalCallback(ddi, device, out var groupingFunction))
                    {

                    }
                    else if (device.IsLifetimeTotal(DDIUtils.ConvertDDI(dlv.ProcessDataDDI)))
                    {
                        dlv.ProcessDataValue = compare.ProcessDataValue;
                    }
                    else if (DDIAlgorithms.AveragesDDIWeightedDdiMap.TryGetValue(ddi, out var weightDDIList))
                    {
                        long weightDDIValue = 0;
                        long previousWeightDDIValue = 0;
                        foreach (var weightDDI in weightDDIList)
                        {
                            var dlvEntry = tim.DataLogValue.FirstOrDefault(entry => DDIUtils.ConvertDDI(entry.ProcessDataDDI) == weightDDI &&
                                entry.DeviceElementIdRef == dlv.DeviceElementIdRef);
                            if (dlvEntry != null)
                            {
                                weightDDIValue = dlvEntry.ProcessDataValue;

                                dlvEntry = lastTim.DataLogValue.FirstOrDefault(entry =>
                                                            DDIUtils.ConvertDDI(entry.ProcessDataDDI) == weightDDI &&
                                                            entry.DeviceElementIdRef == dlv.DeviceElementIdRef
                                                        );
                                if (dlvEntry != null)
                                {
                                    previousWeightDDIValue = 0;
                                }
                                break;
                            }
                        }
                        if (weightDDIValue != 0 && previousWeightDDIValue != 0)
                        {
                            dlv.ProcessDataValue = (long)MathUtils.CalculateCleanedWeightedAverage(compare.ProcessDataValue, previousWeightDDIValue, dlv.ProcessDataValue, weightDDIValue);
                        }
                    }
                    else//It's a classic Total; just sum up
                    {
                        dlv.ProcessDataValue += compare.ProcessDataValue;
                    }
                }
            }

            return tim;
        }

        /// <summary>
        /// Get the value for the defined combination of DDI + DeviceElement from a TIM-Element
        /// </summary>
        /// <param name="ddi">The DDI Number; see https://isobus.net</param>
        /// <param name="deviceElement">The DeviceElementNumber</param>
        /// <param name="lastValue">The OUT variable that receives the value</param>
        /// <returns>True if a value was found</returns>
        public bool TryGetDDIValue(ushort ddi, int deviceElement, out int lastValue)
        {
            var dlv = DataLogValue.ToList().FirstOrDefault(entry => DDIUtils.ConvertDDI(entry.ProcessDataDDI) == ddi && IdList.ToIntId(entry.DeviceElementIdRef) == deviceElement)?.ProcessDataValue;
            if (dlv != null)
            {
                lastValue = (int)dlv;
                return true;
            }
            lastValue = 0;
            return false;
        }

        internal void FixPositionDigits()
        {
            foreach (var entry in Position)
            {
                entry.FixDigits();
            }
        }
    }
}
