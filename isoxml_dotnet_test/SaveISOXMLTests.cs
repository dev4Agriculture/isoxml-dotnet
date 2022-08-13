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
        private void addData(ISOXML isoxml, string taskName)
        {
            var idTable = isoxml.IdTable;
            Task task = new Task()
            {
                TaskDesignator = taskName,
                TaskStatus = TaskStatus.Completed
            };
            task.TaskId = idTable.AddObject(task);
            isoxml.Data.Task.Add(task);

        }


        [TestMethod]
        public void canExtendISOXML()
        {
            string path = "./testdata/Structure/Valid_To_Extend/";
            string path_Out = "./out/Valid_Extended";
            var isoxml = ISOXML.Load(path);
            isoxml.SetFolderPath( path_Out);
            string taskName = "Hello";
            addData(isoxml, taskName); 


            isoxml.Save();

            string tdPath = Path.Combine(path_Out, "TASKDATA.XML");
            Assert.IsTrue(File.Exists(tdPath));
            string allText = File.ReadAllText(tdPath);
            Assert.IsTrue(allText.Contains(taskName));

            var loaded = ISOXML.Load(path_Out);
            Assert.AreEqual(2,loaded.Data.Task.Count);
            Assert.AreEqual("TSK2", loaded.Data.Task[1].TaskId);

        }

        [TestMethod]
        public void canExtendISOXMLAsync()
        {
            string path = "./testdata/Structure/Valid_To_Extend/";
            string path_Out = "./out/Valid_Extended_Async";
            var waiter = ISOXML.LoadAsync(path);
            waiter.Wait();
            var isoxml = waiter.Result;
            isoxml.SetFolderPath( path_Out);
            string taskName = "Hello";
            addData(isoxml, taskName);


            var saver =  isoxml.SaveAsync();
            saver.Wait();

            string tdPath = Path.Combine(path_Out, "TASKDATA.XML");
            Assert.IsTrue(File.Exists(tdPath));
            string allText = File.ReadAllText(tdPath);
            Assert.IsTrue(allText.Contains(taskName));

            var loaded = ISOXML.Load(path_Out);
            Assert.AreEqual(2, loaded.Data.Task.Count);
            Assert.AreEqual("TSK2", loaded.Data.Task[1].TaskId);

        }



        [TestMethod]
        public void canCreateISOXML()
        {
            string path = "./out/valid_new/";
            string taskName = "New";
            var isoxml = new ISOXML(path);
            var idTable = isoxml.IdTable;
            Task task = new Task()
            {
                TaskDesignator = taskName,
                TaskStatus = TaskStatus.Completed
            };
            task.TaskId = idTable.AddObject(task);
            isoxml.Data.Task.Add(task);
            isoxml.Save();

            string tdPath = Path.Combine(path, "TASKDATA.XML");
            Assert.IsTrue(File.Exists(tdPath));
            string allText = File.ReadAllText(tdPath);
            Assert.IsTrue(allText.Contains(taskName));

            var loaded = ISOXML.Load(path);
            Assert.AreEqual(1, loaded.Data.Task.Count);
            Assert.AreEqual("TSK1", loaded.Data.Task[0].TaskId);

        }


    }
}
