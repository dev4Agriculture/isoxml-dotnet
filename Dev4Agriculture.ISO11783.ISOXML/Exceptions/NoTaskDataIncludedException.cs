using System;
using System.Runtime.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.Exceptions
{
    [Serializable]
    public class NoTaskDataIncludedException : Exception
    {
        public NoTaskDataIncludedException()
        {
        }

        public NoTaskDataIncludedException(string message) : base(message)
        {
        }

        public NoTaskDataIncludedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoTaskDataIncludedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
