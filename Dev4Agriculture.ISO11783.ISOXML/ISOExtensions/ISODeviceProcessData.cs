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

        public bool IsTotal() => 0 != (DeviceProcessDataTriggerMethods & (byte)ISODeviceProcessDataTriggerMethodType.Total);
        public bool IsOnChange() => 0 != (DeviceProcessDataTriggerMethods & (byte)ISODeviceProcessDataTriggerMethodType.OnChange);
        public bool IsOnTime() => 0 != (DeviceProcessDataTriggerMethods & (byte)ISODeviceProcessDataTriggerMethodType.OnTime);
        public bool IsOnDistance() => 0 != (DeviceProcessDataTriggerMethods & (byte)ISODeviceProcessDataTriggerMethodType.OnDistance);
        public bool IsOnThreshold() => 0 != (DeviceProcessDataTriggerMethods & (byte)ISODeviceProcessDataTriggerMethodType.OnThreshold);
        public bool IsLifeTimeTotal() => IsTotal() & ((DeviceProcessDataProperty & (byte)ISODeviceProcessDataPropertyType.Setable) == 0);


    }
}
