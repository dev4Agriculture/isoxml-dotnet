using System.Collections.Generic;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIRegistry;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using static Dev4Agriculture.ISO11783.ISOXML.DDI.DDIAlgorithms;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{
    public class ISOTimeLogSingulator
    {
        public ISOTLG SingulateTimeLog(ISOTLG isoTLG, List<ISODevice> devices)
        {
            var previousEntries = new List<LatestTLGEntry>();
            foreach (var entry in isoTLG.Header.Ddis)
            {
                previousEntries.Add(new LatestTLGEntry()
                {
                    DDI = entry.Ddi,
                    DeviceElement = entry.DeviceElement
                });

            }
            var totalsIndex = new Dictionary<byte, IDDITotalsFunctions>();
            byte index = 0;
            foreach (var entry in isoTLG.Header.Ddis)
            {
                var device = devices.FirstOrDefault(dvc => dvc.DeviceElement.Any(det => IdList.ToIntId(det.DeviceElementId) == entry.DeviceElement));
                if (device != null && device.IsTotal(entry.Ddi) && !device.IsLifetimeTotal(entry.Ddi))
                {
                    totalsIndex.Add(index, DDIAlgorithms.FindTotalDDIHandler(entry.Ddi, entry.DeviceElement, device));
                }
                index++;
            }



            //IMPORTANT: We sort Descending here, to ensure, that the Average-DDIs are handled before any totals DDIs.
            //That's the only way we can make sure that we read the raw values before they are adjusted.
            //If we switched that, we would loose our chance to read the initial weight.
            totalsIndex = totalsIndex.OrderByDescending(kvp => kvp.Value.GetTotalType()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var pair in totalsIndex)
            {
                pair.Value.StartSingulateValueInTimeLog(isoTLG.Header.Ddis, devices);
            }

            foreach (var line in isoTLG.Entries)
            {
                for (var ddiIndex = 0; ddiIndex < line.Entries.Length; ddiIndex++)
                {
                    if (line.Entries[ddiIndex].IsSet && previousEntries.Count < ddiIndex)
                    {
                        previousEntries[index].TimeStamp = line.DateTime;
                        previousEntries[index].Value = line.Entries[ddiIndex].Value;
                    }
                }

                foreach (var pair in totalsIndex)
                {
                    if (line.Entries.Length >= pair.Key && line.Entries[pair.Key].IsSet)
                    {
                        line.Entries[pair.Key].Value = (int)pair.Value.SingulateValueInTimeLog(
                            line.Entries[pair.Key].Value,
                            line.DateTime,
                            previousEntries);
                    }
                }
            }
            return isoTLG;
        }
    }
}
