using Dev4ag.ISO11783.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Xml.Serialization;

namespace isoxml_dotnet_test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LoadSimpleTaskData()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ISO11783TaskDataFile));
            string text = File.ReadAllText("./testdata/devices/TASKDATA_Devices.XML");
            using ( StringReader reader = new StringReader(text))
            {
                var output = (ISO11783TaskDataFile)serializer.Deserialize(reader);
                var subElements = output.Device;
                foreach(var codingData in subElements)
                {
                 Console.WriteLine("Device Found: " + codingData.DeviceDesignator);
                }
            }
        }
    }
}
