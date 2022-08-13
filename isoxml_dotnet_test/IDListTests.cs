using Dev4ag;
using Dev4ag.ISO11783.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace isoxml_dotnet_test
{
    [TestClass]
    public class IDListTests
    {
        [TestMethod]
        public void canFindID()
        {
            Task task = new Task();
            task.TaskId = "TSK1";
            IdList idList = new IdList("TSK");
            idList.AddObjectAndAssignIdIfNone(task);
            Assert.AreEqual(idList.FindObject("TSK1"),task);
        }


        [TestMethod]
        public void canGenerateIds()
        {
            IdList idList = new IdList("TSK");

            //Valid Object
            Task task1 = new Task();
            task1.TaskId = "TSK1";
            task1.TaskDesignator = "Task1";
            idList.ReadObject(task1);

            //Add a Task without assigning an ID
            Task task3 = new Task();
            task3.TaskDesignator = "Task3";
            idList.ReadObject(task3);

            //Add one where an ID is forced to be created
            Task task2 = new Task();
            task2.TaskDesignator = "Task2";
            idList.AddObjectAndAssignIdIfNone(task2);

            idList.CleanListFromTempEntries();

            Assert.AreEqual(((Task)idList.FindObject("TSK1")).TaskId, "TSK1");
            Assert.AreEqual(((Task)idList.FindObject("TSK2")).TaskId, "TSK2");
            Assert.AreEqual(((Task)idList.FindObject("TSK3")).TaskId, "TSK3");
                   
            Assert.AreEqual(((Task)idList.FindObject("TSK1")).TaskDesignator, "Task1");
            Assert.AreEqual(((Task)idList.FindObject("TSK2")).TaskDesignator, "Task2");
            Assert.AreEqual(((Task)idList.FindObject("TSK3")).TaskDesignator, "Task3");






        }

    }
}
