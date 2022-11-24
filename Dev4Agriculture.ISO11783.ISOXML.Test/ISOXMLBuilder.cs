using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.Test;

public class ISOXMLBuilder
{
    public ISOXMLBuilder()
    {
        var taskSet = ISOXML.Create("./test");
        var task = new ISOTask();
        taskSet.IdTable.AddObjectAndAssignIdIfNone(task);
    }

}
