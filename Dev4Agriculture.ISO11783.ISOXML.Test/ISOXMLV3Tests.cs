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
        Assert.IsNotNull(check);
        Assert.AreEqual(0, check.Messages.Count);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version3, check.VersionMajor);
    }

    [TestMethod]
    public void UpdateTaskTimeAccordingToDocs()
    {
        var path = "./out/isoxmlv3/validtask";
        CreateTaskWithTimes(path, ISO11783TaskDataFileVersionMajor.Version3, out var startTaskTime, out var stopTaskTime);

        var check = ISOXML.Load(path);
        Assert.IsNotNull(check);
        Assert.AreEqual(0, check.Messages.Count);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version3, check.VersionMajor);

        Assert.IsNotNull(check.Data.Task);
        var taskTime = check.Data.Task.FirstOrDefault()?.Time?.FirstOrDefault();
        Assert.IsNotNull(taskTime);
        Assert.AreEqual(taskTime.Start, new DateTime(startTaskTime.Ticks, DateTimeKind.Unspecified));
        Assert.AreEqual(taskTime.Stop, new DateTime(stopTaskTime.Ticks, DateTimeKind.Unspecified));
        Assert.AreEqual(taskTime.Type, ISOType2.Clearing);
    }

    [TestMethod]
    public void TaskTimeLoadsCorrectlyForV4()
    {
        var path = "./out/isoxmlv3/validtask";
        CreateTaskWithTimes(path, ISO11783TaskDataFileVersionMajor.Version4, out var startTaskTime, out var stopTaskTime);

        var check = ISOXML.Load(path);
        Assert.IsNotNull(check);
        Assert.AreEqual(0, check.Messages.Count);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version4, check.VersionMajor);

        Assert.IsNotNull(check.Data.Task);
        var taskTime = check.Data.Task.FirstOrDefault()?.Time?.FirstOrDefault();
        Assert.IsNotNull(taskTime);
        Assert.AreEqual(startTaskTime, taskTime.Start);
        Assert.AreEqual(stopTaskTime, taskTime.Stop);
        Assert.AreEqual(ISOType2.PoweredDown, taskTime.Type);
    }

    private static void CreateTaskWithTimes(string path, ISO11783TaskDataFileVersionMajor version, out DateTime startTaskTime, out DateTime stopTaskTime)
    {
        //TODO: timezones loading still to discuss
        var taskName = "Taskv3";
        var isoxml = ISOXML.Create(path);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        startTaskTime = TimeZoneInfo.ConvertTime(new DateTime(2022, 12, 25, 10, 30, 0), timeZone);
        stopTaskTime = TimeZoneInfo.ConvertTime(new DateTime(2022, 12, 25, 11, 30, 0), timeZone);
        isoxml.VersionMajor = version;

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
    }

    [TestMethod]
    public void UpdateTaskGuidanceAllocationAndControlAssignment()
    {
        var path = "./out/isoxmlv3/validtask";
        CreateTaskWithGuidanceAllocations(path, ISO11783TaskDataFileVersionMajor.Version3);

        var check = ISOXML.Load(path);
        Assert.IsNotNull(check);
        Assert.AreEqual(0, check.Messages.Count);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version3, check.VersionMajor);

        Assert.IsNotNull(check.Data.Task);
        var taskLoaded = check.Data.Task.First();
        Assert.IsFalse(taskLoaded.GuidanceAllocationSpecified);
        Assert.IsFalse(taskLoaded.ControlAssignmentSpecified);
    }

    [TestMethod]
    public void TaskGuidanceAllocationAndControlAssignmentCorrectForV4()
    {
        var path = "./out/isoxmlv3/validtask";
        CreateTaskWithGuidanceAllocations(path, ISO11783TaskDataFileVersionMajor.Version4);

        var check = ISOXML.Load(path);
        Assert.IsNotNull(check);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version4, check.VersionMajor);

        Assert.IsNotNull(check.Data.Task);
        var taskLoaded = check.Data.Task.First();
        Assert.IsTrue(taskLoaded.GuidanceAllocationSpecified);
        Assert.IsTrue(taskLoaded.ControlAssignmentSpecified);
    }

    private static void CreateTaskWithGuidanceAllocations(string path, ISO11783TaskDataFileVersionMajor version)
    {
        var taskName = "TaskGuidance";
        var isoxml = ISOXML.Create(path);

        isoxml.VersionMajor = version;

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
    }

    [TestMethod]
    public void UpdateTaskTreatmentZone()
    {
        var path = "./out/isoxmlv3/validtask";
        CreateTaskTreatmentZone(path, ISO11783TaskDataFileVersionMajor.Version3);

        var check = ISOXML.Load(path);
        Assert.IsNotNull(check);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version3, check.VersionMajor);

        Assert.IsNotNull(check.Data.Task);
        var taskLoaded = check.Data.Task.First();
        Assert.IsTrue(taskLoaded.TreatmentZoneSpecified);
        var loadedTrZone = taskLoaded.TreatmentZone.First();
        Assert.AreEqual(1, loadedTrZone.ProcessDataVariable.Count);
        Assert.IsNull(loadedTrZone.ProcessDataVariable.First().ActualCulturalPracticeValue);
        Assert.IsNull(loadedTrZone.ProcessDataVariable.First().ElementTypeInstanceValue);
    }

    [TestMethod]
    public void TaskTreatmentZone_CorrectForV4()
    {
        var path = "./out/isoxmlv3/validtask";
        CreateTaskTreatmentZone(path, ISO11783TaskDataFileVersionMajor.Version4);

        var check = ISOXML.Load(path);
        Assert.IsNotNull(check);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version4, check.VersionMajor);

        Assert.IsNotNull(check.Data.Task);
        var taskLoaded = check.Data.Task.First();
        Assert.IsTrue(taskLoaded.TreatmentZoneSpecified);
        var loadedTrZone = taskLoaded.TreatmentZone.First();
        Assert.AreEqual(2, loadedTrZone.ProcessDataVariable.Count);
        Assert.IsNotNull(loadedTrZone.ProcessDataVariable.First().ActualCulturalPracticeValue);
        Assert.IsNotNull(loadedTrZone.ProcessDataVariable.First().ElementTypeInstanceValue);
    }

    private static void CreateTaskTreatmentZone(string path, ISO11783TaskDataFileVersionMajor version)
    {
        var taskName = "TaskTreatmentZone";
        var isoxml = ISOXML.Create(path);
        isoxml.VersionMajor = version;

        var task = new ISOTask()
        {
            TaskDesignator = taskName,
            TaskStatus = ISOTaskStatus.Template
        };

        var trZone = new ISOTreatmentZone() { TreatmentZoneDesignator = "test" };
        trZone.ProcessDataVariable.Add(new ISOProcessDataVariable() { DeviceElementIdRef = "test1", ActualCulturalPracticeValue = 300, ElementTypeInstanceValue = 222 });
        trZone.ProcessDataVariable.Add(new ISOProcessDataVariable() { DeviceElementIdRef = "test23", ActualCulturalPracticeValue = 3, ElementTypeInstanceValue = 2 });
        task.TreatmentZone.Add(trZone);

        Assert.IsTrue(task.TreatmentZoneSpecified);

        isoxml.IdTable.AddObjectAndAssignIdIfNone(task);
        isoxml.Data.Task.Add(task);
        isoxml.Save();
    }

    [TestMethod]
    public void UpdateTaskProductAllocation()
    {
        var path = "./out/isoxmlv3/validtask";
        var startTime = CreatetaskProductAllocation(path, ISO11783TaskDataFileVersionMajor.Version3);

        var check = ISOXML.Load(path);
        Assert.IsNotNull(check);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version3, check.VersionMajor);

        Assert.IsNotNull(check.Data.Task);
        var taskLoaded = check.Data.Task.First();
        Assert.IsTrue(taskLoaded.ProductAllocationSpecified);
        var loadedPAlloc = taskLoaded.ProductAllocation.First();
        Assert.AreEqual(ISOTransferMode.Emptying, loadedPAlloc.TransferMode);
        Assert.AreEqual(new DateTime(startTime.Ticks, DateTimeKind.Unspecified), loadedPAlloc.ASP.Start);
    }

    [TestMethod]
    public void TaskProductAllocation_CorrectForV4()
    {
        var path = "./out/isoxmlv3/validtask";
        var startTime = CreatetaskProductAllocation(path, ISO11783TaskDataFileVersionMajor.Version4);

        var check = ISOXML.Load(path);
        Assert.IsNotNull(check);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version4, check.VersionMajor);

        Assert.IsNotNull(check.Data.Task);
        var taskLoaded = check.Data.Task.First();
        Assert.IsTrue(taskLoaded.ProductAllocationSpecified);
        var loadedPAlloc = taskLoaded.ProductAllocation.First();
        Assert.AreEqual(ISOTransferMode.Remainder, loadedPAlloc.TransferMode);
        Assert.AreEqual(startTime, loadedPAlloc.ASP.Start);
    }

    private static DateTime CreatetaskProductAllocation(string path, ISO11783TaskDataFileVersionMajor version)
    {
        var taskName = "ProductAllocation";
        var isoxml = ISOXML.Create(path);

        isoxml.VersionMajor = version;
        var startTime = new DateTime(2022, 1, 12, 10, 30, 0, DateTimeKind.Local);

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
        return startTime;
    }

    [TestMethod]
    public void UpdatePartfields()
    {
        var path = "./out/isoxmlv3/validPfd";
        CreatePartfields(path, ISO11783TaskDataFileVersionMajor.Version3);

        var check = ISOXML.Load(path);
        Assert.IsNotNull(check);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version3, check.VersionMajor);

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
    public void Partfields_CorrectForV4()
    {
        var path = "./out/isoxmlv3/validPfd";
        CreatePartfields(path, ISO11783TaskDataFileVersionMajor.Version4);

        var check = ISOXML.Load(path);
        Assert.IsNotNull(check);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version4, check.VersionMajor);

        Assert.IsTrue(check.Data.PartfieldSpecified);
        var pfLoaded = check.Data.Partfield.First();
        Assert.IsTrue(pfLoaded.GuidanceGroupSpecified);
        var loadedLineStr = pfLoaded.LineString.First();
        Assert.AreEqual(ISOLineStringType.Obstacle, loadedLineStr.LineStringType);
        Assert.AreEqual(2, loadedLineStr.Point.Count);
        Assert.IsNotNull(loadedLineStr.Point.First().PointId);
        Assert.IsNotNull(loadedLineStr.Point.First().Filename);
    }

    private static void CreatePartfields(string path, ISO11783TaskDataFileVersionMajor version)
    {
        var isoxml = ISOXML.Create(path);
        isoxml.VersionMajor = version;

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
    }

    [TestMethod]
    public void UpdateProducts()
    {
        var path = "./out/isoxmlv3/validPdt";
        CreateProducts(path, ISO11783TaskDataFileVersionMajor.Version3);

        var check = ISOXML.Load(path);
        Assert.IsNotNull(check);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version3, check.VersionMajor);

        Assert.IsTrue(check.Data.ProductSpecified);
        var pdtLoaded = check.Data.Product.First();
        Assert.IsFalse(pdtLoaded.ProductRelationSpecified);
        Assert.IsNull(pdtLoaded.ProductType);
        Assert.IsNull(pdtLoaded.MixtureRecipeQuantity);
        Assert.IsNull(pdtLoaded.DensityMassPerVolume);
        Assert.IsNull(pdtLoaded.DensityMassPerCount);
        Assert.IsNull(pdtLoaded.DensityVolumePerCount);
    }

    [TestMethod]
    public void Products_CorrectForV4()
    {
        var path = "./out/isoxmlv3/validPdt";
        CreateProducts(path, ISO11783TaskDataFileVersionMajor.Version4);

        var check = ISOXML.Load(path);
        Assert.IsNotNull(check);
        Assert.AreEqual(ISO11783TaskDataFileVersionMajor.Version4, check.VersionMajor);

        Assert.IsTrue(check.Data.ProductSpecified);
        var pdtLoaded = check.Data.Product[2];
        Assert.IsTrue(pdtLoaded.ProductRelationSpecified);
        Assert.AreEqual(ISOProductType.Mixture, pdtLoaded.ProductType);
        Assert.AreEqual(200, pdtLoaded.MixtureRecipeQuantity);
        Assert.AreEqual(2, pdtLoaded.DensityMassPerVolume);
        Assert.AreEqual(1, pdtLoaded.DensityMassPerCount);
        Assert.AreEqual(200, pdtLoaded.DensityVolumePerCount);
    }

    private static void CreateProducts(string path, ISO11783TaskDataFileVersionMajor version)
    {
        var isoxml = ISOXML.Create(path);
        isoxml.VersionMajor = version;

        var water = new ISOProduct()
        {
            ProductDesignator = "Water",
            ProductType = ISOProductType.Single
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(water);
        isoxml.Data.Product.Add(water);

        var medium = new ISOProduct()
        {
            ProductDesignator = "Korn Kali",
            ProductType = ISOProductType.Single
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(medium);
        isoxml.Data.Product.Add(medium);


        var productMix = new ISOProduct()
        {
            ProductDesignator = "TankMix",
            ProductType = ISOProductType.Mixture,
            MixtureRecipeQuantity = 200,
            DensityMassPerVolume = 2,
            DensityMassPerCount = 1,
            DensityVolumePerCount = 200
        };

        productMix.ProductRelation.Add(
                    new ISOProductRelation()
                    {
                        ProductIdRef = water.ProductId,
                        QuantityValue = 50
                    }
                  );
        productMix.ProductRelation.Add(
                    new ISOProductRelation()
                    {
                        ProductIdRef = medium.ProductId,
                        QuantityValue = 50
                    }
                  );

        isoxml.IdTable.AddObjectAndAssignIdIfNone(productMix);
        isoxml.Data.Product.Add(productMix);
        isoxml.Save();
    }
}
