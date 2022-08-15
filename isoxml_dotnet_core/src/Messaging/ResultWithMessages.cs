using System.Collections.Generic;

namespace Dev4Agriculture.ISO11783.ISOXML
{
    public class ResultWithMessages<ResultType> where ResultType : class
    {
        public ResultType result = null;
        public List<ResultMessage> messages = new List<ResultMessage>();

        public ResultWithMessages(ResultType result, List<ResultMessage> messages)
        {
            this.result = result;
            this.messages = messages;
        }
        public ResultWithMessages(ResultType result)
        {
            this.result = result;
        }
    }
}
