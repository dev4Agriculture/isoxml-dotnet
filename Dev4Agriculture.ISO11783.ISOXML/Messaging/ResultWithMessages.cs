using System.Collections.Generic;

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
    }
}
