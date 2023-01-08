using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4Agriculture.ISO11783.ISOXML.Messaging
{
    public enum ResultMessageCode
    {
        Unknown = 0,
        MultipleTaskDataFound,
        MissingId,
        FileSizeMissmatch,
        FileNotFound,
        FileAccessImpossible,
        FileInvalid,
        XMLParsingError = 100,
        XMLWrongElement,
        XMLTextIn,
        XMLInvalidElement,
        XMLCommentFound,
        XMLOtherFound,
        XSDAttributeValueRange = 200,
        XSDAttributeValueTooLong,
        XSDAttributeValueTooShort,
        XSDAttributeRegExMissmatch,
        XSDAttributeRequired,
        XSDAttributeParsing,
        XSDAttributeUnknown,
        XSDAttributeProprietary,
        XSDAttributeProprietaryInvalid,
        XSDElementUnknown = 300,
        XSDElementInvalid,
        XSDElementProprietary,
        XSDElementProprietaryInvalid,
        XSDElementWrongChild,
        XSDEnumUnknown = 400,
        BINInvalidData = 500,
        BINInvalidNumberOfDataInRow,
        GRIDFileSizeMissmatch = 600,
        LinkListWrongRootElement = 700
    }

    public enum MessageDetailType
    {
        MDTString,
        MDTFile,
        MDTPath,
        MDTNumber,
        MDTDouble
    }

    public struct ResultDetail
    {
        public MessageDetailType MessageDetailType;
        public string Value;
        private ResultDetail(MessageDetailType messageDetailType, string value)
        {
            MessageDetailType = messageDetailType;
            Value = value;
        }

        public static ResultDetail FromString(string detail)
        {
            return new ResultDetail(MessageDetailType.MDTString, detail);
        }


        public static ResultDetail FromFile(string detail)
        {
            return new ResultDetail(MessageDetailType.MDTFile, detail);
        }

        public static ResultDetail FromPath(string detail)
        {
            return new ResultDetail(MessageDetailType.MDTPath, detail);
        }

        public static ResultDetail FromNumber(long number)
        {
            return new ResultDetail(MessageDetailType.MDTString, number.ToString());
        }

        public static ResultDetail FromFloat(double number)
        {
            return new ResultDetail(MessageDetailType.MDTString, number.ToString());
        }

    }


}
