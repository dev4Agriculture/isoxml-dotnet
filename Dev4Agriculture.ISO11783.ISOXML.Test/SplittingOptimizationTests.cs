using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class SplittingOptimizationTests
{
    private static readonly DateTime Start = new(2025, 6, 22, 10, 0, 0);
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };

    private static ISOXML Synthetic(int seed, out Dictionary<ISOTask, List<DateTime>> boundaries, bool overlap = false)
    {
        var random = new Random(seed);
        var xml = ISOXML.Create(Path.GetTempPath());
        var owner = new ISOTask { TaskDesignator = "original" };
        xml.IdTable.AddObjectAndAssignIdIfNone(owner);
        xml.Data.Task.Add(owner);
        var device = new ISODevice { DeviceId = "DVC1" };
        device.DeviceElement.Add(new ISODeviceElement { DeviceElementId = "DET1", DeviceElementNumber = 1 });
        device.DeviceProcessData.Add(new ISODeviceProcessData { DeviceProcessDataDDI = DDIUtils.FormatDDI(67),
            DeviceProcessDataTriggerMethods = 16, DeviceProcessDataProperty = 2 });
        xml.Data.Device.Add(device);
        for (var number = 1; number <= 3; number++)
        {
            var log = ISOTLG.Generate(number, Path.GetTempPath());
            for (ushort ddi = 67; ddi < 75; ddi++) log.Header.GetOrAddDataLogValue(ddi, 1);
            log.Header.GpsOptions.PosNorth = true;
            log.Header.GpsOptions.PosEast = true;
            log.Header.GpsOptions.PosStatus = true;
            var offset = (number - 1) * (overlap ? 25 : 40);
            for (var row = 0; row < 32; row++)
            {
                var line = new TLGDataLogLine(8) { DateTime = Start.AddSeconds(offset + row),
                    PosNorth = 500000000 + row * 100, PosEast = 300000000 + row * 100, PosStatus = 1 };
                for (var slot = 0; slot < 8; slot++)
                {
                    line.Entries[slot].IsSet = row == 0 || random.Next(4) == 0;
                    line.Entries[slot].Value = line.Entries[slot].IsSet ? row * 100 + slot : 0;
                    if (line.Entries[slot].IsSet) line.NumberOfEntries++;
                }
                log.Entries.Add(line);
            }
            owner.TryAddTimeLog(log);
            xml.TimeLogs.Add(log.Name, log);
        }
        owner.CommentAllocation.Add(new ISOCommentAllocation());
        owner.ProductAllocation.Add(new ISOProductAllocation());
        owner.WorkerAllocation.Add(new ISOWorkerAllocation());
        owner.GuidanceAllocation.Add(new ISOGuidanceAllocation());
        owner.Grid.Add(new ISOGrid());
        owner.TreatmentZone.Add(new ISOTreatmentZone());
        var targets = Enumerable.Range(0, 3).Select(i => new ISOTask { TaskDesignator = "target-" + i }).ToArray();
        if (seed % 5 == 4) targets[0] = owner;
        foreach (var target in targets.Where(target => target != owner)) xml.IdTable.AddObjectAndAssignIdIfNone(target);
        boundaries = targets.ToDictionary(task => task, _ => new List<DateTime>());
        if (seed % 4 != 3)
            boundaries[targets[0]].Add(xml.TimeLogs.Values.First().GetStartTime());
        var boundaryNumber = 0;
        foreach (var log in xml.TimeLogs.Values)
        {
            foreach (var row in new[] { 3, 7, 12, 18, 25, 31 })
            {
                var time = log.Entries[row].DateTime;
                if (seed % 3 == 1 && row != 31) time = time.AddMilliseconds(500);
                boundaries[targets[boundaryNumber++ % targets.Length]].Add(time);
            }
        }
        // Exercise same-row boundaries and gaps as well as the batching path.
        if (seed % 3 == 2)
        {
            boundaries[targets[1]].Add(xml.TimeLogs.Values.First().Entries[7].DateTime.AddMilliseconds(100));
            boundaries[targets[2]].Add(Start.AddSeconds(35));
        }
        return xml;
    }

    private static string Rows(ISOTLG log) => log.Name + ":" + JsonSerializer.Serialize(log.Entries, JsonOptions);

    private static string Snapshot(ISOXML xml, List<ISOTask> result, List<ISOTLG> sources)
    {
        var text = new StringBuilder();
        using (var writer = new StringWriter(text))
            new XmlSerializer(typeof(ISO11783TaskDataFile)).Serialize(writer, xml.Data);
        foreach (var task in result)
        {
            text.AppendLine("TASK:" + task.TaskId + ":" + task.TaskDesignator);
            foreach (var log in task.TimeLogs) text.AppendLine(Rows(log));
        }
        foreach (var log in xml.TimeLogs.Values.OrderBy(log => log.Name)) text.AppendLine("GLOBAL:" + Rows(log));
        foreach (var log in sources) text.AppendLine("SOURCE:" + Rows(log));
        return text.ToString();
    }

    [TestMethod]
    public void Taskset_ManyCuts_MatchesSequentialRowsNamesTotalsMetadataAndSourceMutations()
    {
        for (var seed = 0; seed < 12; seed++)
        {
            foreach (var assign in new[] { false, true })
            foreach (var copy in new[] { false, true })
            {
                var expected = Synthetic(seed, out var expectedBoundaries);
                var actual = Synthetic(seed, out var actualBoundaries);
                var expectedSources = expected.TimeLogs.Values.ToList();
                var actualSources = actual.TimeLogs.Values.ToList();
                var expectedResult = SequentialSplittingReference.SplitTaskSet(expected, expectedBoundaries, assign, copy);
                var actualResult = actual.SplitTaskSet(actualBoundaries, assign, copy);
                Assert.AreEqual(Snapshot(expected, expectedResult, expectedSources), Snapshot(actual, actualResult, actualSources),
                    $"seed={seed}, assign={assign}, copy={copy}");
            }
        }
    }

    [TestMethod]
    public void Taskset_OverlappingLogs_MatchesSequentialBehavior()
    {
        var expected = Synthetic(2, out var expectedBoundaries, true);
        var actual = Synthetic(2, out var actualBoundaries, true);
        var expectedSources = expected.TimeLogs.Values.ToList();
        var actualSources = actual.TimeLogs.Values.ToList();
        var expectedResult = SequentialSplittingReference.SplitTaskSet(expected, expectedBoundaries);
        var actualResult = actual.SplitTaskSet(actualBoundaries);
        Assert.AreEqual(Snapshot(expected, expectedResult, expectedSources), Snapshot(actual, actualResult, actualSources));
    }

    [DataTestMethod]
    [DataRow("autolog1.zip")]
    [DataRow("MiniTaskSetThatJustAddsAPartfield.zip")]
    public void Taskset_RealFixture_SaveReloadMatchesSequentialOutput(string fixture)
    {
        var directory = Path.Combine(Path.GetTempPath(), "isoxml-optimization-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        try
        {
            using var expectedInput = File.OpenRead(Path.Combine("testdata", "TaskSplitting", fixture));
            using var actualInput = File.OpenRead(Path.Combine("testdata", "TaskSplitting", fixture));
            var expected = ISOXML.LoadFromArchive(expectedInput);
            var actual = ISOXML.LoadFromArchive(actualInput);
            var expectedSources = expected.TimeLogs.Values.ToList();
            var actualSources = actual.TimeLogs.Values.ToList();
            Dictionary<ISOTask, List<DateTime>> Boundaries(ISOXML xml)
            {
                var targets = new[] { new ISOTask { TaskDesignator = "A" }, new ISOTask { TaskDesignator = "B" } };
                foreach (var task in targets) xml.IdTable.AddObjectAndAssignIdIfNone(task);
                var boundaries = targets.ToDictionary(task => task, _ => new List<DateTime>());
                var logs = xml.TimeLogs.Values.Where(log => log.Entries.Count > 0).OrderBy(log => log.GetStartTime()).ToList();
                boundaries[targets[0]].Add(logs[0].GetStartTime());
                var largest = logs.OrderByDescending(log => log.Entries.Count).First();
                for (var i = 1; i <= 20; i++)
                    boundaries[targets[i % 2]].Add(largest.Entries[i * (largest.Entries.Count - 1) / 21].DateTime);
                return boundaries;
            }
            var expectedResult = SequentialSplittingReference.SplitTaskSet(expected, Boundaries(expected));
            var actualResult = actual.SplitTaskSet(Boundaries(actual));
            Assert.AreEqual(Snapshot(expected, expectedResult, expectedSources), Snapshot(actual, actualResult, actualSources));
            expected.SetFolderPath(Path.Combine(directory, "expected"));
            actual.SetFolderPath(Path.Combine(directory, "actual"));
            var expectedArchive = Path.Combine(directory, "expected.zip");
            var actualArchive = Path.Combine(directory, "actual.zip");
            expected.SaveToArchive(expectedArchive);
            actual.SaveToArchive(actualArchive);
            using var expectedZip = ZipFile.OpenRead(expectedArchive);
            using var actualZip = ZipFile.OpenRead(actualArchive);
            CollectionAssert.AreEquivalent(expectedZip.Entries.Select(e => e.FullName).ToArray(), actualZip.Entries.Select(e => e.FullName).ToArray());
            foreach (var entry in expectedZip.Entries)
            {
                using var expectedBytes = new MemoryStream();
                using var actualBytes = new MemoryStream();
                using (var stream = entry.Open()) stream.CopyTo(expectedBytes);
                using (var stream = actualZip.GetEntry(entry.FullName).Open()) stream.CopyTo(actualBytes);
                CollectionAssert.AreEqual(expectedBytes.ToArray(), actualBytes.ToArray(), entry.FullName);
            }
            using var expectedReloadInput = File.OpenRead(expectedArchive);
            using var actualReloadInput = File.OpenRead(actualArchive);
            var expectedReload = ISOXML.LoadFromArchive(expectedReloadInput);
            var actualReload = ISOXML.LoadFromArchive(actualReloadInput);
            Assert.AreEqual(Snapshot(expectedReload, expectedReload.Data.Task.ToList(), new()),
                Snapshot(actualReload, actualReload.Data.Task.ToList(), new()));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
    [TestMethod]
    public void Taskset_ManyCuts_AvoidsRepeatedRowArrayAllocations()
    {
        (ISOXML Xml, Dictionary<ISOTask, List<DateTime>> Boundaries) Input()
        {
            var xml = ISOXML.Create(Path.GetTempPath());
            var owner = new ISOTask();
            var target = new ISOTask();
            xml.IdTable.AddObjectAndAssignIdIfNone(owner);
            xml.IdTable.AddObjectAndAssignIdIfNone(target);
            xml.Data.Task.Add(owner);
            var log = ISOTLG.Generate(1, Path.GetTempPath());
            for (ushort ddi = 68; ddi < 76; ddi++) log.Header.GetOrAddDataLogValue(ddi, 1);
            for (var row = 0; row < 20000; row++)
            {
                var line = new TLGDataLogLine(8) { DateTime = Start.AddSeconds(row), NumberOfEntries = 8 };
                for (var slot = 0; slot < 8; slot++) line.Entries[slot] = new() { IsSet = true, Value = row + slot };
                log.Entries.Add(line);
            }
            owner.TryAddTimeLog(log);
            xml.TimeLogs.Add(log.Name, log);
            return (xml, new() { [target] = Enumerable.Range(0, 51).Select(i => log.Entries[i * 20000 / 51].DateTime).ToList() });
        }
        var expected = Input();
        var actual = Input();
        var before = GC.GetAllocatedBytesForCurrentThread();
        var expectedResult = SequentialSplittingReference.SplitTaskSet(expected.Xml, expected.Boundaries, true, false);
        var sequentialBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        before = GC.GetAllocatedBytesForCurrentThread();
        var actualResult = actual.Xml.SplitTaskSet(actual.Boundaries, true, false);
        var optimizedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.AreEqual(expectedResult.Sum(t => t.TimeLogs.Sum(log => log.Entries.Count)),
            actualResult.Sum(t => t.TimeLogs.Sum(log => log.Entries.Count)));
        Assert.IsTrue(optimizedBytes < sequentialBytes * 0.4,
            $"Expected batching to avoid repeated copies: sequential={sequentialBytes}, optimized={optimizedBytes} bytes.");
    }

    [TestMethod]
    public void TimeElement_GpsEndpoints_MatchStableSortWithTiesUnorderedRowsAndInvalidPositions()
    {
        for (var scenario = 0; scenario < 5; scenario++)
        {
            var log = ISOTLG.Generate(1, Path.GetTempPath());
            if (scenario != 0)
            {
                var seconds = new[] { 3, 0, 2, 0, 3 };
                for (var i = 0; i < seconds.Length; i++)
                {
                    log.Entries.Add(new TLGDataLogLine(0) { DateTime = Start.AddSeconds(seconds[i]),
                        PosEast = scenario == 1 || (scenario == 2 && i % 2 == 0) ? 0 : 300 + i,
                        PosNorth = scenario == 3 ? 500 : 500 + i });
                }
            }
            var dates = log.Entries.Select(row => row.DateTime).ToList();
            var expected = new ISOTime { Type = ISOType2.Effective,
                Start = dates.Count == 0 ? DateTime.MaxValue : dates.Min(),
                Stop = dates.Count == 0 ? DateTime.MinValue : dates.Max() };
            var valid = log.Entries.Where(row => row.PosEast != 0 && row.PosNorth != 0)
                .OrderByDescending(row => row.DateTime).ToList();
            var latest = valid.FirstOrDefault();
            var oldest = valid.LastOrDefault();
            ISOPosition Position(TLGDataLogLine row) => (ISOPosition)typeof(ISOPosition)
                .GetMethod("FromTimeLogLine", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .Invoke(null, new object[] { row, log.Header });
            if (latest != null) expected.Position.Add(Position(latest));
            if (oldest != null && oldest.PosEast != latest.PosEast && oldest.PosNorth != latest.PosNorth)
                expected.Position.Add(Position(oldest));
            var actual = log.GenerateTimeElement(new List<ISODevice>());
            string Xml(ISOTime time)
            {
                using var writer = new StringWriter();
                new XmlSerializer(typeof(ISOTime)).Serialize(writer, time);
                return writer.ToString();
            }
            Assert.AreEqual(Xml(expected), Xml(actual), "GPS scenario " + scenario);
        }
    }

    [TestMethod]
    public void Taskset_BatchedRowsRemainIndependentOfOriginalSourceArrays()
    {
        var xml = Synthetic(0, out var boundaries);
        var sourceRows = xml.TimeLogs.Values.SelectMany(log => log.Entries).ToList();
        var result = xml.SplitTaskSet(boundaries, true, false);
        var outputRows = result.SelectMany(task => task.TimeLogs).SelectMany(log => log.Entries).ToList();
        foreach (var output in outputRows)
        {
            Assert.IsFalse(sourceRows.Any(source => ReferenceEquals(source, output) || ReferenceEquals(source.Entries, output.Entries)));
        }
        var sourceSnapshot = JsonSerializer.Serialize(sourceRows, JsonOptions);
        foreach (var output in outputRows) output.Entries[0].Value = -123456;
        Assert.AreEqual(sourceSnapshot, JsonSerializer.Serialize(sourceRows, JsonOptions));
    }

}
