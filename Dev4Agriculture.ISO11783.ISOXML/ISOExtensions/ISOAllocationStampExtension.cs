using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOAllocationStamp
    {

        public ulong GetSeconds()
        {
            if (this.Stop != null)
            {
                TimeSpan? duration = Stop - Start;
                return (ulong)duration?.TotalSeconds;
            }
            else if (Duration != null)
            {
                return Duration??0;
            } else
            {
                return 0;
            }
        }


        public bool TryGetSeconds(out ulong seconds)
        {
            if (this.Stop != null)
            {
                TimeSpan? duration = Stop - Start;
                seconds = (ulong)duration?.TotalSeconds;
                return true;
            }
            else if (Duration != null)
            {
                seconds = Duration ?? 0;
                return true;
            }
            else
            {
                seconds = 0;
                return false;
            }
        }
    }
}
