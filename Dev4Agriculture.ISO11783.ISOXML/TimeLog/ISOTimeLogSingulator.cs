using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using static Dev4Agriculture.ISO11783.ISOXML.DDI.DDIAlgorithms;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{
    public class ISOTimeLogSingulator
    {
        public ISOTLG SingulateTimeLog(ISOTLG isoTLG, List<ISODevice> devices)
        {
            var previousEntries = isoTLG.Header.Ddis
                .Select(entry => new LatestTLGEntry
                {
                    DDI = entry.Ddi,
                    DeviceElement = entry.DeviceElement
                })
                .ToList();

            var totalsIndex = new Dictionary<byte, IDDITotalsFunctions>();
            byte index = 0;
            foreach (var entry in isoTLG.Header.Ddis)
            {
                var device = devices.FirstOrDefault(dvc =>
                    dvc.DeviceElement.Any(det =>
                        IdList.ToIntId(det.DeviceElementId) == entry.DeviceElement));

                if (device != null &&
                    device.IsTotal(entry.Ddi) &&
                    !device.IsLifetimeTotal(entry.Ddi))
                {
                    totalsIndex.Add(index, FindTotalDDIHandler(entry.Ddi, entry.DeviceElement, device));
                }

                index++;
            }

            // IMPORTANT: We sort Descending here, to ensure, that the Average-DDIs are handled before any totals DDIs.
            // That's the only way we can make sure that we read the raw values before they are adjusted.
            // If we switched that, we would loose our chance to read the initial weight.
            totalsIndex = totalsIndex
                .OrderByDescending(kvp => kvp.Value.GetTotalType())
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var pair in totalsIndex)
            {
                pair.Value.StartSingulateValueInTimeLog(isoTLG.Header.Ddis, devices);
            }

            foreach (var line in isoTLG.Entries)
            {
                for (var ddiIndex = 0; ddiIndex < line.Entries.Length; ddiIndex++)
                {
                    if (line.Entries[ddiIndex].IsSet && previousEntries.Count > ddiIndex)
                    {
                        previousEntries[ddiIndex].TimeStamp = line.DateTime;
                        previousEntries[ddiIndex].Value = line.Entries[ddiIndex].Value;
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
