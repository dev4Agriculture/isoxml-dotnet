namespace Dev4Agriculture.ISO11783.ISOXML
{
    public enum ResultMessageType
    {
        Error,
        Warning
    }
    public class ResultMessage {

        public ResultMessage(ResultMessageType type, string title) {
            this.type = type;
            this.title = title;
        }
        public ResultMessageType type;
        public string title;
    }
}