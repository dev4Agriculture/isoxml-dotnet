using System;
using System.Collections.Generic;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
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



        /// <summary>
        /// Get the value for the defined combination of DDI + DeviceElement from a TIM-Element
        /// </summary>
        /// <param name="ddi">The DDI Number; see https://isobus.net</param>
        /// <param name="deviceElement">The DeviceElementNumber</param>
        /// <param name="lastValue">The OUT variable that receives the value</param>
        /// <returns>True if a value was found</returns>
        public bool TryGetDDIValue(ushort ddi, int deviceElement, out int lastValue)
        {
            if (TryGetDataLogValue(ddi, deviceElement, out var dataLogValue))
            {
                lastValue = (int)dataLogValue.ProcessDataValue;
                return true;
            }
            lastValue = 0;
            return false;
        }

        /// <summary>
        /// Find a DataLogValue in a Time Element
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="deviceElement"></param>
        /// <param name="dataLogValue"></param>
        /// <returns></returns>
        public bool TryGetDataLogValue(ushort ddi, int deviceElement, out ISODataLogValue dataLogValue)
        {
            dataLogValue = DataLogValue.FirstOrDefault(entry =>
                                                            DDIUtils.ConvertDDI(entry.ProcessDataDDI) == ddi &&
                                                            IdList.ToIntId(entry.DeviceElementIdRef) == deviceElement
                                                        );
            return dataLogValue != null;
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
