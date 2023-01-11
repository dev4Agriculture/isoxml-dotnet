using System;
using System.Runtime.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.Exceptions
{
    [Serializable]
    public class InvalidLanguageCodeException : Exception
    {
        public InvalidLanguageCodeException()
        {
        }

        public InvalidLanguageCodeException(string message) : base(message)
        {
        }

        public InvalidLanguageCodeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidLanguageCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
