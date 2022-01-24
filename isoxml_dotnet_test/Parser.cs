using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev4ag;
using System;

namespace isoxml_dotnet_test
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void ParseDevice()
        {
            string text = File.ReadAllText("./testdata/devices/Device_description.xml");
            var result = ISOXMLParser.ParseDeviceDescription(text);

            result.messages.ForEach(msg => {
                Console.WriteLine(msg.title);
            });
            Assert.IsNotNull(result.result);
            Assert.AreEqual(0, result.messages.Count);

        }
    }
}

