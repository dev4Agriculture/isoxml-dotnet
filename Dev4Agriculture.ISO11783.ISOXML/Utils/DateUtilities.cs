using System;

namespace Dev4Agriculture.ISO11783.ISOXML.Utils
{

    public class DateUtilities
    {
        public static DateTime ISOXMLEpoch = new DateTime(1980, 1, 1, 0, 0, 0);

        private static readonly double MILLISECONDS_IN_DAY = 24.0 * 60 * 60 * 1000;
        public static readonly string DATE_FORMAT = "dd.MM.yyyy";
        public static string GetDateFromDaysSince1980(int daysSince1980)
        {
            var date = ISOXMLEpoch.AddDays(daysSince1980);
            return date.ToString(DATE_FORMAT);
        }
        public static string GetTimeFromMilliSeconds(uint milliSeconds)
        {
            milliSeconds /= 1000;
            var seconds = milliSeconds % 60;
            var minutes = milliSeconds / 60 % 60;
            var hours = milliSeconds / (60 * 60);
            var timeString = hours.ToString() + ":" + minutes.ToString() + ":" + seconds.ToString();

            return timeString;
        }


        public static DateTime GetDateTimeFromTimeLogInfos(int daysSince1980, uint milliSecondsSinceMidnight)
        {
            return ISOXMLEpoch.AddDays(daysSince1980 + milliSecondsSinceMidnight / MILLISECONDS_IN_DAY);
        }


        public static uint GetMilliSecondsInDay(DateTime timeStamp) => (uint)timeStamp.TimeOfDay.TotalMilliseconds;
        public static ushort GetDaysSince1980(DateTime dateTime)
        {

            return (ushort)dateTime.ToUniversalTime().Subtract(ISOXMLEpoch).TotalDays;

        }

    }
}
