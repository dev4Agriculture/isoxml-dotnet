using System;
using System.Runtime.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.Exceptions
{
    [Serializable]
    internal class DuplicatedISOObjectException : Exception
    {
        public DuplicatedISOObjectException()
        {
        }

        public DuplicatedISOObjectException(string message) : base(message)
        {
        }

        public DuplicatedISOObjectException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DuplicatedISOObjectException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}