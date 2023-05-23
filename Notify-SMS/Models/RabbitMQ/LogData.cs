using Microsoft.Extensions.Logging;

namespace Models.RabbitMQ
{
    public class LogData
    {
        public string TransactionID { get; set; }
        public string IntegrationName { get; set; }
        public string DomainName { get; set; }
        public string Operation { get; set; }
        public string Type { get; set; }
        public DateTime TimeStamp { get; set; }
        public EventType Event { get; set; }
        public string Status { get; set; }
        public string Trace { get; set; }
        public string MessageResult { get; set; }
    }

    public class EventType
    {
        public HeaderType Header { get; set; }
        public object Data { get; set; }
    }

    public class HeaderType
    {
        public string TransactionID { get; set; }
        public string ApplicationID { get; set; }
        public DateTime TransactionDate { get; set; }
        public object FlexField { get; set; }
    }

}
