using System;
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

        [XmlIgnore]
        public ClientName ClientNameParsed { get; private set; }


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

        public bool IsTotal(ushort DDI)
        {
            return DeviceProcessData.FirstOrDefault(entry => DDIUtils.ConvertDDI(entry.DeviceProcessDataDDI) == DDI)?.IsTotal() ?? false;
        }

        public bool IsLifetimeTotal(ushort DDI)
        {
            return DeviceProcessData.FirstOrDefault(entry => DDIUtils.ConvertDDI(entry.DeviceProcessDataDDI) == DDI)?.IsLifeTimeTotal() ?? false;
        }
    }
}
