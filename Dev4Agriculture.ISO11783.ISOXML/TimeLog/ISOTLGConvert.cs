using System;

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
                text += PosStatus;
            }


            if (header.GpsOptions.Pdop)
            {
                text += Pdop;
            }

            if (header.GpsOptions.Hdop)
            {
                text += Hdop;
            }

            if (header.GpsOptions.NumberOfSatellites)
            {
                text += NumberOfSatellites;
            }


            if (header.GpsOptions.GpsUTCTime)
            {
                text += GpsUTCTime;
            }

            if (header.GpsOptions.GpsUTCDate)
            {
                text += GpsUTCDate;
            }


            foreach (var ddi in header.Ddis)
            {
                text += ";";
                if (Entries[ddi.Index].IsSet)
                {
                    text += Entries[ddi.Index].Value;
                }
            }
            return text;
        }


        public String ToKMLString()
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
    }
}
