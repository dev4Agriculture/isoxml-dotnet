using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4Agriculture.ISO11783.ISOXML.Converters
{
    public class DeviceClassConversion
    {
        public static CulturalPracticesType MapDeviceClassToPracticeType(DeviceClass className) => className switch
        {
            DeviceClass.NonSpecificSystem => CulturalPracticesType.Unknown,
            DeviceClass.Tractor => CulturalPracticesType.Unknown,
            DeviceClass.PrimarySoilTillage => CulturalPracticesType.Tillage,
            DeviceClass.SecondarySoilTillage => CulturalPracticesType.Tillage,
            DeviceClass.PlantersSeeders => CulturalPracticesType.SowingAndPlanting,
            DeviceClass.Fertilizer => CulturalPracticesType.Fertilizing,
            DeviceClass.Sprayers => CulturalPracticesType.CropProtection,
            DeviceClass.Harvesters => CulturalPracticesType.Harvesting,
            DeviceClass.RootHarvester => CulturalPracticesType.Harvesting,
            DeviceClass.ForageHarvester => CulturalPracticesType.ForageHarvesting,
            DeviceClass.Irrigation => CulturalPracticesType.Irrigation,
            DeviceClass.TransportTrailers => CulturalPracticesType.Transport,
            DeviceClass.FarmyardWork => CulturalPracticesType.Unknown,
            DeviceClass.PoweredAuxilaryUnits => CulturalPracticesType.Unknown,
            DeviceClass.SpecialCrops => CulturalPracticesType.Unknown,
            DeviceClass.MunicipalWork => CulturalPracticesType.Unknown,
            DeviceClass.UnDefined16 => CulturalPracticesType.Unknown,
            DeviceClass.SensorSystem => CulturalPracticesType.Unknown,
            DeviceClass.ReservedForFutureAssignment => CulturalPracticesType.Unknown,
            DeviceClass.TimberHarvesters => CulturalPracticesType.Harvesting,
            DeviceClass.Forwarders => CulturalPracticesType.Transport,
            DeviceClass.TimberLoaders => CulturalPracticesType.Transport,
            DeviceClass.TimberProcessingMachines => CulturalPracticesType.Unknown,
            DeviceClass.Mulchers => CulturalPracticesType.Mulching,
            DeviceClass.UtilityVehicles => CulturalPracticesType.Unknown,
            DeviceClass.FeederMixer => CulturalPracticesType.Unknown,
            DeviceClass.SlurryApplicators => CulturalPracticesType.SlurryManureApplication,
            DeviceClass.Reserved => CulturalPracticesType.Unknown,
            _ => CulturalPracticesType.Unknown
        };
    }
}
