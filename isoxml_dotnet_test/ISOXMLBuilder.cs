using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class ISOXMLBuilder
    {
        public ISOXMLBuilder()
        {
            var taskSet = ISOXML.Create("./test");
            var task = new ISOTask();
            taskSet.IdTable.AddObjectAndAssignIdIfNone(task);
        }

    }
}
