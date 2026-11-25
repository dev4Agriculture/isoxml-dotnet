using System.Xml.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISODataLogValue
    {
        /// <summary>
        /// Unique identifier of the device element within a device
        /// </summary>
        [XmlIgnore]
        public int? DeviceElementNumber { get; set; }
    }
}
