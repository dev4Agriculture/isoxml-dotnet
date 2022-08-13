using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4ag
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
