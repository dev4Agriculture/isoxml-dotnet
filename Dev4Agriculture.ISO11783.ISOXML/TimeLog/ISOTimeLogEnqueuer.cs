using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{
    public class EnqueuerEntry
    {
        public ushort DDI;
        public int DeviceElementId;
        public int Index;
        public IDDITotalsFunctions Function;
    }

    public class ISOTimeLogEnqueuer
    {

        public List<ISOTLG> EnqeueTimeLogs(List<ISOTLG> tlgs, List<ISODevice> devices)
        {
            var dataLogs = new Dictionary<string, EnqueuerEntry>();
            for (var index = 0; index < tlgs.Count; index++)
            {
                (tlgs[index], dataLogs) = EnqueueTimeLog(tlgs[index], devices, dataLogs);
            }

            return tlgs;

        }


        public (ISOTLG, Dictionary<string, EnqueuerEntry>) EnqueueTimeLog(ISOTLG tlg, List<ISODevice> devices, Dictionary<string, EnqueuerEntry> previousData)
        {
            var index = 0;
            foreach(var entry in tlg.Header.Ddis)
            {
                var device = devices.FirstOrDefault(dvc => dvc.DeviceElement.Any(det => IdList.ToIntId(det.DeviceElementId) == entry.DeviceElement));
                if (device != null && device.IsTotal(entry.Ddi) && !device.IsLifetimeTotal(entry.Ddi))
                {
                    var key = $"{entry.Ddi}_{entry.DeviceElement}";
                    if (!previousData.TryGetValue(key, out var enqueuerEntry))
                    {
                        previousData[key] = new EnqueuerEntry
                        {
                            Index = index,
                            DDI = entry.Ddi,
                            DeviceElementId = entry.DeviceElement,
                            Function = DDIAlgorithms.FindTotalDDIHandler(entry.Ddi, entry.DeviceElement, device)
                        };
                    }
                    enqueuerEntry.Function.UpdateTimeLogEnqueuerWithHeaderLine(tlg.Header.Ddis, devices);
                }
                index++;
            }

            foreach (var line in tlg.Entries)
            {
                foreach (var pair in previousData)
                {
                    pair.Value.Function.UpdateTimeLogEnqueuerWithDataLine(line);
                    if (line.Entries.Length >= pair.Value.Index && line.Entries[pair.Value.Index].IsSet)
                    {
                        line.Entries[pair.Value.Index].Value = pair.Value.Function.EnqueueUpdatedValueInTimeLog(line.Entries[pair.Value.Index].Value);
                    }
                }
            }
            return (tlg, previousData);
        }

    }
}
