namespace Dev4Agriculture.ISO11783.ISOXML.Converters
{
    public class DeviceClassConversion
    {
        public static CulturalPracticesType MapDeviceClassToPracticeType(DeviceClass className)
        {
            switch (className)
            {
                case DeviceClass.NonSpecificSystem:
                    return CulturalPracticesType.Unknown;
                case DeviceClass.Tractor:
                    return CulturalPracticesType.Unknown;
                case DeviceClass.PrimarySoilTillage:
                    return CulturalPracticesType.Tillage;
                case DeviceClass.SecondarySoilTillage:
                    return CulturalPracticesType.Tillage;
                case DeviceClass.PlantersSeeders:
                    return CulturalPracticesType.SowingAndPlanting;
                case DeviceClass.Fertilizer:
                    return CulturalPracticesType.Fertilizing;
                case DeviceClass.Sprayers:
                    return CulturalPracticesType.CropProtection;
                case DeviceClass.Harvesters:
                    return CulturalPracticesType.Harvesting;
                case DeviceClass.RootHarvester:
                    return CulturalPracticesType.Harvesting;
                case DeviceClass.ForageHarvester:
                    return CulturalPracticesType.ForageHarvesting;
                case DeviceClass.Irrigation:
                    return CulturalPracticesType.Irrigation;
                case DeviceClass.TransportTrailers:
                    return CulturalPracticesType.Transport;
                case DeviceClass.FarmyardWork:
                    return CulturalPracticesType.Unknown;
                case DeviceClass.PoweredAuxilaryUnits:
                    return CulturalPracticesType.Unknown;
                case DeviceClass.SpecialCrops:
                    return CulturalPracticesType.Unknown;
                case DeviceClass.MunicipalWork:
                    return CulturalPracticesType.Unknown;
                case DeviceClass.UnDefined16:
                    return CulturalPracticesType.Unknown;
                case DeviceClass.SensorSystem:
                    return CulturalPracticesType.Unknown;
                case DeviceClass.ReservedForFutureAssignment:
                    return CulturalPracticesType.Unknown;
                case DeviceClass.TimberHarvesters:
                    return CulturalPracticesType.Harvesting;
                case DeviceClass.Forwarders:
                    return CulturalPracticesType.Transport;
                case DeviceClass.TimberLoaders:
                    return CulturalPracticesType.Transport;
                case DeviceClass.TimberProcessingMachines:
                    return CulturalPracticesType.Unknown;
                case DeviceClass.Mulchers:
                    return CulturalPracticesType.Mulching;
                case DeviceClass.UtilityVehicles:
                    return CulturalPracticesType.Unknown;
                case DeviceClass.FeederMixer:
                    return CulturalPracticesType.Unknown;
                case DeviceClass.SlurryApplicators:
                    return CulturalPracticesType.SlurryManureApplication;
                case DeviceClass.Reserved:
                    return CulturalPracticesType.Unknown;
                default:
                    return CulturalPracticesType.Unknown;
            };
        }
    }
}
