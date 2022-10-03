using System;
using System.Runtime.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.Exceptions
{
    [Serializable]
    internal class CouldNotStoreTLGException : Exception
    {
        public CouldNotStoreTLGException()
        {
        }

        public CouldNotStoreTLGException(string message) : base(message)
        {
        }

        public CouldNotStoreTLGException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CouldNotStoreTLGException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}