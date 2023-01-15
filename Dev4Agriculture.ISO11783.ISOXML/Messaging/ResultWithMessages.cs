using System.Collections.Generic;
using System.Linq;

namespace Dev4Agriculture.ISO11783.ISOXML.Messaging
{
    public class ResultMessageList : List<ResultMessage>
    {
        public void AddError(ResultMessageCode code, params ResultDetail[] details)
        {
            Add(ResultMessage.Error(code, details));
        }

        public void AddWarning(ResultMessageCode code, params ResultDetail[] details)
        {
            Add(ResultMessage.Warning(code, details));
        }
        public void AddInfo(ResultMessageCode code, params ResultDetail[] details)
        {
            Add(ResultMessage.Info(code, details));
        }
    }

    public class ResultWithMessages<ResultType> where ResultType : class
    {
        public ResultType Result = null;
        public ResultMessageList Messages = new ResultMessageList();

        public ResultWithMessages(ResultType result, List<ResultMessage> messages)
        {
            Result = result;
            Messages = (ResultMessageList)messages;
        }
        public ResultWithMessages(ResultType result)
        {
            Result = result;
        }
        public ResultWithMessages(ResultType result, ResultMessage onlyOneMessage)
        {
            Result = result;
            Messages = new ResultMessageList()
            {
                onlyOneMessage
            };
        }

        public ResultWithMessages()
        {
            Messages = new ResultMessageList();
        }


        /// <summary>
        ///    Set the Result Content
        /// </summary>
        /// <param name="result"></param>
        public void SetResult(ResultType result)
        {
            Result = result;
        }


        public void AddMessages(List<ResultMessage> messages)
        {
            Messages.AddRange(messages);
        }

        /// <summary>
        /// Add an InfoMessage to the result
        /// </summary>
        /// <param name="code"></param>
        /// <param name="details"></param>
        public void AddInfo(ResultMessageCode code, params ResultDetail[] details)
        {
            Messages.Add(
                new ResultMessage(
                    ResultMessageType.Info,
                    code,
                    details ?? new ResultDetail[0]
                    )
                );
        }


        /// <summary>
        /// Add a WarningMessage to the result
        /// </summary>
        /// <param name="code"></param>
        /// <param name="details"></param>
        public void AddWarning(ResultMessageCode code, params ResultDetail[] details)
        {
            Messages.Add(
                new ResultMessage(
                    ResultMessageType.Warning,
                    code,
                    details ?? new ResultDetail[0]
                    )
                );
        }


        /// <summary>
        /// Add Error to the result
        /// </summary>
        /// <param name="code"></param>
        /// <param name="details"></param>
        public void AddError(ResultMessageCode code, params ResultDetail[] details)
        {
            Messages.Add(
                new ResultMessage(
                    ResultMessageType.Error,
                    code,
                    details ?? new ResultDetail[0]
                    )
                );
        }


        public int CountInfos()
        {
            return Messages.Count(entry => entry.Type == ResultMessageType.Info);
        }

        public bool HasInfos()
        {
            return CountInfos() > 0;
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
