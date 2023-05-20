using System;
using System.Runtime.Serialization;

namespace Dev4Agriculture.ISO11783.ISOXML.Exceptions
{
    [Serializable]
    public class InvalidZipFolderException : Exception
    {
        public InvalidZipFolderException()
        {
        }

        public InvalidZipFolderException(string message) : base(message)
        {
        }

        public InvalidZipFolderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidZipFolderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
