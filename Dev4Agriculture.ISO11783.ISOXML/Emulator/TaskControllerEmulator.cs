using System;
using System.Collections.Generic;
using System.Linq;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.Emulator.Generators;
using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.TimeLog;
using Dev4Agriculture.ISO11783.ISOXML.Utils;



namespace Dev4Agriculture.ISO11783.ISOXML.Emulator
{
    public enum WorkSessionProcessDataType
    {
        Current,
        Total,
        LifeTime
    };

    public class WorkSessionProcessData
    {
        public ushort DDI;
        public short DET;
        public ISODevice Device;
        public WorkSessionProcessDataType Type;
        public bool Used;
        public int LastValue;
    }


    public class TaskControllerEmulator
    {

        private readonly ISOXML _isoxml;
        private ISOTask _currentTask;
        private DateTime _startTime;
        private ISOTime _currentTime;
        private List<ISODeviceAllocation> _currentDeviceAllocations;
        private ISOTLG _currentTimeLog;
        private readonly bool _allowAutoLog;
        private byte _currentMaxDPDCount;
        private readonly List<ISODevice> _connectedDevices;
        private TLGDataLogLine _currentDataLine;
        private readonly List<WorkSessionProcessData> _latestDataLogValues;
        private bool _isFirstLine;
        private DeviceGenerator _deviceGenerator;
        private string _languageShorting;
        private UnitSystem_US _unitSystem;
        private UnitSystem_No_US? _unitSystemNoUs;
        private ISOTime _pauseElement;

        public TaskControllerEmulator(ISOXML isoxml, bool allowAutoLog = true)
        {
            _isoxml = isoxml;
            _allowAutoLog = allowAutoLog;
            _latestDataLogValues = new List<WorkSessionProcessData>();
            _connectedDevices = new List<ISODevice>();
        }


        public static TaskControllerEmulator Generate(string exportPath, string tcManufacturer, ISO11783TaskDataFileVersionMajor versionMajor, ISO11783TaskDataFileVersionMinor versionMinor, string tcVersion, bool allowAutoLogging = true)
        {
            var isoxml = ISOXML.Create(exportPath);
            isoxml.TaskControllerManufacturer = tcManufacturer;
            isoxml.TaskControllerVersion = tcVersion;
            isoxml.VersionMajor = versionMajor;
            isoxml.VersionMinor = versionMinor;
            isoxml.DataTransferOrigin = ISO11783TaskDataFileDataTransferOrigin.MICS;
            return new TaskControllerEmulator(isoxml, allowAutoLogging);

        }

        public void ImportISOXML(ISOXML isoxml)
        {
            isoxml.TaskControllerManufacturer = _isoxml.TaskControllerManufacturer;
            isoxml.TaskControllerVersion = _isoxml.TaskControllerVersion;
            isoxml.DataTransferOrigin = _isoxml.DataTransferOrigin;
            isoxml.SetFolderPath(_isoxml.FolderPath);
            foreach (var device in _isoxml.Data.Device)
            {
                if (!isoxml.Data.Device.Any( dvc =>
                        dvc.ClientNAME.Equals(device.ClientNAME) &&
                        dvc.DeviceStructureLabel.SequenceEqual(device.DeviceStructureLabel) &&
                        dvc.DeviceSoftwareVersion.Equals(device.DeviceSoftwareVersion)
                ))
                {
                    isoxml.Data.Device.Add(device);
                }
            }
        }


        public void SetLocalization(string languageShorting, UnitSystem_US unitSystem, UnitSystem_No_US? unitSystemNoUs = null)
        {
            _languageShorting = languageShorting;
            _unitSystem = unitSystem;
            _unitSystemNoUs = unitSystemNoUs;
        }


        public DeviceGenerator NewDeviceGenerator(string name, string softwareVersion, byte[] structureLabel, DeviceClass deviceClass, int manufacturer, int serialNo)
        {
            var generator = new DeviceGenerator(_isoxml, name, softwareVersion, structureLabel, deviceClass, manufacturer, serialNo);
            generator.SetLocalization(_languageShorting, _unitSystem, _unitSystemNoUs);
            _isoxml.Data.Device.Add(generator.GetDevice());
            return generator;
        }

        private void AddTimeLog()
        {
            _currentTimeLog = ISOTLG.Generate(_isoxml.TimeLogs.Count, _isoxml.FolderPath);
            _currentTask.TimeLogs.Add(_currentTimeLog);
            _currentTask.TimeLog.Add(new ISOTimeLog()
            {
                Filename = _currentTimeLog.Name,
                TimeLogType = ISOTimeLogType.Binarytimelogfiletype1
            });
            _isoxml.TimeLogs.Add(_currentTimeLog.Name, _currentTimeLog);
        }

        public ISOXML GetTaskDataSet()
        {
            return _isoxml;
        }

        public void ConnectDevice(ISODevice device)
        {
            _connectedDevices.Add(device);
            _currentMaxDPDCount = (byte)_connectedDevices.Sum(dvc => dvc.DeviceProcessData.Count());
            //TODO if we have more than 255DDIs, we need a second TimeLog

            if (_currentTask != null)
            {
                AddTimeLog();
            }

            if (!_isoxml.Data.Device.Any(dvc => dvc.ClientNAME.Equals(device.ClientNAME) &&
                                                dvc.DeviceSoftwareVersion.Equals(device.DeviceSoftwareVersion) &&
                                                dvc.DeviceSerialNumber.Equals(device.DeviceSerialNumber) &&
                                                dvc.DeviceStructureLabel.SequenceEqual(device.DeviceStructureLabel)))
            {
                _isoxml.Data.Device.Add(device);
            }

        }

        public void DisconnectDevice(ISODevice device)
        {
            _connectedDevices.Remove(device);
            _currentMaxDPDCount = (byte)_connectedDevices.Sum(dvc => dvc.DeviceProcessData.Count());
            //TODO if we have more than 255DDIs, we need a second TimeLog

            if (_currentTask != null)
            {
                AddTimeLog();
            }
        }


        private void StartAutoLog()
        {
            ISOTask autoLogTask = null;
            foreach (var task in _isoxml.Data.Task)
            {
                if (task.TaskDesignator.Equals("AUTOLOG"))
                {
                    autoLogTask = task;
                    break;
                }
            }
            if (autoLogTask == null)
            {
                autoLogTask = new ISOTask()
                {
                    TaskDesignator = "AUTOLOG",
                    TaskStatus = ISOTaskStatus.Running
                };
                autoLogTask.AddDefaultDataLogTrigger();
                _isoxml.IdTable.AddObjectAndAssignIdIfNone(autoLogTask);
                _isoxml.Data.Task.Add(autoLogTask);

            }
            _currentTask = autoLogTask;
        }

        public void StartTask(DateTime timestamp, ISOTask task = null)
        {
            if (_currentTask != null)
            {
                EndTask(ISOTaskStatus.Paused);
            } else if (_pauseElement != null)
            {
                _pauseElement.Stop = timestamp;
                _pauseElement = null;
            }
            _currentDeviceAllocations = new List<ISODeviceAllocation>();
            _currentMaxDPDCount = (byte)_connectedDevices.Sum(dvc => dvc.DeviceProcessData.Count);

            _currentTask = task;
            if (_currentTask == null)
            {
                if (_allowAutoLog)
                {
                    StartAutoLog();
                }
                else
                {
                    throw new NoTaskInWorkSessionException();
                }
            }
            _startTime = timestamp;
            if (_currentTask.Time.Any(entry => entry.Type == ISOType2.Effective))
            {
                UpdateLatestDataLogValuesFromTimeElement(_currentTask.Time.First(entry => entry.Type == ISOType2.Effective));
            }
            _currentTime = new ISOTime()
            {
                Start = _startTime,
                Type = ISOType2.Effective
            };
            _currentTask.Time.Add(_currentTime);
            AddTimeLog();
            _isFirstLine = true;
        }

        private void UpdateLatestDataLogValuesFromTimeElement(ISOTime iSOTime)
        {
            foreach (var dlv in iSOTime.DataLogValue)
            {
                var dlvToUpdate = _latestDataLogValues.FirstOrDefault(entry => entry.DDI == DDIUtils.ConvertDDI(dlv.ProcessDataDDI) && dlv.DeviceElementIdRef == "DET" + entry.DET);
                if (dlvToUpdate != null)
                {
                    dlvToUpdate.LastValue = (int)dlv.ProcessDataValue;
                }
            }
        }

        public void StartPause(DateTime startTime, ISOType2 reason = ISOType2.Ineffective)
        {
            //Finish Worksession
            if (_currentTimeLog != null)
            {
                if (_currentDataLine != null)
                {
                    _currentTimeLog.Entries.Add(_currentDataLine);
                }
            }

            _startTime = startTime;
        }


        public void AddTimeAndPosition(DateTime timestamp, ISOPosition position)
        {
            if (_currentTask == null)
            {
                if (_allowAutoLog)
                {
                    StartAutoLog();
                }
                else
                {
                    throw new NoTaskInWorkSessionException();
                }
            }

            if (_currentDataLine != null)
            {
                _currentTimeLog.Entries.Add(_currentDataLine);
            }

            _currentDataLine = new TLGDataLogLine(_currentMaxDPDCount)
            {
                Date = DateUtilities.GetDaysSince1980(timestamp),
                Time = DateUtilities.GetMilliSecondsInDay(timestamp),
                PosNorth = (int)((double)position.PositionNorth * ISOTLG.TLG_GPS_FACTOR),
                PosEast = (int)((double)position.PositionEast * ISOTLG.TLG_GPS_FACTOR),
                PosStatus = (byte)position.PositionStatus,
                ArraySize = 0,
                GpsUTCDate = position.GpsUtcDate ?? 0,
                GpsUTCTime = (uint)(position.GpsUtcTime ?? 0),
                Hdop = (ushort)(position.HDOP ?? 0), //TODO Ensure that's correct
                Pdop = (ushort)(position.PDOP ?? 0),
                NumberOfSatellites = position.NumberOfSatellites ?? 0,
                NumberOfEntries = 0
            };

            if (timestamp.CompareTo(_currentTime.Start) < 0)
            {
                _currentTime.Start = timestamp;
            }

            if (timestamp.CompareTo(_currentTime.Stop) > 0)
            {
                _currentTime.Stop = timestamp;
            }

            foreach (var dan in _currentDeviceAllocations)
            {
                if (timestamp.CompareTo(dan.AllocationStamp.Start) < 0)
                {
                    dan.AllocationStamp.Start = timestamp;
                }
                if (timestamp.CompareTo(dan.AllocationStamp.Stop) > 0)
                {
                    dan.AllocationStamp.Stop = timestamp;
                }
            }


            if (_isFirstLine)
            {
                foreach (var entry in _latestDataLogValues)
                {
                    if (entry.Type == WorkSessionProcessDataType.Current)
                    {
                        addMachineValue(entry.DDI, entry.LastValue, entry.DET);
                    }
                }
                _isFirstLine = false;
            }
        }

        private void addMachineValue(ushort ddi, int value, short? deviceElement = null)
        {
            if (_currentTask == null)
            {
                if (_allowAutoLog)
                {
                    StartAutoLog();
                }
                else
                {
                    throw new NoTaskInWorkSessionException();
                }
            }

            if (_currentDataLine == null || _currentTimeLog == null)
            {
                throw new NoTaskStartedException();
            }

            var index = _currentTimeLog.Header.GetOrAddDataLogValue(ddi, (byte)deviceElement!);
            if (index < _currentDataLine.Entries.Length && !_currentDataLine.Entries[index].IsSet)
            {
                _currentDataLine.Entries[index].Value = value;
                _currentDataLine.Entries[index].IsSet = true;
                _currentDataLine.NumberOfEntries++;
            }


        }


        private short FindDeviceElementIfNull(short? deviceElement = null)
        {
            if (deviceElement == null)
            {
                var elementId = _isoxml.Data.Device.FirstOrDefault()?.DeviceElement.FirstOrDefault(det => det.DeviceElementType == ISODeviceElementType.device)?.DeviceElementId;
                if (elementId != null)
                {
                    deviceElement = short.Parse(elementId.Substring("DET".Count()));
                }
            }

            if (deviceElement == null)
            {
                throw new ElementNotFoundException("DET");
            }

            return (short)deviceElement;

        }


        private ISODevice FindDeviceForDeviceElement(short? deviceElement)
        {
            return _isoxml.Data.Device.FirstOrDefault(dvc => dvc.DeviceElement.Any(det => det.DeviceElementId == "DET" + deviceElement));
        }

        private WorkSessionProcessData FindOrAddLatestDataLogValue(ushort ddi, short deviceElement, WorkSessionProcessDataType type)
        {
            var dlv = _latestDataLogValues.FirstOrDefault(dpd => dpd.DDI == ddi && dpd.DET == deviceElement);
            if (dlv == null)
            {
                dlv = new WorkSessionProcessData()
                {
                    DDI = ddi,
                    DET = deviceElement,
                    Type = type
                };

                _latestDataLogValues.Add(dlv);

            }
            return dlv;
        }


        private bool FindDataLogValueInTimeElement(ushort ddi, short det)
        {
            return _currentTime.DataLogValue.Any(dlv => DDIUtils.ConvertDDI(dlv.ProcessDataDDI) == ddi && dlv.DeviceElementIdRef == "DET" + det);
        }

        private void AddDeviceAllocationIfNoneExists(ISODevice device)
        {
            if (!_currentDeviceAllocations.Any(dan => dan.DeviceIdRef == device.DeviceId))
            {
                var currentDate = DateUtilities.GetDateTimeFromTimeLogInfos(_currentDataLine.Date, _currentDataLine.Time);
                var deviceAllocation = new ISODeviceAllocation()
                {
                    DeviceIdRef = device.DeviceId,
                    ClientNAMEValue = device.ClientNAME,
                    AllocationStamp = new ISOAllocationStamp()
                    {
                        Start = currentDate,
                        Stop = currentDate,
                        Type = ISOType.Effective_Realized
                    }
                };
                _currentDeviceAllocations.Add(deviceAllocation);
                _currentTask.DeviceAllocation.Add(deviceAllocation);
            }
        }



        private void UpdateTimeElementWithLatestDataLogValue(ushort ddi, short det, int latestValue)
        {
            foreach (var dataLogValueEntry in _currentTime.DataLogValue)
            {
                if (
                    DDIUtils.ConvertDDI(dataLogValueEntry.ProcessDataDDI) == ddi &&
                    dataLogValueEntry.DeviceElementIdRef == "DET" + det
                    )
                {
                    dataLogValueEntry.ProcessDataValue = latestValue;
                    break;
                }
            }
        }

        private WorkSessionProcessDataType FindWorkSessionProcessDataType(ushort ddi, ISODevice device)
        {
            var valueType = WorkSessionProcessDataType.Current;
            var relevantProcessData = device.DeviceProcessData.FirstOrDefault(dpd => DDIUtils.ConvertDDI(dpd.DeviceProcessDataDDI) == ddi);//TODO: This only works as long as we don't have a DDI in multiple DeviceElements with different TriggerMethods
            if (relevantProcessData != null)
            {
                if (relevantProcessData.IsTotal())
                {
                    valueType = WorkSessionProcessDataType.Total;
                }
                if (relevantProcessData.IsLifeTimeTotal())
                {
                    valueType = WorkSessionProcessDataType.LifeTime;
                }
            }
            return valueType;
        }

        private void UpdateOrAddDLVInTIM(ushort ddi, short? deviceElement)
        {
            if (!FindDataLogValueInTimeElement(ddi, deviceElement ?? 0))
            {
                _currentTime.DataLogValue.Add(new ISODataLogValue()
                {
                    DeviceElementIdRef = "DET" + deviceElement,
                    ProcessDataDDI = DDIUtils.FormatDDI(ddi)
                });
            }
        }


        private int ConvertValue(ushort ddi, double value, short? deviceElement = null)
        {
            //TODO: Get Offset and Factor from the DeviceDescription
            var factor = 100;
            var offset = 0;

            return (int)Math.Round(value * factor + offset);
        }


        public void UpdateRawMachineValue(ushort ddi, int value, short? deviceElement = null)
        {
            deviceElement = FindDeviceElementIfNull(deviceElement);
            var device = FindDeviceForDeviceElement(deviceElement);

            AddDeviceAllocationIfNoneExists(device);

            addMachineValue(ddi, value, deviceElement);


            var valueType = FindWorkSessionProcessDataType(ddi, device);

            var latestDataLogValue = FindOrAddLatestDataLogValue(ddi, deviceElement ?? 0, valueType);
            latestDataLogValue.LastValue = value;

            if (valueType == WorkSessionProcessDataType.Total || valueType == WorkSessionProcessDataType.LifeTime)
            {
                UpdateOrAddDLVInTIM(ddi, deviceElement);
                UpdateTimeElementWithLatestDataLogValue(ddi, latestDataLogValue.DET, latestDataLogValue.LastValue);

            }
        }


        public void UpdateRawMachineValue(DDIList ddi, int value, short? deviceElement = null)
        {
            UpdateRawMachineValue((ushort)ddi, value, deviceElement);
        }

        public void UpdateMachineValue(DDIList ddi, double value, short? deviceElement = null)
        {
            var rawValue = ConvertValue((ushort)ddi, value, deviceElement);
            UpdateRawMachineValue((ushort)ddi, rawValue, deviceElement);
        }



        public void AddRawValueToMachineValue(ushort ddi, int value, short? deviceElement = null)
        {

            deviceElement = FindDeviceElementIfNull(deviceElement);
            var device = FindDeviceForDeviceElement(deviceElement);
            var valueType = FindWorkSessionProcessDataType(ddi, device);
            var latestDataLogValue = FindOrAddLatestDataLogValue(ddi, deviceElement ?? 0, valueType);
            AddDeviceAllocationIfNoneExists(device);
            latestDataLogValue.LastValue += value;

            if (valueType == WorkSessionProcessDataType.Total || valueType == WorkSessionProcessDataType.LifeTime)
            {
                UpdateOrAddDLVInTIM(ddi, deviceElement);
                UpdateTimeElementWithLatestDataLogValue(ddi, latestDataLogValue.DET, latestDataLogValue.LastValue);

            }

        }

        public void AddRawValueToMachineValue(DDIList ddi, int value, short? deviceElement = null)
        {
            AddRawValueToMachineValue((ushort)ddi, value, deviceElement);
        }

        public void AddValueToMachineValue(DDIList ddi, double value, short? deviceElement = null)
        {
            var rawValue = ConvertValue((ushort)ddi, value, deviceElement);
            AddRawValueToMachineValue((ushort)ddi, rawValue, deviceElement);
        }



        private void EndTask(ISOTaskStatus status)
        {
            _currentTask.TaskStatus = status;
            _currentTask = null;
        }

        public void PauseTask(ISOType2 pauseReason = ISOType2.Ineffective)
        {
            _pauseElement = new ISOTime()
            {
                Start = DateUtilities.GetDateTimeFromTimeLogInfos(_currentDataLine.Date, _currentDataLine.Time),
                Type = pauseReason
            };
            _currentTask.Time.Add(_pauseElement);
            EndTask(ISOTaskStatus.Paused);
        }

        public void FinishTask()
        {
            EndTask(ISOTaskStatus.Completed);
        }


    }
}
