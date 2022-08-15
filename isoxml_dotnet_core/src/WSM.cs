using System;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public enum DeviceClass
    {
        NonSpecificSystem,
        Tractor,
        PrimarySoilTillage,
        SecondarySoilTillage,
        PlantersSeeders_,
        Fertilizer,
        Sprayers,
        Harvesters,
        RootHarvester,
        ForageHarvester,
        Irrigation,
        TransportTrailers_,
        FarmyardWork,
        PoweredAuxilary_Units,
        SpecialCrops,
        MunicipalWork,
        SensorSystem,
        ReservedForFutureAssignment,
        TimberHarvesters,
        Forwarders,
        TimberLoaders,
        TimberProcessingMachines,
        Mulchers,
        UtilityVehicles,
        FeederMixer,
        SlurryApplicators,
    }

    public class WSM
    {
        static private String[] letters = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
        public bool selfConfigurable;
        public int industryGroup;
        public DeviceClass deviceClass;
        public int deviceClassInstance;
        public int manufacturerCode;
        public int function;
        public int functionInstance;
        public int ecuInstance;
        public long serialNo;



        public String toString()
        {
            byte[] array = new byte[8];


            array[0] = (byte)(this.serialNo & 0xFF);
            array[1] = (byte)((this.serialNo >> 8) & 0xFF);
            array[2] = (byte)((byte)((this.serialNo >> 16) & 0x1F) + (byte)(((manufacturerCode << 5) & 0xFF)));
            array[3] = (byte)((this.manufacturerCode) >> 3 & 0xFF);
            array[4] = (byte)(ecuInstance + (functionInstance << 3));
            array[5] = (byte)(function);
            array[6] = (byte)((int)deviceClass << 1);
            array[7] = (byte)(industryGroup << 4);
            array[7] += (byte)((selfConfigurable ? 1 : 0) << 7);
            array[7] += (byte)(deviceClassInstance);


            String workingSetMasterName = "";
            for (byte a = 0; a < 8; a++)
            {
                byte upper = (byte)(array[a] & 0xF);
                byte downer = (byte)((array[a] & 0xFF) >> 4);

                workingSetMasterName = letters[downer] + letters[upper] + workingSetMasterName;
            }

            return workingSetMasterName;
        }

        public void wsmFromArray(byte[] array)
        {
            this.selfConfigurable = ((array[7] & 0xFF) & 128) == 128;
            this.industryGroup = ((array[7] & 0xFF) >> 4) & 0x7;
            this.deviceClass = (DeviceClass)((array[6] & 0xFF) >> 1);
            this.deviceClassInstance = ((array[7] & 0xFF) & 0xF);
            this.manufacturerCode = (((array[3] & 0xFF) << 3) | ((array[2] & 0xFF) >> 5));
            this.function = ((array[5] & 0xFF));
            this.functionInstance = ((array[4] & 0xFF) >> 3);
            this.ecuInstance = (array[4] & 0xFF) & 0x7;
            this.serialNo = ((array[2] & 0xFF) & 0x1F) << 16 | ((array[1] & 0xFF) << 8) | (array[0] & 0xFF);
        }


        public WSM(String input)
        {
            input = input.Replace(" ", "");
            if (input.Length != 16)
            {
                throw new Exception("WSM is invalid");
            }

            byte[] array = new byte[8];

            for (int a = 0; a < 8; a++)
            {
                String letter1 = input.Substring(a * 2, 1);
                String letter2 = input.Substring(a * 2 + 1, 1);
                for (byte b = 0; b < letters.Length; b++)
                {
                    if (letter1 == letters[b])
                    {
                        array[7 - a] = b;
                        break;
                    }
                }
                array[7 - a] = (byte)(array[7 - a] * 16);
                for (byte b = 0; b < letters.Length; b++)
                {
                    if (letter2 == letters[b])
                    {
                        array[7 - a] += b;
                        break;
                    }
                }
            }
            this.wsmFromArray(array);
        }

        public WSM(byte[] array)
        {
            this.wsmFromArray(array);
        }
    }
}
