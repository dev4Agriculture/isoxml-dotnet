using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOAllocationStamp
    {

        public ulong GetSeconds()
        {
            if (Stop != null)
            {
                var duration = Stop - Start;
                return (ulong)duration?.TotalSeconds;
            }
            else if (Duration != null)
            {
                return Duration ?? 0;
            } else
            {
                return 0;
            }
        }


        public bool TryGetSeconds(out ulong seconds)
        {
            if (Stop != null)
            {
                var duration = Stop - Start;
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

        public DateTime? GetStopTime()
        {
            if (Stop != null)
            {
                return Stop;
            }
            else
            {
                return Start.AddSeconds(Duration ?? 0);
            }
        }

        public bool TryGetStopTime(out DateTime stop)
        {
            stop = (DateTime)(GetStopTime() ?? null);
            return stop != null;
        }

        internal void FixPositionDigits()
        {
            if (Position != null)
            {
                Position.ToList().ForEach( ptn => ptn.FixDigits());
            }
        }
    }
}
