using System;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public enum DeviceClass
    {
        NonSpecificSystem = 0,
        Tractor = 1,
        PrimarySoilTillage = 2,
        SecondarySoilTillage = 3,
        PlantersSeeders = 4,
        Fertilizer = 5,
        Sprayers = 6,
        Harvesters = 7,
        RootHarvester = 8,
        ForageHarvester = 9,
        Irrigation = 10,
        TransportTrailers = 11,
        FarmyardWork = 12,
        PoweredAuxilaryUnits = 13,
        SpecialCrops = 14,
        MunicipalWork = 15,
        UnDefined16 = 16,
        SensorSystem = 17,
        ReservedForFutureAssignment = 18,
        TimberHarvesters = 19,
        Forwarders = 20,
        TimberLoaders = 21,
        TimberProcessingMachines = 22,
        Mulchers = 23,
        UtilityVehicles = 24,
        FeederMixer = 25,
        SlurryApplicators = 26,
        Reserved = 27
    }

    public class WSM
    {

        public static int DeviceClassMin = Enum.GetValues(typeof(DeviceClass)).Cast<int>().Min();
        public static int DeviceClassMax = Enum.GetValues(typeof(DeviceClass)).Cast<int>().Max();
        private static readonly string[] Letters = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
        public bool SelfConfigurable { get; set; }
        public int IndustryGroup { get; set; }

        private int _deviceClassRaw;
        public DeviceClass DeviceClass
        {
            get
            {
                if (_deviceClassRaw <= DeviceClassMin)
                {
                    return DeviceClass.NonSpecificSystem;
                }
                else if (_deviceClassRaw >= DeviceClassMax)
                {
                    return DeviceClass.Reserved;
                }
                else
                {
                    return (DeviceClass)_deviceClassRaw;
                }
            }
            set => _deviceClassRaw = (byte)value;
        }
        public int DeviceClassInstance { get; set; }
        public int ManufacturerCode { get; set; }
        public int Function { get; set; }
        public int FunctionInstance { get; set; }
        public int EcuInstance { get; set; }
        public long SerialNo { get; set; }


        private byte[] ToByteArray()
        {
            var array = new byte[8];


            array[0] = (byte)(SerialNo & 0xFF);
            array[1] = (byte)((SerialNo >> 8) & 0xFF);
            array[2] = (byte)((byte)((SerialNo >> 16) & 0x1F) + (byte)((ManufacturerCode << 5) & 0xFF));
            array[3] = (byte)((ManufacturerCode) >> 3 & 0xFF);
            array[4] = (byte)(EcuInstance + (FunctionInstance << 3));
            array[5] = (byte)Function;
            array[6] = (byte)(_deviceClassRaw << 1);
            array[7] = (byte)(IndustryGroup << 4);
            array[7] += (byte)((SelfConfigurable ? 1 : 0) << 7);
            array[7] += (byte)DeviceClassInstance;

            return array;
        }
        public byte[] ToArray()
        {
            var array = ToByteArray();
            Array.Reverse(array);
            return array;

        }

        public override string ToString()
        {
            var array = ToByteArray();

            var workingSetMasterName = "";
            for (byte a = 0; a < 8; a++)
            {
                var upper = (byte)(array[a] & 0xF);
                var downer = (byte)((array[a] & 0xFF) >> 4);

                workingSetMasterName = Letters[downer] + Letters[upper] + workingSetMasterName;
            }

            return workingSetMasterName;
        }

        private void FillFromArray(byte[] array)
        {
            SelfConfigurable = (array[7] & 0xFF & 128) == 128;
            IndustryGroup = ((array[7] & 0xFF) >> 4) & 0x7;
            _deviceClassRaw = (array[6] & 0xFF) >> 1;
            DeviceClassInstance = array[7] & 0xFF & 0xF;
            ManufacturerCode = ((array[3] & 0xFF) << 3) | ((array[2] & 0xFF) >> 5);
            Function = array[5] & 0xFF;
            FunctionInstance = (array[4] & 0xFF) >> 3;
            EcuInstance = array[4] & 0xFF & 0x7;
            SerialNo = (array[2] & 0xFF & 0x1F) << 16 | ((array[1] & 0xFF) << 8) | (array[0] & 0xFF);
        }


        public WSM()
        {
            SelfConfigurable = false;
            IndustryGroup = 2;
            SerialNo = 0;
            ManufacturerCode = 0;
            DeviceClass = DeviceClass.NonSpecificSystem;
            Function = 0;
            FunctionInstance = 0;
            DeviceClassInstance = 0;
            EcuInstance = 0;
        }

        public WSM(string input)
        {
            input = input.Replace(" ", "");
            input = input.ToUpper();
            if (input.Length < 16)
            {
                throw new ClientNameTooShortException();
            } else if ( input.Length > 16)
            {
                throw new ClientNameTooLongException();
            }

            var array = new byte[8];

            for (var a = 0; a < 8; a++)
            {
                var letter1 = input.Substring(a * 2, 1);
                var letter2 = input.Substring(a * 2 + 1, 1);
                for (byte b = 0; b < Letters.Length; b++)
                {
                    if (letter1 == Letters[b])
                    {
                        array[7 - a] = b;
                        break;
                    }
                }
                array[7 - a] = (byte)(array[7 - a] * 16);
                for (byte b = 0; b < Letters.Length; b++)
                {
                    if (letter2 == Letters[b])
                    {
                        array[7 - a] += b;
                        break;
                    }
                }
            }
            FillFromArray(array);
        }

        public WSM(byte[] array)
        {
            array = array.Reverse().ToArray();
            FillFromArray(array);
        }


        public ResultMessageList Validate()
        {
            var list = new ResultMessageList();
            if (_deviceClassRaw < DeviceClassMin || _deviceClassRaw > DeviceClassMax)
            {
                list.AddWarning(ResultMessageCode.ClientNameDeviceClassInvalid, ResultDetail.FromNumber(_deviceClassRaw), ResultDetail.FromString(ToString()));
            }
            return list;
        }
    }
}
