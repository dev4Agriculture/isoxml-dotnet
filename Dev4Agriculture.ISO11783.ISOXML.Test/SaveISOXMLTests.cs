using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Dev4Agriculture.ISO11783.ISOXML.Test
{
    [TestClass]
    public class SaveISOXMLTests
    {
        private void addData(ISOXML isoxml, string taskName)
        {
            var idTable = isoxml.IdTable;
            var task = new ISOTask()
            {
                TaskDesignator = taskName,
                TaskStatus = ISOTaskStatus.Completed
            };
            idTable.AddObjectAndAssignIdIfNone(task);
            isoxml.Data.Task.Add(task);

        }


        [TestMethod]
        public void canExtendISOXML()
        {
            var path = "./testdata/Structure/Valid_To_Extend/";
            var path_Out = "./out/Valid_Extended";
            var isoxml = ISOXML.Load(path);
            isoxml.SetFolderPath(path_Out);
            var taskName = "Hello";
            addData(isoxml, taskName);


            isoxml.Save();

            var tdPath = Path.Combine(path_Out, "TASKDATA.XML");
            Assert.IsTrue(File.Exists(tdPath));
            var allText = File.ReadAllText(tdPath);
            Assert.IsTrue(allText.Contains(taskName));

            var loaded = ISOXML.Load(path_Out);
            Assert.AreEqual(2, loaded.Data.Task.Count);
            Assert.AreEqual("TSK2", loaded.Data.Task[1].TaskId);

        }

        [TestMethod]
        public void canExtendISOXMLAsync()
        {
            var path = "./testdata/Structure/Valid_To_Extend/";
            var path_Out = "./out/Valid_Extended_Async";
            var waiter = ISOXML.LoadAsync(path);
            waiter.Wait();
            var isoxml = waiter.Result;
            isoxml.SetFolderPath(path_Out);
            var taskName = "Hello";
            addData(isoxml, taskName);


            var saver = isoxml.SaveAsync();
            saver.Wait();

            var tdPath = Path.Combine(path_Out, "TASKDATA.XML");
            Assert.IsTrue(File.Exists(tdPath));
            var allText = File.ReadAllText(tdPath);
            Assert.IsTrue(allText.Contains(taskName));

            var loaded = ISOXML.Load(path_Out);
            Assert.AreEqual(2, loaded.Data.Task.Count);
            Assert.AreEqual("TSK2", loaded.Data.Task[1].TaskId);

        }



        [TestMethod]
        public void canCreateISOXML()
        {
            var path = "./out/valid_new/";
            var taskName = "New";
            var isoxml = ISOXML.Create(path);
            var idTable = isoxml.IdTable;
            var task = new ISOTask()
            {
                TaskDesignator = taskName,
                TaskStatus = ISOTaskStatus.Completed
            };
            idTable.AddObjectAndAssignIdIfNone(task);
            isoxml.Data.Task.Add(task);
            isoxml.Save();

            var tdPath = Path.Combine(path, "TASKDATA.XML");
            Assert.IsTrue(File.Exists(tdPath));
            var allText = File.ReadAllText(tdPath);
            Assert.IsTrue(allText.Contains(taskName));

            var loaded = ISOXML.Load(path);
            Assert.AreEqual(1, loaded.Data.Task.Count);
            Assert.AreEqual("TSK1", loaded.Data.Task[0].TaskId);

        }


    }
}
