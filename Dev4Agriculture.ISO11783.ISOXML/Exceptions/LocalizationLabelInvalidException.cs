using System;
using System.Runtime.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.Exceptions
{
    [Serializable]
    public class LocalizationLabelInvalidException : Exception
    {
        public LocalizationLabelInvalidException()
        {
        }

        public LocalizationLabelInvalidException(string message) : base(message)
        {
        }

        public LocalizationLabelInvalidException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LocalizationLabelInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
