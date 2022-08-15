namespace Dev4Agriculture.ISO11783.ISOXML.Messaging
{
    public enum ResultMessageType
    {
        Error,
        Warning
    }
    public class ResultMessage
    {

        public ResultMessage(ResultMessageType type, string title)
        {
            Type = type;
            Title = title;
        }
        public ResultMessageType Type;
        public string Title;
    }
}
