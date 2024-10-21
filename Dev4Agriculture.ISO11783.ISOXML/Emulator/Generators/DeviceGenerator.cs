using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;

namespace Dev4Agriculture.ISO11783.ISOXML.Emulator.Generators
{
    public class DeviceGenerator
    {
        private LocalizationLabel s_localizationLabel = new LocalizationLabel();
        private readonly ISOXML _isoxml;
        private readonly ISODevice _device;
        private readonly ISODeviceElement _mainDeviceElement;
        private ushort _maxDETObjectId = 0;
        private ushort _maxDPDObjectId = 100;
        private ushort _maxDPTObjectId = 1000;
        private ushort _maxDVPObjectId = 2000;

        private static long CalculateSerialNoInt(string serialNo)
        {
            var serialNoLong = 0;
            foreach (var letter in serialNo)
            {
                serialNoLong *= 10;
                if (letter >= '0' && letter <= '9')
                {
                    serialNoLong += letter - '0';
                }
                else if (letter >= 'a' && letter <= 'z')
                {
                    serialNoLong *= 10;
                    serialNoLong += 10 + letter - 'a';
                }
                else if (letter >= 'A' && letter <= 'Z')
                {
                    serialNoLong *= 10;
                    serialNoLong += 10 + letter - 'A';
                }
                else
                {
                    serialNoLong *= 10;
                    serialNoLong += letter % 99;
                }
            }

            return serialNoLong;
        }


        public void SetLocalization(string languageShorting, UnitSystem_US unitSystem, UnitSystem_No_US? unitSystemNoUs = null)
        {
            var shortingAsByte = languageShorting.Select(entry => (byte)entry).ToArray();
            s_localizationLabel = new LocalizationLabel()
            {
                LanguageShorting = shortingAsByte,
                UnitArea = unitSystemNoUs ?? (unitSystem == UnitSystem_US.US || unitSystem == UnitSystem_US.IMPERIAL ? UnitSystem_No_US.IMPERIAL : UnitSystem_No_US.METRIC),
                Reserved = 0xFF
            };
            _device.DeviceLocalizationLabel = s_localizationLabel.ToArray();
        }


        /// <summary>
        /// This DeviceGenerator can be used, if your device SerialNumber is a normal integer.
        /// The Number will be used as Attribute in the XML tree as well as a value in the ClientName
        /// </summary>
        /// <param name="isoxml">The ISOXML Object, this DeviceGenerator belongs to</param>
        /// <param name="name">The designator of the machine</param>
        /// <param name="softwareVersion">The SoftwareVersion of the machine</param>
        /// <param name="structureLabel">A 7 Byte array with Manufacturer specific StructureLabel</param>
        /// <param name="deviceClass">A deviceClass due to ISOBUS.net. Use "DeviceClass." for Enum values</param>
        /// <param name="manufacturer">The manufacturer ID of the Device, see https://isobus.net</param>
        /// <param name="serialNo">The Serial Number of your machine</param>
        public DeviceGenerator(ISOXML isoxml, string name, string softwareVersion, byte[] structureLabel, DeviceClass deviceClass, int manufacturer, long serialNo) :
            this(isoxml, name, softwareVersion, structureLabel, deviceClass, manufacturer, serialNo.ToString())
        { }


        /// <summary>
        /// This DeviceGenerator can be used, if your device SerialNumber includes letters.
        /// The String will be used in the XML tree, for the ClientName, all Letters will be replaced with a corresponding number (A/a=10,B/b=11,...)
        /// </summary>
        /// <param name="isoxml">The ISOXML Object, this DeviceGenerator belongs to</param>
        /// <param name="name">The designator of the machine</param>
        /// <param name="softwareVersion">The SoftwareVersion of the machine</param>
        /// <param name="structureLabel">A 7 Byte array with Manufacturer specific StructureLabel</param>
        /// <param name="deviceClass">A deviceClass due to ISOBUS.net. Use "DeviceClass." for Enum values</param>
        /// <param name="manufacturer">The manufacturer ID of the Device, see https://isobus.net</param>
        /// <param name="serialNo">The Serial Number of your machine as a string</param>
        public DeviceGenerator(ISOXML isoxml, string name, string softwareVersion, byte[] structureLabel, DeviceClass deviceClass, int manufacturer, string serialNo) :
            this(isoxml, name, softwareVersion, structureLabel, deviceClass, manufacturer, serialNo, CalculateSerialNoInt(serialNo))
        { }


        /// <summary>
        /// This DeviceGenerator shall be used, if there is no overlap between the SerialNumber of your machine and the Serial number of your ClientName.
        /// We advice not to use this or - if required - to define another algorithm to match the SerialNumber of ClientName and DVC
        /// </summary>
        /// <param name="isoxml"></param>
        /// <param name="name"></param>
        /// <param name="softwareVersion"></param>
        /// <param name="structureLabel"></param>
        /// <param name="deviceClass"></param>
        /// <param name="manufacturer"></param>
        /// <param name="serialNo"></param>
        /// <param name="serialNoLong"></param>
        public DeviceGenerator(ISOXML isoxml, string name, string softwareVersion, byte[] structureLabel, DeviceClass deviceClass, int manufacturer, string serialNo, long serialNoLong)
        {
            _isoxml = isoxml;
            _device = new ISODevice()
            {
                ClientNAME = new ClientName()
                {
                    DeviceClass = deviceClass,
                    DeviceClassInstance = 1,
                    EcuInstance = 2,
                    Function = 134,
                    FunctionInstance = 1,
                    IndustryGroup = 2,
                    ManufacturerCode = manufacturer,
                    SelfConfigurable = true,
                    SerialNo = serialNoLong
                }.ToArray(),
                DeviceDesignator = name,
                DeviceLocalizationLabel = s_localizationLabel.ToArray(),
                DeviceSerialNumber = serialNo,
                DeviceStructureLabel = structureLabel.ToArray(),
                DeviceSoftwareVersion = softwareVersion
            };

            var deviceId = isoxml.IdTable.AddObjectAndAssignIdIfNone(_device);
            _mainDeviceElement = new ISODeviceElement()
            {
                DeviceElementDesignator = name,
                DeviceElementObjectId = 1,
                DeviceElementType = ISODeviceElementType.device,
                DeviceElementNumber = 1,
                ParentObjectId = 0,

            };
            _isoxml.IdTable.AddObjectAndAssignIdIfNone(_mainDeviceElement);

            _device.DeviceElement.Add(_mainDeviceElement);

        }

        public string AddDeviceElement(ISODeviceElement deviceElement)
        {

            var id = _isoxml.IdTable.AddObjectAndAssignIdIfNone(deviceElement);
            _device.DeviceElement.Add(deviceElement);
            return id;
        }


        private ushort AddElement(ushort id, string detId = null)
        {
            var dor = new ISODeviceObjectReference()
            {
                DeviceObjectId = id
            };
            if (detId != null)
            {
                var det = _device.DeviceElement.FirstOrDefault(det => det.DeviceElementId == detId);
                det?.DeviceObjectReference.Add(dor);
            }
            else
            {
                _mainDeviceElement.DeviceObjectReference.Add(dor);
            }

            return id;

        }


        public ushort AddDeviceProcessData(ISODeviceProcessData processData, string detId = null, ISODeviceValuePresentation valuePresentation = null)
        {
            if (processData.DeviceProcessDataObjectId == 0)
            {
                processData.DeviceProcessDataObjectId = NextDeviceProcessDataId();
            }

            if (valuePresentation != null)
            {
                if (valuePresentation.DeviceValuePresentationObjectId == 0)
                {
                    valuePresentation.DeviceValuePresentationObjectId = NextDeviceValuePresentationId();
                }
                processData.DeviceValuePresentationObjectId = valuePresentation?.DeviceValuePresentationObjectId;
                if (!_device.DeviceValuePresentation.Any(entry => entry.DeviceValuePresentationObjectId == valuePresentation.DeviceValuePresentationObjectId))
                {
                    _device.DeviceValuePresentation.Add(valuePresentation);
                }
            }
            _device.DeviceProcessData.Add(processData);
            return AddElement(processData.DeviceProcessDataObjectId, detId);
        }


        public ushort AddDeviceProperty(ISODeviceProperty property, string detId = null)
        {
            if (property.DevicePropertyObjectId == 0)
            {
                property.DevicePropertyObjectId = NextDevicePropertyId();
            }

            _device.DeviceProperty.Add(property);
            return AddElement(property.DevicePropertyObjectId, detId);
        }


        public ushort NextDeviceProcessDataId()
        {
            _maxDPDObjectId++;
            return _maxDPDObjectId;
        }


        public ushort NextDevicePropertyId()
        {
            _maxDPTObjectId++;
            return _maxDPTObjectId;
        }

        public ushort NextDeviceValuePresentationId()
        {
            _maxDVPObjectId++;
            return _maxDVPObjectId;
        }

        public ushort NextDeviceElementId()
        {
            _maxDETObjectId++;
            return _maxDETObjectId;
        }


        public ISODevice GetDevice()
        {
            return _device;
        }

    }
}
