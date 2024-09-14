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


        public void SetLocalization(string languageShorting, UnitSystem_US unitSystem, UnitSystem_No_US? unitSystemNoUs = null)
        {
            var shortingAsByte = languageShorting.Select(entry => (byte)entry).ToArray();
            s_localizationLabel = new LocalizationLabel()
            {
                LanguageShorting = shortingAsByte,
                UnitArea = unitSystemNoUs ?? (unitSystem == UnitSystem_US.US || unitSystem == UnitSystem_US.IMPERIAL ? UnitSystem_No_US.IMPERIAL : UnitSystem_No_US.METRIC),

            };
            _device.DeviceLocalizationLabel = s_localizationLabel.ToArray();
        }

        public DeviceGenerator(ISOXML isoxml, string name, string softwareVersion, byte[] structureLabel, DeviceClass deviceClass, int manufacturer, int serialNo)
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
                    SerialNo = serialNo
                }.ToArray(),
                DeviceDesignator = name,
                DeviceLocalizationLabel = s_localizationLabel.ToArray(),
                DeviceSerialNumber = serialNo.ToString(),
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
