using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev4Agriculture.ISO11783.ISOXML.Test
{
    [TestClass]
    public class DeviceTests
    {
        [TestMethod]
        public void ParseDevice()
        {
            var path = "./testdata/devices/Device_description.xml";
            var result = ISOXML.Load(path);

            result.Messages.ForEach(msg =>
            {
                Console.WriteLine(msg.title);
            });
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(0, result.Messages.Count);

        }

        [TestMethod]
        public void DeviceWithBrokenScaleUnitShallCreateWarning()
        {
            var path = "./testdata/devices/Device_Description_Unit.xml";
            var result = ISOXML.Load(path);
            result.Messages.ForEach(msg => Console.WriteLine(msg.title));
            Assert.AreEqual(1, result.Messages.Count);
        }

        [TestMethod]
        public void CommentsShallNotThrowErrors()
        {
            var path = "./testdata/Structure/FileWithComment.XML";
            var result = ISOXML.Load(path);
            result.Messages.ForEach(msg => Console.WriteLine(msg.title));
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(ResultMessageType.Warning, result.Messages[0].type);
        }
    }
}

