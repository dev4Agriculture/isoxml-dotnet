﻿using System;

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
        private static readonly string[] Letters = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
        public bool SelfConfigurable { get; set; }
        public int IndustryGroup { get; set; }
        public DeviceClass DeviceClass { get; set; }
        public int DeviceClassInstance { get; set; }
        public int ManufacturerCode { get; set; }
        public int Function { get; set; }
        public int FunctionInstance { get; set; }
        public int EcuInstance { get; set; }
        public long SerialNo { get; set; }



        public override string ToString()
        {
            var array = new byte[8];


            array[0] = (byte)(SerialNo & 0xFF);
            array[1] = (byte)((SerialNo >> 8) & 0xFF);
            array[2] = (byte)((byte)((SerialNo >> 16) & 0x1F) + (byte)((ManufacturerCode << 5) & 0xFF));
            array[3] = (byte)((ManufacturerCode) >> 3 & 0xFF);
            array[4] = (byte)(EcuInstance + (FunctionInstance << 3));
            array[5] = (byte)Function;
            array[6] = (byte)((int)DeviceClass << 1);
            array[7] = (byte)(IndustryGroup << 4);
            array[7] += (byte)((SelfConfigurable ? 1 : 0) << 7);
            array[7] += (byte)DeviceClassInstance;


            var workingSetMasterName = "";
            for (byte a = 0; a < 8; a++)
            {
                var upper = (byte)(array[a] & 0xF);
                var downer = (byte)((array[a] & 0xFF) >> 4);

                workingSetMasterName = Letters[downer] + Letters[upper] + workingSetMasterName;
            }

            return workingSetMasterName;
        }

        public void WSMFromArray(byte[] array)
        {
            SelfConfigurable = (array[7] & 0xFF & 128) == 128;
            IndustryGroup = ((array[7] & 0xFF) >> 4) & 0x7;
            DeviceClass = (DeviceClass)((array[6] & 0xFF) >> 1);
            DeviceClassInstance = array[7] & 0xFF & 0xF;
            ManufacturerCode = ((array[3] & 0xFF) << 3) | ((array[2] & 0xFF) >> 5);
            Function = array[5] & 0xFF;
            FunctionInstance = (array[4] & 0xFF) >> 3;
            EcuInstance = array[4] & 0xFF & 0x7;
            SerialNo = (array[2] & 0xFF & 0x1F) << 16 | ((array[1] & 0xFF) << 8) | (array[0] & 0xFF);
        }


        public WSM(string input)
        {
            input = input.Replace(" ", "");
            if (input.Length != 16)
            {
                throw new Exception("WSM is invalid");
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
            WSMFromArray(array);
        }

        public WSM(byte[] array)
        {
            WSMFromArray(array);
        }
    }
}