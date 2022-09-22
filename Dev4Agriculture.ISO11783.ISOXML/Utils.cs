using System;
using System.IO;

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

            path = "";
            return false;
        }

    }
}
