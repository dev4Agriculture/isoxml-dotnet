using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev4ag;
using System;

namespace Dev4ag
{
    [TestClass]
    public class DeviceTests
    {
        [TestMethod]
        public void ParseDevice()
        {
            string path = "./testdata/devices/Device_description.xml";
            var result = ISOXML.Load(path);

            result.messages.ForEach(msg => {
                Console.WriteLine(msg.title);
            });
            Assert.IsNotNull(result.data);
            Assert.AreEqual(0, result.messages.Count);

        }

        [TestMethod]
        public void DeviceWithBrokenScaleUnitShallCreateWarning()
        {
            string path = "./testdata/devices/Device_Description_Unit.xml";
            var result = ISOXML.Load(path);
            result.messages.ForEach(msg => Console.WriteLine(msg.title));
            Assert.AreEqual(1, result.messages.Count);
        }

        [TestMethod]
        public void CommentsShallNotThrowErrors()
        {
            string path = "./testdata/Structure/FileWithComment.XML";
            var result = ISOXML.Load(path);
            result.messages.ForEach(msg => Console.WriteLine(msg.title));
            Assert.AreEqual(1, result.messages.Count);
            Assert.AreEqual(ResultMessageType.Warning, result.messages[0].type);
        }
    }
}

