using System;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

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
        Assert.IsTrue(isoxml.HasLinkList);

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


    [TestMethod]
    public void SettingCoreInfosUpdatesTaskDataAndLinkList()
    {

        var isoxml = ISOXML.Create("./out/coredata_withlinkList/");
        isoxml.AddLinkList();
        isoxml.VersionMajor = ISO11783TaskDataFileVersionMajor.Version4;
        isoxml.VersionMinor = ISO11783TaskDataFileVersionMinor.Item3;
        isoxml.ManagementSoftwareManufacturer = "dev4Agriculture";
        isoxml.ManagementSoftwareVersion = "1234";
        isoxml.TaskControllerManufacturer = "dev4Ag";
        isoxml.TaskControllerVersion = "2345";
        isoxml.DataTransferOrigin = ISO11783TaskDataFileDataTransferOrigin.FMIS;

        Assert.AreEqual(isoxml.VersionMajor, isoxml.Data.VersionMajor);
        Assert.AreEqual(isoxml.VersionMinor, isoxml.Data.VersionMinor);
        Assert.AreEqual(isoxml.ManagementSoftwareManufacturer, isoxml.Data.ManagementSoftwareManufacturer);
        Assert.AreEqual(isoxml.ManagementSoftwareVersion, isoxml.Data.ManagementSoftwareVersion);
        Assert.AreEqual(isoxml.TaskControllerManufacturer, isoxml.Data.TaskControllerManufacturer);
        Assert.AreEqual(isoxml.TaskControllerVersion, isoxml.Data.TaskControllerVersion);
        Assert.AreEqual(isoxml.DataTransferOrigin, isoxml.Data.DataTransferOrigin);

        Assert.AreEqual(isoxml.VersionMajor.ToString(), isoxml.LinkList.VersionMajor.ToString());
        Assert.AreEqual(isoxml.VersionMinor.ToString(), isoxml.LinkList.VersionMinor.ToString());
        Assert.AreEqual(isoxml.ManagementSoftwareManufacturer, isoxml.LinkList.ManagementSoftwareManufacturer);
        Assert.AreEqual(isoxml.ManagementSoftwareVersion, isoxml.LinkList.ManagementSoftwareVersion);
        Assert.AreEqual(isoxml.TaskControllerManufacturer, isoxml.LinkList.TaskControllerManufacturer);
        Assert.AreEqual(isoxml.TaskControllerVersion, isoxml.LinkList.TaskControllerVersion);
        Assert.AreEqual(isoxml.DataTransferOrigin.ToString(), isoxml.LinkList.DataTransferOrigin.ToString());
    }

    [TestMethod]
    public void SettingCoreInfosUpdatesTaskDataAndLinkListWhenAddingLinkListLater()
    {

        var isoxml = ISOXML.Create("./out/coredata_withlinkList/");
        isoxml.VersionMajor = ISO11783TaskDataFileVersionMajor.Version4;
        isoxml.VersionMinor = ISO11783TaskDataFileVersionMinor.Item3;
        isoxml.ManagementSoftwareManufacturer = "dev4Agriculture";
        isoxml.ManagementSoftwareVersion = "1234";
        isoxml.TaskControllerManufacturer = "dev4Ag";
        isoxml.TaskControllerVersion = "2345";
        isoxml.DataTransferOrigin = ISO11783TaskDataFileDataTransferOrigin.FMIS;
        isoxml.AddLinkList();

        Assert.AreEqual(isoxml.VersionMajor, isoxml.Data.VersionMajor);
        Assert.AreEqual(isoxml.VersionMinor, isoxml.Data.VersionMinor);
        Assert.AreEqual(isoxml.ManagementSoftwareManufacturer, isoxml.Data.ManagementSoftwareManufacturer);
        Assert.AreEqual(isoxml.ManagementSoftwareVersion, isoxml.Data.ManagementSoftwareVersion);
        Assert.AreEqual(isoxml.TaskControllerManufacturer, isoxml.Data.TaskControllerManufacturer);
        Assert.AreEqual(isoxml.TaskControllerVersion, isoxml.Data.TaskControllerVersion);
        Assert.AreEqual(isoxml.DataTransferOrigin, isoxml.Data.DataTransferOrigin);

        Assert.AreEqual(isoxml.VersionMajor.ToString(), isoxml.LinkList.VersionMajor.ToString());
        Assert.AreEqual(isoxml.VersionMinor.ToString(), isoxml.LinkList.VersionMinor.ToString());
        Assert.AreEqual(isoxml.ManagementSoftwareManufacturer, isoxml.LinkList.ManagementSoftwareManufacturer);
        Assert.AreEqual(isoxml.ManagementSoftwareVersion, isoxml.LinkList.ManagementSoftwareVersion);
        Assert.AreEqual(isoxml.TaskControllerManufacturer, isoxml.LinkList.TaskControllerManufacturer);
        Assert.AreEqual(isoxml.TaskControllerVersion, isoxml.LinkList.TaskControllerVersion);
        Assert.AreEqual(isoxml.DataTransferOrigin.ToString(), isoxml.LinkList.DataTransferOrigin.ToString());
    }



}
