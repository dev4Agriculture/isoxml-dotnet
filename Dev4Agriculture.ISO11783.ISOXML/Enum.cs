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

    public enum CulturalPracticesType
    {
        Unknown = 0,
        Fertilizing = 1,
        SowingAndPlanting = 2,
        CropProtection = 3,
        Tillage = 4,
        BalingPressing = 5,
        Mowing = 6,
        Wrapping = 7,
        Harvesting = 8,
        ForageHarvesting = 9,
        Transport = 10,
        Swathing = 11,
        SlurryManureApplication = 12,
        SelfLoadingWagon = 13,
        Tedding = 14,
        Measuring = 15,
        Irrigation = 16,
        FeedingMixing = 17,
        Mulching = 18
    }
}
