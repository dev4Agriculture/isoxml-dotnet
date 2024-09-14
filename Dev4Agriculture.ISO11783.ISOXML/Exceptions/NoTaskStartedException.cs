using System;
using System.Runtime.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.Exceptions
{
    [Serializable]
    internal class NoTaskStartedException : Exception
    {
        public NoTaskStartedException()
        {
        }

        public NoTaskStartedException(string message) : base(message)
        {
        }

        public NoTaskStartedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoTaskStartedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}