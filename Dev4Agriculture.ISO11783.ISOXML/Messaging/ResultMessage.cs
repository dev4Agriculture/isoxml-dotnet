namespace Dev4Agriculture.ISO11783.ISOXML.Messaging
{

    public enum ResultMessageType
    {
        Error,
        Warning,
        Info
    }

    public class ResultMessage
    {
        public ResultMessageType Type;
        public ResultMessageCode Code;
        public string Title => Code.ToString();
        private string FillDetails(string message)
        {
            if (Details.Length == 0)
            {
                return message;
            }
            var lastIndex = 0;
            for (var index = 1; index <= Details.Length; index++)
            {
                if (message.IndexOf($"%{index}") > -1)
                {
                    message = message.Replace($"%{index}", Details[index - 1].Value);
                }
                else
                {
                    break;
                }
                lastIndex = index;
            }
            if (lastIndex < Details.Length)
            {
                message += "More Details: [";
                for (var index = lastIndex; index <= Details.Length; index++)
                {
                    message += Details[index - 1].Value;
                    if (index < Details.Length)
                    {
                        message += ", ";
                    }
                    message += "]";
                }
            }
            return message;
        }
        public string Description
        {
            get => FillDetails(
                    Code == ResultMessageCode.Unknown ? "Unknown, Details" :
                    Code == ResultMessageCode.FileInvalid ? "File Invalid: %1, Exception: %2" :
                    Code == ResultMessageCode.MultipleTaskDataFound ? "Multiple TaskData files found; only the first one is loaded" :
                    Code == ResultMessageCode.FileSizeMissmatch ? "FileSize of %1 doesn't match; Is %2 but should be %3 Bytes" :
                    Code == ResultMessageCode.FileNotFound ? "File could not be found: %1; Message: %2" :
                    Code == ResultMessageCode.FileAccessImpossible ? "File could not be accessed: %1; Message: %2" :
                    Code == ResultMessageCode.XMLParsingError ? "XML Parsing Error in %1, Details: %2" :
                    Code == ResultMessageCode.XMLWrongElement ? "Wrong XML Element Type found in %1" :
                    Code == ResultMessageCode.XMLTextIn ? "Found Text Element in XML, this is invalid in %1" :
                    Code == ResultMessageCode.XMLInvalidElement ? "Found invalid Element %1" :
                    Code == ResultMessageCode.XMLCommentFound ? "Found Comment Element %1" :
                    Code == ResultMessageCode.XMLOtherFound ? "Found other Type of Element in XML of %1" :
                    Code == ResultMessageCode.XSDAttributeValueRange ? "Value %1 outside Range Min %2 and %3, NodeId: %4, Object: %5" :
                    Code == ResultMessageCode.XSDAttributeValueTooLong ? "Value %1 too long. Max Length: %2, Element: %3, NodeId: %4" :
                    Code == ResultMessageCode.XSDAttributeValueTooShort ? "Value %1 too short. Min Length: %2, Element: %3, NodeId: %4" :
                    Code == ResultMessageCode.XSDAttributeRegExMissmatch ? "Value %1 does not match Regular Expression %2.Element: %3, NodeId: %4" :
                    Code == ResultMessageCode.XSDAttributeRequired ? "Required Attribute %1 missing in Element %2" :
                    Code == ResultMessageCode.XSDAttributeParsing ? "Parsing Error in Value %1, Exception Type: %2, Exception Text: %3, PropertyName: %4, NodeId: %5" :
                    Code == ResultMessageCode.XSDAttributeUnknown ? "Unknown Attribute %1 found in Element %2" :
                    Code == ResultMessageCode.XSDAttributeProprietary ? "Proprietary Attribute %1 found in Element %2" :
                    Code == ResultMessageCode.XSDAttributeProprietaryInvalid ? "Invalid Proprietary Attribute %1 found in Element %2" :
                    Code == ResultMessageCode.XSDElementUnknown ? "Unknown Element %1 found" :
                    Code == ResultMessageCode.XSDElementInvalid ? "Found invalid Element in XML. Type: %1, Content: %2" :
                    Code == ResultMessageCode.XSDElementProprietary ? "Proprietary Element %1 found" :
                    Code == ResultMessageCode.XSDElementProprietaryInvalid ? "Invalid Proprietary Element %1 found" :
                    Code == ResultMessageCode.XSDElementWrongChild ? "Wrong Child Element %1 found in %2, NodeId: %3" :
                    Code == ResultMessageCode.XSDEnumUnknown ? "Value %1 unknown in the corresponding Attribute %2, ID: %3" :
                    Code == ResultMessageCode.BINInvalidData ? "Invalid Data found in Binary File %1; Position %2 of %3; Cause: %4" :
                    Code == ResultMessageCode.BINInvalidNumberOfDataInRow ? "There were more binary data linked in the binary file than exist in the header. File: %1, Position: %2, Solution: %3" :
                    Code == ResultMessageCode.GRIDFileSizeMissmatch ? "FileSize of Grid doesn't match: %1" :
                    Code == ResultMessageCode.MissingId ? "There is no ID defined in %1. Assigning: %2" :
                    Code == ResultMessageCode.DuplicatedTLG ? "A TLG Entry with the same ID was found twice. The second Entry was ignored! TLG Name: %1"
                    Code == ResultMessageCode.LinkListWrongRootElement ? "Wrong root element in LinkList.XML" :
                    Code == ResultMessageCode.LocalizationLabelBroken ? "LocalizationLabel broken in Device %1 : %2; Message: %3" :
                    Code == ResultMessageCode.LocalizationLabelTooShort ? "LocalizationLabel too Short in Device %1: %2; Message: %3" :
                    Code == ResultMessageCode.LocalizationLabelWrongReservedValue ? "Byte 7 of the localisation label shall always be 0xFF but is %1" :
                    Code == ResultMessageCode.ClientNameTooShort ? "Client Name invalid in Device %1: %2; Message: %3" :
                    Code == ResultMessageCode.ClientNameBroken ? "Client Name broken in Device %1: %2; Message: %3" :
                    Code == ResultMessageCode.ClientNameDeviceClassInvalid ? "Invalid DeviceClass %1 in ClientName: %2" :
                    "Other");
            private set
            {

            }
        }
        public ResultDetail[] Details;

        public ResultMessage(ResultMessageType type, ResultMessageCode code, params ResultDetail[] details)
        {
            Details = details ?? new ResultDetail[0];
            Type = type;
            Code = code;
        }

        public static ResultMessage Error(ResultMessageCode code, params ResultDetail[] details)
        {
            return new ResultMessage(ResultMessageType.Error, code, details);
        }

        public static ResultMessage Warning(ResultMessageCode code, params ResultDetail[] details)
        {
            return new ResultMessage(ResultMessageType.Warning, code, details);
        }
        public static ResultMessage Info(ResultMessageCode code, params ResultDetail[] details)
        {
            return new ResultMessage(ResultMessageType.Info, code, details);
        }

    }
}
