using System.Collections.Generic;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{
    public class ISOTimeListSingulator
    {
        public List<ISOTime> SingulateTimeElements(List<ISOTime> times, List<ISODevice> devices)
        {
            if (times.Count(entry => entry.Type == ISOType2.Effective) <= 1)
            {
                return times;
            }

            times = times.OrderBy(entry => entry.Start).ToList();
            var DataLogValues = new Dictionary<string, IDDITotalsFunctions>();

            ISOTime currentTim = null;
            for (var index = times.Count - 1; index >= 0; index--)
            {
                if (times[index].Type != ISOType2.Effective)
                {
                    continue;
                }
                var previousTim = times[index];
                if (currentTim != null)
                {
                    var previousDataLogValues = previousTim.DataLogValue;
                    foreach (var dlv in currentTim.DataLogValue)
                    {
                        var ddi = DDIUtils.ConvertDDI(dlv.ProcessDataDDI);
                        var device = devices.FirstOrDefault(dvc => dvc.DeviceElement.Any(det => det.DeviceElementId == dlv.DeviceElementIdRef));
                        var deviceElement = IdList.ToIntId(dlv.DeviceElementIdRef);
                        if (device == null)
                        {
                            continue;
                        }
                        var listKey = $"{ddi}_{deviceElement}";
                        if (!DataLogValues.TryGetValue(listKey, out var dlvHandler))
                        {
                            dlvHandler = DDIAlgorithms.FindTotalDDIHandler(ddi, deviceElement, device);
                        }
                        var previousValue = previousDataLogValues.FirstOrDefault(prevDLV => DDIUtils.ConvertDDI(prevDLV.ProcessDataDDI) == ddi && prevDLV.DeviceElementIdRef == dlv.DeviceElementIdRef);
                        if (previousValue != null)
                        {
                          dlv.ProcessDataValue = dlvHandler.SingulateValueInISOTime(dlv.ProcessDataValue, previousValue.ProcessDataValue, currentTim, previousTim, devices);
                        }
                    }
                }
                currentTim = previousTim;
            }
            return times;
        }
    }
}
