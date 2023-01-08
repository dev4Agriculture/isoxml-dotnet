using System.IO;

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
            for (var index = 1; index < Details.Length; index++)
            {
                message = message.Replace($"%{index}", Details[index - 1].Value);
            }
            return message;
        }
        public string Description
        {
            get => FillDetails(
                    Code == ResultMessageCode.Unknown ? "Unknown" :
                    Code == ResultMessageCode.FileInvalid ? "File Invalid: %1" :
                    Code == ResultMessageCode.MultipleTaskDataFound ? "Multiple TaskData files found; only the first one is loaded" :
                    Code == ResultMessageCode.FileSizeMissmatch ? "FileSize of %1 doesn't match; Is %2 but should be %3 Bytes" :
                    Code == ResultMessageCode.FileNotFound ? "File could not be found: %1; Message: %2" :
                    Code == ResultMessageCode.FileAccessImpossible ? "File could not be accessed: %1; Message: %2" :
                    Code == ResultMessageCode.XMLParsingError ? "XML Parsing Error in %1" :
                    Code == ResultMessageCode.XMLWrongElement ? "Wrong XML Element Type found in %1" :
                    Code == ResultMessageCode.XMLTextIn ? "Found Text Element in XML, this is invalid in %1" :
                    Code == ResultMessageCode.XMLInvalidElement ? "Found invalid Element %1" :
                    Code == ResultMessageCode.XMLCommentFound ? "Found Comment Element %1" :
                    Code == ResultMessageCode.XMLOtherFound ? "Found other Type of Element in XML of %1" :
                    Code == ResultMessageCode.XSDAttributeValueRange ? "Value %1 outside Range Min %2 and %3" :
                    Code == ResultMessageCode.XSDAttributeValueTooLong ? "Value %1 too long inside %2" :
                    Code == ResultMessageCode.XSDAttributeValueTooShort ? "Value %1 too short inside %2" :
                    Code == ResultMessageCode.XSDAttributeRegExMissmatch ? "Value %1 does not match Regular Expression %2 in %3" :
                    Code == ResultMessageCode.XSDAttributeRequired ? "Required Attribute %1 missing in Element %2" :
                    Code == ResultMessageCode.XSDAttributeParsing ? "Parsing Error during XSD Schema validation" :
                    Code == ResultMessageCode.XSDAttributeUnknown ? "Unknown Attribute %1 found in Element %2" :
                    Code == ResultMessageCode.XSDAttributeProprietary ? "Proprietary Attribute %1 found in Element %2" :
                    Code == ResultMessageCode.XSDAttributeProprietaryInvalid ? "Invalid Proprietary Attribute %1 found in Element %2" :
                    Code == ResultMessageCode.XSDElementUnknown ? "Unknown Element %1 found" :
                    Code == ResultMessageCode.XSDElementInvalid ? "Found invalid Element in XML. Type: %1, Content: %2" :
                    Code == ResultMessageCode.XSDElementProprietary ? "Proprietary Element %1 found" :
                    Code == ResultMessageCode.XSDElementProprietaryInvalid ? "Invalid Proprietary Element %1 found" :
                    Code == ResultMessageCode.XSDElementWrongChild ? "Wrong Child Element %1 found in %2" :
                    Code == ResultMessageCode.XSDEnumUnknown ? "Value %1 unknown in the corresponding Attribute" :
                    Code == ResultMessageCode.BINInvalidData ? "Invalid Data found in Binary File %1; Position %2 of %3; Cause: %4" :
                    Code == ResultMessageCode.BINInvalidNumberOfDataInRow ? "There were more binary data linked in the binary file than exist in the header. File: %1, Position: %2, Solution: %3" :
                    Code == ResultMessageCode.GRIDFileSizeMissmatch ? "FileSize of Grid doesn't match: %1" :
                    Code == ResultMessageCode.MissingId ? "There is no ID defined in %1" :
                    Code == ResultMessageCode.LinkListWrongRootElement ? "Wrong root element in LinkList.XML":
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
