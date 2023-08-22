using System;
using System.IO;
using System.Xml.Serialization;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test
{
    [TestClass]
    public class StructureTests
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
            Assert.AreEqual(Constants.TLG_VALUE_FOR_NO_VALUE, int.MinValue);
        }

        [TestMethod]
        public void CanReadTaskDataWithDuplicatedIDs()
        {
            var isoxml = ISOXML.Load("./testdata/Structure/DuplicatedIDs");

            Assert.AreEqual(2,isoxml.Messages.Count);

        }

        [TestMethod]
        public void DoesRecognizeDataOrignFMIS()
        {
            var isoxml = ISOXML.Create("test");
            isoxml.DataTransferOrigin = ISO11783TaskDataFileDataTransferOrigin.FMIS;
            var device = new ISODevice()
            {
                DeviceDesignator = "Machine"
            };

            var partfield = new ISOPartfield()
            {
                PartfieldDesignator = "Field"
            };

            var partfield2 = new ISOPartfield()
            {
                PartfieldDesignator = "Field2"
            };

            var deviceResult = isoxml.IdTable.AddObjectAndAssignIdIfNone(device);
            Assert.AreEqual("DVC1", device.DeviceId);
            var partFieldResult = isoxml.IdTable.AddObjectAndAssignIdIfNone(partfield);
            Assert.AreEqual("PFD1", partfield.PartfieldId);
            var partFieldResult2 = isoxml.IdTable.AddObjectAndAssignIdIfNone(partfield2);
            Assert.AreEqual("PFD2", partfield2.PartfieldId);



        }

        [TestMethod]
        public void DoesRecognizeDataOrignMICS()
        {
            var isoxml = ISOXML.Create("test");
            isoxml.DataTransferOrigin = ISO11783TaskDataFileDataTransferOrigin.MICS;
            var device = new ISODevice()
            {
                DeviceDesignator = "Machine"
            };

            var partfield = new ISOPartfield()
            {
                PartfieldDesignator = "Field"
            };

            var partfield2 = new ISOPartfield()
            {
                PartfieldDesignator = "Field2"
            };

            var deviceResult = isoxml.IdTable.AddObjectAndAssignIdIfNone(device);
            Assert.AreEqual("DVC-1", device.DeviceId);
            var partFieldResult = isoxml.IdTable.AddObjectAndAssignIdIfNone(partfield);
            Assert.AreEqual("PFD-1", partfield.PartfieldId);
            var partFieldResult2 = isoxml.IdTable.AddObjectAndAssignIdIfNone(partfield2);
            Assert.AreEqual("PFD-2", partfield2.PartfieldId);



        }
    }
}
