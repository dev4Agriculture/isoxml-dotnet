using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOTime
    {

        public ulong GetSeconds()
        {
            if (this.Stop != null)
            {
                TimeSpan? duration = Stop - Start;
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
            if (this.Stop != null)
            {
                TimeSpan? duration = Stop - Start;
                seconds =  (ulong)duration?.TotalSeconds;
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

        internal static ISOTime CreateSummarizedTimeElement(ISOTime lastTim, ISOTime tim, IEnumerable<ISODevice> devices)
        {
            foreach (var dlv in tim.DataLogValue)
            {
                var compare = lastTim.DataLogValue.First(entry =>
                                                            Utils.ConvertDDI(entry.ProcessDataDDI) == Utils.ConvertDDI(dlv.ProcessDataDDI) &&
                                                            entry.DeviceElementIdRef == dlv.DeviceElementIdRef
                                                        );
                if (compare != null)
                {
                    var device = devices.First(dvc => dvc.DeviceElement.Any(det => det.DeviceElementId == dlv.DeviceElementIdRef));
                    if (device.IsLifetimeTotal(Utils.ConvertDDI(dlv.ProcessDataDDI)))
                    {
                        dlv.ProcessDataValue = compare.ProcessDataValue;
                    }
                    else
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
            var dlv = DataLogValue.ToList().First(entry => Utils.ConvertDDI(entry.ProcessDataDDI) == ddi && IdList.ToIntId(entry.DeviceElementIdRef) == deviceElement)?.ProcessDataValue;
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
