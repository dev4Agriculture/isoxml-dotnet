using System.Collections.Generic;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{
    internal enum TLGDDITotalAlgorithmEnum
    {
        Sum = 0,
        Average = 1,
        Special = 2
    }

    internal class TLGDDIUpdate
    {
        public ushort DDI = 0;
        public int DeviceElementId = 0;
        public TLGDDITotalAlgorithmEnum Type = TLGDDITotalAlgorithmEnum.Sum;
        public int Index = 0;
        public bool Initialized = false;
        public byte WeightIndex = 0;
        public int StartValue = 0;
        public bool WeightInitialized = false;
        public long StartWeight = 0;
        public long Weight = 0;
        public IDDITotalsFunctions Function;
    }



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
            var totalsIndex = new Dictionary<byte, TLGDDIUpdate>();
            byte index = 0;
            foreach (var entry in isoTLG.Header.Ddis)
            {
                var device = devices.FirstOrDefault(dvc => dvc.DeviceElement.Any(det => IdList.ToIntId(det.DeviceElementId) == entry.DeviceElement));
                if (device != null && device.IsTotal(entry.Ddi) && !device.IsLifetimeTotal(entry.Ddi))
                {
                    var total = new TLGDDIUpdate()
                    {
                        DDI = entry.Ddi,
                        DeviceElementId = entry.DeviceElement,
                        Type = TLGDDITotalAlgorithmEnum.Sum
                    };
                    if (DDIRegister.TryGetManufacturerSpecificDDI(entry.Ddi, device, out var ddiEntry))
                    {
                        total.Type = TLGDDITotalAlgorithmEnum.Special;
                        total.Function = ddiEntry.GetInstance(entry.DeviceElement);
                        total.Function.StartSingulateTimeLogValue(isoTLG.Header.Ddis, devices);
                    }
                    else if (DDIAlgorithms.AveragesDDIWeightedDdiMap.TryGetValue(total.DDI, out var ddiList))
                    {
                        foreach (var ddi in ddiList)
                        {
                            var weightDDI = isoTLG.Header.Ddis.FirstOrDefault(weightEntry => weightEntry.Ddi == ddi);
                            if (weightDDI != null)
                            {
                                total.WeightIndex = weightDDI.Index;
                                total.Type = TLGDDITotalAlgorithmEnum.Average;
                            }
                        }
                    }
                    totalsIndex.Add(index, total);
                }
                index++;
            }


            //IMPORTANT: We sort Descending here, to ensure, that the Average-DDIs are handled before any totals DDIs.
            //That's the only way we can make sure that we read the raw values before they are adjusted.
            //If we switched that, we would loose our chance to read the initial weight.
            totalsIndex = totalsIndex.OrderByDescending(kvp => kvp.Value.Type).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

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
                    switch (pair.Value.Type)
                    {
                        case TLGDDITotalAlgorithmEnum.Sum:
                            if (line.Entries.Length >= pair.Key && line.Entries[pair.Key].IsSet)
                            {
                                if (!pair.Value.Initialized)
                                {
                                    pair.Value.Initialized = true;
                                    pair.Value.StartValue = line.Entries[pair.Key].Value;
                                }
                                line.Entries[pair.Key].Value -= pair.Value.StartValue;
                            }
                            break;
                        case TLGDDITotalAlgorithmEnum.Average:
                            if (line.Entries.Length >= pair.Value.WeightIndex && line.Entries[pair.Value.WeightIndex].IsSet)
                            {
                                if (!pair.Value.WeightInitialized)
                                {
                                    pair.Value.StartWeight = line.Entries[pair.Value.WeightIndex].Value;
                                    pair.Value.WeightInitialized = true;
                                }
                                pair.Value.Weight = line.Entries[pair.Value.WeightIndex].Value;
                            }
                            if (line.Entries.Length >= pair.Key && line.Entries[pair.Key].IsSet)
                            {
                                if (!pair.Value.Initialized)
                                {
                                    pair.Value.StartValue = line.Entries[pair.Key].Value;
                                    pair.Value.Initialized = true;
                                }

                                if (pair.Value.WeightInitialized)
                                {
                                    line.Entries[pair.Key].Value = (int)MathUtils.CalculateCleanedContinousWeightedAverage(
                                        pair.Value.StartValue,
                                        pair.Value.StartWeight,
                                        line.Entries[pair.Key].Value,
                                        pair.Value.StartWeight + pair.Value.Weight
                                        );
                                }

                            }
                            break;
                        case TLGDDITotalAlgorithmEnum.Special:
                            line.Entries[pair.Key].Value = (int)pair.Value.Function.SingulateTimeLogValue(
                                line.Entries[pair.Key].Value,
                                line.DateTime,
                                previousEntries);
                            break;
                    }
                }
            }
            return isoTLG;
        }
    }
}
