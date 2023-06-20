namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    /// <summary>
    /// Additional Data about a DeviceProcessData Element
    /// </summary>
    public enum ISODeviceProcessDataPropertyType
    {
        None = 0,
        BelongsToDefaultSet = 1,
        Setable = 2,
        ControlSource = 4
    }

    /// <summary>
    /// The Trigger methods that describe in which circumstances a Value can be logged
    /// </summary>
    public enum ISODeviceProcessDataTriggerMethodType
    {
        None = 0,
        OnTime = 1,
        OnDistance = 2,
        OnThreshold = 4,
        OnChange = 8,
        Total = 16
    }

    public partial class ISODeviceProcessData
    {

        /// <summary>
        /// Checks if the Totals-Trigger for the DeviceProcessData (DPD) is set
        /// </summary>
        /// <returns>True if Total</returns>
        public bool IsTotal() => 0 != (DeviceProcessDataTriggerMethods & (byte)ISODeviceProcessDataTriggerMethodType.Total);
        /// <summary>
        /// Checks if the OnChange-Trigger for the DeviceProcessData (DPD) is set
        /// </summary>
        /// <returns>True if OnChange</returns>
        public bool IsOnChange() => 0 != (DeviceProcessDataTriggerMethods & (byte)ISODeviceProcessDataTriggerMethodType.OnChange);
        /// <summary>
        /// Checks if the OnTime-Trigger for the DeviceProcessData (DPD) is set
        /// </summary>
        /// <returns>True if OnTime</returns>
        public bool IsOnTime() => 0 != (DeviceProcessDataTriggerMethods & (byte)ISODeviceProcessDataTriggerMethodType.OnTime);
        /// <summary>
        /// Checks if the OnDistance-Trigger for the DeviceProcessData (DPD) is set
        /// </summary>
        /// <returns>True if OnDistance</returns>
        public bool IsOnDistance() => 0 != (DeviceProcessDataTriggerMethods & (byte)ISODeviceProcessDataTriggerMethodType.OnDistance);
        /// <summary>
        /// Checks if the OnThreshold-Trigger for the DeviceProcessData (DPD) is set
        /// </summary>
        /// <returns>True if OnThreshold</returns>
        public bool IsOnThreshold() => 0 != (DeviceProcessDataTriggerMethods & (byte)ISODeviceProcessDataTriggerMethodType.OnThreshold);
        /// <summary>
        /// Checks if the Totals-Trigger for the DeviceProcessData (DPD) is set and the SetAble Attribute is not set => We have a lifetime Total
        /// </summary>
        /// <returns>True if LifeTimeTotal</returns>
        public bool IsLifeTimeTotal() => IsTotal() & ((DeviceProcessDataProperty & (byte)ISODeviceProcessDataPropertyType.Setable) == 0);


    }
}
