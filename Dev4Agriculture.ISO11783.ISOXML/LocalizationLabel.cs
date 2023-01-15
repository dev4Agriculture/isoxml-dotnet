using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public enum UnitSystem_No_US
    {
        METRIC = 0b00,
        IMPERIAL = 0b01,
        RESERVED = 0b10,
        UNKNOWN = 0b11
    }

    public enum UnitSystem_US
    {
        METRIC = 0b00,
        IMPERIAL = 0b01,
        US = 0b10,
        UNKNOWN = 0b11
    }


    public enum DecimalFormat
    {
        COMMA = 0,
        DOT = 1
    }

    public enum TimeFormat
    {
        TIME_24h = 0,
        TIME_12h = 1
    }


    public enum DateFormat
    {
        ddmmyyyy = 0,
        ddyyyymm = 1,
        mmyyyydd = 2,
        mmddyyyy = 3,
        yyyymmdd = 4,
        yyyyddmm = 5
    }

    public class LocalizationLabel
    {
        public DecimalFormat DecimalFormat { get; set; }
        public DateFormat DateFormat { get; set; }
        public TimeFormat TimeFormat { get; set; }

        public UnitSystem_No_US UnitDistance { get; set; }
        public UnitSystem_US UnitMass { get; set; }
        public UnitSystem_No_US UnitArea { get; set; }
        public UnitSystem_US UnitVolume { get; set; }
        public UnitSystem_No_US UnitTemperature { get; set; }
        public UnitSystem_No_US UnitForce { get; set; }
        public UnitSystem_No_US UnitPressure { get; set; }
        public UnitSystem_US UnitGeneral { get; set; }
        public byte Reserved;
        public byte[] LanguageShorting;

        public LocalizationLabel()
        {
            LanguageShorting = new byte[2];
        }


        public LocalizationLabel(byte[] data)
        {
            if (data == null || data.Length < 7)
            {
                throw new LocalizationLabelInvalidException();
            }
            LanguageShorting = new byte[] { data[0], data[1] };
            DecimalFormat = (DecimalFormat)((data[2] & ((1 << 7) + (1 << 6))) >> 6);
            TimeFormat = (TimeFormat)((data[2] & ((1 << 5) + (1 << 4))) >> 4);

            DateFormat = (DateFormat)data[3];

            UnitDistance = (UnitSystem_No_US)((data[4] & ((1 << 7) + (1 << 6))) >> 6);
            UnitArea = (UnitSystem_No_US)((data[4] & ((1 << 5) + (1 << 4))) >> 4);
            UnitVolume = (UnitSystem_US)((data[4] & ((1 << 3) + (1 << 2))) >> 2);
            UnitMass = (UnitSystem_US)((data[4] & ((1 << 1) + (1))));

            UnitForce = (UnitSystem_No_US)((data[5] & ((1 << 7) + (1 << 6))) >> 6);
            UnitPressure = (UnitSystem_No_US)((data[5] & ((1 << 5) + (1 << 4))) >> 4);
            UnitTemperature = (UnitSystem_No_US)((data[5] & ((1 << 3) + (1 << 2))) >> 2);
            UnitGeneral = (UnitSystem_US)((data[5] & ((1 << 1) + (1))));
            Reserved = data[6];
        }

        public LocalizationLabel(string data) : this(Utils.HexStringToByteArray(data).Reverse().ToArray())
        { }

        public byte[] ToArray()
        {
            var data = new byte[7];
            data[0] = LanguageShorting[0];
            data[1] = LanguageShorting[1];
            data[2] = (byte)(((int)DecimalFormat << 6) | ((int)TimeFormat << 4));
            data[3] = (byte)DateFormat;
            data[4] = (byte)(((int)UnitDistance << 6) | ((int)UnitArea << 4) | ((int)UnitVolume << 2) | (int)UnitMass);
            data[5] = (byte)(((int)UnitForce << 6) | ((int)UnitPressure << 4) | ((int)UnitTemperature << 2) | (int)UnitGeneral);
            data[6] = Reserved;
            return data.Reverse().ToArray();
        }



        public override string ToString()
        {
            return Utils.ByteArrayToHexString(ToArray());
        }

        public ResultMessageList Validate()
        {
            var list = new ResultMessageList();
            if (Reserved == 0xFF)
            {
                return list;
            }
            list.AddWarning(ResultMessageCode.LocalizationLabelWrongReservedValue, ResultDetail.FromNumber(Reserved));

            return list;
        }
    }
}
