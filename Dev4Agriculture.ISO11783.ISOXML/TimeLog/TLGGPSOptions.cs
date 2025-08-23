namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{
    public class TLGGPSOptions
    {
        public bool PosNorth;
        public bool PosEast;
        public bool PosUp;
        public bool PosStatus;
        public bool Pdop;
        public bool Hdop;
        public bool NumberOfSatellites;
        public bool GpsUTCTime;
        public bool GpsUTCDate;

        public TLGGPSOptions() { }


        public TLGGPSOptions(TLGGPSOptions gpsOptions)
        {
            Hdop = gpsOptions.Hdop;
            GpsUTCDate = gpsOptions.GpsUTCDate;
            GpsUTCTime = gpsOptions.GpsUTCTime;
            NumberOfSatellites = gpsOptions.NumberOfSatellites;
            Pdop = gpsOptions.Pdop;
            PosEast = gpsOptions.PosEast;
            PosNorth = gpsOptions.PosNorth;
            PosStatus = gpsOptions.PosStatus;
            PosUp = gpsOptions.PosUp;
        }
    }
}
