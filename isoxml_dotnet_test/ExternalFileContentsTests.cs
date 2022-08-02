using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev4ag;
using System;

namespace isoxml_dotnet_test
{
    [TestClass]
    public class ExternalFileContentsTests
    {
        [TestMethod]
        public void SingleTaskDataIsParsed()
        {
            string path = "./testdata/ExternalFiles/NoExternals/TASKDATA.XML";
            var result = ISOXMLParser.Load(path);

            result.messages.ForEach(msg => {
                Console.WriteLine(msg.title);
            });
            Assert.IsNotNull(result.result);
            Assert.AreEqual(0, result.messages.Count);

        }

        [TestMethod]
        public void TaskDataWithSingleExternalFileIsParsed()
        {
            string path = "./testdata/ExternalFiles/OneExternal/TASKDATA.XML";
            var result = ISOXMLParser.Load(path);

            result.messages.ForEach(msg => {
                Console.WriteLine(msg.title);
            });
            Assert.IsNotNull(result.result);
            Assert.AreEqual(result.result.Customer.Count, 3);
            Assert.AreEqual(result.result.Task.Count, 3);
            Assert.AreEqual(0, result.messages.Count);

        }

        [TestMethod]
        public void TaskDataWithMultipleExternalFilesIsParsed()
        {
            string path = "./testdata/ExternalFiles/MultipleExternals/TASKDATA.XML";
            var result = ISOXMLParser.Load(path);

            result.messages.ForEach(msg => {
                Console.WriteLine(msg.title);
            });
            Assert.IsNotNull(result.result);
            Assert.AreEqual(result.result.Farm.Count, 2);
            Assert.AreEqual(result.result.Customer.Count, 3);
            Assert.AreEqual(result.result.Task.Count, 3);
            Assert.AreEqual(0, result.messages.Count);
        }



        [TestMethod]
        public void TaskDataWithMissingExternalFilesIsRecognized()
        {
            string path = "./testdata/ExternalFiles/MissingExternals/TASKDATA.XML";
            var result = ISOXMLParser.Load(path);

            result.messages.ForEach(msg => {
                Console.WriteLine(msg.title);
            });
            Assert.AreNotEqual(0, result.messages.Count);
        }
    }
}

