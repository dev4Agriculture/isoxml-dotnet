using Dev4ag.ISO11783.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dev4ag
{
    [TestClass]
    public class SaveISOXMLTests
    {
        [TestMethod]
        public void canCreateISOXML()
        {
            string path = "./out/valid/";
            string taskName = "Hello";
            var isoxml = new ISOXML(path);
            var idTable = isoxml.idTable;
            Task task = new Task()
            {
                TaskDesignator = taskName,
                TaskStatus = TaskStatus.Completed
            };
            task.TaskId = idTable.getNewId(task);
            isoxml.data.Task.Add(task);

            isoxml.save();

            string tdPath = Path.Combine(path, "TASKDATA.XML");
            Assert.IsTrue(File.Exists(tdPath));
            string allText = File.ReadAllText(tdPath);
            Assert.IsTrue(allText.Contains(taskName));

            var loaded = ISOXML.Load(path);
            Assert.AreEqual(1,loaded.data.Task.Count);
            Assert.AreEqual("TSK1", loaded.data.Task[0].TaskId);

        }
    }
}
