using System;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class DateUtilities
    {
        public static string GetDateFromDaysSince1980(int daysSince1980)
        {
            var date = new DateTime(1980, 1, 1).AddDays(daysSince1980);
            return date.ToString("dd.MM.yyyy");
        }
        public static string GetTimeFromMilliSeconds(uint milliSeconds)
        {
            milliSeconds /= 1000;
            var seconds = milliSeconds % 60;
            var minutes = (milliSeconds / 60) % 60;
            var hours = (milliSeconds / (60 * 60));
            var timeString = hours.ToString() + ":" + minutes.ToString() + ":" + seconds.ToString();

            return timeString;
        }
    }
}
