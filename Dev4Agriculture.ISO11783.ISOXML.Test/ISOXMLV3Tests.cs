using System;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class ISOXMLV3Tests
{
    [TestMethod]
    public void CanCreateSimpleV3File()
    {
        var path = "./out/isoxmlv3/valid";
        var taskName = "Taskv3";
        var isoxml = ISOXML.Create(path);
        isoxml.VersionMajor = ISO11783TaskDataFileVersionMajor.Version3;

        var task = new ISOTask()
        {
            TaskDesignator = taskName,
            TaskStatus = ISOTaskStatus.Planned
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(task);
        isoxml.Data.Task.Add(task);
        isoxml.Save();

        var check = ISOXML.Load(path);
        Assert.IsTrue(check != null);
        Assert.AreEqual(check.Messages.Count, 0);
        Assert.IsTrue(check.VersionMajor == ISO11783TaskDataFileVersionMajor.Version3);
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddLinkListForV3File()
    {
        var isoxml = ISOXML.Load("./testdata/isoxmlv3/valid");
        isoxml.VersionMajor = ISO11783TaskDataFileVersionMajor.Version3;
        Assert.ThrowsException<Exception>(() => isoxml.AddLinkList());
    }

    [TestMethod]
    public void UpdateTaskTimeAccordingToDocs()
    {
        var path = "./out/isoxmlv3/validtask";
        var taskName = "Taskv3";
        var isoxml = ISOXML.Create(path);
        var startTaskTime = new DateTime(2022, 12, 25, 10, 30, 0, DateTimeKind.Utc);
        var stopTaskTime = new DateTime(2022, 12, 25, 11, 30, 0, DateTimeKind.Utc);

        isoxml.VersionMajor = ISO11783TaskDataFileVersionMajor.Version3;

        var task = new ISOTask()
        {
            TaskDesignator = taskName,
            TaskStatus = ISOTaskStatus.Template,
        };

        task.Time.Add(new ISOTime() { Start = startTaskTime, Stop = stopTaskTime, Type = ISOType2.PoweredDown });

        Assert.AreEqual(task.Time.FirstOrDefault().Start, startTaskTime);
        Assert.AreEqual(task.Time.FirstOrDefault().Stop, stopTaskTime);

        isoxml.IdTable.AddObjectAndAssignIdIfNone(task);
        isoxml.Data.Task.Add(task);
        isoxml.Save();

        var check = ISOXML.Load(path);
        Assert.IsTrue(check != null);
        Assert.AreEqual(check.Messages.Count, 0);
        Assert.IsTrue(check.VersionMajor == ISO11783TaskDataFileVersionMajor.Version3);

        Assert.IsNotNull(check.Data.Task);
        var taskTime = check.Data.Task.FirstOrDefault()?.Time?.FirstOrDefault();
        Assert.IsNotNull(taskTime);
        Assert.AreEqual(taskTime.Start, new DateTime(startTaskTime.Ticks, DateTimeKind.Unspecified));
        Assert.AreEqual(taskTime.StopValue, new DateTime(stopTaskTime.Ticks, DateTimeKind.Unspecified));
        Assert.AreEqual(taskTime.Type, ISOType2.Clearing);
    }
}
