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
            isoxml.data.Task.Add(new ISO11783.TaskFile.Task()
            {
                TaskDesignator = taskName,
                TaskStatus = ISO11783.TaskFile.TaskStatus.Completed
            });

            isoxml.save();

            string tdPath = Path.Combine(path, "TASKDATA.XML");
            Assert.IsTrue(File.Exists(tdPath));
            string allText = File.ReadAllText(tdPath);
            Assert.IsTrue(allText.Contains(taskName));

        }
    }
}
