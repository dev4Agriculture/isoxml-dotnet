using Dev4Agriculture.ISO11783.ISOXML;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace isoxml_dotnet_test
{
    [TestClass]
    public class IDListTests
    {
        [TestMethod]
        public void canFindID()
        {
            ISOTask task = new ISOTask();
            task.TaskId = "TSK1";
            IdList idList = new IdList("TSK");
            idList.AddObjectAndAssignIdIfNone(task);
            Assert.AreEqual(idList.FindObject("TSK1"), task);
        }


        [TestMethod]
        public void canGenerateIds()
        {
            IdList idList = new IdList("TSK");

            //Valid Object
            ISOTask task1 = new ISOTask();
            task1.TaskId = "TSK1";
            task1.TaskDesignator = "Task1";
            idList.ReadObject(task1);

            //Add a Task without assigning an ID
            ISOTask task3 = new ISOTask();
            task3.TaskDesignator = "Task3";
            idList.ReadObject(task3);

            //Add one where an ID is forced to be created
            ISOTask task2 = new ISOTask();
            task2.TaskDesignator = "Task2";
            idList.AddObjectAndAssignIdIfNone(task2);

            idList.CleanListFromTempEntries();

            Assert.AreEqual(((ISOTask)idList.FindObject("TSK1")).TaskId, "TSK1");
            Assert.AreEqual(((ISOTask)idList.FindObject("TSK2")).TaskId, "TSK2");
            Assert.AreEqual(((ISOTask)idList.FindObject("TSK3")).TaskId, "TSK3");

            Assert.AreEqual(((ISOTask)idList.FindObject("TSK1")).TaskDesignator, "Task1");
            Assert.AreEqual(((ISOTask)idList.FindObject("TSK2")).TaskDesignator, "Task2");
            Assert.AreEqual(((ISOTask)idList.FindObject("TSK3")).TaskDesignator, "Task3");






        }

    }
}
