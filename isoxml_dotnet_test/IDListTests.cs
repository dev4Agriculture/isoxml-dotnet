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

    }
}
