﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using de.dev4Agriculture.ISOXML.DDI;
using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;
using Dev4Agriculture.ISO11783.ISOXML.Utils;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISODevice
    {
        [XmlIgnore]
        public LocalizationLabel LocalizationLabelParsed { get; private set; }


        private ClientName _clientName = null;

        [XmlIgnore]
        public ClientName ClientNameParsed
        {
            get
            {
                if (_clientName != null)
                {
                    return _clientName;
                }
                else
                {
                    try
                    {
                        _clientName = new ClientName(ClientNAME);
                        return _clientName;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            private set => _clientName = value;
        }

        /// <summary>
        /// Convert all encoded data like ClientName & LocalizationLabel to readable structures
        /// </summary>
        /// <returns>A list of messages (Errors or Warnings) that showed up during the operation</returns>
        public ResultMessageList Analyse()
        {
            var resultMessageList = new ResultMessageList();
            try
            {
                LocalizationLabelParsed = new LocalizationLabel(DeviceLocalizationLabel);
            }
            catch (IndexOutOfRangeException indexException)
            {
                resultMessageList.AddError(ResultMessageCode.LocalizationLabelTooShort,
                    ResultDetail.FromId(DeviceId),
                    ResultDetail.FromString(HexUtils.ByteArrayToHexString(DeviceLocalizationLabel ?? new byte[0])),
                    ResultDetail.FromString(indexException.Message));
            }
            catch (LocalizationLabelInvalidException invalidLLException)
            {
                resultMessageList.AddError(ResultMessageCode.LocalizationLabelTooShort,
                    ResultDetail.FromId(DeviceId),
                    ResultDetail.FromString(HexUtils.ByteArrayToHexString(DeviceLocalizationLabel ?? new byte[0])),
                    ResultDetail.FromString(invalidLLException.Message));

            }
            catch (Exception ex)
            {
                resultMessageList.AddError(ResultMessageCode.LocalizationLabelBroken,
                    ResultDetail.FromId(DeviceId),
                    ResultDetail.FromString(HexUtils.ByteArrayToHexString(DeviceLocalizationLabel ?? new byte[0])),
                    ResultDetail.FromString(ex.Message));

            }

            try
            {
                ClientNameParsed = new ClientName(ClientNAME);
                resultMessageList.AddRange(ClientNameParsed.Validate());
            }
            catch (IndexOutOfRangeException indexException)
            {
                resultMessageList.AddError(ResultMessageCode.ClientNameTooShort,
                    ResultDetail.FromId(DeviceId),
                    ResultDetail.FromString(HexUtils.ByteArrayToHexString(ClientNAME ?? new byte[0])),
                    ResultDetail.FromString(indexException.Message));
            }
            catch (Exception ex)
            {
                resultMessageList.AddError(ResultMessageCode.ClientNameBroken,
                    ResultDetail.FromId(DeviceId),
                    ResultDetail.FromString(HexUtils.ByteArrayToHexString(ClientNAME ?? new byte[0])),
                    ResultDetail.FromString(ex.Message));

            }

            return resultMessageList;

        }


        /// <summary>
        /// Returns a list of all Combinations of DeviceProcessData + DeviceElement that reflects a Total.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(ISODeviceElement, ISODeviceProcessData)> GetAllTotalsProcessData()
        {
            var result = new List<(ISODeviceElement, ISODeviceProcessData)>();

            foreach (var (det, dpd) in from det in DeviceElement.ToList()
                                       from dor in det.DeviceObjectReference
                                       from dpd in DeviceProcessData
                                       where dpd.DeviceProcessDataObjectId == dor.DeviceObjectId
                                       where dpd.IsTotal() &&
                                             (DDIUtils.ConvertDDI(dpd.DeviceProcessDataDDI) != (ushort)DDIList.RequestDefaultProcessData)
                                       select (det, dpd))
            {
                result.Add((det, dpd));
            }

            return result;
        }

        /// <summary>
        /// Check if the devicedescription includes a specific DDI as Total (Total & setable)
        /// </summary>
        /// <param name="ddi"></param>
        /// <returns></returns>
        public bool IsTotal(ushort ddi)
        {
            return DeviceProcessData.FirstOrDefault(entry => DDIUtils.ConvertDDI(entry.DeviceProcessDataDDI) == ddi)?.IsTotal() ?? false;
        }

        /// <summary>
        /// Check if the devicedescription includes a specific DDI as LifeTime (Total & not setable)
        /// </summary>
        /// <param name="ddi"></param>
        /// <returns></returns>
        public bool IsLifetimeTotal(ushort ddi)
        {
            return DeviceProcessData.FirstOrDefault(entry => DDIUtils.ConvertDDI(entry.DeviceProcessDataDDI) == ddi)?.IsLifeTimeTotal() ?? false;
        }

        /// <summary>
        /// Check if a specific DDI is avaliable as DeviceProcessData within a Device
        /// </summary>
        /// <param name="ddi"></param>
        /// <returns>true if DDI is found</returns>
        public bool IsDeviceProcessData(ushort ddi)
        {
            return DeviceProcessData.Any(dpd => dpd.DeviceProcessDataDDI == DDIUtils.FormatDDI(ddi));
        }

        /// <summary>
        /// Check if a specific DDI is avaliable as DeviceProperty within a device
        /// </summary>
        /// <param name="ddi"></param>
        /// <returns>true if DDI is found</returns>
        public bool IsDeviceProperty(ushort ddi)
        {
            return DeviceProperty.Any(dpd => dpd.DevicePropertyDDI == DDIUtils.FormatDDI(ddi));
        }


    }
}
