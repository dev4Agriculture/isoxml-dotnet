using System;
using System.Runtime.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.Exceptions
{
    [Serializable]
    public class ClientNameTooShortException : Exception
    {
        public ClientNameTooShortException()
        {
        }

        public ClientNameTooShortException(string message) : base(message)
        {
        }

        public ClientNameTooShortException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ClientNameTooShortException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
