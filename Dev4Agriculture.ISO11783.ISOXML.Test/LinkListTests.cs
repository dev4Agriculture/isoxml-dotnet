using System;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.LinkListFile;
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

    [TestMethod]
    public void LoadedCorrectlyFromString()
    {
        var linkListData = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ISO11783LinkList VersionMajor=\"4\" VersionMinor=\"3\"\r\nTaskControllerManufacturer=\"FarmCtrl\" TaskControllerVersion=\"1.0\"\r\n" +
            "ManagementSoftwareManufacturer=\"FarmSystem\"\r\nManagementSoftwareVersion=\"1.0\" FileVersion=\"3.2\" DataTransferOrigin=\"2\">\r\n<LGP A=\"LGP10\" B=\"1\" E=\"Farming Data\">\r\n" +
            "<LNK A=\"CTR1\" B=\"{1059B14E-929F-4C4C-BCD4-C4F52A6A076A}\"/>\r\n<LNK A=\"PFD1\" B=\"{92043685-072D-4CAA-B3A8-C1E9D23BDB31}\" C=\"Headland\"/>\r\n</LGP>\r\n<LGP A=\"LGP31\" B=\"1\" " +
            "E=\"Devices\">\r\n<LNK A=\"DVC1\" B=\"{1059B14E-929F-4C4C-BCD4-C4F5286A07DA}\"/>\r\n<LNK A=\"TSK1\" B=\"{1059BD4E-979F-4C4C-BCD4-C4F5286A07DA}\"/>\r\n</LGP>\r\n</ISO11783LinkList>";

        var isoxml = ISOXML.Create("./out/isoxmlv3/linkListLoadedFromStr");
        isoxml.VersionMajor = ISO11783TaskDataFileVersionMajor.Version3;
        isoxml.LoadLinkListFromString(linkListData);
        isoxml.Save();

        Assert.IsTrue(isoxml.HasLinkList);
        Assert.AreEqual(0, isoxml.Messages.Count);
    }

    [TestMethod]
    public void NotLoadedFromInvalidString()
    {
        var invalidLinkListData = "<?xml version=\"1.0\"></ISO11783LinkList>";

        var isoxml = ISOXML.Create("./out/isoxmlv3/linkListLoadedFromStr");
        isoxml.VersionMajor = ISO11783TaskDataFileVersionMajor.Version3;
        isoxml.LoadLinkListFromString(invalidLinkListData);
        isoxml.Save();

        Assert.IsFalse(isoxml.HasLinkList);
        Assert.AreEqual(1, isoxml.Messages.Count);
    }


    [TestMethod]
    public void CanFindAllLinksAndClearLinkList()
    {
        var isoxml = ISOXML.Load("./testdata/LinkList/ValidLinkList_MultipleLinks");
        var deviceLinks = isoxml.LinkList.FindAllLinks("DVC1");
        var partfieldLinks = isoxml.LinkList.FindAllLinks("PFD1");
        Assert.AreEqual(deviceLinks.Count(), 3);
        Assert.AreEqual(partfieldLinks.Count(), 2);

        isoxml.LinkList.ClearLinks("PFD1");
        deviceLinks = isoxml.LinkList.FindAllLinks("DVC1");
        partfieldLinks = isoxml.LinkList.FindAllLinks("PFD1");
        Assert.AreEqual(deviceLinks.Count(), 3);
        Assert.AreEqual(partfieldLinks.Count(), 0);

        isoxml.LinkList.ClearLinkList();
        deviceLinks = isoxml.LinkList.FindAllLinks("DVC1");
        partfieldLinks = isoxml.LinkList.FindAllLinks("PFD1");
        Assert.AreEqual(deviceLinks.Count(), 0);
        Assert.AreEqual(partfieldLinks.Count(), 0);

        var linkList = new ISO11783LinkListFile()
        {
            FileVersion = isoxml.LinkList.FileVersion + "2",
            DataTransferOrigin = isoxml.LinkList.DataTransferOrigin,
            ManagementSoftwareManufacturer = isoxml.LinkList.ManagementSoftwareManufacturer,
            ManagementSoftwareVersion = isoxml.LinkList.ManagementSoftwareVersion,
            TaskControllerManufacturer = isoxml.LinkList.TaskControllerManufacturer,
            TaskControllerVersion = isoxml.LinkList.TaskControllerVersion,
            VersionMajor = isoxml.LinkList.VersionMajor,
            VersionMinor = isoxml.LinkList.VersionMinor
        };

        var group = new ISOLinkGroup()
        {
            LinkGroupDesignator = "New",
            LinkGroupType = ISOLinkGroupType.UUIDs
        };
        group.Link.Add(new ISOLink()
        {
            LinkDesignator = "Field",
            LinkValue = Guid.NewGuid().ToString(),
            ObjectIdRef = "PFD1"
        });
        linkList.LinkGroup.Add(group);

        isoxml.LinkList.SetLinkList(linkList);

        deviceLinks = isoxml.LinkList.FindAllLinks("DVC1");
        partfieldLinks = isoxml.LinkList.FindAllLinks("PFD1");
        Assert.AreEqual(deviceLinks.Count(), 0);
        Assert.AreEqual(partfieldLinks.Count(), 1);


    }
}
