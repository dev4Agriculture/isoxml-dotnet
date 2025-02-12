using System;
using System.Linq;
using de.dev4Agriculture.ISOXML.DDI;

namespace Dev4Agriculture.ISO11783.ISOXML.Utils
{
    public class DDIUtils
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
        public static ushort ConvertDDI(byte[] entry) => BitConverter.ToUInt16(entry.Reverse().ToArray(),0);
        public static ushort ParseDDI(string entry) => Convert.ToUInt16(entry, 16);
    }
}
