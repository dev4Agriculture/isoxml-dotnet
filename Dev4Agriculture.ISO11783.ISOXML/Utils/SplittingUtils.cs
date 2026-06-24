using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;

namespace Dev4Agriculture.ISO11783.ISOXML.Utils
{
    public class TaskSplitEntry
    {
        public ISOTLG TimeLog;
        public int Index;
        public DateTime Timestamp;
        public ISOTask Task;
    }
}
