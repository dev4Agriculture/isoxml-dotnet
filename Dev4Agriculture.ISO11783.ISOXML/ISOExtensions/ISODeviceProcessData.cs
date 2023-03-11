using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public enum ISODeviceProcessDataPropertyType
    {
        None = 0,
        BelongsToDefaultSet = 1,
        Setable = 2,
        ControlSource = 4
    }

    public enum ISODeviceProcessDataTriggerMethodType
    {
        None = 0,
        OnTime = 1,
        OnDistance = 2,
        OnChange = 4,
        OnThreshold = 8,
        Total = 16
    }

    public partial class ISODeviceProcessData
    {
    }
}
