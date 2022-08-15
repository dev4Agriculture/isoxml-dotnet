﻿using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev4Agriculture.ISO11783.ISOXML.Test
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
            var path = "./out/linklist/valid";
            var taskName = "LinkList";
            var uuid = Guid.NewGuid().ToString();
            var isoxml = ISOXML.Create(path);

            var task = new ISOTask()
            {
                TaskDesignator = taskName,
                TaskStatus = ISOTaskStatus.Planned
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