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
            splitIndices.Sort();
            var splitIndicesIndex = 0;
            var latestEntries = new TLGDataLogEntry[this.Header.Ddis.Count];
            var gpsOptions = this.Header.GpsOptions;


            var splittedTLGs = new List<ISOTLG>();
            ISOTLG currentTLG = new ISOTLG(int.Parse(this.Name.Substring(3)), this.FolderPath);
            currentTLG.Header.GpsOptions = new TLGGPSOptions(gpsOptions);
            for (var index = 0; index < this.Entries.Count; index++)
            {
                var curTLGLine = this.Entries[index];
                var copiedTLGLine = new TLGDataLogLine(curTLGLine);
                if (splitIndicesIndex< splitIndices.Count && index == splitIndices[splitIndicesIndex])
                {
                    splittedTLGs.Add(currentTLG);
                    currentTLG = new ISOTLG(nextTLGNo, this.FolderPath);
                    currentTLG.Header.GpsOptions = new TLGGPSOptions(gpsOptions);
                    var maxDDIEntries = curTLGLine.Entries.Length < latestEntries.Length ? curTLGLine.Entries.Length : latestEntries.Length;
                    for (var ddiEntryIndex = 0; ddiEntryIndex < maxDDIEntries; ddiEntryIndex++)
                    {
                        if (!curTLGLine.Entries[ddiEntryIndex].IsSet && latestEntries[ddiEntryIndex].IsSet)
                        {
                            curTLGLine.Entries[ddiEntryIndex].Value = latestEntries[ddiEntryIndex].Value;
                        }
                    }
                    splitIndicesIndex++;
                }
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
