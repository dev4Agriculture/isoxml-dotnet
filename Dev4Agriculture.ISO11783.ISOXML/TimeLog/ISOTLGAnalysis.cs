using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.DTO;
using Dev4Agriculture.ISO11783.ISOXML.IdHandling;
using Dev4Agriculture.ISO11783.ISOXML.TaskFile;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.TimeLog
{

    /// <summary>
    /// ISOTLG is the class to read and write TimeLog Files (TLG....bin/.xml)
    /// This partial class hosts the 
    /// </summary>
    public partial class ISOTLG
    {
        public const double TLG_GPS_FACTOR = 10000000.0;
        private DDIAvailabilityStatus _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;

        private List<ISODevice> _devices = null;
        private List<ISOTimeLogDeviceProperty> _properties = null;

        /// <summary>
        /// Trying to find the maximum value in the TimeLogFile for the corresponding parameters
        /// </summary>
        /// <param name="ddi">The DDI, see, http://isobus.net</param>
        /// <param name="deviceElement">The DeviceElement of the device that performed the work</param>
        /// <param name="lastValue">This variable is filled with the result</param>
        /// <returns>True on success</returns>
        public bool TryGetMaximum(ushort ddi, int deviceElement, out int maximum)
        {
            if (TryGetPropertyValue(ddi, out var maxLong, deviceElement))
            {
                maximum = (int)maxLong;
                return true;
            }

            if (!Header.TryGetDDIIndex(ddi, deviceElement, out var index))
            {
                _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;
                maximum = 0;
                return false;
            }

            _ddiAvailabilityStatus = DDIAvailabilityStatus.NO_VALUE;
            maximum = 0;
            foreach (var entry in Entries)
            {
                if (entry.TryGetValue(index, out var entryValue))
                {
                    maximum = maximum > entryValue ? maximum : entryValue;
                    _ddiAvailabilityStatus = DDIAvailabilityStatus.HAS_VALUE;
                }
            }
            return true;
        }

        /// <summary>
        /// Trying to find the minimum value in the TimeLogFile for the corresponding parameters
        /// </summary>
        /// <param name="ddi">The DDI, see, http://isobus.net</param>
        /// <param name="deviceElement">The DeviceElement of the device that performed the work</param>
        /// <param name="lastValue">This variable is filled with the result</param>
        /// <returns>True on success</returns>
        public bool TryGetMinimum(ushort ddi, int deviceElement, out int minimum)
        {
            if (TryGetPropertyValue(ddi, out var minLong, deviceElement))
            {
                minimum = (int)minLong;
                return true;
            }

            if (!Header.TryGetDDIIndex(ddi, deviceElement, out var index))
            {
                _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;
                minimum = 0;
                return false;
            }

            _ddiAvailabilityStatus = DDIAvailabilityStatus.NO_VALUE;
            minimum = int.MaxValue;
            foreach (var entry in Entries)
            {
                if (entry.TryGetValue(index, out var entryValue))
                {
                    minimum = minimum < entryValue ? minimum : entryValue;
                    _ddiAvailabilityStatus = DDIAvailabilityStatus.HAS_VALUE;
                }
            }
            return true;
        }



        /// <summary>
        /// Get the Area bounds for a TimeLog; means the area in which points of this TimeLog show up
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public bool TryGetTLGBounds(out AreaBounds bounds)
        {
            bounds = new AreaBounds();
            if ( Entries.Count == 0)
            {
                return false;
            }
            foreach (var entry in Entries)
            {
                if( (entry.PosStatus != (byte)ISOPositionStatus.Error) && (entry.PosStatus != (byte)ISOPositionStatus.noGPSfix))
                {
                    bounds.Update((decimal)entry.Latitude, (decimal)entry.Longitude);
                }
            }

            return true;
        }


        /// <summary>
        /// Trying to find the first available value in the TimeLogFile for the corresponding parameters
        /// </summary>
        /// <param name="ddi">The DDI, see, http://isobus.net</param>
        /// <param name="deviceElement">The DeviceElement of the device that performed the work</param>
        /// <param name="lastValue">This variable is filled with the result</param>
        /// <returns>True on success</returns>
        public bool TryGetFirstValue(ushort ddi, int deviceElement, out int firstValue)
        {
            if (TryGetPropertyValue(ddi, out var firstLong, deviceElement))
            {
                firstValue = (int)firstLong;
                return true;
            }
            if (!Header.TryGetDDIIndex(ddi, deviceElement, out var index))
            {
                _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;
                firstValue = 0;
                return false;
            }

            _ddiAvailabilityStatus = DDIAvailabilityStatus.NO_VALUE;
            foreach (var entry in Entries)
            {
                if (entry.TryGetValue(index, out var entryValue))
                {
                    _ddiAvailabilityStatus = DDIAvailabilityStatus.HAS_VALUE;
                    firstValue = entryValue;
                    return true;
                }
            }
            firstValue = 0;
            return false;
        }


        /// <summary>
        /// Trying to find the last available value in the TimeLogFile for the corresponding parameters
        /// </summary>
        /// <param name="ddi">The DDI, see, http://isobus.net</param>
        /// <param name="deviceElement">The DeviceElement of the device that performed the work</param>
        /// <param name="lastValue">This variable is filled with the result</param>
        /// <returns>True on success</returns>
        public bool TryGetLastValue(ushort ddi, int deviceElement, out int lastValue)
        {
            if (TryGetPropertyValue(ddi, out var lastLong, deviceElement))
            {
                lastValue = (int)lastLong;
                return true;
            }

            if (!Header.TryGetDDIIndex(ddi, deviceElement, out var index))
            {
                _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;
                lastValue = 0;
                return false;
            }

            _ddiAvailabilityStatus = DDIAvailabilityStatus.NO_VALUE;
            var v = Entries.Count - 1;
            for (var entryIndex = v; entryIndex > 0; entryIndex--)
            {
                if (Entries[entryIndex].TryGetValue(index, out var entryValue))
                {
                    _ddiAvailabilityStatus = DDIAvailabilityStatus.HAS_VALUE;
                    lastValue = entryValue;
                    return true;
                }
            }
            lastValue = 0;
            return false;
        }




        /// <summary>
        /// Finds a total for DDI + DeviceElement based on the requested Algorithm.
        /// REMARK: It only returns the Total for this specific TimeLog, so, the difference!
        /// </summary>
        /// <param name="ddi">The Data Dictionary Identifier</param>
        /// <param name="deviceElement">The Data Dictionary Identifier</param>
        /// <param name="totalValue">The RETURNED Total Value</param>
        /// <param name="totalAlgorithm">The Algorithm to use for this Total</param>
        /// <returns></returns>
        public bool TryGetTotalValue(ushort ddi, int deviceElement, out int totalValue, TLGTotalAlgorithmType totalAlgorithm)
        {
            if (!Header.TryGetDDIIndex(ddi, deviceElement, out var index))
            {
                _ddiAvailabilityStatus = DDIAvailabilityStatus.NOT_IN_HEADER;
                totalValue = 0;
                return false;
            }

            _ddiAvailabilityStatus = DDIAvailabilityStatus.NO_VALUE;

            if (totalAlgorithm == TLGTotalAlgorithmType.LIFETIME)
            {
                return TryGetLastValue(ddi, deviceElement, out totalValue);
            }
            else if (totalAlgorithm == TLGTotalAlgorithmType.NO_RESETS)
            {
                if (TryGetFirstValue(ddi, deviceElement, out var first))
                {
                    if (TryGetLastValue(ddi, deviceElement, out var last))
                    {
                        _ddiAvailabilityStatus = DDIAvailabilityStatus.HAS_VALUE;
                        totalValue = last - first;
                        return true;
                    }
                }
                totalValue = 0;
                return false;
            }
            else
            {
                var lastValue = 0;
                var isStarted = false;
                totalValue = 0;
                foreach (var entry in Entries)
                {
                    if (entry.TryGetValue(index, out var curValue))
                    {
                        _ddiAvailabilityStatus = DDIAvailabilityStatus.HAS_VALUE;
                        if (!isStarted)
                        {
                            isStarted = true;
                            lastValue = curValue;
                        }
                        if (curValue >= lastValue)
                        {
                            totalValue += curValue - lastValue;
                            lastValue = curValue;
                        }
                        else
                        {
                            lastValue = curValue;
                        }
                    }
                }
                return true;

            }
        }


        /// <summary>
        /// Returns the Availability-Status for a value based on the result from the latest call for TryGetMaximum,-Minimum, etc.
        /// This function does NOT actually check the availability status
        /// </summary>
        /// <returns></returns>
        public DDIAvailabilityStatus GetLatestDDIAvailabilityStatus()
        {
            return _ddiAvailabilityStatus;
        }

        /// <summary>
        /// Checks if values for a specific combination of DDI and DeviceElement are available
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="deviceElement"></param>
        /// <returns></returns>
        public DDIAvailabilityStatus GetDDIAvailabilityStatus(ushort ddi, int deviceElement)
        {
            if (!Header.TryGetDDIIndex(ddi, deviceElement, out var index))
            {
                return DDIAvailabilityStatus.NOT_IN_HEADER;
            }

            foreach (var entry in Entries)
            {
                if (entry.TryGetValue(index, out var entryValue))
                {
                    return DDIAvailabilityStatus.HAS_VALUE;
                }
            }
            return DDIAvailabilityStatus.NO_VALUE;
        }

        private bool IsRegisteredSpecialDDI(ushort ddi, ushort manufacturer, out DDIRegisterEntry ddiSetting)
        {
            if (DDIRegister.ManufacturersList.TryGetValue(manufacturer, out var manufacturerSettings) &&
                            manufacturerSettings.DDIs.TryGetValue(ddi, out ddiSetting)
                        )
            {
                return true;
            }
            ddiSetting = null;
            return false;
        }

        /// <summary>
        /// Creates a list of DLV-Elements with the totals values for all available Totals DDIs
        /// This is used e.g. when the TimeLogs were generated by the library and we need a DLV-List to create a common TIM-Element like in other TaskSets
        /// </summary>
        /// <param name="totalAlgorithmType">The algorithm to take for all *non-lifetime* Totals</param>
        /// <param name="devices">The list of devices available in the TaskSet. This is required to find out, which available DDI is a total</param>
        /// <returns>A list of DataLogValues (DLVs) elements</returns>
        public List<ISODataLogValue> GenerateTotalsDataLogValues(TLGTotalAlgorithmType totalAlgorithmType, IEnumerable<ISODevice> devices)
        {
            var list = new List<ISODataLogValue>();
            var totals = devices.SelectMany(device => device.GetAllTotalsProcessData());
            foreach (var (det, dpd) in totals)
            {
                var ddi = DDIUtils.ConvertDDI(dpd.DeviceProcessDataDDI);
                var detAsInt = IdList.ToIntId(det.DeviceElementId);
                var dlv = new ISODataLogValue()
                {
                    ProcessDataDDI = dpd.DeviceProcessDataDDI,
                    DeviceElementIdRef = det.DeviceElementId,
                };


                try
                {
                    var device = devices.FirstOrDefault(entry => entry.DeviceElement.Any(deviceElement => deviceElement.DeviceElementId.Equals(det.DeviceElementId)));
                    if (device == null)
                    {
                        continue;
                    }

                    var manufacturer = (ushort)device.ClientNameParsed.ManufacturerCode;
                    if (IsRegisteredSpecialDDI(ddi, manufacturer, out var ddiSetting))
                    {
                        dlv.ProcessDataValue = ddiSetting.SingleTimelogCalculationCallback(new SingleTimelogDDICalculationCallbackParameters()
                        {
                            Device = device,
                            Tlg = this
                        });
                    }
                    else if (dpd.IsLifeTimeTotal())
                    {
                        if (TryGetLastValue(ddi, detAsInt, out var total))
                        {
                            dlv.ProcessDataValue = total;
                        }
                    }
                    else if (dpd.IsTotal())
                    {
                        if (DDIAlgorithms.AveragesDDIWeightedDdiMap.TryGetValue(ddi, out var weightDDIs))
                        {
                            if (TryGetWeightedAverage(
                                ddi,
                                detAsInt,
                                device,
                                weightDDIs,
                                out var total))
                            {
                                dlv.ProcessDataValue = total;
                            }
                        }
                        else
                        {
                            if (TryGetTotalValue(ddi, detAsInt, out var total, totalAlgorithmType))
                            {
                                dlv.ProcessDataValue = total;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO: What should we do in case this fails?
                    dlv.ProcessDataValue = long.MaxValue;
                }
                list.Add(dlv);
            }

            return list;
        }

        /// <summary>
        /// Some DDIs like AverageCropMoisture cannot just be summed up; they require an average algorithm; weighted by another DDI.
        /// In some cases, there are multiple DDIs possible, as a specific DDI like TotalYield might not always exist.
        /// Examples for weighted DDIs can be found in the DDIAlgorithms.cs file.
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="detAsInt"></param>
        /// <param name="device"></param>
        /// <param name="weightDDIs"></param>
        /// <param name="totalValue"></param>
        /// <returns></returns>
        public bool TryGetWeightedAverage(ushort ddi, int detAsInt, ISODevice device, ushort[] weightDDIs, out long totalValue)
        {
            ushort weightDDI = 0;
            foreach (var possibleDDI in weightDDIs)
            {
                if (device.DeviceProcessData.Any(dpd => dpd.DeviceProcessDataDDI == DDIUtils.FormatDDI(possibleDDI)))
                {
                    weightDDI = possibleDDI;
                    break;
                }
            }

            if (weightDDI == 0)
            {
                totalValue = 0;
                return false;
            }

            if (!TryGetFirstValue(ddi, detAsInt, out var startAverage)||
                !TryGetFirstValue(weightDDI, detAsInt, out var startCount) ||
                !TryGetLastValue(ddi, detAsInt, out var endAverage) ||
                !TryGetLastValue(weightDDI, detAsInt, out var endCount)
                )
            {
                totalValue = 0;
                return true;
            }

            totalValue = (long)MathUtils.CalculateCleanedWeightedAverage(startAverage, startCount, endAverage, endCount);

            return true;

        }


        /// <summary>
        /// Generates a Time-Element corresponding to the TimeLog Entry
        /// The List of Devices is required to filter the TimeLog DDI-Entries for Totals and Lifetime Totals
        /// </summary>
        /// <param name="devices"></param>
        /// <returns>Time Elements from a TimeLog</returns>
        public ISOTime GenerateTimeElement(IEnumerable<ISODevice> devices)
        {
            var min = DateTime.MaxValue;
            var max = DateTime.MinValue;
            foreach (var entry in Entries)
            {
                var date = DateUtilities.GetDateTimeFromTimeLogInfos(entry.Date, entry.Time);
                if (min.CompareTo(date) > 0)
                {
                    min = date;
                }

                if (max.CompareTo(date) < 0)
                {
                    max = date;
                }
            }

            var dataLogValues = GenerateTotalsDataLogValues(TLGTotalAlgorithmType.NO_RESETS, devices);

            var isoTime = new ISOTime()
            {
                Start = min,
                Stop = max,
                Type = ISOType2.Effective,
            };

            foreach (var entry in dataLogValues)
            {
                isoTime.DataLogValue.Add(entry);
            }

            return isoTime;
        }


        /// <summary>
        /// Iterates over the Header for the TimeLog and returns all Devices associated  with the TimeLog
        /// After the first call, a list is persisted and will directly be delivered. To force an update, set the optional parameter "update" to true
        /// </summary>
        /// <param name="isoxml">The associated ISOXML Element</param>
        /// <param name="update">If true, the _deviceList is updated</param>
        /// <returns></returns>
        public IEnumerable<ISODevice> GetDevicesForTimeLog(ISOXML isoxml, bool update = false)
        {
            if (_devices == null || update)
            {
                var detIds = Header.Ddis.Select(entry => entry.DeviceElement).ToList().Distinct();
                _devices = detIds.Select(
                    detId => isoxml.Data.Device.FirstOrDefault(
                        dvc => dvc.DeviceElement.Any(
                            det => det.DeviceElementId.Equals("DET" + detId)
                            )
                        )
                ).Distinct().ToList();
            }
            return _devices;
        }


        private void FillPropertiesFromUsedDevices()
        {
            if(_properties == null)
            {
                _properties = new List<ISOTimeLogDeviceProperty>();
            }
            foreach (var device in _devices)
            {
                foreach (var property in device.DeviceProperty)
                {
                    var det = device.DeviceElement.FirstOrDefault(deviceElement =>
                        deviceElement.DeviceObjectReference.Any(dor =>
                            dor.DeviceObjectId == property.DevicePropertyObjectId
                            )
                        );
                    _properties.Add(new ISOTimeLogDeviceProperty(property)
                    {
                        DeviceElement = det
                    });
                }
            }
        }


        /// <summary>
        /// Check if this DDI exists as ProcesseData in the TimeLog
        /// </summary>
        /// <param name="ddi"></param>
        /// <param name="deviceElement"></param>
        /// <returns></returns>
        public bool IsDeviceProcessData(ushort ddi, int? deviceElement = null)
        {
            foreach (var entry in Header.Ddis)
            {
                if (entry.Ddi == ddi && (deviceElement == null || entry.DeviceElement == deviceElement))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns true, if the DeviceProperty + DeviceElement exists
        /// </summary>
        /// <param name="ddi">The DataDictionary Identifier, see https://isobus.net </param>
        /// <param name="deviceElement">A DeviceElement</param>
        /// <returns></returns>
        public bool IsDeviceProperty(ushort ddi, int? deviceElement = null)
        {
            foreach (var entry in _properties)
            {
                if (DDIUtils.ConvertDDI(entry.DevicePropertyDDI) == ddi && (deviceElement == null || entry.DeviceElement.DeviceElementId == "DET" + deviceElement))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns true if a Property exists and also exports the rawValue. 
        /// </summary>
        /// <param name="ddi">The DataDictionary Identifier, see https://isobus.net </param>
        /// <param name="deviceElement">A DeviceElement</param>
        /// <returns>True if found, False otherwise. If True, rawValue is filled. Otherwise it's 0</returns>
        public bool TryGetPropertyValue(ushort ddi, out long rawValue, int? deviceElement = null)
        {
            foreach (var entry in _properties)
            {
                if (DDIUtils.ConvertDDI(entry.DevicePropertyDDI) == ddi && (deviceElement == null || entry.DeviceElementId == deviceElement))
                {
                    rawValue = entry.DevicePropertyValue;
                    return true;
                }
            }
            rawValue = 0;
            return false;
        }

        /// <summary>
        /// This function returns true, if a value can be found, no matter if its source is a Property or a ProcessData
        /// ATTENTION: The performance of this function might not be suitable for you when iterating over every single data point. Better use a oneTime TryGetPropertyValue and a TryGetDDIEntryIndex
        /// </summary>
        /// <param name="ddi">The DataDictionary Identifier, see https://isobus.net</param>
        /// <param name="datalogLine">The Line within the DataSet</param>
        /// <param name="rawValue">The found value</param>
        /// <param name="deviceElement">A deviceElement (0 and null means "first Deviceelement with such DDI")</param>
        /// <returns></returns>
        public bool TryGetMachineValue(ushort ddi, int datalogLine, out long rawValue, int? deviceElement = null)
        {
            if (!TryGetPropertyValue(ddi, out rawValue, deviceElement))
            {
                if (Header.TryGetDDIIndex(ddi, deviceElement ?? 0, out var index))
                {
                    if (datalogLine < 0 || datalogLine >= Entries.Count)
                    {
                        return false;
                    }
                    if (Entries[datalogLine].TryGetValue(index, out var rawOutValue))
                    {
                        rawValue = rawOutValue;
                        return true;
                    }
                }
                return false;
            }
            return true;
        }


        /// <summary>
        /// Find the Index for a specific DDI within TimeLogData Headers
        /// </summary>
        /// <param name="ddi">The DataDictionary Identifier, see https://isobus.net</param>
        /// <param name="index"></param>
        /// <param name="deviceElement"></param>
        /// <returns></returns>
        public bool TryGetDDIIndex(ushort ddi, out uint index, int? deviceElement = null)
        {
            return Header.TryGetDDIIndex(ddi, deviceElement ?? 0, out index);
        }


        internal void Analyse(ISOXML isoxml)
        {
            GetDevicesForTimeLog(isoxml);
            FillPropertiesFromUsedDevices();
        }


    }

}
