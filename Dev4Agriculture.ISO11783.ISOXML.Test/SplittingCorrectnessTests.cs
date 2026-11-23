using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class SplittingCorrectnessTests
{
    private static readonly DateTime Start = new(2025, 6, 22, 10, 0, 0);

    private static ISOTLG Log(int number = 1, int offset = 0, int count = 10)
    {
        var log = ISOTLG.Generate(number, Path.GetTempPath());
        log.Header.GetOrAddDataLogValue(68, 1);
        for (var i = offset; i < offset + count; i++)
        {
            var line = new TLGDataLogLine(1) { DateTime = Start.AddSeconds(i), NumberOfEntries = 1 };
            line.Entries[0] = new TLGDataLogEntry { IsSet = true, Value = i };
            log.Entries.Add(line);
        }
        return log;
    }

    private static (ISOXML Xml, ISOTask Owner) Root()
    {
        var xml = ISOXML.Create(Path.GetTempPath());
        var owner = new ISOTask();
        xml.IdTable.AddObjectAndAssignIdIfNone(owner);
        var log = Log();
        owner.TryAddTimeLog(log);
        xml.Data.Task.Add(owner);
        xml.TimeLogs.Add(log.Name, log);
        return (xml, owner);
    }

    private static int[] Values(IEnumerable<ISOTLG> logs) => logs.SelectMany(l => l.Entries)
        .Select(l => l.Entries[0].Value).ToArray();

    [TestMethod]
    public void ClosestIndex_ExactRowsAndBetweenRows_PreservesPrecedingSampleConvention()
    {
        var log = Log();
        for (var i = 0; i < log.Entries.Count; i++)
        {
            Assert.IsTrue(log.TryFindClosestIndex(log.Entries[i].DateTime, out var exact));
            Assert.AreEqual(i, exact);
            if (i + 1 < log.Entries.Count)
            {
                Assert.IsTrue(log.TryFindClosestIndex(log.Entries[i].DateTime.AddMilliseconds(500), out var between));
                Assert.AreEqual(i, between);
            }
        }
        Assert.IsFalse(log.TryFindClosestIndex(Start.AddDays(-1), out _));
        Assert.IsFalse(log.TryFindClosestIndex(Start.AddDays(1), out _));
    }

    [TestMethod]
    public void SplitTimeLog_DuplicateCutsAndEndpoints_ConservesEveryRowWithoutEmptyFragments()
    {
        var cuts = new List<int> { 4, 2, 2, 0, 10 };
        var original = cuts.ToArray();
        var result = Log().SplitTimeLog(new(), cuts, 20);
        CollectionAssert.AreEqual(new[] { 2, 2, 6 }, result.Select(l => l.Entries.Count).ToArray());
        CollectionAssert.AreEqual(Enumerable.Range(0, 10).ToArray(), Values(result));
        CollectionAssert.AreEqual(original, cuts.ToArray(), "Do not reorder the caller's indices.");
        CollectionAssert.AreEqual(new[] { "TLG00020", "TLG00021", "TLG00022" }, result.Select(l => l.Name).ToArray());
    }

    [TestMethod]
    public void SplitTimeLog_InvalidCuts_AreRejectedBeforeSparseSourceMutation()
    {
        foreach (var invalid in new[] { -1, 11 })
        {
            var log = Log();
            log.Entries[2].Entries[0].IsSet = false;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => log.SplitTimeLog(new(), new() { 2, invalid }, 2));
            Assert.IsFalse(log.Entries[2].Entries[0].IsSet);
        }
    }

    [TestMethod]
    public void SplitTimeLog_SparseBoundaryStillMutatesSourceAndCarriesForward()
    {
        var log = Log();
        log.Entries[2].Entries[0].IsSet = false;
        var result = log.SplitTimeLog(new(), new() { 2 }, 2);
        Assert.IsTrue(log.Entries[2].Entries[0].IsSet);
        Assert.AreEqual(1, log.Entries[2].Entries[0].Value);
        Assert.IsTrue(result[1].Entries[0].Entries[0].IsSet);
        Assert.AreEqual(1, result[1].Entries[0].Entries[0].Value);
    }

    [TestMethod]
    public void TaskDictionary_MultipleLogs_AssignsToLatestBoundaryAndUsesUniqueNames()
    {
        var owner = new ISOTask();
        var first = Log(1, 0, 5);
        var second = Log(2, 5, 5);
        owner.TryAddTimeLog(first);
        owner.TryAddTimeLog(second);
        var a = new ISOTask();
        var b = new ISOTask();
        var result = owner.SplitAtDateTimes(new Dictionary<ISOTask, List<DateTime>>
        {
            [a] = new() { first.Entries[0].DateTime },
            [b] = new() { second.Entries[3].DateTime }
        }, new(), 3);
        CollectionAssert.AreEqual(Enumerable.Range(0, 8).ToArray(), Values(a.TimeLogs));
        CollectionAssert.AreEqual(new[] { 8, 9 }, Values(b.TimeLogs));
        var logs = result.SelectMany(t => t.TimeLogs).ToList();
        Assert.AreEqual(logs.Count, logs.Select(l => l.Name).Distinct().Count());
    }

    [TestMethod]
    public void TaskList_UntouchedLogNameCannotCollideWithGeneratedName()
    {
        var owner = new ISOTask();
        var first = Log(1, 0, 5);
        owner.TryAddTimeLog(first);
        owner.TryAddTimeLog(Log(2, 5, 5));
        var result = owner.SplitAtDateTimes(new List<DateTime> { first.Entries[2].DateTime }, new(), 2);
        Assert.AreEqual(result.Count, result.Select(l => l.Name).Distinct().Count());
        CollectionAssert.AreEqual(Enumerable.Range(0, 10).ToArray(), Values(result));
    }

    [TestMethod]
    public void TaskDictionary_EmptyOrUnmatchedAssignments_ReturnEmptyWithoutRemovingSourceLogs()
    {
        var owner = new ISOTask();
        owner.TryAddTimeLog(Log());
        owner.TryAddTimeLog(ISOTLG.Generate(2, Path.GetTempPath()));
        Assert.AreEqual(0, owner.SplitAtDateTimes(new Dictionary<ISOTask, List<DateTime>>(), new()).Count);
        Assert.AreEqual(0, owner.SplitAtDateTimes(new Dictionary<ISOTask, List<DateTime>>
            { [new ISOTask()] = new() { Start.AddDays(1) } }, new()).Count);
        Assert.AreEqual(2, owner.TimeLogs.Count);
    }

    [TestMethod]
    public void Taskset_EmptyInputOrEmptyLogs_IsNoOp()
    {
        var (xml, owner) = Root();
        Assert.AreEqual(0, xml.SplitTaskSet(new()).Count);
        Assert.AreSame(owner, xml.Data.Task.Single());
        Assert.AreEqual(0, ISOXML.Create(Path.GetTempPath()).SplitTaskSet(new()
            { [new ISOTask()] = new() { Start } }).Count);
    }

    [TestMethod]
    public void Taskset_AssignFalse_PreservesRootTaskMembership()
    {
        var (xml, owner) = Root();
        var destination = new ISOTask();
        var result = xml.SplitTaskSet(new() { [destination] = new() { owner.TimeLogs[0].Entries[0].DateTime } }, false, false);
        Assert.AreSame(owner, xml.Data.Task.Single());
        Assert.AreSame(destination, result.Single());
    }

    [TestMethod]
    public void Taskset_ExactFinalTimestamp_SplitsFinalRowIntoDestination()
    {
        var (xml, owner) = Root();
        var destination = new ISOTask();
        var result = xml.SplitTaskSet(new() { [destination] = new() { owner.TimeLogs[0].Entries[9].DateTime } }, true, false);
        CollectionAssert.AreEqual(Enumerable.Range(0, 9).ToArray(), Values(owner.TimeLogs));
        CollectionAssert.AreEqual(new[] { 9 }, Values(destination.TimeLogs));
        CollectionAssert.AreEqual(Enumerable.Range(0, 10).ToArray(), Values(result.SelectMany(t => t.TimeLogs)));
    }

    [TestMethod]
    public void Taskset_BoundaryBetweenFirstRows_DoesNotDuplicateTheWholeLog()
    {
        var (xml, owner) = Root();
        var destination = new ISOTask();
        var result = xml.SplitTaskSet(new() { [destination] = new() { owner.TimeLogs[0].Entries[0].DateTime.AddMilliseconds(500) } }, true, false);
        CollectionAssert.AreEqual(Enumerable.Range(0, 10).ToArray(), Values(result.SelectMany(t => t.TimeLogs)));
        Assert.AreSame(destination, result.Single());
    }

    [TestMethod]
    public void Taskset_CopyAllocationsToOriginalDestination_DoesNotDuplicateOrThrow()
    {
        var (xml, owner) = Root();
        var comment = new ISOCommentAllocation { AllocationStamp = new ISOAllocationStamp { Start = owner.TimeLogs[0].Entries[0].DateTime } };
        owner.CommentAllocation.Add(comment);
        var destination = new ISOTask();
        xml.SplitTaskSet(new() { [destination] = new() { owner.TimeLogs[0].Entries[5].DateTime } });
        Assert.AreSame(comment, owner.CommentAllocation.Single());
        Assert.AreEqual(0, destination.CommentAllocation.Count);
    }

    [TestMethod]
    public void Wrapper_KeepsOriginalTaskForCallerCompatibility()
    {
        var (xml, owner) = Root();
        var destination = new ISOTask();
        xml.SplitTaskAtTimeStamps(owner, new() { [destination] = new() { owner.TimeLogs[0].Entries[0].DateTime } });
        Assert.IsTrue(xml.Data.Task.Contains(owner));
        Assert.IsTrue(xml.Data.Task.Contains(destination));
    }

    [TestMethod]
    public void TaskDictionary_BetweenSamples_UsesTheSameBoundaryAsTheActualCut()
    {
        var owner = new ISOTask();
        var log = Log();
        owner.TryAddTimeLog(log);
        var a = new ISOTask();
        var b = new ISOTask();
        owner.SplitAtDateTimes(new Dictionary<ISOTask, List<DateTime>>
        {
            [a] = new() { log.Entries[0].DateTime },
            [b] = new() { log.Entries[5].DateTime.AddMilliseconds(500) }
        }, new(), 2);
        CollectionAssert.AreEqual(Enumerable.Range(0, 5).ToArray(), Values(a.TimeLogs));
        CollectionAssert.AreEqual(Enumerable.Range(5, 5).ToArray(), Values(b.TimeLogs));
    }

    [TestMethod]
    public void ClosestIndex_DuplicateTimestamp_ChoosesFirstMatchingRow()
    {
        var log = Log();
        log.Entries[4].Date = log.Entries[3].Date;
        log.Entries[4].Time = log.Entries[3].Time;
        Assert.IsTrue(log.TryFindClosestIndex(log.Entries[3].DateTime, out var index));
        Assert.AreEqual(3, index);
    }

    [TestMethod]
    public void Taskset_CopyGridWithoutMatchingDestination_DoesNotThrowOrAttachToAnotherTask()
    {
        var (xml, owner) = Root();
        var grid = new ISOGrid();
        var zone = new ISOTreatmentZone();
        owner.Grid.Add(grid);
        owner.TreatmentZone.Add(zone);
        var laterOwner = new ISOTask();
        var laterLog = Log(2, 20, 10);
        laterOwner.TryAddTimeLog(laterLog);
        xml.Data.Task.Add(laterOwner);
        xml.TimeLogs.Add(laterLog.Name, laterLog);
        var destination = new ISOTask();
        xml.SplitTaskSet(new() { [destination] = new() { laterLog.GetStartTime() } });
        Assert.AreEqual(0, destination.Grid.Count);
        Assert.AreEqual(0, destination.TreatmentZone.Count);
        Assert.AreSame(grid, owner.Grid.Single());
        Assert.AreSame(zone, owner.TreatmentZone.Single());
    }

    [TestMethod]
    public void Taskset_CopyMetadata_PreservesAllAllocationKindsAndGridReferences()
    {
        var (xml, owner) = Root();
        var comment = new ISOCommentAllocation();
        var product = new ISOProductAllocation();
        var guidance = new ISOGuidanceAllocation();
        var worker = new ISOWorkerAllocation();
        var grid = new ISOGrid();
        var zone = new ISOTreatmentZone();
        owner.CommentAllocation.Add(comment);
        owner.ProductAllocation.Add(product);
        owner.GuidanceAllocation.Add(guidance);
        owner.WorkerAllocation.Add(worker);
        owner.Grid.Add(grid);
        owner.TreatmentZone.Add(zone);
        var destination = new ISOTask();
        xml.SplitTaskSet(new() { [destination] = new() { owner.TimeLogs[0].GetStartTime() } });
        Assert.AreSame(comment, destination.CommentAllocation.Single());
        Assert.AreSame(product, destination.ProductAllocation.Single());
        Assert.AreSame(guidance, destination.GuidanceAllocation.Single());
        Assert.AreSame(worker, destination.WorkerAllocation.Single());
        Assert.AreSame(grid, destination.Grid.Single());
        Assert.AreSame(zone, destination.TreatmentZone.Single());
    }

    [TestMethod]
    public void Taskset_RepeatedDestinationsAndDuplicateTimes_ConservesRows()
    {
        var (xml, owner) = Root();
        var log = owner.TimeLogs[0];
        var a = new ISOTask();
        var b = new ISOTask();
        var result = xml.SplitTaskSet(new()
        {
            [a] = new() { log.Entries[0].DateTime, log.Entries[6].DateTime, log.Entries[6].DateTime },
            [b] = new() { log.Entries[3].DateTime, log.Entries[9].DateTime }
        }, true, false);
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 6, 7, 8 }, Values(a.TimeLogs));
        CollectionAssert.AreEqual(new[] { 3, 4, 5, 9 }, Values(b.TimeLogs));
        CollectionAssert.AreEquivalent(Enumerable.Range(0, 10).ToArray(), Values(result.SelectMany(t => t.TimeLogs)));
    }

    [TestMethod]
    public void Totals_ShortRows_DoNotReadBeyondTheRowArray()
    {
        var log = TotalLog(1, new ushort[] { 68, 67 }, new[] { 99 });
        new ISOTimeLogSingulator().SingulateTimeLog(log, new() { TotalDevice() });
        new ISOTimeLogEnqueuer().EnqueueTimeLogs(new() { log }, new() { TotalDevice() });
        Assert.AreEqual(99, log.Entries[0].Entries[0].Value);
        var weighted = new Dev4Agriculture.ISO11783.ISOXML.DDI.DDIFunctions.WeightedAverageDDIFunctions(
            7, 1, TotalDevice(), new()) { RelevantWeightIndex = 1 };
        weighted.UpdateTimeLogEnqueuerWithDataLine(log.Entries[0]);
    }

    [TestMethod]
    public void TaskDictionary_BoundariesBeforeLogsAndInGaps_RemainEffective()
    {
        var owner = new ISOTask();
        owner.TryAddTimeLog(Log(1, 0, 5));
        owner.TryAddTimeLog(Log(2, 10, 5));
        var a = new ISOTask();
        var b = new ISOTask();
        owner.SplitAtDateTimes(new Dictionary<ISOTask, List<DateTime>>
        {
            [a] = new() { Start.AddSeconds(-1) },
            [b] = new() { Start.AddSeconds(8) }
        }, new(), 3);
        CollectionAssert.AreEqual(Enumerable.Range(0, 5).ToArray(), Values(a.TimeLogs));
        CollectionAssert.AreEqual(Enumerable.Range(10, 5).ToArray(), Values(b.TimeLogs));
    }

    [TestMethod]
    public void Taskset_BoundaryResolvingToRowZero_PreservesUntimedMetadata()
    {
        var (xml, owner) = Root();
        var product = new ISOProductAllocation();
        owner.ProductAllocation.Add(product);
        var destination = new ISOTask();
        xml.SplitTaskSet(new() { [destination] = new() { owner.TimeLogs[0].GetStartTime().AddMilliseconds(500) } });
        Assert.AreSame(product, destination.ProductAllocation.Single());
    }

    private static ISODevice TotalDevice()
    {
        var device = new ISODevice { DeviceId = "DVC1" };
        device.DeviceElement.Add(new ISODeviceElement { DeviceElementId = "DET1", DeviceElementNumber = 1 });
        device.DeviceProcessData.Add(new ISODeviceProcessData { DeviceProcessDataDDI = DDIUtils.FormatDDI(67),
            DeviceProcessDataTriggerMethods = 16, DeviceProcessDataProperty = 2 });
        return device;
    }

    private static ISOTLG TotalLog(int number, ushort[] ddis, int[] values)
    {
        var log = ISOTLG.Generate(number, Path.GetTempPath());
        foreach (var ddi in ddis) log.Header.GetOrAddDataLogValue(ddi, 1);
        var line = new TLGDataLogLine((byte)values.Length) { DateTime = Start.AddSeconds(number), NumberOfEntries = (byte)values.Length };
        for (var i = 0; i < values.Length; i++) line.Entries[i] = new TLGDataLogEntry { IsSet = true, Value = values[i] };
        log.Entries.Add(line);
        var last = new TLGDataLogLine(line) { DateTime = line.DateTime.AddMilliseconds(500) };
        for (var i = 0; i < ddis.Length; i++)
            if (i < last.Entries.Length && ddis[i] == 67) last.Entries[i].Value += 5;
        log.Entries.Add(last);
        return log;
    }

    [TestMethod]
    public void Totals_ChangingHeaderOrderAndMissingTotal_PreservesOtherValuesAndContinuity()
    {
        var first = TotalLog(1, new ushort[] { 68, 67 }, new[] { 99, 10 });
        var missing = TotalLog(2, new ushort[] { 68 }, new[] { 123 });
        var reordered = TotalLog(3, new ushort[] { 67, 68 }, new[] { 20, 456 });
        var shorter = TotalLog(4, new ushort[] { 67 }, new[] { 30 });
        new ISOTimeLogEnqueuer().EnqueueTimeLogs(new() { first, missing, reordered, shorter }, new() { TotalDevice() });
        Assert.AreEqual(123, missing.Entries[0].Entries[0].Value);
        Assert.AreEqual(456, reordered.Entries[0].Entries[1].Value);
        Assert.AreEqual(5, reordered.Entries[0].Entries[0].Value);
        Assert.AreEqual(10, shorter.Entries[0].Entries[0].Value);
        Assert.AreEqual(15, shorter.Entries[1].Entries[0].Value);
    }
}
