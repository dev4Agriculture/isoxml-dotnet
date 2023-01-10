using System;
using System.Xml.Serialization;
using Dev4Agriculture.ISO11783.ISOXML.Exceptions;
using Dev4Agriculture.ISO11783.ISOXML.Messaging;

namespace Dev4Agriculture.ISO11783.ISOXML.TaskFile
{
    public partial class ISODevice
    {
        [XmlIgnore]
        public LocalizationLabel LocalizationLabelParsed { get; private set; }

        [XmlIgnore]
        public ClientName ClientNameParsed { get; private set; }


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
                    ResultDetail.FromString(Utils.ByteArrayToHexString(DeviceLocalizationLabel ?? new byte[0])),
                    ResultDetail.FromString(indexException.Message));
            }
            catch (LocalizationLabelInvalidException invalidLLException)
            {
                resultMessageList.AddError(ResultMessageCode.LocalizationLabelTooShort,
                    ResultDetail.FromId(DeviceId),
                    ResultDetail.FromString(Utils.ByteArrayToHexString(DeviceLocalizationLabel ?? new byte[0])),
                    ResultDetail.FromString(invalidLLException.Message));

            }
            catch (Exception ex)
            {
                resultMessageList.AddError(ResultMessageCode.LocalizationLabelBroken,
                    ResultDetail.FromId(DeviceId),
                    ResultDetail.FromString(Utils.ByteArrayToHexString(DeviceLocalizationLabel ?? new byte[0])),
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
                    ResultDetail.FromString(Utils.ByteArrayToHexString(ClientNAME ?? new byte[0])),
                    ResultDetail.FromString(indexException.Message));
            }
            catch (Exception ex)
            {
                resultMessageList.AddError(ResultMessageCode.ClientNameBroken,
                    ResultDetail.FromId(DeviceId),
                    ResultDetail.FromString(Utils.ByteArrayToHexString(ClientNAME ?? new byte[0])),
                    ResultDetail.FromString(ex.Message));

            }

            return resultMessageList;

        }

    }
}
