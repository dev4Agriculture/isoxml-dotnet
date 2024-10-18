using System;
using System.Runtime.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.Exceptions
{
    [Serializable]
    internal class NoTaskInWorkSessionException : Exception
    {
        public NoTaskInWorkSessionException()
        {
        }

        public NoTaskInWorkSessionException(string message) : base(message)
        {
        }

        public NoTaskInWorkSessionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoTaskInWorkSessionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}