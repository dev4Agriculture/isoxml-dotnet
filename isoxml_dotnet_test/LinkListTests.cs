using Dev4ag;
using Dev4ag.ISO11783.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace isoxml_dotnet_test.testdata
{
    [TestClass]
    public class LinkListTests
    {
        [TestMethod]
        public void CanReadValidLinkList()
        {
            var isoxml = ISOXML.Load("./testdata/LinkList/ValidLinkList");
            Assert.IsTrue(isoxml != null);
            Assert.IsTrue(isoxml.Data != null);
            Assert.IsTrue(isoxml.Messages.Count == 0);
            Assert.IsTrue(isoxml.HasLinkList == true);

        }

        [TestMethod]
        public void CanCreateValidLinkList()
        {
            string path = "./out/linklist/valid";
            var taskName = "LinkList";
            var uuid = Guid.NewGuid().ToString();
            var isoxml = new ISOXML(path);
            
            var task = new Task()
            {
                TaskDesignator = taskName,
                TaskStatus = TaskStatus.Planned
            };
            var id = isoxml.IdTable.AddObjectAndAssignIdIfNone(task);
            isoxml.Data.Task.Add(task);


            isoxml.AddLinkList();
            isoxml.LinkList.AddLink(id, uuid);

            isoxml.Save();

            var check = ISOXML.Load(path);
            Assert.IsTrue(check != null);
            Assert.AreEqual(check.Messages.Count, 0);
            Assert.IsTrue(check.LinkList.GetFirstLink(id).Equals(uuid));
        }

    }
}
