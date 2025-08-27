using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

[TestClass]
public class TaskSplitterTests
{
    [TestMethod]
    public void CanSplitTaskInto3Tasks()
    {
        var file = File.Open("./testdata/TaskSplitting/autolog1.zip",FileMode.Open);
        var isoxml = ISOXML.LoadFromArchive(file);
        var task1 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "Hallo"
        };
        var task2 = new ISOTask()
        {
            TaskStatus = ISOTaskStatus.Paused,
            TaskDesignator = "Welt"
        };
        isoxml.IdTable.AddObjectAndAssignIdIfNone(task1);
        isoxml.Data.Task.Add(task1);
        isoxml.IdTable.AddObjectAndAssignIdIfNone(task2);
        isoxml.Data.Task.Add(task2);
        var splitpoints = new Dictionary<ISOTask, List<DateTime>>()
        {
            {task1,[
            new DateTime(2025,06,22,10,00,00),
            new DateTime(2025,06,22,12,00,00),
            new DateTime(2025,06,22,14,00,00),
            new DateTime(2025,06,22,16,00,00),
            ] },
            {task2,[
            new DateTime(2025,06,22,11,00,00),
            new DateTime(2025,06,22,13,00,00),
            new DateTime(2025,06,22,15,00,00),
            new DateTime(2025,06,22,17,00,00)
            ] }
        };
        var task = isoxml.Data.Task.First();
        isoxml.SplitTaskAtTimeStamps(task, splitpoints);

        Assert.AreEqual(isoxml.Data.Task.Count, 3);
        isoxml.SetFolderPath("C:\\src\\dev4Agriculture\\isodotnet2\\isoxml-dotnet\\Dev4Agriculture.ISO11783.ISOXML.Test\\testdata\\TaskSplitting");
        isoxml.Save();
    }
}
