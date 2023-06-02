using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dev4Agriculture.ISO11783.ISOXML.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class TaskAnalyzerTest
{
    [TestMethod]
    public async Task CulturalPractice_MultiTask()
    {
        var statusResult = new int[] {
            0,
            0,
            (int)CulturalPracticesType.Fertilizing,
            0,
            0,
            (int)CulturalPracticesType.SowingAndPlanting,
            (int)CulturalPracticesType.SowingAndPlanting,
            (int)CulturalPracticesType.SowingAndPlanting,
            0,
            (int)CulturalPracticesType.CropProtection,
            0,
            0,
            0,
            (int)CulturalPracticesType.SowingAndPlanting,
            0,
            (int)CulturalPracticesType.SowingAndPlanting,
            0,
            0,
            0,
            (int)CulturalPracticesType.SowingAndPlanting,
            0,
            0,
            0,
            0,
            0,
            (int)CulturalPracticesType.Fertilizing,
            (int)CulturalPracticesType.SowingAndPlanting,
         };
        var filePath = "./testdata/LoadFromStream/Another-single-ACP.zip";
        ISOXML result = null;
        using (var stream = File.OpenRead(filePath))
        {
            result = await ISOXML.LoadFromArchiveAsync(stream);
        }

        var analyzer = new ISOTaskAnalysis(result);
        for (var index = 0; index < result.Data.Task.Count; index++)
        {
            var task = result.Data.Task[index];
            if(task.TaskStatus != TaskFile.ISOTaskStatus.Planned)
            {
                var item = analyzer.GetTaskCulturalPractice(task);
                Console.WriteLine(index);
                Assert.AreEqual(statusResult[index], (int)item.CulturalPractice);
            }
        }
    }
    [TestMethod]
    public async Task CulturalPractice_SingleTask()
    {
        var filePath = "./testdata/LoadFromStream/SingleDPD.zip";
        ISOXML result = null;
        using (var stream = File.OpenRead(filePath))
        {
            result = await ISOXML.LoadFromArchiveAsync(stream);
        }

        var analyzer = new ISOTaskAnalysis(result);
        var task = result.Data.Task.First();
        var item = analyzer.GetTaskCulturalPractice(task);
        Assert.AreEqual(CulturalPracticesType.SowingAndPlanting, item.CulturalPractice);
        Assert.AreEqual("DET-452", item.DeviceElementId);
        Assert.AreEqual("DVC-36", item.DeviceId);
        Assert.AreEqual(64, (int)item.DurationInSeconds);
        Assert.AreEqual(CulturalPracticeSourceType.ClientName, item.Source);
        Assert.IsTrue(1 > Math.Abs((new DateTime(2022, 10, 19, 16, 3, 35) - item.StartDateTime).TotalSeconds));
        Assert.IsTrue(1 > Math.Abs((new DateTime(2022, 10, 19, 16, 4, 56) - item.StopDateTime).TotalSeconds));
    }
}
