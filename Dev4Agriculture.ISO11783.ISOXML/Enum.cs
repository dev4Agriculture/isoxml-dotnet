namespace Dev4Agriculture.ISO11783.ISOXML
{
    public enum DDIAvailabilityStatus
    {
        HAS_VALUE,
        NOT_IN_HEADER,
        NO_VALUE
    }

    public enum TLGTotalAlgorithmType
    {
        LIFETIME, //The last value counts
        NO_RESETS, //Last Value - First Value
        HAS_RESETS,//Whenever the next value is smaller than the Value before, a new, partial sumup starts
    }
}
