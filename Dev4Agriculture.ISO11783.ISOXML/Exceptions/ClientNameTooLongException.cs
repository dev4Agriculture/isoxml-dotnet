using System;
using System.Runtime.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.Exceptions
{
    [Serializable]
    public class ClientNameTooLongException : Exception
    {
        public ClientNameTooLongException()
        {
        }

        public ClientNameTooLongException(string message) : base(message)
        {
        }

        public ClientNameTooLongException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ClientNameTooLongException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
