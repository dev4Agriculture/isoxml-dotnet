using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class IDListTests
{
    [TestMethod]
    public void CanFindID()
    {
        var task = new ISOTask
        {
            TaskId = "TSK1"
        };
        var idList = new IdList("TSK");
        idList.AddObjectAndAssignIdIfNone(task);
        Assert.AreEqual(idList.FindObject("TSK1"), task);
    }


    [TestMethod]
    public void CanGenerateIds()
    {
        var idList = new IdList("TSK");

        //Valid Object
        var task1 = new ISOTask
        {
            TaskId = "TSK1",
            TaskDesignator = "Task1"
        };
        idList.ReadObject(task1);

        //Add a Task without assigning an ID
        var task3 = new ISOTask
        {
            TaskDesignator = "Task3"
        };
        idList.ReadObject(task3);

        //Add one where an ID is forced to be created
        var task2 = new ISOTask
        {
            TaskDesignator = "Task2"
        };
        idList.AddObjectAndAssignIdIfNone(task2);

        idList.CleanListFromTempEntries();

        Assert.AreEqual(((ISOTask)idList.FindObject("TSK1")).TaskId, "TSK1");
        Assert.AreEqual(((ISOTask)idList.FindObject("TSK2")).TaskId, "TSK2");
        Assert.AreEqual(((ISOTask)idList.FindObject("TSK3")).TaskId, "TSK3");

        Assert.AreEqual(((ISOTask)idList.FindObject("TSK1")).TaskDesignator, "Task1");
        Assert.AreEqual(((ISOTask)idList.FindObject("TSK2")).TaskDesignator, "Task2");
        Assert.AreEqual(((ISOTask)idList.FindObject("TSK3")).TaskDesignator, "Task3");

    }

    [TestMethod]
    public void CanGenerateAndStoreIDs()
    {
        var firstPath = "./testdata/IDList/Valid";
        var secondPath = "./testdata/IDList/Valid/Export";
        var isoxml = ISOXML.Load(firstPath);
        Assert.AreEqual(isoxml.Messages.Count, 0);
        Assert.IsTrue(isoxml.IdTable.FindById("CTP3") is ISOCropType);
        Assert.AreEqual(isoxml.IdTable.FindById("FRM32"), null);

        var variety = new ISOCropVariety()
        {
            CropVarietyDesignator = "Test"
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(variety);
        isoxml.Data.CropType[0].CropVariety.Add(variety);

        var commentListValue = new ISOCodedCommentListValue()
        {
            CodedCommentListValueDesignator = "Test Comment Designator",
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(commentListValue);

        var codedComment = new ISOCodedComment()
        {
            CodedCommentDesignator = "Test"
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(codedComment);

        codedComment.CodedCommentListValue.Add(commentListValue);

        isoxml.Data.CodedComment.Add(codedComment);

        isoxml.SetFolderPath(secondPath);
        isoxml.Save();


        var check = ISOXML.Load(secondPath);
        Assert.AreEqual(isoxml.Messages.Count, 0);
        Assert.IsTrue(isoxml.IdTable.FindById("CCL1") is ISOCodedCommentListValue);
    }

    [TestMethod]
    public void CanGenerateIdsForDeviceElement()
    {
        var isoxml = ISOXML.Create("");
        for (var a = 1; a < 3; a++)
        {
            var device = new ISODevice();
            isoxml.IdTable.AddObjectAndAssignIdIfNone(device);
            for (var b = 0; b < 3; b++)
            {
                var det = new ISODeviceElement();
                isoxml.IdTable.AddObjectAndAssignIdIfNone(det);
                device.DeviceElement.Add(det);
            }
            isoxml.Data.Device.Add(device);
        }
        Assert.IsNotNull(isoxml.IdTable.FindById("DET1"));
        Assert.IsNotNull(isoxml.IdTable.FindById("DET2"));
        Assert.IsNotNull(isoxml.IdTable.FindById("DET3"));
        Assert.IsNotNull(isoxml.IdTable.FindById("DET4"));
        Assert.IsNull(isoxml.IdTable.FindById("DET10"));
        Assert.IsNull(isoxml.IdTable.FindById("DET-1"));
    }


    [TestMethod]
    public void CanFindWrongIdsInTaskData()
    {
        var firstPath = "./testdata/IDList/WrongIds";
        var isoxml = ISOXML.Load(firstPath);
        Assert.AreEqual(6, isoxml.Messages.Count);
        foreach(var message in isoxml.Messages)
        {
            Assert.AreEqual(message.Code, Messaging.ResultMessageCode.WrongId);
        }
    }
}


