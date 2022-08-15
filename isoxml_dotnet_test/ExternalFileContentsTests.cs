using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev4Agriculture.ISO11783.ISOXML.Test
{
    [TestClass]
    public class ExternalFileContentsTests
    {
        [TestMethod]
        public void SingleTaskDataIsParsed()
        {
            var path = "./testdata/ExternalFiles/NoExternals/TASKDATA.XML";
            var result = ISOXML.Load(path);

            result.Messages.ForEach(msg =>
            {
                Console.WriteLine(msg.title);
            });
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(0, result.Messages.Count);

        }

        [TestMethod]
        public void TaskDataWithSingleExternalFileIsParsed()
        {
            var path = "./testdata/ExternalFiles/OneExternal/TASKDATA.XML";
            var result = ISOXML.Load(path);

            result.Messages.ForEach(msg =>
            {
                Console.WriteLine(msg.title);
            });
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(result.Data.Customer.Count, 3);
            Assert.AreEqual(result.Data.Task.Count, 3);
            Assert.AreEqual(0, result.Messages.Count);

        }

        [TestMethod]
        public void TaskDataWithMultipleExternalFilesIsParsed()
        {
            var path = "./testdata/ExternalFiles/MultipleExternals/TASKDATA.XML";
            var result = ISOXML.Load(path);

            result.Messages.ForEach(msg =>
            {
                Console.WriteLine(msg.title);
            });
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Data.Farm.Count, 2);
            Assert.AreEqual(result.Data.Customer.Count, 3);
            Assert.AreEqual(result.Data.Task.Count, 3);
            Assert.AreEqual(0, result.Messages.Count);
        }



        [TestMethod]
        public void TaskDataWithMissingExternalFilesIsRecognized()
        {
            var path = "./testdata/ExternalFiles/MissingExternals/TASKDATA.XML";
            var result = ISOXML.Load(path);

            result.Messages.ForEach(msg =>
            {
                Console.WriteLine(msg.title);
            });
            Assert.AreNotEqual(0, result.Messages.Count);
        }
    }
}

