using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISOLineString
    {

        internal void FixPointDigits()
        {
            foreach (var pnt in Point)
            {
                pnt.FixDigits();
            }

        }
    }
}
