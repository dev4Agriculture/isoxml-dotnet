using System;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class Utils
    {

        /// <summary>
        /// This is a small helper function to create valid DDIs for e.g. DeviceProperty (DPT) and DeviceProcessData(DPD)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] FormatDDI(uint value)
        {
            var longArray = BitConverter.GetBytes(value);
            var byteArray = new byte[2];
            byteArray[0] = longArray[1];
            byteArray[1] = longArray[0];
            return byteArray;
        }

    }
}
