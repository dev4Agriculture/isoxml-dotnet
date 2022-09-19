using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{
    public partial class ISOTLG
    {


        internal string GetKMLLineString()
        {
            var content = "";

            foreach (var entry in Entries)
            {
                if (((GPSQuality)entry.PosStatus != GPSQuality.ERROR) &&
                    ((GPSQuality)entry.PosStatus != GPSQuality.UNKNOWN)
                    )
                {
                    content += "\n" +
                        (entry.PosEast / ISOTLG.TLG_GPS_FACTOR).ToString().Replace(",", ".") + "," +
                        (entry.PosNorth / ISOTLG.TLG_GPS_FACTOR).ToString().Replace(",", ".") + "," +
                        entry.PosUp.ToString();
                }
            }
            return content;
        }
    }
}
