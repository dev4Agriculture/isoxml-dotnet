using System;
using System.IO;
using System.Linq;

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


        public static ushort ConvertDDI(byte[] entry) => BitConverter.ToUInt16(entry.Reverse().ToArray());


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
    }
}
