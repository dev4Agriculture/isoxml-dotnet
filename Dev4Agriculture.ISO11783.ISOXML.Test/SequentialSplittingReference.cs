using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dev4Agriculture.ISO11783.ISOXML.Utils;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

// Frozen correctness-fixed implementation, before batching. Keep this deliberately
// sequential: it is the independent behavioral oracle for optimization tests.
internal static class SequentialSplittingReference
{
    private static int NextIndex(ISOXML xml) => xml.TimeLogs.Keys
        .Select(name => int.TryParse(name.Substring(3), out var number) ? number : 0)
        .DefaultIfEmpty(0)
        .Max() + 1;

    internal static List<ISOTask> SplitTaskSet(
        ISOXML xml,
        Dictionary<ISOTask, List<DateTime>> timeCombos,
        bool assign = true, bool copySubElements = true)
    {
        var resultTasks = new List<ISOTask>();
        var timeLogIndex = 0;
        var splitIndex = 0;
        ISOTask oldTask = null;

        var splitPoints = timeCombos
            .SelectMany(entry => entry.Value.Distinct()
                .Select(dt => new TaskSplitEntry
                {
                    Task = entry.Key,
                    Timestamp = dt
                })
            )
            .OrderBy(entry => entry.Timestamp)
            .ToList();

        var sortedTimeLogs = xml.TimeLogs
            .Where(x => x.Value.Entries.Count > 0)
            .OrderBy(entry => entry.Value.GetStartTime())
            .ToList();

        if (splitPoints.Count == 0 || sortedTimeLogs.Count == 0)
        {
            return resultTasks;
        }

        if (splitPoints.First().Timestamp > sortedTimeLogs.Last().Value.GetEndTime())
        {
            // TODO: If the Splitting does not match the Times in our data anyhow, is it OK to just return an empty list
            return new List<ISOTask>();
        }

        // First skip all - for us - irrelevant Timelogs
        while (splitPoints[splitIndex].Timestamp > sortedTimeLogs[timeLogIndex].Value.GetEndTime())
        {
            timeLogIndex++;
        }

        oldTask = xml.Data.Task.FirstOrDefault(tsk => tsk.TimeLogs.Any(log => log.Name == sortedTimeLogs[timeLogIndex].Key));
        if (oldTask == null)
        {
            return new List<ISOTask>();
        }
        // Moving a boundary can empty an original task. Untimed allocations
        // must still use its original start, not the mutated log collection.
        var originalStartTimes = copySubElements
            ? xml.Data.Task.ToDictionary(task => task, task => task.TimeLogs
                .Where(tlg => tlg.Entries.Count > 0).Select(tlg => tlg.GetStartTime())
                .DefaultIfEmpty(DateTime.MinValue).Min())
            : null;

        while (timeLogIndex < sortedTimeLogs.Count && splitIndex < splitPoints.Count)
        {
            //In case our next splitpoint is after this TimeLogs end, we can just fully add it
            if (splitPoints[splitIndex].Timestamp > sortedTimeLogs[timeLogIndex].Value.GetEndTime())
            {
                if (oldTask != null)
                {
                    oldTask.TryAddTimeLog(sortedTimeLogs[timeLogIndex].Value);
                    if (!resultTasks.Contains(oldTask))
                    {
                        resultTasks.Add(oldTask);
                    }
                }

                timeLogIndex++;
            }
            //In this case, the Splitpoint was set outside any TimeLog or at the very beginning of the next TimeLog
            else if (splitPoints[splitIndex].Timestamp <= sortedTimeLogs[timeLogIndex].Value.GetStartTime())
            {
                oldTask = splitPoints[splitIndex].Task;
                splitIndex++;
            }
            //In this case, the Timestamp is set somewhere in the TimeLog, so we need to split it
            else if (
                splitPoints[splitIndex].Timestamp > sortedTimeLogs[timeLogIndex].Value.GetStartTime() &&
                splitPoints[splitIndex].Timestamp <= sortedTimeLogs[timeLogIndex].Value.GetEndTime()
            )
            {
                var tlgToSplit = sortedTimeLogs[timeLogIndex].Value;
                if (tlgToSplit.TryFindClosestIndex(splitPoints[splitIndex].Timestamp, out var timeIndex))
                {
                    // Between the first two samples the preceding-sample rule
                    // places the boundary at row zero; transfer the entire log.
                    if (timeIndex == 0)
                    {
                        oldTask.TryRemoveTimeLog(tlgToSplit);
                        oldTask = splitPoints[splitIndex].Task;
                        splitIndex++;
                        continue;
                    }

                    var splittedTLGs = tlgToSplit.SplitTimeLog(
                        xml.Data.Device.ToList(),
                        new List<int> { timeIndex },
                        NextIndex(xml));

                    splittedTLGs = splittedTLGs
                        .Where(tlg => tlg.Entries.Count > 0)
                        .OrderBy(entry => entry.GetStartTime())
                        .ToList();

                    if (splittedTLGs.Count > 0 && oldTask != null)
                    {
                        if (oldTask.TryAddTimeLog(splittedTLGs[0]) && !resultTasks.Contains(oldTask))
                        {
                            resultTasks.Add(oldTask);
                        }

                        oldTask.TryRemoveTimeLog(tlgToSplit);
                    }

                    if (splittedTLGs.Count > 1 && splitPoints[splitIndex].Task != null)
                    {
                        if (splitPoints[splitIndex].Task.TryAddTimeLog(splittedTLGs[1])
                            && !resultTasks.Contains(splitPoints[splitIndex].Task))
                        {
                            resultTasks.Add(splitPoints[splitIndex].Task);
                        }
                    }

                    xml.TimeLogs.Remove(tlgToSplit.Name);
                    splittedTLGs.ForEach(tlg => xml.TimeLogs.Add(tlg.Name, tlg));

                    sortedTimeLogs = xml.TimeLogs
                        .Where(tlg => tlg.Value.Entries.Count > 0)
                        .OrderBy(entry => entry.Value.GetStartTime())
                        .ToList();

                    oldTask = splitPoints[splitIndex].Task;
                    splitIndex++;
                    timeLogIndex++;
                }
                else
                {
                    Debug.WriteLine("We found a SplitPoint that is within a TimeLog but does not have a corresponding index!");
                    // TODO, this can theoritically not happen
                }
            }
        }

        while (timeLogIndex < sortedTimeLogs.Count)
        {
            if (!oldTask.TryAddTimeLog(sortedTimeLogs[timeLogIndex].Value))
            {
                Debug.WriteLine($"Could not add timeLog {timeLogIndex}");
            }

            timeLogIndex++;
        }

        if (oldTask != null && !resultTasks.Contains(oldTask))
        {
            resultTasks.Add(oldTask);
        }

        //Assign Products, Grids, etc. from old Task to new Task
        if (copySubElements)
        {
            CopySubElements(xml, resultTasks, originalStartTimes);
        }

        if (assign)
        {
            xml.Data.Task.Clear();
        }

        var enqueuer = new ISOTimeLogEnqueuer();
        foreach (var task in resultTasks)
        {
            var devices = xml.Data.Device.ToList();
            enqueuer.EnqueueTimeLogs(task.TimeLogs, devices);
            var timeList = ISOTimeListEnqueuer.EnqueueTimeElements(task.Time.ToList(), devices);
        }

        foreach (var task in resultTasks)
        {
            task.GenerateTimeElementsFromTimeLogs(xml.Data.Device.ToList(), true);
            task.GenerateDeviceAllocationsFromTimeLogs(xml.Data.Device.ToList());

            if (assign && !xml.Data.Task.Contains(task))
            {
                xml.Data.Task.Add(task);
            }
        }

        return resultTasks;
    }

    private static void CopySubElements(ISOXML xml, List<ISOTask> resultTasks, Dictionary<ISOTask, DateTime> originalStartTimes)
    {
        foreach (var tsk in xml.Data.Task)
        {
            var start = originalStartTimes[tsk];
            foreach (var cta in tsk.CommentAllocation)
            {
                var taskToUse = FindTaskThatContainsTime(
                    resultTasks,
                    cta.AllocationStamp != null ? cta.AllocationStamp.Start : start
                    );

                if (taskToUse != null && taskToUse != tsk && !taskToUse.CommentAllocation.Contains(cta))
                {
                    taskToUse.CommentAllocation.Add(cta);
                }
            }

            foreach (var pdta in tsk.ProductAllocation)
            {
                var taskToUse = FindTaskThatContainsTime(
                    resultTasks,
                    pdta.AllocationStamp != null ? pdta.AllocationStamp.Start : start
                    );

                if (taskToUse != null && taskToUse != tsk && !taskToUse.ProductAllocation.Contains(pdta))
                {
                    taskToUse.ProductAllocation.Add(pdta);
                }
            }

            foreach (var gda in tsk.GuidanceAllocation)
            {
                var taskToUse = FindTaskThatContainsTime(
                    resultTasks,
                    gda.AllocationStamp != null && gda.AllocationStamp.Count > 0 ?
                        gda.AllocationStamp.First().Start :
                        start
                    );

                if (taskToUse != null && taskToUse != tsk && !taskToUse.GuidanceAllocation.Contains(gda))
                {
                    taskToUse.GuidanceAllocation.Add(gda);
                }
            }

            foreach (var wrka in tsk.WorkerAllocation)
            {
                var taskToUse = FindTaskThatContainsTime(
                    resultTasks,
                    wrka.AllocationStamp != null ? wrka.AllocationStamp.Start : start
                    );

                if (taskToUse != null && taskToUse != tsk && !taskToUse.WorkerAllocation.Contains(wrka))
                {
                    taskToUse.WorkerAllocation.Add(wrka);
                }
            }

            if (tsk.GridSpecified && tsk.Grid.Count() > 0 && tsk.TreatmentZone != null)
            {
                var taskToUse = FindTaskThatContainsTime(
                    resultTasks,
                    start
                    );
                if (taskToUse != null && taskToUse != tsk)
                {
                    if (!taskToUse.Grid.Contains(tsk.Grid.First()))
                    {
                        taskToUse.Grid.Add(tsk.Grid.First());
                    }

                    foreach (var tzn in tsk.TreatmentZone)
                    {
                        if (!taskToUse.TreatmentZone.Contains(tzn))
                        {
                            taskToUse.TreatmentZone.Add(tzn);
                        }
                    }
                }
            }
        }
    }

    private static ISOTask FindTaskThatContainsTime(List<ISOTask> tasks, DateTime timestamp)
    {
        return tasks.FirstOrDefault(task => task.GetTaskEndTime() >= timestamp && task.GetTaskStartTime() <= timestamp);
    }
}
