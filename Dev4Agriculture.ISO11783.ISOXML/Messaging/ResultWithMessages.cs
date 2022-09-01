using System.Collections.Generic;
using System.Linq;

namespace Dev4Agriculture.ISO11783.ISOXML.Messaging
{
    public class ResultWithMessages<ResultType> where ResultType : class
    {
        public ResultType Result = null;
        public List<ResultMessage> Messages = new List<ResultMessage>();

        public ResultWithMessages(ResultType result, List<ResultMessage> messages)
        {
            Result = result;
            Messages = messages;
        }
        public ResultWithMessages(ResultType result)
        {
            Result = result;
        }


        public int CountErrors()
        {
            return Messages.Count(entry => entry.Type == ResultMessageType.Error);
        }

        public bool HasErrors()
        {
            return CountErrors() > 0;
        }


        public int CountWarnings()
        {
            return Messages.Count(entry => entry.Type == ResultMessageType.Warning);
        }

        public bool HasWarnings()
        {
            return CountWarnings() > 0;
        }
    }
}
