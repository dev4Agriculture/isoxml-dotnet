using System;
using System.IO;
using System.Xml.Serialization;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LoadSimpleTaskData()
        {
            var serializer = new XmlSerializer(typeof(ISO11783TaskDataFile));
            var text = File.ReadAllText("./testdata/devices/TASKDATA_Devices.XML");
            using var reader = new StringReader(text);
            var output = (ISO11783TaskDataFile)serializer.Deserialize(reader);
            var subElements = output.Device;
            foreach (var codingData in subElements)
            {
                Console.WriteLine("Device Found: " + codingData.DeviceDesignator);
            }
        }

        [TestMethod]
        public void LoadSimpleDeviceDescription()
        {
            var serializer = new XmlSerializer(typeof(ISODevice));
            var text = File.ReadAllText("./testdata/devices/Device_description.xml");
            using var reader = new StringReader(text);
            try
            {
                var device = (ISODevice)serializer.Deserialize(reader);
                Console.WriteLine("Device Name: " + device.DeviceDesignator);
                Console.WriteLine("Device Id: " + device.DeviceId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        [TestMethod]
        public void CanHandleConstants()
        {
            Assert.AreEqual(Constants.TLG_VALUE_FOR_NO_VALUE, -1);
        }

    }
}
