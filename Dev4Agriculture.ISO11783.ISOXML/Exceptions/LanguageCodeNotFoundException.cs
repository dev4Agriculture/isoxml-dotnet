using System;
using System.Runtime.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.Exceptions
{
    [Serializable]
    public class LanguageCodeNotFoundException : Exception
    {
        public LanguageCodeNotFoundException()
        {
        }

        public LanguageCodeNotFoundException(string message) : base(message)
        {
        }

        public LanguageCodeNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LanguageCodeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
