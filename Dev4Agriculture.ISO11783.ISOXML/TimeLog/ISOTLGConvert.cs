using System;
using System.Collections.Generic;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{

    public partial class TLGDataLogLine
    {
        public string ToStringWithDDIsOnly(TLGDataLogHeader header)
        {
            var text = "" +
                DateUtilities.GetDateFromDaysSince1980(Date) + ";" +
                DateUtilities.GetTimeFromMilliSeconds(Time) + ";";
            if (header.GpsOptions.PosNorth)
            {
                text += PosNorth * Math.Pow(10, -7) + ";";
            }
            if (header.GpsOptions.PosEast)
            {
                text += PosEast * Math.Pow(10, -7) + ";";
            }

            if (header.GpsOptions.PosUp)
            {
                text += PosUp + ";";
            }


            if (header.GpsOptions.PosStatus)
            {
                text += PosStatus + ";";
            }


            if (header.GpsOptions.Pdop)
            {
                text += Pdop + ";";
            }

            if (header.GpsOptions.Hdop)
            {
                text += Hdop + ";";
            }

            if (header.GpsOptions.NumberOfSatellites)
            {
                text += NumberOfSatellites + ";";
            }


            if (header.GpsOptions.GpsUTCTime)
            {
                text += GpsUTCTime + ";";
            }

            if (header.GpsOptions.GpsUTCDate)
            {
                text += GpsUTCDate + ";";
            }


            foreach (var ddi in header.Ddis)
            {
                if (Entries[ddi.Index].IsSet)
                {
                    text += Entries[ddi.Index].Value;
                }
                text += ";";
            }
            return text;
        }


        public string ToKMLString()
        {
            return "\n" +
                        (PosEast / ISOTLG.TLG_GPS_FACTOR).ToString().Replace(",", ".") + "," +
                        (PosNorth / ISOTLG.TLG_GPS_FACTOR).ToString().Replace(",", ".") + "," +
                        PosUp.ToString();
        }
    }

    public partial class ISOTLG
    {


        public string GetKMLLineString()
        {
            var content = "";

            foreach (var entry in Entries)
            {
                if (((GPSQuality)entry.PosStatus != GPSQuality.ERROR) &&
                    ((GPSQuality)entry.PosStatus != GPSQuality.UNKNOWN)
                    )
                {
                    content += entry.ToKMLString();
                }
            }
            return content;
        }




        public List<ISOTLG> SplitTimeLog(List<ISODevice> devices, List<int> splitIndices, int nextTLGNo)
        {
            return SplitTimeLogCore(splitIndices, nextTLGNo, false);
        }

        // Equivalent to repeatedly splitting the remaining suffix. Preserve the
        // filenames and mutate only the first boundary in the original source;
        // subsequent sequential cuts would have mutated copies, not that source.
        internal List<ISOTLG> SplitTimeLogSequentially(List<int> splitIndices, int nextTLGNo)
        {
            return SplitTimeLogCore(splitIndices, nextTLGNo, true);
        }

        private List<ISOTLG> SplitTimeLogCore(List<int> splitIndices, int nextTLGNo, bool sequentialCompatibility)
        {
            if (splitIndices == null)
            {
                throw new ArgumentNullException(nameof(splitIndices));
            }

            if (splitIndices.Any(index => index < 0 || index > Entries.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(splitIndices), "Split indices must be between zero and the row count.");
            }

            // Endpoints do not create fragments. Normalize a copy so duplicate cuts
            // cannot leave the cursor stuck, and the caller's list is unchanged.
            splitIndices = splitIndices
                .Where(index => index > 0 && index < Entries.Count)
                .Distinct()
                .OrderBy(index => index)
                .ToList();

            var firstTLGNo = nextTLGNo;
            var splitIndicesIndex = 0;
            var latestEntries = new TLGDataLogEntry[Header.Ddis.Count];
            var gpsOptions = Header.GpsOptions;

            var splittedTLGs = new List<ISOTLG>();
            // Fix: Use nextTLGNo for the first TLG to ensure unique names
            ISOTLG currentTLG = new ISOTLG(nextTLGNo, FolderPath);
            nextTLGNo++;
            currentTLG.Header = new TLGDataLogHeader(Header);
            for (var index = 0; index < Entries.Count; index++)
            {
                var curTLGLine = Entries[index];
                if (splitIndicesIndex < splitIndices.Count && index == splitIndices[splitIndicesIndex])
                {
                    if (currentTLG.Entries.Count > 0)
                    {
                        splittedTLGs.Add(currentTLG);
                    }
                    if (sequentialCompatibility)
                    {
                        // Intermediate suffix names are consumed by the sequential
                        // implementation even though those suffixes are split again.
                        nextTLGNo = firstTLGNo + 2 * (splitIndicesIndex + 1)
                            - (splitIndicesIndex + 1 == splitIndices.Count ? 1 : 0);
                        if (splitIndicesIndex > 0)
                            curTLGLine = new TLGDataLogLine(curTLGLine);
                    }
                    currentTLG = new ISOTLG(nextTLGNo, FolderPath);
                    nextTLGNo++;
                    currentTLG.Header = new TLGDataLogHeader(Header);
                    var maxDDIEntries = curTLGLine.Entries.Length < latestEntries.Length ? curTLGLine.Entries.Length : latestEntries.Length;
                    for (var ddiEntryIndex = 0; ddiEntryIndex < maxDDIEntries; ddiEntryIndex++)
                    {
                        if (!curTLGLine.Entries[ddiEntryIndex].IsSet && latestEntries[ddiEntryIndex].IsSet)
                        {
                            curTLGLine.Entries[ddiEntryIndex].Value = latestEntries[ddiEntryIndex].Value;
                            curTLGLine.Entries[ddiEntryIndex].IsSet = true;
                            curTLGLine.NumberOfEntries++;
                        }
                    }
                    splitIndicesIndex++;
                }
                var copiedTLGLine = new TLGDataLogLine(curTLGLine);
                currentTLG.Entries.Add(copiedTLGLine);
                for (var ddiEntryIndex = 0; ddiEntryIndex < curTLGLine.Entries.Length; ddiEntryIndex++)
                {
                    if (curTLGLine.Entries[ddiEntryIndex].IsSet)
                    {
                        latestEntries[ddiEntryIndex].IsSet = true;
                        latestEntries[ddiEntryIndex].Value = curTLGLine.Entries[ddiEntryIndex].Value;
                    }
                }

            }
            splittedTLGs.Add(currentTLG);
            return splittedTLGs;
        }
    }
}
