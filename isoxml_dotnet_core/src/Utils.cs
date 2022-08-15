using System;
using System.Collections.Generic;
using System.Text;

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
            byte[] longArray = BitConverter.GetBytes(value);
            byte[] byteArray = new byte[2];
            byteArray[0] = longArray[1];
            byteArray[1] = longArray[0];
            return byteArray;
        }

    }
}
