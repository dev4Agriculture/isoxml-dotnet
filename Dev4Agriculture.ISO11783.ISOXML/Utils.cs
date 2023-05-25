using System;
using System.IO;
using System.Linq;
using System.Text;
using de.dev4Agriculture.ISOXML.DDI;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class Utils
    {

        /// <summary>
        /// This is a small helper function to create valid DDIs for e.g. DeviceProperty (DPT) and DeviceProcessData(DPD)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] FormatDDI(ushort value)
        {
            var longArray = BitConverter.GetBytes(value);
            var byteArray = new byte[2];
            byteArray[0] = longArray[1];
            byteArray[1] = longArray[0];
            return byteArray;
        }
        
        public static byte[] FormatDDI(DDIList value) => FormatDDI((ushort)value);
        public static ushort ConvertDDI(byte[] entry) => BitConverter.ToUInt16(entry.Reverse().ToArray());
        public static ushort ParseDDI(string entry) => Convert.ToUInt16(entry,16);



        public static bool AdjustFileNameToIgnoreCasing(string root, string fileName, out string path)
        {

            fileName = fileName.ToLower();
            if (!Directory.Exists(Path.GetFullPath(root)))
            {
                path = "";
                return false;
            }
            foreach (var file in Directory.GetFiles(root))
            {
                if (file.ToLower().EndsWith(fileName))
                {
                    path = file;
                    return true;
                }
            }

            foreach (var subdir in Directory.GetDirectories(root))
            {
                foreach (var file in Directory.GetFiles(Path.Combine(root, subdir)))
                {
                    if (file.ToLower().EndsWith(fileName))
                    {
                        path = file;
                        return true;
                    }
                }
            }

            path = "";
            return false;
        }


        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string ByteArrayToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "");
        }

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

    public sealed class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding encoding;

        public StringWriterWithEncoding() : this(Encoding.UTF8) { }

        public StringWriterWithEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }
}
