using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{
    internal class ISOTimeLogDeviceProperty : ISODeviceProperty
    {
        private ISODeviceElement _deviceElement;
        public ISODeviceElement DeviceElement
        {
            get => _deviceElement;
            set
            {
                _deviceElement = value;
                DeviceElementId = IdList.ToIntId(_deviceElement.DeviceElementId);
            }
        }

        public int DeviceElementId;


        public ISOTimeLogDeviceProperty(ISODeviceProperty property)
        {
            DevicePropertyDDI = property.DevicePropertyDDI;
            DevicePropertyObjectId = property.DevicePropertyObjectId;
            DevicePropertyDesignator = property.DevicePropertyDesignator;
            DevicePropertyValue = property.DevicePropertyValue;
            DeviceValuePresentationObjectId = property.DeviceValuePresentationObjectId;
        }
    }
}
