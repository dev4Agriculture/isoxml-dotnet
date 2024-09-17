namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class Constants
    {
        //TODO It's more save to move this Element to a common class. It's found in Generator and DotnetCore Library
        public static string ISOXMLClassName = "Dev4Agriculture.ISO11783.ISOXML";
        public static string Version = "V0.19.6.3";//Currently unused; Just for the commit
        public static string Author = "Frank Wiebeler, dev4Agriculture";
        public const int TLG_VALUE_FOR_NO_VALUE = int.MinValue;
        public const double EarthRadiusInMeters = 6378137;
        public const int NUMBER_OF_DIGITS_FOR_POSITIONS = 9;
    }
}
