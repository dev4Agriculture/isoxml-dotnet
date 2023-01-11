using System;
using System.Linq;
using System.Threading;
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

    [TestMethod]
    public void UpdateTaskGuidanceAllocationAndControlAssignment()
    {
        var path = "./out/isoxmlv3/validtask";
        var taskName = "TaskGuidance";
        var isoxml = ISOXML.Create(path);

        isoxml.VersionMajor = ISO11783TaskDataFileVersionMajor.Version3;

        var task = new ISOTask()
        {
            TaskDesignator = taskName,
            TaskStatus = ISOTaskStatus.Template
        };
        task.GuidanceAllocation.Add(new ISOGuidanceAllocation() { GuidanceGroupIdRef = "test" });
        task.ControlAssignment.Add(new ISOControlAssignment());

        Assert.IsTrue(task.GuidanceAllocationSpecified);
        Assert.IsTrue(task.ControlAssignmentSpecified);

        isoxml.IdTable.AddObjectAndAssignIdIfNone(task);
        isoxml.Data.Task.Add(task);
        isoxml.Save();

        var check = ISOXML.Load(path);
        Assert.IsTrue(check != null);
        Assert.AreEqual(check.Messages.Count, 0);
        Assert.IsTrue(check.VersionMajor == ISO11783TaskDataFileVersionMajor.Version3);

        Assert.IsNotNull(check.Data.Task);
        var taskLoaded = check.Data.Task.First();
        Assert.IsFalse(taskLoaded.GuidanceAllocationSpecified);
        Assert.IsFalse(taskLoaded.ControlAssignmentSpecified);
    }

    [TestMethod]
    public void UpdateTaskTreatmentZone()
    {
        var path = "./out/isoxmlv3/validtask";
        var taskName = "TaskTreatmentZone";
        var isoxml = ISOXML.Create(path);

        isoxml.VersionMajor = ISO11783TaskDataFileVersionMajor.Version3;

        var task = new ISOTask()
        {
            TaskDesignator = taskName,
            TaskStatus = ISOTaskStatus.Template
        };

        var trZone = new ISOTreatmentZone() { TreatmentZoneDesignator = "test" };
        trZone.ProcessDataVariable.Add(new ISOProcessDataVariable() { DeviceElementIdRef = "test1", ActualCulturalPracticeValue = (long)300, ElementTypeInstanceValue = (long)222 });
        trZone.ProcessDataVariable.Add(new ISOProcessDataVariable() { DeviceElementIdRef = "test23", ActualCulturalPracticeValue = (long)3, ElementTypeInstanceValue = (long)2 });
        task.TreatmentZone.Add(trZone);

        Assert.IsTrue(task.TreatmentZoneSpecified);

        isoxml.IdTable.AddObjectAndAssignIdIfNone(task);
        isoxml.Data.Task.Add(task);
        isoxml.Save();

        var check = ISOXML.Load(path);
        Assert.IsTrue(check != null);
        Assert.IsTrue(check.VersionMajor == ISO11783TaskDataFileVersionMajor.Version3);

        Assert.IsNotNull(check.Data.Task);
        var taskLoaded = check.Data.Task.First();
        Assert.IsTrue(taskLoaded.TreatmentZoneSpecified);
        var loadedTrZone = taskLoaded.TreatmentZone.First();
        Assert.AreEqual(1, loadedTrZone.ProcessDataVariable.Count);
        Assert.IsNull(loadedTrZone.ProcessDataVariable.First().ActualCulturalPracticeValue);
        Assert.IsNull(loadedTrZone.ProcessDataVariable.First().ElementTypeInstanceValue);
    }

    [TestMethod]
    public void UpdateTaskProductAllocation()
    {
        var path = "./out/isoxmlv3/validtask";
        var taskName = "ProductAllocation";
        var isoxml = ISOXML.Create(path);

        isoxml.VersionMajor = ISO11783TaskDataFileVersionMajor.Version3;
        var startTime = new DateTime(2022, 1, 12, 10, 30, 0, DateTimeKind.Utc);

        var task = new ISOTask()
        {
            TaskDesignator = taskName,
            TaskStatus = ISOTaskStatus.Template
        };

        var productAlloc = new ISOProductAllocation()
        {
            ASP = new ISOAllocationStamp()
            {
                Start = startTime
            },
            TransferMode = ISOTransferMode.Remainder
        };
        task.ProductAllocation.Add(productAlloc);

        Assert.IsTrue(task.ProductAllocationSpecified);

        isoxml.IdTable.AddObjectAndAssignIdIfNone(task);
        isoxml.Data.Task.Add(task);
        isoxml.Save();

        var check = ISOXML.Load(path);
        Assert.IsTrue(check != null);
        Assert.IsTrue(check.VersionMajor == ISO11783TaskDataFileVersionMajor.Version3);

        Assert.IsNotNull(check.Data.Task);
        var taskLoaded = check.Data.Task.First();
        Assert.IsTrue(taskLoaded.ProductAllocationSpecified);
        var loadedPAlloc = taskLoaded.ProductAllocation.First();
        Assert.AreEqual(ISOTransferMode.Emptying, loadedPAlloc.TransferModeValue);
        Assert.AreEqual(loadedPAlloc.ASP.Start, new DateTime(startTime.Ticks, DateTimeKind.Unspecified));
    }

    [TestMethod]
    public void UpdatePartfields()
    {
        var path = "./out/isoxmlv3/validPfd";
        var isoxml = ISOXML.Create(path);

        isoxml.VersionMajor = ISO11783TaskDataFileVersionMajor.Version3;

        var partfield = new ISOPartfield()
        {
            PartfieldDesignator = "test"
        };
        partfield.GuidanceGroup.Add(new ISOGuidanceGroup());
        var ls = new ISOLineString() { LineStringType = ISOLineStringType.Obstacle };
        ls.Point.Add(new ISOPoint() { PointType = ISOPointType.Flag, PointId = "flagpointId", Filename = "flagfilenameVal" });
        ls.Point.Add(new ISOPoint() { PointType = ISOPointType.FieldAccess, PointId = "pointId", Filename = "filenameVal" });
        partfield.LineString.Add(ls);
        partfield.PolygonnonTreatmentZoneonly.Add(new ISOPolygon() { PolygonType = ISOPolygonType.Headland, PolygonId = "polygonId" });

        isoxml.IdTable.AddObjectAndAssignIdIfNone(partfield);
        isoxml.Data.Partfield.Add(partfield);
        isoxml.Save();

        var check = ISOXML.Load(path);
        Assert.IsTrue(check != null);
        Assert.IsTrue(check.VersionMajor == ISO11783TaskDataFileVersionMajor.Version3);

        Assert.IsTrue(check.Data.PartfieldSpecified);
        var pfLoaded = check.Data.Partfield.First();
        Assert.IsFalse(pfLoaded.GuidanceGroupSpecified);
        var loadedLineStr = pfLoaded.LineString.First();
        Assert.AreEqual(ISOLineStringType.Flag, loadedLineStr.LineStringType);
        Assert.AreEqual(1, loadedLineStr.Point.Count);
        Assert.IsNull(loadedLineStr.Point.First().PointId);
        Assert.IsNull(loadedLineStr.Point.First().Filename);
    }

    [TestMethod]
    public void UpdateProducts()
    {
        var path = "./out/isoxmlv3/validPdt";
        var isoxml = ISOXML.Create(path);
        isoxml.VersionMajor = ISO11783TaskDataFileVersionMajor.Version3;

        var product = new ISOProduct()
        {
            ProductDesignator = "test",
            ProductType = ISOProductType.SingleDefault,
            MixtureRecipeQuantity = 200,
            DensityMassPerVolume = 2,
            DensityMassPerCount = 1,
            DensityVolumePerCount = 200
        };
        product.ProductRelation.Add(new ISOProductRelation() { ProductIdRef = "tst" });

        isoxml.IdTable.AddObjectAndAssignIdIfNone(product);
        isoxml.Data.Product.Add(product);
        isoxml.Save();

        var check = ISOXML.Load(path);
        Assert.IsTrue(check != null);
        Assert.IsTrue(check.VersionMajor == ISO11783TaskDataFileVersionMajor.Version3);

        Assert.IsTrue(check.Data.ProductSpecified);
        var pdtLoaded = check.Data.Product.First();
        Assert.IsFalse(pdtLoaded.ProductRelationSpecified);
        Assert.IsNull(pdtLoaded.ProductType);
        Assert.IsNull(pdtLoaded.MixtureRecipeQuantity);
        Assert.IsNull(pdtLoaded.DensityMassPerVolume);
        Assert.IsNull(pdtLoaded.DensityMassPerCount);
        Assert.IsNull(pdtLoaded.DensityVolumePerCount);
    }
}
