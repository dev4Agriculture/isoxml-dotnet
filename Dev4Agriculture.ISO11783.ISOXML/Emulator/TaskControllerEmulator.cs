using System;
using System.Collections.Generic;
using System.Linq;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.Emulator.Generators;
using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
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
        public int DET;
        public ISODevice Device;
        public WorkSessionProcessDataType Type;
        public bool Used;
        public int LastValue;
    }

    public class WorkSessionFactor
    {
        public ushort DDI;
        public int DET;
        public ISODeviceValuePresentation DVP;
        public int DeviceId;
    }

    public class TaskControllerEmulator
    {
        private static readonly ISODeviceValuePresentation DefaultDVP = new ISODeviceValuePresentation()
        { Scale = 1 };


        private readonly ISOXML _isoxml;
        private ISOTask _currentTask;
        private ISOTask _autoLogTask;
        private DateTime _startTime;
        private ISOTime _currentTime;
        private List<ISODeviceAllocation> _currentDeviceAllocations;
        private ISOTLG _currentTimeLog;
        private readonly bool _allowAutoLog;
        private byte _currentMaxDPDCount;
        private readonly List<ISODevice> _connectedDevices;
        private TLGDataLogLine _currentDataLine;
        private readonly List<WorkSessionProcessData> _latestDataLogValues;
        private readonly List<WorkSessionFactor> _factors;
        private bool _isFirstLine;
        private string _languageShorting;
        private UnitSystem_US _unitSystem;
        private UnitSystem_No_US? _unitSystemNoUs;

        private void FindOrGenerateAutoLogTask()
        {
            foreach (var task in _isoxml.Data.Task)
            {
                if (task.TaskDesignator == "AUTOLOG")
                {
                    _autoLogTask = task;
                    break;
                }
            }
            if (_autoLogTask == null)
            {
                _autoLogTask = new ISOTask()
                {
                    TaskDesignator = "AUTOLOG",
                    TaskStatus = ISOTaskStatus.Planned
                };
                _autoLogTask.AddDefaultDataLogTrigger();
                _isoxml.IdTable.AddObjectAndAssignIdIfNone(_autoLogTask);
                _isoxml.Data.Task.Add(_autoLogTask);

            }

        }


        public TaskControllerEmulator(ISOXML isoxml, bool allowAutoLog = true)
        {
            _isoxml = isoxml;
            _isoxml.DataTransferOrigin = ISO11783TaskDataFileDataTransferOrigin.MICS;
            _allowAutoLog = allowAutoLog;
            if (allowAutoLog)
            {
                FindOrGenerateAutoLogTask();
            }
            _latestDataLogValues = new List<WorkSessionProcessData>();
            _connectedDevices = new List<ISODevice>();
            _factors = new List<WorkSessionFactor>();
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
                if (!isoxml.Data.Device.Any(dvc =>
                        dvc.ClientNAME.Equals(device.ClientNAME) &&
                        dvc.DeviceStructureLabel.SequenceEqual(device.DeviceStructureLabel) &&
                        dvc.DeviceSoftwareVersion.Equals(device.DeviceSoftwareVersion)
                ))
                {
                    isoxml.Data.Device.Add(device);
                }
            }

            if (_allowAutoLog)
            {
                FindOrGenerateAutoLogTask();
            }
        }

        public ISOXML ExportISOXML(DateTime timeStampOfExport, bool prepareForAnalysis = false)
        {
            foreach (var task in _isoxml.Data.Task)
            {
                var finalTim = GetPauseElement(task);
                if (finalTim != null && finalTim.Stop == null)
                {
                    finalTim.Stop = timeStampOfExport;
                }


                var finalDan = task.DeviceAllocation.LastOrDefault();
                if (finalDan != null && finalDan.AllocationStamp != null && finalDan.AllocationStamp.Stop == null)
                {
                    finalDan.AllocationStamp.Stop = timeStampOfExport;
                }
            }

            if (prepareForAnalysis)
            {
                _isoxml.PrepareDataAnalysis();
            }

            return _isoxml;
        }


        public void SetLocalization(string languageShorting, UnitSystem_US unitSystem, UnitSystem_No_US? unitSystemNoUs = null)
        {
            _languageShorting = languageShorting;
            _unitSystem = unitSystem;
            _unitSystemNoUs = unitSystemNoUs;
        }


        private DeviceGenerator AddGenerator(DeviceGenerator deviceGenerator)
        {
            deviceGenerator.SetLocalization(_languageShorting, _unitSystem, _unitSystemNoUs);
            _isoxml.Data.Device.Add(deviceGenerator.GetDevice());
            return deviceGenerator;

        }


        /// <summary>
        /// Create a new DeviceGenerator for Devices with a SerialNumber that's a string that can be converted to Int
        /// Any non-numeric value will be converted to its index in the alphabet +10 (e.g. "DE45" becomes 131445)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="softwareVersion"></param>
        /// <param name="structureLabel"></param>
        /// <param name="deviceClass"></param>
        /// <param name="manufacturer"></param>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        public DeviceGenerator NewDeviceGenerator(string name, string softwareVersion, byte[] structureLabel, DeviceClass deviceClass, int manufacturer, string serialNo)
        {
            var generator = new DeviceGenerator(_isoxml, name, softwareVersion, structureLabel, deviceClass, manufacturer, serialNo);
            return AddGenerator(generator);
        }

        /// <summary>
        /// Create a new DeviceGenerator for Devices with a pure numeric serial number
        /// </summary>
        /// <param name="name"></param>
        /// <param name="softwareVersion"></param>
        /// <param name="structureLabel"></param>
        /// <param name="deviceClass"></param>
        /// <param name="manufacturer"></param>
        /// <param name="serialNoLong"></param>
        /// <returns></returns>
        public DeviceGenerator NewDeviceGenerator(string name, string softwareVersion, byte[] structureLabel, DeviceClass deviceClass, int manufacturer, int serialNoLong)
        {
            var generator = new DeviceGenerator(_isoxml, name, softwareVersion, structureLabel, deviceClass, manufacturer, serialNoLong);
            return AddGenerator(generator);
        }


        /// <summary>
        /// Create a new DeviceGenerator for a Device which has 2 serial numbers: 1 numeric for the clientname, 1 including letters for the DeviceDescription
        /// </summary>
        /// <param name="name"></param>
        /// <param name="softwareVersion"></param>
        /// <param name="structureLabel"></param>
        /// <param name="deviceClass"></param>
        /// <param name="manufacturer"></param>
        /// <param name="serialNo"></param>
        /// <param name="serialNoLong"></param>
        /// <returns></returns>
        public DeviceGenerator NewDeviceGenerator(string name, string softwareVersion, byte[] structureLabel, DeviceClass deviceClass, int manufacturer, string serialNo, int serialNoLong)
        {
            var generator = new DeviceGenerator(_isoxml, name, softwareVersion, structureLabel, deviceClass, manufacturer, serialNo, serialNoLong);
            return AddGenerator(generator);
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



        /// <summary>
        /// Returns the current TaskdataSet ISOXML.
        /// This function is e.g. used to add other elements like Farms or Customers afterwards.
        /// Attention: To export the full ISOXML, call the ExportISOXML Function
        /// </summary>
        /// <returns></returns>
        public ISOXML GetTaskDataSet()
        {
            return _isoxml;
        }

        /// <summary>
        /// This function connects a machine to the TaskController Emulator. Afterwards the machine will be mentioned in any started Task until it was disconnected
        /// </summary>
        /// <param name="device"></param>
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


        /// <summary>
        /// This is equal to "unplugging" the Device(Machine) from a TaskController
        /// </summary>
        /// <param name="device"></param>
        public void DisconnectDevice(ISODevice device)
        {
            _connectedDevices.Remove(device);
            _currentMaxDPDCount = (byte)_connectedDevices.Sum(dvc => dvc.DeviceProcessData.Count());
            _factors.RemoveAll(factor => ("DVC" + factor.DeviceId) == device.DeviceId);
            //TODO if we have more than 255DDIs, we need a second TimeLog


            if (_currentTask != null)
            {
                AddTimeLog();
            }
        }


        private void StartAutoLog(DateTime timestamp)
        {
            StartTask(timestamp, _autoLogTask);
        }


        /// <summary>
        /// Start a Task with a name. Attention: This function always creates a new Task!
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ISOTask StartTask(DateTime timestamp, string name)
        {
            var task = new ISOTask()
            {
                TaskDesignator = name
            };
            StartTask(timestamp, task);
            return task;
        }


        /// <summary>
        /// Start a Task from an ISOTask Object. If the ISOTask Object is not yet part of the TaskData within the TCEmulator, it'll be added.
        /// When adding the Task, its ID might change. The updated Element is returned
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        /// <exception cref="NoTaskInWorkSessionException"></exception>
        public ISOTask StartTask(DateTime timestamp, ISOTask task)
        {
            if (_currentTask != null)
            {
                EndTask(timestamp, ISOTaskStatus.Paused);
            }
            var pauseElement = GetPauseElement(task);
            if (pauseElement != null)
            {
                pauseElement.Stop = timestamp;
                pauseElement = null;
            }
            _currentDeviceAllocations = new List<ISODeviceAllocation>();
            _currentMaxDPDCount = (byte)_connectedDevices.Sum(dvc => dvc.DeviceProcessData.Count);
            _currentTask = task;
            if (_currentTask == null)
            {
                if (_allowAutoLog && _autoLogTask != task)
                {
                    StartAutoLog(timestamp);
                }
                else
                {
                    throw new NoTaskInWorkSessionException();
                }
            }
            else
            {
                if (!_isoxml.Data.Task.Any(entry => entry.Equals(task)))
                {
                    _isoxml.IdTable.AddObjectAndAssignIdIfNone(task);
                    _isoxml.Data.Task.Add(task);
                }
            }
            _startTime = timestamp;
            if (_currentTask.Time.Any(entry => entry.Type == ISOType2.Effective))
            {
                var isoTime = _currentTask.Time.LastOrDefault(entry => entry.Type == ISOType2.Effective);
                if (isoTime != null)
                {
                    UpdateLatestDataLogValuesFromTimeElement(isoTime);
                }
            }
            else
            {
                ClearLatestDataLogValues();
            }
            _currentTime = new ISOTime()
            {
                Start = _startTime,
                Type = ISOType2.Effective
            };
            _currentTimeLog = null;
            _currentTask.Time.Add(_currentTime);
            AddTimeLog();
            _isFirstLine = true;
            return _currentTask;
        }

        private void ClearLatestDataLogValues()
        {
            //Onchange Machine data need to persist, but Totals need to be reverted to 0
            foreach (var entry in _latestDataLogValues)
            {
                if (entry.Type == WorkSessionProcessDataType.Total)
                {
                    entry.LastValue = 0;
                }
            }
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
                else
                {
                    var detId = IdList.ToIntId(dlv.DeviceElementIdRef);
                    _latestDataLogValues.Add(new WorkSessionProcessData()
                    {
                        DDI = DDIUtils.ConvertDDI(dlv.ProcessDataDDI),
                        DET = detId,
                        Device = FindDeviceForDeviceElement(detId),
                        LastValue = (int)dlv.ProcessDataValue,
                        Type = WorkSessionProcessDataType.Total //This part should only happen if we find a DLV in a TIM for a machine that is not connected anymore. As it will most likely not change, we can set this to be a Total.
                    });
                }
            }
        }


        /// <summary>
        /// Add a new entry for Time+Position to the current Task and by that to the current TimeLog
        /// It also finishes the previous line
        /// Important: This always has to be the first function called before adding machine data
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="position"></param>
        /// <exception cref="NoTaskInWorkSessionException"></exception>
        public void AddTimeAndPosition(DateTime timestamp, ISOPosition position)
        {
            if (_currentTask == null)
            {
                if (_allowAutoLog)
                {
                    StartAutoLog(timestamp);
                }
                else
                {
                    throw new NoTaskInWorkSessionException();
                }
            }

            if (_currentTimeLog == null)
            {
                AddTimeLog();
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
                    AddMachineValue(entry.DDI, entry.LastValue, entry.DET);
                }
                _isFirstLine = false;
            }
        }

        private void AddMachineValue(ushort ddi, int value, int? deviceElement = null)
        {
            if (_currentDataLine == null || _currentTimeLog == null)
            {
                throw new NoTaskStartedException();
            }

            var index = _currentTimeLog.Header.GetOrAddDataLogValue(ddi, deviceElement.Value);
            if (index < _currentDataLine.Entries.Length && !_currentDataLine.Entries[index].IsSet)
            {
                _currentDataLine.Entries[index].Value = value;
                _currentDataLine.Entries[index].IsSet = true;
                _currentDataLine.NumberOfEntries++;
            }


        }


        private short FindDeviceElementIfNull(int? deviceElement = null)
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


        private ISODevice FindDeviceForDeviceElement(int? deviceElementId)
        {
            if (deviceElementId == null)
            {
                return _isoxml.Data.Device.FirstOrDefault();
            }
            else
            {
                return _isoxml.Data.Device.FirstOrDefault(dvc => dvc.DeviceElement.Any(det => det.DeviceElementId == "DET" + deviceElementId));
            }
        }

        private WorkSessionProcessData FindOrAddLatestDataLogValue(ushort ddi, int deviceElement, WorkSessionProcessDataType type)
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


        private bool FindDataLogValueInTimeElement(ushort ddi, int det)
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



        private void UpdateTimeElementWithLatestDataLogValue(ushort ddi, int det, int latestValue)
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

        private void UpdateOrAddDLVInTIM(ushort ddi, int? deviceElement)
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


        private int ConvertValue(ushort ddi, double value, int? deviceElement = null)
        {
            var deviceValuePresentation = FindDeviceValuePresentationByDDI(ddi, deviceElement);
            return (int)Math.Round(value / (double)deviceValuePresentation.Scale - deviceValuePresentation.Offset);
        }

        private ISODeviceValuePresentation FindDeviceValuePresentationByDDI(ushort ddi, int? deviceElement = null)
        {
            ISODevice device = null;
            if (deviceElement == null)
            {
                device = _isoxml.Data.Device.FirstOrDefault();
                if (device == null)
                {
                    _factors.Add(new WorkSessionFactor()
                    {
                        DDI = ddi,
                        DET = 0,
                        DeviceId = 0,
                        DVP = DefaultDVP
                    });
                    return DefaultDVP;
                }
            }
            else
            {
                var fromList = _factors.FirstOrDefault(entry => entry.DDI == ddi && entry.DET == deviceElement);
                if (fromList != null)
                {
                    return fromList.DVP;
                }
            }

            device = FindDeviceForDeviceElement(deviceElement);
            if (device == null)
            {
                _factors.Add(new WorkSessionFactor()
                {
                    DDI = ddi,
                    DET = 0,
                    DeviceId = 0,
                    DVP = DefaultDVP
                });
                return DefaultDVP;
            }



            var potentialDPDs = device.DeviceProcessData.Where(dpd2 => DDIUtils.ConvertDDI(dpd2.DeviceProcessDataDDI) == ddi);
            if (!potentialDPDs.Any())
            {
                _factors.Add(new WorkSessionFactor()
                {
                    DDI = ddi,
                    DET = 0,
                    DeviceId = 0,
                    DVP = DefaultDVP
                });
                return DefaultDVP;
            }

            var dpd = potentialDPDs.FirstOrDefault(
                potDpd => device.DeviceElement.Any(
                    det => det.DeviceObjectReference.Any(
                        dor => dor.DeviceObjectId == potDpd.DeviceProcessDataObjectId)
                    )
                );
            if (dpd == null || !dpd.DeviceValuePresentationObjectIdValueSpecified)
            {
                _factors.Add(new WorkSessionFactor()
                {
                    DDI = ddi,
                    DET = deviceElement ?? 0,
                    DeviceId = 0,
                    DVP = DefaultDVP
                });
                return DefaultDVP;
            }

            var deviceValuePresentation = device.DeviceValuePresentation.FirstOrDefault(dvp => dvp.DeviceValuePresentationObjectId == dpd.DeviceValuePresentationObjectId);
            if (deviceValuePresentation == null)
            {
                _factors.Add(new WorkSessionFactor()
                {
                    DDI = ddi,
                    DET = 0,
                    DeviceId = 0,
                    DVP = DefaultDVP
                });
                return DefaultDVP;
            }
            _factors.Add(new WorkSessionFactor()
            {
                DDI = ddi,
                DET = 0,
                DeviceId = IdList.ToIntId(device.DeviceId),
                DVP = DefaultDVP
            });

            return deviceValuePresentation;
        }


        /// <summary>
        /// Update a "current" value for a Machines DDI; e.g. the Current Fuel Consumption.
        /// This function takes the RawValue like you'ld send it via the ISOBUS.
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="value"></param>
        /// <param name="deviceElement"></param>
        public void UpdateRawMachineValue(ushort ddi, int value, int? deviceElement = null)
        {
            deviceElement = FindDeviceElementIfNull(deviceElement);
            var device = FindDeviceForDeviceElement(deviceElement);

            AddDeviceAllocationIfNoneExists(device);

            AddMachineValue(ddi, value, deviceElement);


            var valueType = FindWorkSessionProcessDataType(ddi, device);

            var latestDataLogValue = FindOrAddLatestDataLogValue(ddi, deviceElement ?? 0, valueType);
            latestDataLogValue.LastValue = value;

            if (valueType == WorkSessionProcessDataType.Total || valueType == WorkSessionProcessDataType.LifeTime)
            {
                UpdateOrAddDLVInTIM(ddi, deviceElement);
                UpdateTimeElementWithLatestDataLogValue(ddi, latestDataLogValue.DET, latestDataLogValue.LastValue);

            }
        }

        /// <summary>
        /// Update a "current" value for a Machines DDI; e.g. the Current Fuel Consumption.
        /// This function takes the RawValue like you'ld send it via the ISOBUS.
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="value"></param>
        /// <param name="deviceElement"></param>
        public void UpdateRawMachineValue(DDIList ddi, int value, int? deviceElement = null)
        {
            UpdateRawMachineValue((ushort)ddi, value, deviceElement);
        }


        /// <summary>
        /// Update a "current" value for a Machines DDI; e.g. the Current Fuel Consumption.
        /// This function takes the value using the unit defined in the corresponding DeviceValuePresentation.
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="value"></param>
        /// <param name="deviceElement"></param>
        public void UpdateMachineValue(DDIList ddi, double value, int? deviceElement = null)
        {
            var rawValue = ConvertValue((ushort)ddi, value, deviceElement);
            UpdateRawMachineValue((ushort)ddi, rawValue, deviceElement);
        }


        /// <summary>
        /// Add (=increase!) a Total value for a machines DDI; e.g. TotalArea.
        /// This function takes the raw value of difference like it would be sent via the ISOBUS.
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="value"></param>
        /// <param name="deviceElement"></param>
        public void AddRawValueToMachineValue(ushort ddi, int value, int? deviceElement = null)
        {

            deviceElement = FindDeviceElementIfNull(deviceElement);
            var device = FindDeviceForDeviceElement(deviceElement);
            var valueType = FindWorkSessionProcessDataType(ddi, device);
            var latestDataLogValue = FindOrAddLatestDataLogValue(ddi, deviceElement ?? 0, valueType);
            AddDeviceAllocationIfNoneExists(device);
            latestDataLogValue.LastValue += value;
            AddMachineValue(ddi, latestDataLogValue.LastValue, deviceElement);
            if (valueType == WorkSessionProcessDataType.Total || valueType == WorkSessionProcessDataType.LifeTime)
            {
                UpdateOrAddDLVInTIM(ddi, deviceElement);
                UpdateTimeElementWithLatestDataLogValue(ddi, latestDataLogValue.DET, latestDataLogValue.LastValue);

            }

        }

        /// <summary>
        /// Add (=increase!) a Total value for a machines DDI; e.g. TotalArea.
        /// This function takes the raw value of difference like it would be sent via the ISOBUS.
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="value"></param>
        /// <param name="deviceElement"></param>
        public void AddRawValueToMachineValue(DDIList ddi, int value, int? deviceElement = null)
        {
            AddRawValueToMachineValue((ushort)ddi, value, deviceElement);
        }

        /// <summary>
        /// Add (=increase!) a Total value for a machines DDI; e.g. TotalArea.
        /// This function takes the value of difference using the unit defined in the corresponding DeviceValuePresentation.
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="value"></param>
        /// <param name="deviceElement"></param>
        public void AddValueToMachineValue(DDIList ddi, double value, int? deviceElement = null)
        {
            var rawValue = ConvertValue((ushort)ddi, value, deviceElement);
            AddRawValueToMachineValue((ushort)ddi, rawValue, deviceElement);
        }



        private void EndTask(DateTime timeStamp, ISOTaskStatus status)
        {
            if (_currentDataLine != null)
            {
                _currentTimeLog.Entries.Add(_currentDataLine);
            }
            _currentDataLine = null;
            _currentTask.TaskStatus = status;
            var activeTaskWasAutoLog = _currentTask == _autoLogTask;
            _currentTask = null;
            _currentTime = null;
            _currentTimeLog = null;
            var pauseElement = GetPauseElement(_currentTask);
            if (pauseElement != null)
            {
                pauseElement.Stop = timeStamp;
            }
            if (_allowAutoLog && !activeTaskWasAutoLog)
            {
                StartAutoLog(timeStamp);
            }
        }

        private ISOTime GetPauseElement(ISOTask currentTask)
        {
            if (currentTask == null)
            {
                return null;
            }
            var orderedElements = currentTask.Time.OrderBy(time => time.Start);
            var lastElement = orderedElements.FirstOrDefault(time => time.Type != ISOType2.Planned && time.Type != ISOType2.Effective);
            return lastElement;

        }

        /// <summary>
        /// Pause a Task so you can restart it later
        /// </summary>
        /// <param name="pauseReason"></param>
        public void PauseTask(ISOType2 pauseReason = ISOType2.Ineffective)
        {
            var timeStamp = DateUtilities.GetDateTimeFromTimeLogInfos(_currentDataLine.Date, _currentDataLine.Time);
            var pauseElement = new ISOTime()
            {
                Start = timeStamp,
                Type = pauseReason
            };
            _currentTask.Time.Add(pauseElement);
            EndTask(timeStamp, ISOTaskStatus.Paused);
        }


        /// <summary>
        /// Stop a Task. This task should not be started again afterwards
        /// </summary>
        public void FinishTask()
        {
            if (_currentDataLine != null)
            {
                var timeStamp = DateUtilities.GetDateTimeFromTimeLogInfos(_currentDataLine.Date, _currentDataLine.Time);
                EndTask(timeStamp, ISOTaskStatus.Completed);
            }
        }

    }
}
